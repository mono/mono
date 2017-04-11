//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		LegendColumns.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	LegendCellColumn, LegendCellColumnCollection, 
//				LegendCell, LegendCellCollection, Margins
//
//  Purpose:	LegendCell and LegendCellColumn classes allow to 
//              create highly customize legends. Please refer to 
//              Chart documentation which contains images and 
//              samples describing this functionality.
//
//	Reviewed:	AG - Microsoft 14, 2007
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
using System.Diagnostics.CodeAnalysis;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
	using System.Windows.Forms.DataVisualization.Charting;
	using System.ComponentModel.Design.Serialization;
	using System.Reflection;
	using System.Windows.Forms.Design;
#else
	using System.Web;
	using System.Web.UI;
	using System.Web.UI.DataVisualization.Charting;
	using System.Web.UI.DataVisualization.Charting.Data;
	using System.Web.UI.DataVisualization.Charting.Utilities;
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
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
		/// An enumeration of legend cell types.
		/// </summary>
		public enum LegendCellType
		{
			/// <summary>
			/// Legend cell contains text.
			/// </summary>
			Text,

			/// <summary>
			/// Legend cell contains series symbol.
			/// </summary>
			SeriesSymbol,

			/// <summary>
			/// Legend cell contains image.
			/// </summary>
			Image
		}

		/// <summary>
        /// An enumeration of legend cell column types.
		/// </summary>
		public enum LegendCellColumnType
		{
			/// <summary>
			/// Legend column contains text.
			/// </summary>
			Text,

			/// <summary>
			/// Legend column contains series symbol.
			/// </summary>
			SeriesSymbol
		}

		#endregion // Enumerations

		/// <summary>
        /// The LegendCellColumn class represents a cell column in a legend, 
        /// used to extend the functionality of the default legend. It contains 
        /// visual appearance properties, legend header settings and also determine
        /// how and in which order cells are formed for each of the legend items.
		/// </summary>
		[
		SRDescription("DescriptionAttributeLegendCellColumn_LegendCellColumn"),
		]
#if Microsoft_CONTROL
        public class LegendCellColumn : ChartNamedElement
#else
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
        public class LegendCellColumn : ChartNamedElement, IChartMapArea
