//
// System.Threading.Timer.cs
//
// Authors:
// 	Dick Porter (dick@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public sealed class Timer : MarshalByRefObject, IDisposable
	{
		sealed class Runner : MarshalByRefObject, IDisposable
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

				return true;
			}

			public void Abort ()
			{
				lock (this) {
					aborted = true;
					wait.Set ();
				}
			}

			public void Start ()
			{
				while (start_event.WaitOne ()) {
					aborted = false;

					if (dueTime == Timeout.Infinite)
						continue;

					if (!WaitForDueTime ())
						return;

					if (aborted || (period == Timeout.Infinite))
						continue;

					bool signaled = false;
					while (true) {
						wait.Reset ();
						signaled = wait.WaitOne (period, false);
						if (disposed)
							return;

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

			public void Dispose ()
			{
				Dispose (true);
			}

			void Dispose (bool disposing)
			{
				disposed = true;
				if (wait != null) {
					wait.Set ();
					Thread.Sleep (100);
					((IDisposable) wait).Dispose ();
					wait = null;
				}

				if (disposing)
					GC.SuppressFinalize (this);
			}

			~Runner ()
			{
				Dispose (false);
			}
		}

		Runner runner;
		AutoResetEvent start_event;

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
			: this (callback, state, dueTime.Milliseconds, period.Milliseconds)
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
			Thread t = new Thread (new ThreadStart (runner.Start));
			t.Start ();
		}

		[MonoTODO("false return?")]
		public bool Change (int dueTime, int period)
		{
			if (dueTime < -1)
				throw new ArgumentOutOfRangeException ("dueTime");

			if (period < -1)
				throw new ArgumentOutOfRangeException ("period");

			runner.DueTime = dueTime;
			runner.Period = period;
			runner.Abort ();
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
			return Change (dueTime.Milliseconds, period.Milliseconds);
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
			runner.Dispose ();
			runner = null;
			GC.SuppressFinalize (this);
		}

		[MonoTODO("How do we signal the handler?")]
		public bool Dispose (WaitHandle notifyObject)
		{
			Dispose ();
			return true; //FIXME
		}

		~Timer ()
		{
			runner = null;
		}
	}
}

