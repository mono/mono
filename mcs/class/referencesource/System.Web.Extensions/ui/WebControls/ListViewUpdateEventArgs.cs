//------------------------------------------------------------------------------
// <copyright file="ListViewUpdateEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.WebControls {

    public class ListViewUpdateEventArgs : CancelEventArgs {
        private int _itemIndex;
        private OrderedDictionary _values;
        private OrderedDictionary _keys;
        private OrderedDictionary _oldValues;

        public ListViewUpdateEventArgs(int itemIndex) {
            _itemIndex = itemIndex;
        }

        /// <devdoc>
        /// <para>Gets the int argument to the command posted to the <see cref='System.Web.UI.WebControls.ListView'/>. This property is read-only.</para>
        /// </devdoc>
        public int ItemIndex {
            get {
                return _itemIndex;
            }
        }

        /// <devdoc>
        /// <para>Gets a keyed list to populate with updated row values.  This property is read-only.</para>
        /// </devdoc>
        public IOrderedDictionary Keys {
            get {
                if (_keys == null) {
                    _keys = new OrderedDictionary();
                }
                return _keys;
            }
        }


        /// <devdoc>
        /// <para>Gets a OrderedDictionary to populate with updated row values.  This property is read-only.</para>
        /// </devdoc>
        public IOrderedDictionary NewValues {
            get {
                if (_values == null) {
                    _values = new OrderedDictionary();
                }
                return _values;
            }
        }


        /// <devdoc>
        /// <para>Gets a OrderedDictionary to populate with pre-edit row values.  This property is read-only.</para>
        /// </devdoc>
        public IOrderedDictionary OldValues {
            get {
                if (_oldValues == null) {
                    _oldValues = new OrderedDictionary();
                }
                return _oldValues;
            }
        }
    }
}
