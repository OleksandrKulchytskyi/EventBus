using EventBus.Infrastructure;
using EventBus.Redis.Extension;
using System;

namespace EventBus.Redis.Implementation
{
	public class PublisherRedis<TEvnt> : IPublisher<TEvnt>
	{
		private readonly int _port;
		private readonly string _host;

		public PublisherRedis()
		{
			if (Config.EventBusRedisConfigSection.IsConfigured)
			{
				_host = Config.EventBusRedisConfigSection.Current.RedisHost;
				if (!Int32.TryParse(Config.EventBusRedisConfigSection.Current.RedisPort, out _port))
					throw new ArgumentException("Fail to parse port value from config file");
			}
			else
			{
				_host = "localhost";
				_port = 6379;
			}
		}

		public void Publish(TEvnt data)
		{
			using (var redisPublisher = new ServiceStack.Redis.RedisClient(_host, _port))
			{
				try
				{
					redisPublisher.PublishMessage(typeof(TEvnt).Name, data.SerializeToJSON());
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message);
					throw;
				}
			}
		}
	}
}