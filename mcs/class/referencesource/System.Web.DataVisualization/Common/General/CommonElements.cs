//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		CommonElements.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	CommonElements
//
//  Purpose:	CommonElements class provides references to common 
//              chart classes like DataManager, ChartTypeRegistry, 
//              ImageLoader and others. It is passed to different 
//              chart elements to simplify access to those common 
//              classes.
//
//	Reviewed:	GS - August 2, 2002
//				AG - August 8, 2002
//              AG - Microsoft 15, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.ComponentModel.Design;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Globalization;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Formulas;
#else
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.Utilities;
	using System.Web.UI.DataVisualization.Charting.Borders3D;
	using System.Web.UI.DataVisualization.Charting.Formulas;
#endif

#endregion
		
#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	/// <summary>
    /// CommonElements class provides references to common chart classes like 
    /// DataManager, ChartTypeRegistry, ImageLoader and others. It is passed 
    /// to different chart elements to simplify access to those common classes.
	/// </summary>
	internal class CommonElements
	{
		#region Fields

        private Chart _chart;
        private ChartImage _chartPicture; 

		// Reference to Chart Graphics Object
		internal ChartGraphics graph = null;

		/// <summary>
		/// Service Container
		/// </summary>
		internal IServiceContainer	container = null;

		/// <summary>
		/// Indicates painting mode
		/// </summary>
		internal bool processModePaint = true;

		/// <summary>
		/// Indicates selection mode
		/// </summary>
		internal bool processModeRegions = false;

		// Private Fields
		private int _width = 0;
		private int _height = 0;

		#endregion

		#region Properties

		/// <summary>
		/// Reference to the Data Manager
		/// </summary>
		internal DataManager DataManager
		{
			get
			{
				return (DataManager)container.GetService(typeof(DataManager));
			}
		}

		/// <summary>
		/// True if painting mode is active
		/// </summary>
		public bool ProcessModePaint
		{
			get
			{
				return processModePaint;
			}
		}

		/// <summary>
		/// True if Hot region or image maps mode is active
		/// </summary>
		public bool ProcessModeRegions
		{
			get
			{
				return processModeRegions;
			}
		}

		/// <summary>
		/// Reference to the hot regions object
		/// </summary>
		public HotRegionsList HotRegionsList
		{
			get
			{
				return ChartPicture.hotRegionsList;
			}
		}

		/// <summary>
		/// Reference to the Data Manipulator
		/// </summary>
		public DataManipulator DataManipulator
		{
			get
			{
				return ChartPicture.DataManipulator;
			}
		}

		/// <summary>
		/// Reference to the ImageLoader
		/// </summary>
		internal ImageLoader ImageLoader
		{
			get
			{
				return (ImageLoader)container.GetService(typeof(ImageLoader));
			}
		}

		/// <summary>
		/// Reference to the Chart
		/// </summary>
		internal Chart Chart
		{
			get
			{
				if (_chart==null)
                    _chart = (Chart)container.GetService(typeof(Chart));
                return _chart;
			}
		}

		/// <summary>
		/// Reference to the ChartTypeRegistry
		/// </summary>
		internal ChartTypeRegistry ChartTypeRegistry
		{
			get
			{
				return (ChartTypeRegistry)container.GetService(typeof(ChartTypeRegistry));
			}
		}

		/// <summary>
		/// Reference to the BorderTypeRegistry
		/// </summary>
		internal BorderTypeRegistry BorderTypeRegistry
		{
			get
			{
				return (BorderTypeRegistry)container.GetService(typeof(BorderTypeRegistry));
			}
		}

		/// <summary>
		/// Reference to the FormulaRegistry
		/// </summary>
		internal FormulaRegistry FormulaRegistry
		{
			get
			{
				return (FormulaRegistry)container.GetService(typeof(FormulaRegistry));
			}
		}



		/// <summary>
		/// Reference to the ChartPicture
		/// </summary>
		internal ChartImage ChartPicture
		{
			get
			{
				if (_chartPicture ==null)
                    _chartPicture = (ChartImage)container.GetService(typeof(ChartImage));
                return _chartPicture;
			}
		}

		/// <summary>
		/// Width of the chart picture
		/// </summary>
		internal int Width
		{
			get
			{
				return _width;
			}
			set
			{
				_width = value;
			}
		}

		/// <summary>
		/// Height of the chart picture
		/// </summary>
		internal int Height
		{
			get
			{
				return _height;
			}
			set
			{
				_height = value;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="container">Service container.</param>
        internal CommonElements(IServiceContainer container)
		{
			this.container = container;
		}
		

		#endregion

		#region String convertion helper methods

        /// <summary>
		/// Converts string to double.
		/// </summary>
		/// <param name="stringToParse">String to convert.</param>
		/// <returns>Double result.</returns>
        internal static double ParseDouble(string stringToParse)
        {
            return ParseDouble(stringToParse, false);
        }
        /// <summary>
        /// Converts string to double.
        /// </summary>
        /// <param name="stringToParse">String to convert.</param>
        /// <param name="throwException">if set to <c>true</c> the exception thrown.</param>
        /// <returns>Double result.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Double.TryParse(System.String,System.Globalization.NumberStyles,System.IFormatProvider,System.Double@)")]
        internal static double ParseDouble(string stringToParse, bool throwException)
        {
            Double result = 0.0;

            if (throwException)
            {
                result = double.Parse(stringToParse, NumberStyles.Any, CultureInfo.InvariantCulture);
            }
            else
            {
                bool parseSucceed = double.TryParse(stringToParse, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
                if (!parseSucceed)
                {
                    double.TryParse(stringToParse, NumberStyles.Any, CultureInfo.CurrentCulture, out result);
                }
            }
            return result;
        }

		/// <summary>
		/// Converts string to double.
		/// </summary>
		/// <param name="stringToParse">String to convert.</param>
		/// <returns>Double result.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.Single.TryParse(System.String,System.Globalization.NumberStyles,System.IFormatProvider,System.Single@)")]
        internal static float ParseFloat(string stringToParse)
        {
            float result = 0f;
            bool parseSucceed = float.TryParse(stringToParse, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

            if (!parseSucceed)
            {
                float.TryParse(stringToParse, NumberStyles.Any, CultureInfo.CurrentCulture, out result);
            }

            return result;
        }

		#endregion
	}
}
