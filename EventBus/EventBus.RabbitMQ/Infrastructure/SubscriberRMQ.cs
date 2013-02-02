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
	public class SubscriberRMQ<TEvnt> : ISubscriber<TEvnt>, IUnsubscribe, IConnectionInfo
	{
		protected ILog Log;

		private IConnection _connection;
		private QueueingBasicConsumer _consumer;
		private IModel _model;

		public SubscriberRMQ()
		{
			Log = LogManager.GetLogger(GetType());
		}

		public SubscriberRMQ(ILog log)
		{
			Log = log;
		}

		public virtual string RoutingKey
		{
			get { return typeof(TEvnt).FullName; }
		}

		public virtual string ExchangeName
		{
			get { return "NEvent"; }
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

		public event EventHandler<BusEventArgs<TEvnt>> EventReceived;

		public event EventHandler<BusEventArgs<TEvnt>> EventHandled;

		public virtual void Handle(TEvnt target)
		{
			OnEventHandled(target);
		}

		public void Subscribe()
		{
			CloseExistingConnectionAndModel();
			CreateConnectionAndModel();

			string queueName = DeclareAndBindQueue();

			_consumer = new QueueingBasicConsumer(_model);
			_model.BasicConsume(queueName, true, _consumer);

			InvokeAsyncDequeue();
		}

		#endregion ISubscriber<E> Members

		#region IUnsubscriber Members

		public void Unsubscribe()
		{
			CloseExistingConnectionAndModel();
		}

		#endregion IUnsubscriber Members

		protected virtual void OnEventHandled(TEvnt message)
		{
			var ev = EventHandled;
			if (null != ev)
			{
				ev(this, new BusEventArgs<TEvnt>(message));
			}
		}

		protected virtual void OnEventCaught(TEvnt message)
		{
			var ev = EventReceived;
			if (null != ev)
			{
				ev(this, new BusEventArgs<TEvnt>(message));
			}
		}

		private string DeclareAndBindQueue()
		{
			_model.ExchangeDeclare(ExchangeName, ExchangeType.Direct);

			var queueName = QueueName;
			if (null == queueName)
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
			var connection = connectionFactory.CreateConnection();
			var existingConnection = Interlocked.CompareExchange(ref _connection, connection, null);
			if (null != existingConnection)
			{
				CloseConnection(connection);
			}
			else
			{
				var model = _connection.CreateModel();
				var existingModel = Interlocked.CompareExchange(ref _model, model, null);
				if (null != existingModel)
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
			if (null != model)
			{
				model.Close();
				model.Dispose();
			}
		}

		private void CloseConnection(IConnection connection)
		{
			if (null != connection)
			{
				connection.Close();
				connection.Dispose();
			}
		}

		private void InvokeAsyncDequeue()
		{
			DequeueMethod dq = _consumer.Queue.Dequeue;
			dq.BeginInvoke(OnDequeueMessage, dq);
		}

		private void OnDequeueMessage(IAsyncResult ar)
		{
			BasicDeliverEventArgs message = null;
			try
			{
				DequeueMethod dq = (DequeueMethod)ar.AsyncState;
				message = dq.EndInvoke(ar) as BasicDeliverEventArgs;
			}
			catch (EndOfStreamException eos)
			{
				Log.Error(
					"an end-of-stream exception has occurred while ending the async dequeue operation; the operation will not be requeued",
					eos);
				return;
			}
			catch (Exception e)
			{
				Log.Error(
					"an exception has occurred while ending the async dequeue operation; the operation will be requeued",
					e);
				InvokeAsyncDequeue();
				return;
			}

			if (null != message)
			{
				try
				{
					HandleMessage(message);
				}
				catch (Exception e)
				{
					Log.Error(
						"an error occured while handling the message; the next dequeue operation will still occur", e);
				}
			}

			InvokeAsyncDequeue();
		}

		private void HandleMessage(BasicDeliverEventArgs message)
		{
			TEvnt e = message.Body.Deserialize<TEvnt>();

			Handle(e);
		}

		#region Nested type: DequeueMethod

		private delegate object DequeueMethod();

		#endregion Nested type: DequeueMethod
	}
}