//------------------------------------------------------------------------------
// <copyright file="FormViewInsertEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Provides data for some <see cref='System.Web.UI.WebControls.FormView'/> events.</para>
    /// </devdoc>
    public class FormViewInsertEventArgs : CancelEventArgs {

        private object _commandArgument;
        private OrderedDictionary _values;
        

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.FormViewInsertEventArgs'/>
        /// class.</para>
        /// </devdoc>
        public FormViewInsertEventArgs(object commandArgument) : base(false) {
            this._commandArgument = commandArgument;
        }
        

        /// <devdoc>
        /// <para>Gets the argument to the command posted to the <see cref='System.Web.UI.WebControls.FormView'/>. This property is read-only.</para>
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

