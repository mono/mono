//------------------------------------------------------------------------------
// <copyright file="DetailsViewUpdatedEventArgs.cs" company="Microsoft">
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
    public class DetailsViewUpdatedEventArgs : EventArgs {

        private int _affectedRows;
        private Exception _exception;
        private bool _exceptionHandled;
        private bool _keepInEditMode;
        private IOrderedDictionary _values;
        private IOrderedDictionary _keys;
        private IOrderedDictionary _oldValues;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.DetailsViewUpdatedEventArgs'/>
        /// class.</para>
        /// </devdoc>
        public DetailsViewUpdatedEventArgs(int affectedRows, Exception e) {
            this._affectedRows = affectedRows;
            this._exceptionHandled = false;
            this._exception = e;
            this._keepInEditMode = false;
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
        public bool KeepInEditMode {
            get {
                return _keepInEditMode;
            }
            set {
                _keepInEditMode = value;
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
    
        internal void SetKeys(IOrderedDictionary keys) {
            _keys = keys;
        }
        
        internal void SetNewValues(IOrderedDictionary newValues) {
            _values = newValues;
        }
        
        internal void SetOldValues(IOrderedDictionary oldValues) {
            _oldValues = oldValues;
        }
    }
}

