//
// System.Threading.ThreadPool
//
// Author:
//   Patrik Torstensson
//   Dick Porter (dick@ximian.com)
//   Maurer Dietmar (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Threading {

	public sealed class ThreadPool {

		private ThreadPool ()
		{
			/* nothing to do */
		}

		public static bool BindHandle (IntPtr osHandle)
		{
			throw new NotSupportedException("This is MS specific");
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void GetAvailableThreads (out int workerThreads, out int completionPortThreads);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void GetMaxThreads (out int workerThreads, out int completionPortThreads);
			
		public static bool QueueUserWorkItem (WaitCallback callback)
		{
			IAsyncResult ar = callback.BeginInvoke (null, null, null);
			if (ar == null)
				return false;
			return true;
		}

		public static bool QueueUserWorkItem (WaitCallback callback, object state)
		{
			IAsyncResult ar = callback.BeginInvoke (state, null, null);
			if (ar == null)
				return false;
			return true;
		}

		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callBack,
										object state,
										int millisecondsTimeOutInterval,
										bool executeOnlyOnce)
		{
			return RegisterWaitForSingleObject (waitObject, callBack, state,
							    (long) millisecondsTimeOutInterval, executeOnlyOnce);
		}

		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callBack,
										object state,
										long millisecondsTimeOutInterval,
										bool executeOnlyOnce)
		{
			if (millisecondsTimeOutInterval < -1)
				throw new ArgumentOutOfRangeException ("timeout", "timeout < -1");

			if (millisecondsTimeOutInterval > Int32.MaxValue)
				throw new NotSupportedException ("Timeout is too big. Maximum is Int32.MaxValue");

			TimeSpan timeout = new TimeSpan (0, 0, 0, 0, (int) millisecondsTimeOutInterval);
			
			RegisteredWaitHandle waiter = new RegisteredWaitHandle (waitObject, callBack, state,
										timeout, executeOnlyOnce);
			QueueUserWorkItem (new WaitCallback (waiter.Wait), null);
			return waiter;
		}

		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callBack,
										object state,
										TimeSpan timeout,
										bool executeOnlyOnce)
		{
			return RegisterWaitForSingleObject (waitObject, callBack, state,
							    (long) timeout.TotalMilliseconds, executeOnlyOnce);

		}

		[CLSCompliant(false)]
		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callBack,
										object state,
										uint millisecondsTimeOutInterval,
										bool executeOnlyOnce)
		{
			return RegisterWaitForSingleObject (waitObject, callBack, state,
							    (long) millisecondsTimeOutInterval, executeOnlyOnce);
		}

		public static bool UnsafeQueueUserWorkItem (WaitCallback callback, object state)
		{
			IAsyncResult ar = callback.BeginInvoke (state, null, null);
			if (ar == null)
				return false;
			return true;
		}

	}
}
