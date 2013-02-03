using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleRabbitMQ
{
	public class Message1
	{
		public Message1()
		{
			StartInstant = DateTime.Now;
		}

		public DateTime StartInstant;
	}

	[Serializable]
	public class Message2
	{
		public Message2()
		{
			StartInstant = DateTime.Now;
		}

		public DateTime StartInstant;
	}

	[Serializable]
	public class Message3
	{
		public Message3()
		{
			StartInstant = DateTime.Now;
		}

		public DateTime StartInstant;
	}
}
