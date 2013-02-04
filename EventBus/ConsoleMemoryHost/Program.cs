using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ConsoleMemoryHost
{
	class Program
	{
		static void Main(string[] args)
		{
			Bootstrap.Init();

			Timer t = new Timer(Publish, null, (int)TimeSpan.FromSeconds(5).TotalMilliseconds, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);

			while (Console.ReadLine() != "exit")
			{

			}
			t.Change(0, System.Threading.Timeout.Infinite);
			t.Dispose();
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}

		static void Publish(object state)
		{
			Bootstrap.CurrentPublishers.Publish(new ConsoleEvent());
		}
	}
}
