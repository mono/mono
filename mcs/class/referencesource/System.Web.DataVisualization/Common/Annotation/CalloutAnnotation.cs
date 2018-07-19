//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		CalloutAnnotation.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	CalloutAnnotation
//
//  Purpose:	Callout annotation classes.
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
	#region Enumerations

	/// <summary>
	/// Annotation callout style.
	/// <seealso cref="CalloutAnnotation.CalloutStyle"/>
	/// </summary>
	[
	SRDescription("DescriptionAttributeCalloutStyle_CalloutStyle"),
	]
	public enum CalloutStyle
	{
		/// <summary>
		/// Callout text is underlined and a line is pointing to the anchor point.
		/// </summary>
		SimpleLine,

		/// <summary>
		/// Border is drawn around text and a line is pointing to the anchor point.
		/// </summary>
		Borderline,

		/// <summary>
		/// Callout text is inside the cloud and smaller clouds are pointing to the anchor point.
		/// </summary>
		Cloud,

		/// <summary>
		/// Rectangle is drawn around the callout text, which is connected with the anchor point.
		/// </summary>
		Rectangle,

		/// <summary>
		/// Rounded rectangle is drawn around the callout text, which is connected with the anchor point.
		/// </summary>
		RoundedRectangle,

		/// <summary>
		/// Ellipse is drawn around the callout text, which is connected with the anchor point.
		/// </summary>
		Ellipse,

		/// <summary>
		/// Perspective rectangle is drawn around the callout text, which is connected with the anchor point.
		/// </summary>
		Perspective,
	}

	#endregion

	/// <summary>
	/// <b>CalloutAnnotation</b> is a class class that represents a callout annotation.
	/// </summary>
	/// <remarks>
	/// Callout annotation is the only annotation that draws a connection between the
	/// annotation position and anchor point. It can display text and automatically 
	/// calculate the required size. Different <see cref="CalloutStyle"/> are supported.
	/// </remarks>
	[
		SRDescription("DescriptionAttributeCalloutAnnotation_CalloutAnnotation"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
	public class CalloutAnnotation : TextAnnotation
	{
		#region Fields

		// Callout anchor type 
		private		LineAnchorCapStyle		_calloutAnchorCap = LineAnchorCapStyle.Arrow;

		// Callout drawing style
		private		CalloutStyle		_calloutStyle = CalloutStyle.Rectangle;

		// Cloud shape path
		private		static				GraphicsPath	_cloudPath = null;

		// Cloud shape outline path
		private		static				GraphicsPath	_cloudOutlinePath = null;

		// Cloud shape boundary rectangle
		private		static				RectangleF	_cloudBounds = RectangleF.Empty;

		#endregion

		#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public CalloutAnnotation() 
            : base()
		{
			// Changing default values of properties
			this.anchorOffsetX = 3.0;
			this.anchorOffsetY = 3.0;
			this.anchorAlignment = ContentAlignment.BottomLeft;
		}

		#endregion

		#region Properties

		#region	Callout properties

        /// <summary>
        /// Gets or sets the annotation callout style.
        /// </summary>
        /// <value>
        /// <see cref="CalloutStyle"/> of the annotation.
        /// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(CalloutStyle.Rectangle),
		SRDescription("DescriptionAttributeCalloutAnnotation_CalloutStyle"),
		ParenthesizePropertyNameAttribute(true),
		]
		virtual public CalloutStyle CalloutStyle
		{
			get
			{
				return _calloutStyle;
			}
			set
			{
				_calloutStyle = value;
				this.ResetCurrentRelativePosition();
				
				// Reset content size to empty
				contentSize = SizeF.Empty;

				Invalidate();
			}
		}

        /// <summary>
        /// Gets or sets the anchor cap style of a callout line.
        /// </summary>
        /// <value>
        /// A <see cref="LineAnchorCapStyle"/> value used as the anchor cap of a callout line.
        /// </value>
        /// <remarks>
        /// This property sets the anchor cap of the line connecting an annotation to 
        /// its anchor point. It only applies when SimpleLine or BorderLine 
        /// are used.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAppearance"),
		Bindable(true),
		DefaultValue(LineAnchorCapStyle.Arrow),
		SRDescription("DescriptionAttributeCalloutAnnotation_CalloutAnchorCap"),
		]
		virtual public LineAnchorCapStyle CalloutAnchorCap
		{
			get
			{
				return _calloutAnchorCap;
			}
			set
			{
				_calloutAnchorCap = value;
				Invalidate();
			}
		}
		#endregion // Callout properties

		#region Applicable Annotation Appearance Attributes (set as Browsable)

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
		Browsable(true),
		DefaultValue(typeof(Color), "Black"),
        SRDescription("DescriptionAttributeLineColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
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
		Browsable(true),
		DefaultValue(1),
        SRDescription("DescriptionAttributeLineWidth"),
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
		Browsable(true),
		DefaultValue(ChartDashStyle.Solid),
        SRDescription("DescriptionAttributeLineDashStyle"),
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
		Browsable(true),
		DefaultValue(typeof(Color), ""),
        SRDescription("DescriptionAttributeBackColor"),
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
		Browsable(true),
		DefaultValue(ChartHatchStyle.None),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeBackHatchStyle"),
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
		Browsable(true),
		DefaultValue(GradientStyle.None),
		NotifyParentPropertyAttribute(true),
        	SRDescription("DescriptionAttributeBackGradientStyle"),
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
		Browsable(true),
		DefaultValue(typeof(Color), ""),
		NotifyParentPropertyAttribute(true),
        SRDescription("DescriptionAttributeBackSecondaryColor"),
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

		#region Anchor

        /// <summary>
        /// Gets or sets the x-coordinate offset between the positions of an annotation and its anchor point.
        /// <seealso cref="AnchorOffsetY"/>
        /// <seealso cref="Annotation.AnchorDataPoint"/>
        /// <seealso cref="Annotation.AnchorX"/>
        /// <seealso cref="AnchorAlignment"/>
        /// </summary>
        /// <value>
        /// A double value that represents the x-coordinate offset between the positions of an annotation and its anchor point.
        /// </value>
        /// <remarks>
        /// The annotation must be anchored using the <see cref="Annotation.AnchorDataPoint"/> or 
        /// <see cref="Annotation.AnchorX"/> properties, and its <see cref="Annotation.X"/> property must be set 
        /// to <b>Double.NaN</b>.
        /// </remarks>
		[
		SRCategory("CategoryAttributeAnchor"),
		DefaultValue(3.0),
		SRDescription("DescriptionAttributeCalloutAnnotation_AnchorOffsetX"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		]
		override public double AnchorOffsetX
		{
			get
			{
				return base.AnchorOffsetX;
			}
			set
			{
				base.AnchorOffsetX = value;
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
		/// <see cref="Annotation.AnchorY"/> properties and its <see cref="Annotation.Y"/> property must be set
		/// to <b>Double.NaN</b>.
		/// </remarks>
		[
		SRCategory("CategoryAttributeAnchor"),
		DefaultValue(3.0),
		SRDescription("DescriptionAttributeCalloutAnnotation_AnchorOffsetY"),
		RefreshPropertiesAttribute(RefreshProperties.All),
		]
		override public double AnchorOffsetY
		{
			get
			{
				return base.AnchorOffsetY;
			}
			set
			{
				base.AnchorOffsetY = value;
			}
		}

        /// <summary>
        /// Gets or sets an annotation position's alignment to the anchor point.
        /// <seealso cref="Annotation.AnchorX"/>
        /// <seealso cref="Annotation.AnchorY"/>
        /// <seealso cref="Annotation.AnchorDataPoint"/>
        /// <seealso cref="AnchorOffsetX"/>
        /// <seealso cref="AnchorOffsetY"/>
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
		DefaultValue(typeof(ContentAlignment), "BottomLeft"),
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
				return "Callout";
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

		#endregion

		#region Methods

		#region Text Spacing

		/// <summary>
		/// Gets text spacing on four different sides in relative coordinates.
		/// </summary>
		/// <param name="annotationRelative">Indicates that spacing is in annotation relative coordinates.</param>
		/// <returns>Rectangle with text spacing values.</returns>
		internal override RectangleF GetTextSpacing(out bool annotationRelative)
		{
			RectangleF spacing = base.GetTextSpacing(out annotationRelative);
			if(this._calloutStyle == CalloutStyle.Cloud ||
				this._calloutStyle == CalloutStyle.Ellipse)
			{
				spacing = new RectangleF(4f, 4f, 4f, 4f);
				annotationRelative = true;
			}
			else if(this._calloutStyle == CalloutStyle.RoundedRectangle)
			{
				spacing = new RectangleF(1f, 1f, 1f, 1f);
				annotationRelative = true;
			}

			return spacing;
		}

		#endregion // Text Spacing

		#region Painting

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

			// Adjust negative rectangle width and height
			RectangleF	rectanglePosition = new RectangleF(selectionRect.Location, selectionRect.Size);
			if(rectanglePosition.Width < 0)
			{
				rectanglePosition.X = rectanglePosition.Right;
				rectanglePosition.Width = -rectanglePosition.Width;
			}
			if(rectanglePosition.Height < 0)
			{
				rectanglePosition.Y = rectanglePosition.Bottom;
				rectanglePosition.Height = -rectanglePosition.Height;
			}

			// Check if position is valid
			if( float.IsNaN(rectanglePosition.X) || 
				float.IsNaN(rectanglePosition.Y) || 
				float.IsNaN(rectanglePosition.Right) || 
				float.IsNaN(rectanglePosition.Bottom) )
			{
				return;
			}

			// Paint different style of callouts
			GraphicsPath hotRegionPathAbs = null;
			if(this.Common.ProcessModePaint)
			{
				switch(this._calloutStyle)
				{
					case(CalloutStyle.SimpleLine):
						hotRegionPathAbs = DrawRectangleLineCallout(
							graphics,
							rectanglePosition,
							anchorPoint,
							false);
						break;
					case(CalloutStyle.Borderline):
						hotRegionPathAbs = DrawRectangleLineCallout(
							graphics,
							rectanglePosition,
							anchorPoint,
							true);
						break;
					case(CalloutStyle.Perspective):
						hotRegionPathAbs = DrawPerspectiveCallout(
							graphics,
							rectanglePosition,
							anchorPoint);
						break;
					case(CalloutStyle.Cloud):
						hotRegionPathAbs = DrawCloudCallout(
							graphics,
							rectanglePosition,
							anchorPoint);
						break;
					case(CalloutStyle.Rectangle):
						hotRegionPathAbs = DrawRectangleCallout(
							graphics,
							rectanglePosition,
							anchorPoint);
						break;
					case(CalloutStyle.Ellipse):
						hotRegionPathAbs = DrawRoundedRectCallout(
							graphics,
							rectanglePosition,
							anchorPoint,
							true);
						break;
					case(CalloutStyle.RoundedRectangle):
						hotRegionPathAbs = DrawRoundedRectCallout(
							graphics,
							rectanglePosition,
							anchorPoint,
							false);
						break;
				}
			}

			if(this.Common.ProcessModeRegions)
			{
				if(hotRegionPathAbs != null)
				{
					// If there is more then one graphical path split them and create 
					// image maps for every graphical path separately.
					GraphicsPathIterator iterator = new GraphicsPathIterator(hotRegionPathAbs);

					// There is more then one path.
                    using (GraphicsPath subPath = new GraphicsPath())
                    {
                        while (iterator.NextMarker(subPath) > 0)
                        {
                            // Use callout defined hot region
                            this.Common.HotRegionsList.AddHotRegion(
                                graphics,
                                subPath,
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

                            // Reset current path
                            subPath.Reset();
                        }
                    }
				}
				else
				{
					// Use rectangular hot region
					this.Common.HotRegionsList.AddHotRegion(
						rectanglePosition,
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
			}

            //Clean up
            if (hotRegionPathAbs != null)
                hotRegionPathAbs.Dispose();

			// Paint selection handles
			PaintSelectionHandles(graphics, selectionRect, null);
		}

		/// <summary>
		/// Draws Rounded rectangle or Ellipse style callout.
		/// </summary>
		/// <param name="graphics">Chart graphics.</param>
		/// <param name="rectanglePosition">Position of annotation objet.</param>
		/// <param name="anchorPoint">Anchor location.</param>
		/// <param name="isEllipse">True if ellipse shape should be used.</param>
		/// <returns>Hot region of the callout.</returns>
		private GraphicsPath DrawRoundedRectCallout(
			ChartGraphics graphics,
			RectangleF rectanglePosition,
			PointF anchorPoint,
			bool isEllipse)
		{
			// Get absolute position
			RectangleF rectanglePositionAbs = graphics.GetAbsoluteRectangle(rectanglePosition);

            // NOTE: Fix for issue #6692.
            // Do not draw the callout if size is not set. This may happen if callou text is set to empty string.
            if (rectanglePositionAbs.Width <= 0 || rectanglePositionAbs.Height <= 0)
            {
                return null;
            }

			// Create ellipse path
			GraphicsPath ellipsePath = new GraphicsPath();
			if(isEllipse)
			{
				// Add ellipse shape
				ellipsePath.AddEllipse(rectanglePositionAbs);
			}
			else
			{
				// Add rounded rectangle shape
				float radius = Math.Min(rectanglePositionAbs.Width, rectanglePositionAbs.Height);
				radius /= 5f;
				ellipsePath = this.CreateRoundedRectPath(rectanglePositionAbs, radius);
			}

			// Draw perspective polygons from anchoring point
			if(!float.IsNaN(anchorPoint.X) && !float.IsNaN(anchorPoint.Y))
			{
				// Check if point is inside annotation position
				if(!rectanglePosition.Contains(anchorPoint.X, anchorPoint.Y))
				{
					// Get absolute anchor point
					PointF anchorPointAbs = graphics.GetAbsolutePoint(new PointF(anchorPoint.X, anchorPoint.Y));

					// Flatten ellipse path
					ellipsePath.Flatten();

					// Find point in the path closest to the anchor point
					PointF[] points = ellipsePath.PathPoints;
					int closestPointIndex = 0;
					int index = 0;
					float currentDistance = float.MaxValue;
					foreach(PointF point in points)
					{
						float deltaX = point.X - anchorPointAbs.X;
						float deltaY = point.Y - anchorPointAbs.Y;
						float distance = deltaX * deltaX + deltaY * deltaY;
						if(distance < currentDistance)
						{
							currentDistance = distance;
							closestPointIndex = index;
						}
						++ index;
					}

					// Change point to the anchor location
					points[closestPointIndex] = anchorPointAbs;

					// Recreate ellipse path
					ellipsePath.Reset();
					ellipsePath.AddLines(points);
					ellipsePath.CloseAllFigures();
				}
			}

			// Draw ellipse
			graphics.DrawPathAbs(
				ellipsePath,
				this.BackColor,
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

			// Draw text 
			DrawText(graphics, rectanglePosition, true, false);

			return ellipsePath;
		}

		/// <summary>
		/// Draws Rectangle style callout.
		/// </summary>
		/// <param name="graphics">Chart graphics.</param>
		/// <param name="rectanglePosition">Position of annotation objet.</param>
		/// <param name="anchorPoint">Anchor location.</param>
		/// <returns>Hot region of the callout.</returns>
		private GraphicsPath DrawRectangleCallout(
			ChartGraphics graphics,
			RectangleF rectanglePosition,
			PointF anchorPoint)
		{
			// Create path for the rectangle connected with anchor point.
			GraphicsPath	hotRegion = null;
			bool anchorVisible = false;
			if(!float.IsNaN(anchorPoint.X) && !float.IsNaN(anchorPoint.Y))
			{
				// Get relative size of a pixel
				SizeF pixelSize = graphics.GetRelativeSize(new SizeF(1f, 1f));

				// Increase annotation position rectangle by 1 pixel
				RectangleF inflatedPosition = new RectangleF(rectanglePosition.Location, rectanglePosition.Size);
				inflatedPosition.Inflate(pixelSize);

				// Check if point is inside annotation position
				if(!inflatedPosition.Contains(anchorPoint.X, anchorPoint.Y))
				{
					anchorVisible = true;

					// Get absolute position
					RectangleF rectanglePositionAbs = graphics.GetAbsoluteRectangle(rectanglePosition);

					// Get absolute anchor point
					PointF anchorPointAbs = graphics.GetAbsolutePoint(new PointF(anchorPoint.X, anchorPoint.Y));

					// Calculate anchor pointer thicness
					float size = Math.Min(rectanglePositionAbs.Width, rectanglePositionAbs.Height);
					size /= 4f;

					// Create shape points
					PointF[] points = new PointF[7];
					if(anchorPoint.X < rectanglePosition.X && 
						anchorPoint.Y > rectanglePosition.Bottom)
					{
						points[0] = rectanglePositionAbs.Location;
						points[1] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y);
						points[2] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
						points[3] = new PointF(rectanglePositionAbs.X + size, rectanglePositionAbs.Bottom);
						points[4] = anchorPointAbs;
						points[5] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom - size);
						points[6] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom - size);
					}
					else if(anchorPoint.X >= rectanglePosition.X && 
						anchorPoint.X <= rectanglePosition.Right &&
						anchorPoint.Y > rectanglePosition.Bottom)
					{
						points[0] = rectanglePositionAbs.Location;
						points[1] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y);
						points[2] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
						points[3] = new PointF(rectanglePositionAbs.X + rectanglePositionAbs.Width / 2f + size, rectanglePositionAbs.Bottom);
						points[4] = anchorPointAbs;
						points[5] = new PointF(rectanglePositionAbs.X + rectanglePositionAbs.Width / 2f - size, rectanglePositionAbs.Bottom);
						points[6] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
					}
					else if(anchorPoint.X > rectanglePosition.Right && 
						anchorPoint.Y > rectanglePosition.Bottom)
					{
						points[0] = rectanglePositionAbs.Location;
						points[1] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y);
						points[2] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom - size);
						points[3] = anchorPointAbs;
						points[4] = new PointF(rectanglePositionAbs.Right - size, rectanglePositionAbs.Bottom);
						points[5] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
						points[6] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
					}
					else if(anchorPoint.X > rectanglePosition.Right && 
						anchorPoint.Y <= rectanglePosition.Bottom && 
						anchorPoint.Y >= rectanglePosition.Y)
					{
						points[0] = rectanglePositionAbs.Location;
						points[1] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y);
						points[2] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y + rectanglePositionAbs.Height / 2f - size);
						points[3] = anchorPointAbs;
						points[4] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y + rectanglePositionAbs.Height / 2f + size);
						points[5] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
						points[6] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
					}
					else if(anchorPoint.X > rectanglePosition.Right && 
						anchorPoint.Y < rectanglePosition.Y)
					{
						points[0] = rectanglePositionAbs.Location;
						points[1] = new PointF(rectanglePositionAbs.Right - size, rectanglePositionAbs.Y);
						points[2] = anchorPointAbs;
						points[3] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y + size);
						points[4] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
						points[5] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
						points[6] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
					}
					else if(anchorPoint.X >= rectanglePosition.X && 
						anchorPoint.X <= rectanglePosition.Right && 
						anchorPoint.Y < rectanglePosition.Y)
					{
						points[0] = rectanglePositionAbs.Location;
						points[1] = new PointF(rectanglePositionAbs.X + rectanglePositionAbs.Width/2f - size, rectanglePositionAbs.Y);
						points[2] = anchorPointAbs;
						points[3] = new PointF(rectanglePositionAbs.X + rectanglePositionAbs.Width/2f + size, rectanglePositionAbs.Y);
						points[4] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y);
						points[5] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
						points[6] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
					}
					else if(anchorPoint.X < rectanglePosition.X &&
						anchorPoint.Y < rectanglePosition.Y)
					{
						points[0] = anchorPointAbs;
						points[1] = new PointF(rectanglePositionAbs.X + size, rectanglePositionAbs.Y);
						points[2] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y);
						points[3] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
						points[4] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
						points[5] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Y + size);
						points[6] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Y + size);
					}
					else if(anchorPoint.X < rectanglePosition.X &&
						anchorPoint.Y >= rectanglePosition.Y &&
						anchorPoint.Y <= rectanglePosition.Bottom)
					{
						points[0] = rectanglePositionAbs.Location;
						points[1] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y);
						points[2] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
						points[3] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
						points[4] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Y + rectanglePositionAbs.Height/2f + size );
						points[5] = anchorPointAbs;
						points[6] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Y + rectanglePositionAbs.Height/2f - size );
					}

					// Create graphics path of the callout
                    hotRegion = new GraphicsPath();
                
                    hotRegion.AddLines(points);
                    hotRegion.CloseAllFigures();

                    // Draw callout
                    graphics.DrawPathAbs(
                        hotRegion,
                        this.BackColor,
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
			}
		
			// Draw rectangle if anchor is not visible
			if(!anchorVisible)
			{
				graphics.FillRectangleRel(
					rectanglePosition,
					this.BackColor,
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
					this.ShadowColor,
					this.ShadowOffset,
					PenAlignment.Center);

				// Get hot region
				hotRegion = new GraphicsPath();
				hotRegion.AddRectangle( graphics.GetAbsoluteRectangle(rectanglePosition) );
			}

			// Draw text 
			DrawText(graphics, rectanglePosition, false, false);

			return hotRegion;
		}

		/// <summary>
		/// Draws Perspective style callout.
		/// </summary>
		/// <param name="graphics">Chart graphics.</param>
		/// <param name="rectanglePosition">Position of annotation objet.</param>
		/// <param name="anchorPoint">Anchor location.</param>
		/// <returns>Hot region of the cloud.</returns>
		private GraphicsPath DrawCloudCallout(
			ChartGraphics graphics,
			RectangleF rectanglePosition,
			PointF anchorPoint)
		{
			// Get absolute position
			RectangleF rectanglePositionAbs = graphics.GetAbsoluteRectangle(rectanglePosition);

			// Draw perspective polygons from anchoring point
			if(!float.IsNaN(anchorPoint.X) && !float.IsNaN(anchorPoint.Y))
			{
				// Check if point is inside annotation position
				if(!rectanglePosition.Contains(anchorPoint.X, anchorPoint.Y))
				{
					// Get center point of the cloud
					PointF cloudCenterAbs = graphics.GetAbsolutePoint(
						new PointF(
						rectanglePosition.X + rectanglePosition.Width / 2f, 
						rectanglePosition.Y + rectanglePosition.Height / 2f) );

					// Calculate absolute ellipse size and position
					SizeF ellipseSize = graphics.GetAbsoluteSize(
						new SizeF(rectanglePosition.Width, rectanglePosition.Height));
					ellipseSize.Width /= 10f;
					ellipseSize.Height /= 10f;
					PointF anchorPointAbs = graphics.GetAbsolutePoint(
						new PointF(anchorPoint.X, anchorPoint.Y));
					PointF ellipseLocation = anchorPointAbs;

					// Get distance between anchor point and center of the cloud
					float dxAbs = anchorPointAbs.X - cloudCenterAbs.X;
					float dyAbs = anchorPointAbs.Y - cloudCenterAbs.Y;

					PointF point = PointF.Empty;
					if(anchorPoint.Y < rectanglePosition.Y)
					{
						point = GetIntersectionY(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Y);
						if(point.X < rectanglePositionAbs.X)
						{
							point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.X);
						}
						else if(point.X > rectanglePositionAbs.Right)
						{
							point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Right);
						}
					}
					else if(anchorPoint.Y > rectanglePosition.Bottom)
					{
						point = GetIntersectionY(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Bottom);
						if(point.X < rectanglePositionAbs.X)
						{
							point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.X);
						}
						else if(point.X > rectanglePositionAbs.Right)
						{
							point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Right);
						}
					}
					else
					{
						if(anchorPoint.X < rectanglePosition.X)
						{
							point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.X);
						}
						else
						{
							point = GetIntersectionX(cloudCenterAbs, anchorPointAbs, rectanglePositionAbs.Right);
						}
					}
						
					SizeF size = new SizeF(Math.Abs(cloudCenterAbs.X - point.X), Math.Abs(cloudCenterAbs.Y - point.Y));
					if(dxAbs > 0)
						dxAbs -= size.Width;
					else
						dxAbs += size.Width;

					if(dyAbs > 0)
						dyAbs -= size.Height;
					else
						dyAbs += size.Height;


					// Draw 3 smaller ellipses from anchor point to the cloud
					for(int index = 0; index < 3; index++)
					{
						using( GraphicsPath path = new GraphicsPath() )
						{
							// Create ellipse path
							path.AddEllipse(
								ellipseLocation.X - ellipseSize.Width / 2f,
								ellipseLocation.Y - ellipseSize.Height / 2f,
								ellipseSize.Width,
								ellipseSize.Height);

							// Draw ellipse
							graphics.DrawPathAbs(
								path,
								this.BackColor,
								this.BackHatchStyle,
								String.Empty,
								ChartImageWrapMode.Scaled,
								Color.Empty,
								ChartImageAlignmentStyle.Center,
								this.BackGradientStyle,
								this.BackSecondaryColor,
								this.LineColor,
								1, // this.LineWidth,	NOTE: Cloud supports only 1 pixel border
								this.LineDashStyle,
								PenAlignment.Center,
								this.ShadowOffset,
								this.ShadowColor);

							// Adjust ellipse size
							ellipseSize.Width *= 1.5f;
							ellipseSize.Height *= 1.5f;

							// Adjust next ellipse position
							ellipseLocation.X -= dxAbs / 3f + (index * (dxAbs / 10f) );
							ellipseLocation.Y -= dyAbs / 3f + (index * (dyAbs / 10f) );
						}
					}
				}
			}

			// Draw cloud
			GraphicsPath pathCloud = GetCloudPath(rectanglePositionAbs);
			graphics.DrawPathAbs(
				pathCloud,
				this.BackColor,
				this.BackHatchStyle,
				String.Empty,
				ChartImageWrapMode.Scaled,
				Color.Empty,
				ChartImageAlignmentStyle.Center,
				this.BackGradientStyle,
				this.BackSecondaryColor,
				this.LineColor,
				1, // this.LineWidth,	NOTE: Cloud supports only 1 pixel border
				this.LineDashStyle,
				PenAlignment.Center,
				this.ShadowOffset,
				this.ShadowColor);

			// Draw cloud outline (Do not draw in SVG or Flash Animation)
			{
				using(GraphicsPath pathCloudOutline = GetCloudOutlinePath(rectanglePositionAbs))
				{
					graphics.DrawPathAbs(
						pathCloudOutline,
						this.BackColor,
						this.BackHatchStyle,
						String.Empty,
						ChartImageWrapMode.Scaled,
						Color.Empty,
						ChartImageAlignmentStyle.Center,
						this.BackGradientStyle,
						this.BackSecondaryColor,
						this.LineColor,
						1, // this.LineWidth,	NOTE: Cloud supports only 1 pixel border
						this.LineDashStyle,
						PenAlignment.Center);
				}
			}
			
			// Draw text 
			DrawText(graphics, rectanglePosition, true, false);

			return pathCloud;
		}

		/// <summary>
		/// Draws Perspective style callout.
		/// </summary>
		/// <param name="graphics">Chart graphics.</param>
		/// <param name="rectanglePosition">Position of annotation objet.</param>
		/// <param name="anchorPoint">Anchor location.</param>
		/// <returns>Hot region of the cloud.</returns>
		private GraphicsPath DrawPerspectiveCallout(
			ChartGraphics graphics,
			RectangleF rectanglePosition,
			PointF anchorPoint)
		{
			// Draw rectangle
			graphics.FillRectangleRel(
				rectanglePosition,
				this.BackColor,
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
				this.ShadowColor,
				0,	// Shadow is never drawn
				PenAlignment.Center);

			// Create hot region path
			GraphicsPath hotRegion = new GraphicsPath();
			hotRegion.AddRectangle( graphics.GetAbsoluteRectangle(rectanglePosition) );

			// Draw text 
			DrawText(graphics, rectanglePosition, false, false);

			// Draw perspective polygons from anchoring point
			if(!float.IsNaN(anchorPoint.X) && !float.IsNaN(anchorPoint.Y))
			{
				// Check if point is inside annotation position
				if(!rectanglePosition.Contains(anchorPoint.X, anchorPoint.Y))
				{
					Color[] perspectivePathColors = new Color[2];
					Color color = (this.BackColor.IsEmpty) ? Color.White : this.BackColor;
					perspectivePathColors[0] = graphics.GetBrightGradientColor(color, 0.6);
					perspectivePathColors[1] = graphics.GetBrightGradientColor(color, 0.8);
					GraphicsPath[] perspectivePaths = new GraphicsPath[2];
					using(perspectivePaths[0] = new GraphicsPath()) 
					{
						using(perspectivePaths[1] = new GraphicsPath()) 
						{
							// Convert coordinates to absolute
							RectangleF rectanglePositionAbs = graphics.GetAbsoluteRectangle(rectanglePosition);
							PointF anchorPointAbs = graphics.GetAbsolutePoint(anchorPoint);

							// Create paths of perspective
							if(anchorPoint.Y < rectanglePosition.Y)
							{
								PointF[] points1 = new PointF[3];
								points1[0] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Y);
								points1[1] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y);
								points1[2] = new PointF(anchorPointAbs.X, anchorPointAbs.Y);
								perspectivePaths[0].AddLines(points1);
								if(anchorPoint.X < rectanglePosition.X)
								{
									PointF[] points2 = new PointF[3];
									points2[0] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
									points2[1] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Y);
									points2[2] = new PointF(anchorPointAbs.X, anchorPointAbs.Y);
									perspectivePaths[1].AddLines(points2);
								}
								else if(anchorPoint.X > rectanglePosition.Right)
								{
									PointF[] points2 = new PointF[3];
									points2[0] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
									points2[1] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y);
									points2[2] = new PointF(anchorPointAbs.X, anchorPointAbs.Y);
									perspectivePaths[1].AddLines(points2);
								}
							}
							else if(anchorPoint.Y > rectanglePosition.Bottom)
							{
								PointF[] points1 = new PointF[3];
								points1[0] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
								points1[1] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
								points1[2] = new PointF(anchorPointAbs.X, anchorPointAbs.Y);
								perspectivePaths[0].AddLines(points1);
								if(anchorPoint.X < rectanglePosition.X)
								{
									PointF[] points2 = new PointF[3];
									points2[0] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
									points2[1] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Y);
									points2[2] = new PointF(anchorPointAbs.X, anchorPointAbs.Y);
									perspectivePaths[1].AddLines(points2);
								}
								else if(anchorPoint.X > rectanglePosition.Right)
								{
									PointF[] points2 = new PointF[3];
									points2[0] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
									points2[1] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y);
									points2[2] = new PointF(anchorPointAbs.X, anchorPointAbs.Y);
									perspectivePaths[1].AddLines(points2);
								}
							}
							else
							{
								if(anchorPoint.X < rectanglePosition.X)
								{
									PointF[] points2 = new PointF[3];
									points2[0] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Bottom);
									points2[1] = new PointF(rectanglePositionAbs.X, rectanglePositionAbs.Y);
									points2[2] = new PointF(anchorPointAbs.X, anchorPointAbs.Y);
									perspectivePaths[1].AddLines(points2);
								}
								else if(anchorPoint.X > rectanglePosition.Right)
								{
									PointF[] points2 = new PointF[3];
									points2[0] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Bottom);
									points2[1] = new PointF(rectanglePositionAbs.Right, rectanglePositionAbs.Y);
									points2[2] = new PointF(anchorPointAbs.X, anchorPointAbs.Y);
									perspectivePaths[1].AddLines(points2);
								}
							}

							// Draw paths if non-empty
							int index = 0;
							foreach(GraphicsPath path in perspectivePaths)
							{
								if(path.PointCount > 0)
								{
									path.CloseAllFigures();
									graphics.DrawPathAbs(
										path,
										perspectivePathColors[index],
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
										PenAlignment.Center);

									// Add area to hot region path
									hotRegion.SetMarkers();
									hotRegion.AddPath( path, false );
								}
								++index;
							}
						}
					}
				}
			}

			return hotRegion;
		}

		/// <summary>
		/// Draws SimpleLine or BorderLine style callout.
		/// </summary>
		/// <param name="graphics">Chart graphics.</param>
		/// <param name="rectanglePosition">Position of annotation objet.</param>
		/// <param name="anchorPoint">Anchor location.</param>
		/// <param name="drawRectangle">If true draws BorderLine style, otherwise SimpleLine.</param>
		/// <returns>Hot region of the cloud.</returns>
		private GraphicsPath DrawRectangleLineCallout(
			ChartGraphics graphics,
			RectangleF rectanglePosition,
			PointF anchorPoint,
			bool drawRectangle)
		{
			// Rectangle mode
			if(drawRectangle)
			{
				// Draw rectangle
				graphics.FillRectangleRel(
					rectanglePosition,
					this.BackColor,
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
					this.ShadowColor,
					this.ShadowOffset,
					PenAlignment.Center);

				// Draw text 
				DrawText(graphics, rectanglePosition, false, false);
			}
			else
			{
				// Draw text 
				rectanglePosition = DrawText(graphics, rectanglePosition, false, true);
				SizeF pixelSize = graphics.GetRelativeSize(new SizeF(2f, 2f));
				rectanglePosition.Inflate(pixelSize);
			}

			// Create hot region path
			GraphicsPath hotRegion = new GraphicsPath();
			hotRegion.AddRectangle( graphics.GetAbsoluteRectangle(rectanglePosition) );

			// Define position of text underlying line
			PointF	textLinePoint1 = new PointF(rectanglePosition.X, rectanglePosition.Bottom);
			PointF	textLinePoint2 = new PointF(rectanglePosition.Right, rectanglePosition.Bottom);

			// Draw line to the anchor point
			if(!float.IsNaN(anchorPoint.X) && !float.IsNaN(anchorPoint.Y))
			{
				// Check if point is inside annotation position
				if(!rectanglePosition.Contains(anchorPoint.X, anchorPoint.Y))
				{
					PointF	lineSecondPoint = PointF.Empty;
					if(anchorPoint.X < rectanglePosition.X)
					{
						lineSecondPoint.X = rectanglePosition.X;
					}
					else if(anchorPoint.X > rectanglePosition.Right)
					{
						lineSecondPoint.X = rectanglePosition.Right;
					}
					else
					{
						lineSecondPoint.X = rectanglePosition.X + rectanglePosition.Width / 2f;
					}

					if(anchorPoint.Y < rectanglePosition.Y)
					{
						lineSecondPoint.Y = rectanglePosition.Y;
					}
					else if(anchorPoint.Y > rectanglePosition.Bottom)
					{
						lineSecondPoint.Y = rectanglePosition.Bottom;
					}
					else
					{
						lineSecondPoint.Y = rectanglePosition.Y + rectanglePosition.Height / 2f;
					}

					// Set line caps
					bool capChanged = false;
					LineCap	oldStartCap = LineCap.Flat;
					if(this.CalloutAnchorCap != LineAnchorCapStyle.None)
					{
						// Save old pen
						capChanged = true;
						oldStartCap = graphics.Pen.StartCap;

						// Apply anchor cap settings
						if(this.CalloutAnchorCap == LineAnchorCapStyle.Arrow)
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
						else if(this.CalloutAnchorCap == LineAnchorCapStyle.Diamond)
						{
                            graphics.Pen.StartCap = LineCap.DiamondAnchor;
						}
						else if(this.CalloutAnchorCap == LineAnchorCapStyle.Round)
						{
                            graphics.Pen.StartCap = LineCap.RoundAnchor;
						}
						else if(this.CalloutAnchorCap == LineAnchorCapStyle.Square)
						{
                            graphics.Pen.StartCap = LineCap.SquareAnchor;
						}
					}

					// Draw callout line
					graphics.DrawLineAbs(
						this.LineColor,
						this.LineWidth,
						this.LineDashStyle,
						graphics.GetAbsolutePoint(anchorPoint),
						graphics.GetAbsolutePoint(lineSecondPoint),
						this.ShadowColor,
						this.ShadowOffset);

					// Create hot region path
					using( GraphicsPath linePath = new GraphicsPath() )
					{
						linePath.AddLine(						
							graphics.GetAbsolutePoint(anchorPoint),
							graphics.GetAbsolutePoint(lineSecondPoint) );

						linePath.Widen(new Pen(Color.Black, this.LineWidth + 2));
						hotRegion.SetMarkers();
						hotRegion.AddPath( linePath, false );
					}

					// Restore line caps
					if(capChanged)
					{
                        graphics.Pen.StartCap = oldStartCap;
					}

					// Adjust text underlying line position
					if(anchorPoint.Y < rectanglePosition.Y)
					{
						textLinePoint1.Y = rectanglePosition.Y;
						textLinePoint2.Y = rectanglePosition.Y;
					}
					else if(anchorPoint.Y > rectanglePosition.Y && 
						anchorPoint.Y < rectanglePosition.Bottom)
					{
						textLinePoint1.Y = rectanglePosition.Y;
						textLinePoint2.Y = rectanglePosition.Bottom;
						if(anchorPoint.X < rectanglePosition.X)
						{
							textLinePoint1.X = rectanglePosition.X;
							textLinePoint2.X = rectanglePosition.X;
						}
						else
						{
							textLinePoint1.X = rectanglePosition.Right;
							textLinePoint2.X = rectanglePosition.Right;
						}
					}
				}

				// Draw text underlying line
				if(!drawRectangle)
				{
					graphics.DrawLineAbs(
						this.LineColor,
						this.LineWidth,
						this.LineDashStyle,
						graphics.GetAbsolutePoint(textLinePoint1),
						graphics.GetAbsolutePoint(textLinePoint2),
						this.ShadowColor,
						this.ShadowOffset);

					// Create hot region path
					using( GraphicsPath linePath = new GraphicsPath() )
					{
						linePath.AddLine(						
							graphics.GetAbsolutePoint(textLinePoint1),
							graphics.GetAbsolutePoint(textLinePoint2) );

						linePath.Widen(new Pen(Color.Black, this.LineWidth + 2));
						hotRegion.SetMarkers();
						hotRegion.AddPath( linePath, false );
					}

				}
			}

			return hotRegion;
		}

		#endregion // Painting

		#region Anchor Methods

		/// <summary>
		/// Checks if annotation draw anything in the anchor position (except selection handle)
		/// </summary>
		/// <returns>True if annotation "connects" itself and anchor point visually.</returns>
		override internal bool IsAnchorDrawn()
		{
			return true;
		}

		#endregion // Anchor Methods

		#region Helper methods

		/// <summary>
		/// Gets cloud callout outline graphics path.
		/// </summary>
		/// <param name="position">Absolute position of the callout cloud.</param>
		/// <returns>Cloud outline path.</returns>
		private static GraphicsPath GetCloudOutlinePath(RectangleF position)
		{
			if(_cloudOutlinePath == null)
			{
				GetCloudPath(position);
			}

			// Translate and sacle original path to fit specified position
			GraphicsPath resultPath = (GraphicsPath)_cloudOutlinePath.Clone();
			Matrix matrix = new Matrix();
			matrix.Translate(-_cloudBounds.X, -_cloudBounds.Y);
			resultPath.Transform(matrix);
			matrix = new Matrix();
			matrix.Translate(position.X, position.Y);
			matrix.Scale(position.Width / _cloudBounds.Width, position.Height / _cloudBounds.Height);
			resultPath.Transform(matrix);

			return resultPath;
		}
	
		/// <summary>
		/// Gets cloud callout graphics path.
		/// </summary>
		/// <param name="position">Absolute position of the callout cloud.</param>
		/// <returns>Cloud path.</returns>
		private static GraphicsPath GetCloudPath(RectangleF position)
		{
			// Check if cloud path was already created
			if(_cloudPath == null)
			{
				// Create cloud path
				_cloudPath = new GraphicsPath();

				_cloudPath.AddBezier(1689.5f, 1998.6f, 1581.8f, 2009.4f, 1500f, 2098.1f, 1500f, 2204f);

				_cloudPath.AddBezier(1500f, 2204f, 1499.9f, 2277.2f, 1539.8f, 2345.1f, 1604.4f, 2382.1f);

				_cloudPath.AddBezier(1603.3f, 2379.7f, 1566.6f, 2417.8f, 1546.2f, 2468.1f, 1546.2f, 2520.1f);
				_cloudPath.AddBezier(1546.2f, 2520.1f, 1546.2f, 2633.7f, 1641.1f, 2725.7f, 1758.1f, 2725.7f);
				_cloudPath.AddBezier(1758.1f, 2725.7f, 1766.3f, 2725.6f, 1774.6f, 2725.2f, 1782.8f, 2724.2f);

				_cloudPath.AddBezier(1781.7f, 2725.6f, 1848.5f, 2839.4f, 1972.8f, 2909.7f, 2107.3f, 2909.7f);
				_cloudPath.AddBezier(2107.3f, 2909.7f, 2175.4f, 2909.7f, 2242.3f, 2891.6f, 2300.6f, 2857.4f);

				_cloudPath.AddBezier(2300f, 2857.6f, 2360.9f, 2946.5f, 2463.3f, 2999.7f, 2572.9f, 2999.7f);
				_cloudPath.AddBezier(2572.9f, 2999.7f, 2717.5f, 2999.7f, 2845.2f, 2907.4f, 2887.1f, 2772.5f);

				_cloudPath.AddBezier(2887.4f, 2774.3f, 2932.1f, 2801.4f, 2983.6f, 2815.7f, 3036.3f, 2815.7f);
				_cloudPath.AddBezier(3036.3f, 2815.7f, 3190.7f, 2815.7f, 3316.3f, 2694.8f, 3317.5f, 2544.8f);

				_cloudPath.AddBezier(3317f, 2544.1f, 3479.2f, 2521.5f, 3599.7f, 2386.5f, 3599.7f, 2227.2f);
				_cloudPath.AddBezier(3599.7f, 2227.2f, 3599.7f, 2156.7f, 3575.7f, 2088.1f, 3531.6f, 2032.2f);

				_cloudPath.AddBezier(3530.9f, 2032f, 3544.7f, 2000.6f, 3551.9f, 1966.7f, 3551.9f, 1932.5f);
				_cloudPath.AddBezier(3551.9f, 1932.5f, 3551.9f, 1818.6f, 3473.5f, 1718.8f, 3360.7f, 1688.8f);

				_cloudPath.AddBezier(3361.6f, 1688.3f, 3341.4f, 1579.3f, 3243.5f, 1500f, 3129.3f, 1500f);
				_cloudPath.AddBezier(3129.3f, 1500f, 3059.8f, 1499.9f, 2994f, 1529.6f, 2949.1f, 1580.9f);

				_cloudPath.AddBezier(2949.5f, 1581.3f, 2909.4f, 1530f, 2847f, 1500f, 2780.8f, 1500f);
				_cloudPath.AddBezier(2780.8f, 1500f, 2700.4f, 1499.9f, 2626.8f, 1544.2f, 2590.9f, 1614.2f);

				_cloudPath.AddBezier(2591.7f, 1617.6f, 2543.2f, 1571.1f, 2477.9f, 1545.1f, 2409.8f, 1545.1f);
				_cloudPath.AddBezier(2409.8f, 1545.1f, 2313.9f, 1545.1f, 2225.9f, 1596.6f, 2180.8f, 1679f);

				_cloudPath.AddBezier(2180.1f, 1680.7f, 2129.7f, 1652f, 2072.4f, 1636.9f, 2014.1f, 1636.9f);
				_cloudPath.AddBezier(2014.1f, 1636.9f, 1832.8f, 1636.9f, 1685.9f, 1779.8f, 1685.9f, 1956f);
				_cloudPath.AddBezier(1685.9f, 1956f, 1685.8f, 1970.4f, 1686.9f, 1984.8f, 1688.8f, 1999f);

				_cloudPath.CloseAllFigures();


				// Create cloud outline path
				_cloudOutlinePath = new GraphicsPath();

				_cloudOutlinePath.AddBezier(1604.4f, 2382.1f, 1636.8f, 2400.6f, 1673.6f, 2410.3f, 1711.2f, 2410.3f);
				_cloudOutlinePath.AddBezier(1711.2f, 2410.3f, 1716.6f, 2410.3f, 1722.2f, 2410.2f, 1727.6f, 2409.8f);
			
				_cloudOutlinePath.StartFigure();
				_cloudOutlinePath.AddBezier(1782.8f, 2724.2f, 1801.3f, 2722.2f, 1819.4f, 2717.7f, 1836.7f, 2711f);

				_cloudOutlinePath.StartFigure();
				_cloudOutlinePath.AddBezier(2267.6f, 2797.2f, 2276.1f, 2818.4f, 2287f, 2838.7f, 2300f, 2857.6f);

				_cloudOutlinePath.StartFigure();
				_cloudOutlinePath.AddBezier(2887.1f, 2772.5f, 2893.8f, 2750.9f, 2898.1f, 2728.7f, 2900f, 2706.3f);

				// NOTE: This cloud segment overlaps text too much. Removed for now!
				//cloudOutlinePath.StartFigure();
				//cloudOutlinePath.AddBezier(3317.5f, 2544.8f, 3317.5f, 2544f, 3317.6f, 2543.3f, 3317.6f, 2542.6f);
				//cloudOutlinePath.AddBezier(3317.6f, 2542.6f, 3317.6f, 2438.1f, 3256.1f, 2342.8f, 3159.5f, 2297f);

				_cloudOutlinePath.StartFigure();
				_cloudOutlinePath.AddBezier(3460.5f, 2124.9f, 3491f, 2099.7f, 3515f, 2067.8f, 3530.9f, 2032f);

				_cloudOutlinePath.StartFigure();
				_cloudOutlinePath.AddBezier(3365.3f, 1732.2f, 3365.3f, 1731.1f, 3365.4f, 1730.1f, 3365.4f, 1729f);
				_cloudOutlinePath.AddBezier(3365.4f, 1729f, 3365.4f, 1715.3f, 3364.1f, 1701.7f, 3361.6f, 1688.3f);

				_cloudOutlinePath.StartFigure();
				_cloudOutlinePath.AddBezier(2949.1f, 1580.9f, 2934.4f, 1597.8f, 2922.3f, 1616.6f, 2913.1f, 1636.9f);
				_cloudOutlinePath.CloseFigure();

				_cloudOutlinePath.StartFigure();
				_cloudOutlinePath.AddBezier(2590.9f, 1614.2f, 2583.1f, 1629.6f, 2577.2f, 1645.8f, 2573.4f, 1662.5f);

				_cloudOutlinePath.StartFigure();
				_cloudOutlinePath.AddBezier(2243.3f, 1727.5f, 2224.2f, 1709.4f, 2203f, 1693.8f, 2180.1f, 1680.7f);

				_cloudOutlinePath.StartFigure();
				_cloudOutlinePath.AddBezier(1688.8f, 1999f, 1691.1f, 2015.7f, 1694.8f, 2032.2f, 1699.9f, 2048.3f);

				_cloudOutlinePath.CloseAllFigures();

				// Get cloud path bounds
				_cloudBounds = _cloudPath.GetBounds();
			}

			// Translate and sacle original path to fit specified position
			GraphicsPath resultPath = (GraphicsPath)_cloudPath.Clone();
			Matrix matrix = new Matrix();
			matrix.Translate(-_cloudBounds.X, -_cloudBounds.Y);
			resultPath.Transform(matrix);
			matrix = new Matrix();
			matrix.Translate(position.X, position.Y);
			matrix.Scale(position.Width / _cloudBounds.Width, position.Height / _cloudBounds.Height);
			resultPath.Transform(matrix);

			return resultPath;
		}

		/// <summary>
		/// Gets intersection point coordinates between point line and and horizontal 
		/// line specified by Y coordinate.
		/// </summary>
		/// <param name="firstPoint">First data point.</param>
		/// <param name="secondPoint">Second data point.</param>
		/// <param name="pointY">Y coordinate.</param>
		/// <returns>Intersection point coordinates.</returns>
		internal static PointF GetIntersectionY(PointF firstPoint, PointF secondPoint, float pointY)
		{
			PointF	intersectionPoint = new PointF();
			intersectionPoint.Y = pointY;
			intersectionPoint.X = (pointY - firstPoint.Y) *
				(secondPoint.X - firstPoint.X) / 
				(secondPoint.Y - firstPoint.Y) + 
				firstPoint.X;
			return intersectionPoint;
		}

		/// <summary>
		/// Gets intersection point coordinates between point line and and vertical 
		/// line specified by X coordinate.
		/// </summary>
		/// <param name="firstPoint">First data point.</param>
		/// <param name="secondPoint">Second data point.</param>
		/// <param name="pointX">X coordinate.</param>
		/// <returns>Intersection point coordinates.</returns>
		internal static PointF GetIntersectionX(PointF firstPoint, PointF secondPoint, float pointX)
		{
			PointF	intersectionPoint = new PointF();
			intersectionPoint.X = pointX;
			intersectionPoint.Y = (pointX - firstPoint.X) *
				(secondPoint.Y - firstPoint.Y) / 
				(secondPoint.X - firstPoint.X) + 
				firstPoint.Y;
			return intersectionPoint;
		}

		/// <summary>
		/// Adds a horizontal or vertical line into the path as multiple segments.
		/// </summary>
		/// <param name="path">Graphics path.</param>
		/// <param name="x1">First point X coordinate.</param>
		/// <param name="y1">First point Y coordinate.</param>
		/// <param name="x2">Second point X coordinate.</param>
		/// <param name="y2">Second point Y coordinate.</param>
		/// <param name="segments">Number of segments to add.</param>
		private void PathAddLineAsSegments(GraphicsPath path, float x1, float y1, float x2, float y2, int segments)
		{
			if(x1 == x2)
			{
				float distance = (y2 - y1) / segments;
				for(int index = 0; index < segments; index++)
				{
					path.AddLine(x1, y1, x1, y1 + distance);
					y1 += distance;
				}
			}
			else if(y1 == y2)
			{
				float distance = (x2 - x1) / segments;
				for(int index = 0; index < segments; index++)
				{
					path.AddLine(x1, y1, x1 + distance, y1);
					x1 += distance;
				}
			}
			else
			{
                throw (new InvalidOperationException(SR.ExceptionAnnotationPathAddLineAsSegmentsInvalid));
			}
		}
		/// <summary>
		/// Helper function which creates a rounded rectangle path.
		/// Extra points are added on the sides to allow anchor connection.
		/// </summary>
		/// <param name="rect">Rectangle coordinates.</param>
		/// <param name="cornerRadius">Corner radius.</param>
		/// <returns>Graphics path object.</returns>
		private GraphicsPath CreateRoundedRectPath(RectangleF rect, float cornerRadius)
		{
			// Create rounded rectangle path
			GraphicsPath path = new GraphicsPath();
			int segments = 10;
			PathAddLineAsSegments(path, rect.X+cornerRadius, rect.Y, rect.Right-cornerRadius, rect.Y, segments);

			path.AddArc(rect.Right-2f*cornerRadius, rect.Y, 2f*cornerRadius, 2f*cornerRadius, 270, 90);

			PathAddLineAsSegments(path, rect.Right, rect.Y + cornerRadius, rect.Right, rect.Bottom - cornerRadius, segments);

			path.AddArc(rect.Right-2f*cornerRadius, rect.Bottom-2f*cornerRadius, 2f*cornerRadius, 2f*cornerRadius, 0, 90);

			PathAddLineAsSegments(path, rect.Right-cornerRadius, rect.Bottom, rect.X + cornerRadius, rect.Bottom, segments);

			path.AddArc(rect.X, rect.Bottom-2f*cornerRadius, 2f*cornerRadius, 2f*cornerRadius, 90, 90);

			PathAddLineAsSegments(path, rect.X, rect.Bottom-cornerRadius, rect.X, rect.Y+cornerRadius, segments);

			path.AddArc(rect.X, rect.Y, 2f*cornerRadius, 2f*cornerRadius, 180, 90);

			return path;
		}

		#endregion // Helper methods

		#endregion
	}
}
