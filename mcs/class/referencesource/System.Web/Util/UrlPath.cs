//------------------------------------------------------------------------------
// <copyright file="UrlPath.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * UrlPath class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Util {
using System.Text;
using System.Runtime.Serialization.Formatters;
using System.Runtime.InteropServices;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Web.Hosting;

/*
 * Code to perform Url path combining
 */
internal static class UrlPath {

    internal const char appRelativeCharacter = '~';
    internal const string appRelativeCharacterString = "~/";

    private static char[] s_slashChars = new char[] { '\\', '/' };

    internal static bool IsRooted(String basepath) {
        return (String.IsNullOrEmpty(basepath) || basepath[0] == '/' || basepath[0] == '\\');
    }

    // Checks if virtual path contains a protocol, which is referred to as a scheme in the
    // URI spec.
    private static bool HasScheme(string virtualPath) {
        // URIs have the format <scheme>:<scheme-specific-path>, e.g. mailto:user@ms.com,
        // http://server/, nettcp://server/, etc.  The <scheme> cannot contain slashes.
        // The virtualPath passed to this method may be absolute or relative. Although
        // ':' is only allowed in the <scheme-specific-path> if it is encoded, the 
        // virtual path that we're receiving here may be decoded, so it is impossible
        // for us to determine if virtualPath has a scheme.  We will be conservative
        // and err on the side of assuming it has a scheme when we cannot tell for certain.
        // To do this, we first check for ':'.  If not found, then it doesn't have a scheme.
        // If ':' is found, then as long as we find a '/' before the ':', it cannot be
        // a scheme because schemes don't contain '/'.  Otherwise, we will assume it has a 
        // scheme.
        int indexOfColon = virtualPath.IndexOf(':');
        if (indexOfColon == -1)
            return false;
        int indexOfSlash = virtualPath.IndexOf('/');
        return (indexOfSlash == -1 || indexOfColon < indexOfSlash);
    }

    // Returns whether the virtual path is relative.  Note that this returns true for
    // app relative paths (e.g. "~/sub/foo.aspx")
    internal static bool IsRelativeUrl(string virtualPath) {
        // If it has a protocol, it's not relative
        if (HasScheme(virtualPath))
            return false;

        return !IsRooted(virtualPath);
    }


    internal static bool IsAppRelativePath(string path) {

        if (path == null)
            return false;

        int len = path.Length;

        // Empty string case
        if (len == 0) return false;

        // It must start with ~
        if (path[0] != appRelativeCharacter)
            return false;

        // Single character case: "~"
        if (len == 1)
            return true;

        // If it's longer, checks if it starts with "~/" or "~\"
        return path[1] == '\\' || path[1] == '/';
    }

    internal static bool IsValidVirtualPathWithoutProtocol(string path) {
        if (path == null)
            return false;
        return !HasScheme(path);
    }

    internal static String GetDirectory(String path) {
        if (String.IsNullOrEmpty(path))
            throw new ArgumentException(SR.GetString(SR.Empty_path_has_no_directory));

        if (path[0] != '/' && path[0] != appRelativeCharacter)
            throw new ArgumentException(SR.GetString(SR.Path_must_be_rooted, path));

        // If it's just "~" or "/", return it unchanged
        if (path.Length == 1)
            return path;

        int slashIndex = path.LastIndexOf('/');

        // This could happen if the input looks like "~abc"
        if (slashIndex < 0)
            throw new ArgumentException(SR.GetString(SR.Path_must_be_rooted, path));

        return path.Substring(0, slashIndex + 1);
    }

    private static bool IsDirectorySeparatorChar(char ch) {
        return (ch == '\\' || ch == '/');
    }

    internal static bool IsAbsolutePhysicalPath(string path) {
        if (path == null || path.Length < 3)
            return false;

        // e.g c:\foo
        if (path[1] == ':' && IsDirectorySeparatorChar(path[2]))
            return true;

        // e.g \\server\share\foo or //server/share/foo
        return IsUncSharePath(path);
    }

