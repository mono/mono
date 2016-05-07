//------------------------------------------------------------------------------
// <copyright file="DetailsViewDeleteEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Provides data for some <see cref='System.Web.UI.WebControls.DetailsView'/> events.</para>
    /// </devdoc>
    public class DetailsViewDeleteEventArgs : CancelEventArgs {

        private int _rowIndex;
        private OrderedDictionary _keys;
        private OrderedDictionary _values;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DetailsViewDeleteEventArgs'/>
        /// class.</para>
        /// </devdoc>
        public DetailsViewDeleteEventArgs(int rowIndex) : base(false) {
           this._rowIndex = rowIndex;
        }


        public int RowIndex {
            get {
                return _rowIndex;
            }
        }


        /// <devdoc>
        /// <para>Gets a keyed list to populate with parameters that identify the row to delete.  This property is read-only.</para>
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
        /// <para>Gets a keyed list to populate with old row values.  This property is read-only.</para>
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

