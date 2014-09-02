//
// System.Timers.Timer
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// The docs talk about server timers and such...

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.Threading;

namespace System.Timers
{
	[DefaultEventAttribute("Elapsed")]
	[DefaultProperty("Interval")]
	public class Timer : Component, ISupportInitialize {
		double interval;
		bool autoReset;
		System.Threading.Timer timer;
		object _lock = new object ();
		ISynchronizeInvoke so;
		bool enabled;

		[Category("Behavior")]
		[TimersDescription("Occurs when the Interval has elapsed.")]
		public event ElapsedEventHandler Elapsed;

		public Timer () : this (100)
		{
		}

		public Timer (double interval)
		{
			// MSBUG: https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=296761
			if (interval <= 0 || interval > 0x7FFFFFFF)
				throw new ArgumentException ("Invalid value: " + interval, "interval");

			autoReset = true;
			timer = new System.Threading.Timer (Callback, this, Timeout.Infinite, Timeout.Infinite); //disabled
			Interval = interval;
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
			get {
				lock (_lock)
					return enabled && timer != null;
			}
			set {
				lock (_lock) {
					if (timer == null)
						throw new ObjectDisposedException (GetType ().ToString (), "The object has been disposed");
                    
					if (enabled == value)
						return;

					if (value) {
						// As per MS docs (throw this only when the timer becomes enabled): http://msdn.microsoft.com/en-us/library/system.timers.timer.enabled(v=vs.110).aspx
						if (interval > Int32.MaxValue)
							throw new ArgumentException ("Invalid value: " + interval, "interval");
						enabled = true;
						timer.Change ((int)interval, autoReset ? (int)interval : 0);
					} else {
						enabled = false;
						timer.Change (Timeout.Infinite, Timeout.Infinite);
					}
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
					throw new ArgumentException ("Invalid value: " + value);
				// As per MS docs (throw only if enabled, otherwise postpone throwing until it becomes enabled): http://msdn.microsoft.com/en-us/library/system.timers.timer.interval(v=vs.110).aspx
				if (value > Int32.MaxValue && enabled)
					throw new ArgumentException ("Invalid value: " + value);

				lock (_lock) {
					if (timer == null)
						return;
					interval = value;
					//call Change only if enabled, otherwise it will be called when Enabled = true, see the comment above on throwing ArgumentException
					if (enabled)
						timer.Change ((int)interval, autoReset? (int)interval: 0);
				}
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
		[Browsable (false)]
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
			lock (_lock)
				Dispose (true);
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
			// Could call Close() twice
			if (timer == null)
				return;

			// If we're disposing explicitly, clear all
			// fields. If not, all fields will have been
			// nulled by the GC during finalization, so
			// trying to lock on _lock will blow up.
			if (disposing)
			{
				timer.Dispose ();
				timer = null;
			}

			base.Dispose (disposing);
		}

		static void Callback (object state)
		{
			Timer timer = (Timer) state;
			if (timer.Enabled == false)
				return;
			ElapsedEventHandler events = timer.Elapsed;

			try
			{
				if (!timer.autoReset)
					timer.Enabled = false; //this could throw ObjectDisposed if timer.Close() was just called, after the check for Enabled above
			}
			catch (ObjectDisposedException) {
				//Probably the Elapsed event should not fire if this Timer is found here to be closed
				return;
			}

			//If another thread calls Close() when this thread is right here (of further down), the Elapsed event might get called once more after this Timer was Closed()
			//It's not a problem, it happens with all Timers, but it's good to know...

			if (events == null)
				return;

			ElapsedEventArgs arg = new ElapsedEventArgs (DateTime.Now);

			if (timer.so != null && timer.so.InvokeRequired) {
				timer.so.BeginInvoke (events, new object [2] {timer, arg});
			} else {
				try {
					events (timer, arg);
				} catch {
				}
			}
		}

	}
}
