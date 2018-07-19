//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		Axis.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	Axis
//
//  Purpose:	Axis related properties and methods. Axis class gives 
//				information to Common.Chart series about 
//				position in the Common.Chart area and keeps all necessary 
//				information about axes.
//
//	Reviewed:	GS - August 6, 2002
//				AG - August 7, 2002
//
//===================================================================

#region Used namespace
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Diagnostics.CodeAnalysis;
#if Microsoft_CONTROL
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
using System.Web.UI.DataVisualization.Charting.ChartTypes;
#endif


#endregion

#if Microsoft_CONTROL
	namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
    #region Axis name enumeration

    /// <summary>
    /// An enumeration of auto-fitting styles of the axis labels.
    /// </summary>
    [Flags]
    public enum LabelAutoFitStyles
    {
        /// <summary>
        /// No auto-fitting.
        /// </summary>
        None = 0,
        /// <summary>
        /// Allow font size increasing.
        /// </summary>
        IncreaseFont = 1,
        /// <summary>
        /// Allow font size decreasing.
        /// </summary>
        DecreaseFont = 2,
        /// <summary>
        /// Allow using staggered labels.
        /// </summary>
        StaggeredLabels = 4,
        /// <summary>
        /// Allow changing labels angle using values of 0, 30, 60 and 90 degrees.
        /// </summary>
        LabelsAngleStep30 = 8,
        /// <summary>
        /// Allow changing labels angle using values of 0, 45, 90 degrees.
        /// </summary>
        LabelsAngleStep45 = 16,
        /// <summary>
        /// Allow changing labels angle using values of 0 and 90 degrees.
        /// </summary>
        LabelsAngleStep90 = 32,
        /// <summary>
        /// Allow replacing spaces with the new line character.
        /// </summary>
        WordWrap = 64,
    }

    /// <summary>
    /// An enumeration of axis names.
    /// </summary>
    public enum AxisName
    {
        /// <summary>
        /// Primary X Axis.
        /// </summary>

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "X")]
        X = 0,
        /// <summary>
        /// Primary Y Axis.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Y")]
        Y = 1,
        /// <summary>
        /// Secondary X Axis.
        /// </summary>
        X2 = 2,
        /// <summary>
        /// Secondary Y Axis.
        /// </summary>
        Y2 = 3
    }

    #endregion

    /// <summary>
    /// The Axis class gives information to the Common.Chart series 
    /// about positions in the Common.Chart area and keeps all of 
    ///	the data about the axis.
    /// </summary>
    [
        SRDescription("DescriptionAttributeAxis_Axis"),
        DefaultProperty("Enabled"),
    ]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
#if Microsoft_CONTROL
    public partial class Axis :  ChartNamedElement
#else
	public partial class Axis :  ChartNamedElement, IChartMapArea
#endif
    {
        #region Axis fields

        /// <summary>
        /// Plot area position
        /// </summary>
        internal ElementPosition PlotAreaPosition;

        // This field synchronies Store and Reset temporary values
        private bool _storeValuesEnabled = true;

        private FontCache _fontCache = new FontCache();
        private Font  _titleFont;
        private Color _titleForeColor = Color.Black;
        private StringAlignment _titleAlignment = StringAlignment.Center;
        private string _title = "";
        private int _lineWidth = 1;
        private ChartDashStyle _lineDashStyle = ChartDashStyle.Solid;
        private Color _lineColor = Color.Black;
        private bool _isLabelAutoFit = true;
        private AxisArrowStyle _arrowStyle = AxisArrowStyle.None;
        private StripLinesCollection _stripLines = null;
        private bool _isMarksNextToAxis = true;

        // Default text orientation
        private TextOrientation _textOrientation = TextOrientation.Auto;

        // Size of the axis elements in percentage
        internal float titleSize = 0F;
        internal float labelSize = 0F;
        internal float labelNearOffset = 0F;
        internal float labelFarOffset = 0F;
        internal float unRotatedLabelSize = 0F;
        internal float markSize = 0F;
        internal float scrollBarSize = 0F;
        internal float totlaGroupingLabelsSize = 0F;
        internal float[] groupingLabelSizes = null;
        internal float totlaGroupingLabelsSizeAdjustment = 0f;
        private LabelAutoFitStyles _labelAutoFitStyle = LabelAutoFitStyles.DecreaseFont |
                                                            LabelAutoFitStyles.IncreaseFont |
                                                            LabelAutoFitStyles.LabelsAngleStep30 |
                                                            LabelAutoFitStyles.StaggeredLabels |
                                                            LabelAutoFitStyles.WordWrap;

        // Auto calculated font for labels
        internal Font autoLabelFont = null;
        internal int autoLabelAngle = -1000;
        internal int autoLabelOffset = -1;

        // Labels auto fitting constants
        private float _aveLabelFontSize = 10F;
        private float _minLabelFontSize = 5F;
        // Determines maximum label size of the chart area.
        private float _maximumAutoSize = 75f;

        // Chart title position rectangle
        private RectangleF _titlePosition = RectangleF.Empty;

        // Element spacing size
        internal const float elementSpacing = 1F;

        // Maximum total size of the axis's elements in percentage
        private const float maxAxisElementsSize = 75F;

        // Maximum size of the axis title in percentage
        private const float maxAxisTitleSize = 20F;

        // Maximum size of the axis second row of labels in percentage 
        // of the total labels size
        private const float maxAxisLabelRow2Size = 45F;

        // Maximum size of the axis tick marks in percentage
        private const float maxAxisMarkSize = 20F;

        // Minimum cached value from data series.
        internal double minimumFromData = double.NaN;

        // Maximum cached value from data series.
        internal double maximumFromData = double.NaN;

        // Flag, which tells to Set Data method to take, again values from 
        // data source and not to use cached values.
        internal bool refreshMinMaxFromData = true;

        // Flag, which tells to Set Data method to take, again values from 
        // data source and not to use cached values.
        internal int numberOfPointsInAllSeries = 0;

        // Original axis scaleView position
        private double _originalViewPosition = double.NaN;


        /// <summary>
        /// Indicates that isInterlaced strip lines will be displayed for the axis.
        /// </summary>
        private bool _isInterlaced = false;

        /// <summary>
        /// Color used to draw isInterlaced strip lines for the axis.
        /// </summary>
        private Color _interlacedColor = Color.Empty;

        /// <summary>
        /// Axis interval offset.
        /// </summary>
        private double _intervalOffset = 0;

        /// <summary>
        /// Axis interval.
        /// </summary>
        internal double interval = 0;

        /// <summary>
        /// Axis interval units type.
        /// </summary>
        internal DateTimeIntervalType intervalType = DateTimeIntervalType.Auto;

        /// <summary>
        /// Axis interval offset units type.
        /// </summary>
        internal DateTimeIntervalType intervalOffsetType = DateTimeIntervalType.Auto;

        /// <summary>
		/// Minimum font size that can be used by the labels auto-fitting algorithm.
		/// </summary>
		internal int					labelAutoFitMinFontSize = 6;

		/// <summary>
		/// Maximum font size that can be used by the labels auto-fitting algorithm.
		/// </summary>
		internal int					labelAutoFitMaxFontSize = 10;

		/// <summary>
		/// Axis tooltip
		/// </summary>
		private	string					_toolTip = String.Empty;

        /// <summary>
        /// Axis HREF
        /// </summary>
        private string _url = String.Empty;

#if !Microsoft_CONTROL

        
        /// <summary>
		/// Axis map area attributes
		/// </summary>
		private	string					_mapAreaAttributes = String.Empty;

        private string _postbackValue = String.Empty;

#endif

        #endregion

        #region Axis constructor and initialization

        /// <summary>
        /// Default constructor of Axis.
        /// </summary>
        public Axis()
            : base(null, GetName(AxisName.X))
        {
            Initialize(AxisName.X);
        }

        /// <summary>
        /// Axis constructor.
        /// </summary>
        /// <param name="chartArea">The chart area the axis belongs to.</param>
        /// <param name="axisTypeName">The type of the axis.</param>
        public Axis(ChartArea chartArea, AxisName axisTypeName)
            : base(chartArea, GetName(axisTypeName)) 
        {
            Initialize(axisTypeName);
        }

        /// <summary>
        /// Initialize axis class
        /// </summary>
        /// <param name="axisTypeName">Name of the axis type.</param>
        private void Initialize(AxisName axisTypeName)
        {
            // DT: Axis could be already created. Don't recreate new labelstyle and other objects.
            // Initialize axis labels
            if (labelStyle == null)
            {
                labelStyle = new LabelStyle(this);
            }
            if (_customLabels == null)
            {
                _customLabels = new CustomLabelsCollection(this);
            }
            if (_scaleView == null)
            {
                // Create axis data scaleView object
                _scaleView = new AxisScaleView(this);
            }
#if Microsoft_CONTROL
            if (scrollBar == null)
            {
                // Create axis croll bar class
                scrollBar = new AxisScrollBar(this);
            }
#endif // Microsoft_CONTROL

            this.axisType = axisTypeName;

            // Create grid & tick marks objects
            if (minorTickMark == null)
            {
                minorTickMark = new TickMark(this, false);
            }
            if (majorTickMark == null)
            {
                majorTickMark = new TickMark(this, true);
                majorTickMark.Interval = double.NaN;
                majorTickMark.IntervalOffset = double.NaN;
                majorTickMark.IntervalType = DateTimeIntervalType.NotSet;
                majorTickMark.IntervalOffsetType = DateTimeIntervalType.NotSet;
            }
            if (minorGrid == null)
            {
                minorGrid = new Grid(this, false);
            }
            if (majorGrid == null)
            {
                majorGrid = new Grid(this, true);
                majorGrid.Interval = double.NaN;
                majorGrid.IntervalOffset = double.NaN;
                majorGrid.IntervalType = DateTimeIntervalType.NotSet;
                majorGrid.IntervalOffsetType = DateTimeIntervalType.NotSet;
            }
            if (this._stripLines == null)
            {
                this._stripLines = new StripLinesCollection(this);
            }

            if (_titleFont == null)
            {
                _titleFont = _fontCache.DefaultFont;
            }
#if SUBAXES
			if(this.subAxes == null)
			{
				this.subAxes = new SubAxisCollection(this);
			}
#endif // SUBAXES

#if Microsoft_CONTROL

            // Initialize axis scroll bar class
            this.ScrollBar.Initialize();

#endif // Microsoft_CONTROL

            // Create collection of scale segments
            if (this.scaleSegments == null)
            {
                this.scaleSegments = new AxisScaleSegmentCollection(this);
            }

            // Create scale break style
            if (this.axisScaleBreakStyle == null)
            {
                this.axisScaleBreakStyle = new AxisScaleBreakStyle(this);
            }
        }

        /// <summary>
        /// Initialize axis class
        /// </summary>
        /// <param name="chartArea">Chart area that the axis belongs.</param>
        /// <param name="axisTypeName">Axis type.</param>
        internal void Initialize(ChartArea chartArea, AxisName axisTypeName)
        {
            this.Initialize(axisTypeName);
            this.Parent = chartArea;
            this.Name = GetName(axisTypeName);
        }

        /// <summary>
        /// Set Axis Name
        /// </summary>
        internal static string GetName(AxisName axisName)
        {
            // Set axis name.
            // NOTE: Strings below should neber be localized. Name properties in the chart are never localized 
            // and represent consisten object name in all locales.
            switch (axisName)
            {
                case (AxisName.X):
                    return "X axis";
                case (AxisName.Y):
                    return "Y (Value) axis";
                case (AxisName.X2):
                    return "Secondary X axis";
                case (AxisName.Y2):
                    return "Secondary Y (Value) axis";
            }
            return null;
        }

        #endregion

        #region Axis properies

        // Internal
        internal ChartArea ChartArea
        {
            get { return Parent as ChartArea; }
        }

        /// <summary>
        /// Text orientation.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
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
                this.Invalidate();
            }
        }

        /// <summary>
        /// Returns sub-axis name.
        /// </summary>
        virtual internal string SubAxisName
        {
            get
            {
                return string.Empty;
            }
        }

#if SUBAXES

		/// <summary>
		/// Indicates if this axis object present the main or sub axis.
		/// </summary>
		virtual internal bool IsSubAxis
		{
			get
			{
				return false;
			}
		}

		private SubAxisCollection subAxes = null;

		/// <summary>
		/// Sub-axes collection.
		/// </summary>
		[
		SRCategory("CategoryAttributeSubAxes"),
		Bindable(true),
		SRDescription("DescriptionAttributeSubAxes"),
#if Microsoft_CONTROL
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
		PersistenceMode(PersistenceMode.InnerProperty),
#endif
        Editor(Editors.ChartCollectionEditor.Editor, Editors.ChartCollectionEditor.Base)
		]
		virtual public SubAxisCollection SubAxes
		{
			get
			{
				return this.subAxes;
			}
		}

#endif // SUBAXES

        /// <summary>
        /// Gets or sets a flag which indicates whether interlaced strip lines will be displayed for the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        DefaultValue(false),
        SRDescription("DescriptionAttributeInterlaced"),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute),
