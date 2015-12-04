//------------------------------------------------------------------------------
// <copyright file="InstallerTypeAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

// SECREVIEW: Remove this attribute once bug#411889 is fixed.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2113:SecureLateBindingMethods", Scope="member", Target="System.ComponentModel.InstallerTypeAttribute.get_InstallerType():System.Type")]

namespace System.ComponentModel {
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Specifies the installer
    ///       to use for a type to install components.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public class InstallerTypeAttribute : Attribute {
        string _typeName;

        /// <devdoc>
        /// <para>Initializes a new instance of the System.Windows.Forms.ComponentModel.InstallerTypeAttribute class.</para>
        /// </devdoc>
        public InstallerTypeAttribute(Type installerType) {
            _typeName = installerType.AssemblyQualifiedName;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public InstallerTypeAttribute(string typeName) {
            _typeName = typeName;
        }

        /// <devdoc>
        ///    <para> Gets the
        ///       type of installer associated with this attribute.</para>
        /// </devdoc>
        public virtual Type InstallerType {
            get {
                return Type.GetType(_typeName);
            }
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            InstallerTypeAttribute other = obj as InstallerTypeAttribute;

            return (other != null) && other._typeName == _typeName;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
