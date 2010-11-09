//
// System.Threading.Thread.cs
//
// Author:
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Security.Principal;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Security;
using System.Runtime.ConstrainedExecution;

namespace System.Threading {

	internal class InternalThread : CriticalFinalizerObject {
#pragma warning disable 169, 414, 649
		#region Sync with metadata/object-internals.h
		int lock_thread_id;
		// stores a thread handle
		internal IntPtr system_thread_handle;

		/* Note this is an opaque object (an array), not a CultureInfo */
		private object cached_culture_info;
		private IntPtr unused0;
		internal bool threadpool_thread;
		/* accessed only from unmanaged code */
		private IntPtr name;
		private int name_len; 
		private ThreadState state;
		private object abort_exc;
		private int abort_state_handle;
		/* thread_id is only accessed from unmanaged code */
		internal Int64 thread_id;
		
		/* start_notify is used by the runtime to signal that Start()
		 * is ok to return
		 */
		private IntPtr start_notify;
		private IntPtr stack_ptr;
		private UIntPtr static_data; /* GC-tracked */
		private IntPtr jit_data;
		private IntPtr lock_data;
		/* current System.Runtime.Remoting.Contexts.Context instance
		   keep as an object to avoid triggering its class constructor when not needed */
		private object current_appcontext;
		internal int stack_size;
		private IntPtr appdomain_refs;
		private int interruption_requested;
		private IntPtr suspend_event;
		private IntPtr suspended_event;
		private IntPtr resume_event;
		private IntPtr synch_cs;
		private bool thread_dump_requested;
		private IntPtr end_stack;
		private bool thread_interrupt_requested;
		internal byte apartment_state;
		internal volatile int critical_region_level;
		private int small_id;
		private IntPtr manage_callback;
		private object pending_exception;
		/* This is the ExecutionContext that will be set by
		   start_wrapper() in the runtime. */
		private ExecutionContext ec_to_set;

		private IntPtr interrupt_on_stop;

		/* 
		 * These fields are used to avoid having to increment corlib versions
		 * when a new field is added to the unmanaged MonoThread structure.
		 */
		private IntPtr unused3;
		private IntPtr unused4;
		private IntPtr unused5;
		private IntPtr unused6;
		#endregion
#pragma warning restore 169, 414, 649

		internal int managed_id;

		internal byte[] _serialized_principal;
		internal int _serialized_principal_version;

		internal byte[] serialized_culture_info;
		internal byte[] serialized_ui_culture_info;

		/* If the current_lcid() isn't known by CultureInfo,
		 * it will throw an exception which may cause
		 * String.Concat to try and recursively look up the
		 * CurrentCulture, which will throw an exception, etc.
		 * Use a boolean to short-circuit this scenario.
		 */
		internal bool in_currentculture=false;

		// Closes the system thread handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Thread_free_internal(IntPtr handle);

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		~InternalThread() {
			Thread_free_internal(system_thread_handle);
		}
	}

	[ClassInterface (ClassInterfaceType.None)]
	[ComVisible (true)]
	[ComDefaultInterface (typeof (_Thread))]
	public sealed class Thread : CriticalFinalizerObject, _Thread {
#pragma warning disable 414		
		#region Sync with metadata/object-internals.h
		private InternalThread internal_thread;
		object start_obj;
		private ExecutionContext ec_to_set;
		#endregion
#pragma warning restore 414

		IPrincipal principal;
		int principal_version;

		// the name of local_slots, current_thread and _ec is
		// important because they are used by the runtime.
		[ThreadStatic]
		static object[] local_slots;

		[ThreadStatic]
		static Thread current_thread;

		/* The actual ExecutionContext of the thread.  It's
		   ThreadStatic so that it's not shared between
		   AppDomains. */
		[ThreadStatic]
		static ExecutionContext _ec;

		// can be both a ThreadStart and a ParameterizedThreadStart
		private MulticastDelegate threadstart;
		//private string thread_name=null;

		private static int _managed_id_counter;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void ConstructInternalThread ();

		private InternalThread Internal {
			get {
				if (internal_thread == null)
					ConstructInternalThread ();
				return internal_thread;
			}
		}

