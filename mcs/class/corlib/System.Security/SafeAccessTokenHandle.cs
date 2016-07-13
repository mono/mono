using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32.SafeHandles
{
	public sealed class SafeAccessTokenHandle : SafeHandle
	{
		public override bool IsInvalid {
			get {
				return handle == IntPtr.Zero;
			}
		}

		public SafeAccessTokenHandle ()
			: base (IntPtr.Zero, true)
		{
			
		}

		protected override bool ReleaseHandle()
		{
			return true;
		}
	}
}
