using EventBus.Infrastructure;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace EventBus.Hosting
{
	public class MemorySubscriber<TEvnt> : ISubscriber<TEvnt>
	{
		public Timer Timer { get; set; }
		public ILog Logger { get; set; }

		public MemorySubscriber()
		{
			if (DefaultSingleton<ICreator>.Instance != null)
				this.Logger = DefaultSingleton<ICreator>.Instance.Create<ILog>();
			DefaultSingleton<Queue<TEvnt>>.Instance = DefaultSingleton<Queue<TEvnt>>.Instance ?? new Queue<TEvnt>();
		}

		public event EventHandler<BusEventArgs<TEvnt>> EventReceived;

		protected virtual void OnEventReceived(TEvnt target)
		{
			if (this.Logger != null)
				this.Logger.Debug(string.Format("Event received '{0}' via MemorySubscriber '{1}'", typeof(TEvnt).Name, this.GetType().Name));

			if (this.EventReceived != null)
				this.EventReceived(this, new BusEventArgs<TEvnt>(target));
		}

		public event EventHandler<BusEventArgs<TEvnt>> EventHandled;

		protected virtual void OnEventHandled(TEvnt target)
		{
			if (this.Logger != null)
				this.Logger.Debug(string.Format("Event handled '{0}' via MemorySubscriber '{1}'", typeof(TEvnt).Name, this.GetType().Name));

			if (this.EventHandled != null)
				this.EventHandled(this, new BusEventArgs<TEvnt>(target));
		}

		public virtual void HandleEvent(TEvnt target)
		{
			OnEventHandled(target);
		}

		public void Subscribe()
		{
			if (this.Logger != null)
				this.Logger.Debug(string.Format("Subscribing to event '{0}' with MemorySubscriber '{1}'", typeof(TEvnt).Name, this.GetType().Name));

			this.Timer = new Timer(TimeSpan.FromMilliseconds(500).TotalMilliseconds);
			this.Timer.Elapsed += new ElapsedEventHandler(OnElapsed);
			this.Timer.Start();
		}

		private void OnElapsed(object sender, ElapsedEventArgs e)
		{
			if (DefaultSingleton<Queue<TEvnt>>.Instance.Any())
			{
				var item = DefaultSingleton<Queue<TEvnt>>.Instance.Dequeue();
				//var item = DefaultSingleton<Queue<TEvnt>>.Instance.Peek();
				this.HandleEvent(item);
			}
		}

		public Type GetEventType()
		{
			return typeof(TEvnt);
		}
	}
}