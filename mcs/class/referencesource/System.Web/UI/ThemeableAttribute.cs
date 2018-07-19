//------------------------------------------------------------------------------
// <copyright file="ThemeableAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.ComponentModel;

    /// <devdoc>
    /// <para></para>
    /// </devdoc>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class ThemeableAttribute : Attribute {

        /// <internalonly/>
        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        public static readonly ThemeableAttribute Yes = new ThemeableAttribute(true);

        /// <internalonly/>
        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        public static readonly ThemeableAttribute No = new ThemeableAttribute(false);

        /// <internalonly/>
        /// <devdoc>
        /// <para></para>
        /// </devdoc>
        public static readonly ThemeableAttribute Default = Yes;

        private bool _themeable = false;
        private static Hashtable _themeableTypes;        

        static ThemeableAttribute() {
            // Create a synchronized wrapper
            _themeableTypes = Hashtable.Synchronized(new Hashtable());
        } 

        /// <devdoc>
        /// </devdoc>
        public ThemeableAttribute(bool themeable) {
            _themeable = themeable;
        }

        /// <devdoc>
        ///    <para> Indicates if the property is themeable.</para>
        /// </devdoc>
        public bool Themeable {
            get {
                return _themeable;
            }
        }

        /// <internalonly/>
        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }

            ThemeableAttribute other = obj as ThemeableAttribute;
            return (other != null) && (other.Themeable == _themeable);
        }

        /// <internalonly/>
        public override int GetHashCode() {
            return _themeable.GetHashCode();
        }

        /// <internalonly/>
        public override bool IsDefaultAttribute() {
            return this.Equals(Default);
        }

        public static bool IsObjectThemeable(Object instance) {
            if (instance == null)
                throw new ArgumentNullException("instance");

            return IsTypeThemeable(instance.GetType());
        }

        public static bool IsTypeThemeable(Type type) {
            if (type == null)
                throw new ArgumentNullException("type");

            object result = _themeableTypes[type];
            if (result != null) {
                return (bool)result;
            }

            //System.ComponentModel.AttributeCollection attrs = TypeDescriptor.GetAttributes(type);
            //ThemeableAttribute attr = (ThemeableAttribute)attrs[typeof(ThemeableAttribute)];
            ThemeableAttribute attr = Attribute.GetCustomAttribute(type, typeof(ThemeableAttribute)) as ThemeableAttribute;
            result = (attr != null) && attr.Themeable;
            _themeableTypes[type] = result;

            return (bool)result;
        }
    }
}
 
