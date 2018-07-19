//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		StackedBarChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	StackedBarChart, HundredPercentStackedBarChart
//
//  Purpose:	This class contains all necessary methods and 
//				properties for drawing and selection of the stacked 
//				bar	and hundred percent stacked bar charts. 
//				Every data point in the Stacked bar chart is 
//				represented with one rectangle. If there is 
//				more then one series with this chart type from 
//				same chart area, bars with same X values are 
//				Stacked.
//
//	Reviewed:	AG - Aug 6, 2002
//              AG - Microsoft 7, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;

#else
	using System.Web.UI.DataVisualization.Charting.Utilities;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
#endif

#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting.ChartTypes
#else
	namespace System.Web.UI.DataVisualization.Charting.ChartTypes
#endif
{
	/// <summary>
    /// HundredPercentStackedBarChart class extends StackedBarChart class
    /// by providing its own algorithm for calculating series data point
    /// Y values. It makes sure that total Y value of all data points in a
    /// single cluster from all series adds up to 100%.
    /// </summary>
	internal class HundredPercentStackedBarChart : StackedBarChart
	{
		#region Constructor

		/// <summary>
		/// Default constructor.
		/// </summary>
		public HundredPercentStackedBarChart()
		{
			hundredPercentStacked = true;
		}

		#endregion 
		
		#region Fields



		// Total Y values from all series at specified index orgonized by stacked groups
		// Hashtable will contain arrays of doubles stored by group name key.
		Hashtable		_stackedGroupsTotalPerPoint = null;


		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		override public string Name			{ get{ return ChartTypeNames.OneHundredPercentStackedBar;}}

		/// <summary>
		/// Indicates that it's a hundredred percent chart.
		/// Axis scale from 0 to 100 percent should be used.
		/// </summary>
		override public bool HundredPercent{ get{return true;} }

		/// <summary>
		/// Indicates that it's a hundredred percent chart.
		/// Axis scale from 0 to 100 percent should be used.
		/// </summary>
		override public bool HundredPercentSupportNegative{ get{return true;} }

		#endregion

		#region Painting and selection methods

		/// <summary>
        /// Paint HundredPercentStackedBarChart Chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		override public void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
		{
			// Reset pre-calculated totals

			this._stackedGroupsTotalPerPoint = null;

			// Call base class painting
			base.Paint( graph, common, area, seriesToDraw );
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
		override public double GetYValue(CommonElements common, ChartArea area, Series series, DataPoint point, int pointIndex, int yValueIndex)
		{
			// Array of Y totals for individual series index in the current stacked group
			double[] currentGroupTotalPerPoint = null;


			string currentStackedGroupName = HundredPercentStackedColumnChart.GetSeriesStackGroupName(series);
			if(this._stackedGroupsTotalPerPoint == null)
			{
				// Create new hashtable
				this._stackedGroupsTotalPerPoint = new Hashtable();

				// Iterate through all stacked groups
				foreach(string groupName in this.stackGroupNames)
				{
					// Get series that belong to the same group
					Series[] seriesArray = HundredPercentStackedColumnChart.GetSeriesByStackedGroupName(
                        common, groupName, series.ChartTypeName, series.ChartArea);

					// Check if series are aligned
					common.DataManipulator.CheckXValuesAlignment(seriesArray);

					// Allocate memory for the array of totals
					double[] totals = new double[series.Points.Count];

					// Calculate the total of Y value per point 
					for(int index = 0; index < series.Points.Count; index++)
					{
						totals[index] = 0;
						foreach( Series ser in seriesArray )
						{
							totals[index] += Math.Abs(ser.Points[index].YValues[0]);
						}
					}

					// Add totals array into the hashtable
					this._stackedGroupsTotalPerPoint.Add(groupName, totals);
				}
			}

			// Find array of total Y values based on the current stacked group name
			currentGroupTotalPerPoint = (double[])this._stackedGroupsTotalPerPoint[currentStackedGroupName];


			if(!area.Area3DStyle.Enable3D)
			{
				if(point.YValues[0] == 0 || point.IsEmpty)
				{
					return 0;
				}
			}

			// Calculate stacked column Y value for 2D chart
			if(area.Area3DStyle.Enable3D == false || yValueIndex == -2)
			{
				if(currentGroupTotalPerPoint[pointIndex] == 0.0)
				{
					return 0.0;
				}
				return (point.YValues[0] / currentGroupTotalPerPoint[pointIndex]) * 100.0;
			}

			// Get point Height if pointIndex == -1
			double yValue = double.NaN;
			if(yValueIndex == -1)
			{
				Axis	vAxis = area.GetAxis(AxisName.Y, series.YAxisType, series.YSubAxisName);
				double	barZeroValue = vAxis.Crossing;
				yValue = GetYValue(common, area, series, point, pointIndex, 0);
				if( yValue >= 0 )
				{
					if(!double.IsNaN(prevPosY))
					{
						barZeroValue = prevPosY;
					}
				}
				else
				{
					if(!double.IsNaN(prevNegY))
					{
						barZeroValue = prevNegY;
					}
				}

				return yValue - barZeroValue;
			}

			
			// Loop through all series to find point value
			prevPosY = double.NaN;
			prevNegY = double.NaN;
			foreach(Series ser in common.DataManager.Series)
			{
				// Check series of the current chart type & area
				if(String.Compare(series.ChartArea, ser.ChartArea, StringComparison.Ordinal) == 0 &&
                    String.Compare(series.ChartTypeName, ser.ChartTypeName, StringComparison.OrdinalIgnoreCase) == 0 &&
					ser.IsVisible())
				{

					// Series must belong to the same stacked group
					if(currentStackedGroupName != HundredPercentStackedColumnChart.GetSeriesStackGroupName(ser))
					{
						continue;
					}


					if(double.IsNaN(yValue))
					{
						if(currentGroupTotalPerPoint[pointIndex] == 0.0)
						{
							yValue = 0.0;
						}
						else
						{
							yValue = (ser.Points[pointIndex].YValues[0] / currentGroupTotalPerPoint[pointIndex]) * 100.0;
						}
					}
					else
					{
						if(currentGroupTotalPerPoint[pointIndex] == 0.0)
						{
							yValue = 0.0;
						}
						else
						{
							yValue = (ser.Points[pointIndex].YValues[0] / currentGroupTotalPerPoint[pointIndex]) * 100.0;
						}
						if(yValue >= 0.0 && !double.IsNaN(prevPosY))
						{
							yValue += prevPosY;
						}
						if(yValue < 0.0 && !double.IsNaN(prevNegY))
						{
							yValue += prevNegY;
						}
					}

					// Exit loop when current series was found
                    if (String.Compare(series.Name, ser.Name, StringComparison.Ordinal) == 0)
					{
						break;
					}

					// Save previous value
					if(yValue >= 0.0)
					{
						prevPosY = yValue;
					}
					else
					{
						prevNegY = yValue;
					}
				}
			}
			
			return (yValue > 100.0) ? 100.0 : yValue;
		}

		#endregion
	}

	/// <summary>
    /// StackedBarChart class contains all the code necessary to draw 
    /// and hit test Stacked Bar chart. 
    /// </summary>
	internal class StackedBarChart : IChartType
	{
		#region Fields

		/// <summary>
		/// Previous stacked positive Y values.
		/// </summary>
		protected	double	prevPosY = double.NaN;

		/// <summary>
		/// Previous stacked negative Y values.
		/// </summary>
		protected	double	prevNegY = double.NaN;

		/// <summary>
		/// Indicates if chart is 100% stacked
		/// </summary>
		protected	bool			hundredPercentStacked = false;



		/// <summary>
		/// True if stacke group name is applicable
		/// </summary>
		internal	bool			stackGroupNameUsed = false;

		/// <summary>
		/// List of all stack group names
		/// </summary>
		internal	ArrayList		stackGroupNames = null;

		/// <summary>
		/// Name of the current stack group.
		/// </summary>
		internal	string			currentStackGroup = string.Empty;



		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		virtual public string Name			{ get{ return ChartTypeNames.StackedBar;}}

		/// <summary>
		/// Gets chart type image.
		/// </summary>
		/// <param name="registry">Chart types registry object.</param>
		/// <returns>Chart type image.</returns>
        virtual public System.Drawing.Image GetImage(ChartTypeRegistry registry)
		{
			return (System.Drawing.Image)registry.ResourceManager.GetObject(this.Name + "ChartType");
		}

		/// <summary>
		/// True if chart type is stacked
		/// </summary>
		public bool Stacked		{ get{ return true;}}


		/// <summary>
		/// True if stacked chart type supports groups
		/// </summary>
		virtual public bool SupportStackedGroups	{ get { return true; } }


		/// <summary>
		/// True if stacked chart type should draw separately positive and 
		/// negative data points ( Bar and column Stacked types ).
		/// </summary>
		public bool StackSign		{ get{ return true;}}

		/// <summary>
		/// True if chart type supports axeses
		/// </summary>
		public bool RequireAxes	{ get{ return true;} }

		/// <summary>
		/// Chart type with two y values used for scale ( bubble chart type )
		/// </summary>
		public bool SecondYScale{ get{ return false;} }

		/// <summary>
		/// True if chart type requires circular chart area.
		/// </summary>
		public bool CircularChartArea	{ get{ return false;} }

		/// <summary>
		/// True if chart type supports logarithmic axes
		/// </summary>
		public bool SupportLogarithmicAxes	{ get{ return true;} }

		/// <summary>
		/// True if chart type requires to switch the value (Y) axes position
		/// </summary>
		public bool SwitchValueAxes	{ get{ return true;} }

		/// <summary>
		/// True if chart series can be placed side-by-side.
		/// </summary>
		public bool SideBySideSeries { get{ return false;} }

		/// <summary>
		/// If the crossing value is auto Crossing value should be 
		/// automatically set to zero for some chart 
		/// types (Bar, column, area etc.)
		/// </summary>
		public bool ZeroCrossing { get{ return true;} }

		/// <summary>
		/// True if each data point of a chart must be represented in the legend
		/// </summary>
		public bool DataPointsInLegend	{ get{ return false;} }

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
		/// True if palette colors should be applied for each data paoint.
		/// Otherwise the color is applied to the series.
		/// </summary>
		public bool ApplyPaletteColorsToPoints	{ get { return false; } }

		/// <summary>
		/// How to draw series/points in legend:
		/// Filled rectangle, Line or Marker
		/// </summary>
		/// <param name="series">Legend item series.</param>
		/// <returns>Legend item style.</returns>
		public LegendImageStyle GetLegendImageStyle(Series series)
		{
			return LegendImageStyle.Rectangle;
		}

		/// <summary>
		/// Number of supported Y value(s) per point 
		/// </summary>
		public int YValuesPerPoint{ get { return 1; } }

		#endregion

		#region Painting and selection methods

		/// <summary>
		/// Paint Stacked Bar Chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object</param>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		virtual public void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
		{		

			// Reset stacked group names flag
			this.stackGroupNameUsed = true;


			// Set Clip Region in rounded to a pixel coordinates
			RectangleF areaPosition = ((ChartGraphics)graph).GetAbsoluteRectangle( area.PlotAreaPosition.ToRectangleF());
			float right = (float)Math.Ceiling(areaPosition.Right);
			float bottom = (float)Math.Ceiling(areaPosition.Bottom);
			areaPosition.X = (float)Math.Floor(areaPosition.X);
			areaPosition.Width = right - areaPosition.X;
			areaPosition.Y = (float)Math.Floor(areaPosition.Y);
			areaPosition.Height = bottom - areaPosition.Y;
			((ChartGraphics)graph).SetClipAbs( areaPosition );

			// Draw shadow
			ProcessChartType( false, graph, common, area, true, false, seriesToDraw );

			// Draw stacked bars
			ProcessChartType( false, graph, common, area, false, false, seriesToDraw );

			// Draw labels
			ProcessChartType( false, graph, common, area, false, true, seriesToDraw );

			// Reset Clip Region
			((ChartGraphics)graph).ResetClip();
		}

		/// <summary>
		/// This method recalculates size of the stacked bars. This method is used 
		/// from Paint or Select method.
		/// </summary>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="shadow">True if shadow mode is active.</param>
		/// <param name="labels">Labels drawing mode.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		private void ProcessChartType( 
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			bool shadow,
			bool labels,
			Series seriesToDraw )
		{

			//************************************************************
			//** If stacked series is attached to diferent X and Y axis
			//** they can not be processed. To solve this issue series 
			//** will be orgonized in groups based on the axes.
			//************************************************************

			// Loop through all series and check if different axes are used
			bool differentAxesAreUsed = false;
			AxisType xAxisType = AxisType.Primary;
			AxisType yAxisType = AxisType.Primary;
			string xSubAxisName = string.Empty;
			string ySubAxisName = string.Empty;
			for(int seriesIndex = 0; seriesIndex < common.DataManager.Series.Count; seriesIndex++)
			{
				// Process non empty series of the area with stacked column chart type
				Series ser = common.DataManager.Series[seriesIndex];
				if( String.Compare( ser.ChartTypeName, Name, StringComparison.OrdinalIgnoreCase ) != 0 
					|| ser.ChartArea != area.Name || !ser.IsVisible())
				{
					continue;
				}

				if(seriesIndex == 0)
				{
					xAxisType = ser.XAxisType;
					yAxisType = ser.YAxisType;
					xSubAxisName = ser.XSubAxisName;
					ySubAxisName = ser.YSubAxisName;
				}
				else if(xAxisType != ser.XAxisType ||
					yAxisType != ser.YAxisType ||
					xSubAxisName != ser.XSubAxisName ||
					ySubAxisName != ser.YSubAxisName)
				{
					differentAxesAreUsed = true;
					break;
				}
			}

			// Set stacked groups based on the axes used
			if(differentAxesAreUsed)
			{
				for(int seriesIndex = 0; seriesIndex < common.DataManager.Series.Count; seriesIndex++)
				{
					// Process non empty series of the area with stacked column chart type
					Series ser = common.DataManager.Series[seriesIndex];
					if( String.Compare( ser.ChartTypeName, Name, StringComparison.OrdinalIgnoreCase ) != 0 
						|| ser.ChartArea != area.Name || !ser.IsVisible())
					{
						continue;
					}

					// Set new group name
					string stackGroupName = StackedColumnChart.GetSeriesStackGroupName(ser);
					stackGroupName = "_X_" + ser.XAxisType.ToString() + ser.XSubAxisName + "_Y_" + ser.YAxisType.ToString() + ser.YSubAxisName + "__"; 
					ser[CustomPropertyName.StackedGroupName] = stackGroupName;
				}
			}

			//************************************************************
			//** Check how many stack groups are available.
			//************************************************************
			
			// Loop through all series and get unique stack group names.
			this.stackGroupNames = new ArrayList();
			foreach( Series ser in common.DataManager.Series )
			{
				// Process non empty series of the area with stacked column chart type
				if( String.Compare( ser.ChartTypeName, Name, StringComparison.OrdinalIgnoreCase ) != 0 
					|| ser.ChartArea != area.Name || !ser.IsVisible())
				{
					continue;
				}

				// Get stack group name from the series
				string stackGroupName = StackedColumnChart.GetSeriesStackGroupName(ser);

				// Add group name if it do not already exsist
				if(!this.stackGroupNames.Contains(stackGroupName))
				{
					this.stackGroupNames.Add(stackGroupName);
				}
			}


			// Prosess 3D chart type
			if(area.Area3DStyle.Enable3D)
			{
				if(!shadow)
				{
					ProcessChartType3D( 
						selection, 
						graph, 
						common, 
						area, 
						labels,
						seriesToDraw );
				}

				return;
			}
			
			// All data series from chart area which have Stacked Bar chart type
			string[]	seriesList = area.GetSeriesFromChartType(Name).ToArray();

			// Get maximum number of data points for all series
			int		maxNumOfPoints = common.DataManager.GetNumberOfPoints(seriesList);

			// Zero X values mode.
			bool	indexedSeries = ChartHelper.IndexedSeries( common, seriesList);

			//************************************************************
			//** Loop through all data points
			//************************************************************
			for( int pointIndx = 0; pointIndx < maxNumOfPoints; pointIndx++ )
			{

				//************************************************************
				//** Loop through all stack groups
				//************************************************************
				for(int groupIndex = 0;  groupIndex < this.stackGroupNames.Count; groupIndex++)
				{
					// Rememmber current stack group name
					this.currentStackGroup = (string)this.stackGroupNames[groupIndex];

					int		seriesIndx = 0;		// Data series index
					double	PreviousPosY = 0;	// Previous positive Y value
					double	PreviousNegY = 0;	// Previous negative Y value

					//************************************************************
					//** Loop through all series
					//************************************************************
					foreach( Series ser in common.DataManager.Series )
					{
						// Process non empty series of the area with stacked bar chart type
						if( String.Compare( ser.ChartTypeName, Name, StringComparison.OrdinalIgnoreCase ) != 0 
							|| ser.ChartArea != area.Name || !ser.IsVisible())
						{
							continue;
						}

						// Series point index is out of range
						if( pointIndx >= ser.Points.Count )
						{
							continue;
						}

	
						// Check if series belongs to the current group name
						string seriesStackGroupName = StackedColumnChart.GetSeriesStackGroupName(ser);
						if(seriesStackGroupName != this.currentStackGroup)
						{
							continue;
						}

	

						// Get data point
						DataPoint point = ser.Points[ pointIndx ];

						// Reset pre-calculated point position
						point.positionRel = new PointF(float.NaN, float.NaN);

						// Set active horizontal/vertical axis
						Axis	vAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
						Axis	hAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

						// Interval between bars
						double interval = 1;
						if( !indexedSeries )
						{
                            if (ser.Points.Count == 1 &&
                                (ser.XValueType == ChartValueType.Date || 
                                 ser.XValueType == ChartValueType.DateTime || 
                                 ser.XValueType == ChartValueType.Time ||
                                 ser.XValueType == ChartValueType.DateTimeOffset))
                            {
                                // Check if interval is the same
                                bool sameInterval = false;
                                List<string> typeSeries = area.GetSeriesFromChartType(Name);
                                area.GetPointsInterval(typeSeries, vAxis.IsLogarithmic, vAxis.logarithmBase, true, out sameInterval);

                                // Special case when there is only one data point and date scale is used.
                                if (!double.IsNaN(vAxis.majorGrid.GetInterval()) && vAxis.majorGrid.GetIntervalType() != DateTimeIntervalType.NotSet)
                                {
                                    interval = ChartHelper.GetIntervalSize(vAxis.minimum, vAxis.majorGrid.GetInterval(), vAxis.majorGrid.GetIntervalType());
                                }
                                else
                                {
                                    interval = ChartHelper.GetIntervalSize(vAxis.minimum, vAxis.Interval, vAxis.IntervalType);
                                }
                            }
                            else
                            {
                                interval = area.GetPointsInterval(vAxis.IsLogarithmic, vAxis.logarithmBase);
                            }
						}

						// Calculates the width of bars.
						double width = ser.GetPointWidth(graph, vAxis, interval, 0.8);

	
						// Adjust width by number of stacked groups
						width = width / (double)this.stackGroupNames.Count;
	

						// Call Back Paint event
						if( !selection )
						{
                            common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
						}
						
						// Change Y value if Bar is out of plot area
						double	yValue = GetYValue(common, area, ser, point, pointIndx, 0);
						if( seriesIndx != 0 )
						{
							if( yValue >= 0 )
							{
								yValue = yValue + PreviousPosY;
							}
							else
							{
								yValue = yValue + PreviousNegY;
							}
						}

                        // Check if scrolling/zooming frames cutting mode is enabled
                        bool ajaxScrollingEnabled = false;

                        // Save original Y Value
						double	originalYValue = yValue;
						
						// Axis is logarithmic
						if( hAxis.IsLogarithmic )
						{
							yValue = Math.Log( yValue, hAxis.logarithmBase );
						}

						// Recalculates Height position and zero position of bars
						double height = hAxis.GetLinearPosition( yValue );

						// Set x position
						double	xValue = point.XValue;
						if( indexedSeries )
						{
							// The formula for position is based on a distance 
							//from the grid line or nPoints position.
							xValue = (double)pointIndx + 1;
						}
						double xPosition = vAxis.GetPosition( xValue );
	
						// Adjust X position of each stack group
						if(this.stackGroupNames.Count > 1)
						{
							xPosition = xPosition - width * ((double) this.stackGroupNames.Count) / 2.0 + width / 2.0 + groupIndex * width;
						}
	

						xValue = vAxis.GetLogValue(xValue);


						// Set Start position for a bar
						double	barZeroValue;
						if( seriesIndx == 0 )
						{
                            if (ajaxScrollingEnabled && labels)
                            {
                                // If AJAX scrolling is used always use 0.0 as a starting point
                                barZeroValue = 0.0;
                            }
                            else
                            {
                                // Set Start position for a Column
                                barZeroValue = hAxis.Crossing;
                            }
						}
						else if( GetYValue(common, area, ser, point, pointIndx, 0) >= 0 )
						{
							barZeroValue = PreviousPosY;
						}
						else
						{
							barZeroValue = PreviousNegY;
						}
						double zero = hAxis.GetPosition(barZeroValue);

						// Calculate bar position
						RectangleF	rectSize = RectangleF.Empty;
						try
						{
							// Set the bar rectangle
							rectSize.Y = (float)(xPosition - width/2);
							rectSize.Height = (float)(width);

							// The left side of rectangle has always 
							// smaller value than a right value
							if( zero < height )
							{
								rectSize.X = (float)zero;
								rectSize.Width = (float)height - rectSize.X;
							}
							else
							{
								rectSize.X = (float)height;
								rectSize.Width = (float)zero - rectSize.X;
							}
						}
						catch(OverflowException)
						{
							continue;
						}

						// Remeber pre-calculated point position
						point.positionRel = new PointF(rectSize.Right, (float)xPosition);


						// if data point is not empty
						if( point.IsEmpty )
						{
							continue;
						}

						// Axis is logarithmic
						if( hAxis.IsLogarithmic )
						{
							barZeroValue = Math.Log( barZeroValue, hAxis.logarithmBase );
						}
						
						// Check if column is completly out of the data scaleView
						bool skipPoint = false;
						if(xValue < vAxis.ViewMinimum || 
							xValue > vAxis.ViewMaximum ||
							(yValue < hAxis.ViewMinimum && barZeroValue < hAxis.ViewMinimum) ||
							(yValue > hAxis.ViewMaximum && barZeroValue > hAxis.ViewMaximum) )
						{
							skipPoint = true;
						}

						// ***************************************************
						// Painting mode
						// ***************************************************
						if(!skipPoint)
						{
							if( common.ProcessModePaint )
							{
								// Check if column is partialy in the data scaleView
								bool	clipRegionSet = false;
								if(rectSize.Y < area.PlotAreaPosition.Y || 
									rectSize.Bottom > area.PlotAreaPosition.Bottom ||
									rectSize.X < area.PlotAreaPosition.X || 
									rectSize.Right > area.PlotAreaPosition.Right)
								{
									// Set clipping region for line drawing 
									graph.SetClip( area.PlotAreaPosition.ToRectangleF() );
									clipRegionSet = true;
								}

								// Set shadow
								int shadowOffset = 0;
								if( shadow )
								{
									shadowOffset = ser.ShadowOffset;
								}

								if( !labels )
								{
									// Start Svg Selection mode
									graph.StartHotRegion( point );

									// Draw the bar rectangle
									graph.FillRectangleRel( rectSize, 
										(!shadow)? point.Color : Color.Transparent, 
										point.BackHatchStyle, 
										point.BackImage, 
										point.BackImageWrapMode, 
										point.BackImageTransparentColor,
										point.BackImageAlignment,
										point.BackGradientStyle, 
										(!shadow)? point.BackSecondaryColor : Color.Transparent, 
										point.BorderColor, 
										point.BorderWidth, 
										point.BorderDashStyle, 
										ser.ShadowColor, 
										shadowOffset,
										PenAlignment.Inset,
										(shadow) ? BarDrawingStyle.Default : ChartGraphics.GetBarDrawingStyle(point),
										false);

									// End Svg Selection mode
									graph.EndHotRegion( );
								}

									// Draw labels 
								else
								{
									// Calculate label rectangle 
									RectangleF labelRect = new RectangleF(rectSize.Location, rectSize.Size);
                                    if (clipRegionSet && !ajaxScrollingEnabled)
									{
										labelRect.Intersect(area.PlotAreaPosition.ToRectangleF());
									}

									// Draw Labels
									DrawLabels( common, graph, area, point, pointIndx, ser, labelRect );
								}

								// Reset Clip Region
								if(clipRegionSet)
								{
									graph.ResetClip();
								}
							}

							// ***************************************************
							// Hot Regions Mode
							// ***************************************************
							if( common.ProcessModeRegions && !shadow && !labels)
							{
								common.HotRegionsList.AddHotRegion( rectSize, point, ser.Name, pointIndx );

								// Process labels and markers regions only if it was not done while painting
								if(labels && !common.ProcessModePaint)
								{
									DrawLabels( common, graph, area, point, pointIndx, ser, rectSize );
								}
							}
											
							// Call Paint event
							if( !selection )
							{
                                common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
							}
						}

						// Axis is logarithmic
						if( hAxis.IsLogarithmic )
						{
							yValue = Math.Pow( hAxis.logarithmBase, yValue );
						}

						// Data series index
						seriesIndx++;
						if( GetYValue(common, area, ser, point, pointIndx, 0) >= 0 )
						{
							PreviousPosY = originalYValue;
						}
						else
						{
							PreviousNegY = originalYValue;
						}
					}

				}

			}
		


			//************************************************************
			//** Remove stacked groups created for series attached to different axis
			//************************************************************

			if(differentAxesAreUsed)
			{
				for(int seriesIndex = 0; seriesIndex < common.DataManager.Series.Count; seriesIndex++)
				{
					// Process non empty series of the area with stacked column chart type
					Series ser = common.DataManager.Series[seriesIndex];
					if( String.Compare( ser.ChartTypeName, Name, StringComparison.OrdinalIgnoreCase ) != 0 
						|| ser.ChartArea != area.Name || !ser.IsVisible())
					{
						continue;
					}

					// Set new group name
					string stackGroupName = StackedColumnChart.GetSeriesStackGroupName(ser);
					int index = stackGroupName.IndexOf("__", StringComparison.Ordinal);
					if(index >= 0)
					{
						stackGroupName = stackGroupName.Substring(index + 2);
					}
					if(stackGroupName.Length > 0)
					{
						ser[CustomPropertyName.StackedGroupName] = stackGroupName;
					}
					else
					{
						ser.DeleteCustomProperty(CustomPropertyName.StackedGroupName);
					}
				}
			}


		
		}

		/// <summary>
		/// Draw Stacked Column labels.
		/// </summary>
		/// <param name="common">Chart common elements.</param>
		/// <param name="graph">Chart Graphics.</param>
		/// <param name="area">Chart area the series belongs to.</param>
		/// <param name="point">Data point.</param>
		/// <param name="pointIndex">Data point index.</param>
		/// <param name="series">Data series.</param>
		/// <param name="rectangle">Column rectangle.</param>
		public void DrawLabels(
			CommonElements common, 
			ChartGraphics graph, 
			ChartArea area, 
			DataPoint point, 
			int pointIndex, 
			Series series, 
			RectangleF rectangle )
		{
			// Label text format
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                // Disable the clip region
                Region oldClipRegion = graph.Clip;
                graph.Clip = new Region();

                if (point.IsValueShownAsLabel || point.Label.Length > 0)
                {
                    // Round Y values for 100% stacked bar
                    double pointLabelValue = GetYValue(common, area, series, point, pointIndex, 0);
                    if (this.hundredPercentStacked && point.LabelFormat.Length == 0)
                    {
                        pointLabelValue = Math.Round(pointLabelValue, 2);
                    }

                    // Get label text
                    string text;
                    if (point.Label.Length == 0)
                    {
                        text = ValueConverter.FormatValue(
                            series.Chart,
                            point,
                            point.Tag,
                            pointLabelValue,
                            point.LabelFormat,
                            series.YValueType,
                            ChartElementType.DataPoint);
                    }
                    else
                    {
                        text = point.ReplaceKeywords(point.Label);
                    }

                    // Calculate position
                    PointF labelPosition = PointF.Empty;
                    labelPosition.X = rectangle.X + rectangle.Width / 2f;
                    labelPosition.Y = rectangle.Y + rectangle.Height / 2f;

                    // Get text angle
                    int textAngle = point.LabelAngle;

                    // Check if text contains white space only
                    if (text.Trim().Length != 0)
                    {
                        //************************************************************
                        // Measure string
                        //************************************************************
                        SizeF sizeFont = graph.GetRelativeSize(
                            graph.MeasureString(
                            text,
                            point.Font,
                            new SizeF(1000f, 1000f),
                            StringFormat.GenericTypographic));

                        //************************************************************
                        // Check labels style custom properties 
                        //************************************************************
                        BarValueLabelDrawingStyle drawingStyle = BarValueLabelDrawingStyle.Center;
                        string valueLabelAttrib = "";
                        if (point.IsCustomPropertySet(CustomPropertyName.BarLabelStyle))
                        {
                            valueLabelAttrib = point[CustomPropertyName.BarLabelStyle];
                        }
                        else if (series.IsCustomPropertySet(CustomPropertyName.BarLabelStyle))
                        {
                            valueLabelAttrib = series[CustomPropertyName.BarLabelStyle];
                        }

                        if (valueLabelAttrib != null && valueLabelAttrib.Length > 0)
                        {
                            if (String.Compare(valueLabelAttrib, "Left", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Left;
                            else if (String.Compare(valueLabelAttrib, "Right", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Right;
                            else if (String.Compare(valueLabelAttrib, "Center", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Center;
                            else if (String.Compare(valueLabelAttrib, "Outside", StringComparison.OrdinalIgnoreCase) == 0)
                                drawingStyle = BarValueLabelDrawingStyle.Outside;
                        }

                        //************************************************************
                        // Adjust label position based on the label drawing style
                        //************************************************************
                        if (drawingStyle == BarValueLabelDrawingStyle.Left)
                        {
                            labelPosition.X = rectangle.X + sizeFont.Width / 2f;
                        }
                        else if (drawingStyle == BarValueLabelDrawingStyle.Right)
                        {
                            labelPosition.X = rectangle.Right - sizeFont.Width / 2f;
                        }
                        else if (drawingStyle == BarValueLabelDrawingStyle.Outside)
                        {
                            labelPosition.X = rectangle.Right + sizeFont.Width / 2f;
                        }


                        // Check if Smart Labels are enabled
                        if (series.SmartLabelStyle.Enabled)
                        {
                            // Force some SmartLabelStyle settings for column chart
                            bool oldMarkerOverlapping = series.SmartLabelStyle.IsMarkerOverlappingAllowed;
                            LabelAlignmentStyles oldMovingDirection = series.SmartLabelStyle.MovingDirection;
                            series.SmartLabelStyle.IsMarkerOverlappingAllowed = true;
                            if (series.SmartLabelStyle.MovingDirection == (LabelAlignmentStyles.Top | LabelAlignmentStyles.Bottom | LabelAlignmentStyles.Right | LabelAlignmentStyles.Left | LabelAlignmentStyles.TopLeft | LabelAlignmentStyles.TopRight | LabelAlignmentStyles.BottomLeft | LabelAlignmentStyles.BottomRight))
                            {
                                series.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Left | LabelAlignmentStyles.Right;
                            }

                            // Adjust label position using SmartLabelStyle algorithm
                            labelPosition = area.smartLabels.AdjustSmartLabelPosition(
                                common,
                                graph,
                                area,
                                series.SmartLabelStyle,
                                labelPosition,
                                sizeFont,
                                format,
                                labelPosition,
                                new SizeF(0f, 0f),
                                LabelAlignmentStyles.Center);

                            // Restore forced values
                            series.SmartLabelStyle.IsMarkerOverlappingAllowed = oldMarkerOverlapping;
                            series.SmartLabelStyle.MovingDirection = oldMovingDirection;

                            // Smart labels always use 0 degrees text angle
                            textAngle = 0;
                        }



                        // Draw label
                        if (!labelPosition.IsEmpty)
                        {
                            // Get label background position
                            RectangleF labelBackPosition = RectangleF.Empty;
                            SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
                            sizeLabel.Height += sizeFont.Height / 8;
                            sizeLabel.Width += sizeLabel.Width / text.Length;
                            labelBackPosition = new RectangleF(
                                labelPosition.X - sizeLabel.Width / 2,
                                labelPosition.Y - sizeLabel.Height / 2 - sizeFont.Height / 10,
                                sizeLabel.Width,
                                sizeLabel.Height);



                            // Adjust label background position that can be changed by the 
                            // Smart Labels algorithm
                            // NOTE: Fixes issue #4688
                            labelBackPosition = area.smartLabels.GetLabelPosition(
                                graph,
                                labelPosition,
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
                                    labelPosition,
                                    format,
                                    textAngle,
                                    labelBackPosition,
                                    point.LabelBackColor,
                                    point.LabelBorderColor,
                                    point.LabelBorderWidth,
                                    point.LabelBorderDashStyle,
                                    series,
                                    point,
                                    pointIndex);
                            }
                        }

                    }
                }

                // Restore old clip region
                graph.Clip = oldClipRegion;
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
		/// <param name="yValueIndex">Index of the Y value to get. Set to -1 to get the height.</param>
		/// <returns>Y value of the point.</returns>
		virtual public double GetYValue(CommonElements common, ChartArea area, Series series, DataPoint point, int pointIndex, int yValueIndex)
		{
			double	yValue = double.NaN;

			// Calculate stacked column Y value for 2D chart
			if(area.Area3DStyle.Enable3D == false || yValueIndex == -2)
			{
				return point.YValues[0];
			}

			// Get point Height if pointIndex == -1
			if(yValueIndex == -1)
			{
				Axis	vAxis = area.GetAxis(AxisName.Y, series.YAxisType, series.YSubAxisName);
				double	barZeroValue = vAxis.Crossing;
				yValue = GetYValue(common, area, series, point, pointIndex, 0);
				if( yValue >= 0 )
				{
					if(!double.IsNaN(prevPosY))
					{
						barZeroValue = prevPosY;
					}
				}
				else
				{
					if(!double.IsNaN(prevNegY))
					{
						barZeroValue = prevNegY;
					}
				}

				return yValue - barZeroValue;
			}

			// Loop through all series
			prevPosY = double.NaN;
			prevNegY = double.NaN;
			foreach(Series ser in common.DataManager.Series)
			{
				// Check series of the current chart type & area
				if(String.Compare(series.ChartArea, ser.ChartArea, StringComparison.Ordinal) == 0 &&
                    String.Compare(series.ChartTypeName, ser.ChartTypeName, StringComparison.OrdinalIgnoreCase) == 0 && 
					ser.IsVisible())
				{

					// Check if series belongs to the current group name
					string seriesStackGroupName = StackedColumnChart.GetSeriesStackGroupName(ser);
					if(this.stackGroupNameUsed && 
						seriesStackGroupName != this.currentStackGroup)
					{
						continue;
					}



					if(double.IsNaN(yValue))
					{
						yValue = ser.Points[pointIndex].YValues[0];
					}
					else
					{
						yValue = ser.Points[pointIndex].YValues[0];
						if(yValue >= 0.0 && !double.IsNaN(prevPosY))
						{
							yValue += prevPosY;
						}
						if(yValue < 0.0 && !double.IsNaN(prevNegY))
						{
							yValue += prevNegY;
						}
					}

					// Exit loop when current series was found
                    if (String.Compare(series.Name, ser.Name, StringComparison.Ordinal) == 0)
					{
						break;
					}

					// Save previous value
					if(yValue >= 0.0)
					{
						prevPosY = yValue;
					}
					if(yValue < 0.0)
					{
						prevNegY = yValue;
					}
				}
			}
			
			return yValue;
		}

		#endregion

		#region 3D Painting and selection methods

		/// <summary>
		/// This method recalculates size of the stacked bars in 3D space. This method is used 
		/// from Paint or Select method.
		/// </summary>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="drawLabels">True if labels must be drawn.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		private void ProcessChartType3D( 
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			bool drawLabels, 
			Series seriesToDraw )
		{

			// Get list of series to draw
			List<string> typeSeries = null;


			// Get all series names that belong the same cluster
			typeSeries = area.GetClusterSeriesNames(seriesToDraw.Name);


			//************************************************************
			//** Get order of data points drawing
			//************************************************************
			ArrayList	dataPointDrawingOrder = area.GetDataPointDrawingOrder(
				typeSeries, 
				this, 
				selection, 
				COPCoordinates.X | COPCoordinates.Y, 
				new BarPointsDrawingOrderComparer(area, selection, COPCoordinates.X | COPCoordinates.Y),
				0,
				false);


			//************************************************************
			//** Loop through all data poins and draw them
			//************************************************************
			if(!drawLabels)
			{
				foreach(object obj in dataPointDrawingOrder)
				{
					// Get point & series
					DataPoint3D	pointEx = (DataPoint3D) obj;
					DataPoint	point = pointEx.dataPoint;
					Series		ser = point.series;


					// Set current stack group name
					this.currentStackGroup = StackedColumnChart.GetSeriesStackGroupName(ser);


					// Reset pre-calculated point position
					point.positionRel = new PointF(float.NaN, float.NaN);

					// Set active horizontal/vertical axis
					Axis	vAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
					Axis	hAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

					// Get point bar drawing style
					BarDrawingStyle	barDrawingStyle = ChartGraphics.GetBarDrawingStyle(point);
		
					// All cut points are darkened except of the first and last series
					float	rightDarkening = 0.5f;
					float	leftDarkening = 0.5f;

					// NOTE: Following code was replaced with the code below to fix issue #5391
//					if((string)typeSeries[typeSeries.Count - 1] == ser.Name)
//					{
//						leftDarkening = 0f;
//					}
//					if((string)typeSeries[0] == ser.Name)
//					{
//						rightDarkening = 0f;
//					}
					bool	firstVisibleSeries = true;
					bool	lastVisibleSeries = false;
					for(int seriesIndex = 0; seriesIndex < typeSeries.Count; seriesIndex++)
					{
						// Get series object
						Series currentSeries = common.DataManager.Series[seriesIndex];

						// Check if it is a first series with non-zero Y value
						if(firstVisibleSeries)
						{
							// Make series has non zero vallue
							if(pointEx.index <= currentSeries.Points.Count &&
								currentSeries.Points[pointEx.index - 1].YValues[0] != 0.0)
							{
								firstVisibleSeries = false;
								if(currentSeries.Name == ser.Name)
								{
									rightDarkening = 0f;
								}
							}
						}

						// Check if it is a last series with non-zero Y value
						if(currentSeries.Name == ser.Name)
						{
							lastVisibleSeries = true;
						}
						else if(pointEx.index <= currentSeries.Points.Count &&
							currentSeries.Points[pointEx.index - 1].YValues[0] != 0.0)
						{
							lastVisibleSeries = false;
						}
					}

					// Remove darkenning from the last series in the group
					if(lastVisibleSeries)
					{
						leftDarkening = 0f;
					}


					// If stacked groups are used remove darkenning from the
					// first/last series in the group
                    if (area.StackGroupNames != null &&
                        area.StackGroupNames.Count > 1 &&
						area.Area3DStyle.IsClustered)
					{
						// Get series group name
						string groupName = StackedColumnChart.GetSeriesStackGroupName(ser);
					
						// Iterate through all series in the group
						bool	firstSeries = true;
						bool	lastSeries = false;
						foreach(string seriesName in typeSeries)
						{
							Series currentSeries = common.DataManager.Series[seriesName];
							if(StackedColumnChart.GetSeriesStackGroupName(currentSeries) == groupName)
							{
								// check if first seris
								if(firstSeries)
								{
									// Make series has non zero vallue
									if(pointEx.index < currentSeries.Points.Count &&
										currentSeries.Points[pointEx.index - 1].YValues[0] != 0.0)
									{
										firstSeries = false;
										if(seriesName == ser.Name)
										{
											rightDarkening = 0f;
										}
									}
								}

								// check if last series
								if(seriesName == ser.Name)
								{
									lastSeries = true;
								}
						 		else if(pointEx.index < currentSeries.Points.Count &&
									currentSeries.Points[pointEx.index - 1].YValues[0] != 0.0)
								{
									lastSeries = false;
								}
							}
						}

						// Remove darkenning from the last series in the group
						if(lastSeries)
						{
							leftDarkening = 0f;
						}
					}



					// Change Y value if Bar is out of plot area
					double	yValue = GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, 0);

					// Set Start position for a bar
					double	barZeroValue = yValue - GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, -1);

					// Convert values if logarithmic axis is used
					yValue = hAxis.GetLogValue(yValue);
					barZeroValue = hAxis.GetLogValue(barZeroValue);

					if( barZeroValue > hAxis.ViewMaximum )
					{
						leftDarkening = 0.5f;
						barZeroValue = hAxis.ViewMaximum;
					}
					else if( barZeroValue < hAxis.ViewMinimum )
					{
						rightDarkening = 0.5f;
						barZeroValue = hAxis.ViewMinimum;
					}
					if( yValue > hAxis.ViewMaximum )
					{
						leftDarkening = 0.5f;
						yValue = hAxis.ViewMaximum;
					}
					else if( yValue < hAxis.ViewMinimum )
					{
						rightDarkening = 0.5f;
						yValue = hAxis.ViewMinimum;
					}
				
					// Recalculates Height position and zero position of bars
					double	height = hAxis.GetLinearPosition(yValue);
					double	zero = hAxis.GetLinearPosition(barZeroValue);

					// Set x position
					double	xValue = (pointEx.indexedSeries) ? pointEx.index : point.XValue;
					xValue = vAxis.GetLogValue(xValue);


					// Calculate bar position
					RectangleF	rectSize = RectangleF.Empty;
					try
					{
						// Set the bar rectangle
						rectSize.Y = (float)(pointEx.xPosition - pointEx.width/2);
						rectSize.Height = (float)(pointEx.width);

						// The left side of rectangle has always 
						// smaller value than a right value
						if( zero < height )
						{
							float temp = leftDarkening;
							leftDarkening = rightDarkening;
							rightDarkening = temp;

							rectSize.X = (float)zero;
							rectSize.Width = (float)height - rectSize.X;
						}
						else
						{
							rectSize.X = (float)height;
							rectSize.Width = (float)zero - rectSize.X;
						}
					}
					catch(OverflowException)
					{
						continue;
					}

					// Remeber pre-calculated point position
					point.positionRel = new PointF(rectSize.Right, (float)pointEx.xPosition);

					// if data point is not empty
					if( point.IsEmpty )
					{
						continue;
					}

					GraphicsPath rectPath = null;
			
					// Check if column is completly out of the data scaleView
					if(xValue < vAxis.ViewMinimum || 
						xValue > vAxis.ViewMaximum ||
						(yValue < hAxis.ViewMinimum && barZeroValue < hAxis.ViewMinimum) ||
						(yValue > hAxis.ViewMaximum && barZeroValue > hAxis.ViewMaximum) )
					{
						continue;
					}

					// Check if column is partialy in the data scaleView
					bool	clipRegionSet = false;
					if(rectSize.Bottom <= area.PlotAreaPosition.Y || rectSize.Y >= area.PlotAreaPosition.Bottom)
					{
						continue;
					}
					if(rectSize.Y < area.PlotAreaPosition.Y)
					{
						rectSize.Height -= area.PlotAreaPosition.Y - rectSize.Y;
						rectSize.Y = area.PlotAreaPosition.Y;
					}
					if(rectSize.Bottom > area.PlotAreaPosition.Bottom)
					{
						rectSize.Height -= rectSize.Bottom - area.PlotAreaPosition.Bottom;
					}
					if(rectSize.Height < 0)
					{
						rectSize.Height = 0;
					}
					if(rectSize.Height == 0f || rectSize.Width == 0f)
					{
						continue;
					}


					// Detect if we need to get graphical path of drawn object
					DrawingOperationTypes	drawingOperationType = DrawingOperationTypes.DrawElement;

					if( common.ProcessModeRegions )
					{
						drawingOperationType |= DrawingOperationTypes.CalcElementPath;
					}

					// Start Svg Selection mode
					graph.StartHotRegion( point );

					// Draw the Bar rectangle
					rectPath = graph.Fill3DRectangle( 
						rectSize, 
						pointEx.zPosition,
						pointEx.depth,
						area.matrix3D,
						area.Area3DStyle.LightStyle,
						point.Color, 
						rightDarkening,
						leftDarkening,
						point.BorderColor, 
						point.BorderWidth, 
						point.BorderDashStyle, 
						barDrawingStyle,
						false,
						drawingOperationType);

					// End Svg Selection mode
					graph.EndHotRegion( );

					// Reset Clip Region
					if(clipRegionSet)
					{
						graph.ResetClip();
					}

					if( common.ProcessModeRegions && !drawLabels)
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
                    if (rectPath != null)
                    {
                        rectPath.Dispose();
                    }
				}
			}

			//************************************************************
			//** Loop through all data poins and draw labels
			//************************************************************
			if(drawLabels)
			{
				foreach(object obj in dataPointDrawingOrder)
				{
					// Get point & series
					DataPoint3D	pointEx = (DataPoint3D) obj;
					DataPoint	point = pointEx.dataPoint;
					Series		ser = point.series;

					// Set active horizontal/vertical axis
					Axis	vAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
					Axis	hAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);
		
					// Change Y value if Bar is out of plot area
					double	yValue = GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, 0);
					
					// Axis is logarithmic
					if( hAxis.IsLogarithmic )
					{
						yValue = Math.Log( yValue, hAxis.logarithmBase );
					}

					// Recalculates Height position and zero position of bars
					double height = pointEx.yPosition;;

					// Set x position
					double	xValue = (pointEx.indexedSeries) ? pointEx.index : point.XValue;

					// Set Start position for a bar
					double	barZeroValue = yValue - GetYValue(common, area, ser, pointEx.dataPoint, pointEx.index - 1, -1);
					double zero = pointEx.height;

					// Calculate bar position
					RectangleF	rectSize = RectangleF.Empty;
					try
					{
						// Set the bar rectangle
						rectSize.Y = (float)(pointEx.xPosition - pointEx.width/2);
						rectSize.Height = (float)(pointEx.width);

						// The left side of rectangle has always 
						// smaller value than a right value
						if( zero < height )
						{
							rectSize.X = (float)zero;
							rectSize.Width = (float)height - rectSize.X;
						}
						else
						{
							rectSize.X = (float)height;
							rectSize.Width = (float)zero - rectSize.X;
						}
					}
					catch(OverflowException)
					{
						continue;
					}

					// if data point is not empty
					if( point.IsEmpty )
					{
						continue;
					}
			
					// Axis is logarithmic
					if( hAxis.IsLogarithmic )
					{
						barZeroValue = Math.Log( barZeroValue, hAxis.logarithmBase );
					}

					// Check if column is completly out of the data scaleView
					if(xValue < vAxis.ViewMinimum || 
						xValue > vAxis.ViewMaximum ||
						(yValue < hAxis.ViewMinimum && barZeroValue < hAxis.ViewMinimum) ||
						(yValue > hAxis.ViewMaximum && barZeroValue > hAxis.ViewMaximum) )
					{
						continue;
					}

					// Draw 3D labels
					DrawLabels3D( area, graph, common, rectSize, pointEx, ser, barZeroValue, height, pointEx.width, pointEx.index - 1);
				}	
			}
		}

		/// <summary>
		/// Draws labels in 3D.
		/// </summary>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="rectSize">Bar rectangle.</param>
		/// <param name="pointEx">Data point.</param>
		/// <param name="ser">Data series.</param>
		/// <param name="barStartPosition">The zero position or the bottom of bars.</param>
		/// <param name="barSize">The Height of bars.</param>
		/// <param name="width">The width of bars.</param>
		/// <param name="pointIndex">Point index.</param>
		private void DrawLabels3D( 
			ChartArea area,
			ChartGraphics graph, 
			CommonElements common, 
			RectangleF rectSize, 
			DataPoint3D pointEx, 
			Series ser, 
			double barStartPosition, 
			double barSize, 
			double width, 
			int pointIndex)
		{
			DataPoint point = pointEx.dataPoint;

			//************************************************************
			// Draw data point value label
			//************************************************************
			if(ser.IsValueShownAsLabel || point.IsValueShownAsLabel || point.Label.Length > 0)
			{
				// Label rectangle
				RectangleF rectLabel = RectangleF.Empty;

				// Label text format
                using (StringFormat format = new StringFormat())
                {

                    //************************************************************
                    // Get label text 
                    //************************************************************
                    string text;
                    if (point.Label.Length == 0)
                    {
                        // Round Y values for 100% stacked bar
                        double pointLabelValue = GetYValue(common, area, ser, point, pointIndex, -2);
                        if (this.hundredPercentStacked && point.LabelFormat.Length == 0)
                        {
                            pointLabelValue = Math.Round(pointLabelValue, 2);
                        }

                        text = ValueConverter.FormatValue(
                            ser.Chart,
                            point,
                            point.Tag,
                            pointLabelValue,
                            point.LabelFormat,
                            ser.YValueType,
                            ChartElementType.DataPoint);
                    }
                    else
                    {
                        text = point.ReplaceKeywords(point.Label);
                    }

				
                    //************************************************************
                    // Check labels style custom properties 
                    //************************************************************
                    BarValueLabelDrawingStyle drawingStyle = BarValueLabelDrawingStyle.Center;
                    string valueLabelAttrib = "";
                    if (point.IsCustomPropertySet(CustomPropertyName.BarLabelStyle))
                    {
                        valueLabelAttrib = point[CustomPropertyName.BarLabelStyle];
                    }
                    else if (ser.IsCustomPropertySet(CustomPropertyName.BarLabelStyle))
                    {
                        valueLabelAttrib = ser[CustomPropertyName.BarLabelStyle];
                    }

                    if (valueLabelAttrib != null && valueLabelAttrib.Length > 0)
                    {
                        if (String.Compare(valueLabelAttrib, "Left", StringComparison.OrdinalIgnoreCase) == 0)
                            drawingStyle = BarValueLabelDrawingStyle.Left;
                        else if (String.Compare(valueLabelAttrib, "Right", StringComparison.OrdinalIgnoreCase) == 0)
                            drawingStyle = BarValueLabelDrawingStyle.Right;
                        else if (String.Compare(valueLabelAttrib, "Center", StringComparison.OrdinalIgnoreCase) == 0)
                            drawingStyle = BarValueLabelDrawingStyle.Center;
                        else if (String.Compare(valueLabelAttrib, "Outside", StringComparison.OrdinalIgnoreCase) == 0)
                            drawingStyle = BarValueLabelDrawingStyle.Outside;
                    }

                    //************************************************************
                    // Make sure label fits. Otherwise change it style
                    //************************************************************
                    bool labelFit = false;
                    while (!labelFit)
                    {
                        // Label text format
                        format.Alignment = StringAlignment.Near;
                        format.LineAlignment = StringAlignment.Center;

                        // LabelStyle rectangle
                        if (barStartPosition < barSize)
                        {
                            rectLabel.X = rectSize.Right;
                            rectLabel.Width = area.PlotAreaPosition.Right - rectSize.Right;
                        }
                        else
                        {
                            rectLabel.X = area.PlotAreaPosition.X;
                            rectLabel.Width = rectSize.X - area.PlotAreaPosition.X;
                        }

                        // Adjust label rectangle
                        rectLabel.Y = rectSize.Y - (float)width / 2F;
                        rectLabel.Height = rectSize.Height + (float)width;

                        // Adjust label position depending on the drawing style
                        if (drawingStyle == BarValueLabelDrawingStyle.Left)
                        {
                            rectLabel = rectSize;
                            format.Alignment = StringAlignment.Near;
                        }
                        else if (drawingStyle == BarValueLabelDrawingStyle.Center)
                        {
                            rectLabel = rectSize;
                            format.Alignment = StringAlignment.Center;
                        }
                        else if (drawingStyle == BarValueLabelDrawingStyle.Right)
                        {
                            rectLabel = rectSize;
                            format.Alignment = StringAlignment.Far;
                        }

                        // Reversed string alignment
                        if (barStartPosition >= barSize)
                        {
                            if (format.Alignment == StringAlignment.Far)
                                format.Alignment = StringAlignment.Near;
                            else if (format.Alignment == StringAlignment.Near)
                                format.Alignment = StringAlignment.Far;
                        }

                        // Stacked bar chart can not change the BarValueLabelDrawingStyle trying to
                        // fit data point labels because it will cause label overlapping.
                        // NOTE: Code below is commented. Fixes issue #4687 - AG
                        labelFit = true;

                        //					// Make sure value label fits rectangle. 
                        //					SizeF valueTextSize = graph.MeasureStringRel(text, point.Font);
                        //					if(!labelSwitched && valueTextSize.Width > rectLabel.Width)
                        //					{
                        //						// Switch label style only once
                        //						labelSwitched = true;
                        //
                        //						// If text do not fit - try to switch between Outside/Inside drawing styles
                        //						if(drawingStyle == BarValueLabelDrawingStyle.Outside)
                        //						{
                        //							drawingStyle = BarValueLabelDrawingStyle.Right;
                        //						}
                        //						else
                        //						{
                        //							drawingStyle = BarValueLabelDrawingStyle.Outside;
                        //						}
                        //					}
                        //					else
                        //					{
                        //						labelFit = true;
                        //					}
                    }

                    //************************************************************
                    // Find text rotation center point
                    //************************************************************

                    // Measure string size
                    SizeF size = graph.MeasureStringRel(text, point.Font, new SizeF(rectLabel.Width, rectLabel.Height), format);

                    PointF rotationCenter = PointF.Empty;
                    if (format.Alignment == StringAlignment.Near)
                    { // Near
                        rotationCenter.X = rectLabel.X + size.Width / 2;
                    }
                    else if (format.Alignment == StringAlignment.Far)
                    { // Far
                        rotationCenter.X = rectLabel.Right - size.Width / 2;
                    }
                    else
                    { // Center
                        rotationCenter.X = (rectLabel.Left + rectLabel.Right) / 2;
                    }

                    if (format.LineAlignment == StringAlignment.Near)
                    { // Near
                        rotationCenter.Y = rectLabel.Top + size.Height / 2;
                    }
                    else if (format.LineAlignment == StringAlignment.Far)
                    { // Far
                        rotationCenter.Y = rectLabel.Bottom - size.Height / 2;
                    }
                    else
                    { // Center
                        rotationCenter.Y = (rectLabel.Bottom + rectLabel.Top) / 2;
                    }

                    // Reset string alignment to center point
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;


                    //************************************************************
                    // Adjust label rotation angle
                    //************************************************************
                    int angle = point.LabelAngle;

                    // Get projection coordinates
                    Point3D[] rotationCenterProjection = new Point3D[] { 
																		 new Point3D(rotationCenter.X, rotationCenter.Y, pointEx.zPosition + pointEx.depth),
																		 new Point3D(rotationCenter.X - 20f, rotationCenter.Y, pointEx.zPosition + pointEx.depth) };
                    // Transform coordinates of text rotation point
                    area.matrix3D.TransformPoints(rotationCenterProjection);

                    // Adjust rotation point
                    rotationCenter = rotationCenterProjection[0].PointF;

                    // Adjust angle of the horisontal text
                    if (angle == 0 || angle == 180)
                    {
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

                    SizeF sizeFont = SizeF.Empty;


                    // Check if Smart Labels are enabled
                    if (ser.SmartLabelStyle.Enabled)
                    {
                        sizeFont = graph.GetRelativeSize(
                            graph.MeasureString(
                            text,
                            point.Font,
                            new SizeF(1000f, 1000f),
                            StringFormat.GenericTypographic));

					// Force some SmartLabelStyle settings for column chart
					bool oldMarkerOverlapping = ser.SmartLabelStyle.IsMarkerOverlappingAllowed;
					LabelAlignmentStyles oldMovingDirection = ser.SmartLabelStyle.MovingDirection;
					ser.SmartLabelStyle.IsMarkerOverlappingAllowed = true;
					if(ser.SmartLabelStyle.MovingDirection == (LabelAlignmentStyles.Top | LabelAlignmentStyles.Bottom | LabelAlignmentStyles.Right | LabelAlignmentStyles.Left | LabelAlignmentStyles.TopLeft | LabelAlignmentStyles.TopRight | LabelAlignmentStyles.BottomLeft | LabelAlignmentStyles.BottomRight) )
					{
						ser.SmartLabelStyle.MovingDirection = LabelAlignmentStyles.Left | LabelAlignmentStyles.Right;
					}

                        // Adjust label position using SmartLabelStyle algorithm
                        rotationCenter = area.smartLabels.AdjustSmartLabelPosition(
                            common,
                            graph,
                            area,
                            ser.SmartLabelStyle,
                            rotationCenter,
                            sizeFont,
                            format,
                            rotationCenter,
                            new SizeF(0f, 0f),
                            LabelAlignmentStyles.Center);

					// Restore forced values
					ser.SmartLabelStyle.IsMarkerOverlappingAllowed = oldMarkerOverlapping;
					ser.SmartLabelStyle.MovingDirection = oldMovingDirection;

                        // Smart labels always use 0 degrees text angle
                        angle = 0;
                    }





                    //************************************************************
                    // Draw label
                    //************************************************************
                    if (!rotationCenter.IsEmpty)
                    {
                        // Measure string
                        if (sizeFont.IsEmpty)
                        {
                            sizeFont = graph.GetRelativeSize(
                                graph.MeasureString(
                                text,
                                point.Font,
                                new SizeF(1000f, 1000f),
                                new StringFormat(StringFormat.GenericTypographic)));
                        }

                        // Get label background position
                        RectangleF labelBackPosition = RectangleF.Empty;
                        SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
                        sizeLabel.Height += sizeFont.Height / 8;
                        sizeLabel.Width += sizeLabel.Width / text.Length;
                        labelBackPosition = new RectangleF(
                            rotationCenter.X - sizeLabel.Width / 2,
                            rotationCenter.Y - sizeLabel.Height / 2 - sizeFont.Height / 10,
                            sizeLabel.Width,
                            sizeLabel.Height);

                        // Draw label text
                        using (Brush brush = new SolidBrush(point.LabelForeColor))
                        {
                            graph.DrawPointLabelStringRel(
                                common,
                                text,
                                point.Font,
                                brush,
                                rotationCenter,
                                format,
                                angle,
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
            // NOTE: Stacked Bar chart type do not support SmartLabelStyle feature
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
}
