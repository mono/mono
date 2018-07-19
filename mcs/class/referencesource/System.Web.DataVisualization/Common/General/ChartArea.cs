//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartArea.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ChartArea
//
//  Purpose:	The ChartArea class represents one chart area within 
//              a chart image, and is used to plot one or more chart 
//              series. The number of chart series that can be plotted 
//              in a chart area is unlimited.
//
//	Reviewed:	GS - August 6, 2002
//				AG - August 7, 2002
//              AG - Microsoft 16, 2007
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
	using System.Web.UI.DataVisualization.Charting.ChartTypes;
	using System.Web.UI.DataVisualization.Charting.Utilities;
	using System.Web.UI.DataVisualization.Charting.Borders3D;
#endif


#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting
#endif
{
	#region Chart area aligment enumerations

    /// <summary>
    /// An enumeration of the alignment orientations of a ChartArea
    /// </summary>
		[Flags]
		public enum AreaAlignmentOrientations
		{
			/// <summary>
            /// Chart areas are not automatically aligned.
			/// </summary>
			None = 0,

			/// <summary>
            /// Chart areas are aligned vertically.
			/// </summary>
			Vertical = 1,

			/// <summary>
            /// Chart areas are aligned horizontally.
			/// </summary>
			Horizontal = 2,

			/// <summary>
            /// Chart areas are aligned using all values (horizontally and vertically).
			/// </summary>
			All = Vertical | Horizontal
		}

        /// <summary>
        /// An enumeration of the alignment styles of a ChartArea
        /// </summary>
		[Flags]
		public enum AreaAlignmentStyles
		{
			/// <summary>
            /// Chart areas are not automatically aligned.
			/// </summary>
			None = 0,

			/// <summary>
            /// Chart areas are aligned by positions.
			/// </summary>
			Position = 1,

			/// <summary>
            /// Chart areas are aligned by inner plot positions.
			/// </summary>
			PlotPosition = 2,

            /// <summary>
            /// Chart areas are aligned by axes views.
            /// </summary>
            AxesView = 4,

#if Microsoft_CONTROL

			/// <summary>
			/// Cursor and Selection alignment.
			/// </summary>
			Cursor = 8,

			/// <summary>
			/// Complete alignment.
			/// </summary>
			All = Position | PlotPosition | Cursor | AxesView
#else // Microsoft_CONTROL

   			/// <summary>
			/// Complete alignment.
			/// </summary>
            All = Position | PlotPosition | AxesView

#endif // Microsoft_CONTROL
        }

	#endregion

	/// <summary>
	/// The ChartArea class is used to create and display a chart 
	/// area within a chart image. The chart area is a rectangular 
	/// area on a chart image.  It has 4 axes, horizontal and vertical grids. 
    /// A chart area can contain more than one different chart type.  
    /// The number of chart series that can be plotted in a chart area 
    /// is unlimited.
    /// 
    /// ChartArea class exposes all the properties and methods
    /// of its base ChartArea3D class.
	/// </summary>
		[
		DefaultProperty("Axes"),
		SRDescription("DescriptionAttributeChartArea_ChartArea"),
		]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
        [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
        public partial class ChartArea : ChartNamedElement
		{
		    #region Chart Area Fields

            /// <summary>
            /// Plot area position
            /// </summary>
            internal ElementPosition PlotAreaPosition;

			// Private data members, which store properties values
			private Axis[]					_axisArray = new Axis[4];
			private Color					_backColor = Color.Empty;
			private ChartHatchStyle			_backHatchStyle = ChartHatchStyle.None;
			private string					_backImage = "";
			private ChartImageWrapMode		_backImageWrapMode = ChartImageWrapMode.Tile;
			private Color					_backImageTransparentColor = Color.Empty;
			private ChartImageAlignmentStyle			_backImageAlignment = ChartImageAlignmentStyle.TopLeft;
			private GradientStyle			_backGradientStyle = GradientStyle.None;
			private Color					_backSecondaryColor = Color.Empty;
			private Color					_borderColor = Color.Black;
			private int						_borderWidth = 1;
			private ChartDashStyle			_borderDashStyle = ChartDashStyle.NotSet;
			private int						_shadowOffset = 0;
			private Color					_shadowColor = Color.FromArgb(128, 0, 0, 0);
			private ElementPosition			_areaPosition = null;
			private ElementPosition			_innerPlotPosition = null;
			internal int					IterationCounter = 0;

			private		bool				_isSameFontSizeForAllAxes = false;
			internal	float				axesAutoFontSize = 8f;

            private string                      _alignWithChartArea = Constants.NotSetValue;
			private		AreaAlignmentOrientations	_alignmentOrientation = AreaAlignmentOrientations.Vertical;
			private		AreaAlignmentStyles			_alignmentStyle = AreaAlignmentStyles.All;
			private		int						_circularSectorNumber = int.MinValue;
			private		int						_circularUsePolygons = int.MinValue;

			// Flag indicates that chart area is acurrently aligned
			internal	bool					alignmentInProcess = false;

			// Chart area position before adjustments
			internal	RectangleF				originalAreaPosition = RectangleF.Empty;

			// Chart area inner plot position before adjustments
			internal	RectangleF				originalInnerPlotPosition = RectangleF.Empty;

            // Chart area position before adjustments
            internal    RectangleF              lastAreaPosition = RectangleF.Empty;


			// Center of the circulat chart area
			internal	PointF					circularCenter = PointF.Empty;

			private		ArrayList				_circularAxisList = null;

#if Microsoft_CONTROL
			// Buffered plotting area image
			internal		Bitmap				areaBufferBitmap = null;

            private	Cursor                      _cursorX = new Cursor();
            private Cursor                      _cursorY = new Cursor();
#endif

            // Area SmartLabel class
			internal		SmartLabel			smartLabels = new SmartLabel();

			// Gets or sets a flag that specifies whether the chart area is visible.
			private			bool				_visible = true;

		#endregion

		    #region Chart Area Cursor properties

#if Microsoft_CONTROL

			/// <summary>
			/// Gets or sets a Cursor object that is used for cursors and selected ranges along the X-axis.
			/// </summary>
			[
			SRCategory("CategoryAttributeCursor"),
			Bindable(true),
			DefaultValue(null),
			SRDescription("DescriptionAttributeChartArea_CursorX"),
			DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
			TypeConverter(typeof(NoNameExpandableObjectConverter)),
			]
			public Cursor CursorX
			{
				get
				{
					return _cursorX;
				}
				set
				{
					_cursorX = value;

					// Initialize chart object
					_cursorX.Initialize(this, AxisName.X);
				}
			}

			/// <summary>
			/// Gets or sets a Cursor object that is used for cursors and selected ranges along the Y-axis.
			/// </summary>
			[
			SRCategory("CategoryAttributeCursor"),
			Bindable(true),
			DefaultValue(null),
			SRDescription("DescriptionAttributeChartArea_CursorY"),
			DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
			TypeConverter(typeof(NoNameExpandableObjectConverter)),
			]
			public Cursor CursorY
			{
				get
				{
					return _cursorY;
				}
				set
				{
					_cursorY = value;

					// Initialize chart object
					_cursorY.Initialize(this, AxisName.Y);
				}
			}

#endif // Microsoft_CONTROL

            #endregion

            #region Chart Area properties

			/// <summary>
			/// Gets or sets a flag that specifies whether the chart area is visible.
			/// </summary>
			/// <remarks>
			/// When this flag is set to false, all series, legends, titles and annotation objects 
			/// associated with the chart area will also be hidden.
			/// </remarks>
			/// <value>
			/// <b>True</b> if the chart area is visible; <b>false</b> otherwise.
			/// </value>
			[
			SRCategory("CategoryAttributeAppearance"),
			DefaultValue(true),
			SRDescription("DescriptionAttributeChartArea_Visible"),
			ParenthesizePropertyNameAttribute(true),
			]
			virtual public bool Visible
			{
				get
				{
					return _visible;
				}
				set
				{
					_visible = value;
					this.Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the name of the ChartArea object to which this chart area should be aligned.
			/// </summary>
			[
			SRCategory("CategoryAttributeAlignment"),
			Bindable(true),
            DefaultValue(Constants.NotSetValue),
			SRDescription("DescriptionAttributeChartArea_AlignWithChartArea"),
			TypeConverter(typeof(LegendAreaNameConverter)),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public string AlignWithChartArea
			{
				get
				{
					return _alignWithChartArea;
				}
				set
				{
                    if (value != _alignWithChartArea)
                    {
                        if (String.IsNullOrEmpty(value))
                        {
                            _alignWithChartArea = Constants.NotSetValue;
                        }
                        else
                        {
                            if (Chart != null && Chart.ChartAreas != null)
                            {
                                Chart.ChartAreas.VerifyNameReference(value);
                            }
                            _alignWithChartArea = value;
                        }
                        Invalidate();
                    }
				}
			}

			/// <summary>
            /// Gets or sets the alignment orientation of a chart area.
			/// </summary>
			[
            SRCategory("CategoryAttributeAlignment"),
			Bindable(true),
			DefaultValue(AreaAlignmentOrientations.Vertical),
			SRDescription("DescriptionAttributeChartArea_AlignOrientation"),
            Editor(Editors.FlagsEnumUITypeEditor.Editor, Editors.FlagsEnumUITypeEditor.Base),
		    #if !Microsoft_CONTROL
		    PersistenceMode(PersistenceMode.Attribute)
		    #endif
			]
			public AreaAlignmentOrientations	AlignmentOrientation
			{
				get
				{
					return _alignmentOrientation;
				}
				set
				{
					_alignmentOrientation = value;
					Invalidate();
				}
			}


			/// <summary>
			/// Gets or sets the alignment style of the ChartArea.
			/// </summary>
			[
            SRCategory("CategoryAttributeAlignment"),
			Bindable(true),
			DefaultValue(AreaAlignmentStyles.All),
			SRDescription("DescriptionAttributeChartArea_AlignType"),
            Editor(Editors.FlagsEnumUITypeEditor.Editor, Editors.FlagsEnumUITypeEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public AreaAlignmentStyles AlignmentStyle
			{
				get
				{
					return _alignmentStyle;
				}
				set
				{
					_alignmentStyle = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets an array that represents all axes for a chart area.
			/// </summary>
			[
			SRCategory("CategoryAttributeAxes"),
			Bindable(true),
			SRDescription("DescriptionAttributeChartArea_Axes"),
			TypeConverter(typeof(AxesArrayConverter)),
            Editor(Editors.AxesArrayEditor.Editor, Editors.AxesArrayEditor.Base),
			DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
			SerializationVisibilityAttribute(SerializationVisibility.Hidden)
			]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
			public Axis[] Axes
			{
				get
				{
					return _axisArray;
				}
				set
				{
					AxisX = value[0];
					AxisY = value[1];
					AxisX2 = value[2];
					AxisY2 = value[3];
					Invalidate();
				}
			}

			/// <summary>
			/// Avoid serialization of the axes array
			/// </summary>
            [EditorBrowsableAttribute(EditorBrowsableState.Never)]
			internal bool ShouldSerializeAxes()
			{
				return false;
			}

			/// <summary>
            /// Gets or sets an Axis object that represents the primary Y-axis. 
			/// </summary>
			[
			SRCategory("CategoryAttributeAxis"),
			Bindable(true),
			Browsable(false),
			SRDescription("DescriptionAttributeChartArea_AxisY"),
#if Microsoft_CONTROL
			DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
			TypeConverter(typeof(NoNameExpandableObjectConverter))
			]
			public Axis AxisY
			{
				get
				{
					return axisY;
				}
				set
				{
					axisY = value;
					axisY.Initialize(this, AxisName.Y);
					_axisArray[1] = axisY;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets an Axis object that represents the primary X-axis. 
			/// </summary>
			[
			SRCategory("CategoryAttributeAxis"),
			Bindable(true),
			Browsable(false),
			SRDescription("DescriptionAttributeChartArea_AxisX"),
#if Microsoft_CONTROL
			DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
			TypeConverter(typeof(NoNameExpandableObjectConverter))
			]
			public Axis AxisX
			{
				get
				{
					return axisX;
				}
				set
				{
					axisX = value;
					axisX.Initialize(this, AxisName.X);
					_axisArray[0] = axisX;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets an Axis object that represents the secondary X-axis. 
			/// </summary>
			[
			SRCategory("CategoryAttributeAxis"),
			Bindable(true),
			Browsable(false),
			SRDescription("DescriptionAttributeChartArea_AxisX2"),
#if Microsoft_CONTROL
			DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
			TypeConverter(typeof(NoNameExpandableObjectConverter))
			]
			public Axis AxisX2
			{
				get
				{
					return axisX2;
				}
				set
				{
					axisX2 = value;
					axisX2.Initialize(this, AxisName.X2);
					_axisArray[2] = axisX2;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets an Axis object that represents the secondary Y-axis.
			/// </summary>
			[
			SRCategory("CategoryAttributeAxis"),
			Bindable(true),
			Browsable(false),
			SRDescription("DescriptionAttributeChartArea_AxisY2"),
#if Microsoft_CONTROL
			DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
			TypeConverter(typeof(NoNameExpandableObjectConverter))
			]
			public Axis AxisY2
			{
				get
				{
					return axisY2;
				}
				set
				{
					axisY2 = value;
					axisY2.Initialize(this, AxisName.Y2);
					_axisArray[3] = axisY2;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets an ElementPosition object, which defines the position of a chart area object within the chart image.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			SRDescription("DescriptionAttributeChartArea_Position"),
#if Microsoft_CONTROL
			DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
		    PersistenceMode(PersistenceMode.InnerProperty),
#endif
			NotifyParentPropertyAttribute(true),
			TypeConverter(typeof(ElementPositionConverter)),
			SerializationVisibilityAttribute(SerializationVisibility.Element)
			]
			public ElementPosition Position
			{
				get
				{	
					// Serialize only position values if Auto set to false
					if(this.Chart != null && this.Chart.serializationStatus == SerializationStatus.Saving )
					{
						if(_areaPosition.Auto)
						{
							return new ElementPosition();	
						}
						else
						{
							ElementPosition newPosition = new ElementPosition();
#if Microsoft_CONTROL
							newPosition.Auto = false;
#else
						newPosition.Auto = true;
#endif
							newPosition.SetPositionNoAuto(_areaPosition.X, _areaPosition.Y, _areaPosition.Width, _areaPosition.Height);
							return newPosition;
						}
					}
					return _areaPosition;
				}
				set
				{
					_areaPosition = value;
					_areaPosition.Parent = this;
					_areaPosition.resetAreaAutoPosition = true;
					Invalidate();
				}
			}

            /// <summary>
            /// Determoines if this position should be serialized.
            /// </summary>
            /// <returns></returns>
            internal bool ShouldSerializePosition()
            {
                return !this.Position.Auto;
            }

			/// <summary>
            /// Gets or sets an ElementPosition object, which defines the inner plot position of a chart area object.  
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			SRDescription("DescriptionAttributeChartArea_InnerPlotPosition"),
#if Microsoft_CONTROL
			DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
		    PersistenceMode(PersistenceMode.InnerProperty),
#endif
			NotifyParentPropertyAttribute(true),
			TypeConverter(typeof(ElementPositionConverter)),
			SerializationVisibilityAttribute(SerializationVisibility.Element)
			]
			public ElementPosition InnerPlotPosition
			{
				get
				{	
					// Serialize only position values if Auto set to false
                    if (this.Common != null && this.Common.Chart != null && this.Common.Chart.serializationStatus == SerializationStatus.Saving)
					{
						if(_innerPlotPosition.Auto)
						{
							return new ElementPosition();	
						}
						else
						{
							ElementPosition newPosition = new ElementPosition();
#if Microsoft_CONTROL
							newPosition.Auto = false;
#else
						newPosition.Auto = true;
#endif
							newPosition.SetPositionNoAuto(_innerPlotPosition.X, _innerPlotPosition.Y, _innerPlotPosition.Width, _innerPlotPosition.Height);
							return newPosition;
						}
					}
					return _innerPlotPosition;
				}
				set
				{
					_innerPlotPosition = value;
					_innerPlotPosition.Parent = this;
					Invalidate();
				}
			}

            /// <summary>
            /// Determoines if this position should be serialized.
            /// </summary>
            /// <returns></returns>
            internal bool ShouldSerializeInnerPlotPosition()
            {
                return !this.InnerPlotPosition.Auto;
            }

			/// <summary>
            /// Gets or sets the background color of a ChartArea object. 
			/// </summary>
			[

			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(typeof(Color), ""),
            SRDescription("DescriptionAttributeBackColor"),
			NotifyParentPropertyAttribute(true),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public Color BackColor
			{
				get
				{
					return _backColor;
				}
				set
				{
					_backColor = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the hatching style of a ChartArea object.
			/// </summary>
			[


			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(ChartHatchStyle.None),
			NotifyParentPropertyAttribute(true),
            SRDescription("DescriptionAttributeBackHatchStyle"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
            Editor(Editors.HatchStyleEditor.Editor, Editors.HatchStyleEditor.Base)

			]
			public ChartHatchStyle BackHatchStyle
			{
				get
				{
					return _backHatchStyle;
				}
				set
				{
					_backHatchStyle = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the background image of a ChartArea object. 
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(""),
            SRDescription("DescriptionAttributeBackImage"),
            Editor(Editors.ImageValueEditor.Editor, Editors.ImageValueEditor.Base),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
			NotifyParentPropertyAttribute(true)
			]
			public string BackImage
			{
				get
				{
					return _backImage;
				}
				set
				{
					_backImage = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the drawing mode of the background image of a ChartArea object.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(ChartImageWrapMode.Tile),
			NotifyParentPropertyAttribute(true),
            SRDescription("DescriptionAttributeImageWrapMode"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public ChartImageWrapMode BackImageWrapMode
			{
				get
				{
					return _backImageWrapMode;
				}
				set
				{
					_backImageWrapMode = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the color of a ChartArea object's background image that will be drawn as transparent.  
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(typeof(Color), ""),
			NotifyParentPropertyAttribute(true),
            SRDescription("DescriptionAttributeImageTransparentColor"),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public Color BackImageTransparentColor
			{
				get
				{
					return _backImageTransparentColor;
				}
				set
				{
					_backImageTransparentColor = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the alignment of a ChartArea object. 
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(ChartImageAlignmentStyle.TopLeft),
			NotifyParentPropertyAttribute(true),
            SRDescription("DescriptionAttributeBackImageAlign"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public ChartImageAlignmentStyle BackImageAlignment
			{
				get
				{
					return _backImageAlignment;
				}
				set
				{
					_backImageAlignment = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the orientation of a chart element's gradient, 
            /// and also determines whether or not a gradient is used.  
			/// </summary>
			[

            SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(GradientStyle.None),
			NotifyParentPropertyAttribute(true),
            SRDescription("DescriptionAttributeBackGradientStyle"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
            Editor(Editors.GradientEditor.Editor, Editors.GradientEditor.Base)
			]		
			public GradientStyle BackGradientStyle
			{
				get
				{
					return _backGradientStyle;
				}
				set
				{
					_backGradientStyle = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the secondary color of a ChartArea object.
			/// </summary>
			[

            SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(typeof(Color), ""),
			NotifyParentPropertyAttribute(true),
            SRDescription("DescriptionAttributeBackSecondaryColor"),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			] 
			public Color BackSecondaryColor
			{
				get
				{
					return _backSecondaryColor;
				}
				set
				{
					_backSecondaryColor = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the shadow color of a ChartArea object.  
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(typeof(Color), "128,0,0,0"),
            SRDescription("DescriptionAttributeShadowColor"),
			NotifyParentPropertyAttribute(true),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public Color ShadowColor
			{
				get
				{
					return _shadowColor;
				}
				set
				{
					_shadowColor = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the shadow offset (in pixels) of a ChartArea object.
			/// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(0),
            SRDescription("DescriptionAttributeShadowOffset"),
			NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public int ShadowOffset
			{
				get
				{
					return _shadowOffset;
				}
				set
				{
					_shadowOffset = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the border color of a ChartArea object.
			/// </summary>
			[

            SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(typeof(Color), "Black"),
            SRDescription("DescriptionAttributeBorderColor"),
			NotifyParentPropertyAttribute(true),
            TypeConverter(typeof(ColorConverter)),
            Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public Color BorderColor
			{
				get
				{
					return _borderColor;
				}
				set
				{
					_borderColor = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the border width of a ChartArea object.
			/// </summary>
			[

			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(1),
            SRDescription("DescriptionAttributeBorderWidth"),
			NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public int BorderWidth
			{
				get
				{
					return _borderWidth;
				}
				set
				{
					if(value < 0)
					{
                        throw (new ArgumentOutOfRangeException("value", SR.ExceptionBorderWidthIsNegative));
					}
					_borderWidth = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the style of the border line of a ChartArea object.
			/// </summary>
			[

            SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(ChartDashStyle.NotSet),
            SRDescription("DescriptionAttributeBorderDashStyle"),
			NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public ChartDashStyle BorderDashStyle
			{
				get
				{
					return _borderDashStyle;
				}
				set
				{
					_borderDashStyle = value;
					Invalidate();
				}
			}

			/// <summary>
            /// Gets or sets the unique name of a ChartArea object.
			/// </summary>
			[

			SRCategory("CategoryAttributeMisc"),
			Bindable(true),
			SRDescription("DescriptionAttributeChartArea_Name"),
			NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
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
            /// Gets or sets a Boolean that determines if the labels of the axes for all chart area
            /// , which have LabelsAutoFit property set to true, are of equal size.  
            /// </summary>
			[
			SRCategory("CategoryAttributeAppearance"),
			Bindable(true),
			DefaultValue(false),
			SRDescription("DescriptionAttributeChartArea_EquallySizedAxesFont"),
			NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
			]
			public bool IsSameFontSizeForAllAxes
			{
				get
				{
					return _isSameFontSizeForAllAxes;
				}
				set
				{
					_isSameFontSizeForAllAxes = value;
					Invalidate();
				}
			}


		#endregion

            #region Constructors
            /// <summary>
			/// ChartArea constructor.
			/// </summary>
			public ChartArea()
			{
				Initialize();
			}

            /// <summary>
            /// ChartArea constructor.
            /// </summary>
            /// <param name="name">The name.</param>
            public ChartArea(string name) : base(name)
            {
                Initialize();
            }
            #endregion

            #region Chart Area Methods
			/// <summary>
			/// Restores series order and X axis reversed mode for the 3D charts.
			/// </summary>
			internal void Restore3DAnglesAndReverseMode()
			{
				if(this.Area3DStyle.Enable3D && !this.chartAreaIsCurcular)
				{
					// Restore axis "IsReversed" property and old Y angle
					this.AxisX.IsReversed = oldReverseX;
					this.AxisX2.IsReversed = oldReverseX;
					this.AxisY.IsReversed = oldReverseY;
					this.AxisY2.IsReversed = oldReverseY;
					this.Area3DStyle.Rotation = oldYAngle;
				}
			}
	
			/// <summary>
			/// Sets series order and X axis reversed mode for the 3D charts.
			/// </summary>
			internal void Set3DAnglesAndReverseMode()
			{
				// Clear series reversed flag
				_reverseSeriesOrder = false;

				// If 3D charting is enabled
				if(this.Area3DStyle.Enable3D)
				{
					// Make sure primary & secondary axis has the same IsReversed settings
					// This is a limitation for the 3D chart required for labels drawing.
					this.AxisX2.IsReversed = this.AxisX.IsReversed;
					this.AxisY2.IsReversed = this.AxisY.IsReversed;

					// Remember reversed order of X & Y axis and Angles
					oldReverseX = this.AxisX.IsReversed;
					oldReverseY = this.AxisY.IsReversed;
					oldYAngle = this.Area3DStyle.Rotation;

					// Check if Y angle 
					if(this.Area3DStyle.Rotation > 90 || this.Area3DStyle.Rotation < -90)
					{
                        // This method depends on the 'switchValueAxes' field which is calculated based on the chart types
                        // of the series associated with the chart area. We need to call SetData method to make sure this field
                        // is correctly initialized. Because we only need to collect information about the series, we pass 'false'
                        // as parameters to limit the amount of work this function does.
                        this.SetData(false, false);

						// Reversed series order
						_reverseSeriesOrder = true;

						// Reversed primary and secondary X axis
						if(!this.switchValueAxes)
						{
							this.AxisX.IsReversed = !this.AxisX.IsReversed;
							this.AxisX2.IsReversed = !this.AxisX2.IsReversed;
						}

							// Reversed primary and secondary Y axis for chart types like Bar
						else
						{
							this.AxisY.IsReversed = !this.AxisY.IsReversed;
							this.AxisY2.IsReversed = !this.AxisY2.IsReversed;
						}

						// Adjust Y angle
						if(this.Area3DStyle.Rotation > 90)
						{
							this.Area3DStyle.Rotation = (this.Area3DStyle.Rotation - 90) - 90;
						}
						else if(this.Area3DStyle.Rotation < -90)
						{
							this.Area3DStyle.Rotation = (this.Area3DStyle.Rotation + 90) + 90;
						}
					}
				}
			}

			/// <summary>
			/// Save all automatic values like Minimum and Maximum.
			/// </summary>
			internal void SetTempValues()
			{
				// Save non automatic area position
				if(!this.Position.Auto)
				{
					this.originalAreaPosition = this.Position.ToRectangleF();
				}

				// Save non automatic area inner plot position
				if(!this.InnerPlotPosition.Auto)
				{
					this.originalInnerPlotPosition = this.InnerPlotPosition.ToRectangleF();
				}
			
				this._circularSectorNumber = int.MinValue;
				this._circularUsePolygons = int.MinValue;
				this._circularAxisList = null;

				// Save Minimum and maximum values for all axes
				axisX.StoreAxisValues();
				axisX2.StoreAxisValues();
				axisY.StoreAxisValues();
				axisY2.StoreAxisValues();
			}

			/// <summary>
			/// Load all automatic values like Minimum and Maximum with original values.
			/// </summary>
			internal void GetTempValues()
			{
				// Take Minimum and maximum values for all axes
				axisX.ResetAxisValues();
				axisX2.ResetAxisValues();
				axisY.ResetAxisValues();
				axisY2.ResetAxisValues();

				// Restore non automatic area position
				if(!this.originalAreaPosition.IsEmpty)
				{
                    this.lastAreaPosition = this.Position.ToRectangleF();
                    this.Position.SetPositionNoAuto(this.originalAreaPosition.X, this.originalAreaPosition.Y, this.originalAreaPosition.Width, this.originalAreaPosition.Height);
					this.originalAreaPosition = RectangleF.Empty;
				}

				// Save non automatic area inner plot position
				if(!this.originalInnerPlotPosition.IsEmpty)
				{
                    this.InnerPlotPosition.SetPositionNoAuto(this.originalInnerPlotPosition.X, this.originalInnerPlotPosition.Y, this.originalInnerPlotPosition.Width, this.originalInnerPlotPosition.Height);
					this.originalInnerPlotPosition = RectangleF.Empty;
				}
			}
		
			/// <summary>
			/// Initialize Chart area and axes
			/// </summary>
			internal void Initialize()
			{
                // Initialize 3D style class
                _area3DStyle = new ChartArea3DStyle(this);

				// Create axes for this chart area.
				axisY = new Axis( );
				axisX = new Axis( );
				axisX2 = new Axis( );
				axisY2 = new Axis( );

				// Initialize axes;
				axisX.Initialize(this, AxisName.X);
				axisY.Initialize(this, AxisName.Y);
				axisX2.Initialize(this, AxisName.X2);
				axisY2.Initialize(this, AxisName.Y2);

				// Initialize axes array
				_axisArray[0] = axisX;
				_axisArray[1] = axisY;
				_axisArray[2] = axisX2;
				_axisArray[3] = axisY2;

				// Set flag to reset auto values for all areas
                _areaPosition = new ElementPosition(this);
				_areaPosition.resetAreaAutoPosition = true;

                _innerPlotPosition = new ElementPosition(this);
			
				// Set the position of the new chart area
				if( PlotAreaPosition == null )
				{
					PlotAreaPosition = new ElementPosition(this);
				}
	
#if Microsoft_CONTROL

				// Initialize cursor class
                this._cursorX.Initialize(this, AxisName.X);
                this._cursorY.Initialize(this, AxisName.Y);

#endif // Microsoft_CONTROL
			}

			/// <summary>
			/// Minimum and maximum do not have to be calculated 
			/// from data series every time. It is very time 
			/// consuming. Minimum and maximum are buffered 
			/// and only when this flags are set Minimum and 
			/// Maximum are refreshed from data.
			/// </summary>
			internal void ResetMinMaxFromData()
			{
				_axisArray[0].refreshMinMaxFromData = true;
				_axisArray[1].refreshMinMaxFromData = true;
				_axisArray[2].refreshMinMaxFromData = true;
				_axisArray[3].refreshMinMaxFromData = true;
			}

 			/// <summary>
			/// Recalculates the axes scale of a chart area.
			/// </summary>
			public void RecalculateAxesScale()
			{
				// Read axis Max/Min from data
				ResetMinMaxFromData();

#if Microsoft_CONTROL
				Set3DAnglesAndReverseMode();
				SetTempValues();
#endif

				// Initialize area position
				_axisArray[0].ReCalc( PlotAreaPosition );
				_axisArray[1].ReCalc( PlotAreaPosition );
				_axisArray[2].ReCalc( PlotAreaPosition );
				_axisArray[3].ReCalc( PlotAreaPosition );
			
				// Find all Data and chart types which belong 
				// to this chart area an set default values
				SetData();

#if Microsoft_CONTROL
				Restore3DAnglesAndReverseMode();
				GetTempValues();
#endif
			}

			/// <summary>
			/// RecalculateAxesScale the chart area
			/// </summary>
			internal void ReCalcInternal()
			{
				// Initialize area position
				_axisArray[0].ReCalc( PlotAreaPosition );
				_axisArray[1].ReCalc( PlotAreaPosition );
				_axisArray[2].ReCalc( PlotAreaPosition );
				_axisArray[3].ReCalc( PlotAreaPosition );
			
				// Find all Data and chart types which belong 
				// to this chart area an set default values
				SetData();
			}


			/// <summary>
			/// Reset auto calculated chart area values.
			/// </summary>
			internal void ResetAutoValues()
			{
				_axisArray[0].ResetAutoValues();
				_axisArray[1].ResetAutoValues();
				_axisArray[2].ResetAutoValues();
				_axisArray[3].ResetAutoValues();
			}

			/// <summary>
			/// Calculates Position for the background.
			/// </summary>
			/// <param name="withScrollBars">Calculate with scroll bars</param>
			/// <returns>Background rectangle</returns>
			internal RectangleF GetBackgroundPosition( bool withScrollBars )
			{
				// For pie and doughnut, which do not have axes, the position 
				// for the background is Chart area position not plotting 
				// area position.
				RectangleF backgroundPosition = PlotAreaPosition.ToRectangleF();
				if( !requireAxes )
				{
					backgroundPosition = Position.ToRectangleF();
				}

                // Without scroll bars
				if( !withScrollBars )
				{
					return backgroundPosition;
				}

				// Add scroll bar rectangles to the area background 
				RectangleF backgroundPositionWithScrollBars = new RectangleF(backgroundPosition.Location, backgroundPosition.Size);

#if Microsoft_CONTROL

				if( requireAxes )
				{
					// Loop through all axis
					foreach(Axis axis in this.Axes)
					{
						// Find axis with visible scroll bars
						if(axis.ScrollBar.IsVisible && axis.ScrollBar.IsPositionedInside)
						{
							// Change size of the background rectangle depending on the axis position
							if(axis.AxisPosition == AxisPosition.Bottom)
							{
								backgroundPositionWithScrollBars.Height += (float)axis.ScrollBar.GetScrollBarRelativeSize();
							}
							else if(axis.AxisPosition == AxisPosition.Top)
							{
								backgroundPositionWithScrollBars.Y -= (float)axis.ScrollBar.GetScrollBarRelativeSize();
								backgroundPositionWithScrollBars.Height += (float)axis.ScrollBar.GetScrollBarRelativeSize();
							}
							else if(axis.AxisPosition == AxisPosition.Left)
							{
								backgroundPositionWithScrollBars.X -= (float)axis.ScrollBar.GetScrollBarRelativeSize();
								backgroundPositionWithScrollBars.Width += (float)axis.ScrollBar.GetScrollBarRelativeSize();
							}
							else if(axis.AxisPosition == AxisPosition.Left)
							{
								backgroundPositionWithScrollBars.Width += (float)axis.ScrollBar.GetScrollBarRelativeSize();
							}
						}
					}
				}

#endif // Microsoft_CONTROL
				return backgroundPositionWithScrollBars;
			}

			/// <summary>
			/// Call when the chart area is resized.
			/// </summary>
			/// <param name="chartGraph">Chart graphics object.</param>
			internal void Resize(ChartGraphics chartGraph)
			{
				// Initialize plotting area position
				RectangleF plottingRect = Position.ToRectangleF();
				if(!InnerPlotPosition.Auto)
				{
					plottingRect.X += (Position.Width / 100F) * InnerPlotPosition.X;
					plottingRect.Y += (Position.Height / 100F) * InnerPlotPosition.Y;
					plottingRect.Width = (Position.Width / 100F) * InnerPlotPosition.Width;
					plottingRect.Height = (Position.Height / 100F) * InnerPlotPosition.Height;
				}

				//******************************************************
				//** Calculate number of vertical and horizontal axis
				//******************************************************
				int	verticalAxes = 0;
				int	horizontalAxes = 0;
				foreach(Axis axis in this.Axes)
				{
					if(axis.enabled)
					{
						if(axis.AxisPosition == AxisPosition.Bottom)
						{
							++horizontalAxes;		
						}
						else if(axis.AxisPosition == AxisPosition.Top)
						{
							++horizontalAxes;
						}
						else if(axis.AxisPosition == AxisPosition.Left)
						{
							++verticalAxes;
						}
						else if(axis.AxisPosition == AxisPosition.Right)
						{
							++verticalAxes;
						}
					}
				}
				if(horizontalAxes <= 0 )
				{
					horizontalAxes = 1;
				}
				if(verticalAxes <= 0 )
				{
					verticalAxes = 1;
				}


				//******************************************************
				//** Find same auto-fit font size
				//******************************************************
				Axis[] axisArray = (this.switchValueAxes) ? 
					new Axis[] {this.AxisX, this.AxisX2, this.AxisY, this.AxisY2} :
					new Axis[] {this.AxisY, this.AxisY2, this.AxisX, this.AxisX2};
				if(this.IsSameFontSizeForAllAxes)
				{
					axesAutoFontSize = 20;
					foreach(Axis axis in axisArray)
					{
						// Process only enabled axis
						if(axis.enabled)
						{
							// Resize axis
							if(axis.AxisPosition == AxisPosition.Bottom || axis.AxisPosition == AxisPosition.Top)
							{
								axis.Resize(chartGraph, this.PlotAreaPosition, plottingRect, horizontalAxes, InnerPlotPosition.Auto);
							}
							else
							{
								axis.Resize(chartGraph, this.PlotAreaPosition, plottingRect, verticalAxes, InnerPlotPosition.Auto);
							}

							// Calculate smallest font size
							if(axis.IsLabelAutoFit && axis.autoLabelFont != null)
							{
								axesAutoFontSize = Math.Min(axesAutoFontSize, axis.autoLabelFont.Size);
							}
						}
					}
				}

				//******************************************************
				//** Adjust plotting area position according to the axes 
				//** elements (title, labels, tick marks) size.
				//******************************************************
				RectangleF	rectLabelSideSpacing = RectangleF.Empty;
				foreach(Axis axis in axisArray)
				{
					// Process only enabled axis
					if( ! axis.enabled )
					{
						//******************************************************
						//** Adjust for the 3D Wall Width for disabled axis
						//******************************************************
						if(InnerPlotPosition.Auto && this.Area3DStyle.Enable3D && !this.chartAreaIsCurcular)
						{
							SizeF areaWallSize = chartGraph.GetRelativeSize(new SizeF(this.Area3DStyle.WallWidth, this.Area3DStyle.WallWidth));
							if(axis.AxisPosition == AxisPosition.Bottom)
							{
								plottingRect.Height -= areaWallSize.Height;
							}
							else if(axis.AxisPosition == AxisPosition.Top)
							{
								plottingRect.Y += areaWallSize.Height;
								plottingRect.Height -= areaWallSize.Height;
							}
							else if(axis.AxisPosition == AxisPosition.Right)
							{
								plottingRect.Width -= areaWallSize.Width;
							}
							else if(axis.AxisPosition == AxisPosition.Left)
							{
								plottingRect.X += areaWallSize.Width;
								plottingRect.Width -= areaWallSize.Width;
							}
						}

						continue;
					}

					//******************************************************
					//** Calculate axes elements position
					//******************************************************
					if(axis.AxisPosition == AxisPosition.Bottom || axis.AxisPosition == AxisPosition.Top)
					{
						axis.Resize(chartGraph, this.PlotAreaPosition, plottingRect, horizontalAxes, InnerPlotPosition.Auto);
					}
					else
					{
						axis.Resize(chartGraph, this.PlotAreaPosition, plottingRect, verticalAxes, InnerPlotPosition.Auto);
					}

					// Shift top/bottom labels so they will not overlap with left/right labels
					PreventTopBottomAxesLabelsOverlapping(axis);

					//******************************************************
					//** Check axis position
					//******************************************************
					float axisPosition = (float)axis.GetAxisPosition();
					if(axis.AxisPosition == AxisPosition.Bottom)
					{
						if(!axis.GetIsMarksNextToAxis())
						{
							axisPosition = plottingRect.Bottom;
						}
						axisPosition = plottingRect.Bottom - axisPosition;
					}
					else if(axis.AxisPosition == AxisPosition.Top)
					{
						if(!axis.GetIsMarksNextToAxis())
						{
							axisPosition = plottingRect.Y;
						}
						axisPosition = axisPosition - plottingRect.Top;
					}
					else if(axis.AxisPosition == AxisPosition.Right)
					{
						if(!axis.GetIsMarksNextToAxis())
						{
							axisPosition = plottingRect.Right;
						}
						axisPosition = plottingRect.Right - axisPosition;
					}
					else if(axis.AxisPosition == AxisPosition.Left)
					{
						if(!axis.GetIsMarksNextToAxis())
						{
							axisPosition = plottingRect.X;
						}
						axisPosition = axisPosition - plottingRect.Left;
					}

					//******************************************************
					//** Adjust axis elements size with axis position
					//******************************************************
					// Calculate total size of axis elements
					float axisSize = axis.markSize + axis.labelSize;

#if SUBAXES
					// Add sub-axis size
					if(!this.chartAreaIsCurcular && !this.Area3DStyle.Enable3D)
					{
						foreach(SubAxis subAxis in axis.SubAxes)
						{
							axisSize += subAxis.markSize + subAxis.labelSize + subAxis.titleSize;
						}
					}
#endif // SUBAXES

                    // Adjust depending on the axis position
					axisSize -= axisPosition;
					if(axisSize < 0)
					{
						axisSize = 0;
					}


					// Add axis title and scroll bar size (always outside of plotting area)
					axisSize += axis.titleSize + axis.scrollBarSize;


					// Calculate horizontal axes size for circualar area
					if(this.chartAreaIsCurcular && 
						(axis.AxisPosition == AxisPosition.Top || axis.AxisPosition == AxisPosition.Bottom) )
					{
						axisSize = axis.titleSize + axis.markSize + axis.scrollBarSize;
					}

					//******************************************************
					//** Adjust plotting area
					//******************************************************
					if(InnerPlotPosition.Auto)
					{
						if(axis.AxisPosition == AxisPosition.Bottom)
						{
							plottingRect.Height -= axisSize;
						}
						else if(axis.AxisPosition == AxisPosition.Top)
						{
							plottingRect.Y += axisSize;
							plottingRect.Height -= axisSize;
						}
						else if(axis.AxisPosition == AxisPosition.Left)
						{
							plottingRect.X += axisSize;
							plottingRect.Width -= axisSize;
						}
						else if(axis.AxisPosition == AxisPosition.Right)
						{
							plottingRect.Width -= axisSize;
						}

                        // Check if labels side offset should be processed
                        bool addLabelsSideOffsets = true;

						// Update the plotting area depending on the size required for labels on the sides
                        if (addLabelsSideOffsets)
                        {
                            if (axis.AxisPosition == AxisPosition.Bottom || axis.AxisPosition == AxisPosition.Top)
                            {
                                if (axis.labelNearOffset != 0 && axis.labelNearOffset < Position.X)
                                {
                                    float offset = Position.X - axis.labelNearOffset;
                                    if (Math.Abs(offset) > plottingRect.Width * 0.3f)
                                    {
                                        offset = plottingRect.Width * 0.3f;
                                    }

                                    // NOTE: Code was removed to solve an issue with extra space when labels angle = 45
                                    //rectLabelSideSpacing.Width = (float)Math.Max(offset, rectLabelSideSpacing.Width);
                                    rectLabelSideSpacing.X = (float)Math.Max(offset, rectLabelSideSpacing.X);
                                }

                                if (axis.labelFarOffset > Position.Right)
                                {
                                    if ((axis.labelFarOffset - Position.Right) < plottingRect.Width * 0.3f)
                                    {
                                        rectLabelSideSpacing.Width = (float)Math.Max(axis.labelFarOffset - Position.Right, rectLabelSideSpacing.Width);
                                    }
                                    else
                                    {
                                        rectLabelSideSpacing.Width = (float)Math.Max(plottingRect.Width * 0.3f, rectLabelSideSpacing.Width);
                                    }
                                }
                            }

                            else
                            {
                                if (axis.labelNearOffset != 0 && axis.labelNearOffset < Position.Y)
                                {
                                    float offset = Position.Y - axis.labelNearOffset;
                                    if (Math.Abs(offset) > plottingRect.Height * 0.3f)
                                    {
                                        offset = plottingRect.Height * 0.3f;
                                    }

                                    // NOTE: Code was removed to solve an issue with extra space when labels angle = 45
                                    //rectLabelSideSpacing.Height = (float)Math.Max(offset, rectLabelSideSpacing.Height);
                                    rectLabelSideSpacing.Y = (float)Math.Max(offset, rectLabelSideSpacing.Y);
                                }

                                if (axis.labelFarOffset > Position.Bottom)
                                {
                                    if ((axis.labelFarOffset - Position.Bottom) < plottingRect.Height * 0.3f)
                                    {
                                        rectLabelSideSpacing.Height = (float)Math.Max(axis.labelFarOffset - Position.Bottom, rectLabelSideSpacing.Height);
                                    }
                                    else
                                    {
                                        rectLabelSideSpacing.Height = (float)Math.Max(plottingRect.Height * 0.3f, rectLabelSideSpacing.Height);
                                    }
                                }
                            }
                        }
					}
				}

				//******************************************************
				//** Make sure there is enough space 
				//** for labels on the chart sides
				//******************************************************
                if (!this.chartAreaIsCurcular)
                {
                    if (rectLabelSideSpacing.Y > 0 && rectLabelSideSpacing.Y > plottingRect.Y - Position.Y)
                    {
                        float delta = (plottingRect.Y - Position.Y) - rectLabelSideSpacing.Y;
                        plottingRect.Y -= delta;
                        plottingRect.Height += delta;
                    }
                    if (rectLabelSideSpacing.X > 0 && rectLabelSideSpacing.X > plottingRect.X - Position.X)
                    {
                        float delta = (plottingRect.X - Position.X) - rectLabelSideSpacing.X;
                        plottingRect.X -= delta;
                        plottingRect.Width += delta;
                    }
                    if (rectLabelSideSpacing.Height > 0 && rectLabelSideSpacing.Height > Position.Bottom - plottingRect.Bottom)
                    {
                        plottingRect.Height += (Position.Bottom - plottingRect.Bottom) - rectLabelSideSpacing.Height;
                    }
                    if (rectLabelSideSpacing.Width > 0 && rectLabelSideSpacing.Width > Position.Right - plottingRect.Right)
                    {
                        plottingRect.Width += (Position.Right - plottingRect.Right) - rectLabelSideSpacing.Width;
                    }
                }

				//******************************************************
				//** Plotting area must be square for the circular 
				//** chart area (in pixels).
				//******************************************************
				if(this.chartAreaIsCurcular)
				{
					// Adjust area to fit the axis title
					float	xTitleSize = (float)Math.Max(this.AxisY.titleSize, this.AxisY2.titleSize);
					if(xTitleSize > 0)
					{
						plottingRect.X += xTitleSize;
						plottingRect.Width -= 2f * xTitleSize;
					}
					float	yTitleSize = (float)Math.Max(this.AxisX.titleSize, this.AxisX2.titleSize);
					if(yTitleSize > 0)
					{
						plottingRect.Y += yTitleSize;
						plottingRect.Height -= 2f * yTitleSize;
					}

					// Make a square plotting rect
					RectangleF rect = chartGraph.GetAbsoluteRectangle( plottingRect );
					if(rect.Width > rect.Height)
					{
						rect.X += (rect.Width - rect.Height) / 2f;
						rect.Width = rect.Height;
					}
					else
					{
						rect.Y += (rect.Height - rect.Width) / 2f;
						rect.Height = rect.Width;					
					}
					plottingRect = chartGraph.GetRelativeRectangle( rect );

					// Remember circular chart area center
					this.circularCenter = new PointF(plottingRect.X + plottingRect.Width/2f, plottingRect.Y + plottingRect.Height/2f);

					// Calculate auto-fit font of the circular axis labels and update area position
					FitCircularLabels(chartGraph, this.PlotAreaPosition, ref plottingRect, xTitleSize, yTitleSize);
				}

				//******************************************************
				//** Set plotting area position
				//******************************************************
				if(plottingRect.Width < 0f)
				{
					plottingRect.Width = 0f;
				}
				if(plottingRect.Height < 0f)
				{
					plottingRect.Height = 0f;
				}
				PlotAreaPosition.FromRectangleF(plottingRect);
				InnerPlotPosition.SetPositionNoAuto(
					(float)Math.Round((plottingRect.X - Position.X) / (Position.Width / 100F), 5), 
					(float)Math.Round((plottingRect.Y - Position.Y) / (Position.Height / 100F), 5), 
					(float)Math.Round(plottingRect.Width / (Position.Width / 100F), 5), 
					(float)Math.Round(plottingRect.Height / (Position.Height / 100F), 5));


				//******************************************************
				//** Adjust label font size for axis, which were 
				//** automatically calculated after the opposite axis 
				//** change the size of plotting area.
				//******************************************************
				this.AxisY2.AdjustLabelFontAtSecondPass(chartGraph, InnerPlotPosition.Auto);
				this.AxisY.AdjustLabelFontAtSecondPass(chartGraph, InnerPlotPosition.Auto);
				if(InnerPlotPosition.Auto)
				{
					this.AxisX2.AdjustLabelFontAtSecondPass(chartGraph, InnerPlotPosition.Auto);
					this.AxisX.AdjustLabelFontAtSecondPass(chartGraph, InnerPlotPosition.Auto);
				}

			}

            /// <summary>
            /// Finds axis by it's position. Can be Null.
            /// </summary>
            /// <param name="axisPosition">Axis position to find</param>
            /// <returns>Found axis.</returns>
			private Axis FindAxis(AxisPosition axisPosition)
			{
				foreach(Axis axis in this.Axes)
				{
					if(axis.AxisPosition == axisPosition)
					{
						return axis;
					}
				}
				return null;
			}

			/// <summary>
			/// Shift top/bottom labels so they will not overlap with left/right labels.
			/// </summary>
			/// <param name="axis">Axis to shift up/down.</param>
			private void PreventTopBottomAxesLabelsOverlapping(Axis axis)
			{
				// If axis is not on the edge of the chart area do not
				// try to adjust it's position when axis labels overlap
				// labels of the oppositie axis.
				if( !axis.IsAxisOnAreaEdge )
				{
					return;
				}

				// Shift bottom axis labels down
				if(axis.AxisPosition == AxisPosition.Bottom)
				{
					// Get labels position
					float labelsPosition = (float)axis.GetAxisPosition();
					if( !axis.GetIsMarksNextToAxis() )
					{
						labelsPosition = axis.PlotAreaPosition.Bottom;
					}

					// Only adjust labels outside plotting area
					if(Math.Round(labelsPosition, 2) < Math.Round(axis.PlotAreaPosition.Bottom, 2))
					{
						return;
					}

					// Check if labels may overlap with Left axis
					Axis	leftAxis = FindAxis(AxisPosition.Left);
					if(leftAxis != null && 
                        leftAxis.enabled &&
						leftAxis.labelFarOffset != 0 &&
						leftAxis.labelFarOffset > labelsPosition &&
						axis.labelNearOffset != 0 && 
						axis.labelNearOffset < PlotAreaPosition.X)
					{
						float overlap = (float)(leftAxis.labelFarOffset - labelsPosition) * 0.75f;
						if(overlap > axis.markSize)
						{
							axis.markSize += overlap - axis.markSize;
						}
					}

					// Check if labels may overlap with Right axis
					Axis	rightAxis = FindAxis(AxisPosition.Right);
					if(rightAxis != null &&
                        rightAxis.enabled &&
						rightAxis.labelFarOffset != 0 &&
						rightAxis.labelFarOffset > labelsPosition &&
						axis.labelFarOffset != 0 && 
						axis.labelFarOffset > PlotAreaPosition.Right)
					{
						float overlap = (float)(rightAxis.labelFarOffset - labelsPosition) * 0.75f;
						if(overlap > axis.markSize)
						{
							axis.markSize += overlap - axis.markSize;
						}
					}
				}

					// Shift top axis labels up
				else if(axis.AxisPosition == AxisPosition.Top)
				{
					// Get labels position
					float labelsPosition = (float)axis.GetAxisPosition();
					if( !axis.GetIsMarksNextToAxis() )
					{
						labelsPosition = axis.PlotAreaPosition.Y;
					}

					// Only adjust labels outside plotting area
					if(Math.Round(labelsPosition, 2) < Math.Round(axis.PlotAreaPosition.Y, 2))
					{
						return;
					}

					// Check if labels may overlap with Left axis
					Axis	leftAxis = FindAxis(AxisPosition.Left);
					if(leftAxis != null &&
                        leftAxis.enabled &&
						leftAxis.labelNearOffset != 0 &&
						leftAxis.labelNearOffset < labelsPosition &&
						axis.labelNearOffset != 0 && 
						axis.labelNearOffset < PlotAreaPosition.X)
					{
						float overlap = (float)(labelsPosition - leftAxis.labelNearOffset) * 0.75f;
						if(overlap > axis.markSize)
						{
							axis.markSize += overlap - axis.markSize;
						}
					}

					// Check if labels may overlap with Right axis
					Axis	rightAxis = FindAxis(AxisPosition.Right);
					if(rightAxis != null &&
                        rightAxis.enabled &&
						rightAxis.labelNearOffset != 0 &&
						rightAxis.labelNearOffset < labelsPosition &&
						axis.labelFarOffset != 0 && 
						axis.labelFarOffset > PlotAreaPosition.Right)
					{
						float overlap = (float)(labelsPosition - rightAxis.labelNearOffset) * 0.75f;
						if(overlap > axis.markSize)
						{
							axis.markSize += overlap - axis.markSize;
						}
					}
				}

			}

		    #endregion

		    #region Painting and Selection Methods

			/// <summary>
			/// Draws chart area background and/or border.
			/// </summary>
			/// <param name="graph">Chart graphics.</param>
			/// <param name="position">Background position.</param>
			/// <param name="borderOnly">Draws chart area border only.</param>
			private void PaintAreaBack(ChartGraphics graph, RectangleF position, bool borderOnly)
			{
				if(!borderOnly)
				{
					// Draw background
					if(!this.Area3DStyle.Enable3D || !requireAxes || chartAreaIsCurcular)
					{
						// 3D Pie Chart doesn't need scene
						// Draw 2D background
						graph.FillRectangleRel( 
							position, 
							BackColor, 
							BackHatchStyle, 
							BackImage, 
							BackImageWrapMode, 				
							BackImageTransparentColor,
							BackImageAlignment,
							BackGradientStyle, 
							BackSecondaryColor, 
							(requireAxes) ? Color.Empty : BorderColor, 
							(requireAxes) ? 0 : BorderWidth, 
							BorderDashStyle, 
							ShadowColor, 
							ShadowOffset, 
							PenAlignment.Outset,
							chartAreaIsCurcular,
							(chartAreaIsCurcular && this.CircularUsePolygons) ? this.CircularSectorsNumber : 0,
							this.Area3DStyle.Enable3D);
					}
					else
					{
						// Draw chart area 3D scene
						this.DrawArea3DScene(graph, position);
					}
				}
				else
				{
					if(!this.Area3DStyle.Enable3D || !requireAxes || chartAreaIsCurcular)
					{
						// Draw chart area border
						if(BorderColor != Color.Empty && BorderWidth > 0)
						{
							graph.FillRectangleRel( position, 
								Color.Transparent, 
								ChartHatchStyle.None, 
								"", 
								ChartImageWrapMode.Tile, 				
								Color.Empty,
								ChartImageAlignmentStyle.Center,
								GradientStyle.None, 
								Color.Empty, 
								BorderColor, 
								BorderWidth, 
								BorderDashStyle, 
								Color.Empty, 
								0, 
								PenAlignment.Outset,
								chartAreaIsCurcular,
								(chartAreaIsCurcular && this.CircularUsePolygons) ? this.CircularSectorsNumber : 0,
								this.Area3DStyle.Enable3D);
						}
					}

				}
			}

			/// <summary>
			/// Paint the chart area.
			/// </summary>
			/// <param name="graph">Chart graphics.</param>
			internal void Paint( ChartGraphics graph )
			{
                    // Check if plot area position was recalculated.
                    // If not and non-auto InnerPlotPosition & Position were
                    // specified - do all needed calculations
                    if (PlotAreaPosition.Width == 0 &&
                        PlotAreaPosition.Height == 0 &&
                        !InnerPlotPosition.Auto
                        && !Position.Auto)
                    {
                        // Initialize plotting area position
                        RectangleF plottingRect = Position.ToRectangleF();
                        if (!InnerPlotPosition.Auto)
                        {
                            plottingRect.X += (Position.Width / 100F) * InnerPlotPosition.X;
                            plottingRect.Y += (Position.Height / 100F) * InnerPlotPosition.Y;
                            plottingRect.Width = (Position.Width / 100F) * InnerPlotPosition.Width;
                            plottingRect.Height = (Position.Height / 100F) * InnerPlotPosition.Height;
                        }

                        PlotAreaPosition.FromRectangleF(plottingRect);
                    }

                    // Get background position rectangle.
                    RectangleF backgroundPositionWithScrollBars = GetBackgroundPosition(true);
                    RectangleF backgroundPosition = GetBackgroundPosition(false);

                    // Add hot region for plotting area.
                    if (Common.ProcessModeRegions)
                    {
                        Common.HotRegionsList.AddHotRegion(backgroundPosition, this, ChartElementType.PlottingArea, true);
                    }
                    // Draw background
                    PaintAreaBack(graph, backgroundPositionWithScrollBars, false);

                    // Call BackPaint event
                    Common.Chart.CallOnPrePaint(new ChartPaintEventArgs(this, graph, Common, PlotAreaPosition));

                    // Draw chart types without axes - Pie.
                    if (!requireAxes && ChartTypes.Count != 0)
                    {
                        // Find first chart type that do not require axis (like Pie) and draw it.
                        // Chart types that do not require axes (circular charts) cannot be combined with
                        // any other chart types.
                        // NOTE: Fixes issues #4672 and #4692
                        for (int chartTypeIndex = 0; chartTypeIndex < ChartTypes.Count; chartTypeIndex++)
                        {
                            IChartType chartType = Common.ChartTypeRegistry.GetChartType((string)ChartTypes[chartTypeIndex]);
                            if (!chartType.RequireAxes)
                            {
                                chartType.Paint(graph, Common, this, null);
                                break;
                            }
                        }

                        // Call Paint event
                        Common.Chart.CallOnPostPaint(new ChartPaintEventArgs(this, graph, Common, PlotAreaPosition));
                        return;
                    }



                    // Reset Smart Labels 
                    this.smartLabels.Reset();



                    // Set values for optimized drawing
                    foreach (Axis currentAxis in this._axisArray)
                    {
                        currentAxis.optimizedGetPosition = true;
                        currentAxis.paintViewMax = currentAxis.ViewMaximum;
                        currentAxis.paintViewMin = currentAxis.ViewMinimum;
                        currentAxis.paintRange = currentAxis.paintViewMax - currentAxis.paintViewMin;
                        currentAxis.paintAreaPosition = PlotAreaPosition.ToRectangleF();
                        if (currentAxis.ChartArea != null && currentAxis.ChartArea.chartAreaIsCurcular)
                        {
                            // Update position for circular chart area
                            currentAxis.paintAreaPosition.Width /= 2.0f;
                            currentAxis.paintAreaPosition.Height /= 2.0f;
                        }
                        currentAxis.paintAreaPositionBottom = currentAxis.paintAreaPosition.Y + currentAxis.paintAreaPosition.Height;
                        currentAxis.paintAreaPositionRight = currentAxis.paintAreaPosition.X + currentAxis.paintAreaPosition.Width;
                        if (currentAxis.AxisPosition == AxisPosition.Top || currentAxis.AxisPosition == AxisPosition.Bottom)
                            currentAxis.paintChartAreaSize = currentAxis.paintAreaPosition.Width;
                        else
                            currentAxis.paintChartAreaSize = currentAxis.paintAreaPosition.Height;

                        currentAxis.valueMultiplier = 0.0;
                        if (currentAxis.paintRange != 0)
                        {
                            currentAxis.valueMultiplier = currentAxis.paintChartAreaSize / currentAxis.paintRange;
                        }
                    }
                    
                    // Draw Axis Striplines (only when StripWidth > 0)
                    bool useScaleSegments = false;
                    Axis[] axesArray = new Axis[] { axisY, axisY2, axisX, axisX2 };
                    foreach (Axis currentAxis in axesArray)
                    {

                        useScaleSegments = (currentAxis.ScaleSegments.Count > 0);

                        if (!useScaleSegments)
                        {
                            currentAxis.PaintStrips(graph, false);
                        }

                        else
                        {
                            foreach (AxisScaleSegment scaleSegment in currentAxis.ScaleSegments)
                            {
                                scaleSegment.SetTempAxisScaleAndInterval();

                                currentAxis.PaintStrips(graph, false);

                                scaleSegment.RestoreAxisScaleAndInterval();
                            }
                        }
					}

                    // Draw Axis Grids
                    axesArray = new Axis[] { axisY, axisX2, axisY2, axisX };
                    foreach (Axis currentAxis in axesArray)
                    {

                        useScaleSegments = (currentAxis.ScaleSegments.Count > 0);

                        if (!useScaleSegments)
                        {
                            currentAxis.PaintGrids(graph);
                        }

                        else
                        {
                            foreach (AxisScaleSegment scaleSegment in currentAxis.ScaleSegments)
                            {
                                scaleSegment.SetTempAxisScaleAndInterval();

                                currentAxis.PaintGrids(graph);

                                scaleSegment.RestoreAxisScaleAndInterval();
                            }
                        }

                    }

                    // Draw Axis Striplines (only when StripWidth == 0)
                    foreach (Axis currentAxis in axesArray)
                    {

                        useScaleSegments = (currentAxis.ScaleSegments.Count > 0);

                        if (!useScaleSegments)
                        {
                            currentAxis.PaintStrips(graph, true);
                        }

                        else
                        {
                            foreach (AxisScaleSegment scaleSegment in currentAxis.ScaleSegments)
                            {
                                scaleSegment.SetTempAxisScaleAndInterval();

                                currentAxis.PaintStrips(graph, true);

                                scaleSegment.RestoreAxisScaleAndInterval();
                            }
                        }

                    }

                    // Draw Axis elements on the back of the 3D scene
                    if (this.Area3DStyle.Enable3D && !this.chartAreaIsCurcular)
                    {
                        foreach (Axis currentAxis in axesArray)
                        {

                            useScaleSegments = (currentAxis.ScaleSegments.Count > 0);

                            if (!useScaleSegments)
                            {
                                currentAxis.PrePaint(graph);
                            }

                            else
                            {
                                foreach (AxisScaleSegment scaleSegment in currentAxis.ScaleSegments)
                                {
                                    scaleSegment.SetTempAxisScaleAndInterval();

                                    currentAxis.PrePaint(graph);

                                    scaleSegment.RestoreAxisScaleAndInterval();
                                }

                            }

                        }
                    }

                    // Draws chart area border
                    bool borderDrawn = false;
                    if (this.Area3DStyle.Enable3D || !IsBorderOnTopSeries())
                    {
                        borderDrawn = true;
                        PaintAreaBack(graph, backgroundPosition, true);
                    }

                    // Draw chart types
                    if (!this.Area3DStyle.Enable3D || this.chartAreaIsCurcular)
                    {
                        // Drawing in 2D space

                        // NOTE: Fixes issue #6443 and #5385
                        // If two chart series of the same type (for example Line) are separated
                        // by other series (for example Area) the order is not correct.
                        // Old implementation draws ALL series that belongs to the chart type.
                        ArrayList typeAndSeries = this.GetChartTypesAndSeriesToDraw(); 
                        
                        // Draw series by chart type or by series
                        foreach (ChartTypeAndSeriesInfo chartTypeInfo in typeAndSeries)
                        {
                            this.IterationCounter = 0;
                            IChartType type = Common.ChartTypeRegistry.GetChartType(chartTypeInfo.ChartType);

                            // If 'chartTypeInfo.Series' set to NULL all series of that chart type are drawn at once
                            type.Paint(graph, Common, this, chartTypeInfo.Series);
                        }
                    }
                    else
                    {
                        // Drawing in 3D space
                        PaintChartSeries3D(graph);
                    }

                    // Draw area border if it wasn't drawn prior to the series
                    if (!borderDrawn)
                    {
                        PaintAreaBack(graph, backgroundPosition, true);
                    }

                    // Draw Axis
                    foreach (Axis currentAxis in axesArray)
                    {

                        useScaleSegments = (currentAxis.ScaleSegments.Count > 0);

                        if (!useScaleSegments)
                        {
                            // Paint axis and Reset temp axis offset for side-by-side charts like column
                            currentAxis.Paint(graph);
                        }

                        else
                        {
                            // Some of the axis elements like grid lines and tickmarks 
                            // are drawn for each segment
                            foreach (AxisScaleSegment scaleSegment in currentAxis.ScaleSegments)
                            {
                                scaleSegment.SetTempAxisScaleAndInterval();

                                currentAxis.PaintOnSegmentedScalePassOne(graph);

                                scaleSegment.RestoreAxisScaleAndInterval();
                            }

                            // Other elements like labels, title, axis line are drawn once
                            currentAxis.PaintOnSegmentedScalePassTwo(graph);
                        }

                    }

                    // Call Paint event
                    Common.Chart.CallOnPostPaint(new ChartPaintEventArgs(this, graph, Common, PlotAreaPosition));

                    // Draw axis scale break lines
                    axesArray = new Axis[] { axisY, axisY2 };
                    foreach (Axis currentAxis in axesArray)
                    {
                        for (int segmentIndex = 0; segmentIndex < (currentAxis.ScaleSegments.Count - 1); segmentIndex++)
                        {
                            currentAxis.ScaleSegments[segmentIndex].PaintBreakLine(graph, currentAxis.ScaleSegments[segmentIndex + 1]);

                        }
                    }

                    // Reset values for optimized drawing
                    foreach (Axis curentAxis in this._axisArray)
                    {
                        curentAxis.optimizedGetPosition = false;


                        // Reset preffered number of intervals on the axis
                        curentAxis.prefferedNumberofIntervals = 5;

                        // Reset flag that scale segments are used
                        curentAxis.scaleSegmentsUsed = false;


                    }
		}

		/// <summary>
		/// Checks if chart area border should be drawn on top of series.
		/// </summary>
		/// <returns>True if border should be darwn on top.</returns>
		private bool IsBorderOnTopSeries()
		{
			// For most of the chart types chart area border is drawn on top.
			bool result = true;
			foreach( Series series in this.Common.Chart.Series )
			{
				if(series.ChartArea == this.Name)
				{
					// It is common for the Bubble and Point chart types to draw markers
					// partially outside of the chart area. By drawing the border before
					// series we avoiding the possibility of drawing the border line on 
					// top of the marker.
					if(series.ChartType == SeriesChartType.Bubble || 
						series.ChartType == SeriesChartType.Point)
					{
						return false;
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Paint the chart area cursors.
		/// </summary>
		/// <param name="graph">Chart graphics.</param>
		/// <param name="cursorOnly">Indicates that only cursors are redrawn.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "These parameters are used when compiling for the Microsoft version of Chart")]
		internal void PaintCursors( ChartGraphics graph, bool cursorOnly )
		{
			// Cursors and selection are supoorted only in 2D charts
			if(this.Area3DStyle.Enable3D == true)
			{
				return;
			}

			// Do not draw cursor/selection for chart types that do not require axis (like Pie)
			if(!this.requireAxes)
			{
				return;
			}

			// Cursors and selection are not supoorted in circular areas
			if(this.chartAreaIsCurcular)
			{
				return;
			}

			// Do not draw cursor/selection while printing
			if(this.Common != null && 
				this.Common.ChartPicture != null && 
				this.Common.ChartPicture.isPrinting)
			{
				return;
			}

			// Do not draw cursor/selection when chart area is not visible
			// because either width or height is set to zero
			if(this.Position.Width == 0f ||
				this.Position.Height == 0f)
			{
				return;
            }

#if Microsoft_CONTROL

            Chart chart = this.Common.Chart;
            ChartPicture chartPicture = Common.ChartPicture;

			// Check if cursor should be drawn
			if(!double.IsNaN(_cursorX.SelectionStart) ||
				!double.IsNaN(_cursorX.SelectionEnd) ||
				!double.IsNaN(_cursorX.Position) ||
				!double.IsNaN(_cursorY.SelectionStart) ||
				!double.IsNaN(_cursorY.SelectionEnd) ||
				!double.IsNaN(_cursorY.Position))
			{

				if(!chartPicture.backgroundRestored &&
					!chartPicture.isSelectionMode )
				{
					chartPicture.backgroundRestored = true;

					Rectangle chartPosition = new Rectangle(0, 0, chartPicture.Width, chartPicture.Height);

					// Get chart area position
					Rectangle absAreaPlotPosition = Rectangle.Round(graph.GetAbsoluteRectangle(PlotAreaPosition.ToRectangleF()));
					int maxCursorWidth = (CursorY.LineWidth > CursorX.LineWidth) ? CursorY.LineWidth + 1 : CursorX.LineWidth + 1;
					absAreaPlotPosition.Inflate(maxCursorWidth, maxCursorWidth);
					absAreaPlotPosition.Intersect(new Rectangle(0, 0, chart.Width, chart.Height));

					// Create area buffer bitmap
					if(areaBufferBitmap == null || 
						chartPicture.nonTopLevelChartBuffer == null ||
						!cursorOnly)
					{
						// Dispose previous bitmap
						if(areaBufferBitmap != null)
						{
							areaBufferBitmap.Dispose();
							areaBufferBitmap = null;
						}
						if(chartPicture.nonTopLevelChartBuffer != null)
						{
							chartPicture.nonTopLevelChartBuffer.Dispose();
							chartPicture.nonTopLevelChartBuffer = null;
						}


						// Copy chart area plotting rectangle from the chart's dubble buffer image into area dubble buffer image
						if(chart.paintBufferBitmap != null)
						{
							areaBufferBitmap = chart.paintBufferBitmap.Clone(absAreaPlotPosition, chart.paintBufferBitmap.PixelFormat);
						}

						// Copy whole chart from the chart's dubble buffer image into area dubble buffer image
						if(chart.paintBufferBitmap != null && 
							chart.paintBufferBitmap.Size.Width >= chartPosition.Size.Width &&
							chart.paintBufferBitmap.Size.Height >= chartPosition.Size.Height)
						{
							chartPicture.nonTopLevelChartBuffer = chart.paintBufferBitmap.Clone(
								chartPosition, chart.paintBufferBitmap.PixelFormat);
						}

					}
					else if(cursorOnly && chartPicture.nonTopLevelChartBuffer != null)
					{
						// Restore previous background
						chart.paintBufferBitmapGraphics.DrawImageUnscaled(
							chartPicture.nonTopLevelChartBuffer,
							chartPosition);
					}
				}

                // Draw chart area cursors and range selection

				_cursorY.Paint(graph);
				_cursorX.Paint(graph);

            }
#endif // Microsoft_CONTROL

        }

		#endregion

		    #region Circular chart area methods

        /// <summary>
        /// Gets a circular chart type interface that belongs to this chart area.
        /// </summary>
        /// <returns>ICircularChartType interface or null.</returns>
		internal ICircularChartType GetCircularChartType()
		{
			// Get number of sectors in circular chart area
			foreach(Series series in this.Common.DataManager.Series)
			{
				if(series.IsVisible() && series.ChartArea == this.Name)
				{
					ICircularChartType type = Common.ChartTypeRegistry.GetChartType(series.ChartTypeName) as ICircularChartType;;
					if(type != null)
					{
						return type;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Calculate size of the circular axis labels and sets auto-fit font.
		/// </summary>
		/// <param name="chartGraph">Chart graphics object.</param>
		/// <param name="chartAreaPosition">The Chart area position.</param>
		/// <param name="plotArea">Plotting area size.</param>
		/// <param name="xTitleSize">Size of title on the axis.</param>
		/// <param name="yTitleSize">Size of title on the axis.</param>
		internal void FitCircularLabels(
			ChartGraphics chartGraph, 
			ElementPosition chartAreaPosition, 
			ref RectangleF plotArea,
			float xTitleSize,
			float yTitleSize)
		{
			// Check if axis labels are enabled
			if(!this.AxisX.LabelStyle.Enabled)
			{
				return;
			}

			// Get absolute titles size
			SizeF	titleSize = chartGraph.GetAbsoluteSize(new SizeF(xTitleSize, yTitleSize));

			// Get absolute position of area
			RectangleF plotAreaRectAbs = chartGraph.GetAbsoluteRectangle( plotArea );
			RectangleF areaRectAbs = chartGraph.GetAbsoluteRectangle( chartAreaPosition.ToRectangleF());

			// Get absolute markers size and spacing
			float	spacing = chartGraph.GetAbsolutePoint(new PointF(0, this.AxisX.markSize + Axis.elementSpacing)).Y;

			// Get circular axis list
			ArrayList axisList = GetCircularAxisList();

			// Get circular axis labels style
			CircularAxisLabelsStyle labelsStyle = GetCircularAxisLabelsStyle();

			//*****************************************************************
			//** Calculate the auto-fit font if required
			//*****************************************************************
			if(this.AxisX.LabelStyle.Enabled && this.AxisX.IsLabelAutoFit)
			{
				// Set max auto fit font
				this.AxisX.autoLabelFont = Common.ChartPicture.FontCache.GetFont(
                    this.AxisX.LabelStyle.Font.FontFamily, 
					14, 
					this.AxisX.LabelStyle.Font.Style, 
					GraphicsUnit.Point);

				// Get estimated labels size
				float labelsSizeEstimate = GetCircularLabelsSize(chartGraph, areaRectAbs, plotAreaRectAbs, titleSize);
				labelsSizeEstimate = (float)Math.Min(labelsSizeEstimate * 1.1f, plotAreaRectAbs.Width / 5f);
				labelsSizeEstimate += spacing;

				// Calculate auto-fit font
				this.AxisX.GetCircularAxisLabelsAutoFitFont(chartGraph, axisList, labelsStyle, plotAreaRectAbs, areaRectAbs, labelsSizeEstimate);
			}

			//*****************************************************************
			//** Shrink plot area size proportionally
			//*****************************************************************

			// Get labels size
			float labelsSize = GetCircularLabelsSize(chartGraph, areaRectAbs, plotAreaRectAbs, titleSize);

			// Check if change size is smaller than radius
			labelsSize = (float)Math.Min(labelsSize, plotAreaRectAbs.Width / 2.5f);
			labelsSize += spacing;
			
			plotAreaRectAbs.X += labelsSize;
			plotAreaRectAbs.Width -= 2f * labelsSize;
			plotAreaRectAbs.Y += labelsSize;
			plotAreaRectAbs.Height -= 2f * labelsSize;

			// Restrict minimum plot area size
			if(plotAreaRectAbs.Width < 1.0f)
			{
				plotAreaRectAbs.Width = 1.0f;
			}
			if(plotAreaRectAbs.Height < 1.0f)
			{
				plotAreaRectAbs.Height = 1.0f;
			}

			plotArea = chartGraph.GetRelativeRectangle( plotAreaRectAbs );
			

			//*****************************************************************
			//** Set axes labels size
			//*****************************************************************
			SizeF	relativeLabelSize = chartGraph.GetRelativeSize(new SizeF(labelsSize, labelsSize));
			this.AxisX.labelSize = relativeLabelSize.Height;
			this.AxisX2.labelSize = relativeLabelSize.Height;
			this.AxisY.labelSize = relativeLabelSize.Width;
			this.AxisY2.labelSize = relativeLabelSize.Width;

		}

		/// <summary>
		/// Calculate size of the circular axis labels.
		/// </summary>
		/// <param name="chartGraph">Chart graphics object.</param>
		/// <param name="areaRectAbs">The Chart area position.</param>
		/// <param name="plotAreaRectAbs">Plotting area size.</param>
		/// <param name="titleSize">Size of title on the axes.</param>
		/// <returns>Circulat labels style.</returns>
		internal float GetCircularLabelsSize(
			ChartGraphics chartGraph, 
			RectangleF areaRectAbs, 
			RectangleF plotAreaRectAbs,
			SizeF titleSize)
		{
			// Find current horiz. and vert. spacing between plotting and chart areas
			SizeF	areaDiff = new SizeF(plotAreaRectAbs.X - areaRectAbs.X, plotAreaRectAbs.Y - areaRectAbs.Y);
			areaDiff.Width -= titleSize.Width;
			areaDiff.Height -= titleSize.Height;

			// Get absolute center of the area
			PointF	areaCenterAbs = chartGraph.GetAbsolutePoint(this.circularCenter);

			// Get circular axis list
			ArrayList axisList = GetCircularAxisList();

			// Get circular axis labels style
			CircularAxisLabelsStyle labelsStyle = GetCircularAxisLabelsStyle();

			// Defines on how much (pixels) the circular chart area radius should be reduced
			float	labelsSize = 0f;

			//*****************************************************************
			//** Loop through all axis labels
			//*****************************************************************
			foreach(CircularChartAreaAxis axis in axisList)
			{
				//*****************************************************************
				//** Measure label text
				//*****************************************************************
				SizeF	textSize = chartGraph.MeasureString(
					axis.Title.Replace("\\n", "\n"), 
					(this.AxisX.autoLabelFont == null) ? this.AxisX.LabelStyle.Font : this.AxisX.autoLabelFont);
				textSize.Width = (float)Math.Ceiling(textSize.Width * 1.1f);
				textSize.Height = (float)Math.Ceiling(textSize.Height * 1.1f);


				//*****************************************************************
				//** Calculate area size change depending on labels style
				//*****************************************************************
				if(labelsStyle == CircularAxisLabelsStyle.Circular)
				{
					labelsSize = (float)Math.Max(
						labelsSize, 
						textSize.Height);
				}
				else if(labelsStyle == CircularAxisLabelsStyle.Radial)
				{
					float textAngle = axis.AxisPosition + 90;

					// For angled text find it's X and Y components
					float	width = (float)Math.Cos(textAngle/180F*Math.PI) * textSize.Width;
					float	height = (float)Math.Sin(textAngle/180F*Math.PI) * textSize.Width;
					width = (float)Math.Abs(Math.Ceiling(width));
					height = (float)Math.Abs(Math.Ceiling(height));

					// Reduce text size by current spacing between plotting area and chart area
					width -= areaDiff.Width;
					height -= areaDiff.Height;
					if(width < 0)
						width = 0;
					if(height < 0)
						height = 0;


					labelsSize = (float)Math.Max(
						labelsSize, 
						Math.Max(width, height));
				}
				else if(labelsStyle == CircularAxisLabelsStyle.Horizontal)
				{
					// Get text angle
					float textAngle = axis.AxisPosition;
					if(textAngle > 180f)
					{
						textAngle -= 180f;
					}

					// Get label rotated position
					PointF[]	labelPosition = new PointF[] { new PointF(areaCenterAbs.X, plotAreaRectAbs.Y) };
					Matrix newMatrix = new Matrix();
					newMatrix.RotateAt(textAngle, areaCenterAbs);
					newMatrix.TransformPoints(labelPosition);

					// Calculate width
					float width = textSize.Width;
					width -= areaRectAbs.Right - labelPosition[0].X;
					if(width < 0f)
					{
						width = 0f;
					}

					labelsSize = (float)Math.Max(
						labelsSize, 
						Math.Max(width, textSize.Height));
				}
			}

			return labelsSize;
		}

		/// <summary>
		/// True if polygons should be used instead of the circles for the chart area.
		/// </summary>
		internal bool CircularUsePolygons
		{
			get
			{
				// Check if value was precalculated
				if(this._circularUsePolygons == int.MinValue)
				{
					_circularUsePolygons = 0;

					// Look for custom properties in series
					foreach(Series series in this.Common.DataManager.Series)
					{
						if(series.ChartArea == this.Name && series.IsVisible())
						{
							// Get custom attribute
                            if (series.IsCustomPropertySet(CustomPropertyName.AreaDrawingStyle))
							{
								if(String.Compare(series[CustomPropertyName.AreaDrawingStyle], "Polygon", StringComparison.OrdinalIgnoreCase) == 0)
								{
									_circularUsePolygons = 1;
								}
                                else if (String.Compare(series[CustomPropertyName.AreaDrawingStyle], "Circle", StringComparison.OrdinalIgnoreCase) == 0)
								{
									_circularUsePolygons = 0;
								}
								else
								{
									throw(new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid( series[CustomPropertyName.AreaDrawingStyle], "AreaDrawingStyle")));
								}
								break;
							}
						}
					}
				}

				return (this._circularUsePolygons == 1);
			}
		}

		/// <summary>
		/// Gets circular area axis labels style.
		/// </summary>
		/// <returns>Axis labels style.</returns>
		internal CircularAxisLabelsStyle GetCircularAxisLabelsStyle()
		{
			CircularAxisLabelsStyle	style = CircularAxisLabelsStyle.Auto;

			// Get maximum number of points in all series
			foreach(Series series in this.Common.DataManager.Series)
			{
				if(series.IsVisible() && series.ChartArea == this.Name && series.IsCustomPropertySet(CustomPropertyName.CircularLabelsStyle))
				{
					string styleName = series[CustomPropertyName.CircularLabelsStyle];
					if(String.Compare( styleName, "Auto", StringComparison.OrdinalIgnoreCase) == 0 )
					{
						style = CircularAxisLabelsStyle.Auto;
					}
					else if(String.Compare( styleName,"Circular", StringComparison.OrdinalIgnoreCase) == 0)
					{
						style = CircularAxisLabelsStyle.Circular;
					}
					else if(String.Compare( styleName,"Radial", StringComparison.OrdinalIgnoreCase) == 0)
					{
						style = CircularAxisLabelsStyle.Radial;
					}
                    else if (String.Compare(styleName, "Horizontal", StringComparison.OrdinalIgnoreCase) == 0)
					{
						style = CircularAxisLabelsStyle.Horizontal;
					}
					else
					{
						throw(new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid( styleName, "CircularLabelsStyle")));
					}
					
				}
			}

			// Get auto style
			if(style == CircularAxisLabelsStyle.Auto)
			{
				int	sectorNumber = CircularSectorsNumber;
				style = CircularAxisLabelsStyle.Horizontal;
				if(sectorNumber > 30)
				{
					style = CircularAxisLabelsStyle.Radial;
				}
			}

			return style;
		}

		/// <summary>
		/// Number of sectors in the circular area.
		/// </summary>
		internal int CircularSectorsNumber
		{
			get
			{
				// Check if value was precalculated
				if(this._circularSectorNumber == int.MinValue)
				{
					this._circularSectorNumber = GetCircularSectorNumber();
				}

				return this._circularSectorNumber;
			}
		}

		/// <summary>
		/// Gets number of sectors in the circular chart area.
		/// </summary>
		/// <returns>Number of sectors.</returns>
		private int GetCircularSectorNumber()
		{
			ICircularChartType type = this.GetCircularChartType();
			if(type != null)
			{
				return type.GetNumerOfSectors(this, this.Common.DataManager.Series);
			}
			return 0;
		}

		/// <summary>
		/// Fills a list of circular axis.
		/// </summary>
		/// <returns>Axes list.</returns>
		internal ArrayList GetCircularAxisList()
		{
			// Check if list was already created
			if(_circularAxisList == null)
			{
				_circularAxisList = new ArrayList();

				// Loop through all sectors
				int sectorNumber = GetCircularSectorNumber();
				for(int sectorIndex = 0; sectorIndex < sectorNumber; sectorIndex++)
				{
					// Create new axis object
					CircularChartAreaAxis	axis = new CircularChartAreaAxis(sectorIndex * 360f/sectorNumber);

					// Check if custom X axis labels will be used
					if(this.AxisX.CustomLabels.Count > 0)
					{
						if(sectorIndex < this.AxisX.CustomLabels.Count)
						{
							axis.Title = this.AxisX.CustomLabels[sectorIndex].Text;
                            axis.TitleForeColor = this.AxisX.CustomLabels[sectorIndex].ForeColor;
						}
					}
					else
					{
						// Get axis title from all series
						foreach(Series series in this.Common.DataManager.Series)
						{
							if(series.IsVisible() && series.ChartArea == this.Name && sectorIndex < series.Points.Count)
							{
								if(series.Points[sectorIndex].AxisLabel.Length > 0)
								{
									axis.Title = series.Points[sectorIndex].AxisLabel;
									break;
								}
							}
						}
					}

					// Add axis into the list
					_circularAxisList.Add(axis);
				}

			}
			return _circularAxisList;
		}

		/// <summary>
		/// Converts circular position of the X axis to angle in degrees.
		/// </summary>
		/// <param name="position">X axis position.</param>
		/// <returns>Angle in degrees.</returns>
		internal float CircularPositionToAngle(double position)
		{
			// Get X axis scale size
			double scaleRatio = 360.0 / Math.Abs(this.AxisX.Maximum - this.AxisX.Minimum);
			
			return (float)(position * scaleRatio + this.AxisX.Crossing);
		}

		#endregion

            #region 2D Series drawing order methods

            /// <summary>
            /// Helper method that returns a list of 'ChartTypeAndSeriesInfo' objects.
            /// This list is used for chart area series drawing in 2D mode. Each
            /// object may represent an individual series or all series that belong
            /// to one chart type.
            /// 
            /// This method is intended to fix issues #6443 and #5385 when area chart 
            /// type incorrectly overlaps point or line chart type.
            /// </summary>
            /// <returns>List of 'ChartTypeAndSeriesInfo' objects.</returns>
            private ArrayList GetChartTypesAndSeriesToDraw()
            {
                ArrayList resultList = new ArrayList();

                // Build chart type or series position based lists
                if (this.ChartTypes.Count > 1 &&
                    (this.ChartTypes.Contains(ChartTypeNames.Area) 
                    || this.ChartTypes.Contains(ChartTypeNames.SplineArea)
                    )
                    )
                {
                    // Array of chart type names that do not require furher processing
                    ArrayList processedChartType = new ArrayList();
                    ArrayList splitChartType = new ArrayList();

                    // Draw using the exact order in the series collection
                    int seriesIndex = 0;
                    foreach (Series series in this.Common.DataManager.Series)
                    {
                        // Check if series is visible and belongs to the chart area
                        if (series.ChartArea==this.Name && series.IsVisible() && series.Points.Count > 0)
                        {
                            // Check if this chart type was already processed
                            if (!processedChartType.Contains(series.ChartTypeName))
                            {
                                // Check if curent chart type can be individually processed
                                bool canBeIndividuallyProcessed = false;
                                if (series.ChartType == SeriesChartType.Point ||
                                    series.ChartType == SeriesChartType.Line ||
                                    series.ChartType == SeriesChartType.Spline ||
                                    series.ChartType == SeriesChartType.StepLine)
                                {
                                    canBeIndividuallyProcessed = true;
                                }

                                if (!canBeIndividuallyProcessed)
                                {
                                    // Add a record to process all series of that chart type at once
                                    resultList.Add(new ChartTypeAndSeriesInfo(series.ChartTypeName));
                                    processedChartType.Add(series.ChartTypeName);
                                }
                                else
                                {
                                    // Check if curent chart type has more that 1 series and they are split 
                                    // by other series
                                    bool chartTypeIsSplit = false;

                                    if (splitChartType.Contains(series.ChartTypeName))
                                    {
                                        chartTypeIsSplit = true;
                                    }
                                    else
                                    {
                                        bool otherChartTypeFound = false;
                                        for (int curentSeriesIndex = seriesIndex + 1; curentSeriesIndex < this.Common.DataManager.Series.Count; curentSeriesIndex++)
                                        {
                                            if (series.ChartTypeName == this.Common.DataManager.Series[curentSeriesIndex].ChartTypeName)
                                            {
                                                if (otherChartTypeFound)
                                                {
                                                    chartTypeIsSplit = true;
                                                    splitChartType.Add(series.ChartTypeName);
                                                }
                                            }
                                            else
                                            {
                                                if (this.Common.DataManager.Series[curentSeriesIndex].ChartType == SeriesChartType.Area ||
                                                    this.Common.DataManager.Series[curentSeriesIndex].ChartType == SeriesChartType.SplineArea)
                                                {
                                                    otherChartTypeFound = true;
                                                }
                                            }
                                        }
                                    }

                                    if (chartTypeIsSplit)
                                    {
                                        // Add a record to process this series individually
                                        resultList.Add(new ChartTypeAndSeriesInfo(series));
                                    }
                                    else
                                    {
                                        // Add a record to process all series of that chart type at once
                                        resultList.Add(new ChartTypeAndSeriesInfo(series.ChartTypeName));
                                        processedChartType.Add(series.ChartTypeName);
                                    }
                                }
                            }
                        }

                        ++seriesIndex;
                    }
                }
                else
                {
                    foreach (string chartType in this.ChartTypes)
                    {
                        resultList.Add(new ChartTypeAndSeriesInfo(chartType));
                    }
                }

                return resultList;
            }

            /// <summary>
            /// Internal data structure that stores chart type name and optionally series object.
            /// </summary>
            internal class ChartTypeAndSeriesInfo
            {
                /// <summary>
                /// Object constructor.
                /// </summary>
                public ChartTypeAndSeriesInfo()
                {
                }

                /// <summary>
                /// Object constructor.
                /// </summary>
                /// <param name="chartType">Chart type name to initialize with.</param>
                public ChartTypeAndSeriesInfo(string chartType)
                {
                    this.ChartType = chartType;
                }

                /// <summary>
                /// Object constructor.
                /// </summary>
                /// <param name="series">Series to initialize with.</param>
                public ChartTypeAndSeriesInfo(Series series)
                {
                    this.ChartType = series.ChartTypeName;
                    this.Series = series;
                }

                // Chart type name
                internal string ChartType = string.Empty;

                // Series object. Can be set to NULL!
                internal Series Series = null;

            }

            #endregion // 2D Series drawing order methods

            #region IDisposable Members

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources
            /// </summary>
            /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "axisX")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "axisX2")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "axisY")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "axisY2")]
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (this._axisArray != null)
                    {
                        foreach (Axis axis in this._axisArray)
                        {
                            axis.Dispose();
                        }
                        this._axisArray = null;
                    }
                    if ( this._areaPosition != null)
                    {
                        this._areaPosition.Dispose();
                        this._areaPosition = null;
                    }
                    if (this._innerPlotPosition != null)
                    {
                        this._innerPlotPosition.Dispose();
                        this._innerPlotPosition = null;
                    }
                    if (this.PlotAreaPosition != null)
                    {
                        this.PlotAreaPosition.Dispose();
                        this.PlotAreaPosition = null;
                    }
#if Microsoft_CONTROL
                    if (this.areaBufferBitmap != null)
                    {
                        this.areaBufferBitmap.Dispose();
                        this.areaBufferBitmap = null;
                    }
                    if (this._cursorX != null)
                    {
                        this._cursorX.Dispose();
                        this._cursorX = null;
                    }
                    if (this._cursorY != null)
                    {
                        this._cursorY.Dispose();
                        this._cursorY = null;
                    }
#endif
                }
                base.Dispose(disposing);
            }


            #endregion

        }
}
