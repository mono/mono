//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartAreaCursor.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	Cursor, CursorEventArgs
//
//  Purpose:	A cursor is a horizontal or vertical line that 
//              defines a position along an axis. A range selection 
//              is a range along an axis that is defined by a beginning 
//              and end position, and is displayed using a semi-transparent 
//              color.
//              
//              Both cursors and range selections are implemented by the 
//              Cursor class, which is exposed as the CursorX and CursorY 
//              properties of the ChartArea object. The CursorX object is 
//              for the X axis of a chart area, and the CursorY object is 
//              for the Y axis. The AxisType property of these objects 
//              determines if the associated axis is primary or secondary.
//              
//              Cursors and range selections can be set via end-user 
//              interaction and programmatically.
//
//              NOTE: ASP.NET version of the chart uses this class only 
//              for appearance and position properties. Drawing of the 
//              selection and cursor is implemented through client side 
//              java script.
//              
//	Reviewed:	AG - August 8, 2002
//              AG - March 16, 2007
//
//===================================================================

#if WINFORMS_CONTROL

#region Used Namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.DataVisualization.Charting.Data;
using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
using System.Windows.Forms.DataVisualization.Charting.Utilities;
using System.Windows.Forms.DataVisualization.Charting.Borders3D;
using System.Drawing.Design;
using System.Collections.Generic;

#endregion

namespace System.Windows.Forms.DataVisualization.Charting
{
	/// <summary>
    /// The Cursor class is responsible for chart axes cursor and selection 
    /// functionality. It contains properties which define visual appearance, 
    /// position and behavior settings. It also contains methods for 
    /// drawing cursor and selection in the plotting area.
	/// </summary>
	[
		DefaultProperty("Enabled"),	
		SRDescription("DescriptionAttributeCursor_Cursor"),
	]
	public class Cursor : IDisposable
	{
        #region Cursor constructors and initialization

        /// <summary>
		/// Public constructor
		/// </summary>
		public Cursor()
		{
		}

		/// <summary>
		/// Initialize cursor class.
		/// </summary>
		/// <param name="chartArea">Chart area the cursor belongs to.</param>
		/// <param name="attachedToXAxis">Indicates which axes should be used X or Y.</param>
		internal void Initialize(ChartArea chartArea, AxisName attachedToXAxis)
		{
			// Set chart are reference
			this._chartArea = chartArea;

			// Attach cursor to specified axis
			this._attachedToXAxis = attachedToXAxis;
		}

        #endregion

        #region Cursor fields

        // Reference to the chart area object the cursor belongs to
		private	ChartArea				_chartArea = null;

		// Defines which axis the cursor attached to X or Y
		private AxisName				_attachedToXAxis = AxisName.X;

		// Enables/Disables chart area cursor.
		private	bool					_isUserEnabled = false;

		// Enables/Disables chart area selection.
		private	bool					_isUserSelectionEnabled = false;

		// Indicates that cursor will automatically scroll the area scaleView if necessary.
		private	bool					_autoScroll = true;
				
		// Cursor line color
		private	Color					_lineColor = Color.Red;

		// Cursor line width
		private	int						_lineWidth = 1;

		// Cursor line style
		private	ChartDashStyle			_lineDashStyle = ChartDashStyle.Solid;

		// Chart area selection color
		private	Color					_selectionColor = Color.LightGray;

		// AxisName of the axes (primary/secondary) the cursor is attached to
		private	AxisType				_axisType = AxisType.Primary;

		// Cursor position
		private	double					_position = Double.NaN;

		// Range selection start position.
		private	double					_selectionStart = Double.NaN;

		// Range selection end position.
		private	double					_selectionEnd = Double.NaN;

		// Cursor movement interval current & original values
		private double					_interval = 1;

		// Cursor movement interval type
		private	DateTimeIntervalType	_intervalType = DateTimeIntervalType.Auto;

		// Cursor movement interval offset current & original values
		private double					_intervalOffset = 0;

		// Cursor movement interval offset type
		private	DateTimeIntervalType	_intervalOffsetType = DateTimeIntervalType.Auto;

		// Reference to the axis obhect
		private	Axis					_axis = null;

		// User selection start point
		private	PointF					_userSelectionStart = PointF.Empty;

		// Indicates that selection must be drawn
		private	bool					_drawSelection = true;

        // Indicates that events must be fired when position/selection is changed
		private	bool					_fireUserChangingEvent = false;

		// Indicates that XXXChanged events must be fired when position/selection is changed
		private	bool					_fireUserChangedEvent = false;

		// Scroll size and direction when AutoScroll is set
		private	MouseEventArgs			_mouseMoveArguments = null;

		// Timer used to scroll the data while selecting
		private	System.Windows.Forms.Timer					_scrollTimer = new System.Windows.Forms.Timer();

		// Indicates that axis data scaleView was scrolled as a result of the mouse move event
		private bool					_viewScrolledOnMouseMove = false;

        #endregion

        #region Cursor "Behavior" public properties.

        /// <summary>
        /// Gets or sets the position of a cursor.
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		DefaultValue(Double.NaN),
		SRDescription("DescriptionAttributeCursor_Position"),
		ParenthesizePropertyNameAttribute(true),
        TypeConverter(typeof(DoubleDateNanValueConverter)),
		]
		public double Position
		{
			get
			{
				return _position;
			}
			set
			{
				if(_position != value)
				{
					_position = value;
					
					// Align cursor in connected areas
					if(this._chartArea != null && this._chartArea.Common != null && this._chartArea.Common.ChartPicture != null)
					{
						if(!this._chartArea.alignmentInProcess)
						{
							AreaAlignmentOrientations orientation = (this._attachedToXAxis == AxisName.X || this._attachedToXAxis == AxisName.X2) ?
								AreaAlignmentOrientations.Vertical : AreaAlignmentOrientations.Horizontal;
							this._chartArea.Common.ChartPicture.AlignChartAreasCursor(this._chartArea, orientation, false);
						}
					}

					if(this._chartArea != null && !this._chartArea.alignmentInProcess)
					{
						this.Invalidate(false);
					}
				
				}
			}
		}

