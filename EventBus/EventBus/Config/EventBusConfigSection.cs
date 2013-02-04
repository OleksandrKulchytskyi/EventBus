using EventBus.Infrastructure;
using System;
using System.Configuration;
using System.Diagnostics;

namespace EventBus.Config
{
	public class EventBusConfigSection : ConfigurationSection
	{
		private static readonly string _secName = "eventBusConfigSection";

		public static EventBusConfigSection Current
		{
			get
			{
				return ConfigurationManager.GetSection(_secName) as EventBusConfigSection;
			}
		}

		public static bool CheckConfig
		{
			get
			{
				try
				{
					var x = ConfigurationManager.GetSection(_secName) as EventBusConfigSection;
					return true;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					return false;
				}
			}
		}

		private const string CreatorTypeProperty = "creatorType";

		[ConfigurationProperty(CreatorTypeProperty, IsRequired = true)]
		public string CreatorType
		{
			get { return (string)base[CreatorTypeProperty]; }
			set { base[CreatorTypeProperty] = value; }
		}

		private const string PublishersProperty = "publishers";

		[ConfigurationProperty(PublishersProperty, IsRequired = false)]
		public PublishersElement Publishers
		{
			get { return (PublishersElement)base[PublishersProperty]; }
			set { base[PublishersProperty] = value; }
		}

		private const string SubscribersProperty = "subscribers";

		[ConfigurationProperty(SubscribersProperty, IsRequired = false)]
		public SubscribersElement Subscribers
		{
			get { return (SubscribersElement)base[SubscribersProperty]; }
			set { base[SubscribersProperty] = value; }
		}

		public static void TriggerCreator()
		{
			var creatorType = Type.GetType(EventBusConfigSection.Current.CreatorType);
			var creator = Activator.CreateInstance(creatorType);
			DefaultSingleton<ICreator>.Instance = (ICreator)creator;
		}
	}

	public class PublishersElement : ConfigurationElement
	{
		private const string AssembliesProperty = "assemblies";

		[ConfigurationProperty(AssembliesProperty, IsRequired = false)]
		public GenericConfigurationCollection<AssemblyElement> Assemblies
		{
			get
			{
				return (GenericConfigurationCollection<AssemblyElement>)base[AssembliesProperty];
			}
		}
	}

	public class SubscribersElement : ConfigurationElement
	{
		private const string AssembliesProperty = "assemblies";

		[ConfigurationProperty(AssembliesProperty, IsRequired = true)]
		public GenericConfigurationCollection<AssemblyElement> Assemblies
		{
			get
			{
				return (GenericConfigurationCollection<AssemblyElement>)base[AssembliesProperty];
			}
		}

		private const string EventTypesProperty = "events";

		[ConfigurationProperty(EventTypesProperty, IsRequired = true)]
		public GenericConfigurationCollection<EventTypeElement> Events
		{
			get
			{
				return (GenericConfigurationCollection<EventTypeElement>)base[EventTypesProperty];
			}
		}
	}

	public class EventTypeElement : ConfigurationElement
	{
		private const string NameProperty = "name";

		[ConfigurationProperty(NameProperty, IsRequired = true)]
		public string Name
		{
			get { return (string)base[NameProperty]; }
			set { base[NameProperty] = value; }
		}
	}

	public class AssemblyElement : ConfigurationElement
	{
		private const string NameProperty = "assembly";
		[ConfigurationProperty(NameProperty, IsRequired = true)]
		public string Assembly
		{
			get { return (string)base[NameProperty]; }
			set { base[NameProperty] = value; }
		}
	}
}