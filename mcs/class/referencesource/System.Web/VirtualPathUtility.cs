//------------------------------------------------------------------------------
// <copyright file="VirtualPathUtility.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * VirtualPathUtility class
 *
 * Copyright (c) 2004 Microsoft Corporation
 */

namespace System.Web {

using System.Web.Util;
using System.Security.Permissions;

/*
 * Code to perform virtual path operations
 */
public static class VirtualPathUtility {

    /* Discover virtual path type */

    public static bool IsAbsolute(string virtualPath) {
        VirtualPath virtualPathObject = VirtualPath.Create(virtualPath);
        return !virtualPathObject.IsRelative && virtualPathObject.VirtualPathStringIfAvailable != null;
    }

    public static bool IsAppRelative(string virtualPath) {
        VirtualPath virtualPathObject = VirtualPath.Create(virtualPath);
        return virtualPathObject.VirtualPathStringIfAvailable == null;
    }

    /* Convert between virtual path types */
    public static string ToAppRelative(string virtualPath) {
        VirtualPath virtualPathObject = VirtualPath.CreateNonRelative(virtualPath);
        return virtualPathObject.AppRelativeVirtualPathString;
    }

    public static string ToAppRelative(string virtualPath, string applicationPath) {
        VirtualPath virtualPathObject = VirtualPath.CreateNonRelative(virtualPath);

        // If it was already app relative, just return it
        if (virtualPathObject.AppRelativeVirtualPathStringIfAvailable != null)
            return virtualPathObject.AppRelativeVirtualPathStringIfAvailable;

        VirtualPath appVirtualPath = VirtualPath.CreateAbsoluteTrailingSlash(applicationPath);

        return UrlPath.MakeVirtualPathAppRelative(virtualPathObject.VirtualPathString,
            appVirtualPath.VirtualPathString, true /*nullIfNotInApp*/);
    }

    public static string ToAbsolute(string virtualPath) {
        VirtualPath virtualPathObject = VirtualPath.CreateNonRelative(virtualPath);
        return virtualPathObject.VirtualPathString;
    }

    public static string ToAbsolute(string virtualPath, string applicationPath) {
        VirtualPath virtualPathObject = VirtualPath.CreateNonRelative(virtualPath);

        // If it was already absolute, just return it
        if (virtualPathObject.VirtualPathStringIfAvailable != null)
            return virtualPathObject.VirtualPathStringIfAvailable;

        VirtualPath appVirtualPath = VirtualPath.CreateAbsoluteTrailingSlash(applicationPath);

        return UrlPath.MakeVirtualPathAppAbsolute(virtualPathObject.AppRelativeVirtualPathString,
            appVirtualPath.VirtualPathString);
    }


    /* Get pieces of virtual path */
    public static string GetFileName(string virtualPath) {
        VirtualPath virtualPathObject = VirtualPath.CreateNonRelative(virtualPath);
        return virtualPathObject.FileName;
    }

    public static string GetDirectory(string virtualPath) {
        VirtualPath virtualPathObject = VirtualPath.CreateNonRelative(virtualPath);

        virtualPathObject = virtualPathObject.Parent;
        if (virtualPathObject == null)
            return null;

        return virtualPathObject.VirtualPathStringWhicheverAvailable;
    }

    public static string GetExtension(string virtualPath) {
        VirtualPath virtualPathObject = VirtualPath.Create(virtualPath);
        return virtualPathObject.Extension;
    }

    /* Canonicalize virtual paths */
    public static string AppendTrailingSlash(string virtualPath) {
        return UrlPath.AppendSlashToPathIfNeeded(virtualPath);
    }

    public static string RemoveTrailingSlash(string virtualPath) {
        return UrlPath.RemoveSlashFromPathIfNeeded(virtualPath);
    }

// Removing Reduce per DevDiv 43118
#if OLD
    public static string Reduce(string virtualPath) {
        VirtualPath virtualPathObject = VirtualPath.CreateNonRelative(virtualPath);
        return virtualPathObject.VirtualPathString;
    }
#endif

    /* Work with multiple virtual paths */
    public static string Combine(string basePath, string relativePath) {
        VirtualPath virtualPath = VirtualPath.Combine(VirtualPath.CreateNonRelative(basePath),
            VirtualPath.Create(relativePath));
        return virtualPath.VirtualPathStringWhicheverAvailable;
    }

    public static string MakeRelative(string fromPath, string toPath) {
        return UrlPath.MakeRelative(fromPath, toPath);
    }
}


}
