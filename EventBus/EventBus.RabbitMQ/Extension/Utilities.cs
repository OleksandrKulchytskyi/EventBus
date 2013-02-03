using EventBus.RabbitMQ.Infrastructure;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EventBus.RabbitMQ.Extension
{
	public static class Utilities
	{
		public static byte[] Serialize<E>(this E e)
		{
			string json=Newtonsoft.Json.JsonConvert.SerializeObject(e, Newtonsoft.Json.Formatting.Indented);
			return System.Text.Encoding.UTF8.GetBytes(json);
		}

		public static E Deserialize<E>(this byte[] messageData)
		{
			var task=Newtonsoft.Json.JsonConvert.DeserializeObjectAsync<E>(System.Text.Encoding.UTF8.GetString(messageData));
			task.Wait();
			return task.Result;
		}

		public static ConnectionFactory CreateConnectionFactory(this IConnectionInfo connectionDescriptor)
		{
			IProtocol protocol = null;
			if (!String.IsNullOrEmpty(connectionDescriptor.Protocol))
				protocol = Protocols.SafeLookup(connectionDescriptor.Protocol);


			if (null == protocol)
				protocol = Protocols.FromConfiguration() ?? Protocols.FromEnvironment() ??
						   Protocols.FromEnvironmentVariable() ?? Protocols.DefaultProtocol;

			ConnectionFactory connectionFactory = new ConnectionFactory
			{
				HostName = connectionDescriptor.HostName,
				Protocol = protocol
			};

			if (!String.IsNullOrEmpty(connectionDescriptor.VirtualHost))
				connectionFactory.VirtualHost = connectionDescriptor.VirtualHost;

			if (!String.IsNullOrEmpty(connectionDescriptor.UserName))
				connectionFactory.UserName = connectionDescriptor.UserName;

			if (!String.IsNullOrEmpty(connectionDescriptor.Password))
				connectionFactory.Password = connectionDescriptor.Password;

			return connectionFactory;
		}
	}
}