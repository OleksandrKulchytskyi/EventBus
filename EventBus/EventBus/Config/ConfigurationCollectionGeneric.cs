using System.Configuration;
using System.Diagnostics;

namespace EventBus.Config
{
	[DebuggerStepThrough]
	public class GenericConfigurationCollection<T> : ConfigurationElementCollection
		where T : ConfigurationElement, new()
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.AddRemoveClearMap;
			}
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new T();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((T)element).GetHashCode(); // this could probably be improved upon
		}
	}
}