#endif
 NotifyParentPropertyAttribute(true),
        ]
        public bool IsInterlaced
        {
            get
            {
                return _isInterlaced;
            }
            set
            {
                _isInterlaced = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color used to draw interlaced strip lines for the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        DefaultValue(typeof(Color), ""),
        SRDescription("DescriptionAttributeInterlacedColor"),
        NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public Color InterlacedColor
        {
            get
            {
                return _interlacedColor;
            }
            set
            {
                _interlacedColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Axis name. This field is reserved for internal use only.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        Browsable(false),
        DefaultValue(""),
        SRDescription("DescriptionAttributeAxis_Name"),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute),
#endif
 DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
        SerializationVisibilityAttribute(SerializationVisibility.Hidden)
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
        /// Axis name. This field is reserved for internal use only.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        Browsable(false),
        DefaultValue(""),
        SRDescription("DescriptionAttributeType"),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute),
#endif
 DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
        SerializationVisibilityAttribute(SerializationVisibility.Hidden)
        ]
        virtual public AxisName AxisName
        {
            get
            {
                return axisType;
            }
        }

        /// <summary>
        /// Gets or sets the arrow style used for the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        DefaultValue(AxisArrowStyle.None),
        NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeArrows"),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public AxisArrowStyle ArrowStyle
        {
            get
            {
                return _arrowStyle;
            }
            set
            {
                _arrowStyle = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the properties used for the major gridlines.
        /// </summary>
        [
        SRCategory("CategoryAttributeGridTickMarks"),
        Bindable(true),
        NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeMajorGrid"),
#if Microsoft_CONTROL
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
 PersistenceMode(PersistenceMode.InnerProperty),
#endif
 TypeConverter(typeof(NoNameExpandableObjectConverter))
        ]
        public Grid MajorGrid
        {
            get
            {
                return majorGrid;
            }
            set
            {
                majorGrid = value;
                majorGrid.Axis = this;
                majorGrid.majorGridTick = true;

                if (!majorGrid.intervalChanged)
                    majorGrid.Interval = double.NaN;
                if (!majorGrid.intervalOffsetChanged)
                    majorGrid.IntervalOffset = double.NaN;
                if (!majorGrid.intervalTypeChanged)
                    majorGrid.IntervalType = DateTimeIntervalType.NotSet;
                if (!majorGrid.intervalOffsetTypeChanged)
                    majorGrid.IntervalOffsetType = DateTimeIntervalType.NotSet;

                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the properties used for the minor gridlines.
        /// </summary>
        [
        SRCategory("CategoryAttributeGridTickMarks"),
        Bindable(true),
        NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeMinorGrid"),
#if Microsoft_CONTROL
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
 PersistenceMode(PersistenceMode.InnerProperty),
#endif
 TypeConverter(typeof(NoNameExpandableObjectConverter))
        ]
        public Grid MinorGrid
        {
            get
            {
                return minorGrid;
            }
            set
            {
                minorGrid = value;
                minorGrid.Initialize(this, false);
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the properties used for the major tick marks.
        /// </summary>
        [
        SRCategory("CategoryAttributeGridTickMarks"),
        Bindable(true),
        NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeMajorTickMark"),
#if Microsoft_CONTROL
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
 PersistenceMode(PersistenceMode.InnerProperty),
#endif
 TypeConverter(typeof(NoNameExpandableObjectConverter))
        ]
        public TickMark MajorTickMark
        {
            get
            {
                return majorTickMark;
            }
            set
            {
                majorTickMark = value;
                majorTickMark.Axis = this;
                majorTickMark.majorGridTick = true;

                if (!majorTickMark.intervalChanged)
                    majorTickMark.Interval = double.NaN;
                if (!majorTickMark.intervalOffsetChanged)
                    majorTickMark.IntervalOffset = double.NaN;
                if (!majorTickMark.intervalTypeChanged)
                    majorTickMark.IntervalType = DateTimeIntervalType.NotSet;
                if (!majorTickMark.intervalOffsetTypeChanged)
                    majorTickMark.IntervalOffsetType = DateTimeIntervalType.NotSet;

                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the properties used for the minor tick marks.
        /// </summary>
        [
        SRCategory("CategoryAttributeGridTickMarks"),
        Bindable(true),
        NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeMinorTickMark"),
#if Microsoft_CONTROL
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
 PersistenceMode(PersistenceMode.InnerProperty),
#endif
 TypeConverter(typeof(NoNameExpandableObjectConverter))
        ]
        public TickMark MinorTickMark
        {
            get
            {
                return minorTickMark;
            }
            set
            {
                minorTickMark = value;
                minorTickMark.Initialize(this, false);
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether auto-fitting of labels is enabled.
        /// </summary>
        [
        SRCategory("CategoryAttributeLabels"),
        Bindable(true),
        DefaultValue(true),
        SRDescription("DescriptionAttributeLabelsAutoFit"),
        NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute),
#endif
 RefreshPropertiesAttribute(RefreshProperties.All)
        ]
        public bool IsLabelAutoFit
        {
            get
            {
                return _isLabelAutoFit;
            }
            set
            {
                _isLabelAutoFit = value;
                this.Invalidate();
            }
        }



		/// <summary>
        /// Gets or sets the minimum font size that can be used by 
        /// the label auto-fitting algorithm.
		/// </summary>
		[
		SRCategory("CategoryAttributeLabels"),
		Bindable(true),
		DefaultValue(6),
		SRDescription("DescriptionAttributeLabelsAutoFitMinFontSize"),
		NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
		RefreshPropertiesAttribute(RefreshProperties.All)
		]
		public int LabelAutoFitMinFontSize
		{
			get
			{
				return this.labelAutoFitMinFontSize;
			}
			set
			{
				// Font size cannot be less than 5
				if(value < 5)
				{
                    throw (new InvalidOperationException(SR.ExceptionAxisLabelsAutoFitMinFontSizeValueInvalid));
				}

				this.labelAutoFitMinFontSize = value;
				this.Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets the maximum font size that can be used by 
        /// the label auto-fitting algorithm.
		/// </summary>
		[
		SRCategory("CategoryAttributeLabels"),
		Bindable(true),
		DefaultValue(10),
		SRDescription("DescriptionAttributeLabelsAutoFitMaxFontSize"),
		NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif
		RefreshPropertiesAttribute(RefreshProperties.All)
		]
		public int LabelAutoFitMaxFontSize
		{
			get
			{
				return this.labelAutoFitMaxFontSize;
			}
			set
			{
				// Font size cannot be less than 5
				if(value < 5)
				{
                    throw (new InvalidOperationException(SR.ExceptionAxisLabelsAutoFitMaxFontSizeInvalid));
				}

				this.labelAutoFitMaxFontSize = value;
				this.Invalidate();
			}
		}



        /// <summary>
        /// Gets or sets the auto-fitting style used for the labels. 
        /// IsLabelAutoFit must be set to true.
        /// </summary>
        [
        SRCategory("CategoryAttributeLabels"),
        Bindable(true),
        DefaultValue(LabelAutoFitStyles.DecreaseFont | LabelAutoFitStyles.IncreaseFont | LabelAutoFitStyles.LabelsAngleStep30 | LabelAutoFitStyles.StaggeredLabels | LabelAutoFitStyles.WordWrap),
        SRDescription("DescriptionAttributeLabelsAutoFitStyle"),
        NotifyParentPropertyAttribute(true),
        Editor(Editors.FlagsEnumUITypeEditor.Editor, Editors.FlagsEnumUITypeEditor.Base),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute),
#endif
]
        public LabelAutoFitStyles LabelAutoFitStyle
        {
            get
            {
                return this._labelAutoFitStyle;
            }
            set
            {
                this._labelAutoFitStyle = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a flag which indicates whether
        /// tick marks and labels move with the axis when 
        /// the crossing value changes.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        DefaultValue(true),
        SRDescription("DescriptionAttributeMarksNextToAxis"),
        NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        virtual public bool IsMarksNextToAxis
        {
            get
            {
                return _isMarksNextToAxis;
            }
            set
            {
                _isMarksNextToAxis = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the axis title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        Bindable(true),
        DefaultValue(""),
        SRDescription("DescriptionAttributeTitle6"),
        NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the color of the axis title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        Bindable(true),
        DefaultValue(typeof(Color), "Black"),
        SRDescription("DescriptionAttributeTitleColor"),
        NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public Color TitleForeColor
        {
            get
            {
                return _titleForeColor;
            }
            set
            {
                _titleForeColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the alignment of the axis title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        Bindable(true),
        DefaultValue(typeof(StringAlignment), "Center"),
        SRDescription("DescriptionAttributeTitleAlignment"),
        NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public StringAlignment TitleAlignment
        {
            get
            {
                return _titleAlignment;
            }
            set
            {
                _titleAlignment = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the font used for the axis title.
        /// </summary>
        [
        SRCategory("CategoryAttributeTitle"),
        Bindable(true),
        DefaultValue(typeof(Font), "Microsoft Sans Serif, 8pt"),
        SRDescription("DescriptionAttributeTitleFont"),
        NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public Font TitleFont
        {
            get
            {
                return _titleFont;
            }
            set
            {
                _titleFont = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the line color of the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        DefaultValue(typeof(Color), "Black"),
        SRDescription("DescriptionAttributeLineColor"),
        NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public Color LineColor
        {
            get
            {
                return _lineColor;
            }
            set
            {
                _lineColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the line width of the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        DefaultValue(1),
        SRDescription("DescriptionAttributeLineWidth"),
        NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public int LineWidth
        {
            get
            {
                return _lineWidth;
            }
            set
            {
                if (value < 0)
                {
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAxisWidthIsNegative));
                }
                _lineWidth = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the line style of the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        DefaultValue(ChartDashStyle.Solid),
        SRDescription("DescriptionAttributeLineDashStyle"),
        NotifyParentPropertyAttribute(true),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public ChartDashStyle LineDashStyle
        {
            get
            {
                return _lineDashStyle;
            }
            set
            {
                _lineDashStyle = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// The collection of strip lines of the axis.
        /// </summary>
        [
        SRCategory("CategoryAttributeAppearance"),
        Bindable(true),
        SRDescription("DescriptionAttributeStripLines"),
#if Microsoft_CONTROL
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else
 PersistenceMode(PersistenceMode.InnerProperty),
#endif
        Editor(Editors.ChartCollectionEditor.Editor, Editors.ChartCollectionEditor.Base)
        ]
        public StripLinesCollection StripLines
        {
            get
            {
                return _stripLines;
            }
        }


        /// <summary>
        /// Gets or sets the maximum size (in percentage) of the axis used in the automatic layout algorithm.
        /// </summary>
        /// <remarks>
        /// This property determines the maximum size of the axis, measured as a percentage of the chart area.
        /// </remarks>
        [
        SRCategory("CategoryAttributeLabels"),
        DefaultValue(75f),
        SRDescription("DescriptionAttributeAxis_MaxAutoSize"),
        ]
        public float MaximumAutoSize
        {
            get
            {
                return this._maximumAutoSize;
            }
            set
            {
                if (value < 0f || value > 100f)
                {
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionValueMustBeInRange("MaximumAutoSize", "0", "100")));
                }
                this._maximumAutoSize = value;
                this.Invalidate();
            }
        }
        #endregion

        #region	IMapAreaAttributes Properties implementation

		/// <summary>
		/// Tooltip of the axis.
		/// </summary>
		[
		SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
		SRDescription("DescriptionAttributeToolTip"),
		DefaultValue(""),
		]
		public string ToolTip
		{
			set
			{
				this._toolTip = value;
			}
			get
			{
				return this._toolTip;
			}
		}

#if !Microsoft_CONTROL

		/// <summary>
		/// URL target of the axis.
		/// </summary>
		[
		SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
		SRDescription("DescriptionAttributeUrl"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute),
        Editor(Editors.UrlValueEditor.Editor, Editors.UrlValueEditor.Base)
		]
		public string Url
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
		/// Gets or sets the map area attributes.
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
				this._mapAreaAttributes = value;
			}
			get
			{
                return this._mapAreaAttributes;
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



        #endregion

        #region Axis Interavl properies

        /// <summary>
        /// Axis interval size.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        Bindable(true),
        DefaultValue(0.0),
        SRDescription("DescriptionAttributeInterval4"),
        RefreshPropertiesAttribute(RefreshProperties.All),
        TypeConverter(typeof(AxisIntervalValueConverter)),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute),
#endif
]
        public double Interval
        {
            get
            {
                return interval;
            }
            set
            {
                // Axis interval properties must be set
                if (double.IsNaN(value))
                {
                    interval = 0;
                }
                else
                {
                    interval = value;
                }

                // Reset initial values
                majorGrid.interval = tempMajorGridInterval;
                majorTickMark.interval = tempMajorTickMarkInterval;
                minorGrid.interval = tempMinorGridInterval;
                minorTickMark.interval = tempMinorTickMarkInterval;
                labelStyle.interval = tempLabelInterval;

                this.Invalidate();
            }
        }

        /// <summary>
        /// Axis interval offset.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        Bindable(true),
        DefaultValue(0.0),
        SRDescription("DescriptionAttributeIntervalOffset6"),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute),
#endif
 RefreshPropertiesAttribute(RefreshProperties.All),
        TypeConverter(typeof(AxisIntervalValueConverter))
        ]
        public double IntervalOffset
        {
            get
            {
                return _intervalOffset;
            }
            set
            {
                // Axis interval properties must be set
                if (double.IsNaN(value))
                {
                    _intervalOffset = 0;
                }
                else
                {
                    _intervalOffset = value;
                }

                this.Invalidate();
            }
        }

        /// <summary>
        /// Axis interval type.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        Bindable(true),
        DefaultValue(DateTimeIntervalType.Auto),
        SRDescription("DescriptionAttributeIntervalType4"),
        RefreshPropertiesAttribute(RefreshProperties.All),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public DateTimeIntervalType IntervalType
        {
            get
            {
                return intervalType;
            }
            set
            {
                // Axis interval properties must be set
                if (value == DateTimeIntervalType.NotSet)
                {
                    intervalType = DateTimeIntervalType.Auto;
                }
                else
                {
                    intervalType = value;
                }

                // Reset initial values
                majorGrid.intervalType = tempGridIntervalType;
                majorTickMark.intervalType = tempTickMarkIntervalType;
                labelStyle.intervalType = tempLabelIntervalType;

                this.Invalidate();
            }
        }

        /// <summary>
        /// Axis interval offset type.
        /// </summary>
        [
        SRCategory("CategoryAttributeInterval"),
        Bindable(true),
        DefaultValue(DateTimeIntervalType.Auto),
        SRDescription("DescriptionAttributeIntervalOffsetType4"),
        RefreshPropertiesAttribute(RefreshProperties.All),
#if !Microsoft_CONTROL
 PersistenceMode(PersistenceMode.Attribute)
#endif
]
        public DateTimeIntervalType IntervalOffsetType
        {
            get
            {
                return intervalOffsetType;
            }
            set
            {
                // Axis interval properties must be set
                if (value == DateTimeIntervalType.NotSet)
                {
                    intervalOffsetType = DateTimeIntervalType.Auto;
                }
                else
                {
                    intervalOffsetType = value;
                }

                this.Invalidate();
            }
        }

        #endregion

        #region Axis painting methods

        /// <summary>
        /// Checks if Common.Chart axis title is drawn vertically.
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
        /// Returns axis title text orientation. If set to Auto automatically determines the
        /// orientation based on the axis position.
        /// </summary>
        /// <returns>Current text orientation.</returns>
        private TextOrientation GetTextOrientation()
        {
            if (this.TextOrientation == TextOrientation.Auto)
            {
                if (this.AxisPosition == AxisPosition.Left)
                {
                    return TextOrientation.Rotated270;
                }
                else if (this.AxisPosition == AxisPosition.Right)
                {
                    return TextOrientation.Rotated90;
                }
                return TextOrientation.Horizontal;
            }
            return this.TextOrientation;
        }

        /// <summary>
        /// Paint Axis elements on the back of the 3D scene.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        internal void PrePaint(ChartGraphics graph)
        {
            if (enabled != false)
            {
                // draw axis hot region
                DrawAxisLineHotRegion(graph, true);

                // Paint Major Tick Marks
                majorTickMark.Paint(graph, true);

                // Paint Minor Tick Marks
                minorTickMark.Paint(graph, true);

                // Draw axis line
                DrawAxisLine(graph, true);

                // Paint Labels
                labelStyle.Paint(graph, true);
            }

#if SUBAXES
			// Process all sub-axis
			if(!ChartArea.Area3DStyle.Enable3D && 
				!ChartArea.chartAreaIsCurcular)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.PrePaint( graph );
				}
			}
#endif // SUBAXES
        }

        /// <summary>
        /// Paint Axis
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        internal void Paint(ChartGraphics graph)
        {
            // Only Y axis is drawn in the circular Common.Chart area
            if (ChartArea != null && ChartArea.chartAreaIsCurcular)
            {
                // Y circular axes
                if (this.axisType == AxisName.Y && enabled != false)
                {
                    ICircularChartType chartType = ChartArea.GetCircularChartType();
                    if (chartType != null)
                    {
                        Matrix oldMatrix = graph.Transform;
                        float[] axesLocation = chartType.GetYAxisLocations(ChartArea);
                        bool drawLabels = true;
                        foreach (float curentSector in axesLocation)
                        {
                            // Set graphics rotation matrix
                            Matrix newMatrix = oldMatrix.Clone();
                            newMatrix.RotateAt(
                                curentSector,
                                graph.GetAbsolutePoint(ChartArea.circularCenter));
                            graph.Transform = newMatrix;

                            // draw axis hot region
                            DrawAxisLineHotRegion(graph, false);

                            // Paint Minor Tick Marks
                            minorTickMark.Paint(graph, false);

                            // Paint Major Tick Marks
                            majorTickMark.Paint(graph, false);

                            // Draw axis line
                            DrawAxisLine(graph, false);

                            // Only first Y axis has labels
                            if (drawLabels)
                            {
                                drawLabels = false;

                                // Save current font angle 
                                int currentAngle = labelStyle.Angle;

                                // Set labels text angle
                                if (labelStyle.Angle == 0)
                                {
                                    if (curentSector >= 45f && curentSector <= 180f)
                                    {
                                        labelStyle.angle = -90;
                                    }
                                    else if (curentSector > 180f && curentSector <= 315f)
                                    {
                                        labelStyle.angle = 90;
                                    }
                                }

                                // Draw labels 
                                labelStyle.Paint(graph, false);

                                // Restore font angle 
                                labelStyle.angle = currentAngle;
                            }
                        }

                        graph.Transform = oldMatrix;
                    }
                }

                // X circular axes
                if (this.axisType == AxisName.X && enabled != false)
                {
                    labelStyle.PaintCircular(graph);
                }

                DrawAxisTitle(graph);

                return;
            }

            // If axis is disabled draw only Title
            if (enabled != false)
            {

                // draw axis hot region
                DrawAxisLineHotRegion(graph, false);

                // Paint Minor Tick Marks
                minorTickMark.Paint(graph, false);

                // Paint Major Tick Marks
                majorTickMark.Paint(graph, false);

                // Draw axis line
                DrawAxisLine(graph, false);

                // Paint Labels
                labelStyle.Paint(graph, false);

#if Microsoft_CONTROL

                // Scroll bar is supoorted only in 2D charts
                if (ChartArea != null && ChartArea.Area3DStyle.Enable3D == false)
                {
                    // Draw axis scroll bar
                    ScrollBar.Paint(graph);
                }
#endif // Microsoft_CONTROL

            }

            // Draw axis title
            this.DrawAxisTitle(graph);

#if SUBAXES
			// Process all sub-axis
			if(ChartArea.IsSubAxesSupported)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.Paint( graph );
				}
			}
#endif // SUBAXES

            // Reset temp axis offset for side-by-side charts like column
            this.ResetTempAxisOffset();
        }



		/// <summary>
		/// Paint Axis element when segmented axis scale feature is used.
		/// </summary>
		/// <param name="graph">Reference to the Chart Graphics object</param>
		internal void PaintOnSegmentedScalePassOne( ChartGraphics graph )
		{
			// If axis is disabled draw only Title
			if( enabled != false )
			{
				// Paint Minor Tick Marks
				minorTickMark.Paint( graph, false );

				// Paint Major Tick Marks
				majorTickMark.Paint( graph, false );
            }

#if SUBAXES
			// Process all sub-axis
			if(ChartArea.IsSubAxesSupported)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.PaintOnSegmentedScalePassOne( graph );
				}
			}
#endif // SUBAXES

        }

		/// <summary>
		/// Paint Axis element when segmented axis scale feature is used.
		/// </summary>
		/// <param name="graph">Reference to the Chart Graphics object</param>
		internal void PaintOnSegmentedScalePassTwo( ChartGraphics graph )
		{
			// If axis is disabled draw only Title
			if( enabled != false )
			{
				// Draw axis line
				DrawAxisLine( graph, false );

				// Paint Labels
				labelStyle.Paint( graph, false);
			}

			// Draw axis title
			this.DrawAxisTitle( graph );
		
			// Reset temp axis offset for side-by-side charts like column
			this.ResetTempAxisOffset();

#if SUBAXES
			// Process all sub-axis
			if(ChartArea.IsSubAxesSupported)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.PaintOnSegmentedScalePassTwo( graph );
				}
			}
#endif // SUBAXES

        }
				
        /// <summary>
        /// Draw axis title
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        private void DrawAxisTitle(ChartGraphics graph)
        {
            if (!this.enabled)
                return;

            // Draw axis title
            if (this.Title.Length > 0)
            {
                Matrix oldTransform = null;

                // Draw title in 3D
                if (ChartArea.Area3DStyle.Enable3D && !ChartArea.chartAreaIsCurcular)
                {
                    DrawAxis3DTitle(graph);
                    return;
                }

                string axisTitle = this.Title;

                //******************************************************
                //** Check axis position
                //******************************************************
                float axisPosition = (float)this.GetAxisPosition();
                if (this.AxisPosition == AxisPosition.Bottom)
                {
                    if (!this.GetIsMarksNextToAxis())
                    {
                        axisPosition = ChartArea.PlotAreaPosition.Bottom;
                    }
                    axisPosition = ChartArea.PlotAreaPosition.Bottom - axisPosition;
                }
                else if (this.AxisPosition == AxisPosition.Top)
                {
                    if (!this.GetIsMarksNextToAxis())
                    {
                        axisPosition = ChartArea.PlotAreaPosition.Y;
                    }
                    axisPosition = axisPosition - ChartArea.PlotAreaPosition.Y;
                }
                else if (this.AxisPosition == AxisPosition.Right)
                {
                    if (!this.GetIsMarksNextToAxis())
                    {
                        axisPosition = ChartArea.PlotAreaPosition.Right;
                    }
                    axisPosition = ChartArea.PlotAreaPosition.Right - axisPosition;
                }
                else if (this.AxisPosition == AxisPosition.Left)
                {
                    if (!this.GetIsMarksNextToAxis())
                    {
                        axisPosition = ChartArea.PlotAreaPosition.X;
                    }
                    axisPosition = axisPosition - ChartArea.PlotAreaPosition.X;
                }

                //******************************************************
                //** Adjust axis elements size with axis position
                //******************************************************
                // Calculate total size of axis elements
                float axisSize = this.markSize + this.labelSize;
                axisSize -= axisPosition;
                if (axisSize < 0)
                {
                    axisSize = 0;
                }
                // Set title alignment
                using (StringFormat format = new StringFormat())
                {
                    format.Alignment = this.TitleAlignment;
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    // VSTS #144398
                    // We need to have the StringFormatFlags set to FitBlackBox as othwerwise axis titles using Fonts like 
                    // "Algerian" or "Forte" are completly clipped (= not drawn) due to the fact that MeasureString returns 
                    // a bounding rectangle that is too small.
                    format.FormatFlags |= StringFormatFlags.FitBlackBox;

                    // Calculate title rectangle
                    _titlePosition = ChartArea.PlotAreaPosition.ToRectangleF();
                    float titleSizeWithoutSpacing = this.titleSize - elementSpacing;
                    if (this.AxisPosition == AxisPosition.Left)
                    {
                        _titlePosition.X = ChartArea.PlotAreaPosition.X - titleSizeWithoutSpacing - axisSize;
                        _titlePosition.Y = ChartArea.PlotAreaPosition.Y;

                        if (!this.IsTextVertical)
                        {
                            SizeF axisTitleSize = new SizeF(titleSizeWithoutSpacing, ChartArea.PlotAreaPosition.Height);
                            _titlePosition.Width = axisTitleSize.Width;
                            _titlePosition.Height = axisTitleSize.Height;

                            format.Alignment = StringAlignment.Center;
                            if (this.TitleAlignment == StringAlignment.Far)
                            {
                                format.LineAlignment = StringAlignment.Near;
                            }
                            else if (this.TitleAlignment == StringAlignment.Near)
                            {
                                format.LineAlignment = StringAlignment.Far;
                            }
                            else
                            {
                                format.LineAlignment = StringAlignment.Center;
                            }
                        }
                        else
                        {
                            SizeF axisTitleSize = graph.GetAbsoluteSize(new SizeF(titleSizeWithoutSpacing, ChartArea.PlotAreaPosition.Height));
                            axisTitleSize = graph.GetRelativeSize(new SizeF(axisTitleSize.Height, axisTitleSize.Width));

							_titlePosition.Width = axisTitleSize.Width;
							_titlePosition.Height = axisTitleSize.Height;

                            _titlePosition.Y += ChartArea.PlotAreaPosition.Height / 2f - _titlePosition.Height / 2f;
							_titlePosition.X += titleSizeWithoutSpacing / 2f - _titlePosition.Width / 2f;

                            // Set graphics rotation transformation
                            oldTransform = this.SetRotationTransformation(graph, _titlePosition);

                            // Set alignment
                            format.LineAlignment = StringAlignment.Center;
                        }
                    }
                    else if (this.AxisPosition == AxisPosition.Right)
                    {
                        _titlePosition.X = ChartArea.PlotAreaPosition.Right + axisSize;
                        _titlePosition.Y = ChartArea.PlotAreaPosition.Y;

                        if (!this.IsTextVertical)
                        {
                            SizeF axisTitleSize = new SizeF(titleSizeWithoutSpacing, ChartArea.PlotAreaPosition.Height);
                            _titlePosition.Width = axisTitleSize.Width;
                            _titlePosition.Height = axisTitleSize.Height;

                            format.Alignment = StringAlignment.Center;
                            if (this.TitleAlignment == StringAlignment.Far)
                            {
                                format.LineAlignment = StringAlignment.Near;
                            }
                            else if (this.TitleAlignment == StringAlignment.Near)
                            {
                                format.LineAlignment = StringAlignment.Far;
                            }
                            else
                            {
                                format.LineAlignment = StringAlignment.Center;
                            }
                        }
                        else
                        {
                            SizeF axisTitleSize = graph.GetAbsoluteSize(new SizeF(titleSizeWithoutSpacing, ChartArea.PlotAreaPosition.Height));
                            axisTitleSize = graph.GetRelativeSize(new SizeF(axisTitleSize.Height, axisTitleSize.Width));

							_titlePosition.Width = axisTitleSize.Width;
							_titlePosition.Height = axisTitleSize.Height;
							
                            _titlePosition.Y += ChartArea.PlotAreaPosition.Height / 2f - _titlePosition.Height / 2f;
							_titlePosition.X += titleSizeWithoutSpacing / 2f - _titlePosition.Width / 2f;

                            // Set graphics rotation transformation
                            oldTransform = this.SetRotationTransformation(graph, _titlePosition);

                            // Set alignment
                            format.LineAlignment = StringAlignment.Center;
                        }
                    }
                    else if (this.AxisPosition == AxisPosition.Top)
                    {
                        _titlePosition.Y = ChartArea.PlotAreaPosition.Y - titleSizeWithoutSpacing - axisSize;
                        _titlePosition.Height = titleSizeWithoutSpacing;
                        _titlePosition.X = ChartArea.PlotAreaPosition.X;
                        _titlePosition.Width = ChartArea.PlotAreaPosition.Width;

                        if (this.IsTextVertical)
                        {
                            // Set graphics rotation transformation
                            oldTransform = this.SetRotationTransformation(graph, _titlePosition);
                        }

                        // Set alignment
                        format.LineAlignment = StringAlignment.Center;
                    }
                    else if (this.AxisPosition == AxisPosition.Bottom)
                    {
                        _titlePosition.Y = ChartArea.PlotAreaPosition.Bottom + axisSize;
                        _titlePosition.Height = titleSizeWithoutSpacing;
                        _titlePosition.X = ChartArea.PlotAreaPosition.X;
                        _titlePosition.Width = ChartArea.PlotAreaPosition.Width;

                        if (this.IsTextVertical)
                        {
                            // Set graphics rotation transformation
                            oldTransform = this.SetRotationTransformation(graph, _titlePosition);
                        }

                        // Set alignment
                        format.LineAlignment = StringAlignment.Center;
                    }

#if DEBUG
                    // TESTING CODE: Shows labels rectangle position.
					//				RectangleF rr = graph.GetAbsoluteRectangle(_titlePosition);
					//				graph.DrawRectangle(Pens.Blue, rr.X, rr.Y, rr.Width, rr.Height);
#endif // DEBUG

                    // Draw title
                    using (Brush brush = new SolidBrush(this.TitleForeColor))
                    {
                        graph.DrawStringRel(
                            axisTitle.Replace("\\n", "\n"),
                            this.TitleFont,
                            brush,
                            _titlePosition,
                            format,
                            this.GetTextOrientation());
                    }
                }

                // Process selection regions
                if (this.Common.ProcessModeRegions)
                {
                    // NOTE: Solves Issue #4423
                    // Transform title position coordinates using curent Graphics matrix
                    RectangleF transformedTitlePosition = graph.GetAbsoluteRectangle(_titlePosition);
                    PointF[] rectPoints = new PointF[] { 
						new PointF(transformedTitlePosition.X, transformedTitlePosition.Y),
						new PointF(transformedTitlePosition.Right, transformedTitlePosition.Bottom) };
                    graph.Transform.TransformPoints(rectPoints);
                    transformedTitlePosition = new RectangleF(
                        rectPoints[0].X,
                        rectPoints[0].Y,
                        rectPoints[1].X - rectPoints[0].X,
                        rectPoints[1].Y - rectPoints[0].Y);
                    if (transformedTitlePosition.Width < 0)
                    {
                        transformedTitlePosition.Width = Math.Abs(transformedTitlePosition.Width);
                        transformedTitlePosition.X -= transformedTitlePosition.Width;
                    }
                    if (transformedTitlePosition.Height < 0)
                    {
                        transformedTitlePosition.Height = Math.Abs(transformedTitlePosition.Height);
                        transformedTitlePosition.Y -= transformedTitlePosition.Height;
                    }

                    // Add hot region 
                    this.Common.HotRegionsList.AddHotRegion(
                        transformedTitlePosition, this, ChartElementType.AxisTitle, false, false);
                }

                // Restore old transformation
                if (oldTransform != null)
                {
                    graph.Transform = oldTransform;
                }
            }
        }

        /// <summary>
        /// Helper method which sets 90 or -90 degrees transformation in the middle of the 
        /// specified rectangle. It is used to draw title text rotated 90 or 270 degrees.
        /// </summary>
        /// <param name="graph">Chart graphics to apply transformation for.</param>
        /// <param name="titlePosition">Title position.</param>
        /// <returns>Old graphics transformation matrix.</returns>
        private Matrix SetRotationTransformation(ChartGraphics graph, RectangleF titlePosition)
        {
            // Save old graphics transformation
            Matrix oldTransform = graph.Transform.Clone();

            // Rotate left tile 90 degrees at center
            PointF center = PointF.Empty;
            center.X = titlePosition.X + titlePosition.Width / 2F;
            center.Y = titlePosition.Y + titlePosition.Height / 2F;

            // Create and set new transformation matrix
            float angle = (this.GetTextOrientation() == TextOrientation.Rotated90) ? 90f : -90f;
            Matrix newMatrix = graph.Transform.Clone();
            newMatrix.RotateAt(angle, graph.GetAbsolutePoint(center));
            graph.Transform = newMatrix;

            return oldTransform;
        }
            

        /// <summary>
        /// Draws a radial line in circular Common.Chart area.
        /// </summary>
        /// <param name="obj">Object requesting the painting.</param>
        /// <param name="graph">Graphics path.</param>
        /// <param name="color">Line color.</param>
        /// <param name="width">Line width.</param>
        /// <param name="style">Line style.</param>
        /// <param name="position">X axis circular position.</param>
        internal void DrawRadialLine(
            object obj,
            ChartGraphics graph,
            Color color,
            int width,
            ChartDashStyle style,
            double position)
        {
            // Create circle position rectangle
            RectangleF rect = ChartArea.PlotAreaPosition.ToRectangleF();
            rect = graph.GetAbsoluteRectangle(rect);

            // Make sure the rectangle width equals rectangle height for the circle
            if (rect.Width != rect.Height)
            {
                if (rect.Width > rect.Height)
                {
                    rect.X += (rect.Width - rect.Height) / 2f;
                    rect.Width = rect.Height;
                }
                else
                {
                    rect.Y += (rect.Height - rect.Width) / 2f;
                    rect.Height = rect.Width;
                }
            }

            // Convert axis position to angle
            float angle = ChartArea.CircularPositionToAngle(position);

            // Set clipping region to the polygon
            Region oldRegion = null;
            if (ChartArea.CircularUsePolygons)
            {
                oldRegion = graph.Clip;
                graph.Clip = new Region(graph.GetPolygonCirclePath(rect, ChartArea.CircularSectorsNumber));
            }

            // Get center point
            PointF centerPoint = graph.GetAbsolutePoint(ChartArea.circularCenter);

            // Set graphics rotation matrix
            Matrix oldMatrix = graph.Transform;
            Matrix newMatrix = oldMatrix.Clone();
            newMatrix.RotateAt(
                angle,
                centerPoint);
            graph.Transform = newMatrix;

            // Draw Line
            PointF endPoint = new PointF(rect.X + rect.Width / 2f, rect.Y);
            graph.DrawLineAbs(color, width, style, centerPoint, endPoint);

            // Process selection regions
            if (this.Common.ProcessModeRegions)
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddLine(centerPoint, endPoint);
                    path.Transform(newMatrix);
                    try
                    {
                        using (Pen pen = new Pen(Color.Black, width + 2))
                        {
                            path.Widen(pen);
                            this.Common.HotRegionsList.AddHotRegion(path, false, ChartElementType.Gridlines, obj);
                        }
                    }
                    catch (OutOfMemoryException)
                    {
                        // GraphicsPath.Widen incorrectly throws OutOfMemoryException
                        // catching here and reacting by not widening
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }

            // Restore graphics
            graph.Transform = oldMatrix;
            newMatrix.Dispose();

            // Restore clip region
            if (ChartArea.CircularUsePolygons)
            {
                graph.Clip = oldRegion;
            }

        }

        /// <summary>
        /// Draws a circular line in circular Common.Chart area.
        /// </summary>
        /// <param name="obj">Object requesting the painting.</param>
        /// <param name="graph">Graphics path.</param>
        /// <param name="color">Line color.</param>
        /// <param name="width">Line width.</param>
        /// <param name="style">Line style.</param>
        /// <param name="position">Line position.</param>
        internal void DrawCircularLine(
            object obj,
            ChartGraphics graph,
            Color color,
            int width,
            ChartDashStyle style,
            float position
            )
        {
            // Create circle position rectangle
            RectangleF rect = ChartArea.PlotAreaPosition.ToRectangleF();
            rect = graph.GetAbsoluteRectangle(rect);

            // Make sure the rectangle width equals rectangle height for the circle
            if (rect.Width != rect.Height)
            {
                if (rect.Width > rect.Height)
                {
                    rect.X += (rect.Width - rect.Height) / 2f;
                    rect.Width = rect.Height;
                }
                else
                {
                    rect.Y += (rect.Height - rect.Width) / 2f;
                    rect.Height = rect.Width;
                }
            }

            // Inflate rectangle
            PointF absPoint = graph.GetAbsolutePoint(new PointF(position, position));
            float rectInflate = absPoint.Y - rect.Top;
            rect.Inflate(-rectInflate, -rectInflate);

            // Create circle pen
            Pen circlePen = new Pen(color, width);
            circlePen.DashStyle = graph.GetPenStyle(style);

            // Draw circle
            if (ChartArea.CircularUsePolygons)
            {
                // Draw eaqula sides polygon
                graph.DrawCircleAbs(circlePen, null, rect, ChartArea.CircularSectorsNumber, false);
            }
            else
            {
                graph.DrawEllipse(circlePen, rect);
            }

            // Process selection regions
            if (this.Common.ProcessModeRegions)
            {
                // Bounding rectangle must be more than 1 pixel by 1 pixel
                if (rect.Width >= 1f && rect.Height > 1)
                {
                    GraphicsPath path = null;
                    try
                    {
                        if (ChartArea.CircularUsePolygons)
                        {
                            path = graph.GetPolygonCirclePath(rect, ChartArea.CircularSectorsNumber);
                        }
                        else
                        {
                            path = new GraphicsPath();
                            path.AddEllipse(rect);
                        }
                        circlePen.Width += 2;
                        path.Widen(circlePen);
                        this.Common.HotRegionsList.AddHotRegion(path, false, ChartElementType.Gridlines, obj);
                    }
                    catch (OutOfMemoryException)
                    {
                        // GraphicsPath.Widen incorrectly throws OutOfMemoryException
                        // catching here and reacting by not widening
                    }
                    catch (ArgumentException)
                    {
                    }
                    finally
                    {
                        path.Dispose();
                    }
                }
            }

        }

        /// <summary>
        /// Draw axis title in 3D.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        private void DrawAxis3DTitle(ChartGraphics graph)
        {
            // Do not draw title if axis is not enabled
            if (!this.enabled)
            {
                return;
            }

            string axisTitle = this.Title;

            // Draw axis title
            PointF rotationCenter = PointF.Empty;
            int angle = 0;

            // Set title alignment
            using (StringFormat format = new StringFormat())
            {
                format.Alignment = this.TitleAlignment;
                format.Trimming = StringTrimming.EllipsisCharacter;
                format.FormatFlags |= StringFormatFlags.LineLimit;

                // Measure title size for non-centered aligment
                SizeF realTitleSize = graph.MeasureString(axisTitle.Replace("\\n", "\n"), this.TitleFont, new SizeF(10000f, 10000f), format, this.GetTextOrientation());
                SizeF axisTitleSize = SizeF.Empty;
                if (format.Alignment != StringAlignment.Center)
                {
                    axisTitleSize = realTitleSize;
                    if (this.IsTextVertical)
                    {
                        // Switch height and width for vertical axis
                        float tempValue = axisTitleSize.Height;
                        axisTitleSize.Height = axisTitleSize.Width;
                        axisTitleSize.Width = tempValue;
                    }

                    // Get relative size
                    axisTitleSize = graph.GetRelativeSize(axisTitleSize);

                    // Change format aligment for the reversed mode
                    if (ChartArea.ReverseSeriesOrder)
                    {
                        if (format.Alignment == StringAlignment.Near)
                        {
                            format.Alignment = StringAlignment.Far;
                        }
                        else
                        {
                            format.Alignment = StringAlignment.Near;
                        }
                    }
                }

                // Set text rotation angle based on the text orientation
                if (this.GetTextOrientation() == TextOrientation.Rotated90)
                {
                    angle = 90;
                }
                else if (this.GetTextOrientation() == TextOrientation.Rotated270)
                {
                    angle = -90;
                }

                // Calculate title center point on the axis 
                if (this.AxisPosition == AxisPosition.Left)
                {
                    rotationCenter = new PointF(ChartArea.PlotAreaPosition.X, ChartArea.PlotAreaPosition.Y + ChartArea.PlotAreaPosition.Height / 2f);
                    if (format.Alignment == StringAlignment.Near)
                    {
                        rotationCenter.Y = ChartArea.PlotAreaPosition.Bottom - axisTitleSize.Height / 2f;
                    }
                    else if (format.Alignment == StringAlignment.Far)
                    {
                        rotationCenter.Y = ChartArea.PlotAreaPosition.Y + axisTitleSize.Height / 2f;
                    }
                }
                else if (this.AxisPosition == AxisPosition.Right)
                {
                    rotationCenter = new PointF(ChartArea.PlotAreaPosition.Right, ChartArea.PlotAreaPosition.Y + ChartArea.PlotAreaPosition.Height / 2f);
                    if (format.Alignment == StringAlignment.Near)
                    {
                        rotationCenter.Y = ChartArea.PlotAreaPosition.Bottom - axisTitleSize.Height / 2f;
                    }
                    else if (format.Alignment == StringAlignment.Far)
                    {
                        rotationCenter.Y = ChartArea.PlotAreaPosition.Y + axisTitleSize.Height / 2f;
                    }
                }
                else if (this.AxisPosition == AxisPosition.Top)
                {
                    rotationCenter = new PointF(ChartArea.PlotAreaPosition.X + ChartArea.PlotAreaPosition.Width / 2f, ChartArea.PlotAreaPosition.Y);
                    if (format.Alignment == StringAlignment.Near)
                    {
                        rotationCenter.X = ChartArea.PlotAreaPosition.X + axisTitleSize.Width / 2f;
                    }
                    else if (format.Alignment == StringAlignment.Far)
                    {
                        rotationCenter.X = ChartArea.PlotAreaPosition.Right - axisTitleSize.Width / 2f;
                    }
                }
                else if (this.AxisPosition == AxisPosition.Bottom)
                {
                    rotationCenter = new PointF(ChartArea.PlotAreaPosition.X + ChartArea.PlotAreaPosition.Width / 2f, ChartArea.PlotAreaPosition.Bottom);
                    if (format.Alignment == StringAlignment.Near)
                    {
                        rotationCenter.X = ChartArea.PlotAreaPosition.X + axisTitleSize.Width / 2f;
                    }
                    else if (format.Alignment == StringAlignment.Far)
                    {
                        rotationCenter.X = ChartArea.PlotAreaPosition.Right - axisTitleSize.Width / 2f;
                    }
                }

                // Transform center of title coordinates and calculate axis angle
                bool isOnEdge = false;
                float zPosition = this.GetMarksZPosition(out isOnEdge);
                Point3D[] rotationCenterPoints = null;
                float angleAxis = 0;
                if (this.AxisPosition == AxisPosition.Top || this.AxisPosition == AxisPosition.Bottom)
                {
                    rotationCenterPoints = new Point3D[] { 
					new Point3D(rotationCenter.X, rotationCenter.Y, zPosition),
					new Point3D(rotationCenter.X - 20f, rotationCenter.Y, zPosition) };

                    // Transform coordinates of text rotation point
                    ChartArea.matrix3D.TransformPoints(rotationCenterPoints);
                    rotationCenter = rotationCenterPoints[0].PointF;

                    // Get absolute coordinates 
                    rotationCenterPoints[0].PointF = graph.GetAbsolutePoint(rotationCenterPoints[0].PointF);
                    rotationCenterPoints[1].PointF = graph.GetAbsolutePoint(rotationCenterPoints[1].PointF);

                    // Calculate X axis angle
                    angleAxis = (float)Math.Atan(
                        (rotationCenterPoints[1].Y - rotationCenterPoints[0].Y) /
                        (rotationCenterPoints[1].X - rotationCenterPoints[0].X));
                }
                else
                {
                    rotationCenterPoints = new Point3D[] { 
					new Point3D(rotationCenter.X, rotationCenter.Y, zPosition),
					new Point3D(rotationCenter.X, rotationCenter.Y - 20f, zPosition) };

                    // Transform coordinates of text rotation point
                    ChartArea.matrix3D.TransformPoints(rotationCenterPoints);
                    rotationCenter = rotationCenterPoints[0].PointF;

                    // Get absolute coordinates 
                    rotationCenterPoints[0].PointF = graph.GetAbsolutePoint(rotationCenterPoints[0].PointF);
                    rotationCenterPoints[1].PointF = graph.GetAbsolutePoint(rotationCenterPoints[1].PointF);

                    // Calculate Y axis angle
                    if (rotationCenterPoints[1].Y != rotationCenterPoints[0].Y)
                    {
                        angleAxis = -(float)Math.Atan(
                            (rotationCenterPoints[1].X - rotationCenterPoints[0].X) /
                            (rotationCenterPoints[1].Y - rotationCenterPoints[0].Y));
                    }
                }
                angle += (int)Math.Round(angleAxis * 180f / (float)Math.PI);


                // Calculate title center offset from the axis line
                float offset = this.labelSize + this.markSize + this.titleSize / 2f;
                float dX = 0f, dY = 0f;


                // Adjust center of title with labels, marker and title size
                if (this.AxisPosition == AxisPosition.Left)
                {
                    dX = (float)(offset * Math.Cos(angleAxis));
                    rotationCenter.X -= dX;
                }
                else if (this.AxisPosition == AxisPosition.Right)
                {
                    dX = (float)(offset * Math.Cos(angleAxis));
                    rotationCenter.X += dX;
                }
                else if (this.AxisPosition == AxisPosition.Top)
                {
                    dY = (float)(offset * Math.Cos(angleAxis));
                    dX = (float)(offset * Math.Sin(angleAxis));
                    rotationCenter.Y -= dY;
                    if (dY > 0)
                    {
                        rotationCenter.X += dX;
                    }
                    else
                    {
                        rotationCenter.X -= dX;
                    }
                }
                else if (this.AxisPosition == AxisPosition.Bottom)
                {
                    dY = (float)(offset * Math.Cos(angleAxis));
                    dX = (float)(offset * Math.Sin(angleAxis));
                    rotationCenter.Y += dY;
                    if (dY > 0)
                    {
                        rotationCenter.X -= dX;
                    }
                    else
                    {
                        rotationCenter.X += dX;
                    }
                }


                // Always align text in the center
                format.LineAlignment = StringAlignment.Center;
                format.Alignment = StringAlignment.Center;
                // SQL VSTS Fix #259954, Dev10: 591135 Windows 7 crashes on empty transformation.
                if (rotationCenter.IsEmpty || float.IsNaN(rotationCenter.X) || float.IsNaN(rotationCenter.Y))
                {
                    return;
                }

                // Draw 3D title
                using (Brush brush = new SolidBrush(this.TitleForeColor))
                {
                    graph.DrawStringRel(
                        axisTitle.Replace("\\n", "\n"),
                        this.TitleFont,
                        brush,
                        rotationCenter,
                        format,
                        angle,
                        this.GetTextOrientation());
                }

                // Add hot region
                if (Common.ProcessModeRegions)
                {
                    using (GraphicsPath hotPath = graph.GetTranformedTextRectPath(rotationCenter, realTitleSize, angle))
                    {
                        this.Common.HotRegionsList.AddHotRegion(hotPath, false, ChartElementType.AxisTitle, this);
                    }
                }
            }

        }

        /// <summary>
        /// Select Axis line
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics</param>
        /// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
        internal void DrawAxisLine(ChartGraphics graph, bool backElements)
        {
            Axis opositeAxis;
            ArrowOrientation arrowOrientation = ArrowOrientation.Top;
            PointF first = Point.Empty;
            PointF second = Point.Empty;

            // Set the position of axis
            switch (AxisPosition)
            {

                case AxisPosition.Left:

                    first.X = (float)GetAxisPosition();
                    first.Y = PlotAreaPosition.Bottom;
                    second.X = (float)GetAxisPosition();
                    second.Y = PlotAreaPosition.Y;
                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Bottom;
                    else
                        arrowOrientation = ArrowOrientation.Top;

                    break;

                case AxisPosition.Right:

                    first.X = (float)GetAxisPosition();
                    first.Y = PlotAreaPosition.Bottom;
                    second.X = (float)GetAxisPosition();
                    second.Y = PlotAreaPosition.Y;
                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Bottom;
                    else
                        arrowOrientation = ArrowOrientation.Top;

                    break;

                case AxisPosition.Bottom:

                    first.X = PlotAreaPosition.X;
                    first.Y = (float)GetAxisPosition();
                    second.X = PlotAreaPosition.Right;
                    second.Y = (float)GetAxisPosition();
                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Left;
                    else
                        arrowOrientation = ArrowOrientation.Right;

                    break;

                case AxisPosition.Top:

                    first.X = PlotAreaPosition.X;
                    first.Y = (float)GetAxisPosition();
                    second.X = PlotAreaPosition.Right;
                    second.Y = (float)GetAxisPosition();
                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Left;
                    else
                        arrowOrientation = ArrowOrientation.Right;

                    break;

            }

            // Update axis line position for circular area
            if (ChartArea.chartAreaIsCurcular)
            {
                first.Y = PlotAreaPosition.Y + PlotAreaPosition.Height / 2f;
            }

            
            if (Common.ProcessModePaint)
            {
                if (!ChartArea.Area3DStyle.Enable3D || ChartArea.chartAreaIsCurcular)
                {

					// Start Svg/Flash Selection mode
					graph.StartHotRegion( this._url, _toolTip );

                    // Draw the line
                    graph.DrawLineRel(_lineColor, _lineWidth, _lineDashStyle, first, second);

					// End Svg/Flash Selection mode
					graph.EndHotRegion( );

                    // Opposite axis. Arrow uses this axis to find 
                    // a shift from Common.Chart area border. This shift 
                    // depend on Tick mark size.
                    switch (arrowOrientation)
                    {
                        case ArrowOrientation.Left:
                            opositeAxis = ChartArea.AxisX;
                            break;
                        case ArrowOrientation.Right:
                            opositeAxis = ChartArea.AxisX2;
                            break;
                        case ArrowOrientation.Top:
                            opositeAxis = ChartArea.AxisY2;
                            break;
                        case ArrowOrientation.Bottom:
                            opositeAxis = ChartArea.AxisY;
                            break;
                        default:
                            opositeAxis = ChartArea.AxisX;
                            break;
                    }

                    // Draw arrow
                    PointF arrowPosition;
                    if (isReversed)
                        arrowPosition = first;
                    else
                        arrowPosition = second;

                    // Draw Arrow
                    graph.DrawArrowRel(arrowPosition, arrowOrientation, _arrowStyle, _lineColor, _lineWidth, _lineDashStyle, opositeAxis.majorTickMark.Size, _lineWidth);
                }
                else
                {
                    Draw3DAxisLine(graph, first, second, (this.AxisPosition == AxisPosition.Top || this.AxisPosition == AxisPosition.Bottom), backElements);
                }
            }

        }


        /// <summary>
        /// Draws the axis line hot region.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="backElements">set to <c>true</c> if we draw back elements.</param>
        private void DrawAxisLineHotRegion(ChartGraphics graph, bool backElements)
        {
            if (Common.ProcessModeRegions)
            {
                //VSTS #229835: During the 3D rendering the axis is drawn twice: 
                //1. In PrePaint() both axis and backelements (labels) are drawn.
                //2. In Paint() the axis is redrawn without labels and as a result it creates a second hot region which covered the labels' hotregions. 
                //In order to avoid this we have to suppress the hotregion drawing in the Paint using the backElements flag (it's false during the Paint)
                //The circular charts and 2D charts are drawn only once in Paint() so we draw the hot regions.
                if (backElements || !ChartArea.Area3DStyle.Enable3D || ChartArea.chartAreaIsCurcular)
                {
                    DrawAxisLineHotRegion(graph);
                }
            }

        }

        /// <summary>
        /// Adds the axis hot region
        /// </summary>
        /// <param name="graph">The chart graphics instance.</param>
        private void DrawAxisLineHotRegion(ChartGraphics graph)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                // Find the topLeft(first) and bottomRight(second) points of the hotregion rectangle
                PointF first = PointF.Empty;
                PointF second = PointF.Empty;
                float axisPosition = (float)GetAxisPosition();

                switch (this.AxisPosition)
                {
                    case AxisPosition.Left:
                        first.X = axisPosition - (labelSize + markSize);
                        first.Y = PlotAreaPosition.Y;
                        second.X = axisPosition;
                        second.Y = PlotAreaPosition.Bottom;
                        break;

                    case AxisPosition.Right:
                        first.X = axisPosition;
                        first.Y = PlotAreaPosition.Y;
                        second.X = axisPosition + labelSize + markSize;
                        second.Y = PlotAreaPosition.Bottom;
                        break;

                    case AxisPosition.Bottom:
                        first.X = PlotAreaPosition.X;
                        first.Y = axisPosition;
                        second.X = PlotAreaPosition.Right;
                        second.Y = axisPosition + labelSize + markSize;
                        break;

                    case AxisPosition.Top:
                        first.X = PlotAreaPosition.X;
                        first.Y = axisPosition - (labelSize + markSize);
                        second.X = PlotAreaPosition.Right;
                        second.Y = axisPosition;
                        break;
                }

                // Update axis line position for circular area
                if (ChartArea.chartAreaIsCurcular)
                {
                    second.Y = PlotAreaPosition.Y + PlotAreaPosition.Height / 2f;
                }
                
                // Create rectangle and inflate it
                RectangleF rect = new RectangleF(first.X, first.Y, second.X - first.X, second.Y - first.Y);
                SizeF size = graph.GetRelativeSize(new SizeF(3, 3));

                if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
                {
                    rect.Inflate(2, size.Height);
                }
                else
                {
                    rect.Inflate(size.Width, 2);
                }

                // Get the rectangle points
                PointF[] points = new PointF[] {
                    new PointF(rect.Left, rect.Top),
                    new PointF(rect.Right, rect.Top), 
                    new PointF(rect.Right, rect.Bottom), 
                    new PointF(rect.Left, rect.Bottom)};

                // If we are dealing with the 3D - transform the rectangle
                if (ChartArea.Area3DStyle.Enable3D && !ChartArea.chartAreaIsCurcular)
                {
                    Boolean axisOnEdge = false;
                    float zPositon = GetMarksZPosition(out axisOnEdge);

                    // Convert points to 3D
                    Point3D[] points3D = new Point3D[points.Length];
                    for (int i = 0; i < points.Length; i++)
                    {
                        points3D[i] = new Point3D(points[i].X, points[i].Y, zPositon);
                    }

                    // Transform
                    ChartArea.matrix3D.TransformPoints(points3D);

                    // Convert to 2D
                    for (int i = 0; i < points3D.Length; i++)
                    {
                        points[i] = points3D[i].PointF;
                    }
                }

                // Transform points to absolute cooordinates
                for (int i = 0; i < points.Length; i++)
                {
                    points[i] = graph.GetAbsolutePoint(points[i]);
                }

                // Add the points to the path
                path.AddPolygon(points);


#if Microsoft_CONTROL
				Common.HotRegionsList.AddHotRegion( 
					graph, 
					path, 
					false, 
					this._toolTip,
					string.Empty,
					string.Empty,
					string.Empty,
					this,
					ChartElementType.Axis);
#else
                Common.HotRegionsList.AddHotRegion(
                    graph,
                    path,
                    false,
                    this._toolTip,
                    this._url,
                    this._mapAreaAttributes,
                    this.PostBackValue,
                    this,
                    ChartElementType.Axis);
#endif 
            
            }
        }


        /// <summary>
        /// Draws axis line in 3D space.
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object.</param>
        /// <param name="point1">First line point.</param>
        /// <param name="point2">Second line point.</param>
        /// <param name="horizontal">Indicates that tick mark line is horizontal</param>
        /// <param name="backElements">Only back elements of axis should be drawn.</param>
        private void Draw3DAxisLine(
            ChartGraphics graph,
            PointF point1,
            PointF point2,
            bool horizontal,
            bool backElements
            )
        {
            // Check if axis is positioned on the plot area adge
            bool onEdge = this.IsAxisOnAreaEdge;

            // Check if axis tick marks are drawn inside plotting area
            bool tickMarksOnEdge = onEdge;
            if (tickMarksOnEdge &&
                this.MajorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis ||
                this.MajorTickMark.TickMarkStyle == TickMarkStyle.InsideArea ||
                this.MinorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis ||
                this.MinorTickMark.TickMarkStyle == TickMarkStyle.InsideArea)
            {
                tickMarksOnEdge = false;
            }

            // Make sure first point of axis coordinates has smaller values
            if ((horizontal && point1.X > point2.X) ||
                (!horizontal && point1.Y > point2.Y))
            {
                PointF tempPoint = new PointF(point1.X, point1.Y);
                point1.X = point2.X;
                point1.Y = point2.Y;
                point2 = tempPoint;
            }

            // Check if the front/back wall is on the top drawing layer
            float zPositon = ChartArea.IsMainSceneWallOnFront() ? ChartArea.areaSceneDepth : 0f;
            SurfaceNames surfName = ChartArea.IsMainSceneWallOnFront() ? SurfaceNames.Front : SurfaceNames.Back;
            if (ChartArea.ShouldDrawOnSurface(SurfaceNames.Back, backElements, tickMarksOnEdge))
            {

				// Start Svg Selection mode
				graph.StartHotRegion( this._url, _toolTip );

                // Draw axis line on the back/front wall
                graph.Draw3DLine(
                    ChartArea.matrix3D,
                    _lineColor, _lineWidth, _lineDashStyle,
                    new Point3D(point1.X, point1.Y, zPositon),
                    new Point3D(point2.X, point2.Y, zPositon),
                    Common,
                    this,
                    ChartElementType.Nothing
                    );

				// End Svg Selection mode
				graph.EndHotRegion();

            }

            // Check if the back wall is on the top drawing layer
            zPositon = ChartArea.IsMainSceneWallOnFront() ? 0f : ChartArea.areaSceneDepth;
            surfName = ChartArea.IsMainSceneWallOnFront() ? SurfaceNames.Back : SurfaceNames.Front;
            if (ChartArea.ShouldDrawOnSurface(surfName, backElements, tickMarksOnEdge))
            {
                // Draw axis line on the front wall
                if (!onEdge ||
                    (this.AxisPosition == AxisPosition.Bottom && ChartArea.IsBottomSceneWallVisible()) ||
                    (this.AxisPosition == AxisPosition.Left && ChartArea.IsSideSceneWallOnLeft()) ||
                    (this.AxisPosition == AxisPosition.Right && !ChartArea.IsSideSceneWallOnLeft()))
                {

					// Start Svg Selection mode
					graph.StartHotRegion( this._url, _toolTip );

                    graph.Draw3DLine(
                        ChartArea.matrix3D,
                        _lineColor, _lineWidth, _lineDashStyle,
                        new Point3D(point1.X, point1.Y, zPositon),
                        new Point3D(point2.X, point2.Y, zPositon),
                        Common,
                        this,
                        ChartElementType.Nothing
                        );

					// End Svg Selection mode
					graph.EndHotRegion();

                }
            }

            // Check if the left/top wall is on the top drawing layer
            SurfaceNames surfaceName = (this.AxisPosition == AxisPosition.Left || this.AxisPosition == AxisPosition.Right) ? SurfaceNames.Top : SurfaceNames.Left;
            if (ChartArea.ShouldDrawOnSurface(surfaceName, backElements, tickMarksOnEdge))
            {
                // Draw axis line on the left/top side walls
                if (!onEdge ||
                    (this.AxisPosition == AxisPosition.Bottom && (ChartArea.IsBottomSceneWallVisible() || ChartArea.IsSideSceneWallOnLeft())) ||
                    (this.AxisPosition == AxisPosition.Left && ChartArea.IsSideSceneWallOnLeft()) ||
                    (this.AxisPosition == AxisPosition.Right && !ChartArea.IsSideSceneWallOnLeft()) ||
                    (this.AxisPosition == AxisPosition.Top && ChartArea.IsSideSceneWallOnLeft()))
                {

					// Start Svg Selection mode
					graph.StartHotRegion( this._url, _toolTip );

                    graph.Draw3DLine(
                        ChartArea.matrix3D,
                        _lineColor, _lineWidth, _lineDashStyle,
                        new Point3D(point1.X, point1.Y, ChartArea.areaSceneDepth),
                        new Point3D(point1.X, point1.Y, 0f),
                        Common,
                        this,
                        ChartElementType.Nothing
                    );

					// End Svg Selection mode
					graph.EndHotRegion( );

                }
            }

            // Check if the right/bottom wall is on the top drawing layer
            surfaceName = (this.AxisPosition == AxisPosition.Left || this.AxisPosition == AxisPosition.Right) ? SurfaceNames.Bottom : SurfaceNames.Right;
            if (ChartArea.ShouldDrawOnSurface(surfaceName, backElements, tickMarksOnEdge))
            {
                // Draw axis line on the bottom/right side walls
                if (!onEdge ||
                    (this.AxisPosition == AxisPosition.Bottom && (ChartArea.IsBottomSceneWallVisible() || !ChartArea.IsSideSceneWallOnLeft())) ||
                    (this.AxisPosition == AxisPosition.Left && (ChartArea.IsSideSceneWallOnLeft() || ChartArea.IsBottomSceneWallVisible())) ||
                    (this.AxisPosition == AxisPosition.Right && (!ChartArea.IsSideSceneWallOnLeft() || ChartArea.IsBottomSceneWallVisible())) ||
                    (this.AxisPosition == AxisPosition.Top && !ChartArea.IsSideSceneWallOnLeft())
                    )
                {

					// Start Svg Selection mode
					graph.StartHotRegion( this._url, _toolTip );

                    graph.Draw3DLine(
                        ChartArea.matrix3D,
                        _lineColor, _lineWidth, _lineDashStyle,
                        new Point3D(point2.X, point2.Y, ChartArea.areaSceneDepth),
                        new Point3D(point2.X, point2.Y, 0f),
                        Common,
                        this,
                        ChartElementType.Nothing
                        );

					// End Svg Selection mode
					graph.EndHotRegion();

                }
            }

        }

        /// <summary>
        /// Gets Z position of axis tick marks and labels.
        /// </summary>
        /// <param name="axisOnEdge">Returns true if axis is on the edge.</param>
        /// <returns>Marks Z position.</returns>
        internal float GetMarksZPosition(out bool axisOnEdge)
        {
            axisOnEdge = this.IsAxisOnAreaEdge;
            if (!this.GetIsMarksNextToAxis())
            {
                // Marks are forced to be on the area edge
                axisOnEdge = true;
            }
            float wallZPosition = 0f;
            if (this.AxisPosition == AxisPosition.Bottom && (ChartArea.IsBottomSceneWallVisible() || !axisOnEdge))
            {
                wallZPosition = ChartArea.areaSceneDepth;
            }
            if (this.AxisPosition == AxisPosition.Left && (ChartArea.IsSideSceneWallOnLeft() || !axisOnEdge))
            {
                wallZPosition = ChartArea.areaSceneDepth;
            }
            if (this.AxisPosition == AxisPosition.Right && (!ChartArea.IsSideSceneWallOnLeft() || !axisOnEdge))
            {
                wallZPosition = ChartArea.areaSceneDepth;
            }
            if (this.AxisPosition == AxisPosition.Top && !axisOnEdge)
            {
                wallZPosition = ChartArea.areaSceneDepth;
            }

            // Check if front wall is shown
            if (ChartArea.IsMainSceneWallOnFront())
            {
                // Switch Z position of tick mark
                wallZPosition = (wallZPosition == 0f) ? ChartArea.areaSceneDepth : 0f;
            }

            return wallZPosition;
        }

        /// <summary>
        /// Paint Axis Grid lines
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        internal void PaintGrids(ChartGraphics graph)
        {
            object obj;

            PaintGrids(graph, out obj);

        }

        /// <summary>
        /// Paint Axis Grid lines or 
        /// hit test function for grid lines
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        /// <param name="obj">Returns selected grid object</param>
        internal void PaintGrids(ChartGraphics graph, out object obj)
        {
            obj = null;

#if SUBAXES
			// Paint grids of sub-axis
			if(!ChartArea.Area3DStyle.Enable3D && 
				!ChartArea.chartAreaIsCurcular)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.PaintGrids( graph, out obj);
				}
			}
#endif // SUBAXES

            // Axis is disabled
            if (enabled == false)
                return;

            // Paint Minor grid lines
            minorGrid.Paint(graph);

            // Paint Major grid lines
            majorGrid.Paint(graph);
        }

        /// <summary>
        /// Paint Axis Strip lines
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        /// <param name="drawLinesOnly">Indicates if Lines or Stripes should be drawn.</param>
        internal void PaintStrips(ChartGraphics graph, bool drawLinesOnly)
        {
            object obj;
            PaintStrips(graph, false, 0, 0, out obj, drawLinesOnly);
        }

        /// <summary>
        /// Paint Axis Strip lines or 
        /// hit test function for Strip lines
        /// </summary>
        /// <param name="graph">Reference to the Chart Graphics object</param>
        /// <param name="selectionMode">The selection mode is active</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="obj">Returns selected grid object</param>
        /// <param name="drawLinesOnly">Indicates if Lines or Stripes should be drawn.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "y"),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "x"), 
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "selectionMode")]
        internal void PaintStrips(ChartGraphics graph, bool selectionMode, int x, int y, out object obj, bool drawLinesOnly)
        {
            obj = null;

#if SUBAXES
			// Paint strips of sub-axis
			if(ChartArea.IsSubAxesSupported)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.PaintStrips( graph, selectionMode, x, y, out obj, drawLinesOnly);
				}
			}
#endif // SUBAXES

            // Axis is disabled
            if (enabled == false)
                return;

            // Add axis isInterlaced strip lines into the collection
            bool interlacedStripAdded = AddInterlacedStrip();

            // Draw axis strips and lines
            foreach (StripLine strip in this.StripLines)
            {
                strip.Paint(graph, this.Common, drawLinesOnly);
            }

            // Remove axis isInterlaced strip line from the collection after drawing
            if (interlacedStripAdded)
            {
                // Remove isInterlaced strips which always is the first strip line
                this.StripLines.RemoveAt(0);
            }

        }

        /// <summary>
        /// Helper function which adds temp. strip lines into the collection
        /// to display isInterlaced lines in axis.
        /// </summary>
        private bool AddInterlacedStrip()
        {
            bool addStrip = false;
            if (this.IsInterlaced)
            {
                StripLine stripLine = new StripLine();
                stripLine.interlaced = true;
                // VSTS fix of 164115 IsInterlaced StripLines with no border are rendered with black border, regression of VSTS 136763
                stripLine.BorderColor = Color.Empty;

                // Get interval from grid lines, tick marks or labels
                if (this.MajorGrid.Enabled && this.MajorGrid.GetInterval() != 0.0)
                {
                    addStrip = true;
                    stripLine.Interval = this.MajorGrid.GetInterval() * 2.0;
                    stripLine.IntervalType = this.MajorGrid.GetIntervalType();
                    stripLine.IntervalOffset = this.MajorGrid.GetIntervalOffset();
                    stripLine.IntervalOffsetType = this.MajorGrid.GetIntervalOffsetType();
                    stripLine.StripWidth = this.MajorGrid.GetInterval();
                    stripLine.StripWidthType = this.MajorGrid.GetIntervalType();
                }
                else if (this.MajorTickMark.Enabled && this.MajorTickMark.GetInterval() != 0.0)
                {
                    addStrip = true;
                    stripLine.Interval = this.MajorTickMark.GetInterval() * 2.0;
                    stripLine.IntervalType = this.MajorTickMark.GetIntervalType();
                    stripLine.IntervalOffset = this.MajorTickMark.GetIntervalOffset();
                    stripLine.IntervalOffsetType = this.MajorTickMark.GetIntervalOffsetType();
                    stripLine.StripWidth = this.MajorTickMark.GetInterval();
                    stripLine.StripWidthType = this.MajorTickMark.GetIntervalType();
                }
                else if (this.LabelStyle.Enabled && this.LabelStyle.GetInterval() != 0.0)
                {
                    addStrip = true;
                    stripLine.Interval = this.LabelStyle.GetInterval() * 2.0;
                    stripLine.IntervalType = this.LabelStyle.GetIntervalType();
                    stripLine.IntervalOffset = this.LabelStyle.GetIntervalOffset();
                    stripLine.IntervalOffsetType = this.LabelStyle.GetIntervalOffsetType();
                    stripLine.StripWidth = this.LabelStyle.GetInterval();
                    stripLine.StripWidthType = this.LabelStyle.GetIntervalType();
                }

                // Insert item into the strips collection
                if (addStrip)
                {
                    // Define stip color
                    if (this.InterlacedColor != Color.Empty)
                    {
                        stripLine.BackColor = this.InterlacedColor;
                    }
                    else
                    {
                        // If isInterlaced strips color is not set - use darker color of the area
                        if (ChartArea.BackColor == Color.Empty)
                        {
                            stripLine.BackColor = (ChartArea.Area3DStyle.Enable3D) ? Color.DarkGray : Color.LightGray;
                        }
                        else if (ChartArea.BackColor == Color.Transparent)
                        {
                            if (Common.Chart.BackColor != Color.Transparent && Common.Chart.BackColor != Color.Black)
                            {
                                stripLine.BackColor = ChartGraphics.GetGradientColor(Common.Chart.BackColor, Color.Black, 0.2);
                            }
                            else
                            {
                                stripLine.BackColor = Color.LightGray;
                            }
                        }
                        else
                        {
                            stripLine.BackColor = ChartGraphics.GetGradientColor(ChartArea.BackColor, Color.Black, 0.2);
                        }
                    }

                    // Insert strip
                    this.StripLines.Insert(0, stripLine);
                }
            }

            return addStrip;
        }

        #endregion

        #region Axis parameters recalculation and resizing methods

        /// <summary>
        /// This method will calculate the maximum and minimum values 
        /// using interval on the X axis automatically. It will make a gap between 
        /// data points and border of the Common.Chart area.
        /// Note that this method can only be called for primary or secondary X axes.
        /// </summary>
        public void RoundAxisValues()
        {
            this.roundedXValues = true;
        }

        /// <summary>
        /// RecalculateAxesScale axis.
        /// </summary>
        /// <param name="position">Plotting area position.</param>
        internal void ReCalc(ElementPosition position)
        {
            PlotAreaPosition = position;

#if SUBAXES

			// Recalculate all sub-axis
			foreach(SubAxis subAxis in this.SubAxes)
			{
				subAxis.ReCalc( position );
			}
#endif // SUBAXES
        }

        /// <summary>
        /// This method store Axis values as minimum, maximum, 
        /// crossing, etc. Axis auto algorithm changes these 
        /// values and they have to be set to default values 
        /// after painting.
        /// </summary>
        internal void StoreAxisValues()
        {
            tempLabels = new CustomLabelsCollection(this);
            foreach (CustomLabel label in CustomLabels)
            {
                tempLabels.Add(label.Clone());
            }

            paintMode = true;

            // This field synchronies the Storing and 
            // resetting of temporary values
            if (_storeValuesEnabled)
            {

                tempMaximum = maximum;
                tempMinimum = minimum;
                tempCrossing = crossing;
                tempAutoMinimum = _autoMinimum;
                tempAutoMaximum = _autoMaximum;

                tempMajorGridInterval = majorGrid.interval;
                tempMajorTickMarkInterval = majorTickMark.interval;

                tempMinorGridInterval = minorGrid.interval;
                tempMinorTickMarkInterval = minorTickMark.interval;


                tempGridIntervalType = majorGrid.intervalType;
                tempTickMarkIntervalType = majorTickMark.intervalType;


                tempLabelInterval = labelStyle.interval;
                tempLabelIntervalType = labelStyle.intervalType;

                // Remember original ScaleView Position
                this._originalViewPosition = this.ScaleView.Position;

                // This field synchronies the Storing and 
                // resetting of temporary values
                _storeValuesEnabled = false;
            }

#if SUBAXES

			// Store values of all sub-axis
			if(ChartArea.IsSubAxesSupported)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.StoreAxisValues( );
				}
			}
#endif // SUBAXES

        }


        /// <summary>
        /// This method reset Axis values as minimum, maximum, 
        /// crossing, etc. Axis auto algorithm changes these 
        /// values and they have to be set to default values 
        /// after painting.
        /// </summary>
        internal void ResetAxisValues()
        {
            // Paint mode is finished
            paintMode = false;

#if Microsoft_CONTROL
			if(Common.Chart == null)
			{
#if SUBAXES
				else if(this is SubAxis)
				{
					if( ((SubAxis)this).parentAxis != null)
					{
						this.Common = ((SubAxis)this).parentAxis.Common;
						Common.Chart = ((SubAxis)this).parentAxis.Common.Chart;
					}
				}
#endif // SUBAXES
            }
			if(Common.Chart != null && Common.Chart.Site != null && Common.Chart.Site.DesignMode)
			{
				ResetAutoValues();
			}
#else
            ResetAutoValues();
#endif

            // Reset back original custom labels
            if (tempLabels != null)
            {
                CustomLabels.Clear();
                foreach (CustomLabel label in tempLabels)
                {
                    CustomLabels.Add(label.Clone());
                }

                tempLabels = null;
            }

#if SUBAXES

			// Reset values of all sub-axis
			if(ChartArea.IsSubAxesSupported)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.ResetAxisValues( );
				}
			}
#endif // SUBAXES
        }


        /// <summary>
        /// Reset auto calculated axis values
        /// </summary>
        internal void ResetAutoValues()
        {
            refreshMinMaxFromData = true;
            maximum = tempMaximum;
            minimum = tempMinimum;
            crossing = tempCrossing;
            _autoMinimum = tempAutoMinimum;
            _autoMaximum = tempAutoMaximum;

            majorGrid.interval = tempMajorGridInterval;
            majorTickMark.interval = tempMajorTickMarkInterval;

            minorGrid.interval = tempMinorGridInterval;
            minorTickMark.interval = tempMinorTickMarkInterval;


            labelStyle.interval = tempLabelInterval;
            majorGrid.intervalType = tempGridIntervalType;
            majorTickMark.intervalType = tempTickMarkIntervalType;
            labelStyle.intervalType = tempLabelIntervalType;

            // Restore original ScaleView Position
            if (Common.Chart != null)
            {
                if (!Common.Chart.serializing)
                {
                    this.ScaleView.Position = this._originalViewPosition;
                }
            }

            // This field synchronies the Storing and 
            // resetting of temporary values
            _storeValuesEnabled = true;

#if SUBAXES

			// Reset auto values of all sub-axis
			if(ChartArea.IsSubAxesSupported)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.ResetAutoValues( );
				}
			}
