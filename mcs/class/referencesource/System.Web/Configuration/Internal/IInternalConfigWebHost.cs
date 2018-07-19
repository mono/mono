//------------------------------------------------------------------------------
// <copyright file="IInternalConfigWebHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration.Internal {
    using System;

    [System.Runtime.InteropServices.ComVisible(false)]
    public interface IInternalConfigWebHost {
        void GetSiteIDAndVPathFromConfigPath(string configPath, out string siteID, out string vpath);
        string GetConfigPathFromSiteIDAndVPath(string siteID, string vpath);
    }    
}
