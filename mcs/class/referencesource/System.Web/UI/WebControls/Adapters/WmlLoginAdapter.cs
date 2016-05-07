//------------------------------------------------------------------------------
// <copyright file="WmlLoginAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {

    public class WmlLoginAdapter : LoginAdapter {

        // Overridden to render the validator *before* the label, to ensure the validator
        // is presented on the correct screen.
        protected override void RenderInput(HtmlTextWriter writer, Literal label, Control textBox, Control validator) {
            validator.RenderControl(writer);

            // In the Login control, many styles are applied to the table cell that contains
            // a child control, rather than the child control itself.  We must apply the proper
            // style from the Login control to any control we create for adaptive rendering.
            // VSWhidbey 81240
            Label newLabel = new Label();
            newLabel.Text = label.Text;
            newLabel.Page = Page;
            newLabel.ApplyStyle(Control.LabelStyle);
            newLabel.RenderControl(writer);

            // TextBoxStyle is applied directly to the TextBox in the Login control
            textBox.RenderControl(writer);
        }

    }
}

#endif
