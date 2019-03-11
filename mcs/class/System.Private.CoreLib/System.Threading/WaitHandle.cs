using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Threading
{
	partial class WaitHandle
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		unsafe static extern int Wait_internal(IntPtr* handles, int numHandles, bool waitAll, int ms);

		static int WaitOneCore (IntPtr waitHandle, int millisecondsTimeout)
		{
			unsafe {
				return Wait_internal (&waitHandle, 1, false, millisecondsTimeout);
			}
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern int SignalAndWaitCore (IntPtr waitHandleToSignal, IntPtr waitHandleToWaitOn, int millisecondsTimeout);

		internal static int WaitMultipleIgnoringSyncContext (Span<IntPtr> waitHandles, bool waitAll, int millisecondsTimeout)
		{
			unsafe {
				fixed (IntPtr* handles = &MemoryMarshal.GetReference (waitHandles)) {
					return Wait_internal (handles, waitHandles.Length, waitAll, millisecondsTimeout);
				}
			}
		}
	}
}