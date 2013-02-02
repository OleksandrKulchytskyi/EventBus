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
			public void Subscribe()
			{
			}

			public Type GetEventType()
			{
				return typeof(object);
			}

			public event EventHandler<BusEventArgs<TestEvent>> EventHandled;
			public void Handle(TestEvent target)
			{
			}

			public void Unsubscribe()
			{
				Called = true;
			}

			internal static bool Called = false;

			public event EventHandler<BusEventArgs<TestEvent>> EventReceived;
		}

		[TestMethod]
		public void TriggerUnsubscribeWhenBusStoppedTest()
		{
			Subscribers.Current.AssembliesToSearch.Add(Assembly.GetExecutingAssembly());
			WireDriver.Start();

			Assert.IsFalse(ConcreteSubscriberUns.Called);

			WireDriver.Stop();
			Assert.IsTrue(ConcreteSubscriberUns.Called);
		}

		[TestMethod]
		public void TriggerUnsubscribeWhenBusDisposedTest()
		{
			Subscribers.Current.AssembliesToSearch.Add(Assembly.GetExecutingAssembly());
			using (WireDriver.Start())
			{
				Assert.IsFalse(ConcreteSubscriberUns.Called);
			}
			Assert.IsTrue(ConcreteSubscriberUns.Called);
		}
	}
}
