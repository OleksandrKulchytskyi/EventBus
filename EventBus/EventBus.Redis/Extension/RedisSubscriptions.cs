using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Threading;

namespace EventBus.Redis.Extension
{
	public class RedisSubscription<T> : IDisposable, IObservable<T>
	{
		private ServiceStack.Redis.RedisClient redis;
		private ServiceStack.Redis.IRedisSubscription subscription;
		private System.Threading.Thread worker;
		private SynchronizationContext workerSyncContext;
		private string _channelName;

		private List<IObserver<T>> observers = new List<IObserver<T>>();
		private List<IDisposable> observerSubDisposables = new List<IDisposable>();

		private bool isDisposed = false;
		private bool isSubscribed = false;

		public RedisSubscription(string channelName)
		{
			_channelName = channelName;

			worker = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(o =>
			{
				try
				{
					workerSyncContext = new SynchronizationContext();
					redis = ServiceStack.Redis.RedisClientFactory.Instance.CreateRedisClient("localhost", 6379);

					//redis = RedisManager.GetClient();
					subscription = redis.CreateSubscription();

					subscription.OnMessage = (channel, msg) =>
					{
						try
						{
							if (isDisposed)
							{
								if (isSubscribed)
								{
									subscription.UnSubscribeFromAllChannels();
									isSubscribed = false;
								}
								return;
							}

							//assume json
							var typedMsg = msg.FromJson<T>();

							lock (observers)
							{
								foreach (var observer in observers)
								{
									observer.OnNext(typedMsg);
								}
							}
						}
						finally
						{
						}
					};

					isSubscribed = true;
					subscription.SubscribeToChannels(_channelName);
				}
				catch (Exception ex)
				{
					subscription.UnSubscribeFromAllChannels();
					isSubscribed = false;
					this.Dispose();
				}
				finally
				{
					subscription.Dispose();
				}
			}));
			worker.Start();
		}

		//private event EventHandler Disposing;
		//private void removeSubs(object sender, EventArgs e)
		//{
		//    subscription.UnSubscribeFromAllChannels();
		//    subscription.Dispose();
		//}

		public bool PushMessage<T>(T message)
		{
			return redis.PublishMessage(_channelName, message.ToJson<T>()) > 0;
		}

		event EventHandler IsDisposing;

		public void Dispose()
		{
			if (isDisposed)
				throw new ObjectDisposedException("RedisSubscription");

			isDisposed = true;

			if (IsDisposing != null)
				IsDisposing.Invoke(this, EventArgs.Empty);

			//bool finished = false;
			//workerSyncContext.Post(_ =>
			//{
			//    try
			//    {
			//        subscription.UnSubscribeFromChannels(_channelName);
			//        //subscription.UnSubscribeFromAllChannels();
			//        subscription.Dispose();

			//        lock (observers)
			//        {
			//            foreach (var observer in observers)
			//            {
			//                observer.OnCompleted();
			//            }
			//        }
			//    }
			//    finally
			//    {
			//        finished = true;
			//    }
			//}, null);
			redis.Dispose();

			lock (observers)
			{
				foreach (var observer in observers)
				{
					observer.OnCompleted();
				}
			}

			lock (observerSubDisposables)
			{
				foreach (var oSubDisposable in observerSubDisposables)
				{
					oSubDisposable.Dispose();
				}
			}
		}

		public IDisposable Subscribe(IObserver<T> observer)
		{
			lock (observers)
			{
				observers.Add(observer);
			}
			var oSubDisposable = new ActionDisposable(() => observers.Remove(observer));
			lock (observerSubDisposables)
			{
				observerSubDisposables.Add(oSubDisposable);
			}
			return oSubDisposable;
		}
	}

	/// <summary>
	/// A simple disposable object that fires the action delegate when dispose is called.
	/// </summary>
	public class ActionDisposable : IDisposable
	{
		private Action process;

		public ActionDisposable(Action _process)
		{
			process = _process;
		}

		public void Dispose()
		{
			try
			{
				process();
			}
			finally
			{
			}
		}
	}

	public static class RedisSubscription
	{
		public static bool PushMessage<T>(T message, string channelName)
		{
			//using (var redis = RedisManager.GetClient())
			using (var redis = ServiceStack.Redis.RedisClientFactory.Instance.CreateRedisClient("localhost", 6379))
			{
				return redis.PublishMessage(channelName, message.ToJson<T>()) > 0;
			}
		}
	}
}