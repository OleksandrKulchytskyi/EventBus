using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MessageBus.Core.Minimal
{
	public class MessageDispatcher : IMessageDispatcher
	{
		private List<KeyValuePair<Type, HandlerItem>> _handlers = new List<KeyValuePair<Type, HandlerItem>>();

		public void Register<T>(IConsumerOf<T> handler) where T : Message
		{
			Register(handler, HandlerMode.Explicit);
		}

		public void Register<T>(IConsumerOf<T> handler, HandlerMode mode) where T : Message
		{
			_handlers.Add(new KeyValuePair<Type, HandlerItem>(typeof(IConsumerOf<T>), new HandlerItem(typeof(T), handler, mode)));
		}

		public void Publish<T>(T message) where T : Message
		{
			_handlers.Where(handler => match<T>(handler)).ToList()
				.ForEach(handler => consume(handler.Value, message));
		}

		private bool match<T>(KeyValuePair<Type, HandlerItem> handler) where T : Message
		{
			if (handler.Value.Mode == HandlerMode.Explicit)
				return handler.Value.Type.Equals(typeof(T));
			else
				return handler.Value.Type.IsAssignableFrom(typeof(T));
		}

		private void consume<T>(HandlerItem handler, T message) where T : Message
		{
			if (handler.Mode == HandlerMode.Explicit)
				ThreadPool.QueueUserWorkItem((state) => { ((IConsumerOf<T>)handler.Handler).Consume(message); });
			else
				ThreadPool.QueueUserWorkItem((state) => { handleThroughReflection(message, handler.Handler); });
		}

		private void handleThroughReflection<T>(T message, object handler) where T : Message
		{
			try
			{
				handler.GetType().GetMethod("Consume", new Type[] { message.GetType() })
					.Invoke(handler, new object[] { message });
			}
			catch (Exception ex)
			{
				// Log whatever happened
			}
		}
	}

	class HandlerItem
	{
		public Type Type { get; private set; }
		public object Handler { get; private set; }
		public HandlerMode Mode { get; private set; }

		public HandlerItem(Type type, object handler, HandlerMode mode)
		{
			Type = type;
			Handler = handler;
			Mode = mode;
		}
	}

	public interface IMessageDispatcher
	{
		void Register<T>(IConsumerOf<T> handler) where T : Message;
		void Register<T>(IConsumerOf<T> handler, HandlerMode mode) where T : Message;
		void Publish<T>(T message) where T : Message;
	}

	public enum HandlerMode
	{
		Explicit,
		Hierarchical
	}

	public interface IConsumerOf<T> where T : Message
	{
		void Consume(T message);
	}

	public abstract class Message
	{
		public abstract string Description { get; }
	}
}
