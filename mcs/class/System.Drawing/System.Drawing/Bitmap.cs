// created on 25.02.2002 at 22:43
//
// Bitmap.cs
//
// Author: Christian Meyer
// eMail: Christian.Meyer@cs.tum.edu
//
using System;
using System.Drawing;

namespace System.Drawing {

	public sealed class Bitmap : Image {

		// constructors
		public Bitmap (Image origial) {
			this.original = original;
		}

		public Bitmap (Stream stream) {
			this.stream = stream;
		}

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

		public Bitmap (Stream stream, bool useIcm) {
			this.stream = stream;
			this.useIcm = useIcm;
		}

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

		public Bitmap (int width, int heigth, PixelFormat format) {
		}

		public Bitmap (int width, int height, int stride,
		               PixelFormat format, IntPtr scan0) {
			this.width = width;
			this.heigth = heigth;
			this.stride = stride;
			this.format = format;
			this.scan0 = scan0;
		}

		// properties
		// methods
	}
}
