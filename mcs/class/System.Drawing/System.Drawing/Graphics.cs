//
// System.Drawing.Graphics.cs
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com) (stubbed out)
//      Alexandre Pigolkine(pigolkine@gmx.de)
//		Jordi Mas i Hernandez (jordi@ximian.com)
//
using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Drawing
{
	[ComVisible(false)]
	public sealed class Graphics : MarshalByRefObject, IDisposable
	{
		internal IntPtr nativeObject = IntPtr.Zero;
		internal static float defDpiX = 0;
		internal static float defDpiY = 0;
		
		public delegate bool EnumerateMetafileProc (EmfPlusRecordType recordType,
							    int flags,
							    int dataSize,
							    IntPtr data,
							    PlayRecordCallback callbackData);
		
		public delegate bool DrawImageAbort (IntPtr callbackData);		
		
		private Graphics (IntPtr nativeGraphics)
		{
			if (nativeGraphics == IntPtr.Zero)
				Console.WriteLine ("Here: " + Environment.StackTrace);
			nativeObject = nativeGraphics;
		}
		
		static internal float systemDpiX {
			get {					
					if (defDpiX==0) {
						Bitmap bmp = new Bitmap(1,1);
						Graphics g = Graphics.FromImage(bmp);	
       					defDpiX = g.DpiX;
       				}
       				return defDpiX;
			}
		}

		static internal float systemDpiY {
			get {
					if (defDpiY==0) {
						Bitmap bmp = new Bitmap(1,1);
						Graphics g = Graphics.FromImage(bmp);	
       					defDpiY = g.DpiY;
       				}
       				return defDpiY;
			}
		}
		
		internal IntPtr NativeObject {
			get{
				return nativeObject;
			}

			set {
				nativeObject = value;
			}
		}

		[MonoTODO]
		public void AddMetafileComment (byte [] data)
		{
			throw new NotImplementedException ();
		}

		
		public GraphicsContainer BeginContainer ()
		{
			int state;

                        GDIPlus.GdipBeginContainer2 (nativeObject, out state);
        	
                        return new GraphicsContainer(state);
		}
		
		public GraphicsContainer BeginContainer (Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit)
		{
			int state;

                        GDIPlus.GdipBeginContainerI (nativeObject, dstrect, srcrect, unit, out state);        	
        	
                        return new GraphicsContainer (state);
		}

		
		public GraphicsContainer BeginContainer (RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit)
		{
			int state;

                        GDIPlus.GdipBeginContainer (nativeObject, dstrect, srcrect, unit, out state);        	
        	
                        return new GraphicsContainer (state);
		}

		
		public void Clear (Color color)
		{			
 			GDIPlus.GdipGraphicsClear(nativeObject, color.ToArgb()); 			
		}

		[MonoTODO]
		public void Dispose ()
		{
		}

		
		public void DrawArc (Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		{
			DrawArc (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}

		
		public void DrawArc (Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		{
			DrawArc (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}

		
		public void DrawArc (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			GDIPlus.GdipDrawArc (nativeObject, pen.nativeObject,
                                        x, y, width, height, startAngle, sweepAngle);                        
		}

		// Microsoft documentation states that the signature for this member should be
		// public void DrawArc( Pen pen,  int x,  int y,  int width,  int height,   int startAngle,
   		// int sweepAngle. However, GdipDrawArcI uses also float for the startAngle and sweepAngle params
   		public void DrawArc (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{			
			GDIPlus.GdipDrawArcI (nativeObject, pen.nativeObject,
                                        x, y, width, height, startAngle, sweepAngle);
                                                    
		}

		public void DrawBezier (Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
		{
			GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject, 
                                        pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);                                     
			
		}

		public void DrawBezier (Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
		{
			GDIPlus.GdipDrawBezierI (nativeObject, pen.nativeObject, 
                                        pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X, pt3.Y, pt4.X, pt4.Y);                                        
		
		}

		public void DrawBezier (Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
		{
			GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject, x1, y1, x2, y2, x3, y3, x4, y4);
		}

                [MonoTODO]
		public void DrawBeziers (Pen pen, Point [] points)
		{
                        int length = points.Length;

                        if (length < 3)
                                return;

			for (int i = 0; i < length; i += 3) {
                                Point p1 = points [i];
                                Point p2 = points [i + 1];
                                Point p3 = points [i + 2];
                                Point p4 = points [i + 3];

                                GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject,
                                                        p1.X, p1.Y, p2.X, p2.Y, 
                                                        p3.X, p3.Y, p4.X, p4.Y);
                        }
		}

                [MonoTODO]
		public void DrawBeziers (Pen pen, PointF [] points)
		{
			int length = points.Length;

                        if (length < 3)
                                return;

			for (int i = 0; i < length; i += 3) {
                                PointF p1 = points [i];
                                PointF p2 = points [i + 1];
                                PointF p3 = points [i + 2];
                                PointF p4 = points [i + 3];

                                GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject,
                                                        p1.X, p1.Y, p2.X, p2.Y, 
                                                        p3.X, p3.Y, p4.X, p4.Y);
                        }
		}

		
		public void DrawClosedCurve (Pen pen, PointF [] points)
		{
	 		GDIPlus.GdipDrawClosedCurve (nativeObject, pen.nativeObject, points, points.Length);
		}
		
		public void DrawClosedCurve (Pen pen, Point [] points)
		{
			GDIPlus.GdipDrawClosedCurveI (nativeObject, pen.nativeObject, points, points.Length);	 		
		}
 			
		public void DrawClosedCurve (Pen pen, Point [] points, float tension, FillMode fillmode)
		{
			GDIPlus.GdipDrawClosedCurve2I (nativeObject, pen.nativeObject, points, points.Length, tension); 
		}
		
		public void DrawClosedCurve (Pen pen, PointF [] points, float tension, FillMode fillmode)
		{
			GDIPlus.GdipDrawClosedCurve2 (nativeObject, pen.nativeObject, points, points.Length, tension);
		}
		
		public void DrawCurve (Pen pen, Point [] points)
		{
			GDIPlus.GdipDrawCurveI (nativeObject, pen.nativeObject, points, points.Length);
		}
		
		public void DrawCurve (Pen pen, PointF [] points)
		{
			GDIPlus.GdipDrawCurve (nativeObject, pen.nativeObject, points, points.Length);
		}
		
		public void DrawCurve (Pen pen, PointF [] points, float tension)
		{			
			GDIPlus.GdipDrawCurve2 (nativeObject, pen.nativeObject, points, points.Length, tension);
		}
		
		public void DrawCurve (Pen pen, Point [] points, float tension)
		{
			GDIPlus.GdipDrawCurve2I (nativeObject, pen.nativeObject, points, points.Length, tension);		
			
		}
		
		
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments)
		{
			GDIPlus.GdipDrawCurve3 (nativeObject, pen.nativeObject, points, points.Length,
                        	offset, numberOfSegments, 0.5f);
		}

		public void DrawCurve (Pen pen, Point [] points, int offset, int numberOfSegments, float tension)
		{
			GDIPlus.GdipDrawCurve3I (nativeObject, pen.nativeObject, points, points.Length,
                                        offset, numberOfSegments, tension);
		}

		
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments, float tension)
		{
			GDIPlus.GdipDrawCurve3 (nativeObject, pen.nativeObject, points, points.Length, 
                                        offset, numberOfSegments, tension);
		}

		public void DrawEllipse (Pen pen, Rectangle rect)
		{
			DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void DrawEllipse (Pen pen, RectangleF rect)
		{
			DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void DrawEllipse (Pen pen, int x, int y, int width, int height)
		{
			GDIPlus.GdipDrawEllipseI (nativeObject, pen.nativeObject, x, y, width, height);
		}

		public void DrawEllipse (Pen pen, float x, float y, float width, float height)
		{
			GDIPlus.GdipDrawEllipse (nativeObject, pen.nativeObject, x, y, width, height); 
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
		
		public void DrawImage (Image image, RectangleF rect)
		{
			GDIPlus.GdipDrawImageRect(nativeObject, image.NativeObject, rect.X, rect.Y,
                           rect.Width, rect.Height);
		}

		
		public void DrawImage (Image image, PointF point)
		{
			GDIPlus.GdipDrawImage (nativeObject, image.NativeObject, point.X, point.Y); 			
		}

		
		public void DrawImage (Image image, Point [] destPoints)
		{
			GDIPlus.GdipDrawImagePointsI (nativeObject, image.NativeObject, destPoints, destPoints.Length);
		}

		
		public void DrawImage (Image image, Point point)
		{
			DrawImage (image, point.X, point.Y);
		}

		
		public void DrawImage (Image image, Rectangle rect)
		{
			DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
		}

		
		public void DrawImage (Image image, PointF [] destPoints)
		{
			GDIPlus.GdipDrawImagePoints (nativeObject, image.NativeObject, destPoints, destPoints.Length);
		}

		
		public void DrawImage (Image image, int x, int y)
		{
			GDIPlus.GdipDrawImageI (nativeObject, image.NativeObject, x, y);
		}

		
		public void DrawImage (Image image, float x, float y)
		{
			GDIPlus.GdipDrawImage (nativeObject, image.NativeObject, x, y);
		}

		
		public void DrawImage (Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject,                                                            
				destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, IntPtr.Zero, null, IntPtr.Zero);
		}
		
		public void DrawImage (Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
		{			
			GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject,                                                            
				destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, IntPtr.Zero, null, IntPtr.Zero);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, IntPtr.Zero, null, IntPtr.Zero);
                
		}

		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit)
		{
			
			GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, IntPtr.Zero, null, IntPtr.Zero);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, 
                                ImageAttributes imageAttr)
		{
			GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, imageAttr.NativeObject , null, IntPtr.Zero);
		}
		
		public void DrawImage (Image image, float x, float y, float width, float height)
		{
			GDIPlus.GdipDrawImageRect(nativeObject, image.NativeObject, x, y,
                           width, height);
		}

		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, 
                                ImageAttributes imageAttr)
		{
			GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, imageAttr.NativeObject, null, IntPtr.Zero);
		}

		
		public void DrawImage (Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit)
		{			
			GDIPlus.GdipDrawImagePointRectI(nativeObject, image.NativeObject, x, y,
                                srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit);
		}
		
		public void DrawImage (Image image, int x, int y, int width, int height)
		{
			GDIPlus.GdipDrawImageRectI (nativeObject, image.nativeObject, x, y, width, height);
		}

		public void DrawImage (Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit)
		{			
			GDIPlus.GdipDrawImagePointRect(nativeObject, 	image.nativeObject, x, y, srcRect.X, srcRect.Y, 
				srcRect.Width, srcRect.Height, srcUnit);                                                            			
		}

		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, 
                                ImageAttributes imageAttr, DrawImageAbort callback)
		{
			GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, imageAttr.NativeObject, callback, IntPtr.Zero);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, 
                                ImageAttributes imageAttr, DrawImageAbort callback)
		{
			
			GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, imageAttr.NativeObject, callback, IntPtr.Zero);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, 
                                ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, imageAttr.NativeObject, callback, (IntPtr) callbackData);
		}

		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, 
                                GraphicsUnit srcUnit)
		{
			GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject,
                                destRect.X, destRect.Y, destRect.Width, destRect.Height,
                       		srcX, srcY, srcWidth, srcHeight, srcUnit, IntPtr.Zero, 
                       		null, IntPtr.Zero);                      					
		}
		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, 
                                ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, imageAttr.NativeObject, callback, (IntPtr) callbackData);
		}

		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, 
                                GraphicsUnit srcUnit)
		{
			GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject,
                                destRect.X, destRect.Y, destRect.Width, destRect.Height,
                       		srcX, srcY, srcWidth, srcHeight, srcUnit, IntPtr.Zero, 
                       		null, IntPtr.Zero);                      					
		}

		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, 
                                GraphicsUnit srcUnit, ImageAttributes imageAttrs)
		{
			GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject,
                                destRect.X, destRect.Y, destRect.Width, destRect.Height,
                       		srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttrs.NativeObject, 
                       		null, IntPtr.Zero);                      		
			
		}
		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, 
                                GraphicsUnit srcUnit, ImageAttributes imageAttr)
		{			
			GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject, 
                                        destRect.X, destRect.Y, destRect.Width, destRect.Height,
                                        srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr.NativeObject, 
                                        null, IntPtr.Zero);
 
		}
		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight,
                                GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject, 
                                        destRect.X, destRect.Y, destRect.Width, destRect.Height,
                                        srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr.NativeObject, 
                                        callback, IntPtr.Zero);
		}
		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight,
                                GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject, 
                                        destRect.X, destRect.Y, destRect.Width, destRect.Height,
                                        srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr.NativeObject, 
                                        callback, IntPtr.Zero);
		}

		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight,
                                GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, IntPtr callbackData)
		{
			GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject, 
				destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr.NativeObject, 
				callback, callbackData);
		}

		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, 
                                GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, IntPtr callbackData)
		{
			GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject, 
                       		destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr.NativeObject, 
				callback, callbackData);
		}		
		
		public void DrawImageUnscaled (Image image, Point point)
		{
			DrawImage(image, point.X, point.Y);
		}
		
		public void DrawImageUnscaled (Image image, Rectangle rect)
		{
			DrawImage(image, rect.X, rect.Y, rect.Width, rect.Height);
		}
		
		public void DrawImageUnscaled (Image image, int x, int y)
		{
			DrawImage(image, x, y);
		}

		public void DrawImageUnscaled (Image image, int x, int y, int width, int height)
		{
			DrawImage(image, x, y, width, height);
		}

		public void DrawLine (Pen pen, PointF pt1, PointF pt2)
		{
                        GDIPlus.GdipDrawLine (
                                nativeObject, pen.nativeObject,
                                pt1.X, pt1.Y,
                                pt2.X, pt2.Y);                             
				
		}

		public void DrawLine (Pen pen, Point pt1, Point pt2)
		{
                        GDIPlus.GdipDrawLineI (
                                nativeObject, pen.nativeObject,
                                pt1.X, pt1.Y,
                                pt2.X, pt2.Y);                           
		}

		public void DrawLine (Pen pen, int x1, int y1, int x2, int y2)
		{
			GDIPlus.GdipDrawLineI (nativeObject, pen.nativeObject, x1, y1, x2, y2);			
		}

		public void DrawLine (Pen pen, float x1, float y1, float x2, float y2)
		{
			GDIPlus.GdipDrawLine (nativeObject, pen.nativeObject, x1, y1, x2, y2);	
		}

		public void DrawLines (Pen pen, PointF [] points)
		{
			GDIPlus.GdipDrawLines (nativeObject, pen.nativeObject, points, points.Length);	
		}

		public void DrawLines (Pen pen, Point [] points)
		{
			GDIPlus.GdipDrawLinesI (nativeObject, pen.nativeObject, points, points.Length);			
		}

		public void DrawPath (Pen pen, GraphicsPath path)
		{
			GDIPlus.GdipDrawPath (nativeObject, pen.nativeObject, path.nativePath);
		}
		
		public void DrawPie (Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		{
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}
		
		public void DrawPie (Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		{
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}
		
		public void DrawPie (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			GDIPlus.GdipDrawPie (nativeObject, pen.nativeObject, x, y, width, height, startAngle, sweepAngle);
		}
		
		// Microsoft documentation states that the signature for this member should be
		// public void DrawPie(Pen pen, int x,  int y,  int width,   int height,   int startAngle
   		// int sweepAngle. However, GdipDrawPieI uses also float for the startAngle and sweepAngle params
   		public void DrawPie (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			GDIPlus.GdipDrawPieI (nativeObject, pen.nativeObject, x, y, width, height, startAngle, sweepAngle);
		}

		public void DrawPolygon (Pen pen, Point [] points)
		{
			GDIPlus.GdipDrawPolygonI (nativeObject, pen.nativeObject, points, points.Length);
		}

		public void DrawPolygon (Pen pen, PointF [] points)
		{
			GDIPlus.GdipDrawPolygon (nativeObject, pen.nativeObject, points, points.Length);
		}

		public void DrawRectangle (Pen pen, RectangleF rect)
		{
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, Rectangle rect)
		{
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, float x, float y, float width, float height)
		{
			GDIPlus.GdipDrawRectangle (nativeObject, pen.nativeObject, x, y, width, height);
		}

		public void DrawRectangle (Pen pen, int x, int y, int width, int height)
		{
			GDIPlus.GdipDrawRectangleI (nativeObject, pen.nativeObject, x, y, width, height);
		}

		public void DrawRectangles (Pen pen, RectangleF [] rects)
		{
			foreach (RectangleF rc in rects)
				DrawRectangle (pen, rc.Left, rc.Top, rc.Width, rc.Height);
		}

		public void DrawRectangles (Pen pen, Rectangle [] rects)
		{
			foreach (RectangleF rc in rects)
				DrawRectangle(pen, rc.Left, rc.Top, rc.Width, rc.Height);
		}

		
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle)
		{			
			GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject, ref layoutRectangle, IntPtr.Zero, brush.nativeObject);			 
		}
		
		public void DrawString (string s, Font font, Brush brush, PointF point)
		{
			RectangleF rc = new RectangleF (point.X, point.Y, 0, 0);
			GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject, ref rc, IntPtr.Zero, brush.nativeObject);
		}
		
		public void DrawString (string s, Font font, Brush brush, PointF point, StringFormat format)
		{
			RectangleF rc = new RectangleF (point.X, point.Y, 0, 0);
			GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject, ref rc, format.NativeObject, brush.nativeObject);
		}
		
		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
		{
			GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject, ref layoutRectangle, 
				format.NativeObject, brush.nativeObject);
		}

		public void DrawString (string s, Font font, Brush brush, float x, float y)
		{
			RectangleF rc = new RectangleF (x, y, 0, 0);
			
			GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject, 
				ref rc, IntPtr.Zero, brush.nativeObject);			
		}

		public void DrawString (string s, Font font, Brush brush, float x, float y, StringFormat format)
		{
			RectangleF rc = new RectangleF (x, y, 0, 0);

			GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject,
				ref rc, format.NativeObject, brush.nativeObject);
		}

		
		public void EndContainer (GraphicsContainer container)
		{
			GDIPlus.GdipEndContainer(nativeObject, container.NativeObject);
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
	
		public void ExcludeClip (Rectangle rect)
		{
			GDIPlus.GdipSetClipRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Exclude);    
		}

		[MonoTODO]
		public void ExcludeClip (Region region)
		{
			throw new NotImplementedException ();
		}

		
		public void FillClosedCurve (Brush brush, PointF [] points)
		{
		       GDIPlus.GdipFillClosedCurve (nativeObject, brush.NativeObject, points, points.Length);
		}

		
		public void FillClosedCurve (Brush brush, Point [] points)
		{
			GDIPlus.GdipFillClosedCurveI (nativeObject, brush.NativeObject, points, points.Length);
		}

		
		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode)
		{
			FillClosedCurve (brush, points, fillmode, 0.5f);
		}
		
		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode)
		{
			FillClosedCurve (brush, points, fillmode, 0.5f);
		}

		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode, float tension)
		{
			GDIPlus.GdipFillClosedCurve2 (nativeObject, brush.NativeObject, points, points.Length, tension, fillmode);
		}

		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode, float tension)
		{
			GDIPlus.GdipFillClosedCurve2I (nativeObject, brush.NativeObject, points, points.Length, tension, fillmode);
		}

		public void FillEllipse (Brush brush, Rectangle rect)
		{
			FillEllipse (brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void FillEllipse (Brush brush, RectangleF rect)
		{
			FillEllipse (brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void FillEllipse (Brush brush, float x, float y, float width, float height)
		{
                        GDIPlus.GdipFillEllipse (nativeObject, brush.nativeObject, x, y, width, height);
		}

		public void FillEllipse (Brush brush, int x, int y, int width, int height)
		{
			GDIPlus.GdipFillEllipseI (nativeObject, brush.nativeObject, x, y, width, height);
		}

		public void FillPath (Brush brush, GraphicsPath path)
		{
			GDIPlus.GdipFillPath (nativeObject, brush.NativeObject,  path.NativeObject);
		}

		public void FillPie (Brush brush, Rectangle rect, float startAngle, float sweepAngle)
		{
			GDIPlus.GdipFillPie (nativeObject, brush.NativeObject, rect.X, rect.Y, rect.Width, rect.Height,
                                        startAngle, sweepAngle);
		}

		public void FillPie (Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			GDIPlus.GdipFillPieI (nativeObject, brush.NativeObject, x, y, width, height, startAngle, sweepAngle);
		}

		public void FillPie (Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			GDIPlus.GdipFillPie (nativeObject, brush.NativeObject, x, y, width, height, startAngle, sweepAngle);
		}

		public void FillPolygon (Brush brush, PointF [] points)
		{
			GDIPlus.GdipFillPolygon2 (nativeObject, brush.nativeObject, points, points.Length);
		}

		public void FillPolygon (Brush brush, Point [] points)
		{
			GDIPlus.GdipFillPolygon2I (nativeObject, brush.nativeObject, points, points.Length);
		}

		public void FillPolygon (Brush brush, Point [] points, FillMode fillMode)
		{
			GDIPlus.GdipFillPolygonI (nativeObject, brush.nativeObject, points, points.Length, fillMode);
		}

		public void FillPolygon (Brush brush, PointF [] points, FillMode fillMode)
		{
			GDIPlus.GdipFillPolygon (nativeObject, brush.nativeObject, points, points.Length, fillMode);
		}

		public void FillRectangle (Brush brush, RectangleF rect)
		{
                        FillRectangle (brush, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void FillRectangle (Brush brush, Rectangle rect)
		{
                        FillRectangle (brush, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void FillRectangle (Brush brush, int x, int y, int width, int height)
		{
			GDIPlus.GdipFillRectangle (nativeObject, brush.nativeObject, (float)x, (float)y, (float)width, (float)height);
		}

		public void FillRectangle (Brush brush, float x, float y, float width, float height)
		{
			GDIPlus.GdipFillRectangle (nativeObject, brush.nativeObject, x, y, width, height);
		}

		public void FillRectangles (Brush brush, Rectangle [] rects)
		{
                        foreach (Rectangle rc in rects)
                                FillRectangle(brush, rc);
		}

		public void FillRectangles (Brush brush, RectangleF [] rects)
		{
			foreach (RectangleF rc in rects)
                                FillRectangle(brush, rc);
		}

		
		public void FillRegion (Brush brush, Region region)
		{
			Status status = GDIPlus.GdipFillRegion (nativeObject, brush.NativeObject, region.NativeObject);                  
                        GDIPlus.CheckStatus (status);                                            
		}

		
		public void Flush ()
		{
			Flush (FlushIntention.Flush);
		}

		
		public void Flush (FlushIntention intention)
		{
			Status status = GDIPlus.GdipFlush (nativeObject, intention);
                        GDIPlus.CheckStatus (status);                                                                
		}
		
		public static Graphics FromHdc (IntPtr hdc)
		{
			int graphics;
			if (GDIPlus.GdipCreateFromHDC (hdc, out graphics) != Status.Ok){
				Console.WriteLine ("Graphics.FromHdc: the HDC is an invalid handle");
				return null;
			}
			    
			Graphics result = new Graphics ((IntPtr) graphics);
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
		
		public static Graphics FromHwnd (IntPtr hwnd)
		{
			IntPtr graphics;
			
			GDIPlus.GdipCreateFromHWND (hwnd, out graphics); 			
 			
			return new Graphics (graphics); 
		}

		[MonoTODO]
		public static Graphics FromHwndInternal (IntPtr hwnd)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static Graphics FromImage (Image image)
		{
			if (image == null) throw new ArgumentException ();
			int graphics;
			GDIPlus.GdipGetImageGraphicsContext (image.nativeObject, out graphics);
			Graphics result = new Graphics ((IntPtr) graphics);
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
			int hdc;
			GDIPlus.GdipGetDC (nativeObject, out hdc);
			return (IntPtr) hdc;
		}

		
		public Color GetNearestColor (Color color)
		{
			int argb;
			
			GDIPlus.GdipGetNearestColor (nativeObject, out argb);			
			return Color.FromArgb (argb);
		}

		[MonoTODO]
		public void IntersectClip (Region region)
		{
			throw new NotImplementedException ();
		}
		
		public void IntersectClip (RectangleF rect)
		{
			GDIPlus.GdipSetClipRect (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Intersect);
		}

		public void IntersectClip (Rectangle rect)
		{			
			GDIPlus.GdipSetClipRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Intersect);
		}

		public bool IsVisible (Point point)
		{
			bool isVisible = false;

			GDIPlus.GdipIsVisiblePointI (nativeObject, point.X, point.Y, out isVisible);

                        return isVisible;
		}

		
		public bool IsVisible (RectangleF rect)
		{
			bool isVisible = false;

			GDIPlus.GdipIsVisibleRect (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, out isVisible);

                        return isVisible;
		}

		public bool IsVisible (PointF point)
		{
			bool isVisible = false;

			GDIPlus.GdipIsVisiblePoint (nativeObject, point.X, point.Y, out isVisible);

                        return isVisible;
		}
		
		public bool IsVisible (Rectangle rect)
		{
			bool isVisible = false;

			GDIPlus.GdipIsVisibleRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, out isVisible);

                        return isVisible;
		}
		
		public bool IsVisible (float x, float y)
		{
			return IsVisible (new PointF (x, y));
		}
		
		public bool IsVisible (int x, int y)
		{
			return IsVisible (new Point (x, y));
		}
		
		public bool IsVisible (float x, float y, float width, float height)
		{
			return IsVisible (new RectangleF (x, y, width, height));
		}

		
		public bool IsVisible (int x, int y, int width, int height)
		{
			return IsVisible (new Rectangle (x, y, width, height));
		}

		[MonoTODO]
		public Region [] MeasureCharacterRanges (string text, Font font, RectangleF layoutRect, StringFormat stringFormat)
		{
			throw new NotImplementedException ();
		}

		
		public SizeF MeasureString (string text, Font font)
		{
			return MeasureString (text, font, new Size(0,0));
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea)
		{
			int charactersFitted, linesFilled;			
			RectangleF boundingBox = new RectangleF ();
			RectangleF rect = new RectangleF (0,0,layoutArea.Width, layoutArea.Height);
					
			GDIPlus.GdipMeasureString (nativeObject, text, text.Length, font.NativeObject,
    		 ref rect, IntPtr.Zero, out boundingBox, out charactersFitted, out linesFilled);
    				
    		return new SizeF(boundingBox.Width, boundingBox.Height);
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, int width)
		{
			throw new NotImplementedException ();
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat)
		{
			int charactersFitted, linesFilled;			
			return MeasureString (text, font, layoutArea, stringFormat, out charactersFitted, out linesFilled);
		}

		[MonoTODO]
		public SizeF MeasureString (string text, Font font, int width, StringFormat format)
		{
			throw new NotImplementedException ();
		}

		
		public SizeF MeasureString (string text, Font font, PointF origin, StringFormat stringFormat)
		{
			RectangleF boundingBox = new RectangleF ();
			RectangleF rect = new RectangleF (origin.X, origin.Y, 0,0);
			int charactersFitted, linesFilled;
					
			GDIPlus.GdipMeasureString (nativeObject, text, text.Length, font.NativeObject,
    		 ref rect, stringFormat.NativeObject, out boundingBox, out charactersFitted, out linesFilled);
    				
    		return new SizeF(boundingBox.Width, boundingBox.Height);
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea, StringFormat stringFormat, 
                                out int charactersFitted, out int linesFilled)
		{	
			RectangleF boundingBox = new RectangleF ();
			RectangleF rect = new RectangleF (0,0,layoutArea.Width, layoutArea.Height);
					
			GDIPlus.GdipMeasureString (nativeObject, text, text.Length, font.NativeObject,
    		 ref rect, stringFormat.NativeObject, out boundingBox, out charactersFitted,
    				out linesFilled);
    				
    		return new SizeF(boundingBox.Width, boundingBox.Height);
		}

		public void MultiplyTransform (Matrix matrix)
		{
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			GDIPlus.GdipMultiplyWorldTransform (nativeObject, matrix.nativeMatrix, order);
		}
		
		public void ReleaseHdc (IntPtr hdc)
		{
			GDIPlus.GdipReleaseDC (nativeObject, hdc);
		}

		[MonoTODO]
		public void ReleaseHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		
		public void ResetClip ()
		{
			GDIPlus.GdipResetClip(nativeObject);
		}

		public void ResetTransform ()
		{
			GDIPlus.GdipResetWorldTransform (nativeObject);
		}

		[MonoTODO]
		public void Restore (GraphicsState gstate)
		{
			Transform = gstate.matrix.Clone();
			GDIPlus.GdipRestoreGraphics (nativeObject, gstate.nativeState);
		}

		[MonoTODO]
		public void RotateTransform (float angle)
		{
			RotateTransform(angle, MatrixOrder.Prepend);
		}

		[MonoTODO]
		public void RotateTransform (float angle, MatrixOrder order)
		{
			//transform.Rotate(angle, order);
			GDIPlus.GdipRotateWorldTransform (nativeObject, angle, order);
		}

		[MonoTODO]
		public GraphicsState Save ()
		{
			//return implementation.Save();
			GraphicsState state = new GraphicsState();
			state.matrix = Transform.Clone ();
			uint saveState;
			GDIPlus.GdipSaveGraphics (nativeObject, out saveState);
			state.nativeState = saveState;
			return state;
		}

		public void ScaleTransform (float sx, float sy)
		{
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
                        GDIPlus.GdipScaleWorldTransform (nativeObject, sx, sy, order);
		}

		
		public void SetClip (RectangleF rect)
		{
                        SetClip (rect, CombineMode.Replace);
		}

		
		public void SetClip (GraphicsPath path)
		{
			SetClip (path, CombineMode.Replace);
		}

		
		public void SetClip (Rectangle rect)
		{
			SetClip (rect, CombineMode.Replace);
		}

		
		public void SetClip (Graphics g)
		{
			SetClip (g, CombineMode.Replace);
		}

		
		public void SetClip (Graphics g, CombineMode combineMode)
		{
			GDIPlus.GdipSetClipGraphics (nativeObject, g.NativeObject, combineMode);
		}

		
		public void SetClip (Rectangle rect, CombineMode combineMode)
		{
			GDIPlus.GdipSetClipRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, combineMode);
		}

		
		public void SetClip (RectangleF rect, CombineMode combineMode)
		{
			GDIPlus.GdipSetClipRect (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, combineMode);		
		}

		[MonoTODO]
		public void SetClip (Region region, CombineMode combineMode)
		{
			//GDIPlus.GdipSetClipRegion(nativeObject,  region.NativeObject, combineMode); //TODO: Region not implemented yet
		}

		
		public void SetClip (GraphicsPath path, CombineMode combineMode)
		{
			GDIPlus.GdipSetClipPath (nativeObject, path.NativeObject, combineMode);
		}

		
		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF [] pts)
		{
			IntPtr ptrPt =  GDIPlus.FromPointToUnManagedMemory (pts);
            
                        Status status = GDIPlus.GdipTransformPoints (nativeObject, destSpace, srcSpace,  ptrPt, pts.Length);
			
			GDIPlus.FromUnManagedMemoryToPoint (ptrPt, pts);
		}

				
		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, Point [] pts)
		{						
                        IntPtr ptrPt =  GDIPlus.FromPointToUnManagedMemoryI (pts);
            
                        Status status = GDIPlus.GdipTransformPointsI (nativeObject, destSpace, srcSpace, ptrPt, pts.Length);
			
			GDIPlus.FromUnManagedMemoryToPointI (ptrPt, pts);
		}

		
		public void TranslateClip (int dx, int dy)
		{
			GDIPlus.GdipTranslateClipI (nativeObject, dx, dy);			
		}

		
		public void TranslateClip (float dx, float dy)
		{
			GDIPlus.GdipTranslateClip (nativeObject, dx, dy);
		}

		public void TranslateTransform (float dx, float dy)
		{
			TranslateTransform (dx, dy, MatrixOrder.Prepend);
		}

		
		public void TranslateTransform (float dx, float dy, MatrixOrder order)
		{			
			GDIPlus.GdipTranslateWorldTransform (nativeObject, dx, dy, order);
		}

		public Region Clip {
			get {
				throw new NotImplementedException ();
			}
			set {
				//throw new NotImplementedException ();
			}
		}

		public RectangleF ClipBounds {
			get {
                                RectangleF rect = new RectangleF ();					
                                GDIPlus.GdipGetClipBounds (nativeObject, out rect);
                                return rect;
			}
		}

		public CompositingMode CompositingMode {
			get {
                                CompositingMode mode;
                                GDIPlus.GdipGetCompositingMode (nativeObject, out mode);    
                                return mode;
			}
			set {
				
                                GDIPlus.GdipSetCompositingMode (nativeObject, value);    
			}

		}

		public CompositingQuality CompositingQuality {
			get {
                                CompositingQuality quality;

                                GDIPlus.GdipGetCompositingQuality (nativeObject, out quality);
        			return quality;
			}
			set {
                                GDIPlus.GdipSetCompositingQuality (nativeObject, value); 
			}
		}

		public float DpiX {
			get {
                                float x;

       				GDIPlus.GdipGetDpiX (nativeObject, out x);
        			return x;
			}
		}

		public float DpiY {
			get {
                                float y;

       				GDIPlus.GdipGetDpiY (nativeObject, out y);
        			return y;
			}
		}

		public InterpolationMode InterpolationMode {
			get {				
                                InterpolationMode imode = InterpolationMode.Invalid;
        			GDIPlus.GdipGetInterpolationMode (nativeObject, out imode);
        			return imode;
			}
			set {
                                GDIPlus.GdipSetInterpolationMode (nativeObject, value);
			}
		}

		public bool IsClipEmpty {
			get {
                                bool isEmpty = false;

        			GDIPlus.GdipIsClipEmpty (nativeObject, out isEmpty);
        			return isEmpty;
			}
		}

		public bool IsVisibleClipEmpty {
			get {
                                bool isEmpty = false;

        			GDIPlus.GdipIsVisibleClipEmpty (nativeObject, out isEmpty);
        			return isEmpty;
			}
		}

		public float PageScale {
			get {
                                float scale;

        			GDIPlus.GdipGetPageScale (nativeObject, out scale);
        			return scale;
			}
			set {
                                GDIPlus.GdipSetPageScale (nativeObject, value);
			}
		}

		public GraphicsUnit PageUnit {
			get {
                                GraphicsUnit unit;
                                
                                GDIPlus.GdipGetPageUnit (nativeObject, out unit);
        			return unit;
			}
			set {
                                GDIPlus.GdipSetPageUnit (nativeObject, value);
			}
		}

		public PixelOffsetMode PixelOffsetMode {
			get {
			        PixelOffsetMode pixelOffset = PixelOffsetMode.Invalid;
                                
                                GDIPlus.GdipGetPixelOffsetMode (nativeObject, out pixelOffset);

        			return pixelOffset;
			}
			set {
                                GDIPlus.GdipSetPixelOffsetMode (nativeObject, value); 
			}
		}

		public Point RenderingOrigin {
			get {
                                int x, y;
				GDIPlus.GdipGetRenderingOrigin (
                                        nativeObject, out x, out y);

                                return new Point (x, y);
			}

			set {
                                GDIPlus.GdipSetRenderingOrigin (nativeObject, value.X, value.Y);
			}
		}

		public SmoothingMode SmoothingMode {
			get {
                                SmoothingMode mode = SmoothingMode.Invalid;

        			GDIPlus.GdipGetSmoothingMode (nativeObject, out mode);
                                
                                return mode;
			}

			set {
                                GDIPlus.GdipSetSmoothingMode (nativeObject, value);
			}
		}

		public int TextContrast {
			get {	
                                int contrast;
					
                                GDIPlus.GdipGetTextContrast (nativeObject, out contrast);
                                return contrast;
			}

                        set {
                                GDIPlus.GdipSetTextContrast (nativeObject, value);
			}
		}

		public TextRenderingHint TextRenderingHint {
			get {
                                TextRenderingHint hint;

                                GDIPlus.GdipGetTextRenderingHint (nativeObject, out hint);
                                return hint;        
			}

			set {
                                GDIPlus.GdipSetTextRenderingHint (nativeObject, value);
			}
		}

		public Matrix Transform {
			get {
                                Matrix matrix = new Matrix ();
                                GDIPlus.GdipGetWorldTransform (nativeObject, matrix.nativeMatrix);

                                return matrix;
			}
			set {
                                GDIPlus.GdipSetWorldTransform (nativeObject, value.nativeMatrix);
			}
		}

		public RectangleF VisibleClipBounds {
			get {
                                RectangleF rect;
					
                                GDIPlus.GdipGetVisibleClipBounds (nativeObject, out rect);
                                return rect;
			}
		}
	}
}

