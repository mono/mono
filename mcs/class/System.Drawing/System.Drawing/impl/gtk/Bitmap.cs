//
// System.Drawing.Bitmap.cs
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer <Christian.Meyer@cs.tum.edu>
//			Jason Perkins <jason@379.com>
//			Dennis Hayes <dennish@raytek.com>
// The original version of this file (Christian Meyer, Mar 2003) tried to 
// store all of the image data in a Win32 compatible BITMAP structure, but
// forgot that arrays are stored as pointers to objects. I removed that
// code, a more complex implementation will be needed. <note I put it back it,
// it may serve as a road map for the more complex implmentation if nothing else.
// I may remove it after furthure review - Dennis>. For now, I am just
// trying to get some image data loaded and on the screen. Long term, it
// will need to be stored as BITMAP in order to work with SWF.
//
// I decided to use GDK for the initial implementation. This probably will
// only work on Linux. There was some talk of making GDK a dependency for
// Mono, but we'll see.
//
// The byte-swap (RGB -> BGR) routine is dirt slow right now. It really needs 
// to be written with unsafe code, but I want to get an okay from the powers-
// that-be before I do.
//

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace System.Drawing {
	namespace GTKImpl {

		internal class BitmapFactory : IBitmapFactory {
			public IBitmap Bitmap(int width, int height) {
				return new Bitmap(width, height);
			}

			public IBitmap Bitmap(int width, int height, System.Drawing.Graphics g) {
				return new Bitmap(width, height, g);
			}

			public IBitmap Bitmap(int width, int height, System.Drawing.Imaging.PixelFormat format) {
				return new Bitmap(width, height, format);
			}

			public IBitmap Bitmap(System.Drawing.Image original, Size newSize){
				return new Bitmap((System.Drawing.GTKImpl.Image)original.implementation, newSize);
			}

			public IBitmap Bitmap(Stream stream, bool useIcm){
				return new Bitmap(stream);
			}

			public IBitmap Bitmap(string filename, bool useIcm){
				return new Bitmap(filename, useIcm);
			}

			public IBitmap Bitmap(Type type, string resource){
				return new Bitmap(type, resource);
			}

			public IBitmap Bitmap(int width, int height, int stride, System.Drawing.Imaging.PixelFormat format, IntPtr scan0){
				return new Bitmap(width, height, stride, format, scan0);
			}
		}

		#region Old code may need to be deleted
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
		#endregion
		internal sealed class Bitmap : Image, IBitmap {
			#region gtk
			/// <summary>
			// GTK/GDK interface
			/// </summary>
		
			const string GDK = "gdk-x11-2.0";
			const string GLIB = "gobject-2.0";
			//const string GDK = "libgdk-0.dll";
			//const string GLIB = "libgobject-2.0-0.dll";
		
			[DllImport(GDK)]
			static extern void gdk_init(int argc, IntPtr argv);
		
			[DllImport(GDK)]
			static extern IntPtr gdk_pixbuf_new(int colorspace, bool has_alpha, int bits_per_sample, int width, int height);

			[DllImport(GDK)]
			static extern int gdk_pixbuf_get_width(IntPtr pixbuf);
		
			[DllImport(GDK)]
			static extern int gdk_pixbuf_get_height(IntPtr pixbuf);
		
			[DllImport(GDK)]
			static extern int gdk_pixbuf_get_rowstride(IntPtr pixbuf);
		
			[DllImport(GDK)]
			static extern IntPtr gdk_pixbuf_get_pixels(IntPtr pixbuf);

			[DllImport(GDK)]
			static extern int gdk_pixbuf_get_has_alpha(IntPtr pixbuf);
		
			[DllImport(GDK)]
			static extern IntPtr gdk_pixbuf_loader_new();
		
			[DllImport(GDK)]
			static extern int gdk_pixbuf_loader_close(IntPtr loader, IntPtr error);
		
			[DllImport(GDK)]
			static extern int gdk_pixbuf_loader_write(IntPtr laoder, byte[] buffer, int size);
		
			[DllImport(GDK)]
			static extern IntPtr gdk_pixbuf_loader_get_pixbuf(IntPtr loader);
		
			[DllImport(GDK)]
			static extern void gdk_pixbuf_ref(IntPtr pixbuf);
		
			[DllImport(GDK)]
			static extern void gdk_pixbuf_unref(IntPtr pixbuf);
		
			[DllImport(GLIB)]
			internal static extern void g_object_unref(IntPtr ptr);

			#endregion

			//The spec does not  have a static constructor. since this is private, does it matter?
			//should it be internal?
			//should it be added with a singlton object in the instance constructors?
			static Bitmap() {
				gdk_init(0, new IntPtr(0));
			}
			internal IntPtr pixbuf;
			internal PixelFormat format;


			// TODO: add following to an enum  with BI_RLE4 and BI_RLE8
			#region old bitmap code remove??
			const int BI_RGB = 0;                   //? 0 is from example;
			bitmapstruct bitmap = new bitmapstruct();
			internal void CommonInit (int width, int height) {
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
			#endregion

			#region constructors
			/// <summary>
			/// Constructors
			/// </summary>

			public Bitmap (int width, int height) {
				CommonInit (width, height);
			}

			public Bitmap (int width, int height, Graphics g) {
				//TODO: Error check X,Y
				CommonInit (width,height);
				//TODO: use graphics to set vertial and horzontal resolution.
				//TODO: that is all the spec requires or desires
			}

			//FIXME: GTK also Should be inherited from Image
			//public override int Height {
			public new int Height {
				get {
					return gdk_pixbuf_get_height(pixbuf);
				}
			}

			[MonoTODO]
			public Bitmap(int width, int height, PixelFormat format) {
				int bits;
				bool alpha;
				switch (format) {
					case PixelFormat.Format24bppRgb:
						bits = 8;
						alpha = false;
						break;
		
					case PixelFormat.Format32bppArgb:
						bits = 8;
						alpha = true;
						break;
			
					default:
						throw new NotImplementedException();
				}
			
				this.format = format;
				//FIXME:GTK
				pixbuf = gdk_pixbuf_new(0, alpha, bits, width, height);
			}

			public Bitmap (Image origial) {
				throw new NotImplementedException ();
				//this.original = original;
			}
			//FIXME: This uses GTK
			public Bitmap(Stream stream) {
				IntPtr loader = gdk_pixbuf_loader_new();
			
				byte[] buffer = new byte[stream.Length];
				int length = stream.Read(buffer, 0, (int)stream.Length);
			
				if (gdk_pixbuf_loader_write(loader, buffer, length) == 0)
					throw new ArgumentException("Not a valid image file.");
			
				gdk_pixbuf_loader_close(loader, new IntPtr(0));
			
				pixbuf = gdk_pixbuf_loader_get_pixbuf(loader);
				gdk_pixbuf_ref(pixbuf);
			
				g_object_unref(loader);
			
				// Only format I support right now
			
				this.format = PixelFormat.Format24bppRgb;
			
				// GDK loads the data as (A)BGR instead of (A)RGB. Convert it now.
				// This really needs to be unsafe code.
			
				IntPtr pixels = gdk_pixbuf_get_pixels(pixbuf);
				int    width  = gdk_pixbuf_get_width(pixbuf);
				int    height = gdk_pixbuf_get_height(pixbuf);
				int    stride = gdk_pixbuf_get_rowstride(pixbuf);
				int    bpp    = (gdk_pixbuf_get_has_alpha(pixbuf) != 0) ? 4 : 3;
			
				int rowOffset = 0;
				for (int row = 0; row < height; ++row) {
					byte red, blue;
					for (int offset = rowOffset; offset < rowOffset + stride; offset += bpp) {
						red  = Marshal.ReadByte(pixels, offset);
						blue = Marshal.ReadByte(pixels, offset + 2);
					
						Marshal.WriteByte(pixels, offset, blue);
						Marshal.WriteByte(pixels, offset + 2, red);
					}
				
					rowOffset += stride;
				}
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


			public Bitmap (int width, int height, int stride,
				PixelFormat format, IntPtr scan0) {
				throw new NotImplementedException ();
				//this.width = width;
				//this.heigth = heigth;
				//this.stride = stride;
				//this.format = format;
				//this.scan0 = scan0;
			}
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
			public IBitmap Clone (Rectangle rect,PixelFormat format) {
				throw new NotImplementedException ();
			}
		
			public IBitmap Clone (RectangleF rect, PixelFormat format) {
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public static Bitmap FromHicon (IntPtr hicon) {
				throw new NotImplementedException ();
			}

			[MonoTODO]
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


			[MonoTODO]
			public BitmapData LockBits(Rectangle rect, ImageLockMode flags, PixelFormat format) {
				if (rect.Left != 0 || rect.Top != 0 || rect.Right != Width || rect.Bottom != Height)
					throw new NotImplementedException();
			
				BitmapData data = new BitmapData();
				data.Width = Width;
				data.Height = Height;
				data.PixelFormat = PixelFormat;
				data.Stride = gdk_pixbuf_get_rowstride(pixbuf);
				data.Scan0 = gdk_pixbuf_get_pixels(pixbuf);
				return data;
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
		
			//Fixme: gtk should be inherited from Image
			//public override PixelFormat PixelFormat {
			public new PixelFormat PixelFormat {
				get {
					return format;
				}
			}

			public void UnlockBits(BitmapData bitmapdata) {
				// nothing to do!
			}
		
			//FIXME: gtk also should be inherited from Image
			//public override int Width {
			public new int Width {
				get {
					return gdk_pixbuf_get_width(pixbuf);
				}
			}

			// properties
			// needs to be done ###FIXME###


			/// <summary>
			/// Methods 
			/// </summary>
			//FIXME: This is inherited from Image and should not be here!
			protected override void Dispose(bool disposing) {
				if (pixbuf != IntPtr.Zero) {
					gdk_pixbuf_unref(pixbuf);
					pixbuf = IntPtr.Zero;
				}
			
				base.Dispose(disposing);
			}
			//FIXME: included with gtk code. should be inherited from Image.
			//		[MonoTODO]
			//		public override void RotateFlip(RotateFlipType rotateFlipType) {
			//			throw new NotImplementedException();
			//		}
		
		}
	}
}
