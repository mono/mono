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
				throw new NotImplementedException();
			}

			public IBitmap Bitmap(System.Drawing.Image original, Size newSize){
				throw new NotImplementedException();
			}

			public IBitmap Bitmap(Stream stream, bool useIcm){
				throw new NotImplementedException();
			}

			public IBitmap Bitmap(string filename, bool useIcm){
				throw new NotImplementedException();
			}

			public IBitmap Bitmap(Type type, string resource){
				throw new NotImplementedException();
			}

			public IBitmap Bitmap(int width, int height, int stride, System.Drawing.Imaging.PixelFormat format, IntPtr scan0){
				throw new NotImplementedException();
			}
		}

		internal sealed class Bitmap : Image, IBitmap 
		{
			#region constructors
			// constructors
			public Bitmap (int width, int height) 
			{
			}

			public Bitmap (int width, int height, Graphics g) 
			{
				IntPtr hdc = g.GetHdc();
				nativeObject_ = Win32.CreateCompatibleBitmap(hdc, width, height);
				imageSize_ = new Size(width, height);
				g.ReleaseHdc(hdc);
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

			public Bitmap (Stream stream) 
			{
				throw new NotImplementedException ();
				//this.stream = stream;
			}

			public Bitmap (string filename) 
			{
				throw new NotImplementedException ();
				//this.filename = filename;
			}

			public Bitmap (Image original, Size newSize) 
			{
				throw new NotImplementedException ();
				//this.original = original;
				//this.newSize = newSize;
			}

			public Bitmap (Stream stream, bool useIcm) 
			{
				throw new NotImplementedException ();
				//this.stream = stream;
				//this.useIcm = useIcm;
			}

			public Bitmap (string filename, bool useIcm) 
			{
				throw new NotImplementedException ();
				//this.filename = filename;
				//this.useIcm = useIcm;
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
