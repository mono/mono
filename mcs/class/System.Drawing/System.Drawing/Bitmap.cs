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
using System;
using System.IO;

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
	// I do not think pinning is needed execpt for when locked
	// Is layout packed attribute needed here?
	struct bitmapstruct {
		//placed in a struct to keep all 3 (4 including the color table) contugious in memory.)
		public BITMAPFILEHEADER fileheader;     //File info header
		//bitmapinfo includes the color table
		public BITMAPINFO       info;           //bitmap info
		public byte[,]          bits;           //Actual bitmap bits
	}
	public sealed class Bitmap : Image {
		// TODO: add following to an enum  with BI_RLE4 and BI_RLE8
		const int BI_RGB = 0;                   //? 0 is from example;
		bitmapstruct bitmap = new bitmapstruct();
		private void CommonInit (int width, int height) {
			// Init BITMAPFILEHANDLE
			// document I am working from says tyoe must allways be "BM",
			// the example has this set to 19778.
			// TODO: verify magic number 19778 for "BM" bfType
			bitmap.fileheader.bfType = 19778;
			// TODO: is this the correct file size?
			bitmap.fileheader.bfSize = (uint)
				//bitmap
				(width * height * 4)
				//add color table, 0 for now
				+ 0
				// add header
				+ 60;
			bitmap.fileheader.bfReserved1 = 0;
			bitmap.fileheader.bfReserved2 = 0;
			// bfOffBits is bytes offset between start of bitmap (bimapfileheader)
			// and start of actual data bits.
			// Example puts it at 118 including 64 bytes of color table.
			// I count 124. What is right?
			// Also I force 32 bit color for first pass, so for now there is no color table (24 bit or greater)
			// TODO: verify magic number 124 for bfOffBits
			// TODO: Could also be sizeof(fileheader and bitmapinfo)
			bitmap.fileheader.bfOffBits = 60; //14 * 4 for ints + 2 * 2 for words.

			// Init BITMAPINFO HEADER
			// TODO: document on bitmaps shows only 1, 4, 8, 24 as valid pixel depths
			// TODO; MS's document says 32ppARGB is 32 bits per pixle, the default.

			bitmap.info.bitmapinfoheader.biBitCount = 32;
			// biclrused is the number of colors in the bitmap that are actualy used
			// in the bitmap. 0 means all. default to this.
			// TODO: As far as I know, it is fine to leave this as 0, but
			// TODO: that it would be better to do an actual count.
			// TODO: If we open an already created bitmap, we could in a later 
			// TODO: version store that.
			bitmap.info.bitmapinfoheader.biClrUsed = 0;
			// biclrused is the number of colors in the bitmap that are importiant
			// in the bitmap. 0 means all. default to this.
			// TODO: As far as I know, it is fine to leave this as 0,
			// TODO: If we open an already created bitmap, we could in a later 
			// TODO: version store that.
			// In a new bitmap, I do not know how we would know which colors are importiant.
			bitmap.info.bitmapinfoheader.biClrImportant = 0;
			// Options are BI_RGB for none, BI_RLE8 for 8 bit color ,BI_RLE4 for 4 bit color
			// Only supprt BI_RGB for now;
			// TODO: add definition for BI_***
			// TODO: correctly set biSizeImage before supporting compression.
			bitmap.info.bitmapinfoheader.biCompression = BI_RGB;
			bitmap.info.bitmapinfoheader.biHeight = height;
			bitmap.info.bitmapinfoheader.biWidth = width;
			// TODO: add support for more planes
			bitmap.info.bitmapinfoheader.biPlanes = 1;
			// TODO: replace 40 with a sizeof() call
			bitmap.info.bitmapinfoheader.biSize = 40;// size of this structure.
			// TODO: correctly set biSizeImage so compression can be supported.
			bitmap.info.bitmapinfoheader.biSizeImage = 0; //0 is allowed for BI_RGB (no compression)
			// The example uses 0 for pels per meter, so do I.
			// TODO: support pels per meter
			bitmap.info.bitmapinfoheader.biXPelsPerMeter = 0;
			bitmap.info.bitmapinfoheader.biYPelsPerMeter = 0;
			bitmap.bits = new byte[width*4, height];
		}
		#region constructors
		// constructors
		public Bitmap (int width, int height) {
			CommonInit (width, height);
		}

		public Bitmap (int width, int height, Graphics g) {
			//TODO: Error check X,Y
			CommonInit (width,height);
			//TODO: use graphics to set vertial and horzontal resolution.
			//TODO: that is all the spec requires or desires
		}

//		public Bitmap (int width, int heigth, PixelFormat format) {
//			if ((int)format != BI_RGB) {
//				throw new NotImplementedException ();
//			}
//			CommonInit (width, heigth);
//		}
//
//		public Bitmap (Image origial) {
//			throw new NotImplementedException ();
//			//this.original = original;
//		}

		public Bitmap (Stream stream) {
			throw new NotImplementedException ();
			//this.stream = stream;
		}

		public Bitmap (string filename) {
			throw new NotImplementedException ();
			//this.filename = filename;
		}

		public Bitmap (Image original, Size newSize) {
			throw new NotImplementedException ();
			//this.original = original;
			//this.newSize = newSize;
		}

		public Bitmap (Stream stream, bool useIcm) {
			throw new NotImplementedException ();
			//this.stream = stream;
			//this.useIcm = useIcm;
		}

		public Bitmap (string filename, bool useIcm) {
			throw new NotImplementedException ();
			//this.filename = filename;
			//this.useIcm = useIcm;
		}

		public Bitmap (Type type, string resource) {
			throw new NotImplementedException ();
			//this.type = type;
			//this.resource = resource;
		}

		public Bitmap (Image original, int width, int heigth) {
			throw new NotImplementedException ();
			//this.original = original;
			//this.width = width;
			//this.heigth = heigth;
		}


//		public Bitmap (int width, int height, int stride,
//			       PixelFormat format, IntPtr scan0) {
//			throw new NotImplementedException ();
//			//this.width = width;
//			//this.heigth = heigth;
//			//this.stride = stride;
//			//this.format = format;
//			//this.scan0 = scan0;
//		}
		#endregion
		// methods
		public Color GetPixel (int x, int y) {
			//TODO: Error check X,Y
			return Color.FromArgb (bitmap.bits[x,y], bitmap.bits[x+1,y], bitmap.bits[x+2,y], bitmap.bits[x+3,y]);
		}

		public void SetPixel (int x, int y, Color color) {
			//TODO: Error check X,Y
			bitmap.bits[x, y]     = color.A;
			bitmap.bits[x + 1, y] = color.R;
			bitmap.bits[x + 2, y] = color.G;
			bitmap.bits[x + 2, y] = color.B;
		}

//		public Bitmap Clone (Rectangle rect,PixelFormat format) {
//			throw new NotImplementedException ();
//		}
//		
//		public Bitmap Clone (RectangleF rect, PixelFormat format) {
//			throw new NotImplementedException ();
//		}

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

//		public BitmapData LockBits (Rectangle rect, ImageLockMode flags,
//		                            PixelFormat format) {
//			throw new NotImplementedException ();
//		}

		public void MakeTransparent () {
			throw new NotImplementedException ();
		}

		public void MakeTransparent (Color transparentColor) {
			throw new NotImplementedException ();
		}

		public void SetResolution (float xDpi, float yDpi) {
			throw new NotImplementedException ();
		}

//		public void UnlockBits (BitmapData bitmapdata) {
//			throw new NotImplementedException ();
//		}

		// properties
		// needs to be done ###FIXME###
	}
}
