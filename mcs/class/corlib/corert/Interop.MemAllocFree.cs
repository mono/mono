
using System;

internal static partial class Interop
{
    internal static IntPtr MemAlloc(UIntPtr sizeInBytes)
    {
        if (Environment.IsRunningOnWindows)
            return Windows_MemAlloc(sizeInBytes);
        else
            return Unix_MemAlloc(sizeInBytes);
    }

    internal static void MemFree(IntPtr allocatedMemory)
    {
        if (Environment.IsRunningOnWindows)
            Windows_MemFree(allocatedMemory);
        else
            Unix_MemFree(allocatedMemory);
    }
}
