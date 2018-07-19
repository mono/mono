//------------------------------------------------------------------------------
// <copyright file="MobileFormsAuthentication.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics;
using System.Security;
using System.Web.Security;
using System.Web.UI.MobileControls;
using System.Security.Permissions;

namespace System.Web.Mobile
{
    /*
     * MobileFormsAuthentication
     * provides mobile comopatible version of ASP.Net methods
     *
     * Copyright (c) 2000 Microsoft Corporation
     */


    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class MobileFormsAuthentication
    {
        // Class only contains statics, so make the constructor private.
        private MobileFormsAuthentication()
        {
        }

        /// <include file='doc\MobileFormsAuthentication.uex' path='docs/doc[@for="MobileFormsAuthentication.RedirectFromLoginPage"]/*' />
        public static void RedirectFromLoginPage(String userName, bool createPersistentCookie)
        {
            FormsAuthentication.RedirectFromLoginPage(userName, createPersistentCookie, FormsAuthentication.FormsCookiePath);
        }

        /// <include file='doc\MobileFormsAuthentication.uex' path='docs/doc[@for="MobileFormsAuthentication.RedirectFromLoginPage1"]/*' />
        public static void RedirectFromLoginPage(String userName, bool createPersistentCookie, String strCookiePath)
        {
            FormsAuthentication.RedirectFromLoginPage(userName, createPersistentCookie, strCookiePath);
            /*
            // Disallow redirection to an absolute url.
            String requestReturnUrl = HttpContext.Current.Request["ReturnUrl"];
            if (requestReturnUrl != null && requestReturnUrl.IndexOf (":") != -1)
            {
                throw new SecurityException(SR.GetString(SR.Security_ReturnUrlCannotBeAbsolute, requestReturnUrl));
            }

            // GetRedirectUrl redirects to returnUrl if it exists, current app's default.aspx otherwise.
            String redirectUrl = FormsAuthentication.GetRedirectUrl(userName, createPersistentCookie);
            Debug.Assert (redirectUrl == requestReturnUrl || requestReturnUrl == null);

            String updatedRedirectUrl = redirectUrl;
            String cookieName = FormsAuthentication.FormsCookieName;
            HttpCookie cookie = FormsAuthentication.GetAuthCookie(userName, createPersistentCookie, strCookiePath);
            String strEncrypted = cookie.Value;

            int ticketLoc = redirectUrl.IndexOf(cookieName + "=");
            if(ticketLoc != -1)
            {
                updatedRedirectUrl = redirectUrl.Substring(0, ticketLoc);
                updatedRedirectUrl += cookieName + "=" + strEncrypted;
                int ampersandLoc = redirectUrl.IndexOf('&', ticketLoc);
                if(ampersandLoc != -1)
                {
                    updatedRedirectUrl += redirectUrl.Substring(ampersandLoc);
                }
            }
            else
            {
                int loc = updatedRedirectUrl.IndexOf('?');
                updatedRedirectUrl += (loc != -1) ? "&" : "?";
                updatedRedirectUrl += cookieName + "=" + strEncrypted;
            }

            HttpContext.Current.Response.Redirect(updatedRedirectUrl, true);
//            MobileRedirect.RedirectToUrl(HttpContext.Current, updatedRedirectUrl, true);
            */
        }

        /// <include file='doc\MobileFormsAuthentication.uex' path='docs/doc[@for="MobileFormsAuthentication.SignOut"]/*' />
        public static void SignOut()
        {
            /*
            MobilePage page = HttpContext.Current.Handler as MobilePage;
            if (page != null)
            {
                page.Adapter.PersistCookielessData = false;
                if (!page.Device.SupportsEmptyStringInCookieValue)
                {
                    // Desktop signout with empty cookie value is not handled properly by the device.
                    InternalSignOut ();
                    return;
                }
            }
            */
            FormsAuthentication.SignOut();
        }

/*
        private static void InternalSignOut ()
        {
            HttpContext context = HttpContext.Current;
            String userName =
                context.User == null || context.User.Identity == null || context.User.Identity.Name == null ?
                "" :
                context.User.Identity.Name;
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket (
                1, // version
                userName,
                DateTime.Now, // Issue-Date
                new DateTime(1999, 10, 12), // Expiration
                false, // IsPersistent
                "", // User-Data
                FormsAuthentication.FormsCookiePath);
            String encryptedTicket = FormsAuthentication.Encrypt (ticket);
            if (encryptedTicket == null) { // Encrypt returned null
                encryptedTicket = "x";
            }
            HttpCookie cookie = new HttpCookie (FormsAuthentication.FormsCookieName, encryptedTicket);
            cookie.Path = FormsAuthentication.FormsCookiePath;
            cookie.Expires = new System.DateTime (1999, 10, 12);
            cookie.Secure = FormsAuthentication.RequireSSL;
            context.Response.Cookies.Remove (FormsAuthentication.FormsCookieName);
            context.Response.Cookies.Add (cookie);
        }
        */
    }
}


