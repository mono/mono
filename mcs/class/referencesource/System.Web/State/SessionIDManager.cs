//------------------------------------------------------------------------------
// <copyright file="SessionIDManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * SessionIDManager
 *
 * Copyright (c) 1998-1999, Microsoft Corporation
 *
 */

namespace System.Web.SessionState {

    using System;
    using System.Collections;
    using System.IO;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Security.Cryptography;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Security;
    using System.Web.Management;
    using System.Web.Hosting;

    public interface ISessionIDManager {

        bool InitializeRequest(HttpContext context, bool suppressAutoDetectRedirect, out bool supportSessionIDReissue);

        // Get the session id from Context or Cookies.
        // Called by session state module in AcquireState event.
        String GetSessionID(HttpContext context);

        // Create a session id.
        String CreateSessionID(HttpContext context);

        // Save the session id to either URL or cookies.
        // For URL case, the param "redirected" will be set
        // to true, meaning the caller should call HttpApplication.CompleteRequest()
        // and return. Called by session state module at the end of AcquireState event
        // and if a session state is successfully retrieved.

        void SaveSessionID(HttpContext context, string id, out bool redirected, out bool cookieAdded);

        // If cookie-ful, remove the session id from cookie.
        // Called by session state module in the ReleaseState event if a new
        // session was created but was unused

        void RemoveSessionID(HttpContext context);

        // Called by GetSessionID to make sure the ID sent by the browser is legitimate

        bool Validate(String id);

        void Initialize();
    }

    /*
     * The sesssion id manager provides session id services
     * for an application.
     */
    public class SessionIDManager : ISessionIDManager {

        // cookieless vars

        const int COOKIELESS_SESSION_LENGTH = SessionId.ID_LENGTH_CHARS + 2;

        internal const String COOKIELESS_SESSION_KEY = "AspCookielessSession";
        internal const String COOKIELESS_BOOL_SESSION_KEY = "AspCookielessBoolSession";
        internal const String ASP_SESSIONID_MANAGER_INITIALIZEREQUEST_CALLED_KEY = "AspSessionIDManagerInitializeRequestCalled";

        static string   s_appPath;
        static int      s_iSessionId;


        internal const HttpCookieMode       COOKIEMODE_DEFAULT = HttpCookieMode.UseCookies;
        internal const String               SESSION_COOKIE_DEFAULT = "ASP.NET_SessionId";
        internal const int                  SESSION_ID_LENGTH_LIMIT = 80;

        #pragma warning disable 0649
        static ReadWriteSpinLock            s_lock;
        #pragma warning restore 0649
        static SessionStateSection          s_config;  

        bool                                _isInherited;

        RandomNumberGenerator               _randgen;

        public SessionIDManager() {
        }

        public static int SessionIDMaxLength {
            get { return SESSION_ID_LENGTH_LIMIT; }
        }

        void OneTimeInit() {
            SessionStateSection config = RuntimeConfig.GetAppConfig().SessionState;

            s_appPath = HostingEnvironment.ApplicationVirtualPathObject.VirtualPathString;

            // s_iSessionId is pointing to the starting "("
            s_iSessionId = s_appPath.Length;

            s_config = config;
        }

        static SessionStateSection Config {
            get {
                if (s_config == null) {
                    throw new HttpException(SR.GetString(SR.SessionIDManager_uninit));
                }

                return s_config;
            }
        }

        public void Initialize() {
            if (s_config == null) {
                s_lock.AcquireWriterLock();
                try {
                    if (s_config == null) {
                        OneTimeInit();
                    }
                }
                finally {
                    s_lock.ReleaseWriterLock();
                }
            }

            _isInherited = !(this.GetType() == typeof(SessionIDManager));
            
            Debug.Trace("SessionIDManager", "cookieMode = " + Config.Cookieless +
                        ", cookieName = " + Config.CookieName +
                        ", inherited = " + _isInherited);
        }

