using EventBus.RabbitMQ.Infrastructure;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;

namespace EventBus.RabbitMQ.Extension
{
	public static class Utilities
	{
		public static byte[] Serialize<E>(this E e)
		{
			var formatter = new XmlMessageFormatter(new[] { typeof(E) });
			var message = new Message();
			formatter.Write(message, e);

			message.BodyStream.Position = 0;
			List<byte> bytes = new List<byte>((int)message.BodyStream.Length);

			using (var ms = message.BodyStream)
			{
				const int bufferSize = 1024;
				byte[] buffer = new byte[bufferSize];
				int read = ms.Read(buffer, 0, bufferSize);
				while (0 != read)
				{
					bytes.AddRange(buffer.Take(read));
					read = ms.Read(buffer, 0, bufferSize);
				}
				return bytes.ToArray();
			}
		}

		public static E Deserialize<E>(this byte[] messageData)
		{
			var formatter = new XmlMessageFormatter(new[] { typeof(E) });
			var message = new Message();
			message.BodyStream.Write(messageData, 0, messageData.Length);
			message.BodyStream.Position = 0;

			object o = formatter.Read(message);
			if (null == o)
			{
				return default(E);
			}

			return (E)o;
		}

		public static ConnectionFactory CreateConnectionFactory(this IConnectionInfo connectionDescriptor)
		{
			IProtocol protocol = null;
			if (!String.IsNullOrEmpty(connectionDescriptor.Protocol))
			{
				protocol = Protocols.SafeLookup(connectionDescriptor.Protocol);
			}

			if (null == protocol)
			{
				protocol = Protocols.FromConfiguration() ??
						   Protocols.FromEnvironment() ??
						   Protocols.FromEnvironmentVariable() ??
						   Protocols.DefaultProtocol;
			}

			ConnectionFactory connectionFactory = new ConnectionFactory
			{
				HostName = connectionDescriptor.HostName,
				Protocol = protocol
			};
			if (!String.IsNullOrEmpty(connectionDescriptor.VirtualHost))
			{
				connectionFactory.VirtualHost = connectionDescriptor.VirtualHost;
			}
			if (!String.IsNullOrEmpty(connectionDescriptor.UserName))
			{
				connectionFactory.UserName = connectionDescriptor.UserName;
			}
			if (!String.IsNullOrEmpty(connectionDescriptor.Password))
			{
				connectionFactory.Password = connectionDescriptor.Password;
			}

			return connectionFactory;
		}
	}
}