		public static Context CurrentContext {
			[SecurityPermission (SecurityAction.LinkDemand, Infrastructure=true)]
			get {
				return(AppDomain.InternalGetContext ());
			}
		}

		/*
		 * These two methods return an array in the target
		 * domain with the same content as the argument.  If
		 * the argument is already in the target domain, then
		 * the argument is returned, otherwise a copy.
		 */
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static byte[] ByteArrayToRootDomain (byte[] arr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static byte[] ByteArrayToCurrentDomain (byte[] arr);

#if !MOONLIGHT
		public static IPrincipal CurrentPrincipal {
			get {
				Thread th = CurrentThread;

				if (th.principal_version != th.Internal._serialized_principal_version)
					th.principal = null;

				if (th.principal != null)
					return th.principal;

				if (th.Internal._serialized_principal != null) {
					try {
						BinaryFormatter bf = new BinaryFormatter ();
						MemoryStream ms = new MemoryStream (ByteArrayToCurrentDomain (th.Internal._serialized_principal));
						th.principal = (IPrincipal) bf.Deserialize (ms);
						th.principal_version = th.Internal._serialized_principal_version;
						return th.principal;
					} catch (Exception) {
					}
				}

				th.principal = GetDomain ().DefaultPrincipal;
				th.principal_version = th.Internal._serialized_principal_version;
				return th.principal;
			}
			[SecurityPermission (SecurityAction.Demand, ControlPrincipal = true)]
			set {
				Thread th = CurrentThread;

				++th.Internal._serialized_principal_version;
				try {
					BinaryFormatter bf = new BinaryFormatter ();
					MemoryStream ms = new MemoryStream ();
					bf.Serialize (ms, value);
					th.Internal._serialized_principal = ByteArrayToRootDomain (ms.ToArray ());
				} catch (Exception) {
					th.Internal._serialized_principal = null;
				}

				th.principal = value;
				th.principal_version = th.Internal._serialized_principal_version;
			}
		}
#endif

		// Looks up the object associated with the current thread
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static InternalThread CurrentInternalThread_internal();

		public static Thread CurrentThread {
			[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
			get {
				if (current_thread == null)
					current_thread = new Thread (CurrentInternalThread_internal ());
				return current_thread;
			}
		}

		internal static int CurrentThreadId {
			get {
				return (int)(CurrentThread.internal_thread.thread_id);
			}
		}

#if !MOONLIGHT
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
		
		public static LocalDataStoreSlot AllocateNamedDataSlot (string name) {
			lock (datastore_lock) {
				if (datastorehash == null)
					InitDataStoreHash ();
				LocalDataStoreSlot slot = (LocalDataStoreSlot)datastorehash [name];
				if (slot != null) {
					// This exception isnt documented (of
					// course) but .net throws it
					throw new ArgumentException("Named data slot already added");
				}
			
				slot = AllocateDataSlot ();

				datastorehash.Add (name, slot);

				return slot;
			}
		}

		public static void FreeNamedDataSlot (string name) {
			lock (datastore_lock) {
				if (datastorehash == null)
					InitDataStoreHash ();
				LocalDataStoreSlot slot = (LocalDataStoreSlot)datastorehash [name];

				if (slot != null) {
					datastorehash.Remove (slot);
				}
			}
		}

		public static LocalDataStoreSlot AllocateDataSlot () {
			return new LocalDataStoreSlot (true);
		}

		public static object GetData (LocalDataStoreSlot slot) {
			object[] slots = local_slots;
			if (slot == null)
				throw new ArgumentNullException ("slot");
			if (slots != null && slot.slot < slots.Length)
				return slots [slot.slot];
			return null;
		}

		public static void SetData (LocalDataStoreSlot slot, object data) {
			object[] slots = local_slots;
			if (slot == null)
				throw new ArgumentNullException ("slot");
			if (slots == null) {
				slots = new object [slot.slot + 2];
				local_slots = slots;
			} else if (slot.slot >= slots.Length) {
				object[] nslots = new object [slot.slot + 2];
				slots.CopyTo (nslots, 0);
				slots = nslots;
				local_slots = slots;
			}
			slots [slot.slot] = data;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void FreeLocalSlotValues (int slot, bool thread_local);

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
#endif
		
		public static AppDomain GetDomain() {
			return AppDomain.CurrentDomain;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int GetDomainID();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void ResetAbort_internal();

		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public static void ResetAbort ()
		{
			ResetAbort_internal ();
		}

#if NET_4_0 || BOOTSTRAP_NET_4_0
		[HostProtectionAttribute (SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static bool Yield ();
#endif


		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Sleep_internal(int ms);

		public static void Sleep (int millisecondsTimeout)
		{
			if (millisecondsTimeout < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout", "Negative timeout");

			Sleep_internal (millisecondsTimeout);
		}

		public static void Sleep (TimeSpan timeout)
		{
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < Timeout.Infinite || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout", "timeout out of range");

			Sleep_internal ((int) ms);
		}

		// Returns the system thread handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern IntPtr Thread_internal (MulticastDelegate start);

		public Thread(ThreadStart start) {
			if(start==null) {
				throw new ArgumentNullException("Null ThreadStart");
			}
			threadstart=start;
		}

		private Thread (InternalThread it) {
			internal_thread = it;
		}

#if !MOONLIGHT
		[Obsolete ("Deprecated in favor of GetApartmentState, SetApartmentState and TrySetApartmentState.")]
		public ApartmentState ApartmentState {
			get {
				if ((ThreadState & ThreadState.Stopped) != 0)
					throw new ThreadStateException ("Thread is dead; state can not be accessed.");

				return (ApartmentState)Internal.apartment_state;
			}

			set {
				TrySetApartmentState (value);
			}
		}
#endif // !NET_2_1

		//[MethodImplAttribute (MethodImplOptions.InternalCall)]
		//private static extern int current_lcid ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static CultureInfo GetCachedCurrentCulture (InternalThread thread);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void SetCachedCurrentCulture (CultureInfo culture);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static CultureInfo GetCachedCurrentUICulture (InternalThread thread);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void SetCachedCurrentUICulture (CultureInfo culture);

		/* FIXME: in_currentculture exists once, but
		   culture_lock is once per appdomain.  Is it correct
		   to lock this way? */
		static object culture_lock = new object ();
		
		/*
		 * Thread objects are shared between appdomains, and CurrentCulture
		 * should always return an object in the calling appdomain. See bug
		 * http://bugzilla.ximian.com/show_bug.cgi?id=50049 for more info.
		 * This is hard to implement correctly and efficiently, so the current
		 * implementation is not perfect: changes made in one appdomain to the 
		 * state of the current cultureinfo object are not visible to other 
		 * appdomains.
		 */		
		public CultureInfo CurrentCulture {
			get {
				if (Internal.in_currentculture)
					/* Bail out */
					return CultureInfo.InvariantCulture;

				CultureInfo culture = GetCachedCurrentCulture (Internal);
				if (culture != null)
					return culture;

				byte[] arr = ByteArrayToCurrentDomain (Internal.serialized_culture_info);
				if (arr == null) {
					lock (culture_lock) {
						Internal.in_currentculture=true;
						culture = CultureInfo.ConstructCurrentCulture ();
						//
						// Don't serialize the culture in this case to avoid
						// initializing the serialization infrastructure in the
						// common case when the culture is not set explicitly.
						//
						SetCachedCurrentCulture (culture);
						Internal.in_currentculture = false;
						NumberFormatter.SetThreadCurrentCulture (culture);
						return culture;
					}
				}

				/*
				 * No cultureinfo object exists for this domain, so create one
				 * by deserializing the serialized form.
				 */
				Internal.in_currentculture = true;
				try {
					BinaryFormatter bf = new BinaryFormatter ();
					MemoryStream ms = new MemoryStream (arr);
					culture = (CultureInfo)bf.Deserialize (ms);
					SetCachedCurrentCulture (culture);
				} finally {
					Internal.in_currentculture = false;
				}

				NumberFormatter.SetThreadCurrentCulture (culture);
				return culture;
			}
			
			[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				CultureInfo culture = GetCachedCurrentCulture (Internal);
				if (culture == value)
					return;

				value.CheckNeutral ();
				Internal.in_currentculture = true;
				try {
					SetCachedCurrentCulture (value);

					byte[] serialized_form = null;

					if (value.IsReadOnly && value.cached_serialized_form != null) {
						serialized_form = value.cached_serialized_form;
					} else {
						BinaryFormatter bf = new BinaryFormatter();
						MemoryStream ms = new MemoryStream ();
						bf.Serialize (ms, value);

						serialized_form = ms.GetBuffer ();
						if (value.IsReadOnly)
							value.cached_serialized_form = serialized_form;
					}

					Internal.serialized_culture_info = ByteArrayToRootDomain (serialized_form);
				} finally {
					Internal.in_currentculture = false;
				}
				NumberFormatter.SetThreadCurrentCulture (value);
			}
		}

		public CultureInfo CurrentUICulture {
			get {
				if (Internal.in_currentculture)
					/* Bail out */
					return CultureInfo.InvariantCulture;

				CultureInfo culture = GetCachedCurrentUICulture (Internal);
				if (culture != null)
					return culture;

				byte[] arr = ByteArrayToCurrentDomain (Internal.serialized_ui_culture_info);
				if (arr == null) {
					lock (culture_lock) {
						Internal.in_currentculture=true;
						/* We don't
						 * distinguish
						 * between
						 * System and
						 * UI cultures
						 */
						culture = CultureInfo.ConstructCurrentUICulture ();
						//
						// Don't serialize the culture in this case to avoid
						// initializing the serialization infrastructure in the
						// common case when the culture is not set explicitly.
						//
						SetCachedCurrentUICulture (culture);
						Internal.in_currentculture = false;
						return culture;
					}
				}

				/*
				 * No cultureinfo object exists for this domain, so create one
				 * by deserializing the serialized form.
				 */
				Internal.in_currentculture = true;
				try {
					BinaryFormatter bf = new BinaryFormatter ();
					MemoryStream ms = new MemoryStream (arr);
					culture = (CultureInfo)bf.Deserialize (ms);
					SetCachedCurrentUICulture (culture);
				}
				finally {
					Internal.in_currentculture = false;
				}

				return culture;
			}
			
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				CultureInfo culture = GetCachedCurrentUICulture (Internal);
				if (culture == value)
					return;

				Internal.in_currentculture = true;
				try {
					SetCachedCurrentUICulture (value);

					byte[] serialized_form = null;

					if (value.IsReadOnly && value.cached_serialized_form != null) {
						serialized_form = value.cached_serialized_form;
					} else {
						BinaryFormatter bf = new BinaryFormatter();
						MemoryStream ms = new MemoryStream ();
						bf.Serialize (ms, value);

						serialized_form = ms.GetBuffer ();
						if (value.IsReadOnly)
							value.cached_serialized_form = serialized_form;
					}

					Internal.serialized_ui_culture_info = ByteArrayToRootDomain (serialized_form);
				} finally {
					Internal.in_currentculture = false;
				}
			}
		}

		public bool IsThreadPoolThread {
			get {
				return IsThreadPoolThreadInternal;
			}
		}

		internal bool IsThreadPoolThreadInternal {
			get {
				return Internal.threadpool_thread;
			}
			set {
				Internal.threadpool_thread = value;
			}
		}

		public bool IsAlive {
			get {
				ThreadState curstate = GetState (Internal);
				
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
				ThreadState thread_state = GetState (Internal);
				if ((thread_state & ThreadState.Stopped) != 0)
					throw new ThreadStateException ("Thread is dead; state can not be accessed.");

				return (thread_state & ThreadState.Background) != 0;
			}
			
			set {
				if (value) {
					SetState (Internal, ThreadState.Background);
				} else {
					ClrState (Internal, ThreadState.Background);
				}
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static string GetName_internal (InternalThread thread);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void SetName_internal (InternalThread thread, String name);

		/* 
		 * The thread name must be shared by appdomains, so it is stored in
		 * unmanaged code.
		 */

		public string Name {
			get {
				return GetName_internal (Internal);
			}
			
			set {
				SetName_internal (Internal, value);
			}
		}

#if !MOONLIGHT
		public ThreadPriority Priority {
			get {
				return(ThreadPriority.Lowest);
			}
			
			set {
				// FIXME: Implement setter.
			}
		}
#endif

		public ThreadState ThreadState {
			get {
				return GetState (Internal);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Abort_internal (InternalThread thread, object stateInfo);

		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public void Abort () 
		{
			Abort_internal (Internal, null);
		}

#if !MOONLIGHT
		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public void Abort (object stateInfo) 
		{
			Abort_internal (Internal, stateInfo);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern object GetAbortExceptionState ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void Interrupt_internal (InternalThread thread);
		
		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public void Interrupt ()
		{
			Interrupt_internal (Internal);
		}
#endif

		// The current thread joins with 'this'. Set ms to 0 to block
		// until this actually exits.
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static bool Join_internal(InternalThread thread, int ms, IntPtr handle);
		
		public void Join()
		{
			Join_internal(Internal, Timeout.Infinite, Internal.system_thread_handle);
		}

		public bool Join(int millisecondsTimeout)
		{
			if (millisecondsTimeout < Timeout.Infinite)
				throw new ArgumentOutOfRangeException ("millisecondsTimeout", "Timeout less than zero");

			return Join_internal (Internal, millisecondsTimeout, Internal.system_thread_handle);
		}

#if !MOONLIGHT
		public bool Join(TimeSpan timeout)
		{
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < Timeout.Infinite || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout", "timeout out of range");

			return Join_internal (Internal, (int) ms, Internal.system_thread_handle);
		}
#endif

#if NET_1_1
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void MemoryBarrier ();
#endif

#if !MOONLIGHT
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Resume_internal();

		[Obsolete ("")]
		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public void Resume () 
		{
			Resume_internal ();
		}
#endif // !NET_2_1

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void SpinWait_nop ();


		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public static void SpinWait (int iterations) 
		{
			if (iterations < 0)
				return;
			while (iterations-- > 0)
			{
				SpinWait_nop ();
			}
		}

#if MOONLIGHT
		private void StartSafe ()
		{
			current_thread = this;

			try {
				if (threadstart is ThreadStart) {
					((ThreadStart) threadstart) ();
				} else {
					((ParameterizedThreadStart) threadstart) (start_obj);
				}
			} catch (ThreadAbortException) {
				// do nothing
			} catch (Exception ex) {
				MoonlightUnhandledException (ex);
			}
		}

		static MethodInfo moonlight_unhandled_exception = null;

		static internal void MoonlightUnhandledException (Exception e)
		{
			try {
				if (moonlight_unhandled_exception == null) {
					var assembly = System.Reflection.Assembly.Load ("System.Windows, Version=2.0.5.0, Culture=Neutral, PublicKeyToken=7cec85d7bea7798e");
					var application = assembly.GetType ("System.Windows.Application");
					moonlight_unhandled_exception = application.GetMethod ("OnUnhandledException", 
						System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
				}
				moonlight_unhandled_exception.Invoke (null, new object [] { null, e });
			}
			catch {
				try {
					Console.WriteLine ("Unexpected exception while trying to report unhandled application exception: {0}", e);
				} catch {
				}
			}
		}
#endif

		private void StartUnsafe ()
		{
			current_thread = this;

			if (threadstart is ThreadStart) {
				((ThreadStart) threadstart) ();
			} else {
				((ParameterizedThreadStart) threadstart) (start_obj);
			}
		}

		public void Start() {
			// propagate informations from the original thread to the new thread
			if (!ExecutionContext.IsFlowSuppressed ())
				ec_to_set = ExecutionContext.Capture ();
			Internal._serialized_principal = CurrentThread.Internal._serialized_principal;

			// Thread_internal creates and starts the new thread, 
#if MOONLIGHT
			if (Thread_internal((ThreadStart) StartSafe) == (IntPtr) 0)
#else
			if (Thread_internal((ThreadStart) StartUnsafe) == (IntPtr) 0)
#endif
				throw new SystemException ("Thread creation failed.");
		}

#if !MOONLIGHT
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Suspend_internal(InternalThread thread);

		[Obsolete ("")]
		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public void Suspend ()
		{
			Suspend_internal (Internal);
		}
#endif // !NET_2_1

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern private static void SetState (InternalThread thread, ThreadState set);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern private static void ClrState (InternalThread thread, ThreadState clr);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern private static ThreadState GetState (InternalThread thread);

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

		private static int GetNewManagedId() {
			return Interlocked.Increment(ref _managed_id_counter);
		}

		public Thread (ThreadStart start, int maxStackSize)
		{
			if (start == null)
				throw new ArgumentNullException ("start");
			if (maxStackSize < 131072)
				throw new ArgumentException ("< 128 kb", "maxStackSize");

			threadstart = start;
			Internal.stack_size = maxStackSize;
		}

		public Thread (ParameterizedThreadStart start)
		{
			if (start == null)
				throw new ArgumentNullException ("start");

			threadstart = start;
		}

		public Thread (ParameterizedThreadStart start, int maxStackSize)
		{
			if (start == null)
				throw new ArgumentNullException ("start");
			if (maxStackSize < 131072)
				throw new ArgumentException ("< 128 kb", "maxStackSize");

			threadstart = start;
			Internal.stack_size = maxStackSize;
		}

		[MonoTODO ("limited to CompressedStack support")]
		public ExecutionContext ExecutionContext {
			[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
			get {
				if (_ec == null)
					_ec = new ExecutionContext ();
				return _ec;
			}
		}

		public int ManagedThreadId {
			[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
			get {
				// Fastpath
				if (internal_thread != null && internal_thread.managed_id != 0)
					return internal_thread.managed_id;

				if (Internal.managed_id == 0) {
					int new_managed_id = GetNewManagedId ();
					
					Interlocked.CompareExchange (ref Internal.managed_id, new_managed_id, 0);
				}
				
				return Internal.managed_id;
			}
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void BeginCriticalRegion ()
		{
			CurrentThread.Internal.critical_region_level++;
		}

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		public static void EndCriticalRegion ()
		{
			CurrentThread.Internal.critical_region_level--;
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void BeginThreadAffinity ()
		{
			// Managed and native threads are currently bound together.
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.MayFail)]
		public static void EndThreadAffinity ()
		{
			// Managed and native threads are currently bound together.
		}

#if !MOONLIGHT
		public ApartmentState GetApartmentState ()
		{
			return (ApartmentState)Internal.apartment_state;
		}

		public void SetApartmentState (ApartmentState state)
		{
			if (!TrySetApartmentState (state))
				throw new InvalidOperationException ("Failed to set the specified COM apartment state.");
		}

		public bool TrySetApartmentState (ApartmentState state) 
		{
			/* Only throw this exception when changing the
			 * state of another thread.  See bug 324338
			 */
			if ((this != CurrentThread) &&
			    (ThreadState & ThreadState.Unstarted) == 0)
				throw new ThreadStateException ("Thread was in an invalid state for the operation being executed.");

			if ((ApartmentState)Internal.apartment_state != ApartmentState.Unknown)
				return false;

			Internal.apartment_state = (byte)state;

			return true;
		}
#endif // !NET_2_1
		
		[ComVisible (false)]
		public override int GetHashCode ()
		{
			return ManagedThreadId;
		}

		public void Start (object parameter)
		{
			start_obj = parameter;
			Start ();
		}

#if !MOONLIGHT
		// NOTE: This method doesn't show in the class library status page because
		// it cannot be "found" with the StrongNameIdentityPermission for ECMA key.
		// But it's there!
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		[StrongNameIdentityPermission (SecurityAction.LinkDemand, PublicKey="00000000000000000400000000000000")]
		[Obsolete ("see CompressedStack class")]
#if NET_1_1
		public
#else
		internal
#endif
		CompressedStack GetCompressedStack ()
		{
			// Note: returns null if no CompressedStack has been set.
			// However CompressedStack.GetCompressedStack returns an 
			// (empty?) CompressedStack instance.
			CompressedStack cs = ExecutionContext.SecurityContext.CompressedStack;
			return ((cs == null) || cs.IsEmpty ()) ? null : cs.CreateCopy ();
		}

		// NOTE: This method doesn't show in the class library status page because
		// it cannot be "found" with the StrongNameIdentityPermission for ECMA key.
		// But it's there!
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		[StrongNameIdentityPermission (SecurityAction.LinkDemand, PublicKey="00000000000000000400000000000000")]
		[Obsolete ("see CompressedStack class")]
#if NET_1_1
		public
#else
		internal
#endif
		void SetCompressedStack (CompressedStack stack)
		{
			ExecutionContext.SecurityContext.CompressedStack = stack;
		}

#endif

#if NET_1_1
		void _Thread.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _Thread.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Thread.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _Thread.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams,
			IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
