using System.Runtime.CompilerServices;

namespace System
{
	partial class MathF
	{
		// [Intrinsic] TODO: implement intrinsic (FMA)
		[MethodImpl (MethodImplOptions.InternalCall)]
		public static extern float FusedMultiplyAdd (float x, float y, float z);

		[MethodImpl (MethodImplOptions.InternalCall)]
		public static extern int ILogB (float x);

		[MethodImpl (MethodImplOptions.InternalCall)]
		public static extern float Log2 (float x);

		[MethodImpl (MethodImplOptions.InternalCall)]
		public static extern float ScaleB (float x, int n);
	}
}