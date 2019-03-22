
namespace System.Runtime.CompilerServices {

	internal static class JitHelpers
	{
		static internal T UnsafeCast<T>(Object o) where T : class
		{
			return Array.UnsafeMov<object, T> (o);
		}

		static internal int UnsafeEnumCast<T>(T val) where T : struct
		{
			return Array.UnsafeMov<T, int> (val);
		}

		static internal long UnsafeEnumCastLong<T>(T val) where T : struct
		{
			return Array.UnsafeMov<T, long> (val);
		}

#if NETCORE
		[Intrinsic]
		internal static bool EnumEquals<T>(T x, T y) where T : struct, Enum => throw new NotImplementedException ();

		[Intrinsic]
		internal static int EnumCompareTo<T>(T x, T y) where T : struct, Enum  => throw new NotImplementedException ();
#endif
	}
}