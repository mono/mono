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
namespace System.Drawing.Win32Impl {

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

		public delegate bool GetThumbnailImageAbort ();
		
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
		//	{
		//		// Fixme: implement me
		//		throw new NotImplementedException ();
		//	}

		// non-static
		RectangleF IImage.GetBounds (ref GraphicsUnit pageUnit)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}
	
//		   EncoderParameters IImage.GetEncoderParameterList (Guid encoder)
//		   {
//			   throw new NotImplementedException ();
//		   }

		int IImage.GetFrameCount (FrameDimension dimension)
		{
			throw new NotImplementedException ();
		}
			
		PropertyItem IImage.GetPropertyItem (int propid)
		{
			throw new NotImplementedException ();
		}


		int IImage.SelectActiveFrame (FrameDimension dimension, int frameIndex)
		{
			throw new NotImplementedException ();
		}
		
//		   Image IImage.GetThumbnailImage (
//			   int thumbWidth, int thumbHeight,
//			   System.Drawing.Image.GetThumbnailImageAbort callback,
//			   IntPtr callbackData)
//		   {
//			   throw new NotImplementedException ();
//		   }
	
		void IImage.RemovePropertyItem (int propid)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}

		void IImage.SetPropertyItem (PropertyItem item)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}		 
	
		void IImage.RotateFlip (RotateFlipType rotateFlipType)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}

		protected InternalImageInfo sourceImageInfo = null;

		InternalImageInfo IImage.ConvertToInternalImageInfo()
		{
			InternalImageInfo result = new InternalImageInfo();
			IntPtr hTempBitmap = IntPtr.Zero;
			IntPtr hdc = IntPtr.Zero;
			result.Size = imageSize_;
			result.PixelFormat = pixelFormat_;
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

		void IImage.Save (string filename)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}

		void IImage.Save(Stream stream, ImageFormat format)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}
			
		void IImage.Save(string filename, ImageFormat format)
		{
			// Fixme: implement me
			throw new NotImplementedException ();
		}
			
		// destructor
		~Image() {}

		// properties
		int IImage.Flags {
			get  {
				throw new NotImplementedException ();
			}
		}
	
		Guid [] IImage.FrameDimensionsList  {
			get  {
				throw new NotImplementedException ();
			}
		}
	
		int IImage.Height {
			get {
				return imageSize_.Height;
			}
		}
	
		float IImage.HorizontalResolution {
			get  {
				throw new NotImplementedException ();
			}
		}
	
		ColorPalette IImage.Palette {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}
		
		SizeF IImage.PhysicalDimension 
		{
			get 
			{
				throw new NotImplementedException ();
			}
		}
	
		PixelFormat IImage.PixelFormat {
			get {
				throw new NotImplementedException ();
			}
		}
	
		int [] IImage.PropertyIdList 
		{
			get 
			{
				throw new NotImplementedException ();
			}
		}
	
		PropertyItem [] IImage.PropertyItems {
			get {
				throw new NotImplementedException();
			}
		}

		ImageFormat IImage.RawFormat {
			get{
				return imageFormat_;
			}
		}

		Size IImage.Size {
			get {
				return imageSize_;
			}
		}
	
		float IImage.VerticalResolution {
			get {
				throw new NotImplementedException ();
			}
		}
	
		public int Width {
			get {
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

