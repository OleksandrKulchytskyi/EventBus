using EventBus.Extensions;
using EventBus.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EventBus.Deferred
{
	internal class DisposableMessageBus : IDisposableMessageBus
	{
		private readonly Lazy<ConcurrentDictionary<Type, HashSet<WeakReference>>> _lazyDict;
		private readonly Lazy<ConcurrentDictionary<Type, HashSet<IMessage>>> _lazyDefferMsgs;
		private Lazy<List<WeakReference>> _subsToRemove;
		private SpinLock _locker;

		private int _clearing = 0;

		public DisposableMessageBus()
		{
			_lazyDict = new Lazy<ConcurrentDictionary<Type, HashSet<WeakReference>>>(() => new ConcurrentDictionary<Type, HashSet<WeakReference>>());
			_lazyDefferMsgs = new Lazy<ConcurrentDictionary<Type, HashSet<IMessage>>>(() => new ConcurrentDictionary<Type, HashSet<IMessage>>());
			_subsToRemove = new Lazy<List<WeakReference>>(() => new List<WeakReference>());

			_locker = new SpinLock();
		}

		private HashSet<WeakReference> CheckForKey(Type key)
		{
			return _lazyDict.Value.GetOrAdd(key, new HashSet<WeakReference>());
		}

		private void NotNull<Tp>(Tp data, string pName)
		{
			if (data == null)
				throw new ArgumentNullException(pName);
		}

		public IDisposable Subscribe<T>(ISubscriber<T> subscriber) where T : class, IMessage
		{
			NotNull(subscriber, "subscriber");
			Type key = typeof(T);
			HashSet<WeakReference> container = CheckForKey(key);

			HashSet<IMessage> delayedMsgs;
			if (container.Count == 0 && _lazyDefferMsgs.Value.TryGetValue(key, out delayedMsgs) && delayedMsgs != null)
			{
				Parallel.ForEach(delayedMsgs, item =>
				{
					if ((subscriber as INotify<T>) != null)
						_locker.SafeWork(() => (subscriber as INotify<T>).Notify(item as T));
					else
						if ((subscriber as ISubscriber<T>) != null)
							_locker.SafeWork(() => (subscriber as ISubscriber<T>).HandleEvent(item as T));
				});
			}

			WeakReference weak = new WeakReference(subscriber);

			_locker.SafeWork(() =>
			{
				if (container.Contains(weak))
					throw new InvalidOperationException("This subscriber has been already subscribed to this event.");

				container.Add(weak);
			});

			return new Disposer(() =>
			{
				HashSet<WeakReference> dump;
				Type msgType = subscriber.GetType().GetInterfaces().Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ISubscriber<>))
					.Select(x => x.GetGenericArguments()[0]).FirstOrDefault();
				if (msgType == null || !_lazyDict.Value.TryGetValue(msgType, out dump))
					return;

				_locker.SafeWork(() => dump.Remove(weak));
			});
		}

		public void Publish<T>(T message) where T : class, IMessage
		{
			NotNull(message, "message");
			Type key = typeof(T);
			HashSet<WeakReference> container = null;

			if (!_lazyDict.Value.ContainsKey(key))
			{
				var holder = _lazyDefferMsgs.Value.GetOrAdd(key, new HashSet<IMessage>());
				_locker.SafeWork(() => holder.Add(message));
				return;
			}
			else
			{
				container = CheckForKey(key);

				if (container.Count == 0)
					return;

				Parallel.ForEach(container, item =>
				{
					if (item.IsAlive)
					{
						if ((item.Target as INotify<T>) != null)
							_locker.SafeWork(() => (item.Target as INotify<T>).Notify(message));
						else if ((item.Target as ISubscriber<T>) != null)
							_locker.SafeWork(() => (item.Target as ISubscriber<T>).HandleEvent(message));
					}
					else
					{
						_locker.SafeWork(() => _subsToRemove.Value.Add(item));
					}
				});

				if (container != null && _subsToRemove.Value.Any())
				{
					foreach (WeakReference weak in _subsToRemove.Value)
					{
						_locker.SafeWork(() => container.Remove(weak));
					}
					_locker.SafeWork(() => _subsToRemove.Value.Clear());
				}
			}
		}

		public void Clear()
		{
			if (Interlocked.Exchange(ref _clearing, 1) != 0)
				return;

			foreach (var key in _lazyDict.Value.Keys)
			{
				HashSet<WeakReference> subscribers;
				if (_lazyDict.Value.TryRemove(key, out subscribers))
				{
					_locker.SafeWork(() => subscribers.Clear());
					subscribers = null;
				}
			}
			_lazyDict.Value.Clear();

			foreach (var key in _lazyDefferMsgs.Value.Keys)
			{
				HashSet<IMessage> subscribers;
				if (_lazyDefferMsgs.Value.TryRemove(key, out subscribers))
				{
					_locker.SafeWork(() => subscribers.Clear());
					subscribers = null;
				}
			}
			_lazyDefferMsgs.Value.Clear();
		}
	}
}