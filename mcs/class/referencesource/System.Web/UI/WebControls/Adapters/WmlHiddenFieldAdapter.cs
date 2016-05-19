//------------------------------------------------------------------------------
// <copyright file="WmlHiddenFieldAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web.UI.Adapters;
    using System.Web.UI.WebControls;

    public class WmlHiddenFieldAdapter : HiddenFieldAdapter {

        protected internal override void BeginRender(HtmlTextWriter writer) {
        }

        protected internal override void EndRender(HtmlTextWriter writer) {
        }

        protected internal override void Render(HtmlTextWriter markupWriter) {
            WmlTextWriter writer = (WmlTextWriter)markupWriter;
            ((WmlPageAdapter)PageAdapter).RegisterPostField(writer, Control.UniqueID, Control.Value, false, false);
        }
    }
}

#endif 
