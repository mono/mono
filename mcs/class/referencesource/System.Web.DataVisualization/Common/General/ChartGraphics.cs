//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartGraphics.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ChartGraphics
//
//  Purpose:	Chart graphic class is used for drawing Chart 
//				elements as Rectangles, Pie slices, lines, areas 
//				etc. This class is used in all classes where 
//				drawing is necessary. The GDI+ graphic class is 
//				used throw this class. Encapsulates a GDI+ chart 
//				drawing functionality
//
//	Reviewed:	GS - Jul 31, 2002
//				AG - August 7, 2002
//              AG - Microsoft 16, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.Diagnostics.CodeAnalysis;

#if Microsoft_CONTROL
    using System.Windows.Forms.DataVisualization.Charting.Utilities;
    using System.Windows.Forms.DataVisualization.Charting.Borders3D;
#else
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
	/// Defines the style how the bars/columns are drawn.
	/// </summary>
    internal enum BarDrawingStyle
	{
		/// <summary>
		/// Default bar/column style.
		/// </summary>
		Default,

		/// <summary>
		/// Cylinder bar/column style.
		/// </summary>
		Cylinder,

		/// <summary>
		/// Emboss bar/column style.
		/// </summary>
		Emboss,

		/// <summary>
		/// LightToDark bar/column style.
		/// </summary>
		LightToDark,
		
		/// <summary>
		/// Wedge bar/column style.
		/// </summary>
		Wedge,
	}

	/// <summary>
	/// Defines the style how the pie and doughnut charts are drawn.
	/// </summary>
    internal enum PieDrawingStyle
	{
		/// <summary>
		/// Default pie/doughnut drawing style.
		/// </summary>
		Default,

        /// <summary>
        /// Soft edge shadow is drawn on the edges of the pie/doughnut slices.
        /// </summary>
		SoftEdge,

		/// <summary>
		/// A shadow is drawn from the top to the bottom of the pie/doughnut chart.
		/// </summary>
		Concave,
	}

	/// <summary>
	/// An enumeration of line styles.
	/// </summary>
	public enum ChartDashStyle
	{
		/// <summary>
		/// Line style not set
		/// </summary>
		NotSet,
		/// <summary>
		/// Specifies a line consisting of dashes. 
		/// </summary>
		Dash,
		/// <summary>
		/// Specifies a line consisting of a repeating pattern of dash-dot. 
		/// </summary>
		DashDot,
		/// <summary>
		/// Specifies a line consisting of a repeating pattern of dash-dot-dot. 
		/// </summary>
		DashDotDot,
		/// <summary>
		/// Specifies a line consisting of dots. 
		/// </summary>
		Dot,
		/// <summary>
		/// Specifies a solid line. 
		/// </summary>
		Solid,
	}

	#endregion

	/// <summary>
    /// The ChartGraphics class provides all chart drawing capabilities. 
    /// It contains methods for drawing 2D primitives and also exposes 
    /// all ChartGraphics3D class methods for 3D shapes. Only this 
    /// class should be used for any drawing in the chart.
	/// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public partial class ChartGraphics : ChartElement
	{
		#region Fields

		// Common Elements
		private CommonElements		_common;

		// Reusable objects
		private Pen                 _pen;
		private SolidBrush			_solidBrush;
		private Matrix				_myMatrix;
	
		// Private fields which represents picture size
		private int					_width;
		private int					_height;
		
		// Indicates that smoothing is applied while drawing shadows
		internal bool				softShadows = true;

		// Anti aliasing flags
		private	AntiAliasingStyles		_antiAliasing = AntiAliasingStyles.All;

		// True if rendering into the metafile
		internal bool				IsMetafile = false;

		#endregion

		#region Lines Methods

		/// <summary>
		/// Draws a line connecting the two specified points.
		/// </summary>
		/// <param name="color">Line color.</param>
		/// <param name="width">Line width.</param>
		/// <param name="style">Line style.</param>
		/// <param name="firstPointF">A Point that represents the first point to connect.</param>
		/// <param name="secondPointF">A Point that represents the second point to connect.</param>
		internal void DrawLineRel( 
			Color color, 
			int width, 
			ChartDashStyle style, 
			PointF firstPointF, 
			PointF secondPointF 
			)
		{
			DrawLineAbs(
				color, 
				width, 
				style, 
				GetAbsolutePoint(firstPointF), 
				GetAbsolutePoint(secondPointF) );
		}

		/// <summary>
		/// Draws a line connecting the two specified points using absolute coordinates.
		/// </summary>
		/// <param name="color">Line color.</param>
		/// <param name="width">Line width.</param>
		/// <param name="style">Line style.</param>
		/// <param name="firstPoint">A Point that represents the first point to connect.</param>
		/// <param name="secondPoint">A Point that represents the second point to connect.</param>
		internal void DrawLineAbs( 
			Color color, 
			int width, 
			ChartDashStyle style, 
			PointF firstPoint, 
			PointF secondPoint 
			)
		{
			// Do not draw line if width is 0 or style not set
			if( width == 0 || style == ChartDashStyle.NotSet )
			{
				return;
			}

			// Set a line color
			if(_pen.Color != color)
			{
				_pen.Color = color;
			}

			// Set a line width
			if(_pen.Width != width)
			{
				_pen.Width = width;
			}

			// Set a line style
			if(_pen.DashStyle != GetPenStyle( style ))
			{
				_pen.DashStyle = GetPenStyle( style );
			}

			// Remember SmoothingMode and turn off anti aliasing for 
			// vertical or horizontal lines usinig 1 pixel dashed pen.
			// This prevents anialiasing from completly smoothing the 
			// dashed line.
			SmoothingMode oldSmoothingMode = this.SmoothingMode;
			if(width <= 1 && style != ChartDashStyle.Solid)
			{
				if(firstPoint.X == secondPoint.X ||
					firstPoint.Y == secondPoint.Y)
				{
                    this.SmoothingMode = SmoothingMode.Default;
				}
			}

			// Draw a line
            this.DrawLine(_pen, 
				(float)Math.Round(firstPoint.X), 
				(float)Math.Round(firstPoint.Y), 
				(float)Math.Round(secondPoint.X), 
				(float)Math.Round(secondPoint.Y) );

			// Return old smoothing mode
            this.SmoothingMode = oldSmoothingMode;
		}

		/// <summary>
		/// Draws a line with shadow connecting the two specified points.
		/// </summary>
		/// <param name="color">Line color.</param>
		/// <param name="width">Line width.</param>
		/// <param name="style">Line style.</param>
		/// <param name="firstPoint">A Point that represents the first point to connect.</param>
		/// <param name="secondPoint">A Point that represents the second point to connect.</param>		
		/// <param name="shadowColor">Shadow Color.</param>
		/// <param name="shadowOffset">Shadow Offset.</param>
		internal void DrawLineRel(	
			Color color, 
			int width, 
			ChartDashStyle style, 
			PointF firstPoint, 
			PointF secondPoint, 
			Color shadowColor, 
			int shadowOffset  
			)
		{
			DrawLineAbs(	
				color, 
				width, 
				style, 
				GetAbsolutePoint(firstPoint), 
				GetAbsolutePoint(secondPoint), 
				shadowColor, 
				shadowOffset );
		}

		/// <summary>
		/// Draws a line with shadow connecting the two specified points.
		/// </summary>
		/// <param name="color">Line color.</param>
		/// <param name="width">Line width.</param>
		/// <param name="style">Line style.</param>
		/// <param name="firstPoint">A Point that represents the first point to connect.</param>
		/// <param name="secondPoint">A Point that represents the second point to connect.</param>		
		/// <param name="shadowColor">Shadow Color.</param>
		/// <param name="shadowOffset">Shadow Offset.</param>
		internal void DrawLineAbs(	
			Color color, 
			int width, 
			ChartDashStyle style, 
			PointF firstPoint, 
			PointF secondPoint, 
			Color shadowColor, 
			int shadowOffset  
			)
		{
			if(shadowOffset != 0)
			{
				// Shadow color
				Color shColor;

				// Make shadow semi transparent 
				// if alpha value not used
				if( shadowColor.A != 255 )
					shColor = shadowColor;
				else
					shColor = Color.FromArgb(color.A/2, shadowColor);

				// Set shadow line position
				PointF firstShadow = new PointF( firstPoint.X + shadowOffset, firstPoint.Y + shadowOffset);
				PointF secondShadow = new PointF( secondPoint.X + shadowOffset, secondPoint.Y + shadowOffset );

				// Draw Shadow of Line
				DrawLineAbs( shColor, width, style, firstShadow, secondShadow );
			}

			// Draw Line
			DrawLineAbs( color, width, style, firstPoint, secondPoint );
		}

		#endregion

		#region Pen and Brush Methods

		/// <summary>
		/// Creates a Hatch Brush.
		/// </summary>
		/// <param name="hatchStyle">Chart Hatch style.</param>
		/// <param name="backColor">Back Color.</param>
		/// <param name="foreColor">Fore Color.</param>
		/// <returns>Brush</returns>
        internal Brush GetHatchBrush( 
			ChartHatchStyle hatchStyle, 
			Color backColor, 
			Color foreColor 
			)
		{
			// Convert Chart Hatch Style enum 
			// to Hatch Style enum.
			HatchStyle hatch;
			hatch = (HatchStyle)Enum.Parse(typeof(HatchStyle),hatchStyle.ToString());
			
			// Create Hatch Brush
			return new HatchBrush( hatch, foreColor, backColor );
		}

		/// <summary>
		/// Creates a textured brush.
		/// </summary>
		/// <param name="name">Image file name or URL.</param>
		/// <param name="backImageTransparentColor">Image transparent color.</param>
		/// <param name="mode">Wrap mode.</param>
		/// <param name="backColor">Image background color.</param>
		/// <returns>Textured brush.</returns>
		internal Brush GetTextureBrush(
			string name, 
			Color backImageTransparentColor, 
			ChartImageWrapMode mode,
			Color backColor
			)
		{
			// Load a image
           System.Drawing.Image image = _common.ImageLoader.LoadImage( name );

			// Create a brush
			ImageAttributes attrib = new ImageAttributes();
			attrib.SetWrapMode((mode == ChartImageWrapMode.Unscaled) ? WrapMode.Clamp : ((WrapMode)mode));
			if(backImageTransparentColor != Color.Empty)
			{
				attrib.SetColorKey(backImageTransparentColor, backImageTransparentColor, ColorAdjustType.Default);
			}

			// If image is a metafile background must be filled first
			// Solves issue that background is not cleared correctly
			if(backImageTransparentColor == Color.Empty &&
				image is Metafile &&
				backColor != Color.Transparent)
			{
				TextureBrush backFilledBrush = null;
				Bitmap bitmap = new Bitmap(image.Width, image.Height);
				using(Graphics graphics = Graphics.FromImage(bitmap))
				{
					using(SolidBrush backBrush = new SolidBrush(backColor))
					{
						graphics.FillRectangle(backBrush, 0, 0, image.Width, image.Height);
						graphics.DrawImageUnscaled(image, 0, 0);
						backFilledBrush= new TextureBrush( bitmap, new RectangleF(0,0,image.Width,image.Height), attrib); 
					}
				}

				return backFilledBrush;
			}
                       
            
            TextureBrush textureBrush;

            if (ImageLoader.DoDpisMatch(image, this.Graphics))
                textureBrush = new TextureBrush(image, new RectangleF(0, 0, image.Width, image.Height), attrib);
            else  // if the image dpi does not match the graphics dpi we have to scale the image    
            {
                Image scaledImage = ImageLoader.GetScaledImage(image, this.Graphics);
                textureBrush = new TextureBrush(scaledImage, new RectangleF(0, 0, scaledImage.Width, scaledImage.Height), attrib);
                scaledImage.Dispose();
            }
            
            return textureBrush;

		}
                
		/// <summary>
		/// This method creates a gradient brush.
		/// </summary>
		/// <param name="rectangle">A rectangle which has to be filled with a gradient color.</param>
		/// <param name="firstColor">First color.</param>
		/// <param name="secondColor">Second color.</param>
		/// <param name="type ">Gradient type .</param>
		/// <returns>Gradient Brush</returns>
        internal Brush GetGradientBrush( 
			RectangleF rectangle, 
			Color firstColor, 
			Color secondColor, 
			GradientStyle type
			)
		{
			// Increse the brush rectangle by 1 pixel to ensure the fit
			rectangle.Inflate(1f, 1f);

			Brush gradientBrush = null;
			float angle = 0;

			// Function which create gradient brush fires exception if 
			// rectangle size is zero.
			if( rectangle.Height == 0 || rectangle.Width == 0 )
			{
				gradientBrush = new SolidBrush( Color.Black );
				return gradientBrush;
			}

			// *******************************************
			// Linear Gradient
			// *******************************************
			// Check linear type .
			if( type == GradientStyle.LeftRight || type == GradientStyle.VerticalCenter )
			{
				angle = 0;
			}
			else if( type == GradientStyle.TopBottom || type == GradientStyle.HorizontalCenter )
			{
				angle = 90;
			}
			else if(  type == GradientStyle.DiagonalLeft )
			{
				angle = (float)(Math.Atan(rectangle.Width / rectangle.Height)* 180 / Math.PI); 
			}
			else if(  type == GradientStyle.DiagonalRight )
			{
				angle = (float)(180 - Math.Atan(rectangle.Width / rectangle.Height)* 180 / Math.PI); 
			}
			
			// Create a linear gradient brush
			if( type == GradientStyle.TopBottom || type == GradientStyle.LeftRight 
				|| type == GradientStyle.DiagonalLeft || type == GradientStyle.DiagonalRight
				|| type == GradientStyle.HorizontalCenter || type == GradientStyle.VerticalCenter )
			{
				RectangleF tempRect = new RectangleF(rectangle.X,rectangle.Y,rectangle.Width,rectangle.Height);
				// For Horizontal and vertical center gradient types
				if( type == GradientStyle.HorizontalCenter )
				{
					// Resize and wrap gradient
					tempRect.Height = tempRect.Height / 2F;
                    LinearGradientBrush linearGradientBrush = new LinearGradientBrush(tempRect, firstColor, secondColor, angle);
                    gradientBrush = linearGradientBrush;
					linearGradientBrush.WrapMode = WrapMode.TileFlipX;
				}
				else if( type == GradientStyle.VerticalCenter )
				{
					// Resize and wrap gradient
					tempRect.Width = tempRect.Width / 2F;
                    LinearGradientBrush linearGradientBrush = new LinearGradientBrush(tempRect, firstColor, secondColor, angle);
                    gradientBrush = linearGradientBrush;
                    linearGradientBrush.WrapMode = WrapMode.TileFlipX;
				}
				else
				{
					gradientBrush = new LinearGradientBrush( rectangle, firstColor, secondColor, angle );
				}
				return gradientBrush;
			}

			// *******************************************
			// Gradient is not linear : From Center.
			// *******************************************
			
			// Create a path
			GraphicsPath path = new GraphicsPath();

			// Add a rectangle to the path
			path.AddRectangle( rectangle );

			// Create a gradient brush
            PathGradientBrush pathGradientBrush = new PathGradientBrush(path);
            gradientBrush = pathGradientBrush;

			// Set the center color
            pathGradientBrush.CenterColor = firstColor;

			// Set the Surround color
			Color[] colors = {secondColor};
            pathGradientBrush.SurroundColors = colors;
			
			if( path != null )
			{
				path.Dispose();
			}

			return gradientBrush;
		}

		/// <summary>
		/// This method creates a gradient brush for pie. This gradient is one 
		/// of the types used only with pie and doughnut.
		/// </summary>
		/// <param name="rectangle">A rectangle which has to be filled with a gradient color</param>
		/// <param name="firstColor">First color</param>
		/// <param name="secondColor">Second color</param>
		/// <returns>Gradient Brush</returns>
		internal Brush GetPieGradientBrush( 
			RectangleF rectangle, 
			Color firstColor, 
			Color secondColor 
			)
		{
			// Create a path that consists of a single ellipse.
			GraphicsPath path = new GraphicsPath();
			path.AddEllipse( rectangle );

			// Use the path to construct a brush.
			PathGradientBrush gradientBrush = new PathGradientBrush(path);

			// Set the color at the center of the path.
			gradientBrush.CenterColor = firstColor;

			// Set the color along the entire boundary 
			// of the path to aqua.
			Color[] colors = {secondColor};

			gradientBrush.SurroundColors = colors;

			if( path != null )
			{
				path.Dispose();
			}

			return gradientBrush;

		}

		/// <summary>
		/// Converts GDI+ line style to Chart Graph line style.
		/// </summary>
		/// <param name="style">Chart Line style.</param>
		/// <returns>GDI+ line style.</returns>
		internal DashStyle GetPenStyle( ChartDashStyle style )
		{
			// Convert to chart line styles. The custom style doesn’t exist.
			switch( style )
			{
				case ChartDashStyle.Dash:
					return DashStyle.Dash;
				case ChartDashStyle.DashDot:
					return DashStyle.DashDot;
				case ChartDashStyle.DashDotDot:
					return DashStyle.DashDotDot;
				case ChartDashStyle.Dot:
					return DashStyle.Dot;
			}

			return DashStyle.Solid;
		}

		#endregion

		#region Markers

		/// <summary>
		/// Creates polygon for multi-corner star marker.
		/// </summary>
		/// <param name="rect">Marker rectangle.</param>
		/// <param name="numberOfCorners">Number of corners (4 and up).</param>
		/// <returns>Array of points.</returns>
		internal PointF[] CreateStarPolygon(RectangleF rect, int numberOfCorners)
		{
            int numberOfCornersX2;
            checked
            {
                numberOfCornersX2 = numberOfCorners * 2;
            }

            bool outside = true;
            PointF[] points = new PointF[numberOfCornersX2];
            PointF[] tempPoints = new PointF[1];
            // overflow check
            for (int pointIndex = 0; pointIndex < numberOfCornersX2; pointIndex++)
			{
				tempPoints[0] = new PointF(rect.X + rect.Width/2f, (outside == true) ? rect.Y : rect.Y + rect.Height/4f);
				Matrix	matrix = new Matrix();
				matrix.RotateAt(pointIndex*(360f/(numberOfCorners*2f)), new PointF(rect.X + rect.Width/2f, rect.Y + rect.Height/2f));
				matrix.TransformPoints(tempPoints);
				points[pointIndex] = tempPoints[0];
				outside = !outside;
			}

			return points;
		}

		/// <summary>
		/// Draw marker using relative coordinates of the center.
		/// </summary>
		/// <param name="point">Coordinates of the center.</param>
		/// <param name="markerStyle">Marker style.</param>
		/// <param name="markerSize">Marker size.</param>
		/// <param name="markerColor">Marker color.</param>
		/// <param name="markerBorderColor">Marker border color.</param>
		/// <param name="markerBorderSize">Marker border size.</param>
		/// <param name="markerImage">Marker image name.</param>
		/// <param name="markerImageTransparentColor">Marker image transparent color.</param>
		/// <param name="shadowSize">Marker shadow size.</param>
		/// <param name="shadowColor">Marker shadow color.</param>
		/// <param name="imageScaleRect">Rectangle to which marker image should be scaled.</param>
		internal void DrawMarkerRel(
			PointF point, 
			MarkerStyle markerStyle, 
			int markerSize, 
			Color markerColor, 
			Color markerBorderColor, 
			int markerBorderSize, 
			string markerImage, 
			Color markerImageTransparentColor, 
			int shadowSize, 
			Color shadowColor, 
			RectangleF imageScaleRect
			)
		{
			DrawMarkerAbs(this.GetAbsolutePoint(point), markerStyle, markerSize, markerColor, markerBorderColor, markerBorderSize, markerImage, markerImageTransparentColor, shadowSize, shadowColor, imageScaleRect, false);
		}

		/// <summary>
		/// Draw marker using absolute coordinates of the center.
		/// </summary>
		/// <param name="point">Coordinates of the center.</param>
		/// <param name="markerStyle">Marker style.</param>
		/// <param name="markerSize">Marker size.</param>
		/// <param name="markerColor">Marker color.</param>
		/// <param name="markerBorderColor">Marker border color.</param>
		/// <param name="markerBorderSize">Marker border size.</param>
		/// <param name="markerImage">Marker image name.</param>
		/// <param name="markerImageTransparentColor">Marker image transparent color.</param>
		/// <param name="shadowSize">Marker shadow size.</param>
		/// <param name="shadowColor">Marker shadow color.</param>
		/// <param name="imageScaleRect">Rectangle to which marker image should be scaled.</param>
		/// <param name="forceAntiAlias">Always use anti aliasing when drawing the marker.</param>
		internal void DrawMarkerAbs(
			PointF point, 
			MarkerStyle markerStyle, 
			int markerSize, 
			Color markerColor, 
			Color markerBorderColor, 
			int markerBorderSize, 
			string markerImage, 
			Color markerImageTransparentColor, 
			int shadowSize, 
			Color shadowColor, 
			RectangleF imageScaleRect, 
			bool forceAntiAlias
			)
		{
			// Hide border when zero width specified
			if(markerBorderSize <= 0)
			{
				markerBorderColor = Color.Transparent;
			}

			// Draw image instead of standart markers
			if(markerImage.Length > 0)
			{
				// Get image
                System.Drawing.Image image = _common.ImageLoader.LoadImage( markerImage );

                if (image != null)
                {
                    // Calculate image rectangle
                    RectangleF rect = RectangleF.Empty;
                    if (imageScaleRect == RectangleF.Empty)
                    {
                        SizeF size = new SizeF();
                        ImageLoader.GetAdjustedImageSize(image, this.Graphics, ref size);
                        imageScaleRect.Width = size.Width;
                        imageScaleRect.Height = size.Height;
                    }

                    rect.X = point.X - imageScaleRect.Width / 2F;
                    rect.Y = point.Y - imageScaleRect.Height / 2F;
                    rect.Width = imageScaleRect.Width;
                    rect.Height = imageScaleRect.Height;

                    // Prepare image properties (transparent color)
                    ImageAttributes attrib = new ImageAttributes();
                    if (markerImageTransparentColor != Color.Empty)
                    {
                        attrib.SetColorKey(markerImageTransparentColor, markerImageTransparentColor, ColorAdjustType.Default);
                    }

                    // Draw image shadow
                    if (shadowSize != 0 && shadowColor != Color.Empty)
                    {
                        ImageAttributes attribShadow = new ImageAttributes();
                        attribShadow.SetColorKey(markerImageTransparentColor, markerImageTransparentColor, ColorAdjustType.Default);
                        ColorMatrix colorMatrix = new ColorMatrix();
                        colorMatrix.Matrix00 = 0.25f; // Red
                        colorMatrix.Matrix11 = 0.25f; // Green
                        colorMatrix.Matrix22 = 0.25f; // Blue
                        colorMatrix.Matrix33 = 0.5f; // alpha
                        colorMatrix.Matrix44 = 1.0f; // w
                        attribShadow.SetColorMatrix(colorMatrix);

                        this.DrawImage(image,
                            new Rectangle((int)rect.X + shadowSize, (int)rect.Y + shadowSize, (int)rect.Width, (int)rect.Height),
                            0, 0, image.Width, image.Height,
                            GraphicsUnit.Pixel,
                            attribShadow);
                    }

                    // Draw image
                    this.DrawImage(image,
                        new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height),
                        0, 0, image.Width, image.Height,
                        GraphicsUnit.Pixel,
                        attrib);
                }
			}

			// Draw standart marker using style, size and color
			else if(markerStyle != MarkerStyle.None && markerSize > 0 && markerColor != Color.Empty)
			{
				// Enable antialising
                SmoothingMode oldSmoothingMode = this.SmoothingMode;
				if(forceAntiAlias)
				{
                    this.SmoothingMode = SmoothingMode.AntiAlias;
				}
				
				// Create solid color brush
                using (SolidBrush brush = new SolidBrush(markerColor))
                {
                    // Calculate marker rectangle
                    RectangleF rect = RectangleF.Empty;
                    rect.X = point.X - ((float)markerSize) / 2F;
                    rect.Y = point.Y - ((float)markerSize) / 2F;
                    rect.Width = markerSize;
                    rect.Height = markerSize;

                    // Draw marker depending on style
                    switch (markerStyle)
                    {
                        case (MarkerStyle.Star4):
                        case (MarkerStyle.Star5):
                        case (MarkerStyle.Star6):
                        case (MarkerStyle.Star10):
                            {
                                // Set number of corners
                                int cornerNumber = 4;
                                if (markerStyle == MarkerStyle.Star5)
                                {
                                    cornerNumber = 5;
                                }
                                else if (markerStyle == MarkerStyle.Star6)
                                {
                                    cornerNumber = 6;
                                }
                                else if (markerStyle == MarkerStyle.Star10)
                                {
                                    cornerNumber = 10;
                                }

                                // Get star polygon
                                PointF[] points = CreateStarPolygon(rect, cornerNumber);

                                // Draw shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                {
                                    Matrix translateMatrix = this.Transform.Clone();
                                    translateMatrix.Translate(shadowSize, shadowSize);
                                    Matrix oldMatrix = this.Transform;
                                    this.Transform = translateMatrix;

                                    this.FillPolygon(new SolidBrush((shadowColor.A != 255) ? shadowColor : Color.FromArgb(markerColor.A / 2, shadowColor)), points);

                                    this.Transform = oldMatrix;
                                }

                                // Draw star
                                this.FillPolygon(brush, points);
                                this.DrawPolygon(new Pen(markerBorderColor, markerBorderSize), points);
                                break;
                            }
                        case (MarkerStyle.Circle):
                            {
                                // Draw marker shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                {
                                    if (!softShadows)
                                    {
                                        using (SolidBrush shadowBrush = new SolidBrush((shadowColor.A != 255) ? shadowColor : Color.FromArgb(markerColor.A / 2, shadowColor)))
                                        {
                                            RectangleF shadowRect = rect;
                                            shadowRect.X += shadowSize;
                                            shadowRect.Y += shadowSize;
                                            this.FillEllipse(shadowBrush, shadowRect);
                                        }
                                    }
                                    else
                                    {
                                        // Add circle to the graphics path
                                        using (GraphicsPath path = new GraphicsPath())
                                        {
                                            path.AddEllipse(rect.X + shadowSize - 1, rect.Y + shadowSize - 1, rect.Width + 2, rect.Height + 2);

                                            // Create path brush
                                            using (PathGradientBrush shadowBrush = new PathGradientBrush(path))
                                            {
                                                shadowBrush.CenterColor = shadowColor;

                                                // Set the color along the entire boundary of the path
                                                Color[] colors = { Color.Transparent };
                                                shadowBrush.SurroundColors = colors;
                                                shadowBrush.CenterPoint = new PointF(point.X, point.Y);

                                                // Define brush focus scale
                                                PointF focusScale = new PointF(1 - 2f * shadowSize / rect.Width, 1 - 2f * shadowSize / rect.Height);
                                                if (focusScale.X < 0)
                                                {
                                                    focusScale.X = 0;
                                                }
                                                if (focusScale.Y < 0)
                                                {
                                                    focusScale.Y = 0;
                                                }
                                                shadowBrush.FocusScales = focusScale;

                                                // Draw shadow
                                                this.FillPath(shadowBrush, path);
                                            }
                                        }
                                    }
                                }

                                this.FillEllipse(brush, rect);
                                this.DrawEllipse(new Pen(markerBorderColor, markerBorderSize), rect);
                                break;
                            }
                        case (MarkerStyle.Square):
                            {
                                // Draw marker shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                {
                                    FillRectangleShadowAbs(rect, shadowColor, shadowSize, shadowColor);
                                }

                                this.FillRectangle(brush, rect);
                                this.DrawRectangle(new Pen(markerBorderColor, markerBorderSize), (int)Math.Round(rect.X, 0), (int)Math.Round(rect.Y, 0), (int)Math.Round(rect.Width, 0), (int)Math.Round(rect.Height, 0));
                                break;
                            }
                        case (MarkerStyle.Cross):
                            {
                                // Calculate cross line width and size
                                float crossLineWidth = (float)Math.Ceiling(markerSize / 4F);
                                float crossSize = markerSize;// * (float)Math.Sin(45f/180f*Math.PI);

                                // Calculate cross coordinates
                                PointF[] points = new PointF[12];
                                points[0].X = point.X - crossSize / 2F;
                                points[0].Y = point.Y + crossLineWidth / 2F;
                                points[1].X = point.X - crossSize / 2F;
                                points[1].Y = point.Y - crossLineWidth / 2F;

                                points[2].X = point.X - crossLineWidth / 2F;
                                points[2].Y = point.Y - crossLineWidth / 2F;
                                points[3].X = point.X - crossLineWidth / 2F;
                                points[3].Y = point.Y - crossSize / 2F;
                                points[4].X = point.X + crossLineWidth / 2F;
                                points[4].Y = point.Y - crossSize / 2F;

                                points[5].X = point.X + crossLineWidth / 2F;
                                points[5].Y = point.Y - crossLineWidth / 2F;
                                points[6].X = point.X + crossSize / 2F;
                                points[6].Y = point.Y - crossLineWidth / 2F;
                                points[7].X = point.X + crossSize / 2F;
                                points[7].Y = point.Y + crossLineWidth / 2F;

                                points[8].X = point.X + crossLineWidth / 2F;
                                points[8].Y = point.Y + crossLineWidth / 2F;
                                points[9].X = point.X + crossLineWidth / 2F;
                                points[9].Y = point.Y + crossSize / 2F;
                                points[10].X = point.X - crossLineWidth / 2F;
                                points[10].Y = point.Y + crossSize / 2F;
                                points[11].X = point.X - crossLineWidth / 2F;
                                points[11].Y = point.Y + crossLineWidth / 2F;

                                // Rotate cross coordinates 45 degrees
                                Matrix rotationMatrix = new Matrix();
                                rotationMatrix.RotateAt(45, point);
                                rotationMatrix.TransformPoints(points);

                                // Draw shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                {
                                    // Create translation matrix
                                    Matrix translateMatrix = this.Transform.Clone();
                                    translateMatrix.Translate(
                                        (softShadows) ? shadowSize + 1 : shadowSize,
                                        (softShadows) ? shadowSize + 1 : shadowSize);
                                    Matrix oldMatrix = this.Transform;
                                    this.Transform = translateMatrix;

                                    if (!softShadows)
                                    {
                                        using (Brush softShadowBrush = new SolidBrush((shadowColor.A != 255) ? shadowColor : Color.FromArgb(markerColor.A / 2, shadowColor)))
                                        {
                                            this.FillPolygon(softShadowBrush, points);
                                        }
                                    }
                                    else
                                    {
                                        // Add polygon to the graphics path
                                        using (GraphicsPath path = new GraphicsPath())
                                        {
                                            path.AddPolygon(points);

                                            // Create path brush
                                            using (PathGradientBrush shadowBrush = new PathGradientBrush(path))
                                            {
                                                shadowBrush.CenterColor = shadowColor;

                                                // Set the color along the entire boundary of the path
                                                Color[] colors = { Color.Transparent };
                                                shadowBrush.SurroundColors = colors;
                                                shadowBrush.CenterPoint = new PointF(point.X, point.Y);

                                                // Define brush focus scale
                                                PointF focusScale = new PointF(1 - 2f * shadowSize / rect.Width, 1 - 2f * shadowSize / rect.Height);
                                                if (focusScale.X < 0)
                                                {
                                                    focusScale.X = 0;
                                                }
                                                if (focusScale.Y < 0)
                                                {
                                                    focusScale.Y = 0;
                                                }
                                                shadowBrush.FocusScales = focusScale;

                                                // Draw shadow
                                                this.FillPath(shadowBrush, path);
                                            }
                                        }
                                    }

                                    this.Transform = oldMatrix;
                                }

                                // Create translation matrix
                                Matrix translateMatrixShape = this.Transform.Clone();
                                Matrix oldMatrixShape = this.Transform;
                                this.Transform = translateMatrixShape;

                                this.FillPolygon(brush, points);
                                this.DrawPolygon(new Pen(markerBorderColor, markerBorderSize), points);

                                this.Transform = oldMatrixShape;

                                break;
                            }
                        case (MarkerStyle.Diamond):
                            {
                                PointF[] points = new PointF[4];
                                points[0].X = rect.X;
                                points[0].Y = rect.Y + rect.Height / 2F;
                                points[1].X = rect.X + rect.Width / 2F;
                                points[1].Y = rect.Top;
                                points[2].X = rect.Right;
                                points[2].Y = rect.Y + rect.Height / 2F;
                                points[3].X = rect.X + rect.Width / 2F;
                                points[3].Y = rect.Bottom;

                                // Draw shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                {
                                    Matrix translateMatrix = this.Transform.Clone();
                                    translateMatrix.Translate((softShadows) ? 0 : shadowSize,
                                        (softShadows) ? 0 : shadowSize);
                                    Matrix oldMatrix = this.Transform;
                                    this.Transform = translateMatrix;

                                    if (!softShadows)
                                    {
                                        using (Brush softShadowBrush = new SolidBrush((shadowColor.A != 255) ? shadowColor : Color.FromArgb(markerColor.A / 2, shadowColor)))
                                        {
                                            this.FillPolygon(softShadowBrush, points);
                                        }
                                    }
                                    else
                                    {
                                        // Calculate diamond size
                                        float diamondSize = markerSize * (float)Math.Sin(45f / 180f * Math.PI);

                                        // Calculate diamond rectangle position
                                        RectangleF diamondRect = RectangleF.Empty;
                                        diamondRect.X = point.X - ((float)diamondSize) / 2F;
                                        diamondRect.Y = point.Y - ((float)diamondSize) / 2F - shadowSize;
                                        diamondRect.Width = diamondSize;
                                        diamondRect.Height = diamondSize;

                                        // Set rotation matrix to 45 
                                        translateMatrix.RotateAt(45, point);
                                        this.Transform = translateMatrix;

                                        FillRectangleShadowAbs(diamondRect, shadowColor, shadowSize, shadowColor);
                                    }


                                    this.Transform = oldMatrix;
                                }

                                this.FillPolygon(brush, points);
                                this.DrawPolygon(new Pen(markerBorderColor, markerBorderSize), points);
                                break;
                            }
                        case (MarkerStyle.Triangle):
                            {
                                PointF[] points = new PointF[3];
                                points[0].X = rect.X;
                                points[0].Y = rect.Bottom;
                                points[1].X = rect.X + rect.Width / 2F;
                                points[1].Y = rect.Top;
                                points[2].X = rect.Right;
                                points[2].Y = rect.Bottom;

                                // Draw image shadow
                                if (shadowSize != 0 && shadowColor != Color.Empty)
                                {
                                    Matrix translateMatrix = this.Transform.Clone();
                                    translateMatrix.Translate((softShadows) ? shadowSize - 1 : shadowSize,
                                        (softShadows) ? shadowSize + 1 : shadowSize);
                                    Matrix oldMatrix = this.Transform;
                                    this.Transform = translateMatrix;

                                    if (!softShadows)
                                    {
                                        using (Brush softShadowBrush = new SolidBrush((shadowColor.A != 255) ? shadowColor : Color.FromArgb(markerColor.A / 2, shadowColor)))
                                        {
                                            this.FillPolygon(softShadowBrush, points);
                                        }
                                    }
                                    else
                                    {
                                        // Add polygon to the graphics path
                                        GraphicsPath path = new GraphicsPath();
                                        path.AddPolygon(points);

                                        // Create path brush
                                        PathGradientBrush shadowBrush = new PathGradientBrush(path);
                                        shadowBrush.CenterColor = shadowColor;

                                        // Set the color along the entire boundary of the path
                                        Color[] colors = { Color.Transparent };
                                        shadowBrush.SurroundColors = colors;
                                        shadowBrush.CenterPoint = new PointF(point.X, point.Y);

                                        // Define brush focus scale
                                        PointF focusScale = new PointF(1 - 2f * shadowSize / rect.Width, 1 - 2f * shadowSize / rect.Height);
                                        if (focusScale.X < 0)
                                        {
                                            focusScale.X = 0;
                                        }
                                        if (focusScale.Y < 0)
                                        {
                                            focusScale.Y = 0;
                                        }
                                        shadowBrush.FocusScales = focusScale;

                                        // Draw shadow
                                        this.FillPath(shadowBrush, path);
                                    }

                                    this.Transform = oldMatrix;
                                }

                                this.FillPolygon(brush, points);
                                this.DrawPolygon(new Pen(markerBorderColor, markerBorderSize), points);
                                break;
                            }
                        default:
                            {
                                throw (new InvalidOperationException(SR.ExceptionGraphicsMarkerStyleUnknown));
                            }
                    }
                }

				// Restore SmoothingMode
				if(forceAntiAlias)
				{
					this.SmoothingMode = oldSmoothingMode;
				}
			}
		}

		#endregion
	
		#region String Methods

        /// <summary>
        /// Measures the specified string when drawn with the specified 
        /// Font object and formatted with the specified StringFormat object.
        /// </summary>
        /// <param name="text">String to measure.</param>
        /// <param name="font">Font object defines the text format of the string.</param>
        /// <param name="layoutArea">SizeF structure that specifies the maximum layout area for the text.</param>
        /// <param name="stringFormat">StringFormat object that represents formatting information, such as line spacing, for the string.</param>
        /// <param name="textOrientation">Text orientation.</param>
        /// <returns>This method returns a SizeF structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
        internal SizeF MeasureString(
            string text,
            Font font,
            SizeF layoutArea,
            StringFormat stringFormat,
            TextOrientation textOrientation
            )
        {
            // Current implementation of the stacked text will simply insert a new
            // line character between all characters in the original string. This
            // apporach will not allow to show multiple lines of stacked text or 
            // correctly handle text wrapping. 
            if (textOrientation == TextOrientation.Stacked)
            {
                text = GetStackedText(text);
            }
            return this.MeasureString(text, font, layoutArea, stringFormat);
        }

        /// <summary>
        /// Measures the specified text string when drawn with 
        /// the specified Font object and formatted with the 
        /// specified StringFormat object.
        /// </summary>
        /// <param name="text">The string to measure</param>
        /// <param name="font">The Font object used to determine the size of the text string. </param>
        /// <param name="layoutArea">A SizeF structure that specifies the layout rectangle for the text. </param>
        /// <param name="stringFormat">A StringFormat object that represents formatting information, such as line spacing, for the text string. </param>
        /// <param name="textOrientation">Text orientation.</param>
        /// <returns>A SizeF structure that represents the size of text as drawn with font.</returns>
        internal SizeF MeasureStringRel(
            string text, 
            Font font, 
            SizeF layoutArea, 
            StringFormat stringFormat,
            TextOrientation textOrientation)
        {
            // Current implementation of the stacked text will simply insert a new
            // line character between all characters in the original string. This
            // apporach will not allow to show multiple lines of stacked text or 
            // correctly handle text wrapping. 
            if (textOrientation == TextOrientation.Stacked)
            {
                text = GetStackedText(text);
            }
            return this.MeasureStringRel(text, font, layoutArea, stringFormat);
        }

		/// <summary>
		/// Draws the specified text string at the specified location with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
		/// </summary>
		/// <param name="text">String to draw.</param>
		/// <param name="font">Font object that defines the text format of the string.</param>
		/// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
        /// <param name="rect">Position of the drawn text in pixels.</param>
		/// <param name="format">StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
        /// <param name="textOrientation">Text orientation.</param>
        internal void DrawString(
            string text,
            Font font,
            Brush brush,
            RectangleF rect,
            StringFormat format,
            TextOrientation textOrientation
            )
        {
            // Current implementation of the stacked text will simply insert a new
            // line character between all characters in the original string. This
            // apporach will not allow to show multiple lines of stacked text or 
            // correctly handle text wrapping. 
            if (textOrientation == TextOrientation.Stacked)
            {
                text = GetStackedText(text);
            }
            this.DrawString(text, font, brush, rect, format);
        }

        /// <summary>
        /// Draw a string.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="font">Text Font.</param>
        /// <param name="brush">Text Brush.</param>
        /// <param name="position">Text Position.</param>
        /// <param name="format">Format and text alignment.</param>
        /// <param name="angle">Text angle.</param>
        /// <param name="textOrientation">Text orientation.</param>
        internal void DrawStringRel(
            string text,
            System.Drawing.Font font,
            System.Drawing.Brush brush,
            PointF position,
            System.Drawing.StringFormat format,
            int angle,
            TextOrientation textOrientation
            )
        {
            // Current implementation of the stacked text will simply insert a new
            // line character between all characters in the original string. This
            // apporach will not allow to show multiple lines of stacked text or 
            // correctly handle text wrapping. 
            if (textOrientation == TextOrientation.Stacked)
            {
                text = GetStackedText(text);
            }

            this.DrawStringRel(text, font, brush, position, format, angle);
        }

        /// <summary>
        /// Draw a string.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="font">Text Font.</param>
        /// <param name="brush">Text Brush.</param>
        /// <param name="position">Text Position.</param>
        /// <param name="format">Format and text alignment.</param>
        /// <param name="textOrientation">Text orientation.</param>
        internal void DrawStringRel(
            string text,
            System.Drawing.Font font,
            System.Drawing.Brush brush,
            RectangleF position,
            System.Drawing.StringFormat format,
            TextOrientation textOrientation
            )
        {
            // Current implementation of the stacked text will simply insert a new
            // line character between all characters in the original string. This
            // apporach will not allow to show multiple lines of stacked text or 
            // correctly handle text wrapping. 
            if (textOrientation == TextOrientation.Stacked)
            {
                text = GetStackedText(text);
            }

            this.DrawStringRel(text, font, brush, position, format);
        }

        /// <summary>
        /// Function returned stacked text by inserting new line characters between
        /// all characters in the original string.
        /// </summary>
        /// <param name="text">Original text.</param>
        /// <returns>Stacked text.</returns>
        internal static string GetStackedText(string text)
        {
            string result = string.Empty;
            foreach (char ch in text)
            {
                result += ch;
                if (ch != '\n')
                {
                    result += '\n';
                }
            }
            return result;
        }

		/// <summary>
		/// Draw a string and fills it's background
		/// </summary>
		/// <param name="common">The Common elements object.</param>
		/// <param name="text">Text.</param>
		/// <param name="font">Text Font.</param>
		/// <param name="brush">Text Brush.</param>
		/// <param name="position">Text Position.</param>
		/// <param name="format">Format and text alignment.</param>
		/// <param name="angle">Text angle.</param>
		/// <param name="backPosition">Text background position.</param>
		/// <param name="backColor">Back Color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		/// <param name="series">Series</param>
		/// <param name="point">Point</param>
		/// <param name="pointIndex">Point index in series</param>
		internal void DrawPointLabelStringRel( 
			CommonElements common,
			string text, 
			System.Drawing.Font font, 
			System.Drawing.Brush brush, 
			RectangleF position, 
			System.Drawing.StringFormat format, 
			int angle,
			RectangleF backPosition,
			Color backColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle,
			Series series,
			DataPoint point,
			int pointIndex)
		{
			// Start Svg/Flash Selection mode
			this.StartHotRegion( point, true );

			// Draw background
			DrawPointLabelBackground( 
				common,
				angle,
				PointF.Empty,
				backPosition,
				backColor, 
				borderColor, 
				borderWidth, 
				borderDashStyle,
				series,
				point,
				pointIndex);

			// End Svg/Flash Selection mode
			this.EndHotRegion( );
            
            point._lastLabelText = text;
            // Draw text
            if (IsRightToLeft)
            {
                // datapoint label alignments should appear as not RTL.
                using (StringFormat fmt = (StringFormat)format.Clone())
                {
                    if (fmt.Alignment == StringAlignment.Far)
                    {
                        fmt.Alignment = StringAlignment.Near;
                    }
                    else if (fmt.Alignment == StringAlignment.Near)
                    {
                        fmt.Alignment = StringAlignment.Far;
                    }
                    DrawStringRel(text,font,brush,position,fmt,angle);
                }
            }
            else
                DrawStringRel(text, font, brush, position, format, angle);
		}

		/// <summary>
		/// Draw a string and fills it's background
		/// </summary>
		/// <param name="common">The Common elements object.</param>
		/// <param name="text">Text.</param>
		/// <param name="font">Text Font.</param>
		/// <param name="brush">Text Brush.</param>
		/// <param name="position">Text Position.</param>
		/// <param name="format">Format and text alignment.</param>
		/// <param name="angle">Text angle.</param>
		/// <param name="backPosition">Text background position.</param>
		/// <param name="backColor">Back Color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		/// <param name="series">Series</param>
		/// <param name="point">Point</param>
		/// <param name="pointIndex">Point index in series</param>
		internal void DrawPointLabelStringRel( 
			CommonElements common,
			string text, 
			System.Drawing.Font font, 
			System.Drawing.Brush brush, 
			PointF position, 
			System.Drawing.StringFormat format, 
			int angle,
			RectangleF backPosition,
			Color backColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle,
			Series series,
			DataPoint point,
			int pointIndex)
		{
			// Start Svg/Flash Selection mode
			this.StartHotRegion( point, true );

			// Draw background
			DrawPointLabelBackground( 
				common,
				angle,
				position, 
				backPosition,
				backColor, 
				borderColor, 
				borderWidth, 
				borderDashStyle,
				series,
				point,
				pointIndex);

			// End Svg/Flash Selection mode
			this.EndHotRegion( );

            point._lastLabelText = text;
            // Draw text
            if (IsRightToLeft)
            {
                // datapoint label alignments should appear as not RTL
                using (StringFormat fmt = (StringFormat)format.Clone())
                {
                    if (fmt.Alignment == StringAlignment.Far)
                    {
                        fmt.Alignment = StringAlignment.Near;
                    }
                    else if (fmt.Alignment == StringAlignment.Near)
                    {
                        fmt.Alignment = StringAlignment.Far;
                    }
                    DrawStringRel(text,font,brush,position,fmt,angle);
                }
            }
            else
                DrawStringRel(text,font,brush,position,format,angle);
		}

		/// <summary>
		/// Draw a string and fills it's background
		/// </summary>
		/// <param name="common">The Common elements object.</param>
		/// <param name="angle">Text angle.</param>
		/// <param name="textPosition">Text position.</param>
		/// <param name="backPosition">Text background position.</param>
		/// <param name="backColor">Back Color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		/// <param name="series">Series</param>
		/// <param name="point">Point</param>
		/// <param name="pointIndex">Point index in series</param>
		private void DrawPointLabelBackground( 
			CommonElements common,
			int angle,
			PointF textPosition,
			RectangleF backPosition,
			Color backColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle,
			Series series,
			DataPoint point,
			int pointIndex)
		{
			// Draw background
			if(!backPosition.IsEmpty)
			{
				RectangleF backPositionAbs = this.Round(this.GetAbsoluteRectangle(backPosition));

				// Get rotation point
				PointF rotationPoint = PointF.Empty;
				if(textPosition.IsEmpty)
				{
					rotationPoint = new PointF(backPositionAbs.X + backPositionAbs.Width/2f, backPositionAbs.Y + backPositionAbs.Height/2f);
				}
				else
				{
					rotationPoint = this.GetAbsolutePoint(textPosition);
				}

				// Create a matrix and rotate it.
				_myMatrix = this.Transform.Clone();
				_myMatrix.RotateAt( angle, rotationPoint );

				// Save old state
				GraphicsState graphicsState = this.Save();

				// Set transformatino
				this.Transform = _myMatrix;

                // Check for empty colors
				if( !backColor.IsEmpty ||
					!borderColor.IsEmpty)
				{
					// Fill box around the label
					using(Brush	brush = new SolidBrush(backColor))
					{
						this.FillRectangle(brush, backPositionAbs);
					}

                    // deliant: Fix VSTS #156433	(2)	Data Label Border in core always shows when the style is set to NotSet	
                    // Draw box border
					if(  borderWidth > 0 &&
                        !borderColor.IsEmpty && borderDashStyle != ChartDashStyle.NotSet)
					{
                        AntiAliasingStyles saveAntiAliasing = this.AntiAliasing;
                        try
                        {
                            this.AntiAliasing = AntiAliasingStyles.None;						
                            using(Pen pen = new Pen(borderColor, borderWidth))
						    {
							    pen.DashStyle = GetPenStyle( borderDashStyle );
							    this.DrawRectangle(
								    pen, 
								    backPositionAbs.X, 
								    backPositionAbs.Y, 
								    backPositionAbs.Width, 
								    backPositionAbs.Height);
						    }
                        }
                        finally
                        {
                            this.AntiAliasing = saveAntiAliasing;
                        }
					}
				}
				else
				{
					// Draw invisible rectangle to handle tooltips
					using(Brush	brush = new SolidBrush(Color.Transparent))
					{
						this.FillRectangle(brush, backPositionAbs);
					}
				}
			

				// Restore old state
				this.Restore(graphicsState);

				// Add point label hot region
				if( common != null &&
					common.ProcessModeRegions)
				{
#if !Microsoft_CONTROL
					// Remember all point attributes
                    string oldToolTip = point.IsCustomPropertySet( CommonCustomProperties.ToolTip) ?  point.ToolTip : null;
					string oldUrl = point.IsCustomPropertySet( CommonCustomProperties.Url) ?  point.Url : null;
					string oldMapAreaAttributes = point.IsCustomPropertySet( CommonCustomProperties.MapAreaAttributes) ?  point.MapAreaAttributes : null;
                    string oldPostback = point.IsCustomPropertySet( CommonCustomProperties.PostBackValue) ?  point.PostBackValue : null;
                    object oldTag = point.Tag;
                    // Set label attributes into the point attribute.
					// Workaround for the AddHotRegion method limitation.
					point.ToolTip = point.LabelToolTip;
					point.Url = point.LabelUrl;
					point.MapAreaAttributes = point.LabelMapAreaAttributes;
                    point.PostBackValue = point.PostBackValue;
#endif // !Microsoft_CONTROL

                    // Insert area
					if(angle == 0)
					{
						common.HotRegionsList.AddHotRegion( 
							backPosition,
							point,
							series.Name,
							pointIndex );
					}
					else
					{
						// Convert rectangle to the graphics path and apply rotation transformation
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            path.AddRectangle(backPositionAbs);
                            path.Transform(_myMatrix);

                            // Add hot region
                            common.HotRegionsList.AddHotRegion(
                                path,
                                false,
                                this,
                                point,
                                series.Name,
                                pointIndex);
                        }
					}

#if !Microsoft_CONTROL
					// Restore all point attributes
                    if (oldToolTip != null) point.ToolTip = oldToolTip; else point.ResetToolTip();
                    if (oldUrl != null) point.Url = oldUrl; else point.ResetUrl();
                    if (oldMapAreaAttributes != null) point.MapAreaAttributes = oldMapAreaAttributes; else point.ResetMapAreaAttributes();
                    if (oldPostback != null) point.PostBackValue = oldPostback; else point.ResetPostBackValue();
                    point.Tag = oldTag;
#endif // !Microsoft_CONTROL

					// Set new hot region element type 
                    if (common.HotRegionsList.List != null && common.HotRegionsList.List.Count > 0)
					{
						((HotRegion)common.HotRegionsList.List[common.HotRegionsList.List.Count - 1]).Type = 
							ChartElementType.DataPointLabel;
					}
				}
			}
		}

		/// <summary>
		/// Draw a string.
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="font">Text Font.</param>
		/// <param name="brush">Text Brush.</param>
		/// <param name="position">Text Position.</param>
		/// <param name="format">Format and text alignment.</param>
		/// <param name="angle">Text angle.</param>
		internal void DrawStringRel( 
			string text, 
			System.Drawing.Font font, 
			System.Drawing.Brush brush, 
			PointF position, 
			System.Drawing.StringFormat format, 
			int angle 
			)
		{
			DrawStringAbs( 
				text, 
				font, 
				brush, 
				GetAbsolutePoint(position), 
				format, 
				angle);
		}

		/// <summary>
		/// Draw a string.
		/// </summary>
		/// <param name="text">Text.</param>
		/// <param name="font">Text Font.</param>
		/// <param name="brush">Text Brush.</param>
		/// <param name="absPosition">Text Position.</param>
		/// <param name="format">Format and text alignment.</param>
		/// <param name="angle">Text angle.</param>
		internal void DrawStringAbs( 
			string text, 
			System.Drawing.Font font, 
			System.Drawing.Brush brush, 
			PointF absPosition, 
			System.Drawing.StringFormat format, 
			int angle 
			)
		{
			// Create a matrix and rotate it.
			_myMatrix = this.Transform.Clone();
			_myMatrix.RotateAt(angle, absPosition);
    
			// Save aold state
			GraphicsState graphicsState = this.Save();

			// Set Angle
			this.Transform = _myMatrix;

			// Draw text with anti-aliasing
			/*
			if( (AntiAliasing & AntiAliasing.Text) == AntiAliasing.Text )
				this.TextRenderingHint = TextRenderingHint.AntiAlias;
			else
				this.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
			*/

			// Draw a string
			this.DrawString( text, font, brush, absPosition , format );

			// Restore old state
			this.Restore(graphicsState);
		}

        /// <summary>
        /// This method is used by the axis title hot region generation code. 
        /// It transforms the centered rectangle the same way as the Axis title text.
        /// </summary>
        /// <param name="center">Title center</param>
        /// <param name="size">Title text size</param>
        /// <param name="angle">Title rotation angle</param>
        /// <returns></returns>
        internal GraphicsPath GetTranformedTextRectPath(PointF center, SizeF size, int angle)
        {
            // Text hot area is 10px greater than the size of text
            size.Width += 10; 
            size.Height += 10;
            
            // Get the absolute center and create the centered rectangle points
            PointF absCenter = GetAbsolutePoint(center);            
            PointF[] points = new PointF[] {
                new PointF(absCenter.X - size.Width / 2f, absCenter.Y - size.Height / 2f), 
                new PointF(absCenter.X + size.Width / 2f, absCenter.Y - size.Height / 2f), 
                new PointF(absCenter.X + size.Width / 2f, absCenter.Y + size.Height / 2f), 
                new PointF(absCenter.X - size.Width / 2f, absCenter.Y + size.Height / 2f)};

            //Prepare the same tranformation matrix as used for the axis title
            Matrix matrix = this.Transform.Clone();
            matrix.RotateAt(angle, absCenter);
            //Tranform the rectangle points
            matrix.TransformPoints(points);

            //Return the path consisting of the rect points
            GraphicsPath path = new GraphicsPath();
            path.AddLines(points);
            path.CloseAllFigures();
            return path;
        }




		/// <summary>
		/// Draw label string.
		/// </summary>
		/// <param name="axis">Label axis.</param>
		/// <param name="labelRowIndex">Label text row index (0-10).</param>
		/// <param name="labelMark">Second row labels mark style.</param>
		/// <param name="markColor">Label mark line color.</param>
		/// <param name="text">Label text.</param>
		/// <param name="image">Label image name.</param>
		/// <param name="imageTransparentColor">Label image transparent color.</param>
		/// <param name="font">Text bont.</param>
		/// <param name="brush">Text brush.</param>
		/// <param name="position">Text position rectangle.</param>
		/// <param name="format">Label text format.</param>
		/// <param name="angle">Label text angle.</param>
		/// <param name="boundaryRect">Specifies the rectangle where the label text MUST be fitted.</param>
		/// <param name="label">Custom Label Item</param>
		/// <param name="truncatedLeft">Label is truncated on the left.</param>
		/// <param name="truncatedRight">Label is truncated on the right.</param>
		internal void DrawLabelStringRel( 
			Axis axis, 
			int labelRowIndex, 
			LabelMarkStyle labelMark, 
			Color markColor,
			string text, 
			string image,
			Color imageTransparentColor,
			System.Drawing.Font font, 
			System.Drawing.Brush brush, 
			RectangleF position, 
			System.Drawing.StringFormat format, 
			int angle, 
			RectangleF boundaryRect,
			CustomLabel label,
			bool truncatedLeft,
			bool truncatedRight)
		{
			Matrix oldTransform;
            using (StringFormat drawingFormat = (StringFormat)format.Clone())
            {
                SizeF labelSize = SizeF.Empty;

                // Check that rectangle is not empty
                if (position.Width == 0 || position.Height == 0)
                {
                    return;
                }

                // Find absolute position
                RectangleF absPosition = this.GetAbsoluteRectangle(position);

                // Make sure the rectangle is not empty
                if (absPosition.Width < 1f)
                {
                    absPosition.Width = 1f;
                }
                if (absPosition.Height < 1f)
                {
                    absPosition.Height = 1f;
                }

#if DEBUG
                // TESTING CODE: Shows labels rectangle position.
                //			Rectangle rr = Rectangle.Round(absPosition);
                //			rr.Width = (int)Math.Round(absPosition.Right) - rr.X;
                //			rr.Height = (int)Math.Round(absPosition.Bottom) - rr.Y;
                //			this.DrawRectangle(Pens.Red,rr.X, rr.Y, rr.Width, rr.Height);
#endif // DEBUG

                CommonElements common = axis.Common;
                if (common.ProcessModeRegions)
                {
                    common.HotRegionsList.AddHotRegion(Rectangle.Round(absPosition), label, ChartElementType.AxisLabels, false, true);
                }

                //********************************************************************
                //** Draw labels in the second row
                //********************************************************************
                if (labelRowIndex > 0)
                {
                    drawingFormat.LineAlignment = StringAlignment.Center;
                    drawingFormat.Alignment = StringAlignment.Center;
                    angle = 0;

                    if (axis.AxisPosition == AxisPosition.Left)
                    {
                        angle = -90;
                    }
                    else if (axis.AxisPosition == AxisPosition.Right)
                    {
                        angle = 90;
                    }
                    else if (axis.AxisPosition == AxisPosition.Top)
                    {
                    }
                    else if (axis.AxisPosition == AxisPosition.Bottom)
                    {
                    }
                }

                //********************************************************************
                //** Calculate rotation point
                //********************************************************************
                PointF rotationPoint = PointF.Empty;
                if (axis.AxisPosition == AxisPosition.Left)
                {
                    rotationPoint.X = absPosition.Right;
                    rotationPoint.Y = absPosition.Y + absPosition.Height / 2F;
                }
                else if (axis.AxisPosition == AxisPosition.Right)
                {
                    rotationPoint.X = absPosition.Left;
                    rotationPoint.Y = absPosition.Y + absPosition.Height / 2F;
                }
                else if (axis.AxisPosition == AxisPosition.Top)
                {
                    rotationPoint.X = absPosition.X + absPosition.Width / 2F;
                    rotationPoint.Y = absPosition.Bottom;
                }
                else if (axis.AxisPosition == AxisPosition.Bottom)
                {
                    rotationPoint.X = absPosition.X + absPosition.Width / 2F;
                    rotationPoint.Y = absPosition.Top;
                }

                //********************************************************************
                //** Adjust rectangle for horisontal axis
                //********************************************************************
                if ((axis.AxisPosition == AxisPosition.Top || axis.AxisPosition == AxisPosition.Bottom) &&
                    angle != 0)
                {
                    // Get rectangle center
                    rotationPoint.X = absPosition.X + absPosition.Width / 2F;
                    rotationPoint.Y = (axis.AxisPosition == AxisPosition.Top) ? absPosition.Bottom : absPosition.Y;

                    // Rotate rectangle 90 degrees
                    RectangleF newRect = RectangleF.Empty;
                    newRect.X = absPosition.X + absPosition.Width / 2F;
                    newRect.Y = absPosition.Y - absPosition.Width / 2F;
                    newRect.Height = absPosition.Width;
                    newRect.Width = absPosition.Height;

                    // Adjust values for bottom axis
                    if (axis.AxisPosition == AxisPosition.Bottom)
                    {
                        if (angle < 0)
                        {
                            newRect.X -= newRect.Width;
                        }

                        // Replace string alignment
                        drawingFormat.Alignment = StringAlignment.Near;
                        if (angle < 0)
                        {
                            drawingFormat.Alignment = StringAlignment.Far;
                        }
                        drawingFormat.LineAlignment = StringAlignment.Center;
                    }

                    // Adjust values for bottom axis
                    if (axis.AxisPosition == AxisPosition.Top)
                    {
                        newRect.Y += absPosition.Height;
                        if (angle > 0)
                        {
                            newRect.X -= newRect.Width;
                        }

                        // Replace string alignment
                        drawingFormat.Alignment = StringAlignment.Far;
                        if (angle < 0)
                        {
                            drawingFormat.Alignment = StringAlignment.Near;
                        }
                        drawingFormat.LineAlignment = StringAlignment.Center;
                    }

                    // Set new label rect
                    absPosition = newRect;
                }

                //********************************************************************
                //** 90 degrees is a special case for vertical axes
                //********************************************************************
                if ((axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right) &&
                    (angle == 90 || angle == -90))
                {
                    // Get rectangle center
                    rotationPoint.X = absPosition.X + absPosition.Width / 2F;
                    rotationPoint.Y = absPosition.Y + absPosition.Height / 2F;

                    // Rotate rectangle 90 degrees
                    RectangleF newRect = RectangleF.Empty;
                    newRect.X = rotationPoint.X - absPosition.Height / 2F;
                    newRect.Y = rotationPoint.Y - absPosition.Width / 2F;
                    newRect.Height = absPosition.Width;
                    newRect.Width = absPosition.Height;
                    absPosition = newRect;

                    // Replace string alignment
                    StringAlignment align = drawingFormat.Alignment;
                    drawingFormat.Alignment = drawingFormat.LineAlignment;
                    drawingFormat.LineAlignment = align;
                    if (angle == 90)
                    {
                        if (drawingFormat.LineAlignment == StringAlignment.Far)
                            drawingFormat.LineAlignment = StringAlignment.Near;
                        else if (drawingFormat.LineAlignment == StringAlignment.Near)
                            drawingFormat.LineAlignment = StringAlignment.Far;
                    }
                    if (angle == -90)
                    {
                        if (drawingFormat.Alignment == StringAlignment.Far)
                            drawingFormat.Alignment = StringAlignment.Near;
                        else if (drawingFormat.Alignment == StringAlignment.Near)
                            drawingFormat.Alignment = StringAlignment.Far;
                    }
                }

                //********************************************************************
                //** Create a matrix and rotate it.
                //********************************************************************
                oldTransform = null;
                if (angle != 0)
                {
                    _myMatrix = this.Transform.Clone();
                    _myMatrix.RotateAt(angle, rotationPoint);

                    // Old angle
                    oldTransform = this.Transform;

                    // Set Angle
                    this.Transform = _myMatrix;
                }

                //********************************************************************
                //** Measure string exact rectangle and adjust label bounding rectangle
                //********************************************************************
                RectangleF labelRect = Rectangle.Empty;
                float offsetY = 0f;
                float offsetX = 0f;
                
                // Measure text size
                labelSize = this.MeasureString(text.Replace("\\n", "\n"), font, absPosition.Size, drawingFormat);

                // Calculate text rectangle
                labelRect.Width = labelSize.Width;
                labelRect.Height = labelSize.Height;
                if (drawingFormat.Alignment == StringAlignment.Far)
                {
                    labelRect.X = absPosition.Right - labelSize.Width;
                }
                else if (drawingFormat.Alignment == StringAlignment.Near)
                {
                    labelRect.X = absPosition.X;
                }
                else if (drawingFormat.Alignment == StringAlignment.Center)
                {
                    labelRect.X = absPosition.X + absPosition.Width / 2F - labelSize.Width / 2F;
                }

                if (drawingFormat.LineAlignment == StringAlignment.Far)
                {
                    labelRect.Y = absPosition.Bottom - labelSize.Height;
                }
                else if (drawingFormat.LineAlignment == StringAlignment.Near)
                {
                    labelRect.Y = absPosition.Y;
                }
                else if (drawingFormat.LineAlignment == StringAlignment.Center)
                {
                    labelRect.Y = absPosition.Y + absPosition.Height / 2F - labelSize.Height / 2F;
                }

                //If the angle is not vertical or horizontal
                if (angle != 0 && angle != 90 && angle != -90)
                {
                    // Adjust label rectangle so it will not overlap the plotting area
                    offsetY = (float)Math.Sin((90 - angle) / 180F * Math.PI) * labelRect.Height / 2F;
                    offsetX = (float)Math.Sin((Math.Abs(angle)) / 180F * Math.PI) * labelRect.Height / 2F;

                    if (axis.AxisPosition == AxisPosition.Left)
                    {
                        _myMatrix.Translate(-offsetX, 0);
                    }
                    else if (axis.AxisPosition == AxisPosition.Right)
                    {
                        _myMatrix.Translate(offsetX, 0);
                    }
                    else if (axis.AxisPosition == AxisPosition.Top)
                    {
                        _myMatrix.Translate(0, -offsetY);
                    }
                    else if (axis.AxisPosition == AxisPosition.Bottom)
                    {
                        _myMatrix.Translate(0, offsetY);
                    }

                    // Adjust label rectangle so it will be inside boundary
                    if (boundaryRect != RectangleF.Empty)
                    {
                        Region region = new Region(labelRect);
                        region.Transform(_myMatrix);

                        // Extend boundary rectangle to the chart picture border
                        if (axis.AxisPosition == AxisPosition.Left)
                        {
                            boundaryRect.Width += boundaryRect.X;
                            boundaryRect.X = 0;
                        }
                        else if (axis.AxisPosition == AxisPosition.Right)
                        {
                            boundaryRect.Width = this._common.Width - boundaryRect.X;
                        }
                        else if (axis.AxisPosition == AxisPosition.Top)
                        {
                            boundaryRect.Height += boundaryRect.Y;
                            boundaryRect.Y = 0;
                        }
                        else if (axis.AxisPosition == AxisPosition.Bottom)
                        {
                            boundaryRect.Height = this._common.Height - boundaryRect.Y;
                        }

                        // Exclude boundary rectangle from the label rectangle
                        region.Exclude(this.GetAbsoluteRectangle(boundaryRect));

                        // If any part of the label was outside bounding rectangle
                        if (!region.IsEmpty(Graphics))
                        {
                            this.Transform = oldTransform;
                            RectangleF truncateRect = region.GetBounds(Graphics);

                            float sizeChange = truncateRect.Width / (float)Math.Cos(Math.Abs(angle) / 180F * Math.PI);
                            if (axis.AxisPosition == AxisPosition.Left)
                            {
                                sizeChange -= labelRect.Height * (float)Math.Tan(Math.Abs(angle) / 180F * Math.PI);
                                absPosition.Y = labelRect.Y;
                                absPosition.X = labelRect.X + sizeChange;
                                absPosition.Width = labelRect.Width - sizeChange;
                                absPosition.Height = labelRect.Height;
                            }
                            else if (axis.AxisPosition == AxisPosition.Right)
                            {
                                sizeChange -= labelRect.Height * (float)Math.Tan(Math.Abs(angle) / 180F * Math.PI);
                                absPosition.Y = labelRect.Y;
                                absPosition.X = labelRect.X;
                                absPosition.Width = labelRect.Width - sizeChange;
                                absPosition.Height = labelRect.Height;
                            }
                            else if (axis.AxisPosition == AxisPosition.Top)
                            {
                                absPosition.Y = labelRect.Y;
                                absPosition.X = labelRect.X;
                                absPosition.Width = labelRect.Width - sizeChange;
                                absPosition.Height = labelRect.Height;
                                if (angle > 0)
                                {
                                    absPosition.X += sizeChange;
                                }
                            }
                            else if (axis.AxisPosition == AxisPosition.Bottom)
                            {
                                absPosition.Y = labelRect.Y;
                                absPosition.X = labelRect.X;
                                absPosition.Width = labelRect.Width - sizeChange;
                                absPosition.Height = labelRect.Height;
                                if (angle < 0)
                                {
                                    absPosition.X += sizeChange;
                                }
                            }
                        }
                    }

                    // Update transformation matrix
                    this.Transform = _myMatrix;
                }

                //********************************************************************
                //** Reserve space on the left for the label iamge
                //********************************************************************
                RectangleF absPositionWithoutImage = new RectangleF(absPosition.Location, absPosition.Size);
                
                System.Drawing.Image labelImage = null;
                SizeF imageAbsSize = new SizeF();

                if (image.Length > 0)
                {
                    labelImage = axis.Common.ImageLoader.LoadImage(label.Image);

                    if (labelImage != null)
                    {
                        ImageLoader.GetAdjustedImageSize(labelImage, this.Graphics, ref imageAbsSize);

                        // Adjust label position using image size
                        absPositionWithoutImage.Width -= imageAbsSize.Width;
                        absPositionWithoutImage.X += imageAbsSize.Width;
                    }

                    if (absPositionWithoutImage.Width < 1f)
                    {
                        absPositionWithoutImage.Width = 1f;
                    }

                }

                //********************************************************************
                //** Draw tick marks for labels in second row
                //********************************************************************
                if (labelRowIndex > 0 && labelMark != LabelMarkStyle.None)
                {
                    // Make sure that me know the exact size of the text
                    labelSize = this.MeasureString(
                        text.Replace("\\n", "\n"),
                        font,
                        absPositionWithoutImage.Size,
                        drawingFormat);

                    // Adjust for label image
                    SizeF labelSizeWithImage = new SizeF(labelSize.Width, labelSize.Height);
                    if (labelImage != null)
                    {
                        labelSizeWithImage.Width += imageAbsSize.Width;
                    }

                    // Draw mark
                    DrawSecondRowLabelMark(
                        axis,
                        markColor,
                        absPosition,
                        labelSizeWithImage,
                        labelMark,
                        truncatedLeft,
                        truncatedRight,
                        oldTransform);
                }

                //********************************************************************
                //** Make sure that one line label will not disapear with LineLimit
                //** flag on.
                //********************************************************************
                if ((drawingFormat.FormatFlags & StringFormatFlags.LineLimit) != 0)
                {
                    // Measure string height out of one character
                    drawingFormat.FormatFlags ^= StringFormatFlags.LineLimit;
                    SizeF size = this.MeasureString("I", font, absPosition.Size, drawingFormat);

                    // If height of one characte is more than rectangle heigjt - remove LineLimit flag
                    if (size.Height < absPosition.Height)
                    {
                        drawingFormat.FormatFlags |= StringFormatFlags.LineLimit;
                    }
                }
                else
                {
                    // Set NoClip flag
                    if ((drawingFormat.FormatFlags & StringFormatFlags.NoClip) != 0)
                    {
                        drawingFormat.FormatFlags ^= StringFormatFlags.NoClip;
                    }

                    // Measure string height out of one character without clipping
                    SizeF size = this.MeasureString("I", font, absPosition.Size, drawingFormat);

                    // Clear NoClip flag
                    drawingFormat.FormatFlags ^= StringFormatFlags.NoClip;

                    // If height of one characte is more than rectangle heigt - set NoClip flag
                    if (size.Height > absPosition.Height)
                    {
                        float delta = size.Height - absPosition.Height;
                        absPosition.Y -= delta / 2f;
                        absPosition.Height += delta;
                    }
                }

                //********************************************************************
                //** Draw a string
                //********************************************************************
                if (IsRightToLeft)
                {
                    // label alignment on the axis should appear as not RTL. 
                    using (StringFormat fmt = (StringFormat)drawingFormat.Clone())
                    {

                        if (fmt.Alignment == StringAlignment.Far)
                        {
                            fmt.Alignment = StringAlignment.Near;
                        }
                        else if (fmt.Alignment == StringAlignment.Near)
                        {
                            fmt.Alignment = StringAlignment.Far;
                        }
                        this.DrawString(text.Replace("\\n", "\n"), font, brush,
                        absPositionWithoutImage,
                        fmt);

                    }
                }
                else
                    this.DrawString(text.Replace("\\n", "\n"), font, brush,
                    absPositionWithoutImage,
                    drawingFormat);

                // Add separate hot region for the label
                if (common.ProcessModeRegions)
                {
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddRectangle(labelRect);
                        path.Transform(this.Transform);
                        string url = string.Empty;
                        string mapAreaAttributes = string.Empty;
                        string postbackValue = string.Empty;
#if !Microsoft_CONTROL
                        url = label.Url;
                        mapAreaAttributes = label.MapAreaAttributes;
                        postbackValue = label.PostBackValue;
#endif // !Microsoft_CONTROL
                        common.HotRegionsList.AddHotRegion(
                            this,
                            path,
                            false,
                            label.ToolTip,
                            url,
                            mapAreaAttributes,
                            postbackValue,
                            label,
                            ChartElementType.AxisLabels);
                    }
                }

                //********************************************************************
                //** Draw an image
                //********************************************************************
                if (labelImage != null)
                {
                    // Make sure we no the text size
                    if (labelSize.IsEmpty)
                    {
                        labelSize = this.MeasureString(
                            text.Replace("\\n", "\n"),
                            font,
                            absPositionWithoutImage.Size,
                            drawingFormat);
                    }

                    // Calculate image rectangle
                    RectangleF imageRect = new RectangleF(
                        absPosition.X + (absPosition.Width - imageAbsSize.Width - labelSize.Width) / 2,
                        absPosition.Y + (absPosition.Height - imageAbsSize.Height) / 2,
                        imageAbsSize.Width,
                        imageAbsSize.Height);

                    if (drawingFormat.LineAlignment == StringAlignment.Center)
                    {
                        imageRect.Y = absPosition.Y + (absPosition.Height - imageAbsSize.Height) / 2;
                    }
                    else if (drawingFormat.LineAlignment == StringAlignment.Far)
                    {
                        imageRect.Y = absPosition.Bottom - (labelSize.Height + imageAbsSize.Height) / 2;
                    }
                    else if (drawingFormat.LineAlignment == StringAlignment.Near)
                    {
                        imageRect.Y = absPosition.Top + (labelSize.Height - imageAbsSize.Height) / 2;
                    }

                    if (drawingFormat.Alignment == StringAlignment.Center)
                    {
                        imageRect.X = absPosition.X + (absPosition.Width - imageAbsSize.Width - labelSize.Width) / 2;
                    }
                    else if (drawingFormat.Alignment == StringAlignment.Far)
                    {
                        imageRect.X = absPosition.Right - imageAbsSize.Width - labelSize.Width;
                    }
                    else if (drawingFormat.Alignment == StringAlignment.Near)
                    {
                        imageRect.X = absPosition.X;
                    }

                    // Create image attribute
                    ImageAttributes attrib = new ImageAttributes();
                    if (imageTransparentColor != Color.Empty)
                    {
                        attrib.SetColorKey(imageTransparentColor, imageTransparentColor, ColorAdjustType.Default);
                    }

                    // Draw image
                    this.DrawImage(
                        labelImage,
                        Rectangle.Round(imageRect),
                        0, 0, labelImage.Width, labelImage.Height,
                        GraphicsUnit.Pixel,
                        attrib);

                    // Add separate hot region for the label image
                    if (common.ProcessModeRegions)
                    {
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            path.AddRectangle(imageRect);
                            path.Transform(this.Transform);
                            string imageUrl = string.Empty;
                            string imageMapAreaAttributes = string.Empty;
                            string postbackValue = string.Empty;
#if !Microsoft_CONTROL
                            imageUrl = label.ImageUrl;
                            imageMapAreaAttributes = label.ImageMapAreaAttributes;
                            postbackValue = label.PostBackValue;
#endif // !Microsoft_CONTROL
                            common.HotRegionsList.AddHotRegion(
                                this,
                                path,
                                false,
                                string.Empty,
                                imageUrl,
                                imageMapAreaAttributes,
                                postbackValue,
                                label,
                                ChartElementType.AxisLabelImage);
                        }
                    }
                }
            }

			// Set Old Angle
			if(oldTransform != null)
			{
				this.Transform = oldTransform;
			}
		}

		/// <summary>
		/// Draw box marks for the labels in second row
		/// </summary>
		/// <param name="axis">Axis object.</param>
		/// <param name="markColor">Label mark color.</param>
		/// <param name="absPosition">Absolute position of the text.</param>
		/// <param name="truncatedLeft">Label is truncated on the left.</param>
		/// <param name="truncatedRight">Label is truncated on the right.</param>
		/// <param name="originalTransform">Original transformation matrix.</param>
		private void DrawSecondRowLabelBoxMark(
			Axis axis, 
			Color markColor,
			RectangleF absPosition, 
			bool truncatedLeft,
			bool truncatedRight,
			Matrix originalTransform)
		{
			// Remeber current and then reset original matrix
			Matrix curentMatrix = this.Transform;
			if(originalTransform != null)
			{
				this.Transform = originalTransform;
			}

			// Calculate center of the text rectangle
			PointF	centerNotRound = new PointF(absPosition.X + absPosition.Width/2F, absPosition.Y + absPosition.Height/2F);

			// Rotate rectangle 90 degrees
			if( axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
			{
				RectangleF newRect = RectangleF.Empty;
				newRect.X = centerNotRound.X - absPosition.Height / 2F;
				newRect.Y = centerNotRound.Y - absPosition.Width / 2F;
				newRect.Height = absPosition.Width;
				newRect.Width = absPosition.Height;
				absPosition = newRect;
			}

			// Get axis position
			float axisPosRelative = (float)axis.GetAxisPosition(true);
			PointF axisPositionAbs = new PointF(axisPosRelative, axisPosRelative);
			axisPositionAbs = this.GetAbsolutePoint(axisPositionAbs);

			// Round position to achieve crisp lines with antialiasing
			Rectangle absPositionRounded = Rectangle.Round(absPosition);

			// Make sure the right and bottom position is not shifted during rounding
			absPositionRounded.Width = (int)Math.Round(absPosition.Right) - absPositionRounded.X;
			absPositionRounded.Height = (int)Math.Round(absPosition.Bottom) - absPositionRounded.Y;

			// Create pen
			Pen	markPen = new Pen(
				(markColor.IsEmpty) ? axis.MajorTickMark.LineColor : markColor, 
				axis.MajorTickMark.LineWidth);

			// Set pen style
			markPen.DashStyle = GetPenStyle( axis.MajorTickMark.LineDashStyle );

			// Draw top/bottom lines
			if( axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
			{
				this.DrawLine(markPen, absPositionRounded.Left, absPositionRounded.Top, absPositionRounded.Left, absPositionRounded.Bottom);
				this.DrawLine(markPen, absPositionRounded.Right, absPositionRounded.Top, absPositionRounded.Right, absPositionRounded.Bottom);
			}
			else
			{
				this.DrawLine(markPen, absPositionRounded.Left, absPositionRounded.Top, absPositionRounded.Right, absPositionRounded.Top);
				this.DrawLine(markPen, absPositionRounded.Left, absPositionRounded.Bottom, absPositionRounded.Right, absPositionRounded.Bottom);
			}

			// Draw left line
			if(!truncatedLeft)
			{
				if( axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
				{
					this.DrawLine(
						markPen, 
						(axis.AxisPosition == AxisPosition.Left) ? absPositionRounded.Left : absPositionRounded.Right, 
						absPositionRounded.Bottom, 
						axisPositionAbs.X, 
						absPositionRounded.Bottom);
				}
				else
				{
					this.DrawLine(
						markPen, 
						absPositionRounded.Left, 
						(axis.AxisPosition == AxisPosition.Top) ? absPositionRounded.Top : absPositionRounded.Bottom, 
						absPositionRounded.Left, 
						axisPositionAbs.Y);
				}
			}

			// Draw right line
			if(!truncatedRight)
			{
				if( axis.AxisPosition == AxisPosition.Left || axis.AxisPosition == AxisPosition.Right)
				{
					this.DrawLine(
						markPen, 
						(axis.AxisPosition == AxisPosition.Left) ? absPositionRounded.Left : absPositionRounded.Right, 
						absPositionRounded.Top, 
						axisPositionAbs.X, 
						absPositionRounded.Top);
				}
				else
				{
					this.DrawLine(
						markPen, 
						absPositionRounded.Right, 
						(axis.AxisPosition == AxisPosition.Top) ? absPositionRounded.Top : absPositionRounded.Bottom, 
						absPositionRounded.Right, 
						axisPositionAbs.Y);
				}
			}

			// Dispose Pen
			if( markPen != null )
			{
				markPen.Dispose();
			}

			// Restore currentmatrix
			if(originalTransform != null)
			{
				this.Transform = curentMatrix;
			}
		}


		/// <summary>
		/// Draw marks for the labels in second row
		/// </summary>
		/// <param name="axis">Axis object.</param>
		/// <param name="markColor">Label mark color.</param>
		/// <param name="absPosition">Absolute position of the text.</param>
		/// <param name="labelSize">Exact mesured size of the text.</param>
		/// <param name="labelMark">Label mark style to draw.</param>
		/// <param name="truncatedLeft">Label is truncated on the left.</param>
		/// <param name="truncatedRight">Label is truncated on the right.</param>
		/// <param name="oldTransform">Original transformation matrix.</param>
		private void DrawSecondRowLabelMark(
			Axis axis, 
			Color markColor,
			RectangleF absPosition, 
			SizeF labelSize, 
			LabelMarkStyle labelMark,
			bool truncatedLeft,
			bool truncatedRight,
			Matrix oldTransform)
		{
			// Do not draw marking line if width is 0 and style or color are not set
			if( axis.MajorTickMark.LineWidth == 0 || 
				axis.MajorTickMark.LineDashStyle == ChartDashStyle.NotSet ||
				axis.MajorTickMark.LineColor == Color.Empty)
			{
				return;
			}

			// Remember SmoothingMode and turn off anti aliasing for 
			// vertical or horizontal lines of the label markers.
			SmoothingMode oldSmoothingMode = this.SmoothingMode;
			this.SmoothingMode = SmoothingMode.None;


			// Draw box marker
			if(labelMark == LabelMarkStyle.Box)
			{
				DrawSecondRowLabelBoxMark(
					axis, 
					markColor,
					absPosition, 
					truncatedLeft,
					truncatedRight,
					oldTransform);
			}
			else

			{
				// Calculate center of the text rectangle
				Point	center = Point.Round(new PointF(absPosition.X + absPosition.Width/2F, absPosition.Y + absPosition.Height/2F));

				// Round position to achieve crisp lines with antialiasing
				Rectangle absPositionRounded = Rectangle.Round(absPosition);

				// Make sure the right and bottom position is not shifted during rounding
				absPositionRounded.Width = (int)Math.Round(absPosition.Right) - absPositionRounded.X;
				absPositionRounded.Height = (int)Math.Round(absPosition.Bottom) - absPositionRounded.Y;


				// Arrays of points for the left and right marking lines
				PointF[]	leftLine = new PointF[3];
				PointF[]	rightLine = new PointF[3];

				// Calculate marking lines coordinates
				leftLine[0].X = absPositionRounded.Left;
				leftLine[0].Y = absPositionRounded.Bottom;
				leftLine[1].X = absPositionRounded.Left;
				leftLine[1].Y = center.Y;
				leftLine[2].X = (float)Math.Round((double)center.X - labelSize.Width/2F - 1F);
				leftLine[2].Y = center.Y;

				rightLine[0].X = absPositionRounded.Right;
				rightLine[0].Y = absPositionRounded.Bottom;
				rightLine[1].X = absPositionRounded.Right;
				rightLine[1].Y = center.Y;
				rightLine[2].X = (float)Math.Round((double)center.X + labelSize.Width/2F - 1F);
				rightLine[2].Y = center.Y;

				if(axis.AxisPosition == AxisPosition.Bottom)
				{
					leftLine[0].Y = absPositionRounded.Top;
					rightLine[0].Y = absPositionRounded.Top;
				}

				// Remove third point to draw only side marks
				if(labelMark == LabelMarkStyle.SideMark)
				{
					leftLine[2] = leftLine[1];
					rightLine[2] = rightLine[1];
				}

				if(truncatedLeft)
				{
					leftLine[0] = leftLine[1];
				}
				if(truncatedRight)
				{
					rightLine[0] = rightLine[1];
				}

				// Create pen
				Pen	markPen = new Pen(
					(markColor.IsEmpty) ? axis.MajorTickMark.LineColor : markColor, 
					axis.MajorTickMark.LineWidth);

				// Set pen style
				markPen.DashStyle = GetPenStyle( axis.MajorTickMark.LineDashStyle );

				// Draw marking lines
				this.DrawLines(markPen, leftLine);
				this.DrawLines(markPen, rightLine);

				// Dispose Pen
				if( markPen != null )
				{
					markPen.Dispose();
				}
			}

			// Restore previous SmoothingMode
			this.SmoothingMode = oldSmoothingMode;
		}

		/// <summary>
		/// Measures the specified text string when drawn with 
		/// the specified Font object and formatted with the 
		/// specified StringFormat object.
		/// </summary>
		/// <param name="text">The string to measure</param>
		/// <param name="font">The Font object used to determine the size of the text string. </param>
		/// <returns>A SizeF structure that represents the size of text as drawn with font.</returns>
		internal SizeF MeasureStringRel( string text, Font font )
		{
			SizeF newSize;

			// Measure string
			newSize = this.MeasureString( text, font );

			// Convert to relative Coordinates
			return GetRelativeSize( newSize );
		}

		/// <summary>
		/// Measures the specified text string when drawn with 
		/// the specified Font object and formatted with the 
		/// specified StringFormat object.
		/// </summary>
		/// <param name="text">The string to measure</param>
		/// <param name="font">The Font object used to determine the size of the text string. </param>
		/// <param name="layoutArea">A SizeF structure that specifies the layout rectangle for the text. </param>
		/// <param name="stringFormat">A StringFormat object that represents formatting information, such as line spacing, for the text string. </param>
		/// <returns>A SizeF structure that represents the size of text as drawn with font.</returns>
		internal SizeF MeasureStringRel( string text, Font font, SizeF layoutArea, StringFormat stringFormat )
		{
			SizeF size, newSize;

			// Get absolute coordinates
			size = GetAbsoluteSize( layoutArea );

			newSize = this.MeasureString( text, font, size, stringFormat );

			// Convert to relative Coordinates
			return GetRelativeSize( newSize );
		}

		/// <summary>
		/// Measures the specified text string when drawn with 
		/// the specified Font object and formatted with the 
		/// specified StringFormat object.
		/// </summary>
		/// <param name="text">The string to measure</param>
		/// <param name="font">The Font object used to determine the size of the text string. </param>
		/// <returns>A SizeF structure that represents the size of text as drawn with font.</returns>
		internal Size MeasureStringAbs( string text, Font font )
		{
			// Measure string
			SizeF size = this.MeasureString( text, font );
			return new Size( (int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
		}

		/// <summary>
		/// Measures the specified text string when drawn with 
		/// the specified Font object and formatted with the 
		/// specified StringFormat object.
		/// </summary>
		/// <param name="text">The string to measure</param>
		/// <param name="font">The Font object used to determine the size of the text string. </param>
		/// <param name="layoutArea">A SizeF structure that specifies the layout rectangle for the text. </param>
		/// <param name="stringFormat">A StringFormat object that represents formatting information, such as line spacing, for the text string. </param>
		/// <returns>A SizeF structure that represents the size of text as drawn with font.</returns>
		internal Size MeasureStringAbs( string text, Font font, SizeF layoutArea, StringFormat stringFormat )
		{
			SizeF size = this.MeasureString( text, font, layoutArea, stringFormat );
			return new Size( (int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
		}

		/// <summary>
		/// Draws the specified text string at the specified location 
		/// with the specified Brush object and font. The formatting 
		/// properties in the specified StringFormat object are applied 
		/// to the text.
		/// </summary>
		/// <param name="text">A string object that specifies the text to draw.</param>
		/// <param name="font">A Font object that specifies the font face and size with which to draw the text.</param>
		/// <param name="brush">A Brush object that determines the color and/or texture of the drawn text.</param>
		/// <param name="layoutRectangle">A RectangleF structure that specifies the location of the drawn text.</param>
		/// <param name="format">A StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
		internal void DrawStringRel( string text, Font font, Brush brush,	RectangleF layoutRectangle,	StringFormat format	)
		{
			RectangleF rect;

			// Check that rectangle is not empty
			if(layoutRectangle.Width == 0 || layoutRectangle.Height == 0)
			{
				return;
			}

			// Get absolute coordinates
			rect = GetAbsoluteRectangle( layoutRectangle );

			// Draw text with anti-aliasing
			/*
			if( (this.AntiAliasing & AntiAliasing.Text) == AntiAliasing.Text )
			{
				this.TextRenderingHint = TextRenderingHint.AntiAlias;
			}
			else
			{
				this.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
			}
			*/

			this.DrawString( text, font, brush, rect, format );
		}

		
		/// <summary>
		/// Draws the specified text string at the specified location 
		/// with the specified angle and with the specified Brush object and font. The 
		/// formatting properties in the specified StringFormat object are applied 
		/// to the text.
		/// </summary>
		/// <param name="text">A string object that specifies the text to draw.</param>
		/// <param name="font">A Font object that specifies the font face and size with which to draw the text.</param>
		/// <param name="brush">A Brush object that determines the color and/or texture of the drawn text.</param>
		/// <param name="layoutRectangle">A RectangleF structure that specifies the location of the drawn text.</param>
		/// <param name="format">A StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
		/// <param name="angle">A angle of the text</param>
		internal void DrawStringRel( 
			string text, 
			Font font, 
			Brush brush,	
			RectangleF layoutRectangle,	
			StringFormat format, 
			int angle	
			)
		{
			RectangleF rect;
			SizeF size;
			Matrix oldTransform;
			PointF rotationCenter = PointF.Empty;

			// Check that rectangle is not empty
			if(layoutRectangle.Width == 0 || layoutRectangle.Height == 0)
			{
				return;
			}

			// Get absolute coordinates
			rect = GetAbsoluteRectangle( layoutRectangle );

			size = this.MeasureString( text, font, rect.Size, format );


			// Find the center of rotation
			if( format.Alignment == StringAlignment.Near )
			{ // Near
				rotationCenter.X = rect.X + size.Width / 2;
				rotationCenter.Y = ( rect.Bottom + rect.Top ) / 2;
			}
			else if( format.Alignment == StringAlignment.Far )
			{ // Far
				rotationCenter.X = rect.Right - size.Width / 2;
				rotationCenter.Y = ( rect.Bottom + rect.Top ) / 2;
			}
			else
			{ // Center
				rotationCenter.X = ( rect.Left + rect.Right ) / 2;
				rotationCenter.Y = ( rect.Bottom + rect.Top ) / 2;
			}
			// Create a matrix and rotate it.
			_myMatrix = this.Transform.Clone();
			_myMatrix.RotateAt( angle, rotationCenter);

			// Old angle
			oldTransform = this.Transform;

			// Set Angle
			this.Transform = _myMatrix;

			// Draw text with anti-aliasing
			/*
			if( (AntiAliasing & AntiAliasing.Text) == AntiAliasing.Text )
			{
				this.TextRenderingHint = TextRenderingHint.AntiAlias;
			}
			else
			{
				this.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
			}
			*/

			this.DrawString( text, font, brush, rect, format );

			// Set Old Angle
			this.Transform = oldTransform;
		}

		#endregion

		#region Rectangle Methods

		/// <summary>
		/// Draws different shadows to create bar styles.
		/// </summary>
		/// <param name="barDrawingStyle">Bar drawing style.</param>
		/// <param name="isVertical">True if a vertical bar.</param>
		/// <param name="rect">Rectangle position.</param>
		internal void DrawRectangleBarStyle(BarDrawingStyle barDrawingStyle, bool isVertical, RectangleF rect)
		{
			// Check if non-default bar drawing style is specified
			if(barDrawingStyle != BarDrawingStyle.Default)
			{
				// Check column/bar size
				if(rect.Width > 0 && rect.Height > 0)
				{
					// Draw gradient(s)
					if(barDrawingStyle == BarDrawingStyle.Cylinder)
					{
						// Calculate gradient position
						RectangleF gradientRect = rect;
						if(isVertical)
						{
							gradientRect.Width *= 0.3f;
						}
						else
						{
							gradientRect.Height *= 0.3f;
						}
						if(gradientRect.Width > 0 && gradientRect.Height > 0)
						{
							this.FillRectangleAbs( 
								gradientRect, 
								Color.Transparent,
								ChartHatchStyle.None, 
								string.Empty, 
								ChartImageWrapMode.Scaled, 
								Color.Empty,
								ChartImageAlignmentStyle.Center,
								(isVertical) ? GradientStyle.LeftRight : GradientStyle.TopBottom, 
								Color.FromArgb(120, Color.White),
								Color.Empty, 
								0, 
								ChartDashStyle.NotSet, 
								PenAlignment.Inset );

						
							if(isVertical)
							{
								gradientRect.X += gradientRect.Width + 1f;
								gradientRect.Width = rect.Right - gradientRect.X;
							}
							else
							{
								gradientRect.Y += gradientRect.Height + 1f;
								gradientRect.Height = rect.Bottom - gradientRect.Y;
							}

							this.FillRectangleAbs( 
								gradientRect, 
								Color.FromArgb(120, Color.White),
								ChartHatchStyle.None, 
								string.Empty, 
								ChartImageWrapMode.Scaled, 
								Color.Empty,
								ChartImageAlignmentStyle.Center,
								(isVertical) ? GradientStyle.LeftRight : GradientStyle.TopBottom, 
								Color.FromArgb(150, Color.Black),
								Color.Empty, 
								0, 
								ChartDashStyle.NotSet, 
								PenAlignment.Inset );

						}
					}
					else if(barDrawingStyle == BarDrawingStyle.Emboss)
					{
						// Calculate width of shadows used to create the effect
						float shadowSize = 3f;
						if(rect.Width < 6f || rect.Height < 6f)
						{
							shadowSize = 1f;
						}
						else if(rect.Width < 15f || rect.Height < 15f)
						{
							shadowSize = 2f;
						}

						// Create and draw left/top path
						using(GraphicsPath path = new GraphicsPath())
						{
							// Add shadow polygon to the path
							PointF[] points = new PointF[] {
															   new PointF(rect.Left, rect.Bottom),
															   new PointF(rect.Left, rect.Top),
															   new PointF(rect.Right, rect.Top),
															   new PointF(rect.Right - shadowSize, rect.Top + shadowSize),
															   new PointF(rect.Left + shadowSize, rect.Top + shadowSize),
															   new PointF(rect.Left + shadowSize, rect.Bottom - shadowSize) };
							path.AddPolygon(points);

							// Create brush
							using(SolidBrush leftTopBrush = new SolidBrush(Color.FromArgb(100, Color.White)))
							{
								// Fill shadow path on the left-bottom side of the bar
								this.FillPath(leftTopBrush, path);
							}
						}

						// Create and draw top/right path
						using(GraphicsPath path = new GraphicsPath())
						{
							// Add shadow polygon to the path
							PointF[] points = new PointF[] {
															   new PointF(rect.Right, rect.Top),
															   new PointF(rect.Right, rect.Bottom),
															   new PointF(rect.Left, rect.Bottom),
															   new PointF(rect.Left + shadowSize, rect.Bottom - shadowSize),
															   new PointF(rect.Right - shadowSize, rect.Bottom - shadowSize),
															   new PointF(rect.Right - shadowSize, rect.Top + shadowSize) };
							path.AddPolygon(points);

							// Create brush
							using(SolidBrush bottomRightBrush = new SolidBrush(Color.FromArgb(80, Color.Black)))
							{
								// Fill shadow path on the left-bottom side of the bar
								this.FillPath(bottomRightBrush, path);
							}
						}
					}
					else if(barDrawingStyle == BarDrawingStyle.LightToDark)
					{
						// Calculate width of shadows used to create the effect
						float shadowSize = 4f;
						if(rect.Width < 6f || rect.Height < 6f)
						{
							shadowSize = 2f;
						}
						else if(rect.Width < 15f || rect.Height < 15f)
						{
							shadowSize = 3f;
						}

						// Calculate gradient position
						RectangleF gradientRect = rect;
						gradientRect.Inflate(-shadowSize, -shadowSize);
						if(isVertical)
						{
							gradientRect.Height = (float)Math.Floor(gradientRect.Height / 3f);
						}
						else
						{
							gradientRect.X = gradientRect.Right - (float)Math.Floor(gradientRect.Width / 3f);
							gradientRect.Width = (float)Math.Floor(gradientRect.Width / 3f);
						}
						if(gradientRect.Width > 0 && gradientRect.Height > 0)
						{
							this.FillRectangleAbs( 
								gradientRect, 
								(isVertical) ? Color.FromArgb(120, Color.White) : Color.Transparent, 
								ChartHatchStyle.None, 
								string.Empty, 
								ChartImageWrapMode.Scaled, 
								Color.Empty,
								ChartImageAlignmentStyle.Center,
								(isVertical) ? GradientStyle.TopBottom : GradientStyle.LeftRight, 
								(isVertical) ? Color.Transparent : Color.FromArgb(120, Color.White), 
								Color.Empty, 
								0, 
								ChartDashStyle.NotSet, 
								PenAlignment.Inset );

							gradientRect = rect;
							gradientRect.Inflate(-shadowSize, -shadowSize);
							if(isVertical)
							{
								gradientRect.Y = gradientRect.Bottom - (float)Math.Floor(gradientRect.Height / 3f);
								gradientRect.Height = (float)Math.Floor(gradientRect.Height / 3f);
							}
							else
							{
								gradientRect.Width = (float)Math.Floor(gradientRect.Width / 3f);
							}


							this.FillRectangleAbs( 
								gradientRect, 
								(!isVertical) ? Color.FromArgb(80, Color.Black) : Color.Transparent, 
								ChartHatchStyle.None, 
								string.Empty, 
								ChartImageWrapMode.Scaled, 
								Color.Empty,
								ChartImageAlignmentStyle.Center,
								(isVertical) ? GradientStyle.TopBottom : GradientStyle.LeftRight, 
								(!isVertical) ? Color.Transparent : Color.FromArgb(80, Color.Black), 
								Color.Empty, 
								0, 
								ChartDashStyle.NotSet, 
								PenAlignment.Inset );

						}
					}
					else if(barDrawingStyle == BarDrawingStyle.Wedge)
					{
						// Calculate wedge size to fit the rectangle
						float size = (isVertical) ? rect.Width / 2f : rect.Height / 2f;
						if(isVertical && 2f * size > rect.Height)
						{
							size = rect.Height/2f;
						}
						if(!isVertical && 2f * size > rect.Width)
						{
							size = rect.Width/2f;
						}

						// Draw left/bottom shadow
						RectangleF gradientRect = rect;
						using(GraphicsPath path = new GraphicsPath())
						{
							if(isVertical)
							{
								path.AddLine(gradientRect.X + gradientRect.Width/2f, gradientRect.Y + size, gradientRect.X + gradientRect.Width/2f, gradientRect.Bottom - size);
								path.AddLine(gradientRect.X + gradientRect.Width/2f, gradientRect.Bottom - size, gradientRect.Right, gradientRect.Bottom);
								path.AddLine(gradientRect.Right, gradientRect.Bottom, gradientRect.Right, gradientRect.Y);
							}
							else
							{
								path.AddLine(gradientRect.X + size, gradientRect.Y + gradientRect.Height/2f, gradientRect.Right - size, gradientRect.Y + gradientRect.Height/2f);
								path.AddLine(gradientRect.Right - size, gradientRect.Y + gradientRect.Height/2f, gradientRect.Right, gradientRect.Bottom);
								path.AddLine(gradientRect.Right, gradientRect.Bottom, gradientRect.Left, gradientRect.Bottom);
							}
							path.CloseAllFigures();

							// Create brush and fill path
							using(SolidBrush brush = new SolidBrush(Color.FromArgb(90, Color.Black)))
							{
								this.FillPath(brush, path);
							}
						}

						// Draw top/right triangle
						using(GraphicsPath path = new GraphicsPath())
						{
							if(isVertical)
							{
								path.AddLine(gradientRect.X, gradientRect.Y, gradientRect.X + gradientRect.Width/2f, gradientRect.Y + size);
								path.AddLine(gradientRect.X + gradientRect.Width/2f, gradientRect.Y + size, gradientRect.Right, gradientRect.Y);
							}
							else
							{
								path.AddLine(gradientRect.Right, gradientRect.Y, gradientRect.Right - size, gradientRect.Y + gradientRect.Height / 2f);
								path.AddLine(gradientRect.Right - size, gradientRect.Y + gradientRect.Height / 2f, gradientRect.Right, gradientRect.Bottom);
							}

							// Create brush and fill path
							using(SolidBrush brush = new SolidBrush(Color.FromArgb(50, Color.Black)))
							{
								// Fill shadow path on the left-bottom side of the bar
								this.FillPath(brush, path);

								// Draw Lines
								using(Pen penDark = new Pen(Color.FromArgb(20, Color.Black), 1))
								{
									this.DrawPath(penDark, path);
									if(isVertical)
									{
										this.DrawLine(
											penDark, 
											rect.X + rect.Width/2f, 
											rect.Y + size,
											rect.X + rect.Width/2f, 
											rect.Bottom - size);
									}
									else
									{
										this.DrawLine(
											penDark, 
											rect.X + size, 
											rect.Y + rect.Height/2f,
											rect.X + size, 
											rect.Bottom - rect.Height/2f);
									}
								}

								// Draw Lines
								using(Pen pen = new Pen(Color.FromArgb(40, Color.White), 1))
								{
									this.DrawPath(pen, path);
									if(isVertical)
									{
										this.DrawLine(
											pen, 
											rect.X + rect.Width/2f, 
											rect.Y + size,
											rect.X + rect.Width/2f, 
											rect.Bottom - size);
									}
									else
									{
										this.DrawLine(
											pen, 
											rect.X + size, 
											rect.Y + rect.Height/2f,
											rect.X + size, 
											rect.Bottom - rect.Height/2f);
									}
								}
							}
						}

						// Draw bottom/left triangle
						using(GraphicsPath path = new GraphicsPath())
						{
							if(isVertical)
							{
								path.AddLine(gradientRect.X, gradientRect.Bottom, gradientRect.X + gradientRect.Width/2f, gradientRect.Bottom - size);
								path.AddLine(gradientRect.X + gradientRect.Width/2f, gradientRect.Bottom - size, gradientRect.Right, gradientRect.Bottom);
							}
							else
							{
								path.AddLine(gradientRect.X, gradientRect.Y, gradientRect.X + size, gradientRect.Y + gradientRect.Height / 2f);
								path.AddLine(gradientRect.X + size, gradientRect.Y + gradientRect.Height / 2f, gradientRect.X, gradientRect.Bottom);
							}

							// Create brush
							using(SolidBrush brush = new SolidBrush(Color.FromArgb(50, Color.Black)))
							{
								// Fill shadow path on the left-bottom side of the bar
								this.FillPath(brush, path);

								// Draw edges
								using(Pen penDark = new Pen(Color.FromArgb(20, Color.Black), 1))
								{
									this.DrawPath(penDark, path);
								}
								using(Pen pen = new Pen(Color.FromArgb(40, Color.White), 1))
								{
									this.DrawPath(pen, path);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Draw a bar with shadow.
		/// </summary>
		/// <param name="rectF">Size of rectangle</param>
		/// <param name="backColor">Color of rectangle</param>
		/// <param name="backHatchStyle">Hatch style</param>
		/// <param name="backImage">Back Image</param>
		/// <param name="backImageWrapMode">Image mode</param>
		/// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
		/// <param name="backGradientStyle">Gradient type </param>
		/// <param name="backSecondaryColor">Gradient End Color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		/// <param name="shadowColor">Shadow Color</param>
		/// <param name="shadowOffset">Shadow Offset</param>
		/// <param name="penAlignment">Pen Alignment</param>
		/// <param name="barDrawingStyle">Bar drawing style.</param>
		/// <param name="isVertical">True if a vertical bar.</param>
		internal void FillRectangleRel( RectangleF rectF, 
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle, 
			Color shadowColor, 
			int shadowOffset,
			PenAlignment penAlignment,
			BarDrawingStyle barDrawingStyle,
			bool isVertical)
		{
			this.FillRectangleRel( 
				rectF, 
				backColor, 
				backHatchStyle, 
				backImage, 
				backImageWrapMode, 
				backImageTransparentColor,
				backImageAlign,
				backGradientStyle, 
				backSecondaryColor, 
				borderColor, 
				borderWidth, 
				borderDashStyle, 
				shadowColor, 
				shadowOffset,
				penAlignment,
				false,
				0,
				false,
				barDrawingStyle,
				isVertical);
		}

		/// <summary>
		/// Draw a bar with shadow.
		/// </summary>
		/// <param name="rectF">Size of rectangle</param>
		/// <param name="backColor">Color of rectangle</param>
		/// <param name="backHatchStyle">Hatch style</param>
		/// <param name="backImage">Back Image</param>
		/// <param name="backImageWrapMode">Image mode</param>
		/// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
		/// <param name="backGradientStyle">Gradient type </param>
		/// <param name="backSecondaryColor">Gradient End Color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		/// <param name="shadowColor">Shadow Color</param>
		/// <param name="shadowOffset">Shadow Offset</param>
		/// <param name="penAlignment">Pen Alignment</param>
		internal void FillRectangleRel( RectangleF rectF, 
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle, 
			Color shadowColor, 
			int shadowOffset,
			PenAlignment penAlignment )
		{
			this.FillRectangleRel( 
				rectF, 
				backColor, 
				backHatchStyle, 
				backImage, 
				backImageWrapMode, 
				backImageTransparentColor,
				backImageAlign,
				backGradientStyle, 
				backSecondaryColor, 
				borderColor, 
				borderWidth, 
				borderDashStyle, 
				shadowColor, 
				shadowOffset,
				penAlignment,
				false,
				0,
				false,
				BarDrawingStyle.Default,
				true);
		}

		/// <summary>
		/// Draws rectangle or circle (inside rectangle) with shadow.
		/// </summary>
		/// <param name="rectF">Size of rectangle</param>
		/// <param name="backColor">Color of rectangle</param>
		/// <param name="backHatchStyle">Hatch style</param>
		/// <param name="backImage">Back Image</param>
		/// <param name="backImageWrapMode">Image mode</param>
		/// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
		/// <param name="backGradientStyle">Gradient type </param>
		/// <param name="backSecondaryColor">Gradient End Color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		/// <param name="shadowColor">Shadow Color</param>
		/// <param name="shadowOffset">Shadow Offset</param>
		/// <param name="penAlignment">Pen Alignment</param>
		/// <param name="circular">Draw circular shape inside the rectangle.</param>
		/// <param name="circularSectorsCount">Number of sectors in circle when drawing the polygon.</param>
		/// <param name="circle3D">3D Circle must be drawn.</param>
		internal void FillRectangleRel( RectangleF rectF, 
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle, 
			Color shadowColor, 
			int shadowOffset,
			PenAlignment penAlignment,
			bool circular,
			int	circularSectorsCount,
			bool circle3D)
		{
			this.FillRectangleRel( 
				rectF, 
				backColor, 
				backHatchStyle, 
				backImage, 
				backImageWrapMode, 
				backImageTransparentColor,
				backImageAlign,
				backGradientStyle, 
				backSecondaryColor, 
				borderColor, 
				borderWidth, 
				borderDashStyle, 
				shadowColor, 
				shadowOffset,
				penAlignment,
                circular,
                circularSectorsCount,
                circle3D,
				BarDrawingStyle.Default,
				true);
		}

		
		/// <summary>
		/// Draws rectangle or circle (inside rectangle) with shadow.
		/// </summary>
		/// <param name="rectF">Size of rectangle</param>
		/// <param name="backColor">Color of rectangle</param>
		/// <param name="backHatchStyle">Hatch style</param>
		/// <param name="backImage">Back Image</param>
		/// <param name="backImageWrapMode">Image mode</param>
		/// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
		/// <param name="backGradientStyle">Gradient type </param>
		/// <param name="backSecondaryColor">Gradient End Color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		/// <param name="shadowColor">Shadow Color</param>
		/// <param name="shadowOffset">Shadow Offset</param>
		/// <param name="penAlignment">Pen Alignment</param>
		/// <param name="circular">Draw circular shape inside the rectangle.</param>
		/// <param name="circularSectorsCount">Number of sectors in circle when drawing the polygon.</param>
		/// <param name="circle3D">3D Circle must be drawn.</param>
		/// <param name="barDrawingStyle">Bar drawing style.</param>
		/// <param name="isVertical">True if a vertical bar.</param>
		internal void FillRectangleRel( RectangleF rectF, 
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle, 
			Color shadowColor, 
			int shadowOffset,
			PenAlignment penAlignment,
			bool circular,
			int	circularSectorsCount,
			bool circle3D,
			BarDrawingStyle barDrawingStyle,
			bool isVertical)
		{
			Brush brush = null;
			Brush backBrush = null;

			// Remember SmoothingMode and turn off anti aliasing
			SmoothingMode oldSmoothingMode = this.SmoothingMode;
			if(!circular)
			{
				this.SmoothingMode = SmoothingMode.Default;
			}

			// Color is empty
			if( backColor.IsEmpty ) 
			{
				backColor = Color.White;
			}

			if( backSecondaryColor.IsEmpty ) 
			{
				backSecondaryColor = Color.White;
			}

			if( borderColor.IsEmpty || borderDashStyle == ChartDashStyle.NotSet) 
			{
				borderWidth = 0;
			}
		
			// Get absolute coordinates
			RectangleF rect = GetAbsoluteRectangle( rectF );
			
			// Rectangle width and height can not be very small value
			if( rect.Width < 1.0F && rect.Width > 0.0F )
			{
				rect.Width = 1.0F;
			}

			if( rect.Height < 1.0F && rect.Height > 0.0F )
			{
				rect.Height = 1.0F;
			}

			// Round the values
			rect = Round( rect );

			// For inset alignment resize fill rectangle
			RectangleF fillRect;
			if( penAlignment == PenAlignment.Inset  &&
				borderWidth > 0)
			{
				// SVG and Metafiles do not support inset pen styles - use same rectangle
				if( this.ActiveRenderingType == RenderingType.Svg ||
					this.IsMetafile)
				{
					fillRect = new RectangleF( rect.X, rect.Y, rect.Width, rect.Height);
				}
                else if (this.Graphics.Transform.Elements[0] != 1f ||
                    this.Graphics.Transform.Elements[3] != 1f)
                {
                    // Do not reduce filling rectangle if scaling is used in the graphics
                    // transformations. Rounding may cause a 1 pixel gap between the border 
                    // and the filling.
                    fillRect = new RectangleF( rect.X, rect.Y, rect.Width, rect.Height);
                }
				else
				{
					// The fill rectangle is resized because of border size.
					fillRect = new RectangleF( 
						rect.X + borderWidth, 
						rect.Y + borderWidth, 
						rect.Width - borderWidth * 2f + 1, 
						rect.Height - borderWidth * 2f + 1);
				}
			}
			else
			{
				// The fill rectangle is same
				fillRect = rect;
			}

			// Fix for issue #6714:
			// Make sure the rectangle coordinates fit the control. In same cases rectangle width or
			// hight ca be extremly large. Drawing such a rectangle may cause an overflow exception. 
			// The code below restricts the maximum size to double the chart size. See issue 
			// description for more information. -AG.
			if(fillRect.Width > 2f * this._width)
			{
				fillRect.Width = 2f * this._width;
			}
			if(fillRect.Height > 2f * this._height)
			{
				fillRect.Height = 2f * this._height;
			}


			if( backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled)
			{
				backBrush = brush;
				brush = GetTextureBrush(backImage, backImageTransparentColor, backImageWrapMode, backColor);
			}
			else if( backHatchStyle != ChartHatchStyle.None )
			{
				brush = GetHatchBrush( backHatchStyle, backColor, backSecondaryColor );
			}
			else if( backGradientStyle != GradientStyle.None )
			{
				// If a gradient type  is set create a brush with gradient
				brush = GetGradientBrush( rect, backColor, backSecondaryColor, backGradientStyle );
			}
			else
			{
				// Set a bar color.
				if(backColor == Color.Empty || backColor == Color.Transparent)
				{
					brush = null;
				}
				else
				{
					brush = new SolidBrush(backColor);
				}
			}

			// Draw shadow
			FillRectangleShadowAbs( rect, shadowColor, shadowOffset, backColor, circular, circularSectorsCount );

			// Draw rectangle image
			if( backImage.Length > 0 && (backImageWrapMode == ChartImageWrapMode.Unscaled || backImageWrapMode == ChartImageWrapMode.Scaled))
			{
				// Load image
                System.Drawing.Image image = _common.ImageLoader.LoadImage( backImage );

				// Prepare image properties (transparent color)
				ImageAttributes attrib = new ImageAttributes();
				if(backImageTransparentColor != Color.Empty)
				{
					attrib.SetColorKey(backImageTransparentColor, backImageTransparentColor, ColorAdjustType.Default);
				}

				// Draw scaled image
				RectangleF imageRect = new RectangleF();
				imageRect.X = fillRect.X;
				imageRect.Y = fillRect.Y;
				imageRect.Width = fillRect.Width;
				imageRect.Height = fillRect.Height;

                SizeF imageAbsSize = new SizeF();

				// Calculate unscaled image position
				if(backImageWrapMode == ChartImageWrapMode.Unscaled)
				{
                    ImageLoader.GetAdjustedImageSize(image, this.Graphics, ref imageAbsSize);

                    // Calculate image position
                    imageRect.Width = Math.Min(fillRect.Width, imageAbsSize.Width);
                    imageRect.Height = Math.Min(fillRect.Height, imageAbsSize.Height);

                   	// Adjust position with alignment property
					if(imageRect.Width < fillRect.Width)
					{
						if(backImageAlign == ChartImageAlignmentStyle.BottomRight ||
							backImageAlign == ChartImageAlignmentStyle.Right ||
							backImageAlign == ChartImageAlignmentStyle.TopRight)
						{
							imageRect.X = fillRect.Right - imageRect.Width;
						}
						else if(backImageAlign == ChartImageAlignmentStyle.Bottom ||
							backImageAlign == ChartImageAlignmentStyle.Center ||
							backImageAlign == ChartImageAlignmentStyle.Top)
						{
							imageRect.X = fillRect.X + (fillRect.Width - imageRect.Width)/2;
						}
					}
					if(imageRect.Height < fillRect.Height)
					{
						if(backImageAlign == ChartImageAlignmentStyle.BottomRight ||
							backImageAlign == ChartImageAlignmentStyle.Bottom ||
							backImageAlign == ChartImageAlignmentStyle.BottomLeft)
						{
							imageRect.Y = fillRect.Bottom - imageRect.Height;
						}
						else if(backImageAlign == ChartImageAlignmentStyle.Left ||
							backImageAlign == ChartImageAlignmentStyle.Center ||
							backImageAlign == ChartImageAlignmentStyle.Right)
						{
							imageRect.Y = fillRect.Y + (fillRect.Height - imageRect.Height)/2;
						}
					}

				}

				// Fill background with brush
				if(brush != null)
				{
					if(circular)
						this.DrawCircleAbs( null, brush, fillRect, circularSectorsCount, circle3D );
					else
						this.FillRectangle( brush, fillRect );
				}
                 
                // Draw image
				this.DrawImage(image, 
					new Rectangle((int)Math.Round(imageRect.X),(int)Math.Round(imageRect.Y), (int)Math.Round(imageRect.Width), (int)Math.Round(imageRect.Height)),
					0, 0,
                    (backImageWrapMode == ChartImageWrapMode.Unscaled) ? imageRect.Width * image.Width / imageAbsSize.Width : image.Width,
                    (backImageWrapMode == ChartImageWrapMode.Unscaled) ? imageRect.Height * image.Height / imageAbsSize.Height : image.Height,
					GraphicsUnit.Pixel, 
					attrib);
			}
				// Draw rectangle
			else
			{
				if(backBrush != null && backImageTransparentColor != Color.Empty)
				{
					// Fill background with brush
					if(circular)
						this.DrawCircleAbs( null, backBrush, fillRect, circularSectorsCount, circle3D );
					else
						this.FillRectangle( backBrush, fillRect );
				}

				if(brush != null)
				{
					if(circular)
						this.DrawCircleAbs( null, brush, fillRect, circularSectorsCount, circle3D );
					else
						this.FillRectangle( brush, fillRect );
				}
			}

			// Draw different bar style
			this.DrawRectangleBarStyle(barDrawingStyle, isVertical, fillRect);

			// Draw border
			if( borderWidth > 0 && borderDashStyle != ChartDashStyle.NotSet)
			{
				// Set a border line color
				if(_pen.Color != borderColor)
				{
					_pen.Color = borderColor;
				}
			
				// Set a border line width
				if(_pen.Width != borderWidth)
				{
					_pen.Width = borderWidth;
				}

				// Set pen alignment
				if(_pen.Alignment != penAlignment)
				{
					_pen.Alignment = penAlignment;
				}

				// Set a border line style
				if(_pen.DashStyle != GetPenStyle( borderDashStyle ))
				{
					_pen.DashStyle = GetPenStyle( borderDashStyle );
				}

				// Draw border
				if(circular)
				{
					this.DrawCircleAbs( _pen, null, rect, circularSectorsCount, false );
				}
				else
				{
					// NOTE: Rectangle with single pixel inset border is drawn 1 pixel larger 
					// in the .Net Framework. Increase size by 1 pixel to solve the issue.
					if(_pen.Alignment == PenAlignment.Inset && _pen.Width > 1f)
					{
						rect.Width += 1;
						rect.Height += 1;
					}

					// Draw rectangle
					this.DrawRectangle( _pen, rect.X, rect.Y, rect.Width, rect.Height );
				}
			}

			// Dispose Image and Gradient
			if(brush != null)
			{
				brush.Dispose();
			}

			// Return old smoothing mode
			this.SmoothingMode = oldSmoothingMode;
		}

		/// <summary>
		/// Draw Shadow for a bar
		/// </summary>
		/// <param name="rect">Bar rectangle</param>
		/// <param name="shadowColor">Shadow Color</param>
		/// <param name="shadowOffset">Shadow Offset</param>
		/// <param name="backColor">Back Color</param>
        internal void FillRectangleShadowAbs( 
			RectangleF rect, 
			Color shadowColor, 
			float shadowOffset, 
			Color backColor)
		{
			FillRectangleShadowAbs( 
				rect, 
				shadowColor, 
				shadowOffset, 
				backColor,
				false,
				0);
		}

        /// <summary>
        /// Draw Shadow for a bar
        /// </summary>
        /// <param name="rect">Bar rectangle</param>
        /// <param name="shadowColor">Shadow Color</param>
        /// <param name="shadowOffset">Shadow Offset</param>
        /// <param name="backColor">Back Color</param>
        /// <param name="circular">Draw circular shape inside the rectangle.</param>
        /// <param name="circularSectorsCount">Number of sectors in circle when drawing the polygon.</param>
		internal void FillRectangleShadowAbs( 
			RectangleF rect, 
			Color shadowColor, 
			float shadowOffset, 
			Color backColor,
			bool circular,
			int	circularSectorsCount)
		{
			// Do not draw shadoe for empty rectangle
			if(rect.Height == 0 || rect.Width == 0 || shadowOffset == 0)
			{
				return;
			}

            // Do not draw  shadow if color is IsEmpty or offset is 0
            if (shadowOffset == 0 || shadowColor == Color.Empty)
            {
                return;
            }

            // For non-circualr shadow with transparent background - use clipping
            bool clippingUsed = false;
            Region oldClipRegion = null;
            if (!circular && backColor == Color.Transparent)
            {
                clippingUsed = true;
                oldClipRegion = this.Clip;
                Region region = new Region();
                region.MakeInfinite();
                region.Xor(rect);
                this.Clip = region;
            }
            
			// Draw usual or "soft" shadows
			if(!softShadows || circularSectorsCount > 2)
			{
				RectangleF absolute;
				RectangleF offset = RectangleF.Empty;

				absolute = Round( rect );

				// Change shadow color
                using (SolidBrush shadowBrush = new SolidBrush((shadowColor.A != 255) ? shadowColor : Color.FromArgb(backColor.A / 2, shadowColor)))
                {
                    // Shadow Position
                    offset.X = absolute.X + shadowOffset;
                    offset.Y = absolute.Y + shadowOffset;
                    offset.Width = absolute.Width;
                    offset.Height = absolute.Height;

                    // Draw rectangle
                    if (circular)
                        this.DrawCircleAbs(null, shadowBrush, offset, circularSectorsCount, false);
                    else
                        this.FillRectangle(shadowBrush, offset);
                }
			}
			else
			{

				RectangleF absolute;
				RectangleF offset = RectangleF.Empty;

				absolute = Round( rect );
				

				// Shadow Position
				offset.X = absolute.X + shadowOffset - 1;
				offset.Y = absolute.Y + shadowOffset - 1;
				offset.Width = absolute.Width + 2;
				offset.Height = absolute.Height + 2;
				
				// Calculate rounded rect radius
				float	radius = shadowOffset * 0.7f;
				radius = (float)Math.Max(radius, 2f);
				radius = (float)Math.Min(radius, offset.Width/4f);
				radius = (float)Math.Min(radius, offset.Height/4f);
				radius = (float)Math.Ceiling(radius);
				if(circular)
				{
					radius = offset.Width/2f;
				}

				// Create rounded rectangle path
				GraphicsPath path = new GraphicsPath();
				if(circular && offset.Width != offset.Height)
				{
					float	radiusX = offset.Width/2f;
					float	radiusY = offset.Height/2f;
					path.AddLine(offset.X+radiusX, offset.Y, offset.Right-radiusX, offset.Y);
					path.AddArc(offset.Right-2f*radiusX, offset.Y, 2f*radiusX, 2f*radiusY, 270, 90);
					path.AddLine(offset.Right, offset.Y + radiusY, offset.Right, offset.Bottom - radiusY);
					path.AddArc(offset.Right-2f*radiusX, offset.Bottom-2f*radiusY, 2f*radiusX, 2f*radiusY, 0, 90);
					path.AddLine(offset.Right-radiusX, offset.Bottom, offset.X + radiusX, offset.Bottom);
					path.AddArc(offset.X, offset.Bottom-2f*radiusY, 2f*radiusX, 2f*radiusY, 90, 90);
					path.AddLine(offset.X, offset.Bottom-radiusY, offset.X, offset.Y+radiusY);
					path.AddArc(offset.X, offset.Y, 2f*radiusX, 2f*radiusY, 180, 90);
				}
				else
				{
					path.AddLine(offset.X+radius, offset.Y, offset.Right-radius, offset.Y);
					path.AddArc(offset.Right-2f*radius, offset.Y, 2f*radius, 2f*radius, 270, 90);
					path.AddLine(offset.Right, offset.Y + radius, offset.Right, offset.Bottom - radius);
					path.AddArc(offset.Right-2f*radius, offset.Bottom-2f*radius, 2f*radius, 2f*radius, 0, 90);
					path.AddLine(offset.Right-radius, offset.Bottom, offset.X + radius, offset.Bottom);
					path.AddArc(offset.X, offset.Bottom-2f*radius, 2f*radius, 2f*radius, 90, 90);
					path.AddLine(offset.X, offset.Bottom-radius, offset.X, offset.Y+radius);
					path.AddArc(offset.X, offset.Y, 2f*radius, 2f*radius, 180, 90);
				}

				PathGradientBrush shadowBrush = new PathGradientBrush(path);
				shadowBrush.CenterColor = shadowColor;

				// Set the color along the entire boundary of the path
				Color[] colors = {Color.Transparent};
				shadowBrush.SurroundColors = colors;
				shadowBrush.CenterPoint = new PointF(offset.X + offset.Width/2f, offset.Y + offset.Height/2f);

				// Define brush focus scale
				PointF focusScale = new PointF(1-2f*shadowOffset/offset.Width, 1-2f*shadowOffset/offset.Height);
				if(focusScale.X < 0)
					focusScale.X = 0;
				if(focusScale.Y < 0)
					focusScale.Y = 0;
				shadowBrush.FocusScales = focusScale;

                // Draw rectangle
				this.FillPath(shadowBrush, path);
			}

            // Reset clip region
            if (clippingUsed)
            {
                Region region = this.Clip;
                this.Clip = oldClipRegion;
                region.Dispose();
            }
		}

		/// <summary>
		/// Gets the path of the polygon which represent the circular area.
		/// </summary>
		/// <param name="position">Circle position.</param>
		/// <param name="polygonSectorsNumber">Number of sectors for the polygon.</param>
		/// <returns>Graphics path of the polygon circle.</returns>
		internal GraphicsPath GetPolygonCirclePath(RectangleF position, int polygonSectorsNumber)
		{
			PointF			firstPoint = new PointF(position.X + position.Width/2f, position.Y);
			PointF			centerPoint = new PointF(position.X + position.Width/2f, position.Y + position.Height/2f);
			float			sectorSize = 0f;
			GraphicsPath	path = new GraphicsPath();
			PointF			prevPoint = PointF.Empty;
			float			curentSector = 0f;

			// Get sector size
			if(polygonSectorsNumber <= 2)
			{
				// Circle sector size
				sectorSize = 1f;
			}
			else
			{
				// Polygon sector size
				sectorSize = 360f / ((float)polygonSectorsNumber);
			}

			// Loop throug all sectors
			for(curentSector = 0f; curentSector < 360f; curentSector += sectorSize)
			{
				// Create matrix
				Matrix matrix = new Matrix();
				matrix.RotateAt(curentSector, centerPoint);

				// Get point and rotate it
				PointF[]	points = new PointF[] { firstPoint };
				matrix.TransformPoints(points);

				// Add point into the path
				if(!prevPoint.IsEmpty)
				{
					path.AddLine(prevPoint, points[0]);
				}

				// Remember last point
				prevPoint = points[0];
			}

			path.CloseAllFigures();

			return path;
		}

		/// <summary>
		/// Fills and/or draws border as circle or polygon.
		/// </summary>
		/// <param name="pen">Border pen.</param>
		/// <param name="brush">Border brush.</param>
		/// <param name="position">Circle position.</param>
		/// <param name="polygonSectorsNumber">Number of sectors for the polygon.</param>
		/// <param name="circle3D">Indicates that circle should be 3D..</param>
		internal void DrawCircleAbs(Pen pen, Brush brush, RectangleF position, int polygonSectorsNumber, bool circle3D)
		{
			bool	fill3DCircle = (circle3D && brush != null);

			// Draw 2D circle
			if(polygonSectorsNumber <= 2 && !fill3DCircle)
			{
				if(brush != null)
				{
					this.FillEllipse(brush, position);
				}
				if(pen != null)
				{
					this.DrawEllipse(pen, position);
				}
			}

				// Draw circle as polygon with specified number of sectors
			else
			{
				PointF			firstPoint = new PointF(position.X + position.Width/2f, position.Y);
				PointF			centerPoint = new PointF(position.X + position.Width/2f, position.Y + position.Height/2f);
				float			sectorSize = 0f;
				PointF			prevPoint = PointF.Empty;
				float			curentSector = 0f;

                using (GraphicsPath path = new GraphicsPath())
                {
                    // Remember current smoothing mode
                    SmoothingMode oldMode = this.SmoothingMode;
                    if (fill3DCircle)
                    {
                        this.SmoothingMode = SmoothingMode.None;
                    }

                    // Get sector size
                    if (polygonSectorsNumber <= 2)
                    {
                        // Circle sector size
                        sectorSize = 1f;
                    }
                    else
                    {
                        // Polygon sector size
                        sectorSize = 360f / ((float)polygonSectorsNumber);
                    }

                    // Loop throug all sectors
                    for (curentSector = 0f; curentSector < 360f; curentSector += sectorSize)
                    {
                        // Create matrix
                        Matrix matrix = new Matrix();
                        matrix.RotateAt(curentSector, centerPoint);

                        // Get point and rotate it
                        PointF[] points = new PointF[] { firstPoint };
                        matrix.TransformPoints(points);

                        // Add point into the path
                        if (!prevPoint.IsEmpty)
                        {
                            path.AddLine(prevPoint, points[0]);

                            // Fill each segment separatly for the 3D look
                            if (fill3DCircle)
                            {
                                path.AddLine(points[0], centerPoint);
                                path.AddLine(centerPoint, prevPoint);
                                using (Brush sectorBrush = GetSector3DBrush(brush, curentSector, sectorSize))
                                {
                                    this.FillPath(sectorBrush, path);
                                }
                                path.Reset();
                            }
                        }

                        // Remember last point
                        prevPoint = points[0];
                    }

                    path.CloseAllFigures();

                    // Fill last segment for the 3D look
                    if (!prevPoint.IsEmpty && fill3DCircle)
                    {
                        path.AddLine(prevPoint, firstPoint);
                        path.AddLine(firstPoint, centerPoint);
                        path.AddLine(centerPoint, prevPoint);
                        using (Brush sectorBrush = GetSector3DBrush(brush, curentSector, sectorSize))
                        {
                            this.FillPath(sectorBrush, path);
                        }
                        path.Reset();
                    }

                    // Restore old mode
                    if (fill3DCircle)
                    {
                        this.SmoothingMode = oldMode;
                    }

                    if (brush != null && !circle3D)
                    {
                        this.FillPath(brush, path);
                    }
                    if (pen != null)
                    {
                        this.DrawPath(pen, path);
                    }
                }				
			}
		}

        /// <summary>
        /// Creates 3D sector brush.
        /// </summary>
        /// <param name="brush">Original brush.</param>
        /// <param name="curentSector">Sector position.</param>
        /// <param name="sectorSize">Sector size.</param>
        /// <returns>3D brush.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily",
            Justification = "Too large of a code change to justify making this change")]
		internal Brush GetSector3DBrush(Brush brush, float curentSector, float sectorSize)
		{
			// Get color from the brush
			Color	brushColor = Color.Gray;
			if(brush is HatchBrush)
			{
				brushColor = ((HatchBrush)brush).BackgroundColor;
			}
			else if(brush is LinearGradientBrush)
			{
				brushColor = ((LinearGradientBrush)brush).LinearColors[0];
			}
			else if(brush is PathGradientBrush)
			{
				brushColor = ((PathGradientBrush)brush).CenterColor;
			}
			else if(brush is SolidBrush)
			{
				brushColor = ((SolidBrush)brush).Color;
			}

			// Adjust sector angle
			curentSector -= sectorSize / 2f;

			// Make adjustment for polygon circle with 5 segments
			// to avoid the issue that bottom segment is too dark
			if(sectorSize == 72f && curentSector == 180f)
			{
                curentSector *= 0.8f;
			}

			// No angles more than 180 
			if(curentSector > 180)
			{
				curentSector = 360f - curentSector;
			}
			curentSector = curentSector / 180F;

			// Get brush
			brushColor = GetBrightGradientColor( brushColor, curentSector);

			// Get brush
			return new SolidBrush(brushColor);
		}

        /// <summary>
        /// This method creates gradient color with brightness
        /// </summary>
        /// <param name="beginColor">Start color for gradient.</param>
        /// <param name="position">Position used between Start and end color.</param>
        /// <returns>Calculated Gradient color from gradient position</returns>
		internal Color GetBrightGradientColor( Color beginColor, double position )
		{
			double brightness = 0.5;
			if( position < brightness )
			{
				return GetGradientColor( Color.FromArgb(beginColor.A,255,255,255), beginColor, 1 - brightness + position );
			}
			else if( -brightness + position < 1 )
			{
				return GetGradientColor( beginColor, Color.Black, -brightness + position);
			}
			else
			{
				return Color.FromArgb( beginColor.A, 0, 0, 0 );
			}
		}

		/// <summary>
		/// Draw Rectangle using absolute coordinates.
		/// </summary>
		/// <param name="rect">Size of rectangle</param>
		/// <param name="backColor">Color of rectangle</param>
		/// <param name="backHatchStyle">Hatch Style</param>
		/// <param name="backImage">Image URL</param>
		/// <param name="backImageWrapMode">Image Mode</param>
		/// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment.</param>
		/// <param name="backGradientStyle">Gradient AxisName</param>
		/// <param name="backSecondaryColor">End Gradient color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		/// <param name="penAlignment">Border is outside or inside rectangle</param>
		internal void FillRectangleAbs( RectangleF rect, 
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle, 
			PenAlignment penAlignment )
		{
			Brush brush = null;
			Brush backBrush = null;

			// Turn off Antialias
			SmoothingMode oldMode = this.SmoothingMode;
			this.SmoothingMode = SmoothingMode.None;

			// Color is empty
			if( backColor.IsEmpty ) 
				backColor = Color.White;

			if( backSecondaryColor.IsEmpty ) 
				backSecondaryColor = Color.White;

			if( borderColor.IsEmpty ) 
			{
				borderColor = Color.White;
				borderWidth = 0;
			}
		
			// Set a border line color
			_pen.Color = borderColor;

			// Set a border line width
			_pen.Width = borderWidth;

			// Set pen alignment
			_pen.Alignment = penAlignment;

			// Set a border line style
			_pen.DashStyle = GetPenStyle( borderDashStyle );

			if( backGradientStyle == GradientStyle.None )
			{
				// Set a bar color.
				_solidBrush.Color = backColor;
				brush = _solidBrush;
			}
			else
			{
				// If a gradient type  is set create a brush with gradient
				brush = GetGradientBrush( rect, backColor, backSecondaryColor, backGradientStyle );
			}

			if( backHatchStyle != ChartHatchStyle.None )
			{
				brush = GetHatchBrush( backHatchStyle, backColor, backSecondaryColor );
			}

			if( backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled)
			{
				backBrush = brush;
				brush = GetTextureBrush(backImage, backImageTransparentColor, backImageWrapMode, backColor );
			}

			// For inset alignment resize fill rectangle
			RectangleF fillRect;
			
			// The fill rectangle is same
			fillRect = new RectangleF( rect.X + borderWidth, rect.Y + borderWidth, rect.Width - borderWidth * 2, rect.Height - borderWidth * 2 );

			// FillRectangle and DrawRectangle works differently with RectangleF.
			fillRect.Width += 1;
			fillRect.Height += 1;

			// Draw rectangle image
			if( backImage.Length > 0 && (backImageWrapMode == ChartImageWrapMode.Unscaled || backImageWrapMode == ChartImageWrapMode.Scaled))
			{
				// Load image
                System.Drawing.Image image = _common.ImageLoader.LoadImage( backImage );
                                

				// Prepare image properties (transparent color)
				ImageAttributes attrib = new ImageAttributes();
				if(backImageTransparentColor != Color.Empty)
				{
					attrib.SetColorKey(backImageTransparentColor, backImageTransparentColor, ColorAdjustType.Default);
				}

				// Draw scaled image
				RectangleF imageRect = new RectangleF();
				imageRect.X = fillRect.X;
				imageRect.Y = fillRect.Y;
				imageRect.Width = fillRect.Width;
				imageRect.Height = fillRect.Height;

				// Draw unscaled image using align property
				if(backImageWrapMode == ChartImageWrapMode.Unscaled)
				{
                    SizeF imageAbsSize = new SizeF();

                    ImageLoader.GetAdjustedImageSize(image, this.Graphics, ref imageAbsSize);

					// Calculate image position
                    imageRect.Width = imageAbsSize.Width;
                    imageRect.Height = imageAbsSize.Height;

					// Adjust position with alignment property
					if(imageRect.Width < fillRect.Width)
					{
						if(backImageAlign == ChartImageAlignmentStyle.BottomRight ||
							backImageAlign == ChartImageAlignmentStyle.Right ||
							backImageAlign == ChartImageAlignmentStyle.TopRight)
						{
							imageRect.X = fillRect.Right - imageRect.Width;
						}
						else if(backImageAlign == ChartImageAlignmentStyle.Bottom ||
							backImageAlign == ChartImageAlignmentStyle.Center ||
							backImageAlign == ChartImageAlignmentStyle.Top)
						{
							imageRect.X = fillRect.X + (fillRect.Width - imageRect.Width)/2;
						}
					}
					if(imageRect.Height < fillRect.Height)
					{
						if(backImageAlign == ChartImageAlignmentStyle.BottomRight ||
							backImageAlign == ChartImageAlignmentStyle.Bottom ||
							backImageAlign == ChartImageAlignmentStyle.BottomLeft)
						{
							imageRect.Y = fillRect.Bottom - imageRect.Height;
						}
						else if(backImageAlign == ChartImageAlignmentStyle.Left ||
							backImageAlign == ChartImageAlignmentStyle.Center ||
							backImageAlign == ChartImageAlignmentStyle.Right)
						{
							imageRect.Y = fillRect.Y + (fillRect.Height - imageRect.Height)/2;
						}
					}

				}

				// Fill background with brush
				this.FillRectangle( brush, rect.X, rect.Y, rect.Width + 1, rect.Height + 1);

				// Draw image
				this.DrawImage(image, 
					new Rectangle((int)Math.Round(imageRect.X),(int)Math.Round(imageRect.Y), (int)Math.Round(imageRect.Width), (int)Math.Round(imageRect.Height)),
					0, 0, image.Width, image.Height,
					GraphicsUnit.Pixel, 
					attrib);
			}
				// Draw rectangle
			else
			{
				if(backBrush != null && backImageTransparentColor != Color.Empty)
				{
					// Fill background with brush
					this.FillRectangle( backBrush, rect.X, rect.Y, rect.Width + 1, rect.Height + 1 );
				}
				this.FillRectangle( brush, rect.X, rect.Y, rect.Width + 1, rect.Height + 1 );
			}

			// Set pen alignment
			if(borderDashStyle != ChartDashStyle.NotSet)
			{
				if( borderWidth > 1 )
					this.DrawRectangle( _pen, rect.X, rect.Y, rect.Width + 1, rect.Height + 1 );
				else if( borderWidth == 1 )
					this.DrawRectangle( _pen, rect.X, rect.Y, rect.Width, rect.Height );
			}

			// Dispose Image and Gradient
			if( backGradientStyle != GradientStyle.None )
			{
				brush.Dispose();
			}
			if( backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled)
			{
				brush.Dispose();
			}
			if( backHatchStyle != ChartHatchStyle.None )
			{
				brush.Dispose();
			}

			// Set Old Smoothing Mode
			this.SmoothingMode = oldMode;
		}

		/// <summary>
		/// Fills graphics path with shadow using absolute coordinates.
		/// </summary>
		/// <param name="path">Graphics path to fill.</param>
		/// <param name="backColor">Color of rectangle</param>
		/// <param name="backHatchStyle">Hatch Style</param>
		/// <param name="backImage">Image URL</param>
		/// <param name="backImageWrapMode">Image Mode</param>
		/// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment.</param>
		/// <param name="backGradientStyle">Gradient AxisName</param>
		/// <param name="backSecondaryColor">End Gradient color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		/// <param name="penAlignment">Border is outside or inside rectangle</param>
		/// <param name="shadowOffset">Shadow offset.</param>
		/// <param name="shadowColor">Shadow color.</param>
		internal void DrawPathAbs( 
			GraphicsPath path, 
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle, 
			PenAlignment penAlignment,
			int shadowOffset,
			Color shadowColor)
		{
			// Draw patj shadow
			if(shadowOffset != 0 && shadowColor != Color.Transparent)
			{
				// Save graphics state and apply translate transformation
				GraphicsState graphicsState = this.Save();
				this.TranslateTransform(shadowOffset, shadowOffset);

				if(backColor == Color.Transparent &&
					backSecondaryColor.IsEmpty )
				{
					this.DrawPathAbs(
						path,
						Color.Transparent,
						ChartHatchStyle.None,
						String.Empty,
						ChartImageWrapMode.Scaled,
						Color.Empty,
						ChartImageAlignmentStyle.Center,
						GradientStyle.None,
						Color.Empty,
						shadowColor,
						borderWidth,
						borderDashStyle,
						PenAlignment.Center);
				}
				else
				{
					this.DrawPathAbs(
						path,
						shadowColor,
						ChartHatchStyle.None,
						String.Empty,
						ChartImageWrapMode.Scaled,
						Color.Empty,
						ChartImageAlignmentStyle.Center,
						GradientStyle.None,
						Color.Empty,
						Color.Transparent,
						0,
						ChartDashStyle.NotSet,
						PenAlignment.Center);
				}

				// Restore graphics state
				this.Restore(graphicsState);
			}

			// Draw path
			this.DrawPathAbs(
				path,
				backColor, 
				backHatchStyle, 
				backImage, 
				backImageWrapMode, 
				backImageTransparentColor,
				backImageAlign,
				backGradientStyle, 
				backSecondaryColor, 
				borderColor, 
				borderWidth, 
				borderDashStyle, 
				penAlignment);
		}

		/// <summary>
		/// Fills graphics path using absolute coordinates.
		/// </summary>
		/// <param name="path">Graphics path to fill.</param>
		/// <param name="backColor">Color of rectangle</param>
		/// <param name="backHatchStyle">Hatch Style</param>
		/// <param name="backImage">Image URL</param>
		/// <param name="backImageWrapMode">Image Mode</param>
		/// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment.</param>
		/// <param name="backGradientStyle">Gradient AxisName</param>
		/// <param name="backSecondaryColor">End Gradient color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		/// <param name="penAlignment">Border is outside or inside rectangle</param>
		internal void DrawPathAbs( GraphicsPath path, 
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle, 
			PenAlignment penAlignment )
		{
			Brush brush = null;
			Brush backBrush = null;

			// Color is empty
			if( backColor.IsEmpty ) 
				backColor = Color.White;

			if( backSecondaryColor.IsEmpty ) 
				backSecondaryColor = Color.White;

			if( borderColor.IsEmpty ) 
			{
				borderColor = Color.White;
				borderWidth = 0;
			}
		
			// Set pen properties
			_pen.Color = borderColor;
			_pen.Width = borderWidth;
			_pen.Alignment = penAlignment;
			_pen.DashStyle = GetPenStyle( borderDashStyle );

			if( backGradientStyle == GradientStyle.None )
			{
				// Set solid brush color.
				_solidBrush.Color = backColor;
				brush = _solidBrush;
			}
			else
			{
				// If a gradient type  is set create a brush with gradient
				RectangleF pathRect = path.GetBounds();
				pathRect.Inflate(new SizeF(2,2));
				brush = GetGradientBrush( 
					pathRect, 
					backColor, 
					backSecondaryColor, 
					backGradientStyle );
			}

			if( backHatchStyle != ChartHatchStyle.None )
			{
				brush = GetHatchBrush( backHatchStyle, backColor, backSecondaryColor );
			}

			if( backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled)
			{
				backBrush = brush;
				brush = GetTextureBrush(backImage, backImageTransparentColor, backImageWrapMode, backColor );
			}

			// For inset alignment resize fill rectangle
			RectangleF fillRect = path.GetBounds();
			
			// Draw rectangle image
			if( backImage.Length > 0 && (backImageWrapMode == ChartImageWrapMode.Unscaled || backImageWrapMode == ChartImageWrapMode.Scaled))
			{
				// Load image
System.Drawing.Image image = _common.ImageLoader.LoadImage( backImage );

				// Prepare image properties (transparent color)
				ImageAttributes attrib = new ImageAttributes();
				if(backImageTransparentColor != Color.Empty)
				{
					attrib.SetColorKey(backImageTransparentColor, backImageTransparentColor, ColorAdjustType.Default);
				}

				// Draw scaled image
				RectangleF imageRect = new RectangleF();
				imageRect.X = fillRect.X;
				imageRect.Y = fillRect.Y;
				imageRect.Width = fillRect.Width;
				imageRect.Height = fillRect.Height;

				// Draw unscaled image using align property
				if(backImageWrapMode == ChartImageWrapMode.Unscaled)
				{
                    SizeF imageSize = new SizeF();

                    ImageLoader.GetAdjustedImageSize(image, this.Graphics, ref imageSize);

					// Calculate image position
                    imageRect.Width = imageSize.Width;
                    imageRect.Height = imageSize.Height;

					// Adjust position with alignment property
					if(imageRect.Width < fillRect.Width)
					{
						if(backImageAlign == ChartImageAlignmentStyle.BottomRight ||
							backImageAlign == ChartImageAlignmentStyle.Right ||
							backImageAlign == ChartImageAlignmentStyle.TopRight)
						{
							imageRect.X = fillRect.Right - imageRect.Width;
						}
						else if(backImageAlign == ChartImageAlignmentStyle.Bottom ||
							backImageAlign == ChartImageAlignmentStyle.Center ||
							backImageAlign == ChartImageAlignmentStyle.Top)
						{
							imageRect.X = fillRect.X + (fillRect.Width - imageRect.Width)/2;
						}
					}
					if(imageRect.Height < fillRect.Height)
					{
						if(backImageAlign == ChartImageAlignmentStyle.BottomRight ||
							backImageAlign == ChartImageAlignmentStyle.Bottom ||
							backImageAlign == ChartImageAlignmentStyle.BottomLeft)
						{
							imageRect.Y = fillRect.Bottom - imageRect.Height;
						}
						else if(backImageAlign == ChartImageAlignmentStyle.Left ||
							backImageAlign == ChartImageAlignmentStyle.Center ||
							backImageAlign == ChartImageAlignmentStyle.Right)
						{
							imageRect.Y = fillRect.Y + (fillRect.Height - imageRect.Height)/2;
						}
					}

				}

				// Fill background with brush
				this.FillPath( brush, path );

				// Draw image
				Region oldClipRegion = this.Clip;
				this.Clip = new Region(path);
				this.DrawImage(image, 
					new Rectangle((int)Math.Round(imageRect.X),(int)Math.Round(imageRect.Y), (int)Math.Round(imageRect.Width), (int)Math.Round(imageRect.Height)),
					0, 0, image.Width, image.Height,
					GraphicsUnit.Pixel, 
					attrib);
				this.Clip = oldClipRegion;
			}
			
				// Draw rectangle
			else
			{
				if(backBrush != null && backImageTransparentColor != Color.Empty)
				{
					// Fill background with brush
					this.FillPath( backBrush, path);
				}
				this.FillPath( brush, path);
			}

			// Draw border
			if(borderColor != Color.Empty && borderWidth > 0 && borderDashStyle != ChartDashStyle.NotSet)
			{
				this.DrawPath( _pen, path );
			}
		}

		/// <summary>
		/// Creates brush with specified properties.
		/// </summary>
		/// <param name="rect">Gradient rectangle</param>
		/// <param name="backColor">Color of rectangle</param>
		/// <param name="backHatchStyle">Hatch style</param>
		/// <param name="backImage">Back Image</param>
		/// <param name="backImageWrapMode">Image mode</param>
		/// <param name="backImageTransparentColor">Image transparent color.</param>
		/// <param name="backGradientStyle">Gradient type </param>
		/// <param name="backSecondaryColor">Gradient End Color</param>
		/// <returns>New brush object.</returns>
		internal Brush CreateBrush( 
			RectangleF rect,
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor
			)
		{
			Brush brush = new SolidBrush(backColor);

			if( backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled)
			{
				brush = GetTextureBrush(backImage, backImageTransparentColor, backImageWrapMode, backColor );
			}
			else if( backHatchStyle != ChartHatchStyle.None )
			{
				brush = GetHatchBrush( backHatchStyle, backColor, backSecondaryColor );
			}
			else if( backGradientStyle != GradientStyle.None )
			{
				// If a gradient type  is set create a brush with gradient
				brush = GetGradientBrush( rect, backColor, backSecondaryColor, backGradientStyle );
			}

			return brush;
		}

		#endregion

		#region Coordinates converter

		/// <summary>
        /// This method takes a RectangleF structure that is using absolute coordinates 
        /// and returns a RectangleF object that uses relative coordinates.
		/// </summary>
        /// <param name="rectangle">RectangleF structure in absolute coordinates.</param>
        /// <returns>RectangleF structure in relative coordinates.</returns>
		public RectangleF GetRelativeRectangle( RectangleF rectangle )
		{
            // Check arguments
            if (rectangle == null)
                throw new ArgumentNullException("rectangle");
            
            RectangleF relative = RectangleF.Empty;

			// Convert absolute coordinates to relative coordinates
			relative.X = rectangle.X * 100F / ((float)(_width - 1)); 
			relative.Y = rectangle.Y * 100F / ((float)(_height - 1)); 
			relative.Width = rectangle.Width * 100F / ((float)(_width - 1)); 
			relative.Height = rectangle.Height * 100F / ((float)(_height - 1)); 

			// Return Relative coordinates
			return relative;
		}

		/// <summary>
        /// This method takes a PointF object that is using absolute coordinates 
        /// and returns a PointF object that uses relative coordinates.
		/// </summary>
		/// <param name="point">PointF object in absolute coordinates.</param>
		/// <returns>PointF object in relative coordinates.</returns>
		public PointF GetRelativePoint( PointF point )
		{
            // Check arguments
            if (point == null)
                throw new ArgumentNullException("point");
            
            PointF relative = PointF.Empty;

			// Convert absolute coordinates to relative coordinates
			relative.X = point.X * 100F / ((float)(_width - 1)); 
			relative.Y = point.Y * 100F / ((float)(_height - 1)); 
			
			// Return Relative coordinates
			return relative;
		}


		/// <summary>
        /// This method takes a SizeF object that uses absolute coordinates 
        /// and returns a SizeF object that uses relative coordinates.
		/// </summary>
		/// <param name="size">SizeF object in absolute coordinates.</param>
        /// <returns>SizeF object in relative coordinates.</returns>
		public SizeF GetRelativeSize( SizeF size )
		{
            // Check arguments
            if (size == null)
                throw new ArgumentNullException("size"); 
            
            SizeF relative = SizeF.Empty;

			// Convert absolute coordinates to relative coordinates
			relative.Width = size.Width * 100F / ((float)(_width - 1)); 
			relative.Height = size.Height * 100F / ((float)(_height - 1)); 
			
			// Return relative coordinates
			return relative;
		}

		/// <summary>
        /// This method takes a PointF object and converts its relative coordinates 
        /// to absolute coordinates.
		/// </summary>
        /// <param name="point">PointF object in relative coordinates.</param>
        /// <returns>PointF object in absolute coordinates.</returns>
		public PointF GetAbsolutePoint( PointF point )
		{
            // Check arguments
            if (point == null)
                throw new ArgumentNullException("point");

			PointF absolute = PointF.Empty;

			// Convert relative coordinates to absolute coordinates
			absolute.X = point.X * (_width - 1) / 100F; 
			absolute.Y = point.Y * (_height - 1) / 100F; 

			// Return Absolute coordinates
			return absolute;
		}

		/// <summary>
        /// This method takes a RectangleF structure and converts its relative coordinates 
        /// to absolute coordinates.
		/// </summary>
        /// <param name="rectangle">RectangleF object in relative coordinates.</param>
        /// <returns>RectangleF object in absolute coordinates.</returns>
		public RectangleF GetAbsoluteRectangle( RectangleF rectangle )
		{
            // Check arguments
            if (rectangle == null)
                throw new ArgumentNullException("rectangle");

			RectangleF absolute = RectangleF.Empty;

			// Convert relative coordinates to absolute coordinates
			absolute.X = rectangle.X * (_width - 1) / 100F; 
			absolute.Y = rectangle.Y * (_height - 1) / 100F; 
			absolute.Width = rectangle.Width * (_width - 1) / 100F; 
			absolute.Height = rectangle.Height * (_height - 1) / 100F; 

			// Return Absolute coordinates
			return absolute;
		}

		/// <summary>
        /// This method takes a SizeF object that uses relative coordinates
        /// and returns a SizeF object that uses absolute coordinates.
		/// </summary>
        /// <param name="size">SizeF object in relative coordinates.</param>
        /// <returns>SizeF object in absolute coordinates.</returns>
		public SizeF GetAbsoluteSize( SizeF size )
		{
            // Check arguments
            if (size == null)
                throw new ArgumentNullException("size"); 
            
            SizeF absolute = SizeF.Empty;

			// Convert relative coordinates to absolute coordinates
			absolute.Width = size.Width * (_width - 1) / 100F; 
			absolute.Height = size.Height * (_height - 1) / 100F; 
			
			// Return Absolute coordinates
			return absolute;
		}

	
		#endregion

		#region Border drawing helper methods

		/// <summary>
		/// Helper function which creates a rounded rectangle path.
		/// </summary>
		/// <param name="rect">Rectangle coordinates.</param>
		/// <param name="cornerRadius">Array of 4 corners radius.</param>
		/// <returns>Graphics path object.</returns>
		internal GraphicsPath CreateRoundedRectPath(RectangleF rect, float[] cornerRadius)
		{
			// Create rounded rectangle path
			GraphicsPath path = new GraphicsPath();
			path.AddLine(rect.X+cornerRadius[0], rect.Y, rect.Right-cornerRadius[1], rect.Y);
			path.AddArc(rect.Right-2f*cornerRadius[1], rect.Y, 2f*cornerRadius[1], 2f*cornerRadius[2], 270, 90);
			path.AddLine(rect.Right, rect.Y + cornerRadius[2], rect.Right, rect.Bottom - cornerRadius[3]);
			path.AddArc(rect.Right-2f*cornerRadius[4], rect.Bottom-2f*cornerRadius[3], 2f*cornerRadius[4], 2f*cornerRadius[3], 0, 90);
			path.AddLine(rect.Right-cornerRadius[4], rect.Bottom, rect.X + cornerRadius[5], rect.Bottom);
			path.AddArc(rect.X, rect.Bottom-2f*cornerRadius[6], 2f*cornerRadius[5], 2f*cornerRadius[6], 90, 90);
			path.AddLine(rect.X, rect.Bottom-cornerRadius[6], rect.X, rect.Y+cornerRadius[7]);
			path.AddArc(rect.X, rect.Y, 2f*cornerRadius[0], 2f*cornerRadius[7], 180, 90);

			return path;
		}

		/// <summary>
		/// Helper function which draws a shadow of the rounded rect.
		/// </summary>
		/// <param name="rect">Rectangle coordinates.</param>
		/// <param name="cornerRadius">Array of 4 corners radius.</param>
		/// <param name="radius">Rounding radius.</param>
		/// <param name="centerColor">Center color.</param>
		/// <param name="surroundColor">Surrounding color.</param>
		/// <param name="shadowScale">Shadow scale value.</param>
		internal void DrawRoundedRectShadowAbs(RectangleF rect, float[] cornerRadius, float radius, Color centerColor, Color surroundColor, float shadowScale)
		{
			// Create rounded rectangle path
			GraphicsPath path = CreateRoundedRectPath(rect, cornerRadius);

			// Create gradient brush
			PathGradientBrush shadowBrush = new PathGradientBrush(path);
			shadowBrush.CenterColor = centerColor;

			// Set the color along the entire boundary of the path
			Color[] colors = {surroundColor};
			shadowBrush.SurroundColors = colors;
			shadowBrush.CenterPoint = new PointF(rect.X + rect.Width/2f, rect.Y + rect.Height/2f);

			// Define brush focus scale
			PointF focusScale = new PointF(1-shadowScale*radius/rect.Width, 1-shadowScale*radius/rect.Height);
			shadowBrush.FocusScales = focusScale;

			// Draw rounded rectangle
			this.FillPath(shadowBrush, path);

			if( path != null )
			{
				path.Dispose();
			}
		}

		/// <summary>
		/// Draws 3D border in absolute coordinates.
		/// </summary>
		/// <param name="borderSkin">Border skin object.</param>
		/// <param name="rect">Rectangle of the border (pixel coordinates).</param>
		/// <param name="backColor">Color of rectangle</param>
		/// <param name="backHatchStyle">Hatch style</param>
		/// <param name="backImage">Back Image</param>
		/// <param name="backImageWrapMode">Image mode</param>
		/// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
		/// <param name="backGradientStyle">Gradient type </param>
		/// <param name="backSecondaryColor">Gradient End Color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		internal void Draw3DBorderRel(
			BorderSkin borderSkin, 
			RectangleF rect, 
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle)
		{
			Draw3DBorderAbs(borderSkin, GetAbsoluteRectangle(rect), backColor, backHatchStyle, 
				backImage, backImageWrapMode, backImageTransparentColor, backImageAlign, backGradientStyle, 
				backSecondaryColor, borderColor, borderWidth, borderDashStyle);
		}


		/// <summary>
		/// Draws 3D border in absolute coordinates.
		/// </summary>
		/// <param name="borderSkin">Border skin object.</param>
		/// <param name="absRect">Rectangle of the border (pixel coordinates).</param>
		/// <param name="backColor">Color of rectangle</param>
		/// <param name="backHatchStyle">Hatch style</param>
		/// <param name="backImage">Back Image</param>
		/// <param name="backImageWrapMode">Image mode</param>
		/// <param name="backImageTransparentColor">Image transparent color.</param>
        /// <param name="backImageAlign">Image alignment</param>
		/// <param name="backGradientStyle">Gradient type </param>
		/// <param name="backSecondaryColor">Gradient End Color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		internal void Draw3DBorderAbs(
			BorderSkin borderSkin, 
			RectangleF absRect, 
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			ChartImageAlignmentStyle backImageAlign,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle)
		{
			// Check input parameters
			if(_common == null || borderSkin.SkinStyle == BorderSkinStyle.None || absRect.Width == 0 || absRect.Height == 0)
			{
				return;
			}

			// Find required border interface
			IBorderType	borderTypeInterface = _common.BorderTypeRegistry.GetBorderType(borderSkin.SkinStyle.ToString());
			if(borderTypeInterface != null)
			{
                borderTypeInterface.Resolution = this.Graphics.DpiX;
				// Draw border
				borderTypeInterface.DrawBorder(this, borderSkin, absRect, backColor, backHatchStyle, backImage, backImageWrapMode, 
					backImageTransparentColor, backImageAlign, backGradientStyle, backSecondaryColor, 
					borderColor, borderWidth, borderDashStyle);
			}
		}

		#endregion

		#region Pie Method

		/// <summary>
		/// Helper function that retrieves pie drawing style.
		/// </summary>
		/// <param name="point">Data point to get the drawing style for.</param>
		/// <returns>pie drawing style.</returns>
		internal static PieDrawingStyle GetPieDrawingStyle(DataPoint point)
		{
			// Get column drawing style
			PieDrawingStyle pieDrawingStyle = PieDrawingStyle.Default;
			string styleName = point[CustomPropertyName.PieDrawingStyle];
			if(styleName != null)
			{
				if(String.Compare(styleName, "Default", StringComparison.OrdinalIgnoreCase) == 0)
				{
					pieDrawingStyle = PieDrawingStyle.Default;
				}
                else if (String.Compare(styleName, "SoftEdge", StringComparison.OrdinalIgnoreCase) == 0)
				{
					pieDrawingStyle = PieDrawingStyle.SoftEdge;
				}
                else if (String.Compare(styleName, "Concave", StringComparison.OrdinalIgnoreCase) == 0)
				{
					pieDrawingStyle = PieDrawingStyle.Concave;
				}					
				else
				{
					throw( new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid( styleName, "PieDrawingStyle")));
				}
			}
			return pieDrawingStyle;
		}

		/// <summary>
		/// Draws a pie defined by an ellipse specified by a Rectangle structure and two radial lines.
		/// </summary>
		/// <param name="rect">Rectangle structure that represents the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
		/// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
		/// <param name="sweepAngle">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
		/// <param name="backColor">Fill color</param>
		/// <param name="backHatchStyle">Fill Hatch Style</param>
		/// <param name="backImage">Fill texture</param>
		/// <param name="backImageWrapMode">Texture image mode</param>
		/// <param name="backImageTransparentColor">Texture transparent color</param>
		/// <param name="backGradientStyle">Fill Gradient type </param>
		/// <param name="backSecondaryColor">Fill Gradient Second Color</param>
		/// <param name="borderColor">Border Color</param>
		/// <param name="borderWidth">Border Width</param>
		/// <param name="borderDashStyle">Border Style</param>
		/// <param name="shadow">True if shadow is active</param>
		/// <param name="doughnut">True if Doughnut is drawn instead of pie</param>
		/// <param name="doughnutRadius">Internal radius of the doughnut</param>
		/// <param name="pieDrawingStyle">Pie drawing style.</param>
		internal void DrawPieRel( 
			RectangleF rect, 
			float startAngle,
			float sweepAngle,
			Color backColor, 
			ChartHatchStyle backHatchStyle, 
			string backImage, 
			ChartImageWrapMode backImageWrapMode, 
			Color backImageTransparentColor,
			GradientStyle backGradientStyle, 
			Color backSecondaryColor, 
			Color borderColor, 
			int borderWidth, 
			ChartDashStyle borderDashStyle, 
			bool shadow,
			bool doughnut,
			float doughnutRadius,
			PieDrawingStyle pieDrawingStyle
			)
		{
			Pen borderPen = null;	// Pen
			Brush fillBrush;		// Brush

			// Get absolute rectangle
			RectangleF absRect = GetAbsoluteRectangle( rect );

			if( doughnutRadius == 100.0 )
			{
				doughnut = false;
			}

			if( doughnutRadius == 0.0 )
			{
				return;
			}

			// Create Brush
			if( backHatchStyle != ChartHatchStyle.None )
			{
				// Create Hatch Brush
				fillBrush = GetHatchBrush( backHatchStyle, backColor, backSecondaryColor );
			}
			else if( backGradientStyle != GradientStyle.None ) 
			{ 
				// Create gradient brush
				if( backGradientStyle == GradientStyle.Center )
				{
					fillBrush = GetPieGradientBrush( absRect, backColor, backSecondaryColor );
				}
				else
				{
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        path.AddPie(absRect.X, absRect.Y, absRect.Width, absRect.Height, startAngle, sweepAngle);
                        fillBrush = GetGradientBrush(path.GetBounds(), backColor, backSecondaryColor, backGradientStyle);
                    }
				}
			}
			else if( backImage.Length > 0 && backImageWrapMode != ChartImageWrapMode.Unscaled && backImageWrapMode != ChartImageWrapMode.Scaled )
			{ 
				// Create textured brush
				fillBrush = GetTextureBrush(backImage, backImageTransparentColor, backImageWrapMode, backColor );
			}
			else
			{
				// Create solid brush
				fillBrush = new SolidBrush( backColor );
			}

			// Create border Pen
			borderPen = new Pen( borderColor, borderWidth );
			
			// Set a border line style
			borderPen.DashStyle = GetPenStyle( borderDashStyle );

			// Use rounded line joins
			borderPen.LineJoin = LineJoin.Round;

			// Draw Doughnut
			if( doughnut )
			{
                using (GraphicsPath path = new GraphicsPath())
                {

                    path.AddArc(absRect.X + absRect.Width * doughnutRadius / 200 - 1, absRect.Y + absRect.Height * doughnutRadius / 200 - 1, absRect.Width - absRect.Width * doughnutRadius / 100 + 2, absRect.Height - absRect.Height * doughnutRadius / 100 + 2, startAngle, sweepAngle);
                    path.AddArc(absRect.X, absRect.Y, absRect.Width, absRect.Height, startAngle + sweepAngle, -sweepAngle);

                    path.CloseFigure();

                    this.FillPath(fillBrush, path);


                    // Draw Pie gradien effects
                    this.DrawPieGradientEffects(pieDrawingStyle, absRect, startAngle, sweepAngle, doughnutRadius);

                    // Draw Doughnut Border
                    if (!shadow &&
                        borderWidth > 0 &&
                        borderDashStyle != ChartDashStyle.NotSet)
                    {
                        this.DrawPath(borderPen, path);
                    }
                }
			}
			else // Draw Pie
			{

				// Draw Soft shadow for pie slice
				if( shadow && softShadows )
				{
					DrawPieSoftShadow( startAngle, sweepAngle, absRect, backColor );
				}
				else 
				{
					// Fill Pie for normal shadow or colored pie slice
					this.FillPie( fillBrush, absRect.X, absRect.Y, absRect.Width, absRect.Height, startAngle, sweepAngle );

					// Draw Pie gradien effects
					this.DrawPieGradientEffects( pieDrawingStyle, absRect, startAngle, sweepAngle, -1f);
				}

				
				// Draw Pie Border
				if( !shadow  &&
					borderWidth > 0 &&
					borderDashStyle != ChartDashStyle.NotSet)
				{
					this.DrawPie( borderPen, absRect.X, absRect.Y, absRect.Width, absRect.Height, startAngle, sweepAngle );
				}
			}

			// Dispose graphics objects
			if( borderPen != null )
			{
				borderPen.Dispose();
			}

			if( fillBrush != null )
			{
				fillBrush.Dispose();
			}
		}

		private void DrawPieGradientEffects( 
			PieDrawingStyle pieDrawingStyle, 
			RectangleF position, 
			float startAngle, 
			float sweepAngle,
			float doughnutRadius)
		{
			if(pieDrawingStyle == PieDrawingStyle.Concave)
			{
				// Calculate the size of the shadow. Note: For Doughnut chart shadow is drawn 
				// twice on the outside and inside radius.
				float minSize = (float)Math.Min(position.Width, position.Height);
				float shadowSize = minSize * 0.05f;
			
				// Create brush path
				RectangleF gradientPath = position;
				gradientPath.Inflate(-shadowSize, -shadowSize);
				using(GraphicsPath brushPath = new GraphicsPath())
				{
					brushPath.AddEllipse(gradientPath);

					// Create shadow path
					using(GraphicsPath path = new GraphicsPath())
					{
						if(doughnutRadius < 0f)
						{
							path.AddPie(Rectangle.Round(gradientPath), startAngle, sweepAngle);
						}
						else
						{
							path.AddArc( 
								gradientPath.X + position.Width * doughnutRadius /200 - 1 - shadowSize, 
								gradientPath.Y + position.Height * doughnutRadius /200 - 1 - shadowSize, 
								gradientPath.Width - position.Width * doughnutRadius / 100 + 2 + 2f * shadowSize, 
								gradientPath.Height - position.Height * doughnutRadius / 100 + 2 + 2f * shadowSize, 
								startAngle, 
								sweepAngle );
							path.AddArc( gradientPath.X, gradientPath.Y, gradientPath.Width, gradientPath.Height, startAngle + sweepAngle, -sweepAngle );
						}

						// Create linear gradient brush
						gradientPath.Inflate(1f, 1f);
						using(LinearGradientBrush brush = new LinearGradientBrush(
								  gradientPath, 
								  Color.Red,
								  Color.Green, 
								  LinearGradientMode.Vertical) )
						{
							ColorBlend colorBlend = new ColorBlend(3);
							colorBlend.Colors[0] = Color.FromArgb(100, Color.Black);
							colorBlend.Colors[1] = Color.Transparent;
							colorBlend.Colors[2] = Color.FromArgb(140, Color.White);
							colorBlend.Positions[0] = 0f;
							colorBlend.Positions[1] = 0.5f;
							colorBlend.Positions[2] = 1f;
							brush.InterpolationColors = colorBlend;

							// Fill shadow
							this.FillPath( brush, path );

						}
					}
				}			
			}
			else if(pieDrawingStyle == PieDrawingStyle.SoftEdge)
			{
				// Calculate the size of the shadow. Note: For Doughnut chart shadow is drawn 
				// twice on the outside and inside radius.
				float minSize = (float)Math.Min(position.Width, position.Height);
				float shadowSize = minSize/10f;
				if(doughnutRadius > 0f)
				{
					shadowSize = (minSize * doughnutRadius / 100f) / 8f;
				}

				// Create brush path
				using(GraphicsPath brushPath = new GraphicsPath())
				{
					brushPath.AddEllipse(position);

					// Create shadow path
					using(GraphicsPath path = new GraphicsPath())
					{
						path.AddArc( position.X + shadowSize, position.Y + shadowSize, position.Width - shadowSize * 2f, position.Height - shadowSize * 2f, startAngle, sweepAngle );
						path.AddArc( position.X, position.Y, position.Width, position.Height, startAngle + sweepAngle, -sweepAngle );
						path.CloseFigure();

						// Create shadow brush
						using( PathGradientBrush brush = new PathGradientBrush(brushPath) )
						{
							brush.CenterColor = Color.Transparent;
							brush.SurroundColors = new Color[] { Color.FromArgb(100, Color.Black) };

							Blend blend = new Blend(3);
							blend.Positions[0] = 0f;
							blend.Factors[0] = 0f;
							blend.Positions[1] = shadowSize / (minSize / 2f);
							blend.Factors[1] = 1f;
							blend.Positions[2] = 1f;
							blend.Factors[2] = 1f;
							brush.Blend = blend;

							// Fill shadow
							this.FillPath( brush, path );
						}
					}

					// Draw inner shadow for the doughnut chart
					if(doughnutRadius > 0f)
					{
						// Create brush path
						using(GraphicsPath brushInsidePath = new GraphicsPath())
						{
							RectangleF innerPosition = position;
							innerPosition.Inflate(- position.Width * doughnutRadius / 200f + shadowSize, -position.Height * doughnutRadius / 200f + shadowSize);
							brushInsidePath.AddEllipse(innerPosition);

							// Create shadow path
							using(GraphicsPath path = new GraphicsPath())
							{
								path.AddArc( innerPosition.X + shadowSize, innerPosition.Y + shadowSize, innerPosition.Width - 2f * shadowSize, innerPosition.Height - 2f * shadowSize, startAngle, sweepAngle );
								path.AddArc( innerPosition.X, innerPosition.Y, innerPosition.Width, innerPosition.Height, startAngle + sweepAngle, -sweepAngle );
								path.CloseFigure();

								// Create shadow brush
								using( PathGradientBrush brushInner = new PathGradientBrush(brushInsidePath) )
								{
									brushInner.CenterColor = Color.FromArgb(100, Color.Black);
									brushInner.SurroundColors = new Color[] { Color.Transparent };

									Blend blend = new Blend(3);
									blend.Positions[0] = 0f;
									blend.Factors[0] = 0f;
									blend.Positions[1] = shadowSize / (innerPosition.Width / 2f);
									blend.Factors[1] = 1f;
									blend.Positions[2] = 1f;
									blend.Factors[2] = 1f;
									brushInner.Blend = blend;

									// Fill shadow
									this.FillPath( brushInner, path );
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// The soft shadow of the pie 
		/// </summary>
		/// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
		/// <param name="sweepAngle">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
		/// <param name="absRect">Rectangle of the pie in absolute coordinates</param>
		/// <param name="backColor">Fill color</param>
		private void DrawPieSoftShadow( float startAngle, float sweepAngle, RectangleF absRect, Color backColor )
		{
			GraphicsPath path = new GraphicsPath();
			
			path.AddEllipse( absRect.X, absRect.Y, absRect.Width, absRect.Height );

			PathGradientBrush brush = new PathGradientBrush( path );
		
			Color[] colors = {
								Color.FromArgb( 0, backColor ),
								Color.FromArgb( backColor.A, backColor ),   
								Color.FromArgb( backColor.A, backColor )}; 

			float[] relativePositions = {
											0f,       
											0.05f,     
											1.0f};    // at the center point.

			ColorBlend colorBlend = new ColorBlend();
			colorBlend.Colors = colors;
			colorBlend.Positions = relativePositions;
			brush.InterpolationColors = colorBlend;

			this.FillPie( brush, absRect.X, absRect.Y, absRect.Width, absRect.Height, startAngle, sweepAngle );
		}

		#endregion

		#region Arrow Methods

		/// <summary>
		/// Draw Arrow.
		/// </summary>
		/// <param name="position">Position of the arrow</param>
		/// <param name="orientation">Orientation of the arrow - left, right, top, bottom </param>
        /// <param name="type">Arrow style: Triangle, Sharp Triangle, Lines</param>
		/// <param name="color">Color of the arrow</param>
		/// <param name="lineWidth">Line width</param>
		/// <param name="lineDashStyle">Line Dash style</param>
		/// <param name="shift">Distance from the chart area</param>
		/// <param name="size">Arrow size</param>
		internal void DrawArrowRel( PointF position, ArrowOrientation orientation, AxisArrowStyle type, Color color, int lineWidth, ChartDashStyle lineDashStyle, double shift, double size )
		{
			// Check if arrow should be drawn
			if(type == AxisArrowStyle.None)
			{
				return;
			}

			// Set a color
            using (SolidBrush brush = new SolidBrush(color))
            {
                PointF endPoint = PointF.Empty; // End point of axis line
                PointF[] points; // arrow points
                PointF absolutePosition; // Absolute position of axis

                absolutePosition = GetAbsolutePoint(position);

                // Arrow type is triangle
                if (type == AxisArrowStyle.Triangle)
                {
                    points = GetArrowShape(absolutePosition, orientation, shift, size, type, ref endPoint);

                    endPoint = GetRelativePoint(endPoint);

                    // Draw center line
                    DrawLineRel(color, lineWidth, lineDashStyle, position, endPoint);

                    // Draw arrow
                    this.FillPolygon(brush, points);

                }
                // Arrow type is sharp triangle
                else if (type == AxisArrowStyle.SharpTriangle)
                {
                    points = GetArrowShape(absolutePosition, orientation, shift, size, type, ref endPoint);

                    endPoint = GetRelativePoint(endPoint);

                    // Draw center line
                    DrawLineRel(color, lineWidth, lineDashStyle, position, endPoint);

                    // Draw arrow
                    this.FillPolygon(brush, points);

                }
                // Arrow type is 'Lines'
                else if (type == AxisArrowStyle.Lines)
                {
                    points = GetArrowShape(absolutePosition, orientation, shift, size, type, ref endPoint);

                    points[0] = GetRelativePoint(points[0]);
                    points[1] = GetRelativePoint(points[1]);
                    points[2] = GetRelativePoint(points[2]);

                    endPoint = GetRelativePoint(endPoint);

                    // Draw arrow
                    DrawLineRel(color, lineWidth, lineDashStyle, position, endPoint);
                    DrawLineRel(color, lineWidth, lineDashStyle, points[0], points[2]);
                    DrawLineRel(color, lineWidth, lineDashStyle, points[1], points[2]);

                }
            }
		}

		/// <summary>
		/// This function calculates points for polygon, which represents 
		/// shape of an arrow. There are four different orientations 
		/// of arrow and three arrow types.
		/// </summary>
		/// <param name="position">Arrow position</param>
		/// <param name="orientation">Arrow orientation ( Left, Right, Top, Bottom )</param>
		/// <param name="shift">Distance from chart area to the arrow</param>
		/// <param name="size">Arrow size</param>
        /// <param name="type">Arrow style.</param>
		/// <param name="endPoint">End point of the axis and the beginning of arrow</param>
		/// <returns>Polygon points</returns>
		private PointF[] GetArrowShape( PointF position, ArrowOrientation orientation, double shift, double size, AxisArrowStyle type, ref PointF endPoint )
		{
			PointF[] points = new PointF[3]; // Polygon points
			double sharp; // Size for sharp triangle

			// Four different orientations for AxisArrowStyle
			switch( orientation )
			{
					// Top orientation
				case ArrowOrientation.Top:
					// Get absolute size for arrow
					// Arrow size has to have the same shape when width and height 
					// are changed. When the picture is resized, width of the chart 
					// picture is used only for arrow size.
					size = GetAbsoluteSize( new SizeF((float)size, (float)size) ).Width;
					shift = GetAbsoluteSize( new SizeF((float)shift,(float)shift) ).Height;

					// Size for sharp and regular triangle
					if( type == AxisArrowStyle.SharpTriangle )
						sharp = size * 4;
					else
						sharp = size * 2;

					points[0].X = position.X - (float)size;
					points[0].Y = position.Y - (float)shift;
					points[1].X = position.X + (float)size;
					points[1].Y = position.Y - (float)shift;
					points[2].X = position.X;
					points[2].Y = position.Y - (float)shift - (float)sharp;
					// End of the axis line
					endPoint.X = position.X;
					if( type == AxisArrowStyle.SharpTriangle || type == AxisArrowStyle.Triangle )
						endPoint.Y = points[1].Y;
					else
						endPoint.Y = points[2].Y;
					
					break;
					// Bottom orientation
				case ArrowOrientation.Bottom:
					// Get absolute size for arrow
					// Arrow size has to have the same shape when width and height 
					// are changed. When the picture is resized, width of the chart 
					// picture is used only for arrow size.
					size = GetAbsoluteSize( new SizeF((float)size, (float)size) ).Width;
					shift = GetAbsoluteSize( new SizeF((float)shift,(float)shift) ).Height;

					// Size for sharp and regular triangle
					if( type == AxisArrowStyle.SharpTriangle )
						sharp = size * 4;
					else
						sharp = size * 2;

					points[0].X = position.X - (float)size;
					points[0].Y = position.Y + (float)shift;
					points[1].X = position.X + (float)size;
					points[1].Y = position.Y + (float)shift;
					points[2].X = position.X;
					points[2].Y = position.Y + (float)shift + (float)sharp;
					// End of the axis line
					endPoint.X = position.X;
					if( type == AxisArrowStyle.SharpTriangle || type == AxisArrowStyle.Triangle )
						endPoint.Y = points[1].Y;
					else
						endPoint.Y = points[2].Y;
					break;
					// Left orientation
				case ArrowOrientation.Left:
					// Get absolute size for arrow
					size = GetAbsoluteSize( new SizeF((float)size, (float)size) ).Width;
					shift = GetAbsoluteSize( new SizeF((float)shift,(float)shift) ).Width;

					// Size for sharp and regular triangle
					if( type == AxisArrowStyle.SharpTriangle )
						sharp = size * 4;
					else
						sharp = size * 2;

					points[0].Y = position.Y - (float)size;
					points[0].X = position.X - (float)shift;
					points[1].Y = position.Y + (float)size;
					points[1].X = position.X - (float)shift;
					points[2].Y = position.Y;
					points[2].X = position.X - (float)shift - (float)sharp;
					// End of the axis line
					endPoint.Y = position.Y;
					if( type == AxisArrowStyle.SharpTriangle || type == AxisArrowStyle.Triangle )
						endPoint.X = points[1].X;
					else
						endPoint.X = points[2].X;
					break;
					// Right orientation
				case ArrowOrientation.Right:
					// Get absolute size for arrow
					size = GetAbsoluteSize( new SizeF((float)size, (float)size) ).Width;
					shift = GetAbsoluteSize( new SizeF((float)shift,(float)shift) ).Width;

					// Size for sharp and regular triangle
					if( type == AxisArrowStyle.SharpTriangle )
						sharp = size * 4;
					else
						sharp = size * 2;

					points[0].Y = position.Y - (float)size;
					points[0].X = position.X + (float)shift;
					points[1].Y = position.Y + (float)size;
					points[1].X = position.X + (float)shift;
					points[2].Y = position.Y;
					points[2].X = position.X + (float)shift + (float)sharp;
					// End of the axis line
					endPoint.Y = position.Y;
					if( type == AxisArrowStyle.SharpTriangle || type == AxisArrowStyle.Triangle )
						endPoint.X = points[1].X;
					else
						endPoint.X = points[2].X;
					break;
			}

			return points;
		}

		#endregion
		
		#region Other methods and properties

		/// <summary>
		/// Helper function that retrieves bar drawing style.
		/// </summary>
		/// <param name="point">Data point to get the drawing style for.</param>
		/// <returns>Bar drawing style.</returns>
		internal static BarDrawingStyle GetBarDrawingStyle(DataPoint point)
		{
			// Get column drawing style
			BarDrawingStyle barDrawingStyle = BarDrawingStyle.Default;
			string styleName = point[CustomPropertyName.DrawingStyle];
			if(styleName != null)
			{
				if(String.Compare(styleName, "Default", StringComparison.OrdinalIgnoreCase) == 0)
				{
					barDrawingStyle = BarDrawingStyle.Default;
				}
                else if (String.Compare(styleName, "Cylinder", StringComparison.OrdinalIgnoreCase) == 0)
				{
					barDrawingStyle = BarDrawingStyle.Cylinder;
				}
                else if (String.Compare(styleName, "Emboss", StringComparison.OrdinalIgnoreCase) == 0)
				{
					barDrawingStyle = BarDrawingStyle.Emboss;
				}
                else if (String.Compare(styleName, "LightToDark", StringComparison.OrdinalIgnoreCase) == 0)
				{
					barDrawingStyle = BarDrawingStyle.LightToDark;
				}
                else if (String.Compare(styleName, "Wedge", StringComparison.OrdinalIgnoreCase) == 0)
				{
					barDrawingStyle = BarDrawingStyle.Wedge;
				}
				else
				{
                    throw (new InvalidOperationException(SR.ExceptionCustomAttributeValueInvalid(styleName, "DrawingStyle")));
				}
			}
			return barDrawingStyle;
		}


		/// <summary>
		/// Find rounding coordinates for a rectangle
		/// </summary>
		/// <param name="rect">Rectangle which has to be rounded</param>
		/// <returns>Rounded rectangle</returns>
		internal RectangleF Round(RectangleF rect)
		{
			float	left = (float)Math.Round( (double)rect.Left );
			float	right = (float)Math.Round( (double)rect.Right );
			float	top = (float)Math.Round( (double)rect.Top );
			float	bottom = (float)Math.Round( (double)rect.Bottom );

			return new RectangleF( left, top, right - left, bottom - top ); 
		}
		
		/// <summary>
        /// This method takes a given axis value for a specified axis and returns the relative pixel value.
		/// </summary>
		/// <param name="chartAreaName">Chart area name.</param>
        /// <param name="axis">An AxisName enum value that identifies the relevant axis.</param>
        /// <param name="axisValue">The axis value that needs to be converted to a relative pixel value.</param>
		/// <returns>The converted axis value, in relative pixel coordinates.</returns>
		public double GetPositionFromAxis( string chartAreaName, AxisName axis, double axisValue )
		{
			if( axis == AxisName.X )
				return _common.ChartPicture.ChartAreas[chartAreaName].AxisX.GetLinearPosition( axisValue );

			if( axis == AxisName.X2 )
				return _common.ChartPicture.ChartAreas[chartAreaName].AxisX2.GetLinearPosition( axisValue );

			if( axis == AxisName.Y )
				return _common.ChartPicture.ChartAreas[chartAreaName].AxisY.GetLinearPosition( axisValue );

			if( axis == AxisName.Y2 )
				return _common.ChartPicture.ChartAreas[chartAreaName].AxisY2.GetLinearPosition( axisValue );

			return 0;
		}

		/// <summary>
		/// Set picture size
		/// </summary>
		/// <param name="width">Width</param>
		/// <param name="height">Height</param>
		internal void SetPictureSize( int width, int height )
		{
			this._width = width;
			this._height = height;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="common">Common elements class</param>
		internal ChartGraphics(CommonElements common)
		{
			// Set Common elements
			this._common = common;
            base.Common = common;
			// Create a pen object
			_pen = new Pen(Color.Black);

			// Create a brush object
			_solidBrush = new SolidBrush(Color.Black);
		}

		/// <summary>
		/// Chart Graphics Anti alias mode
		/// </summary>
		internal AntiAliasingStyles AntiAliasing
		{
			get
			{
				return _antiAliasing;
			}
			set
			{
				_antiAliasing = value;

				// Graphics mode not set
				if( Graphics == null )
					return;

				// Convert Chart's anti alias enumeration to GDI+ SmoothingMode
				if( (_antiAliasing & AntiAliasingStyles.Graphics) == AntiAliasingStyles.Graphics )
				{
					this.SmoothingMode = SmoothingMode.AntiAlias;
				}
				else
				{
					this.SmoothingMode = SmoothingMode.None;
				}
			}
		}

        /// <summary>
        /// Gets reusable pen.
        /// </summary>
        internal Pen Pen
        {
            get { return _pen; }
        }

		/// <summary>
		/// Sets the clipping region of this Graphics object 
		/// to the rectangle specified by a RectangleF structure.
		/// </summary>
		/// <param name="region">Region rectangle</param>
		internal void SetClip( RectangleF region )
		{
			this.SetClipAbs( GetAbsoluteRectangle( region ) );
		}
	
		#endregion

		#region Color manipulation methods

        /// <summary>
        /// Returns the gradient color from a gradient position.
        /// </summary>
        /// <param name="beginColor">The color from the gradient beginning</param>
        /// <param name="endColor">The color from the gradient end.</param>
        /// <param name="relativePosition">The relative position.</param>
        /// <returns>Result color.</returns>
        static internal Color GetGradientColor(Color beginColor, Color endColor, double relativePosition)
		{
			// Check if position is valid
			if(relativePosition < 0 || relativePosition > 1 || double.IsNaN(relativePosition))
			{
				return beginColor;
			}
			
			// Extracts Begin color
			int nBRed = beginColor.R;
			int nBGreen = beginColor.G;
			int nBBlue = beginColor.B;

			// Extracts End color
			int nERed = endColor.R;
			int nEGreen = endColor.G;
			int nEBlue = endColor.B;

			// Gradient positions for Red, Green and Blue colors
			double dRRed = nBRed + (nERed - nBRed) * relativePosition;
			double dRGreen = nBGreen + (nEGreen - nBGreen) * relativePosition;
			double dRBlue = nBBlue + (nEBlue - nBBlue) * relativePosition;

			// Make sure colors are in range from 0 to 255
			if(dRRed > 255.0)
				dRRed = 255.0;
			if(dRRed < 0.0)
				dRRed = 0.0;
			if(dRGreen > 255.0)
				dRGreen = 255.0;
			if(dRGreen < 0.0)
				dRGreen = 0.0;
			if(dRBlue > 255.0)
				dRBlue = 255.0;
			if(dRBlue < 0.0)
				dRBlue = 0.0;

			// Return a gradient color position
			return Color.FromArgb(beginColor.A, (int)dRRed, (int)dRGreen, (int)dRBlue);
		}

		#endregion

        #region RightToLeft
        /// <summary>
        /// Returns chart right to left flag 
        /// </summary>
        internal bool IsRightToLeft
        {
            get
            {
                if (Common == null)
                {
                    return false;
                }
                return Common.ChartPicture.RightToLeft == RightToLeft.Yes;
            }
        }
        
        #endregion //RightToLeft

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {   
                // Free up managed resources
                if (_pen != null)
                {
                    _pen.Dispose();
                    _pen = null;
                }
                if (_solidBrush != null)
                {
                    _solidBrush.Dispose();
                    _solidBrush = null;
                }
                if (_myMatrix != null)
                {
                    _myMatrix.Dispose();
                    _myMatrix = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
