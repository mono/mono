//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		PolarChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	PolarChart
//
//  Purpose:	Polar chart type is similar to the Radar chart.
//              All the drawing functionality is located in the 
//              RadarChart class and PolarChart class provides 
//              positionning and style methods required only in 
//              Polar chart.
//
//  Polar Chart Overview:
//  ---------------------
//
//  The polar chart type is a circular graph on which data points 
//  are displayed using the angle, and the distance from the center 
//  point. The X axis is located on the boundaries of the circle and 
//  the Y axis connects the center of the circle with the X axis.
//  
//  By default, the angle scale ranges from 0 to 360 degrees. However, 
//  the X Axis Minimum and Maximum properties may be used to specify 
//  a different angular scale. The Minimum angle value starts at the 
//  top (12 O'Clock position) of the chart but can be changed to 
//  another angle using the Crossing property. For example, setting 
//  the Crossing property to 90 will move the "zero" value to the 
//  3 O'Clock position.
//  
//	Reviewed:	AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Resources;
using System.Reflection;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;

#if Microsoft_CONTROL
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.Data;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
using System.Windows.Forms.DataVisualization.Charting.Utilities;
using System.Windows.Forms.DataVisualization.Charting.Borders3D;

#else
using System.Web.UI.DataVisualization.Charting;

	using System.Web.UI.DataVisualization.Charting.ChartTypes;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.Utilities;
#endif

#endregion

#if Microsoft_CONTROL
    namespace System.Windows.Forms.DataVisualization.Charting.ChartTypes
#else
	namespace System.Web.UI.DataVisualization.Charting.ChartTypes
