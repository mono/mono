//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ArrowAnnotation.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ArrowAnnotation
//
//  Purpose:	Arrow annotation classes.
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
using System.Web.UI.DataVisualization.Charting.Borders3D;
#endif


#endregion

#if Microsoft_CONTROL
namespace System.Windows.Forms.DataVisualization.Charting

#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	#region Enumeration

	/// <summary>
	/// Arrow annotation styles.
	/// <seealso cref="ArrowAnnotation.ArrowStyle"/>
	/// </summary>
	[
	SRDescription("DescriptionAttributeArrowStyle_ArrowStyle")
	]
	public enum ArrowStyle
	{
		/// <summary>
		/// Simple arrow.
		/// </summary>
		Simple,

		/// <summary>
		/// Arrow pointing in two directions.
		/// </summary>
		DoubleArrow,

		/// <summary>
		/// Arrow with a tail.
		/// </summary>
		Tailed,
	}

	#endregion // Enumeration

    /// <summary>
    /// <b>ArrowAnnotation</b> is a class class that represents an arrow annotation.
    /// </summary>
    /// <remarks>
    /// Arrow annotations can be used to connect to points on the chart or highlight a
    /// single chart area. Different arrow styles and sizes may be applied.
    /// </remarks>
	[
		SRDescription("DescriptionAttributeArrowAnnotation_ArrowAnnotation"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
	public class ArrowAnnotation : Annotation
	{
		#region Fields

		// Annotation arrow style
		private		ArrowStyle		_arrowStyle = ArrowStyle.Simple;

		// Annotation arrow size
		private		int				_arrowSize = 5;

		#endregion

		#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public ArrowAnnotation() 
            : base()
		{
            base.AnchorAlignment = ContentAlignment.TopLeft;
		}

		#endregion

		#region Properties

		#region Arrow properties

		/// <summary>
		/// Gets or sets the arrow style of an arrow annotation.
		/// <seealso cref="ArrowSize"/>
		/// </summary>
		/// <value>
		/// <see cref="ArrowStyle"/> of an annotation.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(ArrowStyle.Simple),
		SRDescription("DescriptionAttributeArrowAnnotation_ArrowStyle"),
		ParenthesizePropertyNameAttribute(true),
		]
		virtual public ArrowStyle ArrowStyle
		{
			get
			{
				return _arrowStyle;
			}
			set
			{
				_arrowStyle = value;
				Invalidate();
			}
		}

		/// <summary>
        /// Gets or sets the arrow size in pixels of an arrow annotation.
		/// <seealso cref="ArrowStyle"/>
		/// </summary>
		/// <value>
		/// Integer value that represents arrow size (thickness) in pixels.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(5),
		SRDescription("DescriptionAttributeArrowAnnotation_ArrowSize"),
		ParenthesizePropertyNameAttribute(true),
		]
		virtual public int ArrowSize
		{
			get
			{
				return _arrowSize;
			}
			set
			{
				if(value <= 0)
				{
					throw(new ArgumentOutOfRangeException("value", SR.ExceptionAnnotationArrowSizeIsZero));
				}
				if(value > 100)
				{
                    throw (new ArgumentOutOfRangeException("value", SR.ExceptionAnnotationArrowSizeMustBeLessThen100));
				}
				_arrowSize = value;
				Invalidate();
			}
		}


		#endregion // Arrow properties

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
				return "Arrow";
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
				return SelectionPointsStyle.TwoPoints;
			}
		}

		#endregion

		#endregion

		#region Methods

		/// <summary>
		/// Paints annotation object on specified graphics.
		/// </summary>
		/// <param name="graphics">
		/// A <see cref="ChartGraphics"/> used to paint annotation object.
		/// </param>
		/// <param name="chart">
		/// Reference to the <see cref="Chart"/> control.
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

			// Check if text position is valid
			if( float.IsNaN(firstPoint.X) || 
				float.IsNaN(firstPoint.Y) || 
				float.IsNaN(secondPoint.X) || 
				float.IsNaN(secondPoint.Y) )
			{
				return;
			}

			// Get arrow shape path
            using (GraphicsPath arrowPathAbs = GetArrowPath(graphics, selectionRect))
            {

                // Draw arrow shape
                if (this.Common.ProcessModePaint)
                {
                    graphics.DrawPathAbs(
                        arrowPathAbs,
                        (this.BackColor.IsEmpty) ? Color.White : this.BackColor,
                        this.BackHatchStyle,
                        String.Empty,
                        ChartImageWrapMode.Scaled,
                        Color.Empty,
                        ChartImageAlignmentStyle.Center,
                        this.BackGradientStyle,
                        this.BackSecondaryColor,
                        this.LineColor,
                        this.LineWidth,
                        this.LineDashStyle,
                        PenAlignment.Center,
                        this.ShadowOffset,
                        this.ShadowColor);
                }

                // Process hot region
                if (this.Common.ProcessModeRegions)
                {
                    // Use callout defined hot region
                    this.Common.HotRegionsList.AddHotRegion(
                        graphics,
                        arrowPathAbs,
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

                // Paint selection handles
                PaintSelectionHandles(graphics, selectionRect, null);
            }
		}

		/// <summary>
		/// Get arrow path for the specified annotation position
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="position"></param>
		/// <returns></returns>
		private GraphicsPath GetArrowPath(
			ChartGraphics graphics,
			RectangleF position)
		{
			// Get absolute position
			RectangleF positionAbs = graphics.GetAbsoluteRectangle(position);
			PointF firstPoint = positionAbs.Location;
			PointF secondPoint = new PointF(positionAbs.Right, positionAbs.Bottom);

			// Calculate arrow length
			float deltaX = secondPoint.X - firstPoint.X;
			float deltaY = secondPoint.Y - firstPoint.Y;
			float arrowLength = (float)Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

			// Create unrotated graphics path for the arrow started at the annotation location
			// and going to the right for the length of the rotated arrow.
			GraphicsPath path = new GraphicsPath();

			PointF[]	points = null;
			float		pointerRatio = 2.1f;
			if(this.ArrowStyle == ArrowStyle.Simple)
			{
				points = new PointF[] {
										  firstPoint,
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y - this.ArrowSize*pointerRatio),
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y - this.ArrowSize),
										  new PointF(firstPoint.X + arrowLength, firstPoint.Y - this.ArrowSize),
										  new PointF(firstPoint.X + arrowLength, firstPoint.Y + this.ArrowSize),
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y + this.ArrowSize),
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y + this.ArrowSize*pointerRatio) };
			}
			else if(this.ArrowStyle == ArrowStyle.DoubleArrow)
			{
				points = new PointF[] {
										  firstPoint,
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y - this.ArrowSize*pointerRatio),
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y - this.ArrowSize),
										  new PointF(firstPoint.X + arrowLength - this.ArrowSize*pointerRatio, firstPoint.Y - this.ArrowSize),
										  new PointF(firstPoint.X + arrowLength - this.ArrowSize*pointerRatio, firstPoint.Y - this.ArrowSize*pointerRatio),
										  new PointF(firstPoint.X + arrowLength, firstPoint.Y),
										  new PointF(firstPoint.X + arrowLength - this.ArrowSize*pointerRatio, firstPoint.Y + this.ArrowSize*pointerRatio),
										  new PointF(firstPoint.X + arrowLength - this.ArrowSize*pointerRatio, firstPoint.Y + this.ArrowSize),
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y + this.ArrowSize),
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y + this.ArrowSize*pointerRatio) };
			}
			else if(this.ArrowStyle == ArrowStyle.Tailed)
			{
				float		tailRatio = 2.1f;
				points = new PointF[] {
										  firstPoint,
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y - this.ArrowSize*pointerRatio),
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y - this.ArrowSize),
										  new PointF(firstPoint.X + arrowLength, firstPoint.Y - this.ArrowSize*tailRatio),
										  new PointF(firstPoint.X + arrowLength - this.ArrowSize*tailRatio, firstPoint.Y),
										  new PointF(firstPoint.X + arrowLength, firstPoint.Y + this.ArrowSize*tailRatio),
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y + this.ArrowSize),
										  new PointF(firstPoint.X + this.ArrowSize*pointerRatio, firstPoint.Y + this.ArrowSize*pointerRatio) };
			}
			else
			{
                throw (new InvalidOperationException(SR.ExceptionAnnotationArrowStyleUnknown));
			}

			path.AddLines(points);
			path.CloseAllFigures();

			// Calculate arrow angle
			float angle = (float)(Math.Atan(deltaY / deltaX) * 180f / Math.PI);
			if(deltaX < 0)
			{
				angle += 180f;
			}

			// Rotate arrow path around the first point
			using( Matrix matrix = new Matrix() )
			{
				matrix.RotateAt(angle, firstPoint);
				path.Transform(matrix);
			}

			return path;
		}

		#endregion
	}
}
