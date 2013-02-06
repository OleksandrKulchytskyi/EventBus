using EventBus.Infrastructure;

namespace EventBus.MessageBus
{
	internal interface IMessageBroker
	{
		int GroupCount { get; }

		void Subscribe<TEvent>(ISubscriber<TEvent> subscriber);

		void Unsubscribe<TEvent>(ISubscriber<TEvent> subscriber);

		void Publish<TEvent>(TEvent msg);

		void Drain();
	}
}