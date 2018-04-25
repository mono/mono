//------------------------------------------------------------------------------
// <copyright file="IProcessPingCallback.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Runtime.InteropServices;  
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;
    

    [ComImport, Guid("f11dc4c9-ddd1-4566-ad53-cf6f3a28fefe"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IProcessPingCallback {

        void Respond();
    }
}
