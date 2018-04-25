//------------------------------------------------------------------------------
// <copyright file="ListViewTableCell.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace System.Web.UI.WebControls {
    /// <summary>
    /// Summary description for ListViewTableCell
    /// </summary>
    internal class ListViewTableCell : HtmlTableCell {
        public ListViewTableCell() {
        }

        protected override ControlCollection CreateControlCollection() {
            return new ControlCollection(this);
        }

        protected internal override void Render(HtmlTextWriter writer) {
            RenderChildren(writer);
        }
    }
}
