
//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		AxesArrayConverter.cs
//
//  Namespace:	DataVisualization.Charting.Design
//
//	Classes:	AxesArrayConverter
//
//  Purpose:	Converter for the Axes array.
//
//	Reviewed:	AG - August 7, 2002
//
//===================================================================

#region Used Namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.IO;
using System.Globalization;
using System.Data;
using System.Reflection;
#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;


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
	/// Converter object of axes array
	/// </summary>
    internal class AxesArrayConverter : TypeConverter
	{
		#region Converter methods

		/// <summary>
		/// Subproperties NOT suported.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <returns>Always false.</returns>
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return false;
		}

		/// <summary>
		/// Overrides the ConvertTo method of TypeConverter.
		/// </summary>
		/// <param name="context">Descriptor context.</param>
		/// <param name="culture">Culture information.</param>
		/// <param name="value">Value.</param>
		/// <param name="destinationType">Destination type.</param>
		/// <returns>Converted object.</returns>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) 
		{  
			// Convert collection to string
			if (destinationType == typeof(string)) 
			{
                return (new CollectionConverter()).ConvertToString(new ArrayList());
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		#endregion
	}
}
