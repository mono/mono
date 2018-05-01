using System;

namespace Microsoft.Win32.SafeHandles
{
    public sealed partial class SafeMemoryMappedViewHandle
    {
        internal SafeMemoryMappedViewHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            throw new PlatformNotSupportedException ();
        }

        protected override bool ReleaseHandle()
        {
            throw new PlatformNotSupportedException ();
        }
    }
}
