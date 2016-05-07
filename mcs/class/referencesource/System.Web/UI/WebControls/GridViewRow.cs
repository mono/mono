//------------------------------------------------------------------------------
// <copyright file="GridViewRow.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Represents an individual row in the <see cref='System.Web.UI.WebControls.GridView'/>.</para>
    /// </devdoc>
    public class GridViewRow : TableRow, IDataItemContainer {

        private int _rowIndex;
        private int _dataItemIndex;
        private DataControlRowType _rowType;
        private DataControlRowState _rowState;
        private object _dataItem;



        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.GridViewRow'/> class.</para>
        /// </devdoc>
        public GridViewRow(int rowIndex, int dataItemIndex, DataControlRowType rowType, DataControlRowState rowState) {
            this._rowIndex = rowIndex;
            this._dataItemIndex = dataItemIndex;
            this._rowType = rowType;
            this._rowState = rowState;
        }



        /// <devdoc>
        /// <para>Represents an item in the <see cref='System.Web.UI.WebControls.GridView'/>. </para>
        /// </devdoc>
        public virtual object DataItem {
            get {
                return _dataItem;
            }
            set {
                _dataItem = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the data set index number. This property is read-only.</para>
        /// </devdoc>
        public virtual int DataItemIndex {
            get {
                return _dataItemIndex;
            }
        }


        /// <devdoc>
        /// <para>Indicates the index of the row in the <see cref='System.Web.UI.WebControls.GridView'/>. This property is 
        ///    read-only.</para>
        /// </devdoc>
        public virtual int RowIndex {
            get {
                return _rowIndex;
            }
        }


        /// <devdoc>
        /// <para>Indicates the type of the row in the <see cref='System.Web.UI.WebControls.GridView'/>.</para>
        /// </devdoc>
        public virtual DataControlRowState RowState {
            get {
                return _rowState;
            }
            set {
                _rowState = value;
            }
        }


        /// <devdoc>
        /// <para>Indicates the type of the row in the <see cref='System.Web.UI.WebControls.GridView'/>.</para>
        /// </devdoc>
        public virtual DataControlRowType RowType {
            get {
                return _rowType;
            }
            set {
                _rowType = value;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            if (e is CommandEventArgs) {
                GridViewCommandEventArgs args = new GridViewCommandEventArgs(this, source, (CommandEventArgs)e);

                RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        object IDataItemContainer.DataItem {
            get {
                return DataItem;
            }
        }

        int IDataItemContainer.DataItemIndex {
            get {
                return DataItemIndex;
            }
        }

        int IDataItemContainer.DisplayIndex {
            get {
                return RowIndex;
            }
        }
    }
}

