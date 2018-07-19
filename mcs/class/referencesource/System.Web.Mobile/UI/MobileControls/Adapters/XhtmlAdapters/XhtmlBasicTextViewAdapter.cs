//------------------------------------------------------------------------------
// <copyright file="XhtmlBasicTextViewAdapter.cs" company="Microsoft">
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

    /// <include file='doc\XhtmlBasicTextViewAdapter.uex' path='docs/doc[@for="XhtmlTextViewAdapter"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class XhtmlTextViewAdapter : XhtmlControlAdapter {
        /// <include file='doc\XhtmlBasicTextViewAdapter.uex' path='docs/doc[@for="XhtmlTextViewAdapter.Control"]/*' />
        protected new TextView Control {
            get {
                return base.Control as TextView;
            }
        }

        /// <include file='doc\XhtmlBasicTextViewAdapter.uex' path='docs/doc[@for="XhtmlTextViewAdapter.Render"]/*' />
        public override void Render(XhtmlMobileTextWriter writer) {
            int beginElement = Control.FirstVisibleElementIndex;
            int beginOffset = Control.FirstVisibleElementOffset;
            int endElement = Control.LastVisibleElementIndex;
            int endOffset = Control.LastVisibleElementOffset;

            // ConditionalClearCachedEndTag() is for a device special case.
            ConditionalClearCachedEndTag(writer, Control.Text);
            ConditionalEnterStyle(writer, Style);
            ConditionalRenderOpeningSpanElement(writer);
            writer.WritePendingBreak();
            for(int i = beginElement; i <= endElement; i++) {
                int begin = (i == beginElement) ? beginOffset : 0;
                int end;
                if (i == endElement) {
                    if (endOffset <= begin) {
                        break;
                    }
                    end = endOffset;
                }
                else {
                    end = -1;
                }
                RenderElement(writer, i, begin, end);
            }
            // ConditionalSetPendingBreak should always be called *before* ConditionalExitStyle.
            // ConditionalExitStyle may render a block element and clear the pending break.
            ConditionalSetPendingBreak(writer);            
            ConditionalRenderClosingSpanElement(writer);
            ConditionalExitStyle(writer, Style);
        }

        /// <include file='doc\XhtmlBasicTextViewAdapter.uex' path='docs/doc[@for="XhtmlTextViewAdapter.RenderElement"]/*' />
        public void RenderElement(XhtmlMobileTextWriter writer, int index, int beginSubstring, int endSubstring) {
            TextViewElement element = Control.GetElement(index);
            writer.WritePendingBreak();
            if (endSubstring == -1) {
                endSubstring = element.Text.Length;
            }
            String text = element.Text;
            if (beginSubstring > 0 || endSubstring < text.Length) {
                text = text.Substring(beginSubstring, endSubstring - beginSubstring);
            }
            BooleanOption prevBold = Style.Font.Bold;
            BooleanOption prevItalic = Style.Font.Italic;
            if (element.IsBold) {
                Style.Font.Bold = BooleanOption.True;
            }
            if (element.IsItalic) {
                Style.Font.Italic = BooleanOption.True;
            }
            ConditionalEnterStyle(writer, Style);           
            if (element.Url != null) {
                RenderBeginLink(writer, element.Url);
                writer.WriteEncodedText(text);
                RenderEndLink(writer);
            }
            else {
                writer.WriteEncodedText(text);
            }
            if (element.BreakAfter) {
                writer.SetPendingBreak();
            }
            ConditionalExitStyle(writer, Style);
            Style.Font.Bold = prevBold;
            Style.Font.Italic = prevItalic;
        }
    }
}
