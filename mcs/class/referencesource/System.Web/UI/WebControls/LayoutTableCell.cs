//------------------------------------------------------------------------------
// <copyright file="LayoutTableCell.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Security.Permissions;
    using System.Web;

    /// <devdoc>
    /// Table cell used for laying out controls in a Render method.  Doesn't parent added controls, so
    /// it is safe to add child controls to this table.  Sets page of added controls if not already set.
    /// Used by LayoutTable.  Top-level class instead of private so LayoutTableCells can be added dynamically
    /// to LayoutTable.
    /// </devdoc>
    internal sealed class LayoutTableCell : TableCell {

        protected internal override void AddedControl(Control control, int index) {
            if (control.Page == null) {
                control.Page = Page;
            }
        }

        protected internal override void RemovedControl(Control control) {
        }
    }
}

