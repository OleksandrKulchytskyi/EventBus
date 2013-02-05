using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MessageBus.Core.Implementation
{
	public class Unit : IComparable
	{
		internal object _internal = null;

		public int CompareTo(object obj)
		{
			return 0;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Unit) && (obj as Unit)._internal != null)
				return false;
			return true;
		}
	}
}
