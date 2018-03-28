//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		FastLineChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	FastLineChart
//
//  Purpose:	When performance is critical, the FastLine chart 
//              type is a good alternative to the Line chart. FastLine 
//              charts significantly reduce the drawing time of a 
//              series that contains a very large number of data points.
//              
//              To make the FastLine chart a high performance chart, 
//              some charting features have been omitted. The features 
//              omitted include the ability to control Point level 
//              visual properties, the ability to draw markers, the 
//              use of data point labels, shadows, and the use of 
//              chart animation.
//
//              FastLine chart performance was improved by limiting 
//              visual appearance features and by introducing data 
//              point compacting algorithm. When chart contains 
//              thousands of data points, it is common to have tens 
//              or hundreds points displayed in the area comparable 
//              to a single pixel. FastLine algorithm accumulates 
//              point information and only draw points if they extend 
//              outside currently filled pixels.
//              
//	Reviewed:	AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

#if Microsoft_CONTROL
using System.Windows.Forms.DataVisualization.Charting.Utilities;
#else
using System.Web.UI.DataVisualization.Charting;

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
    /// FastLineChart class implements a simplified line chart drawing 
    /// algorithm which is optimized for the performance.
	/// </summary>
    internal class FastLineChart : IChartType
	{
		#region Fields and Constructor

		/// <summary>
		/// Indicates that chart is drawn in 3D area
		/// </summary>
		internal bool				chartArea3DEnabled = false;
		
		/// <summary>
		/// Current chart graphics
		/// </summary>
        internal ChartGraphics Graph { get; set; }

		/// <summary>
		/// Z coordinate of the 3D series
		/// </summary>
        internal float seriesZCoordinate = 0f;

		/// <summary>
		/// 3D transformation matrix
		/// </summary>
        internal Matrix3D matrix3D = null;

		/// <summary>
		/// Reference to common chart elements
		/// </summary>
        internal CommonElements Common { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public FastLineChart()
		{
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		virtual public string Name			{ get{ return ChartTypeNames.FastLine;}}

		/// <summary>
		/// True if chart type is stacked
		/// </summary>
		virtual public bool Stacked		{ get{ return false;}}


		/// <summary>
		/// True if stacked chart type supports groups
		/// </summary>
		virtual public bool SupportStackedGroups	{ get { return false; } }


		/// <summary>
		/// True if stacked chart type should draw separately positive and 
		/// negative data points ( Bar and column Stacked types ).
		/// </summary>
		public bool StackSign		{ get{ return false;}}

		/// <summary>
		/// True if chart type supports axeses
		/// </summary>
		virtual public bool RequireAxes	{ get{ return true;} }

		/// <summary>
		/// Chart type with two y values used for scale ( bubble chart type )
		/// </summary>
		virtual public bool SecondYScale{ get{ return false;} }

		/// <summary>
		/// True if chart type requires circular chart area.
		/// </summary>
		public bool CircularChartArea	{ get{ return false;} }

		/// <summary>
		/// True if chart type supports logarithmic axes
		/// </summary>
		virtual public bool SupportLogarithmicAxes	{ get{ return true;} }

		/// <summary>
		/// True if chart type requires to switch the value (Y) axes position
		/// </summary>
		virtual public bool SwitchValueAxes	{ get{ return false;} }

		/// <summary>
		/// True if chart series can be placed side-by-side.
		/// </summary>
		virtual public bool SideBySideSeries { get{ return false;} }

		/// <summary>
		/// True if each data point of a chart must be represented in the legend
		/// </summary>
		virtual public bool DataPointsInLegend	{ get{ return false;} }

		/// <summary>
		/// If the crossing value is auto Crossing value should be 
		/// automatically set to zero for some chart 
		/// types (Bar, column, area etc.)
		/// </summary>
		virtual public bool ZeroCrossing { get{ return false;} }

		/// <summary>
		/// True if palette colors should be applied for each data paoint.
		/// Otherwise the color is applied to the series.
		/// </summary>
		virtual public bool ApplyPaletteColorsToPoints	{ get { return false; } }

		/// <summary>
		/// Indicates that extra Y values are connected to the scale of the Y axis
		/// </summary>
		virtual public bool ExtraYValuesConnectedToYAxis{ get { return false; } }
		
		/// <summary>
		/// Indicates that it's a hundredred percent chart.
		/// Axis scale from 0 to 100 percent should be used.
		/// </summary>
		virtual public bool HundredPercent{ get{return false;} }

		/// <summary>
		/// Indicates that it's a hundredred percent chart.
		/// Axis scale from 0 to 100 percent should be used.
		/// </summary>
		virtual public bool HundredPercentSupportNegative{ get{return false;} }

		/// <summary>
		/// How to draw series/points in legend:
		/// Filled rectangle, Line or Marker
		/// </summary>
		/// <param name="series">Legend item series.</param>
		/// <returns>Legend item style.</returns>
		virtual public LegendImageStyle GetLegendImageStyle(Series series)
		{
			return LegendImageStyle.Line;
		}

		/// <summary>
		/// Number of supported Y value(s) per point 
		/// </summary>
		virtual public int YValuesPerPoint	{ get { return 1; } }

		/// <summary>
		/// Gets chart type image.
		/// </summary>
		/// <param name="registry">Chart types registry object.</param>
		/// <returns>Chart type image.</returns>
        virtual public System.Drawing.Image GetImage(ChartTypeRegistry registry)
		{
			return (System.Drawing.Image)registry.ResourceManager.GetObject(this.Name + "ChartType");
		}

		#endregion

		#region Painting

		/// <summary>
		/// Paint FastLine Chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		virtual public void Paint( 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			Series seriesToDraw )
		{	
			this.Common = common;
			this.Graph = graph;
			bool	clipRegionSet = false;
			if(area.Area3DStyle.Enable3D)
			{
				// Initialize variables
				this.chartArea3DEnabled = true;
				matrix3D = area.matrix3D;
			}
			else
			{
				this.chartArea3DEnabled = false;
			}
			
			//************************************************************
			//** Loop through all series
			//************************************************************
			foreach( Series series in common.DataManager.Series )
			{
				// Process non empty series of the area with FastLine chart type
				if( String.Compare( series.ChartTypeName, this.Name, true, System.Globalization.CultureInfo.CurrentCulture ) != 0 
					|| series.ChartArea != area.Name || 
					!series.IsVisible())
				{
					continue;
				}

				// Get 3D series depth and Z position
				if(this.chartArea3DEnabled)
				{
					float seriesDepth;
					area.GetSeriesZPositionAndDepth(series, out seriesDepth, out seriesZCoordinate);
					this.seriesZCoordinate += seriesDepth/2.0f;
				}

				// Set active horizontal/vertical axis
				Axis hAxis = area.GetAxis(AxisName.X, series.XAxisType, (area.Area3DStyle.Enable3D) ? string.Empty : series.XSubAxisName);
				Axis vAxis = area.GetAxis(AxisName.Y, series.YAxisType, (area.Area3DStyle.Enable3D) ? string.Empty : series.YSubAxisName);
				double hAxisMin = hAxis.ViewMinimum;
				double hAxisMax = hAxis.ViewMaximum;
				double vAxisMin = vAxis.ViewMinimum;
				double vAxisMax = vAxis.ViewMaximum;

				// Get "PermittedPixelError" attribute
				float	permittedPixelError = 1.0f;
                if (series.IsCustomPropertySet(CustomPropertyName.PermittedPixelError))
                {
                    string attrValue = series[CustomPropertyName.PermittedPixelError];

                    float pixelError;
                    bool parseSucceed = float.TryParse(attrValue, NumberStyles.Any, CultureInfo.CurrentCulture, out pixelError);

                    if (parseSucceed)
                    {
                        permittedPixelError = pixelError;
                    }
                    else
                    {
                        throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid2("PermittedPixelError")));
                    }

                    // "PermittedPixelError" attribute value should be in range from zero to 1
                    if (permittedPixelError < 0f || permittedPixelError > 1f)
                    {
                        throw (new InvalidOperationException(SR.ExceptionCustomAttributeIsNotInRange0to1("PermittedPixelError")));
                    }
                }

				// Get pixel size in axes coordinates
				SizeF pixelSize = graph.GetRelativeSize(new SizeF(permittedPixelError, permittedPixelError));
				SizeF axesMin = graph.GetRelativeSize(new SizeF((float)hAxisMin, (float)vAxisMin));
				double axesValuesPixelSizeX = Math.Abs(hAxis.PositionToValue(axesMin.Width + pixelSize.Width, false) - hAxis.PositionToValue(axesMin.Width, false));

				// Create line pen
				Pen	linePen = new Pen(series.Color, series.BorderWidth);
				linePen.DashStyle = graph.GetPenStyle( series.BorderDashStyle );
				linePen.StartCap = LineCap.Round;
				linePen.EndCap = LineCap.Round;

				// Create empty line pen
				Pen	emptyLinePen = new Pen(series.EmptyPointStyle.Color, series.EmptyPointStyle.BorderWidth);
				emptyLinePen.DashStyle = graph.GetPenStyle( series.EmptyPointStyle.BorderDashStyle );
				emptyLinePen.StartCap = LineCap.Round;
				emptyLinePen.EndCap = LineCap.Round;

				// Check if series is indexed
				bool indexedSeries = ChartHelper.IndexedSeries(this.Common, series.Name );

				// Loop through all ponts in the series
				int		index = 0;
				double	yValueRangeMin = double.NaN;
				double	yValueRangeMax = double.NaN;
				DataPoint pointRangeMin = null;
				DataPoint pointRangeMax = null;
				double	xValue = 0;
				double	yValue = 0;
				double	xValuePrev = 0;
				double	yValuePrev = 0;
				DataPoint prevDataPoint = null;
				PointF	lastVerticalSegmentPoint = PointF.Empty;
				PointF	prevPoint = PointF.Empty;
				PointF	currentPoint = PointF.Empty;
				bool	prevPointInAxesCoordinates = false;
				bool	verticalLineDetected = false;
				bool	prevPointIsEmpty = false;
				bool	currentPointIsEmpty = false;
                bool    firstNonEmptyPoint = false;
				double	xPixelConverter = (graph.Common.ChartPicture.Width - 1.0) / 100.0;
				double	yPixelConverter = (graph.Common.ChartPicture.Height - 1.0) / 100.0;
				foreach( DataPoint point in series.Points )
				{
					// Get point X and Y values
					xValue = (indexedSeries) ? index + 1 : point.XValue;
					xValue = hAxis.GetLogValue(xValue);
					yValue = vAxis.GetLogValue(point.YValues[0]);
					currentPointIsEmpty = point.IsEmpty;

                    // NOTE: Fixes issue #7094
                    // If current point is non-empty but the previous one was, 
                    // use empty point style properties to draw it.
                    if (prevPointIsEmpty && !currentPointIsEmpty && !firstNonEmptyPoint)
                    {
                        firstNonEmptyPoint = true;
                        currentPointIsEmpty = true;
                    }
                    else
                    {
                        firstNonEmptyPoint = false;
                    }

					// Check if line is completly out of the data scaleView
					if( !verticalLineDetected &&
						((xValue < hAxisMin && xValuePrev < hAxisMin) ||
						(xValue > hAxisMax && xValuePrev > hAxisMax) ||
						(yValue < vAxisMin && yValuePrev < vAxisMin) ||
						(yValue > vAxisMax && yValuePrev > vAxisMax) ))
					{
						xValuePrev = xValue;
						yValuePrev = yValue;
						prevPointInAxesCoordinates = true;
						++index;
						continue;
					}
					else if(!clipRegionSet)
					{
						// Check if line is partialy in the data scaleView
						if(xValuePrev < hAxisMin || xValuePrev > hAxisMax || 
							xValue > hAxisMax || xValue < hAxisMin ||
							yValuePrev < vAxisMin || yValuePrev > vAxisMax ||
							yValue < vAxisMin || yValue > vAxisMax )
						{
							// Set clipping region for line drawing 
							graph.SetClip( area.PlotAreaPosition.ToRectangleF() );
							clipRegionSet = true;
						}
					}

					// Check if point may be skipped
					if(index > 0 &&
						currentPointIsEmpty == prevPointIsEmpty)
					{
						// Check if points X value in acceptable error boundary
						if( Math.Abs(xValue - xValuePrev) < axesValuesPixelSizeX)
						{
							if(!verticalLineDetected)
							{
								verticalLineDetected = true;
								if(yValue > yValuePrev)
								{
									yValueRangeMax = yValue;
									yValueRangeMin = yValuePrev;
									pointRangeMax = point;
									pointRangeMin = prevDataPoint;
								}
								else
								{
									yValueRangeMax = yValuePrev;
									yValueRangeMin = yValue;
									pointRangeMax = prevDataPoint;
									pointRangeMin = point;
								}

								// NOTE: Prev. version code - A.G.
//								yValueRangeMin = Math.Min(yValue, yValuePrev);
//								yValueRangeMax = Math.Max(yValue, yValuePrev);
							}
							else
							{
								if(yValue > yValueRangeMax)
								{
									yValueRangeMax = yValue;
									pointRangeMax = point;
								}

								else if(yValue < yValueRangeMin)
								{
									yValueRangeMin = yValue;
									pointRangeMin = point;
								}

								// NOTE: Prev. version code - A.G.
//								yValueRangeMin = Math.Min(yValue, yValueRangeMin);
//								yValueRangeMax = Math.Max(yValue, yValueRangeMax);
							}

							// Remember last point
							prevDataPoint = point;

							// Remember last vertical range point
							// Note! Point is in axes coordinate.
							lastVerticalSegmentPoint.Y = (float)yValue;

							// Increase counter and proceed to next data point
							++index;
							continue;
						}
					}

					// Get point pixel position
					currentPoint.X = (float)
						(hAxis.GetLinearPosition( xValue ) * xPixelConverter);
					currentPoint.Y = (float)
						(vAxis.GetLinearPosition( yValue ) * yPixelConverter); 

					// Check if previous point must be converted from axes values to pixels
					if(prevPointInAxesCoordinates)
					{
						prevPoint.X = (float)
							(hAxis.GetLinearPosition( xValuePrev ) * xPixelConverter);
						prevPoint.Y = (float)
							(vAxis.GetLinearPosition( yValuePrev ) * yPixelConverter); 
					}

					// Draw accumulated vertical line (with minimal X values differences)
					if(verticalLineDetected)
					{
						// Convert Y coordinates to pixels
						yValueRangeMin = (vAxis.GetLinearPosition( yValueRangeMin ) * yPixelConverter); 
						yValueRangeMax = (vAxis.GetLinearPosition( yValueRangeMax ) * yPixelConverter); 

						// Draw accumulated vertical line
						DrawLine(
							series,
							prevDataPoint,
							pointRangeMin,
							pointRangeMax,
							index,
							(prevPointIsEmpty) ? emptyLinePen : linePen, 
							prevPoint.X, 
							(float)yValueRangeMin, 
							prevPoint.X,
							(float)yValueRangeMax);
						
						// Reset vertical line detected flag
						verticalLineDetected = false;

						// Convert last point of the vertical line segment to pixel coordinates
						prevPoint.Y = (float)
							(vAxis.GetLinearPosition( lastVerticalSegmentPoint.Y ) * yPixelConverter); 
					}
					
					// Draw line from previous to current point
					if(index > 0)
					{
						DrawLine(
							series,
							point,
							pointRangeMin,
							pointRangeMax,
							index,
							(currentPointIsEmpty) ? emptyLinePen : linePen,  
							prevPoint.X, 
							prevPoint.Y, 
							currentPoint.X,
							currentPoint.Y);
					}

					// Remember last point coordinates
					xValuePrev = xValue;
					yValuePrev = yValue;
					prevDataPoint = point;
					prevPoint = currentPoint;
					prevPointInAxesCoordinates = false;
					prevPointIsEmpty = currentPointIsEmpty;
					++index;
				}

				// Draw last accumulated line segment
				if(verticalLineDetected)
				{
					// Check if previous point must be converted from axes values to pixels
					if(prevPointInAxesCoordinates)
					{
						prevPoint.X = (float)
							(hAxis.GetLinearPosition( xValuePrev ) * xPixelConverter);
						prevPoint.Y = (float)
							(vAxis.GetLinearPosition( yValuePrev ) * yPixelConverter); 
					}

					// Convert Y coordinates to pixels
					yValueRangeMin = (vAxis.GetLinearPosition( yValueRangeMin ) * yPixelConverter); 
					yValueRangeMax = (vAxis.GetLinearPosition( yValueRangeMax ) * yPixelConverter); 

					// Draw accumulated vertical line
					DrawLine(
						series,
						prevDataPoint,
						pointRangeMin,
						pointRangeMax,
						index - 1,
						(prevPointIsEmpty) ? emptyLinePen : linePen, 
						prevPoint.X, 
						(float)yValueRangeMin, 
						prevPoint.X,
						(float)yValueRangeMax);
						
					verticalLineDetected = false;
					yValueRangeMin = double.NaN;
					yValueRangeMax = double.NaN;
					pointRangeMin = null;
					pointRangeMax = null;
				}

			}

			// Reset Clip Region
			if(clipRegionSet)
			{
				graph.ResetClip();
			}
	
		}

		/// <summary>
		/// Draws a line connecting two PointF structures.
		/// </summary>
		/// <param name="series">Chart series.</param>
		/// <param name="point">Series last data point in the group.</param>
		/// <param name="pointMin">Series minimum Y value data point in the group.</param>
		/// <param name="pointMax">Series maximum Y value data point in the group.</param>
		/// <param name="pointIndex">Point index.</param>
		/// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
		/// <param name="firstPointX">First point X coordinate.</param>
		/// <param name="firstPointY">First point Y coordinate</param>
		/// <param name="secondPointX">Second point X coordinate.</param>
		/// <param name="secondPointY">Second point Y coordinate</param>
		public virtual void DrawLine(
			Series series,
			DataPoint point,
			DataPoint pointMin,
			DataPoint pointMax,
			int pointIndex,
			Pen pen,
			float firstPointX,
			float firstPointY,
			float secondPointX,
			float secondPointY
			)
		{
			// Transform 3D coordinates
			if(chartArea3DEnabled)
			{
				Point3D [] points = new Point3D[2];

				// All coordinates has to be transformed in relative coordinate system
				// NOTE: Fixes issue #5496
				PointF firstPoint = Graph.GetRelativePoint(new PointF(firstPointX, firstPointY));
				PointF secondPoint = Graph.GetRelativePoint(new PointF(secondPointX, secondPointY));

				points[0] = new Point3D(firstPoint.X, firstPoint.Y, seriesZCoordinate);
				points[1] = new Point3D(secondPoint.X, secondPoint.Y, seriesZCoordinate);
				matrix3D.TransformPoints( points );

				// All coordinates has to be transformed back to pixels
				// NOTE: Fixes issue #5496
				points[0].PointF = Graph.GetAbsolutePoint(points[0].PointF);
				points[1].PointF = Graph.GetAbsolutePoint(points[1].PointF);

				firstPointX = points[0].X;
				firstPointY = points[0].Y;
				secondPointX = points[1].X;
				secondPointY = points[1].Y;
			}

			// Draw line
			Graph.DrawLine(pen, firstPointX, firstPointY, secondPointX,secondPointY);

			// Process selection regions
			if( this.Common.ProcessModeRegions )
			{
				// Create grapics path object for the line
                using (GraphicsPath path = new GraphicsPath())
                {
                    float width = pen.Width + 2;

                    if (Math.Abs(firstPointX - secondPointX) > Math.Abs(firstPointY - secondPointY))
                    {
                        path.AddLine(firstPointX, firstPointY - width, secondPointX, secondPointY - width);
                        path.AddLine(secondPointX, secondPointY + width, firstPointX, firstPointY + width);
                        path.CloseAllFigures();
                    }
                    else
                    {
                        path.AddLine(firstPointX - width, firstPointY, secondPointX - width, secondPointY);
                        path.AddLine(secondPointX + width, secondPointY, firstPointX + width, firstPointY);
                        path.CloseAllFigures();
                    }

                    // Calculate bounding rectangle
                    RectangleF pathBounds = path.GetBounds();

                    // If one side of the bounding rectangle is less than 2 pixels
                    // use rectangle region shape to optimize used coordinates space
                    if (pathBounds.Width <= 2.0 || pathBounds.Height <= 2.0)
                    {
                        // Add hot region path as rectangle
                        pathBounds.Inflate(pen.Width, pen.Width);
                        this.Common.HotRegionsList.AddHotRegion(
                            Graph.GetRelativeRectangle(pathBounds),
                            point,
                            point.series.Name,
                            pointIndex);
                    }
                    else
                    {
                        // Add hot region path as polygon
                        this.Common.HotRegionsList.AddHotRegion(
                            path,
                            false,
                            Graph,
                            point,
                            point.series.Name,
                            pointIndex);
                    }
                }
			}
		}

		#endregion

		#region Y values related methods

		/// <summary>
		/// Helper function, which returns the Y value of the point.
		/// </summary>
		/// <param name="common">Chart common elements.</param>
		/// <param name="area">Chart area the series belongs to.</param>
		/// <param name="series">Sereis of the point.</param>
		/// <param name="point">Point object.</param>
		/// <param name="pointIndex">Index of the point.</param>
		/// <param name="yValueIndex">Index of the Y value to get.</param>
		/// <returns>Y value of the point.</returns>
		virtual public double GetYValue(
			CommonElements common, 
			ChartArea area, 
			Series series, 
			DataPoint point, 
			int pointIndex, 
			int yValueIndex)
		{
			return point.YValues[yValueIndex];
		}

		#endregion

		#region SmartLabelStyle methods

		/// <summary>
		/// Adds markers position to the list. Used to check SmartLabelStyle overlapping.
		/// </summary>
		/// <param name="common">Common chart elements.</param>
		/// <param name="area">Chart area.</param>
		/// <param name="series">Series values to be used.</param>
		/// <param name="list">List to add to.</param>
		public void AddSmartLabelMarkerPositions(CommonElements common, ChartArea area, Series series, ArrayList list)		
		{
			// Fast Line chart type do not support labels
		}

		#endregion

        #region IDisposable interface implementation
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            //Nothing to dispose at the base class. 
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
