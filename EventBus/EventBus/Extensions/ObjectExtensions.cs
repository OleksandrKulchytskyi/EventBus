using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.Extensions
{
	public static class ObjectExtensions
	{
		public static T WrapTo<T>(this object input)
			where T : class
		{
			if (input == null)
				throw new ArgumentNullException("input");

			var result = input as T;
			if (result == null)
				throw new InvalidOperationException("Unable to convert from " + input.GetType().FullName + " to " + typeof(T).FullName);

			return result;
		}

		public static string ToFriendlyName(this Type type)
		{
			if (!type.IsGenericType)
			{
				return type.FullName;
			}


			string name = type.GetGenericTypeDefinition().FullName;
			if (name == null)
				return type.Name;
			StringBuilder sb = new StringBuilder();
			sb.Append(name.Substring(0, name.IndexOf('`')));
			sb.Append("<");

			Type[] arguments = type.GetGenericArguments();
			for (int i = 0; i < arguments.Length; i++)
			{
				if (i > 0)
					sb.Append(",");

				sb.Append(arguments[i].Name);
			}
			sb.Append(">");

			string formatted = sb.ToString();
			sb.Clear();
			sb = null;
			return formatted;
		}
	}
}
