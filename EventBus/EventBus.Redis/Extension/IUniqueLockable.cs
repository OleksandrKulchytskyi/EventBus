using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.Redis.Extension
{
	public interface IUniqueLockable
	{
		long Id { get; set; }
		bool Lock();

		void Unlock();
	}
}
