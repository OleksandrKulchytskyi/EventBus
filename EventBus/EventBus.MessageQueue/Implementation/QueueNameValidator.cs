using EventBus.MessageQueue.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.MessageQueue.Implementation
{
	internal class QueueNameValidator : IValidator<string>
	{
		private string _data;
		public void SetObject(string data)
		{
			_data = data;
		}

		public bool Validate()
		{
			if (string.IsNullOrEmpty(_data))
				return false;

			for (int i = 0; i < _data.Length; i++)
			{
				switch (i)
				{
					case 0:
						if (_data[i] != '.')
							return false;
						break;

					case 1:
						if (_data[i] != '\\')
							return false;
						break;

					default:
						if (!char.IsLetter(_data[i]))
							return false;
						break;
				}
			}
			return true;
		}
	}
}
