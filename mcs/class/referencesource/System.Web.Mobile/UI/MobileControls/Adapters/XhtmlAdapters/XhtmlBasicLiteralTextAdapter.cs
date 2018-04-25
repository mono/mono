//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicLiteralTextAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
using System.Security.Permissions;
using System.Web.Mobile;
using System.Web.UI.MobileControls;
using System.Web.UI.MobileControls.Adapters;
using System;

#if COMPILING_FOR_SHIPPED_SOURCE
namespace System.Web.UI.MobileControls.ShippedAdapterSource.XhtmlAdapters
#else
namespace System.Web.UI.MobileControls.Adapters.XhtmlAdapters
#endif
{
    
    /// <include file='doc\XhtmlBasicLiteralTextAdapter.uex' path='docs/doc[@for="XhtmlLiteralTextAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlLiteralTextAdapter : XhtmlControlAdapter 
    {
        /// <include file='doc\XhtmlBasicLiteralTextAdapter.uex' path='docs/doc[@for="XhtmlLiteralTextAdapter.Control"]/*' />
        protected new LiteralText Control {
            get {
                return base.Control as LiteralText;
            }
        }
        /// <include file='doc\XhtmlBasicLiteralTextAdapter.uex' path='docs/doc[@for="XhtmlLiteralTextAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            // ConditionalClearCachedEndTag() is for a device special case.
            ConditionalClearCachedEndTag(writer, Control.Text);
            String text = Control.PagedText;
            ConditionalEnterStyle(writer, Style);
            ConditionalRenderOpeningSpanElement(writer);
            writer.WritePendingBreak();
            writer.WriteEncodedText(text);
            // ConditionalSetPendingBreak should always be called *before* ConditionalExitStyle.
            // ConditionalExitStyle may render a block element and clear the pending break.
            ConditionalSetPendingBreak(writer);            
            ConditionalRenderClosingSpanElement(writer);
            ConditionalExitStyle(writer, Style);
        }
    }
}