        internal void GetCookielessSessionID(HttpContext context, bool allowRedirect, out bool cookieless) {
            HttpRequest     request;
            string          id;

            Debug.Trace("SessionIDManager", "Beginning SessionIDManager::GetCookielessSessionID");

            request = context.Request;

            // Please note that even if the page doesn't require session state, we still need
            // to read the session id because we have to update the state's timeout value

            cookieless = CookielessHelperClass.UseCookieless(context, allowRedirect, Config.Cookieless);
            context.Items[COOKIELESS_BOOL_SESSION_KEY] = cookieless;

            Debug.Trace("SessionIDManager", "cookieless=" + cookieless);

            if (cookieless) {
                /*
                 * Check if it's cookie-less session id
                 */

                id = context.CookielessHelper.GetCookieValue('S');
                if (id == null)
                    id = String.Empty;
                // Decode() is caled on all id's before saved to URL or cookie
                id = Decode(id);
                if (!ValidateInternal(id, false)) {
                    Debug.Trace("SessionIDManager", "No legitimate cookie on path\nReturning from SessionStateModule::GetCookielessSessionID");
                    return;
                }

                context.Items.Add(COOKIELESS_SESSION_KEY, id);

                Debug.Trace("SessionIDManager", "CookielessSessionModule found SessionId=" + id +
                            "\nReturning from SessionIDManager::GetCookielessSessionID");            
            }

        }

        static HttpCookie CreateSessionCookie(String id) {
            HttpCookie  cookie;

            cookie = new HttpCookie(Config.CookieName, id);
            cookie.Path = "/";

            // VSWhidbey 414687 Use HttpOnly to prevent client side script manipulation of cookie
            cookie.HttpOnly = true;

            return cookie;
        }

        internal static bool CheckIdLength(string id, bool throwOnFail) {
            bool    ret = true;
            
            if (id.Length > SESSION_ID_LENGTH_LIMIT) {
                if (throwOnFail) {
                    throw new HttpException(
                        SR.GetString(SR.Session_id_too_long,
                            SESSION_ID_LENGTH_LIMIT.ToString(CultureInfo.InvariantCulture), id));
                }
                else {
                    ret = false;
                }
            }

            return ret;
        }

        private bool ValidateInternal(string id, bool throwOnIdCheck) {
            return CheckIdLength(id, throwOnIdCheck) && Validate(id);
        }

