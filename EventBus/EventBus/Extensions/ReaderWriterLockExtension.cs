using System;
using System.Threading;

namespace EventBus.Extensions
{
	public static class ReaderWriterLocerkExtensions
	{
		public static void ExecuteWriteLocked(this ReaderWriterLock readerWriterLock, Action operation)
		{
			readerWriterLock.ExecuteLocked(TimeSpan.FromSeconds(1.0), new Action<TimeSpan>(readerWriterLock.AcquireWriterLock), () => readerWriterLock.IsWriterLockHeld,
											new Action(readerWriterLock.ReleaseWriterLock), new Func<object>(() =>
			{
				operation();
				return null;
			}));
		}

		public static T ExecuteWriteLocked<T>(this ReaderWriterLock readerWriterLock, Func<T> operation)
		{
			return readerWriterLock.ExecuteLocked(TimeSpan.FromSeconds(1.0), new Action<TimeSpan>(readerWriterLock.AcquireWriterLock), () => readerWriterLock.IsWriterLockHeld,
												new Action(readerWriterLock.ReleaseWriterLock), operation);
		}

		public static void ExecuteReadLocked(this ReaderWriterLock readerWriterLock, Action operation)
		{
			readerWriterLock.ExecuteLocked(TimeSpan.FromSeconds(1.0), new Action<TimeSpan>(readerWriterLock.AcquireReaderLock), () => readerWriterLock.IsReaderLockHeld,
											new Action(readerWriterLock.ReleaseReaderLock), new Func<object>(() =>
											{
												operation();
												return null;
											}));
		}

		private static T ExecuteLocked<T>(this ReaderWriterLock readerWriterLock, TimeSpan timeout, Action<TimeSpan> acquireLock, Func<bool> isLockHeld,
											Action releaseLock, Func<T> operation)
		{
			T result;
			try
			{
				acquireLock(timeout);
				result = operation();
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("releasing lock: " + e);
				throw;
			}
			finally
			{
				if (isLockHeld())
				{
					releaseLock();
				}
			}
			return result;
		}
	}
}