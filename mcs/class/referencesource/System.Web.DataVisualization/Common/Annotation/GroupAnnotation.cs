//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		GroupAnnotation.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	AnnotationGroup
//
//  Purpose:	Annotation group class.
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
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
#if Microsoft_CONTROL
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
	/// <summary>
	/// <b>AnnotationGroup</b> is a class that represents an annotation group.
	/// </summary>
	/// <remarks>
	/// This class is a collection of annotations, and can be used 
	/// to manipulate annotations relative to each other.
	/// </remarks>
	[
		SRDescription("DescriptionAttributeAnnotationGroup_AnnotationGroup"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class AnnotationGroup : Annotation
	{
		#region Fields

		// Collection of annotations in the group
		internal	AnnotationCollection	annotations = null;

		#endregion

		#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public AnnotationGroup() 
            : base()
		{
            annotations = new AnnotationCollection(this);
			annotations.AnnotationGroup = this;
		}

		#endregion

        #region Miscellaneous Properties

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
		SRDescription("DescriptionAttributeAnnotationGroup_ClipToChartArea"),
		TypeConverter(typeof(LegendAreaNameConverter)),
        Browsable(false),
		]
		override public string ClipToChartArea
		{
			get
			{
				return base.ClipToChartArea;
			}
			set
			{
				base.ClipToChartArea = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.ClipToChartArea = value;
				}
			}
		}

		#endregion

        #region Position Properties

        /// <summary>
        /// Gets or sets a flag that specifies whether the size of an annotation is always 
        /// defined in relative chart coordinates.
        /// <seealso cref="Annotation.Width"/>
        /// <seealso cref="Annotation.Height"/>
        /// </summary>
        /// <value>
        /// <b>True</b> if an annotation's <see cref="Annotation.Width"/> and <see cref="Annotation.Height"/> are always 
        /// in chart relative coordinates, <b>false</b> otherwise.
        /// </value>
        /// <remarks>
        /// An annotation's width and height may be set in relative chart or axes coordinates. 
        /// By default, relative chart coordinates are used.
        /// <para>
        /// To use axes coordinates for size set the <b>IsSizeAlwaysRelative</b> property to 
        /// <b>false</b> and either anchor the annotation to a data point or set the 
        /// <see cref="Annotation.AxisX"/> or <see cref="Annotation.AxisY"/> properties.
        /// </para>
        /// </remarks>
		[
		SRCategory("CategoryAttributePosition"),
		DefaultValue(true),
		SRDescription("DescriptionAttributeAnnotationGroup_SizeAlwaysRelative"),
		]
		override public bool IsSizeAlwaysRelative
		{
			get
			{
				return base.IsSizeAlwaysRelative;
			}
			set
			{
				base.IsSizeAlwaysRelative = value;
			}
		}

		#endregion

        #region Visual Properties

#if Microsoft_CONTROL
        /// <summary>
        /// Gets or sets a flag that determines if an annotation is selected.
        /// <seealso cref="Annotation.AllowSelecting"/>
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
		SRDescription("DescriptionAttributeAnnotationGroup_Selected"),
		]
		override public bool IsSelected
		{
			get
			{
				return base.IsSelected;
			}
			set
			{
				base.IsSelected = value;

				// Clear selection for all annotations in the group
				foreach(Annotation annotation in this.annotations)
				{
					annotation.IsSelected = false;
				}
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
		SRDescription("DescriptionAttributeAnnotationGroup_Visible"),
		ParenthesizePropertyNameAttribute(true),
		]
		override public bool Visible
		{
			get
			{
				return base.Visible;
			}
			set
			{
				base.Visible = value;
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
        Browsable(false),
		]
		override public ContentAlignment Alignment
		{
			get
			{
				return base.Alignment;
			}
			set
			{
				base.Alignment = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.Alignment = value;
				}
			}
		}

        /// <summary>
        /// Gets or sets an annotation's text style.
        /// <seealso cref="Font"/>
        /// 	<seealso cref="ForeColor"/>
        /// </summary>
        /// <value>
        /// A <see cref="TextStyle"/> value used to draw an annotation's text.
        /// </value>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override TextStyle TextStyle
        {
            get
            {
                return base.TextStyle;
            }
            set
            {
                base.TextStyle = value;
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
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
        Browsable(false),
		]
		override public Color ForeColor
		{
			get
			{
				return base.ForeColor;
			}
			set
			{
				base.ForeColor = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.ForeColor = value;
				}
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
        Browsable(false),
		]
		override public Font Font
		{
			get
			{
				return base.Font;
			}
			set
			{
				base.Font = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.Font = value;
				}
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
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
        Browsable(false),
		]
		override public Color LineColor
		{
			get
			{
				return base.LineColor;
			}
			set
			{
				base.LineColor = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.LineColor = value;
				}
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
        Browsable(false),
		]
		override public int LineWidth
		{
			get
			{
				return base.LineWidth;
			}
			set
			{
				base.LineWidth = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.LineWidth = value;
				}
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
        Browsable(false),
		]
		override public ChartDashStyle LineDashStyle
		{
			get
			{
				return base.LineDashStyle;
			}
			set
			{
				base.LineDashStyle = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.LineDashStyle = value;
				}
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
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
        Browsable(false),
		]
		override public Color BackColor
		{
			get
			{
				return base.BackColor;
			}
			set
			{
				base.BackColor = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.BackColor = value;
				}
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
		Editor(Editors.HatchStyleEditor.Editor, Editors.HatchStyleEditor.Base),
        Browsable(false),
		]
		override public ChartHatchStyle BackHatchStyle
		{
			get
			{
				return base.BackHatchStyle;
			}
			set
			{
				base.BackHatchStyle = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.BackHatchStyle = value;
				}
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
		Editor(Editors.GradientEditor.Editor, Editors.GradientEditor.Base),
        Browsable(false),
		]		
		override public GradientStyle BackGradientStyle
		{
			get
			{
				return base.BackGradientStyle;
			}
			set
			{
				base.BackGradientStyle = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.BackGradientStyle = value;
				}
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
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
        Browsable(false),
		] 
		override public Color BackSecondaryColor
		{
			get
			{
				return base.BackSecondaryColor;
			}
			set
			{
				base.BackSecondaryColor = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.BackSecondaryColor = value;
				}
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
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base),
        Browsable(false),
		]
		override public Color ShadowColor
		{
			get
			{
				return base.ShadowColor;
			}
			set
			{
				base.ShadowColor = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.ShadowColor = value;
				}
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
        Browsable(false),
		]
		override public int ShadowOffset
		{
			get
			{
				return base.ShadowOffset;
			}
			set
			{
				base.ShadowOffset = value;
				foreach(Annotation annotation in this.annotations)
				{
					annotation.ShadowOffset = value;
				}
			}
		}

		#endregion

        #region Editing Permissions Properties

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
		override public bool AllowSelecting
		{
			get
			{
				return base.AllowSelecting;
			}
			set
			{
				base.AllowSelecting = value;
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
		override public bool AllowMoving
		{
			get
			{
				return base.AllowMoving;
			}
			set
			{
				base.AllowMoving = value;
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
		SRDescription("DescriptionAttributeAnnotationGroup_AllowAnchorMoving"),
		]
		override public bool AllowAnchorMoving
		{
			get
			{
				return base.AllowAnchorMoving;
			}
			set
			{
				base.AllowAnchorMoving = value;
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
		override public bool AllowResizing
		{
			get
			{
				return base.AllowResizing;
			}
			set
			{
				base.AllowResizing = value;
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
		override public bool AllowTextEditing
		{
			get
			{
				return base.AllowTextEditing;
			}
			set
			{
				base.AllowTextEditing = value;
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
		SRDescription("DescriptionAttributeAnnotationGroup_AllowPathEditing"),
		]
		override public bool AllowPathEditing
		{
			get
			{
				return base.AllowPathEditing;
			}
			set
			{
				base.AllowPathEditing = value;
			}
		}

#endif // Microsoft_CONTROL

        #endregion

        #region Other Properties

        /// <summary>
		/// Gets the collection of annotations in the group.
		/// </summary>
		/// <value>
		/// An <see cref="AnnotationCollection"/> object.
		/// </value>
		/// <remarks>
		/// Note that the coordinates of all annotations in the group are relative to the 
		/// group annotation.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnnotations"),
		SRDescription("DescriptionAttributeAnnotationGroup_Annotations"),
		Editor(Editors.AnnotationCollectionEditor.Editor, Editors.AnnotationCollectionEditor.Base),
#if Microsoft_CONTROL
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content), 
#else	// Microsoft_CONTROL
		PersistenceMode(PersistenceMode.InnerProperty),
#endif	// Microsoft_CONTROL
		]
		public AnnotationCollection Annotations
		{
			get
			{
				return annotations;
			}
		}

        /// <summary>
        /// Gets or sets an annotation's type name.
        /// </summary>
        /// <remarks>
        /// This property is used to get the name of each annotation type 
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
		SRDescription("DescriptionAttributeAnnotationType"),
		]
		public override string AnnotationType
		{
			get
			{
				return "Group";
			}
		}

		/// <summary>
		/// Gets or sets annotation selection points style.
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
		override internal SelectionPointsStyle SelectionPointsStyle
		{
			get
			{
				return SelectionPointsStyle.Rectangle;
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Paints an annotation object using the specified graphics.
		/// </summary>
		/// <param name="graphics">
		/// A <see cref="ChartGraphics"/> object, used to paint the annotation object.
		/// </param>
		/// <param name="chart">
		/// Reference to the <see cref="Chart"/> control.
		/// </param>
        override internal void Paint(Chart chart, ChartGraphics graphics)
		{
			// Paint all annotations in the group
			foreach(Annotation annotation in this.annotations)
			{
				annotation.Paint(chart, graphics);
			}

			if( (this.Common.ProcessModePaint && this.IsSelected) ||
				this.Common.ProcessModeRegions )
			{
				// Get annotation position in relative coordinates
				PointF firstPoint = PointF.Empty;
				PointF anchorPoint = PointF.Empty;
				SizeF size = SizeF.Empty;
				GetRelativePosition(out firstPoint, out size, out anchorPoint);
				PointF	secondPoint = new PointF(firstPoint.X + size.Width, firstPoint.Y + size.Height);

				// Create selection rectangle
				RectangleF selectionRect = new RectangleF(firstPoint, new SizeF(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y));

				// Check rectangle orientation 
				if(selectionRect.Width < 0)
				{
					selectionRect.X = selectionRect.Right;
					selectionRect.Width = -selectionRect.Width;
				}
				if(selectionRect.Height < 0)
				{
					selectionRect.Y = selectionRect.Bottom;
					selectionRect.Height = -selectionRect.Height;
				}

				// Check if text position is valid
				if( selectionRect.IsEmpty ||
					float.IsNaN(selectionRect.X) || 
					float.IsNaN(selectionRect.Y) || 
					float.IsNaN(selectionRect.Right) || 
					float.IsNaN(selectionRect.Bottom) )
				{
					return;
				}

				if(this.Common.ProcessModeRegions)
				{
					// Add hot region
					this.Common.HotRegionsList.AddHotRegion(
						selectionRect,
						ReplaceKeywords(this.ToolTip),
#if Microsoft_CONTROL
						String.Empty,
						String.Empty,
						String.Empty,
#else // Microsoft_CONTROL
                        ReplaceKeywords(this.Url),
						ReplaceKeywords(this.MapAreaAttributes),
                        ReplaceKeywords(this.PostBackValue),
#endif // Microsoft_CONTROL
						this,
						ChartElementType.Annotation,
						String.Empty);
				}

				// Paint selection handles
				PaintSelectionHandles(graphics, selectionRect, null);
			}
		}

		#endregion	// Methods

        #region IDisposable override
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Clean up managed resources
                if (this.annotations != null)
                {
                    this.annotations.Dispose();
                    this.annotations = null;
                }
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
