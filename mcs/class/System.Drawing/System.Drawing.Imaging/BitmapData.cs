//
// System.Drawing.Imaging.BitmapData.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Drawing.Imaging
{
	public sealed class BitmapData {
		int width, height, stride, reserved;
		PixelFormat pixel_format;
		IntPtr address;
		
		public int Height {
			get {
				return height;
			}

			set {
				height = value;
			}
		}

		public int Width {
			get {
				return width;
			}

			set {
				width = value;
			}
		}

		public PixelFormat PixelFormat {
			get {
				return pixel_format;
			}

			set {
				pixel_format = value;
			}
		}

		public int Reserved {
			get {
				return reserved;
			}

			set {
				reserved = value;
			}
		}

		public IntPtr Scan0 {
			get {
				return address;
			}

			set {
				address = value;
			}
		}

		public int Stride {
			get {
				return stride;
			}

			set {
				stride = value;
			}
		}
	}
}
