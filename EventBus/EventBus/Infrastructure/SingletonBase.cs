using System;

namespace EventBus.Infrastructure
{
	public abstract class SingletonBase<T> where T : class
	{
		private static readonly Lazy<T> _Lazy = new Lazy<T>(() =>
		{
			// Get non-public constructors for T.
			var ctors = typeof(T).GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

			if (!Array.Exists(ctors, (ci) => ci.GetParameters().Length == 0))
				throw new InvalidOperationException("Non-public ctor() was not found.");

			var ctor = Array.Find(ctors, (ci) => ci.GetParameters().Length == 0);

			// Invoke constructor and return resulting object.
			return ctor.Invoke(new object[] { }) as T;
		}, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);

		public static T Instance
		{
			get
			{
				return _Lazy.Value;
			}
		}
	}
}