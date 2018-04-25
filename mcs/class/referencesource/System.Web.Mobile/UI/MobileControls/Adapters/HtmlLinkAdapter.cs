//------------------------------------------------------------------------------
// <copyright file="HtmlLinkAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Drawing;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif
{
    /*
     * HtmlLinkAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlLinkAdapter.uex' path='docs/doc[@for="HtmlLinkAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlLinkAdapter : HtmlControlAdapter
    {
        /// <include file='doc\HtmlLinkAdapter.uex' path='docs/doc[@for="HtmlLinkAdapter.Control"]/*' />
        protected new Link Control
        {
            get
            {
                return (Link)base.Control;
            }
        }

        /// <include file='doc\HtmlLinkAdapter.uex' path='docs/doc[@for="HtmlLinkAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            writer.EnterStyle(Style);
            String navigateUrl = Control.NavigateUrl;
            RenderBeginLink(writer, navigateUrl);
            writer.WriteText(String.IsNullOrEmpty(Control.Text) ? navigateUrl : Control.Text, true);
            RenderEndLink(writer);
            writer.ExitStyle(Style, Control.BreakAfter);
        }
    }
}



