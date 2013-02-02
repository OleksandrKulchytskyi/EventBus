using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.MessageQueue.Infrastructure
{
	public interface IValidator<T>
	{
		void SetObject(T data);
		bool Validate();
	}
}
