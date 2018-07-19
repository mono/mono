//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		StackedAreaChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	StackedAreaChart, HundredPercentStackedAreaChart
//
//  Purpose:	Stacked area and hundred percent stacked area charts.
//
//	Reviewed:	AG - Aug 6, 2002
//              AG - Microsoft 7, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Resources;
using System.Reflection;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;

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
    /// HundredPercentStackedAreaChart class extends StackedAreaChart class
    /// by providing its own algorithm for calculating series data point
    /// Y values. It makes sure that total Y value of all data points in a
    /// single cluster from all series adds up to 100%.
	/// </summary>
	internal class HundredPercentStackedAreaChart : StackedAreaChart
	{
		#region Constructor

		/// <summary>
		/// Default constructor.
		/// </summary>
		public HundredPercentStackedAreaChart()
		{
			hundredPercentStacked = true;
		}

		#endregion 

		#region Fields

		// Array of total points values
		double[]		_totalPerPoint = null;
        int             _seriesCount   = -1;
		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		override public string Name			{ get{ return ChartTypeNames.OneHundredPercentStackedArea;}}

		/// <summary>
		/// Indicates that it's a hundredred percent chart.
		/// Axis scale from 0 to 100 percent should be used.
		/// </summary>
		override public bool HundredPercent{ get{return true;} }

		#endregion

		#region Painting and Selection methods

		/// <summary>
        /// Paint HundredPercentStackedAreaChart Chart
		/// </summary>
		/// <param name="graph">The Chart Graphics object</param>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		override public void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw )
		{
            this.Common = common;
            // Reset total per point value
			_totalPerPoint = null;
            _seriesCount = -1;
            // Call base class implementation
			base.Paint( graph, common, area, seriesToDraw );
		}

		#endregion

		#region Y values related methods
        /// <summary>
        /// Returns series count of same type for given chart area.
        /// </summary>
        /// <param name="common">The common elements</param>
        /// <param name="area">The chart area to inspect</param>
        /// <returns>Series count of same type</returns>
        private int GetSeriesCount(CommonElements common, ChartArea area)
        {
            if (_seriesCount == -1)
            {
                // Get number of series
                int seriesCount = 0;
                foreach (Series ser in common.DataManager.Series)
                {
                    // Use series of the same type which belong to this area 
                    if (String.Compare(ser.ChartTypeName, Name, true, System.Globalization.CultureInfo.CurrentCulture) == 0
                        && ser.ChartArea == area.Name && ser.IsVisible())
                    {
                        ++seriesCount;
                    }
                }
                _seriesCount = seriesCount;
            }
            return _seriesCount;
        }

		/// <summary>
		/// Helper function, which returns the Y value of the point
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

            // Calculate the totals of all Y values for all series
			if(_totalPerPoint == null)
			{
                // Get number of series
                int seriesCount = GetSeriesCount(common, area);
				// Fill array of series with this type, which are drawn on this area
				Series[]	seriesArray = new Series[seriesCount];
				int			seriesIndex = 0;
				foreach( Series ser in common.DataManager.Series )
				{
					// Use series of the same type which belong to this area 
					if( String.Compare( ser.ChartTypeName, Name, true, System.Globalization.CultureInfo.CurrentCulture ) == 0 
						&& ser.ChartArea == area.Name && ser.IsVisible())
					{
						seriesArray[seriesIndex++] = ser;
					}
				}
				
				// Check if series are aligned
				common.DataManipulator.CheckXValuesAlignment(seriesArray);

				// Allocate memory for the array
				_totalPerPoint = new double[series.Points.Count];

				// Calculate the total of Y value per point 
				for(int index = 0; index < series.Points.Count; index++)
				{
					_totalPerPoint[index] = 0;
					foreach( Series ser in seriesArray )
					{
						_totalPerPoint[index] += Math.Abs(ser.Points[index].YValues[0]);
					}
				}
			}

			// NOTE: In stacked area chart we need to do processing even if Y value is not set
//			if(point.YValues[0] == 0 || point.IsEmpty)
//			{
//				return 0;
//			}

			// Calculate stacked area Y value for 2D chart
			if(area.Area3DStyle.Enable3D == false)
			{
                if (_totalPerPoint[pointIndex] == 0)
                {
                    // Get number of series
                    int seriesCount = GetSeriesCount(common, area);
                    return 100.0 / seriesCount;
                }
				return (point.YValues[0] / _totalPerPoint[pointIndex]) * 100.0;
			}

			// Get point Height if pointIndex == -1
			double yValue = double.NaN;
			if(yValueIndex == -1)
			{
				Axis	vAxis = area.GetAxis(AxisName.Y, series.YAxisType, series.YSubAxisName);
				double	areaZeroValue = vAxis.Crossing;
				yValue = GetYValue(common, area, series, point, pointIndex, 0);
				if(area.Area3DStyle.Enable3D && yValue < 0.0)
				{
					// No negative values support in 3D stacked area chart
					yValue = -yValue;
				}
				if( yValue >= 0 )
				{
					if(!double.IsNaN(prevPosY))
					{
						areaZeroValue = prevPosY;
					}
				}
				else
				{
					if(!double.IsNaN(prevNegY))
					{
						areaZeroValue = prevNegY;
					}
				}

				return yValue - areaZeroValue;
			}


			// Loop through all series
			prevPosY = double.NaN;
			prevNegY = double.NaN;
			prevPositionX = double.NaN;
			foreach(Series ser in common.DataManager.Series)
			{
				// Check series of the current chart type & area
				if(String.Compare(series.ChartArea, ser.ChartArea, true, System.Globalization.CultureInfo.CurrentCulture) == 0 &&
					String.Compare(series.ChartTypeName, ser.ChartTypeName, true, System.Globalization.CultureInfo.CurrentCulture) == 0 &&
					series.IsVisible())
				{
					yValue = (ser.Points[pointIndex].YValues[0] / _totalPerPoint[pointIndex]) * 100.0;
                    
                    // Fix of bug #677411 - Dev10 3D stacked area throws an exception when casting NaN to decimal
                    if (double.IsNaN(yValue) && _totalPerPoint[pointIndex] == 0)
                    {
                        yValue = 100.0 / GetSeriesCount(common, area);
                    }
					
                    if(!double.IsNaN(yValue))
						if(area.Area3DStyle.Enable3D && yValue < 0.0)
						{
							// No negative values support in 3D stacked area chart
							yValue = -yValue;
						}
				    {
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

					// Remenber privious position
					if(yValue >= 0.0)
					{
						prevPosY = yValue;
					}
					else
					{
						prevNegY = yValue;
					}
					prevPositionX = ser.Points[pointIndex].XValue;
					if(prevPositionX == 0.0 && ChartHelper.IndexedSeries(series))
					{
						prevPositionX = pointIndex + 1;
					}
				}
			}

			// Y value can't be more than a 100%
			if(yValue > 100.0)
			{
				return 100.0;
			}

            return yValue;
		}

		#endregion
	}

	/// <summary>
	/// StackedAreaChart class extends AreaChart so that chart series are
    /// positioned on top of each other.
	/// </summary>
	internal class StackedAreaChart : AreaChart
	{
		#region Fields

		/// <summary>
		/// Shape of the previous series
		/// </summary>
		protected	GraphicsPath	areaBottomPath = new GraphicsPath();

		/// <summary>
		/// Previous stacked positive Y values.
		/// </summary>
		protected	double			prevPosY = double.NaN;

		/// <summary>
		/// Previous stacked negative Y values.
		/// </summary>
		protected	double			prevNegY = double.NaN;

		/// <summary>
		/// Previous X value.
		/// </summary>
		protected	double			prevPositionX = double.NaN;

		/// <summary>
		/// Indicates if chart is 100% stacked
		/// </summary>
		protected	bool			hundredPercentStacked = false;

		#endregion

		#region Constructor

		/// <summary>
		/// Public constructor.
		/// </summary>
		public StackedAreaChart()
		{
			multiSeries = true;
			COPCoordinatesToCheck = COPCoordinates.X | COPCoordinates.Y;
		}

		#endregion

		#region Default tension method

		/// <summary>
		/// Gets default line tension.
		/// </summary>
		/// <returns>Line tension.</returns>
		override protected float GetDefaultTension()
		{
			return 0f;
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		public override string Name			{ get{ return ChartTypeNames.StackedArea;}}

		/// <summary>
		/// True if chart type is stacked
		/// </summary>
		public override bool Stacked		{ get{ return true;}}

		/// <summary>
		/// Gets chart type image.
		/// </summary>
		/// <param name="registry">Chart types registry object.</param>
		/// <returns>Chart type image.</returns>
		override public System.Drawing.Image GetImage(ChartTypeRegistry registry)
		{
			return (System.Drawing.Image)registry.ResourceManager.GetObject(this.Name + "ChartType");
		}

		#endregion

		#region Painting and Selection methods

		/// <summary>
		/// Paint Stacked Area Chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		public override void Paint( ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw  )
		{
            this.Common = common;
            // Set Clip Region
			graph.SetClip( area.PlotAreaPosition.ToRectangleF() );

			// Draw chart
			ProcessChartType( false, graph, common, area, seriesToDraw );

			// Reset Clip Region
			((ChartGraphics)graph).ResetClip();
		}

		/// <summary>
		/// This method calculates position of the area and either draws it or checks selection.
		/// </summary>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active.</param>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		override protected void ProcessChartType( 
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			Series seriesToDraw)
		{
            this.Common = common;
			ArrayList	prevPointsArray = null;
			ArrayList	curentPointsArray = null;

			// Prosess 3D chart type
			if(area.Area3DStyle.Enable3D)
			{
				base.ProcessChartType( 
					selection, 
					graph, 
					common, 
					area, 
					seriesToDraw);
				return;
			}

			// Zero X values mode.
			bool	indexedSeries = ChartHelper.IndexedSeries(this.Common, area.GetSeriesFromChartType(this.Name).ToArray() );

			// Indicates that the second point loop for drawing lines or labels is required
			bool	requiresSecondPointLoop = false;
			bool	requiresThirdPointLoop = false;

			//************************************************************
			//** Loop through all series
			//************************************************************
			int	seriesPointsNumber = -1;
			foreach( Series ser in common.DataManager.Series )
			{
				// Process non empty series of the area with area chart type
				if( String.Compare( ser.ChartTypeName, this.Name, StringComparison.OrdinalIgnoreCase ) != 0 
					|| ser.ChartArea != area.Name || !ser.IsVisible())
				{
					continue;
				}

				// Reset area shape paths
				if(areaPath != null)
				{
					areaPath.Dispose();
					areaPath = null;
				}
				areaBottomPath.Reset();

				// Check that all seres has the same number of points
				if(seriesPointsNumber == -1)
				{
					seriesPointsNumber = ser.Points.Count;
				}
				else if(seriesPointsNumber != ser.Points.Count)
				{
                    throw (new ArgumentException(SR.ExceptionStackedAreaChartSeriesDataPointsNumberMismatch));
				}

				// Set active horizontal/vertical axis
				HAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
				VAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);
				hAxisMin = HAxis.ViewMinimum;
				hAxisMax = HAxis.ViewMaximum;
				vAxisMin = VAxis.ViewMinimum;
				vAxisMax = VAxis.ViewMaximum;


				// Get axis position
				axisPos.X = (float)VAxis.GetPosition(this.VAxis.Crossing);
				axisPos.Y = (float)VAxis.GetPosition(this.VAxis.Crossing);
				axisPos = graph.GetAbsolutePoint(axisPos);

				// Fill previous series values array 
				if(curentPointsArray == null)
				{
					curentPointsArray = new ArrayList(ser.Points.Count);
				}
				else
				{
					prevPointsArray = curentPointsArray;
					curentPointsArray = new ArrayList(ser.Points.Count);
				}

				// Call Back Paint event
				if( !selection )
				{
                    common.Chart.CallOnPrePaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}

				// The data points loop
				int		index = 0;
				float	prevYValue1 = axisPos.Y;
				float	prevYValue2 = axisPos.Y;
				PointF	firstPoint = PointF.Empty;
				PointF	secondPoint = PointF.Empty;
				foreach( DataPoint point in ser.Points )
				{
					// Reset pre-calculated point position
					point.positionRel = new PointF(float.NaN, float.NaN);

					// Get point value					
					double yValue = (point.IsEmpty) ? 0.0 : GetYValue(common, area, ser, point, index, 0);
					double xValue = (indexedSeries) ? (index + 1.0) : point.XValue;

					// Adjust point position with previous value
					if(prevPointsArray != null && index < prevPointsArray.Count)
					{
						yValue += (double)prevPointsArray[index];
					}
					curentPointsArray.Insert(index, yValue);

					// Get point position
					float yPosition = (float)VAxis.GetPosition(yValue);
					float xPosition = (float)HAxis.GetPosition(xValue);

					// Remeber pre-calculated point position
					point.positionRel = new PointF(xPosition, yPosition);

					yValue = VAxis.GetLogValue(yValue);
					xValue = HAxis.GetLogValue(xValue);
	
					// Calculate 2 points to draw area and line
					if(firstPoint == PointF.Empty)
					{
						firstPoint.X = xPosition;
						firstPoint.Y = yPosition;
						if(prevPointsArray != null && index < prevPointsArray.Count)
						{
							prevYValue1 = (float)VAxis.GetPosition((double)prevPointsArray[index]);
							prevYValue1 = graph.GetAbsolutePoint(new PointF(prevYValue1, prevYValue1)).Y;
						}
						firstPoint = graph.GetAbsolutePoint(firstPoint);
						
						++index;
						continue;
					}
					else
					{
						secondPoint.X = xPosition;
						secondPoint.Y = yPosition;
						if(prevPointsArray != null && index < prevPointsArray.Count)
						{
							prevYValue2 = (float)VAxis.GetPosition((double)prevPointsArray[index]);
							prevYValue2 = graph.GetAbsolutePoint(new PointF(prevYValue2, prevYValue2)).Y;
						}
						secondPoint = graph.GetAbsolutePoint(secondPoint);
					}

					// Round X coordinates
					firstPoint.X = (float)Math.Round(firstPoint.X);
					secondPoint.X = (float)Math.Round(secondPoint.X);

					
					// Calculate data point area segment path
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddLine(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y);
                        path.AddLine(secondPoint.X, secondPoint.Y, secondPoint.X, prevYValue2);
                        path.AddLine(secondPoint.X, prevYValue2, firstPoint.X, prevYValue1);
                        path.AddLine(firstPoint.X, prevYValue1, firstPoint.X, firstPoint.Y);

                        // Painting mode
                        if (common.ProcessModePaint)
                        {
                            // Get previous point value					
                            double xPrevValue = (indexedSeries) ? (index) : ser.Points[index - 1].XValue;

                            // Check if line is completly out of the data scaleView
                            if ((xValue <= hAxisMin && xPrevValue <= hAxisMin) ||
                                (xValue >= hAxisMax && xPrevValue >= hAxisMax))
                            {
                                // Save previous point
                                firstPoint = secondPoint;
                                prevYValue1 = prevYValue2;

                                // Increase data point index
                                ++index;

                                continue;
                            }

                            // Create area brush
                            Brush areaBrush = null;
                            if (point.BackHatchStyle != ChartHatchStyle.None)
                            {
                                areaBrush = graph.GetHatchBrush(point.BackHatchStyle, point.Color, point.BackSecondaryColor);
                            }
                            else if (point.BackGradientStyle != GradientStyle.None)
                            {
                                this.gradientFill = true;
                                this.Series = point.series;
                            }
                            else if (point.BackImage.Length > 0 && point.BackImageWrapMode != ChartImageWrapMode.Unscaled && point.BackImageWrapMode != ChartImageWrapMode.Scaled)
                            {
                                areaBrush = graph.GetTextureBrush(point.BackImage, point.BackImageTransparentColor, point.BackImageWrapMode, point.Color);
                            }
                            else if (point.IsEmpty && point.Color == Color.Empty)
                            {
                                // Stacked area chart empty points should always use 
                                // series color, otherwise chart will have empty 'holes'.
                                areaBrush = new SolidBrush(ser.Color);
                            }
                            else
                            {
                                areaBrush = new SolidBrush(point.Color);
                            }

                            // Check if we need second loop to draw area border
                            if ((point.BorderColor != Color.Empty && point.BorderWidth > 0))
                            {
                                requiresSecondPointLoop = true;
                            }

                            // Check if we need third loop to draw labels
                            if (point.Label.Length > 0 || point.IsValueShownAsLabel)
                            {
                                requiresThirdPointLoop = true;
                            }

                            // Draw area
                            if (!this.gradientFill)
                            {
                                // Start Svg Selection mode
                                graph.StartHotRegion(point);

                                // Turn off anti aliasing and fill area
                                SmoothingMode oldMode = graph.SmoothingMode;
                                graph.SmoothingMode = SmoothingMode.None;
                                graph.FillPath(areaBrush, path);
                                graph.SmoothingMode = oldMode;

                                // Draw top and bottom lines with antialiasing turned On.
                                // Process only if line is drawn by an angle
                                Pen areaLinePen = new Pen(areaBrush, 1);
                                if (!(firstPoint.X == secondPoint.X || firstPoint.Y == secondPoint.Y))
                                {
                                    graph.DrawLine(areaLinePen, firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y);
                                }
                                if (!(firstPoint.X == secondPoint.X || prevYValue2 == prevYValue1))
                                {
                                    graph.DrawLine(areaLinePen, secondPoint.X, prevYValue2, firstPoint.X, prevYValue1);
                                }

                                // End Svg Selection mode
                                graph.EndHotRegion();
                            }

                            if (areaPath == null)
                            {
                                areaPath = new GraphicsPath();
                            }
                            areaPath.AddLine(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y);
                            areaBottomPath.AddLine(firstPoint.X, prevYValue1, secondPoint.X, prevYValue2);

                            //Clean up
                            if (areaBrush != null)
                                areaBrush.Dispose();
                        }

                        if (common.ProcessModeRegions)
                        {
                            //**************************************************************
                            //** Add area for the inside of the area
                            //**************************************************************

                            // Allocate array of floats
                            PointF pointNew = PointF.Empty;
                            float[] coord = new float[path.PointCount * 2];
                            PointF[] pathPoints = path.PathPoints;
                            for (int i = 0; i < path.PointCount; i++)
                            {
                                pointNew = graph.GetRelativePoint(pathPoints[i]);
                                coord[2 * i] = pointNew.X;
                                coord[2 * i + 1] = pointNew.Y;
                            }

                            common.HotRegionsList.AddHotRegion(
                                path,
                                false,
                                coord,
                                point,
                                ser.Name,
                                index);

                            //**************************************************************
                            //** Add area for the top line (with thickness)
                            //**************************************************************
                            if (point.BorderWidth > 1 && point.BorderDashStyle != ChartDashStyle.NotSet && point.BorderColor != Color.Empty)
                            {
                                // Create grapics path object dor the curve
                                using (GraphicsPath linePath = new GraphicsPath())
                                {
                                    try
                                    {
                                        linePath.AddLine(firstPoint.X, firstPoint.Y, secondPoint.X, secondPoint.Y);

                                        // Widen the lines to the size of pen plus 2
                                        linePath.Widen(new Pen(point.Color, point.BorderWidth + 2));
                                    }
                                    catch (OutOfMemoryException)
                                    {
                                        // GraphicsPath.Widen incorrectly throws OutOfMemoryException
                                        // catching here and reacting by not widening
                                    }
                                    catch (ArgumentException)
                                    {
                                    }

                                    // Allocate array of floats
                                    pointNew = PointF.Empty;
                                    coord = new float[linePath.PointCount * 2];
                                    for (int i = 0; i < linePath.PointCount; i++)
                                    {
                                        pointNew = graph.GetRelativePoint(linePath.PathPoints[i]);
                                        coord[2 * i] = pointNew.X;
                                        coord[2 * i + 1] = pointNew.Y;
                                    }

                                    common.HotRegionsList.AddHotRegion(
                                        linePath,
                                        false,
                                        coord,
                                        point,
                                        ser.Name,
                                        index);
                                }
                            }
                        }
                    }
					// Save previous point
					firstPoint = secondPoint;
					prevYValue1 = prevYValue2;

					// Increase data point index
					++index;

				}

				// Fill whole series area with gradient
				if(gradientFill && areaPath != null)
				{
					// Create gradient path
                    using (GraphicsPath gradientPath = new GraphicsPath())
                    {
                        gradientPath.AddPath(areaPath, true);
                        areaBottomPath.Reverse();
                        gradientPath.AddPath(areaBottomPath, true);

                        // Create brush
                        using (Brush areaBrush = graph.GetGradientBrush(gradientPath.GetBounds(), this.Series.Color, this.Series.BackSecondaryColor, this.Series.BackGradientStyle))
                        {
                            // Fill area with gradient
                            graph.FillPath(areaBrush, gradientPath);
                        }
                    }

					areaPath.Dispose();
					areaPath = null;
					gradientFill = false;
				}
				areaBottomPath.Reset();

				// Call Paint event
				if( !selection )
				{
                    common.Chart.CallOnPostPaint(new ChartPaintEventArgs(ser, graph, common, area.PlotAreaPosition));
				}
			}

			//************************************************************
			//** Loop through all series/points for the second time
			//** Draw border lines.
			//************************************************************
			if(requiresSecondPointLoop)
			{
				prevPointsArray = null;
				curentPointsArray = null;
				foreach( Series ser in common.DataManager.Series )
				{
					if( String.Compare( ser.ChartTypeName, this.Name, StringComparison.OrdinalIgnoreCase ) != 0 
						|| ser.ChartArea != area.Name || !ser.IsVisible())
					{
						continue;
					}

					// Set active horizontal/vertical axis
					HAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
					VAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

					// Get axis position
					axisPos.X = (float)VAxis.GetPosition(this.VAxis.Crossing);
					axisPos.Y = (float)VAxis.GetPosition(this.VAxis.Crossing);
					axisPos = graph.GetAbsolutePoint(axisPos);

					// Fill previous series values array 
					if(curentPointsArray == null)
					{
						curentPointsArray = new ArrayList(ser.Points.Count);
					}
					else
					{
						prevPointsArray = curentPointsArray;
						curentPointsArray = new ArrayList(ser.Points.Count);
					}

					// The data points loop
					int		index = 0;
					float	prevYValue1 = axisPos.Y;
					float	prevYValue2 = axisPos.Y;
					PointF	firstPoint = PointF.Empty;
					PointF	secondPoint = PointF.Empty;
					foreach( DataPoint point in ser.Points )
					{
						// Get point value					
						double yValue = (point.IsEmpty) ? 0.0 : GetYValue(common, area, ser, point, index, 0);
						double xValue = (indexedSeries) ? (index + 1.0) : point.XValue;

						// Adjust point position with previous value
						if(prevPointsArray != null && index < prevPointsArray.Count)
						{
							yValue += (double)prevPointsArray[index];
						}
						curentPointsArray.Insert(index, yValue);

						// Get point position
						float yPosition = (float)VAxis.GetPosition(yValue);
						float xPosition = (float)HAxis.GetPosition(xValue);

	
						// Calculate 2 points to draw area and line
						if(firstPoint == PointF.Empty)
						{
							firstPoint.X = xPosition;
							firstPoint.Y = yPosition;
							if(prevPointsArray != null && index < prevPointsArray.Count)
							{
								prevYValue1 = (float)VAxis.GetPosition((double)prevPointsArray[index]);
								prevYValue1 = graph.GetAbsolutePoint(new PointF(prevYValue1, prevYValue1)).Y;
							}
							firstPoint = graph.GetAbsolutePoint(firstPoint);
							secondPoint = firstPoint;
							prevYValue2 = prevYValue1;
						}
						else
						{
							secondPoint.X = xPosition;
							secondPoint.Y = yPosition;
							if(prevPointsArray != null && index < prevPointsArray.Count)
							{
								prevYValue2 = (float)VAxis.GetPosition((double)prevPointsArray[index]);
								prevYValue2 = graph.GetAbsolutePoint(new PointF(prevYValue2, prevYValue2)).Y;
							}
							secondPoint = graph.GetAbsolutePoint(secondPoint);
						}

						if(index != 0)
						{
							// Round X coordinates
							firstPoint.X = (float)Math.Round(firstPoint.X);
							secondPoint.X = (float)Math.Round(secondPoint.X);

							// Draw border
							graph.DrawLineRel(point.BorderColor, point.BorderWidth, point.BorderDashStyle, graph.GetRelativePoint(firstPoint), graph.GetRelativePoint(secondPoint), point.series.ShadowColor, point.series.ShadowOffset );
						}
				
						// Save previous point
						firstPoint = secondPoint;
						prevYValue1 = prevYValue2;

						// Increase data point index
						++index;
					}
				}
			}

			//************************************************************
			//** Loop through all series/points for the second time
			//** Draw labels.
			//************************************************************
			if(requiresThirdPointLoop)
			{
				prevPointsArray = null;
				curentPointsArray = null;
				foreach( Series ser in common.DataManager.Series )
				{
					if( String.Compare( ser.ChartTypeName, this.Name, StringComparison.OrdinalIgnoreCase ) != 0 
						|| ser.ChartArea != area.Name || !ser.IsVisible())
					{
						continue;
					}

					// Set active horizontal/vertical axis
					HAxis = area.GetAxis(AxisName.X, ser.XAxisType, ser.XSubAxisName);
					VAxis = area.GetAxis(AxisName.Y, ser.YAxisType, ser.YSubAxisName);

					// Get axis position
					axisPos.X = (float)VAxis.GetPosition(this.VAxis.Crossing);
					axisPos.Y = (float)VAxis.GetPosition(this.VAxis.Crossing);
					axisPos = graph.GetAbsolutePoint(axisPos);

					// Fill previous series values array 
					if(curentPointsArray == null)
					{
						curentPointsArray = new ArrayList(ser.Points.Count);
					}
					else
					{
						prevPointsArray = curentPointsArray;
						curentPointsArray = new ArrayList(ser.Points.Count);
					}

					// The data points loop
					int		index = 0;
					float	prevYValue1 = axisPos.Y;
					float	prevYValue2 = axisPos.Y;
					PointF	firstPoint = PointF.Empty;
					PointF	secondPoint = PointF.Empty;
					foreach( DataPoint point in ser.Points )
					{
						// Get point value					
						double yValue = (point.IsEmpty) ? 0.0 : GetYValue(common, area, ser, point, index, 0);
						double xValue = (indexedSeries) ? (index + 1.0) : point.XValue;

						// Adjust point position with previous value
						if(prevPointsArray != null && index < prevPointsArray.Count)
						{
							yValue += (double)prevPointsArray[index];
						}
						curentPointsArray.Insert(index, yValue);

						// Get point position
						float yPosition = (float)VAxis.GetPosition(yValue);
						float xPosition = (float)HAxis.GetPosition(xValue);

	
						// Calculate 2 points to draw area and line
						if(firstPoint == PointF.Empty)
						{
							firstPoint.X = xPosition;
							firstPoint.Y = yPosition;
							if(prevPointsArray != null && index < prevPointsArray.Count)
							{
								prevYValue1 = (float)VAxis.GetPosition((double)prevPointsArray[index]);
								prevYValue1 = graph.GetAbsolutePoint(new PointF(prevYValue1, prevYValue1)).Y;
							}
							firstPoint = graph.GetAbsolutePoint(firstPoint);
							secondPoint = firstPoint;
							prevYValue2 = prevYValue1;
						}
						else
						{
							secondPoint.X = xPosition;
							secondPoint.Y = yPosition;
							if(prevPointsArray != null && index < prevPointsArray.Count)
							{
								prevYValue2 = (float)VAxis.GetPosition((double)prevPointsArray[index]);
								prevYValue2 = graph.GetAbsolutePoint(new PointF(prevYValue2, prevYValue2)).Y;
							}
							secondPoint = graph.GetAbsolutePoint(secondPoint);
						}

						if(!point.IsEmpty && (ser.IsValueShownAsLabel || point.IsValueShownAsLabel || point.Label.Length > 0))
						{
							// Label text format
                            using (StringFormat format = new StringFormat())
                            {
                                format.Alignment = StringAlignment.Center;
                                format.LineAlignment = StringAlignment.Center;

                                // Get label text
                                string text;
                                if (point.Label.Length == 0)
                                {
                                    double pointLabelValue = GetYValue(common, area, ser, point, index, 0);
                                    // Round Y values for 100% stacked area
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

                                // Disable the clip region
                                Region oldClipRegion = graph.Clip;
                                graph.Clip = new Region();

                                // Draw label
                                PointF labelPosition = PointF.Empty;
                                labelPosition.X = secondPoint.X;
                                labelPosition.Y = secondPoint.Y - (secondPoint.Y - prevYValue2) / 2f;
                                labelPosition = graph.GetRelativePoint(labelPosition);

                                // Measure string
                                SizeF sizeFont = graph.GetRelativeSize(
                                    graph.MeasureString(
                                    text,
                                    point.Font,
                                    new SizeF(1000f, 1000f),
                                    StringFormat.GenericTypographic));

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
                                        point.LabelAngle,
                                        labelBackPosition,
                                        point.LabelBackColor,
                                        point.LabelBorderColor,
                                        point.LabelBorderWidth,
                                        point.LabelBorderDashStyle,
                                        ser,
                                        point,
                                        index);
                                }

                                // Restore old clip region
                                graph.Clip = oldClipRegion;
                            }
						}

				
						// Save previous point
						firstPoint = secondPoint;
						prevYValue1 = prevYValue2;

						// Increase data point index
						++index;

					}
				}
			}
		}

		#endregion

		#region 3D Drawing and selection methods

		/// <summary>
		/// Draws a 3D surface connecting the two specified points in 2D space.
		/// Used to draw Line based charts.
		/// </summary>
		/// <param name="area">Chart area reference.</param>
		/// <param name="graph">Chart graphics.</param>
		/// <param name="matrix">Coordinates transformation matrix.</param>
		/// <param name="lightStyle">LightStyle style (None, Simplistic, Realistic).</param>
		/// <param name="prevDataPointEx">Previous data point object.</param>
		/// <param name="positionZ">Z position of the back side of the 3D surface.</param>
		/// <param name="depth">Depth of the 3D surface.</param>
		/// <param name="points">Array of points.</param>
		/// <param name="pointIndex">Index of point to draw.</param>
		/// <param name="pointLoopIndex">Index of points loop.</param>
		/// <param name="tension">Line tension.</param>
		/// <param name="operationType">AxisName of operation Drawing, Calculating Path or Both</param>
		/// <param name="topDarkening">Darkenning scale for top surface. 0 - None.</param>
		/// <param name="bottomDarkening">Darkenning scale for bottom surface. 0 - None.</param>
		/// <param name="thirdPointPosition">Position where the third point is actually located or float.NaN if same as in "firstPoint".</param>
		/// <param name="fourthPointPosition">Position where the fourth point is actually located or float.NaN if same as in "secondPoint".</param>
		/// <param name="clippedSegment">Indicates that drawn segment is 3D clipped. Only top/bottom should be drawn.</param>
		/// <returns>Returns elemnt shape path if operationType parameter is set to CalcElementPath, otherwise Null.</returns>
		protected override GraphicsPath Draw3DSurface( 
			ChartArea area,
			ChartGraphics graph, 
			Matrix3D matrix,
			LightStyle lightStyle,
			DataPoint3D prevDataPointEx,
			float positionZ, 
			float depth, 
			ArrayList points,
			int pointIndex, 
			int pointLoopIndex,
			float tension,
			DrawingOperationTypes operationType,
			float topDarkening,
			float bottomDarkening,
			PointF thirdPointPosition,
			PointF fourthPointPosition,
			bool clippedSegment)
		{
			// Call base method
			
			if(pointLoopIndex != 2)
			{
				return base.Draw3DSurface( 
					area,
					graph, 
					matrix,
					lightStyle,
					prevDataPointEx,
					positionZ, 
					depth, 
					points,
					pointIndex, 
					pointLoopIndex,
					tension,
					operationType,
					topDarkening, 
					bottomDarkening,
					thirdPointPosition, 
					fourthPointPosition,
					clippedSegment);
			}

			// Draw labels in the third loop
			else
			{
				DataPoint3D	pointEx = ((DataPoint3D)points[pointIndex]);

				// Draw label for the first point
				if(pointEx.index == 2)
				{
					// Get point with prev index
					int neighborPointIndex = 0;
					DataPoint3D	pointPrevEx = ChartGraphics.FindPointByIndex(points, pointEx.index - 1, pointEx, ref neighborPointIndex);

					// Draw labels in the third loop
					DrawLabels3D( 
						area, 
						graph, 
						area.Common, 
						pointPrevEx,
						positionZ,
						depth);
				}

				// Draw labels in the third loop
				DrawLabels3D( 
					area, 
					graph, 
					area.Common, 
					pointEx,
					positionZ,
					depth);
			}

			return new GraphicsPath();
		}

		/// <summary>
		/// Gets visibility of the top surface.
		/// </summary>
		/// <param name="area">Chart area object.</param>
		/// <param name="firstPoint">First data point of the line.</param>
		/// <param name="secondPoint">Second data point of the line.</param>
		/// <param name="upSideDown">Indicates that Y values of the data points are below axis line.</param>
		/// <param name="positionZ">Z coordinate of the back side of the cube.</param>
		/// <param name="depth">Cube depth.</param>
		/// <param name="matrix">Coordinate transformation matrix.</param>
		/// <param name="visibleSurfaces">Surface visibility reference. Initialized with bounary cube visibility.</param>
		protected override void GetTopSurfaceVisibility(
			ChartArea area,
			DataPoint3D firstPoint, 
			DataPoint3D secondPoint, 
			bool upSideDown,
			float positionZ, 
			float depth, 
			Matrix3D matrix,
			ref SurfaceNames visibleSurfaces)
		{
			// Call base class method first
			base.GetTopSurfaceVisibility(area, firstPoint, secondPoint, upSideDown, 
				positionZ, depth, matrix, ref visibleSurfaces);

			// Check if the Top surface is overlapped with data point from other series
			if( (visibleSurfaces & SurfaceNames.Top) == SurfaceNames.Top )
			{
				// Try to find data point with same index from the series above
				bool	seriesFound = false;
				foreach(Series ser in area.Common.DataManager.Series)
				{
					if(String.Compare(ser.ChartTypeName, secondPoint.dataPoint.series.ChartTypeName, true, System.Globalization.CultureInfo.CurrentCulture) == 0)
					{
						// If series on top of current was found - check point transparency
						if(seriesFound)
						{
							DataPointCustomProperties	pointProperties = ser.Points[secondPoint.index - 1];
							if(ser.Points[secondPoint.index - 1].IsEmpty)
							{
                                pointProperties = ser.EmptyPointStyle;
							}
                            if (pointProperties.Color.A == 255)
							{
								visibleSurfaces ^= SurfaceNames.Top;
							}
							break;
						}

						// Check series name
						if(String.Compare(ser.Name, secondPoint.dataPoint.series.Name, StringComparison.Ordinal) == 0)
						{
							seriesFound = true;
						}
					}
				}
			}

			// Check if the Bottom surface is on top of the transparent data point from other series
			if( (visibleSurfaces & SurfaceNames.Bottom) != SurfaceNames.Bottom )
			{
				// Try to find data point with same index from the series above
				DataPointCustomProperties	pointProperties = null;
				foreach(Series ser in area.Common.DataManager.Series)
				{
					if(String.Compare(ser.ChartTypeName, secondPoint.dataPoint.series.ChartTypeName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						// Check series name
                        if (pointProperties != null && String.Compare(ser.Name, secondPoint.dataPoint.series.Name, StringComparison.Ordinal) == 0)
						{
                            if (pointProperties.Color.A != 255)
							{
								visibleSurfaces |= SurfaceNames.Bottom;
							}
							break;						
						}

						// Get properties
                        pointProperties = ser.Points[secondPoint.index - 1];
						if(ser.Points[secondPoint.index - 1].IsEmpty)
						{
                            pointProperties = ser.EmptyPointStyle;
						}
					}
				}
			}

		}

		/// <summary>
		/// Gets position ob the bottom points in area chart.
		/// </summary>
		/// <param name="common">Chart common elements.</param>
		/// <param name="area">Chart area the series belongs to.</param>
		/// <param name="axisPosition">Axis position.</param>
		/// <param name="firstPoint">First top point coordinates.</param>
		/// <param name="secondPoint">Second top point coordinates.</param>
		/// <param name="thirdPointPosition">Position where the third point is actually located or float.NaN if same as in "firstPoint".</param>
		/// <param name="fourthPointPosition">Position where the fourth point is actually located or float.NaN if same as in "secondPoint".</param>
		/// <param name="thirdPoint">Returns third bottom point coordinates.</param>
		/// <param name="fourthPoint">Returns fourth bottom point coordinates.</param>
		protected override void GetBottomPointsPosition(
			CommonElements common, 
			ChartArea area, 
			float axisPosition, 
			ref DataPoint3D firstPoint, 
			ref DataPoint3D secondPoint,
			PointF thirdPointPosition,
			PointF fourthPointPosition,
			out PointF thirdPoint, 
			out PointF fourthPoint)
		{
			// Set active vertical/horizontal axis
			Axis	vAxis = area.GetAxis(AxisName.Y, firstPoint.dataPoint.series.YAxisType, firstPoint.dataPoint.series.YSubAxisName);
			Axis	hAxis = area.GetAxis(AxisName.X, firstPoint.dataPoint.series.XAxisType, firstPoint.dataPoint.series.XSubAxisName);

			// Find bottom points position
			double yValue = GetYValue(area.Common, area, firstPoint.dataPoint.series, firstPoint.dataPoint, firstPoint.index - 1, 0);
			double xValue = (float)firstPoint.xPosition;
			if(yValue >= 0.0)
			{
				if(double.IsNaN(this.prevPosY))
				{
					yValue = axisPosition;
				}
				else
				{
					yValue = vAxis.GetPosition(this.prevPosY);
					xValue = hAxis.GetPosition(this.prevPositionX);
				}
			}
			else
			{
				if(double.IsNaN(this.prevNegY))
				{
					yValue = axisPosition;
				}
				else
				{
					yValue = vAxis.GetPosition(this.prevNegY);
					xValue = hAxis.GetPosition(this.prevPositionX);
				}
			}
			thirdPoint = new PointF((float)xValue, (float)yValue);


			yValue = GetYValue(area.Common, area, secondPoint.dataPoint.series, secondPoint.dataPoint, secondPoint.index - 1, 0);
			xValue = (float)secondPoint.xPosition;
			if(yValue >= 0.0)
			{
				if(double.IsNaN(this.prevPosY))
				{
					yValue = axisPosition;
				}
				else
				{
					yValue = vAxis.GetPosition(this.prevPosY);
					xValue = hAxis.GetPosition(this.prevPositionX);
				}
			}
			else
			{
				if(double.IsNaN(this.prevNegY))
				{
					yValue = axisPosition;
				}
				else
				{
					yValue = vAxis.GetPosition(this.prevNegY);
					xValue = hAxis.GetPosition(this.prevPositionX);
				}
			}
			fourthPoint = new PointF((float)xValue, (float)yValue);

			// Check if position of the third and/or fourth point(s) should be adjusted
			if(!float.IsNaN(thirdPointPosition.X))
			{
				thirdPoint.X = (float)((firstPoint.xCenterVal == 0.0) ? firstPoint.xPosition : firstPoint.xCenterVal);

				// Calculate new Y value as an intersection point of two lines:
				// line between current 3d & 4th points and vertical line with X value = thirdPointPositionX.
				thirdPoint.Y = (thirdPointPosition.X - fourthPoint.X) / 
					(thirdPoint.X - fourthPoint.X) * 
					(thirdPoint.Y - fourthPoint.Y) + 
					fourthPoint.Y;

				// Set new X value
				thirdPoint.X = thirdPointPosition.X;
			}
			if(!float.IsNaN(thirdPointPosition.Y))
			{
				thirdPoint.Y = thirdPointPosition.Y;
			}			
			
			if(!float.IsNaN(fourthPointPosition.X))
			{
				fourthPoint.X = (float)((secondPoint.xCenterVal == 0.0) ? secondPoint.xPosition : secondPoint.xCenterVal);

				// Calculate new Y value as an intersection point of two lines:
				// line between current 3d & 4th points and vertical line with X value = thirdPointPositionX.
				fourthPoint.Y = (fourthPointPosition.X - fourthPoint.X) / 
					(thirdPoint.X - fourthPoint.X) * 
					(thirdPoint.Y - fourthPoint.Y) + 
					fourthPoint.Y;

				// Set new X value
				fourthPoint.X = fourthPointPosition.X;
			}
			if(!float.IsNaN(fourthPointPosition.Y))
			{
				fourthPoint.Y = fourthPointPosition.Y;
			}			

		}

		/// <summary>
		/// Returns how many loops through all data points is required (1 or 2)
		/// </summary>
		/// <param name="selection">Selection indicator.</param>
		/// <param name="pointsArray">Points array list.</param>
		/// <returns>Number of loops (1 or 2).</returns>
		override protected int GetPointLoopNumber(bool selection, ArrayList pointsArray)
		{
			// Always one loop for selection
			if(selection)
			{
				return 1;
			}

			// Second loop will be required for semi-transparent colors
			int loopNumber = 1;
			foreach(object obj in pointsArray)
			{
				// Get point & series
				DataPoint3D	pointEx = (DataPoint3D) obj;

				// Check properties
				if(pointEx.dataPoint.Color.A != 255)
				{
					loopNumber = 2;
				}

				// Check title
                // VSTS fix #529011: 3-d stacked area and 100% stacked area charts do not show data labels.
				if( pointEx.dataPoint.Label.Length > 0 ||
                    pointEx.dataPoint.IsValueShownAsLabel || 
					pointEx.dataPoint.series.IsValueShownAsLabel)
				{
					// S loops through all data points required
					loopNumber = 3;
					break;
				}
			}

			return loopNumber;
		}

		/// <summary>
		/// This method draws labels in point chart.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="pointEx">Data point 3D.</param>
		/// <param name="positionZ">Z position of the back side of the 3D surface.</param>
		/// <param name="depth">Depth of the 3D surface.</param>
		private void DrawLabels3D( 
			ChartArea area, 
			ChartGraphics graph, 
			CommonElements common, 
			DataPoint3D pointEx,
			float positionZ, 
			float depth)
		{
			// Get some properties for performance
			string	pointLabel = pointEx.dataPoint.Label;
			bool	pointShowLabelAsValue = pointEx.dataPoint.IsValueShownAsLabel;

			// ****************************
			// Draw data point value label
			// ****************************
			if((!pointEx.dataPoint.IsEmpty && (pointEx.dataPoint.series.IsValueShownAsLabel || pointShowLabelAsValue || pointLabel.Length > 0)) ||
				(pointShowLabelAsValue || pointLabel.Length > 0))
			{
				// Label text format
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    // Get label text
                    string text;
                    if (pointLabel.Length == 0)
                    {
                        // Round Y values for 100% stacked area
                        double pointLabelValue = pointEx.dataPoint.YValues[(labelYValueIndex == -1) ? YValueIndex : labelYValueIndex];
                        if (this.hundredPercentStacked && pointEx.dataPoint.LabelFormat.Length == 0)
                        {
                            pointLabelValue = Math.Round(pointLabelValue, 2);
                        }

                        text = ValueConverter.FormatValue(
                            pointEx.dataPoint.series.Chart,
                            pointEx.dataPoint,
                            pointEx.dataPoint.Tag,
                            pointLabelValue,
                            pointEx.dataPoint.LabelFormat,
                            pointEx.dataPoint.series.YValueType,
                            ChartElementType.DataPoint);
                    }
                    else
                    {
                        text = pointEx.dataPoint.ReplaceKeywords(pointLabel);
                    }

                    // Get label position
                    Point3D[] points = new Point3D[1];
                    points[0] = new Point3D((float)pointEx.xPosition, (float)(pointEx.yPosition + pointEx.height) / 2f, positionZ + depth);
                    area.matrix3D.TransformPoints(points);

                    // Measure string
                    SizeF sizeFont = graph.GetRelativeSize(
                        graph.MeasureString(
                        text,
                        pointEx.dataPoint.Font,
                        new SizeF(1000f, 1000f),
                        StringFormat.GenericTypographic));

                    // Get label background position
                    RectangleF labelBackPosition = RectangleF.Empty;
                    SizeF sizeLabel = new SizeF(sizeFont.Width, sizeFont.Height);
                    sizeLabel.Height += sizeFont.Height / 8;
                    sizeLabel.Width += sizeLabel.Width / text.Length;
                    labelBackPosition = new RectangleF(
                        points[0].PointF.X - sizeLabel.Width / 2,
                        points[0].PointF.Y - sizeLabel.Height / 2 - sizeFont.Height / 10,
                        sizeLabel.Width,
                        sizeLabel.Height);

                    // Draw label text
                    using (Brush brush = new SolidBrush(pointEx.dataPoint.LabelForeColor))
                    {
                        graph.DrawPointLabelStringRel(
                            common,
                            text,
                            pointEx.dataPoint.Font,
                            brush,
                            points[0].PointF,
                            format,
                            pointEx.dataPoint.LabelAngle,
                            labelBackPosition,
                            pointEx.dataPoint.LabelBackColor,
                            pointEx.dataPoint.LabelBorderColor,
                            pointEx.dataPoint.LabelBorderWidth,
                            pointEx.dataPoint.LabelBorderDashStyle,
                            pointEx.dataPoint.series,
                            pointEx.dataPoint,
                            pointEx.index - 1);
                    }
                }
			}
		}

		#endregion

		#region Y values methods

		/// <summary>
		/// Helper function, which returns the Y value of the point.
		/// </summary>
		/// <param name="common">Chart common elements.</param>
		/// <param name="area">Chart area the series belongs to.</param>
		/// <param name="series">Sereis of the point.</param>
		/// <param name="point">Point object.</param>
		/// <param name="pointIndex">Index of the point.</param>
		/// <param name="yValueIndex">Index of the Y value to get.  Set to -1 to get the height.</param>
		/// <returns>Y value of the point.</returns>
		override public double GetYValue(
			CommonElements common, 
			ChartArea area, 
			Series series, 
			DataPoint point, 
			int pointIndex, 
			int yValueIndex)
		{
			double	yValue = double.NaN;

			// Calculate stacked column Y value for 2D chart
			if(area.Area3DStyle.Enable3D == false)
			{
				return point.YValues[0];
			}

			// Get point Height if pointIndex == -1
			if(yValueIndex == -1)
			{
				Axis	vAxis = area.GetAxis(AxisName.Y, series.YAxisType, series.YSubAxisName);
				double	areaZeroValue = vAxis.Crossing;
				yValue = GetYValue(common, area, series, point, pointIndex, 0);
				if(area.Area3DStyle.Enable3D && yValue < 0.0)
				{
					// No negative values support in 3D stacked area chart
					yValue = -yValue;
				}
				if( yValue >= 0 )
				{
					if(!double.IsNaN(prevPosY))
					{
						areaZeroValue = prevPosY;
					}
				}
				else
				{
					if(!double.IsNaN(prevNegY))
					{
						areaZeroValue = prevNegY;
					}
				}

				return yValue - areaZeroValue;
			}


			// Loop through all series
			prevPosY = double.NaN;
			prevNegY = double.NaN;
			prevPositionX = double.NaN;
			foreach(Series ser in common.DataManager.Series)
			{
				// Check series of the current chart type & area
				if(String.Compare(series.ChartArea, ser.ChartArea, StringComparison.Ordinal) == 0 &&
                    String.Compare(series.ChartTypeName, ser.ChartTypeName, StringComparison.OrdinalIgnoreCase) == 0 &&
					ser.IsVisible())
				{
					yValue = ser.Points[pointIndex].YValues[0];
					if(area.Area3DStyle.Enable3D && yValue < 0.0)
					{
						// No negative values support in 3D stacked area chart
						yValue = -yValue;
					}
					if(!double.IsNaN(yValue))
					{
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

					// Remember privious position
					if(yValue >= 0.0)
					{
						prevPosY = yValue;
					}
					if(yValue < 0.0)
					{
						prevNegY = yValue;
					}
					prevPositionX = ser.Points[pointIndex].XValue;
					if(prevPositionX == 0.0 && ChartHelper.IndexedSeries(series))
					{
						prevPositionX = pointIndex + 1;
					}
				}
			}
		
			return yValue;
		}

		#endregion

        #region IDisposable overrides
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (this.areaBottomPath != null)
                {
                    this.areaBottomPath.Dispose();
                    this.areaBottomPath = null;
                }
            }
            base.Dispose(disposing);
        }
        #endregion

	}
}
