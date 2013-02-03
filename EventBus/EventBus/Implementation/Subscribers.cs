using EventBus.Infrastructure;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EventBus.Implementation
{
	public class Subscribers
	{
		public static Subscribers Current { get; private set; }

		static Subscribers()
		{
			Subscribers.Current = new Subscribers();
		}

		public ICreator Creator { get; set; }

		public List<Assembly> AssembliesToSearch { get; set; }

		internal ILog Logger { get; set; }

		internal event SubscriberStartedExecutionHandler SubscriberStartedExecution;

		internal event SubscriberActivatedHandler SubscriberActivated;

		internal event SubscriberCompletedExecutionHandler SubscriberCompletedExecution;

		internal event SubscriberExecutionExceptionHandler SubscriberExecutionException;

		protected Subscribers()
		{
			this.AssembliesToSearch = new List<Assembly>();
			this.Creator = new DefaultCreator();
			this.Logger = new Logging.Logger();
		}

		private List<ISubscriber<E>> SearchAssembliesForSubscribers<E>()
		{
			List<ISubscriber<E>> ret = new List<ISubscriber<E>>();

			try
			{
				this.AssembliesToSearch.ForEach((assembly) =>
				{
					assembly.GetTypes()
						.Where(a => typeof(ISubscriber<E>).IsAssignableFrom(a) && !a.IsAbstract).ToList()
						.ForEach((subscriberType) =>
						{
							var subscriber = (ISubscriber<E>)this.Creator.Create(subscriberType);
							this.OnSubscriberActivated(subscriber);
							ret.Add(subscriber);
						});
				});
			}
			catch (Exception ex)
			{
				this.Logger.Error(string.Format("Error during assembly search for '{0}' subscribers", typeof(E).Name), ex);
			}

			return ret;
		}

		public void Unsubscribe<E>()
		{
			var subs = this.SearchAssembliesForSubscribers<E>();

			subs.ForEach((sub) =>
			{
				if (sub as IUnsubscribe != null)
				{
					((IUnsubscribe)sub).Unsubscribe();
				}
			});
		}

		private void Unsubscribe(IUnsubscribe unsubscriber)
		{
			try
			{
				Logger.DebugFormat("Attempting to unsubscribe subscriber of type [{0}]", unsubscriber.GetType().FullName);
				unsubscriber.Unsubscribe();
			}
			catch (Exception e)
			{
				Logger.Error(String.Format("Exception was raised while unsubscribing subscriber of type [{0}]",
					unsubscriber.GetType()), e);
			}
		}

		private void Handle<E>(BusEventArgs<E> eventToHandle, ISubscriber<E> subscriber)
		{
			try
			{
				this.OnSubscriberStarted(subscriber, eventToHandle.Data);
				subscriber.Handle(eventToHandle.Data);
			}
			catch (Exception exception)
			{
				this.OnSubcriberExecutionException(subscriber, exception);
			}
		}

		public Subscribers Subscribe<E>()
		{
			string dbg = string.Format("Searching for ISubscriber implementations for '{0}'", typeof(E).Name);

			this.Logger.Debug(dbg);

			var subs = this.SearchAssembliesForSubscribers<E>();

			subs.ForEach((sub) =>
			{
				sub.Subscribe();

				sub.EventReceived += (sender, e) =>
				{
					subs.ForEach((subscriber) =>
					{
						this.Handle(e, subscriber);
					});
				};

				sub.EventHandled += (sender, e) =>
				{
					this.OnSubscriberCompleted((ISubscriber<E>)sender, e.Data);
				};
			});

			WireDriver.Stopping += (sender, e) => subs.Where(sub => sub is IUnsubscribe)
														.Cast<IUnsubscribe>().ToList()
														.ForEach(Unsubscribe);
			return this;
		}

		protected virtual void OnSubscriberActivated(ISubscriber subscriber)
		{
			if (this.SubscriberActivated != null)
				this.SubscriberActivated(subscriber);
		}

		protected virtual void OnSubscriberStarted(ISubscriber subscriber, object target)
		{
			if (this.SubscriberStartedExecution != null)
				this.SubscriberStartedExecution(subscriber, target);
		}

		protected virtual void OnSubscriberCompleted(ISubscriber subscriber, object target)
		{
			if (this.SubscriberCompletedExecution != null)
				this.SubscriberCompletedExecution(subscriber, target);
		}

		protected virtual void OnSubcriberExecutionException(ISubscriber subscriber, Exception exception)
		{
			if (this.SubscriberExecutionException != null)
				this.SubscriberExecutionException(subscriber, exception);
		}

		public Subscribers WithSubscriberCompletedAction(SubscriberCompletedExecutionHandler handler)
		{
			this.SubscriberCompletedExecution += handler;
			return this;
		}

		public Subscribers WithSubscriberActivatedAction(SubscriberActivatedHandler handler)
		{
			this.SubscriberActivated += handler;
			return this;
		}

		public Subscribers WithSubscriberStartedAction(SubscriberStartedExecutionHandler handler)
		{
			this.SubscriberStartedExecution += handler;
			return this;
		}

		public Subscribers WithSubscriberExceptionHandler(SubscriberExecutionExceptionHandler handler)
		{
			this.SubscriberExecutionException += handler;
			return this;
		}
	}
}