using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventBus.Infrastructure;
using System.Diagnostics;

namespace EventBus.Test
{
	[TestClass]
	public class ActionMessageBusTest
	{
		[TestMethod]
		public void TestActionMB()
		{
			Deferred.ActionMessageBus acMB = new Deferred.ActionMessageBus();

			var sub1 = new Subscr1();
			var sub2 = new Subscr2();
			var sub3 = new Subscr3();

			IDisposable disposer1 = acMB.Subscribe<Msg1>(sub1.Handle);
			IDisposable disposer2 = acMB.Subscribe<Msg1>(sub2.Handle2);
			IDisposable disposer3 = acMB.Subscribe<Msg1>(sub3.Handle3);

			acMB.Publish<Msg1>(new Msg1() { Data = "Hello world , peoples" });

			sub2 = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();


			acMB.Publish<Msg1>(new Msg1() { Data = "Second message" });

			disposer1.Dispose();
			disposer2.Dispose();
			disposer3.Dispose();

			acMB.Clear();
		}
	}

	public class Subscr1
	{
		public void Handle(Msg1 data)
		{
			Debug.WriteLine(data.Data, "Subscr1");
		}
	}

	public class Subscr2
	{
		public void Handle2(Msg1 data)
		{
			Debug.WriteLine(data.Data, "Subscr2");
		}
	}

	public class Subscr3
	{
		public void Handle3(Msg1 data)
		{
			Debug.WriteLine(data.Data, "Subscr3");
		}
	}

	public class Msg1 : IMessage
	{
		public string Data { get; set; }
	}
}