    internal static bool IsUncSharePath(string path) {
        // e.g \\server\share\foo or //server/share/foo
        if (path.Length > 2 && IsDirectorySeparatorChar(path[0]) && IsDirectorySeparatorChar(path[1]))
            return true;
        return false;

    }

    internal static void CheckValidVirtualPath(string path) {

        // Check if it looks like a physical path (UNC shares and C:)
        if (IsAbsolutePhysicalPath(path)) {
            throw new HttpException(SR.GetString(SR.Physical_path_not_allowed, path));
        }

        // Virtual path can't have colons.
        int iqs = path.IndexOf('?');
        if (iqs >= 0) {
            path = path.Substring(0, iqs);
        }
        if (HasScheme(path)) {
            throw new HttpException(SR.GetString(SR.Invalid_vpath, path));
        }
    }

    private static String Combine(String appPath, String basepath, String relative) {
        String path;

        if (String.IsNullOrEmpty(relative))
            throw new ArgumentNullException("relative");
        if (String.IsNullOrEmpty(basepath))
            throw new ArgumentNullException("basepath");

        if (basepath[0] == appRelativeCharacter && basepath.Length == 1) {
            // If it's "~", change it to "~/"
            basepath = appRelativeCharacterString;
        }
        else {
            // If the base path includes a file name, get rid of it before combining
            int lastSlashIndex = basepath.LastIndexOf('/');
            Debug.Assert(lastSlashIndex >= 0);
            if (lastSlashIndex < basepath.Length - 1) {
                basepath = basepath.Substring(0, lastSlashIndex + 1);
            }
        }

        // Make sure it's a virtual path (ASURT 73641)
        CheckValidVirtualPath(relative);

        if (IsRooted(relative)) {
            path = relative;
        }
        else {

            // If the path is exactly "~", just return the app root path
            if (relative.Length == 1 && relative[0] == appRelativeCharacter)
                return appPath;

            // If the relative path starts with "~/" or "~\", treat it as app root
            // relative (ASURT 68628)
            if (IsAppRelativePath(relative)) {
                if (appPath.Length > 1)
                    path = appPath + "/" + relative.Substring(2);
                else
                    path = "/" + relative.Substring(2);
            } else {
                path = SimpleCombine(basepath, relative);
            }
        }

        return Reduce(path);
    }

    internal static String Combine(String basepath, String relative) {
        return Combine(HttpRuntime.AppDomainAppVirtualPathString, basepath, relative);
    }

    // This simple version of combine should only be used when the relative
    // path is known to be relative.  It's more efficient, but doesn't do any
    // sanity checks.
    internal static String SimpleCombine(String basepath, String relative) {
        Debug.Assert(!String.IsNullOrEmpty(basepath));
        Debug.Assert(!String.IsNullOrEmpty(relative));
        Debug.Assert(relative[0] != '/');

        if (HasTrailingSlash(basepath))
            return basepath + relative;
        else
            return basepath + "/" + relative;
    }

    internal static String Reduce(String path) {

        // ignore query string
        String queryString = null;
        if (path != null) {
            int iqs = path.IndexOf('?');
            if (iqs >= 0) {
                queryString = path.Substring(iqs);
                path = path.Substring(0, iqs);
            }
        }

        // Take care of backslashes and duplicate slashes
        path = FixVirtualPathSlashes(path);

        path = ReduceVirtualPath(path);

        return (queryString != null) ? (path + queryString) : path;
    }

