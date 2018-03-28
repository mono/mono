//------------------------------------------------------------------------------
// <copyright file="HtmlPanelAdapter.cs" company="Microsoft">
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
     * HtmlPanelAdapter class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\HtmlPanelAdapter.uex' path='docs/doc[@for="HtmlPanelAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class HtmlPanelAdapter : HtmlControlAdapter
    {
        /// <include file='doc\HtmlPanelAdapter.uex' path='docs/doc[@for="HtmlPanelAdapter.Control"]/*' />
        protected new Panel Control
        {
            get
            {
                return (Panel)base.Control;
            }
        }

        /// <include file='doc\HtmlPanelAdapter.uex' path='docs/doc[@for="HtmlPanelAdapter.OnInit"]/*' />
        public override void OnInit(EventArgs e)
        {
        }       

        /// <include file='doc\HtmlPanelAdapter.uex' path='docs/doc[@for="HtmlPanelAdapter.Render"]/*' />
        public override void Render(HtmlMobileTextWriter writer)
        {
            if (Control.Content != null)
            {
                Control.Content.RenderControl(writer);
            }
            else
            {
                writer.EnterStyle(Style);
                RenderChildren(writer);
                writer.ExitStyle(Style, Control.BreakAfter);
            }
        }
    }
}
