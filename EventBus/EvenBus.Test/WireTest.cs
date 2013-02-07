using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventBus.Implementation;

namespace EventBus.Test
{
	[TestClass]
	public class WireTest
	{
		bool invoked = false;
		[TestMethod]
		public void BusDriverRaisesEventOnStop()
		{
			WireDriver.Stopping += WireDriver_Stopping;
			WireDriver.Start();
			Assert.IsFalse(invoked);
			WireDriver.Stop();
			WireDriver.Stopping -= WireDriver_Stopping;

			Assert.IsTrue(invoked);
		}

		void WireDriver_Stopping(object sender, EventArgs e)
		{
			invoked = true;
		}

		[TestMethod]
		public void BusDriverRaisesEventOnDispose()
		{
			WireDriver.Stopping += WireDriver_Stopping;
			using (WireDriver.Start())
			{
				Assert.IsFalse(invoked);
			}
			WireDriver.Stopping -= WireDriver_Stopping;
			Assert.IsTrue(invoked);
		}
	}
}
