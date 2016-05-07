//------------------------------------------------------------------------------
// <copyright file="RunInstallerAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using System;
    using System.Diagnostics;
    using System.Security.Permissions;

    /// <include file='doc\RunInstallerAttribute.uex' path='docs/doc[@for="RunInstallerAttribute"]/*' />
    /// <devdoc>
    ///    <para>Specifies whether an installer should be invoked during
    ///       installation of an assembly.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public class RunInstallerAttribute : Attribute {
        private bool runInstaller;
        
        /// <include file='doc\RunInstallerAttribute.uex' path='docs/doc[@for="RunInstallerAttribute.RunInstallerAttribute"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of
        ///       the <see cref='System.ComponentModel.RunInstallerAttribute'/> class.
        ///    </para>
        /// </devdoc>
        public RunInstallerAttribute(bool runInstaller) {
            this.runInstaller = runInstaller;
        }

        /// <include file='doc\RunInstallerAttribute.uex' path='docs/doc[@for="RunInstallerAttribute.RunInstaller"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether an installer should be
        ///       invoked during installation of an assembly.
        ///    </para>
        /// </devdoc>
        public bool RunInstaller {
            get {
                return runInstaller;
            }
        }

        /// <include file='doc\RunInstallerAttribute.uex' path='docs/doc[@for="RunInstallerAttribute.Yes"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Specifies that a
        ///       component is visible in a visual designer. This <see langword='static '/>field is
        ///       read-only.
        ///    </para>
        /// </devdoc>
        public static readonly RunInstallerAttribute Yes = new RunInstallerAttribute(true);

        /// <include file='doc\RunInstallerAttribute.uex' path='docs/doc[@for="RunInstallerAttribute.No"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Specifies that a
        ///       component
        ///       is not visible in a visual designer. This <see langword='static '/>field is
        ///       read-only.
        ///    </para>
        /// </devdoc>
        public static readonly RunInstallerAttribute No = new RunInstallerAttribute(false);

        /// <include file='doc\RunInstallerAttribute.uex' path='docs/doc[@for="RunInstallerAttribute.Default"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Specifies the default visiblity, which is <see cref='System.ComponentModel.RunInstallerAttribute.No'/>. This <see langword='static '/>field is
        ///       read-only.
        ///    </para>
        /// </devdoc>
        public static readonly RunInstallerAttribute Default = No;

        /// <include file='doc\RunInstallerAttribute.uex' path='docs/doc[@for="RunInstallerAttribute.Equals"]/*' />
        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            RunInstallerAttribute other = obj as RunInstallerAttribute;
            return other != null && other.RunInstaller == runInstaller;
        }

        /// <include file='doc\RunInstallerAttribute.uex' path='docs/doc[@for="RunInstallerAttribute.GetHashCode"]/*' />
        /// <devdoc>
        ///    <para>
        ///       Returns the hashcode for this object.
        ///    </para>
        /// </devdoc>
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <include file='doc\RunInstallerAttribute.uex' path='docs/doc[@for="RunInstallerAttribute.IsDefaultAttribute"]/*' />
        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override bool IsDefaultAttribute() {
            return (this.Equals(Default));
        }
    }
}
