using System;

partial class Interop
{
	internal static class Kernel32
	{
		// TODO: It's used byu EventSource only and should call CoreFX Interop.Sys.GetPid
		internal static uint GetCurrentProcessId ()
		{
			throw new NotImplementedException ();
		}
	}
}