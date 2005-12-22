//
// System.Drawing.Graphics.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com) (stubbed out)
//      Alexandre Pigolkine(pigolkine@gmx.de)
//	Jordi Mas i Hernandez (jordi@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc. (http://www.ximian.com)
//
// Copyright (C) 2004-2005 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;

namespace System.Drawing
{
#if !NET_2_0
	[ComVisible(false)]  
#endif
	public sealed class Graphics : MarshalByRefObject, IDisposable
#if NET_2_0
	, IDeviceContext
#endif
	{
		internal IntPtr nativeObject = IntPtr.Zero;
		private bool disposed = false;
		private static float defDpiX = 0;
		private static float defDpiY = 0;

		[ComVisible(false)]
		public delegate bool EnumerateMetafileProc (EmfPlusRecordType recordType,
							    int flags,
							    int dataSize,
							    IntPtr data,
							    PlayRecordCallback callbackData);
		
		[ComVisible (false)]
		public delegate bool DrawImageAbort (IntPtr callbackData);

		internal Graphics (IntPtr nativeGraphics)
		{
			nativeObject = nativeGraphics;
		}

		~Graphics ()
		{
			Dispose ();			
		}		

		static internal float systemDpiX {
			get {
				if (defDpiX == 0) {
					Bitmap bmp = new Bitmap (1, 1);
					Graphics g = Graphics.FromImage (bmp);
					defDpiX = g.DpiX;
				}
				return defDpiX;
			}
		}

		static internal float systemDpiY {
			get {
				if (defDpiY == 0) {
					Bitmap bmp = new Bitmap (1, 1);
					Graphics g = Graphics.FromImage (bmp);
					defDpiY = g.DpiY;
				}
				return defDpiY;
			}
		}

		internal IntPtr NativeObject {
			get {
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
			Status status;
			status = GDIPlus.GdipBeginContainer2 (nativeObject, out state);
        		GDIPlus.CheckStatus (status);

                        return new GraphicsContainer(state);
		}
		
		public GraphicsContainer BeginContainer (Rectangle dstrect, Rectangle srcrect, GraphicsUnit unit)
		{
			int state;
			Status status;
			status = GDIPlus.GdipBeginContainerI (nativeObject, dstrect, srcrect, unit, out state);
			GDIPlus.CheckStatus (status);

			return new GraphicsContainer (state);
		}

		
		public GraphicsContainer BeginContainer (RectangleF dstrect, RectangleF srcrect, GraphicsUnit unit)
		{
			int state;
			Status status;
			status = GDIPlus.GdipBeginContainer (nativeObject, dstrect, srcrect, unit, out state);
			GDIPlus.CheckStatus (status);

			return new GraphicsContainer (state);
		}

		
		public void Clear (Color color)
		{
			Status status;
 			status = GDIPlus.GdipGraphicsClear (nativeObject, color.ToArgb ());
 			GDIPlus.CheckStatus (status);
		}
#if NET_2_0		
		public void CopyFromScreen (Point upperLeftSource, Point upperLeftDestination, Size blockRegionSize)
		{
			CopyFromScreen (upperLeftSource.X, upperLeftSource.Y, upperLeftDestination.X, upperLeftDestination.Y,
				blockRegionSize, CopyPixelOperation.SourceCopy);				
		}

		public void CopyFromScreen (Point upperLeftSource, Point upperLeftDestination, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
		{
			CopyFromScreen (upperLeftSource.X, upperLeftSource.Y, upperLeftDestination.X, upperLeftDestination.Y,
				blockRegionSize, copyPixelOperation);
		}
		
		public void CopyFromScreen (int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize)
		{
			CopyFromScreen (sourceX, sourceY, destinationX, destinationY, blockRegionSize,
				CopyPixelOperation.SourceCopy);
		}

		public void CopyFromScreen (int sourceX, int sourceY, int destinationX, int destinationY, Size blockRegionSize, CopyPixelOperation copyPixelOperation)
		{
			IntPtr window;			

			if (!Enum.IsDefined (typeof (CopyPixelOperation), copyPixelOperation))
				throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for CopyPixelOperation", copyPixelOperation));

			if (GDIPlus.UseCocoaDrawable || GDIPlus.UseQuartzDrawable) {
				throw new NotImplementedException ();
			}
			
			if (GDIPlus.UseX11Drawable) { // X11 implementation
				IntPtr image, defvisual, vPtr;
				int AllPlanes = ~0, nitems = 0, pixel;

				if (copyPixelOperation != CopyPixelOperation.SourceCopy)
					throw new NotImplementedException ("Operation not implemented under X11");
		
				if (GDIPlus.Display == IntPtr.Zero) {
					GDIPlus.Display = GDIPlus.XOpenDisplay (IntPtr.Zero);					
				}

				window = GDIPlus.XRootWindow (GDIPlus.Display, 0);
				defvisual = GDIPlus.XDefaultVisual (GDIPlus.Display, 0);				
				XVisualInfo visual = new XVisualInfo ();

				/* Get XVisualInfo for this visual */
				visual.visualid = GDIPlus.XVisualIDFromVisual(defvisual);
				vPtr = GDIPlus.XGetVisualInfo (GDIPlus.Display, 0x1 /* VisualIDMask */, ref visual, ref nitems);
				visual = (XVisualInfo) Marshal.PtrToStructure(vPtr, typeof (XVisualInfo));

				/* Sorry I do not have access to a computer with > deepth. Fell free to add more pixel formats */	
				image = GDIPlus.XGetImage (GDIPlus.Display, window, sourceX, sourceY, blockRegionSize.Width,
					blockRegionSize.Height, AllPlanes, 2 /* ZPixmap*/);
				
				Bitmap bmp = new Bitmap (blockRegionSize.Width, blockRegionSize.Height);
				int red, blue, green;
				for (int y = sourceY; y <  sourceY + blockRegionSize.Height; y++) {
					for (int x = sourceX; x <  sourceX + blockRegionSize.Width; x++) {
						pixel = GDIPlus.XGetPixel (image, x, y);			

						switch (visual.depth) {
							case 16: /* 16bbp pixel transformation */
								red = (int) ((pixel & visual.red_mask ) >> 8) & 0xff;
								green = (int) (((pixel & visual.green_mask ) >> 3 )) & 0xff;
								blue = (int) ((pixel & visual.blue_mask ) << 3 ) & 0xff;
								break;
							case 24:
							case 32:
								red = (int) ((pixel & visual.red_mask ) >> 16) & 0xff;
								green = (int) (((pixel & visual.green_mask ) >> 8 )) & 0xff;
								blue = (int) ((pixel & visual.blue_mask )) & 0xff;
								break;
							default:
								throw new NotImplementedException ("Deepth not supported right now");
						}
						
						bmp.SetPixel (x, y, Color.FromArgb (255, red, green, blue));							 
					}
				}

				DrawImage (bmp, 0, 0);
				bmp.Dispose ();
				GDIPlus.XDestroyImage (image);
				return;
			}			

			// Win32 implementation
			window = GDIPlus.GetDesktopWindow ();
			IntPtr srcDC = GDIPlus.GetDC (window);
			IntPtr dstDC = GetHdc ();
			GDIPlus.BitBlt (dstDC, destinationX, destinationY, blockRegionSize.Width,
				blockRegionSize.Height, srcDC, sourceX, sourceY, (int) copyPixelOperation);

			GDIPlus.ReleaseDC (srcDC);
			ReleaseHdc (dstDC);			
		}
#endif

		public void Dispose ()
		{
			Status status;
			if (! disposed) {
				status = GDIPlus.GdipDeleteGraphics (nativeObject);
				nativeObject = IntPtr.Zero;
				GDIPlus.CheckStatus (status);
				disposed = true;				
			}
			GC.SuppressFinalize(this);
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
			Status status;
			if (pen == null)
				throw new ArgumentNullException ("pen");
			
			status = GDIPlus.GdipDrawArc (nativeObject, pen.nativeObject,
                                        x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		// Microsoft documentation states that the signature for this member should be
		// public void DrawArc( Pen pen,  int x,  int y,  int width,  int height,   int startAngle,
   		// int sweepAngle. However, GdipDrawArcI uses also float for the startAngle and sweepAngle params
   		public void DrawArc (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			Status status;
			if (pen == null)
				throw new ArgumentNullException ("pen");
			status = GDIPlus.GdipDrawArcI (nativeObject, pen.nativeObject,
						x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		public void DrawBezier (Pen pen, PointF pt1, PointF pt2, PointF pt3, PointF pt4)
		{
			Status status;
			if (pen == null)
				throw new ArgumentNullException ("pen");
			status = GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject,
							pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X,
							pt3.Y, pt4.X, pt4.Y);
			GDIPlus.CheckStatus (status);
		}

		public void DrawBezier (Pen pen, Point pt1, Point pt2, Point pt3, Point pt4)
		{
			Status status;
			if (pen == null)
				throw new ArgumentNullException ("pen");
			status = GDIPlus.GdipDrawBezierI (nativeObject, pen.nativeObject,
							pt1.X, pt1.Y, pt2.X, pt2.Y, pt3.X,
							pt3.Y, pt4.X, pt4.Y);
			GDIPlus.CheckStatus (status);
		}

		public void DrawBezier (Pen pen, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
		{
			Status status;
			if (pen == null)
				throw new ArgumentNullException ("pen");
			status = GDIPlus.GdipDrawBezier (nativeObject, pen.nativeObject, x1,
							y1, x2, y2, x3, y3, x4, y4);
			GDIPlus.CheckStatus (status);
		}

		public void DrawBeziers (Pen pen, Point [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
                        int length = points.Length;
			Status status;

                        if (length < 3)
                                return;

			for (int i = 0; i < length; i += 3) {
                                Point p1 = points [i];
                                Point p2 = points [i + 1];
                                Point p3 = points [i + 2];
                                Point p4 = points [i + 3];

                                status = GDIPlus.GdipDrawBezier (nativeObject, 
							pen.nativeObject,
                                                        p1.X, p1.Y, p2.X, p2.Y, 
                                                        p3.X, p3.Y, p4.X, p4.Y);
				GDIPlus.CheckStatus (status);
                        }
		}

		public void DrawBeziers (Pen pen, PointF [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			int length = points.Length;
			Status status;

                        if (length < 3)
                                return;

			for (int i = 0; i < length; i += 3) {
                                PointF p1 = points [i];
                                PointF p2 = points [i + 1];
                                PointF p3 = points [i + 2];
                                PointF p4 = points [i + 3];

                                status = GDIPlus.GdipDrawBezier (nativeObject, 
							pen.nativeObject,
                                                        p1.X, p1.Y, p2.X, p2.Y, 
                                                        p3.X, p3.Y, p4.X, p4.Y);
				GDIPlus.CheckStatus (status);
                        }
		}

		
		public void DrawClosedCurve (Pen pen, PointF [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			Status status;
			status = GDIPlus.GdipDrawClosedCurve (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawClosedCurve (Pen pen, Point [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			Status status;
			status = GDIPlus.GdipDrawClosedCurveI (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}
 			
		public void DrawClosedCurve (Pen pen, Point [] points, float tension, FillMode fillmode)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			Status status;
			status = GDIPlus.GdipDrawClosedCurve2I (nativeObject, pen.nativeObject, points, points.Length, tension);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawClosedCurve (Pen pen, PointF [] points, float tension, FillMode fillmode)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			Status status;
			status = GDIPlus.GdipDrawClosedCurve2 (nativeObject, pen.nativeObject, points, points.Length, tension);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawCurve (Pen pen, Point [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			Status status;
			status = GDIPlus.GdipDrawCurveI (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawCurve (Pen pen, PointF [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			Status status;
			status = GDIPlus.GdipDrawCurve (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawCurve (Pen pen, PointF [] points, float tension)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			Status status;
			status = GDIPlus.GdipDrawCurve2 (nativeObject, pen.nativeObject, points, points.Length, tension);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawCurve (Pen pen, Point [] points, float tension)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			Status status;
			status = GDIPlus.GdipDrawCurve2I (nativeObject, pen.nativeObject, points, points.Length, tension);		
			GDIPlus.CheckStatus (status);
		}
		
		
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			Status status;
			status = GDIPlus.GdipDrawCurve3 (nativeObject, pen.nativeObject,
							points, points.Length, offset,
							numberOfSegments, 0.5f);
			GDIPlus.CheckStatus (status);
		}

		public void DrawCurve (Pen pen, Point [] points, int offset, int numberOfSegments, float tension)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			Status status;
			status = GDIPlus.GdipDrawCurve3I (nativeObject, pen.nativeObject,
							points, points.Length, offset,
							numberOfSegments, tension);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawCurve (Pen pen, PointF [] points, int offset, int numberOfSegments, float tension)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			
			Status status;
			status = GDIPlus.GdipDrawCurve3 (nativeObject, pen.nativeObject,
							points, points.Length, offset,
							numberOfSegments, tension);
			GDIPlus.CheckStatus (status);
		}

		public void DrawEllipse (Pen pen, Rectangle rect)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			
			DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void DrawEllipse (Pen pen, RectangleF rect)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			DrawEllipse (pen, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void DrawEllipse (Pen pen, int x, int y, int width, int height)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			Status status;
			status = GDIPlus.GdipDrawEllipseI (nativeObject, pen.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void DrawEllipse (Pen pen, float x, float y, float width, float height)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			Status status = GDIPlus.GdipDrawEllipse (nativeObject, pen.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void DrawIcon (Icon icon, Rectangle targetRect)
		{
			Image img = icon.ToBitmap ();
			DrawImage (img, targetRect);
		}

		public void DrawIcon (Icon icon, int x, int y)
		{
			Image img = icon.ToBitmap ();
			DrawImage (img, x, y);
		}

		public void DrawIconUnstretched (Icon icon, Rectangle targetRect)
		{
			Image img = icon.ToBitmap ();
			DrawImageUnscaled (img, targetRect);
		}
		
		public void DrawImage (Image image, RectangleF rect)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			
			Status status = GDIPlus.GdipDrawImageRect(nativeObject, image.NativeObject, rect.X, rect.Y, rect.Width, rect.Height);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, PointF point)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			
			Status status = GDIPlus.GdipDrawImage (nativeObject, image.NativeObject, point.X, point.Y);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point [] destPoints)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			if (destPoints == null)
				throw new ArgumentNullException ("destPoints");
			
			Status status = GDIPlus.GdipDrawImagePointsI (nativeObject, image.NativeObject, destPoints, destPoints.Length);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point point)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			DrawImage (image, point.X, point.Y);
		}

		
		public void DrawImage (Image image, Rectangle rect)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			DrawImage (image, rect.X, rect.Y, rect.Width, rect.Height);
		}

		
		public void DrawImage (Image image, PointF [] destPoints)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			if (destPoints == null)
				throw new ArgumentNullException ("destPoints");
			Status status = GDIPlus.GdipDrawImagePoints (nativeObject, image.NativeObject, destPoints, destPoints.Length);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, int x, int y)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageI (nativeObject, image.NativeObject, x, y);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, float x, float y)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImage (nativeObject, image.NativeObject, x, y);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject,
				destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, IntPtr.Zero, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, RectangleF destRect, RectangleF srcRect, GraphicsUnit srcUnit)
		{			
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject,
				destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height,
				srcUnit, IntPtr.Zero, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			if (destPoints == null)
				throw new ArgumentNullException ("destPoints");
			
			Status status = GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, 
				srcRect.Width, srcRect.Height, srcUnit, IntPtr.Zero, 
				null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			if (destPoints == null)
				throw new ArgumentNullException ("destPoints");
			
			Status status = GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, 
				srcRect.Width, srcRect.Height, srcUnit, IntPtr.Zero, 
				null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, 
                                ImageAttributes imageAttr)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			if (destPoints == null)
				throw new ArgumentNullException ("destPoints");
			Status status = GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y,
				srcRect.Width, srcRect.Height, srcUnit,
				imageAttr != null ? imageAttr.NativeObject : IntPtr.Zero, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, float x, float y, float width, float height)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRect(nativeObject, image.NativeObject, x, y,
                           width, height);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, 
                                ImageAttributes imageAttr)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			if (destPoints == null)
				throw new ArgumentNullException ("destPoints");
			Status status = GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y,
				srcRect.Width, srcRect.Height, srcUnit, 
				imageAttr != null ? imageAttr.NativeObject : IntPtr.Zero, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, int x, int y, Rectangle srcRect, GraphicsUnit srcUnit)
		{			
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImagePointRectI(nativeObject, image.NativeObject, x, y, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, int x, int y, int width, int height)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRectI (nativeObject, image.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void DrawImage (Image image, float x, float y, RectangleF srcRect, GraphicsUnit srcUnit)
		{			
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImagePointRect (nativeObject, image.nativeObject, x, y, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, srcUnit);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			if (destPoints == null)
				throw new ArgumentNullException ("destPoints");
			Status status = GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y,
				srcRect.Width, srcRect.Height, srcUnit, 
				imageAttr != null ? imageAttr.NativeObject : IntPtr.Zero, callback, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			if (destPoints == null)
				throw new ArgumentNullException ("destPoints");
			
			Status status = GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y,
				srcRect.Width, srcRect.Height, srcUnit, 
				imageAttr != null ? imageAttr.NativeObject : IntPtr.Zero, callback, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Point [] destPoints, Rectangle srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			if (destPoints == null)
				throw new ArgumentNullException ("destPoints");

			Status status = GDIPlus.GdipDrawImagePointsRectI (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y, 
				srcRect.Width, srcRect.Height, srcUnit, 
				imageAttr != null ? imageAttr.NativeObject : IntPtr.Zero, callback, (IntPtr) callbackData);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject,
                                destRect.X, destRect.Y, destRect.Width, destRect.Height,
                       		srcX, srcY, srcWidth, srcHeight, srcUnit, IntPtr.Zero, 
                       		null, IntPtr.Zero);
			GDIPlus.CheckStatus (status); 					
		}
		
		public void DrawImage (Image image, PointF [] destPoints, RectangleF srcRect, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, int callbackData)
		{
			Status status = GDIPlus.GdipDrawImagePointsRect (nativeObject, image.NativeObject,
				destPoints, destPoints.Length , srcRect.X, srcRect.Y,
				srcRect.Width, srcRect.Height, srcUnit, 
				imageAttr != null ? imageAttr.NativeObject : IntPtr.Zero, callback, (IntPtr) callbackData);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject,
                                destRect.X, destRect.Y, destRect.Width, destRect.Height,
                       		srcX, srcY, srcWidth, srcHeight, srcUnit, IntPtr.Zero, 
                       		null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttrs)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject,
                                destRect.X, destRect.Y, destRect.Width, destRect.Height,
                       		srcX, srcY, srcWidth, srcHeight, srcUnit,
				imageAttrs != null ? imageAttrs.NativeObject : IntPtr.Zero, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr)
		{			
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject, 
                                        destRect.X, destRect.Y, destRect.Width, 
					destRect.Height, srcX, srcY, srcWidth, srcHeight,
					srcUnit, imageAttr != null ? imageAttr.NativeObject : IntPtr.Zero, null, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRectRectI (nativeObject, image.NativeObject, 
                                        destRect.X, destRect.Y, destRect.Width, 
					destRect.Height, srcX, srcY, srcWidth, srcHeight,
					srcUnit, imageAttr != null ? imageAttr.NativeObject : IntPtr.Zero, callback,
					IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject, 
                                        destRect.X, destRect.Y, destRect.Width, 
					destRect.Height, srcX, srcY, srcWidth, srcHeight,
					srcUnit, imageAttr != null ? imageAttr.NativeObject : IntPtr.Zero, 
					callback, IntPtr.Zero);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, float srcX, float srcY, float srcWidth, float srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, IntPtr callbackData)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject, 
				destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcX, srcY, srcWidth, srcHeight, srcUnit, 
				imageAttr != null ? imageAttr.NativeObject : IntPtr.Zero, callback, callbackData);
			GDIPlus.CheckStatus (status);
		}

		
		public void DrawImage (Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr, DrawImageAbort callback, IntPtr callbackData)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Status status = GDIPlus.GdipDrawImageRectRect (nativeObject, image.NativeObject, 
                       		destRect.X, destRect.Y, destRect.Width, destRect.Height,
				srcX, srcY, srcWidth, srcHeight, srcUnit,
				imageAttr != null ? imageAttr.NativeObject : IntPtr.Zero, callback, callbackData);
			GDIPlus.CheckStatus (status);
		}		
		
		public void DrawImageUnscaled (Image image, Point point)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			DrawImageUnscaled (image, point.X, point.Y);
		}
		
		public void DrawImageUnscaled (Image image, Rectangle rect)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			DrawImageUnscaled (image, rect.X, rect.Y, rect.Width, rect.Height);
		}
		
		public void DrawImageUnscaled (Image image, int x, int y)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			DrawImage (image, x, y, image.Width, image.Height);
		}

		public void DrawImageUnscaled (Image image, int x, int y, int width, int height)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			Image tmpImg = new Bitmap (width, height);
			Graphics g = FromImage (tmpImg);
			g.DrawImage (image, 0, 0, image.Width, image.Height);
			this.DrawImage (tmpImg, x, y, width, height);
			tmpImg.Dispose ();
			g.Dispose ();
		}

#if NET_2_0
		public void DrawImageUnscaledAndClipped (Image image, Rectangle rect)
		{
			int height, width;			
			width = (image.Width > rect.Width) ? rect.Width : image.Width;
			height = (image.Height > rect.Height) ? rect.Height : image.Height;

			DrawImageUnscaled (image, rect.X, rect.Y, width, height);			
		}
#endif

		public void DrawLine (Pen pen, PointF pt1, PointF pt2)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
                        Status status = GDIPlus.GdipDrawLine (nativeObject, pen.nativeObject,
		                                pt1.X, pt1.Y, pt2.X, pt2.Y);
			GDIPlus.CheckStatus (status);
		}

		public void DrawLine (Pen pen, Point pt1, Point pt2)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
                        Status status = GDIPlus.GdipDrawLineI (nativeObject, pen.nativeObject,
		                                pt1.X, pt1.Y, pt2.X, pt2.Y);
			GDIPlus.CheckStatus (status);
		}

