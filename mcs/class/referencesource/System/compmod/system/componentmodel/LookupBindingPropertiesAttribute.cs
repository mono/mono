//------------------------------------------------------------------------------
// <copyright file="LookupBindingPropertiesAttribute.cs" company="Microsoft">
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
    ///    <para>Specifies the data source and data member properties for a component.</para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LookupBindingPropertiesAttribute : Attribute {

        private readonly string dataSource;
        private readonly string displayMember;
        private readonly string valueMember;
        private readonly string lookupMember;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of
        ///       the <see cref='System.ComponentModel.LookupBindingPropertiesAttribute'/> class.
        ///    </para>
        /// </devdoc>
        public LookupBindingPropertiesAttribute() {
            this.dataSource = null;
            this.displayMember = null;
            this.valueMember = null;
            this.lookupMember = null;
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of
        ///       the <see cref='System.ComponentModel.LookupBindingPropertiesAttribute'/> class.
        ///    </para>
        /// </devdoc>
        public LookupBindingPropertiesAttribute(string dataSource, string displayMember, string valueMember, string lookupMember) {
            this.dataSource = dataSource;
            this.displayMember = displayMember;
            this.valueMember = valueMember;
            this.lookupMember = lookupMember;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name of the data source property for the component this attribute is
        ///       bound to.
        ///    </para>
        /// </devdoc>
        public string DataSource {
            get {
                return dataSource;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name of the display member property for the component this attribute is
        ///       bound to.
        ///    </para>
        /// </devdoc>
        public string DisplayMember {
            get {
                return displayMember;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name of the value member property for the component this attribute is
        ///       bound to.
        ///    </para>
        /// </devdoc>
        public string ValueMember {
            get {
                return valueMember;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the name of the  member property for the component this attribute is
        ///       bound to.
        ///    </para>
        /// </devdoc>
        public string LookupMember {
            get {
                return lookupMember;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Specifies the default value for the <see cref='System.ComponentModel.LookupBindingPropertiesAttribute'/>, which is <see langword='null'/>. This
        ///    <see langword='static '/>field is read-only. 
        ///    </para>
        /// </devdoc>
        public static readonly LookupBindingPropertiesAttribute Default = new LookupBindingPropertiesAttribute();

        public override bool Equals(object obj) {
            LookupBindingPropertiesAttribute other = obj as LookupBindingPropertiesAttribute; 
            return other != null &&
                   other.DataSource == dataSource &&
                   other.displayMember == displayMember &&
                   other.valueMember == valueMember &&
                   other.lookupMember == lookupMember;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
