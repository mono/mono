//------------------------------------------------------------------------------
// <copyright file="IConfigMapPath.cs" company="Microsoft">
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
    // Note: this interface is public in IIS 7
    // and is implemented by the IIS admin tools.  It cannot
    // therefore refer to VirtualPath, which is internal
    //
    public interface IConfigMapPath {
        string  GetMachineConfigFilename();

        string  GetRootWebConfigFilename();

        void    GetPathConfigFilename(
                    string siteID, 
                    string path,
                    out string directory,
                    out string baseName);

        // The default ID should not be localizable, and must be unique
        void GetDefaultSiteNameAndID(out string siteName, out string siteID);

        // The siteID must be unique - no two sites share the same id
        // Many sites may share the same site name
        // Match the siteID first, then the name if no siteID match
        void ResolveSiteArgument(string siteArgument, out string siteName, out string siteID);

        string  MapPath(string siteID, string path);

        string GetAppPathForPath(string siteID, string path);
    }

    // IConfigMapPath variant which uses VirtualPath
    // objects to avoid extra creation costs
    internal interface IConfigMapPath2
    {
        void    GetPathConfigFilename(
                    string siteID, 
                    VirtualPath path,
                    out string directory,
                    out string baseName);

        string  MapPath(string siteID, VirtualPath path);
        VirtualPath GetAppPathForPath(string siteID, VirtualPath path);        
    }
}


