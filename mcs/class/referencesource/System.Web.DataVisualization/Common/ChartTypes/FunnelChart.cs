//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		FunnelChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	FunnelChart, PyramidChart, FunnelSegmentInfo, 
//				FunnelPointLabelInfo
//
//  Purpose:    Provides 2D/3D drawing and hit testing functionality 
//              for the Funnel and Pyramid charts. 
//				
//				Funnel and Pyramid Chart types display data that 
//				equals 100% when totalled. This type of chart is a 
//				single series chart representing the data as portions 
//				of 100%, and this chart does not use any axes.
//				
//	Reviewed:	AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
#else
	using System.Web.UI.DataVisualization.Charting.Utilities;
#endif

#endregion // Used namespaces

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.ChartTypes
#else // Microsoft_CONTROL
	namespace System.Web.UI.DataVisualization.Charting.ChartTypes
#endif // Microsoft_CONTROL
{
	#region Enumerations

	/// <summary>
	/// Value type of the pyramid chart.
	/// </summary>
	internal enum PyramidValueType
	{
		/// <summary>
		/// Each point value defines linear height of each segment.
		/// </summary>
		Linear,

		/// <summary>
		/// Each point value defines surface of each segment.
		/// </summary>
		Surface
	}

	/// <summary>
	/// Funnel chart drawing style.
	/// </summary>
    internal enum FunnelStyle
	{
		/// <summary>
		/// Shape of the funnel is fixed and point Y value controls the height of the segments.
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "YIs")] 
		YIsHeight,

		/// <summary>
		/// Height of each segment is the same and point Y value controls the diameter of the segment.
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "YIs")]
        YIsWidth
	}

	/// <summary>
	/// Outside labels placement.
	/// </summary>
    internal enum FunnelLabelPlacement
	{
		/// <summary>
		/// Labels are placed on the right side of the funnel.
		/// </summary>
		Right,

		/// <summary>
		/// Labels are placed on the left side of the funnel.
		/// </summary>
		Left
	}
	
	/// <summary>
	/// Vertical alignment of the data point labels
	/// </summary>
    internal enum FunnelLabelVerticalAlignment
	{
		/// <summary>
		/// Label placed in the middle.
		/// </summary>
		Center,

		/// <summary>
		/// Label placed on top.
		/// </summary>
		Top,

		/// <summary>
		/// Label placed on the bottom.
		/// </summary>
		Bottom
	}

	/// <summary>
	/// Funnel chart 3D drawing style.
	/// </summary>
    internal enum Funnel3DDrawingStyle
	{
		/// <summary>
		/// Circle will be used as a shape of the base.
		/// </summary>
		CircularBase,

		/// <summary>
		/// Square will be used as a shape of the base.
		/// </summary>
		SquareBase
	}


	/// <summary>
	/// Funnel chart labels style enumeration.
	/// </summary>
    internal enum FunnelLabelStyle
	{
		/// <summary>
		/// Data point labels are located inside of the funnel.
		/// </summary>
		Inside,

		/// <summary>
		/// Data point labels are located outside of the funnel.
		/// </summary>
		Outside,

		/// <summary>
		/// Data point labels are located outside of the funnel in a column.
		/// </summary>
		OutsideInColumn,

		/// <summary>
		/// Data point labels are disabled.
		/// </summary>
		Disabled
	}

	#endregion // Enumerations

	/// <summary>
    /// FunnelChart class provides 2D/3D drawing and hit testing functionality 
    /// for the Funnel and Pyramid charts.
	/// </summary>
	internal class FunnelChart : IChartType
	{
		#region Fields and Constructor

		// Array list of funnel segments
        internal ArrayList segmentList = null;

		// List of data point labels information 
        internal ArrayList labelInfoList = null;

		// Chart graphics object.
        internal ChartGraphics Graph { get; set; }

		// Chart area the chart type belongs to.
        internal ChartArea Area { get; set; }

		// Common chart elements.
        internal CommonElements Common { get; set; }
		
		// Spacing between each side of the funnel and chart area.
        internal RectangleF plotAreaSpacing = new RectangleF(3f, 3f, 3f, 3f);

		// Current chart type series
		private Series			_chartTypeSeries = null;

		// Sum of all Y values in the data series
        internal double yValueTotal = 0.0;

		// Maximum Y value in the data series
		private double			_yValueMax = 0.0;

		// Sum of all X values in the data series
		private double			_xValueTotal = 0.0;

		// Number of points in the series
        internal int pointNumber;

		// Calculted plotting area of the chart
        private RectangleF _plotAreaPosition = RectangleF.Empty;

		// Funnel chart drawing style
		private	FunnelStyle		_funnelStyle = FunnelStyle.YIsHeight;

		// Define the shape of the funnel neck
		private	SizeF			_funnelNeckSize = new SizeF(50f, 30f);

		// Gap between funnel segments
        internal float funnelSegmentGap = 0f;

		// 3D funnel rotation angle
		private int				_rotation3D = 5;

		// Indicates that rounded shape is used to draw 3D chart type instead of square
        internal bool round3DShape = true;

		// Indicates that Pyramid chart is rendered.
        internal bool isPyramid = false;

		// Minimum data point height
		private	float			_funnelMinPointHeight = 0f;

		// Name of the attribute that controls the height of the gap between the points
        internal string funnelPointGapAttributeName = CustomPropertyName.FunnelPointGap;

		// Name of the attribute that controls the 3D funnel rotation angle
        internal string funnelRotationAngleAttributeName = CustomPropertyName.Funnel3DRotationAngle;

		// Name of the attribute that controls the minimum height of the point
		protected	string		funnelPointMinHeight = CustomPropertyName.FunnelMinPointHeight;

		// Name of the attribute that controls the minimum height of the point
        internal string funnel3DDrawingStyleAttributeName = CustomPropertyName.Funnel3DDrawingStyle;

		// Name of the attribute that controls inside labels vertical alignment
        internal string funnelInsideLabelAlignmentAttributeName = CustomPropertyName.FunnelInsideLabelAlignment;

		// Name of the attribute that controls outside labels placement (Left vs. Right)
		protected	string		funnelOutsideLabelPlacementAttributeName = CustomPropertyName.FunnelOutsideLabelPlacement;
				
		// Name of the attribute that controls labels style
        internal string funnelLabelStyleAttributeName = CustomPropertyName.FunnelLabelStyle;

		// Array of data point value adjusments in percentage
		private		double[]	_valuePercentages = null;

		/// <summary>
		/// Default constructor
		/// </summary>
		public FunnelChart()
		{
		}

		#endregion

        #region Properties

        /// <summary>
        /// Gets or sets the calculted plotting area of the chart 
        /// </summary>
        internal RectangleF PlotAreaPosition
        {
            get { return _plotAreaPosition; }
            set { _plotAreaPosition = value; }
        }

        #endregion // Properties

        #region IChartType interface implementation

        /// <summary>
		/// Chart type name
		/// </summary>
		virtual public string Name			{ get{ return ChartTypeNames.Funnel;}}

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
		virtual public bool DataPointsInLegend	{ get{ return true;} }

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
		virtual public bool ApplyPaletteColorsToPoints	{ get { return true; } }

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

		#region Painting

		/// <summary>
		/// Paint Funnel Chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		virtual public void Paint( 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			Series seriesToDraw )
		{	
			// Reset fields
			this._chartTypeSeries = null;
			this._funnelMinPointHeight = 0f;
			
			// Save reference to the input parameters 
			this.Graph = graph;
			this.Common = common;
			this.Area = area;

			// Funnel chart like a Pie chart shows each data point as part of the whole (100%).
			// Calculate the sum of all Y and X values, which will be used to calculate point percentage.
			GetDataPointValuesStatistic();

			// Check if there are non-zero points 
			if(this.yValueTotal == 0.0 || this.pointNumber == 0)
			{
				return;
			}

			// When Y value is funnel width at least 2 points required
			this._funnelStyle = GetFunnelStyle( this.GetDataSeries() );
			if(this._funnelStyle == FunnelStyle.YIsWidth && 
				this.pointNumber == 1)
			{
				// At least 2 points required
				return;
			}

			// Get minimum point height
			GetFunnelMinPointHeight( this.GetDataSeries() );

			// Fill list of data point labels information
			this.labelInfoList = CreateLabelsInfoList();

			// Calculate the spacing required for the labels.
			GetPlotAreaSpacing();

			// Draw funnel
			ProcessChartType();

			// Draw data point labels
			DrawLabels();
		}

		/// <summary>
		/// Process chart type drawing.
		/// </summary>
		private void ProcessChartType()
		{
			// Reversed drawing order in 3D with positive rotation angle
			if(this.Area.Area3DStyle.Enable3D && 
				( (this._rotation3D > 0 && !this.isPyramid) || (this._rotation3D < 0 && this.isPyramid) ) )
			{
				this.segmentList.Reverse();
			}

			// Check if series shadow should be drawn separatly
			bool	drawShadowSeparatly = true;
			bool	drawSegmentShadow = (this.Area.Area3DStyle.Enable3D) ? false : true;

			// Process all funnel segments shadows
			Series series = this.GetDataSeries();
			if(drawSegmentShadow &&
				drawShadowSeparatly &&
				series != null && 
				series.ShadowOffset != 0)
			{
				foreach(FunnelSegmentInfo segmentInfo in this.segmentList)
				{
					// Draw funnel segment
					this.DrawFunnelCircularSegment(
						segmentInfo.Point,
						segmentInfo.PointIndex,
						segmentInfo.StartWidth,
						segmentInfo.EndWidth,
						segmentInfo.Location,
						segmentInfo.Height,
						segmentInfo.NothingOnTop,
						segmentInfo.NothingOnBottom,
						false,
						true);
				}

				drawSegmentShadow = false;
			}

			// Process all funnel segments
			foreach(FunnelSegmentInfo segmentInfo in this.segmentList)
			{
				// Draw funnel segment
				this.DrawFunnelCircularSegment(
					segmentInfo.Point,
					segmentInfo.PointIndex,
					segmentInfo.StartWidth,
					segmentInfo.EndWidth,
					segmentInfo.Location,
					segmentInfo.Height,
					segmentInfo.NothingOnTop,
					segmentInfo.NothingOnBottom,
					true,
					drawSegmentShadow);
			}
		}

		/// <summary>
		/// Gets funnel data point segment height and width.
		/// </summary>
		/// <param name="series">Chart type series.</param>
		/// <param name="pointIndex">Data point index in the series.</param>
		/// <param name="location">Segment top location. Bottom location if reversed drawing order.</param>
		/// <param name="height">Returns the height of the segment.</param>
		/// <param name="startWidth">Returns top width of the segment.</param>
		/// <param name="endWidth">Returns botom width of the segment.</param>
		protected virtual void GetPointWidthAndHeight(
			Series series,
			int pointIndex,
			float location,
			out float height, 
			out float startWidth, 
			out float endWidth)
		{
			PointF	pointPositionAbs = PointF.Empty;

			// Get plotting area position in pixels
			RectangleF plotAreaPositionAbs = this.Graph.GetAbsoluteRectangle(this.PlotAreaPosition);

			// Calculate total height of plotting area minus reserved space for the gaps
			float plotAreaHeightAbs = plotAreaPositionAbs.Height - 
				this.funnelSegmentGap * (this.pointNumber - ((ShouldDrawFirstPoint()) ? 1 : 2) );
			if(plotAreaHeightAbs < 0f)
			{
				plotAreaHeightAbs = 0f;
			}

			if( this._funnelStyle == FunnelStyle.YIsWidth )
			{
				// Check if X values are provided
				if(this._xValueTotal == 0.0)
				{
					// Calculate segment height in pixels by deviding 
					// plotting area height by number of points.
					height = plotAreaHeightAbs / (this.pointNumber - 1);
				}
				else
				{
					// Calculate segment height as a part of total Y values in series
					height = (float)(plotAreaHeightAbs * (GetXValue(series.Points[pointIndex]) / this._xValueTotal));
				}

				// Check for minimum segment height
				height = CheckMinHeight(height);

				// Calculate start and end width of the segment based on Y value
				// of previous and current data point.
				startWidth = (float)(plotAreaPositionAbs.Width * (GetYValue(series.Points[pointIndex-1], pointIndex-1) / this._yValueMax));
				endWidth = (float)(plotAreaPositionAbs.Width * (GetYValue(series.Points[pointIndex], pointIndex) / this._yValueMax));

				// Set point position for annotation anchoring
				pointPositionAbs  = new PointF(
					plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f, 
					location + height);
			}
			else if( this._funnelStyle == FunnelStyle.YIsHeight )
			{
				// Calculate segment height as a part of total Y values in series
				height = (float)(plotAreaHeightAbs * (GetYValue(series.Points[pointIndex], pointIndex) / this.yValueTotal));

				// Check for minimum segment height
				height = CheckMinHeight(height);

				// Get intersection point of the horizontal line at the start of the segment
				// with the left pre-defined wall of the funnel.
				PointF startIntersection = ChartGraphics.GetLinesIntersection(
					plotAreaPositionAbs.X, location, 
					plotAreaPositionAbs.Right, location, 
					plotAreaPositionAbs.X, plotAreaPositionAbs.Y, 
					plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f - this._funnelNeckSize.Width / 2f, 
					plotAreaPositionAbs.Bottom - this._funnelNeckSize.Height );

				// Get intersection point of the horizontal line at the end of the segment
				// with the left pre-defined wall of the funnel.
				PointF endIntersection = ChartGraphics.GetLinesIntersection(
					plotAreaPositionAbs.X, location + height, 
					plotAreaPositionAbs.Right, location + height, 
					plotAreaPositionAbs.X, plotAreaPositionAbs.Y, 
					plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f - this._funnelNeckSize.Width / 2f, 
					plotAreaPositionAbs.Bottom - this._funnelNeckSize.Height );

				// Get segment start and end width
				startWidth = (float)( plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f - 
					startIntersection.X) * 2f;
				endWidth = (float)( plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f - 
					endIntersection.X) * 2f;

				// Set point position for annotation anchoring
				pointPositionAbs  = new PointF(
					plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f, 
					location + height / 2f);
			}
			else
			{
                throw (new InvalidOperationException(SR.ExceptionFunnelStyleUnknown(this._funnelStyle.ToString())));
			}

			// Set pre-calculated point position
			series.Points[pointIndex].positionRel = Graph.GetRelativePoint(pointPositionAbs);
		}

		/// <summary>
		/// Checks if first point in the series should be drawn.
		/// When point Y value is used to define the diameter of the funnel
		/// segment 2 points are required to draw 1 segment. In this case first
		/// data point is not drawn.
		/// </summary>
		/// <returns>True if first point in the series should be drawn.</returns>
		protected virtual bool ShouldDrawFirstPoint()
		{
			return ( this._funnelStyle == FunnelStyle.YIsHeight || this.isPyramid);
		}

		/// <summary>
		/// Draws funnel 3D square segment.
		/// </summary>
		/// <param name="point">Data point</param>
		/// <param name="pointIndex">Data point index.</param>
		/// <param name="startWidth">Segment top width.</param>
		/// <param name="endWidth">Segment bottom width.</param>
		/// <param name="location">Segment top location.</param>
		/// <param name="height">Segment height.</param>
		/// <param name="nothingOnTop">True if nothing is on the top of that segment.</param>
		/// <param name="nothingOnBottom">True if nothing is on the bottom of that segment.</param>
		/// <param name="drawSegment">True if segment shadow should be drawn.</param>
		/// <param name="drawSegmentShadow">True if segment shadow should be drawn.</param>
		private void DrawFunnel3DSquareSegment(
			DataPoint point,
			int pointIndex,
			float startWidth, 
			float endWidth,
			float location,
			float height,
			bool nothingOnTop,
			bool nothingOnBottom,
			bool drawSegment,
			bool drawSegmentShadow)
		{
			// Increase the height of the segment to make sure there is no gaps between segments 
			if(!nothingOnBottom)
			{
				height += 0.3f;
			}

			// Get lighter and darker back colors
			Color	lightColor = ChartGraphics.GetGradientColor( point.Color, Color.White, 0.3 );
			Color	darkColor = ChartGraphics.GetGradientColor( point.Color, Color.Black, 0.3 );

			// Segment width can't be smaller than funnel neck width
			if( this._funnelStyle == FunnelStyle.YIsHeight && !this.isPyramid )
			{
				if(startWidth < this._funnelNeckSize.Width)
				{
					startWidth = this._funnelNeckSize.Width;
				}
				if(endWidth < this._funnelNeckSize.Width)
				{
					endWidth = this._funnelNeckSize.Width;
				}
			}

			// Get 3D rotation angle
			float	topRotationHeight = (float)( (startWidth / 2f) * Math.Sin(this._rotation3D / 180F * Math.PI) );
			float	bottomRotationHeight = (float)( (endWidth / 2f) * Math.Sin(this._rotation3D / 180F * Math.PI) );

			// Get plotting area position in pixels
			RectangleF plotAreaPositionAbs = this.Graph.GetAbsoluteRectangle(this.PlotAreaPosition);

			// Get the horizontal center point in pixels
			float	xCenterPointAbs = plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f;

			// Start Svg Selection mode
			this.Graph.StartHotRegion( point );

			// Create segment path
			GraphicsPath segmentPath = new GraphicsPath();

			// Draw left part of the pyramid segment
			// Add top line
			if(startWidth > 0f)
			{
				segmentPath.AddLine(						
					xCenterPointAbs - startWidth / 2f, location,
					xCenterPointAbs, location + topRotationHeight);
			}

			// Add middle line
			segmentPath.AddLine(
				xCenterPointAbs, location + topRotationHeight,
				xCenterPointAbs, location + height + bottomRotationHeight);

			// Add bottom line
			if(endWidth > 0f)
			{
				segmentPath.AddLine(
					xCenterPointAbs, location + height + bottomRotationHeight,
					xCenterPointAbs - endWidth / 2f, location + height);
			}

			// Add left line
			segmentPath.AddLine(
				xCenterPointAbs - endWidth / 2f, location + height,
				xCenterPointAbs - startWidth / 2f, location);

			if( this.Common.ProcessModePaint )
			{
				// Fill graphics path
				this.Graph.DrawPathAbs(
					segmentPath,
					(drawSegment) ? lightColor : Color.Transparent,
					point.BackHatchStyle,
					point.BackImage,
					point.BackImageWrapMode,
					point.BackImageTransparentColor,
					point.BackImageAlignment,
					point.BackGradientStyle,
					(drawSegment) ? point.BackSecondaryColor : Color.Transparent,
					(drawSegment) ? point.BorderColor : Color.Transparent,
					point.BorderWidth,
					point.BorderDashStyle,
					PenAlignment.Center,
					(drawSegmentShadow) ? point.series.ShadowOffset : 0,
					point.series.ShadowColor);
			}

			if( this.Common.ProcessModeRegions )
			{
				// Add hot region
				this.Common.HotRegionsList.AddHotRegion( 
					segmentPath,
					false,
					this.Graph,
					point,
					point.series.Name,
					pointIndex);
			}
			segmentPath.Dispose();



			// Draw right part of the pyramid segment
			// Add top line
			segmentPath = new GraphicsPath();
			if(startWidth > 0f)
			{
				segmentPath.AddLine(						
					xCenterPointAbs + startWidth / 2f, location,
					xCenterPointAbs, location + topRotationHeight);
			}

			// Add middle line
			segmentPath.AddLine(
				xCenterPointAbs, location + topRotationHeight,
				xCenterPointAbs, location + height + bottomRotationHeight);

			// Add bottom line
			if(endWidth > 0f)
			{
				segmentPath.AddLine(
					xCenterPointAbs, location + height + bottomRotationHeight,
					xCenterPointAbs + endWidth / 2f, location + height);
			}

			// Add right line
			segmentPath.AddLine(
				xCenterPointAbs + endWidth / 2f, location + height,
				xCenterPointAbs + startWidth / 2f, location);

			if( this.Common.ProcessModePaint )
			{
				// Fill graphics path
				this.Graph.DrawPathAbs(
					segmentPath,
					(drawSegment) ? darkColor : Color.Transparent,
					point.BackHatchStyle,
					point.BackImage,
					point.BackImageWrapMode,
					point.BackImageTransparentColor,
					point.BackImageAlignment,
					point.BackGradientStyle,
					(drawSegment) ? point.BackSecondaryColor : Color.Transparent,
					(drawSegment) ? point.BorderColor : Color.Transparent,
					point.BorderWidth,
					point.BorderDashStyle,
					PenAlignment.Center,
					(drawSegmentShadow) ? point.series.ShadowOffset : 0,
					point.series.ShadowColor);
			}

			if( this.Common.ProcessModeRegions )
			{
				// Add hot region
				this.Common.HotRegionsList.AddHotRegion( 
					segmentPath,
					false,
					this.Graph,
					point,
					point.series.Name,
					pointIndex);
			}
			segmentPath.Dispose();


			// Add top 3D surface
			if(this._rotation3D > 0f && startWidth > 0f && nothingOnTop)
			{
				if(this.Area.Area3DStyle.Enable3D)
				{
					PointF[] sidePoints = new PointF[4];
					sidePoints[0] = new PointF(xCenterPointAbs + startWidth / 2f, location);
					sidePoints[1] = new PointF(xCenterPointAbs, location + topRotationHeight);
					sidePoints[2] = new PointF(xCenterPointAbs - startWidth / 2f, location);
					sidePoints[3] = new PointF(xCenterPointAbs, location - topRotationHeight);
					GraphicsPath topCurve = new GraphicsPath();
					topCurve.AddLines(sidePoints);
					topCurve.CloseAllFigures();

					if( this.Common.ProcessModePaint )
					{
						// Fill graphics path
						this.Graph.DrawPathAbs(
							topCurve,
							(drawSegment) ? ChartGraphics.GetGradientColor( point.Color, Color.Black, 0.4 ) : Color.Transparent,
							point.BackHatchStyle,
							point.BackImage,
							point.BackImageWrapMode,
							point.BackImageTransparentColor,
							point.BackImageAlignment,
							point.BackGradientStyle,
							(drawSegment) ? point.BackSecondaryColor : Color.Transparent,
							(drawSegment) ? point.BorderColor : Color.Transparent,
							point.BorderWidth,
							point.BorderDashStyle,
							PenAlignment.Center,
							(drawSegmentShadow) ? point.series.ShadowOffset : 0,
							point.series.ShadowColor);
					}

					if( this.Common.ProcessModeRegions )
					{
						// Add hot region
						this.Common.HotRegionsList.AddHotRegion( 
							topCurve,
							false,
							this.Graph,
							point,
							point.series.Name,
							pointIndex);
					}
					topCurve.Dispose();
				}
			}

			// Add bottom 3D surface
			if(this._rotation3D < 0f && startWidth > 0f && nothingOnBottom)
			{
				if(this.Area.Area3DStyle.Enable3D)
				{
					PointF[] sidePoints = new PointF[4];
					sidePoints[0] = new PointF(xCenterPointAbs + endWidth / 2f, location + height);
					sidePoints[1] = new PointF(xCenterPointAbs, location + height + bottomRotationHeight);
					sidePoints[2] = new PointF(xCenterPointAbs - endWidth / 2f, location + height);
					sidePoints[3] = new PointF(xCenterPointAbs, location + height - bottomRotationHeight);
					GraphicsPath topCurve = new GraphicsPath();
					topCurve.AddLines(sidePoints);
					topCurve.CloseAllFigures();

					if( this.Common.ProcessModePaint )
					{
						// Fill graphics path
						this.Graph.DrawPathAbs(
							topCurve,
							(drawSegment) ? ChartGraphics.GetGradientColor( point.Color, Color.Black, 0.4 ) : Color.Transparent,
							point.BackHatchStyle,
							point.BackImage,
							point.BackImageWrapMode,
							point.BackImageTransparentColor,
							point.BackImageAlignment,
							point.BackGradientStyle,
							(drawSegment) ? point.BackSecondaryColor : Color.Transparent,
							(drawSegment) ? point.BorderColor : Color.Transparent,
							point.BorderWidth,
							point.BorderDashStyle,
							PenAlignment.Center,
							(drawSegmentShadow) ? point.series.ShadowOffset : 0,
							point.series.ShadowColor);
					}

					if( this.Common.ProcessModeRegions )
					{
						// Add hot region
						this.Common.HotRegionsList.AddHotRegion( 
							topCurve,
							false,
							this.Graph,
							point,
							point.series.Name,
							pointIndex);
					}
					topCurve.Dispose();

				}
			}

			// End Svg Selection mode
			this.Graph.EndHotRegion( );
		}

		/// <summary>
		/// Draws funnel segment.
		/// </summary>
		/// <param name="point">Data point</param>
		/// <param name="pointIndex">Data point index.</param>
		/// <param name="startWidth">Segment top width.</param>
		/// <param name="endWidth">Segment bottom width.</param>
		/// <param name="location">Segment top location.</param>
		/// <param name="height">Segment height.</param>
		/// <param name="nothingOnTop">True if nothing is on the top of that segment.</param>
		/// <param name="nothingOnBottom">True if nothing is on the bottom of that segment.</param>
		/// <param name="drawSegment">True if segment shadow should be drawn.</param>
		/// <param name="drawSegmentShadow">True if segment shadow should be drawn.</param>
		private void DrawFunnelCircularSegment(
			DataPoint point,
			int pointIndex,
			float startWidth, 
			float endWidth,
			float location,
			float height,
			bool nothingOnTop,
			bool nothingOnBottom,
			bool drawSegment,
			bool drawSegmentShadow)
		{
			PointF	leftSideLinePoint = PointF.Empty;
			PointF	rightSideLinePoint = PointF.Empty;

			// Check if square 3D segment should be drawn
			if(this.Area.Area3DStyle.Enable3D && !round3DShape)
			{
				DrawFunnel3DSquareSegment(
					point,
					pointIndex,
					startWidth, 
					endWidth,
					location,
					height,
					nothingOnTop,
					nothingOnBottom,
					drawSegment,
					drawSegmentShadow);
				return;
			}

			// Increase the height of the segment to make sure there is no gaps between segments 
			if(!nothingOnBottom)
			{
				height += 0.3f;
			}

			// Segment width can't be smaller than funnel neck width
			float	originalStartWidth = startWidth;
			float	originalEndWidth = endWidth;
			if( this._funnelStyle == FunnelStyle.YIsHeight && !this.isPyramid)
			{
				if(startWidth < this._funnelNeckSize.Width)
				{
					startWidth = this._funnelNeckSize.Width;
				}
				if(endWidth < this._funnelNeckSize.Width)
				{
					endWidth = this._funnelNeckSize.Width;
				}
			}

			// Get 3D rotation angle
			float	tension = 0.8f;
			float	topRotationHeight = (float)( (startWidth / 2f) * Math.Sin(this._rotation3D / 180F * Math.PI) );
			float	bottomRotationHeight = (float)( (endWidth / 2f) * Math.Sin(this._rotation3D / 180F * Math.PI) );

			// Get plotting area position in pixels
			RectangleF plotAreaPositionAbs = this.Graph.GetAbsoluteRectangle(this.PlotAreaPosition);

			// Get the horizontal center point in pixels
			float	xCenterPointAbs = plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f;

			// Start Svg Selection mode
			this.Graph.StartHotRegion( point );

			// Create segment path
			GraphicsPath segmentPath = new GraphicsPath();

			// Add top line
			if(startWidth > 0f)
			{
				if(this.Area.Area3DStyle.Enable3D)
				{
					PointF[] sidePoints = new PointF[4];
					sidePoints[0] = new PointF(xCenterPointAbs + startWidth / 2f, location);
					sidePoints[1] = new PointF(xCenterPointAbs, location + topRotationHeight);
					sidePoints[2] = new PointF(xCenterPointAbs - startWidth / 2f, location);
					sidePoints[3] = new PointF(xCenterPointAbs, location - topRotationHeight);
					GraphicsPath topCurve = new GraphicsPath();
					topCurve.AddClosedCurve(sidePoints, tension);
					topCurve.Flatten();
					topCurve.Reverse();

					Graph.AddEllipseSegment(
						segmentPath,
						topCurve,
						null,
						true,
						0f,
						out leftSideLinePoint,
						out rightSideLinePoint);
				}
				else
				{
					segmentPath.AddLine(						
						xCenterPointAbs - startWidth / 2f, location,
						xCenterPointAbs + startWidth / 2f, location);
				}
			}

			// Add right line
			if( this._funnelStyle == FunnelStyle.YIsHeight &&
				!this.isPyramid &&
				startWidth > this._funnelNeckSize.Width &&
				endWidth <= this._funnelNeckSize.Width)
			{
				// Get intersection point of the vertical line at the neck border
				// with the left pre-defined wall of the funnel.
				PointF intersection = ChartGraphics.GetLinesIntersection(
					xCenterPointAbs + this._funnelNeckSize.Width / 2f, plotAreaPositionAbs.Top,
					xCenterPointAbs + this._funnelNeckSize.Width / 2f, plotAreaPositionAbs.Bottom,
					xCenterPointAbs + originalStartWidth / 2f, location, 
					xCenterPointAbs + originalEndWidth / 2f, location + height);

				// Adjust intersection point with top of the neck
				intersection.Y = plotAreaPositionAbs.Bottom - this._funnelNeckSize.Height;

				// Add two segment line
				segmentPath.AddLine(
					xCenterPointAbs + startWidth / 2f, location,
					intersection.X, intersection.Y);
				segmentPath.AddLine(
					intersection.X, intersection.Y,
					intersection.X, location + height);
			}
			else
			{
				// Add straight line
				segmentPath.AddLine(
					xCenterPointAbs + startWidth / 2f, location,
					xCenterPointAbs + endWidth / 2f, location + height);
			}

			// Add bottom line
			if(endWidth > 0f)
			{
				if(this.Area.Area3DStyle.Enable3D)
				{
					PointF[] sidePoints = new PointF[4];
					sidePoints[0] = new PointF(xCenterPointAbs + endWidth / 2f, location + height);
					sidePoints[1] = new PointF(xCenterPointAbs, location + height + bottomRotationHeight);
					sidePoints[2] = new PointF(xCenterPointAbs - endWidth / 2f, location + height);
					sidePoints[3] = new PointF(xCenterPointAbs, location + height - bottomRotationHeight);
					GraphicsPath topCurve = new GraphicsPath();
					topCurve.AddClosedCurve(sidePoints, tension);
					topCurve.Flatten();
					topCurve.Reverse();

                    using (GraphicsPath tmp = new GraphicsPath())
                    {
                        Graph.AddEllipseSegment(
                            tmp,
                            topCurve,
                            null,
                            true,
                            0f,
                            out leftSideLinePoint,
                            out rightSideLinePoint);

                        tmp.Reverse();
                        if (tmp.PointCount > 0)
                        {
                            segmentPath.AddPath(tmp, false);
                        }
                    }
				}
				else
				{
					segmentPath.AddLine(
						xCenterPointAbs + endWidth / 2f, location + height,
						xCenterPointAbs - endWidth / 2f, location + height);
				}
			}

			// Add left line
			if( this._funnelStyle == FunnelStyle.YIsHeight &&
				!this.isPyramid &&
				startWidth > this._funnelNeckSize.Width &&
				endWidth <= this._funnelNeckSize.Width)
			{
				// Get intersection point of the horizontal line at the start of the segment
				// with the left pre-defined wall of the funnel.
				PointF intersection = ChartGraphics.GetLinesIntersection(
					xCenterPointAbs - this._funnelNeckSize.Width / 2f, plotAreaPositionAbs.Top,
					xCenterPointAbs - this._funnelNeckSize.Width / 2f, plotAreaPositionAbs.Bottom,
					xCenterPointAbs - originalStartWidth / 2f, location, 
					xCenterPointAbs - originalEndWidth / 2f, location + height);

				// Adjust intersection point with top of the neck
				intersection.Y = plotAreaPositionAbs.Bottom - this._funnelNeckSize.Height;

				// Add two segment line
				segmentPath.AddLine(
					intersection.X, location + height,
					intersection.X, intersection.Y);
				segmentPath.AddLine(
					intersection.X, intersection.Y,
					xCenterPointAbs - startWidth / 2f, location);
			}
			else
			{
				segmentPath.AddLine(
					xCenterPointAbs - endWidth / 2f, location + height,
					xCenterPointAbs - startWidth / 2f, location);
			}

			if( this.Common.ProcessModePaint )
			{
				// Draw lightStyle source blink effect in 3D
				if(this.Area.Area3DStyle.Enable3D &&
					Graph.ActiveRenderingType == RenderingType.Gdi )
				{
					// Get lighter and darker back colors
					Color	lightColor = ChartGraphics.GetGradientColor( point.Color, Color.White, 0.3 );
					Color	darkColor = ChartGraphics.GetGradientColor( point.Color, Color.Black, 0.3 );

					// Create linear gradient brush
					RectangleF boundsRect = segmentPath.GetBounds();
					if(boundsRect.Width == 0f)
					{
						boundsRect.Width = 1f;
					}
					if(boundsRect.Height == 0f)
					{
						boundsRect.Height = 1f;
					}
					using( LinearGradientBrush brush = new LinearGradientBrush(
							   boundsRect,
							   lightColor, 
							   darkColor,
							   0f) )
					{
						// Set linear gradient brush interpolation colors
						ColorBlend colorBlend = new ColorBlend(5);
						colorBlend.Colors[0] = darkColor;
						colorBlend.Colors[1] = darkColor;
						colorBlend.Colors[2] = lightColor;
						colorBlend.Colors[3] = darkColor;
						colorBlend.Colors[4] = darkColor;

						colorBlend.Positions[0] = 0.0f;
						colorBlend.Positions[1] = 0.0f;
						colorBlend.Positions[2] = 0.5f;
						colorBlend.Positions[3] = 1.0f;
						colorBlend.Positions[4] = 1.0f;

						brush.InterpolationColors = colorBlend;

						// Fill path
						this.Graph.Graphics.FillPath(brush, segmentPath);

						// Draw path border
						Pen pen = new Pen(point.BorderColor, point.BorderWidth);
						pen.DashStyle = this.Graph.GetPenStyle( point.BorderDashStyle );
						if(point.BorderWidth == 0 || 
							point.BorderDashStyle == ChartDashStyle.NotSet || 
							point.BorderColor == Color.Empty)
						{
							// Draw line of the darker color inside the cylinder
							pen = new Pen(ChartGraphics.GetGradientColor( point.Color, Color.Black, 0.3 ), 1);
							pen.Alignment = PenAlignment.Inset;
						}

						pen.StartCap = LineCap.Round;
						pen.EndCap = LineCap.Round;
						pen.LineJoin = LineJoin.Bevel;
						this.Graph.DrawPath(pen, segmentPath );
						pen.Dispose();
					}
				}
				else
				{
					// Fill graphics path
					this.Graph.DrawPathAbs(
						segmentPath,
						(drawSegment) ? point.Color : Color.Transparent,
						point.BackHatchStyle,
						point.BackImage,
						point.BackImageWrapMode,
						point.BackImageTransparentColor,
						point.BackImageAlignment,
						point.BackGradientStyle,
						(drawSegment) ? point.BackSecondaryColor : Color.Transparent,
						(drawSegment) ? point.BorderColor : Color.Transparent,
						point.BorderWidth,
						point.BorderDashStyle,
						PenAlignment.Center,
						(drawSegmentShadow) ? point.series.ShadowOffset : 0,
						point.series.ShadowColor);
				}
			}

			if( this.Common.ProcessModeRegions )
			{
				// Add hot region
				this.Common.HotRegionsList.AddHotRegion( 
					segmentPath,
					false,
					this.Graph,
					point,
					point.series.Name,
					pointIndex);
			}
			segmentPath.Dispose();


			// Add top 3D surface
			if(this._rotation3D > 0f && startWidth > 0f && nothingOnTop)
			{
				if(this.Area.Area3DStyle.Enable3D)
				{
					PointF[] sidePoints = new PointF[4];
					sidePoints[0] = new PointF(xCenterPointAbs + startWidth / 2f, location);
					sidePoints[1] = new PointF(xCenterPointAbs, location + topRotationHeight);
					sidePoints[2] = new PointF(xCenterPointAbs - startWidth / 2f, location);
					sidePoints[3] = new PointF(xCenterPointAbs, location - topRotationHeight);
					GraphicsPath topCurve = new GraphicsPath();
					topCurve.AddClosedCurve(sidePoints, tension);

					if( this.Common.ProcessModePaint )
					{
						// Fill graphics path
						this.Graph.DrawPathAbs(
							topCurve,
							(drawSegment) ? ChartGraphics.GetGradientColor( point.Color, Color.Black, 0.4 ) : Color.Transparent,
							point.BackHatchStyle,
							point.BackImage,
							point.BackImageWrapMode,
							point.BackImageTransparentColor,
							point.BackImageAlignment,
							point.BackGradientStyle,
							(drawSegment) ? point.BackSecondaryColor : Color.Transparent,
							(drawSegment) ? point.BorderColor : Color.Transparent,
							point.BorderWidth,
							point.BorderDashStyle,
							PenAlignment.Center,
							(drawSegmentShadow) ? point.series.ShadowOffset : 0,
							point.series.ShadowColor);
					}

					if( this.Common.ProcessModeRegions )
					{
						// Add hot region
						this.Common.HotRegionsList.AddHotRegion( 
							topCurve,
							false,
							this.Graph,
							point,
							point.series.Name,
							pointIndex);
					}
					topCurve.Dispose();
				}
			}

			// Add bottom 3D surface
			if(this._rotation3D < 0f && startWidth > 0f && nothingOnBottom)
			{
				if(this.Area.Area3DStyle.Enable3D)
				{
					PointF[] sidePoints = new PointF[4];
					sidePoints[0] = new PointF(xCenterPointAbs + endWidth / 2f, location + height);
					sidePoints[1] = new PointF(xCenterPointAbs, location + height + bottomRotationHeight);
					sidePoints[2] = new PointF(xCenterPointAbs - endWidth / 2f, location + height);
					sidePoints[3] = new PointF(xCenterPointAbs, location + height - bottomRotationHeight);
					GraphicsPath topCurve = new GraphicsPath();
					topCurve.AddClosedCurve(sidePoints, tension);

					if( this.Common.ProcessModePaint )
					{
						// Fill graphics path
						this.Graph.DrawPathAbs(
							topCurve,
							(drawSegment) ? ChartGraphics.GetGradientColor( point.Color, Color.Black, 0.4 ) : Color.Transparent,
							point.BackHatchStyle,
							point.BackImage,
							point.BackImageWrapMode,
							point.BackImageTransparentColor,
							point.BackImageAlignment,
							point.BackGradientStyle,
							(drawSegment) ? point.BackSecondaryColor : Color.Transparent,
							(drawSegment) ? point.BorderColor : Color.Transparent,
							point.BorderWidth,
							point.BorderDashStyle,
							PenAlignment.Center,
							(drawSegmentShadow) ? point.series.ShadowOffset : 0,
							point.series.ShadowColor);
					}

					if( this.Common.ProcessModeRegions )
					{
						// Add hot region
						this.Common.HotRegionsList.AddHotRegion( 
							topCurve,
							false,
							this.Graph,
							point,
							point.series.Name,
							pointIndex);
					}
					topCurve.Dispose();

				}
			}
		
			// End Svg Selection mode
			this.Graph.EndHotRegion( );
		}


		/// <summary>
		/// Fill list with information about every segment of the funnel.
		/// </summary>
		/// <returns>Funnel segment information list.</returns>
		private ArrayList GetFunnelSegmentPositions()
		{
			// Create new list
			ArrayList list = new ArrayList();

			// Funnel chart process only first series in the chart area
			// and cannot be combined with any other chart types.
			Series series = GetDataSeries();
			if( series != null )
			{
				// Get funnel drawing style 
				this._funnelStyle = GetFunnelStyle(series);

				// Check if round or square base is used in 3D chart
				this.round3DShape = (GetFunnel3DDrawingStyle(series) == Funnel3DDrawingStyle.CircularBase);

				// Get funnel points gap
				this.funnelSegmentGap = GetFunnelPointGap(series);

				// Get funnel neck size
				this._funnelNeckSize = GetFunnelNeckSize(series);

				// Loop through all ponts in the data series
				float	currentLocation = this.Graph.GetAbsolutePoint(this.PlotAreaPosition.Location).Y;
				if(this.isPyramid)
				{
					// Pyramid is drawn in reversed order. 
					currentLocation = this.Graph.GetAbsoluteRectangle(this.PlotAreaPosition).Bottom;
				}
				for( int pointIndex = 0; pointIndex >= 0 && pointIndex < series.Points.Count; pointIndex += 1 )
				{
					DataPoint point = series.Points[pointIndex];

					// Check if first data point should be drawn
					if( pointIndex > 0 || ShouldDrawFirstPoint() )
					{
						// Get height and width of each data point segment
						float startWidth = 0f;
						float endWidth = 0f;
						float height = 0f;
						GetPointWidthAndHeight(
							series, 
							pointIndex, 
							currentLocation,
							out height, 
							out startWidth, 
							out endWidth);

						// Check visibility of previous and next points
						bool nothingOnTop = false;
						bool nothingOnBottom = false;
						if(this.funnelSegmentGap > 0)
						{
							nothingOnTop = true;
							nothingOnBottom = true;
						}
						else
						{
							if(ShouldDrawFirstPoint())
							{
								if(pointIndex == 0 ||
									series.Points[pointIndex-1].Color.A != 255)
								{
									if(this.isPyramid)
									{
										nothingOnBottom = true;
									}
									else
									{
										nothingOnTop = true;
									}
								}
							}
							else
							{
								if(pointIndex == 1 ||
									series.Points[pointIndex-1].Color.A != 255)
								{
									if(this.isPyramid)
									{
										nothingOnBottom = true;
									}
									else
									{
										nothingOnTop = true;
									}
								}
							}
							if( pointIndex == series.Points.Count - 1)
							{
								if(this.isPyramid)
								{
									nothingOnTop = true;
								}
								else
								{
									nothingOnBottom = true;
								}
							}
							else if(series.Points[pointIndex+1].Color.A != 255)
							{
								if(this.isPyramid)
								{
									nothingOnTop = true;
								}
								else
								{
									nothingOnBottom = true;
								}
							}
						}

						// Add segment information
						FunnelSegmentInfo info = new FunnelSegmentInfo();
						info.Point = point;
						info.PointIndex = pointIndex;
						info.StartWidth = startWidth;
						info.EndWidth = endWidth;
						info.Location = (this.isPyramid) ? currentLocation - height : currentLocation;
						info.Height = height;
						info.NothingOnTop = nothingOnTop;
						info.NothingOnBottom = nothingOnBottom;
						list.Add(info);

						// Increase current Y location 
						if(this.isPyramid)
						{
							currentLocation -= height + this.funnelSegmentGap;							
						}
						else
						{
							currentLocation += height + this.funnelSegmentGap;
						}
					}
				}
			}

			return list;
		}

		#endregion

		#region Labels Methods

		/// <summary>
		/// Draws funnel data point labels.
		/// </summary>
		private void DrawLabels()
		{
			// Loop through all labels
			foreach(FunnelPointLabelInfo labelInfo in this.labelInfoList)
			{
				if(!labelInfo.Position.IsEmpty &&
					!float.IsNaN(labelInfo.Position.X) &&
					!float.IsNaN(labelInfo.Position.Y) &&
					!float.IsNaN(labelInfo.Position.Width) &&
					!float.IsNaN(labelInfo.Position.Height) )
				{
					// Start Svg Selection mode
					this.Graph.StartHotRegion( labelInfo.Point );

					// Get size of a single character used for spacing
					SizeF spacing = this.Graph.MeasureString(
						"W",
						labelInfo.Point.Font,
						new SizeF(1000f, 1000F),
						StringFormat.GenericTypographic );

					// Draw a callout line
					if( !labelInfo.CalloutPoint1.IsEmpty &&
						!labelInfo.CalloutPoint2.IsEmpty &&
						!float.IsNaN(labelInfo.CalloutPoint1.X) &&
						!float.IsNaN(labelInfo.CalloutPoint1.Y) &&
						!float.IsNaN(labelInfo.CalloutPoint2.X) &&
						!float.IsNaN(labelInfo.CalloutPoint2.Y) )
					{
						// Add spacing between text and callout line
						if(labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
						{
							labelInfo.CalloutPoint2.X -= spacing.Width / 2f;

							// Add a small spacing between a callout line and a segment
							labelInfo.CalloutPoint1.X += 2;
						}
						else
						{
							labelInfo.CalloutPoint2.X += spacing.Width / 2f;

							// Add a small spacing between a callout line and a segment
							labelInfo.CalloutPoint1.X += 2;
						}

						// Get callout line color
						Color lineColor = GetCalloutLineColor(labelInfo.Point);

						// Draw callout line
						this.Graph.DrawLineAbs(
							lineColor,
							1,
							ChartDashStyle.Solid,
							labelInfo.CalloutPoint1,
							labelInfo.CalloutPoint2 );

					}

					// Get label background position
					RectangleF labelBackPosition = labelInfo.Position;
					labelBackPosition.Inflate(spacing.Width / 2f, spacing.Height / 8f);
					labelBackPosition = this.Graph.GetRelativeRectangle(labelBackPosition);

					// Center label in the middle of the background rectangle
                    using (StringFormat format = new StringFormat())
                    {
                        format.Alignment = StringAlignment.Center;
                        format.LineAlignment = StringAlignment.Center;

                        // Draw label text
                        using (Brush brush = new SolidBrush(labelInfo.Point.LabelForeColor))
                        {

                            this.Graph.DrawPointLabelStringRel(
                                this.Common,
                                labelInfo.Text,
                                labelInfo.Point.Font,
                                brush,
                                labelBackPosition,
                                format,
                                labelInfo.Point.LabelAngle,
                                labelBackPosition,

                                labelInfo.Point.LabelBackColor,
                                labelInfo.Point.LabelBorderColor,
                                labelInfo.Point.LabelBorderWidth,
                                labelInfo.Point.LabelBorderDashStyle,
                                labelInfo.Point.series,
                                labelInfo.Point,
                                labelInfo.PointIndex);
                        }

                        // End Svg Selection mode
                        this.Graph.EndHotRegion();
                    }
				}
			}
		}

		/// <summary>
		/// Creates a list of structures with the data point labels information.
		/// </summary>
		/// <returns>Array list of labels information.</returns>
		private ArrayList CreateLabelsInfoList()
		{
			ArrayList list = new ArrayList();

			// Get area position in pixels
			RectangleF plotAreaPositionAbs = this.Graph.GetAbsoluteRectangle( this.Area.Position.ToRectangleF() );

			// Get funnel chart type series
			Series series = GetDataSeries();
			if( series != null )
			{
				// Loop through all ponts in the data series
				int pointIndex = 0;
				foreach( DataPoint point in series.Points )
				{
					// Ignore empty points
					if( !point.IsEmpty )
					{
						// Get some properties for performance
						string	pointLabel = point.Label;
						bool	pointShowLabelAsValue = point.IsValueShownAsLabel;

						// Check if label text exists
						if(pointShowLabelAsValue || pointLabel.Length > 0)
						{
							// Create new point label information class
							FunnelPointLabelInfo labelInfo = new FunnelPointLabelInfo();
							labelInfo.Point = point;
							labelInfo.PointIndex = pointIndex;

							// Get point label text
							if( pointLabel.Length == 0 )
							{
								labelInfo.Text = ValueConverter.FormatValue(
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
								labelInfo.Text = point.ReplaceKeywords(pointLabel);
							}

							// Get label style
							labelInfo.Style = GetLabelStyle(point);

							// Get inside label vertical alignment
							if(labelInfo.Style == FunnelLabelStyle.Inside)
							{
								labelInfo.VerticalAlignment = GetInsideLabelAlignment(point);
							}

							// Get outside labels placement
							if(labelInfo.Style != FunnelLabelStyle.Inside)
							{
								labelInfo.OutsidePlacement = GetOutsideLabelPlacement(point);
							}

							// Measure string size
							labelInfo.Size = this.Graph.MeasureString(
								labelInfo.Text,
								point.Font,
								plotAreaPositionAbs.Size,
								StringFormat.GenericTypographic);
							
							// Add label information into the list
							if(labelInfo.Text.Length > 0 &&
								labelInfo.Style != FunnelLabelStyle.Disabled)
							{
								list.Add(labelInfo);
							}
						}
					}
					++pointIndex;
				}
			}
			return list;
		}

		/// <summary>
		/// Changes required plotting area spacing, so that all labels fit.
		/// </summary>
		/// <returns>Return True if no resizing required.</returns>
		private bool FitPointLabels()
		{
			// Convert plotting area position to pixels.
			// Make rectangle 4 pixels smaller on each side.
			RectangleF plotAreaPositionAbs = this.Graph.GetAbsoluteRectangle(PlotAreaPosition);
			plotAreaPositionAbs.Inflate(-4f, -4f);

			// Get position of each label
			GetLabelsPosition();

			// Get spacing required to draw labels
			RectangleF requiredSpacing = this.Graph.GetAbsoluteRectangle( new RectangleF(1f, 1f, 1f, 1f) );
			foreach(FunnelPointLabelInfo labelInfo in this.labelInfoList)
			{
				// Add additional horizontal spacing for outside labels
				RectangleF	position = labelInfo.Position;
				if(labelInfo.Style == FunnelLabelStyle.Outside ||
					labelInfo.Style == FunnelLabelStyle.OutsideInColumn)
				{
					float spacing = 10f;
					if(labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
					{
						position.Width += spacing;
					}
					else if(labelInfo.OutsidePlacement == FunnelLabelPlacement.Left)
					{
						position.X -= spacing;
						position.Width += spacing;
					}
				}

				// Horizontal coordinates are ignored for Inside label style
				if(labelInfo.Style != FunnelLabelStyle.Inside)
				{
					if( (plotAreaPositionAbs.X - position.X) > requiredSpacing.X )
					{
						requiredSpacing.X = plotAreaPositionAbs.X - position.X;
					}

					if( (position.Right - plotAreaPositionAbs.Right) > requiredSpacing.Width )
					{
						requiredSpacing.Width = position.Right - plotAreaPositionAbs.Right;
					}
				}

				// Vertical spacing
				if( (plotAreaPositionAbs.Y - position.Y) > requiredSpacing.Y )
				{
					requiredSpacing.Y = plotAreaPositionAbs.Y - position.Y;
				}

				if( (position.Bottom - plotAreaPositionAbs.Bottom) > requiredSpacing.Height )
				{
					requiredSpacing.Height = position.Bottom - plotAreaPositionAbs.Bottom;
				}
			}

			// Convert spacing rectangle to relative coordinates
			requiredSpacing = this.Graph.GetRelativeRectangle(requiredSpacing);

			// Check if non-default spacing was used
			if(requiredSpacing.X > 1f ||
				requiredSpacing.Y > 1f ||
				requiredSpacing.Width > 1f ||
				requiredSpacing.Height > 1f )
			{
				this.plotAreaSpacing = requiredSpacing;

				// Get NEW plotting area position
				this.PlotAreaPosition = GetPlotAreaPosition();

				// Get NEW list of segments
				this.segmentList = GetFunnelSegmentPositions();

				// Get NEW position of each label
				GetLabelsPosition();

				return false;
			}

			return true;
		}

		/// <summary>
		/// Loops through the point labels list and calculates labels position
		/// based on their size, position and funnel chart shape.
		/// </summary>
		private void GetLabelsPosition()
		{
			// Convert plotting area position to pixels
			RectangleF plotAreaPositionAbs = this.Graph.GetAbsoluteRectangle(PlotAreaPosition);
			float plotAreaCenterXAbs = plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f;

			// Define label spacing
			SizeF labelSpacing = new SizeF(3f, 3f);

			//Loop through all labels
			foreach(FunnelPointLabelInfo labelInfo in this.labelInfoList)
			{
				// Get ----osiated funnel segment information
				bool	lastLabel = false;
				int pointIndex = labelInfo.PointIndex + ((ShouldDrawFirstPoint()) ? 0 : 1);
				if(pointIndex > this.segmentList.Count && !ShouldDrawFirstPoint() )
				{
					// Use last point index if first point is not drawn
					pointIndex = this.segmentList.Count;
					lastLabel = true;
				}
				FunnelSegmentInfo segmentInfo = null;
				foreach(FunnelSegmentInfo info in this.segmentList)
				{
					if(info.PointIndex == pointIndex)
					{
						segmentInfo = info;
						break;
					}
				}

				// Check if segment was found
				if(segmentInfo != null)
				{
					// Set label width and height
					labelInfo.Position.Width = labelInfo.Size.Width;
					labelInfo.Position.Height = labelInfo.Size.Height;

					//******************************************************
					//** Labels are placed OUTSIDE of the funnel
					//******************************************************
					if(labelInfo.Style == FunnelLabelStyle.Outside ||
						labelInfo.Style == FunnelLabelStyle.OutsideInColumn)
					{
						// Define position
						if( this._funnelStyle == FunnelStyle.YIsHeight )
						{
							// Get segment top and bottom diameter
							float topDiameter = segmentInfo.StartWidth;
							float bottomDiameter = segmentInfo.EndWidth;
							if(!this.isPyramid)
							{
								if(topDiameter < this._funnelNeckSize.Width)
								{
									topDiameter = this._funnelNeckSize.Width;
								}
								if(bottomDiameter < this._funnelNeckSize.Width)
								{
									bottomDiameter = this._funnelNeckSize.Width;
								}

								// Adjust label position because segment is bent to make a neck
								if(segmentInfo.StartWidth >= this._funnelNeckSize.Width &&
									segmentInfo.EndWidth < this._funnelNeckSize.Width)
								{
									bottomDiameter = segmentInfo.EndWidth;
								}
							}
							
							// Get Y position
							labelInfo.Position.Y = (segmentInfo.Location + segmentInfo.Height / 2f) - 
								labelInfo.Size.Height / 2f;

							// Get X position
							if(labelInfo.Style == FunnelLabelStyle.OutsideInColumn)
							{
								if(labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
								{
									labelInfo.Position.X = plotAreaPositionAbs.Right + 
										4f * labelSpacing.Width;

									// Set callout line coordinates
									if(!this.isPyramid)
									{
										labelInfo.CalloutPoint1.X = plotAreaCenterXAbs + 
											Math.Max(this._funnelNeckSize.Width/2f, (topDiameter + bottomDiameter) / 4f);
									}
									else
									{
										labelInfo.CalloutPoint1.X = plotAreaCenterXAbs + 
											(topDiameter + bottomDiameter) / 4f;
									}
									labelInfo.CalloutPoint2.X = labelInfo.Position.X;
								}
								else
								{
									labelInfo.Position.X = plotAreaPositionAbs.X - 
										labelInfo.Size.Width -
										4f * labelSpacing.Width;
									
									// Set callout line coordinates
									if(!this.isPyramid)
									{
										labelInfo.CalloutPoint1.X = plotAreaCenterXAbs - 
											Math.Max(this._funnelNeckSize.Width/2f, (topDiameter + bottomDiameter) / 4f);
									}
									else
									{
										labelInfo.CalloutPoint1.X = plotAreaCenterXAbs - 
											(topDiameter + bottomDiameter) / 4f;
									}
									labelInfo.CalloutPoint2.X = labelInfo.Position.Right;
								}

								// Fill rest of coordinates required for the callout line
								labelInfo.CalloutPoint1.Y = segmentInfo.Location + segmentInfo.Height / 2f;
								labelInfo.CalloutPoint2.Y = labelInfo.CalloutPoint1.Y;
							}
							else
							{
								if(labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
								{
									labelInfo.Position.X = plotAreaCenterXAbs + 
										(topDiameter + bottomDiameter) / 4f + 
										4f * labelSpacing.Width;
								}
								else
								{
									labelInfo.Position.X = plotAreaCenterXAbs - 
										labelInfo.Size.Width -
										(topDiameter + bottomDiameter) / 4f - 
										4f * labelSpacing.Width;
								}
							}
						}
						else
						{
							// Use bottom part of the segment for the last point
							if(lastLabel)
							{
								if(labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
								{
									labelInfo.Position.X = plotAreaCenterXAbs + 
										segmentInfo.EndWidth / 2f + 
										4f * labelSpacing.Width;
								}
								else
								{
									labelInfo.Position.X = plotAreaCenterXAbs - 
										labelInfo.Size.Width -
										segmentInfo.EndWidth / 2f - 
										4f * labelSpacing.Width;
								}
								labelInfo.Position.Y = segmentInfo.Location + 
									segmentInfo.Height - 
									labelInfo.Size.Height / 2f;
							}
							else
							{
								if(labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
								{
									labelInfo.Position.X = plotAreaCenterXAbs + 
										segmentInfo.StartWidth / 2f + 
										4f * labelSpacing.Width;
								}
								else
								{
									labelInfo.Position.X = plotAreaCenterXAbs -
										labelInfo.Size.Width -
										segmentInfo.StartWidth / 2f -
										4f * labelSpacing.Width;
								}
								labelInfo.Position.Y = segmentInfo.Location - 
									labelInfo.Size.Height / 2f;
							}

							if(labelInfo.Style == FunnelLabelStyle.OutsideInColumn)
							{
								if(labelInfo.OutsidePlacement == FunnelLabelPlacement.Right)
								{
									labelInfo.Position.X = plotAreaPositionAbs.Right + 
										4f * labelSpacing.Width;

									// Set callout line coordinates
									labelInfo.CalloutPoint1.X = plotAreaCenterXAbs + 
										( (lastLabel) ? segmentInfo.EndWidth : segmentInfo.StartWidth) / 2f;
									labelInfo.CalloutPoint2.X = labelInfo.Position.X;

								}
								else
								{
									labelInfo.Position.X = plotAreaPositionAbs.X -
										labelInfo.Size.Width -
										4f * labelSpacing.Width;

									// Set callout line coordinates
									labelInfo.CalloutPoint1.X = plotAreaCenterXAbs -
										( (lastLabel) ? segmentInfo.EndWidth : segmentInfo.StartWidth) / 2f;
									labelInfo.CalloutPoint2.X = labelInfo.Position.Right;
								}

								// Fill rest of coordinates required for the callout line
								labelInfo.CalloutPoint1.Y = segmentInfo.Location;
								if(lastLabel)
								{
									labelInfo.CalloutPoint1.Y += segmentInfo.Height;
								}
								labelInfo.CalloutPoint2.Y = labelInfo.CalloutPoint1.Y;

							}
						}
					}

					//******************************************************
					//** Labels are placed INSIDE of the funnel
					//******************************************************
					else if(labelInfo.Style == FunnelLabelStyle.Inside)
					{
						// Define position
						labelInfo.Position.X = plotAreaCenterXAbs - labelInfo.Size.Width / 2f;
						if( this._funnelStyle == FunnelStyle.YIsHeight )
						{
							labelInfo.Position.Y = (segmentInfo.Location + segmentInfo.Height / 2f) - 
								labelInfo.Size.Height / 2f;
							if(labelInfo.VerticalAlignment == FunnelLabelVerticalAlignment.Top)
							{
								labelInfo.Position.Y -= segmentInfo.Height / 2f - labelInfo.Size.Height / 2f - labelSpacing.Height;
							}
							else if(labelInfo.VerticalAlignment == FunnelLabelVerticalAlignment.Bottom)
							{
								labelInfo.Position.Y += segmentInfo.Height / 2f - labelInfo.Size.Height / 2f - labelSpacing.Height;
							}
						}
						else
						{
							labelInfo.Position.Y = segmentInfo.Location - labelInfo.Size.Height / 2f;
							if(labelInfo.VerticalAlignment == FunnelLabelVerticalAlignment.Top)
							{
								labelInfo.Position.Y -= labelInfo.Size.Height / 2f + labelSpacing.Height;
							}
							else if(labelInfo.VerticalAlignment == FunnelLabelVerticalAlignment.Bottom)
							{
								labelInfo.Position.Y += labelInfo.Size.Height / 2f + labelSpacing.Height;
							}

							// Use bottom part of the segment for the last point
							if(lastLabel)
							{
								labelInfo.Position.Y += segmentInfo.Height;
							}
						}

						// Adjust label Y position in 3D
						if(this.Area.Area3DStyle.Enable3D)
						{
							labelInfo.Position.Y += (float)( ( (segmentInfo.EndWidth + segmentInfo.StartWidth) / 4f) * Math.Sin(this._rotation3D / 180F * Math.PI) );
						}
					}
				
					//******************************************************
					//** Check if label overlaps any previous label
					//******************************************************
					int interation = 0;
					while( IsLabelsOverlap(labelInfo) && interation < 1000)
					{
						float	shiftSize = (this.isPyramid) ? -3f : 3f;

						// Move label down
						labelInfo.Position.Y += shiftSize;

						// Move callout second point down
						if(!labelInfo.CalloutPoint2.IsEmpty)
						{
							labelInfo.CalloutPoint2.Y += shiftSize;
						}

						++interation;
					}
				
				}
			}
		}

		/// <summary>
		/// Checks if specified label overlaps any previous labels.
		/// </summary>
		/// <param name="testLabelInfo">Label to test.</param>
		/// <returns>True if labels overlapp.</returns>
		private bool IsLabelsOverlap(FunnelPointLabelInfo testLabelInfo)
		{
			// Increase rectangle size by 1 pixel
			RectangleF rect = testLabelInfo.Position;
			rect.Inflate(1f, 1f);

			// Increase label rectangle if border is drawn around the label
			if(!testLabelInfo.Point.LabelBackColor.IsEmpty ||
				(testLabelInfo.Point.LabelBorderWidth > 0 && 
				!testLabelInfo.Point.LabelBorderColor.IsEmpty &&
				testLabelInfo.Point.LabelBorderDashStyle != ChartDashStyle.NotSet) )
			{
				rect.Inflate(4f, 4f);
			}

			//Loop through all labels
			foreach(FunnelPointLabelInfo labelInfo in this.labelInfoList)
			{
				// Stop searching
				if(labelInfo.PointIndex == testLabelInfo.PointIndex)
				{
					break;
				}

				// Check if label position overlaps
				if(!labelInfo.Position.IsEmpty && 
					labelInfo.Position.IntersectsWith(rect) )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets label style of the data point.
		/// </summary>
		/// <returns>Label style of the data point.</returns>
        private FunnelLabelStyle GetLabelStyle(DataPointCustomProperties properties)
		{
			// Set default label style
			FunnelLabelStyle labelStyle = FunnelLabelStyle.OutsideInColumn;

			// Get string value of the custom attribute
			string attrValue = properties[this.funnelLabelStyleAttributeName];
			if(attrValue != null && attrValue.Length > 0)
			{
				// Convert string to the labels style
				try
				{
					labelStyle = (FunnelLabelStyle)Enum.Parse(typeof(FunnelLabelStyle), attrValue, true);
				}
				catch
				{
					throw(new InvalidOperationException( SR.ExceptionCustomAttributeValueInvalid(labelStyle.ToString(), this.funnelLabelStyleAttributeName) ) );
				}
			}
			return labelStyle;
		}

		#endregion // Labels Methods

		#region Position Methods

		/// <summary>
		/// Calculate the spacing required for the labels.
		/// </summary>
		private void GetPlotAreaSpacing()
		{
			// Provide small spacing on the sides of chart area
			this.plotAreaSpacing = new RectangleF(1f, 1f, 1f, 1f);

			// Get plotting area position
			this.PlotAreaPosition = GetPlotAreaPosition();

			// Get list of segments
			this.segmentList = GetFunnelSegmentPositions();

			// If plotting area position is automatic
			if( Area.InnerPlotPosition.Auto )
			{
				// Set a position so that data labels fit
				// This method is called several time to adjust label position while 
				// funnel side angle is changed
				int iteration = 0;
				while(!FitPointLabels() && iteration < 5)
				{
					iteration++;
				}
			}
			else
			{
				// Just get labels position
				GetLabelsPosition();
			}

		}

		/// <summary>
		/// Gets a rectangle in relative coordinates where the funnel will chart
		/// will be drawn.
		/// </summary>
		/// <returns>Plotting are of the chart in relative coordinates.</returns>
		private RectangleF GetPlotAreaPosition()
		{
			// Get plotting area rectangle position
			RectangleF	plotAreaPosition = ( Area.InnerPlotPosition.Auto ) ? 
				Area.Position.ToRectangleF() : Area.PlotAreaPosition.ToRectangleF();

			// NOTE: Fixes issue #4085
			// Do not allow decreasing of the plot area height more than 50%
			if(plotAreaSpacing.Y > plotAreaPosition.Height / 2f)
			{
				plotAreaSpacing.Y = plotAreaPosition.Height / 2f;
			}
			if(plotAreaSpacing.Height > plotAreaPosition.Height / 2f)
			{
				plotAreaSpacing.Height = plotAreaPosition.Height / 2f;
			}

			// Decrease plotting are position using pre-calculated ratio
			plotAreaPosition.X += plotAreaSpacing.X;
			plotAreaPosition.Y += plotAreaSpacing.Y;
			plotAreaPosition.Width -= plotAreaSpacing.X + plotAreaSpacing.Width;
			plotAreaPosition.Height -= plotAreaSpacing.Y + plotAreaSpacing.Height;

			// Apply vertical spacing on top and bottom to fit the 3D surfaces
			if(this.Area.Area3DStyle.Enable3D)
			{
				// Convert position to pixels
				RectangleF plotAreaPositionAbs = this.Graph.GetAbsoluteRectangle(plotAreaPosition);

				// Funnel chart process only first series in the chart area
				// and cannot be combined with any other chart types.
				Series series = GetDataSeries();
				if( series != null )
				{
					// Get 3D funnel rotation angle (from 10 to -10)
					this._rotation3D = GetFunnelRotation(series);
				}

				// Get top and bottom spacing
				float	topSpacing = (float)Math.Abs( (plotAreaPositionAbs.Width/ 2f) * Math.Sin(this._rotation3D / 180F * Math.PI) );
				float	bottomSpacing = (float)Math.Abs( (plotAreaPositionAbs.Width/ 2f) * Math.Sin(this._rotation3D / 180F * Math.PI) );

				// Adjust position
				if(this.isPyramid)
				{
					// Only bottom spacing for the pyramid
					plotAreaPositionAbs.Height -= bottomSpacing;
				}
				else
				{
					// Add top/bottom spacing
					plotAreaPositionAbs.Y += topSpacing;
					plotAreaPositionAbs.Height -= topSpacing + bottomSpacing;
				}

				// Convert position back to relative coordinates
				plotAreaPosition = this.Graph.GetRelativeRectangle(plotAreaPositionAbs);
			}

			return plotAreaPosition;
		}

		#endregion // Position Methods

		#region Helper Methods

		/// <summary>
		/// Checks for minimum segment height.
		/// </summary>
		/// <param name="height">Current segment height.</param>
		/// <returns>Adjusted segment height.</returns>
		protected float CheckMinHeight(float height)
		{
			// When point gap is used do not allow to have the segment heigth to be zero.
			float minSize = Math.Min(2f, this.funnelSegmentGap / 2f);
			if(this.funnelSegmentGap > 0 && 
				height < minSize)
			{
				return minSize;
			}

			return height;
		}

		/// <summary>
		/// Gets minimum point height in pixels.
		/// </summary>
		/// <returns>Minimum point height in pixels.</returns>
        private void GetFunnelMinPointHeight(DataPointCustomProperties properties)
		{
			// Set default minimum point size
			this._funnelMinPointHeight = 0f;

			// Get string value of the custom attribute
            string attrValue = properties[this.funnelPointMinHeight];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the point gap size

                float pointHeight;
                bool parseSucceed = float.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out pointHeight);
                if (parseSucceed)
                {
                    this._funnelMinPointHeight = pointHeight;
                }

                if (!parseSucceed || this._funnelMinPointHeight < 0f || this._funnelMinPointHeight > 100f)
                {
                    throw (new InvalidOperationException(SR.ExceptionFunnelMinimumPointHeightAttributeInvalid));
                }

                // Check if specified value is too big
                this._funnelMinPointHeight = (float)(this.yValueTotal * this._funnelMinPointHeight / 100f);

                // Get data statistic again using Min value
                GetDataPointValuesStatistic();
            }

			return;
		}

		/// <summary>
		/// Gets 3D funnel rotation angle.
		/// </summary>
		/// <returns>Rotation angle.</returns>
        private int GetFunnelRotation(DataPointCustomProperties properties)
		{
			// Set default gap size
			int	angle = 5;

			// Get string value of the custom attribute
			string attrValue = properties[this.funnelRotationAngleAttributeName];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the point gap size

                int a;
                bool parseSucceed = int.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out a);
                if (parseSucceed)
                {
                    angle = a;
                }

                // Validate attribute value
                if (!parseSucceed || angle < -10 || angle > 10)
                {
                    throw (new InvalidOperationException(SR.ExceptionFunnelAngleRangeInvalid));
                }
            }

			return angle;
		}

		/// <summary>
		/// Gets callout line color.
		/// </summary>
		/// <returns>Callout line color.</returns>
        private Color GetCalloutLineColor(DataPointCustomProperties properties)
		{
			// Set default gap size
			Color	color = Color.Black;

			// Get string value of the custom attribute
			string attrValue = properties[CustomPropertyName.CalloutLineColor];
			if(attrValue != null && attrValue.Length > 0)
			{
				// Convert string to Color
				bool	failed = false;
				ColorConverter colorConverter = new ColorConverter();
                try
                {
                    color = (Color)colorConverter.ConvertFromInvariantString(attrValue);
                }
                catch (ArgumentException)
                {
                    failed = true;
                }
                catch (NotSupportedException)
                {
                    failed = true;
                }

				// In case of an error try to convert using local settings
				if(failed)
				{
					try
					{
						color = (Color)colorConverter.ConvertFromString(attrValue);
					}
					catch(ArgumentException)
					{
						throw(new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid( attrValue, "CalloutLineColor") ) );
					}
				}
				
			}

			return color;
		}

		/// <summary>
		/// Gets funnel neck size when shape of the funnel do not change.
		/// </summary>
		/// <returns>Funnel neck width and height.</returns>
        private SizeF GetFunnelNeckSize(DataPointCustomProperties properties)
		{
			// Set default gap size
			SizeF	neckSize = new SizeF(5f, 5f);

			// Get string value of the custom attribute
			string attrValue = properties[CustomPropertyName.FunnelNeckWidth];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the point gap size

                float w;
                bool parseSucceed = float.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out w);
                if (parseSucceed)
                {
                    neckSize.Width = w;
                }

                // Validate attribute value
                if (!parseSucceed || neckSize.Width < 0 || neckSize.Width > 100)
                {
                    throw (new InvalidOperationException(SR.ExceptionFunnelNeckWidthInvalid));
                }
            }

			// Get string value of the custom attribute
			attrValue = properties[CustomPropertyName.FunnelNeckHeight];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the point gap size
                float h;
                bool parseSucceed = float.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out h);
                if (parseSucceed)
                {
                    neckSize.Height = h;
                }


                if (!parseSucceed || neckSize.Height < 0 || neckSize.Height > 100)
                {
                    throw (new InvalidOperationException(SR.ExceptionFunnelNeckHeightInvalid));
                }
            }

			// Make sure the neck size do not exceed the plotting area size
			if(neckSize.Height > this.PlotAreaPosition.Height/2f)
			{
				neckSize.Height = this.PlotAreaPosition.Height/2f;
			}
			if(neckSize.Width > this.PlotAreaPosition.Width/2f)
			{
				neckSize.Width = this.PlotAreaPosition.Width/2f;
			}

			// Convert from relative coordinates to pixels
			return this.Graph.GetAbsoluteSize(neckSize);
		}

		/// <summary>
		/// Gets gap between points in pixels.
		/// </summary>
		/// <returns>Gap between funnel points.</returns>
        private float GetFunnelPointGap(DataPointCustomProperties properties)
		{
			// Set default gap size
			float	gapSize = 0f;

			// Get string value of the custom attribute
			string attrValue = properties[this.funnelPointGapAttributeName];
            if (attrValue != null && attrValue.Length > 0)
            {
                // Convert string to the point gap size

                float gs;
                bool parseSucceed = float.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out gs);
                if (parseSucceed)
                {
                    gapSize = gs;
                }
                else
                {
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue, this.funnelPointGapAttributeName)));
                }

                // Make sure the total gap size for all points do not exceed the total height of the plotting area
                float maxGapSize = this.PlotAreaPosition.Height / (this.pointNumber - ((ShouldDrawFirstPoint()) ? 1 : 2));
                if (gapSize > maxGapSize)
                {
                    gapSize = maxGapSize;
                }
                if (gapSize < 0)
                {
                    gapSize = 0;
                }

                // Convert from relative coordinates to pixels
                gapSize = this.Graph.GetAbsoluteSize(new SizeF(gapSize, gapSize)).Height;
            }

			return gapSize;
		}

		/// <summary>
		/// Gets funnel drawing style.
		/// </summary>
		/// <returns>funnel drawing style.</returns>
        private FunnelStyle GetFunnelStyle(DataPointCustomProperties properties)
		{
			// Set default funnel drawing style
			FunnelStyle drawingStyle = FunnelStyle.YIsHeight;

			// Get string value of the custom attribute
			if(!this.isPyramid)
			{
				string attrValue = properties[CustomPropertyName.FunnelStyle];
				if(attrValue != null && attrValue.Length > 0)
				{
					// Convert string to the labels style
					try
					{
						drawingStyle = (FunnelStyle)Enum.Parse(typeof(FunnelStyle), attrValue, true);
					}
					catch
					{
						throw(new InvalidOperationException( SR.ExceptionCustomAttributeValueInvalid( attrValue, "FunnelStyle") ) );
					}
				}
			}
			return drawingStyle;
		}

		/// <summary>
		/// Gets outside labels placement.
		/// </summary>
		/// <returns>Outside labels placement.</returns>
        private FunnelLabelPlacement GetOutsideLabelPlacement(DataPointCustomProperties properties)
		{
			// Set default vertical alignment for the inside labels
			FunnelLabelPlacement placement = FunnelLabelPlacement.Right;

			// Get string value of the custom attribute
			string attrValue = properties[this.funnelOutsideLabelPlacementAttributeName];
			if(attrValue != null && attrValue.Length > 0)
			{
				// Convert string to the labels placement
				try
				{
					placement = (FunnelLabelPlacement)Enum.Parse(typeof(FunnelLabelPlacement), attrValue, true);
				}
				catch
				{
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue, this.funnelOutsideLabelPlacementAttributeName )));
				}
			}
			return placement;
		}

		/// <summary>
		/// Gets inside labels vertical alignment.
		/// </summary>
		/// <returns>Inside labels vertical alignment.</returns>
        private FunnelLabelVerticalAlignment GetInsideLabelAlignment(DataPointCustomProperties properties)
		{
			// Set default vertical alignment for the inside labels
			FunnelLabelVerticalAlignment alignment = FunnelLabelVerticalAlignment.Center;

			// Get string value of the custom attribute
			string attrValue = properties[this.funnelInsideLabelAlignmentAttributeName];
			if(attrValue != null && attrValue.Length > 0)
			{
				// Convert string to the labels style
				try
				{
					alignment = (FunnelLabelVerticalAlignment)Enum.Parse(typeof(FunnelLabelVerticalAlignment), attrValue, true);
				}
				catch
				{
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue, this.funnelInsideLabelAlignmentAttributeName)));
				}
			}
			return alignment;
		}

		/// <summary>
		/// Gets funnel 3D drawing style.
		/// </summary>
		/// <returns>funnel drawing style.</returns>
        private Funnel3DDrawingStyle GetFunnel3DDrawingStyle(DataPointCustomProperties properties)
		{
			// Set default funnel drawing style
			Funnel3DDrawingStyle drawingStyle = (this.isPyramid) ? 
				Funnel3DDrawingStyle.SquareBase : Funnel3DDrawingStyle.CircularBase;

			// Get string value of the custom attribute
			string attrValue = properties[funnel3DDrawingStyleAttributeName];
			if(attrValue != null && attrValue.Length > 0)
			{
				// Convert string to the labels style
				try
				{
					drawingStyle = (Funnel3DDrawingStyle)Enum.Parse(typeof(Funnel3DDrawingStyle), attrValue, true);
				}
				catch
				{
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue, funnel3DDrawingStyleAttributeName) ) );
				}
			}

			return drawingStyle;
		}

		/// <summary>
		/// Get data point Y and X values statistics:
		///   - Total of all Y values
		///   - Total of all X values
		///   - Maximum Y value
		/// Negative values are treated as positive.
		/// </summary>
		private void GetDataPointValuesStatistic()
		{
			// Get funnel chart type series
			Series series = GetDataSeries();
			if( series != null )
			{
				// Reset values
				this.yValueTotal = 0.0;
				this._xValueTotal = 0.0;
				this._yValueMax = 0.0;
				this.pointNumber = 0;

				// Get value type
				this._valuePercentages = null;
				PyramidValueType valueType = this.GetPyramidValueType( series );
				if(valueType == PyramidValueType.Surface)
				{
					// Calculate the total surface area
					double triangleArea = 0.0;
					int pointIndex = 0;
					foreach( DataPoint point in series.Points )
					{
						// Ignore empty points
						if( !point.IsEmpty )
						{
							// Get Y value
							triangleArea += GetYValue(point, pointIndex);
						}
						++pointIndex;
					}

					// Calculate the base
					double triangleHeight = 100.0;
					double triangleBase = (2* triangleArea) / triangleHeight;
 
					// Calculate the base to height ratio
					double baseRatio = triangleBase / triangleHeight;

					// Calcuate the height percentage for each value
					double[] percentages = new double[series.Points.Count];
					double sumArea = 0.0;
					for(int loop = 0; loop < percentages.Length; loop++)
					{
						double yValue = GetYValue(series.Points[loop], loop);
						sumArea += yValue;
						percentages[loop] = Math.Sqrt((2 * sumArea) / baseRatio);
					}
					this._valuePercentages = percentages;
				}

				// Loop through all ponts in the data series
				foreach( DataPoint point in series.Points )
				{
					// Ignore empty points
					if( !point.IsEmpty )
					{
						// Get Y value
						double yValue = GetYValue(point, this.pointNumber);

						// Get data point Y and X values statistics
						this.yValueTotal += yValue;
						this._yValueMax = Math.Max(this._yValueMax, yValue);
						this._xValueTotal += GetXValue(point);
					}

					++this.pointNumber;
				}

			}
		}

		/// <summary>
		/// Gets funnel chart series that belongs to the current chart area.
		/// Method also checks that only one visible Funnel series exists in the chart area.
		/// </summary>
		/// <returns>Funnel chart type series.</returns>
		private Series GetDataSeries()
		{
			// Check if funnel series was already found
			if(this._chartTypeSeries == null)
			{
				// Loop through all series
				Series funnelSeries = null;
				foreach( Series series in Common.DataManager.Series )
				{
					// Check if series is visible and belong to the current chart area
					if( series.IsVisible() && 
						series.ChartArea == this.Area.Name )
					{
						// Check series chart type is Funnel
						if( String.Compare( series.ChartTypeName, this.Name, true, System.Globalization.CultureInfo.CurrentCulture ) == 0 )
						{
							if(funnelSeries == null)
							{
								funnelSeries = series;
							}
						}
						else if(!this.Common.ChartPicture.SuppressExceptions)
						{
							// Funnel chart can not be combined with other chart type
                            throw (new InvalidOperationException(SR.ExceptionFunnelCanNotCombine));
						}
					}
				}

				// Remember the chart type series
				this._chartTypeSeries = funnelSeries;
			}
		
			return this._chartTypeSeries;
		}

		/// <summary>
		/// Gets pyramid value type. Each point value may represent a "Linear" height of
		/// the segment or "Surface" of the segment.
		/// </summary>
		/// <returns>Pyramid value type.</returns>
        private PyramidValueType GetPyramidValueType(DataPointCustomProperties properties)
		{
			// Set default funnel drawing style
			PyramidValueType valueType = PyramidValueType.Linear;

			// Get string value of the custom attribute
			if(this.isPyramid)
			{
				string attrValue = properties[CustomPropertyName.PyramidValueType];
				if(attrValue != null && attrValue.Length > 0)
				{
					// Convert string to the labels style
					try
					{
						valueType = (PyramidValueType)Enum.Parse(typeof(PyramidValueType), attrValue, true);
					}
					catch
					{
                        throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(attrValue,"PyramidValueType") ) );
					}
				}
			}
			return valueType;
		}

		#endregion // Helper Methods

		#region Y & X values related methods

		/// <summary>
		/// Helper function, which returns the Y value of the point.
		/// </summary>
		/// <param name="point">Point object.</param>
		/// <param name="pointIndex">Point index.</param>
		/// <returns>Y value of the point.</returns>
		virtual public double GetYValue(DataPoint point, int pointIndex)
		{
			double	yValue = 0.0;
			if( !point.IsEmpty )
			{
				// Get Y value
				yValue = point.YValues[0];

				// Adjust point value
				if(this._valuePercentages != null &&
					this._valuePercentages.Length > pointIndex )
				{
					yValue = yValue / 100.0 * this._valuePercentages[pointIndex];
				}

				if(this.Area.AxisY.IsLogarithmic)
				{
					yValue = Math.Abs(Math.Log( yValue, this.Area.AxisY.LogarithmBase ));
				}
				else
				{
					yValue = Math.Abs( yValue );
					if(yValue < this._funnelMinPointHeight)
					{
						yValue = this._funnelMinPointHeight;
					}
				}
			}
			return yValue;
		}

		/// <summary>
		/// Helper function, which returns the X value of the point.
		/// </summary>
		/// <param name="point">Point object.</param>
		/// <returns>X value of the point.</returns>
		virtual public double GetXValue(DataPoint point)
		{
			if(this.Area.AxisX.IsLogarithmic)
			{
				return Math.Abs(Math.Log( point.XValue, this.Area.AxisX.LogarithmBase ));
			}
			return Math.Abs(point.XValue);
		}

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

		#endregion // Y & X values related methods

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
			// Fast Line chart type do not support labels
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
	/// PyramidChart class overrides some of the functionality of FunnelChart class.
    /// Most of drawing and othere processing is done in the FunnelChart.
	/// </summary>
	internal class PyramidChart : FunnelChart
	{
		#region Fields and Constructor

		/// <summary>
		/// Default constructor
		/// </summary>
		public PyramidChart()
		{
			// Renering of the pyramid chart type
			base.isPyramid = true;

			// Pyramid chart type uses square base by default
			base.round3DShape = false;

			// Pyramid properties names
			base.funnelLabelStyleAttributeName = CustomPropertyName.PyramidLabelStyle;
			base.funnelPointGapAttributeName = CustomPropertyName.PyramidPointGap;
			base.funnelRotationAngleAttributeName = CustomPropertyName.Pyramid3DRotationAngle;
			base.funnelPointMinHeight = CustomPropertyName.PyramidMinPointHeight;
			base.funnel3DDrawingStyleAttributeName = CustomPropertyName.Pyramid3DDrawingStyle;
			base.funnelInsideLabelAlignmentAttributeName = CustomPropertyName.PyramidInsideLabelAlignment;
			base.funnelOutsideLabelPlacementAttributeName = CustomPropertyName.PyramidOutsideLabelPlacement;
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		override public string Name			{ get{ return ChartTypeNames.Pyramid;}}

		#endregion

		#region Methods

		/// <summary>
		/// Gets pyramid data point segment height and width.
		/// </summary>
		/// <param name="series">Chart type series.</param>
		/// <param name="pointIndex">Data point index in the series.</param>
		/// <param name="location">Segment top location. Bottom location if reversed drawing order.</param>
		/// <param name="height">Returns the height of the segment.</param>
		/// <param name="startWidth">Returns top width of the segment.</param>
		/// <param name="endWidth">Returns botom width of the segment.</param>
		protected override void GetPointWidthAndHeight(
			Series series,
			int pointIndex,
			float location,
			out float height, 
			out float startWidth, 
			out float endWidth)
		{
			PointF	pointPositionAbs = PointF.Empty;

			// Get plotting area position in pixels
			RectangleF plotAreaPositionAbs = this.Graph.GetAbsoluteRectangle(this.PlotAreaPosition);

			// Calculate total height of plotting area minus reserved space for the gaps
			float plotAreaHeightAbs = plotAreaPositionAbs.Height - 
				this.funnelSegmentGap * (this.pointNumber - ((ShouldDrawFirstPoint()) ? 1 : 2) );
			if(plotAreaHeightAbs < 0f)
			{
				plotAreaHeightAbs = 0f;
			}

			// Calculate segment height as a part of total Y values in series
			height = (float)(plotAreaHeightAbs * (GetYValue(series.Points[pointIndex], pointIndex) / this.yValueTotal));

			// Check for minimum segment height
			height = CheckMinHeight(height);

			// Get intersection point of the horizontal line at the start of the segment
			// with the left pre-defined wall of the funnel.
			PointF startIntersection = ChartGraphics.GetLinesIntersection(
				plotAreaPositionAbs.X, location - height, 
				plotAreaPositionAbs.Right, location - height, 
				plotAreaPositionAbs.X, plotAreaPositionAbs.Bottom, 
				plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f, plotAreaPositionAbs.Y );
			if(startIntersection.X > (plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f) )
			{
				startIntersection.X = plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f;
			}

			// Get intersection point of the horizontal line at the end of the segment
			// with the left pre-defined wall of the funnel.
			PointF endIntersection = ChartGraphics.GetLinesIntersection(
				plotAreaPositionAbs.X, location, 
				plotAreaPositionAbs.Right, location, 
				plotAreaPositionAbs.X, plotAreaPositionAbs.Bottom, 
				plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f, plotAreaPositionAbs.Y );
			if(endIntersection.X > (plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f))
			{
				endIntersection.X = plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f;
			}

			// Get segment start and end width
			startWidth = (float)Math.Abs( plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f - 
				startIntersection.X) * 2f;
			endWidth = (float)Math.Abs( plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f - 
				endIntersection.X) * 2f;

			// Set point position for annotation anchoring
			pointPositionAbs  = new PointF(
				plotAreaPositionAbs.X + plotAreaPositionAbs.Width / 2f, 
				location - height / 2f);

			// Set pre-calculated point position
			series.Points[pointIndex].positionRel = Graph.GetRelativePoint(pointPositionAbs);
		}

		#endregion // Methods
	}

	/// <summary>
	/// Helper data structure used to store information about single funnel segment.
	/// </summary>
	internal class FunnelSegmentInfo
	{
		#region Fields

		// ----osiated data point
		public	DataPoint	Point = null;

		// Data point index
		public	int			PointIndex = 0;

		// Segment top position
		public	float		Location = 0f;

		// Segment height
		public	float		Height = 0f;

		// Segment top width
		public	float		StartWidth = 0f;

		// Segment bottom width
		public	float		EndWidth = 0f;

		// Segment has nothing on the top
		public	bool		NothingOnTop = false;

		// Segment has nothing on the bottom
		public	bool		NothingOnBottom = false;

		#endregion // Fields
	}

	/// <summary>
	/// Helper data structure used to store information about funnel data point label.
	/// </summary>
	internal class FunnelPointLabelInfo
	{
		#region Fields

		// ----osiated data point
		public	DataPoint			Point = null;

		// Data point index
		public	int					PointIndex = 0;

		// Label text
		public	string				Text = string.Empty;

		// Data point label size
		public	SizeF				Size = SizeF.Empty;

		// Position of the data point label
		public	RectangleF			Position = RectangleF.Empty;

		// Label style
		public	FunnelLabelStyle	Style = FunnelLabelStyle.OutsideInColumn;

		// Inside label vertical alignment
		public	FunnelLabelVerticalAlignment	VerticalAlignment = FunnelLabelVerticalAlignment.Center;

		// Outside labels placement
		public	FunnelLabelPlacement OutsidePlacement = FunnelLabelPlacement.Right;

		// Label callout first point
		public	PointF				CalloutPoint1 = PointF.Empty;

		// Label callout second point
		public	PointF				CalloutPoint2 = PointF.Empty;

		#endregion // Fields
	}
}
