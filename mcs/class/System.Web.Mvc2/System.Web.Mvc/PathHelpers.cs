/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections.Specialized;
    using System.Web;

    internal static class PathHelpers {

        private const string _urlRewriterServerVar = "HTTP_X_ORIGINAL_URL";

        // this method can accept an app-relative path or an absolute path for contentPath
        public static string GenerateClientUrl(HttpContextBase httpContext, string contentPath) {
            if (String.IsNullOrEmpty(contentPath)) {
                return contentPath;
            }

            // many of the methods we call internally can't handle query strings properly, so just strip it out for
            // the time being
            string query;
            contentPath = StripQuery(contentPath, out query);

            return GenerateClientUrlInternal(httpContext, contentPath) + query;
        }

        private static string GenerateClientUrlInternal(HttpContextBase httpContext, string contentPath) {
            if (String.IsNullOrEmpty(contentPath)) {
                return contentPath;
            }

            // can't call VirtualPathUtility.IsAppRelative since it throws on some inputs
            bool isAppRelative = contentPath[0] == '~';
            if (isAppRelative) {
                string absoluteContentPath = VirtualPathUtility.ToAbsolute(contentPath, httpContext.Request.ApplicationPath);
                string modifiedAbsoluteContentPath = httpContext.Response.ApplyAppPathModifier(absoluteContentPath);
                return GenerateClientUrlInternal(httpContext, modifiedAbsoluteContentPath);
            }

            // we only want to manipulate the path if URL rewriting is active, else we risk breaking the generated URL
            NameValueCollection serverVars = httpContext.Request.ServerVariables;
            bool urlRewriterIsEnabled = (serverVars != null && serverVars[_urlRewriterServerVar] != null);
            if (!urlRewriterIsEnabled) {
                return contentPath;
            }

            // Since the rawUrl represents what the user sees in his browser, it is what we want to use as the base
            // of our absolute paths. For example, consider mysite.example.com/foo, which is internally
            // rewritten to content.example.com/mysite/foo. When we want to generate a link to ~/bar, we want to
            // base it from / instead of /foo, otherwise the user ends up seeing mysite.example.com/foo/bar,
            // which is incorrect.
            string relativeUrlToDestination = MakeRelative(httpContext.Request.Path, contentPath);
            string absoluteUrlToDestination = MakeAbsolute(httpContext.Request.RawUrl, relativeUrlToDestination);
            return absoluteUrlToDestination;
        }

        public static string MakeAbsolute(string basePath, string relativePath) {
            // The Combine() method can't handle query strings on the base path, so we trim it off.
            string query;
            basePath = StripQuery(basePath, out query);
            return VirtualPathUtility.Combine(basePath, relativePath);
        }

        public static string MakeRelative(string fromPath, string toPath) {
            string relativeUrl = VirtualPathUtility.MakeRelative(fromPath, toPath);
            if (String.IsNullOrEmpty(relativeUrl) || relativeUrl[0] == '?') {
                // Sometimes VirtualPathUtility.MakeRelative() will return an empty string when it meant to return '.',
                // but links to {empty string} are browser dependent. We replace it with an explicit path to force
                // consistency across browsers.
                relativeUrl = "./" + relativeUrl;
            }
            return relativeUrl;
        }

        private static string StripQuery(string path, out string query) {
            int queryIndex = path.IndexOf('?');
            if (queryIndex >= 0) {
                query = path.Substring(queryIndex);
                return path.Substring(0, queryIndex);
            }
            else {
                query = null;
                return path;
            }
        }

    }
}