    // Same as Reduce, but for a virtual path that is known to be well formed
    internal static String ReduceVirtualPath(String path) {

        int length = path.Length;
        int examine;

        // quickly rule out situations in which there are no . or ..

        for (examine = 0; ; examine++) {
            examine = path.IndexOf('.', examine);
            if (examine < 0)
                return path;

            if ((examine == 0 || path[examine - 1] == '/')
                && (examine + 1 == length || path[examine + 1] == '/' ||
                    (path[examine + 1] == '.' && (examine + 2 == length || path[examine + 2] == '/'))))
                break;
        }

        // OK, we found a . or .. so process it:

        ArrayList list = new ArrayList();
        StringBuilder sb = new StringBuilder();
        int start;
        examine = 0;

        for (; ; ) {
            start = examine;
            examine = path.IndexOf('/', start + 1);

            if (examine < 0)
                examine = length;

            if (examine - start <= 3 &&
                (examine < 1 || path[examine - 1] == '.') &&
                (start + 1 >= length || path[start + 1] == '.')) {
                if (examine - start == 3) {
                    if (list.Count == 0)
                        throw new HttpException(SR.GetString(SR.Cannot_exit_up_top_directory));

                    // We're about to backtrack onto a starting '~', which would yield
                    // incorrect results.  Instead, make the path App Absolute, and call
                    // Reduce on that.
                    if (list.Count == 1 && IsAppRelativePath(path)) {
                        Debug.Assert(sb.Length == 1);
                        return ReduceVirtualPath(MakeVirtualPathAppAbsolute(path));
                    }

                    sb.Length = (int)list[list.Count - 1];
                    list.RemoveRange(list.Count - 1, 1);
                }
            }
            else {
                list.Add(sb.Length);

                sb.Append(path, start, examine - start);
            }

            if (examine == length)
                break;
        }

        string result = sb.ToString();

        // If we end up with en empty string, turn it into either "/" or "." (VSWhidbey 289175)
        if (result.Length == 0) {
            if (length > 0 && path[0] == '/')
                result = @"/";
            else
                result = ".";
        }

        return result;
    }

    // Change backslashes to forward slashes, and remove duplicate slashes
    internal static String FixVirtualPathSlashes(string virtualPath) {

        // Make sure we don't have any back slashes
        virtualPath = virtualPath.Replace('\\', '/');

        // Replace any double forward slashes
        for (;;) {
            string newPath = virtualPath.Replace("//", "/");

            // If it didn't do anything, we're done
            if ((object)newPath == (object)virtualPath)
                break;

            // We need to loop again to take care of triple (or more) slashes (VSWhidbey 288782)
            virtualPath = newPath;
        }

        return virtualPath;
    }

    // We use file: protocol instead of http:, so that Uri.MakeRelative behaves
    // in a case insensitive way (VSWhidbey 80078)
    private const string dummyProtocolAndServer = "file://foo";

    // Return the relative vpath path from one rooted vpath to another
    internal static string MakeRelative(string from, string to) {

        // If either path is app relative (~/...), make it absolute, since the Uri
        // class wouldn't know how to deal with it.
        from = MakeVirtualPathAppAbsolute(from);
        to = MakeVirtualPathAppAbsolute(to);

        // Make sure both virtual paths are rooted
        if (!IsRooted(from))
            throw new ArgumentException(SR.GetString(SR.Path_must_be_rooted, from));
        if (!IsRooted(to))
            throw new ArgumentException(SR.GetString(SR.Path_must_be_rooted, to));

        // Remove the query string, so that System.Uri doesn't corrupt it
        string queryString = null;
        if (to != null) {
            int iqs = to.IndexOf('?');
            if (iqs >= 0) {
                queryString = to.Substring(iqs);
                to = to.Substring(0, iqs);
            }
        }

        // Uri's need full url's so, we use a dummy root
        Uri fromUri = new Uri(dummyProtocolAndServer + from);
        Uri toUri = new Uri(dummyProtocolAndServer + to);

        string relativePath;

        // VSWhidbey 144946: If to and from points to identical path (excluding query and fragment), just use them instead
        // of returning an empty string.
        if (fromUri.Equals(toUri)) {
            int iPos = to.LastIndexOfAny(s_slashChars);

            if (iPos >= 0) {

                // If it's the same directory, simply return "./"
                // Browsers should interpret "./" as the original directory.
                if (iPos == to.Length - 1) {
                    relativePath = "./";
                }
                else {
                    relativePath = to.Substring(iPos + 1);
                }
            }
            else {
                relativePath = to;
            }
        }
        else {
// To avoid deprecation warning.  It says we should use MakeRelativeUri instead (which returns a Uri),
// but we wouldn't gain anything from it.  The way we use MakeRelative is hacky anyway (fake protocol, ...),
// and I don't want to take the chance of breaking something with this change.
#pragma warning disable 0618
            relativePath = fromUri.MakeRelative(toUri);
#pragma warning restore 0618
        }

        // Note that we need to re-append the query string and fragment (e.g. #anchor)
        return relativePath + queryString + toUri.Fragment;
    }

