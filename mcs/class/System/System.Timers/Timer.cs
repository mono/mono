//
// System.Timers.Timer
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
// The docs talk about server timers and such...

using System;
using System.ComponentModel;
using System.Threading;

namespace System.Timers
{
	[DefaultEventAttribute("Elapsed")]
	[DefaultProperty("Interval")]
	public class Timer : Component, ISupportInitialize
	{
		bool autoReset;
		bool enabled;
		double interval;
		ISynchronizeInvoke so;
		ManualResetEvent wait;

		[Category("Behavior")]
		[TimersDescription("Occurs when the Interval has elapsed.")]
		public event ElapsedEventHandler Elapsed;

		public Timer () : this (100)
		{
		}

		public Timer (double interval)
		{
			autoReset = true;
			enabled = false;
			Interval = interval;
			so = null;
			wait = null;
		}


		[Category("Behavior")]
		[DefaultValue(true)]
		[TimersDescription("Indicates whether the timer will be restarted when it is enabled.")]
		public bool AutoReset
		{
			get { return autoReset; }
			set { autoReset = value; }
		}

		[Category("Behavior")]
		[DefaultValue(false)]
		[TimersDescription("Indicates whether the timer is enabled to fire events at a defined interval.")]
		public bool Enabled
		{
			get { return enabled; }
			set {
				if (enabled == value)
					return;

				enabled = value;
				if (value) {
					Thread t = new Thread (new ThreadStart (StartTimer));
					t.Start ();
				} else {
					StopTimer ();
				}
			}
		}

		[Category("Behavior")]
		[DefaultValue(100)]
		[RecommendedAsConfigurable(true)]
		[TimersDescription( "The number of milliseconds between timer events.")]
		public double Interval
		{
			get { return interval; }
			set { 
				// The doc says 'less than 0', but 0 also throws the exception
				if (value <= 0)
					throw new ArgumentException ("Invalid value: " + interval, "interval");

				interval = value;
			}
		}

		public override ISite Site
		{
			get { return base.Site; }
			set { base.Site = value; }
		}

		[DefaultValue(null)]
		[TimersDescriptionAttribute("The object used to marshal the event handler calls issued " +
					    "when an interval has elapsed.")]
		public ISynchronizeInvoke SynchronizingObject
		{
			get { return so; }
			set { so = value; }
		}

		public void BeginInit ()
		{
			// Nothing to do
		}

		public void Close ()
		{
			StopTimer ();
		}

		public void EndInit ()
		{
			// Nothing to do
		}

		public void Start ()
		{
			Enabled = true;
		}

		public void Stop ()
		{
			Enabled = false;
		}

		protected override void Dispose (bool disposing)
		{
			Close ();
			base.Dispose (disposing);
		}

		static void Callback (object state)
		{
			Timer timer = (Timer) state;
			if (timer.autoReset == false)
				timer.enabled = false;

			if (timer.Elapsed == null)
				return;

			ElapsedEventArgs arg = new ElapsedEventArgs (DateTime.Now);

			if (timer.so != null && timer.so.InvokeRequired) {
				timer.so.BeginInvoke (timer.Elapsed, new object [2] {timer, arg});
			} else {
				timer.Elapsed (timer, arg);
			}
		}

		void StartTimer ()
		{
			wait = new ManualResetEvent (false);

			WaitCallback wc = new WaitCallback (Callback);
			while (enabled && wait.WaitOne ((int) interval, false) == false)
				ThreadPool.QueueUserWorkItem (wc, this);
			
			wc = null;
			((IDisposable) wait).Dispose ();
			wait = null;
		}

		void StopTimer ()
		{
			if (wait != null)
				wait.Set ();
		}
	}
}

