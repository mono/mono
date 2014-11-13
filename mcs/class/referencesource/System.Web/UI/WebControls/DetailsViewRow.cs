//------------------------------------------------------------------------------
// <copyright file="DetailsViewRow.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Represents an individual row in the <see cref='System.Web.UI.WebControls.DetailsView'/>.</para>
    /// </devdoc>
    public class DetailsViewRow : TableRow {

        private int _rowIndex;
        private DataControlRowType _rowType;
        private DataControlRowState _rowState;
        


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DetailsViewRow'/> class.</para>
        /// </devdoc>
        public DetailsViewRow(int rowIndex, DataControlRowType rowType, DataControlRowState rowState) {
            this._rowIndex = rowIndex;
            this._rowType = rowType;
            this._rowState = rowState;
        }


        /// <devdoc>
        /// <para>Indicates the index of the item in the <see cref='System.Web.UI.WebControls.DetailsView'/>. This property is 
        ///    read-only.</para>
        /// </devdoc>
        public virtual int RowIndex {
            get {
                return _rowIndex;
            }
        }


        /// <devdoc>
        /// <para>Indicates the type of the row in the <see cref='System.Web.UI.WebControls.DetailsView'/>.</para>
        /// </devdoc>
        public virtual DataControlRowState RowState {
            get {
                return _rowState;
            }
        }


        /// <devdoc>
        /// <para>Indicates the type of the row in the <see cref='System.Web.UI.WebControls.DetailsView'/>.</para>
        /// </devdoc>
        public virtual DataControlRowType RowType {
            get {
                return _rowType;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            if (e is CommandEventArgs) {
                DetailsViewCommandEventArgs args = new DetailsViewCommandEventArgs(source, (CommandEventArgs)e);

                RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }
    }
}

