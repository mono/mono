//------------------------------------------------------------------------------
// <copyright file="IBindingList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope="type", Target="System.ComponentModel.IBindingList")]

namespace System.ComponentModel {
    using System.Collections;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public interface IBindingList : IList {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        bool AllowNew { get;}
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        object AddNew();
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        bool AllowEdit { get; }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        bool AllowRemove { get; }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        bool SupportsChangeNotification { get; }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        bool SupportsSearching { get; }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        bool SupportsSorting { get; }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        bool IsSorted { get; }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        PropertyDescriptor SortProperty { get; }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        ListSortDirection SortDirection { get; }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        event ListChangedEventHandler ListChanged;
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>

        void AddIndex(PropertyDescriptor property);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void ApplySort(PropertyDescriptor property, ListSortDirection direction);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        int Find(PropertyDescriptor property, object key);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void RemoveIndex(PropertyDescriptor property);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void RemoveSort();
    }
}

