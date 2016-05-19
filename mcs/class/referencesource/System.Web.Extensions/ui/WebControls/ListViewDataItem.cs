//------------------------------------------------------------------------------
// <copyright file="ListViewDataItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls {
    public class ListViewDataItem : ListViewItem {

        private int _dataItemIndex;
        private int _displayIndex;
        private object _dataItem;

        public ListViewDataItem(int dataItemIndex, int displayIndex)
            : base(ListViewItemType.DataItem) {
            _dataItemIndex = dataItemIndex;
            _displayIndex = displayIndex;
        }

        public override object DataItem {
            get {
                return _dataItem;
            }
            set {
                _dataItem = value;
            }
        }

        public override int DataItemIndex {
            get {
                return _dataItemIndex;
            }
        }

        public override int DisplayIndex {
            get {
                return _displayIndex;
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