		/// <summary>
        /// Gets or sets the starting position of a cursor's selected range. 
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		DefaultValue(Double.NaN),
		SRDescription("DescriptionAttributeCursor_SelectionStart"),
        TypeConverter(typeof(DoubleDateNanValueConverter)),
		]
		public double SelectionStart
		{
			get
			{
				return _selectionStart;
			}
			set
			{
				if(_selectionStart != value)
				{
					_selectionStart = value;
			
					// Align cursor in connected areas
					if(this._chartArea != null && this._chartArea.Common != null && this._chartArea.Common.ChartPicture != null)
					{
						if(!this._chartArea.alignmentInProcess)
						{
							AreaAlignmentOrientations orientation = (this._attachedToXAxis == AxisName.X || this._attachedToXAxis == AxisName.X2) ?
								AreaAlignmentOrientations.Vertical : AreaAlignmentOrientations.Horizontal;
							this._chartArea.Common.ChartPicture.AlignChartAreasCursor(this._chartArea, orientation, false);
						}
					}

					if(this._chartArea != null && !this._chartArea.alignmentInProcess)
					{
						this.Invalidate(false);
					}
					
				}
			}
		}

		/// <summary>
        /// Gets or sets the ending position of a range selection.  
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		DefaultValue(Double.NaN),
		SRDescription("DescriptionAttributeCursor_SelectionEnd"),
        TypeConverter(typeof(DoubleDateNanValueConverter)),
		]
		public double SelectionEnd
		{
			get
			{
				return _selectionEnd;
			}
			set
			{
				if(_selectionEnd != value)
				{
					_selectionEnd = value;
				
					// Align cursor in connected areas
					if(this._chartArea != null && this._chartArea.Common != null && this._chartArea.Common.ChartPicture != null)
					{
						if(!this._chartArea.alignmentInProcess)
						{
							AreaAlignmentOrientations orientation = (this._attachedToXAxis == AxisName.X || this._attachedToXAxis == AxisName.X2) ?
								AreaAlignmentOrientations.Vertical : AreaAlignmentOrientations.Horizontal;
							this._chartArea.Common.ChartPicture.AlignChartAreasCursor(this._chartArea, orientation, false);
						}
					}

					if(this._chartArea != null && !this._chartArea.alignmentInProcess)
					{
						this.Invalidate(false);
					}
				}
			}
		}

		/// <summary>
        /// Gets or sets a property that enables or disables the cursor interface.
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		DefaultValue(false),
		SRDescription("DescriptionAttributeCursor_UserEnabled"),
		]
		public bool IsUserEnabled
		{
			get
			{
				return _isUserEnabled;
			}
			set
			{
				_isUserEnabled = value;
			}
		}

		/// <summary>
        /// Gets or sets a property that enables or disables the range selection interface.
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		DefaultValue(false),
        SRDescription("DescriptionAttributeCursor_UserSelection"),
		]
		public bool IsUserSelectionEnabled
		{
			get
			{
				return _isUserSelectionEnabled;
			}
			set
			{
				_isUserSelectionEnabled = value;
			}
		}

		/// <summary>
        /// Determines if scrolling will occur if a range selection operation 
        /// extends beyond a boundary of the chart area.
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		DefaultValue(true),
		SRDescription("DescriptionAttributeCursor_AutoScroll"),
		]
		public bool AutoScroll
		{
			get
			{
				return _autoScroll;
			}
			set
			{
				_autoScroll = value;
			}
		}

		/// <summary>
        ///  Gets or sets the type of axis that the cursor is attached to.
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		SRDescription("DescriptionAttributeCursor_AxisType"),
        DefaultValue(AxisType.Primary)
		]
		public AxisType AxisType
		{
			get
			{
				return _axisType;
			}
			set
			{
				_axisType = value;

				// Reset reference to the axis object
				_axis = null;

				this.Invalidate(true);
			}
		}

		/// <summary>
        /// Gets or sets the cursor movement interval.
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		DefaultValue(1.0),
		SRDescription("DescriptionAttributeCursor_Interval"),
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
			}
		}

		/// <summary>
        /// Gets or sets the unit of measurement of the Interval property.
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		DefaultValue(DateTimeIntervalType.Auto),
        SRDescription("DescriptionAttributeCursor_IntervalType")
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
			}
		}


		/// <summary>
        /// Gets or sets the interval offset, which determines 
        /// where to draw the cursor and range selection.
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		DefaultValue(0.0),
        SRDescription("DescriptionAttributeCursor_IntervalOffset"),
		]
		public double IntervalOffset
		{
			get
			{
				return _intervalOffset;
			}
			set
			{
				// Validation
				if( value < 0.0 )
				{
                    throw (new ArgumentException(SR.ExceptionCursorIntervalOffsetIsNegative, "value")); 
				}

				_intervalOffset = value;
			}
		}

		/// <summary>
		/// Gets or sets the unit of measurement of the IntervalOffset property.
		/// </summary>
		[
		SRCategory("CategoryAttributeBehavior"),
		Bindable(true),
		DefaultValue(DateTimeIntervalType.Auto),
        SRDescription("DescriptionAttributeCursor_IntervalOffsetType"),
		]
		public DateTimeIntervalType IntervalOffsetType
		{
			get
			{
				return _intervalOffsetType;
			}
			set
			{
				_intervalOffsetType = (value != DateTimeIntervalType.NotSet) ? value : DateTimeIntervalType.Auto;
			}
		}
		#endregion

		#region Cursor "Appearance" public properties

		/// <summary>
        /// Gets or sets the color the cursor line.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), "Red"),
        SRDescription("DescriptionAttributeLineColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		]
		public Color LineColor
		{
			get
			{
				return _lineColor;
			}
			set
			{
				_lineColor = value;
				this.Invalidate(false);
			}
		}

		/// <summary>
        /// Gets or sets the style of the cursor line.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartDashStyle.Solid),
        SRDescription("DescriptionAttributeLineDashStyle"),
		]
		public ChartDashStyle LineDashStyle
		{
			get
			{
				return _lineDashStyle;
			}
			set
			{
				_lineDashStyle = value;
				this.Invalidate(false);
			}
		}

		/// <summary>
        /// Gets or sets the width of the cursor line.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(1),
        SRDescription("DescriptionAttributeLineWidth"),
		]
		public int LineWidth
		{
			get
			{
				return _lineWidth;
			}
			set
			{
				if(value < 0)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionCursorLineWidthIsNegative));
				}
				_lineWidth = value;
				this.Invalidate(true);
			}
		}

		/// <summary>
        /// Gets or sets a semi-transparent color that highlights a range of data.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), "LightGray"),
		SRDescription("DescriptionAttributeCursor_SelectionColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		]
		public Color SelectionColor
		{
			get
			{
				return _selectionColor;
			}
			set
			{
				_selectionColor = value;
				this.Invalidate(false);
			}
		}

		#endregion

		#region Cursor painting methods

		/// <summary>
		/// Draws chart area cursor and selection.
		/// </summary>
		/// <param name="graph">Reference to the ChartGraphics object.</param>
		internal void Paint( ChartGraphics graph )
		{
			//***************************************************
			//** Prepare for drawing
			//***************************************************

			// Do not proceed with painting if cursor is not attached to the axis
			if(this.GetAxis() == null || 
				this._chartArea == null ||
				this._chartArea.Common == null ||
				this._chartArea.Common.ChartPicture == null ||
				this._chartArea.Common.ChartPicture.isPrinting)
			{
				return;
			}
			
			// Get plot area position
			RectangleF	plotAreaPosition = this._chartArea.PlotAreaPosition.ToRectangleF();

			// Detect if cursor is horizontal or vertical
			bool	horizontal = true;
			if(this.GetAxis().AxisPosition == AxisPosition.Bottom || this.GetAxis().AxisPosition == AxisPosition.Top)
			{
				horizontal = false;
			}
			
			//***************************************************
			//** Draw selection
			//***************************************************

			// Check if selection need to be drawn
			if(this._drawSelection &&
				!double.IsNaN(this.SelectionStart) && 
				!double.IsNaN(this.SelectionEnd) && 
				this.SelectionColor != Color.Empty)
			{
				// Calculate selection rectangle
				RectangleF	rectSelection = GetSelectionRect(plotAreaPosition);
				rectSelection.Intersect(plotAreaPosition);

				// Get opposite axis selection rectangle
				RectangleF	rectOppositeSelection = GetOppositeSelectionRect(plotAreaPosition);

				// Draw selection if rectangle is not empty
				if(!rectSelection.IsEmpty && rectSelection.Width > 0 && rectSelection.Height > 0)
				{
					// Limit selection rectangle to the area of the opposite selection
					if(!rectOppositeSelection.IsEmpty && rectOppositeSelection.Width > 0 && rectOppositeSelection.Height > 0)
					{
						rectSelection.Intersect(rectOppositeSelection);

						// We do not need to draw selection in the opposite axis 
						Cursor oppositeCursor = 
							(_attachedToXAxis == AxisName.X || _attachedToXAxis == AxisName.X2) ? 
							_chartArea.CursorY : _chartArea.CursorX;
						oppositeCursor._drawSelection = false;
					}

					// Make sure selection is inside plotting area
					rectSelection.Intersect(plotAreaPosition);

					// If selection rectangle is not empty
					if(rectSelection.Width > 0 && rectSelection.Height > 0)
					{
						// Add transparency to solid colors
						Color	rangeSelectionColor = this.SelectionColor;
						if(rangeSelectionColor.A == 255)
						{
							rangeSelectionColor = Color.FromArgb(120, rangeSelectionColor);
						}

						// Draw selection
						graph.FillRectangleRel( rectSelection, 
							rangeSelectionColor, 
							ChartHatchStyle.None, 
							"", 
							ChartImageWrapMode.Tile, 
							Color.Empty,
							ChartImageAlignmentStyle.Center,
							GradientStyle.None, 
							Color.Empty,
							Color.Empty, 
							0, 
							ChartDashStyle.NotSet,
							Color.Empty,
							0,
							PenAlignment.Inset );
					}
				}
			}

			//***************************************************
			//** Draw cursor
			//***************************************************

			// Check if cursor need to be drawn
			if(!double.IsNaN(this.Position) && 
				this.LineColor != Color.Empty && 
				this.LineWidth > 0 && 
				this.LineDashStyle != ChartDashStyle.NotSet)
			{
				// Calculate line position
				bool	insideArea = false;
				PointF	point1 = PointF.Empty;
				PointF	point2 = PointF.Empty;
				if(horizontal)
				{
					// Set cursor coordinates
					point1.X = plotAreaPosition.X;
					point1.Y = (float)this.GetAxis().GetLinearPosition(this.Position);
					point2.X = plotAreaPosition.Right;
					point2.Y = point1.Y;

					// Check if cursor is inside plotting rect
					if(point1.Y >= plotAreaPosition.Y && point1.Y <= plotAreaPosition.Bottom)
					{
						insideArea = true;
					}
				}
				else
				{
					// Set cursor coordinates
					point1.X = (float)this.GetAxis().GetLinearPosition(this.Position);
					point1.Y = plotAreaPosition.Y;
					point2.X = point1.X;
					point2.Y = plotAreaPosition.Bottom;

					// Check if cursor is inside plotting rect
					if(point1.X >= plotAreaPosition.X && point1.X <= plotAreaPosition.Right)
					{
						insideArea = true;
					}
				}

				// Draw cursor if it's inside the chart area plotting rectangle
				if(insideArea)
				{
					graph.DrawLineRel(this.LineColor, this.LineWidth, this.LineDashStyle, point1, point2);
				}
			}
			// Reset draw selection flag
			this._drawSelection = true;
		}

        #endregion

		#region Cursor position setting methods

		/// <summary>
        /// This method sets the position of a cursor within a chart area at a given axis value.
		/// </summary>
        /// <param name="newPosition">The new position of the cursor.  Measured as a value along the relevant axis.</param>
		public void SetCursorPosition(double newPosition)
		{
			// Check if we are setting different value
			if(this.Position != newPosition)
			{
				double newRoundedPosition = RoundPosition(newPosition);
				// Send PositionChanging event
				if(_fireUserChangingEvent && GetChartObject() != null)
				{
					CursorEventArgs	arguments = new CursorEventArgs(this._chartArea, this.GetAxis(), newRoundedPosition);
					GetChartObject().OnCursorPositionChanging(arguments);

					// Check if position values were changed in the event
					newRoundedPosition = arguments.NewPosition;
				}

                // Change position
				this.Position = newRoundedPosition;

				// Send PositionChanged event
				if(_fireUserChangedEvent && GetChartObject() != null)
				{
					CursorEventArgs	arguments = new CursorEventArgs(this._chartArea, this.GetAxis(), this.Position);
					GetChartObject().OnCursorPositionChanged(arguments);
				}
			}
		}


		/// <summary>
        /// This method displays a cursor at the specified position.  Measured in pixels.
		/// </summary>
        /// <param name="point">A PointF structure that specifies where the cursor will be drawn.</param>
        /// <param name="roundToBoundary">If true, the cursor will be drawn along the nearest chart area boundary 
        /// when the specified position does not fall within a ChartArea object.</param>
		public void SetCursorPixelPosition(PointF point, bool roundToBoundary)
		{
			if(this._chartArea != null && this._chartArea.Common != null && this.GetAxis() != null)
			{
				PointF relativeCoord = GetPositionInPlotArea(point, roundToBoundary);
				if(!relativeCoord.IsEmpty)
				{
					// Get new cursor position
					double newCursorPosition = PositionToCursorPosition(relativeCoord);

					// Set new cursor & selection position
					this.SetCursorPosition(newCursorPosition);
				}
			}
		}

		/// <summary>
        /// This method sets the position of a selected range within a chart area at given axis values. 
		/// </summary>
        /// <param name="newStart">The new starting position of the range selection.  Measured as a value along the relevant axis..</param>
        /// <param name="newEnd">The new ending position of the range selection.  Measured as a value along the relevant axis.</param>
		public void SetSelectionPosition(double newStart, double newEnd)
		{
			// Check if we are setting different value
			if(this.SelectionStart != newStart || this.SelectionEnd != newEnd)
			{
				// Send PositionChanging event
				double newRoundedSelectionStart = RoundPosition(newStart);
				double newRoundedSelectionEnd = RoundPosition(newEnd);

				// Send SelectionRangeChanging event
				if(_fireUserChangingEvent && GetChartObject() != null)
				{
					CursorEventArgs	arguments = new CursorEventArgs(this._chartArea, this.GetAxis(), newRoundedSelectionStart, newRoundedSelectionEnd);
					GetChartObject().OnSelectionRangeChanging(arguments);

					// Check if position values were changed in the event
					newRoundedSelectionStart = arguments.NewSelectionStart;
					newRoundedSelectionEnd = arguments.NewSelectionEnd;
				}

				// Change selection position
				this._selectionStart = newRoundedSelectionStart;
				this._selectionEnd = newRoundedSelectionEnd;
				
				// Align cursor in connected areas
				if(this._chartArea != null && this._chartArea.Common != null && this._chartArea.Common.ChartPicture != null)
				{
					if(!this._chartArea.alignmentInProcess)
					{
						AreaAlignmentOrientations orientation = (this._attachedToXAxis == AxisName.X || this._attachedToXAxis == AxisName.X2) ?
							AreaAlignmentOrientations.Vertical : AreaAlignmentOrientations.Horizontal;
						this._chartArea.Common.ChartPicture.AlignChartAreasCursor(this._chartArea, orientation, true);
					}
				}

				if(this._chartArea != null && !this._chartArea.alignmentInProcess)
				{
					this.Invalidate(false);
				}
					
				// Send SelectionRangeChanged event
				if(_fireUserChangedEvent && GetChartObject() != null)
				{
					CursorEventArgs	arguments = new CursorEventArgs(this._chartArea, this.GetAxis(), this.SelectionStart, this.SelectionEnd);
					GetChartObject().OnSelectionRangeChanged(arguments);
				}
			}
		}


		/// <summary>
        /// This method sets the starting and ending positions of a range selection.
		/// </summary>
        /// <param name="startPoint">A PointF structure that specifies where the range selection begins.</param>
        /// <param name="endPoint">A PointF structure that specifies where the range selection ends</param>
        /// <param name="roundToBoundary">If true, the starting and ending points will be rounded to the nearest chart area boundary 
        /// when the specified positions do not fall within a ChartArea object.</param>
		public void SetSelectionPixelPosition(PointF startPoint, PointF endPoint, bool roundToBoundary)
		{
			if(this._chartArea != null && this._chartArea.Common != null && this.GetAxis() != null)
			{
				// Calculating the start position
				double newStart = this.SelectionStart;
				if(!startPoint.IsEmpty)
				{
					PointF relativeCoord = GetPositionInPlotArea(startPoint, roundToBoundary);
					if(!relativeCoord.IsEmpty)
					{
						// Get new selection start position
						newStart = PositionToCursorPosition(relativeCoord);
					}
				}

				// Setting the end position
				double newEnd = newStart;
				if(!endPoint.IsEmpty)
				{
					PointF relativeCoord = GetPositionInPlotArea(endPoint, roundToBoundary);
					if(!relativeCoord.IsEmpty)
					{
						// Get new selection position
						newEnd = PositionToCursorPosition(relativeCoord);
					}
				}

				// Set new selection start & end position
				this.SetSelectionPosition(newStart, newEnd);
			}
		}

        #endregion

        #region Position rounding methods

        /// <summary>
		/// Rounds new position of the cursor or range selection
		/// </summary>
		/// <param name="cursorPosition"></param>
		/// <returns></returns>
		internal double RoundPosition(double cursorPosition)
		{
			double roundedPosition = cursorPosition;

			if(!double.IsNaN(roundedPosition))
			{
				// Check if position rounding is required
				if(this.GetAxis() != null &&
					this.Interval != 0 && 
                    !double.IsNaN(this.Interval))
				{
					// Get first series attached to this axis
					Series	axisSeries = null;
					if(_axis.axisType == AxisName.X || _axis.axisType == AxisName.X2)
					{
						List<string> seriesArray = _axis.ChartArea.GetXAxesSeries((_axis.axisType == AxisName.X) ? AxisType.Primary : AxisType.Secondary, _axis.SubAxisName);
						if(seriesArray.Count > 0)
						{
                            string seriesName = seriesArray[0] as string;
							axisSeries = _axis.Common.DataManager.Series[seriesName];
							if(axisSeries != null && !axisSeries.IsXValueIndexed)
							{
								axisSeries = null;
							}
						}
					}

					// If interval type is not set - use number
					DateTimeIntervalType intervalType = 
						(this.IntervalType == DateTimeIntervalType.Auto) ? 
						DateTimeIntervalType.Number : this.IntervalType;

					// If interval offset type is not set - use interval type
					DateTimeIntervalType offsetType = 
						(this.IntervalOffsetType == DateTimeIntervalType.Auto) ? 
					intervalType : this.IntervalOffsetType;
				
					// Round numbers
					if(intervalType == DateTimeIntervalType.Number)
					{
						double	newRoundedPosition = Math.Round(roundedPosition / this.Interval) * this.Interval;

						// Add offset number
						if(this.IntervalOffset != 0 && 
							!double.IsNaN(IntervalOffset) && 
							offsetType != DateTimeIntervalType.Auto)
						{
							if(this.IntervalOffset > 0)
							{
                                newRoundedPosition += ChartHelper.GetIntervalSize(newRoundedPosition, this.IntervalOffset, offsetType);
							}
							else
							{
                                newRoundedPosition -= ChartHelper.GetIntervalSize(newRoundedPosition, this.IntervalOffset, offsetType);
							}
						}

						// Find rounded position after/before the current
						double nextPosition = newRoundedPosition;
						if(newRoundedPosition <= cursorPosition)
						{
                            nextPosition += ChartHelper.GetIntervalSize(newRoundedPosition, this.Interval, intervalType, axisSeries, 0, DateTimeIntervalType.Number, true);
						}
						else
						{
                            nextPosition -= ChartHelper.GetIntervalSize(newRoundedPosition, this.Interval, intervalType, axisSeries, 0, DateTimeIntervalType.Number, true);
						}

						// Choose closest rounded position
						if(Math.Abs(nextPosition - cursorPosition) > Math.Abs(cursorPosition - newRoundedPosition))
						{
							roundedPosition = newRoundedPosition;
						}
						else
						{
							roundedPosition = nextPosition;
						}

					}

						// Round date/time
					else
					{
						// Find one rounded position prior and one after current position
						// Adjust start position depending on the interval and type
                        double prevPosition = ChartHelper.AlignIntervalStart(cursorPosition, this.Interval, intervalType, axisSeries);

						// Adjust start position depending on the interval offset and offset type
						if( IntervalOffset != 0 && axisSeries == null)
						{
							if(this.IntervalOffset > 0)
							{
                                prevPosition += ChartHelper.GetIntervalSize(
									prevPosition, 
									this.IntervalOffset, 
									offsetType, 
									axisSeries, 
									0, 
									DateTimeIntervalType.Number, 
									true);
							}
							else
							{
                                prevPosition += ChartHelper.GetIntervalSize(
									prevPosition, 
									-this.IntervalOffset, 
									offsetType, 
									axisSeries, 
									0, 
									DateTimeIntervalType.Number, 
									true);
							}
						}

						// Find rounded position after/before the current
						double nextPosition = prevPosition;
						if(prevPosition <= cursorPosition)
						{
                            nextPosition += ChartHelper.GetIntervalSize(prevPosition, this.Interval, intervalType, axisSeries, 0, DateTimeIntervalType.Number, true);
						}
						else
						{
                            nextPosition -= ChartHelper.GetIntervalSize(prevPosition, this.Interval, intervalType, axisSeries, 0, DateTimeIntervalType.Number, true);
						}

						// Choose closest rounded position
						if(Math.Abs(nextPosition - cursorPosition) > Math.Abs(cursorPosition - prevPosition))
						{
							roundedPosition = prevPosition;
						}
						else
						{
							roundedPosition = nextPosition;
						}
					}
				}
			}

			return roundedPosition;
		}
        #endregion

        #region Mouse events handling for the Cursor

		/// <summary>
		/// Mouse down event handler.
		/// </summary>
		internal void Cursor_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// Set flag to fire position changing events
			_fireUserChangingEvent = true;
			_fireUserChangedEvent = false;

			// Check if left mouse button was clicked in chart area
			if(e.Button == MouseButtons.Left && !GetPositionInPlotArea(new PointF(e.X, e.Y), false).IsEmpty)
			{
				// Change cursor position and selection start position when mouse down
				if(this.IsUserEnabled)
				{
					SetCursorPixelPosition(new PointF(e.X, e.Y), false);
				}
				if(this.IsUserSelectionEnabled)
				{
					this._userSelectionStart = new PointF(e.X, e.Y);
					SetSelectionPixelPosition(this._userSelectionStart, PointF.Empty, false);
				}
			}

			// Clear flag to fire position changing events
			_fireUserChangingEvent = false;
			_fireUserChangedEvent = false;
		}

		/// <summary>
		/// Mouse up event handler.
		/// </summary>
		internal void Cursor_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// If in range selection mode
			if(!this._userSelectionStart.IsEmpty)
			{
				// Stop timer
				_scrollTimer.Stop();
				_mouseMoveArguments = null;

				// Check if axis data scaleView zooming UI is enabled
				if(this._axis != null && 
					this._axis.ScaleView.Zoomable &&
					!double.IsNaN(this.SelectionStart) &&
					!double.IsNaN(this.SelectionEnd) &&
					this.SelectionStart != this.SelectionEnd)
				{
					// Zoom data scaleView
					double	start = Math.Min(this.SelectionStart, this.SelectionEnd); 
					double	size = (double)Math.Max(this.SelectionStart, this.SelectionEnd) - start;
					bool zoomed = this._axis.ScaleView.Zoom(start, size, DateTimeIntervalType.Number, true, true);

					// Clear image buffer
					if(this._chartArea.areaBufferBitmap != null && zoomed)
					{
						this._chartArea.areaBufferBitmap.Dispose();
						this._chartArea.areaBufferBitmap = null;
					}
			
					// Clear range selection
					this.SelectionStart = double.NaN;
					this.SelectionEnd = double.NaN;

                    // NOTE: Fixes issue #6823
                    // Clear cursor position after the zoom in operation
                    this.Position = double.NaN;

					// Align cursor in connected areas
					if(this._chartArea != null && this._chartArea.Common != null && this._chartArea.Common.ChartPicture != null)
					{
						if(!this._chartArea.alignmentInProcess)
						{
							AreaAlignmentOrientations orientation = (this._attachedToXAxis == AxisName.X || this._attachedToXAxis == AxisName.X2) ?
								AreaAlignmentOrientations.Vertical : AreaAlignmentOrientations.Horizontal;
							this._chartArea.Common.ChartPicture.AlignChartAreasZoomed(this._chartArea, orientation, zoomed);
						}
					}
				}

				// Fire XXXChanged events
				if(GetChartObject() != null)
				{
					CursorEventArgs	arguments = new CursorEventArgs(this._chartArea, this.GetAxis(), this.SelectionStart, this.SelectionEnd);
					GetChartObject().OnSelectionRangeChanged(arguments);

					arguments = new CursorEventArgs(this._chartArea, this.GetAxis(), this.Position);
					GetChartObject().OnCursorPositionChanged(arguments);
				}

				// Stop range selection mode
				this._userSelectionStart = PointF.Empty;
			}
		}

		/// <summary>
		/// Mouse move event handler.
		/// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges", Justification = "The timer is used for simulating scrolling behavior")]
		internal void Cursor_MouseMove(System.Windows.Forms.MouseEventArgs e, ref bool handled)
		{
			// Process range selection
			if(this._userSelectionStart != PointF.Empty)
			{
				// Mouse move event should not be handled by any other chart elements
				handled = true;

				// Set flag to fire position changing events
				_fireUserChangingEvent = true;
				_fireUserChangedEvent = false;

				// Check if mouse position is outside of the chart area and if not - try scrolling
				if(this.AutoScroll)
				{
					if(this._chartArea != null && this._chartArea.Common != null && this.GetAxis()!= null)
					{
						// Check if axis data scaleView is enabled
						if(!double.IsNaN(this._axis.ScaleView.Position) && !double.IsNaN(this._axis.ScaleView.Size))
						{
							ScrollType	scrollType = ScrollType.SmallIncrement;
							bool		insideChartArea = true;
							double		offsetFromBoundary = 0.0;

							// Translate mouse pixel coordinates into the relative chart area coordinates
							float mouseX = e.X * 100F / ((float)(this._chartArea.Common.Width - 1)); 
							float mouseY = e.Y * 100F / ((float)(this._chartArea.Common.Height - 1)); 

							// Check if coordinate is inside chart plotting area
							if(this._axis.AxisPosition == AxisPosition.Bottom || this._axis.AxisPosition == AxisPosition.Top)
							{
								if(mouseX < this._chartArea.PlotAreaPosition.X)
								{
									scrollType = ScrollType.SmallDecrement;
									insideChartArea = false;
									offsetFromBoundary = this._chartArea.PlotAreaPosition.X - mouseX;
								}
								else if(mouseX > this._chartArea.PlotAreaPosition.Right)
								{
									scrollType = ScrollType.SmallIncrement;
									insideChartArea = false;
									offsetFromBoundary = mouseX - this._chartArea.PlotAreaPosition.Right;
								}
							}
							else
							{
								if(mouseY < this._chartArea.PlotAreaPosition.Y)
								{
									scrollType = ScrollType.SmallIncrement;
									insideChartArea = false;
									offsetFromBoundary = this._chartArea.PlotAreaPosition.Y - mouseY;
								}
								else if(mouseY > this._chartArea.PlotAreaPosition.Bottom)
								{
									scrollType = ScrollType.SmallDecrement;
									insideChartArea = false;
									offsetFromBoundary = mouseY - this._chartArea.PlotAreaPosition.Bottom;
								}
							}
				
							// Try scrolling scaleView position
							if(!insideChartArea)
							{
								// Set flag that data scaleView was scrolled
								_viewScrolledOnMouseMove = true;

								// Get minimum scroll interval
                                double scrollInterval = ChartHelper.GetIntervalSize(
									this._axis.ScaleView.Position, 
									this._axis.ScaleView.GetScrollingLineSize(), 
									this._axis.ScaleView.GetScrollingLineSizeType());
								offsetFromBoundary *= 2;
								if(offsetFromBoundary > scrollInterval)
								{
									scrollInterval = ((int)(offsetFromBoundary / scrollInterval)) * scrollInterval;
								}

								// Scroll axis data scaleView
								double	newDataViewPosition = this._axis.ScaleView.Position;
								if(scrollType == ScrollType.SmallIncrement)
								{
									newDataViewPosition += scrollInterval;
								}
								else
								{
									newDataViewPosition -= scrollInterval;
								}
								
								// Scroll axis data scaleView
								this._axis.ScaleView.Scroll(newDataViewPosition);

								// Save last mouse move arguments
								_mouseMoveArguments = new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta);

								// Start selection scrolling timer
								if(!_scrollTimer.Enabled)
								{
									// Start timer
									_scrollTimer.Tick += new EventHandler(SelectionScrollingTimerEventProcessor);
									_scrollTimer.Interval = 200;
									_scrollTimer.Start();
								}
							}
							else
							{
								// Stop timer
								_scrollTimer.Stop();
								_mouseMoveArguments = null;
							}
						}
					}
				}

				// Change cursor position and selection end position when mouse moving
				if(this.IsUserEnabled)
				{
					SetCursorPixelPosition(new PointF(e.X, e.Y), true);
				}
				if(this.IsUserSelectionEnabled)
				{
					// Set selection
					SetSelectionPixelPosition(PointF.Empty, new PointF(e.X, e.Y), true);
				}

				// Clear flag to fire position changing events
				_fireUserChangingEvent = false;
				_fireUserChangedEvent = false;

				// Clear flag that data scaleView was scrolled
				_viewScrolledOnMouseMove = false;

			}
		}

		/// <summary>
		/// This is the method to run when the timer is raised.
		/// Used to scroll axis data scaleView while mouse is outside of the chart area.
		/// </summary>
		/// <param name="myObject"></param>
		/// <param name="myEventArgs"></param>
		private void SelectionScrollingTimerEventProcessor(Object myObject, EventArgs myEventArgs) 
		{
			// Simulate mouse move events
			if(_mouseMoveArguments != null)
			{
				bool handled = false;
				this.Cursor_MouseMove(_mouseMoveArguments, ref handled);
			}
		}

        #endregion

