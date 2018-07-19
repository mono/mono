//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		GdiGraphics.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	GdiGraphics
//
//  Purpose:	GdiGraphics class is chart GDI+ rendering engine. It 
//              implements IChartRenderingEngine interface by mapping 
//              its methods to the drawing methods of GDI+. This 
//              rendering engine do not support animation.
//
//	Reviwed:	AG - Jul 15, 2003
//              AG - Microsoft 14, 2007
//
//===================================================================

#region Used namespaces

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

#if Microsoft_CONTROL

using System.Windows.Forms.DataVisualization.Charting.Utilities;
using System.Windows.Forms.DataVisualization.Charting.Borders3D;
#else
	//using System.Web.UI.DataVisualization.Charting.Utilities;
	//using System.Web.UI.DataVisualization.Charting.Borders3D;
#endif

#endregion

#if Microsoft_CONTROL
    namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	/// <summary>
    /// GdiGraphics class is chart GDI+ rendering engine.
	/// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gdi")]
    internal class GdiGraphics : IChartRenderingEngine
	{
		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		public GdiGraphics()
		{
		}

		#endregion // Constructor

		#region Drawing Methods

		/// <summary>
		/// Draws a line connecting two PointF structures.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
		/// <param name="pt1">PointF structure that represents the first point to connect.</param>
		/// <param name="pt2">PointF structure that represents the second point to connect.</param>
		public void DrawLine(
			Pen pen,
			PointF pt1,
			PointF pt2
			)
		{
			_graphics.DrawLine( pen, pt1, pt2 );
		}

		/// <summary>
		/// Draws a line connecting the two points specified by coordinate pairs.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
		/// <param name="x1">x-coordinate of the first point.</param>
		/// <param name="y1">y-coordinate of the first point.</param>
		/// <param name="x2">x-coordinate of the second point.</param>
		/// <param name="y2">y-coordinate of the second point.</param>
		public void DrawLine(
			Pen pen,
			float x1,
			float y1,
			float x2,
			float y2
			)
		{
			_graphics.DrawLine( pen, x1, y1, x2, y2 );
		}

		/// <summary>
		/// Draws the specified portion of the specified Image object at the specified location and with the specified size.
		/// </summary>
		/// <param name="image">Image object to draw.</param>
		/// <param name="destRect">Rectangle structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle.</param>
		/// <param name="srcX">x-coordinate of the upper-left corner of the portion of the source image to draw.</param>
		/// <param name="srcY">y-coordinate of the upper-left corner of the portion of the source image to draw.</param>
		/// <param name="srcWidth">Width of the portion of the source image to draw.</param>
		/// <param name="srcHeight">Height of the portion of the source image to draw.</param>
		/// <param name="srcUnit">Member of the GraphicsUnit enumeration that specifies the units of measure used to determine the source rectangle.</param>
		/// <param name="imageAttr">ImageAttributes object that specifies recoloring and gamma information for the image object.</param>
		public void DrawImage(
			System.Drawing.Image image,
			Rectangle destRect,
			int srcX,
			int srcY,
			int srcWidth,
			int srcHeight,
			GraphicsUnit srcUnit,
			ImageAttributes imageAttr
			)
		{
			_graphics.DrawImage( 
					image,
					destRect,
					srcX,
					srcY,
					srcWidth,
					srcHeight,
					srcUnit,
					imageAttr
				);
		}

		/// <summary>
		/// Draws an ellipse defined by a bounding rectangle specified by 
		/// a pair of coordinates: a height, and a width.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the ellipse.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
		/// <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
		public void DrawEllipse(
			Pen pen,
			float x,
			float y,
			float width,
			float height
			)
		{
			_graphics.DrawEllipse( pen, x, y, width, height );
		}

		/// <summary>
		/// Draws a cardinal spline through a specified array of PointF structures 
		/// using a specified tension. The drawing begins offset from 
		/// the beginning of the array.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and height of the curve.</param>
		/// <param name="points">Array of PointF structures that define the spline.</param>
		/// <param name="offset">Offset from the first element in the array of the points parameter to the starting point in the curve.</param>
		/// <param name="numberOfSegments">Number of segments after the starting point to include in the curve.</param>
		/// <param name="tension">Value greater than or equal to 0.0F that specifies the tension of the curve.</param>
		public void DrawCurve(
			Pen pen,
			PointF[] points,
			int offset,
			int numberOfSegments,
			float tension
			)
		{
			_graphics.DrawCurve( pen, points, offset,  numberOfSegments, tension );
		}

		/// <summary>
		/// Draws a rectangle specified by a coordinate pair: a width, and a height.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the rectangle.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="width">Width of the rectangle to draw.</param>
		/// <param name="height">Height of the rectangle to draw.</param>
		public void DrawRectangle(
			Pen pen,
			int x,
			int y,
			int width,
			int height
			)
		{
			_graphics.DrawRectangle( pen, x, y, width, height );
		}

		/// <summary>
		/// Draws a polygon defined by an array of PointF structures.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the polygon.</param>
		/// <param name="points">Array of PointF structures that represent the vertices of the polygon.</param>
		public void DrawPolygon(
			Pen pen,
			PointF[] points
			)
		{
			_graphics.DrawPolygon( pen, points );
		}

		/// <summary>
		/// Draws the specified text string in the specified rectangle with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
		/// </summary>
		/// <param name="s">String to draw.</param>
		/// <param name="font">Font object that defines the text format of the string.</param>
		/// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
		/// <param name="layoutRectangle">RectangleF structure that specifies the location of the drawn text.</param>
		/// <param name="format">StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
		public void DrawString(
			string s,
			Font font,
			Brush brush,
			RectangleF layoutRectangle,
			StringFormat format
			)
		{
			_graphics.DrawString( s, font, brush, layoutRectangle, format );
		}

		/// <summary>
		/// Draws the specified text string at the specified location with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
		/// </summary>
		/// <param name="s">String to draw.</param>
		/// <param name="font">Font object that defines the text format of the string.</param>
		/// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
		/// <param name="point">PointF structure that specifies the upper-left corner of the drawn text.</param>
		/// <param name="format">StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
		public void DrawString(
			string s,
			Font font,
			Brush brush,
			PointF point,
			StringFormat format
			)
		{
			_graphics.DrawString( s, font, brush, point, format );
		}

		/// <summary>
		/// Draws the specified portion of the specified Image object at the specified location and with the specified size.
		/// </summary>
		/// <param name="image">Image object to draw.</param>
		/// <param name="destRect">Rectangle structure that specifies the location and size of the drawn image. The image is scaled to fit the rectangle.</param>
		/// <param name="srcX">x-coordinate of the upper-left corner of the portion of the source image to draw.</param>
		/// <param name="srcY">y-coordinate of the upper-left corner of the portion of the source image to draw.</param>
		/// <param name="srcWidth">Width of the portion of the source image to draw.</param>
		/// <param name="srcHeight">Height of the portion of the source image to draw.</param>
		/// <param name="srcUnit">Member of the GraphicsUnit enumeration that specifies the units of measure used to determine the source rectangle.</param>
		/// <param name="imageAttrs">ImageAttributes object that specifies recoloring and gamma information for the image object.</param>
		public void DrawImage(
            System.Drawing.Image image,
			Rectangle destRect,
			float srcX,
			float srcY,
			float srcWidth,
			float srcHeight,
			GraphicsUnit srcUnit,
			ImageAttributes imageAttrs
			)
		{
			_graphics.DrawImage( image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttrs );
		}

		/// <summary>
		/// Draws a rectangle specified by a coordinate pair: a width, and a height.
		/// </summary>
		/// <param name="pen">A Pen object that determines the color, width, and style of the rectangle.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="width">The width of the rectangle to draw.</param>
		/// <param name="height">The height of the rectangle to draw.</param>
		public void DrawRectangle(
			Pen pen,
			float x,
			float y,
			float width,
			float height
			)
		{
			_graphics.DrawRectangle( pen, x, y, width, height );
		}

		/// <summary>
		/// Draws a GraphicsPath object.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the path.</param>
		/// <param name="path">GraphicsPath object to draw.</param>
		public void DrawPath(
			Pen pen,
			GraphicsPath path
			)
		{
			_graphics.DrawPath( pen, path );
		}

		/// <summary>
		/// Draws a pie shape defined by an ellipse specified by a coordinate pair: a width, a height and two radial lines.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the pie shape.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
		/// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
		/// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
		/// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
		/// <param name="sweepAngle">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
		public void DrawPie(
			Pen pen,
			float x,
			float y,
			float width,
			float height,
			float startAngle,
			float sweepAngle
			)
		{
			_graphics.DrawPie( pen, x, y, width, height, startAngle, sweepAngle );
		}

		/// <summary>
		/// Draws an arc representing a portion of an ellipse specified by a pair of coordinates: a width, and a height.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the arc.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the rectangle that defines the ellipse.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the rectangle that defines the ellipse.</param>
		/// <param name="width">Width of the rectangle that defines the ellipse.</param>
		/// <param name="height">Height of the rectangle that defines the ellipse.</param>
		/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the starting point of the arc.</param>
		/// <param name="sweepAngle">Angle in degrees measured clockwise from the startAngle parameter to ending point of the arc.</param>
		public void DrawArc(
			Pen pen,
			float x,
			float y,
			float width,
			float height,
			float startAngle,
			float sweepAngle
			)
		{
			_graphics.DrawArc( pen, x, y, width, height, startAngle, sweepAngle );
		}

		/// <summary>
		/// Draws the specified Image object at the specified location and with the specified size.
		/// </summary>
		/// <param name="image">Image object to draw.</param>
		/// <param name="rect">RectangleF structure that specifies the location and size of the drawn image.</param>
		public void DrawImage(
            System.Drawing.Image image,
			RectangleF rect
			)
		{
			_graphics.DrawImage( image, rect );
		}

		/// <summary>
		/// Draws an ellipse defined by a bounding RectangleF.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the ellipse.</param>
		/// <param name="rect">RectangleF structure that defines the boundaries of the ellipse.</param>
		public void DrawEllipse(
			Pen pen,
			RectangleF rect
			)
		{
			_graphics.DrawEllipse( pen, rect );
		}

		/// <summary>
		/// Draws a series of line segments that connect an array of PointF structures.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the line segments.</param>
		/// <param name="points">Array of PointF structures that represent the points to connect.</param>
		public void DrawLines(
			Pen pen,
			PointF[] points
			)
		{
			_graphics.DrawLines( pen, points );
		}

		#endregion // Drawing Methods

		#region Filling Methods

		/// <summary>
		/// Fills the interior of an ellipse defined by a bounding rectangle 
		/// specified by a RectangleF structure.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="rect">RectangleF structure that represents the bounding rectangle that defines the ellipse.</param>
		public void FillEllipse(
			Brush brush,
			RectangleF rect
			)
		{
			_graphics.FillEllipse( brush, rect );
		}

		/// <summary>
		/// Fills the interior of a GraphicsPath object.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="path">GraphicsPath object that represents the path to fill.</param>
		public void FillPath(
			Brush brush,
			GraphicsPath path
			)
		{
			_graphics.FillPath( brush, path );
		}

		/// <summary>
		/// Fills the interior of a Region object.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="region">Region object that represents the area to fill.</param>
		public void FillRegion(
			Brush brush,
			Region region
			)
		{
			_graphics.FillRegion( brush, region );
		}

		/// <summary>
		/// Fills the interior of a rectangle specified by a RectangleF structure.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="rect">RectangleF structure that represents the rectangle to fill.</param>
		public void FillRectangle(
			Brush brush,
			RectangleF rect
			)
		{
			_graphics.FillRectangle( brush, rect );
		}

		/// <summary>
		/// Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the rectangle to fill.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the rectangle to fill.</param>
		/// <param name="width">Width of the rectangle to fill.</param>
		/// <param name="height">Height of the rectangle to fill.</param>
		public void FillRectangle(
			Brush brush,
			float x,
			float y,
			float width,
			float height
			)
		{
			_graphics.FillRectangle( brush, x, y, width, height );
		}

		/// <summary>
		/// Fills the interior of a polygon defined by an array of points specified by PointF structures .
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="points">Array of PointF structures that represent the vertices of the polygon to fill.</param>
		public void FillPolygon(
			Brush brush,
			PointF[] points
			)
		{
			_graphics.FillPolygon( brush, points );
		}

		/// <summary>
		/// Fills the interior of a pie section defined by an ellipse 
		/// specified by a pair of coordinates, a width, and a height 
		/// and two radial lines.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
		/// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
		/// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie section comes.</param>
		/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the first side of the pie section.</param>
		/// <param name="sweepAngle">Angle in degrees measured clockwise from the startAngle parameter to the second side of the pie section.</param>
		public void FillPie(
			Brush brush,
			float x,
			float y,
			float width,
			float height,
			float startAngle,
			float sweepAngle
			)
		{
			_graphics.FillPie( brush, x, y, width, height, startAngle, sweepAngle );
		}

		#endregion // Filling Methods

		#region Other Methods

		/// <summary>
		/// Measures the specified string when drawn with the specified 
		/// Font object and formatted with the specified StringFormat object.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="font">Font object defines the text format of the string.</param>
		/// <param name="layoutArea">SizeF structure that specifies the maximum layout area for the text.</param>
		/// <param name="stringFormat">StringFormat object that represents formatting information, such as line spacing, for the string.</param>
		/// <returns>This method returns a SizeF structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
		public SizeF MeasureString(
			string text,
			Font font,
			SizeF layoutArea,
			StringFormat stringFormat
			)
		{
			return _graphics.MeasureString( text, font, layoutArea, stringFormat );
		}

		/// <summary>
		/// Measures the specified string when drawn with the specified 
		/// Font object and formatted with the specified StringFormat object.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="font">Font object defines the text format of the string.</param>
		/// <returns>This method returns a SizeF structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
		public SizeF MeasureString(
			string text,
			Font font
			)
		{
			return _graphics.MeasureString( text, font );
		}

		/// <summary>
		/// Saves the current state of this Graphics object and identifies the saved state with a GraphicsState object.
		/// </summary>
		/// <returns>This method returns a GraphicsState object that represents the saved state of this Graphics object.</returns>
		public GraphicsState Save()
		{
			return _graphics.Save();
		}

		/// <summary>
		/// Restores the state of this Graphics object to the state represented by a GraphicsState object.
		/// </summary>
		/// <param name="gstate">GraphicsState object that represents the state to which to restore this Graphics object.</param>
		public void Restore(
			GraphicsState gstate
			)
		{
			_graphics.Restore( gstate );
		}

		/// <summary>
		/// Resets the clip region of this Graphics object to an infinite region.
		/// </summary>
		public void ResetClip()
		{
			_graphics.ResetClip();
		}

		/// <summary>
		/// Sets the clipping region of this Graphics object to the rectangle specified by a RectangleF structure.
		/// </summary>
		/// <param name="rect">RectangleF structure that represents the new clip region.</param>
		public void SetClip(
			RectangleF rect
			)
		{
			_graphics.SetClip( rect );
		}

		/// <summary>
		/// Sets the clipping region of this Graphics object to the result of the 
		/// specified operation combining the current clip region and the 
		/// specified GraphicsPath object.
		/// </summary>
		/// <param name="path">GraphicsPath object to combine.</param>
		/// <param name="combineMode">Member of the CombineMode enumeration that specifies the combining operation to use.</param>
		public void SetClip(
			GraphicsPath path,
			CombineMode combineMode
			)
		{
			_graphics.SetClip( path, combineMode );
		}

		/// <summary>
		/// Prepends the specified translation to the transformation matrix of this Graphics object.
		/// </summary>
		/// <param name="dx">x component of the translation.</param>
		/// <param name="dy">y component of the translation.</param>
		public void TranslateTransform(
			float dx,
			float dy
			)
		{
			_graphics.TranslateTransform( dx, dy );
		}

		/// <summary>
		/// This method starts Selection mode
		/// </summary>
		/// <param name="hRef">The location of the referenced object, expressed as a URI reference.</param>
		/// <param name="title">Title which could be used for tooltips.</param>
		public void BeginSelection( string hRef, string title )
		{
			// Not supported for GDI+
		}

		/// <summary>
		/// This method stops Selection mode
		/// </summary>
		public void EndSelection( )
		{
			// Not supported for GDI+
		}


		#endregion // Other Methods

		#region Properties

		/// <summary>
		/// Gets or sets the world transformation for this Graphics object.
		/// </summary>
		public Matrix Transform
		{
			get
			{
				return _graphics.Transform;
			}
			set
			{
				_graphics.Transform = value;
			}
		}

		/// <summary>
		/// Gets or sets the rendering quality for this Graphics object.
		/// </summary>
		public SmoothingMode SmoothingMode 
		{
			get
			{
				return _graphics.SmoothingMode;
			}
			set
			{
				_graphics.SmoothingMode = value;
			}
		}

		/// <summary>
		/// Gets or sets the rendering mode for text associated with this Graphics object.
		/// </summary>
		public TextRenderingHint TextRenderingHint 
		{
			get
			{
				return _graphics.TextRenderingHint;
			}
			set
			{
				_graphics.TextRenderingHint = value;
			}
		}

		/// <summary>
		/// Gets or sets a Region object that limits the drawing region of this Graphics object.
		/// </summary>
		public Region Clip 
		{
			get
			{
				return _graphics.Clip;
			}
			set
			{
				_graphics.Clip = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the clipping region of this Graphics object is empty.
		/// </summary>
		public bool IsClipEmpty 
		{
			get
			{
				return _graphics.IsClipEmpty;
			}
		}

		/// <summary>
		/// Reference to the Graphics object
		/// </summary>
		public Graphics Graphics
		{
			get
			{
				return _graphics;
			}
			set
			{
				_graphics = value;
			}
		}

		#endregion // Properties

		#region Fields

		/// <summary>
		/// Graphics object
		/// </summary>
		Graphics		_graphics = null;

		#endregion // Fields
	}
}
