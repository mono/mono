//------------------------------------------------------------------------------
// <copyright file="DataGridLinkButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Drawing;
    using System.Web.Util;


    /// <devdoc>
    ///  Derived version of LinkButton used within a DataGrid.
    /// </devdoc>
    [SupportsEventValidation]
    internal sealed class DataGridLinkButton : LinkButton {
        
        internal DataGridLinkButton() {}
        
        protected internal override void Render(HtmlTextWriter writer) {
            SetForeColor();
            base.Render(writer);
        }


        /// <devdoc>
        ///  In HTML hyperlinks always use the browser's link color.
        ///  For the DataGrid, we want all LinkButtons to honor the ForeColor setting.
        ///  This requires looking up into the control hierarchy to see if either the cell
        ///  or the containing row or table define a ForeColor.
        /// </devdoc>
        private void SetForeColor() {
            if (ControlStyle.IsSet(System.Web.UI.WebControls.Style.PROP_FORECOLOR) == false) {
                Color hyperLinkForeColor;
                Control control = this;

                for (int i = 0; i < 3; i++) {
                    control = control.Parent;

                    Debug.Assert(((i == 0) && (control is TableCell)) ||
                                 ((i == 1) && (control is TableRow)) ||
                                 ((i == 2) && (control is Table)));
                    hyperLinkForeColor = ((WebControl)control).ForeColor;
                    if (hyperLinkForeColor != Color.Empty) {
                        ForeColor = hyperLinkForeColor;
                        break;
                    }
                }
            }
        }
    }
}

