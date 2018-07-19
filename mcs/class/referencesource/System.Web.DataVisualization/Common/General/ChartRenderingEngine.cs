//-------------------------------------------------------------
// <copyright company=’Microsoft Corporation’>
//   Copyright © Microsoft Corporation. All Rights Reserved.
// </copyright>
//-------------------------------------------------------------
// @owner=alexgor, deliant
//=================================================================
//  File:		ChartRenderingEngine.cs
//
//  Namespace:	System.Web.UI.WebControls[Windows.Forms].Charting
//
//	Classes:	ChartRenderingEngine, ValueA, PointA, RectangleA, 
//				ColorA
//
//  Purpose:	ChartRenderingEngine class provides a common interface 
//              to the graphics rendering and animation engines. 
//              Internally it uses SvgChartGraphics, FlashGraphics or 
//              GdiGraphics classes depending on the ActiveRenderingType 
//              property settings.
//
//              ValueA, PointA, RectangleA and ColorA classes are
//              used to store data about animated values like colors
//              position or rectangles. They store starting value/time, 
//              end value/time, repeat flags and other settings. These 
//              clases are used with animation engines.
//
//	Reviwed:	AG - Jul 15, 2003
//              AG - Microsoft 16, 2007
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
using System.Xml;
using System.IO;
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
	/// Specify Rendering AxisName
	/// </summary>
	internal enum RenderingType
	{
		/// <summary>
		/// GDI+ AxisName
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Gdi")]
        Gdi,

		/// <summary>
		/// SVG AxisName
		/// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Svg")]
        Svg,
	}

	#endregion // Enumerations

	/// <summary>
    /// The ChartGraphics class provides a common interface to the 
    /// graphics rendering.
	/// </summary>
