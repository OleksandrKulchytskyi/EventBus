using EventBus.Implementation;
using EventBus.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleRabbitMQ
{
	class Program
	{
		static void Main(string[] args)
		{
			using (WireDriver.Start())
			{
				Publishers.Current.WithAssembly<Program>();
				Publishers.Current.Publish(new Message1());

				Console.ReadLine();
			}

			System.Threading.Thread.Sleep(3550);
		}
	}
}
