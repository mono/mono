//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		LegendConverters.cs
//
//  Namespace:	DataVisualization.Charting.Design
//
//	Classes:	LegendAreaNameConverter, LegendConverter,
//              SizeEmptyValueConverter, MarginExpandableObjectConverter,
//              IntNanValueConverter
//
//  Purpose:	Converter classes for Legend.
//
//	Reviewed:	AG - August 7, 2002
//
//===================================================================

#region Used Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Globalization;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
#else
	using System.Web.UI.WebControls;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
	namespace System.Web.UI.DataVisualization.Charting
#endif

{
	/// <summary>
	/// Chart area name converter. Displays list of available areas names
	/// </summary>
    internal class LegendAreaNameConverter : StringConverter
	{
		#region Converter methods

		/// <summary>
		/// Standart values supported - return true
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Standard values supported.</returns>
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		/// <summary>
		/// Standart values are not exclusive - return false
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Non exclusive standard values.</returns>
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}

		/// <summary>
		/// Fill in the list of data series names.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Standart values collection.</returns>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			ArrayList values = new ArrayList();
            values.Add(Constants.NotSetValue);

			ChartAreaCollection	areaCollection = null;
			string	areaName = "";
			if (context != null && context.Instance != null)
			{
				if (context.Instance is Legend)
				{
					Legend legend = (Legend)context.Instance;
					if(legend.Common != null && legend.Common.ChartPicture != null)
					{
						areaCollection = legend.Common.ChartPicture.ChartAreas;
					}
				}
				else if (context.Instance is ChartArea)
				{
					ChartArea area = (ChartArea)context.Instance;
					if(area.Common != null && area.Common.ChartPicture != null)
					{
						areaCollection = area.Common.ChartPicture.ChartAreas;
						areaName = area.Name;
					}
				}
				else if (context.Instance is Title)
				{
					Title title = (Title)context.Instance;
					if(title.Chart != null && title.Chart.chartPicture != null)
					{
						areaCollection = title.Chart.chartPicture.ChartAreas;
					}
				}

				else if (context.Instance is Annotation)
				{
					Annotation annotation = (Annotation)context.Instance;
					if(annotation.Chart != null && annotation.Chart.chartPicture != null)
					{
						areaCollection = annotation.Chart.chartPicture.ChartAreas;
					}
				}
                else if (context.Instance is IServiceProvider)
                {
                    IServiceProvider provider = context.Instance as IServiceProvider;

                    Chart chart = provider.GetService(typeof(Chart)) as Chart;

                    if (chart != null)
                    {
                        areaCollection = chart.ChartAreas;
                    }
                }
                else if (context.Instance is Array)
                {
                    if (((Array)context.Instance).Length > 0 && ((Array)context.Instance).GetValue(0) is Legend)
                    {
                        Legend legend = (Legend)((Array)context.Instance).GetValue(0);
                        if (legend.Common != null && legend.Common.ChartPicture != null)
                        {
                            areaCollection = legend.Common.ChartPicture.ChartAreas;
                        }
                    }
                    else if (((Array)context.Instance).Length > 0 && ((Array)context.Instance).GetValue(0) is ChartArea)
                    {
                        ChartArea area = (ChartArea)((Array)context.Instance).GetValue(0);
                        if (area.Common != null && area.Common.ChartPicture != null)
                        {
                            areaCollection = area.Common.ChartPicture.ChartAreas;
                        }
                    }
                    else if (((Array)context.Instance).Length > 0 && ((Array)context.Instance).GetValue(0) is Title)
                    {
                        Title title = (Title)((Array)context.Instance).GetValue(0);
                        if (title.Chart != null && title.Chart.chartPicture != null)
                        {
                            areaCollection = title.Chart.chartPicture.ChartAreas;
                        }
                    }

                    else if (((Array)context.Instance).Length > 0 && ((Array)context.Instance).GetValue(0) is Annotation)
                    {
                        Annotation annotation = (Annotation)((Array)context.Instance).GetValue(0);
                        if (annotation.Chart != null && annotation.Chart.chartPicture != null)
                        {
                            areaCollection = annotation.Chart.chartPicture.ChartAreas;
                        }
                    }


                }
			}

			if (areaCollection != null)
			{
				foreach(ChartArea area in areaCollection)
				{
					if(area.Name != areaName)
					{
						values.Add(area.Name);
					}
				}
			}

			return new StandardValuesCollection(values);
		}

		#endregion
	}

	/// <summary>
	/// Legend converter
	/// </summary>
    internal class LegendConverter : NoNameExpandableObjectConverter
	{
		#region Converter methods

#if !Microsoft_CONTROL
		/// <summary>
		/// Overrides the GetPropertiesSupported method of TypeConverter.
		/// Save reference to the descriptor context.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Indicates if properties are supported.</returns>
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			if (context != null && context.Instance != null)
			{
				// Save current control type descriptor context
				if(context.Instance is Chart)
				{
					Chart.controlCurrentContext = context;
				}
			}
			return base.GetPropertiesSupported(context);
		}
