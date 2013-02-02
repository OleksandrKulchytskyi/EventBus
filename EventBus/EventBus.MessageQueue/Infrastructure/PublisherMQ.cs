using EventBus.Infrastructure;
using EventBus.Logging;
using EventBus.MessageQueue.Extensions;
using EventBus.MessageQueue.Infrastructure;
using log4net;
using System.Messaging;
namespace EventBus.MessageQueue.Infrastructure
{
	public class PublisherMQ<E> : IPublisher<E>
	{
		public virtual string QueuePath { get; set; }

		public virtual bool IsTransactional { get; set; }

		protected ILog Logger { get; set; }

		public PublisherMQ()
		{
			this.Logger = new Logger();
		}

		public PublisherMQ(ILog logger)
		{
			this.Logger = logger;
		}

		public virtual void Publish(E eventToBePublished)
		{
			if (string.IsNullOrEmpty(this.QueuePath))
				return;

			Utilities.CreateIfNotExist(new MessageQueueRequest
			{
				IsTransactional = this.IsTransactional,
				QueuePath = this.QueuePath
			});

			var mq = new System.Messaging.MessageQueue(this.QueuePath);

			if (this.Logger != null)
				this.Logger.Debug(string.Format("Sending Event '{1}' to Message Queue '{0}' with publisher '{2}'", this.QueuePath, typeof(E).Name, this.GetType().Name));

			mq.Send(eventToBePublished);

			if (this.Logger != null)
				this.Logger.Debug(string.Format("Sent Event '{1}' to Message Queue '{0}' with publisher '{2}'", this.QueuePath, typeof(E).Name, this.GetType().Name));
		}
	}
}