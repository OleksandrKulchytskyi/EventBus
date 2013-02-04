using EventBus.Infrastructure;
using log4net;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace EventBus.Services
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
	public class MonitorService : IMonitorService, IDisposable
	{
		public static ServiceHost DuplexHost { get; set; }
		private static MonitorService Instance { get; set; }

		private List<IMonitorClient> Clients { get; set; }
		private List<ISubscriber> ActivatedSubscribers { get; set; }
		private int _disposed = 0;

		public MonitorService()
		{
			this.Clients = new List<IMonitorClient>();
			this.ActivatedSubscribers = new List<ISubscriber>();
		}

		public static void Start()
		{
			try
			{
				DefaultSingleton<ICreator>.Instance.Create<ILog>().Info("Starting monitor server");
				MonitorService.Instance = new MonitorService();

				MonitorService.DuplexHost = new ServiceHost(MonitorService.Instance);

				var binding = new NetTcpBinding { ReceiveTimeout = TimeSpan.FromMinutes(30) };
				var address = string.Format("net.tcp://{0}:1234/NEventMonitor", Environment.MachineName);

				MonitorService.DuplexHost.AddServiceEndpoint(typeof(IMonitorService), binding, address);

				DefaultSingleton<ICreator>.Instance.Create<ILog>().Info("Started monitor server");
				DefaultSingleton<ICreator>.Instance.Create<ILog>().Info("Opening duplex host for monitor server");

				MonitorService.DuplexHost.Open();

				DefaultSingleton<ICreator>.Instance.Create<ILog>().Info("Duplex host opened");
			}
			catch (Exception x)
			{
				DefaultSingleton<ICreator>.Instance.Create<ILog>().Error("Error occurrred monitor server", x);
			}
		}

		public static void Stop()
		{
			try
			{
				if (MonitorService.DuplexHost != null &&
					(MonitorService.DuplexHost.State != CommunicationState.Closed &&
					MonitorService.DuplexHost.State != CommunicationState.Closing &&
					MonitorService.DuplexHost.State != CommunicationState.Faulted))
				{
					MonitorService.DuplexHost.Close();
				}
			}
			catch (Exception)
			{
				try
				{
					MonitorService.DuplexHost.Abort();
				}
				catch { }
			}

		}

		internal static void MonitorAlertSubscriberActivated(ISubscriber subscriber)
		{
			try
			{
				Instance.ActivatedSubscribers.Add(subscriber);

				Instance.Clients.ForEach(client =>
				{
					client.OnSubscriberActivated(new SubscriberEventArgs
					{
						EventTypeName = subscriber.GetEventType().Name,
						SubscriberTypeName = subscriber.GetType().Name
					});
				});
			}
			catch (Exception ex)
			{
				DefaultSingleton<ICreator>.Instance.Create<ILog>().Error("Error during MonitorAlertSubscriberActivated", ex);
			}
		}

		internal static void MonitorAlertSubscriberStarted(ISubscriber subscriber)
		{
			try
			{
				Instance.Clients.ForEach(client =>
				{
					client.OnSubscriberStarted(new SubscriberEventArgs
					{
						EventTypeName = subscriber.GetEventType().Name,
						SubscriberTypeName = subscriber.GetType().Name
					});
				});
			}
			catch (Exception ex)
			{
				DefaultSingleton<ICreator>.Instance.Create<ILog>().Error("Error during MonitorAlertSubscriberStarted", ex);
			}
		}

		internal static void MonitorAlertSubscriberCompleted(ISubscriber subscriber)
		{
			try
			{
				Instance.Clients.ForEach(client =>
				{
					client.OnSubscriberCompleted(new SubscriberEventArgs
					{
						EventTypeName = subscriber.GetEventType().Name,
						SubscriberTypeName = subscriber.GetType().Name
					});
				});
			}
			catch (Exception ex)
			{
				DefaultSingleton<ICreator>.Instance.Create<ILog>().Error("Error during MonitorAlertSubscriberCompleted", ex);
			}
		}

		internal static void MonitorAlertSubscriberException(ISubscriber subscriber)
		{
			try
			{
				Instance.Clients.ForEach(client =>
				{
					client.OnSubscriberException(new SubscriberEventArgs
					{
						EventTypeName = subscriber.GetEventType().Name,
						SubscriberTypeName = subscriber.GetType().Name
					});
				});
			}
			catch (Exception ex)
			{
				DefaultSingleton<ICreator>.Instance.Create<ILog>().Error("Error during MonitorAlertSubscriberException", ex);
			}
		}

		public void Connect(MonitorConnectRequest request)
		{
			var client = OperationContext.Current.GetCallbackChannel<IMonitorClient>();

			DefaultSingleton<ICreator>.Instance.Create<ILog>().Info(string.Format("New client has been connected '{0}'", request.ClientMachineName));
			this.Clients.Add(client);

			client.OnClientConnected(new MonitorConnectResponse { Success = true });
		}

		public void Disconnect(MonitorConnectRequest request)
		{
			var client = OperationContext.Current.GetCallbackChannel<IMonitorClient>();

			DefaultSingleton<ICreator>.Instance.Create<ILog>().Info(string.Format("New client has been disconnected '{0}'", request.ClientMachineName));

			this.Clients.Remove(client);
		}

		public void GetSubscribers(MonitorGetSubscriberListRequest request)
		{
			var args = new List<SubscriberEventArgs>();

			this.ActivatedSubscribers.ForEach(sub =>
			{
				args.Add(new SubscriberEventArgs
				{
					EventTypeName = sub.GetEventType().Name,
					SubscriberTypeName = sub.GetType().Name
				});
			});

			OperationContext.Current.GetCallbackChannel<IMonitorClient>().OnSubscriberListReceived(args.ToArray());
		}

		public void Dispose()
		{
			if (System.Threading.Interlocked.Exchange(ref _disposed, 1) == 1)
				return;
			try
			{
				if (MonitorService.DuplexHost != null && (MonitorService.DuplexHost.State != CommunicationState.Closed &&
					MonitorService.DuplexHost.State != CommunicationState.Closing && MonitorService.DuplexHost.State != CommunicationState.Faulted))
				{
					MonitorService.DuplexHost.Close();
					this.ActivatedSubscribers.Clear();
					this.Clients.Clear();
					GC.Collect();
				}
			}
			catch (Exception)
			{
				try
				{
					MonitorService.DuplexHost.Abort();
				}
				catch { }
			}
		}
	}
}