using System.Threading;

namespace Microsoft.Win32.SafeHandles
{
	partial class SafeWaitHandle
	{
		protected override bool ReleaseHandle ()
		{
			NativeEventCalls.CloseEvent_internal (handle);
			return true;
		}
	}
}