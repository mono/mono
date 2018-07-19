//------------------------------------------------------------------------------
// <copyright file="DetailsViewUpdateEventArgs.cs" company="Microsoft">
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
    public class DetailsViewUpdateEventArgs : CancelEventArgs {

        private object _commandArgument;
        private OrderedDictionary _values;
        private OrderedDictionary _keys;
        private OrderedDictionary _oldValues;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DetailsViewUpdateEventArgs'/>
        /// class.</para>
        /// </devdoc>
        public DetailsViewUpdateEventArgs(object commandArgument) : base(false) {
            this._commandArgument = commandArgument;
        }
        

        /// <devdoc>
        /// <para>Gets the argument to the command posted to the <see cref='System.Web.UI.WebControls.DetailsView'/>. This property is read-only.</para>
        /// </devdoc>
        public object CommandArgument {
            get {
                return _commandArgument;
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

