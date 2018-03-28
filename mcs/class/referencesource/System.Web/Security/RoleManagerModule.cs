//------------------------------------------------------------------------------
// <copyright file="RoleManagerModule.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * RoleManagerModule class
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.Security {
    using System.Collections;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Caching;
    using System.Web.Util;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public sealed class RoleManagerModule : IHttpModule {
        private const int MAX_COOKIE_LENGTH = 4096;

        private RoleManagerEventHandler _eventHandler;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.Security.RoleManagerModule'/>
        ///       class.
        ///     </para>
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public RoleManagerModule() {
        }


        public event RoleManagerEventHandler GetRoles {
            add {
                HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, SR.Feature_not_supported_at_this_level);
                _eventHandler += value;
            }
            remove {
                _eventHandler -= value;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Dispose() {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void Init(HttpApplication app) {
            // for IIS 7, skip wireup of these delegates altogether unless the
            // feature is enabled for this application
            // this avoids the initial OnEnter transition unless it's needed
            if (Roles.Enabled) {
                app.PostAuthenticateRequest += new EventHandler(this.OnEnter);
                app.EndRequest += new EventHandler(this.OnLeave);
            }
        }

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        private void OnEnter(Object source, EventArgs eventArgs) {
            if (!Roles.Enabled) {
                if (HttpRuntime.UseIntegratedPipeline) {
                    ((HttpApplication)source).Context.DisableNotifications(RequestNotification.EndRequest, 0);
                }
                return;
            }
            
            HttpApplication app = (HttpApplication)source;
            HttpContext context = app.Context;
            if (_eventHandler != null) {
                RoleManagerEventArgs e = new RoleManagerEventArgs(context);
                _eventHandler(this, e);
                if (e.RolesPopulated)
                    return;
            }
            
            Debug.Assert(null != context.User, "null != context.User");
            
            if (Roles.CacheRolesInCookie)
            {
                if (context.User.Identity.IsAuthenticated && (!Roles.CookieRequireSSL || context.Request.IsSecureConnection))
                {
                    // Try to create from cookie
                    try
                    {
                        HttpCookie cookie = context.Request.Cookies[Roles.CookieName];
                        if (cookie != null)
                        {
                            string cookieValue = cookie.Value;
                            // Ignore cookies that are too long
                            if (cookieValue != null && cookieValue.Length > MAX_COOKIE_LENGTH) {
                                Roles.DeleteCookie();
                            }
                            else {
                                if (!String.IsNullOrEmpty(Roles.CookiePath) && Roles.CookiePath != "/") {
                                    cookie.Path = Roles.CookiePath;
                                }

                                cookie.Domain = Roles.Domain;
                                context.SetPrincipalNoDemand(CreateRolePrincipalWithAssert(context.User.Identity, cookieValue));
                            }
                        }
                    }
                    catch {  } // ---- exceptions
                }
                else
                {
                    if (context.Request.Cookies[Roles.CookieName] != null)
                        Roles.DeleteCookie();
                    // if we're not using cookie caching, we don't need the EndRequest
                    // event and can suppress it
                    if (HttpRuntime.UseIntegratedPipeline) {
                        context.DisableNotifications(RequestNotification.EndRequest, 0);
                    }
                }
            }

            if (!(context.User is RolePrincipal))
                context.SetPrincipalNoDemand(CreateRolePrincipalWithAssert(context.User.Identity));

            HttpApplication.SetCurrentPrincipalWithAssert(context.User);
        }

        [SecurityPermission(SecurityAction.Assert, ControlPrincipal = true)]
        private RolePrincipal CreateRolePrincipalWithAssert(IIdentity identity, string encryptedTicket = null) {
            if (encryptedTicket == null) {
                return new RolePrincipal(identity);
            }
            else {
                return new RolePrincipal(identity, encryptedTicket);
            }
        }

        private void OnLeave(Object source, EventArgs eventArgs) {
            HttpApplication app;
            HttpContext context;

            app = (HttpApplication)source;
            context = app.Context;

            if (!Roles.Enabled || !Roles.CacheRolesInCookie || context.Response.HeadersWritten)
                return;

            if (context.User == null || !(context.User is RolePrincipal) || !context.User.Identity.IsAuthenticated)
                return;
            if (Roles.CookieRequireSSL && !context.Request.IsSecureConnection)
            { // if cookie is sent, then clear it
                if (context.Request.Cookies[Roles.CookieName] != null)
                    Roles.DeleteCookie();
                return;
            }
            RolePrincipal rp = (RolePrincipal) context.User;
            if (rp.CachedListChanged && context.Request.Browser.Cookies)
            {
                string s = rp.ToEncryptedTicket();
                if (string.IsNullOrEmpty(s) || s.Length > MAX_COOKIE_LENGTH) {
                    Roles.DeleteCookie();
                } else {
                    HttpCookie cookie = new HttpCookie(Roles.CookieName, s);
                    cookie.HttpOnly = true;
                    cookie.Path = Roles.CookiePath;
                    cookie.Domain = Roles.Domain;
                    if (Roles.CreatePersistentCookie)
                        cookie.Expires = rp.ExpireDate;
                    cookie.Secure = Roles.CookieRequireSSL;
                    context.Response.Cookies.Add(cookie);
                }
            }
        }
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
    }
}


