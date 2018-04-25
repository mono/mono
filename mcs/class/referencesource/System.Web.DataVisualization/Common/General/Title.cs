//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		Title.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	TitleCollection, Title, Docking
//
//  Purpose:	Titles can be added to the chart by simply including 
//              those titles into the Titles collection, which is 
//              found in the root Chart object. The Title object 
//              incorporates several properties that can be used to 
//              position, dock, and control the appearance of any 
//              Title. Title positioning can be explicitly set, or 
//              you can specify that your title be docked. The 
//              charting control gives you full control over all of 
//              the appearance properties of your Titles, so you have 
//              the ability to set specific properties for such things 
//              as fonts, or colors, and even text effects. 
//
// NOTE: In early versions of the Chart control only 1 title was 
// exposed through the Title, TitleFont and TitleFontColor properties 
// in the root chart object. Due to the customer requests, support for 
// unlimited number of titles was added through the TitleCollection 
// exposed as a Titles property of the root chart object. Old 
// properties were deprecated and marked as non-browsable. 
//
//	Reviewed:	AG - Microsoft 13, 2007
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
	using System.ComponentModel.Design.Serialization;
	using System.Reflection;
	using System.Windows.Forms.Design;
#else
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.DataVisualization.Charting.Data;
using System.Web.UI.DataVisualization.Charting.Utilities;
#endif


#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	#region Title enumerations

		/// <summary>
		/// An enumeration of chart element docking styles.
		/// </summary>
		public enum Docking
		{
			/// <summary>
			/// Docked to the top.
			/// </summary>
			Top, 

			/// <summary>
			/// Docked to the right.
			/// </summary>
			Right,

            /// <summary>
            /// Docked to the bottom.
            /// </summary>
			Bottom, 

			/// <summary>
			/// Docked to the left.
			/// </summary>
			Left,
		};

        /// <summary>
        /// Text drawing styles.
        /// </summary>
        public enum TextStyle
        {
            /// <summary>
            /// Default text drawing style.
            /// </summary>
            Default,

            /// <summary>
            /// Shadow text.
            /// </summary>
            Shadow,

            /// <summary>
            /// Emboss text.
            /// </summary>
            Emboss,

            /// <summary>
            /// Embed text.
            /// </summary>
            Embed,

            /// <summary>
            /// Frame text.
            /// </summary>
            Frame
        }


        /// <summary>
        /// An enumeration of chart text orientation.
        /// </summary>
        public enum TextOrientation
        {
            /// <summary>
            /// Orientation is automatically determined based on the type of the 
            /// chart element it is used in.
            /// </summary>
            Auto,

            /// <summary>
            /// Horizontal text.
            /// </summary>
            Horizontal,

            /// <summary>
            /// Text rotated 90 degrees and oriented from top to bottom.
            /// </summary>
            Rotated90,

            /// <summary>
            /// Text rotated 270 degrees and oriented from bottom to top.
            /// </summary>
            Rotated270,

            /// <summary>
            /// Text characters are not rotated and position one below the other.
            /// </summary>
            Stacked
        }

	#endregion

	/// <summary>
    /// The Title class provides properties which define content, visual 
    /// appearance and position of the single chart title. It also 
    /// contains methods responsible for calculating title position, 
    /// drawing and hit testing.
	/// </summary>
	[
	SRDescription("DescriptionAttributeTitle5"),
	]
#if Microsoft_CONTROL
	public class Title : ChartNamedElement, IDisposable
