using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventBus.Implementation;

namespace EvenBus.Test
{
	[TestClass]
	public class WireTest
	{
		[TestMethod]
		public void BusDriverRaisesEventOnStop()
		{
			bool invoked = false;
			WireDriver.Stopping += (s, a) => invoked = true;
			WireDriver.Start();
			Assert.IsFalse(invoked);
			WireDriver.Stop();
			Assert.IsTrue(invoked);
		}

		[TestMethod]
		public void BusDriverRaisesEventOnDispose()
		{
			bool invoked = false;
			WireDriver.Stopping += (s, a) => invoked = true;
			using (WireDriver.Start())
			{
				Assert.IsFalse(invoked);
			}
			Assert.IsTrue(invoked);
		}
	}
}
