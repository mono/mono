//------------------------------------------------------------------------------
// <copyright file="CookielessHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * CookielessHelper class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System;
    using System.Web;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Caching;
    using System.Collections;
    using System.Web.Util;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.Threading;
    using System.Globalization;
    using System.Security.Permissions;

    internal sealed class CookielessHelperClass {
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        internal CookielessHelperClass(HttpContext context) {
            _Context = context;
        }
        private void Init()
        {
            if (_Headers != null)
                return;
            if (_Headers == null)
                GetCookielessValuesFromHeader();
            if (_Headers == null)
                RemoveCookielessValuesFromPath();
            if (_Headers == null)
                _Headers = String.Empty;
            _OriginalHeaders = _Headers;
        }

        private void GetCookielessValuesFromHeader()
        {
            _Headers = _Context.Request.Headers[COOKIELESS_SESSION_FILTER_HEADER];

            // Dev10 775061 Regression: FormsAuthentication.SignOut does not redirect if cookieless forms authentication is enabled.
            // HttpContext.Init calls RemoveCookielessValues directly so we need to also store the OriginalHeaders for Redirect
            // calling Init directly causes regressions
            _OriginalHeaders = _Headers;
            if (!String.IsNullOrEmpty(_Headers)) {
                // QFE 4512: Ignore old V1.1 Session ids for cookieless in V2.0
                if (_Headers.Length == 24 && !_Headers.Contains("(")) {
                    _Headers = null;
                }
                else {
                    _Context.Response.SetAppPathModifier("(" + _Headers + ")");
                }
            }
        }

        // This function is called for all requests -- it must be performant.
        //    In the common case (i.e. value not present in the URI, it must not
        //    look at the headers collection
        internal void RemoveCookielessValuesFromPath()
        {
            // See if the path contains "/(XXXXX)/"
            string   path      = _Context.Request.ClientFilePath.VirtualPathString;
            // Optimize for the common case where there is no cookie
            if (path.IndexOf('(') == -1) {
                return;
            }
            int      endPos    = path.LastIndexOf(")/", StringComparison.Ordinal);
            int      startPos  = (endPos > 2 ?  path.LastIndexOf("/(", endPos - 1, endPos, StringComparison.Ordinal) : -1);

            if (startPos < 0) // pattern not found: common case, exit immediately
                return;

            if (_Headers == null) // Header should always be processed first
                GetCookielessValuesFromHeader();

            // if the path contains a cookie, remove it
            if (IsValidHeader(path, startPos + 2, endPos))
            {
                // only set _Headers if not already set
                if (_Headers == null) {
                    _Headers = path.Substring(startPos + 2, endPos - startPos - 2);
                }
                // Rewrite the path
                path = path.Substring(0, startPos) + path.Substring(endPos+1);

                // remove cookie from ClientFilePath
                _Context.Request.ClientFilePath = VirtualPath.CreateAbsolute(path);
                // get and append query string to path if it exists
                string rawUrl = _Context.Request.RawUrl;
                int qsIndex = rawUrl.IndexOf('?');
                if (qsIndex > -1) {
                    path = path + rawUrl.Substring(qsIndex);
                }
                // remove cookie from RawUrl
                _Context.Request.RawUrl = path;

                if (!String.IsNullOrEmpty(_Headers)) {
                    _Context.Request.ValidateCookielessHeaderIfRequiredByConfig(_Headers); // ensure that the path doesn't contain invalid chars
                    _Context.Response.SetAppPathModifier("(" + _Headers + ")");

                    // For Cassini and scenarios where aspnet_filter.dll is not used,
                    // HttpRequest.FilePath also needs to have the cookie removed.
                    string filePath = _Context.Request.FilePath;
                    string newFilePath = _Context.Response.RemoveAppPathModifier(filePath);
                    if (!Object.ReferenceEquals(filePath, newFilePath)) {
                        _Context.RewritePath(VirtualPath.CreateAbsolute(newFilePath),
                                             _Context.Request.PathInfoObject,
                                             null /*newQueryString*/,
                                             false /*setClientFilePath*/);
                    }
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        // Get the cookie value from the header/path based on the identifying char
        internal string GetCookieValue(char identifier) {
            int      startPos = 0;
            int      endPos = 0;
            string   returnValue;

            Init();
            ////////////////////////////////////////////////////////////
            // Step 1: Get the positions
            if (!GetValueStartAndEnd(_Headers, identifier, out startPos, out endPos))
                return null;

            returnValue = _Headers.Substring(startPos, endPos-startPos); // get the substring
            return returnValue;
        }
        internal bool DoesCookieValueExistInOriginal(char identifier) {
            int      startPos = 0;
            int      endPos = 0;
            Init();
            return GetValueStartAndEnd(_OriginalHeaders, identifier, out startPos, out endPos);
        }


        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        // Add/Change/Delete a cookie value, given the tag
        internal void SetCookieValue(char identifier, string cookieValue) {
            int      startPos = 0;
            int      endPos = 0;

            Init();
            ////////////////////////////////////////////////////////////
            // Step 1: Remove old values
            while (GetValueStartAndEnd(_Headers, identifier, out startPos, out endPos)) { // find old value position
                _Headers = _Headers.Substring(0, startPos-2) + _Headers.Substring(endPos+1); // Remove old value
            }

            ////////////////////////////////////////////////////////////
            // Step 2: Add new Value if value is specified
            if (!String.IsNullOrEmpty(cookieValue)) {
                _Headers += new string(new char[] {identifier, '('}) + cookieValue + ")";
            }

            ////////////////////////////////////////////////////////////
            // Step 3: Set path for redirection
            if (_Headers.Length > 0)
                _Context.Response.SetAppPathModifier("(" + _Headers + ")");
            else
                _Context.Response.SetAppPathModifier(null);
        }
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        // Get the position (start & end) of cookie value given the identifier
        private static bool GetValueStartAndEnd(string headers, char identifier, out int startPos, out int endPos) {

            ////////////////////////////////////////////////////////////
            // Step 1: Check if the header is non-empty
            if (String.IsNullOrEmpty(headers)) {
                startPos = endPos = -1;
                return false;
            }

            ////////////////////////////////////////////////////////////
            // Step 2: Get the start pos
            string tag = new string(new char[] {identifier, '('}); // Tag is: "X("
            startPos = headers.IndexOf(tag, StringComparison.Ordinal); // Search for "X("
            if (startPos < 0) { // Not Found
                startPos = endPos = -1;
                return false;
            }
            startPos += 2;

            ////////////////////////////////////////////////////////////
            // Step 3: Find the end position
            endPos = headers.IndexOf(')', startPos);
            if (endPos < 0) {
                startPos = endPos = -1;
                return false;
            }
            return true;
        }
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        // Look at config (<httpCookies>) and (maybe) current request, and return
        //     whether to use cookie-less feature
        internal static bool UseCookieless(HttpContext context, bool doRedirect, HttpCookieMode cookieMode) {

            ////////////////////////////////////////////////////////////
            // Step 2: Look at config
            switch(cookieMode)
            {
            case HttpCookieMode.UseUri: // Use cookieless always
                return true;

            case HttpCookieMode.UseCookies: // Never use cookieless
                return false;

            case HttpCookieMode.UseDeviceProfile: // Use browser config
                if (context == null)
                    context = HttpContext.Current;
                if (context == null)
                    return false;
                bool fRet = (!context.Request.Browser.Cookies ||
                             !context.Request.Browser.SupportsRedirectWithCookie);
                return fRet;

            case HttpCookieMode.AutoDetect: // Detect based on whether the client supports cookieless
                if (context == null)
                    context = HttpContext.Current;
                if (context == null)
                    return false;

                if (!context.Request.Browser.Cookies || !context.Request.Browser.SupportsRedirectWithCookie) {
                    return true;
                }

                //////////////////////////////////////////////////////////////////
                // Look in the header
                string cookieDetectHeader = context.CookielessHelper.GetCookieValue('X');
                if (cookieDetectHeader != null && cookieDetectHeader == "1") {
                    return true;
                }

                //////////////////////////////////////////////////
                // Step 3a: See if the client sent any (request) cookies
                string cookieHeader = context.Request.Headers["Cookie"];
                if (!String.IsNullOrEmpty(cookieHeader)) { // Yes, client sent request cookies
                    return false;
                }


                //////////////////////////////////////////////////
                // Step 3b: See if we have done a challenge-response to detect cookie support
                string qs = context.Request.QueryString[s_AutoDetectName];
                if (qs != null && qs == s_AutoDetectValue) { // Yes, we have done a challenge-response: No cookies present => no cookie support
                    context.CookielessHelper.SetCookieValue('X', "1");
                    return true;
                }


                //////////////////////////////////////////////////
                // Step 3c: Do a challenge-response (redirect) to detect cookie support
                if (doRedirect) {
                    context.CookielessHelper.RedirectWithDetection(null);
                }
                // Note: if doRedirect==true, execution won't reach here

                return false;

            default: // Config broken?
                return false;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        // Do a redirect to figure out cookies are supported or not.
        ///    The way we do it: write the cookie and in the query-string,
        //     write the cookie-name value pair.
        internal void  RedirectWithDetection(string redirectPath) {

            Init();
            ////////////////////////////////////////////////////////////
            // Step 1: If URL to redirect to, has not been specified,
            //         the use the current URL
            if (String.IsNullOrEmpty(redirectPath)) {
                redirectPath = _Context.Request.RawUrl;
            }

            ////////////////////////////////////////////////////////////
            // Step 2: Add cookie name and value to query-string
            if (redirectPath.IndexOf("?", StringComparison.Ordinal) > 0)
                redirectPath += "&" + s_AutoDetectName + "=" + s_AutoDetectValue;
            else
                redirectPath += "?" + s_AutoDetectName + "=" + s_AutoDetectValue;


            ////////////////////////////////////////////////////////////
            // Step 3: Add cookie
            _Context.Response.Cookies.Add(new HttpCookie(s_AutoDetectName, s_AutoDetectValue));

            ////////////////////////////////////////////////////////////
            // Step 4: Do redirect
            _Context.Response.Redirect(redirectPath, true);
        }

        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        // RedirectWithDetection if we need to
        internal void RedirectWithDetectionIfRequired(string redirectPath, HttpCookieMode cookieMode) {

            Init();
            ////////////////////////////////////////////////////////////
            // Step 1: Check if config wants us to do a challenge-reponse
            if (cookieMode != HttpCookieMode.AutoDetect) {
                return;
            }

            ////////////////////////////////////////////////////////////
            // Step 2: Check browser caps
            if (!_Context.Request.Browser.Cookies || !_Context.Request.Browser.SupportsRedirectWithCookie) {
                return;
            }

            ////////////////////////////////////////////////////////////
            // Step 3: Check header
            string cookieDetectHeader = GetCookieValue('X');
            if (cookieDetectHeader != null && cookieDetectHeader == "1") {
                return;
            }

            ////////////////////////////////////////////////////////////
            // Step 4: see if the client sent any cookies
            string cookieHeader = _Context.Request.Headers["Cookie"];
            if (!String.IsNullOrEmpty(cookieHeader)) {
                return;
            }

            ////////////////////////////////////////////////////////////
            // Step 5: See if we already did a challenge-reponse
            string qs = _Context.Request.QueryString[s_AutoDetectName];
            if (qs != null && qs == s_AutoDetectValue) { // Yes, we have done a challenge-response
                                                          // No cookies present => no cookie support
                SetCookieValue('X', "1");
                return;
            }
            ////////////////////////////////////////////////////////////
            // Do a challenge response
            RedirectWithDetection(redirectPath);
        }

        ///////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////
        // Make sure sub-string if of the pattern: A(XXXX)N(XXXXX)P(XXXXX) and so on.
        static private bool IsValidHeader(string path, int startPos, int endPos)
        {
            if (endPos - startPos < 3) // Minimum len is "X()"
                return false;

            while (startPos <= endPos - 3) { // Each iteration deals with one "A(XXXX)" pattern

                if (path[startPos] < 'A' || path[startPos] > 'Z') // Make sure pattern starts with a capital letter
                    return false;

                if (path[startPos + 1] != '(') // Make sure next char is '('
                    return false;

                startPos += 2;
                bool found = false;
                for (; startPos < endPos; startPos++) { // Find the ending ')'

                    if (path[startPos] == ')') { // found it!
                        startPos++; // Set position for the next pattern
                        found = true;
                        break; // Break out of this for-loop.
                    }

                    if (path[startPos] == '/') { // Can't contain path separaters
                        return false;
                    }
                }
                if (!found)  {
                    return false; // Ending ')' not found!
                }
            }

            if (startPos < endPos) // All chars consumed?
                return false;

            return true;
        }

        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        // Static data
        internal const String COOKIELESS_SESSION_FILTER_HEADER  = "AspFilterSessionId";
        const string         s_AutoDetectName                 = "AspxAutoDetectCookieSupport";
        const string         s_AutoDetectValue                = "1";

        // Private instance data
        private HttpContext  _Context;
        private string       _Headers;
        private string       _OriginalHeaders;
    }

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    internal enum CookiesSupported {
        Supported, NotSupported, Unknown
    }
}
