// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Text;

namespace System.IO
{
    /// <summary>Contains internal volume helpers that are shared between many projects.</summary>
    internal static partial class DriveInfoInternal
    {
        public static string[] GetLogicalDrives()
        {
            int drives = 0;
            // The GetLogicalDrives method does not exist on Windows SDK versions
            // before 16299. This is manifested at runtime in IL2CPP as a DllNotFoundException
            // for kernel32.dll. If this happens, throw an exception.
#if UNITY_AOT && WIN_PLATFORM
            try
            {
#endif
                drives = Interop.Kernel32.GetLogicalDrives();
#if UNITY_AOT && WIN_PLATFORM
            }
            catch (System.DllNotFoundException)
            {
                throw new InvalidOperationException("GetLogicalDrives is not supported using this version of the Windows SDK. Use SDK versions greater than 16299.");
            }
#endif
            if (drives == 0)
                throw Win32Marshal.GetExceptionForLastWin32Error();

            // GetLogicalDrives returns a bitmask starting from 
            // position 0 "A" indicating whether a drive is present.
            // Loop over each bit, creating a string for each one
            // that is set.

            uint d = (uint)drives;
            int count = 0;
            while (d != 0)
            {
                if (((int)d & 1) != 0) count++;
                d >>= 1;
            }

            string[] result = new string[count];
            char[] root = new char[] { 'A', ':', '\\' };
            d = (uint)drives;
            count = 0;
            while (d != 0)
            {
                if (((int)d & 1) != 0)
                {
                    result[count++] = new string(root);
                }
                d >>= 1;
                root[0]++;
            }
            return result;
        }
    }
}
