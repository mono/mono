//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicLinkAdapter.cs" company="Microsoft">
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

    /// <include file='doc\XhtmlBasicLinkAdapter.uex' path='docs/doc[@for="XhtmlLinkAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlLinkAdapter : XhtmlControlAdapter {
        /// <include file='doc\XhtmlBasicLinkAdapter.uex' path='docs/doc[@for="XhtmlLinkAdapter.Control"]/*' />
        protected new Link Control {
            get {
                return base.Control as Link;
            }
        }

        /// <include file='doc\XhtmlBasicLinkAdapter.uex' path='docs/doc[@for="XhtmlLinkAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            ConditionalClearPendingBreak(writer);
            ConditionalEnterStyle(writer, Style);
            String cssClass = GetCustomAttributeValue(XhtmlConstants.CssClassCustomAttribute);
            String accessKey = GetCustomAttributeValue(XhtmlConstants.AccessKeyCustomAttribute);
            RenderBeginLink(writer, Control.NavigateUrl, accessKey, Style, cssClass);
            String controlText = Control.Text;
            writer.WriteEncodedText(controlText == null || controlText.Length == 0 ? Control.NavigateUrl : controlText);
            RenderEndLink(writer);
            // ConditionalSetPendingBreak should always be called *before* ConditionalExitStyle.
            // ConditionalExitStyle may render a block element and clear the pending break.
            ConditionalSetPendingBreakAfterInline(writer);  
            ConditionalExitStyle(writer, Style);
        }
    }
}
