using EventBus.MessageQueue.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace EvenBus.Test
{
	[TestClass]
	public class MQUtilsTest
	{
		private ManualResetEvent mre;
		private CountdownEvent countEvnt = null;

		[TestInitialize]
		public void Init()
		{
			mre = new ManualResetEvent(false);
			countEvnt = new CountdownEvent(2);
		}

		[TestCleanup]
		public void Clean()
		{
			if (mre != null)
			{
				mre.Dispose();
				mre = null;
			}
			if (countEvnt != null)
				countEvnt.Dispose();
		}

		[TestMethod]
		public void CreateIfNotExistsTest()
		{
			var req = new EventBus.MessageQueue.Infrastructure.MessageQueueRequest() { QueuePath = ".\\Private$\\EventBusMQ" };
			var result = EventBus.MessageQueue.Extensions.Utilities.CreateIfNotExist(req);
			Assert.IsTrue(result == 1);

			result = EventBus.MessageQueue.Extensions.Utilities.DeleteQueue(req.QueuePath);
			Assert.IsTrue(result == 1);
		}

		[TestMethod]
		public void PublisherMqTest()
		{
			PublisherMQ<TestEvent> publisher = new PublisherMQ<TestEvent>();
			publisher.QueuePath = ".\\Private$\\EventBusMQ";
			publisher.IsTransactional = false;
			publisher.Publish(new TestEvent() { Data = "Hello from mq", Processed = false });

			using (var subscriber = new SubscriberMQ<TestEvent>())
			{
				subscriber.QueuePath = ".\\Private$\\EventBusMQ";

				if (subscriber != null)
				{
					subscriber.EventReceived += data_EventReceived;
					subscriber.EventHandled += data_EventHandled;
					subscriber.Subscribe();
				}

				if (!mre.WaitOne(TimeSpan.FromSeconds(40)))
				{
					Assert.Fail("Timeout");
				}
			}
		}

		private void data_EventHandled(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			mre.Set();
		}

		private void data_EventReceived(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			mre.Set();
		}

		[TestMethod]
		public void PublisherMqwithTwoSubscrTest()
		{
			var subscriber1 = new SubscriberMQ<TestEvent>();
			subscriber1.QueuePath = ".\\Private$\\EventBusMQ";

			if (subscriber1 != null)
			{
				subscriber1.EventReceived += data_EventReceived2;
				subscriber1.EventHandled += data_EventHandled2;
				subscriber1.Subscribe();
			}

			var subscriber2 = new SubscriberMQ<TestEvent>();
			subscriber2.QueuePath = ".\\Private$\\EventBusMQ";

			if (subscriber2 != null)
			{
				subscriber2.EventReceived += data_EventReceived2;
				subscriber2.EventHandled += data_EventHandled2;
				subscriber2.Subscribe();
			}

			PublisherMQ<TestEvent> publisher = new PublisherMQ<TestEvent>();
			publisher.QueuePath = ".\\Private$\\EventBusMQ";
			publisher.IsTransactional = false;
			publisher.Publish(new TestEvent() { Data = "Hello from mq", Processed = false });

			if (!countEvnt.Wait(TimeSpan.FromSeconds(5)))
				Assert.Fail();

			subscriber1.Dispose();
			subscriber2.Dispose();
		}

		private void data_EventHandled2(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
		}

		private void data_EventReceived2(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			countEvnt.Signal();
			System.Diagnostics.Debug.WriteLine("+++++");
		}
	}
}