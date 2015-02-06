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
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;

namespace System.Threading {

	public static class ThreadPool {

		[Obsolete("This method is obsolete, use BindHandle(SafeHandle) instead")]
		public static bool BindHandle (IntPtr osHandle)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.BindHandle (osHandle);
			else
				return true;
		}

		public static bool BindHandle (SafeHandle osHandle)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool) {
				return Microsoft.ThreadPool.BindHandle (osHandle);
			} else {
				if (osHandle == null)
					throw new ArgumentNullException ("osHandle");
			
				return true;
			}
		}

		public static void GetAvailableThreads (out int workerThreads, out int completionPortThreads)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				Microsoft.ThreadPool.GetAvailableThreads (out workerThreads, out completionPortThreads);
			else
				GetAvailableThreads_internal (out workerThreads, out completionPortThreads);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void GetAvailableThreads_internal (out int workerThreads, out int completionPortThreads);

		public static void GetMaxThreads (out int workerThreads, out int completionPortThreads)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				Microsoft.ThreadPool.GetMaxThreads (out workerThreads, out completionPortThreads);
			else
				GetMaxThreads_internal (out workerThreads, out completionPortThreads);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void GetMaxThreads_internal (out int workerThreads, out int completionPortThreads);

		public static void GetMinThreads (out int workerThreads, out int completionPortThreads)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				Microsoft.ThreadPool.GetMinThreads (out workerThreads, out completionPortThreads);
			else
				GetMinThreads_internal (out workerThreads, out completionPortThreads);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void GetMinThreads_internal (out int workerThreads, out int completionPortThreads);

		[MonoTODO("The min number of completion port threads is not evaluated.")]
		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public static bool SetMinThreads (int workerThreads, int completionPortThreads)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.SetMinThreads (workerThreads, completionPortThreads);
			else
				return SetMinThreads_internal (workerThreads, completionPortThreads);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool SetMinThreads_internal (int workerThreads, int completionPortThreads);

		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public static bool SetMaxThreads (int workerThreads, int completionPortThreads)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.SetMaxThreads (workerThreads, completionPortThreads);
			else
				return SetMaxThreads_internal (workerThreads, completionPortThreads);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool SetMaxThreads_internal (int workerThreads, int completionPortThreads);

		public static bool QueueUserWorkItem (WaitCallback callBack)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.QueueUserWorkItem (callBack, null);
			else
				return QueueUserWorkItem (callBack, null);
		}

		public static bool QueueUserWorkItem (WaitCallback callBack, object state)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool) {
				return Microsoft.ThreadPool.QueueUserWorkItem (callBack, state);
			} else {
				if (callBack == null)
					throw new ArgumentNullException ("callBack");

				if (callBack.IsTransparentProxy ()) {
					IAsyncResult ar = callBack.BeginInvoke (state, null, null);
					if (ar == null)
						return false;
				} else {
					AsyncResult ares = new AsyncResult (callBack, state, true);
					pool_queue (ares);
				}
				return true;
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern void pool_queue (AsyncResult ares);

		// TODO: It should be interface interface only to avoid extra allocation
		internal static void QueueWorkItem (WaitCallback callBack, object state)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				Microsoft.ThreadPool.QueueUserWorkItem (callBack, state);
			else
				pool_queue (new AsyncResult (callBack, state, false));
		}

		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callBack,
										object state,
										int millisecondsTimeOutInterval,
										bool executeOnlyOnce)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.RegisterWaitForSingleObject (waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce);
			else
				return RegisterWaitForSingleObject (waitObject, callBack, state, (long) millisecondsTimeOutInterval, executeOnlyOnce);
		}

		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callBack,
										object state,
										long millisecondsTimeOutInterval,
										bool executeOnlyOnce)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool) {
				return Microsoft.ThreadPool.RegisterWaitForSingleObject (waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce);
			} else {
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
		}

		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callBack,
										object state,
										TimeSpan timeout,
										bool executeOnlyOnce)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.RegisterWaitForSingleObject (waitObject, callBack, state, timeout, executeOnlyOnce);
			else
				return RegisterWaitForSingleObject (waitObject, callBack, state, (long) timeout.TotalMilliseconds, executeOnlyOnce);

		}

		[CLSCompliant(false)]
		public static RegisteredWaitHandle RegisterWaitForSingleObject (WaitHandle waitObject,
										WaitOrTimerCallback callBack,
										object state,
										uint millisecondsTimeOutInterval,
										bool executeOnlyOnce)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.RegisterWaitForSingleObject (waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce);
			else
				return RegisterWaitForSingleObject (waitObject, callBack, state, (long) millisecondsTimeOutInterval, executeOnlyOnce);
		}

		[CLSCompliant (false)]
		unsafe public static bool UnsafeQueueNativeOverlapped (NativeOverlapped *overlapped)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.UnsafeQueueNativeOverlapped (overlapped);
			else
				throw new NotImplementedException ();
		}

#if !NET_2_1 || MOBILE

		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static bool UnsafeQueueUserWorkItem (WaitCallback callBack, object state)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool) {
				return Microsoft.ThreadPool.UnsafeQueueUserWorkItem (callBack, state);
			} else {
				if (callBack == null)
					throw new ArgumentNullException ("callBack");

				// no stack propagation here (that's why it's unsafe and requires extra security permissions)
				if (!callBack.IsTransparentProxy ()) {
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
		}
		
		[MonoTODO("Not implemented")]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, int millisecondsTimeOutInterval,
			bool executeOnlyOnce) 
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.UnsafeRegisterWaitForSingleObject (waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce);
			else
				throw new NotImplementedException ();
		}
		
		[MonoTODO("Not implemented")]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, long millisecondsTimeOutInterval,
			bool executeOnlyOnce) 
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.UnsafeRegisterWaitForSingleObject (waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce);
			else
				throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, TimeSpan timeout,
			bool executeOnlyOnce) 
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.UnsafeRegisterWaitForSingleObject (waitObject, callBack, state, timeout, executeOnlyOnce);
			else
				throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		[CLSCompliant (false)]
		[SecurityPermission (SecurityAction.Demand, ControlEvidence=true, ControlPolicy=true)]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject (WaitHandle waitObject,
			WaitOrTimerCallback callBack, object state, uint millisecondsTimeOutInterval,
			bool executeOnlyOnce) 
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.UnsafeRegisterWaitForSingleObject (waitObject, callBack, state, millisecondsTimeOutInterval, executeOnlyOnce);
			else
				throw new NotImplementedException ();
		}

#endif

#region ReferenceSources
		// Extracted from ../../../../external/referencesource/mscorlib/system/threading/threadpool.cs
		internal static void UnsafeQueueCustomWorkItem(IThreadPoolWorkItem workItem, bool forceGlobal)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				Microsoft.ThreadPool.UnsafeQueueCustomWorkItem (workItem, forceGlobal);
			else
				QueueWorkItem ((obj) => ((IThreadPoolWorkItem)obj).ExecuteWorkItem (), workItem);
		}

		internal static IEnumerable<IThreadPoolWorkItem> GetQueuedWorkItems()
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.GetQueuedWorkItems ();
			else
				return new IThreadPoolWorkItem [0];
		}

		internal static bool TryPopCustomWorkItem(IThreadPoolWorkItem workItem)
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				return Microsoft.ThreadPool.TryPopCustomWorkItem (workItem);
			else
				return false;
		}

		internal static void NotifyWorkItemProgress()
		{
			if (Microsoft.ThreadPool.UseMicrosoftThreadPool)
				Microsoft.ThreadPool.NotifyWorkItemProgress ();
		}
#endregion
	}
}
