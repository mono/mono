namespace System.Threading
{
	partial class WaitHandle
	{
		internal static int WaitMultipleIgnoringSyncContext (IntPtr[] handles, bool waitAll, int millisecondsTimeout) => throw new PlatformNotSupportedException ();		
	}
}