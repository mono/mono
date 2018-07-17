using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Win32.SafeHandles
{
	public sealed partial class SafePipeHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private const int DefaultInvalidHandle = -1;
		
		protected override bool ReleaseHandle()
		{
			throw new PlatformNotSupportedException();
		}
	}
}
