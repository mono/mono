//------------------------------------------------------------------------------
// <copyright file="IGac.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System.Web.Configuration;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    /*
    interface for adding item to GAC
    */
    internal interface IGac {
        [DispId(0x0000000D)]
            void GacInstall( [MarshalAs(UnmanagedType.BStr)] string assemblyPath );
    }
}
