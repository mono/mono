//------------------------------------------------------------------------------
// <copyright file="DesignTimeVisibleAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    
    using System;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>
    ///       DesignTimeVisibileAttribute marks a component's visibility. If
    ///       DesignTimeVisibileAttribute.Yes is present, a visual designer can show
    ///       this component on a designer.
    ///    </para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class DesignTimeVisibleAttribute : Attribute {
        private bool visible;

        /// <devdoc>
        ///     Creates a new DesignTimeVisibleAttribute with the visible
        ///     property set to the given value.
        /// </devdoc>
        public DesignTimeVisibleAttribute(bool visible) {
            this.visible = visible;
        }
        
        /// <devdoc>
        ///     Creates a new DesignTimeVisibleAttribute set to the default
        ///     value of true.
        /// </devdoc>
        public DesignTimeVisibleAttribute() {
        }

        /// <devdoc>
        ///     True if this component should be shown at design time, or false
        ///     if it shouldn't.
        /// </devdoc>
        public bool Visible {
            get {
                return visible;
            }
        }

        /// <devdoc>
        ///     Marks a component as visible in a visual designer.
        /// </devdoc>
        public static readonly DesignTimeVisibleAttribute Yes = new DesignTimeVisibleAttribute(true);

        /// <devdoc>
        ///     Marks a component as not visible in a visual designer.
        /// </devdoc>
        public static readonly DesignTimeVisibleAttribute No = new DesignTimeVisibleAttribute(false);

        /// <devdoc>
        ///     The default visiblity. (equal to Yes.)
        /// </devdoc>
        public static readonly DesignTimeVisibleAttribute Default = Yes;
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            DesignTimeVisibleAttribute other = obj as DesignTimeVisibleAttribute;
            return other != null && other.Visible == visible;
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override int GetHashCode() {
            return typeof(DesignTimeVisibleAttribute).GetHashCode() ^ (visible ? -1 : 0);
        }
        
#if !SILVERLIGHT
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override bool IsDefaultAttribute() {
            return (this.Visible == Default.Visible);
        }
#endif
    }
}
