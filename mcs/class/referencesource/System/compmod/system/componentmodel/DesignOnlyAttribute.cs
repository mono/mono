//------------------------------------------------------------------------------
// <copyright file="DesignOnlyAttribute.cs" company="Microsoft">
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
    ///    <para>Specifies whether a property can only be set at
    ///       design time.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class DesignOnlyAttribute : Attribute {
        private bool isDesignOnly = false;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.ComponentModel.DesignOnlyAttribute'/> class.
        ///    </para>
        /// </devdoc>
        public DesignOnlyAttribute(bool isDesignOnly) {
            this.isDesignOnly = isDesignOnly;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether a property
        ///       can be set only at design time.
        ///    </para>
        /// </devdoc>
        public bool IsDesignOnly {
            get {
                return isDesignOnly;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Specifies that a property can be set only at design time. This
        ///    <see langword='static '/>field is read-only. 
        ///    </para>
        /// </devdoc>
        public static readonly DesignOnlyAttribute Yes = new DesignOnlyAttribute(true);

        /// <devdoc>
        ///    <para>
        ///       Specifies
        ///       that a
        ///       property can be set at design time or at run
        ///       time. This <see langword='static '/>field is read-only.
        ///    </para>
        /// </devdoc>
        public static readonly DesignOnlyAttribute No = new DesignOnlyAttribute(false);

        /// <devdoc>
        ///    <para>
        ///       Specifies the default value for the <see cref='System.ComponentModel.DesignOnlyAttribute'/>, which is <see cref='System.ComponentModel.DesignOnlyAttribute.No'/>. This <see langword='static'/> field is
        ///       read-only.
        ///    </para>
        /// </devdoc>
        public static readonly DesignOnlyAttribute Default = No;

        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return IsDesignOnly == Default.IsDesignOnly;
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            DesignOnlyAttribute other = obj as DesignOnlyAttribute;

            return (other != null) && other.isDesignOnly == isDesignOnly;
        }

        public override int GetHashCode() {
            return isDesignOnly.GetHashCode();
        }
    }
}
