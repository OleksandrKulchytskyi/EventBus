using EventBus.Infrastructure;
using EventBus.Logging;
using EventBus.MessageQueue.Extensions;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.MessageQueue.Infrastructure
{
	public class SubscriberMQ<TEvnt> : ISubscriber<TEvnt>
	{
		protected readonly System.Messaging.IMessageFormatter _formatter;
		protected ILog Logger { get; set; }

		public virtual string QueuePath { get; set; }

		public SubscriberMQ()
			: this(new Logger())
		{
		}

		public SubscriberMQ(ILog log)
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

		public event EventHandler<BusEventArgs<TEvnt>> EventReceived;

		protected virtual void OnEventReceived(TEvnt target)
		{
			if (this.Logger != null)
				this.Logger.Debug(string.Format("Event received '{1}' via Message Queue '{0}'", this.QueuePath, typeof(TEvnt).Name));

			if (this.EventReceived != null)
				this.EventReceived(this, new BusEventArgs<TEvnt>(target));
		}

		public event EventHandler<BusEventArgs<TEvnt>> EventHandled;

		protected virtual void OnEventHandled(TEvnt target)
		{
			if (this.EventHandled != null)
				this.EventHandled(this, new BusEventArgs<TEvnt>(target));
		}

		public virtual void Handle(TEvnt target)
		{
			OnEventHandled(target);
		}

		public void Subscribe()
		{
			try
			{
				if (string.IsNullOrEmpty(this.QueuePath))
					return;

				Utilities.CreateIfNotExist(new MessageQueueRequest
				{
					QueuePath = this.QueuePath
				});

				if (this.Logger != null)
					this.Logger.Debug(string.Format("Begin subscribing to event '{1}' via Message Queue '{0}' using type '{2}'",
						this.QueuePath, typeof(TEvnt).Name, this.GetType().Name));

				System.Messaging.MessageQueue mq = new System.Messaging.MessageQueue(this.QueuePath);
				mq.Formatter = _formatter;
				mq.ReceiveCompleted += (sender, e) =>
				{
					try
					{
						System.Messaging.Message msg = mq.EndReceive(e.AsyncResult);
						var target = (TEvnt)msg.Body;

						this.OnEventReceived(target);
						mq.BeginReceive();
					}
					catch (Exception ex)
					{
						this.Logger.Error(string.Format("Error during {0}.Subscribe(), in ReceiveCompleted Handler",
								this.GetType().Name), ex);
					}
				};

				mq.BeginReceive();
			}
			catch (Exception ex)
			{
				this.Logger.Error(string.Format("Error during {0}.Subscribe()", this.GetType().Name), ex);
			}
		}

		public Type GetEventType()
		{
			return typeof(TEvnt);
		}
	}
}
