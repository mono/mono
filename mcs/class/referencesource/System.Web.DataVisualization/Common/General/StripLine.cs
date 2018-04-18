//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		StripLine.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	StripLinesCollection, StripLine
//
//  Purpose:	StripLinesCollection class is used to expose stripes 
//              or lines on the plotting area and is exposed through 
//              StripLines property of each Axis.
//
//              Each StripLine class presents one or series of 
//              repeated axis horizontal or vertical strips within 
//              the plotting are.
//
//              When multiple strip lines are defined for an axis, 
//              there is a possibility of overlap. The z-order of 
//              StripLine objects is determined by their order of 
//              occurrence in the StripLinesCollection object. The 
//              z-order follows this convention, the first occurrence 
//              is drawn first, the second occurrence is drawn second, 
//              and so on.
//
//              Highlighting weekends on date axis is a good example 
//              of using strip lines with interval. 
//
//	Reviewed:	AG - Jul 31, 2002; 
//              GS - Aug 7, 2002
//              AG - Microsoft 13, 2007
//
//===================================================================


#region Used namespaces

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
	using System.Globalization;
	using System.ComponentModel.Design.Serialization;
	using System.Reflection;
	using System.Windows.Forms.Design;
#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.Utilities;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
    namespace System.Web.UI.DataVisualization.Charting
#endif

