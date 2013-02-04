using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventBus.Infrastructure;
using EventBus.Implementation;
using System.Reflection;

namespace EvenBus.Test
{
	[TestClass]
	public class TestSubscriber
	{
		public class ConcreteSubscriberUns : ISubscriber<TestEvent>, IUnsubscribe
		{
			internal static bool UnsubscribeCalled = false;

			public void Subscribe()
			{
			}

			public Type GetEventType()
			{
				return typeof(object);
			}

			public event EventHandler<BusEventArgs<TestEvent>> EventHandled;

			public void HandleEvent(TestEvent target)
			{
			}

			public void Unsubscribe()
			{
				UnsubscribeCalled = true;
			}

			public event EventHandler<BusEventArgs<TestEvent>> EventReceived;
		}

		[TestMethod]
		public void TriggerUnsubscribeWhenBusStoppedTest()
		{
			Subscribers.Current.AssembliesToSearch.Add(Assembly.GetExecutingAssembly());
			WireDriver.Start();

			Assert.IsFalse(ConcreteSubscriberUns.UnsubscribeCalled);

			WireDriver.Stop();
			Assert.IsTrue(ConcreteSubscriberUns.UnsubscribeCalled);
		}

		[TestMethod]
		public void TriggerUnsubscribeWhenBusDisposedTest()
		{
			Subscribers.Current.AssembliesToSearch.Add(Assembly.GetExecutingAssembly());
			using (WireDriver.Start())
			{
				Assert.IsFalse(ConcreteSubscriberUns.UnsubscribeCalled);
			}
			Assert.IsTrue(ConcreteSubscriberUns.UnsubscribeCalled);
		}
	}
}
