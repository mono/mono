//
// System.Threading.Timer.cs
//
// Authors:
// 	Dick Porter (dick@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
using System.Collections;

namespace System.Threading
{
#if NET_2_0
	[ComVisible (true)]
#endif
	public sealed class Timer : MarshalByRefObject, IDisposable
	{
#region Timer instance fields
		TimerCallback callback;
		object state;
		long due_time_ms;
		long period_ms;
		long next_run; // in ticks
		bool disposed;
#endregion

#region Timer static fields
		static Thread scheduler;
		static Hashtable jobs;
		static AutoResetEvent change_event;
#endregion

		/* we use a static initializer to avoid race issues with the thread creation */
		static Timer ()
		{
			change_event = new AutoResetEvent (false);
			jobs = new Hashtable ();
			scheduler = new Thread (SchedulerThread);
			scheduler.IsBackground = true;
			scheduler.Start ();
		}

		static long Ticks ()
		{
			return DateTime.GetTimeMonotonic ();
		}

		static private void SchedulerThread ()
		{
			Thread.CurrentThread.Name = "Timer-Scheduler";
			while (true) {
				long min_wait = long.MaxValue;
				lock (jobs) {
					ArrayList expired = null;
					long ticks = Ticks ();
					foreach (Timer t1 in jobs.Keys) {
						if (t1.next_run <= ticks) {
							ThreadPool.QueueUserWorkItem (new WaitCallback (t1.callback), t1.state);
							if (t1.period_ms == -1 || ((t1.period_ms == 0 | t1.period_ms == Timeout.Infinite) && t1.due_time_ms != Timeout.Infinite)) {
								t1.next_run = long.MaxValue;
								if (expired == null)
									expired = new ArrayList ();
								expired.Add (t1);
							} else {
								t1.next_run = ticks + TimeSpan.TicksPerMillisecond * t1.period_ms;
							}
						}
						if (t1.next_run != long.MaxValue) {
							min_wait = Math.Min (min_wait, t1.next_run - ticks);
						}
					}
					if (expired != null) {
						int count = expired.Count;
						for (int i = 0; i < count; ++i) {
							jobs.Remove (expired [i]);
						}
						expired.Clear ();
						if (count > 50)
							expired = null;
					}
				}
				if (min_wait != long.MaxValue) {
					change_event.WaitOne ((int)(min_wait / TimeSpan.TicksPerMillisecond), true);
				} else {
					change_event.WaitOne (Timeout.Infinite, true);
				}
			}
		}

		public Timer (TimerCallback callback, object state, int dueTime, int period)
		{
			if (dueTime < -1)
				throw new ArgumentOutOfRangeException ("dueTime");

			if (period < -1)
				throw new ArgumentOutOfRangeException ("period");

			Init (callback, state, dueTime, period);
		}

		public Timer (TimerCallback callback, object state, long dueTime, long period)
		{
			if (dueTime < -1)
				throw new ArgumentOutOfRangeException ("dueTime");

			if (period < -1)
				throw new ArgumentOutOfRangeException ("period");

			Init (callback, state, dueTime, period);
		}

		public Timer (TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
			: this (callback, state, (long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds)
		{
		}

		[CLSCompliant(false)]
		public Timer (TimerCallback callback, object state, uint dueTime, uint period)
			: this (callback, state, (long) dueTime, (long) period)
		{
		}

#if NET_2_0
		public Timer (TimerCallback callback)
		{
			Init (callback, this, Timeout.Infinite, Timeout.Infinite);
		}
#endif

		void Init (TimerCallback callback, object state, long dueTime, long period)
		{
			this.callback = callback;
			this.state = state;

			Change (dueTime, period);
		}

		public bool Change (int dueTime, int period)
		{
			return Change ((long)dueTime, (long)period);
		}

		public bool Change (long dueTime, long period)
		{
			if(dueTime > 4294967294)
				throw new NotSupportedException ("Due time too large");

			if(period > 4294967294)
				throw new NotSupportedException ("Period too large");

			if (dueTime < -1)
				throw new ArgumentOutOfRangeException ("dueTime");

			if (period < -1)
				throw new ArgumentOutOfRangeException ("period");

			if (disposed)
				return false;

			due_time_ms = dueTime;
			period_ms = period;
			if (dueTime == 0) {
				next_run = Ticks ();
			} else if (dueTime == Timeout.Infinite) {
				next_run = long.MaxValue;
			} else {
				next_run = dueTime * TimeSpan.TicksPerMillisecond + Ticks ();
			}
			lock (jobs) {
				if (next_run != long.MaxValue) {
					Timer t = jobs [this] as Timer;
					if (t == null)
						jobs [this] = this;
					change_event.Set ();
				} else {
					jobs.Remove (this);
				}
			}
			return true;
		}

		public bool Change (TimeSpan dueTime, TimeSpan period)
		{
			return Change ((long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);
		}

		[CLSCompliant(false)]
		public bool Change (uint dueTime, uint period)
		{
			if (dueTime > Int32.MaxValue)
				throw new NotSupportedException ("Due time too large");

			if (period > Int32.MaxValue)
				throw new NotSupportedException ("Period too large");

			return Change ((long) dueTime, (long) period);
		}

		public void Dispose ()
		{
			disposed = true;
			lock (jobs) {
				jobs.Remove (this);
			}
		}

		public bool Dispose (WaitHandle notifyObject)
		{
			Dispose ();
			NativeEventCalls.SetEvent_internal (notifyObject.Handle);
			return true;
		}

	}
}

