//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicPanelAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Security.Permissions;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{

    /// <include file='doc\XhtmlBasicPanelAdapter.uex' path='docs/doc[@for="XhtmlPanelAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlPanelAdapter : XhtmlControlAdapter {
        /// <include file='doc\XhtmlBasicPanelAdapter.uex' path='docs/doc[@for="XhtmlPanelAdapter.Control"]/*' />
        protected new Panel Control {
            get {
                return base.Control as Panel;
            }
        }

        /// <include file='doc\XhtmlBasicPanelAdapter.uex' path='docs/doc[@for="XhtmlPanelAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            if (Control.Content != null) {
                Control.Content.RenderControl(writer);
            }
            else {
                ConditionalEnterStyle(writer, Style);
                ConditionalRenderOpeningDivElement(writer);
                RenderChildren(writer);
                ConditionalSetPendingBreak(writer);
                ConditionalRenderClosingDivElement(writer);
                ConditionalExitStyle(writer, Style);
            }
        }    
    }
}
