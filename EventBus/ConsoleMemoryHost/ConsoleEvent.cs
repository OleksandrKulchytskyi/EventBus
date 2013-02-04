using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleMemoryHost
{
	public class ConsoleEvent
	{
		public ConsoleEvent()
		{
			Id = Guid.NewGuid();
			Published = DateTime.Now;
		}

		public Guid Id { get; set; }
		public DateTime Published { get; set; }
		public DateTime Handled { get; set; }

		public override string ToString()
		{
			int dur = this.Handled.Subtract(this.Published).Milliseconds;

			var s = Handled == DateTime.MinValue
				? string.Format("{1}: Unhandled event {0}", this.Id, this.GetType().Name)
				: string.Format("{2}: Handled event{1} in {0} Milliseconds", dur, this.Id, this.GetType().Name);

			return s;
		}
	}
}
