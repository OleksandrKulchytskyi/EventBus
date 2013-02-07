using System;
using System.Collections.Concurrent;

namespace EventBus.Implementation
{
	public class DefaultCreator : Infrastructure.ICreator
	{
		private readonly ConcurrentDictionary<Type, object> _container;
		private readonly ConcurrentDictionary<Type, Type> _binding;

		public DefaultCreator()
		{
			_container = new ConcurrentDictionary<Type, object>();
			_binding = new ConcurrentDictionary<Type, Type>();
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
				object data = null;
				if (t.IsInterface)
				{
					Type result = Resolve(t);
					if (result == default(Type))
						throw new InvalidOperationException(string.Format("Fail to find appropriate implementation of type {0} \n\r Creation failed.", t.FullName));

					data = Activator.CreateInstance(result);
					if (_container.TryAdd(t, data))
						return data;

					else
						throw new InvalidOperationException("Fail to perform operation");
				}

				data = Activator.CreateInstance(t);
				if (_container.TryAdd(t, data))
					return data;

				else
					throw new InvalidOperationException("Fail to perform operation");
			}
		}

		private Type Resolve(Type t)
		{
			Type result;
			if (_binding.TryGetValue(t, out result))
				return result;
			return default(Type);
		}

		public void Clear()
		{
			if (_container.Count > 0)
			{
				_container.Clear();
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();
			}

			if (_binding.Count > 0)
				_binding.Clear();

		}
	}
}