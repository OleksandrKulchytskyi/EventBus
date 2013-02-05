using MessageBus.Core.Infrastructure;
using System;

namespace MessageBus.Core.Implementation
{
	public class AsyncMessageQueue : IMessageQueue
	{
		public AsyncMessageQueue(object service)
		{
			if (service == null)
				throw new ArgumentNullException("service");

			Service = service;
		}

		public object Service
		{
			get;
			private set;
		}

		public void Publish<TEvent>(TEvent data, IMessageDispatcher dispatcher)
		{
			throw new NotImplementedException();
		}

		public System.Threading.Tasks.Task<TResponse> Send<TRequest, TResponse>(TRequest request, IMessageDispatcher dispatcher)
		{
			throw new NotImplementedException();
		}
	}
}