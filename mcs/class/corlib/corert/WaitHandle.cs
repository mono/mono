using Internal.Runtime.Augments;
using Microsoft.Win32.SafeHandles;

namespace System.Threading
{
	public abstract partial class WaitHandle
	{
		private static bool WaitOneCore (IntPtr handle, int millisecondsTimeout)
		{
			throw new NotSupportedException ();
		}

		private static bool WaitAllCore (
			RuntimeThread currentThread,
			SafeWaitHandle[] safeWaitHandles,
			WaitHandle[] waitHandles,
			int millisecondsTimeout)
		{
			throw new NotSupportedException ();
		}

		private static int WaitAnyCore (
			RuntimeThread currentThread,
			SafeWaitHandle[] safeWaitHandles,
			WaitHandle[] waitHandles,
			int millisecondsTimeout)
		{
			throw new NotSupportedException ();
		}

		private static bool SignalAndWaitCore (
			IntPtr handleToSignal,
			IntPtr handleToWaitOne,
			int millisecondsTimeout)
		{
			throw new NotSupportedException ();
		}
	}
}