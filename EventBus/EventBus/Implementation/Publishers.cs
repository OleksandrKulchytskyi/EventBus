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

		public Publishers Publish<E>(E eventToPublish)
		{
			if (this.AssembliesToSearch.Count == 0)
				this.WithAssembly<Publishers>();

			this.AssembliesToSearch.ForEach((assembly) =>
			{
				assembly.GetTypes()
					.Where(a => typeof(IPublisher<E>).IsAssignableFrom(a) && !a.IsAbstract).ToList()
					.ForEach((pub) =>
					{
						var publisherInstance = this.CreateInstanceOfPublisher(pub);

						pub.InvokeMember("Publish", BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Instance,
							null, publisherInstance, new object[] { eventToPublish });
					});
			});

			return this;
		}

		private object CreateInstanceOfPublisher(Type publisherType)
		{
			return this.Creator.Create(publisherType);
		}
	}
}