    internal static string GetDirectoryOrRootName(string path) {
        string dir;

        dir = Path.GetDirectoryName(path);
        if (dir == null) {
            dir = Path.GetPathRoot(path);
        }

        return dir;
    }

    internal static string GetFileName(string virtualPath) {
        // Code copied from CLR\BCL\System\IO\Path.cs
        //    - Check for invalid chars removed
        //    - Only '/' is used as separator (path.cs also used '\' and ':')
        if (virtualPath != null) {
            int length = virtualPath.Length;
            for (int i = length; --i >= 0;) {
                char ch = virtualPath[i];
                if (ch == '/')
                    return virtualPath.Substring(i + 1, length - i - 1);

            }
        }
        return virtualPath;
    }

    internal static string GetFileNameWithoutExtension(string virtualPath) {
        // Code copied from CLR\BCL\System\IO\Path.cs
        //    - Check for invalid chars removed
        virtualPath = GetFileName(virtualPath);
        if (virtualPath != null) {
            int i;
            if ((i=virtualPath.LastIndexOf('.')) == -1)
                return virtualPath; // No extension found
            else
                return virtualPath.Substring(0,i);
        }
        return null;
    }

    internal static string GetExtension(string virtualPath) {
        if (virtualPath == null)
            return null;

        int length = virtualPath.Length;
        for (int i = length; --i >= 0;) {
            char ch = virtualPath[i];
            if (ch == '.') {
                if (i != length - 1)
                    return virtualPath.Substring(i, length - i);
                else
                    return String.Empty;
            }
            if (ch == '/')
                break;
        }
        return String.Empty;
    }

    internal static bool HasTrailingSlash(string virtualPath) {
        return virtualPath[virtualPath.Length - 1] == '/';
    }

    internal static string AppendSlashToPathIfNeeded(string path) {

        if (path == null) return null;

        int l = path.Length;
        if (l == 0) return path;

        if (path[l-1] != '/')
            path += '/';

        return path;
    }

    //
    // Remove the trailing forward slash ('/') except in the case of the root ("/").
    // If the string is null or empty, return null, which represents a machine.config or root web.config.
    //
    internal static string RemoveSlashFromPathIfNeeded(string path) {
        if (string.IsNullOrEmpty(path)) {
            return null;
        }

        int l = path.Length;
        if (l <= 1 || path[l-1] != '/') {
            return path;
        }

        return path.Substring(0, l-1);
    }

    private static bool VirtualPathStartsWithVirtualPath(string virtualPath1, string virtualPath2) {
        if (virtualPath1 == null) {
            throw new ArgumentNullException("virtualPath1");
        }

        if (virtualPath2 == null) {
            throw new ArgumentNullException("virtualPath2");
        }

        // if virtualPath1 as a string doesn't start with virtualPath2 as s string, then no for sure
        if (!StringUtil.StringStartsWithIgnoreCase(virtualPath1, virtualPath2)) {
            return false;
        }

        int virtualPath2Length = virtualPath2.Length;

        // same length - same path
        if (virtualPath1.Length == virtualPath2Length) {
            return true;
        }

        // Special case for apps rooted at the root. VSWhidbey 286145
        if (virtualPath2Length == 1) {
            Debug.Assert(virtualPath2[0] == '/');
            return true;
        }

        // If virtualPath2 ends with a '/', it's definitely a child
        if (virtualPath2[virtualPath2Length - 1] == '/')
            return true;

        // If it doesn't, make sure the next char in virtualPath1 is a '/'.
        // e.g. /app1 vs /app11 (VSWhidbey 285038)
        if (virtualPath1[virtualPath2Length] != '/') {
            return false;
        }

        // passed all checks
        return true;
    }

