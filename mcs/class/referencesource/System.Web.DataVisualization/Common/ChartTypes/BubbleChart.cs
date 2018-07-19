//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		BubbleChart.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	BubbleChart
//
//  Purpose:	Bubble chart type is similar to the Point chart 
//              where each data point is presented with a marker 
//              positioned using X and Y values. The difference 
//              of the Bubble chart is that an additional Y value 
//              is used to control the size of the marker.
//
//	Reviewed:	AG - August 6, 2002
//              AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Resources;
using System.Reflection;
using System.Drawing;
using System.Globalization;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;

#else
using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
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
	/// BubbleChart class extends PointChart class to add support for
    /// additional Y value which controls the size of the markers used.
	/// </summary>
	internal class BubbleChart : PointChart
	{
		#region Fields and Constructor

		// Indicates that bubble size scale is calculated
		private bool			_scaleDetected = false;
	
		// Minimum/Maximum bubble size
		private double			_maxPossibleBubbleSize = 15F;
		private double			_minPossibleBubbleSize = 3F;
		private float			_maxBubleSize = 0f;
		private float			_minBubleSize = 0f;

		// Current min/max size of the bubble size
		private double			_minAll = double.MaxValue;
		private double			_maxAll = double.MinValue;


		// Bubble size difference value
		private double	_valueDiff = 0;
		private double	_valueScale = 1;

		/// <summary>
		/// Class public constructor
		/// </summary>
		public BubbleChart() : base(true)
		{
		}

		#endregion

		#region IChartType interface implementation

		/// <summary>
		/// Chart type name
		/// </summary>
		override public string Name			{ get{ return ChartTypeNames.Bubble;}}

		/// <summary>
		/// Number of supported Y value(s) per point 
		/// </summary>
		override public int YValuesPerPoint	{ get { return 2; } }

		/// <summary>
		/// Chart type with two y values used for scale ( bubble chart type )
		/// </summary>
		override public bool SecondYScale{ get{ return true;} }

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

		#region Bubble chart methods

		/// <summary>
		/// This method recalculates size of the bars. This method is used 
		/// from Paint or Select method.
		/// </summary>
		/// <param name="selection">If True selection mode is active, otherwise paint mode is active</param>
		/// <param name="graph">The Chart Graphics object</param>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
		override protected void ProcessChartType( 
			bool selection, 
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			Series seriesToDraw )
		{
			_scaleDetected = false;
			base.ProcessChartType(selection, graph, common, area, seriesToDraw );
		}

		/// <summary>
		/// Gets marker border size.
		/// </summary>
		/// <param name="point">Data point.</param>
		/// <returns>Marker border size.</returns>
        override protected int GetMarkerBorderSize(DataPointCustomProperties point)
		{
			if(point.series != null)
			{
				return point.series.BorderWidth;
			}

			return 1;
		}

		/// <summary>
		/// Returns marker size.
		/// </summary>
		/// <param name="graph">The Chart Graphics object.</param>
		/// <param name="common">The Common elements object.</param>
		/// <param name="area">Chart area for this chart.</param>
		/// <param name="point">Data point.</param>
		/// <param name="markerSize">Marker size.</param>
		/// <param name="markerImage">Marker image.</param>
		/// <returns>Marker width and height.</returns>
		override protected SizeF GetMarkerSize(
			ChartGraphics graph, 
			CommonElements common, 
			ChartArea area, 
			DataPoint point, 
			int markerSize, 
			string markerImage)
		{
			// Check required Y values number
			if(point.YValues.Length < this.YValuesPerPoint)
			{
				throw(new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues(this.Name, this.YValuesPerPoint.ToString(CultureInfo.InvariantCulture))));
			}

			// Marker size
			SizeF size = new SizeF(markerSize, markerSize);
            if (graph != null && graph.Graphics != null)
            {
                // Marker size is in pixels and we do the mapping for higher DPIs
                size.Width = markerSize * graph.Graphics.DpiX / 96;
                size.Height = markerSize * graph.Graphics.DpiY / 96;
            }

			// Check number of Y values for non empty points
			if(point.series.YValuesPerPoint > 1 && !point.IsEmpty)
			{
				// Scale Y values
				size.Width = ScaleBubbleSize(graph, common, area, point.YValues[1]);
				size.Height = ScaleBubbleSize(graph, common, area, point.YValues[1]);
			}

			return size;
		}
		
		/// <summary>
		/// Scales the value used to determine the size of the Bubble.
		/// </summary>
		/// <param name="graph">The Chart Graphics object</param>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="value">Value to scale.</param>
		/// <returns>Scaled values.</returns>
		private float ScaleBubbleSize(ChartGraphics graph, CommonElements common, ChartArea area, double value)
		{
			// Check if scaling numbers are detected
			if(!_scaleDetected)
			{
				// Try to find bubble size scale in the custom series properties
				_minAll = double.MaxValue;
				_maxAll = double.MinValue;
				foreach( Series ser in common.DataManager.Series )
				{
					if( String.Compare( ser.ChartTypeName, this.Name, true, System.Globalization.CultureInfo.CurrentCulture) == 0 &&
						ser.ChartArea == area.Name &&
						ser.IsVisible())
					{
						// Check if custom properties are set to specify scale
						if(ser.IsCustomPropertySet(CustomPropertyName.BubbleScaleMin))
						{
							_minAll = Math.Min(_minAll, CommonElements.ParseDouble(ser[CustomPropertyName.BubbleScaleMin]));
						}
						if(ser.IsCustomPropertySet(CustomPropertyName.BubbleScaleMax))
						{
							_maxAll = Math.Max(_maxAll, CommonElements.ParseDouble(ser[CustomPropertyName.BubbleScaleMax]));
						}

						// Check if attribute for max. size is set
						if(ser.IsCustomPropertySet(CustomPropertyName.BubbleMaxSize))
						{
							_maxPossibleBubbleSize = CommonElements.ParseDouble(ser[CustomPropertyName.BubbleMaxSize]);
							if(_maxPossibleBubbleSize < 0 || _maxPossibleBubbleSize > 100)
							{
								throw(new ArgumentException(SR.ExceptionCustomAttributeIsNotInRange0to100("BubbleMaxSize")));
							}
						}

						// Check if attribute for min. size is set
						if(ser.IsCustomPropertySet(CustomPropertyName.BubbleMinSize))
						{
							_minPossibleBubbleSize = CommonElements.ParseDouble(ser[CustomPropertyName.BubbleMinSize]);
							if(_minPossibleBubbleSize < 0 || _minPossibleBubbleSize > 100)
							{
								throw(new ArgumentException(SR.ExceptionCustomAttributeIsNotInRange0to100("BubbleMinSize")));
							}
						}


						// Check if custom properties set to use second Y value (bubble size) as label text
						labelYValueIndex = 0;
						if(ser.IsCustomPropertySet(CustomPropertyName.BubbleUseSizeForLabel))
						{
							if(String.Compare(ser[CustomPropertyName.BubbleUseSizeForLabel], "true", StringComparison.OrdinalIgnoreCase) == 0)
							{
								labelYValueIndex = 1;
								break;
							}
						}
					}
				}

				// Scale values are not specified - auto detect
				if(_minAll == double.MaxValue || _maxAll == double.MinValue)
				{
					double	minSer = double.MaxValue;
					double	maxSer = double.MinValue;
					foreach( Series ser in common.DataManager.Series )
					{
						if( ser.ChartTypeName == this.Name && ser.ChartArea == area.Name && ser.IsVisible() )
						{
							foreach(DataPoint point in ser.Points)
							{
                                if (!point.IsEmpty)
                                {
                                    // Check required Y values number
                                    if (point.YValues.Length < this.YValuesPerPoint)
                                    {
                                        throw (new InvalidOperationException(SR.ExceptionChartTypeRequiresYValues(this.Name, this.YValuesPerPoint.ToString(CultureInfo.InvariantCulture))));
                                    }

                                    minSer = Math.Min(minSer, point.YValues[1]);
                                    maxSer = Math.Max(maxSer, point.YValues[1]);
                                }
							}
						}
					}
					if(_minAll == double.MaxValue)
					{
						_minAll = minSer;
					}
					if(_maxAll == double.MinValue)
					{
						_maxAll = maxSer;
					}
				}

				// Calculate maximum bubble size
				SizeF	areaSize = graph.GetAbsoluteSize(area.PlotAreaPosition.Size);
				_maxBubleSize = (float)(Math.Min(areaSize.Width, areaSize.Height) / (100.0/_maxPossibleBubbleSize));
				_minBubleSize = (float)(Math.Min(areaSize.Width, areaSize.Height) / (100.0/_minPossibleBubbleSize));

				// Calculate scaling variables depending on the Min/Max values
				if(_maxAll == _minAll)
				{
					this._valueScale = 1;
					this._valueDiff = _minAll - (_maxBubleSize - _minBubleSize)/2f;
				}
				else
				{
					this._valueScale = (_maxBubleSize - _minBubleSize) / (_maxAll - _minAll);
					this._valueDiff = _minAll;
				}

				_scaleDetected = true;
			}

			// Check if value do not exceed Min&Max
			if(value > _maxAll)
			{
				return 0F;
			}
			if(value < _minAll)
			{
				return 0F;
			}

			// Return scaled value
			return (float)((value - this._valueDiff) * this._valueScale) + _minBubleSize;
		}

		/// <summary>
		/// Scales the value used to determine the size of the Bubble.
		/// </summary>
		/// <param name="common">The Common elements object</param>
		/// <param name="area">Chart area for this chart</param>
		/// <param name="value">Value to scale.</param>
		/// <param name="yValue">True if Y value is calculated, false if X.</param>
		/// <returns>Scaled values.</returns>
		static internal double AxisScaleBubbleSize(CommonElements common, ChartArea area, double value, bool yValue )
		{
			
			// Try to find bubble size scale in the custom series properties
			double minAll = double.MaxValue;
			double maxAll = double.MinValue;
			double maxPossibleBubbleSize = 15F;
			double minPossibleBubbleSize = 3F;
			float maxBubleSize;
			float minBubleSize;
			double valueScale;
			double valueDiff;
			foreach( Series ser in common.DataManager.Series )
			{
				if( String.Compare( ser.ChartTypeName, ChartTypeNames.Bubble, StringComparison.OrdinalIgnoreCase) == 0 &&
					ser.ChartArea == area.Name &&
					ser.IsVisible())
				{
					// Check if custom properties are set to specify scale
					if(ser.IsCustomPropertySet(CustomPropertyName.BubbleScaleMin))
					{
						minAll = Math.Min(minAll, CommonElements.ParseDouble(ser[CustomPropertyName.BubbleScaleMin]));
					}
					if(ser.IsCustomPropertySet(CustomPropertyName.BubbleScaleMax))
					{
						maxAll = Math.Max(maxAll, CommonElements.ParseDouble(ser[CustomPropertyName.BubbleScaleMax]));
					}

					// Check if attribute for max. size is set
					if(ser.IsCustomPropertySet(CustomPropertyName.BubbleMaxSize))
					{
						maxPossibleBubbleSize = CommonElements.ParseDouble(ser[CustomPropertyName.BubbleMaxSize]);
						if(maxPossibleBubbleSize < 0 || maxPossibleBubbleSize > 100)
						{
							throw(new ArgumentException(SR.ExceptionCustomAttributeIsNotInRange0to100("BubbleMaxSize")));
						}
					}

					// Check if custom properties set to use second Y value (bubble size) as label text
					if(ser.IsCustomPropertySet(CustomPropertyName.BubbleUseSizeForLabel))
					{
						if(String.Compare(ser[CustomPropertyName.BubbleUseSizeForLabel], "true", StringComparison.OrdinalIgnoreCase) == 0)
						{
							break;
						}
					}
				}
			}

			// Scale values are not specified - auto detect
            double minimum = double.MaxValue;
            double maximum = double.MinValue;
			double	minSer = double.MaxValue;
			double	maxSer = double.MinValue;
			foreach( Series ser in common.DataManager.Series )
			{
				if( String.Compare(ser.ChartTypeName, ChartTypeNames.Bubble, StringComparison.OrdinalIgnoreCase) == 0 
                    && ser.ChartArea == area.Name 
                    && ser.IsVisible() )
				{
					foreach(DataPoint point in ser.Points)
					{
                        if (!point.IsEmpty)
                        {
                            minSer = Math.Min(minSer, point.YValues[1]);
                            maxSer = Math.Max(maxSer, point.YValues[1]);

                            if (yValue)
                            {
                                minimum = Math.Min(minimum, point.YValues[0]);
                                maximum = Math.Max(maximum, point.YValues[0]);
                            }
                            else
                            {
                                minimum = Math.Min(minimum, point.XValue);
                                maximum = Math.Max(maximum, point.XValue);
                            }
                        }
					}
				}
			}
			if(minAll == double.MaxValue)
			{
				minAll = minSer;
			}
			if(maxAll == double.MinValue)
			{
				maxAll = maxSer;
			}

			// Calculate maximum bubble size
			maxBubleSize = (float)( (maximum - minimum) / (100.0/maxPossibleBubbleSize));
			minBubleSize = (float)( (maximum - minimum) / (100.0/minPossibleBubbleSize));

			// Calculate scaling variables depending on the Min/Max values
			if(maxAll == minAll)
			{
				valueScale = 1;
				valueDiff = minAll - (maxBubleSize - minBubleSize)/2f;
			}
			else
			{
				valueScale = (maxBubleSize - minBubleSize) / (maxAll - minAll);
				valueDiff = minAll;
			}

			
			// Check if value do not exceed Min&Max
			if(value > maxAll)
			{
				return 0F;
			}
			if(value < minAll)
			{
				return 0F;
			}

			// Return scaled value
			return (float)((value - valueDiff) * valueScale) + minBubleSize;
		}

		/// <summary>
		/// Get value from custom attribute BubbleMaxSize 
		/// </summary>
		/// <param name="area">Chart Area</param>
		/// <returns>Bubble Max size</returns>
		static internal double GetBubbleMaxSize( ChartArea area )
		{
			double maxPossibleBubbleSize = 15;
			// Try to find bubble size scale in the custom series properties
			foreach( Series ser in area.Common.DataManager.Series )
			{
				if( String.Compare( ser.ChartTypeName, ChartTypeNames.Bubble, StringComparison.OrdinalIgnoreCase) == 0 &&
					ser.ChartArea == area.Name &&
					ser.IsVisible())
				{
					// Check if attribute for max. size is set
					if(ser.IsCustomPropertySet(CustomPropertyName.BubbleMaxSize))
					{
						maxPossibleBubbleSize = CommonElements.ParseDouble(ser[CustomPropertyName.BubbleMaxSize]);
						if(maxPossibleBubbleSize < 0 || maxPossibleBubbleSize > 100)
						{
							throw(new ArgumentException(SR.ExceptionCustomAttributeIsNotInRange0to100("BubbleMaxSize")));
						}
					}
				}
			}

			return maxPossibleBubbleSize / 100;
		}
	
		#endregion
	}
}
