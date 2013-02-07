using EventBus.Infrastructure;
using EventBus.Redis.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace EventBus.Test
{
	[TestClass]
	public class EventRedisTest
	{
		private bool RedisSubscribed = false;
		private bool Handled = false;
		private bool Received = false;
		private ManualResetEvent mre = null;
		private CountdownEvent countEvent = null;

		#region init
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
		#endregion

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
			else
				Assert.IsTrue(RedisSubscribed);

			mre.Reset();

			if (!mre.WaitOne(TimeSpan.FromSeconds(10)))
				Assert.Fail();

			mre.Reset();

			(subs as IUnsubscribe).Unsubscribe();
		}

		private void subs_RedisSubscriptionSuccess(object sender, EventArgs e)
		{
			RedisSubscribed = true;
			mre.Set();
		}

		private void subs_EventHandled(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			Handled = true;
		}

		private void subs_EventReceived(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			Received = true;
			mre.Set();
			(sender as ISubscriber<TestEvent>).HandleEvent(e.Data);
		}

		#endregion Test1

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

		private void subs_RedisSubscriptionSuccess2(object sender, EventArgs e)
		{
			RedisSubscribed = true;
			mre.Set();
		}

		private void subs_EventHandled2(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			System.Diagnostics.Debug.WriteLine("New message handled");
		}

		private void subs_EventReceived2(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			countEvent.Signal();
			System.Diagnostics.Debug.WriteLine(string.Format("New message received {0}", e.Data.Data));
			Received = true;
			(sender as ISubscriber<TestEvent>).HandleEvent(e.Data);
		}
	}
}