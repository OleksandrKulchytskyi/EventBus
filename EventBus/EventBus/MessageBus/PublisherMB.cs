using EventBus.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.MessageBus
{
	public class PublisherMB<TEvent> : IPublisher<TEvent>
	{
		public virtual void Publish(TEvent data)
		{
			MessageBroker.Instance.Publish<TEvent>(data);
		}
	}
}
