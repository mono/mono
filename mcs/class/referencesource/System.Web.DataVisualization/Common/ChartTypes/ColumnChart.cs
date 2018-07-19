//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ColumnChart.cs
//
//  Namespace:	System.Web.UI.DataVisualization.Charting.ChartTypes
//
//	Classes:	ColumnChart, RangeColumnChart
//
//  Purpose:	Provides 2D/3D drawing and hit testing functionality 
//              for the Column and RangeColumn charts.
//
//	Reviewed:	AG - Aug 8, 2002
//              AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    /// <summary>
    /// ColumnChart class provides 2D/3D drawing and hit testing 
    /// functionality for the Column and RangeColumn charts. The 
    /// only difference between the RangeColumn and Column chart 
    /// is that 2 Y values are used to position top and bottom 
    /// side of each RangeColumn column.
    /// </summary>
    internal class ColumnChart : PointChart
	{
		#region Fields

		/// <summary>
		/// Labels and markers have to be shifted if there 
		/// is more than one series for column chart.
		/// </summary>
		private double _shiftedX = 0;

		/// <summary>
		/// Labels and markers have to be shifted if there 
		/// is more than one series for column chart. This property 
		/// will give a name of the series, which is used, for 
		/// labels and markers. Point chart 
		/// </summary>
		private string _shiftedSerName = "";

		/// <summary>
		/// Indicates that two Y values are used to calculate column position
		/// </summary>
		protected	bool	useTwoValues = false;

		/// <summary>
		/// Indicates that columns from different series are drawn side by side
		/// </summary>
		protected	bool	drawSeriesSideBySide = true;

		/// <summary>
		/// Coordinates of COP used when sorting 3D points order
		/// </summary>
		protected	COPCoordinates	coordinates = COPCoordinates.X;

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		override public string Name			{ get{ return ChartTypeNames.Column;}}

		/// <summary>
		/// Gets chart type image.
		/// </summary>
		/// <param name="registry">Chart types registry object.</param>
		/// <returns>Chart type image.</returns>
		override public System.Drawing.Image GetImage(ChartTypeRegistry registry)
		{
			return (System.Drawing.Image)registry.ResourceManager.GetObject(this.Name + "ChartType");
		}

		/// <summary>
		/// True if chart type is stacked
		/// </summary>
		override public bool Stacked		{ get{ return false;}}

		/// <summary>
		/// True if chart type supports axeses
		/// </summary>
		override public bool RequireAxes	{ get{ return true;} }

		/// <summary>
		/// True if chart type supports logarithmic axes
		/// </summary>
		override public bool SupportLogarithmicAxes	{ get{ return true;} }

		/// <summary>
		/// True if chart type requires to switch the value (Y) axes position
		/// </summary>
		override public bool SwitchValueAxes	{ get{ return false;} }

		/// <summary>
		/// True if chart series can be placed side-by-side.
		/// </summary>
		override public bool SideBySideSeries { get{ return true;} }

		/// <summary>
		/// True if each data point of a chart must be represented in the legend
		/// </summary>
		override public bool DataPointsInLegend	{ get{ return false;} }

		/// <summary>
		/// Indicates that extra Y values are connected to the scale of the Y axis
		/// </summary>
		override public bool ExtraYValuesConnectedToYAxis{ get { return false; } }

		/// <summary>
		/// True if palette colors should be applied for each data paoint.
		/// Otherwise the color is applied to the series.
		/// </summary>
		override public bool ApplyPaletteColorsToPoints	{ get { return false; } }

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
		/// Number of supported Y value(s) per point 
		/// </summary>
		override public int YValuesPerPoint{ get { return 1; } }

		/// <summary>
		/// If the crossing value is auto Crossing value should be 
		/// automatically set to zero for some chart 
		/// types (Bar, column, area etc.)
		/// </summary>
		override public bool ZeroCrossing { get{ return true;} }

		#endregion

		#region Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public ColumnChart() : base(false)
		{
		}

		#endregion

		#region Properties

		/// <summary>
		/// Labels and markers have to be shifted if there 
		/// is more than one series for column chart.
		/// </summary>
		override public double ShiftedX
		{
			get
			{
				return _shiftedX;
			}
			set
			{
				_shiftedX = value;
			}
		}

		/// <summary>
		/// Labels and markers have to be shifted if there 
		/// is more than one series for column chart. This property 
		/// will give a name of the series, which is used, for 
		/// labels and markers.
		/// </summary>
		override public string ShiftedSerName
		{
			get
			{
				return _shiftedSerName;
			}
			set
			{
				_shiftedSerName = value;
			}
		}

		#endregion

		#region Painting and selection methods

		/// <summary>
		/// Paint Column Chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		override public void Paint( 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			Series seriesToDraw 
			)
		{
            this.Common = common;
			// Draw columns
			ProcessChartType( false, false, graph, common, area, seriesToDraw );

			// Draw labels and markers
			ProcessChartType( true, false, graph, common, area, seriesToDraw );
		}
				
		/// <summary>
		/// This method recalculates size of the columns and paint them or do the hit test.
		/// This method is used from Paint or Select method.
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
			// Prosess 3D chart type
			if(area.Area3DStyle.Enable3D)
			{
				ProcessChartType3D( labels, selection, graph, common, area, seriesToDraw );
				return;
			}
			
			// Get pixel size
			SizeF	pixelRelSize = graph.GetRelativeSize(new SizeF(1.1f, 1.1f));
		
			// All data series from chart area which have Column chart type
			List<string> typeSeries = area.GetSeriesFromChartType(Name);

			// Check if series should be drawn side by side
			bool	currentDrawSeriesSideBySide = this.drawSeriesSideBySide;
			foreach(string seriesName in typeSeries)
			{
				if(common.DataManager.Series[seriesName].IsCustomPropertySet(CustomPropertyName.DrawSideBySide))
				{
					string attribValue = common.DataManager.Series[seriesName][CustomPropertyName.DrawSideBySide];
					if(String.Compare( attribValue, "False", StringComparison.OrdinalIgnoreCase) == 0 )
					{
						currentDrawSeriesSideBySide = false;
					}
					else if(String.Compare( attribValue, "True", StringComparison.OrdinalIgnoreCase) == 0)
					{
						currentDrawSeriesSideBySide = true;
					}
					else if(String.Compare( attribValue, "Auto", StringComparison.OrdinalIgnoreCase) == 0)
					{
						// Do nothing
					}
					else
					{
                        throw (new InvalidOperationException(SR.ExceptionAttributeDrawSideBySideInvalid));
					}
				}
			}

			// Find the number of "Column chart" data series
			double	numOfSeries = typeSeries.Count;
			if(!currentDrawSeriesSideBySide)
			{
				numOfSeries = 1;
			}

			// Check if column chart series are indexed
            bool indexedSeries = ChartHelper.IndexedSeries(this.Common, area.GetSeriesFromChartType(Name).ToArray());

			//************************************************************
			//** Loop through all series
			//************************************************************
			int	seriesIndx = 0;
			foreach( Series ser in common.DataManager.Series )
			{
				// Process non empty series of the area with Column chart type
				if( String.Compare( ser.ChartTypeName, Name, true, System.Globalization.CultureInfo.CurrentCulture) != 0 
					|| ser.ChartArea != area.Name || ser.Points.Count == 0 || !ser.IsVisible())
				{
					continue;
				}

				// Set shifted series name property
				ShiftedSerName = ser.Name;

				// Set active vertical/horizontal axis
				Axis	vAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);
				Axis	hAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
				double horizontalViewMax = hAxis.ViewMaximum;
				double horizontalViewMin = hAxis.ViewMinimum;
				double verticalViewMax = vAxis.ViewMaximum;
				double verticalViewMin = vAxis.ViewMinimum;
				double verticalAxisCrossing = vAxis.GetPosition(vAxis.Crossing);

				// Get points interval:
				//  - set interval to 1 for indexed series
				//  - if points are not equaly spaced, the minimum interval between points is selected.
				//  - if points have same interval bars do not overlap each other.
				bool	sameInterval = false;
				double	interval = 1;
				if(!indexedSeries)
				{
                    if (ser.Points.Count == 1 &&
                        (ser.XValueType == ChartValueType.Date || 
                         ser.XValueType == ChartValueType.DateTime || 
                         ser.XValueType == ChartValueType.Time ||
                         ser.XValueType == ChartValueType.DateTimeOffset))
                    {
                        // Check if interval is the same
                        area.GetPointsInterval(typeSeries, hAxis.IsLogarithmic, hAxis.logarithmBase, true, out sameInterval);

                        // Special case when there is only one data point and date scale is used.
                        if (!double.IsNaN(hAxis.majorGrid.GetInterval()) && hAxis.majorGrid.GetIntervalType() != DateTimeIntervalType.NotSet)
                        {
                            interval = ChartHelper.GetIntervalSize(hAxis.minimum, hAxis.majorGrid.GetInterval(), hAxis.majorGrid.GetIntervalType());
                        }
                        else
                        {
                            interval = ChartHelper.GetIntervalSize(hAxis.minimum, hAxis.Interval, hAxis.IntervalType);
                        }
                    }
                    else
                    {
                        interval = area.GetPointsInterval( typeSeries, hAxis.IsLogarithmic, hAxis.logarithmBase, true, out sameInterval );
                    }
				}

				// Get column width
				double	width = ser.GetPointWidth(graph, hAxis, interval, 0.8) / numOfSeries;
				
				// Call Back Paint event
				if( !selection )
				{
                    common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}

				//************************************************************
				//** Loop through all points in series
				//************************************************************
				int	index = 0;
				foreach( DataPoint point in ser.Points )
				{
					// Change Y value if Column is out of plot area
					double	yValue = vAxis.GetLogValue( GetYValue(common, area, ser, point, index, (useTwoValues) ? 1 : 0) );
					
					if( yValue > verticalViewMax )
					{
						yValue = verticalViewMax;
					}
					if( yValue < verticalViewMin )
					{
						yValue = verticalViewMin;
					}

					// Recalculates Height position and zero position of Columns
					double	height = vAxis.GetLinearPosition( yValue );

					// Set start position for a column
					double	columnStartPosition = 0;
					if(useTwoValues)
					{
						// Point Y value (first) is used to determine the column starting position
						double yValueStart = vAxis.GetLogValue( GetYValue(common, area, ser, point, index, 0 ) );
						if( yValueStart > verticalViewMax )
						{
							yValueStart = verticalViewMax;
						}
						else if( yValueStart < verticalViewMin )
						{
							yValueStart = verticalViewMin;
						}

						columnStartPosition = vAxis.GetLinearPosition(yValueStart);
					}
					else
					{
						// Column starts on the horizontal axis crossing
						columnStartPosition = verticalAxisCrossing;
					}

					// Increase point index
					index++;
					
					// Set x position
					double	xCenterVal;
					double	xPosition;
					if( indexedSeries )
					{
						// The formula for position is based on a distance 
						//from the grid line or nPoints position.
						xPosition = hAxis.GetPosition( (double)index ) - width * ((double) numOfSeries) / 2.0 + width/2 + seriesIndx * width;
						xCenterVal = hAxis.GetPosition( (double)index );
					}
					else if( sameInterval )
					{
						xPosition = hAxis.GetPosition( point.XValue ) - width * ((double) numOfSeries) / 2.0 + width/2 + seriesIndx * width;
						xCenterVal = hAxis.GetPosition( point.XValue );
					}
					else
					{
						xPosition = hAxis.GetPosition( point.XValue );
						xCenterVal = hAxis.GetPosition( point.XValue );
					}

					// Labels and markers have to be shifted if there 
					// is more than one series for column chart.
					ShiftedX = xPosition - xCenterVal;
					

					// Make sure that points with small values are still visible
					if( height < columnStartPosition && 
						(columnStartPosition - height) <  pixelRelSize.Height)
					{
						height = columnStartPosition - pixelRelSize.Height;
					}
					if( height > columnStartPosition && 
						(height - columnStartPosition) <  pixelRelSize.Height)
					{
						height = columnStartPosition + pixelRelSize.Height;
					}
							
					// Get column rectangle
					RectangleF	rectSize = RectangleF.Empty;
					try
					{
						// Set the Column rectangle
						rectSize.X = (float)(xPosition - width/2);
						rectSize.Width = (float)(width);


						// The top side of rectangle has always 
						// smaller value than a bottom value
						if( columnStartPosition < height )
						{
							rectSize.Y = (float)columnStartPosition;
							rectSize.Height = (float)height - rectSize.Y;
						}
						else
						{
							rectSize.Y = (float)height;
							rectSize.Height = (float)columnStartPosition - rectSize.Y;
						}

					}
					catch(OverflowException)
					{
						continue;
					}

					// if data point is not empty
					if( point.IsEmpty )
					{
						continue;
					}

					//************************************************************
					// Painting mode
					//************************************************************
					if( common.ProcessModePaint )
					{
						if( !labels )
						{
							// Check if column is completly out of the data scaleView
							double	xValue = (indexedSeries) ? index : point.XValue;
							xValue = hAxis.GetLogValue(xValue);
							if(xValue < horizontalViewMin || xValue > horizontalViewMax )
							{
								continue;
							}

							// Check if column is partialy in the data scaleView
							bool	clipRegionSet = false;
							if(rectSize.X < area.PlotAreaPosition.X || rectSize.Right > area.PlotAreaPosition.Right)
							{
								// Set clipping region for line drawing 
								graph.SetClip( area.PlotAreaPosition.ToRectangleF() );
								clipRegionSet = true;
							}

							// Start Svg Selection mode
							graph.StartHotRegion( point );

							// Draw the Column rectangle
							DrawColumn2D(graph, vAxis, rectSize, point, ser);

							// End Svg Selection mode
							graph.EndHotRegion( );

							// Reset Clip Region
							if(clipRegionSet)
							{
								graph.ResetClip();
							}
						}
						else if(this.useTwoValues)
						{
							// Draw labels and markers
							DrawLabel( 
								area, 
								graph, 
								common, 
								rectSize, 
								point, 
								ser,
								index);
						}
					}

					//************************************************************
					// Hot Regions mode used for image maps, tool tips and 
					// hit test function
					//************************************************************
					if( common.ProcessModeRegions && !labels)
					{
						common.HotRegionsList.AddHotRegion( rectSize, point, ser.Name, index - 1 );
					}
				}
				
				// Call Paint event
				if( !selection )
				{
                    common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}

				// Data series index
				if(currentDrawSeriesSideBySide)
				{
					seriesIndx++;
				}

				// Draw labels and markers using the base class algorithm
				if( labels && !this.useTwoValues)
				{
					base.ProcessChartType( false, graph, common, area, seriesToDraw );
				}
			}
		}

		/// <summary>
		/// Draws 2D column.
		/// </summary>
		/// <param name="graph">Chart graphics.</param>
		/// <param name="vAxis">Vertical axis.</param>
		/// <param name="rectSize">Column position and size.</param>
		/// <param name="point">Column data point.</param>
		/// <param name="ser">Column series.</param>
		protected virtual void DrawColumn2D( 
			ChartGraphics graph,
			Axis vAxis,
			RectangleF rectSize,
			DataPoint point, 
			Series ser)
		{
			graph.FillRectangleRel( 
				rectSize, 
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
				true);
		}

		/// <summary>
		/// Gets label position for the column depending on the Y value.
		/// </summary>
		/// <returns>Return automaticly detected label position.</returns>
		/// <param name="series">Data series.</param>
		/// <param name="pointIndex">Point index.</param>
		/// <returns>Label aligning.</returns>
		override protected LabelAlignmentStyles GetAutoLabelPosition(Series series, int pointIndex)
		{
			if( series.Points[pointIndex].YValues[0] >= 0 )
				return LabelAlignmentStyles.Top;
			else
				return LabelAlignmentStyles.Bottom;
		}

		/// <summary>
		/// Indicates that markers are drawnd on the X edge of the data scaleView.
		/// </summary>
		/// <returns>False. Column chart never draws markers on the edge.</returns>
		override protected bool ShouldDrawMarkerOnViewEdgeX()
		{
			return false;
		}

		#endregion

		#region 3D painting and selection methods

		/// <summary>
		/// This method recalculates size of the columns and paint them or do the hit test in 3d space.
		/// This method is used from Paint or Select method.
		/// </summary>
		/// <param name="labels">Mode which draws only labels and markers.</param>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		private void ProcessChartType3D( bool labels, bool selection, ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
		{
			// Labels & markers are drawn with the data points in the first iteration
			if(labels && !selection)
			{
				return;
			}
			
			// Get pixel size
			SizeF	pixelRelSize = graph.GetRelativeSize(new SizeF(1.1f, 1.1f));

			// Get list of series to draw
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
						if(String.Compare( attribValue, "False", StringComparison.OrdinalIgnoreCase)==0)
						{
							currentDrawSeriesSideBySide = false;
						}
						else if(String.Compare( attribValue, "True", StringComparison.OrdinalIgnoreCase)==0)
						{
							currentDrawSeriesSideBySide = true;
						}
						else if(String.Compare( attribValue, "Auto", StringComparison.OrdinalIgnoreCase)==0)
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
			ArrayList	dataPointDrawingOrder = area.GetDataPointDrawingOrder(typeSeries, this, selection, coordinates, null, this.YValueIndex, currentDrawSeriesSideBySide);

			//************************************************************
			//** Loop through all data poins
			//************************************************************
			foreach(object obj in dataPointDrawingOrder)
			{
				// Get point & series
				DataPoint3D	pointEx = (DataPoint3D) obj;
				DataPoint	point = pointEx.dataPoint;
				Series		ser = point.series;

				// Get point bar drawing style
				BarDrawingStyle	barDrawingStyle = ChartGraphics.GetBarDrawingStyle(point);

				// Set active vertical/horizontal axis
				Axis	vAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);
				Axis	hAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);

				// Change Y value if Column is out of plot area
				float	topDarkening = 0f;
				float	bottomDarkening = 0f;
				double	yValue = GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, (useTwoValues) ? 1 : 0);
				yValue = vAxis.GetLogValue(yValue);
				if( yValue > vAxis.ViewMaximum )
				{
					topDarkening = 0.5f;
					yValue = vAxis.ViewMaximum;
				}
				if( yValue < vAxis.ViewMinimum )
				{
					topDarkening = 0.5f;
					yValue = vAxis.ViewMinimum;
				}

				// Recalculates Height position and zero position of Columns
				double	height = vAxis.GetLinearPosition( yValue );

				// Set start position for a column
				double	columnStartPosition = 0;
				if(useTwoValues)
				{
					// Point Y value (first) is used to determine the column starting position
					double yValueStart = vAxis.GetLogValue( GetYValue(common, area, ser, point, pointEx.index - 1, 0 ) );
					if( yValueStart > vAxis.ViewMaximum )
					{
						bottomDarkening = 0.5f;
						yValueStart = vAxis.ViewMaximum;
					}
					else if( yValueStart < vAxis.ViewMinimum )
					{
						bottomDarkening = 0.5f;
						yValueStart = vAxis.ViewMinimum;
					}

					columnStartPosition = vAxis.GetLinearPosition(yValueStart);
				}
				else
				{
					// Column starts on the horizontal axis crossing
					columnStartPosition = vAxis.GetPosition(vAxis.Crossing);
				}

				// Labels and markers have to be shifted if there 
				// is more than one series for column chart.
				if(!currentDrawSeriesSideBySide)
				{
					pointEx.xPosition = pointEx.xCenterVal;
				}
				ShiftedX = pointEx.xPosition - pointEx.xCenterVal;
					
				// Make sure that points with small values are still visible
				if( height < columnStartPosition && 
					(columnStartPosition - height) <  pixelRelSize.Height)
				{
					height = columnStartPosition - pixelRelSize.Height;
				}
				if( height > columnStartPosition && 
					(height - columnStartPosition) <  pixelRelSize.Height)
				{
					height = columnStartPosition + pixelRelSize.Height;
				}

				// Get column rectangle
				RectangleF	rectSize = RectangleF.Empty;
				try
				{
					// Set the Column rectangle
					rectSize.X = (float)(pointEx.xPosition - pointEx.width/2);
					rectSize.Width = (float)(pointEx.width);

					// The top side of rectangle has always 
					// smaller value than a bottom value
					if( columnStartPosition < height )
					{
						float temp = bottomDarkening;
						bottomDarkening = topDarkening;
						topDarkening = temp;
						
						rectSize.Y = (float)columnStartPosition;
						rectSize.Height = (float)height - rectSize.Y;
					}
					else
					{
						rectSize.Y = (float)height;
						rectSize.Height = (float)columnStartPosition - rectSize.Y;
					}
				}
				catch(OverflowException)
				{
					continue;
				}

				//************************************************************
				//** Painting mode
				//************************************************************
				// Path projection of 3D rect.
				GraphicsPath rectPath = null;

				// Check if column is completly out of the data scaleView
				double	xValue = (pointEx.indexedSeries) ? pointEx.index : point.XValue;
				xValue = hAxis.GetLogValue(xValue);
				if(xValue < hAxis.ViewMinimum || xValue > hAxis.ViewMaximum )
				{
					continue;
				}

				// Check if column is partialy in the data scaleView
				bool	clipRegionSet = false;
				if(rectSize.Right <= area.PlotAreaPosition.X || rectSize.X >= area.PlotAreaPosition.Right)
				{
					continue;
				}

				if(rectSize.X < area.PlotAreaPosition.X)
				{
					rectSize.Width -= area.PlotAreaPosition.X - rectSize.X;
					rectSize.X = area.PlotAreaPosition.X;
				}
				if(rectSize.Right > area.PlotAreaPosition.Right)
				{
					rectSize.Width -= rectSize.Right - area.PlotAreaPosition.Right;
				}
				if(rectSize.Width < 0)
				{
					rectSize.Width = 0;
				}

				// Detect if we need to get graphical path of drawn object
				DrawingOperationTypes	drawingOperationType = DrawingOperationTypes.DrawElement;
				
				if( common.ProcessModeRegions )
				{
					drawingOperationType |= DrawingOperationTypes.CalcElementPath;
				}

				if(!point.IsEmpty &&
					rectSize.Height > 0f &&
					rectSize.Width > 0f)
				{
					// Start Svg Selection mode
					graph.StartHotRegion( point );

					rectPath = graph.Fill3DRectangle(
						rectSize,
						pointEx.zPosition,
						pointEx.depth,
						area.matrix3D,
						area.Area3DStyle.LightStyle,
						point.Color, 
						topDarkening,
						bottomDarkening,
						point.BorderColor, 
						point.BorderWidth, 
						point.BorderDashStyle, 
						barDrawingStyle,
						true,
						drawingOperationType);

					// End Svg Selection mode
					graph.EndHotRegion( );

					//************************************************************
					// Hot Regions mode used for image maps, tool tips and 
					// hit test function
					//************************************************************
					if( common.ProcessModeRegions && !labels)
					{
						common.HotRegionsList.AddHotRegion(
							rectPath,
							false,
							graph,
							point,
							ser.Name,
							pointEx.index - 1
							);
					}
                    if (rectPath != null)
                    {
                        rectPath.Dispose();
                    }
				}

				// Reset Clip Region
				if(clipRegionSet)
				{
					graph.ResetClip();
				}

				// Draw Labels & markers for each data point
				this.ProcessSinglePoint3D(
					pointEx,
					selection, 
					graph, 
					common, 
					area, 
					rectSize,
					pointEx.index - 1
					);
			}

			// Finish processing 3D labels
			this.DrawAccumulated3DLabels(graph, common, area);
		}

		#endregion

		#region 2D and 3D Labels Drawing

		/// <summary>
		/// This method draws label.
		/// </summary>
		/// <param name="graph">The Chart Graphics object</param>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="columnPosition">Column position</param>
		/// <param name="point">Data point</param>
		/// <param name="ser">Data series</param>
		/// <param name="pointIndex">Data point index.</param>
		protected virtual void DrawLabel( 
			ChartArea area, 
			ChartGraphics graph, 
			CommonElements common, 
			RectangleF columnPosition, 
			DataPoint point, 
			Series ser,
			int pointIndex)
		{
            // Labels drawing functionality is inhereted from the PointChart class.
		}

		/// <summary>
		/// Draws\Hit tests single 3D point.
		/// </summary>
		/// <param name="pointEx">3D point information.</param>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="columnPosition">Column position</param>
		/// <param name="pointIndex">Point index.</param>
		protected virtual void ProcessSinglePoint3D( 
			DataPoint3D	pointEx,
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			RectangleF columnPosition,
			int pointIndex
			)
		{
			// Draw Labels & markers for each data point
			base.ProcessSinglePoint3D(
				pointEx,
				graph, 
				common, 
				area
				);
		}

		#endregion
	}

    /// <summary>
    /// ColumnChart class contains all the code necessary to draw 
    /// both Column and RangeColumn charts. The RangeColumnChart class 
    /// is used to override few default settings, so that 2 Y values 
    /// will be used to define top and bottom position of each column.
    /// </summary>
    internal class RangeColumnChart : ColumnChart
	{
		#region Constructor

		/// <summary>
		/// Public constructor
		/// </summary>
		public RangeColumnChart()
		{
			// Set the flag to use two Y values, while drawing the columns
			this.useTwoValues = true;

			// Coordinates of COP used when sorting 3D points order
			this.coordinates = COPCoordinates.X | COPCoordinates.Y;

			// Index of the main Y value
			this.YValueIndex = 1;
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		override public string Name			{ get{ return ChartTypeNames.RangeColumn;}}

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
		override public double GetYValue(
			CommonElements common, 
			ChartArea area, 
			Series series, 
			DataPoint point, 
			int pointIndex, 
			int yValueIndex)
		{
			// Calculate column height
			if(yValueIndex == -1)
			{
				return -(base.GetYValue(common, area, series, point, pointIndex, 1) - 
					 base.GetYValue(common, area, series, point, pointIndex, 0));
			}

			return base.GetYValue(common, area, series, point, pointIndex, yValueIndex);
		}

		#endregion

		#region 2D and 3D Labels Drawing

		/// <summary>
		/// This method draws label.
		/// </summary>
		/// <param name="graph">The Chart Graphics object</param>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="columnPosition">Column position</param>
		/// <param name="point">Data point</param>
		/// <param name="series">Data series</param>
		/// <param name="pointIndex">Data point index.</param>
		protected override void DrawLabel( 
			ChartArea area, 
			ChartGraphics graph, 
			CommonElements common, 
			RectangleF columnPosition, 
			DataPoint point, 
			Series series,
			int pointIndex)
		{
			//************************************************************
			//** Get marker position and size
			//************************************************************

			// Get intersection between column rectangle and plotting area rectangle
			RectangleF intersection = RectangleF.Intersect( 
				columnPosition, area.PlotAreaPosition.ToRectangleF() );

			// If intersection is empty no drawing required
			if(intersection.Height <= 0f || intersection.Width <= 0f)
			{
				return;
			}

			// Get marker position
			PointF markerPosition = PointF.Empty;
			markerPosition.X = intersection.X + intersection.Width / 2f;
			markerPosition.Y = intersection.Y;

			// Remeber pre-calculated point position
			point.positionRel = new PointF(markerPosition.X, markerPosition.Y);

			// Get point some point properties and save them in variables
			int			pointMarkerSize = point.MarkerSize;
			string		pointMarkerImage = point.MarkerImage;
			MarkerStyle	pointMarkerStyle = point.MarkerStyle;

			// Get marker size
			SizeF markerSize = base.GetMarkerSize(
				graph, 
				common, 
				area, 
				point, 
				pointMarkerSize, 
				pointMarkerImage);

			//************************************************************
			//** Draw point chart
			//************************************************************
			if(pointMarkerStyle != MarkerStyle.None || 
				pointMarkerImage.Length > 0)
			{
			// Start Svg Selection mode
			graph.StartHotRegion( point );

			// Draw the marker
			graph.DrawMarkerRel(markerPosition, 
				(pointMarkerStyle == MarkerStyle.None) ? MarkerStyle.Circle : pointMarkerStyle,
				(int)markerSize.Height,
				(point.MarkerColor == Color.Empty) ? point.Color : point.MarkerColor,
				(point.MarkerBorderColor == Color.Empty) ? point.BorderColor : point.MarkerBorderColor,
				GetMarkerBorderSize(point),
				pointMarkerImage,
				point.MarkerImageTransparentColor,
				(point.series != null) ? point.series.ShadowOffset : 0,
				(point.series != null) ? point.series.ShadowColor : Color.Empty,
				new RectangleF(markerPosition.X, markerPosition.Y, markerSize.Width, markerSize.Height));

				// End Svg Selection mode
				graph.EndHotRegion( );

				//************************************************************
				// Hot Regions mode used for image maps, tool tips and 
				// hit test function
				//************************************************************
				if( common.ProcessModeRegions )
				{
					// Get relative marker size
					SizeF relativeMarkerSize = graph.GetRelativeSize(markerSize);

					// Insert area just after the last custom area
					int insertIndex = common.HotRegionsList.FindInsertIndex();

					// Insert circle area
					if(pointMarkerStyle == MarkerStyle.Circle)
					{
						float[]	circCoord = new float[3];
						circCoord[0] = markerPosition.X;
						circCoord[1] = markerPosition.Y;
						circCoord[2] = relativeMarkerSize.Width/2f;

						common.HotRegionsList.AddHotRegion( 
							insertIndex,
							graph,
							circCoord[0],
							circCoord[1],
							circCoord[2],
							point,
							series.Name,
							pointIndex - 1 );
					}
					// All other markers represented as rectangles
					else
					{
						common.HotRegionsList.AddHotRegion( 
							new RectangleF(markerPosition.X - relativeMarkerSize.Width/2f, markerPosition.Y - relativeMarkerSize.Height/2f, relativeMarkerSize.Width, relativeMarkerSize.Height),
							point,
							series.Name,
							pointIndex - 1 );
					}
				}
			}
		
		
			//************************************************************
			//** Draw LabelStyle
			//************************************************************

			// Label text format
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                // Disable the clip region
                Region oldClipRegion = graph.Clip;
                graph.Clip = new Region();

                if (point.IsValueShownAsLabel || point.Label.Length > 0)
                {
                    // Get label text
                    string text;
                    if (point.Label.Length == 0)
                    {
                        // Round Y values for 100% stacked area
                        double pointLabelValue = GetYValue(common, area, series, point, pointIndex, 0);

                        text = ValueConverter.FormatValue(
                            series.Chart,
                            point,
                            point.Tag,
                            pointLabelValue,
                            point.LabelFormat,
                            series.YValueType,
                            ChartElementType.DataPoint);
                    }
                    else
                    {
                        text = point.ReplaceKeywords(point.Label);
                    }

                    // Calculate label position
                    PointF labelPosition = PointF.Empty;
                    labelPosition.X = intersection.X + intersection.Width / 2f;
                    labelPosition.Y = intersection.Y + intersection.Height / 2f;

                    // Start Svg Selection mode
                    graph.StartHotRegion(point, true);

                    // Get string size
                    SizeF sizeFont = graph.GetRelativeSize(graph.MeasureString(text, point.Font, new SizeF(1000f, 1000f), StringFormat.GenericTypographic));

                    // Get label background position
                    RectangleF labelBackPosition = RectangleF.Empty;
                    SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
                    sizeLabel.Width += sizeLabel.Width / text.Length;
                    sizeLabel.Height += sizeFont.Height / 8;
                    labelBackPosition = GetLabelPosition(
                        graph,
                        labelPosition,
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
                            labelPosition,
                            format,
                            point.LabelAngle,
                            labelBackPosition,
                            point.LabelBackColor,
                            point.LabelBorderColor,
                            point.LabelBorderWidth,
                            point.LabelBorderDashStyle,
                            series,
                            point,
                            pointIndex - 1);
                    }

                    // End Svg Selection mode
                    graph.EndHotRegion();
                }

                // Restore old clip region
                graph.Clip = oldClipRegion;
            }
		}

		/// <summary>
		/// Draws\Hit tests single 3D point.
		/// </summary>
		/// <param name="pointEx">3D point information.</param>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="columnPosition">Column position</param>
		/// <param name="pointIndex">Point index.</param>
		protected override void ProcessSinglePoint3D( 
			DataPoint3D	pointEx,
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			RectangleF columnPosition,
			int pointIndex
			)
		{
			DataPoint point = pointEx.dataPoint; 

			// Check required Y values number
			if(point.YValues.Length < this.YValuesPerPoint)
			{
				throw(new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues(this.Name,this.YValuesPerPoint.ToString(CultureInfo.InvariantCulture))));
			}

			// Label text format
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                // Disable the clip region
                Region oldClipRegion = graph.Clip;
                graph.Clip = new Region();

                if (point.IsValueShownAsLabel || point.Label.Length > 0)
                {
                    // Get label text
                    string text;
                    if (point.Label.Length == 0)
                    {
                        // Get Y value
                        double pointLabelValue = GetYValue(common, area, pointEx.dataPoint.series, point, pointEx.index - 1, 0);
                        text = ValueConverter.FormatValue(
                            pointEx.dataPoint.series.Chart,
                            point,
                            point.Tag,
                            pointLabelValue,
                            point.LabelFormat,
                            pointEx.dataPoint.series.YValueType,
                            ChartElementType.DataPoint);
                    }
                    else
                    {
                        text = point.ReplaceKeywords(point.Label);

                    }

                    // Calculate label position
                    PointF labelPosition = PointF.Empty;
                    labelPosition.X = columnPosition.X + columnPosition.Width / 2f;
                    labelPosition.Y = columnPosition.Y + columnPosition.Height / 2f;

                    // Transform coordinates
                    Point3D[] marker3DPosition = new Point3D[1];
                    marker3DPosition[0] = new Point3D(labelPosition.X, labelPosition.Y, (float)(pointEx.zPosition + pointEx.depth));
                    area.matrix3D.TransformPoints(marker3DPosition);

                    labelPosition.X = marker3DPosition[0].X;
                    labelPosition.Y = marker3DPosition[0].Y;

                    // Start Svg Selection mode
                    graph.StartHotRegion(point, true);

                    // Get string size
                    SizeF sizeFont = graph.GetRelativeSize(graph.MeasureString(text, point.Font, new SizeF(1000f, 1000f), StringFormat.GenericTypographic));

                    // Get label background position
                    RectangleF labelBackPosition = RectangleF.Empty;
                    SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
                    sizeLabel.Width += sizeLabel.Width / text.Length;
                    sizeLabel.Height += sizeFont.Height / 8;
                    labelBackPosition = GetLabelPosition(
                        graph,
                        labelPosition,
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
                            labelPosition,
                            format,
                            point.LabelAngle,
                            labelBackPosition,
                            point.LabelBackColor,
                            point.LabelBorderColor,
                            point.LabelBorderWidth,
                            point.LabelBorderDashStyle,
                            point.series,
                            point,
                            pointIndex);
                    }

                    // End Svg Selection mode
                    graph.EndHotRegion();
                }

                // Restore old clip region
                graph.Clip = oldClipRegion;
            }
		}

		#endregion
	}
}
