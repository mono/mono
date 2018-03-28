//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		PieChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	PieChart
//
//  Purpose:	Provides 2D/3D drawing and hit testing functionality 
//              for the Pie chart. A pie chart shows how proportions 
//              of data, shown as pie-shaped pieces, contribute to 
//              the data as a whole.
//              
//              PieChart class is used as a base class for the 
//              DoughnutChart class.
//
//	Reviewed:	GS - Aug 8, 2002
//				AG - Aug 8, 2002
//				AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel.Design;
using System.Globalization;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
#else
	using System.Web.UI.DataVisualization.Charting.Utilities;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.ChartTypes
#else
	namespace System.Web.UI.DataVisualization.Charting.ChartTypes
#endif
{
	#region Enumerations

	/// <summary>
	/// Pie Labels style
	/// </summary>
	internal enum PieLabelStyle
	{ 
		/// <summary>
		/// Labels are inside pie slice
		/// </summary>
		Inside,

		/// <summary>
		/// Labels are outside pie slice
		/// </summary>
		Outside, 

		/// <summary>
		/// Labels are disabled
		/// </summary>
		Disabled, 

	};

	#endregion

	/// <summary>
    /// PieChart class provides 2D/3D drawing and hit testing functionality 
    /// for the Pie chart.
	/// </summary>
	internal class PieChart : IChartType
	{
		#region Enumerations

		/// <summary>
		/// Labels Mode for preparing data
		/// </summary>
		enum LabelsMode
		{
			/// <summary>
			/// There are no labels
			/// </summary>
			Off,

			/// <summary>
			/// Drawing labels mode
			/// </summary>
			Draw,

			/// <summary>
			/// Labels Estimation mode
			/// </summary>
			EstimateSize,

			/// <summary>
			/// Labels Overlap Mode
			/// </summary>
			LabelsOverlap
		};

		#endregion

		#region Fields

		// True if labels fit inside plot area.
		private bool _labelsFit = true;

		// Field that is used to resize pie 
		// because of labels.
		private float _sizeCorrection = 0.95F;

		// True if any pie slice is exploded
		private bool _sliceExploded = false;

		// True if labels overlap for 2D Pie and outside labels
		private bool _labelsOverlap = false;

		// Left Lable column used for 3D chart and outside labels
		internal LabelColumn labelColumnLeft;

		// Right Lable column used for 3D chart and outside labels
		internal LabelColumn labelColumnRight;

		// Array of label rectangles used to prevent labels overlapping 
		// for 2D pie chart outside labels.
		private	ArrayList _labelsRectangles = new ArrayList();

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		virtual public string Name			{ get{ return ChartTypeNames.Pie;}}

		/// <summary>
		/// Gets chart type image.
		/// </summary>
		/// <param name="registry">Chart types registry object.</param>
		/// <returns>Chart type image.</returns>
		virtual public System.Drawing.Image GetImage(ChartTypeRegistry registry)
		{
			return (System.Drawing.Image)registry.ResourceManager.GetObject(this.Name + "ChartType");
		}

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
		virtual public bool RequireAxes	{ get{ return false;} }

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
		virtual public bool SupportLogarithmicAxes	{ get{ return false;} }

		/// <summary>
		/// True if chart type requires to switch the value (Y) axes position
		/// </summary>
		virtual public bool SwitchValueAxes	{ get{ return false;} }

		/// <summary>
		/// True if chart series can be placed side-by-side.
		/// </summary>
		virtual public bool SideBySideSeries { get{ return false;} }

		/// <summary>
		/// If the crossing value is auto Crossing value should be 
		/// automatically set to zero for some chart 
		/// types (Bar, column, area etc.)
		/// </summary>
		virtual public bool ZeroCrossing { get{ return false;} }

		/// <summary>
		/// True if each data point of a chart must be represented in the legend
		/// </summary>
		virtual public bool DataPointsInLegend	{ get{ return true;} }

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
		/// True if palette colors should be applied for each data paoint.
		/// Otherwise the color is applied to the series.
		/// </summary>
		virtual public bool ApplyPaletteColorsToPoints	{ get { return true; } }

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
		virtual public int YValuesPerPoint{ get { return 1; } }

		/// <summary>
		/// Chart is Doughnut or Pie type
		/// </summary>
		virtual public bool Doughnut{ get { return false; } }

		#endregion

		#region Methods

		/// <summary>
		/// Default constructor
		/// </summary>
		public PieChart()
		{
		}



		/// <summary>
		/// Calculates Collected pie slice if required.	
		/// </summary>
		/// <param name="series">Series to be prepared.</param>
		internal static void PrepareData(Series series)
		{
			// Check series chart type
			if( String.Compare(series.ChartTypeName, ChartTypeNames.Pie, StringComparison.OrdinalIgnoreCase ) != 0 && 
				String.Compare(series.ChartTypeName, ChartTypeNames.Doughnut, StringComparison.OrdinalIgnoreCase ) != 0 
                )
			{
				return;
			}

			// Check if collected threshold value is set
			double threshold = 0.0;
            if (series.IsCustomPropertySet(CustomPropertyName.CollectedThreshold))
            {
                double t;
                bool parseSucceed = double.TryParse(series[CustomPropertyName.CollectedThreshold], NumberStyles.Any, CultureInfo.InvariantCulture, out t);
                if (parseSucceed)
                {
                    threshold = t;
                }
                else
                {
                    throw (new InvalidOperationException(SR.ExceptionDoughnutCollectedThresholdInvalidFormat));
                }

                if (threshold < 0.0)
                {
                    throw (new InvalidOperationException(SR.ExceptionDoughnutThresholdInvalid));
                }
            }

			// Check if threshold is set
			if(threshold > 0.0)
			{
                // Get reference to the chart control
				Chart	chart = series.Chart;
				if(chart == null)
				{
                    throw (new InvalidOperationException(SR.ExceptionDoughnutNullReference));
				}

				// Create a temp series which will hold original series data points
				Series seriesOriginalData = new Series("PIE_ORIGINAL_DATA_" + series.Name, series.YValuesPerPoint);
				seriesOriginalData.Enabled = false;
				seriesOriginalData.IsVisibleInLegend = false;
				chart.Series.Add(seriesOriginalData);
				foreach(DataPoint dp in series.Points)
				{
					seriesOriginalData.Points.Add(dp.Clone());
				}

				// Copy temporary design data attribute
				if(series.IsCustomPropertySet("TempDesignData"))
				{
					seriesOriginalData["TempDesignData"] = "true";
				}

				// Calculate total value of all data points. IsEmpty points are
				// ignored. Absolute value is used.
				double total = 0.0;
				foreach(DataPoint dp in series.Points)
				{
					if(!dp.IsEmpty)
					{
						total += Math.Abs(dp.YValues[0]);
					}
				}

				// Check if threshold value is set in percents
				bool percent = true;
				if(series.IsCustomPropertySet(CustomPropertyName.CollectedThresholdUsePercent))
				{
					if(string.Compare(series[CustomPropertyName.CollectedThresholdUsePercent], "True", StringComparison.OrdinalIgnoreCase) == 0)
					{
						percent = true;
					}
                    else if (string.Compare(series[CustomPropertyName.CollectedThresholdUsePercent], "False", StringComparison.OrdinalIgnoreCase) == 0)
					{
						percent = false;
					}
					else
					{
                        throw (new InvalidOperationException(SR.ExceptionDoughnutCollectedThresholdUsePercentInvalid));
					}
				}

				// Convert from percent valur to data point value
				if(percent)
				{
					if(threshold > 100.0)
					{
                        throw (new InvalidOperationException(SR.ExceptionDoughnutCollectedThresholdInvalidRange));
					}

					threshold = total * threshold / 100.0;
				}

				// Count how many points will be collected and remove collected points
				DataPoint collectedPoint = null;
				double collectedTotal = 0.0;
				int collectedCount = 0;
				int firstCollectedPointIndex = 0;
				int originalDataPointIndex = 0;
				for(int dataPointIndex = 0; dataPointIndex < series.Points.Count; dataPointIndex++)
				{
					DataPoint dataPoint = series.Points[dataPointIndex];
					if(!dataPoint.IsEmpty && Math.Abs(dataPoint.YValues[0]) <= threshold)
					{
						// Keep statistics
						++collectedCount;
						collectedTotal += Math.Abs(dataPoint.YValues[0]);

						// Make a template for the collected point using the first removed point
						if(collectedPoint == null)
						{
							firstCollectedPointIndex = dataPointIndex;
							collectedPoint = dataPoint.Clone();
						}

						// Remove first collected point only when second collected point found
						if(collectedCount == 2)
						{
							series.Points.RemoveAt(firstCollectedPointIndex);
							--dataPointIndex;
						}

						// Remove collected point
						if(collectedCount > 1)
						{
							series.Points.RemoveAt(dataPointIndex);
							--dataPointIndex;
						}
					}

					// Set point index that will be used for tooltips
					dataPoint["OriginalPointIndex"] = originalDataPointIndex.ToString(CultureInfo.InvariantCulture);
					++originalDataPointIndex;
				}

				// Add collected data point into the series
				if(collectedCount > 1 && collectedPoint != null)
				{
					collectedPoint["_COLLECTED_DATA_POINT"] = "TRUE";
					collectedPoint.YValues[0] = collectedTotal;
					series.Points.Add(collectedPoint);

					// Set collected point color
					if(series.IsCustomPropertySet(CustomPropertyName.CollectedColor))
					{
						ColorConverter colorConverter = new ColorConverter();
						try
						{
							collectedPoint.Color = (Color)colorConverter.ConvertFromString(null, CultureInfo.InvariantCulture, series[CustomPropertyName.CollectedColor]);
						}
						catch
						{
                            throw (new InvalidOperationException(SR.ExceptionDoughnutCollectedColorInvalidFormat));
						}
					}

					// Set collected point exploded attribute
					if(series.IsCustomPropertySet(CustomPropertyName.CollectedSliceExploded))
					{
						collectedPoint[CustomPropertyName.Exploded] = series[CustomPropertyName.CollectedSliceExploded];
					}

					// Set collected point tooltip
					if(series.IsCustomPropertySet(CustomPropertyName.CollectedToolTip))
					{
						collectedPoint.ToolTip = series[CustomPropertyName.CollectedToolTip];
					}

					// Set collected point legend text
					if(series.IsCustomPropertySet(CustomPropertyName.CollectedLegendText))
					{
						collectedPoint.LegendText = series[CustomPropertyName.CollectedLegendText];
					}
					else
					{
                        collectedPoint.LegendText = SR.DescriptionCustomAttributeCollectedLegendDefaultText;
					}

					// Set collected point label
					if(series.IsCustomPropertySet(CustomPropertyName.CollectedLabel))
					{
						collectedPoint.Label = series[CustomPropertyName.CollectedLabel];
					}
                }
			}
		}

		/// <summary>
		/// Remove any changes done while preparing Pie/Doughnut charts
		/// to draw the collected slice.
		/// </summary>
		/// <param name="series">Series to be un-prepared.</param>
		/// <returns>True if series was removed from collection.</returns>
		internal static bool UnPrepareData(Series series)
		{
			if(series.Name.StartsWith("PIE_ORIGINAL_DATA_", StringComparison.Ordinal))
			{
				// Get reference to the chart control
				Chart	chart = series.Chart;
				if(chart == null)
				{
                    throw (new InvalidOperationException(SR.ExceptionDoughnutNullReference));
				}

				// Get original Renko series
				Series	pieSeries = chart.Series[series.Name.Substring(18)];

				// Copy data back to original Pie series
				pieSeries.Points.Clear();
				if(!series.IsCustomPropertySet("TempDesignData"))
				{
					foreach(DataPoint dp in series.Points)
					{
						pieSeries.Points.Add(dp);
					}
				}

				// Remove series from the collection
				chart.Series.Remove(series);
				return true;
			}
			return false;
		}



		/// <summary>
		/// Paint Pie Chart
		/// </summary>
		/// <param name="graph">The Chart Graphics object</param>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		public void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
		{		
			// Pie chart cannot be combined with other chart types
			foreach( Series series in common.DataManager.Series )
			{
				// Check if series is visible and belong to the current chart area
				if( series.IsVisible() && 
					series.ChartArea == area.Name )
				{
					// Check if series chart type matches
					if( String.Compare( series.ChartTypeName, this.Name, true, System.Globalization.CultureInfo.CurrentCulture ) != 0 )
					{
						if(!common.ChartPicture.SuppressExceptions)
						{
							// Pie/Doughnut chart can not be combined with other chart type
                            throw (new InvalidOperationException(SR.ExceptionChartCanNotCombine( this.Name )));
						}
					}
				}
			}

			// 3D Pie Chart
			if( area.Area3DStyle.Enable3D )
			{

				float pieWidth = 10 * area.Area3DStyle.PointDepth / 100;
				
                // Set Clip Region
				graph.SetClip(area.Position.ToRectangleF());

				// Make reversed X angle because of default angle.
				area.Area3DStyle.Inclination *= -1;
				int oldYAngle = area.Area3DStyle.Rotation;
				area.Area3DStyle.Rotation = area.GetRealYAngle( );

				// Draw Pie
				ProcessChartType3D( false, graph, common, area, pieWidth );

				// Make reversed X angle because of default angle.
				area.Area3DStyle.Inclination *= -1;
				area.Area3DStyle.Rotation = oldYAngle;

				// Reset Clip Region
				graph.ResetClip();

			}
			else
			{
				// Reset overlapped labels flag
				this._labelsOverlap = false;

				//Set Clip Region
				((ChartGraphics)graph).SetClip( area.Position.ToRectangleF() );

				// Resize pie because of labels
				SizeCorrection( graph, common, area );

				// Draw Pie labels
				ProcessChartType( false, graph, common, area, false, LabelsMode.LabelsOverlap );

				// If overlapping labels are detected they will be drawn in "columns" on each
				// side of the pie. Adjust plotting area to fit the labels
				if(this._labelsOverlap)
				{
					// Resize pie because of labels
					SizeCorrection( graph, common, area );

					// Reset overlapped labels flag
					this._labelsOverlap = false;

					// Draw Pie labels
					ProcessChartType( false, graph, common, area, false, LabelsMode.LabelsOverlap );
				}
				
				// Draw Shadow
				ProcessChartType( false, graph, common, area, true, LabelsMode.Off );

				// Draw Pie
				ProcessChartType( false, graph, common, area, false, LabelsMode.Off );

				// Draw Pie labels
				ProcessChartType( false, graph, common, area, false, LabelsMode.Draw );

				//Reset Clip Region
				((ChartGraphics)graph).ResetClip();
			}
		}

		/// <summary>
		/// Take Relative Minimum Pie Size attribute
		/// </summary>
		/// <param name="area">Chart Area</param>
		/// <returns>Custom attribute value.</returns>
		private double MinimumRelativePieSize( ChartArea area )
		{
			// Default value
			double minimumSize = 0.3;

			// All data series from chart area which have Pie chart type
			List<string>	typeSeries = area.GetSeriesFromChartType(Name);

			// Data series collection
			SeriesCollection	dataSeries = area.Common.DataManager.Series;

			// Take Relative Minimum Pie Size attribute
			if(dataSeries[typeSeries[0]].IsCustomPropertySet(CustomPropertyName.MinimumRelativePieSize))
			{
				minimumSize = CommonElements.ParseFloat(dataSeries[typeSeries[0]][CustomPropertyName.MinimumRelativePieSize]) / 100.0;

				// Validation
				if( minimumSize < 0.1 || minimumSize > 0.7 )
                    throw (new ArgumentException(SR.ExceptionPieMinimumRelativePieSizeInvalid));
			
			}

			return minimumSize;
		}

		/// <summary>
		/// Method that is used to resize pie 
		/// because of labels.
		/// </summary>
		private void SizeCorrection( ChartGraphics graph, CommonElements common, ChartArea area )
		{
			float correction = (this._labelsOverlap) ? this._sizeCorrection : 0.95F;
			_sliceExploded = false;

			// Estimate Labels
			if( area.InnerPlotPosition.Auto )
			{
				for( ; correction >= (float)MinimumRelativePieSize( area ); correction -= 0.05F )
				{
					// Decrease Pie size
					this._sizeCorrection = correction;

					// Check if labels fit.
					ProcessChartType( false, graph, common, area, false, LabelsMode.EstimateSize );
					if( _labelsFit )
					{
						break;
					}
				}

				// Size correction for exploded pie can not be larger then 0.8
				if( _sliceExploded && _sizeCorrection > 0.8F )
				{
					_sizeCorrection = 0.8F;
				}
			}
			else
			{
				_sizeCorrection = 0.95F;
			}
		}

		/// <summary>
		/// This method recalculates position of pie slices 
		/// or checks if pie slice is selected.
		/// </summary>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active</param>
		/// <param name="graph">The Chart Graphics object</param>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="shadow">Draw pie shadow</param>
		/// <param name="labels">Pie labels</param>
		private void ProcessChartType( bool selection, ChartGraphics graph, CommonElements common, ChartArea area, bool shadow, LabelsMode labels )
		{
			float startAngle = 0;			// Angle in degrees measured clockwise from the x-axis to the first side of the pie section. 
			string	explodedAttrib = "";	// Exploded attribute
			bool exploded;					// Exploded pie slice
			float midAngle;					// Angle between Start Angle and End Angle
						
			// Data series collection
			SeriesCollection	dataSeries = common.DataManager.Series;

			// Clear Labels overlap collection
			if( labels == LabelsMode.LabelsOverlap )
			{
				_labelsRectangles.Clear();
			}

			// All data series from chart area which have Pie chart type
			List<string>	typeSeries = area.GetSeriesFromChartType(Name);
			if(typeSeries.Count == 0)
			{
				return;
			}

			// Get first pie starting angle
			if(typeSeries.Count > 0)
			{
                if (dataSeries[typeSeries[0]].IsCustomPropertySet(CustomPropertyName.PieStartAngle))
                {
                    float angle;
                    bool parseSucceed = float.TryParse(dataSeries[typeSeries[0]][CustomPropertyName.PieStartAngle], NumberStyles.Any, CultureInfo.InvariantCulture, out angle);
                    if (parseSucceed)
                    {
                        startAngle = angle;
                    }

                    if (!parseSucceed || startAngle > 360f || startAngle < 0f)
                    {
                        throw (new InvalidOperationException(SR.ExceptionCustomAttributeAngleOutOfRange("PieStartAngle")));
                    }
                }
			}

			// Call Back Paint event
			if( !selection )
			{
                common.Chart.CallOnPrePaint(new ChartPaintEventArgs(dataSeries[typeSeries[0]], graph, common, area.PlotAreaPosition));
			}

			// The data points loop. Find Sum of data points.
			double	sum = 0.0;
			foreach( DataPoint point in dataSeries[typeSeries[0]].Points )
			{
				if( !point.IsEmpty )
				{
					sum += Math.Abs( point.YValues[0] );
				}
			}

			// No points or all points have zero values
			if(sum == 0.0)
			{
				return;
			}

			// Take radius attribute
			float	doughnutRadius = 60f;
			if(dataSeries[typeSeries[0]].IsCustomPropertySet(CustomPropertyName.DoughnutRadius))
			{
				doughnutRadius = CommonElements.ParseFloat(dataSeries[typeSeries[0]][CustomPropertyName.DoughnutRadius]);

				// Validation
				if( doughnutRadius < 0f || doughnutRadius > 99f )
                    throw (new ArgumentException(SR.ExceptionPieRadiusInvalid));
			
			}

			// This method is introduced to check colors of palette. For 
			// pie chart the first pie slice and the second pie slice can 
			// not have same color because they are connected.
			CheckPaleteColors( dataSeries[typeSeries[0]].Points );
	
			//************************************************************
			//** Data point loop
			//************************************************************
			int	pointIndx = 0;
			int nonEmptyPointIndex = 0;
			foreach( DataPoint point in dataSeries[typeSeries[0]].Points )
			{	
				// Do not process empty points
				if( point.IsEmpty )
				{
					pointIndx++;
					continue;
				}
				
				// Rectangle size
				RectangleF	rectangle;
				if( area.InnerPlotPosition.Auto )
				{
					rectangle = new RectangleF( area.Position.ToRectangleF().X, area.Position.ToRectangleF().Y, area.Position.ToRectangleF().Width, area.Position.ToRectangleF().Height );
				}
				else
				{
					rectangle = new RectangleF( area.PlotAreaPosition.ToRectangleF().X, area.PlotAreaPosition.ToRectangleF().Y, area.PlotAreaPosition.ToRectangleF().Width, area.PlotAreaPosition.ToRectangleF().Height );
				}
				if(rectangle.Width < 0f || rectangle.Height < 0f)
				{
					return;
				}

				// Find smallest edge
				SizeF absoluteSize = graph.GetAbsoluteSize( new SizeF( rectangle.Width, rectangle.Height ) );
				float absRadius = ( absoluteSize.Width < absoluteSize.Height ) ? absoluteSize.Width : absoluteSize.Height;

				// Size of the square, which will be used for drawing pie. 
				SizeF relativeSize = graph.GetRelativeSize( new SizeF( absRadius, absRadius ) );

				// Center of the pie
				PointF middlePoint = new PointF( rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2 );

				// Rectangle which will always create circle, never ellipse.
				rectangle = new RectangleF( middlePoint.X - relativeSize.Width / 2, middlePoint.Y - relativeSize.Height / 2, relativeSize.Width, relativeSize.Height );

				// Size correction because of exploded or labels
				if( _sizeCorrection != 1 )
				{
					rectangle.X += rectangle.Width * ( 1 - _sizeCorrection ) / 2;
					rectangle.Y += rectangle.Height * ( 1 - _sizeCorrection ) / 2;
					rectangle.Width = rectangle.Width * _sizeCorrection;
					rectangle.Height = rectangle.Height * _sizeCorrection;

					// Adjust inner plot position 
					if(area.InnerPlotPosition.Auto)
					{
						RectangleF rect = rectangle;
						rect.X = (rect.X - area.Position.X) / area.Position.Width * 100f;
						rect.Y = (rect.Y - area.Position.Y) / area.Position.Height * 100f;
						rect.Width = rect.Width / area.Position.Width * 100f;
						rect.Height = rect.Height / area.Position.Height * 100f;
						area.InnerPlotPosition.SetPositionNoAuto(rect.X, rect.Y, rect.Width, rect.Height);
					}
				}

				float	sweepAngle = (float)( Math.Abs(point.YValues[0]) / sum * 360);

				// Check Exploded attribute for data point
				exploded = false;
				if(point.IsCustomPropertySet(CustomPropertyName.Exploded))
				{
					explodedAttrib = point[CustomPropertyName.Exploded];
					if( String.Compare(explodedAttrib,"true", StringComparison.OrdinalIgnoreCase) == 0 )
						exploded = true;
					else
						exploded = false;
				}

				Color pieLineColor = Color.Empty;
				ColorConverter colorConverter = new ColorConverter();

				// Check if special color properties are set
				if(point.IsCustomPropertySet(CustomPropertyName.PieLineColor) || dataSeries[typeSeries[0]].IsCustomPropertySet(CustomPropertyName.PieLineColor))
				{
                    bool failed = false;
                    try
                    {
                        pieLineColor = (Color)colorConverter.ConvertFromString(
                            (point.IsCustomPropertySet(CustomPropertyName.PieLineColor)) ? point[CustomPropertyName.PieLineColor] : dataSeries[typeSeries[0]][CustomPropertyName.PieLineColor]);
                        failed = false;
                    }
                    catch (ArgumentException)
                    {
                        failed = true;
                    }
                    catch (NotSupportedException)
                    {
                        failed = true;
                    }

                    if (failed)
                    {
                        pieLineColor = (Color)colorConverter.ConvertFromInvariantString(
    (point.IsCustomPropertySet(CustomPropertyName.PieLineColor)) ? point[CustomPropertyName.PieLineColor] : dataSeries[typeSeries[0]][CustomPropertyName.PieLineColor]);
                    }
				}
 
				// Find Direction to move exploded pie slice
				if( exploded )
				{
					_sliceExploded = true;
					midAngle = ( 2 * startAngle + sweepAngle ) / 2;
					double xComponent = Math.Cos( midAngle * Math.PI / 180 ) * rectangle.Width / 10;
					double yComponent = Math.Sin( midAngle * Math.PI / 180 ) * rectangle.Height / 10;

					rectangle.Offset( (float)xComponent, (float)yComponent );
				}

				// Hot regions of the data points. Labels hot regions are processed aftre drawing.
				if( common.ProcessModeRegions && labels == LabelsMode.Draw )
				{
					Map( common, point, startAngle, sweepAngle, rectangle, Doughnut, doughnutRadius, graph, pointIndx );
				}
				
				// Painting mode
				if( common.ProcessModePaint )
				{
					// Draw Shadow
					if( shadow )
					{
						double offset = graph.GetRelativeSize( new SizeF( point.series.ShadowOffset, point.series.ShadowOffset ) ).Width;

						// Offset is zero. Do not draw shadow pie slice.
						if( offset == 0.0 )
						{
							break;
						}

						// Shadow Rectangle
						RectangleF shadowRect = new RectangleF( rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height );
						shadowRect.Offset( (float)offset, (float)offset ); 

						// Change shadow color
						Color shcolor = new Color();
						Color shGradientColor = new Color();
						Color shBorderColor = new Color();

						// Solid color
						if( point.Color.A != 255 )
							shcolor = Color.FromArgb( point.Color.A/2, point.series.ShadowColor );
						else
							shcolor = point.series.ShadowColor;

						// Gradient Color
						if( !point.BackSecondaryColor.IsEmpty )
						{
							if( point.BackSecondaryColor.A != 255 )
								shGradientColor = Color.FromArgb( point.BackSecondaryColor.A/2, point.series.ShadowColor );
							else
								shGradientColor = point.series.ShadowColor;
						}
						else
							shGradientColor = Color.Empty;

						// Border color
						if( !point.BorderColor.IsEmpty )
						{
							if( point.BorderColor.A != 255 )
								shBorderColor = Color.FromArgb( point.BorderColor.A/2, point.series.ShadowColor );
							else
								shBorderColor = point.series.ShadowColor;
						}
						else
							shBorderColor = Color.Empty;

						// Draw shadow of pie slice
						graph.DrawPieRel(	
							shadowRect, 
							startAngle, 
							sweepAngle,
							shcolor, 
							ChartHatchStyle.None, 
							"", 
							point.BackImageWrapMode, 
							point.BackImageTransparentColor,
							point.BackGradientStyle, 
							shGradientColor, 
							shBorderColor, 
							point.BorderWidth, 
							point.BorderDashStyle, 
							true,
							Doughnut,
							doughnutRadius,
							PieDrawingStyle.Default);
					}
					else
					{
						if( labels == LabelsMode.Off )
						{
							// Start Svg Selection mode
							graph.StartHotRegion( point );

							// Draw pie slice
							graph.DrawPieRel(	
								rectangle, 
								startAngle, 
								sweepAngle,
								point.Color, 
								point.BackHatchStyle, 
								point.BackImage, 
								point.BackImageWrapMode, 
								point.BackImageTransparentColor,
								point.BackGradientStyle, 
								point.BackSecondaryColor, 
								point.BorderColor, 
								point.BorderWidth, 
								point.BorderDashStyle, 
								false,
								Doughnut,
								doughnutRadius,
								ChartGraphics.GetPieDrawingStyle(point) );

							// End Svg Selection mode
							graph.EndHotRegion( );
						}
					}

					// Estimate labels
					if( labels == LabelsMode.EstimateSize )
					{
						EstimateLabels( graph, middlePoint, rectangle.Size, startAngle, sweepAngle, point, exploded, area );
						if( _labelsFit == false )
						{
							return;
						}
					}

					// Labels overlap test
					if( labels == LabelsMode.LabelsOverlap )
					{
						DrawLabels( graph, middlePoint, rectangle.Size, startAngle, sweepAngle, point, doughnutRadius, exploded, area, true, nonEmptyPointIndex, pieLineColor );
					}

					// Draw labels and markers
					if( labels == LabelsMode.Draw )
					{
						DrawLabels( graph, middlePoint, rectangle.Size, startAngle, sweepAngle, point, doughnutRadius, exploded, area, false, nonEmptyPointIndex, pieLineColor );
					}

				}

				if( common.ProcessModeRegions && labels == LabelsMode.Draw )
				{
					// Add labels hot regions if it was not done during the painting
					if( !common.ProcessModePaint )
					{
						DrawLabels( graph, middlePoint, rectangle.Size, startAngle, sweepAngle, point, doughnutRadius, exploded, area, false, nonEmptyPointIndex, pieLineColor );
					}
				}
			

				//**************************************************
				//** Remember point relative position
				//**************************************************
				point.positionRel = new PointF(float.NaN, float.NaN);


				// If exploded the shift is bigger
				float expShift = 1;
				if( exploded )
					expShift = 1.2F;

				midAngle = startAngle + sweepAngle / 2;

				// Find first line position
				point.positionRel.X = (float)Math.Cos( (midAngle) * Math.PI / 180 ) * rectangle.Width * expShift / 2 + middlePoint.X;
				point.positionRel.Y = (float)Math.Sin( (midAngle) * Math.PI / 180 ) * rectangle.Height * expShift / 2 + middlePoint.Y;

				// Increase point index and sweep angle
				pointIndx++;
				nonEmptyPointIndex++;
				startAngle += sweepAngle;
				if(startAngle >= 360)
				{
					startAngle -= 360;
				}
			}

			if( labels == LabelsMode.LabelsOverlap && this._labelsOverlap )
			{
				this._labelsOverlap = PrepareLabels( area.Position.ToRectangleF() );
			}
					
			// Call Paint event
			if( !selection )
			{
                common.Chart.CallOnPostPaint(new ChartPaintEventArgs(dataSeries[typeSeries[0]], graph, common, area.PlotAreaPosition));
			}
		}
		
		/// <summary>
		/// Draw Pie labels or test for overlaping.
		/// </summary>
		/// <param name="graph">Chart Graphics object</param>
		/// <param name="middlePoint">Center of the pie chart</param>
		/// <param name="relativeSize">Size of the square, which will be used for drawing pie.</param>
		/// <param name="startAngle">Starting angle of a pie slice</param>
		/// <param name="sweepAngle">Sweep angle of a pie slice</param>
		/// <param name="point">Data point</param>
		/// <param name="doughnutRadius">Radius for Doughnut Chart in %</param>
		/// <param name="exploded">The pie slice is exploded</param>
		/// <param name="area">Chart area</param>
		/// <param name="overlapTest">True if test mode is on</param>
		/// <param name="pointIndex">Data Point Index</param>
		/// <param name="pieLineColor">Color of line labels</param>
		public void DrawLabels( ChartGraphics graph, PointF middlePoint, SizeF relativeSize, float startAngle, float sweepAngle, DataPoint point, float doughnutRadius, bool exploded, ChartArea area, bool overlapTest, int pointIndex, Color pieLineColor )
		{
			bool added = false;	// Indicates that label position was added
			float x; // Label Position
			float y; // Label Position
			Series series; // Data Series
			float labelsHorizontalLineSize = 1; // Horizontal line size for outside labels
			float labelsRadialLineSize = 1; // Radial line size for outside labels
			string text;

			// Disable the clip region
			Region oldClipRegion = graph.Clip;
			graph.Clip = new Region();

			// Get label text
			text = this.GetLabelText( point );
			if(text.Length == 0)
			{
				return;
			}

			float shift;

			series = point.series;

			PieLabelStyle style = PieLabelStyle.Inside;

			// Get label style attribute from series
			if(series.IsCustomPropertySet(CustomPropertyName.LabelStyle))
			{
				string labelStyleAttrib = series[CustomPropertyName.LabelStyle];

				// Labels Disabled
				if( String.Compare(labelStyleAttrib,"disabled",StringComparison.OrdinalIgnoreCase) == 0 )
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}
			else if(series.IsCustomPropertySet(CustomPropertyName.PieLabelStyle))
			{
				string labelStyleAttrib = series[CustomPropertyName.PieLabelStyle];

				// Labels Disabled
                if (String.Compare(labelStyleAttrib, "disabled", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}

			// Get label style attribute from point
			if(point.IsCustomPropertySet(CustomPropertyName.LabelStyle))
			{
				string labelStyleAttrib = point[CustomPropertyName.LabelStyle];

				// Labels Disabled
                if (String.Compare(labelStyleAttrib, "disabled", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}
			// Get label style attribute from point
			else if(point.IsCustomPropertySet(CustomPropertyName.PieLabelStyle))
			{
				string labelStyleAttrib = point[CustomPropertyName.PieLabelStyle];

				// Labels Disabled
                if (String.Compare(labelStyleAttrib, "disabled", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}


			// Take labels radial line size attribute from series
			if(series.IsCustomPropertySet(CustomPropertyName.LabelsRadialLineSize))
			{
				string labelsRadialLineSizeAttrib = series[CustomPropertyName.LabelsRadialLineSize];
				labelsRadialLineSize = CommonElements.ParseFloat( labelsRadialLineSizeAttrib); 

				// Validation
				if( labelsRadialLineSize < 0 || labelsRadialLineSize > 100 )
                    throw new InvalidOperationException(SR.ExceptionPieRadialLineSizeInvalid);
			}

			// Take labels radial line size attribute from point
			if(point.IsCustomPropertySet(CustomPropertyName.LabelsRadialLineSize))
			{
				string labelsRadialLineSizeAttrib = point[CustomPropertyName.LabelsRadialLineSize];
				labelsRadialLineSize = CommonElements.ParseFloat( labelsRadialLineSizeAttrib); 

				// Validation
				if( labelsRadialLineSize < 0 || labelsRadialLineSize > 100 )
                    throw new InvalidOperationException(SR.ExceptionPieRadialLineSizeInvalid);
			}

			// Take labels horizontal line size attribute from series
			if(series.IsCustomPropertySet(CustomPropertyName.LabelsHorizontalLineSize))
			{
				string labelsHorizontalLineSizeAttrib = series[CustomPropertyName.LabelsHorizontalLineSize];
				labelsHorizontalLineSize = CommonElements.ParseFloat( labelsHorizontalLineSizeAttrib); 

				// Validation
				if( labelsHorizontalLineSize < 0 || labelsHorizontalLineSize > 100 )
                    throw new InvalidOperationException(SR.ExceptionPieHorizontalLineSizeInvalid);
			}

			// Take labels horizontal line size attribute from point
			if(point.IsCustomPropertySet(CustomPropertyName.LabelsHorizontalLineSize))
			{
				string labelsHorizontalLineSizeAttrib = point[CustomPropertyName.LabelsHorizontalLineSize];
				labelsHorizontalLineSize = CommonElements.ParseFloat( labelsHorizontalLineSizeAttrib); 

				// Validation
				if( labelsHorizontalLineSize < 0 || labelsHorizontalLineSize > 100 )
                    throw new InvalidOperationException(SR.ExceptionPieHorizontalLineSizeInvalid);
			}

			float expShift = 1;

			// ********************************************
			// Labels are set inside pie
			// ********************************************
			if( style == PieLabelStyle.Inside && !overlapTest ) 
			{
				float width;
				float height;

				// If exploded the shift is bigger
				if( exploded )
				{
					expShift = 1.4F;
				}

				// Get offset of the inside labels position
				// NOTE: This custom attribute is NOT released!
				float positionRatio = 4.0f;
				if(point.IsCustomPropertySet("InsideLabelOffset"))
				{
                    bool parseSucceed = float.TryParse(point["InsideLabelOffset"], NumberStyles.Any, CultureInfo.InvariantCulture, out positionRatio);
					if(!parseSucceed || positionRatio < 0f || positionRatio > 100f)
					{
						throw(new InvalidOperationException(SR.ExceptionCustomAttributeIsNotInRange0to100("InsideLabelOffset")));
					}
					positionRatio = 4f / (1f + positionRatio / 100f);
				}
				

				// Shift the string for Doughnut type
				if( Doughnut )
				{
					width = relativeSize.Width * expShift / positionRatio * ( 1 + ( 100 - doughnutRadius ) / 100F );
					height = relativeSize.Height * expShift / positionRatio * ( 1 + ( 100 - doughnutRadius ) / 100F );
				}
				else
				{
					width = relativeSize.Width * expShift / positionRatio;
					height = relativeSize.Height * expShift / positionRatio;
				}

				// Find string position
				x = (float)Math.Cos( (startAngle + sweepAngle / 2) * Math.PI / 180 ) * width + middlePoint.X;
				y = (float)Math.Sin( (startAngle + sweepAngle / 2) * Math.PI / 180 ) * height + middlePoint.Y;

				// Center the string horizontally and vertically.
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    SizeF sizeFont = graph.GetRelativeSize(
                        graph.MeasureString(
                        text.Replace("\\n", "\n"),
                        point.Font,
                        new SizeF(1000f, 1000f),
                        StringFormat.GenericTypographic));

                    // Get label background position
                    RectangleF labelBackPosition = RectangleF.Empty;
                    SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
                    sizeLabel.Height += sizeLabel.Height / 8;
                    sizeLabel.Width += sizeLabel.Width / text.Length;
                    labelBackPosition = PointChart.GetLabelPosition(
                        graph,
                        new PointF(x, y),
                        sizeLabel,
                        format,
                        true);

                    // Draw the label inside the pie
                    using (Brush brush = new SolidBrush(point.LabelForeColor))
                    {
                        graph.DrawPointLabelStringRel(
                            area.Common,
                            text,
                            point.Font,
                            brush,
                            new PointF(x, y),
                            format,
                            point.LabelAngle,
                            labelBackPosition,
                            point.LabelBackColor,
                            point.LabelBorderColor,
                            point.LabelBorderWidth,
                            point.LabelBorderDashStyle,
                            series,
                            point,
                            pointIndex);
                    }
                }
			}

			// ********************************************
			// Labels are set outside pie
			// ********************************************
			else if( style == PieLabelStyle.Outside )
			{

				// Coefficient which represent shift from pie border
				shift = 0.5F + labelsRadialLineSize * 0.1F;

				// If exploded the shift is bigger
				if( exploded )
					expShift = 1.2F;

				float midAngle = startAngle + sweepAngle / 2;

				// Find first line position
				float x1 = (float)Math.Cos( (midAngle) * Math.PI / 180 ) * relativeSize.Width * expShift / 2 + middlePoint.X;
				float y1 = (float)Math.Sin( (midAngle) * Math.PI / 180 ) * relativeSize.Height * expShift / 2 + middlePoint.Y;

				float x2 = (float)Math.Cos( (midAngle) * Math.PI / 180 ) * relativeSize.Width * shift * expShift + middlePoint.X;
				float y2 = (float)Math.Sin( (midAngle) * Math.PI / 180 ) * relativeSize.Height * shift * expShift + middlePoint.Y;

				if( pieLineColor == Color.Empty )
				{
					pieLineColor = point.BorderColor;
				}

				// Draw first line
				if( !overlapTest )
				{
					graph.DrawLineRel( pieLineColor, point.BorderWidth, ChartDashStyle.Solid, new PointF( x1, y1 ), new PointF( x2, y2 ) );
				}

				// Set string alingment
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    // Find second line position
                    float y3 = (float)Math.Sin((midAngle) * Math.PI / 180) * relativeSize.Height * shift * expShift + middlePoint.Y;
                    float x3;
                    float x3Overlap;

                    RectangleF labelRect = RectangleF.Empty;
                    RectangleF labelRectOver = RectangleF.Empty;

                    if (midAngle > 90 && midAngle < 270)
                    {
                        format.Alignment = StringAlignment.Far;
                        x3Overlap = -relativeSize.Width * shift * expShift + middlePoint.X - relativeSize.Width / 10 * labelsHorizontalLineSize;
                        x3 = (float)Math.Cos((midAngle) * Math.PI / 180) * relativeSize.Width * shift * expShift + middlePoint.X - relativeSize.Width / 10 * labelsHorizontalLineSize;

                        if (overlapTest)
                        {
                            x3Overlap = x3;
                        }

                        // This method returns calculated rectangle from point position 
                        // for outside label. Rectangle mustn’t be out of chart area.
                        labelRect = GetLabelRect(new PointF(x3, y3), area, text, format, graph, point, true);
                        labelRectOver = GetLabelRect(new PointF(x3Overlap, y3), area, text, format, graph, point, true);
                    }
                    else
                    {
                        format.Alignment = StringAlignment.Near;

                        x3Overlap = relativeSize.Width * shift * expShift + middlePoint.X + relativeSize.Width / 10 * labelsHorizontalLineSize;
                        x3 = (float)Math.Cos((midAngle) * Math.PI / 180) * relativeSize.Width * shift * expShift + middlePoint.X + relativeSize.Width / 10 * labelsHorizontalLineSize;

                        if (overlapTest)
                        {
                            x3Overlap = x3;
                        }

                        // This method returns calculated rectangle from point position 
                        // for outside label. Rectangle mustn’t be out of chart area.
                        labelRect = GetLabelRect(new PointF(x3, y3), area, text, format, graph, point, false);
                        labelRectOver = GetLabelRect(new PointF(x3Overlap, y3), area, text, format, graph, point, false);
                    }

                    // Draw second line
                    if (!overlapTest)
                    {
                        if (this._labelsOverlap)
                        {
                            float calculatedY3 = (((RectangleF)this._labelsRectangles[pointIndex]).Top + ((RectangleF)this._labelsRectangles[pointIndex]).Bottom) / 2f;
                            graph.DrawLineRel(pieLineColor, point.BorderWidth, ChartDashStyle.Solid, new PointF(x2, y2), new PointF(x3Overlap, calculatedY3));
                        }
                        else
                        {
                            graph.DrawLineRel(pieLineColor, point.BorderWidth, ChartDashStyle.Solid, new PointF(x2, y2), new PointF(x3, y3));
                        }
                    }

                    // Draw the string
                    if (!overlapTest)
                    {
                        RectangleF rect = new RectangleF(labelRect.Location, labelRect.Size);
                        if (this._labelsOverlap)
                        {
                            // Draw label from collection if original labels overlap.
                            rect = (RectangleF)this._labelsRectangles[pointIndex];
                            rect.X = labelRectOver.X;
                            rect.Width = labelRectOver.Width;
                        }

                        // Get label background position
                        SizeF valueTextSize = graph.MeasureStringRel(text.Replace("\\n", "\n"), point.Font);
                        valueTextSize.Height += valueTextSize.Height / 8;
                        float spacing = valueTextSize.Width / text.Length / 2;
                        valueTextSize.Width += spacing;
                        RectangleF labelBackPosition = new RectangleF(
                            rect.X,
                            rect.Y + rect.Height / 2f - valueTextSize.Height / 2f,
                            valueTextSize.Width,
                            valueTextSize.Height);

                        // Adjust position based on alignment
                        if (format.Alignment == StringAlignment.Near)
                        {
                            labelBackPosition.X -= spacing / 2f;
                        }
                        else if (format.Alignment == StringAlignment.Center)
                        {
                            labelBackPosition.X = rect.X + (rect.Width - valueTextSize.Width) / 2f;
                        }
                        else if (format.Alignment == StringAlignment.Far)
                        {
                            labelBackPosition.X = rect.Right - valueTextSize.Width - spacing / 2f;
                        }

                        // Draw label text outside
                        using (Brush brush = new SolidBrush(point.LabelForeColor))
                        {
                            graph.DrawPointLabelStringRel(
                                area.Common,
                                text,
                                point.Font,
                                brush,
                                rect,
                                format,
                                point.LabelAngle,
                                labelBackPosition,
                                point.LabelBackColor,
                                point.LabelBorderColor,
                                point.LabelBorderWidth,
                                point.LabelBorderDashStyle,
                                series,
                                point,
                                pointIndex);
                        }
                    }
                    else
                    {
                        // Insert labels in label collection. This 
                        // code is executed only if labels overlap.
                        this.InsertOverlapLabel(labelRectOver);
                        added = true;
                    }
                }
			}
			// Restore old clip region
			graph.Clip = oldClipRegion;


			// Add empty overlap empty position
			if(!added)
			{
				InsertOverlapLabel( RectangleF.Empty );
			}

			return;
		
		}

		
		/// <summary>
		/// This method returns calculated rectangle from point position 
		/// for outside label. Rectangle mustn’t be out of chart area.
		/// </summary>
		/// <param name="labelPosition">The first position for label</param>
		/// <param name="area">Chart area used for chart area position</param>
		/// <param name="text">Label text</param>
		/// <param name="format">Text format</param>
		/// <param name="graph">Chart Graphics object</param>
		/// <param name="point">Data point</param>
		/// <param name="leftOrientation">Orientation for label. It could be left or right.</param>
		/// <returns>Calculated rectangle for label</returns>
		private RectangleF GetLabelRect( PointF labelPosition, ChartArea area, string text, StringFormat format, ChartGraphics graph, DataPoint point, bool leftOrientation )
		{
			RectangleF labelRect = RectangleF.Empty;
			if( leftOrientation )
			{
				labelRect.X = area.Position.X;
				labelRect.Y = area.Position.Y;
				labelRect.Width = labelPosition.X - area.Position.X;
				labelRect.Height = area.Position.Height;
			}
			else
			{
				labelRect.X = labelPosition.X;
				labelRect.Y = area.Position.Y;
				labelRect.Width = area.Position.Right - labelPosition.X;
				labelRect.Height = area.Position.Height;
			}

			// Find bounding rectangle of the text
			SizeF size = graph.MeasureStringRel( text.Replace("\\n", "\n"), point.Font, labelRect.Size, format );
			labelRect.Y = labelPosition.Y - size.Height / 2 * 1.8f;
			labelRect.Height = size.Height * 1.8f;

			return labelRect;
		}

		

		/// <summary>
		/// This method returns Pie Label Style enumeration 
		/// from Data Point Custom attribute.
		/// </summary>
		/// <param name="point">Data Point</param>
		/// <returns>Pie label style enumeration</returns>
		private PieLabelStyle GetLabelStyle( DataPoint point )
		{
			Series series = point.series;

			PieLabelStyle style = PieLabelStyle.Inside;

			// Get label style attribute from series
			if(series.IsCustomPropertySet(CustomPropertyName.LabelStyle))
			{
				string labelStyleAttrib = series[CustomPropertyName.LabelStyle];

				// Labels Disabled
                if (String.Compare(labelStyleAttrib, "disabled", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}
			else if(series.IsCustomPropertySet(CustomPropertyName.PieLabelStyle))
			{
				string labelStyleAttrib = series[CustomPropertyName.PieLabelStyle];

				// Labels Disabled
                if (String.Compare(labelStyleAttrib, "disabled", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}

			// Get label style attribute from point
			if(point.IsCustomPropertySet(CustomPropertyName.LabelStyle))
			{
				string labelStyleAttrib = point[CustomPropertyName.LabelStyle];

				// Labels Disabled
                if (String.Compare(labelStyleAttrib, "disabled", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}
			else if(point.IsCustomPropertySet(CustomPropertyName.PieLabelStyle))
			{
				string labelStyleAttrib = point[CustomPropertyName.PieLabelStyle];

				// Labels Disabled
                if (String.Compare(labelStyleAttrib, "disabled", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}

			return style;
		}
        
		/// <summary>
		/// Estimate Labels.
		/// </summary>
		/// <param name="graph">Chart Graphics object</param>
		/// <param name="middlePoint">Center of the pie chart</param>
		/// <param name="relativeSize">Size of the square, which will be used for drawing pie.</param>
		/// <param name="startAngle">Starting angle of a pie slice</param>
		/// <param name="sweepAngle">Sweep angle of a pie slice</param>
		/// <param name="point">Data point</param>
		/// <param name="exploded">The pie slice is exploded</param>
		/// <param name="area">Chart area</param>
		public bool EstimateLabels( ChartGraphics graph, PointF middlePoint, SizeF relativeSize, float startAngle, float sweepAngle, DataPoint point, bool exploded, ChartArea area )
		{
			float labelsHorizontalLineSize = 1; // Horizontal line size for outside labels
			float labelsRadialLineSize = 1; // Radial line size for outside labels
			float shift;

			string pointLabel = this.GetPointLabel(point);
			
			Series	series = point.series;
			
			PieLabelStyle style = PieLabelStyle.Inside;

			// Get label style attribute from series
			if(series.IsCustomPropertySet(CustomPropertyName.LabelStyle))
			{
				string labelStyleAttrib = series[CustomPropertyName.LabelStyle];

				// Labels Disabled
				if( String.Compare(labelStyleAttrib,"disabled", StringComparison.OrdinalIgnoreCase) == 0 )
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}
			else if(series.IsCustomPropertySet(CustomPropertyName.PieLabelStyle))
			{
				string labelStyleAttrib = series[CustomPropertyName.PieLabelStyle];

				// Labels Disabled
                if (String.Compare(labelStyleAttrib, "disabled", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}

			// Get label style attribute from point
			if(point.IsCustomPropertySet(CustomPropertyName.LabelStyle))
			{
				string labelStyleAttrib = point[CustomPropertyName.LabelStyle];

				// Labels Disabled
                if (String.Compare(labelStyleAttrib, "disabled", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}
			else if(point.IsCustomPropertySet(CustomPropertyName.PieLabelStyle))
			{
				string labelStyleAttrib = point[CustomPropertyName.PieLabelStyle];

				// Labels Disabled
                if (String.Compare(labelStyleAttrib, "disabled", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Disabled;
                else if (String.Compare(labelStyleAttrib, "outside", StringComparison.OrdinalIgnoreCase) == 0)
					style = PieLabelStyle.Outside;
				else
					style = PieLabelStyle.Inside;
			}

			// Take labels radial line size attribute from series
			if(series.IsCustomPropertySet(CustomPropertyName.LabelsRadialLineSize))
			{
				string labelsRadialLineSizeAttrib = series[CustomPropertyName.LabelsRadialLineSize];
				labelsRadialLineSize = CommonElements.ParseFloat( labelsRadialLineSizeAttrib ); 

				// Validation
				if( labelsRadialLineSize < 0 || labelsRadialLineSize > 100 )
                    throw new InvalidOperationException(SR.ExceptionPieRadialLineSizeInvalid);
			}

			// Take labels radial line size attribute from point
			if(point.IsCustomPropertySet(CustomPropertyName.LabelsRadialLineSize))
			{
				string labelsRadialLineSizeAttrib = point[CustomPropertyName.LabelsRadialLineSize];
				labelsRadialLineSize = CommonElements.ParseFloat( labelsRadialLineSizeAttrib ); 

				// Validation
				if( labelsRadialLineSize < 0 || labelsRadialLineSize > 100 )
                    throw new InvalidOperationException(SR.ExceptionPieRadialLineSizeInvalid);
			}

			// Take labels horizontal line size attribute from series
			if(series.IsCustomPropertySet(CustomPropertyName.LabelsHorizontalLineSize))
			{
				string labelsHorizontalLineSizeAttrib = series[CustomPropertyName.LabelsHorizontalLineSize];
				labelsHorizontalLineSize = CommonElements.ParseFloat( labelsHorizontalLineSizeAttrib ); 

				// Validation
				if( labelsHorizontalLineSize < 0 || labelsHorizontalLineSize > 100 )
                    throw new InvalidOperationException(SR.ExceptionPieHorizontalLineSizeInvalid);
			}

			// Take labels horizontal line size attribute from point
			if(point.IsCustomPropertySet(CustomPropertyName.LabelsHorizontalLineSize))
			{
				string labelsHorizontalLineSizeAttrib = point[CustomPropertyName.LabelsHorizontalLineSize];
				labelsHorizontalLineSize = CommonElements.ParseFloat( labelsHorizontalLineSizeAttrib ); 

				// Validation
				if( labelsHorizontalLineSize < 0 || labelsHorizontalLineSize > 100 )
                    throw new InvalidOperationException(SR.ExceptionPieHorizontalLineSizeInvalid);
			}

			float expShift = 1;

			
			// ********************************************
			// Labels are set outside pie
			// ********************************************
			if( style == PieLabelStyle.Outside )
			{
				// Coefficient which represent shift from pie border
				shift = 0.5F + labelsRadialLineSize * 0.1F;

				// If exploded the shift is bigger
				if( exploded )
					expShift = 1.2F;

				float midAngle = startAngle + sweepAngle / 2;


				// Find second line position
				float y3 = (float)Math.Sin( (midAngle) * Math.PI / 180 ) * relativeSize.Height * shift * expShift + middlePoint.Y;
				float x3;

				if( midAngle > 90 && midAngle < 270 )
				{
					x3 = (float)Math.Cos( (midAngle) * Math.PI / 180 ) * relativeSize.Width * shift * expShift + middlePoint.X - relativeSize.Width / 10 * labelsHorizontalLineSize;
				}
				else
				{
					x3 = (float)Math.Cos( (midAngle) * Math.PI / 180 ) * relativeSize.Width * shift * expShift + middlePoint.X + relativeSize.Width / 10 * labelsHorizontalLineSize;
				}

				// Get label text
				string text;
				if( pointLabel.Length == 0 && point.IsValueShownAsLabel )
				{
					text = ValueConverter.FormatValue(
						series.Chart,
						point,
                        point.Tag,
						point.YValues[0], 
						point.LabelFormat, 
						point.series.YValueType,
						ChartElementType.DataPoint);
				}
				else
				{
					text = pointLabel;
				}

				SizeF size = graph.MeasureStringRel( text.Replace("\\n", "\n"), point.Font);

				_labelsFit = true;

				if(this._labelsOverlap)
				{
					if( midAngle > 90 && midAngle < 270 )
					{
						float xOverlap = -relativeSize.Width * shift * expShift + middlePoint.X - relativeSize.Width / 10 * labelsHorizontalLineSize;
						if( (xOverlap - size.Width) < area.Position.X )
						{
							_labelsFit = false;
						}
					}
					else
					{
						float xOverlap = relativeSize.Width * shift * expShift + middlePoint.X + relativeSize.Width / 10 * labelsHorizontalLineSize;
						if( (xOverlap + size.Width) > area.Position.Right )
						{
							_labelsFit = false;
						}
					}
				}
				else
				{
					if( midAngle > 90 && midAngle < 270 )
					{
						if( x3 - size.Width < area.PlotAreaPosition.ToRectangleF().Left )
							_labelsFit = false;
					}
					else
					{
						if( x3 + size.Width > area.PlotAreaPosition.ToRectangleF().Right )
							_labelsFit = false;
					}

					if( midAngle > 180 && midAngle < 360 )
					{
						if( y3 - size.Height/2 < area.PlotAreaPosition.ToRectangleF().Top )
							_labelsFit = false;
					}
					else
					{
						if( y3 + size.Height/2 > area.PlotAreaPosition.ToRectangleF().Bottom )
							_labelsFit = false;
					}
				}

			}
			return true;
		}
		
		/// <summary>
		/// This method adds map area information.
		/// </summary>
		/// <param name="common">The Common elements object</param>
		/// <param name="point">Data Point</param>
		/// <param name="startAngle">Start Angle</param>
		/// <param name="sweepAngle">Sweep Angle</param>
		/// <param name="rectangle">Rectangle of the pie</param>
		/// <param name="doughnut">True if doughnut</param>
		/// <param name="doughnutRadius">Doughnut radius in %</param>
		/// <param name="graph">Chart graphics object</param>
		/// <param name="pointIndex">Data point index</param>
		private void Map( CommonElements common, DataPoint point, float startAngle, float sweepAngle, RectangleF rectangle, bool doughnut, float doughnutRadius, ChartGraphics graph, int pointIndex )
		{
			// Create a graphics path
            using (GraphicsPath path = new GraphicsPath())
            {

                // Create the interior doughnut rectangle
                RectangleF doughnutRect = RectangleF.Empty;

                doughnutRect.X = rectangle.X + rectangle.Width * (1 - (100 - doughnutRadius) / 100) / 2;
                doughnutRect.Y = rectangle.Y + rectangle.Height * (1 - (100 - doughnutRadius) / 100) / 2;
                doughnutRect.Width = rectangle.Width * (100 - doughnutRadius) / 100;
                doughnutRect.Height = rectangle.Height * (100 - doughnutRadius) / 100;

                // Get absolute coordinates of the pie rectangle
                rectangle = graph.GetAbsoluteRectangle(rectangle);

                // Add the pie to the graphics path
                path.AddPie(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, startAngle, sweepAngle);
                // VSTS #250394 (Dev10:591140) Fix - Control should not return “useless” map areas
                if (sweepAngle <= 0)
                {
                    return;
                }
                // If the chart type is doughnut
                if (doughnut)
                {

                    // Get absolute coordinates of the interior doughnut rectangle
                    doughnutRect = graph.GetAbsoluteRectangle(doughnutRect);

                    // Add the interior doughnut region to the graphics path
                    path.AddPie(doughnutRect.X, doughnutRect.Y, doughnutRect.Width, doughnutRect.Width, startAngle, sweepAngle);
                }

                // Make a polygon from curves
                path.Flatten(new Matrix(), 1f);

                // Create an area of points and convert them to 
                // relative coordinates.
                PointF[] pointNew = new PointF[path.PointCount];
                for (int i = 0; i < path.PointCount; i++)
                {
                    pointNew[i] = graph.GetRelativePoint(path.PathPoints[i]);
                }

                // Allocate array of floats
                float[] coord = new float[path.PointCount * 2];

                // Transfer path points
                for (int index = 0; index < path.PointCount; index++)
                {
                    coord[2 * index] = pointNew[index].X;
                    coord[2 * index + 1] = pointNew[index].Y;
                }



                // Check if processing collected data point
                if (point.IsCustomPropertySet("_COLLECTED_DATA_POINT"))
                {
                    // Add point to the map area
                    common.HotRegionsList.AddHotRegion(
                        graph,
                        path,
                        false,
                        point.ReplaceKeywords(point.ToolTip),
#if Microsoft_CONTROL
					string.Empty,
					string.Empty,
					string.Empty,
#else // Microsoft_CONTROL
 point.ReplaceKeywords(point.Url),
                        point.ReplaceKeywords(point.MapAreaAttributes),
                        point.ReplaceKeywords(point.PostBackValue),
#endif // Microsoft_CONTROL
 point,
                        ChartElementType.DataPoint);

                    return;
                }



                // Add points to the map area
                common.HotRegionsList.AddHotRegion(
                    path,
                    false,
                    coord,
                    point,
                    point.series.Name,
                    pointIndex
                    );
            }
		}
				
		/// <summary>
		/// This method is introduced to check colors of palette. For 
		/// pie chart the first pie slice and the second pie slice can 
		/// not have same color because they are connected.
		/// </summary>
		/// <param name="points">Data points used for pie chart</param>
		private void CheckPaleteColors( DataPointCollection points )
		{
			DataPoint firstPoint, lastPoint;

			firstPoint = points[0];
			lastPoint = points[ points.Count - 1 ];

			// Change color for last point if same as the first and if it is from pallete.
			if( firstPoint.tempColorIsSet && lastPoint.tempColorIsSet && firstPoint.Color == lastPoint.Color )
			{
				lastPoint.Color = points[ points.Count / 2 ].Color;
				lastPoint.tempColorIsSet = true;
			}
		}

		#endregion

		#region 2DLabels

		/// <summary>
		/// This method finds vertical position for left and 
		/// right labels on that way that labels do not 
		/// overlap each other.
		/// </summary>
		/// <param name="area">Chart area position</param>
		/// <returns>True if it is possible to find position that labels do not overlap each other.</returns>
		private bool PrepareLabels( RectangleF area )
		{
			// Initialization of local variables
			float splitPoint = area.X + area.Width / 2f;
			int numberOfLeft = 0;
			int numberOfRight = 0;

			// Find the number of left and right labels.
			foreach( RectangleF rect in this._labelsRectangles )
			{
				if( rect.X < splitPoint )
				{
					numberOfLeft++;
				}
				else
				{
					numberOfRight++;
				}
			}

			// **********************************************
			// Find the best position for LEFT labels
			// **********************************************
			bool leftResult = true;
			if(numberOfLeft > 0)
			{
				double [] startPoints = new double[numberOfLeft];
				double [] endPoints = new double[numberOfLeft];
				int [] positionIndex = new Int32[numberOfLeft];
			
				// Fill double arrays with Top and Bottom coordinates 
				// from the label rectangle.
				int splitIndex = 0;
				for( int index = 0; index < _labelsRectangles.Count; index++ )
				{
					RectangleF rect = (RectangleF)_labelsRectangles[index];
					if( rect.X < splitPoint )
					{
						startPoints[ splitIndex ] = rect.Top;
						endPoints[ splitIndex ] = rect.Bottom;
						positionIndex[ splitIndex ] = index;
						splitIndex++;
					}
				}
			
				// Sort label positions
				this.SortIntervals( startPoints, endPoints, positionIndex );

				// Find no overlapping positions if possible.
				if( this.ArrangeOverlappingIntervals( startPoints, endPoints, area.Top, area.Bottom ) )
				{
					// Fill label rectangle top and bottom coordinates 
					// from double arrays.
					splitIndex = 0;
					for( int index = 0; index < _labelsRectangles.Count; index++ )
					{
						RectangleF rect = (RectangleF)_labelsRectangles[index];
						if( rect.X < splitPoint )
						{
							rect.Y = (float)startPoints[ splitIndex ];
							rect.Height = (float)(endPoints[ splitIndex ] - rect.Top);
							_labelsRectangles[positionIndex[ splitIndex ]] = rect;
							splitIndex++;
					
						}
					}
				}
				else
				{
					leftResult = false;
				}
			}

			// **********************************************
			// Find the best position for Right labels
			// **********************************************
			bool rigthResult = true;
			if(numberOfRight > 0)
			{
				double [] startPoints = new double[numberOfRight];
				double [] endPoints = new double[numberOfRight];
				int [] positionIndex = new Int32[numberOfRight];

				// Fill double arrays with Top and Bottom coordinates 
				// from the label rectangle.
				int splitIndex = 0;
				for( int index = 0; index < _labelsRectangles.Count; index++ )
				{
					RectangleF rect = (RectangleF)_labelsRectangles[index];
					if( rect.X >= splitPoint )
					{
						startPoints[ splitIndex ] = rect.Top;
						endPoints[ splitIndex ] = rect.Bottom;
						positionIndex[ splitIndex ] = index;
						splitIndex++;
					}
				}
			
				// Sort label positions
				this.SortIntervals( startPoints, endPoints, positionIndex );

				// Find no overlapping positions if possible.
				if( this.ArrangeOverlappingIntervals( startPoints, endPoints, area.Top, area.Bottom ) )
				{
					// Fill label rectangle top and bottom coordinates 
					// from double arrays.
					splitIndex = 0;
					for( int index = 0; index < _labelsRectangles.Count; index++ )
					{
						RectangleF rect = (RectangleF)_labelsRectangles[index];
						if( rect.X >= splitPoint )
						{
							rect.Y = (float)startPoints[ splitIndex ];
							rect.Height = (float)(endPoints[ splitIndex ] - rect.Top);
							_labelsRectangles[positionIndex[ splitIndex ]] = rect;
							splitIndex++;
						}
					}
				}
				else
				{
					rigthResult = false;
				}
			}

			return ( (!leftResult || !rigthResult) ? true : false );
		}

		/// <summary>
		/// This algorithm sorts labels vertical intervals.
		/// </summary>
		/// <param name="startOfIntervals">Double array of label interval start points</param>
		/// <param name="endOfIntervals">Double array of label interval end points</param>
		/// <param name="positinIndex">Integer array of label interval indexes</param>
		private void SortIntervals( double [] startOfIntervals, double [] endOfIntervals, int [] positinIndex )
		{
			double firstCenter;
			double secondCenter;
			double midDouble;
			int midInt;

			// Sorting loops
			for( int firstIndex = 0; firstIndex < startOfIntervals.Length; firstIndex++ )
			{
				for( int secondIndex = firstIndex; secondIndex < startOfIntervals.Length; secondIndex++ )
				{
					firstCenter = ( startOfIntervals[ firstIndex ] + endOfIntervals[ firstIndex ] ) / 2.0;
					secondCenter = ( startOfIntervals[ secondIndex ] + endOfIntervals[ secondIndex ] ) / 2.0;
					
					if( firstCenter > secondCenter )
					{
						// Sort start points
						midDouble = startOfIntervals[ firstIndex ];
						startOfIntervals[ firstIndex ] = startOfIntervals[ secondIndex ];
						startOfIntervals[ secondIndex ] = midDouble;

						// Sort end points
						midDouble = endOfIntervals[ firstIndex ];
						endOfIntervals[ firstIndex ] = endOfIntervals[ secondIndex ];
						endOfIntervals[ secondIndex ] = midDouble;

						// Sort indexes
						midInt = positinIndex[ firstIndex ];
						positinIndex[ firstIndex ] = positinIndex[ secondIndex ];
						positinIndex[ secondIndex ] = midInt;
					}
				}
			}
		}
		
		/// <summary>
		/// This method inserts label rectangles 
		/// into the collection.
		/// </summary>
		/// <param name="labelRect">Label Rectangle</param>
		private void InsertOverlapLabel( RectangleF labelRect )
		{
			// Check if any pair of labels overlap
			if(!labelRect.IsEmpty)
			{
				foreach( RectangleF rect in _labelsRectangles )
				{
					if( labelRect.IntersectsWith( rect ) )
					{
						this._labelsOverlap = true;
					}
				}
			}

			// Add rectangle to the collection
			_labelsRectangles.Add( labelRect );
		}

		/// <summary>
		/// This method will find the best position for labels. 
		/// It is based on finding non overlap intervals for 
		/// left or right side of the pie. This is 
		/// recursive algorithm.
		/// </summary>
		/// <param name="startOfIntervals">The start positions of intervals.</param>
		/// <param name="endOfIntervals">The end positions of intervals.</param>
		/// <param name="startArea">Start position of chart area vertical range.</param>
		/// <param name="endArea">End position of chart area vertical range.</param>
		/// <returns>False if non overlapping positions for intervals can not be found.</returns>
		private bool ArrangeOverlappingIntervals( double [] startOfIntervals, double [] endOfIntervals, double startArea, double endArea )
		{
	
			// Invalidation
			if( startOfIntervals.Length != endOfIntervals.Length )
			{
                throw new InvalidOperationException(SR.ExceptionPieIntervalsInvalid);
			}
						
			ShiftOverlappingIntervals( startOfIntervals, endOfIntervals );
			
			// Find amount of empty space between intervals.
			double emptySpace = 0;
			for( int intervalIndex = 0; intervalIndex < startOfIntervals.Length - 1; intervalIndex++ )
			{
				// Check overlapping
				if( startOfIntervals[ intervalIndex + 1 ] < endOfIntervals[ intervalIndex ] )
				{
                    //throw new InvalidOperationException( SR.ExceptionPieIntervalsOverlapping );
				}

				emptySpace += startOfIntervals[ intervalIndex + 1 ] -  endOfIntervals[ intervalIndex ];
			}

			//Find how much intervals are out of area. Out of area could be positive value only.
			double outOfArea = ( endOfIntervals[ endOfIntervals.Length - 1 ] - endArea ) + ( startArea - startOfIntervals[ 0 ] );
			if( outOfArea <= 0 )
			{
				// This algorithm shifts all intervals for the same 
				// amount. It is trying to put all intervals inside 
				// chart area range.
				ShiftIntervals( startOfIntervals, endOfIntervals, startArea, endArea );
				return true;
			}

			// There is no enough space for all intervals.
			if( outOfArea > emptySpace )
			{
				return false;
			}

			// This method reduces empty space between intervals. 
			ReduceEmptySpace( startOfIntervals, endOfIntervals, ( emptySpace - outOfArea ) / emptySpace );

			// This algorithm shifts all intervals for the same 
			// amount. It is trying to put all intervals inside 
			// chart area range.
			ShiftIntervals( startOfIntervals, endOfIntervals, startArea, endArea );

			return true;
		}

		/// <summary>
		/// This method reduces empty space between intervals. 
		/// </summary>
		/// <param name="startOfIntervals">The start positions of intervals.</param>
		/// <param name="endOfIntervals">The end positions of intervals.</param>
		/// <param name="reduction">Relative value which presents size reduction.</param>
		private void ReduceEmptySpace( double [] startOfIntervals, double [] endOfIntervals, double reduction )
		{
			for( int intervalIndex = 0; intervalIndex < startOfIntervals.Length - 1; intervalIndex++ )
			{
				// Check overlapping
				if( startOfIntervals[ intervalIndex + 1 ] < endOfIntervals[ intervalIndex ] )
				{
                    //throw new InvalidOperationException( SR.ExceptionPieIntervalsOverlapping );
				}

				// Reduce space
				double shift = ( startOfIntervals[ intervalIndex + 1 ] -  endOfIntervals[ intervalIndex ] ) - ( startOfIntervals[ intervalIndex + 1 ] -  endOfIntervals[ intervalIndex ] ) * reduction;
				for( int reductionIndex = intervalIndex + 1; reductionIndex < startOfIntervals.Length; reductionIndex++ )
				{
					startOfIntervals[ reductionIndex ] -= shift;
					endOfIntervals[ reductionIndex ] -= shift;
				}
			}
		}

		/// <summary>
		/// This algorithm shifts all intervals for the same 
		/// amount. It is trying to put all intervals inside 
		/// chart area range.
		/// </summary>
		/// <param name="startOfIntervals">The start positions of intervals.</param>
		/// <param name="endOfIntervals">The end positions of intervals.</param>
		/// <param name="startArea">Start position of chart area vertical range.</param>
		/// <param name="endArea">End position of chart area vertical range.</param>
		private void ShiftIntervals( double [] startOfIntervals, double [] endOfIntervals, double startArea, double endArea )
		{

			double shift = 0;

			if( startOfIntervals[ 0 ] < startArea )
			{
				shift = startArea - startOfIntervals[ 0 ];
			}
			else if( endOfIntervals[ endOfIntervals.Length - 1 ] > endArea )
			{
				shift = endArea - endOfIntervals[ endOfIntervals.Length - 1 ];
			}

			for( int index = 0; index < startOfIntervals.Length; index++ )
			{
				startOfIntervals[ index ] += shift;
				endOfIntervals[ index ] += shift;
			}
		}

		/// <summary>
		/// This is used to find non overlapping position for intervals.
		/// </summary>
		/// <param name="startOfIntervals">The start positions of intervals.</param>
		/// <param name="endOfIntervals">The end positions of intervals.</param>
		/// <returns>Returns true if any label overlaps before method is used.</returns>
		private void ShiftOverlappingIntervals( double [] startOfIntervals, double [] endOfIntervals )
		{
			// Invalidation
			if( startOfIntervals.Length != endOfIntervals.Length )
			{
                throw new InvalidOperationException(SR.ExceptionPieIntervalsInvalid);
			}

			// Find first overlaping intervals
			for( int index = 0; index < startOfIntervals.Length - 1; index++ )
			{
				// Intervals overlap
				if( endOfIntervals[ index ] > startOfIntervals[ index + 1 ] )
				{
					double overlapRange = endOfIntervals[ index ] - startOfIntervals[ index + 1 ];
					SpreadInterval( startOfIntervals, endOfIntervals, index, Math.Floor( overlapRange / 2.0 ) );
				}
			}
		}

		/// <summary>
		/// This method spread all intervals down or up from 
		/// splitIndex. Intervals are spread only if there is no 
		/// empty space which will compensate shifting of intervals.
		/// </summary>
		/// <param name="startOfIntervals">The start positions of intervals.</param>
		/// <param name="endOfIntervals">The end positions of intervals.</param>
		/// <param name="splitIndex">Position of the interval which ovelap.</param>
		/// <param name="overlapShift">The half of the overlapping range.</param>
		private void SpreadInterval( double [] startOfIntervals, double [] endOfIntervals, int splitIndex, double overlapShift )
		{
			// Move first overlapping intervals.
			endOfIntervals[ splitIndex ] -= overlapShift;
			startOfIntervals[ splitIndex ] -= overlapShift;

			endOfIntervals[ splitIndex + 1 ] += overlapShift;
			startOfIntervals[ splitIndex + 1 ] += overlapShift;

			// Move up other intervals if there is no enough empty space 
			// to compensate overlapping intervals.
			if( splitIndex > 0 )
			{
				for( int index = splitIndex - 1; index >= 0; index-- )
				{
					if( endOfIntervals[ index ] > startOfIntervals[ index + 1 ] - overlapShift )
					{
						endOfIntervals[ index ] -= overlapShift;
						startOfIntervals[ index ] -= overlapShift;
					}
					else
					{
						break;
					}
				}
			}

			// Move down other intervals if there is no enough empty space 
			// to compensate overlapping intervals.
			if( splitIndex + 2 < startOfIntervals.Length - 1 )
			{
				for( int index = splitIndex + 2; index < startOfIntervals.Length; index++ )
				{
					if( startOfIntervals[ index ] > endOfIntervals[ index - 1 ] + overlapShift )
					{
						endOfIntervals[ index ] += overlapShift;
						startOfIntervals[ index ] += overlapShift;
					}
					else
					{
						break;
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

		#region 3D painting and selection methods

		/// <summary>
		/// This method recalculates position of pie slices 
		/// or checks if pie slice is selected.
		/// </summary>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active</param>
		/// <param name="graph">The Chart Graphics object</param>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="pieWidth">Pie width.</param>
		private void ProcessChartType3D( 
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			float pieWidth )
		{
			string	explodedAttrib = "";					// Exploded attribute
			bool exploded;									// Exploded pie slice
			float midAngle;									// Angle between Start Angle and End Angle

					
			// Data series collection
			SeriesCollection	dataSeries = common.DataManager.Series;

			// All data series from chart area which have Pie chart type
			List<string>	typeSeries = area.GetSeriesFromChartType(Name);

			if( typeSeries.Count == 0 )
			{
				return;
			}

			// Get first pie starting angle
            if (dataSeries[typeSeries[0]].IsCustomPropertySet(CustomPropertyName.PieStartAngle))
            {
                int angle;
                bool parseSucceed = int.TryParse(dataSeries[typeSeries[0]][CustomPropertyName.PieStartAngle], NumberStyles.Any, CultureInfo.InvariantCulture, out angle);

                if (parseSucceed)
                {
                    if (angle > 180 && angle <= 360)
                    {
                        angle = -(360 - angle);
                    }
                    area.Area3DStyle.Rotation = angle;
                }


                if (!parseSucceed || area.Area3DStyle.Rotation > 180 || area.Area3DStyle.Rotation < -180)
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeAngleOutOfRange("PieStartAngle")));
                }
            }
						
			// Call Back Paint event
			if( !selection )
			{
                common.Chart.CallOnPrePaint(new ChartPaintEventArgs(dataSeries[typeSeries[0]], graph, common, area.PlotAreaPosition));
			}

			// The data points loop. Find Sum of data points.
			double	sum = 0;
			foreach( DataPoint point in dataSeries[typeSeries[0]].Points )
			{
				if( !point.IsEmpty )
				{
					sum += Math.Abs(point.YValues[0]);
				}
			}

			// Is exploded if only one is exploded
			bool isExploded = false;
			foreach( DataPoint point in dataSeries[typeSeries[0]].Points )
			{
				if(point.IsCustomPropertySet(CustomPropertyName.Exploded))
				{
					explodedAttrib = point[CustomPropertyName.Exploded];
					if( String.Compare(explodedAttrib,"true",StringComparison.OrdinalIgnoreCase) == 0 )
					{
						isExploded = true;
					}
				}
			}

			// Take radius attribute
			float	doughnutRadius = 60f;
			if(dataSeries[typeSeries[0]].IsCustomPropertySet(CustomPropertyName.DoughnutRadius))
			{
				doughnutRadius = CommonElements.ParseFloat(dataSeries[typeSeries[0]][CustomPropertyName.DoughnutRadius] );

				// Validation
				if( doughnutRadius < 0f || doughnutRadius > 99f )
                    throw (new ArgumentException(SR.ExceptionPieRadiusInvalid));
			
			}

			// Take 3D Label Line Size attribute
			float	labelLineSize = 100f;
			if(dataSeries[typeSeries[0]].IsCustomPropertySet(CustomPropertyName._3DLabelLineSize))
			{
				labelLineSize = CommonElements.ParseFloat(dataSeries[typeSeries[0]][CustomPropertyName._3DLabelLineSize] );

				// Validation
				if( labelLineSize < 30f || labelLineSize > 200f )
                    throw (new ArgumentException(SR.ExceptionPie3DLabelLineSizeInvalid));
			
			}
			labelLineSize = labelLineSize * 0.1F / 100F;
	
			//************************************************************
			//** Data point loop
			//************************************************************
			float [] startAngleList;
			float [] sweepAngleList;
			int [] pointIndexList;

			// This method is introduced to check colors of palette. For 
			// pie chart the first pie slice and the second pie slice can 
			// not have same color because they are connected.
			CheckPaleteColors( dataSeries[typeSeries[0]].Points );
	
			bool sameBackFront;
			DataPoint [] points = PointOrder( dataSeries[typeSeries[0]], area, out startAngleList, out sweepAngleList, out pointIndexList, out sameBackFront );

			// There are no points or all points are empty.
			if( points == null )
			{
				return;
			}

			RectangleF plotingRectangle = new RectangleF( area.Position.ToRectangleF().X + 1, area.Position.ToRectangleF().Y + 1, area.Position.ToRectangleF().Width-2, area.Position.ToRectangleF().Height-2 );

			// Check if any data point has outside label
			bool outside = false;
			foreach( DataPoint point in points )
			{
				if( GetLabelStyle( point ) == PieLabelStyle.Outside )
				{
					outside = true;
				}
			}

			// If outside labels resize Pie size
			if( outside )
			{
				InitPieSize( graph, area, ref plotingRectangle, ref pieWidth, points, startAngleList, sweepAngleList, dataSeries[typeSeries[0]], labelLineSize );
			}

			// Initialize Matrix 3D
			area.matrix3D.Initialize(
				plotingRectangle, 
				pieWidth, 
				area.Area3DStyle.Inclination,
				0F,
				0,
				false);

			//***********************************************************
			//** Initialize Lighting
			//***********************************************************
			area.matrix3D.InitLight( 
				area.Area3DStyle.LightStyle
				);

			// Turns are introduce because of special case – Big pie slice, which 
			// is bigger, then 180 degree and it is back and 
			// front point in same time. If special case exists drawing has to be split 
			// into 4 parts: 1. Drawing back pie slices, 2. Drawing the first part of 
			// big slice and other points, 3. Drawing second part of big slice and 
			// 4. Drawing top of the pie slices. 
			for( int turn = 0; turn < 5; turn++ )
			{
				int	pointIndx = 0;
				foreach( DataPoint point in points )
				{
                    // Reset point anchor location
                    point.positionRel = PointF.Empty;

					// Do not process empty points
					if( point.IsEmpty )
					{
						pointIndx++;
						continue;
					}

					float	sweepAngle = sweepAngleList[pointIndx];
					float	startAngle = startAngleList[pointIndx];
					
					// Rectangle size
					RectangleF	rectangle;
					if( area.InnerPlotPosition.Auto )
						rectangle = new RectangleF( plotingRectangle.X, plotingRectangle.Y, plotingRectangle.Width, plotingRectangle.Height );
					else
						rectangle = new RectangleF( area.PlotAreaPosition.ToRectangleF().X, area.PlotAreaPosition.ToRectangleF().Y, area.PlotAreaPosition.ToRectangleF().Width, area.PlotAreaPosition.ToRectangleF().Height );

					// Find smallest edge
					SizeF absoluteSize = graph.GetAbsoluteSize( new SizeF( rectangle.Width, rectangle.Height ) );
					float absRadius = ( absoluteSize.Width < absoluteSize.Height ) ? absoluteSize.Width : absoluteSize.Height;

					// Size of the square, which will be used for drawing pie. 
					SizeF relativeSize = graph.GetRelativeSize( new SizeF( absRadius, absRadius ) );

					// Center of the pie
					PointF middlePoint = new PointF( rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2 );

					// Rectangle which will always create circle, never ellipse.
					rectangle = new RectangleF( middlePoint.X - relativeSize.Width / 2, middlePoint.Y - relativeSize.Height / 2, relativeSize.Width, relativeSize.Height );
								
					// Check Exploded attribute for data point
					exploded = false;
					if(point.IsCustomPropertySet(CustomPropertyName.Exploded))
					{
						explodedAttrib = point[CustomPropertyName.Exploded];
						if( String.Compare(explodedAttrib,"true",StringComparison.OrdinalIgnoreCase) == 0 )
							exploded = true;
						else
							exploded = false;
					}

					// Size correction because of exploded or labels
					float sizeCorrection = 1.0F;
					if( isExploded )
					{
						sizeCorrection = 0.82F;

						rectangle.X += rectangle.Width * ( 1 - sizeCorrection ) / 2;
						rectangle.Y += rectangle.Height * ( 1 - sizeCorrection ) / 2;
						rectangle.Width = rectangle.Width * sizeCorrection;
						rectangle.Height = rectangle.Height * sizeCorrection;
					}

 
					// Find Direction to move exploded pie slice
					if( exploded )
					{
						_sliceExploded = true;
						midAngle = ( 2 * startAngle + sweepAngle ) / 2;
						double xComponent = Math.Cos( midAngle * Math.PI / 180 ) * rectangle.Width / 10;
						double yComponent = Math.Sin( midAngle * Math.PI / 180 ) * rectangle.Height / 10;

						rectangle.Offset( (float)xComponent, (float)yComponent );
					}

					// Adjust inner plot position 
					if(area.InnerPlotPosition.Auto)
					{
						RectangleF rect = rectangle;
						rect.X = (rect.X - area.Position.X) / area.Position.Width * 100f;
						rect.Y = (rect.Y - area.Position.Y) / area.Position.Height * 100f;
						rect.Width = rect.Width / area.Position.Width * 100f;
						rect.Height = rect.Height / area.Position.Height * 100f;
						area.InnerPlotPosition.SetPositionNoAuto(rect.X, rect.Y, rect.Width, rect.Height);
					}

					// Start Svg Selection mode
					graph.StartHotRegion( point );

					// Drawing or selection of pie clice
					Draw3DPie( turn, graph, point, area, rectangle, startAngle, sweepAngle, doughnutRadius, pieWidth, sameBackFront, exploded, pointIndexList[pointIndx] );

					// End Svg Selection mode
					graph.EndHotRegion( );
				
					if( turn == 1 )
					{
						// Outside labels
						if( GetLabelStyle( point ) == PieLabelStyle.Outside )
						{
							FillPieLabelOutside( graph, area, rectangle, pieWidth, point, startAngle, sweepAngle, pointIndx, doughnutRadius, exploded );
						}
					}
					if( turn == 2 )
					{
						
						// Outside labels
						if( GetLabelStyle( point ) == PieLabelStyle.Outside && pointIndx == 0 )
						{
							labelColumnLeft.Sort();
							labelColumnLeft.AdjustPositions();
							labelColumnRight.Sort();
							labelColumnRight.AdjustPositions();
						}
						
					}

					// Increae point index
					pointIndx++;
				}
			}
					
			// Call Paint event
			if( !selection )
			{
                common.Chart.CallOnPostPaint(new ChartPaintEventArgs(dataSeries[typeSeries[0]], graph, common, area.PlotAreaPosition));
			}
		}

		/// <summary>
		/// This method draws a part of a pie slice. Which part is drown 
		/// depend on turn. There is special case if there is a big pie 
		/// slice (>180) when one pie slice has to be split on parts 
		/// and between that other small pie slices has to be drawn.
		/// </summary>
		/// <param name="turn">Turn for drawing.</param>
		/// <param name="graph">Chart Graphics</param>
		/// <param name="point">Data Point to draw</param>
		/// <param name="area">Chart area</param>
		/// <param name="rectangle">Rectangle used for drawing pie clice.</param>
		/// <param name="startAngle">Start angle for pie slice</param>
		/// <param name="sweepAngle">End angle for pie slice</param>
		/// <param name="doughnutRadius">Inner Radius if chart is doughnut</param>
		/// <param name="pieWidth">Width of the pie</param>
		/// <param name="sameBackFront">Pie slice is >180 and same pie slice is back and front slice</param>
		/// <param name="exploded">Pie slice is exploded</param>
		/// <param name="pointIndex">Point Index</param>
		private void Draw3DPie( 
			int turn, 
			ChartGraphics graph, 
			DataPoint point, 
			ChartArea area, 
			RectangleF rectangle, 
			float startAngle, 
			float sweepAngle, 
			float doughnutRadius, 
			float pieWidth, 
			bool sameBackFront, 
			bool exploded,
			int pointIndex
			)
		{
			SolidBrush brush = new SolidBrush(point.Color);

			// For lightStyle style Non, Border color always exist.
			Color penColor = Color.Empty;
			Color penCurveColor = Color.Empty;

			if( point.BorderColor == Color.Empty && area.Area3DStyle.LightStyle == LightStyle.None )
			{
				penColor = ChartGraphics.GetGradientColor( point.Color, Color.Black, 0.5 );
			}
			else if( point.BorderColor == Color.Empty )
			{
				penColor = point.Color;
			}
			else
			{
				penColor = point.BorderColor;
			}

			if( point.BorderColor != Color.Empty || area.Area3DStyle.LightStyle == LightStyle.None )
			{
				penCurveColor = penColor;
			}

			Pen pen = new Pen(penColor, point.BorderWidth);
			pen.DashStyle = graph.GetPenStyle( point.BorderDashStyle );

			// Pen for back side slice.
			Pen backSlicePen;
			if( point.BorderColor == Color.Empty )
			{
				backSlicePen = new Pen(point.Color);
			}
			else
			{
				backSlicePen = pen;
			}

			Pen penCurve = new Pen(penCurveColor, point.BorderWidth);
			penCurve.DashStyle = graph.GetPenStyle( point.BorderDashStyle );

			// Set Border Width;
			PointF [] points = GetPiePoints( graph, area, pieWidth, rectangle, startAngle, sweepAngle, true, doughnutRadius, exploded );

			if( points == null )
				return;

            // Remember data point anchor location
            point.positionRel.X = points[(int)PiePoints.TopLabelLine].X;
            point.positionRel.Y = points[(int)PiePoints.TopLabelLine].Y;
            point.positionRel = graph.GetRelativePoint(point.positionRel);


			float midAngle = startAngle + sweepAngle / 2F;
			float endAngle = startAngle + sweepAngle;
			
			if( turn == 0 )
			{
				// Draw back pie slice (do not fill). 
				// Used for transparency.
				if( !this.Doughnut )
				{
					graph.FillPieSlice( 
						area,
						point,
						brush,
						backSlicePen,
						points[(int)PiePoints.BottomRectTopLeftPoint], 
						points[(int)PiePoints.BottomStart], 
						points[(int)PiePoints.BottomRectBottomRightPoint],
						points[(int)PiePoints.BottomEnd], 
						points[(int)PiePoints.BottomCenter],
						startAngle,
						sweepAngle,
						false,
						pointIndex
						);	
				}
				else
				{
					graph.FillDoughnutSlice( 
						area,
						point,
						brush,
						backSlicePen,
						points[(int)PiePoints.BottomRectTopLeftPoint], 
						points[(int)PiePoints.BottomStart], 
						points[(int)PiePoints.BottomRectBottomRightPoint],
						points[(int)PiePoints.BottomEnd], 
						points[(int)PiePoints.DoughnutBottomEnd],
						points[(int)PiePoints.DoughnutBottomStart],
						startAngle,
						sweepAngle,
						false,
						doughnutRadius,
						pointIndex
						);	
				}

			}
			else if( turn == 1 )
			{
				// Case when there is big pie slice ( > 180 ) and big slice is 
				// back and front point in same time.
				if( sameBackFront )
				{
		

					// Draw the first part of the curve of the big slice and 
					// all curves from other slices. Big pie slice could be on the 
					// right or the left side.
					if( midAngle > -90 && midAngle < 90 || midAngle > 270 && midAngle < 450 )
					{
						// Draw Inner Arc for Doughnut
						if( Doughnut )
						{
							DrawDoughnutCurves( graph, area, point, startAngle, sweepAngle, points, brush, penCurve, false, true, pointIndex );
						}

						DrawPieCurves( graph, area, point, startAngle, sweepAngle, points, brush, penCurve, true, true, pointIndex );
					}
					else
					{
						// Draw Inner Arc for Doughnut
						if( Doughnut )
						{
							DrawDoughnutCurves( graph, area, point, startAngle, sweepAngle, points, brush, penCurve, true, true, pointIndex );
						}

						DrawPieCurves( graph, area, point, startAngle, sweepAngle, points, brush, penCurve, false, true, pointIndex );
					}

					// Draw sides of pie slices
					graph.FillPieSides( area, area.Area3DStyle.Inclination, startAngle, sweepAngle, points, brush, pen, Doughnut );

					
				}
				else
				{
					// Draw Inner Arc for Doughnut
					if( Doughnut )
					{
						DrawDoughnutCurves( graph, area, point, startAngle, sweepAngle, points, brush, penCurve, false, false, pointIndex );
					}

					// This is regular case. There is no big pie slice 
					// which is back nad front point in same time.
					graph.FillPieSides( area, area.Area3DStyle.Inclination, startAngle, sweepAngle, points, brush, pen, Doughnut );

					DrawPieCurves( graph, area, point, startAngle, sweepAngle, points, brush, penCurve, false, false, pointIndex );
				}

			}
			else if( turn == 2 )
			{
				// This second turned is used only for big pie slice (>180). If big pie 
				// slice exist it has to be split if it is necessary. If the big pie slice 
				// cover other pie slice from both sides, the big pie slice have to curves. 
				// The first curve from big pie slice is drawn first, after that all other 
				// pie slices and at the end second curve from big pie slice.
				if( sameBackFront && sweepAngle > 180 )
				{
					// Condition when two draw Doughnut arcs after sides for big pie slice ( > 180 ).
					bool BackFrontDoughnut = ( startAngle > -180 && startAngle < 0 || startAngle > 180 && startAngle < 360 ) &&	( endAngle > -180 && endAngle < 0 || endAngle > 180 && endAngle < 360 );

					if( area.Area3DStyle.Inclination > 0 )
						BackFrontDoughnut = !BackFrontDoughnut;

					if( midAngle > -90 && midAngle < 90 || midAngle > 270 && midAngle < 450 )
					{
						// Draw Inner Arc for Doughnut
						if( Doughnut )
						{
							// Draw second part of doughnut curve only for very big slices > 300 ( Visibility issue for Big point depth ).
							if( BackFrontDoughnut && sweepAngle > 300 )
							{
								DrawDoughnutCurves( graph, area, point, startAngle, sweepAngle, points, brush, penCurve, true, true, pointIndex );
							}
						}

						DrawPieCurves( graph, area, point, startAngle, sweepAngle, points, brush, penCurve, false, true, pointIndex );
					}
					else
					{
						// Draw Inner Arc for Doughnut
						if( Doughnut )
						{
							// Draw second part of doughnut curve only for very big slices > 300( Visibility issue for Big point depth ).
							if( BackFrontDoughnut && sweepAngle > 300 )
							{
								DrawDoughnutCurves( graph, area, point, startAngle, sweepAngle, points, brush, penCurve, false, true, pointIndex );
							}
						}
						DrawPieCurves( graph, area, point, startAngle, sweepAngle, points, brush, penCurve, true, true, pointIndex );
					}
				}
			}
			else if( turn == 3 )
			{
				if( !this.Doughnut )
				{
					// Fill pie slice
					graph.FillPieSlice( 
						area,
						point,
						brush,
						pen,
						points[(int)PiePoints.TopRectTopLeftPoint], 
						points[(int)PiePoints.TopStart], 
						points[(int)PiePoints.TopRectBottomRightPoint],
						points[(int)PiePoints.TopEnd], 
						points[(int)PiePoints.TopCenter],
						startAngle,
						sweepAngle,
						true,
						pointIndex
						);	
					
					// Draw Border
					graph.FillPieSlice( 
						area,
						point,
						brush,
						pen,
						points[(int)PiePoints.TopRectTopLeftPoint], 
						points[(int)PiePoints.TopStart], 
						points[(int)PiePoints.TopRectBottomRightPoint],
						points[(int)PiePoints.TopEnd], 
						points[(int)PiePoints.TopCenter],
						startAngle,
						sweepAngle,
						false,
						pointIndex
						);	
				}
				else
				{
					// Fill
					graph.FillDoughnutSlice( 
						area,
						point,
						brush,
						pen,
						points[(int)PiePoints.TopRectTopLeftPoint], 
						points[(int)PiePoints.TopStart], 
						points[(int)PiePoints.TopRectBottomRightPoint],
						points[(int)PiePoints.TopEnd], 
						points[(int)PiePoints.DoughnutTopEnd],
						points[(int)PiePoints.DoughnutTopStart],
						startAngle,
						sweepAngle,
						true,
						doughnutRadius,
						pointIndex
						);

					// Draw Border
					graph.FillDoughnutSlice( 
						area,
						point,
						brush,
						pen,
						points[(int)PiePoints.TopRectTopLeftPoint], 
						points[(int)PiePoints.TopStart], 
						points[(int)PiePoints.TopRectBottomRightPoint],
						points[(int)PiePoints.TopEnd], 
						points[(int)PiePoints.DoughnutTopEnd],
						points[(int)PiePoints.DoughnutTopStart], 
						startAngle,
						sweepAngle,
						false,
						doughnutRadius,
						pointIndex
						);
				}

				// Draw 3D Outside labels
				if( GetLabelStyle( point ) == PieLabelStyle.Outside )
				{
					// Check if special color properties are set
					Color pieLineColor = pen.Color;
					if(point.IsCustomPropertySet(CustomPropertyName.PieLineColor) || (point.series != null && point.series.IsCustomPropertySet(CustomPropertyName.PieLineColor)) )
					{
						ColorConverter colorConverter = new ColorConverter();
                        bool failed = false;

						try
						{
							if(point.IsCustomPropertySet(CustomPropertyName.PieLineColor))
							{
								pieLineColor = (Color)colorConverter.ConvertFromString(point[CustomPropertyName.PieLineColor]);
							}
							else if(point.series != null && point.series.IsCustomPropertySet(CustomPropertyName.PieLineColor))
							{
								pieLineColor = (Color)colorConverter.ConvertFromString(point.series[CustomPropertyName.PieLineColor]);
							}
						}
                        catch (ArgumentException)
                        {
                            failed = true;
                        }
                        catch (NotSupportedException)
                        {
                            failed = true;
                        }
						
                        if(failed)
						{
							if(point.IsCustomPropertySet(CustomPropertyName.PieLineColor))
							{
								pieLineColor = (Color)colorConverter.ConvertFromInvariantString(point[CustomPropertyName.PieLineColor]);
							}
							else if(point.series != null && point.series.IsCustomPropertySet(CustomPropertyName.PieLineColor))
							{
								pieLineColor = (Color)colorConverter.ConvertFromInvariantString(point.series[CustomPropertyName.PieLineColor]);
							}
						}
					}

					// Draw labels
                    using (Pen labelPen = new Pen(pieLineColor, pen.Width))
                    {
                        Draw3DOutsideLabels(graph, area, labelPen, points, point, midAngle, pointIndex);
                    }
				}

			}
			else
			{

				// Draw 3D Inside labels
				if( GetLabelStyle( point ) == PieLabelStyle.Inside )
				{
					Draw3DInsideLabels( graph, points, point, pointIndex );
				}
			}

            //Clean up resources
            if (brush!=null) 
                brush.Dispose();
            if (pen != null)
                pen.Dispose();
            if (penCurve != null)
                penCurve.Dispose();
		}

		/// <summary>
		/// This method transforms in 3D space important points for 
		/// doughnut or pie slice.
		/// </summary>
		/// <param name="graph">Chart Graphics</param>
		/// <param name="area">Chart Area</param>
		/// <param name="pieWidth">The width of a pie.</param>
		/// <param name="rectangle">Rectangle used for drawing pie clice.</param>
		/// <param name="startAngle">Start angle for pie slice.</param>
		/// <param name="sweepAngle">End angle for pie slice.</param>
		/// <param name="relativeCoordinates">true if relative coordinates has to be returned.</param>
		/// <param name="doughnutRadius">Doughnut Radius</param>
		/// <param name="exploded">Exploded pie slice</param>
		/// <returns>Returns 3D Transformed pie or doughnut points.</returns>
		private PointF [] GetPiePoints( 
			ChartGraphics graph, 
			ChartArea area, 
			float pieWidth, 
			RectangleF rectangle, 
			float startAngle, 
			float sweepAngle, 
			bool relativeCoordinates, 
			float doughnutRadius,
			bool exploded
			)
		{
			doughnutRadius = 1 - doughnutRadius / 100F;

			Point3D [] points;
			PointF [] result;

			// Doughnut chart has 12 more points
			if( Doughnut )
			{
				points = new Point3D[29];
				result = new PointF[29];
			}
			else
			{
				points = new Point3D[17];
				result = new PointF[17];
			}
			
			// Angle 180 Top point on the arc
			points[(int)PiePoints.Top180] = new Point3D(
				rectangle.X + (float)Math.Cos( 180 * Math.PI / 180 ) * rectangle.Width / 2F + rectangle.Width / 2F,
				rectangle.Y + (float)Math.Sin( 180 * Math.PI / 180 ) * rectangle.Height / 2F + rectangle.Height / 2F,
				pieWidth );

			// Angle 180 Bottom point on the arc
			points[(int)PiePoints.Bottom180] = new Point3D(
				rectangle.X + (float)Math.Cos( 180 * Math.PI / 180 ) * rectangle.Width / 2F + rectangle.Width / 2F,
				rectangle.Y + (float)Math.Sin( 180 * Math.PI / 180 ) * rectangle.Height / 2F + rectangle.Height / 2F,
				0 );

			// Angle 0 Top point on the arc
			points[(int)PiePoints.Top0] = new Point3D(
				rectangle.X + (float)Math.Cos( 0 * Math.PI / 180 ) * rectangle.Width / 2F + rectangle.Width / 2F,
				rectangle.Y + (float)Math.Sin( 0 * Math.PI / 180 ) * rectangle.Height / 2F + rectangle.Height / 2F,
				pieWidth );

			// Angle 0 Bottom point on the arc
			points[(int)PiePoints.Bottom0] = new Point3D(
				rectangle.X + (float)Math.Cos( 0 * Math.PI / 180 ) * rectangle.Width / 2F + rectangle.Width / 2F,
				rectangle.Y + (float)Math.Sin( 0 * Math.PI / 180 ) * rectangle.Height / 2F + rectangle.Height / 2F,
				0 );

			// Top Start Angle point on the arc
			points[(int)PiePoints.TopStart] = new Point3D(
			rectangle.X + (float)Math.Cos( startAngle * Math.PI / 180 ) * rectangle.Width / 2F + rectangle.Width / 2F,
			rectangle.Y + (float)Math.Sin( startAngle * Math.PI / 180 ) * rectangle.Height / 2F + rectangle.Height / 2F,
			pieWidth );

			// Top End Angle point on the arc
			points[(int)PiePoints.TopEnd] = new Point3D(
			rectangle.X + (float)Math.Cos( ( startAngle + sweepAngle ) * Math.PI / 180 ) * rectangle.Width / 2F + rectangle.Width / 2F,
			rectangle.Y + (float)Math.Sin( ( startAngle + sweepAngle ) * Math.PI / 180 ) * rectangle.Height / 2F + rectangle.Height / 2F,
			pieWidth );

			// Bottom Start Angle point on the arc
			points[(int)PiePoints.BottomStart] = new Point3D(
			rectangle.X + (float)Math.Cos( startAngle * Math.PI / 180 ) * rectangle.Width / 2F + rectangle.Width / 2F,
			rectangle.Y + (float)Math.Sin( startAngle * Math.PI / 180 ) * rectangle.Height / 2F + rectangle.Height / 2F,
			0 );

			// Bottom End Angle point on the arc
			points[(int)PiePoints.BottomEnd] = new Point3D(
			rectangle.X + (float)Math.Cos( ( startAngle + sweepAngle ) * Math.PI / 180 ) * rectangle.Width / 2F + rectangle.Width / 2F,
			rectangle.Y + (float)Math.Sin( ( startAngle + sweepAngle ) * Math.PI / 180 ) * rectangle.Height / 2F + rectangle.Height / 2F,
			0 );

			// Center Top
			points[(int)PiePoints.TopCenter] = new Point3D(
			rectangle.X + rectangle.Width / 2F,
			rectangle.Y + rectangle.Height / 2F,
			pieWidth );

			// Center Bottom
			points[(int)PiePoints.BottomCenter] = new Point3D(
			rectangle.X + rectangle.Width / 2F,
			rectangle.Y + rectangle.Height / 2F,
			0 );

			// Top Label Line
			points[(int)PiePoints.TopLabelLine] = new Point3D(
				rectangle.X + (float)Math.Cos( ( startAngle + sweepAngle / 2 ) * Math.PI / 180 ) * rectangle.Width / 2F + rectangle.Width / 2F,
				rectangle.Y + (float)Math.Sin( ( startAngle + sweepAngle / 2 ) * Math.PI / 180 ) * rectangle.Height / 2F + rectangle.Height / 2F,
				pieWidth );

			// If Pie slice is exploded Label line out size is changed
			float sizeOut;
			if( exploded )
			{
				sizeOut = 1.1F;
			}
			else
			{
				sizeOut = 1.3F;
			}

			// Top Label Line Out
			points[(int)PiePoints.TopLabelLineout] = new Point3D(
				rectangle.X + (float)Math.Cos( ( startAngle + sweepAngle / 2 ) * Math.PI / 180 ) * rectangle.Width * sizeOut / 2F + rectangle.Width / 2F,
				rectangle.Y + (float)Math.Sin( ( startAngle + sweepAngle / 2 ) * Math.PI / 180 ) * rectangle.Height * sizeOut / 2F + rectangle.Height / 2F,
				pieWidth );

			// Top Label Center
			if( this.Doughnut )
			{
				points[(int)PiePoints.TopLabelCenter] = new Point3D(
					rectangle.X + (float)Math.Cos( ( startAngle + sweepAngle / 2 ) * Math.PI / 180 ) * rectangle.Width * ( 1 + doughnutRadius ) / 4F + rectangle.Width / 2F,
					rectangle.Y + (float)Math.Sin( ( startAngle + sweepAngle / 2 ) * Math.PI / 180 ) * rectangle.Height * ( 1 + doughnutRadius ) / 4F + rectangle.Height / 2F,
					pieWidth );
			}
			else
			{
				points[(int)PiePoints.TopLabelCenter] = new Point3D(
					rectangle.X + (float)Math.Cos( ( startAngle + sweepAngle / 2 ) * Math.PI / 180 ) * rectangle.Width * 0.5F / 2F + rectangle.Width / 2F,
					rectangle.Y + (float)Math.Sin( ( startAngle + sweepAngle / 2 ) * Math.PI / 180 ) * rectangle.Height * 0.5F / 2F + rectangle.Height / 2F,
					pieWidth );
			}
			

			// Top Rectangle Top Left Point
			points[(int)PiePoints.TopRectTopLeftPoint] = new Point3D(rectangle.X,rectangle.Y,pieWidth);
		
			// Top Rectangle Right Bottom Point
			points[(int)PiePoints.TopRectBottomRightPoint] = new Point3D(rectangle.Right,rectangle.Bottom,pieWidth);

			// Bottom Rectangle Top Left Point
			points[(int)PiePoints.BottomRectTopLeftPoint] = new Point3D(rectangle.X,rectangle.Y,0);
		
			// Bottom Rectangle Right Bottom Point
			points[(int)PiePoints.BottomRectBottomRightPoint] = new Point3D(rectangle.Right,rectangle.Bottom,0);

			if( Doughnut )
			{
				// Angle 180 Top point on the Doughnut arc
				points[(int)PiePoints.DoughnutTop180] = new Point3D(
					rectangle.X + (float)Math.Cos( 180 * Math.PI / 180 ) * rectangle.Width * doughnutRadius / 2F + rectangle.Width / 2F,
					rectangle.Y + (float)Math.Sin( 180 * Math.PI / 180 ) * rectangle.Height * doughnutRadius / 2F + rectangle.Height / 2F,
					pieWidth );

				// Angle 180 Bottom point on the Doughnut arc
				points[(int)PiePoints.DoughnutBottom180] = new Point3D(
					rectangle.X + (float)Math.Cos( 180 * Math.PI / 180 ) * rectangle.Width * doughnutRadius / 2F + rectangle.Width / 2F,
					rectangle.Y + (float)Math.Sin( 180 * Math.PI / 180 ) * rectangle.Height * doughnutRadius / 2F + rectangle.Height / 2F,
					0 );

				// Angle 0 Top point on the Doughnut arc
				points[(int)PiePoints.DoughnutTop0] = new Point3D(
					rectangle.X + (float)Math.Cos( 0 * Math.PI / 180 ) * rectangle.Width * doughnutRadius / 2F + rectangle.Width / 2F,
					rectangle.Y + (float)Math.Sin( 0 * Math.PI / 180 ) * rectangle.Height * doughnutRadius / 2F + rectangle.Height / 2F,
					pieWidth );

				// Angle 0 Bottom point on the Doughnut arc
				points[(int)PiePoints.DoughnutBottom0] = new Point3D(
					rectangle.X + (float)Math.Cos( 0 * Math.PI / 180 ) * rectangle.Width * doughnutRadius / 2F + rectangle.Width / 2F,
					rectangle.Y + (float)Math.Sin( 0 * Math.PI / 180 ) * rectangle.Height * doughnutRadius / 2F + rectangle.Height / 2F,
					0 );

				// Top Start Angle point on the Doughnut arc
				points[(int)PiePoints.DoughnutTopStart] = new Point3D(
					rectangle.X + (float)Math.Cos( startAngle * Math.PI / 180 ) * rectangle.Width * doughnutRadius / 2F + rectangle.Width / 2F,
					rectangle.Y + (float)Math.Sin( startAngle * Math.PI / 180 ) * rectangle.Height * doughnutRadius / 2F + rectangle.Height / 2F,
					pieWidth );

				// Top End Angle point on the Doughnut arc
				points[(int)PiePoints.DoughnutTopEnd] = new Point3D(
					rectangle.X + (float)Math.Cos( ( startAngle + sweepAngle ) * Math.PI / 180 ) * rectangle.Width * doughnutRadius / 2F + rectangle.Width / 2F,
					rectangle.Y + (float)Math.Sin( ( startAngle + sweepAngle ) * Math.PI / 180 ) * rectangle.Height * doughnutRadius / 2F + rectangle.Height / 2F,
					pieWidth );

				// Bottom Start Angle point on the Doughnut arc
				points[(int)PiePoints.DoughnutBottomStart] = new Point3D(
					rectangle.X + (float)Math.Cos( startAngle * Math.PI / 180 ) * rectangle.Width * doughnutRadius / 2F + rectangle.Width / 2F,
					rectangle.Y + (float)Math.Sin( startAngle * Math.PI / 180 ) * rectangle.Height * doughnutRadius / 2F + rectangle.Height / 2F,
					0 );

				// Bottom End Angle point on the Doughnut arc
				points[(int)PiePoints.DoughnutBottomEnd] = new Point3D(
					rectangle.X + (float)Math.Cos( ( startAngle + sweepAngle ) * Math.PI / 180 ) * rectangle.Width * doughnutRadius / 2F + rectangle.Width / 2F,
					rectangle.Y + (float)Math.Sin( ( startAngle + sweepAngle ) * Math.PI / 180 ) * rectangle.Height * doughnutRadius / 2F + rectangle.Height / 2F,
					0 );

				rectangle.Inflate( -rectangle.Width * (1 - doughnutRadius) / 2F, -rectangle.Height * (1 - doughnutRadius) / 2F);

				// Doughnut Top Rectangle Top Left Point
				points[(int)PiePoints.DoughnutTopRectTopLeftPoint] = new Point3D(rectangle.X,rectangle.Y,pieWidth);
		
				// Doughnut Top Rectangle Right Bottom Point
				points[(int)PiePoints.DoughnutTopRectBottomRightPoint] = new Point3D(rectangle.Right,rectangle.Bottom,pieWidth);

				// Doughnut Bottom Rectangle Top Left Point
				points[(int)PiePoints.DoughnutBottomRectTopLeftPoint] = new Point3D(rectangle.X,rectangle.Y,0);
		
				// Doughnut Bottom Rectangle Right Bottom Point
				points[(int)PiePoints.DoughnutBottomRectBottomRightPoint] = new Point3D(rectangle.Right,rectangle.Bottom,0);


			}
		
			// Make 3D transformations
			area.matrix3D.TransformPoints(points);
			
			int pointIndx = 0;
			foreach( Point3D point in points )
			{
				result[pointIndx] = point.PointF;

				// Convert Relative coordinates to absolute.
				if( relativeCoordinates )
				{
					result[pointIndx] = graph.GetAbsolutePoint(result[pointIndx]);
				}
				pointIndx++;
			}
			
			return result;
			
		}


		
		
		#endregion
		
		#region 3D Drawing surfaces

		/// <summary>
		/// This method is used for drawing curve around pie slices. This is 
		/// the most complex part of 3D Pie slice. There is special case if 
		/// pie slice is bigger then 180 degree.
		/// </summary>
		/// <param name="graph">Chart Grahics.</param>
		/// <param name="area">Chart Area.</param>
		/// <param name="dataPoint">Data Point used for pie slice.</param>
		/// <param name="startAngle">Start angle of a pie slice.</param>
		/// <param name="sweepAngle">Sweep angle of a pie slice.</param>
		/// <param name="points">Important 3d points of a pie slice.</param>
		/// <param name="brushWithoutLight">Brush without lithing efects.</param>
		/// <param name="pen">Pen used for border.</param>
		/// <param name="rightPosition">Position of the curve of big pie slice. Big pie slice coud have to visible curves - left and right</param>
		/// <param name="sameBackFront">This is big pie slice which is in same time back and front slice.</param>
		/// <param name="pointIndex">Data Point Index</param>
		private void DrawPieCurves( 
			ChartGraphics graph,
			ChartArea area,
			DataPoint dataPoint,
			float startAngle,
			float sweepAngle,
			PointF [] points,
			SolidBrush brushWithoutLight,
			Pen pen,
			bool rightPosition,
			bool sameBackFront,
			int pointIndex
			)
		{
			// Create a graphics path
            using (GraphicsPath path = new GraphicsPath())
            {
                Brush brush;

                if (area.Area3DStyle.LightStyle == LightStyle.None)
                {
                    brush = brushWithoutLight;
                }
                else
                {
                    brush = graph.GetGradientBrush(graph.GetAbsoluteRectangle(area.Position.ToRectangleF()), Color.FromArgb(brushWithoutLight.Color.A, 0, 0, 0), brushWithoutLight.Color, GradientStyle.VerticalCenter);
                }

                float endAngle = startAngle + sweepAngle;

                // Very big pie slice ( > 180 degree )
                if (sweepAngle > 180)
                {
                    if (DrawPieCurvesBigSlice(graph, area, dataPoint, startAngle, sweepAngle, points, brush, pen, rightPosition, sameBackFront, pointIndex))
                        return;
                }

                // Pie slice pass throw 180 degree. Curve has to be spited.
                if (startAngle < 180 && endAngle > 180)
                {
                    if (area.Area3DStyle.Inclination < 0)
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.TopRectTopLeftPoint],
                            points[(int)PiePoints.TopRectBottomRightPoint],
                            points[(int)PiePoints.BottomRectTopLeftPoint],
                            points[(int)PiePoints.BottomRectBottomRightPoint],
                            points[(int)PiePoints.TopStart],
                            points[(int)PiePoints.Top180],
                            points[(int)PiePoints.BottomStart],
                            points[(int)PiePoints.Bottom180],
                            startAngle,
                            180 - startAngle,
                            pointIndex
                            );

                    }
                    else
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.TopRectTopLeftPoint],
                            points[(int)PiePoints.TopRectBottomRightPoint],
                            points[(int)PiePoints.BottomRectTopLeftPoint],
                            points[(int)PiePoints.BottomRectBottomRightPoint],
                            points[(int)PiePoints.Top180],
                            points[(int)PiePoints.TopEnd],
                            points[(int)PiePoints.Bottom180],
                            points[(int)PiePoints.BottomEnd],
                            180,
                            startAngle + sweepAngle - 180,
                            pointIndex
                            );

                    }
                }

                // Pie slice pass throw 0 degree. Curve has to be spited.
                else if (startAngle < 0 && endAngle > 0)
                {
                    if (area.Area3DStyle.Inclination > 0)
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.TopRectTopLeftPoint],
                            points[(int)PiePoints.TopRectBottomRightPoint],
                            points[(int)PiePoints.BottomRectTopLeftPoint],
                            points[(int)PiePoints.BottomRectBottomRightPoint],
                            points[(int)PiePoints.TopStart],
                            points[(int)PiePoints.Top0],
                            points[(int)PiePoints.BottomStart],
                            points[(int)PiePoints.Bottom0],
                            startAngle,
                            -startAngle,
                            pointIndex
                            );

                    }
                    else
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.TopRectTopLeftPoint],
                            points[(int)PiePoints.TopRectBottomRightPoint],
                            points[(int)PiePoints.BottomRectTopLeftPoint],
                            points[(int)PiePoints.BottomRectBottomRightPoint],
                            points[(int)PiePoints.Top0],
                            points[(int)PiePoints.TopEnd],
                            points[(int)PiePoints.Bottom0],
                            points[(int)PiePoints.BottomEnd],
                            0,
                            sweepAngle + startAngle,
                            pointIndex
                            );

                    }
                }
                // Pie slice pass throw 360 degree. Curve has to be spited.
                else if (startAngle < 360 && endAngle > 360)
                {
                    if (area.Area3DStyle.Inclination > 0)
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.TopRectTopLeftPoint],
                            points[(int)PiePoints.TopRectBottomRightPoint],
                            points[(int)PiePoints.BottomRectTopLeftPoint],
                            points[(int)PiePoints.BottomRectBottomRightPoint],
                            points[(int)PiePoints.TopStart],
                            points[(int)PiePoints.Top0],
                            points[(int)PiePoints.BottomStart],
                            points[(int)PiePoints.Bottom0],
                            startAngle,
                            360 - startAngle,
                            pointIndex
                            );

                    }
                    else
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.TopRectTopLeftPoint],
                            points[(int)PiePoints.TopRectBottomRightPoint],
                            points[(int)PiePoints.BottomRectTopLeftPoint],
                            points[(int)PiePoints.BottomRectBottomRightPoint],
                            points[(int)PiePoints.Top0],
                            points[(int)PiePoints.TopEnd],
                            points[(int)PiePoints.Bottom0],
                            points[(int)PiePoints.BottomEnd],
                            0,
                            endAngle - 360,
                            pointIndex
                            );
                    }
                }
                else
                {
                    // ***************************************************
                    // REGULAR CASE: The curve is not split.
                    // ***************************************************
                    if (startAngle < 180 && startAngle >= 0 && area.Area3DStyle.Inclination < 0
                        || startAngle < 540 && startAngle >= 360 && area.Area3DStyle.Inclination < 0
                        || startAngle >= 180 && startAngle < 360 && area.Area3DStyle.Inclination > 0
                        || startAngle >= -180 && startAngle < 0 && area.Area3DStyle.Inclination > 0
                        )
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.TopRectTopLeftPoint],
                            points[(int)PiePoints.TopRectBottomRightPoint],
                            points[(int)PiePoints.BottomRectTopLeftPoint],
                            points[(int)PiePoints.BottomRectBottomRightPoint],
                            points[(int)PiePoints.TopStart],
                            points[(int)PiePoints.TopEnd],
                            points[(int)PiePoints.BottomStart],
                            points[(int)PiePoints.BottomEnd],
                            startAngle,
                            sweepAngle,
                            pointIndex
                            );
                    }
                }
            }
		}

		/// <summary>
		/// This method is used for special case when big pie slice has to be drawn.
		/// </summary>
		/// <param name="graph">Chart Grahics.</param>
		/// <param name="area">Chart Area.</param>
		/// <param name="dataPoint">Data Point used for pie slice.</param>
		/// <param name="startAngle">Start angle of a pie slice.</param>
		/// <param name="sweepAngle">Sweep angle of a pie slice.</param>
		/// <param name="points">Important 3d points of a pie slice.</param>
		/// <param name="brush">Brush without lithing efects.</param>
		/// <param name="pen">Pen used for border.</param>
		/// <param name="rightPosition">Position of the curve of big pie slice. Big pie slice coud have to visible curves - left and right</param>
		/// <param name="sameBackFront">This is big pie slice which is in same time back and front slice.</param>		
		/// <param name="pointIndex">Data Point Index</param>
		/// <returns>True if slice is special case and it is drawn as a special case.</returns>
		private bool DrawPieCurvesBigSlice
		(
			ChartGraphics graph,
			ChartArea area,
			DataPoint dataPoint,
			float startAngle,
			float sweepAngle,
			PointF [] points,
			Brush brush,
			Pen pen,
			bool rightPosition,
			bool sameBackFront,
			int pointIndex
		)
		{
			float endAngle = startAngle + sweepAngle;

			// Two different cases connected with X angle.
			// *****************************************************
			// X angle is positive
			// *****************************************************
			if( area.Area3DStyle.Inclination > 0 )
			{
				// Show curve from 0 to 180.
				if( startAngle < 180 && endAngle > 360 )
				{
					graph.FillPieCurve(
						area,
						dataPoint,
						brush,
						pen,
						points[(int)PiePoints.TopRectTopLeftPoint], 
						points[(int)PiePoints.TopRectBottomRightPoint],
						points[(int)PiePoints.BottomRectTopLeftPoint], 
						points[(int)PiePoints.BottomRectBottomRightPoint],
						points[(int)PiePoints.Top0],
						points[(int)PiePoints.Top180],
						points[(int)PiePoints.Bottom0],
						points[(int)PiePoints.Bottom180],
						0,
						-180,
						pointIndex
						);
				}
				else if( startAngle < 0 && endAngle > 180 )
				{
					// There is big data point which is back and 
					// front point in same time.
					if( sameBackFront )
					{
						// The big pie slice has to be split. This part makes 
						// decision which part of this big slice will be 
						// drawn first.
						if( rightPosition )
						{
							graph.FillPieCurve(
								area,
								dataPoint,
								brush,
								pen,
								points[(int)PiePoints.TopRectTopLeftPoint], 
								points[(int)PiePoints.TopRectBottomRightPoint],
								points[(int)PiePoints.BottomRectTopLeftPoint], 
								points[(int)PiePoints.BottomRectBottomRightPoint],
								points[(int)PiePoints.Top180],
								points[(int)PiePoints.TopEnd],
								points[(int)PiePoints.Bottom180],
								points[(int)PiePoints.BottomEnd],
								180,
								endAngle - 180,
								pointIndex
								);
						}
						else
						{
							graph.FillPieCurve(
								area,
								dataPoint,
								brush,
								pen,
								points[(int)PiePoints.TopRectTopLeftPoint], 
								points[(int)PiePoints.TopRectBottomRightPoint],
								points[(int)PiePoints.BottomRectTopLeftPoint], 
								points[(int)PiePoints.BottomRectBottomRightPoint],
								points[(int)PiePoints.TopStart],
								points[(int)PiePoints.Top0],
								points[(int)PiePoints.BottomStart],
								points[(int)PiePoints.Bottom0],
								startAngle,
								-startAngle,
								pointIndex
								);
						}
					}
					else
					{
						// There is big pie slice (>180), but that pie slice 
						// is not back and front point in same time.
						graph.FillPieCurve(
							area,
							dataPoint,
							brush,
							pen,
							points[(int)PiePoints.TopRectTopLeftPoint], 
							points[(int)PiePoints.TopRectBottomRightPoint],
							points[(int)PiePoints.BottomRectTopLeftPoint], 
							points[(int)PiePoints.BottomRectBottomRightPoint],
							points[(int)PiePoints.TopStart],
							points[(int)PiePoints.Top0],
							points[(int)PiePoints.BottomStart],
							points[(int)PiePoints.Bottom0],
							startAngle,
							-startAngle,
							pointIndex
							);

						graph.FillPieCurve(
							area,
							dataPoint,
							brush,
							pen,
							points[(int)PiePoints.TopRectTopLeftPoint], 
							points[(int)PiePoints.TopRectBottomRightPoint],
							points[(int)PiePoints.BottomRectTopLeftPoint], 
							points[(int)PiePoints.BottomRectBottomRightPoint],
							points[(int)PiePoints.Top180],
							points[(int)PiePoints.TopEnd],
							points[(int)PiePoints.Bottom180],
							points[(int)PiePoints.BottomEnd],
							180,
							endAngle - 180,
							pointIndex
							);
					}
					
				}
				else
				{
					// Big pie slice behaves as normal pie slice. Continue 
					// Non special case alghoritham
					return false;
				}
			}
			// *********************************************
			// X angle negative
			// *********************************************
			else
			{
				// Show curve from 0 to 180.
				if( startAngle < 0 && endAngle > 180 )
				{
					graph.FillPieCurve(
						area,
						dataPoint,
						brush,
						pen,
						points[(int)PiePoints.TopRectTopLeftPoint], 
						points[(int)PiePoints.TopRectBottomRightPoint],
						points[(int)PiePoints.BottomRectTopLeftPoint], 
						points[(int)PiePoints.BottomRectBottomRightPoint],
						points[(int)PiePoints.Top0],
						points[(int)PiePoints.Top180],
						points[(int)PiePoints.Bottom0],
						points[(int)PiePoints.Bottom180],
						0,
						180,
						pointIndex
						);
				}
				else if( startAngle < 180 && endAngle > 360 )
				{
					// There is big data point which is back and 
					// front point in same time.
					if( sameBackFront )
					{
						// The big pie slice has to be split. This part makes 
						// decision which part of this big slice will be 
						// drawn first.
						if( rightPosition )
						{
							graph.FillPieCurve(
								area,
								dataPoint,
								brush,
								pen,
								points[(int)PiePoints.TopRectTopLeftPoint], 
								points[(int)PiePoints.TopRectBottomRightPoint],
								points[(int)PiePoints.BottomRectTopLeftPoint], 
								points[(int)PiePoints.BottomRectBottomRightPoint],
								points[(int)PiePoints.TopStart],
								points[(int)PiePoints.Top180],
								points[(int)PiePoints.BottomStart],
								points[(int)PiePoints.Bottom180],
								startAngle,
								180 - startAngle,
								pointIndex
								);
						}
						else
						{
							graph.FillPieCurve(
								area,
								dataPoint,
								brush,
								pen,
								points[(int)PiePoints.TopRectTopLeftPoint], 
								points[(int)PiePoints.TopRectBottomRightPoint],
								points[(int)PiePoints.BottomRectTopLeftPoint], 
								points[(int)PiePoints.BottomRectBottomRightPoint],
								points[(int)PiePoints.Top0],
								points[(int)PiePoints.TopEnd],
								points[(int)PiePoints.Bottom0],
								points[(int)PiePoints.BottomEnd],
								0,
								endAngle - 360,
								pointIndex
								);
						}
					}
					else
					{
						// There is big pie slice (>180), but that pie slice 
						// is not back and front point in same time.
						graph.FillPieCurve(
							area,
							dataPoint,
							brush,
							pen,
							points[(int)PiePoints.TopRectTopLeftPoint], 
							points[(int)PiePoints.TopRectBottomRightPoint],
							points[(int)PiePoints.BottomRectTopLeftPoint], 
							points[(int)PiePoints.BottomRectBottomRightPoint],
							points[(int)PiePoints.Top0],
							points[(int)PiePoints.TopEnd],
							points[(int)PiePoints.Bottom0],
							points[(int)PiePoints.BottomEnd],
							0,
							endAngle - 360,
							pointIndex
							);

						graph.FillPieCurve(
							area,
							dataPoint,
							brush,
							pen,
							points[(int)PiePoints.TopRectTopLeftPoint], 
							points[(int)PiePoints.TopRectBottomRightPoint],
							points[(int)PiePoints.BottomRectTopLeftPoint], 
							points[(int)PiePoints.BottomRectBottomRightPoint],
							points[(int)PiePoints.TopStart],
							points[(int)PiePoints.Top180],
							points[(int)PiePoints.BottomStart],
							points[(int)PiePoints.Bottom180],
							startAngle,
							180 - startAngle,
							pointIndex
							);
					}
					
				}
				else
				{
					// Big pie slice behaves as normal pie slice. Continue 
					// Non special case alghoritham
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// This method is used for drawing curve around doughnut slices - inner curve. 
		/// This is the most complex part of 3D Doughnut slice. There is special case if 
		/// pie slice is bigger then 180 degree.
		/// </summary>
		/// <param name="graph">Chart Grahics.</param>
		/// <param name="area">Chart Area.</param>
		/// <param name="dataPoint">Data Point used for pie slice.</param>
		/// <param name="startAngle">Start angle of a pie slice.</param>
		/// <param name="sweepAngle">Sweep angle of a pie slice.</param>
		/// <param name="points">Important 3d points of a pie slice.</param>
		/// <param name="brushWithoutLight">Brush without lithing efects.</param>
		/// <param name="pen">Pen used for border.</param>
		/// <param name="rightPosition">Position of the curve of big pie slice. Big pie slice coud have to visible curves - left and right</param>
		/// <param name="sameBackFront">This is big pie slice which is in same time back and front slice.</param>		
		/// <param name="pointIndex">Data Point Index</param>
		private void DrawDoughnutCurves( 
			ChartGraphics graph,
			ChartArea area,
			DataPoint dataPoint,
			float startAngle,
			float sweepAngle,
			PointF [] points,
			SolidBrush brushWithoutLight,
			Pen pen,
			bool rightPosition,
			bool sameBackFront,
			int pointIndex
			)
		{
			// Create a graphics path
            using (GraphicsPath path = new GraphicsPath())
            {

                Brush brush;

                if (area.Area3DStyle.LightStyle == LightStyle.None)
                {
                    brush = brushWithoutLight;
                }
                else
                {
                    brush = graph.GetGradientBrush(graph.GetAbsoluteRectangle(area.Position.ToRectangleF()), Color.FromArgb(brushWithoutLight.Color.A, 0, 0, 0), brushWithoutLight.Color, GradientStyle.VerticalCenter);
                }

                float endAngle = startAngle + sweepAngle;

                // Very big pie slice ( > 180 degree )
                if (sweepAngle > 180)
                {
                    if (DrawDoughnutCurvesBigSlice(graph, area, dataPoint, startAngle, sweepAngle, points, brush, pen, rightPosition, sameBackFront, pointIndex))
                        return;
                }

                // Pie slice pass throw 180 degree. Curve has to be spited.
                if (startAngle < 180 && endAngle > 180)
                {
                    if (area.Area3DStyle.Inclination > 0)
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.DoughnutTopRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutBottomRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutTopStart],
                            points[(int)PiePoints.DoughnutTop180],
                            points[(int)PiePoints.DoughnutBottomStart],
                            points[(int)PiePoints.DoughnutBottom180],
                            startAngle,
                            180 - startAngle,
                            pointIndex
                            );

                    }
                    else
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.DoughnutTopRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutBottomRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutTop180],
                            points[(int)PiePoints.DoughnutTopEnd],
                            points[(int)PiePoints.DoughnutBottom180],
                            points[(int)PiePoints.DoughnutBottomEnd],
                            180,
                            startAngle + sweepAngle - 180,
                            pointIndex
                            );

                    }
                }

                    // Pie slice pass throw 0 degree. Curve has to be spited.
                else if (startAngle < 0 && endAngle > 0)
                {
                    if (area.Area3DStyle.Inclination < 0)
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.DoughnutTopRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutBottomRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutTopStart],
                            points[(int)PiePoints.DoughnutTop0],
                            points[(int)PiePoints.DoughnutBottomStart],
                            points[(int)PiePoints.DoughnutBottom0],
                            startAngle,
                            -startAngle,
                            pointIndex
                            );

                    }
                    else
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.DoughnutTopRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutBottomRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutTop0],
                            points[(int)PiePoints.DoughnutTopEnd],
                            points[(int)PiePoints.DoughnutBottom0],
                            points[(int)PiePoints.DoughnutBottomEnd],
                            0,
                            sweepAngle + startAngle,
                            pointIndex
                            );

                    }
                }
                // Pie slice pass throw 360 degree. Curve has to be spited.
                else if (startAngle < 360 && endAngle > 360)
                {
                    if (area.Area3DStyle.Inclination < 0)
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.DoughnutTopRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutBottomRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutTopStart],
                            points[(int)PiePoints.DoughnutTop0],
                            points[(int)PiePoints.DoughnutBottomStart],
                            points[(int)PiePoints.DoughnutBottom0],
                            startAngle,
                            360 - startAngle,
                            pointIndex
                            );

                    }
                    else
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.DoughnutTopRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutBottomRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutTop0],
                            points[(int)PiePoints.DoughnutTopEnd],
                            points[(int)PiePoints.DoughnutBottom0],
                            points[(int)PiePoints.DoughnutBottomEnd],
                            0,
                            endAngle - 360,
                            pointIndex
                            );
                    }
                }
                else
                {
                    // ***************************************************
                    // REGULAR CASE: The curve is not split.
                    // ***************************************************
                    if (startAngle < 180 && startAngle >= 0 && area.Area3DStyle.Inclination > 0
                        || startAngle < 540 && startAngle >= 360 && area.Area3DStyle.Inclination > 0
                        || startAngle >= 180 && startAngle < 360 && area.Area3DStyle.Inclination < 0
                        || startAngle >= -180 && startAngle < 0 && area.Area3DStyle.Inclination < 0
                        )
                    {
                        graph.FillPieCurve(
                            area,
                            dataPoint,
                            brush,
                            pen,
                            points[(int)PiePoints.DoughnutTopRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutBottomRectTopLeftPoint],
                            points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
                            points[(int)PiePoints.DoughnutTopStart],
                            points[(int)PiePoints.DoughnutTopEnd],
                            points[(int)PiePoints.DoughnutBottomStart],
                            points[(int)PiePoints.DoughnutBottomEnd],
                            startAngle,
                            sweepAngle,
                            pointIndex
                            );
                    }
                }

            }

		}


		/// <summary>
		/// This method is used for special case when big doughnut slice has to be drawn.
		/// </summary>
		/// <param name="graph">Chart Grahics.</param>
		/// <param name="area">Chart Area.</param>
		/// <param name="dataPoint">Data Point used for pie slice.</param>
		/// <param name="startAngle">Start angle of a pie slice.</param>
		/// <param name="sweepAngle">Sweep angle of a pie slice.</param>
		/// <param name="points">Important 3d points of a pie slice.</param>
		/// <param name="brush">Brush without lithing efects.</param>
		/// <param name="pen">Pen used for border.</param>
		/// <param name="rightPosition">Position of the curve of big pie slice. Big pie slice coud have to visible curves - left and right</param>
		/// <param name="sameBackFront">This is big pie slice which is in same time back and front slice.</param>		
		/// <param name="pointIndex">Data Point Index</param>
		/// <returns>True if slice is special case and it is drawn as a special case.</returns>
		private bool DrawDoughnutCurvesBigSlice
			(
			ChartGraphics graph,
			ChartArea area,
			DataPoint dataPoint,
			float startAngle,
			float sweepAngle,
			PointF [] points,
			Brush brush,
			Pen pen,
			bool rightPosition,
			bool sameBackFront,
			int pointIndex
			)
		{
			float endAngle = startAngle + sweepAngle;

			// Two different cases connected with X angle.
			// *****************************************************
			// X angle is positive
			// *****************************************************
			if( area.Area3DStyle.Inclination < 0 )
			{
				// Show curve from 0 to 180.
				if( startAngle < 180 && endAngle > 360 )
				{
					graph.FillPieCurve(
						area,
						dataPoint,
						brush,
						pen,
						points[(int)PiePoints.DoughnutTopRectTopLeftPoint], 
						points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
						points[(int)PiePoints.DoughnutBottomRectTopLeftPoint], 
						points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
						points[(int)PiePoints.DoughnutTop0],
						points[(int)PiePoints.DoughnutTop180],
						points[(int)PiePoints.DoughnutBottom0],
						points[(int)PiePoints.DoughnutBottom180],
						0,
						-180,
						pointIndex
						);
				}
				else if( startAngle < 0 && endAngle > 180 )
				{
					// There is big data point which is back and 
					// front point in same time.
					if( sameBackFront )
					{
						// The big pie slice has to be split. This part makes 
						// decision which part of this big slice will be 
						// drawn first.
						if( rightPosition )
						{
							graph.FillPieCurve(
								area,
								dataPoint,
								brush,
								pen,
								points[(int)PiePoints.DoughnutTopRectTopLeftPoint], 
								points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
								points[(int)PiePoints.DoughnutBottomRectTopLeftPoint], 
								points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
								points[(int)PiePoints.DoughnutTop180],
								points[(int)PiePoints.DoughnutTopEnd],
								points[(int)PiePoints.DoughnutBottom180],
								points[(int)PiePoints.DoughnutBottomEnd],
								180,
								endAngle - 180,
								pointIndex
								);
						}
						else
						{
							graph.FillPieCurve(
								area,
								dataPoint,
								brush,
								pen,
								points[(int)PiePoints.DoughnutTopRectTopLeftPoint], 
								points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
								points[(int)PiePoints.DoughnutBottomRectTopLeftPoint], 
								points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
								points[(int)PiePoints.DoughnutTopStart],
								points[(int)PiePoints.DoughnutTop0],
								points[(int)PiePoints.DoughnutBottomStart],
								points[(int)PiePoints.DoughnutBottom0],
								startAngle,
								-startAngle,
								pointIndex
								);
						}
					}
					else
					{
						// There is big pie slice (>180), but that pie slice 
						// is not back and front point in same time.
						graph.FillPieCurve(
							area,
							dataPoint,
							brush,
							pen,
							points[(int)PiePoints.DoughnutTopRectTopLeftPoint], 
							points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
							points[(int)PiePoints.DoughnutBottomRectTopLeftPoint], 
							points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
							points[(int)PiePoints.DoughnutTopStart],
							points[(int)PiePoints.DoughnutTop0],
							points[(int)PiePoints.DoughnutBottomStart],
							points[(int)PiePoints.DoughnutBottom0],
							startAngle,
							-startAngle,
							pointIndex
							);

						graph.FillPieCurve(
							area,
							dataPoint,
							brush,
							pen,
							points[(int)PiePoints.DoughnutTopRectTopLeftPoint], 
							points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
							points[(int)PiePoints.DoughnutBottomRectTopLeftPoint], 
							points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
							points[(int)PiePoints.DoughnutTop180],
							points[(int)PiePoints.DoughnutTopEnd],
							points[(int)PiePoints.DoughnutBottom180],
							points[(int)PiePoints.DoughnutBottomEnd],
							180,
							endAngle - 180,
							pointIndex
							);
					}
					
				}
				else
				{
					// Big pie slice behaves as normal pie slice. Continue 
					// Non special case alghoritham
					return false;
				}
			}
				// *********************************************
				// X angle negative
				// *********************************************
			else
			{
				// Show curve from 0 to 180.
				if( startAngle < 0 && endAngle > 180 )
				{
					graph.FillPieCurve(
						area,
						dataPoint,
						brush,
						pen,
						points[(int)PiePoints.DoughnutTopRectTopLeftPoint], 
						points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
						points[(int)PiePoints.DoughnutBottomRectTopLeftPoint], 
						points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
						points[(int)PiePoints.DoughnutTop0],
						points[(int)PiePoints.DoughnutTop180],
						points[(int)PiePoints.DoughnutBottom0],
						points[(int)PiePoints.DoughnutBottom180],
						0,
						180,
						pointIndex
						);
				}
				else if( startAngle < 180 && endAngle > 360 )
				{
					// There is big data point which is back and 
					// front point in same time.
					if( sameBackFront )
					{
						// The big pie slice has to be split. This part makes 
						// decision which part of this big slice will be 
						// drawn first.
						if( rightPosition )
						{
							graph.FillPieCurve(
								area,
								dataPoint,
								brush,
								pen,
								points[(int)PiePoints.DoughnutTopRectTopLeftPoint], 
								points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
								points[(int)PiePoints.DoughnutBottomRectTopLeftPoint], 
								points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
								points[(int)PiePoints.DoughnutTopStart],
								points[(int)PiePoints.DoughnutTop180],
								points[(int)PiePoints.DoughnutBottomStart],
								points[(int)PiePoints.DoughnutBottom180],
								startAngle,
								180 - startAngle,
								pointIndex
								);
						}
						else
						{
							graph.FillPieCurve(
								area,
								dataPoint,
								brush,
								pen,
								points[(int)PiePoints.DoughnutTopRectTopLeftPoint], 
								points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
								points[(int)PiePoints.DoughnutBottomRectTopLeftPoint], 
								points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
								points[(int)PiePoints.DoughnutTop0],
								points[(int)PiePoints.DoughnutTopEnd],
								points[(int)PiePoints.DoughnutBottom0],
								points[(int)PiePoints.DoughnutBottomEnd],
								0,
								endAngle - 360,
								pointIndex
								);
						}
					}
					else
					{
						// There is big pie slice (>180), but that pie slice 
						// is not back and front point in same time.
						graph.FillPieCurve(
							area,
							dataPoint,
							brush,
							pen,
							points[(int)PiePoints.DoughnutTopRectTopLeftPoint], 
							points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
							points[(int)PiePoints.DoughnutBottomRectTopLeftPoint], 
							points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
							points[(int)PiePoints.DoughnutTop0],
							points[(int)PiePoints.DoughnutTopEnd],
							points[(int)PiePoints.DoughnutBottom0],
							points[(int)PiePoints.DoughnutBottomEnd],
							0,
							endAngle - 360,
							pointIndex
							);

						graph.FillPieCurve(
							area,
							dataPoint,
							brush,
							pen,
							points[(int)PiePoints.DoughnutTopRectTopLeftPoint], 
							points[(int)PiePoints.DoughnutTopRectBottomRightPoint],
							points[(int)PiePoints.DoughnutBottomRectTopLeftPoint], 
							points[(int)PiePoints.DoughnutBottomRectBottomRightPoint],
							points[(int)PiePoints.DoughnutTopStart],
							points[(int)PiePoints.DoughnutTop180],
							points[(int)PiePoints.DoughnutBottomStart],
							points[(int)PiePoints.DoughnutBottom180],
							startAngle,
							180 - startAngle,
							pointIndex
							);
					}
					
				}
				else
				{
					// Big pie slice behaves as normal pie slice. Continue 
					// Non special case alghoritham
					return false;
				}
			}

			return true;
		}

		

		#endregion

		#region 3D Order of points Methods

		/// <summary>
		/// This method sort data points on specific way. Because 
		/// of order of drawing in 3D space, the back data point 
		/// (point which pass throw 270 degree has to be drawn first. 
		/// After that side data points have to be drawn. At the end 
		/// front data point (data point which pass throw 0 degree) 
		/// has to be drawn. There is special case if there is big 
		/// data point, which is back and front point in same time.
		/// </summary>
		/// <param name="series">Data series</param>
		/// <param name="area">Chart area</param>
		/// <param name="newStartAngleList">Unsorted List of Start angles.</param>
		/// <param name="newSweepAngleList">Unsorted List of Sweep angles.</param>
		/// <param name="newPointIndexList">Data Point index list</param>
		/// <param name="sameBackFrontPoint">Beck and Fron Points are same - There is a big pie slice.</param>
		/// <returns>Sorted data point list.</returns>
		private DataPoint [] PointOrder( Series series, ChartArea area, out float [] newStartAngleList, out float [] newSweepAngleList, out int [] newPointIndexList, out bool sameBackFrontPoint )
		{
						
			double startAngle;
			double sweepAngle;
			double endAngle;
			int backPoint = -1;
			int frontPoint = -1;
			sameBackFrontPoint = false;
			
			// The data points loop. Find Sum of data points.
			double	sum = 0;
			int numOfEmpty = 0;
			foreach( DataPoint point in series.Points )
			{
				if( point.IsEmpty )
					numOfEmpty++;
				
				if( !point.IsEmpty )
				{
					sum += Math.Abs(point.YValues[0]);
				}
			}

			// Find number of data points
			int numOfPoints = series.Points.Count - numOfEmpty;

			DataPoint [] points = new DataPoint[ numOfPoints ];
			float [] startAngleList = new float[ numOfPoints ];
			float [] sweepAngleList = new float[ numOfPoints ];
			int [] pointIndexList = new int[ numOfPoints ];
			newStartAngleList = new float[ numOfPoints ];
			newSweepAngleList = new float[ numOfPoints ];
			newPointIndexList = new int[ numOfPoints ];

			// If sum is less then 0 do not draw pie chart
			if( sum <= 0 )
			{
				return null;
			}
			// *****************************************************
			// Find Back and Front Points. Back point is a point 
			// which pass throw 270 degree. Front point pass 
			// throw 90 degree.
			// There are two points in the data point list which will be 
			// placed at the end and at the beginning on the sorted list: Back 
			// point (beginning) and Front point (the end). Back point could 
			// be only after Front point at the unsorted list.
			// *****************************************************
			int	pointIndx = 0;
			startAngle = area.Area3DStyle.Rotation;		
			foreach( DataPoint point in series.Points )
			{	
				// Do not process empty points
				if( point.IsEmpty )
				{
					continue;
				}
							
				// Find angles
				sweepAngle = (float)( Math.Abs(point.YValues[0]) * 360 / sum );
				endAngle = startAngle + sweepAngle;

				startAngleList[ pointIndx ] = (float)startAngle;
				sweepAngleList[ pointIndx ] = (float)sweepAngle;
				pointIndexList[ pointIndx ] = pointIndx;

				// ***************************************************************
				// Find Back point.
				// Because angle could be between -180 and 540 ( Y axis 
				// rotation from -180 to 180 ), Back point could be at -90 and 270
				// ***************************************************************
				if( startAngle <= -90 && endAngle > -90 || startAngle <= 270 && endAngle > 270 && points[0] == null )
				{
                    /*if( points[0] != null ) 
                        throw new InvalidOperationException(SR.ExceptionPiePointOrderInvalid);
                        */
                    backPoint = pointIndx;
					points[0] = point;
					newStartAngleList[0] = startAngleList[pointIndx];
					newSweepAngleList[0] = sweepAngleList[pointIndx];
					newPointIndexList[0] = pointIndexList[pointIndx];
				}

				// ***************************************************************
				// Find Front point.
				// Because angle could be between -180 and 540 ( Y axis 
				// rotation from -180 to 180 ), Front point could be at 90 and 450
				// Case frontPoint == -1 is set because of rounding error.
				// ***************************************************************
				if( startAngle <= 90 && endAngle > 90 || startAngle <= 450 && endAngle > 450 && frontPoint == -1 && ( points[points.Length-1] == null || points.Length == 1 ) )
				{
                    /*	
                   if( points[points.Length-1] != null && points.Length != 1) 
                        throw new InvalidOperationException(SR.ExceptionPiePointOrderInvalid);
                    */
                    frontPoint = pointIndx;
					points[points.Length-1] = point;
					newStartAngleList[points.Length-1] = startAngleList[pointIndx];
					newSweepAngleList[points.Length-1] = sweepAngleList[pointIndx];
					newPointIndexList[points.Length-1] = pointIndexList[pointIndx];
				}

				pointIndx++;
				startAngle += sweepAngle;
			}

			if( frontPoint == -1 || backPoint == -1 )
			{
                throw new InvalidOperationException(SR.ExceptionPieUnassignedFrontBackPoints);
			}

			// If front point and back point are same do not 
			// put same point in two fields.
			if( frontPoint == backPoint && points.Length != 1 )
			{
				points[points.Length-1] = null;
				newStartAngleList[points.Length-1] = 0;
				newSweepAngleList[points.Length-1] = 0;
				newPointIndexList[points.Length-1] = 0;
				sameBackFrontPoint = true;
			}

			// ********************************************
			// Special case. Front Point and Back points 
			// are same.
			// ********************************************
			if( frontPoint == backPoint )
			{
				// Find middle angle of a data point
				float midAngle = startAngleList[backPoint] + sweepAngleList[backPoint] / 2F;

				int	listIndx;
				bool rightSidePoints = false;

				// If big pie slice is on the right and all other 
				// pie slices are on the left.
				if( midAngle > -90 && midAngle < 90 || midAngle > 270 && midAngle < 450 )
				{
					rightSidePoints = true;
				}
				
				listIndx = numOfPoints - frontPoint;
				pointIndx = 0;
				foreach( DataPoint point in series.Points )
				{	
					// Do not process empty points
					if( point.IsEmpty )
					{
						continue;
					}

					// If Front and back points continue with loop
					if( pointIndx == frontPoint )
					{
						pointIndx++;
						continue;
					}

					if( pointIndx < frontPoint )
					{
						if( points[listIndx] != null )
                            throw new InvalidOperationException(SR.ExceptionPiePointOrderInvalid);
						points[listIndx] = point;
						newStartAngleList[listIndx] = startAngleList[pointIndx];
						newSweepAngleList[listIndx] = sweepAngleList[pointIndx];
						newPointIndexList[listIndx] = pointIndexList[pointIndx];

						listIndx++;
					}
					pointIndx++;
				}

				pointIndx = 0;
				listIndx = 1;
				foreach( DataPoint point in series.Points )
				{	
					// Do not process empty points
					if( point.IsEmpty )
					{
						continue;
					}

					// If Front and back points continue with loop
					if( pointIndx == frontPoint )
					{
						pointIndx++;
						continue;
					}

					if( pointIndx > frontPoint )
					{
						if( points[listIndx] != null )
                            throw new InvalidOperationException(SR.ExceptionPiePointOrderInvalid);
						points[listIndx] = point;
						newStartAngleList[listIndx] = startAngleList[pointIndx];
						newSweepAngleList[listIndx] = sweepAngleList[pointIndx];
						newPointIndexList[listIndx] = pointIndexList[pointIndx];
						listIndx++;
					}
					pointIndx++;
				}
				if( rightSidePoints )
				{
					SwitchPoints( numOfPoints, ref points, ref newStartAngleList, ref newSweepAngleList, ref newPointIndexList, backPoint == frontPoint );
				}
			}
			else if( frontPoint < backPoint )
			{
				
				// ************************************************
				// Fill From Back Point to the end of unsorted list
				// ************************************************
				pointIndx = 0;
				int	listIndx = 1;
				foreach( DataPoint point in series.Points )
				{	
					// Do not process empty points
					if( point.IsEmpty )
					{
						continue;
					}

					// If Front and back points continue with loop
					if( pointIndx == frontPoint || pointIndx == backPoint )
					{
						pointIndx++;
						continue;
					}

						// If curent point is after front point.
					else if( pointIndx > backPoint )
					{
						if( points[listIndx] != null )
                            throw new InvalidOperationException(SR.ExceptionPiePointOrderInvalid);
						points[listIndx] = point;
						newStartAngleList[listIndx] = startAngleList[pointIndx];
						newSweepAngleList[listIndx] = sweepAngleList[pointIndx];
						newPointIndexList[listIndx] = pointIndexList[pointIndx];
						listIndx++;
					}

					pointIndx++;
				}

				// ******************************************************
				// Fill from the begining of unsorted list to Front Point
				// ******************************************************
				pointIndx = 0;
				foreach( DataPoint point in series.Points )
				{	
					// Do not process empty points
					if( point.IsEmpty )
					{
						continue;
					}

					// If Front and back points continue with loop
					if( pointIndx == frontPoint || pointIndx == backPoint )
					{
						pointIndx++;
						continue;
					}
			
						// If curent point is before front point.
					else if( pointIndx < frontPoint )
					{
						if( points[listIndx] != null )
                            throw new InvalidOperationException(SR.ExceptionPiePointOrderInvalid);
						points[listIndx] = point;
						newStartAngleList[listIndx] = startAngleList[pointIndx];
						newSweepAngleList[listIndx] = sweepAngleList[pointIndx];
						newPointIndexList[listIndx] = pointIndexList[pointIndx];
						listIndx++;
					}

					pointIndx++;
				}
			

				// *********************************************************
				// This code run only if special case is not active.
				// Special case: FrontPoint and back point are same. This is 
				// happening because pie slice is bigger then 180 degree.
				// *********************************************************
			
			
				// **********************************
				// Fill from Front Point to Back Point
				// **********************************
				listIndx = points.Length - 2;
				pointIndx = 0;
				foreach( DataPoint point in series.Points )
				{	
					// Do not process empty points
					if( point.IsEmpty )
					{
						continue;
					}

					// If Front and back points continue with loop
					if( pointIndx == frontPoint || pointIndx == backPoint )
					{
						pointIndx++;
						continue;
					}

						// If curent point is between front point and back point.
					else if( pointIndx > frontPoint && pointIndx < backPoint )
					{
                        if (points[listIndx] != null) throw new InvalidOperationException(SR.ExceptionPiePointOrderInvalid);
						points[listIndx] = point;
						newStartAngleList[listIndx] = startAngleList[pointIndx];
						newSweepAngleList[listIndx] = sweepAngleList[pointIndx];
						newPointIndexList[listIndx] = pointIndexList[pointIndx];
						listIndx--;
					}

					pointIndx++;
				}
			}
			else
			{
				// **********************************
				// Fill from Back Point to Front Point
				// **********************************
				int listIndx = 1;
				pointIndx = 0;
				foreach( DataPoint point in series.Points )
				{	
					// Do not process empty points
					if( point.IsEmpty )
					{
						continue;
					}

					// If Front and back points continue with loop
					if( pointIndx == frontPoint || pointIndx == backPoint )
					{
						pointIndx++;
						continue;
					}

						// If curent point is between front back and front points.
					else if( pointIndx > backPoint && pointIndx < frontPoint )
					{
                        if (points[listIndx] != null) throw new InvalidOperationException(SR.ExceptionPiePointOrderInvalid);
						points[listIndx] = point;
						newStartAngleList[listIndx] = startAngleList[pointIndx];
						newSweepAngleList[listIndx] = sweepAngleList[pointIndx];
						newPointIndexList[listIndx] = pointIndexList[pointIndx];
						listIndx++;
					}

					pointIndx++;
				}

				// ************************************************
				// Fill From Front Point to the end of unsorted list
				// ************************************************
				listIndx = points.Length - 2;
				pointIndx = 0;
				foreach( DataPoint point in series.Points )
				{	
					// Do not process empty points
					if( point.IsEmpty )
					{
						continue;
					}

					// If Front and back points continue with loop
					if( pointIndx == frontPoint || pointIndx == backPoint )
					{
						pointIndx++;
						continue;
					}
						// If curent point is after front point.
					else if( pointIndx > frontPoint )
					{
						if( points[listIndx] != null )
                            throw new InvalidOperationException(SR.ExceptionPiePointOrderInvalid);
						points[listIndx] = point;
						newStartAngleList[listIndx] = startAngleList[pointIndx];
						newSweepAngleList[listIndx] = sweepAngleList[pointIndx];
						newPointIndexList[listIndx] = pointIndexList[pointIndx];
						listIndx--;
					}

					pointIndx++;
				}

				// ******************************************************
				// Fill from the begining of unsorted list to Back Point
				// ******************************************************
				pointIndx = 0;
				foreach( DataPoint point in series.Points )
				{	
					// Do not process empty points
					if( point.IsEmpty )
					{
						continue;
					}

					// If Front and back points continue with loop
					if( pointIndx == frontPoint || pointIndx == backPoint )
					{
						pointIndx++;
						continue;
					}
			
						// If curent point is before front point.
					else if( pointIndx < backPoint )
					{
						if( points[listIndx] != null )
                            throw new InvalidOperationException(SR.ExceptionPiePointOrderInvalid);
						points[listIndx] = point;
						newStartAngleList[listIndx] = startAngleList[pointIndx];
						newSweepAngleList[listIndx] = sweepAngleList[pointIndx];
						newPointIndexList[listIndx] = pointIndexList[pointIndx];
						listIndx--;
					}

					pointIndx++;
				}
			

				// *********************************************************
				// This code run only if special case is not active.
				// Special case: FrontPoint and back point are same. This is 
				// happening because pie slice is bigger then 180 degree.
				// *********************************************************
			
			
				
			}
			

			// *******************************************************
			// If X angle is positive direction of drawing data points 
			// should be opposite. This part of code switch order of 
			// data points.
			// *******************************************************
			if( area.Area3DStyle.Inclination > 0 ) 
			{
				SwitchPoints( numOfPoints, ref points, ref newStartAngleList, ref newSweepAngleList, ref newPointIndexList, backPoint == frontPoint );
			}
	
			return points;
		}

		/// <summary>
		/// This method switches order of data points in the array of points.
		/// </summary>
		/// <param name="numOfPoints">Number of data points</param>
		/// <param name="points">Array of Data points</param>
		/// <param name="newStartAngleList">List of start angles which has to be switched together with data points</param>
		/// <param name="newSweepAngleList">List of sweep angles which has to be switched together with data points</param>
		/// <param name="newPointIndexList">Indexes (position) of data points in the series</param>
		/// <param name="sameBackFront">There is big pie slice which has same back and front pie slice</param>
		private void SwitchPoints( int numOfPoints, ref DataPoint [] points, ref float [] newStartAngleList, ref float [] newSweepAngleList, ref int [] newPointIndexList, bool sameBackFront )
		{
			float [] tempStartAngles = new float[ numOfPoints ];
			float [] tempSweepAngles = new float[ numOfPoints ];
			int [] tempPointIndexList = new int[ numOfPoints ];
			DataPoint [] tempPoints = new DataPoint[ numOfPoints ];
			int start = 0;;
		
			// The big pie slice (special case) is always on the beginning.
			if( sameBackFront )
			{
				start = 1;

				// Switch order.
				tempPoints[0] = points[0];
				tempStartAngles[0] = newStartAngleList[0];
				tempSweepAngles[0] = newSweepAngleList[0];
				tempPointIndexList[0] = newPointIndexList[0];
			}

			for( int index = start; index < numOfPoints; index++ )
			{
				if( points[ index ] == null )
				{
                    throw new InvalidOperationException(SR.ExceptionPieOrderOperationInvalid);
				}

				// Switch order.
				tempPoints[ numOfPoints - index - 1 + start] = points[ index ];
				tempStartAngles[ numOfPoints - index - 1 + start] = newStartAngleList[ index ];
				tempSweepAngles[ numOfPoints - index - 1 + start] = newSweepAngleList[ index ];
				tempPointIndexList[ numOfPoints - index - 1 + start] = newPointIndexList[ index ];
					
			}

			points = tempPoints;
			newStartAngleList = tempStartAngles;
			newSweepAngleList = tempSweepAngles;
			newPointIndexList = tempPointIndexList;

		}

		#endregion

		#region 3D Label column class
				
		/// <summary>
		/// LabelColumn class is used for labels manipulation - outside label style
		/// </summary>
		internal class LabelColumn
		{
			// Fields of Label Column class
			private RectangleF _chartAreaPosition;
			private RectangleF _innerPlotPosition;
			internal float columnHeight;
			internal int numOfItems = 0;
			private int _numOfInsertedLabels = 0;
			private DataPoint [] _points;
			private float [] _yPositions;
			private bool _rightPosition = true;
			private float _labelLineSize;
			
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="position">Chart Area position.</param>
			public LabelColumn( RectangleF position )
			{
				_chartAreaPosition = position;
			}

			/// <summary>
			/// Return index of label position in the column.
			/// </summary>
			/// <param name="y">y coordinate</param>
			/// <returns>Index of column</returns>
			internal int GetLabelIndex( float y )
			{
				// y coordinate is out of chart area.
				if( y < _chartAreaPosition.Y )
				{
					y = _chartAreaPosition.Y;
				}
				else if( y > _chartAreaPosition.Bottom )
				{
					y = _chartAreaPosition.Bottom - columnHeight;
				}

				return (int) (( y - _chartAreaPosition.Y ) / columnHeight ) ;
			}

			/// <summary>
			/// This method sorts labels by y Position
			/// </summary>
			internal void Sort()
			{
				for( int indexA = 0; indexA < _points.Length; indexA++ )
				{
					for( int indexB = 0; indexB < indexA; indexB++ )
					{
						if( _yPositions[indexA] < _yPositions[indexB] && _points[indexA] != null && _points[indexB] != null )
						{
							float tempYPos;
							DataPoint tempPoint;
							tempYPos = _yPositions[indexA];
							tempPoint = _points[indexA];
							_yPositions[indexA] = _yPositions[indexB];
							_points[indexA] = _points[indexB];
							_yPositions[indexB] = tempYPos;
							_points[indexB] = tempPoint;
						}
					}
				}
			}
		
			/// <summary>
			/// Returns label position y coordinate from index position
			/// </summary>
			/// <param name="index">Index position of the row</param>
			/// <returns>Y coordinate row position</returns>
			internal float GetLabelPosition( int index )
			{
				if( index < 0 || index > numOfItems - 1 )
                    throw new InvalidOperationException(SR.Exception3DPieLabelsIndexInvalid);

				return (float) _chartAreaPosition.Y + columnHeight * index + columnHeight / 2;
			}

			/// <summary>
			/// This method finds X and Y position for outside 
			/// labels. There is discrete number of cells and 
			/// Y position depends on cell position. X position 
			/// is connected with angle between invisible 
			/// line (which connects center of a pie and label) 
			/// and any horizontal line.
			/// </summary>
			/// <param name="dataPoint">Data Point</param>
			/// <returns>Position of a label</returns>
			internal PointF GetLabelPosition( DataPoint dataPoint )
			{
				PointF position = PointF.Empty;
				int pointIndex = 0;

				// Find Y position of Data Point
				// Loop is necessary to find index of data point in the array list.
				foreach( DataPoint point in _points )
				{
					if( point == dataPoint )
					{
						position.Y = GetLabelPosition( pointIndex );
						break;
					}
					pointIndex++;
				}

				// Find initial X position for labels ( All labels are aligne ).
				if( _rightPosition )
				{
					position.X = _innerPlotPosition.Right + _chartAreaPosition.Width * this._labelLineSize;
				}
				else
				{
					position.X = _innerPlotPosition.Left - _chartAreaPosition.Width * this._labelLineSize;
				}

				// Find angle between invisible line (which connects center of a pie and label) 
				// and any horizontal line.
				float angle;
				angle = (float)Math.Atan( ( position.Y - _innerPlotPosition.Top - _innerPlotPosition.Height / 2) / ( position.X - _innerPlotPosition.Left - _innerPlotPosition.Width / 2 ));

				// Make Angle correction for X Position
				float correct;
				if( Math.Cos( angle ) == 0 )
				{
					correct = 0;
				}
				else
				{
					correct = (float)(_innerPlotPosition.Width * 0.4 - _innerPlotPosition.Width * 0.4 / Math.Cos( angle ));
				}

				// Set Corrected X Position
				if( _rightPosition )
				{
					position.X += correct;
				}
				else
				{
					position.X -= correct;
				}

				return position;
			}

			/// <summary>
			/// This method inserts outside labels in Column label list. Column label 
			/// list has defined number of cells. This method has to put labels on 
			/// the best position in the list. If two labels according to their 
			/// positions belong to same cell of the list, this method should 
			/// assign to them different positions.
			/// </summary>
			/// <param name="point">Data Point which label has to be inserted</param>
			/// <param name="yCoordinate">Y coordinate which is the best position for this label</param>
			/// <param name="pointIndx">Point index of this data point in the series</param>
			internal void InsertLabel( DataPoint point, float yCoordinate, int pointIndx )
			{
				
				// Find index of label list by Y value
				int indexYValue = GetLabelIndex( yCoordinate );

				// This position is already used.
				if( _points[indexYValue] != null )
				{
					// All even elements go up and other 
					// Down (If there are many labels which use this position).
					if( pointIndx % 2 == 0 )
					{
						// Check if there is space Down
						if( CheckFreeSpace( indexYValue, false ) )
						{
							// Move labels Down
							MoveLabels( indexYValue, false );
						}
						else
						{
							// Move labels Up
							MoveLabels( indexYValue, true );
						}
					}
					else
					{
						// Check if there is space Up
						if( CheckFreeSpace( indexYValue, true ) )
						{
							// Move labels Up
							MoveLabels( indexYValue, true );
						}
						else
						{
							// Move labels Down
							MoveLabels( indexYValue, false );
						}
					}
				}

				// Set label position
				_points[indexYValue] = point;
				_yPositions[indexYValue] = yCoordinate;
				_numOfInsertedLabels++;
			}
			
			/// <summary>
			/// This method is used for inserting labels. When label is inserted 
			/// and that position was previously used, labels have to be 
			/// moved on proper way. 
			/// </summary>
			/// <param name="position">Position which has to be free</param>
			/// <param name="upDirection">Direction for moving labels</param>
			private void MoveLabels( int position, bool upDirection )
			{
				if( upDirection )
				{
					DataPoint point = _points[position];
					float yValue = _yPositions[position];
					_points[position] = null;
					_yPositions[position] = 0;

					for( int index = position; index > 0; index-- )
					{
						// IsEmpty position found. Stop moving cells UP
						if( _points[index-1] == null )
						{
							_points[index-1] = point;
							_yPositions[index-1] = yValue;
							break;
						}
						else
						{
							DataPoint tempPoint;
							float tempYValue;

							tempPoint = _points[index-1];
							tempYValue = _yPositions[index-1];
							_points[index-1] = point;
							_yPositions[index-1] = yValue;
							point = tempPoint;
							yValue = tempYValue;
						}
					}
				}
				else
				{
					DataPoint point = _points[position];
					float yValue = _yPositions[position];
					_points[position] = null;
					_yPositions[position] = 0;

					for( int index = position; index < numOfItems-1; index++ )
					{
						// IsEmpty position found. Stop moving cells UP
						if( _points[index+1] == null )
						{
							_points[index+1] = point;
							_yPositions[index+1] = yValue;
							break;
						}
						else
						{
							DataPoint tempPoint;
							float tempYValue;

							tempPoint = _points[index+1];
							tempYValue = _yPositions[index+1];
							_points[index+1] = point;
							_yPositions[index+1] = yValue;
							point = tempPoint;
							yValue = tempYValue;
						}
					}
				}
			}

			/// <summary>
			/// This method is used to center labels in 
			/// the middle of chart area (vertically).
			/// </summary>
			internal void AdjustPositions()
			{
				int numEmptyUp = 0;
				int numEmptyDown = 0;

				// Adjust position only if there are many labels
				if( _numOfInsertedLabels < _points.Length / 2 )
					return;

				// Find the number of empty label positions on the top.
				for( int point = 0; point < _points.Length && _points[point] == null; point++ )
				{
					numEmptyUp++;
				}

				// Find the number of empty label positions on the bottom.
				for( int point = _points.Length - 1; point >= 0 && _points[point] == null; point-- )
				{
					numEmptyDown++;
				}

				// Find where are more empty spaces – on the top or on the bottom.
				bool moreEmptyUp = numEmptyUp > numEmptyDown ? true : false;

				// Find average number of empty spaces for top and bottom.
				int numMove = ( numEmptyUp + numEmptyDown ) / 2;

				// If difference between empty spaces on the top and 
				// the bottom is not bigger then 2 do not adjust labels.
				if( Math.Abs( numEmptyUp - numEmptyDown ) < 2 )
					return;

				if( moreEmptyUp )
				{
					// Move labels UP
					int indexPoint = 0;
					for( int point = numMove; point < _points.Length; point++ )
					{
						if(numEmptyUp+indexPoint > _points.Length - 1)
							break;

						_points[point] = _points[numEmptyUp+indexPoint];
						_points[numEmptyUp+indexPoint] = null;
						indexPoint++;
					}
				}
				else
				{
					// Move labels DOWN
					int indexPoint = _points.Length - 1;
					for( int point = _points.Length - 1 - numMove; point >= 0; point-- )
					{
						if(indexPoint - numEmptyDown < 0)
							break;

						_points[point] = _points[indexPoint - numEmptyDown];
						_points[indexPoint - numEmptyDown] = null;
						indexPoint--;
					}
				}
			}


			/// <summary>
			/// Check if there is empty cell Labels column in 
			/// specified direction from specified position
			/// </summary>
			/// <param name="position">Start Position for testing</param>
			/// <param name="upDirection">True if direction is upward, false if downward</param>
			/// <returns>True if there is empty cell</returns>
			private bool CheckFreeSpace( int position, bool upDirection )
			{
				if( upDirection )
				{
					// Position is on the beginning. There is no empty space.
					if( position == 0 )
					{
						return false;
					}

					for( int index = position - 1; index >= 0; index-- )
					{
						// There is empty space
						if( _points[index] == null )
						{
							return true;
						}
					}
				}
				else
				{
					// Position is on the end. There is no empty space.
					if( position == numOfItems - 1 )
					{
						return false;
					}

					for( int index = position + 1; index < numOfItems; index++ )
					{
						// There is empty space
						if( _points[index] == null )
						{
							return true;
						}
					}
				}

				// There is no empty space
				return false;
			}
		
			
			/// <summary>
			/// This method initialize label column.
			/// </summary>
			/// <param name="rectangle">Rectangle used for labels</param>
			/// <param name="rightPosition">True if labels are on the right side of chart area.</param>
			/// <param name="maxNumOfRows">Maximum nuber of rows.</param>
			/// <param name="labelLineSize">Value for label line size from custom attribute.</param>
			internal void Initialize( RectangleF rectangle, bool rightPosition, int maxNumOfRows, float labelLineSize )
			{
				
				// Minimum number of rows.
				numOfItems = Math.Max( numOfItems, maxNumOfRows );

				// Find height of rows
				columnHeight = _chartAreaPosition.Height / numOfItems;

				// Set inner plot position
				_innerPlotPosition = rectangle;

				// Init data column
				_points = new DataPoint[numOfItems];
				
				// Init y position column
				_yPositions = new float[numOfItems];

				// Label column position
				this._rightPosition = rightPosition;

				// 3D Label line size
				this._labelLineSize = labelLineSize;
				
			}

		}

		#endregion // 3D Label column class

		#region 3D Labels
		
		/// <summary>
		/// This method calculates initial pie size if outside 3D labels is active.
		/// </summary>
		/// <param name="graph">Chart Graphics object.</param>
		/// <param name="area">Chart Area.</param>
		/// <param name="pieRectangle">Rectangle which is used for drawing pie.</param>
		/// <param name="pieWidth">Width of pie slice.</param>
		/// <param name="dataPoints">List of data points.</param>
		/// <param name="startAngleList">List of start angles.</param>
		/// <param name="sweepAngleList">List of sweep angles.</param>
		/// <param name="series">Data series used for drawing pie chart.</param>
		/// <param name="labelLineSize">Custom Attribute for label line size.</param>
		private void InitPieSize( 
			ChartGraphics graph, 
			ChartArea area, 
			ref RectangleF pieRectangle, 
			ref float pieWidth, 
			DataPoint [] dataPoints,  
			float [] startAngleList, 
			float [] sweepAngleList, 
			Series series,
			float labelLineSize
			)
		{
			labelColumnLeft = new LabelColumn(area.Position.ToRectangleF());
			labelColumnRight = new LabelColumn(area.Position.ToRectangleF());
			float maxSize = float.MinValue;
			float maxSizeVertical = float.MinValue;
			
			int	pointIndx = 0;
			// Loop which finds max label size and number of label rows.
			foreach( DataPoint point in dataPoints )
			{	
				// Do not process empty points
				if( point.IsEmpty )
				{
					continue;
				}

				float   midAngle = startAngleList[pointIndx] + sweepAngleList[pointIndx] / 2F;

				if( midAngle >= -90 && midAngle < 90 || midAngle >= 270 && midAngle < 450 )
				{
					labelColumnRight.numOfItems++;
				}
				else
				{
					labelColumnLeft.numOfItems++;
				}

				// Find size of the maximum label string.
				SizeF size = graph.MeasureStringRel( GetLabelText( point ).Replace("\\n", "\n"), point.Font );

				maxSize = Math.Max( size.Width, maxSize );
				maxSizeVertical = Math.Max( size.Height, maxSizeVertical );
				
				pointIndx++;
			}

			float oldWidth = pieRectangle.Width;
			float oldHeight = pieRectangle.Height;

			// Find size of inner plot are
			pieRectangle.Width = pieRectangle.Width - 2F * maxSize - 2 * pieRectangle.Width * labelLineSize;

			pieRectangle.Height = pieRectangle.Height - pieRectangle.Height * 0.3F;

			// Size of pie chart can not be less then MinimumRelativePieSize of chart area.
			if( pieRectangle.Width < oldWidth * (float)this.MinimumRelativePieSize( area ) )
			{
				pieRectangle.Width = oldWidth * (float)this.MinimumRelativePieSize( area );
			}

			// Size of pie chart can not be less then MinimumRelativePieSize of chart area.
			if( pieRectangle.Height < oldHeight * (float)this.MinimumRelativePieSize( area ) )
			{
				pieRectangle.Height = oldHeight * (float)this.MinimumRelativePieSize( area );
			}
			
			// Size has to be reduce always because of label lines.
			if( oldWidth * 0.8F < pieRectangle.Width )
			{
				pieRectangle.Width *= 0.8F;
			}

			pieRectangle.X = pieRectangle.X + ( oldWidth - pieRectangle.Width ) / 2F;
			pieWidth = pieRectangle.Width / oldWidth * pieWidth;

			pieRectangle.Y = pieRectangle.Y + ( oldHeight - pieRectangle.Height ) / 2F;

			// Find maximum number of rows. Number of rows will be changed 
			// but this is only recommendation, which depends on font size 
			// and Height of chart area.
			SizeF fontSize = new SizeF(1.4F * series.Font.Size,1.4F * series.Font.Size);
			fontSize = graph.GetRelativeSize( fontSize );
			int maxNumOfRows = (int)( pieRectangle.Height / maxSizeVertical/*fontSize.Height*/ );

			// Initialize label column
			labelColumnRight.Initialize( pieRectangle, true, maxNumOfRows, labelLineSize );
			labelColumnLeft.Initialize( pieRectangle, false, maxNumOfRows, labelLineSize );

		}

		/// <summary>
		/// This method inserts outside 3D labels into array of Label column class.
		/// </summary>
		/// <param name="graph">Chart Graphics object.</param>
		/// <param name="area">Chart Area.</param>
		/// <param name="pieRectangle">Rectangle used for drawing pie slices.</param>
		/// <param name="pieWidth">Width of a pie slice.</param>
		/// <param name="point">Data Point.</param>
		/// <param name="startAngle">Start angle of a pie slice.</param>
		/// <param name="sweepAngle">Sweep angle of a pie slice.</param>
		/// <param name="pointIndx">Data point index.</param>
		/// <param name="doughnutRadius">Inner Radius of the doughnut.</param>
		/// <param name="exploded">true if pie slice is exploded.</param>
		private void FillPieLabelOutside( 
			ChartGraphics graph, 
			ChartArea area, 
			RectangleF pieRectangle, 
			float pieWidth, 
			DataPoint point,  
			float startAngle, 
			float sweepAngle, 
			int pointIndx, 
			float doughnutRadius, 
			bool exploded 
			)
		{
			float midAngle = startAngle + sweepAngle / 2F;

			PointF [] piePoints = GetPiePoints( graph, area, pieWidth, pieRectangle, startAngle, sweepAngle, false, doughnutRadius, exploded );

			float y = piePoints[(int)PiePoints.TopLabelLineout].Y;
			if( midAngle >= -90 && midAngle < 90 || midAngle >= 270 && midAngle < 450 )
			{
				labelColumnRight.InsertLabel( point, y, pointIndx );
			}
			else
			{
				labelColumnLeft.InsertLabel( point, y, pointIndx );
			}
		}
	
		/// <summary>
		/// This method draws outside labels with lines, which 
		/// connect labels with pie slices.
		/// </summary>
		/// <param name="graph">Chart Graphics object</param>
		/// <param name="area">Chart Area</param>
		/// <param name="pen">Pen object</param>
		/// <param name="points">Important pie points</param>
		/// <param name="point">Data point</param>
		/// <param name="midAngle">Middle Angle for pie slice</param>
		/// <param name="pointIndex">Point Index.</param>
		private void Draw3DOutsideLabels( 
			ChartGraphics graph, 
			ChartArea area, 
			Pen pen, 
			PointF [] points, 
			DataPoint point, 
			float midAngle,
			int pointIndex)
		{
			// Take label text
			string text = GetLabelText( point );
			if(text.Length == 0)
			{
				return;
			}

			graph.DrawLine( pen, points[(int)PiePoints.TopLabelLine], points[(int)PiePoints.TopLabelLineout] );
			LabelColumn columnLabel;

            using (StringFormat format = new StringFormat())
            {
                format.LineAlignment = StringAlignment.Center;

                RectangleF chartAreaPosition = graph.GetAbsoluteRectangle(area.Position.ToRectangleF());
                RectangleF labelPosition = RectangleF.Empty;

                PointF labelPoint;

                if (midAngle >= -90 && midAngle < 90 || midAngle >= 270 && midAngle < 450)
                {
                    columnLabel = labelColumnRight;
                    format.Alignment = StringAlignment.Near;

                    float labelVertSize = graph.GetAbsoluteSize(new SizeF(0f, this.labelColumnRight.columnHeight)).Height;
                    labelPoint = graph.GetAbsolutePoint(columnLabel.GetLabelPosition(point));

                    // Label has to be right from TopLabelLineOut
                    if (points[(int)PiePoints.TopLabelLineout].X > labelPoint.X)
                    {
                        labelPoint.X = points[(int)PiePoints.TopLabelLineout].X + 10;
                    }

                    labelPosition.X = labelPoint.X;
                    labelPosition.Width = chartAreaPosition.Right - labelPosition.X;
                    labelPosition.Y = labelPoint.Y - labelVertSize / 2;
                    labelPosition.Height = labelVertSize;

                }
                else
                {
                    columnLabel = labelColumnLeft;
                    format.Alignment = StringAlignment.Far;

                    float labelVertSize = graph.GetAbsoluteSize(new SizeF(0f, this.labelColumnLeft.columnHeight)).Height;
                    labelPoint = graph.GetAbsolutePoint(columnLabel.GetLabelPosition(point));

                    // Label has to be left from TopLabelLineOut
                    if (points[(int)PiePoints.TopLabelLineout].X < labelPoint.X)
                    {
                        labelPoint.X = points[(int)PiePoints.TopLabelLineout].X - 10;
                    }

                    labelPosition.X = chartAreaPosition.X;
                    labelPosition.Width = labelPoint.X - labelPosition.X;
                    labelPosition.Y = labelPoint.Y - labelVertSize / 2;
                    labelPosition.Height = labelVertSize;
                }
                format.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit;
                format.Trimming = StringTrimming.EllipsisWord;

                graph.DrawLine(pen, points[(int)PiePoints.TopLabelLineout], labelPoint);

                // Get label relative position
                labelPosition = graph.GetRelativeRectangle(labelPosition);

                // Get label background position
                SizeF valueTextSize = graph.MeasureStringRel(text.Replace("\\n", "\n"), point.Font);
                valueTextSize.Height += valueTextSize.Height / 8;
                float spacing = valueTextSize.Width / text.Length / 2;
                valueTextSize.Width += spacing;
                RectangleF labelBackPosition = new RectangleF(
                    labelPosition.X,
                    labelPosition.Y + labelPosition.Height / 2f - valueTextSize.Height / 2f,
                    valueTextSize.Width,
                    valueTextSize.Height);

                // Adjust position based on alignment
                if (format.Alignment == StringAlignment.Near)
                {
                    labelBackPosition.X -= spacing / 2f;
                }
                else if (format.Alignment == StringAlignment.Center)
                {
                    labelBackPosition.X = labelPosition.X + (labelPosition.Width - valueTextSize.Width) / 2f;
                }
                else if (format.Alignment == StringAlignment.Far)
                {
                    labelBackPosition.X = labelPosition.Right - valueTextSize.Width - spacing / 2f;
                }

                // Draw label text
                using (Brush brush = new SolidBrush(point.LabelForeColor))
                {
                    graph.DrawPointLabelStringRel(
                        graph.Common,
                        text,
                        point.Font,
                        brush,
                        labelPosition,
                        format,
                        0,
                        labelBackPosition,
                        point.LabelBackColor,
                        point.LabelBorderColor,
                        point.LabelBorderWidth,
                        point.LabelBorderDashStyle,
                        point.series,
                        point,
                        pointIndex);
                }
            }
		}

		/// <summary>
		/// This method draws inside labels.
		/// </summary>
		/// <param name="graph">Chart Graphics object</param>
		/// <param name="points">Important pie points</param>
		/// <param name="point">Data point</param>
		/// <param name="pointIndex">Data point index</param>
		private void Draw3DInsideLabels( ChartGraphics graph, PointF [] points, DataPoint point, int pointIndex )		
		{	
			// Set String Alignment
			StringFormat format = new StringFormat();
			format.LineAlignment = StringAlignment.Center;
			format.Alignment = StringAlignment.Center;

			// Take label text
			string text = GetLabelText( point );

			// Get label relative position
			PointF labelPosition = graph.GetRelativePoint(points[(int)PiePoints.TopLabelCenter]);
			
			// Measure string
			SizeF sizeFont = graph.GetRelativeSize(
				graph.MeasureString(
				text.Replace("\\n", "\n"), 
				point.Font, 
				new SizeF(1000f, 1000f), 
				new StringFormat(StringFormat.GenericTypographic)));
					
			// Get label background position
			RectangleF labelBackPosition = RectangleF.Empty;
			SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
			sizeLabel.Height += sizeFont.Height / 8;
			sizeLabel.Width += sizeLabel.Width / text.Length;
			labelBackPosition = new RectangleF(
				labelPosition.X - sizeLabel.Width/2,
				labelPosition.Y - sizeLabel.Height/2  - sizeFont.Height / 10,
				sizeLabel.Width,
				sizeLabel.Height);

			// Draw label text
            using (Brush brush = new SolidBrush(point.LabelForeColor))
            {
                graph.DrawPointLabelStringRel(
                    graph.Common,
                    text,
                    point.Font,
                    brush,
                    labelPosition,
                    format,
                    0,
                    labelBackPosition,
                    point.LabelBackColor,
                    point.LabelBorderColor,
                    point.LabelBorderWidth,
                    point.LabelBorderDashStyle,
                    point.series,
                    point,
                    pointIndex);
            }
		}

        /// <summary>
        /// Gets the point label.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns></returns>
        private String GetPointLabel(DataPoint point)
        {
            String pointLabel = String.Empty;  
			
            // If There is no Label take axis Label
			if( point.Label.Length == 0 )
            {
                pointLabel = point.AxisLabel;
                // remove axis label if is set the CustomPropertyName.PieAutoAxisLabels and is set to false
                if (point.series != null && 
                    point.series.IsCustomPropertySet(CustomPropertyName.PieAutoAxisLabels) &&
                    String.Equals(point.series.GetCustomProperty(CustomPropertyName.PieAutoAxisLabels), "false", StringComparison.OrdinalIgnoreCase))
                {
                    pointLabel = String.Empty;
                }
            }
			else
				pointLabel = point.Label;

            return point.ReplaceKeywords(pointLabel);
        }

		/// <summary>
		/// Take formated text from label or axis label
		/// </summary>
		/// <param name="point">Data point which is used.</param>
		/// <returns>Formated text</returns>
		private string GetLabelText( DataPoint point )
		{
            string pointLabel = this.GetPointLabel(point);
			// Get label text
			string text;
			if( point.Label.Length == 0 && point.IsValueShownAsLabel )
			{
				text = ValueConverter.FormatValue(
					point.series.Chart,
					point,
                    point.Tag,
					point.YValues[0], 
					point.LabelFormat, 
					point.series.YValueType,
					ChartElementType.DataPoint);
			}
			else
			{
				text = pointLabel;
			}

			// Retuen formated label or axis label text
			return text;
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

