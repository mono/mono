//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		KagiChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	Provides 2D and 3D drawing and hit testing of the 
//              Kagi chart.
//
//  Purpose:	
//	
//	Kagi Chart Overview:
//	--------------------
//
//	Kagi charts are believed to have been created around the time 
//	that the Japanese stock market began trading in the 1870's. Kagi 
//	charts display a series of connecting vertical lines where the 
//	thickness and direction of the lines are dependent on the action 
//	of the price value. These charts ignore the passage of time, but 
//	can be used to illustrate the forces of supply and demand on a 
//	security.
//	
//	When working with this type of chart, the following should be 
//	taken into account:
//	
//	- The X values of data points are automatically indexed.  
//	
//	- There is a formula applied to the original data before plotting, 
//	which changes the number of points and their X/Y values. 
//	
//	- Due to the data being recalculated we do not recommend setting 
//	the minimum and/or maximum values for the X axis, since it cannot 
//	be determined how many data points will actually get plotted. 
//	However, if the axis' maximum or minimum is set then the Maximum 
//	or Minimum properties should use data point index values. 
//	
//	- Data point anchoring, used for annotations, is not supported 
//	with this type of chart.
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
using System.ComponentModel.Design;
using System.Globalization;

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
    /// KagiChart class provides 2D and 3D drawing and hit testing of
    /// the Kagi chart.
    /// </summary>
	internal class KagiChart : StepLineChart
	{
		#region Fields

		// Color used to draw up direction lines
		internal	Color	kagiUpColor = Color.Empty;

		// Current properties used for kagi line (1 up; -1 down; 0 none)
		internal	int		currentKagiDirection = 0;

		#endregion // Fields

		#region Methods

		/// <summary>
		/// Prepares Kagi chart type for rendering.
		/// </summary>
		/// <param name="series">Series to be prepared.</param>
		internal static void PrepareData(Series series)
		{
			// Check series chart type
            if (String.Compare(series.ChartTypeName, ChartTypeNames.Kagi, StringComparison.OrdinalIgnoreCase) != 0 || !series.IsVisible())
			{
				return;
			}

			// Get reference to the chart control
			Chart	chart = series.Chart;
			if(chart == null)
			{
				throw(new InvalidOperationException(SR.ExceptionKagiNullReference));
			}

            // Kagi chart may not be combined with any other chart types
            ChartArea	area = chart.ChartAreas[series.ChartArea];
            foreach (Series currentSeries in chart.Series)
            {
                if (currentSeries.IsVisible() && currentSeries != series && area == chart.ChartAreas[currentSeries.ChartArea])
                {
                    throw (new InvalidOperationException(SR.ExceptionKagiCanNotCombine));
                }
            }

			// Create a temp series which will hold original series data points
			string tempSeriesName = "KAGI_ORIGINAL_DATA_" + series.Name;
			if (chart.Series.IndexOf(tempSeriesName) != -1)
			{
				return; // the temp series has already been added
			}
			Series seriesOriginalData = new Series(tempSeriesName, series.YValuesPerPoint);
			seriesOriginalData.Enabled = false;
			seriesOriginalData.IsVisibleInLegend = false;
			chart.Series.Add(seriesOriginalData);
			foreach(DataPoint dp in series.Points)
			{
				seriesOriginalData.Points.Add(dp);
			}
			series.Points.Clear();
			if(series.IsCustomPropertySet("TempDesignData"))
			{
				seriesOriginalData["TempDesignData"] = "true";
			}


			// Remember prev. series parameters
            series["OldXValueIndexed"] = series.IsXValueIndexed.ToString(CultureInfo.InvariantCulture);
            series["OldYValuesPerPoint"] = series.YValuesPerPoint.ToString(CultureInfo.InvariantCulture);
			series.IsXValueIndexed = true;

			// Calculate date-time interval for indexed series
			if(series.ChartArea.Length > 0 &&
				series.IsXValueDateTime())
			{
				// Get X axis connected to the series
				Axis		xAxis = area.GetAxis(AxisName.X, series.XAxisType, series.XSubAxisName);

				// Change interval for auto-calculated interval only
				if(xAxis.Interval == 0 && xAxis.IntervalType == DateTimeIntervalType.Auto)
				{
					// Check if original data has X values set to date-time values and
					// calculate min/max X values.
					bool	nonZeroXValues = false;
					double	minX = double.MaxValue;
					double	maxX = double.MinValue;
					foreach(DataPoint dp in seriesOriginalData.Points)
					{
						if(!dp.IsEmpty)
						{
							if(dp.XValue != 0.0)
							{
								nonZeroXValues = true;
							}
							if(dp.XValue > maxX)
							{
								maxX = dp.XValue;
							}
							if(dp.XValue < minX)
							{
								minX = dp.XValue;
							}
						}
					}

					if(nonZeroXValues)
					{
						// Save flag that axis interval is automatic
						series["OldAutomaticXAxisInterval"] = "true";

						// Calculate and set axis date-time interval
						DateTimeIntervalType	intervalType = DateTimeIntervalType.Auto;
						xAxis.interval = xAxis.CalcInterval(minX, maxX, true, out intervalType, series.XValueType);
						xAxis.intervalType = intervalType;
					}
				}
			}

			// Calculate Kagi bricks data points values
			FillKagiData(series, seriesOriginalData);
		}

		/// <summary>
		/// Remove any changes done while preparing Kagi chart type for rendering.
		/// </summary>
		/// <param name="series">Series to be un-prepared.</param>
		/// <returns>True if series was removed from collection.</returns>
		internal static bool UnPrepareData(Series series)
		{
			if(series.Name.StartsWith("KAGI_ORIGINAL_DATA_", StringComparison.Ordinal))
			{
				// Get reference to the chart control
				Chart	chart = series.Chart;
				if(chart == null)
				{
                    throw (new InvalidOperationException(SR.ExceptionKagiNullReference));
				}

				// Get original Kagi series
				Series	kagiSeries = chart.Series[series.Name.Substring(19)];
                Series.MovePositionMarkers(kagiSeries, series);
                // Copy data back to original Kagi series
				kagiSeries.Points.Clear();
				if(!series.IsCustomPropertySet("TempDesignData"))
				{
					foreach(DataPoint dp in series.Points)
					{
						kagiSeries.Points.Add(dp);
					}
				}

				// Restore series properties
                bool xValIndexed;
                bool parseSucceed = bool.TryParse(kagiSeries["OldXValueIndexed"], out xValIndexed);
                kagiSeries.IsXValueIndexed = parseSucceed && xValIndexed;

                int yValPerPoint;
                parseSucceed = int.TryParse(kagiSeries["OldYValuesPerPoint"], NumberStyles.Any, CultureInfo.InvariantCulture, out yValPerPoint);
                if (parseSucceed)
                {
                    kagiSeries.YValuesPerPoint = yValPerPoint;
                }

				kagiSeries.DeleteCustomProperty("OldXValueIndexed");
				kagiSeries.DeleteCustomProperty("OldYValuesPerPoint");

				series["OldAutomaticXAxisInterval"] = "true";
				if(kagiSeries.IsCustomPropertySet("OldAutomaticXAxisInterval"))
				{
					kagiSeries.DeleteCustomProperty("OldAutomaticXAxisInterval");

					// Reset automatic interval for X axis
					if(kagiSeries.ChartArea.Length > 0)
					{
						// Get X axis connected to the series
						ChartArea	area = chart.ChartAreas[kagiSeries.ChartArea];
						Axis		xAxis = area.GetAxis(AxisName.X, kagiSeries.XAxisType, kagiSeries.XSubAxisName);

						xAxis.interval = 0.0;
						xAxis.intervalType = DateTimeIntervalType.Auto;
					}
				}

				// Remove series from the collection
				chart.Series.Remove(series);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets reversal amount of the kagi chart.
		/// </summary>
		/// <param name="series">Step line chart series used to dispaly the kagi chart.</param>
		/// <param name="percentOfPrice">Returns reversal amount in percentage.</param>
		private static double GetReversalAmount(Series series, out double percentOfPrice)
		{
			// Check "ReversalAmount" custom attribute
			double	reversalAmount = 1.0;
			percentOfPrice = 3.0;
            if (series.IsCustomPropertySet(CustomPropertyName.ReversalAmount))
            {
                string attrValue = series[CustomPropertyName.ReversalAmount].Trim();
                bool usePercentage = attrValue.EndsWith("%", StringComparison.Ordinal);
                if (usePercentage)
                {
                    attrValue = attrValue.Substring(0, attrValue.Length - 1);
                }

                if (usePercentage)
                {
                    double percent;
                    bool parseSucceed = double.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out percent);
                    if (parseSucceed)
                    {
                        percentOfPrice = percent;
                    }
                    else
                    {
                        throw (new InvalidOperationException(SR.ExceptionKagiAttributeFormatInvalid("ReversalAmount")));
                    }
                }
                else
                {
                    double amount;
                    bool parseSucceed = double.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);
                    if (parseSucceed)
                    {
                        reversalAmount = amount;
                        percentOfPrice = 0.0;
                    }
                    else
                    {
                        throw (new InvalidOperationException(SR.ExceptionKagiAttributeFormatInvalid("ReversalAmount")));
                    }
                }

            }

			return reversalAmount;
		}


		/// <summary>
		/// Fills step line series with data to draw the Kagi chart.
		/// </summary>
		/// <param name="series">Step line chart series used to dispaly the Kagi chart.</param>
		/// <param name="originalData">Series with original data.</param>
		private static void FillKagiData(Series series, Series originalData)
		{
			// Get index of the Y values used
			int	yValueIndex = 0;
            if (series.IsCustomPropertySet(CustomPropertyName.UsedYValue))
            {
                int yi;
                bool parseSucceed = int.TryParse(series[CustomPropertyName.UsedYValue], NumberStyles.Any, CultureInfo.InvariantCulture, out yi);

                if (parseSucceed)
                {
                    yValueIndex = yi;
                }
                else
                {
                    throw (new InvalidOperationException(SR.ExceptionKagiAttributeFormatInvalid("UsedYValue")));
                }

                if (yValueIndex >= series.YValuesPerPoint)
                {
                    throw (new InvalidOperationException(SR.ExceptionKagiAttributeOutOfRange("UsedYValue")));
                }
            }

			// Calculate reversal amount
			double	reversalAmountPercentage = 0.0;
			double	reversalAmount = GetReversalAmount(series, out reversalAmountPercentage);

			// Fill points
			double	prevClose = double.NaN;
			int		prevDirection = 0;	// 1 up; -1 down; 0 none
			int		pointIndex = 0;
			foreach(DataPoint dataPoint in originalData.Points)
			{
				// Check if previus values exists
				if(double.IsNaN(prevClose))
				{
					prevClose = dataPoint.YValues[yValueIndex];

					// Add first point
					DataPoint newDataPoint = (DataPoint)dataPoint.Clone();
                    newDataPoint["OriginalPointIndex"] = pointIndex.ToString(CultureInfo.InvariantCulture);
					newDataPoint.series = series;
					newDataPoint.XValue = dataPoint.XValue;
					newDataPoint.YValues[0] = dataPoint.YValues[yValueIndex];
                    newDataPoint.Tag = dataPoint;
					series.Points.Add(newDataPoint);
					++pointIndex;
					continue;
				}

				// Calculate reversal amount as percentage of previous price
				if(reversalAmountPercentage != 0.0)
				{
					reversalAmount = (prevClose / 100.0) * reversalAmountPercentage;
				}

				// Check direction of the price change
				int direction = 0;
				if(dataPoint.YValues[yValueIndex] > prevClose)
				{
					direction = 1;
				}
				else if(dataPoint.YValues[yValueIndex] < prevClose)
				{
					direction = -1;
				}
				else
				{
					direction = 0;
				}

				// Check if value was changed - otherwise do nothing
				if(direction != 0)
				{
					// Extend line in same direction
					if(direction == prevDirection)
					{
						series.Points[series.Points.Count - 1].YValues[0] = 
							dataPoint.YValues[yValueIndex];
						series.Points[series.Points.Count - 1]["OriginalPointIndex"] = pointIndex.ToString(CultureInfo.InvariantCulture);
                        series.Points[series.Points.Count - 1].Tag = dataPoint;
					}
					else if( Math.Abs(dataPoint.YValues[yValueIndex] - prevClose) < reversalAmount)
					{
						// Ignore opposite direction change if value is less than reversal amount
						++pointIndex;
						continue;
					}
					else
					{
						// Opposite direction by more than reversal amount
						DataPoint newDataPoint = (DataPoint)dataPoint.Clone();
                        newDataPoint["OriginalPointIndex"] = pointIndex.ToString(CultureInfo.InvariantCulture);
						newDataPoint.series = series;
						newDataPoint.XValue = dataPoint.XValue;
						newDataPoint.YValues[0] = dataPoint.YValues[yValueIndex];
                        newDataPoint.Tag = dataPoint;
                        // Add Kagi to the range step line series
						series.Points.Add(newDataPoint);
					}

					// Save previous close value and direction
					prevClose = dataPoint.YValues[yValueIndex];
					prevDirection = direction;
				}
				++pointIndex;
			}
		}

		#endregion // Methods
		
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
			
			if(currentKagiDirection == 0)
			{
				// Get up price color
				this.kagiUpColor = ChartGraphics.GetGradientColor(series.Color, Color.Black, 0.5);
				string	priceUpColorString = series[CustomPropertyName.PriceUpColor];
				ColorConverter colorConverter = new ColorConverter();
				if(priceUpColorString != null)
				{
					try
					{
						this.kagiUpColor = (Color)colorConverter.ConvertFromString(null, CultureInfo.InvariantCulture, priceUpColorString);
					}
					catch
					{
						throw(new InvalidOperationException(SR.ExceptionKagiAttributeFormatInvalid("Up Brick color")));
					}
				}

				// Check direction of first line (up or down)
				currentKagiDirection = (points[pointIndex - 1].Y > points[pointIndex].Y) ? 
					1 : -1;
			}

			// Set up movement colors and width
			Color	lineColor = (currentKagiDirection == 1) ? this.kagiUpColor : point.Color;

			// Prepare coordinate to draw 2 or 3 segments of the step line
			PointF	point1 = points[pointIndex - 1];
			PointF	point2 = new PointF(points[pointIndex].X, points[pointIndex - 1].Y);
			PointF	point3 = points[pointIndex];
			PointF	point4 = PointF.Empty;
			
			// Check if vertical line should be draw as to segments of different color
			if(pointIndex >= 2)
			{
				// Check current direction
				int direction = (points[pointIndex - 1].Y > points[pointIndex].Y) ? 
					1 : -1;

				// Proceed only when direction is changed
				if(direction != currentKagiDirection)
				{
					// Find prev line low & high
					PointF	prevPoint = points[pointIndex - 2];
					bool	twoVertSegments = false;
					if(point1.Y > prevPoint.Y &&
						point1.Y > point3.Y &&
						prevPoint.Y > point3.Y)
					{
						twoVertSegments = true;
					}
					else if(point1.Y < prevPoint.Y &&
						point1.Y < point3.Y &&
						prevPoint.Y < point3.Y)
					{
						twoVertSegments = true;
					}

					if(twoVertSegments)
					{
						// Calculate point where vertical line is split
						point4.Y = prevPoint.Y;
						point4.X = point2.X;
					}
				}
			}

			// Round line point values
			point1.X = (float)Math.Round(point1.X);
			point1.Y = (float)Math.Round(point1.Y);
			point2.X = (float)Math.Round(point2.X);
			point2.Y = (float)Math.Round(point2.Y);
			point3.X = (float)Math.Round(point3.X);
			point3.Y = (float)Math.Round(point3.Y);
			if(!point4.IsEmpty)
			{
				point4.X = (float)Math.Round(point4.X);
				point4.Y = (float)Math.Round(point4.Y);
			}


			// Draw horizontal segment
			graph.DrawLineRel( lineColor, point.BorderWidth, point.BorderDashStyle, 
				graph.GetRelativePoint(point1), graph.GetRelativePoint(point2), 
				series.ShadowColor, series.ShadowOffset );
			
			// Check if vertical segment should be drawn as one ore two segments
			if(point4.IsEmpty)
			{
				// Draw 1 vertical segment
				graph.DrawLineRel( lineColor, point.BorderWidth, point.BorderDashStyle, 
					graph.GetRelativePoint(point2), graph.GetRelativePoint(point3), 
					series.ShadowColor, series.ShadowOffset );
			}
			else
			{
				// Draw firts part of vertical segment
				graph.DrawLineRel( lineColor, point.BorderWidth, point.BorderDashStyle, 
					graph.GetRelativePoint(point2), graph.GetRelativePoint(point4), 
					series.ShadowColor, series.ShadowOffset );

				// Change direction 
				currentKagiDirection = (currentKagiDirection == 1) ? -1 : 1;

				// Set color
				lineColor = (currentKagiDirection == 1) ? this.kagiUpColor : point.Color;

				// Draw second part of vertical segment
				graph.DrawLineRel( lineColor, point.BorderWidth, point.BorderDashStyle, 
					graph.GetRelativePoint(point4), graph.GetRelativePoint(point3), 
					series.ShadowColor, series.ShadowOffset );
			}

			if( common.ProcessModeRegions )
			{
				// Create grapics path object dor the curve
                using (GraphicsPath path = new GraphicsPath())
                {
                    try
                    {
                        path.AddLine(point1, point2);
                        path.AddLine(point2, point3);
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
                    PointF pointNew = PointF.Empty;
                    float[] coord = new float[path.PointCount * 2];
                    PointF[] pathPoints = path.PathPoints;
                    for (int i = 0; i < path.PointCount; i++)
                    {
                        pointNew = graph.GetRelativePoint(pathPoints[i]);
                        coord[2 * i] = pointNew.X;
                        coord[2 * i + 1] = pointNew.Y;
                    }

                    common.HotRegionsList.AddHotRegion(
                        path,
                        false,
                        coord,
                        point,
                        series.Name,
                        pointIndex);
                }
			}

		}
		
		/// <summary>
		/// Fills a PointF array of data points absolute pixel positions.
		/// </summary>
		/// <param name="graph">Graphics object.</param>
		/// <param name="series">Point series.</param>
		/// <param name="indexedSeries">Indicate that point index should be used as X value.</param>
		/// <returns>Array of data points position.</returns>
		override protected PointF[] GetPointsPosition(ChartGraphics graph, Series series, bool indexedSeries)
		{
			PointF[]	pointPos = new PointF[series.Points.Count];
			int index = 0;
			foreach( DataPoint point in series.Points )
			{
				// Change Y value if line is out of plot area
				double yValue = GetYValue(Common, Area, series, point, index, this.YValueIndex);

				// Recalculates y position
				double yPosition = VAxis.GetPosition( yValue );

				// Recalculates x position
				double xPosition = HAxis.GetPosition( point.XValue );
				if( indexedSeries )
				{
					xPosition = HAxis.GetPosition( index + 1 );
				}
								
				// Add point position into array
				pointPos[index] = new PointF(
                    (float)(xPosition * (graph.Common.ChartPicture.Width - 1) / 100F),
                    (float)(yPosition * (graph.Common.ChartPicture.Height - 1) / 100F)); 

				index++;
			}

			return pointPos;
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

			// Set up kagi chart
			if(currentKagiDirection == 0)
			{
				// Get up price color
				this.kagiUpColor = secondPoint.dataPoint.series.Color;
				string	priceUpColorString = secondPoint.dataPoint.series[CustomPropertyName.PriceUpColor];
				ColorConverter colorConverter = new ColorConverter();
				if(priceUpColorString != null)
				{
					try
					{
						this.kagiUpColor = (Color)colorConverter.ConvertFromString(null, CultureInfo.InvariantCulture, priceUpColorString);
					}
					catch
					{
						throw(new InvalidOperationException(SR.ExceptionKagiAttributeFormatInvalid("Up Brick color")));
					}
				}

				// Check direction of first line (up or down)
				currentKagiDirection = (firstPoint.yPosition > secondPoint.yPosition) ? 
					1 : -1;
			}

			// Set up movement colors and width
			Color	lineColor = (currentKagiDirection == 1) ? this.kagiUpColor : color;

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

	
			// Check if vertical line should be draw as to segments of different color
			DataPoint3D	vertSplitPoint = null;
			if(secondPoint.index >= 3) //Point3D.index is 1 based
			{
				// Check current direction
				int direction = (firstPoint.yPosition > secondPoint.yPosition) ? 
					1 : -1;

				// Proceed only when direction is changed
				if(direction != currentKagiDirection)
				{
					// Find prev line low & high
					DataPoint3D prevPoint = ChartGraphics.FindPointByIndex(
						points, 
						secondPoint.index - 2, 
						(this.multiSeries) ? secondPoint : null, 
						ref pointArrayIndex);

					bool	twoVertSegments = false;
					if(firstPoint.yPosition > prevPoint.yPosition &&
						firstPoint.yPosition > secondPoint.yPosition &&
						prevPoint.yPosition > secondPoint.yPosition)
					{
						twoVertSegments = true;
					}
					else if(firstPoint.yPosition < prevPoint.yPosition &&
						firstPoint.yPosition < secondPoint.yPosition &&
						prevPoint.yPosition < secondPoint.yPosition)
					{
						twoVertSegments = true;
					}

					if(twoVertSegments)
					{
						vertSplitPoint = new DataPoint3D();
						vertSplitPoint.xPosition = secondPoint.xPosition;
						vertSplitPoint.yPosition = prevPoint.yPosition;
						vertSplitPoint.dataPoint = secondPoint.dataPoint;
					}
				}
			}

			// Draw two or three segments of the step line
			GraphicsPath[] resultPathLine = new GraphicsPath[3];
			for(int segmentIndex = 0; segmentIndex < 2; segmentIndex++)
			{
				DataPoint3D	point1 = firstPoint, point2 = secondPoint;
				LineSegmentType	lineSegmentType = LineSegmentType.First;

				if(segmentIndex == 0)
				{
					lineSegmentType = (originalDrawOrder) ? LineSegmentType.First : LineSegmentType.Last;
					middlePoint.dataPoint = (originalDrawOrder) ? secondPoint.dataPoint : firstPoint.dataPoint;
					point1 = (originalDrawOrder) ? firstPoint : middlePoint;
					point2 = (originalDrawOrder) ? middlePoint : secondPoint;
				}
				else if(segmentIndex == 1)
				{
					lineSegmentType = (!originalDrawOrder) ? LineSegmentType.First : LineSegmentType.Last;
					middlePoint.dataPoint = (!originalDrawOrder) ? secondPoint.dataPoint : secondPoint.dataPoint;
					point1 = (!originalDrawOrder) ? firstPoint : middlePoint;
					point2 = (!originalDrawOrder) ? middlePoint : secondPoint;
				}

				// Draw horizontal surface
				if(lineSegmentType == LineSegmentType.First ||
					vertSplitPoint == null)
				{
					resultPathLine[segmentIndex] = new GraphicsPath();
					resultPathLine[segmentIndex] = graph.Draw3DSurface( 
						area, matrix, lightStyle, SurfaceNames.Top, positionZ, depth, lineColor, 
						pointAttr.dataPoint.BorderColor, pointAttr.dataPoint.BorderWidth, dashStyle, 
						point1, point2, 
						points, pointIndex, 0f, operationType, lineSegmentType, 
						(this.showPointLines) ? true : false, false,
                        area.ReverseSeriesOrder,
						this.multiSeries, 0, true);
				}
				else
				{
					if(!originalDrawOrder)
					{
						lineColor = (currentKagiDirection == -1) ? this.kagiUpColor : color;
					}

					// Draw verticla line as two segments
					resultPathLine[segmentIndex] = new GraphicsPath();
					resultPathLine[segmentIndex] = graph.Draw3DSurface( 
						area, matrix, lightStyle, SurfaceNames.Top, positionZ, depth, lineColor, 
						pointAttr.dataPoint.BorderColor, pointAttr.dataPoint.BorderWidth, dashStyle, 
						point1, vertSplitPoint, 
						points, pointIndex, 0f, operationType, LineSegmentType.Middle, 
						(this.showPointLines) ? true : false, false,
                        area.ReverseSeriesOrder,
						this.multiSeries, 0, true);

					// No second draw of the prev. front line required
					graph.frontLinePen = null;

					// Change direction 
					currentKagiDirection = (currentKagiDirection == 1) ? -1 : 1;

					// Set color
					if(originalDrawOrder)
					{
						lineColor = (currentKagiDirection == 1) ? this.kagiUpColor : color;
					}
					else
					{
						lineColor = (currentKagiDirection == -1) ? this.kagiUpColor : color;
					}

					resultPathLine[2] = new GraphicsPath();
					resultPathLine[2] = graph.Draw3DSurface( 
						area, matrix, lightStyle, SurfaceNames.Top, positionZ, depth, lineColor, 
						pointAttr.dataPoint.BorderColor, pointAttr.dataPoint.BorderWidth, dashStyle, 
						vertSplitPoint, point2, 
						points, pointIndex, 0f, operationType, lineSegmentType, 
						(this.showPointLines) ? true : false, false,
                        area.ReverseSeriesOrder,
						this.multiSeries, 0, true);

					if(!originalDrawOrder)
					{
						lineColor = (currentKagiDirection == 1) ? this.kagiUpColor : color;
					}

				}

				// No second draw of the prev. front line required
				graph.frontLinePen = null;
			}

			if(resultPath != null)
			{
				if(resultPathLine[0] != null)
					resultPath.AddPath(resultPathLine[0], true);
				if(resultPathLine[1] != null)
					resultPath.AddPath(resultPathLine[1], true);
				if(resultPathLine[2] != null)
					resultPath.AddPath(resultPathLine[2], true);
			}
			return resultPath;
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		override public string Name			{ get{ return ChartTypeNames.Kagi;}}

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

		#region Painting and selection methods

		/// <summary>
		/// Paint Line Chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this char.t</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		public override void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
		{	
			// Reset current direction
			this.currentKagiDirection = 0;

			// Call base class methods
			base.Paint(graph, common, area, seriesToDraw);
		}

		#endregion	// Painting and selection methods
	}
}

