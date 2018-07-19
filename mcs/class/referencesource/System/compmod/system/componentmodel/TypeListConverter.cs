//------------------------------------------------------------------------------
// <copyright file="TypeListConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using Microsoft.Win32;
    using System.Collections;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;

    /// <devdoc>
    ///    <para>Provides a type
    ///       converter that can be used to populate a list box with available types.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public abstract class TypeListConverter : TypeConverter {
        private Type[] types;
        private StandardValuesCollection values;
    
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.TypeListConverter'/> class using
        ///    the type array as the available types.</para>
        /// </devdoc>
        protected TypeListConverter(Type[] types) {
            this.types = types;
        }
    
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter
        ///       can convert an object in the given source type to an enumeration object using
        ///       the specified context.</para>
        /// </devdoc>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }
        
        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter can
        ///       convert an object to the given destination type using the context.</para>
        /// </devdoc>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if (destinationType == typeof(InstanceDescriptor)) {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Converts the specified value object to an enumeration object.</para>
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string) {
                foreach(Type t in types) {
                    if (value.Equals(t.FullName)) {
                        return t;
                    }
                }
            }
            
            return base.ConvertFrom(context, culture, value);
        }
    
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Converts the given value object to the specified destination type.</para>
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == null) {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(string)) {
                if (value == null) {
                    return SR.GetString(SR.toStringNone);
                }
                else {
                    return((Type)value).FullName;
                }
            }
            if (destinationType == typeof(InstanceDescriptor) && value is Type) {
                MethodInfo method = typeof(Type).GetMethod("GetType", new Type[] {typeof(string)});
                if (method != null) {
                    return new InstanceDescriptor(method, new object[] {((Type)value).AssemblyQualifiedName});
                }
            }
            
            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a collection of standard values for the data type this validator is
        ///       designed for.</para>
        /// </devdoc>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            if (values == null) {
                object[] objTypes;
                
                if (types != null) {
                    objTypes = new object[types.Length];
                    Array.Copy(types, objTypes, types.Length);
                }
                else {
                    objTypes = null;
                }
                
                values = new StandardValuesCollection(objTypes);
            }
            return values;
        }
    
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a value indicating whether the list of standard values returned from
        ///    <see cref='System.ComponentModel.TypeListConverter.GetStandardValues'/> is an exclusive list. </para>
        /// </devdoc>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            return true;
        }
        
        /// <internalonly/>
        /// <devdoc>
        ///    <para>Gets a value indicating whether this object supports a
        ///       standard set of values that can be picked from a list using the specified
        ///       context.</para>
        /// </devdoc>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }
    }
}

