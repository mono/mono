//
// System.Drawing.Imaging.BitmapData.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.InteropServices;

namespace System.Drawing.Imaging
{
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BitmapData {
		int width, height, stride;
		PixelFormat pixel_format;
		IntPtr address;
		int reserved;
		
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
