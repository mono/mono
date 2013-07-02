//
// System.Threading.ThreadPool.cs
//
// Author:
//   Patrik Torstensson
//   Dick Porter (dick@ximian.com)
//   Maurer Dietmar (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
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

using System.Collections;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading {

	public static class ThreadPool {
		[Obsolete("This method is obsolete, use BindHandle(SafeHandle) instead")]
		public static bool BindHandle (IntPtr osHandle)
		{
			return true;
		}

		public static bool BindHandle (SafeHandle osHandle)
		{
			if (osHandle == null)
				throw new ArgumentNullException ("osHandle");
			
			return true;
		}

#if !NET_2_1		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void GetAvailableThreads (out int workerThreads, out int completionPortThreads);
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void GetMaxThreads (out int workerThreads, out int completionPortThreads);
			
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void GetMinThreads (out int workerThreads, out int completionPortThreads);

		[MonoTODO("The min number of completion port threads is not evaluated.")]
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public static extern bool SetMinThreads (int workerThreads, int completionPortThreads);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public static extern bool SetMaxThreads (int workerThreads, int completionPortThreads);
			
		public static bool QueueUserWorkItem (WaitCallback callBack)
		{
			return QueueUserWorkItem (callBack, null);
		}

		public static bool QueueUserWorkItem (WaitCallback callBack, object state)
		{
			if (callBack == null)
				throw new ArgumentNullException ("callBack");

			if (callBack.IsTransparentProxy ()) {
				IAsyncResult ar = callBack.BeginInvoke (state, null, null);
				if (ar == null)
					return false;
			} else {
				if (!callBack.HasSingleTarget)
					throw new Exception ("The delegate must have only one target");

				AsyncResult ares = new AsyncResult (callBack, state, true);
				pool_queue (ares);
			}
			return true;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void pool_queue (AsyncResult ares);

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
			if (waitObject == null)
				throw new ArgumentNullException ("waitObject");

			if (callBack == null)
				throw new ArgumentNullException ("callBack");
			
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

		[CLSCompliant (false)]
		unsafe public static bool UnsafeQueueNativeOverlapped (NativeOverlapped *overlapped)
		{
			throw new NotImplementedException ();
		}

#if !NET_2_1 || MOBILE

		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static bool UnsafeQueueUserWorkItem (WaitCallback callBack, object state)
		{
			// no stack propagation here (that's why it's unsafe and requires extra security permissions)
			if (!callBack.IsTransparentProxy ()) {
				if (!callBack.HasSingleTarget)
					throw new Exception ("The delegate must have only one target");

				AsyncResult ares = new AsyncResult (callBack, state, false);
				pool_queue (ares);
				return true;
			}
			try {
				if (!ExecutionContext.IsFlowSuppressed ())
					ExecutionContext.SuppressFlow (); // on current thread only
				IAsyncResult ar = callBack.BeginInvoke (state, null, null);
				if (ar == null)
					return false;
			} finally {
				if (ExecutionContext.IsFlowSuppressed ())
					ExecutionContext.RestoreFlow ();
			}
			return true;
		}
		
		[MonoTODO("Not implemented")]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, int millisecondsTimeOutInterval,
			bool executeOnlyOnce) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, long millisecondsTimeOutInterval,
			bool executeOnlyOnce) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, TimeSpan timeout,
			bool executeOnlyOnce) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		[CLSCompliant (false)]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval,
			bool executeOnlyOnce) 
		{
			throw new NotImplementedException ();
		}

#endif
	}
}
