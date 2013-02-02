using System;

namespace EventBus.RabbitMQ.Infrastructure
{
	public interface IConnectionInfo
	{
		string HostName { get; }

		string VirtualHost { get; }

		string UserName { get; }

		string Password { get; }

		string Protocol { get; }

		Nullable<int> Port { get; }
	}
}