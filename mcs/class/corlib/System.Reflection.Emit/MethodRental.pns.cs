#if !MONO_FEATURE_SRE
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	public partial class MethodRental : _MethodRental {

		private MethodRental () {}

		public const int JitImmediate = 1;
		public const int JitOnDemand = 0;

		public static void SwapMethodBody (Type cls, int methodtoken, IntPtr rgIL, int methodSize, int flags) => throw new PlatformNotSupportedException ();

		void _MethodRental.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId) => throw new PlatformNotSupportedException ();

		void _MethodRental.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo) => throw new PlatformNotSupportedException ();

		void _MethodRental.GetTypeInfoCount (out uint pcTInfo) => throw new PlatformNotSupportedException ();

		void _MethodRental.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr) => throw new PlatformNotSupportedException ();
	}
}

#endif