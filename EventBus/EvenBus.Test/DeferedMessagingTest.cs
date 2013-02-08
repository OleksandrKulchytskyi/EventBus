using EventBus.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;

namespace EventBus.Test
{
	[TestClass]
	public class DeferedMessagingTest
	{
		private CountdownEvent countEvent;

		[TestInitialize]
		public void Init()
		{
			countEvent = new CountdownEvent(1);
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
		public void TestDeffered()
		{
			Publicator p1 = new Publicator();

			Subscriber1 sub = new Subscriber1();
			sub.EventReceived += sub_EventReceived;
			sub.EventHandled += sub_EventHandled;
			sub.Subscribe();

			ThreadPool.QueueUserWorkItem(state =>
			{
				Thread.Sleep(TimeSpan.FromSeconds(1));
				(state as IPublisher<DefferMessage2>).Publish(new DefferMessage2() { Content = "Hello world" });
			}, p1);

			if (!countEvent.Wait(TimeSpan.FromMinutes(3)))
				Assert.Fail();

			sub.Dispose();
		}

		private void sub_EventReceived(object sender, BusEventArgs<DefferMessage2> e)
		{
			(sender as ISubscriber<DefferMessage2>).HandleEvent(e.Data);
		}

		private void sub_EventHandled(object sender, BusEventArgs<DefferMessage2> e)
		{
			countEvent.Signal();
		}
	}

	public class DefferMessage1 : IMessage
	{
		public string Data { get; set; }
	}

	public class DefferMessage2 : IMessage
	{
		public DefferMessage2()
		{
			Id = Guid.NewGuid();
			Inner = 1;
		}

		public Guid Id { get; set; }

		public string Content { get; set; }

		public int Inner { get; set; }
	}

	internal class Publicator : Deffered.DelayedPublisher<DefferMessage2>
	{
		public override void Publish(DefferMessage2 data)
		{
			base.Publish(data);
			Debug.WriteLine("Published");
		}
	}

	internal class Subscriber1 : Deffered.DelayedSubscriber<DefferMessage2>
	{
		public override void Unsubscribe()
		{
			base.Unsubscribe();
			Debug.WriteLine("Unsubscribed");
		}

		public override void Subscribe()
		{
			base.Subscribe();
			Debug.WriteLine("Subscribed");
		}
	}
}