#region Cursor helper methods

		/// <summary>
		/// Helper function which returns a reference to the chart object
		/// </summary>
		/// <returns>Chart object reference.</returns>
		private Chart GetChartObject()
		{
			if(this._chartArea != null )
			{
                return this._chartArea.Chart;
			}

			return null;
		}

		/// <summary>
		/// Get rectangle of the axis range selection.
		/// </summary>
		/// <returns>Selection rectangle.</returns>
		/// <param name="plotAreaPosition">Plot area rectangle.</param>
		/// <returns></returns>
		private RectangleF GetSelectionRect(RectangleF	plotAreaPosition)
		{
			RectangleF	rect = RectangleF.Empty;

			if(this._axis != null &&
				this.SelectionStart != this.SelectionEnd)
			{
				double		start = (float)this._axis.GetLinearPosition(this.SelectionStart);
				double		end = (float)this._axis.GetLinearPosition(this.SelectionEnd);

				// Detect if cursor is horizontal or vertical
				bool	horizontal = true;
				if(this.GetAxis().AxisPosition == AxisPosition.Bottom || this.GetAxis().AxisPosition == AxisPosition.Top)
				{
					horizontal = false;
				}

				if(horizontal)
				{
					rect.X = plotAreaPosition.X;
					rect.Width = plotAreaPosition.Width;
					rect.Y = (float)Math.Min(start, end);
					rect.Height = (float)Math.Max(start, end) - rect.Y;
				}
				else
				{
					rect.Y = plotAreaPosition.Y;
					rect.Height = plotAreaPosition.Height;
					rect.X = (float)Math.Min(start, end);
					rect.Width = (float)Math.Max(start, end) - rect.X;
				}
			}

			return rect;
		}

		/// <summary>
		/// Get rectangle of the opposite axis selection
		/// </summary>
		/// <param name="plotAreaPosition">Plot area rectangle.</param>
		/// <returns>Opposite selection rectangle.</returns>
		private RectangleF GetOppositeSelectionRect(RectangleF	plotAreaPosition)
		{
			if(_chartArea != null)
			{
				// Get opposite cursor 
				Cursor oppositeCursor = 
					(_attachedToXAxis == AxisName.X || _attachedToXAxis == AxisName.X2) ? 
					_chartArea.CursorY : _chartArea.CursorX;
				return oppositeCursor.GetSelectionRect(plotAreaPosition);
			}

			return RectangleF.Empty;
		}

		/// <summary>
		/// Converts X or Y position value to the cursor axis value
		/// </summary>
		/// <param name="position">Position in relative coordinates.</param>
		/// <returns>Cursor position as axis value.</returns>
		private double PositionToCursorPosition(PointF position)
		{
			// Detect if cursor is horizontal or vertical
			bool	horizontal = true;
			if(this.GetAxis().AxisPosition == AxisPosition.Bottom || this.GetAxis().AxisPosition == AxisPosition.Top)
			{
				horizontal = false;
			}

			// Convert relative coordinates into axis values
			double newCursorPosition = double.NaN;
			if(horizontal)
			{
				newCursorPosition = this.GetAxis().PositionToValue(position.Y);
			}
			else
			{
				newCursorPosition = this.GetAxis().PositionToValue(position.X);
			}

			// Round new position using Step & StepType properties
			newCursorPosition = RoundPosition(newCursorPosition);

			return newCursorPosition;
		}


		/// <summary>
		/// Checks if specified point is located inside the plotting area.
		/// Converts pixel coordinates to relative.
		/// </summary>
		/// <param name="point">Point coordinates to test.</param>
		/// <param name="roundToBoundary">Indicates that coordinates must be rounded to area boundary.</param>
		/// <returns>PointF.IsEmpty or relative coordinates in plotting area.</returns>
		private PointF GetPositionInPlotArea(PointF point, bool roundToBoundary)
		{
			PointF	result = PointF.Empty;

			if(this._chartArea != null && this._chartArea.Common != null && this.GetAxis()!= null)
			{
				// Translate mouse pixel coordinates into the relative chart area coordinates
				result.X = point.X * 100F / ((float)(this._chartArea.Common.Width - 1)); 
				result.Y = point.Y * 100F / ((float)(this._chartArea.Common.Height - 1)); 

				// Round coordinate if it' outside chart plotting area
				RectangleF	plotAreaPosition = this._chartArea.PlotAreaPosition.ToRectangleF();
				if(roundToBoundary)
				{
					if(result.X < plotAreaPosition.X)
					{
						result.X = plotAreaPosition.X;
					}
					if(result.X > plotAreaPosition.Right)
					{
						result.X = plotAreaPosition.Right;
					}
					if(result.Y < plotAreaPosition.Y)
					{
						result.Y = plotAreaPosition.Y;
					}
					if(result.Y > plotAreaPosition.Bottom)
					{
						result.Y = plotAreaPosition.Bottom;
					}
				}
				else
				{
					// Check if coordinate is inside chart plotting area
					if(result.X < plotAreaPosition.X || 
						result.X > plotAreaPosition.Right ||
						result.Y < plotAreaPosition.Y ||
						result.Y > plotAreaPosition.Bottom)
					{
						result = PointF.Empty;
					}
				}
			}

			return result;
		}

        /// <summary>
		/// Invalidate chart are with the cursor.
		/// </summary>
		/// <param name="invalidateArea">Chart area must be invalidated.</param>
		private void Invalidate(bool invalidateArea)
		{
			if(this.GetChartObject() != null && this._chartArea != null && !this.GetChartObject().disableInvalidates)
			{
				// If data scaleView was scrolled - just invalidate the chart area
				if(_viewScrolledOnMouseMove || invalidateArea || this.GetChartObject().dirtyFlag)
				{
					this._chartArea.Invalidate();
				}

				// If only cursor/selection position was changed - use optimized drawing algorithm
				else
				{
					// Set flag to redraw cursor/selection only
					this.GetChartObject().paintTopLevelElementOnly = true;

					// Invalidate and update the chart
					this._chartArea.Invalidate();
					this.GetChartObject().Update();

					// Clear flag to redraw cursor/selection only
					this.GetChartObject().paintTopLevelElementOnly = false;
				}
			}
		}

		/// <summary>
		/// Gets axis objects the cursor is attached to.
		/// </summary>
		/// <returns>Axis object.</returns>
		internal Axis GetAxis()
		{
			if(_axis == null && _chartArea != null)
			{
				if(_attachedToXAxis == AxisName.X)
				{
					_axis = (_axisType == AxisType.Primary) ? _chartArea.AxisX : _chartArea.AxisX2;
				}
				else
				{
					_axis = (_axisType == AxisType.Primary) ? _chartArea.AxisY : _chartArea.AxisY2;
				}
			}

			return _axis;
		}

        #endregion

