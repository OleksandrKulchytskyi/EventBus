using EventBus.Infrastructure;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace EventBus.Redis.Extension
{
	class RedisClientsManager : SingletonBase<RedisClientsManager>
	{
		private IRedisClientsManager Pool;

		private RedisClientsManager()
		{
			//var servers = ConfigurationManager.Instance.GetList("RedisServers");
			Pool = new PooledRedisClientManager(new string[] { }, Enumerable.Empty<string>());
		}

		public IRedisClient GetClient()
		{
			return Pool.GetClient();
		}
	}
}
