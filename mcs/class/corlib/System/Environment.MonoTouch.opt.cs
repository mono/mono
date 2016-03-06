#if MONOTOUCH

// this file is a shim to enable compiling monotouch profiles without mono-extensions
namespace System
{
	public static partial class Environment
	{
		public static string GetFolderPath(SpecialFolder folder, SpecialFolderOption option)
		{
			throw new NotSupportedException ();
		}

		internal static string UnixGetFolderPath (SpecialFolder folder, SpecialFolderOption option)
		{
			throw new NotSupportedException ();
		}
	}
}

#endif
