//------------------------------------------------------------------------------
// <copyright file="ProxyWebPartConnectionCollection.cs" company="Microsoft">
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

    [
    Editor("System.ComponentModel.Design.CollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor))
    ]
    public sealed class ProxyWebPartConnectionCollection : CollectionBase {

        private WebPartManager _webPartManager;

        public bool IsReadOnly {
            get {
                if (_webPartManager != null) {
                    return _webPartManager.StaticConnections.IsReadOnly;
                }
                else {
                    // This collection is never read-only before _webPartManager is set
                    return false;
                }
            }
        }

        public WebPartConnection this[int index] {
            get {
                return (WebPartConnection)List[index];
            }
            set {
                List[index] = value;
            }
        }

        // Returns the WebPartConnection with the specified id, performing a case-insensitive comparison.
        // Returns null if there are no matches.
        public WebPartConnection this[string id] {
            // PERF: Use a hashtable for lookup, instead of a linear search
            get {
                foreach (WebPartConnection connection in List) {
                    if (connection != null && String.Equals(connection.ID, id, StringComparison.OrdinalIgnoreCase)) {
                        return connection;
                    }
                }

                return null;
            }
        }

        public int Add(WebPartConnection value) {
            return List.Add(value);
        }

        private void CheckReadOnly() {
            if (IsReadOnly) {
                throw new InvalidOperationException(SR.GetString(SR.ProxyWebPartConnectionCollection_ReadOnly));
            }
        }

        public bool Contains(WebPartConnection value) {
            return List.Contains(value);
        }

        public void CopyTo(WebPartConnection[] array, int index) {
            List.CopyTo(array, index);
        }

        public int IndexOf(WebPartConnection value) {
            return List.IndexOf(value);
        }

        public void Insert(int index, WebPartConnection value) {
            List.Insert(index, value);
        }

        protected override void OnClear() {
            CheckReadOnly();
            if (_webPartManager != null) {
                // Remove all of the connections in this collection from the main WebPartManager
                foreach (WebPartConnection connection in this) {
                    _webPartManager.StaticConnections.Remove(connection);
                }
            }
            base.OnClear();
        }

        protected override void OnInsert(int index, object value) {
            CheckReadOnly();
            if (_webPartManager != null) {
                _webPartManager.StaticConnections.Insert(index, (WebPartConnection)value);
            }
            base.OnInsert(index, value);
        }

        protected override void OnRemove(int index, object value) {
            CheckReadOnly();
            if (_webPartManager != null) {
                _webPartManager.StaticConnections.Remove((WebPartConnection)value);
            }
            base.OnRemove(index, value);
        }

        protected override void OnSet(int index, object oldValue, object newValue) {
            CheckReadOnly();
            if (_webPartManager != null) {
                int webPartManagerIndex = _webPartManager.StaticConnections.IndexOf((WebPartConnection)oldValue);
                // It is a 
                Debug.Assert(webPartManagerIndex >= 0);
                _webPartManager.StaticConnections[webPartManagerIndex] = (WebPartConnection)newValue;
            }
            base.OnSet(index, oldValue, newValue);
        }

        // Validates that an object is a WebPartConnection.
        protected override void OnValidate(object value) {
            base.OnValidate(value);
            if (value == null) {
                throw new ArgumentNullException("value", SR.GetString(SR.Collection_CantAddNull));
            }
            if (!(value is WebPartConnection)) {
                throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "WebPartConnection"));
            }
        }

        public void Remove(WebPartConnection value) {
            List.Remove(value);
        }

        internal void SetWebPartManager(WebPartManager webPartManager) {
            // This method should only be called once in the lifetime of the ProxyWebPartConnectionCollection
            Debug.Assert(_webPartManager == null);

            Debug.Assert(webPartManager != null);

            _webPartManager = webPartManager;

            // When the _webPartManager is first set, add all the connections in this collection
            // to the main WebPartManager
            foreach (WebPartConnection connection in this) {
                _webPartManager.StaticConnections.Add(connection);
            }
        }
    }
}

