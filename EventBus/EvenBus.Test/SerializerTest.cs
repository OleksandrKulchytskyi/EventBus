using EventBus.Infrastructure;
using EventBus.Implementation;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Diagnostics;

namespace EventBus.Test
{
	[TestClass]
	public class SerializerTest
	{
		[TestMethod]
		public void TestMethod()
		{
			int msgCount = 5000;
			IMessageSerializer json = new JsonMessageSerializer();
			IMessageSerializer bson = new BsonMessageSerializer();
			long elapsed1;
			long elapsed2;

			Stopwatch sw = new Stopwatch();
			sw.Start();
			for (int i = 0; i < msgCount; i++)
			{
				IMessage msg = new MessageSerDes() { Content = "Hello world people" };
				using (MemoryStream ms = new MemoryStream())
				{
					json.Serialize(msg, ms);

					if (ms.CanSeek)
						ms.Seek(0, SeekOrigin.Begin);

					var des = json.Deserialize(ms);
					if ((des as MessageSerDes).Content != (msg as MessageSerDes).Content ||
						(des as MessageSerDes).Id != (msg as MessageSerDes).Id ||
						(des as MessageSerDes).Age != (msg as MessageSerDes).Age)
						Assert.Fail();
				}
				msg = null;
			}

			sw.Stop();
			elapsed1 = sw.ElapsedMilliseconds;
			sw.Reset();

			sw.Start();
			for (int i = 0; i < msgCount; i++)
			{
				IMessage msg = new MessageSerDes() { Content = "Hello world people" };
				using (MemoryStream ms = new MemoryStream())
				{
					bson.Serialize(msg, ms);

					if (ms.CanSeek)
						ms.Seek(0, SeekOrigin.Begin);

					var des = bson.Deserialize(ms);
					if ((des as MessageSerDes).Content != (msg as MessageSerDes).Content ||
						(des as MessageSerDes).Id != (msg as MessageSerDes).Id ||
						(des as MessageSerDes).Age != (msg as MessageSerDes).Age)
						Assert.Fail();
				}
				msg = null;
			}
			sw.Stop();
			elapsed2 = sw.ElapsedMilliseconds;

			Assert.AreNotEqual(elapsed1, elapsed2);
		}
	}

	class MessageSerDes : IMessage
	{
		public MessageSerDes()
		{
			Id = Guid.NewGuid();
			Age = 34;
			Content = string.Empty;
		}

		public Guid Id { get; set; }
		public int Age { get; set; }
		public string Content { get; set; }
		public bool Active { get; set; }
	}
}
