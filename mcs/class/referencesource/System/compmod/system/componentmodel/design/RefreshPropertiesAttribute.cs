//------------------------------------------------------------------------------
// <copyright file="RefreshPropertiesAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para> Specifies how a designer refreshes when the property value is changed.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.All)]
    public sealed class RefreshPropertiesAttribute : Attribute {

        /// <devdoc>
        ///    <para>
        ///       Indicates all properties should
        ///       be refreshed if the property value is changed. This field is
        ///       read-only.
        ///    </para>
        /// </devdoc>
        public static readonly RefreshPropertiesAttribute All = new RefreshPropertiesAttribute(RefreshProperties.All);

        /// <devdoc>
        ///    <para>
        ///       Indicates all properties should
        ///       be invalidated and repainted if the
        ///       property value is changed. This field is read-only.
        ///    </para>
        /// </devdoc>
        public static readonly RefreshPropertiesAttribute Repaint = new RefreshPropertiesAttribute(RefreshProperties.Repaint);

        /// <devdoc>
        ///    <para>
        ///       Indicates that by default
        ///       no
        ///       properties should be refreshed if the property value
        ///       is changed. This field is read-only.
        ///    </para>
        /// </devdoc>
        public static readonly RefreshPropertiesAttribute Default = new RefreshPropertiesAttribute(RefreshProperties.None);

        private RefreshProperties refresh;

        /// <devdoc>
        /// </devdoc>
        /// <internalonly/>
        public RefreshPropertiesAttribute(RefreshProperties refresh) {
            this.refresh = refresh;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets
        ///       the refresh properties for the member.
        ///    </para>
        /// </devdoc>
        public RefreshProperties RefreshProperties {
            get {
                return refresh;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Overrides object's Equals method.
        ///    </para>
        /// </devdoc>
        public override bool Equals(object value) {
            if (value is RefreshPropertiesAttribute) {
                return(((RefreshPropertiesAttribute)value).RefreshProperties == refresh);
            }
            return false;
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
        ///    <para>Gets a value indicating whether the current attribute is the default.</para>
        /// </devdoc>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }
    }
}

