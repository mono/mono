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
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Runtime.ConstrainedExecution;

namespace System.Threading {
	[StructLayout (LayoutKind.Sequential)]
	internal class InternalThread : CriticalFinalizerObject {
#pragma warning disable 169, 414, 649
		#region Sync with metadata/object-internals.h
		int lock_thread_id;
		// stores a thread handle
		internal IntPtr system_thread_handle;

		/* Note this is an opaque object (an array), not a CultureInfo */
		private object cached_culture_info;
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
		private IntPtr runtime_thread_info;
		/* current System.Runtime.Remoting.Contexts.Context instance
		   keep as an object to avoid triggering its class constructor when not needed */
		private object current_appcontext;
		private object pending_exception;
		private object root_domain_thread;
		internal byte[] _serialized_principal;
		internal int _serialized_principal_version;
		private IntPtr appdomain_refs;
		private int interruption_requested;
		private IntPtr suspend_event;
		private IntPtr suspended_event;
		private IntPtr resume_event;
		private IntPtr synch_cs;
		internal bool threadpool_thread;
		private bool thread_dump_requested;
		private bool thread_interrupt_requested;
		private IntPtr end_stack;
		/* These are used from managed code */
		internal int stack_size;
		internal byte apartment_state;
		internal volatile int critical_region_level;
		internal int managed_id;
		private int small_id;
		private IntPtr manage_callback;
		private IntPtr interrupt_on_stop;
		private IntPtr flags;
		private IntPtr android_tid;
		private IntPtr thread_pinning_ref;
		private int ignore_next_signal;
		/* 
		 * These fields are used to avoid having to increment corlib versions
		 * when a new field is added to the unmanaged MonoThread structure.
		 */
		private IntPtr unused0;
		private IntPtr unused1;
		private IntPtr unused2;
		#endregion
#pragma warning restore 169, 414, 649

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
	[StructLayout (LayoutKind.Sequential)]
#if MOBILE
	public sealed class Thread : CriticalFinalizerObject {
#else
	public sealed class Thread : CriticalFinalizerObject, _Thread {
#endif
#pragma warning disable 414		
		#region Sync with metadata/object-internals.h
		private InternalThread internal_thread;
		object start_obj;
		private ExecutionContext ec_to_set;
		#endregion
#pragma warning restore 414

		IPrincipal principal;
		int principal_version;
		CultureInfo current_culture;
		CultureInfo current_ui_culture;

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

		static NamedDataSlot namedDataSlot;		

		// can be both a ThreadStart and a ParameterizedThreadStart
		private MulticastDelegate threadstart;
		//private string thread_name=null;

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

		static void DeserializePrincipal (Thread th)
		{
			MemoryStream ms = new MemoryStream (ByteArrayToCurrentDomain (th.Internal._serialized_principal));
			int type = ms.ReadByte ();
			if (type == 0) {
				BinaryFormatter bf = new BinaryFormatter ();
				th.principal = (IPrincipal) bf.Deserialize (ms);
				th.principal_version = th.Internal._serialized_principal_version;
			} else if (type == 1) {
				BinaryReader reader = new BinaryReader (ms);
				string name = reader.ReadString ();
				string auth_type = reader.ReadString ();
				int n_roles = reader.ReadInt32 ();
				string [] roles = null;
				if (n_roles >= 0) {
					roles = new string [n_roles];
					for (int i = 0; i < n_roles; i++)
						roles [i] = reader.ReadString ();
				}
				th.principal = new GenericPrincipal (new GenericIdentity (name, auth_type), roles);
			} else if (type == 2 || type == 3) {
				string [] roles = type == 2 ? null : new string [0];
				th.principal = new GenericPrincipal (new GenericIdentity ("", ""), roles);
			}
		}

