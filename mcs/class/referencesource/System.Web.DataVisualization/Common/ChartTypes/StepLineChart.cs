//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		StepLineChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	StepLineChart
//
//  Purpose:	Step Line chart uses two line segments (horizontal 
//              and vertical) to connect data points. Markers and 
//              labels drawing code is inherited from the Line chart.
//
//	Reviewed:	AG - Aug 6, 2002
//              AG - Microsoft 7, 2007
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
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
	using System.Windows.Forms.DataVisualization.Charting;
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
    /// StepLine class extends its base class LineChart by changing how two 
    /// neighbouring data points are connected with a line. Step Line chart 
    /// uses two line segments (horizontal and vertical) to connect data 
    /// points. Markers and labels drawing code is inherited from the Line chart.
	/// </summary>
	internal class StepLineChart : LineChart
	{
		#region Constructor

		/// <summary>
		/// StepLineChart class constructor.
		/// </summary>
		public StepLineChart()
		{
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return ChartTypeNames.StepLine;}}

		/// <summary>
		/// Gets chart type image.
		/// </summary>
		/// <param name="registry">Chart types registry object.</param>
		/// <returns>Chart type image.</returns>
		override public System.Drawing.Image GetImage(ChartTypeRegistry registry)
		{
			return (System.Drawing.Image)registry.ResourceManager.GetObject(this.Name + "ChartType");
		}

		#endregion

		#region Line drawing and selecting methods

		/// <summary>
		/// Draw chart line using horisontal and vertical lines.
		/// </summary>
		/// <param name="graph">Graphics object.</param>
		/// <param name="common">The Common elements object</param>
		/// <param name="point">Point to draw the line for.</param>
		/// <param name="series">Point series.</param>
		/// <param name="points">Array of points coordinates.</param>
		/// <param name="pointIndex">Index of point to draw.</param>
		/// <param name="tension">Line tension</param>
		override protected void DrawLine(
			ChartGraphics graph,  
			CommonElements common, 
			DataPoint point, 
			Series series, 
			PointF[] points, 
			int pointIndex, 
			float tension)
		{
			// Start drawing from the second point
			if(pointIndex <= 0)
			{
				return;
			}

			// Darw two lines
			PointF	point1 = points[pointIndex - 1];
			PointF	point2 = new PointF(points[pointIndex].X, points[pointIndex - 1].Y);
			PointF	point3 = points[pointIndex];
			graph.DrawLineRel( point.Color, point.BorderWidth, point.BorderDashStyle, graph.GetRelativePoint(point1), graph.GetRelativePoint(point2), series.ShadowColor, series.ShadowOffset );
			graph.DrawLineRel( point.Color, point.BorderWidth, point.BorderDashStyle, graph.GetRelativePoint(point2), graph.GetRelativePoint(point3), series.ShadowColor, series.ShadowOffset );

			if( common.ProcessModeRegions )
			{
				// Create grapics path object for the line
				// Split line into 2 segments.
				GraphicsPath	path = new GraphicsPath();
				try
				{
					path.AddLine(point2, point3);
                    if (!point2.Equals(point3))
                    {
                        path.Widen(new Pen(point.Color, point.BorderWidth + 2));
                    }
				}
                catch (OutOfMemoryException)
                {
                    // GraphicsPath.Widen incorrectly throws OutOfMemoryException
                    // catching here and reacting by not widening
                }
                catch (ArgumentException)
                {
                }

				// Allocate array of floats
				PointF	pointNew = PointF.Empty;
				float[]	coord = new float[path.PointCount * 2];
				PointF[] pathPoints = path.PathPoints;
				for( int i = 0; i < path.PointCount; i++ )
				{
					pointNew = graph.GetRelativePoint( pathPoints[i] );
					coord[2*i] = pointNew.X;
					coord[2*i + 1] = pointNew.Y;
				}

				common.HotRegionsList.AddHotRegion( 
					path, 
					false, 
					coord, 
					point, 
					series.Name, 
					pointIndex );
                path.Dispose();
				// Create grapics path object for the line
				path = new GraphicsPath();
				try
				{
					path.AddLine(point1, point2);
					path.Widen(new Pen(point.Color, point.BorderWidth + 2));
				}
                catch (OutOfMemoryException)
                {
                    // GraphicsPath.Widen incorrectly throws OutOfMemoryException
                    // catching here and reacting by not widening
                }
                catch (ArgumentException)
                {
                }

				// Allocate array of floats
				coord = new float[path.PointCount * 2];
				pathPoints = path.PathPoints;
				for( int i = 0; i < path.PointCount; i++ )
				{
					pointNew = graph.GetRelativePoint( pathPoints[i] );
					coord[2*i] = pointNew.X;
					coord[2*i + 1] = pointNew.Y;
				}

				common.HotRegionsList.AddHotRegion( 
					path, 
					false, 
					coord, 
					series.Points[pointIndex - 1],
					series.Name, 
					pointIndex - 1);
                path.Dispose();
			}
		}
		
		#endregion

		#region 3D Line drawing and selection

		/// <summary>
		/// Draws a 3D surface connecting the two specified points in 2D space.
		/// Used to draw Line based charts.
		/// </summary>
		/// <param name="area">Chart area reference.</param>
		/// <param name="graph">Chart graphics.</param>
		/// <param name="matrix">Coordinates transformation matrix.</param>
		/// <param name="lightStyle">LightStyle style (None, Simplistic, Realistic).</param>
		/// <param name="prevDataPointEx">Previous data point object.</param>
		/// <param name="positionZ">Z position of the back side of the 3D surface.</param>
		/// <param name="depth">Depth of the 3D surface.</param>
		/// <param name="points">Array of points.</param>
		/// <param name="pointIndex">Index of point to draw.</param>
		/// <param name="pointLoopIndex">Index of points loop.</param>
		/// <param name="tension">Line tension.</param>
		/// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
		/// <param name="topDarkening">Darkenning scale for top surface. 0 - None.</param>
		/// <param name="bottomDarkening">Darkenning scale for bottom surface. 0 - None.</param>
		/// <param name="thirdPointPosition">Position where the third point is actually located or float.NaN if same as in "firstPoint".</param>
		/// <param name="fourthPointPosition">Position where the fourth point is actually located or float.NaN if same as in "secondPoint".</param>
		/// <param name="clippedSegment">Indicates that drawn segment is 3D clipped. Only top/bottom should be drawn.</param>
		/// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
		protected override GraphicsPath Draw3DSurface( 
			ChartArea area,
			ChartGraphics graph, 
			Matrix3D matrix,
			LightStyle lightStyle,
			DataPoint3D prevDataPointEx,
			float positionZ, 
			float depth, 
			ArrayList points,
			int pointIndex, 
			int pointLoopIndex,
			float tension,
			DrawingOperationTypes operationType,
			float topDarkening,
			float bottomDarkening,
			PointF thirdPointPosition,
			PointF fourthPointPosition,
			bool clippedSegment)
		{
			// Create graphics path for selection
			GraphicsPath	resultPath = ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
				? new GraphicsPath() : null;

			// Check if points are drawn from sides to center (do only once)
			if(centerPointIndex == int.MaxValue)
			{
				centerPointIndex = GetCenterPointIndex(points);
			}

			//************************************************************
			//** Find line first & second points
			//************************************************************
			DataPoint3D	secondPoint = (DataPoint3D)points[pointIndex];
			int pointArrayIndex = pointIndex;
			DataPoint3D firstPoint = ChartGraphics.FindPointByIndex(
				points, 
				secondPoint.index - 1, 
				(this.multiSeries) ? secondPoint : null, 
				ref pointArrayIndex);

			// Fint point with line properties
			DataPoint3D		pointAttr = secondPoint;
			if(prevDataPointEx.dataPoint.IsEmpty)
			{
				pointAttr = prevDataPointEx;
			}
			else if(firstPoint.index > secondPoint.index)
			{
				pointAttr = firstPoint;
			}

			// Adjust point visual properties 
			Color			color = (useBorderColor) ? pointAttr.dataPoint.BorderColor : pointAttr.dataPoint.Color;
			ChartDashStyle	dashStyle = pointAttr.dataPoint.BorderDashStyle;
			if( pointAttr.dataPoint.IsEmpty && pointAttr.dataPoint.Color == Color.Empty)
			{
				color = Color.Gray;
			}
			if( pointAttr.dataPoint.IsEmpty && pointAttr.dataPoint.BorderDashStyle == ChartDashStyle.NotSet )
			{
				dashStyle = ChartDashStyle.Solid;
			}

			//************************************************************
			//** Create "middle" point
			//************************************************************
			DataPoint3D	middlePoint = new DataPoint3D();
			middlePoint.xPosition = secondPoint.xPosition;
			middlePoint.yPosition = firstPoint.yPosition;

			// Check if reversed drawing order required
			bool originalDrawOrder = true;
			if((pointIndex + 1) < points.Count)
			{
				DataPoint3D p = (DataPoint3D)points[pointIndex + 1];
				if(p.index == firstPoint.index)
				{
					originalDrawOrder = false;
				}
			}

			// Check in which order vertical & horizontal lines segments should be drawn
			if(centerPointIndex != int.MaxValue)
			{
				if(pointIndex >= centerPointIndex)
				{
					originalDrawOrder = false;
				}
			}

			// Draw two segments of the step line
			GraphicsPath resultPathLine1, resultPathLine2;
			if(originalDrawOrder)
			{
				// Draw first line
				middlePoint.dataPoint = secondPoint.dataPoint;
				resultPathLine1 = graph.Draw3DSurface( 
					area, matrix, lightStyle, SurfaceNames.Top, positionZ, depth, color, 
					pointAttr.dataPoint.BorderColor, pointAttr.dataPoint.BorderWidth, dashStyle, 
					firstPoint, middlePoint, 
					points, pointIndex, 0f, operationType, LineSegmentType.First, 
					(this.showPointLines) ? true : false, false,
                    area.ReverseSeriesOrder,
					this.multiSeries, 0, true);

				// No second draw of the prev. front line required
				graph.frontLinePen = null;

				// Draw second line
				middlePoint.dataPoint = firstPoint.dataPoint;
				resultPathLine2 = graph.Draw3DSurface( 
					area, matrix, lightStyle, SurfaceNames.Top, positionZ, depth, color, 
					pointAttr.dataPoint.BorderColor, pointAttr.dataPoint.BorderWidth, dashStyle, 
					middlePoint, secondPoint, 
					points, pointIndex, 0f, operationType, LineSegmentType.Last, 
					(this.showPointLines) ? true : false, false,
                    area.ReverseSeriesOrder,
					this.multiSeries, 0, true);

				// No second draw of the prev. front line required
				graph.frontLinePen = null;
			}
			else
			{
				// Draw second line
				middlePoint.dataPoint = firstPoint.dataPoint;
				resultPathLine2 = graph.Draw3DSurface( 
					area, matrix, lightStyle, SurfaceNames.Top, positionZ, depth, color, 
					pointAttr.dataPoint.BorderColor, pointAttr.dataPoint.BorderWidth, dashStyle, 
					middlePoint, secondPoint, 
					points, pointIndex, 0f, operationType, LineSegmentType.Last, 
					(this.showPointLines) ? true : false, false,
                    area.ReverseSeriesOrder,
					this.multiSeries, 0, true);
				
				// No second draw of the prev. front line required
				graph.frontLinePen = null;
				
				// Draw first line
				middlePoint.dataPoint = secondPoint.dataPoint;
				resultPathLine1 = graph.Draw3DSurface( 
					area, matrix, lightStyle, SurfaceNames.Top, positionZ, depth, color, 
					pointAttr.dataPoint.BorderColor, pointAttr.dataPoint.BorderWidth, dashStyle, 
					firstPoint, middlePoint, 
					points, pointIndex, 0f, operationType, LineSegmentType.First, 
					(this.showPointLines) ? true : false, false,
                    area.ReverseSeriesOrder,
					this.multiSeries, 0, true);

				// No second draw of the prev. front line required
				graph.frontLinePen = null;
			}

			if(resultPath != null)
			{
				if( area.Common.ProcessModeRegions)
				{
					if(resultPathLine1 != null && resultPathLine1.PointCount > 0)
					{
						area.Common.HotRegionsList.AddHotRegion( 
							resultPathLine1, 
							false, 
							graph, 
							prevDataPointEx.dataPoint, 
							prevDataPointEx.dataPoint.series.Name, 
							prevDataPointEx.index - 1 );
					}
				}

				if(resultPathLine2 != null && resultPathLine2.PointCount > 0)
				{
					resultPath.AddPath(resultPathLine2, true);
				}
			}
			return resultPath;
		}

		#endregion
	}
}
