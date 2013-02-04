using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventBus.Redis.Implementation;
using EventBus.Infrastructure;
using System.Threading;

namespace EvenBus.Test
{
	[TestClass]
	public class EventRedisTest
	{
		bool RedisSubscribed = false;
		bool Handled = false;
		bool Received = false;
		ManualResetEvent mre = null;
		CountdownEvent countEvent = null;

		[TestInitialize]
		public void Init()
		{
			mre = new ManualResetEvent(false);
			countEvent = new CountdownEvent(10);
		}

		[TestCleanup]
		public void Close()
		{
			if (mre != null)
			{
				mre.Close();
				mre.Dispose();
				mre = null;
			}
			if (countEvent != null)
				countEvent.Dispose();
		}

		#region Test1
		[TestMethod]
		public void RedisTest()
		{
			SubscriberRedis<TestEvent> subs = new SubscriberRedis<TestEvent>();
			subs.RedisSubscriptionSuccess += subs_RedisSubscriptionSuccess;
			subs.EventReceived += subs_EventReceived;
			subs.EventHandled += subs_EventHandled;

			subs.Subscribe();

			PublisherRedis<TestEvent> pub = new PublisherRedis<TestEvent>();
			pub.Publish(new TestEvent());

			if (!mre.WaitOne(TimeSpan.FromSeconds(10)))
				Assert.Fail();

			mre.Reset();

			if (!mre.WaitOne(TimeSpan.FromSeconds(10)))
				Assert.Fail();

			mre.Reset();

			(subs as IUnsubscribe).Unsubscribe();
		}

		void subs_EventHandled(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			Handled = true;
		}

		void subs_EventReceived(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			Received = true;
			mre.Set();
			(sender as ISubscriber<TestEvent>).HandleEvent(e.Data);
		}

		void subs_RedisSubscriptionSuccess(object sender, EventArgs e)
		{
			RedisSubscribed = true;
			mre.Set();
		}
		#endregion

		[TestMethod]
		public void RedisCountMsgsTest()
		{
			SubscriberRedis<TestEvent> subs = new SubscriberRedis<TestEvent>();
			subs.RedisSubscriptionSuccess += subs_RedisSubscriptionSuccess2;
			subs.EventReceived += subs_EventReceived2;
			subs.EventHandled += subs_EventHandled2;

			subs.Subscribe();

			if (!mre.WaitOne(TimeSpan.FromSeconds(3)))
				Assert.Fail();

			new Thread(new ThreadStart(() =>
			{
				for (int i = 0; i < 10; i++)
				{
					ThreadPool.QueueUserWorkItem((state) =>
					{
						PublisherRedis<TestEvent> pub = new PublisherRedis<TestEvent>();
						pub.Publish(new TestEvent() { Data = DateTime.Now.ToString("hh:mm:ss") });
						Thread.Sleep(950);
					});
				}
			})).Start();

			if (!countEvent.Wait(TimeSpan.FromSeconds(15)))
			{
				Assert.Fail();
			}
			else
			{
				if (subs.GetTotalReceivedMsgs == 10)
					(subs as IUnsubscribe).Unsubscribe();
			}
		}

		void subs_RedisSubscriptionSuccess2(object sender, EventArgs e)
		{
			RedisSubscribed = true;
			mre.Set();
		}

		void subs_EventHandled2(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			System.Diagnostics.Debug.WriteLine("New message handled");
		}

		void subs_EventReceived2(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			countEvent.Signal();
			System.Diagnostics.Debug.WriteLine(string.Format("New message received {0}", e.Data.Data));
			Received = true;
			(sender as ISubscriber<TestEvent>).HandleEvent(e.Data);
		}
	}
}
