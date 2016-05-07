//------------------------------------------------------------------------------
// <copyright file="PagedDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Web.UI;

    /// <devdoc>
    ///    <para>Provides a wrapper over an ICollection datasource to implement paging 
    ///       semantics or 'paged views' on top of the underlying datasource.</para>
    /// </devdoc>
    public sealed class PagedDataSource : ICollection, ITypedList {

        private IEnumerable dataSource;
        private int currentPageIndex;
        private int pageSize;
        private bool allowPaging;
        private bool allowCustomPaging;
        private bool allowServerPaging;
        private int virtualCount;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.WebControls.PagedDataSource'/> class.</para>
        /// </devdoc>
        public PagedDataSource() {
            this.pageSize = 10;
            this.allowPaging = false;
            this.currentPageIndex = 0;
            this.allowCustomPaging = false;
            this.allowServerPaging = false;
            this.virtualCount = 0;
        }



        /// <devdoc>
        ///    Indicates whether to assume the underlying datasource
        ///    contains data for just the current page.
        /// </devdoc>
        public bool AllowCustomPaging {
            get {
                return allowCustomPaging;
            }
            set {
                allowCustomPaging = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates whether to implement page semantics on top of the underlying datasource.</para>
        /// </devdoc>
        public bool AllowPaging {
            get {
                return allowPaging;
            }
            set {
                allowPaging = value;
            }
        }


        /// <devdoc>
        /// <para>Indicates whether to implement page semantics on top of the underlying datasource.</para>
        /// </devdoc>
        public bool AllowServerPaging {
            get {
                return allowServerPaging;
            }
            set {
                allowServerPaging = value;
            }
        }
        

        /// <devdoc>
        ///    <para> 
        ///       Specifies the number of items
        ///       to be used from the datasource.</para>
        /// </devdoc>
        public int Count {
            get {
                if (dataSource == null)
                    return 0;

                if (IsPagingEnabled) {
                    if (IsCustomPagingEnabled || (IsLastPage == false)) {
                        // In custom paging the datasource can contain at most
                        // a single page's worth of data.
                        // In non-custom paging, all pages except last one have
                        // a full page worth of data.
                        return pageSize;
                    }
                    else {
                        // last page might have fewer items in datasource
                        return DataSourceCount - FirstIndexInPage;
                    }
                }
                else {
                    return DataSourceCount;
                }
            }
        }


        /// <devdoc>
        ///    <para>Indicates the index of the current page.</para>
        /// </devdoc>
        public int CurrentPageIndex {
            get {
                return currentPageIndex;
            }
            set {
                currentPageIndex = value;
            }
        }


        /// <devdoc>
        ///    <para> Indicates the data source.</para>
        /// </devdoc>
        public IEnumerable DataSource {
            get {
                return dataSource;
            }
            set {
                dataSource = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public int DataSourceCount {
            get {
                if (dataSource == null)
                    return 0;
                if (IsCustomPagingEnabled || IsServerPagingEnabled) {
                    return virtualCount;
                }
                else {
                    if (dataSource is ICollection) {
                        return ((ICollection)dataSource).Count;
                    }
                    else {
                        // The caller should not call this in the case of an IEnumerator datasource
                        // This is required for paging, but the assumption is that the user will set
                        // up custom paging.
                        throw new HttpException(SR.GetString(SR.PagedDataSource_Cannot_Get_Count));
                    }
                }
            }
        }

        /// <devdoc>
        /// </devdoc>
        public int FirstIndexInPage {
            get {
                if ((dataSource == null) || (IsPagingEnabled == false)) {
                    return 0;
                }
                else {
                    if (IsCustomPagingEnabled || IsServerPagingEnabled) {
                        // In this mode, all the data belongs to the current page.
                        return 0;
                    }
                    else {
                        return currentPageIndex * pageSize;
                    }
                }
            }
        }


        /// <devdoc>
        /// </devdoc>
        public bool IsCustomPagingEnabled {
            get {   
                return IsPagingEnabled && allowCustomPaging;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public bool IsFirstPage {
            get {
                if (IsPagingEnabled)
                    return(CurrentPageIndex == 0);
                else
                    return true;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public bool IsLastPage {
            get {
                if (IsPagingEnabled)
                    return(CurrentPageIndex == (PageCount - 1));
                else
                    return true;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public bool IsPagingEnabled {
            get {
                return allowPaging && (pageSize != 0);
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
                return IsPagingEnabled && allowServerPaging;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public bool IsSynchronized {
            get {
                return false;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public int PageCount {
            get {
                if (dataSource == null)
                    return 0;

                int dataSourceItemCount = DataSourceCount;
                if (IsPagingEnabled && (dataSourceItemCount > 0)) {
                    int pageCountNum = dataSourceItemCount + pageSize - 1;
                    if (pageCountNum < 0) {
                        return 1;   // integer overflow
                    }
                    return (int)(pageCountNum/pageSize);
                }
                else {
                    return 1;
                }
            }
        }


        /// <devdoc>
        /// </devdoc>
        public int PageSize {
            get {
                return pageSize;
            }
            set {
                pageSize = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public object SyncRoot {
            get {
                return this;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public int VirtualCount {
            get {
                return virtualCount;
            }
            set {
                virtualCount = value;
            }
        }


        /// <devdoc>
        /// </devdoc>
        public void CopyTo(Array array, int index) {
            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }

        /// <devdoc>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            int startIndex = FirstIndexInPage;
            int count = -1;
            
            if (dataSource is ICollection) {
                count = Count;
            }

            if (dataSource is IList) {
                return new EnumeratorOnIList((IList)dataSource, startIndex, count);
            }
            else if (dataSource is Array) {
                return new EnumeratorOnArray((object[])dataSource, startIndex, count);
            }
            else if (dataSource is ICollection) {
                return new EnumeratorOnICollection((ICollection)dataSource, startIndex, count);
            }
            else {
                if (allowCustomPaging || allowServerPaging) {
                    // startIndex does not matter
                    // however count does... even if the data source contains more than 1 page of data in
                    // it, we only want to enumerate over a single page of data
                    // note: we can call Count here, even though we're dealing with an IEnumerator
                    //       because by now we have ensured that we're in custom paging mode
                    return new EnumeratorOnIEnumerator(dataSource.GetEnumerator(), Count);
                }
                else {
                    // startIndex and count don't matter since we're going to enumerate over all the
                    // data (either non-paged or custom paging scenario)
                    return dataSource.GetEnumerator();
                }
            }
        }


        /// <devdoc>
        /// </devdoc>
        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors) {
            if (dataSource == null)
                return null;

            if (dataSource is ITypedList) {
                return((ITypedList)dataSource).GetItemProperties(listAccessors);
            }
            return null;
        }


        /// <devdoc>
        /// </devdoc>
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
            private int startIndex;
            private int index;
            private int indexBounds;

            public EnumeratorOnICollection(ICollection collection, int startIndex, int count) {
                this.collection = collection;
                this.startIndex = startIndex;
                this.index = -1;

                this.indexBounds = startIndex + count;
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
                    for (int i = 0; i < startIndex; i++)
                        collectionEnum.MoveNext();
                }
                collectionEnum.MoveNext();
                index++;
                return (startIndex + index) < indexBounds;
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
            private int startIndex;
            private int index;
            private int indexBounds;

            public EnumeratorOnIList(IList collection, int startIndex, int count) {
                this.collection = collection;
                this.startIndex = startIndex;
                this.index = -1;

                this.indexBounds = startIndex + count;
                if (indexBounds > collection.Count) {
                    indexBounds = collection.Count;
                }
            }

            public object Current {
                get {
                    if (index < 0) {
                        throw new InvalidOperationException(SR.GetString(SR.Enumerator_MoveNext_Not_Called));
                    }
                    return collection[startIndex + index];
                }
            }

            public bool MoveNext() {
                index++;
                return (startIndex + index) < indexBounds;
            }

            public void Reset() {
                index = -1;
            }
        }



        /// <devdoc>
        /// </devdoc>
        private sealed class EnumeratorOnArray : IEnumerator {
            private object[] array;
            private int startIndex;
            private int index;
            private int indexBounds;

            public EnumeratorOnArray(object[] array, int startIndex, int count) {
                this.array = array;
                this.startIndex = startIndex;
                this.index = -1;

                this.indexBounds = startIndex + count;
                if (indexBounds > array.Length) {
                    indexBounds = array.Length;
                }
            }

            public object Current {
                get {
                    if (index < 0) {
                        throw new InvalidOperationException(SR.GetString(SR.Enumerator_MoveNext_Not_Called));
                    }
                    return array[startIndex + index];
                }
            }

            public bool MoveNext() {
                index++;
                return (startIndex + index) < indexBounds;
            }

            public void Reset() {
                index = -1;
            }
        }
    }
}

