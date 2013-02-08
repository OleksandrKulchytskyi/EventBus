using EventBus.Infrastructure;
using System;

namespace EventBus.Deffered
{
	public class DelayedSubscriber<TEvent> : ISubscriber<TEvent>, INotify<TEvent>, IUnsubscribe, IDisposable where TEvent : class,IMessage
	{
		private IDisposable subscription = null;
		protected int _unsubscribed = 0;
		protected int disposed;

		public DelayedSubscriber()
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

		public void Notify(TEvent msg)
		{
			OnEventReceived(msg);
		}

		public event EventHandler<BusEventArgs<TEvent>> EventReceived;

		protected virtual void OnEventReceived(TEvent data)
		{
			var eh = System.Threading.Interlocked.CompareExchange(ref EventReceived, null, null);
			if (eh != null)
				eh.Invoke(this, new BusEventArgs<TEvent>(data));
		}

		public event EventHandler<BusEventArgs<TEvent>> EventHandled;

		protected virtual void OnEventHandled(TEvent data)
		{
			var eh = System.Threading.Interlocked.CompareExchange(ref EventHandled, null, null);
			if (eh != null)
				eh.Invoke(this, new BusEventArgs<TEvent>(data));
		}

		public void HandleEvent(TEvent target)
		{
			OnEventHandled(target);
		}

		public virtual void Subscribe()
		{
			System.Threading.Interlocked.Exchange(ref _unsubscribed, 0);
			subscription = DefaultSingleton<IDsiposingMessageBus>.Instance.Subscribe<TEvent>(this);
		}

		public Type GetEventType()
		{
			return typeof(TEvent);
		}

		public virtual void Unsubscribe()
		{
			if (System.Threading.Interlocked.Exchange(ref _unsubscribed, 1) == 0)
				subscription.Dispose();
		}

		#region IDisposable Members

		protected virtual void Dispose(Boolean disposing)
		{
			if (System.Threading.Interlocked.Exchange(ref disposed, 1) == 1)
				return;

			Unsubscribe();

			if (disposing)
			{
				foreach (var eh in EventHandled.GetInvocationList())
				{
					EventHandled -= (EventHandler<BusEventArgs<TEvent>>)eh;
				}
				foreach (var eh in EventReceived.GetInvocationList())
				{
					EventReceived -= (EventHandler<BusEventArgs<TEvent>>)eh;
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion IDisposable Members
	}
}