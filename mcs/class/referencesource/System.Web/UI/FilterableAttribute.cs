//------------------------------------------------------------------------------
// <copyright file="FilterableAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 */
namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Security.Permissions;

    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class FilterableAttribute : Attribute {

        /// <internalonly/>
        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        public static readonly FilterableAttribute Yes = new FilterableAttribute(true);

        /// <internalonly/>
        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        public static readonly FilterableAttribute No = new FilterableAttribute(false);

        /// <internalonly/>
        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        public static readonly FilterableAttribute Default = Yes;

        private bool _filterable = false;
        private static Hashtable _filterableTypes;

        static FilterableAttribute() {
            // Create a synchronized wrapper
            _filterableTypes = Hashtable.Synchronized(new Hashtable());
        }

        /// <devdoc>
        /// </devdoc>
        public FilterableAttribute(bool filterable) {
            _filterable = filterable;
        }

        /// <devdoc>
        ///    <para> Indicates if the property is Filterable.</para>
        /// </devdoc>
        public bool Filterable {
            get {
                return _filterable;
            }
        }

        /// <internalonly/>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            FilterableAttribute other = obj as FilterableAttribute;
            return (other != null) && (other.Filterable == _filterable);
        }

        /// <internalonly/>
        public override int GetHashCode() {
            return _filterable.GetHashCode();
        }

        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }

        public static bool IsObjectFilterable(Object instance) {
            if (instance == null)
                throw new ArgumentNullException("instance");

            return IsTypeFilterable(instance.GetType());
        }

        public static bool IsPropertyFilterable(PropertyDescriptor propertyDescriptor) {
            FilterableAttribute filterableAttr = (FilterableAttribute)propertyDescriptor.Attributes[typeof(FilterableAttribute)];
            if (filterableAttr != null) {
                return filterableAttr.Filterable;
            }

            return true;
        }

        public static bool IsTypeFilterable(Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            object result = _filterableTypes[type];
            if (result != null) {
                return (bool)result;
            }

            System.ComponentModel.AttributeCollection attrs = TypeDescriptor.GetAttributes(type);
            FilterableAttribute attr = (FilterableAttribute)attrs[typeof(FilterableAttribute)];
            result = (attr != null) && attr.Filterable;
            _filterableTypes[type] = result;

            return (bool)result;
        }
    }
}

