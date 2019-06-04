//
// System.Threading.WaitHandle.cs
//
// Author:
// 	Dick Porter (dick@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com
//
// (C) 2002,2003 Ximian, Inc.	(http://www.ximian.com)
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

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
#if FEATURE_REMOTING
using System.Runtime.Remoting.Contexts;
#endif
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;

namespace System.Threading
{
	[StructLayout (LayoutKind.Sequential)]
	public abstract partial class WaitHandle
	{
		protected static readonly IntPtr InvalidHandle = (IntPtr) (-1);

		internal const int MaxWaitHandles = 64;

		// We rely on the reference source implementation of WaitHandle, and it delegates to a function named
		//  WaitOneNative to perform the actual operation of waiting on a handle.
		// This native operation actually has to call back into managed code and invoke .Wait
		//  on the current SynchronizationContext. As such, our implementation of this "native" method
		//  is actually managed code, and the real native icall being used is Wait_internal.
		static int WaitOneNative (SafeHandle waitableSafeHandle, uint millisecondsTimeout, bool hasThreadAffinity, bool exitContext)
		{
			bool release = false;
#if !MONODROID
			var context = SynchronizationContext.Current;
#endif
			try {
				waitableSafeHandle.DangerousAddRef (ref release);

#if FEATURE_REMOTING
				if (exitContext)
					SynchronizationAttribute.ExitContext ();
#endif

#if !MONODROID
				// HACK: Documentation (and public posts by experts like Joe Duffy) suggests that
				//  users must first call SetWaitNotificationRequired to flag that a given synchronization
				//  context overrides .Wait. Because invoking the Wait method is somewhat expensive, we use
				//  the notification-required flag to determine whether or not we should invoke the managed
				//  wait method.
				// Another option would be to check whether this context uses the default Wait implementation,
				//  but I don't know of a cheap way to do this that handles derived types correctly.
				// If the thread does not have a synchronization context set at all, we can safely just
				//  jump directly to invoking Wait_internal.
				if ((context != null) && context.IsWaitNotificationRequired ()) {
					return context.Wait (
						new IntPtr[] { waitableSafeHandle.DangerousGetHandle () },
						false, 
						(int)millisecondsTimeout
					);
				} else
#endif
				{
					unsafe {
						IntPtr handle = waitableSafeHandle.DangerousGetHandle ();
						return Wait_internal (&handle, 1, false, (int)millisecondsTimeout);
					}
				}
			} finally {
				if (release)
					waitableSafeHandle.DangerousRelease ();

#if FEATURE_REMOTING
				if (exitContext)
					SynchronizationAttribute.EnterContext ();
#endif
			}

		}

		static int WaitMultiple(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext, bool WaitAll)
		{
			if (waitHandles.Length > MaxWaitHandles)
				return WAIT_FAILED;

			int release_last = -1;
			var context = SynchronizationContext.Current;

			try {
#if FEATURE_REMOTING
				if (exitContext)
					SynchronizationAttribute.ExitContext ();
#endif

				for (int i = 0; i < waitHandles.Length; ++i) {
					try {} finally {
						/* we have to put it in a finally block, to avoid having a ThreadAbortException
						 * between the return from DangerousAddRef and the assignement to release_last */
						bool release = false;
						waitHandles [i].SafeWaitHandle.DangerousAddRef (ref release);
						release_last = i;
					}
				}

				if ((context != null) && context.IsWaitNotificationRequired ()) {
					IntPtr[] handles = new IntPtr[waitHandles.Length];
					for (int i = 0; i < waitHandles.Length; ++i)
						handles[i] = waitHandles[i].SafeWaitHandle.DangerousGetHandle ();

					return context.Wait (
						handles,
						false, 
						(int)millisecondsTimeout
					);
				} else {
					unsafe {
						IntPtr* handles = stackalloc IntPtr[waitHandles.Length];

						for (int i = 0; i < waitHandles.Length; ++i)
							handles[i] = waitHandles[i].SafeWaitHandle.DangerousGetHandle ();

						return Wait_internal (handles, waitHandles.Length, WaitAll, millisecondsTimeout);
					}
				}
			} finally {
				for (int i = release_last; i >= 0; --i) {
					waitHandles [i].SafeWaitHandle.DangerousRelease ();
				}

#if FEATURE_REMOTING
				if (exitContext)
					SynchronizationAttribute.EnterContext ();
#endif
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal unsafe static extern int Wait_internal(IntPtr* handles, int numHandles, bool waitAll, int ms);

		static int SignalAndWaitOne (SafeWaitHandle waitHandleToSignal,SafeWaitHandle waitHandleToWaitOn, int millisecondsTimeout, bool hasThreadAffinity,  bool exitContext)
		{
			bool releaseHandleToSignal = false, releaseHandleToWaitOn = false;
			try {
				waitHandleToSignal.DangerousAddRef (ref releaseHandleToSignal);
				waitHandleToWaitOn.DangerousAddRef (ref releaseHandleToWaitOn);

				return SignalAndWait_Internal (waitHandleToSignal.DangerousGetHandle (), waitHandleToWaitOn.DangerousGetHandle (), millisecondsTimeout);
			} finally {
				if (releaseHandleToSignal)
					waitHandleToSignal.DangerousRelease ();
				if (releaseHandleToWaitOn)
					waitHandleToWaitOn.DangerousRelease ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern int SignalAndWait_Internal (IntPtr toSignal, IntPtr toWaitOn, int ms);

		internal static int ToTimeoutMilliseconds(TimeSpan timeout)
		{
			var timeoutMilliseconds = (long)timeout.TotalMilliseconds;
			if (timeoutMilliseconds < -1 || timeoutMilliseconds > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(timeout), SR.ArgumentOutOfRange_NeedNonNegOrNegative1);
			}
			return (int)timeoutMilliseconds;
		}
	}
}
