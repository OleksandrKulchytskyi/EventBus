namespace EventBus.Redis.Extension
{
	public static class Utils
	{
		public static string SerializeToJSON<T>(this T data) 
		{
			var ser = new ServiceStack.Text.TypeSerializer<T>();
			return ser.SerializeToString(data);
		}

		public static T DeserializeFromJSON<T>(this string data) 
		{
			var ser = new ServiceStack.Text.TypeSerializer<T>();
			if (ser.CanCreateFromString(typeof(T)))
				return ser.DeserializeFromString(data);
			return default(T);
		}
	}
}