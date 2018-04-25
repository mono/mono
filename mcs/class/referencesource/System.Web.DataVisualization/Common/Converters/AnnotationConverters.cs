//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		AnnotationConverters.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	AnchorPointValueConverter, AnnotationAxisValueConverter
//
//  Purpose:	Annotation Converters.
//
//	Reviewed:	
//
//===================================================================


#region Used namespace
using System;
using System.Globalization;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
#else
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.DataVisualization.Charting.Data;
using System.Web.UI.DataVisualization.Charting.Utilities;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
    namespace System.Web.UI.DataVisualization.Charting
#endif
{
    /// <summary>
	/// Converts anchor data point to string name.
	/// </summary>
	internal class AnchorPointValueConverter : TypeConverter
	{
		#region Converter methods

	/// <summary>
	/// Converts anchor data point to string name.
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
            if (value == null)
            {
                return Constants.NotSetValue;
            }
            DataPoint dataPoint = value as DataPoint;

            if (dataPoint != null)
            {
                if (dataPoint.series != null)
                {
                    int pointIndex = dataPoint.series.Points.IndexOf(dataPoint) + 1;
                    return dataPoint.series.Name + " - " + SR.DescriptionTypePoint + pointIndex.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

		// Call base class
		return base.ConvertTo(context, culture, value, destinationType);
	}
		#endregion
	}

	/// <summary>
	/// Converts anchor data point to string name.
	/// </summary>
    internal class AnnotationAxisValueConverter : TypeConverter
	{
		#region Converter methods

		/// <summary>
		/// Converts axis associated with anootation to string.
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
                if (value == null)
                {
                    return Constants.NotSetValue;
                }

                Axis axis = value as Axis;
                if (axis != null)
                {
                    if (axis.ChartArea != null)
                    {
                        return axis.ChartArea.Name + " - " + axis.Name;
                    }
                }
            }

            // Call base class
            return base.ConvertTo(context, culture, value, destinationType);
        }
		#endregion
	}
}

