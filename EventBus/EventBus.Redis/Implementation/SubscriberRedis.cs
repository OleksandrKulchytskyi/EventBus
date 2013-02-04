using EventBus.Infrastructure;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.Redis.Implementation
{
	public class SubscriberRedis<TEvnt> : ISubscriber<TEvnt>, IUnsubscribe, IDisposable
	{
		private int _disposed = 0;
		private int _subscribed = 0;
		private readonly int _port;
		private readonly string _host;

		private RedisClient _redisConsumer = null;
		private IRedisSubscription _redisSubscription = null;

		public SubscriberRedis()
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
				_port = 2928;
			}
		}

		public event EventHandler<BusEventArgs<TEvnt>> EventReceived;

		protected virtual void OnEventReceived(TEvnt message)
		{
			var ev = EventReceived;
			if (null != ev)
			{
				ev(this, new BusEventArgs<TEvnt>(message));
			}
		}

		public event EventHandler<BusEventArgs<TEvnt>> EventHandled;

		protected virtual void OnEventHandled(TEvnt message)
		{
			var ev = EventHandled;
			if (null != ev)
			{
				ev(this, new BusEventArgs<TEvnt>(message));
			}
		}

		public virtual void HandleEvent(TEvnt target)
		{
			OnEventHandled(target);
		}

		public void Subscribe()
		{
			if (System.Threading.Interlocked.Exchange(ref _subscribed, 1) == 0)
			{
				_redisConsumer = new RedisClient(_host, _port);
				_redisSubscription= _redisConsumer.CreateSubscription();
				_redisSubscription.SubscribeToChannels(typeof(TEvnt).Name);
			}
		}

		public Type GetEventType()
		{
			return typeof(TEvnt);
		}

		public void Unsubscribe()
		{
			Dispose();
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (System.Threading.Interlocked.Exchange(ref _disposed, 1) == 0)
			{
				if (disposing)
				{
					var client = System.Threading.Interlocked.CompareExchange<RedisClient>(ref _redisConsumer, null, null);
					var subs = System.Threading.Interlocked.CompareExchange<IRedisSubscription>(ref _redisSubscription, null, null);
					if(client!=null && subs!=null)
					{
						subs.UnSubscribeFromAllChannels();
						subs.Dispose();
						client.Dispose();
						subs = null; client = null;
					}
				}
			}
		}
	}
}
