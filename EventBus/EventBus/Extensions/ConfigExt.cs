using EventBus.Config;
using EventBus.Implementation;
using EventBus.Infrastructure;
using System;
using System.Linq;
using System.Reflection;

namespace EventBus.Extensions
{
	public static class PublisherConfigurationExtensions
	{
		public static Publishers FromConfiguration(this Publishers publishers)
		{
			if (EventBusConfigSection.CheckConfig)
			{
				if (EventBusConfigSection.Current.Publishers != null)
				{
					EventBusConfigSection.Current.Publishers.Assemblies.Cast<AssemblyToWatchElement>().ToList()
						.ForEach(element =>
						{
							publishers = publishers.WithAssembly(Assembly.Load(element.Assembly));
						});
				}

				publishers = publishers.WithCreator(DefaultSingleton<ICreator>.Instance);
			}

			return publishers;
		}
	}

	public static class SubscriberConfigurationExtensions
	{
		public static Subscribers FromConfiguration(this Subscribers subscribers)
		{
			if (EventBusConfigSection.CheckConfig)
			{
				subscribers = subscribers.WithCreator(DefaultSingleton<ICreator>.Instance);

				if (EventBusConfigSection.Current.Subscribers != null)
				{
					EventBusConfigSection.Current.Subscribers.Assemblies.Cast<AssemblyToWatchElement>().ToList()
						.ForEach(element =>
						{
							subscribers = subscribers.WithAssembly(Assembly.Load(element.Assembly));
						});

					EventBusConfigSection.Current.Subscribers.Events.Cast<EventTypeElement>().ToList()
						.ForEach(element =>
						{
							typeof(Subscribers)
								.GetMethod("Subscribe")
									.MakeGenericMethod(Type.GetType(element.Name))
										.Invoke(subscribers, new object[] { });
						});
				}
			}

			return subscribers;
		}
	}
}