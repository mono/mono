//
// System.Threading.WaitHandle.cs
//
// Author:
// 	Dick Porter (dick@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com
//
// (C) 2002,2003 Ximian, Inc.	(http://www.ximian.com)
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

using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;

namespace System.Threading
{
	public abstract class WaitHandle : MarshalByRefObject, IDisposable
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern bool WaitAll_internal(WaitHandle[] handles, int ms, bool exitContext);
		
		static void CheckArray (WaitHandle [] handles)
		{
			if (handles == null)
				throw new ArgumentNullException ("waitHandles");

			int length = handles.Length;
			if (length > 64)
				throw new NotSupportedException ("Too many handles");

			foreach (WaitHandle w in handles) {
				if (w == null)
					throw new ArgumentNullException ("waitHandles", "null handle");

				if (w.os_handle == InvalidHandle)
					throw new ArgumentException ("null element found", "waitHandle");
			}
		}
		
		public static bool WaitAll(WaitHandle[] waitHandles)
		{
			CheckArray (waitHandles);
			return(WaitAll_internal(waitHandles, Timeout.Infinite, false));
		}

		public static bool WaitAll(WaitHandle[] waitHandles, int millisecondsTimeout, bool exitContext)
		{
			CheckArray (waitHandles);
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
			CheckArray (waitHandles);
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
		public static int WaitAny(WaitHandle[] waitHandles)
		{
			CheckArray (waitHandles);
			return(WaitAny_internal(waitHandles, Timeout.Infinite, false));
		}

		public static int WaitAny(WaitHandle[] waitHandles,
					  int millisecondsTimeout,
					  bool exitContext)
		{
			CheckArray (waitHandles);
			try {
				if (exitContext) SynchronizationAttribute.ExitContext ();
				return(WaitAny_internal(waitHandles, millisecondsTimeout, exitContext));
			}
			finally {
				if (exitContext) SynchronizationAttribute.EnterContext ();
			}
		}

		public static int WaitAny(WaitHandle[] waitHandles,
					  TimeSpan timeout, bool exitContext)
		{
			CheckArray (waitHandles);
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

		[MonoTODO]
		public WaitHandle() {
			// FIXME
		}

		public const int WaitTimeout = 258;

		private IntPtr os_handle = InvalidHandle;
		
		public virtual IntPtr Handle {
			get {
				return(os_handle);
			}
				
			set {
				os_handle=value;
			}
		}

		public virtual void Close() {
			Dispose(true);
			GC.SuppressFinalize (this);
		}

		internal void CheckDisposed ()
		{
			if (disposed || os_handle == InvalidHandle)
				throw new ObjectDisposedException (GetType ().FullName);
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern bool WaitOne_internal(IntPtr handle, int ms, bool exitContext);

		public virtual bool WaitOne()
		{
			CheckDisposed ();
			return(WaitOne_internal(os_handle, Timeout.Infinite, false));
		}

		public virtual bool WaitOne(int millisecondsTimeout, bool exitContext)
		{
			CheckDisposed ();
			try {
				if (exitContext) SynchronizationAttribute.ExitContext ();
				return(WaitOne_internal(os_handle, millisecondsTimeout, exitContext));
			}
			finally {
				if (exitContext) SynchronizationAttribute.EnterContext ();
			}
		}

		public virtual bool WaitOne(TimeSpan timeout, bool exitContext)
		{
			CheckDisposed ();
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < -1 || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");

			try {
				if (exitContext) SynchronizationAttribute.ExitContext ();
				return (WaitOne_internal(os_handle, (int) ms, exitContext));
			}
			finally {
				if (exitContext) SynchronizationAttribute.EnterContext ();
			}
		}

		protected static readonly IntPtr InvalidHandle = IntPtr.Zero;

		bool disposed = false;

		void IDisposable.Dispose() {
			Dispose(true);
			// Take yourself off the Finalization queue
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool explicitDisposing) {
			// Check to see if Dispose has already been called.
			if (!disposed) {
				disposed=true;
				if (os_handle == InvalidHandle)
					return;

				lock (this) {
					if (os_handle != InvalidHandle) {
						NativeEventCalls.CloseEvent_internal (os_handle);
						os_handle = InvalidHandle;
					}
				}
			}
		}

		~WaitHandle() {
			Dispose(false);
		}
	}
}
