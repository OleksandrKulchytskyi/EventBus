using EventBus.Infrastructure;
using EventBus.Logging;
using EventBus.MessageQueue.Extensions;
using log4net;
using System;

namespace EventBus.MessageQueue.Infrastructure
{
	public class PublisherMQ<TEvnt> : IPublisher<TEvnt>
	{
		public virtual string QueuePath { get; set; }

		public virtual bool IsTransactional { get; set; }

		protected readonly System.Messaging.IMessageFormatter _formatter;

		protected ILog Logger { get; set; }

		public PublisherMQ()
			: this(new Logger())
		{
		}

		public PublisherMQ(ILog log)
		{
			if (log == null)
				throw new ArgumentNullException("log");
			this.Logger = log;

			if (Config.EventBusMsmqSection.CheckConfig
				&& !string.IsNullOrEmpty(Config.EventBusMsmqSection.Current.FormatterType))
			{
				Type t = Type.GetType(Config.EventBusMsmqSection.Current.FormatterType);
				_formatter = Activator.CreateInstance(t) as System.Messaging.IMessageFormatter;
			}
			else
				_formatter = new System.Messaging.XmlMessageFormatter(new Type[] { typeof(TEvnt) });
		}

		public virtual void Publish(TEvnt eventToBePublished)
		{
			if (string.IsNullOrEmpty(this.QueuePath))
				return;

			Utilities.CreateIfNotExist(new MessageQueueRequest
			{
				IsTransactional = this.IsTransactional,
				QueuePath = this.QueuePath
			});

			using (var mq = new System.Messaging.MessageQueue(this.QueuePath))
			{
				mq.Formatter = _formatter;
				if (this.Logger != null)
					this.Logger.Debug(string.Format("Sending Event '{1}' to Message Queue '{0}' with publisher '{2}'", this.QueuePath, typeof(TEvnt).Name, this.GetType().Name));

				mq.Send(eventToBePublished);

				if (this.Logger != null)
					this.Logger.Debug(string.Format("Sent Event '{1}' to Message Queue '{0}' with publisher '{2}'", this.QueuePath, typeof(TEvnt).Name, this.GetType().Name));
				
				mq.Close();
			}
		}
	}
}