using EventBus.Infrastructure;
using System.Collections.Generic;

namespace EventBus.Hosting
{
	public class MemoryPublisher<T> : IPublisher<T>
	{
		public MemoryPublisher()
		{
			DefaultSingleton<Queue<T>>.Instance = DefaultSingleton<Queue<T>>.Instance ?? new Queue<T>();
		}

		public virtual void Publish(T data)
		{
			DefaultSingleton<Queue<T>>.Instance.Enqueue(data);
		}
	}
}