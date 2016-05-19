//------------------------------------------------------------------------------
// <copyright file="ListViewInsertEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.WebControls {

    public class ListViewInsertEventArgs : CancelEventArgs {
        private ListViewItem _item;
        private OrderedDictionary _values;

        public ListViewInsertEventArgs(ListViewItem item) : base(false) {
            _item = item;
        }

        /// <devdoc>
        /// <para>Gets the ListViewItem containing the insert item. This property is read-only.</para>
        /// </devdoc>
        public ListViewItem Item {
            get {
                return _item;
            }
        }


        /// <devdoc>
        /// <para>Gets a OrderedDictionary to populate with inserted row values.  This property is read-only.</para>
        /// </devdoc>
        public IOrderedDictionary Values {
            get {
                if (_values == null) {
                    _values = new OrderedDictionary();
                }
                return _values;
            }
        }
    }
}
