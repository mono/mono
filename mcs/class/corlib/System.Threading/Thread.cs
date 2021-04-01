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

using System.Runtime.Serialization;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;

#if !NETCORE
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Remoting.Contexts;
using System.Security.Principal;
#endif

namespace System.Threading {
	[StructLayout (LayoutKind.Sequential)]
	sealed class InternalThread : CriticalFinalizerObject {
#pragma warning disable 169, 414, 649
		#region Sync with metadata/object-internals.h
		int lock_thread_id;
		// stores a thread handle
		IntPtr handle;
		IntPtr native_handle; // used only on Win32
		/* accessed only from unmanaged code */
		private IntPtr name_chars;
		private int name_free; // bool
		private int name_length;
		private ThreadState state;
		private object abort_exc;
		private int abort_state_handle;
		/* thread_id is only accessed from unmanaged code */
		internal Int64 thread_id;
		private IntPtr debugger_thread; // FIXME switch to bool as soon as CI testing with corlib version bump works
		private UIntPtr static_data; /* GC-tracked */
		private IntPtr runtime_thread_info;
		/* current System.Runtime.Remoting.Contexts.Context instance
		   keep as an object to avoid triggering its class constructor when not needed */
		private object current_appcontext;
		private object root_domain_thread;
		internal byte[] _serialized_principal;
		internal int _serialized_principal_version;
		private IntPtr appdomain_refs;
		private int interruption_requested;
		private IntPtr longlived;
		internal bool threadpool_thread;
		private bool thread_interrupt_requested;
		/* These are used from managed code */
		internal int stack_size;
		internal byte apartment_state;
		internal volatile int critical_region_level;
		internal int managed_id;
		private int small_id;
		private IntPtr manage_callback;
		private IntPtr flags;
		private IntPtr thread_pinning_ref;
		private IntPtr abort_protected_block_count;
		private int priority = (int) ThreadPriority.Normal;
		private IntPtr owned_mutex;
		private IntPtr suspended_event;
		private int self_suspended;
		private IntPtr thread_state;

		// Unused fields to have same size as netcore.
		private IntPtr netcore0;
		private IntPtr netcore1;
		private IntPtr netcore2;

		/* This is used only to check that we are in sync between the representation
		 * of MonoInternalThread in native and InternalThread in managed
		 *
		 * DO NOT RENAME! DO NOT ADD FIELDS AFTER! */
		private IntPtr last;
		#endregion
#pragma warning restore 169, 414, 649

		// Closes the system thread handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void Thread_free_internal();

		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.Success)]
		~InternalThread() {
			Thread_free_internal();
		}
	}

	[StructLayout (LayoutKind.Sequential)]
#if !NETCORE
	public
