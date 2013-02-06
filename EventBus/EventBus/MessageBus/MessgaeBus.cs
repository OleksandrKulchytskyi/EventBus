using EventBus.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EventBus.MessageBus
{
	public class MessgaeBus : SingletonBase<MessgaeBus>, IMessageBus
	{
		private readonly Lazy<ConcurrentDictionary<Type, HashSet<object>>> _lazyContainer = new Lazy<ConcurrentDictionary<Type, HashSet<object>>>(
				() => new ConcurrentDictionary<Type, HashSet<object>>());

		private int _draining = 0;
		private int _groupCount;

		private MessgaeBus()
		{
			_groupCount = 0;
		}

		public void Subscribe<TEvent>(Infrastructure.ISubscriber<TEvent> subscriber)
		{
			if (_draining == 1)
				return;
			Type key = typeof(TEvent);
			if (_lazyContainer.Value.ContainsKey(key))
			{
				if (_lazyContainer.Value[key].Contains(subscriber))
				{
					throw new InvalidOperationException("Type has been already subscribed.");
				}
				else
					_lazyContainer.Value[key].Add(subscriber);
			}
			else
			{
				if (_lazyContainer.Value.TryAdd(key, new HashSet<object>()))
				{
					Interlocked.Increment(ref _groupCount);
					_lazyContainer.Value[key].Add(subscriber);
				}
			}
		}

		public void Unsubscribe<TEvent>(Infrastructure.ISubscriber<TEvent> subscriber)
		{
			if (_draining == 1)
				return;
			Type key = typeof(TEvent);
			if (_lazyContainer.Value.ContainsKey(key))
			{
				if (_lazyContainer.Value[key].Contains(subscriber))
				{
					_lazyContainer.Value[key].Remove(subscriber);
				}
			}
		}

		public void Publish<TEvent>(TEvent message)
		{
			if (_draining == 1)
				return;
			Type key = typeof(TEvent);
			if (_lazyContainer.Value.ContainsKey(key) && _lazyContainer.Value[key] != null)
			{
				foreach (var item in _lazyContainer.Value[key])
				{
					if (_draining == 1)
						break;

					if (item is INotify<TEvent>)
						(item as INotify<TEvent>).Notify(message);
					else
						(item as ISubscriber<TEvent>).HandleEvent(message);
				}
			}
		}

		public void Drain()
		{
			if (Interlocked.Exchange(ref _draining, 1) == 1)
				return;

			var keys = _lazyContainer.Value.Keys;
			foreach (var item in keys)
			{
				HashSet<object> container;
				if (_lazyContainer.Value.TryRemove(item, out container) && container != null)
				{
					Interlocked.Decrement(ref _groupCount);
					container.Clear();
					container = null;
				}
			}

			_lazyContainer.Value.Clear();
			GC.Collect();

			Interlocked.Exchange(ref _draining, 0);
		}

		public int GroupCount
		{
			get { return _groupCount; }
		}
	}
}
