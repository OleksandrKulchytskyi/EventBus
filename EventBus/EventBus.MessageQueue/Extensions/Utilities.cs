using EventBus.Infrastructure;
using EventBus.MessageQueue.Infrastructure;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.MessageQueue.Extensions
{
	public class Utilities
	{
		private static IValidator<string> _validator;
		static Utilities()
		{
			_validator = new Implementation.FakeValidator();
		}

		public static uint CreateIfNotExist(MessageQueueRequest request)
		{
			if (request == null)
				throw new ArgumentNullException("request");

			_validator.SetObject(request.QueuePath);
			if (!_validator.Validate())
				throw new InvalidOperationException(string.Format("Queue name is invalid, {0}", request.QueuePath));

			try
			{
				if (!System.Messaging.MessageQueue.Exists(request.QueuePath))
				{
					System.Messaging.MessageQueue.Create(request.QueuePath, request.IsTransactional);
					return 1;
				}
				return 0;
			}
			catch (Exception ex)
			{
				if (DefaultSingleton<ILog>.Instance != null)
					DefaultSingleton<ILog>.Instance.Error(string.Format("Error creating or accessing MSMQ {0}", request.QueuePath), ex);
				return 2;
			}
		}

		public static uint DeleteQueue(string queuePath)
		{
			if (string.IsNullOrEmpty(queuePath))
				throw new ArgumentNullException("queuePath");

			_validator.SetObject(queuePath);
			if (!_validator.Validate())
				throw new InvalidOperationException(string.Format("Queue name is invalid, {0}", queuePath));

			try
			{
				if (System.Messaging.MessageQueue.Exists(queuePath))
				{
					System.Messaging.MessageQueue.Delete(queuePath);
					return 1;
				}
				return 0;
			}
			catch (Exception ex)
			{
				if (DefaultSingleton<ILog>.Instance != null)
					DefaultSingleton<ILog>.Instance.Error(string.Format("Error deleteing or accessing MSMQ {0}", queuePath), ex);

				return 2;
			}
		}
	}
}
