//------------------------------------------------------------------------------
// <copyright file="IConfigMapPathFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Runtime.InteropServices;  
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    //
    // This interface is public and implemented by 
    // the admin tools in IIS 7.  It must not refer to the VirtualPath
    // type
    //
    public interface IConfigMapPathFactory {
        IConfigMapPath Create(string virtualPath, string physicalPath);
    }
}
