using EventBus.Infrastructure;

namespace EventBus.MessageBus
{
	internal interface IMessageBroker:IClearable
	{
		int GroupCount { get; }

		void Subscribe<TEvent>(ISubscriber<TEvent> subscriber);

		void Unsubscribe<TEvent>(ISubscriber<TEvent> subscriber);

		void Publish<TEvent>(TEvent msg);
	}
}