//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		DataManagerConverters.cs
//
//  Namespace:	DataVisualization.Charting.Design
//
//	Classes:	SeriesAreaNameConverter, 
//				ChartTypeConverter, SeriesNameConverter, 
//				NoNameExpandableObjectConverter, DoubleArrayConverter,
//				DataPointValueConverter, SeriesYValueTypeConverter
//
//  Purpose:	Converter classes for the Series and DataPoint properties.
//
//	Reviewed:	AG - August 7, 2002
//
//===================================================================



using System.ComponentModel.Design.Serialization;
#region Used Namespaces

using System;
using System.Resources;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Drawing.Text;
using System.IO;
using System.Globalization;
using System.Data;
using System.Reflection;
#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
    using System.Collections.Generic;

#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.WebControls;
    using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
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
    internal class SeriesAreaNameConverter : StringConverter
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
		/// Fill in the list of the chart areas for the series.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Standart values collection.</returns>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			ArrayList values = new ArrayList();

            Chart chart = ConverterHelper.GetChartFromContext(context);

            if (chart != null)
            {
                foreach (ChartArea area in chart.ChartAreas)
                {
                    values.Add(area.Name);
                }
            }
			return new StandardValuesCollection(values);
		}
	
		#endregion
	}

	/// <summary>
	/// Chart data source design-time converter. Displays list of available data sources.
	/// </summary>
    internal class ChartDataSourceConverter : StringConverter
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
				return true;
			}

			/// <summary>
			/// Fill in the list of chart type names.
			/// </summary>
			/// <param name="context">Descriptor context.</param>
			/// <returns>Standard values collection.</returns>
			public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				ArrayList values = new ArrayList();

				if (context != null && context.Container != null)
				{
					// Loop through all components in the container
					foreach(IComponent comonent in context.Container.Components)
					{
						// Check if component can be a data source
						if(ChartImage.IsValidDataSource(comonent))
						{
							// Add component name
							values.Add(comonent.Site.Name);
						}
					}
				}

				// Add "None" data source
				values.Add("(none)");

				return new StandardValuesCollection(values);
			}

		#endregion
	}

	/// <summary>
	/// Series data source members converter.
	/// </summary>
    internal class SeriesDataSourceMemberConverter : StringConverter
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
			/// Fill in the list of the data source members.
			/// </summary>
			/// <param name="context">Descriptor context.</param>
			/// <returns>Standart values collection.</returns>
			public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
			{
				ArrayList values = new ArrayList();

                Chart chart = ConverterHelper.GetChartFromContext(context);
                object dataSource = null;

				if(chart != null)
				{
                    if (chart != null && ChartImage.IsValidDataSource(chart.DataSource))
                    {
                        dataSource = chart.DataSource;
                    }

                    // Check if it's Y values member
					bool usedForYValues = false;
                    if (context.PropertyDescriptor != null && context.PropertyDescriptor.Name == "YValueMembers")
					{
						usedForYValues = true;
					}

					// Populate list with all members names
					ArrayList	memberNames = ChartImage.GetDataSourceMemberNames(dataSource, usedForYValues);
					foreach(string name in memberNames)
					{
						values.Add(name);
					}

					values.Add("(none)");
				}

				return new StandardValuesCollection(values);
			}

	#endregion
	}

	/// <summary>
	/// Chart legend name converter. Displays list of available legend names
	/// </summary>
    internal class SeriesLegendNameConverter : StringConverter
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
		/// Fill in the list of the chart legend for the series.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Standart values collection.</returns>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			ArrayList values = new ArrayList();

            Chart chart = ConverterHelper.GetChartFromContext(context);

            if (chart != null)
            {
                foreach (Legend legend in chart.Legends)
                {
                    values.Add(legend.Name);
                }
            }

			return new StandardValuesCollection(values);
		}

	#endregion
	}

	/// <summary>
	/// Chart type converter. Displays list of available chart type names
	/// </summary>
    internal class ChartTypeConverter : StringConverter
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
			return true;
		}

		/// <summary>
		/// Fill in the list of chart type names.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Standard values collection.</returns>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			ChartTypeRegistry	registry = null;
			ArrayList values = new ArrayList();

            Chart chart = ConverterHelper.GetChartFromContext(context);
            if (chart!=null)
			{
				// Get chart type registry service
				registry = (ChartTypeRegistry)chart.GetService(typeof(ChartTypeRegistry));
				if(registry != null)
				{
					// Enumerate all chart types names
					foreach(Object obj in registry.registeredChartTypes.Keys)
					{
						if(obj is string)
						{
							values.Add(obj);
						}
					}
				}
				else
				{
                    throw (new InvalidOperationException(SR.ExceptionEditorChartTypeRegistryServiceInaccessible));
				}
			}

			// Sort all values
			values.Sort();

			return new StandardValuesCollection(values);
		}
	
		#endregion
	}


	/// <summary>
	/// Data series name converter. Displays list of available series names
	/// </summary>
    internal class SeriesNameConverter : StringConverter
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
		/// <returns>Standard values collection.</returns>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			DataManager	dataManager = null;
			ArrayList values = new ArrayList();

			if (context != null && context.Instance != null)
			{
				// Call GetService method using reflection
				MethodInfo	methodInfo = context.Instance.GetType().GetMethod("GetService");
				if(methodInfo != null)
				{
					object[] parameters = new object[1];
					parameters[0] = typeof(DataManager);
					dataManager = (DataManager)methodInfo.Invoke(context.Instance, parameters);
				}

				// If data manager service was seccesfully retrived
				if(dataManager != null)
				{
					foreach(Series series in dataManager.Series)
					{
						values.Add(series.Name);
					}
				}
				else
				{
                    throw (new InvalidOperationException(SR.ExceptionEditorChartTypeRegistryServiceInObjectInaccessible(context.Instance.GetType().ToString())));
				}
			}

			return new StandardValuesCollection(values);
		}
	
		#endregion
	}

	/// <summary>
	/// Data point properties converter
	/// </summary>
    internal class NoNameExpandableObjectConverter : ExpandableObjectConverter
	{
		#region Converter methods

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
			if (context != null && context.Instance != null)
			{
				if (destinationType == typeof(string)) 
				{
					return "";
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		#endregion
	}

	/// <summary>
	/// Converter for the array of doubles
	/// </summary>
    internal class DoubleArrayConverter : ArrayConverter
	{
		#region Converter methods

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
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="culture">Culture information.</param>
		/// <param name="value">Value to convert from.</param>
		/// <returns>Converted object.</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) 
		{
			object	result = null;
			bool	convertFromDate = false;

			// Try to check if value type is date
			if (context != null && context.Instance != null)
			{
				DataPoint dataPoint = (DataPoint)context.Instance;
				if(dataPoint.series != null && dataPoint.series.IsYValueDateTime())
				{
					convertFromDate = true;
				}
			}

			// Can convert from string where each array element is separated by comma
            string stringValue = value as string;
			if (stringValue != null) 
			{
				string[] values = stringValue.Split(new char[] {','});
				double[] array = new double[values.Length];
				for(int index = 0; index < values.Length; index ++)
				{
					// Try to convert from date-time string format
                    if (convertFromDate)
                    {
                        DateTime valueAsDate;
                        if (DateTime.TryParse(values[index], CultureInfo.InvariantCulture, DateTimeStyles.None, out valueAsDate))
                        {
                            result = valueAsDate;
                        }
                        else if (DateTime.TryParse(values[index], CultureInfo.CurrentCulture, DateTimeStyles.None, out valueAsDate))
                        {
                            result = valueAsDate;
                        }
                        else
                        {
                            result = null;
                        }
                    }

					// Save converted value in the array
					if(result != null)
					{
						array[index] = (double)result;
					}
					else
					{
						array[index] = CommonElements.ParseDouble(values[index]);
					}
				}
										
				return array;
			}

			// Call base class
			return base.ConvertFrom(context, culture, value);
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
			bool	convertToDate = false;

			// Check if we should convert to date string format
			if (context != null && context.Instance != null)
			{
				DataPoint dataPoint = (DataPoint)context.Instance;
				if(dataPoint.series != null && dataPoint.series.IsYValueDateTime())
				{
					convertToDate = true;
				}
			}


			if (destinationType == typeof(string)) 
			{
				double[] array = (double[]) value;
				string result = "";
					
				foreach(double d in array)
				{
					if(convertToDate)
					{
						result += DateTime.FromOADate(d).ToString("g", System.Globalization.CultureInfo.InvariantCulture) + ",";
					}
					else
					{
						result += d.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",";
					}
				}

				return result.TrimEnd(',');
			}


			return base.ConvertTo(context, culture, value, destinationType);
		}
	
		#endregion
	}

	/// <summary>
	/// Converts data point values to and from date string format
	/// </summary>
    internal class DataPointValueConverter : DoubleConverter
	{
		#region Converter methods

		/// <summary>
		/// Convert values to date string
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="culture">Culture information.</param>
		/// <param name="value">Value to convert.</param>
		/// <param name="destinationType">Convertion destination type.</param>
		/// <returns>Converted object.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) 
		{  
			if (context != null && context.Instance != null)
			{
				DataPoint	dataPoint = (DataPoint)context.Instance;

				if (destinationType == typeof(string) && dataPoint.series.IsXValueDateTime()) 
				{
					DateTime valueAsSate = DateTime.FromOADate((double)value);
					return valueAsSate.ToString("g", System.Globalization.CultureInfo.CurrentCulture);
				}
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

		/// <summary>
		/// Convert values from date string.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="culture">Culture information.</param>
		/// <param name="value">Value to convert from.</param>
		/// <returns>Converted object.</returns>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (context != null && context.Instance != null)
			{
                string stringValue = value as string;

                if (stringValue != null)
                {
                    DataPoint dataPoint = (DataPoint)context.Instance;

                    if (dataPoint.series.IsXValueDateTime())
                    {
                        DateTime valueAsSate = DateTime.Parse(stringValue, System.Globalization.CultureInfo.CurrentCulture);
                        return valueAsSate.ToOADate();
                    }
                }
			}

			return base.ConvertFrom(context, culture, value);
		}

		#endregion
	}

	/// <summary>
	/// Removes the String type for Y axes
	/// </summary>
    internal class SeriesYValueTypeConverter : EnumConverter
	{
		#region Converter methods

		/// <summary>
		/// Public constructor
		/// </summary>
		/// <param name="type">Enumeration type.</param>
		public SeriesYValueTypeConverter(Type type) : base(type)
		{
		}

		/// <summary>
		/// Fill in the list of data series names.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Standard values collection.</returns>
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			ArrayList values = new ArrayList();

			// Call base class
			StandardValuesCollection	val = base.GetStandardValues(context);

			// Remove string type
			foreach(object o in val)
			{
				if(o.ToString() != "String")
				{
					
					values.Add(o);
				}
			}

			return new StandardValuesCollection(values);
		}
	
		#endregion
	}

	/// <summary>
	/// Data point properties converter
	/// </summary>
    internal class ColorArrayConverter : TypeConverter
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
				return ColorArrayToString(value as Color[]);
			}

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
                return StringToColorArray(stringValue);
			}

			// Call base class
			return base.ConvertFrom(context, culture, value);
		}

		/// <summary>
		/// Converts array of colors into string.
		/// </summary>
		/// <param name="colors">Colors array.</param>
		/// <returns>Result string.</returns>
		public static string ColorArrayToString(Color[] colors)
		{
			if(colors != null && colors.GetLength(0) > 0)
			{
				ColorConverter colorConverter = new ColorConverter();
				string result = string.Empty;
				foreach(Color color in colors)
				{
					if(result.Length > 0)
					{
						result += "; ";
					}
					result += colorConverter.ConvertToInvariantString(color);
				}
				return result;
			}
			return string.Empty;
		}

		/// <summary>
		/// Converts string into array of colors.
		/// </summary>
		/// <param name="colorNames">String data.</param>
		/// <returns>Array of colors.</returns>
		public static Color[] StringToColorArray(String colorNames)
		{
			ColorConverter colorConverter = new ColorConverter();
			Color[] array = new Color[0];
			if(colorNames.Length > 0)
			{
				string[] colorValues = colorNames.Split(';');
				array = new Color[colorValues.Length];
				int index = 0;
				foreach(string str in colorValues)
				{
					array[index++] = (Color)colorConverter.ConvertFromInvariantString(str);
				}
			}
			return array;
		}

		#endregion
	}

    /// <summary>
    /// Provides a set of helper methods used by converters
    /// </summary>
    internal static class ConverterHelper 
    {

        #region Static
        /// <summary>
        /// Gets the chart from context.
        /// </summary>
        /// <param name="context">The context.</param>
        public static Chart GetChartFromContext(ITypeDescriptorContext context)
        {
            if (context == null || context.Instance == null)
            {
                return null;
            }

            IChartElement element = context.Instance as IChartElement;
            if (element != null && element.Common != null)
            {
                return element.Common.Chart;
            }

            IList list = context.Instance as IList;
            if (list != null && list.Count > 0)
            {
                element = list[0] as IChartElement;
                if (element.Common != null)
                {
                    return element.Common.Chart;
                }

            }

            Chart chart = context.Instance as Chart;
            if (chart != null)
            {
                return chart;
            }

            IServiceProvider provider = context.Instance as IServiceProvider;
            if (provider != null)
            {
                chart = provider.GetService(typeof(Chart)) as Chart;
                if (chart != null)
                {
                    return chart;
                }
            }

            return null;
        }
        #endregion
    }
}


