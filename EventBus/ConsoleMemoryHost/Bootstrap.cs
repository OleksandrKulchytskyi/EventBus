using EventBus.Extensions;
using EventBus.Implementation;
using EventBus.Logging;
using log4net;
using Microsoft.Practices.Unity;

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