using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EventBus.MessageQueue.Config
{
	public class EventBusMsmqSection : ConfigurationSection
	{
		public static EventBusMsmqSection Current
		{
			get
			{
				return ConfigurationManager.GetSection("eventBusMsmqSection") as EventBusMsmqSection;
			}
		}

		public static bool CheckConfig
		{
			get
			{
				try
				{
					var x = ConfigurationManager.GetSection("eventBusMsmqSection") as EventBusMsmqSection;
					return true;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					return false;
				}
			}
		}

		private const string FormatterTypePoperty = "formatterType";
		[ConfigurationProperty(FormatterTypePoperty, IsRequired = true)]
		public string FormatterType
		{
			get { return (string)base[FormatterTypePoperty]; }
			set { base[FormatterTypePoperty] = value; }
		}
	}
}