#endif // SUBAXES

        }

        /// <summary>
        /// Calculate size of the axis elements like title, labels and marks.
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="chartAreaPosition">The Chart area position.</param>
        /// <param name="plotArea">Plotting area size.</param>
        /// <param name="axesNumber">Number of axis of the same orientation.</param>
        /// <param name="autoPlotPosition">Indicates that inner plot position is automatic.</param>
        virtual internal void Resize(
            ChartGraphics chartGraph,
            ElementPosition chartAreaPosition,
            RectangleF plotArea,
            float axesNumber,
            bool autoPlotPosition)
        {
#if SUBAXES
			// Resize all sub-axis
			if(ChartArea.IsSubAxesSupported)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.Resize(chartGraph, chartAreaPosition, plotArea, axesNumber, autoPlotPosition);
				}
			}
#endif // SUBAXES


#if Microsoft_CONTROL
            // Disable Common.Chart invalidation
            bool oldDisableInvalidates = Common.Chart.disableInvalidates;
			Common.Chart.disableInvalidates = true;
#endif //Microsoft_CONTROL

            // Set Common.Chart area position
            PlotAreaPosition = chartAreaPosition;

            // Initialize plot area size
            PlotAreaPosition.FromRectangleF(plotArea);

            //******************************************************
            //** Calculate axis title size
            //******************************************************
            this.titleSize = 0F;
            if (this.Title.Length > 0)
            {
                // Measure axis title
                SizeF titleStringSize = chartGraph.MeasureStringRel(this.Title.Replace("\\n", "\n"), this.TitleFont, new SizeF(10000f, 10000f), StringFormat.GenericTypographic, this.GetTextOrientation());

                // Switch Width & Heigth for vertical axes
                // If axis is horizontal
                float maxTitlesize = 0;
                if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                {
                    maxTitlesize = (plotArea.Height / 100F) * (Axis.maxAxisTitleSize / axesNumber);
                    if (this.IsTextVertical)
                    {
                        this.titleSize = Math.Min(titleStringSize.Width, maxTitlesize);
                    }
                    else
                    {
                        this.titleSize = Math.Min(titleStringSize.Height, maxTitlesize);
                    }
                }
                // If axis is vertical
                else
                {
					titleStringSize = chartGraph.GetAbsoluteSize(titleStringSize);
					titleStringSize = chartGraph.GetRelativeSize(new SizeF(titleStringSize.Height, titleStringSize.Width));
					maxTitlesize = (plotArea.Width / 100F) * (Axis.maxAxisTitleSize / axesNumber);
                    if (this.IsTextVertical)
                    {
						this.titleSize = Math.Min(titleStringSize.Width, maxTitlesize);
                    }
                    else
                    {
                        this.titleSize = Math.Min(titleStringSize.Height, maxTitlesize);
                    }
                }
            }
            if (this.titleSize > 0)
            {
                this.titleSize += elementSpacing;
            }

            //*********************************************************
            //** Get arrow size of the opposite axis
            //*********************************************************
            float arrowSize = 0F;
            SizeF arrowSizePrimary = SizeF.Empty;
            SizeF arrowSizeSecondary = SizeF.Empty;
            ArrowOrientation arrowOrientation = ArrowOrientation.Bottom;
            if (this.axisType == AxisName.X || this.axisType == AxisName.X2)
            {
                if (ChartArea.AxisY.ArrowStyle != AxisArrowStyle.None)
                {
                    arrowSizePrimary = ChartArea.AxisY.GetArrowSize(out arrowOrientation);
                    if (!IsArrowInAxis(arrowOrientation, this.AxisPosition))
                    {
                        arrowSizePrimary = SizeF.Empty;
                    }
                }

                if (ChartArea.AxisY2.ArrowStyle != AxisArrowStyle.None)
                {
                    arrowSizeSecondary = ChartArea.AxisY2.GetArrowSize(out arrowOrientation);
                    if (!IsArrowInAxis(arrowOrientation, this.AxisPosition))
                    {
                        arrowSizeSecondary = SizeF.Empty;
                    }
                }
            }
            else
            {
                if (ChartArea.AxisX.ArrowStyle != AxisArrowStyle.None)
                {
                    arrowSizePrimary = ChartArea.AxisX.GetArrowSize(out arrowOrientation);
                    if (!IsArrowInAxis(arrowOrientation, this.AxisPosition))
                    {
                        arrowSizePrimary = SizeF.Empty;
                    }
                }

                if (ChartArea.AxisX2.ArrowStyle != AxisArrowStyle.None)
                {
                    arrowSizeSecondary = ChartArea.AxisX2.GetArrowSize(out arrowOrientation);
                    if (!IsArrowInAxis(arrowOrientation, this.AxisPosition))
                    {
                        arrowSizeSecondary = SizeF.Empty;
                    }
                }
            }

            // If axis is horizontal
            if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
            {
                arrowSize = Math.Max(arrowSizePrimary.Height, arrowSizeSecondary.Height);
            }
            // If axis is vertical
            else
            {
                arrowSize = Math.Max(arrowSizePrimary.Width, arrowSizeSecondary.Width);
            }

            //*********************************************************
            //** Calculate axis tick marks, axis thickness, arrow size
            //** and scroll bar size
            //*********************************************************
            this.markSize = 0F;

            // Get major and minor tick marks sizes
            float majorTickSize = 0;
            if (this.MajorTickMark.Enabled && this.MajorTickMark.TickMarkStyle != TickMarkStyle.None)
            {
                if (this.MajorTickMark.TickMarkStyle == TickMarkStyle.InsideArea)
                {
                    majorTickSize = 0F;
                }
                else if (this.MajorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis)
                {
                    majorTickSize = this.MajorTickMark.Size / 2F;
                }
                else if (this.MajorTickMark.TickMarkStyle == TickMarkStyle.OutsideArea)
                {
                    majorTickSize = this.MajorTickMark.Size;
                }
            }

            float minorTickSize = 0;
            if (this.MinorTickMark.Enabled && this.MinorTickMark.TickMarkStyle != TickMarkStyle.None && this.MinorTickMark.GetInterval() != 0)
            {
                if (this.MinorTickMark.TickMarkStyle == TickMarkStyle.InsideArea)
                {
                    minorTickSize = 0F;
                }
                else if (this.MinorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis)
                {
                    minorTickSize = this.MinorTickMark.Size / 2F;
                }
                else if (this.MinorTickMark.TickMarkStyle == TickMarkStyle.OutsideArea)
                {
                    minorTickSize = this.MinorTickMark.Size;
                }
            }

            this.markSize += (float)Math.Max(majorTickSize, minorTickSize);


            // Add axis line size
            SizeF borderSize = chartGraph.GetRelativeSize(new SizeF(this.LineWidth, this.LineWidth));

            // If axis is horizontal
            if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
            {
                this.markSize += borderSize.Height / 2f;
                this.markSize = Math.Min(this.markSize, (plotArea.Height / 100F) * (Axis.maxAxisMarkSize / axesNumber));
            }
            // If axis is vertical
            else
            {
                this.markSize += borderSize.Width / 2f;
                this.markSize = Math.Min(this.markSize, (plotArea.Width / 100F) * (Axis.maxAxisMarkSize / axesNumber));
            }

            // Add axis scroll bar size (if it's visible)
            this.scrollBarSize = 0f;

