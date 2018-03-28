//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		BarChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	BarChart, RangeBarChart
//
//  Purpose:	Provides 2D/3D drawing and hit testing functionality 
//              for the Bar and RangeBar charts. 
//
//	Reviewed:	AG - August 1, 2002
//              AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces
using System;
using System.Resources;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
	using System.Windows.Forms.DataVisualization.Charting;
#else
    using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
	using System.Web.UI.DataVisualization.Charting.Utilities;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.ChartTypes
#else
	namespace System.Web.UI.DataVisualization.Charting.ChartTypes
#endif
{
	#region Bar label style enumeration

	/// <summary>
	/// Bar chart value label drawing style.
	/// </summary>
	internal enum BarValueLabelDrawingStyle
	{
		/// <summary>
		/// Outside of the bar.
		/// </summary>
		Outside,

		/// <summary>
		/// Inside the bar aligned to the left.
		/// </summary>
		Left,

		/// <summary>
		/// Inside the bar aligned to the center.
		/// </summary>
		Center,

		/// <summary>
		/// Inside the bar aligned to the right.
		/// </summary>
		Right,
	};

	#endregion

	/// <summary>
    /// BarChart class contains all the code necessary to draw 
    /// both Bar and RangeBar charts. The RangeBarChart class is used 
    /// to override few default settings, so that 2 Y values 
    /// will be used to define left and right position of each bar.
	/// </summary>
	internal class RangeBarChart : BarChart
	{
		#region Constructor

		/// <summary>
		/// Public constructor
		/// </summary>
		public RangeBarChart()
		{
			// Set the flag to use two Y values, while drawing the bars
			this.useTwoValues = true;

			// Change default style of the labels
			this.defLabelDrawingStyle = BarValueLabelDrawingStyle.Center;
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		override public string Name			{ get{ return ChartTypeNames.RangeBar;}}

		/// <summary>
		/// If the crossing value is auto Crossing value should be 
		/// automatically set to zero for some chart 
		/// types (Bar, column, area etc.)
		/// </summary>
		override public bool ZeroCrossing { get{ return true;} }

		/// <summary>
		/// Number of supported Y value(s) per point 
		/// </summary>
		override public int YValuesPerPoint{ get { return 2; } }

		/// <summary>
		/// Indicates that extra Y values are connected to the scale of the Y axis
		/// </summary>
		override public bool ExtraYValuesConnectedToYAxis{ get { return true; } }

		#endregion
	}

	/// <summary>
    /// BarChart class provides 2D/3D drawing and hit testing 
    /// functionality for the Bar and RangeBar charts. The only 
    /// difference between the RangeBar and Bar chart is that 
    /// 2 Y values are used to position left and right side 
    /// of each RangeBar bar.
	/// </summary>
	internal class BarChart : IChartType
	{
		#region Fields

			/// <summary>
			/// Indicates that two Y values are used to calculate bar position
			/// </summary>
			protected	bool	useTwoValues = false;

			/// <summary>
			/// Indicates that bars from different series are drawn side by side
			/// </summary>
			protected	bool	drawSeriesSideBySide = true;

			/// <summary>
			/// Defines the default drawing style of the labels.
			/// </summary>
			protected	BarValueLabelDrawingStyle	defLabelDrawingStyle = BarValueLabelDrawingStyle.Outside;

			/// <summary>
			/// Indicates that second point loop is required to draw points labels or markers.
			/// </summary>
			protected	bool	pointLabelsMarkersPresent = false;

		#endregion

		#region Constructor

			/// <summary>
			/// Default constructor
			/// </summary>
			public BarChart()
			{
			}

		#endregion

		#region IChartType interface implementation

			/// <summary>
			/// Chart type name
			/// </summary>
			virtual public string Name			{ get{ return ChartTypeNames.Bar;}}

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
			public bool Stacked		{ get{ return false;}}


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
			public bool RequireAxes	{ get{ return true;} }

			/// <summary>
			/// Chart type with two y values used for scale ( bubble chart type )
			/// </summary>
			public bool SecondYScale{ get{ return false;} }

			/// <summary>
			/// True if chart type requires circular chart area.
			/// </summary>
			public bool CircularChartArea	{ get{ return false;} }

			/// <summary>
			/// True if chart type supports Logarithmic axes
			/// </summary>
			public bool SupportLogarithmicAxes	{ get{ return true;} }

			/// <summary>
			/// True if chart type requires to switch the value (Y) axes position
			/// </summary>
			public bool SwitchValueAxes	{ get{ return true;} }

			/// <summary>
			/// True if chart series can be placed side-by-side.
			/// </summary>
			virtual public bool SideBySideSeries { get{ return true;} }

			/// <summary>
			/// If the crossing value is auto Crossing value should be 
			/// automatically set to zero for some chart 
			/// types (Bar, column, area etc.)
			/// </summary>
			virtual public bool ZeroCrossing { get{ return true;} }

			/// <summary>
			/// True if each data point of a chart must be represented in the legend
			/// </summary>
			public bool DataPointsInLegend	{ get{ return false;} }

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
			public bool ApplyPaletteColorsToPoints	{ get { return false; } }

		
			/// <summary>
			/// How to draw series/points in legend:
			/// Filled rectangle, Line or Marker
			/// </summary>
			/// <param name="series">Legend item series.</param>
			/// <returns>Legend item style.</returns>
			public LegendImageStyle GetLegendImageStyle(Series series)
			{
				return LegendImageStyle.Rectangle;
			}

			/// <summary>
			/// Number of supported Y value(s) per point 
			/// </summary>
			virtual public int YValuesPerPoint{ get { return 1; } }

		#endregion

		#region Painting and selection methods

			/// <summary>
			/// Paint Bar Chart.
			/// </summary>
			/// <param name="graph">The Chart Graphics object.</param>
			/// <param name="common">The Common elements object.</param>
			/// <param name="area">Chart area for this chart.</param>
			/// <param name="seriesToDraw">Chart series to draw.</param>
			public void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
			{		
				// Draw data points
				this.pointLabelsMarkersPresent = false;
				ProcessChartType( false, false, graph, common, area, seriesToDraw );

				// Draw markers and lables
				if(this.pointLabelsMarkersPresent)
				{
					ProcessChartType( true, false, graph, common, area, seriesToDraw );
				}
			}
			
			/// <summary>
			/// Calculates position of each bar in all series and either draws it or checks the selection.
			/// </summary>
			/// <param name="labels">Mode which draws only labels and markers.</param>
			/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
			/// <param name="graph">The Chart Graphics object.</param>
			/// <param name="common">The Common elements object.</param>
			/// <param name="area">Chart area for this chart.</param>
			/// <param name="seriesToDraw">Chart series to draw.</param>
			private void ProcessChartType( 
				bool labels, 
				bool selection, 
				ChartGraphics graph, 
				CommonElements common, 
				ChartArea area, 
				Series seriesToDraw )
			{
				//************************************************************
				//** Local variables declaration
				//************************************************************
				int		seriesIndx = 0;				// Data Series index
				bool	sameInterval = false;		// Series points are equally spaced
							
				// Get pixel size
				SizeF	pixelRelSize = graph.GetRelativeSize(new SizeF(1.1f, 1.1f));


				//************************************************************
				//** Prosess 3D chart type
				//************************************************************
				if(area.Area3DStyle.Enable3D)
				{
					ProcessChartType3D( selection, graph, common, area, seriesToDraw );
					return;
				}
			
		
				//************************************************************
				//** Collect initial series data
				//************************************************************

				// All data series from chart area which have Bar chart type
				List<string> typeSeries = area.GetSeriesFromChartType(Name);

				// Check if series should be drawn side by side
				bool	currentDrawSeriesSideBySide = this.drawSeriesSideBySide;
				foreach(string seriesName in typeSeries)
				{
					if(common.DataManager.Series[seriesName].IsCustomPropertySet(CustomPropertyName.DrawSideBySide))
					{
						string attribValue = common.DataManager.Series[seriesName][CustomPropertyName.DrawSideBySide];
						if(String.Compare(attribValue, "False", StringComparison.OrdinalIgnoreCase ) == 0)
						{
							currentDrawSeriesSideBySide = false;
						}
                        else if (String.Compare(attribValue, "True", StringComparison.OrdinalIgnoreCase) == 0)
						{
							currentDrawSeriesSideBySide = true;
						}
                        else if (String.Compare(attribValue, "Auto", StringComparison.OrdinalIgnoreCase) == 0)
						{
							// Do nothing
						}
						else
						{
                            throw (new InvalidOperationException(SR.ExceptionAttributeDrawSideBySideInvalid));
						}
					}
				}

				// Find the number of "Bar chart" data series
				int numOfSeries = typeSeries.Count;
				if(!currentDrawSeriesSideBySide)
				{
					numOfSeries = 1;
				}

				// Check if bar chart series are indexed
                bool indexedSeries = ChartHelper.IndexedSeries(area.Common, typeSeries.ToArray());

		
				//************************************************************
				//** Loop through all series
				//************************************************************
				foreach( Series ser in common.DataManager.Series )
				{
					// Process non empty series of the area with Bar chart type
					if( String.Compare( ser.ChartTypeName, Name, true, System.Globalization.CultureInfo.CurrentCulture ) != 0 
						|| ser.ChartArea != area.Name || ser.Points.Count == 0 || !ser.IsVisible())
					{
						continue;
					}

					// Set active horizontal axis
					Axis	vAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
					double	vertViewMax = vAxis.ViewMaximum;
					double	vertViewMin = vAxis.ViewMinimum;

					// Set active vertical axis
					Axis	hAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);
					double	horizViewMax = hAxis.ViewMaximum;
					double	horizViewMin = hAxis.ViewMinimum;

					// Get points interval:
					//  - set interval to 1 for indexed series
					//  - if points are not equaly spaced, the minimum interval between points is selected.
					//  - if points have same interval bars do not overlap each other.
					double	interval = 1;
					if(!indexedSeries)
					{
						if(ser.Points.Count == 1 && 
							(ser.XValueType == ChartValueType.Date || 
                             ser.XValueType == ChartValueType.DateTime || 
                             ser.XValueType == ChartValueType.Time ||
                             ser.XValueType == ChartValueType.DateTimeOffset))
						{
							// Check if interval is the same
							area.GetPointsInterval( typeSeries, vAxis.IsLogarithmic, vAxis.logarithmBase, true, out sameInterval );

							// Special case when there is only one data point and date scale is used.
							if(!double.IsNaN(vAxis.majorGrid.GetInterval()) && vAxis.majorGrid.GetIntervalType() != DateTimeIntervalType.NotSet)
							{
                                interval = ChartHelper.GetIntervalSize(vAxis.minimum, vAxis.majorGrid.GetInterval(), vAxis.majorGrid.GetIntervalType());
							}
							else
							{
                                interval = ChartHelper.GetIntervalSize(vAxis.minimum, vAxis.Interval, vAxis.IntervalType);
							}
						}
						else
						{
							interval = area.GetPointsInterval( typeSeries, vAxis.IsLogarithmic, vAxis.logarithmBase, true, out sameInterval );
						}
					}

					// Calculates the width of bars.
					double	width = ser.GetPointWidth(graph, vAxis, interval, 0.8) / numOfSeries;

					// Call Back Paint event
					if( !selection )
					{
                        common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
					}

					//************************************************************
					//** Loop through all points in series
					//************************************************************
					int	pointIndex = 0;
					int	markerIndex = 0;
					foreach( DataPoint point in ser.Points )
					{
						// Check required Y values number
						if(point.YValues.Length < this.YValuesPerPoint)
						{
							throw(new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues(this.Name, this.YValuesPerPoint.ToString(CultureInfo.InvariantCulture) )));
						}

						// Reset pre-calculated point position
						point.positionRel = new PointF(float.NaN, float.NaN);

						// Get Y value and make sure it fits the chart area.
						// If chart type uses 2 Y values (RangeBar) use second Y value for size.
						double	yValue = hAxis.GetLogValue( GetYValue(common, area, ser, point, pointIndex, (useTwoValues) ? 1 : 0) );

						bool yValueOutside = false;
						bool yValueStartOutside = true;
						if( (decimal)yValue > (decimal)horizViewMax )
						{
							yValue = horizViewMax;
							yValueOutside = true;
						}
						else if( (decimal)yValue < (decimal)horizViewMin )
						{
							yValue = horizViewMin;
							yValueOutside = true;
						}

						// Calculate the bar size
						double	barSize = hAxis.GetLinearPosition( yValue );

						// Set start position for a bar
						double	barStartPosition = 0;
						if(useTwoValues)
						{
							// Point Y value (first) is used to determine the bar starting position
							double yValueStart = hAxis.GetLogValue( GetYValue(common, area, ser, point, pointIndex, 0 ) );
							yValueStartOutside = false;
							if( (decimal)yValueStart > (decimal)horizViewMax )
							{
								yValueStart = horizViewMax;
								yValueStartOutside = true;
							}
							else if( (decimal)yValueStart < (decimal)horizViewMin )
							{
								yValueStart = horizViewMin;
								yValueStartOutside = true;
							}

							barStartPosition = hAxis.GetLinearPosition(yValueStart);
						}
						else
						{
							// Bar starts on the vertical axis
							barStartPosition = hAxis.GetPosition(hAxis.Crossing);
						}
					
						// Calculate X position of the Bar
						double	xPosition = 0;
						if( indexedSeries )
						{
							// The formula for position is based on a distance 
							// from the grid line or nPoints position.
							xPosition = vAxis.GetPosition( (double)pointIndex + 1 ) - width * ((double) numOfSeries) / 2.0 + width/2 + seriesIndx * width;
						}
						else if( sameInterval )
						{
							xPosition = vAxis.GetPosition( point.XValue ) - width * ((double) numOfSeries) / 2.0 + width/2 + seriesIndx * width;
						}
						else
						{
							xPosition = vAxis.GetPosition( point.XValue );
						}

						// Make sure that points with small values are still visible
						if( barSize < barStartPosition && 
							(barStartPosition - barSize) <  pixelRelSize.Width)
						{
							barSize = barStartPosition - pixelRelSize.Width;
						}
						if( barSize > barStartPosition && 
							(barSize - barStartPosition) <  pixelRelSize.Width)
						{
							barSize = barStartPosition + pixelRelSize.Width;
						}

						// Set rectangle coordinates of the bar
						RectangleF rectSize = RectangleF.Empty; 
						try
						{
							// Set the bar rectangle
							rectSize.Y = (float)(xPosition - width/2);
							rectSize.Height = (float)(width);

							// The left side of rectangle has always 
							// smaller value than a right value
							if( barStartPosition < barSize )
							{
								rectSize.X = (float)barStartPosition;
								rectSize.Width = (float)barSize - rectSize.X;
							}
							else
							{
								rectSize.X = (float)barSize;
								rectSize.Width = (float)barStartPosition - rectSize.X;
							}
						}
						catch(OverflowException)
						{
							pointIndex++;
							continue;
						}

						// Remeber pre-calculated point position
						point.positionRel = new PointF(( barStartPosition < barSize ) ? rectSize.Right : rectSize.X, (float)xPosition);

						//************************************************************
						//** Painting mode
						//************************************************************
						if( common.ProcessModePaint )
						{
							// if data point is not empty and not labels drawing mode
							if( !point.IsEmpty && !labels)
							{
								// Check if column is completly out of the data scaleView
								double	xValue = (indexedSeries) ? pointIndex + 1 : point.XValue;
								xValue = vAxis.GetLogValue(xValue);
								if(xValue < vertViewMin || xValue > vertViewMax )
								{
									pointIndex++;
									continue;
								}

								// Check if column is partialy in the data scaleView
								bool	clipRegionSet = false;
								if(rectSize.Y < area.PlotAreaPosition.Y || rectSize.Bottom > area.PlotAreaPosition.Bottom)
								{
									// Set clipping region for line drawing 
									graph.SetClip( area.PlotAreaPosition.ToRectangleF() );
									clipRegionSet = true;
								}

								// Start Svg Selection mode
								graph.StartHotRegion( point );

								// Draw the bar rectangle
								graph.FillRectangleRel( rectSize, 
									point.Color, 
									point.BackHatchStyle, 
									point.BackImage, 
									point.BackImageWrapMode, 
									point.BackImageTransparentColor,
									point.BackImageAlignment,
									point.BackGradientStyle, 
									point.BackSecondaryColor, 
									point.BorderColor, 
									point.BorderWidth, 
									point.BorderDashStyle, 
									ser.ShadowColor, 
									ser.ShadowOffset,
									PenAlignment.Inset,
									ChartGraphics.GetBarDrawingStyle(point),
									false);

								// End Svg Selection mode
								graph.EndHotRegion( );

								// Reset Clip Region
								if(clipRegionSet)
								{
									graph.ResetClip();
								}

                                if (common.ProcessModeRegions)
                                {
                                    common.HotRegionsList.AddHotRegion(rectSize, point, ser.Name, pointIndex);
                                }
							}

							// Draw labels and markers (only if part of the bar is visible)
							if( !(yValueOutside && yValueStartOutside && rectSize.Width == 0f) )
							{
                                if ((rectSize.Y + rectSize.Height / 2f) >= area.PlotAreaPosition.Y &&
                                    (rectSize.Y + rectSize.Height / 2f) <= area.PlotAreaPosition.Bottom)
								{
									if(labels)
									{
										DrawLabelsAndMarkers( area, graph, common, rectSize, point, ser, barStartPosition, barSize, width, pointIndex, ref markerIndex);
									}
									else
									{
										// Check if separate drawing loop required for labels and markers
										if(point.MarkerStyle != MarkerStyle.None || point.MarkerImage.Length > 0)
										{
											this.pointLabelsMarkersPresent = true;
										}
										else if(ser.IsValueShownAsLabel || point.IsValueShownAsLabel || point.Label.Length > 0)
										{
											this.pointLabelsMarkersPresent = true;
										}
									}
								}
							}
						}  

                        //************************************************************
						// Hot Regions mode used for image maps, tool tips and 
						// hit test function
						//************************************************************
                        if (common.ProcessModeRegions && !common.ProcessModePaint)
						{
							common.HotRegionsList.AddHotRegion( rectSize, point, ser.Name, pointIndex );

							// Process labels and markers regions only if it was not done while painting
							if(labels )
							{
								DrawLabelsAndMarkers( area, graph, common, rectSize, point, ser, barStartPosition, barSize, width, pointIndex, ref markerIndex);
							}
						}
			
						// Increase the data index counter
						pointIndex++;
					}
						
					// Call Paint event
					if( !selection )
					{
                        common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
					}

					// Increase data series index
					if(currentDrawSeriesSideBySide)
					{
						seriesIndx++;
					}

				}
			}

            /// <summary>
            /// Adjusts size in pixels to the DPIs different than 96.
            /// </summary>
            /// <param name="pixelSize">Size in pixels.</param>
            /// <param name="graph">Chart graphics.</param>
            /// <returns>Adjusted pixel size.</returns>
            private static int GetAdjustedPixelSize(int pixelSize, ChartGraphics graph)
            {
                if (graph != null && graph.Graphics != null)
                {
                    // Marker size is in pixels and we do the mapping for higher DPIs
                    SizeF size = new SizeF();
                    size.Width = pixelSize * graph.Graphics.DpiX / 96;
                    size.Height = pixelSize * graph.Graphics.DpiY / 96;
                    pixelSize = (int)Math.Max(size.Width, size.Height);
                }

                return pixelSize;
            }
		
			/// <summary>
			/// Draws labels and markers.
			/// </summary>
			/// <param name="area">Chart area for this chart.</param>
			/// <param name="graph">The Chart Graphics object.</param>
			/// <param name="common">The Common elements object.</param>
			/// <param name="rectSize">Bar rectangle.</param>
			/// <param name="point">Data point.</param>
			/// <param name="ser">Data series.</param>
			/// <param name="barStartPosition">The zero position or the bottom of bars.</param>
			/// <param name="barSize">The Height of bars.</param>
			/// <param name="width">The width of bars.</param>
			/// <param name="pointIndex">Point index.</param>
			/// <param name="markerIndex">Marker index reference.</param>
			private void DrawLabelsAndMarkers( 
				ChartArea area,
				ChartGraphics graph, 
				CommonElements common, 
				RectangleF rectSize, 
				DataPoint point, 
				Series ser, 
				double barStartPosition, 
				double barSize, 
				double width, 
				int pointIndex, 
				ref int markerIndex)
			{
				
				//************************************************************
				// Draw data point value marker
				//************************************************************
				SizeF	markerSize = SizeF.Empty;
				if(point.MarkerStyle != MarkerStyle.None || point.MarkerImage.Length > 0)
				{
					// Check if this marker should be drawn
					if(markerIndex == 0)
					{
						// Find relative marker size
						if(point.MarkerImage.Length == 0)
						{
							markerSize.Width = point.MarkerSize;
							markerSize.Height = point.MarkerSize;
						}
						else
                            common.ImageLoader.GetAdjustedImageSize(point.MarkerImage, graph.Graphics, ref markerSize);

                        markerSize = graph.GetRelativeSize( markerSize );

						// Calculate marker position
						PointF	markerPosition = PointF.Empty;
						if( barStartPosition < barSize )
						{
							markerPosition.X = rectSize.Right;
						}
						else
						{
							markerPosition.X = rectSize.X;
						}
						markerPosition.Y = rectSize.Y + rectSize.Height/2F;

						if( common.ProcessModePaint )
						{
							// Draw marker
							graph.DrawMarkerRel( 
								markerPosition, 
								point.MarkerStyle,
                                BarChart.GetAdjustedPixelSize(point.MarkerSize, graph),
								(point.MarkerColor == Color.Empty) ? point.Color : point.MarkerColor, 
								point.MarkerBorderColor, 
								point.MarkerBorderWidth,
								point.MarkerImage, 
								point.MarkerImageTransparentColor, 
								(point.series != null) ? point.series.ShadowOffset : 0,
								(point.series != null) ? point.series.ShadowColor : Color.Empty,
								RectangleF.Empty);
						}
						if( common.ProcessModeRegions )
						{
							SetHotRegions(
								common,
								graph,
								point,
                                markerSize,
								point.series.Name,
								pointIndex,
								point.MarkerStyle,
								markerPosition );
						}
					}

					// Increase the markers counter
					++markerIndex;
					if(ser.MarkerStep == markerIndex)
					{
						markerIndex = 0;
					}

				}

				//************************************************************
				// Draw data point value label
				//************************************************************
                if (point.Label.Length > 0 ||
                    (!point.IsEmpty && (ser.IsValueShownAsLabel || point.IsValueShownAsLabel)))
                {
                    // Label rectangle
                    RectangleF rectLabel = RectangleF.Empty;

                    // Label text format
                    using (StringFormat format = new StringFormat())
                    {

					//************************************************************
					// Get label text 
					//************************************************************
					string text;
					if( point.Label.Length == 0 )
					{
						text = ValueConverter.FormatValue(
							ser.Chart, 
							point,
                            point.Tag,
							GetYValue(common, area, ser, point, pointIndex, 0 ), 
							point.LabelFormat, 
							ser.YValueType,
							ChartElementType.DataPoint);
					}
					else
					{
						text = point.ReplaceKeywords(point.Label);
					}

                        //************************************************************
                        // Check labels style custom properties 
                        //************************************************************
                        BarValueLabelDrawingStyle drawingStyle = defLabelDrawingStyle;
                        string valueLabelAttrib = "";
                        if (point.IsCustomPropertySet(CustomPropertyName.BarLabelStyle))
                        {
                            valueLabelAttrib = point[CustomPropertyName.BarLabelStyle];
                        }
                        else if (ser.IsCustomPropertySet(CustomPropertyName.BarLabelStyle))
                        {
                            valueLabelAttrib = ser[CustomPropertyName.BarLabelStyle];
                        }

                        if (valueLabelAttrib.Length > 0)
                        {
                            if (String.Compare(valueLabelAttrib, "Left", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Left;
                            if (String.Compare(valueLabelAttrib, "Right", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Right;
                            if (String.Compare(valueLabelAttrib, "Center", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Center;
                            else if (String.Compare(valueLabelAttrib, "Outside", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Outside;
                        }

                        //************************************************************
                        // Make sure label fits. Otherwise change it style
                        //************************************************************
                        bool labelFit = false;
                        bool labelSwitched = false;
                        bool labelSwitchedBack = false;
                        float prevWidth = 0f;
                        while (!labelFit)
                        {
                            // LabelStyle text format
                            format.Alignment = StringAlignment.Near;
                            format.LineAlignment = StringAlignment.Center;

                            // Label rectangle
                            if (barStartPosition <= barSize)
                            {
                                rectLabel.X = rectSize.Right;
                                rectLabel.Width = area.PlotAreaPosition.Right - rectSize.Right;
                                if (rectLabel.Width < 0.001f &&
                                    barStartPosition == barSize)
                                {
                                    rectLabel.Width = rectSize.X - area.PlotAreaPosition.X;
                                    rectLabel.X = area.PlotAreaPosition.X;
                                    format.Alignment = StringAlignment.Far;
                                }
                            }
                            else
                            {
                                rectLabel.X = area.PlotAreaPosition.X;
                                rectLabel.Width = rectSize.X - area.PlotAreaPosition.X;
                            }

                            // Adjust label rectangle
                            rectLabel.Y = rectSize.Y - (float)width / 2F;
                            rectLabel.Height = rectSize.Height + (float)width;

                            // Adjust label position depending on the drawing style
                            if (drawingStyle == BarValueLabelDrawingStyle.Outside)
                            {
                                // Adjust position if point marker is drawn
                                if (!markerSize.IsEmpty)
                                {
                                    rectLabel.Width -= (float)Math.Min(rectLabel.Width, markerSize.Width / 2F);
                                    if (barStartPosition < barSize)
                                    {
                                        rectLabel.X += (float)Math.Min(rectLabel.Width, markerSize.Width / 2F);
                                    }
                                }
                            }
                            else if (drawingStyle == BarValueLabelDrawingStyle.Left)
                            {
                                rectLabel = rectSize;
                                format.Alignment = StringAlignment.Near;
                            }
                            else if (drawingStyle == BarValueLabelDrawingStyle.Center)
                            {
                                rectLabel = rectSize;
                                format.Alignment = StringAlignment.Center;
                            }
                            else if (drawingStyle == BarValueLabelDrawingStyle.Right)
                            {
                                rectLabel = rectSize;
                                format.Alignment = StringAlignment.Far;

                                // Adjust position if point marker is drawn
                                if (!markerSize.IsEmpty)
                                {
                                    rectLabel.Width -= (float)Math.Min(rectLabel.Width, markerSize.Width / 2F);
                                    if (barStartPosition >= barSize)
                                    {
                                        rectLabel.X += (float)Math.Min(rectLabel.Width, markerSize.Width / 2F);
                                    }
                                }
                            }

                            // Reversed string alignment
                            if (barStartPosition > barSize)
                            {
                                if (format.Alignment == StringAlignment.Far)
                                    format.Alignment = StringAlignment.Near;
                                else if (format.Alignment == StringAlignment.Near)
                                    format.Alignment = StringAlignment.Far;
                            }

                            // Make sure value label fits rectangle. 
                            SizeF valueTextSize = graph.MeasureStringRel(text, point.Font);
                            if (!labelSwitched &&
                                !labelSwitchedBack &&
                                valueTextSize.Width > rectLabel.Width - 1)
                            {
                                // Switch label style only once
                                labelSwitched = true;
                                prevWidth = rectLabel.Width;

                                // If text do not fit - try to switch between Outside/Inside drawing styles
                                if (drawingStyle == BarValueLabelDrawingStyle.Outside)
                                {
                                    drawingStyle = BarValueLabelDrawingStyle.Right;
                                }
                                else
                                {
                                    drawingStyle = BarValueLabelDrawingStyle.Outside;
                                }
                            }
                            else
                            {
                                // If label do not fit either Outside or to the Right,
                                // select the style that has more space available.
                                if (labelSwitched &&
                                    !labelSwitchedBack &&
                                    valueTextSize.Width > rectLabel.Width - 1 &&
                                    prevWidth > rectLabel.Width)
                                {
                                    labelSwitchedBack = true;

                                    // Change back to the previous labels style
                                    if (drawingStyle == BarValueLabelDrawingStyle.Outside)
                                    {
                                        drawingStyle = BarValueLabelDrawingStyle.Right;
                                    }
                                    else
                                    {
                                        drawingStyle = BarValueLabelDrawingStyle.Outside;
                                    }
                                }
                                else
                                {
                                    // Do not try to fit labels any more
                                    labelFit = true;
                                }
                            }
                        }

                        //************************************************************
                        // Draw label
                        //************************************************************

                        // Calculate label background position
                        RectangleF labelBackPosition = RectangleF.Empty;
                        if (common.ProcessModeRegions ||
                            !point.LabelBackColor.IsEmpty ||
                            !point.LabelBorderColor.IsEmpty)
                        {
                            if (rectLabel.Width > 0 && rectLabel.Height > 0)
                            {
                                // Get label background position
                                SizeF valueTextSize = graph.MeasureStringRel(text, point.Font);
                                valueTextSize.Height += valueTextSize.Height / 8;
                                float spacing = valueTextSize.Width / text.Length / 2;
                                valueTextSize.Width += spacing;
                                labelBackPosition = new RectangleF(
                                    rectLabel.X,
                                    rectLabel.Y + (rectLabel.Height - valueTextSize.Height) / 2,
                                    valueTextSize.Width,
                                    valueTextSize.Height);

                                // Adjust position based on alignment
                                if (format.Alignment == StringAlignment.Near)
                                {
                                    labelBackPosition.X += spacing / 2f;
                                    rectLabel.X += spacing;
                                }
                                else if (format.Alignment == StringAlignment.Center)
                                {
                                    labelBackPosition.X = rectLabel.X + (rectLabel.Width - valueTextSize.Width) / 2f;
                                }
                                else if (format.Alignment == StringAlignment.Far)
                                {
                                    labelBackPosition.X = rectLabel.Right - valueTextSize.Width - spacing / 2f;
                                    rectLabel.X -= spacing;
                                }
                            }
                        }

                        // Make sure there is enough vertical space for the label
                        // NOTE: Fixes issue #4502
                        SizeF textSize = graph.MeasureStringRel(text, point.Font);
                        if (textSize.Height > rectLabel.Height)
                        {
                            rectLabel.Y -= (textSize.Height - rectLabel.Height) / 2f;
                            rectLabel.Height = textSize.Height;
                        }

                        // Draw label text
                        using (Brush brush = new SolidBrush(point.LabelForeColor))
                        {
                            graph.DrawPointLabelStringRel(
                                common,
                                text,
                                point.Font,
                                brush,
                                rectLabel,
                                format,
                                point.LabelAngle,
                                labelBackPosition,

                                point.LabelBackColor,
                                point.LabelBorderColor,
                                point.LabelBorderWidth,
                                point.LabelBorderDashStyle,
                                ser,
                                point,
                                pointIndex);
                        }
                    }
                }
			}

			/// <summary>
			/// Inserts Hot Regions used for image maps, tool tips and 
			/// hit test function
			/// </summary>
			/// <param name="common">Common elements object</param>
			/// <param name="graph">Chart Graphics object</param>
			/// <param name="point">Data point used for hot region</param>
			/// <param name="markerSize">Size of the marker</param>
			/// <param name="seriesName">Name of the series</param>
			/// <param name="pointIndex">Data point index</param>
			/// <param name="pointMarkerStyle">Marker Style</param>
			/// <param name="markerPosition">Marker Position</param>
			private void SetHotRegions( CommonElements common, ChartGraphics graph, DataPoint point, SizeF markerSize, string seriesName, int pointIndex, MarkerStyle pointMarkerStyle, PointF markerPosition )
			{
				
				// Get relative marker size
				SizeF relativeMarkerSize = markerSize;

				int insertIndex = common.HotRegionsList.FindInsertIndex();

				// Insert circle area
				if( pointMarkerStyle == MarkerStyle.Circle )
				{
					common.HotRegionsList.AddHotRegion( insertIndex, graph, markerPosition.X, markerPosition.Y, relativeMarkerSize.Width/2f, point, seriesName, pointIndex ); 
				}
					// All other markers represented as rectangles
				else
				{
					// Insert area
					common.HotRegionsList.AddHotRegion( 
						new RectangleF(markerPosition.X - relativeMarkerSize.Width/2f, markerPosition.Y - relativeMarkerSize.Height/2f, relativeMarkerSize.Width, relativeMarkerSize.Height),
						point,
						seriesName,
						pointIndex );
				}
			
			}

		#endregion

		#region Getting Y value methods
	
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
				// Point chart do not have height
				if(yValueIndex == -1)
				{
					return 0.0;
				}

                // Check required Y values number
                if (point.YValues.Length <= yValueIndex)
                {
                    throw (new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues(this.Name, this.YValuesPerPoint.ToString(CultureInfo.InvariantCulture))));
                }

                if (point.IsEmpty || double.IsNaN(point.YValues[yValueIndex]))
				{
					// Get empty point value
                    double result = GetEmptyPointValue(point, pointIndex, yValueIndex);

                    // NOTE: Fixes issue #6921
                    // If empty point Y value is zero then check if the scale of
                    // the Y axis and if it is not containing zero adjust the Y value
                    // of the empty point, so it will be visible
                    if (result == 0.0)
                    {
                        Axis yAxis = area.GetAxis(AxisName.Y, series.YAxisType, series.YSubAxisName);
                        double yViewMax = yAxis.maximum;
                        double yViewMin = yAxis.minimum;
                        if (result < yViewMin)
                        {
                            result = yViewMin;
                        }
                        else if (result > yViewMax)
                        {
                            result = yViewMax;
                        }
                    }

                    return result;
				}

				// Return Y value from the point
				return point.YValues[yValueIndex];
			}

			/// <summary>
			/// This method will find previous and next data point, which is not 
			/// empty and recalculate a new value for current empty data point. 
			/// New value depends on custom attribute “EmptyPointValue” and 
			/// it could be zero or average.
			/// </summary>
			/// <param name="point">IsEmpty data point.</param>
			/// <param name="pointIndex">IsEmpty data point index.</param>
			/// <param name="yValueIndex">Index of the Y value to get.</param>
			/// <returns>A Value for empty data point.</returns>
			internal double GetEmptyPointValue( DataPoint point, int pointIndex, int yValueIndex)
			{
				Series	series = point.series;				// Data series
				double	previousPoint = 0;					// Previous data point value (not empty)
				double	nextPoint = 0;						// Next data point value (not empty)
				int		prevIndx = 0;						// Previous data point index
				int		nextIndx = series.Points.Count - 1;	// Next data point index

				//************************************************************
				//** Check custom attribute "EmptyPointValue"
				//************************************************************
				string emptyPointValue = "";
				if( series.EmptyPointStyle.IsCustomPropertySet(CustomPropertyName.EmptyPointValue) )
				{
					emptyPointValue = series.EmptyPointStyle[CustomPropertyName.EmptyPointValue];
				}
				else if( series.IsCustomPropertySet(CustomPropertyName.EmptyPointValue) )
				{
					emptyPointValue = series[CustomPropertyName.EmptyPointValue];
				}

				// Take attribute value
                if (String.Compare(emptyPointValue, "Zero", StringComparison.OrdinalIgnoreCase) == 0)
				{
					// IsEmpty points represented with zero values
					return 0;
				}
			
				//************************************************************
				//** IsEmpty point value is an average of neighbour points
				//************************************************************

				// Find previous non-empty point value
				for( int indx = pointIndex; indx >= 0; indx-- )
				{
					if( !series.Points[indx].IsEmpty )
					{
						previousPoint = series.Points[indx].YValues[yValueIndex];
						prevIndx = indx;
						break;
					}
					previousPoint = Double.NaN;
				}

				// Find next non-empty point value
				for( int indx = pointIndex; indx < series.Points.Count; indx++ )
				{
					if( !series.Points[indx].IsEmpty )
					{
						nextPoint = series.Points[indx].YValues[yValueIndex];
						nextIndx = indx;
						break;
					}
					nextPoint = Double.NaN;
				}

				// All Previous points are empty
				if( Double.IsNaN( previousPoint ) )
				{
					// All points are empty
					if( Double.IsNaN( nextPoint ) )
					{
						previousPoint = 0;
					}
					else // Next point is equal to previous point
					{
						previousPoint = nextPoint;
					}
				}

				// All next points are empty
				if( Double.IsNaN( nextPoint ) )
				{
					// Previous point is equal to next point
					nextPoint = previousPoint;
				}

				// If points value are the same use average
				if( series.Points[nextIndx].XValue == series.Points[prevIndx].XValue )
				{
					return ( previousPoint + nextPoint ) / 2;
				}

				// Calculate and return average value
				double aCoeff = (previousPoint - nextPoint) / (series.Points[nextIndx].XValue - series.Points[prevIndx].XValue);
				return -aCoeff * (point.XValue - series.Points[prevIndx].XValue) + previousPoint;
			}

		#endregion

		#region 3D Drawing and Selection

			/// <summary>
			/// Calculates position of each bar in all series and either draws it or checks the selection in 3D space.
			/// </summary>
			/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
			/// <param name="graph">The Chart Graphics object.</param>
			/// <param name="common">The Common elements object.</param>
			/// <param name="area">Chart area for this chart.</param>
			/// <param name="seriesToDraw">Chart series to draw.</param>
			private void ProcessChartType3D( 
				bool selection, 
				ChartGraphics graph, 
				CommonElements common, 
				ChartArea area, 
				Series seriesToDraw )
			{
				//************************************************************
				//** Local variables declaration
				//************************************************************

				// Get pixel size
				SizeF	pixelRelSize = graph.GetRelativeSize(new SizeF(1.1f, 1.1f));


				//************************************************************
				//** Get list of series to draw
				//************************************************************
				double	xValue = 0; 
				List<string> typeSeries = null;
				bool	currentDrawSeriesSideBySide = this.drawSeriesSideBySide;
				if( (area.Area3DStyle.IsClustered && this.SideBySideSeries) ||
					this.Stacked)
				{
					// Draw all series of the same chart type
					typeSeries = area.GetSeriesFromChartType(Name);

					// Check if series should be drawn side by side
					foreach(string seriesName in typeSeries)
					{
						if(common.DataManager.Series[seriesName].IsCustomPropertySet(CustomPropertyName.DrawSideBySide))
						{
							string attribValue = common.DataManager.Series[seriesName][CustomPropertyName.DrawSideBySide];
							if(String.Compare(attribValue, "False", StringComparison.OrdinalIgnoreCase) == 0)
							{
								currentDrawSeriesSideBySide = false;
							}
							else if(String.Compare(attribValue, "True", StringComparison.OrdinalIgnoreCase) == 0)
							{
								currentDrawSeriesSideBySide = true;
							}
							else if(String.Compare(attribValue, "Auto", StringComparison.OrdinalIgnoreCase) == 0)
							{
								// Do nothing
							}
							else
							{
                                throw (new InvalidOperationException(SR.ExceptionAttributeDrawSideBySideInvalid));
							}
						}
					}
				}
				else
				{
					// Draw just one chart series
					typeSeries = new List<string>();
					typeSeries.Add(seriesToDraw.Name);
				}

				//************************************************************
				//** Get order of data points drawing
				//************************************************************
				ArrayList	dataPointDrawingOrder = area.GetDataPointDrawingOrder(
					typeSeries, 
					this, 
					selection, 
					COPCoordinates.X | COPCoordinates.Y, 
					new BarPointsDrawingOrderComparer(area, selection, COPCoordinates.X | COPCoordinates.Y),
					0,
					currentDrawSeriesSideBySide);

				//************************************************************
				//** Loop through all data poins
				//************************************************************
				bool	drawLabels = false;
				foreach(object obj in dataPointDrawingOrder)
				{
					// Get point & series
					DataPoint3D	pointEx = (DataPoint3D) obj;
					DataPoint	point = pointEx.dataPoint;
					Series		ser = point.series;

					// Check required Y values number
					if(point.YValues.Length < this.YValuesPerPoint)
					{
						throw(new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues(this.Name, this.YValuesPerPoint.ToString(CultureInfo.InvariantCulture))));
					}

					// Reset pre-calculated point position
					point.positionRel = new PointF(float.NaN, float.NaN);

					// Set active vertical/horizontal axis
					Axis	vAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
					Axis	hAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

					// Get point bar drawing style
					BarDrawingStyle	barDrawingStyle = ChartGraphics.GetBarDrawingStyle(point);

					// Get Y value and make sure it fits the chart area.
					// If chart type uses 2 Y values (RangeBar) use second Y value for size.
					float	rightDarkening = 0f;
					float	leftDarkening = 0f;
					double	yValue = hAxis.GetLogValue( GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, (useTwoValues) ? 1 : 0) );
					if( yValue > hAxis.ViewMaximum )
					{
						rightDarkening = 0.5f;
						yValue = hAxis.ViewMaximum;
					}
					else if( yValue < hAxis.ViewMinimum )
					{
						rightDarkening = 0.5f;
						yValue = hAxis.ViewMinimum;
					}

					// Calculate the bar size
					double	barSize = hAxis.GetLinearPosition( yValue );

					// Set start position for a bar
					double	barStartPosition = 0;
					if(useTwoValues)
					{
						// Point Y value (first) is used to determine the bar starting position
						double yValueStart = hAxis.GetLogValue( GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, 0 ) );
						if( yValueStart > hAxis.ViewMaximum )
						{
							leftDarkening = 0.5f;
							yValueStart = hAxis.ViewMaximum;
						}
						else if( yValueStart < hAxis.ViewMinimum )
						{
							leftDarkening = 0.5f;
							yValueStart = hAxis.ViewMinimum;
						}

						barStartPosition = hAxis.GetLinearPosition(yValueStart);
					}
					else
					{
						// Bar starts on the vertical axis
						barStartPosition = hAxis.GetPosition(hAxis.Crossing);
					}

					// Calculate X position of the Bar
					double	xPosition = pointEx.xPosition;

					// Make sure that points with small values are still visible
					if( barSize < barStartPosition && 
						(barStartPosition - barSize) <  pixelRelSize.Width)
					{
						barSize = barStartPosition - pixelRelSize.Width;
					}
					if( barSize > barStartPosition && 
						(barSize - barStartPosition) <  pixelRelSize.Width)
					{
						barSize = barStartPosition + pixelRelSize.Width;
					}

					// Set rectangle coordinates of the bar
					RectangleF rectSize = RectangleF.Empty; 
					try
					{
						// Set the bar rectangle
						rectSize.Y = (float)(xPosition - pointEx.width/2);
						rectSize.Height = (float)(pointEx.width);

						// The left side of rectangle has always 
						// smaller value than a right value
						if( barStartPosition < barSize )
						{
							rectSize.X = (float)barStartPosition;
							rectSize.Width = (float)barSize - rectSize.X;
						}
						else
						{
							float temp = rightDarkening;
							rightDarkening = leftDarkening;
							leftDarkening = temp;

							rectSize.X = (float)barSize;
							rectSize.Width = (float)barStartPosition - rectSize.X;
						}
					}
					catch(OverflowException)
					{
						continue;
					}

					// Remeber pre-calculated point position
					point.positionRel = new PointF(rectSize.Right, (float)xPosition);


					//************************************************************
					//** Painting mode
					//************************************************************
					GraphicsPath	rectPath = null;
					
					// if data point is not empty
					if( !point.IsEmpty )
					{
						// Check if column is completly out of the data scaleView
						xValue = (pointEx.indexedSeries) ? pointEx.index : point.XValue;
						xValue = vAxis.GetLogValue(xValue);
						if(xValue < vAxis.ViewMinimum || xValue > vAxis.ViewMaximum )
						{
							continue;
						}

						// Check if column is partialy in the data scaleView
						bool	clipRegionSet = false;
						if(rectSize.Bottom <= area.PlotAreaPosition.Y || rectSize.Y >= area.PlotAreaPosition.Bottom)
						{
							continue;
						}
						if(rectSize.Y < area.PlotAreaPosition.Y)
						{
							rectSize.Height -= area.PlotAreaPosition.Y - rectSize.Y;
							rectSize.Y = area.PlotAreaPosition.Y;
						}
						if(rectSize.Bottom > area.PlotAreaPosition.Bottom)
						{
							rectSize.Height -= rectSize.Bottom - area.PlotAreaPosition.Bottom;
						}
						if(rectSize.Height < 0)
						{
							rectSize.Height = 0;
						}
						if(rectSize.Height == 0f || rectSize.Width == 0f)
						{
							continue;
						}

						// Detect if we need to get graphical path of drawn object
						DrawingOperationTypes	drawingOperationType = DrawingOperationTypes.DrawElement;
				
						if( common.ProcessModeRegions )
						{
							drawingOperationType |= DrawingOperationTypes.CalcElementPath;
						}

						// Start Svg Selection mode
						graph.StartHotRegion( point );

						// Draw the bar rectangle
						rectPath = graph.Fill3DRectangle( 
							rectSize, 
							pointEx.zPosition,
							pointEx.depth,
							area.matrix3D,
							area.Area3DStyle.LightStyle,
							point.Color, 
							rightDarkening,
							leftDarkening,
							point.BorderColor, 
							point.BorderWidth, 
							point.BorderDashStyle, 
							barDrawingStyle,
							false,
							drawingOperationType );

						// End Svg Selection mode
						graph.EndHotRegion( );

						// Reset Clip Region
						if(clipRegionSet)
						{
							graph.ResetClip();
						}
					}

					// Draw 3D markers
					DrawMarkers3D( area, graph, common, rectSize, pointEx, ser, barStartPosition, barSize );

					// Check if labels should be drawn (in additional points loop)
					if( point.IsValueShownAsLabel || point.Label.Length > 0 )
					{
						drawLabels = true;
					}

					//************************************************************
					// Hot Regions mode used for image maps, tool tips and 
					// hit test function
					//************************************************************
					if( common.ProcessModeRegions )
					{
						common.HotRegionsList.AddHotRegion(
							rectPath,
							false,
							graph,
							point,
							ser.Name,
							pointEx.index - 1 );
					}
                    if (rectPath != null)
                    {
                        rectPath.Dispose();
                    }
				}


				//************************************************************
				//** Loop through all data poins and draw labels
				//************************************************************
				if(drawLabels)
				{
					foreach(object obj in dataPointDrawingOrder)
					{
						// Get point & series
						DataPoint3D	pointEx = (DataPoint3D) obj;
						DataPoint	point = pointEx.dataPoint;
						Series		ser = point.series;

						// Set active vertical/horizontal axis
						Axis	vAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
						Axis	hAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

						// Get Y value and make sure it fits the chart area.
						// If chart type uses 2 Y values (RangeBar) use second Y value for size.
						double	yValue = hAxis.GetLogValue( GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, (useTwoValues) ? 1 : 0) );
						if( yValue > hAxis.ViewMaximum )
						{
							yValue = hAxis.ViewMaximum;
						}
						else if( yValue < hAxis.ViewMinimum )
						{
							yValue = hAxis.ViewMinimum;
						}

						// Calculate the bar size
						double	barSize = hAxis.GetLinearPosition( yValue );

						// Set start position for a bar
						double	barStartPosition = 0;
						if(useTwoValues)
						{
							// Point Y value (first) is used to determine the bar starting position
							double yValueStart = hAxis.GetLogValue( GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, 0 ) );
							if( yValueStart > hAxis.ViewMaximum )
							{
								yValueStart = hAxis.ViewMaximum;
							}
							else if( yValueStart < hAxis.ViewMinimum )
							{
								yValueStart = hAxis.ViewMinimum;
							}

							barStartPosition = hAxis.GetLinearPosition(yValueStart);
						}
						else
						{
							// Bar starts on the vertical axis
							barStartPosition = hAxis.GetPosition(hAxis.Crossing);
						}

						// Calculate X position of the Bar
						double	xPosition = pointEx.xPosition;

						// Set rectangle coordinates of the bar
						RectangleF rectSize = RectangleF.Empty; 
						try
						{
							// Set the bar rectangle
							rectSize.Y = (float)(xPosition - pointEx.width/2);
							rectSize.Height = (float)(pointEx.width);

							// The left side of rectangle has always 
							// smaller value than a right value
							if( barStartPosition < barSize )
							{
								rectSize.X = (float)barStartPosition;
								rectSize.Width = (float)barSize - rectSize.X;
							}
							else
							{
								rectSize.X = (float)barSize;
								rectSize.Width = (float)barStartPosition - rectSize.X;
							}
						}
						catch(OverflowException)
						{
							continue;
						}


						//************************************************************
						//** Painting mode
						//************************************************************
						// if data point is not empty
						if( !point.IsEmpty )
						{
							// Check if column is completly out of the data scaleView
							xValue = (pointEx.indexedSeries) ? pointEx.index : point.XValue;
							xValue = vAxis.GetLogValue(xValue);
							if(xValue < vAxis.ViewMinimum || xValue > vAxis.ViewMaximum )
							{
								continue;
							}

							// Check if column is partialy in the data scaleView
                            if ((decimal)rectSize.Y >= (decimal)area.PlotAreaPosition.Y && (decimal)rectSize.Bottom <= (decimal)area.PlotAreaPosition.Bottom)
                            {
                                // Draw 3D labels
                                DrawLabels3D(area, graph, common, rectSize, pointEx, ser, barStartPosition, barSize, pointEx.width, pointEx.index - 1);
                            }
                        }
					}				
				}
					
			}

			/// <summary>
			/// Draws markers in 3D.
			/// </summary>
			/// <param name="area">Chart area for this chart.</param>
			/// <param name="graph">The Chart Graphics object.</param>
			/// <param name="common">The Common elements object.</param>
			/// <param name="rectSize">Bar rectangle.</param>
			/// <param name="pointEx">Data point.</param>
			/// <param name="ser">Data series.</param>
			/// <param name="barStartPosition">The zero position or the bottom of bars.</param>
			/// <param name="barSize">The Height of bars.</param>
			private void DrawMarkers3D( 
				ChartArea area,
				ChartGraphics graph, 
				CommonElements common, 
				RectangleF rectSize, 
				DataPoint3D pointEx, 
				Series ser, 
				double barStartPosition, 
				double barSize)
			{
				DataPoint point = pointEx.dataPoint;

				//************************************************************
				// Draw data point value marker
				//************************************************************
				SizeF	markerSize = SizeF.Empty;
				if(point.MarkerStyle != MarkerStyle.None || point.MarkerImage.Length > 0)
				{
					// Check if this marker should be drawn
					if((pointEx.index % ser.MarkerStep) == 0)
					{
						// Find relative marker size
						if(point.MarkerImage.Length == 0)
						{
							markerSize.Width = point.MarkerSize;
							markerSize.Height = point.MarkerSize;
						}
						else
						    common.ImageLoader.GetAdjustedImageSize(point.MarkerImage, graph.Graphics, ref markerSize);
						
                        markerSize = graph.GetRelativeSize( markerSize );

						// Calculate marker position
						PointF	markerPosition = PointF.Empty;
						if( barStartPosition < barSize )
						{
							markerPosition.X = rectSize.Right;
						}
						else
						{
							markerPosition.X = rectSize.X;
						}
						markerPosition.Y = rectSize.Y + rectSize.Height/2F;

						//************************************************************
						//** Transform marker position in 3D space
						//************************************************************
						// Get projection coordinates
						Point3D[]	marker3DPosition = new Point3D[1];
						marker3DPosition[0] = new Point3D(markerPosition.X, markerPosition.Y, (float)(pointEx.zPosition + pointEx.depth/2f));

						// Transform coordinates of text size
						area.matrix3D.TransformPoints(marker3DPosition);


						//************************************************************
						//** Draw 3D marker
						//************************************************************
						graph.DrawMarker3D(
							area.matrix3D, 
							area.Area3DStyle.LightStyle,
							pointEx.zPosition + pointEx.depth/2f,
							markerPosition, 
							point.MarkerStyle,
                            BarChart.GetAdjustedPixelSize(point.MarkerSize, graph), 
							(point.MarkerColor.IsEmpty) ? point.series.Color : point.MarkerColor, 
							point.MarkerBorderColor, 
							point.MarkerBorderWidth,
							point.MarkerImage, 
							point.MarkerImageTransparentColor, 
							(point.series != null) ? point.series.ShadowOffset : 0,
							(point.series != null) ? point.series.ShadowColor : Color.Empty,
							RectangleF.Empty,
							DrawingOperationTypes.DrawElement);
					}
				}
			}


			/// <summary>
			/// Draws labels in 3D.
			/// </summary>
			/// <param name="area">Chart area for this chart.</param>
			/// <param name="graph">The Chart Graphics object.</param>
			/// <param name="common">The Common elements object.</param>
			/// <param name="rectSize">Bar rectangle.</param>
			/// <param name="pointEx">Data point.</param>
			/// <param name="ser">Data series.</param>
			/// <param name="barStartPosition">The zero position or the bottom of bars.</param>
			/// <param name="barSize">The Height of bars.</param>
			/// <param name="width">The width of bars.</param>
			/// <param name="pointIndex">Point index.</param>
			private void DrawLabels3D( 
				ChartArea area,
				ChartGraphics graph, 
				CommonElements common, 
				RectangleF rectSize, 
				DataPoint3D pointEx, 
				Series ser, 
				double barStartPosition, 
				double barSize, 
				double width, 
				int pointIndex)
			{
				DataPoint point = pointEx.dataPoint;

                //************************************************************
                // Draw data point value label
                //************************************************************
                if (ser.IsValueShownAsLabel || point.IsValueShownAsLabel || point.Label.Length > 0)
                {
                    // Label rectangle
                    RectangleF rectLabel = RectangleF.Empty;
                    // Label text format
                    using (StringFormat format = new StringFormat())
                    {

					//************************************************************
					// Get label text 
					//************************************************************
					string text;
					if( point.Label.Length == 0 )
					{
						text = ValueConverter.FormatValue(
							ser.Chart,
							point,
                            point.Tag,
							GetYValue(common, area, ser, point, pointIndex, 0 ), 
							point.LabelFormat, 
							ser.YValueType,
							ChartElementType.DataPoint);
					}
					else
					{
						text = point.ReplaceKeywords(point.Label);
					}

					//************************************************************
					// Calculate marker size
					//************************************************************
					SizeF	markerSize = SizeF.Empty;
					if(point.MarkerStyle != MarkerStyle.None || point.MarkerImage.Length > 0)
					{
						// Check if this marker should be drawn
						if((pointEx.index % ser.MarkerStep) == 0)
						{
							// Find relative marker size
                            if (point.MarkerImage.Length == 0)
                            {
                                markerSize.Width = point.MarkerSize;
                                markerSize.Height = point.MarkerSize;
                            }
                            else
                                common.ImageLoader.GetAdjustedImageSize(point.MarkerImage, graph.Graphics, ref markerSize);
							
                            markerSize = graph.GetRelativeSize( markerSize );
						}
					}

                        //************************************************************
                        // Check labels style custom properties 
                        //************************************************************
                        BarValueLabelDrawingStyle drawingStyle = defLabelDrawingStyle;
                        string valueLabelAttrib = "";
                        if (point.IsCustomPropertySet(CustomPropertyName.BarLabelStyle))
                        {
                            valueLabelAttrib = point[CustomPropertyName.BarLabelStyle];
                        }
                        else if (ser.IsCustomPropertySet(CustomPropertyName.BarLabelStyle))
                        {
                            valueLabelAttrib = ser[CustomPropertyName.BarLabelStyle];
                        }

                        if (valueLabelAttrib != null && valueLabelAttrib.Length > 0)
                        {
                            if (String.Compare(valueLabelAttrib, "Left", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Left;
                            else if (String.Compare(valueLabelAttrib, "Right", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Right;
                            else if (String.Compare(valueLabelAttrib, "Center", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Center;
                            else if (String.Compare(valueLabelAttrib, "Outside", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Outside;
                        }

                        //************************************************************
                        // Make sure label fits. Otherwise change it style
                        //************************************************************
                        bool labelFit = false;
                        bool labelSwitched = false;
                        bool labelSwitchedBack = false;
                        float prevWidth = 0f;
                        while (!labelFit)
                        {
                            // Label text format
                            format.Alignment = StringAlignment.Near;
                            format.LineAlignment = StringAlignment.Center;

                            // LabelStyle rectangle
                            if (barStartPosition < barSize)
                            {
                                rectLabel.X = rectSize.Right;
                                rectLabel.Width = area.PlotAreaPosition.Right - rectSize.Right;
                            }
                            else
                            {
                                rectLabel.X = area.PlotAreaPosition.X;
                                rectLabel.Width = rectSize.X - area.PlotAreaPosition.X;
                            }

                            // Adjust label rectangle
                            rectLabel.Y = rectSize.Y - (float)width / 2F;
                            rectLabel.Height = rectSize.Height + (float)width;

                            // Adjust label position depending on the drawing style
                            if (drawingStyle == BarValueLabelDrawingStyle.Outside)
                            {
                                // Adjust position if point marker is drawn
                                if (!markerSize.IsEmpty)
                                {
                                    rectLabel.Width -= (float)Math.Min(rectLabel.Width, markerSize.Width / 2F);
                                    if (barStartPosition < barSize)
                                    {
                                        rectLabel.X += (float)Math.Min(rectLabel.Width, markerSize.Width / 2F);
                                    }
                                }
                            }
                            else if (drawingStyle == BarValueLabelDrawingStyle.Left)
                            {
                                rectLabel = rectSize;
                                format.Alignment = StringAlignment.Near;
                            }
                            else if (drawingStyle == BarValueLabelDrawingStyle.Center)
                            {
                                rectLabel = rectSize;
                                format.Alignment = StringAlignment.Center;
                            }
                            else if (drawingStyle == BarValueLabelDrawingStyle.Right)
                            {
                                rectLabel = rectSize;
                                format.Alignment = StringAlignment.Far;

                                // Adjust position if point marker is drawn
                                if (!markerSize.IsEmpty)
                                {
                                    rectLabel.Width -= (float)Math.Min(rectLabel.Width, markerSize.Width / 2F);
                                    if (barStartPosition >= barSize)
                                    {
                                        rectLabel.X += (float)Math.Min(rectLabel.Width, markerSize.Width / 2F);
                                    }
                                }
                            }

                            // Reversed string alignment
                            if (barStartPosition >= barSize)
                            {
                                if (format.Alignment == StringAlignment.Far)
                                    format.Alignment = StringAlignment.Near;
                                else if (format.Alignment == StringAlignment.Near)
                                    format.Alignment = StringAlignment.Far;
                            }

                            // Make sure value label fits rectangle. 
                            SizeF valueTextSize = graph.MeasureStringRel(text, point.Font);
                            if (!labelSwitched &&
                                !labelSwitchedBack &&
                                valueTextSize.Width > rectLabel.Width)
                            {
                                // Switch label style only once
                                labelSwitched = true;
                                prevWidth = rectLabel.Width;

                                // If text do not fit - try to switch between Outside/Inside drawing styles
                                if (drawingStyle == BarValueLabelDrawingStyle.Outside)
                                {
                                    drawingStyle = BarValueLabelDrawingStyle.Right;
                                }
                                else
                                {
                                    drawingStyle = BarValueLabelDrawingStyle.Outside;
                                }
                            }
                            else
                            {
                                // If label do not fit either Outside or to the Right,
                                // select the style that has more space available.
                                if (labelSwitched &&
                                    !labelSwitchedBack &&
                                    valueTextSize.Width > rectLabel.Width - 1 &&
                                    prevWidth > rectLabel.Width)
                                {
                                    labelSwitchedBack = true;

                                    // Change back to the previous labels style
                                    if (drawingStyle == BarValueLabelDrawingStyle.Outside)
                                    {
                                        drawingStyle = BarValueLabelDrawingStyle.Right;
                                    }
                                    else
                                    {
                                        drawingStyle = BarValueLabelDrawingStyle.Outside;
                                    }
                                }
                                else
                                {
                                    // Do not try to fit labels any more
                                    labelFit = true;
                                }
                            }
                        }

                        //************************************************************
                        // Find text rotation center point
                        //************************************************************

                        // Measure string size
                        SizeF size = graph.MeasureStringRel(text, point.Font, new SizeF(rectLabel.Width, rectLabel.Height), format);

                        PointF rotationCenter = PointF.Empty;
                        if (format.Alignment == StringAlignment.Near)
                        { // Near
                            rotationCenter.X = rectLabel.X + size.Width / 2;
                        }
                        else if (format.Alignment == StringAlignment.Far)
                        { // Far
                            rotationCenter.X = rectLabel.Right - size.Width / 2;
                        }
                        else
                        { // Center
                            rotationCenter.X = (rectLabel.Left + rectLabel.Right) / 2;
                        }

                        if (format.LineAlignment == StringAlignment.Near)
                        { // Near
                            rotationCenter.Y = rectLabel.Top + size.Height / 2;
                        }
                        else if (format.LineAlignment == StringAlignment.Far)
                        { // Far
                            rotationCenter.Y = rectLabel.Bottom - size.Height / 2;
                        }
                        else
                        { // Center
                            rotationCenter.Y = (rectLabel.Bottom + rectLabel.Top) / 2;
                        }

                        // Reset string alignment to center point
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Center;


                        //************************************************************
                        // Adjust label rotation angle
                        //************************************************************
                        int angle = point.LabelAngle;

                        // Get projection coordinates
                        Point3D[] rotationCenterProjection = new Point3D[] { 
						new Point3D(rotationCenter.X, rotationCenter.Y, pointEx.zPosition + pointEx.depth),
						new Point3D(rotationCenter.X - 20f, rotationCenter.Y, pointEx.zPosition + pointEx.depth) };

                        // Transform coordinates of text rotation point
                        area.matrix3D.TransformPoints(rotationCenterProjection);

                        // Adjust rotation point
                        rotationCenter = rotationCenterProjection[0].PointF;

                        // Adjust angle of the horisontal text
                        if (angle == 0 || angle == 180)
                        {
                            // Convert coordinates to absolute
                            rotationCenterProjection[0].PointF = graph.GetAbsolutePoint(rotationCenterProjection[0].PointF);
                            rotationCenterProjection[1].PointF = graph.GetAbsolutePoint(rotationCenterProjection[1].PointF);

                            // Calcuate axis angle
                            float angleXAxis = (float)Math.Atan(
                                (rotationCenterProjection[1].Y - rotationCenterProjection[0].Y) /
                                (rotationCenterProjection[1].X - rotationCenterProjection[0].X));
                            angleXAxis = (float)Math.Round(angleXAxis * 180f / (float)Math.PI);
                            angle += (int)angleXAxis;
                        }


                        // Calculate label background position
                        RectangleF labelBackPosition = RectangleF.Empty;
                        if (common.ProcessModeRegions ||
                            !point.LabelBackColor.IsEmpty ||
                            !point.LabelBorderColor.IsEmpty)
                        {
                            SizeF sizeLabel = new SizeF(size.Width, size.Height);
                            sizeLabel.Height += sizeLabel.Height / 8;
                            sizeLabel.Width += sizeLabel.Width / text.Length;
                            labelBackPosition = new RectangleF(
                                rotationCenter.X - sizeLabel.Width / 2,
                                rotationCenter.Y - sizeLabel.Height / 2,
                                sizeLabel.Width,
                                sizeLabel.Height);
                        }

                        //************************************************************
                        // Draw label
                        //************************************************************
                        using (Brush brush = new SolidBrush(point.LabelForeColor))
                        {
                            graph.DrawPointLabelStringRel(
                                common,
                                text,
                                point.Font,
                                brush,
                                rotationCenter,
                                format,
                                angle,
                                labelBackPosition,
                                point.LabelBackColor,
                                point.LabelBorderColor,
                                point.LabelBorderWidth,
                                point.LabelBorderDashStyle,
                                ser,
                                point,
                                pointIndex);
                        }
                    }

                }
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
            // NOTE: Bar chart do not support SmartLabelStyle feature.
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

	#region Points drawing order comparer class

	/// <summary>
	/// Chart 3D engine relies on the data point drawing order 
    /// to achieve correct visual appearance. All data points 
    /// have to be drawn in the correct order depending on the 
    /// 3D angles, perspective and the depth of the series.
    /// 
    /// BarPointsDrawingOrderComparer class is used sort data 
    /// points of the Bar chart type.
	/// </summary>
	internal class BarPointsDrawingOrderComparer : IComparer
    {
        #region Fields

        /// <summary>
		/// Chart area object reference.
		/// </summary>
		private	ChartArea	_area = null;

		/// <summary>
		/// Area X position where visible sides are switched.
		/// </summary>
		private	Point3D		_areaProjectionCenter = new Point3D(float.NaN, float.NaN, float.NaN);

		/// <summary>
		/// Selection mode. Points order should be reversed.
		/// </summary>
		private bool		_selection = false;

        #endregion // Fields

        #region Methods

        /// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="area">Chart area.</param>
		/// <param name="selection">Selection indicator.</param>
		/// <param name="coord">Which coordinate of COP (X, Y or Z) to test for surface everlapping.</param>
		public BarPointsDrawingOrderComparer(ChartArea	area, bool selection, COPCoordinates coord)
		{
			this._area = area;
			this._selection = selection;

			// Get center of projection
			if(area.DrawPointsToCenter(ref coord))
			{
				// Get COP
				_areaProjectionCenter = area.GetCenterOfProjection(coord);

				// Switch X & Y coordinates
				float val = _areaProjectionCenter.X;
				_areaProjectionCenter.X = _areaProjectionCenter.Y;
				_areaProjectionCenter.Y = val;
			}
		}

		/// <summary>
		/// Comarer method.
		/// </summary>
		/// <param name="o1">First object.</param>
		/// <param name="o2">Second object.</param>
		/// <returns>Comparison result.</returns>
		public int Compare(object o1, object o2)
		{
			DataPoint3D point1 = (DataPoint3D) o1;
			DataPoint3D point2 = (DataPoint3D) o2;

			int	result = 0;
			if(point1.xPosition < point2.xPosition)
			{
				result = -1;
			}
			else if(point1.xPosition > point2.xPosition)
			{
				result = 1;
			}
			else
			{
				// If X coordinate is the same - filter by Y coordinate
				if(point1.yPosition < point2.yPosition)
				{
					result = 1;
				}
				else if(point1.yPosition > point2.yPosition)
				{
					result = -1;
				}
				
				// Order points from sides to center
				if(!float.IsNaN(_areaProjectionCenter.Y))
				{
					double yMin1 = Math.Min(point1.yPosition, point1.height);
					double yMax1 = Math.Max(point1.yPosition, point1.height);
					double yMin2 = Math.Min(point2.yPosition, point2.height);
					double yMax2 = Math.Max(point2.yPosition, point2.height);

					if(_area.IsBottomSceneWallVisible())
					{
						if( yMin1 <= _areaProjectionCenter.Y && yMin2 <= _areaProjectionCenter.Y )
						{
							result *= -1;
						}
						else if( yMin1 <= _areaProjectionCenter.Y)
						{
							result = 1;
						}

					}
					else
					{
						
						if( yMax1 >= _areaProjectionCenter.Y && yMax2 >= _areaProjectionCenter.Y )
						{
							result *= 1;
						}
						else if( yMax1 >= _areaProjectionCenter.Y)
						{
							result = 1;
						}
						else
						{
							result *= -1;
						}
					}
				}
				
				// Reversed order if looking from left or right
				else if(!_area.DrawPointsInReverseOrder())
				{
					result *= -1;
				}
			}

			if(point1.xPosition != point2.xPosition)
			{
				// Order points from sides to center
				if(!float.IsNaN(_areaProjectionCenter.X))
				{
					if((point1.xPosition + point1.width / 2f) >= _areaProjectionCenter.X && 
						(point2.xPosition + point2.width / 2f) >= _areaProjectionCenter.X)
					{
						result *= -1;
					}
				}

					// Reversed order of points by X value
				else if(_area.IsBottomSceneWallVisible())
				{
					result *= -1;
				}
			}

			return (_selection) ? - result : result;
        }

    #endregion // Methods
    }

#endregion	
}
