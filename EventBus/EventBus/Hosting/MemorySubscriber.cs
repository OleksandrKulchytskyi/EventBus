using EventBus.Infrastructure;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace EventBus.Hosting
{
	public class MemorySubscriber<T> : ISubscriber<T>
	{
		public Timer Timer { get; set; }

		public ILog Logger { get; set; }

		public MemorySubscriber()
		{
			this.Logger = DefaultSingleton<ICreator>.Instance.Create<ILog>();
			DefaultSingleton<Queue<T>>.Instance = DefaultSingleton<Queue<T>>.Instance ?? new Queue<T>();
		}

		public event EventHandler<BusEventArgs<T>> EventReceived;

		protected virtual void OnEventreceived(T target)
		{
			if (this.Logger != null)
				this.Logger.Debug(string.Format("Received event '{0}' via MemorySubscriber '{1}'", typeof(T).Name, this.GetType().Name));

			if (this.EventReceived != null)
				this.EventReceived(this, new BusEventArgs<T>(target));
		}

		public event EventHandler<BusEventArgs<T>> EventHandled;

		protected virtual void OnEventHandled(T target)
		{
			if (this.Logger != null)
				this.Logger.Debug(string.Format("Handled event '{0}' via MemorySubscriber '{1}'", typeof(T).Name, this.GetType().Name));

			if (this.EventHandled != null)
				this.EventHandled(this, new BusEventArgs<T>(target));
		}

		public virtual void Handle(T target)
		{
			OnEventHandled(target);
		}

		public void Subscribe()
		{
			if (this.Logger != null)
				this.Logger.Debug(string.Format("Subscribing to event '{0}' with MemorySubscriber '{1}'", typeof(T).Name, this.GetType().Name));

			this.Timer = new Timer(1000);
			this.Timer.Elapsed += new ElapsedEventHandler(OnElapsed);
			this.Timer.Start();
		}

		private void OnElapsed(object sender, ElapsedEventArgs e)
		{
			if (DefaultSingleton<Queue<T>>.Instance.Any())
			{
				var item = DefaultSingleton<Queue<T>>.Instance.Dequeue();
				this.Handle(item);
			}
		}

		public Type GetEventType()
		{
			return typeof(T);
		}
	}
}