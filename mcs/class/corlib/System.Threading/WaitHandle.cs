//
// System.Threading.WaitHandle.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Threading
{
	public abstract class WaitHandle : MarshalByRefObject, IDisposable
	{
		public static bool WaitAll(WaitHandle[] waitHandles) {
			// FIXME
			return(false);
		}

		public static bool WaitAll(WaitHandle[] waitHandles,
					   int millisecondsTimeout,
					   bool exitContext) {
			// FIXME
			return(false);
		}

		public static bool WaitAll(WaitHandle[] waitHandles,
					   TimeSpan timeout,
					   bool exitContext) {
			// FIXME
			return(false);
		}

		public static int WaitAny(WaitHandle[] waitHandles) {
			// FIXME
			return(0);
		}

		public static int WaitAny(WaitHandle[] waitHandles,
					  int millisecondsTimeout,
					  bool exitContext) {
			// FIXME
			return(0);
		}

		public static int WaitAny(WaitHandle[] waitHandles,
					  TimeSpan timeout, bool exitContext) {
			if(timeout.Milliseconds < 0 ||
			   timeout.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("Timeout out of range");
			}
			// FIXME
			return(0);
		}

		public WaitHandle() {
			// FIXME
		}

		public virtual IntPtr Handle {
			get {
				// FIXME
				return new IntPtr();
			}
				
			set {
				// FIXME
			}
		}

		public virtual void Close() {
			Dispose(false);
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
