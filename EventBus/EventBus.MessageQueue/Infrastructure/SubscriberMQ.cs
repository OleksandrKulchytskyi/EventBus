using EventBus.Infrastructure;
using EventBus.Logging;
using EventBus.MessageQueue.Extensions;
using log4net;
using System;

namespace EventBus.MessageQueue.Infrastructure
{
	public class SubscriberMQ<TEvnt> : ISubscriber<TEvnt>, IUnsubscribe, IDisposable
	{
		protected readonly System.Messaging.IMessageFormatter _formatter;
		protected ILog Logger { get; set; }
		public virtual string QueuePath { get; set; }
		private int _disposed = 0;
		private int _subscribed = 0;

		private System.Messaging.MessageQueue _mq;
		private readonly System.Threading.CancellationTokenSource _cts;

		public SubscriberMQ()
			: this(new Logger())
		{
		}

		public SubscriberMQ(ILog log)
		{
			if (log == null)
				throw new ArgumentNullException("log");
			this.Logger = log;
			_cts = new System.Threading.CancellationTokenSource();

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

		public virtual void HandleEvent(TEvnt target)
		{
			OnEventHandled(target);
		}

		public void Subscribe()
		{
			if (System.Threading.Interlocked.Exchange(ref _subscribed, 1) == 0)
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

					_mq = new System.Messaging.MessageQueue(this.QueuePath);
					_mq.Formatter = _formatter;
					_mq.ReceiveCompleted += OnMQReceiveCompleted;

					_mq.BeginReceive();
				}
				catch (Exception ex)
				{
					this.Logger.Error(string.Format("Error during {0}.Subscribe()", this.GetType().Name), ex);
				}
			}
		}

		private void OnMQReceiveCompleted(object sender, System.Messaging.ReceiveCompletedEventArgs e)
		{
			try
			{
				if (_cts.IsCancellationRequested)
					return;

				System.Messaging.Message mqMsg = null;
				TEvnt data = default(TEvnt);
				var mq = (sender as System.Messaging.MessageQueue);

				mqMsg = mq.EndReceive(e.AsyncResult);
				data = (TEvnt)mqMsg.Body;
				this.OnEventReceived(data);

				if (_cts.IsCancellationRequested)
					return;

				mq.BeginReceive();
			}
			catch (Exception ex)
			{
				this.Logger.Error(string.Format("Error during {0}.Subscribe(), in ReceiveCompleted Handler",
						this.GetType().Name), ex);
			}
		}

		public Type GetEventType()
		{
			return typeof(TEvnt);
		}

		public void Unsubscribe()
		{
			if (_disposed == 0 && !_cts.IsCancellationRequested)
				_cts.Cancel();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (System.Threading.Interlocked.Exchange(ref _disposed, 1) == 0)
			{
				if (disposing)
				{
					if (!_cts.IsCancellationRequested)
						_cts.Cancel();

					_mq.ReceiveCompleted -= OnMQReceiveCompleted;
					_mq.Close();
					_mq.Dispose();

				}
			}
		}
	}
}