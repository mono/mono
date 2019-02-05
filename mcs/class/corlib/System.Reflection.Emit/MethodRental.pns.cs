#if FULL_AOT_RUNTIME

//using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	public sealed class MethodRental {
		public const int JitImmediate = 1;
		public const int JitOnDemand = 0;

		MethodRental ()
		{
		}

		public static void SwapMethodBody (Type cls, int methodtoken, IntPtr rgIL, int methodSize, int flags)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}

#endif
