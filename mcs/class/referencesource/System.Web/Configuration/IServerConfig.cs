//------------------------------------------------------------------------------
// <copyright file="IServerConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System.Configuration;
    using System.Collections;
    using System.Globalization;
    using System.Text;
    using System.Web.Util;
    using System.Web.Hosting;
    using System.Web.Caching;
    using Microsoft.Win32;

    // Config functions used by ASP.NET.
    interface IServerConfig {
        // if appHost is null, the site for the current application will be used to map the path -- this is probably what you want
        string     MapPath(IApplicationHost appHost, VirtualPath path);
        string     GetSiteNameFromSiteID(string siteID);
        bool       GetUncUser(IApplicationHost appHost, VirtualPath path, out string username, out string password);
        string[]   GetVirtualSubdirs(VirtualPath path, bool inApp);
        long       GetW3WPMemoryLimitInKB();
    }
}

