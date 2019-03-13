//
// System.Threading.Timer.cs
//
// Authors:
// 	Dick Porter (dick@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace System.Threading
{
#if WASM
	internal static class WasmRuntime {
		static Dictionary<int, Action> callbacks;
		static int next_id;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void SetTimeout (int timeout, int id);

		internal static void ScheduleTimeout (int timeout, Action action) {
			if (callbacks == null)
				callbacks = new Dictionary<int, Action> ();
			int id = ++next_id;
			callbacks [id] = action;
			SetTimeout (timeout, id);
		}

		//XXX Keep this in sync with mini-wasm.c:mono_set_timeout_exec
		static void TimeoutCallback (int id) {
			var cb = callbacks [id];
			callbacks.Remove (id);
			cb ();
		}
	}
#endif


	[ComVisible (true)]
	public sealed class Timer
		: MarshalByRefObject, IDisposable, IAsyncDisposable
	{
		static Scheduler scheduler => Scheduler.Instance;
#region Timer instance fields
		TimerCallback callback;
		object state;
		long due_time_ms;
		long period_ms;
		long next_run; // in ticks. Only 'Scheduler' can change it except for new timers without due time.
		bool disposed;
		bool is_dead, is_added;
#endregion
		public Timer (TimerCallback callback, object state, int dueTime, int period)
		{
			Init (callback, state, dueTime, period);
		}

		public Timer (TimerCallback callback, object state, long dueTime, long period)
		{
			Init (callback, state, dueTime, period);
		}

		public Timer (TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
		{
			Init (callback, state, (long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);
		}

		[CLSCompliant(false)]
		public Timer (TimerCallback callback, object state, uint dueTime, uint period)
		{
			// convert all values to long - with a special case for -1 / 0xffffffff
			long d = (dueTime == UInt32.MaxValue) ? Timeout.Infinite : (long) dueTime;
			long p = (period == UInt32.MaxValue) ? Timeout.Infinite : (long) period;
			Init (callback, state, d, p);
		}

		public Timer (TimerCallback callback)
		{
			Init (callback, this, Timeout.Infinite, Timeout.Infinite);
		}

		void Init (TimerCallback callback, object state, long dueTime, long period)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");
			
			this.callback = callback;
			this.state = state;
			this.is_dead = false;
			this.is_added = false;

			Change (dueTime, period, true);
		}

		public bool Change (int dueTime, int period)
		{
			return Change (dueTime, period, false);
		}

		public bool Change (TimeSpan dueTime, TimeSpan period)
		{
			return Change ((long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds, false);
		}

		[CLSCompliant(false)]
		public bool Change (uint dueTime, uint period)
		{
			// convert all values to long - with a special case for -1 / 0xffffffff
			long d = (dueTime == UInt32.MaxValue) ? Timeout.Infinite : (long) dueTime;
			long p = (period == UInt32.MaxValue) ? Timeout.Infinite : (long) period;
			return Change (d, p, false);
		}

		public void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;
			scheduler.Remove (this);
		}

		public bool Change (long dueTime, long period)
		{
			return Change (dueTime, period, false);
		}

		const long MaxValue = UInt32.MaxValue - 1;

		bool Change (long dueTime, long period, bool first)
		{
			if (dueTime > MaxValue)
				throw new ArgumentOutOfRangeException ("dueTime", "Due time too large");

			if (period > MaxValue)
				throw new ArgumentOutOfRangeException ("period", "Period too large");

			// Timeout.Infinite == -1, so this accept everything greater than -1
			if (dueTime < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("dueTime");

			if (period < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("period");

			if (disposed)
				throw new ObjectDisposedException (null, Environment.GetResourceString ("ObjectDisposed_Generic"));

			due_time_ms = dueTime;
			period_ms = period;
			long nr;
			if (dueTime == 0) {
				nr = 0; // Due now
			} else if (dueTime < 0) { // Infinite == -1
				nr = long.MaxValue;
				/* No need to call Change () */
				if (first) {
					next_run = nr;
					return true;
				}
			} else {
				nr = dueTime * TimeSpan.TicksPerMillisecond + GetTimeMonotonic ();
			}

			scheduler.Change (this, nr);
			return true;
		}

		public bool Dispose (WaitHandle notifyObject)
		{
			if (notifyObject == null)
				throw new ArgumentNullException ("notifyObject");
			Dispose ();
			NativeEventCalls.SetEvent (notifyObject.SafeWaitHandle);
			return true;
		}

		public ValueTask DisposeAsync ()
		{
			Dispose ();
			return new ValueTask (Task.FromResult<object> (null));
		}

		// extracted from ../../../../external/referencesource/mscorlib/system/threading/timer.cs
		internal void KeepRootedWhileScheduled()
		{
		}

		// TODO: Environment.TickCount should be enough as is everywhere else
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern long GetTimeMonotonic ();

		struct TimerComparer : IComparer, IComparer<Timer> {
			int IComparer.Compare (object x, object y)
			{
				if (x == y)
					return 0;
				Timer tx = (x as Timer);
				if (tx == null)
					return -1;
				Timer ty = (y as Timer);
				if (ty == null)
					return 1;
				return Compare(tx, ty);
			}

			public int Compare (Timer tx, Timer ty)
			{
				long result = tx.next_run - ty.next_run;
				return (int)Math.Sign(result);
			}
		}

		sealed class Scheduler {
			static readonly Scheduler instance = new Scheduler ();
			
			volatile bool needReSort = true;
			List<Timer> list;
			long current_next_run = Int64.MaxValue;

#if WASM
			bool scheduled_zero;

			void InitScheduler () {
			}

			void WakeupScheduler () {
				if (!scheduled_zero) {
					WasmRuntime.ScheduleTimeout (0, this.RunScheduler);
					scheduled_zero = true;
				}
			}

			void RunScheduler() {
				scheduled_zero = false;
				int ms_wait = RunSchedulerLoop ();
				if (ms_wait >= 0) {
					WasmRuntime.ScheduleTimeout (ms_wait, this.RunScheduler);
					if (ms_wait == 0)
						scheduled_zero = true;
				}
			}
#else
			ManualResetEvent changed;

			void InitScheduler () {
				changed = new ManualResetEvent (false);
				Thread thread = new Thread (SchedulerThread);
				thread.IsBackground = true;
				thread.Start ();
			}

			void WakeupScheduler () {
				changed.Set ();
			}

			void SchedulerThread ()
			{
				Thread.CurrentThread.Name = "Timer-Scheduler";
				while (true) {
					int ms_wait = -1;
					lock (this) {
						changed.Reset ();
						ms_wait = RunSchedulerLoop ();
					}
					// Wait until due time or a timer is changed and moves from/to the first place in the list.
					changed.WaitOne (ms_wait);
				}
			}

#endif
			public static Scheduler Instance {
				get { return instance; }
			}

			private Scheduler ()
			{
				list = new List<Timer> (1024);
				InitScheduler ();
			}

			public void Remove (Timer timer)
			{
				lock (this) {
					// If this is the next item due (index = 0), the scheduler will wake up and find nothing.
					// No need to Pulse ()
					InternalRemove (timer);
				}
			}

			public void Change (Timer timer, long new_next_run)
			{
				if (timer.is_dead)
					timer.is_dead = false;

				bool wake = false;
				lock (this) {
					needReSort = true;

					if (!timer.is_added) {
						timer.next_run = new_next_run;
						Add(timer);
						wake = current_next_run > new_next_run;
					} else {
						if (new_next_run == Int64.MaxValue) {
							timer.next_run = new_next_run;
							InternalRemove (timer);
							return;
						}

						if (!timer.disposed) {
							// We should only change next_run after removing and before adding
							timer.next_run = new_next_run;
							// FIXME
							wake = current_next_run > new_next_run;
						}
					}
				}
				if (wake)
					WakeupScheduler();
			}

			// This should be the only caller to list.Add!
			void Add (Timer timer)
			{
				timer.is_added = true;
				needReSort = true;
				list.Add (timer);
				if (list.Count == 1)
					WakeupScheduler();
				//PrintList ();
			}

			void InternalRemove (Timer timer)
			{
				timer.is_dead = true;
				needReSort = true;
			}

			static void TimerCB (object o)
			{
				Timer timer = (Timer) o;
				timer.callback (timer.state);
			}

			void FireTimer (Timer timer) {
				long period = timer.period_ms;
				long due_time = timer.due_time_ms;
				bool no_more = (period == -1 || ((period == 0 || period == Timeout.Infinite) && due_time != Timeout.Infinite));
				if (no_more) {
					timer.next_run = Int64.MaxValue;
					timer.is_dead = true;
				} else {
					timer.next_run = GetTimeMonotonic () + TimeSpan.TicksPerMillisecond * timer.period_ms;
					timer.is_dead = false;
				}
				ThreadPool.UnsafeQueueUserWorkItem (TimerCB, timer);
			}

			int RunSchedulerLoop () {
				int ms_wait = -1;
				int i;
				long ticks = GetTimeMonotonic ();
				var comparer = new TimerComparer();

				if (needReSort) {
					list.Sort(comparer);
					needReSort = false;
				}

				long min_next_run = Int64.MaxValue;

				for (i = 0; i < list.Count; i++) {
					Timer timer = list[i];
					if (timer.is_dead)
						continue;

					if (timer.next_run <= ticks) {
						FireTimer(timer);
					}

					min_next_run = Math.Min(min_next_run, timer.next_run);

					if ((timer.next_run > ticks) && (timer.next_run < Int64.MaxValue))
						timer.is_dead = false;
				}

				for (i = 0; i < list.Count; i++) {
					Timer timer = list[i];
					if (!timer.is_dead)
						continue;
					
					timer.is_added = false;
					needReSort = true;
					list[i] = list[list.Count - 1];
					i--;
					list.RemoveAt(list.Count - 1);

					if (list.Count == 0)
						break;
				}

				if (needReSort) {
					list.Sort(comparer);
					needReSort = false;
				}

				//PrintList ();
				ms_wait = -1;
				current_next_run = min_next_run;
				if (min_next_run != Int64.MaxValue) {
					long diff = (min_next_run - GetTimeMonotonic ())  / TimeSpan.TicksPerMillisecond;
					if (diff > Int32.MaxValue)
						ms_wait = Int32.MaxValue - 1;
					else {
						ms_wait = (int)(diff);
						if (ms_wait < 0)
							ms_wait = 0;
					}
				}
				return ms_wait;
			}

			/*
			void PrintList ()
			{
				Console.WriteLine ("BEGIN--");
				for (int i = 0; i < list.Count; i++) {
					Timer timer = (Timer) list.GetByIndex (i);
					Console.WriteLine ("{0}: {1}", i, timer.next_run);
				}
				Console.WriteLine ("END----");
			}
			*/
		}
	}
}

