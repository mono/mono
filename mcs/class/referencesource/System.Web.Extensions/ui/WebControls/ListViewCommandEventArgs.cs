//------------------------------------------------------------------------------
// <copyright file="ListViewCommandEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.UI.WebControls {

    public class ListViewCommandEventArgs : CommandEventArgs {
        private ListViewItem _item;
        private object _commandSource;

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "2#")]
        public ListViewCommandEventArgs(ListViewItem item, object commandSource, CommandEventArgs originalArgs) : base(originalArgs) {
            _item = item;
            _commandSource = commandSource;
        }

        public object CommandSource {
            get {
                return _commandSource;
            }
        }

        public ListViewItem Item {
            get {
                return _item;
            }
        }

        public bool Handled { get; set; }

    }
}
