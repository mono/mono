//------------------------------------------------------------------------------
// <copyright file="WmlLabelAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web.UI.Adapters;
    using System.Web.UI.WebControls;

    public class WmlLabelAdapter : LabelAdapter {

        protected internal override void Render(HtmlTextWriter markupWriter) {
            markupWriter.EnterStyle(Control.ControlStyle);
            markupWriter.Write(LiteralControlAdapterUtility.ProcessWmlLiteralText(Control.Text));
            markupWriter.ExitStyle(Control.ControlStyle);
        }
    }
}

#endif

