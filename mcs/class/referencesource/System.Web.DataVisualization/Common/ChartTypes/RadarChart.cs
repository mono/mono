//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		RadarChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	RadarChart, ICircularChartType
//
//  Purpose:	Provides 2D/3D drawing and hit testing functionality 
//              for the Radar chart. RadarChart class is used as a 
//              base class for the PolarChart.
//
//	Reviewed:	GS - Jul 15, 2003
//              AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Resources;
using System.Reflection;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;

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
	#region Enumerations
	
	/// <summary>
	/// Circular chart drawing style.
	/// </summary>
	internal enum RadarDrawingStyle
	{
		/// <summary>
		/// Series are drawn as filled areas.
		/// </summary>
		Area,
		/// <summary>
		/// Series are drawn as lines.
		/// </summary>
		Line,
		/// <summary>
		/// Series are drawn as markers.
		/// </summary>
		Marker
	}

	#endregion // Enumerations

	/// <summary>
    /// RadarChart class provides 2D/3D drawing and hit testing 
    /// functionality for the Radar chart. It is also used as a
    /// base class for the PolarChart. 
	/// </summary>
	internal class RadarChart : IChartType, ICircularChartType
	{
		#region Fields

		/// <summary>
		/// Common elements object
		/// </summary>
        internal CommonElements Common { get; set; }

		/// <summary>
		/// Chart area object
		/// </summary>
        internal ChartArea Area { get; set; }

		/// <summary>
		/// Auto label position flag
		/// </summary>
		private	bool				_autoLabelPosition = true;

		/// <summary>
		/// Label position
		/// </summary>
		private	LabelAlignmentStyles		_labelPosition = LabelAlignmentStyles.Top;

		#endregion

        #region Constructors

        /// <summary>
		/// Class public constructor.
		/// </summary>
		public RadarChart()
		{
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		virtual public string Name			{ get{ return ChartTypeNames.Radar;}}

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
		public bool CircularChartArea	{ get{ return true;} }

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
		/// True if palette colors should be applied for each data point.
		/// Otherwise the color is applied to the series.
		/// </summary>
		virtual public bool ApplyPaletteColorsToPoints	{ get { return false; } }

		/// <summary>
		/// Indicates that extra Y values are connected to the scale of the Y axis
		/// </summary>
		virtual public bool ExtraYValuesConnectedToYAxis{ get { return false; } }
		
		/// <summary>
		/// Indicates that this is a one hundred percent chart.
		/// Axis scale from 0 to 100 percent should be used.
		/// </summary>
		virtual public bool HundredPercent{ get{return false;} }

		/// <summary>
		/// Indicates that negative 100% stacked values are shown on
		/// the other side of the X axis
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
			if(series != null)
			{
				RadarDrawingStyle drawingStyle = GetDrawingStyle(series, new DataPoint(series)); 
				if(drawingStyle == RadarDrawingStyle.Line)
				{
					return LegendImageStyle.Line;
				}
				else if(drawingStyle == RadarDrawingStyle.Marker)
				{
					return LegendImageStyle.Marker;
				}
			}
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

		#region ICircularChartType interface implementation

		/// <summary>
		/// Checks if closed figure should be drawn even in Line drawing mode.
		/// </summary>
		/// <returns>True if closed figure should be drawn even in Line drawing mode.</returns>
		public virtual bool RequireClosedFigure()
		{
			return true;
		}

		/// <summary>
		/// Checks if Y axis position may be changed using X axis Crossing property.
		/// </summary>
		/// <returns>True if Y axis position may be changed using X axis Crossing property.</returns>
		public virtual bool XAxisCrossingSupported()
		{
			return false;
		}

		/// <summary>
		/// Checks if automatic X axis labels are supported.
		/// </summary>
		/// <returns>True if automatic X axis labels are supported.</returns>
		public virtual bool XAxisLabelsSupported()
		{
			return false;
		}

		/// <summary>
		/// Checks if radial grid lines (X axis) are supported by the chart type.
		/// </summary>
		/// <returns>True if radial grid lines are supported.</returns>
		public virtual bool RadialGridLinesSupported()
		{
			return false;
		}

		/// <summary>
		/// Gets number of sectors in the circular chart area.
		/// </summary>
		/// <param name="area">Chart area to get number of sectors for.</param>
		/// <param name="seriesCollection">Collection of series.</param>
		/// <returns>Returns number of sectors in circular chart.</returns>
		public virtual int GetNumerOfSectors(ChartArea area, SeriesCollection seriesCollection)
		{
			int	sectorNumber = 0;

			// Get maximum number of points in all series
			foreach(Series series in seriesCollection)
			{
				if(series.IsVisible() && series.ChartArea == area.Name)
				{
					sectorNumber = (int)Math.Max(series.Points.Count, sectorNumber);
				}
			}
			return sectorNumber;
		}

		/// <summary>
		/// Get a location of each Y axis in degrees.
		/// </summary>
		/// <param name="area">Chart area to get Y axes locations for.</param>
		/// <returns>Returns an array of one or more locations of Y axis.</returns>
		public virtual float[] GetYAxisLocations(ChartArea area)
		{
			float[]	axesLocation = new float[area.CircularSectorsNumber];
			float sectorSize = 360f / ((float)axesLocation.Length);
			for(int index = 0; index < axesLocation.Length; index++)
			{
				axesLocation[index] = sectorSize * index;
			}
			return axesLocation;
		}

		#endregion // ICircularChartType interface implementation

		#region Painting and Selection

		/// <summary>
		/// Paint Radar Chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		virtual public void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
		{	
			this.Common = common;
			this.Area = area;

			// Draw chart
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
				
			//************************************************************
			//** Loop through all series
			//************************************************************
			foreach( Series ser in common.DataManager.Series )
			{
				// Process only series in this chart area
				if( ser.ChartArea != area.Name || !ser.IsVisible())
				{
					continue;
				}

				// All series attached to this chart area must have Radar chart type
				if(String.Compare( ser.ChartTypeName, this.Name, true, System.Globalization.CultureInfo.CurrentCulture ) != 0 )
				{
					throw(new InvalidOperationException(SR.ExceptionChartTypeCanNotCombine(ser.ChartTypeName, this.Name)));
				}

				//************************************************************
				//** Call Back Paint event
				//************************************************************
				if( !selection )
				{
                    common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}

				// Chart type do not supprot secondary axes
                //if (ser.YAxisType == AxisType.Secondary)
                //{
                //    throw (new InvalidOperationException(SR.ExceptionChartTypeSecondaryYAxisUnsupported(this.Name)));
                //}
                //if (ser.XAxisType == AxisType.Secondary)
                //{
                //    throw (new InvalidOperationException(SR.ExceptionChartTypeSecondaryXAxisUnsupported(this.Name)));
                //}

				// Set active vertical axis and scaleView boundary
				Axis vAxis = area.GetAxis(AxisName.Y, AxisType.Primary, ser.YSubAxisName);
				double vAxisMin = vAxis.ViewMinimum;
				double vAxisMax = vAxis.ViewMaximum;

				//************************************************************
				//** Fill the array of data points coordinates (absolute)
				//************************************************************
				PointF[]	dataPointPos = GetPointsPosition(graph, area, ser);

				//************************************************************
				//** Draw shadow of area first
				//************************************************************
				int index = 0;				// Data points loop
				if(ser.ShadowOffset != 0 && !selection)
				{
					foreach( DataPoint point in ser.Points )
					{
						// Calculate second point index
						int	secondPointIndex = index + 1;
						if(secondPointIndex >= ser.Points.Count)
						{
							secondPointIndex = 0;
						}

						// Get visual properties of the point 
						DataPointCustomProperties pointAttributes = point;

						if(ser.Points[secondPointIndex].IsEmpty)
						{
							pointAttributes = ser.Points[secondPointIndex];
						}


						//************************************************************
						//** Check what is the main element of radar point. It can be
						//** area (default), line or marker.
						//************************************************************
						Color	areaColor = pointAttributes.Color;
						Color	borderColor = pointAttributes.BorderColor;
						int		borderWidth = pointAttributes.BorderWidth;
						ChartDashStyle borderDashStyle = pointAttributes.BorderDashStyle;
						RadarDrawingStyle drawingStyle = GetDrawingStyle(ser, point); 

						// Check if point Y value is in axis scaleView
						if(vAxis.GetLogValue(point.YValues[0]) > vAxisMax || vAxis.GetLogValue(point.YValues[0]) < vAxisMin ||
							vAxis.GetLogValue(ser.Points[secondPointIndex].YValues[0]) > vAxisMax || vAxis.GetLogValue(ser.Points[secondPointIndex].YValues[0]) < vAxisMin)
						{
							++index;
							continue;
						}

						if(drawingStyle == RadarDrawingStyle.Line)
						{
							// Use the main color for the border and make sure border is visible
							borderColor = pointAttributes.Color;
							borderWidth = (borderWidth < 1) ? 1 : borderWidth;
							borderDashStyle = (borderDashStyle == ChartDashStyle.NotSet) ? ChartDashStyle.Solid : borderDashStyle;

							// Area is not visible
							areaColor = Color.Transparent;
						}
						else if(drawingStyle == RadarDrawingStyle.Marker)
						{
							// Area is not visible
							areaColor = Color.Transparent;
						}

						// Check if line should be always closed
						if(secondPointIndex == 0 && 
							!RequireClosedFigure() &&
							drawingStyle != RadarDrawingStyle.Area)
						{
							break;
						}

						//************************************************************
						//** Fill area
						//************************************************************
                        if (areaColor != Color.Transparent && areaColor != Color.Empty && ser.ShadowOffset != 0)
                        {
                            // Create sector path
                            using (GraphicsPath fillPath = new GraphicsPath())
                            {
                                fillPath.AddLine(graph.GetAbsolutePoint(area.circularCenter), dataPointPos[index]);
                                fillPath.AddLine(dataPointPos[index], dataPointPos[secondPointIndex]);
                                fillPath.AddLine(dataPointPos[secondPointIndex], graph.GetAbsolutePoint(area.circularCenter));

                                // Shift shadow position
                                Matrix shadowMatrix = new Matrix();
                                shadowMatrix.Translate(ser.ShadowOffset, ser.ShadowOffset);
                                fillPath.Transform(shadowMatrix);

                                // Fill shadow sector
                                using (Brush brush = new SolidBrush(ser.ShadowColor))
                                {
                                    graph.FillPath(brush, fillPath);
                                }
                            }
                        }

						// Increase index
						++index;
					}
				}


				//************************************************************
				//** Loop through all data points in the series and fill areas
				//** and draw border lines.
				//************************************************************
				index = 0;				// Data points loop
				foreach( DataPoint point in ser.Points )
				{
					// Set pre-calculated point position
					point.positionRel = graph.GetRelativePoint(dataPointPos[index]);

					// Calculate second point index
					int	secondPointIndex = index + 1;
					if(secondPointIndex >= ser.Points.Count)
					{
						secondPointIndex = 0;
					}

					// Get visual properties of the point 
					DataPointCustomProperties pointAttributes = point;

					if(ser.Points[secondPointIndex].IsEmpty)
					{
						pointAttributes = ser.Points[secondPointIndex];
					}


					//************************************************************
					//** Check what is the main element of radar point. It can be
					//** area (default), line or marker.
					//************************************************************
					Color	areaColor = pointAttributes.Color;
					Color	borderColor = pointAttributes.BorderColor;
					int		borderWidth = pointAttributes.BorderWidth;
					ChartDashStyle borderDashStyle = pointAttributes.BorderDashStyle;
					RadarDrawingStyle drawingStyle = GetDrawingStyle(ser, point); 

					// Check if point Y value is in axis scaleView
					if(vAxis.GetLogValue(point.YValues[0]) > vAxisMax || vAxis.GetLogValue(point.YValues[0]) < vAxisMin ||
						vAxis.GetLogValue(ser.Points[secondPointIndex].YValues[0]) > vAxisMax || vAxis.GetLogValue(ser.Points[secondPointIndex].YValues[0]) < vAxisMin)
					{
						++index;
						continue;
					}

					if(drawingStyle == RadarDrawingStyle.Line)
					{
						// Use the main color for the border and make sure border is visible
						borderColor = pointAttributes.Color;
						borderWidth = (borderWidth < 1) ? 1 : borderWidth;
						borderDashStyle = (borderDashStyle == ChartDashStyle.NotSet) ? ChartDashStyle.Solid : borderDashStyle;

						// Area is not visible
						areaColor = Color.Transparent;
					}
					else if(drawingStyle == RadarDrawingStyle.Marker)
					{
						// Area is not visible
						areaColor = Color.Transparent;
					}

					// Check if line should be always closed
                    using (GraphicsPath selectionPath = new GraphicsPath())
                    {
                        if (secondPointIndex == 0 &&
                            !RequireClosedFigure() &&
                            drawingStyle != RadarDrawingStyle.Area)
                        {
                            // Process hot region for the last point
                            if (common.ProcessModeRegions)
                            {
                                // Add area to the selection path
                                AddSelectionPath(area, selectionPath, dataPointPos, index, secondPointIndex, graph.GetAbsolutePoint(area.circularCenter), 0);

                                // Insert area just after the last custom area
                                int insertIndex = common.HotRegionsList.FindInsertIndex();

                                // Insert area
                                common.HotRegionsList.AddHotRegion(
                                    insertIndex,
                                    selectionPath,
                                    false,
                                    graph,
                                    point,
                                    ser.Name,
                                    index);
                            }
                            break;
                        }

                        //************************************************************
                        //** Fill area
                        //************************************************************
                        if (areaColor != Color.Transparent && areaColor != Color.Empty)
                        {
                            // Create sector path
                            using (GraphicsPath fillPath = new GraphicsPath())
                            {
                                fillPath.AddLine(graph.GetAbsolutePoint(area.circularCenter), dataPointPos[index]);
                                fillPath.AddLine(dataPointPos[index], dataPointPos[secondPointIndex]);
                                fillPath.AddLine(dataPointPos[secondPointIndex], graph.GetAbsolutePoint(area.circularCenter));

                                if (common.ProcessModePaint)
                                {
                                    // Create fill brush
                                    using (Brush brush = graph.CreateBrush(
                                        fillPath.GetBounds(),
                                        areaColor,
                                        pointAttributes.BackHatchStyle,
                                        pointAttributes.BackImage,
                                        pointAttributes.BackImageWrapMode,
                                        pointAttributes.BackImageTransparentColor,
                                        pointAttributes.BackGradientStyle,
                                        pointAttributes.BackSecondaryColor))
                                    {

                                        // Start Svg Selection mode
                                        graph.StartHotRegion(point);

                                        // Fill sector
                                        graph.FillPath(brush, fillPath);

                                        // End Svg Selection mode
                                        graph.EndHotRegion();
                                    }
                                }
                            }

                            if (common.ProcessModeRegions)
                            {
                                // Add area to the selection path
                                AddSelectionPath(area, selectionPath, dataPointPos, index, secondPointIndex, graph.GetAbsolutePoint(area.circularCenter), 0);
                            }

                        }

                        //************************************************************
                        //** Draw Line
                        //************************************************************
                        if (borderColor != Color.Empty && borderWidth > 0 && borderDashStyle != ChartDashStyle.NotSet)
                        {
                            if (secondPointIndex < ser.Points.Count)
                            {
                                if (common.ProcessModePaint)
                                {
                                    // Start Svg Selection mode
                                    graph.StartHotRegion(point);

                                    graph.DrawLineAbs(
                                        borderColor,
                                        borderWidth,
                                        borderDashStyle,
                                        dataPointPos[index],
                                        dataPointPos[secondPointIndex],
                                        ser.ShadowColor,
                                        (areaColor == Color.Transparent || areaColor == Color.Empty) ? ser.ShadowOffset : 0);

                                    // End Svg Selection mode
                                    graph.EndHotRegion();
                                }

                                if (common.ProcessModeRegions)
                                {
                                    // Add line to the selection path
                                    AddSelectionPath(area, selectionPath, dataPointPos, index, secondPointIndex, PointF.Empty, borderWidth);
                                }
                            }
                        }

                        //************************************************************
                        //** Image map for the area and line
                        //************************************************************
                        if (common.ProcessModeRegions)
                        {
                            // Insert area just after the last custom area
                            int insertIndex = common.HotRegionsList.FindInsertIndex();

                            // Insert area
                            common.HotRegionsList.AddHotRegion(
                                insertIndex,
                                selectionPath,
                                false,
                                graph,
                                point,
                                ser.Name,
                                index);
                        }
                    }
					// Increase index
					++index;
				}

				//************************************************************
				//** Loop through all data points in the series and draw
				//** markers and points labels.
				//************************************************************
				int	markerIndex = 0;		// Marker index
				index = 0;					// Data points loop
				foreach( DataPoint point in ser.Points )
				{
					//************************************************************
					//** Check what is the main element of radar point. It can be
					//** area (default), line or marker.
					//************************************************************
					Color	markerColor = point.MarkerColor;
					MarkerStyle markerStyle = point.MarkerStyle;
					RadarDrawingStyle drawingStyle = GetDrawingStyle(ser, point); 

					// Check if point Y value is in axis scaleView
					if(vAxis.GetLogValue(point.YValues[0]) > vAxisMax || 
						vAxis.GetLogValue(point.YValues[0]) < vAxisMin)
					{
						++index;
						continue;
					}

					if(drawingStyle == RadarDrawingStyle.Marker &&
						markerColor.IsEmpty)
					{
						// Set main color to marker
						markerColor = point.Color;
					}

					//************************************************************
					//** Get marker  size
					//************************************************************
					// Get marker size
					SizeF markerSize = GetMarkerSize(
						graph, 
						common, 
						area, 
						point, 
						point.MarkerSize, 
						point.MarkerImage);

					//************************************************************
					//** Draw point chart
					//************************************************************
					if( common.ProcessModePaint )
					{
						if(markerStyle != MarkerStyle.None || point.MarkerImage.Length > 0)
						{
							// If marker style is set and color is not - use main color of point
							if(markerColor.IsEmpty)
							{
								markerColor = point.Color;
							}

							// Check marker index
							if(markerIndex == 0)
							{
								// Start Svg Selection mode
								graph.StartHotRegion( point );

								// Draw the marker
								graph.DrawMarkerAbs(dataPointPos[index], 
									markerStyle,
									(int)markerSize.Height,
									markerColor,
									point.MarkerBorderColor,
									point.MarkerBorderWidth,
									point.MarkerImage,
									point.MarkerImageTransparentColor,
									(point.series != null) ? point.series.ShadowOffset : 0,
									(point.series != null) ? point.series.ShadowColor : Color.Empty,
									new RectangleF(dataPointPos[index].X, dataPointPos[index].Y, markerSize.Width, markerSize.Height),
									false);

                                // End Svg Selection mode
								graph.EndHotRegion( );
							}
						
							// Increase the markers counter
							++markerIndex;
							if(ser.MarkerStep == markerIndex)
							{
								markerIndex = 0;
							}
						}

                        // Draw labels
						DrawLabels( 
							area, 
							graph, 
							common, 
							dataPointPos[index], 
							(int)markerSize.Height, 
							point, 
							ser, 
							index);
					}

					if( common.ProcessModeRegions )
					{
						// Get relative marker size
						SizeF relativeMarkerSize = graph.GetRelativeSize(markerSize);

						// Get relative marker position
						PointF relativeMarkerPosition = graph.GetRelativePoint(dataPointPos[index]);

						// Insert area just after the last custom area
						int insertIndex = common.HotRegionsList.FindInsertIndex();
										
						// Insert circle area
						if(point.MarkerStyle == MarkerStyle.Circle)
						{
							float[]	circCoord = new float[3];
							circCoord[0] = relativeMarkerPosition.X;
							circCoord[1] = relativeMarkerPosition.Y;
							circCoord[2] = relativeMarkerSize.Width/2f;

							common.HotRegionsList.AddHotRegion( 
								insertIndex, 
								graph, 
								circCoord[0], 
								circCoord[1],
								circCoord[2],
								point,
								ser.Name,
								index );
						}

							// All other markers represented as rectangles
						else
						{
							common.HotRegionsList.AddHotRegion(
								new RectangleF(relativeMarkerPosition.X - relativeMarkerSize.Width/2f, relativeMarkerPosition.Y - relativeMarkerSize.Height/2f, relativeMarkerSize.Width, relativeMarkerSize.Height),
								point,
								ser.Name,
								index );
						}
					}
				
					++index;
				}
						
				// Call Paint event
				if( !selection )
				{
                    common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}
			}
		}

		/// <summary>
		/// Creates selection path for one data point.
		/// </summary>
		/// <param name="area">Chart area object.</param>
		/// <param name="selectionPath">Selection path used for data storing.</param>
		/// <param name="dataPointPos">Array of points positions.</param>
		/// <param name="firstPointIndex">First point index.</param>
		/// <param name="secondPointIndex">Second point index.</param>
		/// <param name="centerPoint">Center point for segment area.</param>
		/// <param name="borderWidth">Border width</param>
		internal void AddSelectionPath(
			ChartArea area,
			GraphicsPath selectionPath, 
			PointF[] dataPointPos, 
			int firstPointIndex, 
			int secondPointIndex, 
			PointF centerPoint, 
			int borderWidth)
		{
			// Calculate "half" points on the left and right side of the point
			PointF	rightSidePoint = GetMiddlePoint(dataPointPos[firstPointIndex], dataPointPos[secondPointIndex]);
			PointF	leftSidePoint = PointF.Empty;
			if(firstPointIndex > 0)
			{
				leftSidePoint = GetMiddlePoint(dataPointPos[firstPointIndex], dataPointPos[firstPointIndex - 1]);
			}
			else if(firstPointIndex == 0)
			{
				if(area.CircularSectorsNumber == dataPointPos.Length - 1)
				{
					leftSidePoint = GetMiddlePoint(dataPointPos[firstPointIndex], dataPointPos[dataPointPos.Length - 2]);
				}
			}

			// Add area segment
			if(!centerPoint.IsEmpty)
			{
				selectionPath.AddLine(centerPoint, rightSidePoint);
				selectionPath.AddLine(rightSidePoint, dataPointPos[firstPointIndex]);
				if(leftSidePoint.IsEmpty)
				{
					selectionPath.AddLine(dataPointPos[firstPointIndex], centerPoint);
				}
				else
				{
					selectionPath.AddLine(dataPointPos[firstPointIndex], leftSidePoint);
					selectionPath.AddLine(leftSidePoint, centerPoint);
				}
			}
			else
			{
				// Add line
				GraphicsPath	linePath = new GraphicsPath();
				if(!leftSidePoint.IsEmpty)
				{
					linePath.AddLine(leftSidePoint, dataPointPos[firstPointIndex]);
				}
				linePath.AddLine(dataPointPos[firstPointIndex], rightSidePoint);

				// Widen path
				try
				{
					linePath.Widen(new Pen(Color.Black, borderWidth + 2));
					linePath.Flatten();
				}
                catch (OutOfMemoryException)
                {
                    // GraphicsPath.Widen incorrectly throws OutOfMemoryException
                    // catching here and reacting by not widening
                }
                catch (ArgumentException)
                {
                }

				// Add to the selection path
				selectionPath.AddPath(linePath, false);
			}

		}

		/// <summary>
		/// Gets the middle point of the line.
		/// </summary>
		/// <param name="p1">First line point.</param>
		/// <param name="p2">Second line point.</param>
		/// <returns></returns>
		private PointF GetMiddlePoint(PointF p1, PointF p2)
		{
			PointF middlePoint = PointF.Empty;
			middlePoint.X = (p1.X + p2.X) / 2f;
			middlePoint.Y = (p1.Y + p2.Y) / 2f;
			return middlePoint;
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
		/// Fills a PointF array of data points absolute pixel positions.
		/// </summary>
		/// <param name="graph">Graphics object.</param>
		/// <param name="area">Chart area.</param>
		/// <param name="series">Point series.</param>
		/// <returns>Array of data points position.</returns>
		virtual protected PointF[] GetPointsPosition(ChartGraphics graph, ChartArea area, Series series)
		{
			PointF[]	pointPos = new PointF[series.Points.Count + 1];
			int index = 0;
			foreach( DataPoint point in series.Points )
			{
				// Change Y value if line is out of plot area
				double yValue = GetYValue(Common, area, series, point, index, 0);

				// Recalculates y position
				double yPosition = area.AxisY.GetPosition( yValue );

				// Recalculates x position
				double xPosition = area.circularCenter.X;

				// Add point position into array
				pointPos[index] = graph.GetAbsolutePoint(new PointF((float)xPosition, (float)yPosition));

				// Rotate position
				float	sectorAngle = 360f / area.CircularSectorsNumber * index;
				Matrix matrix = new Matrix();
				matrix.RotateAt(sectorAngle, graph.GetAbsolutePoint(area.circularCenter));
				PointF[]	rotatedPoint = new PointF[] { pointPos[index] };
				matrix.TransformPoints(rotatedPoint);
				pointPos[index] = rotatedPoint[0];
								
				index++;
			}

			// Add last center point
			pointPos[index] = graph.GetAbsolutePoint(area.circularCenter);

			return pointPos;
		}

		/// <summary>
		/// This method draws labels in point chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object</param>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="markerPosition">Marker position</param>
		/// <param name="markerSize">Marker size</param>
		/// <param name="point">Data point</param>
		/// <param name="ser">Data series</param>
		/// <param name="pointIndex">Data point index.</param>
		internal void DrawLabels( 
			ChartArea area, 
			ChartGraphics graph, 
			CommonElements common, 
			PointF markerPosition, 
			int markerSize, 
			DataPoint point, 
			Series ser,
			int pointIndex)
		{
			// Get some properties for performance
			string	pointLabel = point.Label;
			bool	pointShowLabelAsValue = point.IsValueShownAsLabel;

			// ****************************
			// Draw data point value label
			// ****************************
			if((!point.IsEmpty && (ser.IsValueShownAsLabel || pointShowLabelAsValue || pointLabel.Length > 0)) ||
				(pointShowLabelAsValue || pointLabel.Length > 0))
			{
				// Label text format
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;

                    // Get label text
                    string text;
                    if (pointLabel.Length == 0)
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
                        text = point.ReplaceKeywords(pointLabel);
                    }

                    // Get point label style attribute
                    SizeF sizeMarker = new SizeF(markerSize, markerSize);
                    SizeF sizeFont = graph.MeasureString(text, point.Font, new SizeF(1000f, 1000f), StringFormat.GenericTypographic);

                    // Increase label size when background is drawn
                    SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
                    sizeLabel.Height += sizeLabel.Height / 2;
                    sizeLabel.Width += sizeLabel.Width / text.Length;

                    // Get attribute from point or series
                    this._autoLabelPosition = true;
                    string attrib = point[CustomPropertyName.LabelStyle];
                    if (attrib == null || attrib.Length == 0)
                    {
                        attrib = ser[CustomPropertyName.LabelStyle];
                    }
                    if (attrib != null && attrib.Length > 0)
                    {
                        this._autoLabelPosition = false;

                        // Get label position from attribute
                        if (String.Compare(attrib, "Auto", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._autoLabelPosition = true;
                        }
                        else if (String.Compare(attrib, "Center", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._labelPosition = LabelAlignmentStyles.Center;
                        }
                        else if (String.Compare(attrib, "Bottom", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._labelPosition = LabelAlignmentStyles.Bottom;
                        }
                        else if (String.Compare(attrib, "TopLeft", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._labelPosition = LabelAlignmentStyles.TopLeft;
                        }
                        else if (String.Compare(attrib, "TopRight", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._labelPosition = LabelAlignmentStyles.TopRight;
                        }
                        else if (String.Compare(attrib, "BottomLeft", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._labelPosition = LabelAlignmentStyles.BottomLeft;
                        }
                        else if (String.Compare(attrib, "BottomRight", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._labelPosition = LabelAlignmentStyles.BottomRight;
                        }
                        else if (String.Compare(attrib, "Left", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._labelPosition = LabelAlignmentStyles.Left;
                        }
                        else if (String.Compare(attrib, "Right", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._labelPosition = LabelAlignmentStyles.Right;
                        }
                        else if (String.Compare(attrib, "Top", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._labelPosition = LabelAlignmentStyles.Top;
                        }
                        else
                        {
                            throw (new ArgumentException(SR.ExceptionCustomAttributeValueInvalid(attrib, "LabelStyle")));
                        }
                    }

                    // Try to get automatic label position
                    if (this._autoLabelPosition)
                    {
                        this._labelPosition = GetAutoLabelPosition(area, ser, pointIndex);
                    }

                    // Calculate label position
                    PointF position = new PointF(markerPosition.X, markerPosition.Y);
                    switch (this._labelPosition)
                    {
                        case LabelAlignmentStyles.Center:
                            format.Alignment = StringAlignment.Center;
                            break;
                        case LabelAlignmentStyles.Bottom:
                            format.Alignment = StringAlignment.Center;
                            position.Y += sizeMarker.Height / 1.75F;
                            position.Y += sizeLabel.Height / 2F;
                            break;
                        case LabelAlignmentStyles.Top:
                            format.Alignment = StringAlignment.Center;
                            position.Y -= sizeMarker.Height / 1.75F;
                            position.Y -= sizeLabel.Height / 2F;
                            break;

                        case LabelAlignmentStyles.Left:
                            format.Alignment = StringAlignment.Far;
                            position.X -= sizeMarker.Height / 1.75F;
                            break;
                        case LabelAlignmentStyles.TopLeft:
                            format.Alignment = StringAlignment.Far;
                            position.X -= sizeMarker.Height / 1.75F;
                            position.Y -= sizeMarker.Height / 1.75F;
                            position.Y -= sizeLabel.Height / 2F;
                            break;
                        case LabelAlignmentStyles.BottomLeft:
                            format.Alignment = StringAlignment.Far;
                            position.X -= sizeMarker.Height / 1.75F;
                            position.Y += sizeMarker.Height / 1.75F;
                            position.Y += sizeLabel.Height / 2F;
                            break;
                        case LabelAlignmentStyles.Right:
                            //format.Alignment = StringAlignment.Near;
                            position.X += sizeMarker.Height / 1.75F;
                            break;
                        case LabelAlignmentStyles.TopRight:
                            //format.Alignment = StringAlignment.Near;
                            position.X += sizeMarker.Height / 1.75F;
                            position.Y -= sizeMarker.Height / 1.75F;
                            position.Y -= sizeLabel.Height / 2F;
                            break;
                        case LabelAlignmentStyles.BottomRight:
                            //format.Alignment = StringAlignment.Near;
                            position.X += sizeMarker.Height / 1.75F;
                            position.Y += sizeMarker.Height / 1.75F;
                            position.Y += sizeLabel.Height / 2F;
                            break;
                    }

                    // Get text angle
                    int textAngle = point.LabelAngle;

                    // Check if text contains white space only
                    if (text.Trim().Length != 0)
                    {


                        // Check if Smart Labels are enabled
                        if (ser.SmartLabelStyle.Enabled)
                        {
                            position = graph.GetRelativePoint(position);
                            markerPosition = graph.GetRelativePoint(markerPosition);
                            sizeFont = graph.GetRelativeSize(sizeFont);
                            sizeMarker = graph.GetRelativeSize(sizeMarker);

                            // Adjust label position using SmartLabelStyle algorithm
                            position = area.smartLabels.AdjustSmartLabelPosition(
                                common,
                                graph,
                                area,
                                ser.SmartLabelStyle,
                                position,
                                sizeFont,
                                format,
                                markerPosition,
                                sizeMarker,
                                this._labelPosition);

                            // Restore absolute coordinates
                            if (!position.IsEmpty)
                            {
                                position = graph.GetAbsolutePoint(position);
                            }
                            sizeFont = graph.GetAbsoluteSize(sizeFont);

                            // Smart labels always use 0 degrees text angle
                            textAngle = 0;
                        }


                        // Draw label
                        if (!position.IsEmpty)
                        {
                            position = graph.GetRelativePoint(position);

                            // Get label background position
                            RectangleF labelBackPosition = RectangleF.Empty;
                            sizeLabel = graph.GetRelativeSize(sizeFont);
                            sizeLabel.Height += sizeLabel.Height / 8;
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
                                    pointIndex);
                            }
                        }
                    }
                }
			}
		}

		/// <summary>
		/// Gets label position depending on the prev/next point values.
		/// This method will reduce label overlapping with the chart itself (line).
		/// </summary>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="series">Series.</param>
		/// <param name="pointIndex">Data point index in series.</param>
		/// <returns>Return automaticly detected label position.</returns>
		virtual protected LabelAlignmentStyles GetAutoLabelPosition(ChartArea area, Series series, int pointIndex)
		{
			LabelAlignmentStyles	labelAlignment = LabelAlignmentStyles.Top;

			// Calculate data point sector angle
			float	sectorAngle = 360f / area.CircularSectorsNumber * pointIndex;

			if(sectorAngle == 0f)
			{
				labelAlignment = LabelAlignmentStyles.TopRight;
			}
			else if(sectorAngle >= 0 && sectorAngle <= 45)
			{
				labelAlignment = LabelAlignmentStyles.Top;
			}
			else if(sectorAngle >= 45 && sectorAngle <= 90)
			{
				labelAlignment = LabelAlignmentStyles.TopRight;
			}
			else if(sectorAngle >= 90 && sectorAngle <= 135)
			{
				labelAlignment = LabelAlignmentStyles.BottomRight;
			}
			else if(sectorAngle >= 135 && sectorAngle <= 180)
			{
				labelAlignment = LabelAlignmentStyles.BottomRight;
			}
			else if(sectorAngle >= 180 && sectorAngle <= 225)
			{
				labelAlignment = LabelAlignmentStyles.BottomLeft;
			}
			else if(sectorAngle >= 225 && sectorAngle <= 270)
			{
				labelAlignment = LabelAlignmentStyles.BottomLeft;
			}
			else if(sectorAngle >= 270 && sectorAngle <= 315)
			{
				labelAlignment = LabelAlignmentStyles.TopLeft;
			}
			else if(sectorAngle >= 315 && sectorAngle <= 360)
			{
				labelAlignment = LabelAlignmentStyles.TopLeft;
			}

			return labelAlignment;
		}

		/// <summary>
		/// Gets radar chart drawing style.
		/// </summary>
		/// <param name="ser">Chart series.</param>
		/// <param name="point">Series point.</param>
		/// <returns>Returns radar drawing style.</returns>
		virtual protected RadarDrawingStyle GetDrawingStyle(Series ser, DataPoint point)
		{
			RadarDrawingStyle drawingStyle = RadarDrawingStyle.Area;
			if(point.IsCustomPropertySet(CustomPropertyName.RadarDrawingStyle) || 
				ser.IsCustomPropertySet(CustomPropertyName.RadarDrawingStyle))
			{
				string	attributeValue = 
					(point.IsCustomPropertySet(CustomPropertyName.RadarDrawingStyle)) ? 
					point[CustomPropertyName.RadarDrawingStyle] : 
					ser[CustomPropertyName.RadarDrawingStyle];
				if(String.Compare(attributeValue, "Area", StringComparison.OrdinalIgnoreCase) == 0 )
				{
					drawingStyle = RadarDrawingStyle.Area;
				}
                else if (String.Compare(attributeValue, "Line", StringComparison.OrdinalIgnoreCase) == 0)
				{
					drawingStyle = RadarDrawingStyle.Line;
				}
				else if(String.Compare(attributeValue, "Marker", StringComparison.OrdinalIgnoreCase) == 0)
				{
					drawingStyle = RadarDrawingStyle.Marker;
				}
				else
				{
					throw(new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attributeValue, "RadarDrawingStyle")));
				}
			}
			return drawingStyle;
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

			// Point chart do not have height
			if(yValueIndex == -1)
			{
				return 0.0;
			}

			if( point.IsEmpty )
			{
                double result = GetEmptyPointValue(point, pointIndex);

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
		/// <returns>A Value for empty data point.</returns>
		internal double GetEmptyPointValue( DataPoint point, int pointIndex )
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
			if( String.Compare(emptyPointValue, "Zero", StringComparison.OrdinalIgnoreCase) == 0 )
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
					previousPoint = series.Points[indx].YValues[0];
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
					nextPoint = series.Points[indx].YValues[0];
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
			//************************************************************
			//** Fill the array of data points coordinates (absolute)
			//************************************************************
			PointF[]	dataPointPos = GetPointsPosition(common.graph, area, series);

			//************************************************************
			//** Loop through all data points in the series and draw
			//** markers and points labels.
			//************************************************************
			int	markerIndex = 0;		// Marker index
			int index = 0;					// Data points loop
			foreach( DataPoint point in series.Points )
			{
				//************************************************************
				//** Check what is the main element of radar point. It can be
				//** area (default), line or marker.
				//************************************************************
				Color	markerColor = point.MarkerColor;
				MarkerStyle markerStyle = point.MarkerStyle;
				RadarDrawingStyle drawingStyle = GetDrawingStyle(series, point);
				if(drawingStyle == RadarDrawingStyle.Marker)
				{
					// Set main color to marker
					markerColor = point.Color;
				}

				//************************************************************
				//** Get marker  size
				//************************************************************
				// Get marker size
				SizeF markerSize = GetMarkerSize(
					common.graph, 
					common, 
					area, 
					point, 
					point.MarkerSize, 
					point.MarkerImage);

				//************************************************************
				//** Draw point chart
				//************************************************************
				if(markerStyle != MarkerStyle.None || point.MarkerImage.Length > 0)
				{
					// If marker style is set and color is not - use main color of point
					if(markerColor.IsEmpty)
					{
						markerColor = point.Color;
					}

					// Check marker index
					if(markerIndex == 0)
					{
						PointF markerPosition = common.graph.GetRelativePoint(dataPointPos[index]);
						markerSize = common.graph.GetRelativeSize(markerSize);
						RectangleF	markerRect = new RectangleF(
							markerPosition.X - markerSize.Width/2f,
							markerPosition.Y - markerSize.Height/2f,
							markerSize.Width,
							markerSize.Height);
						list.Add(markerRect);
					}
				
					// Increase the markers counter
					++markerIndex;
					if(series.MarkerStep == markerIndex)
					{
						markerIndex = 0;
					}
				}

				++index;
			}
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
    
	/// <summary>
    /// ICircularChartType interface provides behaviuour information for circular 
    /// chart types like Radar or Polar. This interface is similar to IChartType 
    /// interface.
	/// </summary>
	internal interface ICircularChartType 
	{
		#region Methods

		/// <summary>
		/// Checks if closed figure should be drawn even in Line drawing mode.
		/// </summary>
		/// <returns>True if closed figure should be drawn even in Line drawing mode.</returns>
		bool RequireClosedFigure();

		/// <summary>
		/// Checks if Y axis position may be changed using X axis Crossing property.
		/// </summary>
		/// <returns>True if Y axis position may be changed using X axis Crossing property.</returns>
		bool XAxisCrossingSupported();

		/// <summary>
		/// Checks if automatic X axis labels are supported.
		/// </summary>
		/// <returns>True if automatic X axis labels are supported.</returns>
		bool XAxisLabelsSupported();

		/// <summary>
		/// Checks if radial grid lines (X axis) are supported by the chart type.
		/// </summary>
		/// <returns>True if radial grid lines are supported.</returns>
		bool RadialGridLinesSupported();

		/// <summary>
		/// Gets number of sectors in the circular chart area.
		/// </summary>
		/// <param name="area">Chart area to get number of sectors for.</param>
		/// <param name="seriesCollection">Collection of series.</param>
		/// <returns>Returns number of sectors in circular chart.</returns>
		int GetNumerOfSectors(ChartArea area, SeriesCollection seriesCollection);

		/// <summary>
		/// Get a location of each Y axis in degrees.
		/// </summary>
		/// <param name="area">Chart area to get Y axes locations for.</param>
		/// <returns>Returns an array of one or more locations of Y axis.</returns>
		float[] GetYAxisLocations(ChartArea area);

		#endregion // Methods
	}

}
