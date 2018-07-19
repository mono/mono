//------------------------------------------------------------------------------
// <copyright file="DataControlPagerLinkButton.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Drawing;
    using System.Web.Util;


    /// <devdoc>
    ///  Derived version of LinkButton used within a DataControl.
    /// </devdoc>
    [SupportsEventValidation]
    internal class DataControlPagerLinkButton : DataControlLinkButton {
        
        internal DataControlPagerLinkButton(IPostBackContainer container) : base(container) {
        }

        public override bool CausesValidation {
            get {
                return false;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.CannotSetValidationOnPagerButtons));
            }
        }

        /// <devdoc>
        ///  In HTML hyperlinks always use the browser's link color.
        ///  For the DataControl, we want all LinkButtons to honor the ForeColor setting.
        ///  This requires looking up into the control hierarchy to see if either the cell
        ///  or the containing row or table define a ForeColor.
        /// </devdoc>
        protected override void SetForeColor() {
            if (ControlStyle.IsSet(System.Web.UI.WebControls.Style.PROP_FORECOLOR) == false) {
                Color hyperLinkForeColor;
                Control control = this;

                for (int i = 0; i < 6; i++) {
                    control = control.Parent;

                    // pager buttons are usually inside a table that's inside the pager row
                    Debug.Assert(((i == 0) && (control is TableCell)) ||
                                 ((i == 1) && (control is TableRow)) ||
                                 ((i == 2) && (control is Table)) ||
                                 ((i == 3) && (control is TableCell)) ||
                                 ((i == 4) && (control is TableRow)) ||
                                 ((i == 5) && (control is Table)));
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

