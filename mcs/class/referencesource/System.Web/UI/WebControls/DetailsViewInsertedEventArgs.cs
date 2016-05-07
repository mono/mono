//------------------------------------------------------------------------------
// <copyright file="DetailsViewInsertedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <devdoc>
    /// <para>Provides data for some <see cref='System.Web.UI.WebControls.DetailsView'/> events.</para>
    /// </devdoc>

    public class DetailsViewInsertedEventArgs : EventArgs {

        private int _affectedRows;
        private Exception _exception;
        private bool _exceptionHandled;
        private bool _keepInInsertMode;
        private IOrderedDictionary _values;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DetailsViewInsertedEventArgs'/>
        /// class.</para>
        /// </devdoc>
        public DetailsViewInsertedEventArgs(int affectedRows, Exception e) {
            this._affectedRows = affectedRows;
            this._exceptionHandled = false;
            this._exception = e;
            this._keepInInsertMode = false;
        }


        /// <devdoc>
        ///    <para>Gets the source of the command. This property is read-only.</para>
        /// </devdoc>
        public int AffectedRows {
            get {
                return _affectedRows;
            }
        }


        /// <devdoc>
        ///    <para>Gets the exception (if any) that occurred during the operation. This property is read-only.</para>
        /// </devdoc>
        public Exception Exception {
            get {
                return _exception;
            }
        }


        /// <devdoc>
        ///    <para>Gets a flag telling whether the exception was handled.</para>
        /// </devdoc>
        public bool ExceptionHandled {
            get {
                return _exceptionHandled;
            }
            set {
                _exceptionHandled = value;
            }
        }
    
        /// <devdoc>
        ///    <para>Gets or sets whether the control should be rebound.</para>
        /// </devdoc>
        public bool KeepInInsertMode {
            get {
                return _keepInInsertMode;
            }
            set {
                _keepInInsertMode = value;
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
    
        internal void SetValues(IOrderedDictionary values) {
            _values = values;
        }
    }
}

