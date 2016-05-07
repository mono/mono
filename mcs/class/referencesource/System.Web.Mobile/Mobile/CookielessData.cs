//------------------------------------------------------------------------------
// <copyright file="CookielessData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Mobile
{
    /*
     * CookielessData
     * encapsulates access to data to be persisted in local links
     *
     * Copyright (c) 2000 Microsoft Corporation
     */

    using System.Collections.Specialized;
    using System.Web.Security;
    using System.Security.Permissions;

    /// <include file='doc\CookielessData.uex' path='docs/doc[@for="CookielessData"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class CookielessData : HybridDictionary
    {
        /// <include file='doc\CookielessData.uex' path='docs/doc[@for="CookielessData.CookielessData"]/*' />
        public CookielessData()
        {
            String name = FormsAuthentication.FormsCookieName;
            String inboundValue = HttpContext.Current.Request.QueryString[name];
            if(inboundValue == null)
            {
                inboundValue = HttpContext.Current.Request.Form[name];
            }
            if(inboundValue != null)
            {
                FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(inboundValue);
                FormsAuthenticationTicket ticket2 = FormsAuthentication.RenewTicketIfOld(ticket);
                this[name] = FormsAuthentication.Encrypt(ticket2);
            }
        }
    }
}


