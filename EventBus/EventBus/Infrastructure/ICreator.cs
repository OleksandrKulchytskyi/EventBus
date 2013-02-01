using System;

namespace EventBus.Infrastructure
{
	public interface ICreator
	{
		T Create<T>();

		object Create(Type t);

		void Drain();
	}
}