using EventBus.Implementation;
using EventBus.MessageQueue.Infrastructure;

namespace EventBus.MessageQueue.Extensions
{
	public static class MsqmExtensions
	{
		public static Publishers PublishToMessageQueue<TEvnt>(this Publishers publishers, MessageQueueRequest request, TEvnt eventToPublish)
		{
			var publisher = new PublisherMQ<TEvnt>();
			publisher.IsTransactional = request.IsTransactional;
			publisher.QueuePath = request.QueuePath;
			publisher.Publish(eventToPublish);
			return publishers;
		}
	}
}