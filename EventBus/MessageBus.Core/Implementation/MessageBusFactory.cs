using MessageBus.Core.Infrastructure;

namespace MessageBus.Core.Implementation
{
	/// <summary>
	/// Preferred way to create MessageBus instances. Maintains a singleton MessageQueueCache instance
	/// that is used for all bus instances so that a sequential service that interact with more than one bus
	/// still processes all messages sequentially, rather than messages from one bus concurrently with
	/// messages from another bus (even though the message stream from each bus would still be sequentially processed).
	/// </summary>
	public static class MessageBusFactory
	{
		public static readonly MessageQueueCache MessageQueueCache = new MessageQueueCache();

		public static IMessageBus Create()
		{
			return new MessageBus(new MessageToQueuesMap(MessageQueueCache));
		}
	}
}