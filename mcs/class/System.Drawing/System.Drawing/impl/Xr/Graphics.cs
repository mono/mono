//
// System.Drawing.Bitmap.cs
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com) (stubbed out)
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
	namespace XrImpl {

		internal class GraphicsFactory : IGraphicsFactory {

			public System.Drawing.IGraphics Graphics (IntPtr nativeGraphics)
			{
				return new Graphics (nativeGraphics);
			}

			public System.Drawing.IGraphics FromImage (System.Drawing.Image image)
			{
				return XrImpl.Graphics.FromImage (image);
			}

			public System.Drawing.IGraphics FromHwnd (IntPtr hwnd)
			{
				return XrImpl.Graphics.FromHwnd (hwnd);
			}
		}


		[ComVisible (false)]
		internal sealed class Graphics : MarshalByRefObject, IGraphics
		{
			public delegate bool EnumerateMetafileProc (EmfPlusRecordType recordType,
								    int flags,
								    int dataSize,
								    IntPtr data,
								    PlayRecordCallback callbackData);
			
			public delegate bool DrawImageAbort (IntPtr callbackData);

			internal enum GraphicsType {
				FromHDC, FromHwnd, FromImage
			};

			internal GraphicsType type;
			internal IntPtr native_object = IntPtr.Zero;
			internal IntPtr initial_hwnd = IntPtr.Zero;
			internal System.Drawing.XrImpl.Image initialized_from_image = null;

			internal const double C1 = 0.552; // this is some *magic* number

			internal Graphics (IntPtr nativeGraphics)
			{
				native_object = nativeGraphics;
			}

			#region Converters
			internal static Pen ConvertPen (System.Drawing.Pen pen) 
			{
				return pen.implementation as Pen;
			}

			internal static Brush ConvertBrush (System.Drawing.Brush brush) 
			{
				return brush.implementation as Brush;
			}

			internal static Image ConvertImage (System.Drawing.Image image) 
			{
				return image.implementation as Image;
			}
			
			internal static Font ConvertFont (System.Drawing.Font font) 
			{
				return font.implementation as Font;
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
				switch (type) {
					case GraphicsType.FromHwnd:
						break;
					case GraphicsType.FromHDC:
						break;
					case GraphicsType.FromImage:
						Xr.XrDestroy (native_object);
						break;
				}
			}

			void IGraphics.DrawArc (System.Drawing.Pen pen, Rectangle rect, float startAngle, float sweepAngle)
			{
				((IGraphics) this).DrawArc (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
			}

			void IGraphics.DrawArc (System.Drawing.Pen pen, RectangleF rect, float startAngle, float sweepAngle)
			{
				((IGraphics) this).DrawArc (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
			}

			[MonoTODO]
			void IGraphics.DrawArc (System.Drawing.Pen pen,
					float x, float y, float width, float height, float startAngle, float sweepAngle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawArc (System.Drawing.Pen pen,
					int x, int y, int width, int height, int startAngle, int sweepAngle)
			{
				throw new NotImplementedException ();
			}

			void IGraphics.DrawBezier (System.Drawing.Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
			{
				((IGraphics) this).DrawBezier (pen, pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
			}

			void IGraphics.DrawBezier (System.Drawing.Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
			{
				((IGraphics) this).DrawBezier (pen, pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);
			}

			void IGraphics.DrawBezier (System.Drawing.Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
			{
				Pen xrPen = ConvertPen (pen);
				xrPen.SetXrValues (native_object);

				Xr.XrMoveTo (native_object, x1, y1);
				Xr.XrCurveTo (native_object, x2, y2, x3, y3, x4, y4);

				Xr.XrStroke (native_object);
			}

			void IGraphics.DrawBeziers (System.Drawing.Pen pen, Point [] points)
			{
				Pen xrPen = ConvertPen (pen);
				xrPen.SetXrValues (native_object);

				Xr.XrMoveTo (native_object, points [0].X, points [0].Y);

				for (int i = 1; i < points.Length; i += 3)
					Xr.XrCurveTo (native_object,
							points [i].X, points [i].Y,
							points [i + 1].X, points [i + 1].Y,
							points [i + 2].X, points [i + 2].Y);

				Xr.XrStroke (native_object);
			}

			void IGraphics.DrawBeziers (System.Drawing.Pen pen, PointF [] points)
			{
				Pen xrPen = ConvertPen (pen);
				xrPen.SetXrValues (native_object);

				Xr.XrMoveTo (native_object, points [0].X, points [0].Y);

				for (int i = 1; i < points.Length; i += 3)
					Xr.XrCurveTo (native_object,
							points [i].X, points [i].Y,
							points [i + 1].X, points [i + 1].Y,
							points [i + 2].X, points [i + 2].Y);

				Xr.XrStroke (native_object);
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

			void IGraphics.DrawEllipse (System.Drawing.Pen pen, Rectangle rect)
			{
				((IGraphics) this).DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
			}

			void IGraphics.DrawEllipse (System.Drawing.Pen pen, RectangleF rect)
			{
				((IGraphics) this).DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
			}

			void IGraphics.DrawEllipse (System.Drawing.Pen pen, int x, int y, int width, int height)
			{
				((IGraphics) this).DrawEllipse (pen, (float) x, (float) y, (float) width, (float) height);
			}

			void IGraphics.DrawEllipse (System.Drawing.Pen pen, float x, float y, float width, float height)
			{
				double rx = width / 2;
				double ry = height / 2;
				double cx = x + rx;
				double cy = y + ry;

				Pen xrPen = ConvertPen (pen);

				xrPen.SetXrValues (native_object);
				Xr.XrMoveTo (native_object, cx + rx, cy);

				// Do an approprimation of the ellipse by drawing a curve in each quatrant
				Xr.XrCurveTo (native_object,
						cx + rx, cy - C1 * ry,
						cx + C1 * rx, cy - ry,
						cx, cy - ry);
				Xr.XrCurveTo (native_object,
						cx - C1 * rx, cy - ry,
						cx - rx, cy - C1 * ry,
						cx - rx, cy);
				Xr.XrCurveTo (native_object,
						cx - rx, cy + C1 * ry,
						cx - C1 * rx, cy + ry,
						cx, cy + ry);
				Xr.XrCurveTo (native_object,
						cx + C1 * rx, cy + ry,
						cx + rx, cy + C1 * ry,
						cx + rx, cy);

				Xr.XrClosePath (native_object);
				Xr.XrStroke (native_object);
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
				throw new NotImplementedException ();
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

			void DrawLine (Pen xrPen, float x1, float y1, float x2, float y2) 
			{
				xrPen.SetXrValues (native_object);
				Xr.XrMoveTo (native_object, (double)x1, (double)y1);
				Xr.XrLineTo (native_object, (double)x2, (double)y2);
				Xr.XrStroke (native_object);
			}
			
			[MonoTODO]
			void IGraphics.DrawLine (System.Drawing.Pen pen, PointF pt1, PointF pt2)
			{
				DrawLine (ConvertPen (pen), pt1.X, pt1.Y, pt2.X, pt2.Y);
			}

			[MonoTODO]
			void IGraphics.DrawLine (System.Drawing.Pen pen, Point pt1, Point pt2)
			{
				DrawLine (ConvertPen (pen), (float)pt1.X, (float)pt1.Y, (float)pt2.X, (float)pt2.Y);
			}

			[MonoTODO]
			void IGraphics.DrawLine (System.Drawing.Pen pen, int x1, int y1, int x2, int y2)
			{
				DrawLine (ConvertPen (pen), (float)x1, (float)y1, (float)x2, (float)y2);
			}

			[MonoTODO]
			void IGraphics.DrawLine (System.Drawing.Pen pen, float x1, float y1, float x2, float y2)
			{
				DrawLine (ConvertPen (pen), x1, y1, x2, y2);
			}

			[MonoTODO]
			void IGraphics.DrawLines (System.Drawing.Pen pen, PointF [] points)
			{
				if (points.Length != 0) {
					Pen XrPen = ConvertPen (pen);
					XrPen.SetXrValues (native_object);
					Xr.XrMoveTo (native_object, (double)points[0].X, (double)points[0].Y);
					if (points.Length == 1) {
						Xr.XrLineTo (native_object, (double)points[0].X, (double)points[0].Y);
					}
					else {
						for (int i = 1; i < points.Length; i++) {
							Xr.XrLineTo (native_object, (double)points[i].X, (double)points[i].Y);
						}
					}
					Xr.XrStroke (native_object);
				}
			}

			[MonoTODO]
			void IGraphics.DrawLines (System.Drawing.Pen pen, Point [] points)
			{
				if (points.Length != 0) {
					PointF[] pointsf = new PointF[points.Length];
					for (int i = 0; i < points.Length; i++) {
						pointsf[i] = new PointF (points[i].X, points[i].Y);
					}
					 ((IGraphics)this).DrawLines (pen, pointsf);
				}
			}

			[MonoTODO]
			void IGraphics.DrawPath (System.Drawing.Pen pen, GraphicsPath path)
			{
				throw new NotImplementedException ();
			}

			void IGraphics.DrawPie (System.Drawing.Pen pen, Rectangle rect, float startAngle, float sweepAngle)
			{
				((IGraphics) this).DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
			}

			void IGraphics.DrawPie (System.Drawing.Pen pen, RectangleF rect, float startAngle, float sweepAngle)
			{
				((IGraphics) this).DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
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

			void IGraphics.DrawPolygon (System.Drawing.Pen pen, Point [] points)
			{
				Pen xrPen = ConvertPen (pen);

				xrPen.SetXrValues (native_object);
				Xr.XrMoveTo (native_object, (double) points [0].X, (double) points [0].Y);

				for (int i = 1; i < points.Length; i ++)
					Xr.XrLineTo (native_object, (double) points [i].X, (double) points [i].Y);
				
				Xr.XrClosePath (native_object);
				Xr.XrStroke (native_object);
			}

			void IGraphics.DrawPolygon (System.Drawing.Pen pen, PointF [] points)
			{
				Pen xrPen = ConvertPen (pen);

				xrPen.SetXrValues (native_object);

				for (int i = 0; i < points.Length; i += 2)
					((IGraphics) this).DrawLine (pen, points [i], points [i + 1]);
				
				Xr.XrClosePath (native_object);
				Xr.XrStroke (native_object);
			}

			[MonoTODO]
			void IGraphics.DrawRectangle (System.Drawing.Pen pen, Rectangle rect)
			{
				DrawRectangle (ConvertPen (pen), (double)rect.Left, (double)rect.Top, (double)rect.Width, (double)rect.Height);
			}

			[MonoTODO]
			void IGraphics.DrawRectangle (System.Drawing.Pen pen, float x, float y, float width, float height)
			{
				DrawRectangle (ConvertPen (pen), (double)x, (double)y, (double)width, (double)height);
			}

			void DrawRectangle (Pen XrPen, double x, double y, double width, double height)
			{
				XrPen.SetXrValues (native_object);

				Xr.XrMoveTo (native_object, x, y);
				Xr.XrLineTo (native_object, x + width, y);
				Xr.XrLineTo (native_object, x + width, y + height);
				Xr.XrLineTo (native_object, x, y + height);
				Xr.XrLineTo (native_object, x, y);
				Xr.XrStroke (native_object);
			}

			[MonoTODO]
			void IGraphics.DrawRectangle (System.Drawing.Pen pen, int x, int y, int width, int height)
			{
				DrawRectangle (ConvertPen (pen), (double)x, (double)y, (double)width, (double)height);
			}

			[MonoTODO]
			void IGraphics.DrawRectangles (System.Drawing.Pen pen, RectangleF [] rects)
			{
				foreach (RectangleF rc in rects) 
				{
					DrawRectangle (ConvertPen (pen), (double)rc.Left, (double)rc.Top, (double)rc.Width, (double)rc.Height);
				}
			}

			[MonoTODO]
			void IGraphics.DrawRectangles (System.Drawing.Pen pen, Rectangle [] rects)
			{
				foreach (RectangleF rc in rects) 
				{
					DrawRectangle (ConvertPen (pen), (double)rc.Left, (double)rc.Top, (double)rc.Width, (double)rc.Height);
				}
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, RectangleF layoutRectangle)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, PointF point)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, PointF point, StringFormat format)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, RectangleF layoutRectangle, StringFormat format)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y)
			{
				Font xrFont = ConvertFont (font);
				xrFont.SetXrValues (native_object);
				Brush xrBrush = ConvertBrush (brush);
				if (xrBrush is SolidBrush) {
					SolidBrush xrSolidBrush = xrBrush as SolidBrush;
					xrSolidBrush.SetXrValues (native_object);
					Xr.XrMoveTo (native_object, (double)x, (double)y);
					Xr.XrShowText (native_object, s);
				}
				//throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.DrawString (string s, System.Drawing.Font font, System.Drawing.Brush brush, float x, float y, StringFormat format)
			{
				throw new NotImplementedException ();
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
				FillRectangle (ConvertBrush (brush), rect.Left, rect.Top, rect.Width, rect.Height);
			}

			[MonoTODO]
			void IGraphics.FillRectangle (System.Drawing.Brush brush, Rectangle rect)
			{
				FillRectangle (ConvertBrush (brush), (float)rect.Left, (float)rect.Top, (float)rect.Width, (float)rect.Height);
			}

			void FillRectangle (Brush brush, RectangleF rect)
			{
				FillRectangle (brush, rect.Left, rect.Top, rect.Width, rect.Height);
			}

			void FillRectangle (Brush brush, Rectangle rect)
			{
				FillRectangle (brush, (float)rect.Left, (float)rect.Top, (float)rect.Width, (float)rect.Height);
			}

			void FillRectangle (Brush brush, float x, float y, float width, float height)
			{
				//throw new NotImplementedException ();
				if (brush is SolidBrush) {
					SolidBrush xrBrush = brush as SolidBrush;
					xrBrush.SetXrValues (native_object);
					Xr.XrRectangle (native_object, x, y, width, height);
					Xr.XrFill (native_object);
				}
			}

			[MonoTODO]
			void IGraphics.FillRectangle (System.Drawing.Brush brush, int x, int y, int width, int height)
			{
				FillRectangle (brush, (float)x, (float)y, (float)width, (float)height);
			}

			[MonoTODO]
			void IGraphics.FillRectangle (System.Drawing.Brush brush, float x, float y, float width, float height)
			{
				FillRectangle (ConvertBrush (brush), x, y, width, height);
			}

			[MonoTODO]
			void IGraphics.FillRectangles (System.Drawing.Brush brush, Rectangle [] rects)
			{
				if (rects != null) 
				{
					foreach (Rectangle rc in rects) 
					{
						FillRectangle (ConvertBrush (brush), rc);
					}
				}
			}

			[MonoTODO]
			void IGraphics.FillRectangles (System.Drawing.Brush brush, RectangleF [] rects)
			{
				if (rects != null) 
				{
					foreach (RectangleF rc in rects) 
					{
						FillRectangle (ConvertBrush (brush), rc);
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
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IGraphics.Flush (FlushIntention intention)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public static Graphics FromHdc (IntPtr hdc)
			{
				Graphics result = new Graphics (hdc);
				result.type = GraphicsType.FromHDC;
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
			public static Graphics FromImage (System.Drawing.Image image)
			{
				System.Drawing.XrImpl.Image	xrImage = ConvertImage (image);
				IntPtr nativeObj = Xr.XrCreate ();
				Graphics result = new Graphics (nativeObj);
				Xr.XrSetTargetImage (nativeObj, GDK.gdk_pixbuf_get_pixels (xrImage.native_object),
						xrImage.xr_format, ((IImage) xrImage).Width, ((IImage) xrImage).Height,
						GDK.gdk_pixbuf_get_rowstride (xrImage.native_object));
				
				xrImage.selected_into_graphics = result;
				result.initialized_from_image = xrImage;
				result.type = GraphicsType.FromImage;
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
				return native_object;
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
				throw new NotImplementedException ();
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
				Xr.XrRestore (native_object);
			}

			void IGraphics.RotateTransform (float angle)
			{
				double rad = angle * Math.PI / 180.0;				
				Xr.XrRotate (native_object, rad);
			}

			[MonoTODO]
			void IGraphics.RotateTransform (float angle, MatrixOrder order)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public GraphicsState Save ()
			{
				Xr.XrSave (native_object);
				return new GraphicsState ();
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
				Xr.XrTranslate (native_object, (double)dx, (double)dy);
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