        public virtual bool Validate(string id) {
            return SessionId.IsLegit(id);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual String Encode(String id) {
            // Need to do UrlEncode if the session id could be custom created.
            if (_isInherited) {
                Debug.Trace("SessionIDManager", "Encode is doing UrlEncode ");
                return HttpUtility.UrlEncode(id);
            }
            else {
                Debug.Trace("SessionIDManager", "Encode is doing nothing ");
                return id;
            }
        }

        public virtual String Decode(String id) {
            // Need to do UrlDecode if the session id could be custom created.
            if (_isInherited) {
                Debug.Trace("SessionIDManager", "Decode is doing UrlDecode ");
                return HttpUtility.UrlDecode(id);
            }
            else {
                Debug.Trace("SessionIDManager", "Decode is doing nothing");
                return id.ToLower(CultureInfo.InvariantCulture);
            }
        }

        internal bool UseCookieless(HttpContext context) {
            Debug.Assert(context.Items[ASP_SESSIONID_MANAGER_INITIALIZEREQUEST_CALLED_KEY] != null);
            
            if (Config.Cookieless == HttpCookieMode.UseCookies) {
                return false;
            }
            else {
                object o = context.Items[COOKIELESS_BOOL_SESSION_KEY];

                Debug.Assert(o != null, "GetCookielessSessionID should be run already");
                
                return (bool)o;
            }
        }

        void CheckInitializeRequestCalled(HttpContext context) {
            if (context.Items[ASP_SESSIONID_MANAGER_INITIALIZEREQUEST_CALLED_KEY] == null) {
                throw new HttpException(SR.GetString(SR.SessionIDManager_InitializeRequest_not_called));
            }
        }

        public bool InitializeRequest(HttpContext context, bool suppressAutoDetectRedirect, out bool supportSessionIDReissue) {
            // Note: We support cookie reissue only if we're using cookieless. VSWhidbey 384892
            
            if (context.Items[ASP_SESSIONID_MANAGER_INITIALIZEREQUEST_CALLED_KEY] != null) {
                supportSessionIDReissue = UseCookieless(context);
                return false;
            }
            
            context.Items[ASP_SESSIONID_MANAGER_INITIALIZEREQUEST_CALLED_KEY] = true;
                
            if (Config.Cookieless == HttpCookieMode.UseCookies) {
                supportSessionIDReissue = false;   
                return false;
            }
            else {
                bool    cookieless;
                GetCookielessSessionID(context, !suppressAutoDetectRedirect, out cookieless);

                supportSessionIDReissue = cookieless;       
                return context.Response.IsRequestBeingRedirected;
            }
        }

        // Get the session id from Context or Cookies.
        // Called by session state module in AcquireState event.
        public String GetSessionID(HttpContext context) {
            String      s = null;
            HttpCookie  cookie;

            CheckInitializeRequestCalled(context);

            if (UseCookieless(context)) {
                s = (String) context.Items[COOKIELESS_SESSION_KEY];
            }
            else {
                cookie = context.Request.Cookies[Config.CookieName];
                if (cookie != null && cookie.Value != null) {
                    s = Decode((String)cookie.Value);
                    if (s != null && !ValidateInternal(s, false)) {
                        s = null;
                    }
                }
            }

            return s;
        }

        // Create a session id.
        virtual public String CreateSessionID(HttpContext context) {
            return SessionId.Create(ref _randgen);
        }

        // Save the session id to either URL or cookies.
        // For URL case, the param "redirected" will be set
        // to true, and we've called HttpApplication.CompleteRequest().
        // The caller should return. Called by session state module at the end of AcquireState event
        // and if a session state is successfully retrieved.
        public void SaveSessionID(HttpContext context, String id, out bool redirected,
                                        out bool cookieAdded)
        {
            HttpCookie          cookie;
            String              idEncoded;

            redirected = false;
            cookieAdded = false;

            CheckInitializeRequestCalled(context);

            if (context.Response.HeadersWritten) {
                // We support setting the session ID in a cookie or by redirecting to a munged URL.
                // Both techniques require that response headers haven't yet been flushed.
                throw new HttpException(
                    SR.GetString(SR.Cant_save_session_id_because_response_was_flushed));
            }

            if (!ValidateInternal(id, true)) {
                // VSWhidbey 439376
                throw new HttpException(
                    SR.GetString(SR.Cant_save_session_id_because_id_is_invalid, id));
            }

            idEncoded = Encode(id);

            if (!UseCookieless(context)) {
                /*
                 * Set the cookie.
                 */
                Debug.Trace("SessionIDManager",
                            "Creating session cookie, id=" + id + ", idEncoded=" + idEncoded);

                cookie = CreateSessionCookie(idEncoded);
                context.Response.Cookies.Add(cookie);
                cookieAdded = true;
            }
            else {
                context.CookielessHelper.SetCookieValue('S', idEncoded);

                /*
                 * Redirect.
                 */
                HttpRequest request = context.Request;
                string path = request.Path;
                string qs = request.QueryStringText;
                if (!String.IsNullOrEmpty(qs)) {
                    path = path + "?" + qs;
                }

                Debug.Trace("SessionIDManager",
                            "Redirecting to create cookieless session, path=" + path + ", idEncoded=" + idEncoded +
                            "\nReturning from SessionIDManager::SaveSessionID");

                context.Response.Redirect(path, false);
                context.ApplicationInstance.CompleteRequest();

                // Caller has to return immediately.
                redirected = true;
            }

            return;
        }

        // If cookie-ful, remove the session id from cookie.
        // Called by session state module in the ReleaseState event if a new
        // session was created but was unused

        // If cookieless, we can't do anything.
        public void RemoveSessionID(HttpContext context) {
            Debug.Trace("SessionIDManager", "Removing session id cookie");
            context.Response.Cookies.RemoveCookie(Config.CookieName);
        }
    }

