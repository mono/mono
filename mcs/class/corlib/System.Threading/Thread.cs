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

		// Looks up the object associated with the current thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static Thread CurrentThread_internal();
		
		public static Thread CurrentThread {
			get {
				return(CurrentThread_internal());
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

		// Returns milliseconds remaining (due to interrupted sleep)
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static int Sleep_internal(int ms);

		// Causes thread to give up its timeslice
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Schedule_internal();
		
		public static void Sleep(int millisecondsTimeout) {
			if(millisecondsTimeout<0) {
				throw new ArgumentException("Negative timeout");
			}
			if(millisecondsTimeout==0) {
				// Schedule another thread
				Schedule_internal();
			} else {
				Thread thread=CurrentThread;
				
				thread.state |= ThreadState.WaitSleepJoin;
				int ms_remaining=Sleep_internal(millisecondsTimeout);
				thread.state &= ~ThreadState.WaitSleepJoin;

				if(ms_remaining>0) {
					throw new ThreadInterruptedException("Thread interrupted while sleeping");
				}
			}
			
		}

		public static void Sleep(TimeSpan timeout) {
			// LAMESPEC: says to throw ArgumentException too
			if(timeout.Milliseconds < 0 || timeout.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("Timeout out of range");
			}
			if(timeout.Milliseconds==0) {
				// Schedule another thread
				Schedule_internal();
			} else {
				Thread thread=CurrentThread;
				
				thread.state |= ThreadState.WaitSleepJoin;
				int ms_remaining=Sleep_internal(timeout.Milliseconds);
				thread.state &= ~ThreadState.WaitSleepJoin;
				
				if(ms_remaining>0) {
					throw new ThreadInterruptedException("Thread interrupted while sleeping");
				}
			}
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
				// LAMESPEC: is a Stopped thread dead?
				if((state & ThreadState.Running) != 0 ||
				   (state & ThreadState.Stopped) != 0) {
					return(true);
				} else {
					return(false);
				}
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

		private string thread_name=null;
		
		public string Name {
			get {
				return(thread_name);
			}
			
			set {
				thread_name=value;
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

		private ThreadState state=ThreadState.Unstarted;
		
		public ThreadState ThreadState {
			get {
				return(state);
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

		// The current thread joins with 'this'. Set ms to 0 to block
		// until this actually exits.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern /*bool*/ int Join_internal(int ms, UInt32 handle);
		
		public void Join() {
			if(state == ThreadState.Unstarted) {
				throw new ThreadStateException("Thread has not been started");
			}
			
			Thread thread=CurrentThread;
				
			thread.state |= ThreadState.WaitSleepJoin;
			Join_internal(0, system_thread_handle);
			thread.state &= ~ThreadState.WaitSleepJoin;
		}

		public bool Join(int millisecondsTimeout) {
			if(millisecondsTimeout<0) {
				throw new ArgumentException("Timeout less than zero");
			}
			if(state == ThreadState.Unstarted) {
				throw new ThreadStateException("Thread has not been started");
			}

			Thread thread=CurrentThread;
				
			thread.state |= ThreadState.WaitSleepJoin;
			bool ret=(Join_internal(millisecondsTimeout,
						system_thread_handle)==1);
			thread.state &= ~ThreadState.WaitSleepJoin;

			return(ret);
		}

		public bool Join(TimeSpan timeout) {
			// LAMESPEC: says to throw ArgumentException too
			if(timeout.Milliseconds < 0 || timeout.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("timeout out of range");
			}
			if(state == ThreadState.Unstarted) {
				throw new ThreadStateException("Thread has not been started");
			}

			Thread thread=CurrentThread;
				
			thread.state |= ThreadState.WaitSleepJoin;
			bool ret=(Join_internal(timeout.Milliseconds,
						system_thread_handle)==1);
			thread.state &= ~ThreadState.WaitSleepJoin;

			return(ret);
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
			if(state!=ThreadState.Unstarted) {
				throw new ThreadStateException("Thread has already been started");
			}

			state=ThreadState.Running;
			
			// Don't rely on system_thread_handle being
			// set before start_delegate runs!
			system_thread_handle=Start_internal(start_delegate);
		}

		public void Suspend() {
			// FIXME
		}

		~Thread() {
			// FIXME
		}
	}
}
