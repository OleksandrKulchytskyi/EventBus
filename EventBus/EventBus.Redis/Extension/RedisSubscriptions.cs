using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text;
using ServiceStack.Redis;
using System.Reactive.Linq;

namespace EventBus.Redis.Extension
{
	public class RedisSubscription<T> : IDisposable, IObservable<T>
	{
		ServiceStack.Redis.RedisClient redis;
		ServiceStack.Redis.IRedisSubscription subscription;
		System.Threading.Thread worker;
		SynchronizationContext workerSyncContext;
		string _channelName;

		List<IObserver<T>> observers = new List<IObserver<T>>();
		List<IDisposable> observerSubDisposables = new List<IDisposable>();

		bool isDisposed = false;
		bool isSubscribed = false;

		public RedisSubscription(string channelName)
		{
			_channelName = channelName;

			worker = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(o =>
			{
				try
				{
					workerSyncContext = new SynchronizationContext();

					redis = RedisManager.GetClient();
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
		Action process;

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
			using (var redis = RedisManager.GetClient())
			{
				return redis.PublishMessage(channelName, message.ToJson<T>()) > 0;
			}
		}
	}
}
