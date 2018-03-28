//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		AxisScaleBreaks.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	AxisScaleBreakStyle
//
//  Purpose:	Automatic scale breaks feature related classes.
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
	#region Enumerations

	/// <summary>
	/// An enumeration of line styles for axis scale breaks.
	/// </summary>
    public enum BreakLineStyle
	{
		/// <summary>
		/// No scale break line visible.
		/// </summary>
		None,

		/// <summary>
		/// Straight scale break.
		/// </summary>
		Straight,

		/// <summary>
		/// Wave scale break.
		/// </summary>
		Wave,

		/// <summary>
		/// Ragged scale break.
		/// </summary>
		Ragged,
	}

    /// <summary>
    /// An enumeration which indicates whether an axis segment should start
    /// from zero when scale break is used.
    /// </summary>
    public enum StartFromZero
    {
        /// <summary>
        /// Auto mode
        /// </summary>
        Auto,

        /// <summary>
        /// Start the axis segment scale from zero.
        /// </summary>
        Yes,

        /// <summary>
        /// Do not start the axis segment scale from zero.
        /// </summary>
        No

    };

	#endregion // Enumerations

	/// <summary>
	/// <b>AxisScaleBreakStyle</b> class represents the settings that control the scale break.
	/// </summary>
	[
	SRDescription("DescriptionAttributeAxisScaleBreakStyle_AxisScaleBreakStyle"),
	DefaultProperty("Enabled"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class AxisScaleBreakStyle
	{
        #region Fields

        // Associated axis
		internal Axis axis = null;

		// True if scale breaks are enabled
		private bool _enabled = false;

		// AxisName of the break line 
		private BreakLineStyle _breakLineStyle = BreakLineStyle.Ragged;

		// Spacing between scale segments created by scale breaks
		private double _segmentSpacing = 1.5;

		// Break line color
		private Color _breakLineColor = Color.Black;

		// Break line width
		private int _breakLineWidth = 1;

		// Break line style
		private ChartDashStyle _breakLineDashStyle = ChartDashStyle.Solid;

		// Minimum segment size in axis length percentage 
		private double _minSegmentSize = 10.0;

		// Number of segments the axis is devided into to perform statistical analysis
		private int _totalNumberOfSegments = 100;

		// Minimum "empty" size to be replace by the scale break
		private int _minimumNumberOfEmptySegments = 25;

		// Maximum number of breaks
		private int _maximumNumberOfBreaks = 2;

		// Indicates if scale segment should start from zero.
        private StartFromZero _startFromZero = StartFromZero.Auto;

		#endregion // Fields

		#region Constructor

		/// <summary>
        /// AxisScaleBreakStyle constructor.
		/// </summary>
		public AxisScaleBreakStyle()
		{
		}

		/// <summary>
        /// AxisScaleBreakStyle constructor.
		/// </summary>
		/// <param name="axis">Chart axis this class belongs to.</param>
        internal AxisScaleBreakStyle(Axis axis)
		{
			this.axis = axis;
		}

		#endregion // Constructor

		#region Properties

		/// <summary>
		/// Gets or sets a flag which indicates whether one of the axis segments should start its scale from zero 
		/// when scale break is used.
		/// </summary>
		/// <remarks>
        /// When property is set to <b>StartFromZero.Auto</b>, the range of the scale determines
		/// if zero value should be included in the scale.
		/// </remarks>
		[
		SRCategory("CategoryAttributeMisc"),
        DefaultValue(StartFromZero.Auto),
		SRDescription("DescriptionAttributeAxisScaleBreakStyle_StartFromZero"),
		]
        public StartFromZero StartFromZero
		{
			get
			{
				return this._startFromZero;
			}
			set
			{
				this._startFromZero = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Maximum number of scale breaks that can be used.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(2),
		SRDescription("DescriptionAttributeAxisScaleBreakStyle_MaxNumberOfBreaks"),
		]
		public int MaxNumberOfBreaks
		{
			get
			{
				return this._maximumNumberOfBreaks;
			}
			set
			{
				if(value < 1 || value > 5)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAxisScaleBreaksNumberInvalid));
				}
				this._maximumNumberOfBreaks = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Minimum axis scale region size, in percentage of the total axis length, 
		/// that can be collapsed with the scale break.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(25),
		SRDescription("DescriptionAttributeAxisScaleBreakStyle_CollapsibleSpaceThreshold"),
		]
		public int CollapsibleSpaceThreshold
		{
			get
			{
				return this._minimumNumberOfEmptySegments;
			}
			set
			{
				if(value < 10 || value > 90)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAxisScaleBreaksCollapsibleSpaceInvalid));
				}
				this._minimumNumberOfEmptySegments = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets a flag which determines if axis automatic scale breaks are enabled.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(false),
		SRDescription("DescriptionAttributeAxisScaleBreakStyle_Enabled"),
		ParenthesizePropertyNameAttribute(true),
		]
		public bool Enabled
		{
			get
			{
				return this._enabled;
			}
			set
			{
				this._enabled = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the style of the scale break line.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(BreakLineStyle.Ragged),
		SRDescription("DescriptionAttributeAxisScaleBreakStyle_BreakLineType"),
		]
		public BreakLineStyle BreakLineStyle
		{
			get
			{
				return this._breakLineStyle;
			}
			set
			{
				this._breakLineStyle = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the spacing of the scale break.
		/// </summary>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(1.5),
		SRDescription("DescriptionAttributeAxisScaleBreakStyle_Spacing"),
		]
		public double Spacing
		{
			get
			{
				return this._segmentSpacing;
			}
			set
			{
				if(value < 0.0 || value > 10)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAxisScaleBreaksSpacingInvalid));
				}
				this._segmentSpacing = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the color of the scale break line.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(typeof(Color), "Black"),
        SRDescription("DescriptionAttributeLineColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		public Color LineColor
		{
			get
			{
				return this._breakLineColor;
			}
			set
			{
				this._breakLineColor = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the width of the scale break line.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(1),
        SRDescription("DescriptionAttributeLineWidth"),
		]
		public int LineWidth
		{
			get
			{
				return this._breakLineWidth;
			}
			set
			{
				if(value < 1.0 || value > 10)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAxisScaleBreaksLineWidthInvalid));
				}
				this._breakLineWidth = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the line style of the scale break line.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(ChartDashStyle.Solid),
        SRDescription("DescriptionAttributeLineDashStyle"),
		]
		public ChartDashStyle LineDashStyle
		{
			get
			{
				return this._breakLineDashStyle;
			}
			set
			{
				this._breakLineDashStyle = value;
				this.Invalidate();
			}
		}

		#endregion // Properties

		#region Helper Methods

		/// <summary>
		/// Checks if automatic scale breaks are currently enabled.
		/// </summary>
		/// <returns>True if scale breaks are currently enabled.</returns>
		internal bool IsEnabled()
		{
			// Axis scale breaks must be enabled AND supported by the axis.
			if(this.Enabled && 
				this.CanUseAxisScaleBreaks())
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Checks if scale breaks can be used on specified axis.
		/// </summary>
		/// <returns>True if scale breaks can be used on this axis</returns>
		internal bool CanUseAxisScaleBreaks()
		{
			// Check input parameters
			if(this.axis == null || this.axis.ChartArea == null || this.axis.ChartArea.Common.Chart == null)
			{
				return false;
			}

			// No scale breaks in 3D charts
			if(this.axis.ChartArea.Area3DStyle.Enable3D)
			{
				return false;
			}

			// Axis scale break can only be applied to the Y and Y 2 axis
			if(this.axis.axisType == AxisName.X || this.axis.axisType == AxisName.X2)
			{
				return false;
			}
	
			// No scale breaks for logarithmic axis
			if(this.axis.IsLogarithmic)
			{
				return false;
			}

			// No scale breaks if axis zooming is enabled
			if(this.axis.ScaleView.IsZoomed)
			{
				return false;
			}

			// Check series associated with this axis
			ArrayList axisSeries = AxisScaleBreakStyle.GetAxisSeries(this.axis);
			foreach(Series series in axisSeries)
			{

				// Some special chart type are not supported
				if(series.ChartType == SeriesChartType.Renko || 
					series.ChartType == SeriesChartType.PointAndFigure)
				{
					return false;
				}


				// Get chart type interface
				IChartType chartType = this.axis.ChartArea.Common.ChartTypeRegistry.GetChartType(series.ChartTypeName);
				if(chartType == null)
				{
					return false;
				}

				// Circular and stacked chart types can not use scale breaks
				if(chartType.CircularChartArea || 
					chartType.Stacked || 
					!chartType.RequireAxes)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Gets a list of series objects attached to the specified axis.
		/// </summary>
		/// <param name="axis">Axis to get the series for.</param>
		/// <returns>A list of series that are attached to the specified axis.</returns>
		static internal ArrayList GetAxisSeries(Axis axis)
		{
			ArrayList seriesList = new ArrayList();
			if(axis != null && axis.ChartArea != null && axis.ChartArea.Common.Chart != null)
			{
				// Iterate through series in the chart
				foreach(Series series in axis.ChartArea.Common.Chart.Series)
				{
					// Series should be on the same chart area and visible
					if(series.ChartArea == axis.ChartArea.Name &&
						series.Enabled)
					{
						// Check primary/secondary axis
						if( (axis.axisType == AxisName.Y && series.YAxisType == AxisType.Secondary) || 
							(axis.axisType == AxisName.Y2 && series.YAxisType == AxisType.Primary))
						{
							continue;
						}

						// Add series into the list
						seriesList.Add(series);
					}
				}
			}
			return seriesList;
		}
	
		/// <summary>
		/// Invalidate chart control.
		/// </summary>
		private void Invalidate()
		{
			if(this.axis != null)
			{
				this.axis.Invalidate();
			}
		}

		#endregion // Helper Methods

		#region Series StatisticFormula Methods

		/// <summary>
		/// Get collection of axis segments to present scale breaks.
		/// </summary>
		/// <param name="axisSegments">Collection of axis scale segments.</param>
		internal void GetAxisSegmentForScaleBreaks(AxisScaleSegmentCollection axisSegments)
		{
			// Clear segment collection
			axisSegments.Clear();

			// Check if scale breaks are enabled
			if(this.IsEnabled())
			{
				// Fill collection of segments
				this.FillAxisSegmentCollection(axisSegments);

				// Check if more than 1 segments were defined
				if(axisSegments.Count >= 1)
				{
					// Get index of segment which scale should start from zero
					int startFromZeroSegmentIndex = this.GetStartScaleFromZeroSegmentIndex(axisSegments);

					// Calculate segment interaval and round the scale
					int index = 0;
					foreach(AxisScaleSegment axisScaleSegment in axisSegments)
					{
						// Check if segment scale should start from zero
						bool startFromZero = (index == startFromZeroSegmentIndex) ? true : false;

						// Calculate interval and round scale
						double minimum = axisScaleSegment.ScaleMinimum;
						double maximum = axisScaleSegment.ScaleMaximum;
						axisScaleSegment.Interval = this.axis.EstimateNumberAxis( 
							ref minimum, ref maximum, startFromZero, this.axis.prefferedNumberofIntervals, true, true);
						axisScaleSegment.ScaleMinimum = minimum; 
						axisScaleSegment.ScaleMaximum = maximum;

                        // Make sure new scale break value range do not exceed axis current scale
                        if (axisScaleSegment.ScaleMinimum < this.axis.Minimum)
                        {
                            axisScaleSegment.ScaleMinimum = this.axis.Minimum;
                        }
                        if (axisScaleSegment.ScaleMaximum > this.axis.Maximum)
                        {
                            axisScaleSegment.ScaleMaximum = this.axis.Maximum;
                        }

						// Increase segment index
						++index;
					}

                    // Defined axis scale segments cannot overlap. 
                    // Check for overlapping and join segments or readjust min/max.
                    bool adjustPosition = false;
                    AxisScaleSegment prevSegment = axisSegments[0];
                    for (int segmentIndex = 1; segmentIndex < axisSegments.Count; segmentIndex++)
                    {
                        AxisScaleSegment currentSegment = axisSegments[segmentIndex];
                        if (currentSegment.ScaleMinimum <= prevSegment.ScaleMaximum)
                        {
                            if (currentSegment.ScaleMaximum > prevSegment.ScaleMaximum)
                            {
                                // If segments are partially overlapping make sure the previous
                                // segment scale is extended
                                prevSegment.ScaleMaximum = currentSegment.ScaleMaximum;
                            }

                            // Remove the overlapped segment
                            adjustPosition = true;
                            axisSegments.RemoveAt(segmentIndex);
                            --segmentIndex;
                        }
                        else
                        {
                            prevSegment = currentSegment;
                        }
                    }

                    // Calculate the position of each segment
                    if (adjustPosition)
                    {
                        this.SetAxisSegmentPosition(axisSegments);
                    }
				}
			}
		}

        /// <summary>
        /// Gets index of segment that should be started from zero.
        /// </summary>
        /// <param name="axisSegments">Axis scale segment collection.</param>
        /// <returns>Index axis segment or -1.</returns>
		private int GetStartScaleFromZeroSegmentIndex(AxisScaleSegmentCollection axisSegments)
		{
            if (this.StartFromZero == StartFromZero.Auto ||
                this.StartFromZero == StartFromZero.Yes)
			{
				int index = 0;
				foreach(AxisScaleSegment axisScaleSegment in axisSegments)
				{
					// Check if zero value is already part of the scale
					if(axisScaleSegment.ScaleMinimum < 0.0 && axisScaleSegment.ScaleMaximum > 0.0)
					{
						return -1;
					}

					// As soon as we get first segment with positive minimum value or
					// we reached last segment adjust scale to start from zero.
					if(axisScaleSegment.ScaleMinimum > 0.0 ||
						index == (axisSegments.Count - 1) )
					{
						// Check if setting minimum scale to zero will make the
						// data points in the segment hard to read. This may hapen 
						// when the distance from zero to current minimum is 
						// significantly larger than current scale size.
                        if (this.StartFromZero == StartFromZero.Auto &&
							axisScaleSegment.ScaleMinimum > 2.0 * (axisScaleSegment.ScaleMaximum - axisScaleSegment.ScaleMinimum) )
						{
							return -1;
						}

						return index;
					}

					// Increase segment index
					++index;
				}
			}
			return -1;
		}

		/// <summary>
		/// Sets position of all scale segments in the axis.
		/// </summary>
		/// <param name="axisSegments">Collection of axis scale segments.</param>
		private void SetAxisSegmentPosition(AxisScaleSegmentCollection axisSegments)
		{
			// Calculate total number of points
			int totalPointNumber = 0;
			foreach(AxisScaleSegment axisScaleSegment in axisSegments)
			{
				if(axisScaleSegment.Tag is int)
				{
					totalPointNumber += (int)axisScaleSegment.Tag;
				}
			}

			// Calculate segment minimum size
			double minSize = Math.Min(this._minSegmentSize, Math.Floor(100.0 / axisSegments.Count));

			// Set segment position
			double currentPosition = 0.0;
			for(int index = 0; index < axisSegments.Count; index++)
			{
				axisSegments[index].Position = (currentPosition > 100.0) ? 100.0 : currentPosition;
				axisSegments[index].Size = Math.Round(((int)axisSegments[index].Tag) / (totalPointNumber / 100.0),5);
				if(axisSegments[index].Size < minSize)
				{
					axisSegments[index].Size = minSize;
				}
				
				// Set spacing for all segments except the last one
				if(index < (axisSegments.Count - 1) )
				{
					axisSegments[index].Spacing = this._segmentSpacing;
				}

				// Advance current position
				currentPosition += axisSegments[index].Size;
			}

			// Make sure we do not exceed the 100% axis length
			double totalHeight = 0.0;
			do
			{
				// Calculate total height
				totalHeight = 0.0;
				double maxSize = double.MinValue;
				int maxSizeIndex = -1;
				for(int index = 0; index < axisSegments.Count; index++)
				{
					totalHeight += axisSegments[index].Size;
					if(axisSegments[index].Size > maxSize)
					{
						maxSize = axisSegments[index].Size;
						maxSizeIndex = index;
					}
				}

				// If height is too large find largest segment 
				if(totalHeight > 100.0)
				{
					// Adjust segment size
					axisSegments[maxSizeIndex].Size -= totalHeight - 100.0;
					if(axisSegments[maxSizeIndex].Size < minSize)
					{
						axisSegments[maxSizeIndex].Size = minSize;
					}

					// Adjust position of the next segment
					double curentPosition = axisSegments[maxSizeIndex].Position + axisSegments[maxSizeIndex].Size;
					for(int index = maxSizeIndex + 1; index < axisSegments.Count; index++)
					{
						axisSegments[index].Position = curentPosition;
						curentPosition += axisSegments[index].Size;
					}
				}

			} while(totalHeight > 100.0);

		}

		/// <summary>
		/// Fill collection of axis scale segments.
		/// </summary>
		/// <param name="axisSegments">Collection of axis segments.</param>
		private void FillAxisSegmentCollection(AxisScaleSegmentCollection axisSegments)
		{
			// Clear axis segments collection
			axisSegments.Clear();

			// Get statistics for the series attached to the axis
			double minYValue = 0.0;
			double maxYValue = 0.0;
			double segmentSize = 0.0;
			double[] segmentMaxValue = null;
			double[] segmentMinValue = null;
            int[] segmentPointNumber = GetSeriesDataStatistics(
				this._totalNumberOfSegments, 
				out minYValue, 
				out maxYValue, 
				out segmentSize, 
				out segmentMaxValue, 
				out segmentMinValue);
            if (segmentPointNumber == null)
            {
                return;
            }

			// Calculate scale maximum and minimum
			double minimum = minYValue;
			double maximum = maxYValue;
			this.axis.EstimateNumberAxis(
				ref minimum, 
				ref maximum, 
				this.axis.IsStartedFromZero, 
				this.axis.prefferedNumberofIntervals, 
				true, 
				true);

            // Make sure max/min Y values are not the same
            if (maxYValue == minYValue)
            {
                return;
            }

			// Calculate the percentage of the scale range covered by the data range.
			double dataRangePercent = (maxYValue - minYValue) / ((maximum - minimum) / 100.0);

			// Get sequences of empty segments
			ArrayList	emptySequences = new ArrayList();
			bool doneFlag = false;
			while(!doneFlag)
			{
				doneFlag = true;

				// Get longest sequence of segments with no points
				int startSegment = 0; 
				int numberOfSegments = 0;
				this.GetLargestSequenseOfSegmentsWithNoPoints(
					segmentPointNumber, 
					out startSegment, 
					out numberOfSegments);

				// Adjust minimum empty segments  number depending on current segments
				int minEmptySegments = (int)(this._minimumNumberOfEmptySegments * (100.0 / dataRangePercent));
				if(axisSegments.Count > 0 && numberOfSegments > 0)
				{
					// Find the segment which contain newly found empty segments sequence
					foreach(AxisScaleSegment axisScaleSegment in axisSegments)
					{
						if(startSegment > 0 && (startSegment + numberOfSegments) <= segmentMaxValue.Length - 1)
						{
							if(segmentMaxValue[startSegment - 1] >= axisScaleSegment.ScaleMinimum &&
								segmentMinValue[startSegment + numberOfSegments] <= axisScaleSegment.ScaleMaximum)
							{
								// Get percentage of segment scale that is empty and suggested for collapsing
								double segmentScaleRange = axisScaleSegment.ScaleMaximum - axisScaleSegment.ScaleMinimum;
								double emptySpaceRange = segmentMinValue[startSegment + numberOfSegments] - segmentMaxValue[startSegment - 1];
								double emptySpacePercent = emptySpaceRange / (segmentScaleRange / 100.0);
								emptySpacePercent = emptySpacePercent / 100 * axisScaleSegment.Size;

								if(emptySpacePercent > minEmptySegments &&
									numberOfSegments > this._minSegmentSize)
								{
									minEmptySegments = numberOfSegments;
								}
							}
						}
					}
				}

				// Check if found sequence is long enough
				if(numberOfSegments >= minEmptySegments)
				{
					doneFlag = false;

					// Store start segment and number of segments in the list
					emptySequences.Add(startSegment);
					emptySequences.Add(numberOfSegments);

					// Check if there are any emty segments sequence found
					axisSegments.Clear();
					if(emptySequences.Count > 0)
					{
						double segmentFrom = double.NaN;
						double segmentTo = double.NaN;

						// Based on the segments that need to be excluded create axis segments that
						// will present on the axis scale.
						int numberOfPoints = 0;
						for(int index = 0; index < segmentPointNumber.Length; index++)
						{
							// Check if current segment is excluded
							bool excludedSegment = this.IsExcludedSegment(emptySequences, index);

							// If not excluded segment - update from/to range if they were set
							if(!excludedSegment && 
								!double.IsNaN(segmentMinValue[index]) &&
								!double.IsNaN(segmentMaxValue[index]))
							{
								// Calculate total number of points
								numberOfPoints += segmentPointNumber[index];

								// Set From/To of the visible segment
								if(double.IsNaN(segmentFrom))
								{
									segmentFrom = segmentMinValue[index];
									segmentTo = segmentMaxValue[index];
								}
								else
								{
									segmentTo = segmentMaxValue[index];
								}
							}

							// If excluded or last segment - add current visible segment range
							if(!double.IsNaN(segmentFrom) && 
								(excludedSegment || index == (segmentPointNumber.Length - 1) ))
							{
								// Make sure To and From do not match
								if(segmentTo == segmentFrom)
								{
									segmentFrom -= segmentSize;
									segmentTo += segmentSize;
								}

								// Add axis scale segment
								AxisScaleSegment axisScaleSegment = new AxisScaleSegment();
								axisScaleSegment.ScaleMaximum = segmentTo;
								axisScaleSegment.ScaleMinimum = segmentFrom;
								axisScaleSegment.Tag = numberOfPoints;
								axisSegments.Add(axisScaleSegment);

								// Reset segment range
								segmentFrom = double.NaN;
								segmentTo = double.NaN;
								numberOfPoints = 0;
							}
						}
					}

					// Calculate the position of each segment
					this.SetAxisSegmentPosition(axisSegments);
				}

				// Make sure we do not exceed specified number of breaks
				if( (axisSegments.Count - 1) >= this._maximumNumberOfBreaks)
				{
					doneFlag = true;
				}
			}

		}

		/// <summary>
		/// Check if segment was excluded.
		/// </summary>
		/// <param name="excludedSegments">Array of segment indexes.</param>
		/// <param name="segmentIndex">Index of the segment to check.</param>
		/// <returns>True if segment with specified index is marked as excluded.</returns>
		private bool IsExcludedSegment(ArrayList excludedSegments, int segmentIndex)
		{
			for(int index = 0; index < excludedSegments.Count; index += 2)
			{
				if(segmentIndex >= (int)excludedSegments[index] && 
					segmentIndex < (int)excludedSegments[index] + (int)excludedSegments[index + 1])
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Collect statistical information about the series.
		/// </summary>
		/// <param name="segmentCount">Segment count.</param>
		/// <param name="minYValue">Minimum Y value.</param>
		/// <param name="maxYValue">Maximum Y value.</param>
		/// <param name="segmentSize">Segment size.</param>
		/// <param name="segmentMaxValue">Array of segment scale maximum values.</param>
		/// <param name="segmentMinValue">Array of segment scale minimum values.</param>
		/// <returns></returns>
		internal int[] GetSeriesDataStatistics(
			int segmentCount, 
			out double minYValue, 
			out double maxYValue, 
			out double segmentSize,
			out double[] segmentMaxValue,
			out double[] segmentMinValue)
		{
			// Get all series associated with the axis
			ArrayList axisSeries = AxisScaleBreakStyle.GetAxisSeries(this.axis);

			// Get range of Y values from axis series
			minYValue = 0.0;
			maxYValue = 0.0;
			axis.Common.DataManager.GetMinMaxYValue(axisSeries, out minYValue, out maxYValue);
            
            int numberOfPoints = 0;
            foreach (Series series in axisSeries)
            {
                numberOfPoints = Math.Max(numberOfPoints, series.Points.Count);
            }
            
            if (axisSeries.Count == 0 || numberOfPoints == 0)
            {
                segmentSize = 0.0;
                segmentMaxValue = null;
                segmentMinValue = null;
                return null;
            }

			// Split range of values into predefined number of segments and calculate
			// how many points will be in each segment.
			segmentSize = (maxYValue - minYValue) / segmentCount;
			int[] segmentPointNumber = new int[segmentCount];
			segmentMaxValue = new double[segmentCount];
			segmentMinValue = new double[segmentCount];
			for(int index = 0; index < segmentCount; index++)
			{
				segmentMaxValue[index] = double.NaN;
				segmentMinValue[index] = double.NaN;
			}
			foreach(Series series in axisSeries)
			{
				// Get number of Y values to process
				int maxYValueCount = 1;
				IChartType chartType = this.axis.ChartArea.Common.ChartTypeRegistry.GetChartType(series.ChartTypeName);
				if(chartType != null)
				{
					if(chartType.ExtraYValuesConnectedToYAxis && chartType.YValuesPerPoint > 1)
					{
						maxYValueCount = chartType.YValuesPerPoint;
					}
				}

				// Iterate throug all data points
				foreach(DataPoint dataPoint in series.Points)
				{
					if(!dataPoint.IsEmpty)
					{
						// Iterate through all yValues
						for(int yValueIndex = 0; yValueIndex < maxYValueCount; yValueIndex++)
						{
							// Calculate index of the scale segment
							int segmentIndex = (int)Math.Floor((dataPoint.YValues[yValueIndex] - minYValue) / segmentSize);
							if(segmentIndex < 0)
							{
								segmentIndex = 0;
							}
							if(segmentIndex > segmentCount - 1)
							{
								segmentIndex = segmentCount - 1;
							}

							// Increase number points in that segment
							++segmentPointNumber[segmentIndex];

							// Store Min/Max values for the segment
							if(segmentPointNumber[segmentIndex] == 1)
							{
								segmentMaxValue[segmentIndex] = dataPoint.YValues[yValueIndex];
								segmentMinValue[segmentIndex] = dataPoint.YValues[yValueIndex];
							}
							else
							{
								segmentMaxValue[segmentIndex] = Math.Max(segmentMaxValue[segmentIndex], dataPoint.YValues[yValueIndex]);
								segmentMinValue[segmentIndex] = Math.Min(segmentMinValue[segmentIndex], dataPoint.YValues[yValueIndex]);
							}
						}
					}
				}
			}

			return segmentPointNumber;
		}

		/// <summary>
		/// Gets largest segment with no points.
		/// </summary>
		/// <param name="segmentPointNumber">Array that stores number of points for each segment.</param>
		/// <param name="startSegment">Returns largest empty segment sequence starting index.</param>
		/// <param name="numberOfSegments">Returns largest empty segment sequence length.</param>
		/// <returns>True if long empty segment sequence was found.</returns>
		internal bool GetLargestSequenseOfSegmentsWithNoPoints(
			int[] segmentPointNumber, 
			out int startSegment, 
			out int numberOfSegments)
		{
			// Find the longest sequence of empty segments
			startSegment = -1;
			numberOfSegments = 0;
			int currentSegmentStart = -1;
			int currentNumberOfSegments = -1;
			for(int index = 0; index < segmentPointNumber.Length; index++)
			{
				// Check for the segment with no points
				if(segmentPointNumber[index] == 0)
				{
					if(currentSegmentStart == -1)
					{
						currentSegmentStart = index;
						currentNumberOfSegments = 1;
					}
					else
					{
						++currentNumberOfSegments;
					}
				}
				
				// Check if longest sequence found
				if(currentNumberOfSegments > 0 && 
					(segmentPointNumber[index] != 0 || index == segmentPointNumber.Length - 1))
				{
					if(currentNumberOfSegments > numberOfSegments)
					{
						startSegment = currentSegmentStart;
						numberOfSegments = currentNumberOfSegments;
					}
					currentSegmentStart = -1;
					currentNumberOfSegments = 0;
				}
			}

			// Store value of "-1" in found sequence
			if(numberOfSegments != 0)
			{
				for(int index = startSegment; index < (startSegment + numberOfSegments); index++)
				{
					segmentPointNumber[index] = -1;
				}

				return true;
			}

			return false;
		}

		#endregion // Series StatisticFormula Methods
	}
}


