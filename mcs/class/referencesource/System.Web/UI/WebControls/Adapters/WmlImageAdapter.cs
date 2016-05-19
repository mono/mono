//------------------------------------------------------------------------------
// <copyright file="WmlImageAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web.UI.WebControls;
    using System.Web.Util;

    public class WmlImageAdapter : ImageAdapter {

        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter)markupWriter;
            String source = Control.ImageUrl;
            String text   = Control.AlternateText;
            writer.EnterStyle(Control.ControlStyle);

            // writer.EnterLayout(Style);

            if (source != null && source.Length == 0) {
                // Just write the alternate as text
                writer.WriteEncodedText(text);
            }
            else {
                String localSource;

                string symbolProtocol = "symbol:"; 
                if (StringUtil.StringStartsWith(source, symbolProtocol)) {
                    localSource = source.Substring(symbolProtocol.Length);
                    source = String.Empty;
                }
                else {
                    localSource = null;
                    // AUI 3652
                    source = Control.ResolveClientUrl(source);
                }
                writer.RenderImage(source, localSource, text);
            }
            writer.ExitStyle(Control.ControlStyle);
        }
    }
}

#endif

