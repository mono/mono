//------------------------------------------------------------------------------
// <copyright file="SiteMapNodeCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Copyright (c) 2002 Microsoft Corporation
 */

namespace System.Web {

    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class SiteMapNodeCollection : IHierarchicalEnumerable, IList {

        internal static SiteMapNodeCollection Empty = new ReadOnlySiteMapNodeCollection(new SiteMapNodeCollection());
        private int _initialSize = 10;
        private ArrayList _innerList;


        public SiteMapNodeCollection() {
        }

        // Create the collection with initial capacity.
        public SiteMapNodeCollection(int capacity) {
            _initialSize = capacity;
        }

        public SiteMapNodeCollection(SiteMapNode value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            _initialSize = 1;
            List.Add(value);
        }


        public SiteMapNodeCollection(SiteMapNode[] value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            _initialSize = value.Length;
            AddRangeInternal(value);
        }


        public SiteMapNodeCollection(SiteMapNodeCollection value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            _initialSize = value.Count;
            AddRangeInternal(value);
        }

        public virtual int Count {
            get {
                return List.Count;
            }
        }

        public virtual bool IsSynchronized {
            get {
                return List.IsSynchronized;
            }
        }

        public virtual object SyncRoot {
            get {
                return List.SyncRoot;
            }
        }

        private ArrayList List {
            get {
                Debug.Assert(!(this is ReadOnlySiteMapNodeCollection), "List should not be called on ReadOnlySiteMapNodeCollection.");

                if (_innerList == null) {
                    _innerList = new ArrayList(_initialSize);
                }

                return _innerList;
            }
        }


        public virtual bool IsFixedSize {
            get { return false; }
        }


        public virtual bool IsReadOnly {
            get { return false; }
        }


        public virtual SiteMapNode this[int index] {
            get {
                return (SiteMapNode)List[index];
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                List[index] = value;
            }
        }


        public virtual int Add(SiteMapNode value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            return List.Add(value);
        }


        public virtual void AddRange(SiteMapNode[] value) {
            AddRangeInternal(value);
        }


        public virtual void AddRange(SiteMapNodeCollection value) {
            AddRangeInternal(value);
        }

        private void AddRangeInternal(IList value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            List.AddRange(value);
        }

        public virtual void Clear() {
            List.Clear();
        }

        public virtual bool Contains(SiteMapNode value) {
            return List.Contains(value);
        }


        public virtual void CopyTo(SiteMapNode[] array, int index) {
            CopyToInternal(array, index);
        }

        internal virtual void CopyToInternal(Array array, int index) {
            List.CopyTo(array, index);
        }

        public SiteMapDataSourceView GetDataSourceView(SiteMapDataSource owner, string viewName) {
            return new SiteMapDataSourceView(owner, viewName, this);
        }

        public virtual IEnumerator GetEnumerator() {
            return List.GetEnumerator();
        }


        public SiteMapHierarchicalDataSourceView GetHierarchicalDataSourceView() {
            return new SiteMapHierarchicalDataSourceView(this);
        }


        public virtual IHierarchyData GetHierarchyData(object enumeratedItem) {
            return enumeratedItem as IHierarchyData;
        }


        public virtual int IndexOf(SiteMapNode value) {
            return List.IndexOf(value);
        }


        public virtual void Insert(int index, SiteMapNode value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            List.Insert(index, value);
        }


        protected virtual void OnValidate(object value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            if (!(value is SiteMapNode)) {
                throw new ArgumentException(
                    SR.GetString(SR.SiteMapNodeCollection_Invalid_Type, value.GetType().ToString()));
            }
        }


        public static SiteMapNodeCollection ReadOnly(SiteMapNodeCollection collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            return new ReadOnlySiteMapNodeCollection(collection);
        }


        public virtual void Remove(SiteMapNode value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            List.Remove(value);
        }

        public virtual void RemoveAt(int index) {
            List.RemoveAt(index);
        }

        #region ICollection implementation
        int ICollection.Count {
            get {
                return Count;
            }
        }

        bool ICollection.IsSynchronized {
            get {
                return IsSynchronized;
            }
        }

        object ICollection.SyncRoot {
            get {
                return SyncRoot;
            }
        }

        void ICollection.CopyTo(Array array, int index) {
            CopyToInternal(array, index);
        }
        #endregion

        #region IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion

        #region IHierarchicalEnumerable implementation

        /// <internalonly/>
        IHierarchyData IHierarchicalEnumerable.GetHierarchyData(object enumeratedItem) {
            return GetHierarchyData(enumeratedItem);
        }
        #endregion

        #region IList implementation
        /// <internalonly/>
        bool IList.IsFixedSize {
            get { return IsFixedSize; }
        }

        /// <internalonly/>
        bool IList.IsReadOnly {
            get { return IsReadOnly; }
        }


        /// <internalonly/>
        object IList.this[int index] {
            get {
                return this[index];
            }
            set {
                OnValidate(value);
                this[index] = (SiteMapNode)value;
            }
        }

        int IList.Add(object value) {
            OnValidate(value);
            return Add((SiteMapNode)value);
        }

        void IList.Clear() {
            Clear();
        }

        bool IList.Contains(object value) {
            OnValidate(value);
            return Contains((SiteMapNode)value);
        }

        int IList.IndexOf(object value) {
            OnValidate(value);
            return IndexOf((SiteMapNode)value);
        }

        void IList.Insert(int index, object value) {
            OnValidate(value);
            Insert(index, (SiteMapNode)value);
        }

        void IList.Remove(object value) {
            OnValidate(value);
            Remove((SiteMapNode)value);
        }

        void IList.RemoveAt(int index) {
            RemoveAt(index);
        }
        #endregion

        private sealed class ReadOnlySiteMapNodeCollection : SiteMapNodeCollection {
            private SiteMapNodeCollection _internalCollection;

            internal ReadOnlySiteMapNodeCollection(SiteMapNodeCollection collection) {
                if (collection == null) {
                    throw new ArgumentNullException("collection");
                }

                _internalCollection = collection;
            }

            public override int Count {
                get {
                    return _internalCollection.Count;
                }
            }

            public override bool IsFixedSize {
                get { return true; }
            }

            public override bool IsReadOnly {
                get { return true; }
            }

            public override bool IsSynchronized {
                get {
                    return _internalCollection.IsSynchronized;
                }
            }

            public override object SyncRoot {
                get {
                    return _internalCollection.SyncRoot;
                }
            }


            public override int Add(SiteMapNode value) {
                throw new NotSupportedException(SR.GetString(SR.Collection_readonly));
            }


            public override void AddRange(SiteMapNode[] value) {
                throw new NotSupportedException(SR.GetString(SR.Collection_readonly));
            }


            public override void AddRange(SiteMapNodeCollection value) {
                throw new NotSupportedException(SR.GetString(SR.Collection_readonly));
            }

            public override void Clear() {
                throw new NotSupportedException(SR.GetString(SR.Collection_readonly));
            }

            public override bool Contains(SiteMapNode node) {
                return _internalCollection.Contains(node);
            }

            internal override void CopyToInternal(Array array, int index) {
                _internalCollection.List.CopyTo(array, index);
            }

            public override SiteMapNode this[int index] {
                get {
                    return _internalCollection[index];
                }
                set {
                    throw new NotSupportedException(SR.GetString(SR.Collection_readonly));
                }
            }

            public override IEnumerator GetEnumerator() {
                return _internalCollection.GetEnumerator();
            }

            public override int IndexOf(SiteMapNode value) {
                return _internalCollection.IndexOf(value);
            }

            public override void Insert(int index, SiteMapNode value) {
                throw new NotSupportedException(SR.GetString(SR.Collection_readonly));
            }

            public override void Remove(SiteMapNode value) {
                throw new NotSupportedException(SR.GetString(SR.Collection_readonly));
            }

            public override void RemoveAt(int index) {
                throw new NotSupportedException(SR.GetString(SR.Collection_readonly));
            }
        }
    }
}
