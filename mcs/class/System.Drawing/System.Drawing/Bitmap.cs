// created on 25.02.2002 at 22:43
//
// Bitmap.cs
//
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
//
// No implementation has been done yet. I first want to write the method
// headers of every System.Drawing.
//
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace System.Drawing {

	public sealed class Bitmap : Image {

		// constructors
		public Bitmap (Image origial) {
			this.original = original;
		}

/*		public Bitmap (Stream stream) {
			this.stream = stream;
		}
*/
		public Bitmap (string filename) {
			this.filename = filename;
		}

		public Bitmap (Image original, Size newSize) {
			this.original = original;
			this.newSize = newSize;
		}

		public Bitmap (int width, int heigth) {
			this.width = width;
			this.heigth = heigth;
		}

/*		public Bitmap (Stream stream, bool useIcm) {
			this.stream = stream;
			this.useIcm = useIcm;
		} 
*/
		public Bitmap (string filename, bool useIcm) {
			this.filename = filename;
			this.useIcm = useIcm;
		}

		public Bitmap (Type type, string resource) {
			this.type = type;
			this.resource = resource;
		}

		public Bitmap (Image original, int width, int heigth) {
			this.original = original;
			this.width = width;
			this.heigth = heigth;
		}

		public Bitmap (int width, int heigth, Graphics g) {
			this.width = width;
			this.heigth = heigth;
			this.g = g;
		}
/*
		public Bitmap (int width, int heigth, PixelFormat format) {
			this.width = width;
			this.heigth = heigth;
			this.format = format;
		}

		public Bitmap (int width, int height, int stride,
		               PixelFormat format, IntPtr scan0) {
			this.width = width;
			this.heigth = heigth;
			this.stride = stride;
			this.format = format;
			this.scan0 = scan0;
		}

		// methods
		public Bitmap Clone (Rectangle rect,PixelFormat format) {
			throw new NotImplementedException ();
		}
		
		public Bitmap Clone (RectangleF rect, PixelFormat format) {
			throw new NotImplementedException ();
		}
*/
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
		
		public Color GetPixel (int x, int y) {
			throw new NotImplementedException ();
		}
		
/*		public BitmapData LockBits (Rectangle rect, ImageLockMode flags,
		                            PixelFormat format) {
			throw new NotImplementedException ();
		} */
		
		public void MakeTransparent () {
			throw new NotImplementedException ();
		}
		
		public void MakeTransparent (Color transparentColor) {
			throw new NotImplementedException ();
		}
		
		public void SetPixel (int x, int y, Color color) {
			throw new NotImplementedException ();
		}
		
		public void SetResolution (float xDpi, float yDpi) {
			throw new NotImplementedException ();
		}
		
/*		public void UnlockBits (BitmapData bitmapdata) {
			throw new NotImplementedException ();
		} */
		
		// properties
		// needs to be done ###FIXME###
	}
}
