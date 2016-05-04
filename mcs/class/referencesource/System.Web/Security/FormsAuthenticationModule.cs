//------------------------------------------------------------------------------
// <copyright file="FormsAuthenticationModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * FormsAuthenticationModule class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System.Globalization;
    using System.Web;
    using System.Text;
    using System.Web.Configuration;
    using System.Web.Caching;
    using System.Web.Handlers;
    using System.Collections;
    using System.Web.Util;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Web.Management;

    public sealed class FormsAuthenticationModule : IHttpModule {

        // Config values
        private static bool      _fAuthChecked;
        private static bool      _fAuthRequired;
        internal static bool FormsAuthRequired {
            get {
                return _fAuthRequired;
            }
        }

        private bool _fOnEnterCalled;
        private FormsAuthenticationEventHandler _eventHandler;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.Security.FormsAuthenticationModule'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public FormsAuthenticationModule() {
        }

        /// <devdoc>
        ///    This is a Global.asax event which must be
        ///    named FormsAuthenticate_OnAuthenticate event. It's used by advanced users to
        ///    customize cookie authentication.
        /// </devdoc>
        public event FormsAuthenticationEventHandler Authenticate {
            add {
                _eventHandler += value;
            }
            remove {
                _eventHandler -= value;
            }
        }

        public void Dispose() {
        }

        public void Init(HttpApplication app) {
            // authentication is an app level setting only
            // so we can read app config early on in an attempt to try and
            // skip wiring up event delegates
            if (!_fAuthChecked) {
                _fAuthRequired = (AuthenticationConfig.Mode == AuthenticationMode.Forms);
                _fAuthChecked = true;
            }
            
            if (_fAuthRequired) {
                // initialize if mode is forms auth
                FormsAuthentication.Initialize();

                app.AuthenticateRequest += new EventHandler(this.OnEnter);
                app.EndRequest          += new EventHandler(this.OnLeave);
            }
        }

        ////////////////////////////////////////////////////////////
        // OnAuthenticate: Forms Authentication modules can override
        //             this method to create a Forms IPrincipal object from
        //             a WindowsIdentity
        private void OnAuthenticate(FormsAuthenticationEventArgs e) {

            HttpCookie cookie = null;

            ////////////////////////////////////////////////////////////
            // Step 1: If there are event handlers, invoke the handlers
            if (_eventHandler != null)
                _eventHandler(this, e);

            ////////////////////////////////////////////////////////////
            // Step 2: Check if the event handler created a user-object
            if (e.Context.User != null) {
                // do nothing because someone else authenticated
                return;
            }

            if (e.User != null) {
                // the event handler created a user
                e.Context.SetPrincipalNoDemand(e.User);
                return;
            }

            ////////////////////////////////////////////////////////////
            // Step 3: Extract the cookie and create a ticket from it
            bool cookielessTicket = false;
            FormsAuthenticationTicket ticket = ExtractTicketFromCookie(e.Context, FormsAuthentication.FormsCookieName, out cookielessTicket);

            ////////////////////////////////////////////////////////////
            // Step 4: See if the ticket was created: No => exit immediately
            if (ticket == null || ticket.Expired)
                return;

            ////////////////////////////////////////////////////////////
            // Step 5: Renew the ticket
            FormsAuthenticationTicket ticket2 = ticket;
            if (FormsAuthentication.SlidingExpiration)
                ticket2 = FormsAuthentication.RenewTicketIfOld(ticket);

            ////////////////////////////////////////////////////////////
            // Step 6: Create a user object for the ticket
            e.Context.SetPrincipalNoDemand(new GenericPrincipal(new FormsIdentity(ticket2), new String[0]));

            ////////////////////////////////////////////////////////////
            // Step 7: Browser does not send us the correct cookie-path
            //         Update the cookie to show the correct path
            if (!cookielessTicket && !ticket2.CookiePath.Equals("/"))
            {
                cookie = e.Context.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie != null) {
                    cookie.Path = ticket2.CookiePath;
                }
            }

            ////////////////////////////////////////////////////////////
            // Step 8: If the ticket was renewed, save the ticket in the cookie
            if (ticket2 != ticket)
            {
                if(cookielessTicket && ticket2.CookiePath != "/" && ticket2.CookiePath.Length > 1) {
                    FormsAuthenticationTicket tempTicket = FormsAuthenticationTicket.FromUtc(ticket2.Version, ticket2.Name, ticket2.IssueDateUtc,
                                                                                             ticket2.ExpirationUtc, ticket2.IsPersistent, ticket2.UserData,
                                                                                             "/");
                    ticket2 = tempTicket;
                }
                String  strEnc = FormsAuthentication.Encrypt(ticket2, !cookielessTicket);

                if (cookielessTicket) {
                    e.Context.CookielessHelper.SetCookieValue('F', strEnc);
                    e.Context.Response.Redirect(e.Context.Request.RawUrl);
                } else {
                    if (cookie != null)
                        cookie = e.Context.Request.Cookies[FormsAuthentication.FormsCookieName];

                    if (cookie == null) {
                        cookie = new HttpCookie(FormsAuthentication.FormsCookieName, strEnc);
                        cookie.Path = ticket2.CookiePath;
                    }

                    if (ticket2.IsPersistent)
                        cookie.Expires = ticket2.Expiration;
                    cookie.Value = strEnc;
                    cookie.Secure = FormsAuthentication.RequireSSL;
                    cookie.HttpOnly = true;
                    if (FormsAuthentication.CookieDomain != null)
                        cookie.Domain = FormsAuthentication.CookieDomain;
                    e.Context.Response.Cookies.Remove(cookie.Name);
                    e.Context.Response.Cookies.Add(cookie);
                }
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void OnEnter(Object source, EventArgs eventArgs) {
            _fOnEnterCalled = true;

            HttpApplication app;
            HttpContext context;

            app = (HttpApplication)source;
            context = app.Context;

#if DBG
            Trace("*******************Request path: " + context.Request.RawUrl);
#endif

            ////////////////////////////////////////////////////////
            // Step 2: Call OnAuthenticate virtual method to create
            //    an IPrincipal for this request
            OnAuthenticate( new FormsAuthenticationEventArgs(context) );

            ////////////////////////////////////////////////////////
            // Skip AuthZ if accessing the login page

            // We do this here to force the cookieless helper to fish out and
            // remove the token from the URL if it's present there.
            CookielessHelperClass cookielessHelper = context.CookielessHelper;

            if (AuthenticationConfig.AccessingLoginPage(context, FormsAuthentication.LoginUrl)) {

                context.SetSkipAuthorizationNoDemand(true, false /*managedOnly*/);
                cookielessHelper.RedirectWithDetectionIfRequired(null, FormsAuthentication.CookieMode);
            }
            if (!context.SkipAuthorization) {
                context.SetSkipAuthorizationNoDemand(AssemblyResourceLoader.IsValidWebResourceRequest(context), false /*managedOnly*/);
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void OnLeave(Object source, EventArgs eventArgs) {
            if (_fOnEnterCalled)
                _fOnEnterCalled = false;
            else
                return; // no need to continue if we skipped OnEnter

            HttpApplication    app;
            HttpContext        context;

            app       = (HttpApplication)source;
            context   = app.Context;

            ////////////////////////////////////////////////////////////
            // Step 1: Check if we are using cookie authentication and
            //         if authentication failed
            if (context.Response.StatusCode != 401)
                return;

            ////////////////////////////////////////////////////////////
            // Change 401 to a redirect to login page

            // Don't do it if the redirect is suppressed for this response

            if (context.Response.SuppressFormsAuthenticationRedirect) {
                return;
            }

            // Don't do it if already there is ReturnUrl, already being redirected,
            // to avoid infinite redirection loop

            String strUrl = context.Request.RawUrl;
            if (strUrl.IndexOf("?" + FormsAuthentication.ReturnUrlVar + "=", StringComparison.Ordinal) != -1
                || strUrl.IndexOf("&" + FormsAuthentication.ReturnUrlVar + "=", StringComparison.Ordinal) != -1) {
                return;
            }

            ////////////////////////////////////////////////////////////
            // Step 2: Get the complete url to the login-page
            String loginUrl = null;
            if (!String.IsNullOrEmpty(FormsAuthentication.LoginUrl))
                loginUrl = AuthenticationConfig.GetCompleteLoginUrl(context, FormsAuthentication.LoginUrl);

            ////////////////////////////////////////////////////////////
            // Step 3: Check if we have a valid url to the login-page
            if (loginUrl == null || loginUrl.Length <= 0)
                throw new HttpException(SR.GetString(SR.Auth_Invalid_Login_Url));


            ////////////////////////////////////////////////////////////
            // Step 4: Construct the redirect-to url
            String             strRedirect;
            int                iIndex;
            CookielessHelperClass cookielessHelper;

//            if(context.Request.Browser["isMobileDevice"] == "true") {
//                //__redir=1 is marker for devices that post on redirect
//                if(strUrl.IndexOf("__redir=1") >= 0) {
//                    strUrl = SanitizeUrlForCookieless(strUrl);
//                }
//                else {
//                    if(strUrl.IndexOf('?') >= 0)
//                        strSep = "&";
//                    else
//                        strSep = "?";
//                    strUrl = SanitizeUrlForCookieless(strUrl + strSep + "__redir=1");
//                }
//            }

            // Create the CookielessHelper class to rewrite the path, if needed.
            cookielessHelper = context.CookielessHelper;

            if (loginUrl.IndexOf('?') >= 0) {
                loginUrl = FormsAuthentication.RemoveQueryStringVariableFromUrl(loginUrl, FormsAuthentication.ReturnUrlVar);
                strRedirect = loginUrl + "&" + FormsAuthentication.ReturnUrlVar + "=" + HttpUtility.UrlEncode(strUrl, context.Request.ContentEncoding);
            }
            else {
                strRedirect = loginUrl + "?" + FormsAuthentication.ReturnUrlVar + "=" + HttpUtility.UrlEncode(strUrl, context.Request.ContentEncoding);
            }

            ////////////////////////////////////////////////////////////
            // Step 5: Add the query-string from the current url

            iIndex = strUrl.IndexOf('?');
            if (iIndex >= 0 && iIndex < strUrl.Length - 1) {
                strRedirect += "&" + strUrl.Substring(iIndex + 1);
            }
            cookielessHelper.SetCookieValue('F', null); // remove old ticket if present
            cookielessHelper.RedirectWithDetectionIfRequired(strRedirect, FormsAuthentication.CookieMode);

            ////////////////////////////////////////////////////////////
            // Step 6: Do the redirect
            context.Response.Redirect(strRedirect, false);
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        // Private method for decrypting a cookie
        private static FormsAuthenticationTicket ExtractTicketFromCookie(HttpContext context, String name, out bool cookielessTicket) {
            FormsAuthenticationTicket ticket   = null;
            string                    encValue = null;
            bool                      ticketExpired = false;
            bool                      badTicket = false;

            try {
                try {

                    ////////////////////////////////////////////////////////////
                    // Step 0: Check if we should use cookieless
                    cookielessTicket = CookielessHelperClass.UseCookieless(context, false, FormsAuthentication.CookieMode);

                    ////////////////////////////////////////////////////////////
                    // Step 1: Check URI/cookie for ticket
                    if (cookielessTicket) {
                        encValue = context.CookielessHelper.GetCookieValue('F');
                    } else {
                        HttpCookie cookie = context.Request.Cookies[name];
                        if (cookie != null) {
                            encValue = cookie.Value;
                        }
                    }

                    ////////////////////////////////////////////////////////////
                    // Step 2: Decrypt encrypted ticket
                    if (encValue != null && encValue.Length > 1) {
                        try {
                            ticket = FormsAuthentication.Decrypt(encValue);
                        } catch {
                            if (cookielessTicket)
                                context.CookielessHelper.SetCookieValue('F', null);
                            else
                                context.Request.Cookies.Remove(name);
                            badTicket = true;
                            //throw;
                        }

                        if (ticket == null) {
                            badTicket = true;
                        }

                        if (ticket != null && !ticket.Expired) {
                            if (cookielessTicket || !FormsAuthentication.RequireSSL || context.Request.IsSecureConnection) // Make sure it is NOT a secure cookie over an in-secure connection
                                return ticket; // Found valid ticket
                        }

                        if (ticket != null && ticket.Expired) {
                            ticketExpired = true;
                        }

                        // Step 2b: Remove expired/bad ticket
                        ticket = null;
                        if (cookielessTicket)
                            context.CookielessHelper.SetCookieValue('F', null);
                        else
                            context.Request.Cookies.Remove(name);
                    }


                    ////////////////////////////////////////////////////////////
                    // Step 3: Look in QueryString
                    if (FormsAuthentication.EnableCrossAppRedirects) {
                        encValue = context.Request.QueryString[name];
                        if (encValue != null && encValue.Length > 1) {
                            if (!cookielessTicket && FormsAuthentication.CookieMode == HttpCookieMode.AutoDetect)
                                cookielessTicket = CookielessHelperClass.UseCookieless(context, true, FormsAuthentication.CookieMode); // find out for sure

                            try {
                                ticket = FormsAuthentication.Decrypt(encValue);
                            } catch {
                                badTicket = true;
                                //throw;
                            }

                            if (ticket == null) {
                                badTicket = true;
                            }
                        }

                        // Step 3b: Look elsewhere in the request (i.e. posted body)
                        if (ticket == null || ticket.Expired) {
                            encValue = context.Request.Form[name];
                            if (encValue != null && encValue.Length > 1) {
                                if (!cookielessTicket && FormsAuthentication.CookieMode == HttpCookieMode.AutoDetect)
                                    cookielessTicket = CookielessHelperClass.UseCookieless(context, true, FormsAuthentication.CookieMode); // find out for sure

                                try {
                                    ticket = FormsAuthentication.Decrypt(encValue);
                                } catch {
                                    badTicket = true;
                                    //throw;
                                }

                                if (ticket == null) {
                                    badTicket = true;
                                }
                            }
                        }
                    }

                    if (ticket == null || ticket.Expired) {
                        if (ticket != null && ticket.Expired)
                            ticketExpired = true;

                        return null; // not found! Exit with null
                    }

                    if (FormsAuthentication.RequireSSL && !context.Request.IsSecureConnection) // Bad scenario: valid ticket over non-SSL
                        throw new HttpException(SR.GetString(SR.Connection_not_secure_creating_secure_cookie));

                    ////////////////////////////////////////////////////////////
                    // Step 4: Create the cookie/URI value
                    if (cookielessTicket) {
                        if (ticket.CookiePath != "/") {
                            FormsAuthenticationTicket tempTicket = FormsAuthenticationTicket.FromUtc(ticket.Version, ticket.Name, ticket.IssueDateUtc,
                                                                                                     ticket.ExpirationUtc, ticket.IsPersistent, ticket.UserData,
                                                                                                     "/");
                            ticket = tempTicket;
                            encValue = FormsAuthentication.Encrypt(ticket);
                        }
                        context.CookielessHelper.SetCookieValue('F', encValue);
                        string strUrl = FormsAuthentication.RemoveQueryStringVariableFromUrl(context.Request.RawUrl, name);
                        context.Response.Redirect(strUrl);
                    } else {
                        HttpCookie cookie = new HttpCookie(name, encValue);
                        cookie.HttpOnly = true;
                        cookie.Path = ticket.CookiePath;
                        if (ticket.IsPersistent)
                            cookie.Expires = ticket.Expiration;
                        cookie.Secure = FormsAuthentication.RequireSSL;
                        if (FormsAuthentication.CookieDomain != null)
                            cookie.Domain = FormsAuthentication.CookieDomain;
                        context.Response.Cookies.Remove(cookie.Name);
                        context.Response.Cookies.Add(cookie);
                    }

                    return ticket;
                } finally {
                    if (badTicket) {
                        WebBaseEvent.RaiseSystemEvent(null, WebEventCodes.AuditFormsAuthenticationFailure,
                            WebEventCodes.InvalidTicketFailure);
                    } else if (ticketExpired) {
                        WebBaseEvent.RaiseSystemEvent(null, WebEventCodes.AuditFormsAuthenticationFailure,
                            WebEventCodes.ExpiredTicketFailure);
                    }
                }
            } catch {
                throw;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private static void Trace(String str) {
            Debug.Trace("cookieauth", str);
        }
    }
}