#endif // !Microsoft_CONTROL

		#endregion
	}

	/// <summary>
	/// Designer converter class
	/// Converts Size.Emty tofrom "Auto".
	/// </summary>
    internal class SizeEmptyValueConverter : System.Drawing.SizeConverter
	{
	#region Converter methods

		/// <summary>
		/// Standard values supported - return true
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Standard values supported.</returns>
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		/// <summary>
		/// Standard values are not exclusive - return false
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Non exclusive standard values.</returns>
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}

		/// <summary>
		/// Fill in the list of predefined values.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			ArrayList values = new ArrayList();
			values.Add(System.Drawing.Size.Empty);
		
			return new StandardValuesCollection(values);
		}

		/// <summary>
		/// Convert Size.IsEmpty value to string "Auto"
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="culture">Culture information.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="destinationType">Convertion destination type.</param>
		/// <returns>Converted object.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) 
		{  
			if (destinationType == typeof(string))
			{
				if(((System.Drawing.Size)value).IsEmpty)
				{
                    return Constants.AutoValue;
				}
			}

			// Call base class
			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// Convert minimum or maximum values from string
		/// </summary>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			// If converting from string value
            string stringValue = value as string;
            if (stringValue != null)
			{
                if (String.Compare(stringValue, Constants.AutoValue, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return System.Drawing.Size.Empty;
				}
			}
		
			// Call base converter
			return base.ConvertFrom(context, culture, value);
		}

	#endregion
	}

	/// <summary>
	/// Data point properties converter
	/// </summary>
    internal class MarginExpandableObjectConverter : ExpandableObjectConverter
	{
	#region Converter methods

		/// <summary>
		/// This method overrides CanConvertTo from TypeConverter. This is called when someone
		/// wants to convert an instance of object to another type.  Here,
		/// only conversion to an InstanceDescriptor is supported.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="destinationType">Destination type.</param>
		/// <returns>True if object can be converted.</returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				return true;
			}


			// Always call the base to see if it can perform the conversion.
			return base.CanConvertTo(context, destinationType);
		}

		/// <summary>
		/// Returns whether this converter can convert an object of the given type 
		/// to the type of this converter, using the specified context.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="sourceType">Source type.</param>
		/// <returns>True if object can be converted.</returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
			{
				return true;
			}

			// Always call the base to see if it can perform the conversion.
			return base.CanConvertFrom(context, sourceType);
		}

		/// <summary>
		/// This code performs the actual conversion from an object to a string.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="culture">Culture information.</param>
		/// <param name="value">Object value.</param>
		/// <param name="destinationType">Destination type.</param>
		/// <returns>Converted object.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			Margins margins = value as Margins;
			if (destinationType == typeof(string) && margins != null)
			{
				return string.Format( 
                    CultureInfo.InvariantCulture,
					"{0:D}, {1:D}, {2:D}, {3:D}", 
					margins.Top, 
					margins.Bottom, 
					margins.Left, 
					margins.Right);
			}

			// Always call base, even if you can't convert.
			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// Overrides the ConvertFrom method of TypeConverter.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="culture">Culture information.</param>
		/// <param name="value">Value to convert from.</param>
		/// <returns>Converted object.</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
		{
			// Can convert from string where each array element is separated by comma
            string stringValue = value as string;
			if (stringValue != null) 
			{
				Margins margins = new Margins();
				string[] values = stringValue.Split(',');
				if(values.Length == 4)
				{
					try
					{
                        margins.Top = int.Parse(values[0].Trim(), CultureInfo.InvariantCulture);
                        margins.Bottom = int.Parse(values[1].Trim(), CultureInfo.InvariantCulture);
                        margins.Left = int.Parse(values[2].Trim(), CultureInfo.InvariantCulture);
                        margins.Right = int.Parse(values[3].Trim(), CultureInfo.InvariantCulture);
					}
					catch
					{
                        throw (new InvalidOperationException(SR.ExceptionLegendDesignerMarginObjectInvalid(stringValue)));
					}
				}
				else
				{
                    throw (new InvalidOperationException(SR.ExceptionLegendDesignerMarginObjectInvalid(stringValue)));
				}

				return margins;
			}

			// Call base class
			return base.ConvertFrom(context, culture, value);
		}

	#endregion
	}

	/// <summary>
	/// Designer converter class
	/// Converts Integer value -1 to/from "Auto".
	/// </summary>
    internal class IntNanValueConverter : Int32Converter
	{
	#region Converter methods

		/// <summary>
		/// Standard values supported - return true
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Standard values supported.</returns>
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		/// <summary>
		/// Standard values are not exclusive - return false
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Non exclusive standard values.</returns>
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}

		/// <summary>
		/// Fill in the list of predefined values.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			ArrayList values = new ArrayList();
			values.Add(-1);
		
			return new StandardValuesCollection(values);
		}

		/// <summary>
		/// Convert integer value -1 to string "Auto"
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="culture">Culture information.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="destinationType">Convertion destination type.</param>
		/// <returns>Converted object.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType) 
		{  
			int	intValue = (int)value;
			if (destinationType == typeof(string))
			{
				if(intValue == -1)
				{
                    return Constants.AutoValue;
				}
			}

			// Call base class
			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// Convert minimum or maximum values from string
		/// </summary>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			// If converting from string value
            string stringValue = value as string;
            if (stringValue != null)
			{
                if (String.Compare(stringValue, Constants.AutoValue, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return -1;
				}
			}
			
			// Call base converter
			return base.ConvertFrom(context, culture, value);
		}

	#endregion
	}
}
