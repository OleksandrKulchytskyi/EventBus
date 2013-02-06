using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.MessageBus
{
	internal interface INotify<TEvent>
	{
		void Notify(TEvent msg);
	}
}