#if Microsoft_CONTROL

            if (this.ScrollBar.IsVisible &&
                (this.IsAxisOnAreaEdge || !this.IsMarksNextToAxis))
            {
                if (this.ScrollBar.IsPositionedInside)
                {
                    this.markSize += (float)this.ScrollBar.GetScrollBarRelativeSize();
                }
                else
                {
                    this.scrollBarSize = (float)this.ScrollBar.GetScrollBarRelativeSize();
                }
            }

#endif // Microsoft_CONTROL


            //*********************************************************
            //** Adjust mark size using area scene wall width
            //*********************************************************
            if (ChartArea.Area3DStyle.Enable3D &&
                !ChartArea.chartAreaIsCurcular &&
                ChartArea.BackColor != Color.Transparent &&
                ChartArea.Area3DStyle.WallWidth > 0)
            {
                SizeF areaWallSize = chartGraph.GetRelativeSize(new SizeF(ChartArea.Area3DStyle.WallWidth, ChartArea.Area3DStyle.WallWidth));
                if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                {
                    this.markSize += areaWallSize.Height;
                }
                else
                {
                    this.markSize += areaWallSize.Width;
                }

                // Ignore Max marks size for the 3D wall size.
                //this.markSize = Math.Min(this.markSize, (plotArea.Width / 100F) * (Axis.maxAxisMarkSize / axesNumber));
            }

            //*********************************************************
            //** Adjust title size and mark size using arrow size
            //*********************************************************
            if (arrowSize > (this.markSize + this.scrollBarSize + this.titleSize))
            {
                this.markSize = Math.Max(this.markSize, arrowSize - (this.markSize + this.scrollBarSize + this.titleSize));
                this.markSize = Math.Min(this.markSize, (plotArea.Width / 100F) * (Axis.maxAxisMarkSize / axesNumber));
            }

            //*********************************************************
            //** Calculate max label size
            //*********************************************************
            float maxLabelSize = 0;

            if (!autoPlotPosition)
            {
                if (this.GetIsMarksNextToAxis())
                {
                    if (this.AxisPosition == AxisPosition.Top)
                        maxLabelSize = (float)GetAxisPosition() - ChartArea.Position.Y;
                    else if (this.AxisPosition == AxisPosition.Bottom)
                        maxLabelSize = ChartArea.Position.Bottom - (float)GetAxisPosition();
                    if (this.AxisPosition == AxisPosition.Left)
                        maxLabelSize = (float)GetAxisPosition() - ChartArea.Position.X;
                    else if (this.AxisPosition == AxisPosition.Right)
                        maxLabelSize = ChartArea.Position.Right - (float)GetAxisPosition();
                }
                else
                {
                    if (this.AxisPosition == AxisPosition.Top)
                        maxLabelSize = plotArea.Y - ChartArea.Position.Y;
                    else if (this.AxisPosition == AxisPosition.Bottom)
                        maxLabelSize = ChartArea.Position.Bottom - plotArea.Bottom;
                    if (this.AxisPosition == AxisPosition.Left)
                        maxLabelSize = plotArea.X - ChartArea.Position.X;
                    else if (this.AxisPosition == AxisPosition.Right)
                        maxLabelSize = ChartArea.Position.Right - plotArea.Right;
                }

                maxLabelSize *= 2F;
            }
            else
            {
                if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                    maxLabelSize = plotArea.Height * (_maximumAutoSize / 100f);
                else
                    maxLabelSize = plotArea.Width * (_maximumAutoSize / 100f);
            }



            //******************************************************
            //** First try to select the interval that will 
            //** generate best fit labels.
            //******************************************************



			// Make sure the variable interval mode is enabled and
			// no custom label interval used.
			if( this.Enabled != AxisEnabled.False &&
				this.LabelStyle.Enabled &&
				this.IsVariableLabelCountModeEnabled() )
			{
				// Increase font by several points when height of the font is the most important
				// dimension. Use original size whenwidth is the most important size.
				float extraSize = 3f;
				if( (this.AxisPosition == AxisPosition.Left || this.AxisPosition == AxisPosition.Right) && 
					(this.LabelStyle.Angle == 90 || this.LabelStyle.Angle == -90) )
				{
					extraSize = 0f;
				}
				if( (this.AxisPosition == AxisPosition.Top || this.AxisPosition == AxisPosition.Bottom) && 
					(this.LabelStyle.Angle == 180 || this.LabelStyle.Angle == 0) )
				{
					extraSize = 0f;
				}

				// If 3D Common.Chart is used make the measurements with font several point larger
				if(ChartArea.Area3DStyle.Enable3D)
				{
					extraSize += 1f;
				}

				this.autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(this.LabelStyle.Font.FontFamily, 
					this.LabelStyle.Font.Size + extraSize, 
					this.LabelStyle.Font.Style, 
					GraphicsUnit.Point);

				// Reset angle and stagged flag used in the auto-fitting algorithm
				this.autoLabelAngle = this.LabelStyle.Angle;
				this.autoLabelOffset = (this.LabelStyle.IsStaggered) ? 1 : 0;

				// Adjust interval
				this.AdjustIntervalToFitLabels(chartGraph, autoPlotPosition, false);
			}



            //******************************************************
            //** Automatically calculate the best font size, angle 
            //** and try to use offset labels.
            //******************************************************
            // Reset all automatic label properties
            autoLabelFont = null;
            autoLabelAngle = -1000;
            autoLabelOffset = -1;

            // For circular Common.Chart area process auto-fitting for Y Axis only
            if (this.IsLabelAutoFit &&
                this.LabelAutoFitStyle != LabelAutoFitStyles.None &&
                !ChartArea.chartAreaIsCurcular)
            {
                bool fitDone = false;
                bool noWordWrap = false;

                // Set default font angle and labels offset flag
                autoLabelAngle = 0;
                autoLabelOffset = 0;

                // Original labels collection
                CustomLabelsCollection originalLabels = null;

                // Pick up maximum font size
                float size = 8f;
				size = (float)Math.Max(this.LabelAutoFitMaxFontSize, this.LabelAutoFitMinFontSize);
				_minLabelFontSize = Math.Min(this.LabelAutoFitMinFontSize, this.LabelAutoFitMaxFontSize);
				_aveLabelFontSize = _minLabelFontSize + Math.Abs(size - _minLabelFontSize)/2f;


                // Check if common font size should be used
                if (ChartArea.IsSameFontSizeForAllAxes)
                {
                    size = (float)Math.Min(size, ChartArea.axesAutoFontSize);
                }

                //Set new font
                autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(this.LabelStyle.Font.FontFamily,
                    size,
                    this.LabelStyle.Font.Style,
					GraphicsUnit.Point
                );

                // Check if we allowed to increase font size while auto-fitting
                if ((this.LabelAutoFitStyle & LabelAutoFitStyles.IncreaseFont) != LabelAutoFitStyles.IncreaseFont)
                {
                    // Use axis labels font as starting point
                    autoLabelFont = this.LabelStyle.Font;
                }

                // Loop while labels do not fit
                float spacer = 0f;
                while (!fitDone)
                {
                    //******************************************************
                    //** Check if labels fit
                    //******************************************************

                    // Check if grouping labels fit should be checked
                    bool checkLabelsFirstRowOnly = true;
                    if ((this.LabelAutoFitStyle & LabelAutoFitStyles.DecreaseFont) == LabelAutoFitStyles.DecreaseFont)
                    {
                        // Only check grouping labels if we can reduce fonts size
                        checkLabelsFirstRowOnly = false;
                    }

                    // Check labels fit
                    fitDone = CheckLabelsFit(
                        chartGraph,
                        this.markSize + this.scrollBarSize + this.titleSize + spacer,
                        autoPlotPosition,
                        checkLabelsFirstRowOnly,
                        false);

                    //******************************************************
                    //** Adjust labels text properties to fit
                    //******************************************************
                    if (!fitDone)
                    {
                        // If font is bigger than average try to make it smaller
                        if (autoLabelFont.SizeInPoints >= _aveLabelFontSize &&
                            (this.LabelAutoFitStyle & LabelAutoFitStyles.DecreaseFont) == LabelAutoFitStyles.DecreaseFont)
                        {
                            //Clean up the old font
                            autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                                autoLabelFont.FontFamily,
                                autoLabelFont.SizeInPoints - 0.5f,
                                autoLabelFont.Style,
                                GraphicsUnit.Point);
                        }

                            // Try to use offset labels (2D charts and non-circular arae only!!!)
                        else if (!ChartArea.Area3DStyle.Enable3D &&
                            !ChartArea.chartAreaIsCurcular &&
                            originalLabels == null &&
                            autoLabelAngle == 0 &&
                            autoLabelOffset == 0 &&
                            (this.LabelAutoFitStyle & LabelAutoFitStyles.StaggeredLabels) == LabelAutoFitStyles.StaggeredLabels)
                        {
                            autoLabelOffset = 1;
                        }

                            // Try to insert new line characters in labels text
                        else if (!noWordWrap &&
                            (this.LabelAutoFitStyle & LabelAutoFitStyles.WordWrap) == LabelAutoFitStyles.WordWrap)
                        {
                            bool changed = false;
                            autoLabelOffset = 0;

                            // Check if backup copy of the original lables was made
                            if (originalLabels == null)
                            {
                                // Copy current labels collection
                                originalLabels = new CustomLabelsCollection(this);
                                foreach (CustomLabel label in this.CustomLabels)
                                {
                                    originalLabels.Add(label.Clone());
                                }
                            }

                            // Try to insert new line character into the longest label
                            changed = WordWrapLongestLabel(this.CustomLabels);

                            // Word wrapping do not solve the labels overlapping issue
                            if (!changed)
                            {
                                noWordWrap = true;

                                // Restore original labels
                                if (originalLabels != null)
                                {
                                    this.CustomLabels.Clear();
                                    foreach (CustomLabel label in originalLabels)
                                    {
                                        this.CustomLabels.Add(label.Clone());
                                    }

                                    originalLabels = null;
                                }

                                if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                                {
                                    if ((spacer == 0 ||
                                        spacer == 30f ||
                                        spacer == 20f) &&
                                        ((this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep30) == LabelAutoFitStyles.LabelsAngleStep30 ||
                                        (this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep45) == LabelAutoFitStyles.LabelsAngleStep45 ||
                                        (this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep90) == LabelAutoFitStyles.LabelsAngleStep90))
                                    {
                                        // Try to use 90 degrees angle
                                        autoLabelAngle = 90;
                                        noWordWrap = false;

                                        // Usually 55% of Common.Chart area size is allowed for labels
                                        // Reduce that space.
                                        if (spacer == 0f)
                                        {
                                            // 30
                                            spacer = 30f;
                                        }
                                        else if (spacer == 30f)
                                        {
                                            // 20
                                            spacer = 20f;
                                        }
                                        else if (spacer == 20f)
                                        {
                                            // 5
                                            spacer = 5f;
                                        }
                                        else
                                        {
                                            autoLabelAngle = 0;
                                            noWordWrap = true;
                                        }

                                    }
                                    else
                                    {
                                        spacer = 0f;
                                    }
                                }
                            }
                        }

                            // Try to change font angle
                        else if (autoLabelAngle != 90 &&
                            ((this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep30) == LabelAutoFitStyles.LabelsAngleStep30 ||
                            (this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep45) == LabelAutoFitStyles.LabelsAngleStep45 ||
                            (this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep90) == LabelAutoFitStyles.LabelsAngleStep90))
                        {
                            spacer = 0f;
                            autoLabelOffset = 0;

                            if ((this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep30) == LabelAutoFitStyles.LabelsAngleStep30)
                            {
                                // Increase angle by 45 degrees in 2D and 45 in 3D
                                autoLabelAngle += (ChartArea.Area3DStyle.Enable3D) ? 45 : 30;
                            }
                            else if ((this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep45) == LabelAutoFitStyles.LabelsAngleStep45)
                            {
                                // Increase angle by 45 degrees
                                autoLabelAngle += 45;
                            }
                            else if ((this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep90) == LabelAutoFitStyles.LabelsAngleStep90)
                            {
                                // Increase angle by 90 degrees
                                autoLabelAngle += 90;
                            }
                        }

                            // Try to reduce font again
                        else if (autoLabelFont.SizeInPoints > _minLabelFontSize &&
                            (this.LabelAutoFitStyle & LabelAutoFitStyles.DecreaseFont) == LabelAutoFitStyles.DecreaseFont)
                        {
                            //Clean up the old font
                            autoLabelAngle = 0;
                            autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                                autoLabelFont.FontFamily,
                                autoLabelFont.SizeInPoints - 0.5f,
                                autoLabelFont.Style,
                                GraphicsUnit.Point);
                        }

                            // Failed to fit
                        else
                        {
                            // Use last font
                            if ((this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep30) == LabelAutoFitStyles.LabelsAngleStep30 ||
                                (this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep45) == LabelAutoFitStyles.LabelsAngleStep45 ||
                                (this.LabelAutoFitStyle & LabelAutoFitStyles.LabelsAngleStep90) == LabelAutoFitStyles.LabelsAngleStep90)
                            {
                                // Reset angle
                                if (this.AxisPosition == AxisPosition.Top || this.AxisPosition == AxisPosition.Bottom)
                                {
                                    autoLabelAngle = 90;
                                }
                                else
                                {
                                    autoLabelAngle = 0;
                                }
                            }
                            if ((this.LabelAutoFitStyle & LabelAutoFitStyles.StaggeredLabels) == LabelAutoFitStyles.StaggeredLabels)
                            {
                                // Reset offset labels
                                autoLabelOffset = 0;
                            }
                            fitDone = true;
                        }
                    }
                    else if (ChartArea.Area3DStyle.Enable3D &&
                        !ChartArea.chartAreaIsCurcular &&
                        autoLabelFont.SizeInPoints > _minLabelFontSize)
                    {
                        // Reduce auto-fit font by 1 for the 3D charts
                        autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                            autoLabelFont.FontFamily,
                            autoLabelFont.SizeInPoints - 0.5f,
                            autoLabelFont.Style,
                            GraphicsUnit.Point);
                    }
                }

				// Change the auto-fit angle for top and bottom axes from 90 to -90
				if(this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
				{
					if(autoLabelAngle == 90)
					{
						autoLabelAngle = -90;
					}
				}
            }

            //*********************************************************
            //** Calculate overall labels size
            //*********************************************************
            this.labelSize = 0;

            // if labels are not enabled their size needs to remain zero
            if (this.LabelStyle.Enabled)
            {
                //******************************************************
                //** Calculate axis second labels row size
                //******************************************************
                this.labelSize = (maxAxisElementsSize) - this.markSize - this.scrollBarSize - this.titleSize;
                if (this.labelSize > 0)
                {
                    this.groupingLabelSizes = GetRequiredGroupLabelSize(chartGraph, (maxLabelSize / 100F) * maxAxisLabelRow2Size);
                    this.totlaGroupingLabelsSize = GetGroupLablesToatalSize();
                }

                //******************************************************
                //** Calculate axis labels size
                //******************************************************
                this.labelSize -= this.totlaGroupingLabelsSize;
                if (this.labelSize > 0)
                {
                    // If axis is horizontal
                    if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                    {
                        this.labelSize = elementSpacing + GetRequiredLabelSize(chartGraph,
                            (maxLabelSize / 100F) * (maxAxisElementsSize - this.markSize - this.scrollBarSize - this.titleSize), out this.unRotatedLabelSize);
                    }
                    // If axis is horizontal
                    else
                    {
                        this.labelSize = elementSpacing + GetRequiredLabelSize(chartGraph,
                            (maxLabelSize / 100F) * (maxAxisElementsSize - this.markSize - this.scrollBarSize - this.titleSize), out this.unRotatedLabelSize);
                    }

                    if (!this.LabelStyle.Enabled)
                    {
                        this.labelSize -= elementSpacing;
                    }
                }
                else
                {
                    this.labelSize = 0;
                }

                this.labelSize += this.totlaGroupingLabelsSize;
            }

