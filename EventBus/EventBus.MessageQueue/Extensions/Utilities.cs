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
		public static void CreateIfNotExist(MessageQueueRequest request)
		{
			try
			{
				if (!System.Messaging.MessageQueue.Exists(request.QueuePath))
					System.Messaging.MessageQueue.Create(request.QueuePath, request.IsTransactional);
			}
			catch (Exception x)
			{
				DefaultSingleton<ILog>.Instance.Error(string.Format("Error creating or accessing MSMQ {0}", request.QueuePath), x);
			}
		}
	}
}
