using System;

namespace EventBus.Infrastructure
{
	public interface ICreator : IClearable
	{
		T Create<T>();

		object Create(Type t);

	}
}