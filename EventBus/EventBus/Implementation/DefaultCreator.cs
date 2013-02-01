using System;
using System.Collections.Concurrent;

namespace EventBus.Implementation
{
	public class DefaultCreator : Infrastructure.ICreator
	{
		private readonly ConcurrentDictionary<Type, object> _container;

		public DefaultCreator()
		{
			_container = new ConcurrentDictionary<Type, object>();
		}

		public T Create<T>()
		{
			return (T)Create(typeof(T));
		}

		public object Create(Type t)
		{
			object impl = null;
			if (_container.TryGetValue(t, out impl))
			{
				return impl;
			}
			else
			{
				object data = Activator.CreateInstance(t);
				if (_container.TryAdd(t, data))
				{
					return data;
				}
				else
				{
					throw new InvalidOperationException("Fail to perform operation");
				}
			}
		}

		public void Drain()
		{
			if (_container.Count <= 0)
				return;

			_container.Clear();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
		}
	}
}