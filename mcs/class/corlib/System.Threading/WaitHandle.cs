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
			if(waitHandles.Length>64) {
				throw new NotSupportedException("Too many handles");
			}
			for(int i=0; i<waitHandles.Length; i++) {
				if(waitHandles[i]==null) {
					throw new ArgumentNullException("null handle");
				}
			}
			
			return(WaitAll_internal(waitHandles, Timeout.Infinite,
						false));
		}

		public static bool WaitAll(WaitHandle[] waitHandles,
					   int millisecondsTimeout,
					   bool exitContext) {
			if(waitHandles.Length>64) {
				throw new NotSupportedException("Too many handles");
			}
			for(int i=0; i<waitHandles.Length; i++) {
				if(waitHandles[i]==null) {
					throw new ArgumentNullException("null handle");
				}
			}
			
			return(WaitAll_internal(waitHandles, millisecondsTimeout, false));
		}

		public static bool WaitAll(WaitHandle[] waitHandles,
					   TimeSpan timeout,
					   bool exitContext) {
			int ms=Convert.ToInt32(timeout.TotalMilliseconds);
			
			if(ms < 0 || ms > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("Timeout out of range");
			}
			if(waitHandles.Length>64) {
				throw new NotSupportedException("Too many handles");
			}
			for(int i=0; i<waitHandles.Length; i++) {
				if(waitHandles[i]==null) {
					throw new ArgumentNullException("null handle");
				}
			}
			
			return(WaitAll_internal(waitHandles, ms, exitContext));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern int WaitAny_internal(WaitHandle[] handles, int ms, bool exitContext);

		// LAMESPEC: Doesn't specify how to signal failures
		public static int WaitAny(WaitHandle[] waitHandles) {
			if(waitHandles.Length>64) {
				throw new NotSupportedException("Too many handles");
			}
			for(int i=0; i<waitHandles.Length; i++) {
				if(waitHandles[i]==null) {
					throw new ArgumentNullException("null handle");
				}
			}
			
			return(WaitAny_internal(waitHandles, Timeout.Infinite,
						false));
		}

		public static int WaitAny(WaitHandle[] waitHandles,
					  int millisecondsTimeout,
					  bool exitContext) {
			if(waitHandles.Length>64) {
				throw new NotSupportedException("Too many handles");
			}
			for(int i=0; i<waitHandles.Length; i++) {
				if(waitHandles[i]==null) {
					throw new ArgumentNullException("null handle");
				}
			}
			
			return(WaitAny_internal(waitHandles,
						millisecondsTimeout,
						exitContext));
		}

		public static int WaitAny(WaitHandle[] waitHandles,
					  TimeSpan timeout, bool exitContext) {
			int ms=Convert.ToInt32(timeout.TotalMilliseconds);
			
			if(ms < 0 || ms > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("Timeout out of range");
			}
			if(waitHandles.Length>64) {
				throw new NotSupportedException("Too many handles");
			}
			for(int i=0; i<waitHandles.Length; i++) {
				if(waitHandles[i]==null) {
					throw new ArgumentNullException("null handle");
				}
			}
			
			return(WaitAny_internal(waitHandles, ms, exitContext));
		}

		[MonoTODO]
		public WaitHandle() {
			// FIXME
		}

		public const int WaitTimeout = 258;

		private IntPtr os_handle = IntPtr.Zero;
		
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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern bool WaitOne_internal(IntPtr handle, int ms, bool exitContext);

		public virtual bool WaitOne() {
			return(WaitOne_internal(os_handle, Timeout.Infinite,
						false));
		}

		public virtual bool WaitOne(int millisecondsTimeout, bool exitContext) {
			return(WaitOne_internal(os_handle,
						millisecondsTimeout,
						exitContext));
		}

		public virtual bool WaitOne(TimeSpan timeout, bool exitContext) {
			int ms=Convert.ToInt32(timeout.TotalMilliseconds);
			
			return(WaitOne_internal(os_handle, ms, exitContext));
		}

		protected static readonly IntPtr InvalidHandle;

		private bool disposed = false;

		void IDisposable.Dispose() {
			Dispose(true);
			// Take yourself off the Finalization queue
			GC.SuppressFinalize(this);
		}
		
		protected virtual void Dispose(bool explicitDisposing) {
			// Check to see if Dispose has already been called.
			if(!this.disposed) {
				this.disposed=true;
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
				//Release(handle);
				//handle=IntPtr.Zero;
			}
		}

		~WaitHandle() {
			Dispose(false);
		}
	}
}
