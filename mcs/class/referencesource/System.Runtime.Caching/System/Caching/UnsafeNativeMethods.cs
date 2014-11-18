// <copyright file="UnsafeNativeMethods.cs" company="Microsoft">
//   Copyright (c) 2009 Microsoft Corporation.  All rights reserved.
// </copyright>
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace System.Runtime.Caching {
    [SuppressUnmanagedCodeSecurity]
    [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Grandfathered suppression from original caching code checkin")]
    [SecurityCritical]
    internal static class UnsafeNativeMethods {
        private const string KERNEL32 = "KERNEL32.DLL";
        private const string ADVAPI32 = "ADVAPI32.DLL";

        /*
         * KERNEL32.DLL
         */

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Grandfathered suppression from original caching code checkin")]
        [DllImport(KERNEL32, CharSet=CharSet.Unicode)]
        internal extern static int GetModuleFileName(IntPtr module, StringBuilder filename, int size);

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Grandfathered suppression from original caching code checkin")]
        [DllImport(KERNEL32, CharSet=CharSet.Unicode)]
        internal extern static int GlobalMemoryStatusEx(ref MEMORYSTATUSEX memoryStatusEx);

        /*
         * ADVAPI32.DLL
         */

        [SuppressMessage("Microsoft.Security", "CA2118:ReviewSuppressUnmanagedCodeSecurityUsage", Justification = "Grandfathered suppression from original caching code checkin")]
        [DllImport(ADVAPI32), ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static extern int RegCloseKey(IntPtr hKey);
    }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct MEMORYSTATUSEX {
        internal int dwLength;
        internal int dwMemoryLoad;
        internal long ullTotalPhys;
        internal long ullAvailPhys;
        internal long ullTotalPageFile;
        internal long ullAvailPageFile;
        internal long ullTotalVirtual;
        internal long ullAvailVirtual;
        internal long ullAvailExtendedVirtual;
        internal  void Init() {
            dwLength = Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }
}
