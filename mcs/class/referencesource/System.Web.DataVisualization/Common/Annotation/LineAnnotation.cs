//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		LineAnnotation.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	LineAnnotation, VerticalLineAnnotation, 
//				HorizontalLineAnnotation
//
//  Purpose:	Line annotation class.
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
	/// <b>LineAnnotation</b> is a class that represents a line annotation.
	/// </summary>
	[
		SRDescription("DescriptionAttributeLineAnnotation_LineAnnotation"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class LineAnnotation : Annotation
	{
		#region Fields

		// Indicates that an infinitive line should be drawn through 2 specified points.
		private		bool		_isInfinitive = false;

		// Line start/end caps
		private		LineAnchorCapStyle		_startCap = LineAnchorCapStyle.None;
		private		LineAnchorCapStyle		_endCap = LineAnchorCapStyle.None;

		#endregion

		#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public LineAnnotation() 
            : base()
		{
			this.anchorAlignment = ContentAlignment.TopLeft;
		}

		#endregion

		#region Properties

		#region Line Visual Attributes

		/// <summary>
		/// Gets or sets a flag that indicates if an infinitive line should be drawn.
		/// </summary>
		/// <value>
		/// <b>True</b> if a line should be drawn infinitively through 2 points provided, <b>false</b> otherwise.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(false),
		SRDescription("DescriptionAttributeDrawInfinitive"),
		]
		virtual public bool IsInfinitive
		{
			get
			{
				return _isInfinitive;
			}
			set
			{
				_isInfinitive = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets a cap style used at the start of an annotation line.
		/// <seealso cref="EndCap"/>
		/// </summary>
		/// <value>
        /// A <see cref="LineAnchorCapStyle"/> value, used for a cap style used at the start of an annotation line.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(LineAnchorCapStyle.None),
		SRDescription("DescriptionAttributeStartCap3"),
		]
		virtual public LineAnchorCapStyle StartCap
		{
			get
			{
				return _startCap;
			}
			set
			{
				_startCap = value;
				Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets a cap style used at the end of an annotation line.
		/// <seealso cref="StartCap"/>
		/// </summary>
		/// <value>
        /// A <see cref="LineAnchorCapStyle"/> value, used for a cap style used at the end of an annotation line.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(LineAnchorCapStyle.None),
		SRDescription("DescriptionAttributeStartCap3"),
		]
		virtual public LineAnchorCapStyle EndCap
		{
			get
			{
				return _endCap;
			}
			set
			{
				_endCap = value;
				Invalidate();
			}
		}

		#endregion

		#region Non Applicable Annotation Appearance Attributes (set as Non-Browsable)

		/// <summary>
		/// Not applicable to this annotation type.
		/// </summary>
		/// <value>
		/// A <see cref="ContentAlignment"/> value.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(ContentAlignment), "MiddleCenter"),
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
		/// Not applicable to this annotation type.
		/// <seealso cref="Font"/>
		/// </summary>
		/// <value>
		/// A <see cref="Color"/> value.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Color), "Black"),
        SRDescription("DescriptionAttributeForeColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
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
			}
		}

		/// <summary>
		/// Not applicable to this annotation type.
		/// <seealso cref="ForeColor"/>
		/// </summary>
		/// <value>
		/// A <see cref="Font"/> object.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Font), "Microsoft Sans Serif, 8pt"),
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
			}
		}

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
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
			}
		}

		/// <summary>
		/// Not applicable to this annotation type.
		/// </summary>
		/// <value>
		/// A <see cref="ChartHatchStyle"/> value.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(ChartHatchStyle.None),
		NotifyParentPropertyAttribute(true),
		Editor(Editors.HatchStyleEditor.Editor, Editors.HatchStyleEditor.Base)
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
			}
		}

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(GradientStyle.None),
		NotifyParentPropertyAttribute(true),
		Editor(Editors.GradientEditor.Editor, Editors.GradientEditor.Base)
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
			}
		}

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
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
			}
		}

		#endregion

		#region Position

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
		SRDescription("DescriptionAttributeSizeAlwaysRelative3"),
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

		#endregion // Position

		#region Anchor

        /// <summary>
        /// Gets or sets an annotation position's alignment to the anchor point.
        /// <seealso cref="Annotation.AnchorX"/>
        /// <seealso cref="Annotation.AnchorY"/>
        /// <seealso cref="Annotation.AnchorDataPoint"/>
        /// <seealso cref="Annotation.AnchorOffsetX"/>
        /// <seealso cref="Annotation.AnchorOffsetY"/>
        /// </summary>
        /// <value>
        /// A <see cref="ContentAlignment"/> value that represents the annotation's alignment to 
        /// the anchor point.
        /// </value>
        /// <remarks>
        /// The annotation must be anchored using either <see cref="Annotation.AnchorDataPoint"/>, or the <see cref="Annotation.AnchorX"/> 
        /// and <see cref="Annotation.AnchorY"/> properties. Its <see cref="Annotation.X"/> and <see cref="Annotation.Y"/> 
        /// properties must be set to <b>Double.NaN</b>.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAnchor"),
		Browsable(false),
		EditorBrowsableAttribute(EditorBrowsableState.Never),
		DefaultValue(typeof(ContentAlignment), "TopLeft"),
        SRDescription("DescriptionAttributeAnchorAlignment"),
		]
		override public ContentAlignment AnchorAlignment
		{
			get
			{
				return base.AnchorAlignment;
			}
			set
			{
				base.AnchorAlignment = value;
			}
		}

		#endregion	// Anchoring

		#region Other

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
				return "Line";
			}
		}

		/// <summary>
		/// Gets or sets an annotation's selection points style.
		/// </summary>
		/// <value>
		/// A <see cref="SelectionPointsStyle"/> value that represents the annotation
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
				return SelectionPointsStyle.TwoPoints;
			}
		}

		#endregion

		#endregion

		#region Methods

		/// <summary>
		/// Adjusts the two coordinates of the line.
		/// </summary>
		/// <param name="point1">First line coordinate.</param>
		/// <param name="point2">Second line coordinate.</param>
		/// <param name="selectionRect">Selection rectangle.</param>
		virtual internal void AdjustLineCoordinates(ref PointF point1, ref PointF point2, ref RectangleF selectionRect)
		{
			// Adjust line points to draw infinitive line
			if(IsInfinitive)
			{
				if(Math.Round(point1.X , 3) == Math.Round(point2.X, 3))
				{
					point1.Y = (point1.Y < point2.Y) ? 0f : 100f;
					point2.Y = (point1.Y < point2.Y) ? 100f : 0f;
				}
				else if(Math.Round(point1.Y , 3) == Math.Round(point2.Y, 3))
				{
					point1.X = (point1.X < point2.X) ? 0f : 100f;
					point2.X = (point1.X < point2.X) ? 100f : 0f;
				}
				else
				{
					// Calculate intersection point of the line with two bounaries Y = 0 and Y = 100
					PointF	intersectionPoint1 = PointF.Empty;
					intersectionPoint1.Y = 0f;
					intersectionPoint1.X = (0f - point1.Y) *
						(point2.X - point1.X) / 
						(point2.Y - point1.Y) + 
						point1.X;
					PointF	intersectionPoint2 = PointF.Empty;
					intersectionPoint2.Y = 100f;
					intersectionPoint2.X = (100f - point1.Y) *
						(point2.X - point1.X) / 
						(point2.Y - point1.Y) + 
						point1.X;

					// Select point closect to the intersection
					point1 = (point1.Y < point2.Y) ? intersectionPoint1 : intersectionPoint2;
					point2 = (point1.Y < point2.Y) ? intersectionPoint2 : intersectionPoint1;
				}
			}
		}

		/// <summary>
		/// Paints an annotation object on the specified graphics.
		/// </summary>
		/// <param name="graphics">
		/// A <see cref="ChartGraphics"/> object, used to paint an annotation object.
		/// </param>
		/// <param name="chart">
		/// Reference to the <see cref="Chart"/> owner control.
		/// </param>
        override internal void Paint(Chart chart, ChartGraphics graphics)
		{
			// Get annotation position in relative coordinates
			PointF firstPoint = PointF.Empty;
			PointF anchorPoint = PointF.Empty;
			SizeF size = SizeF.Empty;
			GetRelativePosition(out firstPoint, out size, out anchorPoint);
			PointF	secondPoint = new PointF(firstPoint.X + size.Width, firstPoint.Y + size.Height);

			// Create selection rectangle
			RectangleF selectionRect = new RectangleF(firstPoint, new SizeF(secondPoint.X - firstPoint.X, secondPoint.Y - firstPoint.Y));

			// Adjust coordinates
			AdjustLineCoordinates(ref firstPoint, ref secondPoint, ref selectionRect);

			// Check if text position is valid
			if( float.IsNaN(firstPoint.X) || 
				float.IsNaN(firstPoint.Y) || 
				float.IsNaN(secondPoint.X) || 
				float.IsNaN(secondPoint.Y) )
			{
				return;
			}

			// Set line caps
			bool capChanged = false;
			LineCap	oldStartCap = LineCap.Flat;
			LineCap	oldEndCap = LineCap.Flat;
			if(this._startCap != LineAnchorCapStyle.None || 
				this._endCap != LineAnchorCapStyle.None)
			{
				capChanged = true;
                oldStartCap = graphics.Pen.StartCap;
                oldEndCap = graphics.Pen.EndCap;

				// Apply anchor cap settings
				if(this._startCap == LineAnchorCapStyle.Arrow)
				{
					// Adjust arrow size for small line width
					if(this.LineWidth < 4)
					{
						int adjustment = 3 - this.LineWidth;
                        graphics.Pen.StartCap = LineCap.Custom;
                        graphics.Pen.CustomStartCap = new AdjustableArrowCap(
							this.LineWidth + adjustment, 
							this.LineWidth + adjustment, 
							true);
					}
					else
					{
                        graphics.Pen.StartCap = LineCap.ArrowAnchor;
					}
				}
				else if(this._startCap == LineAnchorCapStyle.Diamond)
				{
                    graphics.Pen.StartCap = LineCap.DiamondAnchor;
				}
				else if(this._startCap == LineAnchorCapStyle.Round)
				{
                    graphics.Pen.StartCap = LineCap.RoundAnchor;
				}
				else if(this._startCap == LineAnchorCapStyle.Square)
				{
                    graphics.Pen.StartCap = LineCap.SquareAnchor;
				}
				if(this._endCap == LineAnchorCapStyle.Arrow)
				{
					// Adjust arrow size for small line width
					if(this.LineWidth < 4)
					{
						int adjustment = 3 - this.LineWidth;
                        graphics.Pen.EndCap = LineCap.Custom;
                        graphics.Pen.CustomEndCap = new AdjustableArrowCap(
							this.LineWidth + adjustment, 
							this.LineWidth + adjustment, 
							true);
					}
					else
					{
                        graphics.Pen.EndCap = LineCap.ArrowAnchor;
					}
				}
				else if(this._endCap == LineAnchorCapStyle.Diamond)
				{
                    graphics.Pen.EndCap = LineCap.DiamondAnchor;
				}
				else if(this._endCap == LineAnchorCapStyle.Round)
				{
                    graphics.Pen.EndCap = LineCap.RoundAnchor;
				}
				else if(this._endCap == LineAnchorCapStyle.Square)
				{
                    graphics.Pen.EndCap = LineCap.SquareAnchor;
				}
			}

			if(this.Common.ProcessModePaint)
			{
				// Draw line
				graphics.DrawLineRel(
					this.LineColor,
					this.LineWidth,
					this.LineDashStyle,
					firstPoint,
					secondPoint,
					this.ShadowColor,
					this.ShadowOffset);
			}

			if(this.Common.ProcessModeRegions)
			{
				// Create line graphics path
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddLine(
                        graphics.GetAbsolutePoint(firstPoint),
                        graphics.GetAbsolutePoint(secondPoint));
                    using (Pen pen = (Pen)graphics.Pen.Clone())
                    {
                        // Increase pen size by 2 pixels
                        pen.DashStyle = DashStyle.Solid;
                        pen.Width += 2;
                        try
                        {
                            path.Widen(pen);
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

				// Add hot region
				this.Common.HotRegionsList.AddHotRegion(
					graphics,
					path,
					false,
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
                        ChartElementType.Annotation);
                }
			}


			// Restore line caps
			if(capChanged)
			{
                graphics.Pen.StartCap = oldStartCap;
                graphics.Pen.EndCap = oldEndCap;
			}

			// Paint selection handles
			PaintSelectionHandles(graphics, selectionRect, null);
		}

		#endregion
	}

	/// <summary>
	/// <b>VerticalLineAnnotation</b> is a class that represents a vertical line annotation.
	/// </summary>
	[
		SRDescription("DescriptionAttributeVerticalLineAnnotation_VerticalLineAnnotation"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class VerticalLineAnnotation : LineAnnotation
	{
		#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public VerticalLineAnnotation() 
            : base()
		{
		}

		#endregion

		#region Properties

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
				return "VerticalLine";
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adjusts the two coordinates of the line.
		/// </summary>
		/// <param name="point1">First line coordinate.</param>
		/// <param name="point2">Second line coordinate.</param>
		/// <param name="selectionRect">Selection rectangle.</param>
		override internal void AdjustLineCoordinates(ref PointF point1, ref PointF point2, ref RectangleF selectionRect)
		{
			// Make line vertical
			point2.X = point1.X;
			selectionRect.Width = 0f;

			// Call base class
			base.AdjustLineCoordinates(ref point1, ref point2, ref selectionRect);
		}

		#region Content Size

		/// <summary>
		/// Gets text annotation content size based on the text and font.
		/// </summary>
		/// <returns>Annotation content position.</returns>
		override internal RectangleF GetContentPosition()
		{
			return new RectangleF(float.NaN, float.NaN, 0f, float.NaN);
		}

		#endregion // Content Size

		#endregion
	}

	/// <summary>
	/// <b>HorizontalLineAnnotation</b> is a class that represents a horizontal line annotation.
	/// </summary>
	[
		SRDescription("DescriptionAttributeHorizontalLineAnnotation_HorizontalLineAnnotation"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class HorizontalLineAnnotation : LineAnnotation
	{
		#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public HorizontalLineAnnotation() 
            : base()
		{
		}

		#endregion

		#region Properties

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
				return "HorizontalLine";
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adjusts the two coordinates of the line.
		/// </summary>
		/// <param name="point1">First line coordinate.</param>
		/// <param name="point2">Second line coordinate.</param>
		/// <param name="selectionRect">Selection rectangle.</param>
		override internal void AdjustLineCoordinates(ref PointF point1, ref PointF point2, ref RectangleF selectionRect)
		{
			// Make line horizontal
			point2.Y = point1.Y;
			selectionRect.Height = 0f;

			// Call base class
			base.AdjustLineCoordinates(ref point1, ref point2, ref selectionRect);
		}

		#region Content Size

		/// <summary>
		/// Gets text annotation content size based on the text and font.
		/// </summary>
		/// <returns>Annotation content position.</returns>
		override internal RectangleF GetContentPosition()
		{
			return new RectangleF(float.NaN, float.NaN, float.NaN, 0f);
		}

		#endregion // Content Size

		#endregion
	}
}
