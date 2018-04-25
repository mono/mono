//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartAreaAxes.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ChartAreaAxes
//
//  Purpose:	ChartAreaAxes is base class of Chart Area class. 
//				This class searches for all series, which belongs 
//				to this chart area and sets axes minimum and 
//				maximum values using data. This class also checks 
//				for chart types, which belong to this chart area 
//				and prepare axis scale according to them (Stacked 
//				chart types have different max and min values). 
//				This class recognizes indexed values and prepares 
//				axes for them.
//
//	Reviewed:	GS - Jul 31, 2002
//				AG - August 7, 2002
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Collections.Generic;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
#else
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
#endif

#endregion

#if Microsoft_CONTROL
namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting
#endif
{
	/// <summary>
    /// ChartAreaAxes class represents axes (X, Y, X2 and Y2) in the chart area. 
    /// It contains methods that collect statistical information on the series data and 
    /// other axes related methods.
	/// </summary>
	public partial class ChartArea
	{
		#region Fields

		// Axes which belong to this Chart Area
		internal Axis					axisY = null;
		internal Axis					axisX = null;
		internal Axis					axisX2 = null;
		internal Axis					axisY2 = null;
		
		// Array of series which belong to this chart area
		private List<string>		    _series =		new List<string>();

		// Array of chart types which belong to this chart area
		internal ArrayList				chartTypes =	new ArrayList();

		/// <summary>
		/// List of series names that last interval numbers where cashed for
		/// </summary>
		private	 string					_intervalSeriesList = "";

		// Minimum interval between two data points for all 
		// series which belong to this chart area.
		internal double					intervalData = double.NaN;

		// Minimum interval between two data points for all 
		// series which belong to this chart area.
		// IsLogarithmic version of the interval.
		internal double					intervalLogData = double.NaN;

		// Series with minimum interval between two data points for all 
		// series which belong to this chart area.
		private Series					_intervalSeries = null;

		// Indicates that points are located through equal X intervals
		internal bool					intervalSameSize = false;

		// Indicates that points alignment checked
		internal bool					diffIntervalAlignmentChecked = false;

		// Chart Area contains stacked chart types
		internal bool					stacked = false;

		// Chart type with two y values used for scale ( bubble chart type )
		internal bool					secondYScale = false;

		// The X and Y axes are switched
		internal bool					switchValueAxes = false;

		// True for all chart types, which have axes. False for doughnut and pie chart.
		internal bool					requireAxes = true;

		// Indicates that chart area has circular shape (like in radar or polar chart)
		internal bool					chartAreaIsCurcular = false;

		// Chart Area contains 100 % stacked chart types
		internal bool					hundredPercent = false;

		// Chart Area contains 100 % stacked chart types
		internal bool					hundredPercentNegative = false;

		#endregion

		#region Internal properties

		/// <summary>
		/// True if sub axis supported on this chart area
		/// </summary>
		internal bool IsSubAxesSupported
		{
			get
			{
				if(((ChartArea)this).Area3DStyle.Enable3D ||
					((ChartArea)this).chartAreaIsCurcular)
				{
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Data series which belongs to this chart area.
		/// </summary>
		internal List<string> Series
		{
			get
			{
				return _series;
			}
		}

		/// <summary>
		/// Chart types which belongs to this chart area.
		/// </summary>
		internal ArrayList ChartTypes
		{
			get
			{
				return chartTypes;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Gets main or sub axis from the chart area.
		/// </summary>
		/// <param name="axisName">Axis name. NOTE: This parameter only defines X or Y axis. 
        /// Second axisType parameter is used to select primary or secondary axis. </param>
		/// <param name="axisType">Axis type.</param>
		/// <param name="subAxisName">Sub-axis name or empty string.</param>
		/// <returns>Main or sub axis of the chart area.</returns>
		internal Axis GetAxis(AxisName axisName, AxisType axisType, string subAxisName)
		{
			// Ignore sub axis in 3D
			if( ((ChartArea)this).Area3DStyle.Enable3D)
			{
				subAxisName = string.Empty;
			}

			if(axisName == AxisName.X || axisName == AxisName.X2)
			{
				if(axisType == AxisType.Primary)
				{
					return ((ChartArea)this).AxisX.GetSubAxis(subAxisName);
				}
				return ((ChartArea)this).AxisX2.GetSubAxis(subAxisName);
			}
			else 
			{
				if(axisType == AxisType.Primary)
				{
					return ((ChartArea)this).AxisY.GetSubAxis(subAxisName);
				}
				return ((ChartArea)this).AxisY2.GetSubAxis(subAxisName);
			}
		}

		/// <summary>
		/// Sets default axis values for all different chart type 
		/// groups. Chart type groups are sets of chart types.
		/// </summary>
		internal void SetDefaultAxesValues( )
		{
			// The X and Y axes are switched ( Bar chart, stacked bar ... )
			if( switchValueAxes )
			{
				// Set axis positions
				axisY.AxisPosition = AxisPosition.Bottom;
				axisX.AxisPosition = AxisPosition.Left;
				axisX2.AxisPosition = AxisPosition.Right;
				axisY2.AxisPosition = AxisPosition.Top;
			}
			else
			{
				// Set axis positions
				axisY.AxisPosition = AxisPosition.Left;
				axisX.AxisPosition = AxisPosition.Bottom;
				axisX2.AxisPosition = AxisPosition.Top;
				axisY2.AxisPosition = AxisPosition.Right;
			}

			// Reset opposite Axes field. This cashing 
			// value is used for optimization.
			foreach( Axis axisItem in ((ChartArea)this).Axes )
			{
                axisItem.oppositeAxis = null;
#if SUBAXES
				foreach( SubAxis subAxisItem in axisItem.SubAxes )
				{
					subAxisItem.m_oppositeAxis = null;
				}
#endif // SUBAXES
            }
				
			// ***********************
			// Primary X Axes
			// ***********************
			// Find the  number  of series which belong to this axis
            if (this.chartAreaIsCurcular)
            {
                // Set axis Maximum/Minimum and Interval for circular chart
                axisX.SetAutoMaximum(360.0);
                axisX.SetAutoMinimum(0.0);
                axisX.SetInterval = Math.Abs(axisX.maximum - axisX.minimum) / 12.0;
            }
            else
            {
                SetDefaultFromIndexesOrData(axisX, AxisType.Primary);
            }

#if SUBAXES
			// ***********************
			// Primary X Sub-Axes
			// ***********************
			foreach(SubAxis subAxis in axisX.SubAxes)
			{
                SetDefaultFromIndexesOrData(subAxis, AxisType.Primary);
			}
#endif // SUBAXES

            // ***********************
			// Secondary X Axes
			// ***********************
            SetDefaultFromIndexesOrData(axisX2, AxisType.Secondary);

#if SUBAXES
			// ***********************
			// Secondary X Sub-Axes
			// ***********************
			foreach(SubAxis subAxis in axisX2.SubAxes)
			{
                SetDefaultFromIndexesOrData(subAxis, AxisType.Secondary);
			}
#endif // SUBAXES

            // ***********************
			// Primary Y axis
			// ***********************
			if( GetYAxesSeries( AxisType.Primary, string.Empty ).Count != 0 )
			{
				// Find minimum and maximum from Y values.
				SetDefaultFromData( axisY );
				axisY.EstimateAxis();
            }

#if SUBAXES
			// ***********************
			// Primary Y Sub-Axes
			// ***********************
			foreach(SubAxis subAxis in axisY.SubAxes)
			{
				// Find the  number  of series which belong to this axis
				if( GetYAxesSeries( AxisType.Primary, subAxis.SubAxisName ).Count != 0 )
				{
					// Find minimum and maximum from Y values.
					SetDefaultFromData( subAxis );
					subAxis.EstimateAxis();
				}
			}
#endif // SUBAXES

            // ***********************
			// Secondary Y axis
			// ***********************
			if( GetYAxesSeries( AxisType.Secondary, string.Empty ).Count != 0 )
			{
				// Find minimum and maximum from Y values.
				SetDefaultFromData( axisY2 );
				axisY2.EstimateAxis();
            }

#if SUBAXES
			// ***********************
			// Secondary Y Sub-Axes
			// ***********************
			foreach(SubAxis subAxis in axisY2.SubAxes)
			{
				// Find the  number  of series which belong to this axis
				if( GetYAxesSeries( AxisType.Secondary, subAxis.SubAxisName ).Count != 0 )
				{
					// Find minimum and maximum from Y values.
					SetDefaultFromData( subAxis );
					subAxis.EstimateAxis();
				}
			}
#endif // SUBAXES

            // Sets axis position. Axis position depends 
			// on crossing and reversed value.
			axisX.SetAxisPosition();
			axisX2.SetAxisPosition();
			axisY.SetAxisPosition();
			axisY2.SetAxisPosition();

			// Enable axes, which are
			// used in data series.
			this.EnableAxes();

			


			// Get scale break segments
			Axis[] axesYArray = new Axis[] { axisY, axisY2 };
			foreach(Axis currentAxis in axesYArray)
			{
				// Get automatic scale break segments
				currentAxis.ScaleBreakStyle.GetAxisSegmentForScaleBreaks(currentAxis.ScaleSegments);

				// Make sure axis scale do not exceed segments scale
				if(currentAxis.ScaleSegments.Count > 0)
				{
					// Save flag that scale segments are used
					currentAxis.scaleSegmentsUsed = true;

					if(currentAxis.minimum < currentAxis.ScaleSegments[0].ScaleMinimum)
					{
						currentAxis.minimum = currentAxis.ScaleSegments[0].ScaleMinimum;
					}
					if(currentAxis.minimum > currentAxis.ScaleSegments[currentAxis.ScaleSegments.Count - 1].ScaleMaximum)
					{
						currentAxis.minimum = currentAxis.ScaleSegments[currentAxis.ScaleSegments.Count - 1].ScaleMaximum;
					}
				}
			}



			bool useScaleSegments = false;

			// Fill Labels
			Axis[] axesArray = new Axis[] { axisX, axisX2, axisY, axisY2 };
			foreach(Axis currentAxis in axesArray)
			{

				useScaleSegments = (currentAxis.ScaleSegments.Count > 0);

				if(!useScaleSegments)
				{
					currentAxis.FillLabels(true);
				}

				else
				{
					bool removeLabels = true;
					int segmentIndex = 0;
					foreach(AxisScaleSegment scaleSegment in currentAxis.ScaleSegments)
					{
						scaleSegment.SetTempAxisScaleAndInterval();

						currentAxis.FillLabels(removeLabels);
						removeLabels = false;

						scaleSegment.RestoreAxisScaleAndInterval();

						// Remove last label for all segments except of the last
						if(segmentIndex < (currentAxis.ScaleSegments.Count - 1) &&
							currentAxis.CustomLabels.Count > 0)
						{
							currentAxis.CustomLabels.RemoveAt(currentAxis.CustomLabels.Count - 1);
						}

						++segmentIndex;
					}
				}

			}
            foreach (Axis currentAxis in axesArray)
            {
                currentAxis.PostFillLabels();
            }
		}

        /// <summary>
        /// Sets the axis defaults. 
        /// If the at least one of the series bound to this axis is Indexed then the defaults are set using the SetDefaultsFromIndexes(). 
        /// Otherwise the SetDefaultFromData() is used.
        /// </summary>
        /// <param name="axis">Axis to process</param>
        /// <param name="axisType">Axis type</param>
        private void SetDefaultFromIndexesOrData(Axis axis, AxisType axisType) 
        {
            //Get array of the series that are linked to this axis
            List<string> axisSeriesNames = GetXAxesSeries(axisType, axis.SubAxisName);
            // VSTS: 196381
            // before this change: If we find one indexed series we will treat all series as indexed.
            // after this change : We will assume that all series are indexed.
            // If we find one non indexed series we will treat all series as non indexed.
            bool indexedSeries = true;
            // DT comments 1:
            // If we have mix of indexed with non-indexed series
            // enforce  all indexed series as non-indexed;
            // The result of mixed type of series will be more natural 
            // and easy to detect the problem - all datapoints of indexed 
            // series will be displayed on zero position.
            //=====================================
            // bool  nonIndexedSeries = false;
            //=======================================
            //Loop through the series looking for a indexed one
            foreach(string seriesName in axisSeriesNames)
            {
                // Get series
                Series series = Common.DataManager.Series[seriesName];
                // Check if series is indexed                
                if (!ChartHelper.IndexedSeries(series))
                {
                    // found one nonindexed series - we will treat all series as non indexed.
                    indexedSeries = false;
                    break;
                }
                // DT comments 2
                //else
                //{
                //    nonIndexedSeries = true;
                //}
            }

            //DT comments 3
            //if (!indexedSeries && nonIndexedSeries)
            //{
            //    foreach (string seriesName in axisSeriesNames)
            //    {
            //        // Get series
            //        Series series = Common.DataManager.Series[seriesName];
            //        series.xValuesZeros = false;
            //    }
            //}

            if (indexedSeries)
            {
                if (axis.IsLogarithmic)
                {
                    throw (new InvalidOperationException(SR.ExceptionChartAreaAxisScaleLogarithmicUnsuitable));
                }
                //Set axis defaults from the indexed series
                SetDefaultFromIndexes(axis);
                //We are done...
                return;
            }

           // If haven't found any indexed series -> Set axis defaults from the series data
           SetDefaultFromData(axis);
           axis.EstimateAxis();
        }

		/// <summary>
		/// Enable axes, which are
		/// used in chart area data series.
		/// </summary>
		private void EnableAxes()
		{
			if( _series == null )
			{
				return;
			}

			bool activeX = false;
			bool activeY = false;
			bool activeX2 = false;
			bool activeY2 = false;

			// Data series from this chart area
			foreach( string ser in _series )
			{
				Series	dataSeries = Common.DataManager.Series[ ser ];

				// X axes
				if( dataSeries.XAxisType == AxisType.Primary )
				{
					activeX = true;
#if SUBAXES
					this.Activate( axisX, true, dataSeries.XSubAxisName );
#else
                    this.Activate( axisX, true );
#endif // SUBAXES

                }
				else
				{
					activeX2 = true;
#if SUBAXES
					this.Activate( axisX2, true, dataSeries.XSubAxisName );
#else
                    this.Activate( axisX2, true );
#endif // SUBAXES
                }
				// Y axes
				if( dataSeries.YAxisType == AxisType.Primary )
				{
					activeY = true;
#if SUBAXES
					this.Activate( axisY, true, dataSeries.YSubAxisName );
#else
                    this.Activate( axisY, true );
#endif // SUBAXES
                }
				else
				{
					activeY2 = true;
#if SUBAXES
					this.Activate( axisY2, true, dataSeries.YSubAxisName );
#else
                    this.Activate( axisY2, true );
#endif // SUBAXES
                }
            }

#if SUBAXES			
			// Enable Axes
			if(!activeX)
				this.Activate( axisX, false, string.Empty );
			if(!activeY)
				this.Activate( axisY, false, string.Empty );
			if(!activeX2)
				this.Activate( axisX2, false, string.Empty );
			if(!activeY2)
				this.Activate( axisY2, false, string.Empty );
#else // SUBAXES
            // Enable Axes
			if(!activeX)
				this.Activate( axisX, false);
			if(!activeY)
				this.Activate( axisY, false);
			if(!activeX2)
				this.Activate( axisX2, false);
			if(!activeY2)
				this.Activate( axisY2, false);
#endif // SUBAXES
        }

#if SUBAXES

		/// <summary>
		/// Enable axis.
		/// </summary>
		/// <param name="axis">Axis.</param>
		/// <param name="active">True if axis is active.</param>
		/// <param name="subAxisName">Sub axis name to activate.</param>
		private void Activate( Axis axis, bool active, string subAxisName )
		{
			// Auto-Enable axis
			if( axis.autoEnabled == true ) 
			{
				axis.enabled = active;		
			}

			// Auto-Enable sub axes
			if(subAxisName.Length > 0)
			{
				SubAxis subAxis = axis.SubAxes.FindByName(subAxisName);
				if(subAxis != null)
				{
					if( subAxis.autoEnabled == true ) 
					{
						subAxis.enabled = active;		
					}
				}
			}
		}
#else
        /// <summary>
		/// Enable axis.
		/// </summary>
		/// <param name="axis">Axis.</param>
		/// <param name="active">True if axis is active.</param>
		private void Activate( Axis axis, bool active )
		{
			if( axis.autoEnabled == true ) 
			{
				axis.enabled = active;		
			}
        }
#endif // SUBAXES

        /// <summary>
		/// Check if all data points from series in 
		/// this chart area are empty.
		/// </summary>
		/// <returns>True if all points are empty</returns>
		bool AllEmptyPoints()
		{
			// Data series from this chart area
			foreach( string seriesName in this._series )
			{
				Series	dataSeries = Common.DataManager.Series[ seriesName ];

				// Data point loop
				foreach( DataPoint point in dataSeries.Points )
				{
					if( !point.IsEmpty )
					{
						return false;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// This method sets default minimum and maximum 
		/// values from values in the data manager. This 
		/// case is used if X values are not equal to 0 or IsXValueIndexed flag is set.
		/// </summary>
		/// <param name="axis">Axis</param>
		private void SetDefaultFromData( Axis axis )
        {
#if SUBAXES
			// Process all sub-axes
			if(!axis.IsSubAxis)
			{
				foreach(SubAxis subAxis in axis.SubAxes)
				{
					this.SetDefaultFromData( subAxis );
				}
			}
#endif // SUBAXES


            // Used for scrolling with logarithmic axes.
			if( !Double.IsNaN(axis.ScaleView.Position) && 
				!Double.IsNaN(axis.ScaleView.Size) &&
				!axis.refreshMinMaxFromData &&
				axis.IsLogarithmic )
			{
				return;
			}

			// Get minimum and maximum from data source
			double autoMaximum;
			double autoMinimum;
			this.GetValuesFromData( axis, out autoMinimum, out autoMaximum );

			// ***************************************************
			// This part of code is used to add a margin to the 
			// axis and to set minimum value to zero if 
			// IsStartedFromZero property is used. There is special 
			// code for logarithmic scale, which will set minimum 
			// to one instead of zero.
			// ***************************************************
			// The minimum and maximum values from data manager don’t exist.

			if( axis.enabled &&
				( (axis.AutoMaximum || double.IsNaN( axis.Maximum )) && (autoMaximum == Double.MaxValue || autoMaximum == Double.MinValue)) ||
				( (axis.AutoMinimum || double.IsNaN( axis.Minimum )) && (autoMinimum == Double.MaxValue || autoMinimum == Double.MinValue )) )
			{
				if( this.AllEmptyPoints() )
				{
					// Supress exception and use predefined min & max
					autoMaximum = 8.0;
					autoMinimum = 1.0;
				}
				else
				{
					if(!this.Common.ChartPicture.SuppressExceptions)
					{
                        throw (new InvalidOperationException(SR.ExceptionAxisMinimumMaximumInvalid)); 
					}
				}
			}

			// Axis margin used for zooming
			axis.marginView = 0.0;
			if( axis.margin == 100 && (axis.axisType == AxisName.X || axis.axisType == AxisName.X2) )
			{
				axis.marginView = this.GetPointsInterval( false, 10 );
			}

			// If minimum and maximum are same margin always exist.
			if( autoMaximum == autoMinimum &&
				axis.Maximum == axis.Minimum )
			{
				axis.marginView = 1;
			}

			// Do not make axis margine for logarithmic axes
			if( axis.IsLogarithmic )
			{
				axis.marginView = 0.0;
			}

			// Adjust Maximum - Add a gap
			if( axis.AutoMaximum ) 
			{
				// Add a Gap for X axis
				if( !axis.roundedXValues && ( axis.axisType == AxisName.X || axis.axisType == AxisName.X2 ) )
				{
					axis.SetAutoMaximum( autoMaximum + axis.marginView );
				}
				else
				{
					if( axis.isStartedFromZero && autoMaximum < 0 )
					{
						axis.SetAutoMaximum( 0.0 );
					}
					else
					{
						axis.SetAutoMaximum( autoMaximum );
					}
				}
			}

			// Adjust Minimum - make rounded values and add a gap
			if( axis.AutoMinimum )
			{
				// IsLogarithmic axis
				if( axis.IsLogarithmic )
				{
					if( autoMinimum < 1.0 ) 
					{
						axis.SetAutoMinimum( autoMinimum );
					}
					else if( axis.isStartedFromZero )
					{
						axis.SetAutoMinimum( 1.0 );
					}
					else
					{
						axis.SetAutoMinimum( autoMinimum );
					}
				}
				else
				{
					if( autoMinimum > 0.0 ) // If Auto calculated Minimum value is positive
					{
						// Adjust Minimum
						if( !axis.roundedXValues && ( axis.axisType == AxisName.X || axis.axisType == AxisName.X2 ) )
						{
							axis.SetAutoMinimum( autoMinimum - axis.marginView );
						}
						// If start From Zero property is true 0 is always on the axis.
						// NOTE: Not applicable if date-time values are drawn. Fixes issue #5644
						else if( axis.isStartedFromZero && 
							!this.SeriesDateTimeType( axis.axisType, axis.SubAxisName ) )
						{
							axis.SetAutoMinimum( 0.0 );
						}
						else
						{
							axis.SetAutoMinimum( autoMinimum );
						}
					}
					else // If Auto calculated Minimum value is non positive
					{
						if( axis.axisType == AxisName.X || axis.axisType == AxisName.X2 )
						{
							axis.SetAutoMinimum( autoMinimum - axis.marginView );
						}
						else
						{
							// If start From Zero property is true 0 is always on the axis.
							axis.SetAutoMinimum( autoMinimum );
						}
					}
				}
			}					

			// If maximum or minimum are not auto set value to non logarithmic
			if( axis.IsLogarithmic && axis.logarithmicConvertedToLinear )
			{
				if( !axis.AutoMinimum )
				{
					axis.minimum = axis.logarithmicMinimum;
				}

				if( !axis.AutoMaximum )
				{
					axis.maximum = axis.logarithmicMaximum;
				}
				// Min and max will take real values again if scale is logarithmic.
				axis.logarithmicConvertedToLinear = false;						
			}

			// Check if Minimum == Maximum
			if(this.Common.ChartPicture.SuppressExceptions &&
				axis.maximum == axis.minimum)
			{
				axis.minimum = axis.maximum;
				axis.maximum = axis.minimum + 1.0;
			}
		}

		/// <summary>
		/// This method checks if all series in the chart area have “integer type” 
		/// for specified axes, which means int, uint, long and ulong.
		/// </summary>
		/// <param name="axisName">Name of the axis</param>
		/// <param name="subAxisName">Sub axis name.</param>
		/// <returns>True if all series are integer</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "subAxisName")]
        internal bool SeriesIntegerType( AxisName axisName, string subAxisName )
		{
			// Series which belong to this chart area
			foreach( string seriesName in this._series ) 
			{
				Series ser = Common.DataManager.Series[ seriesName ];
				// X axes type
				if( axisName == AxisName.X )
                {
#if SUBAXES
					if(	ser.XAxisType == AxisType.Primary && ser.XSubAxisName == subAxisName)
#else //SUBAXES
                    if (	ser.XAxisType == AxisType.Primary)
#endif //SUBAXES
                    {
						if(ser.XValueType != ChartValueType.Int32 && 
							ser.XValueType != ChartValueType.UInt32 && 
							ser.XValueType != ChartValueType.UInt64 && 
							ser.XValueType != ChartValueType.Int64 )
						{
							return false;
						}
						else
						{
							return true;
						}
					}
				}
				// X axes type
				else if( axisName == AxisName.X2 )
                {
#if SUBAXES
					if(	ser.XAxisType == AxisType.Secondary && ser.XSubAxisName == subAxisName)
#else //SUBAXES
                    if (	ser.XAxisType == AxisType.Secondary)
#endif //SUBAXES

                    { 
						if(ser.XValueType != ChartValueType.Int32 && 
							ser.XValueType != ChartValueType.UInt32 && 
							ser.XValueType != ChartValueType.UInt64 && 
							ser.XValueType != ChartValueType.Int64 )
						{
							return false;
						}
						else
						{
							return true;
						}
					}
				}
				// Y axes type
				else if( axisName == AxisName.Y )
                {
#if SUBAXES
					if(	ser.YAxisType == AxisType.Primary && ser.YSubAxisName == subAxisName)
#else //SUBAXES
                    if (	ser.YAxisType == AxisType.Primary)
#endif //SUBAXES

                    { 
						if(ser.YValueType != ChartValueType.Int32 && 
							ser.YValueType != ChartValueType.UInt32 && 
							ser.YValueType != ChartValueType.UInt64 && 
							ser.YValueType != ChartValueType.Int64 )
						{
							return false;
						}
						else
						{
							return true;
						}
					}
				}
				else if( axisName == AxisName.Y2 )
                {
#if SUBAXES
					if(	ser.YAxisType == AxisType.Secondary && ser.YSubAxisName == subAxisName)
#else //SUBAXES
                    if (	ser.YAxisType == AxisType.Secondary)
#endif //SUBAXES

                    { 
						if(ser.YValueType != ChartValueType.Int32 && 
							ser.YValueType != ChartValueType.UInt32 && 
							ser.YValueType != ChartValueType.UInt64 && 
							ser.YValueType != ChartValueType.Int64 )
						{
							return false;
						}
						else
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// This method checks if all series in the chart area have “date-time type” 
		/// for specified axes.
		/// </summary>
		/// <param name="axisName">Name of the axis</param>
		/// <param name="subAxisName">Sub axis name.</param>
		/// <returns>True if all series are date-time.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "subAxisName")]
        internal bool SeriesDateTimeType( AxisName axisName, string subAxisName )
		{
			// Series which belong to this chart area
			foreach( string seriesName in this._series ) 
			{
				Series ser = Common.DataManager.Series[ seriesName ];
				// X axes type
				if( axisName == AxisName.X )
                {
#if SUBAXES
					if(	ser.XAxisType == AxisType.Primary && ser.XSubAxisName == subAxisName)
#else //SUBAXES
                    if (	ser.XAxisType == AxisType.Primary)
#endif //SUBAXES
                    {
						if(ser.XValueType != ChartValueType.Date && 
							ser.XValueType != ChartValueType.DateTime && 
							ser.XValueType != ChartValueType.Time &&
                            ser.XValueType != ChartValueType.DateTimeOffset)
						{
							return false;
						}
						else
						{
							return true;
						}
					}
				}
					// X axes type
				else if( axisName == AxisName.X2 )
                {
#if SUBAXES
					if(	ser.XAxisType == AxisType.Secondary && ser.XSubAxisName == subAxisName)
#else //SUBAXES
                    if (	ser.XAxisType == AxisType.Secondary)
#endif //SUBAXES
                    { 
						if(ser.XValueType != ChartValueType.Date && 
							ser.XValueType != ChartValueType.DateTime && 
							ser.XValueType != ChartValueType.Time &&
                            ser.XValueType != ChartValueType.DateTimeOffset)
						{
							return false;
						}
						else
						{
							return true;
						}
					}
				}
					// Y axes type
				else if( axisName == AxisName.Y )
                {
#if SUBAXES
					if(	ser.YAxisType == AxisType.Primary && ser.YSubAxisName == subAxisName)
#else //SUBAXES
                    if (	ser.YAxisType == AxisType.Primary)
#endif //SUBAXES
                    { 
						if(ser.YValueType != ChartValueType.Date && 
							ser.YValueType != ChartValueType.DateTime && 
							ser.YValueType != ChartValueType.Time &&
                            ser.YValueType != ChartValueType.DateTimeOffset)
						{
							return false;
						}
						else
						{
							return true;
						}
					}
				}
				else if( axisName == AxisName.Y2 )
                {
#if SUBAXES
					if(	ser.YAxisType == AxisType.Secondary && ser.YSubAxisName == subAxisName)
#else //SUBAXES
                    if (	ser.YAxisType == AxisType.Secondary)
#endif //SUBAXES
                    { 
						if(ser.YValueType != ChartValueType.Date && 
							ser.YValueType != ChartValueType.DateTime && 
							ser.YValueType != ChartValueType.Time &&
                            ser.YValueType != ChartValueType.DateTimeOffset)
						{
							return false;
						}
						else
						{
							return true;
						}
					}
				}
			}
			return false;
		}

        /// <summary>
        /// This method calculates minimum and maximum from data series.
        /// </summary>
        /// <param name="axis">Axis which is used to find minimum and maximum</param>
        /// <param name="autoMinimum">Minimum value from data.</param>
        /// <param name="autoMaximum">Maximum value from data.</param>
		private void GetValuesFromData( Axis axis, out double autoMinimum, out double autoMaximum )
		{
			// Get number of points in series
			int currentPointsNumber = this.GetNumberOfAllPoints();

			if( !axis.refreshMinMaxFromData && 
				!double.IsNaN(axis.minimumFromData) &&
				!double.IsNaN(axis.maximumFromData) &&
				axis.numberOfPointsInAllSeries == currentPointsNumber )
			{
				autoMinimum = axis.minimumFromData;
				autoMaximum = axis.maximumFromData;
				return;
			}

			// Set Axis type
			AxisType type = AxisType.Primary;
			if( axis.axisType == AxisName.X2 || axis.axisType == AxisName.Y2 )
			{
				type = AxisType.Secondary;
			}

			// Creates a list of series, which have same X axis type.
			string [] xAxesSeries = GetXAxesSeries(type, axis.SubAxisName).ToArray();

			// Creates a list of series, which have same Y axis type.
			string [] yAxesSeries = GetYAxesSeries( type, axis.SubAxisName ).ToArray();

			// Get auto maximum and auto minimum value
			if( axis.axisType == AxisName.X2 || axis.axisType == AxisName.X ) // X axis type is used (X or X2)
			{
				if( stacked ) // Chart area has a stacked chart types
				{
					try
					{
						Common.DataManager.GetMinMaxXValue(out autoMinimum, out autoMaximum, xAxesSeries );
					}
					catch(System.Exception)
					{
                        throw (new InvalidOperationException(SR.ExceptionAxisStackedChartsDataPointsNumberMismatch));
					}
				}

				// Chart type with two y values used for scale ( bubble chart type )
				else if( secondYScale )
				{
					autoMaximum = Common.DataManager.GetMaxXWithRadiusValue( (ChartArea)this, xAxesSeries );
					autoMinimum = Common.DataManager.GetMinXWithRadiusValue( (ChartArea)this, xAxesSeries );
					ChartValueType valueTypes = Common.DataManager.Series[xAxesSeries[0]].XValueType;
					if( valueTypes != ChartValueType.Date && 
                        valueTypes != ChartValueType.DateTime && 
                        valueTypes != ChartValueType.Time &&
                        valueTypes != ChartValueType.DateTimeOffset ) 
					{
						axis.roundedXValues = true;
					}
				}
				else
				{
					Common.DataManager.GetMinMaxXValue(out autoMinimum, out autoMaximum, xAxesSeries );
				}
			}
			else // Y axis type is used (Y or Y2)
			{				
				
				// *****************************
				// Stacked Chart AxisName
				// *****************************
				if( stacked ) // Chart area has a stacked chart types
				{
					try
					{
						if(hundredPercent)	// It's a hundred percent stacked chart
						{
							autoMaximum = Common.DataManager.GetMaxHundredPercentStackedYValue(hundredPercentNegative, yAxesSeries );
							autoMinimum = Common.DataManager.GetMinHundredPercentStackedYValue(hundredPercentNegative, yAxesSeries );
						}
						else
						{
							// If stacked groupes are used Min/Max range must calculated
							// for each group seperatly.
							double stackMaxBarColumn = double.MinValue;
							double stackMinBarColumn = double.MaxValue;
							double stackMaxArea = double.MinValue;
							double stackMinArea = double.MaxValue;

							// Split series by group names
							ArrayList	stackedGroups = this.SplitSeriesInStackedGroups(yAxesSeries);
							foreach(string[] groupSeriesNames in stackedGroups)
							{
								// For stacked bar and column
								double stackMaxBarColumnForGroup = Common.DataManager.GetMaxStackedYValue(0, groupSeriesNames );
								double stackMinBarColumnForGroup = Common.DataManager.GetMinStackedYValue(0, groupSeriesNames );

								// For stacked area
								double stackMaxAreaForGroup = Common.DataManager.GetMaxUnsignedStackedYValue(0, groupSeriesNames );
								double stackMinAreaForGroup = Common.DataManager.GetMinUnsignedStackedYValue(0, groupSeriesNames );

								// Select minimum/maximum
								stackMaxBarColumn = Math.Max(stackMaxBarColumn, stackMaxBarColumnForGroup);
								stackMinBarColumn = Math.Min(stackMinBarColumn, stackMinBarColumnForGroup);
								stackMaxArea = Math.Max(stackMaxArea, stackMaxAreaForGroup);
								stackMinArea = Math.Min(stackMinArea, stackMinAreaForGroup);
							}

							
							autoMaximum = Math.Max(stackMaxBarColumn,stackMaxArea);
							autoMinimum = Math.Min(stackMinBarColumn,stackMinArea);
						}
						// IsLogarithmic axis
						if( axis.IsLogarithmic && autoMinimum < 1.0 )
							autoMinimum = 1.0;
					}
					catch(System.Exception)
					{
                        throw (new InvalidOperationException(SR.ExceptionAxisStackedChartsDataPointsNumberMismatch));
					}
				}
				// Chart type with two y values used for scale ( bubble chart type )
				else if( secondYScale )
				{
					autoMaximum = Common.DataManager.GetMaxYWithRadiusValue( (ChartArea)this, yAxesSeries );
					autoMinimum = Common.DataManager.GetMinYWithRadiusValue( (ChartArea)this, yAxesSeries );
				}

				// *****************************
				// Non Stacked Chart Types
				// *****************************
				else
				{
					// Check if any series in the area has ExtraYValuesConnectedToYAxis flag set
					bool extraYValuesConnectedToYAxis = false;
					if(this.Common != null && this.Common.Chart != null)
					{
						foreach(Series series in this.Common.Chart.Series)
						{
							if(series.ChartArea == ((ChartArea)this).Name)
							{
								IChartType charType = Common.ChartTypeRegistry.GetChartType( series.ChartTypeName );
								if(charType != null && charType.ExtraYValuesConnectedToYAxis)
								{
									extraYValuesConnectedToYAxis = true;
									break;
								}
							}
						}
					}

					// The first Chart type can have many Y values (Stock Chart, Range Chart)
					if( extraYValuesConnectedToYAxis )
					{
						Common.DataManager.GetMinMaxYValue(out autoMinimum, out autoMaximum, yAxesSeries );
					}
					else
					{ // The first Chart type can have only one Y value
						Common.DataManager.GetMinMaxYValue(0, out autoMinimum, out autoMaximum, yAxesSeries );
					}
				}
			}

			// Store Minimum and maximum from data. There is no 
			// reason to calculate this values every time.
			axis.maximumFromData = autoMaximum;
			axis.minimumFromData = autoMinimum;
			axis.refreshMinMaxFromData = false;

			// Make extra test for stored minimum and maximum values 
			// from data. If Number of points is different then data 
			// source is changed. That means that we should read 
			// data again.
			axis.numberOfPointsInAllSeries = currentPointsNumber;
		}


		/// <summary>
		/// Splits a single array of series names into multiple arrays
		/// based on the stacked group name.
		/// </summary>
		/// <param name="seriesNames">Array of series name to split.</param>
		/// <returns>An array list that contains sub-arrays of series names split by group name.</returns>
		private ArrayList SplitSeriesInStackedGroups(string[] seriesNames)
		{
			Hashtable groupsHashTable = new Hashtable();
			foreach(string seriesName in seriesNames)
			{
				// Get series object
				Series series = this.Common.Chart.Series[seriesName];

				// NOTE: Fix for issue #6716
				// Double check that series supports stacked group feature
                string groupName = string.Empty;
				if(StackedColumnChart.IsSeriesStackGroupNameSupported(series))
				{
					// Get stacked group name (empty string by default)
					groupName = StackedColumnChart.GetSeriesStackGroupName(series);
				}

                // Check if this group was alreday added in to the hashtable
                if (groupsHashTable.ContainsKey(groupName))
                {
                    ArrayList list = (ArrayList)groupsHashTable[groupName];
                    list.Add(seriesName);
                }
                else
                {
                    ArrayList list = new ArrayList();
                    list.Add(seriesName);
                    groupsHashTable.Add(groupName, list);
                }
            }

			// Convert results to a list that contains array of strings
			ArrayList result = new ArrayList();
			foreach(DictionaryEntry entry in groupsHashTable)
			{
				ArrayList list = (ArrayList)entry.Value;
				if(list.Count > 0)
				{
					int index = 0;
					string[] stringArray = new String[list.Count];
					foreach(string str in list)
					{
						stringArray[index++] = str;
					}
					result.Add(stringArray);
				}
			}

			return result;
		}



		/// <summary>
		/// Find number of points for all series
		/// </summary>
		/// <returns>Number of points</returns>
		private int GetNumberOfAllPoints()
		{
			int numOfPoints = 0;
			foreach( Series series in Common.DataManager.Series )
			{
				numOfPoints += series.Points.Count;
			}

			return numOfPoints;
		}

		/// <summary>
		/// This method sets default minimum and maximum values from 
		/// indexes. This case is used if all X values in a series 
		/// have 0 value or IsXValueIndexed flag is set.
		/// </summary>
		/// <param name="axis">Axis</param>
		private void SetDefaultFromIndexes(  Axis axis )
		{
			// Adjust margin for side-by-side charts like column
			axis.SetTempAxisOffset( );
			
			// Set Axis type
			AxisType type = AxisType.Primary;
			if( axis.axisType == AxisName.X2 || axis.axisType == AxisName.Y2 )
			{
				type = AxisType.Secondary;
			}

			// The maximum is equal to the number of data points.
			double autoMaximum = Common.DataManager.GetNumberOfPoints( GetXAxesSeries( type, axis.SubAxisName ).ToArray() );
			double autoMinimum = 0.0;

			// Axis margin used only for zooming
			axis.marginView = 0.0;
			if( axis.margin == 100 )
				axis.marginView = 1.0;
			
			// If minimum and maximum are same margin always exist.
			if( autoMaximum + axis.margin/100 == autoMinimum - axis.margin/100 + 1 )
			{
				// Set Maximum Number.
				axis.SetAutoMaximum( autoMaximum + 1 );
				axis.SetAutoMinimum( autoMinimum );
			}
			else // Nomal case
			{
				// Set Maximum Number.
				axis.SetAutoMaximum( autoMaximum + axis.margin/100 );
				axis.SetAutoMinimum( autoMinimum - axis.margin/100 + 1 );
			}

			// Find the interval. If the nuber of points 
			// is less then 10 interval is 1.
			double axisInterval;
			
			if( axis.ViewMaximum - axis.ViewMinimum <= 10 ) 
			{
				axisInterval = 1.0;
			}
			else
			{
				axisInterval = axis.CalcInterval( ( axis.ViewMaximum - axis.ViewMinimum ) / 5 );
			}

			ChartArea area = (ChartArea)this;
			if( area.Area3DStyle.Enable3D && !double.IsNaN(axis.interval3DCorrection) )
			{
				axisInterval = Math.Ceiling( axisInterval / axis.interval3DCorrection );

				axis.interval3DCorrection = double.NaN;

				// Use interval 
				if( axisInterval > 1.0 && 
					axisInterval < 4.0 && 
					axis.ViewMaximum - axis.ViewMinimum <= 4 ) 
				{
					axisInterval = 1.0;
				}

			}

			axis.SetInterval = axisInterval;

			// If temporary offsets were defined for the margin, 
			// adjust offset for minor ticks and grids.
			if(axis.offsetTempSet)
			{
				axis.minorGrid.intervalOffset -= axis.MajorGrid.GetInterval();
				axis.minorTickMark.intervalOffset -= axis.MajorTickMark.GetInterval();
			}
		}

		/// <summary>
		/// Sets the names of all data series which belong to
		/// this chart area to collection and sets a list of all 
		/// different chart types.
		/// </summary>
        internal void SetData()
        {
            this.SetData(true, true);
        }

        /// <summary>
        /// Sets the names of all data series which belong to
        /// this chart area to collection and sets a list of all
        /// different chart types.
        /// </summary>
        /// <param name="initializeAxes">If set to <c>true</c> the method will initialize axes default values.</param>
        /// <param name="checkIndexedAligned">If set to <c>true</c> the method will check that all primary X axis series are aligned if use the IsXValueIndexed flag.</param>
		internal void SetData( bool initializeAxes, bool checkIndexedAligned)
		{
			// Initialize chart type properties
			stacked = false;
			switchValueAxes = false;
			requireAxes = true;
			hundredPercent = false;
			hundredPercentNegative = false;
			chartAreaIsCurcular = false;
			secondYScale = false;
			
			// AxisName of the chart area already set.
			bool typeSet = false;

			// Remove all elements from the collection
			this._series.Clear();

            // Add series to the collection
			foreach( Series series in Common.DataManager.Series )
			{
                if (series.ChartArea == this.Name && series.IsVisible() && series.Points.Count > 0)
				{
					this._series.Add(series.Name);
				}
			}

			// Remove all elements from the collection
			this.chartTypes.Clear();

			// Add series to the collection
			foreach( Series series in Common.DataManager.Series )
			{
				// A item already exist.
				bool foundItem = false;
                if (series.IsVisible() && series.ChartArea==this.Name)
                {
					foreach( string type in chartTypes )
					{
						// AxisName already exist in the chart area
						if( type == series.ChartTypeName )
						{
							foundItem = true;
						}
					}
					// Add chart type to the collection of
					// Chart area's chart types
					if( !foundItem )
					{
						// Set stacked type
						if( Common.ChartTypeRegistry.GetChartType(series.ChartTypeName).Stacked )
						{
							stacked = true;
						}

						if( !typeSet )
						{
							if( Common.ChartTypeRegistry.GetChartType(series.ChartTypeName).SwitchValueAxes )
								switchValueAxes = true;
							if( !Common.ChartTypeRegistry.GetChartType(series.ChartTypeName).RequireAxes )
								requireAxes = false;
							if( Common.ChartTypeRegistry.GetChartType(series.ChartTypeName).CircularChartArea )
								chartAreaIsCurcular = true;
							if( Common.ChartTypeRegistry.GetChartType(series.ChartTypeName).HundredPercent )
								hundredPercent = true;
							if( Common.ChartTypeRegistry.GetChartType(series.ChartTypeName).HundredPercentSupportNegative )
								hundredPercentNegative = true;
							if( Common.ChartTypeRegistry.GetChartType(series.ChartTypeName).SecondYScale )
								secondYScale = true;
							
							typeSet = true;
						}
						else
						{
							if( Common.ChartTypeRegistry.GetChartType(series.ChartTypeName).SwitchValueAxes != switchValueAxes )
							{
                                throw (new InvalidOperationException(SR.ExceptionChartAreaChartTypesCanNotCombine));
							}
						}
						
						// Series is not empty
						if( Common.DataManager.GetNumberOfPoints( series.Name ) != 0 )
						{
							this.chartTypes.Add( series.ChartTypeName );
						}
					}
				}
			}

			// Check that all primary X axis series are aligned if use the IsXValueIndexed flag
            if (checkIndexedAligned)
            {
                for (int axisIndex = 0; axisIndex <= 1; axisIndex++)
                {
                    List<string> seriesArray = this.GetXAxesSeries((axisIndex == 0) ? AxisType.Primary : AxisType.Secondary, string.Empty);
                    if (seriesArray.Count > 0)
                    {
                        bool indexed = false;
                        string seriesNamesStr = "";
                        foreach (string seriesName in seriesArray)
                        {
                            seriesNamesStr = seriesNamesStr + seriesName.Replace(",", "\\,") + ",";
                            if (Common.DataManager.Series[seriesName].IsXValueIndexed)
                            {
                                indexed = true;
                            }
                        }

                        if (indexed)
                        {
                            try
                            {
                                Common.DataManipulator.CheckXValuesAlignment(
                                    Common.DataManipulator.ConvertToSeriesArray(seriesNamesStr.TrimEnd(','), false));
                            }
                            catch (Exception e)
                            {
                                throw (new ArgumentException(SR.ExceptionAxisSeriesNotAligned + e.Message));
                            }
                        }
                    }
                }
			}
            if (initializeAxes)
            {
                // Set default min, max etc.
                SetDefaultAxesValues();
            }
		}

		/// <summary>
		/// Returns names of all series, which belong to this chart area 
		/// and have same chart type.
		/// </summary>
		/// <param name="chartType">Chart type</param>
		/// <returns>Collection with series names</returns>
		internal List<string> GetSeriesFromChartType( string chartType )
		{
			// New collection
            List<string> list = new List<string>();
			
			foreach( string seriesName in _series )
			{
				if( String.Compare( chartType, Common.DataManager.Series[seriesName].ChartTypeName, StringComparison.OrdinalIgnoreCase ) == 0 )
				{
					// Add a series name to the collection
					list.Add( seriesName );
				}
			}

			return list;
		}

		/// <summary>
		/// Returns all series which belong to this chart area.
		/// </summary>
		/// <returns>Collection with series</returns>
		internal List<Series> GetSeries(  )
		{
			// New collection
            List<Series> list = new List<Series>();
			
			foreach( string seriesName in _series )
			{
                list.Add(Common.DataManager.Series[seriesName]);
            }

			return list;
		}

		/// <summary>
		/// Creates a list of series, which have same X axis type.
		/// </summary>
		/// <param name="type">Axis type</param>
		/// <param name="subAxisName">Sub Axis name</param>
		/// <returns>A list of series</returns>
		internal List<string> GetXAxesSeries( AxisType type, string subAxisName )
		{
			// Create a new collection of series
            List<string> list = new List<string>();
            if (_series.Count == 0)
            {
                return list;
            }
			// Ignore sub axis in 3D
			if( !this.IsSubAxesSupported )
			{
				if(subAxisName.Length > 0)
				{
					return list;
				}
			}

			// Find series which have same axis type
			foreach( string ser in _series )
            {
#if SUBAXES
				if( Common.DataManager.Series[ser].XAxisType == type &&
					(Common.DataManager.Series[ser].XSubAxisName == subAxisName || !this.IsSubAxesSupported) )
#else // SUBAXES
                if ( Common.DataManager.Series[ser].XAxisType == type)
#endif // SUBAXES
                {
					// Add a series to the collection
					list.Add( ser );
				}
            }

#if SUBAXES
			// If series list is empty for the sub-axis then
			// try using the main axis.
			if ( list.Count == 0 && subAxisName.Length > 0 )
			{
				return GetXAxesSeries( type, string.Empty );
			}
#endif // SUBAXES

            // If primary series do not exist return secondary series
			// Axis should always be connected with any series.
			if ( list.Count == 0  )
			{
                if (type == AxisType.Secondary)
                {
                    return GetXAxesSeries(AxisType.Primary, string.Empty);
                }
                return GetXAxesSeries(AxisType.Secondary, string.Empty);
			}
            
			return list;
		}

		/// <summary>
		/// Creates a list of series, which have same Y axis type.
		/// </summary>
		/// <param name="type">Axis type</param>
		/// <param name="subAxisName">Sub Axis name</param>
		/// <returns>A list of series</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "subAxisName")]
		internal List<string> GetYAxesSeries( AxisType type, string subAxisName )
		{
			// Create a new collection of series
            List<string> list = new List<string>();

			// Find series which have same axis type
			foreach( string ser in _series )
			{
                // Get series Y axis type
                AxisType seriesYAxisType = Common.DataManager.Series[ser].YAxisType;
#if SUBAXES
                string seriesYSubAxisName = subAxisName;
#endif // SUBAXES

                // NOTE: Fixes issue #6969
                // Ignore series settings if only Primary Y axis supported by the chart type
                if (Common.DataManager.Series[ser].ChartType == SeriesChartType.Radar ||
                    Common.DataManager.Series[ser].ChartType == SeriesChartType.Polar)
                {
                    seriesYAxisType = AxisType.Primary;
#if SUBAXES
                    seriesYSubAxisName = string.Empty;
#endif // SUBAXES
                }


#if SUBAXES
				if( seriesYAxisType == type &&
					(Common.DataManager.Series[ser].YSubAxisName == seriesYSubAxisName || !this.IsSubAxesSupported) )
#else // SUBAXES
                if (seriesYAxisType == type)
#endif // SUBAXES
                {
					// Add a series to the collection
					list.Add( ser );
				}
            }

#if SUBAXES
			// If series list is empty for the sub-axis then
			// try using the main axis.
			if ( list.Count == 0 && subAxisName.Length > 0 )
			{
				return GetYAxesSeries( type, string.Empty );
			}
#endif // SUBAXES

            // If primary series do not exist return secondary series
			// Axis should always be connected with any series.
			if ( list.Count == 0 && type == AxisType.Secondary )
			{
                return GetYAxesSeries( AxisType.Primary, string.Empty );
			}

			return list;
		}

		/// <summary>
		/// Get first series from the chart area
		/// </summary>
		/// <returns>Data series</returns>
		internal Series GetFirstSeries()
		{
			if( _series.Count == 0 )
			{
                throw (new InvalidOperationException(SR.ExceptionChartAreaSeriesNotFound));
			}

			return Common.DataManager.Series[_series[0]];
		}
		
		/// <summary>
		/// This method returns minimum interval between 
		/// any two data points from series which belong
		/// to this chart area.
		/// </summary>
		/// <param name="isLogarithmic">Indicates logarithmic scale.</param>
		/// <param name="logarithmBase">Logarithm Base</param>
		/// <returns>Minimum Interval</returns>
		internal double GetPointsInterval(bool isLogarithmic, double logarithmBase)
		{
			bool sameInterval;
			return GetPointsInterval( _series, isLogarithmic, logarithmBase, false, out sameInterval );
		}
		
		/// <summary>
		/// This method returns minimum interval between 
		/// any two data points from specified series. 
		/// </summary>
		/// <param name="seriesList">List of series.</param>
		/// <param name="isLogarithmic">Indicates logarithmic scale.</param>
		/// <param name="logarithmBase">Base for logarithmic base</param>
		/// <param name="checkSameInterval">True if check for the same interval should be performed.</param>
		/// <param name="sameInterval">Return true if interval is the same.</param>
		/// <returns>Minimum Interval</returns>
		internal double GetPointsInterval( List<string> seriesList, bool isLogarithmic, double logarithmBase, bool checkSameInterval, out bool sameInterval )
		{
			Series nullSeries = null;
            return GetPointsInterval(seriesList, isLogarithmic, logarithmBase, checkSameInterval, out sameInterval, out nullSeries);
		}
		
		/// <summary>
		/// This method returns minimum interval between 
		/// any two data points from specified series. 
		/// </summary>
		/// <param name="seriesList">List of series.</param>
		/// <param name="isLogarithmic">Indicates logarithmic scale.</param>
		/// <param name="logarithmicBase">Logarithm Base</param>
		/// <param name="checkSameInterval">True if check for the same interval should be performed.</param>
		/// <param name="sameInterval">Return true if interval is the same.</param>
		/// <param name="series">Series with the smallest interval between points.</param>
		/// <returns>Minimum Interval</returns>
		internal double GetPointsInterval( List<string> seriesList, bool isLogarithmic, double logarithmicBase, bool checkSameInterval, out bool sameInterval, out Series series )
		{
			long	ticksInterval = long.MaxValue;
			int		monthsInteval = 0;
			double	previousInterval = double.MinValue;
			double	oldInterval = Double.MaxValue;

			// Initialize return value
			sameInterval = true;
			series = null;

			// Create comma separate string of series names
			string	seriesNames = "";
			if(seriesList != null)
			{
				foreach( string serName in seriesList )
				{
					seriesNames += serName + ",";
				}
			}

			// Do not calculate interval every time;
			if( checkSameInterval == false || diffIntervalAlignmentChecked == true)
			{
                if (!isLogarithmic)
				{
					if( !double.IsNaN(intervalData) && _intervalSeriesList == seriesNames)
					{
						sameInterval = intervalSameSize;
						series = _intervalSeries;
						return intervalData;
					}
				}
				else
				{
					if( !double.IsNaN(intervalLogData) && _intervalSeriesList == seriesNames)
					{
						sameInterval = intervalSameSize;
						series = _intervalSeries;
						return intervalLogData;
					}
				}
			}

			// Data series loop
			int			seriesIndex = 0;
			Series		currentSmallestSeries = null;
			ArrayList[] seriesXValues = new ArrayList[seriesList.Count];
			foreach( string ser in seriesList )
			{
				Series	dataSeries = Common.DataManager.Series[ ser ];
				bool isXValueDateTime = dataSeries.IsXValueDateTime();

				// Copy X values to array and prepare for sorting Sort X values.
				seriesXValues[seriesIndex] = new ArrayList();
				bool	sortPoints = false;
				double	prevXValue = double.MinValue;
				double	curentXValue = 0.0;
				if(dataSeries.Points.Count > 0)
				{
                    if (isLogarithmic)
					{
						prevXValue = Math.Log(dataSeries.Points[0].XValue, logarithmicBase);
					}
					else
					{
						prevXValue = dataSeries.Points[0].XValue;
					}
				}
				foreach( DataPoint point in dataSeries.Points )
				{
                    if (isLogarithmic)
					{
						curentXValue = Math.Log(point.XValue, logarithmicBase);
					}
					else
					{
						curentXValue = point.XValue;
					}

					if(prevXValue > curentXValue)
					{
						sortPoints = true;
					}

					seriesXValues[seriesIndex].Add(curentXValue);
					prevXValue = curentXValue;
				}

				//  Sort X values
				if(sortPoints)
				{
					seriesXValues[seriesIndex].Sort();
				}

				// Data point loop
				for( int point = 1; point < seriesXValues[seriesIndex].Count; point++ )
				{
					// Interval between two sorted data points.
					double	interval = Math.Abs( (double)seriesXValues[seriesIndex][ point - 1 ] - (double)seriesXValues[seriesIndex][ point ] );

					// Check if all intervals are same
					if(sameInterval)
					{
						if(isXValueDateTime)
						{
							if(ticksInterval == long.MaxValue)
							{
								// Calculate first interval
								GetDateInterval(
									(double)seriesXValues[seriesIndex][ point - 1 ], 
									(double)seriesXValues[seriesIndex][ point ],
									out monthsInteval, 
									out ticksInterval);
							}
							else
							{
								// Calculate current interval
								long	curentTicksInterval = long.MaxValue;
								int		curentMonthsInteval = 0;
								GetDateInterval(
									(double)seriesXValues[seriesIndex][ point - 1 ], 
									(double)seriesXValues[seriesIndex][ point ],
									out curentMonthsInteval, 
									out curentTicksInterval);

								// Compare current interval with previous
								if(curentMonthsInteval != monthsInteval || curentTicksInterval != ticksInterval)
								{
									sameInterval = false;
								}

							}
						}
						else
						{
							if( previousInterval != interval && previousInterval != double.MinValue )
							{
								sameInterval = false;
							}
						}
					}

					previousInterval = interval;

					// If not minimum interval keep the old one
					if( oldInterval > interval && interval != 0)
					{
						oldInterval = interval;
						currentSmallestSeries = dataSeries;
					}
				}

				++seriesIndex;
			}

			// If interval is not the same check if points from all series are aligned
			this.diffIntervalAlignmentChecked = false;
			if( checkSameInterval &&  !sameInterval && seriesXValues.Length > 1)
			{
				bool	sameXValue = false;
				this.diffIntervalAlignmentChecked = true;

				// All X values must be same
				int	listIndex = 0;
				foreach(ArrayList xList in seriesXValues)
				{
					for(int pointIndex = 0; pointIndex < xList.Count && !sameXValue; pointIndex++)
					{
						double	xValue = (double)xList[pointIndex];

						// Loop through all other lists and see if point is there
						for(int index = listIndex + 1; index < seriesXValues.Length && !sameXValue; index++)
						{
							if( (pointIndex < seriesXValues[index].Count && (double)seriesXValues[index][pointIndex] == xValue) ||
								seriesXValues[index].Contains(xValue))
							{
								sameXValue = true;
								break;
							}
						}
					}

					++listIndex;
				}


				// Use side-by-side if at least one xommon X value between eries found
				if(sameXValue)
				{
					sameInterval = true;
				}
			}


			// Interval not found. Interval is 1.
			if( oldInterval == Double.MaxValue)
			{
				oldInterval = 1;
			}

			intervalSameSize = sameInterval;
            if (!isLogarithmic)
			{
				intervalData = oldInterval;
				_intervalSeries = currentSmallestSeries;
				series = _intervalSeries;
				_intervalSeriesList = seriesNames;
				return intervalData;
			}
			else
			{
				intervalLogData = oldInterval;
				_intervalSeries = currentSmallestSeries;
				series = _intervalSeries;
				_intervalSeriesList = seriesNames;
				return intervalLogData;
			}
		}

		/// <summary>
		/// Calculates the difference between two values in years, months, days, ...
		/// </summary>
		/// <param name="value1">First value.</param>
		/// <param name="value2">Second value.</param>
		/// <param name="monthsInteval">Interval in months.</param>
		/// <param name="ticksInterval">Interval in ticks.</param>
		private void GetDateInterval(double value1, double value2, out int monthsInteval, out long ticksInterval)
		{
			// Convert values to dates
			DateTime	date1 = DateTime.FromOADate(value1);
			DateTime	date2 = DateTime.FromOADate(value2);

			// Calculate months difference
			monthsInteval = date2.Month - date1.Month;
			monthsInteval += (date2.Year - date1.Year) * 12;

			// Calculate interval in ticks for days, hours, ...
			ticksInterval = 0;
			ticksInterval += (date2.Day - date1.Day) * TimeSpan.TicksPerDay;
			ticksInterval += (date2.Hour - date1.Hour) * TimeSpan.TicksPerHour;
			ticksInterval += (date2.Minute - date1.Minute) * TimeSpan.TicksPerMinute;
			ticksInterval += (date2.Second - date1.Second) * TimeSpan.TicksPerSecond;
			ticksInterval += (date2.Millisecond - date1.Millisecond) * TimeSpan.TicksPerMillisecond;
		}

		#endregion
	}
}
