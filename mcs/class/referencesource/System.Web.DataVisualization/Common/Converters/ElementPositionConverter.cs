//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ElementPositionConverter.cs
//
//  Namespace:	DataVisualization.Charting.Design
//
//	Classes:	ElementPositionConverter
//
//  Purpose:	Converter of the element position.
//
//	Reviewed:	AG - August 7, 2002
//
//===================================================================


#region Used Namespaces

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
#else
    using System.Web.UI.DataVisualization.Charting;
#endif

#endregion

#if Microsoft_CONTROL

	namespace System.Windows.Forms.DataVisualization.Charting

#else
	namespace System.Web.UI.DataVisualization.Charting
#endif
{
	/// <summary>
	/// Element position converter.
	/// </summary>
    internal class ElementPositionConverter : ExpandableObjectConverter
	{
		#region Converter methods

		/// <summary>
		/// Overrides the CanConvertFrom method of TypeConverter.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="sourceType">Convertion source type.</param>
		/// <returns>Indicates if convertion is possible.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if(sourceType == typeof(string))
			{
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		/// Overrides the CanConvertTo method of TypeConverter.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="destinationType">Destination type.</param>
		/// <returns>Indicates if convertion is possible.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
            if (destinationType == typeof(string))
			{
				return true;
			}

			return base.CanConvertTo(context, destinationType);
		}

		/// <summary>
		/// Overrides the ConvertTo method of TypeConverter.
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
				return ((ElementPosition)value).ToString();
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// Overrides the ConvertFrom method of TypeConverter.
		/// Converts from string with comma separated values.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="culture">Culture information.</param>
		/// <param name="value">Value to convert from.</param>
		/// <returns>Indicates if convertion is possible.</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
		{
            string posValue = value as string;
			if(posValue != null) 
			{
                if (String.Compare(posValue, Constants.AutoValue, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return new ElementPosition();
				}
				else
				{
					string[]	array = posValue.Split(',');
					if(array.Length == 4)
					{
						return new ElementPosition(
							float.Parse(array[0], System.Globalization.CultureInfo.CurrentCulture), 
							float.Parse(array[1], System.Globalization.CultureInfo.CurrentCulture), 
							float.Parse(array[2], System.Globalization.CultureInfo.CurrentCulture), 
							float.Parse(array[3], System.Globalization.CultureInfo.CurrentCulture));
					}
					else
					{
						throw(new ArgumentException( SR.ExceptionElementPositionConverter ));
					}
				}
			}
			return base.ConvertFrom(context, culture, value);
		}
	
		#endregion
	}

}
