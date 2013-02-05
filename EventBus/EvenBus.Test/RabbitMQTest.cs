using EventBus.Infrastructure;
using EventBus.RabbitMQ.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;

namespace EvenBus.Test
{
	[TestClass]
	public class RabbitMQTest
	{
		private ManualResetEvent mre = null;
		private CountdownEvent countEvent = null;
		private bool Received = false;
		private bool Handled = false;
		SubscriberRMQ<TestEvent>[] subscribers = null;

		#region Init

		[TestInitialize]
		public void Init()
		{
			try
			{
				mre = new ManualResetEvent(false);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				throw;
			}

			countEvent = new CountdownEvent(10);
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
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
				throw;
			}

			if (countEvent != null)
				countEvent.Dispose();
		}

		#endregion Init

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

			if (!mre.WaitOne(TimeSpan.FromSeconds(13)))
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

		private void subscriber_EventReceived(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			Received = true;
			mre.Set();
			(sender as EventBus.Infrastructure.ISubscriber<TestEvent>).HandleEvent(e.Data);
		}

		private void subscriber_EventHandled(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
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
			publisher.Protocol = "AMQP_0_8";
			publisher.Publish(new TestEvent() { Data = DateTime.Now.ToString(), Processed = false });
		}

		[TestMethod]
		public void SubscribeAndUnsubscribeTest()
		{
			SubscriberRMQ<TestEvent> subscriber = new SubscriberRMQ<TestEvent>();
			subscriber.HostName = "localhost";
			subscriber.Port = 5672;
			subscriber.VirtualHost = "/";
			subscriber.Protocol = "AMQP_0_8";
			subscriber.EventHandled += subscriber_EventHandled;
			subscriber.EventReceived += subscriber_EventReceived;
			subscriber.Subscribe();

			subscriber.Unsubscribe();
		}

		[TestMethod]
		public void RMQCountTest()
		{
			subscribers = new SubscriberRMQ<TestEvent>[10];

			GenSubscribers(subscribers);

			System.Threading.Tasks.Task.Factory.StartNew(() =>
			{
				PublisherRMQ<TestEvent> publisher = new PublisherRMQ<TestEvent>();
				publisher.HostName = "localhost";
				publisher.Port = 5672;
				publisher.VirtualHost = "/";
				publisher.Protocol = "AMQP_0_8";
				publisher.Publish(new TestEvent() { Data = DateTime.Now.ToString(), Processed = false });
			});

			if (countEvent.Wait(TimeSpan.FromSeconds(6)) == false)
				Assert.Fail();

			Unsubscribe(subscribers);
			subscribers = null;
		}

		private void GenSubscribers(SubscriberRMQ<TestEvent>[] subscribers)
		{
			for (int i = 0; i < subscribers.Length; i++)
			{
				subscribers[i] = new SubscriberRMQ<TestEvent>();
				subscribers[i].HostName = "localhost";
				subscribers[i].Port = 5672;
				subscribers[i].VirtualHost = "/";
				subscribers[i].Protocol = "AMQP_0_8";
				subscribers[i].EventHandled += subscriber_EventHandled2;
				subscribers[i].EventReceived += subscriber_EventReceived2;
				subscribers[i].Subscribe();
			}
		}

		private void Unsubscribe(SubscriberRMQ<TestEvent>[] subscribers)
		{
			for (int i = 0; i < subscribers.Length; i++)
			{
				if (subscribers[i] != null)
					subscribers[i].Unsubscribe();
			}
		}

		private void subscriber_EventReceived2(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			Received = true;
			(sender as EventBus.Infrastructure.ISubscriber<TestEvent>).HandleEvent(e.Data);
		}

		private void subscriber_EventHandled2(object sender, EventBus.Infrastructure.BusEventArgs<TestEvent> e)
		{
			Handled = true;
			countEvent.Signal();
		}

	}
}