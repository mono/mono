//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		IChartRenderingEngine.cs
//
//  Namespace:	DataVisualization.Charting
//
//	Classes:	IChartRenderingEngine, IChartAnimationEngine
//
//  Purpose:	Defines interfaces which must be implemented by 
//				every rendering and animation engine class. These 
//              interfaces are used in GDI+, SVG and Flash rendering. 
//              Note that animation is only available in SVG and 
//              Flash rendering engines.
//
//	Reviwed:	AG - Jul 15, 2003
//              AG - MArch 14, 2007
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

#endregion

#if WINFORMS_CONTROL
    namespace System.Windows.Forms.DataVisualization.Charting
#else
namespace System.Web.UI.DataVisualization.Charting

#endif
{
	/// <summary>
    /// IChartRenderingEngine interface defines a set of methods and properties 
    /// which must be implemented by any chart rendering engine. It contains 
    /// methods for drawing basic shapes.
	/// </summary>
	internal interface IChartRenderingEngine
	{
		#region Drawing Methods

		/// <summary>
		/// Draws a line connecting two PointF structures.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
		/// <param name="pt1">PointF structure that represents the first point to connect.</param>
		/// <param name="pt2">PointF structure that represents the second point to connect.</param>
		void DrawLine(
			Pen pen,
			PointF pt1,
			PointF pt2
			);

		/// <summary>
		/// Draws a line connecting the two points specified by coordinate pairs.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
		/// <param name="x1">x-coordinate of the first point.</param>
		/// <param name="y1">y-coordinate of the first point.</param>
		/// <param name="x2">x-coordinate of the second point.</param>
		/// <param name="y2">y-coordinate of the second point.</param>
		void DrawLine(
			Pen pen,
			float x1,
			float y1,
			float x2,
			float y2
			);

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
		void DrawImage(
            System.Drawing.Image image,
			Rectangle destRect,
			int srcX,
			int srcY,
			int srcWidth,
			int srcHeight,
			GraphicsUnit srcUnit,
			ImageAttributes imageAttr
			);

		/// <summary>
		/// Draws an ellipse defined by a bounding rectangle specified by 
		/// a pair of coordinates, a height, and a width.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the ellipse.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
		/// <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
		void DrawEllipse(
			Pen pen,
			float x,
			float y,
			float width,
			float height
			);

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
		void DrawCurve(
			Pen pen,
			PointF[] points,
			int offset,
			int numberOfSegments,
			float tension
			);

		/// <summary>
		/// Draws a rectangle specified by a coordinate pair, a width, and a height.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the rectangle.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="width">Width of the rectangle to draw.</param>
		/// <param name="height">Height of the rectangle to draw.</param>
		void DrawRectangle(
			Pen pen,
			int x,
			int y,
			int width,
			int height
			);

		/// <summary>
		/// Draws a polygon defined by an array of PointF structures.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the polygon.</param>
		/// <param name="points">Array of PointF structures that represent the vertices of the polygon.</param>
		void DrawPolygon(
			Pen pen,
			PointF[] points
			);

		/// <summary>
		/// Draws the specified text string in the specified rectangle with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
		/// </summary>
		/// <param name="s">String to draw.</param>
		/// <param name="font">Font object that defines the text format of the string.</param>
		/// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
		/// <param name="layoutRectangle">RectangleF structure that specifies the location of the drawn text.</param>
		/// <param name="format">StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
		void DrawString(
			string s,
			Font font,
			Brush brush,
			RectangleF layoutRectangle,
			StringFormat format
			);

		/// <summary>
		/// Draws the specified text string at the specified location with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
		/// </summary>
		/// <param name="s">String to draw.</param>
		/// <param name="font">Font object that defines the text format of the string.</param>
		/// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
		/// <param name="point">PointF structure that specifies the upper-left corner of the drawn text.</param>
		/// <param name="format">StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
		void DrawString(
			string s,
			Font font,
			Brush brush,
			PointF point,
			StringFormat format
			);

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
		void DrawImage(
            System.Drawing.Image image,
			Rectangle destRect,
			float srcX,
			float srcY,
			float srcWidth,
			float srcHeight,
			GraphicsUnit srcUnit,
			ImageAttributes imageAttrs
			);

		/// <summary>
		/// Draws a rectangle specified by a coordinate pair, a width, and a height.
		/// </summary>
		/// <param name="pen">A Pen object that determines the color, width, and style of the rectangle.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="width">The width of the rectangle to draw.</param>
		/// <param name="height">The height of the rectangle to draw.</param>
		void DrawRectangle(
			Pen pen,
			float x,
			float y,
			float width,
			float height
			);

		/// <summary>
		/// Draws a GraphicsPath object.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the path.</param>
		/// <param name="path">GraphicsPath object to draw.</param>
		void DrawPath(
			Pen pen,
			GraphicsPath path
			);
						
		/// <summary>
		/// Draws a pie shape defined by an ellipse specified by a coordinate pair, a width, and a height and two radial lines.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the pie shape.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
		/// <param name="width">Width of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
		/// <param name="height">Height of the bounding rectangle that defines the ellipse from which the pie shape comes.</param>
		/// <param name="startAngle">Angle measured in degrees clockwise from the x-axis to the first side of the pie shape.</param>
		/// <param name="sweepAngle">Angle measured in degrees clockwise from the startAngle parameter to the second side of the pie shape.</param>
		void DrawPie(
			Pen pen,
			float x,
			float y,
			float width,
			float height,
			float startAngle,
			float sweepAngle
			);

		/// <summary>
		/// Draws an arc representing a portion of an ellipse specified by a pair of coordinates, a width, and a height.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the arc.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the rectangle that defines the ellipse.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the rectangle that defines the ellipse.</param>
		/// <param name="width">Width of the rectangle that defines the ellipse.</param>
		/// <param name="height">Height of the rectangle that defines the ellipse.</param>
		/// <param name="startAngle">Angle in degrees measured clockwise from the x-axis to the starting point of the arc.</param>
		/// <param name="sweepAngle">Angle in degrees measured clockwise from the startAngle parameter to ending point of the arc.</param>
		void DrawArc(
			Pen pen,
			float x,
			float y,
			float width,
			float height,
			float startAngle,
			float sweepAngle
			);

		/// <summary>
		/// Draws the specified Image object at the specified location and with the specified size.
		/// </summary>
		/// <param name="image">Image object to draw.</param>
		/// <param name="rect">RectangleF structure that specifies the location and size of the drawn image.</param>
		void DrawImage(
            System.Drawing.Image image,
			RectangleF rect
			);

		/// <summary>
		/// Draws an ellipse defined by a bounding RectangleF.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the ellipse.</param>
		/// <param name="rect">RectangleF structure that defines the boundaries of the ellipse.</param>
		void DrawEllipse(
			Pen pen,
			RectangleF rect
			);

		/// <summary>
		/// Draws a series of line segments that connect an array of PointF structures.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the line segments.</param>
		/// <param name="points">Array of PointF structures that represent the points to connect.</param>
		void DrawLines(
			Pen pen,
			PointF[] points
			);
		
		#endregion // Drawing Methods

		#region Filling Methods

		/// <summary>
		/// Fills the interior of an ellipse defined by a bounding rectangle 
		/// specified by a RectangleF structure.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="rect">RectangleF structure that represents the bounding rectangle that defines the ellipse.</param>
		void FillEllipse(
			Brush brush,
			RectangleF rect
			);

		/// <summary>
		/// Fills the interior of a GraphicsPath object.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="path">GraphicsPath object that represents the path to fill.</param>
		void FillPath(
			Brush brush,
			GraphicsPath path
			);

		/// <summary>
		/// Fills the interior of a Region object.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="region">Region object that represents the area to fill.</param>
		void FillRegion(
			Brush brush,
			Region region
			);

		/// <summary>
		/// Fills the interior of a rectangle specified by a RectangleF structure.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="rect">RectangleF structure that represents the rectangle to fill.</param>
		void FillRectangle(
			Brush brush,
			RectangleF rect
			);
		
		/// <summary>
		/// Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the rectangle to fill.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the rectangle to fill.</param>
		/// <param name="width">Width of the rectangle to fill.</param>
		/// <param name="height">Height of the rectangle to fill.</param>
		void FillRectangle(
			Brush brush,
			float x,
			float y,
			float width,
			float height
			);

		/// <summary>
		/// Fills the interior of a polygon defined by an array of points specified by PointF structures .
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="points">Array of PointF structures that represent the vertices of the polygon to fill.</param>
		void FillPolygon(
			Brush brush,
			PointF[] points
			);

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
		void FillPie(
			Brush brush,
			float x,
			float y,
			float width,
			float height,
			float startAngle,
			float sweepAngle
			);

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
		SizeF MeasureString(
			string text,
			Font font,
			SizeF layoutArea,
			StringFormat stringFormat
			);

		/// <summary>
		/// Measures the specified string when drawn with the specified 
		/// Font object and formatted with the specified StringFormat object.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="font">Font object defines the text format of the string.</param>
		/// <returns>This method returns a SizeF structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
		SizeF MeasureString(
			string text,
			Font font
			);

		/// <summary>
		/// Saves the current state of this Graphics object and identifies the saved state with a GraphicsState object.
		/// </summary>
		/// <returns>This method returns a GraphicsState object that represents the saved state of this Graphics object.</returns>
		GraphicsState Save();

		/// <summary>
		/// Restores the saved state of graphics object.
		/// </summary>
		/// <param name="gstate">State to restore.</param>
		void Restore(
			GraphicsState gstate
			);

		/// <summary>
		/// Resets the clip region of this Graphics object to an infinite region.
		/// </summary>
		void ResetClip();

		/// <summary>
		/// Sets the clipping region of this Graphics object to the rectangle specified by a RectangleF structure.
		/// </summary>
		/// <param name="rect">RectangleF structure that represents the new clip region.</param>
		void SetClip(
			RectangleF rect
			);

		/// <summary>
		/// Sets the clipping region of this Graphics object to the result of the 
		/// specified operation combining the current clip region and the 
		/// specified GraphicsPath object.
		/// </summary>
		/// <param name="path">GraphicsPath object to combine.</param>
		/// <param name="combineMode">Member of the CombineMode enumeration that specifies the combining operation to use.</param>
		void SetClip(
			GraphicsPath path,
			CombineMode combineMode
			);

		/// <summary>
		/// Prepends the specified translation to the transformation matrix of this Graphics object.
		/// </summary>
		/// <param name="dx">x component of the translation.</param>
		/// <param name="dy">y component of the translation.</param>
		void TranslateTransform(
			float dx,
			float dy
			);

		/// <summary>
		/// This method starts Selection mode
		/// </summary>
		/// <param name="hRef">The location of the referenced object, expressed as a URI reference.</param>
		/// <param name="title">Title which could be used for tooltips.</param>
		void BeginSelection( string hRef, string title );

		/// <summary>
		/// This method stops Selection mode
		/// </summary>
		void EndSelection( );
		
		#endregion // Other Methods
		
		#region Properties

		/// <summary>
		/// Gets or sets the world transformation for this Graphics object.
		/// </summary>
		Matrix Transform {get; set;}

		/// <summary>
		/// Gets or sets the rendering quality for this Graphics object.
		/// </summary>
		SmoothingMode SmoothingMode {get; set;}

		/// <summary>
		/// Gets or sets the rendering mode for text associated with this Graphics object.
		/// </summary>
		TextRenderingHint TextRenderingHint {get; set;}

		/// <summary>
		/// Gets or sets a Region object that limits the drawing region of this Graphics object.
		/// </summary>
		Region Clip {get; set;}

		/// <summary>
		/// Reference to the Graphics object
		/// </summary>
		Graphics Graphics {get; set;}
		
		/// <summary>
		/// Gets a value indicating whether the clipping region of this Graphics object is empty.
		/// </summary>
		bool IsClipEmpty {get;}

		#endregion // Properties
	}
}
