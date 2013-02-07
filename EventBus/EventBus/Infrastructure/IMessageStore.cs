using System.Collections.Generic;

namespace EventBus.Infrastructure
{
	public interface IMessageStore
	{
		/// <summary>
		/// Retrieves all messages since last <paramref name="checkpoint" /> and marks them
		/// as received.
		/// </summary>
		/// <param name="checkpoint"></param>
		/// <returns></returns>
		IEnumerable<IMessage> GetMessages(long checkpoint);

		void SaveMessage<TEvent>(TEvent message, long checkpoint) where TEvent : IMessage;
	}

	public interface IMessage
	{
	}
}