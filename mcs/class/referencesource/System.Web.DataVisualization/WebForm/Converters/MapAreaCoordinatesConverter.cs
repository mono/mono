//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		MapAreaCoordinatesConverter.cs
//
//  Namespace:	System.Web.UI.DataVisualization.Charting
//
//	Classes:	MapAreaCoordinatesConverter
//
//  Purpose:	Design-time converter for map area coordinates
//
//	Reviewed:	AG - August 7, 2002
//
//===================================================================

using System.ComponentModel;
using System.Globalization;

namespace System.Web.UI.DataVisualization.Charting
{
    /// <summary>
    /// Converter for the array of map area coordinates
    /// </summary>
    internal class MapAreaCoordinatesConverter : ArrayConverter
    {
        /// <summary>
        /// Overrides the CanConvertFrom method of TypeConverter.
        /// The ITypeDescriptorContext interface provides the context for the
        /// conversion. Typically this interface is used at design time to 
        /// provide information about the design-time container.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="sourceType">Convertion source type.</param>
        /// <returns>Indicates if convertion is possible.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Overrides the ConvertFrom method of TypeConverter.
        /// Convert from comma separated values in the string.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert from.</param>
        /// <returns>Indicates if convertion is possible.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            // Can convert from string where each array element is separated by comma
            string stringValue = value as string;
            if (stringValue != null)
            {
                string[] values = stringValue.Split(new char[] { ',' });
                float[] array = new float[values.Length];
                for (int index = 0; index < values.Length; index++)
                {
                    array[index] = float.Parse(values[index], System.Globalization.CultureInfo.CurrentCulture);
                }

                return array;
            }

            // Call base class
            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Overrides the ConvertTo method of TypeConverter.
        /// Convert coordinates array to comma separated values in string.
        /// </summary>
        /// <param name="context">Descriptor context.</param>
        /// <param name="culture">Culture information.</param>
        /// <param name="value">Value to convert.</param>
        /// <param name="destinationType">Convertion destination type.</param>
        /// <returns>Converted object.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                float[] array = (float[])value;
                string result = "";

                foreach (float d in array)
                {
                    result += d.ToString(System.Globalization.CultureInfo.CurrentCulture) + ",";
                }

                return result.TrimEnd(',');
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
