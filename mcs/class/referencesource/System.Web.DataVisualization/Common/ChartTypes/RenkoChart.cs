//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		RenkoChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	RenkoChart
//
//  Purpose:	Renko chart type provides methods for calculations and
//				depends on the Range Column chart type to do all the 
//				drawing. PrepareData method is used to create temporary
//				RangeColumn series and fill it with data. Changes are 
//				then reversed in the UnPrepareData method.
//
//	Renko Chart Overview:
//	---------------------
//	The Renko charting method is thought to have acquired its name 
//	from Renga, a Japanese word for bricks. Renko charts isolate the 
//	underlying price trends by filtering out minor price changes. 
//	These charts can be very useful for determining major trend lines, 
//	or support and resistance levels.
//	
//	Basic trend reversals are signaled with the emergence of a new 
//	color brick which depends on the choice of colors used in the 
//	series. Since the Renko chart is used as a trend following aid, 
//	there are times when Renko charts produce whip saws, giving 
//	signals near the end of short-lived trends. However, the 
//	expectation with any trend following technique is that it allows 
//	you to ride the major portion of any significant trends.
//	
//	Renko charts are normally based on closing price values. However, 
//	unless otherwise specified, the value reflected in the chart will 
//	be the first YValue. You can also specify a box size that 
//	determines the minimum price change to display in the chart. The 
//	default box size is calculated from the average share price over 
//	the charted period.

