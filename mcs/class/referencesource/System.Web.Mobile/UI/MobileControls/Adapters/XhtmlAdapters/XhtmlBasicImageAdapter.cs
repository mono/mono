//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicImageAdapter.cs" company="Microsoft">
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

    /// <include file='doc\XhtmlBasicImageAdapter.uex' path='docs/doc[@for="XhtmlImageAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlImageAdapter : XhtmlControlAdapter {

        /// <include file='doc\XhtmlBasicImageAdapter.uex' path='docs/doc[@for="XhtmlImageAdapter.Control"]/*' />
        protected new Image Control {
            get {
                return base.Control as Image;
            }
        }

        /// <include file='doc\XhtmlBasicImageAdapter.uex' path='docs/doc[@for="XhtmlImageAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            string target = Control.NavigateUrl;
            ConditionalClearPendingBreak(writer);
            Style style = Style;
            StyleFilter filter = writer.CurrentStyleClass.GetFilter(style);
            if ((filter & XhtmlConstants.Layout) != 0) {
                ConditionalEnterLayout(writer, style);
            }

            if(target != null && target.Length > 0) {
                String cssClass = GetCustomAttributeValue(XhtmlConstants.CssClassCustomAttribute);
                String accessKey = GetCustomAttributeValue(XhtmlConstants.AccessKeyCustomAttribute);
                String title = GetCustomAttributeValue(XhtmlConstants.TitleCustomAttribute);
                RenderBeginLink(writer, target, accessKey, style, cssClass, title);
            }
            else{
                ConditionalEnterFormat(writer, style);
                ConditionalRenderOpeningSpanElement(writer);
            }
            String controlIU = Control.ImageUrl;
            if(controlIU == null || controlIU.Length == 0) {            
                writer.WriteEncodedText(Control.AlternateText);
            }
            else {
                RenderImage(writer);
            }
            ConditionalSetPendingBreakAfterInline(writer);
            if(target != null && target.Length > 0) {
                RenderEndLink(writer);
            }
            else {
                ConditionalRenderClosingSpanElement(writer);
                ConditionalExitFormat(writer, style);
            }
            if ((filter & XhtmlConstants.Layout) != 0) {
                ConditionalExitLayout(writer, style);
            }
        }

        /// <include file='doc\XhtmlBasicImageAdapter.uex' path='docs/doc[@for="XhtmlImageAdapter.RenderImage"]/*' />
        protected virtual void RenderImage(XhtmlMobileTextWriter writer) {
            String source = Control.ImageUrl;
            writer.WriteBeginTag("img");
            if(source != null && source.Length > 0) {
                source = Page.Server.UrlPathEncode(Control.ResolveUrl(source.Trim()));
                writer.WriteAttribute("src", source, true);
                writer.AddResource(source);
            }

            String alternateText = Control.AlternateText;
            if (alternateText == null || alternateText.Length == 0) {
                alternateText = " "; // ASURT 143759 and VSWhidbey 78593
            }
            writer.WriteAttribute("alt", alternateText, true);
            
            // Review: Html adapter writes border=0 attribute, but don't need this here?
            writer.Write(" />");
        }
    }
}
