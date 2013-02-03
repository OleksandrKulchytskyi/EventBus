using EventBus.Config;
using EventBus.Implementation;
using EventBus.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EvenBus.Test
{
	[TestClass]
	public class ConfigTest
	{
		[TestInitialize()]
		public void Stat()
		{
			if(EventBusConfigSection.CheckConfig)
			{
				EventBusConfigSection.TriggerCreator();
			}
		}

		[TestMethod]
		public void IsSetTest()
		{
			Assert.IsTrue(EventBus.Config.EventBusConfigSection.CheckConfig);
		}

		[TestMethod]
		public void ReadSubscribedAssembliesTest()
		{
			EventBusConfigSection.Current.Subscribers.Assemblies.Cast<AssemblyElement>().ToList()
									.ForEach(x =>
									{
										Assembly assm = Assembly.Load(x.Assembly);
										Assert.IsNotNull(assm);
									});
		}

		[TestMethod]
		public void CreatorFromConfigTest()
		{
			string creatorTypeStr = EventBusConfigSection.Current.CreatorType;
			Assert.IsTrue(!string.IsNullOrEmpty(creatorTypeStr));
			Type creatorType = Type.GetType(creatorTypeStr);
			Assert.IsTrue(creatorType != null);
			object obj = null;
			try
			{
				obj = Activator.CreateInstance(creatorType);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
			}
			Assert.IsTrue(obj is ICreator);
		}

		[TestMethod]
		public void SubscribetoEventsFromConfigTest()
		{
			EventBusConfigSection.Current.Subscribers.Events.Cast<EventTypeElement>().ToList()
									.ForEach(x =>
									{
										Assert.IsTrue(!string.IsNullOrEmpty(x.Name));
										Assert.IsTrue(Type.GetType(x.Name) != null);
									});
		}

		[TestMethod]
		public void TestGenericMethod()
		{
			this.GetType().GetMethod("DoWork")
					.MakeGenericMethod(typeof(string)).Invoke(this, new object[] { "param stuff" });
		}

		public void DoWork<T>(T param)
		{
			Assert.IsTrue(param.Equals("param stuff"));
		}

		[TestMethod]
		public void can_configured_publishers_load_in_configured_assemblies()
		{
			Publishers p = Publishers.Current;
			Assert.IsTrue(p.AssembliesToSearch.Any());
		}

		[TestMethod]
		public void can_configured_publishers_load_creator_from_config()
		{
			Assert.IsTrue(Publishers.Current.Creator is UnityCreator);
		}

		[TestMethod]
		public void can_configured_subscribers_load_in_configured_assemblies()
		{
			Subscribers s = Subscribers.Current;
			Assert.IsTrue(s.AssembliesToSearch.Any());
		}

		[TestMethod]
		public void can_configured_subscribers_load_in_creator()
		{
			Assert.IsTrue(Subscribers.Current.Creator is UnityCreator);
		}
	}
}