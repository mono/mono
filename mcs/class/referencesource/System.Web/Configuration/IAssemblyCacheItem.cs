//------------------------------------------------------------------------------
// <copyright file="IAssemblyCacheItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Web.Configuration;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9e3aaeb4-d1cd-11d2-bab9-00c04f8eceae")]
        internal interface IAssemblyCacheItem {
        void CreateStream([MarshalAs(UnmanagedType.LPWStr)] String pszName,uint dwFormat, uint dwFlags, uint dwMaxSize, out System.Runtime.InteropServices.ComTypes.IStream ppStream);
        void IsNameEqual(IAssemblyName pName);
        void Commit(uint dwFlags);
        void MarkAssemblyVisible(uint dwFlags);
    }// IAssemblyCacheItem System.Runtime.InteropServices.ComTypes.IStream
}
