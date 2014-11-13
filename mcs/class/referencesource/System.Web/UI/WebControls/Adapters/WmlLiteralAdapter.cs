//------------------------------------------------------------------------------
// <copyright file="WmlLiteralAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.Adapters;

    public class WmlLiteralAdapter : LiteralAdapter {

        protected internal override void BeginRender(HtmlTextWriter writer) {
        }

        protected internal override void EndRender(HtmlTextWriter writer) {
        }

        // 
        protected internal override void Render(HtmlTextWriter writer) {
            WmlTextWriter wmlWriter = writer as WmlTextWriter;
            if (wmlWriter == null) {
                // MMIT legacy case (else pageAdapter would have generated a WmlTextWriter).
                Control.Render(writer);  
                return;            
            }
            Render(wmlWriter);
        }        

        public virtual void Render(WmlTextWriter writer) {
            LiteralMode mode = Control.Mode;
            if (mode == LiteralMode.PassThrough || mode == LiteralMode.Encode) {
                Style emptyStyle = new Style();
                writer.BeginRender();
                writer.EnterStyle(emptyStyle); // VSWhidbey 114083
                
                if (mode == LiteralMode.PassThrough) {
                    writer.Write(Control.Text);
                }
                else /* mode == LiteralMode.Encode */ {
                    writer.WriteEncodedText(Control.Text);
                }
                
                writer.ExitStyle(emptyStyle);
                writer.EndRender();
                return;
            }
            
            /* mode == LiteralMode.Transform */
            ((WmlPageAdapter)PageAdapter).RenderTransformedText(writer, Control.Text);
        }
    }
}

#endif
