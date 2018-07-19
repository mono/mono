//------------------------------------------------------------------------------
// <copyright file="IPageAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /*
     * PageAdapter Interface.
     * A control adapter handles all of the (potentially) device specific 
     * functionality for a mobile page.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\IPageAdapter.uex' path='docs/doc[@for="IPageAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public interface IPageAdapter : IControlAdapter
    {
        /// <include file='doc\IPageAdapter.uex' path='docs/doc[@for="IPageAdapter.Page"]/*' />
        new MobilePage Page
        {
            get;
            set;
        }
        /// <include file='doc\IPageAdapter.uex' path='docs/doc[@for="IPageAdapter.OptimumPageWeight"]/*' />

        int OptimumPageWeight
        {
            get;
        }
        /// <include file='doc\IPageAdapter.uex' path='docs/doc[@for="IPageAdapter.CookielessDataDictionary"]/*' />

        IDictionary CookielessDataDictionary
        {
            get;
            set;
        }
        /// <include file='doc\IPageAdapter.uex' path='docs/doc[@for="IPageAdapter.PersistCookielessData"]/*' />

        bool PersistCookielessData
        {
            get;
            set;
        }
        /// <include file='doc\IPageAdapter.uex' path='docs/doc[@for="IPageAdapter.CreateTextWriter"]/*' />

        //  return null to indicate use base implementation
        HtmlTextWriter CreateTextWriter(TextWriter writer);
        /// <include file='doc\IPageAdapter.uex' path='docs/doc[@for="IPageAdapter.DeterminePostBackMode"]/*' />

        // Each device specific PageAdapter can manipulate the incoming post
        // back value collection and return a new collection.
        NameValueCollection DeterminePostBackMode
        (
            HttpRequest request,
            String postEventSourceID,
            String postEventArgumentID,
            NameValueCollection baseCollection
        );
        /// <include file='doc\IPageAdapter.uex' path='docs/doc[@for="IPageAdapter.CacheVaryByHeaders"]/*' />

        // Return a list of additional HTTP headers that want to be keyed for
        // the ASP.NET page output caching mechanism.
        IList CacheVaryByHeaders
        {
            get;
        }
        /// <include file='doc\IPageAdapter.uex' path='docs/doc[@for="IPageAdapter.HandleError"]/*' />

        bool HandleError(Exception e, HtmlTextWriter writer);
        /// <include file='doc\IPageAdapter.uex' path='docs/doc[@for="IPageAdapter.HandlePagePostBackEvent"]/*' />
        bool HandlePagePostBackEvent(String eventSource, String eventArgument);
    }
}
