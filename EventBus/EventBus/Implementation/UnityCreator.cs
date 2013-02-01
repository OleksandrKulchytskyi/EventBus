using EventBus.Infrastructure;
using EventBus.Logging;
using log4net;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus.Implementation
{
	public sealed class UnityCreator:SingletonBase<UnityCreator>,ICreator
	{
		public IUnityContainer Container { get; set; }

		private UnityCreator()
		{
			this.Container = new UnityContainer();
			this.Container.RegisterInstance<ILog>(new Logger());
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
