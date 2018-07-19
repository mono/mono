//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		KeywordsRegistry.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting.Utilities
//
//	Classes:	KeywordsRegistry, KeywordInfo
//
//  Purpose:	A registry that keeps track of all available 
//				keywords and name of the objects and properties
//				where they can be used.
//
//  Formatting Keywords Overview:
//  -----------------------------
//  A Formatting Keyword is a specially formatted character sequence 
//  that gets replaced by an associated Chart Series value, or 
//  calculated value. Keywords can be used with most string properties 
//  of Series and DataPoint objects. 
//  
//  Here is an example of setting series labels so that the first 
//  line will display the Y value and the second line displays 
//  the X value. 
//      
//      Chart1.Series["Series1"].Label = "Y = #VALY\nX = #VALX";
//  
//  Series label in this case will look like this: 
//  
//  	Y = 45.78
//  	X = 456
//  
//  An optional format string can be added after the keyword. 
//  For example, when you set the Format option to Percent for 
//  the first Y value, the resulting keyword produced is "#VALY{P}".  
//  You can also apply format strings in code-behind using the same 
//  nomenclature; you do this by following the keyword with a format 
//  specifier enclosed in braces.  For information concerning the 
//  types of formatting that can be used, see the Formatting Types 
//  topic in the MSDN library.
//
//	Reviewed:	AG - Microsoft 5, 2007
//
//===================================================================


#region Used Namespaces

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;

#if Microsoft_CONTROL
    using System.Windows.Forms.DataVisualization.Charting;
    using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
#else
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.DataVisualization.Charting.ChartTypes;
#endif

#endregion

#if Microsoft_CONTROL
    namespace System.Windows.Forms.DataVisualization.Charting.Utilities
#else // Microsoft_CONTROL
	namespace System.Web.UI.DataVisualization.Charting.Utilities
