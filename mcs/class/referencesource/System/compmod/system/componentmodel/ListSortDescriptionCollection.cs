//------------------------------------------------------------------------------
// <copyright file="ListSortDescriptionCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.ComponentModel {
    using System.Collections;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class ListSortDescriptionCollection : IList {
        ArrayList sorts = new ArrayList();

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListSortDescriptionCollection() {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListSortDescriptionCollection(ListSortDescription[] sorts) {
            if (sorts != null) {
                for (int i = 0; i < sorts.Length; i ++) {
                    this.sorts.Add(sorts[i]);
                }
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListSortDescription this[int index] {
            get {
                return (ListSortDescription) sorts[index];
            }
            set {
                throw new InvalidOperationException(SR.GetString(SR.CantModifyListSortDescriptionCollection));
            }
        }

        // IList implementation
        //

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        bool IList.IsFixedSize {
            get {
                return true;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        bool IList.IsReadOnly {
            get {
                return true;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        object IList.this[int index] {
            get {
                return this[index];
            }
            set {
                throw new InvalidOperationException(SR.GetString(SR.CantModifyListSortDescriptionCollection));
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        int IList.Add(object value) {
            throw new InvalidOperationException(SR.GetString(SR.CantModifyListSortDescriptionCollection));
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void IList.Clear() {
            throw new InvalidOperationException(SR.GetString(SR.CantModifyListSortDescriptionCollection));
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool Contains(object value) {
            return ((IList)this.sorts).Contains(value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int IndexOf(object value) {
            return ((IList)this.sorts).IndexOf(value);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void IList.Insert(int index, object value) {
            throw new InvalidOperationException(SR.GetString(SR.CantModifyListSortDescriptionCollection));
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void IList.Remove(object value) {
            throw new InvalidOperationException(SR.GetString(SR.CantModifyListSortDescriptionCollection));
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void IList.RemoveAt(int index) {
            throw new InvalidOperationException(SR.GetString(SR.CantModifyListSortDescriptionCollection));
        }

        // ICollection
        //

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int Count {
            get {
                return this.sorts.Count;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        bool ICollection.IsSynchronized {
            get {
                // true because after the constructor finished running the ListSortDescriptionCollection is Read Only
                return true;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        object ICollection.SyncRoot {
            get {
                return this;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void CopyTo(Array array, int index) {
            this.sorts.CopyTo(array, index);
        }

        // IEnumerable
        //

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.sorts.GetEnumerator();
        }
    }
}
