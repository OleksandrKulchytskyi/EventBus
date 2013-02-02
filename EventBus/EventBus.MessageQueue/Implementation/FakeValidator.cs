using EventBus.MessageQueue.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.MessageQueue.Implementation
{
	public class FakeValidator : IValidator<string>
	{
		public void SetObject(string data)
		{
		}

		public bool Validate()
		{
			return true;
		}
	}
}
