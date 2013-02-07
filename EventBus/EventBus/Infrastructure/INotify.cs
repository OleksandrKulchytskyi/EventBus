using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.Infrastructure
{
	internal interface INotify<TEvent>
	{
		void Notify(TEvent msg);
	}
}
