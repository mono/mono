//------------------------------------------------------------------------------
// <copyright file="DataGridItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.Web.UI;

    /// <devdoc>
    /// <para>Represents an individual item in the <see cref='System.Web.UI.WebControls.DataGrid'/>.</para>
    /// </devdoc>
    public class DataGridItem : TableRow, IDataItemContainer {

        private int itemIndex;
        private int dataSetIndex;
        private ListItemType itemType;
        private object dataItem;



        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DataGridItem'/> class.</para>
        /// </devdoc>
        public DataGridItem(int itemIndex, int dataSetIndex, ListItemType itemType) {
            this.itemIndex = itemIndex;
            this.dataSetIndex = dataSetIndex;
            this.itemType = itemType;
        }



        /// <devdoc>
        /// <para>Represents an item in the <see cref='System.Web.UI.WebControls.DataGrid'/>. </para>
        /// </devdoc>
        public virtual object DataItem {
            get {
                return dataItem;
            }
            set {
                dataItem = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the data set index number. This property is read-only.</para>
        /// </devdoc>
        public virtual int DataSetIndex {
            get {
                return dataSetIndex;
            }
        }


        /// <devdoc>
        /// <para>Indicates the index of the item in the <see cref='System.Web.UI.WebControls.DataGrid'/>. This property is 
        ///    read-only.</para>
        /// </devdoc>
        public virtual int ItemIndex {
            get {
                return itemIndex;
            }
        }


        /// <devdoc>
        /// <para>Indicates the type of the item in the <see cref='System.Web.UI.WebControls.DataGrid'/>.</para>
        /// </devdoc>
        public virtual ListItemType ItemType {
            [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get {
                return itemType;
            }
        }



        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            if (e is CommandEventArgs) {
                DataGridCommandEventArgs args = new DataGridCommandEventArgs(this, source, (CommandEventArgs)e);

                RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }


        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        protected internal virtual void SetItemType(ListItemType itemType) {
            this.itemType = itemType;
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
                return DataSetIndex;
            }
        }

        int IDataItemContainer.DisplayIndex {
            get {
                return ItemIndex;
            }
        }
    }
}

