//
// System.Drawing.ItfBitmap.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
// Alexandre Pigolkine <pigolkine@gmx.de>
// 
//
//
using System;
using System.IO;
using System.Drawing.Imaging;

namespace System.Drawing {
	internal interface IBitmapFactory 
	{
		IBitmap Bitmap(int width, int height);
		IBitmap Bitmap(int width, int height, Graphics g);
		IBitmap Bitmap(int width, int height, PixelFormat format);
		IBitmap Bitmap(Image original, Size newSize);
		IBitmap Bitmap(Stream stream, bool useIcm);
		IBitmap Bitmap(string filename, bool useIcm);
		IBitmap Bitmap(Type type, string resource);
		IBitmap Bitmap(int width, int height, int stride, PixelFormat format, IntPtr scan0);
	}

	internal interface IBitmap : IImage {
		// methods
		Color GetPixel (int x, int y) ;

		void SetPixel (int x, int y, Color color) ;

		IBitmap Clone (Rectangle rect, PixelFormat format);
		
		IBitmap Clone (RectangleF rect, PixelFormat format);

		IntPtr GetHbitmap () ;

		IntPtr GetHbitmap (Color background) ;

		IntPtr GetHicon () ;

		BitmapData LockBits (Rectangle rect, ImageLockMode flags,
		                            PixelFormat format);

		void MakeTransparent () ;

		void MakeTransparent (Color transparentColor) ;

		void SetResolution (float xDpi, float yDpi);

		void UnlockBits (BitmapData bitmapdata);

		// properties
		// needs to be done ###FIXME###
	}
}
