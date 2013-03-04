using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using EventBus.Extensions;

namespace EventBus.Test
{
	[TestClass]
	public class WeakEventHandlerTest
	{
		[TestMethod]
		public void ShouldHandleEventWhenBothReferencesAreAlive()
		{
			var alarm = new Alarm();
			var sleepy = new Sleepy(alarm);
			alarm.Beep();
			alarm.Beep();

			Assert.AreEqual(2, sleepy.SnoozeCount);
		}

		[TestMethod]
		public void ShouldAllowSubscriberReferenceToBeCollected()
		{
			var alarm = new Alarm();
			var sleepyReference = (null as WeakReference);
			new Action(() =>
			{
				// Run this in a delegate to that the local variable gets garbage collected
				var sleepy = new Sleepy(alarm);
				alarm.Beep();
				alarm.Beep();
				Assert.AreEqual(2, sleepy.SnoozeCount);
				sleepyReference = new WeakReference(sleepy);
			})();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.IsNull(sleepyReference.Target);
		}

		[TestMethod]
		public void SubscriberShouldNotBeUnsubscribedUntilCollection()
		{
			var alarm = new Alarm();
			var sleepy = new Sleepy(alarm);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			alarm.Beep();
			alarm.Beep();
			Assert.AreEqual(2, sleepy.SnoozeCount);
		}
	}

	public class Alarm
	{
		public event PropertyChangedEventHandler Beeped;

		public void Beep()
		{
			var handler = Beeped;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs("Beep!"));
		}
	}

	public class Sleepy
	{
		private readonly Alarm _alarm;
		private int _snoozeCount;

		public Sleepy(Alarm alarm)
		{
			_alarm = alarm;
			_alarm.Beeped += new WeakEventHandler<PropertyChangedEventArgs>(Alarm_Beeped).Handler;
		}

		private void Alarm_Beeped(object sender, PropertyChangedEventArgs e)
		{
			_snoozeCount++;
		}

		public int SnoozeCount
		{
			get { return _snoozeCount; }
		}
	}
}
