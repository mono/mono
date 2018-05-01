using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace System.IO.MemoryMappedFiles
{
    internal partial class MemoryMappedView
    {
        public static unsafe MemoryMappedView CreateView(
            SafeMemoryMappedFileHandle memMappedFileHandle, MemoryMappedFileAccess access,
            long requestedOffset, long requestedSize)
        {
            throw new PlatformNotSupportedException ();
        }

        public unsafe void Flush(UIntPtr capacity)
        {
            throw new PlatformNotSupportedException ();
        }
    }
}
