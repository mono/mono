//
// System.Threading.Thread.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004 Novell (http://www.novell.com)
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

using System.Runtime.Remoting.Contexts;
using System.Security.Permissions;
using System.Security.Principal;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Collections;

#if NET_2_0
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
#endif

namespace System.Threading
{
	public sealed class Thread
	{
		#region Sync with object.h
		// stores a thread handle
		private IntPtr system_thread_handle;
		
		private CultureInfo current_culture;
		private CultureInfo current_ui_culture;
		private bool threadpool_thread;
		/* accessed only from unmanaged code */
		private IntPtr name;
		private int name_len; 
		private ThreadState state = ThreadState.Unstarted;
		private object abort_exc;
		internal object abort_state;
		/* thread_id is only accessed from unmanaged code */
		private int thread_id;
		
		/* start_notify is used by the runtime to signal that Start()
		 * is ok to return
		 */
		private IntPtr start_notify;
		private IntPtr stack_ptr;
		private IntPtr static_data;
		private IntPtr jit_data;
		private IntPtr lock_data;
		private IntPtr appdomain_refs;
		private bool interruption_requested;
		private IntPtr suspend_event;
		private IntPtr resume_event;
		private object synch_lock = new Object();
		#endregion

		private ThreadStart threadstart;
		private string thread_name=null;
		
		private IPrincipal _principal;
		
		public static Context CurrentContext {
			get {
				return(AppDomain.InternalGetContext ());
			}
		}

		public static IPrincipal CurrentPrincipal {
			get {
				IPrincipal p = null;
				Thread th = CurrentThread;
				lock (th) {
					p = th._principal;
					if (p == null) {
						p = GetDomain ().DefaultPrincipal;
						th._principal = p;
					}
				}
				return p;
			}
			set {
				new SecurityPermission (SecurityPermissionFlag.ControlPrincipal).Demand ();
				CurrentThread._principal = value;
			}
		}

		// Looks up the object associated with the current thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static Thread CurrentThread_internal();
		
		public static Thread CurrentThread {
#if NET_2_0
			[ReliabilityContract (Consistency.WillNotCorruptState, CER.MayFail)]
#endif
			get {
				return(CurrentThread_internal());
			}
		}

		internal static int CurrentThreadId {
			get {
				return CurrentThread.thread_id;
			}
		}

		// Looks up the slot hash for the current thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static Hashtable SlotHash_lookup();

		// Stores the slot hash for the current thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void SlotHash_store(Hashtable slothash);

		private static Hashtable GetTLSSlotHash() {
			Hashtable slothash=SlotHash_lookup();
			if(slothash==null) {
				// Not synchronised, because this is
				// thread specific anyway.
				slothash=new Hashtable();
				SlotHash_store(slothash);
			}

			return(slothash);
		}
		
		internal static object ResetDataStoreStatus () {
			Hashtable slothash=SlotHash_lookup();
			SlotHash_store(null);
			return slothash;
		}

		internal static void RestoreDataStoreStatus (object data) {
			SlotHash_store((Hashtable)data);
		}

		public static LocalDataStoreSlot AllocateDataSlot() {
			LocalDataStoreSlot slot = new LocalDataStoreSlot();

			return(slot);
		}

		// Stores a hash keyed by strings of LocalDataStoreSlot objects
		static Hashtable datastorehash;
		private static object datastore_lock = new object ();
		
		private static void InitDataStoreHash () {
			lock (datastore_lock) {
				if (datastorehash == null) {
					datastorehash = Hashtable.Synchronized(new Hashtable());
				}
			}
		}
		
		public static LocalDataStoreSlot AllocateNamedDataSlot(string name) {
			lock (datastore_lock) {
				if (datastorehash == null)
					InitDataStoreHash ();
				LocalDataStoreSlot slot = (LocalDataStoreSlot)datastorehash[name];
				if(slot!=null) {
					// This exception isnt documented (of
					// course) but .net throws it
					throw new ArgumentException("Named data slot already added");
				}
			
				slot = new LocalDataStoreSlot();

				datastorehash.Add(name, slot);

				return(slot);
			}
		}