#endif //!Microsoft_CONTROL
        {
            #region Fields

			// Legend column type
			private LegendCellColumnType _columnType = LegendCellColumnType.Text;

			// Legend column text
			private string	_text = KeywordName.LegendText;

			// Legend column text color
			private Color	_foreColor = Color.Empty;

			// Legend column back color
			private Color	_backColor = Color.Empty;

            // Font cache
            private FontCache _fontCache = new FontCache();

			// Legend column text font
			private Font	_font = null;

			// Legend column series symbol size
			private Size	_seriesSymbolSize = new Size(200, 70);

			// Legend column content allignment
			private ContentAlignment _alignment = ContentAlignment.MiddleCenter;

			// Legend column tooltip
			private string	_toolTip = string.Empty;

			// Legend column margins
			private Margins	_margins = new Margins(0, 0, 15, 15);

#if !Microsoft_CONTROL

			// Legend column Url
			private string	_url = string.Empty;

			// Legend column map area attribute
			private string _mapAreaAttribute = string.Empty;
            private string _postbackValue = String.Empty;
#endif // !Microsoft_CONTROL

            // Legend column header text
			private string	_headerText = string.Empty;

			// Legend column/cell content allignment
			private StringAlignment _headerAlignment = StringAlignment.Center;

			// Legend column header text color
			private Color	_headerForeColor = Color.Black;

			// Legend column header text back color
			private Color	_headerBackColor = Color.Empty;

			// Legend column header text font
            private Font    _headerFont = null;

			// Minimum column width
			private int		_minimumCellWidth = -1;

			// Maximum column width
			private int		_maximumCellWidth = -1;

			#endregion // Fields

			#region Constructors

			/// <summary>
            /// LegendCellColumn constructor.
			/// </summary>
            public LegendCellColumn()
                : this(string.Empty, LegendCellColumnType.Text, KeywordName.LegendText, ContentAlignment.MiddleCenter)
			{
                _headerFont = _fontCache.DefaultBoldFont;
			}
			

			/// <summary>
            /// LegendCellColumn constructor.
			/// </summary>
			/// <param name="headerText">Column header text.</param>
			/// <param name="columnType">Column type.</param>
			/// <param name="text">Column cell text.</param>
			public LegendCellColumn(string headerText, LegendCellColumnType columnType, string text) :  this(headerText, columnType, text, ContentAlignment.MiddleCenter)
			{
				
			}

			/// <summary>
			/// Legend column object constructor.
			/// </summary>
			/// <param name="headerText">Column header text.</param>
			/// <param name="columnType">Column type.</param>
			/// <param name="text">Column cell text .</param>
			/// <param name="alignment">Column cell content alignment.</param>
			public LegendCellColumn(string headerText, LegendCellColumnType columnType, string text, ContentAlignment alignment)
			{
                this._headerText = headerText;
                this._columnType = columnType;
                this._text = text;
                this._alignment = alignment;
            }


			#endregion // Constructors

			#region Properties

			/// <summary>
			/// Gets or sets name of legend column.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			SRDescription("DescriptionAttributeLegendCellColumn_Name"),
			]
			public override string Name
			{
				get
				{
					return base.Name;
				}
				set
				{
                    base.Name = value;
				}
			}


            /// <summary>
            /// Gets legend this column belongs too.
            /// </summary>
            [
            Browsable(false),
            DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
            SerializationVisibilityAttribute(SerializationVisibility.Hidden),
            ]
            public virtual Legend Legend
            {
                get
                {
                    if (Parent != null)
                        return Parent.Parent as Legend;
                    else
                        return null;
                }
            }


			/// <summary>
			/// Gets or sets legend column type.  This is only applicable to items that are automatically generated for the series.
			/// </summary>
			[
			SRCategory("CategoryAttributeSeriesItems"),
			DefaultValue(LegendCellColumnType.Text),
			SRDescription("DescriptionAttributeLegendCellColumn_ColumnType"),
			ParenthesizePropertyNameAttribute(true)
			]
			public virtual LegendCellColumnType ColumnType
			{
				get
				{
					return this._columnType;
				}
				set
				{
					this._columnType = value;
					this.Invalidate();
				}
			}

			/// <summary>
			/// Gets or sets legend column text.  This is only applicable to items that are automatically generated for the series.  
            /// Set the ColumnType property to text to use this property.
			/// </summary>
			[
			SRCategory("CategoryAttributeSeriesItems"),
			DefaultValue(KeywordName.LegendText),
			SRDescription("DescriptionAttributeLegendCellColumn_Text"),
            Editor(Editors.KeywordsStringEditor.Editor, Editors.KeywordsStringEditor.Base),
			]
			public virtual string Text
			{
				get
				{
					return this._text;
				}
				set
				{
					this._text = value;
					this.Invalidate();
				}
			}

			/// <summary>
			/// Gets or sets the text color of the legend column.
			/// </summary>
			[
			SRCategory("CategoryAttributeSeriesItems"),
			DefaultValue(typeof(Color), ""),
            SRDescription("DescriptionAttributeForeColor"),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
			]
			public virtual Color ForeColor
			{
				get
				{
					return this._foreColor;
				}
				set
				{
					this._foreColor = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the background color of the legend column.
			/// </summary>
			[
			SRCategory("CategoryAttributeSeriesItems"),
			DefaultValue(typeof(Color), ""),
            SRDescription("DescriptionAttributeBackColor"),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
			]
			public virtual Color BackColor
			{
				get
				{
					return this._backColor;
				}
				set
				{
					this._backColor = value;
					this.Invalidate();
				}
			}


			/// <summary>
            /// Gets or sets the font of the legend column text.
			/// </summary>
			[
			SRCategory("CategoryAttributeSeriesItems"),
			DefaultValue(null),
			SRDescription("DescriptionAttributeLegendCellColumn_Font"),
			]
			public virtual Font Font
			{
				get
				{
					return this._font;
				}
				set
				{
					this._font = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the series symbol size of the legend column
			/// for the items automatically generated for the series.
			/// </summary>
			[
			SRCategory("CategoryAttributeSeriesItems"),
			DefaultValue(typeof(Size), "200, 70"),
			SRDescription("DescriptionAttributeLegendCellColumn_SeriesSymbolSize"),
			]
			public virtual Size SeriesSymbolSize
			{
				get
				{
					return this._seriesSymbolSize;
				}
				set
				{
					if(value.Width < 0 || value.Height < 0)
					{
                        throw (new ArgumentException(SR.ExceptionSeriesSymbolSizeIsNegative, "value"));
					}
					this._seriesSymbolSize = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the content alignment of the legend column.
			/// This is only applicable to items that are automatically generated for the series.
			/// </summary>
			[
			SRCategory("CategoryAttributeSeriesItems"),
			DefaultValue(ContentAlignment.MiddleCenter),
			SRDescription("DescriptionAttributeLegendCellColumn_Alignment"),
			]
			public virtual ContentAlignment Alignment
			{
				get
				{
					return this._alignment;
				}
				set
				{
					this._alignment = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the margins of the legend column (as a percentage of legend font size).  
            /// This is only applicable to items that are automatically generated for the series.
			/// </summary>
			[
			SRCategory("CategoryAttributeSeriesItems"),
			DefaultValue(typeof(Margins), "0,0,15,15"),
			SRDescription("DescriptionAttributeLegendCellColumn_Margins"),
			SerializationVisibilityAttribute(SerializationVisibility.Attribute),
			DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
			NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
			PersistenceMode(PersistenceMode.InnerProperty),
#endif
			]
			public virtual Margins Margins
			{
				get
				{
					return this._margins;
				}
				set
				{
					this._margins = value;
					this.Invalidate();

#if Microsoft_CONTROL
					// Set common elements of the new margins class
					if(this.Legend != null)
					{
						this._margins.Common = this.Legend.Common;
					}
#endif // Microsoft_CONTROL
				}
			}

			/// <summary>
			/// Returns true if property should be serialized.  This is for internal use only.
			/// </summary>
			[EditorBrowsableAttribute(EditorBrowsableState.Never)]
			public bool ShouldSerializeMargins()
			{
				if(this._margins.Top == 0 &&
					this._margins.Bottom == 0 &&
					this._margins.Left == 15 &&
					this._margins.Right == 15 )
				{
					return false;
				}
				return true;
			}

			/// <summary>
			/// Gets or sets the legend column tooltip. This is only applicable to items that are automatically generated for the series.
			/// </summary>
			[
			SRCategory("CategoryAttributeSeriesItems"),
            SRDescription("DescriptionAttributeToolTip"),
			DefaultValue(""),
            Editor(Editors.KeywordsStringEditor.Editor, Editors.KeywordsStringEditor.Base),
			]
			public virtual string ToolTip
			{
				set
				{
					this._toolTip = value;
#if Microsoft_CONTROL
					if(Chart != null && 
					   Chart.selection != null)
					{
						Chart.selection.enabledChecked = false;
					}
#endif
				}
				get
				{
					return this._toolTip;
				}
			}

#if !Microsoft_CONTROL

			/// <summary>
			/// Gets or sets the URL target of the legend items automatically generated for the series.
			/// </summary>
			[
			SRCategory("CategoryAttributeSeriesItems"),
			SRDescription("DescriptionAttributeUrl"),
			DefaultValue(""),
            Editor(Editors.KeywordsStringEditor.Editor, Editors.KeywordsStringEditor.Base),
			]
			public virtual string Url
			{
				set
				{
					this._url = value;
				}
				get
				{
					return this._url;
				}
			}

			/// <summary>
			/// Gets or sets the other attributes of the legend items automatically generated for the series.
			/// </summary>
			[
			SRCategory("CategoryAttributeSeriesItems"),
			SRDescription("DescriptionAttributeMapAreaAttributes"),
            Editor(Editors.KeywordsStringEditor.Editor, Editors.KeywordsStringEditor.Base),
			DefaultValue(""),
			]
			public virtual string MapAreaAttributes
			{
				set
				{
					this._mapAreaAttribute = value;
				}
				get
				{
					return this._mapAreaAttribute;
				}
			}

            /// <summary>
            /// Gets or sets the postback value which can be processed on a click event.
            /// </summary>
            /// <value>The value which is passed to a click event as an argument.</value>
            [DefaultValue("")]
            [SRCategory(SR.Keys.CategoryAttributeSeriesItems)]
            [SRDescription(SR.Keys.DescriptionAttributePostBackValue)]
            public string PostBackValue 
            {
                get
                {
                    return this._postbackValue;
                }
                set
                {
                    this._postbackValue = value;
                }
            }


#endif // !Microsoft_CONTROL

            /// <summary>
			/// Gets or sets the legend column header text.
			/// </summary>
			[
			SRCategory("CategoryAttributeHeader"),
			DefaultValue(""),
			SRDescription("DescriptionAttributeLegendCellColumn_HeaderText"),
			]
			public virtual string HeaderText
			{
				get
				{
					return this._headerText;
				}
				set
				{
					this._headerText = value;
					this.Invalidate();
				}
			}

			/// <summary>
			/// Gets or sets the color of the legend column header text.
			/// </summary>
			[
			SRCategory("CategoryAttributeHeader"),
			DefaultValue(typeof(Color), "Black"),
			SRDescription("DescriptionAttributeLegendCellColumn_HeaderColor"),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
			]
			public virtual Color HeaderForeColor
			{
				get
				{
					return this._headerForeColor;
				}
				set
				{
					this._headerForeColor = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the background color of the legend column header.
			/// </summary>
			[
			SRCategory("CategoryAttributeHeader"),
			DefaultValue(typeof(Color), ""),
            SRDescription("DescriptionAttributeHeaderBackColor"),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
			]
			public virtual Color HeaderBackColor
			{
				get
				{
					return this._headerBackColor;
				}
				set
				{
					this._headerBackColor = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the font of the legend column header.
			/// </summary>
			[
			SRCategory("CategoryAttributeHeader"),
            DefaultValue(typeof(Font), "Microsoft Sans Serif, 8pt, style=Bold"),
			SRDescription("DescriptionAttributeLegendCellColumn_HeaderFont"),
			]
			public virtual Font HeaderFont
			{
				get
				{
					return this._headerFont;
				}
				set
				{
					this._headerFont = value;
					this.Invalidate();
				}
			}

			/// <summary>
			/// Gets or sets the horizontal text alignment of the legend column header.
			/// </summary>
			[
			SRCategory("CategoryAttributeHeader"),
			DefaultValue(typeof(StringAlignment), "Center"),
			SRDescription("DescriptionAttributeLegendCellColumn_HeaderTextAlignment"),
			]
			public StringAlignment HeaderAlignment
			{
				get
				{
					return this._headerAlignment;
				}
				set
				{
					if(value != this._headerAlignment)
					{
						this._headerAlignment = value;
						this.Invalidate();
					}
				}
			}

			/// <summary>
			/// Gets or sets the minimum width (as a percentage of legend font size) of legend column. Set this property to -1 for automatic calculation.
			/// </summary>
			[
			SRCategory("CategoryAttributeSize"),
			DefaultValue(-1),
			TypeConverter(typeof(IntNanValueConverter)),
			SRDescription("DescriptionAttributeLegendCellColumn_MinimumWidth"),
			]
			public virtual int MinimumWidth
			{
				get
				{
					return this._minimumCellWidth;
				}
				set
				{
					if(value < -1)
					{
                        throw (new ArgumentException(SR.ExceptionMinimumCellWidthIsWrong, "value"));
					}

					this._minimumCellWidth = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the maximum width (as a percentage of legend font size) of legend column.  Set this property to -1 for automatic calculation.
			/// </summary>
			[
			SRCategory("CategoryAttributeSize"),
			DefaultValue(-1),
			TypeConverter(typeof(IntNanValueConverter)),
			SRDescription("DescriptionAttributeLegendCellColumn_MaximumWidth"),
			]
			public virtual int MaximumWidth
			{
				get
				{
					return this._maximumCellWidth;
				}
				set
				{
					if(value < -1)
					{
                        throw (new ArgumentException(SR.ExceptionMaximumCellWidthIsWrong, "value"));
					}
					this._maximumCellWidth = value;
					this.Invalidate();
				}
			}

			#endregion // Properties
			
			#region Methods

			/// <summary>
			/// Creates a new LegendCell object and copies all properties from the 
			/// current column into the newly created one.
			/// </summary>
			/// <returns>A new copy of the LegendCell</returns>
			internal LegendCell CreateNewCell()
			{
				LegendCell newCell = new LegendCell();
				newCell.CellType = (this.ColumnType == LegendCellColumnType.SeriesSymbol) ? LegendCellType.SeriesSymbol : LegendCellType.Text;
				newCell.Text = this.Text;
				newCell.ToolTip = this.ToolTip;
#if !Microsoft_CONTROL
				newCell.Url = this.Url;
				newCell.MapAreaAttributes = this.MapAreaAttributes;
                newCell.PostBackValue = this.PostBackValue;
#endif // !Microsoft_CONTROL

				newCell.SeriesSymbolSize = this.SeriesSymbolSize;
				newCell.Alignment = this.Alignment;
				newCell.Margins = new Margins(this.Margins.Top, this.Margins.Bottom, this.Margins.Left, this.Margins.Right);
				return newCell;
			}

			#endregion // Methods

            #region IDisposable Members

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources
            /// </summary>
            /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_fontCache != null)
                    {
                        _fontCache.Dispose();
                        _fontCache = null;
                    }
                }
            }


            #endregion
		}

		/// <summary>
        /// The LegendCell class represents a single cell in the chart legend. 
        /// Legend contains several legend items.  Each item contains several
        /// cells which form the vertical columns. This class provides properties 
        /// which determine content of the cell and its visual appearance. It
        /// also contains method which determine the size of the cell and draw
        /// cell in the chart.
		/// </summary>
		[
		SRDescription("DescriptionAttributeLegendCell_LegendCell"),
		]
#if Microsoft_CONTROL
        public class LegendCell  : ChartNamedElement
#else
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
        public class LegendCell : ChartNamedElement, IChartMapArea
#endif
        {
            #region Fields

			// Legend cell type
			private LegendCellType _cellType = LegendCellType.Text;

			// Legend cell text
			private string	_text = string.Empty;

			// Legend cell text color
			private Color	_foreColor = Color.Empty;

			// Legend cell back color
			private Color	_backColor = Color.Empty;

            // Font cache
            private FontCache _fontCache = new FontCache();

            // Legend cell text font
			private Font	_font = null;

			// Legend cell image name
			private string	_image = string.Empty;

			// Legend cell image transparent color
			private Color	_imageTransparentColor = Color.Empty;

			// Legend cell image size
			private Size	_imageSize = Size.Empty;

			// Legend cell series symbol size
			private Size	_seriesSymbolSize = new Size(200, 70);

			// Legend cell content allignment
			private ContentAlignment _alignment = ContentAlignment.MiddleCenter;

			// Numer of cells this cell uses to show it's content
			private int		_cellSpan = 1;

			// Legend cell tooltip
			private string	_toolTip = string.Empty;

			// Legend cell margins
			private Margins	_margins = new Margins(0, 0, 15, 15);

			// Cell row index
			private int _rowIndex = -1;

#if !Microsoft_CONTROL

			// Legend cell Url
			private string	_url = string.Empty;

			// Legend cell map area attribute
			private string _mapAreaAttribute = string.Empty;
            private string _postbackValue = String.Empty;

#endif // !Microsoft_CONTROL

            // Position where cell is drawn in pixel coordinates.
			// Exncludes margins and space required for separators
			internal Rectangle	cellPosition = Rectangle.Empty;

			// Position where cell is drawn in pixel coordinates. 
			// Includes margins and space required for separators
			internal Rectangle	cellPositionWithMargins = Rectangle.Empty;

			// Last cached cell size.
			private Size _cachedCellSize = Size.Empty;

			// Font reduced value used to calculate last cached cell size
			private int _cachedCellSizeFontReducedBy = 0;

            #endregion // Fields

            #region Constructors

            /// <summary>
            /// LegendCell constructor.
			/// </summary>
			public LegendCell()
			{
				this.Intitialize(LegendCellType.Text, string.Empty, ContentAlignment.MiddleCenter);
			}

			/// <summary>
            /// LegendCell constructor.
			/// </summary>
			/// <param name="text">Cell text or image name, depending on the type.</param>
			public LegendCell(string text)
			{
				this.Intitialize(LegendCellType.Text, text, ContentAlignment.MiddleCenter);
			}

			/// <summary>
            /// LegendCell constructor.
			/// </summary>
			/// <param name="cellType">Cell type.</param>
			/// <param name="text">Cell text or image name, depending on the type.</param>
			public LegendCell(LegendCellType cellType, string text)
			{
				this.Intitialize(cellType, text, ContentAlignment.MiddleCenter);
			}

			/// <summary>
            /// LegendCell constructor.
			/// </summary>
			/// <param name="cellType">Cell type.</param>
			/// <param name="text">Cell text or image name, depending on the type.</param>
			/// <param name="alignment">Cell content alignment.</param>
			public LegendCell(LegendCellType cellType, string text, ContentAlignment alignment)
			{
				this.Intitialize(cellType, text, alignment);
			}

			/// <summary>
			/// Initializes newly created object.
			/// </summary>
			/// <param name="cellType">Cell type.</param>
			/// <param name="text">Cell text or image name depending on the type.</param>
			/// <param name="alignment">Cell content alignment.</param>
			private void Intitialize(LegendCellType cellType, string text, ContentAlignment alignment)
			{
				this._cellType = cellType;
				if(this._cellType == LegendCellType.Image)
				{
					this._image = text;
				}
				else
				{
					this._text = text;
				}
				this._alignment = alignment;
#if !Microsoft_CONTROL
                this.PostBackValue = String.Empty;
#endif //!WIN_CONTROL
            }

			#endregion // Constructors

			#region Properties

			/// <summary>
			/// Gets or sets the name of the legend cell.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			SRDescription("DescriptionAttributeLegendCell_Name"),
			]
			public override string Name
			{
				get
				{
					return base.Name;
				}
				set
				{
                    base.Name = value;
				}
			}

			/// <summary>
			/// Gets or sets the type of the legend cell.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			DefaultValue(LegendCellType.Text),
			SRDescription("DescriptionAttributeLegendCell_CellType"),
			ParenthesizePropertyNameAttribute(true)
			]
			public virtual LegendCellType CellType
			{
				get
				{
					return this._cellType;
				}
				set
				{
					this._cellType = value;
					this.Invalidate();
				}
			}

            /// <summary>
            /// Gets legend this column/cell belongs too.
            /// </summary>
            [
            Browsable(false),
            DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
            SerializationVisibilityAttribute(SerializationVisibility.Hidden),
            ]
            public virtual Legend Legend
            {
                get
                {
                    LegendItem item = this.LegendItem;
                    if (item != null)
                        return item.Legend;
                    else
                        return null;
                }
            }

            /// <summary>
            /// Gets legend item this cell belongs too.
            /// </summary>
            [
            Browsable(false),
            DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
            SerializationVisibilityAttribute(SerializationVisibility.Hidden),
            ]
            public virtual LegendItem LegendItem
            {
                get
                {
                    if (Parent != null)
                        return Parent.Parent as LegendItem;
                    else
                        return null;
                }
            }



			/// <summary>
			/// Gets or sets the text of the legend cell. Set CellType to text to use this property.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			DefaultValue(""),
			SRDescription("DescriptionAttributeLegendCell_Text"),
			]
			public virtual string Text
			{
				get
				{
					return this._text;
				}
				set
				{
					this._text = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the text color of the legend cell. Set CellType to text to use this property.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			DefaultValue(typeof(Color), ""),
            SRDescription("DescriptionAttributeForeColor"),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
			]
			public virtual Color ForeColor
			{
				get
				{
					return this._foreColor;
				}
				set
				{
					this._foreColor = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the background color of the legend cell.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			DefaultValue(typeof(Color), ""),
            SRDescription("DescriptionAttributeBackColor"),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
			]
			public virtual Color BackColor
			{
				get
				{
					return this._backColor;
				}
				set
				{
					this._backColor = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the font of the legend cell. Set CellType to text to use this property.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			DefaultValue(null),
			SRDescription("DescriptionAttributeLegendCell_Font"),
			]
			public virtual Font Font
			{
				get
				{
                    return this._font;
				}
				set
				{
                    this._font = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the URL of the image of the legend cell. Set CellType to image to use this property.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			DefaultValue(""),
			SRDescription("DescriptionAttributeLegendCell_Image"),
            Editor(Editors.ImageValueEditor.Editor, Editors.ImageValueEditor.Base),
			]
			public virtual string Image
			{
				get
				{
					return this._image;
				}
				set
				{
					this._image = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets a color which will be replaced with a transparent color while drawing the image.  Set CellType to image to use this property.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			DefaultValue(typeof(Color), ""),
            SRDescription("DescriptionAttributeImageTransparentColor"),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
			]
			public virtual Color ImageTransparentColor
			{
				get
				{
					return this._imageTransparentColor;
				}
				set
				{
					this._imageTransparentColor = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the image size (as a percentage of legend font size) of the legend cell. 
            /// Set CellType to Image to use this property.
			/// </summary>
			/// <remarks>
			/// If property is set to Size.IsEmpty, the original image pixels size is used.
			/// </remarks>
			[
			SRCategory("CategoryAttributeLayout"),
			DefaultValue(typeof(Size), "0, 0"),
			TypeConverter(typeof(SizeEmptyValueConverter)),
			SRDescription("DescriptionAttributeLegendCell_ImageSize"),
			]
			public virtual Size ImageSize
			{
				get
				{
					return this._imageSize;
				}
				set
				{
					if(value.Width < 0 || value.Height < 0)
					{
                        throw (new ArgumentException(SR.ExceptionLegendCellImageSizeIsNegative, "value"));
					}
					this._imageSize = value;
					this.Invalidate();
				}
			}

			/// <summary>
			/// Gets or sets the series symbol size (as a percentage of legend font size) of the legend cell. 
            /// Set CellType to SeriesSymbol to use this property.
			/// </summary>
			[
			SRCategory("CategoryAttributeLayout"),
			DefaultValue(typeof(Size), "200, 70"),
			SRDescription("DescriptionAttributeLegendCell_SeriesSymbolSize"),
			]
			public virtual Size SeriesSymbolSize
			{
				get
				{
					return this._seriesSymbolSize;
				}
				set
				{
					if(value.Width < 0 || value.Height < 0)
					{
                        throw (new ArgumentException(SR.ExceptionLegendCellSeriesSymbolSizeIsNegative, "value"));
					}
					this._seriesSymbolSize = value;
					this.Invalidate();
				}
			}

			/// <summary>
			/// Gets or sets the content alignment of the legend cell.
			/// </summary>
			[
			SRCategory("CategoryAttributeLayout"),
			DefaultValue(ContentAlignment.MiddleCenter),
			SRDescription("DescriptionAttributeLegendCell_Alignment"),
			]
			public virtual ContentAlignment Alignment
			{
				get
				{
					return this._alignment;
				}
				set
				{
					this._alignment = value;
					this.Invalidate();
				}
			}

			/// <summary>
			/// Gets or sets the number of horizontal cells used to draw the content.
			/// </summary>
			[
			SRCategory("CategoryAttributeLayout"),
			DefaultValue(1),
			SRDescription("DescriptionAttributeLegendCell_CellSpan"),
			]
			public virtual int CellSpan
			{
				get
				{
					return this._cellSpan;
				}
				set
				{
					if(value < 1)
					{
                        throw (new ArgumentException(SR.ExceptionLegendCellSpanIsLessThenOne, "value"));
					}
					this._cellSpan = value;
					this.Invalidate();
				}
			}

			/// <summary>
			/// Gets or sets the legend cell margins (as a percentage of legend font size).
			/// </summary>
			[
			SRCategory("CategoryAttributeLayout"),
			DefaultValue(typeof(Margins), "0,0,15,15"),
			SRDescription("DescriptionAttributeLegendCell_Margins"),
			SerializationVisibilityAttribute(SerializationVisibility.Attribute),
			NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
			PersistenceMode(PersistenceMode.InnerProperty),
#endif
			]
			public virtual Margins Margins
			{
				get
				{
					return this._margins;
				}
				set
				{
					this._margins = value;
					this.Invalidate();

#if Microsoft_CONTROL
					// Set common elements of the new margins class
					if(this.Legend != null)
					{
						this._margins.Common = this.Common;
					}
#endif // Microsoft_CONTROL
				}
			}

			/// <summary>
			/// Returns true if property should be serialized.  This method is for internal use only.
			/// </summary>
			[EditorBrowsableAttribute(EditorBrowsableState.Never)]
			internal bool ShouldSerializeMargins()
			{
				if(this._margins.Top == 0 &&
					this._margins.Bottom == 0 &&
					this._margins.Left == 15 &&
					this._margins.Right == 15 )
				{
					return false;
				}
				return true;
			}

			/// <summary>
			/// Gets or sets the tooltip of the legend cell.
			/// </summary>
			[
			SRCategory("CategoryAttributeMapArea"),
            SRDescription("DescriptionAttributeToolTip"),
			DefaultValue(""),
			]
			public virtual string ToolTip
			{
				set
				{
					this._toolTip = value;
#if Microsoft_CONTROL
					if(this.Chart != null && 
					   this.Chart.selection != null)
					{
						this.Chart.selection.enabledChecked = false;
					}
#endif
				}
				get
				{
					return this._toolTip;
				}
			}
			
#if !Microsoft_CONTROL

			/// <summary>
            /// Gets or sets the URL target of the legend cell.
			/// </summary>
			[
			SRCategory("CategoryAttributeMapArea"),
			SRDescription("DescriptionAttributeUrl"),
			DefaultValue(""),
            Editor(Editors.UrlValueEditor.Editor, Editors.UrlValueEditor.Base)
			]
			public virtual string Url
			{
				set
				{
					this._url = value;
				}
				get
				{
					return this._url;
				}
			}

			/// <summary>
            /// Gets or sets the other map area attributes of the legend cell.
			/// </summary>
			[
			SRCategory("CategoryAttributeMapArea"),
			SRDescription("DescriptionAttributeMapAreaAttributes"),
			DefaultValue(""),
			]
			public virtual string MapAreaAttributes
			{
				set
				{
					this._mapAreaAttribute = value;
				}
				get
				{
					return this._mapAreaAttribute;
				}
			}

            /// <summary>
            /// Gets or sets the postback value which can be processed on a click event.
            /// </summary>
            /// <value>The value which is passed to a click event as an argument.</value>
            [DefaultValue("")]
            [SRCategory(SR.Keys.CategoryAttributeMapArea)]
            [SRDescription(SR.Keys.DescriptionAttributePostBackValue)]
            public string PostBackValue 
            {
                get
                {
                    return this._postbackValue;
                }
                set
                {
                    this._postbackValue = value;
                }
            }


#endif  // !Microsoft_CONTROL

            #endregion // Properties

            #region Methods

            /// <summary>
			/// Resets cached cell values.
			/// </summary>
			internal void ResetCache()
			{
				this._cachedCellSize = Size.Empty;
				this._cachedCellSizeFontReducedBy = 0;
			}

			/// <summary>
			/// Sets cell position in relative coordinates.
			/// </summary>
			/// <param name="rowIndex">Cell row index.</param>
			/// <param name="position">Cell position.</param>
			/// <param name="singleWCharacterSize">Size of the 'W' character used to calculate elements.</param>
			internal void SetCellPosition(
				int rowIndex,
				Rectangle position, 
				Size singleWCharacterSize)
			{
				// Set cell position 
				this.cellPosition = position;
				this.cellPositionWithMargins = position;
				this._rowIndex = rowIndex;

				// Adjust cell position by specified margin
				this.cellPosition.X += (int)(this.Margins.Left * singleWCharacterSize.Width / 100f);
				this.cellPosition.Y += (int)(this.Margins.Top * singleWCharacterSize.Height / 100f);
				this.cellPosition.Width -= (int)(this.Margins.Left * singleWCharacterSize.Width / 100f)
					+ (int)(this.Margins.Right * singleWCharacterSize.Width / 100f);
				this.cellPosition.Height -= (int)(this.Margins.Top * singleWCharacterSize.Height / 100f)
					+ (int)(this.Margins.Bottom * singleWCharacterSize.Height / 100f);

				// Adjust cell position by space required for the separatorType
				if( LegendItem != null && 
					LegendItem.SeparatorType != LegendSeparatorStyle.None)
				{
					this.cellPosition.Height -= this.Legend.GetSeparatorSize(LegendItem.SeparatorType).Height;
				}
			}

			/// <summary>
			/// Measures legend cell size in chart relative coordinates.
			/// </summary>
			/// <param name="graph">
			/// Chart graphics.
			/// </param>
			/// <param name="fontSizeReducedBy">
			/// A positive or negative integer value that determines the how standard cell font size
			/// should be adjusted. As a result smaller or larger font can be used.
			/// </param>
			/// <param name="legendAutoFont">
			/// Auto fit font used in the legend.
			/// </param>
			/// <param name="singleWCharacterSize">
			/// Size of the 'W' character used to calculate elements.
			/// </param>
			/// <returns>Legend cell size.</returns>
            internal Size MeasureCell(
				ChartGraphics graph, 
				int fontSizeReducedBy,
				Font legendAutoFont,
				Size singleWCharacterSize)
			{
				// Check if cached size may be reused
				if(this._cachedCellSizeFontReducedBy == fontSizeReducedBy &&
					!this._cachedCellSize.IsEmpty)
				{
					return this._cachedCellSize;
				}

				// Get cell font
				Size cellSize = Size.Empty;
				bool disposeFont = false;
				Font cellFont = this.GetCellFont(legendAutoFont, fontSizeReducedBy, out disposeFont);
			
				// Measure cell content size based on the type
				if(this.CellType == LegendCellType.SeriesSymbol)
				{
					cellSize.Width = (int)(Math.Abs(this.SeriesSymbolSize.Width) * singleWCharacterSize.Width / 100f);
					cellSize.Height = (int)(Math.Abs(this.SeriesSymbolSize.Height) * singleWCharacterSize.Height / 100f);
				}
				else if(this.CellType == LegendCellType.Image)
				{
					if(this.ImageSize.IsEmpty && this.Image.Length > 0)
					{
                        SizeF imageSize = new SizeF();

						// Use original image size
                        if (this.Common.ImageLoader.GetAdjustedImageSize(this.Image, graph.Graphics, ref imageSize))
                        {
                            cellSize.Width = (int)imageSize.Width;
                            cellSize.Height = (int)imageSize.Height;
                        }
					}
					else
					{
						cellSize.Width = (int)(Math.Abs(this.ImageSize.Width) * singleWCharacterSize.Width / 100f);
						cellSize.Height = (int)(Math.Abs(this.ImageSize.Height) * singleWCharacterSize.Height / 100f);
					}
				}
				else if(this.CellType == LegendCellType.Text)
				{
					// Get current cell text taking in consideration keywords
					// and automatic text wrapping.
					string cellText = this.GetCellText();

					// Measure text size.
					// Note that extra "I" character added to add more horizontal spacing
					cellSize =  graph.MeasureStringAbs(cellText + "I", cellFont);
				}
				else
				{
                    throw (new InvalidOperationException(SR.ExceptionLegendCellTypeUnknown(this.CellType.ToString())));
				}

				// Add cell margins 
				cellSize.Width += (int)((this.Margins.Left + this.Margins.Right) * singleWCharacterSize.Width / 100f);
				cellSize.Height += (int)((this.Margins.Top + this.Margins.Bottom) * singleWCharacterSize.Height / 100f);

				// Add space required for the separatorType
				if( LegendItem != null && 
					LegendItem.SeparatorType != LegendSeparatorStyle.None)
				{
					cellSize.Height += this.Legend.GetSeparatorSize(LegendItem.SeparatorType).Height;
				}

				// Dispose created font object
				if(disposeFont)
				{
					cellFont.Dispose();
					cellFont = null;
				}

				// Save calculated size
				this._cachedCellSize = cellSize;
				this._cachedCellSizeFontReducedBy = fontSizeReducedBy;

				return cellSize;
			}

			/// <summary>
			/// Gets cell background color.
			/// </summary>
			/// <returns></returns>
			private Color GetCellBackColor()
			{
				Color resultColor = this.BackColor;
				if(this.BackColor.IsEmpty && this.Legend != null)
				{
					// Try getting back color from the associated column
					if(this.LegendItem != null)
					{
						// Get index of this cell
						int cellIndex = this.LegendItem.Cells.IndexOf(this);
						if(cellIndex >= 0)
						{
							// Check if associated column exsists
							if(cellIndex < this.Legend.CellColumns.Count && 
								!this.Legend.CellColumns[cellIndex].BackColor.IsEmpty)
							{
								resultColor = this.Legend.CellColumns[cellIndex].BackColor;
							}
						}
					}

					// Get font from the legend isInterlaced 
					if(resultColor.IsEmpty && 
						this.Legend.InterlacedRows &&
						this._rowIndex % 2 != 0)
					{
						if(this.Legend.InterlacedRowsColor.IsEmpty)
						{
							// Automatically determine background color
							// If isInterlaced strips color is not set - use darker color of the area
							if(this.Legend.BackColor == Color.Empty)
							{
								resultColor = Color.LightGray;
							}
							else if(this.Legend.BackColor == Color.Transparent)
							{
								if(Chart.BackColor != Color.Transparent && 
									Chart.BackColor != Color.Black)
								{
									resultColor = ChartGraphics.GetGradientColor( Chart.BackColor, Color.Black, 0.2 );
								}
								else
								{
									resultColor = Color.LightGray;
								}
							}
							else
							{
								resultColor = ChartGraphics.GetGradientColor( this.Legend.BackColor, Color.Black, 0.2 );
							}
						}
						else
						{
							resultColor = this.Legend.InterlacedRowsColor;
						}
					}
				}
				return resultColor;
			}

			/// <summary>
			/// Gets default cell font. Font can be specified in the cell, column or in the legend.
			/// </summary>
			/// <param name="legendAutoFont">Auto fit font used in the legend.</param>
			/// <param name="fontSizeReducedBy">Number of points legend auto-font reduced by.</param>
			/// <param name="disposeFont">Returns a flag if result font object should be disposed.</param>
			/// <returns>Default cell font.</returns>
			private Font GetCellFont(Font legendAutoFont, int fontSizeReducedBy, out bool disposeFont)
			{
				Font cellFont = this.Font;
				disposeFont = false;

				// Check if font is not set in the cell and legend object reference is valid
				if(cellFont == null && 
					this.Legend != null)
				{
					// Try getting font from the associated column
					if(this.LegendItem != null)
					{
						// Get index of this cell
						int cellIndex = this.LegendItem.Cells.IndexOf(this);
						if(cellIndex >= 0)
						{
							// Check if associated column exsists
							if(cellIndex < this.Legend.CellColumns.Count && 
								this.Legend.CellColumns[cellIndex].Font != null)
							{
								cellFont = this.Legend.CellColumns[cellIndex].Font;
							}
						}
					}

					// Get font from the legend
					if(cellFont == null)
					{
						cellFont = legendAutoFont;

						// No further processing required.
						// Font is already reduced.
						return cellFont;
					}
				}

				// Check if font size should be adjusted
				if(cellFont != null && fontSizeReducedBy != 0)
				{
					// New font is created anf it must be disposed
					disposeFont = true;

					// Calculate new font size
					int newFontSize = (int)Math.Round(cellFont.Size - fontSizeReducedBy);
					if(newFontSize < 1)
					{
						// Font can't be less than size 1
						newFontSize = 1;
					}

					// Create new font
					cellFont = new Font(
						cellFont.FontFamily, 
						newFontSize, 
						cellFont.Style, 
						cellFont.Unit);
				}

				return cellFont;
			}

			/// <summary>
			/// Helper function that returns cell tooltip.
			/// </summary>
			/// <remarks>
			/// Tooltip can be set in the cell or in the legend item. Cell 
			/// tooltip always has a higher priority.
			/// </remarks>
			/// <returns>Returns cell text.</returns>
			private string GetCellToolTip()
			{
				// Check if tooltip is set in the cell (highest priority)
				if(this.ToolTip.Length > 0)
				{
					return this.ToolTip;
				}

				// Check if tooltip is set in associated legend item
				if(this.LegendItem != null)
				{
					return this.LegendItem.ToolTip;
				}

				return string.Empty;
			}

			/// <summary>
			/// Helper function that returns cell url.
			/// </summary>
			/// <remarks>
			/// Url can be set in the cell or in the legend item. Cell 
			/// tooltip always has a higher priority.
			/// </remarks>
			/// <returns>Returns cell text.</returns>
			private string GetCellUrl()
			{
#if !Microsoft_CONTROL
				// Check if tooltip is set in the cell (highest priority)
				if(this._url.Length > 0)
				{
					return this._url;
				}

				// Check if tooltip is set in associated legend item
				if(this.LegendItem != null)
				{
                    return this.LegendItem.Url;
				}
#endif // !Microsoft_CONTROL
				return string.Empty;
			}

			/// <summary>
			/// Helper function that returns cell url.
			/// </summary>
			/// <remarks>
			/// Url can be set in the cell or in the legend item. Cell 
			/// tooltip always has a higher priority.
			/// </remarks>
			/// <returns>Returns cell text.</returns>
			private string GetCellMapAreaAttributes()
			{
#if !Microsoft_CONTROL
				// Check if tooltip is set in the cell (highest priority)
				if(this._mapAreaAttribute.Length > 0)
				{
					return this._mapAreaAttribute;
				}

				// Check if tooltip is set in associated legend item
				if(this.LegendItem != null)
				{
					return this.LegendItem.MapAreaAttributes;
				}
#endif // !Microsoft_CONTROL
				return string.Empty;
			}

            /// <summary>
            /// Helper function that returns cell url.
            /// </summary>
            /// <remarks>
            /// Url can be set in the cell or in the legend item. Cell 
            /// tooltip always has a higher priority.
            /// </remarks>
            /// <returns>Returns cell text.</returns>
            private string GetCellPostBackValue()
            {
#if !Microsoft_CONTROL
                // Check if tooltip is set in the cell (highest priority)
                if (this._postbackValue.Length > 0)
                {
                    return this._postbackValue;
                }

                // Check if tooltip is set in associated legend item
                if (this.LegendItem != null)
                {
                    return this.LegendItem.PostBackValue;
                }
#endif // !Microsoft_CONTROL
                return string.Empty;
            }

			/// <summary>
			/// Helper function that returns the exact text presented in the cell.
			/// </summary>
			/// <remarks>
			/// This method replaces the "\n" substring with the new line character 
			/// and automatically wrap text if required.
			/// </remarks>
			/// <returns>Returns cell text.</returns>
			private string GetCellText()
			{
				// Replace all "\n" strings with the new line character
				string resultString = this.Text.Replace("\\n", "\n");

				// Replace the KeywordName.LegendText keyword with legend item Name property
				if(this.LegendItem != null)
				{
					resultString = resultString.Replace(KeywordName.LegendText, this.LegendItem.Name);
				}
				else
				{
					resultString = resultString.Replace(KeywordName.LegendText, "");
				}

				// Check if text width exceeds recomended character length
				if(this.Legend != null)
				{
					int recomendedTextLength = this.Legend.TextWrapThreshold;

					if(recomendedTextLength > 0 &&
						resultString.Length > recomendedTextLength)
					{
						// Iterate through all text characters
						int lineLength = 0;
						for(int charIndex = 0; charIndex < resultString.Length; charIndex++)
						{
							// Reset line length when new line character is found
							if(resultString[charIndex] == '\n')
							{
								lineLength = 0;
								continue;
							}

							// Increase line length counter
							++lineLength;

							// Check if current character is a white space and
							// current line length exceeds the recomended values.
							if(char.IsWhiteSpace(resultString, charIndex) &&
								lineLength >= recomendedTextLength)
							{
								// Insert new line character in the string
								lineLength = 0;
								resultString = resultString.Substring(0, charIndex) + "\n" + 
									resultString.Substring(charIndex + 1).TrimStart();
							}
						}
					}
				}

				return resultString;
			}

			/// <summary>
			/// Helper function that returns cell text color.
			/// </summary>
			/// <returns>Cell text color.</returns>
			private Color GetCellForeColor()
			{
				// Check if cell text color defined in the cell
				if(!this.ForeColor.IsEmpty)
				{
					return this.ForeColor;
				}

				// Check if color from the Column or legend should be used
				if(this.Legend != null)
				{
					// Try getting font from the associated column
					if(this.LegendItem != null)
					{
						// Get index of this cell
						int cellIndex = this.LegendItem.Cells.IndexOf(this);
						if(cellIndex >= 0)
						{
							// Check if associated column exsists
							if(cellIndex < this.Legend.CellColumns.Count && 
								!this.Legend.CellColumns[cellIndex].ForeColor.IsEmpty)
							{
								return this.Legend.CellColumns[cellIndex].ForeColor;
							}
						}
					}

					// Use legend text color
					return this.Legend.ForeColor;
				}

				return Color.Black;
			}

			#endregion // Methods

			#region Cell Painting Methods

			/// <summary>
			/// Paints content of the legend cell.
			/// </summary>
			/// <param name="chartGraph">Chart graphics to draw content on.</param>
			/// <param name="fontSizeReducedBy">Number that determines how much the cell font should be reduced by.</param>
			/// <param name="legendAutoFont">Auto-fit font used in the legend.</param>
			/// <param name="singleWCharacterSize">Size of the 'W' character in auto-fit font.</param>
			internal void Paint(
				ChartGraphics chartGraph, 
				int fontSizeReducedBy,
				Font legendAutoFont,
				Size singleWCharacterSize)
			{
				// Check cell size before painting
				if(this.cellPosition.Width <= 0 || this.cellPosition.Height <= 0)
				{
					return;
				}

				// Chart elements painting mode
				if( this.Common.ProcessModePaint )
				{
					// Check if cell background should be painted
					Color cellBackColor = this.GetCellBackColor();
					RectangleF rectRelative = chartGraph.GetRelativeRectangle(this.cellPositionWithMargins);
					if(!cellBackColor.IsEmpty)
					{
						chartGraph.FillRectangleRel( 
							rectRelative, 
							cellBackColor, 
							ChartHatchStyle.None,
							string.Empty,
							ChartImageWrapMode.Tile, 
							Color.Empty,
							ChartImageAlignmentStyle.Center,
							GradientStyle.None, 
							Color.Empty,
							Color.Empty, 
							0, 
							ChartDashStyle.NotSet, 
							Color.Empty, 
							0,
							PenAlignment.Inset);
					}

					// Fire an event for custom cell back drawing
                    this.Chart.CallOnPrePaint(new ChartPaintEventArgs(this, chartGraph, this.Common, new ElementPosition(rectRelative.X, rectRelative.Y, rectRelative.Width, rectRelative.Height)));

					// Check legend cell type
					switch(this.CellType)
					{
						case(LegendCellType.Text):
							this.PaintCellText(chartGraph, fontSizeReducedBy, legendAutoFont);
							break;
						case(LegendCellType.Image):
							this.PaintCellImage(chartGraph, singleWCharacterSize);
							break;
						case(LegendCellType.SeriesSymbol):
							this.PaintCellSeriesSymbol(chartGraph, singleWCharacterSize);
							break;
						default:
                            				throw (new InvalidOperationException(SR.ExceptionLegendCellTypeUnknown(this.CellType.ToString())));
					}

					// Fire an event for custom cell drawing
                    this.Chart.CallOnPostPaint(new ChartPaintEventArgs(this, chartGraph, this.Common, new ElementPosition(rectRelative.X, rectRelative.Y, rectRelative.Width, rectRelative.Height)));
				}
#if DEBUG
				// Draw bounding rectangle for debug purpose
//				RectangleF absRectangle = this.cellPosition;
//				chartGraph.DrawRectangle(Pens.Red, absRectangle.X, absRectangle.Y, absRectangle.Width, absRectangle.Height);
#endif // DEBUG

				// Legend cell selection mode
				if( this.Common.ProcessModeRegions )
				{
					// Add hot region.
					// Note that legend cell is passed as sub-object of legend item
					this.Common.HotRegionsList.AddHotRegion(
						chartGraph.GetRelativeRectangle(this.cellPositionWithMargins),
						this.GetCellToolTip(),
						this.GetCellUrl(),
						this.GetCellMapAreaAttributes(),
                        this.GetCellPostBackValue(),
						this.LegendItem,
						this,
						ChartElementType.LegendItem,
						this.LegendItem.SeriesName);
				}
			}

			/// <summary>
			/// Draw legend cell text.
			/// </summary>
			/// <param name="chartGraph">Chart graphics to draw the text on.</param>
			/// <param name="fontSizeReducedBy">Number that determines how much the cell font should be reduced by.</param>
			/// <param name="legendAutoFont">Auto-fit font used in the legend.</param>
			private void PaintCellText(
				ChartGraphics chartGraph, 
				int fontSizeReducedBy,
				Font legendAutoFont)
			{
				// Get cell font
				bool disposeFont = false;
				Font cellFont = this.GetCellFont(legendAutoFont, fontSizeReducedBy, out disposeFont);

				// Start Svg Selection mode
				chartGraph.StartHotRegion( this.GetCellUrl(), this.GetCellToolTip() );

				// Create font brush
				using(SolidBrush fontBrush = new SolidBrush(this.GetCellForeColor()))
				{
					// Create cell text format
                    using (StringFormat format = new StringFormat(StringFormat.GenericDefault))
                    {
                        format.FormatFlags = StringFormatFlags.LineLimit;
                        format.Trimming = StringTrimming.EllipsisCharacter;
                        format.Alignment = StringAlignment.Center;
                        if (this.Alignment == ContentAlignment.BottomLeft ||
                            this.Alignment == ContentAlignment.MiddleLeft ||
                            this.Alignment == ContentAlignment.TopLeft)
                        {
                            format.Alignment = StringAlignment.Near;
                        }
                        else if (this.Alignment == ContentAlignment.BottomRight ||
                            this.Alignment == ContentAlignment.MiddleRight ||
                            this.Alignment == ContentAlignment.TopRight)
                        {
                            format.Alignment = StringAlignment.Far;
                        }
                        format.LineAlignment = StringAlignment.Center;
                        if (this.Alignment == ContentAlignment.BottomCenter ||
                            this.Alignment == ContentAlignment.BottomLeft ||
                            this.Alignment == ContentAlignment.BottomRight)
                        {
                            format.LineAlignment = StringAlignment.Far;
                        }
                        else if (this.Alignment == ContentAlignment.TopCenter ||
                            this.Alignment == ContentAlignment.TopLeft ||
                            this.Alignment == ContentAlignment.TopRight)
                        {
                            format.LineAlignment = StringAlignment.Near;
                        }

                        // Measure string height out of one character
                        SizeF charSize = chartGraph.MeasureStringAbs(this.GetCellText(), cellFont, new SizeF(10000f, 10000f), format);

                        // If height of one characte is more than rectangle heigjt - remove LineLimit flag
                        if (charSize.Height > this.cellPosition.Height && (format.FormatFlags & StringFormatFlags.LineLimit) != 0)
                        {
                            format.FormatFlags ^= StringFormatFlags.LineLimit;
                        }

                        else if (charSize.Height < this.cellPosition.Height && (format.FormatFlags & StringFormatFlags.LineLimit) == 0)
                        {
                            format.FormatFlags |= StringFormatFlags.LineLimit;
                        }

                        // Draw text
                        chartGraph.DrawStringRel(
                            this.GetCellText(),
                            cellFont,
                            fontBrush,
                            chartGraph.GetRelativeRectangle(this.cellPosition),
                            format);
                    }
				}

				// End Svg Selection mode
				chartGraph.EndHotRegion( );

				// Dispose created cell font object
				if(disposeFont)
				{
					cellFont.Dispose();
					cellFont = null;
				}
			}

			/// <summary>
			/// Paints cell image.
			/// </summary>
			/// <param name="chartGraph">Graphics used to draw cell image.</param>
			/// <param name="singleWCharacterSize">Size of the 'W' character in auto-fit font.</param>
			private void PaintCellImage(
				ChartGraphics chartGraph, 
				Size singleWCharacterSize)
			{
				if(this.Image.Length > 0)
				{
					// Get image size in relative coordinates
					Rectangle	imagePosition = Rectangle.Empty;
                    System.Drawing.Image image = this.Common.ImageLoader.LoadImage(this.Image);
                                        
                    SizeF imageSize = new SizeF();

                    ImageLoader.GetAdjustedImageSize(image, chartGraph.Graphics, ref imageSize);

                    imagePosition.Width = (int)imageSize.Width;
                    imagePosition.Height = (int)imageSize.Height;

					// Calculate cell position
					Rectangle imageCellPosition = this.cellPosition;
					imageCellPosition.Width = imagePosition.Width;
					imageCellPosition.Height = imagePosition.Height;
					if(!this.ImageSize.IsEmpty)
					{
						// Adjust cell size using image symbol size specified
						if(this.ImageSize.Width > 0)
						{
							int newWidth = (int)(this.ImageSize.Width * singleWCharacterSize.Width / 100f);
							if(newWidth > this.cellPosition.Width)
							{
								newWidth = this.cellPosition.Width;
							}
							imageCellPosition.Width = newWidth;
						}
						if(this.ImageSize.Height > 0)
						{
							int newHeight = (int)(this.ImageSize.Height * singleWCharacterSize.Height / 100f);
							if(newHeight > this.cellPosition.Height)
							{
								newHeight = this.cellPosition.Height;
							}
							imageCellPosition.Height = newHeight;
						}
					}

					// Make sure image size fits into the cell drawing rectangle
					float	scaleValue = 1f;
					if(imagePosition.Height > imageCellPosition.Height)
					{
						scaleValue = (float)imagePosition.Height / (float)imageCellPosition.Height;
					}
					if(imagePosition.Width > imageCellPosition.Width)
					{
						scaleValue = Math.Max(scaleValue, (float)imagePosition.Width / (float)imageCellPosition.Width);
					}

					// Scale image size
					imagePosition.Height = (int)(imagePosition.Height / scaleValue);
					imagePosition.Width = (int)(imagePosition.Width / scaleValue);

					// Get image location
					imagePosition.X = (int)((this.cellPosition.X + this.cellPosition.Width/2f) - imagePosition.Width/2f);
					imagePosition.Y = (int)((this.cellPosition.Y + this.cellPosition.Height/2f) - imagePosition.Height/2f);

					// Adjust image location based on the cell content alignment
					if(this.Alignment == ContentAlignment.BottomLeft || 
						this.Alignment == ContentAlignment.MiddleLeft ||
						this.Alignment == ContentAlignment.TopLeft)
					{
						imagePosition.X = this.cellPosition.X;
					}
					else if(this.Alignment == ContentAlignment.BottomRight || 
						this.Alignment == ContentAlignment.MiddleRight ||
						this.Alignment == ContentAlignment.TopRight)
					{
						imagePosition.X = this.cellPosition.Right - imagePosition.Width;
					}
					
					if(this.Alignment == ContentAlignment.BottomCenter || 
						this.Alignment == ContentAlignment.BottomLeft ||
						this.Alignment == ContentAlignment.BottomRight)
					{
						imagePosition.Y = this.cellPosition.Bottom - imagePosition.Height;
					}
					else if(this.Alignment == ContentAlignment.TopCenter || 
						this.Alignment == ContentAlignment.TopLeft ||
						this.Alignment == ContentAlignment.TopRight)
					{
						imagePosition.Y = this.cellPosition.Y;
					}

					// Set image transparent color
					System.Drawing.Imaging.ImageAttributes imageAttributes = new System.Drawing.Imaging.ImageAttributes();
					if(this.ImageTransparentColor != Color.Empty)
					{
						imageAttributes.SetColorKey(this.ImageTransparentColor, this.ImageTransparentColor, System.Drawing.Imaging.ColorAdjustType.Default);
					}

					// Increase quality of image scaling
					SmoothingMode oldSmoothingMode = chartGraph.SmoothingMode;
					CompositingQuality oldCompositingQuality = chartGraph.Graphics.CompositingQuality;
					InterpolationMode oldInterpolationMode = chartGraph.Graphics.InterpolationMode;
					chartGraph.SmoothingMode = SmoothingMode.AntiAlias;
					chartGraph.Graphics.CompositingQuality = CompositingQuality.HighQuality;
					chartGraph.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

					// Draw image
					chartGraph.DrawImage(
						image, 
						imagePosition, 
						0, 
						0, 
						image.Width,
						image.Height,
						GraphicsUnit.Pixel,
						imageAttributes);

					// Restore graphics settings
					chartGraph.SmoothingMode = oldSmoothingMode;
					chartGraph.Graphics.CompositingQuality = oldCompositingQuality;
					chartGraph.Graphics.InterpolationMode = oldInterpolationMode;
				}
			}

            /// <summary>
            /// Paint a series symbol in the cell.
            /// </summary>
            /// <param name="chartGraph">Chart graphics</param>
            /// <param name="singleWCharacterSize">Size of the 'W' character in auto-fit font.</param>
			private void PaintCellSeriesSymbol(
				ChartGraphics chartGraph, 
				SizeF singleWCharacterSize)
			{
                //Cache legend item
                LegendItem legendItem = this.LegendItem;

				// Calculate cell position
				Rectangle seriesMarkerPosition = this.cellPosition;

				// Adjust cell size using image symbol size specified
				if(this.SeriesSymbolSize.Width >= 0)
				{
					int newWidth = (int)(this.SeriesSymbolSize.Width * singleWCharacterSize.Width / 100f);
					if(newWidth > this.cellPosition.Width)
					{
						newWidth = this.cellPosition.Width;
					}
					seriesMarkerPosition.Width = newWidth;
				}
				if(this.SeriesSymbolSize.Height >= 0)
				{
					int newHeight = (int)(this.SeriesSymbolSize.Height * singleWCharacterSize.Height / 100f);
					if(newHeight > this.cellPosition.Height)
					{
						newHeight = this.cellPosition.Height;
					}
					seriesMarkerPosition.Height = newHeight;
				}

				// Check for empty size
				if(seriesMarkerPosition.Height <= 0 || seriesMarkerPosition.Width <= 0)
				{
					return;
				}

				// Get symbol location
				seriesMarkerPosition.X = (int)((this.cellPosition.X + this.cellPosition.Width/2f) - seriesMarkerPosition.Width/2f);
				seriesMarkerPosition.Y = (int)((this.cellPosition.Y + this.cellPosition.Height/2f) - seriesMarkerPosition.Height/2f);

				// Adjust image location based on the cell content alignment
				if(this.Alignment == ContentAlignment.BottomLeft || 
					this.Alignment == ContentAlignment.MiddleLeft ||
					this.Alignment == ContentAlignment.TopLeft)
				{
					seriesMarkerPosition.X = this.cellPosition.X;
				}
				else if(this.Alignment == ContentAlignment.BottomRight || 
					this.Alignment == ContentAlignment.MiddleRight ||
					this.Alignment == ContentAlignment.TopRight)
				{
					seriesMarkerPosition.X = this.cellPosition.Right - seriesMarkerPosition.Width;
				}
				
				if(this.Alignment == ContentAlignment.BottomCenter || 
					this.Alignment == ContentAlignment.BottomLeft ||
					this.Alignment == ContentAlignment.BottomRight)
				{
					seriesMarkerPosition.Y = this.cellPosition.Bottom - seriesMarkerPosition.Height;
				}
				else if(this.Alignment == ContentAlignment.TopCenter || 
					this.Alignment == ContentAlignment.TopLeft ||
					this.Alignment == ContentAlignment.TopRight)
				{
					seriesMarkerPosition.Y = this.cellPosition.Y;
				}

				// Start Svg Selection mode
				chartGraph.StartHotRegion( this.GetCellUrl(), this.GetCellToolTip() );

				// Draw legend item image
				if(legendItem.Image.Length > 0)
				{
					// Get image size
					Rectangle	imageScale = Rectangle.Empty;
                    System.Drawing.Image image = this.Common.ImageLoader.LoadImage(legendItem.Image);
                                       
                    if (image != null)
                    {
                        SizeF imageSize = new SizeF();

                        ImageLoader.GetAdjustedImageSize(image, chartGraph.Graphics, ref imageSize);

                        imageScale.Width = (int)imageSize.Width;
                        imageScale.Height = (int)imageSize.Height;

                        // Make sure image size fits into the drawing rectangle
                        float scaleValue = 1f;
                        if (imageScale.Height > seriesMarkerPosition.Height)
                        {
                            scaleValue = (float)imageScale.Height / (float)seriesMarkerPosition.Height;
                        }
                        if (imageScale.Width > seriesMarkerPosition.Width)
                        {
                            scaleValue = Math.Max(scaleValue, (float)imageScale.Width / (float)seriesMarkerPosition.Width);
                        }

                        // Scale image size
                        imageScale.Height = (int)(imageScale.Height / scaleValue);
                        imageScale.Width = (int)(imageScale.Width / scaleValue);

                        imageScale.X = (int)((seriesMarkerPosition.X + seriesMarkerPosition.Width / 2f) - imageScale.Width / 2f);
                        imageScale.Y = (int)((seriesMarkerPosition.Y + seriesMarkerPosition.Height / 2f) - imageScale.Height / 2f);

                        // Set image transparent color
                        System.Drawing.Imaging.ImageAttributes imageAttributes = new System.Drawing.Imaging.ImageAttributes();
                        if (legendItem.BackImageTransparentColor != Color.Empty)
                        {
                            imageAttributes.SetColorKey(legendItem.BackImageTransparentColor, legendItem.BackImageTransparentColor, System.Drawing.Imaging.ColorAdjustType.Default);
                        }

                        // Draw image
                        chartGraph.DrawImage(
                            image,
                            imageScale,
                            0,
                            0,
                            image.Width,
                            image.Height,
                            GraphicsUnit.Pixel,
                            imageAttributes);
                    }
				}

				else
				{
                    int maxShadowOffset = (int)Math.Round((3 * chartGraph.Graphics.DpiX) / 96);
                    int maxBorderWidth = (int)Math.Round((3 * chartGraph.Graphics.DpiX) / 96);

					if(legendItem.ImageStyle == LegendImageStyle.Rectangle)
					{
                        int maxBorderWidthRect = (int)Math.Round((2 * chartGraph.Graphics.DpiX) / 96);
                    
						// Draw series rectangle
						chartGraph.FillRectangleRel(
							chartGraph.GetRelativeRectangle(seriesMarkerPosition), 
							legendItem.Color, 
							legendItem.BackHatchStyle, 
							legendItem.Image, 
							legendItem.backImageWrapMode, 
							legendItem.BackImageTransparentColor,
							legendItem.backImageAlign, 
							legendItem.backGradientStyle, 
							legendItem.backSecondaryColor,
							legendItem.borderColor,
                            (legendItem.BorderWidth > maxBorderWidthRect) ? maxBorderWidthRect : legendItem.BorderWidth,
							legendItem.BorderDashStyle,
							legendItem.ShadowColor,
                            (legendItem.ShadowOffset > maxShadowOffset) ? maxShadowOffset : legendItem.ShadowOffset,
							PenAlignment.Inset);
					}
					if(legendItem.ImageStyle == LegendImageStyle.Line)
					{
						// Prepare line coordinates
						Point	point1 = new Point();
						point1.X = seriesMarkerPosition.X;
						point1.Y = (int)(seriesMarkerPosition.Y + seriesMarkerPosition.Height/2F);
						Point	point2 = new Point();
						point2.Y = point1.Y;
						point2.X = seriesMarkerPosition.Right;

						// Disable antialiasing
						SmoothingMode oldMode = chartGraph.SmoothingMode;
						chartGraph.SmoothingMode = SmoothingMode.None;
                        
						// Draw line
						chartGraph.DrawLineRel(
							legendItem.Color,
                            (legendItem.borderWidth > maxBorderWidth) ? maxBorderWidth : legendItem.borderWidth,
							legendItem.borderDashStyle, 
							chartGraph.GetRelativePoint(point1), 
							chartGraph.GetRelativePoint(point2),
							legendItem.shadowColor,
                            (legendItem.shadowOffset > maxShadowOffset) ? maxShadowOffset : legendItem.shadowOffset);

						// Restore antialiasing mode
						chartGraph.SmoothingMode = oldMode;
					}
				
					// Draw symbol (for line also)
					if(legendItem.ImageStyle == LegendImageStyle.Marker ||
						legendItem.ImageStyle == LegendImageStyle.Line)
					{
						MarkerStyle	markerStyle = legendItem.markerStyle;
						if(legendItem.style == LegendImageStyle.Marker)
						{
							markerStyle = (legendItem.markerStyle == MarkerStyle.None) ? 
								MarkerStyle.Circle : legendItem.markerStyle;
						}

						if(markerStyle != MarkerStyle.None || 
							legendItem.markerImage.Length > 0)
						{
							// Calculate marker size
							int	markerSize = (int)Math.Min(seriesMarkerPosition.Width, seriesMarkerPosition.Height);
							markerSize = (int)Math.Min(legendItem.markerSize, (legendItem.style == LegendImageStyle.Line) ? 2f*(markerSize/3f) : markerSize);

							// Reduce marker size to fit border
                            int markerBorderWidth = (legendItem.MarkerBorderWidth > maxBorderWidth) ? maxBorderWidth : legendItem.MarkerBorderWidth;
							if(markerBorderWidth > 0)
							{
								markerSize -= markerBorderWidth;
								if(markerSize < 1)
								{
									markerSize = 1;
								}
							}
							
							// Draw marker
							Point	point = new Point();
							point.X = (int)(seriesMarkerPosition.X + seriesMarkerPosition.Width/2f);
							point.Y = (int)(seriesMarkerPosition.Y + seriesMarkerPosition.Height/2f);

							// Calculate image scale
							Rectangle	imageScale = Rectangle.Empty;
							if(legendItem.markerImage.Length > 0)
							{
								// Get image size
                                System.Drawing.Image image = this.Common.ImageLoader.LoadImage(legendItem.markerImage);

                                SizeF imageSize = new SizeF();

                                ImageLoader.GetAdjustedImageSize(image, chartGraph.Graphics, ref imageSize);

                                imageScale.Width = (int)imageSize.Width;
                                imageScale.Height = (int)imageSize.Height;
                                
								// Make sure image size fits into the drawing rectangle
								float	scaleValue = 1f;
								if(imageScale.Height > seriesMarkerPosition.Height)
								{
									scaleValue = (float)imageScale.Height / (float)seriesMarkerPosition.Height;
								}
								if(imageScale.Width > seriesMarkerPosition.Width)
								{
									scaleValue = Math.Max(scaleValue, (float)imageScale.Width / (float)seriesMarkerPosition.Width);
								}

								// Scale image size
								imageScale.Height = (int)(imageScale.Height / scaleValue);
								imageScale.Width = (int)(imageScale.Width / scaleValue);
							}

							// Adjust marker position so that it always drawn on pixel
							// boundary.
							PointF pointF = new PointF(point.X, point.Y);
							if( (markerSize%2) != 0.0 )
							{
								pointF.X -= 0.5f;
								pointF.Y -= 0.5f;
							}

							// Draw marker if it's not image
							chartGraph.DrawMarkerRel(
								chartGraph.GetRelativePoint(pointF),
								markerStyle,
								markerSize,
								(legendItem.markerColor == Color.Empty) ? legendItem.Color : legendItem.markerColor,
								(legendItem.markerBorderColor == Color.Empty) ? legendItem.borderColor : legendItem.markerBorderColor,
								markerBorderWidth,
								legendItem.markerImage, 
								legendItem.markerImageTransparentColor,
								(legendItem.shadowOffset > maxShadowOffset) ? maxShadowOffset : legendItem.shadowOffset,
								legendItem.shadowColor,
								imageScale);
						}
					}
				}
				
				// End Svg Selection mode
				chartGraph.EndHotRegion( );
			}

			#endregion // Cell Painting Methods

            #region IDisposable Members

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources
            /// </summary>
            /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_fontCache != null)
                    {
                        _fontCache.Dispose();
                        _fontCache = null;
                    }
                }
                base.Dispose(disposing);
            }


            #endregion
        }

		/// <summary>
        /// The Margins class represents the margins for various chart elements. 
		/// </summary>
		[
		SRDescription("DescriptionAttributeMargins_Margins"),
		TypeConverter(typeof(MarginExpandableObjectConverter)),
		]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
        [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
        public class Margins
		{
			#region Fields
			
			// Top margin
			private		int _top = 0;

			// Bottom margin
			private		int _bottom = 0;

			// Left margin
			private		int _left = 0;

			// Right margin
			private		int _right = 0;

#if Microsoft_CONTROL

			// Reference to common chart elements which allows to invalidate
			// chart when one of the properties is changed.
			internal	CommonElements	Common = null;

#endif // Microsoft_CONTROL

			#endregion // Fields

			#region Constructor

			/// <summary>
            /// Margins constructor.
			/// </summary>
            public Margins()
			{
			}

			/// <summary>
            /// Margins constructor.
			/// </summary>
			/// <param name="top">Top margin.</param>
			/// <param name="bottom">Bottom margin.</param>
			/// <param name="left">Left margin.</param>
			/// <param name="right">Right margin.</param>
            public Margins(int top, int bottom, int left, int right)
			{
				this._top = top;
				this._bottom = bottom;
				this._left = left;
				this._right = right;
			}

			#endregion // Constructor

			#region Properties

			/// <summary>
			/// Gets or sets the top margin.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			DefaultValue(0),
			SRDescription("DescriptionAttributeMargins_Top"),
			RefreshPropertiesAttribute(RefreshProperties.All),
			NotifyParentPropertyAttribute(true),
			]
			public int Top
			{
                get
				{
					return this._top;
				}
                set
				{
					if(value < 0)
					{
                        throw (new ArgumentException(SR.ExceptionMarginTopIsNegative, "value"));
					}
					this._top = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the bottom margin.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			DefaultValue(0),
			SRDescription("DescriptionAttributeMargins_Bottom"),
			RefreshPropertiesAttribute(RefreshProperties.All),
			NotifyParentPropertyAttribute(true),
			]
			public int Bottom
			{
                get
				{
					return this._bottom;
				}
                set
				{
					if(value < 0)
					{
                        throw (new ArgumentException(SR.ExceptionMarginBottomIsNegative, "value"));
					}
					this._bottom = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the left margin.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			DefaultValue(0),
			RefreshPropertiesAttribute(RefreshProperties.All),
			SRDescription("DescriptionAttributeMargins_Left"),
			NotifyParentPropertyAttribute(true),
			]
			public int Left
			{
                get
				{
					return this._left;
				}
                set
				{
					if(value < 0)
					{
                        throw (new ArgumentException(SR.ExceptionMarginLeftIsNegative, "value"));
					}
					this._left = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the right margin.
			/// </summary>
			[
			SRCategory("CategoryAttributeMisc"),
			DefaultValue(0),
			SRDescription("DescriptionAttributeMargins_Right"),
			RefreshPropertiesAttribute(RefreshProperties.All),
			NotifyParentPropertyAttribute(true),
			]
			public int Right
			{
                get
				{
					return this._right;
				}
                set
				{
					if(value < 0)
					{
                        throw (new ArgumentException(SR.ExceptionMarginRightIsNegative, "value"));
					}
					this._right = value;
					this.Invalidate();
				}
			}

			#endregion // Properties

			#region Methods

			/// <summary>
			/// Convert margins object to string.
			/// </summary>
			/// <returns>A string that represents the margins object.</returns>
            [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
            public override string ToString()
			{
				return string.Format(
                    CultureInfo.InvariantCulture,
					"{0:D}, {1:D}, {2:D}, {3:D}", 
					this.Top, 
					this.Bottom, 
					this.Left, 
					this.Right);
			}

			/// <summary>
			/// Determines whether the specified Object is equal to the current Object.
			/// </summary>
			/// <param name="obj">
			/// The Object to compare with the current Object.
			/// </param>
			/// <returns>
			/// True if the specified Object is equal to the current Object; otherwise, false.
			/// </returns>
            [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
            public override bool Equals(object obj)
			{
				Margins margins = obj as Margins;
				if(margins != null)
				{
					if(this.Top == margins.Top &&
						this.Bottom == margins.Bottom &&
						this.Left == margins.Left &&
						this.Right == margins.Right)
					{
						return true;
					}
				}
				return false;
			}

			/// <summary>
			/// Gets object hash code.
			/// </summary>
			/// <returns>Margins object hash value.</returns>
            [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
            public override int GetHashCode()
			{
				return this.Top.GetHashCode() + this.Bottom.GetHashCode() + this.Left.GetHashCode() + this.Right.GetHashCode();
			}

			/// <summary>
			/// Checks if there is no margin.
			/// </summary>
			/// <returns>
			/// <b>True</b> if all margins values are zeros.
			/// </returns>
            public bool IsEmpty()
			{
				return (this.Top == 0 && this.Bottom == 0 && this.Left == 0 && this.Right ==0);
			}

			/// <summary>
			/// Converts Margins class to RectangleF class.
			/// </summary>
			/// <returns>A RectangleF class that contains the values of the margins.</returns>
            public RectangleF ToRectangleF()
			{
				return new RectangleF(this.Left, this.Top, this.Right, this.Bottom);
			}

			/// <summary>
			/// Invalidates chart.
			/// </summary>
			private void Invalidate()
			{
#if Microsoft_CONTROL
				if(this.Common != null && this.Common.Chart != null)
				{
					this.Common.Chart.Invalidate();
				}
#endif // Microsoft_CONTROL
			}

			#endregion // Methods
		}

		/// <summary>
		/// <b>LegendCellCollection</b> is a strongly typed collection of LegendCell objects.
		/// </summary>
		[
		SRDescription("DescriptionAttributeLegendCellCollection_LegendCellCollection"),
		]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
        [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
        public class LegendCellCollection : ChartNamedElementCollection<LegendCell>
		{

			#region Constructors

			/// <summary>
            /// LegendCellCollection constructor.
			/// </summary>
			/// <remarks>
			/// This constructor is for internal use and should not be part of documentation.
			/// </remarks>
            /// <param name="parent">Legend item this collection belongs to.</param>
			internal LegendCellCollection(LegendItem parent) : base (parent)
			{
			}

			#endregion 

            #region Methods
            /// <summary>
			/// Adds a cell to the end of the collection.
			/// </summary>
			/// <param name="cellType">
			/// A <see cref="LegendCellType"/> value representing the cell type.
			/// </param>
			/// <param name="text">
			/// A <b>string</b> value representing cell text or image name depending 
			/// on the <b>cellType</b> parameter.
			/// </param>
			/// <param name="alignment">
			/// A <see cref="ContentAlignment"/> value representing cell content alignment.
			/// </param>
			/// <returns>
			/// Index of the newly added object.
			/// </returns>
			public int Add(LegendCellType cellType, string text, ContentAlignment alignment)
			{                
				Add(new LegendCell(cellType, text, alignment));
                return Count - 1;
			}

			/// <summary>
			/// Inserts a cell into the collection.
			/// </summary>
			/// <param name="index">
			/// Index to insert the object at.
			/// </param>
			/// <param name="cellType">
			/// A <see cref="LegendCellType"/> value representing the cell type.
			/// </param>
			/// <param name="text">
			/// A <b>string</b> value representing cell text or image name depending 
			/// on the <b>cellType</b> parameter.
			/// </param>
			/// <param name="alignment">
			/// A <see cref="ContentAlignment"/> value representing cell content alignment.
			/// </param>
			public void Insert(int index, LegendCellType cellType, string text, ContentAlignment alignment)
			{
				this.Insert(index, new LegendCell(cellType, text, alignment));
            }

            #endregion

        }

		/// <summary>
		/// The <b>LegendCellColumnCollection</b> class is a strongly typed collection
        /// of LegendCellColumn objects.
		/// </summary>
		[
		SRDescription("DescriptionAttributeLegendCellColumnCollection_LegendCellColumnCollection"),
		]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
        [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
        public class LegendCellColumnCollection : ChartNamedElementCollection<LegendCellColumn>
		{
    		
			#region Construction and Initialization

			/// <summary>
            /// LegendCellColumnCollection constructor.
			/// </summary>
			/// <remarks>
			/// This constructor is for internal use and should not be part of documentation.
			/// </remarks>
			/// <param name="legend">
			/// Chart legend which this collection belongs to.
			/// </param>
            internal LegendCellColumnCollection(Legend legend)
                : base(legend)
            {
            }

			#endregion // Construction and Initialization

            #region IDisposable Members

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources
            /// </summary>
            /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    //Free managed resources
                    foreach (LegendCellColumn item in this)
                    {
                        item.Dispose();
                    }
                    this.ClearItems();
                }
                base.Dispose(disposing);
            }


            #endregion
		}
    
}