#endif
	sealed partial class Thread {
#pragma warning disable 414		
		#region Sync with metadata/object-internals.h
		private InternalThread internal_thread;
		object m_ThreadStartArg;
		object pending_exception;
		#endregion
#pragma warning restore 414

		// the name of current_thread is
		// important because they are used by the runtime.

		[ThreadStatic]
		static Thread current_thread;

		// can be both a ThreadStart and a ParameterizedThreadStart
		private MulticastDelegate m_Delegate;

		private ExecutionContext m_ExecutionContext;    // this call context follows the logical thread

		private bool m_ExecutionContextBelongsToOuterScope;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void ConstructInternalThread ();

		private InternalThread Internal {
			get {
				if (internal_thread == null)
					ConstructInternalThread ();
				return internal_thread;
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

#if !NETCORE
#if !DISABLE_REMOTING
		public static Context CurrentContext {
			get {
				return(AppDomain.InternalGetContext ());
			}
		}
#endif

#if !DISABLE_SECURITY
		IPrincipal principal;
		int principal_version;

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

				var logicalPrincipal = th.GetExecutionContextReader().LogicalCallContext.Principal;
				if (logicalPrincipal != null)
					return logicalPrincipal;

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
			set {
				Thread th = CurrentThread;

				th.GetMutableExecutionContext().LogicalCallContext.Principal = value;

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
#else
		public static IPrincipal CurrentPrincipal {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}
#endif

		public static AppDomain GetDomain() {
			return AppDomain.CurrentDomain;
		}
#endif

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void GetCurrentThread_icall (ref Thread thread);

		private static Thread GetCurrentThread () {
			Thread thread = null;
			GetCurrentThread_icall (ref thread);
			return thread;
		}

		public static Thread CurrentThread {
			[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
			get {
				Thread current = current_thread;
				if (current != null)
					return current;
				// This will set the current_thread tls variable
				return GetCurrentThread ();
			}
		}

		internal static int CurrentThreadId {
			get {
				return (int)(CurrentThread.internal_thread.thread_id);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static int GetDomainID();

		// Returns the system thread handle
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern bool Thread_internal (MulticastDelegate start);

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
				ValidateThreadState ();
				return (ApartmentState)Internal.apartment_state;
			}

			set {
				ValidateThreadState ();
				TrySetApartmentState (value);
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
				var state = ValidateThreadState ();
				return (state & ThreadState.Background) != 0;
			}
			
			set {
				ValidateThreadState ();
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
		private static unsafe extern void SetName_icall (InternalThread thread, char *name, int nameLength);

		private static unsafe void SetName_internal (InternalThread thread, String name)
		{
			fixed (char* fixed_name = name)
				SetName_icall (thread, fixed_name, name?.Length ?? 0);
		}

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

		public ThreadState ThreadState {
			get {
				return GetState (Internal);
			}
		}

#if MONO_FEATURE_THREAD_ABORT
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern static void Abort_internal (InternalThread thread, object stateInfo);

		public void Abort () 
		{
			Abort_internal (Internal, null);
		}

		public void Abort (object stateInfo) 
		{
			Abort_internal (Internal, stateInfo);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern object GetAbortExceptionState ();

		internal object AbortReason {
			get {
				return GetAbortExceptionState ();
			}
		}

		void ClearAbortReason ()
		{
		}
#else
		[Obsolete ("Thread.Abort is not supported on the current platform.", true)]
		public void Abort ()
		{
			throw new PlatformNotSupportedException ("Thread.Abort is not supported on the current platform.");
		}

		[Obsolete ("Thread.Abort is not supported on the current platform.", true)]
		public void Abort (object stateInfo)
		{
			throw new PlatformNotSupportedException ("Thread.Abort is not supported on the current platform.");
		}

		[Obsolete ("Thread.ResetAbort is not supported on the current platform.", true)]
		public static void ResetAbort ()
		{
			throw new PlatformNotSupportedException ("Thread.ResetAbort is not supported on the current platform.");
		}

		internal object AbortReason {
			get {
				throw new PlatformNotSupportedException ("Thread.ResetAbort is not supported on the current platform.");
			}
		}
#endif // MONO_FEATURE_THREAD_ABORT

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

		void StartInternal (object principal, ref StackCrawlMark stackMark)
		{
#if FEATURE_ROLE_BASED_SECURITY
			Internal._serialized_principal = CurrentThread.Internal._serialized_principal;
#endif

			// Thread_internal creates and starts the new thread, 
			if (!Thread_internal(m_Delegate))
				throw new SystemException ("Thread creation failed.");

			m_ThreadStartArg = null;
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
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static int SystemMaxStackStize ();

		static int GetProcessDefaultStackSize (int maxStackSize)
		{
			if (maxStackSize == 0)
				return 0;

			if (maxStackSize < 131072) // make sure stack is at least 128k big
				return 131072;

			int page_size = Environment.GetPageSize ();

			if ((maxStackSize % page_size) != 0) // round up to a divisible of page size
				maxStackSize = (maxStackSize / (page_size - 1)) * page_size;

			/* Respect the max stack size imposed by the system*/
			return Math.Min (maxStackSize, SystemMaxStackStize ());
		}

		void SetStart (MulticastDelegate start, int maxStackSize)
		{
			m_Delegate = start;
			Internal.stack_size = maxStackSize;
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
			ValidateThreadState ();
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

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void GetStackTraces (out Thread[] threads, out object[] stack_frames);

		// This is a mono extension to gather the stack traces for all running threads
		internal static Dictionary<Thread, StackTrace> Mono_GetStackTraces () {
			Thread[] threads;
			object[] stack_frames;

			GetStackTraces (out threads, out stack_frames);

			var res = new Dictionary<Thread, StackTrace> ();
			for (int i = 0; i < threads.Length; ++i)
				res [threads [i]] = new StackTrace ((StackFrame[])stack_frames [i]);
			return res;
		}

#if !MONO_FEATURE_THREAD_SUSPEND_RESUME
		[Obsolete ("Thread.Suspend is not supported on the current platform.", true)]
		public void Suspend ()
		{
			throw new PlatformNotSupportedException ("Thread.Suspend is not supported on the current platform.");
		}

		[Obsolete ("Thread.Resume is not supported on the current platform.", true)]
		public void Resume ()
		{
			throw new PlatformNotSupportedException ("Thread.Resume is not supported on the current platform.");
		}
#endif

		public void DisableComObjectEagerCleanup ()
		{
			throw new PlatformNotSupportedException ();
		}

		ThreadState ValidateThreadState ()
		{
			var state = GetState (Internal);
			if ((state & ThreadState.Stopped) != 0)
				throw new ThreadStateException ("Thread is dead; state can not be accessed.");
			return state;
		}

		public static int GetCurrentProcessorId() => global::Internal.Runtime.Augments.RuntimeThread.GetCurrentProcessorId();
	}
}
