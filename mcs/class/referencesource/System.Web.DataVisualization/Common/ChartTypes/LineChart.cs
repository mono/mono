//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		LineChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	LineChart, SplineChart
//
//  Purpose:	Provides 2D/3D drawing and hit testing 
//              functionality for the Line and Spline charts.
//
//	Reviewed:	AG - August 6, 2002
//              AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

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
    /// SplineChart class extends the LineChart class by 
    /// providing a different initial tension for the line.
	/// </summary>
	internal class SplineChart : LineChart
	{
		#region Constructor

		/// <summary>
		/// Default constructor.
		/// </summary>
		public SplineChart()
		{
			// Set default line tension
			base.lineTension = 0.5f;
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return ChartTypeNames.Spline;}}

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

		#region Helper methods

		/// <summary>
		/// Checks if line tension is supported by the chart type.
		/// </summary>
		/// <returns>True if line tension is supported.</returns>
		protected override bool IsLineTensionSupported()
		{
			return true;
		}

		/// <summary>
		/// Fills a PointF array of data points positions.
		/// </summary>
		/// <param name="graph">Graphics object.</param>
		/// <param name="series">Point series.</param>
		/// <param name="indexedSeries">Indicate that point index should be used as X value.</param>
		/// <returns>Array of data points position.</returns>
		override protected PointF[] GetPointsPosition(
			ChartGraphics graph, 
			Series series, 
			bool indexedSeries)
		{
			// Check tension attribute in the series
			base.lineTension = GetDefaultTension();
			if(IsLineTensionSupported() && series.IsCustomPropertySet(CustomPropertyName.LineTension))
			{
				base.lineTension = CommonElements.ParseFloat(series[CustomPropertyName.LineTension]);
			}

			// Call base LineChart class
			return base.GetPointsPosition(graph, series, indexedSeries);
		}

		/// <summary>
		/// Gets default line tension.
		/// </summary>
		/// <returns>Default line tension.</returns>
		override protected float GetDefaultTension()
		{
			return 0.5f;
		}

		#endregion
	}

	/// <summary>
    /// LineChart class provides 2D/3D drawing and hit testing 
    /// functionality for the Line and Spline charts. The only 
    /// difference of the Spline chart is the default tension 
    /// of the line.
    /// 
    /// PointChart base class provides functionality realted
    /// to drawing labels and markers.
	/// </summary>
	internal class LineChart : PointChart
	{
		#region Fields and Constructor
		
		/// <summary>
		/// Line tension
		/// </summary>
		protected	float	lineTension = 0f;

		/// <summary>
		/// Index of the drawing center point. int.MaxValue if drawn from left->right or right->left.
		/// </summary>
		protected	int		centerPointIndex = int.MaxValue;

		/// <summary>
		/// Inicates that border color attribute must be used to draw the line
		/// </summary>
		protected	bool	useBorderColor = false;

		/// <summary>
		/// Inicates that line shadow should not be drawn
		/// </summary>
		protected	bool	disableShadow = false;

		/// <summary>
		/// Inicates that only line shadow must be drawn
		/// </summary>
		protected	bool	drawShadowOnly = false;

		// Pen used to draw the line chart
		private		Pen		_linePen = new Pen(Color.Black);

		/// <summary>
		/// Horizontal axis minimum value
		/// </summary>
		protected	double	hAxisMin = 0.0;

		/// <summary>
		/// Horizontal axis maximum value
		/// </summary>
		protected	double	hAxisMax = 0.0;

		/// <summary>
		/// Vertical axis minimum value
		/// </summary>
		protected	double	vAxisMin = 0.0;

		/// <summary>
		/// Vertical axis maximum value
		/// </summary>
		protected	double	vAxisMax = 0.0;

		/// <summary>
		/// Clip region indicator
		/// </summary>
		protected	bool	clipRegionSet = false;
		
		/// <summary>
		/// Indicates that several series are drawn at the same time. Stacked or Side-by-side.
		/// </summary>
		protected	bool	multiSeries = false;

		/// <summary>
		/// Indicates which coordinates should be tested against the COP.
		/// </summary>
		protected	COPCoordinates	COPCoordinatesToCheck = COPCoordinates.X;

		/// <summary>
		/// Number of data points loops required to draw chart.
		/// </summary>
		protected	int		allPointsLoopsNumber = 1;

		/// <summary>
		/// Indicates that line markers are shown at data point.
		/// </summary>
		protected	bool	showPointLines = false;

		/// <summary>
		/// Indicates that that lines outside the area should be still processed while drawing.
		/// </summary>
		protected	bool	drawOutsideLines = false;


		/// <summary>
		/// Indicates if base (point) chart type should be processed
		/// </summary>
		private		bool	_processBaseChart = false;

		/// <summary>
		/// Default constructor
		/// </summary>
		public LineChart() : base(false)
		{
			// Draw markers on the front edge
			middleMarker = false;
		}

		#endregion

		#region IChartType interface implementation
		
		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return ChartTypeNames.Line;}}

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
		public override bool Stacked		{ get{ return false;}}

		/// <summary>
		/// True if chart type supports axeses
		/// </summary>
		public override bool RequireAxes	{ get{ return true;} }

		/// <summary>
		/// True if chart type supports logarithmic axes
		/// </summary>
		public override bool SupportLogarithmicAxes	{ get{ return true;} }

		/// <summary>
		/// True if chart type requires to switch the value (Y) axes position
		/// </summary>
		public override bool SwitchValueAxes	{ get{ return false;} }

		/// <summary>
		/// True if chart series can be placed side-by-side.
		/// </summary>
		override public bool SideBySideSeries { get{ return false;} }

		/// <summary>
		/// If the crossing value is auto Crossing value should be 
		/// automatically set to zero for some chart 
		/// types (Bar, column, area etc.)
		/// </summary>
		override public bool ZeroCrossing { get{ return true;} }

		/// <summary>
		/// True if each data point of a chart must be represented in the legend
		/// </summary>
		public override bool DataPointsInLegend	{ get{ return false;} }

		/// <summary>
		/// True if palette colors should be applied for each data paoint.
		/// Otherwise the color is applied to the series.
		/// </summary>
		public override bool ApplyPaletteColorsToPoints	{ get { return false; } }

		/// <summary>
		/// How to draw series/points in legend:
		/// Filled rectangle, Line or Marker
		/// </summary>
		/// <param name="series">Legend item series.</param>
		/// <returns>Legend item style.</returns>
		override public LegendImageStyle GetLegendImageStyle(Series series)
		{
			return LegendImageStyle.Line;
		}
	
		/// <summary>
		/// Number of supported Y value(s) per point 
		/// </summary>
		public override int YValuesPerPoint{ get { return 1; } }

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
			// Save chart area reference
			this.Area = area;
            this.Common = common;
			// Draw lines
			_processBaseChart = false;
			ProcessChartType( false, graph, common, area, seriesToDraw );

			// Draw labels and markers using base class PointChart
			if(_processBaseChart)
			{
				base.ProcessChartType( false, graph, common, area, seriesToDraw );
			}
		}
	
		/// <summary>
		/// Draws or perform the hit test for the line chart.
		/// </summary>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		protected override void ProcessChartType( 
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			Series seriesToDraw )
		{
            this.Common = common;
            // Prosess 3D chart type
			if(area.Area3DStyle.Enable3D)
			{
				_processBaseChart = true;
				ProcessLineChartType3D( selection, graph, common, area, seriesToDraw );
				return;
			}
			
			
			// All data series from chart area which have Bar chart type
			List<string>	typeSeries = area.GetSeriesFromChartType(this.Name);

			// Check if series are indexed
            bool indexedSeries = ChartHelper.IndexedSeries(this.Common, typeSeries.ToArray());
		
			//************************************************************
			//** Loop through all series
			//************************************************************
			foreach( Series ser in common.DataManager.Series )
			{
				// Process non empty series of the area with Line chart type
				if( String.Compare( ser.ChartTypeName, this.Name, true, System.Globalization.CultureInfo.CurrentCulture ) != 0 
					|| ser.ChartArea != area.Name || !ser.IsVisible())
				{
					continue;
				}

                // Check if only 1 specified series must be processed
                if (seriesToDraw != null && seriesToDraw.Name != ser.Name)
                {
                    continue;
                }

				// Set active horizontal/vertical axis
				HAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
				VAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);
				hAxisMin = HAxis.ViewMinimum;
				hAxisMax = HAxis.ViewMaximum;
				vAxisMin = VAxis.ViewMinimum;
				vAxisMax = VAxis.ViewMaximum;

                float chartWidthPercentage = (graph.Common.ChartPicture.Width - 1) / 100F;
                float chartHeightPercentage = (graph.Common.ChartPicture.Height - 1) / 100F; 

				// Call Back Paint event
				if( !selection )
				{
                    common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}

                // Check tension attribute in the series
                this.lineTension = GetDefaultTension();
                if (IsLineTensionSupported() && ser.IsCustomPropertySet(CustomPropertyName.LineTension))
                {
                    this.lineTension = CommonElements.ParseFloat(ser[CustomPropertyName.LineTension]);
                }

				// Fill the array of data points coordinates (absolute)
				bool		dataPointPosFilled = false;
				PointF[]	dataPointPos = null;
				if(this.lineTension == 0 && !common.ProcessModeRegions)
				{
					dataPointPos = new PointF[ser.Points.Count];
				}
				else
				{
					dataPointPosFilled = true;
					dataPointPos = GetPointsPosition(graph, ser, indexedSeries);

					//*************************************************************************
					//** Solution for the "Out of Memory"  exception in the DrawCurve method
					//** All points in the array should be at least 0.1 pixel apart.
					//*************************************************************************
					if(this.lineTension != 0)
					{
						float	minDifference = 0.1f;
						for(int pointIndex = 1; pointIndex < dataPointPos.Length; pointIndex++)
						{
							if( Math.Abs(dataPointPos[pointIndex - 1].X - dataPointPos[pointIndex].X ) < minDifference )
							{
								if(dataPointPos[pointIndex].X > dataPointPos[pointIndex - 1].X)
								{
									dataPointPos[pointIndex].X = dataPointPos[pointIndex - 1].X + minDifference;
								}
								else
								{
									dataPointPos[pointIndex].X = dataPointPos[pointIndex - 1].X - minDifference;
								}
							}
							if( Math.Abs(dataPointPos[pointIndex - 1].Y - dataPointPos[pointIndex].Y ) < minDifference )
							{
								if(dataPointPos[pointIndex].Y > dataPointPos[pointIndex - 1].Y)
								{
									dataPointPos[pointIndex].Y = dataPointPos[pointIndex - 1].Y + minDifference;
								}
								else
								{
									dataPointPos[pointIndex].Y = dataPointPos[pointIndex - 1].Y - minDifference;
								}
							}
						}
					}

				}

				// Draw line if we have more than one data point
				if(dataPointPos.Length > 1)
				{
					// Draw each data point
					int index = 0;
					DataPoint	prevDataPoint = null;
					double	yValuePrev = 0.0;
					double	xValuePrev = 0.0;
					bool showLabelAsValue = ser.IsValueShownAsLabel;
					bool prevPointInArray = false;
					foreach( DataPoint point in ser.Points )
					{
						prevPointInArray = false;

						// Reset pre-calculated point position
						point.positionRel = new PointF(float.NaN, float.NaN);

						//************************************************************
						//** Check if point marker or label is visible
						//************************************************************
						if(!_processBaseChart)
						{
							string		pointMarkerImage = point.MarkerImage;
							MarkerStyle	pointMarkerStyle = point.MarkerStyle;

							if( alwaysDrawMarkers || 
								pointMarkerStyle != MarkerStyle.None || 
								pointMarkerImage.Length > 0 ||
								showLabelAsValue || 
								point.IsValueShownAsLabel ||
								point.Label.Length > 0 )
							{
								_processBaseChart = true;
							}
						}

						// Change Y value if line is out of plot area
						double yValue = GetYValue(common, area, ser, point, index, this.YValueIndex);

						// Recalculates x position
						double	xValue = (indexedSeries) ? index + 1 : point.XValue;

						// If not first point
						if(index != 0)
						{
							// Axes are logarithmic
							yValue = VAxis.GetLogValue( yValue );
							xValue = HAxis.GetLogValue( xValue );
							
							// Check if line is completly out of the data scaleView
							if( (xValue <= hAxisMin && xValuePrev < hAxisMin) ||
								(xValue >= hAxisMax && xValuePrev > hAxisMax) ||
								(yValue <= vAxisMin && yValuePrev < vAxisMin) ||
								(yValue >= vAxisMax && yValuePrev > vAxisMax) )
							{
								if(!drawOutsideLines)
								{
									// Check if next point also outside of the scaleView and on the 
									// same side as current point. If not line has to be processed
									// to correctly handle tooltips.
									// NOTE: Fixes issue #4961
									bool skipPoint = true;
									if( common.ProcessModeRegions &&
										(index + 1) < ser.Points.Count)
									{
										DataPoint nextPoint = ser.Points[index + 1];

										// Recalculates x position
										double	xValueNext = (indexedSeries) ? index + 2 : nextPoint.XValue;

										if( (xValue < hAxisMin && xValueNext > hAxisMin) ||
											(xValue > hAxisMax && xValueNext < hAxisMax) )
										{
											skipPoint = false;
										}


										// Change Y value if line is out of plot area
										if(skipPoint)
										{
											if( (yValue < vAxisMin && xValueNext > vAxisMin) ||
												(yValue > vAxisMax && xValueNext < vAxisMax) )
											{
												skipPoint = false;
											}
										}
									}

									// Skip point
									if(skipPoint)
									{
										++index;
										prevDataPoint = point;
										yValuePrev = yValue;
										xValuePrev = xValue;
										continue;
									}
								}
							}

							// Check if line is partialy in the data scaleView
							clipRegionSet = false;
							if(this.lineTension != 0.0 ||
								xValuePrev < hAxisMin || xValuePrev > hAxisMax || 
								xValue > hAxisMax || xValue < hAxisMin ||
								yValuePrev < vAxisMin || yValuePrev > vAxisMax ||
								yValue < vAxisMin || yValue > vAxisMax )
							{
								// Set clipping region for line drawing 
								graph.SetClip( area.PlotAreaPosition.ToRectangleF() );
								clipRegionSet = true;
							}


							if(this.lineTension == 0 && !dataPointPosFilled)
							{
								float yPosition = 0f;
								float xPosition = 0f;

								// Line reqires two points to draw
								// Check if previous point is in the array
								if(!prevPointInArray)
								{
									// Recalculates x/y position
									yPosition = (float)VAxis.GetLinearPosition( yValuePrev );
									xPosition = (float)HAxis.GetLinearPosition( xValuePrev );

									// Add point position into array
									// IMPORTANT: Rounding was removed from this part of code because of 
									// very bad drawing in Flash.
									dataPointPos[index - 1] = new PointF(
										xPosition * chartWidthPercentage,
										yPosition * chartHeightPercentage); 
								}


								// Recalculates x/y position
								yPosition = (float)VAxis.GetLinearPosition( yValue );
								xPosition = (float)HAxis.GetLinearPosition( xValue );

								// Add point position into array
								// IMPORTANT: Rounding was removed from this part of code because of 
								// very bad drawing in Flash.
								dataPointPos[index] = new PointF(
									xPosition * chartWidthPercentage,
									yPosition * chartHeightPercentage);

								prevPointInArray = true;
							}

							// Remeber pre-calculated point position
							point.positionRel = graph.GetRelativePoint(dataPointPos[index]);

							// Start Svg Selection mode
							graph.StartHotRegion( point );

							if( index != 0 && prevDataPoint.IsEmpty )
							{ 
								// IsEmpty data point - second line
								DrawLine(
									graph, 
									common, 
									prevDataPoint, 
									ser, 
									dataPointPos, 
									index, 
									this.lineTension);
							}
							else
							{
								// Regular data point and empty point - first line
								DrawLine(
									graph, 
									common, 
									point, 
									ser, 
									dataPointPos, 
									index, 
									this.lineTension);
							}

    						// End Svg Selection mode
							graph.EndHotRegion( );

							// Reset Clip Region
							if(clipRegionSet)
							{
								graph.ResetClip();
							}
						
							// Remember previous point data
							prevDataPoint = point;
							yValuePrev = yValue;
							xValuePrev = xValue;
						}
						else
						{
							// Get Y values of the current and previous data points
							prevDataPoint = point;
							yValuePrev = GetYValue(common, area, ser, point, index, 0);
							xValuePrev = (indexedSeries) ? index + 1 : point.XValue;
							yValuePrev = VAxis.GetLogValue( yValuePrev );
							xValuePrev = HAxis.GetLogValue( xValuePrev );

							// Remeber pre-calculated point position
							point.positionRel = new PointF(
								(float)HAxis.GetPosition( xValuePrev ), 
								(float)VAxis.GetPosition( yValuePrev ) );
						}

						// Process image map selection for the first point
						if(index == 0)
						{
							DrawLine(
								graph, 
								common, 
								point, 
								ser, 
								dataPointPos, 
								index, 
								this.lineTension);
						}

						// Increase data point index
						++index;
					}
				}
				else if(dataPointPos.Length == 1 &&
					ser.Points.Count == 1)
				{
					//************************************************************
					//** Check if point marker or label is visible
					//************************************************************
					if(!_processBaseChart)
					{
						if( alwaysDrawMarkers || 
							ser.Points[0].MarkerStyle != MarkerStyle.None || 
							ser.Points[0].MarkerImage.Length > 0 ||
							ser.IsValueShownAsLabel || 
							ser.Points[0].IsValueShownAsLabel ||
							ser.Points[0].Label.Length > 0 )
						{
							_processBaseChart = true;
						}
					}
				}

				// Reset points array
				dataPointPos = null;
						
				// Call Paint event
				if( !selection )
				{
                    common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}
			}
		}

		/// <summary>
		/// Calculate position and draw one chart line and/or shadow.
		/// </summary>
		/// <param name="graph">Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="point">Point to draw the line for.</param>
		/// <param name="series">Point series.</param>
		/// <param name="points">Array of oints coordinates.</param>
		/// <param name="pointIndex">Index of point to draw.</param>
		/// <param name="tension">Line tension.</param>
		virtual protected void DrawLine(
			ChartGraphics graph, 
			CommonElements common, 
			DataPoint point, 
			Series series, 
			PointF[] points, 
			int pointIndex, 
			float tension)
		{
			int	pointBorderWidth = point.BorderWidth;

			// ****************************************************
			// Paint Mode
			// ****************************************************
			if( common.ProcessModePaint )
			{
				// Start drawing from the second point
				if(pointIndex > 0)
				{
					Color			color = (useBorderColor) ? point.BorderColor : point.Color;
					ChartDashStyle	dashStyle = point.BorderDashStyle;

					// Draw line shadow
					if(!disableShadow && series.ShadowOffset != 0 && series.ShadowColor != Color.Empty)
					{
						if(color != Color.Empty && color != Color.Transparent && pointBorderWidth > 0 && dashStyle != ChartDashStyle.NotSet)
						{
							Pen shadowPen = new Pen((series.ShadowColor.A != 255) ? series.ShadowColor : Color.FromArgb((useBorderColor) ? point.BorderColor.A/2 : point.Color.A/2, series.ShadowColor), pointBorderWidth);
							shadowPen.DashStyle = graph.GetPenStyle( point.BorderDashStyle );
							shadowPen.StartCap = LineCap.Round;
							shadowPen.EndCap = LineCap.Round;

							// Translate curve
							GraphicsState graphicsState = graph.Save();
							Matrix transform = graph.Transform.Clone();
							transform.Translate(series.ShadowOffset, series.ShadowOffset);
							graph.Transform = transform;

							// Draw shadow
							if(this.lineTension == 0)
							{
                                try
                                {
                                    graph.DrawLine(shadowPen, points[pointIndex - 1], points[pointIndex]);
                                }
                                catch (OverflowException)
                                {
                                    this.DrawTruncatedLine(graph, shadowPen, points[pointIndex - 1], points[pointIndex]);
                                }
							}
							else
							{
								graph.DrawCurve(shadowPen, points, pointIndex - 1, 1, tension);
							}
							graph.Restore(graphicsState);
						}
					}

					// If only shadow must be drawn - return
					if(drawShadowOnly)
					{
						return;
					}

                    //// IsEmpty data ` color and style
                    // DT - removed code - line chart will have MS Office behavior for empty points -> broken line.
                    //if( point.IsEmpty )
                    //{
                    //    if( point.Color == Color.IsEmpty)
                    //    {
                    //        color = Color.Black;
                    //    }
                    //    if( point.BorderDashStyle == ChartDashStyle.NotSet )
                    //    {
                    //        dashStyle = ChartDashStyle.Dash;
                    //    }
                    //}

					// Draw data point line
					if(color != Color.Empty && pointBorderWidth > 0 && dashStyle != ChartDashStyle.NotSet)
					{
						if(_linePen.Color != color)
						{
							_linePen.Color = color;
						}
						if(_linePen.Width != pointBorderWidth)
						{
							_linePen.Width = pointBorderWidth;
						}
						if(_linePen.DashStyle != graph.GetPenStyle( dashStyle ))
						{
							_linePen.DashStyle = graph.GetPenStyle( dashStyle );
						}

						// Set Rounded Cap
						if(_linePen.StartCap != LineCap.Round)
							_linePen.StartCap = LineCap.Round;
						if(_linePen.EndCap != LineCap.Round)
							_linePen.EndCap = LineCap.Round;

						if(tension == 0)
						{
                            // VSTS: 9698 - issue: the line start from X = 0 when GDI overflows (before we expected exception)
                            if (IsLinePointsOverflow(points[pointIndex - 1]) || IsLinePointsOverflow(points[pointIndex]))
                            {
                                this.DrawTruncatedLine(graph, _linePen, points[pointIndex - 1], points[pointIndex]);
                            }
                            else
                            {
                                try
                                {
                                    graph.DrawLine(_linePen, points[pointIndex - 1], points[pointIndex]);
                                }
                                catch (OverflowException)
                                {
                                    this.DrawTruncatedLine(graph, _linePen, points[pointIndex - 1], points[pointIndex]);
                                }
                            }
						}
						else
						{
							graph.DrawCurve(_linePen, points, pointIndex - 1, 1, tension);
						}
					}
				}
			}

			//************************************************************
			// Hot Regions mode used for image maps, tool tips and 
			// hit test function
			//************************************************************
			if( common.ProcessModeRegions )
			{
				int width = pointBorderWidth + 2;

				// Create grapics path object dor the curve
                using (GraphicsPath path = new GraphicsPath())
                {

                    // If line tension is zero - it's a straight line
                    if (this.lineTension == 0)
                    {
                        // Add half line segment prior to the data point
                        if (pointIndex > 0)
                        {
                            PointF first = points[pointIndex - 1];
                            PointF second = points[pointIndex];
                            first.X = (first.X + second.X) / 2f;
                            first.Y = (first.Y + second.Y) / 2f;

                            if (Math.Abs(first.X - second.X) > Math.Abs(first.Y - second.Y))
                            {
                                path.AddLine(first.X, first.Y - width, second.X, second.Y - width);
                                path.AddLine(second.X, second.Y + width, first.X, first.Y + width);
                                path.CloseAllFigures();
                            }
                            else
                            {
                                path.AddLine(first.X - width, first.Y, second.X - width, second.Y);
                                path.AddLine(second.X + width, second.Y, first.X + width, first.Y);
                                path.CloseAllFigures();

                            }
                        }

                        // Add half line segment after the data point
                        if (pointIndex + 1 < points.Length)
                        {
                            PointF first = points[pointIndex];
                            PointF second = points[pointIndex + 1];
                            second.X = (first.X + second.X) / 2f;
                            second.Y = (first.Y + second.Y) / 2f;

                            // Set a marker in the path to separate from the first line segment
                            if (pointIndex > 0)
                            {
                                path.SetMarkers();
                            }

                            if (Math.Abs(first.X - second.X) > Math.Abs(first.Y - second.Y))
                            {
                                path.AddLine(first.X, first.Y - width, second.X, second.Y - width);
                                path.AddLine(second.X, second.Y + width, first.X, first.Y + width);
                                path.CloseAllFigures();
                            }
                            else
                            {
                                path.AddLine(first.X - width, first.Y, second.X - width, second.Y);
                                path.AddLine(second.X + width, second.Y, first.X + width, first.Y);
                                path.CloseAllFigures();
                            }
                        }

                    }
                    else if (pointIndex > 0)
                    {
                        try
                        {
                            path.AddCurve(points, pointIndex - 1, 1, this.lineTension);
                            path.Widen(new Pen(point.Color, pointBorderWidth + 2));
                            path.Flatten();
                        }
                        catch (OutOfMemoryException)
                        {
                            // GraphicsPath.Widen incorrectly throws OutOfMemoryException
                            // catching here and reacting by not widening
                        }
                        catch (ArgumentException)
                        {
                        }
                    }

                    // Path is empty
                    if (path.PointCount == 0)
                    {
                        return;
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

                    common.HotRegionsList.AddHotRegion(path, false, coord, point, series.Name, pointIndex);
                }
			}
		}


        private const long maxGDIRange = 0x800000;
        // VSTS: 9698 - issue: the line start from X = 0 when GDI overflows (before we expected exception)
        private bool IsLinePointsOverflow(PointF point)
        {
            return point.X <= -maxGDIRange || point.X >= maxGDIRange || point.Y <= -maxGDIRange || point.Y >= maxGDIRange;
        }

		/// <summary>
		/// During zooming there are scenarios when the line coordinates are extremly large and
        /// originate outside of the chart pixel boundaries. This cause GDI+ line drawing methods 
        /// to throw stack overflow exceptions.
        /// This method tries to change the coordinates into the chart boundaries and draw the line.
		/// </summary>
        /// <param name="graph">Chart graphics.</param>
		/// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
		/// <param name="pt1">PointF structure that represents the first point to connect.</param>
		/// <param name="pt2">PointF structure that represents the second point to connect.</param>
        private void DrawTruncatedLine(ChartGraphics graph, Pen pen, PointF pt1, PointF pt2)
        {
            PointF adjustedPoint1 = PointF.Empty;
            PointF adjustedPoint2 = PointF.Empty;

            // Check line angle. Intersection with vertical or horizontal lines will be done based on the results
            bool topBottomLine = (Math.Abs(pt2.Y - pt1.Y) > Math.Abs(pt2.X - pt1.X));
            RectangleF rect = new RectangleF(0, 0, graph.Common.ChartPicture.Width, graph.Common.ChartPicture.Height);
            if (topBottomLine)
            {
                // Find the intersection point between the original line and Y = 0 and Y = Height lines
                adjustedPoint1 = rect.Contains(pt1) ? pt1 : GetIntersectionY(pt1, pt2, 0);
                adjustedPoint2 = rect.Contains(pt2) ? pt2 : GetIntersectionY(pt1, pt2, graph.Common.ChartPicture.Height);
            }
            else
            {
                // Find the intersection point between the original line and X = 0 and X = Width lines
                adjustedPoint1 = rect.Contains(pt1) ? pt1 : GetIntersectionX(pt1, pt2, 0);
                adjustedPoint2 = rect.Contains(pt2) ? pt2 : GetIntersectionX(pt1, pt2, graph.Common.ChartPicture.Width);
            }

            // Draw Line
            graph.DrawLine(pen, adjustedPoint1, adjustedPoint2);
        }

        /// <summary>
        /// Gets intersection point coordinates between point line and and horizontal 
        /// line specified by Y coordinate.
        /// </summary>
        /// <param name="firstPoint">First data point.</param>
        /// <param name="secondPoint">Second data point.</param>
        /// <param name="pointY">Y coordinate.</param>
        /// <returns>Intersection point coordinates.</returns>
        internal static PointF GetIntersectionY(PointF firstPoint, PointF secondPoint, float pointY)
        {
            PointF intersectionPoint = new PointF();
            intersectionPoint.Y = pointY;
            intersectionPoint.X = (pointY - firstPoint.Y) *
                (secondPoint.X - firstPoint.X) /
                (secondPoint.Y - firstPoint.Y) +
                firstPoint.X;
            return intersectionPoint;
        }

        /// <summary>
        /// Gets intersection point coordinates between point line and and vertical 
        /// line specified by X coordinate.
        /// </summary>
        /// <param name="firstPoint">First data point.</param>
        /// <param name="secondPoint">Second data point.</param>
        /// <param name="pointX">X coordinate.</param>
        /// <returns>Intersection point coordinates.</returns>
        internal static PointF GetIntersectionX(PointF firstPoint, PointF secondPoint, float pointX)
        {
            PointF intersectionPoint = new PointF();
            intersectionPoint.X = pointX;
            intersectionPoint.Y = (pointX - firstPoint.X) *
                (secondPoint.Y - firstPoint.Y) /
                (secondPoint.X - firstPoint.X) +
                firstPoint.Y;
            return intersectionPoint;
        }

		/// <summary>
		/// Draw chart line.
		/// </summary>
		/// <param name="graph">Graphics object.</param>
		/// <param name="point">Point to draw the line for.</param>
		/// <param name="series">Point series.</param>
		/// <param name="firstPoint">First line point.</param>
		/// <param name="secondPoint">Seconf line point.</param>
		protected void DrawLine(
			ChartGraphics graph, 
			DataPoint point, 
			Series series, 
			PointF firstPoint, 
			PointF secondPoint)
		{
			graph.DrawLineRel( point.Color, point.BorderWidth, point.BorderDashStyle, firstPoint, secondPoint, series.ShadowColor, series.ShadowOffset );
		}

		/// <summary>
		/// Checks if line tension is supported by the chart type.
		/// </summary>
		/// <returns>True if line tension is supported.</returns>
		protected virtual bool IsLineTensionSupported()
		{
			return false;
		}
		
		#endregion

		#region Position helper methods

		/// <summary>
		/// Gets default line tension.
		/// </summary>
		/// <returns>Default line tension.</returns>
		virtual protected float GetDefaultTension()
		{
			return 0f;
		}

		/// <summary>
		/// Gets label position depending on the prev/next point values.
		/// This method will reduce label overlapping with the chart itself (line).
		/// </summary>
		/// <param name="series">Data series.</param>
		/// <param name="pointIndex">Point index.</param>
		/// <returns>Return automaticly detected label position.</returns>
		override protected LabelAlignmentStyles GetAutoLabelPosition(Series series, int pointIndex)
		{
			int pointsCount = series.Points.Count;	// Number of data points
			double previous;						// Y Value from the previous data point
			double next;							// Y Value from the next data point

			// There is only one data point
			if( pointsCount == 1 )
			{
				return LabelAlignmentStyles.Top;
			}

			// Y Value from the current data point
			double current = GetYValue(Common, Area, series, series.Points[pointIndex], pointIndex, 0);

			// The data point is between two data points
			if( pointIndex < pointsCount - 1 && pointIndex > 0 )
			{
				// Y Value from the previous data point
				previous = GetYValue(Common, Area, series, series.Points[pointIndex-1], pointIndex-1, 0);

				// Y Value from the next data point
				next = GetYValue(Common, Area, series, series.Points[pointIndex+1], pointIndex+1, 0);

				// Put the label below lines
				if( previous > current && next > current )
				{
					return LabelAlignmentStyles.Bottom;
				}
			}

			// This is the last data point
			if( pointIndex == pointsCount - 1 )
			{
				// Y Value from the previous data point
				previous = GetYValue(Common, Area, series, series.Points[pointIndex-1], pointIndex-1, 0);

				// Put the label below line
				if( previous > current )
				{
					return LabelAlignmentStyles.Bottom;
				}
			}

			// This is the first data point
			if( pointIndex == 0 )
			{
				// Y Value from the next data point
				next = GetYValue(Common, Area, series, series.Points[pointIndex + 1], pointIndex + 1, 0);

				// Put the label below line
				if( next > current )
				{
					return LabelAlignmentStyles.Bottom;
				}
			}

			return LabelAlignmentStyles.Top;
		}

		/// <summary>
		/// Fills a PointF array of data points absolute pixel positions.
		/// </summary>
		/// <param name="graph">Graphics object.</param>
		/// <param name="series">Point series.</param>
		/// <param name="indexedSeries">Indicate that point index should be used as X value.</param>
		/// <returns>Array of data points position.</returns>
		virtual protected PointF[] GetPointsPosition(ChartGraphics graph, Series series, bool indexedSeries)
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
				// IMPORTANT: Rounding was removed from this part of code because of 
				// very bad drawing in Flash.
				pointPos[index] = new PointF(
                    (float)xPosition * (graph.Common.ChartPicture.Width - 1) / 100F,
                    (float)yPosition * (graph.Common.ChartPicture.Height - 1) / 100F); 

				index++;
			}

			return pointPos;
		}

		#endregion

		#region 3D Drawing and selection methods

		/// <summary>
		/// Draws or perform the hit test for the line chart in 3D.
		/// </summary>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		protected void ProcessLineChartType3D( 
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			Series seriesToDraw )
		{
			
			// Reset graphics fields
			graph.frontLinePen = null;
			graph.frontLinePoint1 = PointF.Empty;
			graph.frontLinePoint2 = PointF.Empty;

			// Get list of series to draw
			List<string> typeSeries = null;
			if( (area.Area3DStyle.IsClustered && this.SideBySideSeries) ||
				this.Stacked)
			{
				// Draw all series of the same chart type
				typeSeries = area.GetSeriesFromChartType(Name);
			}
			else
			{
				// Draw just one chart series
				typeSeries = new List<string>();
				typeSeries.Add(seriesToDraw.Name);
			}
			
			//***************************************************************
			//** Check that data points XValues. Must be sorted or set to 0.
			//***************************************************************
			foreach(string seriesName in typeSeries)
			{
				// Get series object
				Series	currentSeries = common.DataManager.Series[seriesName];

				// Do not check indexed series
				if(currentSeries.IsXValueIndexed)
				{
					continue;
				}

				// Loop through all data points in the series
				bool	allZeros = true;
				int		order = int.MaxValue;	// 0 - Ascending; 1 - Descending;
				double	prevValue = double.NaN;
				foreach(DataPoint dp in currentSeries.Points)
				{
					// Check if X values were set (or all zeros)
					if(allZeros && dp.XValue == 0.0)
					{
						continue;
					}
					allZeros = false;

					// Check X values order
					bool	validOrder = true;
					if(!double.IsNaN(prevValue) && dp.XValue != prevValue)
					{
						// Determine sorting order
						if(order == int.MaxValue)
						{
							order = (dp.XValue > prevValue) ? 0 : 1;	// 0 - Ascending; 1 - Descending;
						}

						// Compare current X value with previous
						if(dp.XValue > prevValue && order == 1)
						{
							validOrder = false;
						}
						if(dp.XValue < prevValue && order == 0)
						{
							validOrder = false;
						}
					}

					// Throw error exception
					if(!validOrder)
					{
                        throw (new InvalidOperationException(SR.Exception3DChartPointsXValuesUnsorted));
					}

					// Remember previous value
					prevValue = dp.XValue;
				}
			}

			//************************************************************
			//** Get order of data points drawing
			//************************************************************
			ArrayList	dataPointDrawingOrder = area.GetDataPointDrawingOrder(
				typeSeries, 
				this, 
				selection, 
				this.COPCoordinatesToCheck, 
				null,
				0,
				false);


			//************************************************************
			//** Get line tension attribute
			//************************************************************
			this.lineTension = GetDefaultTension();
			if(dataPointDrawingOrder.Count > 0)
			{
				Series firstSeries = firstSeries = ((DataPoint3D)dataPointDrawingOrder[0]).dataPoint.series;
				if(IsLineTensionSupported() && firstSeries.IsCustomPropertySet(CustomPropertyName.LineTension))
				{
					this.lineTension = CommonElements.ParseFloat(firstSeries[CustomPropertyName.LineTension]);
				}
			}

			//************************************************************
			//** Check if second ALL points loop is required
			//************************************************************
			allPointsLoopsNumber = GetPointLoopNumber(selection, dataPointDrawingOrder);

			//************************************************************
			//** Loop through all data poins (one or two times)
			//************************************************************
			for(int pointsLoop = 0; pointsLoop < allPointsLoopsNumber; pointsLoop++)
			{
				int		index = 0;
				this.centerPointIndex = int.MaxValue;
				foreach(object obj in dataPointDrawingOrder)
				{
					// Get point & series
					DataPoint3D	pointEx = (DataPoint3D) obj;
					DataPoint	point = pointEx.dataPoint;
					Series		ser = point.series;

					// Set active horizontal/vertical axis
					HAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
					VAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);
					hAxisMin = HAxis.ViewMinimum;
					hAxisMax = HAxis.ViewMaximum;
					vAxisMin = VAxis.ViewMinimum;
					vAxisMax = VAxis.ViewMaximum;
				
					// First point is not drawn as a 3D line
					if(pointEx.index > 1)
					{
						//************************************************************
						//** Get previous data point using the point index in the series
						//************************************************************
						int	pointArrayIndex = index;
						DataPoint3D	prevDataPointEx = ChartGraphics.FindPointByIndex(
							dataPointDrawingOrder, 
							pointEx.index - 1,	
							(this.multiSeries) ? pointEx : null, 
							ref pointArrayIndex);

						//************************************************************
						//** Painting mode
						//************************************************************
						GraphicsPath	rectPath = null;

						// Get Y values of the current and previous data points
						double	yValue = GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, 0);
						double	yValuePrev = GetYValue(common, area, ser, prevDataPointEx.dataPoint, prevDataPointEx.index - 1, 0);
						double	xValue = (pointEx.indexedSeries) ? pointEx.index : pointEx.dataPoint.XValue;
						double	xValuePrev = (prevDataPointEx.indexedSeries) ? prevDataPointEx.index : prevDataPointEx.dataPoint.XValue;

						// Axes are logarithmic
						yValue = VAxis.GetLogValue( yValue );
						yValuePrev = VAxis.GetLogValue( yValuePrev );

						xValue = HAxis.GetLogValue( xValue );
						xValuePrev = HAxis.GetLogValue( xValuePrev );
					
						//************************************************************
						//** Draw line
						//************************************************************
						DataPoint3D		pointAttr = (prevDataPointEx.dataPoint.IsEmpty) ? prevDataPointEx : pointEx;
						if(pointAttr.dataPoint.Color != Color.Empty)
						{
							// Detect if we need to get graphical path of drawn object
							DrawingOperationTypes	drawingOperationType = DrawingOperationTypes.DrawElement;
				
							if( common.ProcessModeRegions )
							{
								drawingOperationType |= DrawingOperationTypes.CalcElementPath;
							}

							// Check if point markers lines should be drawn
							this.showPointLines = false;
							if(pointAttr.dataPoint.IsCustomPropertySet(CustomPropertyName.ShowMarkerLines))
							{
								if(String.Compare(pointAttr.dataPoint[CustomPropertyName.ShowMarkerLines], "TRUE", StringComparison.OrdinalIgnoreCase) == 0)
								{
									this.showPointLines = true;
								}
							}
							else
							{
								if(pointAttr.dataPoint.series.IsCustomPropertySet(CustomPropertyName.ShowMarkerLines))
								{
                                    if (String.Compare(pointAttr.dataPoint.series[CustomPropertyName.ShowMarkerLines], "TRUE", StringComparison.OrdinalIgnoreCase) == 0)
									{
										this.showPointLines = true;
									}
								}
							}

							// Start Svg Selection mode
							graph.StartHotRegion( point );

							// Draw line surface
							area.IterationCounter = 0;
							rectPath = Draw3DSurface(
								area,
								graph,
								area.matrix3D, 
								area.Area3DStyle.LightStyle,
								prevDataPointEx,
								pointAttr.zPosition, 
								pointAttr.depth, 
								dataPointDrawingOrder, 
								index, 
								pointsLoop,
								lineTension,
								drawingOperationType,
								0f, 0f,
								new PointF(float.NaN, float.NaN), 
								new PointF(float.NaN, float.NaN),
								false);
							
							// End Svg Selection mode
							graph.EndHotRegion( );
						}

						//************************************************************
						// Hot Regions mode used for image maps, tool tips and 
						// hit test function
						//************************************************************
						if( common.ProcessModeRegions && rectPath != null)
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
			
					// Increase point index
					++index;
				}
			}
		}

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
		protected virtual GraphicsPath Draw3DSurface( 
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

			// Draw point using 2 points
			return graph.Draw3DSurface( 
				area,
				matrix,
				lightStyle,
				SurfaceNames.Top,
				positionZ, 
				depth, 
				color, 
				pointAttr.dataPoint.BorderColor, 
				pointAttr.dataPoint.BorderWidth, 
				dashStyle, 
				firstPoint,
				secondPoint, 
				points,
				pointIndex,
				tension,
				operationType,
				LineSegmentType.Single,
				(this.showPointLines) ? true : false,
				false,
                area.ReverseSeriesOrder,
				this.multiSeries,
				0, 
				true);
		}

		/// <summary>
		/// Gets index of center point.
		/// </summary>
		/// <param name="points">Points list.</param>
		/// <returns>Index of center point or int.MaxValue.</returns>
		protected int GetCenterPointIndex(ArrayList points)
		{
			for(int pointIndex = 1; pointIndex < points.Count; pointIndex++)
			{
				DataPoint3D	firstPoint = (DataPoint3D)points[pointIndex - 1];
				DataPoint3D	secondPoint = (DataPoint3D)points[pointIndex];
				if(Math.Abs(secondPoint.index - firstPoint.index) != 1)
				{
					return pointIndex - 1;
				}
			}
			return int.MaxValue;
		}

		/// <summary>
		/// Returns how many loops through all data points is required (1 or 2)
		/// </summary>
		/// <param name="selection">Selection indicator.</param>
		/// <param name="pointsArray">Points array list.</param>
		/// <returns>Number of loops (1 or 2).</returns>
		virtual protected int GetPointLoopNumber(bool selection, ArrayList pointsArray)
		{
			return 1;
		}

		/// <summary>
		/// Clips the top (left and right) points of the segment to plotting area.
		/// Used in area and range charts.
		/// </summary>
		/// <param name="resultPath">Segment area path.</param>
		/// <param name="firstPoint">First data point.</param>
		/// <param name="secondPoint">Second data point.</param>
		/// <param name="reversed">Points are in reversed order.</param>
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
		/// <param name="surfaceSegmentType">Define surface segment type if it consists of several segments.</param>
		/// <param name="topDarkening">Darkenning scale for top surface. 0 - None.</param>
		/// <param name="bottomDarkening">Darkenning scale for bottom surface. 0 - None.</param>
		/// <returns>Returns element shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
		protected bool ClipTopPoints(
			GraphicsPath resultPath,
			ref DataPoint3D firstPoint, 
			ref DataPoint3D secondPoint, 
			bool reversed,
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
			LineSegmentType surfaceSegmentType,
			float topDarkening,
			float bottomDarkening)
		{
			// Do not allow recursion to go too deep
			++area.IterationCounter;
			if(area.IterationCounter > 20)
			{
				area.IterationCounter = 0;
				return true;
			}

			//****************************************************************
			//** Check point values
			//****************************************************************
			if( double.IsNaN(firstPoint.xPosition) || 
				double.IsNaN(firstPoint.yPosition) ||
				double.IsNaN(secondPoint.xPosition) ||
				double.IsNaN(secondPoint.yPosition) )
			{
				return true;
			}

			//****************************************************************
			//** Round plot are position and point coordinates
			//****************************************************************
			int decimals = 3;
			decimal plotAreaPositionX = Math.Round((decimal)area.PlotAreaPosition.X, decimals);
			decimal plotAreaPositionY = Math.Round((decimal)area.PlotAreaPosition.Y, decimals);
			decimal plotAreaPositionRight = Math.Round((decimal)area.PlotAreaPosition.Right, decimals);
			decimal plotAreaPositionBottom = Math.Round((decimal)area.PlotAreaPosition.Bottom, decimals);

			// Make area a little bit bigger
			plotAreaPositionX -= 0.001M;
			plotAreaPositionY -= 0.001M;
			plotAreaPositionRight += 0.001M;
			plotAreaPositionBottom += 0.001M;

			// Round top points coordinates
			firstPoint.xPosition = Math.Round(firstPoint.xPosition, decimals);
			firstPoint.yPosition = Math.Round(firstPoint.yPosition, decimals);
			secondPoint.xPosition = Math.Round(secondPoint.xPosition, decimals);
			secondPoint.yPosition = Math.Round(secondPoint.yPosition, decimals);


			//****************************************************************
			//** Clip area data points inside the plotting area
			//****************************************************************

			// Chech data points X values
			if((decimal)firstPoint.xPosition < plotAreaPositionX || 
				(decimal)firstPoint.xPosition > plotAreaPositionRight ||
				(decimal)secondPoint.xPosition < plotAreaPositionX || 
				(decimal)secondPoint.xPosition > plotAreaPositionRight )
			{
				// Check if surface completly out of the plot area
				if((decimal)firstPoint.xPosition < plotAreaPositionX &&
					(decimal)secondPoint.xPosition < plotAreaPositionX)
				{
					return true;
				}
				// Check if surface completly out of the plot area
				if((decimal)firstPoint.xPosition > plotAreaPositionRight &&
					(decimal)secondPoint.xPosition > plotAreaPositionRight)
				{
					return true;
				}

				// Only part of the surface is outside - fix X value and adjust Y value
				if((decimal)firstPoint.xPosition < plotAreaPositionX)
				{
					firstPoint.yPosition = ((double)plotAreaPositionX - secondPoint.xPosition) /
						(firstPoint.xPosition - secondPoint.xPosition) *
						(firstPoint.yPosition - secondPoint.yPosition) +
						secondPoint.yPosition;
					firstPoint.xPosition = (double)plotAreaPositionX;
				}
				else if((decimal)firstPoint.xPosition > plotAreaPositionRight)
				{
					firstPoint.yPosition = ((double)plotAreaPositionRight - secondPoint.xPosition) /
						(firstPoint.xPosition - secondPoint.xPosition) *
						(firstPoint.yPosition - secondPoint.yPosition) +
						secondPoint.yPosition;
					firstPoint.xPosition = (double)plotAreaPositionRight;
				}
				if((decimal)secondPoint.xPosition < plotAreaPositionX)
				{
					secondPoint.yPosition = ((double)plotAreaPositionX - secondPoint.xPosition) /
						(firstPoint.xPosition - secondPoint.xPosition) *
						(firstPoint.yPosition - secondPoint.yPosition) +
						secondPoint.yPosition;
					secondPoint.xPosition = (double)plotAreaPositionX;
				}
				else if((decimal)secondPoint.xPosition > plotAreaPositionRight)
				{
					secondPoint.yPosition = ((double)plotAreaPositionRight - secondPoint.xPosition) /
						(firstPoint.xPosition - secondPoint.xPosition) *
						(firstPoint.yPosition - secondPoint.yPosition) +
						secondPoint.yPosition;
					secondPoint.xPosition = (double)plotAreaPositionRight;
				}
			}

			// Chech data points Y values
			if((decimal)firstPoint.yPosition < plotAreaPositionY || 
				(decimal)firstPoint.yPosition > plotAreaPositionBottom ||
				(decimal)secondPoint.yPosition < plotAreaPositionY || 
				(decimal)secondPoint.yPosition > plotAreaPositionBottom )
			{
				// Remember previous y positions
				double prevFirstPointY = firstPoint.yPosition;
				double prevSecondPointY = secondPoint.yPosition;

				// Check if whole line is outside plotting region
				bool	surfaceCompletlyOutside = false;
				bool	outsideBottom = false;
				if((decimal)firstPoint.yPosition < plotAreaPositionY && 
					(decimal)secondPoint.yPosition < plotAreaPositionY)
				{
					surfaceCompletlyOutside = true;
					firstPoint.yPosition = (double)plotAreaPositionY;
					secondPoint.yPosition = (double)plotAreaPositionY;
				}
				if((decimal)firstPoint.yPosition > plotAreaPositionBottom && 
					(decimal)secondPoint.yPosition > plotAreaPositionBottom)
				{
					surfaceCompletlyOutside = true;
					outsideBottom = true;
					firstPoint.yPosition = (double)plotAreaPositionBottom;
					secondPoint.yPosition = (double)plotAreaPositionBottom;
				}

				// Draw just one surface
				if(surfaceCompletlyOutside)
				{
					resultPath =  Draw3DSurface( firstPoint, secondPoint, reversed,
						area, graph, matrix, lightStyle, prevDataPointEx,
						positionZ, depth, points, pointIndex, pointLoopIndex,
						tension, operationType, surfaceSegmentType, 
						0.5f, 0f,
						new PointF(float.NaN, float.NaN),
						new PointF(float.NaN, float.NaN),
						outsideBottom,
						false, true);

					// Restore previous y positions
					firstPoint.yPosition = prevFirstPointY;
					secondPoint.yPosition = prevSecondPointY;

					return true;
				}

				// Get intersection point
				DataPoint3D	intersectionPoint = new DataPoint3D();
				intersectionPoint.yPosition = (double)plotAreaPositionY;
				if((decimal)firstPoint.yPosition > plotAreaPositionBottom ||
					(decimal)secondPoint.yPosition > plotAreaPositionBottom )
				{
					intersectionPoint.yPosition = (double)plotAreaPositionBottom;
				}
				intersectionPoint.xPosition = (intersectionPoint.yPosition - secondPoint.yPosition) *
					(firstPoint.xPosition - secondPoint.xPosition) / 
					(firstPoint.yPosition - secondPoint.yPosition) + 
					secondPoint.xPosition;

				if(double.IsNaN(intersectionPoint.xPosition) ||
					double.IsInfinity(intersectionPoint.xPosition) ||
					double.IsNaN(intersectionPoint.yPosition) ||
					double.IsInfinity(intersectionPoint.yPosition) )
				{
					return true;
				}

				// Check if there are 2 intersection points (3 segments)
				int		segmentNumber = 2;
				DataPoint3D	intersectionPoint2 = null;
				if( ((decimal)firstPoint.yPosition < plotAreaPositionY &&
					(decimal)secondPoint.yPosition > plotAreaPositionBottom) ||
					((decimal)firstPoint.yPosition > plotAreaPositionBottom &&
					(decimal)secondPoint.yPosition < plotAreaPositionY))
				{
					segmentNumber = 3;
					intersectionPoint2 = new DataPoint3D();
					if((decimal)intersectionPoint.yPosition == plotAreaPositionY)
					{
						intersectionPoint2.yPosition = (double)plotAreaPositionBottom;
					}
					else
					{
						intersectionPoint2.yPosition = (double)plotAreaPositionY;
					}
					intersectionPoint2.xPosition = (intersectionPoint2.yPosition - secondPoint.yPosition) *
						(firstPoint.xPosition - secondPoint.xPosition) / 
						(firstPoint.yPosition - secondPoint.yPosition) + 
						secondPoint.xPosition;

					if(double.IsNaN(intersectionPoint2.xPosition) ||
						double.IsInfinity(intersectionPoint2.xPosition) ||
						double.IsNaN(intersectionPoint2.yPosition) ||
						double.IsInfinity(intersectionPoint2.yPosition) )
					{
						return true;
					}

					// Switch intersection points
					if((decimal)firstPoint.yPosition > plotAreaPositionBottom)
					{
						DataPoint3D tempPoint = new DataPoint3D();
						tempPoint.xPosition = intersectionPoint.xPosition;
						tempPoint.yPosition = intersectionPoint.yPosition;
						intersectionPoint.xPosition = intersectionPoint2.xPosition;
						intersectionPoint.yPosition = intersectionPoint2.yPosition;
						intersectionPoint2.xPosition = tempPoint.xPosition;
						intersectionPoint2.yPosition = tempPoint.yPosition;
					}
				}


				// Adjust points Y values
				bool	firstSegmentVisible = true;
				bool	firstSegmentOutsideBottom = false;
				bool	secondSegmentOutsideBottom = false;
				if((decimal)firstPoint.yPosition < plotAreaPositionY)
				{
					firstSegmentVisible = false;
					firstPoint.yPosition = (double)plotAreaPositionY;
				}
				else if((decimal)firstPoint.yPosition > plotAreaPositionBottom)
				{
					firstSegmentOutsideBottom = true;
					firstSegmentVisible = false;
					firstPoint.yPosition = (double)plotAreaPositionBottom;
				}
				if((decimal)secondPoint.yPosition < plotAreaPositionY)
				{
					secondPoint.yPosition = (double)plotAreaPositionY;
				}
				else if((decimal)secondPoint.yPosition > plotAreaPositionBottom)
				{
					secondSegmentOutsideBottom = true;
					secondPoint.yPosition = (double)plotAreaPositionBottom;
				}

				// Draw surfaces in 2 or 3 segments
				for(int segmentIndex = 0; segmentIndex < 3; segmentIndex++)
				{
					GraphicsPath segmentPath = null;
					if(segmentIndex == 0 && !reversed ||
						segmentIndex == 2 && reversed)
					{
						// Draw first segment
						if(intersectionPoint2 == null)
						{
							intersectionPoint2 = intersectionPoint;
						}
						intersectionPoint2.dataPoint = secondPoint.dataPoint;
						intersectionPoint2.index = secondPoint.index;
						intersectionPoint2.xCenterVal = secondPoint.xCenterVal;

						segmentPath =  Draw3DSurface( firstPoint, intersectionPoint2, reversed,
							area, graph, matrix, lightStyle, prevDataPointEx,
							positionZ, depth, points, pointIndex, pointLoopIndex,
							tension, operationType, 
							(surfaceSegmentType == LineSegmentType.Middle) ? LineSegmentType.Middle : LineSegmentType.First,
							(firstSegmentVisible && segmentNumber != 3) ? 0f : 0.5f, 0f,
							new PointF(float.NaN, float.NaN), 
							new PointF((float)intersectionPoint2.xPosition, float.NaN),
							firstSegmentOutsideBottom,
							false, true);
							
					}

					if(segmentIndex == 1 && intersectionPoint2 != null && segmentNumber == 3)
					{
						// Draw middle segment
						intersectionPoint2.dataPoint = secondPoint.dataPoint;
						intersectionPoint2.index = secondPoint.index;
						intersectionPoint2.xCenterVal = secondPoint.xCenterVal;

						intersectionPoint.xCenterVal = firstPoint.xCenterVal;
						intersectionPoint.index = firstPoint.index;
						intersectionPoint.dataPoint = firstPoint.dataPoint;

						segmentPath =  Draw3DSurface( intersectionPoint, intersectionPoint2, reversed,
							area, graph, matrix, lightStyle, prevDataPointEx,
							positionZ, depth, points, pointIndex, pointLoopIndex,
							tension, operationType, LineSegmentType.Middle,
							topDarkening, bottomDarkening,
							new PointF((float)intersectionPoint.xPosition, float.NaN),
							new PointF((float)intersectionPoint2.xPosition, float.NaN),
							false,
							false, true);
							
					}

					if(segmentIndex == 2 && !reversed ||
						segmentIndex == 0 && reversed)
					{
						// Draw second segment
						intersectionPoint.dataPoint = firstPoint.dataPoint;
						intersectionPoint.index = firstPoint.index;
						intersectionPoint.xCenterVal = firstPoint.xCenterVal;

						segmentPath =  Draw3DSurface( intersectionPoint, secondPoint, reversed,
							area, graph, matrix, lightStyle, prevDataPointEx,
							positionZ, depth, points, pointIndex, pointLoopIndex,
							tension, operationType, 
							(surfaceSegmentType == LineSegmentType.Middle) ? LineSegmentType.Middle : LineSegmentType.Last,
							(!firstSegmentVisible && segmentNumber != 3) ? 0f : 0.5f, 0f,
							new PointF((float)intersectionPoint.xPosition, float.NaN),
							new PointF(float.NaN, float.NaN),
							secondSegmentOutsideBottom,
							false, true);
						
					}

					// Add segment path
					if(resultPath != null && segmentPath != null && segmentPath.PointCount > 0)
					{
						resultPath.AddPath(segmentPath, true);
					}
				}

				// Restore previous y positions
				firstPoint.yPosition = prevFirstPointY;
				secondPoint.yPosition = prevSecondPointY;

				return true;
			}
			return false;
		}

		/// <summary>
		/// Clips the bottom (left and right) points of the segment to plotting area.
		/// Used in area and range charts.
		/// </summary>
		/// <param name="resultPath"></param>
		/// <param name="firstPoint">First data point.</param>
		/// <param name="secondPoint">Second data point.</param>
		/// <param name="thirdPoint">Coordinates of the bottom left point.</param>
		/// <param name="fourthPoint">Coordinates of the bottom right point.</param>
		/// <param name="reversed">Points are in reversed order.</param>
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
		/// <param name="surfaceSegmentType">Define surface segment type if it consists of several segments.</param>
		/// <param name="topDarkening">Darkenning scale for top surface. 0 - None.</param>
		/// <param name="bottomDarkening">Darkenning scale for bottom surface. 0 - None.</param>
		/// <returns>Returns element shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
		protected bool ClipBottomPoints(
			GraphicsPath resultPath,
			ref DataPoint3D firstPoint, 
			ref DataPoint3D secondPoint, 
			ref PointF thirdPoint,
			ref PointF fourthPoint,
			bool reversed,
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
			LineSegmentType surfaceSegmentType,
			float topDarkening,
			float bottomDarkening)
		{
			// Do not allow recursion to go too deep
			++area.IterationCounter;
			if(area.IterationCounter > 20)
			{
				area.IterationCounter = 0;
				return true;
			}

			//****************************************************************
			//** Round plot are position and point coordinates
			//****************************************************************
			int decimals = 3;
			decimal plotAreaPositionX = Math.Round((decimal)area.PlotAreaPosition.X, decimals);
			decimal plotAreaPositionY = Math.Round((decimal)area.PlotAreaPosition.Y, decimals);
			decimal plotAreaPositionRight = Math.Round((decimal)area.PlotAreaPosition.Right, decimals);
			decimal plotAreaPositionBottom = Math.Round((decimal)area.PlotAreaPosition.Bottom, decimals);

			// Make area a little bit bigger
			plotAreaPositionX -= 0.001M;
			plotAreaPositionY -= 0.001M;
			plotAreaPositionRight += 0.001M;
			plotAreaPositionBottom += 0.001M;



			// Round top points coordinates
			firstPoint.xPosition = Math.Round(firstPoint.xPosition, decimals);
			firstPoint.yPosition = Math.Round(firstPoint.yPosition, decimals);
			secondPoint.xPosition = Math.Round(secondPoint.xPosition, decimals);
			secondPoint.yPosition = Math.Round(secondPoint.yPosition, decimals);

			thirdPoint.X = (float)Math.Round(thirdPoint.X, decimals);
			thirdPoint.Y = (float)Math.Round(thirdPoint.Y, decimals);

			fourthPoint.X = (float)Math.Round(fourthPoint.X, decimals);
			fourthPoint.Y = (float)Math.Round(fourthPoint.Y, decimals);

			//****************************************************************
			//** Clip area data points inside the plotting area
			//****************************************************************

			// Chech data points Y values
			if((decimal)thirdPoint.Y < plotAreaPositionY || 
				(decimal)thirdPoint.Y > plotAreaPositionBottom ||
				(decimal)fourthPoint.Y < plotAreaPositionY || 
				(decimal)fourthPoint.Y > plotAreaPositionBottom )
			{
				// Remember previous y positions
				PointF prevThirdPoint = new PointF(thirdPoint.X, thirdPoint.Y);
				PointF prevFourthPoint = new PointF(fourthPoint.X, fourthPoint.Y);

				// Check if whole line is outside plotting region
				bool	surfaceCompletlyOutside = false;
				bool	outsideTop = false;
				if((decimal)thirdPoint.Y < plotAreaPositionY && 
					(decimal)fourthPoint.Y < plotAreaPositionY)
				{
					outsideTop = true;
					surfaceCompletlyOutside = true;
					thirdPoint.Y = area.PlotAreaPosition.Y;
					fourthPoint.Y = area.PlotAreaPosition.Y;
				}
				if((decimal)thirdPoint.Y > plotAreaPositionBottom && 
					(decimal)fourthPoint.Y > plotAreaPositionBottom)
				{
					surfaceCompletlyOutside = true;
					thirdPoint.Y = area.PlotAreaPosition.Bottom;
					fourthPoint.Y = area.PlotAreaPosition.Bottom;
				}

				// Draw just one surface
				if(surfaceCompletlyOutside)
				{
					resultPath =  Draw3DSurface( firstPoint, secondPoint, reversed,
						area, graph, matrix, lightStyle, prevDataPointEx,
						positionZ, depth, points, pointIndex, pointLoopIndex,
						tension, operationType, surfaceSegmentType, 
						topDarkening, 0.5f,
						new PointF(thirdPoint.X, thirdPoint.Y), 
						new PointF(fourthPoint.X, fourthPoint.Y),
						outsideTop,
						false, false);

					// Restore previous x\y positions
					thirdPoint = new PointF(prevThirdPoint.X, prevThirdPoint.Y);
					fourthPoint = new PointF(prevFourthPoint.X, prevFourthPoint.Y);

					return true;
				}

				// Get intersection point
				DataPoint3D	intersectionPoint = new DataPoint3D();
				bool		firstIntersectionOnBottom = false;
				intersectionPoint.yPosition = (double)plotAreaPositionY;
				if((decimal)thirdPoint.Y > plotAreaPositionBottom ||
					(decimal)fourthPoint.Y > plotAreaPositionBottom )
				{
					intersectionPoint.yPosition = (double)area.PlotAreaPosition.Bottom;
					firstIntersectionOnBottom = true;
				}
				intersectionPoint.xPosition = (intersectionPoint.yPosition - fourthPoint.Y) *
					(thirdPoint.X - fourthPoint.X) / 
					(thirdPoint.Y - fourthPoint.Y) + 
					fourthPoint.X;
                				
				// Intersection point must be between first and second points
				intersectionPoint.yPosition = (intersectionPoint.xPosition - secondPoint.xPosition) /
					(firstPoint.xPosition - secondPoint.xPosition) * 
					(firstPoint.yPosition - secondPoint.yPosition) + 
					secondPoint.yPosition;

				if(double.IsNaN(intersectionPoint.xPosition) ||
					double.IsInfinity(intersectionPoint.xPosition) ||
					double.IsNaN(intersectionPoint.yPosition) ||
					double.IsInfinity(intersectionPoint.yPosition) )
				{
					return true;
				}

				// Check if there are 2 intersection points (3 segments)
				int		segmentNumber = 2;
				DataPoint3D	intersectionPoint2 = null;
				bool	switchPoints = false;
				if( ((decimal)thirdPoint.Y < plotAreaPositionY &&
					(decimal)fourthPoint.Y > plotAreaPositionBottom) ||
					((decimal)thirdPoint.Y > plotAreaPositionBottom &&
					(decimal)fourthPoint.Y < plotAreaPositionY))
				{
					segmentNumber = 3;
					intersectionPoint2 = new DataPoint3D();
					if(!firstIntersectionOnBottom)
					{
						intersectionPoint2.yPosition = (double)area.PlotAreaPosition.Bottom;
					}
					else
					{
						intersectionPoint2.yPosition = (double)area.PlotAreaPosition.Y;
					}
					intersectionPoint2.xPosition = (intersectionPoint2.yPosition - fourthPoint.Y) *
						(thirdPoint.X - fourthPoint.X) / 
						(thirdPoint.Y - fourthPoint.Y) + 
						fourthPoint.X;

					intersectionPoint2.yPosition = (intersectionPoint2.xPosition - secondPoint.xPosition) /
						(firstPoint.xPosition - secondPoint.xPosition) * 
						(firstPoint.yPosition - secondPoint.yPosition) + 
						secondPoint.yPosition;

					if(double.IsNaN(intersectionPoint2.xPosition) ||
						double.IsInfinity(intersectionPoint2.xPosition) ||
						double.IsNaN(intersectionPoint2.yPosition) ||
						double.IsInfinity(intersectionPoint2.yPosition) )
					{
						return true;
					}


					// Switch intersection points
					//if(firstPoint.yPosition > plotAreaPositionBottom)
					if((decimal)thirdPoint.Y > plotAreaPositionBottom)
					{
						switchPoints = true;
						/*
						DataPoint3D tempPoint = new DataPoint3D();
						tempPoint.xPosition = intersectionPoint.xPosition;
						tempPoint.yPosition = intersectionPoint.yPosition;
						intersectionPoint.xPosition = intersectionPoint2.xPosition;
						intersectionPoint.yPosition = intersectionPoint2.yPosition;
						intersectionPoint2.xPosition = tempPoint.xPosition;
						intersectionPoint2.yPosition = tempPoint.yPosition;
						*/
					}
				}


				// Adjust points Y values
				bool	firstSegmentVisible = true;
				float	bottomDarken = bottomDarkening;
				bool	firstSegmentOutsideTop = false;
				bool	secondSegmentOutsideTop = false;
				if((decimal)thirdPoint.Y < plotAreaPositionY)
				{
					firstSegmentOutsideTop = true;
					firstSegmentVisible = false;
					thirdPoint.Y = area.PlotAreaPosition.Y;
					bottomDarken = 0.5f;
				}
				else if((decimal)thirdPoint.Y > plotAreaPositionBottom)
				{
					firstSegmentVisible = false;
					thirdPoint.Y = area.PlotAreaPosition.Bottom;
					if(firstPoint.yPosition >= thirdPoint.Y)
					{
						bottomDarken = 0.5f;
					}
				}
				if((decimal)fourthPoint.Y < plotAreaPositionY)
				{
					secondSegmentOutsideTop = true;
					fourthPoint.Y = area.PlotAreaPosition.Y;
					bottomDarken = 0.5f;
				}
				else if((decimal)fourthPoint.Y > plotAreaPositionBottom)
				{
					fourthPoint.Y = area.PlotAreaPosition.Bottom;
					if(fourthPoint.Y <= secondPoint.yPosition)
					{
						bottomDarken = 0.5f;
					}
				}

				// Draw surfaces in 2 or 3 segments
				for(int segmentIndex = 0; segmentIndex < 3; segmentIndex++)
				{
					GraphicsPath segmentPath = null;
					if(segmentIndex == 0 && !reversed ||
						segmentIndex == 2 && reversed)
					{
						// Draw first segment
						if(intersectionPoint2 == null)
						{
							intersectionPoint2 = intersectionPoint;
						}

						if(switchPoints)
						{
							DataPoint3D tempPoint = new DataPoint3D();
							tempPoint.xPosition = intersectionPoint.xPosition;
							tempPoint.yPosition = intersectionPoint.yPosition;
							intersectionPoint.xPosition = intersectionPoint2.xPosition;
							intersectionPoint.yPosition = intersectionPoint2.yPosition;
							intersectionPoint2.xPosition = tempPoint.xPosition;
							intersectionPoint2.yPosition = tempPoint.yPosition;
						}


						intersectionPoint2.dataPoint = secondPoint.dataPoint;
						intersectionPoint2.index = secondPoint.index;
						intersectionPoint2.xCenterVal = secondPoint.xCenterVal;

						segmentPath =  Draw3DSurface( firstPoint, intersectionPoint2, reversed,
							area, graph, matrix, lightStyle, prevDataPointEx,
							positionZ, depth, points, pointIndex, pointLoopIndex,
							tension, operationType, 
							(surfaceSegmentType == LineSegmentType.Middle) ? LineSegmentType.Middle : LineSegmentType.First,
							topDarkening, bottomDarken,
							new PointF(float.NaN, thirdPoint.Y),
							new PointF((float)intersectionPoint2.xPosition, (!firstSegmentVisible || segmentNumber == 3) ? thirdPoint.Y : fourthPoint.Y),
							firstSegmentOutsideTop,
							false, false);

						if(switchPoints)
						{
							DataPoint3D tempPoint = new DataPoint3D();
							tempPoint.xPosition = intersectionPoint.xPosition;
							tempPoint.yPosition = intersectionPoint.yPosition;
							intersectionPoint.xPosition = intersectionPoint2.xPosition;
							intersectionPoint.yPosition = intersectionPoint2.yPosition;
							intersectionPoint2.xPosition = tempPoint.xPosition;
							intersectionPoint2.yPosition = tempPoint.yPosition;
						}

					}

					if(segmentIndex == 1 && intersectionPoint2 != null && segmentNumber == 3)
					{
						if(!switchPoints)
						{
							DataPoint3D tempPoint = new DataPoint3D();
							tempPoint.xPosition = intersectionPoint.xPosition;
							tempPoint.yPosition = intersectionPoint.yPosition;
							intersectionPoint.xPosition = intersectionPoint2.xPosition;
							intersectionPoint.yPosition = intersectionPoint2.yPosition;
							intersectionPoint2.xPosition = tempPoint.xPosition;
							intersectionPoint2.yPosition = tempPoint.yPosition;
						}

						// Draw middle segment
						intersectionPoint2.dataPoint = secondPoint.dataPoint;
						intersectionPoint2.index = secondPoint.index;
						intersectionPoint2.xCenterVal = secondPoint.xCenterVal;

						intersectionPoint.xCenterVal = firstPoint.xCenterVal;
						intersectionPoint.index = firstPoint.index;
						intersectionPoint.dataPoint = firstPoint.dataPoint;
					
						segmentPath =  Draw3DSurface( intersectionPoint, intersectionPoint2, reversed,
							area, graph, matrix, lightStyle, prevDataPointEx,
							positionZ, depth, points, pointIndex, pointLoopIndex,
							tension, operationType, LineSegmentType.Middle,
							topDarkening, bottomDarkening,
							new PointF((float)intersectionPoint.xPosition, thirdPoint.Y),
							new PointF((float)intersectionPoint2.xPosition, fourthPoint.Y),
							false,
							false, false);

						if(!switchPoints)
						{
							DataPoint3D tempPoint = new DataPoint3D();
							tempPoint.xPosition = intersectionPoint.xPosition;
							tempPoint.yPosition = intersectionPoint.yPosition;
							intersectionPoint.xPosition = intersectionPoint2.xPosition;
							intersectionPoint.yPosition = intersectionPoint2.yPosition;
							intersectionPoint2.xPosition = tempPoint.xPosition;
							intersectionPoint2.yPosition = tempPoint.yPosition;
						}

					}

					if(segmentIndex == 2 && !reversed ||
						segmentIndex == 0 && reversed)
					{
						if(switchPoints)
						{
							DataPoint3D tempPoint = new DataPoint3D();
							tempPoint.xPosition = intersectionPoint.xPosition;
							tempPoint.yPosition = intersectionPoint.yPosition;
							intersectionPoint.xPosition = intersectionPoint2.xPosition;
							intersectionPoint.yPosition = intersectionPoint2.yPosition;
							intersectionPoint2.xPosition = tempPoint.xPosition;
							intersectionPoint2.yPosition = tempPoint.yPosition;
						}

						// Draw second segment
						intersectionPoint.dataPoint = firstPoint.dataPoint;
						intersectionPoint.index = firstPoint.index;
						intersectionPoint.xCenterVal = firstPoint.xCenterVal;

						float thirdPointNewY = (!firstSegmentVisible || segmentNumber == 3) ? thirdPoint.Y : fourthPoint.Y;
						if(segmentNumber == 3)
						{
							thirdPointNewY = (secondSegmentOutsideTop) ? thirdPoint.Y : fourthPoint.Y;
						}

						segmentPath =  Draw3DSurface( intersectionPoint, secondPoint, reversed,
							area, graph, matrix, lightStyle, prevDataPointEx,
							positionZ, depth, points, pointIndex, pointLoopIndex,
							tension, operationType, 
							(surfaceSegmentType == LineSegmentType.Middle) ? LineSegmentType.Middle : LineSegmentType.Last,
							topDarkening, bottomDarken,
							new PointF((float)intersectionPoint.xPosition, thirdPointNewY),
							new PointF(float.NaN, fourthPoint.Y),
							secondSegmentOutsideTop,
							false, false);

						if(switchPoints)
						{
							DataPoint3D tempPoint = new DataPoint3D();
							tempPoint.xPosition = intersectionPoint.xPosition;
							tempPoint.yPosition = intersectionPoint.yPosition;
							intersectionPoint.xPosition = intersectionPoint2.xPosition;
							intersectionPoint.yPosition = intersectionPoint2.yPosition;
							intersectionPoint2.xPosition = tempPoint.xPosition;
							intersectionPoint2.yPosition = tempPoint.yPosition;
						}

					}

					// Add segment path
					if(resultPath != null && segmentPath != null && segmentPath.PointCount > 0)
					{
						resultPath.AddPath(segmentPath, true);
					}
				}

				// Restore previous x\y positions
				thirdPoint = new PointF(prevThirdPoint.X, prevThirdPoint.Y);
				fourthPoint = new PointF(prevFourthPoint.X, prevFourthPoint.Y);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Draws a 3D surface connecting the two specified points in 2D space.
		/// Used to draw Line based charts.
		/// </summary>
		/// <param name="firstPoint">First data point.</param>
		/// <param name="secondPoint">Second data point.</param>
		/// <param name="reversed">Points are in reversed order.</param>
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
		/// <param name="surfaceSegmentType">Define surface segment type if it consists of several segments.</param>
		/// <param name="topDarkening">Darkenning scale for top surface. 0 - None.</param>
		/// <param name="bottomDarkening">Darkenning scale for bottom surface. 0 - None.</param>
		/// <param name="thirdPointPosition">Position where the third point is actually located or float.NaN if same as in "firstPoint".</param>
		/// <param name="fourthPointPosition">Position where the fourth point is actually located or float.NaN if same as in "secondPoint".</param>
		/// <param name="clippedSegment">Indicates that drawn segment is 3D clipped. Only top/bottom should be drawn.</param>
		/// <param name="clipOnTop">Indicates that top segment line should be clipped to the pkot area.</param>
		/// <param name="clipOnBottom">Indicates that bottom segment line should be clipped to the pkot area.</param>
		/// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
		protected virtual GraphicsPath Draw3DSurface( 
			DataPoint3D firstPoint, 
			DataPoint3D secondPoint, 
			bool reversed,
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
			LineSegmentType surfaceSegmentType,
			float topDarkening,
			float bottomDarkening,
			PointF thirdPointPosition,
			PointF fourthPointPosition,
			bool clippedSegment,
			bool clipOnTop,
			bool clipOnBottom)
		{
			// Implemented in area and range chart
			return null;
		}
		#endregion

        #region IDisposable overrides
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            { 
                // Dispose managed resources
                if (this._linePen != null)
                {
                    this._linePen.Dispose();
                    this._linePen = null;
                }
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
