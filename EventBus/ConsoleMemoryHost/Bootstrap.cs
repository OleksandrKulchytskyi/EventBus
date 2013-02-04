using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventBus.Extensions;
using EventBus.Implementation;
using Microsoft.Practices.Unity;
using log4net;
using EventBus.Logging;

namespace ConsoleMemoryHost
{
	internal class Bootstrap
	{
		public static Publishers CurrentPublishers
		{
			get;
			private set;
		}

		internal static EventBus.Infrastructure.ICreator Creator
		{
			get;
			private set;
		}

		public static void Init()
		{
			Creator = new UnityCreator(new UnityContainer().RegisterType<ILog, Logger>());
			Subscribers.Current.WithCreator(Creator)
				.WithAssembly<Bootstrap>().Subscribe<ConsoleEvent>();

			CurrentPublishers = Publishers.Current.WithCreator(Creator)
				.WithAssembly<Bootstrap>();
		}

	}
}