		static void SerializePrincipal (Thread th, IPrincipal value)
		{
			MemoryStream ms = new MemoryStream ();
			bool done = false;
			if (value.GetType () == typeof (GenericPrincipal)) {
				GenericPrincipal gp = (GenericPrincipal) value;
				if (gp.Identity != null && gp.Identity.GetType () == typeof (GenericIdentity)) {
					GenericIdentity id = (GenericIdentity) gp.Identity;
					if (id.Name == "" && id.AuthenticationType == "") {
						if (gp.Roles == null) {
							ms.WriteByte (2);
							done = true;
						} else if (gp.Roles.Length == 0) {
							ms.WriteByte (3);
							done = true;
						}
					} else {
						ms.WriteByte (1);
						BinaryWriter br = new BinaryWriter (ms);
						br.Write (gp.Identity.Name);
						br.Write (gp.Identity.AuthenticationType);
						string [] roles = gp.Roles;
						if  (roles == null) {
							br.Write ((int) (-1));
						} else {
							br.Write (roles.Length);
							foreach (string s in roles) {
								br.Write (s);
							}
						}
						br.Flush ();
						done = true;
					}
				}
			}
			if (!done) {
				ms.WriteByte (0);
				BinaryFormatter bf = new BinaryFormatter ();
				try {
					bf.Serialize (ms, value);
				} catch {}
			}
			th.Internal._serialized_principal = ByteArrayToRootDomain (ms.ToArray ());
		}

		public static IPrincipal CurrentPrincipal {
			get {
				Thread th = CurrentThread;

				if (th.principal_version != th.Internal._serialized_principal_version)
					th.principal = null;

				if (th.principal != null)
					return th.principal;

				if (th.Internal._serialized_principal != null) {
					try {
						DeserializePrincipal (th);
						return th.principal;
					} catch {}
				}

				th.principal = GetDomain ().DefaultPrincipal;
				th.principal_version = th.Internal._serialized_principal_version;
				return th.principal;
			}
			[SecurityPermission (SecurityAction.Demand, ControlPrincipal = true)]
			set {
				Thread th = CurrentThread;

				if (value != GetDomain ().DefaultPrincipal) {
					++th.Internal._serialized_principal_version;
					try {
						SerializePrincipal (th, value);
					} catch (Exception) {
						th.Internal._serialized_principal = null;
					}
					th.principal_version = th.Internal._serialized_principal_version;
				} else {
					th.Internal._serialized_principal = null;
				}

				th.principal = value;
			}
		}

		// Looks up the object associated with the current thread
		// this is called by the JIT directly, too
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static InternalThread CurrentInternalThread_internal();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static uint AllocTlsData (Type type);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static void DestroyTlsData (uint offset);

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
		
		static NamedDataSlot NamedDataSlot {
			get {
				if (namedDataSlot == null)
					Interlocked.CompareExchange (ref namedDataSlot, new NamedDataSlot (), null);

				return namedDataSlot;
			}
		}
		
		public static LocalDataStoreSlot AllocateNamedDataSlot (string name)
		{
			return NamedDataSlot.Allocate (name);
		}

		public static void FreeNamedDataSlot (string name)
		{
			NamedDataSlot.Free (name);
		}

