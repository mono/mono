//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ImageAnnotation.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ImageAnnotation
//
//  Purpose:	Image annotation classes.
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
	/// <summary>
	/// <b>ImageAnnotation</b> is a class that represents an image annotation.
	/// </summary>
	[
		SRDescription("DescriptionAttributeImageAnnotation_ImageAnnotation"),
	]
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public class ImageAnnotation : Annotation
	{
		#region Fields

		// Annotation image name
		private		string					_imageName = String.Empty;

		// Image wrapping mode
		private		ChartImageWrapMode		_imageWrapMode = ChartImageWrapMode.Scaled;

		// Image transparent color
		private		Color					_imageTransparentColor = Color.Empty;

		#endregion

		#region Construction and Initialization

		/// <summary>
		/// Default public constructor.
		/// </summary>
		public ImageAnnotation() 
            : base()
		{
		}

		#endregion

		#region Properties

		#region Image properties

		/// <summary>
        /// Gets or sets the name of an annotation's image. 
		/// <seealso cref="ImageTransparentColor"/>
		/// </summary>
		/// <value>
		/// A string value representing the name of an annotation's image.
		/// </value>
		/// <remarks>
		/// The name can be a file name, URL for the web control or a name from 
		/// the <see cref="NamedImagesCollection"/> class.	
		/// </remarks>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(""),
		Editor(Editors.ImageValueEditor.Editor, Editors.ImageValueEditor.Base),
		SRDescription("DescriptionAttributeImageAnnotation_Image"),
		]
		public virtual string Image
		{
			get
			{
				return _imageName;
			}
			set
			{
				_imageName = value;
				this.Invalidate();
			}
		}


		/// <summary>
		/// Gets or sets the drawing mode of the image.
		/// </summary>
		/// <value>
		/// A <see cref="ChartImageWrapMode"/> value that defines the drawing mode of the image. 
		/// </value>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(ChartImageWrapMode.Scaled),
        SRDescription("DescriptionAttributeImageWrapMode"),
		]
		public ChartImageWrapMode ImageWrapMode
		{
			get
			{
				return _imageWrapMode;
			}
			set
			{
				_imageWrapMode = value;
				this.Invalidate();
			}
		}

		/// <summary>
		/// Gets or sets a color which will be replaced with a transparent color while drawing the image.
		/// </summary>
		/// <value>
        /// A <see cref="Color"/> value which will be replaced with a transparent color while drawing the image.
		/// </value>
		[
		SRCategory("CategoryAttributeImage"),
		Bindable(true),
		DefaultValue(typeof(Color), ""),
        SRDescription("DescriptionAttributeImageTransparentColor"),
        TypeConverter(typeof(ColorConverter)),
        Editor(Editors.ChartColorEditor.Editor, Editors.ChartColorEditor.Base)
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
		SRCategory("CategoryAttributeImage"),
		DefaultValue(typeof(ContentAlignment), "MiddleCenter"),
		SRDescription("DescriptionAttributeImageAnnotation_Alignment"),
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
				Invalidate();
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

		#endregion // Image properties

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
				return "Image";
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

		#region Non Applicable Annotation Appearance Attributes (set as Non-Browsable)

		/// <summary>
		/// Not applicable to this type of annotation.
		/// <seealso cref="Font"/>
		/// </summary>
		/// <value>
		/// A <see cref="Color"/> value.
		/// </value>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Color), "Black"),
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
		/// Not applicable to this type of annotation.
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

        /// <summary>
        /// Not applicable to this annotation type.
        /// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
		DefaultValue(typeof(Color), "Black"),
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
        /// Not applicable to this annotation type.
        /// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		DefaultValue(1),
		Browsable(false),
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
        /// Not applicable to this annotation type.
        /// </summary>
		[
		SRCategory("CategoryAttributeAppearance"),
		Browsable(false),
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


		#endregion

		#endregion

		#region Methods

		#region Painting

		/// <summary>
		/// Paints the annotation object on the specified graphics.
		/// </summary>
		/// <param name="graphics">
		/// A <see cref="ChartGraphics"/> object, used to paint the annotation object.
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

			// Get position
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

			if(this.Common.ProcessModePaint)
			{
				// Draw "empty" image at design time
				if(this._imageName.Length == 0 && this.Chart.IsDesignMode() )
				{
					graphics.FillRectangleRel(
						rectanglePosition,
						this.BackColor,
						this.BackHatchStyle,
						this._imageName,
						this._imageWrapMode,
						this._imageTransparentColor,
						GetImageAlignment(this.Alignment),
						this.BackGradientStyle,
						this.BackSecondaryColor,
						this.LineColor,
						this.LineWidth,
						this.LineDashStyle,
						this.ShadowColor,
						this.ShadowOffset,
						PenAlignment.Center);

					// Draw text
					using( Brush textBrush = new SolidBrush(this.ForeColor) )
					{
                        using (StringFormat format = new StringFormat(StringFormat.GenericTypographic))
                        {
                            format.Alignment = StringAlignment.Center;
                            format.LineAlignment = StringAlignment.Center;
                            format.FormatFlags = StringFormatFlags.LineLimit;
                            format.Trimming = StringTrimming.EllipsisCharacter;
                            graphics.DrawStringRel(
                                "(no image)",
                                this.Font,
                                textBrush,
                                rectanglePosition,
                                format);
                        }
					}
				}
				else
				{
					// Draw image
					graphics.FillRectangleRel(
						rectanglePosition,
						Color.Transparent,
						this.BackHatchStyle,
						this._imageName,
						this._imageWrapMode,
						this._imageTransparentColor,
						GetImageAlignment(this.Alignment),
						this.BackGradientStyle,
						Color.Transparent,
						Color.Transparent,
						0,
						this.LineDashStyle,
						this.ShadowColor,
						this.ShadowOffset,
						PenAlignment.Center);
				}
			}

			if(this.Common.ProcessModeRegions)
			{
				// Add hot region
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

			// Paint selection handles
			PaintSelectionHandles(graphics, selectionRect, null);
		}

		/// <summary>
		/// Coverts ContentAlignment enumeration to ChartImageAlignmentStyle enumeration.
		/// </summary>
		/// <param name="alignment">Content alignment.</param>
		/// <returns>Image content alignment.</returns>
		private ChartImageAlignmentStyle GetImageAlignment(ContentAlignment alignment)
		{
			if(alignment == ContentAlignment.TopLeft)
			{
				return ChartImageAlignmentStyle.TopLeft;
			}
			else if(alignment == ContentAlignment.TopCenter)
			{
				return ChartImageAlignmentStyle.Top;
			}
			else if(alignment == ContentAlignment.TopRight)
			{
				return ChartImageAlignmentStyle.TopRight;
			}
			else if(alignment == ContentAlignment.MiddleRight)
			{
				return ChartImageAlignmentStyle.Right;
			}
			else if(alignment == ContentAlignment.BottomRight)
			{
				return ChartImageAlignmentStyle.BottomRight;
			}
			else if(alignment == ContentAlignment.BottomCenter)
			{
				return ChartImageAlignmentStyle.Bottom;
			}
			else if(alignment == ContentAlignment.BottomLeft)
			{
				return ChartImageAlignmentStyle.BottomLeft;
			}
			else if(alignment == ContentAlignment.MiddleLeft)
			{
				return ChartImageAlignmentStyle.Left;
			}
			return ChartImageAlignmentStyle.Center;
		}

		#endregion // Painting

		#region Content Size

		/// <summary>
		/// Gets text annotation content size based on the text and font.
		/// </summary>
		/// <returns>Annotation content position.</returns>
		override internal RectangleF GetContentPosition()
		{
			// Check image size
			if(this.Image.Length > 0)
			{
				// Try loading image and getting its size
				try
				{
					if(this.Chart != null)
					{
						ImageLoader imageLoader = this.Common.ImageLoader;
						
                        if(imageLoader != null)
						{
                            ChartGraphics chartGraphics = this.GetGraphics();

                            if (chartGraphics != null)
                            {
                                SizeF absSize = new SizeF();

                                if (imageLoader.GetAdjustedImageSize(this.Image, chartGraphics.Graphics, ref absSize))
                                {
                                    SizeF imageSize = chartGraphics.GetRelativeSize(absSize);
                                    return new RectangleF(float.NaN, float.NaN, imageSize.Width, imageSize.Height);
                                }
                            }
						}
					}
				}
				catch(ArgumentException)
				{
					// ArgumentException is thrown by LoadImage in certain situations when it can't load the image
				}
			}

			return new RectangleF(float.NaN, float.NaN, float.NaN, float.NaN);
		}

		#endregion

		#endregion
	}
}
