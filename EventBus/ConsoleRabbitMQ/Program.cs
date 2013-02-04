using EventBus.Extensions;
using EventBus.Implementation;
using System;

namespace ConsoleRabbitMQ
{
	internal class Program
	{
		private static void Main(string[] args)
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