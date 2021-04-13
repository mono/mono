// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Kernel32
    {
        // Rename this method so that it does not conflict with the method of the
        // same name for Windows Desktop. In the unityjit profile, this source
        // file is not included in the build.
#if UNITY_AOT && WIN_PLATFORM
        internal static int CopyFileUWP(string src, string dst, bool failIfExists)
#else
        internal static int CopyFile(string src, string dst, bool failIfExists)
#endif
        {
            uint copyFlags = failIfExists ? (uint)Interop.Kernel32.FileOperations.COPY_FILE_FAIL_IF_EXISTS : 0;
            Interop.Kernel32.COPYFILE2_EXTENDED_PARAMETERS parameters = new Interop.Kernel32.COPYFILE2_EXTENDED_PARAMETERS()
            {
                dwSize = (uint)Marshal.SizeOf<Interop.Kernel32.COPYFILE2_EXTENDED_PARAMETERS>(),
                dwCopyFlags = copyFlags
            };

            int hr = Interop.Kernel32.CopyFile2(src, dst, ref parameters);

            return Win32Marshal.TryMakeWin32ErrorCodeFromHR(hr);
        }
    }
}
