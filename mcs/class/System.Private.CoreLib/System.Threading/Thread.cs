using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Threading
{
	partial class Thread
	{
		string name;
		InternalThread internalThread;
		internal ExecutionContext _executionContext;

		[ThreadStatic]
		static Thread current_thread;

		Thread ()
		{
		}

		public ExecutionContext ExecutionContext => ExecutionContext.Capture ();

		public static Thread CurrentThread {
			get {
				Thread current = current_thread;
				if (current != null)
					return current;

				// This will set the current_thread tls variable
				return GetCurrentThread ();
			}
		}

		internal static ulong CurrentOSThreadId {
			get {
				throw new NotImplementedException ();
			}
		}

		InternalThread Internal {
			get {
				if (internalThread == null)
					throw new NotImplementedException ();

				return internalThread;
			}
		}

		public bool IsAlive {
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsBackground {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		public bool IsThreadPoolThread {
			get {
				throw new NotImplementedException ();
			}
		}

		public int ManagedThreadId {
			get {
				throw new NotImplementedException ();
			}
		}

		public string Name {
			get => name;
			set {
				lock (this) {
					if (name != null)
						throw new InvalidOperationException (SR.InvalidOperation_WriteOnce);

					name = value;
				}
			}
		}

		internal static int OptimalMaxSpinWaitsPerSpinIteration {
			get {
				throw new NotImplementedException ();
			}
		}

		public ThreadPriority Priority {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		internal SynchronizationContext SynchronizationContext { get; set; }

		public ThreadState ThreadState {
			get {
				throw new NotImplementedException ();
			}
		}

		void Create (ThreadStart start) => SetStartHelper ((Delegate)start, 0); // 0 will setup Thread with default stackSize

		void Create (ThreadStart start, int maxStackSize) => SetStartHelper ((Delegate)start, maxStackSize);

		void Create (ParameterizedThreadStart start) => SetStartHelper ((Delegate)start, 0);

		void Create (ParameterizedThreadStart start, int maxStackSize) => SetStartHelper ((Delegate)start, maxStackSize);

		public ApartmentState GetApartmentState () => ApartmentState.MTA;

		public void DisableComObjectEagerCleanup ()
		{
			// no-op
		}

		public static int GetCurrentProcessorId ()
		{
			throw new NotImplementedException ();
		}

		public void Interrupt ()
		{
			throw new NotImplementedException ();
		}

		public bool Join (int millisecondsTimeout)
		{
			throw new NotImplementedException ();
		}

		public void ResetThreadPoolThread ()
		{
		}

		void SetCultureOnUnstartedThreadNoCheck (CultureInfo value, bool uiCulture)
		{
			throw new NotImplementedException ();
		}

		void SetStartHelper (Delegate start, int maxStackSize)
		{
			throw new NotImplementedException ();
		}

		public static void SpinWait (int iterations)
		{
			if (iterations < 0)
				return;

			while (iterations-- > 0)
				SpinWait_nop ();
		}

		public static void Sleep (int millisecondsTimeout)
		{
			throw new NotImplementedException ();
		}

		public void Start ()
		{
		}

		public void Start (object parameter)
		{
		}

		public static bool Yield ()
		{
			throw new NotImplementedException ();
		}

		public bool TrySetApartmentStateUnchecked (ApartmentState state) => false;

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static Thread GetCurrentThread ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static void SpinWait_nop ();
	}

	[StructLayout (LayoutKind.Sequential)]
	sealed class InternalThread
	{
	}
}