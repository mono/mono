//
// System.Drawing.Bitmap.cs
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com) (stubbed out)
//
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace System.Drawing
{
	[ComVisible(false)]
	public sealed class Graphics : MarshalByRefObject, IDisposable
	{
		public delegate bool EnumerateMetafileProc (EmfPlusRecordType recordType,
							   int flags,
							   int dataSize,
							   IntPtr data,
							   PlayRecordCallback callbackData);

		public delegate bool DrawImageAbort (IntPtr callbackData);

		private Graphics (IntPtr nativeGraphics)
		{
			hdc_ = nativeGraphics;
		}

		[MonoTODO]
		public void AddMetafileComment (byte [] data)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public GraphicsContainer BeginContainer ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public GraphicsContainer BeginContainer (Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public GraphicsContainer BeginContainer (RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear (Color color)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Dispose ()
		{
		}

		[MonoTODO]
		public void DrawArc (Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawArc (Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawArc (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawArc (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawBezier (Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawBezier (Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawBezier (Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawBeziers (Pen pen, Point [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawBeziers (Pen pen, PointF [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawClosedCurve (Pen pen, PointF [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawClosedCurve (Pen pen, Point [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawClosedCurve (Pen pen, Point [] points, float tension, FillMode fillmode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawClosedCurve (Pen pen, PointF [] points, float tension, FillMode fillmode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawCurve (Pen pen, Point [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawCurve (Pen pen, PointF [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawCurve (Pen pen, PointF [] points, float tension)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawCurve (Pen pen, Point [] points, float tension)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawCurve (Pen pen, Point [] points, int offset, int numberOfSegments, float tension)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments, float tension)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawEllipse (Pen pen, Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawEllipse (Pen pen, RectangleF rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawEllipse (Pen pen, int x, int y, int width, int height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawEllipse (Pen pen, float x, float y, float width, float height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawIcon (Icon icon, Rectangle targetRect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawIcon (Icon icon, int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawIconUnstretched (Icon icon, Rectangle targetRect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, RectangleF rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, PointF point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Point [] destPoints)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Point point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, PointF [] destPoints)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, float x, float y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, float x, float y, float width, float height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, int x, int y, int width, int height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, DrawImageAbort callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImageUnscaled (Image image, Point point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImageUnscaled (Image image, Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImageUnscaled (Image image, int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawImageUnscaled (Image image, int x, int y, int width, int height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawLine (Pen pen, PointF pt1, PointF pt2)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawLine (Pen pen, Point pt1, Point pt2)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawLine (Pen pen, int x1, int y1, int x2, int y2)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawLine (Pen pen, float x1, float y1, float x2, float y2)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawLines (Pen pen, PointF [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawLines (Pen pen, Point [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawPath (Pen pen, GraphicsPath path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawPie (Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawPie (Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawPie (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawPie (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawPolygon (Pen pen, Point [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawPolygon (Pen pen, PointF [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawRectangle (Pen pen, Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawRectangle (Pen pen, float x, float y, float width, float height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawRectangle (Pen pen, int x, int y, int width, int height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawRectangles (Pen pen, RectangleF [] rects)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawRectangles (Pen pen, Rectangle [] rects)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle)
		{
			//throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString (string s, Font font, Brush brush, PointF point)
		{
			//throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString (string s, Font font, Brush brush, PointF point, StringFormat format)
		{
			//throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
		{
			//throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString (string s, Font font, Brush brush, float x, float y)
		{
			//throw new NotImplementedException ();
		}

		[MonoTODO]
		public void DrawString (string s, Font font, Brush brush, float x, float y, StringFormat format)
		{
			//throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EndContainer (GraphicsContainer container)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, EnumerateMetafileProc callback, IntPtr callbackData)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit unit, EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ExcludeClip (Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ExcludeClip (Region region)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, PointF [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, Point [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode, float tension)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode, float tension)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillEllipse (Brush brush, Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillEllipse (Brush brush, RectangleF rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillEllipse (Brush brush, float x, float y, float width, float height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillEllipse (Brush brush, int x, int y, int width, int height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillPath (Brush brush, GraphicsPath path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillPie (Brush brush, Rectangle rect, float startAngle, float sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillPie (Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillPie (Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillPolygon (Brush brush, PointF [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillPolygon (Brush brush, Point [] points)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillPolygon (Brush brush, Point [] points, FillMode fillMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillPolygon (Brush brush, PointF [] points, FillMode fillMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillRectangle (Brush brush, RectangleF rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillRectangle (Brush brush, Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillRectangle (Brush brush, int x, int y, int width, int height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillRectangle (Brush brush, float x, float y, float width, float height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillRectangles (Brush brush, Rectangle [] rects)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillRectangles (Brush brush, RectangleF [] rects)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void FillRegion (Brush brush, Region region)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Flush ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Flush (FlushIntention intention)
		{
			throw new NotImplementedException ();
		}

		IntPtr hdc_ = IntPtr.Zero;
		[MonoTODO]
		public static Graphics FromHdc (IntPtr hdc)
		{
			Graphics result = new Graphics(hdc);
			return result;
		}

		[MonoTODO]
		public static Graphics FromHdc (IntPtr hdc, IntPtr hdevice)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromHwnd (IntPtr hwnd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromHwndInternal (IntPtr hwnd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromImage (Image image)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IntPtr GetHalftonePalette ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IntPtr GetHdc ()
		{
			return hdc_;
		}

		[MonoTODO]
		public Color GetNearestColor (Color color)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void IntersectClip (Region region)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void IntersectClip (RectangleF rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void IntersectClip (Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (Point point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (RectangleF rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (PointF point)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (float x, float y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (int x, int y)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (float x, float y, float width, float height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool IsVisible (int x, int y, int width, int height)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public Region [] MeasureCharacterRanges (string text, Font font, RectangleF layoutRect, StringFormat stringFormat)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, SizeF layoutArea)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, int width)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, int width, StringFormat format)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, PointF origin, StringFormat stringFormat)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat, ref int charactersFitted, ref int linesFilled)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void MultiplyTransform (Matrix matrix)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ReleaseHdc (IntPtr hdc)
		{
		}

		[MonoTODO]
		public void ReleaseHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResetClip ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ResetTransform ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Restore (GraphicsState gstate)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RotateTransform (float angle)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RotateTransform (float angle, MatrixOrder order)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public GraphicsState Save ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ScaleTransform (float sx, float sy)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (RectangleF rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (GraphicsPath path)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (Rectangle rect)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (Graphics g)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (Graphics g, CombineMode combineMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (Rectangle rect, CombineMode combineMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (RectangleF rect, CombineMode combineMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (Region region, CombineMode combineMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetClip (GraphicsPath path, CombineMode combineMode)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF [] pts)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, Point [] pts)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void TranslateClip (int dx, int dy)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void TranslateClip (float dx, float dy)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void TranslateTransform (float dx, float dy)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void TranslateTransform (float dx, float dy, MatrixOrder order)
		{
			throw new NotImplementedException ();
		}

		public Region Clip
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				//throw new NotImplementedException ();
			}
		}

		public RectangleF ClipBounds
		{
			get {
				throw new NotImplementedException ();
			}
		}

		public CompositingMode CompositingMode
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}

		}
		public CompositingQuality CompositingQuality
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public float DpiX
		{
			get {
				throw new NotImplementedException ();
			}
		}

		public float DpiY
		{
			get {
				throw new NotImplementedException ();
			}
		}

		public InterpolationMode InterpolationMode
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public bool IsClipEmpty
		{
			get {
				throw new NotImplementedException ();
			}
		}

		public bool IsVisibleClipEmpty
		{
			get {
				throw new NotImplementedException ();
			}
		}

		public float PageScale
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public GraphicsUnit PageUnit
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public PixelOffsetMode PixelOffsetMode
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public Point RenderingOrigin
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public SmoothingMode SmoothingMode
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public int TextContrast
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public TextRenderingHint TextRenderingHint
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public Matrix Transform
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public RectangleF VisibleClipBounds
		{
			get {
				throw new NotImplementedException ();
			}
		}
	}
}

