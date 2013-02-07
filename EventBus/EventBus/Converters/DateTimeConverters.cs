using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace EventBus.Converters
{
	internal class DateTimeConverter : JavaScriptDateTimeConverter
	{
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			object value = base.ReadJson(reader, objectType, existingValue, serializer);
			return (value is DateTime) ? DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc) : value;
		}
	}
}