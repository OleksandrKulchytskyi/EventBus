namespace EventBus.Infrastructure
{
	public interface IPublisher<TMsg>
	{
		void Publish(TMsg data);
	}
}