#endif // Microsoft_CONTROL
{
    /// <summary>
    /// KeywordName class contains constant strings defining
    /// names of all keywords used in the data point and series classes.
    /// </summary>
        internal static class KeywordName
        {
            #region Keyword Names

            internal const string Index = "#INDEX";
            internal const string ValX = "#VALX";
            internal const string ValY = "#VALY";
            internal const string Val = "#VAL";
            internal const string Total = "#TOTAL";
            internal const string Percent = "#PERCENT";
            internal const string Label = "#LABEL";
            internal const string AxisLabel = "#AXISLABEL";
            internal const string LegendText = "#LEGENDTEXT";
            internal const string SeriesName = "#SERIESNAME";
            internal const string Ser = "#SER";
            internal const string Avg = "#AVG";
            internal const string Max = "#MAX";
            internal const string Min = "#MIN";
            internal const string Last = "#LAST";
            internal const string First = "#FIRST";
            internal const string CustomProperty = "#CUSTOMPROPERTY";

            #endregion // Keyword Names
        }

	/// <summary>
    /// KeywordRegistry class stores information about all 
    /// chart formatting keywords. It automatically registers 
    /// all known keywords when object is constructed. This 
    /// data is exposed as ArrayList through the ‘registeredKeywords’ 
    /// field. Each item in this ArrayList is a KeywordInfo 
    /// object which describes a single formatting keyword.
	/// </summary>
	internal class KeywordsRegistry : IServiceProvider
	{
		#region Fields

		// List of registered keywords
		internal	ArrayList		registeredKeywords = new ArrayList();

		#endregion

		#region Constructor and Services

		/// <summary>
		/// Keywords registry public constructor.
		/// </summary>
		public KeywordsRegistry()
		{
			// Register Keywords used in the chart
			RegisterKeywords();
		}

		/// <summary>
		/// Returns Keywords registry service object.
		/// </summary>
		/// <param name="serviceType">Service type to get.</param>
		/// <returns>Custom properties registry service.</returns>
		[EditorBrowsableAttribute(EditorBrowsableState.Never)]
		object IServiceProvider.GetService(Type serviceType)
		{
			if(serviceType == typeof(KeywordsRegistry))
			{
				return this;
			}
			throw (new ArgumentException( SR.ExceptionKeywordsRegistryUnsupportedType(serviceType.ToString())));
		}

		#endregion

		#region Keywords Registering methods

		/// <summary>
		/// Registers all chart formatting keywords.
		/// </summary>
		private void RegisterKeywords()
		{
            string seriesPointSupportedProperties = "Text,Label,LabelMapAreaAttributes,ToolTip,Url,LabelToolTip,MapAreaAttributes,AxisLabel,LegendToolTip,LegendMapAreaAttributes,LegendUrl,LegendText";

			// #INDEX keyword
			this.Register(
                SR.DescriptionKeyWordNameIndexDataPoint,
				KeywordName.Index,
				string.Empty,
                SR.DescriptionKeyWordIndexDataPoint2,
				"DataPoint",
				seriesPointSupportedProperties,
				false,
				false);

			// #VALX keyword
			this.Register(
                SR.DescriptionKeyWordNameXValue,
				KeywordName.ValX,
				string.Empty,
                SR.DescriptionKeyWordXValue,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				true,
				false);

			// #VALY keyword
			this.Register(
                SR.DescriptionKeyWordNameYValue,
				KeywordName.Val,
				string.Empty,
                SR.DescriptionKeyWordYValue,
				"Series,DataPoint,Annotation,LegendCellColumn,LegendCellColumn",
				seriesPointSupportedProperties,
				true,
				true);

			// #TOTAL keyword
			this.Register(
                SR.DescriptionKeyWordNameTotalYValues,
				KeywordName.Total,
				string.Empty,
                SR.DescriptionKeyWordTotalYValues,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				true,
				false);

			// #PERCENT keyword
			this.Register(
                SR.DescriptionKeyWordNameYValuePercentTotal,
				KeywordName.Percent,
				string.Empty,
                SR.DescriptionKeyWordYValuePercentTotal,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				true,
				true);

			// #INDEX keyword
			this.Register(
                SR.DescriptionKeyWordNameIndexTheDataPoint,
				KeywordName.Index,
				string.Empty,
                SR.DescriptionKeyWordIndexDataPoint,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				false,
				false);

			// #LABEL keyword
			this.Register(
                SR.DescriptionKeyWordNameLabelDataPoint,
				KeywordName.Label,
				string.Empty,
                SR.DescriptionKeyWordLabelDataPoint,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				false,
				false);

			// #AXISLABEL keyword
			this.Register(
                SR.DescriptionKeyWordNameAxisLabelDataPoint,
				KeywordName.AxisLabel,
				string.Empty,
                SR.DescriptionKeyWordAxisLabelDataPoint,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				false,
				false);

			// #LEGENDTEXT keyword
			this.Register(
                SR.DescriptionKeyWordNameLegendText,
				KeywordName.LegendText,
				string.Empty,
                SR.DescriptionKeyWordLegendText,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				false,
				false);

			// #SERIESNAME keyword
			this.Register(
                SR.DescriptionKeyWordNameSeriesName,
				KeywordName.SeriesName,
				KeywordName.Ser,
                SR.DescriptionKeyWordSeriesName,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				false,
				false);

			// *************** NEW KEYWORDS in version 5.5 ***************

			// #AVG keyword
			this.Register(
                SR.DescriptionKeyWordNameAverageYValues,
				KeywordName.Avg,
				string.Empty,
                SR.DescriptionKeyWordAverageYValues,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				true,
				true);

			// #MAX keyword
			this.Register(
                SR.DescriptionKeyWordNameMaximumYValues,
				KeywordName.Max,
				string.Empty,
                SR.DescriptionKeyWordMaximumYValues,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				true,
				true);

			// #MIN keyword
			this.Register(
                SR.DescriptionKeyWordNameMinimumYValues,
				KeywordName.Min,
				string.Empty,
                SR.DescriptionKeyWordMinimumYValues,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				true,
				true);

			// #LAST keyword
			this.Register(
                SR.DescriptionKeyWordNameLastPointYValue,
				KeywordName.Last,
				string.Empty,
                SR.DescriptionKeyWordLastPointYValue,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				true,
				true);

			// #FIRST keyword
			this.Register(
                SR.DescriptionKeyWordNameFirstPointYValue,
				KeywordName.First,
				string.Empty,
                SR.DescriptionKeyWordFirstPointYValue,
				"Series,DataPoint,Annotation,LegendCellColumn",
				seriesPointSupportedProperties,
				true,
				true);
		}

		#endregion // Keywords Registering methods

		#region Registry methods

        /// <summary>
        /// Adds keyword information into the registry.
        /// </summary>
        /// <param name="name">Keyword full name.</param>
        /// <param name="keyword">Keyword text.</param>
        /// <param name="keywordAliases">Keyword alternative text.</param>
        /// <param name="description">Keyword description.</param>
        /// <param name="appliesToTypes">Comma separated list of applicable classes</param>
        /// <param name="appliesToProperties">Comma separated list of applicable properties.</param>
        /// <param name="supportsFormatting">True if formatting is supported.</param>
        /// <param name="supportsValueIndex">True if different point Y values are supported.</param>
		public void Register(
			string name,
			string keyword,
			string keywordAliases,
			string description,
			string appliesToTypes,
			string appliesToProperties,
			bool supportsFormatting,
			bool supportsValueIndex)
		{
			// Create new keyword information object
			KeywordInfo keywordInfo = new KeywordInfo(
				name,
				keyword,
				keywordAliases,
				description,
				appliesToTypes,
				appliesToProperties,
				supportsFormatting,
				supportsValueIndex);

			// Add keyword information to the hash table
			registeredKeywords.Add(keywordInfo);
		}

		#endregion
	}

	/// <summary>
    /// KeywordInfo class stores information about a single 
    /// formatting keyword. This information includes Name, 
    /// Description, list of data types and properties it 
    /// applies to and other information.
	/// </summary>
    internal class KeywordInfo
	{
		#region Public Fields

		/// <summary>
		/// Keyword full name.
		/// </summary>
		public	string				Name = String.Empty;

		/// <summary>
		/// String that represent this keyword in the property (keyword).
		/// </summary>
		public	string				Keyword = String.Empty;

		/// <summary>
		/// Comma separated strings that may alternatively represent this 
		/// keyword in the property.
		/// </summary>
		public	string				KeywordAliases = String.Empty;

		/// <summary>
		/// Keyword description.
		/// </summary>
		public	string				Description = String.Empty;

		/// <summary>
		/// Comma separated names of classes this keyword applies to.
		/// </summary>
		public	string				AppliesToTypes = String.Empty;

		/// <summary>
		/// Comma separated names of properties this keyword applies to.
		/// </summary>
		public	string				AppliesToProperties = String.Empty;

		/// <summary>
		/// True if keyword value can be formatted.
		/// </summary>
		public	bool				SupportsFormatting = false;

		/// <summary>
		/// True if keyword can be used with different point Y values.
		/// </summary>
		public	bool				SupportsValueIndex = false;

		#endregion // Public Fields

		#region Constructor

        /// <summary>
        /// Keyword information object constructor
        /// </summary>
        /// <param name="name">Keyword full name.</param>
        /// <param name="keyword">Keyword text.</param>
        /// <param name="keywordAliases">Keyword alternative text.</param>
        /// <param name="description">Keyword description.</param>
        /// <param name="appliesToTypes">Comma separated list of applicable classes</param>
        /// <param name="appliesToProperties">Comma separated list of applicable properties.</param>
        /// <param name="supportsFormatting">True if formatting is supported.</param>
        /// <param name="supportsValueIndex">True if different point Y values are supported.</param>
		public KeywordInfo(
			string name,
			string keyword,
			string keywordAliases,
			string description,
			string appliesToTypes,
			string appliesToProperties,
			bool supportsFormatting,
			bool supportsValueIndex)
		{
			this.Name = name;
			this.Keyword = keyword;
			this.KeywordAliases = keywordAliases;
			this.Description = description;
			this.AppliesToTypes = appliesToTypes;
			this.AppliesToProperties = appliesToProperties;
			this.SupportsFormatting = supportsFormatting;
			this.SupportsValueIndex = supportsValueIndex;
		}

		#endregion // Constructor

		#region Methods

		/// <summary>
		/// Returns a String that represents the current keyword Information.
		/// </summary>
		/// <returns>Returns keyword name.</returns>
		public override string ToString()
		{
			return this.Name;
		}
		/// <summary>
		/// Gets an array of keywords names including the aliases.
		/// </summary>
		/// <returns>A string array of keyword names that represent this keyword.</returns>
		public string[] GetKeywords()
		{
            // NOTE: Each keyword has a unique name. In addition the keyword may have an
            // alternative names (aliases). 
            // Most common scenario for a keyword aliase is when keyword has a long and
            // short form. For example, KeywordName.Ser and "#SERIES".
            
			// Fill array of possible names for that keyword
			if(this.KeywordAliases.Length > 0)
			{
				string[] keywordAliases = this.KeywordAliases.Split(',');
				string[] keywordNames = new string[keywordAliases.Length + 1];
				keywordNames[0] = this.Keyword;
				keywordAliases.CopyTo(keywordNames, 1);
				return keywordNames;
			}
			else
			{
				return new string[] { this.Keyword };
			}
		}

		#endregion // Methods
	}
}