#if ASPPERM_35
	[AspNetHostingPermission(System.Security.Permissions.SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(System.Security.Permissions.SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
    public partial class ChartGraphics
	{
		#region Fields

        // Current rendering type
        private RenderingType _activeRenderingType = RenderingType.Gdi;

        // GDI+ rendering engine
        private GdiGraphics _gdiGraphics = new GdiGraphics();

        // Document title used for SVG rendering
        //private string documentTitle = string.Empty;

        // True if text should be clipped
        internal bool IsTextClipped = false;

		#endregion // Fields

		#region Drawing Methods

		/// <summary>
		/// Draws a line connecting two PointF structures.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
		/// <param name="pt1">PointF structure that represents the first point to connect.</param>
		/// <param name="pt2">PointF structure that represents the second point to connect.</param>
		internal void DrawLine(
			Pen pen,
			PointF pt1,
			PointF pt2
			)
		{
    		RenderingObject.DrawLine( pen, pt1, pt2 );
		}

		/// <summary>
		/// Draws a line connecting the two points specified by coordinate pairs.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the line.</param>
		/// <param name="x1">x-coordinate of the first point.</param>
		/// <param name="y1">y-coordinate of the first point.</param>
		/// <param name="x2">x-coordinate of the second point.</param>
		/// <param name="y2">y-coordinate of the second point.</param>
		internal void DrawLine(
			Pen pen,
			float x1,
			float y1,
			float x2,
			float y2
			)
		{
			RenderingObject.DrawLine( pen, x1, y1, x2, y2 );
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
		internal void DrawImage(
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
			RenderingObject.DrawImage( 
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
		/// a pair of coordinates, a height, and a width.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the ellipse.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the bounding rectangle that defines the ellipse.</param>
		/// <param name="width">Width of the bounding rectangle that defines the ellipse.</param>
		/// <param name="height">Height of the bounding rectangle that defines the ellipse.</param>
		internal void DrawEllipse(
			Pen pen,
			float x,
			float y,
			float width,
			float height
			)
		{
			RenderingObject.DrawEllipse( pen, x, y, width, height );
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
		internal void DrawCurve(
			Pen pen,
			PointF[] points,
			int offset,
			int numberOfSegments,
			float tension
			)
		{
            ChartGraphics chartGraphics = this as ChartGraphics;
            if (chartGraphics == null || !chartGraphics.IsMetafile)
            {
                RenderingObject.DrawCurve(pen, points, offset, numberOfSegments, tension);
            }
            else
            {
                // Special handling required for the metafiles. We cannot pass large array of
                // points because they will be persisted inside EMF file and cause exponential 
                // increase in emf file size. Draw curve method uses additional 2, 3 or 4 points
                // depending on which segement is drawn.
                PointF[] pointsExact = null;
                if (offset == 0 && numberOfSegments == points.Length - 1)
                {
                    // In case the array contains the minimum required number of points
                    // to draw segments - just call the curve drawing method
                    RenderingObject.DrawCurve(pen, points, offset, numberOfSegments, tension);
                }
                else
                {
                    if (offset == 0 && numberOfSegments < points.Length - 1)
                    {
                        // Segment is at the beginning of the array with more points following
                        pointsExact = new PointF[numberOfSegments + 2];
                        for (int index = 0; index < numberOfSegments + 2; index++)
                        {
                            pointsExact[index] = points[index];
                        }
                    }
                    else if (offset > 0 && (offset + numberOfSegments) == points.Length - 1)
                    {
                        // Segment is at the end of the array with more points prior to it
                        pointsExact = new PointF[numberOfSegments + 2];
                        for (int index = 0; index < numberOfSegments + 2; index++)
                        {
                            pointsExact[index] = points[offset + index - 1];
                        }
                        offset = 1;
                    }
                    else if (offset > 0 && (offset + numberOfSegments) < points.Length - 1)
                    {
                        // Segment in the middle of the array with points prior and following it
                        pointsExact = new PointF[numberOfSegments + 3];
                        for (int index = 0; index < numberOfSegments + 3; index++)
                        {
                            pointsExact[index] = points[offset + index - 1];
                        }
                        offset = 1;
                    }

                    // Render the curve using minimum number of required points in the array 
                    RenderingObject.DrawCurve(pen, pointsExact, offset, numberOfSegments, tension);
                }
            }
		}

		/// <summary>
		/// Draws a rectangle specified by a coordinate pair, a width, and a height.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the rectangle.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="width">Width of the rectangle to draw.</param>
		/// <param name="height">Height of the rectangle to draw.</param>
		internal void DrawRectangle(
			Pen pen,
			int x,
			int y,
			int width,
			int height
			)
		{
			RenderingObject.DrawRectangle( pen, x, y, width, height );
		}

		/// <summary>
		/// Draws a polygon defined by an array of PointF structures.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the polygon.</param>
		/// <param name="points">Array of PointF structures that represent the vertices of the polygon.</param>
		internal void DrawPolygon(
			Pen pen,
			PointF[] points
			)
		{
			RenderingObject.DrawPolygon( pen, points );
		}

		/// <summary>
		/// Draws the specified text string in the specified rectangle with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
		/// </summary>
		/// <param name="s">String to draw.</param>
		/// <param name="font">Font object that defines the text format of the string.</param>
		/// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
		/// <param name="layoutRectangle">RectangleF structure that specifies the location of the drawn text.</param>
		/// <param name="format">StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
		internal void DrawString(
			string s,
			Font font,
			Brush brush,
			RectangleF layoutRectangle,
			StringFormat format
			)
		{
            using (StringFormat fmt = (StringFormat)format.Clone())
            {
                if ( IsRightToLeft )
                    fmt.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                if (!IsTextClipped && (fmt.FormatFlags & StringFormatFlags.NoClip) != StringFormatFlags.NoClip)
                    fmt.FormatFlags |= StringFormatFlags.NoClip;
                RenderingObject.DrawString(s, font, brush, layoutRectangle, fmt);
            }
		}

		/// <summary>
		/// Draws the specified text string at the specified location with the specified Brush and Font objects using the formatting properties of the specified StringFormat object.
		/// </summary>
		/// <param name="s">String to draw.</param>
		/// <param name="font">Font object that defines the text format of the string.</param>
		/// <param name="brush">Brush object that determines the color and texture of the drawn text.</param>
		/// <param name="point">PointF structure that specifies the upper-left corner of the drawn text.</param>
		/// <param name="format">StringFormat object that specifies formatting properties, such as line spacing and alignment, that are applied to the drawn text.</param>
		internal void DrawString(
			string s,
			Font font,
			Brush brush,
			PointF point,
			StringFormat format
			)
		{
            if (IsRightToLeft)
            {
                using (StringFormat fmt = (StringFormat)format.Clone())
                {
                    fmt.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                    if (fmt.Alignment == StringAlignment.Far)
                    {
                        fmt.Alignment = StringAlignment.Near;
                    }
                    else if (fmt.Alignment == StringAlignment.Near)
                    {
                        fmt.Alignment = StringAlignment.Far;
                    }
                    RenderingObject.DrawString(s, font, brush, point, fmt);
                }
            }
            else 
                RenderingObject.DrawString(s, font, brush, point, format);
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
		internal void DrawImage(
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
			RenderingObject.DrawImage( image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttrs );
		}

		/// <summary>
		/// Draws a rectangle specified by a coordinate pair, a width, and a height.
		/// </summary>
		/// <param name="pen">A Pen object that determines the color, width, and style of the rectangle.</param>
		/// <param name="x">The x-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="y">The y-coordinate of the upper-left corner of the rectangle to draw.</param>
		/// <param name="width">The width of the rectangle to draw.</param>
		/// <param name="height">The height of the rectangle to draw.</param>
		internal void DrawRectangle(
			Pen pen,
			float x,
			float y,
			float width,
			float height
			)
		{
			RenderingObject.DrawRectangle( pen, x, y, width, height );
		}

		/// <summary>
		/// Draws a GraphicsPath object.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the path.</param>
		/// <param name="path">GraphicsPath object to draw.</param>
		internal void DrawPath(
			Pen pen,
			GraphicsPath path
			)
		{
			// Check if path is empty
			if(path == null || 
				path.PointCount == 0)
			{
				return;
			}

			RenderingObject.DrawPath( pen, path );
		}

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
		internal void DrawPie(
			Pen pen,
			float x,
			float y,
			float width,
			float height,
			float startAngle,
			float sweepAngle
			)
		{
			RenderingObject.DrawPie( pen, x, y, width, height, startAngle, sweepAngle );
		}

		/// <summary>
		/// Draws an ellipse defined by a bounding RectangleF.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the ellipse.</param>
		/// <param name="rect">RectangleF structure that defines the boundaries of the ellipse.</param>
		internal void DrawEllipse(
			Pen pen,
			RectangleF rect
			)
		{
			RenderingObject.DrawEllipse( pen, rect );
		}

		/// <summary>
		/// Draws a series of line segments that connect an array of PointF structures.
		/// </summary>
		/// <param name="pen">Pen object that determines the color, width, and style of the line segments.</param>
		/// <param name="points">Array of PointF structures that represent the points to connect.</param>
		internal void DrawLines(
			Pen pen,
			PointF[] points
			)
		{
			RenderingObject.DrawLines( pen, points );
		}

		#endregion // Drawing Methods

		#region Filling Methods

		/// <summary>
		/// Fills the interior of an ellipse defined by a bounding rectangle 
		/// specified by a RectangleF structure.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="rect">RectangleF structure that represents the bounding rectangle that defines the ellipse.</param>
		internal void FillEllipse(
			Brush brush,
			RectangleF rect
			)
		{
			RenderingObject.FillEllipse( brush, rect );
		}

		/// <summary>
		/// Fills the interior of a GraphicsPath object.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="path">GraphicsPath object that represents the path to fill.</param>
		internal void FillPath(
			Brush brush,
			GraphicsPath path
			)
		{
			// Check if path is empty
			if(path == null || 
				path.PointCount == 0)
			{
				return;
			}

			RenderingObject.FillPath( brush, path );
		}

		/// <summary>
		/// Fills the interior of a Region object.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="region">Region object that represents the area to fill.</param>
		internal void FillRegion(
			Brush brush,
			Region region
			)
		{
			RenderingObject.FillRegion( brush, region );
		}

		/// <summary>
		/// Fills the interior of a rectangle specified by a RectangleF structure.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="rect">RectangleF structure that represents the rectangle to fill.</param>
		internal void FillRectangle(
			Brush brush,
			RectangleF rect
			)
		{
			RenderingObject.FillRectangle( brush, rect );
		}

		/// <summary>
		/// Fills the interior of a rectangle specified by a pair of coordinates, a width, and a height.
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="x">x-coordinate of the upper-left corner of the rectangle to fill.</param>
		/// <param name="y">y-coordinate of the upper-left corner of the rectangle to fill.</param>
		/// <param name="width">Width of the rectangle to fill.</param>
		/// <param name="height">Height of the rectangle to fill.</param>
		internal void FillRectangle(
			Brush brush,
			float x,
			float y,
			float width,
			float height
			)
		{
			RenderingObject.FillRectangle( brush, x, y, width, height );
		}

		/// <summary>
		/// Fills the interior of a polygon defined by an array of points specified by PointF structures .
		/// </summary>
		/// <param name="brush">Brush object that determines the characteristics of the fill.</param>
		/// <param name="points">Array of PointF structures that represent the vertices of the polygon to fill.</param>
		internal void FillPolygon(
			Brush brush,
			PointF[] points
			)
		{
			RenderingObject.FillPolygon( brush, points );
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
		internal void FillPie(
			Brush brush,
			float x,
			float y,
			float width,
			float height,
			float startAngle,
			float sweepAngle
			)
		{
			RenderingObject.FillPie( brush, x, y, width, height, startAngle, sweepAngle );
		}
        
		#endregion // Filling Methods

		#region Other Methods

		/// <summary>
		/// This method starts SVG Selection mode
		/// </summary>
        /// <param name="url">The location of the referenced object, expressed as a URI reference.</param>
		/// <param name="title">Title which could be used for tooltips.</param>
        internal void StartHotRegion( string url, string title )
		{
			RenderingObject.BeginSelection( url, title );
		}

		/// <summary>
		/// This method starts SVG Selection mode
		/// </summary>
		/// <param name="point">Data Point which properties are used for SVG selection</param>
        internal void StartHotRegion(DataPoint point)
		{
			StartHotRegion( point, false );
		}

		/// <summary>
		/// This method starts SVG Selection mode
		/// </summary>
		/// <param name="point">Data Point which properties are used for SVG selection</param>
		/// <param name="labelRegion">Indicates if point label region is processed.</param>
		internal void StartHotRegion(DataPoint point, bool labelRegion)
		{
			string hRef = string.Empty;
			string tooltip = (labelRegion) ? point.LabelToolTip : point.ToolTip;
#if !Microsoft_CONTROL
			hRef = (labelRegion) ? point.LabelUrl : point.Url;
#endif
			if(hRef.Length > 0 || tooltip.Length > 0)
			{
				RenderingObject.BeginSelection( 
					point.ReplaceKeywords( hRef ), 
					point.ReplaceKeywords( tooltip ) );
			}
		}

		/// <summary>
		/// This method stops SVG Selection mode
		/// </summary>
		internal void EndHotRegion()
		{
			RenderingObject.EndSelection();
		}

		/// <summary>
		/// Measures the specified string when drawn with the specified 
		/// Font object and formatted with the specified StringFormat object.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="font">Font object defines the text format of the string.</param>
		/// <param name="layoutArea">SizeF structure that specifies the maximum layout area for the text.</param>
		/// <param name="stringFormat">StringFormat object that represents formatting information, such as line spacing, for the string.</param>
		/// <returns>This method returns a SizeF structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
		internal SizeF MeasureString(
			string text,
			Font font,
			SizeF layoutArea,
			StringFormat stringFormat
			)
		{
			return RenderingObject.MeasureString( text, font, layoutArea, stringFormat );
		}

		/// <summary>
		/// Measures the specified string when drawn with the specified 
		/// Font object and formatted with the specified StringFormat object.
		/// </summary>
		/// <param name="text">String to measure.</param>
		/// <param name="font">Font object defines the text format of the string.</param>
		/// <returns>This method returns a SizeF structure that represents the size, in pixels, of the string specified in the text parameter as drawn with the font parameter and the stringFormat parameter.</returns>
		internal SizeF MeasureString(
			string text,
			Font font
			)
		{
			return RenderingObject.MeasureString( text, font );
		}

		/// <summary>
		/// Saves the current state of this Graphics object and identifies the saved state with a GraphicsState object.
		/// </summary>
		/// <returns>This method returns a GraphicsState object that represents the saved state of this Graphics object.</returns>
		internal GraphicsState Save()
		{
			return RenderingObject.Save();
		}

		/// <summary>
		/// Restores the state of this Graphics object to the state represented by a GraphicsState object.
		/// </summary>
		/// <param name="gstate">GraphicsState object that represents the state to which to restore this Graphics object.</param>
		internal void Restore(
			GraphicsState gstate
			)
		{
			RenderingObject.Restore( gstate );
		}

        /// <summary>
		/// Resets the clip region of this Graphics object to an infinite region.
		/// </summary>
		internal void ResetClip()
		{
            RenderingObject.ResetClip();
		}

		/// <summary>
		/// Sets the clipping region of this Graphics object to the rectangle specified by a RectangleF structure.
		/// </summary>
		/// <param name="rect">RectangleF structure that represents the new clip region.</param>
		internal void SetClipAbs(RectangleF rect)
		{
			RenderingObject.SetClip( rect );
		}

		/// <summary>
		/// Prepends the specified translation to the transformation matrix of this Graphics object.
		/// </summary>
		/// <param name="dx">x component of the translation.</param>
		/// <param name="dy">y component of the translation.</param>
		internal void TranslateTransform(
			float dx,
			float dy
			)
		{
			RenderingObject.TranslateTransform( dx, dy );
		}

		#endregion // Other Methods

		#region Properties

		/// <summary>
		/// Gets current rendering object.
		/// </summary>
		internal IChartRenderingEngine RenderingObject
		{
			get
			{
                return _gdiGraphics;
            }
		}

		/// <summary>
		/// Gets the active rendering type.
		/// </summary>
		internal RenderingType ActiveRenderingType
		{
			get
			{
				return _activeRenderingType;
			}
		}

		/// <summary>
		/// Gets or sets the rendering mode for text associated with this Graphics object.
		/// </summary>
		internal TextRenderingHint TextRenderingHint 
		{
			get
			{
				return RenderingObject.TextRenderingHint;
			}
			set
			{
				RenderingObject.TextRenderingHint = value;
			}
		}

		/// <summary>
		/// Gets or sets the world transformation for this Graphics object.
		/// </summary>
		internal Matrix Transform
		{
			get
			{
				return RenderingObject.Transform;
			}
			set
			{
				RenderingObject.Transform = value;
			}
		}

		/// <summary>
		/// Gets or sets the rendering quality for this Graphics object.
		/// </summary>
		internal SmoothingMode SmoothingMode 
		{
			get
			{
				return RenderingObject.SmoothingMode;
			}
			set
			{
				RenderingObject.SmoothingMode = value;
			}
		}

		/// <summary>
		/// Gets or sets a Region object that limits the drawing region of this Graphics object.
		/// </summary>
		internal Region Clip 
		{
			get
			{
				return RenderingObject.Clip;
			}
			set
			{
				RenderingObject.Clip = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the clipping region of this Graphics object is empty.
		/// </summary>
		internal bool IsClipEmpty {
			get
			{
				return RenderingObject.IsClipEmpty;
			}
		}

		/// <summary>
		/// Gets or sets the reference to the Graphics object.
		/// </summary>
		public Graphics Graphics
		{
			get
			{
				return RenderingObject.Graphics;
			}
			set
			{
				RenderingObject.Graphics = value;
			}
		}

		#endregion // Properties
	}
}
