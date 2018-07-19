//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		PointAndFigureChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	PointAndFigureChart
//
//  Purpose:	Point and Figure chart type do not plot series data 
//              point directly as most of the other chart types. 
//              Instead it uses different calculation to create a 
//              new RangeColumn type series based on its data in 
//              the PrepareData method. All the changes in this 
//              method are reversed back in the UnPrepareData 
//              method. RangeColumn chart type is extended to 
//              display a column of Os or Xs.
//	
//	Point and Figure Charts Overview:
//  ---------------------------------
//	
//	Point and Figure charts differ from traditional price charts in 
//	that they completely disregard the passage of time, and only 
//	display changes in prices. Rather than having price on the y-axis, 
//	and time on the x-axis, Point and Figure charts display price 
//	changes on both axes. This is similar to the Kagi, Renko, and 
//	Three Line Break charts.
//	
//	The Point and Figure chart displays the underlying supply and 
//	demand as reflected in the price values. A column of Xs shows 
//	that demand is exceeding supply, which is known as a rally, 
//	a column of Os shows that supply is exceeding demand, which is 
//	known as a decline, and a series of short columns shows that 
//	supply and demand are relatively equal, which of course, 
//	represents a market equilibrium.
//	
//	The following should be taken into account when working with 
//	this type of chart:
//	
//	- The X values of data points are automatically indexed. For 
//	more information see the topic on Indexing Data Point X Values. 
//	
//	- There is a formula applied to the original data before it gets 
//	plotted. This formula changes the number of points, as well as 
//	their X/Y values. 
//	
//	- Due to the data being recalculated, we do not recommend setting 
//	the minimum, or maximum values for the X axis. This is because it 
//	cannot be determined how many data points will actually be plotted. 
//	However,  if the axis' Maximum, or Minimum is set, then the Maximum, 
//	and Minimum properties will use data point index values. 
//	
//	- Data point anchoring, used for annotations, is not supported 
//	with this type of chart. 
//	
//	Reviewed:   AG - Microsoft 6, 2007
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
    /// PointAndFigureChart class contains all the code necessary for calculation
    /// and drawing Point and Figure chart.
	/// </summary>
	internal class PointAndFigureChart : RangeColumnChart
	{
		#region Fields

		/// <summary>
		/// Indicates that class subscribed fro the customize event.
		/// </summary>
		static private	bool	_customizeSubscribed = false;

		#endregion // Fields

		#region Methods

		/// <summary>
		/// Prepares PointAndFigure chart type for rendering. We hide original series
        /// during rendering and only using the data for calculations. New RangeColumn
        /// type series is added wich displayes the columns of Os or Xs.
        /// All the changes in this method are reversed back in the UnPrepareData method. 
		/// </summary>
		/// <param name="series">Series to be prepared.</param>
		internal static void PrepareData(Series series)
		{
			// Check series chart type
			if(String.Compare( series.ChartTypeName, ChartTypeNames.PointAndFigure, StringComparison.OrdinalIgnoreCase ) != 0 || !series.IsVisible())
			{
				return;
			}

			// Get reference to the chart control
			Chart	chart = series.Chart;
			if(chart == null)
			{
                throw (new InvalidOperationException(SR.ExceptionPointAndFigureNullReference));
			}

            // PointAndFigure chart may not be combined with any other chart types
            ChartArea area = chart.ChartAreas[series.ChartArea];
            foreach (Series currentSeries in chart.Series)
            {
                if (currentSeries.IsVisible() && currentSeries != series && area == chart.ChartAreas[currentSeries.ChartArea])
                {
                    throw (new InvalidOperationException(SR.ExceptionPointAndFigureCanNotCombine));
                }
            }

			// Subscribe for customize event
			if(!_customizeSubscribed)
			{
				_customizeSubscribed = true;
				chart.Customize += new EventHandler(OnCustomize);
			}

			// Create a temp series which will hold original series data points
			string tempSeriesName = "POINTANDFIGURE_ORIGINAL_DATA_" + series.Name;
			if (chart.Series.IndexOf(tempSeriesName) != -1)
			{
				return; // the temp series has already been added
			}
			Series seriesOriginalData = new Series(tempSeriesName, series.YValuesPerPoint);
			seriesOriginalData.Enabled = false;
			seriesOriginalData.IsVisibleInLegend = false;
			seriesOriginalData.YValuesPerPoint = series.YValuesPerPoint;
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

			// Calculate PointAndFigure bricks data points values
			FillPointAndFigureData(series, seriesOriginalData);
		}

		/// <summary>
		/// Remove any changes done while preparing PointAndFigure chart type for rendering.
		/// </summary>
		/// <param name="series">Series to be un-prepared.</param>
		/// <returns>True if series was removed from collection.</returns>
		internal static bool UnPrepareData(Series series)
		{
			if(series.Name.StartsWith("POINTANDFIGURE_ORIGINAL_DATA_", StringComparison.Ordinal))
			{
				// Get reference to the chart control
				Chart	chart = series.Chart;
				if(chart == null)
				{
                    throw (new InvalidOperationException(SR.ExceptionPointAndFigureNullReference));
				}

				// Unsubscribe for customize event
				if(_customizeSubscribed)
				{
					_customizeSubscribed = false;
					chart.Customize -= new EventHandler(OnCustomize);
				}

				// Get original PointAndFigure series
				Series	pointAndFigureSeries = chart.Series[series.Name.Substring(29)];
                Series.MovePositionMarkers(pointAndFigureSeries, series);

				// Copy data back to original PointAndFigure series
				pointAndFigureSeries.Points.Clear();
				if(!series.IsCustomPropertySet("TempDesignData"))
				{
					foreach(DataPoint dp in series.Points)
					{
						pointAndFigureSeries.Points.Add(dp);
					}
				}

				// Restore series properties
                bool xValIndexed;
                bool parseSucceed = bool.TryParse(pointAndFigureSeries["OldXValueIndexed"], out xValIndexed);

                pointAndFigureSeries.IsXValueIndexed = parseSucceed && xValIndexed;

                int yVals;
                parseSucceed = int.TryParse(pointAndFigureSeries["OldYValuesPerPoint"], NumberStyles.Any, CultureInfo.InvariantCulture, out yVals);

                if (parseSucceed)
                {
                    pointAndFigureSeries.YValuesPerPoint = yVals;
                }

				pointAndFigureSeries.DeleteCustomProperty("OldXValueIndexed");
				pointAndFigureSeries.DeleteCustomProperty("OldYValuesPerPoint");
				pointAndFigureSeries.DeleteCustomProperty(CustomPropertyName.EmptyPointValue);

				series["OldAutomaticXAxisInterval"] = "true";
				if(pointAndFigureSeries.IsCustomPropertySet("OldAutomaticXAxisInterval"))
				{
					pointAndFigureSeries.DeleteCustomProperty("OldAutomaticXAxisInterval");

					// Reset automatic interval for X axis
					if(pointAndFigureSeries.ChartArea.Length > 0)
					{
						// Get X axis connected to the series
						ChartArea	area = chart.ChartAreas[pointAndFigureSeries.ChartArea];
						Axis		xAxis = area.GetAxis(AxisName.X, pointAndFigureSeries.XAxisType, pointAndFigureSeries.XSubAxisName);

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
		/// Gets price range in the point and figure chart.
		/// </summary>
		/// <param name="originalData">Series with original data.</param>
		/// <param name="yValueHighIndex">Index of the Y value to use as High price.</param>
		/// <param name="yValueLowIndex">Index of the Y value to use as Low price.</param>
		/// <param name="minPrice">Returns max price.</param>
		/// <param name="maxPrice">Returns min price.</param>
		private static void GetPriceRange(
			Series originalData, 
			int yValueHighIndex, 
			int yValueLowIndex, 
			out double minPrice,
			out double maxPrice)
		{
			// Calculate percent of the highest and lowest price difference.
			maxPrice = double.MinValue;
			minPrice = double.MaxValue;
			foreach(DataPoint dp in originalData.Points)
			{
				if(!dp.IsEmpty)
				{
					// Check required Y values number
					if(dp.YValues.Length < 2)
					{
						throw(new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues(ChartTypeNames.PointAndFigure, ((int)(2)).ToString(CultureInfo.CurrentCulture))));
					}

					if(dp.YValues[yValueHighIndex] > maxPrice)
					{
						maxPrice = dp.YValues[yValueHighIndex];
					}
					else if(dp.YValues[yValueLowIndex] > maxPrice)
					{
						maxPrice = dp.YValues[yValueLowIndex];
					}

					if(dp.YValues[yValueHighIndex] < minPrice)
					{
						minPrice = dp.YValues[yValueHighIndex];
					}
					else if(dp.YValues[yValueLowIndex] < minPrice)
					{
						minPrice = dp.YValues[yValueLowIndex];
					}
				}
			}
		}

		/// <summary>
		/// Gets box size of the renko chart.
		/// </summary>
		/// <param name="series">Range column chart series used to dispaly the renko chart.</param>
		/// <param name="minPrice">Max price.</param>
		/// <param name="maxPrice">Min price.</param>
		private static double GetBoxSize(
			Series series, 
			double minPrice,
			double maxPrice)
		{
			// Check "BoxSize" custom attribute
			double	boxSize = 1.0;
			double	percentOfPriceRange = 4.0;
			bool	roundBoxSize = true;
            if (series.IsCustomPropertySet(CustomPropertyName.BoxSize))
            {
                string attrValue = series[CustomPropertyName.BoxSize].Trim();
                bool usePercentage = attrValue.EndsWith("%", StringComparison.Ordinal);
                if (usePercentage)
                {
                    attrValue = attrValue.Substring(0, attrValue.Length - 1);
                }

                bool parseSucceed = false;
                if (usePercentage)
                {
                    double percent;
                    parseSucceed = double.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out percent);
                    if (parseSucceed)
                    {
                        percentOfPriceRange = percent;
                        roundBoxSize = false;
                    }
                }
                else
                {
                    double b = 0;
                    parseSucceed = double.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out b);
                    if (parseSucceed)
                    {
                        boxSize = b;
                        percentOfPriceRange = 0.0;
                    }
                }
                if (!parseSucceed)
                {
                    throw (new InvalidOperationException(SR.ExceptionRenkoBoxSizeFormatInvalid));
                }
            }

			// Calculate box size using the percentage of price range
			if(percentOfPriceRange > 0.0)
			{
				// Set default box size
				boxSize = 1.0;

				// Calculate box size as percentage of price difference
				if(minPrice == maxPrice)
				{
					boxSize = 1.0;
				}
				else if( (maxPrice - minPrice) < 0.000001)
				{
					boxSize = 0.000001;
				}
				else
				{
					boxSize = (maxPrice - minPrice) * (percentOfPriceRange / 100.0);
				}


				// Round calculated value
				if(roundBoxSize)
				{

					double[] availableBoxSizes = new double[] 
						{ 0.000001, 0.00001, 0.0001, 0.001, 0.01, 0.1, 0.25, 0.5, 1.0, 2.0, 2.5, 3.0, 4.0, 5.0, 7.5, 10.0, 15.0, 20.0, 25.0, 50.0, 100.0, 200.0, 500.0, 1000.0, 5000.0, 10000.0, 50000.0, 100000.0, 1000000.0, 1000000.0};

					for(int index = 1; index < availableBoxSizes.Length; index ++)
					{
						if(boxSize > availableBoxSizes[index - 1] &&
							boxSize < availableBoxSizes[index])
						{
							boxSize = availableBoxSizes[index];
						}
					}
				}
			}

			// Save current box size as a custom attribute of the original series
			series["CurrentBoxSize"] = boxSize.ToString(CultureInfo.InvariantCulture);

			return boxSize;
		}

		/// <summary>
		/// Gets reversal amount of the pointAndFigure chart.
		/// </summary>
		/// <param name="series">Step line chart series used to dispaly the pointAndFigure chart.</param>
		private static double GetReversalAmount(
			Series series)
		{
			// Check "ReversalAmount" custom attribute
			double	reversalAmount = 3.0;
            if (series.IsCustomPropertySet(CustomPropertyName.ReversalAmount))
            {
                string attrValue = series[CustomPropertyName.ReversalAmount].Trim();

                double amount;
                bool parseSucceed = double.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);
                if (parseSucceed)
                {
                    reversalAmount = amount;
                }
                else
                {
                    throw (new InvalidOperationException(SR.ExceptionPointAndFigureReversalAmountInvalidFormat));
                }
            }

			return reversalAmount;
		}


		/// <summary>
		/// Fills step line series with data to draw the PointAndFigure chart.
		/// </summary>
		/// <param name="series">Step line chart series used to dispaly the PointAndFigure chart.</param>
		/// <param name="originalData">Series with original data.</param>
		private static void FillPointAndFigureData(Series series, Series originalData)
		{
			// Get index of the Y values used for High/Low
			int	yValueHighIndex = 0;
			if(series.IsCustomPropertySet(CustomPropertyName.UsedYValueHigh))
			{
				try
				{

					yValueHighIndex = int.Parse(series[CustomPropertyName.UsedYValueHigh], CultureInfo.InvariantCulture);
				}
				catch
				{
                    throw (new InvalidOperationException(SR.ExceptionPointAndFigureUsedYValueHighInvalidFormat));
				}

				if(yValueHighIndex >= series.YValuesPerPoint)
				{
                    throw (new InvalidOperationException(SR.ExceptionPointAndFigureUsedYValueHighOutOfRange));
				}
			}
			int	yValueLowIndex = 1;
			if(series.IsCustomPropertySet(CustomPropertyName.UsedYValueLow))
			{
				try
				{
					yValueLowIndex = int.Parse(series[CustomPropertyName.UsedYValueLow], CultureInfo.InvariantCulture);
				}
				catch
				{
                    throw (new InvalidOperationException(SR.ExceptionPointAndFigureUsedYValueLowInvalidFormat));
				}

				if(yValueLowIndex >= series.YValuesPerPoint)
				{
                    throw (new InvalidOperationException(SR.ExceptionPointAndFigureUsedYValueLowOutOfrange));
				}
			}

			// Get Up Brick color
			Color	upPriceColor = ChartGraphics.GetGradientColor(series.Color, Color.Black, 0.5);
			string	upPriceColorString = series[CustomPropertyName.PriceUpColor];
			if(upPriceColorString != null)
			{
				try
				{
					ColorConverter colorConverter = new ColorConverter();
					upPriceColor = (Color)colorConverter.ConvertFromString(null, CultureInfo.InvariantCulture, upPriceColorString);
				}
				catch
				{
                    throw (new InvalidOperationException(SR.ExceptionPointAndFigureUpBrickColorInvalidFormat));
				}
			}

			// Get price range
			double	priceHigh, priceLow;
			GetPriceRange(originalData, yValueHighIndex, yValueLowIndex, out priceHigh, out priceLow);

			// Calculate box size
			double	boxSize = GetBoxSize(series, priceHigh, priceLow);

			// Calculate reversal amount
			double	reversalAmount = GetReversalAmount(series);

			// Fill points
			double	prevHigh = double.NaN;
			double	prevLow = double.NaN;
			int		prevDirection = 0;	// 1 up; -1 down; 0 none
			int		pointIndex = 0;
			foreach(DataPoint dataPoint in originalData.Points)
			{
				if(!dataPoint.IsEmpty)
				{
                    // Indicates that all updates are already performed and no further processing required
                    bool    doNotUpdate = false;    

                    // Number of brciks total or added to the curent column
					int		numberOfBricks = 0;

					// Check if previus values exists
					if(double.IsNaN(prevHigh))
					{
						prevHigh = dataPoint.YValues[yValueHighIndex];
						prevLow = dataPoint.YValues[yValueLowIndex];
						++pointIndex;
						continue;
					}

					// Check direction of the price change
					int direction = 0;
					if(prevDirection == 1 || prevDirection == 0)
					{
						if(dataPoint.YValues[yValueHighIndex] >= (prevHigh + boxSize))
						{
							direction = 1;
							numberOfBricks = (int)Math.Floor(
								(dataPoint.YValues[yValueHighIndex] - prevHigh) / boxSize);
						}
						else if(dataPoint.YValues[yValueLowIndex] <= (prevHigh - boxSize * reversalAmount))
						{
							direction = -1;
							numberOfBricks = (int)Math.Floor(
								(prevHigh - dataPoint.YValues[yValueLowIndex]) / boxSize);
						}
                            // Adjust the lower part of the column while going up
                        else if (dataPoint.YValues[yValueHighIndex] <= (prevLow - boxSize))
                        {
                            doNotUpdate = true;
                            numberOfBricks = (int)Math.Floor(
                                (prevLow - dataPoint.YValues[yValueHighIndex]) / boxSize);

                            if (series.Points.Count > 0)
                            {
                                series.Points[series.Points.Count - 1].YValues[0] -= numberOfBricks * boxSize;
                            }
                            prevLow -= numberOfBricks * boxSize;
                        }

                    }
					if(direction == 0 &&
						(prevDirection == -1 || prevDirection == 0) )
					{
						if(dataPoint.YValues[yValueLowIndex] <= (prevLow - boxSize))
						{
							direction = -1;
							numberOfBricks = (int)Math.Floor(
								(prevLow - dataPoint.YValues[yValueLowIndex]) / boxSize);
						}
						else if(dataPoint.YValues[yValueHighIndex] >= (prevLow + boxSize * reversalAmount))
						{
							direction = 1;
							numberOfBricks = (int)Math.Floor(
								(dataPoint.YValues[yValueHighIndex] - prevLow) / boxSize);
						}
                        // Adjust the upper part of the column while going down
                        else if (dataPoint.YValues[yValueLowIndex] >= (prevHigh + boxSize))
                        {
                            doNotUpdate = true;
                            numberOfBricks = (int)Math.Floor(
                                (prevHigh - dataPoint.YValues[yValueLowIndex]) / boxSize);

                            if (series.Points.Count > 0)
                            {
                                series.Points[series.Points.Count - 1].YValues[1] += numberOfBricks * boxSize;
                            }
                            prevHigh += numberOfBricks * boxSize;
                        }

					}

					// Check if value was changed - otherwise do nothing
                    if (direction != 0 && !doNotUpdate)
					{
						// Extend line in same direction
						if(direction == prevDirection)
						{
                            if (direction == 1)
							{
                                series.Points[series.Points.Count - 1].YValues[1] += numberOfBricks * boxSize;
								prevHigh += numberOfBricks * boxSize;
								series.Points[series.Points.Count - 1]["OriginalPointIndex"] = pointIndex.ToString(CultureInfo.InvariantCulture);
							}
							else
							{
                                series.Points[series.Points.Count - 1].YValues[0] -= numberOfBricks * boxSize;
                                prevLow -= numberOfBricks * boxSize;
                                series.Points[series.Points.Count - 1]["OriginalPointIndex"] = pointIndex.ToString(CultureInfo.InvariantCulture);
							}
						}
						else
						{
							// Opposite direction by more than reversal amount
							DataPoint newDataPoint = (DataPoint)dataPoint.Clone();
							newDataPoint["OriginalPointIndex"] = pointIndex.ToString(CultureInfo.InvariantCulture);
							newDataPoint.series = series;
							newDataPoint.XValue = dataPoint.XValue;
							if(direction == 1)
							{
								newDataPoint.Color = upPriceColor;
								newDataPoint["PriceUpPoint"] = "true";
								newDataPoint.YValues[0] = prevLow + ((prevDirection != 0) ? boxSize : 0.0);
                                newDataPoint.YValues[1] = newDataPoint.YValues[0] + numberOfBricks * boxSize - ((prevDirection != 0) ? boxSize : 0.0);
							}
							else
							{
								newDataPoint.YValues[1] = prevHigh - ((prevDirection != 0) ? boxSize : 0.0);
								newDataPoint.YValues[0] = newDataPoint.YValues[1] - numberOfBricks * boxSize;
                            }

                            prevHigh = newDataPoint.YValues[1];
                            prevLow = newDataPoint.YValues[0];

							// Add PointAndFigure to the range step line series
							series.Points.Add(newDataPoint);
						}

                        // Save previous close value and direction
						prevDirection = direction;
					}
				}
				++pointIndex;
			}

		}

		/// <summary>
		/// Customize chart event, used to add empty points to make point and
		/// figure chart symbols look proportional.
		/// </summary>
        /// <param name="sender">The source Chart object of this event.</param>
        /// <param name="e">The EventArgs object that contains the event data.</param>
		static private void OnCustomize(Object sender, EventArgs e)
		{
			bool	chartResized = false;
            Chart chart = (Chart)sender;
			// Loop through all series
			foreach(Series series in chart.Series)
			{
				// Check for the PointAndFigure chart type
				if(series.Name.StartsWith("POINTANDFIGURE_ORIGINAL_DATA_", StringComparison.Ordinal))
				{
					// Get original series
					Series	pointAndFigureSeries = chart.Series[series.Name.Substring(29)];

					// Check if proportional symbol custom attribute is set
					bool	proportionalSymbols = true;
					string	attrValue = pointAndFigureSeries[CustomPropertyName.ProportionalSymbols];
					if(attrValue != null && String.Compare( attrValue, "True", StringComparison.OrdinalIgnoreCase ) != 0 )
					{
						proportionalSymbols = false;
					}

					if(proportionalSymbols && 
						pointAndFigureSeries.Enabled && 
						pointAndFigureSeries.ChartArea.Length > 0)
					{
						// Resize chart
						if(!chartResized)
						{
							chartResized = true;
							chart.chartPicture.Resize(chart.chartPicture.ChartGraph, false);
						}

						// Find series chart area, X & Y axes
						ChartArea	chartArea = chart.ChartAreas[pointAndFigureSeries.ChartArea];
						Axis		axisX = chartArea.GetAxis(AxisName.X, pointAndFigureSeries.XAxisType, pointAndFigureSeries.XSubAxisName);
						Axis		axisY = chartArea.GetAxis(AxisName.Y, pointAndFigureSeries.YAxisType, pointAndFigureSeries.YSubAxisName);

						// Symbols are drawn only in 2D mode
						if(!chartArea.Area3DStyle.Enable3D)
						{
							// Get current box size
							double boxSize = double.Parse(
								pointAndFigureSeries["CurrentBoxSize"],
								CultureInfo.InvariantCulture);

							// Calculate symbol width and height
							double boxYSize = Math.Abs(
								axisY.GetPosition(axisY.Minimum) - 
								axisY.GetPosition(axisY.Minimum + boxSize) );
							double boxXSize = Math.Abs(
								axisX.GetPosition(1.0) - 
								axisX.GetPosition(0.0) );
							boxXSize *= 0.8;

							// Get absolute size in pixels
							SizeF markSize = chart.chartPicture.ChartGraph.GetAbsoluteSize(
								new SizeF((float)boxXSize, (float)boxYSize));

							// Calculate number of empty points that should be added
							int pointCount = 0;
							if(markSize.Width > markSize.Height)
							{
								pointCount = (int)(pointAndFigureSeries.Points.Count * (markSize.Width / markSize.Height));
							}

							// Add empty points
							DataPoint emptyPoint = new DataPoint(pointAndFigureSeries);
							emptyPoint.IsEmpty = true;
							emptyPoint.AxisLabel = " ";
							while(pointAndFigureSeries.Points.Count < pointCount)
							{
								pointAndFigureSeries.Points.Add(emptyPoint);
							}

							// Always use zeros for Y values of empty points
							pointAndFigureSeries[CustomPropertyName.EmptyPointValue] = "Zero";
		
							// RecalculateAxesScale chart are data
							chartArea.ReCalcInternal();
						}
					}
				}
			}
		}

		#endregion // Methods

		#region Drawing methods
		
		/// <summary>
		/// Draws 2D column using 'X' or 'O' symbols.
		/// </summary>
		/// <param name="graph">Chart graphics.</param>
		/// <param name="vAxis">Vertical axis.</param>
		/// <param name="rectSize">Column position and size.</param>
		/// <param name="point">Column data point.</param>
		/// <param name="ser">Column series.</param>
		protected override void DrawColumn2D( 
			ChartGraphics graph,
			Axis vAxis,
			RectangleF rectSize,
			DataPoint point, 
			Series ser)
		{
			// Get box size
            double boxSize = double.Parse(ser["CurrentBoxSize"], CultureInfo.InvariantCulture);
			double boxSizeRel = vAxis.GetLogValue(vAxis.ViewMinimum);
			boxSizeRel = vAxis.GetLinearPosition(boxSizeRel);
			boxSizeRel = Math.Abs(boxSizeRel - 
				vAxis.GetLinearPosition(vAxis.GetLogValue(vAxis.ViewMinimum + boxSize)));

			// Draw a series of Xs or Os
			for(float positionY = rectSize.Y; positionY < rectSize.Bottom - (float)(boxSizeRel - boxSizeRel/4.0); positionY += (float)boxSizeRel)
			{
				// Get position of symbol
				RectangleF	position = RectangleF.Empty;
				position.X = rectSize.X;
				position.Y = positionY;
				position.Width = rectSize.Width;
				position.Height = (float)boxSizeRel;

				// Get absolute position and add 1 pixel spacing
				position = graph.GetAbsoluteRectangle(position);
				int	spacing = 1 + point.BorderWidth / 2;
				position.Y += spacing;
				position.Height -= 2 * spacing;

				// Calculate shadow position
				RectangleF	shadowPosition = new RectangleF(position.Location, position.Size);
				shadowPosition.Offset(ser.ShadowOffset, ser.ShadowOffset);

				if(point.IsCustomPropertySet("PriceUpPoint"))
				{
					// Draw shadow
					if(ser.ShadowOffset != 0)
					{
						graph.DrawLineAbs( 
							ser.ShadowColor, 
							point.BorderWidth, 
							ChartDashStyle.Solid, 
							new PointF(shadowPosition.Left, shadowPosition.Top),
							new PointF(shadowPosition.Right, shadowPosition.Bottom));
						graph.DrawLineAbs( 
							ser.ShadowColor, 
							point.BorderWidth, 
							ChartDashStyle.Solid, 
							new PointF(shadowPosition.Left, shadowPosition.Bottom),
							new PointF(shadowPosition.Right, shadowPosition.Top));
					}
					
					// Draw 'X' symbol
					graph.DrawLineAbs( 
						point.Color, 
						point.BorderWidth, 
						ChartDashStyle.Solid, 
						new PointF(position.Left, position.Top),
						new PointF(position.Right, position.Bottom));
					graph.DrawLineAbs( 
						point.Color, 
						point.BorderWidth, 
						ChartDashStyle.Solid, 
						new PointF(position.Left, position.Bottom),
						new PointF(position.Right, position.Top));
				}
				else
				{
					// Draw circles when price is dropping
					if(ser.ShadowOffset != 0)
					{
						graph.DrawCircleAbs(
							new Pen(ser.ShadowColor, point.BorderWidth), 
							null, 
							shadowPosition, 
							1, 
							false);
					}

					// Draw 'O' symbol
					graph.DrawCircleAbs(
						new Pen(point.Color, point.BorderWidth), 
						null, 
						position, 
						1, 
						false);
				}
			}


		}

		#endregion // Drawing methods

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		override public string Name			{ get{ return ChartTypeNames.PointAndFigure;}}

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
	}
}

