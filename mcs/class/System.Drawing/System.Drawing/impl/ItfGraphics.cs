//
// System.Drawing.ItfGraphics.cs
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com) (stubbed out)
//  Alexandre Pigolkine (pigolkine@gmx.de)
//
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace System.Drawing
{
	internal interface IGraphicsFactory
	{
		IGraphics Graphics(IntPtr nativeGraphics);
		IGraphics FromImage (Image image);
		IGraphics FromHwnd( IntPtr hwnd);
	}

	[ComVisible(false)]
	internal interface IGraphics : IDisposable
	{
		[MonoTODO]
		void AddMetafileComment (byte [] data);

		[MonoTODO]
		GraphicsContainer BeginContainer ();

		[MonoTODO]
		GraphicsContainer BeginContainer (Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit);

		[MonoTODO]
		GraphicsContainer BeginContainer (RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit);

		[MonoTODO]
		void Clear (Color color);

		[MonoTODO]
		void DrawArc (Pen pen, Rectangle rect, float startAngle, float sweepAngle);

		[MonoTODO]
		void DrawArc (Pen pen, RectangleF rect, float startAngle, float sweepAngle);

		[MonoTODO]
		void DrawArc (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle);

		[MonoTODO]
		void DrawArc (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle);

		[MonoTODO]
		void DrawBezier (Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4);

		[MonoTODO]
		void DrawBezier (Pen pen, Point pt1, Point pt2, Point pt3, Point pt4);

		[MonoTODO]
		void DrawBezier (Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4);

		[MonoTODO]
		void DrawBeziers (Pen pen, Point [] points);

		[MonoTODO]
		void DrawBeziers (Pen pen, PointF [] points);

		[MonoTODO]
		void DrawClosedCurve (Pen pen, PointF [] points);

		[MonoTODO]
		void DrawClosedCurve (Pen pen, Point [] points);

		[MonoTODO]
		void DrawClosedCurve (Pen pen, Point [] points, float tension, FillMode fillmode);

		[MonoTODO]
		void DrawClosedCurve (Pen pen, PointF [] points, float tension, FillMode fillmode);

		[MonoTODO]
		void DrawCurve (Pen pen, Point [] points);

		[MonoTODO]
		void DrawCurve (Pen pen, PointF [] points);

		[MonoTODO]
		void DrawCurve (Pen pen, PointF [] points, float tension);

		[MonoTODO]
		void DrawCurve (Pen pen, Point [] points, float tension);

		[MonoTODO]
		void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments);

		[MonoTODO]
		void DrawCurve (Pen pen, Point [] points, int offset, int numberOfSegments, float tension);

		[MonoTODO]
		void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments, float tension);

		[MonoTODO]
		void DrawEllipse (Pen pen, Rectangle rect);

		[MonoTODO]
		void DrawEllipse (Pen pen, RectangleF rect);

		[MonoTODO]
		void DrawEllipse (Pen pen, int x, int y, int width, int height);

		[MonoTODO]
		void DrawEllipse (Pen pen, float x, float y, float width, float height);

		[MonoTODO]
		void DrawIcon (Icon icon, Rectangle targetRect);

		[MonoTODO]
		void DrawIcon (Icon icon, int x, int y);

		[MonoTODO]
		void DrawIconUnstretched (Icon icon, Rectangle targetRect);

		[MonoTODO]
		void DrawImage (Image image, RectangleF rect);

		[MonoTODO]
		void DrawImage (Image image, PointF point);

		[MonoTODO]
		void DrawImage (Image image, Point [] destPoints);

		[MonoTODO]
		void DrawImage (Image image, Point point);

		[MonoTODO]
		void DrawImage (Image image, Rectangle rect);

		[MonoTODO]
		void DrawImage (Image image, PointF [] destPoints);

		[MonoTODO]
		void DrawImage (Image image, int x, int y);

		[MonoTODO]
		void DrawImage (Image image, float x, float y);

		[MonoTODO]
		void DrawImage (Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit);

		[MonoTODO]
		void DrawImage (Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit);

		[MonoTODO]
		void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit);

		[MonoTODO]
		void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit);

		[MonoTODO]
		void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr);

		[MonoTODO]
		void DrawImage (Image image, float x, float y, float width, float height);

		[MonoTODO]
		void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr);

		[MonoTODO]
		void DrawImage (Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit);

		[MonoTODO]
		void DrawImage (Image image, int x, int y, int width, int height);

		[MonoTODO]
		void DrawImage (Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit);

		[MonoTODO]
		void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, Graphics.DrawImageAbort callback);

		[MonoTODO]
		void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, Graphics.DrawImageAbort callback);

		[MonoTODO]
		void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, Graphics.DrawImageAbort callback, int callbackData);

		[MonoTODO]
		void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit);

		[MonoTODO]
		void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, Graphics.DrawImageAbort callback, int callbackData);

		[MonoTODO]
		void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit);

		[MonoTODO]
		void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs);

		[MonoTODO]
		void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr);

		[MonoTODO]
		void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, Graphics.DrawImageAbort callback);

		[MonoTODO]
		void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, Graphics.DrawImageAbort callback);

		[MonoTODO]
		void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, Graphics.DrawImageAbort callback, IntPtr callbackData);

		[MonoTODO]
		void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, Graphics.DrawImageAbort callback, IntPtr callbackData);

		[MonoTODO]
		void DrawImageUnscaled (Image image, Point point);

		[MonoTODO]
		void DrawImageUnscaled (Image image, Rectangle rect);

		[MonoTODO]
		void DrawImageUnscaled (Image image, int x, int y);

		[MonoTODO]
		void DrawImageUnscaled (Image image, int x, int y, int width, int height);

		[MonoTODO]
		void DrawLine (Pen pen, PointF pt1, PointF pt2);

		[MonoTODO]
		void DrawLine (Pen pen, Point pt1, Point pt2);

		[MonoTODO]
		void DrawLine (Pen pen, int x1, int y1, int x2, int y2);

		[MonoTODO]
		void DrawLine (Pen pen, float x1, float y1, float x2, float y2);

		[MonoTODO]
		void DrawLines (Pen pen, PointF [] points);

		[MonoTODO]
		void DrawLines (Pen pen, Point [] points);

		[MonoTODO]
		void DrawPath (Pen pen, GraphicsPath path);

		[MonoTODO]
		void DrawPie (Pen pen, Rectangle rect, float startAngle, float sweepAngle);

		[MonoTODO]
		void DrawPie (Pen pen, RectangleF rect, float startAngle, float sweepAngle);

		[MonoTODO]
		void DrawPie (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle);

		[MonoTODO]
		void DrawPie (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle);

		[MonoTODO]
		void DrawPolygon (Pen pen, Point [] points);

		[MonoTODO]
		void DrawPolygon (Pen pen, PointF [] points);

		[MonoTODO]
		void DrawRectangle (Pen pen, Rectangle rect);

		[MonoTODO]
		void DrawRectangle (Pen pen, float x, float y, float width, float height);

		[MonoTODO]
		void DrawRectangle (Pen pen, int x, int y, int width, int height);

		[MonoTODO]
		void DrawRectangles (Pen pen, RectangleF [] rects);

		[MonoTODO]
		void DrawRectangles (Pen pen, Rectangle [] rects);

		[MonoTODO]
		void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle);

		[MonoTODO]
		void DrawString (string s, Font font, Brush brush, PointF point);

		[MonoTODO]
		void DrawString (string s, Font font, Brush brush, PointF point, StringFormat format);

		[MonoTODO]
		void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format);

		[MonoTODO]
		void DrawString (string s, Font font, Brush brush, float x, float y);

		[MonoTODO]
		void DrawString (string s, Font font, Brush brush, float x, float y, StringFormat format);

		[MonoTODO]
		void EndContainer (GraphicsContainer container);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point [] destPoints, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, RectangleF destRect, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF [] destPoints, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Rectangle destRect, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point destPoint, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF destPoint, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF destPoint, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Rectangle destRect, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF [] destPoints, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point destPoint, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point [] destPoints, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, RectangleF destRect, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, RectangleF destRect, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point destPoint, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF destPoint, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point [] destPoints, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF [] destPoints, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Rectangle destRect, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit unit, Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr);

		[MonoTODO]
		void ExcludeClip (Rectangle rect);

		[MonoTODO]
		void ExcludeClip (Region region);

		[MonoTODO]
		void FillClosedCurve (Brush brush, PointF [] points);

		[MonoTODO]
		void FillClosedCurve (Brush brush, Point [] points);

		[MonoTODO]
		void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode);

		[MonoTODO]
		void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode);

		[MonoTODO]
		void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode, float tension);

		[MonoTODO]
		void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode, float tension);

		[MonoTODO]
		void FillEllipse (Brush brush, Rectangle rect);

		[MonoTODO]
		void FillEllipse (Brush brush, RectangleF rect);

		[MonoTODO]
		void FillEllipse (Brush brush, float x, float y, float width, float height);

		[MonoTODO]
		void FillEllipse (Brush brush, int x, int y, int width, int height);

		[MonoTODO]
		void FillPath (Brush brush, GraphicsPath path);

		[MonoTODO]
		void FillPie (Brush brush, Rectangle rect, float startAngle, float sweepAngle);

		[MonoTODO]
		void FillPie (Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle);

		[MonoTODO]
		void FillPie (Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle);

		[MonoTODO]
		void FillPolygon (Brush brush, PointF [] points);

		[MonoTODO]
		void FillPolygon (Brush brush, Point [] points);

		[MonoTODO]
		void FillPolygon (Brush brush, Point [] points, FillMode fillMode);

		[MonoTODO]
		void FillPolygon (Brush brush, PointF [] points, FillMode fillMode);

		[MonoTODO]
		void FillRectangle (Brush brush, RectangleF rect);

		[MonoTODO]
		void FillRectangle (Brush brush, Rectangle rect);

		[MonoTODO]
		void FillRectangle (Brush brush, int x, int y, int width, int height);
		[MonoTODO]
		void FillRectangle (Brush brush, float x, float y, float width, float height);

		[MonoTODO]
		void FillRectangles (Brush brush, Rectangle [] rects);

		[MonoTODO]
		void FillRectangles (Brush brush, RectangleF [] rects);

		[MonoTODO]
		void FillRegion (Brush brush, Region region);

		[MonoTODO]
		void Flush ();

		[MonoTODO]
		void Flush (FlushIntention intention);

		[MonoTODO]
		IntPtr GetHdc ();

		[MonoTODO]
		Color GetNearestColor (Color color);

		[MonoTODO]
		void IntersectClip (Region region);

		[MonoTODO]
		void IntersectClip (RectangleF rect);

		[MonoTODO]
		void IntersectClip (Rectangle rect);

		[MonoTODO]
		bool IsVisible (Point point);

		[MonoTODO]
		bool IsVisible (RectangleF rect);

		[MonoTODO]
		bool IsVisible (PointF point);

		[MonoTODO]
		bool IsVisible (Rectangle rect);

		[MonoTODO]
		bool IsVisible (float x, float y);

		[MonoTODO]
		bool IsVisible (int x, int y);

		[MonoTODO]
		bool IsVisible (float x, float y, float width, float height);

		[MonoTODO]
		bool IsVisible (int x, int y, int width, int height);

		[MonoTODO]
		Region [] MeasureCharacterRanges (string text, Font font, RectangleF layoutRect, StringFormat stringFormat);

		[MonoTODO]
		SizeF MeasureString (string text, Font font);

		[MonoTODO]
		SizeF MeasureString (string text, Font font, SizeF layoutArea);

		[MonoTODO]
		SizeF MeasureString (string text, Font font, int width);

		[MonoTODO]
		SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat);

		[MonoTODO]
		SizeF MeasureString (string text, Font font, int width, StringFormat format);

		[MonoTODO]
		SizeF MeasureString (string text, Font font, PointF origin, StringFormat stringFormat);

		[MonoTODO]
		SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat, ref int charactersFitted, ref int linesFilled);

		[MonoTODO]
		void MultiplyTransform (Matrix matrix);

		[MonoTODO]
		void MultiplyTransform (Matrix matrix, MatrixOrder order);

		[MonoTODO]
		void ReleaseHdc (IntPtr hdc);

		[MonoTODO]
		void ReleaseHdcInternal (IntPtr hdc);

		[MonoTODO]
		void ResetClip ();

		[MonoTODO]
		void ResetTransform ();

		[MonoTODO]
		void Restore (GraphicsState gstate);

		[MonoTODO]
		void RotateTransform (float angle);

		[MonoTODO]
		void RotateTransform (float angle, MatrixOrder order);

		[MonoTODO]
		GraphicsState Save ();

		[MonoTODO]
		void ScaleTransform (float sx, float sy);

		[MonoTODO]
		void ScaleTransform (float sx, float sy, MatrixOrder order);

		[MonoTODO]
		void SetClip (RectangleF rect);

		[MonoTODO]
		void SetClip (GraphicsPath path);

		[MonoTODO]
		void SetClip (Rectangle rect);

		[MonoTODO]
		void SetClip (Graphics g);

		[MonoTODO]
		void SetClip (Graphics g, CombineMode combineMode);

		[MonoTODO]
		void SetClip (Rectangle rect, CombineMode combineMode);

		[MonoTODO]
		void SetClip (RectangleF rect, CombineMode combineMode);

		[MonoTODO]
		void SetClip (Region region, CombineMode combineMode);

		[MonoTODO]
		void SetClip (GraphicsPath path, CombineMode combineMode);

		[MonoTODO]
		void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF [] pts);

		[MonoTODO]
		void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, Point [] pts);

		[MonoTODO]
		void TranslateClip (int dx, int dy);

		[MonoTODO]
		void TranslateClip (float dx, float dy);

		[MonoTODO]
		void TranslateTransform (float dx, float dy);

		[MonoTODO]
		void TranslateTransform (float dx, float dy, MatrixOrder order);

		Region Clip
		{
			get;
			set;
		}

		RectangleF ClipBounds
		{
			get ;
		}

		CompositingMode CompositingMode
		{
			get ;
			set ;

		}
		CompositingQuality CompositingQuality
		{
			get ;
			set ;
		}

		float DpiX
		{
			get ;
		}

		float DpiY
		{
			get ;
		}

		InterpolationMode InterpolationMode
		{
			get ;
			set ;
		}

		bool IsClipEmpty
		{
			get ;
		}

		bool IsVisibleClipEmpty
		{
			get ;
		}

		float PageScale
		{
			get ;
			set ;
		}

		GraphicsUnit PageUnit
		{
			get ;
			set ;
		}

		PixelOffsetMode PixelOffsetMode
		{
			get ;
			set ;
		}

		Point RenderingOrigin
		{
			get ;
			set ;
		}

		SmoothingMode SmoothingMode
		{
			get ;
			set ;
		}

		int TextContrast
		{
			get ;
			set ;
		}

		TextRenderingHint TextRenderingHint
		{
			get ;
			set ;
		}

		Matrix Transform
		{
			get ;
			set ;
		}

		RectangleF VisibleClipBounds
		{
			get ;
		}
	}
}

