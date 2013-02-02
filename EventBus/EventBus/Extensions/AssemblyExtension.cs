using EventBus.Implementation;
using System.Reflection;

namespace EventBus.Extensions
{
	public static class AssemblyListExtensions
	{
		public static Publishers WithAssembly<T>(this Publishers publishers)
		{
			return publishers.WithAssembly(typeof(T).Assembly);
		}

		public static Publishers WithAssembly(this Publishers publishers, Assembly assembly)
		{
			if (!publishers.AssembliesToSearch.Contains(assembly))
				publishers.AssembliesToSearch.Add(assembly);
			return publishers;
		}

		public static Subscribers WithAssembly<T>(this Subscribers subscribers)
		{
			return subscribers.WithAssembly(typeof(T).Assembly);
		}

		public static Subscribers WithAssembly(this Subscribers subscribers, Assembly assembly)
		{
			if (!subscribers.AssembliesToSearch.Contains(assembly))
				subscribers.AssembliesToSearch.Add(assembly);
			return subscribers;
		}
	}
}