//------------------------------------------------------------------------------
// <copyright file="ListViewItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls {   
    [
    ToolboxItem(false)
    ]
    // ListViewItem is an IDataItemContainer so that we can have controls that databind in the ListView's InsertItem.
    public class ListViewItem : Control, INamingContainer, IDataItemContainer {

        private ListViewItemType _itemType;

        public ListViewItem(ListViewItemType itemType) {
            _itemType = itemType;
        }

        public ListViewItemType ItemType {
            get {
                return _itemType;
            }
        }

        // DataItem in will always return null.
        public virtual object DataItem {
            get;
            set;
        }


        public virtual int DataItemIndex {
            get {
                return -1;
            }
        }

        public virtual int DisplayIndex {
            get {
                return -1;
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "1#")]
        protected override bool OnBubbleEvent(object source, EventArgs e) {
            if (e is CommandEventArgs) {
                ListViewCommandEventArgs args = new ListViewCommandEventArgs(this, source, (CommandEventArgs)e);
                RaiseBubbleEvent(this, args);
                return true;
            }
            return false;
        }
    }
}
