//
// System.Threading.WaitHandle.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.CompilerServices;

namespace System.Threading
{
	public abstract class WaitHandle : MarshalByRefObject, IDisposable
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern bool WaitAll_internal(WaitHandle[] handles, int ms, bool exitContext);
		
		public static bool WaitAll(WaitHandle[] waitHandles) {
			return(WaitAll_internal(waitHandles, 0, false));
		}

		public static bool WaitAll(WaitHandle[] waitHandles,
					   int millisecondsTimeout,
					   bool exitContext) {
			return(WaitAll_internal(waitHandles, millisecondsTimeout, false));
		}

		public static bool WaitAll(WaitHandle[] waitHandles,
					   TimeSpan timeout,
					   bool exitContext) {
			return(WaitAll_internal(waitHandles, timeout.Milliseconds, exitContext));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int WaitAny_internal(WaitHandle[] handles, int ms, bool exitContext);

		public static int WaitAny(WaitHandle[] waitHandles) {
			return(WaitAny_internal(waitHandles, 0, false));
		}

		public static int WaitAny(WaitHandle[] waitHandles,
					  int millisecondsTimeout,
					  bool exitContext) {
			return(WaitAny_internal(waitHandles, millisecondsTimeout, exitContext));
		}

		public static int WaitAny(WaitHandle[] waitHandles,
					  TimeSpan timeout, bool exitContext) {
			if(timeout.Milliseconds < 0 ||
			   timeout.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("Timeout out of range");
			}
			return(WaitAny_internal(waitHandles, timeout.Milliseconds, exitContext));
		}

		public WaitHandle() {
			// FIXME
		}

		public const int WaitTimeout = 258;

		private IntPtr handle = IntPtr.Zero;
		
		public virtual IntPtr Handle {
			get {
				return(handle);
			}
				
			set {
				handle=value;
			}
		}

		public virtual void Close() {
			Dispose(false);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected virtual extern bool WaitOne_internal(IntPtr handle, int ms, bool exitContext);

		public virtual bool WaitOne() {
			return(WaitOne_internal(handle, 0, false));
		}

		public virtual bool WaitOne(int millisecondsTimeout, bool exitContext) {
			return(WaitOne_internal(handle, millisecondsTimeout, exitContext));
		}

		public virtual bool WaitOne(TimeSpan timeout, bool exitContext) {
			return(WaitOne_internal(handle, timeout.Milliseconds, exitContext));
		}

		protected static readonly IntPtr InvalidHandle;

		private bool disposed = false;

		public void Dispose() {
			Dispose(true);
			// Take yourself off the Finalization queue
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool explicitDisposing) {
			// Check to see if Dispose has already been called.
			if(!this.disposed) {
				// If this is a call to Dispose,
				// dispose all managed resources.
				if(explicitDisposing) {
					// Free up stuff here
					//Components.Dispose();
				}

				// Release unmanaged resources
				// Note that this is not thread safe.
				// Another thread could start
				// disposing the object after the
				// managed resources are disposed, but
				// before the disposed flag is set to
				// true.
				this.disposed=true;
				//Release(handle);
				//handle=IntPtr.Zero;
			}
		}

		~WaitHandle() {
			Dispose(false);
		}
	}
}