    /*
     * Provides and verifies the integrity of a session id.
     *
     * A session id is a logically 120 bit random number,
     * represented in a string of 20 characters from a
     * size 64 character set. The session id can be placed
     * in a url without url-encoding.
     */
    internal static class SessionId {
        internal const int  NUM_CHARS_IN_ENCODING = 32;
        internal const int  ENCODING_BITS_PER_CHAR = 5;
        internal const int  ID_LENGTH_BITS  = 120;
        internal const int  ID_LENGTH_BYTES = (ID_LENGTH_BITS / 8 );                        // 15
        internal const int  ID_LENGTH_CHARS = (ID_LENGTH_BITS / ENCODING_BITS_PER_CHAR);    // 24

        static char[] s_encoding = new char[NUM_CHARS_IN_ENCODING]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            '0', '1', '2', '3', '4', '5'
        };

        static bool[] s_legalchars;

        static SessionId() {
            int     i;
            char    ch;

            s_legalchars = new bool[128];
            for (i = s_encoding.Length - 1; i >= 0; i--) {
                ch = s_encoding[i];
                s_legalchars[ch] = true;
            }
        }

        internal static bool IsLegit(String s) {
            int     i;
            char    ch;

            if (s == null || s.Length != ID_LENGTH_CHARS)
                return false;

            try {
                i = ID_LENGTH_CHARS;
                while (--i >= 0) {
                    ch = s[i];
                    if (!s_legalchars[ch])
                        return false;
                }

                return true;
            }
            catch (IndexOutOfRangeException) {
                return false;
            }
        }

        static String Encode(byte[] buffer) {
            int     i, j, k, n;
            char[]  chars = new char[ID_LENGTH_CHARS];

            Debug.Assert(buffer.Length == ID_LENGTH_BYTES);

            j = 0;
            for (i = 0; i < ID_LENGTH_BYTES; i += 5) {
                n =  (int) buffer[i] |
                     ((int) buffer[i+1] << 8)  |
                     ((int) buffer[i+2] << 16) |
                     ((int) buffer[i+3] << 24);

                k = (n & 0x0000001F);
                chars[j++] = s_encoding[k];

                k = ((n >> 5) & 0x0000001F);
                chars[j++] = s_encoding[k];

                k = ((n >> 10) & 0x0000001F);
                chars[j++] = s_encoding[k];

                k = ((n >> 15) & 0x0000001F);
                chars[j++] = s_encoding[k];

                k = ((n >> 20) & 0x0000001F);
                chars[j++] = s_encoding[k];

                k = ((n >> 25) & 0x0000001F);
                chars[j++] = s_encoding[k];

                n = ((n >> 30) & 0x00000003) | ((int)buffer[i + 4] << 2);

                k = (n & 0x0000001F);
                chars[j++] = s_encoding[k];

                k = ((n >> 5) & 0x0000001F);
                chars[j++] = s_encoding[k];
            }

            Debug.Assert(j == ID_LENGTH_CHARS);

            return new String(chars);
        }

        static internal String Create(ref RandomNumberGenerator randgen) {
            byte[]  buffer;
            String  encoding;

            if (randgen == null) {
                randgen = new RNGCryptoServiceProvider();
            }

            buffer = new byte [15];
            randgen.GetBytes(buffer);
            encoding = Encode(buffer);
            return encoding;
        }
    }

}
