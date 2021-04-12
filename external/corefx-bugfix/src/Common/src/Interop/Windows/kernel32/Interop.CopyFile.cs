// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Kernel32
    {
#if UNITY_AOT && WIN_PLATFORM
		static bool useUWPFallback = false;
#endif
        internal static int CopyFile(string src, string dst, bool failIfExists)
        {
            int copyFlags = failIfExists ? Interop.Kernel32.FileOperations.COPY_FILE_FAIL_IF_EXISTS : 0;
            int cancel = 0;
            // The CopyFileExW method does not exist on Windows SDK versions
            // before 16299. This is manifested at runtime in IL2CPP as a DllNotFoundException
            // for kernel32.dll. If this happens, fall back to a copy file implementation
            // that does work on UWP with that SDK.
#if UNITY_AOT && WIN_PLATFORM
			if (useUWPFallback)
				return CopyFileUWP(src, dst, failIfExists);

            try
            {
#endif
                if (!Interop.Kernel32.CopyFileEx(src, dst, IntPtr.Zero, IntPtr.Zero, ref cancel, copyFlags))
                {
                    return Marshal.GetLastWin32Error();
                }
#if UNITY_AOT && WIN_PLATFORM
            }
            catch(DllNotFoundException)
            {
				useUWPFallback = true;
                return CopyFileUWP(src, dst, failIfExists);
            }
#endif
            return Interop.Errors.ERROR_SUCCESS;
        }
    }
}
