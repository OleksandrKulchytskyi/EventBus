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

		[TestInitialize]
		public void Init()
		{
			mre = new ManualResetEvent(false);
		}

		[TestCleanup]
		public void Clean()
		{
			if (mre != null)
			{
				mre.Dispose();
				mre = null;
			}
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

			var subscriber = new SubscriberMQ<TestEvent>();
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

		private void data_EventHandled(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			mre.Set();
		}

		private void data_EventReceived(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			mre.Set();
		}
	}
}