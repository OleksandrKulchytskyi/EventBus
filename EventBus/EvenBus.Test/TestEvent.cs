using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.Test
{
	public class TestEvent
	{
		public string Data { get; set; }
		public bool Processed { get; set; }
	}
}
