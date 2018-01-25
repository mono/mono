using System;

namespace Internal.Runtime.Augments
{
	class RuntimeThread
	{
		public static RuntimeThread InitializeThreadPoolThread ()
		{
			return default;
		}

		public void ResetThreadPoolThread ()
		{
		}

		public static RuntimeThread Create(object start, int maxStackSize) => throw new NotImplementedException ();

		public bool IsBackground { get; set; }

		public void Start () => throw new NotImplementedException ();

		public void Start (object state) => throw new NotImplementedException ();

		public static bool Yield () => throw new NotImplementedException ();

		public static bool SpinWait (int iterations) => throw new NotImplementedException ();
	}
}