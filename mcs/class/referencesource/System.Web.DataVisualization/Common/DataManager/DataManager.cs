//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		DataManager.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Data
//
//	Classes:	DataManager
//
//  Purpose:	Series storage and manipulation class.
//
//	Reviewed:	AG - Aug 1, 2002; GS - Aug 7, 2002
//
//===================================================================


#region Used namespaces

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;

#if Microsoft_CONTROL

	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;


#else
	using System.Web.UI;
	using System.Web.UI.WebControls;
    using System.Web.UI.DataVisualization.Charting;
    using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.Utilities;
    using System.Web.UI.DataVisualization.Charting.ChartTypes;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.Data
#else
	namespace System.Web.UI.DataVisualization.Charting.Data
#endif
{
	/// <summary>
	/// Data Manager.
	/// </summary>
	internal class DataManager : ChartElement, IServiceProvider
	{
		#region Fields
		// Series collection
		private SeriesCollection		_series = null;

		// Servise container reference
		internal IServiceContainer		serviceContainer = null;

        // Chart color palette
		private	ChartColorPalette		_colorPalette = ChartColorPalette.BrightPastel;

        #endregion

        #region Constructors and initialization

		/// <summary>
		/// Data manager public constructor
		/// </summary>
		/// <param name="container">Service container object.</param>
		public DataManager(IServiceContainer container)
		{
			if(container == null)
			{
				throw(new ArgumentNullException(SR.ExceptionInvalidServiceContainer));
			}
			serviceContainer = container;
            Common = new CommonElements(container);
			_series = new SeriesCollection(this);
		}

		/// <summary>
		/// Returns Data Manager service object.
		/// </summary>
		/// <param name="serviceType">Service type requested.</param>
		/// <returns>Data Manager service object.</returns>
		[EditorBrowsableAttribute(EditorBrowsableState.Never)]
		object IServiceProvider.GetService(Type serviceType)
		{
			if(serviceType == typeof(DataManager))
			{
				return this;
			}
			throw (new ArgumentException( SR.ExceptionDataManagerUnsupportedType(serviceType.ToString())));
		}

		/// <summary>
		/// Initialize data manger object
		/// </summary>
		internal void Initialize()
		{
			// Attach to the Chart Picture painting events
			ChartImage chartPicture = (ChartImage)serviceContainer.GetService(typeof(ChartImage));
            chartPicture.BeforePaint += new EventHandler<ChartPaintEventArgs>(this.ChartPicture_BeforePaint);
            chartPicture.AfterPaint += new EventHandler<ChartPaintEventArgs>(this.ChartPicture_AfterPaint);
		}

		#endregion

		#region Chart picture painting events hanlers

        internal override void Invalidate()
        {
            base.Invalidate();

#if Microsoft_CONTROL
            if (Chart!=null)
                Chart.Invalidate();
#endif
        }


		/// <summary>
		/// Event fired when chart picture is going to be painted.
		/// </summary>
		/// <param name="sender">Sender object.</param>
		/// <param name="e">Event arguments.</param>
		private void ChartPicture_BeforePaint(object sender, ChartPaintEventArgs e) 
		{
			// Prepare series for drawing
			int	markerIndex = 1;
			for(int index = 0; index < this.Series.Count; index++)
			{
				Series series = this.Series[index];

				// Reset series "X values are zeros" flag
				series.xValuesZerosChecked = false;
				series.xValuesZeros = false;

				// Set series colors from palette
				IChartType chartType = e.CommonElements.ChartTypeRegistry.GetChartType(series.ChartTypeName);
				bool	paletteColorsInPoints = chartType.ApplyPaletteColorsToPoints;
                // if the series palette is set the we can color all data points, even on column chart.
                if (series.Palette != ChartColorPalette.None)
                {
                    paletteColorsInPoints = true;
                }
				
                this.PrepareData(
					paletteColorsInPoints, 
					series.Name);

				// Clear temp. marker style
				if(series.tempMarkerStyleIsSet)
				{
					series.MarkerStyle = MarkerStyle.None;
					series.tempMarkerStyleIsSet = false;
				}

				// Set marker style for chart types based on markes
				if(chartType.GetLegendImageStyle(series) == LegendImageStyle.Marker && series.MarkerStyle == MarkerStyle.None)
				{
					series.MarkerStyle = (MarkerStyle)markerIndex++;
					series.tempMarkerStyleIsSet = true;

					if(markerIndex > 9)
					{
						markerIndex = 1;
					}
				}
			}
		}

        /// <summary>
        /// Event fired after chart picture was painted.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Event arguments.</param>
		private void ChartPicture_AfterPaint(object sender, ChartPaintEventArgs e) 
		{
			Chart control = (Chart)serviceContainer.GetService(typeof(Chart));
			if(control != null)
			{
				// Clean up series after drawing
				for(int index = 0; index < this.Series.Count; index++)
				{
					Series series = this.Series[index];
					if(series.UnPrepareData(control.Site))
					{
						--index;
					}
				}
			}
		}

		#endregion

		#region Series data preparation methods

		/// <summary>
		/// Apply palette colors to the data series if UsePaletteColors property is set.
		/// </summary>
		internal void ApplyPaletteColors()
		{
            ChartColorPalette palette = this.Palette;
            // switch to default pallette if is none and custom collors array is empty.
            if (palette == ChartColorPalette.None && this.PaletteCustomColors.Length == 0)
            {
                palette = ChartColorPalette.BrightPastel;
            }
			
            // Get palette colors
			int colorIndex = 0;
            Color[] paletteColors = (palette == ChartColorPalette.None) ?
                this.PaletteCustomColors : ChartPaletteColors.GetPaletteColors(palette);
            
            foreach (Series dataSeries in _series)
			{
				// Check if chart area name is valid
				bool	validAreaName = false;
                if (Chart!=null)
				{
                    validAreaName = Chart.ChartAreas.IsNameReferenceValid(dataSeries.ChartArea);
				}

				// Change color of the series only if valid chart area name is specified
				if(validAreaName)
				{
					// Change color of the series only if default color is set
					if(dataSeries.Color == Color.Empty || dataSeries.tempColorIsSet)
					{
						dataSeries.color =  paletteColors[colorIndex++];
						dataSeries.tempColorIsSet = true;
						if(colorIndex >=  paletteColors.Length)
						{
							colorIndex = 0;
						}
					}
				}
			}
		}

		/// <summary>
		/// Called just before the data from the series to be used to perform these operations:
		///  - apply palette colors to the data series
		///  - prepare data in series
		/// </summary>
		/// <param name="pointsApplyPaletteColors">If true each data point will be assigned a color from the palette (if it's set)</param>
		/// <param name="series">List of series indexes, which requires data preparation</param>
		internal void PrepareData(bool pointsApplyPaletteColors, params string[] series)
		{
			this.ApplyPaletteColors();

			// Prepare data in series
			Chart control = (Chart)serviceContainer.GetService(typeof(Chart));
			if(control != null)
			{
				foreach(string seriesName in series)
				{
					this.Series[seriesName].PrepareData(pointsApplyPaletteColors);
				}
			}
		}

		#endregion

		#region Series Min/Max values methods

		/// <summary>
		/// This method checks if data point should be skipped. This 
		/// method will return true if data point is empty.
		/// </summary>
		/// <param name="point">Data point</param>
		/// <returns>This method returns true if data point is empty.</returns>
		private bool IsPointSkipped( DataPoint point )
		{
			if( point.IsEmpty )
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets max number of data points in specified series.
		/// </summary>
		/// <param name="series">Series IDs</param>
		/// <returns>Maximum number of data points</returns>
		internal int GetNumberOfPoints(params string[] series)
		{
			int	numberOfPoints = 0;
			foreach(string seriesName in series)
			{
				numberOfPoints = Math.Max(numberOfPoints, this._series[seriesName].Points.Count);
			}
			return numberOfPoints;
		}

		/// <summary>
		/// Gets maximum Y value from many series
		/// </summary>
		/// <param name="valueIndex">Index of Y value to use</param>
		/// <param name="series">Series IDs</param>
		/// <returns>Maximum Y value</returns>
		internal double GetMaxYValue(int valueIndex, params string[] series)
		{
			double	returnValue = Double.MinValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					// The empty point
					if( IsPointSkipped( seriesPoint ) )
					{
						continue;
					}

					if(!double.IsNaN(seriesPoint.YValues[valueIndex]))
					{
						returnValue = Math.Max(returnValue, seriesPoint.YValues[valueIndex]);
					}
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Get Maximum value for Y and and Radius (Y2) ( used for bubble chart )
		/// </summary>
		/// <param name="area">Chart Area</param>
		/// <param name="series">Series IDs</param>
		/// <returns>Maximum Y value</returns>
		internal double GetMaxYWithRadiusValue( ChartArea area, params string[] series )
		{
			double	returnValue = Double.MinValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					// The empty point
					if( IsPointSkipped( seriesPoint ) )
					{
						continue;
					}

					if(!double.IsNaN(seriesPoint.YValues[0]))
					{
                        if (seriesPoint.YValues.Length > 1)
                        {
                            returnValue = Math.Max(returnValue, seriesPoint.YValues[0] + BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.YValues[1], true));
                        }
                        else
                        {
                            returnValue = Math.Max(returnValue, seriesPoint.YValues[0]);
                        }
					}
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Get Maximum value for X and Radius (Y2) ( used for bubble chart )
		/// </summary>
		/// <param name="area">Chart Area</param>
		/// <param name="series">Series IDs</param>
		/// <returns>Maximum X value</returns>
		internal double GetMaxXWithRadiusValue( ChartArea area, params string[] series )
		{
			double	returnValue = Double.MinValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					// The empty point
					if( IsPointSkipped( seriesPoint ) )
					{
						continue;
					}

					if(!double.IsNaN(seriesPoint.XValue))
					{
                        if (seriesPoint.YValues.Length > 1)
                        {
                            returnValue = Math.Max(returnValue, seriesPoint.XValue + BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.XValue, false));
                        }
                        else
                        {
                            returnValue = Math.Max(returnValue, seriesPoint.XValue);
                        }
					}
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Get Minimum value for X and Radius Y2 ( used for bubble chart )
		/// </summary>
		/// <param name="area">Chart Area</param>
		/// <param name="series">Series IDs</param>
		/// <returns>Minimum X value</returns>
		internal double GetMinXWithRadiusValue( ChartArea area, params string[] series )
		{
			double	returnValue = Double.MaxValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					// The empty point
					if( IsPointSkipped( seriesPoint ) )
					{
						continue;
					}

					if(!double.IsNaN(seriesPoint.XValue))
					{
                        if (seriesPoint.YValues.Length > 1)
                        {
                            returnValue = Math.Min(returnValue, seriesPoint.XValue - BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.YValues[1], false));
                        }
                        else
                        {
                            returnValue = Math.Min(returnValue, seriesPoint.XValue);
                        }
					}
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Gets maximum Y value from many series
		/// </summary>
		/// <param name="series">Series IDs</param>
		/// <returns>Maximum Y value</returns>
		internal double GetMaxYValue(params string[] series)
		{
			double	returnValue = Double.MinValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					// The empty point
					if( IsPointSkipped( seriesPoint ) )
					{
						continue;
					}

					foreach( double y in seriesPoint.YValues )
					{
						if(!double.IsNaN(y))
						{
							returnValue = Math.Max(returnValue, y);
						}
					}
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Gets maximum X value from many series
		/// </summary>
		/// <param name="series">Series IDs</param>
		/// <returns>Maximum X value</returns>
		internal double GetMaxXValue(params string[] series)
		{
			double	returnValue = Double.MinValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					returnValue = Math.Max(returnValue, seriesPoint.XValue);
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Gets minimum and maximum X value from many series.
		/// </summary>
		/// <param name="min">Returns maximum X value.</param>
		/// <param name="max">Returns minimum X value.</param>
		/// <param name="series">Series IDs</param>
		internal void GetMinMaxXValue(out double min, out double max, params string[] series)
		{
			max = Double.MinValue;
			min = Double.MaxValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					max = Math.Max(max, seriesPoint.XValue);
					min = Math.Min(min, seriesPoint.XValue);
				}
			}
		}

		/// <summary>
		/// Gets minimum and maximum Y value from many series.
		/// </summary>
		/// <param name="valueIndex">Index of Y value to use.</param>
		/// <param name="min">Returns maximum Y value.</param>
		/// <param name="max">Returns minimum Y value.</param>
		/// <param name="series">Series IDs</param>
		internal void GetMinMaxYValue(int valueIndex, out double min, out double max, params string[] series)
		{
			max = Double.MinValue;
			min = Double.MaxValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					// Skip empty point
					if( IsPointSkipped( seriesPoint ) )
					{
						continue;
					}

					double yValue = seriesPoint.YValues[valueIndex];
					if(!double.IsNaN(yValue))
					{
						max = Math.Max(max, yValue);
						min = Math.Min(min, yValue);
					}
				}
			}
		}

		/// <summary>
		/// Gets minimum and maximum Y value from many series.
		/// </summary>
		/// <param name="min">Returns maximum Y value.</param>
		/// <param name="max">Returns minimum Y value.</param>
		/// <param name="series">Series IDs</param>
		internal void GetMinMaxYValue(out double min, out double max, params string[] series)
		{
			max = Double.MinValue;
			min = Double.MaxValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					// Skip empty point
					if( IsPointSkipped( seriesPoint ) )
					{
						continue;
					}

					// Iterate through all Y values
					foreach( double y in seriesPoint.YValues )
					{
						if(!double.IsNaN(y))
						{
							max = Math.Max(max, y);
							min = Math.Min(min, y);
						}
					}						
				}
			}
		}

		/// <summary>
		/// Gets minimum and maximum Y value from many series.
		/// </summary>
		/// <param name="seriesList">Series objects list.</param>
		/// <param name="min">Returns maximum Y value.</param>
		/// <param name="max">Returns minimum Y value.</param>
		internal void GetMinMaxYValue(System.Collections.ArrayList seriesList, out double min, out double max)
		{
			max = Double.MinValue;
			min = Double.MaxValue;
			foreach(Series series in seriesList)
			{
				foreach(DataPoint seriesPoint in series.Points)
				{
					// Skip empty point
					if( IsPointSkipped( seriesPoint ) )
					{
						continue;
					}

					// Iterate through all Y values
					foreach( double y in seriesPoint.YValues )
					{
						if(!double.IsNaN(y))
						{
							max = Math.Max(max, y);
							min = Math.Min(min, y);
						}
					}						
				}
			}
		}

		/// <summary>
		/// Gets maximum stacked Y value from many series
		/// </summary>
		/// <param name="valueIndex">Index of Y value to use</param>
		/// <param name="series">Series IDs</param>
		/// <returns>Maximum stacked Y value</returns>
		internal double GetMaxStackedYValue(int valueIndex, params string[] series)
		{
			double	returnValue = 0;
			double	numberOfPoints = GetNumberOfPoints(series);
			for(int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
			{
				double stackedMax = 0;
				double noStackedMax = 0;
				foreach(string seriesName in series)
				{
					if(this._series[seriesName].Points.Count > pointIndex)
					{
						// Take chart type from the series 
						ChartTypeRegistry chartTypeRegistry = (ChartTypeRegistry)serviceContainer.GetService(typeof(ChartTypeRegistry));
						IChartType chartType = chartTypeRegistry.GetChartType(this._series[seriesName].ChartTypeName);

						// If stacked area
						if( !chartType.StackSign )
							continue;

						if( chartType.Stacked )
						{
							if(this._series[seriesName].Points[pointIndex].YValues[valueIndex] > 0)
							{
								stackedMax += this._series[seriesName].Points[pointIndex].YValues[valueIndex];
							}
						}
						else
						{
							noStackedMax = Math.Max(noStackedMax,this._series[seriesName].Points[pointIndex].YValues[valueIndex]);
						}
					}
				}
				stackedMax = Math.Max(stackedMax, noStackedMax);
				returnValue = Math.Max(returnValue, stackedMax);
			}
			return returnValue;
		}

		/// <summary>
		/// Gets maximum Unsigned stacked Y value from many series ( Stacked Area chart )
		/// </summary>
		/// <param name="valueIndex">Index of Y value to use</param>
		/// <param name="series">Series IDs</param>
		/// <returns>Maximum stacked Y value</returns>
		internal double GetMaxUnsignedStackedYValue(int valueIndex, params string[] series)
		{
			double	returnValue = 0;
			double	maxValue = Double.MinValue;
			double	numberOfPoints = GetNumberOfPoints(series);
			for(int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
			{
				double stackedMax = 0;
				double noStackedMax = 0;
				foreach(string seriesName in series)
				{
                    if (this._series[seriesName].Points.Count > pointIndex)
                    {
                        // Take chart type from the series 
                        ChartTypeRegistry chartTypeRegistry = (ChartTypeRegistry)serviceContainer.GetService(typeof(ChartTypeRegistry));
                        IChartType chartType = chartTypeRegistry.GetChartType(this._series[seriesName].ChartTypeName);

                        // If stacked column and bar
                        if (chartType.StackSign || double.IsNaN(this._series[seriesName].Points[pointIndex].YValues[valueIndex]))
                        {
                            continue;
                        }

                        if (chartType.Stacked)
                        {
                            maxValue = Double.MinValue;
                            stackedMax += this._series[seriesName].Points[pointIndex].YValues[valueIndex];
                            if (stackedMax > maxValue)
                                maxValue = stackedMax;
                        }
                        else
                        {
                            noStackedMax = Math.Max(noStackedMax, this._series[seriesName].Points[pointIndex].YValues[valueIndex]);
                        }
                    }
				}
				maxValue = Math.Max(maxValue, noStackedMax);
				returnValue = Math.Max(returnValue, maxValue);
			}
			return returnValue;
		}

		/// <summary>
		/// Gets maximum stacked X value from many series
		/// </summary>
		/// <param name="series">Series IDs</param>
		/// <returns>Maximum stacked X value</returns>
		internal double GetMaxStackedXValue(params string[] series)
		{
			double	returnValue = 0;
			double	numberOfPoints = GetNumberOfPoints(series);
			for(int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
			{
				double doubleIndexValue = 0;
				foreach(string seriesName in series)
				{
                    if (this._series[seriesName].Points.Count > pointIndex)
                    {
                        if (this._series[seriesName].Points[pointIndex].XValue > 0)
                        {
                            doubleIndexValue += this._series[seriesName].Points[pointIndex].XValue;
                        }
                    }
				}
				returnValue = Math.Max(returnValue, doubleIndexValue);
			}
			return returnValue;
		}

		/// <summary>
		/// Gets minimum Y value from many series
		/// </summary>
		/// <param name="valueIndex">Index of Y value to use</param>
		/// <param name="series">Series IDs</param>
		/// <returns>Minimum Y value</returns>
		internal double GetMinYValue(int valueIndex, params string[] series)
		{
			double	returnValue = Double.MaxValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					// The empty point
					if( IsPointSkipped( seriesPoint ) )
					{
						continue;
					}

					if(!double.IsNaN(seriesPoint.YValues[valueIndex]))
					{
						returnValue = Math.Min(returnValue, seriesPoint.YValues[valueIndex]);
					}
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Get Minimum value for Y and and Radius (Y2) ( used for bubble chart )
		/// </summary>
		/// <param name="area">Chart Area</param>
		/// <param name="series">Series IDs</param>
		/// <returns>Minimum Y value</returns>
		internal double GetMinYWithRadiusValue( ChartArea area, params string[] series )
		{
			double	returnValue = Double.MaxValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					// The empty point
					if( IsPointSkipped( seriesPoint ) )
					{
						continue;
					}

					if(!double.IsNaN(seriesPoint.YValues[0]))
					{
                        if (seriesPoint.YValues.Length > 1)
                        {
                            returnValue = Math.Min(returnValue, seriesPoint.YValues[0] - BubbleChart.AxisScaleBubbleSize(area.Common, area, seriesPoint.YValues[1], true));
                        }
                        else
                        {
                            returnValue = Math.Min(returnValue, seriesPoint.YValues[0]);
                        }
					}
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Gets minimum Y value from many series
		/// </summary>
		/// <param name="series">Series IDs</param>
		/// <returns>Minimum Y value</returns>
		internal double GetMinYValue(params string[] series)
		{
			double	returnValue = Double.MaxValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					// The empty point
					if( IsPointSkipped( seriesPoint ) )
					{
						continue;
					}

					foreach(double y in seriesPoint.YValues)
					{
						if(!double.IsNaN(y))
						{
							returnValue = Math.Min(returnValue, y);
						}
					}
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Gets minimum X value from many series
		/// </summary>
		/// <param name="series">Series IDs</param>
		/// <returns>Minimum X value</returns>
		internal double GetMinXValue(params string[] series)
		{
			double	returnValue = Double.MaxValue;
			foreach(string seriesName in series)
			{
				foreach(DataPoint seriesPoint in this._series[seriesName].Points)
				{
					returnValue = Math.Min(returnValue, seriesPoint.XValue);
				}
			}
			return returnValue;
		}

		/// <summary>
		/// Gets minimum stacked Y value from many series
		/// </summary>
		/// <param name="valueIndex">Index of Y value to use</param>
		/// <param name="series">Series IDs</param>
		/// <returns>Minimum stacked Y value</returns>
		internal double GetMinStackedYValue(int valueIndex, params string[] series)
		{
			double	returnValue = Double.MaxValue;
			double	numberOfPoints = GetNumberOfPoints(series);
			for(int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
			{
				double stackedMin = 0;
				double noStackedMin = 0;
				foreach(string seriesName in series)
				{
					if(this._series[seriesName].Points.Count > pointIndex)
					{
						// Take chart type from the series 
						ChartTypeRegistry chartTypeRegistry = (ChartTypeRegistry)serviceContainer.GetService(typeof(ChartTypeRegistry));
						IChartType chartType = chartTypeRegistry.GetChartType(this._series[seriesName].ChartTypeName);

						// If stacked area
						if( !chartType.StackSign || double.IsNaN(this._series[seriesName].Points[pointIndex].YValues[valueIndex]))
							continue;

						if( chartType.Stacked )
						{
							if(this._series[seriesName].Points[pointIndex].YValues[valueIndex] < 0)
							{
								stackedMin += this._series[seriesName].Points[pointIndex].YValues[valueIndex];
							}
						}
						else
						{
							noStackedMin = Math.Min(noStackedMin,this._series[seriesName].Points[pointIndex].YValues[valueIndex]);
						}
					}
				}
				stackedMin = Math.Min(stackedMin, noStackedMin);
				if( stackedMin == 0 )
				{
					stackedMin = this._series[series[0]].Points[this._series[series[0]].Points.Count - 1].YValues[valueIndex];
				}
				returnValue = Math.Min(returnValue, stackedMin);
			}
			return returnValue;
		}

		/// <summary>
		/// Gets minimum Unsigned stacked Y value from many series
		/// </summary>
		/// <param name="valueIndex">Index of Y value to use</param>
		/// <param name="series">Series IDs</param>
		/// <returns>Minimum stacked Y value</returns>
		internal double GetMinUnsignedStackedYValue(int valueIndex, params string[] series)
		{
			double	returnValue = Double.MaxValue;
			double	minValue = Double.MaxValue;
			double	numberOfPoints = GetNumberOfPoints(series);
			for(int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
			{
				double stackedMin = 0;
				double noStackedMin = 0;
				minValue = Double.MaxValue;
				foreach(string seriesName in series)
				{
                    if (this._series[seriesName].Points.Count > pointIndex)
                    {
                        // Take chart type from the series 
                        ChartTypeRegistry chartTypeRegistry = (ChartTypeRegistry)serviceContainer.GetService(typeof(ChartTypeRegistry));
                        IChartType chartType = chartTypeRegistry.GetChartType(this._series[seriesName].ChartTypeName);

                        // If stacked column and bar
                        if (chartType.StackSign || double.IsNaN(this._series[seriesName].Points[pointIndex].YValues[valueIndex]))
                        {
                            continue;
                        }

                        if (chartType.Stacked)
                        {
                            if (this._series[seriesName].Points[pointIndex].YValues[valueIndex] < 0)
                            {
                                stackedMin += this._series[seriesName].Points[pointIndex].YValues[valueIndex];
                                if (stackedMin < minValue)
                                    minValue = stackedMin;
                            }
                        }
                        else
                        {
                            noStackedMin = Math.Min(noStackedMin, this._series[seriesName].Points[pointIndex].YValues[valueIndex]);
                        }
                    }
				}
				minValue = Math.Min(noStackedMin, minValue);
                returnValue = Math.Min(returnValue, minValue);
			}
			return returnValue;
		}

		/// <summary>
		/// Gets minimum stacked X value from many series
		/// </summary>
		/// <param name="series">Series IDs</param>
		/// <returns>Minimum stacked X value</returns>
		internal double GetMinStackedXValue(params string[] series)
		{
			double	returnValue = 0;
			double	numberOfPoints = GetNumberOfPoints(series);
			for(int pointIndex = 0; pointIndex < numberOfPoints; pointIndex++)
			{
				double doubleIndexValue = 0;
				foreach(string seriesName in series)
				{
					if(this._series[seriesName].Points[pointIndex].XValue < 0)
					{
						doubleIndexValue += this._series[seriesName].Points[pointIndex].XValue;
					}
				}
				returnValue = Math.Min(returnValue, doubleIndexValue);
			}
			return returnValue;
		}

		
		/// <summary>
		/// Gets maximum hundred percent stacked Y value
		/// </summary>
		/// <param name="supportNegative">Indicates that negative values are shown on the other side of the axis.</param>
		/// <param name="series">Series names</param>
		/// <returns>Maximum 100% stacked Y value</returns>
		internal double GetMaxHundredPercentStackedYValue(bool supportNegative, params string[] series)
		{
			double	returnValue = 0;

			// Convert array of series names into array of series
			Series[]	seriesArray = new Series[series.Length];
			int			seriesIndex = 0;
			foreach(string seriesName in series)
			{
				seriesArray[seriesIndex++] = this._series[seriesName];
			}

			// Loop through all dat points
			try
			{
				for(int pointIndex = 0; pointIndex < this._series[series[0]].Points.Count; pointIndex++)
				{
					// Calculate the total for all series
					double totalPerPoint = 0;
					double positiveTotalPerPoint = 0;
					foreach(Series ser in seriesArray)
					{
						if(supportNegative)
						{
							totalPerPoint += Math.Abs(ser.Points[pointIndex].YValues[0]);
						}
						else
						{
							totalPerPoint += ser.Points[pointIndex].YValues[0];
						}

						if(ser.Points[pointIndex].YValues[0] > 0 || supportNegative == false)
						{
							positiveTotalPerPoint += ser.Points[pointIndex].YValues[0];
						}
					}
					totalPerPoint = Math.Abs(totalPerPoint);

					// Calculate percentage of total
					if(totalPerPoint != 0)
					{
						returnValue = Math.Max(returnValue, 
							(positiveTotalPerPoint / totalPerPoint) * 100.0);
					}
				}
			}
			catch(System.Exception)
			{
                throw (new InvalidOperationException(SR.ExceptionDataManager100StackedSeriesPointsNumeberMismatch));
			}

			return returnValue;
		}

		/// <summary>
		/// Gets minimum hundred percent stacked Y value
		/// </summary>
		/// <param name="supportNegative">Indicates that negative values are shown on the other side of the axis.</param>
		/// <param name="series">Series names</param>
		/// <returns>Minimum 100% stacked Y value</returns>
		internal double GetMinHundredPercentStackedYValue(bool supportNegative, params string[] series)
		{
			double	returnValue = 0.0;

			// Convert array of series names into array of series
			Series[]	seriesArray = new Series[series.Length];
			int			seriesIndex = 0;
			foreach(string seriesName in series)
			{
				seriesArray[seriesIndex++] = this._series[seriesName];
			}

			// Loop through all dat points
			try
			{
				for(int pointIndex = 0; pointIndex < this._series[series[0]].Points.Count; pointIndex++)
				{
					// Calculate the total for all series
					double totalPerPoint = 0;
					double negativeTotalPerPoint = 0;
					foreach(Series ser in seriesArray)
					{
						if(supportNegative)
						{
							totalPerPoint += Math.Abs(ser.Points[pointIndex].YValues[0]);
						}
						else
						{
							totalPerPoint += ser.Points[pointIndex].YValues[0];
						}

						if(ser.Points[pointIndex].YValues[0] < 0 || supportNegative == false)
						{
							negativeTotalPerPoint += ser.Points[pointIndex].YValues[0];
						}
					}

					totalPerPoint = Math.Abs(totalPerPoint);

					// Calculate percentage of total
					if(totalPerPoint != 0)
					{
						returnValue = Math.Min(returnValue, 
							(negativeTotalPerPoint / totalPerPoint) * 100.0);
					}
				}
			}
			catch(System.Exception)
			{
                throw (new InvalidOperationException(SR.ExceptionDataManager100StackedSeriesPointsNumeberMismatch));
			}

			return returnValue;
		}

		#endregion

		#region DataManager Properties

		/// <summary>
		/// Chart series collection.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
		Editor(Editors.SeriesCollectionEditor.Editor, Editors.SeriesCollectionEditor.Base),
		Bindable(true)
		]
		public SeriesCollection Series
		{
			get
			{
				return _series;
			}
		}

		/// <summary>
		/// Color palette to use
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		SRDescription("DescriptionAttributePalette"),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
        DefaultValue(ChartColorPalette.BrightPastel),
        Editor(Editors.ColorPaletteEditor.Editor, Editors.ColorPaletteEditor.Base)
		]
		public ChartColorPalette Palette
		{
			get
			{
				return _colorPalette;
			}
			set
			{
				_colorPalette = value;
			}
		}

		// Array of custom palette colors.
		private Color[] _paletteCustomColors = new Color[0];

		/// <summary>
		/// Array of custom palette colors.
		/// </summary>
		/// <remarks>
		/// When this custom colors array is non-empty the <b>Palette</b> property is ignored.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		SerializationVisibilityAttribute(SerializationVisibility.Attribute),
		SRDescription("DescriptionAttributeDataManager_PaletteCustomColors"),
		TypeConverter(typeof(ColorArrayConverter))
		]
		public Color[] PaletteCustomColors
		{
			set
			{
				this._paletteCustomColors = value;
			}
			get
			{
				return this._paletteCustomColors;
			}
		}




		#endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_series != null) 
                {
                    _series.Dispose();
                    _series = null;
                }
            }
        }

        #endregion
	}
}
