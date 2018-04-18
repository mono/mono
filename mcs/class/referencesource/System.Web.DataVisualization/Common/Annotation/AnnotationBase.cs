//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		Annotation.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	Annotation, AnnotationPositionChangingEventArgs
//
//  Purpose:	Base class for all anotation objects. Provides 
//				basic set of properties and methods.
//
//	Reviewed:	
//
//===================================================================

#region Used namespace
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#if Microsoft_CONTROL
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Windows.Forms;
	using System.Windows.Forms.DataVisualization.Charting;
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
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
	#region Enumerations

	/// <summary>
	/// Annotation object selection points style.
	/// </summary>
	/// <remarks>
	/// Enumeration is for internal use only and should not be part of the documentation.
	/// </remarks>
	internal enum SelectionPointsStyle
	{
		/// <summary>
		/// Selection points are displayed top left and bottom right corners
		/// </summary>
		TwoPoints,

		/// <summary>
		/// Selection points are displayed on all sides and corners of the rectangle.
		/// </summary>
		Rectangle,
	}

	/// <summary>
	/// Annotation object resizing\moving mode.
	/// </summary>
	/// <remarks>
	/// Enumeration is for internal use only and should not be part of the documentation.
	/// </remarks>
	internal enum ResizingMode
	{
		/// <summary>
		/// Top Left selection handle is used.
		/// </summary>
		TopLeftHandle = 0,
		/// <summary>
		/// Top selection handle is used.
		/// </summary>
		TopHandle = 1,
		/// <summary>
		/// Top Right selection handle is used.
		/// </summary>
		TopRightHandle = 2,
		/// <summary>
		/// Right selection handle is used.
		/// </summary>
		RightHandle = 3,
		/// <summary>
		/// Bottom Right selection handle is used.
		/// </summary>
		BottomRightHandle = 4,
		/// <summary>
		/// Bottom selection handle is used.
		/// </summary>
		BottomHandle = 5,
		/// <summary>
		/// Bottom Left selection handle is used.
		/// </summary>
		BottomLeftHandle = 6,
		/// <summary>
		/// Left selection handle is used.
		/// </summary>
		LeftHandle = 7,
		/// <summary>
		/// Anchor selection handle is used.
		/// </summary>
		AnchorHandle = 8,
		/// <summary>
		/// No selection handles used - moving mode.
		/// </summary>
		Moving = 16,
		/// <summary>
		/// Moving points of the annotation path.
		/// </summary>
		MovingPathPoints = 32,
		/// <summary>
		/// No moving or resizing.
		/// </summary>
		None = 64,
	}

