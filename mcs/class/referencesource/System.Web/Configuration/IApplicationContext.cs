//------------------------------------------------------------------------------
// <copyright file="IApplicationContext.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Web.Configuration;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComImport,InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7c23ff90-33af-11d3-95da-00a024a85b51")]
        internal interface IApplicationContext {
        void SetContextNameObject(IAssemblyName pName);
        void GetContextNameObject(out IAssemblyName ppName);
        void Set([MarshalAs(UnmanagedType.LPWStr)] String szName, int pvValue, uint cbValue, uint dwFlags);
        void Get([MarshalAs(UnmanagedType.LPWStr)] String szName, out int pvValue, ref uint pcbValue, uint dwFlags);
        void GetDynamicDirectory(out int wzDynamicDir, ref uint pdwSize);
    }// IApplicationContext
}
