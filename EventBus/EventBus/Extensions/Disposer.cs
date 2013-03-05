using System;

namespace EventBus.Extensions
{
	/// <summary>
	/// Helper class which implements IDisposable and incorporate specified action while calling Dispose method.
	/// </summary>
	public class Disposer : IDisposable
	{
		private readonly Action _callOnDispose;

		public static IDisposable Fake
		{
			get
			{
				return new Disposer(() => { });
			}
		}

		public Disposer(Action callOnDispose)
		{
			this._callOnDispose = callOnDispose;
		}

		public void Dispose()
		{
			this._callOnDispose();
		}
	}
}