		public static void FreeNamedDataSlot(string name) {
			lock (datastore_lock) {
				if (datastorehash == null)
					InitDataStoreHash ();
				LocalDataStoreSlot slot=(LocalDataStoreSlot)datastorehash[name];

				if(slot!=null) {
					datastorehash.Remove(slot);
				}
			}
		}

		public static object GetData(LocalDataStoreSlot slot) {
			Hashtable slothash=GetTLSSlotHash();
			return(slothash[slot]);
		}

		public static AppDomain GetDomain() {
			return AppDomain.CurrentDomain;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int GetDomainID();

		public static LocalDataStoreSlot GetNamedDataSlot(string name) {
			lock (datastore_lock) {
				if (datastorehash == null)
					InitDataStoreHash ();
				LocalDataStoreSlot slot=(LocalDataStoreSlot)datastorehash[name];

				if(slot==null) {
					slot=AllocateNamedDataSlot(name);
				}
			
				return(slot);
			}
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void ResetAbort_internal();

		public static void ResetAbort()
		{
			ResetAbort_internal();
		}
		

		public static void SetData(LocalDataStoreSlot slot,
					   object data) {
			Hashtable slothash=GetTLSSlotHash();

			if(slothash.Contains(slot)) {
				slothash.Remove(slot);
			}
			
			slothash.Add(slot, data);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Sleep_internal(int ms);

		public static void Sleep(int millisecondsTimeout) {
			if((millisecondsTimeout<0) && (millisecondsTimeout != Timeout.Infinite)) {
				throw new ArgumentException("Negative timeout");
			}
			Thread thread=CurrentThread;
			Sleep_internal(millisecondsTimeout);
		}

		public static void Sleep(TimeSpan timeout) {
			// LAMESPEC: says to throw ArgumentException too
			int ms=Convert.ToInt32(timeout.TotalMilliseconds);
			
			if(ms < 0 || ms > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("Timeout out of range");
			}

			Thread thread=CurrentThread;
			Sleep_internal(ms);
		}

		// Returns the system thread handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern IntPtr Thread_internal(ThreadStart start);

		public Thread(ThreadStart start) {
			if(start==null) {
				throw new ArgumentNullException("Null ThreadStart");
			}
			threadstart=start;
		}

		[MonoTODO]
#if NET_2_0
		[Obsolete ("")]
#endif
		public ApartmentState ApartmentState {
			get {
				return(ApartmentState.Unknown);
			}
			
			set {
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern int current_lcid ();

		/* If the current_lcid() isn't known by CultureInfo,
		 * it will throw an exception which may cause
		 * String.Concat to try and recursively look up the
		 * CurrentCulture, which will throw an exception, etc.
		 * Use a boolean to short-circuit this scenario.
		 */
		private static bool in_currentculture=false;
		
		public CultureInfo CurrentCulture {
			get {
				if (current_culture == null) {
					lock (typeof (Thread)) {
						if(current_culture==null) {
							if(in_currentculture==true) {
								/* Bail out */
								current_culture = CultureInfo.InvariantCulture;
							} else {
								in_currentculture=true;
							
								current_culture = CultureInfo.ConstructCurrentCulture ();
							}
						}
						
						in_currentculture=false;
					}
				}
				
				return(current_culture);
			}
			
			set {
				current_culture = value;
			}
		}

		public CultureInfo CurrentUICulture {
			get {
				if (current_ui_culture == null) {
					lock (synch_lock) {
						if(current_ui_culture==null) {
							/* We don't
							 * distinguish
							 * between
							 * System and
							 * UI cultures
							 */
							current_ui_culture = CultureInfo.ConstructCurrentUICulture ();
						}
					}
				}
				
				return(current_ui_culture);
			}
			
			set {
				current_ui_culture = value;
			}
		}

		public bool IsThreadPoolThread {
			get {
				return IsThreadPoolThreadInternal;
			}
		}

		internal bool IsThreadPoolThreadInternal {
			get {
				return threadpool_thread;
			}
			set {
				threadpool_thread = value;
			}
		}

		public bool IsAlive {
			get {
				ThreadState curstate=state;
				
				if((curstate & ThreadState.Aborted) != 0 ||
				   (curstate & ThreadState.Stopped) != 0 ||
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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern string GetName_internal ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void SetName_internal (String name);

		/* 
		 * The thread name must be shared by appdomains, so it is stored in
		 * unmanaged code.
		 */

		public string Name {
			get {
				return GetName_internal ();
			}
			
			set {
				lock (synch_lock) {
					if(Name!=null) {
						throw new InvalidOperationException ("Thread.Name can only be set once.");
					}
				
					SetName_internal (value);
				}
			}
		}

		[MonoTODO]
		public ThreadPriority Priority {
			get {
				return(ThreadPriority.Lowest);
			}
			
			set {
			}
		}

		public ThreadState ThreadState {
			get {
				return(state);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Abort_internal (object stateInfo);

		public void Abort() {
			Abort_internal (null);
		}

		public void Abort(object stateInfo) {
			Abort_internal(stateInfo);
		}
		

		[MonoTODO]
		public void Interrupt() {
		}

		// The current thread joins with 'this'. Set ms to 0 to block
		// until this actually exits.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern bool Join_internal(int ms, IntPtr handle);
		
		public void Join() {
			if((state & ThreadState.Unstarted) != 0) {
				throw new ThreadStateException("Thread has not been started");
			}
			
			Thread thread=CurrentThread;
				
			Join_internal(Timeout.Infinite, system_thread_handle);
		}

		public bool Join(int millisecondsTimeout) {
			if (millisecondsTimeout != Timeout.Infinite && millisecondsTimeout < 0)
				throw new ArgumentException ("Timeout less than zero", "millisecondsTimeout");

			if((state & ThreadState.Unstarted) != 0) {
				throw new ThreadStateException("Thread has not been started");
			}

			Thread thread=CurrentThread;
			return Join_internal(millisecondsTimeout, system_thread_handle);
		}

		public bool Join(TimeSpan timeout) {
			// LAMESPEC: says to throw ArgumentException too
			int ms=Convert.ToInt32(timeout.TotalMilliseconds);
			
			if(ms < 0 || ms > Int32.MaxValue) {
				throw new ArgumentOutOfRangeException("timeout out of range");
			}
			if((state & ThreadState.Unstarted) != 0) {
				throw new ThreadStateException("Thread has not been started");
			}

			Thread thread=CurrentThread;
			return Join_internal(ms, system_thread_handle);
		}

#if NET_1_1
		[MonoTODO ("seems required for multi-processors systems like Itanium")]
		public static void MemoryBarrier ()
		{
			throw new NotImplementedException ();
		}
#endif
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Resume_internal();

#if NET_2_0
		[Obsolete ("")]
#endif
		public void Resume () 
		{
			if ((state & ThreadState.Unstarted) != 0 || !IsAlive || 
				((state & ThreadState.Suspended) == 0 && (state & ThreadState.SuspendRequested) == 0)) 
			{
				throw new ThreadStateException("Thread has not been started, or is dead");
			}
			
			Resume_internal ();
		}

		[MonoTODO]
		public static void SpinWait (int iterations) 
		{
			throw new NotImplementedException ();
		}

		// Launches the thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Start_internal(IntPtr handle);
		
		public void Start() {
			lock(synch_lock) {
				if((state & ThreadState.Unstarted) == 0) {
					throw new ThreadStateException("Thread has already been started");
				}
				

				// Thread_internal creates the new thread, but
				// blocks it until Start() is called later.
				system_thread_handle=Thread_internal(threadstart);

				if (system_thread_handle == (IntPtr) 0) {
					throw new SystemException ("Thread creation failed");
				}

				// Launch this thread
				Start_internal(system_thread_handle);

				// Mark the thread state as Running
				// (which is all bits
				// cleared). Therefore just remove the
				// Unstarted bit
				clr_state(ThreadState.Unstarted);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Suspend_internal();

#if NET_2_0
		[Obsolete ("")]
#endif
		public void Suspend() {
			if((state & ThreadState.Unstarted) != 0 || !IsAlive) {
				throw new ThreadStateException("Thread has not been started, or is dead");
			}
			Suspend_internal ();
		}

		// Closes the system thread handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Thread_free_internal(IntPtr handle);

		~Thread() {
			// Free up the handle
			if (system_thread_handle != (IntPtr) 0)
				Thread_free_internal(system_thread_handle);
		}

		private void set_state(ThreadState set) {
			lock(synch_lock) {
				state |= set;
			}
		}
		private void clr_state(ThreadState clr) {
			lock(synch_lock) {
				state &= ~clr;
			}
		}

#if NET_1_1
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static byte VolatileRead (ref byte address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static double VolatileRead (ref double address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static short VolatileRead (ref short address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static int VolatileRead (ref int address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static long VolatileRead (ref long address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static IntPtr VolatileRead (ref IntPtr address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static object VolatileRead (ref object address);

		[CLSCompliant(false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static sbyte VolatileRead (ref sbyte address);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static float VolatileRead (ref float address);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static ushort VolatileRead (ref ushort address);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static uint VolatileRead (ref uint address);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static ulong VolatileRead (ref ulong address);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static UIntPtr VolatileRead (ref UIntPtr address);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref byte address, byte value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref double address, double value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref short address, short value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref int address, int value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref long address, long value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref IntPtr address, IntPtr value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref object address, object value);

		[CLSCompliant(false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref sbyte address, sbyte value);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref float address, float value);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref ushort address, ushort value);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref uint address, uint value);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref ulong address, ulong value);

		[CLSCompliant (false)]
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern public static void VolatileWrite (ref UIntPtr address, UIntPtr value);
		
#endif

#if NET_2_0
		[MonoTODO ("stack size is ignored")]
		public Thread (ThreadStart start, int maxStackSize)
		{
			if (start == null)
				throw new ArgumentNullException ("start");
			if (maxStackSize < 131072)
				throw new ArgumentException ("< 128 kb", "maxStackSize");

			threadstart=start;
		}

		[MonoTODO]
		public Thread (ParameterizedThreadStart start)
		{
			if (start == null)
				throw new ArgumentNullException ("start");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Thread (ParameterizedThreadStart start, int maxStackSize)
		{
			if (start == null)
				throw new ArgumentNullException ("start");
			if (maxStackSize < 131072)
				throw new ArgumentException ("< 128 kb", "maxStackSize");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ExecutionContext ExecutionContext {
			[ReliabilityContract (Consistency.WillNotCorruptState, CER.MayFail)]
			get { throw new NotImplementedException (); }
		}

		public int ManagedThreadId {
			get { return thread_id; }
		}

		[MonoTODO]
		[ReliabilityContract (Consistency.WillNotCorruptState, CER.MayFail)]
		public static void BeginCriticalRegion ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		[ReliabilityContract (Consistency.WillNotCorruptState, CER.Success)]
		public static void EndCriticalRegion ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void BeginThreadAffinity ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void EndThreadAffinity ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ApartmentState GetApartmentState ()
		{
			return this.ApartmentState;
		}

		[MonoTODO]
		public void SetApartmentState (ApartmentState state)
		{
			this.ApartmentState = state;
		}

		[MonoTODO]
		public bool TrySetApartmentState (ApartmentState state)
		{
			try {
				this.ApartmentState = state;
				return true;
			}
			catch (ArgumentException) {
				throw;
			}
			catch {
				return false;
			}
		}

		[MonoTODO]
		public CompressedStack GetCompressedStack ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetCompressedStack (CompressedStack stack)
		{
			throw new NotImplementedException ();
		}

		[ComVisible (false)]
		public override int GetHashCode ()
		{
			// ??? overridden but not guaranteed to be unique ???
			return thread_id;
		}

		[MonoTODO]
		public void Start (object parameter)
		{
			throw new NotImplementedException ();
		}
#endif
		
	}
}