{
	/// <summary>
    /// The StripLinesCollection class is a strongly typed collection of 
    /// StripLine classes. 
	/// </summary>
	[
		SRDescription("DescriptionAttributeStripLinesCollection_StripLinesCollection"),

	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
        [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class StripLinesCollection : ChartElementCollection<StripLine>
    {

        #region Constructor
        /// <summary>
		/// Legend item collection object constructor
		/// </summary>
		/// <param name="axis">Axis object reference.</param>
        internal StripLinesCollection(Axis axis)
            : base(axis)
        {
        }
        #endregion


	}

	/// <summary>
    /// The StripLine class contains properties which define visual appearance 
    /// of the stripe or line, its position according to the axis.  It 
    /// may optionally contain the repeat interval. Text may associate 
    /// with a strip or a line.  It also contains methods of drawing and hit 
    /// testing.
	/// </summary>
	[
		SRDescription("DescriptionAttributeStripLine_StripLine"),
		DefaultProperty("IntervalOffset"),
	]
#if Microsoft_CONTROL
	public class StripLine : ChartElement
#else
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class StripLine : ChartElement, IChartMapArea
#endif
    {

		#region Fields

		// Private data members, which store properties values
		private double					_intervalOffset = 0;
		private double					_interval = 0;
		private DateTimeIntervalType	_intervalType = DateTimeIntervalType.Auto;
		internal DateTimeIntervalType	intervalOffsetType = DateTimeIntervalType.Auto;
		internal bool					interlaced = false;
		private double					_stripWidth = 0;
		private DateTimeIntervalType	_stripWidthType = DateTimeIntervalType.Auto;
		private Color					_backColor = Color.Empty;
		private ChartHatchStyle			_backHatchStyle = ChartHatchStyle.None;
		private string					_backImage = "";
		private ChartImageWrapMode		_backImageWrapMode = ChartImageWrapMode.Tile;
		private Color					_backImageTransparentColor = Color.Empty;
		private ChartImageAlignmentStyle	_backImageAlignment = ChartImageAlignmentStyle.TopLeft;
		private GradientStyle			_backGradientStyle = GradientStyle.None;
		private Color					_backSecondaryColor = Color.Empty;
		private Color					_borderColor = Color.Empty;
		private int						_borderWidth = 1;
		private ChartDashStyle			_borderDashStyle = ChartDashStyle.Solid;

		// Strip/Line title properties
		private	string					_text = "";
		private Color					_foreColor = Color.Black;
        private FontCache               _fontCache = new FontCache();
		private Font					_font = null;
		private StringAlignment			_textAlignment = StringAlignment.Far;
		private StringAlignment			_textLineAlignment = StringAlignment.Near;

		// Chart image map properties 
		private	string					_toolTip = "";

#if !Microsoft_CONTROL
		private	string					_url = "";
		private	string					_attributes = "";
        private string                  _postbackValue = String.Empty;
#endif

        // Default text orientation
        private TextOrientation _textOrientation = TextOrientation.Auto;

		#endregion

        #region Properties
        /// <summary>
        /// Gets axes to which this object attached to.
        /// </summary>
        /// <returns>Axis object reference.</returns>
        internal Axis Axis
        {
            get
            {
                if (Parent != null)
                    return Parent.Parent as Axis;
                else
                    return null;
            }
        }
        #endregion

        #region Constructors

        /// <summary>
		/// Strip line object constructor.
		/// </summary>
		public StripLine() 
            : base()
		{
            _font = _fontCache.DefaultFont;
		}

        #endregion

		#region Painting methods

        /// <summary>
        /// Checks if chart title is drawn vertically.
        /// Note: From the drawing perspective stacked text orientation is not vertical.
        /// </summary>
        /// <returns>True if text is vertical.</returns>
        private bool IsTextVertical
        {
            get
            {
                TextOrientation currentTextOrientation = this.GetTextOrientation();
                return currentTextOrientation == TextOrientation.Rotated90 || currentTextOrientation == TextOrientation.Rotated270;
            }
        }

        /// <summary>
        /// Returns stripline text orientation. If set to Auto automatically determines the
        /// orientation based on the orientation of the stripline.
        /// </summary>
        /// <returns>Current text orientation.</returns>
        private TextOrientation GetTextOrientation()
        {
            if (this.TextOrientation == TextOrientation.Auto && this.Axis != null)
            {
                if (this.Axis.AxisPosition == AxisPosition.Bottom || this.Axis.AxisPosition == AxisPosition.Top)
                {
                    return TextOrientation.Rotated270;
                }
                return TextOrientation.Horizontal;
            }
            return this.TextOrientation;
        }

		/// <summary>
		/// Draw strip(s) or line(s).
		/// </summary>
		/// <param name="graph">Reference to the Chart Graphics object.</param>
		/// <param name="common">Common objects.</param>
		/// <param name="drawLinesOnly">Indicates if Lines or Stripes should be drawn.</param>
		internal void Paint( 
			ChartGraphics graph, 
			CommonElements common,
			bool drawLinesOnly)
		{
			// Strip lines are not supported in circular chart area
			if(this.Axis.ChartArea.chartAreaIsCurcular)
			{
				return;
			}

			// Get plot area position
			RectangleF	plotAreaPosition = this.Axis.ChartArea.PlotAreaPosition.ToRectangleF();

			// Detect if strip/line is horizontal or vertical
			bool	horizontal = true;
			if(this.Axis.AxisPosition == AxisPosition.Bottom || this.Axis.AxisPosition == AxisPosition.Top)
			{
				horizontal = false;
			}

			// Get first series attached to this axis
			Series	axisSeries = null;			
			if(Axis.axisType == AxisName.X || Axis.axisType == AxisName.X2)
			{
				List<string> seriesArray = Axis.ChartArea.GetXAxesSeries((Axis.axisType == AxisName.X) ? AxisType.Primary : AxisType.Secondary, Axis.SubAxisName);
				if(seriesArray.Count > 0)
				{
					axisSeries = Axis.Common.DataManager.Series[seriesArray[0]];
					if(axisSeries != null && !axisSeries.IsXValueIndexed)
					{
						axisSeries = null;
					}
				}
			}

			// Get starting position from axis
			// NOTE: Starting position was changed from "this.Axis.minimum" to 
			// fix the minimum scaleView location to fix issue #5962 -- AG
            double currentPosition = this.Axis.ViewMinimum;

			// Adjust start position depending on the interval type
			if(!Axis.ChartArea.chartAreaIsCurcular ||
				Axis.axisType == AxisName.Y || 
				Axis.axisType == AxisName.Y2 )
			{
                double intervalToUse = this.Interval;

                // NOTE: fix for issue #5962
                // Always use original grid interval for isInterlaced strip lines.
                if (this.interlaced)
                {
                    // Automaticly generated isInterlaced strips have interval twice as big as major grids
                    intervalToUse /= 2.0;
                }
                currentPosition = ChartHelper.AlignIntervalStart(currentPosition, intervalToUse, this.IntervalType, axisSeries);
			}

			// Too many tick marks
			if(this.Interval != 0)
			{
				if( ( Axis.ViewMaximum - Axis.ViewMinimum ) / ChartHelper.GetIntervalSize(currentPosition, this._interval, this._intervalType, axisSeries, 0, DateTimeIntervalType.Number, false) > ChartHelper.MaxNumOfGridlines)
					return;
			}

			DateTimeIntervalType offsetType = (IntervalOffsetType == DateTimeIntervalType.Auto) ? IntervalType : IntervalOffsetType;
			if(this.Interval == 0)
			{
				currentPosition = this.IntervalOffset;
			}
			/******************************************************************
			 * Removed by AG. Causing issues with interalced strip lines.
			 /******************************************************************
			else if(axisSeries != null && axisSeries.IsXValueIndexed)
			{
				// Align first position for indexed series
				currentPosition += this.Axis.AlignIndexedIntervalStart(
					currentPosition, 
					this.Interval, 
					this.IntervalType, 
					axisSeries, 
					this.IntervalOffset, 
					offsetType, 
					false);
			}
			*/
			else
			{
				if(this.IntervalOffset > 0)
				{
                    currentPosition += ChartHelper.GetIntervalSize(currentPosition, this.IntervalOffset, 
						offsetType, axisSeries, 0, DateTimeIntervalType.Number, false);
				}
				else if(this.IntervalOffset < 0)
				{
                    currentPosition -= ChartHelper.GetIntervalSize(currentPosition, -this.IntervalOffset, 
						offsetType, axisSeries, 0, DateTimeIntervalType.Number, false);
				}
			}

			// Draw several lines or strips if Interval property is set
			int	counter = 0;
			do
			{
				// Check if we do not exceed max number of elements
				if(counter++ > ChartHelper.MaxNumOfGridlines)
				{
					break;
				}
						
				// Draw strip
				if(this.StripWidth > 0 && !drawLinesOnly)
				{
                    double stripRightPosition = currentPosition + ChartHelper.GetIntervalSize(currentPosition, this.StripWidth, this.StripWidthType, axisSeries, this.IntervalOffset, offsetType, false);
                    if (stripRightPosition > this.Axis.ViewMinimum && currentPosition < this.Axis.ViewMaximum)
                    {
					    // Calculate strip rectangle
					    RectangleF	rect = RectangleF.Empty;
					    double		pos1 = (float)this.Axis.GetLinearPosition(currentPosition);
                        double      pos2 = (float)this.Axis.GetLinearPosition(stripRightPosition);
					    if(horizontal)
					    {
						    rect.X = plotAreaPosition.X;
						    rect.Width = plotAreaPosition.Width;
						    rect.Y = (float)Math.Min(pos1, pos2);
						    rect.Height = (float)Math.Max(pos1, pos2) - rect.Y;

						    // Check rectangle boundaries
						    rect.Intersect(plotAreaPosition);
					    }
					    else
					    {
						    rect.Y = plotAreaPosition.Y;
						    rect.Height = plotAreaPosition.Height;
						    rect.X = (float)Math.Min(pos1, pos2);
						    rect.Width = (float)Math.Max(pos1, pos2) - rect.X;

						    // Check rectangle boundaries
						    rect.Intersect(plotAreaPosition);
					    }

					    if(rect.Width > 0 && rect.Height > 0)
					    {
    					
    #if Microsoft_CONTROL
						    // Start Svg Selection mode
						    graph.StartHotRegion( "", this._toolTip );
    #else
						    // Start Svg Selection mode
						    graph.StartHotRegion( this._url, this._toolTip );
    #endif
						    if(!this.Axis.ChartArea.Area3DStyle.Enable3D)
						    {
							    // Draw strip
							    graph.FillRectangleRel( rect, 
								    this.BackColor, this.BackHatchStyle, this.BackImage, 
								    this.BackImageWrapMode, this.BackImageTransparentColor, this.BackImageAlignment,
								    this.BackGradientStyle, this.BackSecondaryColor, this.BorderColor, 
								    this.BorderWidth, this.BorderDashStyle, Color.Empty,
								    0, PenAlignment.Inset );
						    }
						    else
						    {
							    Draw3DStrip( graph, rect, horizontal );
						    }

						    // End Svg Selection mode
						    graph.EndHotRegion( );

						    // Draw strip line title
						    PaintTitle(graph, rect);

						    if( common.ProcessModeRegions )
						    {
							    if(!this.Axis.ChartArea.Area3DStyle.Enable3D)
							    {
#if !Microsoft_CONTROL
                                    				common.HotRegionsList.AddHotRegion(rect, this.ToolTip, this.Url, this.MapAreaAttributes, this.PostBackValue, this, ChartElementType.StripLines, string.Empty);
#else
								common.HotRegionsList.AddHotRegion(rect, this.ToolTip, string.Empty, string.Empty, string.Empty, this, ChartElementType.StripLines, null );
#endif // !Microsoft_CONTROL
                                }
						    }
					    }
                        
				    }
                }
				// Draw line
				else if(this.StripWidth == 0 && drawLinesOnly)
				{
					if(currentPosition > this.Axis.ViewMinimum && currentPosition < this.Axis.ViewMaximum)
					{
						// Calculate line position
						PointF	point1 = PointF.Empty;
						PointF	point2 = PointF.Empty;
						if(horizontal)
						{
							point1.X = plotAreaPosition.X;
							point1.Y = (float)this.Axis.GetLinearPosition(currentPosition);
							point2.X = plotAreaPosition.Right;
							point2.Y = point1.Y;
						}
						else
						{
							point1.X = (float)this.Axis.GetLinearPosition(currentPosition);
							point1.Y = plotAreaPosition.Y;
							point2.X = point1.X;
							point2.Y = plotAreaPosition.Bottom;
						}

#if Microsoft_CONTROL
						// Start Svg Selection mode
						graph.StartHotRegion( "", this._toolTip );
#else
						// Start Svg Selection mode
						graph.StartHotRegion( this._url, this._toolTip );
#endif
						// Draw Line
						if(!this.Axis.ChartArea.Area3DStyle.Enable3D)
						{
							graph.DrawLineRel(this.BorderColor, this.BorderWidth, this.BorderDashStyle, point1, point2);
						}
						else
						{
							graph.Draw3DGridLine(this.Axis.ChartArea, _borderColor, _borderWidth, _borderDashStyle, point1, point2, horizontal, Axis.Common, this );
						}

						// End Svg Selection mode
						graph.EndHotRegion( );

						// Draw strip line title
						PaintTitle(graph, point1, point2);

						if( common.ProcessModeRegions )
						{
							SizeF		relBorderWidth = new SizeF(this.BorderWidth + 1, this.BorderWidth +  1);
							relBorderWidth = graph.GetRelativeSize(relBorderWidth);
							RectangleF	lineRect = RectangleF.Empty;
							if(horizontal)
							{
								lineRect.X = point1.X;
								lineRect.Y = point1.Y - relBorderWidth.Height / 2f;
								lineRect.Width = point2.X - point1.X;
								lineRect.Height = relBorderWidth.Height;
							}
							else
							{
								lineRect.X = point1.X - relBorderWidth.Width / 2f;
								lineRect.Y = point1.Y;
								lineRect.Width = relBorderWidth.Width;
								lineRect.Height = point2.Y - point1.Y;
							}

#if !Microsoft_CONTROL
							common.HotRegionsList.AddHotRegion( lineRect, this.ToolTip, this.Url, this.MapAreaAttributes, this.PostBackValue, this, ChartElementType.StripLines, string.Empty );
#else
							common.HotRegionsList.AddHotRegion( lineRect, this.ToolTip, null, null, null, this, ChartElementType.StripLines, null );
#endif // !Microsoft_CONTROL
						}
					}
				}

				// Go to the next line/strip
				if(this.Interval > 0)
				{
                    currentPosition += ChartHelper.GetIntervalSize(currentPosition, this.Interval, this.IntervalType, axisSeries, this.IntervalOffset, offsetType, false);
				}

			} while(this.Interval > 0 && currentPosition <= this.Axis.ViewMaximum);
		}

        /// <summary>
        /// Draws strip line in 3d.
        /// </summary>
        /// <param name="graph">Chart graphics.</param>
        /// <param name="rect">Strip rectangle.</param>
        /// <param name="horizontal">Indicates that strip is horizontal</param>
		private void Draw3DStrip(ChartGraphics graph, RectangleF rect, bool horizontal )
		{
			ChartArea	area = this.Axis.ChartArea;
			GraphicsPath path = null;
			DrawingOperationTypes operationType = DrawingOperationTypes.DrawElement;

			if( this.Axis.Common.ProcessModeRegions )
			{
				operationType |= DrawingOperationTypes.CalcElementPath;
			}

			// Draw strip on the back/front wall
			path = graph.Fill3DRectangle(
				rect, 
                area.IsMainSceneWallOnFront() ? area.areaSceneDepth : 0f, 
                0, 
                area.matrix3D, 
                area.Area3DStyle.LightStyle,
				this.BackColor, 
                this.BorderColor, 
				this.BorderWidth, 
                this.BorderDashStyle, 
				operationType );

			if( this.Axis.Common.ProcessModeRegions )
			{
			 
#if !Microsoft_CONTROL
				this.Axis.Common.HotRegionsList.AddHotRegion( graph, path, false, this.ToolTip, this.Url, this.MapAreaAttributes, this.PostBackValue, this, ChartElementType.StripLines );
#else
				this.Axis.Common.HotRegionsList.AddHotRegion( graph, path, false, this.ToolTip, null, null, null, this, ChartElementType.StripLines );
#endif // !Microsoft_CONTROL
			}

			if(horizontal)
			{
				// Draw strip on the side wall (left or right)
				if(!area.IsSideSceneWallOnLeft())
				{
					rect.X = rect.Right;
				}
				rect.Width = 0f;

				path = graph.Fill3DRectangle( 
					rect, 
                    0f, 
                    area.areaSceneDepth, 
                    area.matrix3D, 
                    area.Area3DStyle.LightStyle,
					this.BackColor, 
                    this.BorderColor, 
					this.BorderWidth, 
                    this.BorderDashStyle, 
					operationType );

			}
			else if(area.IsBottomSceneWallVisible())
			{
				// Draw strip on the bottom wall (if visible)
				rect.Y = rect.Bottom;
				rect.Height = 0f;

				path = graph.Fill3DRectangle( 
					rect,
                    0f, 
                    area.areaSceneDepth,
                    area.matrix3D,
                    area.Area3DStyle.LightStyle,
					this.BackColor,
                    this.BorderColor, 
					this.BorderWidth,
                    this.BorderDashStyle,
					operationType );

			}
			if( this.Axis.Common.ProcessModeRegions )
			{
#if !Microsoft_CONTROL
				this.Axis.Common.HotRegionsList.AddHotRegion( graph, path, false, this.ToolTip, this.Url, this.MapAreaAttributes, this.PostBackValue, this, ChartElementType.StripLines );
#else
				this.Axis.Common.HotRegionsList.AddHotRegion( graph, path, false, this.ToolTip, null, null, null, this, ChartElementType.StripLines );
#endif // !Microsoft_CONTROL
			}
            if (path != null)
            {
                path.Dispose();
            }
		}

		/// <summary>
		/// Draw strip/line title text
		/// </summary>
		/// <param name="graph">Chart graphics object.</param>
		/// <param name="point1">First line point.</param>
		/// <param name="point2">Second line point.</param>
		private void PaintTitle(ChartGraphics graph, PointF point1, PointF point2)
		{
			if(this.Text.Length > 0)
			{
				// Define a rectangle to draw the title
				RectangleF rect = RectangleF.Empty;
				rect.X = point1.X;
				rect.Y = point1.Y;
				rect.Height = point2.Y - rect.Y;
				rect.Width = point2.X - rect.X;

				// Paint title using a rect
				PaintTitle(graph, rect);
			}
		}

        /// <summary>
        /// Draw strip/line title text
        /// </summary>
        /// <param name="graph">Chart graphics object.</param>
        /// <param name="rect">Rectangle to draw in.</param>
		private void PaintTitle(ChartGraphics graph, RectangleF rect)
		{
			if(this.Text.Length > 0)
			{
				// Get title text
				string	titleText = this.Text;

				// Prepare string format
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = this.TextAlignment;
                    
                    if (graph.IsRightToLeft)
                    {
                        if (format.Alignment == StringAlignment.Far)
                        {
                            format.Alignment = StringAlignment.Near;
                        }
                        else if (format.Alignment == StringAlignment.Near)
                        {
                            format.Alignment = StringAlignment.Far;
                        }
                    }
                    
                    format.LineAlignment = this.TextLineAlignment;

                    // Adjust default title angle for horizontal lines
                    int angle = 0;
                    switch (this.TextOrientation)
                    {
                        case (TextOrientation.Rotated90):
                            angle = 90;
                            break;
                        case (TextOrientation.Rotated270):
                            angle = 270;
                            break;
                        case (TextOrientation.Auto):
                            if (this.Axis.AxisPosition == AxisPosition.Bottom || this.Axis.AxisPosition == AxisPosition.Top)
                            {
                                angle = 270;
                            }
                            break;
                    }

                    // Set vertical text for horizontal lines
                    if (angle == 90)
                    {
                        format.FormatFlags = StringFormatFlags.DirectionVertical;
                        angle = 0;
                    }
                    else if (angle == 270)
                    {
                        format.FormatFlags = StringFormatFlags.DirectionVertical;
                        angle = 180;
                    }

                    // Measure string size
                    SizeF size = graph.MeasureStringRel(titleText.Replace("\\n", "\n"), this.Font, new SizeF(100, 100), format, this.GetTextOrientation());

                    // Adjust text size
                    float zPositon = 0f;
                    if (this.Axis.ChartArea.Area3DStyle.Enable3D)
                    {
                        // Get projection coordinates
                        Point3D[] textSizeProjection = new Point3D[3];
                        zPositon = this.Axis.ChartArea.IsMainSceneWallOnFront() ? this.Axis.ChartArea.areaSceneDepth : 0f;
                        textSizeProjection[0] = new Point3D(0f, 0f, zPositon);
                        textSizeProjection[1] = new Point3D(size.Width, 0f, zPositon);
                        textSizeProjection[2] = new Point3D(0f, size.Height, zPositon);

                        // Transform coordinates of text size
                        this.Axis.ChartArea.matrix3D.TransformPoints(textSizeProjection);

                        // Adjust text size
                        int index = this.Axis.ChartArea.IsMainSceneWallOnFront() ? 0 : 1;
                        size.Width *= size.Width / (textSizeProjection[index].X - textSizeProjection[(index == 0) ? 1 : 0].X);
                        size.Height *= size.Height / (textSizeProjection[2].Y - textSizeProjection[0].Y);
                    }


                    // Get relative size of the border width
                    SizeF sizeBorder = graph.GetRelativeSize(new SizeF(this.BorderWidth, this.BorderWidth));

                    // Find the center of rotation
                    PointF rotationCenter = PointF.Empty;
                    if (format.Alignment == StringAlignment.Near)
                    { // Near
                        rotationCenter.X = rect.X + size.Width / 2 + sizeBorder.Width;
                    }
                    else if (format.Alignment == StringAlignment.Far)
                    { // Far
                        rotationCenter.X = rect.Right - size.Width / 2 - sizeBorder.Width;
                    }
                    else
                    { // Center
                        rotationCenter.X = (rect.Left + rect.Right) / 2;
                    }

                    if (format.LineAlignment == StringAlignment.Near)
                    { // Near
                        rotationCenter.Y = rect.Top + size.Height / 2 + sizeBorder.Height;
                    }
                    else if (format.LineAlignment == StringAlignment.Far)
                    { // Far
                        rotationCenter.Y = rect.Bottom - size.Height / 2 - sizeBorder.Height;
                    }
                    else
                    { // Center
                        rotationCenter.Y = (rect.Bottom + rect.Top) / 2;
                    }

                    // Reset string alignment to center point
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    if (this.Axis.ChartArea.Area3DStyle.Enable3D)
                    {
                        // Get projection coordinates
                        Point3D[] rotationCenterProjection = new Point3D[2];
                        rotationCenterProjection[0] = new Point3D(rotationCenter.X, rotationCenter.Y, zPositon);
                        if (format.FormatFlags == StringFormatFlags.DirectionVertical)
                        {
                            rotationCenterProjection[1] = new Point3D(rotationCenter.X, rotationCenter.Y - 20f, zPositon);
                        }
                        else
                        {
                            rotationCenterProjection[1] = new Point3D(rotationCenter.X - 20f, rotationCenter.Y, zPositon);
                        }

                        // Transform coordinates of text rotation point
                        this.Axis.ChartArea.matrix3D.TransformPoints(rotationCenterProjection);

                        // Adjust rotation point
                        rotationCenter = rotationCenterProjection[0].PointF;

                        // Adjust angle of the text
                        if (angle == 0 || angle == 180 || angle == 90 || angle == 270)
                        {
                            if (format.FormatFlags == StringFormatFlags.DirectionVertical)
                            {
                                angle += 90;
                            }

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
                    }

                    // Draw string
                    using (Brush brush = new SolidBrush(this.ForeColor))
                    {
                        graph.DrawStringRel(
                            titleText.Replace("\\n", "\n"),
                            this.Font,
                            brush,
                            rotationCenter,
                            format,
                            angle,
                            this.GetTextOrientation());
                    }

                }
			}
		}

		#endregion

		#region	Strip line properties

        /// <summary>
        /// Gets or sets the text orientation.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        DefaultValue(TextOrientation.Auto),
        SRDescription("DescriptionAttribute_TextOrientation"),
        NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public TextOrientation TextOrientation
        {
            get
            {
                return this._textOrientation;
            }
            set
            {
                this._textOrientation = value;
                this.Invalidate();
            }
        }

		/// <summary>
		/// Gets or sets the strip or line starting position offset.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		Bindable(true),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeStripLine_IntervalOffset"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
        TypeConverter(typeof(AxisLabelDateValueConverter))
		]
		public double IntervalOffset
		{
			get
			{
				return _intervalOffset;
			}
			set
			{
				_intervalOffset = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the unit of measurement of the strip or line offset.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		Bindable(true),
		DefaultValue(DateTimeIntervalType.Auto),
		SRDescription("DescriptionAttributeStripLine_IntervalOffsetType"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public DateTimeIntervalType IntervalOffsetType
		{
			get
			{
				return intervalOffsetType;
			}
			set
			{
				intervalOffsetType = (value != DateTimeIntervalType.NotSet) ? value : DateTimeIntervalType.Auto;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the strip or line step size.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		Bindable(true),
		DefaultValue(0.0),
		RefreshPropertiesAttribute(RefreshProperties.All),
		SRDescription("DescriptionAttributeStripLine_Interval"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public double Interval
		{
			get
			{
				return _interval;
			}
			set
			{
				_interval = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the unit of measurement of the strip or line step.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		Bindable(true),
		DefaultValue(DateTimeIntervalType.Auto),
		SRDescription("DescriptionAttributeStripLine_IntervalType"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public DateTimeIntervalType IntervalType
		{
			get
			{
				return _intervalType;
			}
			set
			{
				_intervalType = (value != DateTimeIntervalType.NotSet) ? value : DateTimeIntervalType.Auto;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the strip width.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		Bindable(true),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeStripLine_StripWidth"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public double StripWidth
		{
			get
			{
				return _stripWidth;
			}
			set
			{
				if(value < 0)
				{
                    throw (new ArgumentException(SR.ExceptionStripLineWidthIsNegative, "value"));
				}
				_stripWidth = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the unit of measurement of the strip width.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		Bindable(true),
		DefaultValue(DateTimeIntervalType.Auto),
		SRDescription("DescriptionAttributeStripLine_StripWidthType"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public DateTimeIntervalType StripWidthType
		{
			get
			{
				return _stripWidthType;
			}
			set
			{
				_stripWidthType = (value != DateTimeIntervalType.NotSet) ? value : DateTimeIntervalType.Auto;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the background color.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), ""),
        SRDescription("DescriptionAttributeBackColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public Color BackColor
		{
			get
			{
				return _backColor;
			}
			set
			{
				_backColor = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the border color.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
        DefaultValue(typeof(Color), ""),
        SRDescription("DescriptionAttributeBorderColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public Color BorderColor
		{
			get
			{
				return _borderColor;
			}
			set
			{
				_borderColor = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the border style.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartDashStyle.Solid),
        SRDescription("DescriptionAttributeBorderDashStyle"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public ChartDashStyle BorderDashStyle
		{
			get
			{
				return _borderDashStyle;
			}
			set
			{
				_borderDashStyle = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the border width.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(1),
        SRDescription("DescriptionAttributeBorderWidth"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public int BorderWidth
		{
			get
			{
				return _borderWidth;
			}
			set
			{
				_borderWidth = value;
				this.Invalidate(); 
			}
		}
		
		/// <summary>
		/// Gets or sets the background image.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(""),
        SRDescription("DescriptionAttributeBackImage"),
        Editor(Editors.ImageValueEditor.Editor, Editors.ImageValueEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
		NotifyParentPropertyAttribute(true)
		]
		public string BackImage
		{
			get
			{
				return _backImage;
			}
			set
			{
				_backImage = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the background image drawing mode.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartImageWrapMode.Tile),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeImageWrapMode"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public ChartImageWrapMode BackImageWrapMode
		{
			get
			{
				return _backImageWrapMode;
			}
			set
			{
				_backImageWrapMode = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
        /// Gets or sets a color which will be replaced with a transparent color while drawing the background image.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeImageTransparentColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public Color BackImageTransparentColor
		{
			get
			{
				return _backImageTransparentColor;
			}
			set
			{
				_backImageTransparentColor = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the background image alignment used by unscale drawing mode.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartImageAlignmentStyle.TopLeft),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeBackImageAlign"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public ChartImageAlignmentStyle BackImageAlignment
		{
			get
			{
				return _backImageAlignment;
			}
			set
			{
				_backImageAlignment = value;
				this.Invalidate(); 
			}
		}

        /// <summary>
        /// Gets or sets the background gradient style.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="GradientStyle"/> value used for the background.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the gradient, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(GradientStyle.None),
        SRDescription("DescriptionAttributeBackGradientStyle"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
        Editor(Editors.GradientEditor.Editor, Editors.GradientEditor.Base)
		]
		public GradientStyle BackGradientStyle
		{
			get
			{
				return _backGradientStyle;
			}
			set
			{
				_backGradientStyle = value;
				this.Invalidate(); 
			}
		}

        /// <summary>
        /// Gets or sets the secondary background color.
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the secondary color of a background with 
        /// hatching or gradient fill.
        /// </value>
        /// <remarks>
        /// This color is used with <see cref="BackColor"/> when <see cref="BackHatchStyle"/> or
        /// <see cref="BackGradientStyle"/> are used.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), ""),
        SRDescription("DescriptionAttributeBackSecondaryColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public Color BackSecondaryColor
		{
			get
			{
				return _backSecondaryColor;
			}
			set
			{
				_backSecondaryColor = value;
				this.Invalidate(); 
			}
		}

        /// <summary>
        /// Gets or sets the background hatch style.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="ChartHatchStyle"/> value used for the background.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the hatching, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartHatchStyle.None),
        SRDescription("DescriptionAttributeBackHatchStyle"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
        Editor(Editors.HatchStyleEditor.Editor, Editors.HatchStyleEditor.Base)
		]
		public ChartHatchStyle BackHatchStyle
		{
			get
			{
				return _backHatchStyle;
			}
			set
			{
				_backHatchStyle = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the name of the strip line.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(false),
		Browsable(false),
		DefaultValue("StripLine"),
		SRDescription("DescriptionAttributeStripLine_Name"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden)
		]
		public string Name
		{
			get
			{
				return "StripLine";
			}
		}

		/// <summary>
        /// Gets or sets the title text of the strip line.
		/// </summary>
		[
		SRCategory("CategoryAttributeTitle"),
		Bindable(true),
		DefaultValue(""),
		SRDescription("DescriptionAttributeStripLine_Title"),
		NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
        /// Gets or sets the fore color of the strip line.
		/// </summary>
		[
		SRCategory("CategoryAttributeTitle"),
		Bindable(true),
		DefaultValue(typeof(Color), "Black"),
		SRDescription("DescriptionAttributeStripLine_TitleColor"),
		NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public Color ForeColor
		{
			get
			{
				return _foreColor;
			}
			set
			{
				_foreColor = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
        /// Gets or sets the text alignment of the strip line.
		/// </summary>
		[
		SRCategory("CategoryAttributeTitle"),
		Bindable(true),
		DefaultValue(typeof(StringAlignment), "Far"),
		SRDescription("DescriptionAttributeStripLine_TitleAlignment"),
		NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public StringAlignment TextAlignment
		{
			get
			{
				return _textAlignment;
			}
			set
			{
				_textAlignment = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
        /// Gets or sets the text line alignment of the strip line.
		/// </summary>
		[
		SRCategory("CategoryAttributeTitle"),
		Bindable(true),
		DefaultValue(typeof(StringAlignment), "Near"),
		SRDescription("DescriptionAttributeStripLine_TitleLineAlignment"),
		NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public StringAlignment TextLineAlignment
		{
			get
			{
				return _textLineAlignment;
			}
			set
			{
				_textLineAlignment = value;
				this.Invalidate(); 
			}
		}

		/// <summary>
		/// Gets or sets the title font.
		/// </summary>
		[
		SRCategory("CategoryAttributeTitle"),
		Bindable(true),
		DefaultValue(typeof(Font), "Microsoft Sans Serif, 8pt"),
        SRDescription("DescriptionAttributeTitleFont"),
		NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public Font Font
		{
			get
			{
				return _font;
			}
			set
			{
				_font = value;
				this.Invalidate(); 
			}
		}


		/// <summary>
		/// Gets or sets the tooltip.
		/// </summary>
		[
		SRCategory("CategoryAttributeMapArea"),

        Bindable(true),
        SRDescription("DescriptionAttributeToolTip"),
		DefaultValue(""),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public string ToolTip
		{
			set
			{
				this.Invalidate(); 
				_toolTip = value;
			}
			get
			{
				return _toolTip;
			}
		}

#if !Microsoft_CONTROL

		/// <summary>
		/// Gets or sets the URL.
		/// </summary>
		[
		SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
		SRDescription("DescriptionAttributeUrl"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute),
	        Editor(Editors.UrlValueEditor.Editor, Editors.UrlValueEditor.Base)

		]
		public string Url
		{
			set
			{
				_url = value;
				this.Invalidate(); 
			}
			get
			{
				return _url;
			}
		}

        /// <summary>
		/// Gets or sets the other map area attributes.
		/// </summary>
		[
		SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
		SRDescription("DescriptionAttributeMapAreaAttributes"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string MapAreaAttributes
		{
			set
			{
				_attributes = value;
				this.Invalidate(); 
			}
			get
			{
				return _attributes;
			}
		}

        /// <summary>
        /// Gets or sets the postback value which can be processed on a click event.
        /// </summary>
        /// <value>The value which is passed to a click event as an argument.</value>
        [DefaultValue("")]
        [SRCategory(SR.Keys.CategoryAttributeMapArea)]
        [SRDescription(SR.Keys.DescriptionAttributePostBackValue)]
        public string PostBackValue 
        {
            get
            {
                return this._postbackValue;
            }
            set
            {
                this._postbackValue = value;
            } 
        }


#endif	//#if !Microsoft_CONTROL

        #endregion


		#region Invalidation methods

		/// <summary>
		/// Invalidate chart area
		/// </summary>
		private new void Invalidate()
		{
#if Microsoft_CONTROL

			if(this.Axis != null)
			{
				Axis.Invalidate();
			}
#endif
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
                if (_fontCache != null)
                {
                    _fontCache.Dispose();
                    _fontCache = null;
                }
            }
            base.Dispose(disposing);
        }


        #endregion
	}
}
