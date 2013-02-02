using System;

namespace EventBus.Infrastructure
{
	public interface ISubscriber
	{
		void Subscribe();

		Type GetEventType();
	}

	public interface ISubscriber<TData> : ISubscriber
	{
		event EventHandler<BusEventArgs<TData>> EventReceived;

		event EventHandler<BusEventArgs<TData>> EventHandled;

		void Handle(TData target);
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