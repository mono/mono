//------------------------------------------------------------------------------
// <copyright file="WmlLinkAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.MobileControls;
using System.Security.Permissions;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource
#else
namespace System.Web.UI.MobileControls.Adapters
#endif    

{

    /*
     * WmlLinkAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\WmlLinkAdapter.uex' path='docs/doc[@for="WmlLinkAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class WmlLinkAdapter : WmlControlAdapter
    {
        /// <include file='doc\WmlLinkAdapter.uex' path='docs/doc[@for="WmlLinkAdapter.Control"]/*' />
        protected new Link Control
        {
            get
            {
                return (Link)base.Control;
            }
        }

        /// <include file='doc\WmlLinkAdapter.uex' path='docs/doc[@for="WmlLinkAdapter.Render"]/*' />
        public override void Render(WmlMobileTextWriter writer)
        {
            String navigateUrl = Control.NavigateUrl;
            String text = (String.IsNullOrEmpty(Control.Text)) ? navigateUrl : Control.Text;
            bool breakAfter = Control.BreakAfter && !Device.RendersBreaksAfterWmlAnchor;
            String softkeyLabel = Control.SoftkeyLabel;
            bool implicitSoftkeyLabel = false;
            if (softkeyLabel.Length == 0)
            {
                implicitSoftkeyLabel = true;
                softkeyLabel = Control.Text;
            }

            writer.EnterStyle(Style);
            RenderLink(writer, navigateUrl, softkeyLabel, implicitSoftkeyLabel, true, text, breakAfter);
            writer.ExitStyle(Style);
        }
    }

}

