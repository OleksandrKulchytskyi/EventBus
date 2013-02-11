using EventBus.Infrastructure;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventBus.Redis.Extension
{
	public enum RedisErrorCodes
	{
		None = 1,
		SessionIsNotFound = 1,
		SessionIsBusy = 2
	}

	public abstract class RedisObjectsContainer<ObjectType, DerrivingType> : SingletonBase<DerrivingType>
		where ObjectType : class, IUniqueLockable, new()
		where DerrivingType : class
	{
		private readonly ReaderWriterLockSlim _ObjectsLocker = new ReaderWriterLockSlim();
		private IDictionary<string, ObjectType> _Objects = new Dictionary<string, ObjectType>();
		public readonly string TypeName;

		public bool AutoLoadNewObjects { get; set; }

		private RedisListener Listener = null;

		public class ObjectEventArgs : EventArgs
		{
			public ObjectEventArgs(string key)
			{
				ObjectKey = key;
				ObjectId = int.Parse(key.Remove(0, key.IndexOf(':') + 1));
			}

			public string ObjectKey { get; set; }

			public long ObjectId { get; set; }
		}

		public event EventHandler<ObjectEventArgs> OnAdded;

		public event EventHandler<ObjectEventArgs> OnChanged;

		public event EventHandler<ObjectEventArgs> OnDeleted;

		private void OnAddedHandler(object sender, ObjectEventArgs args)
		{
			if (AutoLoadNewObjects)
			{
				PullFromServer(args.ObjectKey);
			}
		}

		private void OnChangedHandler(object sender, ObjectEventArgs args)
		{
			if (_Objects.ContainsKey(args.ObjectKey))
			{
				PullFromServer(args.ObjectKey);
			}
		}

		private void OnDeletedHandler(object sender, ObjectEventArgs args)
		{
			if (_Objects.ContainsKey(args.ObjectKey))
			{
				_Objects.Remove(args.ObjectKey);
			}
		}

		protected RedisObjectsContainer()
		{
			TypeName = typeof(ObjectType).Name;
			OnAdded += new EventHandler<ObjectEventArgs>(OnAddedHandler);
			OnChanged += new EventHandler<ObjectEventArgs>(OnChangedHandler);
			OnDeleted += new EventHandler<ObjectEventArgs>(OnDeletedHandler);
			Listener = new RedisListener(this);
		}

		public void Start()
		{
			Task.Factory.StartNew(() => Listener.StartListen(), TaskCreationOptions.LongRunning);
		}

		public void Stop()
		{
			Listener.StopListen();
		}

		public void LoadAllObjects()
		{
			using (var client = RedisClientsManager.Instance.GetClient())
			{
				var allkeys = client.SearchKeys(TypeName + ":*");
				if (allkeys.Count > 0)
				{
					_Objects = client.GetAll<ObjectType>(allkeys);
				}
			}
		}

		public Dictionary<long, ObjectType> GetAllLocalObjects()
		{
			var dic = new Dictionary<long, ObjectType>();
			foreach (var kvp in _Objects)
			{
				dic.Add(kvp.Value.Id, kvp.Value);
			}
			return dic;
		}

		public void UnloadAllObjects()
		{
			_Objects.Clear();
		}

		public ObjectType Get(long objectId)
		{
			try
			{
				_ObjectsLocker.EnterReadLock();
				string key = TypeName + ":" + objectId;

				if (_Objects.ContainsKey(key))
				{
					return _Objects[key];
				}

				//Checking cache
				return PullFromServer(key);
			}
			finally
			{
				_ObjectsLocker.ExitReadLock();
			}
		}

		private ObjectType PullFromServer(string key)
		{
			ObjectType obj;
			using (var client = RedisClientsManager.Instance.GetClient())
			{
				obj = client.Get<ObjectType>(key);
			}
			if (obj != null)
			{
				_Objects[key] = obj;
			}
			return obj;
		}

		public void Set(ObjectType value)
		{
			string key = TypeName + ":" + value.Id;
			if (Get(value.Id) == null)
			{
				//TODO: insert logger messages here
				//Logger.Instance.Error("Object ({0}) not found! use Add() instead", key);
				return;
			}
			_Objects[key] = value;
			using (var client = RedisClientsManager.Instance.GetClient())
			{
				client.Set(key, value);
				Listener.PublishChanged(client, key);
			}
		}

		public ObjectType this[long objectId]
		{
			get
			{
				return Get(objectId);
			}
		}

		public long Add(ObjectType newObject)
		{
			try
			{
				_ObjectsLocker.EnterWriteLock();
				long objectId;
				string key;
				using (var client = RedisClientsManager.Instance.GetClient())
				using (var lockObj = client.AcquireLock(TypeName + "-Lock"))
				{
					objectId = client.IncrementValue(TypeName + "-MaxId");
					newObject.Id = objectId;
					key = TypeName + ":" + objectId;
					client.Add(key, newObject);
					Listener.PublishAdded(client, key);
				}

				_Objects.Add(key, newObject);

				return objectId;
			}
			catch
			{
				throw;
			}
			finally
			{
				_ObjectsLocker.ExitWriteLock();
			}
		}

		public bool Remove(long objectId)
		{
			try
			{
				_ObjectsLocker.EnterWriteLock();
				string key = TypeName + ":" + objectId;

				using (var client = RedisClientsManager.Instance.GetClient())
				{
					client.Remove(key);
					Listener.PublishRemoved(client, key);
				}
				return _Objects.Remove(key);
			}
			finally
			{
				_ObjectsLocker.ExitWriteLock();
			}
		}

		public ObjectType GetAndLock(long objectId, ref RedisErrorCodes errorCode)
		{
			string key = TypeName + ":" + objectId;
			ObjectType obj = Get(objectId);
			if (obj != null)
			{
				if (obj.Lock())
				{
					return PullFromServer(key);
				}

				errorCode = RedisErrorCodes.SessionIsBusy;
				return null;
			}
			else
			{
				errorCode = RedisErrorCodes.SessionIsNotFound;
				return null;
			}
		}

		private class RedisListener
		{
			private readonly string AddedChannel;
			private readonly string ChangedChannel;
			private readonly string DeletedChannel;
			private readonly RedisObjectsContainer<ObjectType, DerrivingType> _parent;
			private const string Heartbeat = "Server/Heartbeat";
			private bool _closing = false;
			private IRedisSubscription _subscription;

			public RedisListener(RedisObjectsContainer<ObjectType, DerrivingType> parent)
			{
				_parent = parent;
				AddedChannel = _parent.TypeName + "/Added";
				ChangedChannel = _parent.TypeName + "/Changed";
				DeletedChannel = _parent.TypeName + "/Deleted";
			}

			public void StartListen()
			{
				using (var redisConsumer = RedisClientsManager.Instance.GetClient())
				{
					_subscription = redisConsumer.CreateSubscription();
					_subscription.OnMessage = OnMessage;
					_subscription.OnSubscribe = OnSubscribe;
					_subscription.OnUnSubscribe = OnUnSubscribe;
					_subscription.SubscribeToChannels(ChangedChannel, AddedChannel, DeletedChannel, Heartbeat);
				}
			}

			public void StopListen()
			{
				using (var client = RedisClientsManager.Instance.GetClient())
				{
					_closing = true;
					client.PublishMessage(Heartbeat, Heartbeat);
				}
			}

			private void OnMessage(string channel, string message)
			{
				if (_closing)
				{
					_subscription.UnSubscribeFromAllChannels();
				}
				if (channel == Heartbeat)
				{
					return;
				}

				var args = new ObjectEventArgs(message);
				if (channel == AddedChannel)
				{
					_parent.OnAdded(this._parent, args);
				}
				else if (channel == ChangedChannel)
				{
					_parent.OnChanged(this._parent, args);
				}
				else if (channel == DeletedChannel)
				{
					_parent.OnDeleted(this._parent, args);
				}
				else
				{
					//TODO: Inject logger here
					//Logger.Instance.Error("Missing handler for redis channel " + channel);
				}
			}

			private void OnUnSubscribe(string channel)
			{
				//
			}

			private void OnSubscribe(string channel)
			{
				//
			}

			public void PublishChanged(IRedisClient client, string key)
			{
				client.PublishMessage(ChangedChannel, key);
			}

			public void PublishAdded(IRedisClient client, string key)
			{
				client.PublishMessage(AddedChannel, key);
			}

			public void PublishRemoved(IRedisClient client, string key)
			{
				client.PublishMessage(DeletedChannel, key);
			}
		}
	}
}