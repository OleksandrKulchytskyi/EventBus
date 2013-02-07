using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters;

namespace EventBus.Infrastructure
{
	public class BsonMessageSerializer : IMessageSerializer
	{
		public void Serialize<TMessage>(TMessage message, Stream stream)
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			jsonSerializer.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
			jsonSerializer.TypeNameHandling = TypeNameHandling.All;
			
			BsonWriter bsonWriter = new BsonWriter(stream);
			jsonSerializer.Serialize(bsonWriter, message);
			bsonWriter.Flush();
		}

		public IMessage Deserialize(Stream stream)
		{
			JsonSerializer jsonSerializer = new JsonSerializer();
			jsonSerializer.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
			jsonSerializer.TypeNameHandling = TypeNameHandling.Arrays;
			jsonSerializer.ObjectCreationHandling = ObjectCreationHandling.Auto;
			
			BsonReader jsonReader = new BsonReader(stream, false, DateTimeKind.Utc);
			return (IMessage)jsonSerializer.Deserialize(jsonReader);
		}
	}
}