//
// System.Threading.WaitHandle.cs
//
// Author:
// 	Dick Porter (dick@ximian.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com
//
// (C) 2002,2003 Ximian, Inc.	(http://www.ximian.com)
//

using System.Runtime.CompilerServices;

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
			return(WaitAll_internal(waitHandles, millisecondsTimeout, false));
		}

		public static bool WaitAll(WaitHandle[] waitHandles,
					   TimeSpan timeout,
					   bool exitContext)
		{
			CheckArray (waitHandles);
			long ms = (long) timeout.TotalMilliseconds;
			
			if (ms < -1 || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");

			return (WaitAll_internal (waitHandles, (int) ms, exitContext));
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
			return(WaitAny_internal(waitHandles, millisecondsTimeout, exitContext));
		}

		public static int WaitAny(WaitHandle[] waitHandles,
					  TimeSpan timeout, bool exitContext)
		{
			CheckArray (waitHandles);
			long ms = (long) timeout.TotalMilliseconds;
			
			if (ms < -1 || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");

			return (WaitAny_internal(waitHandles, (int) ms, exitContext));
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
			return(WaitOne_internal(os_handle, millisecondsTimeout, exitContext));
		}

		public virtual bool WaitOne(TimeSpan timeout, bool exitContext)
		{
			CheckDisposed ();
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < -1 || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout");

			return (WaitOne_internal(os_handle, (int) ms, exitContext));
		}

		protected static readonly IntPtr InvalidHandle = IntPtr.Zero;

		private bool disposed = false;

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
