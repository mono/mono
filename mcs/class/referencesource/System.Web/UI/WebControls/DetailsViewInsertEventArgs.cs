//------------------------------------------------------------------------------
// <copyright file="DetailsViewInsertEventArgs.cs" company="Microsoft">
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
    public class DetailsViewInsertEventArgs : CancelEventArgs {

        private object _commandArgument;
        private OrderedDictionary _values;
        

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DetailsViewInsertEventArgs'/>
        /// class.</para>
        /// </devdoc>
        public DetailsViewInsertEventArgs(object commandArgument) : base(false) {
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