		public static LocalDataStoreSlot AllocateDataSlot ()
		{
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

		public static LocalDataStoreSlot GetNamedDataSlot(string name)
	 	{
	 		return NamedDataSlot.Get (name);
		}
		
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

#if NET_4_0
		[HostProtectionAttribute (SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
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
		
		// part of ".NETPortable,Version=v4.0,Profile=Profile3" i.e. FX4 and SL4
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		~Thread ()
		{
		}

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

		//[MethodImplAttribute (MethodImplOptions.InternalCall)]
		//private static extern int current_lcid ();

		public CultureInfo CurrentCulture {
			get {
				CultureInfo culture = current_culture;
				if (culture != null)
					return culture;

				current_culture = culture = CultureInfo.ConstructCurrentCulture ();
				NumberFormatter.SetThreadCurrentCulture (culture);
				return culture;
			}
			
			[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				value.CheckNeutral ();
				current_culture = value;
				NumberFormatter.SetThreadCurrentCulture (value);
			}
		}

		public CultureInfo CurrentUICulture {
			get {
				CultureInfo culture = current_ui_culture;
				if (culture != null)
					return culture;

				current_ui_culture = culture = CultureInfo.ConstructCurrentUICulture ();
				return culture;
			}
			
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
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

		public ThreadPriority Priority {
			get {
				return(ThreadPriority.Lowest);
			}
			
			set {
				// FIXME: Implement setter.
			}
		}

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

		public bool Join(TimeSpan timeout)
		{
			long ms = (long) timeout.TotalMilliseconds;
			if (ms < Timeout.Infinite || ms > Int32.MaxValue)
				throw new ArgumentOutOfRangeException ("timeout", "timeout out of range");

			return Join_internal (Internal, (int) ms, Internal.system_thread_handle);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static void MemoryBarrier ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Resume_internal();

		[Obsolete ("")]
		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public void Resume () 
		{
			Resume_internal ();
		}

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

		private void StartInternal ()
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
			if (Thread_internal((ThreadStart) StartInternal) == (IntPtr) 0)
				throw new SystemException ("Thread creation failed.");
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Suspend_internal(InternalThread thread);

		[Obsolete ("")]
		[SecurityPermission (SecurityAction.Demand, ControlThread=true)]
		public void Suspend ()
		{
			Suspend_internal (Internal);
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern private static void SetState (InternalThread thread, ThreadState set);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern private static void ClrState (InternalThread thread, ThreadState clr);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern private static ThreadState GetState (InternalThread thread);

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
		

		static int CheckStackSize (int maxStackSize)
		{
			if (maxStackSize < 0)
				throw new ArgumentOutOfRangeException ("less than zero", "maxStackSize");

			if (maxStackSize < 131072) // make sure stack is at least 128k big
				return 131072;

			int page_size = Environment.GetPageSize ();

			if ((maxStackSize % page_size) != 0) // round up to a divisible of page size
				maxStackSize = (maxStackSize / (page_size - 1)) * page_size;

			int default_stack_size = (IntPtr.Size / 4) * 1024 * 1024; // from wthreads.c

			if (maxStackSize > default_stack_size)
				return default_stack_size;

			return maxStackSize; 
		}

		public Thread (ThreadStart start, int maxStackSize)
		{
			if (start == null)
				throw new ArgumentNullException ("start");

			threadstart = start;
			Internal.stack_size = CheckStackSize (maxStackSize);;
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

			threadstart = start;
			Internal.stack_size = CheckStackSize (maxStackSize);
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
			if ((ThreadState & ThreadState.Unstarted) == 0)
				throw new ThreadStateException ("Thread was in an invalid state for the operation being executed.");

			if ((ApartmentState)Internal.apartment_state != ApartmentState.Unknown && 
			    (ApartmentState)Internal.apartment_state != state)
				return false;

			Internal.apartment_state = (byte)state;

			return true;
		}
		
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

		// NOTE: This method doesn't show in the class library status page because
		// it cannot be "found" with the StrongNameIdentityPermission for ECMA key.
		// But it's there!
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		[StrongNameIdentityPermission (SecurityAction.LinkDemand, PublicKey="00000000000000000400000000000000")]
		[Obsolete ("see CompressedStack class")]
		public CompressedStack GetCompressedStack ()
		{
#if MOBILE
			throw new NotSupportedException ();
#else			
			// Note: returns null if no CompressedStack has been set.
			// However CompressedStack.GetCompressedStack returns an 
			// (empty?) CompressedStack instance.
			CompressedStack cs = ExecutionContext.SecurityContext.CompressedStack;
			return ((cs == null) || cs.IsEmpty ()) ? null : cs.CreateCopy ();
#endif
		}

		// NOTE: This method doesn't show in the class library status page because
		// it cannot be "found" with the StrongNameIdentityPermission for ECMA key.
		// But it's there!
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		[StrongNameIdentityPermission (SecurityAction.LinkDemand, PublicKey="00000000000000000400000000000000")]
		[Obsolete ("see CompressedStack class")]
		public void SetCompressedStack (CompressedStack stack)
		{
#if MOBILE
			throw new NotSupportedException ();
#else
			ExecutionContext.SecurityContext.CompressedStack = stack;
#endif
		}

#if !MOBILE
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
