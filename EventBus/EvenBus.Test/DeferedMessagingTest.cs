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

			if (!countEvent.Wait(TimeSpan.FromMinutes(1)))
				Assert.Fail();

			sub.Dispose();
		}

		[TestMethod]
		public void TestDeffered2()
		{
			countEvent.Reset(2);

			Publicator p1 = new Publicator();
			Publicator2 p2 = new Publicator2();

			p1.Publish(new DefferMessage2() { Content = "Hello world" });
			p2.Publish(new DefferMessage1() { Data = "Hello world .........1221." });

			Subscriber1 sub = new Subscriber1();
			sub.EventReceived += sub_EventReceived;
			sub.EventHandled += sub_EventHandled;

			Subscriber2 sub2 = new Subscriber2();
			sub2.EventReceived += sub_EventReceived2;
			sub2.EventHandled += sub_EventHandled2;

			ThreadPool.QueueUserWorkItem(state =>
			{
				Thread.Sleep(TimeSpan.FromSeconds(3));
				sub2.Subscribe();
			}, null);

			ThreadPool.QueueUserWorkItem(state =>
			{
				Thread.Sleep(TimeSpan.FromSeconds(5));
				sub.Subscribe();
			}, null);

			if (!countEvent.Wait(TimeSpan.FromMinutes(2)))
				Assert.Fail();

			sub.Dispose();
			sub2.Dispose();
		}

		private void sub_EventReceived(object sender, BusEventArgs<DefferMessage2> e)
		{
			Debug.WriteLine("Received " + sender.ToString());
			(sender as ISubscriber<DefferMessage2>).HandleEvent(e.Data);
		}

		private void sub_EventHandled(object sender, BusEventArgs<DefferMessage2> e)
		{
			Debug.WriteLine("Handled " + sender.ToString());
			countEvent.Signal();
		}

		private void sub_EventReceived2(object sender, BusEventArgs<DefferMessage1> e)
		{
			Debug.WriteLine("Received " + sender.ToString());
			(sender as ISubscriber<DefferMessage1>).HandleEvent(e.Data);
		}

		private void sub_EventHandled2(object sender, BusEventArgs<DefferMessage1> e)
		{
			Debug.WriteLine("Handled " + sender.ToString());
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

	internal class Publicator2 : Deffered.DelayedPublisher<DefferMessage1>
	{
		public override void Publish(DefferMessage1 data)
		{
			base.Publish(data);
			Debug.WriteLine("Publicator2 published");
		}
	}

	internal class Subscriber1 : Deffered.DelayedSubscriber<DefferMessage2>
	{
		public override void Unsubscribe()
		{
			base.Unsubscribe();
			Debug.WriteLine("Subscriber1 unsubscribed");
		}

		public override void Subscribe()
		{
			Debug.WriteLine("Subscriber1 subscribe has been invoked");
			base.Subscribe();
		}
	}

	internal class Subscriber2 : Deffered.DelayedSubscriber<DefferMessage1>
	{
		public override void Unsubscribe()
		{
			Debug.WriteLine("Subscriber2 subscribe has been invoked");
			base.Unsubscribe();
		}

		public override void Subscribe()
		{
			base.Subscribe();
			Debug.WriteLine("Subscriber2 subscribed");
		}
	}
}