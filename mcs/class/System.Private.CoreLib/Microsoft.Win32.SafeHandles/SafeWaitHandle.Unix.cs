using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Microsoft.Win32.SafeHandles
{
	partial class SafeWaitHandle
	{
		protected override bool ReleaseHandle ()
		{
			CloseEventInternal (handle);
			return true;
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void CloseEventInternal (IntPtr handle);
	}
}