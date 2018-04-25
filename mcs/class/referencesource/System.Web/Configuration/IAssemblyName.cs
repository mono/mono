//------------------------------------------------------------------------------
// <copyright file="IAssemblyName.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Web.Configuration;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("CD193BC0-B4BC-11d2-9833-00C04FC31D2E")]
        internal interface IAssemblyName {
        [PreserveSig()]
            int SetProperty(uint PropertyId, IntPtr pvProperty, uint cbProperty);
        [PreserveSig()]
            int GetProperty(uint PropertyId, IntPtr pvProperty, ref uint pcbProperty);
        [PreserveSig()]
            int Finalize();
        [PreserveSig()]
            int GetDisplayName(IntPtr szDisplayName, ref uint pccDisplayName, uint dwDisplayFlags);
        [PreserveSig()]
            int BindToObject(Object /*REFIID*/ refIID, 
                             Object /*IAssemblyBindSink*/ pAsmBindSink, 
                             IApplicationContext pApplicationContext,
                             [MarshalAs(UnmanagedType.LPWStr)] String szCodeBase,
                             Int64 llFlags,
                             int pvReserved,
                             uint cbReserved,
                             out int ppv);
        [PreserveSig()]
            int GetName(out uint lpcwBuffer, out int pwzName);
        [PreserveSig()]
            int GetVersion(out uint pdwVersionHi, out uint pdwVersionLow);
        [PreserveSig()]
            int IsEqual(IAssemblyName pName, uint dwCmpFlags);
        [PreserveSig()]
            int Clone(out IAssemblyName pName);
    }// IAssemblyName
}
