using EventBus.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Text;

namespace EventBus.Implementation
{
	public class JsonMessageSerializer : IMessageSerializer
	{
		public void Serialize<TMessage>(TMessage message, Stream stream)
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			jsonSerializer.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
			jsonSerializer.TypeNameHandling = TypeNameHandling.All;

			StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
			JsonTextWriter jsonWriter = new JsonTextWriter(streamWriter);

			jsonSerializer.Serialize(jsonWriter, message);
			jsonWriter.Flush();
			streamWriter.Flush();
		}

		public IMessage Deserialize(Stream stream)
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			jsonSerializer.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
			jsonSerializer.TypeNameHandling = TypeNameHandling.Arrays;
			jsonSerializer.ObjectCreationHandling = ObjectCreationHandling.Auto;
			jsonSerializer.Converters.Add(new DataTableConverter());

			JsonTextReader jsonReader = new JsonTextReader(new StreamReader(stream, Encoding.UTF8));
			return (IMessage)jsonSerializer.Deserialize(jsonReader);
		}
	}
}