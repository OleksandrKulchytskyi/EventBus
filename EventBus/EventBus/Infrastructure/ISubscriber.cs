using System;

namespace EventBus.Infrastructure
{
	public interface ISubscriber
	{
		void Subscribe();

		Type GetEventType();
	}

	public interface ISubscriber<TMsg> : ISubscriber
	{
		event EventHandler<BusEventArgs<TMsg>> EventReceived;

		event EventHandler<BusEventArgs<TMsg>> EventHandled;

		void HandleEvent(TMsg target);
	}

	[Serializable]
	public class BusEventArgs<TMsg> : EventArgs
	{
		public TMsg Data { get; private set; }

		public BusEventArgs(TMsg eventData)
		{
			this.Data = eventData;
		}
	}
}