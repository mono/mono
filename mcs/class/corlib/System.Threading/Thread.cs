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
using System.Collections;

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

		// Registers a new LocalDataStoreSlot with a thread key.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void DataSlot_register(LocalDataStoreSlot slot);
		
		public static LocalDataStoreSlot AllocateDataSlot() {
			LocalDataStoreSlot slot = new LocalDataStoreSlot();
			
			DataSlot_register(slot);
			
			return(slot);
		}

		// Stores a hash keyed by strings of LocalDataStoreSlot objects
		static Hashtable datastorehash = Hashtable.Synchronized(new Hashtable());
		
		public static LocalDataStoreSlot AllocateNamedDataSlot(string name) {
			LocalDataStoreSlot slot = (LocalDataStoreSlot)datastorehash[name];
			if(slot!=null) {
				// This exception isnt documented (of
				// course) but .net throws it
				throw new ArgumentException("Named data slot already added");
			}
			
			slot = new LocalDataStoreSlot();

			datastorehash.Add(name, slot);
			
			DataSlot_register(slot);
			
			return(slot);
		}

		public static void FreeNamedDataSlot(string name) {
			LocalDataStoreSlot slot=(LocalDataStoreSlot)datastorehash[name];

			if(slot!=null) {
				// FIXME
			}
		}

		// Retrieves an object from slot 'slot' in this thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static object DataSlot_retrieve(LocalDataStoreSlot slot);

		public static object GetData(LocalDataStoreSlot slot) {
			return(DataSlot_retrieve(slot));
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
			LocalDataStoreSlot slot=(LocalDataStoreSlot)datastorehash[name];

			if(slot==null) {
				slot=AllocateNamedDataSlot(name);
			}
			
			return(slot);
		}

		public static void ResetAbort() {
			// FIXME
		}

		// Stores 'data' into slot 'slot' in this thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void DataSlot_store(LocalDataStoreSlot slot, object data);
		
		public static void SetData(LocalDataStoreSlot slot, object data) {
			DataSlot_store(slot, data);
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
				
				thread.set_state(ThreadState.WaitSleepJoin);
				
				int ms_remaining=Sleep_internal(millisecondsTimeout);
				thread.clr_state(ThreadState.WaitSleepJoin);

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
				
				thread.set_state(ThreadState.WaitSleepJoin);
				int ms_remaining=Sleep_internal(timeout.Milliseconds);
				thread.clr_state(ThreadState.WaitSleepJoin);
				
				if(ms_remaining>0) {
					throw new ThreadInterruptedException("Thread interrupted while sleeping");
				}
			}
		}

		// stores a pthread_t, which is defined as unsigned long
		// on my system.  I _think_ windows uses "unsigned int" for
		// its thread handles, so that _should_ work too.
		private UInt32 system_thread_handle;

		// Returns the system thread handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern UInt32 Thread_internal(ThreadStart start);

		public Thread(ThreadStart start) {
			if(start==null) {
				throw new ArgumentNullException("Null ThreadStart");
			}

			// This is a two-stage thread launch.  Thread_internal
			// creates the new thread, but blocks it until
			// Start() is called later.
			system_thread_handle=Thread_internal(start);
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
				// LAMESPEC: is a Stopped or Suspended
				// thread dead?
				ThreadState curstate=state;
				
				if((curstate & ThreadState.Aborted) != 0 ||
				   (curstate & ThreadState.AbortRequested) != 0 ||
				   (curstate & ThreadState.Unstarted) != 0) {
					return(false);
				} else {
					return(true);
				}
			}
		}

		public bool IsBackground {
			get {
				if((state & ThreadState.Background) != 0) {
					return(true);
				} else {
					return(false);
				}
			}
			
			set {
				if(value==true) {
					set_state(ThreadState.Background);
				} else {
					clr_state(ThreadState.Background);
				}
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
		private extern /* FIXME waiting for impl in mono_create trampoline bool*/ int Join_internal(int ms, UInt32 handle);
		
		public void Join() {
			if((state & ThreadState.Unstarted) != 0) {
				throw new ThreadStateException("Thread has not been started");
			}
			
			Thread thread=CurrentThread;
				
			thread.set_state(ThreadState.WaitSleepJoin);
			Join_internal(0, system_thread_handle);
			thread.clr_state(ThreadState.WaitSleepJoin);
		}

		public bool Join(int millisecondsTimeout) {
			if(millisecondsTimeout<0) {
				throw new ArgumentException("Timeout less than zero");
			}
			if((state & ThreadState.Unstarted) != 0) {
				throw new ThreadStateException("Thread has not been started");
			}

			Thread thread=CurrentThread;
				
			thread.set_state(ThreadState.WaitSleepJoin);
			bool ret=(Join_internal(millisecondsTimeout,
						system_thread_handle)==1);
			thread.clr_state(ThreadState.WaitSleepJoin);

			return(ret);
		}

		public bool Join(TimeSpan timeout) {
			// LAMESPEC: says to throw ArgumentException too
			if(timeout.Milliseconds < 0 || timeout.Milliseconds > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("timeout out of range");
			}
			if((state & ThreadState.Unstarted) != 0) {
				throw new ThreadStateException("Thread has not been started");
			}

			Thread thread=CurrentThread;

			thread.set_state(ThreadState.WaitSleepJoin);
			bool ret=(Join_internal(timeout.Milliseconds,
						system_thread_handle)==1);
			thread.clr_state(ThreadState.WaitSleepJoin);

			return(ret);
		}

		public void Resume() {
			// FIXME
		}

		// Launches the thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Start_internal(UInt32 handle);
		
		public void Start() {
			if((state & ThreadState.Unstarted) == 0) {
				throw new ThreadStateException("Thread has already been started");
			}

			// Mark the thread state as Running (which is
			// all bits cleared). Therefore just remove
			// the Unstarted bit
			clr_state(ThreadState.Unstarted);
				
			// Launch this thread
			Start_internal(system_thread_handle);
		}

		public void Suspend() {
			if((state & ThreadState.Unstarted) != 0 || !IsAlive) {
				throw new ThreadStateException("Thread has not been started, or is dead");
			}

			set_state(ThreadState.SuspendRequested);
			// FIXME - somehow let the interpreter know that
			// this thread should now suspend
		}

		~Thread() {
			// FIXME
		}

		private void set_state(ThreadState set) {
			lock(this) {
				state |= set;
			}
		}
		private void clr_state(ThreadState clr) {
			lock(this) {
				state &= ~clr;
			}
		}
	}
}
