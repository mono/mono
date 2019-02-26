using System.Runtime.CompilerServices;

namespace System.Threading
{
	partial class WaitHandle
	{
		static int WaitOneCore (IntPtr waitHandle, int millisecondsTimeout)
		{
			throw new NotImplementedException ();
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern int SignalAndWaitCore (IntPtr waitHandleToSignal, IntPtr waitHandleToWaitOn, int millisecondsTimeout);

		internal static int WaitMultipleIgnoringSyncContext (Span<IntPtr> waitHandles, bool waitAll, int millisecondsTimeout) => throw new PlatformNotSupportedException ();
	}
}