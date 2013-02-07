using System.IO;

namespace EventBus.Infrastructure
{
	public interface IMessageSerializer
	{
		void Serialize<TMessage>(TMessage message, Stream stream);

		IMessage Deserialize(Stream stream);
	}
}