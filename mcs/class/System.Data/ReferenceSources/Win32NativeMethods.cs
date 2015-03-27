using System;

class Win32NativeMethods
{
	public static bool IsTokenRestrictedWrapper (IntPtr token)
	{
		throw new NotSupportedException ("It is native method used by Microsoft System.Data implementation that Mono or non-Windows platform does not support.");
	}
}

