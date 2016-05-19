//------------------------------------------------------------------------------
// <copyright file="AnonymousIdentificationModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * AnonymousIdentificationModule class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System.Web;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Caching;
    using System.Web.Handlers;
    using System.Collections;
    using System.Configuration.Provider;
    using System.Web.Util;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Globalization;
    using System.Web.Management;
    using System.Web.Hosting;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Web.Security.Cryptography;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class AnonymousIdentificationModule : IHttpModule {

        private const int MAX_ENCODED_COOKIE_STRING = 512;
        private const int MAX_ID_LENGTH             = 128;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.Security.AnonymousIdentificationModule'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public AnonymousIdentificationModule() {
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Events
        private AnonymousIdentificationEventHandler _CreateNewIdEventHandler;

        public event AnonymousIdentificationEventHandler Creating {
            add    {  _CreateNewIdEventHandler += value; }
            remove {  _CreateNewIdEventHandler -= value; }
        }

        public static void ClearAnonymousIdentifier()
        {
            if (!s_Initialized)
                Initialize();

            HttpContext context = HttpContext.Current;
            if (context == null)
                return;

            // VSWhidbey 418835: When this feature is enabled, prevent infinite loop when cookieless
            // mode != Cookies and there was no cookie, also we cannot clear when current user is anonymous.
            if (!s_Enabled || !context.Request.IsAuthenticated) {
                throw new NotSupportedException(SR.GetString(SR.Anonymous_ClearAnonymousIdentifierNotSupported));
            }

            ////////////////////////////////////////////////////////////
            // Check if we need to clear the ticket stored in the URI
            bool clearUri = false;
            if (context.CookielessHelper.GetCookieValue('A') != null) {
                context.CookielessHelper.SetCookieValue('A', null); // Always clear the uri-cookie
                clearUri = true;
            }

            ////////////////////////////////////////////////////////////
            // Clear cookie if cookies are supported by the browser
            if (!CookielessHelperClass.UseCookieless(context, false, s_CookieMode) || context.Request.Browser.Cookies)
            { // clear cookie if required
                string cookieValue = String.Empty;
                if (context.Request.Browser["supportsEmptyStringInCookieValue"] == "false")
                    cookieValue = "NoCookie";
                HttpCookie cookie = new HttpCookie(s_CookieName, cookieValue);
                cookie.HttpOnly = true;
                cookie.Path = s_CookiePath;
                cookie.Secure = s_RequireSSL;
                if (s_Domain != null)
                    cookie.Domain = s_Domain;
                cookie.Expires = new System.DateTime(1999, 10, 12);
                context.Response.Cookies.RemoveCookie(s_CookieName);
                context.Response.Cookies.Add(cookie);
            }

            ////////////////////////////////////////////////////////////
            // Redirect back to this page if we removed a URI ticket
            if (clearUri) {
                context.Response.Redirect(context.Request.RawUrl, false);
            }
        }
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Public functions

        public void Dispose() {  }

        public void Init(HttpApplication app) {
            // for IIS 7, skip event wireup altogether if this feature isn't
            // enabled
            if (!s_Initialized) {
                Initialize();
            }
            if (s_Enabled) {
                app.PostAuthenticateRequest += new EventHandler(this.OnEnter);
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void OnEnter(Object source, EventArgs eventArgs) {
            if (!s_Initialized)
                Initialize();

            if (!s_Enabled)
                return;

            HttpApplication  app;
            HttpContext      context;
            HttpCookie       cookie = null;
            bool             createCookie = false;
            AnonymousIdData  decodedValue = null;
            bool             cookieLess;
            string           encValue = null;
            bool             isAuthenticated = false;

            app = (HttpApplication)source;
            context = app.Context;

            isAuthenticated = context.Request.IsAuthenticated;

            if (isAuthenticated) {
                cookieLess = CookielessHelperClass.UseCookieless(context, false /* no redirect */, s_CookieMode);
            } else {
                cookieLess = CookielessHelperClass.UseCookieless(context, true /* do redirect */, s_CookieMode);
                //if (!cookieLess && s_RequireSSL && !context.Request.IsSecureConnection)
                //    throw new HttpException(SR.GetString(SR.Connection_not_secure_creating_secure_cookie));
            }

            ////////////////////////////////////////////////////////////////////////
            // Handle secure-cookies over non SSL
            if (s_RequireSSL && !context.Request.IsSecureConnection)
            {
                if (!cookieLess)
                {
                    cookie = context.Request.Cookies[s_CookieName];
                    if (cookie != null)
                    {
                        cookie = new HttpCookie(s_CookieName, String.Empty);
                        cookie.HttpOnly = true;
                        cookie.Path = s_CookiePath;
                        cookie.Secure = s_RequireSSL;
                        if (s_Domain != null)
                            cookie.Domain = s_Domain;
                        cookie.Expires = new System.DateTime(1999, 10, 12);

                        if (context.Request.Browser["supportsEmptyStringInCookieValue"] == "false")
                            cookie.Value = "NoCookie";
                        context.Response.Cookies.Add(cookie);
                    }

                    return;
                }
            }


            ////////////////////////////////////////////////////////////
            // Step 2: See if cookie, or cookie-header has the value
            if (!cookieLess)
            {
                cookie = context.Request.Cookies[s_CookieName];
                if (cookie != null)
                {
                    encValue = cookie.Value;
                    cookie.Path = s_CookiePath;
                    if (s_Domain != null)
                        cookie.Domain = s_Domain;
                }
            }
            else
            {
                encValue = context.CookielessHelper.GetCookieValue('A');
            }

            decodedValue = GetDecodedValue(encValue);

            if (decodedValue != null && decodedValue.AnonymousId != null) {
                // Copy existing value in Request
                context.Request.AnonymousID = decodedValue.AnonymousId;
            }
            if (isAuthenticated) // For the authenticated case, we are done
                return;

            if (context.Request.AnonymousID == null) {
                ////////////////////////////////////////////////////////////
                // Step 3: Create new Identity

                // Raise event
                if (_CreateNewIdEventHandler != null) {
                    AnonymousIdentificationEventArgs e = new AnonymousIdentificationEventArgs(context);
                    _CreateNewIdEventHandler(this, e);
                    context.Request.AnonymousID = e.AnonymousID;
                }

                // Create from GUID
                if (string.IsNullOrEmpty(context.Request.AnonymousID)) {
                    context.Request.AnonymousID = Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture);
                } else {
                    if (context.Request.AnonymousID.Length > MAX_ID_LENGTH)
                        throw new HttpException(SR.GetString(SR.Anonymous_id_too_long));
                }
                if (s_RequireSSL && !context.Request.IsSecureConnection && !cookieLess)
                    return; // Don't create secure-cookie in un-secured connection

                createCookie = true;
            }

            ////////////////////////////////////////////////////////////
            // Step 4: Check if cookie has to be created
            DateTime dtNow = DateTime.UtcNow;
            if (!createCookie) {
                if (s_SlidingExpiration) {
                    if (decodedValue == null || decodedValue.ExpireDate < dtNow) {
                        createCookie = true;
                    } else {
                        double secondsLeft =  (decodedValue.ExpireDate - dtNow).TotalSeconds;
                        if (secondsLeft < (double) ((s_CookieTimeout*60)/2)) {
                            createCookie = true;
                        }
                    }
                }
            }

            ////////////////////////////////////////////////////////////
            // Step 4: Create new cookie or cookieless header
            if (createCookie) {
                DateTime dtExpireTime = dtNow.AddMinutes(s_CookieTimeout);
                encValue = GetEncodedValue(new AnonymousIdData(context.Request.AnonymousID, dtExpireTime));
                if (encValue.Length > MAX_ENCODED_COOKIE_STRING)
                    throw new HttpException(SR.GetString(SR.Anonymous_id_too_long_2));

                if (!cookieLess) {
                    cookie          = new HttpCookie(s_CookieName, encValue);
                    cookie.HttpOnly = true;
                    cookie.Expires  = dtExpireTime;
                    cookie.Path     = s_CookiePath;
                    cookie.Secure   = s_RequireSSL;
                    if (s_Domain != null)
                        cookie.Domain   = s_Domain;
                    context.Response.Cookies.Add(cookie);
                } else {
                    context.CookielessHelper.SetCookieValue('A', encValue);
                    context.Response.Redirect(context.Request.RawUrl);
                }
            }
        }

        public static bool Enabled {
            get {
                if (!s_Initialized) {
                    Initialize();
                }
                return s_Enabled;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Static config settings
        private static bool       s_Initialized         = false;
        private static bool       s_Enabled             = false;
        private static string     s_CookieName          = ".ASPXANONYMOUS";
        private static string     s_CookiePath          = "/";
        private static int        s_CookieTimeout       = 100000;
        private static bool       s_RequireSSL          = false;
        private static string     s_Domain              = null;
        private static bool       s_SlidingExpiration   = true;
        private static byte []    s_Modifier            = null;
        private static object     s_InitLock            = new object();
        private static HttpCookieMode   s_CookieMode    = HttpCookieMode.UseDeviceProfile;
        private static CookieProtection s_Protection    = CookieProtection.None;

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Static functions
        private static void Initialize() {

            if (s_Initialized)
                return;

            lock(s_InitLock) {
                if (s_Initialized)
                    return;

                AnonymousIdentificationSection settings = RuntimeConfig.GetAppConfig().AnonymousIdentification;
                s_Enabled            = settings.Enabled;
                s_CookieName         = settings.CookieName;
                s_CookiePath         = settings.CookiePath;
                s_CookieTimeout      = (int) settings.CookieTimeout.TotalMinutes;
                s_RequireSSL         = settings.CookieRequireSSL;
                s_SlidingExpiration  = settings.CookieSlidingExpiration;
                s_Protection         = settings.CookieProtection;
                s_CookieMode         = settings.Cookieless;
                s_Domain             = settings.Domain;

                s_Modifier = Encoding.UTF8.GetBytes("AnonymousIdentification");
                if (s_CookieTimeout < 1)
                    s_CookieTimeout = 1;
                if (s_CookieTimeout > 60 * 24 * 365 * 2)
                    s_CookieTimeout = 60 * 24 * 365 * 2; // 2 years
                s_Initialized = true;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private static string GetEncodedValue(AnonymousIdData data){
            if (data == null)
                return null;
            byte [] bufId       = Encoding.UTF8.GetBytes(data.AnonymousId);
            byte [] bufIdLen    = BitConverter.GetBytes(bufId.Length);
            byte [] bufDate     = BitConverter.GetBytes(data.ExpireDate.ToFileTimeUtc());
            byte [] buffer      = new byte[12 + bufId.Length];

            Buffer.BlockCopy(bufDate,   0, buffer, 0,  8);
            Buffer.BlockCopy(bufIdLen,  0, buffer, 8,  4);
            Buffer.BlockCopy(bufId,     0, buffer, 12, bufId.Length);
            return CookieProtectionHelper.Encode(s_Protection, buffer, Purpose.AnonymousIdentificationModule_Ticket);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private static AnonymousIdData GetDecodedValue(string data){
            if (data == null || data.Length < 1 || data.Length > MAX_ENCODED_COOKIE_STRING)
                return null;

            try {
                byte [] bBlob = CookieProtectionHelper.Decode(s_Protection, data, Purpose.AnonymousIdentificationModule_Ticket);
                if (bBlob == null || bBlob.Length < 13)
                    return null;
                DateTime expireDate = DateTime.FromFileTimeUtc(BitConverter.ToInt64(bBlob, 0));
                if (expireDate < DateTime.UtcNow)
                    return null;
                int len = BitConverter.ToInt32(bBlob, 8);
                if (len < 0 || len > bBlob.Length - 12)
                    return null;
                string id = Encoding.UTF8.GetString(bBlob, 12, len);
                if (id.Length > MAX_ID_LENGTH)
                    return null;
                return new AnonymousIdData(id, expireDate);
            }
            catch {}
            return null;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
    }

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    [Serializable]
    internal class AnonymousIdData
    {
        internal AnonymousIdData(string id, DateTime dt) {
            ExpireDate = dt;
            AnonymousId = (dt > DateTime.UtcNow) ? id : null;  // Ignore expired data
        }

        internal string     AnonymousId;
        internal DateTime   ExpireDate;
    }

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////

    public delegate void AnonymousIdentificationEventHandler(object sender, AnonymousIdentificationEventArgs e);

    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////
    /////////////////////////////////////////////////////////////////////////////

    public sealed class AnonymousIdentificationEventArgs : EventArgs {

        public string         AnonymousID { get { return _AnonymousId;} set { _AnonymousId = value;}}

        public HttpContext    Context     { get { return _Context; }}

        private string        _AnonymousId;
        private HttpContext   _Context;


        public AnonymousIdentificationEventArgs(HttpContext context) {
            _Context = context;
        }
    }

}