#if SUBAXES
			// Calculate offsets for all sub axes
			if(!ChartArea.Area3DStyle.Enable3D && 
				!ChartArea.chartAreaIsCurcular)
			{
				float currentOffset = this.markSize + this.labelSize + this.titleSize + this.scrollBarSize;
				foreach(SubAxis subAxis in this.SubAxes)
				{
					if(subAxis.Enabled != AxisEnabled.False)
					{
						currentOffset += (float)subAxis.LocationOffset;
						subAxis.offsetFromParent = currentOffset;
						currentOffset += subAxis.markSize + subAxis.labelSize + subAxis.titleSize;
					}
				}
			}
#endif // SUBAXES


#if Microsoft_CONTROL
            // Restore previous invalidation flag
			Common.Chart.disableInvalidates = oldDisableInvalidates;
#endif //Microsoft_CONTROL
        }

		/// <summary>
		/// Calculates axis interval so that labels will fit most efficiently.
		/// </summary>
		/// <param name="chartGraph">Chart graphics.</param>
		/// <param name="autoPlotPosition">True if plot position is auto calculated.</param>
		/// <param name="onlyIncreaseInterval">True if interval should only be increased.</param>
		private void AdjustIntervalToFitLabels(ChartGraphics chartGraph, bool autoPlotPosition, bool onlyIncreaseInterval)
		{
			// Calculates axis interval so that labels will fit most efficiently.
			if(this.ScaleSegments.Count == 0)
			{
				this.AdjustIntervalToFitLabels(chartGraph, autoPlotPosition, null, onlyIncreaseInterval);
			}
			else
			{
				// Allow values to go outside the segment boundary
				this.ScaleSegments.AllowOutOfScaleValues = true;

				// Adjust interval of each segment first
				foreach(AxisScaleSegment axisScaleSegment in this.ScaleSegments)
				{
					this.AdjustIntervalToFitLabels(chartGraph, autoPlotPosition, axisScaleSegment, onlyIncreaseInterval);
				}

				// Fill labels using new segment intervals
				bool removeLabels = true;
				int segmentIndex = 0;
				ArrayList removedLabels = new ArrayList();
				ArrayList removedLabelsIndexes = new ArrayList();
				foreach(AxisScaleSegment scaleSegment in this.ScaleSegments)
				{
					scaleSegment.SetTempAxisScaleAndInterval();
					this.FillLabels(removeLabels);
					removeLabels = false;
					scaleSegment.RestoreAxisScaleAndInterval();

					// Remove last label of all segmenst except of the last
					if(segmentIndex < this.ScaleSegments.Count - 1 &&
						this.CustomLabels.Count > 0)
					{
						// Remove label and save it in the list
						removedLabels.Add(this.CustomLabels[this.CustomLabels.Count - 1]);
						removedLabelsIndexes.Add(this.CustomLabels.Count - 1);
						this.CustomLabels.RemoveAt(this.CustomLabels.Count - 1);
					}

					++segmentIndex;
				}

				// Check all previously removed last labels of each segment if there 
				// is enough space to fit them
				int reInsertedLabelsCount = 0;
				int labelIndex = 0;
				foreach(CustomLabel label in removedLabels)
				{
					// Re-insert the label
					int labelInsertIndex = (int)removedLabelsIndexes[labelIndex] + reInsertedLabelsCount;
					if(labelIndex < this.CustomLabels.Count)
					{
						this.CustomLabels.Insert(labelInsertIndex, label);
					}
					else
					{
						this.CustomLabels.Add(label);
					}

					// Check labels fit. Only horizontal or vertical fit is checked depending 
					// on the axis orientation.
					ArrayList labelPositions = new ArrayList();
					bool fitDone = CheckLabelsFit(
						chartGraph, 
						this.markSize + this.scrollBarSize + this.titleSize, 
						autoPlotPosition,
						true,
						false,
						(this.AxisPosition == AxisPosition.Left || this.AxisPosition == AxisPosition.Right) ? false : true,
						(this.AxisPosition == AxisPosition.Left || this.AxisPosition == AxisPosition.Right) ? true : false,
						labelPositions);

					// If labels fit check if any of the label positions overlap
					if(fitDone)
					{
						for(int index = 0; fitDone && index < labelPositions.Count; index++)
						{
							RectangleF rect1 = (RectangleF)labelPositions[index];
							for(int index2 = index + 1; fitDone && index2 < labelPositions.Count; index2++)
							{
								RectangleF rect2 = (RectangleF)labelPositions[index2];
								if(rect1.IntersectsWith(rect2))
								{
									fitDone = false;
								}
							}
						}
					}

					// If labels do not fit or overlapp - remove completly
					if(!fitDone)
					{
						this.CustomLabels.RemoveAt(labelInsertIndex);
					}
					else
					{
						++reInsertedLabelsCount;
					}

					++labelIndex;
				}

				// Make sure now values are rounded on segment boundary
				this.ScaleSegments.AllowOutOfScaleValues = false;
			}
		}

		/// <summary>
		/// Checks if variable count labels mode is enabled.
		/// </summary>
		/// <returns>True if variable count labels mode is enabled.</returns>
		private bool IsVariableLabelCountModeEnabled()
		{
			// Make sure the variable interval mode is enabled and
			// no custom label interval used.
			if( (this.IntervalAutoMode == IntervalAutoMode.VariableCount || this.ScaleSegments.Count > 0) &&
				!this.IsLogarithmic &&
				(this.tempLabelInterval <= 0.0 || (double.IsNaN(this.tempLabelInterval) && this.Interval <= 0.0)) )
			{
				// This feature is not supported for charts that do not
				// require X and Y axes (Pie, Radar, ...)
				if(!ChartArea.requireAxes)
				{
					return false;
				}
                // This feature is not supported if the axis doesn't have data range 
                if (Double.IsNaN(this.minimum) || Double.IsNaN(this.maximum))
                {
                    return false;
                }
				// Check if custom labels are used in the first row
				bool customLabels = false;
				foreach(CustomLabel label in this.CustomLabels)
				{
					if(label.customLabel && label.RowIndex == 0)
					{
						customLabels = true;
						break;
					}
				}

				// Proceed only if no custom labels are used in the first row
				if(!customLabels)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Calculates axis interval so that labels will fit most efficiently.
		/// </summary>
		/// <param name="chartGraph">Chart graphics.</param>
		/// <param name="autoPlotPosition">True if plot position is auto calculated.</param>
		/// <param name="axisScaleSegment">Axis scale segment to process.</param>
		/// <param name="onlyIncreaseInterval">True if interval should only be increased.</param>
		private void AdjustIntervalToFitLabels(
			ChartGraphics chartGraph, 
			bool autoPlotPosition, 
			AxisScaleSegment axisScaleSegment,
			bool onlyIncreaseInterval)
		{
			// Re-fill the labels just for the scale segment provided
			if(axisScaleSegment != null)
			{
				// Re-fill new axis labels
				if(this.tempLabels != null)
				{
					this.CustomLabels.Clear();
					foreach( CustomLabel label in this.tempLabels )
					{
						this.CustomLabels.Add(label.Clone());
					}
				}

				// Fill labels just for the segment
				axisScaleSegment.SetTempAxisScaleAndInterval();
				this.FillLabels( true );
				axisScaleSegment.RestoreAxisScaleAndInterval();
			}

			// Calculate minimum interval size
			double minIntervalSzie = double.NaN;
			ArrayList axisSeries = AxisScaleBreakStyle.GetAxisSeries(this);
			foreach(Series series in axisSeries)
			{
				if(this.axisType == AxisName.X || this.axisType == AxisName.X2)
				{
					if(ChartHelper.IndexedSeries(series))
					{
						minIntervalSzie = 1.0;
					}
					else if(series.XValueType == ChartValueType.String || 
						series.XValueType == ChartValueType.Int32 || 
						series.XValueType == ChartValueType.UInt32 || 
						series.XValueType == ChartValueType.UInt64 ||
						series.XValueType == ChartValueType.Int64 )
					{
						minIntervalSzie = 1.0;
					}
				}
				else
				{
					if(series.YValueType == ChartValueType.String || 
						series.YValueType == ChartValueType.Int32 || 
						series.YValueType == ChartValueType.UInt32 || 
						series.YValueType == ChartValueType.UInt64 ||
						series.YValueType == ChartValueType.Int64 )
					{
						minIntervalSzie = 1.0;
					}
				}
			}


			// Iterate while interval is not found
			bool firstIteration = true;
			bool increaseNumberOfLabels = true;
			double currentInterval = (axisScaleSegment == null) ? this.labelStyle.GetInterval() : axisScaleSegment.Interval;
			DateTimeIntervalType currentIntervalType = (axisScaleSegment == null) ? this.labelStyle.GetIntervalType() : axisScaleSegment.IntervalType;
			DateTimeIntervalType lastFitIntervalType = currentIntervalType;
			double lastFitInterval = currentInterval;
			ArrayList lastFitLabels = new ArrayList();
			bool intervalFound = false;
			int iterationNumber = 0;
			while(!intervalFound && iterationNumber <= 1000)
			{
				bool fillNewLabels = true;
#if DEBUG
				if(iterationNumber >= 999)
				{
                    throw (new InvalidOperationException(SR.ExceptionAxisDynamicIntervalCalculationFailed));
				}
#endif // DEBUG

				// Check labels fit. Only horizontal or vertical fit is checked depending 
				// on the axis orientation.
				bool fitDone = CheckLabelsFit(
					chartGraph, 
					this.markSize + this.scrollBarSize + this.titleSize, 
					autoPlotPosition,
					true,
					false,
					(this.AxisPosition == AxisPosition.Left || this.AxisPosition == AxisPosition.Right) ? false : true,
					(this.AxisPosition == AxisPosition.Left || this.AxisPosition == AxisPosition.Right) ? true : false,
					null);

				// Check if we need to increase or reduce number of labels
				if(firstIteration)
				{
					firstIteration = false;
					increaseNumberOfLabels = (fitDone) ? true : false;

					// Check if we can decrease the interva;
					if(onlyIncreaseInterval && increaseNumberOfLabels)
					{
						intervalFound = true;
						continue;
					}
				}

				// Find new interval. Value 0.0 means that interval cannot be
				// reduced/increased any more and current interval should be used
				double newInterval = 0.0;
				DateTimeIntervalType newIntervalType = DateTimeIntervalType.Number;
				if(increaseNumberOfLabels)
				{
					if(fitDone)
					{
						// Make a copy of last interval and labels collection that previously fit
						lastFitInterval = currentInterval;
						lastFitIntervalType = currentIntervalType;
						lastFitLabels.Clear();
						foreach(CustomLabel label in this.CustomLabels)
						{
							lastFitLabels.Add(label);
						}

						newIntervalType = currentIntervalType;
						newInterval = this.ReduceLabelInterval(
							currentInterval, 
							minIntervalSzie, 
							ref newIntervalType);
					}
					else
					{
						newInterval = lastFitInterval;
						newIntervalType = lastFitIntervalType;
						intervalFound = true;

						// Reuse previously saved labels
						fillNewLabels = false;
						this.CustomLabels.Clear();
						foreach(CustomLabel label in lastFitLabels)
						{
							this.CustomLabels.Add(label);
						}

					}
				}
				else
				{
					if(!fitDone && this.CustomLabels.Count > 1)
					{
						newIntervalType = currentIntervalType;
						newInterval = this.IncreaseLabelInterval(
							currentInterval, 
							ref newIntervalType);
					}
					else
					{
						intervalFound = true;
					}
				}

				// Set new interval
				if(newInterval != 0.0)
				{
					currentInterval = newInterval;
					currentIntervalType = newIntervalType;

					if(axisScaleSegment == null)
					{
						this.SetIntervalAndType(newInterval, newIntervalType);
					}
					else
					{
						axisScaleSegment.Interval = newInterval;
						axisScaleSegment.IntervalType = newIntervalType;
					}

					// Re-fill new axis labels
					if(fillNewLabels)
					{
						if(this.tempLabels != null)
						{
							this.CustomLabels.Clear();
							foreach( CustomLabel label in this.tempLabels )
							{
								CustomLabels.Add(label.Clone());
							}
						}
					
						if(axisScaleSegment == null)
						{
							this.FillLabels(true);
						}
						else
						{
							axisScaleSegment.SetTempAxisScaleAndInterval();
							this.FillLabels( true );
							axisScaleSegment.RestoreAxisScaleAndInterval();
						}
					}
				}
				else
				{
					intervalFound = true;
				}
			
				++iterationNumber;
			}
		}

		/// <summary>
		/// Reduces current label interval, so that more labels can fit.
		/// </summary>
		/// <param name="oldInterval">An interval to reduce.</param>
		/// <param name="minInterval">Minimum interval size.</param>
        /// <param name="axisIntervalType">Interval type.</param>
		/// <returns>New interval or 0.0 if interval cannot be reduced.</returns>
		private double ReduceLabelInterval(
			double oldInterval, 
			double minInterval,
			ref DateTimeIntervalType axisIntervalType)
		{
			double newInterval = oldInterval;

			// Calculate rounded interval value
			double range = this.maximum - this.minimum;
			int iterationIndex = 0;
			if( axisIntervalType == DateTimeIntervalType.Auto ||
				axisIntervalType == DateTimeIntervalType.NotSet ||
				axisIntervalType == DateTimeIntervalType.Number)
			{
				// Process numeric scale
				double devider = 2.0;
				do
				{
#if DEBUG
					if(iterationIndex >= 99)
					{
                        throw (new InvalidOperationException(SR.ExceptionAxisIntervalDecreasingFailed));
					}
#endif // DEBUG

					newInterval = CalcInterval( range / (range / (newInterval / devider)) );
					if(newInterval == oldInterval)
					{
						devider *= 2.0;
					}

					++iterationIndex;
				} while(newInterval == oldInterval && iterationIndex <= 100);
			}
			else
			{
				// Process date scale
				if(oldInterval > 1.0 || oldInterval < 1.0)
				{
					if( axisIntervalType == DateTimeIntervalType.Minutes || 
						axisIntervalType == DateTimeIntervalType.Seconds)
					{
						if(oldInterval >= 60)
						{
							newInterval = Math.Round(oldInterval / 2.0);
						}
						else if(oldInterval >= 30.0)
						{
							newInterval = 15.0;
						}
						else if(oldInterval >= 15.0)
						{
							newInterval = 5.0;
						}
						else if(oldInterval >= 5.0)
						{
							newInterval = 1.0;
						}
					}
					else
					{
						newInterval = Math.Round(oldInterval / 2.0);
					}
					if(newInterval < 1.0)
					{
						newInterval = 1.0;
					}
				}
				if(oldInterval == 1.0)
				{
					if(axisIntervalType == DateTimeIntervalType.Years)
					{
						newInterval = 6.0;
						axisIntervalType = DateTimeIntervalType.Months;
					}
					else if(axisIntervalType == DateTimeIntervalType.Months)
					{
						newInterval = 2.0;
						axisIntervalType = DateTimeIntervalType.Weeks;
					}
					else if(axisIntervalType == DateTimeIntervalType.Weeks)
					{
						newInterval = 2.0;
						axisIntervalType = DateTimeIntervalType.Days;
					}
					else if(axisIntervalType == DateTimeIntervalType.Days)
					{
						newInterval = 12.0;
						axisIntervalType = DateTimeIntervalType.Hours;
					}
					else if(axisIntervalType == DateTimeIntervalType.Hours)
					{
						newInterval = 30.0;
						axisIntervalType = DateTimeIntervalType.Minutes;
					}
					else if(axisIntervalType == DateTimeIntervalType.Minutes)
					{
						newInterval = 30.0;
						axisIntervalType = DateTimeIntervalType.Seconds;
					}
					else if(axisIntervalType == DateTimeIntervalType.Seconds)
					{
						newInterval = 100.0;
						axisIntervalType = DateTimeIntervalType.Milliseconds;
					}
				}
			}


			// Make sure interal is not less than min interval specified
			if(!double.IsNaN(minInterval) && newInterval < minInterval)
			{
				newInterval = 0.0;
			}

			return newInterval;
		}

		/// <summary>
		/// Increases current label interval, so that less labels fit.
		/// </summary>
		/// <param name="oldInterval">An interval to increase.</param>
        /// <param name="axisIntervalType">Interval type.</param>
		/// <returns>New interval or 0.0 if interval cannot be increased.</returns>
		private double IncreaseLabelInterval(
			double oldInterval,  
			ref DateTimeIntervalType axisIntervalType)
		{
			double newInterval = oldInterval;
			
			// Calculate rounded interval value
			double range = this.maximum - this.minimum;
			int iterationIndex = 0;
			if( axisIntervalType == DateTimeIntervalType.Auto ||
				axisIntervalType == DateTimeIntervalType.NotSet ||
				axisIntervalType == DateTimeIntervalType.Number)
			{
				// Process numeric scale
				double devider = 2.0;
				do
				{
#if DEBUG
					if(iterationIndex >= 99)
					{
                        throw (new InvalidOperationException(SR.ExceptionAxisIntervalIncreasingFailed));
					}
#endif // DEBUG

					newInterval = CalcInterval( range / (range / (newInterval * devider)) );
					if(newInterval == oldInterval)
					{
						devider *= 2.0;
					}
					++iterationIndex;
				} while(newInterval == oldInterval && iterationIndex <= 100);
			}
			else
			{
				// Process date scale
				newInterval = oldInterval * 2.0;
				if(axisIntervalType == DateTimeIntervalType.Years)
				{
					// Do nothing for years
				}
				else if(axisIntervalType == DateTimeIntervalType.Months)
				{
					if(newInterval >= 12.0)
					{
						newInterval = 1.0;
						axisIntervalType = DateTimeIntervalType.Years;
					}
				}
				else if(axisIntervalType == DateTimeIntervalType.Weeks)
				{
					if(newInterval >= 4.0)
					{
						newInterval = 1.0;
						axisIntervalType = DateTimeIntervalType.Months;
					}
				}
				else if(axisIntervalType == DateTimeIntervalType.Days)
				{
					if(newInterval >= 7.0)
					{
						newInterval = 1.0;
						axisIntervalType = DateTimeIntervalType.Weeks;
					}
				}
				else if(axisIntervalType == DateTimeIntervalType.Hours)
				{
					if(newInterval >= 60.0)
					{
						newInterval = 1.0;
						axisIntervalType = DateTimeIntervalType.Days;
					}
				}
				else if(axisIntervalType == DateTimeIntervalType.Minutes)
				{
					if(newInterval >= 60.0)
					{
						newInterval = 1.0;
						axisIntervalType = DateTimeIntervalType.Hours;
					}
				}
				else if(axisIntervalType == DateTimeIntervalType.Seconds)
				{
					if(newInterval >= 60.0)
					{
						newInterval = 1.0;
						axisIntervalType = DateTimeIntervalType.Minutes;
					}
				}
				else if(axisIntervalType == DateTimeIntervalType.Milliseconds)
				{
					if(newInterval >= 1000.0)
					{
						newInterval = 1.0;
						axisIntervalType = DateTimeIntervalType.Seconds;
					}
				}
			}

			return newInterval;
		}

        /// <summary>
        /// Finds the longest labels with the space and inserts the new line character.
        /// </summary>
        /// <param name="labels">Labels collection.</param>
        /// <returns>True if collection was modified.</returns>
        private bool WordWrapLongestLabel(CustomLabelsCollection labels)
        {
            bool changed = false;

            // Each label may contain several lines of text.
            // Create a list that contains an array of text for each label.
            ArrayList labelTextRows = new ArrayList(labels.Count);
            foreach (CustomLabel label in labels)
            {
                labelTextRows.Add(label.Text.Split('\n'));
            }

            // Find the longest label with a space
            int longestLabelSize = 5;
            int longestLabelIndex = -1;
            int longestLabelRowIndex = -1;
            int index = 0;
            foreach (string[] textRows in labelTextRows)
            {
                for (int rowIndex = 0; rowIndex < textRows.Length; rowIndex++)
                {
                    if (textRows[rowIndex].Length > longestLabelSize && textRows[rowIndex].Trim().IndexOf(' ') > 0)
                    {
                        longestLabelSize = textRows[rowIndex].Length;
                        longestLabelIndex = index;
                        longestLabelRowIndex = rowIndex;
                    }
                }
                ++index;
            }

            // Longest label with a space was found
            if (longestLabelIndex >= 0 && longestLabelRowIndex >= 0)
            {
                // Try to find a space and replace it with a new line
                string newText = ((string[])labelTextRows[longestLabelIndex])[longestLabelRowIndex];
                for (index = 0; index < (newText.Length) / 2 - 1; index++)
                {
                    if (newText[(newText.Length) / 2 - index] == ' ')
                    {
                        newText =
                            newText.Substring(0, (newText.Length) / 2 - index) +
                            "\n" +
                            newText.Substring((newText.Length) / 2 - index + 1);
                        changed = true;
                    }
                    else if (newText[(newText.Length) / 2 + index] == ' ')
                    {
                        newText =
                            newText.Substring(0, (newText.Length) / 2 + index) +
                            "\n" +
                            newText.Substring((newText.Length) / 2 + index + 1);
                        changed = true;
                    }

                    if (changed)
                    {
                        ((string[])labelTextRows[longestLabelIndex])[longestLabelRowIndex] = newText;
                        break;
                    }
                }

                // Update label text
                if (changed)
                {
                    // Construct label text from multiple rows separated by "\n"
                    CustomLabel label = labels[longestLabelIndex];
                    label.Text = string.Empty;
                    for (int rowIndex = 0; rowIndex < ((string[])labelTextRows[longestLabelIndex]).Length; rowIndex++)
                    {
                        if (rowIndex > 0)
                        {
                            label.Text += "\n";
                        }
                        label.Text += ((string[])labelTextRows[longestLabelIndex])[rowIndex];
                    }
                }
            }

            return changed;
        }

        /// <summary>
        /// Calculates the auto-fit font for the circular Common.Chart area axis labels.
        /// </summary>
        /// <param name="graph">Chart graphics object.</param>
        /// <param name="axisList">List of sector labels.</param>
        /// <param name="labelsStyle">Circular labels style.</param>
        /// <param name="plotAreaRectAbs">Plotting area position.</param>
        /// <param name="areaRectAbs">Chart area position.</param>
        /// <param name="labelsSizeEstimate">Estimated size of labels.</param>
        internal void GetCircularAxisLabelsAutoFitFont(
            ChartGraphics graph,
            ArrayList axisList,
            CircularAxisLabelsStyle labelsStyle,
            RectangleF plotAreaRectAbs,
            RectangleF areaRectAbs,
            float labelsSizeEstimate)
        {
            // X axis settings defines if auto-fit font should be calculated
            if (!this.IsLabelAutoFit ||
                this.LabelAutoFitStyle == LabelAutoFitStyles.None ||
                !this.LabelStyle.Enabled)
            {
                return;
            }

			// Set minimum font size
			_minLabelFontSize = Math.Min(this.LabelAutoFitMinFontSize, this.LabelAutoFitMaxFontSize);

            // Create new auto-fit font
            this.autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                this.LabelStyle.Font.FontFamily,
				Math.Max(this.LabelAutoFitMaxFontSize, this.LabelAutoFitMinFontSize),
                this.LabelStyle.Font.Style,
				GraphicsUnit.Point);

            // Check if we allowed to increase font size while auto-fitting
            if ((this.LabelAutoFitStyle & LabelAutoFitStyles.IncreaseFont) != LabelAutoFitStyles.IncreaseFont)
            {
                // Use axis labels font as starting point
                this.autoLabelFont = this.LabelStyle.Font;
            }

            // Loop while labels do not fit
            bool fitDone = false;
            while (!fitDone)
            {
                //******************************************************
                //** Check if labels fit
                //******************************************************
                fitDone = CheckCircularLabelsFit(
                    graph,
                    axisList,
                    labelsStyle,
                    plotAreaRectAbs,
                    areaRectAbs,
                    labelsSizeEstimate);

                //******************************************************
                //** Adjust labels text properties to fit
                //******************************************************
                if (!fitDone)
                {
                    // Try to reduce font size
                    if (autoLabelFont.SizeInPoints > _minLabelFontSize &&
                        (this.LabelAutoFitStyle & LabelAutoFitStyles.DecreaseFont) == LabelAutoFitStyles.DecreaseFont)
                    {
                        autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                            autoLabelFont.FontFamily, 
                            autoLabelFont.SizeInPoints - 1, 
                            autoLabelFont.Style, 
                            GraphicsUnit.Point);

                    }

                    // Failed to fit
                    else
                    {
                        // Use last font with no angles
                        autoLabelAngle = 0;
                        autoLabelOffset = 0;
                        fitDone = true;
                    }
                }
            }
        }

        /// <summary>
        /// Checks id circular axis labels fits using current auto-fit font.
        /// </summary>
        /// <param name="graph">Chart graphics object.</param>
        /// <param name="axisList">List of sector labels.</param>
        /// <param name="labelsStyle">Circular labels style.</param>
        /// <param name="plotAreaRectAbs">Plotting area position.</param>
        /// <param name="areaRectAbs">Chart area position.</param>
        /// <param name="labelsSizeEstimate">Estimated size of labels.</param>
        /// <returns>True if labels fit.</returns>
        internal bool CheckCircularLabelsFit(
            ChartGraphics graph,
            ArrayList axisList,
            CircularAxisLabelsStyle labelsStyle,
            RectangleF plotAreaRectAbs,
            RectangleF areaRectAbs,
            float labelsSizeEstimate)
        {
            bool labelsFit = true;

            // Get absolute center of the area
            PointF areaCenterAbs = graph.GetAbsolutePoint(ChartArea.circularCenter);

            // Get absolute markers size and spacing
            float spacing = graph.GetAbsolutePoint(new PointF(0, this.markSize + Axis.elementSpacing)).Y;

            //*****************************************************************
            //** Loop through all axis labels
            //*****************************************************************
            RectangleF prevLabelPosition = RectangleF.Empty;
            float prevLabelSideAngle = float.NaN;
            foreach (CircularChartAreaAxis axis in axisList)
            {
                //*****************************************************************
                //** Measure label text
                //*****************************************************************
                SizeF textSize = graph.MeasureString(
                    axis.Title.Replace("\\n", "\n"),
                    this.autoLabelFont);

                //*****************************************************************
                //** Get circular style label position.
                //*****************************************************************
                if (labelsStyle == CircularAxisLabelsStyle.Circular ||
                    labelsStyle == CircularAxisLabelsStyle.Radial)
                {
                    // Swith text size for the radial style
                    if (labelsStyle == CircularAxisLabelsStyle.Radial)
                    {
                        float tempValue = textSize.Width;
                        textSize.Width = textSize.Height;
                        textSize.Height = tempValue;
                    }

                    //*****************************************************************
                    //** Check overlapping with previous label
                    //*****************************************************************

                    // Get radius of plot area
                    float plotAreaRadius = areaCenterAbs.Y - plotAreaRectAbs.Y;
                    plotAreaRadius -= labelsSizeEstimate;
                    plotAreaRadius += spacing;

                    // Calculate angle on the side of the label
                    float leftSideAngle = (float)(Math.Atan((textSize.Width / 2f) / plotAreaRadius) * 180f / Math.PI);
                    float rightSideAngle = axis.AxisPosition + leftSideAngle;
                    leftSideAngle = axis.AxisPosition - leftSideAngle;

                    // Check if label overlap the previous label
                    if (!float.IsNaN(prevLabelSideAngle))
                    {
                        if (prevLabelSideAngle > leftSideAngle)
                        {
                            // Labels overlap
                            labelsFit = false;
                            break;
                        }
                    }

                    // Remember label side angle
                    prevLabelSideAngle = rightSideAngle - 1;


                    //*****************************************************************
                    //** Check if label is inside the Common.Chart area
                    //*****************************************************************

                    // Find the most outside point of the label
                    PointF outsidePoint = new PointF(areaCenterAbs.X, plotAreaRectAbs.Y);
                    outsidePoint.Y += labelsSizeEstimate;
                    outsidePoint.Y -= textSize.Height;
                    outsidePoint.Y -= spacing;

                    PointF[] rotatedPoint = new PointF[] { outsidePoint };
                    Matrix newMatrix = new Matrix();
                    newMatrix.RotateAt(axis.AxisPosition, areaCenterAbs);
                    newMatrix.TransformPoints(rotatedPoint);

                    // Check if rotated point is inside Common.Chart area
                    if (!areaRectAbs.Contains(rotatedPoint[0]))
                    {
                        // Label is not inside Common.Chart area
                        labelsFit = false;
                        break;
                    }

                }

                //*****************************************************************
                //** Get horizontal style label position.
                //*****************************************************************
                else if (labelsStyle == CircularAxisLabelsStyle.Horizontal)
                {
                    // Get text angle
                    float textAngle = axis.AxisPosition;
                    if (textAngle > 180f)
                    {
                        textAngle -= 180f;
                    }

                    // Get label rotated position
                    PointF[] labelPosition = new PointF[] { new PointF(areaCenterAbs.X, plotAreaRectAbs.Y) };
                    labelPosition[0].Y += labelsSizeEstimate;
                    labelPosition[0].Y -= spacing;
                    Matrix newMatrix = new Matrix();
                    newMatrix.RotateAt(textAngle, areaCenterAbs);
                    newMatrix.TransformPoints(labelPosition);

                    // Calculate label position
                    RectangleF curLabelPosition = new RectangleF(
                        labelPosition[0].X,
                        labelPosition[0].Y - textSize.Height / 2f,
                        textSize.Width,
                        textSize.Height);
                    if (textAngle < 5f)
                    {
                        curLabelPosition.X = labelPosition[0].X - textSize.Width / 2f;
                        curLabelPosition.Y = labelPosition[0].Y - textSize.Height;
                    }
                    if (textAngle > 175f)
                    {
                        curLabelPosition.X = labelPosition[0].X - textSize.Width / 2f;
                        curLabelPosition.Y = labelPosition[0].Y;
                    }

                    // Decrease label rectangle
                    curLabelPosition.Inflate(0f, -curLabelPosition.Height * 0.15f);

                    // Check if label position goes outside of the Common.Chart area.
                    if (!areaRectAbs.Contains(curLabelPosition))
                    {
                        // Label is not inside Common.Chart area
                        labelsFit = false;
                        break;
                    }

                    // Check if label position overlap previous label position.
                    if (!prevLabelPosition.IsEmpty && curLabelPosition.IntersectsWith(prevLabelPosition))
                    {
                        // Label intersects with previous label
                        labelsFit = false;
                        break;
                    }

                    // Set previous point position 
                    prevLabelPosition = curLabelPosition;
                }
            }

            return labelsFit;
        }

        #endregion

        #region Axis labels auto-fitting methods

        /// <summary>
        /// Adjust labels font size at second pass of auto fitting.
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="autoPlotPosition">Indicates that inner plot position is automatic.</param>
        internal void AdjustLabelFontAtSecondPass(ChartGraphics chartGraph, bool autoPlotPosition)
        {
#if SUBAXES
			// Process all sub-axis
			if(!ChartArea.Area3DStyle.Enable3D && 
				!ChartArea.chartAreaIsCurcular)
			{
				foreach(SubAxis subAxis in this.SubAxes)
				{
					subAxis.AdjustLabelFontAtSecondPass(chartGraph, autoPlotPosition);
				}
			}
#endif //SUBAXES


            //******************************************************
            //** First try to select the interval that will 
            //** generate best fit labels.
            //******************************************************



			// Make sure the variable interval mode is enabled
			if( this.Enabled != AxisEnabled.False &&
				this.LabelStyle.Enabled &&
				this.IsVariableLabelCountModeEnabled() )
			{
				// Set font for labels fitting
				if(this.autoLabelFont == null) 
				{
					this.autoLabelFont = this.LabelStyle.Font;
				}

				// Reset angle and stagged flag used in the auto-fitting algorithm
				if(this.autoLabelAngle < 0)
				{
					this.autoLabelAngle = this.LabelStyle.Angle;
				}
				if(this.autoLabelOffset < 0)
				{
					this.autoLabelOffset = (this.LabelStyle.IsStaggered) ? 1 : 0;
				}

				// Check labels fit
				bool fitDone = CheckLabelsFit(
					chartGraph, 
					this.markSize + this.scrollBarSize + this.titleSize, 
					autoPlotPosition,
					true,
					true,
					(this.AxisPosition == AxisPosition.Left || this.AxisPosition == AxisPosition.Right) ? false : true,
					(this.AxisPosition == AxisPosition.Left || this.AxisPosition == AxisPosition.Right) ? true : false,
					null);

				// If there is a problem fitting labels try to reduce number of labels by
				// increasing of the interval.
				if(!fitDone)
				{
					// Adjust interval
					this.AdjustIntervalToFitLabels(chartGraph, autoPlotPosition, true);
				}
			}




            //******************************************************
            //** If labels auto-fit is on try reducing font size.
            //******************************************************

            totlaGroupingLabelsSizeAdjustment = 0f;
            if (this.IsLabelAutoFit &&
                this.LabelAutoFitStyle != LabelAutoFitStyles.None &&
                this.Enabled != AxisEnabled.False)
            {
                bool fitDone = false;

                if (autoLabelFont == null)
                {
                    autoLabelFont = this.LabelStyle.Font;
                }

                // Loop while labels do not fit
                float oldLabelSecondRowSize = totlaGroupingLabelsSize;
                while (!fitDone)
                {
                    //******************************************************
                    //** Check if labels fit
                    //******************************************************
                    fitDone = CheckLabelsFit(
                        chartGraph,
                        this.markSize + this.scrollBarSize + this.titleSize,
                        autoPlotPosition,
                        true,
                        true);

                    //******************************************************
                    //** Adjust labels text properties to fit
                    //******************************************************
                    if (!fitDone)
                    {
                        // Try to reduce font
                        if (autoLabelFont.SizeInPoints > _minLabelFontSize)
                        {
                            // Reduce auto fit font
                            if (ChartArea != null && ChartArea.IsSameFontSizeForAllAxes)
                            {
                                // Same font for all axes
                                foreach (Axis currentAxis in ChartArea.Axes)
                                {
                                    if (currentAxis.enabled && currentAxis.IsLabelAutoFit && currentAxis.autoLabelFont != null)
                                    {
                                        currentAxis.autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                                            currentAxis.autoLabelFont.FontFamily,
                                            autoLabelFont.SizeInPoints - 1,
                                            currentAxis.autoLabelFont.Style,
                                            GraphicsUnit.Point);
                                    }
                                }
                            }
                            else if ((this.LabelAutoFitStyle & LabelAutoFitStyles.DecreaseFont) == LabelAutoFitStyles.DecreaseFont)
                            {
                                autoLabelFont = Common.Chart.chartPicture.FontCache.GetFont(
                                    autoLabelFont.FontFamily,
                                    autoLabelFont.SizeInPoints - 1,
                                    autoLabelFont.Style,
                                    GraphicsUnit.Point);
                            }
                            else
                            {
                                // Failed to fit
                                fitDone = true;
                            }
                        }
                        else
                        {
                            // Failed to fit
                            fitDone = true;
                        }
                    }
                }

                this.totlaGroupingLabelsSizeAdjustment = oldLabelSecondRowSize - totlaGroupingLabelsSize;
            }
        }

        /// <summary>
        /// Check if axis is logarithmic
        /// </summary>
        /// <param name="yValue">Y value from data</param>
        /// <returns>Corected Y value if axis is logarithmic</returns>
        internal double GetLogValue(double yValue)
        {
            // Check if axis is logarithmic
            if (this.IsLogarithmic)
            {
                yValue = Math.Log(yValue, this.logarithmBase);
            }

            return yValue;
        }

        /// <summary>
        /// Checks if labels fit using current auto fit properties
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="otherElementsSize">Axis title and marks size.</param>
        /// <param name="autoPlotPosition">Indicates auto calculation of plotting area.</param>
        /// <param name="checkLabelsFirstRowOnly">Labels fit is checked during the second pass.</param>
        /// <param name="secondPass">Indicates second pass of labels fitting.</param>
        /// <returns>True if labels fit.</returns>
        private bool CheckLabelsFit(
            ChartGraphics chartGraph,
            float otherElementsSize,
            bool autoPlotPosition,
            bool checkLabelsFirstRowOnly,
            bool secondPass)
        {
            return this.CheckLabelsFit(
                chartGraph,
                otherElementsSize,
                autoPlotPosition,
                checkLabelsFirstRowOnly,
                secondPass,
                true,
                true,
                null);
        }

        /// <summary>
        /// Checks if labels fit using current auto fit properties
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="otherElementsSize">Axis title and marks size.</param>
        /// <param name="autoPlotPosition">Indicates auto calculation of plotting area.</param>
        /// <param name="checkLabelsFirstRowOnly">Labels fit is checked during the second pass.</param>
        /// <param name="secondPass">Indicates second pass of labels fitting.</param>
        /// <param name="checkWidth">True if width should be checked.</param>
        /// <param name="checkHeight">True if height should be checked.</param>
        /// <param name="labelPositions">Returns an array of label positions.</param>
        /// <returns>True if labels fit.</returns>
        private bool CheckLabelsFit(
            ChartGraphics chartGraph,
            float otherElementsSize,
            bool autoPlotPosition,
            bool checkLabelsFirstRowOnly,
            bool secondPass,
            bool checkWidth,
            bool checkHeight,
            ArrayList labelPositions)
        {
            // Reset list of label positions
            if (labelPositions != null)
            {
                labelPositions.Clear();
            }

            // Label string drawing format			
            using (StringFormat format = new StringFormat())
            {
                format.FormatFlags |= StringFormatFlags.LineLimit;
                format.Trimming = StringTrimming.EllipsisCharacter;

                // Initialize all labels position rectangle
                RectangleF rect = RectangleF.Empty;

                // Calculate max label size
                float maxLabelSize = 0;
                if (!autoPlotPosition)
                {
                    if (this.GetIsMarksNextToAxis())
                    {
                        if (this.AxisPosition == AxisPosition.Top)
                            maxLabelSize = (float)GetAxisPosition() - ChartArea.Position.Y;
                        else if (this.AxisPosition == AxisPosition.Bottom)
                            maxLabelSize = ChartArea.Position.Bottom - (float)GetAxisPosition();
                        if (this.AxisPosition == AxisPosition.Left)
                            maxLabelSize = (float)GetAxisPosition() - ChartArea.Position.X;
                        else if (this.AxisPosition == AxisPosition.Right)
                            maxLabelSize = ChartArea.Position.Right - (float)GetAxisPosition();
                    }
                    else
                    {
                        if (this.AxisPosition == AxisPosition.Top)
                            maxLabelSize = this.PlotAreaPosition.Y - ChartArea.Position.Y;
                        else if (this.AxisPosition == AxisPosition.Bottom)
                            maxLabelSize = ChartArea.Position.Bottom - this.PlotAreaPosition.Bottom;
                        if (this.AxisPosition == AxisPosition.Left)
                            maxLabelSize = this.PlotAreaPosition.X - ChartArea.Position.X;
                        else if (this.AxisPosition == AxisPosition.Right)
                            maxLabelSize = ChartArea.Position.Right - this.PlotAreaPosition.Right;
                    }
                    maxLabelSize *= 2F;
                }
                else
                {
                    if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                        maxLabelSize = ChartArea.Position.Height;
                    else
                        maxLabelSize = ChartArea.Position.Width;
                }

                // Loop through all grouping labels (all except first row)
                this.totlaGroupingLabelsSize = 0;


                // Get number of groups
                int groupLabelLevelCount = GetGroupLabelLevelCount();

                // Check ig grouping labels exist
                if (groupLabelLevelCount > 0)
                {
                    groupingLabelSizes = new float[groupLabelLevelCount];

                    // Loop through each level of grouping labels
                    bool fitResult = true;
                    for (int groupLevelIndex = 1; groupLevelIndex <= groupLabelLevelCount; groupLevelIndex++)
                    {
                        groupingLabelSizes[groupLevelIndex - 1] = 0f;

                        // Loop through all labels in the level
                        foreach (CustomLabel label in this.CustomLabels)
                        {
                            // Skip if label middle point is outside current scaleView
                            if (label.RowIndex == 0)
                            {
                                double middlePoint = (label.FromPosition + label.ToPosition) / 2.0;
                                if (middlePoint < this.ViewMinimum || middlePoint > this.ViewMaximum)
                                {
                                    continue;
                                }
                            }

                            if (label.RowIndex == groupLevelIndex)
                            {
                                // Calculate label rect
                                double fromPosition = this.GetLinearPosition(label.FromPosition);
                                double toPosition = this.GetLinearPosition(label.ToPosition);
                                if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                                {
                                    rect.Height = (maxLabelSize / 100F) * maxAxisLabelRow2Size / groupLabelLevelCount;
                                    rect.X = (float)Math.Min(fromPosition, toPosition);
                                    rect.Width = (float)Math.Max(fromPosition, toPosition) - rect.X;
                                }
                                else
                                {
                                    rect.Width = (maxLabelSize / 100F) * maxAxisLabelRow2Size / groupLabelLevelCount;
                                    rect.Y = (float)Math.Min(fromPosition, toPosition);
                                    rect.Height = (float)Math.Max(fromPosition, toPosition) - rect.Y;
                                }

                                // Measure string
                                SizeF axisLabelSize = chartGraph.MeasureStringRel(label.Text.Replace("\\n", "\n"), autoLabelFont);

                                // Add image size
                                if (label.Image.Length > 0)
                                {
                                    SizeF imageAbsSize = new SizeF();

                                    if (this.Common.ImageLoader.GetAdjustedImageSize(label.Image, chartGraph.Graphics, ref imageAbsSize))
                                    {
                                        SizeF imageRelSize = chartGraph.GetRelativeSize(imageAbsSize);
                                        axisLabelSize.Width += imageRelSize.Width;
                                        axisLabelSize.Height = Math.Max(axisLabelSize.Height, imageRelSize.Height);
                                    }
                                }

                                // Add extra spacing for the box marking of the label
                                if (label.LabelMark == LabelMarkStyle.Box)
                                {
                                    // Get relative size from pixels and add it to the label size
                                    SizeF spacerSize = chartGraph.GetRelativeSize(new SizeF(4, 4));
                                    axisLabelSize.Width += spacerSize.Width;
                                    axisLabelSize.Height += spacerSize.Height;
                                }

                                // Calculate max height of the second row of labels
                                if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                                {
                                    groupingLabelSizes[groupLevelIndex - 1] = (float)Math.Max(groupingLabelSizes[groupLevelIndex - 1], axisLabelSize.Height);
                                }
                                else
                                {
                                    axisLabelSize.Width = chartGraph.GetAbsoluteSize(new SizeF(axisLabelSize.Height, axisLabelSize.Height)).Height;
                                    axisLabelSize.Width = chartGraph.GetRelativeSize(new SizeF(axisLabelSize.Width, axisLabelSize.Width)).Width;
                                    groupingLabelSizes[groupLevelIndex - 1] = (float)Math.Max(groupingLabelSizes[groupLevelIndex - 1], axisLabelSize.Width);
                                }

                                // Check if string fits
                                if (Math.Round(axisLabelSize.Width) >= Math.Round(rect.Width) &&
                                    checkWidth)
                                {
                                    fitResult = false;
                                }
                                if (Math.Round(axisLabelSize.Height) >= Math.Round(rect.Height) &&
                                    checkHeight)
                                {
                                    fitResult = false;
                                }
                            }
                        }
                    }

                    this.totlaGroupingLabelsSize = this.GetGroupLablesToatalSize();
                    if (!fitResult && !checkLabelsFirstRowOnly)
                    {
                        return false;
                    }

                }

                // Loop through all labels in the first row
                float angle = autoLabelAngle;
                int labelIndex = 0;
                foreach (CustomLabel label in this.CustomLabels)
                {
                    // Skip if label middle point is outside current scaleView
                    if (label.RowIndex == 0)
                    {
                        double middlePoint = (label.FromPosition + label.ToPosition) / 2.0;
                        if (middlePoint < this.ViewMinimum || middlePoint > this.ViewMaximum)
                        {
                            continue;
                        }
                    }

                    if (label.RowIndex == 0)
                    {

                        // Force which scale segment to use when calculating label position
                        if (labelPositions != null)
                        {
                            this.ScaleSegments.EnforceSegment(this.ScaleSegments.FindScaleSegmentForAxisValue((label.FromPosition + label.ToPosition) / 2.0));
                        }


                        // Set label From and To coordinates
                        double fromPosition = this.GetLinearPosition(label.FromPosition);
                        double toPosition = this.GetLinearPosition(label.ToPosition);


                        // Reset scale segment to use when calculating label position
                        if (labelPositions != null)
                        {
                            this.ScaleSegments.EnforceSegment(null);
                        }


                        // Calculate single label position
                        rect.X = this.PlotAreaPosition.X;
                        rect.Y = (float)Math.Min(fromPosition, toPosition);
                        rect.Height = (float)Math.Max(fromPosition, toPosition) - rect.Y;

                        float maxElementSize = maxAxisElementsSize;
                        if (maxAxisElementsSize - this.totlaGroupingLabelsSize > 55)
                        {
                            maxElementSize = 55 + this.totlaGroupingLabelsSize;
                        }
                        if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                        {
                            rect.Width = (maxLabelSize / 100F) *
                                (maxElementSize - this.totlaGroupingLabelsSize - otherElementsSize - elementSpacing);
                        }
                        else
                        {
                            rect.Width = (maxLabelSize / 100F) *
                                (maxElementSize - this.totlaGroupingLabelsSize - otherElementsSize - elementSpacing);
                        }

                        // Adjust label From/To position if labels are displayed with offset
                        if (autoLabelOffset == 1)
                        {
                            rect.Y -= rect.Height / 2F;
                            rect.Height *= 2F;
                            rect.Width /= 2F;
                        }

                        // If horizontal axis
                        if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                        {
                            // Switch rectangle sizes
                            float val = rect.Height;
                            rect.Height = rect.Width;
                            rect.Width = val;

                            // Set vertical font for measuring
                            if (angle != 0)
                            {
                                format.FormatFlags |= StringFormatFlags.DirectionVertical;
                            }
                        }
                        else
                        {
                            // Set vertical font for measuring
                            if (angle == 90 || angle == -90)
                            {
                                angle = 0;
                                format.FormatFlags |= StringFormatFlags.DirectionVertical;
                            }
                        }

                        // Measure label text size. Add the 'I' character to allow a little bit of spacing between labels.
                        SizeF axisLabelSize = chartGraph.MeasureStringRel(
                            label.Text.Replace("\\n", "\n") + "W",
                            autoLabelFont,
                            (secondPass) ? rect.Size : ChartArea.Position.ToRectangleF().Size,
                            format);

                        // Width and height maybe zeros if rect is too small to fit the text and
                        // the LineLimit format flag is set. 
                        if (label.Text.Length > 0 &&
                            (axisLabelSize.Width == 0f ||
                            axisLabelSize.Height == 0f))
                        {
                            // Measure string without the LineLimit flag
                            format.FormatFlags ^= StringFormatFlags.LineLimit;
                            axisLabelSize = chartGraph.MeasureStringRel(
                                label.Text.Replace("\\n", "\n"),
                                autoLabelFont,
                                (secondPass) ? rect.Size : ChartArea.Position.ToRectangleF().Size,
                                format);
                            format.FormatFlags |= StringFormatFlags.LineLimit;
                        }


                        // Add image size
                        if (label.Image.Length > 0)
                        {
                            SizeF imageAbsSize = new SizeF();

                            if(this.Common.ImageLoader.GetAdjustedImageSize(label.Image, chartGraph.Graphics, ref imageAbsSize))
                            {
                                SizeF imageRelSize = chartGraph.GetRelativeSize(imageAbsSize);
                                if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical)
                                {
                                    axisLabelSize.Height += imageRelSize.Height;
                                    axisLabelSize.Width = Math.Max(axisLabelSize.Width, imageRelSize.Width);
                                }
                                else
                                {
                                    axisLabelSize.Width += imageRelSize.Width;
                                    axisLabelSize.Height = Math.Max(axisLabelSize.Height, imageRelSize.Height);
                                }
                            }
                        }

                        // Add extra spacing for the box marking of the label
                        if (label.LabelMark == LabelMarkStyle.Box)
                        {
                            // Get relative size from pixels and add it to the label size
                            SizeF spacerSize = chartGraph.GetRelativeSize(new SizeF(4, 4));
                            axisLabelSize.Width += spacerSize.Width;
                            axisLabelSize.Height += spacerSize.Height;
                        }


                        // Calculate size using label angle
                        float width = axisLabelSize.Width;
                        float height = axisLabelSize.Height;
                        if (angle != 0)
                        {
                            // Decrease label rectangle width by 3%
                            rect.Width *= 0.97f;

                            if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                            {
                                width = (float)Math.Cos((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;
                                width += (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;

                                height = (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;
                                height += (float)Math.Cos((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;
                            }
                            else
                            {
                                width = (float)Math.Cos((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;
                                width += (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;

                                height = (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;
                                height += (float)Math.Cos((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;
                            }
                        }

                        // Save label position
                        if (labelPositions != null)
                        {
                            RectangleF labelPosition = rect;
                            if (angle == 0F || angle == 90F || angle == -90F)
                            {
                                if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                                {
                                    labelPosition.X = labelPosition.X + labelPosition.Width / 2f - width / 2f;
                                    labelPosition.Width = width;
                                }
                                else
                                {
                                    labelPosition.Y = labelPosition.Y + labelPosition.Height / 2f - height / 2f;
                                    labelPosition.Height = height;
                                }
                            }
                            labelPositions.Add(labelPosition);
                        }

                        // Check if string fits
                        if (angle == 0F)
                        {
                            if (width >= rect.Width && checkWidth)
                            {
                                return false;
                            }
                            if (height >= rect.Height && checkHeight)
                            {
                                return false;
                            }
                        }
                        if (angle == 90F || angle == -90F)
                        {
                            if (width >= rect.Width && checkWidth)
                            {
                                return false;
                            }
                            if (height >= rect.Height && checkHeight)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                            {
                                if (width >= rect.Width * 2F && checkWidth)
                                {
                                    return false;
                                }
                                if (height >= rect.Height * 2F && checkHeight)
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                if (width >= rect.Width * 2F && checkWidth)
                                {
                                    return false;
                                }
                                if (height >= rect.Height * 2F && checkHeight)
                                {
                                    return false;
                                }
                            }
                        }

                        ++labelIndex;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Calculates the best size for labels area.
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="maxLabelSize">Maximum labels area size.</param>
        /// <param name="resultSize">Label size without angle = 0.</param>
        private float GetRequiredLabelSize(ChartGraphics chartGraph, float maxLabelSize, out float resultSize)
        {
            float resultRotatedSize = 0F;
            resultSize = 0F;
            float angle = (autoLabelAngle < -90) ? this.LabelStyle.Angle : autoLabelAngle;
            labelNearOffset = float.MaxValue;
            labelFarOffset = float.MinValue;

            // Label string drawing format			
            using (StringFormat format = new StringFormat())
            {
                format.FormatFlags |= StringFormatFlags.LineLimit;
                format.Trimming = StringTrimming.EllipsisCharacter;

                // Initialize all labels position rectangle
                RectangleF rectLabels = ChartArea.Position.ToRectangleF();

                // Loop through all labels in the first row
                foreach (CustomLabel label in this.CustomLabels)
                {
                    // Skip if label middle point is outside current scaleView
                    if (label.RowIndex == 0)
                    {
                        decimal middlePoint = (decimal)(label.FromPosition + label.ToPosition) / (decimal)2.0;
                        if (middlePoint < (decimal)this.ViewMinimum || middlePoint > (decimal)this.ViewMaximum)
                        {
                            continue;
                        }
                    }
                    if (label.RowIndex == 0)
                    {
                        // Calculate single label position
                        RectangleF rect = rectLabels;
                        rect.Width = maxLabelSize;

                        // Set label From and To coordinates
                        double fromPosition = this.GetLinearPosition(label.FromPosition);
                        double toPosition = this.GetLinearPosition(label.ToPosition);
                        rect.Y = (float)Math.Min(fromPosition, toPosition);
                        rect.Height = (float)Math.Max(fromPosition, toPosition) - rect.Y;

                        // Adjust label From/To position if labels are displayed with offset
                        if ((autoLabelOffset == -1) ? this.LabelStyle.IsStaggered : (autoLabelOffset == 1))
                        {
                            rect.Y -= rect.Height / 2F;
                            rect.Height *= 2F;
                        }

                        // If horizontal axis
                        if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                        {
                            // Switch rectangle sizes
                            float val = rect.Height;
                            rect.Height = rect.Width;
                            rect.Width = val;

                            // Set vertical font for measuring
                            if (angle != 0)
                            {
                                format.FormatFlags |= StringFormatFlags.DirectionVertical;
                            }
                        }
                        else
                        {
                            // Set vertical font for measuring
                            if (angle == 90 || angle == -90)
                            {
                                angle = 0;
                                format.FormatFlags |= StringFormatFlags.DirectionVertical;
                            }
                        }

                        // Measure label text size
                        rect.Width = (float)Math.Ceiling(rect.Width);
                        rect.Height = (float)Math.Ceiling(rect.Height);
                        SizeF axisLabelSize = chartGraph.MeasureStringRel(label.Text.Replace("\\n", "\n"),
                            (autoLabelFont != null) ? autoLabelFont : this.LabelStyle.Font,
                            rect.Size,
                            format);

                        // Width and height maybe zeros if rect is too small to fit the text and
                        // the LineLimit format flag is set. 
                        if (axisLabelSize.Width == 0f || axisLabelSize.Height == 0f)
                        {
                            // Measure string without the LineLimit flag
                            format.FormatFlags ^= StringFormatFlags.LineLimit;
                            axisLabelSize = chartGraph.MeasureStringRel(label.Text.Replace("\\n", "\n"),
                                (autoLabelFont != null) ? autoLabelFont : this.LabelStyle.Font,
                                rect.Size,
                                format);
                            format.FormatFlags |= StringFormatFlags.LineLimit;
                        }


                        // Add image size
                        if (label.Image.Length > 0)
                        {
                            SizeF imageAbsSize = new SizeF();

                            if (this.Common.ImageLoader.GetAdjustedImageSize(label.Image, chartGraph.Graphics, ref imageAbsSize))
                            {
                                SizeF imageRelSize = chartGraph.GetRelativeSize(imageAbsSize);
                                
                                if ((format.FormatFlags & StringFormatFlags.DirectionVertical) == StringFormatFlags.DirectionVertical)
                                {
                                    axisLabelSize.Height += imageRelSize.Height;
                                    axisLabelSize.Width = Math.Max(axisLabelSize.Width, imageRelSize.Width);
                                }
                                else
                                {
                                    axisLabelSize.Width += imageRelSize.Width;
                                    axisLabelSize.Height = Math.Max(axisLabelSize.Height, imageRelSize.Height);
                                }
                            }
                        }

                        // Add extra spacing for the box marking of the label
                        if (label.LabelMark == LabelMarkStyle.Box)
                        {
                            // Get relative size from pixels and add it to the label size
                            SizeF spacerSize = chartGraph.GetRelativeSize(new SizeF(4, 4));
                            axisLabelSize.Width += spacerSize.Width;
                            axisLabelSize.Height += spacerSize.Height;
                        }


                        // Calculate size using label angle
                        float width = axisLabelSize.Width;
                        float height = axisLabelSize.Height;
                        if (angle != 0)
                        {
                            width = (float)Math.Cos((90 - Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;
                            width += (float)Math.Cos((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;

                            height = (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Height;
                            height += (float)Math.Sin((90 - Math.Abs(angle)) / 180F * Math.PI) * axisLabelSize.Width;
                        }

                        width = (float)Math.Ceiling(width) * 1.05f;
                        height = (float)Math.Ceiling(height) * 1.05f;
                        axisLabelSize.Width = (float)Math.Ceiling(axisLabelSize.Width) * 1.05f;
                        axisLabelSize.Height = (float)Math.Ceiling(axisLabelSize.Height) * 1.05f;


                        // If axis is horizontal
                        if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                        {
                            if (angle == 90 || angle == -90 || angle == 0)
                            {
                                resultSize = Math.Max(resultSize, axisLabelSize.Height);
                                resultRotatedSize = Math.Max(resultRotatedSize, axisLabelSize.Height);

                                // Calculate the over hang of labels on the side
                                labelNearOffset = (float)Math.Min(labelNearOffset, (fromPosition + toPosition) / 2f - axisLabelSize.Width / 2f);
                                labelFarOffset = (float)Math.Max(labelFarOffset, (fromPosition + toPosition) / 2f + axisLabelSize.Width / 2f);

                            }
                            else
                            {
                                resultSize = Math.Max(resultSize, axisLabelSize.Height);
                                resultRotatedSize = Math.Max(resultRotatedSize, height);

                                // Calculate the over hang of labels on the side
                                if (angle > 0)
                                {
                                    labelFarOffset = (float)Math.Max(labelFarOffset, (fromPosition + toPosition) / 2f + width * 1.1f);
                                }
                                else
                                {
                                    labelNearOffset = (float)Math.Min(labelNearOffset, (fromPosition + toPosition) / 2f - width * 1.1f);
                                }
                            }
                        }
                        // If axis is vertical
                        else
                        {
                            if (angle == 90 || angle == -90 || angle == 0)
                            {
                                resultSize = Math.Max(resultSize, axisLabelSize.Width);
                                resultRotatedSize = Math.Max(resultRotatedSize, axisLabelSize.Width);

                                // Calculate the over hang of labels on the side
                                labelNearOffset = (float)Math.Min(labelNearOffset, (fromPosition + toPosition) / 2f - axisLabelSize.Height / 2f);
                                labelFarOffset = (float)Math.Max(labelFarOffset, (fromPosition + toPosition) / 2f + axisLabelSize.Height / 2f);
                            }
                            else
                            {
                                resultSize = Math.Max(resultSize, axisLabelSize.Width);
                                resultRotatedSize = Math.Max(resultRotatedSize, width);

                                // Calculate the over hang of labels on the side
                                if (angle > 0)
                                {
                                    labelFarOffset = (float)Math.Max(labelFarOffset, (fromPosition + toPosition) / 2f + height * 1.1f);
                                }
                                else
                                {
                                    labelNearOffset = (float)Math.Min(labelNearOffset, (fromPosition + toPosition) / 2f - height * 1.1f);
                                }
                            }
                        }

                        // Check if we exceed the maximum value
                        if (resultSize > maxLabelSize)
                        {
                            resultSize = maxLabelSize;
                        }
                    }
                }
            }

            // Adjust results if labels are displayed with offset
            if ((autoLabelOffset == -1) ? this.LabelStyle.IsStaggered : (autoLabelOffset == 1))
            {
                resultSize *= 2F;
                resultRotatedSize *= 2F;

                // Check if we exceed the maximum value
                if (resultSize > maxLabelSize)
                {
                    resultSize = maxLabelSize;
                    resultRotatedSize = maxLabelSize;
                }
            }

            // Adjust labels size for the 3D Common.Chart
            if (ChartArea.Area3DStyle.Enable3D && !ChartArea.chartAreaIsCurcular)
            {
                // Increase labels size
                resultSize *= 1.1f;
                resultRotatedSize *= 1.1f;
            }

            return resultRotatedSize;
        }

        /// <summary>
        /// Gets total size of all grouping labels.
        /// </summary>
        /// <returns>Total size of all grouping labels.</returns>
        internal float GetGroupLablesToatalSize()
        {
            float size = 0f;
            if (this.groupingLabelSizes != null && this.groupingLabelSizes.Length > 0)
            {
                foreach (float val in this.groupingLabelSizes)
                {
                    size += val;
                }
            }

            return size;
        }

        /// <summary>
        /// Gets number of levels of the grouping labels.
        /// </summary>
        /// <returns>Number of levels of the grouping labels.</returns>
        internal int GetGroupLabelLevelCount()
        {
            int groupLabelLevel = 0;
            foreach (CustomLabel label in this.CustomLabels)
            {
                if (label.RowIndex > 0)
                {
                    groupLabelLevel = Math.Max(groupLabelLevel, label.RowIndex);
                }
            }

            return groupLabelLevel;
        }

        /// <summary>
        /// Calculates the best size for axis labels for all rows except first one (grouping labels).
        /// </summary>
        /// <param name="chartGraph">Chart graphics object.</param>
        /// <param name="maxLabelSize">Maximum labels area size.</param>
        /// <returns>Array of grouping label sizes for each level.</returns>
        private float[] GetRequiredGroupLabelSize(ChartGraphics chartGraph, float maxLabelSize)
        {
            float[] resultSize = null;

            // Get number of groups
            int groupLabelLevelCount = GetGroupLabelLevelCount();

            // Check ig grouping labels exist
            if (groupLabelLevelCount > 0)
            {
                // Create result array
                resultSize = new float[groupLabelLevelCount];

                // Loop through each level of grouping labels
                for (int groupLevelIndex = 1; groupLevelIndex <= groupLabelLevelCount; groupLevelIndex++)
                {
                    resultSize[groupLevelIndex - 1] = 0f;

                    // Loop through all labels in the level
                    foreach (CustomLabel label in this.CustomLabels)
                    {
                        // Skip if label middle point is outside current scaleView
                        if (label.RowIndex == 0)
                        {
                            double middlePoint = (label.FromPosition + label.ToPosition) / 2.0;
                            if (middlePoint < this.ViewMinimum || middlePoint > this.ViewMaximum)
                            {
                                continue;
                            }
                        }

                        if (label.RowIndex == groupLevelIndex)
                        {
                            // Measure label text size
                            SizeF axisLabelSize = chartGraph.MeasureStringRel(label.Text.Replace("\\n", "\n"), (autoLabelFont != null) ? autoLabelFont : this.LabelStyle.Font);
                            axisLabelSize.Width = (float)Math.Ceiling(axisLabelSize.Width);
                            axisLabelSize.Height = (float)Math.Ceiling(axisLabelSize.Height);


							// Add image size
							if(label.Image.Length > 0)
							{
                                SizeF imageAbsSize = new SizeF();

                                if(this.Common.ImageLoader.GetAdjustedImageSize(label.Image, chartGraph.Graphics, ref imageAbsSize))
								{
									SizeF imageRelSize = chartGraph.GetRelativeSize(imageAbsSize);
									axisLabelSize.Width += imageRelSize.Width;
									axisLabelSize.Height = Math.Max(axisLabelSize.Height, imageRelSize.Height);
								}
							}

							// Add extra spacing for the box marking of the label
							if(label.LabelMark == LabelMarkStyle.Box)
							{
								// Get relative size from pixels and add it to the label size
								SizeF	spacerSize = chartGraph.GetRelativeSize(new SizeF(4, 4));
								axisLabelSize.Width += spacerSize.Width;
								axisLabelSize.Height += spacerSize.Height;
							}



                            // If axis is horizontal
                            if (this.AxisPosition == AxisPosition.Bottom || this.AxisPosition == AxisPosition.Top)
                            {
                                resultSize[groupLevelIndex - 1] = Math.Max(resultSize[groupLevelIndex - 1], axisLabelSize.Height);
                            }
                            // If axis is vertical
                            else
                            {
                                axisLabelSize.Width = chartGraph.GetAbsoluteSize(new SizeF(axisLabelSize.Height, axisLabelSize.Height)).Height;
                                axisLabelSize.Width = chartGraph.GetRelativeSize(new SizeF(axisLabelSize.Width, axisLabelSize.Width)).Width;
                                resultSize[groupLevelIndex - 1] = Math.Max(resultSize[groupLevelIndex - 1], axisLabelSize.Width);
                            }

                            // Check if we exceed the maximum value
                            if (resultSize[groupLevelIndex - 1] > maxLabelSize / groupLabelLevelCount)
                            {
                                // NOTE: Group Labels size limitations are removed !!!
                                //	resultSize[groupLevelIndex - 1] = maxLabelSize / groupLabelLevelCount;
                                //	break;
                            }
                        }
                    }
                }
            }

            return resultSize;
        }

        #endregion

        #region Axis helper methods


        /// <summary>
        /// Gets main or sub axis associated with this axis.
        /// </summary>
        /// <param name="subAxisName">Sub axis name or empty string to get the main axis.</param>
        /// <returns>Main or sub axis of the main axis.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "subAxisName")]
        internal Axis GetSubAxis(string subAxisName)
        {
#if SUBAXES
			if(!this.IsSubAxis && subAxisName.Length > 0)
			{
				SubAxis subAxis = this.SubAxes.FindByName(subAxisName);
				if(subAxis == null)
				{
					throw(new InvalidOperationException( SR.ExceptionSubAxisNameNotFoundShort( subAxisName )));
				}
				return subAxis;
			}
#endif // SUBAXES
            return this;
        }

        /// <summary>
        /// Checks if axis marks should be next to the axis
        /// </summary>
        /// <returns>true if marks are next to axis.</returns>
        internal bool GetIsMarksNextToAxis()
        {
            if (ChartArea != null && ChartArea.chartAreaIsCurcular)
            {
                return true;
            }
            return this.IsMarksNextToAxis;
        }

		/// <summary>
		/// Gets axis auto interval type.
		/// </summary>
		/// <returns>Axis interval type.</returns>
		internal DateTimeIntervalType GetAxisIntervalType()
		{
			if(InternalIntervalType == DateTimeIntervalType.Auto)
			{
				if(GetAxisValuesType() == ChartValueType.DateTime ||
					GetAxisValuesType() == ChartValueType.Date ||
					GetAxisValuesType() == ChartValueType.Time ||
                    GetAxisValuesType() == ChartValueType.DateTimeOffset)
				{
					return DateTimeIntervalType.Years;
				}

                return DateTimeIntervalType.Number;
            }

            return InternalIntervalType;
        }

        /// <summary>
        /// Gets axis values type depending on the series attached
        /// </summary>
        /// <returns>Axis values type.</returns>
        internal ChartValueType GetAxisValuesType()
        {
            ChartValueType type = ChartValueType.Double;

            // Check all series in this Common.Chart area attached to this axis
            if (this.Common != null && this.Common.DataManager.Series != null && ChartArea != null)
            {
                foreach (Series series in this.Common.DataManager.Series)
                {
                    bool seriesAttached = false;

                    // Check series name
                    if (series.ChartArea == ChartArea.Name && series.IsVisible())
                    {
                        // Check if axis type of series match
                        if (this.axisType == AxisName.X && series.XAxisType == AxisType.Primary)
                        {
#if SUBAXES
							if(((Axis)this).SubAxisName == series.XSubAxisName)
#endif // SUBAXES
                            {
                                seriesAttached = true;
                            }
                        }
                        else if (this.axisType == AxisName.X2 && series.XAxisType == AxisType.Secondary)
                        {
#if SUBAXES
							if(((Axis)this).SubAxisName == series.XSubAxisName)
#endif // SUBAXES
                            {
                                seriesAttached = true;
                            }
                        }
                        else if (this.axisType == AxisName.Y && series.YAxisType == AxisType.Primary)
                        {
#if SUBAXES
							if(((Axis)this).SubAxisName == series.YSubAxisName)
#endif // SUBAXES
                            {
                                seriesAttached = true;
                            }
                        }
                        else if (this.axisType == AxisName.Y2 && series.YAxisType == AxisType.Secondary)
                        {
#if SUBAXES
							if(((Axis)this).SubAxisName == series.YSubAxisName)
#endif // SUBAXES
                            {
                                seriesAttached = true;
                            }
                        }
                    }

                    // If series attached to this axes
                    if (seriesAttached)
                    {
                        if (this.axisType == AxisName.X || this.axisType == AxisName.X2)
                        {
                            type = series.XValueType;
                        }
                        else if (this.axisType == AxisName.Y || this.axisType == AxisName.Y2)
                        {
                            type = series.YValueType;
                        }
                        break;
                    }
                }
            }
            return type;
        }

        /// <summary>
        /// Returns Arrow size
        /// </summary>
        /// <param name="arrowOrientation">Return arrow orientation.</param>
        /// <returns>Size of arrow</returns>
        internal SizeF GetArrowSize(out ArrowOrientation arrowOrientation)
        {
            Axis opositeAxis;
            double size;
            double sizeOpposite;
            arrowOrientation = ArrowOrientation.Top;

            // Set the position of axis
            switch (AxisPosition)
            {
                case AxisPosition.Left:

                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Bottom;
                    else
                        arrowOrientation = ArrowOrientation.Top;

                    break;
                case AxisPosition.Right:

                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Bottom;
                    else
                        arrowOrientation = ArrowOrientation.Top;

                    break;
                case AxisPosition.Bottom:

                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Left;
                    else
                        arrowOrientation = ArrowOrientation.Right;

                    break;
                case AxisPosition.Top:

                    if (isReversed)
                        arrowOrientation = ArrowOrientation.Left;
                    else
                        arrowOrientation = ArrowOrientation.Right;

                    break;
            }

            // Opposite axis. Arrow uses this axis to find 
            // a shift from Common.Chart area border. This shift 
            // depend on Tick mark size.
            switch (arrowOrientation)
            {
                case ArrowOrientation.Left:
                    opositeAxis = ChartArea.AxisX;
                    break;
                case ArrowOrientation.Right:
                    opositeAxis = ChartArea.AxisX2;
                    break;
                case ArrowOrientation.Top:
                    opositeAxis = ChartArea.AxisY2;
                    break;
                case ArrowOrientation.Bottom:
                    opositeAxis = ChartArea.AxisY;
                    break;
                default:
                    opositeAxis = ChartArea.AxisX;
                    break;
            }

            // Arrow size has to have the same shape when width and height 
            // are changed. When the picture is resized, width of the Common.Chart 
            // picture is used only for arrow size.
            if (arrowOrientation == ArrowOrientation.Top || arrowOrientation == ArrowOrientation.Bottom)
            {
                size = _lineWidth;
                sizeOpposite = (double)(_lineWidth) * Common.Width / Common.Height;
            }
            else
            {
                size = (double)(_lineWidth) * Common.Width / Common.Height;
                sizeOpposite = _lineWidth;
            }

            // Arrow is sharp triangle
            if (_arrowStyle == AxisArrowStyle.SharpTriangle)
            {
                // Arrow direction is vertical
                if (arrowOrientation == ArrowOrientation.Top || arrowOrientation == ArrowOrientation.Bottom)
                    return new SizeF((float)(size * 2), (float)(opositeAxis.MajorTickMark.Size + sizeOpposite * 4));
                else
                    // Arrow direction is horizontal
                    return new SizeF((float)(opositeAxis.MajorTickMark.Size + sizeOpposite * 4), (float)(size * 2));
            }
            // There is no arrow
            else if (_arrowStyle == AxisArrowStyle.None)
                return new SizeF(0, 0);
            else// Arrow is triangle or line type
            {
                // Arrow direction is vertical
                if (arrowOrientation == ArrowOrientation.Top || arrowOrientation == ArrowOrientation.Bottom)
                    return new SizeF((float)(size * 2), (float)(opositeAxis.MajorTickMark.Size + sizeOpposite * 2));
                else
                    // Arrow direction is horizontal
                    return new SizeF((float)(opositeAxis.MajorTickMark.Size + sizeOpposite * 2), (float)(size * 2));
            }
        }


        /// <summary>
        /// Checks if arrow with specified orientation will require space
        /// in axis with specified position
        /// </summary>
        /// <param name="arrowOrientation">Arrow orientation.</param>
        /// <param name="axisPosition">Axis position.</param>
        /// <returns>True if arrow will be drawn in axis space</returns>
        private bool IsArrowInAxis(ArrowOrientation arrowOrientation, AxisPosition axisPosition)
        {
            if (axisPosition == AxisPosition.Top && arrowOrientation == ArrowOrientation.Top)
                return true;
            else if (axisPosition == AxisPosition.Bottom && arrowOrientation == ArrowOrientation.Bottom)
                return true;
            if (axisPosition == AxisPosition.Left && arrowOrientation == ArrowOrientation.Left)
                return true;
            else if (axisPosition == AxisPosition.Right && arrowOrientation == ArrowOrientation.Right)
                return true;

            return false;
        }


        /// <summary>
        /// This function converts real Interval to 
        /// absolute Interval
        /// </summary>
        /// <param name="realInterval">A interval represented as double value</param>
        /// <returns>A interval represented in pixels</returns>
        internal float GetPixelInterval(double realInterval)
        {
            double chartAreaSize;

            // The Chart area pixel size as double
            if (AxisPosition == AxisPosition.Top || AxisPosition == AxisPosition.Bottom)
            {
                chartAreaSize = PlotAreaPosition.Right - PlotAreaPosition.X;
            }
            else
            {
                chartAreaSize = PlotAreaPosition.Bottom - PlotAreaPosition.Y;
            }

            // Avoid division by zero.
            if (ViewMaximum - ViewMinimum == 0)
            {
                return (float)(chartAreaSize / realInterval);
            }

            // The interval integer
            return (float)(chartAreaSize / (ViewMaximum - ViewMinimum) * realInterval);
        }

        /// <summary>
        /// Find if axis is on the edge of the Common.Chart plot area
        /// </summary>
        internal bool IsAxisOnAreaEdge
        {
            get
            {
                double edgePosition = 0;
                if (this.AxisPosition == AxisPosition.Bottom)
                {
                    edgePosition = PlotAreaPosition.Bottom;
                }
                else if (this.AxisPosition == AxisPosition.Left)
                {
                    edgePosition = PlotAreaPosition.X;
                }
                else if (this.AxisPosition == AxisPosition.Right)
                {
                    edgePosition = PlotAreaPosition.Right;
                }
                else if (this.AxisPosition == AxisPosition.Top)
                {
                    edgePosition = PlotAreaPosition.Y;
                }

                // DT Fix : problems with values on edge ~0.0005
                if (Math.Abs(GetAxisPosition() - edgePosition) < 0.0015)
                {
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Find axis position using crossing value.
        /// </summary>
        /// <returns>Relative position</returns>
        internal double GetAxisPosition()
        {
            return GetAxisPosition(false);
        }

        /// <summary>
        /// Find axis position using crossing value.
        /// </summary>
        /// <param name="ignoreCrossing">Axis crossing should be ignored.</param>
        /// <returns>Relative position</returns>
        virtual internal double GetAxisPosition(bool ignoreCrossing)
        {
            Axis axisOpposite = GetOppositeAxis();

            // Get axis position for circular Common.Chart area
            if (ChartArea != null && ChartArea.chartAreaIsCurcular)
            {
                return PlotAreaPosition.X + PlotAreaPosition.Width / 2f;
            }

            // Axis is not connected with any series. There is no maximum and minimum
            if (axisOpposite.maximum == axisOpposite.minimum ||
                double.IsNaN(axisOpposite.maximum) ||
                double.IsNaN(axisOpposite.minimum) ||
                maximum == minimum ||
                double.IsNaN(maximum) ||
                double.IsNaN(minimum))
            {
                switch (AxisPosition)
                {
                    case AxisPosition.Top:
                        return PlotAreaPosition.Y;
                    case AxisPosition.Bottom:
                        return PlotAreaPosition.Bottom;
                    case AxisPosition.Right:
                        return PlotAreaPosition.Right;
                    case AxisPosition.Left:
                        return PlotAreaPosition.X;
                }
            }

            // Auto crossing enabled
            if (Double.IsNaN(axisOpposite.crossing) || ignoreCrossing)
            {
                // Primary
                if (axisType == AxisName.X || axisType == AxisName.Y)
                    return axisOpposite.GetLinearPosition(axisOpposite.ViewMinimum);
                else // Secondary
                    return axisOpposite.GetLinearPosition(axisOpposite.ViewMaximum);
            }
            else // Auto crossing disabled
            {
                axisOpposite.crossing = axisOpposite.tempCrossing;

                if (axisOpposite.crossing < axisOpposite.ViewMinimum)
                {
                    axisOpposite.crossing = axisOpposite.ViewMinimum;
                }
                else if (axisOpposite.crossing > axisOpposite.ViewMaximum)
                {
                    axisOpposite.crossing = axisOpposite.ViewMaximum;
                }
            }

            return axisOpposite.GetLinearPosition(axisOpposite.crossing);
        }

        #endregion

        #region Axis 3D helper methods

        /// <summary>
        /// Returns angle between 2D axis line and it's 3D transformed projection.
        /// </summary>
        /// <returns>Axis projection angle.</returns>
        internal double GetAxisProjectionAngle()
        {
            // Get Z position
            bool axisOnEdge;
            float zPosition = GetMarksZPosition(out axisOnEdge);

            // Get axis position
            float axisPosition = (float)GetAxisPosition();

            // Create two points on the sides of the axis
            Point3D[] axisPoints = new Point3D[2];
            if (this.AxisPosition == AxisPosition.Top || this.AxisPosition == AxisPosition.Bottom)
            {
                axisPoints[0] = new Point3D(0f, axisPosition, zPosition);
                axisPoints[1] = new Point3D(100f, axisPosition, zPosition);
            }
            else
            {
                axisPoints[0] = new Point3D(axisPosition, 0f, zPosition);
                axisPoints[1] = new Point3D(axisPosition, 100f, zPosition);
            }

            // Transform coordinates
            ChartArea.matrix3D.TransformPoints(axisPoints);

            // Round result
            axisPoints[0].X = (float)Math.Round(axisPoints[0].X, 4);
            axisPoints[0].Y = (float)Math.Round(axisPoints[0].Y, 4);
            axisPoints[1].X = (float)Math.Round(axisPoints[1].X, 4);
            axisPoints[1].Y = (float)Math.Round(axisPoints[1].Y, 4);

            // Calculate angle
            double angle = 0.0;
            if (this.AxisPosition == AxisPosition.Top || this.AxisPosition == AxisPosition.Bottom)
            {
                angle = Math.Atan((axisPoints[1].Y - axisPoints[0].Y) / (axisPoints[1].X - axisPoints[0].X));
            }
            else
            {
                angle = Math.Atan((axisPoints[1].X - axisPoints[0].X) / (axisPoints[1].Y - axisPoints[0].Y));
            }

            // Conver to degrees
            return (angle * 180.0) / Math.PI;
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

                if (labelStyle != null)
                {
                    labelStyle.Dispose();
                    labelStyle = null;
                }

                if (_stripLines != null)
                {
                    _stripLines.Dispose();
                    _stripLines = null;
                }
                if (_customLabels != null)
                {
                    _customLabels.Dispose();
                    _customLabels = null;
                }
                if (tempLabels != null)
                {
                    tempLabels.Dispose();
                    tempLabels = null;
                }
#if Microsoft_CONTROL
                if (this.scrollBar != null)
                {
                    this.scrollBar.Dispose();
                    this.scrollBar = null;
                }
#endif // Microsoft_CONTROL
            }
            base.Dispose(disposing);
        }


        #endregion

    }
}
