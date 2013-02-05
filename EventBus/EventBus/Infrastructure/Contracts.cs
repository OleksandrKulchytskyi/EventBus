using System;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace EventBus.Infrastructure
{
	[ServiceContract(CallbackContract = typeof(IMonitorClient))]
	public interface IMonitorService
	{
		[OperationContract(IsOneWay = true)]
		void Connect(ConnectRequest request);

		[OperationContract(IsOneWay = true)]
		void Disconnect(DisconnectRequest request);

		[OperationContract(IsOneWay = true)]
		void GetSubscribers(GetSubscriberListRequest request);
	}

	public interface IMonitorClient
	{
		[OperationContract(IsOneWay = true)]
		void OnSubscriberListReceived(SubscriberEventArgs[] args);

		[OperationContract(IsOneWay = true)]
		void OnClientConnected(ConnectResponse response);

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
	public class ConnectRequest
	{
		[DataMember]
		public string ClientMachineName { get; set; }

		[DataMember]
		public DateTime Connected { get; set; }
	}

	[DataContract]
	public class DisconnectRequest
	{
		[DataMember]
		public string ClientMachineName { get; set; }

		[DataMember]
		public DateTime Connected { get; set; }
	}

	[DataContract]
	public class ConnectResponse
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
	public class GetSubscriberListRequest
	{
		[DataMember]
		public string ClientMachineName { get; set; }
	}
}