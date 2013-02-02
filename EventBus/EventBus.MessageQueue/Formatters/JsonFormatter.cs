using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EventBus.MessageQueue.Formatters
{
	public class JsonFormatter : System.Messaging.IMessageFormatter, ICloneable
	{
		readonly int bodyType;
		readonly Newtonsoft.Json.JsonSerializerSettings _settings;

		public JsonFormatter()
		{
			bodyType = 0x350;
			_settings = new Newtonsoft.Json.JsonSerializerSettings();
			_settings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects;
			_settings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
			_settings.Formatting = Newtonsoft.Json.Formatting.Indented;
			_settings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
			_settings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
		}

		public bool CanRead(System.Messaging.Message message)
		{
			if (message == null)
				throw new ArgumentNullException("message");

			if (message.BodyType != bodyType)
				return false;

			return true;
		}

		public object Read(System.Messaging.Message message)
		{
			if (message == null)
				throw new ArgumentNullException("message");

			if (message.BodyType != bodyType)
				throw new InvalidOperationException("InvalidTypeDeserialization");

			Stream bodyStream = message.BodyStream;
			string msg = string.Empty;
			using (StreamReader reader = new StreamReader(bodyStream))
			{
				msg = reader.ReadToEnd();
			}
			if (message.BodyStream.CanSeek)
				message.BodyStream.Position = 0L;

			return Newtonsoft.Json.JsonConvert.DeserializeObject(msg, _settings);
		}

		public void Write(System.Messaging.Message message, object obj)
		{
			if (message == null)
				throw new ArgumentNullException("message");

			if (obj == null)
				throw new ArgumentNullException("obj");

			var msgBody = Newtonsoft.Json.JsonConvert.SerializeObject(obj, _settings);
			Stream serializationStream = new MemoryStream();
			StreamWriter sw = new StreamWriter(serializationStream);
			sw.Write(msgBody);
			sw.Flush();

			message.BodyType = bodyType;
			message.BodyStream = serializationStream;
		}

		public object Clone()
		{
			return (JsonFormatter)this.MemberwiseClone();
		}
	}
}
