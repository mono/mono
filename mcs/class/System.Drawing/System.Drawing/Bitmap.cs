 //
// System.Drawing.Bitmap.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
//
// No implementation has been done yet. I first want to write the method
// headers of every System.Drawing.
//
//Buid warnings. Note add 6 to line numbers for these comments!
//C:\cygwin\usr\local\mcs\class\System.Drawing\System.Drawing\Bitmap.cs(47,18): warning CS0649: Field 'System.Drawing.RGBQUAD.rgbBlue' is never assigned to, and will always have its default value 0
//C:\cygwin\usr\local\mcs\class\System.Drawing\System.Drawing\Bitmap.cs(48,18): warning CS0649: Field 'System.Drawing.RGBQUAD.rgbGreen' is never assigned to, and will always have its default value 0
//C:\cygwin\usr\local\mcs\class\System.Drawing\System.Drawing\Bitmap.cs(49,18): warning CS0649: Field 'System.Drawing.RGBQUAD.rgbRed' is never assigned to, and will always have its default value 0
//C:\cygwin\usr\local\mcs\class\System.Drawing\System.Drawing\Bitmap.cs(50,18): warning CS0649: Field 'System.Drawing.RGBQUAD.rgbReserved' is never assigned to, and will always have its default value 0
//C:\cygwin\usr\local\mcs\class\System.Drawing\System.Drawing\Bitmap.cs(54,20): warning CS0649: Field 'System.Drawing.BITMAPINFO.colorpalette' is never assigned to, and will always have its default value null
// 2002-03-27  Christian Meyer  <Christian.Meyer@cs.tum.edu>
// I'll have a closer look at it next week.
//
// Alexandre Pigolkine (pigolkine@gmx.de)
// delegate all calls to implementation
//
using System;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.Serialization;

namespace System.Drawing {
	struct BITMAPFILEHEADER {        // File info header
		public uint bfType;      // Specifies the type of file. This member must be BM.
		public uint bfSize;      // Specifies the size of the file, in bytes.
		public uint bfReserved1; // Reserved; must be set to zero.
		public uint bfReserved2; // Reserved; must be set to zero.
		public uint bfOffBits;   // Specifies the byte offset from the BITMAPFILEHEADER
                                         // structure to the actual bitmap data in the file.
	}
	struct BITMAPINFOHEADER {        // bitmap info header
		public uint   biSize;
		public int    biWidth;
		public int    biHeight;
		public ushort biPlanes;
		public ushort biBitCount;
		public uint   biCompression;
		public uint   biSizeImage;
		public int    biXPelsPerMeter;
		public int    biYPelsPerMeter;
		public uint   biClrUsed;
		public uint   biClrImportant;
	}

	struct RGBQUAD {
		public byte rgbBlue;
		public byte rgbGreen;
		public byte rgbRed;
		public byte rgbReserved;
	}
	struct BITMAPINFO {              // bitmap info
		public BITMAPINFOHEADER bitmapinfoheader;
		public RGBQUAD[] colorpalette;
	}

	[Serializable]
	public sealed class Bitmap : Image {

		internal static IBitmapFactory	factory_ = Factories.GetBitmapFactory();

		#region constructors
		// constructors
		public Bitmap (int width, int height) {
			implementation_ = factory_.Bitmap(width, height);
			imageSize_ = new Size(width, height);
		}

		public Bitmap (int width, int height, Graphics g) {
			implementation_ = factory_.Bitmap(width, height, g);
			imageSize_ = new Size(width, height);
		}

		public Bitmap (int width, int height, PixelFormat format) {
			implementation_ = factory_.Bitmap(width, height, format);
			imageSize_ = new Size(width, height);
		}

		public Bitmap (Image original) {
			implementation_ = factory_.Bitmap(original, original.Size);
			imageSize_ = original.Size;
		}

		public Bitmap (Stream stream) {
			implementation_ = factory_.Bitmap(stream, false);
			imageSize_ = implementation_.Size;
		}

		public Bitmap (string filename) {
			implementation_ = factory_.Bitmap(filename, false);
			imageSize_ = implementation_.Size;
		}

		public Bitmap (Image original, Size newSize) {
			implementation_ = factory_.Bitmap(original, newSize);
			imageSize_ = newSize;
		}

		public Bitmap (Stream stream, bool useIcm) {
			implementation_ = factory_.Bitmap(stream, useIcm);
			imageSize_ = implementation_.Size;
		}

		public Bitmap (string filename, bool useIcm) {
			implementation_ = factory_.Bitmap(filename, useIcm);
			imageSize_ = implementation_.Size;
		}

		public Bitmap (Type type, string resource) {
			implementation_ = factory_.Bitmap(type, resource);
			imageSize_ = implementation_.Size;
		}

		public Bitmap (Image original, int width, int heigth) {
			implementation_ = factory_.Bitmap(original, new Size(width, heigth));
			imageSize_ = implementation_.Size;
		}


		public Bitmap (int width, int height, int stride,
			       PixelFormat format, IntPtr scan0) {
			implementation_ = factory_.Bitmap(width, height, stride, format, scan0);
			imageSize_ = implementation_.Size;
		}

        	private Bitmap (SerializationInfo info, StreamingContext context)
		{
		}

		#endregion
		// methods
		public Color GetPixel (int x, int y) {
			return ((IBitmap)implementation_).GetPixel(x, y);
		}

		public void SetPixel (int x, int y, Color color) {
			((IBitmap)implementation_).SetPixel(x, y, color);
		}

		public Bitmap Clone (Rectangle rect,PixelFormat format) {
			Bitmap result = new Bitmap(1, 1);
			result.implementation_ = ((IBitmap)implementation_).Clone(rect, format);
			result.imageSize_ = result.implementation_.Size;
			return result;
		}
		
		public Bitmap Clone (RectangleF rect, PixelFormat format) {
			Bitmap result = new Bitmap(1, 1);
			result.implementation_ = ((IBitmap)implementation_).Clone(rect, format);
			result.imageSize_ = result.implementation_.Size;
			return result;
		}

		public static Bitmap FromHicon (IntPtr hicon) {
			throw new NotImplementedException ();
		}

		public static Bitmap FromResource (IntPtr hinstance,
		                                   string bitmapName) {
			throw new NotImplementedException ();
		}

		public IntPtr GetHbitmap () {
			return ((IBitmap)implementation_).GetHbitmap();
		}

		public IntPtr GetHbitmap (Color background) {
			return ((IBitmap)implementation_).GetHbitmap(background);
		}

		public IntPtr GetHicon () {
			return ((IBitmap)implementation_).GetHicon();
		}

		public BitmapData LockBits (Rectangle rect, ImageLockMode flags,
		                            PixelFormat format) {
			return ((IBitmap)implementation_).LockBits(rect, flags, format);
		}

		public void MakeTransparent () {
			((IBitmap)implementation_).MakeTransparent();
		}

		public void MakeTransparent (Color transparentColor) {
			((IBitmap)implementation_).MakeTransparent(transparentColor);
		}

		public void SetResolution (float xDpi, float yDpi) {
			((IBitmap)implementation_).SetResolution(xDpi,yDpi );
		}

		public void UnlockBits (BitmapData bitmapdata) {
			((IBitmap)implementation_).UnlockBits(bitmapdata);
		}

		// properties
		// needs to be done ###FIXME###
	}
}
