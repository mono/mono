//
// System.Drawing.Win32.Bitmap.cs
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
// Alexandre Pigolkine (pigolkine@gmx.de)
//
//
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace System.Drawing {
	namespace Win32Impl {

		internal class BitmapFactory : IBitmapFactory
		{
			public IBitmap Bitmap(int width, int height)
			{
				return new Bitmap(width, height);
			}

			public IBitmap Bitmap(int width, int height, System.Drawing.Graphics g)
			{
				return new Bitmap(width, height, (Graphics)g.implementation_);
			}

			public IBitmap Bitmap(int width, int height, System.Drawing.Imaging.PixelFormat format) {
				return new Bitmap(width, height, format);
			}

			public IBitmap Bitmap(System.Drawing.Image original, Size newSize){
				return new Bitmap((System.Drawing.Win32Impl.Image)original.implementation_, newSize);
			}

			public IBitmap Bitmap(Stream stream, bool useIcm){
				return new Bitmap(stream, useIcm);
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

		internal sealed class Bitmap : Image, IBitmap 
		{
			#region constructors
			// constructors
			public Bitmap (int width, int height) : this( width, height, PixelFormat.Format32bppArgb)
			{
			}

			public Bitmap (int width, int height, Graphics g) 
			{
				IntPtr hdc = g.GetHdc();
				nativeObject_ = Win32.CreateCompatibleBitmap(hdc, width, height);
				imageSize_ = new Size(width, height);
				g.ReleaseHdc(hdc);
			}

			public Bitmap (int width, int height, System.Drawing.Imaging.PixelFormat format) {
				IntPtr hdc = Win32.GetDC(IntPtr.Zero);
				pixelFormat_ = format;
				BITMAPINFO_FLAT bmi = new BITMAPINFO_FLAT();
				bmi.bmiHeader_biSize = 40;
				bmi.bmiHeader_biWidth = width;
				bmi.bmiHeader_biHeight = height;
				bmi.bmiHeader_biPlanes = 1;
				bmi.bmiHeader_biBitCount = (short)System.Drawing.Image.GetPixelFormatSize(pixelFormat_);
				bmi.bmiHeader_biCompression = 0;
				bmi.bmiHeader_biSizeImage = 0;
				bmi.bmiHeader_biXPelsPerMeter = 0;
				bmi.bmiHeader_biYPelsPerMeter = 0;
				bmi.bmiHeader_biClrUsed = 0;
				bmi.bmiHeader_biClrImportant = 0;
				IntPtr bitsPtr;
				nativeObject_ = Win32.CreateDIBSection(hdc, ref bmi, DibUsage.DIB_RGB_COLORS,
								out bitsPtr, IntPtr.Zero, 0);
				imageSize_ = new Size(width, height);
				Win32.ReleaseDC( IntPtr.Zero, hdc);
			}
			
			public Bitmap (Image origial) {
				throw new NotImplementedException ();
			}

			public Bitmap (Stream stream) 
			{
				throw new NotImplementedException ();
				//this.stream = stream;
			}

			public Bitmap (string filename) : this(filename, false)
			{
			}

			public Bitmap (Image original, Size newSize) 
			{
				throw new NotImplementedException ();
				//this.original = original;
				//this.newSize = newSize;
			}

			void InitFromStream( Stream stream) {
				InternalImageInfo info = System.Drawing.Image.Decode(stream);
				if (info != null) {
					createdFrom_ = info;
					IntPtr memDC = Win32.CreateCompatibleDC(IntPtr.Zero);
					IntPtr dibBits;
					BITMAPINFO_FLAT bmi = new BITMAPINFO_FLAT();
					bmi.bmiHeader_biSize = 40;
					bmi.bmiHeader_biWidth = info.Size.Width;
					bmi.bmiHeader_biHeight = info.Size.Height;
					bmi.bmiHeader_biPlanes = 1;
					bmi.bmiHeader_biBitCount = (short)System.Drawing.Image.GetPixelFormatSize(info.Format);
					bmi.bmiHeader_biCompression = (int)BitmapCompression.BI_RGB;
					bmi.bmiHeader_biSizeImage = (int)info.RawImageBytes.Length;
					bmi.bmiHeader_biXPelsPerMeter = 0;
					bmi.bmiHeader_biYPelsPerMeter = 0;
					bmi.bmiHeader_biClrUsed = 0;
					bmi.bmiHeader_biClrImportant = 0;
					int palIdx = 0;
					foreach( Color col in info.Palette.Entries) {
						bmi.bmiColors[palIdx++] = col.B;
						bmi.bmiColors[palIdx++] = col.G;
						bmi.bmiColors[palIdx++] = col.R;
						bmi.bmiColors[palIdx++] = col.A;
					}

					//byte[] bmpInfoBytes = CreateBITMAPINFOArray( info);
					nativeObject_ = Win32.CreateDIBSection(memDC, ref bmi, DibUsage.DIB_RGB_COLORS, out dibBits, IntPtr.Zero, 0);
					if (nativeObject_ == IntPtr.Zero) {
						Console.WriteLine("Error creating Win32 DIBSection {0}", Win32.FormatMessage(Win32.GetLastError()));
					}
					Marshal.Copy(info.RawImageBytes, 0, dibBits, info.RawImageBytes.Length);
					Win32.DeleteDC(memDC);
					imageSize_ = info.Size;
					imageFormat_ = info.RawFormat;
					pixelFormat_ =  info.Format;
				}
			}
			
			public Bitmap (Stream stream, bool useIcm) 
			{
				InitFromStream(stream);
			}

			public Bitmap (string filename, bool useIcm) 
			{
				FileStream file = new FileStream(filename, FileMode.Open);
				InitFromStream(file);
				file.Close();
			}

			public Bitmap (Type type, string resource) 
			{
				throw new NotImplementedException ();
				//this.type = type;
				//this.resource = resource;
			}

			public Bitmap (Image original, int width, int heigth) 
			{
				throw new NotImplementedException ();
				//this.original = original;
				//this.width = width;
				//this.heigth = heigth;
			}


			public Bitmap (int width, int height, int stride,
						       System.Drawing.Imaging.PixelFormat format, IntPtr scan0) {
						throw new NotImplementedException ();
			//			//this.width = width;
			//			//this.heigth = heigth;
			//			//this.stride = stride;
			//			//this.format = format;
			//			//this.scan0 = scan0;
			}
			#endregion
			// methods
			Color IBitmap.GetPixel (int x, int y) 
			{
				throw new NotImplementedException();
			}

			void IBitmap.SetPixel (int x, int y, Color color) 
			{
			}

			public IBitmap Clone (Rectangle rect, System.Drawing.Imaging.PixelFormat format) {
				throw new NotImplementedException ();
			}
					
			public IBitmap Clone (RectangleF rect, System.Drawing.Imaging.PixelFormat format) {
				throw new NotImplementedException ();
			}

			public static Bitmap FromHicon (IntPtr hicon) 
			{
				throw new NotImplementedException ();
			}

			public static Bitmap FromResource (IntPtr hinstance,
				string bitmapName) 
			{
				throw new NotImplementedException ();
			}

			IntPtr IBitmap.GetHbitmap () 
			{
				throw new NotImplementedException ();
			}

			IntPtr IBitmap.GetHbitmap (Color background) 
			{
				throw new NotImplementedException ();
			}

			IntPtr IBitmap.GetHicon () 
			{
				throw new NotImplementedException ();
			}

			public System.Drawing.Imaging.BitmapData LockBits (Rectangle rect, System.Drawing.Imaging.ImageLockMode flags,
					                            System.Drawing.Imaging.PixelFormat format) {
				throw new NotImplementedException ();
			}

			void IBitmap.MakeTransparent () 
			{
				throw new NotImplementedException ();
			}

			void IBitmap.MakeTransparent (Color transparentColor) 
			{
				throw new NotImplementedException ();
			}

			void IBitmap.SetResolution (float xDpi, float yDpi) 
			{
				throw new NotImplementedException ();
			}

			void IDisposable.Dispose() {
				Win32.DeleteObject(nativeObject_);
			}

			public void UnlockBits (System.Drawing.Imaging.BitmapData bitmapdata) {
				throw new NotImplementedException ();
			}

			// properties
			// needs to be done ###FIXME###
		}
	}
}
