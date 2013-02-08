using EventBus.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.Deferred
{
	public class DelayedPublisher<TEvent> : IPublisher<TEvent> where TEvent : class,IMessage
	{

		public DelayedPublisher()
		{
			if (Config.EventBusConfigSection.CheckConfig && !Config.EventBusConfigSection.IsCreatorTriggered)
			{
				Config.EventBusConfigSection.TriggerCreator();
				if (DefaultSingleton<ICreator>.Instance == null)
					DefaultSingleton<ICreator>.Instance = new Implementation.DefaultCreator();
			}
			else
			{
				DefaultSingleton<ICreator>.Instance = new Implementation.DefaultCreator();
			}

			if (DefaultSingleton<IDisposableMessageBus>.Instance == null)
			{
				DefaultSingleton<IDisposableMessageBus>.Instance = DefaultSingleton<ICreator>.Instance.Create<DisposableMessageBus>();
			}
		}

		public virtual void Publish(TEvent data)
		{
			DefaultSingleton<IDisposableMessageBus>.Instance.Publish<TEvent>(data);
		}
	}
}
