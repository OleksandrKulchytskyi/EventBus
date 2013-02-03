using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace EventBus.Infrastructure
{
	[ServiceContract(CallbackContract = typeof(IMonitorClient))]
	public interface IMonitorService
	{
		[OperationContract(IsOneWay = true)]
		void Connect(MonitorConnectRequest request);

		[OperationContract(IsOneWay = true)]
		void Disconnect(MonitorConnectRequest request);

		[OperationContract(IsOneWay = true)]
		void GetSubscribers(MonitorGetSubscriberListRequest request);
	}

	public interface IMonitorClient
	{
		[OperationContract(IsOneWay = true)]
		void OnSubscriberListReceived(SubscriberEventArgs[] args);

		[OperationContract(IsOneWay = true)]
		void OnClientConnected(MonitorConnectResponse response);

		[OperationContract(IsOneWay = true)]
		void OnSubscriberActivated(SubscriberEventArgs args);

		[OperationContract(IsOneWay = true)]
		void OnSubscriberStarted(SubscriberEventArgs args);

		[OperationContract(IsOneWay = true)]
		void OnSubscriberCompleted(SubscriberEventArgs args);

		[OperationContract(IsOneWay = true)]
		void OnSubscriberException(SubscriberEventArgs args);
	}

	[DataContract]
	public class MonitorConnectRequest
	{
		[DataMember]
		public string ClientMachineName { get; set; }

		[DataMember]
		public DateTime Connected { get; set; }
	}

	[DataContract]
	public class MonitorConnectResponse
	{
		[DataMember]
		public bool Success { get; set; }
	}

	[DataContract]
	public class SubscriberEventArgs
	{
		[DataMember]
		public string EventTypeName { get; set; }

		[DataMember]
		public string SubscriberTypeName { get; set; }
	}

	[DataContract]
	public class MonitorGetSubscriberListRequest
	{
		[DataMember]
		public string ClientMachineName { get; set; }
	}
}