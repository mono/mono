//------------------------------------------------------------------------------
// <copyright file="MinimizableAttributeTypeConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Globalization;

	/// <summary>
	/// Summary description for MinimizableAttributeTypeConverter.
	/// </summary>
    internal class MinimizableAttributeTypeConverter : BooleanConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            string strValue = value as string;
            if (strValue != null) {
                if ((strValue.Length > 0) && !String.Equals(strValue, "false", StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
                else {
                    return false;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

    }
}
