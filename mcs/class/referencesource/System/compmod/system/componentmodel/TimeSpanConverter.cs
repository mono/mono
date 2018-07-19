//------------------------------------------------------------------------------
// <copyright file="TimeSpanConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel {
    using Microsoft.Win32;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Serialization.Formatters;
    using System.Security.Permissions;
    using System.Threading;

    /// <devdoc>
    /// <para>Provides a type converter to convert <see cref='System.TimeSpan'/>
    /// objects to and from various
    /// other representations.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class TimeSpanConverter : TypeConverter
    {
    
        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter can
        ///       convert an object in the given source type to a <see cref='System.TimeSpan'/> object using the
        ///       specified context.</para>
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

        /// <devdoc>
        /// <para>Converts the given object to a <see cref='System.TimeSpan'/>
        /// object.</para>
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture,  object value) {
            if (value is string) {
                string text = ((string)value).Trim();
                try {
                    return TimeSpan.Parse(text, culture);
                }
                catch (FormatException e) {
                    throw new FormatException(SR.GetString(SR.ConvertInvalidPrimitive, (string)value, "TimeSpan"), e);
                }
            }
            
            return base.ConvertFrom(context, culture, value);
        }
        
        /// <devdoc>
        ///      Converts the given object to another type.  The most common types to convert
        ///      are to and from a string object.  The default implementation will make a call
        ///      to ToString on the object if the object is valid and if the destination
        ///      type is string.  If this cannot convert to the desitnation type, this will
        ///      throw a NotSupportedException.
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == null) {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType == typeof(InstanceDescriptor) && value is TimeSpan) {
                MethodInfo method = typeof(TimeSpan).GetMethod("Parse", new Type[] {typeof(string)});
                if (method != null) {
                    return new InstanceDescriptor(method, new object[] {value.ToString()});
                }
            }
            
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

