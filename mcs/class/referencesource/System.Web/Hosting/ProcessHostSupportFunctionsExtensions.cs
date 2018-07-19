//------------------------------------------------------------------------------
// <copyright file="ProcessHostSupportFunctionsExtensions.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;
    using System.Text;
    using System.Web.Util;

    internal static class ProcessHostSupportFunctionsExtensions {

        // Use this instead of MapPath, as it encapsulates the correct way to invoke
        // the MapPath method. Didn't bother much with efficiency since it's called
        // so few times.
        //
        // Example inputs: MapPathInternal(appId, "/appVPath", "/bin/foo.dll");
        public static string MapPathInternal(this IProcessHostSupportFunctions supportFunctions, string appId, string appVirtualPath, string relativePath) {
            Debug.Assert(supportFunctions != null);
            Debug.Assert(!String.IsNullOrWhiteSpace(appId));
            Debug.Assert(!String.IsNullOrWhiteSpace(appVirtualPath) && appVirtualPath[0] == '/');
            Debug.Assert(relativePath != null);

            StringBuilder fullVirtualPath = new StringBuilder(appVirtualPath.Length + relativePath.Length + 2 /* for slashes */);

            // Ensure the app virtual path is always preceded by a slash
            if (appVirtualPath[0] != '/') {
                fullVirtualPath.Append('/');
            }
            fullVirtualPath.Append(appVirtualPath);

            // Ensure the app virtual path always ends with a slash
            if (fullVirtualPath[fullVirtualPath.Length - 1] != '/') {
                fullVirtualPath.Append('/');
            }

            // Append the relative path, removing the preceding slash if necessary
            if (relativePath.Length > 0) {
                if (relativePath[0] == '/') {
                    fullVirtualPath.Append(relativePath, 1, relativePath.Length - 1);
                }
                else {
                    fullVirtualPath.Append(relativePath);
                }
            }

            string physicalPath;
            supportFunctions.MapPath(appId, fullVirtualPath.ToString(), out physicalPath);
            return physicalPath;
        }

    }
}
