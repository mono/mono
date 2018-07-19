//------------------------------------------------------------------------------
// <copyright file="GridViewUpdateEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Provides data for some <see cref='System.Web.UI.WebControls.GridView'/> events.</para>
    /// </devdoc>
    public class GridViewUpdateEventArgs : CancelEventArgs {

        private int _rowIndex;
        private OrderedDictionary _values;
        private OrderedDictionary _keys;
        private OrderedDictionary _oldValues;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.GridViewUpdateEventArgs'/>
        /// class.</para>
        /// </devdoc>
        public GridViewUpdateEventArgs(int rowIndex) : base(false) {
            this._rowIndex = rowIndex;
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

        /// <devdoc>
        /// <para>Gets the int argument to the command posted to the <see cref='System.Web.UI.WebControls.GridView'/>. This property is read-only.</para>
        /// </devdoc>
        public int RowIndex {
            get {
                return _rowIndex;
            }
        }
    }
}

