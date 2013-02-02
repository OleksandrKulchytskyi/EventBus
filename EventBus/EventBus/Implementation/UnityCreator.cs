using EventBus.Infrastructure;
using EventBus.Logging;
using log4net;
using Microsoft.Practices.Unity;
using System;

namespace EventBus.Implementation
{
	public sealed class UnityCreator : ICreator
	{
		public IUnityContainer Container { get; set; }

		public UnityCreator()
		{
			this.Container = new UnityContainer();
			this.Container.RegisterInstance<ILog>(new Logger());
			DefaultSingleton<ICreator>.Instance = this;
		}

		public UnityCreator(IUnityContainer unityContainer)
		{
			this.Container = unityContainer;
		}

		public object Create(Type type)
		{
			return this.Container.Resolve(type);
		}

		public T Create<T>()
		{
			return this.Container.Resolve<T>();
		}

		public void Drain()
		{
			return;
		}
	}
}