//
// System.Threading.Thread.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Remoting.Contexts;
using System.Security.Principal;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Threading
{
	public sealed class Thread
	{
		public static Context CurrentContext {
			get {
				// FIXME -
				// System.Runtime.Remoting.Context not
				// yet implemented
				return(null);
			}
		}

		public static IPrincipal CurrentPrincipal {
			get {
				// FIXME -
				// System.Security.Principal.IPrincipal
				// not yet implemented
				return(null);
			}
			
			set {
			}
		}

		public static Thread CurrentThread {
			get {
				// FIXME
				return(null);
			}
		}

		public static LocalDataStoreSlot AllocateDataSlot() {
			// FIXME
			return(null);
		}

		public static LocalDataStoreSlot AllocateNamedDataSlot(string name) {
			// FIXME
			return(null);
		}

		public static void FreeNamedDataSlot(string name) {
			// FIXME
		}

		public static object GetData(LocalDataStoreSlot slot) {
			// FIXME
			return(null);
		}

		public static AppDomain GetDomain() {
			// FIXME
			return(null);
		}

		public static int GetDomainID() {
			// FIXME
			return(0);
		}

		public static LocalDataStoreSlot GetNamedDataSlot(string name) {
			// FIXME
			return(null);
		}

		public static void ResetAbort() {
			// FIXME
		}

		public static void SetData(LocalDataStoreSlot slot, object data) {
			// FIXME
		}

		public static void Sleep(int millisecondsTimeout) {
			if(millisecondsTimeout<0) {
				throw new ArgumentException("Negative timeout");
			}
			// FIXME
		}

		public static void Sleep(TimeSpan timeout) {
			// LAMESPEC: says to throw ArgumentException too
			if(timeout.Milliseconds < 0 || timeout.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("Timeout out of range");
			}
			// FIXME
		}

		private ThreadStart start_delegate=null;
		
		public Thread(ThreadStart start) {
			if(start==null) {
				throw new ArgumentNullException("Null ThreadStart");
			}

			// Nothing actually happens here, the fun
			// begins when Thread.Start() is called.  For
			// now, just record what the ThreadStart
			// delegate is.
			start_delegate=start;
		}

		public ApartmentState ApartmentState {
			get {
				// FIXME
				return(ApartmentState.Unknown);
			}
			
			set {
			}
		}

		public CultureInfo CurrentCulture {
			get {
				// FIXME
				return(null);
			}
			
			set {
			}
		}

		public CultureInfo CurrentUICulture {
			get {
				// FIXME
				return(null);
			}
			
			set {
			}
		}

		public bool IsAlive {
			get {
				// FIXME
				return(false);
			}
		}

		public bool IsBackground {
			get {
				// FIXME
				return(false);
			}
			
			set {
			}
		}

		public string Name {
			get {
				// FIXME
				return("none");
			}
			
			set {
			}
		}

		public ThreadPriority Priority {
			get {
				// FIXME
				return(ThreadPriority.Lowest);
			}
			
			set {
			}
		}

		public ThreadState ThreadState {
			get {
				// FIXME
				return(ThreadState.Unstarted);
			}
		}

		public void Abort() {
			// FIXME
		}

		public void Abort(object stateInfo) {
			// FIXME
		}

		public void Interrupt() {
			// FIXME
		}

		public void Join() {
			// FIXME
		}

		public bool Join(int millisecondsTimeout) {
			if(millisecondsTimeout<0) {
				throw new ArgumentException("Timeout less than zero");
			}
			// FIXME
			return(false);
		}

		public bool Join(TimeSpan timeout) {
			// LAMESPEC: says to throw ArgumentException too
			if(timeout.Milliseconds < 0 || timeout.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("timeout out of range");
			}
			// FIXME
			return(false);
		}

		public void Resume() {
			// FIXME
		}
		
		// stores a pthread_t, which is defined as unsigned long
		// on my system.  I _think_ windows uses "unsigned int" for
		// its thread handles, so that _should_ work too.
		private UInt32 system_thread_handle;

		// Returns the system thread handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern UInt32 Start_internal(ThreadStart start);
		
		public void Start() {
			system_thread_handle=Start_internal(start_delegate);
			// FIXME
		}

		public void Suspend() {
			// FIXME
		}

		~Thread() {
			// FIXME
		}
	}
}
