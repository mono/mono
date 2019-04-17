using System.Runtime.CompilerServices;

namespace System
{
	partial class Math
	{
		// [Intrinsic] TODO: implement FMA intrinsic
		[MethodImpl (MethodImplOptions.InternalCall)]
		public static extern double FusedMultiplyAdd (double x, double y, double z);

		[MethodImpl (MethodImplOptions.InternalCall)]
		public static extern int ILogB (double x);

		[MethodImpl (MethodImplOptions.InternalCall)]
		public static extern double Log2 (double x);

		[MethodImpl (MethodImplOptions.InternalCall)]
		public static extern double ScaleB (double x, int n);
	}
}