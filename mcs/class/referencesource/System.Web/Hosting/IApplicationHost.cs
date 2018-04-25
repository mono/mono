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

    public interface IApplicationHost {

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        string GetVirtualPath();

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        String GetPhysicalPath();

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        IConfigMapPathFactory GetConfigMapPathFactory();

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        IntPtr GetConfigToken();

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        String GetSiteName();

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        String GetSiteID();

        [SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Unrestricted = true)]
        void MessageReceived();

    }
}
