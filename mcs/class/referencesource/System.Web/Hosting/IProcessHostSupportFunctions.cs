//------------------------------------------------------------------------------
// <copyright file="ApplicationManager.cs" company="Microsoft">
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


    [ComImport, Guid("35f9c4c1-3800-4d17-99bc-018a62243687"), InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIUnknown)]
    [System.Security.SuppressUnmanagedCodeSecurityAttribute]
    public interface IProcessHostSupportFunctions {

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        void GetApplicationProperties(
            [In, MarshalAs(UnmanagedType.LPWStr)] String appId,
            out String virtualPath,
            out String physicalPath,
            out String siteName,
            out String siteId);

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        void MapPath(
            [In, MarshalAs(UnmanagedType.LPWStr)] String appId,
            [In, MarshalAs(UnmanagedType.LPWStr)] String virtualPath,
            out String physicalPath);

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        IntPtr GetConfigToken(
            [In, MarshalAs(UnmanagedType.LPWStr)] String appId);

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        [return: MarshalAs(UnmanagedType.BStr)]
        String GetAppHostConfigFilename();

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        [return: MarshalAs(UnmanagedType.BStr)]
        String GetRootWebConfigFilename();

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        IntPtr GetNativeConfigurationSystem();
    }
}

