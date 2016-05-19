//------------------------------------------------------------------------------
// <copyright file="ControlRenderingHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Web.Util;

    internal static class ControlRenderingHelper {
        private static readonly string SkipLinkContentMark = "_SkipLink";

        internal static void WriteSkipLinkStart(HtmlTextWriter writer, Version renderingCompatibility, bool designMode, string skipLinkText, string spacerImageUrl, string clientID) {
            if (skipLinkText.Length != 0 && !designMode) {
                if (renderingCompatibility >= VersionUtil.Framework45) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, '#' + clientID + SkipLinkContentMark);
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Position, "absolute");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Left, "-10000px");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Top, "auto");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Width, "1px");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Height, "1px");
                    writer.AddStyleAttribute(HtmlTextWriterStyle.Overflow, "hidden");
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.Write(skipLinkText);
                    writer.RenderEndTag();
                }
                else {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, '#' + clientID + SkipLinkContentMark);
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.AddAttribute(HtmlTextWriterAttribute.Alt, skipLinkText);
                    writer.AddAttribute(HtmlTextWriterAttribute.Src, spacerImageUrl);
                    writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "0px");
                    writer.AddAttribute(HtmlTextWriterAttribute.Width, "0");
                    writer.AddAttribute(HtmlTextWriterAttribute.Height, "0");
                    writer.RenderBeginTag(HtmlTextWriterTag.Img);
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
            }
        }

        internal static void WriteSkipLinkEnd(HtmlTextWriter writer, bool designMode, string skipLinkText, string clientID) {
            if (skipLinkText.Length != 0 && !designMode) {
                writer.AddAttribute(HtmlTextWriterAttribute.Id, clientID + SkipLinkContentMark); // XHTML 1.1 needs id instead of name
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.RenderEndTag();
            }
        }
    }
}
