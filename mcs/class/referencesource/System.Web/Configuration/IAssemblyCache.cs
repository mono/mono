//------------------------------------------------------------------------------
// <copyright file="IAssemblyCache.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Web.Configuration;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("e707dcde-d1cd-11d2-bab9-00c04f8eceae")]
        internal interface IAssemblyCache {
        [PreserveSig()]
            int UninstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] String pszAssemblyName, IntPtr pvReserved, out uint pulDisposition);
        [PreserveSig()]
            int QueryAssemblyInfo(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] String pszAssemblyName, IntPtr pAsmInfo);
        [PreserveSig()]
            int CreateAssemblyCacheItem(uint dwFlags, IntPtr pvReserved, out IAssemblyCacheItem ppAsmItem, [MarshalAs(UnmanagedType.LPWStr)] String pszAssemblyName);
        [PreserveSig()]
            int CreateAssemblyScavenger(out Object ppAsmScavenger);
        [PreserveSig()]
            int InstallAssembly(uint dwFlags, [MarshalAs(UnmanagedType.LPWStr)] String pszManifestFilePath, IntPtr pvReserved);
    }// IAssemblyCache
}
