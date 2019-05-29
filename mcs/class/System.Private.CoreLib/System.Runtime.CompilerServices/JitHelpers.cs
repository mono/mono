namespace System.Runtime.CompilerServices
{
	static class JitHelpers
	{
		[Intrinsic]
		public static bool EnumEquals<T> (T x, T y) where T : struct, Enum => throw new NotImplementedException ();

		[Intrinsic]
		public static int EnumCompareTo<T> (T x, T y) where T : struct, Enum  => throw new NotImplementedException ();
	}
}