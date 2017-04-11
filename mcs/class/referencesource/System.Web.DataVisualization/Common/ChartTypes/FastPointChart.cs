//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		FastPointChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	FastPointChart
//
//  Purpose:	When performance is critical, the FastPoint chart 
//              type is a good alternative to the Point chart. FastPoint 
//              charts significantly reduce the drawing time of a 
//              series that contains a very large number of data points.
//              
//              To make the FastPoint chart a high performance chart, 
//              some charting features have been omitted. The features 
//              omitted include the ability to control Point level 
//              visual properties the use of data point labels, shadows, 
//              and the use of chart animation.
//
//              FastPoint chart performance was improved by limiting 
//              visual appearance features and by introducing data 
//              point compacting algorithm. When chart contains 
//              thousands of data points, it is common to have tens 
//              or hundreds points displayed in the area comparable 
//              to a single pixel. FastPoint algorithm accumulates 
//              point information and only draw points if they extend 
//              outside currently filled pixels.
//
//	Reviewed:	AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

#if Microsoft_CONTROL
using System.Windows.Forms.DataVisualization.Charting.Utilities;
#else
using System.Web.UI.DataVisualization.Charting;

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
    /// FastPointChart class implements a simplified point chart drawing 
    /// algorithm which is optimized for the performance.
    /// </summary>
    internal class FastPointChart : IChartType
	{
		#region Fields and Constructor

		/// <summary>
		/// Indicates that chart is drawn in 3D area
		/// </summary>
		internal bool				chartArea3DEnabled = false;
		
		/// <summary>
		/// Current chart graphics
		/// </summary>
        internal ChartGraphics Graph { get; set; }

		/// <summary>
		/// Z coordinate of the 3D series
		/// </summary>
        internal float seriesZCoordinate = 0f;

		/// <summary>
		/// 3D transformation matrix
		/// </summary>
        internal Matrix3D matrix3D = null;

		/// <summary>
		/// Reference to common chart elements
		/// </summary>
        internal CommonElements Common { get; set; }

		/// <summary>
		/// Default constructor
		/// </summary>
		public FastPointChart()
		{
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		virtual public string Name			{ get{ return ChartTypeNames.FastPoint;}}

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
		virtual public bool RequireAxes	{ get{ return true;} }

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
		virtual public bool DataPointsInLegend	{ get{ return false;} }

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
		virtual public bool ApplyPaletteColorsToPoints	{ get { return false; } }

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
			return LegendImageStyle.Marker;
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
		/// Paint FastPoint Chart.
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
			this.Common = common;
			this.Graph = graph;
			if(area.Area3DStyle.Enable3D)
			{
				// Initialize variables
				this.chartArea3DEnabled = true;
				matrix3D = area.matrix3D;
			}
			else
			{
				this.chartArea3DEnabled = false;
			}
			
			//************************************************************
			//** Loop through all series
			//************************************************************
			foreach( Series series in common.DataManager.Series )
			{
				// Process non empty series of the area with FastPoint chart type
				if( String.Compare( series.ChartTypeName, this.Name, true, System.Globalization.CultureInfo.CurrentCulture ) != 0 
					|| series.ChartArea != area.Name || 
					!series.IsVisible())
				{
					continue;
				}

				// Get 3D series depth and Z position
				if(this.chartArea3DEnabled)
				{
					float seriesDepth;
					area.GetSeriesZPositionAndDepth(series, out seriesDepth, out this.seriesZCoordinate);
					this.seriesZCoordinate += seriesDepth/2.0f;
				}

				// Set active horizontal/vertical axis
				Axis hAxis = area.GetAxis(AxisName.X, series.XAxisType, (area.Area3DStyle.Enable3D) ? string.Empty : series.XSubAxisName);
				Axis vAxis = area.GetAxis(AxisName.Y, series.YAxisType, (area.Area3DStyle.Enable3D) ? string.Empty : series.YSubAxisName);
				double hAxisMin = hAxis.ViewMinimum;
				double hAxisMax = hAxis.ViewMaximum;
				double vAxisMin = vAxis.ViewMinimum;
				double vAxisMax = vAxis.ViewMaximum;

				// Get "PermittedPixelError" attribute.
				// By default use 1/3 of the marker size.
				float	permittedPixelError = series.MarkerSize / 3f;
                if (series.IsCustomPropertySet(CustomPropertyName.PermittedPixelError))
				{
                    string attrValue = series[CustomPropertyName.PermittedPixelError];

                    float pixelError;
                    bool parseSucceed = float.TryParse(attrValue, NumberStyles.Any, CultureInfo.CurrentCulture, out pixelError);

                    if (parseSucceed)
                    {
                        permittedPixelError = pixelError;
                    }
                    else
                    {
                        throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid2("PermittedPixelError")));
                    }

                    // "PermittedPixelError" attribute value should be in range from zero to 1
                    if (permittedPixelError < 0f || permittedPixelError > 1f)
                    {
                        throw (new InvalidOperationException(SR.ExceptionCustomAttributeIsNotInRange0to1("PermittedPixelError")));
                    }
				}

				// Get pixel size in axes coordinates
				SizeF pixelSize = graph.GetRelativeSize(new SizeF(permittedPixelError, permittedPixelError));
				SizeF axesMin = graph.GetRelativeSize(new SizeF((float)hAxisMin, (float)vAxisMin));
				double axesValuesPixelSizeX = Math.Abs(hAxis.PositionToValue(axesMin.Width + pixelSize.Width, false) - hAxis.PositionToValue(axesMin.Width, false));
				double axesValuesPixelSizeY = Math.Abs(vAxis.PositionToValue(axesMin.Height + pixelSize.Height, false) - vAxis.PositionToValue(axesMin.Height, false));

				// Create point marker brush
				SolidBrush	markerBrush = new SolidBrush( ((series.MarkerColor.IsEmpty) ? series.Color : series.MarkerColor) );
				SolidBrush	emptyMarkerBrush = new SolidBrush( ((series.EmptyPointStyle.MarkerColor.IsEmpty) ? series.EmptyPointStyle.Color : series.EmptyPointStyle.MarkerColor) );

				// Create point marker border pen
				Pen	borderPen = null;
				Pen	emptyBorderPen = null;
				if(!series.MarkerBorderColor.IsEmpty && series.MarkerBorderWidth > 0)
				{
					borderPen = new Pen(series.MarkerBorderColor, series.MarkerBorderWidth);
				}
				if(!series.EmptyPointStyle.MarkerBorderColor.IsEmpty && series.EmptyPointStyle.MarkerBorderWidth > 0)
				{
					emptyBorderPen = new Pen(series.EmptyPointStyle.MarkerBorderColor, series.EmptyPointStyle.MarkerBorderWidth);
				}

				// Check if series is indexed
				bool indexedSeries = ChartHelper.IndexedSeries(this.Common, series.Name );

                // Get marker size taking in consideration current DPIs
                int markerSize = series.MarkerSize;
                if (graph != null && graph.Graphics != null)
                {
                    // Marker size is in pixels and we do the mapping for higher DPIs
                    markerSize = (int)Math.Max(markerSize * graph.Graphics.DpiX / 96, markerSize * graph.Graphics.DpiY / 96);
                }

				// Loop through all ponts in the series
				int		index = 0;
				double	xValue = 0.0;
				double	yValue = 0.0;
				double	xValuePrev = 0.0;
				double	yValuePrev = 0.0;
				PointF	currentPoint = PointF.Empty;
				bool	currentPointIsEmpty = false;
				double	xPixelConverter = (graph.Common.ChartPicture.Width - 1.0) / 100.0;
				double	yPixelConverter = (graph.Common.ChartPicture.Height - 1.0) / 100.0;
				MarkerStyle	markerStyle = series.MarkerStyle;
				MarkerStyle	emptyMarkerStyle = series.EmptyPointStyle.MarkerStyle;
				foreach( DataPoint point in series.Points )
				{
					// Get point X and Y values
					xValue = (indexedSeries) ? index + 1 : point.XValue;
					xValue = hAxis.GetLogValue(xValue);
					yValue = vAxis.GetLogValue(point.YValues[0]);
					currentPointIsEmpty = point.IsEmpty;

					// Check if point is completly out of the data scaleView
					if( xValue < hAxisMin ||
						xValue > hAxisMax ||
						yValue < vAxisMin ||
						yValue > vAxisMax )
					{
						xValuePrev = xValue;
						yValuePrev = yValue;
						++index;
						continue;
					}

					// Check if point may be skipped
					if(index > 0)
					{
						// Check if current point location is in the specified distance from the 
						// preious data location.
						if(Math.Abs(xValue - xValuePrev) < axesValuesPixelSizeX &&
							Math.Abs(yValue - yValuePrev) < axesValuesPixelSizeY)
						{
							// Increase counter and proceed to the next data point
							++index;
							continue;
						}
					}

					// Get point pixel position
					currentPoint.X = (float)
						(hAxis.GetLinearPosition( xValue ) * xPixelConverter);
					currentPoint.Y = (float)
						(vAxis.GetLinearPosition( yValue ) * yPixelConverter);

					// Draw point marker
                    MarkerStyle	currentMarkerStyle = (currentPointIsEmpty) ? emptyMarkerStyle : markerStyle;
                    if(currentMarkerStyle != MarkerStyle.None)
                    {
					    this.DrawMarker(
						    graph,
						    point,
						    index,
						    currentPoint,
                            currentMarkerStyle,
						    markerSize,
						    (currentPointIsEmpty) ? emptyMarkerBrush : markerBrush,
						    (currentPointIsEmpty) ? emptyBorderPen : borderPen);
                    }

					// Remember last point coordinates
					xValuePrev = xValue;
					yValuePrev = yValue;
					++index;
				}

				// Dispose used brushes and pens
				markerBrush.Dispose();
				emptyMarkerBrush.Dispose();
				if(borderPen != null)
				{
					borderPen.Dispose();
				}
				if(emptyBorderPen != null)
				{
					emptyBorderPen.Dispose();
				}
			}
		}

		/// <summary>
		/// Draws a marker that represents a data point in FastPoint series.
		/// </summary>
		/// <param name="graph">Chart graphics used to draw the marker.</param>
		/// <param name="point">Series data point drawn.</param>
		/// <param name="pointIndex">Data point index in the series.</param>
		/// <param name="location">Marker location in pixels.</param>
		/// <param name="markerStyle">Marker style.</param>
		/// <param name="markerSize">Marker size in pixels.</param>
		/// <param name="brush">Brush used to fill marker shape.</param>
		/// <param name="borderPen">Marker border pen.</param>
		virtual protected void DrawMarker(
			ChartGraphics graph, 
			DataPoint point, 
			int pointIndex, 
			PointF location, 
			MarkerStyle markerStyle, 
			int markerSize, 
			Brush brush,
			Pen borderPen)
		{
			// Transform 3D coordinates
			if(chartArea3DEnabled)
			{
				Point3D [] points = new Point3D[1];
				location = graph.GetRelativePoint(location);
				points[0] = new Point3D(location.X, location.Y, this.seriesZCoordinate);
				matrix3D.TransformPoints( points );
				location.X = points[0].X;
				location.Y = points[0].Y;
				location = graph.GetAbsolutePoint(location);
			}

			// Create marker bounding rectangle in pixels
			RectangleF markerBounds = new RectangleF(
				location.X - markerSize / 2f, location.Y - markerSize / 2f, markerSize, markerSize);

			// Draw Marker
			switch(markerStyle)
			{
				case(MarkerStyle.Star4):
				case(MarkerStyle.Star5):
				case(MarkerStyle.Star6):
				case(MarkerStyle.Star10):
				{
					// Set number of corners
					int cornerNumber = 4;
					if(markerStyle == MarkerStyle.Star5)
					{
						cornerNumber = 5;
					}
					else if(markerStyle == MarkerStyle.Star6)
					{
						cornerNumber = 6;
					}
					else if(markerStyle == MarkerStyle.Star10)
					{
						cornerNumber = 10;
					}

					// Get star polygon
					PointF[]	points = graph.CreateStarPolygon(markerBounds, cornerNumber);

					// Fill shape
					graph.FillPolygon(brush, points);

					// Draw border
					if(borderPen != null)
					{
						graph.DrawPolygon(borderPen, points);
					}
					break;
				}
				case(MarkerStyle.Circle):
				{
					graph.FillEllipse(brush, markerBounds);

					// Draw border
					if(borderPen != null)
					{
						graph.DrawEllipse(borderPen, markerBounds);
					}

					break;
				}
				case(MarkerStyle.Square):
				{
					graph.FillRectangle(brush, markerBounds);

					// Draw border
					if(borderPen != null)
					{
						graph.DrawRectangle(
							borderPen, 
							(int)Math.Round(markerBounds.X, 0), 
							(int)Math.Round(markerBounds.Y, 0), 
							(int)Math.Round(markerBounds.Width, 0), 
							(int)Math.Round(markerBounds.Height, 0) );
					}

					break;
				}
				case(MarkerStyle.Cross):
				{
					// Calculate cross line width and size
					float crossLineWidth = (float)Math.Ceiling(markerSize/4F);
					float crossSize = markerSize;	// * (float)Math.Sin(45f/180f*Math.PI);

					// Calculate cross coordinates
					PointF[] points = new PointF[12];
					points[0].X = location.X - crossSize/2F;
					points[0].Y = location.Y + crossLineWidth/2F;
					points[1].X = location.X - crossSize/2F;
					points[1].Y = location.Y - crossLineWidth/2F;

					points[2].X = location.X - crossLineWidth/2F;
					points[2].Y = location.Y - crossLineWidth/2F;
					points[3].X = location.X - crossLineWidth/2F;
					points[3].Y = location.Y - crossSize/2F;
					points[4].X = location.X + crossLineWidth/2F;
					points[4].Y = location.Y - crossSize/2F;

					points[5].X = location.X + crossLineWidth/2F;
					points[5].Y = location.Y - crossLineWidth/2F;
					points[6].X = location.X + crossSize/2F;
					points[6].Y = location.Y - crossLineWidth/2F;
					points[7].X = location.X + crossSize/2F;
					points[7].Y = location.Y + crossLineWidth/2F;

					points[8].X = location.X + crossLineWidth/2F;
					points[8].Y = location.Y + crossLineWidth/2F;
					points[9].X = location.X + crossLineWidth/2F;
					points[9].Y = location.Y + crossSize/2F;
					points[10].X = location.X - crossLineWidth/2F;
					points[10].Y = location.Y + crossSize/2F;
					points[11].X = location.X - crossLineWidth/2F;
					points[11].Y = location.Y + crossLineWidth/2F;

					// Rotate cross coordinates 45 degrees
					Matrix rotationMatrix = new Matrix();
					rotationMatrix.RotateAt(45, location);
					rotationMatrix.TransformPoints(points);
					rotationMatrix.Dispose();

					// Fill shape
					graph.FillPolygon(brush, points);

					// Draw border
					if(borderPen != null)
					{
						graph.DrawPolygon(borderPen, points);
					}
					break;
				}
				case(MarkerStyle.Diamond):
				{
					PointF[] points = new PointF[4];
					points[0].X = markerBounds.X;
					points[0].Y = markerBounds.Y + markerBounds.Height/2F;
					points[1].X = markerBounds.X + markerBounds.Width/2F;
					points[1].Y = markerBounds.Top;
					points[2].X = markerBounds.Right;
					points[2].Y = markerBounds.Y + markerBounds.Height/2F;
					points[3].X = markerBounds.X + markerBounds.Width/2F;
					points[3].Y = markerBounds.Bottom;

					graph.FillPolygon(brush, points);

					// Draw border
					if(borderPen != null)
					{
						graph.DrawPolygon(borderPen, points);
					}
					break;
				}
				case(MarkerStyle.Triangle):
				{
					PointF[] points = new PointF[3];
					points[0].X = markerBounds.X;
					points[0].Y = markerBounds.Bottom;
					points[1].X = markerBounds.X + markerBounds.Width/2F;
					points[1].Y = markerBounds.Top;
					points[2].X = markerBounds.Right;
					points[2].Y = markerBounds.Bottom;

					graph.FillPolygon(brush, points);

					// Draw border
					if(borderPen != null)
					{
						graph.DrawPolygon(borderPen, points);
					}
					break;
				}
				default:
				{
                    throw (new InvalidOperationException(SR.ExceptionFastPointMarkerStyleUnknown));
				}
			}
			
			// Process selection regions
			if( this.Common.ProcessModeRegions )
			{
				this.Common.HotRegionsList.AddHotRegion(
					graph.GetRelativeRectangle(markerBounds),
					point, 
					point.series.Name, 
					pointIndex );
			}
		}

		#endregion

		#region Y values related methods

		/// <summary>
		/// Helper function, which returns the Y value of the location.
		/// </summary>
		/// <param name="common">Chart common elements.</param>
		/// <param name="area">Chart area the series belongs to.</param>
		/// <param name="series">Sereis of the location.</param>
		/// <param name="point">Point object.</param>
		/// <param name="pointIndex">Index of the location.</param>
		/// <param name="yValueIndex">Index of the Y value to get.</param>
		/// <returns>Y value of the location.</returns>
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
			// Fast Point chart type do not support labels
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
