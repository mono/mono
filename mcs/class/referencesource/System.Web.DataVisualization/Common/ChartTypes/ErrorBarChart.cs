//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ErrorBarChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	ErrorBarChart
//
//  Purpose:	Provides 2D and 3D drawing and hit testing of the 
//              ErrorBar chart.
//
//  Error Bar Chart Overview:
//  -------------------------
//  Error bar charts consist of lines with markers that are used to 
//  display statistical information about the data displayed in a 
//  graph. An Error Bar chart type is a series that has 3 Y values. 
//  While it is true that these values can be assigned to each point 
//  in an Error Bar chart, in most cases, the values will be 
//  calculated from the data present in another series. 
//  
//  The order of the Y values is important because each position in 
//  the array of values represents a value on the Error Bar:
//      0 - Center or Average point value
//      1 - Lower Error value
//      2 - Upper Error value
//  
//	Reviewed:	GS - Jul 15, 2003
//              AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel.Design;
using System.Globalization;
using System.Collections.Generic;

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
	#region Enumerations

	/// <summary>
	/// Defines the way error amount is calculated.
	/// </summary>
	internal enum ErrorBarType
	{
		/// <summary>
		/// Error is a fixed value.
		/// </summary>
		FixedValue,

		/// <summary>
		/// Error is percentage of the center value.
		/// </summary>
		Percentage,

		/// <summary>
		/// Error is standard deviation of all center values in series.
		/// </summary>
		StandardDeviation,

		/// <summary>
		/// Error is standard error of all center values in series.
		/// </summary>
		StandardError
	}

	/// <summary>
	/// Error bars drawing styles.
	/// </summary>
	internal enum ErrorBarStyle
	{
		/// <summary>
		/// Draws both lower and upper error bar.
		/// </summary>
		Both,

		/// <summary>
		/// Draws only upper error bar.
		/// </summary>
		UpperError,

		/// <summary>
		/// Draws only lower error bar.
		/// </summary>
		LowerError
	}

	#endregion

	/// <summary>
    /// ErrorBarChart class provides 2D and 3D drawing and hit testing of
    /// the ErrorBar chart.
	/// </summary>
	internal class ErrorBarChart : IChartType
	{
		#region Fields

		/// <summary>
		/// Vertical axis
		/// </summary>
		protected	Axis	vAxis = null;	

		/// <summary>
		/// Horizontal axis
		/// </summary>
		protected	Axis	hAxis = null;

		#endregion

		#region Constructor

		/// <summary>
		/// Error bar chart constructor.
		/// </summary>
		public ErrorBarChart()
		{
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		virtual public string Name			{ get{ return ChartTypeNames.ErrorBar;}}

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
		/// True if chart type supports axes
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
		/// If the crossing value is auto Crossing value ZeroCrossing should be 
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
		/// Indicates that this is a one hundred percent chart.
		/// Axis scale from 0 to 100 percent should be used.
		/// </summary>
		virtual public bool HundredPercent{ get{return false;} }

		/// <summary>
		/// Indicates that this is a one hundred percent chart.
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
		virtual public int YValuesPerPoint	{ get { return 3; } }

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

		#region Painting and Selection methods

		/// <summary>
		/// Paint error bar chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		virtual public void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
		{	
			ProcessChartType( false, graph, common, area, seriesToDraw );
		}

		/// <summary>
		/// This method recalculates size of the bars. This method is used 
		/// from Paint or Select method.
		/// </summary>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		virtual protected void ProcessChartType( 
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			Series seriesToDraw )
		{
			// Prosess 3D chart type
			if(area.Area3DStyle.Enable3D)
			{
				ProcessChartType3D( selection, graph, common, area, seriesToDraw );
				return;
			}

			// All data series from chart area which have Error bar chart type
			List<string>	typeSeries = area.GetSeriesFromChartType(this.Name);

			// Zero X values mode.
            bool indexedSeries = ChartHelper.IndexedSeries(common, typeSeries.ToArray());

			//************************************************************
			//** Loop through all series
			//************************************************************
			int seriesIndex = 0;
			foreach( Series ser in common.DataManager.Series )
			{
				// Process non empty series of the area with error bar chart type
				if( String.Compare( ser.ChartTypeName, this.Name, StringComparison.OrdinalIgnoreCase ) != 0 
					|| ser.ChartArea != area.Name || !ser.IsVisible())
				{
					continue;
				}

				// Set active horizontal/vertical axis
				hAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
				vAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

				// Get interval between points
				double interval = (indexedSeries) ? 1 : area.GetPointsInterval( hAxis.IsLogarithmic, hAxis.logarithmBase );

				// Calculates points width
				float	width = (float)(ser.GetPointWidth(graph, hAxis, interval, 0.4));


				// Align error bar X position with linked series				
				float	sideBySideWidth = width;
				int		numberOfLinkedSeries = 1;
				int		indexOfLinkedSeries = 0;
				bool	showSideBySide = false;
				string	linkedSeriesName = string.Empty;
				bool	currentDrawSeriesSideBySide = false;
				if(ser.IsCustomPropertySet(CustomPropertyName.ErrorBarSeries))
				{
					// Get series name
					linkedSeriesName = ser[CustomPropertyName.ErrorBarSeries];
					int valueTypeIndex = linkedSeriesName.IndexOf(":", StringComparison.Ordinal);
					if(valueTypeIndex >= 0)
					{
						linkedSeriesName = linkedSeriesName.Substring(0, valueTypeIndex);
					}

					// All linked data series from chart area which have Error bar chart type
					string linkedSeriesChartType = common.DataManager.Series[linkedSeriesName].ChartTypeName;
                    ChartArea linkedSeriesArea = common.ChartPicture.ChartAreas[common.DataManager.Series[linkedSeriesName].ChartArea];
                    List<string> typeLinkedSeries = linkedSeriesArea.GetSeriesFromChartType(linkedSeriesChartType);

					// Get index of linked serries
					foreach(string name in typeLinkedSeries)
					{
						if(name == linkedSeriesName)
						{
							break;
						}
						++indexOfLinkedSeries;
					}

					
					currentDrawSeriesSideBySide = false;
					if(String.Compare(linkedSeriesChartType, ChartTypeNames.Column, StringComparison.OrdinalIgnoreCase) ==0 
                        || String.Compare(linkedSeriesChartType, ChartTypeNames.RangeColumn, StringComparison.OrdinalIgnoreCase) == 0
                        )
					{
						currentDrawSeriesSideBySide = true;
					}
					foreach(string seriesName in typeLinkedSeries)
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

					if(currentDrawSeriesSideBySide)
					{
						// Find the number of linked data series
						numberOfLinkedSeries = typeLinkedSeries.Count;
						width /= numberOfLinkedSeries;
						showSideBySide = true;

						// Check if side by side
						if(!indexedSeries)
						{
							area.GetPointsInterval( typeLinkedSeries, hAxis.IsLogarithmic, hAxis.logarithmBase, true, out showSideBySide );
						}

						sideBySideWidth = (float)(common.DataManager.Series[linkedSeriesName].GetPointWidth(graph, hAxis, interval, 0.8)) / numberOfLinkedSeries;
					}
				}

				// Check if side-by-side attribute is set
				if(!currentDrawSeriesSideBySide && ser.IsCustomPropertySet(CustomPropertyName.DrawSideBySide))
				{
					string attribValue = ser[CustomPropertyName.DrawSideBySide];
					if(String.Compare(attribValue, "False", StringComparison.OrdinalIgnoreCase) ==0)
					{
						showSideBySide = false;
					}
					else if(String.Compare(attribValue, "True", StringComparison.OrdinalIgnoreCase) ==0)
					{
						showSideBySide = true;
						numberOfLinkedSeries = typeSeries.Count;
						indexOfLinkedSeries = seriesIndex;
						width /= numberOfLinkedSeries;

						// NOTE: Lines of code below were added to fix issue #4048
						sideBySideWidth = (float)(ser.GetPointWidth(graph, hAxis, interval, 0.8)) / numberOfLinkedSeries;
					}
					else if(String.Compare(attribValue, "Auto", StringComparison.OrdinalIgnoreCase) ==0)
					{
					}
					else
					{
                        throw (new InvalidOperationException(SR.ExceptionAttributeDrawSideBySideInvalid));
					}
				}


				// Call Back Paint event
				if( !selection )
				{
                    common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}


				//************************************************************
				//** Series data points loop
				//************************************************************
				int	index = 1;
				foreach( DataPoint point in ser.Points )
				{
					// Check required Y values number
					if(point.YValues.Length < this.YValuesPerPoint)
					{
						throw(new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues(this.Name, this.YValuesPerPoint.ToString(CultureInfo.InvariantCulture))));
					}

					// Reset pre-calculated point position
					point.positionRel = new PointF(float.NaN, float.NaN);

					// Get point X position
					float xPosition = 0f;
					double	xValue = point.XValue;
					if( indexedSeries )
					{
						xValue = (double)index;
					//	xPosition = (float)(hAxis.GetPosition( (double)index ) - sideBySideWidth * ((double) numberOfLinkedSeries) / 2.0 + sideBySideWidth/2 + indexOfLinkedSeries * sideBySideWidth);
					}
					
					if( showSideBySide )
					{
						xPosition = (float)(hAxis.GetPosition( xValue ) - sideBySideWidth * ((double) numberOfLinkedSeries) / 2.0 + sideBySideWidth/2 + indexOfLinkedSeries * sideBySideWidth);
					}
					else
					{
						xPosition = (float)hAxis.GetPosition( xValue );
					}
						
					double yValue0 = vAxis.GetLogValue( point.YValues[1] );
					double yValue1 = vAxis.GetLogValue( point.YValues[2] );
					xValue = hAxis.GetLogValue(xValue);
					
					// Check if chart is completly out of the data scaleView
					if(xValue < hAxis.ViewMinimum || 
						xValue > hAxis.ViewMaximum ||
						(yValue0 < vAxis.ViewMinimum && yValue1 < vAxis.ViewMinimum) ||
						(yValue0 > vAxis.ViewMaximum && yValue1 > vAxis.ViewMaximum) )
					{
						++index;
						continue;
					}
						
					// Make sure High/Low values are in data scaleView range						
					double	low = vAxis.GetLogValue( point.YValues[1] );
					double	high = vAxis.GetLogValue( point.YValues[2] );

					// Check if lower and/or upper error bar are drawn
					ErrorBarStyle	barStyle = ErrorBarStyle.Both;
					if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle) || ser.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle))
					{
						string errorBarStyle = ser[CustomPropertyName.ErrorBarStyle];
						if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle))
						{
							errorBarStyle = point[CustomPropertyName.ErrorBarStyle];
						}

                        if(String.Compare( errorBarStyle, "Both", StringComparison.OrdinalIgnoreCase ) == 0)
						{
							// default - do nothing
						}
						else if(String.Compare(errorBarStyle, "UpperError", StringComparison.OrdinalIgnoreCase ) == 0)
						{
							barStyle = ErrorBarStyle.UpperError;
							low = vAxis.GetLogValue( point.YValues[0] );
							high = vAxis.GetLogValue( point.YValues[2] );
						}
						else if(String.Compare(errorBarStyle, "LowerError", StringComparison.OrdinalIgnoreCase ) == 0)
						{
							barStyle = ErrorBarStyle.LowerError;
							low = vAxis.GetLogValue( point.YValues[1] );
							high = vAxis.GetLogValue( point.YValues[0] );
						}
						else
						{
							throw(new InvalidOperationException( SR.ExceptionCustomAttributeValueInvalid( point[CustomPropertyName.ErrorBarStyle], "ErrorBarStyle")));
						}
					}

					// Check if values are in range
					if( high > vAxis.ViewMaximum )
					{
						high = vAxis.ViewMaximum;
					}
					if( high < vAxis.ViewMinimum )
					{
						high = vAxis.ViewMinimum;
					}
					high = (float)vAxis.GetLinearPosition(high);
					
					if( low > vAxis.ViewMaximum )
					{
						low = vAxis.ViewMaximum;
					}
					if( low < vAxis.ViewMinimum )
					{
						low = vAxis.ViewMinimum;
					}
					low = vAxis.GetLinearPosition(low);

					// Remeber pre-calculated point position
					point.positionRel = new PointF((float)xPosition, (float)Math.Min(high, low));

					if( common.ProcessModePaint )
					{

						// Check if chart is partialy in the data scaleView
						bool	clipRegionSet = false;
						if(xValue == hAxis.ViewMinimum || xValue == hAxis.ViewMaximum )
						{
							// Set clipping region for line drawing 
							graph.SetClip( area.PlotAreaPosition.ToRectangleF() );
							clipRegionSet = true;
						}

                        // Start Svg Selection mode
						graph.StartHotRegion( point );

						// Draw error bar line
						graph.DrawLineRel( 
							point.Color, 
							point.BorderWidth, 
							point.BorderDashStyle, 
							new PointF(xPosition, (float)high), 
							new PointF(xPosition, (float)low),
							ser.ShadowColor, 
							ser.ShadowOffset );

						// Draw Error Bar marks
						DrawErrorBarMarks(graph, barStyle, area, ser, point, xPosition, width);

						// End Svg Selection mode
						graph.EndHotRegion( );

						// Reset Clip Region
						if(clipRegionSet)
						{
							graph.ResetClip();
						}
					}

					if( common.ProcessModeRegions )
					{
						// Calculate rect around the error bar marks
						RectangleF	areaRect = RectangleF.Empty;
						areaRect.X = xPosition - width / 2f;
						areaRect.Y = (float)Math.Min(high, low);
						areaRect.Width = width;
						areaRect.Height = (float)Math.Max(high, low) - areaRect.Y;

						// Add area
						common.HotRegionsList.AddHotRegion( areaRect, point, ser.Name, index - 1 );
					}
					++index;
				}

				//************************************************************
				//** Second series data points loop, when labels are drawn.
				//************************************************************
				if( !selection )
				{
					index = 1;
					foreach( DataPoint point in ser.Points )
					{
						// Get point X position
						float xPosition = 0f;
						double	xValue = point.XValue;
						if( indexedSeries )
						{
							xValue = (double)index;
							xPosition = (float)(hAxis.GetPosition( (double)index ) - sideBySideWidth * ((double) numberOfLinkedSeries) / 2.0 + sideBySideWidth/2 + indexOfLinkedSeries * sideBySideWidth);
						}
						else if( showSideBySide )
						{
							xPosition = (float)(hAxis.GetPosition( xValue ) - sideBySideWidth * ((double) numberOfLinkedSeries) / 2.0 + sideBySideWidth/2 + indexOfLinkedSeries * sideBySideWidth);
						}
						else
						{
							xPosition = (float)hAxis.GetPosition( xValue );
						}

						double yValue0 = vAxis.GetLogValue( point.YValues[1] );
						double yValue1 = vAxis.GetLogValue( point.YValues[2] );
						xValue = hAxis.GetLogValue(xValue);
					
						// Check if chart is completly out of the data scaleView
						if(xValue < hAxis.ViewMinimum || 
							xValue > hAxis.ViewMaximum ||
							(yValue0 < vAxis.ViewMinimum && yValue1 < vAxis.ViewMinimum) ||
							(yValue0 > vAxis.ViewMaximum && yValue1 > vAxis.ViewMaximum) )
						{
							++index;
							continue;
						}

						// Make sure High/Low values are in data scaleView range						
						double	high = vAxis.GetLogValue( point.YValues[1] );
						double	low = vAxis.GetLogValue( point.YValues[2] );
					
						// Check if lower and/or upper error bar are drawn
						if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle) || ser.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle))
						{
							string errorBarStyle = ser[CustomPropertyName.ErrorBarStyle];
							if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle))
							{
								errorBarStyle = point[CustomPropertyName.ErrorBarStyle];
							}
							if(String.Compare(errorBarStyle, "Both", StringComparison.OrdinalIgnoreCase) == 0)
							{
								// default - do nothing
							}
							else if(String.Compare(errorBarStyle, "UpperError", StringComparison.OrdinalIgnoreCase) == 0)
							{
								low = vAxis.GetLogValue( point.YValues[0] );
								high = vAxis.GetLogValue( point.YValues[2] );
							}
							else if(String.Compare(errorBarStyle, "LowerError", StringComparison.OrdinalIgnoreCase) == 0)
							{
								low = vAxis.GetLogValue( point.YValues[1] );
								high = vAxis.GetLogValue( point.YValues[0] );
							}
							else
							{
								throw(new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(point[CustomPropertyName.ErrorBarStyle], "ErrorBarStyle")));
							}
						}


						if( high > vAxis.ViewMaximum )
						{
							high = vAxis.ViewMaximum;
						}
						if( high < vAxis.ViewMinimum )
						{
							high = vAxis.ViewMinimum;
						}
						high = (float)vAxis.GetLinearPosition(high);
					
						if( low > vAxis.ViewMaximum )
						{
							low = vAxis.ViewMaximum;
						}
						if( low < vAxis.ViewMinimum )
						{
							low = vAxis.ViewMinimum;
						}
						low = vAxis.GetLinearPosition(low);

						// Start Svg Selection mode
						graph.StartHotRegion( point, true );

						// Draw label
						DrawLabel(common, area, graph, ser, point, new PointF(xPosition, (float)Math.Min(high, low)), index);

						// End Svg Selection mode
						graph.EndHotRegion( );

						++index;
					}
				}
				
				// Call Paint event
				if( !selection )
				{
                    common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}

				++seriesIndex;
			}
		}

		/// <summary>
		/// Draws error bar markers.
		/// </summary>
		/// <param name="graph">Chart graphics object.</param>
		/// <param name="barStyle">Style of the error bar.</param>
		/// <param name="area">Chart area.</param>
		/// <param name="ser">Data point series.</param>
		/// <param name="point">Data point to draw.</param>
		/// <param name="xPosition">X position.</param>
		/// <param name="width">Point width.</param>
		virtual protected void DrawErrorBarMarks(
			ChartGraphics graph, 
			ErrorBarStyle barStyle,
			ChartArea area,
			Series ser, 
			DataPoint point, 
			float xPosition, 
			float width)
		{
			double	yPosition = 0.0;
			string	markerStyle = String.Empty;

			// Draw lower error marker
			if(barStyle == ErrorBarStyle.Both || barStyle == ErrorBarStyle.LowerError)
			{
				// Get Y position
				yPosition = vAxis.GetLogValue( point.YValues[1] );

				// Get marker style name
				markerStyle = "LINE";
				if(point.MarkerStyle != MarkerStyle.None)
				{
					markerStyle = point.MarkerStyle.ToString();
				}

				// Draw marker
				DrawErrorBarSingleMarker(graph, area, point, markerStyle, xPosition, (float)yPosition, 0f, width, false);
			}

			// Draw upper error marker
			if(barStyle == ErrorBarStyle.Both || barStyle == ErrorBarStyle.UpperError)
			{
				// Get Y position
				yPosition = vAxis.GetLogValue( point.YValues[2] );

				// Get marker style name
				markerStyle = "LINE";
				if(point.MarkerStyle != MarkerStyle.None)
				{
					markerStyle = point.MarkerStyle.ToString();
				}

				// Draw marker
				DrawErrorBarSingleMarker(graph, area, point, markerStyle, xPosition, (float)yPosition, 0f, width, false);
			}

			// Draw center value marker
			if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarCenterMarkerStyle) || point.series.IsCustomPropertySet(CustomPropertyName.ErrorBarCenterMarkerStyle))
			{
				// Get Y position
				yPosition = vAxis.GetLogValue( point.YValues[0] );

				// Get marker style name
				markerStyle = point.series[CustomPropertyName.ErrorBarCenterMarkerStyle];
				if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarCenterMarkerStyle))
				{
					markerStyle = point[CustomPropertyName.ErrorBarCenterMarkerStyle];
				}
                markerStyle = markerStyle.ToUpper(System.Globalization.CultureInfo.InvariantCulture);

				// Draw marker
				DrawErrorBarSingleMarker(graph, area, point, markerStyle, xPosition, (float)yPosition, 0f, width, false);
			}
		}

		/// <summary>
		/// Draws single marker on the error bar.
		/// </summary>
		/// <param name="graph">Chart graphics.</param>
		/// <param name="area">Chart area.</param>
		/// <param name="point">Series point.</param>
		/// <param name="markerStyle">Marker style name.</param>
		/// <param name="xPosition">X position.</param>
		/// <param name="yPosition">Y position.</param>
		/// <param name="zPosition">Z position.</param>
		/// <param name="width">Point width.</param>
		/// <param name="draw3D">Used for 3d drawing.</param>
		private void DrawErrorBarSingleMarker(
			ChartGraphics graph, 
			ChartArea area,
			DataPoint point, 
			string markerStyle,
			float xPosition, 
			float yPosition, 
			float zPosition, 
			float width,
			bool draw3D)
		{
			markerStyle = markerStyle.ToUpper(CultureInfo.InvariantCulture);
			if(markerStyle.Length > 0 && String.Compare(markerStyle, "None", StringComparison.OrdinalIgnoreCase) != 0)
			{
				// Make sure Y value is in range
				if( yPosition > vAxis.ViewMaximum || yPosition < vAxis.ViewMinimum)
				{
					return;
				}
				yPosition = (float)vAxis.GetLinearPosition(yPosition);

				// 3D Transform coordinates
				if(draw3D)
				{
					Point3D[] points = new Point3D[1];
					points[0] = new Point3D(xPosition, yPosition, zPosition);
					area.matrix3D.TransformPoints(points);
					xPosition = points[0].X;
					yPosition = points[0].Y;
				}

				// Draw horizontal line marker
				if(String.Compare(markerStyle, "Line", StringComparison.OrdinalIgnoreCase) == 0)
				{
					graph.DrawLineRel(
						point.Color, 
						point.BorderWidth, 
						point.BorderDashStyle, 
						new PointF(xPosition - width/2f, yPosition), 
						new PointF(xPosition + width/2f, yPosition),
						(point.series != null) ? point.series.ShadowColor : Color.Empty, 
						(point.series != null) ? point.series.ShadowOffset : 0 );
				}

				// Draw standard marker
				else
				{
					MarkerStyle marker = (MarkerStyle)Enum.Parse(typeof(MarkerStyle), markerStyle, true);

					// Get marker size
					SizeF markerSize = GetMarkerSize(
						graph, 
						area.Common, 
						area, 
						point, 
						point.MarkerSize, 
						point.MarkerImage);

					// Get marker color
					Color markerColor = (point.MarkerColor == Color.Empty) ? point.Color : point.MarkerColor;

					// Draw the marker
					graph.DrawMarkerRel(
						new PointF(xPosition, yPosition), 
						marker,
						point.MarkerSize,
						markerColor,
						point.MarkerBorderColor,
						point.MarkerBorderWidth,
						point.MarkerImage,
						point.MarkerImageTransparentColor,
						(point.series != null) ? point.series.ShadowOffset : 0,
						(point.series != null) ? point.series.ShadowColor : Color.Empty,
						new RectangleF(xPosition, yPosition, markerSize.Width, markerSize.Height));
				}
			}
		}

		/// <summary>
		/// Returns marker size.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="point">Data point.</param>
		/// <param name="markerSize">Marker size.</param>
		/// <param name="markerImage">Marker image.</param>
		/// <returns>Marker width and height.</returns>
		virtual protected SizeF GetMarkerSize(
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			DataPoint point, 
			int markerSize, 
			string markerImage)
		{
			SizeF size = new SizeF(markerSize, markerSize);
            if (graph != null && graph.Graphics != null)
            {
                // Marker size is in pixels and we do the mapping for higher DPIs
                size.Width = markerSize * graph.Graphics.DpiX / 96;
                size.Height = markerSize * graph.Graphics.DpiY / 96;
            }

            if(markerImage.Length > 0)
			    common.ImageLoader.GetAdjustedImageSize(markerImage, graph.Graphics, ref size);
			
            return size;
		}


		/// <summary>
		/// Draws error bar chart data point label.
		/// </summary>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="graph">Chart graphics object.</param>
		/// <param name="ser">Data point series.</param>
		/// <param name="point">Data point to draw.</param>
		/// <param name="position">Label position.</param>
		/// <param name="pointIndex">Data point index.</param>
		virtual protected void DrawLabel(
			CommonElements common, 
			ChartArea area,
			ChartGraphics graph, 
			Series ser, 
			DataPoint point, 
			PointF position,
			int pointIndex)
		{
			if(ser.IsValueShownAsLabel || point.IsValueShownAsLabel || point.Label.Length > 0)
			{
				// Label text format
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;
                    if (point.LabelAngle == 0)
                    {
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Far;
                    }

                    // Get label text
                    string text;
                    if (point.Label.Length == 0)
                    {
                        text = ValueConverter.FormatValue(
                            ser.Chart,
                            point,
                            point.Tag,
                            point.YValues[0],
                            point.LabelFormat,
                            ser.YValueType,
                            ChartElementType.DataPoint);
                    }
                    else
                    {
                        text = point.ReplaceKeywords(point.Label);
                    }

                    // Adjust label positio to the marker size
                    SizeF markerSizes = new SizeF(0f, 0f);
                    if (point.MarkerStyle != MarkerStyle.None)
                    {
                        markerSizes = graph.GetRelativeSize(new SizeF(point.MarkerSize, point.MarkerSize));
                        position.Y -= markerSizes.Height / 2f;
                    }

                    // Get text angle
                    int textAngle = point.LabelAngle;

                    // Check if text contains white space only
                    if (text.Trim().Length != 0)
                    {
                        SizeF sizeFont = SizeF.Empty;


                        // Check if Smart Labels are enabled
                        if (ser.SmartLabelStyle.Enabled)
                        {
                            // Get text size
                            sizeFont = graph.GetRelativeSize(
                                graph.MeasureString(text, point.Font, new SizeF(1000f, 1000f), StringFormat.GenericTypographic));

                            // Adjust label position using SmartLabelStyle algorithm
                            position = area.smartLabels.AdjustSmartLabelPosition(
                                common,
                                graph,
                                area,
                                ser.SmartLabelStyle,
                                position,
                                sizeFont,
                                format,
                                position,
                                markerSizes,
                                LabelAlignmentStyles.Top);

                            // Smart labels always use 0 degrees text angle
                            textAngle = 0;
                        }



                        // Draw label
                        if (!position.IsEmpty)
                        {
                            // Get text size
                            if (sizeFont.IsEmpty)
                            {
                                sizeFont = graph.GetRelativeSize(
                                    graph.MeasureString(text, point.Font, new SizeF(1000f, 1000f), StringFormat.GenericTypographic));
                            }

                            // Get label background position
                            RectangleF labelBackPosition = RectangleF.Empty;
                            SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
                            sizeLabel.Height += sizeFont.Height / 8;
                            sizeLabel.Width += sizeLabel.Width / text.Length;
                            labelBackPosition = PointChart.GetLabelPosition(
                                graph,
                                position,
                                sizeLabel,
                                format,
                                true);

                            // Draw label text
                            using (Brush brush = new SolidBrush(point.LabelForeColor))
                            {
                                graph.DrawPointLabelStringRel(
                                    common,
                                    text,
                                    point.Font,
                                    brush,
                                    position,
                                    format,
                                    textAngle,
                                    labelBackPosition,
                                    point.LabelBackColor,
                                    point.LabelBorderColor,
                                    point.LabelBorderWidth,
                                    point.LabelBorderDashStyle,
                                    ser,
                                    point,
                                    pointIndex - 1);
                            }
                        }
                    }
                }
			}
		}

		#endregion

		#region 3D Drawing and Selection methods

		/// <summary>
		/// This method recalculates size of the bars. This method is used 
		/// from Paint or Select method.
		/// </summary>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		virtual protected void ProcessChartType3D( 
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			Series seriesToDraw )
		{
			// All data series from chart area which have Error Bar chart type
			List<string>	typeSeries = area.GetSeriesFromChartType(this.Name);

			// Zero X values mode.
            bool indexedSeries = ChartHelper.IndexedSeries(common, typeSeries.ToArray());

			//************************************************************
			//** Loop through all series
			//************************************************************
			foreach( Series ser in common.DataManager.Series )
			{
				// Process non empty series of the area with stock chart type
				if( String.Compare( ser.ChartTypeName, this.Name, StringComparison.OrdinalIgnoreCase ) != 0 
					|| ser.ChartArea != area.Name || !ser.IsVisible())
				{
					continue;
				}

				// Check that we have at least 4 Y values
				if(ser.YValuesPerPoint < 3)
				{
					throw(new ArgumentException(SR.ExceptionChartTypeRequiresYValues( ChartTypeNames.ErrorBar, ((int)(3)).ToString(CultureInfo.CurrentCulture))));
				}

				// Set active horizontal/vertical axis
				hAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
				vAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

				// Get interval between points
				double interval = (indexedSeries) ? 1 : area.GetPointsInterval( hAxis.IsLogarithmic, hAxis.logarithmBase );

				// Calculates the width of the candles.
				float	width = (float)(ser.GetPointWidth(graph, hAxis, interval, 0.4));

				// Align error bar X position with linked series				
				float	sideBySideWidth = width;
				int		numberOfLinkedSeries = 1;
				int		indexOfLinkedSeries = 0;
				bool	showSideBySide = false;
				if(ser.IsCustomPropertySet(CustomPropertyName.ErrorBarSeries))
				{
					// Get series name
					string attribValue = ser[CustomPropertyName.ErrorBarSeries];
                    int valueTypeIndex = attribValue.IndexOf(":", StringComparison.Ordinal);
					if(valueTypeIndex >= 0)
					{
						attribValue = attribValue.Substring(0, valueTypeIndex);
					}

					// All linked data series from chart area which have Error bar chart type
					string linkedSeriesChartType = common.DataManager.Series[attribValue].ChartTypeName;
					List<string>	typeLinkedSeries = area.GetSeriesFromChartType(linkedSeriesChartType);

					// Get index of linked serries
					foreach(string name in typeLinkedSeries)
					{
						if(name == attribValue)
						{
							break;
						}
						++indexOfLinkedSeries;
					}

					bool	currentDrawSeriesSideBySide = false;
					if(String.Compare(linkedSeriesChartType, ChartTypeNames.Column, StringComparison.OrdinalIgnoreCase ) == 0
                        || String.Compare(linkedSeriesChartType, ChartTypeNames.RangeColumn, StringComparison.OrdinalIgnoreCase) == 0
                        )
					{
						currentDrawSeriesSideBySide = true;
					}
					foreach(string seriesName in typeLinkedSeries)
					{
						if(common.DataManager.Series[seriesName].IsCustomPropertySet(CustomPropertyName.DrawSideBySide))
						{
							attribValue = common.DataManager.Series[seriesName][CustomPropertyName.DrawSideBySide];
							if(String.Compare(attribValue, "False", StringComparison.OrdinalIgnoreCase ) == 0)
							{
								currentDrawSeriesSideBySide = false;
							}
							else if(String.Compare(attribValue, "True", StringComparison.OrdinalIgnoreCase ) == 0)
							{
								currentDrawSeriesSideBySide = true;
							}
							else if(String.Compare(attribValue, "Auto", StringComparison.OrdinalIgnoreCase ) == 0)
							{
								// Do nothing
							}
							else
							{
                                throw (new InvalidOperationException(SR.ExceptionAttributeDrawSideBySideInvalid));
							}
						}
					}

					if(currentDrawSeriesSideBySide)
					{
						// Find the number of linked data series
						numberOfLinkedSeries = typeLinkedSeries.Count;
						width /= numberOfLinkedSeries;

						// Check if side by side
						if(!indexedSeries)
						{
							area.GetPointsInterval( typeLinkedSeries, hAxis.IsLogarithmic, hAxis.logarithmBase, true, out showSideBySide );
						}
					}
				}

				// Call Back Paint event
				if( !selection )
				{
                    common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}

				//************************************************************
				//** Get series depth and Z position
				//************************************************************
				float seriesDepth, seriesZPosition;
				area.GetSeriesZPositionAndDepth(ser, out seriesDepth, out seriesZPosition);

				//************************************************************
				//** Series data points loop
				//************************************************************
				int	index = 1;
				foreach( DataPoint point in ser.Points )
				{
					// Check required Y values number
					if(point.YValues.Length < this.YValuesPerPoint)
					{
						throw(new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues( this.Name, this.YValuesPerPoint.ToString(CultureInfo.InvariantCulture))));
					}

					// Reset pre-calculated point position
					point.positionRel = new PointF(float.NaN, float.NaN);

					// Get point X position
					float xPosition = 0f;
					double	xValue = point.XValue;
					if( indexedSeries )
					{
						xValue = (double)index;
						xPosition = (float)(hAxis.GetPosition( (double)index ) - sideBySideWidth * ((double) numberOfLinkedSeries) / 2.0 + sideBySideWidth/2 + indexOfLinkedSeries * sideBySideWidth);
					}
					else if( showSideBySide )
					{
						xPosition = (float)(hAxis.GetPosition( xValue ) - sideBySideWidth * ((double) numberOfLinkedSeries) / 2.0 + sideBySideWidth/2 + indexOfLinkedSeries * sideBySideWidth);
					}
					else
					{
						xPosition = (float)hAxis.GetPosition( xValue );
					}

					double yValue0 = vAxis.GetLogValue( point.YValues[1] );
					double yValue1 = vAxis.GetLogValue( point.YValues[2] );
					xValue = hAxis.GetLogValue(xValue);
					
					// Check if chart is completly out of the data scaleView
					if(xValue < hAxis.ViewMinimum || 
						xValue > hAxis.ViewMaximum ||
						(yValue0 < vAxis.ViewMinimum && yValue1 < vAxis.ViewMinimum) ||
						(yValue0 > vAxis.ViewMaximum && yValue1 > vAxis.ViewMaximum) )
					{
						++index;
						continue;
					}

					// Make sure High/Low values are in data scaleView range						
					double	high = vAxis.GetLogValue( point.YValues[2] );
					double	low = vAxis.GetLogValue( point.YValues[1] );

					// Check if lower and/or upper error bar are drawn
					ErrorBarStyle	barStyle = ErrorBarStyle.Both;
					if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle) || ser.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle))
					{
						string errorBarStyle = ser[CustomPropertyName.ErrorBarStyle];
						if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle))
						{
							errorBarStyle = point[CustomPropertyName.ErrorBarStyle];
						}
						if(String.Compare(errorBarStyle, "Both", StringComparison.OrdinalIgnoreCase) == 0)
						{
							// default - do nothing
						}
						else if(String.Compare(errorBarStyle, "UpperError", StringComparison.OrdinalIgnoreCase) == 0)
						{
							barStyle = ErrorBarStyle.UpperError;
							low = vAxis.GetLogValue( point.YValues[0] );
							high = vAxis.GetLogValue( point.YValues[2] );
						}
						else if(String.Compare(errorBarStyle, "LowerError", StringComparison.OrdinalIgnoreCase) == 0)
						{
							barStyle = ErrorBarStyle.LowerError;
							low = vAxis.GetLogValue( point.YValues[1] );
							high = vAxis.GetLogValue( point.YValues[0] );
						}
						else
						{
							throw(new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(point[CustomPropertyName.ErrorBarStyle], "ErrorBarStyle")));
						}
					}
				
					if( high > vAxis.ViewMaximum )
					{
						high = vAxis.ViewMaximum;
					}
					if( high < vAxis.ViewMinimum )
					{
						high = vAxis.ViewMinimum;
					}
					high = (float)vAxis.GetLinearPosition(high);
					
					if( low > vAxis.ViewMaximum )
					{
						low = vAxis.ViewMaximum;
					}
					if( low < vAxis.ViewMinimum )
					{
						low = vAxis.ViewMinimum;
					}
					low = vAxis.GetLinearPosition(low);

					// Remeber pre-calculated point position
					point.positionRel = new PointF((float)xPosition, (float)Math.Min(high, low));

					// 3D Transform coordinates
					Point3D[] points = new Point3D[2];
					points[0] = new Point3D(xPosition, (float)high, seriesZPosition+seriesDepth/2f);
					points[1] = new Point3D(xPosition, (float)low, seriesZPosition+seriesDepth/2f);
					area.matrix3D.TransformPoints(points);

					if( common.ProcessModePaint )
					{

						// Check if chart is partialy in the data scaleView
						bool	clipRegionSet = false;
						if(xValue == hAxis.ViewMinimum || xValue == hAxis.ViewMaximum )
						{
							// Set clipping region for line drawing 
							graph.SetClip( area.PlotAreaPosition.ToRectangleF() );
							clipRegionSet = true;
						}

                        // Start Svg Selection mode
						graph.StartHotRegion( point );

						// Draw error bar line
						graph.DrawLineRel( 
							point.Color, 
							point.BorderWidth, 
							point.BorderDashStyle, 
							points[0].PointF, 
							points[1].PointF,
							ser.ShadowColor, 
							ser.ShadowOffset );

						// Draw Error Bar marks
						DrawErrorBarMarks3D(graph, barStyle, area, ser, point, xPosition, width, seriesZPosition, seriesDepth);
						xPosition = points[0].X;
						high = points[0].Y;
						low = points[1].Y;

						// End Svg Selection mode
						graph.EndHotRegion( );

						// Reset Clip Region
						if(clipRegionSet)
						{
							graph.ResetClip();
						}
					}

					if( common.ProcessModeRegions )
					{
						xPosition = points[0].X;
						high = points[0].Y;
						low = points[1].Y;

						// Calculate rect around the error bar marks
						RectangleF	areaRect = RectangleF.Empty;
						areaRect.X = xPosition - width / 2f;
						areaRect.Y = (float)Math.Min(high, low);
						areaRect.Width = width;
						areaRect.Height = (float)Math.Max(high, low) - areaRect.Y;

						// Add area
						common.HotRegionsList.AddHotRegion( 
							areaRect, 
							point, 
							ser.Name, 
							index - 1 );
					}
				
					++index;
				}

				//************************************************************
				//** Second series data points loop, when labels are drawn.
				//************************************************************
				if( !selection )
				{
					index = 1;
					foreach( DataPoint point in ser.Points )
					{
						// Get point X position
						float xPosition = 0f;
						double	xValue = point.XValue;
						if( indexedSeries )
						{
							xValue = (double)index;
							xPosition = (float)(hAxis.GetPosition( (double)index ) - sideBySideWidth * ((double) numberOfLinkedSeries) / 2.0 + sideBySideWidth/2 + indexOfLinkedSeries * sideBySideWidth);
						}
						else if( showSideBySide )
						{
							xPosition = (float)(hAxis.GetPosition( xValue ) - sideBySideWidth * ((double) numberOfLinkedSeries) / 2.0 + sideBySideWidth/2 + indexOfLinkedSeries * sideBySideWidth);
						}
						else
						{
							xPosition = (float)hAxis.GetPosition( xValue );
						}


						double yValue0 = vAxis.GetLogValue( point.YValues[1] );
						double yValue1 = vAxis.GetLogValue( point.YValues[2] );
						xValue = hAxis.GetLogValue(xValue);
					
						// Check if chart is completly out of the data scaleView
						if(xValue < hAxis.ViewMinimum || 
							xValue > hAxis.ViewMaximum ||
							(yValue0 < vAxis.ViewMinimum && yValue1 < vAxis.ViewMinimum) ||
							(yValue0 > vAxis.ViewMaximum && yValue1 > vAxis.ViewMaximum) )
						{
							++index;
							continue;
						}

						// Make sure High/Low values are in data scaleView range						
						double	high = vAxis.GetLogValue( point.YValues[2] );
						double	low = vAxis.GetLogValue( point.YValues[1] );

						// Check if lower and/or upper error bar are drawn
						if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle) || ser.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle))
						{
							string errorBarStyle = ser[CustomPropertyName.ErrorBarStyle];
							if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarStyle))
							{
								errorBarStyle = point[CustomPropertyName.ErrorBarStyle];
							}
							if(String.Compare(errorBarStyle, "Both", StringComparison.OrdinalIgnoreCase ) == 0)
							{
								// default - do nothing
							}
							else if(String.Compare(errorBarStyle, "UpperError", StringComparison.OrdinalIgnoreCase ) == 0)
							{
								low = vAxis.GetLogValue( point.YValues[0] );
								high = vAxis.GetLogValue( point.YValues[2] );
							}
							else if(String.Compare(errorBarStyle, "LowerError", StringComparison.OrdinalIgnoreCase ) == 0)
							{
								low = vAxis.GetLogValue( point.YValues[1] );
								high = vAxis.GetLogValue( point.YValues[0] );
							}
							else
							{
								throw(new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid( point[CustomPropertyName.ErrorBarStyle], "ErrorBarStyle")));
							}
						}
					
						if( high > vAxis.ViewMaximum )
						{
							high = vAxis.ViewMaximum;
						}
						if( high < vAxis.ViewMinimum )
						{
							high = vAxis.ViewMinimum;
						}
						high = (float)vAxis.GetLinearPosition(high);
					
						if( low > vAxis.ViewMaximum )
						{
							low = vAxis.ViewMaximum;
						}
						if( low < vAxis.ViewMinimum )
						{
							low = vAxis.ViewMinimum;
						}
						low = vAxis.GetLinearPosition(low);


						// 3D Transform coordinates
						Point3D[] points = new Point3D[2];
						points[0] = new Point3D(xPosition, (float)high, seriesZPosition+seriesDepth/2f);
						points[1] = new Point3D(xPosition, (float)low, seriesZPosition+seriesDepth/2f);
						area.matrix3D.TransformPoints(points);
						xPosition = points[0].X;
						high = points[0].Y;
						low = points[1].Y;

						// Draw label
						DrawLabel(common, area, graph, ser, point, new PointF(xPosition, (float)Math.Min(high, low)), index);
				
						++index;
					}
				}
				
				// Call Paint event
				if( !selection )
				{
                    common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}
			}
		}

		/// <summary>
		/// Draws stock chart open-close marks depending on selected style.
		/// </summary>
		/// <param name="graph">Chart graphics object.</param>
		/// <param name="barStyle">Style of the error bar.</param>
		/// <param name="area">Chart area.</param>
		/// <param name="ser">Data point series.</param>
		/// <param name="point">Data point to draw.</param>
		/// <param name="xPosition">X position.</param>
		/// <param name="width">Point width.</param>
		/// <param name="zPosition">Series Z position.</param>
		/// <param name="depth">Series depth.</param>
		virtual protected void DrawErrorBarMarks3D(
			ChartGraphics graph, 
			ErrorBarStyle barStyle,
			ChartArea area,
			Series ser, 
			DataPoint point, 
			float xPosition, 
			float width,
			float zPosition,
			float depth)
		{
			float	yPosition = 0f;
			string	markerStyle = String.Empty;

			// Draw lower error marker
			if(barStyle == ErrorBarStyle.Both || barStyle == ErrorBarStyle.LowerError)
			{
				// Get Y position
				yPosition = (float)vAxis.GetLogValue( point.YValues[1] );

				// Get marker style name
				markerStyle = "LINE";
				if(point.MarkerStyle != MarkerStyle.None)
				{
					markerStyle = point.MarkerStyle.ToString();
				}

				// Draw marker
				DrawErrorBarSingleMarker(graph, area, point, markerStyle, xPosition, yPosition, zPosition+depth/2f, width, true);
			}

			// Draw upper error marker
			if(barStyle == ErrorBarStyle.Both || barStyle == ErrorBarStyle.UpperError)
			{
				// Get Y position
				yPosition = (float)vAxis.GetLogValue( point.YValues[2] );

				// Get marker style name
				markerStyle = "LINE";
				if(point.MarkerStyle != MarkerStyle.None)
				{
					markerStyle = point.MarkerStyle.ToString();
				}

				// Draw marker
				DrawErrorBarSingleMarker(graph, area, point, markerStyle, xPosition, yPosition, zPosition+depth/2f, width, true);
			}

			// Draw center value marker
			if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarCenterMarkerStyle) || point.series.IsCustomPropertySet(CustomPropertyName.ErrorBarCenterMarkerStyle))
			{
				// Get Y position
				yPosition = (float)vAxis.GetLogValue( point.YValues[0] );

				// Get marker style name
				markerStyle = point.series[CustomPropertyName.ErrorBarCenterMarkerStyle];
				if(point.IsCustomPropertySet(CustomPropertyName.ErrorBarCenterMarkerStyle))
				{
					markerStyle = point[CustomPropertyName.ErrorBarCenterMarkerStyle];
				}
				markerStyle = markerStyle.ToUpper(CultureInfo.InvariantCulture);

				// Draw marker
				DrawErrorBarSingleMarker(graph, area, point, markerStyle, xPosition, yPosition, zPosition+depth/2f, width, true);
			}
		}

		#endregion

		#region Y values related methods

		/// <summary>
		/// Helper function that returns the Y value of the point.
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

		#region Automatic Values Calculation methods

		/// <summary>
		/// Calculates lower and upper error amount using specified formula.
		/// </summary>
		/// <param name="errorBarSeries">Error bar chart type series.</param>
		internal static void CalculateErrorAmount(Series errorBarSeries)
		{
			// Check input parameters
			if(String.Compare(errorBarSeries.ChartTypeName, ChartTypeNames.ErrorBar, StringComparison.OrdinalIgnoreCase) != 0 )
			{
				return;
			}

			// Check if "ErrorBarType" custom attribute is set
			if(!errorBarSeries.IsCustomPropertySet(CustomPropertyName.ErrorBarType) && 
				!errorBarSeries.IsCustomPropertySet(CustomPropertyName.ErrorBarSeries))
			{
				return;
			}

			// Parase the value of the ErrorBarType attribute.
			double param = double.NaN;
			ErrorBarType errorBarType = ErrorBarType.StandardError;
			if(errorBarSeries.IsCustomPropertySet(CustomPropertyName.ErrorBarType))
			{
				string	typeName = errorBarSeries[CustomPropertyName.ErrorBarType];
				if(typeName.StartsWith("FixedValue", StringComparison.OrdinalIgnoreCase))
				{
					errorBarType = ErrorBarType.FixedValue;
				}
				else if(typeName.StartsWith("Percentage", StringComparison.OrdinalIgnoreCase))
				{
					errorBarType = ErrorBarType.Percentage;
				}
				else if(typeName.StartsWith("StandardDeviation", StringComparison.OrdinalIgnoreCase))
				{
					errorBarType = ErrorBarType.StandardDeviation;
				}
				else if(typeName.StartsWith("StandardError", StringComparison.OrdinalIgnoreCase))
				{
					errorBarType = ErrorBarType.StandardError;
				}
				else if(typeName.StartsWith("None", StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
				else
				{
					throw(new InvalidOperationException(SR.ExceptionErrorBarTypeInvalid(errorBarSeries[CustomPropertyName.ErrorBarType])));
				}
		
				// Check if parameter is specified
				typeName = typeName.Substring(errorBarType.ToString().Length);
				if(typeName.Length > 0)
				{
					// Must be followed by '(' and ends with ')'
                    if (!typeName.StartsWith("(", StringComparison.Ordinal) || !typeName.EndsWith(")", StringComparison.Ordinal))
					{
						throw(new InvalidOperationException(SR.ExceptionErrorBarTypeFormatInvalid(errorBarSeries[CustomPropertyName.ErrorBarType])));
					}
					typeName = typeName.Substring(1, typeName.Length - 2);


					if(typeName.Length > 0)
					{
                        if (!double.TryParse(typeName, NumberStyles.Any, CultureInfo.InvariantCulture, out param))
                        {
                            throw (new InvalidOperationException(SR.ExceptionErrorBarTypeFormatInvalid(errorBarSeries[CustomPropertyName.ErrorBarType])));
                        }
					}
				}
			}

			// Points number
			int	pointNumber = errorBarSeries.Points.Count;

			// Find number of empty data points
			int numberOfEmptyPoints = 0;
			foreach(DataPoint point in errorBarSeries.Points)
			{
				if( point.IsEmpty )
				{
					numberOfEmptyPoints++;
				}
			}

			// Number of poist without empty points
			pointNumber -= numberOfEmptyPoints;

            if (double.IsNaN(param))
            {
                param = DefaultErrorBarTypeValue(errorBarType);
            }

			// Calculate error amount
			double	errorAmount = 0.0;
			if(errorBarType == ErrorBarType.FixedValue)
			{
			    errorAmount = param;
			}
			else if(errorBarType == ErrorBarType.Percentage)
			{
                // no processing or errorAmount
			}
			else if( errorBarType == ErrorBarType.StandardDeviation )
			{
				// Formula for standard deviation need 
				// more then one data point
				if( pointNumber > 1 )
				{
				
					// Calculate series mean value
					double mean = 0.0;
					foreach(DataPoint point in errorBarSeries.Points)
					{
						mean += point.YValues[0];
					}
					mean /= pointNumber;

					// Calculate series variance
					errorAmount = 0.0;
					foreach(DataPoint point in errorBarSeries.Points)
					{
						if( !point.IsEmpty )
						{
							errorAmount += Math.Pow(point.YValues[0] - mean, 2);
						}
					}
				
					errorAmount = param * Math.Sqrt(errorAmount/ ( pointNumber - 1 ) );
				}
				else
				{
					errorAmount = 0;
				}
			}
			else if( errorBarType == ErrorBarType.StandardError )
			{
				// Formula for standard deviation need 
				// more then one data point
				if( pointNumber > 1 )
				{
					// Calculate standard error
					errorAmount = 0.0;
					foreach(DataPoint point in errorBarSeries.Points)
					{
						if( !point.IsEmpty )
						{
							errorAmount += Math.Pow(point.YValues[0], 2);
						}
					}
				
					errorAmount = param * Math.Sqrt( errorAmount/( pointNumber * ( pointNumber - 1 ) ) ) / 2.0;
				}
				else
				{
					errorAmount = 0;
				}
			}


			// Loop through all points to calculate error amount
			foreach(DataPoint point in errorBarSeries.Points)
			{
				if(errorBarType == ErrorBarType.Percentage)
				{
					point.YValues[1] = point.YValues[0] - point.YValues[0] * param / 100.0;
					point.YValues[2] = point.YValues[0] + point.YValues[0] * param / 100.0;
				}
				else
				{
					point.YValues[1] = point.YValues[0] - errorAmount;
					point.YValues[2] = point.YValues[0] + errorAmount;
				}
			}

		}

        internal static double DefaultErrorBarTypeValue(ErrorBarType errorBarType)
        {
            switch (errorBarType)
            {
                case ErrorBarType.FixedValue:
                case ErrorBarType.Percentage:
                    return 10.0;
                case ErrorBarType.StandardDeviation:
                case ErrorBarType.StandardError:
                    return 1.0;
                default:
                    System.Diagnostics.Debug.Fail("Unknown ErrorBarType=" + errorBarType.ToString());
                    break;
            }
            return 10.0;
        }

		/// <summary>
		/// Populates error bar center value using the linked series specified by 
		/// "ErrorBarSeries" custom attribute.
		/// </summary>
		/// <param name="errorBarSeries">Error bar chart type series.</param>
		internal static void GetDataFromLinkedSeries(Series errorBarSeries)
		{
			// Check input parameters
			if(String.Compare(errorBarSeries.ChartTypeName, ChartTypeNames.ErrorBar, StringComparison.OrdinalIgnoreCase) != 0 || errorBarSeries.Chart == null)
			{
				return;
			}

			// Check if "ErrorBarSeries" custom attribute is set
			if(!errorBarSeries.IsCustomPropertySet(CustomPropertyName.ErrorBarSeries))
			{
				return;
			}

			// Get series and value name
			string	linkedSeriesName = errorBarSeries[CustomPropertyName.ErrorBarSeries];
			String	valueName = "Y";
			int valueTypeIndex = linkedSeriesName.IndexOf(":", StringComparison.Ordinal);
			if(valueTypeIndex >= 0)
			{
				valueName = linkedSeriesName.Substring(valueTypeIndex + 1);
				linkedSeriesName = linkedSeriesName.Substring(0, valueTypeIndex);
			}

			// Get reference to the chart control
			Chart control = errorBarSeries.Chart;
			if(control != null)
			{
				// Get linked series and check existance
				if(control.Series.IndexOf(linkedSeriesName) == -1)
				{
                    throw (new InvalidOperationException(SR.ExceptionDataSeriesNameNotFound(linkedSeriesName)));
				}
				Series linkedSeries = control.Series[linkedSeriesName];

				// Make sure we use the same X and Y axis as the linked series
				errorBarSeries.XAxisType = linkedSeries.XAxisType;
				errorBarSeries.YAxisType = linkedSeries.YAxisType;

				// Get cennter values from the linked series
				errorBarSeries.Points.Clear();
				foreach(DataPoint point in linkedSeries.Points)
				{
					// Add new point into the collection
					errorBarSeries.Points.AddXY(point.XValue, point.GetValueByName(valueName));
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
			// No data point markers supported for SmartLabelStyle
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

