
namespace System.Runtime.CompilerServices {

	[FriendAccessAllowed]
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
			throw new NotImplementedException ();
		}
	}
}