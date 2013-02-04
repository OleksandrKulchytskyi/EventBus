using EventBus.Extensions;
using EventBus.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventBus.Implementation
{
	public class Publishers
	{
		public static Publishers Current { get; private set; }

		static Publishers()
		{
			Publishers.Current = new Publishers();
		}

		public ICreator Creator { get; internal set; }

		public List<Assembly> AssembliesToSearch { get; set; }

		private Publishers()
		{
			this.AssembliesToSearch = new List<Assembly>();
			this.Creator = new DefaultCreator();
		}

		public Publishers Publish<TEvnt>(TEvnt eventToPublish)
		{
			if (this.AssembliesToSearch.Count == 0)
				this.WithAssembly<Publishers>();

			this.AssembliesToSearch.ForEach((assembly) =>
			{
				assembly.GetTypes().Where(a => typeof(IPublisher<TEvnt>).IsAssignableFrom(a) && !a.IsAbstract).ToList()
					.ForEach((pub) =>
					{
						var publisherInstance = this.GetOrCreatePublisherInstance(pub);

						pub.InvokeMember("Publish", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance,
							null, publisherInstance, new object[] { eventToPublish });
					});
			});

			return this;
		}

		private object GetOrCreatePublisherInstance(Type publisherType)
		{
			return this.Creator.Create(publisherType);
		}
	}
}