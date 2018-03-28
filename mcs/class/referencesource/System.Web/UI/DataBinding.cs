//------------------------------------------------------------------------------
// <copyright file="DataBinding.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI {

    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.Util;
    

    /// <devdoc>
    ///    <para>Enables RAD designers to create data binding expressions 
    ///       at design time. This class cannot be inherited.</para>
    /// </devdoc>
    public sealed class DataBinding {

        private string propertyName;
        private Type propertyType;
        private string expression;


        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Web.UI.DataBinding'/> class.</para>
        /// </devdoc>
        public DataBinding(string propertyName, Type propertyType, string expression) {
            this.propertyName = propertyName;
            this.propertyType = propertyType;
            this.expression = expression;
        }



        /// <devdoc>
        ///    <para>Indicates the data binding expression to be evaluated.</para>
        /// </devdoc>
        public string Expression {
            get {
                return expression;
            }
            set {
                Debug.Assert((value != null) && (value.Length != 0),
                             "Invalid expression");
                expression = value;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the name of the property that is to be data bound against. This 
        ///       property is read-only.</para>
        /// </devdoc>
        public string PropertyName {
            get {
                return propertyName;
            }
        }


        /// <devdoc>
        ///    <para>Indicates the type of the data bound property. This property is 
        ///       read-only.</para>
        /// </devdoc>
        public Type PropertyType {
            get {
                return propertyType;
            }
        }


        /// <devdoc>
        ///     DataBinding objects are placed in a hashtable representing the collection
        ///     of bindings on a control. There can only be one binding/property, so
        ///     the hashcode computation should match the Equals implementation and only
        ///    take the property name into account.
        /// </devdoc>
        public override int GetHashCode() {
            return propertyName.ToLower(CultureInfo.InvariantCulture).GetHashCode();
        }


        /// <devdoc>
        /// </devdoc>
        public override bool Equals(object obj) {
            if ((obj != null) && (obj is DataBinding)) {
                DataBinding binding = (DataBinding)obj;

                return StringUtil.EqualsIgnoreCase(propertyName, binding.PropertyName);
            }
            return false;
        }
    }
}

