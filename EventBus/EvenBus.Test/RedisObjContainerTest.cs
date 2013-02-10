using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventBus.Test
{
	using EventBus.Redis.Extension;

	[TestClass]
	public class RedisObjContainerTest
	{
		[TestInitialize]
		public void Init()
		{
			Container.Instance.AutoLoadNewObjects = true;
			Container.Instance.Start();
		}

		[TestCleanup]
		public void Clean()
		{
			Container.Instance.Stop();
		}

		[TestMethod]
		public void TestMethod()
		{
			var coll = Container.Instance.GetAllLocalObjects();
			Assert.IsTrue(coll.Count == 0);
			Container.Instance.Add(new LockableMessage() { Content = "Hello world", Id = 12 });
			coll = Container.Instance.GetAllLocalObjects();
			Assert.IsTrue(coll.Count == 1);
		}
	}

	class LockableMessage : IUniqueLockable
	{
		public long Id { get; set; }
		public string Content { get; set; }

		public bool Lock()
		{
			throw new NotImplementedException();
		}

		public void Unlock()
		{
			throw new NotImplementedException();
		}
	}

	class Container : RedisObjectsContainer<LockableMessage, Container>
	{
		private Container() { }
	}
}
