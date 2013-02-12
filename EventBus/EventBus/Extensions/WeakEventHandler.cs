using System;
using System.Linq;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace EventBus.Extensions
{
	public sealed class WeakEventHandler<TEventArgs> where TEventArgs : EventArgs
	{
		private readonly WeakReference _targetReference;
		private readonly MethodInfo _method;

		public WeakEventHandler(EventHandler<TEventArgs> callback)
		{
			_method = callback.Method;
			_targetReference = new WeakReference(callback.Target, true);
		}

		static Func<T, object, object> MagicMethod<T>(MethodInfo method)
		{
			var parameter = method.GetParameters().Single();
			var instance = Expression.Parameter(typeof(T), "instance");
			var argument = Expression.Parameter(typeof(object), "argument");

			var methodCall = Expression.Call(instance, method, Expression.Convert(argument, parameter.ParameterType));

			return Expression.Lambda<Func<T, object, object>>(Expression.Convert(methodCall, typeof(object)), instance, argument).
					Compile();
		}

		[DebuggerNonUserCode]
		public void Handler(object sender, TEventArgs e)
		{
			var target = _targetReference.Target;
			if (target != null)
			{
				var callback = (Action<object, TEventArgs>)Delegate.CreateDelegate(typeof(Action<object, TEventArgs>), target, _method, true);
				if (callback != null)
				{
					callback(sender, e);
				}
			}
		}
	}
}