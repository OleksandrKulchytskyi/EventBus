using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EventBus.LINQ;

namespace EventBus.Test
{
	[TestClass]
	public class LingTest
	{
		[TestMethod]
		public void TestBatchMethod()
		{
			var sequencw = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
			var batches =sequencw.Batch(2);
			Assert.IsTrue(batches.Count() == 5);
		}
	}
}
