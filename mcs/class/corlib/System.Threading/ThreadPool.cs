//
// System.Threading.ThreadPool
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
using System.Security.Permissions;

namespace System.Threading {

	public sealed class ThreadPool {

		private ThreadPool ()
		{
			/* nothing to do */
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool BindHandleInternal (IntPtr osHandle);

		[SecurityPermission (SecurityAction.Demand, UnmanagedCode=true)]
		public static bool BindHandle (IntPtr osHandle)
		{
			return BindHandleInternal (osHandle);
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void GetAvailableThreads (out int workerThreads, out int completionPortThreads);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void GetMaxThreads (out int workerThreads, out int completionPortThreads);
			
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void GetMinThreads (out int workerThreads, out int completionPortThreads);
			
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public static extern bool SetMinThreads (int workerThreads, int completionPortThreads);
			
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

		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static bool UnsafeQueueUserWorkItem (WaitCallback callback, object state)
		{
			IAsyncResult ar = callback.BeginInvoke (state, null, null);
			if (ar == null)
				return false;
			return true;
		}
		
		[MonoTODO]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, int millisecondsTimeOutInterval,
			bool executeOnlyOnce) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, long millisecondsTimeOutInterval,
			bool executeOnlyOnce) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, TimeSpan timeout,
			bool executeOnlyOnce) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[CLSCompliant (false)]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval,
			bool executeOnlyOnce) 
		{
			throw new NotImplementedException ();
		}
	}
}
