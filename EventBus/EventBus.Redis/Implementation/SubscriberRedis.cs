using EventBus.Infrastructure;
using EventBus.Redis.Extension;
using ServiceStack.Redis;
using System;

namespace EventBus.Redis.Implementation
{
	public class SubscriberRedis<TEvnt> : ISubscriber<TEvnt>, IUnsubscribe, IDisposable
	{
		private const string Heartbeat = "Server/Heartbeat";
		private readonly string MainChannel = typeof(TEvnt).Name + "/Main";
		private int _disposed = 0;
		private int _closing = 0;
		private int _subscribed = 0;
		private readonly int _port;
		private readonly string _host;
		private int _MessageReceived = 0;
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
				_port = 6379;
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

		public event EventHandler RedisSubscriptionSuccess;

		protected virtual void OnRedisSubscriptionSuccess()
		{
			EventHandler eh = System.Threading.Interlocked.CompareExchange(ref RedisSubscriptionSuccess, null, null);
			if (eh != null)
			{
				eh.Invoke(this, EventArgs.Empty);
			}
		}

		public void Subscribe()
		{
			if (System.Threading.Interlocked.Exchange(ref _subscribed, 1) == 0)
			{
				System.Threading.Tasks.Task.Factory.StartNew(() =>
				{
					StartListen();
				}, System.Threading.Tasks.TaskCreationOptions.LongRunning);
			}
		}

		private void StartListen()
		{
			using (var redisConsumer = new RedisClient(_host, _port))
			{
				try
				{
					_redisSubscription = redisConsumer.CreateSubscription();

					_redisSubscription.OnSubscribe = OnChannelSubscribe;
					_redisSubscription.OnUnSubscribe = OnChannelUnsubscribe;
					_redisSubscription.OnMessage = OnChannelMessage;

					System.Diagnostics.Debug.WriteLine("Before calling SubscribeToChannels");

					_redisSubscription.SubscribeToChannels(MainChannel, Heartbeat);
					_redisSubscription.OnSubscribe = null;
					_redisSubscription.OnUnSubscribe = null;
					_redisSubscription.OnMessage = null;
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.Message);
					if (_redisSubscription != null)
						_redisSubscription.UnSubscribeFromAllChannels();

					throw;
				}
				finally
				{
					_redisSubscription.Dispose();
					_redisSubscription = null;
				}
			}
		}

		protected void OnChannelSubscribe(string channel)
		{
			OnRedisSubscriptionSuccess();
		}

		protected void OnChannelUnsubscribe(string channel)
		{
			System.Diagnostics.Debug.WriteLine("UnSubscribed from '{0}'", channel);
		}

		protected void OnChannelMessage(string channel, string msg)
		{
			if (_closing == 1)
			{
				_redisSubscription.UnSubscribeFromAllChannels();
			}
			if (channel.Equals(Heartbeat, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			else if (channel.Equals(MainChannel, StringComparison.OrdinalIgnoreCase))
			{
				var evnt = msg.DeserializeFromJSON<TEvnt>();
				System.Threading.Interlocked.Increment(ref _MessageReceived);
				OnEventReceived(evnt);
			}
		}

		public Type GetEventType()
		{
			return typeof(TEvnt);
		}

		public int GetTotalReceivedMsgs
		{
			get { return _MessageReceived; }
		}

		public void Unsubscribe()
		{
			if (System.Threading.Interlocked.Exchange(ref _closing, 1) == 0)
			{
				using (var client = new RedisClient(_host, _port))
				{
					client.PublishMessage(Heartbeat, Heartbeat);
				}
			}
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
					if (_closing == 0)
					{
						Unsubscribe();
					}

					var subs = System.Threading.Interlocked.CompareExchange<IRedisSubscription>(ref _redisSubscription, null, null);
					if (subs != null)
					{
						subs.UnSubscribeFromChannels(MainChannel, Heartbeat);
						subs.Dispose();
					}
				}
			}
		}
	}
}