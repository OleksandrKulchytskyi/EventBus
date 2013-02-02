namespace EventBus.Infrastructure
{
	public class DefaultSingleton<T>
	{
		private static readonly object _locker = new object();
		private static T _instance;

		public static T Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_locker)
					{
						return _instance;
					}
				}
				return _instance;
			}
			set
			{
				lock (_locker)
				{
					_instance = value;
				}
			}
		}
	}
}