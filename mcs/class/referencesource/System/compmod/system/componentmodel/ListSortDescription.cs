//------------------------------------------------------------------------------
// <copyright file="ListSortDescription.cs" company="Microsoft">
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
    public class ListSortDescription {
        PropertyDescriptor property;
        ListSortDirection sortDirection;
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListSortDescription(PropertyDescriptor property, ListSortDirection direction) {
            this.property = property;
            this.sortDirection = direction;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PropertyDescriptor PropertyDescriptor {
            get {
                return this.property;
            }
            set {
                this.property = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListSortDirection SortDirection {
            get {
                return this.sortDirection;
            }
            set {
                this.sortDirection = value;
            }
        }
    }
}
