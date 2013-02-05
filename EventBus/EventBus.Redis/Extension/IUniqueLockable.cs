namespace EventBus.Redis.Extension
{
	public interface IUniqueLockable
	{
		long Id { get; set; }
		bool Lock();
		void Unlock();
	}
}