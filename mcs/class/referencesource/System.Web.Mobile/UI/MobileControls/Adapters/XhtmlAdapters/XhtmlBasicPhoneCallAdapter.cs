//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicPhoneCallAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Globalization;
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

    /// <include file='doc\XhtmlBasicPhoneCallAdapter.uex' path='docs/doc[@for="XhtmlPhoneCallAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlPhoneCallAdapter : XhtmlControlAdapter {
        /// <include file='doc\XhtmlBasicPhoneCallAdapter.uex' path='docs/doc[@for="XhtmlPhoneCallAdapter.Control"]/*' />
        protected new PhoneCall Control {
            get {
                return base.Control as PhoneCall;
            }
        }

        /// <include file='doc\XhtmlBasicPhoneCallAdapter.uex' path='docs/doc[@for="XhtmlPhoneCallAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer)
        {
            ConditionalClearPendingBreak(writer);
            Style style = Style;
            StyleFilter filter = writer.CurrentStyleClass.GetFilter(style);
            if ((filter & XhtmlConstants.Layout) != 0) {
                ConditionalEnterLayout(writer, style);
            }
            if (Device.CanInitiateVoiceCall) {
                String text = Control.Text;
                String phoneNumber = Control.PhoneNumber;

                if (text == null || text.Length == 0) {
                    text = phoneNumber;
                }

                writer.WriteBeginTag("a");

                if ((String)Device["supportsWtai"] == "true") {
                    writer.Write(" href=\"wtai://wp/mc;");
                }
                else {
                    writer.Write(" href=\"tel:");
                }

                foreach (char ch in phoneNumber) {
                    if (ch >= '0' && ch <= '9' || ch == '#' || ch == '+') {
                        writer.Write(ch);
                    }
                }
                writer.Write("\"");
                ConditionalRenderCustomAttribute(writer, XhtmlConstants.AccessKeyCustomAttribute);
                String cssClass = GetCustomAttributeValue(XhtmlConstants.CssClassCustomAttribute);
                if (CssLocation != StyleSheetLocation.PhysicalFile) {
                    String className = writer.GetCssFormatClassName(style);
                    if (className != null) {
                        writer.WriteAttribute ("class", className);
                    }
                }
                else if (cssClass != null && cssClass.Length > 0) {
                    writer.WriteAttribute ("class", cssClass, true /* encode */);
                }
                writer.Write(">");
                writer.WriteEncodedText(text);
                writer.WriteEndTag("a");
                ConditionalSetPendingBreakAfterInline(writer);
            }
            else {
                // Format the text string based on properties
                String text = String.Format(
                    CultureInfo.CurrentCulture,
                    Control.AlternateFormat,
                    Control.Text,
                    Control.PhoneNumber);
                String url = Control.AlternateUrl;

                // If URI specified, create a link.  Otherwise, only text is displayed.
                if (url != null && url.Length > 0) {
                    String cssClass = GetCustomAttributeValue(XhtmlConstants.CssClassCustomAttribute);
                    String accessKey = GetCustomAttributeValue(XhtmlConstants.AccessKeyCustomAttribute);
                    RenderBeginLink(writer, url, accessKey, style, cssClass);
                    writer.WriteEncodedText(text);
                    RenderEndLink(writer);
                    ConditionalSetPendingBreakAfterInline(writer);
                }
                else {
                    writer.WritePendingBreak();
                    ConditionalEnterFormat(writer, style);
                    ConditionalRenderOpeningSpanElement(writer);
                    writer.WriteEncodedText(text);
                    ConditionalRenderClosingSpanElement(writer);
                    ConditionalExitFormat(writer, style);
                    ConditionalSetPendingBreak(writer);
                }
            }
            if ((filter & XhtmlConstants.Layout) != 0) {
                ConditionalExitLayout(writer, style);
            }
        }

    }
}
