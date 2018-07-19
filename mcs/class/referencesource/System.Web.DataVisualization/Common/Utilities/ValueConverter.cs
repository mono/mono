//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ValueConverter.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Utilities
//
//	Classes:	ValueConverter
//
//  Purpose:	Helper class which converts DateTime or numeric 
//              values to string. It used to display data point
//              values as labels, tooltips and axis labels.
//
//	Reviewed:	AG - August 7, 2002
//              AG - Microsoft 5, 2007
//
//===================================================================


#region Used Namespaces

using System;
using System.Globalization;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
#else
	using System.Web.UI.DataVisualization.Charting;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.Utilities
#else
	namespace System.Web.UI.DataVisualization.Charting.Utilities
#endif
{
	/// <summary>
    /// ValueConverter class is used when numeric or DateTime 
    /// value needs to be converted to a string using specified format.
	/// </summary>
	internal static class ValueConverter
	{
		#region Methods

        /// <summary>
        /// Converts value to string using specified format.
        /// </summary>
        /// <param name="chart">Reference to the chart object.</param>
        /// <param name="obj">Reference to the object being formatted.</param>
        /// <param name="objTag">Additional object tag.</param>
        /// <param name="value">Value converted to string.</param>
        /// <param name="format">Format string.</param>
        /// <param name="valueType">Value type.</param>
        /// <param name="elementType">Chart element type being formatted.</param>
		public static string FormatValue(
			Chart chart,
			object obj,
            object objTag,
			double value, 
			string format, 
			ChartValueType valueType,
			ChartElementType elementType)
		{
            format = format ?? String.Empty;
            string	convertionFormat = format;
			string	result = "";

			// Make sure value index is part of the format
			if(convertionFormat != null && convertionFormat.Length > 0)
			{
				int	bracketIndex = convertionFormat.IndexOf('{', 0);
				if(bracketIndex >= 0)
				{
					while(bracketIndex >= 0)
					{
						// If format is not followed by the value index
						if(!convertionFormat.Substring(bracketIndex).StartsWith("{0:", StringComparison.Ordinal))
						{
							// Check charcter prior to the bracket
							if(bracketIndex >= 1 && convertionFormat.Substring(bracketIndex - 1, 1) == "{")
							{
								continue;
							}
							else
							{
								// Insert value index in format
								convertionFormat = convertionFormat.Insert(bracketIndex + 1, "0:");
							}
						}

						bracketIndex = convertionFormat.IndexOf('{', bracketIndex + 1);
					}
				}
				else
				{
					convertionFormat = "{0:" + convertionFormat + "}";
				}
			}

			// Date/time formating
            if (valueType == ChartValueType.DateTime || 
                valueType == ChartValueType.DateTimeOffset || 
                valueType == ChartValueType.Date)
			{
				// Set default format
				if(convertionFormat.Length == 0)
				{
					convertionFormat = "{0:d}";
                    if (valueType == ChartValueType.DateTimeOffset)
                        convertionFormat += " +0";
				}

				// Convert date to string
                result = String.Format(CultureInfo.CurrentCulture, convertionFormat, DateTime.FromOADate(value));
			}
			else if(valueType == ChartValueType.Time)
			{
				// Set default format
				if(convertionFormat.Length == 0)
				{
					convertionFormat = "{0:t}";
				}

				// Convert date to string
                result = String.Format(CultureInfo.CurrentCulture, convertionFormat, DateTime.FromOADate(value));
			}
			else
			{
				bool	failedFlag = false;

				// Set default format
				if(convertionFormat.Length == 0)
				{
					convertionFormat = "{0:G}";
				}

				try
				{
					// Numeric value formatting
                    result = String.Format(CultureInfo.CurrentCulture,convertionFormat, value);
				}
				catch(FormatException)
				{
					failedFlag = true;
				}

				// If numeric formatting failed try to format using decimal number
				if(failedFlag)
				{
					failedFlag = false;
                    try
                    {
                        // Decimal value formatting
                        result = String.Format(CultureInfo.CurrentCulture, convertionFormat, (long)value);
                    }
                    catch (ArgumentNullException)
                    {
                        failedFlag = true;
                    }
                    catch (FormatException)
                    {
                        failedFlag = true;
                    }
				}

				// Return format string as result (literal) if all formatting methods failed
				if(failedFlag)
				{
                    result = format;
				}
			}

            // For the Reporting Services chart a special number formatting
            // handler may be set and used for all formatting needs.
            if (chart != null)
            {
                // Call number formatter
                FormatNumberEventArgs eventArguments = new FormatNumberEventArgs(value, format, valueType, result, objTag, elementType);
                chart.CallOnFormatNumber(obj, eventArguments);
                result = eventArguments.LocalizedValue;
            }

			return result;
		}
	
		#endregion
	}
}
