//------------------------------------------------------------------------------
// <copyright file="WebPartConnectionCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web.Util;

    // 
    [
    Editor("System.ComponentModel.Design.CollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))
    ]
    public sealed class WebPartConnectionCollection : CollectionBase {

        private bool _readOnly;
        private string _readOnlyExceptionMessage;
        private WebPartManager _webPartManager;

        internal WebPartConnectionCollection(WebPartManager webPartManager) {
            _webPartManager = webPartManager;
        }

        public bool IsReadOnly {
            get {
                return _readOnly;
            }
        }

        /// <devdoc>
        /// Returns the WebPartConnection at a given index.
        /// </devdoc>
        public WebPartConnection this[int index] {
            get {
                return (WebPartConnection)List[index];
            }
            set {
                List[index] = value;
            }
        }

        /// <devdoc>
        /// Returns the WebPartConnection with the specified id, performing a case-insensitive comparison.
        /// Returns null if there are no matches.
        /// </devdoc>
        public WebPartConnection this[string id] {
            // PERF: Use a hashtable for lookup, instead of a linear search
            get {
                foreach (WebPartConnection connection in List) {
                    if (String.Equals(connection.ID, id, StringComparison.OrdinalIgnoreCase)) {
                        return connection;
                    }
                }

                return null;
            }
        }

        /// <devdoc>
        /// Adds a Connection to the collection.
        /// </devdoc>
        public int Add(WebPartConnection value) {
            return List.Add(value);
        }

        private void CheckReadOnly() {
            if (_readOnly) {
                throw new InvalidOperationException(SR.GetString(_readOnlyExceptionMessage));
            }
        }

        /// <devdoc>
        /// True if the WebPartConnection is contained in the collection.
        /// </devdoc>
        public bool Contains(WebPartConnection value) {
            return List.Contains(value);
        }

        internal bool ContainsProvider(WebPart provider) {
            foreach (WebPartConnection connection in List) {
                if (connection.Provider == provider) {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(WebPartConnection[] array, int index) {
            List.CopyTo(array, index);
        }

        public int IndexOf(WebPartConnection value) {
            return List.IndexOf(value);
        }

        /// <devdoc>
        /// Inserts a WebPartConnection into the collection.
        /// </devdoc>
        public void Insert(int index, WebPartConnection value) {
            List.Insert(index, value);
        }

        /// <devdoc>
        /// </devdoc>
        protected override void OnClear() {
            CheckReadOnly();
            base.OnClear();
        }

        protected override void OnInsert(int index, object value) {
            CheckReadOnly();
            ((WebPartConnection)value).SetWebPartManager(_webPartManager);
            base.OnInsert(index, value);
        }

        protected override void OnRemove(int index, object value) {
            CheckReadOnly();
            ((WebPartConnection)value).SetWebPartManager(null);
            base.OnRemove(index, value);
        }

        protected override void OnSet(int index, object oldValue, object newValue) {
            CheckReadOnly();
            ((WebPartConnection)oldValue).SetWebPartManager(null);
            ((WebPartConnection)newValue).SetWebPartManager(_webPartManager);
            base.OnSet(index, oldValue, newValue);
        }

        /// <devdoc>
        /// Validates that an object is a WebPartConnection.
        /// </devdoc>
        protected override void OnValidate(object value) {
            base.OnValidate(value);
            if (value == null) {
                throw new ArgumentNullException("value", SR.GetString(SR.Collection_CantAddNull));
            }
            if (!(value is WebPartConnection)) {
                throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "WebPartConnection"), "value");
            }
        }

        /// <devdoc>
        /// Removes a WebPartConnection from the collection.
        /// </devdoc>
        public void Remove(WebPartConnection value) {
            List.Remove(value);
        }

        /// <devdoc>
        /// Marks the collection readonly. This is useful, because we assume that the list
        /// of static connections is known at Init time, and new connections are added
        /// through the WebPartManager.
        /// </devdoc>
        internal void SetReadOnly(string exceptionMessage) {
            _readOnlyExceptionMessage = exceptionMessage;
            _readOnly = true;
        }
    }
}
