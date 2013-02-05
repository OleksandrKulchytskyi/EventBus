using MessageBus.Core.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageBus.Core.Implementation
{
	/// <summary>
	/// Represents a map from each message type to the services that implements the message handlers. 
	/// Each service is placed behind an IMessageQueue to ensure correct message processing 
	/// (sequential processing if the handler is non-threadsafe; otherwise concurrent processing).
	/// </summary>
	public class MessageToQueuesMap
	{
		private readonly IMessageQueueCache queueCache;

		private readonly ConcurrentDictionary<Type, ConcurrentBag<IMessageQueue>> messageQueues = new ConcurrentDictionary<Type, ConcurrentBag<IMessageQueue>>();

		private readonly IEnumerable<IMessageQueue> noMatchingQueues = Enumerable.Empty<IMessageQueue>();

		public MessageToQueuesMap(IMessageQueueCache queueCache)
		{
			this.queueCache = queueCache;
		}

		public void AddMessageQueue<TRequest, TResponse>(object service, ProcessingMode messageProcessingMode)
		{
			var messageQueue = queueCache.GetOrAdd(service, messageProcessingMode);

			var messageSignature = GetMessageSignature<TRequest, TResponse>();

			var handlersForThisMessageSignature = messageQueues.GetOrAdd(messageSignature, t => new ConcurrentBag<IMessageQueue>());

			if (handlersForThisMessageSignature.Contains(messageQueue))
			{
				throw new InvalidOperationException(string.Format("This service has already been registered as a handler for message type {0}", messageSignature));
			}

			handlersForThisMessageSignature.Add(messageQueue);
		}

		public IEnumerable<IMessageQueue> GetQueuesFor<TRequest, TResponse>()
		{
			ConcurrentBag<IMessageQueue> handlersForThisMessageSignature;
			if (messageQueues.TryGetValue(GetMessageSignature<TRequest, TResponse>(), out handlersForThisMessageSignature))
			{
				return handlersForThisMessageSignature;
			}

			return noMatchingQueues;
		}

		public static Type GetMessageSignature<TRequest, TResponse>()
		{
			return typeof(Func<TRequest, TResponse>);
		}
	}
}
