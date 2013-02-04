using System;

namespace ConsoleMemoryHost
{
	internal class ConsoleSubscriber : EventBus.Hosting.MemorySubscriber<ConsoleEvent>
	{
		private Guid _id;

		public ConsoleSubscriber()
			: base()
		{
			_id = Guid.NewGuid();
			if (Logger == null)
			{
				Console.WriteLine("Initiating logger in ConsoleSubscriber");
				base.Logger = Bootstrap.Creator.Create<log4net.ILog>();
			}
			Console.WriteLine(_id.ToString("D"));
		}

		public override void HandleEvent(ConsoleEvent target)
		{
			base.Logger.Debug(string.Format("ConsoleSubscriber {2}Received notification of event '{0}' getting proccessed at {1}",
											target.Id, target.Published.ToShortTimeString(), Environment.NewLine));

			base.HandleEvent(target);
		}
	}

	internal class ConsolePublisher : EventBus.Hosting.MemoryPublisher<ConsoleEvent>
	{
		public ConsolePublisher()
			: base()
		{
		}
	}
}