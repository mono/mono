//
// System.Threading.Timer.cs
//
// Authors:
// 	Dick Porter (dick@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
// (C) 2004 Novell, Inc. http://www.novell.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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


namespace System.Threading
{
	public sealed class Timer : MarshalByRefObject, IDisposable
	{
		sealed class Runner : MarshalByRefObject
		{
			ManualResetEvent wait;
			AutoResetEvent start_event;
			TimerCallback callback;
			object state;
			int dueTime;
			int period;
			bool disposed;
			bool aborted;

			public Runner (TimerCallback callback, object state, AutoResetEvent start_event)
			{
				this.callback = callback;
				this.state = state;
				this.start_event = start_event;
				this.wait = new ManualResetEvent (false);
			}

			public int DueTime {
				get { return dueTime; }
				set { dueTime = value; }
			}

			public int Period {
				get { return period; }
				set { period = value == 0 ? Timeout.Infinite : value; }
			}

			bool WaitForDueTime ()
			{
				if (dueTime > 0) {
					bool signaled;
					do {
						wait.Reset ();
						signaled = wait.WaitOne (dueTime, false);
					} while (signaled == true && !disposed && !aborted);

					if (!signaled)
						callback (state);

					if (disposed)
						return false;
				}
				else
					callback (state);

				return true;
			}

			public void Abort ()
			{
				lock (this) {
					aborted = true;
					wait.Set ();
				}
			}
			
			public void Dispose ()
			{
				lock (this) {
					disposed = true;
					Abort ();
				}
			}

			public void Start ()
			{
				while (start_event.WaitOne () && !disposed) {
					aborted = false;

					if (dueTime == Timeout.Infinite)
						continue;

					if (!WaitForDueTime ())
						return;

					if (aborted || (period == Timeout.Infinite))
						continue;

					bool signaled = false;
					while (true) {
						if (disposed)
							return;

						if (aborted)
							break;

						wait.Reset ();
						signaled = wait.WaitOne (period, false);

						if (aborted)
							break;

						if (!signaled) {
							callback (state);
						} else if (!WaitForDueTime ()) {
							return;
						}
					}
				}
			}
		}

		Runner runner;
		AutoResetEvent start_event;
		Thread t;

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

			Init (callback, state, (int) dueTime, (int) period);
		}

		public Timer (TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
			: this (callback, state, Convert.ToInt32(dueTime.TotalMilliseconds), Convert.ToInt32(period.TotalMilliseconds))
		{
		}

		[CLSCompliant(false)]
		public Timer (TimerCallback callback, object state, uint dueTime, uint period)
			: this (callback, state, (long) dueTime, (long) period)
		{
		}

		void Init (TimerCallback callback, object state, int dueTime, int period)
		{
			start_event = new AutoResetEvent (false);
			runner = new Runner (callback, state, start_event);
			Change (dueTime, period);
			t = new Thread (new ThreadStart (runner.Start));
			t.IsBackground = true;
			t.Start ();
		}

		public bool Change (int dueTime, int period)
		{
			if (dueTime < -1)
				throw new ArgumentOutOfRangeException ("dueTime");

			if (period < -1)
				throw new ArgumentOutOfRangeException ("period");

			if (runner == null)
				return false;

			start_event.Reset ();
			runner.Abort ();
			runner.DueTime = dueTime;
			runner.Period = period;
			start_event.Set ();
			return true;
		}

		public bool Change (long dueTime, long period)
		{
			if(dueTime > 4294967294)
				throw new NotSupportedException ("Due time too large");

			if(period > 4294967294)
				throw new NotSupportedException ("Period too large");

			return Change ((int) dueTime, (int) period);
		}

		public bool Change (TimeSpan dueTime, TimeSpan period)
		{
			return Change (Convert.ToInt32(dueTime.TotalMilliseconds), Convert.ToInt32(period.TotalMilliseconds));
		}

		[CLSCompliant(false)]
		public bool Change (uint dueTime, uint period)
		{
			if (dueTime > Int32.MaxValue)
				throw new NotSupportedException ("Due time too large");

			if (period > Int32.MaxValue)
				throw new NotSupportedException ("Period too large");

			return Change ((int) dueTime, (int) period);
		}

		public void Dispose ()
		{
			if (t != null && t.IsAlive) {
				if (t != Thread.CurrentThread)
					t.Abort ();
				t = null;
			}
			runner.Dispose ();
			runner = null;
			GC.SuppressFinalize (this);
		}

		public bool Dispose (WaitHandle notifyObject)
		{
			Dispose ();
			NativeEventCalls.SetEvent_internal (notifyObject.Handle);
			return true;
		}

		~Timer ()
		{
			if (t != null && t.IsAlive)
				t.Abort ();

			if (runner != null)
				runner.Abort ();
		}
	}
}

