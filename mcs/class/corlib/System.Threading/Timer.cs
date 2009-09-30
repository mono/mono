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

		// timers that expire after FutureTime will be put in future_jobs
		// 5 seconds seems reasonable, this must be at least 1 second
		const long FutureTime = 5 * 1000;
		const long FutureTimeTicks = FutureTime * TimeSpan.TicksPerMillisecond;

#region Timer static fields
		static Thread scheduler;
		static Hashtable jobs;
		static Hashtable future_jobs;
		static Timer future_checker;
		static AutoResetEvent change_event;
		static object locker;
#endregion

		/* we use a static initializer to avoid race issues with the thread creation */
		static Timer ()
		{
			change_event = new AutoResetEvent (false);
			jobs = new Hashtable ();
			future_jobs = new Hashtable ();
			locker = new object ();
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
				long min_next_run = long.MaxValue;
				lock (locker) {
					ArrayList expired = null;
					long ticks = Ticks ();
					bool future_queue_activated = false;
					foreach (Timer t1 in jobs.Keys) {
						if (t1.next_run <= ticks) {
							ThreadPool.QueueUserWorkItem (new WaitCallback (t1.callback), t1.state);
							if (t1.period_ms == -1 || ((t1.period_ms == 0 | t1.period_ms == Timeout.Infinite) && t1.due_time_ms != Timeout.Infinite)) {
								t1.next_run = long.MaxValue;
								if (expired == null)
									expired = new ArrayList ();
								expired.Add (t1);
							} else {
								t1.next_run = Ticks () + TimeSpan.TicksPerMillisecond * t1.period_ms;
								// if it expires too late, postpone to future_jobs
								if (t1.period_ms >= FutureTime) {
									if (future_jobs.Count == 0)
										future_queue_activated = true;
									future_jobs [t1] = t1;
									if (expired == null)
										expired = new ArrayList ();
									expired.Add (t1);
								}
							}
						}
						if (t1.next_run != long.MaxValue) {
							min_next_run = Math.Min (min_next_run, t1.next_run);
						}
					}
					if (future_queue_activated) {
						StartFutureHandler ();
						min_next_run = Math.Min (min_next_run, future_checker.next_run);
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

				const bool exit_context =
#if MONOTOUCH
					// MonoTouch doesn't support remoting,
					// so avoid calling into the remoting infrastructure.
					false;
#else
					true;
#endif

				if (min_next_run != long.MaxValue) {
					long diff = min_next_run - Ticks ();
					if (diff >= 0)
						change_event.WaitOne ((int)(diff / TimeSpan.TicksPerMillisecond), exit_context);
				} else {
					change_event.WaitOne (Timeout.Infinite, exit_context);
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
			if (callback == null)
				throw new ArgumentNullException ("callback");
			
			this.callback = callback;
			this.state = state;

			Change (dueTime, period);
		}

		public bool Change (int dueTime, int period)
		{
			return Change ((long)dueTime, (long)period);
		}

		// FIXME: handle this inside the scheduler, so no additional timer is ever active
		static void CheckFuture (object state) {
			lock (locker) {
				ArrayList moved = null;
				long now = Ticks ();
				foreach (Timer t1 in future_jobs.Keys) {
					if (t1.next_run <= now + FutureTimeTicks) {
						if (moved == null)
							moved = new ArrayList ();
						moved.Add (t1);
						jobs [t1] = t1;
					}
				}
				if (moved != null) {
					int count = moved.Count;
					for (int i = 0; i < count; ++i) {
						future_jobs.Remove (moved [i]);
					}
					moved.Clear ();
					change_event.Set ();
				}
				// no point in keeping this helper timer running
				if (future_jobs.Count == 0) {
					future_checker.Dispose ();
					future_checker = null;
				}
			}
		}

		static void StartFutureHandler ()
		{
			if (future_checker == null)
				future_checker = new Timer (CheckFuture, null, FutureTime - 500, FutureTime - 500);
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
			long now = Ticks ();
			if (dueTime == 0) {
				next_run = now;
			} else if (dueTime == Timeout.Infinite) {
				next_run = long.MaxValue;
			} else {
				next_run = dueTime * TimeSpan.TicksPerMillisecond + now;
			}
			lock (locker) {
				if (next_run != long.MaxValue) {
					bool is_future = next_run - now > FutureTimeTicks;
					Timer t = jobs [this] as Timer;
					if (t == null) {
						t = future_jobs [this] as Timer;
					} else {
						if (is_future) {
							future_jobs [this] = this;
							jobs.Remove (this);
						}
					}
					if (t == null) {
						if (is_future)
							future_jobs [this] = this;
						else
							jobs [this] = this;
					}
					if (is_future)
						StartFutureHandler ();
					change_event.Set ();
				} else {
					jobs.Remove (this);
					future_jobs.Remove (this);
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
			lock (locker) {
				jobs.Remove (this);
				future_jobs.Remove (this);
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

