//------------------------------------------------------------------------------
// <copyright file="WmlFileUploadAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web.UI.Adapters;
    using System.Web.UI.WebControls;

    public class WmlFileUploadAdapter : FileUploadAdapter {

        protected internal override void Render(HtmlTextWriter writer) {
            String alternateText = Control.AlternateText;
            if (!String.IsNullOrEmpty(alternateText)) {
                writer.EnterStyle(Control.ControlStyle);
                writer.Write(LiteralControlAdapterUtility.ProcessWmlLiteralText(alternateText));
                writer.ExitStyle(Control.ControlStyle);
            }
        }
    }
}

#endif