		public void DrawLine (Pen pen, int x1, int y1, int x2, int y2)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			Status status = GDIPlus.GdipDrawLineI (nativeObject, pen.nativeObject, x1, y1, x2, y2);
			GDIPlus.CheckStatus (status);
		}

		public void DrawLine (Pen pen, float x1, float y1, float x2, float y2)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			Status status = GDIPlus.GdipDrawLine (nativeObject, pen.nativeObject, x1, y1, x2, y2);
			GDIPlus.CheckStatus (status);
		}

		public void DrawLines (Pen pen, PointF [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipDrawLines (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		public void DrawLines (Pen pen, Point [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipDrawLinesI (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		public void DrawPath (Pen pen, GraphicsPath path)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (path == null)
				throw new ArgumentNullException ("path");
			Status status = GDIPlus.GdipDrawPath (nativeObject, pen.nativeObject, path.nativePath);
			GDIPlus.CheckStatus (status);
		}
		
		public void DrawPie (Pen pen, Rectangle rect, float startAngle, float sweepAngle)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}
		
		public void DrawPie (Pen pen, RectangleF rect, float startAngle, float sweepAngle)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			DrawPie (pen, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
		}
		
		public void DrawPie (Pen pen, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			Status status = GDIPlus.GdipDrawPie (nativeObject, pen.nativeObject, x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}
		
		// Microsoft documentation states that the signature for this member should be
		// public void DrawPie(Pen pen, int x,  int y,  int width,   int height,   int startAngle
   		// int sweepAngle. However, GdipDrawPieI uses also float for the startAngle and sweepAngle params
   		public void DrawPie (Pen pen, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			Status status = GDIPlus.GdipDrawPieI (nativeObject, pen.nativeObject, x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		public void DrawPolygon (Pen pen, Point [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipDrawPolygonI (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		public void DrawPolygon (Pen pen, PointF [] points)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipDrawPolygon (nativeObject, pen.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		internal void DrawRectangle (Pen pen, RectangleF rect)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, Rectangle rect)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			DrawRectangle (pen, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void DrawRectangle (Pen pen, float x, float y, float width, float height)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			Status status = GDIPlus.GdipDrawRectangle (nativeObject, pen.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void DrawRectangle (Pen pen, int x, int y, int width, int height)
		{
			if (pen == null)
				throw new ArgumentNullException ("pen");
			Status status = GDIPlus.GdipDrawRectangleI (nativeObject, pen.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void DrawRectangles (Pen pen, RectangleF [] rects)
		{
			if (pen == null)
				throw new ArgumentNullException ("image");
			if (rects == null)
				throw new ArgumentNullException ("rects");
			Status status = GDIPlus.GdipDrawRectangles (nativeObject, pen.nativeObject, rects, rects.Length);
			GDIPlus.CheckStatus (status);
		}

		public void DrawRectangles (Pen pen, Rectangle [] rects)
		{
			if (pen == null)
				throw new ArgumentNullException ("image");
			if (rects == null)
				throw new ArgumentNullException ("rects");
			Status status = GDIPlus.GdipDrawRectanglesI (nativeObject, pen.nativeObject, rects, rects.Length);
			GDIPlus.CheckStatus (status);
		}

		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle)
		{
			DrawString (s, font, brush, layoutRectangle, null);
		}

		public void DrawString (string s, Font font, Brush brush, PointF point)
		{
			DrawString (s, font, brush, new RectangleF (point.X, point.Y, 0, 0), null);
		}

		public void DrawString (string s, Font font, Brush brush, PointF point, StringFormat format)
		{
			DrawString(s, font, brush, new RectangleF(point.X, point.Y, 0, 0), format);
		}

		public void DrawString (string s, Font font, Brush brush, float x, float y)
		{
			DrawString (s, font, brush, new RectangleF (x, y, 0, 0), null);
		}

		public void DrawString (string s, Font font, Brush brush, float x, float y, StringFormat format)
		{
			DrawString (s, font, brush, new RectangleF(x, y, 0, 0), format);
		}

		public void DrawString (string s, Font font, Brush brush, RectangleF layoutRectangle, StringFormat format)
		{
			if (font == null)
				throw new ArgumentNullException ("font");
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (s == null || s.Length == 0)
				return;

			Status status = GDIPlus.GdipDrawString (nativeObject, s, s.Length, font.NativeObject, ref layoutRectangle, format != null ? format.NativeObject : IntPtr.Zero, brush.nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void EndContainer (GraphicsContainer container)
		{
			if (container == null)
				throw new ArgumentNullException ("container");
			Status status = GDIPlus.GdipEndContainer(nativeObject, container.NativeObject);
			GDIPlus.CheckStatus (status);
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
			Status status = GDIPlus.GdipSetClipRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Exclude);
			GDIPlus.CheckStatus (status);
		}

		public void ExcludeClip (Region region)
		{
			if (region == null)
				throw new ArgumentNullException ("region");
			Status status = GDIPlus.GdipSetClipRegion (nativeObject, region.NativeObject, CombineMode.Exclude);
			GDIPlus.CheckStatus (status);
		}

		
		public void FillClosedCurve (Brush brush, PointF [] points)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipFillClosedCurve (nativeObject, brush.NativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		
		public void FillClosedCurve (Brush brush, Point [] points)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipFillClosedCurveI (nativeObject, brush.NativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		
		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			FillClosedCurve (brush, points, fillmode, 0.5f);
		}
		
		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			FillClosedCurve (brush, points, fillmode, 0.5f);
		}

		public void FillClosedCurve (Brush brush, PointF [] points, FillMode fillmode, float tension)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipFillClosedCurve2 (nativeObject, brush.NativeObject, points, points.Length, tension, fillmode);
			GDIPlus.CheckStatus (status);
		}

		public void FillClosedCurve (Brush brush, Point [] points, FillMode fillmode, float tension)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipFillClosedCurve2I (nativeObject, brush.NativeObject, points, points.Length, tension, fillmode);
			GDIPlus.CheckStatus (status);
		}

		public void FillEllipse (Brush brush, Rectangle rect)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			FillEllipse (brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void FillEllipse (Brush brush, RectangleF rect)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			FillEllipse (brush, rect.X, rect.Y, rect.Width, rect.Height);
		}

		public void FillEllipse (Brush brush, float x, float y, float width, float height)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
                        Status status = GDIPlus.GdipFillEllipse (nativeObject, brush.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void FillEllipse (Brush brush, int x, int y, int width, int height)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			Status status = GDIPlus.GdipFillEllipseI (nativeObject, brush.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void FillPath (Brush brush, GraphicsPath path)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (path == null)
				throw new ArgumentNullException ("path");
			Status status = GDIPlus.GdipFillPath (nativeObject, brush.NativeObject,  path.NativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void FillPie (Brush brush, Rectangle rect, float startAngle, float sweepAngle)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			Status status = GDIPlus.GdipFillPie (nativeObject, brush.NativeObject, rect.X, rect.Y, rect.Width, rect.Height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		public void FillPie (Brush brush, int x, int y, int width, int height, int startAngle, int sweepAngle)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			Status status = GDIPlus.GdipFillPieI (nativeObject, brush.NativeObject, x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		public void FillPie (Brush brush, float x, float y, float width, float height, float startAngle, float sweepAngle)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			Status status = GDIPlus.GdipFillPie (nativeObject, brush.NativeObject, x, y, width, height, startAngle, sweepAngle);
			GDIPlus.CheckStatus (status);
		}

		public void FillPolygon (Brush brush, PointF [] points)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipFillPolygon2 (nativeObject, brush.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		public void FillPolygon (Brush brush, Point [] points)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipFillPolygon2I (nativeObject, brush.nativeObject, points, points.Length);
			GDIPlus.CheckStatus (status);
		}

		public void FillPolygon (Brush brush, Point [] points, FillMode fillMode)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipFillPolygonI (nativeObject, brush.nativeObject, points, points.Length, fillMode);
			GDIPlus.CheckStatus (status);
		}

		public void FillPolygon (Brush brush, PointF [] points, FillMode fillMode)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (points == null)
				throw new ArgumentNullException ("points");
			Status status = GDIPlus.GdipFillPolygon (nativeObject, brush.nativeObject, points, points.Length, fillMode);
			GDIPlus.CheckStatus (status);
		}

		public void FillRectangle (Brush brush, RectangleF rect)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
                        FillRectangle (brush, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void FillRectangle (Brush brush, Rectangle rect)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
                        FillRectangle (brush, rect.Left, rect.Top, rect.Width, rect.Height);
		}

		public void FillRectangle (Brush brush, int x, int y, int width, int height)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			
			Status status = GDIPlus.GdipFillRectangleI (nativeObject, brush.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void FillRectangle (Brush brush, float x, float y, float width, float height)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			
			Status status = GDIPlus.GdipFillRectangle (nativeObject, brush.nativeObject, x, y, width, height);
			GDIPlus.CheckStatus (status);
		}

		public void FillRectangles (Brush brush, Rectangle [] rects)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			
			Status status = GDIPlus.GdipFillRectanglesI (nativeObject, brush.nativeObject, rects, rects.Length);
			GDIPlus.CheckStatus (status);
		}

		public void FillRectangles (Brush brush, RectangleF [] rects)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			
			Status status = GDIPlus.GdipFillRectangles (nativeObject, brush.nativeObject, rects, rects.Length);
			GDIPlus.CheckStatus (status);
		}

		
		public void FillRegion (Brush brush, Region region)
		{
			if (brush == null)
				throw new ArgumentNullException ("brush");
			if (region == null)
				throw new ArgumentNullException ("region");
			
			Status status = GDIPlus.GdipFillRegion (nativeObject, brush.NativeObject, region.NativeObject);                  
                        GDIPlus.CheckStatus(status);
		}

		
		public void Flush ()
		{
			Flush (FlushIntention.Flush);
		}

		
		public void Flush (FlushIntention intention)
		{
			if (nativeObject == IntPtr.Zero) {
				return;
			}

			Status status = GDIPlus.GdipFlush (nativeObject, intention);
                        GDIPlus.CheckStatus (status);                    
			if (GDIPlus.UseQuartzDrawable || GDIPlus.UseCocoaDrawable)
				Carbon.CGContextSynchronize (GDIPlus.Display);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]		
		public static Graphics FromHdc (IntPtr hdc)
		{
			IntPtr graphics;
			Status status = GDIPlus.GdipCreateFromHDC (hdc, out graphics);
			GDIPlus.CheckStatus (status);
			return new Graphics (graphics);
		}

		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Graphics FromHdc (IntPtr hdc, IntPtr hdevice)
		{
			throw new NotImplementedException ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static Graphics FromHdcInternal (IntPtr hdc)
		{
			GDIPlus.Display = hdc;
			return null;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]		
		public static Graphics FromHwnd (IntPtr hwnd)
		{
			IntPtr graphics;

			if (GDIPlus.UseCocoaDrawable) {
				CarbonContext cgContext = Carbon.GetCGContextForNSView (hwnd);
				GDIPlus.GdipCreateFromQuartz_macosx (cgContext.ctx, cgContext.width, cgContext.height, out graphics);
				
				GDIPlus.Display = cgContext.ctx;
				return new Graphics (graphics);
			}
			if (GDIPlus.UseQuartzDrawable) {
				CarbonContext cgContext = Carbon.GetCGContextForView (hwnd);
				GDIPlus.GdipCreateFromQuartz_macosx (cgContext.ctx, cgContext.width, cgContext.height, out graphics);
				
				GDIPlus.Display = cgContext.ctx;
				return new Graphics (graphics);
			}
			if (GDIPlus.UseX11Drawable) {
				if (GDIPlus.Display == IntPtr.Zero) {
					GDIPlus.Display = GDIPlus.XOpenDisplay (IntPtr.Zero);
				}

				return FromXDrawable (hwnd, GDIPlus.Display);

			}

			Status status = GDIPlus.GdipCreateFromHWND (hwnd, out graphics);
			GDIPlus.CheckStatus (status);

			return new Graphics (graphics);
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public static Graphics FromHwndInternal (IntPtr hwnd)
		{
			return FromHwnd (hwnd);
		}

		public static Graphics FromImage (Image image)
		{
			IntPtr graphics;

			if (image == null) 
				throw new ArgumentNullException ("image");

			Status status = GDIPlus.GdipGetImageGraphicsContext (image.nativeObject, out graphics);
			GDIPlus.CheckStatus (status);
			Graphics result = new Graphics (graphics);
				
			// check for Unix platforms - see FAQ for more details
			// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
			int platform = (int) Environment.OSVersion.Platform;
			if ((platform == 4) || (platform == 128)) {
				Rectangle rect  = new Rectangle (0,0, image.Width, image.Height);
				GDIPlus.GdipSetVisibleClip_linux (result.NativeObject, ref rect);
			}
				
			return result;
		}

		internal static Graphics FromXDrawable (IntPtr drawable, IntPtr display)
		{
			IntPtr graphics;

			Status s = GDIPlus.GdipCreateFromXDrawable_linux (drawable, display, out graphics);
			GDIPlus.CheckStatus (s);
			return new Graphics (graphics);
		}

		[MonoTODO]
		public static IntPtr GetHalftonePalette ()
		{
			throw new NotImplementedException ();
		}

#if !NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
		public IntPtr GetHdc ()
		{
			IntPtr hdc;
			GDIPlus.CheckStatus (GDIPlus.GdipGetDC (this.nativeObject, out hdc));
			return hdc;
		}

		
		public Color GetNearestColor (Color color)
		{
			int argb;
			
			Status status = GDIPlus.GdipGetNearestColor (nativeObject, out argb);
			GDIPlus.CheckStatus (status);

			return Color.FromArgb (argb);
		}

		
		public void IntersectClip (Region region)
		{
			if (region == null)
				throw new ArgumentNullException ("region");
			Status status = GDIPlus.GdipSetClipRegion (nativeObject, region.NativeObject, CombineMode.Intersect);
			GDIPlus.CheckStatus (status);
		}
		
		public void IntersectClip (RectangleF rect)
		{
			Status status = GDIPlus.GdipSetClipRect (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Intersect);
			GDIPlus.CheckStatus (status);
		}

		public void IntersectClip (Rectangle rect)
		{			
			Status status = GDIPlus.GdipSetClipRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, CombineMode.Intersect);
			GDIPlus.CheckStatus (status);
		}

		public bool IsVisible (Point point)
		{
			bool isVisible = false;

			Status status = GDIPlus.GdipIsVisiblePointI (nativeObject, point.X, point.Y, out isVisible);
			GDIPlus.CheckStatus (status);

                        return isVisible;
		}

		
		public bool IsVisible (RectangleF rect)
		{
			bool isVisible = false;

			Status status = GDIPlus.GdipIsVisibleRect (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, out isVisible);
			GDIPlus.CheckStatus (status);

                        return isVisible;
		}

		public bool IsVisible (PointF point)
		{
			bool isVisible = false;

			Status status = GDIPlus.GdipIsVisiblePoint (nativeObject, point.X, point.Y, out isVisible);
			GDIPlus.CheckStatus (status);

                        return isVisible;
		}
		
		public bool IsVisible (Rectangle rect)
		{
			bool isVisible = false;

			Status status = GDIPlus.GdipIsVisibleRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, out isVisible);
			GDIPlus.CheckStatus (status);

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

		
		public Region [] MeasureCharacterRanges (string text, Font font, RectangleF layoutRect, StringFormat stringFormat)
		{	
			Status status;			
			int regcount = stringFormat.GetMeasurableCharacterRangeCount ();
			IntPtr[] native_regions = new IntPtr [regcount];
			Region[] regions = new Region [regcount];
			
			for (int i = 0; i < regcount; i++) {
				regions[i] = new Region ();
				native_regions[i] = regions[i].NativeObject;
			}
			
			status =  GDIPlus.GdipMeasureCharacterRanges (nativeObject, text, text.Length,
				font.NativeObject, ref layoutRect, stringFormat.NativeObject, 
				regcount, out native_regions[0]); 
			
			GDIPlus.CheckStatus (status);				
							
			return regions;							
		}

		
		public SizeF MeasureString (string text, Font font)
		{
			return MeasureString (text, font, new Size (0, 0));
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea)
		{
			int charactersFitted, linesFilled;
			RectangleF boundingBox = new RectangleF ();
			RectangleF rect = new RectangleF (0, 0, layoutArea.Width,
							  layoutArea.Height);

			if (text == null || text.Length == 0)
				return SizeF.Empty;

			if (font == null)
				throw new ArgumentNullException ("font");

			Status status = GDIPlus.GdipMeasureString (nativeObject, text, text.Length,
								   font.NativeObject, ref rect,
								   IntPtr.Zero, out boundingBox,
								   out charactersFitted, out linesFilled);
			GDIPlus.CheckStatus (status);

			return new SizeF (boundingBox.Width, boundingBox.Height);
		}

		
		public SizeF MeasureString (string text, Font font, int width)
		{				
			RectangleF boundingBox = new RectangleF ();
			RectangleF rect = new RectangleF (0, 0, width, 999999);
			int charactersFitted, linesFilled;

			if (text == null || text.Length == 0)
				return SizeF.Empty;

			if (font == null)
				throw new ArgumentNullException ("font");

			Status status = GDIPlus.GdipMeasureString (nativeObject, text, text.Length, 
								   font.NativeObject, ref rect,
								   IntPtr.Zero, out boundingBox,
								   out charactersFitted, out linesFilled);
			GDIPlus.CheckStatus (status);

			return new SizeF (boundingBox.Width, boundingBox.Height);
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea,
					    StringFormat stringFormat)
		{
			int charactersFitted, linesFilled;			
			return MeasureString (text, font, layoutArea, stringFormat,
					      out charactersFitted, out linesFilled);
		}

		
		public SizeF MeasureString (string text, Font font, int width, StringFormat format)
		{
			int charactersFitted, linesFilled;			
			return MeasureString (text, font, new SizeF (width, 999999), 
					      format, out charactersFitted, out linesFilled);
		}

		
		public SizeF MeasureString (string text, Font font, PointF origin,
					    StringFormat stringFormat)
		{
			RectangleF boundingBox = new RectangleF ();
			RectangleF rect = new RectangleF (origin.X, origin.Y, 0, 0);
			int charactersFitted, linesFilled;

			if (text == null || text.Length == 0)
				return SizeF.Empty;

			if (font == null)
				throw new ArgumentNullException ("font");

			if (stringFormat == null)
				stringFormat = new StringFormat ();

			Status status = GDIPlus.GdipMeasureString (nativeObject, text, text.Length, 
								   font.NativeObject, ref rect,
								   stringFormat.NativeObject, 
								   out boundingBox,
								   out charactersFitted,
								   out linesFilled);
			GDIPlus.CheckStatus (status);

			return new SizeF (boundingBox.Width, boundingBox.Height);
		}

		
		public SizeF MeasureString (string text, Font font, SizeF layoutArea,
					    StringFormat stringFormat, out int charactersFitted,
					    out int linesFilled)
		{	
			RectangleF boundingBox = new RectangleF ();
			RectangleF rect = new RectangleF (0, 0, layoutArea.Width, layoutArea.Height);
			charactersFitted = 0;
			linesFilled = 0;

			if (text == null || text.Length == 0)
				return SizeF.Empty;

			if (font == null)
				throw new ArgumentNullException ("font");

			if (stringFormat == null)
				stringFormat = new StringFormat ();

			Status status = GDIPlus.GdipMeasureString (nativeObject, text, text.Length, 
								   font.NativeObject, ref rect,
								   stringFormat.NativeObject,
								   out boundingBox,
								   out charactersFitted,
								   out linesFilled);
			GDIPlus.CheckStatus (status);

			return new SizeF (boundingBox.Width, boundingBox.Height);
		}

		public void MultiplyTransform (Matrix matrix)
		{
			MultiplyTransform (matrix, MatrixOrder.Prepend);
		}

		public void MultiplyTransform (Matrix matrix, MatrixOrder order)
		{
			Status status = GDIPlus.GdipMultiplyWorldTransform (nativeObject,
									    matrix.nativeMatrix,
									    order);
			GDIPlus.CheckStatus (status);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public void ReleaseHdc (IntPtr hdc)
		{
			Status status = GDIPlus.GdipReleaseDC (nativeObject, hdc);
			GDIPlus.CheckStatus (status);
		}
#if NET_2_0
		public void ReleaseHdc()
		{
		      
		}
#endif
		[MonoTODO]
#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
#else
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#endif
		[SecurityPermission (SecurityAction.LinkDemand, UnmanagedCode = true)]
		public void ReleaseHdcInternal (IntPtr hdc)
		{
			throw new NotImplementedException ();
		}

		
		public void ResetClip ()
		{
			Status status = GDIPlus.GdipResetClip (nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void ResetTransform ()
		{
			Status status = GDIPlus.GdipResetWorldTransform (nativeObject);
			GDIPlus.CheckStatus (status);
		}

		public void Restore (GraphicsState gstate)
		{			
			Status status = GDIPlus.GdipRestoreGraphics (nativeObject, gstate.nativeState);
			GDIPlus.CheckStatus (status);
		}


		public void RotateTransform (float angle)
		{
			RotateTransform (angle, MatrixOrder.Prepend);
		}

		public void RotateTransform (float angle, MatrixOrder order)
		{

			Status status = GDIPlus.GdipRotateWorldTransform (nativeObject, angle, order);
			GDIPlus.CheckStatus (status);
		}

		public GraphicsState Save ()
		{						
			uint saveState;
			Status status = GDIPlus.GdipSaveGraphics (nativeObject, out saveState);
			GDIPlus.CheckStatus (status);

			GraphicsState state = new GraphicsState ();
			state.nativeState = saveState;
			return state;
		}

		public void ScaleTransform (float sx, float sy)
		{
			ScaleTransform (sx, sy, MatrixOrder.Prepend);
		}

		public void ScaleTransform (float sx, float sy, MatrixOrder order)
		{
                        Status status = GDIPlus.GdipScaleWorldTransform (nativeObject, sx, sy, order);
			GDIPlus.CheckStatus (status);
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
			if (g == null)
				throw new ArgumentNullException ("g");
			
			Status status = GDIPlus.GdipSetClipGraphics (nativeObject, g.NativeObject, combineMode);
			GDIPlus.CheckStatus (status);
		}

		
		public void SetClip (Rectangle rect, CombineMode combineMode)
		{
			Status status = GDIPlus.GdipSetClipRectI (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, combineMode);
			GDIPlus.CheckStatus (status);
		}

		
		public void SetClip (RectangleF rect, CombineMode combineMode)
		{
			Status status = GDIPlus.GdipSetClipRect (nativeObject, rect.X, rect.Y, rect.Width, rect.Height, combineMode);
			GDIPlus.CheckStatus (status);
		}

		
		public void SetClip (Region region, CombineMode combineMode)
		{
			if (region == null)
				throw new ArgumentNullException ("region");
			Status status =   GDIPlus.GdipSetClipRegion(nativeObject,  region.NativeObject, combineMode); 
			GDIPlus.CheckStatus (status);
		}

		
		public void SetClip (GraphicsPath path, CombineMode combineMode)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			Status status = GDIPlus.GdipSetClipPath (nativeObject, path.NativeObject, combineMode);
			GDIPlus.CheckStatus (status);
		}

		
		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, PointF [] pts)
		{
			if (pts == null)
				throw new ArgumentNullException ("pts");

			IntPtr ptrPt =  GDIPlus.FromPointToUnManagedMemory (pts);
            
                        Status status = GDIPlus.GdipTransformPoints (nativeObject, destSpace, srcSpace,  ptrPt, pts.Length);
			GDIPlus.CheckStatus (status);
			
			GDIPlus.FromUnManagedMemoryToPoint (ptrPt, pts);
		}


		public void TransformPoints (CoordinateSpace destSpace, CoordinateSpace srcSpace, Point [] pts)
		{						
			if (pts == null)
				throw new ArgumentNullException ("pts");
                        IntPtr ptrPt =  GDIPlus.FromPointToUnManagedMemoryI (pts);
            
                        Status status = GDIPlus.GdipTransformPointsI (nativeObject, destSpace, srcSpace, ptrPt, pts.Length);
			GDIPlus.CheckStatus (status);
			
			GDIPlus.FromUnManagedMemoryToPointI (ptrPt, pts);
		}

		
		public void TranslateClip (int dx, int dy)
		{
			Status status = GDIPlus.GdipTranslateClipI (nativeObject, dx, dy);
			GDIPlus.CheckStatus (status);
		}

		
		public void TranslateClip (float dx, float dy)
		{
			Status status = GDIPlus.GdipTranslateClip (nativeObject, dx, dy);
			GDIPlus.CheckStatus (status);
		}

		public void TranslateTransform (float dx, float dy)
		{
			TranslateTransform (dx, dy, MatrixOrder.Prepend);
		}

		
		public void TranslateTransform (float dx, float dy, MatrixOrder order)
		{			
			Status status = GDIPlus.GdipTranslateWorldTransform (nativeObject, dx, dy, order);
			GDIPlus.CheckStatus (status);
		}

		public Region Clip {
			get {
				Region reg = new Region();
				Status status = GDIPlus.GdipGetClip (nativeObject, reg.NativeObject);
				GDIPlus.CheckStatus (status);
				return reg;				
			}
			set {
				SetClip (value, CombineMode.Replace);
			}
		}

		public RectangleF ClipBounds {
			get {
                                RectangleF rect = new RectangleF ();
                                Status status = GDIPlus.GdipGetClipBounds (nativeObject, out rect);
				GDIPlus.CheckStatus (status);
				return rect;
			}
		}

		public CompositingMode CompositingMode {
			get {
                                CompositingMode mode;
                                Status status = GDIPlus.GdipGetCompositingMode (nativeObject, out mode);
				GDIPlus.CheckStatus (status);

				return mode;
			}
			set {
                                Status status = GDIPlus.GdipSetCompositingMode (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}

		}

		public CompositingQuality CompositingQuality {
			get {
                                CompositingQuality quality;

                                Status status = GDIPlus.GdipGetCompositingQuality (nativeObject, out quality);
				GDIPlus.CheckStatus (status);
        			return quality;
			}
			set {
                                Status status = GDIPlus.GdipSetCompositingQuality (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public float DpiX {
			get {
                                float x;

       				Status status = GDIPlus.GdipGetDpiX (nativeObject, out x);
				GDIPlus.CheckStatus (status);
        			return x;
			}
		}

		public float DpiY {
			get {
                                float y;

       				Status status = GDIPlus.GdipGetDpiY (nativeObject, out y);
				GDIPlus.CheckStatus (status);
        			return y;
			}
		}

		public InterpolationMode InterpolationMode {
			get {				
                                InterpolationMode imode = InterpolationMode.Invalid;
        			Status status = GDIPlus.GdipGetInterpolationMode (nativeObject, out imode);
				GDIPlus.CheckStatus (status);
        			return imode;
			}
			set {
                                Status status = GDIPlus.GdipSetInterpolationMode (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public bool IsClipEmpty {
			get {
                                bool isEmpty = false;

        			Status status = GDIPlus.GdipIsClipEmpty (nativeObject, out isEmpty);
				GDIPlus.CheckStatus (status);
        			return isEmpty;
			}
		}

		public bool IsVisibleClipEmpty {
			get {
                                bool isEmpty = false;

        			Status status = GDIPlus.GdipIsVisibleClipEmpty (nativeObject, out isEmpty);
				GDIPlus.CheckStatus (status);
        			return isEmpty;
			}
		}

		public float PageScale {
			get {
                                float scale;

        			Status status = GDIPlus.GdipGetPageScale (nativeObject, out scale);
				GDIPlus.CheckStatus (status);
        			return scale;
			}
			set {
                                Status status = GDIPlus.GdipSetPageScale (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public GraphicsUnit PageUnit {
			get {
                                GraphicsUnit unit;
                                
                                Status status = GDIPlus.GdipGetPageUnit (nativeObject, out unit);
				GDIPlus.CheckStatus (status);
        			return unit;
			}
			set {
                                Status status = GDIPlus.GdipSetPageUnit (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public PixelOffsetMode PixelOffsetMode {
			get {
			        PixelOffsetMode pixelOffset = PixelOffsetMode.Invalid;
                                
                                Status status = GDIPlus.GdipGetPixelOffsetMode (nativeObject, out pixelOffset);
				GDIPlus.CheckStatus (status);
        			return pixelOffset;
			}
			set {
                                Status status = GDIPlus.GdipSetPixelOffsetMode (nativeObject, value); 
				GDIPlus.CheckStatus (status);
			}
		}

		public Point RenderingOrigin {
			get {
                                int x, y;
				Status status = GDIPlus.GdipGetRenderingOrigin (nativeObject, out x, out y);
				GDIPlus.CheckStatus (status);
                                return new Point (x, y);
			}

			set {
                                Status status = GDIPlus.GdipSetRenderingOrigin (nativeObject, value.X, value.Y);
				GDIPlus.CheckStatus (status);
			}
		}

		public SmoothingMode SmoothingMode {
			get {
                                SmoothingMode mode = SmoothingMode.Invalid;

				Status status = GDIPlus.GdipGetSmoothingMode (nativeObject, out mode);
				GDIPlus.CheckStatus (status);
                                return mode;
			}

			set {
                                Status status = GDIPlus.GdipSetSmoothingMode (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public int TextContrast {
			get {	
                                int contrast;
					
                                Status status = GDIPlus.GdipGetTextContrast (nativeObject, out contrast);
				GDIPlus.CheckStatus (status);
                                return contrast;
			}

                        set {
                                Status status = GDIPlus.GdipSetTextContrast (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public TextRenderingHint TextRenderingHint {
			get {
                                TextRenderingHint hint;

                                Status status = GDIPlus.GdipGetTextRenderingHint (nativeObject, out hint);
				GDIPlus.CheckStatus (status);
                                return hint;        
			}

			set {
                                Status status = GDIPlus.GdipSetTextRenderingHint (nativeObject, value);
				GDIPlus.CheckStatus (status);
			}
		}

		public Matrix Transform {
			get {
                                Matrix matrix = new Matrix ();
                                Status status = GDIPlus.GdipGetWorldTransform (nativeObject, matrix.nativeMatrix);
				GDIPlus.CheckStatus (status);
                                return matrix;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				
                                Status status = GDIPlus.GdipSetWorldTransform (nativeObject, value.nativeMatrix);
				GDIPlus.CheckStatus (status);
			}
		}

		public RectangleF VisibleClipBounds {
			get {
                                RectangleF rect;
					
                                Status status = GDIPlus.GdipGetVisibleClipBounds (nativeObject, out rect);
				GDIPlus.CheckStatus (status);
                                return rect;
			}
		}
	}
}

