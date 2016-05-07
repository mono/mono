//------------------------------------------------------------------------------
// <copyright file="IBindingListView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------



namespace System.ComponentModel {
    using System.Collections;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public interface IBindingListView : IBindingList {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void ApplySort(ListSortDescriptionCollection sorts);
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        string Filter {get;set;}
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        ListSortDescriptionCollection SortDescriptions {get;}
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        void RemoveFilter();
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        bool SupportsAdvancedSorting{get;}
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        bool SupportsFiltering{get;}
    }

}

