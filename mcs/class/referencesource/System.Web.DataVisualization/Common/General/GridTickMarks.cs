//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		GridTickMarks.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	TickMark, Grid
//
//  Purpose:	Axis tick marks and grid lines a very similar chart 
//              elements and most of the functionality is located 
//              in the Grid class. TickMark class is derived from 
//              the Grid class and provides tick mark specific 
//              functionality.
//
//	Reviewed:	AG - Jul 31, 2002
//              AG - Microsoft 14, 2007
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
#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.Utilities;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
    namespace System.Web.UI.DataVisualization.Charting
#endif

{
	#region Tick marks style enumeration

	/// <summary>
	/// An enumeration of tick mark styles.
	/// </summary>
	public enum TickMarkStyle
	{
		/// <summary>
		/// Tickmarks are disabled.
		/// </summary>
		None, 
		/// <summary>
		/// Tickmarks are located outside of the chart area.
		/// </summary>
		OutsideArea, 
		/// <summary>
		/// Tickmarks are located inside of the chart area.
		/// </summary>
		InsideArea, 
		/// <summary>
		/// Tickmarks are set across the axis line.
		/// </summary>
		AcrossAxis
	};

	#endregion

    /// <summary>
    /// The TickMark class represents axis tick marks which are drawn next to 
    /// the axis line. TickMark shares many common properties with the Grid 
    /// class. This class also contains methods for tick marks drawing.
    /// </summary>
	[
		DefaultProperty("Enabled"),
		SRDescription("DescriptionAttributeTickMark_TickMark"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class TickMark : Grid
	{
        #region Private fields and Constructors

        // Tick marks style
		private TickMarkStyle	_style = TickMarkStyle.OutsideArea;

		// Tick marks size
		private float			_size = 1;

		/// <summary>
		/// Public default constructor
		/// </summary>
		public TickMark() : base(null, true)
		{
		}

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="axis">Axis which owns the grid or tick mark.</param>
        /// <param name="major">Major axis element.</param>
		internal TickMark(Axis axis, bool major) : base(axis, major)
		{
		}

		#endregion

		#region Tick marks painting method

		/// <summary>
		/// Draws and hit test for TickMarks.
		/// </summary>
		/// <param name="graph">Reference to the Chart Graphics object.</param>
		/// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
		internal void Paint( ChartGraphics graph, bool backElements )
		{
			PointF first = PointF.Empty; // The First point of a tick mark
			PointF second = PointF.Empty; // The Second point of a tick mark
			float axisPosition; // Axis position. 

			// Tick Marks are disabled
			if( !this.enabled )
			{
				return;
			}

			// ****************************************************************
			// This code creates auto interval for auto tick marks and 
			// gridlines. If type is not date there are always four tickmarks 
			// or gridlines between major gridlines and tickmarks. For date 
			// type interval is calculated using CalcInterval function.
			// ****************************************************************
			double oldInterval = this.interval;
			DateTimeIntervalType oldIntervalType = this.intervalType;
			double oldIntervalOffset = this.intervalOffset;
			DateTimeIntervalType oldIntervalOffsetType = this.intervalOffsetType;
			if( !this.majorGridTick && ( this.interval == 0 || double.IsNaN(this.interval) ) )
			{
				// Number type
                if (this.Axis.majorGrid.GetIntervalType() == DateTimeIntervalType.Auto)
				{
                    this.interval = this.Axis.majorGrid.GetInterval() / Grid.NumberOfIntervals;
				}
					// Date type
				else
				{
                    DateTimeIntervalType localIntervalType = this.Axis.majorGrid.GetIntervalType();
                    this.interval = Axis.CalcInterval(
                        this.Axis.ViewMinimum,
                        this.Axis.ViewMinimum + (this.Axis.ViewMaximum - this.Axis.ViewMinimum) / Grid.NumberOfDateTimeIntervals, 
						true, 
						out localIntervalType, 
						ChartValueType.DateTime );
					this.intervalType = localIntervalType;
                    this.intervalOffsetType = this.Axis.majorGrid.GetIntervalOffsetType();
                    this.intervalOffset = this.Axis.majorGrid.GetIntervalOffset();
				}
			}

			if( _style == TickMarkStyle.None )
			{
				return;
			}

			// Check if custom tick marks should be drawn from custom labels
            if (Axis.IsCustomTickMarks())
			{
				PaintCustom(graph, backElements);
				return;
			}

			// Get first series attached to this axis
			Series	axisSeries = null;
            if (Axis.axisType == AxisName.X || Axis.axisType == AxisName.X2)
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

            // Current position for tick mark is minimum
            double current = Axis.ViewMinimum;

            // Get offse type
            DateTimeIntervalType offsetType = (GetIntervalOffsetType() == DateTimeIntervalType.Auto) ? GetIntervalType() : GetIntervalOffsetType();


            // ***********************************
            // Check if the AJAX zooming and scrolling mode is enabled.
            // ***********************************

			// Adjust start position depending on the interval type
            if (!Axis.ChartArea.chartAreaIsCurcular ||
                Axis.axisType == AxisName.Y ||
                Axis.axisType == AxisName.Y2)
			{
                current = ChartHelper.AlignIntervalStart(current, this.GetInterval(), this.GetIntervalType(), axisSeries, this.majorGridTick);
			}

			// The Current position is start position, not minimum
            if (GetIntervalOffset() != 0 && !double.IsNaN(GetIntervalOffset()) && axisSeries == null)
			{
                current += ChartHelper.GetIntervalSize(current, GetIntervalOffset(), 
					offsetType, axisSeries, 0, DateTimeIntervalType.Number, true, false);
			}

			// Too many tick marks
            if ((Axis.ViewMaximum - Axis.ViewMinimum) / ChartHelper.GetIntervalSize(current, this.GetInterval(), this.GetIntervalType(), axisSeries, 0, DateTimeIntervalType.Number, true) > ChartHelper.MaxNumOfGridlines)
			{
				return;
			}
		
			// If Maximum, minimum and interval don’t have 
			// proper value do not draw tick marks.
            if (Axis.ViewMaximum <= Axis.ViewMinimum)
			{
				return;
			}

			// Axis scroll bar will increase size of the Outside and Cross style tick marks
			float	scrollBarSize = 0;

#if Microsoft_CONTROL

            if (this.Axis.ScrollBar.IsVisible &&
                this.Axis.ScrollBar.IsPositionedInside &&
                (this.Axis.IsAxisOnAreaEdge || !this.Axis.IsMarksNextToAxis))
			{
                scrollBarSize = (float)this.Axis.ScrollBar.GetScrollBarRelativeSize();
			}

#endif // Microsoft_CONTROL

			// Left tickmarks
            if (Axis.AxisPosition == AxisPosition.Left)
			{
				// The tick marks will follow axis or they will 
				// be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
				else
                    axisPosition = Axis.PlotAreaPosition.X;

				if( _style == TickMarkStyle.InsideArea )
				{
					first.X = axisPosition;
					second.X = axisPosition + _size;
				}
				else if( _style == TickMarkStyle.OutsideArea )
				{
					first.X = axisPosition - _size - scrollBarSize;
					second.X = axisPosition;
				}
				else if( _style == TickMarkStyle.AcrossAxis )
				{
					first.X = axisPosition - _size/2 - scrollBarSize;
					second.X = axisPosition + _size/2;
				}
			}

			// Right tickmarks
            else if (Axis.AxisPosition == AxisPosition.Right)
			{
				// The tick marks will follow axis or they will 
				// be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
				else
                    axisPosition = Axis.PlotAreaPosition.Right;

				if( _style == TickMarkStyle.InsideArea )
				{
					first.X = axisPosition - _size;
					second.X = axisPosition;
				}
				else if( _style == TickMarkStyle.OutsideArea )
				{
					first.X = axisPosition;
					second.X = axisPosition + _size + scrollBarSize;
				}
				else if( _style == TickMarkStyle.AcrossAxis )
				{
					first.X = axisPosition - _size/2;
					second.X = axisPosition + _size/2 + scrollBarSize;
				}
			}

			// Top tickmarks
            else if (Axis.AxisPosition == AxisPosition.Top)
			{
				// The tick marks will follow axis or they will 
				// be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
				else
                    axisPosition = Axis.PlotAreaPosition.Y;

				if( _style == TickMarkStyle.InsideArea )
				{
					first.Y = axisPosition;
					second.Y = axisPosition + _size;
				}
				else if( _style == TickMarkStyle.OutsideArea )
				{
					first.Y = axisPosition - _size - scrollBarSize;
					second.Y = axisPosition;
				}
				else if( _style == TickMarkStyle.AcrossAxis )
				{
					first.Y = axisPosition - _size/2 - scrollBarSize;
					second.Y = axisPosition + _size/2;
				}
			}

			// Bottom tickmarks
            else if (Axis.AxisPosition == AxisPosition.Bottom)
			{
				// The tick marks will follow axis or they will 
				// be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
				else
                    axisPosition = Axis.PlotAreaPosition.Bottom;

				if( _style == TickMarkStyle.InsideArea )
				{
					first.Y = axisPosition - _size;
					second.Y = axisPosition;
				}
				else if( _style == TickMarkStyle.OutsideArea )
				{
					first.Y = axisPosition;
					second.Y = axisPosition + _size + scrollBarSize;
				}
				else if( _style == TickMarkStyle.AcrossAxis )
				{
					first.Y = axisPosition - _size/2;
					second.Y = axisPosition + _size/2 + scrollBarSize;
				}
			}


			// Loop for drawing grid tick marks
			int	counter = 0;
			int logStep = 1;
			double oldCurrent = current;
			double interval = 0;
            while (current <= Axis.ViewMaximum)
			{
				double logInterval = 0;

				// Take an interval between gridlines. Interval 
				// depends on interval type.
                if (this.majorGridTick || this.Axis.IsLogarithmic == false)
				{
					// Take an interval between tickmarks. Interval 
					// depends on interval type.
                    interval = ChartHelper.GetIntervalSize(current, this.GetInterval(), this.GetIntervalType(), axisSeries, this.GetIntervalOffset(), offsetType, true);

				}
				// Code for linear minor gridlines and tickmarks 
				// if scale is logarithmic.
				else
				{
					// This code is used only for logarithmic scale and minor tick marks or 
					// gridlines which have linear minor scale in logarithmic major scale. 
					// This code is used to find minimum value for the interval. For example 
					// if logarithmic base is 2 and interval is between 4 and 8; current value 
					// is 5.6; this method will return linearised value for 4. This code works 
					// like Math.Floor for logarithmic scale.
					double logMinimum = this.GetLogMinimum( current, axisSeries );

					if( oldCurrent != logMinimum )
					{
						oldCurrent = logMinimum;
						logStep = 1;
					}
					
					// Find interval for logarithmic linearised scale
                    logInterval = Math.Log(1 + this.interval * logStep, Axis.logarithmBase);

					current = oldCurrent;

					interval = logInterval;
					
					logStep++;

					// Reset current position if major interval is passed.
					if( this.GetLogMinimum( current + logInterval, axisSeries ) != logMinimum )
					{
						current += logInterval;
						continue;
					}
				}

				// For indexed series do not draw the last tickmark
                if (current == Axis.ViewMaximum && axisSeries != null)
				{
					current += interval;
					
					continue;
				}

				// Check interval size
				if( interval == 0 )
				{
                    throw (new InvalidOperationException(SR.ExceptionTickMarksIntervalIsZero));
				}

                // Check if we do not exceed max number of elements
                if (counter++ > ChartHelper.MaxNumOfGridlines)
                {
                    break;
                }

				// Do not draw the very first tick mark for circular chart area
                if (this.Axis != null && this.Axis.ChartArea != null)
				{
                    if (this.Axis.ChartArea.chartAreaIsCurcular &&
                        ((this.Axis.IsReversed == false && current == Axis.ViewMinimum) ||
                        (this.Axis.IsReversed == true && current == Axis.ViewMaximum)))
					{
						current += interval;
												
						continue;
					}
				}

                if (!this.majorGridTick && this.Axis.IsLogarithmic)
				{
					current += logInterval;

                    if (current > Axis.ViewMaximum)
					{
						break;
					}
				}

                if ((decimal)current >= (decimal)Axis.ViewMinimum)
				{
					// Left tickmarks
                    if (Axis.AxisPosition == AxisPosition.Left)
					{
                        first.Y = (float)Axis.GetLinearPosition(current);
						second.Y = first.Y;
					}

					// Right tickmarks
                    else if (Axis.AxisPosition == AxisPosition.Right)
					{
                        first.Y = (float)Axis.GetLinearPosition(current);
						second.Y = first.Y;
					}

					// Top tickmarks
                    else if (Axis.AxisPosition == AxisPosition.Top)
					{
                        first.X = (float)Axis.GetLinearPosition(current);
						second.X = first.X;
					}

					// Bottom tickmarks
                    else if (Axis.AxisPosition == AxisPosition.Bottom)
					{
                        first.X = (float)Axis.GetLinearPosition(current);
						second.X = first.X;
					}

                    if (Axis.Common.ProcessModeRegions)
					{
                        if (this.Axis.ChartArea.chartAreaIsCurcular)
						{
							RectangleF rect = new RectangleF( first.X - 0.5f, first.Y - 0.5f, Math.Abs( second.X - first.X ) + 1, Math.Abs( second.Y - first.Y ) + 1 );
                            using (GraphicsPath path = new GraphicsPath())
                            {
                                path.AddRectangle(graph.GetAbsoluteRectangle(rect));
                                path.Transform(graph.Transform);
                                this.Axis.Common.HotRegionsList.AddHotRegion(
                                    path,
                                    false,
                                    ChartElementType.TickMarks,
                                    this);
                            }
						}
                        else if (!this.Axis.ChartArea.Area3DStyle.Enable3D || this.Axis.ChartArea.chartAreaIsCurcular)
						{
							RectangleF rect = new RectangleF( first.X - 0.5f, first.Y - 0.5f, Math.Abs( second.X - first.X ) + 1, Math.Abs( second.Y - first.Y ) + 1 );

                            Axis.Common.HotRegionsList.AddHotRegion(rect, this, ChartElementType.TickMarks, true);
						}
                        else
                        {
                            if (!Axis.Common.ProcessModePaint) //if ProcessModePaint is true it will be called later
                                Draw3DTickLine(graph, first, second, backElements);
                        }

					}

                    if (Axis.Common.ProcessModePaint)
					{
						// Draw grid line
                        if (!this.Axis.ChartArea.Area3DStyle.Enable3D || this.Axis.ChartArea.chartAreaIsCurcular)
						{
							graph.DrawLineRel( borderColor, borderWidth, borderDashStyle, first, second );
						}
						else
						{
							Draw3DTickLine(graph, first, second, backElements);
						}
					}
				}

				// Move position
                if (this.majorGridTick || this.Axis.IsLogarithmic == false)
				{
					current += interval;
				}
			}

			// Used for auto interval for auto tick marks and 
			// gridlines
			if( !this.majorGridTick )
			{
				this.interval = oldInterval;
				this.intervalType = oldIntervalType;
				this.intervalOffset = oldIntervalOffset;
				this.intervalOffsetType = oldIntervalOffsetType;
			}
		}

		/// <summary>
		/// This method returns linearized logarithmic value 
		/// which is minimum for range with interval 1.
		/// </summary>
		/// <param name="current">Current value</param>
		/// <param name="axisSeries">First series attached to axis.</param>
		/// <returns>Returns Minimum for the range which contains current value</returns>
		private double GetLogMinimum( double current, Series axisSeries )
		{
            double viewMinimum = Axis.ViewMinimum;
			DateTimeIntervalType offsetType = (GetIntervalOffsetType() == DateTimeIntervalType.Auto) ? GetIntervalType() : GetIntervalOffsetType();
			if( GetIntervalOffset() != 0 && axisSeries == null)
			{
                viewMinimum += ChartHelper.GetIntervalSize(viewMinimum, GetIntervalOffset(), 
					offsetType, axisSeries, 0, DateTimeIntervalType.Number, true, false);
			}

			return viewMinimum + Math.Floor( ( current - viewMinimum ));
		}

		/// <summary>
		/// Draws and hit test for custom TickMarks from the custom labels collection.
		/// </summary>
		/// <param name="graph">Reference to the Chart Graphics object.</param>
		/// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
		internal void PaintCustom( ChartGraphics graph, bool backElements )
		{
			PointF first = PointF.Empty;	// The First point of a tick mark
			PointF second = PointF.Empty;	// The Second point of a tick mark
			float axisPosition;				// Axis position. 

			// Axis scroll bar will increase size of the Outside and Cross style tick marks
			float	scrollBarSize = 0;

#if Microsoft_CONTROL

            if (this.Axis.ScrollBar.IsVisible && this.Axis.ScrollBar.IsPositionedInside && this.Axis.IsAxisOnAreaEdge)
			{
                scrollBarSize = (float)this.Axis.ScrollBar.GetScrollBarRelativeSize();
			}

#endif // Microsoft_CONTROL

			// Left tickmarks
            if (Axis.AxisPosition == AxisPosition.Left)
			{
				// The tick marks will follow axis or they will 
				// be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
				else
                    axisPosition = Axis.PlotAreaPosition.X;

				if( _style == TickMarkStyle.InsideArea )
				{
					first.X = axisPosition;
					second.X = axisPosition + _size;
				}
				else if( _style == TickMarkStyle.OutsideArea )
				{
					first.X = axisPosition - _size - scrollBarSize;
					second.X = axisPosition;
				}
				else if( _style == TickMarkStyle.AcrossAxis )
				{
					first.X = axisPosition - _size/2 - scrollBarSize;
					second.X = axisPosition + _size/2;
				}
			}

			// Right tickmarks
            else if (Axis.AxisPosition == AxisPosition.Right)
			{
				// The tick marks will follow axis or they will 
				// be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
				else
                    axisPosition = Axis.PlotAreaPosition.Right;

				if( _style == TickMarkStyle.InsideArea )
				{
					first.X = axisPosition - _size;
					second.X = axisPosition;
				}
				else if( _style == TickMarkStyle.OutsideArea )
				{
					first.X = axisPosition;
					second.X = axisPosition + _size + scrollBarSize;
				}
				else if( _style == TickMarkStyle.AcrossAxis )
				{
					first.X = axisPosition - _size/2;
					second.X = axisPosition + _size/2 + scrollBarSize;
				}
			}

			// Top tickmarks
            else if (Axis.AxisPosition == AxisPosition.Top)
			{
				// The tick marks will follow axis or they will 
				// be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
				else
                    axisPosition = Axis.PlotAreaPosition.Y;

				if( _style == TickMarkStyle.InsideArea )
				{
					first.Y = axisPosition;
					second.Y = axisPosition + _size;
				}
				else if( _style == TickMarkStyle.OutsideArea )
				{
					first.Y = axisPosition - _size - scrollBarSize;
					second.Y = axisPosition;
				}
				else if( _style == TickMarkStyle.AcrossAxis )
				{
					first.Y = axisPosition - _size/2 - scrollBarSize;
					second.Y = axisPosition + _size/2;
				}
			}

			// Bottom tickmarks
            else if (Axis.AxisPosition == AxisPosition.Bottom)
			{
				// The tick marks will follow axis or they will 
				// be always on the border of the chart area.
                if (Axis.GetIsMarksNextToAxis())
                    axisPosition = (float)Axis.GetAxisPosition();
				else
                    axisPosition = Axis.PlotAreaPosition.Bottom;

				if( _style == TickMarkStyle.InsideArea )
				{
					first.Y = axisPosition - _size;
					second.Y = axisPosition;
				}
				else if( _style == TickMarkStyle.OutsideArea )
				{
					first.Y = axisPosition;
					second.Y = axisPosition + _size + scrollBarSize;
				}
				else if( _style == TickMarkStyle.AcrossAxis )
				{
					first.Y = axisPosition - _size/2;
					second.Y = axisPosition + _size/2 + scrollBarSize;
				}
			}

			// Loop through all custom labels
            foreach (CustomLabel label in Axis.CustomLabels)
			{
				if((label.GridTicks & GridTickTypes.TickMark) == GridTickTypes.TickMark)
				{
					double	position = (label.ToPosition + label.FromPosition) / 2.0;
                    if (position >= Axis.ViewMinimum && position <= Axis.ViewMaximum)
					{
						// Left tickmarks
                        if (Axis.AxisPosition == AxisPosition.Left)
						{
                            first.Y = (float)Axis.GetLinearPosition(position);
							second.Y = first.Y;
						}

							// Right tickmarks
                        else if (Axis.AxisPosition == AxisPosition.Right)
						{
                            first.Y = (float)Axis.GetLinearPosition(position);
							second.Y = first.Y;
						}

							// Top tickmarks
                        else if (Axis.AxisPosition == AxisPosition.Top)
						{
                            first.X = (float)Axis.GetLinearPosition(position);
							second.X = first.X;
						}

							// Bottom tickmarks
                        else if (Axis.AxisPosition == AxisPosition.Bottom)
						{
                            first.X = (float)Axis.GetLinearPosition(position);
							second.X = first.X;
						}

                        if (Axis.Common.ProcessModeRegions)
						{
                            if (!this.Axis.ChartArea.Area3DStyle.Enable3D || this.Axis.ChartArea.chartAreaIsCurcular)
							{
								RectangleF rect = new RectangleF( first.X - 0.5f, first.Y - 0.5f, Math.Abs( second.X - first.X ) + 1, Math.Abs( second.Y - first.Y ) + 1 );

                                Axis.Common.HotRegionsList.AddHotRegion(rect, this, ChartElementType.TickMarks, true);
							}
							else
							{
								Draw3DTickLine(graph, first, second, backElements);
							}
						}

                        if (Axis.Common.ProcessModePaint)
						{
							// Draw grid line
                            if (!this.Axis.ChartArea.Area3DStyle.Enable3D || this.Axis.ChartArea.chartAreaIsCurcular)
							{
								graph.DrawLineRel( borderColor, borderWidth, borderDashStyle, first, second );
							}
							else
							{
								Draw3DTickLine(graph, first, second, backElements);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Draws tick mark line in 3D space.
		/// </summary>
		/// <param name="graph">Reference to the Chart Graphics object.</param>
		/// <param name="point1">First line point.</param>
		/// <param name="point2">Second line point.</param>
		/// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
		internal void Draw3DTickLine(
			ChartGraphics graph, 
			PointF point1, 
			PointF point2, 
			bool backElements
			)
		{
		
			ChartArea	area = this.Axis.ChartArea;

			//*****************************************************************
			//** Set the tick marks line depth
			//*****************************************************************
			bool	axisOnEdge;
			float	wallZPosition = Axis.GetMarksZPosition(out axisOnEdge);

			//*****************************************************************
			//** Check if tick mark should be drawn as back or front element
			//*****************************************************************

			// Check if axis tick marks are drawn inside plotting area
			bool	tickMarksOnEdge = axisOnEdge;
			if(tickMarksOnEdge &&
                this.Axis.MajorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis ||
                this.Axis.MajorTickMark.TickMarkStyle == TickMarkStyle.InsideArea ||
                this.Axis.MinorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis ||
                this.Axis.MinorTickMark.TickMarkStyle == TickMarkStyle.InsideArea)
			{
				tickMarksOnEdge = false;
			}

			SurfaceNames surfaceName = (wallZPosition == 0f) ? SurfaceNames.Back : SurfaceNames.Front;
			if( !area.ShouldDrawOnSurface(surfaceName, backElements, tickMarksOnEdge) )
			{
				// Skip drawing
				return;
			}

			//*****************************************************************
			//** Add area scene wall width to the length of the tick mark
			//*****************************************************************
            if (Axis.AxisPosition == AxisPosition.Bottom &&
                (!Axis.GetIsMarksNextToAxis() || axisOnEdge) &&
				area.IsBottomSceneWallVisible())
			{
				point2.Y += area.areaSceneWallWidth.Height;
			}
            else if (Axis.AxisPosition == AxisPosition.Left &&
                (!Axis.GetIsMarksNextToAxis() || axisOnEdge) &&
				area.IsSideSceneWallOnLeft())
			{
				point1.X -= area.areaSceneWallWidth.Width;
			}
            else if (Axis.AxisPosition == AxisPosition.Right &&
                (!Axis.GetIsMarksNextToAxis() || axisOnEdge) &&
				!area.IsSideSceneWallOnLeft())
			{
				point2.X += area.areaSceneWallWidth.Width;
			}
            else if (Axis.AxisPosition == AxisPosition.Top &&
                (!Axis.GetIsMarksNextToAxis() || axisOnEdge))
			{
				point1.Y -= area.areaSceneWallWidth.Height;
			}

			//*****************************************************************
			//** Adjust grid line direction for the Top axis
			//*****************************************************************
			Point3D	point3 = null, point4 = null;
			if(axisOnEdge && area.areaSceneWallWidth.Width != 0f)
			{
                if (Axis.AxisPosition == AxisPosition.Top)
				{
					// Always use plot area position to draw tick mark
                    float axisPosition = Axis.PlotAreaPosition.Y;

					if( _style == TickMarkStyle.InsideArea )
					{
						point1.Y = axisPosition;
						point2.Y = axisPosition + _size;

						point3 = new Point3D(point1.X, point1.Y, - area.areaSceneWallWidth.Width);
						point4 = new Point3D(point1.X, point1.Y, 0f);
					}
					else if( _style == TickMarkStyle.OutsideArea )
					{
						point1.Y = axisPosition;
						point2.Y = axisPosition;

						point3 = new Point3D(point1.X, axisPosition, wallZPosition);
						point4 = new Point3D(point1.X, point1.Y, - _size - area.areaSceneWallWidth.Width);
					}
					else if( _style == TickMarkStyle.AcrossAxis )
					{
						point1.Y = axisPosition;
						point2.Y = axisPosition + _size/2;

						point3 = new Point3D(point1.X, axisPosition, wallZPosition);
						point4 = new Point3D(point1.X, point1.Y,  - _size/2 - area.areaSceneWallWidth.Width);
					}

					// Do not show "bent" tick marks on the top surface
					if(area.ShouldDrawOnSurface(SurfaceNames.Top, backElements, false))
					{
						point3 = null;
						point4 = null;
					}
				}

				//*****************************************************************
				//** Adjust grid line direction for the Left axis
				//*****************************************************************
                if (Axis.AxisPosition == AxisPosition.Left && !area.IsSideSceneWallOnLeft())
				{
					// Always use plot area position to draw tick mark
                    float axisPosition = Axis.PlotAreaPosition.X;

					if( _style == TickMarkStyle.InsideArea )
					{
						point1.X = axisPosition;
						point2.X = axisPosition + _size;

						point3 = new Point3D(point1.X, point1.Y, - area.areaSceneWallWidth.Width);
						point4 = new Point3D(point1.X, point1.Y, 0f);
					}
					else if( _style == TickMarkStyle.OutsideArea )
					{
						point1.X = axisPosition;
						point2.X = axisPosition;

						point3 = new Point3D(axisPosition, point1.Y, wallZPosition);
						point4 = new Point3D(axisPosition, point1.Y, - _size - area.areaSceneWallWidth.Width);
					}
					else if( _style == TickMarkStyle.AcrossAxis )
					{
						point1.X = axisPosition;
						point2.X = axisPosition + _size/2;

						point3 = new Point3D(axisPosition, point1.Y, wallZPosition);
						point4 = new Point3D(axisPosition, point1.Y, - _size/2 - area.areaSceneWallWidth.Width);
					}

					// Do not show "bent" tick marks on the left surface
					if(area.ShouldDrawOnSurface(SurfaceNames.Left, backElements, false))
					{
						point3 = null;
						point4 = null;
					}
				}

				//*****************************************************************
				//** Adjust grid line direction for the Right axis
				//*****************************************************************
                else if (Axis.AxisPosition == AxisPosition.Right && area.IsSideSceneWallOnLeft())
				{
					// Always use plot area position to draw tick mark
                    float axisPosition = Axis.PlotAreaPosition.Right;

					if( _style == TickMarkStyle.InsideArea )
					{
						point1.X = axisPosition - _size;
						point2.X = axisPosition;

						point3 = new Point3D(point2.X, point2.Y, - area.areaSceneWallWidth.Width);
						point4 = new Point3D(point2.X, point2.Y, 0f);
					}
					else if( _style == TickMarkStyle.OutsideArea )
					{
						point1.X = axisPosition;
						point2.X = axisPosition;

						point3 = new Point3D(axisPosition, point1.Y, wallZPosition);
						point4 = new Point3D(axisPosition, point1.Y, - _size - area.areaSceneWallWidth.Width);

					}
					else if( _style == TickMarkStyle.AcrossAxis )
					{
						point1.X = axisPosition - _size/2;
						point2.X = axisPosition;

						point3 = new Point3D(axisPosition, point1.Y, wallZPosition);
						point4 = new Point3D(axisPosition, point1.Y, - _size/2 - area.areaSceneWallWidth.Width);
					}

					// Do not show "bent" tick marks on the right surface
					if(area.ShouldDrawOnSurface(SurfaceNames.Right, backElements, false))
					{
						point3 = null;
						point4 = null;
					}
				}
			}

			//*****************************************************************
			//** Draw tick mark (first line)
			//*****************************************************************
			graph.Draw3DLine( 
				area.matrix3D,
				borderColor, borderWidth, borderDashStyle,
				new Point3D(point1.X, point1.Y, wallZPosition),
				new Point3D(point2.X, point2.Y, wallZPosition),
                Axis.Common,
				this,
				ChartElementType.TickMarks
				);

			
			//*****************************************************************
			//** Draw tick mark (second line)
			//*****************************************************************
			if(point3 != null && point4 != null)
			{
				graph.Draw3DLine( 
					area.matrix3D,
					borderColor, borderWidth, borderDashStyle,
					point3,
					point4,
                    Axis.Common,
					this,
					ChartElementType.TickMarks
					);
			}
		}

		#endregion

		#region	TickMark properties

		/// <summary>
		/// Tick mark style.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(TickMarkStyle.OutsideArea),
		SRDescription("DescriptionAttributeTickMark_Style"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
        public TickMarkStyle TickMarkStyle
		{
			get
			{
				return _style;
			}
			set
			{
				_style = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Tick mark size.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(1.0F),
		SRDescription("DescriptionAttributeTickMark_Size"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public float Size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
				this.Invalidate();
			}
		}

		#endregion
	}

	/// <summary>
    /// The Grid class represents axis grid lines which are drawn in the 
    /// plotting area. It contains grid interval and visual appearance 
    /// properties. This class also contains methods for grid lines drawing. 
	/// </summary>
	[
		DefaultProperty("Enabled"),
		SRDescription("DescriptionAttributeGrid_Grid"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class Grid
	{
		#region Grid fields and Constructors

		// Reference to the Axis object
		private Axis							_axis = null;

		// Flags indicate that interval properties where changed
		internal bool							intervalOffsetChanged = false;
		internal bool							intervalChanged = false;
		internal bool							intervalTypeChanged = false;
		internal bool							intervalOffsetTypeChanged = false;

		internal bool							enabledChanged = false;

		// Data members, which store properties values
		internal double							intervalOffset = 0;
		internal double							interval = 0;
		internal DateTimeIntervalType			intervalType = DateTimeIntervalType.Auto;
		internal DateTimeIntervalType			intervalOffsetType = DateTimeIntervalType.Auto;
		internal Color							borderColor = Color.Black;
		internal int							borderWidth = 1;
		internal ChartDashStyle					borderDashStyle = ChartDashStyle.Solid;
		internal bool							enabled = true;

		// Indicates that object describes Major Tick Mark or Grid Line
		internal bool							majorGridTick = false;

        // Common number of intervals on the numeric and date-time axis
        internal const double NumberOfIntervals = 5.0;
        internal const double NumberOfDateTimeIntervals = 4.0;

		/// <summary>
		/// Public default constructor.
		/// </summary>
		public Grid()
		{
		}

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="axis">Axis which owns the grid.</param>
        /// <param name="major">Major axis element.</param>
		internal Grid(Axis axis, bool major)
		{
			Initialize(axis, major);
		}

        /// <summary>
        /// Initializes the object.
        /// </summary>
        /// <param name="axis">Axis which owns the grid.</param>
        /// <param name="major">Major axis element.</param>
		internal void Initialize(Axis axis, bool major)
		{
			// Minor elements are disabled by default
			if(!this.enabledChanged &&
				this._axis == null && 
				!major)
			{
				enabled = false;
			}

			// If object was first created and populated with data and then added into the axis 
			// we need to remember changed values.
			// NOTE: Fixes issue #6237
			if(this._axis == null)
			{
                TickMark tickMark = this as TickMark;

				if(this.interval != 0)
				{
                    if (tickMark != null)
					{
						if(major)
						{
							axis.tempMajorTickMarkInterval = this.interval;
						}
						else
						{
							axis.tempMinorTickMarkInterval = this.interval;
						}
					}
					else
					{
						if(major)
						{
							axis.tempMajorGridInterval = this.interval;
						}
						else
						{
							axis.tempMinorGridInterval = this.interval;
						}
					}
				}
				if(this.intervalType != DateTimeIntervalType.Auto)
				{
					if(tickMark != null)
					{
						if(major)
						{
							axis.tempTickMarkIntervalType = this.intervalType;
						}
					}
					else
					{
						if(major)
						{
							axis.tempGridIntervalType = this.intervalType;
						}
					}
				}
			}

			// Set axis object reference
			this._axis = axis;

			// Set a flag if this object represent minor or major tick
			this.majorGridTick = major;

//		internal double							interval = 0;
//		internal DateTimeIntervalType			intervalType = DateTimeIntervalType.Auto;

		}

		#endregion

		#region Grid helper functions
		/// <summary>
		/// Gets axes to which this object attached to
		/// </summary>
		/// <returns>Axis object.</returns>
		internal Axis GetAxis()
		{
			return _axis;
		}
        /// <summary>
		/// Invalidate chart area the axis belong to.
		/// </summary>
		internal void Invalidate()
		{
#if Microsoft_CONTROL

			if(this._axis != null)
			{
				this._axis.Invalidate();
			}
#endif
		}

		#endregion

		#region Grid lines drawing functions

		/// <summary>
		/// Draws grid lines.
		/// </summary>
		/// <param name="graph">Reference to the Chart Graphics object.</param>
		internal void Paint( ChartGraphics graph )
		{
			// Grids are disabled
			if( !this.enabled )
			{
				return;
			}

			// Check if custom grid lines should be drawn from custom labels
			if(_axis.IsCustomGridLines())
			{
				PaintCustom( graph );
				return;
			}
		
			double gridInterval; // Grid interval
			double current; // Current position
			
			// Get first series attached to this axis
			Series	axisSeries = null;
			if(_axis.axisType == AxisName.X || _axis.axisType == AxisName.X2)
			{
				List<string> seriesArray = _axis.ChartArea.GetXAxesSeries((_axis.axisType == AxisName.X) ? AxisType.Primary : AxisType.Secondary, _axis.SubAxisName);
				if(seriesArray.Count > 0)
				{
					axisSeries = _axis.Common.DataManager.Series[seriesArray[0]];
					if(axisSeries != null && !axisSeries.IsXValueIndexed)
					{
						axisSeries = null;
					}
				}
			}

			// ****************************************************************
			// This code creates auto interval for auto tick marks and 
			// gridlines. If type is not date there are always four tickmarks 
			// or gridlines between major gridlines and tickmarks. For date 
			// type interval is calculated using CalcInterval function.
			// ****************************************************************
			double oldInterval = this.interval;
			DateTimeIntervalType oldIntervalType = this.intervalType;
			double oldIntervalOffset = this.intervalOffset;
			DateTimeIntervalType oldIntervalOffsetType = this.intervalOffsetType;
			if( !this.majorGridTick && ( this.interval == 0 || double.IsNaN(this.interval) ) )
			{
				// Number type
				if( this._axis.majorGrid.GetIntervalType() == DateTimeIntervalType.Auto )
				{
                    this.interval = this._axis.majorGrid.GetInterval() / Grid.NumberOfIntervals;
				}
					// Date type
				else
				{
					DateTimeIntervalType localIntervalType = this._axis.majorGrid.GetIntervalType();
                    this.interval = _axis.CalcInterval(
                        this._axis.minimum, 
                        this._axis.minimum + (this._axis.maximum - this._axis.minimum) / Grid.NumberOfDateTimeIntervals, 
                        true, 
                        out localIntervalType, 
                        ChartValueType.DateTime);
					this.intervalType = localIntervalType;
					this.intervalOffsetType = this._axis.majorGrid.GetIntervalOffsetType();
					this.intervalOffset = this._axis.majorGrid.GetIntervalOffset();
				}
			}

			// Current position for grid lines is minimum
			current = _axis.ViewMinimum;


            // ***********************************
            // Check if the AJAX zooming and scrolling mode is enabled.
            // ***********************************

			// Adjust start position depending on the interval type
			if(!_axis.ChartArea.chartAreaIsCurcular ||
				_axis.axisType == AxisName.Y || 
				_axis.axisType == AxisName.Y2 )
			{
                current = ChartHelper.AlignIntervalStart(current, this.GetInterval(), this.GetIntervalType(), axisSeries, this.majorGridTick);
			}

			// The Current position is start position, not minimum
			DateTimeIntervalType offsetType = (GetIntervalOffsetType() == DateTimeIntervalType.Auto) ? GetIntervalType() : GetIntervalOffsetType();
            if (GetIntervalOffset() != 0 && !double.IsNaN(GetIntervalOffset()) && axisSeries == null)
			{
                current += ChartHelper.GetIntervalSize(current, GetIntervalOffset(), offsetType, axisSeries, 0, DateTimeIntervalType.Number, true, false);
			}

			// Too many gridlines
			if( ( _axis.ViewMaximum - _axis.ViewMinimum ) / ChartHelper.GetIntervalSize( current, this.GetInterval(), this.GetIntervalType(), axisSeries, 0, DateTimeIntervalType.Number, true ) > ChartHelper.MaxNumOfGridlines )
				return;

			// If Maximum, minimum and interval don’t have 
			// proper value do not draw grid lines.
			if( _axis.ViewMaximum <= _axis.ViewMinimum )
				return;

			if( this.GetInterval() <= 0 )
				return;

			// Loop for drawing grid lines
			int	counter = 0;
			int logStep = 1;
			double oldCurrent = current;
			decimal viewMaximum = (decimal)_axis.ViewMaximum;
			while( (decimal)current <= viewMaximum )
			{
				// Take an interval between gridlines. Interval 
				// depends on interval type.
				if( this.majorGridTick || this._axis.IsLogarithmic == false )
				{
					double autoInterval = this.GetInterval();

                    gridInterval = ChartHelper.GetIntervalSize(current, autoInterval, this.GetIntervalType(), axisSeries, this.GetIntervalOffset(), offsetType, true);

					// Check interval size
                    if (gridInterval == 0)
                    {
                        throw (new InvalidOperationException(SR.ExceptionTickMarksIntervalIsZero));
                    }

					// Draw between min & max values only
					if((decimal)current >= (decimal)_axis.ViewMinimum)
					{
						DrawGrid( graph, current );
					}

					// Move position
					current += gridInterval;
				}
				// Code for linear minor gridlines and tickmarks 
				// if scale is logarithmic.
				else
				{
					// This code is used only for logarithmic scale and minor tick marks or 
					// gridlines which have linear minor scale in logarithmic major scale. 
					// This code is used to find minimum value for the interval. For example 
					// if logarithmic base is 2 and interval is between 4 and 8; current value 
					// is 5.6; this method will return linearised value for 4. This code works 
					// like Math.Floor for logarithmic scale.
					double logMinimum = this.GetLogMinimum( current, axisSeries );

					if( oldCurrent != logMinimum )
					{
						oldCurrent = logMinimum;
						logStep = 1;
					}
					
					// Find interval for logarithmic linearised scale
					double logInterval = Math.Log( 1 + this.interval * logStep, _axis.logarithmBase );

					current = oldCurrent;

					// Move position
					current += logInterval;

					logStep++;

					// Reset current position if major interval is passed.
					if( this.GetLogMinimum( current, axisSeries ) != logMinimum )
					{
						continue;
					}

					// Check interval size
                    if (logInterval == 0)
                    {
                        throw (new InvalidOperationException(SR.ExceptionTickMarksIntervalIsZero));
                    }

					// Draw between min & max values only
					if( (decimal)current >= (decimal)_axis.ViewMinimum && (decimal)current <= (decimal)_axis.ViewMaximum )
					{
						DrawGrid( graph, current );
					}
                }

                // Check if we do not exceed max number of elements
                if (counter++ > ChartHelper.MaxNumOfGridlines)
                {
                    break;
                }
			}

			// Used for auto interval for auto tick marks and 
			// gridlines
			if( !this.majorGridTick )
			{
				this.interval = oldInterval;
				this.intervalType = oldIntervalType;
				this.intervalOffset = oldIntervalOffset;
				this.intervalOffsetType = oldIntervalOffsetType;
			}
		}

		/// <summary>
		/// This method returns linearized logarithmic value 
		/// which is minimum for range with interval 1.
		/// </summary>
		/// <param name="current">Current value</param>
		/// <param name="axisSeries">First series attached to axis.</param>
		/// <returns>Returns Minimum for the range which contains current value</returns>
		private double GetLogMinimum( double current, Series axisSeries )
		{
			double viewMinimum = _axis.ViewMinimum;
			DateTimeIntervalType offsetType = (GetIntervalOffsetType() == DateTimeIntervalType.Auto) ? GetIntervalType() : GetIntervalOffsetType();
			if( GetIntervalOffset() != 0 && axisSeries == null)
			{
                viewMinimum += ChartHelper.GetIntervalSize(viewMinimum, GetIntervalOffset(), 
					offsetType, axisSeries, 0, DateTimeIntervalType.Number, true, false);
			}

			return viewMinimum + Math.Floor( ( current - viewMinimum ));
		}

		/// <summary>
		/// Draw the grid line 
		/// </summary>
		/// <param name="graph">Chart Graphics object</param>
		/// <param name="current">Current position of the gridline</param>
		private void DrawGrid( ChartGraphics graph, double current )
		{
			// Common elements
			CommonElements common = this._axis.Common;

			PointF first = PointF.Empty; // The First point of a grid line
			PointF second = PointF.Empty; // The Second point of a grid line
			RectangleF plotArea; // Plot area position

			plotArea = _axis.PlotAreaPosition.ToRectangleF();

			// Horizontal gridlines
			if( _axis.AxisPosition == AxisPosition.Left || _axis.AxisPosition == AxisPosition.Right )
			{
				first.X = plotArea.X;
				second.X = plotArea.Right;
				first.Y = (float)_axis.GetLinearPosition( current );
				second.Y = first.Y;
			}

			// Vertical gridlines
			if( _axis.AxisPosition == AxisPosition.Top || _axis.AxisPosition == AxisPosition.Bottom )
			{
				first.Y = plotArea.Y;
				second.Y = plotArea.Bottom;
				first.X = (float)_axis.GetLinearPosition( current );
				second.X = first.X;
			}

			if( common.ProcessModeRegions )
			{
				if( this._axis.ChartArea.Area3DStyle.Enable3D && !this._axis.ChartArea.chartAreaIsCurcular)
				{
                    if(!common.ProcessModePaint) //if ProcessModePaint is true it will be called later
				    	graph.Draw3DGridLine(this._axis.ChartArea, borderColor, borderWidth, borderDashStyle, first, second, ( _axis.AxisPosition == AxisPosition.Left || _axis.AxisPosition == AxisPosition.Right ), common, this );
				}
				else if(!this._axis.ChartArea.chartAreaIsCurcular)
				{
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        if (Math.Abs(first.X - second.X) > Math.Abs(first.Y - second.Y))
                        {
                            path.AddLine(first.X, first.Y - 1, second.X, second.Y - 1);
                            path.AddLine(second.X, second.Y + 1, first.X, first.Y + 1);
                            path.CloseAllFigures();
                        }
                        else
                        {
                            path.AddLine(first.X - 1, first.Y, second.X - 1, second.Y);
                            path.AddLine(second.X + 1, second.Y, first.X + 1, first.Y);
                            path.CloseAllFigures();

                        }
                        common.HotRegionsList.AddHotRegion(path, true, ChartElementType.Gridlines, this);
                    }
				}
			}

			if( common.ProcessModePaint )
			{
				// Check if grid lines should be drawn for circular chart area
				if(_axis.ChartArea.chartAreaIsCurcular)
				{
					if(_axis.axisType == AxisName.Y)
					{
						_axis.DrawCircularLine( this, graph, borderColor, borderWidth, borderDashStyle, first.Y );
					}
					if(_axis.axisType == AxisName.X)
					{
						ICircularChartType chartType = this._axis.ChartArea.GetCircularChartType();
						if(chartType != null && chartType.RadialGridLinesSupported())
						{
							_axis.DrawRadialLine( this, graph, borderColor, borderWidth, borderDashStyle, current );
						}
					}
				}
				else if(!this._axis.ChartArea.Area3DStyle.Enable3D || this._axis.ChartArea.chartAreaIsCurcular)
				{
					graph.DrawLineRel( borderColor, borderWidth, borderDashStyle, first, second );
				}
				else
				{
					graph.Draw3DGridLine( this._axis.ChartArea, borderColor, borderWidth, borderDashStyle, first, second, ( _axis.AxisPosition == AxisPosition.Left || _axis.AxisPosition == AxisPosition.Right ), _axis.Common, this );
				}
			}
		}
		
		/// <summary>
		/// Draws custom grid lines from custom labels.
		/// </summary>
		/// <param name="graph">Reference to the Chart Graphics object.</param>
		internal void PaintCustom( ChartGraphics graph )
		{
			// Common Elements
			CommonElements common = this._axis.Common;
			
			PointF first = PointF.Empty; // The First point of a grid line
			PointF second = PointF.Empty; // The Second point of a grid line
			RectangleF plotArea = _axis.PlotAreaPosition.ToRectangleF(); // Plot area position


			// Loop through all custom labels
			foreach(CustomLabel label in _axis.CustomLabels)
			{
				if((label.GridTicks & GridTickTypes.Gridline) == GridTickTypes.Gridline)
				{
					double	position = (label.ToPosition + label.FromPosition) / 2.0;
					if(position >= _axis.ViewMinimum && position <= _axis.ViewMaximum)
					{
						// Horizontal gridlines
						if( _axis.AxisPosition == AxisPosition.Left || _axis.AxisPosition == AxisPosition.Right )
						{
							first.X = plotArea.X;
							second.X = plotArea.Right;
							first.Y = (float)_axis.GetLinearPosition( position );
							second.Y = first.Y;

						}

						// Vertical gridlines
						if( _axis.AxisPosition == AxisPosition.Top || _axis.AxisPosition == AxisPosition.Bottom )
						{
							first.Y = plotArea.Y;
							second.Y = plotArea.Bottom;
							first.X = (float)_axis.GetLinearPosition( position );
							second.X = first.X;
						}

						if( common.ProcessModeRegions )
						{
							if( !this._axis.ChartArea.Area3DStyle.Enable3D || this._axis.ChartArea.chartAreaIsCurcular )
							{
                                using (GraphicsPath path = new GraphicsPath())
                                {

                                    if (Math.Abs(first.X - second.X) > Math.Abs(first.Y - second.Y))
                                    {
                                        path.AddLine(first.X, first.Y - 1, second.X, second.Y - 1);
                                        path.AddLine(second.X, second.Y + 1, first.X, first.Y + 1);
                                        path.CloseAllFigures();
                                    }
                                    else
                                    {
                                        path.AddLine(first.X - 1, first.Y, second.X - 1, second.Y);
                                        path.AddLine(second.X + 1, second.Y, first.X + 1, first.Y);
                                        path.CloseAllFigures();

                                    }
                                    common.HotRegionsList.AddHotRegion(path, true, ChartElementType.Gridlines, this);
                                }
							}
							else
							{
								graph.Draw3DGridLine(this._axis.ChartArea, borderColor, borderWidth, borderDashStyle, first, second, ( _axis.AxisPosition == AxisPosition.Left || _axis.AxisPosition == AxisPosition.Right ), common, this );
							}
						}
						
						if( common.ProcessModePaint )
						{
							if(!this._axis.ChartArea.Area3DStyle.Enable3D || this._axis.ChartArea.chartAreaIsCurcular)
							{
								graph.DrawLineRel( borderColor, borderWidth, borderDashStyle, first, second );
							}
							else
							{
								graph.Draw3DGridLine(this._axis.ChartArea, borderColor, borderWidth, borderDashStyle, first, second, ( _axis.AxisPosition == AxisPosition.Left || _axis.AxisPosition == AxisPosition.Right ), _axis.Common, this );
							}
						}
					}
				}
			}
		}

		#endregion

		#region	Grid properties

		/// <summary>
		/// Gets or sets grid or tick mark interval offset.
		/// </summary>
        [
        SRCategory("CategoryAttributeData"),
        Bindable(true),
        SRDescription("DescriptionAttributeIntervalOffset3"),
        TypeConverter(typeof(AxisElementIntervalValueConverter)),
        #if !Microsoft_CONTROL
         PersistenceMode(PersistenceMode.Attribute),
        #endif
        ]
        public double IntervalOffset
        {
            get
            {
                return intervalOffset;
            }
            set
            {
                intervalOffset = value;
                intervalOffsetChanged = true;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Determines if Enabled property should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeIntervalOffset()
        {
            if (this.majorGridTick)
            {
                return !double.IsNaN(intervalOffset);
            }
            return intervalOffset != 0d;
        }


        /// <summary>
        /// Gets the interval offset.
        /// </summary>
        /// <returns></returns>
		internal double GetIntervalOffset()
		{
			if(this.majorGridTick && double.IsNaN(intervalOffset) && this._axis != null)
			{

				return this._axis.IntervalOffset;
			}
			return intervalOffset;
		}


		/// <summary>
        /// Gets or sets the unit of measurement of grid or tick mark offset.
		/// </summary>
        [
        SRCategory("CategoryAttributeData"),
        Bindable(true),
        SRDescription("DescriptionAttributeIntervalOffsetType6"),
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
                intervalOffsetType = value;
                intervalOffsetTypeChanged = true;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Determines if IntervalOffsetType property should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeIntervalOffsetType()
        {
            if (this.majorGridTick)
            {
                return intervalOffsetType != DateTimeIntervalType.NotSet;
            }
            return intervalOffsetType != DateTimeIntervalType.Auto;
        }

        /// <summary>
        /// Gets the type of the interval offset.
        /// </summary>
        /// <returns></returns>
		internal DateTimeIntervalType GetIntervalOffsetType()
		{
			if(this.majorGridTick && intervalOffsetType == DateTimeIntervalType.NotSet && this._axis != null)
			{
				return this._axis.IntervalOffsetType;
			}
			return intervalOffsetType;
		}

		/// <summary>
        /// Gets or sets grid or tick mark interval size.
		/// </summary>
        [
        SRCategory("CategoryAttributeData"),
        Bindable(true),
        SRDescription("DescriptionAttributeInterval6"),
        TypeConverter(typeof(AxisElementIntervalValueConverter)),
        RefreshProperties(RefreshProperties.All),
        #if !Microsoft_CONTROL
         PersistenceMode(PersistenceMode.Attribute)
        #endif
        ]
        public double Interval
        {
            get
            {
                return interval;
            }
            set
            {
                // Validation
                if (value < 0.0)
                    throw (new ArgumentException(SR.ExceptionTickMarksIntervalIsNegative, "value"));

                interval = value;
                intervalChanged = true;

                // Enable minor elements
                if (!this.majorGridTick && value != 0.0 && !Double.IsNaN(value))
                {
                    // Prevent grids enabling during the serialization
                    if (this._axis != null)
                    {
                        if (this._axis.Chart != null && this._axis.Chart.serializing != false)
                        {
                            this.Enabled = true;
                        }
                    }
                }

                // Reset original property value fields
                if (this._axis != null)
                {
                    if (this is TickMark)
                    {
                        if (this.majorGridTick)
                        {
                            this._axis.tempMajorTickMarkInterval = interval;
                        }
                        else
                        {
                            this._axis.tempMinorTickMarkInterval = interval;
                        }
                    }
                    else
                    {
                        if (this.majorGridTick)
                        {
                            this._axis.tempMajorGridInterval = interval;
                        }
                        else
                        {
                            this._axis.tempMinorGridInterval = interval;
                        }
                    }
                }

                this.Invalidate();
            }

        }

        /// <summary>
        /// Determines if IntervalOffsetType property should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeInterval()
        {
            if (this.majorGridTick)
            {
                return !double.IsNaN(interval);
            }
            return interval != 0d;
        }

        /// <summary>
        /// Gets the interval.
        /// </summary>
        /// <returns></returns>
		internal double GetInterval()
		{
			if(this.majorGridTick && double.IsNaN(interval) && this._axis != null)
			{
				return this._axis.Interval;
			}
			return interval;
		}

		/// <summary>
        /// Gets or sets the unit of measurement of grid or tick mark interval.
		/// </summary>
        [
        SRCategory("CategoryAttributeData"),
        Bindable(true),
        SRDescription("DescriptionAttributeIntervalType3"),
        #if !Microsoft_CONTROL
         PersistenceMode(PersistenceMode.Attribute),
        #endif
         RefreshPropertiesAttribute(RefreshProperties.All)
        ]
        public DateTimeIntervalType IntervalType
        {
            get
            {
                return intervalType;
            }
            set
            {
                intervalType = value;
                intervalTypeChanged = true;

                // Reset original property value fields
                if (this._axis != null)
                {
                    if (this is TickMark)
                    {
                        this._axis.tempTickMarkIntervalType = intervalType;
                    }
                    else
                    {
                        this._axis.tempGridIntervalType = intervalType;
                    }
                }

                this.Invalidate();
            }
        }

        /// <summary>
        /// Determines if IntervalOffsetType property should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeIntervalType()
        {
            if (this.majorGridTick)
            {
                return intervalType != DateTimeIntervalType.NotSet;
            }
            return intervalType != DateTimeIntervalType.Auto;
        }


        /// <summary>
        /// Gets the type of the interval.
        /// </summary>
        /// <returns></returns>
		internal DateTimeIntervalType GetIntervalType()
		{
			if(this.majorGridTick && intervalType == DateTimeIntervalType.NotSet && this._axis != null)
			{
				// Return default value during serialization
				return this._axis.IntervalType;
			}
			return intervalType;
		}

		/// <summary>
        /// Gets or sets grid or tick mark line color.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), "Black"),
        SRDescription("DescriptionAttributeLineColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public Color LineColor
		{
			get
			{
				return borderColor;
			}
			set
			{
				borderColor = value;
				this.Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets grid or tick mark line style.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartDashStyle.Solid),
        SRDescription("DescriptionAttributeLineDashStyle"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public ChartDashStyle LineDashStyle
		{
			get
			{
				return borderDashStyle;
			}
			set
			{
				borderDashStyle = value;
				this.Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets grid or tick mark line width.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(1),
        SRDescription("DescriptionAttributeLineWidth"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public int LineWidth
		{
			get
			{
				return borderWidth;
			}
			set
			{
				borderWidth = value;
				this.Invalidate();
			}
		}
		
		/// <summary>
        /// Gets or sets a flag which indicates if the grid or tick mark is enabled.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		SRDescription("DescriptionAttributeEnabled5"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public bool Enabled
		{
			get
			{
                //// Never serialize this property for minor elements
                //// "Disabled" property should be serialized instead.
                //if(this.axis != null && this.axis.IsSerializing())
                //{
                //    if(!this.majorGridTick)
                //    {
                //        // Return default value to prevent serialization
                //        return true;
                //    }
                //}

				return enabled;
			}
			set
			{
				enabled = value;
				enabledChanged = true;
				this.Invalidate();
			}
		}

        /// <summary>
        /// Determines if Enabled property should be serialized.
        /// </summary>
        /// <returns></returns>
        internal bool ShouldSerializeEnabled()
        {
            if (this.majorGridTick)
            {
                return !this.Enabled;
            }
            return this.Enabled;
        }

        /// <summary>
        /// Gets or sets the reference to the Axis object
        /// </summary>
        internal Axis Axis
        {
            get { return _axis; }
            set { _axis = value; }
        }

		#endregion
	}
}
