//
// System.Drawing.Image.cs
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
// Authors:
//   Christian Meyer <Christian.Meyer@cs.tum.edu>
//   Jason Perkins <jason@379.com>
//   Dennis Hayes <dennish@raytek.com>
//   Alexandre Pigolkine <pigolkine@gmx.de>

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

			internal IntPtr native_object;
			internal Xr.Format xr_format;
			internal System.Drawing.XrImpl.Graphics selected_into_graphics = null;
			internal Size size;
			internal PixelFormat pixelFormat;
			protected ImageFormat imageFormat;
			
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

			protected InternalImageInfo sourceImageInfo = null;
			public unsafe InternalImageInfo ConvertToInternalImageInfo() {
				if (sourceImageInfo == null) {
					sourceImageInfo = new InternalImageInfo();
					sourceImageInfo.Size = size;
					sourceImageInfo.RawFormat = imageFormat;
					sourceImageInfo.PixelFormat = PixelFormat.Format32bppArgb;
					sourceImageInfo.Stride = GDK.gdk_pixbuf_get_rowstride (native_object);
					sourceImageInfo.RawImageBytes = new byte[sourceImageInfo.Stride * size.Height];
					IntPtr memptr = GDK.gdk_pixbuf_get_pixels (native_object);
					Marshal.Copy( memptr, sourceImageInfo.RawImageBytes, 0, sourceImageInfo.RawImageBytes.Length);
					sourceImageInfo.ChangePixelFormat (pixelFormat);
				}
				return sourceImageInfo;
			}
			public void Save (string filename) {
				throw new NotImplementedException();
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
					return pixelFormat;
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
					return imageFormat;
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
