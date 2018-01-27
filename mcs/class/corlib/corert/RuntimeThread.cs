using System;
using System.Threading;

namespace Internal.Runtime.Augments
{
	class RuntimeThread
	{
		readonly Thread thread;

		RuntimeThread (Thread t) { thread = t; }
		
		public void ResetThreadPoolThread () {}
		
		public static RuntimeThread InitializeThreadPoolThread () => default;

		public static RuntimeThread Create (ParameterizedThreadStart start, int maxStackSize) 
			=> new RuntimeThread (new Thread (start, maxStackSize));

		public bool IsBackground
		{
			get => thread.IsBackground;
			set => thread.IsBackground = value;
		}

		public void Start () => thread.Start ();

		public void Start (object state) => thread.Start (state);

		public static bool Yield () => Thread.Yield ();

		public static bool SpinWait (int iterations)
		{
			Thread.SpinWait (iterations);
			return true;
		}
	}
}