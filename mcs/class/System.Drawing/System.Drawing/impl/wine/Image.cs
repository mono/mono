//
// System.Drawing.Image.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
// Alexandre Pigolkine(pigolkine@gmx.de)
//
// Many methods are still commented. I'll care about them when all necessary
// classes are implemented.
//
namespace System.Drawing {

	namespace Win32Impl {

		using System;
		using System.Runtime.Remoting;
		using System.Runtime.Serialization;
		using System.Drawing.Imaging;
		using System.IO;
		using System.Runtime.InteropServices;

		//[Serializable]
		//[ComVisible(true)]

		internal abstract class Image : MarshalByRefObject, IImage /*, ICloneable, ISerializable */ 
		{

			internal IntPtr nativeObject_;
			internal System.Drawing.Win32Impl.Graphics selectedIntoGraphics_ = null;

			protected Size imageSize_;
			protected ImageFormat imageFormat_;
			protected PixelFormat pixelFormat_;
			// constructor
			public Image () {}
    
			// public methods
			// static
			public static Image FromFile (string filename)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			public static Image FromFile (string filename, bool useEmbeddedColorManagement)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			public static Bitmap FromHbitmap (IntPtr hbitmap)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}

			public static Bitmap FromHbitmap (IntPtr hbitmap, IntPtr hpalette)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			//	public static int GetPixelFormatSize (PixelFormat pixfmt)
			//	{
			//		// Fixme: implement me
			//		throw new NotImplementedException ();
			//	}
			//
			//	public static bool IsAlphaPixelFormat (PixelFormat pixfmt)
			//	{
			//		// Fixme: implement me
			//		throw new NotImplementedException ();
			//	}
			//	
			//	public static bool IsCanonicalPixelFormat (PixelFormat pixfmt)
			//	{
			//		// Fixme: implement me
			//		throw new NotImplementedException ();
			//	}
			//	
			//	public static bool IsExtendedPixelFormat (PixelFormat pixfmt)
			//    	{
			//		// Fixme: implement me
			//		throw new NotImplementedException ();
			//	}

			// non-static
			public RectangleF GetBounds (ref GraphicsUnit pageUnit)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			//public EncoderParameters GetEncoderParameterList(Guid encoder);
			//public int GetFrameCount(FrameDimension dimension);
			//public PropertyItem GetPropertyItem(int propid);
			/*
			  public Image GetThumbnailImage(int thumbWidth, int thumbHeight,
			  Image.GetThumbnailImageAbort callback,
			  IntPtr callbackData);
			*/
	
			public void RemovePropertyItem (int propid)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
	
			public void RotateFlip (RotateFlipType rotateFlipType)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}

			protected InternalImageInfo createdFrom_ = null;
			public InternalImageInfo ConvertToInternalImageInfo() {
				InternalImageInfo result = new InternalImageInfo();
				IntPtr hTempBitmap = IntPtr.Zero;
				IntPtr hdc = IntPtr.Zero;
				result.Size = imageSize_;
				result.Format = pixelFormat_;
				//result.Stride = 
				if(selectedIntoGraphics_ != null) {
					hdc = selectedIntoGraphics_.GetHdc();
					hTempBitmap = Win32.CreateCompatibleBitmap(hdc,1,1);
					Win32.SelectObject( hdc, hTempBitmap);
				}
				else {
					hdc = Win32.GetDC(IntPtr.Zero);
				}
				//nativeObject_;Win32.GetDIBits();
				BITMAPINFO_FLAT bmi = new BITMAPINFO_FLAT();
				bmi.bmiHeader_biSize = 40;
				bmi.bmiHeader_biWidth = imageSize_.Width;
				bmi.bmiHeader_biHeight = imageSize_.Height;
				bmi.bmiHeader_biPlanes = 1;
				bmi.bmiHeader_biBitCount = (short) System.Drawing.Image.GetPixelFormatSize(pixelFormat_);
				bmi.bmiHeader_biCompression = 0;
				bmi.bmiHeader_biSizeImage = 0;
				bmi.bmiHeader_biXPelsPerMeter = 0;
				bmi.bmiHeader_biYPelsPerMeter = 0;
				bmi.bmiHeader_biClrUsed = 0;
				bmi.bmiHeader_biClrImportant = 0;

				int res = Win32.GetDIBits(hdc, nativeObject_, 0, imageSize_.Height, 0, ref bmi, (int)DibUsage.DIB_RGB_COLORS);
				if( res != 0) {
					IntPtr nativeBits = Marshal.AllocHGlobal(bmi.bmiHeader_biSizeImage);
					res = Win32.GetDIBits(hdc, nativeObject_, 0, imageSize_.Height, nativeBits.ToInt32(), ref bmi, (int)DibUsage.DIB_RGB_COLORS);
					result.RawImageBytes = new byte[bmi.bmiHeader_biSizeImage];
					Marshal.Copy( nativeBits, result.RawImageBytes, 0, bmi.bmiHeader_biSizeImage);
					Marshal.FreeHGlobal(nativeBits);
				}
				else {
					uint err = Win32.GetLastError();
				}
				
				if(selectedIntoGraphics_ != null) {
					Win32.SelectObject( hdc, nativeObject_);
					Win32.DeleteObject(hTempBitmap);
					selectedIntoGraphics_.ReleaseHdc(hdc);
				}
				else {
					Win32.ReleaseDC(IntPtr.Zero,hdc);
				}
				return result;
			}

			public void Save (string filename)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}

			public void Save(Stream stream, ImageFormat format)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
			
			public void Save(string filename, ImageFormat format)
			{
				// Fixme: implement me
				throw new NotImplementedException ();
			}
			
			//public void Save(Stream stream, ImageCodecInfo encoder,
			//                 EncoderParameters encoderParams);
			//public void Save(string filename, ImageCodecInfo encoder,
			//                 EncoderParameters encoderParams);
			//public void SaveAdd(EncoderParameters_ encoderParams);
			//public void SaveAdd(Image image, EncoderParameters_ encoderParams);
			//public int SelectActiveFrame(FrameDimension dimension, int frameIndex);
			//public void SetPropertyItem(PropertyItem propitem);

			// destructor
			~Image() {}

			// properties
			public int Flags 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			public Guid[] FrameDimensionsList 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			public int Height 
			{
				get 
				{
					return imageSize_.Height;
				}
			}
	
			public float HorizontalResolution 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			//	public ColorPalette Palette {
			//		get {
			//			throw new NotImplementedException ();
			//		}
			//		set {
			//			throw new NotImplementedException ();
			//		}
			//	}
	
			public SizeF PhysicalDimension 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			public PixelFormat PixelFormat {
				get {
					throw new NotImplementedException ();
				}
			}
	
			public int[] PropertyIdList 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			public PropertyItem[] PropertyItems {
				get {
					throw new NotImplementedException();
				}
			}

			public ImageFormat RawFormat {
				get{
					return imageFormat_;
				}
			}

			public Size Size 
			{
				get 
				{
					return imageSize_;
				}
			}
	
			public float VerticalResolution 
			{
				get 
				{
					throw new NotImplementedException ();
				}
			}
	
			public int Width 
			{
				get 
				{
					return imageSize_.Width;
				}
			}
			[MonoTODO]
			public void Dispose ()
			{
				throw new NotImplementedException ();
			}

		}
	}
}
