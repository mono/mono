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
using System.Runtime.Remoting.Contexts;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Runtime.ConstrainedExecution;

namespace System.Threading
{
	[ComVisible (true)]
	public abstract class WaitHandle : MarshalByRefObject, IDisposable
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern bool WaitAll_internal(WaitHandle[] handles, int ms, bool exitContext);
		
		static void CheckArray (WaitHandle [] handles, bool waitAll)
		{
			if (handles == null)
				throw new ArgumentNullException ("waitHandles");

			int length = handles.Length;
			if (length > 64)
				throw new NotSupportedException ("Too many handles");

			if (handles.Length == 0) {
				// MS throws different exceptions from the different methods.
				if (waitAll)
					throw new ArgumentNullException ("waitHandles");
				else
					throw new ArgumentException ();
			}

#if false
			//
			// Although we should thrown an exception if this is an STA thread,
			// Mono does not know anything about STA threads, and just makes
			// things like Paint.NET not even possible to work.
			//
			// See bug #78455 for the bug this is supposed to fix. 
			// 
			if (waitAll && length > 1 && IsSTAThread)
				throw new NotSupportedException ("WaitAll for multiple handles is not allowed on an STA thread.");
#endif
			foreach (WaitHandle w in handles) {
				if (w == null)
					throw new ArgumentNullException ("waitHandles", "null handle");

				if (w.safe_wait_handle == null)
					throw new ArgumentException ("null element found", "waitHandle");

			}
		}
#if false
		// usage of property is commented - see above
		static bool IsSTAThread {
			get {
				bool isSTA = Thread.CurrentThread.ApartmentState ==
					ApartmentState.STA;

				// FIXME: remove this check after Thread.ApartmentState
				// has been properly implemented.
				if (!isSTA) {
					Assembly asm = Assembly.GetEntryAssembly ();
					if (asm != null)
						isSTA = asm.EntryPoint.GetCustomAttributes (typeof (STAThreadAttribute), false).Length > 0;
				}

				return isSTA;
			}
		}
#endif
		public static bool WaitAll(WaitHandle[] waitHandles)
		{
			CheckArray (waitHandles, true);
			return(WaitAll_internal(waitHandles, Timeout.Infinite, false));
		}

