//------------------------------------------------------------------------------
// <copyright file="ApplicationHost.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {

    using System;
    using System.IO;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;
    using System.Security.Permissions;


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class ApplicationHost {

        private ApplicationHost() {
        }

        /*
         * Creates new app domain for hosting of ASP.NET apps with a
         * user defined 'host' object in it.  The host is needed to make
         * cross-domain calls to process requests in the host's app domain
         */

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static Object CreateApplicationHost(Type hostType, String virtualDir, String physicalDir) {
#if !FEATURE_PAL // FEATURE_PAL does not require PlatformID.Win32NT
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new PlatformNotSupportedException(SR.GetString(SR.RequiresNT));
#else // !FEATURE_PAL
            // FEATURE_PAL
#endif // !FEATURE_PAL

            if (!StringUtil.StringEndsWith(physicalDir, Path.DirectorySeparatorChar))
                physicalDir = physicalDir + Path.DirectorySeparatorChar;

            ApplicationManager appManager = ApplicationManager.GetApplicationManager();

            String appId = (String.Concat(virtualDir, physicalDir).GetHashCode()).ToString("x");

            ObjectHandle h = appManager.CreateInstanceInNewWorkerAppDomain(
                                hostType, appId, VirtualPath.CreateNonRelative(virtualDir), physicalDir);

            return h.Unwrap();
        }
    }
}
