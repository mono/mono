//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		AxisScaleSegments.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	AxisScaleSegment, AxisScaleSegmentCollection
//
//  Purpose:	
//
//	Reviewed:	
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
#if Microsoft_CONTROL

	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
	using System.Windows.Forms.DataVisualization.Charting;

#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
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
	/// <b>AxisScaleSegment</b> class represents a single segment of the axis with
	/// it's own scale and intervals.
	/// </summary>
	[
	SRDescription("DescriptionAttributeAxisScaleSegment_AxisScaleSegment"),
	]
     internal class AxisScaleSegment
	{
		#region Fields

		// Associated axis
		internal Axis axis = null;

		// Axis segment position in percent of the axis size
		private double _position = 0.0;

		// Axis segment size in percent of the axis size
		private double _size = 0.0;

		// Axis segment spacing in percent of the axis size
		private double _spacing = 0.0;

		// Axis segment scale minimum value
		private double _scaleMinimum = 0.0;

		// Axis segment scale maximum value
		private double _scaleMaximum = 0.0;

		// Axis segment interval offset.
		private double _intervalOffset = 0;

		// Axis segment interval.
		private double _interval = 0;

		// Axis segment interval units type.
		private DateTimeIntervalType _intervalType = DateTimeIntervalType.Auto;

		// Axis segment interval offset units type.
		private DateTimeIntervalType _intervalOffsetType = DateTimeIntervalType.Auto;

		// Object associated with the segment
		private object _tag = null;

		// Stack used to save/load axis settings
		private Stack	_oldAxisSettings = new Stack();

		#endregion // Fields

		#region Constructor

		/// <summary>
		/// Default object constructor.
		/// </summary>
		public AxisScaleSegment()
		{
		}

		#endregion // Constructor

		#region Properties

		/// <summary>
		/// Axis segment position in axis size percentage.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeAxisScaleSegment_Position"),
		]
		public double Position
		{
			get
			{
				return this._position;
			}
			set
			{
				if(value < 0.0 || value > 100.0)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAxisScaleSegmentsPositionInvalid));
				}
				this._position = value;
			}
		}

		/// <summary>
		/// Axis segment size in axis size percentage.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeAxisScaleSegment_Size"),
		]
		public double Size
		{
			get
			{
				return this._size;
			}
			set
			{
				if(value < 0.0 || value > 100.0)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAxisScaleSegmentsSizeInvalid));
					}
				this._size = value;
			}
		}

		/// <summary>
		/// Axis segment spacing in axis size percentage.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeAxisScaleSegment_Spacing"),
		]
		public double Spacing
		{
			get
			{
				return this._spacing;
			}
			set
			{
				if(value < 0.0 || value > 100.0)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAxisScaleSegmentsSpacingInvalid));
				}
				this._spacing = value;
			}
		}

		/// <summary>
		/// Axis segment scale maximum value.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeAxisScaleSegment_ScaleMaximum"),
		]
		public double ScaleMaximum
		{
			get
			{
				return this._scaleMaximum;
			}
			set
			{
				this._scaleMaximum = value;
			}
		}

		/// <summary>
		/// Axis segment scale minimum value.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeAxisScaleSegment_ScaleMinimum"),
		]
		public double ScaleMinimum
		{
			get
			{
				return this._scaleMinimum;
			}
			set
			{
				this._scaleMinimum = value;
			}
		}


		/// <summary>
		/// Axis segment interval size.
		/// </summary>
		[
		SRCategory("CategoryAttributeInterval"),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeAxisScaleSegment_Interval"),
		TypeConverter(typeof(AxisIntervalValueConverter)),
		]
		public double Interval
		{
			get
			{
				return this._interval;
			}
			set
			{
				// Axis interval properties must be set
				if(double.IsNaN(value))
				{
					this._interval = 0;
				}
				else
				{
					this._interval = value;
				}
			}
		}

		/// <summary>
		/// Axis segment interval offset.
		/// </summary>
		[
		SRCategory("CategoryAttributeInterval"),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeAxisScaleSegment_IntervalOffset"),
		TypeConverter(typeof(AxisIntervalValueConverter))
		]
		public double IntervalOffset
		{
			get
			{
				return _intervalOffset;
			}
		}

		/// <summary>
		/// Axis segment interval type.
		/// </summary>
		[
		SRCategory("CategoryAttributeInterval"),
		DefaultValue(DateTimeIntervalType.Auto),
		SRDescription("DescriptionAttributeAxisScaleSegment_IntervalType"),
		]
		public DateTimeIntervalType IntervalType
		{
			get
			{
				return this._intervalType;
			}
			set
			{
				// Axis interval properties must be set
				if(value == DateTimeIntervalType.NotSet)
				{
					this._intervalType = DateTimeIntervalType.Auto;
				}
				else
				{
					_intervalType = value;
				}
			}
		}

		/// <summary>
		/// Axis segment interval offset type.
		/// </summary>
		[
		SRCategory("CategoryAttributeInterval"),
		DefaultValue(DateTimeIntervalType.Auto),
		SRDescription("DescriptionAttributeAxisScaleSegment_IntervalOffsetType"),
		]
		public DateTimeIntervalType IntervalOffsetType
		{
			get
			{
				return this._intervalOffsetType;
			}
		}

		/// <summary>
		/// Object associated with axis scale segment.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		Browsable(false),
		DefaultValue(null),
		SRDescription("DescriptionAttributeAxisScaleSegment_Tag"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
		SerializationVisibility(SerializationVisibility.Hidden),
		]
		public object Tag
		{
			get
			{
				return this._tag;
			}
			set
			{
				this._tag = value;
			}
		}

		#endregion // Properties

		#region Break Line Painting Methods

        /// <summary>
        /// Paints the axis break line.
        /// </summary>
        /// <param name="graph">Chart graphics to use.</param>
        /// <param name="nextSegment">Axis scale segment next to current.</param>
		internal void PaintBreakLine(ChartGraphics graph, AxisScaleSegment nextSegment)
		{
			// Get break line position 
			RectangleF breakPosition = this.GetBreakLinePosition(graph, nextSegment);

			// Get top line graphics path
			GraphicsPath breakLinePathTop = this.GetBreakLinePath(breakPosition, true);
			GraphicsPath breakLinePathBottom = null;

			// Clear break line space using chart color behind the area
			if(breakPosition.Width > 0f && breakPosition.Height > 0f)
			{
				// Get bottom line graphics path
				breakLinePathBottom = this.GetBreakLinePath(breakPosition, false);

				// Clear plotting area background
				using(GraphicsPath fillPath = new GraphicsPath())
				{
					// Create fill path out of top and bottom break lines
					fillPath.AddPath(breakLinePathTop, true);
					fillPath.Reverse();
					fillPath.AddPath(breakLinePathBottom, true);
					fillPath.CloseAllFigures();

					// Use chart back color to fill the area
					using(Brush fillBrush = this.GetChartFillBrush(graph))
					{
						graph.FillPath(fillBrush, fillPath);

						// Check if shadow exsits in chart area
						if( this.axis.ChartArea.ShadowOffset != 0 && !this.axis.ChartArea.ShadowColor.IsEmpty)
						{
							// Clear shadow
							RectangleF shadowPartRect = breakPosition;
							if( this.axis.AxisPosition == AxisPosition.Right || this.axis.AxisPosition == AxisPosition.Left )
							{
								shadowPartRect.Y += this.axis.ChartArea.ShadowOffset;
								shadowPartRect.Height -= this.axis.ChartArea.ShadowOffset;
								shadowPartRect.X = shadowPartRect.Right - 1;
								shadowPartRect.Width = this.axis.ChartArea.ShadowOffset + 2;
							}
							else
							{
								shadowPartRect.X += this.axis.ChartArea.ShadowOffset;
								shadowPartRect.Width -= this.axis.ChartArea.ShadowOffset;
								shadowPartRect.Y = shadowPartRect.Bottom - 1;
								shadowPartRect.Height = this.axis.ChartArea.ShadowOffset + 2;
							}
							graph.FillRectangle(fillBrush, shadowPartRect);

							// Draw new shadow
							using(GraphicsPath shadowPath = new GraphicsPath())
							{
								shadowPath.AddPath(breakLinePathTop, false);

								// Define maximum size
								float size = this.axis.ChartArea.ShadowOffset;
								if( this.axis.AxisPosition == AxisPosition.Right || this.axis.AxisPosition == AxisPosition.Left )
								{
									size = Math.Min(size, breakPosition.Height);
								}
								else
								{
									size = Math.Min(size, breakPosition.Width);
								}

								// Define step to increase transperancy
								int transparencyStep = (int)(this.axis.ChartArea.ShadowColor.A / size);

								// Set clip region to achieve spacing of the shadow
								// Start with the plotting rectangle position
								RectangleF clipRegion = graph.GetAbsoluteRectangle(this.axis.PlotAreaPosition.ToRectangleF());
								if( this.axis.AxisPosition == AxisPosition.Right || this.axis.AxisPosition == AxisPosition.Left )
								{
									clipRegion.X += this.axis.ChartArea.ShadowOffset;
									clipRegion.Width += this.axis.ChartArea.ShadowOffset;
								}
								else
								{
									clipRegion.Y += this.axis.ChartArea.ShadowOffset;
									clipRegion.Height += this.axis.ChartArea.ShadowOffset;
								}
								graph.SetClip(graph.GetRelativeRectangle(clipRegion));

								// Draw several lines to form shadow
								for(int index = 0; index < size; index ++)
								{
									using(Matrix newMatrix = new Matrix())
									{
										// Shift top break line by 1 pixel
										if( this.axis.AxisPosition == AxisPosition.Right || this.axis.AxisPosition == AxisPosition.Left )
										{
											newMatrix.Translate(0f, 1f);
										}
										else
										{
											newMatrix.Translate(1f, 0f);
										}
										shadowPath.Transform(newMatrix);
									}

									// Get line color
									Color color = Color.FromArgb(
										this.axis.ChartArea.ShadowColor.A - transparencyStep * index, 
										this.axis.ChartArea.ShadowColor);

									using(Pen shadowPen = new Pen(color, 1))
									{
										// Draw shadow
										graph.DrawPath(shadowPen, shadowPath);
									}
								}

								graph.ResetClip();
							}
						}
					}
				}
			}

			// Draw Separator Line(s)
			if(this.axis.ScaleBreakStyle.BreakLineStyle != BreakLineStyle.None)
			{
				using(Pen pen = new Pen(this.axis.ScaleBreakStyle.LineColor, this.axis.ScaleBreakStyle.LineWidth))
				{
					// Set line style
					pen.DashStyle = graph.GetPenStyle(this.axis.ScaleBreakStyle.LineDashStyle);

					// Draw break lines
					graph.DrawPath(pen, breakLinePathTop);
					if(breakPosition.Width > 0f && breakPosition.Height > 0f)
					{
						graph.DrawPath(pen, breakLinePathBottom);
					}
				}
			}

			// Dispose break line paths
			breakLinePathTop.Dispose();
			breakLinePathTop = null;
			if(breakLinePathBottom != null)
			{
				breakLinePathBottom.Dispose();
				breakLinePathBottom = null;
			}

		}

		/// <summary>
		/// Get fill brush used to fill axis break lines spacing.
		/// </summary>
		/// <param name="graph">chart graphics.</param>
		/// <returns>Fill brush.</returns>
		private Brush GetChartFillBrush(ChartGraphics graph)
		{
			Chart chart = this.axis.ChartArea.Common.Chart;
			Brush brush = null;

			if( chart.BackGradientStyle == GradientStyle.None )
			{
				brush = new SolidBrush(chart.BackColor);
			}
			else
			{
				// If a gradient type is set create a brush with gradient
                brush = graph.GetGradientBrush(new RectangleF(0, 0, chart.chartPicture.Width - 1, chart.chartPicture.Height - 1), chart.BackColor, chart.BackSecondaryColor, chart.BackGradientStyle);
			}

			if( chart.BackHatchStyle != ChartHatchStyle.None )
			{
				brush = graph.GetHatchBrush( chart.BackHatchStyle, chart.BackColor, chart.BackSecondaryColor );
			}

			if( chart.BackImage.Length > 0 && 
				chart.BackImageWrapMode != ChartImageWrapMode.Unscaled && 
				chart.BackImageWrapMode != ChartImageWrapMode.Scaled)
			{
				brush = graph.GetTextureBrush(chart.BackImage, chart.BackImageTransparentColor, chart.BackImageWrapMode, chart.BackColor );
			}

			return brush;
		}

		/// <summary>
		/// Gets a path of the break line for specified position.
		/// </summary>
		/// <param name="breakLinePosition">Break line position.</param>
		/// <param name="top">Indicates if top or bottom break line path should be retrieved.</param>
		/// <returns>Graphics path with break line path.</returns>
		private GraphicsPath GetBreakLinePath(RectangleF breakLinePosition, bool top)
		{
			GraphicsPath path = new GraphicsPath();

			if(this.axis.ScaleBreakStyle.BreakLineStyle == BreakLineStyle.Wave)
			{
				PointF[] points = null;
				int pointNumber = 0;
				if( this.axis.AxisPosition == AxisPosition.Right || this.axis.AxisPosition == AxisPosition.Left )
				{
					float startX = breakLinePosition.X;
					float endX = breakLinePosition.Right;
					float y = (top) ? breakLinePosition.Y : breakLinePosition.Bottom;
					pointNumber = ((int)(endX - startX) / 40) * 2 ;
					if(pointNumber < 2)
					{
						pointNumber = 2;
					}
					float step = (endX - startX) / pointNumber;
					points = new PointF[pointNumber + 1];
					for(int pointIndex = 1; pointIndex < pointNumber + 1; pointIndex++)
					{
						points[pointIndex] = new PointF(
							startX + pointIndex * step, 
							y + ((pointIndex%2 == 0) ? -2f : 2f) );
					}
					
					points[0] = new PointF(startX, y);
					points[points.Length - 1] = new PointF(endX, y);
				}
				else
				{
					float startY = breakLinePosition.Y;
					float endY = breakLinePosition.Bottom;
					float x = (top) ? breakLinePosition.X : breakLinePosition.Right;
					pointNumber = ((int)(endY - startY) / 40) * 2 ;
					if(pointNumber < 2)
					{
						pointNumber = 2;
					}
					float step = (endY - startY) / pointNumber;
					points = new PointF[pointNumber + 1];
					for(int pointIndex = 1; pointIndex < pointNumber + 1; pointIndex++)
					{
						points[pointIndex] = new PointF(
							x + ((pointIndex%2 == 0) ? -2f : 2f),
							startY + pointIndex * step);
					}
					
					points[0] = new PointF(x, startY);
					points[points.Length - 1] = new PointF(x, endY);
				}

				path.AddCurve(points, 0, pointNumber, 0.8f);
			}
			else if(this.axis.ScaleBreakStyle.BreakLineStyle == BreakLineStyle.Ragged)
			{
				PointF[] points = null;
				Random rand = new Random(435657);
				if( this.axis.AxisPosition == AxisPosition.Right || this.axis.AxisPosition == AxisPosition.Left )
				{
					float startX = breakLinePosition.X;
					float endX = breakLinePosition.Right;
					float y = (top) ? breakLinePosition.Y : breakLinePosition.Bottom;
					float step = 10f;
					int pointNumber = (int)((endX - startX) / step);
					if(pointNumber < 2)
					{
						pointNumber = 2;
					}
					points = new PointF[pointNumber];
					
					for(int pointIndex = 1; pointIndex < pointNumber - 1; pointIndex++)
					{
						points[pointIndex] = new PointF(
							startX + pointIndex * step, 
							y + rand.Next(-3, 3) );
					}
					
					points[0] = new PointF(startX, y);
					points[points.Length - 1] = new PointF(endX, y);
				}
				else
				{
					float startY = breakLinePosition.Y;
					float endY = breakLinePosition.Bottom;
					float x = (top) ? breakLinePosition.X : breakLinePosition.Right;
					float step = 10f;
					int pointNumber = (int)((endY - startY) / step);
					if(pointNumber < 2)
					{
						pointNumber = 2;
					}
					points = new PointF[pointNumber];
					
					for(int pointIndex = 1; pointIndex < pointNumber - 1; pointIndex++)
					{
						points[pointIndex] = new PointF(
							x + rand.Next(-3, 3),
							startY + pointIndex * step );
					}
					
					points[0] = new PointF(x, startY);
					points[points.Length - 1] = new PointF(x, endY);
				}

				path.AddLines(points);
			}
			else
			{
				if( this.axis.AxisPosition == AxisPosition.Right || this.axis.AxisPosition == AxisPosition.Left )
				{
					if(top)
					{
						path.AddLine(breakLinePosition.X, breakLinePosition.Y, breakLinePosition.Right, breakLinePosition.Y);
					}
					else
					{
						path.AddLine(breakLinePosition.X, breakLinePosition.Bottom, breakLinePosition.Right, breakLinePosition.Bottom);
					}
				}
				else
				{
					if(top)
					{
						path.AddLine(breakLinePosition.X, breakLinePosition.Y, breakLinePosition.X, breakLinePosition.Bottom);
					}
					else
					{
						path.AddLine(breakLinePosition.Right, breakLinePosition.Y, breakLinePosition.Right, breakLinePosition.Bottom);
					}
				}
			}
			return path;
		}

		/// <summary>
		/// Gets position of the axis break line. Break line may be shown as a single 
		/// line or two lines separated with a spacing.
		/// </summary>
		/// <param name="graph">Chart graphics.</param>
		/// <param name="nextSegment">Next segment reference.</param>
		/// <returns>Position of the axis break line in pixel coordinates.</returns>
		internal RectangleF GetBreakLinePosition(ChartGraphics graph, AxisScaleSegment nextSegment)
		{
			// Start with the plotting rectangle position
			RectangleF breakPosition = this.axis.PlotAreaPosition.ToRectangleF();

			// Find maximum scale value of the current segment and minimuj of the next
			double from = this.axis.GetLinearPosition(nextSegment.ScaleMinimum);
			double to = this.axis.GetLinearPosition(this.ScaleMaximum);
			if( this.axis.AxisPosition == AxisPosition.Right || this.axis.AxisPosition == AxisPosition.Left )
			{
				breakPosition.Y = (float)Math.Min(from, to);
				breakPosition.Height = (float)Math.Max(from, to);
			}
			else
			{
				breakPosition.X = (float)Math.Min(from, to);
				breakPosition.Width = (float)Math.Max(from, to);;
			}

			// Convert to pixels
			breakPosition = Rectangle.Round(graph.GetAbsoluteRectangle(breakPosition));

			// Add border width
			if( this.axis.AxisPosition == AxisPosition.Right || this.axis.AxisPosition == AxisPosition.Left )
			{
				breakPosition.Height = (float)Math.Abs(breakPosition.Y - breakPosition.Height);
				breakPosition.X -= this.axis.ChartArea.BorderWidth;
				breakPosition.Width += 2 * this.axis.ChartArea.BorderWidth;
			}
			else
			{
				breakPosition.Width = (float)Math.Abs(breakPosition.X - breakPosition.Width);
				breakPosition.Y -= this.axis.ChartArea.BorderWidth;
				breakPosition.Height += 2 * this.axis.ChartArea.BorderWidth;
			}

			return breakPosition;
		}

		#endregion // Break Line Painting Methods

		#region Helper Methods

		/// <summary>
		/// Gets segment scale position and size in relative coordinates.
		/// Method takes in consideration segment spacing and space required fro separatorType.
		/// </summary>
		/// <param name="plotAreaSize">Plotting area size in relative coordinates.</param>
		/// <param name="scalePosition">Returns scale position.</param>
		/// <param name="scaleSize">Returns scale size.</param>
		internal void GetScalePositionAndSize(double plotAreaSize, out double scalePosition, out double scaleSize)
		{
			scaleSize = (this.Size - this.Spacing) * (plotAreaSize / 100.0);
			scalePosition = this.Position * (plotAreaSize / 100.0);
		}

		/// <summary>
		/// Saves axis settings and set them from the current segment.
		/// </summary>
		internal void SetTempAxisScaleAndInterval()
		{
			// Save current axis settings
			if(_oldAxisSettings.Count == 0)
			{
				_oldAxisSettings.Push(this.axis.maximum);
				_oldAxisSettings.Push(this.axis.minimum);

				_oldAxisSettings.Push(this.axis.majorGrid.interval);
				_oldAxisSettings.Push(this.axis.majorGrid.intervalType);
				_oldAxisSettings.Push(this.axis.majorGrid.intervalOffset);
				_oldAxisSettings.Push(this.axis.majorGrid.intervalOffsetType);

				_oldAxisSettings.Push(this.axis.majorTickMark.interval);
				_oldAxisSettings.Push(this.axis.majorTickMark.intervalType);
				_oldAxisSettings.Push(this.axis.majorTickMark.intervalOffset);
				_oldAxisSettings.Push(this.axis.majorTickMark.intervalOffsetType);

				_oldAxisSettings.Push(this.axis.LabelStyle.interval);
				_oldAxisSettings.Push(this.axis.LabelStyle.intervalType);
				_oldAxisSettings.Push(this.axis.LabelStyle.intervalOffset);
				_oldAxisSettings.Push(this.axis.LabelStyle.intervalOffsetType);
			}

			// Copy settings from the segment into the axis
			this.axis.maximum = this.ScaleMaximum;
			this.axis.minimum = this.ScaleMinimum;

			this.axis.majorGrid.interval = this.Interval;
			this.axis.majorGrid.intervalType = this.IntervalType;
			this.axis.majorGrid.intervalOffset = this.IntervalOffset;
			this.axis.majorGrid.intervalOffsetType = this.IntervalOffsetType;

			this.axis.majorTickMark.interval = this.Interval;
			this.axis.majorTickMark.intervalType = this.IntervalType;
			this.axis.majorTickMark.intervalOffset = this.IntervalOffset;
			this.axis.majorTickMark.intervalOffsetType = this.IntervalOffsetType;

			this.axis.LabelStyle.interval = this.Interval;
			this.axis.LabelStyle.intervalType = this.IntervalType;
			this.axis.LabelStyle.intervalOffset = this.IntervalOffset;
			this.axis.LabelStyle.intervalOffsetType = this.IntervalOffsetType;
		}

		/// <summary>
		/// Restore previously saved axis settings.
		/// </summary>
		internal void RestoreAxisScaleAndInterval()
		{
			if(_oldAxisSettings.Count > 0)
			{
				this.axis.LabelStyle.intervalOffsetType = (DateTimeIntervalType)_oldAxisSettings.Pop();
				this.axis.LabelStyle.intervalOffset = (double)_oldAxisSettings.Pop();
				this.axis.LabelStyle.intervalType = (DateTimeIntervalType)_oldAxisSettings.Pop();
				this.axis.LabelStyle.interval = (double)_oldAxisSettings.Pop();

				this.axis.majorTickMark.intervalOffsetType = (DateTimeIntervalType)_oldAxisSettings.Pop();
				this.axis.majorTickMark.intervalOffset = (double)_oldAxisSettings.Pop();
				this.axis.majorTickMark.intervalType = (DateTimeIntervalType)_oldAxisSettings.Pop();
				this.axis.majorTickMark.interval = (double)_oldAxisSettings.Pop();

				this.axis.majorGrid.intervalOffsetType = (DateTimeIntervalType)_oldAxisSettings.Pop();
				this.axis.majorGrid.intervalOffset = (double)_oldAxisSettings.Pop();
				this.axis.majorGrid.intervalType = (DateTimeIntervalType)_oldAxisSettings.Pop();
				this.axis.majorGrid.interval = (double)_oldAxisSettings.Pop();

				this.axis.minimum = (double)_oldAxisSettings.Pop();
				this.axis.maximum = (double)_oldAxisSettings.Pop();
			}
		}

		#endregion // Helper Methods

	}

	/// <summary>
	/// <b>AxisScaleSegmentCollection</b> is a class that stores collection of axis segments.
	/// </summary>
	[
	SRDescription("DescriptionAttributeAxisScaleSegmentCollection_AxisScaleSegmentCollection"),
	]
	internal class AxisScaleSegmentCollection : CollectionBase
	{
		#region Fields

		// Axis this segment collection belongs to.
		private Axis _axis = null;

		// Segment which is always used to convert scale values.
		// This value is set tmporarly when only one segment has 
		// to handle all the values.
		private AxisScaleSegment _enforcedSegment = null;

		// Indicates that values allowed to be outside of the scale segment.
		// Otherwise they will be rounded to Min and Max values.
		internal bool AllowOutOfScaleValues = false;

		#endregion // Fields

		#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		/// <remarks>
		/// This constructor is for internal use and should not be part of documentation.
		/// </remarks>
		public AxisScaleSegmentCollection()
		{
		}

		/// <summary>
		/// Default public constructor.
		/// </summary>
		/// <remarks>
		/// This constructor is for internal use and should not be part of documentation.
		/// </remarks>
		/// <param name="axis">
		/// Chart axis this collection belongs to
		/// </param>
		internal AxisScaleSegmentCollection(Axis axis)
		{
			this._axis = axis;
		}

		#endregion // Construction and Initialization

		#region Indexer

		/// <summary>
		/// Axis scale segment collection indexer.
		/// </summary>
		/// <remarks>
		/// The <b>AxisScaleSegment</b> object index can be provided as a parameter. Returns the <see cref="AxisScaleSegment"/> object. 
		/// </remarks>
		[
		SRDescription("DescriptionAttributeAxisScaleSegmentCollection_Item"),
		]
		public AxisScaleSegment this[int index] 
		{
			get 
			{ 
				return (AxisScaleSegment)this.List[(int)index]; 
			} 
		}

		#endregion // Indexer

		#region Collection Add and Insert methods

		/// <summary>
		/// Adds a segment to the end of the collection.
		/// </summary>
		/// <param name="segment">
		/// <see cref="AxisScaleSegment"/> object to add.
		/// </param>
		/// <returns>
		/// Index of the newly added object.
		/// </returns>
		public int Add(AxisScaleSegment segment)
		{
			return this.List.Add(segment);
		}


		#endregion // Collection Add and Insert methods

		#region Items Inserting and Removing Notification methods

		/// <summary>
		/// After new item inserted.
		/// </summary>
		/// <param name="index">Item index.</param>
		/// <param name="value">Item object.</param>
		/// <remarks>
		/// This is an internal method and should not be part of the documentation.
		/// </remarks>
		protected override void OnInsertComplete(int index, object value)
		{
			((AxisScaleSegment)value).axis  = this._axis;
		}

		/// <summary>
		/// After items is set.
		/// </summary>
		/// <param name="index">The zero-based index at which oldValue can be found.</param>
		/// <param name="oldValue">The value to replace with newValue.</param>
		/// <param name="newValue">The new value of the element at index.</param>
		/// <remarks>
		/// This is an internal method and should not be part of the documentation.
		/// </remarks>
		protected override void OnSetComplete(int index, object oldValue, object newValue)
		{
			((AxisScaleSegment)newValue).axis  = this._axis;
		}

		#endregion

		#region Helper Methods

        /// <summary>
        /// Ensures that specified axis scale segment is used for all coordinate transformations.
        /// Set tot NULL to reset.
        /// </summary>
        /// <param name="segment"></param>
		internal void EnforceSegment(AxisScaleSegment segment)
		{
			this._enforcedSegment = segment;
		}

		/// <summary>
		/// Find axis scale segment that should be used to translate axis value to relative coordinates.
		/// </summary>
		/// <param name="axisValue">Axis value to convert.</param>
		/// <returns>Scale segment to use for the convertion.</returns>
		public AxisScaleSegment FindScaleSegmentForAxisValue(double axisValue)
		{
			// Check if no segments defined
			if(this.List.Count == 0)
			{
				return null;
			}

			// Check if segment enforcment is enabled
			if(_enforcedSegment != null)
			{
				return _enforcedSegment;
			}

			// Iterate through all segments
			for(int index = 0; index < this.Count; index++)
			{
				if(axisValue < this[index].ScaleMinimum)
				{
					if(index == 0)
					{
						return this[index];
					}
					else
					{
						// Find the segment which is "closer" to the value
						if( Math.Abs(this[index].ScaleMinimum - axisValue) < Math.Abs(axisValue - this[index - 1].ScaleMaximum))
						{
							return this[index];
						}
						else
						{
							return this[index - 1];
						}
					}
				}

				if(axisValue <= this[index].ScaleMaximum)
				{
					return this[index];
				}
				else if(index == this.Count - 1)
				{
					return this[index];
				}
			}

			return null;
		}

		#endregion // Helper Methods
	}
}

