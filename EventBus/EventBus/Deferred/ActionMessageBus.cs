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
	public class ActionMessageBus : IClearable
	{
		private readonly Lazy<ConcurrentDictionary<Type, HashSet<WeakAction>>> _lazyDict;
		private SpinLock _locker;
		private int _clearing = 0;

		public ActionMessageBus()
		{
			_lazyDict = new Lazy<ConcurrentDictionary<Type, HashSet<WeakAction>>>(() => new ConcurrentDictionary<Type, HashSet<WeakAction>>());
			_locker = new SpinLock();
		}

		public IDisposable Subscribe<T>(WeakAction<T> action) where T : class,IMessage
		{
			NotNull(action, "subscriber");

			Type key = typeof(T);
			var container = CheckForKey(key);

			_locker.SafeWork(() =>
			{
				if (container.Contains(action))
					throw new InvalidOperationException("This subscriber has been already subscribed to this event.");

				container.Add(action);
			});

			return new Disposer(() =>
			{
				HashSet<WeakAction> dump;

				Type[] args = action.GetType().GetGenericArguments();
				if (args != null && args.Length > 0)
				{
					if (!_lazyDict.Value.TryGetValue(args[0], out dump))
						return;

					_locker.SafeWork(() => dump.Remove(action));
					(action).MarkForDeletion();
				}
			});
		}

		public IDisposable Subscribe<T>(Action<T> action) where T : class,IMessage
		{
			return Subscribe<T>(new WeakAction<T>(action));
		}

		public void Publish<T>(T message) where T : class, IMessage
		{
			NotNull(message, "message");
			Type key = typeof(T);
			var container = CheckForKey(key);

			if (container.Count == 0) return;

			Parallel.ForEach(container, item =>
			{
				if (item != null && (item as WeakAction<T>) != null)
					(item as WeakAction<T>).ExecuteWithObject(message);
				else if (item != null)
					item.Execute();
			});
		}

		public void Clear()
		{
			if (Interlocked.Exchange(ref _clearing, 1) != 0)
				return;

			foreach (var key in _lazyDict.Value.Keys)
			{
				HashSet<WeakAction> subscribers;
				if (_lazyDict.Value.TryRemove(key, out subscribers))
				{
					_locker.SafeWork(() => subscribers.Clear());
					subscribers = null;
				}
			}
			_lazyDict.Value.Clear();
		}

		private HashSet<WeakAction> CheckForKey(Type key)
		{
			return _lazyDict.Value.GetOrAdd(key, new HashSet<WeakAction>());
		}

		private void NotNull<Tp>(Tp data, string pName)
		{
			if (data == null)
				throw new ArgumentNullException(pName);
		}
	}
}