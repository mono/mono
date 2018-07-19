//------------------------------------------------------------------------------
// <copyright file="WmlLiteralControlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.Adapters {
    using System;
    using System.Web;
    using System.Web.UI;
    using System.Text.RegularExpressions;

    public class WmlLiteralControlAdapter : LiteralControlAdapter {

        protected internal override void BeginRender(HtmlTextWriter writer) {
        }

        protected internal override void EndRender(HtmlTextWriter writer) {
        }

        // BUGBUG: This override is for compatibility with MMIT.
        // MMIT legacy pages also use this adapter -UNDONE: Review once MMIT legacy plan is complete.
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
            ((WmlPageAdapter)PageAdapter).RenderTransformedText(writer, Control.Text);
        }
    }
}

#endif
