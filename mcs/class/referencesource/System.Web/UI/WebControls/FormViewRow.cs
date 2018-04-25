//------------------------------------------------------------------------------
// <copyright file="FormViewRow.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Represents an individual row in the <see cref='System.Web.UI.WebControls.FormView'/>.</para>
    /// </devdoc>
    public class FormViewRow : TableRow {

        private int _itemIndex;
        private DataControlRowType _rowType;
        private DataControlRowState _rowState;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.FormViewRow'/> class.</para>
        /// </devdoc>
        public FormViewRow(int itemIndex, DataControlRowType rowType, DataControlRowState rowState) {
            this._itemIndex = itemIndex;
            this._rowType = rowType;
            this._rowState = rowState;
            RenderTemplateContainer = true;
        }


        /// <devdoc>
        /// <para>Indicates the index of the item in the <see cref='System.Web.UI.WebControls.FormView'/>. This property is 
        ///    read-only.</para>
        /// </devdoc>
        public virtual int ItemIndex {
            get {
                return _itemIndex;
            }
        }


        /// <devdoc>
        /// <para>Indicates the type of the row in the <see cref='System.Web.UI.WebControls.FormView'/>.</para>
        /// </devdoc>
        public virtual DataControlRowState RowState {
            get {
                return _rowState;
            }
        }


        /// <devdoc>
        /// <para>Indicates the type of the row in the <see cref='System.Web.UI.WebControls.FormView'/>.</para>
        /// </devdoc>
        public virtual DataControlRowType RowType {
            get {
                return _rowType;
            }
        }

        internal bool RenderTemplateContainer { get; set; }

        protected internal override void Render(HtmlTextWriter writer) {
            if (RenderTemplateContainer) {
                //render the table row normally
                base.Render(writer);
            } else {
                //render the contents of the cells
                foreach (TableCell cell in Cells) {
                    cell.RenderContents(writer);
                }
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            if (e is CommandEventArgs) {
                FormViewCommandEventArgs args = new FormViewCommandEventArgs(source, (CommandEventArgs)e);

                RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }
    }
}

