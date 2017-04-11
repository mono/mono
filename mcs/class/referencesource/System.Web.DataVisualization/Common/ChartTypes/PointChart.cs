//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		PointChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	PointChart
//
//  Purpose:	Provides 2D/3D drawing and hit testing functionality 
//              for the Point chart.
//
//	Reviewed:	AG - Aug 6, 2002
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
    /// PointChart class provides 2D/3D drawing and hit testing 
    /// functionality for the Point chart.
	/// </summary>
    internal class PointChart : IChartType
	{
		#region Fields

		/// <summary>
		/// Indicates that markers will be always drawn
		/// </summary>
		internal bool	alwaysDrawMarkers = true;

		/// <summary>
		/// Index of the Y value used to draw chart
		/// </summary>
        internal int YValueIndex { get; set; }

		/// <summary>
		/// Index of the Y value used to be shown as point value label
		/// </summary>
        internal int labelYValueIndex = -1;

		/// <summary>
		/// Auto label position flag
		/// </summary>
        internal bool autoLabelPosition = true;

		/// <summary>
		/// Label position
		/// </summary>
        internal LabelAlignmentStyles labelPosition = LabelAlignmentStyles.Top;

		/// <summary>
		/// Vertical axes
		/// </summary>
        internal Axis VAxis { get; set; }

		/// <summary>
		/// Horizontal axes
		/// </summary>
        internal Axis HAxis { get; set; }

		/// <summary>
		/// Indexed series flag
		/// </summary>
        internal bool indexedSeries = false;

		/// <summary>
		/// Common elements object
		/// </summary>
        internal CommonElements Common { get; set; }

		/// <summary>
		/// Chart area object
		/// </summary>
        internal ChartArea Area { get; set; }

		/// <summary>
		/// Indicates that marker and label are drawn in the middle of 3D depth
		/// </summary>
        internal bool middleMarker = true;

		/// <summary>
		/// Stores information about 3D labels. Used to draw 3D labels in layers.
		/// </summary>
		internal ArrayList label3DInfoList = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Class public constructor.
		/// </summary>
		public PointChart()
		{
		}

		/// <summary>
		/// Class public constructor.
		/// </summary>
		/// <param name="alwaysDrawMarkers">Indicates if markers should be always painted.</param>
		public PointChart(bool alwaysDrawMarkers)
		{
			this.alwaysDrawMarkers = alwaysDrawMarkers;
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		virtual public string Name			{ get{ return ChartTypeNames.Point;}}

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
		virtual public bool SecondYScale{ get{ return false;} }

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
		/// True if palette colors should be applied for each data paoint.
		/// Otherwise the color is applied to the series.
		/// </summary>
		virtual public bool ApplyPaletteColorsToPoints	{ get { return false; } }

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
		/// How to draw series/points in legend:
		/// Filled rectangle, Line or Marker
		/// </summary>
		/// <param name="series">Legend item series.</param>
		/// <returns>Legend item style.</returns>
		virtual public LegendImageStyle GetLegendImageStyle(Series series)
		{
			return LegendImageStyle.Marker;
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

		#region Painting and Selection

		/// <summary>
		/// Paint Point Chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		virtual public void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
		{	
			this.Common = common;
			this.Area = area;
			ProcessChartType( false, graph, common, area, seriesToDraw);
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
            
            this.Common = common;
			// Prosess 3D chart type
			if(area.Area3DStyle.Enable3D)
			{
				ProcessChartType3D( selection, graph, common, area, seriesToDraw );
				return;
			}

			// Check if series is indexed
			if( ShiftedSerName.Length == 0)
			{
                indexedSeries = ChartHelper.IndexedSeries(this.Common, area.GetSeriesFromChartType(this.Name).ToArray());
			}
			else
			{
				indexedSeries = ChartHelper.IndexedSeries( common.DataManager.Series[ShiftedSerName] );
			}

			//************************************************************
			//** Loop through all series
			//************************************************************
			foreach( Series ser in common.DataManager.Series )
			{
				// Labels and markers have to be shifted if there 
				// is more than one series for column chart. This property 
				// will give a name of the series, which is used, for 
				// labels and markers.
				bool	breakSeriesLoop = false;
				if( ShiftedSerName.Length > 0)
				{
					if( ShiftedSerName != ser.Name )
					{
						continue;
					}
					breakSeriesLoop = true;
				}

				// Process only point chart series in this chart area
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

				//************************************************************
				//** Set active horizontal/vertical axis
				//************************************************************
				HAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
				VAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);
				double horizontalViewMax = HAxis.ViewMaximum;
				double horizontalViewMin = HAxis.ViewMinimum;
				double verticalViewMax = VAxis.ViewMaximum;
				double verticalViewMin = VAxis.ViewMinimum;

				//************************************************************
				//** Call Back Paint event
				//************************************************************
				if( !selection )
				{
                    common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}

				//************************************************************
				//** Loop through all data points in the series
				//************************************************************
				int	markerIndex = 0;		// Marker index
				int	index = 1;				// Data points loop
				foreach( DataPoint point in ser.Points )
				{
					// Reset pre-calculated point position
					point.positionRel = new PointF(float.NaN, float.NaN);

					//************************************************************
					//** Check if point values are in the chart area
					//************************************************************

					// Check for min/max X values
					double xValue = (indexedSeries) ? (double)index : point.XValue;
					xValue = HAxis.GetLogValue(xValue);
					if(xValue > horizontalViewMax || xValue < horizontalViewMin)
					{
						index++;
						continue;
					}

					// Check for min/max Y values
					double	yValue = GetYValue(common, area, ser, point, index - 1, YValueIndex);

					// Axis is logarithmic
					yValue = VAxis.GetLogValue( yValue );
					
					if( yValue > verticalViewMax || yValue < verticalViewMin)
					{
						index++;
						continue;
					}


					// Check if point should be drawn on the edge of the data scaleView.
					bool	skipMarker = false;
					if(!ShouldDrawMarkerOnViewEdgeX())
					{
						// Check for min/max X values
						if(xValue == horizontalViewMax && ShiftedX >= 0)
						{
							skipMarker = true;
						}

						// Check for min/max X values
						if(xValue == horizontalViewMin && ShiftedX <= 0)
						{
							skipMarker = true;
						}
					}

					//************************************************************
					//** Get marker position and size
					//************************************************************
					int			pointMarkerSize = point.MarkerSize;
					string		pointMarkerImage = point.MarkerImage;
					MarkerStyle	pointMarkerStyle = point.MarkerStyle;


					// Get marker position
					PointF markerPosition = PointF.Empty;
					markerPosition.Y = (float)VAxis.GetLinearPosition(yValue);
					if( indexedSeries )
					{
						// The formula for position is based on a distance 
						// from the grid line or nPoints position.
						markerPosition.X = (float)HAxis.GetPosition( (double)index );
					}
					else
					{
						markerPosition.X = (float)HAxis.GetPosition( point.XValue );
					}

					// Labels and markers have to be shifted if there 
					// is more than one series for column chart.
					markerPosition.X += (float)ShiftedX;

					// Remeber pre-calculated point position
					point.positionRel = new PointF(markerPosition.X, markerPosition.Y);

					// Get marker size
					SizeF markerSize = GetMarkerSize(
						graph, 
						common, 
						area, 
						point, 
						pointMarkerSize, 
						pointMarkerImage);

					//************************************************************
					//** Skip marker drawing
					//************************************************************
					if( skipMarker )
					{
						index++;
						continue;
					}

					//************************************************************
					//** Draw point chart
					//************************************************************
					if(alwaysDrawMarkers || 
						pointMarkerStyle != MarkerStyle.None || 
						pointMarkerImage.Length > 0)
					{
						if( common.ProcessModePaint )
						{
						// Check marker index
							if(markerIndex == 0)
							{
								// Start Svg Selection mode
								graph.StartHotRegion( point );

								// Draw the marker
								this.DrawPointMarker(
									graph,
									point.series,
									point,
									markerPosition, 
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
							}

							if( common.ProcessModeRegions )
							{
								SetHotRegions(
									common,
									graph,
									point,
									markerSize,
									point.series.Name,
									index - 1,
									pointMarkerStyle,
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

					// Start Svg Selection mode
					graph.StartHotRegion( point, true );

					// Draw labels
					DrawLabels( 
						area, 
						graph, 
						common, 
						markerPosition, 
						(int)markerSize.Height, 
						point, 
						ser, 
						index - 1);

					// End Svg Selection mode
					graph.EndHotRegion( );

				
					++index;
				}
						
				// Call Paint event
				if( !selection )
				{
                    common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}
			
				// Break series loop.
				if( breakSeriesLoop )
				{
					break;
				}
			}
		}

        /// <summary>
        /// Draw series point marker.
        /// </summary>
        /// <param name="graph">Chart Graphics used for drawing.</param>
        /// <param name="series">Series.</param>
        /// <param name="dataPoint">Series data point.</param>
        /// <param name="point">Coordinates of the center.</param>
        /// <param name="markerStyle">Marker style.</param>
        /// <param name="markerSize">Marker size.</param>
        /// <param name="markerColor">Marker color.</param>
        /// <param name="markerBorderColor">Marker border color.</param>
        /// <param name="markerBorderSize">Marker border size.</param>
        /// <param name="markerImage">Marker image name.</param>
        /// <param name="markerImageTransparentColor">Color of the marker image transparent.</param>
        /// <param name="shadowSize">Marker shadow size.</param>
        /// <param name="shadowColor">Marker shadow color.</param>
        /// <param name="imageScaleRect">Rectangle to which marker image should be scaled.</param>
		protected virtual void DrawPointMarker(
			ChartGraphics graph,
			Series series,
			DataPoint dataPoint,
			PointF point, 
			MarkerStyle markerStyle, 
			int markerSize, 
			Color markerColor, 
			Color markerBorderColor, 
			int markerBorderSize, 
			string markerImage, 
			Color markerImageTransparentColor, 
			int shadowSize, 
			Color shadowColor, 
			RectangleF imageScaleRect
			)
		{
			// Draw marker using relative coordinates
			graph.DrawMarkerRel(
				point, 
				markerStyle,
				markerSize,
				markerColor, 
				markerBorderColor, 
				markerBorderSize, 
				markerImage, 
				markerImageTransparentColor, 
				shadowSize, 
				shadowColor, 
				imageScaleRect);
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
			SizeF relativeMarkerSize = graph.GetRelativeSize(markerSize);

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
		private void DrawLabels( 
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
                            point.YValues[(labelYValueIndex == -1) ? YValueIndex : labelYValueIndex],
                            point.LabelFormat,
                            ser.YValueType,
                            ChartElementType.DataPoint);
                    }
                    else
                    {
                        text = point.ReplaceKeywords(pointLabel);
                    }

                    // Get point label style attribute
                    SizeF sizeMarker = graph.GetRelativeSize(new SizeF(markerSize, markerSize));
                    SizeF sizeFont = graph.GetRelativeSize(
                        graph.MeasureString(text, point.Font, new SizeF(1000f, 1000f), StringFormat.GenericTypographic));

                    SizeF sizeSingleCharacter = graph.GetRelativeSize(
                        graph.MeasureString("W", point.Font, new SizeF(1000f, 1000f), StringFormat.GenericTypographic));

                    // Increase label size when background is drawn
                    SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
                    float horizontalSpacing = sizeLabel.Width / text.Length;
                    sizeLabel.Height += sizeSingleCharacter.Height / 2;
                    sizeLabel.Width += horizontalSpacing;

                    // Get attribute from point or series
                    string attrib = point[CustomPropertyName.LabelStyle];
                    if (attrib == null || attrib.Length == 0)
                    {
                        attrib = ser[CustomPropertyName.LabelStyle];
                    }
                    this.autoLabelPosition = true;
                    if (attrib != null && attrib.Length > 0)
                    {
                        this.autoLabelPosition = false;

                        // Get label position from attribute
                        if (String.Compare(attrib, "Auto", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.autoLabelPosition = true;
                        }
                        else if (String.Compare(attrib, "Center", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.labelPosition = LabelAlignmentStyles.Center;
                        }
                        else if (String.Compare(attrib, "Bottom", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.labelPosition = LabelAlignmentStyles.Bottom;
                        }
                        else if (String.Compare(attrib, "TopLeft", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.labelPosition = LabelAlignmentStyles.TopLeft;
                        }
                        else if (String.Compare(attrib, "TopRight", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.labelPosition = LabelAlignmentStyles.TopRight;
                        }
                        else if (String.Compare(attrib, "BottomLeft", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.labelPosition = LabelAlignmentStyles.BottomLeft;
                        }
                        else if (String.Compare(attrib, "BottomRight", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.labelPosition = LabelAlignmentStyles.BottomRight;
                        }
                        else if (String.Compare(attrib, "Left", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.labelPosition = LabelAlignmentStyles.Left;
                        }
                        else if (String.Compare(attrib, "Right", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.labelPosition = LabelAlignmentStyles.Right;
                        }
                        else if (String.Compare(attrib, "Top", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this.labelPosition = LabelAlignmentStyles.Top;
                        }
                        else
                        {
                            throw (new ArgumentException(SR.ExceptionCustomAttributeValueInvalid(attrib, "LabelStyle")));
                        }
                    }

                    // Try to get automatic label position
                    if (this.autoLabelPosition)
                    {
                        this.labelPosition = GetAutoLabelPosition(ser, pointIndex);
                    }

                    // Calculate label position
                    PointF position = new PointF(markerPosition.X, markerPosition.Y);
                    switch (this.labelPosition)
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
                            position.X -= sizeMarker.Height / 1.75F + horizontalSpacing / 2f;
                            break;
                        case LabelAlignmentStyles.TopLeft:
                            format.Alignment = StringAlignment.Far;
                            position.X -= sizeMarker.Height / 1.75F + horizontalSpacing / 2f;
                            position.Y -= sizeMarker.Height / 1.75F;
                            position.Y -= sizeLabel.Height / 2F;
                            break;
                        case LabelAlignmentStyles.BottomLeft:
                            format.Alignment = StringAlignment.Far;
                            position.X -= sizeMarker.Height / 1.75F + horizontalSpacing / 2f;
                            position.Y += sizeMarker.Height / 1.75F;
                            position.Y += sizeLabel.Height / 2F;
                            break;
                        case LabelAlignmentStyles.Right:
                            //format.Alignment = StringAlignment.Near;
                            position.X += sizeMarker.Height / 1.75F + horizontalSpacing / 2f;
                            break;
                        case LabelAlignmentStyles.TopRight:
                            //format.Alignment = StringAlignment.Near;
                            position.X += sizeMarker.Height / 1.75F + horizontalSpacing / 2f;
                            position.Y -= sizeMarker.Height / 1.75F;
                            position.Y -= sizeLabel.Height / 2F;
                            break;
                        case LabelAlignmentStyles.BottomRight:
                            //format.Alignment = StringAlignment.Near;
                            position.X += sizeMarker.Height / 1.75F + horizontalSpacing / 2f;
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
                                this.labelPosition);

                            // Smart labels always use 0 degrees text angle
                            textAngle = 0;
                        }



                        // Adjust alignment of vertical labels
                        // NOTE: Fixes issue #4560
                        if (textAngle == 90 || textAngle == -90)
                        {
                            switch (this.labelPosition)
                            {
                                case LabelAlignmentStyles.Top:
                                    format.Alignment = StringAlignment.Near;
                                    position.Y += sizeLabel.Height / 2F;
                                    break;
                                case LabelAlignmentStyles.Bottom:
                                    format.Alignment = StringAlignment.Far;
                                    position.Y -= sizeLabel.Height / 2F;
                                    break;
                                case LabelAlignmentStyles.Right:
                                    format.Alignment = StringAlignment.Center;
                                    format.LineAlignment = StringAlignment.Near;
                                    break;
                                case LabelAlignmentStyles.Left:
                                    format.Alignment = StringAlignment.Center;
                                    format.LineAlignment = StringAlignment.Center;
                                    break;
                                case LabelAlignmentStyles.TopLeft:
                                    format.Alignment = StringAlignment.Near;
                                    break;
                                case LabelAlignmentStyles.TopRight:
                                    break;
                                case LabelAlignmentStyles.BottomLeft:
                                    break;
                                case LabelAlignmentStyles.BottomRight:
                                    format.Alignment = StringAlignment.Far;
                                    break;
                            }
                        }

                        // Draw label
                        if (!position.IsEmpty)
                        {
                            // Get label background position
                            RectangleF labelBackPosition = RectangleF.Empty;
                            sizeLabel.Height -= sizeFont.Height / 2;
                            sizeLabel.Height += sizeFont.Height / 8;
                            labelBackPosition = GetLabelPosition(
                                graph,
                                position,
                                sizeLabel,
                                format,
                                true);

                            // Adjust rectangle position due to horizontal spacing
                            switch (this.labelPosition)
                            {
                                case LabelAlignmentStyles.Left:
                                    labelBackPosition.X += horizontalSpacing / 2f;
                                    break;
                                case LabelAlignmentStyles.TopLeft:
                                    labelBackPosition.X += horizontalSpacing / 2f;
                                    break;
                                case LabelAlignmentStyles.BottomLeft:
                                    labelBackPosition.X += horizontalSpacing / 2f;
                                    break;
                                case LabelAlignmentStyles.Right:
                                    labelBackPosition.X -= horizontalSpacing / 2f;
                                    break;
                                case LabelAlignmentStyles.TopRight:
                                    labelBackPosition.X -= horizontalSpacing / 2f;
                                    break;
                                case LabelAlignmentStyles.BottomRight:
                                    labelBackPosition.X -= horizontalSpacing / 2f;
                                    break;
                            }

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
		/// Gets rectangle position of the label.
		/// </summary>
		/// <param name="graph">Chart graphics object.</param>
		/// <param name="position">Original label position.</param>
		/// <param name="size">Label text size.</param>
		/// <param name="format">Label string format.</param>
		/// <param name="adjustForDrawing">Result position is adjusted for drawing.</param>
		/// <returns>Label rectangle position.</returns>
		internal static RectangleF GetLabelPosition(
			ChartGraphics graph,
			PointF position, 
			SizeF size,
			StringFormat format,
			bool adjustForDrawing)
		{
			// Calculate label position rectangle
			RectangleF	labelPosition = RectangleF.Empty;
			labelPosition.Width = size.Width;
			labelPosition.Height = size.Height;

			// Calculate pixel size in relative coordiantes
			SizeF	pixelSize = SizeF.Empty;
			if(graph != null)
			{
				pixelSize = graph.GetRelativeSize(new SizeF(1f, 1f));
			}

			if(format.Alignment == StringAlignment.Far)
			{
				labelPosition.X = position.X - size.Width;
				if(adjustForDrawing && !pixelSize.IsEmpty)
				{
					labelPosition.X -= 4f*pixelSize.Width;
					labelPosition.Width += 4f*pixelSize.Width;
				}
			}
			else if(format.Alignment == StringAlignment.Near)
			{
				labelPosition.X = position.X;
				if(adjustForDrawing && !pixelSize.IsEmpty)
				{
					labelPosition.Width += 4f*pixelSize.Width;
				}
			}
			else if(format.Alignment == StringAlignment.Center)
			{
				labelPosition.X = position.X - size.Width/2F;
				if(adjustForDrawing && !pixelSize.IsEmpty)
				{
					labelPosition.X -= 2f*pixelSize.Width;
					labelPosition.Width += 4f*pixelSize.Width;
				}
			}

			if(format.LineAlignment == StringAlignment.Far)
			{
				labelPosition.Y = position.Y - size.Height;
			}
			else if(format.LineAlignment == StringAlignment.Near)
			{
				labelPosition.Y = position.Y;
			}
			else if(format.LineAlignment == StringAlignment.Center)
			{
				labelPosition.Y = position.Y - size.Height/2F;
			}

			labelPosition.Y -= 1f * pixelSize.Height;

			return labelPosition;
		}

		#endregion

		#region 3D painting and Selection

		/// <summary>
		/// This method recalculates size of the point marker. This method is used 
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


			//************************************************************
			//** Get order of data points drawing
			//************************************************************
			ArrayList	dataPointDrawingOrder = area.GetDataPointDrawingOrder(typeSeries, this, selection, COPCoordinates.X, null, this.YValueIndex, false);

			//************************************************************
			//** Loop through all data poins
			//************************************************************
			foreach(object obj in dataPointDrawingOrder)
			{
				// Process single point
				ProcessSinglePoint3D(
					(DataPoint3D) obj,
					graph, 
					common, 
					area
					);
			}

			// Finish processing 3D labels
			this.DrawAccumulated3DLabels(graph, common, area);
			
		}


		/// <summary>
		/// Draws\Hit tests single 3D point.
		/// </summary>
		/// <param name="pointEx">3D point information.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		internal void ProcessSinglePoint3D( 
			DataPoint3D	pointEx,
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area
			)
		{
			// Get point & series
			DataPoint	point = pointEx.dataPoint;
			Series		ser = point.series;

			// Reset pre-calculated point position
			point.positionRel = new PointF(float.NaN, float.NaN);

			//************************************************************
			//** Set active horizontal/vertical axis
			//************************************************************
			HAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
			VAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

			//************************************************************
			//** Check if point values are in the chart area
			//************************************************************

			// Check for min/max Y values
			double	yValue = GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, YValueIndex);

			// Axis is logarithmic
			yValue = VAxis.GetLogValue( yValue );
				
			if( yValue > VAxis.ViewMaximum || yValue < VAxis.ViewMinimum)
			{
				return;
			}

			// Check for min/max X values
			double xValue = (pointEx.indexedSeries) ? (double)pointEx.index : point.XValue;
			xValue = HAxis.GetLogValue(xValue);
			if(xValue > HAxis.ViewMaximum || xValue < HAxis.ViewMinimum)
			{
				return;
			}

			// Check if point should be drawn on the edge of the data scaleView.
			if(!ShouldDrawMarkerOnViewEdgeX())
			{
				// Check for min/max X values
				if(xValue == HAxis.ViewMaximum && ShiftedX >= 0)
				{
					return;
				}

				// Check for min/max X values
				if(xValue == HAxis.ViewMinimum && ShiftedX <= 0)
				{
					return;
				}
			}

			//************************************************************
			//** Get marker position and size
			//************************************************************

			// Get marker position
			PointF markerPosition = PointF.Empty;
			markerPosition.Y = (float)pointEx.yPosition;
			markerPosition.X = (float)HAxis.GetLinearPosition(xValue);	// No Log transformation required. Done above!

			// Labels and markers have to be shifted if there 
			// is more than one series for column chart.
			markerPosition.X += (float)ShiftedX;

			// Remeber pre-calculated point position
			point.positionRel = new PointF(markerPosition.X, markerPosition.Y);

			// Get point some point properties and save them in variables
			int			pointMarkerSize = point.MarkerSize;
			string		pointMarkerImage = point.MarkerImage;
			MarkerStyle	pointMarkerStyle = point.MarkerStyle;

			// Get marker size
			SizeF markerSize = GetMarkerSize(
				graph, 
				common, 
				area, 
				point, 
				pointMarkerSize, 
				pointMarkerImage);

			//************************************************************
			//** Transform marker position in 3D space
			//************************************************************
			// Get projection coordinates
			Point3D[]	marker3DPosition = new Point3D[1];
			marker3DPosition[0] = new Point3D(markerPosition.X, markerPosition.Y, (float)(pointEx.zPosition + ((this.middleMarker) ? pointEx.depth/2f : pointEx.depth)));

			// Transform coordinates of text size
			area.matrix3D.TransformPoints(marker3DPosition);
			PointF	markerRotatedPosition = marker3DPosition[0].PointF;

			//************************************************************
			//** Draw point chart
			//************************************************************
			GraphicsPath rectPath = null;
			
			if(alwaysDrawMarkers || 
				pointMarkerStyle != MarkerStyle.None || 
				pointMarkerImage.Length > 0)
			{
				// Check marker index
				if((pointEx.index % ser.MarkerStep) == 0)
				{
					// Detect if we need to get graphical path of drawn object
					DrawingOperationTypes	drawingOperationType = DrawingOperationTypes.DrawElement;
				
					if( common.ProcessModeRegions )
					{
						drawingOperationType |= DrawingOperationTypes.CalcElementPath;
					}

					// Start Svg Selection mode
					graph.StartHotRegion( point );

					// Draw the marker
					rectPath = graph.DrawMarker3D(area.matrix3D, 
						area.Area3DStyle.LightStyle,
						pointEx.zPosition + ((this.middleMarker) ? pointEx.depth/2f : pointEx.depth),
						markerPosition, 
						(pointMarkerStyle == MarkerStyle.None) ? MarkerStyle.Circle : pointMarkerStyle,
						(int)markerSize.Height,
						(point.MarkerColor == Color.Empty) ? point.Color : point.MarkerColor,
						(point.MarkerBorderColor == Color.Empty) ? point.BorderColor : point.MarkerBorderColor,
						GetMarkerBorderSize(point),
						pointMarkerImage,
						point.MarkerImageTransparentColor,
						(point.series != null) ? point.series.ShadowOffset : 0,
						(point.series != null) ? point.series.ShadowColor : Color.Empty,
						new RectangleF(markerRotatedPosition.X, markerRotatedPosition.Y, markerSize.Width, markerSize.Height),
						drawingOperationType);

					// End Svg Selection mode
					graph.EndHotRegion( );
				}
			}


			//**********************************************************************
			//** Data point label is not drawn with the data point. Instead the 
            //** information about label is collected and drawn when all points 
			//** with current Z position are drawn.
			//** This is done to achieve correct Z order layering of labels.
			//**********************************************************************
			if(this.label3DInfoList != null && 
				this.label3DInfoList.Count > 0 &&
				((Label3DInfo)this.label3DInfoList[this.label3DInfoList.Count-1]).PointEx.zPosition != pointEx.zPosition)
			{
				// Draw labels with information previously collected
				this.DrawAccumulated3DLabels(graph, common, area);
			}

			// Check if labels info list was created
			if(this.label3DInfoList == null)
			{
				this.label3DInfoList = new ArrayList();
			}

			// Store information about the label for future drawing
			Label3DInfo label3DInfo = new Label3DInfo();
			label3DInfo.PointEx = pointEx;
			label3DInfo.MarkerPosition = markerRotatedPosition;
			label3DInfo.MarkerSize = markerSize;
			this.label3DInfoList.Add(label3DInfo);

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
					circCoord[0] = markerRotatedPosition.X;
					circCoord[1] = markerRotatedPosition.Y;
					circCoord[2] = relativeMarkerSize.Width/2f;

					common.HotRegionsList.AddHotRegion( 
						insertIndex, 
						graph, 
						circCoord[0], 
						circCoord[1], 
						circCoord[2], 
						point, 
						ser.Name, 
						pointEx.index - 1 
						); 
				}

				// Insert path for 3D bar
				if(pointMarkerStyle == MarkerStyle.Square)
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

				// All other markers represented as rectangles
				else
				{
					common.HotRegionsList.AddHotRegion( 
						new RectangleF(markerRotatedPosition.X - relativeMarkerSize.Width/2f, markerRotatedPosition.Y - relativeMarkerSize.Height/2f, relativeMarkerSize.Width, relativeMarkerSize.Height),
						point,
						ser.Name,
						pointEx.index - 1
						);
				}
			}
            if (rectPath != null)
            {
                rectPath.Dispose();
            }
		}

		/// <summary>
		/// Draws labels which are srored in the collection.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		internal void DrawAccumulated3DLabels(
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area)
		{
			if(this.label3DInfoList != null)
			{
				foreach(Label3DInfo labelInfo in this.label3DInfoList)
				{
					// Draw labels
					DrawLabels( 
						area, 
						graph, 
						common, 
						labelInfo.MarkerPosition, 
						(int)labelInfo.MarkerSize.Height, 
						labelInfo.PointEx.dataPoint, 
						labelInfo.PointEx.dataPoint.series, 
						labelInfo.PointEx.index - 1);
	
				}

				// Clear labels info list
				this.label3DInfoList.Clear();
			}
		}

		#endregion

		#region Marker and Labels related methods

		/// <summary>
		/// Indicates that markers are drawnd on the X edge of the data scaleView.
		/// </summary>
		/// <returns>True. Point chart always draws markers on the edge.</returns>
		virtual protected bool ShouldDrawMarkerOnViewEdgeX()
		{
			return true;
		}

		/// <summary>
		/// Gets marker border size.
		/// </summary>
		/// <param name="point">Data point.</param>
		/// <returns>Marker border size.</returns>
        virtual protected int GetMarkerBorderSize(DataPointCustomProperties point)
		{
			return point.MarkerBorderWidth;
		}

		/// <summary>
		/// Gets label position. For point chart this function always returns 'Top'.
		/// </summary>
		/// <param name="series">Series.</param>
		/// <param name="pointIndex">Data point index in series.</param>
		/// <returns>Return automaticly detected label position.</returns>
		virtual protected LabelAlignmentStyles GetAutoLabelPosition(Series series, int pointIndex)
		{
			return LabelAlignmentStyles.Top;
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
            
            if (markerImage.Length > 0) // Get image size
			    common.ImageLoader.GetAdjustedImageSize(markerImage, graph.Graphics, ref size);

			return size;
		}

		#endregion

		#region Labels shifting properties

		/// <summary>
		/// Labels and markers have to be shifted if there 
		/// is more than one series for column chart.
		/// NOT USED IN POINT CHART.
		/// </summary>
		virtual public double ShiftedX
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		/// <summary>
		/// Labels and markers have to be shifted if there 
		/// is more than one series for column chart. This property 
		/// will give a name of the series, which is used, for 
		/// labels and markers.
		/// NOT USED IN POINT CHART.
		/// </summary>
		virtual public string ShiftedSerName
		{
			get
			{
				return "";
			}
			set
			{
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
            
            // Check empty point
            if (point.IsEmpty || double.IsNaN(point.YValues[yValueIndex]))
			{
				double result = GetEmptyPointValue( point, pointIndex );

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
                return 0.0;
			}
			
			//************************************************************
			//** IsEmpty point value is an average of neighbour points
			//************************************************************

			// Find previous non-empty point value
			for( int indx = pointIndex; indx >= 0; indx-- )
			{
				if( !series.Points[indx].IsEmpty )
				{
					previousPoint = series.Points[indx].YValues[YValueIndex];
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
					nextPoint = series.Points[indx].YValues[YValueIndex];
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
            this.Common = common;
            // Check if series is indexed
            indexedSeries = ChartHelper.IndexedSeries(this.Common, area.GetSeriesFromChartType(this.Name).ToArray());

			//************************************************************
			//** Set active horizontal/vertical axis
			//************************************************************
			Axis hAxis = area.GetAxis(AxisName.X, series.XAxisType, series.XSubAxisName);
			Axis vAxis = area.GetAxis(AxisName.Y, series.YAxisType, series.YSubAxisName);

			//************************************************************
			//** Loop through all data points in the series
			//************************************************************
			int	markerIndex = 0;		// Marker index
			int	index = 1;				// Data points loop
			foreach( DataPoint point in series.Points )
			{
				//************************************************************
				//** Check if point values are in the chart area
				//************************************************************

				// Check for min/max Y values
				double	yValue = GetYValue(common, area, series, point, index - 1, YValueIndex);

				// Axis is logarithmic
				yValue = vAxis.GetLogValue( yValue );
				
				if( yValue > vAxis.ViewMaximum || yValue < vAxis.ViewMinimum)
				{
					index++;
					continue;
				}

				// Check for min/max X values
				double xValue = (indexedSeries) ? (double)index : point.XValue;
				xValue = hAxis.GetLogValue(xValue);
				if(xValue > hAxis.ViewMaximum || xValue < hAxis.ViewMinimum)
				{
					index++;
					continue;
				}

				// Check if point should be drawn on the edge of the data scaleView.
				if(!ShouldDrawMarkerOnViewEdgeX())
				{
					// Check for min/max X values
					if(xValue == hAxis.ViewMaximum && ShiftedX >= 0)
					{
						index++;
						continue;
					}

					// Check for min/max X values
					if(xValue == hAxis.ViewMinimum && ShiftedX <= 0)
					{
						index++;
						continue;
					}
				}

				//************************************************************
				//** Get marker position and size
				//************************************************************

				// Get marker position
				PointF markerPosition = PointF.Empty;
				markerPosition.Y = (float)vAxis.GetLinearPosition(yValue);
				if( indexedSeries )
				{
					// The formula for position is based on a distance 
					// from the grid line or nPoints position.
					markerPosition.X = (float)hAxis.GetPosition( (double)index );
				}
				else
				{
					markerPosition.X = (float)hAxis.GetPosition( point.XValue );
				}

				// Labels and markers have to be shifted if there 
				// is more than one series for column chart.
				markerPosition.X += (float)ShiftedX;

				// Get point some point properties and save them in variables
				int			pointMarkerSize = point.MarkerSize;
				string		pointMarkerImage = point.MarkerImage;
				MarkerStyle	pointMarkerStyle = point.MarkerStyle;

				// Get marker size
				SizeF markerSize = GetMarkerSize(
					common.graph, 
					common, 
					area, 
					point, 
					pointMarkerSize, 
					pointMarkerImage);

				// Transform marker position in 3D space
				if(area.Area3DStyle.Enable3D)
				{
					// Get series depth and Z position
					float seriesDepth, seriesZPosition;
					area.GetSeriesZPositionAndDepth(series, out seriesDepth, out seriesZPosition);

					Point3D[]	marker3DPosition = new Point3D[1];
					marker3DPosition[0] = new Point3D(
						markerPosition.X, 
						markerPosition.Y, 
						(float)(seriesZPosition + ((this.middleMarker) ? seriesDepth/2f : seriesDepth)));

					// Transform coordinates
					area.matrix3D.TransformPoints(marker3DPosition);
					markerPosition = marker3DPosition[0].PointF;
				}

				// Check if marker visible
				if(alwaysDrawMarkers || 
					pointMarkerStyle != MarkerStyle.None || 
					pointMarkerImage.Length > 0)
				{
					// Check marker index
					if(markerIndex == 0)
					{
						markerSize = common.graph.GetRelativeSize(markerSize);

						// Add marker position into the list
						RectangleF	markerRect = new RectangleF(
							markerPosition.X - markerSize.Width / 2f,
							markerPosition.Y - markerSize.Height / 2f,
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

		#region 3D Label Info class

		/// <summary>
		/// 3D LabelStyle info.
		/// </summary>
		internal class Label3DInfo
		{
			internal DataPoint3D PointEx = null;
			internal PointF MarkerPosition = PointF.Empty;
			internal SizeF MarkerSize = SizeF.Empty;
		}

		#endregion // 3D Label Info class

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
