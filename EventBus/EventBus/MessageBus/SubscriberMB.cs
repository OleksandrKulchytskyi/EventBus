using EventBus.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EventBus.MessageBus
{
	public class SubscriberMB<TEvent> : ISubscriber<TEvent>, INotify<TEvent>, IUnsubscribe, IDisposable
	{
		public event EventHandler<BusEventArgs<TEvent>> EventReceived;
		protected virtual void OnEventReceived(TEvent data)
		{
			var eh = Interlocked.CompareExchange(ref EventReceived, null, null);
			if (eh != null)
				eh.Invoke(this, new BusEventArgs<TEvent>(data));
		}

		public event EventHandler<BusEventArgs<TEvent>> EventHandled;
		protected virtual void OnEventHandled(TEvent data)
		{
			var eh = Interlocked.CompareExchange(ref EventHandled, null, null);
			if (eh != null)
				eh.Invoke(this, new BusEventArgs<TEvent>(data));
		}

		public virtual void HandleEvent(TEvent target)
		{
			OnEventHandled(target);
		}

		public void Subscribe()
		{
			MessageBroker.Instance.Subscribe<TEvent>(this);
		}

		public Type GetEventType()
		{
			return typeof(TEvent);
		}

		public void Unsubscribe()
		{
			MessageBroker.Instance.Unsubscribe<TEvent>(this);
		}

		public virtual void Notify(TEvent msg)
		{
			OnEventReceived(msg);
		}


		#region IDisposable Members

		/// <summary>
		/// Internal variable which checks if Dispose has already been called
		/// </summary>
		private int disposed = 0;

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(Boolean disposing)
		{
			if (Interlocked.Exchange(ref  disposed, 1) == 1)
				return;

			if (disposing)
			{
				Unsubscribe();

				foreach (Delegate d in EventHandled.GetInvocationList())
				{
					EventHandled -= (EventHandler<BusEventArgs<TEvent>>)d;
				}
				foreach (Delegate d in EventReceived.GetInvocationList())
				{
					EventReceived -= (EventHandler<BusEventArgs<TEvent>>)d;
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

		#endregion
	}
}
