using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.MessageQueue.Infrastructure
{
	public class MessageQueueRequest
	{
		public bool IsTransactional { get; set; }
		public string QueuePath { get; set; }
	}
}
