//------------------------------------------------------------------------------
// <copyright file="WmlBaseValidatorAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

#if WMLSUPPORT

namespace System.Web.UI.WebControls.Adapters {
    using System.Web.UI.WebControls;

    public class WmlBaseValidatorAdapter : WmlLabelAdapter {

        protected new BaseValidator Control {
            get {
                return (BaseValidator)base.Control;
            }
        }

        // Renders the control only if the control is evaluated as invalid.
        protected internal override void Render(HtmlTextWriter writer) {
            if (Control.Enabled &&
                !Control.IsValid &&
                Control.Display != ValidatorDisplay.None) {

                if (Control.Text.Trim().Length == 0 && !Control.HasControls()) {
                    Control.Text = Control.ErrorMessage;
                }
                base.Render(writer);
            }
        }
    }
}

#endif
