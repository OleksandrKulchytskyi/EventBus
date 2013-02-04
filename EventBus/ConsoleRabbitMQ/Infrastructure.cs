using EventBus.Implementation;
using EventBus.RabbitMQ.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ConsoleRabbitMQ
{
	public class BaseSubscriber<T> : SubscriberRMQ<T>
	{
		public BaseSubscriber()
		{
			HostName = ConfigurationManager.AppSettings["RABBITMQ_SERVER"];
			Protocol = ConfigurationManager.AppSettings["RABBITMQ_PROTOCOL"];
		}
	}

	public class BasePublisher<T> : PublisherRMQ<T>
	{
		public BasePublisher()
		{
			HostName = ConfigurationManager.AppSettings["RABBITMQ_SERVER"];
			Protocol = ConfigurationManager.AppSettings["RABBITMQ_PROTOCOL"];
		}
	}

	public class Message1Publisher : BasePublisher<Message1>
	{
	}

	public class Message2Publisher : BasePublisher<Message2>
	{
	}

	public class Message3Publisher : BasePublisher<Message3>
	{
	}

	public class Message1Subscriber : BaseSubscriber<Message1>
	{
		public override void HandleEvent(Message1 target)
		{
			base.HandleEvent(target);

			Console.WriteLine("Message1 created [{0}] received [{1}]", target.StartInstant, DateTime.Now);
			Publishers.Current.Publish(new Message2());
		}
	}

	public class Message2Subscriber : BaseSubscriber<Message2>
	{
		public override void HandleEvent(Message2 target)
		{
			base.HandleEvent(target);

			Console.WriteLine("Message2 created [{0}] received [{1}]", target.StartInstant, DateTime.Now);
			Publishers.Current.Publish(new Message3());
		}
	}

	public class Message3Subscriber : BaseSubscriber<Message3>
	{
		public override void HandleEvent(Message3 target)
		{
			base.HandleEvent(target);
			Console.WriteLine("Message3 created [{0}] received [{1}]", target.StartInstant, DateTime.Now);
		}
	}
}
