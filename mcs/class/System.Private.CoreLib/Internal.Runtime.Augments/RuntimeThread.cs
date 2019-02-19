using System;
using System.Threading;
using System.Runtime.ConstrainedExecution;
using System.Runtime.CompilerServices;

namespace Internal.Runtime.Augments
{
	public partial class RuntimeThread : CriticalFinalizerObject
	{
		// Note: Magic number copied from CoreRT's RuntimeThread.cs. See the original source code for an explanation.
		internal static readonly int OptimalMaxSpinWaitsPerSpinIteration = 64;

		readonly Thread thread;

		RuntimeThread (Thread t)
		{
			thread = t;
		}

		public bool IsBackground {
			get { throw new NotImplementedException (); }
			set { }
		}

		public static RuntimeThread CurrentThread => throw new NotImplementedException ();

		internal static ulong CurrentOSThreadId {
			get {
				throw new NotImplementedException ();
			}
		}

		public extern bool IsThreadPoolThread
		{
			[MethodImpl (MethodImplOptions.InternalCall)]
			get;
		}

		public int ManagedThreadId => AsThread ().ManagedThreadId;

		public string Name {
			get {
				return AsThread ().Name;
			}
			set {
				AsThread ().Name = value;
			}
		}

		public ThreadPriority Priority {
			get {
				throw new NotImplementedException ();
			}
			set {
			}
		}

		public ThreadState ThreadState {
			get {
				throw new NotImplementedException ();
			}
		}

		public extern bool IsAlive {
			[MethodImpl(MethodImplOptions.InternalCall)]
			get;
		}

		public static bool Yield () => Thread.Yield ();

		public void Interrupt ()
		{
			throw new NotImplementedException ();
		}

		private Thread AsThread ()
		{
			throw new NotImplementedException ();
		}

		public static RuntimeThread Create (ThreadStart start) => throw new NotImplementedException ();
		public static RuntimeThread Create (ThreadStart start, int maxStackSize) => throw new NotImplementedException ();
		public static RuntimeThread Create (ParameterizedThreadStart start) => throw new NotImplementedException ();
		public static RuntimeThread Create (ParameterizedThreadStart start, int maxStackSize) => throw new NotImplementedException ();

		public static int GetCurrentProcessorId ()
		{
			// TODO: Implement correctly
			return 1;
		}

		public static bool SpinWait (int iterations)
		{
			Thread.SpinWait (iterations);
			return true;
		}

		public static void Sleep (int millisecondsTimeout) => Thread.Sleep (millisecondsTimeout);

		public void Start () => throw new NotImplementedException ();
		public void Start (object parameter) => throw new NotImplementedException ();

		public void Join () => JoinInternal (Timeout.Infinite);

		public bool Join (int millisecondsTimeout) => JoinInternal (millisecondsTimeout);

		private bool JoinInternal (int millisecondsTimeout)
		{
			throw new NotImplementedException ();
		}
	}
}