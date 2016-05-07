//------------------------------------------------------------------------------
// <copyright file="LayoutTable.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Collections;
    using System.Security.Permissions;
    using System.Web;


    /// <devdoc>
    /// Table used for laying out controls in a Render method.  Doesn't parent added controls, so
    /// it is safe to add child controls to this table.  Sets page of added controls if not already set.
    /// </devdoc>
    [SupportsEventValidation]
    internal sealed class LayoutTable : Table {


        public LayoutTable(int rows, int columns, Page page) {
            if (rows <= 0) {
                throw new ArgumentOutOfRangeException("rows");
            }
            if (columns <= 0) {
                throw new ArgumentOutOfRangeException("columns");
            }

            // page may be null in the designer
            if (page != null) {
                this.Page = page;
            }

            for (int r = 0; r < rows; r++) {
                TableRow row = new TableRow();
                Rows.Add(row);
                for (int c = 0; c < columns; c++) {
                    TableCell cell = new LayoutTableCell();
                    row.Cells.Add(cell);
                }
            }
        }


        public TableCell this[int row, int column] {
            get {
                return (TableCell)Rows[row].Cells[column];
            }
        }
    }
}