//	The following should be taken into account when working with Renko 
//	charts:
//	
//	- The X values of data points are automatically indexed. 
//	
//	- There is a formula applied to the original data before that data 
//	gets plotted. This formula changes the number of points in the data, 
//	and also changes the X and Y values of the data points. 
//	
//	- Due to data being recalculated, we do not recommend setting the 
//	minimum and/or maximum values for the X axis. This is because it 
//	cannot be determined how many data points will actually be plotted. 
//	However, if the axis' Maximum, or Minimum is set, then the Maximum, 
//	or Minimum properties should use data point index values. 
//	
//	- Data point anchoring, used for annotations, is not supported in 
//	this type of chart. 
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
    /// RenkoChart class provides methods to perform all nessesary 
    /// calculations to display Renko chart with the help of the 
    /// temporary RangeColumn series. This series is created in the 
    /// PrepareData method and then removed in the UnPrepareData method.
	/// </summary>
	internal class RenkoChart : IChartType
	{
		#region Methods

		/// <summary>
		/// Prepares renko chart type for rendering.
		/// </summary>
		/// <param name="series">Series to be prepared.</param>
		internal static void PrepareData(Series series)
		{
			// Check series chart type
			if( String.Compare( series.ChartTypeName, ChartTypeNames.Renko, StringComparison.OrdinalIgnoreCase ) != 0 || !series.IsVisible())
			{
				return;
			}

			// Get reference to the chart control
			Chart	chart = series.Chart;
			if(chart == null)
			{
                throw (new InvalidOperationException(SR.ExceptionRenkoNullReference));
			}

            // Renko chart may not be combined with any other chart types
            ChartArea area = chart.ChartAreas[series.ChartArea];
            foreach (Series currentSeries in chart.Series)
            {
                if (currentSeries.IsVisible() && currentSeries != series && area == chart.ChartAreas[currentSeries.ChartArea])
                {
                    throw (new InvalidOperationException(SR.ExceptionRenkoCanNotCobine));
                }
            }


			// Create a temp series which will hold original series data points
			Series seriesOriginalData = new Series("RENKO_ORIGINAL_DATA_" + series.Name, series.YValuesPerPoint);
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


			// Change renko series type to range column
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

			// Calculate renko bricks data points values
			FillRenkoData(series, seriesOriginalData);
		}

		/// <summary>
		/// Remove any changes done while preparing renko chart type for rendering.
		/// </summary>
		/// <param name="series">Series to be un-prepared.</param>
		/// <returns>True if series was removed from collection.</returns>
		internal static bool UnPrepareData(Series series)
		{
            if (series.Name.StartsWith("RENKO_ORIGINAL_DATA_", StringComparison.Ordinal))
            {
                // Get reference to the chart control
                Chart chart = series.Chart;
                if (chart == null)
                {
                    throw (new InvalidOperationException(SR.ExceptionRenkoNullReference));
                }

                // Get original Renko series
                Series renkoSeries = chart.Series[series.Name.Substring(20)];
                Series.MovePositionMarkers(renkoSeries, series);
                // Copy data back to original Renko series
                renkoSeries.Points.Clear();
                if (!series.IsCustomPropertySet("TempDesignData"))
                {
                    foreach (DataPoint dp in series.Points)
                    {
                        renkoSeries.Points.Add(dp);
                    }
                }

                // Restore renko series properties
                renkoSeries.ChartType = SeriesChartType.Renko;

                bool isXValIndexed;
                bool parseSucceed = bool.TryParse(renkoSeries["OldXValueIndexed"], out isXValIndexed);
                renkoSeries.IsXValueIndexed = parseSucceed && isXValIndexed;

                int yValsPerPoint;
                parseSucceed = int.TryParse(renkoSeries["OldYValuesPerPoint"], NumberStyles.Any, CultureInfo.InvariantCulture, out yValsPerPoint);

                if (parseSucceed)
                {
                    renkoSeries.YValuesPerPoint = yValsPerPoint;
                }

                renkoSeries.DeleteCustomProperty("OldXValueIndexed");
                renkoSeries.DeleteCustomProperty("OldYValuesPerPoint");

                series["OldAutomaticXAxisInterval"] = "true";
                if (renkoSeries.IsCustomPropertySet("OldAutomaticXAxisInterval"))
                {
                    renkoSeries.DeleteCustomProperty("OldAutomaticXAxisInterval");

                    // Reset automatic interval for X axis
                    if (renkoSeries.ChartArea.Length > 0)
                    {
                        // Get X axis connected to the series
                        ChartArea area = chart.ChartAreas[renkoSeries.ChartArea];
                        Axis xAxis = area.GetAxis(AxisName.X, renkoSeries.XAxisType, renkoSeries.XSubAxisName);

                        xAxis.interval = 0.0;
                        xAxis.intervalType = DateTimeIntervalType.Auto;
                    }
                }

                // Remove series from the collection
                chart.Series.Remove(series);
                return true;
            }

			// Remove current box size attribute
			if(series.IsCustomPropertySet("CurrentBoxSize"))
			{
				series.DeleteCustomProperty("CurrentBoxSize");
			}

			return false;
		}

		/// <summary>
		/// Gets box size of the renko chart.
		/// </summary>
		/// <param name="series">Range column chart series used to dispaly the renko chart.</param>
		/// <param name="originalData">Series with original data.</param>
		/// <param name="yValueIndex">Index of the Y value to use.</param>
		private static double GetBoxSize(Series series, Series originalData, int yValueIndex)
		{
			// Check "BoxSize" custom attribute
			double	boxSize = 1.0;
			double	percentOfPriceRange = 4.0;
			bool	roundBoxSize = true;
			if(series.IsCustomPropertySet(CustomPropertyName.BoxSize))
			{
				string	attrValue = series[CustomPropertyName.BoxSize].Trim();
				bool	usePercentage = attrValue.EndsWith("%", StringComparison.Ordinal);
				if(usePercentage)
				{
					attrValue = attrValue.Substring(0, attrValue.Length - 1);
				}

				try
				{
					if(usePercentage)
					{
						percentOfPriceRange = double.Parse(attrValue, CultureInfo.InvariantCulture);
						roundBoxSize = false;
					}
					else
					{
						boxSize = double.Parse(attrValue, CultureInfo.InvariantCulture);
						percentOfPriceRange = 0.0;
					}
				}
				catch
				{
                    throw (new InvalidOperationException(SR.ExceptionRenkoBoxSizeFormatInvalid));
				}
			}

			// Calculate box size using the percentage of price range
			if(percentOfPriceRange > 0.0)
			{
				// Set default box size
				boxSize = 1.0;

				// Calculate percent of the highest and lowest price difference.
				double highest = double.MinValue;
				double lowest = double.MaxValue;
				foreach(DataPoint dp in originalData.Points)
				{
					if(!dp.IsEmpty)
					{
						if(dp.YValues[yValueIndex] > highest)
						{
							highest = dp.YValues[yValueIndex];
						}
						if(dp.YValues[yValueIndex] < lowest)
						{
							lowest = dp.YValues[yValueIndex];
						}
					}
				}

				// Calculate box size as percentage of price difference
				if(lowest == highest)
				{
					boxSize = 1.0;
				}
				else if( (highest - lowest) < 0.000001)
				{
					boxSize = 0.000001;
				}
				else
				{
					boxSize = (highest - lowest) * (percentOfPriceRange / 100.0);
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
		/// Fills range column series with data to draw the renko chart.
		/// </summary>
		/// <param name="series">Range column chart series used to dispaly the renko chart.</param>
		/// <param name="originalData">Series with original data.</param>
		private static void FillRenkoData(Series series, Series originalData)
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
                    throw (new InvalidOperationException(SR.ExceptionRenkoUsedYValueFormatInvalid));
				}

				if(yValueIndex >= series.YValuesPerPoint)
				{
                    throw (new InvalidOperationException(SR.ExceptionRenkoUsedYValueOutOfRange));
				}
			}

			// Calculate box size
			double	boxSize = GetBoxSize(series, originalData, yValueIndex);

			// Fill points
			double	prevLow = double.NaN;
			double	prevHigh = double.NaN;
			int		pointIndex = 0;
			foreach(DataPoint dataPoint in originalData.Points)
			{
				if(!dataPoint.IsEmpty)
				{
					int		numberOfBricks = 0;
					bool	goingUp = true;

					// Check if previus values exists
					if(double.IsNaN(prevLow) || double.IsNaN(prevHigh))
					{
						prevHigh = dataPoint.YValues[yValueIndex];
						prevLow = dataPoint.YValues[yValueIndex];
						++pointIndex;
						continue;
					}

					// Get Up Brick color
					Color	upBrickColor = Color.Transparent;
					string	upBrickColorString = dataPoint[CustomPropertyName.PriceUpColor];
					if(upBrickColorString == null)
					{
						upBrickColorString = series[CustomPropertyName.PriceUpColor];
					}
					if(upBrickColorString != null)
					{
						try
						{
							ColorConverter colorConverter = new ColorConverter();
							upBrickColor = (Color)colorConverter.ConvertFromString(null, CultureInfo.InvariantCulture, upBrickColorString);
						}
						catch
						{
                            throw (new InvalidOperationException(SR.ExceptionRenkoUpBrickColorInvalid));
						}
					}

					// Check if close value exceeds last brick position by box size
					if(dataPoint.YValues[yValueIndex] >= (prevHigh + boxSize))
					{
						goingUp = true;
						numberOfBricks = (int)Math.Floor((dataPoint.YValues[yValueIndex] - prevHigh) / boxSize);
					}
					else if(dataPoint.YValues[yValueIndex] <= (prevLow - boxSize))
					{
						goingUp = false;
						numberOfBricks = (int)Math.Floor((prevLow - dataPoint.YValues[yValueIndex]) / boxSize);
					}

					// Add points
					while(numberOfBricks > 0)
					{
						// Create new point
						DataPoint newDataPoint = (DataPoint)dataPoint.Clone();
						newDataPoint["OriginalPointIndex"] = pointIndex.ToString(CultureInfo.InvariantCulture);
						newDataPoint.series = series;
						newDataPoint.YValues = new double[2];
						newDataPoint.XValue = dataPoint.XValue;
                        newDataPoint.Tag = dataPoint;
						if(goingUp)
						{
							newDataPoint.YValues[1] = prevHigh;
							newDataPoint.YValues[0] = prevHigh + boxSize;
							prevLow = prevHigh;
							prevHigh = prevLow + boxSize;

							// Set renko up brick appearance
							newDataPoint.Color = upBrickColor;
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
							newDataPoint.YValues[0] = prevLow - boxSize;
							prevHigh = prevLow;
							prevLow = prevHigh - boxSize;
						}

						// Add renko brick to the range column series
						series.Points.Add(newDataPoint);
						--numberOfBricks;
					}
				}
				++pointIndex;
			}
		}

		#endregion // Methods

		#region Painting and Selection methods

		/// <summary>
		/// Paint stock chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		virtual public void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
		{
            // Renko series is never drawn directly. It is replaced with the range column chart. 
            // See PrepareData method.
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		virtual public string Name			{ get{ return ChartTypeNames.Renko;}}

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
            // NOTE: SmartLabelStyle feature is not supported by this chart type.
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