#endif
{
	/// <summary>
    /// PolarChart class uses its base class RadarChart to perform most of the 
    /// drawing and calculation operations.
	/// </summary>
	internal class PolarChart : RadarChart
	{
		#region Constructors

		/// <summary>
		/// Class public constructor.
		/// </summary>
		public PolarChart()
		{
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		override public string Name			{ get{ return ChartTypeNames.Polar;}}

		#endregion

		#region ICircularChartType interface implementation

		/// <summary>
		/// Checks if closed figure should be drawn even in Line drawing mode.
		/// </summary>
		/// <returns>True if closed figure should be drawn even in Line drawing mode.</returns>
		public override bool RequireClosedFigure()
		{
			return false;
		}

		/// <summary>
		/// Checks if Y axis position may be changed using X axis Crossing property.
		/// </summary>
		/// <returns>True if Y axis position may be changed using X axis Crossing property.</returns>
		public override bool XAxisCrossingSupported()
		{
			return true;
		}

		/// <summary>
		/// Checks if automatic X axis labels are supported.
		/// </summary>
		/// <returns>True if automatic X axis labels are supported.</returns>
		public override bool XAxisLabelsSupported()
		{
			return true;
		}

		/// <summary>
		/// Checks if radial grid lines (X axis) are supported by the chart type.
		/// </summary>
		/// <returns>True if radial grid lines are supported.</returns>
		public override bool RadialGridLinesSupported()
		{
			return true;
		}

		/// <summary>
		/// Gets number of sectors in the circular chart area.
		/// </summary>
		/// <param name="area">Chart area to get number of sectors for.</param>
		/// <param name="seriesCollection">Collection of series.</param>
		/// <returns>Returns number of sectors in circular chart.</returns>
		public override int GetNumerOfSectors(ChartArea area, SeriesCollection seriesCollection)
		{
			// By default we split polar chart into 12 sectors (30 degrees in case of 360 degrees scale)
			int	sectorNumber = 12;

			// Custom interval is set on the X axis
			double interval = area.AxisX.Interval;
			if(area.AxisX.LabelStyle.GetInterval() > 0)
			{
				interval = area.AxisX.LabelStyle.GetInterval();
			}
			if(interval != 0)
			{
				// Get X axis scale size
				double max = (area.AxisX.AutoMaximum) ? 360.0 : area.AxisX.Maximum;
				double min = (area.AxisX.AutoMinimum) ? 0.0 : area.AxisX.Minimum;

				// Calculate number of sectors
				sectorNumber = (int)(Math.Abs(max - min) / interval);
			}

			return sectorNumber;
		}

		/// <summary>
		/// Get a location of Y axis in degrees.
		/// </summary>
		/// <param name="area">Chart area to get Y axes locations for.</param>
		/// <returns>Returns an array of one or more locations of Y axis.</returns>
		public override float[] GetYAxisLocations(ChartArea area)
		{
			float[]	axesLocation = new float[1];
			axesLocation[0] = 0f;
			
			// Check if X axis crossing is set to change location of Y axis
			if( !double.IsNaN(area.AxisX.Crossing) )
			{
				axesLocation[0] = (float)area.AxisX.Crossing;
				while(axesLocation[0] < 0)
				{
					axesLocation[0] = 360f + axesLocation[0];
				}

			}

			return axesLocation;
		}

		#endregion // ICircularChartType interface implementation

		#region Helper Methods

		/// <summary>
		/// Gets polar chart drawing style.
		/// </summary>
		/// <param name="ser">Chart series.</param>
		/// <param name="point">Series point.</param>
		/// <returns>Returns polar drawing style.</returns>
		override protected RadarDrawingStyle GetDrawingStyle(Series ser, DataPoint point)
		{
			RadarDrawingStyle drawingStyle = RadarDrawingStyle.Line;
            if (point.IsCustomPropertySet(CustomPropertyName.PolarDrawingStyle) ||
                ser.IsCustomPropertySet(CustomPropertyName.PolarDrawingStyle))
			{
				string	attributeValue =
                    (point.IsCustomPropertySet(CustomPropertyName.PolarDrawingStyle)) ?
                    point[CustomPropertyName.PolarDrawingStyle] :
                    ser[CustomPropertyName.PolarDrawingStyle];
				if(String.Compare(attributeValue, "Line", StringComparison.OrdinalIgnoreCase ) == 0)
				{
					drawingStyle = RadarDrawingStyle.Line;
				}
				else if(String.Compare(attributeValue, "Marker", StringComparison.OrdinalIgnoreCase ) == 0)
				{
					drawingStyle = RadarDrawingStyle.Marker;
				}
				else
				{
					throw(new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attributeValue, "PolarDrawingStyle")));
				}
			}
			return drawingStyle;
		}

		/// <summary>
		/// Fills a PointF array of data points absolute pixel positions.
		/// </summary>
		/// <param name="graph">Graphics object.</param>
		/// <param name="area">Chart area.</param>
		/// <param name="series">Point series.</param>
		/// <returns>Array of data points position.</returns>
		override protected PointF[] GetPointsPosition(ChartGraphics graph, ChartArea area, Series series)
		{
			PointF[]	pointPos = new PointF[series.Points.Count + 1];
			int index = 0;
			foreach( DataPoint point in series.Points )
			{
				// Change Y value if line is out of plot area
				double yValue = GetYValue(Common, area, series, point, index, 0);

				// Recalculates y position
				double yPosition = area.AxisY.GetPosition( yValue );

				// Recalculates x position
				double xPosition = area.circularCenter.X;

				// Add point position into array
				pointPos[index] = graph.GetAbsolutePoint(new PointF((float)xPosition, (float)yPosition));

				// Rotate position
				float	sectorAngle = area.CircularPositionToAngle(point.XValue);
				Matrix matrix = new Matrix();
				matrix.RotateAt(sectorAngle, graph.GetAbsolutePoint(area.circularCenter));
				PointF[]	rotatedPoint = new PointF[] { pointPos[index] };
				matrix.TransformPoints(rotatedPoint);
				pointPos[index] = rotatedPoint[0];
								
				index++;
			}

			// Add last center point
			pointPos[index] = graph.GetAbsolutePoint(area.circularCenter);

			return pointPos;
		}

		#endregion // Helper Methods
	}
}

