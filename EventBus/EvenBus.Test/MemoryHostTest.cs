using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventBus.Infrastructure;
using EventBus.Implementation;
using EventBus.Extensions;
using System.Threading;
using EventBus.Hosting;

namespace EvenBus.Test
{
	[TestClass]
	public class MemoryHostTest
	{
		ICreator Creator { get; set; }
		System.Threading.ManualResetEvent WaitHandle { get; set; }

		private void Configure()
		{
			this.Creator = new UnityCreator();
		}

		[TestMethod]
		public void TestMethod1()
		{
			Configure();

			var targetEvent = new TestEvent
			{
				Data = string.Format("Testing_{0}", Environment.TickCount)
			};

			this.WaitHandle = new ManualResetEvent(false);

			var completedHandler = new SubscriberCompletedExecutionHandler(
					delegate(ISubscriber sub, object target)
					{
						var testingEvent = target as TestEvent;
						Assert.IsNotNull(testingEvent);
						this.WaitHandle.Set();
					});

			Subscribers.Current
					.WithCreator(this.Creator)
					.WithAssembly<TestEvent>()
					.Subscribe<TestEvent>()
					.WithSubscriberCompletedAction(completedHandler);

			Publishers.Current
					.WithCreator(this.Creator)
					.WithAssembly<TestEvent>()
					.Publish<TestEvent>(targetEvent);

			this.WaitHandle.WaitOne();

			Assert.IsTrue(targetEvent.Processed, "Event wasn't handled during the unit test");
		}
	}

	public class EventPublisherConcrete : MemoryPublisher<TestEvent>
	{
		public override void Publish(TestEvent eventToBePublished)
		{
			Console.WriteLine("Publishing TestEvent '{0}'", eventToBePublished.Data);
			base.Publish(eventToBePublished);
		}
	}

	public class EventSubscriberConcrete : MemorySubscriber<TestEvent>
	{
		public override void HandleEvent(TestEvent target)
		{
			Console.WriteLine("Handling TestEvent '{0}'", target.Data);
			target.Processed = true;
			base.HandleEvent(target);
		}
	}
}
