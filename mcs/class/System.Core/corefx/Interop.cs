using System;

internal static partial class Interop
{
	internal static partial class Sys
	{
		internal static uint GetEUid ()
		{
			throw new PlatformNotSupportedException ();
		}

		internal static int SetEUid (uint euid)
		{
			throw new PlatformNotSupportedException ();
		}

	}
}