		public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
		{
			CheckArray (waitHandles, true);
			// check negative - except for -1 (which is Timeout.Infinite)
			if (millisecondsTimeout < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			try {
				if (exitContext) SynchronizationAttribute.ExitContext ();
				return(WaitAll_internal(waitHandles, millisecondsTimeout, false));
			}
			finally {
				if (exitContext) SynchronizationAttribute.EnterContext ();
			}
		}

		public static bool WaitAll(WaitHandle[] waitHandles,
					   TimeSpan timeout,
					   bool exitContext)
		{
			CheckArray (waitHandles, true);
			long ms = (long) timeout.TotalMilliseconds;
			
			if (ms < -1 || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");

			try {
				if (exitContext) SynchronizationAttribute.ExitContext ();
				return (WaitAll_internal (waitHandles, (int) ms, exitContext));
			}
			finally {
				if (exitContext) SynchronizationAttribute.EnterContext ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int WaitAny_internal(WaitHandle[] handles, int ms, bool exitContext);

		// LAMESPEC: Doesn't specify how to signal failures
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int WaitAny(WaitHandle[] waitHandles)
		{
			CheckArray (waitHandles, false);
			return(WaitAny_internal(waitHandles, Timeout.Infinite, false));
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int WaitAny(WaitHandle[] waitHandles,
					  int millisecondsTimeout,
					  bool exitContext)
		{
			CheckArray (waitHandles, false);
			// check negative - except for -1 (which is Timeout.Infinite)
			if (millisecondsTimeout < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			try {
				if (exitContext) SynchronizationAttribute.ExitContext ();
				return(WaitAny_internal(waitHandles, millisecondsTimeout, exitContext));
			}
			finally {
				if (exitContext) SynchronizationAttribute.EnterContext ();
			}
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int WaitAny(WaitHandle[] waitHandles, TimeSpan timeout)
		{
			return WaitAny (waitHandles, timeout, false);
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int WaitAny(WaitHandle[] waitHandles, int millisecondsTimeout)
		{
			return WaitAny (waitHandles, millisecondsTimeout, false);
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public static int WaitAny(WaitHandle[] waitHandles,
					  TimeSpan timeout, bool exitContext)
		{
			CheckArray (waitHandles, false);
			long ms = (long) timeout.TotalMilliseconds;
			
			if (ms < -1 || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");

			try {
				if (exitContext) SynchronizationAttribute.ExitContext ();
				return (WaitAny_internal(waitHandles, (int) ms, exitContext));
			}
			finally {
				if (exitContext) SynchronizationAttribute.EnterContext ();
			}
		}

		protected WaitHandle()
		{
			// FIXME
		}

		public virtual void Close() {
			Dispose(true);
			GC.SuppressFinalize (this);
		}

#if NET_4_0
		public void Dispose ()
#else		
		void IDisposable.Dispose ()
#endif
		{
			Close ();
		}

		public const int WaitTimeout = 258;

		//
		// In 2.0 we use SafeWaitHandles instead of IntPtrs
		//
		SafeWaitHandle safe_wait_handle;

		[Obsolete ("In the profiles > 2.x, use SafeHandle instead of Handle")]
		public virtual IntPtr Handle {
			get {
				return safe_wait_handle.DangerousGetHandle ();
			}

			[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
			[SecurityPermission (SecurityAction.InheritanceDemand, UnmanagedCode = true)]
			set {
				if (value == InvalidHandle)
					safe_wait_handle = new SafeWaitHandle (InvalidHandle, false);
				else
					safe_wait_handle = new SafeWaitHandle (value, true);
			}
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern bool WaitOne_internal(IntPtr handle, int ms, bool exitContext);

		protected virtual void Dispose (bool explicitDisposing)
		{
			if (!disposed){
				disposed = true;

				//
				// This is only the case if the handle was never properly initialized
				// most likely a bug in the derived class
				//
				if (safe_wait_handle == null)
					return;

				lock (this){
					if (safe_wait_handle != null)
						safe_wait_handle.Dispose ();
				}
			}
		}

		public SafeWaitHandle SafeWaitHandle {
			[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
			get {
				return safe_wait_handle;
			}

			[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
			set {
				if (value == null)
					safe_wait_handle = new SafeWaitHandle (InvalidHandle, false);
				else
					safe_wait_handle = value;
			}
		}

		public static bool SignalAndWait (WaitHandle toSignal,
						  WaitHandle toWaitOn)
		{
			return SignalAndWait (toSignal, toWaitOn, -1, false);
		}
		
		public static bool SignalAndWait (WaitHandle toSignal,
						  WaitHandle toWaitOn,
						  int millisecondsTimeout,
						  bool exitContext)
		{
			if (toSignal == null)
				throw new ArgumentNullException ("toSignal");
			if (toWaitOn == null)
				throw new ArgumentNullException ("toWaitOn");

			if (millisecondsTimeout < -1)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			return SignalAndWait_Internal (toSignal.Handle, toWaitOn.Handle, millisecondsTimeout, exitContext);
		}
		
		public static bool SignalAndWait (WaitHandle toSignal,
						  WaitHandle toWaitOn,
						  TimeSpan timeout,
						  bool exitContext)
		{
			double ms = timeout.TotalMilliseconds;
			if (ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");

			return SignalAndWait (toSignal, toWaitOn, Convert.ToInt32 (ms), false);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool SignalAndWait_Internal (IntPtr toSignal, IntPtr toWaitOn, int ms, bool exitContext);

		public virtual bool WaitOne()
		{
			CheckDisposed ();
			bool release = false;
			try {
				safe_wait_handle.DangerousAddRef (ref release);
				return (WaitOne_internal(safe_wait_handle.DangerousGetHandle (), Timeout.Infinite, false));
			} finally {
				if (release)
					safe_wait_handle.DangerousRelease ();
			}
		}

		public virtual bool WaitOne(int millisecondsTimeout, bool exitContext)
		{
			CheckDisposed ();
			// check negative - except for -1 (which is Timeout.Infinite)
			if (millisecondsTimeout < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout");

			bool release = false;
			try {
				if (exitContext)
					SynchronizationAttribute.ExitContext ();
				safe_wait_handle.DangerousAddRef (ref release);
				return (WaitOne_internal(safe_wait_handle.DangerousGetHandle (), millisecondsTimeout, exitContext));
			} finally {
				if (exitContext)
					SynchronizationAttribute.EnterContext ();
				if (release)
					safe_wait_handle.DangerousRelease ();
			}
		}

		public virtual bool WaitOne (int millisecondsTimeout)
		{
			return WaitOne (millisecondsTimeout, false);
		}

		public virtual bool WaitOne (TimeSpan timeout)
		{
			return WaitOne (timeout, false);
		}

		public virtual bool WaitOne(TimeSpan timeout, bool exitContext)
		{
			CheckDisposed ();
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < -1 || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");

			bool release = false;
			try {
				if (exitContext)
					SynchronizationAttribute.ExitContext ();
				safe_wait_handle.DangerousAddRef (ref release);
				return (WaitOne_internal(safe_wait_handle.DangerousGetHandle (), (int) ms, exitContext));
			}
			finally {
				if (exitContext)
					SynchronizationAttribute.EnterContext ();
				if (release)
					safe_wait_handle.DangerousRelease ();
			}
		}

		internal void CheckDisposed ()
		{
			if (disposed || safe_wait_handle == null)
				throw new ObjectDisposedException (GetType ().FullName);
		}

		public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout)
		{
			return WaitAll (waitHandles, millisecondsTimeout, false);
		}

		public static bool WaitAll(WaitHandle[] waitHandles, TimeSpan timeout)
		{
			return WaitAll (waitHandles, timeout, false);
		}
		
		protected static readonly IntPtr InvalidHandle = (IntPtr) (-1);
		bool disposed = false;

		~WaitHandle() {
			Dispose(false);
		}
	}
}
