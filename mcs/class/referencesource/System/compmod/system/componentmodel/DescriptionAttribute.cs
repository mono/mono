//------------------------------------------------------------------------------
// <copyright file="DescriptionAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Specifies a description for a property
    ///       or event.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.All)]
    public class DescriptionAttribute : Attribute {
        /// <devdoc>
        /// <para>Specifies the default value for the <see cref='System.ComponentModel.DescriptionAttribute'/> , which is an
        ///    empty string (""). This <see langword='static'/> field is read-only.</para>
        /// </devdoc>
        public static readonly DescriptionAttribute Default = new DescriptionAttribute();
        private string description;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DescriptionAttribute() : this (string.Empty) {
        }

        /// <devdoc>
        ///    <para>Initializes a new instance of the <see cref='System.ComponentModel.DescriptionAttribute'/> class.</para>
        /// </devdoc>
        public DescriptionAttribute(string description) {
            this.description = description;
        }

        /// <devdoc>
        ///    <para>Gets the description stored in this attribute.</para>
        /// </devdoc>
        public virtual string Description {
            get {
                return DescriptionValue;
            }
        }

        /// <devdoc>
        ///     Read/Write property that directly modifies the string stored
        ///     in the description attribute. The default implementation
        ///     of the Description property simply returns this value.
        /// </devdoc>
        protected string DescriptionValue {
            get {
                return description;
            }
            set {
                description = value;
            }
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            DescriptionAttribute other = obj as DescriptionAttribute;

            return (other != null) && other.Description == Description;
        }

        public override int GetHashCode() {
            return Description.GetHashCode();
        }

#if !SILVERLIGHT
        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override bool IsDefaultAttribute() {
            return (this.Equals(Default));
        }
#endif

    }
}
