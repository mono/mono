//
// System.Drawing.gdipFunctions.cs
//
// Author: 
// Alexandre Pigolkine (pigolkine@gmx.de)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace System.Drawing {
	/// <summary>
	/// GDI+ API Functions
	/// </summary>
	public class GDIPlus {
		
		#region gdiplus.dll functions
		
		// startup / shutdown
		[DllImport("gdiplus.dll")]
		static internal extern Status GdiplusStartup(ref ulong token, ref GdiplusStartupInput input, ref GdiplusStartupOutput output);
		[DllImport("gdiplus.dll")]
		static internal extern void GdiplusShutdown(ref ulong token);
		
		static ulong GdiPlusToken;
		static GDIPlus ()
		{
			GdiplusStartupInput input = GdiplusStartupInput.MakeGdiplusStartupInput();
			GdiplusStartupOutput output = GdiplusStartupOutput.MakeGdiplusStartupOutput();
			GdiplusStartup (ref GdiPlusToken, ref input, ref output);
		}

		// Memory functions
		[DllImport("gdiplus.dll")]
		static internal extern IntPtr GdipAlloc (int size);
		[DllImport("gdiplus.dll")]
		static internal extern void GdipFree (IntPtr ptr);

		
		// Brush functions
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCloneBrush (IntPtr brush, out IntPtr clonedBrush);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipDeleteBrush (IntPtr brush);
		
		// Solid brush functions
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCreateSolidFill (int color, out int brush);
		
		// Graphics functions
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipCreateFromHDC(IntPtr hDC, out int graphics);
				
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipDeleteGraphics(IntPtr graphics);
		
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipRestoreGraphics(IntPtr graphics, uint graphicsState);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipSaveGraphics(IntPtr graphics, out uint state);
		
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipRotateWorldTransform(IntPtr graphics, float angle, MatrixOrder order);
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipTranslateWorldTransform(IntPtr graphics, float dx, float dy, MatrixOrder order);

		[DllImport("gdiplus.dll")]
                static internal extern Status GdipDrawEllipseI (IntPtr graphics, IntPtr pen, int x, int y, int width, int height);

		[DllImport("gdiplus.dll")]
                static internal extern Status GdipDrawEllipse (IntPtr graphics, IntPtr pen, float x, float y, float width, float height);

		[DllImport("gdiplus.dll")]
		static internal extern Status GdipDrawLine (IntPtr graphics, IntPtr pen, float x1, float y1, float x2, float y2);

		[DllImport("gdiplus.dll")]
		static internal extern Status GdipDrawLineI (IntPtr graphics, IntPtr pen, int x1, int y1, int x2, int y2);

                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawPolygon (IntPtr graphics, IntPtr pen, PointF [] points, int count);

                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawPolygonI (IntPtr graphics, IntPtr pen, Point [] points, int count);

                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawRectangle (IntPtr graphics, IntPtr pen, float x, float y, float width, float height);

                [DllImport ("gdiplus.dll")]
                static internal extern Status GdipDrawRectangleI (IntPtr graphics, IntPtr pen, int x, int y, int width, int height);
                
		[DllImport("gdiplus.dll")]
                static internal extern Status GdipFillEllipseI (IntPtr graphics, IntPtr pen, int x, int y, int width, int height);

		[DllImport("gdiplus.dll")]
                static internal extern Status GdipFillEllipse (IntPtr graphics, IntPtr pen, float x, float y, float width, float height);
                [DllImport ("gdiplus.dll")]
                static internal extern  Status GdipFillPolygon (IntPtr graphics, IntPtr brush, PointF [] points, int count, FillMode fillMode);

                [DllImport ("gdiplus.dll")]
                static internal extern  Status GdipFillPolygonI (IntPtr graphics, IntPtr brush, Point [] points, int count, FillMode fillMode);
                
                [DllImport ("gdiplus.dll")]
                static internal extern  Status GdipFillPolygon2 (IntPtr graphics, IntPtr brush, PointF [] points, int count);

                [DllImport ("gdiplus.dll")]
                static internal extern  Status GdipFillPolygon2I (IntPtr graphics, IntPtr brush, Point [] points, int count);
                
                [DllImport("gdiplus.dll")]
		static internal extern Status GdipFillRectangle (IntPtr graphics, IntPtr brush, float x1, float y1, float x2, float y2);

		[DllImport("gdiplus.dll")]
		static internal extern Status GdipDrawString (IntPtr graphics, string text, int len, IntPtr font, ref GdipRectF rc, IntPtr format, IntPtr brush);

		[DllImport("gdiplus.dll")]
		static internal extern Status GdipGetDC (IntPtr graphics, out int hdc);

		[DllImport("gdiplus.dll")]
		static internal extern Status GdipReleaseDC (IntPtr graphics, IntPtr hdc);
		
		[DllImport("gdiplus.dll")]
		static internal extern Status GdipDrawImageRectI (IntPtr graphics, IntPtr image, int x, int y, int width, int height);

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipGetRenderingOrigin (IntPtr graphics, out int x, out int y);

		[DllImport ("gdiplus.dll")]
		static internal extern Status GdipSetRenderingOrigin (IntPtr graphics, int x, int y);
	
		// Pen functions
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreatePen1(int argb, float width, Unit unit, out int pen);
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipDeletePen(IntPtr pen);
		
		// Bitmap functions
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreateBitmapFromScan0 (int width, int height, int stride, PixelFormat format, IntPtr scan0, out int bitmap);

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipCreateBitmapFromGraphics (int width, int height, IntPtr target, out int bitmap);

		[DllImport("gdiplus.dll")]
		internal static extern Status GdipBitmapLockBits (IntPtr bmp, ref GdipRect rc, ImageLockMode flags, PixelFormat format, [In, Out] BitmapData bmpData);
		[DllImport("gdiplus.dll")]
		internal static extern Status ____BitmapLockBits (IntPtr bmp, ref GdipRect rc, ImageLockMode flags, PixelFormat format, ref int width, ref int height, ref int stride, ref int format2, ref int reserved, ref IntPtr scan0);
		
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipBitmapUnlockBits (IntPtr bmp, [In,Out] BitmapData bmpData);

		// Image functions
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipDisposeImage (IntPtr image);
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipDrawImageI( IntPtr graphics, IntPtr image, int x, int y);
		[DllImport("gdiplus.dll")]
		internal static extern Status GdipGetImageGraphicsContext( IntPtr image, out int graphics);
		
		#endregion
	}
}
