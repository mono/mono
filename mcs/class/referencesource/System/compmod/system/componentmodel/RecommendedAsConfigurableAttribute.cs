//------------------------------------------------------------------------------
// <copyright file="RecommendedAsConfigurableAttribute.cs" company="Microsoft">
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
    ///    <para>Specifies that the property can be
    ///       used as an application setting.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property)]
    [Obsolete("Use System.ComponentModel.SettingsBindableAttribute instead to work with the new settings model.")]
    public class RecommendedAsConfigurableAttribute : Attribute {
        private bool recommendedAsConfigurable = false;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of
        ///       the <see cref='System.ComponentModel.RecommendedAsConfigurableAttribute'/> class.
        ///    </para>
        /// </devdoc>
        public RecommendedAsConfigurableAttribute(bool recommendedAsConfigurable) {
            this.recommendedAsConfigurable = recommendedAsConfigurable;
        }

        /// <devdoc>
        ///    <para>Gets a value indicating whether the property this
        ///       attribute is bound to can be used as an application setting.</para>
        /// </devdoc>
        public bool RecommendedAsConfigurable {
            get {
                return recommendedAsConfigurable;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Specifies that a property cannot be used as an application setting. This
        ///    <see langword='static '/>field is read-only. 
        ///    </para>
        /// </devdoc>
        public static readonly RecommendedAsConfigurableAttribute No = new RecommendedAsConfigurableAttribute(false);

        /// <devdoc>
        ///    <para>
        ///       Specifies
        ///       that a property can be used as an application setting. This <see langword='static '/>field is read-only.
        ///    </para>
        /// </devdoc>
        public static readonly RecommendedAsConfigurableAttribute Yes = new RecommendedAsConfigurableAttribute(true);

        /// <devdoc>
        ///    <para>
        ///       Specifies the default value for the <see cref='System.ComponentModel.RecommendedAsConfigurableAttribute'/>, which is <see cref='System.ComponentModel.RecommendedAsConfigurableAttribute.No'/>. This <see langword='static '/>field is
        ///       read-only.
        ///    </para>
        /// </devdoc>
        public static readonly RecommendedAsConfigurableAttribute Default = No;
        
        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            RecommendedAsConfigurableAttribute other = obj as RecommendedAsConfigurableAttribute;

            return other != null && other.RecommendedAsConfigurable == recommendedAsConfigurable;
            
            
        }
        
        /// <devdoc>
        ///    <para>
        ///       Returns the hashcode for this object.
        ///    </para>
        /// </devdoc>
        public override int GetHashCode() {
            return base.GetHashCode();
        }

        /// <internalonly/>
        /// <devdoc>
        /// </devdoc>
        public override bool IsDefaultAttribute() {
            return !recommendedAsConfigurable;
        }
    }
}