#endregion

	/// <summary>
	/// <b>Annotation</b> is an abstract class that defines properties and methods 
	/// common to all annotations.
	/// </summary>
	/// <remarks>
	/// All annotations are derived from the <b>Annotation</b> class, which can be 
	/// used to set properties common to all annotation objects (e.g. color, position, 
	/// anchoring and others). 
	/// </remarks>
	[
	SRDescription("DescriptionAttributeAnnotation_Annotation"),
	DefaultProperty("Name"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
#if !Microsoft_CONTROL
    public abstract class Annotation : ChartNamedElement, IChartMapArea
#else
    public abstract class Annotation : ChartNamedElement
#endif
	{
        #region Fields


		// Name of the chart area the annotation is clipped to
		private		string					_clipToChartArea = Constants.NotSetValue;

		// Indicates that annotation is selected
		private		bool					_isSelected = false;

		// Indicates that annotation size is defined in relative chart coordinates
		private		bool					_isSizeAlwaysRelative = true;
		
		// Position attribute fields
		private		double					_x = double.NaN;
		private		double					_y = double.NaN;
		private		double					_width = double.NaN;
		private		double					_height = double.NaN;

		// Annotation axes attaching fields
		private		string					_axisXName = String.Empty;
		private		string					_axisYName = String.Empty;
		private		Axis					_axisX = null;
		private		Axis					_axisY = null;

		// Visual attribute fields
		private		bool					_visible = true;
		private		ContentAlignment		_alignment = ContentAlignment.MiddleCenter;
		private		Color					_foreColor = Color.Black;
        private     FontCache               _fontCache = new FontCache();
        private     Font                    _textFont;
        private     TextStyle               _textStyle = TextStyle.Default;
		internal    Color					lineColor = Color.Black;
		private		int						_lineWidth = 1;
		private		ChartDashStyle			_lineDashStyle = ChartDashStyle.Solid;
		private		Color					_backColor = Color.Empty;
		private		ChartHatchStyle			_backHatchStyle = ChartHatchStyle.None;
		private		GradientStyle			_backGradientStyle = GradientStyle.None;
		private		Color					_backSecondaryColor = Color.Empty;
		private		Color					_shadowColor = Color.FromArgb(128, 0, 0, 0);
		private		int						_shadowOffset = 0;

		// Anchor position attribute fields
		private		string					_anchorDataPointName = String.Empty;
		private		DataPoint				_anchorDataPoint = null;
		private		DataPoint				_anchorDataPoint2 = null;
		private		double					_anchorX = double.NaN;
		private		double					_anchorY = double.NaN;
		internal	double					anchorOffsetX = 0.0;
        internal    double                  anchorOffsetY = 0.0;
        internal    ContentAlignment        anchorAlignment = ContentAlignment.BottomCenter;

		// Selection handles position (starting top-left and moving clockwise)
		internal	RectangleF[]			selectionRects = null;

		// Annotation tooltip
		private		string					_tooltip = String.Empty;

		// Selection handles size
		internal const int					selectionMarkerSize = 6;

		// Pre calculated relative position of annotation and anchor point
		internal	RectangleF				currentPositionRel = new RectangleF(float.NaN, float.NaN, float.NaN, float.NaN);
		internal	PointF					currentAnchorLocationRel = new PointF(float.NaN, float.NaN);

		// Smart labels style		
		private		AnnotationSmartLabelStyle	_smartLabelStyle = null;

		// Index of last selected point in the annotation path
		internal	int						currentPathPointIndex = -1;

		// Group this annotation belongs too
		internal	AnnotationGroup			annotationGroup = null;

#if Microsoft_CONTROL

		// Selection and editing permissions
        private     bool                    _allowSelecting = false;
        private     bool                    _allowMoving = false;
        private     bool                    _allowAnchorMoving = false;
        private     bool                    _allowResizing = false;
        private     bool                    _allowTextEditing = false;
        private     bool                    _allowPathEditing = false;

#endif //Microsoft_CONTROL

#if Microsoft_CONTROL

		// Indicates that annotation position was changed. Flag used to fire events.
		internal	bool					positionChanged = false;

		// Relative location of last placement position
		internal	PointF					lastPlacementPosition = PointF.Empty;

		// Relative location of annotation anchor, when it's started to move
		internal	PointF					startMoveAnchorLocationRel = PointF.Empty;

#endif // Microsoft_CONTROL

        // Relative position of annotation, when it's started to move/resize
		internal	RectangleF				startMovePositionRel = RectangleF.Empty;

		// Relative position of annotation, when it's started to move/resize
		internal	GraphicsPath			startMovePathRel = null;

#if !Microsoft_CONTROL

		// Annotation map area attributes
		private		string					_url = String.Empty;
		private		string					_mapAreaAttributes = String.Empty;
        private     string                  _postbackValue = String.Empty;

#endif	// !Microsoft_CONTROL

        /// <summary>
        /// Limit of annotation width and height.
        /// </summary>
        internal static  double             WidthHightLimit = 290000000;

		#endregion

		#region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Annotation"/> class.
        /// </summary>
        protected Annotation() 
        {
            _textFont = _fontCache.DefaultFont;
        } 

		#endregion

		#region Properties

		#region Miscellaneous
        
		/// <summary>
		/// Gets or sets an annotation's unique name.
		/// </summary>
		/// <value>
		/// A <b>string</b> that represents an annotation's unique name.
		/// </value>
		[
		SRCategory("CategoryAttributeMisc"),
		Bindable(true),
		SRDescription("DescriptionAttributeName4"),
		ParenthesizePropertyNameAttribute(true),
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
		/// Gets or sets an annotation's type name.
		/// </summary>
		/// <remarks>
		/// This property is used to get the name of each annotation Style
		/// (e.g. Line, Rectangle, Ellipse). 
		/// <para>
		/// This property is for internal use and is hidden at design and run time.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributeMisc"),
		Bindable(true),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		SRDescription("DescriptionAttributeAnnotation_AnnotationType"),
		]
		public abstract string AnnotationType
		{
			get;
		}


		/// <summary>
		/// Gets or sets the name of the chart area which an annotation is clipped to.
		/// </summary>
		/// <value>
		/// A string which represents the name of an existing chart area.
		/// </value>
		/// <remarks>
		/// If the chart area name is specified, an annotation will only be drawn inside the 
		/// plotting area of the chart area specified.  All parts of the annotation 
		/// outside of the plotting area will be clipped.
		/// <para>
		/// To disable chart area clipping, set the property to "NotSet" or an empty string.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributeMisc"),
        DefaultValue(Constants.NotSetValue),
		SRDescription("DescriptionAttributeAnnotationClipToChartArea"),
        TypeConverter(typeof(LegendAreaNameConverter))
		]
		virtual public string ClipToChartArea
		{
			get
			{
				return _clipToChartArea;
			}
			set
			{
                if (value != _clipToChartArea)
                {
                    if (String.IsNullOrEmpty(value))
                    {
                        _clipToChartArea = Constants.NotSetValue;
                    }
                    else
                    {
                        if (Chart != null && Chart.ChartAreas != null)
                        {
                            Chart.ChartAreas.VerifyNameReference(value);
                        }
                        _clipToChartArea = value;
                    }
                    this.Invalidate();
                }
			}
		}


		/// <summary>
		/// Gets or sets the smart labels style of an annotation.
		/// </summary>
		/// <value>
		/// An <see cref="AnnotationSmartLabelStyle"/> object that represents an annotation's 
		/// smart labels style properties.
		/// </value>
		/// <remarks>
		/// Smart labels are used to prevent an annotation from overlapping data point labels 
		/// and other annotations.
		/// <para>
		/// Note that data point labels must also have smart labels enabled.
		/// </para>
		/// </remarks>
		[
		Browsable(true),
		SRCategory("CategoryAttributeMisc"),
		Bindable(true),
		SRDescription("DescriptionAttributeSmartLabels"),
#if Microsoft_CONTROL
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
		]
		public AnnotationSmartLabelStyle SmartLabelStyle
		{
			get
			{
                if (this._smartLabelStyle == null)
                {
                    this._smartLabelStyle = new AnnotationSmartLabelStyle(this);
                }
                return _smartLabelStyle;
			}
			set
			{
				value.chartElement = this;
				_smartLabelStyle = value;
				this.Invalidate();
			}
		}

        /// <summary>
        /// Gets the group, if any, the annotation belongs to.
        /// </summary>
        [
        Browsable(false),
        DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
        SerializationVisibilityAttribute(SerializationVisibility.Hidden),
        ]
        public AnnotationGroup AnnotationGroup
        {
            get { return this.annotationGroup; }
        }

		#endregion

		#region Position

		/// <summary>
		/// Gets or sets a flag that specifies whether the size of an annotation is always 
		/// defined in relative chart coordinates.
		/// <seealso cref="Width"/>
		/// <seealso cref="Height"/>
		/// </summary>
		/// <value>
		/// <b>True</b> if an annotation's <see cref="Width"/> and <see cref="Height"/> are always 
		/// in chart relative coordinates, <b>false</b> otherwise.
		/// </value>
		/// <remarks>
		/// An annotation's width and height may be set in relative chart or axes coordinates. 
		/// By default, relative chart coordinates are used.
		/// <para>
		/// To use axes coordinates for size set the <b>IsSizeAlwaysRelative</b> property to 
		/// <b>false</b> and either anchor the annotation to a data point or set the 
		/// <see cref="AxisX"/> or <see cref="AxisY"/> properties.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(true),
		SRDescription("DescriptionAttributeSizeAlwaysRelative"),
		]
		virtual public bool IsSizeAlwaysRelative
		{
			get
			{
				return _isSizeAlwaysRelative;
			}
			set
			{
				_isSizeAlwaysRelative = value;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the X coordinate of an annotation.
		/// <seealso cref="AnchorDataPoint"/>
		/// <seealso cref="AnchorX"/>
		/// </summary>
		/// <value>
        /// A Double value that represents the X coordinate of an annotation.
		/// </value>
		/// <remarks>
		/// The X coordinate of an annotation is in relative chart coordinates or axes coordinates. Chart 
		/// relative coordinates are used by default.
		/// <para>
		/// To use axes coordinates, anchor 
		/// an annotation to a data point using the <see cref="AnchorDataPoint"/> property, or 
		/// set the annotation axes using the <see cref="AxisX"/> or <see cref="AxisY"/> properties.
		/// </para>
		/// <para>
		/// Set the X position to Double.NaN ("NotSet") to achieve automatic position calculation 
		/// when the annotation is anchored using the <see cref="AnchorDataPoint"/> property or 
		/// the <see cref="AnchorX"/> and <see cref="AnchorY"/> properties.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(double.NaN),
		SRDescription("DescriptionAttributeAnnotationBaseX"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		TypeConverter(typeof(DoubleNanValueConverter)),
		]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
        virtual public double X
		{
			get
			{
				return _x;
			}
			set
			{
				_x = value;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the Y coordinate of an annotation.
		/// <seealso cref="AnchorDataPoint"/>
		/// <seealso cref="AnchorY"/>
		/// </summary>
		/// <value>
        /// A Double value that represents the Y coordinate of an annotation.
		/// </value>
		/// <remarks>
		/// The Y coordinate of an annotation is in relative chart coordinates or axes coordinates. Chart 
		/// relative coordinates are used by default.
		/// <para>
		/// To use axes coordinates, anchor 
		/// an annotation to a data point using the <see cref="AnchorDataPoint"/> property, or 
		/// set the annotation axes using the <see cref="AxisX"/> or <see cref="AxisY"/> properties.
		/// </para>
		/// <para>
		/// Set the Y position to Double.NaN ("NotSet") to achieve automatic position calculation 
		/// when the annotation is anchored using the <see cref="AnchorDataPoint"/> property or 
		/// the <see cref="AnchorX"/> and <see cref="AnchorY"/> properties.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(double.NaN),
		SRDescription("DescriptionAttributeAnnotationBaseY"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		TypeConverter(typeof(DoubleNanValueConverter)),
		]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
		virtual public double Y
		{
			get
			{
				return _y;
			}
			set
			{
				_y = value;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets an annotation's width.
		/// <seealso cref="IsSizeAlwaysRelative"/>
		/// <seealso cref="AxisX"/>
		/// </summary>
		/// <value>
        /// A Double value that represents an annotation's width.
		/// </value>
		/// <remarks>
		/// An annotation's width can be a negative value, in which case the annotation orientation 
		/// is switched.
		/// <para>
		/// Annotation width can be in relative chart or axes coordinates. Chart 
		/// relative coordinates are used by default.
		/// </para>
		/// <para>
		/// To use axes coordinates, anchor 
		/// an annotation to a data point using the <see cref="AnchorDataPoint"/> property, or 
		/// set the annotation axes using the <see cref="AxisX"/> or <see cref="AxisY"/> properties 
		/// and set the <see cref="IsSizeAlwaysRelative"/> property to <b>false</b>.
		/// </para>
		/// <para>
		/// Set the width to Double.NaN ("NotSet") to achieve automatic size calculation for 
		/// annotations with text. The size will automatically be calculated based on 
		/// the annotation text and font size.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(double.NaN),
		SRDescription("DescriptionAttributeAnnotationWidth"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		TypeConverter(typeof(DoubleNanValueConverter)),
		]
		virtual public double Width
		{
			get
			{
				return _width;
			}
			set
			{
                if (value < -WidthHightLimit || value > WidthHightLimit)
                {
                    throw new ArgumentException(SR.ExceptionValueMustBeInRange("Width", (-WidthHightLimit).ToString(CultureInfo.CurrentCulture), WidthHightLimit.ToString(CultureInfo.CurrentCulture)));
                }
                _width = value;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets an annotation's height.
		/// <seealso cref="IsSizeAlwaysRelative"/>
		/// <seealso cref="AxisY"/>
		/// </summary>
		/// <value>
		/// A Double value that represents an annotation's height.
		/// </value>
		/// <remarks>
		/// An annotation's height can be a negative value, in which case the annotation orientation 
		/// is switched.
		/// <para>
		/// Annotation height can be in relative chart or axes coordinates. Chart 
		/// relative coordinates are used by default.
		/// </para>
		/// <para>
		/// To use axes coordinates, anchor 
		/// an annotation to a data point using the <see cref="AnchorDataPoint"/> property, or 
		/// set the annotation axes using the <see cref="AxisX"/> or <see cref="AxisY"/> properties 
		/// and set the <see cref="IsSizeAlwaysRelative"/> property to <b>false</b>.
		/// </para>
		/// <para>
		/// Set the height to Double.NaN ("NotSet") to achieve automatic size calculation for 
		/// annotations with text. The size will automatically be calculated based on 
		/// the annotation text and font size.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(double.NaN),
		SRDescription("DescriptionAttributeAnnotationHeight"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		TypeConverter(typeof(DoubleNanValueConverter)),
		]
		virtual public double Height
		{
			get
			{
				return _height;
			}
			set
			{
                if (value < -WidthHightLimit || value > WidthHightLimit)
                {
                    throw new ArgumentException(SR.ExceptionValueMustBeInRange("Height", (-WidthHightLimit).ToString(CultureInfo.CurrentCulture), WidthHightLimit.ToString(CultureInfo.CurrentCulture)));
                }
                _height = value;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets an annotation position's right boundary.
		/// <seealso cref="IsSizeAlwaysRelative"/>
		/// <seealso cref="AxisX"/>
		/// </summary>
		/// <value>
		/// A Double value that represents the position of an annotation's right boundary.
		/// </value>
		/// <remarks>
		/// To use axes coordinates, anchor 
		/// an annotation to a data point using the <see cref="AnchorDataPoint"/> property, or 
		/// set the annotation axes using the <see cref="AxisX"/> or <see cref="AxisY"/> properties 
		/// and set the <see cref="IsSizeAlwaysRelative"/> property to <b>false</b>.
		/// </remarks>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(double.NaN),
		SRDescription("DescriptionAttributeRight3"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		Browsable(false),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		TypeConverter(typeof(DoubleNanValueConverter)),
		]
		virtual public double Right
		{
			get
			{
				return _x + _width;
			}
			set
			{
				_width = value - _x;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets an annotation position's bottom boundary.
		/// <seealso cref="IsSizeAlwaysRelative"/>
		/// <seealso cref="AxisX"/>
		/// </summary>
		/// <value>
		/// A Double value that represents the position of an annotation's bottom boundary.
		/// </value>
		/// <remarks>
		/// To use axes coordinates, anchor 
		/// an annotation to a data point using the <see cref="AnchorDataPoint"/> property, or 
		/// set the annotation axes using the <see cref="AxisX"/> or <see cref="AxisY"/> properties 
		/// and set the <see cref="IsSizeAlwaysRelative"/> property to <b>false</b>.
		/// </remarks>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(double.NaN),
		SRDescription("DescriptionAttributeBottom"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		Browsable(false),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		TypeConverter(typeof(DoubleNanValueConverter)),
		]
		virtual public double Bottom
		{
			get
			{
				return _y + _height;
			}
			set
			{
				_height = value - _y;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		#endregion

		#region Visual Attributes

#if Microsoft_CONTROL
        /// <summary>
		/// Gets or sets a flag that determines if an annotation is selected.
		/// <seealso cref="AllowSelecting"/>
		/// </summary>
		/// <value>
		/// <b>True</b> if the annotation is selected, <b>false</b> otherwise.
		/// </value>
#else
        /// <summary>
		/// Gets or sets a flag that determines if an annotation is selected.
		/// </summary>
		/// <value>
		/// <b>True</b> if the annotation is selected, <b>false</b> otherwise.
		/// </value>
#endif // Microsoft_CONTROL
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(false),
		Browsable(false),
		SRDescription("DescriptionAttributeSelected"),
		]
		virtual public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				_isSelected = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets an annotation selection points style.
		/// </summary>
		/// <value>
		/// A <see cref="SelectionPointsStyle"/> value that represents annotation
		/// selection style.
		/// </value>
		/// <remarks>
        /// This property is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(SelectionPointsStyle.Rectangle),
		ParenthesizePropertyNameAttribute(true),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		SRDescription("DescriptionAttributeSelectionPointsStyle"),
		]
		virtual internal SelectionPointsStyle SelectionPointsStyle
		{
			get
			{
				return SelectionPointsStyle.Rectangle;
			}
		}

		/// <summary>
		/// Gets or sets a flag that specifies whether an annotation is visible.
		/// </summary>
		/// <value>
		/// <b>True</b> if the annotation is visible, <b>false</b> otherwise.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(true),
		SRDescription("DescriptionAttributeVisible"),
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
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets an annotation's content alignment.
		/// </summary>
		/// <value>
		/// A <see cref="ContentAlignment"/> value that represents the content alignment.
		/// </value>
		/// <remarks>
		/// This property is used to align text for <see cref="TextAnnotation"/>, <see cref="RectangleAnnotation"/>,  
		/// <see cref="EllipseAnnotation"/> and <see cref="CalloutAnnotation"/> objects, and to align 
		/// a non-scaled image inside an <see cref="ImageAnnotation"/> object.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(typeof(ContentAlignment), "MiddleCenter"),
		SRDescription("DescriptionAttributeAlignment"),
		]
		virtual public ContentAlignment Alignment
		{
			get
			{
				return _alignment;
			}
			set
			{
				_alignment = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the text color of an annotation.
		/// <seealso cref="Font"/>
		/// </summary>
		/// <value>
		/// A <see cref="Color"/> value used for the text color of an annotation.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(typeof(Color), "Black"),
        SRDescription("DescriptionAttributeForeColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		virtual public Color ForeColor
		{
			get
			{
				return _foreColor;
			}
			set
			{
				_foreColor = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the font of an annotation's text.
		/// <seealso cref="ForeColor"/>
		/// </summary>
		/// <value>
		/// A <see cref="Font"/> object used for an annotation's text.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(typeof(Font), "Microsoft Sans Serif, 8pt"),
		SRDescription("DescriptionAttributeTextFont"),
		]
		virtual public Font Font
		{
			get
			{
				return _textFont;
			}
			set
			{
				_textFont = value;
				this.Invalidate(); 
			}
		}

        /// <summary>
        /// Gets or sets an annotation's text style.
        /// <seealso cref="Font"/>
        /// <seealso cref="ForeColor"/>
        /// </summary>
        /// <value>
        /// A <see cref="TextStyle"/> value used to draw an annotation's text.
        /// </value>
        [
        SRCategory("CategoryAttributeAppearance"),
        DefaultValue(typeof(TextStyle), "Default"),
        SRDescription("DescriptionAttributeTextStyle"),
        ]
        virtual public TextStyle TextStyle
        {
            get
            {
                return _textStyle;
            }
            set
            {
                _textStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of an annotation line.
        /// <seealso cref="LineWidth"/>
        /// <seealso cref="LineDashStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used to draw an annotation line.
        /// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(typeof(Color), "Black"),
        SRDescription("DescriptionAttributeLineColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		virtual public Color LineColor
		{
			get
			{
				return lineColor;
			}
			set
			{
				lineColor = value;
				Invalidate();
			}
		}

        /// <summary>
        /// Gets or sets the width of an annotation line.
        /// <seealso cref="LineColor"/>
        /// <seealso cref="LineDashStyle"/>
        /// </summary>
        /// <value>
        /// An integer value defining the width of an annotation line in pixels.
        /// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(1),
        SRDescription("DescriptionAttributeLineWidth"),
		]
		virtual public int LineWidth
		{
			get
			{
				return _lineWidth;
			}
			set
			{
				if(value < 0)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAnnotationLineWidthIsNegative));
				}
				_lineWidth = value;
				Invalidate();
			}
		}

        /// <summary>
        /// Gets or sets the style of an annotation line.
        /// <seealso cref="LineWidth"/>
        /// <seealso cref="LineColor"/>
        /// </summary>
        /// <value>
        /// A <see cref="ChartDashStyle"/> value used to draw an annotation line.
        /// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(ChartDashStyle.Solid),
        SRDescription("DescriptionAttributeLineDashStyle"),
		]
		virtual public ChartDashStyle LineDashStyle
		{
			get
			{
				return _lineDashStyle;
			}
			set
			{
				_lineDashStyle = value;
				Invalidate();
			}
		}

        /// <summary>
        /// Gets or sets the background color of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the background of an annotation.
        /// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(typeof(Color), ""),
        SRDescription("DescriptionAttributeBackColor"),
		NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		virtual public Color BackColor
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
        /// Gets or sets the background hatch style of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="ChartHatchStyle"/> value used for the background of an annotation.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the hatching, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(ChartHatchStyle.None),
		NotifyParentPropertyAttribute(true),
		SRDescription("DescriptionAttributeBackHatchStyle"),
		Editor(Editors.HatchStyleEditor.Editor, Editors.HatchStyleEditor.Base)
		]
		virtual public ChartHatchStyle BackHatchStyle
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
        /// Gets or sets the background gradient style of an annotation.
        /// <seealso cref="BackSecondaryColor"/>
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="GradientStyle"/> value used for the background of an annotation.
        /// </value>
        /// <remarks>
        /// Two colors are used to draw the gradient, <see cref="BackColor"/> and <see cref="BackSecondaryColor"/>.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(GradientStyle.None),
		NotifyParentPropertyAttribute(true),
       	SRDescription("DescriptionAttributeBackGradientStyle"),
		Editor(Editors.GradientEditor.Editor, Editors.GradientEditor.Base)
		]		
		virtual public GradientStyle BackGradientStyle
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
        /// Gets or sets the secondary background color of an annotation.
        /// <seealso cref="BackColor"/>
        /// <seealso cref="BackHatchStyle"/>
        /// <seealso cref="BackGradientStyle"/>
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> value used for the secondary color of an annotation background with 
        /// hatching or gradient fill.
        /// </value>
        /// <remarks>
        /// This color is used with <see cref="BackColor"/> when <see cref="BackHatchStyle"/> or
        /// <see cref="BackGradientStyle"/> are used.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeBackSecondaryColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		] 
		virtual public Color BackSecondaryColor
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
		/// Gets or sets the color of an annotation's shadow.
		/// <seealso cref="ShadowOffset"/>
		/// </summary>
		/// <value>
		/// A <see cref="Color"/> value used to draw an annotation's shadow.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(typeof(Color), "128,0,0,0"),
        SRDescription("DescriptionAttributeShadowColor"),
        TypeConverter(typeof(ColorConverter)), 
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
		]
		virtual public Color ShadowColor
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
		/// Gets or sets the offset between an annotation and its shadow.
		/// <seealso cref="ShadowColor"/>
		/// </summary>
		/// <value>
		/// An integer value that represents the offset between an annotation and its shadow.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(0),
        SRDescription("DescriptionAttributeShadowOffset"),
		]
		virtual public int ShadowOffset
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

		#endregion

		#region Axes Attaching

		/// <summary>
		/// Gets or sets the name of the X axis which an annotation is attached to.
		/// </summary>
		/// <value>
		/// A string value that represents the name of the X axis which an annotation
		/// is attached to.
		/// </value>
		/// <remarks>
        /// This property is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchorAxes"),
		DefaultValue(""),
		Browsable(false),
		Bindable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		SRDescription("DescriptionAttributeAxisXName"),
		]
		virtual public string  AxisXName
		{
			get
			{
				if(_axisXName.Length == 0 && _axisX != null)
				{
					_axisXName = GetAxisName(_axisX);
				}
				return _axisXName;
			}
			set
			{
				_axisXName = value;
				_axisX = null;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}		

		/// <summary>
        /// Gets or sets the name of the Y axis which an annotation is attached to.
		/// </summary>
		/// <value>
		/// A string value that represents the name of the Y axis which an annotation
		/// is attached to.
		/// </value>
		/// <remarks>
        /// This property is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchorAxes"),
		Browsable(false),
		Bindable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DefaultValue(""),
		SRDescription("DescriptionAttributeAxisYName"),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		]
		virtual public string  AxisYName
		{
			get
			{
				//  Always return empty string to prevent property serialization
				// "YAxisName" property will be used instead.
				return string.Empty;
			}
			set
			{
				this.YAxisName = value;
			}
		}		


		/// <summary>
        /// Gets or sets the name of the Y axis which an annotation is attached to.
		/// NOTE: "AxisYName" property was used before but the name was changed to solve the
		/// duplicated hash value during the serialization with the "TitleSeparator" property. 
		/// </summary>
		/// <value>
		/// A string value that represents the name of the Y axis which an annotation
		/// is attached to.
		/// </value>
		/// <remarks>
        /// This property is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchorAxes"),
		Browsable(false),
		Bindable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DefaultValue(""),
		SRDescription("DescriptionAttributeAxisYName"),
		]
		virtual public string  YAxisName
		{
			get
			{
				if(_axisYName.Length == 0 && _axisY != null)
				{
					_axisYName = GetAxisName(_axisY);
				}
				return _axisYName;
			}
			set
			{
				_axisYName = value;
				_axisY = null;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}		

		/// <summary>
		/// Gets or sets the X axis which an annotation is attached to.
		/// <seealso cref="AxisY"/>
		/// <seealso cref="IsSizeAlwaysRelative"/>
		/// </summary>
		/// <value>
		/// <see cref="Axis"/> object which an annotation is attached to.
		/// </value>
		/// <remarks>
		/// When an annotation is attached to an axis, its X position is always in 
		/// axis coordinates. To define an annotation's size in axis coordinates as well, 
		/// make sure the <see cref="IsSizeAlwaysRelative"/> property is set to <b>false</b>.
		/// <para>
		/// Set this value to <b>null</b> or <b>nothing</b> to disable attachment to the axis.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchorAxes"),
		DefaultValue(null),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		SRDescription("DescriptionAttributeAxisX"),
		Editor(Editors.AnnotationAxisUITypeEditor.Editor, Editors.AnnotationAxisUITypeEditor.Base),
		TypeConverter(typeof(AnnotationAxisValueConverter)),
		]
		virtual public Axis AxisX
		{
			get
			{
				if(_axisX == null && _axisXName.Length > 0)
				{
					_axisX = GetAxisByName(_axisXName);
				}
				return _axisX;
			}
			set
			{
				_axisX = value;
				_axisXName = String.Empty;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}		

		/// <summary>
		/// Gets or sets the Y axis which an annotation is attached to.
		/// <seealso cref="AxisX"/>
		/// <seealso cref="IsSizeAlwaysRelative"/>
		/// </summary>
		/// <value>
		/// <see cref="Axis"/> object which an annotation is attached to.
		/// </value>
		/// <remarks>
		/// When an annotation is attached to an axis, its Y position is always in 
		/// axis coordinates. To define an annotation's size in axis coordinates as well, 
		/// make sure <see cref="IsSizeAlwaysRelative"/> property is set to <b>false</b>.
		/// <para>
		/// Set this value to <b>null</b> or <b>nothing</b> to disable annotation attachment to an axis.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchorAxes"),
		DefaultValue(null),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		SRDescription("DescriptionAttributeAxisY"),
		Editor(Editors.AnnotationAxisUITypeEditor.Editor, Editors.AnnotationAxisUITypeEditor.Base),
		TypeConverter(typeof(AnnotationAxisValueConverter)),
		]
		virtual public Axis AxisY
		{
			get
			{
				if(_axisY == null && _axisYName.Length > 0)
				{
					_axisY = GetAxisByName(_axisYName);
				}
				return _axisY;
			}
			set
			{
				_axisY = value;
				_axisYName = String.Empty;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}		

		#endregion

		#region Anchor

		/// <summary>
		/// Gets or sets the name of a data point which an annotation is anchored to.
		/// </summary>
		/// <value>
		/// A string value that represents the name of the data point which an 
		/// annotation is anchored to.
		/// </value>
		/// <remarks>
        /// This property is for internal use and is hidden at design and run time.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchor"),
		Browsable(false),
		Bindable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DefaultValue(""),
		SRDescription("DescriptionAttributeAnchorDataPointName"),
		]
		virtual public string  AnchorDataPointName
		{
			get
			{
				if(_anchorDataPointName.Length == 0 && _anchorDataPoint != null)
				{
					_anchorDataPointName = GetDataPointName(_anchorDataPoint);
				}
				return _anchorDataPointName;
			}
			set
			{
				_anchorDataPointName = value;
				_anchorDataPoint = null;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}		

		/// <summary>
		/// Gets or sets the data point an annotation is anchored to.
		/// <seealso cref="AnchorAlignment"/>
		/// <seealso cref="AnchorOffsetX"/>
		/// <seealso cref="AnchorOffsetY"/>
		/// <seealso cref="AnchorX"/>
		/// <seealso cref="AnchorY"/>
        /// <seealso cref="SetAnchor(Charting.DataPoint)"/>
        /// <seealso cref="SetAnchor(Charting.DataPoint, Charting.DataPoint)"/>
		/// </summary>
		/// <value>
		/// A <see cref="DataPoint"/> object an annotation is anchored to.
		/// </value>
		/// <remarks>
		/// The annotation is anchored to the X and Y values of the specified data point, 
		/// and automatically uses the same axes coordinates as the data point.
		/// <para>
		/// To automatically position an annotation relative to an anchor point, make sure 
		/// its <see cref="X"/> and <see cref="Y"/> properties are set to <b>Double.NaN</b>.
		/// The <see cref="AnchorAlignment"/> property may be used to change an annotation's 
		/// automatic position alignment to an anchor point. The <see cref="AnchorOffsetX"/> and 
		/// <see cref="AnchorOffsetY"/> properties may be used to add extra spacing.
		/// </para>
		/// <para>
		/// When using this property, make sure the <see cref="AnchorX"/> and <see cref="AnchorY"/> 
		/// properties are set to <b>Double.NaN</b> (they have precedence).
		/// </para>
		/// <para>
		/// Set this value to <b>null</b> or <b>nothing</b> to disable annotation anchoring to a data point.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchor"),
		DefaultValue(null),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		SerializationVisibilityAttribute(SerializationVisibility.Hidden),
		SRDescription("DescriptionAttributeAnchorDataPoint"),
		Editor(Editors.AnchorPointUITypeEditor.Editor, Editors.AnchorPointUITypeEditor.Base),
		TypeConverter(typeof(AnchorPointValueConverter)),
		]
		virtual public DataPoint AnchorDataPoint
		{
			get
			{
				if(_anchorDataPoint == null && _anchorDataPointName.Length > 0)
				{
					_anchorDataPoint = GetDataPointByName(_anchorDataPointName);
				}
				return _anchorDataPoint;
			}
			set
			{
				_anchorDataPoint = value;
				_anchorDataPointName = String.Empty;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the X coordinate the annotation is anchored to.
		/// <seealso cref="AnchorY"/>
		/// <seealso cref="AnchorOffsetX"/>
		/// <seealso cref="AnchorOffsetY"/>
		/// <seealso cref="AnchorAlignment"/>
		/// <seealso cref="AnchorDataPoint"/>
		/// </summary>
		/// <value>
		/// A double value that represents the X coordinate which an annotation is anchored to.
		/// </value>
		/// <remarks>
		/// The annotation is anchored to the X coordinate specified in relative or axis coordinates, 
		/// depending on the <see cref="AxisX"/> property value.
		/// <para>
		/// To automatically position an annotation relative to an anchor point, make sure 
		/// its <see cref="X"/> property is set to <b>Double.NaN</b>.
		/// The <see cref="AnchorAlignment"/> property may be used to change the annotation's 
		/// automatic position alignment to the anchor point. The <see cref="AnchorOffsetX"/> and 
		/// <see cref="AnchorOffsetY"/> properties may be used to add extra spacing.
		/// </para>
		/// <para>
		/// This property has a higher priority than the <see cref="AnchorDataPoint"/> property.
		/// </para>
		/// <para>
		/// Set this value to <b>Double.NaN</b> to disable annotation anchoring to the value.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchor"),
		DefaultValue(double.NaN),
		SRDescription("DescriptionAttributeAnchorX"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		TypeConverter(typeof(DoubleNanValueConverter)),
		]
		virtual public double AnchorX
		{
			get
			{
				return _anchorX;
			}
			set
			{
				_anchorX = value;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the Y coordinate which an annotation is anchored to.
		/// <seealso cref="AnchorY"/>
		/// <seealso cref="AnchorOffsetX"/>
		/// <seealso cref="AnchorOffsetY"/>
		/// <seealso cref="AnchorAlignment"/>
		/// <seealso cref="AnchorDataPoint"/>
		/// </summary>
		/// <value>
		/// A double value that represents the Y coordinate which an annotation is anchored to.
		/// </value>
		/// <remarks>
		/// The annotation is anchored to the Y coordinate specified in relative or axis coordinates, 
		/// depending on the <see cref="AxisX"/> property value.
		/// <para>
		/// To automatically position an annotation relative to an anchor point, make sure 
		/// its <see cref="Y"/> property is set to <b>Double.NaN</b>.
		/// The <see cref="AnchorAlignment"/> property may be used to change the annotation's 
		/// automatic position alignment to the anchor point. The <see cref="AnchorOffsetX"/> and 
		/// <see cref="AnchorOffsetY"/> properties may be used to add extra spacing.
		/// </para>
		/// <para>
		/// This property has a higher priority than the <see cref="AnchorDataPoint"/> property.
		/// </para>
		/// <para>
		/// Set this value to <b>Double.NaN</b> to disable annotation anchoring to the value.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchor"),
		DefaultValue(double.NaN),
		SRDescription("DescriptionAttributeAnchorY"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		TypeConverter(typeof(DoubleNanValueConverter)),
		]
		virtual public double AnchorY
		{
			get
			{
				return _anchorY;
			}
			set
			{
				_anchorY = value;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the x-coordinate offset between the positions of an annotation and its anchor point.
		/// <seealso cref="AnchorOffsetY"/>
		/// <seealso cref="AnchorDataPoint"/>
		/// <seealso cref="AnchorX"/>
		/// <seealso cref="AnchorAlignment"/>
		/// </summary>
		/// <value>
        /// A double value that represents the x-coordinate offset between the positions of an annotation and its anchor point.
		/// </value>
		/// <remarks>
		/// The annotation must be anchored using the <see cref="AnchorDataPoint"/> or 
		/// <see cref="AnchorX"/> properties, and its <see cref="X"/> property must be set 
		/// to <b>Double.NaN</b>.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchor"),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeAnchorOffsetX3"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		]
		virtual public double AnchorOffsetX
		{
			get
			{
				return anchorOffsetX;
			}
			set
			{
				if(value > 100.0 || value < -100.0)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAnnotationAnchorOffsetInvalid));
				}
				anchorOffsetX = value;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

        /// <summary>
        /// Gets or sets the y-coordinate offset between the positions of an annotation and its anchor point.
        /// <seealso cref="Annotation.AnchorOffsetX"/>
        /// <seealso cref="Annotation.AnchorDataPoint"/>
        /// <seealso cref="Annotation.AnchorY"/>
        /// <seealso cref="Annotation.AnchorAlignment"/>
        /// </summary>
        /// <value>
        /// A double value that represents the y-coordinate offset between the positions of an annotation and its anchor point.
        /// </value>
        /// <remarks>
        /// Annotation must be anchored using <see cref="Annotation.AnchorDataPoint"/> or 
        /// <see cref="Annotation.AnchorY"/> properties and it's <see cref="Annotation.Y"/> property must be set
        /// to <b>Double.NaN</b>.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAnchor"),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeAnchorOffsetY3"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		]
		virtual public double AnchorOffsetY
		{
			get
			{
				return anchorOffsetY;
			}
			set
			{
				if(value > 100.0 || value < -100.0)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAnnotationAnchorOffsetInvalid));
				}
				anchorOffsetY = value;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets an annotation position's alignment to the anchor point.
		/// <seealso cref="AnchorX"/>
		/// <seealso cref="AnchorY"/>
		/// <seealso cref="AnchorDataPoint"/>
		/// <seealso cref="AnchorOffsetX"/>
		/// <seealso cref="AnchorOffsetY"/>
		/// </summary>
		/// <value>
		/// A <see cref="ContentAlignment"/> value that represents the annotation's alignment to 
		/// the anchor point.
		/// </value>
		/// <remarks>
		/// The annotation must be anchored using either <see cref="AnchorDataPoint"/>, or the <see cref="AnchorX"/> 
		/// and <see cref="AnchorY"/> properties. Its <see cref="X"/> and <see cref="Y"/> 
		/// properties must be set to <b>Double.NaN</b>.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchor"),
		DefaultValue(typeof(ContentAlignment), "BottomCenter"),
        SRDescription("DescriptionAttributeAnchorAlignment"),
		]
		virtual public ContentAlignment AnchorAlignment
		{
			get
			{
				return anchorAlignment;
			}
			set
			{
				anchorAlignment = value;
				this.ResetCurrentRelativePosition();
				Invalidate();
			}
		}

		#endregion	// Anchoring

		#region Editing Permissions

#if Microsoft_CONTROL

		/// <summary>
		/// Gets or sets a flag that specifies whether an annotation may be selected 
		/// with a mouse by the end user.
		/// </summary>
		/// <value>
		/// <b>True</b> if the annotation may be selected, <b>false</b> otherwise.
		/// </value>
		[
        SRCategory("CategoryAttributeEditing"),
        DefaultValue(false),
		SRDescription("DescriptionAttributeAllowSelecting"),
		]
		virtual public bool AllowSelecting
		{
			get
			{
				return _allowSelecting;
			}
			set
			{
				_allowSelecting = value;
			}
		}

		/// <summary>
		/// Gets or sets a flag that specifies whether an annotation may be moved 
		/// with a mouse by the end user.
		/// </summary>
		/// <value>
		/// <b>True</b> if the annotation may be moved, <b>false</b> otherwise.
		/// </value>
		[
        SRCategory("CategoryAttributeEditing"),
        DefaultValue(false),
		SRDescription("DescriptionAttributeAllowMoving"),
		]
		virtual public bool AllowMoving
		{
			get
			{
				return _allowMoving;
			}
			set
			{
				_allowMoving = value;
			}
		}

		/// <summary>
		/// Gets or sets a flag that specifies whether an annotation anchor may be moved 
		/// with a mouse by the end user.
		/// </summary>
		/// <value>
		/// <b>True</b> if the annotation anchor may be moved, <b>false</b> otherwise.
		/// </value>
		[
		SRCategory("CategoryAttributeEditing"),
		DefaultValue(false),
		SRDescription("DescriptionAttributeAllowAnchorMoving3"),
		]
		virtual public bool AllowAnchorMoving
		{
			get
			{
				return _allowAnchorMoving;
			}
			set
			{
				_allowAnchorMoving = value;
			}
		}		

		/// <summary>
		/// Gets or sets a flag that specifies whether an annotation may be resized 
		/// with a mouse by the end user.
		/// </summary>
		/// <value>
		/// <b>True</b> if the annotation may be resized, <b>false</b> otherwise.
		/// </value>
		[
        SRCategory("CategoryAttributeEditing"),
        DefaultValue(false),
		SRDescription("DescriptionAttributeAllowResizing"),
		]
		virtual public bool AllowResizing
		{
			get
			{
				return _allowResizing;
			}
			set
			{
				_allowResizing = value;
			}
		}

		/// <summary>
		/// Gets or sets a flag that specifies whether an annotation's text may be edited 
		/// when the end user double clicks on the text.
		/// </summary>
		/// <value>
		/// <b>True</b> if the annotation text may be edited, <b>false</b> otherwise.
		/// </value>
		[
        SRCategory("CategoryAttributeEditing"),
        DefaultValue(false),
		SRDescription("DescriptionAttributeAllowTextEditing"),
		]
		virtual public bool AllowTextEditing
		{
			get
			{
				return _allowTextEditing;
			}
			set
			{
				_allowTextEditing = value;
			}
		}

		/// <summary>
		/// Gets or sets a flag that specifies whether a polygon annotation's points 
		/// may be moved with a mouse by the end user.
		/// </summary>
		/// <value>
		/// <b>True</b> if the polygon annotation's points may be moved, <b>false</b> otherwise.
		/// </value>
		[
        SRCategory("CategoryAttributeEditing"),
        DefaultValue(false),
		SRDescription("DescriptionAttributeAllowPathEditing3"),
		]
		virtual public bool AllowPathEditing
		{
			get
			{
				return _allowPathEditing;
			}
			set
			{
				_allowPathEditing = value;
			}
		}
		
#endif // Microsoft_CONTROL

		#endregion

		#region Interactivity

		/// <summary>
		/// Gets or sets an annotation's tooltip text.
		/// </summary>
		/// <value>
		/// A string value.
		/// </value>
		/// <remarks>
		/// Special keywords can be used in the text when an annotation is anchored to 
		/// a data point using the <see cref="AnchorDataPoint"/> property.  For a listing of 
		/// these keywords, refer to the "Annotations" help topic.
		/// </remarks>
		[

        SRCategory("CategoryAttributeMisc"),
		DefaultValue(""),
        SRDescription("DescriptionAttributeToolTip"),
		]
		virtual public string  ToolTip
		{
			get
			{
				return _tooltip;
			}
			set
			{
				_tooltip = value;

			}
		}

#if !Microsoft_CONTROL

        /// <summary>
		/// Gets or sets an annotation's Url.
		/// </summary>
		/// <value>
		/// A string value.
		/// </value>
		/// <remarks>
		/// Special keywords can be used when an annotation is anchored to 
		/// a data point using the <see cref="AnchorDataPoint"/> property.  For a listing of 
		/// these keywords, refer to the "Annotations" help topic.
		/// </remarks>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(""),
		SRDescription("DescriptionAttributeUrl"),
		]
		virtual public string  Url
		{
			get
			{
				return _url;
			}
			set
			{
				_url = value;

			}
		}	

		/// <summary>
		/// Gets or sets an annotation's map area attributes.
		/// </summary>
		/// <value>
		/// A string value.
		/// </value>
		/// <remarks>
		/// This string will be added to the attributes of the image map generated
		/// for the annotation.
		/// <para>
		/// Special keywords can be used when an annotation is anchored to 
		/// a data point using the <see cref="AnchorDataPoint"/> property.  For a listing of 
		/// these keywords, refer to the "Annotations" help topic.
		/// </para>
		/// </remarks>
		[
		SRCategory("CategoryAttributeMisc"),
		DefaultValue(""),
		SRDescription("DescriptionAttributeMapAreaAttributes"),
		]
		virtual public string  MapAreaAttributes
		{
			get
			{
				return _mapAreaAttributes;
			}
			set
			{
				_mapAreaAttributes = value;

			}
		}

        /// <summary>
        /// Gets or sets the postback value which can be processed on click event.
        /// </summary>
        /// <value>The value which is passed to click event as argument.</value>
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


#endif // !Microsoft_CONTROL

        #endregion // Interactivity

        #endregion

        #region Methods

        #region Painting

        /// <summary>
		/// Paints the annotation object using the specified graphics.
		/// </summary>
		/// <param name="graphics">
		/// A <see cref="ChartGraphics"/> object used to paint the annotation object.
		/// </param>
		/// <param name="chart">
		/// Reference to the annotation's <see cref="Chart"/> control owner.
		/// </param>
        abstract internal void Paint(Chart chart, ChartGraphics graphics);

		/// <summary>
		/// Paints annotation selection markers.
		/// </summary>
		/// <param name="chartGraphics">Chart graphics used for painting.</param>
		/// <param name="rect">Selection rectangle.</param>
		/// <param name="path">Selection path.</param>
		virtual internal void PaintSelectionHandles(ChartGraphics chartGraphics, RectangleF rect, GraphicsPath path)
		{
			// Define markers appearance style
			Color	markerBorderColor = Color.Black;
			Color	markerColor = Color.FromArgb(200, 255, 255, 255);
            MarkerStyle markerStyle = MarkerStyle.Square;
            int markerSize = selectionMarkerSize;
            Boolean selected = this.IsSelected;

            SizeF markerSizeRel = chartGraphics.GetRelativeSize(new SizeF(markerSize, markerSize));
            if (this.Common.ProcessModePaint &&
				!this.Common.ChartPicture.isPrinting)
			{
				// Clear selection rectangles
				this.selectionRects = null;

				// Check if annotation is selected
                if (selected)
				{
					// Create selection rectangles
					this.selectionRects = new RectangleF[9];

					// Draw selection handles for single dimension annotations like line.
					if(this.SelectionPointsStyle == SelectionPointsStyle.TwoPoints)
					{
						// Save selection handles position in array elements 0 and 4
						this.selectionRects[(int)ResizingMode.TopLeftHandle] = new RectangleF(
							rect.X - markerSizeRel.Width/2f,
							rect.Y - markerSizeRel.Height/2f,
							markerSizeRel.Width,
							markerSizeRel.Height);
						this.selectionRects[(int)ResizingMode.BottomRightHandle] = new RectangleF(
							rect.Right - markerSizeRel.Width/2f,
							rect.Bottom - markerSizeRel.Height/2f,
							markerSizeRel.Width,
							markerSizeRel.Height);


						// Draw selection handle
						chartGraphics.DrawMarkerRel(
							rect.Location,
                            markerStyle,
							markerSize,
							markerColor,
							markerBorderColor,
							1,
							"",
							Color.Empty,
							0,
							Color.FromArgb(128, 0, 0, 0),
							RectangleF.Empty);

						chartGraphics.DrawMarkerRel(
							new PointF(rect.Right, rect.Bottom),
                            markerStyle,
							markerSize,
							markerColor,
							markerBorderColor,
							1,
							"",
							Color.Empty,
							0,
							Color.FromArgb(128, 0, 0, 0),
							RectangleF.Empty);
					}
					else if(this.SelectionPointsStyle == SelectionPointsStyle.Rectangle)
					{
						for(int index = 0; index < 8; index++)
						{
							// Get handle position
							PointF	handlePosition = PointF.Empty;
							switch((ResizingMode)index)
							{
								case ResizingMode.TopLeftHandle:
									handlePosition = rect.Location;
									break;
								case ResizingMode.TopHandle:
									handlePosition = new PointF(rect.X + rect.Width / 2f, rect.Y);
									break;
								case ResizingMode.TopRightHandle:
									handlePosition = new PointF(rect.Right, rect.Y);
									break;
								case ResizingMode.RightHandle:
									handlePosition = new PointF(rect.Right, rect.Y + rect.Height / 2f);
									break;
								case ResizingMode.BottomRightHandle:
									handlePosition = new PointF(rect.Right, rect.Bottom);
									break;
								case ResizingMode.BottomHandle:
									handlePosition = new PointF(rect.X + rect.Width / 2f, rect.Bottom);
									break;
								case ResizingMode.BottomLeftHandle:
									handlePosition = new PointF(rect.X, rect.Bottom);
									break;
								case ResizingMode.LeftHandle:
									handlePosition = new PointF(rect.X, rect.Y + rect.Height / 2f);
									break;
							}

							// Save selection handles position in array elements 0 and 4
							this.selectionRects[index] = new RectangleF(
								handlePosition.X - markerSizeRel.Width/2f,
								handlePosition.Y - markerSizeRel.Height/2f,
								markerSizeRel.Width,
								markerSizeRel.Height);

							// Draw selection handle
							chartGraphics.DrawMarkerRel(
								handlePosition,
                                markerStyle,
								markerSize,
								markerColor,
								markerBorderColor,
								1,
								"",
								Color.Empty,
								0,
								Color.FromArgb(128, 0, 0, 0),
								RectangleF.Empty);
						}
					}


					//********************************************************************
					//** Draw anchor selection handle
					//********************************************************************

					// Get vertical and horizontal axis
					Axis	vertAxis = null;
					Axis	horizAxis = null;
					GetAxes(ref vertAxis, ref horizAxis);

					// Get anchor position
					double	anchorX = double.NaN;
					double	anchorY = double.NaN;
					bool	relativeX = false;
					bool	relativeY = false;
					this.GetAnchorLocation(ref anchorX, ref anchorY, ref relativeX, ref relativeY);

					// Convert anchor location to relative coordinates
					if(!double.IsNaN(anchorX) && !double.IsNaN(anchorY))
					{
						if( !relativeX && horizAxis != null )
						{
							anchorX = horizAxis.ValueToPosition(anchorX);
						}
						if( !relativeY && vertAxis != null )
						{
							anchorY = vertAxis.ValueToPosition(anchorY);
						}

						// Apply 3D transforamtion if required
						ChartArea	chartArea = null;
						if(horizAxis != null && horizAxis.ChartArea != null)
						{
							chartArea = horizAxis.ChartArea;
						}
						if(vertAxis != null && vertAxis.ChartArea != null)
						{
							chartArea = vertAxis.ChartArea;
						}
						if(chartArea != null && 
							chartArea.Area3DStyle.Enable3D == true &&
							!chartArea.chartAreaIsCurcular &&
                            chartArea.requireAxes &&
							chartArea.matrix3D.IsInitialized())
						{
                            // Get anotation Z coordinate (use scene depth or anchored point Z position)
                            float positionZ = chartArea.areaSceneDepth;
                            if (this.AnchorDataPoint != null && this.AnchorDataPoint.series != null)
                            {
                                float depth = 0f;
                                chartArea.GetSeriesZPositionAndDepth(
                                    this.AnchorDataPoint.series,
                                    out depth,
                                    out positionZ);
                                positionZ += depth / 2f;
                            }

                            // Define 3D points of annotation object
                            Point3D[] annot3DPoints = new Point3D[1];
                            annot3DPoints[0] = new Point3D((float)anchorX, (float)anchorY, positionZ);

                            // Tranform cube coordinates
                            chartArea.matrix3D.TransformPoints(annot3DPoints);

                            // Get transformed coordinates
                            anchorX = annot3DPoints[0].X;
                            anchorY = annot3DPoints[0].Y;
						}

						// Save selection handles position in array elements 0 and 4
						this.selectionRects[(int)ResizingMode.AnchorHandle] = new RectangleF(
							(float)anchorX - markerSizeRel.Width/2f,
							(float)anchorY - markerSizeRel.Height/2f,
							markerSizeRel.Width,
							markerSizeRel.Height);

						// Draw circular selection handle
						chartGraphics.DrawMarkerRel(
							new PointF((float)anchorX, (float)anchorY),
							MarkerStyle.Cross,
							selectionMarkerSize + 3,
							markerColor,
							markerBorderColor,
							1,
							"",
							Color.Empty,
							0,
							Color.FromArgb(128, 0, 0, 0),
							RectangleF.Empty);
					}

#if Microsoft_CONTROL

					//********************************************************************
					//** Draw path selection handles
					//********************************************************************
					if(path != null && AllowPathEditing)
					{
						// Create selection rectangles for each point
						PointF[]	pathPoints = path.PathPoints;
						RectangleF[] newSelectionRects = new RectangleF[pathPoints.Length + 9];

						// Copy previous rectangles (first nine elements)
						for(int index = 0; index < this.selectionRects.Length; index++)
						{
							newSelectionRects[index] = this.selectionRects[index];
						}
						this.selectionRects = newSelectionRects;

						// Loop through all points
						for(int index = 0; index < pathPoints.Length; index++)
						{
							// Get handle position
							PointF	handlePosition = chartGraphics.GetRelativePoint(pathPoints[index]);

							// Save selection handles position in array elements 0 and 4
							this.selectionRects[9 + index] = new RectangleF(
								handlePosition.X - markerSizeRel.Width/2f,
								handlePosition.Y - markerSizeRel.Height/2f,
								markerSizeRel.Width,
								markerSizeRel.Height);

							// Draw selection handle
							chartGraphics.DrawMarkerRel(
								handlePosition,
								MarkerStyle.Circle,
								selectionMarkerSize + 1,
								markerColor,
								markerBorderColor,
								1,
								"",
								Color.Empty,
								0,
								Color.FromArgb(128, 0, 0, 0),
								RectangleF.Empty);
						}
					}

#endif // Microsoft_CONTROL

				}
			}
		}

		#endregion

		#region Position and Size

		/// <summary>
		/// Resizes an annotation according to its content size.
		/// </summary>
		/// <remarks>
		/// Sets the annotation width and height to fit the specified text. This method applies to 
		/// <see cref="TextAnnotation"/>, <see cref="RectangleAnnotation"/>, <see cref="EllipseAnnotation"/>
		/// and <see cref="CalloutAnnotation"/> objects only.
		/// </remarks>
		virtual public void ResizeToContent()
		{
			RectangleF position = GetContentPosition();
			if(!double.IsNaN(position.Width))
			{
				this.Width = position.Width;
			}
			if(!double.IsNaN(position.Height))
			{
				this.Height = position.Height;
			}
		}

		/// <summary>
		/// Gets an annotation's content position.
		/// </summary>
		/// <returns>Annotation's content size.</returns>
		virtual internal RectangleF GetContentPosition()
		{
			return new RectangleF(float.NaN, float.NaN, float.NaN, float.NaN);
		}

		/// <summary>
		/// Gets an annotation's anchor point location.
		/// </summary>
		/// <param name="anchorX">Returns the anchor X coordinate.</param>
		/// <param name="anchorY">Returns the anchor Y coordinate.</param>
		/// <param name="inRelativeAnchorX">Indicates if X coordinate is in relative chart coordinates.</param>
		/// <param name="inRelativeAnchorY">Indicates if Y coordinate is in relative chart coordinates.</param>
		private void GetAnchorLocation(ref double anchorX, ref double anchorY, ref bool inRelativeAnchorX, ref bool inRelativeAnchorY)
		{
			anchorX = this.AnchorX;
			anchorY = this.AnchorY;

			if(this.AnchorDataPoint != null &&
				this.AnchorDataPoint.series != null &&
				this.Chart != null &&
				this.Chart.chartPicture != null)
			{
				// Anchor data point is not allowed for gropped annotations
				if(this.AnnotationGroup != null)
				{
                    throw (new InvalidOperationException(SR.ExceptionAnnotationGroupedAnchorDataPointMustBeEmpty));
				}

				// Get data point relative coordinate
				if( double.IsNaN(anchorX) || double.IsNaN(anchorY))
				{
					// Get X value from data point
					if( double.IsNaN(anchorX) )
					{
						anchorX = this.AnchorDataPoint.positionRel.X;
						inRelativeAnchorX = true;
					}

					// Get Y value from data point
					if( double.IsNaN(anchorY) )
					{
						anchorY = this.AnchorDataPoint.positionRel.Y;
						inRelativeAnchorY = true;
					}
				}
			}
		}
	
		/// <summary>
		/// Gets annotation object position in relative coordinates.
		/// </summary>
		/// <param name="location">Returns annotation location.</param>
		/// <param name="size">Returns annotation size.</param>
		/// <param name="anchorLocation">Returns annotation anchor point location.</param>
		virtual internal void GetRelativePosition(out PointF location, out SizeF size, out PointF anchorLocation)
		{
			bool	saveCurrentPosition = true;

			//***********************************************************************
			//** Check if position was precalculated
			//***********************************************************************
			if(!double.IsNaN(currentPositionRel.X) && !double.IsNaN(currentPositionRel.X))
			{
				location = currentPositionRel.Location;
				size = currentPositionRel.Size;
				anchorLocation = currentAnchorLocationRel;
				return;
			}

			//***********************************************************************
			//** Get vertical and horizontal axis
			//***********************************************************************
			Axis	vertAxis = null;
			Axis	horizAxis = null;
			GetAxes(ref vertAxis, ref horizAxis);

			//***********************************************************************
			//** Check if annotation was anchored to 2 points.
			//***********************************************************************
			if(this._anchorDataPoint != null &&
				this._anchorDataPoint2 != null)
			{
				// Annotation size is in axis coordinates
				this.IsSizeAlwaysRelative = false;
 
				// Set annotation size
				this.Height = 
					vertAxis.PositionToValue(this._anchorDataPoint2.positionRel.Y, false) - 
					vertAxis.PositionToValue(this._anchorDataPoint.positionRel.Y, false);
				this.Width = 
					horizAxis.PositionToValue(this._anchorDataPoint2.positionRel.X, false) - 
					horizAxis.PositionToValue(this._anchorDataPoint.positionRel.X, false);

				// Reset second anchor point after setting width and height
				this._anchorDataPoint2 = null;
			}

			//***********************************************************************
			//** Flags which indicate that coordinate was already transformed 
			//** into chart relative coordinate system.
			//***********************************************************************
			bool	inRelativeX = false;
			bool	inRelativeY = false;
			bool	inRelativeWidth = (_isSizeAlwaysRelative) ? true : false;
			bool	inRelativeHeight = (_isSizeAlwaysRelative) ? true : false;
			bool	inRelativeAnchorX = false;
			bool	inRelativeAnchorY = false;
			
			//***********************************************************************
			//** Get anchoring coordinates from anchored Data Point.
			//***********************************************************************
			double	anchorX = this.AnchorX;
			double	anchorY = this.AnchorY;
			GetAnchorLocation(ref anchorX, ref anchorY, ref inRelativeAnchorX, ref inRelativeAnchorY);

			//***********************************************************************
			//** Calculate scaling and translation for the annotations in the group.
			//***********************************************************************
			AnnotationGroup group = this.AnnotationGroup;
			PointF groupLocation = PointF.Empty;
			double groupScaleX = 1.0;
			double groupScaleY = 1.0;
			if(group != null)
			{
				// Do not save relative position of annotations inside the group
				saveCurrentPosition = false;

				// Take relative position of the group
				SizeF groupSize = SizeF.Empty;
				PointF groupAnchorLocation = PointF.Empty;
				group.GetRelativePosition(out groupLocation, out groupSize, out groupAnchorLocation);

				// Calculate Scale
				groupScaleX = groupSize.Width / 100.0;
				groupScaleY = groupSize.Height / 100.0;
			}


			//***********************************************************************
			//** Get annotation automatic size.
			//***********************************************************************
			double	relativeWidth = this._width;
			double	relativeHeight = this._height;

			// Get annotation content position
			RectangleF	contentPosition = GetContentPosition();

			// Set annotation size if not set to custom value
			if( double.IsNaN(relativeWidth) )
			{
				relativeWidth = contentPosition.Width;
				inRelativeWidth = true;
			}
			else
			{
				relativeWidth *= groupScaleX;
			}
			if( double.IsNaN(relativeHeight) )
			{
				relativeHeight = contentPosition.Height;
				inRelativeHeight = true;
			}
			else
			{
				relativeHeight *= groupScaleY;   
			}

			//***********************************************************************
			//** Provide "dummy" size at design time
			//***********************************************************************
			if(this.Chart != null && this.Chart.IsDesignMode())
			{
				if(this.IsSizeAlwaysRelative ||
					(vertAxis == null && horizAxis == null) )
				{
					if(double.IsNaN(relativeWidth))
					{
						relativeWidth = 20.0;
						saveCurrentPosition = false;
					}
					if(double.IsNaN(relativeHeight))
					{
						relativeHeight = 20.0;
						saveCurrentPosition = false;
					}
				}
			}

			//***********************************************************************
			//** Get annotation location.
			//***********************************************************************
			double	relativeX = this.X;
			double	relativeY = this.Y;
			
			// Check if annotation location Y coordinate is defined
			if( double.IsNaN(relativeY) && !double.IsNaN(anchorY) )
			{
				inRelativeY = true;
				double	relativeAnchorY = anchorY;
				if(!inRelativeAnchorY && vertAxis != null)
				{
					relativeAnchorY = vertAxis.ValueToPosition(anchorY);
				}
				if(this.AnchorAlignment == ContentAlignment.TopCenter ||
					this.AnchorAlignment == ContentAlignment.TopLeft ||
					this.AnchorAlignment == ContentAlignment.TopRight)
				{
					relativeY = relativeAnchorY + this.AnchorOffsetY;
					relativeY *= groupScaleY;
				}
				else if(this.AnchorAlignment == ContentAlignment.BottomCenter ||
					this.AnchorAlignment == ContentAlignment.BottomLeft ||
					this.AnchorAlignment == ContentAlignment.BottomRight)
				{
					relativeY = relativeAnchorY - this.AnchorOffsetY;
					relativeY *= groupScaleY;
					if(relativeHeight != 0f && !double.IsNaN(relativeHeight))
					{
						if(inRelativeHeight)
						{
							relativeY -= relativeHeight;
						}
						else if(vertAxis != null)
						{
							float yValue = (float)vertAxis.PositionToValue(relativeY);
							float bottomRel = (float)vertAxis.ValueToPosition(yValue + relativeHeight);
							relativeY -= bottomRel - relativeY;
						}
					}
				}
				else 
				{
					relativeY = relativeAnchorY + this.AnchorOffsetY;
					relativeY *= groupScaleY;
					if(relativeHeight != 0f && !double.IsNaN(relativeHeight))
					{
						if(inRelativeHeight)
						{
							relativeY -= relativeHeight/2f;
						}
						else if(vertAxis != null)
						{
							float yValue = (float)vertAxis.PositionToValue(relativeY);
							float bottomRel = (float)vertAxis.ValueToPosition(yValue + relativeHeight);
							relativeY -= (bottomRel - relativeY) / 2f;
						}
					}
				}
			}
			else
			{
				relativeY *= groupScaleY;
			}

			// Check if annotation location X coordinate is defined
			if( double.IsNaN(relativeX) && !double.IsNaN(anchorX) )
			{
				inRelativeX = true;
				double	relativeAnchorX = anchorX;
				if(!inRelativeAnchorX && horizAxis != null)
				{
					relativeAnchorX = horizAxis.ValueToPosition(anchorX);
				}
				if(this.AnchorAlignment == ContentAlignment.BottomLeft ||
					this.AnchorAlignment == ContentAlignment.MiddleLeft ||
					this.AnchorAlignment == ContentAlignment.TopLeft)
				{
					relativeX = relativeAnchorX + this.AnchorOffsetX;
					relativeX *= groupScaleX;
				}
				else if(this.AnchorAlignment == ContentAlignment.BottomRight ||
					this.AnchorAlignment == ContentAlignment.MiddleRight ||
					this.AnchorAlignment == ContentAlignment.TopRight)
				{
					relativeX = relativeAnchorX - this.AnchorOffsetX;
					relativeX *= groupScaleX;
					if(relativeWidth != 0f && !double.IsNaN(relativeWidth))
					{
						if(inRelativeWidth)
						{
							relativeX -= relativeWidth;
						}
						else if(horizAxis != null)
						{
							float xValue = (float)horizAxis.PositionToValue(relativeX);
							relativeX -= horizAxis.ValueToPosition(xValue + relativeWidth) - relativeX;
						}
					}
				}
				else 
				{
					relativeX = relativeAnchorX + this.AnchorOffsetX;
					relativeX *= groupScaleX;
					if(relativeWidth != 0f && !double.IsNaN(relativeWidth))
					{
						if(inRelativeWidth)
						{
							relativeX -= relativeWidth/2f;
						}
						else if(horizAxis != null)
						{
							float xValue = (float)horizAxis.PositionToValue(relativeX);
							relativeX -= (horizAxis.ValueToPosition(xValue + relativeWidth) - relativeX) / 2f;
						}
					}
				}
			}
			else
			{
				relativeX *= groupScaleX;
			}

			// Translate
			relativeX += groupLocation.X;
			relativeY += groupLocation.Y;

			//***********************************************************************
			//** Get annotation automatic location.
			//***********************************************************************

			// Set annotation size if not set to custom value
			if( double.IsNaN(relativeX) )
			{
				relativeX = contentPosition.X * groupScaleX;
				inRelativeX = true;
			}
			if( double.IsNaN(relativeY) )
			{
				relativeY = contentPosition.Y * groupScaleY;
				inRelativeY = true;
			}

			//***********************************************************************
			//** Convert coordinates from axes values to relative coordinates.
			//***********************************************************************

			// Check if values are set in axis values
			if(horizAxis != null)
			{
				if(!inRelativeX)
				{
					relativeX = horizAxis.ValueToPosition(relativeX);
				}
				if(!inRelativeAnchorX)
				{
					anchorX = horizAxis.ValueToPosition(anchorX);
				}
				if(!inRelativeWidth)
				{
					relativeWidth = horizAxis.ValueToPosition(
						horizAxis.PositionToValue(relativeX, false) + relativeWidth) - relativeX;
				}
			}
			if(vertAxis != null)
			{
				if(!inRelativeY)
				{
					relativeY = vertAxis.ValueToPosition(relativeY);
				}
				if(!inRelativeAnchorY)
				{
					anchorY = vertAxis.ValueToPosition(anchorY);
				}
				if(!inRelativeHeight)
				{
					relativeHeight = vertAxis.ValueToPosition(
						vertAxis.PositionToValue(relativeY, false) + relativeHeight) - relativeY;
				}
			}
            bool isTextAnnotation = this is TextAnnotation;
			//***********************************************************************
			//** Apply 3D transforamtion if required
			//***********************************************************************
			ChartArea	chartArea = null;
			if(horizAxis != null && horizAxis.ChartArea != null)
			{
				chartArea = horizAxis.ChartArea;
			}
			if(vertAxis != null && vertAxis.ChartArea != null)
			{
				chartArea = vertAxis.ChartArea;
			}
			if(chartArea != null && 
				chartArea.Area3DStyle.Enable3D == true &&
				!chartArea.chartAreaIsCurcular &&
                chartArea.requireAxes &&
				chartArea.matrix3D.IsInitialized())
			{
				// Get anotation Z coordinate (use scene depth or anchored point Z position)
				float			positionZ = chartArea.areaSceneDepth;	
				if(this.AnchorDataPoint != null && this.AnchorDataPoint.series != null)
				{
					float depth = 0f;
					chartArea.GetSeriesZPositionAndDepth(
						this.AnchorDataPoint.series,
						out depth,
						out positionZ);
					positionZ += depth / 2f;
				}

				// Define 3D points of annotation object
				Point3D[]		annot3DPoints = new Point3D[3];
				annot3DPoints[0] = new Point3D((float)relativeX, (float)relativeY, positionZ);
				annot3DPoints[1] = new Point3D((float)(relativeX + relativeWidth), (float)(relativeY + relativeHeight), positionZ);
				annot3DPoints[2] = new Point3D((float)anchorX, (float)anchorY, positionZ);

				// Tranform cube coordinates
				chartArea.matrix3D.TransformPoints( annot3DPoints );

				// Get transformed coordinates
				relativeX = annot3DPoints[0].X;
				relativeY = annot3DPoints[0].Y;
				anchorX = annot3DPoints[2].X;
				anchorY = annot3DPoints[2].Y;
				
				// Don't adjust size for text annotation
                if (!(isTextAnnotation && this.IsSizeAlwaysRelative))
				{
					relativeWidth = annot3DPoints[1].X - relativeX;
					relativeHeight = annot3DPoints[1].Y - relativeY;
				}
			}

			//***********************************************************************
			//** Provide "dummy" position at design time
			//***********************************************************************
			if(this.Chart != null && this.Chart.IsDesignMode())
			{
				if(double.IsNaN(relativeX))
				{
					relativeX = groupLocation.X;
					saveCurrentPosition = false;
				}
				if(double.IsNaN(relativeY))
				{
					relativeY = groupLocation.Y;
					saveCurrentPosition = false;
				}
				if(double.IsNaN(relativeWidth))
				{
					relativeWidth = 20.0 * groupScaleX;
					saveCurrentPosition = false;
				}
				if(double.IsNaN(relativeHeight))
				{
					relativeHeight = 20.0 * groupScaleY;
					saveCurrentPosition = false;
				}
			}

			//***********************************************************************
			//** Initialize returned values
			//***********************************************************************
			location = new PointF( (float)relativeX, (float)relativeY );
			size = new SizeF( (float)relativeWidth, (float)relativeHeight );
			anchorLocation = new PointF( (float)anchorX, (float)anchorY );

			//***********************************************************************
			//** Adjust text based annotaion position using SmartLabelStyle.
			//***********************************************************************
			// Check if smart labels are enabled
            if (this.SmartLabelStyle.Enabled && isTextAnnotation &&
				group == null)
			{
				// Anchor point must be set
				if(!double.IsNaN(anchorX) && !double.IsNaN(anchorY) &&
					double.IsNaN(this.X) && double.IsNaN(this.Y))
				{
					if(this.Chart != null && 
						this.Chart.chartPicture != null)
					{
						// Remember old movement distance restriction
						double oldMinMovingDistance = this.SmartLabelStyle.MinMovingDistance;
						double oldMaxMovingDistance = this.SmartLabelStyle.MaxMovingDistance;

						// Increase annotation moving restrictions according to the anchor offset
						PointF anchorOffsetAbs = this.GetGraphics().GetAbsolutePoint(
							new PointF((float)this.AnchorOffsetX, (float)this.AnchorOffsetY));
						float maxAnchorOffsetAbs = Math.Max(anchorOffsetAbs.X, anchorOffsetAbs.Y);
						if(maxAnchorOffsetAbs > 0.0)
						{
							this.SmartLabelStyle.MinMovingDistance += maxAnchorOffsetAbs;
							this.SmartLabelStyle.MaxMovingDistance += maxAnchorOffsetAbs;
						}

						// Adjust label position using SmartLabelStyle algorithm
						LabelAlignmentStyles	labelAlignment = LabelAlignmentStyles.Bottom;
                        using (StringFormat format = new StringFormat())
                        {
                            SizeF markerSizeRel = new SizeF((float)this.AnchorOffsetX, (float)this.AnchorOffsetY);
                            PointF newlocation = this.Chart.chartPicture.annotationSmartLabel.AdjustSmartLabelPosition(
                                this.Common,
                                this.Chart.chartPicture.ChartGraph,
                                chartArea,
                                this.SmartLabelStyle,
                                location,
                                size,
                                format,
                                anchorLocation,
                                markerSizeRel,
                                labelAlignment,
                                (this is CalloutAnnotation));

                            // Restore old movement distance restriction
                            this.SmartLabelStyle.MinMovingDistance = oldMinMovingDistance;
                            this.SmartLabelStyle.MaxMovingDistance = oldMaxMovingDistance;

                            // Check if annotation should be hidden
                            if (newlocation.IsEmpty)
                            {
                                location = new PointF(float.NaN, float.NaN);
                            }
                            else
                            {
                                // Get new position using alignment in format
                                RectangleF newPosition = this.Chart.chartPicture.annotationSmartLabel.GetLabelPosition(
                                    this.Chart.chartPicture.ChartGraph,
                                    newlocation,
                                    size,
                                    format,
                                    false);

                                // Set new location
                                location = newPosition.Location;
                            }
                        }
					}
				}
				else
				{
					// Add annotation position into the list (to prevent overlapping)
                    using (StringFormat format = new StringFormat())
                    {
                        this.Chart.chartPicture.annotationSmartLabel.AddSmartLabelPosition(
                            this.Chart.chartPicture.ChartGraph,
                            location,
                            size,
                            format);
                    }
				}
			}

			//***********************************************************************
			//** Save calculated position
			//***********************************************************************
			if(saveCurrentPosition)
			{
				currentPositionRel = new RectangleF(location, size);
				currentAnchorLocationRel = new PointF(anchorLocation.X, anchorLocation.Y);
			}
		}

#if Microsoft_CONTROL
		/// <summary>
		/// Set annotation object position using rectangle in relative coordinates.
		/// Automatically converts relative coordinates to axes values if required.
		/// </summary>
		/// <param name="position">Position in relative coordinates.</param>
		/// <param name="anchorPoint">Anchor location in relative coordinates.</param>
		internal void SetPositionRelative(RectangleF position, PointF anchorPoint)
		{
			SetPositionRelative(position, anchorPoint, false);
		}
#endif // Microsoft_CONTROL

		/// <summary>
		/// Set annotation object position using rectangle in relative coordinates.
		/// Automatically converts relative coordinates to axes values if required.
		/// </summary>
		/// <param name="position">Position in relative coordinates.</param>
		/// <param name="anchorPoint">Anchor location in relative coordinates.</param>
		/// <param name="userInput">Indicates if position changing was a result of the user input.</param>
		internal void SetPositionRelative(RectangleF position, PointF anchorPoint, bool userInput)
		{
			double	newX = position.X;
			double	newY = position.Y;
			double	newRight = position.Right;
			double	newBottom = position.Bottom;
			double	newWidth = position.Width;
			double	newHeight = position.Height;
			double	newAnchorX = anchorPoint.X;
			double	newAnchorY = anchorPoint.Y;

			//***********************************************************************
			//** Set pre calculated position and anchor location
			//***********************************************************************
			this.currentPositionRel = new RectangleF(position.Location, position.Size);
			this.currentAnchorLocationRel = new PointF(anchorPoint.X, anchorPoint.Y);

			//***********************************************************************
			//** Get vertical and horizontal axis
			//***********************************************************************
			Axis	vertAxis = null;
			Axis	horizAxis = null;
			GetAxes(ref vertAxis, ref horizAxis);

			//***********************************************************************
			//** Disable anchoring to point and axes in 3D
			//** This is done due to the issues of moving elements in 3D space.
			//***********************************************************************
			ChartArea	chartArea = null;
			if(horizAxis != null && horizAxis.ChartArea != null)
			{
				chartArea = horizAxis.ChartArea;
			}
			if(vertAxis != null && vertAxis.ChartArea != null)
			{
				chartArea = vertAxis.ChartArea;
			}
			if(chartArea != null && chartArea.Area3DStyle.Enable3D == true)
			{
				// If anchor point was set - get its relative position and use it as an anchor point
				if(this.AnchorDataPoint != null)
				{
					bool	inRelativeCoordX = true;
					bool	inRelativeCoordY = true;
					this.GetAnchorLocation(ref newAnchorX, ref newAnchorY, ref inRelativeCoordX, ref inRelativeCoordY);
					this.currentAnchorLocationRel = new PointF((float)newAnchorX, (float)newAnchorY);
				}

				// In 3D always use relative annotation coordinates
				// Disconnect annotation from axes and anchor point
				this.AnchorDataPoint = null;
				this.AxisX = null;
				this.AxisY = null;
				horizAxis = null;
				vertAxis = null;
			}


			//***********************************************************************
			//** Convert relative coordinates to axis values
			//***********************************************************************
			if(horizAxis != null)
			{
				newX = horizAxis.PositionToValue(newX, false);
				if(!double.IsNaN(newAnchorX))
				{
					newAnchorX = horizAxis.PositionToValue(newAnchorX, false);
				}

				// Adjust for the IsLogarithmic axis
				if( horizAxis.IsLogarithmic ) 
				{
					newX = Math.Pow( horizAxis.logarithmBase, newX );
					if(!double.IsNaN(newAnchorX))
					{
						newAnchorX = Math.Pow( horizAxis.logarithmBase, newAnchorX );
					}
				}

				if(!this.IsSizeAlwaysRelative)
				{
					if(float.IsNaN(position.Right) && 
						!float.IsNaN(position.Width) && 
						!float.IsNaN(anchorPoint.X) )
					{
						newRight = horizAxis.PositionToValue(anchorPoint.X + position.Width, false);
						if( horizAxis.IsLogarithmic ) 
						{
							newRight = Math.Pow( horizAxis.logarithmBase, newRight );
						}
						newWidth = newRight - newAnchorX;
					}
					else
					{
						newRight = horizAxis.PositionToValue(position.Right, false);
						if( horizAxis.IsLogarithmic ) 
						{
							newRight = Math.Pow( horizAxis.logarithmBase, newRight );
						}
						newWidth = newRight - newX;
					}
				}
			}
			if(vertAxis != null)
			{
				newY = vertAxis.PositionToValue(newY, false);
				if(!double.IsNaN(newAnchorY))
				{
					newAnchorY = vertAxis.PositionToValue(newAnchorY, false);
				}

				// NOTE: Fixes issue #4113
				// Adjust for the IsLogarithmic axis
				if( vertAxis.IsLogarithmic ) 
				{
					newY = Math.Pow( vertAxis.logarithmBase, newY );
					if(!double.IsNaN(newAnchorY))
					{
						newAnchorY = Math.Pow( vertAxis.logarithmBase, newAnchorY );
					}
				}

				if(!this.IsSizeAlwaysRelative)
				{
					if(float.IsNaN(position.Bottom) && 
						!float.IsNaN(position.Height) && 
						!float.IsNaN(anchorPoint.Y) )
					{
						newBottom = vertAxis.PositionToValue(anchorPoint.Y + position.Height, false);
						if( vertAxis.IsLogarithmic ) 
						{
							newBottom = Math.Pow( vertAxis.logarithmBase, newBottom );
						}
						newHeight = newBottom - newAnchorY;
					}
					else
					{
						newBottom = vertAxis.PositionToValue(position.Bottom, false);
						if( vertAxis.IsLogarithmic ) 
						{
							newBottom = Math.Pow( vertAxis.logarithmBase, newBottom );
						}
						newHeight = newBottom - newY;
					}
				}
			}

			// Fire position changing event when position changed by user.
			if(userInput)
			{
#if Microsoft_CONTROL
				// Set flag that annotation position was changed
				this.positionChanged = true;

				// Fire position changing event
				if(this.Chart != null)
				{
					AnnotationPositionChangingEventArgs args = new AnnotationPositionChangingEventArgs();
					args.NewLocationX = newX;
					args.NewLocationY = newY;
					args.NewSizeWidth = newWidth;
					args.NewSizeHeight = newHeight;
					args.NewAnchorLocationX = newAnchorX;
					args.NewAnchorLocationY = newAnchorY;
					args.Annotation = this;

					if(this.Chart.OnAnnotationPositionChanging(ref args))
					{
						// Get user changed position/anchor
						newX = args.NewLocationX;
						newY = args.NewLocationY;
						newWidth = args.NewSizeWidth;
						newHeight = args.NewSizeHeight;
						newAnchorX = args.NewAnchorLocationX;
						newAnchorY = args.NewAnchorLocationY;
					}
				}
#endif // Microsoft_CONTROL
			}

			// Adjust location & size
			this.X = newX;
			this.Y = newY;
			this.Width = newWidth;
			this.Height = newHeight;
			this.AnchorX = newAnchorX;
			this.AnchorY = newAnchorY;

			// Invalidate annotation
			this.Invalidate();

			return;
		}
        /// <summary>
        /// Adjust annotation location and\or size as a result of user action.
        /// </summary>
        /// <param name="movingDistance">Distance to resize/move the annotation.</param>
        /// <param name="resizeMode">Resizing mode.</param>
		virtual internal void AdjustLocationSize(SizeF movingDistance, ResizingMode resizeMode)
		{
			AdjustLocationSize(movingDistance, resizeMode, true);
		}

        /// <summary>
        /// Adjust annotation location and\or size as a result of user action.
        /// </summary>
        /// <param name="movingDistance">Distance to resize/move the annotation.</param>
        /// <param name="resizeMode">Resizing mode.</param>
        /// <param name="pixelCoord">Distance is in pixels, otherwise relative.</param>
		virtual internal void AdjustLocationSize(SizeF movingDistance, ResizingMode resizeMode, bool pixelCoord)
		{
			AdjustLocationSize(movingDistance, resizeMode, pixelCoord, false);
		}

        /// <summary>
        /// Adjust annotation location and\or size as a result of user action.
        /// </summary>
        /// <param name="movingDistance">Distance to resize/move the annotation.</param>
        /// <param name="resizeMode">Resizing mode.</param>
        /// <param name="pixelCoord">Distance is in pixels, otherwise relative.</param>
        /// <param name="userInput">Indicates if position changing was a result of the user input.</param>
		virtual internal void AdjustLocationSize(SizeF movingDistance, ResizingMode resizeMode, bool pixelCoord, bool userInput)
		{
			if(!movingDistance.IsEmpty)
			{
				// Convert pixel coordinates into relative 
				if(pixelCoord)
				{
					movingDistance = Chart.chartPicture.ChartGraph.GetRelativeSize(movingDistance);
				}

				// Get annotation position in relative coordinates
				PointF firstPoint = PointF.Empty;
				PointF anchorPoint = PointF.Empty;
				SizeF size = SizeF.Empty;
				if(userInput)
				{
#if Microsoft_CONTROL
					if(this.startMovePositionRel.X == 0f &&
						this.startMovePositionRel.Y == 0f &&
						this.startMovePositionRel.Width == 0f &&
						this.startMovePositionRel.Height == 0f)
					{
						GetRelativePosition(out firstPoint, out size, out anchorPoint);
						this.startMovePositionRel = new RectangleF(firstPoint, size);
						this.startMoveAnchorLocationRel = new PointF(anchorPoint.X, anchorPoint.Y);
					}
					firstPoint = this.startMovePositionRel.Location;
					size = this.startMovePositionRel.Size;
					anchorPoint = this.startMoveAnchorLocationRel;
#else // Microsoft_CONTROL
					GetRelativePosition(out firstPoint, out size, out anchorPoint);
#endif // Microsoft_CONTROL
					
				}
				else
				{
					GetRelativePosition(out firstPoint, out size, out anchorPoint);
				}

				if(resizeMode == ResizingMode.TopLeftHandle)
				{
					firstPoint.X -= movingDistance.Width;
					firstPoint.Y -= movingDistance.Height;
					size.Width += movingDistance.Width;
					size.Height += movingDistance.Height;
				}
				else if(resizeMode == ResizingMode.TopHandle)
				{
					firstPoint.Y -= movingDistance.Height;
					size.Height += movingDistance.Height;
				}
				else if(resizeMode == ResizingMode.TopRightHandle)
				{
					firstPoint.Y -= movingDistance.Height;
					size.Width -= movingDistance.Width;
					size.Height += movingDistance.Height;
				}
				else if(resizeMode == ResizingMode.RightHandle)
				{
					size.Width -= movingDistance.Width;
				}
				else if(resizeMode == ResizingMode.BottomRightHandle)
				{
					size.Width -= movingDistance.Width;
					size.Height -= movingDistance.Height;
				}
				else if(resizeMode == ResizingMode.BottomHandle)
				{
					size.Height -= movingDistance.Height;
				}
				else if(resizeMode == ResizingMode.BottomLeftHandle)
				{
					firstPoint.X -= movingDistance.Width;
					size.Width += movingDistance.Width;
					size.Height -= movingDistance.Height;
				}
				else if(resizeMode == ResizingMode.LeftHandle)
				{
					firstPoint.X -= movingDistance.Width;
					size.Width += movingDistance.Width;
				}
				else if(resizeMode == ResizingMode.AnchorHandle)
				{
					anchorPoint.X -= movingDistance.Width;
					anchorPoint.Y -= movingDistance.Height;
				}
				else if(resizeMode == ResizingMode.Moving)
				{
					firstPoint.X -= movingDistance.Width;
					firstPoint.Y -= movingDistance.Height;
				}

				// Make sure we do not override automatic Width and Heigth
				if(resizeMode == ResizingMode.Moving)
				{
					if( double.IsNaN(this.Width) )
					{
						size.Width = float.NaN;
					}
					if( double.IsNaN(this.Height) )
					{
						size.Height = float.NaN;
					}
				}

				// Make sure we do not override automatic X and Y
				if(resizeMode == ResizingMode.AnchorHandle)
				{
					if( double.IsNaN(this.X) )
					{
						firstPoint.X = float.NaN;
					}
					if( double.IsNaN(this.Y) )
					{
						firstPoint.Y = float.NaN;
					}
				}
				else if(double.IsNaN(this.AnchorX) || double.IsNaN(this.AnchorY) )
				{
					anchorPoint = new PointF(float.NaN, float.NaN);
				}

				// Set annotation position from rectangle in relative coordinates
				SetPositionRelative(new RectangleF(firstPoint, size), anchorPoint, userInput);
			}
			return;
		}

		#endregion

		#region Anchor Point and Axes Converters

		/// <summary>
		/// Checks if annotation draw anything in the anchor position (except selection handle)
		/// </summary>
		/// <returns>True if annotation "connects" itself and anchor point visually.</returns>
		virtual internal bool IsAnchorDrawn()
		{
			return false;
		}

		/// <summary>
		/// Gets data point by name.
		/// </summary>
		/// <param name="dataPointName">Data point name to find.</param>
		/// <returns>Data point.</returns>
		internal DataPoint GetDataPointByName(string dataPointName)
		{
			DataPoint dataPoint = null;

            if (Chart != null && dataPointName.Length > 0)
            {
                // Split series name and point index
                int separatorIndex = dataPointName.IndexOf("\\r", StringComparison.Ordinal);
                if (separatorIndex > 0)
                {
                    string seriesName = dataPointName.Substring(0, separatorIndex);
                    string pointIndex = dataPointName.Substring(separatorIndex + 2);

                    int index;
                    if (int.TryParse(pointIndex, NumberStyles.Any, CultureInfo.InvariantCulture, out index))
                    {
                        dataPoint = Chart.Series[seriesName].Points[index];
                    }
                }
            }
        	
			return dataPoint;
		}

		/// <summary>
		/// Gets axis by name.
		/// </summary>
		/// <param name="axisName">Axis name to find.</param>
		/// <returns>Data point.</returns>
		private Axis GetAxisByName(string axisName)
		{
            Debug.Assert(axisName != null, "GetAxisByName: handed a null axis name");

			Axis axis = null;

            try
            {
                if (Chart != null && axisName.Length > 0)
                {
                    // Split series name and point index
                    int separatorIndex = axisName.IndexOf("\\r", StringComparison.Ordinal);
                    if (separatorIndex > 0)
                    {
                        string areaName = axisName.Substring(0, separatorIndex);
                        string axisType = axisName.Substring(separatorIndex + 2);
                        switch ((AxisName)Enum.Parse(typeof(AxisName), axisType))
                        {
                            case (AxisName.X):
                                axis = Chart.ChartAreas[areaName].AxisX;
                                break;
                            case (AxisName.Y):
                                axis = Chart.ChartAreas[areaName].AxisY;
                                break;
                            case (AxisName.X2):
                                axis = Chart.ChartAreas[areaName].AxisX2;
                                break;
                            case (AxisName.Y2):
                                axis = Chart.ChartAreas[areaName].AxisY2;
                                break;
                        }
                    }
                }
            }
            catch (ArgumentNullException)
            {
                axis = null;
            }
            catch (ArgumentException)
            {
                axis = null;
            }
				
			return axis;
		}

		/// <summary>
		/// Gets data point unique name.
		/// </summary>
		/// <param name="dataPoint">Data point to get the name for.</param>
		/// <returns>Data point name.</returns>
		internal string GetDataPointName(DataPoint dataPoint)
		{
			string name = String.Empty;
			if(dataPoint.series != null)
			{
				int pointIndex = dataPoint.series.Points.IndexOf(dataPoint);
				if(pointIndex >= 0)
				{
					name = dataPoint.series.Name + 
						"\\r" + 
						pointIndex.ToString(CultureInfo.InvariantCulture);
				}
			}
			return name;
		}

		/// <summary>
		/// Gets axis unique name.
		/// </summary>
		/// <param name="axis">Axis to get the name for.</param>
		/// <returns>Axis name.</returns>
		private string GetAxisName(Axis axis)
		{
			string name = String.Empty;
			if(axis.ChartArea != null)
			{
				name = axis.ChartArea.Name + 
					"\\r" + 
					axis.AxisName.ToString();
			}
			return name;
		}

		#endregion

		#region Z Order Methods

		/// <summary>
		/// Sends an annotation to the back of all annotations.
		/// <seealso cref="BringToFront"/>
		/// </summary>
		virtual public void SendToBack()
		{
			// Find collection of annotation objects this annotation belongs too
			AnnotationCollection collection = null;
			if(Chart != null)
			{
				collection = Chart.Annotations;
			}

			// Check if annotation belongs to the group
			AnnotationGroup group = AnnotationGroup;
			if(group != null)
			{
				collection = group.Annotations;
			}

			// Check if annotation is found
			if(collection != null)
			{
				Annotation annot = collection.FindByName(this.Name);
				if(annot != null)
				{
					// Reinsert annotation at the beginning of the collection
					collection.Remove(annot);
					collection.Insert(0, annot);
				}
			}
		}

		/// <summary>
		/// Brings an annotation to the front of all annotations.
		/// <seealso cref="SendToBack"/>
		/// </summary>
		virtual public void BringToFront()
		{
			// Find collection of annotation objects this annotation belongs too
			AnnotationCollection collection = null;
			if(Chart != null)
			{
				collection = Chart.Annotations;
			}

			// Check if annotation belongs to the group
			AnnotationGroup group = AnnotationGroup;
			if(group != null)
			{
				collection = group.Annotations;
			}

			// Check if annotation is found
			if(collection != null)
			{
				Annotation annot = collection.FindByName(this.Name);
				if(annot != null)
				{
					// Reinsert annotation at the end of the collection
					collection.Remove(annot);
					collection.Add(this);
				}
			}
		}

		#endregion // Z Order Methods

		#region Group Related Methods

		#endregion // Group Related Methods

		#region SmartLabelStyle methods

		/// <summary>
		/// Adds anchor position to the list. Used to check SmartLabelStyle overlapping.
		/// </summary>
		/// <param name="list">List to add to.</param>
		internal void AddSmartLabelMarkerPositions(ArrayList list)		
		{
			// Anchor position is added to the list of non-overlapped markers
			if(this.Visible && this.IsAnchorDrawn())
			{
				// Get vertical and horizontal axis
				Axis	vertAxis = null;
				Axis	horizAxis = null;
				GetAxes(ref vertAxis, ref horizAxis);

				// Get anchor position
				double	anchorX = double.NaN;
				double	anchorY = double.NaN;
				bool	relativeX = false;
				bool	relativeY = false;
				this.GetAnchorLocation(ref anchorX, ref anchorY, ref relativeX, ref relativeY);

				// Convert anchor location to relative coordinates
				if(!double.IsNaN(anchorX) && !double.IsNaN(anchorY))
				{
					if( !relativeX && horizAxis != null )
					{
						anchorX = horizAxis.ValueToPosition(anchorX);
					}
					if( !relativeY && vertAxis != null )
					{
						anchorY = vertAxis.ValueToPosition(anchorY);
					}

					// Apply 3D transforamtion if required
					ChartArea	chartArea = null;
					if(horizAxis != null && horizAxis.ChartArea != null)
					{
						chartArea = horizAxis.ChartArea;
					}
					if(vertAxis != null && vertAxis.ChartArea != null)
					{
						chartArea = vertAxis.ChartArea;
					}
					if(chartArea != null && 
						chartArea.Area3DStyle.Enable3D == true &&
						!chartArea.chartAreaIsCurcular &&
                        chartArea.requireAxes &&
						chartArea.matrix3D.IsInitialized())
					{
						// Get anotation Z coordinate (use scene depth or anchored point Z position)
						float			positionZ = chartArea.areaSceneDepth;	
						if(this.AnchorDataPoint != null && this.AnchorDataPoint.series != null)
						{
							float depth = 0f;
							chartArea.GetSeriesZPositionAndDepth(
								this.AnchorDataPoint.series,
								out depth,
								out positionZ);
							positionZ += depth / 2f;
						}

						// Define 3D points of annotation object
						Point3D[]		annot3DPoints = new Point3D[1];
						annot3DPoints[0] = new Point3D((float)anchorX, (float)anchorY, positionZ);

						// Tranform cube coordinates
						chartArea.matrix3D.TransformPoints( annot3DPoints );

						// Get transformed coordinates
						anchorX = annot3DPoints[0].X;
						anchorY = annot3DPoints[0].Y;
					}

					// Save selection handles position in array elements 0 and 4
					if(this.GetGraphics() != null)
					{
						SizeF	markerSizeRel = this.GetGraphics().GetRelativeSize(
							new SizeF(1f, 1f));
						RectangleF anchorRect = new RectangleF(
							(float)anchorX - markerSizeRel.Width/2f,
							(float)anchorY - markerSizeRel.Height/2f,
							markerSizeRel.Width,
							markerSizeRel.Height);

						list.Add(anchorRect);
					}
				}
			}
		}

		#endregion

		#region Public Anchoring Methods

		/// <summary>
		/// Anchors an annotation to a data point.
		/// <seealso cref="AnchorDataPoint"/>
		/// <seealso cref="AnchorX"/>
		/// <seealso cref="AnchorY"/>
		/// </summary>
		/// <param name="dataPoint">
		/// <see cref="DataPoint"/> to be anchored to.
		/// </param>
		/// <remarks>
		/// Anchors an annotation to the specified data point.
		/// </remarks>
		public void SetAnchor(DataPoint dataPoint)
		{
			SetAnchor(dataPoint, null);
		}

		/// <summary>
		/// Anchors an annotation to two data points.
		/// <seealso cref="AnchorDataPoint"/>
		/// <seealso cref="AnchorX"/>
		/// <seealso cref="AnchorY"/>
		/// </summary>
		/// <param name="dataPoint1">
		/// First anchor <see cref="DataPoint"/>.
		/// </param>
		/// <param name="dataPoint2">
		/// Second anchor <see cref="DataPoint"/>.
		/// </param>
		/// <remarks>
		/// Anchors an annotation's top/left and bottom/right corners to the 
		/// specified data points.
		/// </remarks>
		public void SetAnchor(DataPoint dataPoint1, DataPoint dataPoint2)
		{
			// Set annotation position to automatic
			this.X = double.NaN;
			this.Y = double.NaN;

			// Reset anchor point if any
			this.AnchorX = double.NaN;
			this.AnchorY = double.NaN;

			// Set anchor point
			this.AnchorDataPoint = dataPoint1;

			// Get vertical and horizontal axis
			Axis	vertAxis = null;
			Axis	horizAxis = null;
			GetAxes(ref vertAxis, ref horizAxis);

			// Set Width and Height in axis coordinates
			if(dataPoint2 != null && dataPoint1 != null)
			{
                this._anchorDataPoint2 = dataPoint2;
			}

			// Invalidate annotation
			this.Invalidate();
		}

		#endregion // Public Anchoring Methods

		#region Placement Methods

#if Microsoft_CONTROL

		/// <summary>
		/// Begins end user placement of an annotation using the mouse.
		/// </summary>
		/// <remarks>
		/// When this method is called, the end user is allowed to place an annotation using the 
		/// mouse.
		/// <para>
		/// Placement will finish when the end user specifies all required points, or 
		/// the <see cref="EndPlacement"/> method is called.</para>
		/// </remarks>
		virtual public void BeginPlacement()
		{
			// Can't place annotations inside the group
			if(this.AnnotationGroup != null)
			{
                throw (new InvalidOperationException(SR.ExceptionAnnotationGroupedUnableToStartPlacement));
			}

			if(this.Chart != null)
			{
				// Set the annotation object which is currently placed 
				this.Chart.Annotations.placingAnnotation = this;
			}
			else
			{
                throw (new InvalidOperationException(SR.ExceptionAnnotationNotInCollection));
			}

		}

		/// <summary>
		/// Ends user placement of an annotation.
		/// </summary>
		/// <remarks>
		/// Ends an annotation placement operation previously started by a 
		/// <see cref="BeginPlacement"/> method call.
		/// <para>
		/// Calling this method is not required, since placement will automatically
		/// end when an end user enters all required points. However, it is useful when an annotation 
		/// placement operation needs to be aborted for some reason.
		/// </para>
		/// </remarks>
		virtual public void EndPlacement()
		{
			if(this.Chart != null)
			{
				// Reset currently placed annotation object
				this.Chart.Annotations.placingAnnotation = null;

				// Restore default cursor
				this.Chart.Cursor = this.Chart.defaultCursor;

				// Clear last placement mouse position
				this.lastPlacementPosition = PointF.Empty;

				// Fire annotation placed event
				this.Chart.OnAnnotationPlaced(this);
			}
		}

		/// <summary>
		/// Handles mouse down event during annotation placement.
		/// </summary>
		/// <param name="point">Mouse cursor position in pixels.</param>
		/// <param name="buttons">Mouse button down.</param>
		internal virtual void PlacementMouseDown(PointF point, MouseButtons buttons)
		{
			if(buttons == MouseButtons.Right)
			{
				// Stop any pacement
				this.EndPlacement();
			}
			if(buttons == MouseButtons.Left &&
				IsValidPlacementPosition(point.X, point.Y))
			{
				if(this.lastPlacementPosition.IsEmpty)
				{
					// Remeber position where mouse was clicked
					this.lastPlacementPosition = this.GetGraphics().GetRelativePoint(point);

					// Get annotation position in relative coordinates
					PointF firstPoint = PointF.Empty;
					PointF anchorPoint = PointF.Empty;
					SizeF size = SizeF.Empty;
					this.GetRelativePosition(out firstPoint, out size, out anchorPoint);

					// Set annotation X, Y coordinate
					if(this.AllowMoving)
					{
						firstPoint = this.GetGraphics().GetRelativePoint(point);

						// Do not change default position
						if(double.IsNaN(this.AnchorX))
						{
							anchorPoint.X = float.NaN;
						}
						if(double.IsNaN(this.AnchorY))
						{
							anchorPoint.Y = float.NaN;
						}

					}
					else if(this.AllowAnchorMoving)
					{
						anchorPoint = this.GetGraphics().GetRelativePoint(point);

						// Do not change default position
						if(double.IsNaN(this.X))
						{
							firstPoint.X = float.NaN;
						}
						if(double.IsNaN(this.Y))
						{
							firstPoint.Y = float.NaN;
						}
					}

					// Do not change default size
					if(double.IsNaN(this.Width))
					{
						size.Width = float.NaN;
					}
					if(double.IsNaN(this.Height))
					{
						size.Height = float.NaN;
					}

					// Set annotation position
					this.positionChanged = true;
					this.SetPositionRelative(
						new RectangleF(firstPoint, size), 
						anchorPoint, 
						true);

					// Invalidate and update the chart
					if(Chart != null)
					{
						Invalidate();
						Chart.UpdateAnnotations();
					}
				}
			}
		}

		/// <summary>
		/// Handles mouse up event during annotation placement.
		/// </summary>
		/// <param name="point">Mouse cursor position in pixels.</param>
		/// <param name="buttons">Mouse button Up.</param>
		/// <returns>Return true when placing finished.</returns>
		internal virtual bool PlacementMouseUp(PointF point, MouseButtons buttons)
		{
			bool result = false;
			if(buttons == MouseButtons.Left)
			{
				// Get annotation position in relative coordinates
				PointF firstPoint = PointF.Empty;
				PointF anchorPoint = PointF.Empty;
				SizeF size = SizeF.Empty;
				this.GetRelativePosition(out firstPoint, out size, out anchorPoint);

				if(this.AllowResizing)
				{
					PointF pointRel = this.GetGraphics().GetRelativePoint(point);
					size = new SizeF(
						pointRel.X - this.lastPlacementPosition.X, 
						pointRel.Y - this.lastPlacementPosition.Y);
				}
				else
				{
					// Do not change default size
					if(double.IsNaN(this.Width))
					{
						size.Width = float.NaN;
					}
					if(double.IsNaN(this.Height))
					{
						size.Height = float.NaN;
					}
				}

				// Do not change default position
				if(double.IsNaN(this.X))
				{
					firstPoint.X = float.NaN;
				}
				if(double.IsNaN(this.Y))
				{
					firstPoint.Y = float.NaN;
				}
				if(double.IsNaN(this.AnchorX))
				{
					anchorPoint.X = float.NaN;
				}
				if(double.IsNaN(this.AnchorY))
				{
					anchorPoint.Y = float.NaN;
				}

				// Set annotation position
				this.positionChanged = true;
				this.SetPositionRelative(
					new RectangleF(firstPoint, size), 
					anchorPoint, 
					true);

				// End placement
				if(!size.IsEmpty || !this.AllowResizing)
				{
					result = true;
					this.EndPlacement();
				}

				// Invalidate and update the chart
				if(Chart != null)
				{
					Invalidate();
					Chart.UpdateAnnotations();
				}
			}

			return result;
		}

		/// <summary>
		/// Handles mouse move event during annotation placement.
		/// </summary>
		/// <param name="point">Mouse cursor position in pixels.</param>
		internal virtual void PlacementMouseMove(PointF point)
		{
			// Check if annotation was moved
			if( this.GetGraphics() != null &&
				!this.lastPlacementPosition.IsEmpty)
			{
				// Get annotation position in relative coordinates
				PointF firstPoint = PointF.Empty;
				PointF anchorPoint = PointF.Empty;
				SizeF size = SizeF.Empty;
				this.GetRelativePosition(out firstPoint, out size, out anchorPoint);

				if(this.AllowResizing)
				{
					PointF pointRel = this.GetGraphics().GetRelativePoint(point);
					size = new SizeF(
						pointRel.X - this.lastPlacementPosition.X, 
						pointRel.Y - this.lastPlacementPosition.Y);
				}

				// Do not change default position
				if(double.IsNaN(this.X))
				{
					firstPoint.X = float.NaN;
				}
				if(double.IsNaN(this.Y))
				{
					firstPoint.Y = float.NaN;
				}
				if(double.IsNaN(this.AnchorX))
				{
					anchorPoint.X = float.NaN;
				}
				if(double.IsNaN(this.AnchorY))
				{
					anchorPoint.Y = float.NaN;
				}

				// Set annotation position
				this.positionChanged = true;
				this.SetPositionRelative(
					new RectangleF(firstPoint, size), 
					anchorPoint, 
					true);

				// Invalidate and update the chart
				if(this.Chart != null)
				{
					Invalidate();
					this.Chart.UpdateAnnotations();
				}
			}
		}

		/// <summary>
		/// Checks if specified position is valid for placement.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <returns>True if annotation can be placed at specified coordinates.</returns>
		virtual internal bool IsValidPlacementPosition(float x, float y)
		{
			if(this.Chart != null &&
				this.GetGraphics() != null)
			{
				// Check if cursor is over the area where placement allowed
				// If so - change cursor to cross
				RectangleF	placementRect = new RectangleF(0f, 0f, 100f, 100f);
				if(this.ClipToChartArea.Length > 0 &&
					this.ClipToChartArea != Constants.NotSetValue)
				{
					ChartArea area = Chart.ChartAreas[this.ClipToChartArea];
					placementRect = area.PlotAreaPosition.ToRectangleF();
				}
				placementRect = this.GetGraphics().GetAbsoluteRectangle(placementRect);
				if(placementRect.Contains(x, y))
				{
					return true;
				}
			}
			return false;
		}

#endif // Microsoft_CONTROL

        #endregion // Placement Methods

        #region Helper Methods

        /// <summary>
		/// Helper method that checks if annotation is visible.
		/// </summary>
		/// <returns>True if annotation is visible.</returns>
		internal bool IsVisible()
		{
			if(this.Visible)
			{
				if(this.Chart != null)
				{
					// Check if annotation is anchored to the data point
					ChartArea area = null;
					if(this.AnchorDataPoint != null && 
						this.AnchorDataPoint.series != null)
					{
						if(this.Chart.ChartAreas.IndexOf(this.AnchorDataPoint.series.ChartArea) >= 0)
						{
							area = this.Chart.ChartAreas[this.AnchorDataPoint.series.ChartArea];
						}
					}
					if(area == null &&
						this._anchorDataPoint2 != null && 
						this._anchorDataPoint2.series != null)
					{
						if(this.Chart.ChartAreas.IndexOf(this._anchorDataPoint2.series.ChartArea) >= 0)
						{
							area = this.Chart.ChartAreas[this._anchorDataPoint2.series.ChartArea];
						}
					}

					// Check if annotation uses chart area axis values
					if(area == null && this.AxisX != null)
					{
						area = this.AxisX.ChartArea;
					}
					if(area == null && this.AxisY != null)
					{
						area = this.AxisY.ChartArea;
					}

					// Check if associated area is visible
					if(area != null &&
						!area.Visible)
					{
						return false;
					}
				}
			
					
				return true;
			}
			return false;
		}

		/// <summary>
		/// Resets pre-calculated annotation position.
		/// </summary>
		internal void ResetCurrentRelativePosition()
		{
			this.currentPositionRel = new RectangleF(float.NaN, float.NaN, float.NaN, float.NaN);
			this.currentAnchorLocationRel = new PointF(float.NaN, float.NaN);
		}

		/// <summary>
		/// Replaces predefined keyword inside the string with their values if
		/// annotation is anchored to the data point.
		/// </summary>
		/// <param name="strOriginal">Original string with keywords.</param>
		/// <returns>Modified string.</returns>
		internal string ReplaceKeywords(string strOriginal)
		{
			if(this.AnchorDataPoint != null)
			{
				return this.AnchorDataPoint.ReplaceKeywords(strOriginal);
			}
			return strOriginal;
		}

		/// <summary>
		/// Checks if anchor point of the annotation is visible.
		/// </summary>
		/// <returns>True if anchor point is visible.</returns>
		internal bool IsAnchorVisible()
		{
			// Get axes objects
			Axis	vertAxis = null;
			Axis	horizAxis = null;
			GetAxes(ref vertAxis, ref horizAxis);

			// Get anchor position
			bool	inRelativeAnchorX = false;
			bool	inRelativeAnchorY = false;
			double	anchorX = this.AnchorX;
			double	anchorY = this.AnchorY;
			GetAnchorLocation(ref anchorX, ref anchorY, ref inRelativeAnchorX, ref inRelativeAnchorY);

			// Check if anchor is set
			if( !double.IsNaN(anchorX) && !double.IsNaN(anchorY) )
			{
				// Check if anchor is in axes coordinates
				if(this.AnchorDataPoint != null || 
					this.AxisX != null ||
					this.AxisY != null)
				{
					// Convert anchor point to relative coordinates
					if(!inRelativeAnchorX && horizAxis != null)
					{
						anchorX = horizAxis.ValueToPosition(anchorX);
					}
					if(!inRelativeAnchorY && vertAxis != null)
					{
						anchorY = vertAxis.ValueToPosition(anchorY);
					}

					// Get chart area
					ChartArea chartArea = null;
					if(horizAxis != null)
					{
						chartArea = horizAxis.ChartArea;
					}
					if(chartArea == null && vertAxis != null)
					{
						chartArea = vertAxis.ChartArea;
					}

					// Apply 3D transforamtion if required
					if(chartArea != null && chartArea.Area3DStyle.Enable3D == true)
					{
						if(!chartArea.chartAreaIsCurcular &&
                            chartArea.requireAxes &&
							chartArea.matrix3D.IsInitialized())
						{
                            // Get anotation Z coordinate (use scene depth or anchored point Z position)
                            float positionZ = chartArea.areaSceneDepth;
                            if (this.AnchorDataPoint != null && this.AnchorDataPoint.series != null)
                            {
                                float depth = 0f;
                                chartArea.GetSeriesZPositionAndDepth(
                                    this.AnchorDataPoint.series,
                                    out depth,
                                    out positionZ);
                                positionZ += depth / 2f;
                            }

                            // Define 3D points of annotation object
                            Point3D[] annot3DPoints = new Point3D[1];
                            annot3DPoints[0] = new Point3D((float)anchorX, (float)anchorY, positionZ);

                            // Tranform cube coordinates
                            chartArea.matrix3D.TransformPoints(annot3DPoints);

                            // Get transformed coordinates
                            anchorX = annot3DPoints[0].X;
                            anchorY = annot3DPoints[0].Y;
						}
					}

					// Get plot rectangle position and inflate it slightly 
					// to solve any float rounding issues.
					RectangleF rect = chartArea.PlotAreaPosition.ToRectangleF();
					rect.Inflate(0.00001f, 0.00001f);

					// Check if anchor point is in the plotting area
					if(!rect.Contains((float)anchorX, (float)anchorY))
					{
						return false;
					}
				}
			}

			return true;
		}

		/// <summary>
		/// Returns chart graphics objects.
		/// </summary>
		/// <returns>Chart graphics object.</returns>
		internal ChartGraphics GetGraphics()
		{
            if (this.Common != null)
			{
				return this.Common.graph;
			}
			return null;
		}
#if Microsoft_CONTROL
		/// <summary>
		/// Checks if provided pixel coordinate is contained in one of the 
		/// selection handles rectangle.
		/// </summary>
		/// <param name="point">Coordinate in pixels.</param>
		/// <returns>Resizing mode.</returns>
		internal ResizingMode GetSelectionHandle(PointF point)
		{
			ResizingMode resizingMode = ResizingMode.None;

			if( this.Common != null &&
				this.Common.graph != null)
			{
				// Convert point to relative coordinates
				point = this.Common.graph.GetRelativePoint(point);

				// Check if point is in one of the selection handles
				if(this.selectionRects != null)
				{
					for(int index = 0; index < this.selectionRects.Length; index++)
					{
						if(!this.selectionRects[index].IsEmpty && 
							this.selectionRects[index].Contains(point))
						{
							if(index > (int)ResizingMode.AnchorHandle)
							{
								resizingMode = ResizingMode.MovingPathPoints;
								this.currentPathPointIndex = index - 9;
							}
							else
							{
								resizingMode = (ResizingMode)index;
							}
						}
					}
				}
			}

			return resizingMode;
		}
#endif //Microsoft_CONTROL
		/// <summary>
		/// Gets data point X or Y axis.
		/// </summary>
		/// <param name="dataPoint">Data point to get the axis for.</param>
		/// <param name="axisName">X or Y axis to get.</param>
		/// <returns>Data point axis.</returns>
		private Axis GetDataPointAxis(DataPoint dataPoint, AxisName axisName)
		{
            if (dataPoint != null && dataPoint.series != null && Chart != null)
            {
                // Get data point chart area
                ChartArea chartArea = Chart.ChartAreas[dataPoint.series.ChartArea];

                // Get point X axis
                if ((axisName == AxisName.X || axisName == AxisName.X2) &&
                    !chartArea.switchValueAxes)
                {
                    return chartArea.GetAxis(axisName, dataPoint.series.XAxisType, dataPoint.series.XSubAxisName);
                }
                else
                {
                    return chartArea.GetAxis(axisName, dataPoint.series.YAxisType, dataPoint.series.YSubAxisName);
                }
            }
			return null;
		}

		/// <summary>
		/// Gets annotation vertical and horizontal axes.
		/// </summary>
		/// <param name="vertAxis">Returns annotation vertical axis or null.</param>
		/// <param name="horizAxis">Returns annotation horizontal axis or null.</param>
		internal void GetAxes(ref Axis vertAxis, ref Axis horizAxis)
		{
			vertAxis = null;
			horizAxis = null;

			if(this.AxisX != null && this.AxisX.ChartArea != null)
			{
				if(this.AxisX.ChartArea.switchValueAxes)
				{
					vertAxis = this.AxisX;
				}
				else
				{
					horizAxis = this.AxisX;
				}
			}
			if(this.AxisY != null && this.AxisY.ChartArea != null)
			{
				if(this.AxisY.ChartArea.switchValueAxes)
				{
					horizAxis = this.AxisY;
				}
				else
				{
					vertAxis = this.AxisY;
				}
			}

			// Get axes from attached data point
			if(this.AnchorDataPoint != null)
			{
				if(horizAxis == null)
				{
					horizAxis = GetDataPointAxis(this.AnchorDataPoint, AxisName.X);

                    // For chart types like Bar, RangeBar and others, position of X and Y axes are flipped
                    if (horizAxis != null && horizAxis.ChartArea != null && horizAxis.ChartArea.switchValueAxes)
                    {
                        horizAxis = GetDataPointAxis(this.AnchorDataPoint, AxisName.Y);
                    }
				}
				if(vertAxis == null)
				{
					vertAxis = GetDataPointAxis(this.AnchorDataPoint, AxisName.Y);

                    // For chart types like Bar, RangeBar and others, position of X and Y axes are flipped
                    if (vertAxis != null && vertAxis.ChartArea != null && vertAxis.ChartArea.switchValueAxes)
                    {
                        vertAxis = GetDataPointAxis(this.AnchorDataPoint, AxisName.X);
                    }
                }
			}

			// No axes coordinate system for grouped annotations
			if(vertAxis != null || horizAxis != null)
			{
				if(this.AnnotationGroup != null)
				{
                    throw (new InvalidOperationException(SR.ExceptionAnnotationGroupedAxisMustBeEmpty));
				}
			}
		}

		#endregion

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
                //Free managed resources
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

#if Microsoft_CONTROL

	/// <summary>
	/// This class is used to stores position changing event data for an annotation.
	/// </summary>
	/// <remarks>
	/// Provides additional data like the new annotation and anchor position when an end user 
	/// is moving the annotation with the mouse.
	/// <para>
	/// Can be used to restrict annotation movement, or snap the annotation position to 
	/// specific points.
	/// </para>
	/// </remarks>
	[
	SRDescription("DescriptionAttributeAnnotationPositionChangingEventArgs_AnnotationPositionChangingEventArgs"),
	]
	public class AnnotationPositionChangingEventArgs : EventArgs
	{
		#region Fields

        private Annotation _Annotation = null;
		/// <summary>
		/// Gets or sets the annotation the event is fired for.
		/// </summary>
        public Annotation Annotation
        {
            get { return _Annotation; }
            set { _Annotation = value; }
        }

        private double _NewLocationX = 0.0;
        /// <summary>
        /// Gets or sets the new X location of the annotation.
        /// </summary>
        public double NewLocationX
        {
            get { return _NewLocationX; }
            set { _NewLocationX = value; }
        }

        private double _NewLocationY = 0.0;
        /// <summary>
        /// Gets or sets the new Y location of the annotation.
        /// </summary>
        public double NewLocationY
        {
            get { return _NewLocationY; }
            set { _NewLocationY = value; }
        }

        private double _NewSizeWidth = 0.0;
        /// <summary>
        /// Gets or sets the new width of the annotation.
        /// </summary>
        public double NewSizeWidth
        {
            get { return _NewSizeWidth; }
            set { _NewSizeWidth = value; }
        }

        private double _NewSizeHeight = 0.0;
        /// <summary>
        /// Gets or sets the new height of the annotation.
        /// </summary>
        public double NewSizeHeight
        {
            get { return _NewSizeHeight; }
            set { _NewSizeHeight = value; }
        }

        private double _NewAnchorLocationX = 0.0;
        /// <summary>
        /// Gets or sets the new annotation anchor point X location.
        /// </summary>
        public double NewAnchorLocationX
        {
            get { return _NewAnchorLocationX; }
            set { _NewAnchorLocationX = value; }
        }

        private double _NewAnchorLocationY = 0.0;
        /// <summary>
        /// Gets or sets the new annotation anchor point Y location.
        /// </summary>
        public double NewAnchorLocationY
        {
            get { return _NewAnchorLocationY; }
            set { _NewAnchorLocationY = value; }
        }

		#endregion // Fields

		#region Properties


		/// <summary>
		/// Gets or sets the new location and size of the annotation.
		/// </summary>
		[
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		]
		public RectangleF NewPosition 
		{
			get
			{
				return new RectangleF(
					(float)this.NewLocationX,
					(float)this.NewLocationY,
					(float)this.NewSizeWidth,
					(float)this.NewSizeHeight);
			}
			set
			{
				this.NewLocationX = value.X;
				this.NewLocationY = value.Y;
				this.NewSizeWidth = value.Width;
				this.NewSizeHeight = value.Height;
			}
		}

		/// <summary>
		/// Gets or sets the new anchor location of the annotation.
		/// </summary>
		[
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		]
		public PointF NewAnchorLocation
		{
			get
			{
				return new PointF(
					(float)this.NewAnchorLocationX,
					(float)this.NewAnchorLocationY);
			}
			set
			{
				this.NewAnchorLocationX = value.X;
				this.NewAnchorLocationY = value.Y;
			}
		}

		#endregion // Properties
	}

#endif //Microsoft_CONTROL
}
