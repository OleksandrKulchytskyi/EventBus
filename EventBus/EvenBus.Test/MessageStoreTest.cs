using System;
using System.Linq;
using EventBus.Infrastructure;
using EventBus.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventBus.Test
{
	[TestClass]
	public class MessageStoreTest
	{
		[TestMethod]
		public void MessageStoreTestMethod()
		{
			IMessageStore ms = new MemoryMessageStore();

			Assert.IsTrue(ms != null);

			ms.SaveMessage(new Message1() { Data = "Hello world" }, DateTime.UtcNow.AddMinutes(30).Ticks);
			ms.SaveMessage(new Message2() { Data = "Hello world" }, DateTime.UtcNow.AddMinutes(30).Ticks);

			var msgs = ms.GetMessages(DateTime.UtcNow.Ticks);
			Assert.IsTrue(msgs.Count() == 2);
		}

		[TestMethod]
		public void MessageStoreTimeTestMethod()
		{
			IMessageStore ms = new MemoryMessageStore();

			Assert.IsTrue(ms != null);

			ms.SaveMessage(new Message1() { Data = "Hello world" }, DateTime.UtcNow.AddSeconds(10).Ticks);
			ms.SaveMessage(new Message2() { Data = "Hello world" }, DateTime.UtcNow.AddSeconds(5).Ticks);
			ms.SaveMessage(new Message1() { Data = "Hello world" }, DateTime.UtcNow.AddSeconds(6).Ticks);
			System.Threading.Thread.Sleep(TimeSpan.FromSeconds(8));

			var msgs = ms.GetMessages(DateTime.UtcNow.Ticks);
			Assert.IsTrue(msgs.Count() == 1);
		}
	}

	internal class Message1 : IMessage
	{
		public string Data { get; set; }
	}

	internal class Message2 : IMessage
	{
		public Message2()
		{
			Id = Guid.NewGuid();
		}
		public string Data { get; set; }
		public Guid Id { get; set; }
	}
}
