//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartAreaCircular.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	CircularChartAreaAxis
//
//  Purpose:	CircularChartAreaAxis is a helper class which is used
//              in circular chart areas for charts like Polar and 
//              Radar. 
//
//	Reviewed:	AG - Microsoft 16, 2007
//
//===================================================================


#region Used namespaces
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
#if Microsoft_CONTROL

using System.Windows.Forms.DataVisualization.Charting.Data;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
using System.Windows.Forms.DataVisualization.Charting.Utilities;
using System.Windows.Forms.DataVisualization.Charting.Borders3D;
using System.Windows.Forms.DataVisualization.Charting;

using System.Globalization;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Windows.Forms.Design;
#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
	using System.Web.UI.DataVisualization.Charting.Utilities;
	using System.Web.UI.DataVisualization.Charting.Borders3D;
#endif

#endregion

#if Microsoft_CONTROL
namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	/// <summary>
    /// CircularChartAreaAxis class represents a single axis in the circular 
    /// chart area chart like radar or polar. It contains axis angular 
    /// position, size and title properties.
	/// </summary>
	internal class CircularChartAreaAxis
	{
		#region Fields

		/// <summary>
		/// Angle where axis is located.
		/// </summary>
		internal	float	AxisPosition = 0f;

		/// <summary>
		/// Axis title.
		/// </summary>
		internal	string	Title = string.Empty;

        /// <summary>
        /// Axis title color.
        /// </summary>
        internal    Color TitleForeColor = Color.Empty;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public CircularChartAreaAxis()
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		internal CircularChartAreaAxis(float axisPosition)
		{
			this.AxisPosition = axisPosition;
		}

		#endregion
	}
}
