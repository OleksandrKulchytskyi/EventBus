using EventBus.Implementation;
using EventBus.Infrastructure;

namespace EventBus.Extensions
{
	public static class CreatorExtensions
	{
		public static Publishers WithCreator(this Publishers publishers, ICreator creator)
		{
			publishers.Creator = creator;
			return publishers;
		}

		public static Subscribers WithCreator(this Subscribers subscribers, ICreator creator)
		{
			subscribers.Creator = creator;
			return subscribers;
		}
	}
}