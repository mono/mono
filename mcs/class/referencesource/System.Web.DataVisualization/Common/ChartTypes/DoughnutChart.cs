//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		DoughnutChart.cs
//
//  Namespace:	System.Web.UI.DataVisualization.Charting.ChartTypes
//
//	Classes:	DoughnutChart
//
//  Purpose:	DoughnutChart class provide only the behaviour 
//              information for the Doughnut chart, all the drawing 
//              routines are located in the PieChart base class.
//
//	Reviewed:	GS - Aug 8, 2002
//				AG - Aug 8, 2002
//              AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Drawing;

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.ChartTypes
#else
	namespace System.Web.UI.DataVisualization.Charting.ChartTypes
#endif
{
	/// <summary>
    /// DoughnutChart class provide only the behaviour information for the 
    /// Doughnut chart, all the drawing routines are located in the PieChart 
    /// base class.
	/// </summary>
	internal class DoughnutChart : PieChart
	{
		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		override public string Name			{ get{ return ChartTypeNames.Doughnut;}}

		/// <summary>
		/// Gets chart type image.
		/// </summary>
		/// <param name="registry">Chart types registry object.</param>
		/// <returns>Chart type image.</returns>
		override public System.Drawing.Image GetImage(ChartTypeRegistry registry)
		{
			return (System.Drawing.Image)registry.ResourceManager.GetObject(this.Name + "ChartType");
		}

		/// <summary>
		/// True if chart type is stacked
		/// </summary>
		override public bool Stacked		{ get{ return false;}}

		/// <summary>
		/// True if chart type supports axeses
		/// </summary>
		override public bool RequireAxes	{ get{ return false;} }

		/// <summary>
		/// True if chart type supports logarithmic axes
		/// </summary>
		override public bool SupportLogarithmicAxes	{ get{ return false;} }

		/// <summary>
		/// True if chart type requires to switch the value (Y) axes position
		/// </summary>
		override public bool SwitchValueAxes	{ get{ return false;} }

		/// <summary>
		/// True if chart series can be placed side-by-side.
		/// </summary>
		override public bool SideBySideSeries { get{ return false;} }

		/// <summary>
		/// If the crossing value is auto Crossing value should be 
		/// automatically set to zero for some chart 
		/// types (Bar, column, area etc.)
		/// </summary>
		override public bool ZeroCrossing { get{ return false;} }

		/// <summary>
		/// True if each data point of a chart must be represented in the legend
		/// </summary>
		override public bool DataPointsInLegend	{ get{ return true;} }

		/// <summary>
		/// Indicates that extra Y values are connected to the scale of the Y axis
		/// </summary>
		override public bool ExtraYValuesConnectedToYAxis{ get { return false; } }

		/// <summary>
		/// True if palette colors should be applied for each data paint.
		/// Otherwise the color is applied to the series.
		/// </summary>
		override public bool ApplyPaletteColorsToPoints	{ get { return true; } }

		/// <summary>
		/// How to draw series/points in legend:
		/// Filled rectangle, Line or Marker
		/// </summary>
		/// <param name="series">Legend item series.</param>
		/// <returns>Legend item style.</returns>
		override public LegendImageStyle GetLegendImageStyle(Series series)
		{
			return LegendImageStyle.Rectangle;
		}

		/// <summary>
		/// Number of supported Y value(s) per point 
		/// </summary>
		override public int YValuesPerPoint{ get { return 1; } }

		/// <summary>
		/// Chart is Doughnut or Pie type
		/// </summary>
		override public bool Doughnut{ get { return true; } }

		#endregion

		#region Methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public DoughnutChart()
		{
		}

		#endregion
	}
}
