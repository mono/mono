
namespace System.Runtime.CompilerServices {

	[FriendAccessAllowed]
	internal static class JitHelpers
	{
		static internal T UnsafeCast<T>(Object o) where T : class
		{
			return (T)o;
		}
	}
}