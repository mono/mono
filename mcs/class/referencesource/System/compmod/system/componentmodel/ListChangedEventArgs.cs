//------------------------------------------------------------------------------
// <copyright file="ListChangedEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

//can not fix - Everett breaking change
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="System.ComponentModel.ListChangedEventArgs..ctor(System.ComponentModel.ListChangedType,System.Int32,System.ComponentModel.PropertyDescriptor)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="member", Target="System.ComponentModel.ListChangedEventArgs..ctor(System.ComponentModel.ListChangedType,System.ComponentModel.PropertyDescriptor)")]

namespace System.ComponentModel {

    using Microsoft.Win32;
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class ListChangedEventArgs : EventArgs {

        private ListChangedType listChangedType;
        private int newIndex;
        private int oldIndex;
        private PropertyDescriptor propDesc;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListChangedEventArgs(ListChangedType listChangedType, int newIndex) : this(listChangedType, newIndex, -1) {
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListChangedEventArgs(ListChangedType listChangedType, int newIndex, PropertyDescriptor propDesc) : this(listChangedType, newIndex) {
            this.propDesc = propDesc;
            this.oldIndex = newIndex;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListChangedEventArgs(ListChangedType listChangedType, PropertyDescriptor propDesc) {
            Debug.Assert(listChangedType != ListChangedType.Reset, "this constructor is used only for changes in the list MetaData");
            Debug.Assert(listChangedType != ListChangedType.ItemAdded, "this constructor is used only for changes in the list MetaData");
            Debug.Assert(listChangedType != ListChangedType.ItemDeleted, "this constructor is used only for changes in the list MetaData");
            Debug.Assert(listChangedType != ListChangedType.ItemChanged, "this constructor is used only for changes in the list MetaData");

            this.listChangedType = listChangedType;
            this.propDesc = propDesc;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListChangedEventArgs(ListChangedType listChangedType, int newIndex, int oldIndex) {
            Debug.Assert(listChangedType != ListChangedType.PropertyDescriptorAdded, "this constructor is used only for item changed in the list");
            Debug.Assert(listChangedType != ListChangedType.PropertyDescriptorDeleted, "this constructor is used only for item changed in the list");
            Debug.Assert(listChangedType != ListChangedType.PropertyDescriptorChanged, "this constructor is used only for item changed in the list");
            this.listChangedType = listChangedType;
            this.newIndex = newIndex;
            this.oldIndex = oldIndex;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListChangedType ListChangedType {
            get {
                return listChangedType;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int NewIndex {
            get {
                return newIndex;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int OldIndex {
            get {
                return oldIndex;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PropertyDescriptor PropertyDescriptor {
            get {
                return propDesc;
            }
        }
    }
}


