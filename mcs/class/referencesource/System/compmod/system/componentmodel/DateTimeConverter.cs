//------------------------------------------------------------------------------
// <copyright file="DateTimeConverter.cs" company="Microsoft">
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

    /// <devdoc>
    /// <para>Provides a type converter to convert <see cref='System.DateTime'/>
    /// objects to and from various other representations.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class DateTimeConverter : TypeConverter {
    
        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter can
        ///       convert an object in the given source type to a <see cref='System.DateTime'/>
        ///       object using the
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
        /// <para>Converts the given value object to a <see cref='System.DateTime'/>
        /// object.</para>
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string) {
                string text = ((string)value).Trim();
                if (text.Length == 0) {
                    return DateTime.MinValue;
                }
                try {
                    // See if we have a culture info to parse with.  If so, then use it.
                    //
                    DateTimeFormatInfo formatInfo = null;
                    
                    if (culture != null ) {
                        formatInfo = (DateTimeFormatInfo)culture.GetFormat(typeof(DateTimeFormatInfo));
                    }
                    
                    if (formatInfo != null) {
                        return DateTime.Parse(text, formatInfo);
                    }
                    else {
                        return DateTime.Parse(text, culture);
                    }
                }
                catch (FormatException e) {
                    throw new FormatException(SR.GetString(SR.ConvertInvalidPrimitive, (string)value, "DateTime"), e);
                }
            }
            
            return base.ConvertFrom(context, culture, value);
        }

        /// <devdoc>
        /// <para>Converts the given value object to a <see cref='System.DateTime'/>
        /// object
        /// using the arguments.</para>
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string) && value is DateTime) {
                DateTime dt = (DateTime) value;
                if (dt == DateTime.MinValue) {
                    return string.Empty;
                }
                
                if (culture == null) {
                    culture = CultureInfo.CurrentCulture;
                }

                DateTimeFormatInfo formatInfo = null;                
                formatInfo = (DateTimeFormatInfo)culture.GetFormat(typeof(DateTimeFormatInfo));
                                
                string format;
                if (culture == CultureInfo.InvariantCulture) {
                    if (dt.TimeOfDay.TotalSeconds == 0) {
                        return dt.ToString("yyyy-MM-dd", culture);
                    }
                    else {
                        return dt.ToString(culture);
                    }                
                }
                if (dt.TimeOfDay.TotalSeconds == 0) {
                    format = formatInfo.ShortDatePattern;
                }
                else {
                    format = formatInfo.ShortDatePattern + " " + formatInfo.ShortTimePattern;
                }
                
                return dt.ToString(format, CultureInfo.CurrentCulture);
            }
            if (destinationType == typeof(InstanceDescriptor) && value is DateTime) {
                DateTime dt = (DateTime)value;
                
                if (dt.Ticks == 0) {
                    // Make a special case for the empty DateTime
                    //
                    ConstructorInfo ctr = typeof(DateTime).GetConstructor(new Type[] {typeof(Int64)});
                        
                    if (ctr != null) {
                        return new InstanceDescriptor(ctr, new object[] {
                            dt.Ticks });
                    }
                }
                
                ConstructorInfo ctor = typeof(DateTime).GetConstructor(new Type[] {
                    typeof(int), typeof(int), typeof(int), typeof(int), 
                    typeof(int), typeof(int), typeof(int)});
                    
                if (ctor != null) {
                    return new InstanceDescriptor(ctor, new object[] {
                        dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond});
                }
            }
            
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

