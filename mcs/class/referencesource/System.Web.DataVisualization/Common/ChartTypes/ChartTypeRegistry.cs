//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartTypeRegistry.cs
//
//  Namespace:	DataVisualization.Charting.ChartTypes
//
//	Classes:	ChartTypeRegistry, IChartType
//
//  Purpose:	ChartTypeRegistry is a repository for all standard 
//              and custom chart types. Each chart type has unique 
//              name and IChartType derived class which provides
//              behaviour information about the chart type and
//              also contains drwaing functionality.
//
//              ChartTypeRegistry can be used by user for custom 
//              chart type registering and can be retrieved using 
//              Chart.GetService(typeof(ChartTypeRegistry)) method.
//
//	Reviewed:	AG - Aug 6, 2002
//              AG - Microsoft 6, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Resources;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Reflection;
using System.Drawing;

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
    /// ChartTypeName class contains constant strings defining
    /// names of all ChartTypes used in the Chart.
    /// </summary>
    internal static class ChartTypeNames
    {
        #region Chart type names

        internal const string Area = "Area";
        internal const string RangeBar = "RangeBar";
        internal const string Bar = "Bar";
        internal const string SplineArea = "SplineArea";
        internal const string BoxPlot = "BoxPlot";
        internal const string Bubble = "Bubble";
        internal const string Column = "Column";
        internal const string RangeColumn = "RangeColumn";
        internal const string Doughnut = "Doughnut";
        internal const string ErrorBar = "ErrorBar";
        internal const string FastLine = "FastLine";
        internal const string FastPoint = "FastPoint";
        internal const string Funnel = "Funnel";
        internal const string Pyramid = "Pyramid";
        internal const string Kagi = "Kagi";
        internal const string Spline = "Spline";
        internal const string Line = "Line";
        internal const string PointAndFigure = "PointAndFigure";
        internal const string Pie = "Pie";
        internal const string Point = "Point";
        internal const string Polar = "Polar";
        internal const string Radar = "Radar";
        internal const string SplineRange = "SplineRange";
        internal const string Range = "Range";
        internal const string Renko = "Renko";
        internal const string OneHundredPercentStackedArea = "100%StackedArea";
        internal const string StackedArea = "StackedArea";
        internal const string OneHundredPercentStackedBar = "100%StackedBar";
        internal const string StackedBar = "StackedBar";
        internal const string OneHundredPercentStackedColumn = "100%StackedColumn";
        internal const string StackedColumn = "StackedColumn";
        internal const string StepLine = "StepLine";
        internal const string Candlestick = "Candlestick";
        internal const string Stock = "Stock";
        internal const string ThreeLineBreak = "ThreeLineBreak";

        #endregion // Keyword Names
    }

	/// <summary>
	/// ChartTypeRegistry class is a repository for all standard and custom 
    /// chart types. In order for the chart control to display the chart 
    /// type, it first must be registered using unique name and IChartType 
    /// derived class which provides the description of the chart type and 
    /// also responsible for all drawing and hit testing.
    /// 
    /// ChartTypeRegistry can be used by user for custom chart type registering 
    /// and can be retrieved using Chart.GetService(typeof(ChartTypeRegistry)) 
    /// method.
	/// </summary>
    internal class ChartTypeRegistry : IServiceProvider, IDisposable
	{
		#region Fields

		// Chart types image resource manager
		private		ResourceManager		_resourceManager = null;

		// Storage for registered/created chart types
        internal    Hashtable           registeredChartTypes = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private     Hashtable           _createdChartTypes = new Hashtable(StringComparer.OrdinalIgnoreCase);

		#endregion

		#region Constructor and Services

		/// <summary>
		/// Chart types registry public constructor.
		/// </summary>
		public ChartTypeRegistry()
		{
		}

		/// <summary>
		/// Returns chart type registry service object.
		/// </summary>
		/// <param name="serviceType">Service type to get.</param>
		/// <returns>Chart type registry service.</returns>
		[EditorBrowsableAttribute(EditorBrowsableState.Never)]
		object IServiceProvider.GetService(Type serviceType)
		{
			if(serviceType == typeof(ChartTypeRegistry))
			{
				return this;
			}
            throw (new ArgumentException(SR.ExceptionChartTypeRegistryUnsupportedType( serviceType.ToString() ) ) );
		}

		#endregion

		#region Registry methods

		/// <summary>
		/// Adds chart type into the registry.
		/// </summary>
		/// <param name="name">Chart type name.</param>
		/// <param name="chartType">Chart class type.</param>
		public void Register(string name, Type chartType)
		{
			// First check if chart type with specified name already registered
			if(registeredChartTypes.Contains(name))
			{
				// If same type provided - ignore
				if(registeredChartTypes[name].GetType() == chartType)
				{
					return;
				}

				// Error - throw exception
				throw( new ArgumentException( SR.ExceptionChartTypeNameIsNotUnique( name ) ) );
			}

			// Make sure that specified class support IChartType interface
			bool	found = false;
			Type[]	interfaces = chartType.GetInterfaces();
			foreach(Type type in interfaces)
			{   
				if(type == typeof(IChartType))
				{
					found = true;
					break;
				}
			}
			if(!found)
			{
                throw (new ArgumentException(SR.ExceptionChartTypeHasNoInterface ));
			}

			// Add chart type to the hash table
			registeredChartTypes[name] = chartType;
		}

		/// <summary>
		/// Returns chart type object by name.
		/// </summary>
		/// <param name="chartType">Chart type.</param>
		/// <returns>Chart type object derived from IChartType.</returns>
		public IChartType GetChartType(SeriesChartType chartType)
		{
			return this.GetChartType(Series.GetChartTypeName(chartType));
		}

		/// <summary>
		/// Returns chart type object by name.
		/// </summary>
		/// <param name="name">Chart type name.</param>
		/// <returns>Chart type object derived from IChartType.</returns>
		public IChartType GetChartType(string name)
		{
			// First check if chart type with specified name registered
			if(!registeredChartTypes.Contains(name))
			{
				throw( new ArgumentException( SR.ExceptionChartTypeUnknown( name ) ) );
			}

			// Check if the chart type object is already created
			if(!_createdChartTypes.Contains(name))
			{	
				// Create chart type object
				_createdChartTypes[name] = 
					((Type)registeredChartTypes[name]).Assembly.
					CreateInstance(((Type)registeredChartTypes[name]).ToString());
			}

			return (IChartType)_createdChartTypes[name];
		}

		/// <summary>
		/// Chart images resource manager.
		/// </summary>
		public ResourceManager	ResourceManager
		{
			get
			{
				// Create chart images resource manager
				if(_resourceManager == null)
				{
                    _resourceManager = new ResourceManager(typeof(Chart).Namespace + ".Design", Assembly.GetExecutingAssembly());
				}
				return _resourceManager;
			}
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
                // Dispose managed resource
                foreach (string name in this._createdChartTypes.Keys)
                {
                    IChartType chartType = (IChartType)_createdChartTypes[name];
                    chartType.Dispose();
                }
                this._createdChartTypes.Clear();
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
	/// IChartType interface must be implemented for any standard or custom 
    /// chart type displayed in the chart control. This interface defines 
    /// properties which provide information on chart type behaviour including 
    /// how many Y values supported, is it a stacked chart type, how it 
    /// interacts with axes and much more.
    /// 
    /// IChartType interface methods define how to draw series data point, 
    /// calculate Y values and process SmartLabelStyle.
	/// </summary>
    internal interface IChartType : IDisposable
	{
		#region Properties

		/// <summary>
		/// Chart type name
		/// </summary>
		string Name			{ get; }

		/// <summary>
		/// Gets chart type image
		/// </summary>
		/// <param name="registry">Chart types registry object.</param>
		/// <returns>Chart type image.</returns>
        System.Drawing.Image GetImage(ChartTypeRegistry registry);

		/// <summary>
		/// True if chart type is stacked
		/// </summary>
		bool Stacked		{ get; }


		/// <summary>
		/// True if stacked chart type supports groups
		/// </summary>
		bool SupportStackedGroups	{ get; }


		/// <summary>
		/// True if stacked chart type should draw separately positive and 
		/// negative data points ( Bar and column Stacked types ).
		/// </summary>
		bool StackSign		{ get; }

		/// <summary>
		/// True if chart type supports axeses
		/// </summary>
		bool RequireAxes	{ get; }

		/// <summary>
		/// True if chart type requires circular chart area.
		/// </summary>
		bool CircularChartArea	{ get; }

		/// <summary>
		/// True if chart type supports logarithmic axes
		/// </summary>
		bool SupportLogarithmicAxes	{ get; }

		/// <summary>
		/// True if chart type requires to switch the value (Y) axes position
		/// </summary>
		bool SwitchValueAxes	{ get; }

		/// <summary>
		/// True if chart series can be placed side-by-side.
		/// </summary>
		bool SideBySideSeries	{ get; }

		/// <summary>
		/// True if each data point of a chart must be represented in the legend
		/// </summary>
		bool DataPointsInLegend	{ get; }

		/// <summary>
		/// True if palette colors should be applied for each data paoint.
		/// Otherwise the color is applied to the series.
		/// </summary>
		bool ApplyPaletteColorsToPoints	{ get; }

		/// <summary>
		/// Indicates that extra Y values are connected to the scale of the Y axis
		/// </summary>
		bool ExtraYValuesConnectedToYAxis{ get; }

		/// <summary>
		/// If the crossing value is auto Crossing value should be 
		/// automatically set to zero for some chart 
		/// types (Bar, column, area etc.)
		/// </summary>
		bool ZeroCrossing { get; }

		/// <summary>
		/// Number of supported Y value(s) per point 
		/// </summary>
		int YValuesPerPoint{ get; }

		/// <summary>
		/// Chart type with two y values used for scale ( bubble chart type )
		/// </summary>
		bool SecondYScale{ get; }

		/// <summary>
		/// Indicates that it's a hundredred percent chart.
		/// Axis scale from 0 to 100 percent should be used.
		/// </summary>
		bool HundredPercent{ get; }

		/// <summary>
		/// Indicates that negative 100% stacked values are shown on
		/// the other side of the X axis
		/// </summary>
		bool HundredPercentSupportNegative{ get; }

		/// <summary>
		/// How to draw series/points in legend:
		/// Filled rectangle, Line or Marker
		/// </summary>
		/// <param name="series">Legend item series.</param>
		/// <returns>Legend item style.</returns>
		LegendImageStyle GetLegendImageStyle(Series series);

		#endregion

		#region Painting and Selection methods

		/// <summary>
		/// Draw chart on specified chart graphics.
		/// </summary>
		/// <param name="graph">Chart grahhics object.</param>
		/// <param name="common">Common elements.</param>
		/// <param name="area">Chart area to draw on.</param>
		/// <param name="seriesToDraw">Chart series to draw.</param>
        void Paint(ChartGraphics graph, CommonElements common, ChartArea area, Series seriesToDraw);
		
		#endregion

		#region Y values methods

		/// <summary>
		/// Helper function, which returns the Y value of the data point.
		/// </summary>
		/// <param name="common">Chart common elements.</param>
		/// <param name="area">Chart area the series belongs to.</param>
		/// <param name="series">Sereis of the point.</param>
		/// <param name="point">Point object.</param>
		/// <param name="pointIndex">Index of the point.</param>
		/// <param name="yValueIndex">Index of the Y value to get.</param>
		/// <returns>Y value of the point.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "5#y")]
		double GetYValue(CommonElements common, ChartArea area, Series series, DataPoint point, int pointIndex, int yValueIndex);
		
		#endregion

		#region SmartLabelStyle methods

		/// <summary>
		/// Adds markers position to the list. Used to check SmartLabelStyle overlapping.
		/// </summary>
		/// <param name="common">Common chart elements.</param>
		/// <param name="area">Chart area.</param>
		/// <param name="series">Series values to be used.</param>
		/// <param name="list">List to add to.</param>
		void AddSmartLabelMarkerPositions(CommonElements common, ChartArea area, Series series, ArrayList list);

		#endregion
	}
}
