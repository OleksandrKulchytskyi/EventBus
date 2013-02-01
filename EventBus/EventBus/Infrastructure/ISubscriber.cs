using System;

namespace EventBus.Infrastructure
{
	public interface ISubscriber
	{
		void Subscribe();

		Type GetEventType();
	}

	public interface ISubscriber<Evnt> : ISubscriber
	{
		event EventHandler<BusEventArgs<Evnt>> EventCaught;

		event EventHandler<BusEventArgs<Evnt>> EventHandled;

		void Handle(Evnt target);
	}

	[Serializable]
	public class BusEventArgs<T> : EventArgs
	{
		public T Data { get; private set; }

		public BusEventArgs(T eventData)
		{
			this.Data = eventData;
		}
	}
}