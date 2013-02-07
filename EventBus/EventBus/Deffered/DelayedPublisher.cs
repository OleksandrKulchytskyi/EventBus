using EventBus.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.Deffered
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

			if (DefaultSingleton<IDsiposingMessageBus>.Instance == null)
			{
				DefaultSingleton<IDsiposingMessageBus>.Instance = DefaultSingleton<ICreator>.Instance.Create<DisposingMessageBus>();
			}
		}

		public void Publish(TEvent data)
		{
			DefaultSingleton<IDsiposingMessageBus>.Instance.Publish<TEvent>(data);
		}
	}
}
