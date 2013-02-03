using EventBus.Infrastructure;
using EventBus.RabbitMQ.Extension;
using log4net;
using RabbitMQ.Client;

namespace EventBus.RabbitMQ.Infrastructure
{
	public class PublisherRMQ<TEvnt> : IPublisher<TEvnt>, IConnectionInfo
	{
		protected ILog Log;

		public PublisherRMQ()
		{
			Log = LogManager.GetLogger(GetType());
		}

		public PublisherRMQ(ILog logger)
		{
			Log = logger;
		}

		public virtual string RoutingKey
		{
			get { return typeof(TEvnt).FullName; }
		}

		public virtual string ExchangeName
		{
			get { return "EvetBus"; }
		}

		#region IConnectionDescriptor Members

		public virtual string HostName { get; set; }

		public virtual string VirtualHost { get; set; }

		public virtual string UserName { get; set; }

		public virtual string Password { get; set; }

		public virtual string Protocol { get; set; }

		public virtual int? Port { get; set; }

		#endregion IConnectionDescriptor Members

		#region IPublisher<E> Members

		public virtual void Publish(TEvnt eventToBePublished)
		{
			//var formatter = Formatter;
			byte[] body = eventToBePublished.Serialize();

			ConnectionFactory connectionFactory = this.CreateConnectionFactory();
			using (IConnection connection = connectionFactory.CreateConnection())
			{
				using (IModel model = connection.CreateModel())
				{
					model.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
					model.BasicPublish(ExchangeName, RoutingKey, null, body);
				}
			}
		}

		#endregion IPublisher<E> Members
	}
}