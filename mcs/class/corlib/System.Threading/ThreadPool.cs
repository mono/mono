//
// System.Threading.ThreadPool.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public sealed class ThreadPool
	{
		[MonoTODO]
		public static bool BindHandle(IntPtr osHandle) {
			// FIXME
			return(false);
		}

		[MonoTODO]
		public static bool QueueUserWorkItem(WaitCallback callback) {
			// FIXME
			return(false);
		}

		[MonoTODO]
		public static bool QueueUserWorkItem(WaitCallback callback,
						     object state) {
			// FIXME
			return(false);
		}

		[MonoTODO]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject,
									       WaitOrTimerCallback callback,
									       object state,
									       int millisecondsTimeOutInterval,
									       bool executeOnlyOnce) {
			if(millisecondsTimeOutInterval < -1) {
				throw new ArgumentOutOfRangeException("timeout < -1");
			}
			// FIXME
			return(null);
		}

		[MonoTODO]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject,
									       WaitOrTimerCallback callback,
									       object state,
									       long millisecondsTimeOutInterval,
									       bool executeOnlyOnce) {
			if(millisecondsTimeOutInterval < -1) {
				throw new ArgumentOutOfRangeException("timeout < -1");
			}
			// FIXME
			return(null);
		}

		[MonoTODO]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, TimeSpan timeout, bool executeOnlyOnce) {
			// LAMESPEC: I assume it means "timeout" when it says "millisecondsTimeOutInterval"
			if(timeout.Milliseconds < -1) {
				throw new ArgumentOutOfRangeException("timeout < -1");
			}
			if(timeout.Milliseconds > Int32.MaxValue) {
				throw new NotSupportedException("timeout too large");
			}
			// FIXME
			return(null);
		}

		[CLSCompliant(false)][MonoTODO]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce) {
			// FIXME
			return(null);
		}

		[MonoTODO]
		public static bool UnsafeQueueUserWorkItem(WaitCallback callback, object state) {
			// FIXME
			return(false);
		}

		[MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, int millisecondsTimeOutInterval, bool executeOnlyOnce) {
			// FIXME
			return(null);
		}

		[MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, long millisecondsTimeOutInterval, bool executeOnlyOnce) {
			// FIXME
			return(null);
		}

		[MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, TimeSpan timeout, bool executeOnlyOnce) {
			// FIXME
			return(null);
		}

		[CLSCompliant(false)][MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce) {
			// FIXME
			return(null);
		}
	}
}
