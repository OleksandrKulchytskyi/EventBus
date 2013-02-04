using System.Configuration;

namespace EventBus.Redis.Config
{
	public class EventBusRedisConfigSection : ConfigurationSection
	{
		private static readonly string _secName = "eventBusRedisSection";

		public static EventBusRedisConfigSection Current
		{
			get { return (ConfigurationManager.GetSection(_secName) as EventBusRedisConfigSection); }
		}

		public static bool IsConfigured
		{
			get
			{
				try
				{
					return ((ConfigurationManager.GetSection(_secName) as EventBusRedisConfigSection) != null);
				}
				catch (System.Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex);
					return false;
				}
			}
		}

		private const string _RedisPortProperty = "port";
		[ConfigurationProperty(_RedisPortProperty, IsRequired = true)]
		public string RedisPort
		{
			get { return (string)base[_RedisPortProperty]; }
			set { base[_RedisPortProperty] = value; }
		}

		private const string _RedisHostProperty = "host";
		[ConfigurationProperty(_RedisHostProperty, IsRequired = true)]
		public string RedisHost
		{
			get { return (string)base[_RedisHostProperty]; }
			set { base[_RedisPortProperty] = value; }
		}
	}
}