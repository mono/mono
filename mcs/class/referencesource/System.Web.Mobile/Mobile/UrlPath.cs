//------------------------------------------------------------------------------
// <copyright file="UrlPath.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * UrlPath class.
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

using System.Text;
using System.Runtime.Serialization.Formatters;
using System.Runtime.InteropServices;
using System.Collections;
using System.Diagnostics;

namespace System.Web.Mobile
{
    /*
     * URL Path library.
     */
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal static class UrlPath
    {
        private const char appRelativeCharacter = '~';

        internal static bool IsRooted(String basepath)
        {
            return(basepath == null || basepath.Length == 0 || basepath[0] == '/' || basepath[0] == '\\');
        }

        internal static bool IsRelativeUrl(string url)
        {
            // If it has a protocol, it's not relative
            if (url.IndexOf(":", StringComparison.Ordinal) != -1)
            {
                return false;
            }

            return !IsRooted(url);
        }

        internal static String GetDirectory(String path)
        {
            if (path == null || path.Length == 0)
            {
                throw new ArgumentException(SR.GetString(SR.UrlPath_EmptyPathHasNoDirectory));
            }

            if (path[0] != '/' && path[0] != appRelativeCharacter)
            {
                throw new ArgumentException(SR.GetString(SR.UrlPath_PathMustBeRooted));
            }

            // Make sure there is a filename after the last '/'
            Debug.Assert(path[path.Length-1] != '/', "Path should not end with a /");

            string dir = path.Substring(0, path.LastIndexOf('/'));

            // If it's the root dir, we would end up with "".  Return "/" instead
            if (dir.Length == 0)
            {
                return "/";
            }

            return dir;
        }

        private static void FailIfPhysicalPath(string path)
        {
            if (path == null || path.Length < 4)
            {
                return;
            }

            if (path[1] == ':' || (path[0] == '\\' && path[1] == '\\'))
            {
                throw new Exception(SR.GetString(SR.UrlPath_PhysicalPathNotAllowed, path));
            }
        }

        internal static String Combine(String basepath, String relative)
        {
            String path;

            // Make sure the relative path is not a physical path (bug 73641)
            FailIfPhysicalPath(relative);

            if (IsRooted(relative))
            {
                path = relative;
                if (path == null || path.Length == 0)
                {
                    return String.Empty;
                }
            }
            else
            {
                // If the relative path starts with "~/" or "~\", treat it as app root
                // relative (bug 68628)
                if (relative.Length >=3 && relative[0] == appRelativeCharacter && (relative[1] == '/' || relative[1] == '\\'))
                {
                    String appPath = HttpRuntime.AppDomainAppVirtualPath;
                    if (appPath.Length > 1)
                    {
                        path = appPath + "/" + relative.Substring(2);
                    }
                    else
                    {
                        path = "/" + relative.Substring(2);
                    }
                }
                else
                {
                    if (basepath == null || (basepath.Length == 1 && basepath[0] == '/'))
                    {
                        basepath = String.Empty;
                    }

                    path = basepath + "/" + relative;
                }
            }

            return Reduce(path);
        }

        internal static String Reduce(String path)
        {
            // ignore query string
            String queryString = null;
            if (path != null)
            {
                int iqs = path.IndexOf('?');
                if (iqs >= 0)
                {
                    queryString = path.Substring(iqs);
                    path = path.Substring(0, iqs);
                }
            }

            int length = path.Length;
            int examine;

            // Make sure we don't have any back slashes
            path = path.Replace('\\', '/');

            // quickly rule out situations in which there are no . or ..

            for (examine = 0; ; examine++)
            {
                examine = path.IndexOf('.', examine);
                if (examine < 0)
                {
                    return (queryString != null) ? (path + queryString) : path;
                }

                if ((examine == 0 || path[examine - 1] == '/')
                    && (examine + 1 == length || path[examine + 1] == '/' ||
                        (path[examine + 1] == '.' && (examine + 2 == length || path[examine + 2] == '/'))))
                {
                    break;
                }
            }

            // OK, we found a . or .. so process it:

            ArrayList list = new ArrayList();
            StringBuilder sb = new StringBuilder();
            int start;
            examine = 0;

            for (;;)
            {
                start = examine;
                examine = path.IndexOf('/', start + 1);

                if (examine < 0)
                {
                    examine = length;
                }

                if (examine - start <= 3 &&
                    (examine < 1 || path[examine - 1] == '.') &&
                    (start + 1 >= length || path[start + 1] == '.'))
                {
                    if (examine - start == 3)
                    {
                        if (list.Count == 0)
                        {
                            throw new Exception(SR.GetString(SR.UrlPath_CannotExitUpTopDirectory));
                        }

                        sb.Length = (int)list[list.Count - 1];
                        list.RemoveRange(list.Count - 1, 1);
                    }
                }
                else
                {
                    list.Add(sb.Length);

                    sb.Append(path, start, examine - start);
                }

                if (examine == length)
                {
                    break;
                }
            }

            return sb.ToString() + queryString;
        }

        private const string dummyProtocolAndServer = "http://foo";

        // Return the relative vpath path from one rooted vpath to another
        internal static string MakeRelative(string from, string to)
        {
            // If either path is app relative (~/...), make it absolute, since the Uri
            // class wouldn't know how to deal with it.
            from = MakeVirtualPathAppAbsolute(from);
            to = MakeVirtualPathAppAbsolute(to);

            // Make sure both virtual paths are rooted
            Debug.Assert(IsRooted(from));
            Debug.Assert(IsRooted(to));

            // Uri's need full url's so, we use a dummy root
            Uri fromUri = new Uri(dummyProtocolAndServer + from);
            Uri toUri = new Uri(dummyProtocolAndServer + to);
            return fromUri.MakeRelative(toUri);
        }

        // If a virtual path is app relative (i.e. starts with ~/), change it to
        // start with the actuall app path.
        // E.g. ~/Sub/foo.aspx --> /MyApp/Sub/foo.aspx
        internal static string MakeVirtualPathAppAbsolute(string virtualPath) {

            // If the path is exactly "~", just return the app root path
            if (virtualPath.Length == 1 && virtualPath[0] == appRelativeCharacter)
                return HttpRuntime.AppDomainAppVirtualPath;

            // If the virtual path starts with "~/" or "~\", replace with the app path
            // relative (ASURT 68628)
            if (virtualPath.Length >=2 && virtualPath[0] == appRelativeCharacter &&
                (virtualPath[1] == '/' || virtualPath[1] == '\\')) {

                string appPath = HttpRuntime.AppDomainAppVirtualPath;

                if (appPath.Length > 1)
                    return appPath + "/" + virtualPath.Substring(2);
                else
                    return "/" + virtualPath.Substring(2);
            }

            // Return it unchanged
            return virtualPath;
        }
    }
}


