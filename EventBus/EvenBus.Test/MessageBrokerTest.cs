using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventBus.MessageBus;
using EvenBus.Test;
using EventBus.Infrastructure;
using System.Threading;

namespace EventBus.Test
{
	[TestClass]
	public class MessageBrokerTest
	{
		CountdownEvent countEvent;

		[TestInitialize]
		public void Init()
		{
			countEvent = new CountdownEvent(3);
		}
		[TestCleanup]
		public void Destr()
		{
			if (countEvent != null)
			{
				countEvent.Dispose();
			}
		}

		[TestMethod]
		public void GroupTestMethod()
		{
			Assert.IsTrue(MessageBus.MessgaeBroker.Instance.GroupCount == 0);

			var pub = new TestEventPublisherMB();

			var sub1 = new TestEventSubscriberMB();
			var sub2 = new TestEventSubscriberMB();
			var sub3 = new TestEventSubscriberMB();

			sub1.EventReceived += sub1_EventReceived;
			sub1.EventHandled += sub1_EventHandled;
			sub1.Subscribe();

			sub2.EventReceived += sub1_EventReceived;
			sub2.EventHandled += sub1_EventHandled;
			sub2.Subscribe();

			sub3.EventReceived += sub1_EventReceived;
			sub3.EventHandled += sub1_EventHandled;
			sub3.Subscribe();

			bool thrown = false;
			try
			{
				sub3.Subscribe();
			}
			catch (InvalidOperationException) { thrown = true; }

			Assert.IsTrue(thrown);


			Assert.IsTrue(MessageBus.MessgaeBroker.Instance.GroupCount == 1);

			pub.Publish(new TestEvent() { Data = "Hello subscribers", Processed = false });


			if (!countEvent.Wait(TimeSpan.FromSeconds(1)))
				Assert.Fail();

			sub3.Dispose();

			countEvent.Reset(2);

			pub.Publish(new TestEvent() { Data = "Hello subscribers2", Processed = false });

			if (!countEvent.Wait(TimeSpan.FromSeconds(1)))
				Assert.Fail();

			MessageBus.MessgaeBroker.Instance.Drain();

			Assert.IsTrue(MessageBus.MessgaeBroker.Instance.GroupCount == 0);
		}

		void sub1_EventHandled(object sender, Infrastructure.BusEventArgs<TestEvent> e)
		{
			System.Diagnostics.Debug.WriteLine("Handled " + sender.GetType().Name);
			countEvent.Signal();
		}

		void sub1_EventReceived(object sender, Infrastructure.BusEventArgs<TestEvent> e)
		{
			System.Diagnostics.Debug.WriteLine("Received " + sender.GetType().Name);
			(sender as ISubscriber<TestEvent>).HandleEvent(e.Data);
		}
	}


	public class TestEventPublisherMB : MessageBus.PublisherMB<TestEvent>
	{
		public int PublishCount = 0;

		public override void Publish(TestEvent data)
		{
			base.Publish(data);
			System.Threading.Interlocked.Increment(ref PublishCount);
		}
	}

	public class TestEventSubscriberMB : MessageBus.SubscriberMB<TestEvent>
	{
		public int NotifiedCount = 0;


		public override void Notify(TestEvent msg)
		{
			base.Notify(msg);
			System.Threading.Interlocked.Increment(ref NotifiedCount);
		}

		public override void HandleEvent(TestEvent target)
		{
			base.HandleEvent(target);
			System.Diagnostics.Debug.WriteLine(target.Data);
		}

		protected override void OnEventReceived(TestEvent data)
		{
			base.OnEventReceived(data);
			System.Diagnostics.Debug.WriteLine("Event received.");
		}
	}
}
