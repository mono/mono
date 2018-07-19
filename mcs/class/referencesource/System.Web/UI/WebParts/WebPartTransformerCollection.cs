//------------------------------------------------------------------------------
// <copyright file="WebPartTransformerCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;

    public sealed class WebPartTransformerCollection : CollectionBase {

        private bool _readOnly;

        public bool IsReadOnly {
            get {
                return _readOnly;
            }
        }

        public WebPartTransformer this[int index] {
            get {
                return (WebPartTransformer) List[index];
            }
            set {
                List[index] = value;
            }
        }

        public int Add(WebPartTransformer transformer) {
            return List.Add(transformer);
        }

        private void CheckReadOnly() {
            if (_readOnly) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartTransformerCollection_ReadOnly));
            }
        }

        public bool Contains(WebPartTransformer transformer) {
            return List.Contains(transformer);
        }

        public void CopyTo(WebPartTransformer[] array, int index) {
            List.CopyTo(array, index);
        }

        public int IndexOf(WebPartTransformer transformer) {
            return List.IndexOf(transformer);
        }

        public void Insert(int index, WebPartTransformer transformer) {
            List.Insert(index, transformer);
        }

        protected override void OnClear() {
            CheckReadOnly();
            base.OnClear();
        }

        protected override void OnInsert(int index, object value) {
            CheckReadOnly();

            if (List.Count > 0) {
                throw new InvalidOperationException(SR.GetString(SR.WebPartTransformerCollection_NotEmpty));
            }

            base.OnInsert(index, value);
        }

        protected override void OnRemove(int index, object value) {
            CheckReadOnly();
            base.OnRemove(index, value);
        }

        protected override void OnSet(int index, object oldValue, object newValue) {
            CheckReadOnly();
            base.OnSet(index, oldValue, newValue);
        }

        protected override void OnValidate(object value) {
            base.OnValidate(value);
            if (value == null) {
                throw new ArgumentNullException("value", SR.GetString(SR.Collection_CantAddNull));
            }
            if (!(value is WebPartTransformer)) {
                throw new ArgumentException(SR.GetString(SR.Collection_InvalidType, "WebPartTransformer"), "value");
            }
        }

        public void Remove(WebPartTransformer transformer) {
            List.Remove(transformer);
        }

        internal void SetReadOnly() {
            _readOnly = true;
        }
    }
}
