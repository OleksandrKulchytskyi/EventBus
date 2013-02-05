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
			var serTask = Newtonsoft.Json.JsonConvert.SerializeObjectAsync(e, Newtonsoft.Json.Formatting.Indented);
			serTask.Wait();
			return System.Text.Encoding.UTF8.GetBytes(serTask.Result);
		}

		public static E Deserialize<E>(this byte[] messageData)
		{
			var task = Newtonsoft.Json.JsonConvert.DeserializeObjectAsync<E>(System.Text.Encoding.UTF8.GetString(messageData));
			task.Wait();
			return task.Result;
		}

		public static ConnectionFactory CreateConnectionFactory(this IConnectionInfo connectionDescriptor)
		{
			IProtocol protocol = null;
			if (!string.IsNullOrEmpty(connectionDescriptor.Protocol))
				protocol = Protocols.SafeLookup(connectionDescriptor.Protocol);

			if (protocol != null)
				protocol = Protocols.FromConfiguration() ?? Protocols.FromEnvironment() ?? Protocols.FromEnvironmentVariable() ?? Protocols.DefaultProtocol;

			ConnectionFactory connectionFactory = new ConnectionFactory
			{
				HostName = connectionDescriptor.HostName,
				Protocol = protocol
			};

			if (!string.IsNullOrEmpty(connectionDescriptor.VirtualHost))
				connectionFactory.VirtualHost = connectionDescriptor.VirtualHost;

			if (!string.IsNullOrEmpty(connectionDescriptor.UserName))
				connectionFactory.UserName = connectionDescriptor.UserName;

			if (!string.IsNullOrEmpty(connectionDescriptor.Password))
				connectionFactory.Password = connectionDescriptor.Password;

			return connectionFactory;
		}
	}
}