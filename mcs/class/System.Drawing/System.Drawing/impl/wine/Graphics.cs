//
// System.Drawing.Bitmap.cs
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com) (stubbed out)
//  Alexandre Pigolkine (pigolkine@gmx.de)
//
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace System.Drawing
{
	namespace Win32Impl	{

		internal class GraphicsFactory : IGraphicsFactory {

			public System.Drawing.IGraphics Graphics(IntPtr nativeGraphics) {
				return new Graphics(nativeGraphics);
			}

			public System.Drawing.IGraphics FromImage(System.Drawing.Image image) {
				return Win32Impl.Graphics.FromImage(image);
			}

			public System.Drawing.IGraphics FromHwnd( IntPtr hwnd) {
				return Win32Impl.Graphics.FromHwnd(hwnd);
			}
		}


		[ComVisible(false)]
		internal sealed class Graphics : MarshalByRefObject, IGraphics
		{
			public delegate bool EnumerateMetafileProc (EmfPlusRecordType recordType,
				int flags,
				int dataSize,
				IntPtr data,
				PlayRecordCallback callbackData);

			public delegate bool DrawImageAbort (IntPtr callbackData);

			internal enum GraphicsType {
				fromHdc, fromHwnd, fromImage
			};

			internal GraphicsType type_;
			internal IntPtr hdc_ = IntPtr.Zero;
			internal IntPtr initialBitmap_ = IntPtr.Zero;
			internal IntPtr initialHwnd_ = IntPtr.Zero;
			internal System.Drawing.Win32Impl.Image initializedFromImage_ = null;

			internal Graphics (IntPtr nativeGraphics)
			{
				hdc_ = nativeGraphics;
			}

			#region Converters
			internal static Pen ConvertPen( System.Drawing.Pen pen) 
			{
				return pen.implementation_ as Pen;
			}

			internal static Brush ConvertBrush( System.Drawing.Brush brush) 
			{
				return brush.implementation_ as Brush;
			}

			internal static Image ConvertImage( System.Drawing.Image image) 
			{
				return image.implementation_ as Image;
			}
			#endregion

			[MonoTODO]
			void IGraphics.AddMetafileComment (byte [] data)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			GraphicsContainer IGraphics.BeginContainer ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			GraphicsContainer IGraphics.BeginContainer (Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			GraphicsContainer IGraphics.BeginContainer (RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.Clear (Color color)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IDisposable.Dispose ()
			{
				switch(type_) {
					case GraphicsType.fromHwnd:
						Win32.ReleaseDC(initialHwnd_, hdc_);
						break;
					case GraphicsType.fromHdc:
						break;
					case GraphicsType.fromImage:
						Win32.SelectObject(hdc_, initialBitmap_);
						initializedFromImage_.selectedIntoGraphics_ = null;
						break;
				}
			}

			[MonoTODO]
			void IGraphics.DrawArc (System.Drawing.Pen pen, Rectangle rect, float startAngle, float sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawArc (System.Drawing.Pen pen, RectangleF rect, float startAngle, float sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawArc (System.Drawing.Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawArc (System.Drawing.Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawBezier (System.Drawing.Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawBezier (System.Drawing.Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawBezier (System.Drawing.Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawBeziers (System.Drawing.Pen pen, Point [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawBeziers (System.Drawing.Pen pen, PointF [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawClosedCurve (System.Drawing.Pen pen, PointF [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawClosedCurve (System.Drawing.Pen pen, Point [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawClosedCurve (System.Drawing.Pen pen, Point [] points, float tension, FillMode fillmode)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawClosedCurve (System.Drawing.Pen pen, PointF [] points, float tension, FillMode fillmode)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawCurve (System.Drawing.Pen pen, Point [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawCurve (System.Drawing.Pen pen, PointF [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawCurve (System.Drawing.Pen pen, PointF [] points, float tension)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawCurve (System.Drawing.Pen pen, Point [] points, float tension)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawCurve (System.Drawing.Pen pen, PointF [] points, int offset, int numberOfSegments)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawCurve (System.Drawing.Pen pen, Point [] points, int offset, int numberOfSegments, float tension)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawCurve (System.Drawing.Pen pen, PointF [] points, int offset, int numberOfSegments, float tension)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawEllipse (System.Drawing.Pen pen, Rectangle rect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawEllipse (System.Drawing.Pen pen, RectangleF rect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawEllipse (System.Drawing.Pen pen, int x, int y, int width, int height)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawEllipse (System.Drawing.Pen pen, float x, float y, float width, float height)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawIcon (System.Drawing.Icon icon, Rectangle targetRect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawIcon (System.Drawing.Icon icon, int x, int y)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawIconUnstretched (System.Drawing.Icon icon, Rectangle targetRect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, RectangleF rect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, PointF point)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Point [] destPoints)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Point point)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Rectangle rect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, PointF [] destPoints)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, int x, int y)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, float x, float y)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, float x, float y, float width, float height)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, int x, int y, int width, int height)
			{
				System.Drawing.Win32Impl.Image wineImage = image.implementation_ as System.Drawing.Win32Impl.Image;
				Graphics imageGraphics = wineImage.selectedIntoGraphics_;
				if( imageGraphics == null) {
					IntPtr tempDC = Win32.CreateCompatibleDC (hdc_);
					IntPtr oldBmp = Win32.SelectObject (tempDC, wineImage.nativeObject_);
					Win32.BitBlt(hdc_, x, y, width, height, tempDC, 0, 0, PatBltTypes.SRCCOPY);
					Win32.SelectObject (tempDC, oldBmp);
					Win32.DeleteDC (tempDC);
				}
				else {
					Win32.BitBlt(hdc_, x, y, width, height, imageGraphics.hdc_, 0, 0, PatBltTypes.SRCCOPY);
				}
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, System.Drawing.Graphics.DrawImageAbort callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, System.Drawing.Graphics.DrawImageAbort callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, System.Drawing.Graphics.DrawImageAbort callback, int callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, System.Drawing.Graphics.DrawImageAbort callback, int callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, System.Drawing.Graphics.DrawImageAbort callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, System.Drawing.Graphics.DrawImageAbort callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, System.Drawing.Graphics.DrawImageAbort callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImage (System.Drawing.Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs, System.Drawing.Graphics.DrawImageAbort callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImageUnscaled (System.Drawing.Image image, Point point)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImageUnscaled (System.Drawing.Image image, Rectangle rect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImageUnscaled (System.Drawing.Image image, int x, int y)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawImageUnscaled (System.Drawing.Image image, int x, int y, int width, int height)
			{
				throw new NotImplementedException ();
			}

			void DrawLine (Pen winePen, int x1, int y1, int x2, int y2)
			{
				POINT[] pts = new POINT[2];
				pts[0].x = x1;
				pts[0].y = y1;
				pts[1].x = x2;
				pts[1].y = y2;
				IntPtr oldPen = Win32.SelectObject(hdc_, winePen.hpen_);
				Win32.Polyline( hdc_, pts, 2);
				Win32.SelectObject(hdc_, oldPen);
			}

			[MonoTODO]
			void IGraphics.DrawLine (System.Drawing.Pen pen, PointF pt1, PointF pt2)
			{
				DrawLine( ConvertPen(pen), (int)pt1.X, (int)pt1.Y, (int)pt2.X, (int)pt2.Y);
			}

			[MonoTODO]
			void IGraphics.DrawLine (System.Drawing.Pen pen, Point pt1, Point pt2)
			{
				DrawLine( ConvertPen(pen), (int)pt1.X, (int)pt1.Y, (int)pt2.X, (int)pt2.Y);
			}

			[MonoTODO]
			void IGraphics.DrawLine (System.Drawing.Pen pen, int x1, int y1, int x2, int y2)
			{
				DrawLine( ConvertPen(pen), x1, y1, x2, y2);
			}

			[MonoTODO]
			void IGraphics.DrawLine (System.Drawing.Pen pen, float x1, float y1, float x2, float y2)
			{
				DrawLine( ConvertPen(pen), (int)x1, (int)y1, (int)x2, (int)y2);
			}

			[MonoTODO]
			void IGraphics.DrawLines (System.Drawing.Pen pen, PointF [] points)
			{
				//throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawLines (System.Drawing.Pen pen, Point [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawPath (System.Drawing.Pen pen, GraphicsPath path)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawPie (System.Drawing.Pen pen, Rectangle rect, float startAngle, float sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawPie (System.Drawing.Pen pen, RectangleF rect, float startAngle, float sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawPie (System.Drawing.Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawPie (System.Drawing.Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawPolygon (System.Drawing.Pen pen, Point [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawPolygon (System.Drawing.Pen pen, PointF [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawRectangle (System.Drawing.Pen pen, Rectangle rect)
			{
				DrawRectangle(ConvertPen(pen), rect.Left, rect.Top, rect.Width, rect.Height);
			}

			[MonoTODO]
			void IGraphics.DrawRectangle (System.Drawing.Pen pen, float x, float y, float width, float height)
			{
				DrawRectangle(ConvertPen(pen), (int)x, (int)y, (int)width, (int)height);
			}

			void DrawRectangle (Pen winePen, int x, int y, int width, int height)
			{
				POINT[] pts = new POINT[5];
				pts[0].x = x;
				pts[0].y = y;
				pts[1].x = x;
				pts[1].y = y + height;
				pts[2].x = x + width;
				pts[2].y = y + height;
				pts[3].x = x + width;
				pts[3].y = y;
				pts[4].x = x;
				pts[4].y = y;
				IntPtr oldPen = Win32.SelectObject(hdc_, winePen.hpen_);
				Win32.Polyline( hdc_, pts, 5);
				Win32.SelectObject(hdc_, oldPen);
			}

			[MonoTODO]
			void IGraphics.DrawRectangle (System.Drawing.Pen pen, int x, int y, int width, int height)
			{
				DrawRectangle(ConvertPen(pen), x, y, width, height);
			}

			[MonoTODO]
			void IGraphics.DrawRectangles (System.Drawing.Pen pen, RectangleF [] rects)
			{
				foreach( RectangleF rc in rects) 
				{
					DrawRectangle(ConvertPen(pen), (int)rc.Left, (int)rc.Top, (int)rc.Width, (int)rc.Height);
				}
			}

			[MonoTODO]
			void IGraphics.DrawRectangles (System.Drawing.Pen pen, Rectangle [] rects)
			{
				foreach( RectangleF rc in rects) 
				{
					DrawRectangle(ConvertPen(pen), (int)rc.Left, (int)rc.Top, (int)rc.Width, (int)rc.Height);
				}
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, RectangleF layoutRectangle)
			{
				//throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, PointF point)
			{
				//throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, PointF point, StringFormat format)
			{
				//throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, RectangleF layoutRectangle, StringFormat format)
			{
				//throw new NotImplementedException ();
				RECT rc = new RECT();
				rc.left = (int)layoutRectangle.Left;
				rc.top = (int)layoutRectangle.Top;
				rc.right = (int)layoutRectangle.Right;
				rc.bottom = (int)layoutRectangle.Bottom;
				IntPtr prevFont = Win32.SelectObject(hdc_, font.ToHfont());
				Win32.SetTextColor(hdc_, Win32.RGB(((IBrush)ConvertBrush(brush)).TextColor));
				Win32.SetBkMode(hdc_, BackgroundMode.TRANSPARENT);
				Win32.ExtTextOut(hdc_, (int)layoutRectangle.Left, (int)layoutRectangle.Top, 0, ref rc, 
					s, s.Length, IntPtr.Zero);
				Win32.SelectObject(hdc_, prevFont);
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y)
			{
				IntPtr prevFont = Win32.SelectObject(hdc_, font.ToHfont());
				Win32.SetTextColor(hdc_, Win32.RGB(((IBrush)ConvertBrush(brush)).TextColor));
				Win32.SetBkMode(hdc_, BackgroundMode.TRANSPARENT);
				Win32.ExtTextOut(hdc_, (int)x, (int)y, 0, IntPtr.Zero, 
					s, s.Length, IntPtr.Zero);
				Win32.SelectObject(hdc_, prevFont);
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y, StringFormat format)
			{
				//throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EndContainer (GraphicsContainer container)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point [] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, RectangleF destRect, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF [] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Rectangle destRect, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Rectangle destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF [] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point [] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, RectangleF destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, RectangleF destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF destPoint, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point [] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF [] destPoints, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Rectangle destRect, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point [] destPoints, Rectangle srcRect, GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Rectangle destRect, Rectangle srcRect, GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, Point destPoint, Rectangle srcRect, GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, RectangleF destRect, RectangleF srcRect, GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF [] destPoints, RectangleF srcRect, GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.EnumerateMetafile (Metafile metafile, PointF destPoint, RectangleF srcRect, GraphicsUnit unit, System.Drawing.Graphics.EnumerateMetafileProc callback, IntPtr callbackData, ImageAttributes imageAttr)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.ExcludeClip (Rectangle rect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.ExcludeClip (System.Drawing.Region region)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillClosedCurve (System.Drawing.Brush brush, PointF [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillClosedCurve (System.Drawing.Brush brush, Point [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillClosedCurve (System.Drawing.Brush brush, PointF [] points, FillMode fillmode)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillClosedCurve (System.Drawing.Brush brush, Point [] points, FillMode fillmode)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillClosedCurve (System.Drawing.Brush brush, PointF [] points, FillMode fillmode, float tension)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillClosedCurve (System.Drawing.Brush brush, Point [] points, FillMode fillmode, float tension)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillEllipse (System.Drawing.Brush brush, Rectangle rect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillEllipse (System.Drawing.Brush brush, RectangleF rect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillEllipse (System.Drawing.Brush brush, float x, float y, float width, float height)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillEllipse (System.Drawing.Brush brush, int x, int y, int width, int height)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillPath (System.Drawing.Brush brush, GraphicsPath path)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillPie (System.Drawing.Brush brush, Rectangle rect, float startAngle, float sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillPie (System.Drawing.Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillPie (System.Drawing.Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillPolygon (System.Drawing.Brush brush, PointF [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillPolygon (System.Drawing.Brush brush, Point [] points)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillPolygon (System.Drawing.Brush brush, Point [] points, FillMode fillMode)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillPolygon (System.Drawing.Brush brush, PointF [] points, FillMode fillMode)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.FillRectangle (System.Drawing.Brush brush, RectangleF rect)
			{
				FillRectangle( ConvertBrush(brush), (int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
			}

			[MonoTODO]
			void IGraphics.FillRectangle (System.Drawing.Brush brush, Rectangle rect)
			{
				FillRectangle( ConvertBrush(brush), rect.Left, rect.Top, rect.Width, rect.Height);
			}

			void FillRectangle (Brush wineBrush, RectangleF rect)
			{
				FillRectangle( wineBrush, (int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);
			}

			void FillRectangle (Brush wineBrush, Rectangle rect)
			{
				FillRectangle( wineBrush, rect.Left, rect.Top, rect.Width, rect.Height);
			}

			void FillRectangle (Brush wineBrush, int x, int y, int width, int height)
			{
				RECT rc = new RECT();
				rc.left = x;
				rc.top = y;
				rc.right = rc.left + width /*+ 1*/;
				rc.bottom = rc.top + height /*+ 1*/;
				Win32.FillRect( hdc_, ref rc, wineBrush.hbrush_);
			}

			[MonoTODO]
			void IGraphics.FillRectangle (System.Drawing.Brush brush, int x, int y, int width, int height)
			{
				System.Drawing.Win32Impl.Brush wineBrush = ConvertBrush(brush);
				// Do the job
				if( wineBrush is SolidBrush) 
				{
					FillRectangle(wineBrush, x, y, width, height);
				}
				else if( brush is LinearGradientBrush) 
				{
					// FIXME: just to indicate 2 colours
					LinearGradientBrush br = brush as LinearGradientBrush;
					Color[] colors = br.LinearColors;
					SolidBrush	sb1 = new SolidBrush(colors[0]);
					SolidBrush	sb2 = new SolidBrush(colors[1]);
					// FIXME: find a way to call those
					FillRectangle(sb1, x, y, width / 2, height);
					FillRectangle(sb2, x + width / 2, y, width , height);
					sb1.Dispose();
					sb2.Dispose();
				}
			}

			[MonoTODO]
			void IGraphics.FillRectangle (System.Drawing.Brush brush, float x, float y, float width, float height)
			{
				FillRectangle( ConvertBrush(brush), (int)x, (int)y, (int)width, (int)height);
			}

			[MonoTODO]
			void IGraphics.FillRectangles (System.Drawing.Brush brush, Rectangle [] rects)
			{
				if(rects != null) 
				{
					foreach( Rectangle rc in rects) 
					{
						FillRectangle(ConvertBrush(brush), rc);
					}
				}
			}

			[MonoTODO]
			void IGraphics.FillRectangles (System.Drawing.Brush brush, RectangleF [] rects)
			{
				if(rects != null) 
				{
					foreach( RectangleF rc in rects) 
					{
						FillRectangle(ConvertBrush(brush), rc);
					}
				}
			}

			[MonoTODO]
			void IGraphics.FillRegion (System.Drawing.Brush brush, System.Drawing.Region region)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.Flush ()
			{
				((IGraphics)this).Flush(FlushIntention.Flush);
			}

			[MonoTODO]
			void IGraphics.Flush (FlushIntention intention)
			{
				Win32.GdiFlush ();
			}

			[MonoTODO]
			public static Graphics FromHdc (IntPtr hdc)
			{
				Graphics result = new Graphics(hdc);
				result.type_ = GraphicsType.fromHdc;
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
				IntPtr hdc = Win32.GetDC(hwnd);
				Graphics result = new Graphics(hdc);
				result.type_ = GraphicsType.fromHwnd;
				return result;
			}

			[MonoTODO]
			public static Graphics FromHwndInternal (IntPtr hwnd)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public static Graphics FromImage (System.Drawing.Image image)
			{
				System.Drawing.Win32Impl.Image	wineImage = ConvertImage(image);
				IntPtr display = Win32.CreateDC("DISPLAY", "", "", IntPtr.Zero);
				IntPtr hdc = Win32.CreateCompatibleDC(display);
				Graphics result = new Graphics( hdc);
				result.initialBitmap_ = Win32.SelectObject(hdc, wineImage.nativeObject_);
				wineImage.selectedIntoGraphics_ = result;
				result.initializedFromImage_ = wineImage;
				Win32.DeleteDC(display);
				result.type_ = GraphicsType.fromImage;
				return result;
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
			void IGraphics.IntersectClip (System.Drawing.Region region)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.IntersectClip (RectangleF rect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.IntersectClip (Rectangle rect)
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
			public System.Drawing.Region [] MeasureCharacterRanges (string text, System.Drawing.Font font, RectangleF layoutRect, StringFormat stringFormat)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public SizeF MeasureString (string text, System.Drawing.Font font)
			{
				SIZE sz = new SIZE();
				IntPtr prevFont = Win32.SelectObject(hdc_, font.ToHfont());
				Win32.GetTextExtentPoint32(hdc_, text, text.Length, ref sz);
				Win32.SelectObject(hdc_, prevFont);
				return new SizeF((float)sz.cx, (float)sz.cy);
			}

			[MonoTODO]
			public SizeF MeasureString (string text, System.Drawing.Font font, SizeF layoutArea)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public SizeF MeasureString (string text, System.Drawing.Font font, int width)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public SizeF MeasureString (string text, System.Drawing.Font font, SizeF layoutArea, StringFormat stringFormat)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public SizeF MeasureString (string text, System.Drawing.Font font, int width, StringFormat format)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public SizeF MeasureString (string text, System.Drawing.Font font, PointF origin, StringFormat stringFormat)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public SizeF MeasureString (string text, System.Drawing.Font font, SizeF layoutArea, StringFormat stringFormat, ref int charactersFitted, ref int linesFilled)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.MultiplyTransform (Matrix matrix)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.MultiplyTransform (Matrix matrix, MatrixOrder order)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			internal void ReleaseHdc (IntPtr hdc)
			{
			}

			[MonoTODO]
			void IGraphics.ReleaseHdc (IntPtr hdc)
			{
			}

			[MonoTODO]
			void IGraphics.ReleaseHdcInternal (IntPtr hdc)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.ResetClip ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.ResetTransform ()
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.Restore (GraphicsState gstate)
			{
				//throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.RotateTransform (float angle)
			{
				//throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.RotateTransform (float angle, MatrixOrder order)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public GraphicsState Save ()
			{
				//throw new NotImplementedException ();
				return new GraphicsState();
			}

			[MonoTODO]
			void IGraphics.ScaleTransform (float sx, float sy)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.ScaleTransform (float sx, float sy, MatrixOrder order)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.SetClip (RectangleF rect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.SetClip (GraphicsPath path)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.SetClip (Rectangle rect)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.SetClip (System.Drawing.Graphics g)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.SetClip (System.Drawing.Graphics g, CombineMode combineMode)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.SetClip (Rectangle rect, CombineMode combineMode)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.SetClip (RectangleF rect, CombineMode combineMode)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.SetClip (System.Drawing.Region region, CombineMode combineMode)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.SetClip (GraphicsPath path, CombineMode combineMode)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF [] pts)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, Point [] pts)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.TranslateClip (int dx, int dy)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.TranslateClip (float dx, float dy)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.TranslateTransform (float dx, float dy)
			{
				//throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.TranslateTransform (float dx, float dy, MatrixOrder order)
			{
				throw new NotImplementedException ();
			}

			System.Drawing.Region System.Drawing.IGraphics.Clip
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					//throw new NotImplementedException ();
				}
			}

			RectangleF IGraphics.ClipBounds
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}

			CompositingMode IGraphics.CompositingMode
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					throw new NotImplementedException ();
				}

			}
			CompositingQuality IGraphics.CompositingQuality
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					throw new NotImplementedException ();
				}
			}

			float IGraphics.DpiX
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}

			float IGraphics.DpiY
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}

			InterpolationMode IGraphics.InterpolationMode
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					throw new NotImplementedException ();
				}
			}

			bool IGraphics.IsClipEmpty
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}

			bool IGraphics.IsVisibleClipEmpty
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}

			float IGraphics.PageScale
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					throw new NotImplementedException ();
				}
			}

			GraphicsUnit IGraphics.PageUnit
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					throw new NotImplementedException ();
				}
			}

			PixelOffsetMode IGraphics.PixelOffsetMode
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					throw new NotImplementedException ();
				}
			}

			Point IGraphics.RenderingOrigin
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					throw new NotImplementedException ();
				}
			}

			SmoothingMode IGraphics.SmoothingMode
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					throw new NotImplementedException ();
				}
			}

			int IGraphics.TextContrast
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					throw new NotImplementedException ();
				}
			}

			TextRenderingHint IGraphics.TextRenderingHint
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					throw new NotImplementedException ();
				}
			}

			Matrix IGraphics.Transform
			{
				get 
				{
					throw new NotImplementedException ();
				}
				set 
				{
					throw new NotImplementedException ();
				}
			}

			RectangleF IGraphics.VisibleClipBounds
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
		}
	}
}

