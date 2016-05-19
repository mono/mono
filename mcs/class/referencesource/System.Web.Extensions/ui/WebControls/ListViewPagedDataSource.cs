//------------------------------------------------------------------------------
// <copyright file="ListViewPagedDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Resources;
using System.Web.UI;

namespace System.Web.UI.WebControls {
    [
    SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix",
                    Justification = "Type is a generalized data structure that happens to implement ICollection"),
    ]
    public class ListViewPagedDataSource : ICollection, ITypedList {

        private IEnumerable _dataSource;
        private bool _allowServerPaging;

        private int _startRowIndex;
        private int _maximumRows;
        private int _totalRowCount;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.PagedDataSource'/> class.</para>
        /// </devdoc>
        public ListViewPagedDataSource() {
            _allowServerPaging = false;
            _totalRowCount = 0;
        }

        /// <devdoc>
        /// <para>Indicates whether to implement page semantics on top of the underlying datasource.</para>
        /// </devdoc>
        public bool AllowServerPaging {
            get {
                return _allowServerPaging;
            }
            set {
                _allowServerPaging = value;
            }
        }


        /// <devdoc>
        ///    <para> 
        ///       Specifies the number of items
        ///       to be used from the datasource.</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public int Count {
            get {
                if (_dataSource == null)
                    return 0;

                if (IsLastPage == false) {
                    // In custom paging the datasource can contain at most
                    // a single page's worth of data.
                    // In non-custom paging, all pages except last one have
                    // a full page worth of data.
                    if (MaximumRows >= 0) {
                        return MaximumRows;
                    }
                    else {
                        return DataSourceCount - StartRowIndex;
                    }
                }
                else {
                    // last page might have fewer items in datasource
                    return DataSourceCount - StartRowIndex;
                }
            }
        }


        /// <devdoc>
        ///    <para> Indicates the data source.</para>
        /// </devdoc>
        public IEnumerable DataSource {
            get {
                return _dataSource;
            }
            set {
                _dataSource = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public int DataSourceCount {
            get {
                if (_dataSource == null)
                    return 0;
                if (IsServerPagingEnabled) {
                    return _totalRowCount;
                }
                else {
                    if (_dataSource is ICollection) {
                        return ((ICollection)_dataSource).Count;
                    }
                    else {
                        // The caller should not call this in the case of an IEnumerator datasource
                        // This is required for paging, but the assumption is that the user will set
                        // up custom paging.
                        throw new InvalidOperationException(AtlasWeb.ListViewPagedDataSource_CannotGetCount);
                    }
                }
            }
        }

        /// <devdoc>
        /// </devdoc>
        private bool IsLastPage {
            get {
                if (StartRowIndex + MaximumRows >= DataSourceCount)
                    return true;
                else
                    return false;
            }
        }

        /// <devdoc>
        /// </devdoc>
        public bool IsReadOnly {
            get {
                return false;
            }
        }


        /// <devdoc>
        /// Indicates whether server-side paging is enabled
        /// </devdoc>
        public bool IsServerPagingEnabled {
            get {
                return _allowServerPaging;
            }
        }


        /// <devdoc>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public bool IsSynchronized {
            get {
                return false;
            }
        }

        public int MaximumRows {
            get {
                return _maximumRows;
            }
            set {
                _maximumRows = value;
            }
        }

        public int StartRowIndex {
            get {
                return _startRowIndex;
            }
            set {
                _startRowIndex = value;
            }
        }

        /// <devdoc>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public object SyncRoot {
            get {
                return this;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public int TotalRowCount {
            get {
                return _totalRowCount;
            }
            set {
                _totalRowCount = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext(); )
                array.SetValue(e.Current, index++);
        }

        /// <devdoc>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public IEnumerator GetEnumerator() {
            int startRowIndex = 0;
            int count = -1;

            if (!IsServerPagingEnabled) {
                startRowIndex = StartRowIndex;
            }

            if (_dataSource is ICollection) {
                count = Count;
            }

            if (_dataSource is IList) {
                return new EnumeratorOnIList((IList)_dataSource, startRowIndex, count);
            }
            else if (_dataSource is Array) {
                return new EnumeratorOnArray((object[])_dataSource, startRowIndex, count);
            }
            else if (_dataSource is ICollection) {
                return new EnumeratorOnICollection((ICollection)_dataSource, startRowIndex, count);
            }
            else {
                if (_allowServerPaging) {
                    // startRowIndex does not matter
                    // however count does... even if the data source contains more than 1 page of data in
                    // it, we only want to enumerate over a single page of data
                    // note: we can call Count here, even though we're dealing with an IEnumerator
                    //       because by now we have ensured that we're in custom paging mode
                    return new EnumeratorOnIEnumerator(_dataSource.GetEnumerator(), Count);
                }
                else {
                    // startRowIndex and count don't matter since we're going to enumerate over all the
                    // data (either non-paged or custom paging scenario)
                    return _dataSource.GetEnumerator();
                }
            }
        }


        /// <devdoc>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors) {
            if (_dataSource == null)
                return null;

            if (_dataSource is ITypedList) {
                return ((ITypedList)_dataSource).GetItemProperties(listAccessors);
            }
            return null;
        }


        /// <devdoc>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public string GetListName(PropertyDescriptor[] listAccessors) {
            return String.Empty;
        }



        /// <devdoc>
        /// </devdoc>
        private sealed class EnumeratorOnIEnumerator : IEnumerator {
            private IEnumerator realEnum;
            private int index;
            private int indexBounds;

            public EnumeratorOnIEnumerator(IEnumerator realEnum, int count) {
                this.realEnum = realEnum;
                this.index = -1;
                this.indexBounds = count;
            }

            public object Current {
                get {
                    return realEnum.Current;
                }
            }

            public bool MoveNext() {
                bool result = realEnum.MoveNext();
                index++;
                return result && (index < indexBounds);
            }

            public void Reset() {
                realEnum.Reset();
                index = -1;
            }
        }


        /// <devdoc>
        /// </devdoc>
        private sealed class EnumeratorOnICollection : IEnumerator {
            private ICollection collection;
            private IEnumerator collectionEnum;
            private int startRowIndex;
            private int index;
            private int indexBounds;

            public EnumeratorOnICollection(ICollection collection, int startRowIndex, int count) {
                this.collection = collection;
                this.startRowIndex = startRowIndex;
                this.index = -1;

                this.indexBounds = startRowIndex + count;
                if (indexBounds > collection.Count) {
                    indexBounds = collection.Count;
                }
            }

            public object Current {
                get {
                    return collectionEnum.Current;
                }
            }

            public bool MoveNext() {
                if (collectionEnum == null) {
                    collectionEnum = collection.GetEnumerator();
                    for (int i = 0; i < startRowIndex; i++)
                        collectionEnum.MoveNext();
                }
                collectionEnum.MoveNext();
                index++;
                return (startRowIndex + index) < indexBounds;
            }

            public void Reset() {
                collectionEnum = null;
                index = -1;
            }
        }



        /// <devdoc>
        /// </devdoc>
        private sealed class EnumeratorOnIList : IEnumerator {
            private IList collection;
            private int startRowIndex;
            private int index;
            private int indexBounds;

            public EnumeratorOnIList(IList collection, int startRowIndex, int count) {
                this.collection = collection;
                this.startRowIndex = startRowIndex;
                this.index = -1;

                this.indexBounds = startRowIndex + count;
                if (indexBounds > collection.Count) {
                    indexBounds = collection.Count;
                }
            }

            public object Current {
                get {
                    if (index < 0) {
                        throw new InvalidOperationException(AtlasWeb.ListViewPagedDataSource_EnumeratorMoveNextNotCalled);
                    }
                    return collection[startRowIndex + index];
                }
            }

            public bool MoveNext() {
                index++;
                return (startRowIndex + index) < indexBounds;
            }

            public void Reset() {
                index = -1;
            }
        }



        /// <devdoc>
        /// </devdoc>
        private sealed class EnumeratorOnArray : IEnumerator {
            private object[] array;
            private int startRowIndex;
            private int index;
            private int indexBounds;

            public EnumeratorOnArray(object[] array, int startRowIndex, int count) {
                this.array = array;
                this.startRowIndex = startRowIndex;
                this.index = -1;

                this.indexBounds = startRowIndex + count;
                if (indexBounds > array.Length) {
                    indexBounds = array.Length;
                }
            }

            public object Current {
                get {
                    if (index < 0) {
                        throw new InvalidOperationException(AtlasWeb.ListViewPagedDataSource_EnumeratorMoveNextNotCalled);
                    }
                    return array[startRowIndex + index];
                }
            }

            public bool MoveNext() {
                index++;
                return (startRowIndex + index) < indexBounds;
            }

            public void Reset() {
                index = -1;
            }
        }
    }
}
