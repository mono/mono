//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ThreeLineBreakChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//  Purpose:	ThreeLineBreak chart type provides methods for 
//              calculations and depends on the Range Column chart 
//              type to do all the drawing. PrepareData method is 
//              used to create temporary RangeColumn series and fill 
//              it with data. Changes are then reversed in the 
//              UnPrepareData method.
//
//	ThreeLineBreak Chart Overview:
//	------------------------------
//  
//  The Three Line Break chart is popular in Japan for financial 
//  charting. These charts display a series of vertical boxes ("lines") 
//  that reflect changes in price values. Similar to Kagi, Renko, and 
//  Point & Figure charts, the Three Line Break chart ignores the 
//  passage of time.
//  
//  The Three Line Break charting method is so-named because of the 
//  number of lines typically used. Each line may indicate "Buy", 
//  "Sell", and "trend less" markets. An advantage of Three Line Break 
//  charts is that there is no arbitrary fixed reversal amount. It is 
//  the price action which gives the indication of a reversal. The 
//  disadvantage of Three Line Break charts is that the signals are 
//  generated after the new trend is well under way. However, many 
//  traders are willing to accept the late signals in exchange for 
//  calling major trends.
//  
//  The sensitivity of the reversal criteria can be set by changing 
//  the number of lines in the break. For example, short-term traders 
//  might use two-line breaks to get more reversals, while a 
//  longer-term investor might use four-line, or even 10-line breaks 
//  to reduce the number of reversals. This is done using the 
//  NumberOfLinesInBreak custom attribute.
//  
//  The following should be taken into account when working with 
//  Three Line Break charts:
//  
//  - The X values of data points are automatically indexed. 
//  
//  - There is a formula applied to the original data before that data 
//  gets plotted. This formula changes the number of points in the data, 
//  and also changes the data points' X/Y values. 
//  
//  - Due to data being recalculated, we do not recommend setting the 
//  minimum and/or maximum values for the X axis. This is because it 
//  cannot be determined how many data points will actually be plotted. 
//  However, if the axis' Maximum, or Minimum is set, then the Maximum, 
//  or Minimum properties should use data point index values. 
//  
//  - Data point anchoring, used for annotations, is not supported in 
//  this type of chart. 
//  
//	Reviewed:	AG - Microsoft 7, 2007
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
    /// ThreeLineBreakChart class provides methods to perform all nessesary 
    /// calculations to display ThreeLineBreak chart with the help of the 
    /// temporary RangeColumn series. This series is created in the 
    /// PrepareData method and then removed in the UnPrepareData method.
	/// </summary>
	internal class ThreeLineBreakChart : IChartType
	{
		#region Methods

		/// <summary>
		/// Prepares ThreeLineBreak chart type for rendering.
		/// </summary>
		/// <param name="series">Series to be prepared.</param>
		internal static void PrepareData(Series series)
		{
			// Check series chart type
			if(String.Compare(series.ChartTypeName, ChartTypeNames.ThreeLineBreak, StringComparison.OrdinalIgnoreCase ) != 0 || !series.IsVisible())
			{
				return;
			}

			// Get reference to the chart control
			Chart	chart = series.Chart;
			if(chart == null)
			{
                throw (new InvalidOperationException(SR.ExceptionThreeLineBreakNullReference));
			}

            // ThreeLineBreak chart may not be combined with any other chart types
            ChartArea area = chart.ChartAreas[series.ChartArea];
            foreach (Series currentSeries in chart.Series)
            {
                if (currentSeries.IsVisible() && currentSeries != series && area == chart.ChartAreas[currentSeries.ChartArea])
                {
                    throw (new InvalidOperationException(SR.ExceptionThreeLineBreakCanNotCobine));
                }
            }

			// Create a temp series which will hold original series data points
			Series seriesOriginalData = new Series("THREELINEBREAK_ORIGINAL_DATA_" + series.Name, series.YValuesPerPoint);
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


			// Change ThreeLineBreak series type to range column
			series["OldXValueIndexed"] = series.IsXValueIndexed.ToString(CultureInfo.InvariantCulture);
			series["OldYValuesPerPoint"] = series.YValuesPerPoint.ToString(CultureInfo.InvariantCulture);
			series.ChartType = SeriesChartType.RangeColumn;
			series.IsXValueIndexed = true;
			series.YValuesPerPoint = 2;

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

			// Calculate ThreeLineBreak bricks data points values
			FillThreeLineBreakData(series, seriesOriginalData);
		}

		/// <summary>
		/// Remove any changes done while preparing ThreeLineBreak chart type for rendering.
		/// </summary>
		/// <param name="series">Series to be un-prepared.</param>
		/// <returns>True if series was removed from collection.</returns>
		internal static bool UnPrepareData(Series series)
		{
            if (series.Name.StartsWith("THREELINEBREAK_ORIGINAL_DATA_", StringComparison.Ordinal))
            {
                // Get reference to the chart control
                Chart chart = series.Chart;
                if (chart == null)
                {
                    throw (new InvalidOperationException(SR.ExceptionThreeLineBreakNullReference));
                }

                // Get original ThreeLineBreak series
                Series threeLineBreakSeries = chart.Series[series.Name.Substring(29)];
                Series.MovePositionMarkers(threeLineBreakSeries, series);
                // Copy data back to original ThreeLineBreak series
                threeLineBreakSeries.Points.Clear();
                if (!series.IsCustomPropertySet("TempDesignData"))
                {
                    foreach (DataPoint dp in series.Points)
                    {
                        threeLineBreakSeries.Points.Add(dp);
                    }
                }

                // Restore ThreeLineBreak series properties
                threeLineBreakSeries.ChartType = SeriesChartType.ThreeLineBreak;

                bool xValIndexed;
                bool parseSucceed = bool.TryParse(threeLineBreakSeries["OldXValueIndexed"], out xValIndexed);
                threeLineBreakSeries.IsXValueIndexed = parseSucceed && xValIndexed;

                int yValsPerPoint;
                parseSucceed = int.TryParse(threeLineBreakSeries["OldYValuesPerPoint"], NumberStyles.Any, CultureInfo.InvariantCulture, out yValsPerPoint);

                if (parseSucceed)
                {
                    threeLineBreakSeries.YValuesPerPoint = yValsPerPoint;
                }


                threeLineBreakSeries.DeleteCustomProperty("OldXValueIndexed");
                threeLineBreakSeries.DeleteCustomProperty("OldYValuesPerPoint");

                series["OldAutomaticXAxisInterval"] = "true";
                if (threeLineBreakSeries.IsCustomPropertySet("OldAutomaticXAxisInterval"))
                {
                    threeLineBreakSeries.DeleteCustomProperty("OldAutomaticXAxisInterval");

                    // Reset automatic interval for X axis
                    if (threeLineBreakSeries.ChartArea.Length > 0)
                    {
                        // Get X axis connected to the series
                        ChartArea area = chart.ChartAreas[threeLineBreakSeries.ChartArea];
                        Axis xAxis = area.GetAxis(AxisName.X, threeLineBreakSeries.XAxisType, threeLineBreakSeries.XSubAxisName);

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
		/// Fills range column series with data to draw the ThreeLineBreak chart.
		/// </summary>
		/// <param name="series">Range column chart series used to dispaly the ThreeLineBreak chart.</param>
		/// <param name="originalData">Series with original data.</param>
		private static void FillThreeLineBreakData(Series series, Series originalData)
		{
			// Get index of the Y values used
			int	yValueIndex = 0;
			if(series.IsCustomPropertySet(CustomPropertyName.UsedYValue))
			{
				try
				{
					yValueIndex = int.Parse(series[CustomPropertyName.UsedYValue], CultureInfo.InvariantCulture);
				}
				catch
				{
                    throw (new InvalidOperationException(SR.ExceptionThreeLineBreakUsedYValueInvalid));
				}

				if(yValueIndex >= series.YValuesPerPoint)
				{
                    throw (new InvalidOperationException(SR.ExceptionThreeLineBreakUsedYValueOutOfRange));
				}
			}

			// Get number of lines in the break
			int	linesInBreak = 3;
			if(series.IsCustomPropertySet(CustomPropertyName.NumberOfLinesInBreak))
			{
				try
				{
					linesInBreak = int.Parse(series[CustomPropertyName.NumberOfLinesInBreak], CultureInfo.InvariantCulture);
				}
				catch
				{
                    throw (new InvalidOperationException(SR.ExceptionThreeLineBreakNumberOfLinesInBreakFormatInvalid));
				}

				if(linesInBreak <= 0)
				{
                    throw (new InvalidOperationException(SR.ExceptionThreeLineBreakNumberOfLinesInBreakValueInvalid));
				}
			}

			// Create an array to store the history of high/low values of drawn lines
			ArrayList	highLowHistory = new ArrayList();

			// Fill points
			double	prevLow = double.NaN;
			double	prevHigh = double.NaN;
			int		sameDirectionLines = 0;
			int		prevDirection = 0;
			int		pointIndex = 0;
			foreach(DataPoint dataPoint in originalData.Points)
			{
				int	direction = 0;	// 1 up; -1 down

				// Skip empty points
				if(dataPoint.IsEmpty)
				{
					++pointIndex;
					continue;
				}

				// Check if previus values exists
				if(double.IsNaN(prevLow) || double.IsNaN(prevHigh))
				{
					prevHigh = dataPoint.YValues[yValueIndex];
					prevLow = dataPoint.YValues[yValueIndex];
					++pointIndex;
					continue;
				}

				// Get up price color
				Color	priceUpColor = Color.Transparent;
				string	priceUpColorString = dataPoint[CustomPropertyName.PriceUpColor];
				if(priceUpColorString == null)
				{
					priceUpColorString = series[CustomPropertyName.PriceUpColor];
				}
				if(priceUpColorString != null)
				{
					try
					{
						ColorConverter colorConverter = new ColorConverter();
						priceUpColor = (Color)colorConverter.ConvertFromString(null, CultureInfo.InvariantCulture, priceUpColorString);
					}
					catch
					{
                        throw (new InvalidOperationException(SR.ExceptionThreeLineBreakUpBrickColorInvalid));
					}
				}

				// Check if close value exceeds last brick position by box size
				if(dataPoint.YValues[yValueIndex] > prevHigh)
				{
					direction = 1;
				}
				else if(dataPoint.YValues[yValueIndex] < prevLow)
				{
					direction = -1;
				}
				else
				{
					direction = 0;
				}

				// Process up/down direction
				if(direction != 0)
				{
					// Check if direction is same as previous
					if(prevDirection == direction)
					{
						++sameDirectionLines;
					}
					else
					{
						// If number of lines darwn in same direction is more or equal
						// to number of lines in the break, the price must extend the 
						// high or low price of the lines in the whole break.
						if(sameDirectionLines >= linesInBreak)
						{
							if(direction == 1)
							{
								// Calculate high value for the last N lines
								double lineBreakHigh = double.MinValue;
								for(int index = 0; index < highLowHistory.Count; index += 2)
								{
									if(((double)highLowHistory[index]) > lineBreakHigh)
									{
										lineBreakHigh = ((double)highLowHistory[index]);
									}
								}

								// If point value is less - ignore it
								if(dataPoint.YValues[yValueIndex] <= lineBreakHigh)
								{
									direction = 0;
								}
							}
							else if(direction == -1)
							{
								// Calculate low value for the last N lines
								double lineBreakLow = double.MaxValue;
								for(int index = 1; index < highLowHistory.Count; index += 2)
								{
									if(((double)highLowHistory[index]) < lineBreakLow)
									{
										lineBreakLow = ((double)highLowHistory[index]);
									}
								}

								// If point value is more - ignore it
								if(dataPoint.YValues[yValueIndex] >= lineBreakLow)
								{
									direction = 0;
								}
							}
						}

						if(direction != 0)
						{
							sameDirectionLines = 1;
						}
					}

					if(direction != 0)
					{
						// Add point
						DataPoint newDataPoint = (DataPoint)dataPoint.Clone();
						newDataPoint["OriginalPointIndex"] = pointIndex.ToString(CultureInfo.InvariantCulture);
						newDataPoint.series = series;
						newDataPoint.YValues = new double[2];
						newDataPoint.XValue = dataPoint.XValue;
                        newDataPoint.Tag = dataPoint;
						if(direction == 1)
						{
							newDataPoint.YValues[1] = prevHigh;
							newDataPoint.YValues[0] = dataPoint.YValues[yValueIndex];
							prevLow = prevHigh;
							prevHigh = dataPoint.YValues[yValueIndex];

							// Set ThreeLineBreak up brick appearance
							newDataPoint.Color = priceUpColor;
							if(newDataPoint.BorderWidth < 1)
							{
								newDataPoint.BorderWidth = 1;
							}
							if(newDataPoint.BorderDashStyle == ChartDashStyle.NotSet)
							{
								newDataPoint.BorderDashStyle = ChartDashStyle.Solid;
							}
							if( (newDataPoint.BorderColor == Color.Empty || newDataPoint.BorderColor == Color.Transparent) &&
								(newDataPoint.Color == Color.Empty || newDataPoint.Color == Color.Transparent) )
							{
								newDataPoint.BorderColor = series.Color;
							}
						}
						else
						{
							newDataPoint.YValues[1] = prevLow;
							newDataPoint.YValues[0] = dataPoint.YValues[yValueIndex];
							prevHigh = prevLow;
							prevLow = dataPoint.YValues[yValueIndex];
						}

						// Add ThreeLineBreak brick to the range column series
						series.Points.Add(newDataPoint);

						// Remember high/low values of drawn line
						highLowHistory.Add(prevHigh);
						highLowHistory.Add(prevLow);

						// Do not store all values in array only number of break lines
						if(highLowHistory.Count > linesInBreak * 2)
						{
							// Remove two items at a time (high & low)
							highLowHistory.RemoveAt(0);
							highLowHistory.RemoveAt(0);
						}
					}
				}

				// Remember last direction
				if(direction != 0)
				{
					prevDirection = direction;
				}

				++pointIndex;
			}
		}

		#endregion // Methods

		#region Painting and Selection methods

		/// <summary>
		/// Paint chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		virtual public void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
		{
            // Three Line Break series is never drawn directly. It is replaced with the range column chart. 
            // See PrepareData method.
		}
		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		virtual public string Name			{ get{ return ChartTypeNames.ThreeLineBreak;}}

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
		public bool SecondYScale{ get{ return false;} }

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
		public bool SideBySideSeries { get{ return false;} }

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
		virtual public bool ExtraYValuesConnectedToYAxis{ get { return true; } }
		
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
			return LegendImageStyle.Rectangle;
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

