//------------------------------------------------------------------------------
// <copyright file="FormsAuthentication.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * FormsAuthentication class
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
    using System.Web.Management;
    using System.Collections.Specialized;
    using System.Web.Compilation;
    using System.Web.Security.Cryptography;



    /// <devdoc>
    ///    This class consists of static methods that
    ///    provides helper utilities for manipulating authentication tickets.
    /// </devdoc>
    public sealed class FormsAuthentication {
        private const int MAX_TICKET_LENGTH = 4096;
        private static object _lockObject = new object();

        public FormsAuthentication() { }

		/////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Helper functions: Hash a password

        /// <devdoc>
        ///    Initializes FormsAuthentication by reading
        ///    configuration and getting the cookie values and encryption keys for the given
        ///    application.
        /// </devdoc>
        [Obsolete("The recommended alternative is to use the Membership APIs, such as Membership.CreateUser. For more information, see http://go.microsoft.com/fwlink/?LinkId=252463.")]
        public static String HashPasswordForStoringInConfigFile(String password, String passwordFormat) {
            if (password == null) {
                throw new ArgumentNullException("password");
            }
            if (passwordFormat == null) {
                throw new ArgumentNullException("passwordFormat");
            }
            HashAlgorithm hashAlgorithm;
            if (StringUtil.EqualsIgnoreCase(passwordFormat, "sha1"))
                hashAlgorithm = CryptoAlgorithms.CreateSHA1();
            else if (StringUtil.EqualsIgnoreCase(passwordFormat, "md5"))
                hashAlgorithm = CryptoAlgorithms.CreateMD5();
            else
                throw new ArgumentException(SR.GetString(SR.InvalidArgumentValue, "passwordFormat"));

            using (hashAlgorithm) {
                return CryptoUtil.BinaryToHex(hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Initialize this

        /// <devdoc>
        ///    Initializes FormsAuthentication by reading
        ///    configuration and getting the cookie values and encryption keys for the given
        ///    application.
        /// </devdoc>
        public static void Initialize() {
            if (_Initialized)
                return;

            lock(_lockObject) {
                if (_Initialized)
                    return;

                AuthenticationSection settings = RuntimeConfig.GetAppConfig().Authentication;
                settings.ValidateAuthenticationMode();
                _FormsName = settings.Forms.Name;
                _RequireSSL = settings.Forms.RequireSSL;
                _SlidingExpiration = settings.Forms.SlidingExpiration;
                if (_FormsName == null)
                    _FormsName = CONFIG_DEFAULT_COOKIE;

                _Protection = settings.Forms.Protection;
                _Timeout = (int) settings.Forms.Timeout.TotalMinutes;
                _FormsCookiePath = settings.Forms.Path;
                _LoginUrl = settings.Forms.LoginUrl;
                if (_LoginUrl == null)
                    _LoginUrl = "login.aspx";
                _DefaultUrl = settings.Forms.DefaultUrl;
                if (_DefaultUrl == null)
                    _DefaultUrl = "default.aspx";
                _CookieMode = settings.Forms.Cookieless;
                _CookieDomain = settings.Forms.Domain;
                _EnableCrossAppRedirects = settings.Forms.EnableCrossAppRedirects;
                _TicketCompatibilityMode = settings.Forms.TicketCompatibilityMode;

                _Initialized = true;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Decrypt and get the auth ticket

        /// <devdoc>
        ///    <para>Given an encrypted authenitcation ticket as
        ///       obtained from an HTTP cookie, this method returns an instance of a
        ///       FormsAuthenticationTicket class.</para>
        /// </devdoc>
        public static FormsAuthenticationTicket Decrypt(string encryptedTicket) {
            if (String.IsNullOrEmpty(encryptedTicket) || encryptedTicket.Length > MAX_TICKET_LENGTH)
                throw new ArgumentException(SR.GetString(SR.InvalidArgumentValue, "encryptedTicket"));

            Initialize();
            byte[] bBlob = null;
            if ((encryptedTicket.Length % 2) == 0) { // Could be a hex string
                try {
                    bBlob = CryptoUtil.HexToBinary(encryptedTicket);
                } catch { }
            }
            if (bBlob == null)
                bBlob = HttpServerUtility.UrlTokenDecode(encryptedTicket);
            if (bBlob == null || bBlob.Length < 1)
                throw new ArgumentException(SR.GetString(SR.InvalidArgumentValue, "encryptedTicket"));

            int ticketLength;

            if (AspNetCryptoServiceProvider.Instance.IsDefaultProvider) {
                // If new crypto routines are enabled, call them instead.
                ICryptoService cryptoService = AspNetCryptoServiceProvider.Instance.GetCryptoService(Purpose.FormsAuthentication_Ticket);
                byte[] unprotectedData = cryptoService.Unprotect(bBlob);
                ticketLength = unprotectedData.Length;
                bBlob = unprotectedData;
            } else {
#pragma warning disable 618 // calling obsolete methods
                // Otherwise call into MachineKeySection routines.

                if (_Protection == FormsProtectionEnum.All || _Protection == FormsProtectionEnum.Encryption)
                {
                    // DevDiv Bugs 137864: Include a random IV if under the right compat mode
                    // for improved encryption semantics
                    bBlob = MachineKeySection.EncryptOrDecryptData(false, bBlob, null, 0, bBlob.Length, false, false, IVType.Random);
                    if (bBlob == null)
                        return null;
                }

                ticketLength = bBlob.Length;

                if (_Protection == FormsProtectionEnum.All || _Protection == FormsProtectionEnum.Validation)
                {
                    if (!MachineKeySection.VerifyHashedData(bBlob))
                        return null;
                    ticketLength -= MachineKeySection.HashSize;
                }
#pragma warning restore 618 // calling obsolete methods
            }

            //////////////////////////////////////////////////////////////////////
            // Step 4: Change binary ticket to managed struct

            // ** MSRC 11838 **
            // Framework20 / Framework40 ticket generation modes are insecure. We should use a
            // secure serialization mode by default.
            if (!AppSettings.UseLegacyFormsAuthenticationTicketCompatibility) {
                return FormsAuthenticationTicketSerializer.Deserialize(bBlob, ticketLength);
            }

            // ** MSRC 11838 **
            // If we have reached this point of execution, the developer has explicitly elected
            // to continue using the insecure code path instead of the secure one. We removed
            // the Framework40 serialization mode, so everybody using the legacy code path is
            // forced to Framework20.

            int iSize = ((ticketLength > MAX_TICKET_LENGTH) ? MAX_TICKET_LENGTH : ticketLength);
            StringBuilder     name = new StringBuilder(iSize);
            StringBuilder     data = new StringBuilder(iSize);
            StringBuilder     path = new StringBuilder(iSize);
            byte []           pBin = new byte[4];
            long []           pDates = new long[2];

            int iRet = UnsafeNativeMethods.CookieAuthParseTicket(bBlob, ticketLength,
                                                                   name, iSize,
                                                                   data, iSize,
                                                                   path, iSize,
                                                                   pBin, pDates);

            if (iRet != 0)
                return null;

            DateTime dt1 = DateTime.FromFileTime(pDates[0]);
            DateTime dt2 = DateTime.FromFileTime(pDates[1]);

            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket((int) pBin[0],
                                                     name.ToString(),
                                                     dt1,
                                                     dt2,
                                                     (bool) (pBin[1] != 0),
                                                     data.ToString(),
                                                     path.ToString());
            return ticket;
        }


        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Encrypt a ticket

        /// <devdoc>
        ///    Given a FormsAuthenticationTicket, this
        ///    method produces a string containing an encrypted authentication ticket suitable
        ///    for use in an HTTP cookie.
        /// </devdoc>
        public static String Encrypt(FormsAuthenticationTicket ticket) {
            return Encrypt(ticket, true);
        }
        internal static String Encrypt(FormsAuthenticationTicket ticket, bool hexEncodedTicket) {
            if (ticket == null)
                throw new ArgumentNullException("ticket");

            Initialize();
            //////////////////////////////////////////////////////////////////////
            // Step 1a: Make it into a binary blob
            byte[] bBlob = MakeTicketIntoBinaryBlob(ticket);
            if (bBlob == null)
                return null;

            //////////////////////////////////////////////////////////////////////
            // Step 1b: If new crypto routines are enabled, call them instead.
            if (AspNetCryptoServiceProvider.Instance.IsDefaultProvider) {
                ICryptoService cryptoService = AspNetCryptoServiceProvider.Instance.GetCryptoService(Purpose.FormsAuthentication_Ticket);
                byte[] protectedData = cryptoService.Protect(bBlob);
                bBlob = protectedData;
            }
            else {
#pragma warning disable 618 // calling obsolete methods
                // otherwise..

                //////////////////////////////////////////////////////////////////////
                // Step 2: Get the MAC and add to the blob
                if (_Protection == FormsProtectionEnum.All || _Protection == FormsProtectionEnum.Validation) {
                    byte[] bMac = MachineKeySection.HashData(bBlob, null, 0, bBlob.Length);
                    if (bMac == null)
                        return null;
                    byte[] bAll = new byte[bMac.Length + bBlob.Length];
                    Buffer.BlockCopy(bBlob, 0, bAll, 0, bBlob.Length);
                    Buffer.BlockCopy(bMac, 0, bAll, bBlob.Length, bMac.Length);
                    bBlob = bAll;
                }

                if (_Protection == FormsProtectionEnum.All || _Protection == FormsProtectionEnum.Encryption) {
                    //////////////////////////////////////////////////////////////////////
                    // Step 3: Do the actual encryption
                    // DevDiv Bugs 137864: Include a random IV if under the right compat mode
                    // for improved encryption semantics
                    bBlob = MachineKeySection.EncryptOrDecryptData(true, bBlob, null, 0, bBlob.Length, false, false, IVType.Random);
                }
#pragma warning restore 618 // calling obsolete methods
            }

            if (!hexEncodedTicket)
                return HttpServerUtility.UrlTokenEncode(bBlob);
            else
                return CryptoUtil.BinaryToHex(bBlob);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Verify User name and Password

        /// <devdoc>
        ///    Given the supplied credentials, this method
        ///    attempts to validate the credentials against those contained in the configured
        ///    credential store.
        /// </devdoc>
        [Obsolete("The recommended alternative is to use the Membership APIs, such as Membership.ValidateUser. For more information, see http://go.microsoft.com/fwlink/?LinkId=252463.")]
        public static bool Authenticate(String name, String password) {
            bool retVal = InternalAuthenticate(name, password);

            if (retVal) {
                PerfCounters.IncrementCounter(AppPerfCounter.FORMS_AUTH_SUCCESS);
                WebBaseEvent.RaiseSystemEvent(null, WebEventCodes.AuditFormsAuthenticationSuccess, name);
            }
            else {
                PerfCounters.IncrementCounter(AppPerfCounter.FORMS_AUTH_FAIL);
                WebBaseEvent.RaiseSystemEvent(null, WebEventCodes.AuditFormsAuthenticationFailure, name);
            }

            return retVal;
        }

        private static bool InternalAuthenticate(String name, String password) {
            //////////////////////////////////////////////////////////////////////
            // Step 1: Make sure we are initialized
            if (name == null || password == null)
                return false;

            Initialize();
            //////////////////////////////////////////////////////////////////////
            // Step 2: Get the user database
            AuthenticationSection settings = RuntimeConfig.GetAppConfig().Authentication;
            settings.ValidateAuthenticationMode();
            FormsAuthenticationUserCollection Users = settings.Forms.Credentials.Users;

//            Hashtable hTable = settings.Credentials;

            if (Users == null) {
                return false;
            }

            //////////////////////////////////////////////////////////////////////
            // Step 3: Get the (hashed) password for this user
            FormsAuthenticationUser u = Users[name.ToLower(CultureInfo.InvariantCulture)];
            if (u == null)
                return false;

            String pass = (String)u.Password;

            if (pass == null) {
                return false;
            }

            //////////////////////////////////////////////////////////////////////
            // Step 4: Hash the given password
            String   encPassword;

#pragma warning disable 618 // HashPasswordForStorignInConfigFile is now obsolete
            switch (settings.Forms.Credentials.PasswordFormat)
            {
                case FormsAuthPasswordFormat.SHA1:
                    encPassword = HashPasswordForStoringInConfigFile(password, "sha1");
                    break;

                case FormsAuthPasswordFormat.MD5:
                    encPassword = HashPasswordForStoringInConfigFile(password, "md5");
                    break;

                case FormsAuthPasswordFormat.Clear:
                    encPassword = password;
                    break;

                default:
                    return false;
            }
#pragma warning restore 618

            //////////////////////////////////////////////////////////////////////
            // Step 5: Compare the hashes
            return(String.Compare(encPassword,
                                  pass,
                                  ((settings.Forms.Credentials.PasswordFormat != FormsAuthPasswordFormat.Clear)
                                        ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                   == 0);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    Given an authenticated user, calling SignOut
        ///    removes the authentication ticket by doing a SetForms with an empty value. This
        ///    removes either durable or session cookies.
        /// </devdoc>
        public static void SignOut() {
            Initialize();

            HttpContext    context    = HttpContext.Current;
            bool           needToRedirect  = context.CookielessHelper.DoesCookieValueExistInOriginal('F');

            context.CookielessHelper.SetCookieValue('F', null); // Always clear the uri-cookie

            if (!CookielessHelperClass.UseCookieless(context, false, CookieMode) || context.Request.Browser.Cookies)
            { // clear cookie if required
                string cookieValue = String.Empty;
                if (context.Request.Browser["supportsEmptyStringInCookieValue"] == "false")
                    cookieValue = "NoCookie";
                HttpCookie cookie = new HttpCookie(FormsCookieName, cookieValue);
                cookie.HttpOnly = true;
                cookie.Path = _FormsCookiePath;
                cookie.Expires = new System.DateTime(1999, 10, 12);
                cookie.Secure = _RequireSSL;
                if (_CookieDomain != null)
                    cookie.Domain = _CookieDomain;
                context.Response.Cookies.RemoveCookie(FormsCookieName);
                context.Response.Cookies.Add(cookie);
            }
            if (needToRedirect)
                context.Response.Redirect(GetLoginPage(null), false);
        }
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    This method creates an authentication ticket
        ///    for the given userName and attaches it to the cookies collection of the outgoing
        ///    response. It does not perform a redirect.
        /// </devdoc>
        public static void SetAuthCookie(String userName, bool createPersistentCookie) {
            Initialize();
            SetAuthCookie(userName, createPersistentCookie, FormsAuthentication.FormsCookiePath);
        }

        /// <devdoc>
        ///    This method creates an authentication ticket
        ///    for the given userName and attaches it to the cookies collection of the outgoing
        ///    response. It does not perform a redirect.
        /// </devdoc>
        public static void SetAuthCookie(String userName, bool createPersistentCookie, String strCookiePath) {
            Initialize();
            HttpContext context = HttpContext.Current;

            if (!context.Request.IsSecureConnection && RequireSSL)
                throw new HttpException(SR.GetString(SR.Connection_not_secure_creating_secure_cookie));
            bool        cookieless  = CookielessHelperClass.UseCookieless(context, false, CookieMode);
            HttpCookie  cookie      = GetAuthCookie(userName, createPersistentCookie, cookieless ? "/" : strCookiePath, !cookieless);

            if (!cookieless) {
                HttpContext.Current.Response.Cookies.Add(cookie);
                context.CookielessHelper.SetCookieValue('F', null);
            }
            else {
                context.CookielessHelper.SetCookieValue('F', cookie.Value);
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    Creates an authentication cookie for a given
        ///    user name. This does not set the cookie as part of the outgoing response, so
        ///    that an application can have more control over how the cookie is issued.
        /// </devdoc>
        public static HttpCookie GetAuthCookie(String userName, bool createPersistentCookie) {
            Initialize();
            return GetAuthCookie(userName, createPersistentCookie, FormsAuthentication.FormsCookiePath);
        }

        public static HttpCookie GetAuthCookie(String userName, bool createPersistentCookie, String strCookiePath) {
            return GetAuthCookie(userName, createPersistentCookie, strCookiePath, true);
        }
        private static HttpCookie GetAuthCookie(String userName, bool createPersistentCookie, String strCookiePath, bool hexEncodedTicket) {
            Initialize();
            if (userName == null)
                userName = String.Empty;

            if (strCookiePath == null || strCookiePath.Length < 1)
                strCookiePath = FormsCookiePath;

            DateTime issueDateUtc = DateTime.UtcNow;
            DateTime expirationUtc = issueDateUtc.AddMinutes(_Timeout);

            FormsAuthenticationTicket ticket = FormsAuthenticationTicket.FromUtc(
                2, // version
                userName, // User-Name
                issueDateUtc, // Issue-Date
                expirationUtc, // Expiration
                createPersistentCookie, // IsPersistent
                String.Empty, // User-Data
                strCookiePath // Cookie Path
                );

            String strTicket = Encrypt(ticket, hexEncodedTicket);
            if (strTicket == null || strTicket.Length < 1)
                        throw new HttpException(
                                SR.GetString(SR.Unable_to_encrypt_cookie_ticket));


            HttpCookie cookie = new HttpCookie(FormsCookieName, strTicket);

            cookie.HttpOnly = true;
            cookie.Path = strCookiePath;
            cookie.Secure = _RequireSSL;
            if (_CookieDomain != null)
                cookie.Domain = _CookieDomain;
            if (ticket.IsPersistent)
                cookie.Expires = ticket.Expiration;
            return cookie;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        internal static String GetReturnUrl(bool useDefaultIfAbsent)
        {
            Initialize();

            HttpContext     context     = HttpContext.Current;
            String          returnUrl   = context.Request.QueryString[ReturnUrlVar];

            // If it is not in the QueryString, look in the Posted-body
            if (returnUrl == null) {
                returnUrl = context.Request.Form[ReturnUrlVar];
                if (!string.IsNullOrEmpty(returnUrl) && !returnUrl.Contains("/") && returnUrl.Contains("%"))
                    returnUrl = HttpUtility.UrlDecode(returnUrl);
            }

            // Make sure it is on the current server if EnableCrossAppRedirects is false
            if (!string.IsNullOrEmpty(returnUrl) && !EnableCrossAppRedirects) {
                if (!UrlPath.IsPathOnSameServer(returnUrl, context.Request.Url))
                    returnUrl = null;
            }

            // Make sure it is not dangerous, i.e. does not contain script, etc.
            if (!string.IsNullOrEmpty(returnUrl) && CrossSiteScriptingValidation.IsDangerousUrl(returnUrl))
                throw new HttpException(SR.GetString(SR.Invalid_redirect_return_url));

            return ((returnUrl == null && useDefaultIfAbsent) ? DefaultUrl : returnUrl);
        }

        /// <devdoc>
        ///    Returns the redirect URL for the original
        ///    request that caused the redirect to the login page.
        /// </devdoc>
        public static String GetRedirectUrl(String userName, bool createPersistentCookie)
        {
            if (userName == null)
                return null;
            return GetReturnUrl(true);
        }
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Redirect from logon page to orignal page

        /// <devdoc>
        ///    This method redirects an authenticated user
        ///    back to the original URL that they requested.
        /// </devdoc>
        public static void RedirectFromLoginPage(String userName, bool createPersistentCookie) {
            Initialize();
            RedirectFromLoginPage(userName, createPersistentCookie, FormsAuthentication.FormsCookiePath);
        }

        public static void RedirectFromLoginPage(String userName, bool createPersistentCookie, String strCookiePath) {
            Initialize();
            if (userName == null)
                return;

            HttpContext context = HttpContext.Current;
            string strUrl = GetReturnUrl(true);
            if (  CookiesSupported || // Cookies-supported: Most common scenario
                  IsPathWithinAppRoot(context, strUrl)) { // Cookies not suported, so add it to the current app URL

                SetAuthCookie(userName, createPersistentCookie, strCookiePath);
                strUrl = RemoveQueryStringVariableFromUrl(strUrl, FormsCookieName); // Make sure there is no other ticket in the Query String.
                if (!CookiesSupported) {// Make sure the URL is relative, if we are using cookieless.
                    int pos = strUrl.IndexOf("://", StringComparison.Ordinal);
                    if (pos > 0) {
                        pos = strUrl.IndexOf('/', pos + 3);
                        if (pos > 0)
                            strUrl = strUrl.Substring(pos);
                    }
                }
            } else if (EnableCrossAppRedirects) { // Cookieless scenario -- add it to the QueryString if allowed to

                HttpCookie cookie = GetAuthCookie(userName, createPersistentCookie, strCookiePath);
                strUrl = RemoveQueryStringVariableFromUrl(strUrl, cookie.Name); // Make sure there is no other ticket in the Query String.
                if (strUrl.IndexOf('?') > 0) {
                    strUrl += "&" + cookie.Name + "=" + cookie.Value;
                }
                else {
                    strUrl += "?" + cookie.Name + "=" + cookie.Value;
                }

            } else {
                // Broken scenario:
                throw new HttpException(SR.GetString(SR.Can_not_issue_cookie_or_redirect));
            }

            context.Response.Redirect(strUrl, false);
        }

        public static FormsAuthenticationTicket RenewTicketIfOld(FormsAuthenticationTicket tOld) {
            if (tOld == null)
                return null;

            DateTime utcNow = DateTime.UtcNow;
            TimeSpan ticketAge = utcNow - tOld.IssueDateUtc;
            TimeSpan ticketRemainingLifetime = tOld.ExpirationUtc - utcNow;

            if (ticketRemainingLifetime > ticketAge)
                return tOld; // no need to renew

            // The original ticket may have had a custom-specified lifetime separate from
            // the default timeout specified in config. We should honor that original
            // lifetime when renewing the ticket.
            TimeSpan originalTicketTotalLifetime = tOld.ExpirationUtc - tOld.IssueDateUtc;
            DateTime newExpirationUtc = utcNow + originalTicketTotalLifetime;

            FormsAuthenticationTicket ticket = FormsAuthenticationTicket.FromUtc(
                tOld.Version /* version */,
                tOld.Name /* name */,
                utcNow /* issueDateUtc */,
                newExpirationUtc /* expirationUtc */,
                tOld.IsPersistent /* isPersistent */,
                tOld.UserData /* userData */,
                tOld.CookiePath /* cookiePath */);

            return ticket;
        }

        public static void EnableFormsAuthentication(NameValueCollection configurationData) {
            BuildManager.ThrowIfPreAppStartNotRunning();
            configurationData = configurationData ?? new NameValueCollection();
            AuthenticationConfig.Mode = AuthenticationMode.Forms;
            Initialize();
            // Last caller overwrites only the values that are present in the dictionary.
            string defaultUrl = configurationData["defaultUrl"];
            if (!String.IsNullOrEmpty(defaultUrl)) {
                _DefaultUrl = defaultUrl;
            }
            string loginUrl = configurationData["loginUrl"];
            if (!String.IsNullOrEmpty(loginUrl)) {
                _LoginUrl = loginUrl;
            }
        }

        public static bool IsEnabled {
            get {
                return AuthenticationConfig.Mode == AuthenticationMode.Forms;
            }
        }

        public static String FormsCookieName { get { Initialize(); return _FormsName; }}

        public static String FormsCookiePath { get { Initialize(); return _FormsCookiePath; }}

        public static bool   RequireSSL { get { Initialize(); return _RequireSSL; }}

        public static TimeSpan Timeout { get { Initialize(); return new TimeSpan(0, _Timeout, 0); } }

        public static bool   SlidingExpiration { get { Initialize(); return _SlidingExpiration; }}

        public static HttpCookieMode CookieMode { get { Initialize(); return _CookieMode; }}

        public static string CookieDomain { get { Initialize ();return _CookieDomain; } }

        public static bool EnableCrossAppRedirects { get { Initialize(); return _EnableCrossAppRedirects; } }

        public static TicketCompatibilityMode TicketCompatibilityMode { get { Initialize(); return _TicketCompatibilityMode; } }

        public static bool CookiesSupported {
            get {
                HttpContext context = HttpContext.Current;
                if (context != null) {
                    return !(CookielessHelperClass.UseCookieless(context, false, CookieMode));
                }
                return true;
            }
        }

        public static string LoginUrl {
            get {
                Initialize();
                HttpContext context = HttpContext.Current;
                if (context != null)  {
                    return AuthenticationConfig.GetCompleteLoginUrl(context, _LoginUrl);
                }
                if (_LoginUrl.Length == 0 || (_LoginUrl[0] != '/' && _LoginUrl.IndexOf("//", StringComparison.Ordinal) < 0))
                    return ("/" + _LoginUrl);
                return _LoginUrl;
            }
        }

        public static string DefaultUrl {
            get {
                Initialize();
                HttpContext context = HttpContext.Current;
                if (context != null)  {
                    return AuthenticationConfig.GetCompleteLoginUrl(context, _DefaultUrl);
                }
                if (_DefaultUrl.Length == 0 || (_DefaultUrl[0] != '/' && _DefaultUrl.IndexOf("//", StringComparison.Ordinal) < 0))
                    return ("/" + _DefaultUrl);
                return _DefaultUrl;
            }
        }

        internal static string ReturnUrlVar {
            get {
                if (!String.IsNullOrEmpty(AppSettings.FormsAuthReturnUrlVar)) {
                    return AppSettings.FormsAuthReturnUrlVar;
                }

                return "ReturnUrl";
            }
        }

        internal static string GetLoginPage(string extraQueryString) {
            return GetLoginPage(extraQueryString, false);
        }
        internal static string GetLoginPage(string extraQueryString, bool reuseReturnUrl) {
            HttpContext context = HttpContext.Current;
            string loginUrl = FormsAuthentication.LoginUrl;
            if (loginUrl.IndexOf('?') >= 0)
                loginUrl = RemoveQueryStringVariableFromUrl(loginUrl, ReturnUrlVar);
            int pos = loginUrl.IndexOf('?');
            if (pos < 0)
                loginUrl += "?";
            else
                if (pos < loginUrl.Length -1)
                    loginUrl += "&";
            string returnUrl = null;
            if (reuseReturnUrl) {
                returnUrl = HttpUtility.UrlEncode( GetReturnUrl(false),
                                                   context.Request.QueryStringEncoding );
            }
            if (returnUrl == null)
                returnUrl = HttpUtility.UrlEncode(context.Request.RawUrl, context.Request.ContentEncoding);

            loginUrl += ReturnUrlVar + "=" + returnUrl;
            if (!String.IsNullOrEmpty(extraQueryString)) {
                loginUrl += "&" + extraQueryString;
            }
            return loginUrl;
        }


        public static void RedirectToLoginPage() {
            RedirectToLoginPage(null);
        }


        public static void RedirectToLoginPage(string extraQueryString) {
            HttpContext context = HttpContext.Current;
            string loginUrl = GetLoginPage(extraQueryString);
            context.Response.Redirect(loginUrl, false);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        // Private stuff

        /////////////////////////////////////////////////////////////////////////////
        // Config Tags
        private  const String   CONFIG_DEFAULT_COOKIE    = ".ASPXAUTH";

        /////////////////////////////////////////////////////////////////////////////
        // Private data
        private static bool                _Initialized;
        private static String              _FormsName;
        //private static FormsProtectionEnum _Protection;
        private static FormsProtectionEnum _Protection;
        private static Int32               _Timeout;
        private static String              _FormsCookiePath;
        private static bool                _RequireSSL;
        private static bool                _SlidingExpiration;
        private static string              _LoginUrl;
        private static string              _DefaultUrl;
        private static HttpCookieMode      _CookieMode;
        private static string              _CookieDomain = null;
        private static bool                _EnableCrossAppRedirects;
        private static TicketCompatibilityMode _TicketCompatibilityMode;

        /////////////////////////////////////////////////////////////////////////////
        private static byte[] MakeTicketIntoBinaryBlob(FormsAuthenticationTicket ticket) {
            // None of the modes (Framework20 / Framework40 / beyond) support null values for these fields;
            // they always eventually just returned a null value.
            if (ticket.Name == null || ticket.UserData == null || ticket.CookiePath == null) {
                return null;
            }

            // ** MSRC 11838 **
            // Framework20 / Framework40 ticket generation modes are insecure. We should use a
            // secure serialization mode by default.
            if (!AppSettings.UseLegacyFormsAuthenticationTicketCompatibility) {
                return FormsAuthenticationTicketSerializer.Serialize(ticket);
            }

            // ** MSRC 11838 **
            // If we have reached this point of execution, the developer has explicitly elected
            // to continue using the insecure code path instead of the secure one. We removed
            // the Framework40 serialization mode, so everybody using the legacy code path is
            // forced to Framework20.

            byte []   bData  = new byte[4096];
            byte []   pBin   = new byte[4];
            long []   pDates = new long[2];
            byte []   pNull  = { 0, 0, 0 };

            // DevDiv Bugs 137864: 8 bytes may not be enough random bits as the length should be equal to the
            // key size. In CompatMode > Framework20SP1, use the IVType.Random feature instead of these 8 bytes,
            // but still include empty 8 bytes for compat with webengine.dll, where CookieAuthConstructTicket is.
            // Note that even in CompatMode = Framework20SP2 we fill 8 bytes with random data if the ticket
            // is not going to be encrypted.

            bool willEncrypt = (_Protection == FormsProtectionEnum.All || _Protection == FormsProtectionEnum.Encryption);
            bool legacyPadding = !willEncrypt || (MachineKeySection.CompatMode == MachineKeyCompatibilityMode.Framework20SP1);
            if (legacyPadding) {
                // Fill the first 8 bytes of the blob with random bits
                byte[] bRandom = new byte[8];
                RNGCryptoServiceProvider randgen = new RNGCryptoServiceProvider();
                randgen.GetBytes(bRandom);
                Buffer.BlockCopy(bRandom, 0, bData, 0, 8);
            }
            else {
                // use blank 8 bytes for compatibility with CookieAuthConstructTicket (do nothing)
            }

            pBin[0] = (byte) ticket.Version;
            pBin[1] = (byte) (ticket.IsPersistent ? 1 : 0);

            pDates[0] = ticket.IssueDate.ToFileTime();
            pDates[1] = ticket.Expiration.ToFileTime();

            int iRet = UnsafeNativeMethods.CookieAuthConstructTicket(
                        bData, bData.Length,
                        ticket.Name, ticket.UserData, ticket.CookiePath,
                        pBin, pDates);

            if (iRet < 0)
                return null;

            byte[] ciphertext = new byte[iRet];
            Buffer.BlockCopy(bData, 0, ciphertext, 0, iRet);
            return ciphertext;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        internal static string RemoveQueryStringVariableFromUrl(string strUrl, string QSVar) {
            int posQ = strUrl.IndexOf('?');
            if (posQ < 0)
                return strUrl;

            // Remove non-encoded QSVars
            string amp   = @"&";
            string question = @"?";

            string token = amp + QSVar + "=";
            RemoveQSVar(ref strUrl, posQ, token, amp, amp.Length);

            token = question + QSVar + "=";
            RemoveQSVar(ref strUrl, posQ, token, amp, question.Length);

            // Remove Url-enocoded strings
            amp = HttpUtility.UrlEncode(@"&");
            question = HttpUtility.UrlEncode(@"?");

            token = amp + HttpUtility.UrlEncode(QSVar + "=");
            RemoveQSVar(ref strUrl, posQ, token, amp, amp.Length);

            token = question + HttpUtility.UrlEncode(QSVar + "=");
            RemoveQSVar(ref strUrl, posQ, token, amp, question.Length);
            return strUrl;
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        static private void RemoveQSVar(ref string strUrl, int posQ, string token, string sep, int lenAtStartToLeave)
        {
            for (int pos = strUrl.LastIndexOf(token, StringComparison.Ordinal); pos >= posQ; pos = strUrl.LastIndexOf(token, StringComparison.Ordinal))
            {
                int end = strUrl.IndexOf(sep, pos + token.Length, StringComparison.Ordinal) + sep.Length;
                if (end < sep.Length || end >= strUrl.Length)
                { // ending separator not found or nothing is at the end
                    strUrl = strUrl.Substring(0, pos);
                }
                else
                {
                    strUrl = strUrl.Substring(0, pos + lenAtStartToLeave) + strUrl.Substring(end);
                }
            }
        }
        static private bool IsPathWithinAppRoot(HttpContext context, string path)
        {
            Uri absUri;
            if (!Uri.TryCreate(path, UriKind.Absolute, out absUri))
                return HttpRuntime.IsPathWithinAppRoot(path);

            if (!absUri.IsLoopback && !string.Equals(context.Request.Url.Host, absUri.Host, StringComparison.OrdinalIgnoreCase))
                return false; // different servers

            return HttpRuntime.IsPathWithinAppRoot(absUri.AbsolutePath);
        }
    }
}

