using System.Threading.Tasks;

namespace MessageBus.Core.Infrastructure
{
	public interface IEventHandler<TEvent>
	{
		void Handle(TEvent data);
	}

	public interface IRequestHandler<TRequest, TResponse>
	{
		Task<TResponse> HandleRequest(TRequest request);
	}

	public enum ProcessingMode
	{
		Synchronous = 0,
		Async = 1
	}

	public interface IMessageDispatcher
	{
		void Publish<TEvent>(TEvent data);

		Task<TResponse> Send<TRequest, TResponse>(TRequest request);
	}

	public interface IMessageBus : IMessageDispatcher
	{
		void RegisterEventHandler<TEvent>(IEventHandler<TEvent> data, ProcessingMode mode);

		void RegisterRequestHandler<TRequest, TResponse>(IRequestHandler<TRequest, TResponse> reqHandler, ProcessingMode mode);
	}

	public interface IMessageQueue
	{
		object Service { get; }

		void Publish<TEvent>(TEvent data, IMessageDispatcher dispatcher);

		Task<TResponse> Send<TRequest, TResponse>(TRequest request, IMessageDispatcher dispatcher);
	}

	public interface IMessageQueueCache
	{
		IMessageQueue GetOrAdd(object service, ProcessingMode mode);
	}
}