using EventBus.Infrastructure;
using EventBus.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System;

namespace EventBus.Implementation
{
	public class MemoryMessageStore : IMessageStore, IDisposable
	{
		private readonly IList<StoreInfo> messages;
		private readonly ReaderWriterLock messagesLock;

		public MemoryMessageStore()
		{
			messagesLock = new ReaderWriterLock();
			messages = new List<StoreInfo>();
		}

		public void SaveMessage<TMessage>(TMessage message, long checkpoint) where TMessage : IMessage
		{
			this.messagesLock.ExecuteWriteLocked(delegate
			{
				this.messages.Add(new StoreInfo(message, checkpoint));
			});
		}

		public IEnumerable<IMessage> GetMessages(long checkpoint)
		{
			return this.messagesLock.ExecuteWriteLocked(delegate
			{
				List<StoreInfo> receivedMessages = (from msi in this.messages
													where msi.Checkpoint >= checkpoint
													select msi).ToList<StoreInfo>();
				foreach (StoreInfo receivedMessage in receivedMessages)
				{
					this.messages.Remove(receivedMessage);
				}
				return (from msi in receivedMessages select msi.Message).ToList<IMessage>();
			});
		}

		#region IDisposable Members

		private int disposed = 0;

		private void Dispose(Boolean disposing)
		{
			if (Interlocked.Exchange(ref disposed, 1) == 1)
				return;

			if (disposing)
			{
				messagesLock.ExecuteWriteLocked(() =>
				{
					this.messages.Clear();
				});
			}
		}

		public void Dispose()
		{
			if (disposed == 1)
				throw new ObjectDisposedException("MemoryMessageStore have been already disposed");

			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}

	internal sealed class StoreInfo
	{
		public StoreInfo(IMessage message, long checkpoint)
		{
			this.Checkpoint = checkpoint;
			this.Message = message;
		}

		public long Checkpoint
		{
			get;
			private set;
		}

		public IMessage Message
		{
			get;
			private set;
		}
	}
}