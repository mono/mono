// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;

internal partial class Interop
{
    internal partial class Kernel32
    {
        /// <summary>
        /// WARNING: This method does not implicitly handle long paths. Use DeleteVolumeMountPoint.
        /// </summary>
        [DllImport(Libraries.Kernel32, EntryPoint = "DeleteVolumeMountPointW", SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false)]
        internal static extern bool DeleteVolumeMountPointPrivate(string mountPoint);


        internal static bool DeleteVolumeMountPoint(string mountPoint)
        {
            mountPoint = PathInternal.EnsureExtendedPrefixIfNeeded(mountPoint);
            // The DeleteVolumeMountPointW method does not exist on Windows SDK versions
            // before 16299. This is manifested at runtime in IL2CPP as a DllNotFoundException
            // for kernel32.dll. If this happens, throw a proper exception, as this should not
            // be possible for UWP apps.
#if UNITY_AOT && WIN_PLATFORM
            try
            {
#endif
                return DeleteVolumeMountPointPrivate(mountPoint);
#if UNITY_AOT && WIN_PLATFORM
            }
            catch (System.DllNotFoundException)
            {
                throw new System.UnauthorizedAccessException(); 
            }
#endif
        }
    }
}
