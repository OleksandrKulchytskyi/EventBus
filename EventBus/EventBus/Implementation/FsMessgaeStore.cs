using EventBus.Extensions;
using EventBus.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EventBus.Implementation
{
	public class FsMessageStore : IMessageStore
	{
		private readonly string storePath;
		private long identity;
		private const string msgFormat = "{0}-{1}.mechanic";

		public IMessageSerializer MessageSerializer
		{
			get;
			private set;
		}

		public FsMessageStore(string storePath)
			: this(storePath, new BsonMessageSerializer())
		{
		}

		public FsMessageStore(string storePath, IMessageSerializer serializer)
		{
			if (string.IsNullOrEmpty(storePath) || !Directory.Exists(storePath))
				throw new System.ArgumentException("storePath");

			if (serializer == null)
				throw new System.ArgumentNullException("serializer");

			this.storePath = storePath;
			MessageSerializer = serializer;
		}

		public void SaveMessage<TMessage>(TMessage message, long checkpoint) where TMessage : IMessage
		{
			string globalPath = this.storePath;
			object check = checkpoint;
			long num;
			this.identity = (num = this.identity) + 1L;
			string fpath = Path.Combine(globalPath, string.Format(msgFormat, check, num));
			using (FileStream fileStream = new FileStream(fpath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				this.MessageSerializer.Serialize<TMessage>(message, fileStream);
				fileStream.Close();
			}
		}

		public IEnumerable<IMessage> GetMessages(long checkpoint)
		{
			IEnumerable<FileInfo> files = from fi in new DirectoryInfo(this.storePath).GetFiles("*.mechanic")
										  where long.Parse(Path.GetFileNameWithoutExtension(fi.Name).SubstringBefore("-")) >= checkpoint
										  select fi;
			List<IMessage> messages = new List<IMessage>();
			foreach (FileInfo file in files)
			{
				using (FileStream fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.None))
				{
					messages.Add(this.MessageSerializer.Deserialize(fileStream));
				}
				file.Delete();
			}
			return messages;
		}
	}
}