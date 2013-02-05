using EventBus.Infrastructure;
using System;
using System.Collections.Generic;

namespace EventBus.Implementation
{
	public sealed class MessageBus : IMessageBus
	{
		private Dictionary<Type, List<Object>> _Subscribers = new Dictionary<Type, List<Object>>();

		public void Subscribe<TMessage>(Action<TMessage> handler)
		{
			if (_Subscribers.ContainsKey(typeof(TMessage)))
			{
				var handlers = _Subscribers[typeof(TMessage)];
				handlers.Add(handler);
			}
			else
			{
				var handlers = new List<Object>();
				handlers.Add(handler);
				_Subscribers[typeof(TMessage)] = handlers;
			}
		}

		public void Unsubscribe<TMessage>(Action<TMessage> handler)
		{
			if (_Subscribers.ContainsKey(typeof(TMessage)))
			{
				var handlers = _Subscribers[typeof(TMessage)];
				handlers.Remove(handler);

				if (handlers.Count == 0)
				{
					_Subscribers.Remove(typeof(TMessage));
				}
			}
		}

		public void Publish<TMessage>(TMessage message)
		{
			if (_Subscribers.ContainsKey(typeof(TMessage)))
			{
				var handlers = _Subscribers[typeof(TMessage)];
				foreach (Action<TMessage> handler in handlers)
				{
					handler.Invoke(message);
				}
			}
		}

		public void Publish(Object message)
		{
			var messageType = message.GetType();
			if (_Subscribers.ContainsKey(messageType))
			{
				var handlers = _Subscribers[messageType];
				foreach (var handler in handlers)
				{
					var actionType = handler.GetType();
					var invoke = actionType.GetMethod("Invoke", new Type[] { messageType });
					invoke.Invoke(handler, new Object[] { message });
				}
			}
		}
	}
}