    internal static bool VirtualPathStartsWithAppPath(string virtualPath) {
        Debug.Assert(HttpRuntime.AppDomainAppVirtualPathObject != null);
        return VirtualPathStartsWithVirtualPath(virtualPath,
            HttpRuntime.AppDomainAppVirtualPathString);
    }

    internal static string MakeVirtualPathAppRelative(string virtualPath) {
        Debug.Assert(HttpRuntime.AppDomainAppVirtualPathObject != null);
        return MakeVirtualPathAppRelative(virtualPath,
            HttpRuntime.AppDomainAppVirtualPathString, false /*nullIfNotInApp*/);
    }

    // Same as MakeVirtualPathAppRelative, but return null if app relative can't be obtained
    internal static string MakeVirtualPathAppRelativeOrNull(string virtualPath) {
        Debug.Assert(HttpRuntime.AppDomainAppVirtualPathObject != null);
        return MakeVirtualPathAppRelative(virtualPath,
            HttpRuntime.AppDomainAppVirtualPathString, true /*nullIfNotInApp*/);
    }

    // If a virtual path starts with the app path, make it start with
    // ~ instead, so that it becomes application agnostic
    // E.g. /MyApp/Sub/foo.aspx --> ~/Sub/foo.aspx
    internal static string MakeVirtualPathAppRelative(string virtualPath,
        string applicationPath, bool nullIfNotInApp) {

        if (virtualPath == null)
            throw new ArgumentNullException("virtualPath");

        Debug.Assert(applicationPath[0] == '/');
        Debug.Assert(HasTrailingSlash(applicationPath));

        int appPathLength = applicationPath.Length;
        int virtualPathLength = virtualPath.Length;

        // If virtualPath is the same as the app path, but without the ending slash,
        // treat it as if it were truly the app path (VSWhidbey 495949)
        if (virtualPathLength == appPathLength - 1) {
            if (StringUtil.StringStartsWithIgnoreCase(applicationPath, virtualPath))
                return appRelativeCharacterString;
        }

        if (!VirtualPathStartsWithVirtualPath(virtualPath, applicationPath)) {
            // If it doesn't start with the app path, return either null or the input path
            if (nullIfNotInApp)
                return null;
            else
                return virtualPath;
        }

        // If they are the same, just return "~/"
        if (virtualPathLength == appPathLength)
            return appRelativeCharacterString;

        // Special case for apps rooted at the root:
        if (appPathLength == 1) {
            return appRelativeCharacter + virtualPath;
        }

        return appRelativeCharacter + virtualPath.Substring(appPathLength-1);
    }

    internal static string MakeVirtualPathAppAbsolute(string virtualPath) {
        Debug.Assert(HttpRuntime.AppDomainAppVirtualPathObject != null);
        return MakeVirtualPathAppAbsolute(virtualPath, HttpRuntime.AppDomainAppVirtualPathString);
    }

