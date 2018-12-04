// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Windows.Markup
{
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.ComponentModel.Design.Serialization;

    class DateTimeOffsetConverter2 : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string) || destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <SecurityNote>
        /// Critical: Uses InstanceDescriptor, which LinkDemands
        /// Safe: InstanceDescriptor for DateTimeOffset doesn't contain any private data.
        ///       Also, the Descriptor is returned intact to the caller, who would need to satisfy a LinkDemand to do anything with it.
        /// </SecurityNote>
        [SecuritySafeCritical]
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if ((destinationType == typeof(string)) && (value is DateTimeOffset))
            {
                if (culture == null)
                {
                    culture = CultureInfo.CurrentCulture;
                }

                return ((DateTimeOffset)value).ToString("O", culture);
            }
            if ((destinationType == typeof(InstanceDescriptor)) && (value is DateTimeOffset))
            {
                // Use the year, month, day, hour, minute, second, millisecond, offset constructor
                // Should there be a branch to use the calendar constructor?
                DateTimeOffset dtOffset = (DateTimeOffset)value;

                Type intType = typeof(int);
                ConstructorInfo constructor = typeof(DateTimeOffset).GetConstructor(
                    new Type[] {
                        intType,
                        intType,
                        intType,
                        intType,
                        intType,
                        intType,
                        intType,
                        typeof(TimeSpan)
                    }
                    );

                if (constructor != null)
                {
                    return new InstanceDescriptor(
                        constructor,
                        new object[] {
                            dtOffset.Year,
                            dtOffset.Month,
                            dtOffset.Day,
                            dtOffset.Hour,
                            dtOffset.Minute,
                            dtOffset.Second,
                            dtOffset.Millisecond,
                            dtOffset.Offset
                        },
                        true);
                }

                return null;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string s = ((string)value).Trim();

                if (culture == null)
                {
                    culture = CultureInfo.CurrentCulture;
                }
                return DateTimeOffset.Parse(s, culture, DateTimeStyles.None);
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
