using EventBus.Extensions;
using EventBus.Infrastructure;
using EventBus.Services;
using log4net;
using System;

namespace EventBus.Implementation
{
	public class WireDriver
	{
		private class WireSession : IDisposable
		{
			public void Dispose()
			{
				WireDriver.Stop();
			}
		}

		public static event EventHandler Stopping;


		public static void Stop()
		{
			OnStopping();
			MonitorService.Stop();
		}

		static void OnStopping()
		{
			var ev = Stopping;

			if (null != ev)
			{
				ev.Invoke(typeof(WireDriver), EventArgs.Empty);
			}
		}

		public static IDisposable Start()
		{
			// set up the creator
			Config.EventBusConfigSection.TriggerCreator();
			// start the monitor wcf service
			MonitorService.Start();

			// load up all the subscribers
			Subscribers.Current.WithSubscriberActivatedAction(new SubscriberActivatedHandler(
							delegate(ISubscriber subscriber)
							{
								MonitorService.MonitorAlertSubscriberActivated(subscriber);

								DefaultSingleton<ICreator>.Instance.Create<ILog>().Debug(string.Format("Activated '{0}' to handle '{1}'",
										subscriber.GetEventType().Name, subscriber.GetType().Name));
							}))
					.WithSubscriberStartedAction(new SubscriberStartedExecutionHandler(
							delegate(ISubscriber subscriber, object target)
							{
								MonitorService.MonitorAlertSubscriberStarted(subscriber);

								DefaultSingleton<ICreator>.Instance.Create<ILog>().Debug(string.Format("Started handling '{0}' with '{1}'",
										subscriber.GetEventType().Name, subscriber.GetType().Name));
							}))
					.WithSubscriberCompletedAction(
						new SubscriberCompletedExecutionHandler(
							delegate(ISubscriber subscriber, object target)
							{
								MonitorService.MonitorAlertSubscriberCompleted(subscriber);

								DefaultSingleton<ICreator>.Instance.Create<ILog>().Debug(string.Format("Completed handling of '{0}' with '{1}'",
										subscriber.GetEventType().Name, subscriber.GetType().Name));
							}))
					.WithSubscriberExceptionHandler(
						new SubscriberExecutionExceptionHandler(
							delegate(ISubscriber subscriber, Exception exception)
							{
								MonitorService.MonitorAlertSubscriberException(subscriber);

								DefaultSingleton<ICreator>.Instance.Create<ILog>().Error(string.Format("Error handling '{0}' with '{1}'",
										subscriber.GetEventType().Name, subscriber.GetType().Name), exception);
							}))
					.FromConfiguration();

			return new WireSession();
		}
	}
}