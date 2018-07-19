//------------------------------------------------------------------------------
// <copyright file="ListBindableAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class ListBindableAttribute : Attribute {
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly ListBindableAttribute Yes = new ListBindableAttribute(true);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly ListBindableAttribute No = new ListBindableAttribute(false);

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static readonly ListBindableAttribute Default = Yes;

        private bool listBindable   = false;
        private bool isDefault  = false;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListBindableAttribute(bool listBindable) {
            this.listBindable = listBindable;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public ListBindableAttribute(BindableSupport flags) {
            this.listBindable = (flags != BindableSupport.No);
            this.isDefault = (flags == BindableSupport.Default);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool ListBindable {
            get {
                return listBindable;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            
            ListBindableAttribute other = obj as ListBindableAttribute;
            return other != null && other.ListBindable == listBindable;
        }
        
        /// <devdoc>
        ///    <para>
        ///       Returns the hashcode for this object.
        ///    </para>
        /// </devdoc>
        public override int GetHashCode() {
            return base.GetHashCode();
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool IsDefaultAttribute() {
            return (this.Equals(Default) || isDefault);
        }
    }
}
