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
			ContainerRedis.Instance.AutoLoadNewObjects = true;
			ContainerRedis.Instance.Start();
		}

		[TestCleanup]
		public void Clean()
		{
			ContainerRedis.Instance.Stop();
		}

		[TestMethod]
		public void TestMethod()
		{
			try
			{
				ContainerRedis.Instance.LoadAllObjects();
				if (ContainerRedis.Instance.GetAllLocalObjects().Count > 0)
				{
					foreach (var item in ContainerRedis.Instance.GetAllLocalObjects())
					{
						ContainerRedis.Instance.Remove(item.Key);
					}
				}

				var coll = ContainerRedis.Instance.GetAllLocalObjects();

				Assert.IsTrue(coll.Count == 0);
				var obj = new LockableMessage() { Content = "Hello world", Id = 12 };
				ContainerRedis.Instance.Add(obj);
				coll = ContainerRedis.Instance.GetAllLocalObjects();
				Assert.IsTrue(coll.Count == 1);

				RedisErrorCodes error = RedisErrorCodes.None;
				var lockableObj = ContainerRedis.Instance.GetAndLock(obj.Id, ref error);
				if (error == RedisErrorCodes.None)
				{
					obj.Content = "Changed";
					obj.Unlock();
				}

				ContainerRedis.Instance.Remove(obj.Id);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}
		}
	}

	class LockableMessage : IUniqueLockable
	{
		public long Id { get; set; }
		public string Content { get; set; }
		private IDisposable _Locker;
		private const int LOCKER_TIMEOUT = 4000;
		private const string LOCKER_FORMAT = "lock.{0}:{1}";

		public bool Lock()
		{
			if (_Locker != null)
				return false;

			try
			{
				using (var redis = EventBus.Redis.Extension.RedisClientsManager.Instance.GetClient())
				{
					_Locker = redis.AcquireLock(string.Format(LOCKER_FORMAT, this.GetType().Name, Id), TimeSpan.FromMilliseconds(LOCKER_TIMEOUT));
					return true;
				}
			}
			catch (TimeoutException tex)
			{
				System.Diagnostics.Debug.WriteLine(tex.Message);
				return false;
			}
		}

		public void Unlock()
		{
			System.Threading.ThreadPool.QueueUserWorkItem(state =>
			{
				if (_Locker != null)
				{
					_Locker.Dispose();
					_Locker = null;
				}
			}, null);
		}
	}

	class ContainerRedis : RedisObjectsContainer<LockableMessage, ContainerRedis>
	{
		private ContainerRedis() { }
	}
}
