using EventBus.Infrastructure;
using EventBus.RabbitMQ.Extension;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Threading;

namespace EventBus.RabbitMQ.Infrastructure
{
	public class SubscriberRMQ<TEvnt> : ISubscriber<TEvnt>, IUnsubscribe, IConnectionInfo, IDisposable
	{
		private bool _HandleOnReceive = false;
		protected readonly ILog Log;
		private IConnection _connection;
		private QueueingBasicConsumer _consumer;
		private IModel _model;
		private delegate object DequeueMethod();

		private readonly CancellationTokenSource _cts;

		public SubscriberRMQ()
			: this(LogManager.GetLogger(typeof(TEvnt)))
		{
		}

		public SubscriberRMQ(ILog log)
		{
			Log = log;
			_cts = new CancellationTokenSource();
			if (Config.ReceiverRMQConfigSection.IsConfigured)
				_HandleOnReceive = Config.ReceiverRMQConfigSection.Current.HandleOnReceive;
		}

		public virtual string RoutingKey
		{
			get { return typeof(TEvnt).FullName; }
		}

		public virtual string ExchangeName
		{
			get { return "EvetBus"; }
		}

		public virtual string QueueName
		{
			get { return null; }
		}

		#region IConnectionDescriptor Members

		public string HostName { get; set; }

		public string VirtualHost { get; set; }

		public string UserName { get; set; }

		public string Password { get; set; }

		public string Protocol { get; set; }

		public int? Port { get; set; }

		#endregion IConnectionDescriptor Members

		#region ISubscriber<E> Members

		public Type GetEventType()
		{
			return typeof(TEvnt);
		}

		#region events
		public event EventHandler<BusEventArgs<TEvnt>> EventReceived;

		protected virtual void OnEventReceived(TEvnt message)
		{
			var ev = EventReceived;
			if (null != ev)
			{
				ev(this, new BusEventArgs<TEvnt>(message));
			}
		}

		public event EventHandler<BusEventArgs<TEvnt>> EventHandled;

		protected virtual void OnEventHandled(TEvnt message)
		{
			var ev = EventHandled;
			if (null != ev)
			{
				ev(this, new BusEventArgs<TEvnt>(message));
			}
		}

		public virtual void HandleEvent(TEvnt target)
		{
			OnEventHandled(target);
		}
		#endregion

		public void Subscribe()
		{
			try
			{
				CloseExistingConnectionAndModel();
				CreateConnectionAndModel();

				string queueName = DeclareAndBindQueue();

				_consumer = new QueueingBasicConsumer(_model);
				_model.BasicConsume(queueName, true, _consumer);

				InvokeAsyncDequeue();
			}
			catch (Exception ex)
			{
				Log.Error("Error occurred while perform subscribe operation", ex);
				CancelReceive();
				CloseExistingConnectionAndModel();
			}
		}

		#endregion ISubscriber<E> Members

		#region IUnsubscriber Members

		public void Unsubscribe()
		{
			CancelReceive();
			CloseExistingConnectionAndModel();
		}

		private void CancelReceive()
		{
			if (!_cts.IsCancellationRequested)
			{
				_cts.Cancel();
			}
		}

		#endregion IUnsubscriber Members

		private string DeclareAndBindQueue()
		{
			_model.ExchangeDeclare(ExchangeName, ExchangeType.Direct);

			string queueName = QueueName;
			if (string.IsNullOrEmpty(queueName))
			{
				queueName = _model.QueueDeclare();
			}
			else
			{
				_model.QueueDeclare(queueName, true, false, true, null);
			}

			_model.QueueBind(queueName, ExchangeName, RoutingKey);
			return queueName;
		}

		private void CreateConnectionAndModel()
		{
			ConnectionFactory connectionFactory = this.CreateConnectionFactory();
			IConnection connection = connectionFactory.CreateConnection();
			var existCon = Interlocked.CompareExchange(ref _connection, connection, null);
			if (existCon != null)
			{
				CloseConnection(connection);
			}
			else
			{
				IModel model = _connection.CreateModel();
				var existModel = Interlocked.CompareExchange(ref _model, model, null);
				if (existModel != null)
				{
					CloseModel(model);
				}
			}
		}

		private void CloseExistingConnectionAndModel()
		{
			var model = Interlocked.Exchange(ref _model, null);
			var connection = Interlocked.Exchange(ref _connection, null);

			CloseModel(model);
			CloseConnection(connection);
		}

		private void CloseModel(IModel model)
		{
			if (model != null)
			{
				model.Close();
				model.Dispose();
			}
		}

		private void CloseConnection(IConnection connection)
		{
			if (connection != null)
			{
				connection.Close();
				connection.Dispose();
			}
		}

		private void InvokeAsyncDequeue()
		{
			DequeueMethod dq = _consumer.Queue.Dequeue;
			System.Threading.Tasks.Task.Factory.FromAsync(dq.BeginInvoke, OnMessageDequed, dq);
		}

		private void OnMessageDequed(IAsyncResult ar)
		{
			BasicDeliverEventArgs message = null;
			try
			{
				if (_disposed != 0 || _cts.IsCancellationRequested)
					return;

				DequeueMethod dq = (DequeueMethod)ar.AsyncState;
				message = dq.EndInvoke(ar) as BasicDeliverEventArgs;
				if (message != null)
				{
					OnEventReceived(message.Body.Deserialize<TEvnt>());
				}
				//_model.BasicAck(message.DeliveryTag, false);
			}
			catch (EndOfStreamException endEx)
			{
				Log.Error("End-of-stream exception has occurred while ending the async dequeue operation; the operation will not be requeued", endEx);
				return;
			}
			catch (Exception genEx)
			{
				Log.Error("Exception has occurred while ending the async dequeue operation; \n\r the operation will be requeued", genEx);
				InvokeAsyncDequeue();
				return;
			}

			if (message != null && _HandleOnReceive)
			{
				if (_disposed != 0 || _cts.IsCancellationRequested)
					return;
				try
				{
					HandleData(message);
				}
				catch (Exception e)
				{
					Log.Error("Error occured while try to handle the message; \n\r the next dequeue operation will still occur", e);
				}
			}
			message = null;

			InvokeAsyncDequeue();
		}

		private void HandleData(BasicDeliverEventArgs message)
		{
			TEvnt e = message.Body.Deserialize<TEvnt>();
			HandleEvent(e);
		}

		#region IDisposable Members

		private int _disposed;

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(Boolean disposing)
		{
			if (System.Threading.Interlocked.Exchange(ref _disposed, 1) == 0)
			{
				if (disposing)
				{
					Unsubscribe();
				}
			}
		}
		#endregion
	}
}