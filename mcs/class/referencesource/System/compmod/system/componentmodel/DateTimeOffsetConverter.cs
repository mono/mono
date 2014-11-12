//------------------------------------------------------------------------------
// <copyright file="DateTimeOffsetConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
DevDiv Bugs 181818: DateTimeOffset should have a type converter just like DateTime.
This converter should behave just like DateTimeConverter only it should convert DateTimeOffsets.
The code was copied from DateTimeConverter and adapted for DateTimeOffset.
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
    /// <para>Provides a type converter to convert <see cref='System.DateTimeOffset'/>
    /// objects to and from various other representations.</para>
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public class DateTimeOffsetConverter : TypeConverter {
    
        /// <devdoc>
        ///    <para>Gets a value indicating whether this converter can
        ///       convert an object in the given source type to a <see cref='System.DateTimeOffset'/>
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
                    return DateTimeOffset.MinValue;
                }
                try {
                    // See if we have a culture info to parse with.  If so, then use it.
                    //
                    DateTimeFormatInfo formatInfo = null;
                    
                    if (culture != null ) {
                        formatInfo = (DateTimeFormatInfo)culture.GetFormat(typeof(DateTimeFormatInfo));
                    }
                    
                    if (formatInfo != null) {
                        return DateTimeOffset.Parse(text, formatInfo);
                    }
                    else {
                        return DateTimeOffset.Parse(text, culture);
                    }
                }
                catch (FormatException e) {
                    throw new FormatException(SR.GetString(SR.ConvertInvalidPrimitive, (string)value, "DateTimeOffset"), e);
                }
            }
            
            return base.ConvertFrom(context, culture, value);
        }

        /// <devdoc>
        /// <para>Converts the given value object to a <see cref='System.DateTimeOffset'/>
        /// object
        /// using the arguments.</para>
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            // logic is exactly as in DateTimeConverter, only the offset pattern ' zzz' is added to the default
            // ConvertToString pattern.
            if (destinationType == typeof(string) && value is DateTimeOffset) {
                DateTimeOffset dto = (DateTimeOffset) value;
                if (dto == DateTimeOffset.MinValue) {
                    return string.Empty;
                }
                
                if (culture == null) {
                    culture = CultureInfo.CurrentCulture;
                }

                DateTimeFormatInfo formatInfo = null;                
                formatInfo = (DateTimeFormatInfo)culture.GetFormat(typeof(DateTimeFormatInfo));
                                
                string format;
                if (culture == CultureInfo.InvariantCulture) {
                    // note: the y/m/d format used when there is or isn't a time component is not consistent
                    // when the culture is invariant, because for some reason InvariantCulture's date format is
                    // MM/dd/yyyy. However, this matches the behavior of DateTimeConverter so it is preserved here.

                    if (dto.TimeOfDay.TotalSeconds == 0) {
                        // pattern just like DateTimeConverter when DateTime.TimeOfDay.TotalSeconds==0
                        // but with ' zzz' offset pattern added.
                        return dto.ToString("yyyy-MM-dd zzz", culture);
                    }
                    else {
                        return dto.ToString(culture);
                    }                
                }
                if (dto.TimeOfDay.TotalSeconds == 0) {
                    // pattern just like DateTimeConverter when DateTime.TimeOfDay.TotalSeconds==0
                    // but with ' zzz' offset pattern added.
                    format = formatInfo.ShortDatePattern + " zzz";
                }
                else {
                    // pattern just like DateTimeConverter when DateTime.TimeOfDay.TotalSeconds!=0
                    // but with ' zzz' offset pattern added.
                    format = formatInfo.ShortDatePattern + " " + formatInfo.ShortTimePattern + " zzz";
                }
                
                return dto.ToString(format, CultureInfo.CurrentCulture);
            }
            if (destinationType == typeof(InstanceDescriptor) && value is DateTimeOffset) {
                DateTimeOffset dto = (DateTimeOffset)value;
                
                if (dto.Ticks == 0) {
                    // Make a special case for the empty DateTimeOffset
                    //
                    ConstructorInfo ctr = typeof(DateTimeOffset).GetConstructor(new Type[] {typeof(Int64)});
                        
                    if (ctr != null) {
                        return new InstanceDescriptor(ctr, new object[] {
                            dto.Ticks });
                    }
                }
                
                ConstructorInfo ctor = typeof(DateTimeOffset).GetConstructor(new Type[] {
                    typeof(int), typeof(int), typeof(int), typeof(int), 
                    typeof(int), typeof(int), typeof(int), typeof(TimeSpan) });
                    
                if (ctor != null) {
                    return new InstanceDescriptor(ctor, new object[] {
                        dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, dto.Millisecond, dto.Offset });
                }
            }
            
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

