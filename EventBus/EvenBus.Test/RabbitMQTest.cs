using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventBus.RabbitMQ.Infrastructure;
using System.Threading;
namespace EvenBus.Test
{
	[TestClass]
	public class RabbitMQTest
	{
		ManualResetEvent mre = null;
		bool Received = false;
		bool Handled = false;
		[TestInitialize]
		public void Init()
		{
			try
			{
				mre = new ManualResetEvent(false);
				//EventBus.Implementation.WireDriver.Start();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				throw;
			}
		}

		[TestCleanup]
		public void Clean()
		{
			try
			{
				if (mre != null)
				{
					mre.Dispose();
					mre = null;
				}
				//EventBus.Implementation.WireDriver.Stop();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				throw;
			}
		}

		[TestMethod]
		public void TestPublisherReceiverRMQTest()
		{
			SubscriberRMQ<TestEvent> subscriber = new SubscriberRMQ<TestEvent>();
			subscriber.HostName = "localhost";
			subscriber.Port = 5672;
			subscriber.VirtualHost = "/";
			subscriber.Protocol = "AMQP_0_8";
			subscriber.EventHandled += subscriber_EventHandled;
			subscriber.EventReceived += subscriber_EventReceived;
			subscriber.Subscribe();

			System.Threading.Tasks.Task.Factory.StartNew(() =>
			{
				PublisherRMQ<TestEvent> publisher = new PublisherRMQ<TestEvent>();
				publisher.HostName = "localhost";
				publisher.Port = 5672;
				publisher.VirtualHost = "/";
				publisher.Protocol = "AMQP_0_8";
				publisher.Publish(new TestEvent() { Data = DateTime.Now.ToString(), Processed = false });
			});

			if (!mre.WaitOne(TimeSpan.FromSeconds(10)))
			{
				Assert.Fail();
			}
			else
				Assert.IsTrue(Received);

			mre.Reset();

			if (!mre.WaitOne(TimeSpan.FromSeconds(10)))
			{
				Assert.Fail();
			}
			else
				Assert.IsTrue(Handled);

			subscriber.Unsubscribe();
		}

		void subscriber_EventReceived(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			Received = true;
			mre.Set();
		}

		void subscriber_EventHandled(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			Handled = true;
			mre.Set();
		}

		[TestMethod]
		public void PublishRMQTest()
		{
			PublisherRMQ<TestEvent> publisher = new PublisherRMQ<TestEvent>();
			publisher.HostName = "localhost";
			publisher.Port = 5672;
			publisher.VirtualHost = "/";
			publisher.Publish(new TestEvent() { Data = DateTime.Now.ToString(), Processed = false });
		}

		[TestMethod]
		public void SubscribeAndUnsubscribeTest()
		{
			SubscriberRMQ<TestEvent> subscriber = new SubscriberRMQ<TestEvent>();
			subscriber.HostName = "localhost";
			subscriber.Port = 5672;
			subscriber.VirtualHost = "/";
			subscriber.EventHandled += subscriber_EventHandled;
			subscriber.EventReceived += subscriber_EventReceived;
			subscriber.Subscribe();

			subscriber.Unsubscribe();
		}
	}
}
