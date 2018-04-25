//------------------------------------------------------------------------------
// <copyright file="WmlXmlAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web.UI.WebControls;

    public class WmlXmlAdapter : XmlAdapter {

        protected internal override void Render(HtmlTextWriter writer) {
            if (((WmlTextWriter)writer).AnalyzeMode) {
                return;
            }
            HtmlTextWriter w = new HtmlTextWriter(writer);
            Control.Render(w);
        }
    }
}

#endif

