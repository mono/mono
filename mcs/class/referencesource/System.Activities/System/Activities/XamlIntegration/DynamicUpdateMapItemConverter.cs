// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities.DynamicUpdate;
    using System.ComponentModel;
    using System.Globalization;    

    public class DynamicUpdateMapItemConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string stringValue = value as string;
            if (stringValue != null)
            {
                int result1;
                int result2;
                string[] strArray = stringValue.Split(new char[] { '.' });
                if (strArray.Length == 1)
                {
                    if (int.TryParse(strArray[0], NumberStyles.Integer, culture, out result1))
                    {
                        return new DynamicUpdateMapItem(result1);
                    }
                }
                else if (strArray.Length == 2)
                {
                    if (int.TryParse(strArray[0], NumberStyles.Integer, culture, out result1) && int.TryParse(strArray[1], NumberStyles.Integer, culture, out result2))
                    {
                        return new DynamicUpdateMapItem(result1, result2);
                    }
                }                
            }            

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            DynamicUpdateMapItem objectInfo = value as DynamicUpdateMapItem;
            if (destinationType == typeof(string) && objectInfo != null)
            {
                if (objectInfo.IsVariableMapItem)
                {
                    // MapItem for Variable is converted to a string with its owner Id plus the variable index delimeted by "."
                    // Assumption:  No culture uses "." in its Int32 string representation
                    return objectInfo.OriginalVariableOwnerId.ToString(culture) + "." + objectInfo.OriginalId.ToString(culture);
                }
                return objectInfo.OriginalId.ToString(culture);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
