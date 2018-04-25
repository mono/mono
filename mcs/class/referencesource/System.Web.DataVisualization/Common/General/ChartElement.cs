//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartElement.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ChartHelper
//
//  Purpose:	The chart element is base class for the big number 
//				of classes. It stores common methods and data.
//
//	Reviewed:	GS - August 2, 2002
//				AG - August 8, 2002
//              AG - Microsoft 16, 2007
//
//===================================================================


#region Used namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	#region Enumerations

		/// <summary>
		/// An enumeration that specifies a label alignment.
		/// </summary>
		[
		Flags
		]
		public enum LabelAlignmentStyles
		{
			/// <summary>
		        /// Label is aligned to the top of the data point.
			/// </summary>
			Top = 1,
			/// <summary>
                        /// Label is aligned to the bottom of the data point.
			/// </summary>
			Bottom = 2,
			/// <summary>
                        /// Label is aligned to the right of the data point.
			/// </summary>
			Right = 4,
			/// <summary>
                        /// Label is aligned to the left of the data point.
			/// </summary>
			Left = 8,
			/// <summary>
                        /// Label is aligned to the top-left corner of the data point.
			/// </summary>
			TopLeft = 16,
			/// <summary>
                        /// Label is aligned to the top-right corner of the data point.
			/// </summary>
			TopRight = 32,
			/// <summary>
                        /// Label is aligned to the bottom-left of the data point.
			/// </summary>
			BottomLeft = 64,
			/// <summary>
                        /// Label is aligned to the bottom-right of the data point.
			/// </summary>
			BottomRight = 128,
			/// <summary>
                        /// Label is aligned to the center of the data point.
			/// </summary>
			Center = 256,
		}

	/// <summary>
	/// An enumeration of chart types.
	/// </summary>
	public enum SeriesChartType
	{	
		/// <summary>
		/// Point chart type.
		/// </summary>
		Point,

		/// <summary>
		/// FastPoint chart type.
		/// </summary>
		FastPoint,

		/// <summary>
		/// Bubble chart type.
		/// </summary>
		Bubble,
		/// <summary>
		/// Line chart type.
		/// </summary>
		Line,
		/// <summary>
		/// Spline chart type.
		/// </summary>
		Spline,
		/// <summary>
		/// StepLine chart type.
		/// </summary>
		StepLine,

		/// <summary>
		/// FastLine chart type.
		/// </summary>
		FastLine,

		/// <summary>
		/// Bar chart type.
		/// </summary>
		Bar,
		/// <summary>
		/// Stacked bar chart type.
		/// </summary>
		StackedBar,
		/// <summary>
		/// Hundred percent stacked bar chart type.
		/// </summary>
		StackedBar100,
		/// <summary>
		/// Column chart type.
		/// </summary>
		Column,
		/// <summary>
		/// Stacked column chart type.
		/// </summary>
		StackedColumn,
		/// <summary>
		/// Hundred percent stacked column chart type.
		/// </summary>
		StackedColumn100,
		/// <summary>
		/// Area chart type.
		/// </summary>
		Area,
		/// <summary>
		/// Spline area chart type.
		/// </summary>
		SplineArea,
		/// <summary>
		/// Stacked area chart type.
		/// </summary>
		StackedArea,
		/// <summary>
		/// Hundred percent stacked area chart type.
		/// </summary>
		StackedArea100,
		/// <summary>
		/// Pie chart type.
		/// </summary>
		Pie,
		/// <summary>
		/// Doughnut chart type.
		/// </summary>
		Doughnut,
		/// <summary>
		/// Stock chart type.
		/// </summary>
		Stock,
		/// <summary>
		/// CandleStick chart type.
		/// </summary>
		Candlestick,
		/// <summary>
		/// Range chart type.
		/// </summary>
		Range,
		/// <summary>
		/// Spline range chart type.
		/// </summary>
		SplineRange,
		/// <summary>
		/// RangeBar chart type.
		/// </summary>
		RangeBar,
		/// <summary>
		/// Range column chart type.
		/// </summary>
		RangeColumn,
		/// <summary>
		/// Radar chart type.
		/// </summary>
		Radar,
		/// <summary>
		/// Polar chart type.
		/// </summary>
		Polar,
		/// <summary>
		/// Error bar chart type.
		/// </summary>
		ErrorBar,
		/// <summary>
		/// Box plot chart type.
		/// </summary>
		BoxPlot,
		/// <summary>
		/// Renko chart type.
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Renko")]
        Renko,
		/// <summary>
		/// ThreeLineBreak chart type.
		/// </summary>
		ThreeLineBreak,
		/// <summary>
		/// Kagi chart type.
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Kagi")]
        Kagi,
		/// <summary>
		/// PointAndFigure chart type.
		/// </summary>
		PointAndFigure,
		/// <summary>
		/// Funnel chart type.
		/// </summary>
		Funnel,
		/// <summary>
		/// Pyramid chart type.
		/// </summary>
		Pyramid,
	}

	/// <summary>
	/// Axis Arrow orientation
	/// </summary>
	internal enum ArrowOrientation
	{
		/// <summary>
		/// Arrow direction is Right - Left
		/// </summary>
		Left, 
		/// <summary>
		/// Arrow direction is Left - Right
		/// </summary>
		Right, 
		/// <summary>
		/// Arrow direction is Bottom - Top
		/// </summary>
		Top,
		/// <summary>
		/// Arrow direction is Top - Bottom
		/// </summary>
		Bottom
	}

	/// <summary>
	/// An enumeration of image alignment.
	/// </summary>
	public enum ChartImageAlignmentStyle
	{
		/// <summary>
        /// The mage is aligned to the top left corner of the chart element.
		/// </summary>
		TopLeft,
		/// <summary>
        /// The image is aligned to the top boundary of the chart element.
		/// </summary>
		Top,
		/// <summary>
        /// The image is aligned to the top right corner of the chart element.
		/// </summary>
		TopRight,
		/// <summary>
        /// The image is aligned to the right boundary of the chart element.
		/// </summary>
		Right,
		/// <summary>
        /// The image is aligned to the bottom right corner of the chart element.
		/// </summary>
		BottomRight,
		/// <summary>
        /// The image is aligned to the bottom boundary of the chart element.
		/// </summary>
		Bottom,
		/// <summary>
        /// The image is aligned to the bottom left corner of the chart element.
		/// </summary>
		BottomLeft,
		/// <summary>
        /// The image is aligned to the left boundary of the chart element.
		/// </summary>
		Left,
		/// <summary>
        /// The image is aligned in the center of the chart element.
		/// </summary>
		Center
	};

	/// <summary>
    /// An enumeration that specifies a background image drawing mode.
	/// </summary>
	public enum ChartImageWrapMode
	{
		/// <summary>
        /// Background image is scaled to fit the entire chart element.
		/// </summary>		
		Scaled = WrapMode.Clamp,

		/// <summary>
        /// Background image is tiled to fit the entire chart element.
		/// </summary>
		Tile = WrapMode.Tile,

		/// <summary>
        /// Every other tiled image is reversed around the X-axis.
		/// </summary>
		TileFlipX = WrapMode.TileFlipX,

		/// <summary>
        /// Every other tiled image is reversed around the X-axis and Y-axis.
		/// </summary>
		TileFlipXY = WrapMode.TileFlipXY,

		/// <summary>
        /// Every other tiled image is reversed around the Y-axis.
		/// </summary>
		TileFlipY = WrapMode.TileFlipY,

		/// <summary>
        /// Background image is not scaled.
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unscaled")]
        Unscaled = 100
	};

	/// <summary>
    /// An enumeration that specifies the state of an axis.
	/// </summary>
	public enum AxisEnabled
	{
		/// <summary>
        /// The axis is only enabled if it used to plot a Series.
		/// </summary>
		Auto,
		
		/// <summary>
		/// The axis is always enabled.
		/// </summary>
		True,
		
		/// <summary>
		/// The axis is never enabled.
		/// </summary>
		False

	};

	/// <summary>
	/// An enumeration of units of measurement of an interval.
	/// </summary>
	public enum DateTimeIntervalType
	{
		/// <summary>
        /// Automatically determined by the Chart control.
		/// </summary>
		Auto, 
		
		/// <summary>
		/// The interval is numerical.
		/// </summary>
		Number, 
		
		/// <summary>
		/// The interval is years.
		/// </summary>
		Years, 
		
		/// <summary>
		/// The interval is months.
		/// </summary>
		Months, 
		
		/// <summary>
        /// The interval is weeks.
		/// </summary>
		Weeks, 
		
		/// <summary>
		/// The interval is days.
		/// </summary>
		Days, 
		
		/// <summary>
		/// The interval is hours.
		/// </summary>
		Hours, 
		
		/// <summary>
		/// The interval is minutes.
		/// </summary>
		Minutes,

		/// <summary>
		/// The interval is seconds.
		/// </summary>
		Seconds,
		
		/// <summary>
		/// The interval is milliseconds.
		/// </summary>
		Milliseconds,

		/// <summary>
		/// The interval type is not defined.
		/// </summary>
		NotSet, 
	}

	/// <summary>
    /// An enumeration that specifies value types for various chart properties
	/// </summary>
	public enum ChartValueType
	{ 
		/// <summary>
        /// Property type is set automatically by the Chart control.
		/// </summary>
		Auto, 
		
		/// <summary>
		/// Double value.
		/// </summary>
		Double, 
		
		/// <summary>
		/// Single value.
		/// </summary>
		Single, 
		
		/// <summary>
		/// Int32 value.
		/// </summary>
		Int32, 
		
		/// <summary>
		/// Int64 value.
		/// </summary>
		Int64, 
		
		/// <summary>
		/// UInt32 value.
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "These names are patterned after the standard CLR types for consistency")]
		UInt32, 
		
		/// <summary>
		/// UInt64 value.
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "These names are patterned after the standard CLR types for consistency")]
		UInt64, 
		
		/// <summary>
		/// String value.
		/// </summary>
		String, 
		
		/// <summary>
		/// DateTime value.
		/// </summary>
		DateTime,

		/// <summary>
		/// Date portion of the DateTime value.
		/// </summary>
		Date,

		/// <summary>
		/// Time portion of the DateTime value.
		/// </summary>
		Time,

        /// <summary>
		/// DateTime with offset
		/// </summary>
		DateTimeOffset
	};

	/// <summary>
	/// An enumeration that specifies a hatching style.
	/// </summary>
	public enum ChartHatchStyle 
	{ 
		/// <summary>
		/// No hatching style.
		/// </summary>
		None, 
		/// <summary>
		/// Backward diagonal style.
		/// </summary>
		BackwardDiagonal, 
		/// <summary>
		/// Cross style.
		/// </summary>
		Cross, 
		/// <summary>
		/// Dark downward diagonal style.
		/// </summary>
		DarkDownwardDiagonal, 
		/// <summary>
		/// Dark horizontal style.
		/// </summary>
		DarkHorizontal, 
		/// <summary>
		/// Dark upward diagonal style.
		/// </summary>
		DarkUpwardDiagonal, 
		/// <summary>
		/// Dark vertical style.
		/// </summary>
		DarkVertical, 
		/// <summary>
		/// Dashed downward diagonal style.
		/// </summary>
		DashedDownwardDiagonal,
		/// <summary>
		/// Dashed horizontal style.
		/// </summary>
		DashedHorizontal, 
		/// <summary>
		/// Dashed upward diagonal style.
		/// </summary>
		DashedUpwardDiagonal, 
		/// <summary>
		/// Dashed vertical style.
		/// </summary>
		DashedVertical, 
		/// <summary>
		/// Diagonal brick style.
		/// </summary>
		DiagonalBrick, 
		/// <summary>
		/// Diagonal cross style.
		/// </summary>
		DiagonalCross, 
		/// <summary>
		/// Divot style.
		/// </summary>
		Divot, 
		/// <summary>
		/// Dotted diamond style.
		/// </summary>
		DottedDiamond, 
		/// <summary>
		/// Dotted grid style.
 		/// </summary>
		DottedGrid, 
		/// <summary>
		/// Forward diagonal style.
		/// </summary>
		ForwardDiagonal, 
		/// <summary>
		/// Horizontal style.
		/// </summary>
		Horizontal, 
		/// <summary>
		/// Horizontal brick style.
		/// </summary>
		HorizontalBrick, 
		/// <summary>
		/// Large checker board style.
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CheckerBoard")]
        LargeCheckerBoard, 
		/// <summary>
		/// Large confetti style.
		/// </summary>
		LargeConfetti, 
		/// <summary>
		/// Large grid style.
		/// </summary>
		LargeGrid, 
		/// <summary>
		/// Light downward diagonal style.
		/// </summary>
		LightDownwardDiagonal, 
		/// <summary>
		/// Light horizontal style.
		/// </summary>
		LightHorizontal, 
		/// <summary>
		/// Light upward diagonal style.
		/// </summary>
		LightUpwardDiagonal, 
		/// <summary>
		/// Light vertical style.
		/// </summary>
		LightVertical, 
		/// <summary>
		/// Narrow horizontal style.
		/// </summary>
		NarrowHorizontal, 
		/// <summary>
		/// Narrow vertical style.
		/// </summary>
		NarrowVertical, 
		/// <summary>
		/// Outlined diamond style.
		/// </summary>
		OutlinedDiamond, 
		/// <summary>
		/// Percent05 style.
		/// </summary>
		Percent05, 
		/// <summary>
		/// Percent10 style.
		/// </summary>
		Percent10, 
		/// <summary>
		/// Percent20 style.
		/// </summary>
		Percent20, 
		/// <summary>
		/// Percent25 style.
		/// </summary>
		Percent25, 
		/// <summary>
		/// Percent30 style.
		/// </summary>
		Percent30, 
		/// <summary>
		/// Percent40 style.
		/// </summary>
		Percent40, 
		/// <summary>
		/// Percent50 style.
		/// </summary>
		Percent50, 
		/// <summary>
		/// Percent60 style.
		/// </summary>
		Percent60, 
		/// <summary>
		/// Percent70 style.
		/// </summary>
		Percent70, 
		/// <summary>
		/// Percent75 style.
		/// </summary>
		Percent75, 
		/// <summary>
		/// Percent80 style.
		/// </summary>
		Percent80, 
		/// <summary>
		/// Percent90 style.
		/// </summary>
		Percent90, 
		/// <summary>
		/// Plaid style.
		/// </summary>
		Plaid, 
		/// <summary>
		/// Shingle style.
		/// </summary>
		Shingle, 
		/// <summary>
		/// Small checker board style.
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "CheckerBoard")]
        SmallCheckerBoard,
		/// <summary>
		/// Small confetti style.
		/// </summary>
		SmallConfetti, 
		/// <summary>
		/// Small grid style.
		/// </summary>
		SmallGrid, 
		/// <summary>
		/// Solid diamond style.
		/// </summary>
		SolidDiamond, 
		/// <summary>
		/// Sphere style.
		/// </summary>
		Sphere, 
		/// <summary>
		/// Trellis style.
		/// </summary>
		Trellis, 
		/// <summary>
		/// Vertical style.
		/// </summary>
		Vertical, 
		/// <summary>
		/// Wave style.
		/// </summary>
		Wave, 
		/// <summary>
		/// Weave style.
		/// </summary>
		Weave, 
		/// <summary>
		/// Wide downward diagonal style.
		/// </summary>
		WideDownwardDiagonal, 
		/// <summary>
		/// Wide upward diagonal style.
		/// </summary>
		WideUpwardDiagonal, 
		/// <summary>
		/// ZigZag style.
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ZigZag")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zig")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Zag")]
        ZigZag
	};

	/// <summary>
    /// An enumeration that specifies the level of anti-aliasing quality.
	/// </summary>
	public enum TextAntiAliasingQuality
	{
		/// <summary>
		/// Normal anti-aliasing quality.
		/// </summary>
		Normal,
		/// <summary>
		/// High anti-aliasing quality.
		/// </summary>
		High,
		/// <summary>
		/// System default anti-aliasing quality.
		/// </summary>
		SystemDefault
	}

	/// <summary>
	/// An enumeration of anti-aliasing flags.
	/// </summary>
	[Flags]
	public enum AntiAliasingStyles
	{
		/// <summary>
		/// No anti-aliasing.
		/// </summary>
		None = 0,

		/// <summary>
		/// Use anti-aliasing when drawing text.
		/// </summary>
		Text = 1,

		/// <summary>
		/// Use anti-aliasing when drawing grahics primitives (e.g. lines, rectangle)
		/// </summary>
		Graphics = 2,

		/// <summary>
		/// Use anti-alias for everything.
		/// </summary>
		All = Text | Graphics

	};
	
	/// <summary>
	/// An enumeration of marker styles.
	/// </summary>
	public enum MarkerStyle
	{
		/// <summary>
        /// No marker is displayed for the series/data point.
		/// </summary>
		None = 0, 

		/// <summary>
        /// A square marker is displayed.
		/// </summary>
		Square = 1, 

		/// <summary>
        /// A circle marker is displayed.
		/// </summary>
		Circle = 2, 

		/// <summary>
        /// A diamond-shaped marker is displayed.
		/// </summary>
		Diamond = 3, 

		/// <summary>
        /// A triangular marker is displayed.
		/// </summary>
		Triangle = 4, 

		/// <summary>
        /// A cross-shaped marker is displayed.
		/// </summary>
		Cross = 5,

		/// <summary>
        /// A 4-point star-shaped marker is displayed.
		/// </summary>
		Star4 = 6,

		/// <summary>
        /// A 5-point star-shaped marker is displayed.
		/// </summary>
		Star5 = 7,

		/// <summary>
        /// A 6-point star-shaped marker is displayed.
		/// </summary>
		Star6 = 8,

		/// <summary>
        /// A 10-point star-shaped marker is displayed.
		/// </summary>
		Star10 = 9

	};

	/// <summary>
	/// An enumeration of gradient styles.
	/// </summary>
	public enum GradientStyle
	{
		/// <summary>
        /// No gradient is used.
		/// </summary>
		None, 
		
		/// <summary>
        /// Gradient is applied from left to right.
		/// </summary>
		LeftRight, 
		
		/// <summary>
        /// Gradient is applied from top to bottom.
		/// </summary>
		TopBottom, 
		
		/// <summary>
        /// Gradient is applied from the center outwards.
		/// </summary>
		Center, 
		
		/// <summary>
        /// Gradient is applied diagonally from left to right.
		/// </summary>
		DiagonalLeft, 
		
		/// <summary>
        /// Gradient is applied diagonally from right to left.
		/// </summary>
		DiagonalRight, 
		
		/// <summary>
        /// Gradient is applied horizontally from the center outwards.
		/// </summary>
		HorizontalCenter, 
		
		/// <summary>
        /// Gradient is applied vertically from the center outwards.
		/// </summary>
		VerticalCenter
	};

	#endregion
    
    #region ChartElement

    /// <summary>
    /// Common chart helper methods used across different chart elements.
    /// </summary>
    internal class ChartHelper
    {
        #region Fields

        /// <summary>
        /// Maximum number of grid lines per Axis
        /// </summary>
        internal const int MaxNumOfGridlines = 10000;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Private constructor to avoid instantiating the class
        /// </summary>
        private ChartHelper() { }

        #endregion // Constructor

        #region Methods

        /// <summary>
		/// Adjust the beginnin of the first interval depending on the type and size.
		/// </summary>
		/// <param name="start">Original start point.</param>
		/// <param name="intervalSize">Interval size.</param>
		/// <param name="type">AxisName of the interval (Month, Year, ...).</param>
		/// <returns>Adjusted interval start position as double.</returns>
        internal static double AlignIntervalStart(double start, double intervalSize, DateTimeIntervalType type)
		{
			return AlignIntervalStart(start, intervalSize, type, null);
		}

		/// <summary>
		/// Adjust the beginnin of the first interval depending on the type and size.
		/// </summary>
		/// <param name="start">Original start point.</param>
		/// <param name="intervalSize">Interval size.</param>
		/// <param name="type">AxisName of the interval (Month, Year, ...).</param>
		/// <param name="series">First series connected to the axis.</param>
		/// <returns>Adjusted interval start position as double.</returns>
        internal static double AlignIntervalStart(double start, double intervalSize, DateTimeIntervalType type, Series series)
		{
			return AlignIntervalStart( start, intervalSize, type, series, true );
		}

		/// <summary>
		/// Adjust the beginnin of the first interval depending on the type and size.
		/// </summary>
		/// <param name="start">Original start point.</param>
		/// <param name="intervalSize">Interval size.</param>
		/// <param name="type">AxisName of the interval (Month, Year, ...).</param>
		/// <param name="series">First series connected to the axis.</param>
		/// <param name="majorInterval">Interval is used for major gridlines or tickmarks.</param>
		/// <returns>Adjusted interval start position as double.</returns>
        internal static double AlignIntervalStart(double start, double intervalSize, DateTimeIntervalType type, Series series, bool majorInterval)
		{
			// Special case for indexed series
			if(series != null && series.IsXValueIndexed)
			{
				if(type == DateTimeIntervalType.Auto ||
					type == DateTimeIntervalType.Number)
				{
					if( majorInterval )
					{
						return 1;
					}
					else
					{
						return 0;
					}
				}
					
				return -(series.Points.Count + 1);
			}

			// Non indexed series
			else
			{
				// Do not adjust start position for these interval type
				if(type == DateTimeIntervalType.Auto ||
					type == DateTimeIntervalType.Number)
				{
					return start;
				}

				// Get the beginning of the interval depending on type
				DateTime	newStartDate = DateTime.FromOADate(start);

				// Adjust the months interval depending on size
				if(intervalSize > 0.0 && intervalSize != 1.0)
				{
					if(type == DateTimeIntervalType.Months && intervalSize <= 12.0 && intervalSize > 1)
					{
						// Make sure that the beginning is aligned correctly for cases
						// like quarters and half years
						DateTime	resultDate = newStartDate;
						DateTime	sizeAdjustedDate = new DateTime(newStartDate.Year, 1, 1, 0, 0, 0);
						while(sizeAdjustedDate < newStartDate)
						{
							resultDate = sizeAdjustedDate;
							sizeAdjustedDate = sizeAdjustedDate.AddMonths((int)intervalSize);
						}

						newStartDate = resultDate;
						return newStartDate.ToOADate();
					}
				}

				// Check interval type
				switch(type)
				{
					case(DateTimeIntervalType.Years):
						int year = (int)((int)(newStartDate.Year / intervalSize) * intervalSize);
						if(year <= 0)
						{
							year = 1;
						}
						newStartDate = new DateTime(year, 
							1, 1, 0, 0, 0);
						break;

					case(DateTimeIntervalType.Months):
						int month = (int)((int)(newStartDate.Month / intervalSize) * intervalSize);
						if(month <= 0)
						{
							month = 1;
						}
						newStartDate = new DateTime(newStartDate.Year, 
							month, 1, 0, 0, 0);
						break;

					case(DateTimeIntervalType.Days):
						int day = (int)((int)(newStartDate.Day / intervalSize) * intervalSize);
						if(day <= 0)
						{
							day = 1;
						}
						newStartDate = new DateTime(newStartDate.Year, 
							newStartDate.Month, day, 0, 0, 0);
						break;

					case(DateTimeIntervalType.Hours):
						int hour = (int)((int)(newStartDate.Hour / intervalSize) * intervalSize);
						newStartDate = new DateTime(newStartDate.Year, 
							newStartDate.Month, newStartDate.Day, hour, 0, 0);
						break;

					case(DateTimeIntervalType.Minutes):
						int minute = (int)((int)(newStartDate.Minute / intervalSize) * intervalSize);
						newStartDate = new DateTime(newStartDate.Year, 
							newStartDate.Month, 
							newStartDate.Day, 
							newStartDate.Hour, 
							minute, 
							0);
						break;

					case(DateTimeIntervalType.Seconds):
						int second = (int)((int)(newStartDate.Second / intervalSize) * intervalSize);
						newStartDate = new DateTime(newStartDate.Year, 
							newStartDate.Month, 
							newStartDate.Day, 
							newStartDate.Hour, 
							newStartDate.Minute, 
							second, 
							0);
						break;

					case(DateTimeIntervalType.Milliseconds):
						int milliseconds = (int)((int)(newStartDate.Millisecond / intervalSize) * intervalSize);
						newStartDate = new DateTime(newStartDate.Year, 
							newStartDate.Month, 
							newStartDate.Day, 
							newStartDate.Hour, 
							newStartDate.Minute, 
							newStartDate.Second, 
							milliseconds);
						break;

					case(DateTimeIntervalType.Weeks):

                        // NOTE: Code below was changed to fix issue #5962
                        // Elements that have interval set to weeks should be aligned to the 
                        // nearest Monday no matter how many weeks is the interval.
						//newStartDate = newStartDate.AddDays(-((int)newStartDate.DayOfWeek * intervalSize));
                        newStartDate = newStartDate.AddDays(-((int)newStartDate.DayOfWeek));
						newStartDate = new DateTime(newStartDate.Year, 
							newStartDate.Month, newStartDate.Day, 0, 0, 0);
						break;
				}

				return newStartDate.ToOADate();
			}
		}


		/// <summary>
		/// Gets interval size as double number.
		/// </summary>
		/// <param name="current">Current value.</param>
		/// <param name="interval">Interval size.</param>
		/// <param name="type">AxisName of the interval (Month, Year, ...).</param>
		/// <returns>Interval size as double.</returns>
        internal static double GetIntervalSize(double current, double interval, DateTimeIntervalType type)
		{
			return GetIntervalSize(
				current, 
				interval, 
				type, 
				null, 
				0, 
				DateTimeIntervalType.Number, 
				true, 
				true);
		}

		/// <summary>
		/// Gets interval size as double number.
		/// </summary>
		/// <param name="current">Current value.</param>
		/// <param name="interval">Interval size.</param>
		/// <param name="type">AxisName of the interval (Month, Year, ...).</param>
		/// <param name="series">First series connected to the axis.</param>
		/// <param name="intervalOffset">Offset size.</param>
		/// <param name="intervalOffsetType">Offset type(Month, Year, ...).</param>
		/// <param name="forceIntIndex">Force Integer indexed</param>
		/// <returns>Interval size as double.</returns>
        internal static double GetIntervalSize(
			double current, 
			double interval, 
			DateTimeIntervalType type, 
			Series series,
			double intervalOffset, 
			DateTimeIntervalType intervalOffsetType,
			bool forceIntIndex)
		{
			return GetIntervalSize(
				current, 
				interval, 
				type, 
				series,
				intervalOffset, 
				intervalOffsetType,
				forceIntIndex,
				true);
		}

		/// <summary>
		/// Gets interval size as double number.
		/// </summary>
		/// <param name="current">Current value.</param>
		/// <param name="interval">Interval size.</param>
		/// <param name="type">AxisName of the interval (Month, Year, ...).</param>
		/// <param name="series">First series connected to the axis.</param>
		/// <param name="intervalOffset">Offset size.</param>
		/// <param name="intervalOffsetType">Offset type(Month, Year, ...).</param>
		/// <param name="forceIntIndex">Force Integer indexed</param>
		/// <param name="forceAbsInterval">Force Integer indexed</param>
		/// <returns>Interval size as double.</returns>
        internal static double GetIntervalSize(
			double current, 
			double interval, 
			DateTimeIntervalType type, 
			Series series,
			double intervalOffset, 
			DateTimeIntervalType intervalOffsetType,
			bool forceIntIndex,
			bool forceAbsInterval)
		{
			// AxisName is not date.
			if( type == DateTimeIntervalType.Number || type == DateTimeIntervalType.Auto )
			{
				return interval;
			}

			// Special case for indexed series
			if(series != null && series.IsXValueIndexed)
			{
				// Check point index
				int pointIndex = (int)Math.Ceiling(current - 1);
				if(pointIndex < 0)
				{
					pointIndex = 0;
				}
				if(pointIndex >= series.Points.Count || series.Points.Count <= 1)
				{
					return interval;
				}

				// Get starting and ending values of the closest interval
				double		adjuster = 0;
				double		xValue = series.Points[pointIndex].XValue;
				xValue = AlignIntervalStart(xValue, 1, type, null);
				double		xEndValue = xValue + GetIntervalSize(xValue, interval, type);
				xEndValue += GetIntervalSize(xEndValue, intervalOffset, intervalOffsetType);
				xValue += GetIntervalSize(xValue, intervalOffset, intervalOffsetType);
				if(intervalOffset < 0)
				{
					xValue = xValue + GetIntervalSize(xValue, interval, type);
					xEndValue = xEndValue + GetIntervalSize(xEndValue, interval, type);
				}

				// The first point in the series
				if(pointIndex == 0 && current < 0)
				{
					// Round the first point value depending on the interval type
					DateTime	dateValue = DateTime.FromOADate(series.Points[pointIndex].XValue);
					DateTime	roundedDateValue = dateValue;
					switch(type)
					{
						case(DateTimeIntervalType.Years): // Ignore hours,...
							roundedDateValue = new DateTime(dateValue.Year, 
								dateValue.Month, dateValue.Day, 0, 0, 0);
							break;

						case(DateTimeIntervalType.Months): // Ignore hours,...
							roundedDateValue = new DateTime(dateValue.Year, 
								dateValue.Month, dateValue.Day, 0, 0, 0);
							break;

						case(DateTimeIntervalType.Days): // Ignore hours,...
							roundedDateValue = new DateTime(dateValue.Year, 
								dateValue.Month, dateValue.Day, 0, 0, 0);
							break;

						case(DateTimeIntervalType.Hours): //
							roundedDateValue = new DateTime(dateValue.Year, 
								dateValue.Month, dateValue.Day, dateValue.Hour, 
								dateValue.Minute, 0);
							break;

						case(DateTimeIntervalType.Minutes):
							roundedDateValue = new DateTime(dateValue.Year, 
								dateValue.Month, 
								dateValue.Day, 
								dateValue.Hour, 
								dateValue.Minute, 
								dateValue.Second);
							break;

						case(DateTimeIntervalType.Seconds):
							roundedDateValue = new DateTime(dateValue.Year, 
								dateValue.Month, 
								dateValue.Day, 
								dateValue.Hour, 
								dateValue.Minute, 
								dateValue.Second,
								0);
							break;

						case(DateTimeIntervalType.Weeks):
							roundedDateValue = new DateTime(dateValue.Year, 
								dateValue.Month, dateValue.Day, 0, 0, 0);
							break;
					}

					// The first point value is exactly on the interval boundaries
					if(roundedDateValue.ToOADate() == xValue || roundedDateValue.ToOADate() == xEndValue)
					{
						return - current + 1;
					}
				}

				// Adjuster of 0.5 means that position should be between points
				++pointIndex;
				while(pointIndex < series.Points.Count)
				{
					if(series.Points[pointIndex].XValue >= xEndValue)
					{
						if(series.Points[pointIndex].XValue > xEndValue && !forceIntIndex)
						{
							adjuster = -0.5;
						}
						break;
					}

					++pointIndex;
				}

				// If last point outside of the max series index
				if(pointIndex == series.Points.Count)
				{
					pointIndex += series.Points.Count/5 + 1;
				}

				double size = (pointIndex + 1) - current + adjuster;
		
				return (size != 0) ? size : interval;
			}
	
			// Non indexed series
			else
			{
				DateTime	date = DateTime.FromOADate(current);
				TimeSpan	span = new TimeSpan(0);

				if(type == DateTimeIntervalType.Days)
				{
					span = TimeSpan.FromDays(interval);
				}
				else if(type == DateTimeIntervalType.Hours)
				{
					span = TimeSpan.FromHours(interval);
				}
				else if(type == DateTimeIntervalType.Milliseconds)
				{
					span = TimeSpan.FromMilliseconds(interval);
				}
				else if(type == DateTimeIntervalType.Seconds)
				{
					span = TimeSpan.FromSeconds(interval);
				}
				else if(type == DateTimeIntervalType.Minutes)
				{
					span = TimeSpan.FromMinutes(interval);
				}
				else if(type == DateTimeIntervalType.Weeks)
				{
					span = TimeSpan.FromDays(7.0 * interval);
				}
				else if(type == DateTimeIntervalType.Months)
				{
					// Special case handling when current date points 
					// to the last day of the month
					bool lastMonthDay = false;
					if(date.Day == DateTime.DaysInMonth(date.Year, date.Month))
					{
						lastMonthDay = true;
					}

					// Add specified amount of months
					date = date.AddMonths((int)Math.Floor(interval));
					span = TimeSpan.FromDays(30.0 * ( interval - Math.Floor(interval) ));

					// Check if last month of the day was used
					if(lastMonthDay && span.Ticks == 0)
					{
						// Make sure the last day of the month is selected
						int daysInMobth = DateTime.DaysInMonth(date.Year, date.Month);
						date = date.AddDays(daysInMobth - date.Day);
					}
				}
				else if(type == DateTimeIntervalType.Years)
				{
					date = date.AddYears((int)Math.Floor(interval));
					span = TimeSpan.FromDays(365.0 * ( interval - Math.Floor(interval) ));
				}

				// Check if an absolute interval size must be returned
				double result = date.Add(span).ToOADate() - current;
				if(forceAbsInterval)
				{
					result = Math.Abs(result);
				}
				return result;
			}
		}

		/// <summary>
		/// Check if series is indexed. IsXValueIndexed flag is set or all X values are zeros.
		/// </summary>
		/// <param name="series">Data series to test.</param>
		/// <returns>True if series is indexed.</returns>
		static internal bool IndexedSeries( Series series)
		{
            // X value indexed flag set
            if (series.IsXValueIndexed)
            {
                return true;
            }

            if (Utilities.CustomPropertyRegistry.IsXAxisQuantitativeChartTypes.Contains(series.ChartType) && 
                series.IsCustomPropertySet(Utilities.CustomPropertyName.IsXAxisQuantitative))
            {
                string attribValue = series[Utilities.CustomPropertyName.IsXAxisQuantitative];
                if (String.Compare(attribValue, "True", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return false;
                }
            }

            // Check if series has all X values set to zero
            return SeriesXValuesZeros(series);
		}

		/// <summary>
		/// Check if all data points in the series have X value set to 0.
		/// </summary>
		/// <param name="series">Data series to check.</param>
		static private bool SeriesXValuesZeros( Series series )
		{
			// Check if X value zeros check was already done
			if(series.xValuesZerosChecked)
			{
				return series.xValuesZeros;
			}

			// Data point loop
            series.xValuesZerosChecked = true;
			series.xValuesZeros = true;
			foreach( DataPoint point in series.Points )
			{
				if( point.XValue != 0.0 )
				{
					// If any data point has value different than 0 return false
					series.xValuesZeros = false;
					break;
				}
			}
			return series.xValuesZeros;
		}

		/// <summary>
		/// Check if any series is indexed. IsXValueIndexed flag is set or all X values are zeros.
		/// </summary>
        /// <param name="common">Reference to common chart classes.</param>
		/// <param name="series">Data series names.</param>
		/// <returns>True if any series is indexed.</returns>
        static internal bool IndexedSeries(CommonElements common, params string[] series)
		{
			// Data series loop
			bool	zeroXValues = true;
            foreach (string ser in series)
            {
                Series localSeries = common.DataManager.Series[ser];

                // Check series indexed flag
                if (localSeries.IsXValueIndexed)
                {
                    // If flag set in at least one series - all series are indexed
                    return true;
                }

                // Check if series has all X values set to zero
                if (zeroXValues && !IndexedSeries(localSeries))
                {
                    zeroXValues = false;
                }
            }

            return zeroXValues;
		}

		/// <summary>
		/// Check if all data points in many series have X value set to 0.
		/// </summary>
        /// <param name="common">Reference to common chart classes.</param>
		/// <param name="series">Data series.</param>
		/// <returns>True if all data points have value 0.</returns>
        static internal bool SeriesXValuesZeros(CommonElements common, params string[] series)
		{
			// Data series loop
			foreach( string ser in series )
			{
				// Check one series X values
				if(!SeriesXValuesZeros(common.DataManager.Series[ ser ]))
				{
					return false;
				}
			}
			return true;
		}

		#endregion
    }

    #endregion //ChartElement
}
