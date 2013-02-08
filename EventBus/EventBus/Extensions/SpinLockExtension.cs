using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EventBus.Extensions
{
	public static class SpinLockExtension
	{
		public static void SafeWork(this SpinLock spinLock, Action action)
		{
			bool lockTaken = false;
			try
			{
				spinLock.Enter(ref lockTaken);
				action();
			}
			finally
			{
				if (lockTaken) spinLock.Exit();
			}

		}
	}
}