#else
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class Title : ChartNamedElement, IDisposable, IChartMapArea
#endif
    {
		#region Fields

		// Spacing between title text and the border in pixels
		internal int						titleBorderSpacing = 4;


        //***********************************************************
		//** Private data members, which store properties values
		//***********************************************************

		// Title text
		private	string					_text = String.Empty;

        // Title drawing style
        private TextStyle               _style = TextStyle.Default;

		// Title position
		private ElementPosition			_position = null;

		// Background properties
		private bool					_visible = true;
		private Color					_backColor = Color.Empty;
		private ChartHatchStyle			_backHatchStyle = ChartHatchStyle.None;
		private string					_backImage = "";
		private ChartImageWrapMode		_backImageWrapMode = ChartImageWrapMode.Tile;
		private Color					_backImageTransparentColor = Color.Empty;
		private ChartImageAlignmentStyle			_backImageAlignment = ChartImageAlignmentStyle.TopLeft;
		private GradientStyle			_backGradientStyle = GradientStyle.None;
		private Color					_backSecondaryColor = Color.Empty;
		private int						_shadowOffset = 0;
		private Color					_shadowColor = Color.FromArgb(128, 0, 0, 0);

		// Border properties
		private Color					_borderColor = Color.Empty;
		private int						_borderWidth = 1;
		private ChartDashStyle			_borderDashStyle = ChartDashStyle.Solid;

		// Font properties
        private FontCache               _fontCache = new FontCache();
		private Font					_font;
		private Color					_foreColor = Color.Black;

		// Docking and Alignment properties
		private ContentAlignment		_alignment = ContentAlignment.MiddleCenter;
		private Docking					_docking = Docking.Top;
        private string                  _dockedToChartArea = Constants.NotSetValue;
		private bool					_isDockedInsideChartArea = true;
		private	int						_dockingOffset = 0;

		// Interactive properties
		private	string					_toolTip = String.Empty;

#if !Microsoft_CONTROL
        private string                  _url = String.Empty;
		private	string					_mapAreaAttributes = String.Empty;
        private string                  _postbackValue = String.Empty;
#endif

        // Default text orientation
        private TextOrientation         _textOrientation = TextOrientation.Auto;

		#endregion 

		#region Constructors and Initialization

		/// <summary>
        /// Title constructor.
		/// </summary>
		public Title()
		{
			Initialize(string.Empty, Docking.Top, null, Color.Black);
		}

		/// <summary>
		/// Public constructor.
		/// </summary>
		/// <param name="text">Title text.</param>
		public Title(string text)
		{
			Initialize(text, Docking.Top, null, Color.Black);
		}

		/// <summary>
        /// Title constructor.
		/// </summary>
		/// <param name="text">Title text.</param>
		/// <param name="docking">Title docking.</param>
		public Title(string text, Docking docking)
		{
			Initialize(text, docking, null, Color.Black);
		}

		/// <summary>
        /// Title constructor.
		/// </summary>
		/// <param name="text">Title text.</param>
		/// <param name="docking">Title docking.</param>
		/// <param name="font">Title font.</param>
		/// <param name="color">Title color.</param>
		public Title(string text, Docking docking, Font font, Color color)
		{
			Initialize(text, docking, font, color);
		}

		/// <summary>
		/// Initialize title object.
		/// </summary>
		/// <param name="text">Title text.</param>
		/// <param name="docking">Title docking.</param>
		/// <param name="font">Title font.</param>
		/// <param name="color">Title color.</param>
		private void Initialize(string text, Docking docking, Font font, Color color)
		{
			// Initialize fields
            this._position = new ElementPosition(this);
            this._font = _fontCache.DefaultFont;
			this._text = text;
			this._docking = docking;
			this._foreColor = color;
			if(font != null)
			{
				this._font = font;
			}
		}

		#endregion 

		#region	Properties

        /// <summary>
        /// Gets or sets the unique name of a ChartArea object.
        /// </summary>
        [

        SRCategory("CategoryAttributeMisc"),
        Bindable(true),
        SRDescription("DescriptionAttributeTitle_Name"),
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
        /// Gets or sets the text orientation.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        DefaultValue(TextOrientation.Auto),
        SRDescription("DescriptionAttribute_TextOrientation"),
        NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public TextOrientation TextOrientation
        {
            get
            {
                return this._textOrientation;
            }
            set
            {
                this._textOrientation = value;
                this.Invalidate(true);
            }
        }

		/// <summary>
		/// Gets or sets a flag that specifies whether the title is visible.
		/// </summary>
		/// <value>
		/// <b>True</b> if the title is visible; <b>false</b> otherwise.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(true),
		SRDescription("DescriptionAttributeTitle_Visible"),
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
				this.Invalidate(false);
			}
		}

		/// <summary>
		/// Gets or sets the chart area name which the title is docked to inside or outside.
		/// </summary>
		[
		SRCategory("CategoryAttributeDocking"),
		Bindable(true),
        DefaultValue(Constants.NotSetValue),
		SRDescription("DescriptionAttributeTitle_DockToChartArea"),
        TypeConverter(typeof(LegendAreaNameConverter)),
		NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
		]
		public string DockedToChartArea
		{
			get
			{
				return _dockedToChartArea;
			}
			set
			{
				if(value != _dockedToChartArea)
				{
					if(value.Length == 0)
					{
                        _dockedToChartArea = Constants.NotSetValue;
					}
					else
					{
                        if (Chart != null && Chart.ChartAreas != null)
                        {
                            Chart.ChartAreas.VerifyNameReference(value);
                        }
						_dockedToChartArea = value;
					}
					this.Invalidate(false);
				}
			}
		}

		/// <summary>
		/// Gets or sets a property which indicates whether the title is docked inside chart area. 
        /// DockedToChartArea property must be set first.
		/// </summary>
		[
        SRCategory("CategoryAttributeDocking"),
		Bindable(true),
		DefaultValue(true),
		SRDescription("DescriptionAttributeTitle_DockInsideChartArea"),
		NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
		]
		public bool IsDockedInsideChartArea
		{
			get
			{
				return _isDockedInsideChartArea;
			}
			set
			{
				if(value != _isDockedInsideChartArea)
				{
					_isDockedInsideChartArea = value;
					this.Invalidate(false);
				}
			}
		}

		/// <summary>
		/// Gets or sets the positive or negative offset of the docked title position.
		/// </summary>
		[
		SRCategory("CategoryAttributeDocking"),
		Bindable(true),
		DefaultValue(0),
		SRDescription("DescriptionAttributeTitle_DockOffset"),
		NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
		]
		public int DockingOffset
		{
			get
			{
				return _dockingOffset;
			}
			set
			{
				if(value != _dockingOffset)
				{
                    if (value < -100 || value > 100)
                    {
                        throw (new ArgumentOutOfRangeException("value", SR.ExceptionValueMustBeInRange("DockingOffset", (-100).ToString(CultureInfo.CurrentCulture), (100).ToString(CultureInfo.CurrentCulture))));
                    }
					_dockingOffset = value;
					this.Invalidate(false);
				}
			}
		}


		/// <summary>
		/// Gets or sets the position of the title.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		SRDescription("DescriptionAttributeTitle_Position"),
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
                if (Chart != null && Chart.serializationStatus == SerializationStatus.Saving)
				{
					if(_position.Auto)
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
						newPosition.SetPositionNoAuto(_position.X, _position.Y, _position.Width, _position.Height);
						return newPosition;
					}
				}
				return _position;
			}
			set
			{
				_position = value;
                _position.Parent = this;
				this.Invalidate(false);
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
		/// Gets or sets the text of the title.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(""),
		SRDescription("DescriptionAttributeTitle_Text"),
		NotifyParentPropertyAttribute(true),
		ParenthesizePropertyNameAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = (value == null) ? string.Empty : value;
				this.Invalidate(false);
			}
		}


        /// <summary>
        /// Title drawing style.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        DefaultValue(TextStyle.Default),
        SRDescription("DescriptionAttributeTextStyle"),
        NotifyParentPropertyAttribute(true),
        #if !Microsoft_CONTROL
        PersistenceMode(PersistenceMode.Attribute)
        #endif
        ]
        public TextStyle TextStyle
        {
            get
            {
                return _style;
            }
            set
            {
                _style = value;
                this.Invalidate(true);
            }
        }


		/// <summary>
		/// Gets or sets the background color of the title.
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
				this.Invalidate(true);
			}
		}

		/// <summary>
		/// Gets or sets the border color of the title.
		/// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), ""),
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
				this.Invalidate(true);
			}
		}

		/// <summary>
		/// Gets or sets the border style of the title.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ChartDashStyle.Solid),
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
				this.Invalidate(true);
			}
		}

		/// <summary>
		/// Gets or sets the border width of the title.
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
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionTitleBorderWidthIsNegative));
				}
				_borderWidth = value;
				this.Invalidate(false);
			}
		}
		
		/// <summary>
		/// Gets or sets the background image.
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
		NotifyParentPropertyAttribute(true),
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
				this.Invalidate(true);
			}
		}

        /// <summary>
        /// Gets or sets the background image drawing mode.
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
				this.Invalidate(true);
			}
		}

        /// <summary>
        /// Gets or sets a color which will be replaced with a transparent color while drawing the background image.
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
				this.Invalidate(true);
			}
		}

        /// <summary>
        /// Gets or sets the background image alignment used by unscale drawing mode.
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
				this.Invalidate(true);
			}
		}

        /// <summary>
        /// Gets or sets the background gradient style.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="GradientStyle"/> value used for the background.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the gradient, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
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
				this.Invalidate(true);
			}
		}

        /// <summary>
        /// Gets or sets the secondary background color.
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the secondary color of a background with 
        /// hatching or gradient fill.
        /// </value>
        /// <remarks>
        /// This color is used with <see cref="BackColor"/> when <see cref="BackHatchStyle"/> or
        /// <see cref="BackGradientStyle"/> are used.
        /// </remarks>
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
				this.Invalidate(true);
			}
		}

        /// <summary>
        /// Gets or sets the background hatch style.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="ChartHatchStyle"/> value used for the background.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the hatching, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
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
				this.Invalidate(true);
			}
		}

		/// <summary>
		/// Gets or sets the title font.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Font), "Microsoft Sans Serif, 8pt"),
		SRDescription("DescriptionAttributeTitle_Font"),
		NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public Font Font
		{
			get
			{
				return _font;
			}
			set
			{
				_font = value;
				this.Invalidate(false);
			}
		}

		/// <summary>
		/// Gets or sets the title fore color.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), "Black"),
		SRDescription("DescriptionAttributeTitle_Color"),
		NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public Color ForeColor
		{
			get
			{
				return _foreColor;
			}
			set
			{
				_foreColor = value;
				this.Invalidate(true);
			}
		}

		/// <summary>
		/// Gets or sets title alignment.
		/// </summary>
		[
		SRCategory("CategoryAttributeDocking"),
		Bindable(true),
		DefaultValue(ContentAlignment.MiddleCenter),
		SRDescription("DescriptionAttributeTitle_Alignment"),
		NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public ContentAlignment Alignment
		{
			get
			{
				return _alignment;
			}
			set
			{
				_alignment = value;
				this.Invalidate(false);
			}
		}
		
		/// <summary>
		/// Gets or sets the title docking style.
		/// </summary>
		[
        SRCategory("CategoryAttributeDocking"),
		Bindable(true),
		DefaultValue(Docking.Top),
		SRDescription("DescriptionAttributeTitle_Docking"),
		NotifyParentPropertyAttribute(true),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public Docking Docking
		{
			get
			{
				return _docking;
			}
			set
			{
				_docking = value;
				this.Invalidate(false);
			}
		}

		/// <summary>
		/// Gets or sets the title shadow offset.
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
				this.Invalidate(false);
			}
		}

		/// <summary>
		/// Gets or sets the title shadow color.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), "128, 0, 0, 0"),
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
				this.Invalidate(false);
			}
		}

		/// <summary>
		/// Gets or sets the tooltip.
		/// </summary>
		[
#if !Microsoft_CONTROL
		SRCategory("CategoryAttributeMapArea"),
#else
		SRCategory("CategoryAttributeToolTip"),
#endif
		Bindable(true),
        SRDescription("DescriptionAttributeToolTip"),
		DefaultValue(""),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public string ToolTip
		{
			set
			{
				_toolTip = value;
			}
			get
			{
				return _toolTip;
			}
		}


#if !Microsoft_CONTROL

		/// <summary>
		/// Gets or sets the URL target of the title.
		/// </summary>
		[
		SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
		SRDescription("DescriptionAttributeUrl"),
		DefaultValue(""),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
        Editor(Editors.UrlValueEditor.Editor, Editors.UrlValueEditor.Base)
#endif
		]
		public string Url
		{
			set
			{
				_url = value;
			}
			get
			{
				return _url;
			}
		}
		/// <summary>
		/// Gets or sets the other attributes of the title map area.
		/// </summary>
		[
		SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
		SRDescription("DescriptionAttributeMapAreaAttributes"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string MapAreaAttributes
		{
			set
			{
				_mapAreaAttributes = value;
			}
			get
			{
				return _mapAreaAttributes;
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


#endif

        /// <summary>
		/// True if title background or border is visible
		/// </summary>
		internal bool BackGroundIsVisible
		{
			get
			{
				if(!this.BackColor.IsEmpty ||
					this.BackImage.Length > 0 ||
					(!this.BorderColor.IsEmpty && this.BorderDashStyle != ChartDashStyle.NotSet) )
				{
					return true;
				}

				return false;
			}
		}

		#endregion

		#region Helper Methods

        /// <summary>
        /// Checks if chart title is drawn vertically.
        /// Note: From the drawing perspective stacked text orientation is not vertical.
        /// </summary>
        /// <returns>True if text is vertical.</returns>
        private bool IsTextVertical
        {
            get
            {
                TextOrientation currentTextOrientation = this.GetTextOrientation();
                return currentTextOrientation == TextOrientation.Rotated90 || currentTextOrientation == TextOrientation.Rotated270;
            }
        }

        /// <summary>
        /// Returns title text orientation. If set to Auto automatically determines the
        /// orientation based on title docking.
        /// </summary>
        /// <returns>Current text orientation.</returns>
        private TextOrientation GetTextOrientation()
        {
            if (this.TextOrientation == TextOrientation.Auto)
            {
                // When chart title is docked to the left or right we automatically 
                // set vertical text with different rotation angles.
                if (this.Position.Auto)
                {
                    if (this.Docking == Docking.Left)
                    {
                        return TextOrientation.Rotated270;
                    }
                    else if (this.Docking == Docking.Right)
                    {
                        return TextOrientation.Rotated90;
                    }
                }
                return TextOrientation.Horizontal;
            }
            return this.TextOrientation;
        }

		/// <summary>
		/// Helper method that checks if title is visible.
		/// </summary>
		/// <returns>True if title is visible.</returns>
		internal bool IsVisible()
		{
			if(this.Visible)
			{

				// Check if title is docked to the chart area
				if(this.DockedToChartArea.Length > 0 &&
					this.Chart != null)
				{
					if(this.Chart.ChartAreas.IndexOf(this.DockedToChartArea) >= 0)
					{
						// Do not show title when it is docked to invisible chart area
						ChartArea area = this.Chart.ChartAreas[this.DockedToChartArea];
						if(!area.Visible)
						{
							return false;
						}
					}
				}
					

				return true;
			}
			return false;
		}

		/// <summary>
		/// Invalidate chart title when one of the properties is changed.
		/// </summary>
		/// <param name="invalidateTitleOnly">Indicates that only title area should be invalidated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "This parameter is used when compiling for the Microsoft version of Chart")]
		internal void Invalidate(bool invalidateTitleOnly)
		{
#if Microsoft_CONTROL
			if(Chart != null)
			{
				// Set dirty flag
				Chart.dirtyFlag = true;

				// Invalidate chart
				if(invalidateTitleOnly)
				{
					// Calculate the position of the title
					Rectangle	invalRect = Chart.ClientRectangle;
					if(this.Position.Width != 0 && this.Position.Height != 0 )
					{
						// Convert relative coordinates to absolute coordinates
                        invalRect.X = (int)(this.Position.X * (Common.ChartPicture.Width - 1) / 100F);
                        invalRect.Y = (int)(this.Position.Y * (Common.ChartPicture.Height - 1) / 100F);
                        invalRect.Width = (int)(this.Position.Width * (Common.ChartPicture.Width - 1) / 100F);
                        invalRect.Height = (int)(this.Position.Height * (Common.ChartPicture.Height - 1) / 100F); 

						// Inflate rectangle size using border size and shadow size
						invalRect.Inflate(this.BorderWidth + this.ShadowOffset + 1, this.BorderWidth + this.ShadowOffset + 1);
					}

					// Invalidate title rectangle only
					Chart.Invalidate(invalRect);
				}
				else
				{
					Invalidate();
				}
			}
#endif // #if Microsoft_CONTROL
		}

		#endregion 

		#region Painting and Selection Methods

		/// <summary>
		/// Paints title using chart graphics object.
		/// </summary>
		/// <param name="chartGraph">The graph provides drawing object to the display device. A Graphics object is associated with a specific device context.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        internal void Paint(ChartGraphics chartGraph )
		{
			// check if title is visible
			if(!this.IsVisible())
			{
				return;
			}

			// Title text
			string	titleText = this.Text;
			
			//***************************************************************
			//** Calculate title relative position
			//***************************************************************
			RectangleF	titlePosition = this.Position.ToRectangleF();
			
			// Auto set the title position if width or height is not set for custom position
			if(!this.Position.Auto && Common != null && Common.ChartPicture != null)
			{
				if(titlePosition.Width == 0 || titlePosition.Height == 0)
				{
					// Calculate text layout area
					SizeF layoutArea = new SizeF(
                        (titlePosition.Width == 0) ? Common.ChartPicture.Width : titlePosition.Width,
                        (titlePosition.Height == 0) ? Common.ChartPicture.Height : titlePosition.Height);
                    if (this.IsTextVertical)
                    {
                        float tempValue = layoutArea.Width;
                        layoutArea.Width = layoutArea.Height;
                        layoutArea.Height = tempValue;
                    }

					// Measure text size
					layoutArea = chartGraph.GetAbsoluteSize(layoutArea);
					SizeF titleSize = chartGraph.MeasureString(
						"W" + titleText.Replace("\\n", "\n"), 
						this.Font, 
						layoutArea, 
						StringFormat.GenericDefault,
                        this.GetTextOrientation());

                    // Increase text size by 4 pixels
					if(this.BackGroundIsVisible)
					{
						titleSize.Width += titleBorderSpacing;
						titleSize.Height += titleBorderSpacing;
					}

					// Switch width and height for vertical text
                    if (this.IsTextVertical)
                    {
                        float tempValue = titleSize.Width;
                        titleSize.Width = titleSize.Height;
                        titleSize.Height = tempValue;
                    }

					// Convert text size to relative coordinates
					titleSize = chartGraph.GetRelativeSize(titleSize);

					// Update custom position
					if(titlePosition.Width == 0)
					{
						titlePosition.Width = titleSize.Width;
						if(this.Alignment == ContentAlignment.BottomRight ||
							this.Alignment == ContentAlignment.MiddleRight ||
							this.Alignment == ContentAlignment.TopRight)
						{
							titlePosition.X = titlePosition.X - titlePosition.Width;
						}
						else if(this.Alignment == ContentAlignment.BottomCenter ||
							this.Alignment == ContentAlignment.MiddleCenter ||
							this.Alignment == ContentAlignment.TopCenter)
						{
							titlePosition.X = titlePosition.X - titlePosition.Width/2f;
						}
					}
					if(titlePosition.Height == 0)
					{
						titlePosition.Height = titleSize.Height;
						if(this.Alignment == ContentAlignment.BottomRight ||
							this.Alignment == ContentAlignment.BottomCenter ||
							this.Alignment == ContentAlignment.BottomLeft)
						{
							titlePosition.Y = titlePosition.Y - titlePosition.Height;
						}
						else if(this.Alignment == ContentAlignment.MiddleCenter ||
							this.Alignment == ContentAlignment.MiddleLeft ||
							this.Alignment == ContentAlignment.MiddleRight)
						{
							titlePosition.Y = titlePosition.Y - titlePosition.Height/2f;
						}
					}

				}
			}

			//***************************************************************
			//** Convert title position to absolute coordinates
			//***************************************************************
			RectangleF	absPosition = new RectangleF(titlePosition.Location, titlePosition.Size);
			absPosition = chartGraph.GetAbsoluteRectangle(absPosition);

			//***************************************************************
			//** Draw title background, border and shadow
			//***************************************************************
			if(this.BackGroundIsVisible && Common.ProcessModePaint )
			{
				chartGraph.FillRectangleRel( titlePosition, 
					BackColor, 
					BackHatchStyle, 
					BackImage, 
					BackImageWrapMode, 
					BackImageTransparentColor,
					BackImageAlignment,
					BackGradientStyle, 
					BackSecondaryColor,
					BorderColor, 
					BorderWidth, 
					BorderDashStyle, 
					ShadowColor, 
					ShadowOffset,
					PenAlignment.Inset);
			}
			else 
			{
				// Adjust text position to be only around the text itself
				SizeF titleArea = chartGraph.GetAbsoluteSize(titlePosition.Size);
				SizeF titleSize = chartGraph.MeasureString(
                    "W" + titleText.Replace("\\n", "\n"), 
					this.Font, 
					titleArea,
                    StringFormat.GenericDefault,
                    this.GetTextOrientation());

				// Convert text size to relative coordinates
				titleSize = chartGraph.GetRelativeSize(titleSize);

				// Adjust position depending on alignment
				RectangleF exactTitleRect = new RectangleF(
					titlePosition.X, 
					titlePosition.Y, 
					titleSize.Width,
					titleSize.Height);
				if(this.Alignment == ContentAlignment.BottomCenter ||
					this.Alignment == ContentAlignment.BottomLeft ||
					this.Alignment == ContentAlignment.BottomRight )
				{
					exactTitleRect.Y = titlePosition.Bottom - exactTitleRect.Height;
				}
				else if(this.Alignment == ContentAlignment.MiddleCenter ||
					this.Alignment == ContentAlignment.MiddleLeft ||
					this.Alignment == ContentAlignment.MiddleRight )
				{
					exactTitleRect.Y = titlePosition.Y + 
						titlePosition.Height / 2f - 
						exactTitleRect.Height / 2f;
				}
				
				if(this.Alignment == ContentAlignment.BottomRight ||
					this.Alignment == ContentAlignment.MiddleRight ||
					this.Alignment == ContentAlignment.TopRight )
				{
					exactTitleRect.X = titlePosition.Right - exactTitleRect.Width;
				}
				else if(this.Alignment == ContentAlignment.BottomCenter ||
					this.Alignment == ContentAlignment.MiddleCenter ||
					this.Alignment == ContentAlignment.TopCenter )
				{
					exactTitleRect.X = titlePosition.X + 
						titlePosition.Width / 2f - 
						exactTitleRect.Width / 2f;
				}

				// NOTE: This approach for text selection can not be used with
				// Flash animations because of the bug in Flash viewer. When the 
				// button shape is placed in the last frame the Alpha value of the
				// color is ignored.

				// NOTE: Feature tested again with Flash Player 7 and it seems to be 
				// working fine. Code below is commented to enable selection in flash
				// through transparent rectangle.
				// Fixes issue #4172.

				bool drawRect = true;

				// Draw transparent rectangle in the text position
				if(drawRect)
				{
					chartGraph.FillRectangleRel( 
						exactTitleRect, 
						Color.FromArgb(0, Color.White),
						ChartHatchStyle.None,
						String.Empty, 
						ChartImageWrapMode.Tile, 
						BackImageTransparentColor,
						BackImageAlignment,
						GradientStyle.None, 
						BackSecondaryColor,
						Color.Transparent, 
						0, 
						BorderDashStyle, 
						Color.Transparent, 
						0,
						PenAlignment.Inset);
				}

				// End Selection mode
				chartGraph.EndHotRegion( );
			}

            if( Common.ProcessModePaint)
                Common.Chart.CallOnPrePaint(new ChartPaintEventArgs(this, chartGraph, Common, Position));

			//***************************************************************
			//** Add spacing between text and border
			//***************************************************************
			if(this.BackGroundIsVisible)
			{
				absPosition.Width -= this.titleBorderSpacing;
				absPosition.Height -= this.titleBorderSpacing;
				absPosition.X += this.titleBorderSpacing / 2f;
				absPosition.Y += this.titleBorderSpacing / 2f;
			}

			//***************************************************************
			//** Create string format
			//***************************************************************
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = StringAlignment.Center;
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

                //***************************************************************
                //** Draw text shadow for the default style when background is not drawn anf ShadowOffset is not null
                //***************************************************************
                Color textShadowColor = ChartGraphics.GetGradientColor(this.ForeColor, Color.Black, 0.8);
                int textShadowOffset = 1;
                TextStyle textStyle = this.TextStyle;
                if ((textStyle == TextStyle.Default || textStyle == TextStyle.Shadow) &&
                    !this.BackGroundIsVisible &&
                    ShadowOffset != 0)
                {
                    // Draw shadowed text
                    textStyle = TextStyle.Shadow;
                    textShadowColor = ShadowColor;
                    textShadowOffset = ShadowOffset;
                }
                
                if (textStyle == TextStyle.Shadow)
                {
                    textShadowColor = (textShadowColor.A != 255) ? textShadowColor : Color.FromArgb(textShadowColor.A / 2, textShadowColor);
                }

                //***************************************************************
                //** Replace new line characters
                //***************************************************************
                titleText = titleText.Replace("\\n", "\n");

                //***************************************************************
                //** Define text angle depending on the docking
                //***************************************************************
                Matrix oldTransform = null;
                if (this.IsTextVertical)
                {
                    if (this.GetTextOrientation() == TextOrientation.Rotated270)
                    {
                        // IMPORTANT !
                        // Right to Left flag has to be used because of bug with .net with multi line vertical text. As soon as .net bug is fixed this flag HAS TO be removed. Bug number 1870.
                        format.FormatFlags |= StringFormatFlags.DirectionVertical | StringFormatFlags.DirectionRightToLeft;

                        // Save old graphics transformation
                        oldTransform = chartGraph.Transform.Clone();

                        // Rotate tile 180 degrees at center
                        PointF center = PointF.Empty;

                        center.X = absPosition.X + absPosition.Width / 2F;
                        center.Y = absPosition.Y + absPosition.Height / 2F;

                        // Create and set new transformation matrix
                        Matrix newMatrix = chartGraph.Transform.Clone();
                        newMatrix.RotateAt(180, center);
                        chartGraph.Transform = newMatrix;
                    }
                    else if (this.GetTextOrientation() == TextOrientation.Rotated90)
                    {
                        // IMPORTANT !
                        // Right to Left flag has to be used because of bug with .net with multi line vertical text. As soon as .net bug is fixed this flag HAS TO be removed. Bug number 1870.
                        format.FormatFlags |= StringFormatFlags.DirectionVertical | StringFormatFlags.DirectionRightToLeft;
                    }
                }
                try
                {
                    chartGraph.IsTextClipped = !Position.Auto;
                    Title.DrawStringWithStyle(chartGraph, titleText, textStyle, this.Font, absPosition, this.ForeColor, textShadowColor, textShadowOffset, format, this.GetTextOrientation());
                }
                finally
                {
                    chartGraph.IsTextClipped = false;
                }
                // Call Paint event
                if (Common.ProcessModePaint)
                    Common.Chart.CallOnPostPaint(new ChartPaintEventArgs(this, chartGraph, Common, Position));
            
			    //***************************************************************
			    //** Restore old transformation
			    //***************************************************************
			    if(oldTransform != null)
			    {
				    chartGraph.Transform = oldTransform;
			    }

			    if( Common.ProcessModeRegions )
			    {
#if !Microsoft_CONTROL
				Common.HotRegionsList.AddHotRegion( titlePosition, this.ToolTip, this.Url, this.MapAreaAttributes, this.PostBackValue, this, ChartElementType.Title, string.Empty );
#else
				Common.HotRegionsList.AddHotRegion( titlePosition, this.ToolTip, null, null, null, this, ChartElementType.Title, null );
#endif // !Microsoft_CONTROL
			    }
            }
        }

        /// <summary>
        /// Draws the string with style.
        /// </summary>
        /// <param name="chartGraph">The chart graph.</param>
        /// <param name="titleText">The title text.</param>
        /// <param name="textStyle">The text style.</param>
        /// <param name="font">The font.</param>
        /// <param name="absPosition">The abs position.</param>
        /// <param name="foreColor">Color of the fore.</param>
        /// <param name="shadowColor">Color of the shadow.</param>
        /// <param name="shadowOffset">The shadow offset.</param>
        /// <param name="format">The format.</param>
        /// <param name="orientation">The orientation.</param>
        internal static void DrawStringWithStyle(
            ChartGraphics chartGraph,
            string titleText,
            TextStyle textStyle, 
            Font font,
            RectangleF absPosition,
            Color foreColor,
            Color shadowColor,
            int shadowOffset,
            StringFormat format,
            TextOrientation orientation
            )
        {
            //***************************************************************
            //** Draw title text
            //***************************************************************
            if (titleText.Length > 0)
            {
                if (textStyle == TextStyle.Default)
                {
                    using (SolidBrush brush = new SolidBrush(foreColor))
                    {
                        chartGraph.DrawString(titleText, font, brush, absPosition, format, orientation);
                    }
                }
                else if (textStyle == TextStyle.Frame)
                {
                    using (GraphicsPath graphicsPath = new GraphicsPath())
                    {
                        graphicsPath.AddString(
                            titleText,
                            font.FontFamily,
                            (int)font.Style,
                            font.Size * 1.3f,
                            absPosition,
                            format);
                        graphicsPath.CloseAllFigures();


                        using (Pen pen = new Pen(foreColor, 1))
                        {
                            chartGraph.DrawPath(pen, graphicsPath);
                        }
                    }
                }
                else if (textStyle == TextStyle.Embed)
                {
                    // Draw shadow
                    RectangleF shadowPosition = new RectangleF(absPosition.Location, absPosition.Size);
                    shadowPosition.X -= 1;
                    shadowPosition.Y -= 1;
                    using (SolidBrush brush = new SolidBrush(shadowColor))
                    {
                        chartGraph.DrawString(titleText, font, brush, shadowPosition, format, orientation);
                    }
                    // Draw highlighting
                    shadowPosition.X += 2;
                    shadowPosition.Y += 2;
                    Color texthighlightColor = ChartGraphics.GetGradientColor(Color.White, foreColor, 0.3);
                    using (SolidBrush brush = new SolidBrush(texthighlightColor))
                    {
                        chartGraph.DrawString(titleText, font, brush, shadowPosition, format, orientation);
                    }
                    using (SolidBrush brush = new SolidBrush(foreColor))
                    {
                        // Draw text
                        chartGraph.DrawString(titleText, font, brush, absPosition, format, orientation);
                    }
                }
                else if (textStyle == TextStyle.Emboss)
                {
                    // Draw shadow
                    RectangleF shadowPosition = new RectangleF(absPosition.Location, absPosition.Size);
                    shadowPosition.X += 1;
                    shadowPosition.Y += 1;
                    using (SolidBrush brush = new SolidBrush(shadowColor))
                    {
                        chartGraph.DrawString(titleText, font, brush, shadowPosition, format, orientation);
                    }
                    // Draw highlighting
                    shadowPosition.X -= 2;
                    shadowPosition.Y -= 2;
                    Color texthighlightColor = ChartGraphics.GetGradientColor(Color.White, foreColor, 0.3);
                    using (SolidBrush brush = new SolidBrush(texthighlightColor))
                    {
                        chartGraph.DrawString(titleText, font, brush, shadowPosition, format, orientation);
                    }
                    // Draw text
                    using (SolidBrush brush = new SolidBrush(foreColor))
                    {
                        chartGraph.DrawString(titleText, font, brush, absPosition, format, orientation);
                    }

                }
                else if (textStyle == TextStyle.Shadow)
                {
                    // Draw shadow
                    RectangleF shadowPosition = new RectangleF(absPosition.Location, absPosition.Size);
                    shadowPosition.X += shadowOffset;
                    shadowPosition.Y += shadowOffset;
                    using (SolidBrush brush = new SolidBrush(shadowColor))
                    {
                        chartGraph.DrawString(titleText, font, brush, shadowPosition, format, orientation);
                    }
                    // Draw text
                    using (SolidBrush brush = new SolidBrush(foreColor))
                    {
                        chartGraph.DrawString(titleText, font, brush, absPosition, format, orientation);
                    }

                }
            }
        }

		#endregion 

		#region Position Calculation Methods

		/// <summary>
		/// Recalculates title position.
		/// </summary>
		/// <param name="chartGraph">Chart graphics used.</param>
		/// <param name="chartAreasRectangle">Area where the title should be docked.</param>
		/// <param name="frameTitlePosition">Position of the title in the frame.</param>
		/// <param name="elementSpacing">Spacing size in percentage of the area.</param>
		internal void CalcTitlePosition(
			ChartGraphics chartGraph, 
			ref RectangleF chartAreasRectangle, 
			ref RectangleF frameTitlePosition,
			float elementSpacing)
		{
			// Special case for the first title docked to the top when the title frame is used
			if(!frameTitlePosition.IsEmpty && 
				this.Position.Auto &&
				this.Docking == Docking.Top &&
                this.DockedToChartArea == Constants.NotSetValue)
			{
				this.Position.SetPositionNoAuto(
					frameTitlePosition.X + elementSpacing, 
					frameTitlePosition.Y, 
					frameTitlePosition.Width - 2f * elementSpacing, 
					frameTitlePosition.Height);
				frameTitlePosition = RectangleF.Empty;
				return;
			}

			// Get title size
			RectangleF		titlePosition = new RectangleF();
			SizeF			layoutArea = new SizeF(chartAreasRectangle.Width, chartAreasRectangle.Height);

			// Switch width and height for vertical text
            if (this.IsTextVertical)
            {
                float tempValue = layoutArea.Width;
                layoutArea.Width = layoutArea.Height;
                layoutArea.Height = tempValue;
            }

			// Meausure text size
			layoutArea.Width -= 2f * elementSpacing;
			layoutArea.Height -= 2f * elementSpacing;
			layoutArea = chartGraph.GetAbsoluteSize(layoutArea);
			SizeF titleSize = chartGraph.MeasureString(
                "W" + this.Text.Replace("\\n", "\n"), 
				this.Font, 
				layoutArea, 
				StringFormat.GenericDefault,
                this.GetTextOrientation());

            // Increase text size by 4 pixels
			if(this.BackGroundIsVisible)
			{
				titleSize.Width += titleBorderSpacing;
				titleSize.Height += titleBorderSpacing;
			}

			// Switch width and height for vertical text
            if (this.IsTextVertical)
            {
                float tempValue = titleSize.Width;
                titleSize.Width = titleSize.Height;
                titleSize.Height = tempValue;
            }

			// Convert text size to relative coordinates
			titleSize = chartGraph.GetRelativeSize(titleSize);
			titlePosition.Height = titleSize.Height;
			titlePosition.Width = titleSize.Width;
			if(float.IsNaN(titleSize.Height) || float.IsNaN(titleSize.Width))
			{
				return;
			}

			// Calculate title position
			if(this.Docking == Docking.Top)
			{
				titlePosition.Y = chartAreasRectangle.Y + elementSpacing;
				titlePosition.X = chartAreasRectangle.X + elementSpacing;
				titlePosition.Width = chartAreasRectangle.Right - titlePosition.X - elementSpacing;
				if(titlePosition.Width < 0)
				{
					titlePosition.Width = 0;
				}

				// Adjust position of the chart area(s)
				chartAreasRectangle.Height -= titlePosition.Height + elementSpacing;
				chartAreasRectangle.Y = titlePosition.Bottom;
			}
			else if(this.Docking == Docking.Bottom)
			{
				titlePosition.Y = chartAreasRectangle.Bottom - titleSize.Height - elementSpacing;
				titlePosition.X = chartAreasRectangle.X + elementSpacing;
				titlePosition.Width = chartAreasRectangle.Right - titlePosition.X - elementSpacing;
				if(titlePosition.Width < 0)
				{
					titlePosition.Width = 0;
				}

				// Adjust position of the chart area(s)
				chartAreasRectangle.Height -= titlePosition.Height + elementSpacing;
			}
			if(this.Docking == Docking.Left)
			{
				titlePosition.X = chartAreasRectangle.X + elementSpacing;
				titlePosition.Y = chartAreasRectangle.Y + elementSpacing;
				titlePosition.Height = chartAreasRectangle.Bottom - titlePosition.Y - elementSpacing;
				if(titlePosition.Height < 0)
				{
					titlePosition.Height = 0;
				}

				// Adjust position of the chart area(s)
				chartAreasRectangle.Width -= titlePosition.Width + elementSpacing;
				chartAreasRectangle.X = titlePosition.Right;
			}
			if(this.Docking == Docking.Right)
			{
				titlePosition.X = chartAreasRectangle.Right - titleSize.Width - elementSpacing;
				titlePosition.Y = chartAreasRectangle.Y + elementSpacing;
				titlePosition.Height = chartAreasRectangle.Bottom - titlePosition.Y - elementSpacing;
				if(titlePosition.Height < 0)
				{
					titlePosition.Height = 0;
				}

				// Adjust position of the chart area(s)
				chartAreasRectangle.Width -= titlePosition.Width + elementSpacing;
			}


			// Offset calculated docking position
			if(this.DockingOffset != 0)
			{
				if(this.Docking == Docking.Top || this.Docking == Docking.Bottom)
				{
					titlePosition.Y += this.DockingOffset;
				}
				else
				{
					titlePosition.X += this.DockingOffset;
				}
			}

			this.Position.SetPositionNoAuto(titlePosition.X, titlePosition.Y, titlePosition.Width, titlePosition.Height);
		}

		#endregion

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
                if (_position != null)
                {
                    _position.Dispose();
                    _position = null;
                }
            }
        }


        #endregion
	}

	/// <summary>
    /// The TitleCollection class is a strongly typed collection of Title classes.
    /// Indexer of this collection can take the title index (integer) or unique 
    /// title name (string) as a parameter.
	/// </summary>
	[
		SRDescription("DescriptionAttributeTitles"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class TitleCollection : ChartNamedElementCollection<Title>
	{

		#region Constructors

		/// <summary>
        /// TitleCollection constructor.
		/// </summary>
		/// <param name="parent">Parent chart element.</param>
        internal TitleCollection(IChartElement parent)
            : base(parent)
        {
        }

		#endregion 

		#region Methods

        /// <summary>
        /// Creates a new Title with the specified name and adds it to the collection.
        /// </summary>
        /// <param name="name">The new chart area name.</param>
        /// <returns>New title</returns>
        public Title Add(string name)
        {
            Title title = new Title(name);
            this.Add(title);
            return title;
        }


		/// <summary>
		/// Recalculates title position in the collection for titles docked outside of chart area.
		/// </summary>
		/// <param name="chartPicture">Chart picture object.</param>
		/// <param name="chartGraph">Chart graphics used.</param>
		/// <param name="area">Area the title is docked to.</param>
		/// <param name="chartAreasRectangle">Area where the title should be positioned.</param>
		/// <param name="elementSpacing">Spacing size in percentage of the area.</param>
		internal static void CalcOutsideTitlePosition(
			ChartPicture chartPicture,
			ChartGraphics chartGraph, 
			ChartArea area,
			ref RectangleF chartAreasRectangle, 
			float elementSpacing)
		{
			if(chartPicture != null)
			{
				// Get elemets spacing
				float areaSpacing = Math.Min((chartAreasRectangle.Height/100F) * elementSpacing, (chartAreasRectangle.Width/100F) * elementSpacing);

				// Loop through all titles
				foreach(Title title in chartPicture.Titles)
				{
					// Check if title visible
					if(!title.IsVisible())
					{
						continue;
					}

					// Check if all chart area names are valid
                    if (title.DockedToChartArea != Constants.NotSetValue && chartPicture.ChartAreas.IndexOf(title.DockedToChartArea)<0)
                    {
                        throw (new ArgumentException(SR.ExceptionChartTitleDockedChartAreaIsMissing((string)title.DockedToChartArea)));
                    }

					// Process only titles docked to specified area
					if(title.IsDockedInsideChartArea == false &&
						title.DockedToChartArea == area.Name && 
						title.Position.Auto)
					{
						// Calculate title position
						RectangleF frameRect = RectangleF.Empty;
                        RectangleF prevChartAreasRectangle = chartAreasRectangle;
						title.CalcTitlePosition(chartGraph, 
							ref chartAreasRectangle,
							ref frameRect,
							areaSpacing);

						// Adjust title position
						RectangleF titlePosition = title.Position.ToRectangleF();
						if(title.Docking == Docking.Top)
						{
							titlePosition.Y -= areaSpacing;
							if(!area.Position.Auto)
							{
								titlePosition.Y -= titlePosition.Height;
                                prevChartAreasRectangle.Y -= titlePosition.Height + areaSpacing;
                                prevChartAreasRectangle.Height += titlePosition.Height + areaSpacing;
							}
						}
						else if(title.Docking == Docking.Bottom)
						{
							titlePosition.Y += areaSpacing;
							if(!area.Position.Auto)
							{
                                titlePosition.Y = prevChartAreasRectangle.Bottom + areaSpacing;
                                prevChartAreasRectangle.Height += titlePosition.Height +areaSpacing;
							}
						}
						if(title.Docking == Docking.Left)
						{
							titlePosition.X -= areaSpacing;
							if(!area.Position.Auto)
							{
								titlePosition.X -= titlePosition.Width;
                                prevChartAreasRectangle.X -= titlePosition.Width + areaSpacing;
                                prevChartAreasRectangle.Width += titlePosition.Width + areaSpacing;
                            }
						}
						if(title.Docking == Docking.Right)
						{
							titlePosition.X += areaSpacing;
							if(!area.Position.Auto)
							{
                                titlePosition.X = prevChartAreasRectangle.Right + areaSpacing;
                                prevChartAreasRectangle.Width += titlePosition.Width + areaSpacing;
							}
						}

                        // Set title position without changing the 'Auto' flag
						title.Position.SetPositionNoAuto(titlePosition.X, titlePosition.Y, titlePosition.Width, titlePosition.Height);

                        // If custom position is used in the chart area reset the curent adjusted position
                        if (!area.Position.Auto)
                        {
                            chartAreasRectangle = prevChartAreasRectangle;
                        }

					}
				}

			}
		}

		/// <summary>
		/// Recalculates all titles position inside chart area in the collection.
		/// </summary>
		/// <param name="chartPicture">Chart picture object.</param>
		/// <param name="chartGraph">Chart graphics used.</param>
		/// <param name="elementSpacing">Spacing size in percentage of the area.</param>
		internal static void CalcInsideTitlePosition(
			ChartPicture chartPicture,
			ChartGraphics chartGraph, 
			float elementSpacing)
		{
			if(chartPicture != null)
			{
				// Check if all chart area names are valid
				foreach(Title title in chartPicture.Titles)
				{
					// Check if title visible
					if(!title.IsVisible())
					{
						continue;
					}

                    if (title.DockedToChartArea != Constants.NotSetValue)
					{
						try
						{
							ChartArea area = chartPicture.ChartAreas[title.DockedToChartArea];
						}
						catch
						{
							throw(new ArgumentException( SR.ExceptionChartTitleDockedChartAreaIsMissing( (string)title.DockedToChartArea ) ) );
						}
					}
				}

				// Loop through all chart areas
				foreach(ChartArea area in chartPicture.ChartAreas)
				{

					// Check if chart area is visible
					if(area.Visible)

					{
						// Get area position
						RectangleF titlePlottingRectangle = area.PlotAreaPosition.ToRectangleF();

						// Get elemets spacing
						float areaSpacing = Math.Min((titlePlottingRectangle.Height/100F) * elementSpacing, (titlePlottingRectangle.Width/100F) * elementSpacing);

						// Loop through all titles
						foreach(Title title in chartPicture.Titles)
						{
							if(title.IsDockedInsideChartArea == true &&
								title.DockedToChartArea == area.Name && 
								title.Position.Auto)
							{
								// Calculate title position
								RectangleF frameRect = RectangleF.Empty;
								title.CalcTitlePosition(chartGraph, 
									ref titlePlottingRectangle, 
									ref frameRect,
									areaSpacing);
							}
						}
					}
				}
			}
		}

		#endregion

        #region Event handlers
        internal void ChartAreaNameReferenceChanged(object sender, NameReferenceChangedEventArgs e)
        {
            //If all the chart areas are removed and then the first one is added we don't want to dock the titles
            if (e.OldElement == null)
                return;

            foreach (Title title in this)
                if (title.DockedToChartArea == e.OldName)
                    title.DockedToChartArea = e.NewName;
        }
        #endregion


	}
}
