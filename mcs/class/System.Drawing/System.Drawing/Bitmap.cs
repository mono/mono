//
// System.Drawing.Bitmap.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
//
// Alexandre Pigolkine (pigolkine@gmx.de)
//
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.Serialization;

namespace System.Drawing {

	[Serializable]
	public sealed class Bitmap : Image {

		#region constructors
		// constructors
		public Bitmap (int width, int height) : this (width, height, PixelFormat.Format32bppArgb)
		{
		}

		public Bitmap (int width, int height, Graphics g) {
			image_size = new Size(width, height);
			int bmp = 0;
			GDIPlus.GdipCreateBitmapFromGraphics (width, height, g.nativeObject, out bmp);
			nativeObject = (IntPtr)bmp;
			pixel_format = PixelFormat.Format32bppArgb;
		}

		public Bitmap (int width, int height, PixelFormat format) {
			image_size = new Size(width, height);
			int bmp = 0;
			GDIPlus.GdipCreateBitmapFromScan0 (width, height, 0, format, IntPtr.Zero, out bmp);
			nativeObject = (IntPtr)bmp;
			pixel_format = format;
		}

		public Bitmap (Image original) {
			image_size = original.Size;
		}

		public Bitmap (Stream stream) {
			throw new NotImplementedException ();
		}

		public Bitmap (string filename) {
			throw new NotImplementedException ();
		}

		public Bitmap (Image original, Size newSize) {
			image_size = newSize;
		}

		public Bitmap (Stream stream, bool useIcm) {
			throw new NotImplementedException ();
		}

		public Bitmap (string filename, bool useIcm) {
			throw new NotImplementedException ();
		}

		public Bitmap (Type type, string resource) {
			throw new NotImplementedException ();
		}

		public Bitmap (Image original, int width, int heigth) {
			throw new NotImplementedException ();
		}

		public Bitmap (int width, int height, int stride,
			       PixelFormat format, IntPtr scan0) {
			throw new NotImplementedException ();
		}

       	private Bitmap (SerializationInfo info, StreamingContext context)
		{
		}

		#endregion
		// methods
		public Color GetPixel (int x, int y) {
			throw new NotImplementedException ();
		}

		public void SetPixel (int x, int y, Color color) {
			throw new NotImplementedException ();
		}

		public Bitmap Clone (Rectangle rect,PixelFormat format) {
			Bitmap result = new Bitmap(1, 1);
			throw new NotImplementedException ();
		}
		
		public Bitmap Clone (RectangleF rect, PixelFormat format) {
			Bitmap result = new Bitmap(1, 1);
			throw new NotImplementedException ();
		}

		public static Bitmap FromHicon (IntPtr hicon) {
			throw new NotImplementedException ();
		}

		public static Bitmap FromResource (IntPtr hinstance,
		                                   string bitmapName) {
			throw new NotImplementedException ();
		}

		public IntPtr GetHbitmap () {
			throw new NotImplementedException ();
		}

		public IntPtr GetHbitmap (Color background) {
			throw new NotImplementedException ();
		}

		public IntPtr GetHicon () {
			throw new NotImplementedException ();
		}

		public BitmapData LockBits (Rectangle rect, ImageLockMode flags,
		                            PixelFormat format) {
		    Rect rc = new Rect ();
		    rc.left = rect.Left;
		    rc.right = rect.Right;
		    rc.top = rect.Top;
		    rc.bottom = rect.Bottom;
		    BitmapData_RAW raw = new BitmapData_RAW();
			GDIPlus.GdipBitmapLockBits (nativeObject, ref rc, flags, format, ref raw);
		    BitmapData bmpData = new BitmapData();
		    bmpData.Height = raw.height;
		    bmpData.Width = raw.width;
		    bmpData.PixelFormat = (PixelFormat)raw.pixelFormat;
		    bmpData.Reserved = raw.reserved;
		    bmpData.Scan0 = raw.scan0;
		    bmpData.Stride = raw.stride;
			return bmpData;
		}

		public void MakeTransparent () {
			throw new NotImplementedException ();
		}

		public void MakeTransparent (Color transparentColor) {
			throw new NotImplementedException ();
		}

		public void SetResolution (float xDpi, float yDpi) {
			throw new NotImplementedException ();
		}

		public void UnlockBits (BitmapData bitmapdata) {
		    BitmapData_RAW raw = new BitmapData_RAW();
		    raw.height = bitmapdata.Height;
		    raw.width = bitmapdata.Width;
		    raw.pixelFormat = (int)bitmapdata.PixelFormat;
		    raw.reserved = bitmapdata.Reserved;
		    raw.scan0 = bitmapdata.Scan0;
		    raw.stride = bitmapdata.Stride;
			GDIPlus.GdipBitmapUnlockBits (nativeObject, ref raw);
		}

		// properties
		// needs to be done ###FIXME###
	}
}
