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

	[Serializable]
	public sealed class Bitmap : Image {

		internal static IBitmapFactory	factory = Factories.GetBitmapFactory();

		#region constructors
		// constructors
		public Bitmap (int width, int height) {
			implementation = factory.Bitmap(width, height);
			image_size = new Size(width, height);
		}

		public Bitmap (int width, int height, Graphics g) {
			implementation = factory.Bitmap(width, height, g);
			image_size = new Size(width, height);
		}

		public Bitmap (int width, int height, PixelFormat format) {
			implementation = factory.Bitmap(width, height, format);
			image_size = new Size(width, height);
		}

		public Bitmap (Image original) {
			implementation = factory.Bitmap(original, original.Size);
			image_size = original.Size;
		}

		public Bitmap (Stream stream) {
			implementation = factory.Bitmap(stream, false);
			image_size = implementation.Size;
		}

		public Bitmap (string filename) {
			implementation = factory.Bitmap(filename, false);
			image_size = implementation.Size;
		}

		public Bitmap (Image original, Size newSize) {
			implementation = factory.Bitmap(original, newSize);
			image_size = newSize;
		}

		public Bitmap (Stream stream, bool useIcm) {
			implementation = factory.Bitmap(stream, useIcm);
			image_size = implementation.Size;
		}

		public Bitmap (string filename, bool useIcm) {
			implementation = factory.Bitmap(filename, useIcm);
			image_size = implementation.Size;
		}

		public Bitmap (Type type, string resource) {
			implementation = factory.Bitmap(type, resource);
			image_size = implementation.Size;
		}

		public Bitmap (Image original, int width, int heigth) {
			implementation = factory.Bitmap(original, new Size(width, heigth));
			image_size = implementation.Size;
		}


		public Bitmap (int width, int height, int stride,
			       PixelFormat format, IntPtr scan0) {
			implementation = factory.Bitmap(width, height, stride, format, scan0);
			image_size = implementation.Size;
		}

        	private Bitmap (SerializationInfo info, StreamingContext context)
		{
		}

		#endregion
		// methods
		public Color GetPixel (int x, int y) {
			return ((IBitmap)implementation).GetPixel(x, y);
		}

		public void SetPixel (int x, int y, Color color) {
			((IBitmap)implementation).SetPixel(x, y, color);
		}

		public Bitmap Clone (Rectangle rect,PixelFormat format) {
			Bitmap result = new Bitmap(1, 1);
			result.implementation = ((IBitmap)implementation).Clone(rect, format);
			result.image_size = result.implementation.Size;
			return result;
		}
		
		public Bitmap Clone (RectangleF rect, PixelFormat format) {
			Bitmap result = new Bitmap(1, 1);
			result.implementation = ((IBitmap)implementation).Clone(rect, format);
			result.image_size = result.implementation.Size;
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
			return ((IBitmap)implementation).GetHbitmap();
		}

		public IntPtr GetHbitmap (Color background) {
			return ((IBitmap)implementation).GetHbitmap(background);
		}

		public IntPtr GetHicon () {
			return ((IBitmap)implementation).GetHicon();
		}

		public BitmapData LockBits (Rectangle rect, ImageLockMode flags,
		                            PixelFormat format) {
			return ((IBitmap)implementation).LockBits(rect, flags, format);
		}

		public void MakeTransparent () {
			((IBitmap)implementation).MakeTransparent();
		}

		public void MakeTransparent (Color transparentColor) {
			((IBitmap)implementation).MakeTransparent(transparentColor);
		}

		public void SetResolution (float xDpi, float yDpi) {
			((IBitmap)implementation).SetResolution(xDpi,yDpi );
		}

		public void UnlockBits (BitmapData bitmapdata) {
			((IBitmap)implementation).UnlockBits(bitmapdata);
		}

		// properties
		// needs to be done ###FIXME###
	}
}
