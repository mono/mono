//------------------------------------------------------------------------------
// <copyright file="WmlLiteralControlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.Adapters {
    using System;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.Adapters;

    public class WmlDataBoundLiteralControlAdapter : DataBoundLiteralControlAdapter {

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
            ((WmlPageAdapter)PageAdapter).RenderTransformedText(writer, Control.Text);            
        }
    }
}

#endif
