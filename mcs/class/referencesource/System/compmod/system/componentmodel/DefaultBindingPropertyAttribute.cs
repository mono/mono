//------------------------------------------------------------------------------
// <copyright file="DefaultBindingPropertyAttribute.cs" company="Microsoft">
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
    ///    <para>Specifies the default binding property for a component.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DefaultBindingPropertyAttribute : Attribute {

        private readonly string name;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of
        ///       the <see cref='System.ComponentModel.DefaultBindingPropertyAttribute'/> class.
        ///    </para>
        /// </devdoc>
        public DefaultBindingPropertyAttribute() {
            this.name = null;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of
        ///       the <see cref='System.ComponentModel.DefaultBindingPropertyAttribute'/> class.
        ///    </para>
        /// </devdoc>
        public DefaultBindingPropertyAttribute(string name) {
            this.name = name;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name of the default binding property for the component this attribute is
        ///       bound to.
        ///    </para>
        /// </devdoc>
        public string Name {
            get {
                return name;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Specifies the default value for the <see cref='System.ComponentModel.DefaultBindingPropertyAttribute'/>, which is <see langword='null'/>. This
        ///    <see langword='static '/>field is read-only. 
        ///    </para>
        /// </devdoc>
        public static readonly DefaultBindingPropertyAttribute Default = new DefaultBindingPropertyAttribute();

        public override bool Equals(object obj) {
            DefaultBindingPropertyAttribute other = obj as DefaultBindingPropertyAttribute; 
            return other != null && other.Name == name;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
