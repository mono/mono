//------------------------------------------------------------------------------
// <copyright file="ListViewCancelEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.WebControls {

    public class ListViewCancelEventArgs : CancelEventArgs {
        private int _itemIndex;
        private ListViewCancelMode _cancelMode;

        public ListViewCancelEventArgs(int itemIndex, ListViewCancelMode cancelMode) : base(false) {
            _itemIndex = itemIndex;
            _cancelMode = cancelMode;
        }

        public int ItemIndex {
            get {
                return _itemIndex;
            }
        }

        public ListViewCancelMode CancelMode {
            get {
                return _cancelMode;
            }
        }
    }
}
