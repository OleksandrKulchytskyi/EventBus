using MessageBus.Core.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageBus.Core.Implementation
{
	public class MessageQueueCache : IMessageQueueCache
	{
		private readonly ConcurrentDictionary<Tuple<object, ProcessingMode>, IMessageQueue> _queues = new ConcurrentDictionary<Tuple<object, ProcessingMode>, IMessageQueue>();
		
		public Infrastructure.IMessageQueue GetOrAdd(object service, Infrastructure.ProcessingMode mode)
		{
			var key = new Tuple<object, ProcessingMode>(service, mode);

			var queue = _queues.GetOrAdd(key, CreateQueue(key));
			return queue;
		}

		private static IMessageQueue CreateQueue(Tuple<object, ProcessingMode> key)
		{
			return key.Item2 == ProcessingMode.Synchronous ?	new SyncMessageQueue(key.Item1) :new  AsyncMessageQueue(key.Item1) as IMessageQueue;
		}
	}
}
