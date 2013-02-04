using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace EventBus.RabbitMQ.Config
{
	public class ReceiverRMQConfigSection : ConfigurationSection
	{
		public static ReceiverRMQConfigSection Current
		{
			get
			{
				return ConfigurationManager.GetSection("receiverRMQConfigSection") as ReceiverRMQConfigSection;
			}
		}

		public static bool IsConfigured
		{
			get
			{
				try
				{
					return (ConfigurationManager.GetSection("receiverRMQConfigSection") as ReceiverRMQConfigSection) != null;
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		private const string _HandleOnReceiveProperty = "handleOnReceive";

		private bool _HandleOnReceive;
		[ConfigurationProperty(_HandleOnReceiveProperty, IsRequired = true)]
		public bool HandleOnReceive
		{
			get { bool.TryParse((string)base[_HandleOnReceiveProperty], out _HandleOnReceive); return _HandleOnReceive; }
			set { base[_HandleOnReceiveProperty] = Convert.ToString(value); }
		}

	}
}