#region IDisposable Members

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (this._scrollTimer != null)
                {
                    this._scrollTimer.Dispose();
                    this._scrollTimer = null;
                }
            }
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
    /// The CursorEventArgs class stores the event arguments for cursor and selection events.
	/// </summary>
	public class CursorEventArgs : EventArgs
	{
#region Private fields

		// Private fields for properties values storage
		private		ChartArea		_chartArea = null;
		private		Axis			_axis = null;
		private		double			_newPosition = double.NaN;
		private		double			_newSelectionStart = double.NaN;
		private		double			_newSelectionEnd = double.NaN;

        #endregion

#region Constructors

		/// <summary>
        /// CursorEventArgs constructor.
		/// </summary>
		/// <param name="chartArea">ChartArea of the cursor.</param>
		/// <param name="axis">Axis of the cursor.</param>
		/// <param name="newPosition">New cursor position.</param>
		public CursorEventArgs(ChartArea chartArea, Axis axis, double newPosition)
		{
			this._chartArea = chartArea;
			this._axis = axis;
			this._newPosition = newPosition;
			this._newSelectionStart = double.NaN;
			this._newSelectionEnd = double.NaN;
		}

		/// <summary>
        /// CursorEventArgs constructor.
		/// </summary>
		/// <param name="chartArea">ChartArea of the cursor.</param>
		/// <param name="axis">Axis of the cursor.</param>
		/// <param name="newSelectionStart">New range selection starting position.</param>
        /// <param name="newSelectionEnd">New range selection ending position.</param>
		public CursorEventArgs(ChartArea chartArea, Axis axis, double newSelectionStart, double newSelectionEnd)
		{
			this._chartArea = chartArea;
			this._axis = axis;
			this._newPosition = double.NaN;
			this._newSelectionStart = newSelectionStart;
			this._newSelectionEnd = newSelectionEnd;
		}

        #endregion

#region Properties

		/// <summary>
		/// ChartArea of the event.
		/// </summary>
		[
		SRDescription("DescriptionAttributeChartArea"),
		]
		public ChartArea ChartArea
		{
			get
			{
				return _chartArea;
			}
		}

		/// <summary>
		/// Axis of the event.
		/// </summary>
		[
		SRDescription("DescriptionAttributeAxis"),
		]
        public Axis Axis
		{
			get
			{
				return _axis;
			}
		}

		/// <summary>
		/// New cursor position.
		/// </summary>
		[
		SRDescription("DescriptionAttributeCursorEventArgs_NewPosition"),
		]
		public double NewPosition
		{
			get
			{
				return _newPosition;
			}
			set
			{
				_newPosition = value;
			}
		}

		/// <summary>
		/// New range selection starting position.
		/// </summary>
		[
		SRDescription("DescriptionAttributeCursorEventArgs_NewSelectionStart"),
		]
		public double NewSelectionStart
		{
			get
			{
				return _newSelectionStart;
			}
			set
			{
				_newSelectionStart = value;
			}
		}

		/// <summary>
		/// New range selection ending position.
		/// </summary>
		[
		SRDescription("DescriptionAttributeCursorEventArgs_NewSelectionEnd"),
		]
		public double NewSelectionEnd
		{
			get
			{
				return _newSelectionEnd;
			}
			set
			{
				_newSelectionEnd = value;
			}
		}

        #endregion
	}
}

#endif	// #if WINFORMS_CONTROL