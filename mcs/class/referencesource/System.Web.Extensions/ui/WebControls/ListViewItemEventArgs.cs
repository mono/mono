//------------------------------------------------------------------------------
// <copyright file="ListViewItemEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace System.Web.UI.WebControls {

    /// <summary>
    /// Summary description for ListViewItemEventArgs
    /// </summary>
    public class ListViewItemEventArgs : EventArgs {
        private ListViewItem _item;

        public ListViewItemEventArgs(ListViewItem item) {
            _item = item;
        }

        public ListViewItem Item {
            get {
                return _item;
            }
        }
    }
}
