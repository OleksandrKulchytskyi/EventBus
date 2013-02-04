using EventBus.Infrastructure;
using System.Collections.Generic;

namespace EventBus.Hosting
{
	public class MemoryPublisher<TEvnt> : IPublisher<TEvnt>
	{
		private readonly int _Limit = 100;

		public MemoryPublisher()
		{
			DefaultSingleton<Queue<TEvnt>>.Instance = DefaultSingleton<Queue<TEvnt>>.Instance ?? new Queue<TEvnt>();
		}

		public virtual void Publish(TEvnt data)
		{
			if (DefaultSingleton<Queue<TEvnt>>.Instance.Count > _Limit)
				DefaultSingleton<Queue<TEvnt>>.Instance.Dequeue();

			DefaultSingleton<Queue<TEvnt>>.Instance.Enqueue(data);
		}
	}
}