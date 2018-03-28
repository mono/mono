//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		LabelStyle.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	CustomLabelsCollection, CustomLabel, LabelStyle
//
//  Purpose:	LabelStyle and CustomLabel classes are used to determine 
//              chart axis labels. Labels can be automatically 
//              generated based on the series data or be “manually” 
//              set by the user.
//
//	Reviewed:	AG - Jul 31, 2002
//              AG - Microsoft 14, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

#if Microsoft_CONTROL
	using System.Windows.Forms.DataVisualization.Charting.Data;
	using System.Windows.Forms.DataVisualization.Charting.ChartTypes;
	using System.Windows.Forms.DataVisualization.Charting.Utilities;
	using System.Windows.Forms.DataVisualization.Charting.Borders3D;
	using System.Windows.Forms.DataVisualization.Charting;
    using System.Globalization;
	using System.ComponentModel.Design.Serialization;
	using System.Reflection;
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
	#region Labels enumerations

	/// <summary>
    /// An enumeration that specifies a mark for custom labels.
	/// </summary>
	public enum LabelMarkStyle
	{
		/// <summary>
        /// No label marks are used.
		/// </summary>
		None, 

		/// <summary>
		/// Labels use side marks.
		/// </summary>
		SideMark, 

		/// <summary>
		/// Labels use line and side marks.
		/// </summary>
		LineSideMark,


		/// <summary>
		/// Draws a box around the label. The box always starts at the axis position.
		/// </summary>
		Box
	};


	/// <summary>
	/// An enumeration of custom grid lines and tick marks flags used in the custom labels.
	/// </summary>
	[Flags]
	public enum GridTickTypes
	{
		/// <summary>
		/// No tick mark or grid line are shown.
		/// </summary>
		None = 0,

		/// <summary>
		/// Tick mark is shown.
		/// </summary>
		TickMark = 1,

		/// <summary>
		/// Grid line is shown.
		/// </summary>
		Gridline = 2,

		/// <summary>
		/// Tick mark and grid line are shown.
		/// </summary>
		All = TickMark | Gridline
	}


	/// <summary>
	/// An enumeration of label styles for circular chart area axis.
	/// </summary>
	internal enum CircularAxisLabelsStyle
	{
		/// <summary>
		/// Style depends on number of labels.
		/// </summary>
		Auto,

		/// <summary>
		/// Label text positions around the circular area.
		/// </summary>
		Circular,

		/// <summary>
		/// Label text is always horizontal.
		/// </summary>
		Horizontal,

		/// <summary>
		/// Label text has the same angle as circular axis.
		/// </summary>
		Radial
	}
	#endregion

	/// <summary>
    /// The CustomLabelsCollection class is a strongly typed collection of 
    /// custom axis labels.
	/// </summary>
	[
		SRDescription("DescriptionAttributeCustomLabelsCollection_CustomLabelsCollection"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class CustomLabelsCollection : ChartElementCollection<CustomLabel>
	{
		#region Constructors

		/// <summary>
		/// Custom labels collection object constructor
		/// </summary>
		/// <param name="axis">Reference to the axis object.</param>
        internal CustomLabelsCollection(Axis axis) : base(axis)
		{
		}

		#endregion

        #region Properties
        internal Axis Axis
        {
            get { return Parent as Axis; }
        }
        #endregion

        #region Labels adding methods

        /// <summary>
		/// Adds a custom label into the collection.
		/// </summary>
		/// <param name="fromPosition">Label left position.</param>
		/// <param name="toPosition">Label right position.</param>
		/// <param name="text">Label text.</param>
        /// <returns>Newly added item.</returns>
        public CustomLabel Add(double fromPosition, double toPosition, string text)
		{
			CustomLabel label = new CustomLabel(fromPosition, toPosition, text, 0, LabelMarkStyle.None);
            Add(label);
            return label;
		}

		/// <summary>
		/// Adds one custom label into the collection. Custom label flag may be specified.
		/// </summary>
		/// <param name="fromPosition">Label left position.</param>
		/// <param name="toPosition">Label right position.</param>
		/// <param name="text">Label text.</param>
		/// <param name="customLabel">Indicates if label is custom (created by user).</param>
		/// <returns>Newly added item.</returns>
		internal CustomLabel Add(double fromPosition, double toPosition, string text, bool customLabel)
		{
			CustomLabel label = new CustomLabel(fromPosition, toPosition, text, 0, LabelMarkStyle.None);
			label.customLabel = customLabel;
            Add(label);
            return label;
        }

		/// <summary>
		/// Adds a custom label into the collection.
		/// </summary>
		/// <param name="fromPosition">Label left position.</param>
		/// <param name="toPosition">Label right position.</param>
		/// <param name="text">Label text.</param>
		/// <param name="rowIndex">Label row index.</param>
		/// <param name="markStyle">Label marking style.</param>
        /// <returns>Newly added item.</returns>
		public CustomLabel Add(double fromPosition, double toPosition, string text, int rowIndex, LabelMarkStyle markStyle)
		{
			CustomLabel label = new CustomLabel(fromPosition, toPosition, text, rowIndex, markStyle);
            Add(label);
            return label;
        }

		/// <summary>
		/// Adds a custom label into the collection.
		/// </summary>
		/// <param name="fromPosition">Label left position.</param>
		/// <param name="toPosition">Label right position.</param>
		/// <param name="text">Label text.</param>
		/// <param name="rowIndex">Label row index.</param>
		/// <param name="markStyle">Label marking style.</param>
		/// <returns>Index of newly added item.</returns>
		/// <param name="gridTick">Custom grid line and tick mark flag.</param>
		public CustomLabel Add(double fromPosition, double toPosition, string text, int rowIndex, LabelMarkStyle markStyle, GridTickTypes gridTick)
		{
			CustomLabel label = new CustomLabel(fromPosition, toPosition, text, rowIndex, markStyle, gridTick);
            Add(label);
            return label;
        }


		/// <summary>
        /// Adds multiple custom labels to the collection.
        /// The labels will be DateTime labels with the specified interval type, 
        /// and will be generated for the axis range that is determined by the minimum and maximum arguments.
		/// </summary>
        /// <param name="labelsStep">The label step determines how often the custom labels will be drawn.</param>
		/// <param name="intervalType">Unit of measurement of the label step.</param>
		/// <param name="min">Minimum value..</param>
		/// <param name="max">Maximum value..</param>
		/// <param name="format">Label text format.</param>
		/// <param name="rowIndex">Label row index.</param>
		/// <param name="markStyle">Label marking style.</param>
		public void Add(double labelsStep, DateTimeIntervalType intervalType, double min, double max, string format, int rowIndex, LabelMarkStyle markStyle)
		{
            // Find labels range min/max values
			if(min == 0.0 && 
				max == 0.0 &&
				this.Axis != null &&
				!double.IsNaN(this.Axis.Minimum) &&
				!double.IsNaN(this.Axis.Maximum))
			{
				min = this.Axis.Minimum;
				max = this.Axis.Maximum;
			}
			double	fromX = Math.Min(min, max);
			double	toX = Math.Max(min, max);

            this.SuspendUpdates();
            try
            {

                // Loop through all label points
                double labelStart = fromX;
                double labelEnd = 0;
                while (labelStart < toX)
                {
                    // Determine label end location
                    if (intervalType == DateTimeIntervalType.Number)
                    {
                        labelEnd = labelStart + labelsStep;
                    }
                    else if (intervalType == DateTimeIntervalType.Milliseconds)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddMilliseconds(labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Seconds)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddSeconds(labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Minutes)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddMinutes(labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Hours)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddHours(labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Days)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddDays(labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Weeks)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddDays(7 * labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Months)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddMonths((int)labelsStep).ToOADate();
                    }
                    else if (intervalType == DateTimeIntervalType.Years)
                    {
                        labelEnd = DateTime.FromOADate(labelStart).AddYears((int)labelsStep).ToOADate();
                    }
                    else
                    {
                        // Unsupported step type
                        throw (new ArgumentException(SR.ExceptionAxisLabelsIntervalTypeUnsupported(intervalType.ToString())));
                    }
                    if (labelEnd > toX)
                    {
                        labelEnd = toX;
                    }

                    // Generate label text
                    ChartValueType valueType = ChartValueType.Double;
                    if (intervalType != DateTimeIntervalType.Number)
                    {
                        if (this.Axis.GetAxisValuesType() == ChartValueType.DateTimeOffset)
                            valueType = ChartValueType.DateTimeOffset;
                        else
                            valueType = ChartValueType.DateTime;
                    }
                    string text = ValueConverter.FormatValue(
                        this.Common.Chart,
                        this.Axis,
                        null,
                        labelStart + (labelEnd - labelStart) / 2,
                        format,
                        valueType,
                        ChartElementType.AxisLabels);

                    // Add label
                    CustomLabel label = new CustomLabel(labelStart, labelEnd, text, rowIndex, markStyle);
                    this.Add(label);

                    labelStart = labelEnd;
                }
            }
            finally
            {
                this.ResumeUpdates();
            }
		}

		/// <summary>
        /// Adds multiple custom labels to the collection.
        /// The labels will be DateTime labels with the specified interval type, 
        /// and will be generated for the axis range that is determined by the minimum and maximum arguments.
		/// </summary>
        /// <param name="labelsStep">The label step determines how often the custom labels will be drawn.</param>
        /// <param name="intervalType">Unit of measurement of the label step.</param>
		public void Add(double labelsStep, DateTimeIntervalType intervalType)
		{
			Add(labelsStep, intervalType, 0, 0, "", 0, LabelMarkStyle.None);
		}

		/// <summary>
        /// Adds multiple custom labels to the collection.
        /// The labels will be DateTime labels with the specified interval type, 
        /// and will be generated for the axis range that is determined by the minimum and maximum arguments.
        /// </summary>
        /// <param name="labelsStep">The label step determines how often the custom labels will be drawn.</param>
        /// <param name="intervalType">Unit of measurement of the label step.</param>
		/// <param name="format">Label text format.</param>
		public void Add(double labelsStep, DateTimeIntervalType intervalType, string format)
		{
			Add(labelsStep, intervalType, 0, 0, format, 0, LabelMarkStyle.None);
		}

		/// <summary>
        /// Adds multiple custom labels to the collection.
        /// The labels will be DateTime labels with the specified interval type, 
        /// and will be generated for the axis range that is determined by the minimum and maximum arguments.
        /// </summary>
        /// <param name="labelsStep">The label step determines how often the custom labels will be drawn.</param>
        /// <param name="intervalType">Unit of measurement of the label step.</param>
		/// <param name="format">Label text format.</param>
		/// <param name="rowIndex">Label row index.</param>
		/// <param name="markStyle">Label marking style.</param>
		public void Add(double labelsStep, DateTimeIntervalType intervalType, string format, int rowIndex, LabelMarkStyle markStyle)
		{
			Add(labelsStep, intervalType, 0, 0, format, rowIndex, markStyle);
		}

		#endregion

	}


	/// <summary>
    /// The CustomLabel class represents a single custom axis label. Text and 
    /// position along the axis is provided by the caller.
	/// </summary>
	[
	SRDescription("DescriptionAttributeCustomLabel_CustomLabel"),
	DefaultProperty("Text"),
	]
#if Microsoft_CONTROL
	public class CustomLabel : ChartNamedElement
#else
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class CustomLabel : ChartNamedElement, IChartMapArea
#endif
    {
		#region Fields and Constructors

		// Private data members, which store properties values
		private double			_fromPosition = 0;
		private double			_toPosition = 0;
		private string			_text = "";
		private LabelMarkStyle	_labelMark = LabelMarkStyle.None;
		private Color			_foreColor = Color.Empty;
		private Color			_markColor = Color.Empty;
		private int				_labelRowIndex = 0;

		// Custom grid lines and tick marks flags
		private	GridTickTypes	_gridTick = GridTickTypes.None;

		// Indicates if label was automatically created or cpecified by user (custom)
		internal bool			customLabel = true;

		// Image associated with the label
		private	string			_image = string.Empty;

		// Image transparent color
		private Color			_imageTransparentColor = Color.Empty;

		// Label tooltip
		private string			_tooltip = string.Empty;

        private Axis            _axis = null;
#if !Microsoft_CONTROL

		// URL target of the label image.
		private	string			_imageUrl = string.Empty;

		// Other attributes of the label image map area.
		private	string			_imageMapAreaAttributes = string.Empty;

        private string          _imagePostbackValue = String.Empty;

		// Label tooltip
		private string			_url = string.Empty;

		// Other attributes of the label image map area.
		private	string			_mapAreaAttributes = string.Empty;
        
        private string          _postbackValue = String.Empty;


#endif // !Microsoft_CONTROL



		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public CustomLabel()
		{
		}

		/// <summary>
		/// CustomLabel constructor
		/// </summary>
		/// <param name="fromPosition">From position.</param>
		/// <param name="toPosition">To position.</param>
		/// <param name="text">Label text.</param>
		/// <param name="labelRow">Label row index.</param>
		/// <param name="markStyle">Label mark style.</param>
		public CustomLabel(double fromPosition, double toPosition, string text, int labelRow, LabelMarkStyle markStyle)
		{
			this._fromPosition = fromPosition;
			this._toPosition = toPosition;
			this._text = text;
			this._labelRowIndex = labelRow;
			this._labelMark = markStyle;
			this._gridTick = GridTickTypes.None;
		}

		/// <summary>
		/// CustomLabel constructor
		/// </summary>
		/// <param name="fromPosition">From position.</param>
		/// <param name="toPosition">To position.</param>
		/// <param name="text">Label text.</param>
		/// <param name="labelRow">Label row index.</param>
		/// <param name="markStyle">Label mark style.</param>
		/// <param name="gridTick">Custom grid line and tick marks flag.</param>
		public CustomLabel(double fromPosition, double toPosition, string text, int labelRow, LabelMarkStyle markStyle, GridTickTypes gridTick)
		{
			this._fromPosition = fromPosition;
			this._toPosition = toPosition;
			this._text = text;
			this._labelRowIndex = labelRow;
			this._labelMark = markStyle;
			this._gridTick = gridTick;
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Returns a cloned label object.
		/// </summary>
		/// <returns>Copy of current custom label.</returns>
		public CustomLabel Clone()
		{
			CustomLabel newLabel = new CustomLabel();

			newLabel.FromPosition = this.FromPosition;
			newLabel.ToPosition = this.ToPosition;
			newLabel.Text = this.Text;
			newLabel.ForeColor = this.ForeColor;
			newLabel.MarkColor = this.MarkColor;
			newLabel.RowIndex = this.RowIndex;
			newLabel.LabelMark = this.LabelMark;
			newLabel.GridTicks = this.GridTicks;



			newLabel.ToolTip = this.ToolTip;
			newLabel.Tag = this.Tag;
			newLabel.Image = this.Image;
			newLabel.ImageTransparentColor = this.ImageTransparentColor;

#if !Microsoft_CONTROL
			newLabel.Url = this.Url;
			newLabel.MapAreaAttributes = this.MapAreaAttributes;
			newLabel.ImageUrl = this.ImageUrl;
			newLabel.ImageMapAreaAttributes = this.ImageMapAreaAttributes;
#endif // !Microsoft_CONTROL



			return newLabel;
		}

        internal override IChartElement Parent
        {
            get
            {
                return base.Parent;
            }
            set
            {
                base.Parent = value;
                if (value != null)
                {
                    _axis = Parent.Parent as Axis;
                }
            }
        }

		/// <summary>
		/// Gets the axis to which this object is attached to.
		/// </summary>
		/// <returns>Axis.</returns>
        [
        Browsable(false),
        DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
        SerializationVisibilityAttribute(SerializationVisibility.Hidden),
        ]
		public Axis Axis
		{
            get 
            {
                return _axis;
            }
		}

		#endregion

		#region	CustomLabel properties

		/// <summary>
		/// Gets or sets the tooltip of the custom label.
		/// </summary>
		[
#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
#endif //!Microsoft_CONTROL
		SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
        SRDescription("DescriptionAttributeToolTip"),
		DefaultValue("")
		]
		public string ToolTip
		{
			set
			{
				this._tooltip = value;
			}
			get
			{
				return this._tooltip;
			}
		}


#if !Microsoft_CONTROL

		/// <summary>
		/// URL target of the custom label.
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
		/// Gets or sets the other attributes of the custom label map area.
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

		/// <summary>
		/// Gets or sets the URL target of the custom label image.
		/// </summary>
		[
		SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
		SRDescription("DescriptionAttributeCustomLabel_ImageUrl"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute),
        Editor(Editors.UrlValueEditor.Editor, Editors.UrlValueEditor.Base),
        System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")
		]
		public string ImageUrl
		{
			set
			{
                this._imageUrl = value;
			}
			get
			{
                return this._imageUrl;
			}
		}

		/// <summary>
        /// Gets or sets the other attributes of the map area of the custom label image.
		/// </summary>
		[
		SRCategory("CategoryAttributeMapArea"),
		Bindable(true),
		SRDescription("DescriptionAttributeMapAreaAttributes"),
		DefaultValue(""),
		PersistenceMode(PersistenceMode.Attribute)
		]
		public string ImageMapAreaAttributes
		{
			set
			{
                this._imageMapAreaAttributes = value;
			}
			get
			{
                return this._imageMapAreaAttributes;
			}
		}

        /// <summary>
        /// Gets or sets the postback value which can be processed on a click event.
        /// </summary>
        /// <value>The value which is passed to a click event as an argument.</value>
        [DefaultValue("")]
        [SRCategory(SR.Keys.CategoryAttributeMapArea)]
        [SRDescription(SR.Keys.DescriptionAttributePostBackValue)]
        public string ImagePostBackValue
        {
            get
            {
                return this._imagePostbackValue;
            }
            set
            {
                this._imagePostbackValue = value;
            }
        }


#endif // !Microsoft_CONTROL

        /// <summary>
		/// Gets or sets the label image.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(""),
		SRDescription("DescriptionAttributeCustomLabel_Image"),
        Editor(Editors.ImageValueEditor.Editor, Editors.ImageValueEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
		NotifyParentPropertyAttribute(true)
		]
		public string Image
		{
			get
			{
				return _image;
			}
			set
			{
				_image = value;
				Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets a color which will be replaced with a transparent color while drawing the image.
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
		public Color ImageTransparentColor
		{
			get
			{
				return _imageTransparentColor;
			}
			set
			{
				_imageTransparentColor = value;
				this.Invalidate();
			}
		}



		/// <summary>
		/// Custom label name. This property is for internal use only.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		SRDescription("DescriptionAttributeCustomLabel_Name"),
		DefaultValue("Custom LabelStyle"),
		DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
		DesignOnlyAttribute(true),
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
		/// Gets or sets a property which specifies whether
        /// custom tick marks and grid lines will be drawn in the center of the label.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(GridTickTypes.None),
		SRDescription("DescriptionAttributeCustomLabel_GridTicks"),
        Editor(Editors.FlagsEnumUITypeEditor.Editor, Editors.FlagsEnumUITypeEditor.Base)
		]
		public GridTickTypes GridTicks
		{
			get
			{
				return _gridTick;
			}
			set
			{
				_gridTick = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the end position of the custom label in axis coordinates.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeCustomLabel_From"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
        TypeConverter(typeof(AxisLabelDateValueConverter))
		]
		public double FromPosition
		{
			get
			{
				return _fromPosition;
			}
			set
			{
				_fromPosition = value;
				this.Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets the starting position of the custom label in axis coordinates.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(0.0),
		SRDescription("DescriptionAttributeCustomLabel_To"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
        TypeConverter(typeof(AxisLabelDateValueConverter))
		]
		public double ToPosition
		{
			get
			{
				return _toPosition;
			}
			set
			{
				_toPosition = value;
				this.Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets the text of the custom label.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(""),
		SRDescription("DescriptionAttributeCustomLabel_Text"),
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
				_text = value;
				this.Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets the text color of the custom label.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), ""),
        SRDescription("DescriptionAttributeForeColor"),
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
				this.Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets the color of the label mark line of the custom label.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), ""),
		SRDescription("DescriptionAttributeCustomLabel_MarkColor"),
		NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public Color MarkColor
		{
			get
			{
				return _markColor;
			}
			set
			{
				_markColor = value;
				this.Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets the row index of the custom label.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(0),
		SRDescription("DescriptionAttributeCustomLabel_RowIndex"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public int RowIndex
		{
			get
			{
				return this._labelRowIndex;
			}
			set
			{
				if(value < 0)
				{
                    throw (new InvalidOperationException(SR.ExceptionAxisLabelRowIndexIsNegative));
				}

				this._labelRowIndex = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets a property which define the marks for the labels in the second row. 
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(LabelMarkStyle.None),
		SRDescription("DescriptionAttributeCustomLabel_LabelMark"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public LabelMarkStyle LabelMark
		{
			get
			{
				return _labelMark;
			}
			set
			{
				_labelMark = value;
				this.Invalidate();
			}
		}

		#endregion

	}

	/// <summary>
    /// The LabelStyle class contains properties which define the visual appearance of 
    /// the axis labels, their interval and position. This class is also 
    /// responsible for calculating the position of all the labels and 
    /// drawing them. 
	/// </summary>
	[
		SRDescription("DescriptionAttributeLabel_Label"),
		DefaultProperty("Enabled"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class LabelStyle : ChartElement
	{
		#region Fields

		// Reference to the Axis 
		private Axis					_axis = null;

		// Private data members, which store properties values
		private bool					_enabled = true;

		internal double					intervalOffset = double.NaN;
		internal double					interval = double.NaN;
		internal DateTimeIntervalType	intervalType = DateTimeIntervalType.NotSet;
		internal DateTimeIntervalType	intervalOffsetType = DateTimeIntervalType.NotSet;

        private FontCache               _fontCache = new FontCache();
		private Font					_font;
		private Color					_foreColor = Color.Black;
		internal int					angle = 0;
		internal bool					isStaggered = false;
		private bool					_isEndLabelVisible = true;
		private bool					_truncatedLabels = false;
		private string					_format = "";

		#endregion

		#region Constructors

		/// <summary>
		/// Public default constructor.
		/// </summary>
		public LabelStyle()
		{
            _font = _fontCache.DefaultFont;
		}

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="axis">Axis which owns the grid.</param>
		internal LabelStyle(Axis axis) 
            : this()
		{
			_axis = axis;
		}

		#endregion

		#region Axis labels drawing methods

		/// <summary>
		/// Draws axis labels on the circular chart area.
		/// </summary>
		/// <param name="graph">Reference to the Chart Graphics object.</param>
		internal void PaintCircular( ChartGraphics graph )
		{
			// Label string drawing format			
            using (StringFormat format = new StringFormat())
            {
                format.FormatFlags |= StringFormatFlags.LineLimit;
                format.Trimming = StringTrimming.EllipsisCharacter;

                // Labels are disabled for this axis
                if (!_axis.LabelStyle.Enabled)
                    return;

                // Draw text with anti-aliasing
                /*
                if( (graph.AntiAliasing & AntiAliasing.Text) == AntiAliasing.Text )
                {
                    graph.TextRenderingHint = TextRenderingHint.AntiAlias;
                }
                else
                {
                    graph.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
                }
                */

                // Gets axis labels style
                CircularAxisLabelsStyle labelsStyle = this._axis.ChartArea.GetCircularAxisLabelsStyle();

                // Get list of circular axes with labels
                ArrayList circularAxes = this._axis.ChartArea.GetCircularAxisList();

                // Draw each axis label
                int index = 0;
                foreach (CircularChartAreaAxis circAxis in circularAxes)
                {
                    if (circAxis.Title.Length > 0)
                    {
                        //******************************************************************
                        //** Calculate label position corner position
                        //******************************************************************
                        PointF labelRelativePosition = new PointF(
                            this._axis.ChartArea.circularCenter.X,
                            this._axis.ChartArea.PlotAreaPosition.Y);

                        // Adjust labels Y position
                        labelRelativePosition.Y -= _axis.markSize + Axis.elementSpacing;

                        // Convert to absolute
                        PointF[] labelPosition = new PointF[] { graph.GetAbsolutePoint(labelRelativePosition) };

                        // Get label rotation angle
                        float labelAngle = circAxis.AxisPosition;
                        ICircularChartType chartType = this._axis.ChartArea.GetCircularChartType();
                        if (chartType != null && chartType.XAxisCrossingSupported())
                        {
                            if (!double.IsNaN(this._axis.ChartArea.AxisX.Crossing))
                            {
                                labelAngle += (float)this._axis.ChartArea.AxisX.Crossing;
                            }
                        }

                        // Make sure angle is presented as a positive number
                        while (labelAngle < 0)
                        {
                            labelAngle = 360f + labelAngle;
                        }

                        // Set graphics rotation matrix
                        Matrix newMatrix = new Matrix();
                        newMatrix.RotateAt(labelAngle, graph.GetAbsolutePoint(this._axis.ChartArea.circularCenter));
                        newMatrix.TransformPoints(labelPosition);

                        // Set text alignment
                        format.LineAlignment = StringAlignment.Center;
                        format.Alignment = StringAlignment.Near;
                        if (labelsStyle != CircularAxisLabelsStyle.Radial)
                        {
                            if (labelAngle < 5f || labelAngle > 355f)
                            {
                                format.Alignment = StringAlignment.Center;
                                format.LineAlignment = StringAlignment.Far;
                            }
                            if (labelAngle < 185f && labelAngle > 175f)
                            {
                                format.Alignment = StringAlignment.Center;
                                format.LineAlignment = StringAlignment.Near;
                            }
                            if (labelAngle > 185f && labelAngle < 355f)
                            {
                                format.Alignment = StringAlignment.Far;
                            }
                        }
                        else
                        {
                            if (labelAngle > 180f)
                            {
                                format.Alignment = StringAlignment.Far;
                            }
                        }


                        // Set text rotation angle
                        float textAngle = labelAngle;
                        if (labelsStyle == CircularAxisLabelsStyle.Radial)
                        {
                            if (labelAngle > 180)
                            {
                                textAngle += 90f;
                            }
                            else
                            {
                                textAngle -= 90f;
                            }
                        }
                        else if (labelsStyle == CircularAxisLabelsStyle.Circular)
                        {
                            format.Alignment = StringAlignment.Center;
                            format.LineAlignment = StringAlignment.Far;
                        }

                        // Set text rotation matrix
                        Matrix oldMatrix = graph.Transform;
                        if (labelsStyle == CircularAxisLabelsStyle.Radial || labelsStyle == CircularAxisLabelsStyle.Circular)
                        {
                            Matrix textRotationMatrix = oldMatrix.Clone();
                            textRotationMatrix.RotateAt(textAngle, labelPosition[0]);
                            graph.Transform = textRotationMatrix;
                        }

                        // Get axis titl (label) color
                        Color labelColor = _foreColor;
                        if (!circAxis.TitleForeColor.IsEmpty)
                        {
                            labelColor = circAxis.TitleForeColor;
                        }

                        // Draw label
                        using (Brush brush = new SolidBrush(labelColor))
                        {
                            graph.DrawString(
                                circAxis.Title.Replace("\\n", "\n"),
                                (_axis.autoLabelFont == null) ? _font : _axis.autoLabelFont,
                                brush,
                                labelPosition[0],
                                format);
                        }

                        // Process selection region 
                        if (this._axis.Common.ProcessModeRegions)
                        {
                            SizeF size = graph.MeasureString(circAxis.Title.Replace("\\n", "\n"), (_axis.autoLabelFont == null) ? _font : _axis.autoLabelFont);
                            RectangleF labelRect = GetLabelPosition(
                                labelPosition[0],
                                size,
                                format);
                            PointF[] points = new PointF[]
							{
								labelRect.Location, 
								new PointF(labelRect.Right, labelRect.Y),
								new PointF(labelRect.Right, labelRect.Bottom),
								new PointF(labelRect.X, labelRect.Bottom)
							};

                            using (GraphicsPath path = new GraphicsPath())
                            {
                                path.AddPolygon(points);
                                path.CloseAllFigures();
                                path.Transform(graph.Transform);
                                this._axis.Common.HotRegionsList.AddHotRegion(
                                    path,
                                    false,
                                    ChartElementType.AxisLabels,
                                    circAxis.Title);
                            }
                    }

					// Restore graphics
					if(labelsStyle == CircularAxisLabelsStyle.Radial || labelsStyle == CircularAxisLabelsStyle.Circular)
					{
						graph.Transform = oldMatrix;
					}
				}

                    ++index;
                }
            }

		}

		/// <summary>
		/// Gets rectangle position of the label.
		/// </summary>
		/// <param name="position">Original label position.</param>
		/// <param name="size">Label text size.</param>
		/// <param name="format">Label string format.</param>
		/// <returns>Label rectangle position.</returns>
		internal static RectangleF GetLabelPosition(
			PointF position, 
			SizeF size,
			StringFormat format)
		{
			// Calculate label position rectangle
			RectangleF	labelPosition = RectangleF.Empty;
			labelPosition.Width = size.Width;
			labelPosition.Height = size.Height;

			if(format.Alignment == StringAlignment.Far)
			{
				labelPosition.X = position.X - size.Width;
			}
			else if(format.Alignment == StringAlignment.Near)
			{
				labelPosition.X = position.X;
			}
			else if(format.Alignment == StringAlignment.Center)
			{
				labelPosition.X = position.X - size.Width/2F;
			}

			if(format.LineAlignment == StringAlignment.Far)
			{
				labelPosition.Y = position.Y - size.Height;
			}
			else if(format.LineAlignment == StringAlignment.Near)
			{
				labelPosition.Y = position.Y;
			}
			else if(format.LineAlignment == StringAlignment.Center)
			{
				labelPosition.Y = position.Y - size.Height/2F;
			}

			return labelPosition;
		}

		/// <summary>
		/// Draws axis labels.
		/// </summary>
		/// <param name="graph">Reference to the Chart Graphics object.</param>
		/// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
		internal void Paint( ChartGraphics graph, bool backElements )
		{
			// Label string drawing format			
            using (StringFormat format = new StringFormat())
            {
                format.FormatFlags |= StringFormatFlags.LineLimit;
                format.Trimming = StringTrimming.EllipsisCharacter;

                // Labels are disabled for this axis
                if (!_axis.LabelStyle.Enabled)
                    return;
                // deliant fix-> VSTS #157848, #143286 - drawing custom label in empty axis
                if (Double.IsNaN(_axis.ViewMinimum) || Double.IsNaN(_axis.ViewMaximum))
                    return;


                // Draw labels in 3D space
                if (this._axis.ChartArea.Area3DStyle.Enable3D && !this._axis.ChartArea.chartAreaIsCurcular)
                {
                    this.Paint3D(graph, backElements);
                    return;
                }

                // Initialize all labels position rectangle
                RectangleF rectLabels = _axis.ChartArea.Position.ToRectangleF();
                float labelSize = _axis.labelSize;

                if (_axis.AxisPosition == AxisPosition.Left)
                {
                    rectLabels.Width = labelSize;
                    if (_axis.GetIsMarksNextToAxis())
                        rectLabels.X = (float)_axis.GetAxisPosition();
                    else
                        rectLabels.X = _axis.PlotAreaPosition.X;

                    rectLabels.X -= labelSize + _axis.markSize;

                    // Set label text alignment
                    format.Alignment = StringAlignment.Far;
                    format.LineAlignment = StringAlignment.Center;
                }
                else if (_axis.AxisPosition == AxisPosition.Right)
                {
                    rectLabels.Width = labelSize;
                    if (_axis.GetIsMarksNextToAxis())
                        rectLabels.X = (float)_axis.GetAxisPosition();
                    else
                        rectLabels.X = _axis.PlotAreaPosition.Right;
                    rectLabels.X += _axis.markSize;

                    // Set label text alignment
                    format.Alignment = StringAlignment.Near;
                    format.LineAlignment = StringAlignment.Center;
                }
                else if (_axis.AxisPosition == AxisPosition.Top)
                {
                    rectLabels.Height = labelSize;
                    if (_axis.GetIsMarksNextToAxis())
                        rectLabels.Y = (float)_axis.GetAxisPosition();
                    else
                        rectLabels.Y = _axis.PlotAreaPosition.Y;
                    rectLabels.Y -= labelSize + _axis.markSize;

                    // Set label text alignment
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Far;
                }
                else if (_axis.AxisPosition == AxisPosition.Bottom)
                {
                    rectLabels.Height = labelSize;
                    if (_axis.GetIsMarksNextToAxis())
                        rectLabels.Y = (float)_axis.GetAxisPosition();
                    else
                        rectLabels.Y = _axis.PlotAreaPosition.Bottom;
                    rectLabels.Y += _axis.markSize;

                    // Set label text alignment
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Near;
                }

                // Calculate bounding rectangle
                RectangleF boundaryRect = rectLabels;
                if (boundaryRect != RectangleF.Empty && _axis.totlaGroupingLabelsSize > 0)
                {
                    if (_axis.AxisPosition == AxisPosition.Left)
                    {
                        boundaryRect.X += _axis.totlaGroupingLabelsSize;
                        boundaryRect.Width -= _axis.totlaGroupingLabelsSize;
                    }
                    else if (_axis.AxisPosition == AxisPosition.Right)
                    {
                        boundaryRect.Width -= _axis.totlaGroupingLabelsSize;
                    }
                    else if (_axis.AxisPosition == AxisPosition.Top)
                    {
                        boundaryRect.Y += _axis.totlaGroupingLabelsSize;
                        boundaryRect.Height -= _axis.totlaGroupingLabelsSize;
                    }
                    else if (_axis.AxisPosition == AxisPosition.Bottom)
                    {
                        boundaryRect.Height -= _axis.totlaGroupingLabelsSize;
                    }
                }

                // Check if the AJAX zooming and scrolling mode is enabled.
                // Labels are drawn slightly different in this case.
                bool ajaxScrollingEnabled = false;
                bool firstFrame = true;
                bool lastFrame = true;

                // Draw all labels from the collection
                int labelIndex = 0;
                foreach (CustomLabel label in this._axis.CustomLabels)
                {
                    bool truncatedLeft = false;
                    bool truncatedRight = false;
                    double labelFrom = label.FromPosition;
                    double labelTo = label.ToPosition;
                    bool useRelativeCoordiantes = false;
                    double labelFromRelative = double.NaN;
                    double labelToRelative = double.NaN;

                    // Skip if label middle point is outside current scaleView
                    if (label.RowIndex == 0)
                    {
                        double middlePoint = (label.FromPosition + label.ToPosition) / 2.0;
                        decimal viewMin = (decimal)_axis.ViewMinimum;
                        decimal viewMax = (decimal)_axis.ViewMaximum;

                        if (ajaxScrollingEnabled)
                        {
                            // Skip very first and last labels if they are partialy outside the scaleView
                            if (firstFrame)
                            {
                                if ((decimal)label.FromPosition < (decimal)_axis.Minimum)
                                {
                                    continue;
                                }
                            }
                            if (lastFrame)
                            {
                                if ((decimal)label.ToPosition > (decimal)_axis.Maximum)
                                {
                                    continue;
                                }
                            }

                            // Skip label only if it is compleltly out of the scaleView
                            if ((decimal)label.ToPosition < viewMin ||
                                (decimal)label.FromPosition > viewMax)
                            {
                                continue;
                            }

                            // RecalculateAxesScale label index starting from the first frame.
                            // Index is used to determine position of the offset labels
                            if ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1))
                            {
                                // Reset index
                                labelIndex = 0;

                                // Get first series attached to this axis
                                Series axisSeries = null;
                                if (_axis.axisType == AxisName.X || _axis.axisType == AxisName.X2)
                                {
                                    List<string> seriesArray = _axis.ChartArea.GetXAxesSeries((_axis.axisType == AxisName.X) ? AxisType.Primary : AxisType.Secondary, _axis.SubAxisName);
                                    if (seriesArray.Count > 0)
                                    {
                                        axisSeries = _axis.Common.DataManager.Series[seriesArray[0]];
                                        if (axisSeries != null && !axisSeries.IsXValueIndexed)
                                        {
                                            axisSeries = null;
                                        }
                                    }
                                }

                                // Set start position and iterate through label positions
                                // NOTE: Labels offset should not be taken in the account
                                double currentPosition = _axis.Minimum;
                                while (currentPosition < _axis.Maximum)
                                {
                                    if (currentPosition >= middlePoint)
                                    {
                                        break;
                                    }

                                    currentPosition += ChartHelper.GetIntervalSize(currentPosition, _axis.LabelStyle.GetInterval(), _axis.LabelStyle.GetIntervalType(),
                                        axisSeries, 0.0, DateTimeIntervalType.Number, true);
                                    ++labelIndex;
                                }

                            }
                        }
                        else
                        {
                            // Skip label if label middle point is not in the scaleView
                            if ((decimal)middlePoint < viewMin ||
                                (decimal)middlePoint > viewMax)
                            {
                                continue;
                            }
                        }



                        // Make sure label To and From coordinates are processed by one scale segment based 
                        // on the label middle point position.
                        if (_axis.ScaleSegments.Count > 0)
                        {
                            AxisScaleSegment scaleSegment = _axis.ScaleSegments.FindScaleSegmentForAxisValue(middlePoint);
                            _axis.ScaleSegments.AllowOutOfScaleValues = true;
                            _axis.ScaleSegments.EnforceSegment(scaleSegment);
                        }



                        // Use center point instead of the To/From if label takes all scaleView
                        // This is done to avoid issues with labels drawing with high 
                        // zooming levels.
                        if ((decimal)label.FromPosition < viewMin &&
                            (decimal)label.ToPosition > viewMax)
                        {
                            // Indicates that chart relative coordinates should be used instead of axis values
                            useRelativeCoordiantes = true;

                            // Calculate label From/To in relative coordinates using 
                            // label middle point and 100% width.
                            labelFromRelative = _axis.GetLinearPosition(middlePoint) - 50.0;
                            labelToRelative = labelFromRelative + 100.0;
                        }
                    }
                    else
                    {
                        // Skip labels completly outside the scaleView
                        if (label.ToPosition <= _axis.ViewMinimum || label.FromPosition >= _axis.ViewMaximum)
                        {
                            continue;
                        }

                        // Check if label is partially visible.
                        if (!ajaxScrollingEnabled &&
                            _axis.ScaleView.IsZoomed)
                        {
                            if (label.FromPosition < _axis.ViewMinimum)
                            {
                                truncatedLeft = true;
                                labelFrom = _axis.ViewMinimum;
                            }
                            if (label.ToPosition > _axis.ViewMaximum)
                            {
                                truncatedRight = true;
                                labelTo = _axis.ViewMaximum;
                            }
                        }
                    }

                    // Calculate single label position
                    RectangleF rect = rectLabels;

                    // Label is in the first row
                    if (label.RowIndex == 0)
                    {
                        if (_axis.AxisPosition == AxisPosition.Left)
                        {
                            rect.X = rectLabels.Right - _axis.unRotatedLabelSize;
                            rect.Width = _axis.unRotatedLabelSize;

                            // Adjust label rectangle if offset labels are used
                            if ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1))
                            {
                                rect.Width /= 2F;
                                if (labelIndex % 2 != 0F)
                                {
                                    rect.X += rect.Width;
                                }
                            }
                        }
                        else if (_axis.AxisPosition == AxisPosition.Right)
                        {
                            rect.Width = _axis.unRotatedLabelSize;

                            // Adjust label rectangle if offset labels are used
                            if ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1))
                            {
                                rect.Width /= 2F;
                                if (labelIndex % 2 != 0F)
                                {
                                    rect.X += rect.Width;
                                }
                            }
                        }
                        else if (_axis.AxisPosition == AxisPosition.Top)
                        {
                            rect.Y = rectLabels.Bottom - _axis.unRotatedLabelSize;
                            rect.Height = _axis.unRotatedLabelSize;

                            // Adjust label rectangle if offset labels are used
                            if ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1))
                            {
                                rect.Height /= 2F;
                                if (labelIndex % 2 != 0F)
                                {
                                    rect.Y += rect.Height;
                                }
                            }
                        }
                        else if (_axis.AxisPosition == AxisPosition.Bottom)
                        {
                            rect.Height = _axis.unRotatedLabelSize;

                            // Adjust label rectangle if offset labels are used
                            if ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1))
                            {
                                rect.Height /= 2F;
                                if (labelIndex % 2 != 0F)
                                {
                                    rect.Y += rect.Height;
                                }
                            }
                        }

                        // Increase label index
                        ++labelIndex;
                    }

                    // Label is in the second row
                    else if (label.RowIndex > 0)
                    {
                        if (_axis.AxisPosition == AxisPosition.Left)
                        {
                            rect.X += _axis.totlaGroupingLabelsSizeAdjustment;
                            for (int index = _axis.groupingLabelSizes.Length; index > label.RowIndex; index--)
                            {
                                rect.X += _axis.groupingLabelSizes[index - 1];
                            }
                            rect.Width = _axis.groupingLabelSizes[label.RowIndex - 1];
                        }
                        else if (_axis.AxisPosition == AxisPosition.Right)
                        {
                            rect.X = rect.Right - _axis.totlaGroupingLabelsSize - _axis.totlaGroupingLabelsSizeAdjustment;// + Axis.elementSpacing * 0.25f;
                            for (int index = 1; index < label.RowIndex; index++)
                            {
                                rect.X += _axis.groupingLabelSizes[index - 1];
                            }
                            rect.Width = _axis.groupingLabelSizes[label.RowIndex - 1];
                        }
                        else if (_axis.AxisPosition == AxisPosition.Top)
                        {
                            rect.Y += _axis.totlaGroupingLabelsSizeAdjustment;
                            for (int index = _axis.groupingLabelSizes.Length; index > label.RowIndex; index--)
                            {
                                rect.Y += _axis.groupingLabelSizes[index - 1];
                            }
                            rect.Height = _axis.groupingLabelSizes[label.RowIndex - 1];
                        }
                        if (_axis.AxisPosition == AxisPosition.Bottom)
                        {
                            rect.Y = rect.Bottom - _axis.totlaGroupingLabelsSize - _axis.totlaGroupingLabelsSizeAdjustment;
                            for (int index = 1; index < label.RowIndex; index++)
                            {
                                rect.Y += _axis.groupingLabelSizes[index - 1];
                            }
                            rect.Height = _axis.groupingLabelSizes[label.RowIndex - 1];
                        }
                    }

                    // Unknown label row value
                    else
                    {
                        throw (new InvalidOperationException(SR.ExceptionAxisLabelIndexIsNegative));
                    }

                    // Set label From and To coordinates
                    double fromPosition = _axis.GetLinearPosition(labelFrom);
                    double toPosition = _axis.GetLinearPosition(labelTo);
                    if (useRelativeCoordiantes)
                    {
                        useRelativeCoordiantes = false;
                        fromPosition = labelFromRelative;
                        toPosition = labelToRelative;
                    }

                    if (_axis.AxisPosition == AxisPosition.Top || _axis.AxisPosition == AxisPosition.Bottom)
                    {
                        rect.X = (float)Math.Min(fromPosition, toPosition);
                        rect.Width = (float)Math.Max(fromPosition, toPosition) - rect.X;

                        // Adjust label To/From position if offset labels are used
                        if (label.RowIndex == 0 &&
                            ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1)))
                        {
                            rect.X -= rect.Width / 2F;
                            rect.Width *= 2F;
                        }
                    }
                    else
                    {
                        rect.Y = (float)Math.Min(fromPosition, toPosition);
                        rect.Height = (float)Math.Max(fromPosition, toPosition) - rect.Y;

                        // Adjust label To/From position if offset labels are used
                        if (label.RowIndex == 0 &&
                            ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1)))
                        {
                            rect.Y -= rect.Height / 2F;
                            rect.Height *= 2F;
                        }
                    }

                    // Draw label
                    using (Brush brush = new SolidBrush((label.ForeColor.IsEmpty) ? _foreColor : label.ForeColor))
                    {
                        graph.DrawLabelStringRel(_axis,
                            label.RowIndex,
                            label.LabelMark,
                            label.MarkColor,
                            label.Text,
                            label.Image,
                            label.ImageTransparentColor,
                            (_axis.autoLabelFont == null) ? _font : _axis.autoLabelFont,
                            brush,
                            rect,
                            format,
                            (_axis.autoLabelAngle < -90) ? angle : _axis.autoLabelAngle,
                            (!this.TruncatedLabels || label.RowIndex > 0) ? RectangleF.Empty : boundaryRect,
                            label,
                            truncatedLeft,
                            truncatedRight);
                    }

                    // Clear scale segment enforcement
                    _axis.ScaleSegments.EnforceSegment(null);
                    _axis.ScaleSegments.AllowOutOfScaleValues = false;
                }
            }
		}

		#endregion

		#region 3D Axis labels drawing methods

		/// <summary>
		/// Get a rectangle between chart area position and plotting area on specified side.
		/// Also sets axis labels string formatting for the specified labels position.
		/// </summary>
		/// <param name="area">Chart area object.</param>
		/// <param name="position">Position in chart area.</param>
		/// <param name="stringFormat">Axis labels string format.</param>
		/// <returns>Axis labels rectangle.</returns>
		private RectangleF GetAllLabelsRect(ChartArea area, AxisPosition position, StringFormat stringFormat)
		{
			// Find axis with same position
			Axis labelsAxis = null;
			foreach(Axis curAxis in area.Axes)
			{
				if(curAxis.AxisPosition == position)
				{
					labelsAxis = curAxis;
					break;
				}
			}

			if(labelsAxis == null)
			{
				return RectangleF.Empty;
			}

			// Calculate rect for different positions
			RectangleF rectLabels = area.Position.ToRectangleF();
			if( position == AxisPosition.Left )
			{
				rectLabels.Width = labelsAxis.labelSize;
				if( labelsAxis.GetIsMarksNextToAxis() )
				{
					rectLabels.X = (float)labelsAxis.GetAxisPosition();
					rectLabels.Width = (float)Math.Max(rectLabels.Width, rectLabels.X - labelsAxis.PlotAreaPosition.X);
				}
				else
				{
					rectLabels.X = labelsAxis.PlotAreaPosition.X;
				}

				rectLabels.X -= rectLabels.Width;

				if(area.IsSideSceneWallOnLeft() || area.Area3DStyle.WallWidth == 0)
				{
					rectLabels.X -= labelsAxis.markSize;
				}

				// Set label text alignment
				stringFormat.Alignment = StringAlignment.Far;
				stringFormat.LineAlignment = StringAlignment.Center;
			}
			else if( position == AxisPosition.Right )
			{
				rectLabels.Width = labelsAxis.labelSize;
				if( labelsAxis.GetIsMarksNextToAxis() )
				{
					rectLabels.X = (float)labelsAxis.GetAxisPosition();
					rectLabels.Width = (float)Math.Max(rectLabels.Width, labelsAxis.PlotAreaPosition.Right - rectLabels.X);
				}
				else
				{
					rectLabels.X = labelsAxis.PlotAreaPosition.Right;
				}
				
				if(!area.IsSideSceneWallOnLeft() || area.Area3DStyle.WallWidth == 0)
				{
					rectLabels.X += labelsAxis.markSize;
				}

				// Set label text alignment
				stringFormat.Alignment = StringAlignment.Near;
				stringFormat.LineAlignment = StringAlignment.Center;
			}
			else if( position == AxisPosition.Top )
			{
				rectLabels.Height = labelsAxis.labelSize;
				if( labelsAxis.GetIsMarksNextToAxis() )
				{
					rectLabels.Y = (float)labelsAxis.GetAxisPosition();
					rectLabels.Height = (float)Math.Max(rectLabels.Height, rectLabels.Y - labelsAxis.PlotAreaPosition.Y);
				}
				else
				{
					rectLabels.Y = labelsAxis.PlotAreaPosition.Y;
				}

				rectLabels.Y -= rectLabels.Height;

				if(area.Area3DStyle.WallWidth == 0)
				{
					rectLabels.Y -= labelsAxis.markSize;
				}

				// Set label text alignment
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Far;
			}
			else if( position == AxisPosition.Bottom )
			{
				rectLabels.Height = labelsAxis.labelSize;
				if( labelsAxis.GetIsMarksNextToAxis() )
				{
					rectLabels.Y = (float)labelsAxis.GetAxisPosition();
					rectLabels.Height = (float)Math.Max(rectLabels.Height, labelsAxis.PlotAreaPosition.Bottom - rectLabels.Y);
				}
				else
				{
					rectLabels.Y = labelsAxis.PlotAreaPosition.Bottom;
				}
				rectLabels.Y += labelsAxis.markSize;

				// Set label text alignment
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Near;
			}

			return rectLabels;
		}

        /// <summary>
        /// Gets position of axis labels.
        /// Top and Bottom axis labels can be drawn on the sides (left or right)
        /// of the plotting area. If angle between axis and it's projection is
        /// between -25 and 25 degrees the axis are drawn at the bottom/top, 
        /// otherwise labels are moved on the left or right side.
        /// </summary>
        /// <param name="axis">Axis object.</param>
        /// <returns>Position where axis labels should be drawn.</returns>
		private AxisPosition GetLabelsPosition(Axis axis)
		{
			// Get angle between 2D axis and it's 3D projection.
			double axisAngle = axis.GetAxisProjectionAngle();

			// Pick the side to draw the labels on
			if(axis.AxisPosition == AxisPosition.Bottom)
			{
				if(axisAngle <= -25 )
					return AxisPosition.Right;
				else if(axisAngle >= 25 )
					return AxisPosition.Left;
			}
			else if(axis.AxisPosition == AxisPosition.Top)
			{
				if(axisAngle <= -25 )
					return AxisPosition.Left;
				else if(axisAngle >= 25 )
					return AxisPosition.Right;
			}

			// Labels are on the same side as the axis
			return axis.AxisPosition;
		}

		/// <summary>
		/// Draws axis labels in 3D space.
		/// </summary>
		/// <param name="graph">Reference to the Chart Graphics object.</param>
		/// <param name="backElements">Back elements of the axis should be drawn in 3D scene.</param>
		internal void Paint3D( ChartGraphics graph, bool backElements )
		{
			// Label string drawing format			
            using (StringFormat format = new StringFormat())
            {
                format.Trimming = StringTrimming.EllipsisCharacter;

                // Calculate single pixel size in relative coordinates
                SizeF pixelSize = graph.GetRelativeSize(new SizeF(1f, 1f));

                //********************************************************************
                //** Determine the side of the plotting area to draw the labels on.
                //********************************************************************
                AxisPosition labelsPosition = GetLabelsPosition(_axis);

                //*****************************************************************
                //** Set the labels Z position
                //*****************************************************************
                bool axisOnEdge;
                float labelsZPosition = _axis.GetMarksZPosition(out axisOnEdge);

                // Adjust Z position for the "bent" tick marks
                bool adjustForWallWidth = false;
                if (this._axis.AxisPosition == AxisPosition.Top &&
                    !this._axis.ChartArea.ShouldDrawOnSurface(SurfaceNames.Top, backElements, false))
                {
                    adjustForWallWidth = true;
                }
                if (this._axis.AxisPosition == AxisPosition.Left &&
                    !this._axis.ChartArea.ShouldDrawOnSurface(SurfaceNames.Left, backElements, false))
                {
                    adjustForWallWidth = true;
                }
                if (this._axis.AxisPosition == AxisPosition.Right &&
                    !this._axis.ChartArea.ShouldDrawOnSurface(SurfaceNames.Right, backElements, false))
                {
                    adjustForWallWidth = true;
                }

                if (adjustForWallWidth && this._axis.ChartArea.Area3DStyle.WallWidth > 0)
                {
                    if (this._axis.MajorTickMark.TickMarkStyle == TickMarkStyle.InsideArea)
                    {
                        labelsZPosition -= this._axis.ChartArea.areaSceneWallWidth.Width;
                    }
                    else if (this._axis.MajorTickMark.TickMarkStyle == TickMarkStyle.OutsideArea)
                    {
                        labelsZPosition -= this._axis.MajorTickMark.Size + this._axis.ChartArea.areaSceneWallWidth.Width;
                    }
                    else if (this._axis.MajorTickMark.TickMarkStyle == TickMarkStyle.AcrossAxis)
                    {
                        labelsZPosition -= this._axis.MajorTickMark.Size / 2f + this._axis.ChartArea.areaSceneWallWidth.Width;
                    }
                }

                //*****************************************************************
                //** Check if labels should be drawn as back or front element.
                //*****************************************************************
                bool labelsInsidePlotArea = (this._axis.GetIsMarksNextToAxis() && !axisOnEdge);
                if (backElements == labelsInsidePlotArea)
                {
                    // Skip drawing
                    return;
                }

                //********************************************************************
                //** Initialize all labels position rectangle
                //********************************************************************
                RectangleF rectLabels = this.GetAllLabelsRect(this._axis.ChartArea, this._axis.AxisPosition, format);

                //********************************************************************
                //** Calculate bounding rectangle used to truncate labels on the
                //** chart area boundary if TruncatedLabels property is set to true.
                //********************************************************************
                RectangleF boundaryRect = rectLabels;
                if (boundaryRect != RectangleF.Empty && _axis.totlaGroupingLabelsSize > 0)
                {
                    if (this._axis.AxisPosition == AxisPosition.Left)
                    {
                        boundaryRect.X += _axis.totlaGroupingLabelsSize;
                        boundaryRect.Width -= _axis.totlaGroupingLabelsSize;
                    }
                    else if (this._axis.AxisPosition == AxisPosition.Right)
                    {
                        boundaryRect.Width -= _axis.totlaGroupingLabelsSize;
                    }
                    else if (this._axis.AxisPosition == AxisPosition.Top)
                    {
                        boundaryRect.Y += _axis.totlaGroupingLabelsSize;
                        boundaryRect.Height -= _axis.totlaGroupingLabelsSize;
                    }
                    else if (this._axis.AxisPosition == AxisPosition.Bottom)
                    {
                        boundaryRect.Height -= _axis.totlaGroupingLabelsSize;
                    }
                }

                // Pre-calculated height of the first labels row
                float firstLabelsRowHeight = -1f;

                // For 3D axis labels the first row of labels 
                // has to be drawn after all other rows because 
                // of hot regions.
                for (int selectionRow = 0; selectionRow <= this._axis.GetGroupLabelLevelCount(); selectionRow++)
                {
                    //********************************************************************
                    //** Draw all labels from the collection
                    //********************************************************************
                    int labelIndex = 0;
                    foreach (CustomLabel label in this._axis.CustomLabels)
                    {
                        bool truncatedLeft = false;
                        bool truncatedRight = false;
                        double labelFrom = label.FromPosition;
                        double labelTo = label.ToPosition;

                        if (label.RowIndex != selectionRow)
                        {
                            continue;
                        }

                        // Skip if label middle point is outside current scaleView
                        if (label.RowIndex == 0)
                        {
                            double middlePoint = (label.FromPosition + label.ToPosition) / 2.0;
                            if ((decimal)middlePoint < (decimal)_axis.ViewMinimum ||
                                (decimal)middlePoint > (decimal)_axis.ViewMaximum)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            // Skip labels completly outside the scaleView
                            if (label.ToPosition <= _axis.ViewMinimum || label.FromPosition >= _axis.ViewMaximum)
                            {
                                continue;
                            }

                            // Check if label is partially visible
                            if (_axis.ScaleView.IsZoomed)
                            {
                                if (label.FromPosition < _axis.ViewMinimum)
                                {
                                    truncatedLeft = true;
                                    labelFrom = _axis.ViewMinimum;
                                }
                                if (label.ToPosition > _axis.ViewMaximum)
                                {
                                    truncatedRight = true;
                                    labelTo = _axis.ViewMaximum;
                                }
                            }
                        }


                        // Calculate single label position
                        RectangleF rect = rectLabels;

                        // Label is in the first row
                        if (label.RowIndex == 0)
                        {
                            if (this._axis.AxisPosition == AxisPosition.Left)
                            {
                                if (!this._axis.GetIsMarksNextToAxis())
                                {
                                    rect.X = rectLabels.Right - _axis.unRotatedLabelSize;
                                    rect.Width = _axis.unRotatedLabelSize;
                                }

                                // Adjust label rectangle if offset labels are used
                                if ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1))
                                {
                                    rect.Width /= 2F;
                                    if (labelIndex % 2 != 0F)
                                    {
                                        rect.X += rect.Width;
                                    }
                                }
                            }
                            else if (this._axis.AxisPosition == AxisPosition.Right)
                            {
                                if (!this._axis.GetIsMarksNextToAxis())
                                {
                                    rect.Width = _axis.unRotatedLabelSize;
                                }

                                // Adjust label rectangle if offset labels are used
                                if ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1))
                                {
                                    rect.Width /= 2F;
                                    if (labelIndex % 2 != 0F)
                                    {
                                        rect.X += rect.Width;
                                    }
                                }
                            }
                            else if (this._axis.AxisPosition == AxisPosition.Top)
                            {
                                if (!this._axis.GetIsMarksNextToAxis())
                                {
                                    rect.Y = rectLabels.Bottom - _axis.unRotatedLabelSize;
                                    rect.Height = _axis.unRotatedLabelSize;
                                }

                                // Adjust label rectangle if offset labels are used
                                if ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1))
                                {
                                    rect.Height /= 2F;
                                    if (labelIndex % 2 != 0F)
                                    {
                                        rect.Y += rect.Height;
                                    }
                                }
                            }
                            else if (this._axis.AxisPosition == AxisPosition.Bottom)
                            {
                                if (!this._axis.GetIsMarksNextToAxis())
                                {
                                    rect.Height = _axis.unRotatedLabelSize;
                                }

                                // Adjust label rectangle if offset labels are used
                                if ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1))
                                {
                                    rect.Height /= 2F;
                                    if (labelIndex % 2 != 0F)
                                    {
                                        rect.Y += rect.Height;
                                    }
                                }
                            }

                            // Increase label index
                            ++labelIndex;
                        }

                            // Label is in the second row
                        else if (label.RowIndex > 0)
                        {
                            // Hide grouping labels (where index of row > 0) when they are displayed 
                            // not on the same side as their axis. Fixes MS issue #64.
                            if (labelsPosition != this._axis.AxisPosition)
                            {
                                continue;
                            }

                            if (_axis.AxisPosition == AxisPosition.Left)
                            {
                                rect.X += _axis.totlaGroupingLabelsSizeAdjustment;
                                for (int index = _axis.groupingLabelSizes.Length; index > label.RowIndex; index--)
                                {
                                    rect.X += _axis.groupingLabelSizes[index - 1];
                                }
                                rect.Width = _axis.groupingLabelSizes[label.RowIndex - 1];
                            }
                            else if (_axis.AxisPosition == AxisPosition.Right)
                            {
                                rect.X = rect.Right - _axis.totlaGroupingLabelsSize - _axis.totlaGroupingLabelsSizeAdjustment;// + Axis.elementSpacing * 0.25f;
                                for (int index = 1; index < label.RowIndex; index++)
                                {
                                    rect.X += _axis.groupingLabelSizes[index - 1];
                                }
                                rect.Width = _axis.groupingLabelSizes[label.RowIndex - 1];
                            }
                            else if (_axis.AxisPosition == AxisPosition.Top)
                            {
                                rect.Y += _axis.totlaGroupingLabelsSizeAdjustment;
                                for (int index = _axis.groupingLabelSizes.Length; index > label.RowIndex; index--)
                                {
                                    rect.Y += _axis.groupingLabelSizes[index - 1];
                                }
                                rect.Height = _axis.groupingLabelSizes[label.RowIndex - 1];
                            }
                            if (_axis.AxisPosition == AxisPosition.Bottom)
                            {
                                rect.Y = rect.Bottom - _axis.totlaGroupingLabelsSize - _axis.totlaGroupingLabelsSizeAdjustment;
                                for (int index = 1; index < label.RowIndex; index++)
                                {
                                    rect.Y += _axis.groupingLabelSizes[index - 1];
                                }
                                rect.Height = _axis.groupingLabelSizes[label.RowIndex - 1];
                            }
                        }

                            // Unknown label row value
                        else
                        {
                            throw (new InvalidOperationException(SR.ExceptionAxisLabelRowIndexMustBe1Or2));
                        }

                        //********************************************************************
                        //** Set label From and To coordinates.
                        //********************************************************************
                        double fromPosition = _axis.GetLinearPosition(labelFrom);
                        double toPosition = _axis.GetLinearPosition(labelTo);
                        if (this._axis.AxisPosition == AxisPosition.Top || this._axis.AxisPosition == AxisPosition.Bottom)
                        {
                            rect.X = (float)Math.Min(fromPosition, toPosition);
                            rect.Width = (float)Math.Max(fromPosition, toPosition) - rect.X;
                            if (rect.Width < pixelSize.Width)
                            {
                                rect.Width = pixelSize.Width;
                            }

                            // Adjust label To/From position if offset labels are used
                            if (label.RowIndex == 0 &&
                                ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1)))
                            {
                                rect.X -= rect.Width / 2F;
                                rect.Width *= 2F;
                            }
                        }
                        else
                        {
                            rect.Y = (float)Math.Min(fromPosition, toPosition);
                            rect.Height = (float)Math.Max(fromPosition, toPosition) - rect.Y;
                            if (rect.Height < pixelSize.Height)
                            {
                                rect.Height = pixelSize.Height;
                            }

                            // Adjust label To/From position if offset labels are used
                            if (label.RowIndex == 0 &&
                                ((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1)))
                            {
                                rect.Y -= rect.Height / 2F;
                                rect.Height *= 2F;
                            }
                        }

                        // Save original rect
                        RectangleF initialRect = new RectangleF(rect.Location, rect.Size);

                        //********************************************************************
                        //** Transform and adjust label rectangle coordinates in 3D space.
                        //********************************************************************
                        Point3D[] rectPoints = new Point3D[3];
                        if (this._axis.AxisPosition == AxisPosition.Left)
                        {
                            rectPoints[0] = new Point3D(rect.Right, rect.Y, labelsZPosition);
                            rectPoints[1] = new Point3D(rect.Right, rect.Y + rect.Height / 2f, labelsZPosition);
                            rectPoints[2] = new Point3D(rect.Right, rect.Bottom, labelsZPosition);
                            this._axis.ChartArea.matrix3D.TransformPoints(rectPoints);
                            rect.Y = rectPoints[0].Y;
                            rect.Height = rectPoints[2].Y - rect.Y;
                            rect.Width = rectPoints[1].X - rect.X;
                        }
                        else if (this._axis.AxisPosition == AxisPosition.Right)
                        {
                            rectPoints[0] = new Point3D(rect.X, rect.Y, labelsZPosition);
                            rectPoints[1] = new Point3D(rect.X, rect.Y + rect.Height / 2f, labelsZPosition);
                            rectPoints[2] = new Point3D(rect.X, rect.Bottom, labelsZPosition);
                            this._axis.ChartArea.matrix3D.TransformPoints(rectPoints);
                            rect.Y = rectPoints[0].Y;
                            rect.Height = rectPoints[2].Y - rect.Y;
                            rect.Width = rect.Right - rectPoints[1].X;
                            rect.X = rectPoints[1].X;
                        }
                        else if (this._axis.AxisPosition == AxisPosition.Top)
                        {
                            // Transform 3 points of the rectangle
                            rectPoints[0] = new Point3D(rect.X, rect.Bottom, labelsZPosition);
                            rectPoints[1] = new Point3D(rect.X + rect.Width / 2f, rect.Bottom, labelsZPosition);
                            rectPoints[2] = new Point3D(rect.Right, rect.Bottom, labelsZPosition);
                            this._axis.ChartArea.matrix3D.TransformPoints(rectPoints);

                            if (labelsPosition == AxisPosition.Top)
                            {
                                rect.X = rectPoints[0].X;
                                rect.Width = rectPoints[2].X - rect.X;
                                rect.Height = rectPoints[1].Y - rect.Y;
                            }
                            else if (labelsPosition == AxisPosition.Right)
                            {
                                RectangleF rightLabelsRect = this.GetAllLabelsRect(this._axis.ChartArea, labelsPosition, format);
                                rect.Y = rectPoints[0].Y;
                                rect.Height = rectPoints[2].Y - rect.Y;
                                rect.X = rectPoints[1].X;
                                rect.Width = rightLabelsRect.Right - rect.X;
                            }
                            else if (labelsPosition == AxisPosition.Left)
                            {
                                RectangleF rightLabelsRect = this.GetAllLabelsRect(this._axis.ChartArea, labelsPosition, format);
                                rect.Y = rectPoints[2].Y;
                                rect.Height = rectPoints[0].Y - rect.Y;
                                rect.X = rightLabelsRect.X;
                                rect.Width = rectPoints[1].X - rightLabelsRect.X;
                            }
                        }
                        else if (this._axis.AxisPosition == AxisPosition.Bottom)
                        {
                            // Transform 3 points of the rectangle
                            rectPoints[0] = new Point3D(rect.X, rect.Y, labelsZPosition);
                            rectPoints[1] = new Point3D(rect.X + rect.Width / 2f, rect.Y, labelsZPosition);
                            rectPoints[2] = new Point3D(rect.Right, rect.Y, labelsZPosition);
                            this._axis.ChartArea.matrix3D.TransformPoints(rectPoints);

                            if (labelsPosition == AxisPosition.Bottom)
                            {
                                rect.X = rectPoints[0].X;
                                rect.Width = rectPoints[2].X - rect.X;
                                rect.Height = rect.Bottom - rectPoints[1].Y;
                                rect.Y = rectPoints[1].Y;
                            }
                            else if (labelsPosition == AxisPosition.Right)
                            {
                                RectangleF rightLabelsRect = this.GetAllLabelsRect(this._axis.ChartArea, labelsPosition, format);
                                rect.Y = rectPoints[2].Y;
                                rect.Height = rectPoints[0].Y - rect.Y;
                                rect.X = rectPoints[1].X;
                                rect.Width = rightLabelsRect.Right - rect.X;

                                // Adjust label rect by shifting it down by quarter of the tick size
                                if (this._axis.autoLabelAngle == 0)
                                {
                                    rect.Y += this._axis.markSize / 4f;
                                }
                            }
                            else if (labelsPosition == AxisPosition.Left)
                            {
                                RectangleF rightLabelsRect = this.GetAllLabelsRect(this._axis.ChartArea, labelsPosition, format);
                                rect.Y = rectPoints[0].Y;
                                rect.Height = rectPoints[2].Y - rect.Y;
                                rect.X = rightLabelsRect.X;
                                rect.Width = rectPoints[1].X - rightLabelsRect.X;

                                // Adjust label rect by shifting it down by quarter of the tick size
                                if (this._axis.autoLabelAngle == 0)
                                {
                                    rect.Y += this._axis.markSize / 4f;
                                }
                            }
                        }

                        // Find axis with same position
                        Axis labelsAxis = null;
                        foreach (Axis curAxis in this._axis.ChartArea.Axes)
                        {
                            if (curAxis.AxisPosition == labelsPosition)
                            {
                                labelsAxis = curAxis;
                                break;
                            }
                        }

                        //********************************************************************
                        //** Adjust font angles for Top and Bottom axis
                        //********************************************************************
                        int labelsFontAngle = (_axis.autoLabelAngle < -90) ? angle : _axis.autoLabelAngle;
                        if (labelsPosition != this._axis.AxisPosition)
                        {
                            if ((this._axis.AxisPosition == AxisPosition.Top || this._axis.AxisPosition == AxisPosition.Bottom) &&
                                (labelsFontAngle == 90 || labelsFontAngle == -90))
                            {
                                labelsFontAngle = 0;
                            }
                            else if (this._axis.AxisPosition == AxisPosition.Bottom)
                            {
                                if (labelsPosition == AxisPosition.Left && labelsFontAngle > 0)
                                {
                                    labelsFontAngle = -labelsFontAngle;
                                }
                                else if (labelsPosition == AxisPosition.Right && labelsFontAngle < 0)
                                {
                                    labelsFontAngle = -labelsFontAngle;
                                }
                            }
                            else if (this._axis.AxisPosition == AxisPosition.Top)
                            {
                                if (labelsPosition == AxisPosition.Left && labelsFontAngle < 0)
                                {
                                    labelsFontAngle = -labelsFontAngle;
                                }
                                else if (labelsPosition == AxisPosition.Right && labelsFontAngle > 0)
                                {
                                    labelsFontAngle = -labelsFontAngle;
                                }
                            }
                        }

                        //********************************************************************
                        //** NOTE: Code below improves chart labels readability in scenarios 
                        //** described in MS issue #65.
                        //**
                        //** Prevent labels in the first row from overlapping the grouping
                        //** labels in the rows below. The solution only apply to the limited 
                        //** use cases defined by the condition below.
                        //********************************************************************
                        StringFormatFlags oldFormatFlags = format.FormatFlags; 

                        if (label.RowIndex == 0 &&
                            labelsFontAngle == 0 &&
                            _axis.groupingLabelSizes != null &&
                            _axis.groupingLabelSizes.Length > 0 &&
                            this._axis.AxisPosition == AxisPosition.Bottom &&
                            labelsPosition == AxisPosition.Bottom &&
                            !((this._axis.autoLabelOffset == -1) ? this.IsStaggered : (this._axis.autoLabelOffset == 1)))
                        {
                            if (firstLabelsRowHeight == -1f)
                            {
                                // Calculate first labels row max height
                                Point3D[] labelPositionPoints = new Point3D[1];
                                labelPositionPoints[0] = new Point3D(initialRect.X, initialRect.Bottom - _axis.totlaGroupingLabelsSize - _axis.totlaGroupingLabelsSizeAdjustment, labelsZPosition);
                                this._axis.ChartArea.matrix3D.TransformPoints(labelPositionPoints);

                                float height = labelPositionPoints[0].Y - rect.Y;

                                firstLabelsRowHeight = (height > 0f) ? height : rect.Height;
                            }

                            // Resuse pre-calculated first labels row height
                            rect.Height = firstLabelsRowHeight;

                            // Change current string format to prevent strings to go out of the 
                            // specified bounding rectangle
                            if ((format.FormatFlags & StringFormatFlags.LineLimit) == 0)
                            {
                                format.FormatFlags |= StringFormatFlags.LineLimit;
                            }
                        }


                        //********************************************************************
                        //** Draw label text.
                        //********************************************************************

                        using (Brush brush = new SolidBrush((label.ForeColor.IsEmpty) ? _foreColor : label.ForeColor))
                        {
                            graph.DrawLabelStringRel(
                                labelsAxis,
                                label.RowIndex,
                                label.LabelMark,
                                label.MarkColor,
                                label.Text,
                                label.Image,
                                label.ImageTransparentColor,
                                (_axis.autoLabelFont == null) ? _font : _axis.autoLabelFont,
                                brush,
                                rect,
                                format,
                                labelsFontAngle,
                                (!this.TruncatedLabels || label.RowIndex > 0) ? RectangleF.Empty : boundaryRect,
                                label,
                                truncatedLeft,
                                truncatedRight);
                        }

                        // Restore old string format that was temporary modified
                        if (format.FormatFlags != oldFormatFlags)
                        {
                            format.FormatFlags = oldFormatFlags;
                        }
                    }
                }
            }
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Sets the axis to which this object attached to.
		/// </summary>
		/// <returns>Axis object.</returns>
		internal Axis Axis
		{
            set { _axis = value; }
		}


		/// <summary>
		/// Invalidate chart picture
		/// </summary>
		internal override void Invalidate()
		{
#if Microsoft_CONTROL

			if(this._axis != null)
			{
				this._axis.Invalidate();
			}
#endif
            base.Invalidate();
		}

		#endregion

		#region	Label properties

		/// <summary>
		/// Gets or sets the interval offset of the label.
		/// </summary>
        [
        SRCategory("CategoryAttributeData"),
        Bindable(true),
        SRDescription("DescriptionAttributeLabel_IntervalOffset"),
        DefaultValue(Double.NaN),
        #if !Microsoft_CONTROL
         PersistenceMode(PersistenceMode.Attribute),
        #endif
         RefreshPropertiesAttribute(RefreshProperties.All),
        TypeConverter(typeof(AxisElementIntervalValueConverter))
        ]
        public double IntervalOffset
        {
            get
            {
                return intervalOffset;
            }
            set
            {
                intervalOffset = value;
                this.Invalidate();
            }
        }



        /// <summary>
        /// Gets the interval offset.
        /// </summary>
        /// <returns></returns>
        internal double GetIntervalOffset()
		{
			if(double.IsNaN(intervalOffset) && this._axis != null)
			{
				return this._axis.IntervalOffset;
			}
			return intervalOffset;
		}

		/// <summary>
		/// Gets or sets the unit of measurement of the label offset.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		Bindable(true),
		DefaultValue(DateTimeIntervalType.NotSet),
		SRDescription("DescriptionAttributeLabel_IntervalOffsetType"),
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
                intervalOffsetType = value;
                this.Invalidate();
            }
        }


        /// <summary>
        /// Gets the type of the interval offset.
        /// </summary>
        /// <returns></returns>
		internal DateTimeIntervalType GetIntervalOffsetType()
		{
			if(intervalOffsetType == DateTimeIntervalType.NotSet && this._axis != null)
			{
				return this._axis.IntervalOffsetType;
			}
			return intervalOffsetType;
		}

		/// <summary>
		/// Gets or sets the interval size of the label.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		Bindable(true),
		DefaultValue(Double.NaN),
		SRDescription("DescriptionAttributeLabel_Interval"),
        TypeConverter(typeof(AxisElementIntervalValueConverter)),
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
                interval = value;

                // Reset original property value fields
                if (this._axis != null)
                {
                    this._axis.tempLabelInterval = interval;
                }

                this.Invalidate();
            }
        }


        /// <summary>
        /// Gets the interval.
        /// </summary>
        /// <returns></returns>
		internal double GetInterval()
		{
				if(double.IsNaN(interval) && this._axis != null)
				{
					return this._axis.Interval;
				}
				return interval;
		}

		/// <summary>
        /// Gets or sets the unit of measurement of the interval size of the label.
		/// </summary>
		[
		SRCategory("CategoryAttributeData"),
		Bindable(true),
		DefaultValue(DateTimeIntervalType.NotSet),
		SRDescription("DescriptionAttributeLabel_IntervalType"),
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
                intervalType = value;

                // Reset original property value fields
                if (this._axis != null)
                {
                    this._axis.tempLabelIntervalType = intervalType;
                }

                this.Invalidate();
            }
        }


        /// <summary>
        /// Gets the type of the interval.
        /// </summary>
        /// <returns></returns>
		internal DateTimeIntervalType GetIntervalType()
		{
			if(intervalType == DateTimeIntervalType.NotSet && this._axis != null)
			{
				return this._axis.IntervalType;
			}
			return intervalType;
		}

		/// <summary>
        /// Gets or sets the font of the label.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Font), "Microsoft Sans Serif, 8pt"),
		SRDescription("DescriptionAttributeLabel_Font"),
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
				// Turn off labels autofitting 
                if (this._axis != null && this._axis.Common!=null && this._axis.Common.Chart != null)
				{
					if(!this._axis.Common.Chart.serializing)
					{
						this._axis.IsLabelAutoFit = false;
					}
				}

				_font = value;
				this.Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets the fore color of the label.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(typeof(Color), "Black"),
        SRDescription("DescriptionAttributeFontColor"),
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
				this.Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets a value that represents the angle at which font is drawn.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(0),
		SRDescription("DescriptionAttributeLabel_FontAngle"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
		RefreshPropertiesAttribute(RefreshProperties.All)
		]
		public int Angle
		{
			get
			{
				return angle;
			}
			set
			{
				if(value < -90 || value > 90)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAxisLabelFontAngleInvalid));
				}
				
				// Turn of label offset if angle is not 0, 90 or -90
				if(IsStaggered && value != 0 && value != -90 && value != 90)
				{
					IsStaggered = false;
				}

				// Turn off labels autofitting 
				if(this._axis != null && this._axis.Common!=null && this._axis.Common.Chart != null)
				{
                    if (!this._axis.Common.Chart.serializing)
					{
						this._axis.IsLabelAutoFit = false;
					}
				}

				angle = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets a property which specifies whether the labels are shown with offset.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(false),
		SRDescription("DescriptionAttributeLabel_OffsetLabels"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
		RefreshPropertiesAttribute(RefreshProperties.All)
		]
		public bool IsStaggered
		{
			get
			{
				return isStaggered;
			}
			set
			{
				// Make sure that angle is 0, 90 or -90
				if(value && (this.Angle != 0 || this.Angle != -90 || this.Angle != 90))
				{
					this.Angle = 0;
				}

				// Turn off labels autofitting 
                if (this._axis != null && this._axis.Common != null && this._axis.Common.Chart != null)
                {
                    if (!this._axis.Common.Chart.serializing)
                    {
                        this._axis.IsLabelAutoFit = false;
					}
				}

				isStaggered = value;

				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets a property which specifies whether the labels are shown at axis ends.
		/// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(true),
		SRDescription("DescriptionAttributeLabel_ShowEndLabels"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public bool IsEndLabelVisible
		{
			get
			{
				return _isEndLabelVisible;
			}
			set
			{
				_isEndLabelVisible = value;
				this.Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets a property which specifies whether the label can be truncated.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(false),
		SRDescription("DescriptionAttributeLabel_TruncatedLabels"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public bool TruncatedLabels
		{
			get
			{
				return _truncatedLabels;
			}
			set
			{
				_truncatedLabels = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets the formatting string for the label text.
		/// </summary>
		[
        SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(""),
		SRDescription("DescriptionAttributeLabel_Format"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute),
		#endif
		]
		public string Format
		{
			get
			{
				return _format;
			}
			set
			{
				_format = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets a property which indicates whether the label is enabled.
		/// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(true),
		SRDescription("DescriptionAttributeLabel_Enabled"),
		#if !Microsoft_CONTROL
		PersistenceMode(PersistenceMode.Attribute)
		#endif
		]
		public bool Enabled
		{
			get
			{
                return _enabled;
			}
			set
			{
                _enabled = value;
				this.Invalidate();
			}
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
                //Free managed resources
                if (_fontCache!=null)
                {
                    _fontCache.Dispose();
                    _fontCache = null;
                }
            }
        }

        #endregion

	}
}
