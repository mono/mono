//
// System.Drawing.Image.cs
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
// Author:	Christian Meyer <Christian.Meyer@cs.tum.edu>
//			Jason Perkins <jason@379.com>
//			Dennis Hayes <dennish@raytek.com>
//			Alexandre Pigolkine <pigolkine@gmx.de>

using System;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace System.Drawing {

	namespace XrImpl {
		//[Serializable]
		//[ComVisible(true)]

		internal abstract class Image : MarshalByRefObject, IImage /*, ICloneable, ISerializable */ {

			internal IntPtr nativeObject_;
			internal Xr.XrFormat xrFormat_;
			internal System.Drawing.XrImpl.Graphics selectedIntoGraphics_ = null;
			internal Size  size;
			internal PixelFormat format;
			
			// constructor
			public Image () {}

			[MonoTODO]
			public virtual object Clone() {
				throw new NotImplementedException();
			}
    
			// public methods
			// static
			public static Image FromFile (string filename) {
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			public static Image FromFile (string filename, bool useEmbeddedColorManagement) {
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			public static Bitmap FromHbitmap (IntPtr hbitmap) {
				// Fixme: implement me
				throw new NotImplementedException ();
			}

			public static Bitmap FromHbitmap (IntPtr hbitmap, IntPtr hpalette) {
				// Fixme: implement me
				throw new NotImplementedException ();
			}
		
			[MonoTODO]
			public static Image FromStream(Stream stream) {
				throw new NotImplementedException();
			}
		
			[MonoTODO]
			public static Image FromStream(Stream stream, bool useIcm) {
				throw new NotImplementedException();
			}
			
			public static int GetPixelFormatSize (PixelFormat pixfmt) {
				// Fixme: implement me
				throw new NotImplementedException ();
			}
		
			public static bool IsAlphaPixelFormat (PixelFormat pixfmt) {
				// Fixme: implement me
				throw new NotImplementedException ();
			}
			
			public static bool IsCanonicalPixelFormat (PixelFormat pixfmt) {
				// Fixme: implement me
				throw new NotImplementedException ();
			}
			
			public static bool IsExtendedPixelFormat (PixelFormat pixfmt) {
				// Fixme: implement me
				throw new NotImplementedException ();
			}

			// non-static
			public RectangleF GetBounds (ref GraphicsUnit pageUnit) {
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			//		public EncoderParameters GetEncoderParameterList(Guid encoder)
			//		{
			//			throw new NotImplementedException();
			//		}
			[MonoTODO]
			public int GetFrameCount(FrameDimension dimension) {
				throw new NotImplementedException();
			}
		
			[MonoTODO]
			public PropertyItem GetPropertyItem(int propid) {
				throw new NotImplementedException();
			}

			//		public Image GetThumbnailImage(int thumbWidth, int thumbHeight, Image.GetThumbnailImageAbort callback, IntPtr callbackData)
			//		{
			//			throw new NotImplementedException();
			//		}

			public void RemovePropertyItem (int propid) {
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			public void RotateFlip (RotateFlipType rotateFlipType) {
				// Fixme: implement me
				throw new NotImplementedException ();
			}

			#region 
			struct BITMAPFILEHEADER {        // File info header
				public ushort bfType;      // Specifies the type of file. This member must be BM.
				public uint bfSize;      // Specifies the size of the file, in bytes.
				public ushort bfReserved1; // Reserved; must be set to zero.
				public ushort bfReserved2; // Reserved; must be set to zero.
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
			const int BI_RGB = 0;                   //? 0 is from example;
			#endregion

			public void Save (string filename) {
				FileStream fs = new FileStream(filename, FileMode.Create);
				
				int rowSize = (format == PixelFormat.Format32bppArgb) ? 4 * size.Width: 3 * size.Width;
				int totalSize = rowSize * size.Height;
				
				// store bitmap header
				bitmapstruct bitmap = new bitmapstruct();
				
				// Init BITMAPFILEHANDLE
				// document I am working from says tyoe must allways be "BM",
				// the example has this set to 19778.
				// TODO: verify magic number 19778 for "BM" bfType
				bitmap.fileheader.bfType = 19778;
				// TODO: is this the correct file size?
				bitmap.fileheader.bfSize = (uint)
					//bitmap
					totalSize
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

				bitmap.info.bitmapinfoheader.biBitCount = (format == PixelFormat.Format32bppArgb) ? (ushort)32: (ushort)24;
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
				bitmap.info.bitmapinfoheader.biHeight = size.Height;
				bitmap.info.bitmapinfoheader.biWidth = size.Width;
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
				
				BinaryWriter bw = new BinaryWriter(fs);
				bw.Write(bitmap.fileheader.bfType);
				bw.Write(bitmap.fileheader.bfSize);
				bw.Write(bitmap.fileheader.bfReserved1);
				bw.Write(bitmap.fileheader.bfReserved2);
				bw.Write(bitmap.fileheader.bfOffBits);

				bw.Write(bitmap.info.bitmapinfoheader.biSize);
				bw.Write(bitmap.info.bitmapinfoheader.biWidth);
				bw.Write(bitmap.info.bitmapinfoheader.biHeight);
				bw.Write(bitmap.info.bitmapinfoheader.biPlanes);
				bw.Write(bitmap.info.bitmapinfoheader.biBitCount);
				bw.Write(bitmap.info.bitmapinfoheader.biCompression);
				bw.Write(bitmap.info.bitmapinfoheader.biSizeImage);
				bw.Write(bitmap.info.bitmapinfoheader.biXPelsPerMeter);
				bw.Write(bitmap.info.bitmapinfoheader.biYPelsPerMeter);
				bw.Write(bitmap.info.bitmapinfoheader.biClrUsed);
				bw.Write(bitmap.info.bitmapinfoheader.biClrImportant);
				
				// store bitmap bytes
				IntPtr memptr = GDK.gdk_pixbuf_get_pixels(nativeObject_);
				byte[] buffer = new byte[totalSize];
				Marshal.Copy( memptr, buffer, 0, totalSize);
				fs.Write(buffer, 0, totalSize);
				
				fs.Flush();
				fs.Close();				
			}
	
			[MonoTODO]
			public void Save(Stream stream, ImageFormat format) {
				throw new NotImplementedException();
			}
		
			[MonoTODO]
			public void Save(string filename, ImageFormat format) {
				throw new NotImplementedException();
			}
		
			//		public void Save(Stream stream, ImageCodecInfo encoder, EncoderParameters encoderParams)
			//		{
			//			throw new NotImplementedException();
			//		}
			//		public void Save(string filename, ImageCodecInfo encoder, EncoderParameters encoderParams)
			//		{
			//			throw new NotImplementedException();
			//		}
			//		public void SaveAdd(EncoderParameters encoderParams)
			//		{
			//			throw new NotImplementedException();
			//		}
		
			//		public void SaveAdd(Image image, EncoderParameters encoderParams)
			//		{
			//			throw new NotImplementedException();
			//		}
			[MonoTODO]
			public int SelectActiveFrame(FrameDimension dimension, int frameIndex) {
				throw new NotImplementedException();
			}
	
			[MonoTODO]
			public void SetPropertyItem(PropertyItem item) {
				throw new NotImplementedException();
			}
			// destructor
			~Image() {}

			// properties
			public int Flags {
				get {
					throw new NotImplementedException ();
				}
			}
	
			public Guid[] FrameDimensionsList {
				get {
					throw new NotImplementedException ();
				}
			}
	
			public int Height {
				get {
					return size.Height;
				}
			}
	
			public float HorizontalResolution {
				get {
					throw new NotImplementedException ();
				}
			}
	
			public ColorPalette Palette {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}
	
			public SizeF PhysicalDimension {
				get {
					throw new NotImplementedException ();
				}
			}
	
			public PixelFormat PixelFormat {
				get {
					return format;
				}
			}
	
			public int[] PropertyIdList {
				get {
					throw new NotImplementedException ();
				}
			}
	
			[MonoTODO]
			public PropertyItem[] PropertyItems {
				get {
					throw new NotImplementedException();
				}
			}

			[MonoTODO]
			public ImageFormat RawFormat {
				get {
					throw new NotImplementedException();
				}
			}
			public Size Size {
				get {
					return size;
				}
			}
	
			public float VerticalResolution {
				get {
					throw new NotImplementedException ();
				}
			}
	
			public int Width {
				get {
					return size.Width;
				}
			}
			[MonoTODO]
			public void Dispose () {
			}

			[MonoTODO]
			protected virtual void Dispose(bool disposing) {
				throw new NotImplementedException();
			}

		}
	}
}
