//
// System.Threading.Timer.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public sealed class Timer : IDisposable
	{
		[MonoTODO]
		public Timer(TimerCallback callback, object state, int dueTime, int period) {
			if(dueTime < -1) {
				throw new ArgumentOutOfRangeException("Due time < -1");
			}
			if(period < -1) {
				throw new ArgumentOutOfRangeException("Period < -1");
			}
			
			// FIXME
		}

		[MonoTODO]
		public Timer(TimerCallback callback, object state, long dueTime, long period) {
			if(dueTime < -1) {
				throw new ArgumentOutOfRangeException("Due time < -1");
			}
			if(period < -1) {
				throw new ArgumentOutOfRangeException("Period < -1");
			}
			// FIXME
		}

		[MonoTODO]
		public Timer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period) {
			if(dueTime.Milliseconds < 0 || dueTime.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("Due time out of range");
			}
			if(period.Milliseconds < 0 || period.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("Period out of range");
			}
			// FIXME
		}

		[CLSCompliant(false)][MonoTODO]
		public Timer(TimerCallback callback, object state, uint dueTime, uint period) {
			// FIXME
		}

		[MonoTODO]
		public bool Change(int dueTime, int period) {
			if(dueTime < -1) {
				throw new ArgumentOutOfRangeException("Due time < -1");
			}
			if(period < -1) {
				throw new ArgumentOutOfRangeException("Period < -1");
			}
			// FIXME
			return(false);
		}

		[MonoTODO]
		public bool Change(long dueTime, long period) {
			if(dueTime < -1) {
				throw new ArgumentOutOfRangeException("Due time < -1");
			}
			if(period < -1) {
				throw new ArgumentOutOfRangeException("Period < -1");
			}
			if(dueTime > 4294967294) {
				throw new NotSupportedException("Due time too large");
			}
			if(period > 4294967294) {
				throw new NotSupportedException("Period too large");
			}
			// FIXME
			return(false);
		}

		[MonoTODO]
		public bool Change(TimeSpan dueTime, TimeSpan period) {
			// FIXME
			return(false);
		}

		[CLSCompliant(false)][MonoTODO]
		public bool Change(uint dueTime, uint period) {
			// FIXME
			return(false);
		}

		[MonoTODO]
		public void Dispose() {
			// FIXME
		}

		[MonoTODO]
		public bool Dispose(WaitHandle notifyObject) {
			// FIXME
			return(false);
		}

		[MonoTODO]
		~Timer() {
			// FIXME
		}
	}
}
