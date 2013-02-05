using MessageBus.Core.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBus.Core.Implementation
{
	public class MessageBus : IMessageBus
	{
		private readonly MessageToQueuesMap messageToQueuesMap;

		/// <summary>
		/// Do not use unless you know what you are doing. Prefer to use MessageBusFactory.Create()
		/// - see comment on that class for details.
		/// </summary>
		/// <param name="messageToQueuesMap"></param>
		public MessageBus(MessageToQueuesMap messageToQueuesMap)
		{
			this.messageToQueuesMap = messageToQueuesMap;
		}

		public void RegisterEventHandler<TEvent>(IEventHandler<TEvent> handler, ProcessingMode messageProcessingMode)
		{
			messageToQueuesMap.AddMessageQueue<TEvent, Unit>(handler, messageProcessingMode);
		}

		public void RegisterRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> handler, ProcessingMode messageProcessingMode)
		{
			if (messageToQueuesMap.GetQueuesFor<TRequest, TResponse>().Any())
			{
				throw GetInvalidOperationException<TRequest, TResponse>("A handler has already been registered for unicast message signature '{0}'.");
			}

			messageToQueuesMap.AddMessageQueue<TRequest, TResponse>(handler, messageProcessingMode);
		}

		public void Publish<TEvent>(TEvent @event)
		{
			var handlersForThisEvent = messageToQueuesMap.GetQueuesFor<TEvent, Unit>();

			foreach (var handler in handlersForThisEvent)
			{
				var thisHandler = handler;

				thisHandler.Publish(@event, this);
			}
		}

		public Task<TResponse> Send<TRequest, TResponse>(TRequest request)
		{
			var handlersForThisRequest = messageToQueuesMap.GetQueuesFor<TRequest, TResponse>().ToList();

			switch (handlersForThisRequest.Count)
			{
				case 0:
					throw GetInvalidOperationException<TRequest, TResponse>("No handler has been registered for unicast message signature '{0}'.");
				case 1:
					return handlersForThisRequest.First().Send<TRequest, TResponse>(request, this);
				default:
					// This condition should never occur because of the guard clause in RegisterRequestHandler<TRequest, TResponse>(...)
					throw GetInvalidOperationException<TRequest, TResponse>("More than one handler has been registered for unicast message signature '{0}'.");
			}
		}

		private static InvalidOperationException GetInvalidOperationException<TRequest, TResponse>(string message)
		{
			return new InvalidOperationException(string.Format(message, MessageToQueuesMap.GetMessageSignature<TRequest, TResponse>()));
		}
	}
}