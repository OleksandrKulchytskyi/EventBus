using System;

namespace EventBus.Infrastructure
{
	public delegate void SubscriberActivatedHandler(ISubscriber subscriber);

	public delegate void SubscriberStartedExecutionHandler(ISubscriber subscriber, object target);

	public delegate void SubscriberCompletedExecutionHandler(ISubscriber subscriber, object target);

	public delegate void SubscriberExecutionExceptionHandler(ISubscriber subscriber, Exception exception);
}