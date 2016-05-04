//------------------------------------------------------------------------------
// <copyright file="WmlPanelAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System;
    using System.IO;
    using System.Web;

    // Provides adaptive rendering for the Panel control.
    public class WmlPanelAdapter : PanelAdapter {

        protected internal override void BeginRender(HtmlTextWriter writer) {
            ((WmlTextWriter)writer).PushLayout(Control.HorizontalAlign, Control.Wrap);
            // Children will call BeginRender.
            // writer.BeginRender();
        }
        
        // Renders the control.
        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter)markupWriter;
            // Review: In our literalControl transformation, we suppress p's at beginning of a form or a panel.  
            // This saves real estate, and in practice it tends to look much better.  If the developer really wants a
            // break at the beginning of a panel, they can use <br/> to accomplish this.
            writer.BeginFormOrPanel();
            writer.PushPanelStyle(Control.ControlStyle); // to be written after next opening p tag.
            RenderChildren(writer);
            writer.PopPanelStyle();
            writer.PopLayout();
        }
    }
}

#endif
