//------------------------------------------------------------------------------
// <copyright file="WebPartDisplayModeCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;

    public sealed class WebPartDisplayModeCollection : CollectionBase {
        private bool _readOnly;
        private string _readOnlyExceptionMessage;

        // Prevent instantiation outside of our assembly.  We want third-part code to use the collection
        // returned by the base method, not create a new collection.
        internal WebPartDisplayModeCollection() {
        }

        public bool IsReadOnly {
            get {
                return _readOnly;
            }
        }

        public WebPartDisplayMode this[int index] {
            get {
                return (WebPartDisplayMode)List[index];
            }
        }

        public WebPartDisplayMode this[string modeName] {
            get {
                foreach (WebPartDisplayMode displayMode in List) {
                    if (String.Equals(displayMode.Name, modeName, StringComparison.OrdinalIgnoreCase)) {
                        return displayMode;
                    }
                }

                return null;
            }
        }

        public int Add(WebPartDisplayMode value) {
            return List.Add(value);
        }

        internal int AddInternal(WebPartDisplayMode value) {
            bool isReadOnly = _readOnly;

            _readOnly = false;
            // Extra try-catch block to prevent elevation of privilege attack via exception filter
            try {
                try {
                    return List.Add(value);
                }
                finally {
                    _readOnly = isReadOnly;
                }
            }
            catch {
                throw;
            }
        }

        private void CheckReadOnly() {
            if (_readOnly) {
                throw new InvalidOperationException(SR.GetString(_readOnlyExceptionMessage));
            }
        }

        public bool Contains(WebPartDisplayMode value) {
            return List.Contains(value);
        }

        public void CopyTo(WebPartDisplayMode[] array, int index) {
            List.CopyTo(array, index);
        }

        public int IndexOf(WebPartDisplayMode value) {
            return List.IndexOf(value);
        }

        public void Insert(int index, WebPartDisplayMode value) {
            List.Insert(index, value);
        }

        protected override void OnClear() {
            throw new InvalidOperationException(SR.GetString(SR.WebPartDisplayModeCollection_CantRemove));
        }

        protected override void OnInsert(int index, object value) {
            CheckReadOnly();
            WebPartDisplayMode displayMode = (WebPartDisplayMode)value;
            foreach (WebPartDisplayMode existingDisplayMode in List) {
                if (displayMode.Name == existingDisplayMode.Name) {
                    throw new ArgumentException(SR.GetString(SR.WebPartDisplayModeCollection_DuplicateName, displayMode.Name));
                }
            }
            base.OnInsert(index, value);
        }

        protected override void OnRemove(int index, object value) {
            throw new InvalidOperationException(SR.GetString(SR.WebPartDisplayModeCollection_CantRemove));
        }

        protected override void OnSet(int index, object oldValue, object newValue) {
            throw new InvalidOperationException(SR.GetString(SR.WebPartDisplayModeCollection_CantSet));
        }

        protected override void OnValidate(object value) {
            base.OnValidate(value);
            if (value == null) {
                throw new ArgumentNullException("value", SR.GetString(SR.Collection_CantAddNull));
            }
            if (!(value is WebPartDisplayMode)) {
                throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "WebPartDisplayMode"), "value");
            }
        }

        internal void SetReadOnly(string exceptionMessage) {
            _readOnlyExceptionMessage = exceptionMessage;
            _readOnly = true;
        }
    }
}
