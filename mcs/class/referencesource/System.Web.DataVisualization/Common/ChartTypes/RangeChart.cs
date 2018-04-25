//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		RangeChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	RangeChart, SplineRangeChart
//
//  Purpose:	Provides 2D/3D drawing and hit testing functionality 
//              for the Range and SplineRange charts. The Range chart 
//              displays a range of data by plotting two Y values per 
//              data point, with each Y value being drawn as a line 
//              chart. The range between the Y values can then be 
//              filled with color. Spline Range chart changes the 
//              default tension of the lines between data points.
//              
//	Reviewed:	AG - Aug 6, 2002
//              AG - Microsoft 6, 2007
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
    /// SplineRangeChart class extends the RangeChart class by 
    /// providing a different initial tension for the lines.
    /// </summary>
	internal class SplineRangeChart : RangeChart
	{
		#region Constructor

		/// <summary>
		/// Default constructor.
		/// </summary>
		public SplineRangeChart()
		{
			// Set default line tension
			base.lineTension = 0.5f;
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return ChartTypeNames.SplineRange;}}

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

		#region Default tension method

		/// <summary>
		/// Gets default line tension.
		/// </summary>
		/// <returns>Default line tension.</returns>
		override protected float GetDefaultTension()
		{
			return 0.5f;
		}

		/// <summary>
		/// Checks if line tension is supported by the chart type.
		/// </summary>
		/// <returns>True if line tension is supported.</returns>
		protected override bool IsLineTensionSupported()
		{
			return true;
		}

		#endregion
	}

	/// <summary>
    /// RangeChart class provides 2D/3D drawing and hit testing 
    /// functionality for the Range and SplineRange charts. The 
    /// only difference of the SplineRange chart is the default 
    /// tension of the line.
    /// 
    /// SplineChart base class provides most of the functionality 
    /// like drawing lines, labels and markers.
    /// </summary>
	internal class RangeChart : SplineChart
	{
		#region Fields

		/// <summary>
		/// Fields used to fill area with gradient 
		/// </summary>
        internal bool gradientFill = false;

		/// <summary>
		/// Shape of the low values
		/// </summary>
        internal GraphicsPath areaBottomPath = new GraphicsPath();

		/// <summary>
		/// Coordinates of the area path
		/// </summary>
		protected	GraphicsPath	areaPath = null;

		/// <summary>
		/// Reference to the current series object
		/// </summary>
        private Series _series = null;

		/// <summary>
		/// Array of low line values
		/// </summary>
        internal PointF[] lowPoints = null;

		/// <summary>
		/// Check if series are indexed based
		/// </summary>
        internal bool indexedBasedX = false;

		/// <summary>
		/// Secondary Y coordinate that should be used for bottom line of the range (left point)
		/// </summary>
		private		float			_thirdPointY2Value = float.NaN;

		/// <summary>
		/// Secondary Y coordinate that should be used for bottom line of the range (right point)
		/// </summary>
		private		float			_fourthPointY2Value = float.NaN;

		#endregion

		#region Constructor

		/// <summary>
		/// Default constructor.
		/// </summary>
		public RangeChart()
		{
			this.drawOutsideLines = true;
		}

		#endregion 

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return ChartTypeNames.Range;}}

		/// <summary>
		/// Number of supported Y value(s) per point 
		/// </summary>
		override public int YValuesPerPoint	{ get { return 2; } }

		/// <summary>
		/// Indicates that extra Y values are connected to the scale of the Y axis
		/// </summary>
		override public bool ExtraYValuesConnectedToYAxis{ get { return true; } }

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
		/// Gets chart type image.
		/// </summary>
		/// <param name="registry">Chart types registry object.</param>
		/// <returns>Chart type image.</returns>
		override public System.Drawing.Image GetImage(ChartTypeRegistry registry)
		{
			return (System.Drawing.Image)registry.ResourceManager.GetObject(this.Name + "ChartType");
		}

		#endregion

		#region Default tension method

		/// <summary>
		/// Gets default line tension.
		/// </summary>
		/// <returns>Line tension.</returns>
		override protected float GetDefaultTension()
		{
			return 0.0f;
		}

		/// <summary>
		/// Checks if line tension is supported by the chart type.
		/// </summary>
		/// <returns>True if line tension is supported.</returns>
		protected override bool IsLineTensionSupported()
		{
			return false;
		}

		#endregion

		#region Painting and Selection related methods

		/// <summary>
		/// Fills last series area with gradient.
		/// If gradient is used as a back color of the series it must be drawn 
		/// at the same time.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		private void FillLastSeriesGradient(ChartGraphics graph)
		{
			// Add last line in the path
			if(areaPath != null)
			{
				areaPath.AddLine(
					areaPath.GetLastPoint().X, 
					areaPath.GetLastPoint().Y, 
					areaPath.GetLastPoint().X, 
					areaBottomPath.GetLastPoint().Y);
			}
			
			// Fill whole area with gradient
			if(gradientFill && areaPath != null)
			{
				// Set clip region
				graph.SetClip( Area.PlotAreaPosition.ToRectangleF() );

				// Create new path from high/low lines 
                using (GraphicsPath gradientPath = new GraphicsPath())
                {
                    gradientPath.AddPath(areaPath, true);
                    areaBottomPath.Reverse();
                    gradientPath.AddPath(areaBottomPath, true);

                    // Create brush
                    using (Brush areaGradientBrush = graph.GetGradientBrush(gradientPath.GetBounds(), this._series.Color, this._series.BackSecondaryColor, this._series.BackGradientStyle))
                    {
                        // Fill area with gradient
                        graph.FillPath(areaGradientBrush, gradientPath);
                        gradientFill = false;
                    }
                }

				// Reset clip region
				graph.ResetClip();
			}
			if(areaPath != null)
			{
				areaPath.Dispose();
				areaPath = null;
			}

			// Reset bottom area path
			areaBottomPath.Reset();
		}

		/// <summary>
		/// This method recalculates position of the end points of lines. This method 
		/// is used from Paint or Select method.
		/// </summary>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		protected override void ProcessChartType( 
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			Series seriesToDraw )
		{
			gradientFill = false;
			lowPoints = null;

			// Check if series is indexed based
            indexedBasedX = ChartHelper.IndexedSeries(common, area.GetSeriesFromChartType(this.Name).ToArray());

			// Call base class
			base.ProcessChartType(selection, graph, common, area, seriesToDraw);

			// Fill gradient fro the previous series
			FillLastSeriesGradient(graph);
		}

		/// <summary>
		/// Override line drawing method to fill the range and draw lines.
		/// </summary>
		/// <param name="graph">Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="point">Point to draw the line for.</param>
		/// <param name="series">Point series.</param>
		/// <param name="points">Array of oints coordinates.</param>
		/// <param name="pointIndex">Index of point to draw.</param>
		/// <param name="tension">Line tension.</param>
		override protected void DrawLine(
			ChartGraphics graph,  
			CommonElements common, 
			DataPoint point, 
			Series series, 
			PointF[] points, 
			int pointIndex, 
			float tension)
		{
			// Two Y values required
			if(point.YValues.Length < 2)
			{
				throw(new InvalidOperationException( SR.ExceptionChartTypeRequiresYValues( this.Name, "2" )));
			}

			// Start drawing from the second point
			if(pointIndex <= 0)
			{
				return;
			}

			// Do nothing for the low values line
			if(this.YValueIndex == 1)
			{
				return;
			}

			// Check if its a beginning of a new series
			if(this._series != null)
			{
				if(this._series.Name != series.Name)
				{
					// Fill gradient from the previous series
					FillLastSeriesGradient(graph);
					this._series = series;
					lowPoints = null;
					areaBottomPath.Reset();
				}
			}
			else
			{
				this._series = series;
			}

			// Fill array of lower points of the range
			if(lowPoints == null)
			{
				this.YValueIndex = 1;
				lowPoints = GetPointsPosition(graph, series, indexedBasedX);
				this.YValueIndex = 0;
			}

			// Calculate points position
			PointF	highPoint1 = points[pointIndex-1];
			PointF	highPoint2 = points[pointIndex];
			PointF	lowPoint1 = lowPoints[pointIndex-1];
			PointF	lowPoint2 = lowPoints[pointIndex];
			
			// Create area brush
			Brush	areaBrush = null;
			if( point.BackHatchStyle != ChartHatchStyle.None )
			{
				areaBrush = graph.GetHatchBrush( point.BackHatchStyle, point.Color, point.BackSecondaryColor);
			}
			else if( point.BackGradientStyle != GradientStyle.None )
			{
				this.gradientFill = true;
				this._series = point.series;
			}
			else if( point.BackImage.Length > 0 && point.BackImageWrapMode != ChartImageWrapMode.Unscaled && point.BackImageWrapMode != ChartImageWrapMode.Scaled)
			{
				areaBrush = graph.GetTextureBrush(point.BackImage, point.BackImageTransparentColor, point.BackImageWrapMode, point.Color );
			}
			else
			{
				areaBrush = new SolidBrush(point.Color);
			}

			// Calculate data point area segment path
			GraphicsPath path = new GraphicsPath();
			path.AddLine(highPoint1.X, lowPoint1.Y, highPoint1.X, highPoint1.Y);
			if(this.lineTension == 0)
			{
				path.AddLine(points[pointIndex-1], points[pointIndex]);
			}
			else
			{
				path.AddCurve(points, pointIndex-1, 1, this.lineTension);
			}

			path.AddLine(highPoint2.X, highPoint2.Y, highPoint2.X, lowPoint2.Y);

			// Because of SVG Rendering order of points in the close shape 
			// has to be respected.
			if( graph.ActiveRenderingType == RenderingType.Svg )
			{
                using (GraphicsPath pathReverse = new GraphicsPath())
                {
                    // Add curve to the new graphics path
                    if (this.lineTension == 0)
                    {
                        path.AddLine(lowPoints[pointIndex - 1], lowPoints[pointIndex]);
                    }
                    else
                    {
                        pathReverse.AddCurve(lowPoints, pointIndex - 1, 1, this.lineTension);

                        // Convert to polygon
                        pathReverse.Flatten();

                        // Reversed points order in the aray
                        PointF[] pointsReversed = pathReverse.PathPoints;
                        PointF[] pointF = new PointF[pointsReversed.Length];
                        int pntIndex = pointsReversed.Length - 1;
                        foreach (PointF pp in pointsReversed)
                        {
                            pointF[pntIndex] = pp;
                            pntIndex--;
                        }

                        // Path can not have polygon width two points
                        if (pointF.Length == 2)
                        {
                            PointF[] newPointF = new PointF[3];
                            newPointF[0] = pointF[0];
                            newPointF[1] = pointF[1];
                            newPointF[2] = pointF[1];
                            pointF = newPointF;
                        }

                        // Add Polygon to the path
                        path.AddPolygon(pointF);
                    }
                }
			}
			else
			{
				if(this.lineTension == 0)
				{
					path.AddLine(lowPoints[pointIndex-1], lowPoints[pointIndex]);
				}
				else
				{
					path.AddCurve(lowPoints, pointIndex-1, 1, this.lineTension);
				}

			}

			// Check if bottom line is partialy in the data scaleView
			if(!clipRegionSet)
			{
				double	xValue = (indexedSeries) ? pointIndex + 1 : series.Points[pointIndex].XValue;
				double	xPrevValue = (indexedSeries) ? pointIndex : series.Points[pointIndex - 1].XValue;
				if(xPrevValue < hAxisMin || xPrevValue > hAxisMax || 
					xValue > hAxisMax || xValue < hAxisMin ||
					series.Points[pointIndex-1].YValues[1] < vAxisMin || series.Points[pointIndex-1].YValues[1] > vAxisMax ||
					series.Points[pointIndex].YValues[1] < vAxisMin || series.Points[pointIndex].YValues[1] > vAxisMax )
				{
					// Set clipping region for bottom line drawing 
					graph.SetClip( Area.PlotAreaPosition.ToRectangleF() );
					clipRegionSet = true;
				}
			}

			// Draw shadow
			if(series.ShadowColor != Color.Empty && series.ShadowOffset != 0)
			{
				if(point.Color != Color.Empty && point.Color != Color.Transparent)
				{
					// Translate drawing matrix
					Matrix translateMatrix = graph.Transform.Clone();
					translateMatrix.Translate(series.ShadowOffset, series.ShadowOffset);
					Matrix oldMatrix = graph.Transform;
					graph.Transform = translateMatrix;

					Region	shadowRegion = new Region(path);
                    using (Brush shadowBrush = new SolidBrush((series.ShadowColor.A != 255) ? series.ShadowColor : Color.FromArgb(point.Color.A / 2, series.ShadowColor)))
                    {
                        Region clipRegion = null;
                        if (!graph.IsClipEmpty && !graph.Clip.IsInfinite(graph.Graphics))
                        {
                            clipRegion = graph.Clip;
                            clipRegion.Translate(series.ShadowOffset + 1, series.ShadowOffset + 1);
                            graph.Clip = clipRegion;

                        }

                        // Fill region
                        graph.FillRegion(shadowBrush, shadowRegion);

                        // Draw leftmost and rightmost vertical lines
                        using (Pen areaLinePen = new Pen(shadowBrush, 1))
                        {
                            if (pointIndex == 0)
                            {
                                graph.DrawLine(areaLinePen, highPoint1.X, lowPoint1.Y, highPoint1.X, highPoint1.Y);
                            }
                            if (pointIndex == series.Points.Count - 1)
                            {
                                graph.DrawLine(areaLinePen, highPoint2.X, highPoint2.Y, highPoint2.X, lowPoint2.Y);
                            }
                        }

                        // Restore graphics parameters
                        graph.Transform = oldMatrix;

                        // Draw high and low line shadows
                        this.drawShadowOnly = true;
                        base.DrawLine(graph, common, point, series, points, pointIndex, tension);
                        this.YValueIndex = 1;
                        base.DrawLine(graph, common, point, series, lowPoints, pointIndex, tension);
                        this.YValueIndex = 0;
                        this.drawShadowOnly = false;

                        // Restore clip region
                        if (clipRegion != null)
                        {
                            clipRegion = graph.Clip;
                            clipRegion.Translate(-(series.ShadowOffset + 1), -(series.ShadowOffset + 1));
                            graph.Clip = clipRegion;
                        }
                    }
                }
            }

			// Draw area
			if(!gradientFill)
			{
				// Turn off anti aliasing and fill area
				SmoothingMode oldMode = graph.SmoothingMode;
				graph.SmoothingMode = SmoothingMode.None;
				path.CloseAllFigures();
				graph.FillPath(areaBrush, path);
				graph.SmoothingMode = oldMode;

				// Draw top and bottom lines, because anti aliasing is not working for the FillPath method
				if(graph.SmoothingMode != SmoothingMode.None)
				{
					Pen areaLinePen = new Pen(areaBrush, 1);

					// This code is introduce because of problem 
					// with Svg and Hatch Color
                    HatchBrush hatchBrush = areaBrush as HatchBrush;
					if( hatchBrush != null )
					{
						areaLinePen.Color = hatchBrush.ForegroundColor;
					}

					if(pointIndex == 0)
					{
						graph.DrawLine(areaLinePen, highPoint1.X, lowPoint1.Y, highPoint1.X, highPoint1.Y);
					}
					if(pointIndex == series.Points.Count - 1)
					{
						graph.DrawLine(areaLinePen, highPoint2.X, highPoint2.Y, highPoint2.X, lowPoint2.Y);
					}
					
					if(this.lineTension == 0)
					{
						graph.DrawLine(areaLinePen, points[pointIndex - 1], points[pointIndex]);
					}
					else
					{
						graph.DrawCurve(areaLinePen, points, pointIndex-1, 1, this.lineTension);
					}

					if(this.lineTension == 0)
					{
						graph.DrawLine(areaLinePen, lowPoints[pointIndex - 1], lowPoints[pointIndex]);
					}
					else
					{
						graph.DrawCurve(areaLinePen, lowPoints, pointIndex-1, 1, this.lineTension);
					}
				}
			}
			
			// Add first line
			if(areaPath == null)
			{
				areaPath = new GraphicsPath();
				areaPath.AddLine(highPoint1.X, lowPoint1.Y, highPoint1.X, highPoint1.Y);
			}

			// Add line to the gradient path
			if(this.lineTension == 0)
			{
				areaPath.AddLine(points[pointIndex-1], points[pointIndex]);
			}
			else
			{
				areaPath.AddCurve(points, pointIndex-1, 1, this.lineTension);
			}

			if(this.lineTension == 0)
			{
				areaBottomPath.AddLine(lowPoints[pointIndex-1], lowPoints[pointIndex]);
			}
			else
			{
				areaBottomPath.AddCurve(lowPoints, pointIndex-1, 1, this.lineTension);
			}

			// Draw range High and Low border lines
			if((point.BorderWidth > 0 && 
				point.BorderDashStyle != ChartDashStyle.NotSet && 
				point.BorderColor != Color.Empty) || 
				areaBrush is SolidBrush)
			{
				this.useBorderColor = true;
				this.disableShadow = true;
				base.DrawLine(graph, common, point, series, points, pointIndex, tension);
				this.YValueIndex = 1;
				base.DrawLine(graph, common, point, series, lowPoints, pointIndex, tension);
				this.YValueIndex = 0;
				this.useBorderColor = false;
				this.disableShadow = false;
			}

			if( common.ProcessModeRegions )
			{
				//**************************************************************
				//** Add area for the inside of the area
				//**************************************************************

				path.AddLine(highPoint1.X, lowPoint1.Y, highPoint1.X, highPoint1.Y);
				if(this.lineTension == 0)
				{
					path.AddLine(points[pointIndex-1], points[pointIndex]);
				}
				else
				{
					path.AddCurve(points, pointIndex-1, 1, this.lineTension);
				}
				path.AddLine(highPoint2.X, highPoint2.Y, highPoint2.X, lowPoint2.Y);
				if(this.lineTension == 0)
				{
					path.AddLine(lowPoints[pointIndex-1], lowPoints[pointIndex]);
				}
				else
				{
					path.AddCurve(lowPoints, pointIndex-1, 1, this.lineTension);
				}

				// Create grapics path object dor the curved area
				GraphicsPath mapAreaPath = new GraphicsPath();
				mapAreaPath.AddLine(highPoint1.X, lowPoint1.Y, highPoint1.X, highPoint1.Y);
				if(this.lineTension == 0)
				{
					mapAreaPath.AddLine(points[pointIndex - 1], points[pointIndex]);
				}
				else
				{
					mapAreaPath.AddCurve(points, pointIndex-1, 1, this.lineTension);
					mapAreaPath.Flatten();
				}
				mapAreaPath.AddLine(highPoint2.X, highPoint2.Y, highPoint2.X, lowPoint2.Y);
				if(this.lineTension == 0)
				{
					mapAreaPath.AddLine(lowPoints[pointIndex - 1], lowPoints[pointIndex]);
				}
				else
				{
					mapAreaPath.AddCurve(lowPoints, pointIndex-1, 1, this.lineTension);
					mapAreaPath.Flatten();
				}

				// Allocate array of floats
				PointF	pointNew = PointF.Empty;
				float[]	coord = new float[mapAreaPath.PointCount * 2];
				PointF[] pathPoints = mapAreaPath.PathPoints;
				for( int i = 0; i < mapAreaPath.PointCount; i++ )
				{
					pointNew = graph.GetRelativePoint( pathPoints[i] );
					coord[2*i] = pointNew.X;
					coord[2*i + 1] = pointNew.Y;
				}

				common.HotRegionsList.AddHotRegion(
					mapAreaPath, 
					false, 
					coord, 
					point, 
					series.Name,
					pointIndex );
			
			}
            //Clean up
            if (areaBrush != null)
                areaBrush.Dispose();
            if (path != null)
            {
                path.Dispose();
                path = null;
            }

		}

		
		#endregion

		#region 3D painting and selection methods

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


			//****************************************************************
			//** Find line first and second points.
			//****************************************************************

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

			//****************************************************************
			//** Switch first and second points.
			//****************************************************************
			bool reversed = false;
			if(firstPoint.index > secondPoint.index)
			{
				DataPoint3D	tempPoint = firstPoint;
				firstPoint = secondPoint;
				secondPoint = tempPoint;
				reversed = true;
			}


			// Points can be drawn from sides to the center.
			// In this case can't use index in the list to find first point.
			// Use point series and real point index to find the first point.
			// Get required point index
			if(matrix.Perspective != 0 && centerPointIndex != int.MaxValue)
			{
				pointArrayIndex = pointIndex;
				if( pointIndex != (centerPointIndex + 1))
				{
					firstPoint = ChartGraphics.FindPointByIndex(points, secondPoint.index - 1, (this.multiSeries) ? secondPoint : null, ref pointArrayIndex);
				}
				else
				{
                    if (!area.ReverseSeriesOrder)
					{
						secondPoint = ChartGraphics.FindPointByIndex(points, firstPoint.index + 1, (this.multiSeries) ? secondPoint : null, ref pointArrayIndex);
					}
					else
					{
						firstPoint = secondPoint;
						secondPoint = ChartGraphics.FindPointByIndex(points, secondPoint.index - 1, (this.multiSeries) ? secondPoint : null, ref pointArrayIndex);
					}
				}
			}

			// Check if points are not null
			if(firstPoint == null || secondPoint == null)
			{
				return resultPath;
			}


            // Area point is drawn as one segment
			return Draw3DSurface( firstPoint, secondPoint, reversed,
				area, graph, matrix, lightStyle, prevDataPointEx,
				positionZ, depth, points, pointIndex, pointLoopIndex,
				tension, operationType, LineSegmentType.Single,
				topDarkening, bottomDarkening,
				thirdPointPosition, fourthPointPosition,
				clippedSegment,
				true, true);
		}

		/// <summary>
		/// Draws a 3D surface connecting the two specified points in 2D space.
		/// Used to draw Line based charts.
		/// </summary>
		/// <param name="firstPoint">First data point.</param>
		/// <param name="secondPoint">Second data point.</param>
		/// <param name="reversed">Points are in reversed order.</param>
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
		/// <param name="surfaceSegmentType">Define surface segment type if it consists of several segments.</param>
		/// <param name="topDarkening">Darkenning scale for top surface. 0 - None.</param>
		/// <param name="bottomDarkening">Darkenning scale for bottom surface. 0 - None.</param>
		/// <param name="thirdPointPosition">Position where the third point is actually located or float.NaN if same as in "firstPoint".</param>
		/// <param name="fourthPointPosition">Position where the fourth point is actually located or float.NaN if same as in "secondPoint".</param>
		/// <param name="clippedSegment">Indicates that drawn segment is 3D clipped. Only top/bottom should be drawn.</param>
		/// <param name="clipOnTop">Indicates that top segment line should be clipped to the pkot area.</param>
		/// <param name="clipOnBottom">Indicates that bottom segment line should be clipped to the pkot area.</param>
		/// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
		protected override GraphicsPath Draw3DSurface( 
			DataPoint3D firstPoint, 
			DataPoint3D secondPoint, 
			bool reversed,
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
			LineSegmentType surfaceSegmentType,
			float topDarkening,
			float bottomDarkening,
			PointF thirdPointPosition,
			PointF fourthPointPosition,
			bool clippedSegment,
			bool clipOnTop,
			bool clipOnBottom)
		{
			// Create graphics path for selection
			GraphicsPath	resultPath = ((operationType & DrawingOperationTypes.CalcElementPath) == DrawingOperationTypes.CalcElementPath)
				? new GraphicsPath() : null;

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

			//****************************************************************
			//** Adjust point visual properties.
			//****************************************************************
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

			//****************************************************************
			//** Get axis position
			//****************************************************************
			float	axisPosition = (float)VAxis.GetPosition(this.VAxis.Crossing);


			//****************************************************************
			//** Calculate position of top/bootom points.
			//****************************************************************
			PointF	thirdPoint, fourthPoint; 
			GetBottomPointsPosition(
				Common, 
				area, 
				axisPosition, 
				ref firstPoint, 
				ref secondPoint, 
				out thirdPoint, 
				out fourthPoint);

			// Check if point's position provided as parameter
			if(!float.IsNaN(thirdPointPosition.Y))
			{
				thirdPoint.Y = thirdPointPosition.Y;
			}
			if(!float.IsNaN(fourthPointPosition.Y))
			{
				fourthPoint.Y = fourthPointPosition.Y;
			}


			//****************************************************************
			//** Check if top/bottom lines of range segment intersect.
			//****************************************************************
            double smallDouble = 0.0001;
            if (Math.Abs(firstPoint.yPosition - (double)thirdPoint.Y) > smallDouble && 
                Math.Abs(secondPoint.yPosition - (double)fourthPoint.Y) > smallDouble &&
                ((firstPoint.yPosition > thirdPoint.Y && secondPoint.yPosition < fourthPoint.Y) ||
                (firstPoint.yPosition < thirdPoint.Y && secondPoint.yPosition > fourthPoint.Y)))
			{
				// This feature is not supported in 3D SplineRange chart type
				if(tension != 0f)
				{
                    throw (new InvalidOperationException(SR.Exception3DSplineY1ValueIsLessThenY2));
				}

				// Find intersection point
				PointF	intersectionCoordinates = ChartGraphics.GetLinesIntersection(
					(float)firstPoint.xPosition, (float)firstPoint.yPosition,
					(float)secondPoint.xPosition, (float)secondPoint.yPosition,
					thirdPoint.X, thirdPoint.Y,
					fourthPoint.X, fourthPoint.Y);
				DataPoint3D	intersectionPoint = new DataPoint3D();
				intersectionPoint.xPosition = intersectionCoordinates.X;
				intersectionPoint.yPosition = intersectionCoordinates.Y;

				// Check if intersection point is valid
				bool splitDraw = true;
				if( double.IsNaN(intersectionCoordinates.X) ||
					double.IsNaN(intersectionCoordinates.Y) )
				{
					splitDraw = false;
				}
				else
				{
					if( (decimal)intersectionCoordinates.X == (decimal)firstPoint.xPosition && 
						(decimal)intersectionCoordinates.Y == (decimal)firstPoint.yPosition )
					{
						splitDraw = false;
					}
					if( (decimal)intersectionCoordinates.X == (decimal)secondPoint.xPosition && 
						(decimal)intersectionCoordinates.Y == (decimal)secondPoint.yPosition )
					{
						splitDraw = false;
					}
				}

				if(splitDraw)
				{
					// Check if reversed drawing order required
					reversed = false;
					if((pointIndex + 1) < points.Count)
					{
						DataPoint3D p = (DataPoint3D)points[pointIndex + 1];
						if(p.index == firstPoint.index)
						{
							reversed = true;
						}
					}

					// Draw two segments
					for(int segmentIndex = 0; segmentIndex <= 1; segmentIndex++)
					{
						GraphicsPath segmentPath = null;
						if(segmentIndex == 0 && !reversed ||
							segmentIndex == 1 && reversed)
						{
							// Set coordinates of bottom points
							_fourthPointY2Value = (float)intersectionPoint.yPosition;

							// Draw first segment
							intersectionPoint.dataPoint = secondPoint.dataPoint;
							intersectionPoint.index = secondPoint.index;
							segmentPath =  Draw3DSurface( firstPoint, intersectionPoint, reversed,
								area, graph, matrix, lightStyle, prevDataPointEx,
								positionZ, depth, points, pointIndex, pointLoopIndex,
								tension, operationType, surfaceSegmentType,
								topDarkening, bottomDarkening,
								new PointF(float.NaN, float.NaN),
								new PointF(float.NaN, float.NaN),
								clippedSegment,
								true, true);
						}

						if(segmentIndex == 1 && !reversed ||
							segmentIndex == 0 && reversed)
						{
							// Set coordinates of bottom points
							_thirdPointY2Value = (float)intersectionPoint.yPosition;

							// Draw second segment
							intersectionPoint.dataPoint = firstPoint.dataPoint;
							intersectionPoint.index = firstPoint.index;
							segmentPath =  Draw3DSurface( intersectionPoint, secondPoint, reversed,
								area, graph, matrix, lightStyle, prevDataPointEx,
								positionZ, depth, points, pointIndex, pointLoopIndex,
								tension, operationType, surfaceSegmentType,
								topDarkening, bottomDarkening,
								new PointF(float.NaN, float.NaN),
								new PointF(float.NaN, float.NaN),
								clippedSegment,
								true, true);
						}

						// Add segment path
						if(resultPath != null && segmentPath != null && segmentPath.PointCount > 0)
						{
							resultPath.AddPath(segmentPath, true);
						}

						// Reset bottom line "forced" Y coordinates
						_thirdPointY2Value = float.NaN;
						_fourthPointY2Value = float.NaN;

					}

					return resultPath;
				}
			}

            
			//****************************************************************
			//** Detect visibility of the bounding rectangle.
			//****************************************************************
			float minX = (float)Math.Min(firstPoint.xPosition, secondPoint.xPosition);
			float minY = (float)Math.Min(firstPoint.yPosition, secondPoint.yPosition);
			minY = (float)Math.Min(minY, axisPosition);
			float maxX = (float)Math.Max(firstPoint.xPosition, secondPoint.xPosition);
			float maxY = (float)Math.Max(firstPoint.yPosition, secondPoint.yPosition);
			maxY = (float)Math.Max(maxY, axisPosition);
			RectangleF position = new RectangleF(minX, minY, maxX - minX, maxY - minY);
			SurfaceNames visibleSurfaces = graph.GetVisibleSurfaces(position,positionZ,depth,matrix);

			// Check if area point is drawn upside down.
			bool upSideDown = false;
			if(firstPoint.yPosition >= thirdPoint.Y && secondPoint.yPosition >= fourthPoint.Y)
			{
				upSideDown = true;

				// Switch visibility between Top & Bottom surfaces
				bool topVisible = ( (visibleSurfaces & SurfaceNames.Top) == SurfaceNames.Top );
				bool bottomVisible = ( (visibleSurfaces & SurfaceNames.Bottom) == SurfaceNames.Bottom );
				visibleSurfaces ^= SurfaceNames.Bottom;
				visibleSurfaces ^= SurfaceNames.Top;
				if(topVisible)
				{
					visibleSurfaces |= SurfaceNames.Bottom;
				}
				if(bottomVisible)
				{
					visibleSurfaces |= SurfaceNames.Top;
				}
			}

			// Check Top/Bottom surface visibility
			GetTopSurfaceVisibility(area, firstPoint, secondPoint, upSideDown, positionZ, depth, matrix, ref visibleSurfaces);

			// Top and bottom surfaces are always visible for spline range
			bool bottomFirst = true;
			if(tension != 0f)
			{
				if( (visibleSurfaces & SurfaceNames.Bottom) == SurfaceNames.Bottom )
				{
					bottomFirst = false;
				}
				if( (visibleSurfaces & SurfaceNames.Bottom) == 0 &&
					(visibleSurfaces & SurfaceNames.Top) == 0 )
				{
					bottomFirst = false;
				}


				visibleSurfaces |= SurfaceNames.Bottom;
				visibleSurfaces |= SurfaceNames.Top;
			}

			// Round double values to 5 decimals
			firstPoint.xPosition = Math.Round(firstPoint.xPosition, 5);
			firstPoint.yPosition = Math.Round(firstPoint.yPosition, 5);
			secondPoint.xPosition = Math.Round(secondPoint.xPosition, 5);
			secondPoint.yPosition = Math.Round(secondPoint.yPosition, 5);

			//****************************************************************
			//** Clip area first and second data points inside 
			//** the plotting area.
			//****************************************************************
			if(ClipTopPoints(
				resultPath,
				ref firstPoint, 
				ref secondPoint, 
				reversed,
				area,
				graph, 
				matrix,
				lightStyle,
				prevDataPointEx,
				positionZ, 
				depth, 
				points,
				pointIndex, 
				pointLoopIndex,
				tension,
				operationType,
				surfaceSegmentType,
				topDarkening,
				bottomDarkening
				) == true)
			{
				return resultPath;
			}

			//****************************************************************
			//** Clip area third and fourth data points inside 
			//** the plotting area.
			//****************************************************************
			if(ClipBottomPoints(
				resultPath,
				ref firstPoint, 
				ref secondPoint, 
				ref thirdPoint,
				ref fourthPoint,
				reversed,
				area,
				graph, 
				matrix,
				lightStyle,
				prevDataPointEx,
				positionZ, 
				depth, 
				points,
				pointIndex, 
				pointLoopIndex,
				tension,
				operationType,
				surfaceSegmentType,
				topDarkening,
				bottomDarkening
				) == true)
			{
				return resultPath;
			}

			//****************************************************************
			//** Draw elements of area chart in 2 layers (back & front)
			//****************************************************************
			for(int elemLayer = 1; elemLayer <= 2; elemLayer++)
			{
				// Loop through all surfaces
				SurfaceNames[]	surfacesOrder = null;
				if(bottomFirst)
					surfacesOrder = new SurfaceNames[] {SurfaceNames.Back, SurfaceNames.Bottom, SurfaceNames.Top, SurfaceNames.Left, SurfaceNames.Right, SurfaceNames.Front};
				else
					surfacesOrder = new SurfaceNames[] {SurfaceNames.Back, SurfaceNames.Top, SurfaceNames.Bottom, SurfaceNames.Left, SurfaceNames.Right, SurfaceNames.Front};

				LineSegmentType lineSegmentType = LineSegmentType.Middle;
				foreach(SurfaceNames	currentSurface in surfacesOrder)
				{
					// Check id surface should be drawn
                    if (ChartGraphics.ShouldDrawLineChartSurface(area, area.ReverseSeriesOrder, currentSurface, visibleSurfaces, color, 
						points, firstPoint, secondPoint, this.multiSeries, ref lineSegmentType) != elemLayer)
					{
						continue;
					}

					// Draw only borders of the invisible elements on the back layer
					Color	surfaceColor = color;
					Color	surfaceBorderColor = pointAttr.dataPoint.BorderColor;
					if(elemLayer == 1)
					{
						// Draw only if point color is semi-transparent
						if(surfaceColor.A == 255)
						{
							continue;
						}

						// Define drawing colors
						surfaceColor = Color.Transparent;
						if(surfaceBorderColor == Color.Empty)
						{
							// If border color is emty use color slightly darker than main back color
							surfaceBorderColor = ChartGraphics.GetGradientColor( color, Color.Black, 0.2 );
						}
					}

					// Draw surfaces
					GraphicsPath surfacePath = null;
					switch(currentSurface)
					{
						case(SurfaceNames.Top):
							surfacePath = graph.Draw3DSurface( area, matrix, lightStyle, currentSurface, positionZ,  depth, 
								surfaceColor, surfaceBorderColor, pointAttr.dataPoint.BorderWidth, dashStyle, 
								firstPoint, secondPoint,  points, pointIndex,
								tension, operationType, LineSegmentType.Middle,
                                (this.showPointLines) ? true : false, false, area.ReverseSeriesOrder, this.multiSeries, 0, true);
							break;
						case(SurfaceNames.Bottom):
						{
							// Calculate coordinates
							DataPoint3D	dp1 = new DataPoint3D();
							dp1.dataPoint = firstPoint.dataPoint;
							dp1.index = firstPoint.index;
							dp1.xPosition = firstPoint.xPosition;
							dp1.yPosition = thirdPoint.Y;
							DataPoint3D	dp2 = new DataPoint3D();
							dp2.dataPoint = secondPoint.dataPoint;
							dp2.index = secondPoint.index;
							dp2.xPosition = secondPoint.xPosition;
							dp2.yPosition = fourthPoint.Y;

							// Draw surface
							surfacePath = graph.Draw3DSurface( area, matrix, lightStyle, currentSurface, positionZ, depth, 
								surfaceColor, surfaceBorderColor, pointAttr.dataPoint.BorderWidth, dashStyle, 
								dp1, dp2, points, pointIndex,
								tension, operationType, LineSegmentType.Middle,
                                (this.showPointLines) ? true : false, false, area.ReverseSeriesOrder, this.multiSeries, 1, true);
							break;
						}

						case(SurfaceNames.Left):
						{
							if(surfaceSegmentType == LineSegmentType.Single ||
                                (!area.ReverseSeriesOrder && surfaceSegmentType == LineSegmentType.First) ||
                                (area.ReverseSeriesOrder && surfaceSegmentType == LineSegmentType.Last))
							{
								
								// Calculate coordinates
								DataPoint3D	leftMostPoint = (firstPoint.xPosition <= secondPoint.xPosition) ? firstPoint : secondPoint;
								DataPoint3D	dp1 = new DataPoint3D();
								dp1.xPosition = leftMostPoint.xPosition;
								dp1.yPosition = (firstPoint.xPosition <= secondPoint.xPosition) ? thirdPoint.Y : fourthPoint.Y;
								DataPoint3D	dp2 = new DataPoint3D();
								dp2.xPosition = leftMostPoint.xPosition;;
								dp2.yPosition = leftMostPoint.yPosition;

								// Draw surface
								surfacePath = graph.Draw3DSurface( area, matrix, lightStyle, currentSurface, positionZ, depth, 
									surfaceColor, surfaceBorderColor, pointAttr.dataPoint.BorderWidth, dashStyle, 
									dp1, dp2, points, pointIndex,
                                    0f, operationType, LineSegmentType.Single, false, true, area.ReverseSeriesOrder, this.multiSeries, 0, true);
									
							}
							break;
						}
						case(SurfaceNames.Right):
						{
							if(surfaceSegmentType == LineSegmentType.Single ||
                                (!area.ReverseSeriesOrder && surfaceSegmentType == LineSegmentType.Last) ||
                                (area.ReverseSeriesOrder && surfaceSegmentType == LineSegmentType.First))

							{
								// Calculate coordinates
								DataPoint3D	rightMostPoint = (secondPoint.xPosition >= firstPoint.xPosition) ? secondPoint : firstPoint;
								DataPoint3D	dp1 = new DataPoint3D();
								dp1.xPosition = rightMostPoint.xPosition;
								dp1.yPosition = (secondPoint.xPosition >= firstPoint.xPosition) ? fourthPoint.Y : thirdPoint.Y;
								DataPoint3D	dp2 = new DataPoint3D();
								dp2.xPosition = rightMostPoint.xPosition;
								dp2.yPosition = rightMostPoint.yPosition;

								// Draw surface
								surfacePath = graph.Draw3DSurface( area, matrix, lightStyle, currentSurface, positionZ, depth, 
									surfaceColor, surfaceBorderColor, pointAttr.dataPoint.BorderWidth, dashStyle, 
									dp1, dp2, points, pointIndex,
                                    0f, operationType, LineSegmentType.Single, false, true, area.ReverseSeriesOrder, this.multiSeries, 0, true);
							}

							break;
						}
						case(SurfaceNames.Back):
						{
							// Calculate coordinates
							DataPoint3D	dp1 = new DataPoint3D();
							dp1.dataPoint = firstPoint.dataPoint;
							dp1.index = firstPoint.index;
							dp1.xPosition = firstPoint.xPosition;
							dp1.yPosition = thirdPoint.Y;
							DataPoint3D	dp2 = new DataPoint3D();
							dp2.dataPoint = secondPoint.dataPoint;
							dp2.index = secondPoint.index;
							dp2.xPosition = secondPoint.xPosition;
							dp2.yPosition = fourthPoint.Y;

							// Draw surface
							surfacePath = Draw3DSplinePolygon( graph, area, positionZ,
								surfaceColor, surfaceBorderColor, pointAttr.dataPoint.BorderWidth, 
								firstPoint, secondPoint, dp2, dp1, points, 
								tension, operationType, lineSegmentType, 
								(this.showPointLines) ? true : false);

							break;
						}
						case(SurfaceNames.Front):
						{
							
							// Calculate coordinates
							DataPoint3D	dp1 = new DataPoint3D();
							dp1.dataPoint = firstPoint.dataPoint;
							dp1.index = firstPoint.index;
							dp1.xPosition = firstPoint.xPosition;
							dp1.yPosition = thirdPoint.Y;
							DataPoint3D	dp2 = new DataPoint3D();
							dp2.dataPoint = secondPoint.dataPoint;
							dp2.index = secondPoint.index;
							dp2.xPosition = secondPoint.xPosition;
							dp2.yPosition = fourthPoint.Y;

							// Change segment type for the reversed series order
                            if (area.ReverseSeriesOrder)
							{
								if(lineSegmentType == LineSegmentType.First)
								{
									lineSegmentType = LineSegmentType.Last;
								}
								else if(lineSegmentType == LineSegmentType.Last)
								{
									lineSegmentType = LineSegmentType.First;
								}
							}

							if(surfaceSegmentType != LineSegmentType.Single)
							{
								if( surfaceSegmentType == LineSegmentType.Middle ||
									( surfaceSegmentType == LineSegmentType.First && lineSegmentType != LineSegmentType.First) ||
									( surfaceSegmentType == LineSegmentType.Last && lineSegmentType != LineSegmentType.Last) )
								{
									lineSegmentType = LineSegmentType.Middle;
								}
								if(reversed) 
								{
									if(lineSegmentType == LineSegmentType.First)
									{
										lineSegmentType = LineSegmentType.Last;
									}
									else if(lineSegmentType == LineSegmentType.Last)
									{
										lineSegmentType = LineSegmentType.First;
									}
								}
							}

							// Draw surface
							surfacePath = Draw3DSplinePolygon( graph, area, positionZ + depth,
								surfaceColor, surfaceBorderColor, pointAttr.dataPoint.BorderWidth, 
								firstPoint, secondPoint, dp2, dp1, points, 
								tension, operationType, lineSegmentType, 
								(this.showPointLines) ? true : false);
								
							break;
						}
					}

					// Add path of the fully visible surfaces to the result surface
					if(elemLayer == 2 && resultPath != null && surfacePath != null && surfacePath.PointCount > 0)
					{
						resultPath.CloseFigure();
						resultPath.AddPath(surfacePath, true);
					}

				}
			}

			return resultPath;
		}

		/// <summary>
		/// Gets visibility of the top surface.
		/// </summary>
		/// <param name="area">Chart area object.</param>
		/// <param name="firstPoint">First data point of the line.</param>
		/// <param name="secondPoint">Second data point of the line.</param>
		/// <param name="upSideDown">Indicates that Y values of the data points are below axis line.</param>
		/// <param name="positionZ">Z coordinate of the back side of the cube.</param>
		/// <param name="depth">Cube depth.</param>
		/// <param name="matrix">Coordinate transformation matrix.</param>
		/// <param name="visibleSurfaces">Surface visibility reference. Initialized with bounary cube visibility.</param>
		protected virtual void GetTopSurfaceVisibility(
			ChartArea area,
			DataPoint3D firstPoint, 
			DataPoint3D secondPoint, 
			bool upSideDown,
			float positionZ, 
			float depth, 
			Matrix3D matrix,
			ref SurfaceNames visibleSurfaces)
		{
			//***********************************************************************
			//** Check Top surface visibility
			//***********************************************************************
			// If Top surface visibility in bounding rectangle - do not gurantee angled linde visibility
			if( (visibleSurfaces & SurfaceNames.Top) == SurfaceNames.Top)
			{
				visibleSurfaces ^= SurfaceNames.Top;
			}

			// Create top surface coordinates in 3D space
			Point3D[] cubePoints = new Point3D[3];
            if (!area.ReverseSeriesOrder)
			{
				if(!upSideDown && firstPoint.xPosition < secondPoint.xPosition ||
					upSideDown && firstPoint.xPosition > secondPoint.xPosition)
				{
					cubePoints[0] = new Point3D( (float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ + depth );
					cubePoints[1] = new Point3D( (float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ );
					cubePoints[2] = new Point3D( (float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ );
				}
				else
				{
					cubePoints[0] = new Point3D( (float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ + depth );
					cubePoints[1] = new Point3D( (float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ );
					cubePoints[2] = new Point3D( (float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ );
				}
			}
			else
			{
				if(!upSideDown && secondPoint.xPosition < firstPoint.xPosition ||
					upSideDown && secondPoint.xPosition > firstPoint.xPosition)
				{
					cubePoints[0] = new Point3D( (float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ + depth );
					cubePoints[1] = new Point3D( (float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ );
					cubePoints[2] = new Point3D( (float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ );
				}
				else
				{
					cubePoints[0] = new Point3D( (float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ + depth );
					cubePoints[1] = new Point3D( (float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ );
					cubePoints[2] = new Point3D( (float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ );
				}
			}

			// Tranform coordinates 
			matrix.TransformPoints( cubePoints );

			// Check the top side visibility
			if(ChartGraphics.IsSurfaceVisible(cubePoints[0],cubePoints[1],cubePoints[2]))
			{
				visibleSurfaces |= SurfaceNames.Top;
			}


			//***********************************************************************
			//** Check Bottom surface visibility
			//***********************************************************************

			// Get bottom surface points
			PointF	thirdPoint, fourthPoint; 
			GetBottomPointsPosition(area.Common, area, 0, ref firstPoint, ref secondPoint, out thirdPoint, out fourthPoint);


			// If Bottom surface visibility in bounding rectangle - do not gurantee angled linde visibility
			if( (visibleSurfaces & SurfaceNames.Bottom) == SurfaceNames.Bottom)
			{
				visibleSurfaces ^= SurfaceNames.Bottom;
			}

			// Create top surface coordinates in 3D space
			cubePoints = new Point3D[3];
            if (!area.ReverseSeriesOrder)
			{
				if(!upSideDown && firstPoint.xPosition < secondPoint.xPosition ||
					upSideDown && firstPoint.xPosition > secondPoint.xPosition)
				{
					cubePoints[0] = new Point3D( (float)firstPoint.xPosition, (float)thirdPoint.Y, positionZ + depth );
					cubePoints[1] = new Point3D( (float)firstPoint.xPosition, (float)thirdPoint.Y, positionZ );
					cubePoints[2] = new Point3D( (float)secondPoint.xPosition, (float)fourthPoint.Y, positionZ );
				}
				else
				{
					cubePoints[0] = new Point3D( (float)secondPoint.xPosition, (float)fourthPoint.Y, positionZ + depth );
					cubePoints[1] = new Point3D( (float)secondPoint.xPosition, (float)fourthPoint.Y, positionZ );
					cubePoints[2] = new Point3D( (float)firstPoint.xPosition, (float)thirdPoint.Y, positionZ );
				}
			}
			else
			{
				if(!upSideDown && secondPoint.xPosition < firstPoint.xPosition ||
					upSideDown && secondPoint.xPosition > firstPoint.xPosition)
				{
					cubePoints[0] = new Point3D( (float)secondPoint.xPosition, (float)fourthPoint.Y, positionZ + depth );
					cubePoints[1] = new Point3D( (float)secondPoint.xPosition, (float)fourthPoint.Y, positionZ );
					cubePoints[2] = new Point3D( (float)firstPoint.xPosition, (float)thirdPoint.Y, positionZ );
				}
				else
				{
					cubePoints[0] = new Point3D( (float)firstPoint.xPosition, (float)thirdPoint.Y, positionZ + depth );
					cubePoints[1] = new Point3D( (float)firstPoint.xPosition, (float)thirdPoint.Y, positionZ );
					cubePoints[2] = new Point3D( (float)secondPoint.xPosition, (float)fourthPoint.Y, positionZ );
				}
			}

			// Tranform coordinates 
			matrix.TransformPoints( cubePoints );

			// Check the top side visibility
			if(ChartGraphics.IsSurfaceVisible(cubePoints[2],cubePoints[1],cubePoints[0]))
			{
				visibleSurfaces |= SurfaceNames.Bottom;
			}

		}

		/// <summary>
		/// Gets position ob the bottom points in area chart.
		/// </summary>
		/// <param name="common">Chart common elements.</param>
		/// <param name="area">Chart area the series belongs to.</param>
		/// <param name="axisPosition">Axis position.</param>
		/// <param name="firstPoint">First top point coordinates.</param>
		/// <param name="secondPoint">Second top point coordinates.</param>
		/// <param name="thirdPoint">Returns third bottom point coordinates.</param>
		/// <param name="fourthPoint">Returns fourth bottom point coordinates.</param>
		protected virtual void GetBottomPointsPosition(
			CommonElements common, 
			ChartArea area, 
			float axisPosition, 
			ref DataPoint3D firstPoint, 
			ref DataPoint3D secondPoint, 
			out PointF thirdPoint, 
			out PointF fourthPoint)
		{
			// Set active vertical axis
			Axis	vAxis = area.GetAxis(AxisName.Y, firstPoint.dataPoint.series.YAxisType, firstPoint.dataPoint.series.YSubAxisName);

			// Initialize points using second Y value
			float secondYValue = (float)vAxis.GetPosition(firstPoint.dataPoint.YValues[1]);
			thirdPoint = new PointF((float)firstPoint.xPosition, secondYValue);
			secondYValue = (float)vAxis.GetPosition(secondPoint.dataPoint.YValues[1]);
			fourthPoint = new PointF((float)secondPoint.xPosition, secondYValue);

			// Check if "forced" Y values where set
			if(!float.IsNaN(_thirdPointY2Value))
			{
				thirdPoint.Y = _thirdPointY2Value;
				
			}
			if(!float.IsNaN(_fourthPointY2Value))
			{
				fourthPoint.Y = _fourthPointY2Value;
			}
		}

		/// <summary>
		/// Draws a 3D polygon defined by 4 points in 2D space.
		/// Top and Bottom lines are drawn as splines of specified tension.
		/// </summary>
		/// <param name="area">Chart area reference.</param>
		/// <param name="graph">Chart graphics.</param>
		/// <param name="positionZ">Z position of the back side of the 3D surface.</param>
		/// <param name="backColor">Color of rectangle</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="firstPoint">First point.</param>
		/// <param name="secondPoint">Second point.</param>
		/// <param name="thirdPoint">Third point.</param>
		/// <param name="fourthPoint">Fourth point.</param>
		/// <param name="points">Array of points.</param>
		/// <param name="tension">Line tension.</param>
		/// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
		/// <param name="lineSegmentType">AxisName of line segment. Used for step lines and splines.</param>
		/// <param name="forceThinBorder">Thin border will be drawn on all segments.</param>
		/// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
		internal GraphicsPath Draw3DSplinePolygon( 
			ChartGraphics graph, 
			ChartArea area,
			float positionZ, 
			Color backColor, 
			Color borderColor, 
			int borderWidth, 
			DataPoint3D	firstPoint,
			DataPoint3D	secondPoint, 
			DataPoint3D	thirdPoint,
			DataPoint3D	fourthPoint, 
			ArrayList points,
			float tension,
			DrawingOperationTypes operationType,
			LineSegmentType lineSegmentType,
			bool forceThinBorder)
		{
			// Check tension parameter
			if(tension == 0f)
			{
				SurfaceNames	thinBorderSides = 0;
				if(forceThinBorder)
				{
					thinBorderSides = SurfaceNames.Left | SurfaceNames.Right;
				}

				return graph.Draw3DPolygon( area, area.matrix3D, SurfaceNames.Front, positionZ, 
					backColor, borderColor, borderWidth, 
					firstPoint, secondPoint, thirdPoint, fourthPoint, 
					operationType, lineSegmentType, thinBorderSides);
			}

			// Create graphics path for selection
			bool	drawElements = ((operationType & DrawingOperationTypes.DrawElement) == DrawingOperationTypes.DrawElement);
			GraphicsPath	resultPath = new GraphicsPath();

			//**********************************************************************
			//** Prepare, transform polygon coordinates
			//**********************************************************************

			// Get top line path
			GraphicsPath	topLine = graph.GetSplineFlattenPath(
				area, positionZ, 
				firstPoint, secondPoint, points, tension, false, true, 0);

			// Get bottom line path
			GraphicsPath	bottomLine = graph.GetSplineFlattenPath(
				area, positionZ, 
				thirdPoint, fourthPoint, points, tension, false, true, 1);

			// Add paths to the result path
			resultPath.AddPath(topLine, true);
			resultPath.AddPath(bottomLine, true);
			resultPath.CloseAllFigures();


			//**********************************************************************
			//** Define drawing colors
			//**********************************************************************

			// Define 3 points polygon
			Point3D [] points3D = new Point3D[3];
			points3D[0] = new Point3D((float)firstPoint.xPosition, (float)firstPoint.yPosition, positionZ);
			points3D[1] = new Point3D((float)secondPoint.xPosition, (float)secondPoint.yPosition, positionZ);
			points3D[2] = new Point3D((float)thirdPoint.xPosition, (float)thirdPoint.yPosition, positionZ);

			// Transform coordinates
			area.matrix3D.TransformPoints( points3D );

			// Get colors
			bool topIsVisible = ChartGraphics.IsSurfaceVisible( points3D[0], points3D[1], points3D[2]);
            Color polygonColor = area.matrix3D.GetPolygonLight(points3D, backColor, topIsVisible, area.Area3DStyle.Rotation, SurfaceNames.Front, area.ReverseSeriesOrder);
			Color	surfaceBorderColor = borderColor;
			if(surfaceBorderColor == Color.Empty)
			{
				// If border color is emty use color slightly darker than main back color
				surfaceBorderColor = ChartGraphics.GetGradientColor( backColor, Color.Black, 0.2 );
			}

			//**********************************************************************
			//** Draw elements if required.
			//**********************************************************************
			Pen thickBorderPen = null;
			if(drawElements)
			{
				// Remember SmoothingMode and turn off anti aliasing
				SmoothingMode oldSmoothingMode = graph.SmoothingMode;
				graph.SmoothingMode = SmoothingMode.Default;

				// Draw the polygon
                using (Brush brush = new SolidBrush(polygonColor))
                {
                    graph.FillPath(brush, resultPath);
                }

				// Return old smoothing mode
				graph.SmoothingMode = oldSmoothingMode;
			
				// Draw thin polygon border of darker color around the whole polygon
				if(forceThinBorder)
				{
					graph.DrawPath(new Pen(surfaceBorderColor, 1), resultPath);
				}
				else if(polygonColor.A == 255)
				{
					graph.DrawPath(new Pen(polygonColor, 1), resultPath);
				}

				// Create thick border line pen
				thickBorderPen = new Pen(surfaceBorderColor, borderWidth);
				thickBorderPen.StartCap = LineCap.Round;
				thickBorderPen.EndCap = LineCap.Round;

				// Draw thick Top & Bottom lines
				graph.DrawPath(thickBorderPen, topLine);
				graph.DrawPath(thickBorderPen, bottomLine);

				// Draw thick Right & Left lines on first & last segments of the line
				if(lineSegmentType == LineSegmentType.First)
				{
					graph.DrawLine(thickBorderPen, topLine.PathPoints[0], bottomLine.GetLastPoint());
				
				}
				else if(lineSegmentType == LineSegmentType.Last)
				{
					graph.DrawLine(thickBorderPen, topLine.GetLastPoint(), bottomLine.PathPoints[0]);					
				}
			}


			// Calculate path for selection
			if(resultPath != null && thickBorderPen != null)
			{
				// Widen result path
				try
				{
					resultPath.Widen(thickBorderPen);
				}
                catch (OutOfMemoryException)
                {
                    // GraphicsPath.Widen incorrectly throws OutOfMemoryException
                    // catching here and reacting by not widening
                }
                catch (ArgumentException)
                {
                }
			}

			return resultPath;
		}

	#endregion

        #region IDisposable overrides
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (this.areaBottomPath != null)
                {
                    this.areaBottomPath.Dispose();
                    this.areaBottomPath = null;
                }
                if (this.areaPath != null)
                {
                    this.areaPath.Dispose();
                    this.areaPath = null;
                }
            }
            base.Dispose(disposing);
        }
        #endregion

	}
}

