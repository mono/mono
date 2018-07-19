//------------------------------------------------------------------------------
// <copyright file="unitconverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;
    using System.Web.Util;


    public class UnitConverter : TypeConverter {


        /// <internalonly/>
        /// <devdoc>
        ///   Returns a value indicating whether the unit converter can 
        ///   convert from the specified source type.
        /// </devdoc>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }
            else {
                return base.CanConvertFrom(context, sourceType);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///   Returns a value indicating whether the converter can
        ///   convert to the specified destination type.
        /// </devdoc>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if ((destinationType == typeof(string)) ||
                (destinationType == typeof(InstanceDescriptor))) {
                return true;
            }
            else {
                return base.CanConvertTo(context, destinationType);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///   Performs type conversion from the given value into a Unit.
        /// </devdoc>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value == null)
                return null;

            string stringValue = value as string;
            if (stringValue != null) {
                string textValue = stringValue.Trim();
                if (textValue.Length == 0) {
                    return Unit.Empty;
                }
                if (culture != null)  {
                    return Unit.Parse(textValue, culture);
                }
                else {
                    return Unit.Parse(textValue, CultureInfo.CurrentCulture);
                }
            }
            else {
                return base.ConvertFrom(context, culture, value);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///   Performs type conversion to the specified destination type
        /// </devdoc>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if (destinationType == typeof(string)) {
                if ((value == null) || ((Unit)value).IsEmpty)
                    return String.Empty;
                else
                    return ((Unit)value).ToString(culture);
            }
            else if ((destinationType == typeof(InstanceDescriptor)) && (value != null)) {
                Unit u = (Unit)value;
                MemberInfo member = null;
                object[] args = null;

                if (u.IsEmpty) {
                    member = typeof(Unit).GetField("Empty");
                }
                else {
                    member = typeof(Unit).GetConstructor(new Type[] { typeof(double), typeof(UnitType) });
                    args = new object[] { u.Value, u.Type };
                }

                Debug.Assert(member != null, "Looks like we're missing Unit.Empty or Unit::ctor(double, UnitType)");
                if (member != null) {
                    return new InstanceDescriptor(member, args);
                }
                else {
                    return null;
                }
            }
            else {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}