    // If a virtual path is app relative (i.e. starts with ~/), change it to
    // start with the actuall app path.
    // E.g. ~/Sub/foo.aspx --> /MyApp/Sub/foo.aspx
    internal static string MakeVirtualPathAppAbsolute(string virtualPath, string applicationPath) {

        // If the path is exactly "~", just return the app root path
        if (virtualPath.Length == 1 && virtualPath[0] == appRelativeCharacter)
            return applicationPath;

        // If the virtual path starts with "~/" or "~\", replace with the app path
        // relative (ASURT 68628)
        if (virtualPath.Length >=2 && virtualPath[0] == appRelativeCharacter &&
            (virtualPath[1] == '/' || virtualPath[1] == '\\')) {

            if (applicationPath.Length > 1) {
                Debug.Assert(HasTrailingSlash(applicationPath));
                return applicationPath + virtualPath.Substring(2);
            }
            else
                return "/" + virtualPath.Substring(2);
        }

        // Don't allow relative paths, since they cannot be made App Absolute
        if (!IsRooted(virtualPath))
            throw new ArgumentOutOfRangeException("virtualPath");

        // Return it unchanged
        return virtualPath;
    }

    // To be called by APIs accepting virtual path that is expectedto be within the app.
    // returns reduced absolute virtual path or throws
    internal static string MakeVirtualPathAppAbsoluteReduceAndCheck(string virtualPath) {
        if (virtualPath == null) {
            throw new ArgumentNullException("virtualPath");
        }

        string path = Reduce(MakeVirtualPathAppAbsolute(virtualPath));

        if (!UrlPath.VirtualPathStartsWithAppPath(path)) {
            throw new ArgumentException(SR.GetString(SR.Invalid_app_VirtualPath, virtualPath));
        }

        return path;
    }

    internal static bool PathEndsWithExtraSlash(String path) {
        if (path == null)
            return false;
        int l = path.Length;
        if (l == 0 || path[l-1] != '\\')
            return false;
        if (l == 3 && path[1] == ':')   // c:\ case
            return false;
        return true;
    }

    internal static bool PathIsDriveRoot(string path) {
        if (path != null) {
            int l = path.Length;
            if (l == 3 && path[1] == ':' && path[2] == '\\') {
                return true;
            }
        }

        return false;
    }

    //
    // NOTE: This function is also present in fx\src\configuration\system\configuration\urlpath.cs
    // Please propagate any changes to that file.
    //
    // Determine if subpath is a subpath of path.
    // For example, /myapp/foo.aspx is a subpath of /myapp
    // Account for optional trailing slashes.
    //
    internal static bool IsEqualOrSubpath(string path, string subpath) {
        if (String.IsNullOrEmpty(path))
            return true;

        if (String.IsNullOrEmpty(subpath))
            return false;

        //
        // Compare up to but not including trailing slash
        //
        int lPath = path.Length;
        if (path[lPath - 1] == '/') {
            lPath -= 1;
        }

        int lSubpath = subpath.Length;
        if (subpath[lSubpath - 1] == '/') {
            lSubpath -= 1;
        }

        if (lSubpath < lPath)
            return false;

        if (!StringUtil.EqualsIgnoreCase(path, 0, subpath, 0, lPath))
            return false;

        // Check subpath that character following length of path is a slash
        if (lSubpath > lPath && subpath[lPath] != '/')
            return false;

        return true;
    }

    internal static bool IsPathOnSameServer(string absUriOrLocalPath, Uri currentRequestUri)
    {
        // Assuming
        // (1) currentRequestUri does belong to the THIS host
        // (2) absUriOrLocalPath is allowed to have different scheme like file:// or https://
        // (3) absUriOrLocalPath is allowed to point "above" the currentRequestUri path
        Uri absUri;
        if (!Uri.TryCreate(absUriOrLocalPath, UriKind.Absolute, out absUri)) {
            // MSRC 11063
            // A failure to construct absolute url (by System.Uri) doesn't implictly mean the url is relative (on the same server)
            // Make sure the url path can't be recognized as absolute
            return AppSettings.AllowRelaxedRelativeUrl ||
                   ((IsRooted(absUriOrLocalPath) || IsRelativeUrl(absUriOrLocalPath)) && !absUriOrLocalPath.TrimStart(' ').StartsWith("//", StringComparison.Ordinal));
        }

        return absUri.IsLoopback || string.Equals(currentRequestUri.Host, absUri.Host, StringComparison.OrdinalIgnoreCase);
    }


}
}
