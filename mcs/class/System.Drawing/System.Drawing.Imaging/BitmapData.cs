//
// System.Drawing.Imaging.BitmapData.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Vladimir Vukicevic (vladimir@pobox.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.InteropServices;
using System.IO;

namespace System.Drawing.Imaging
{
	// MUST BE KEPT IN SYNC WITH gdip.h in libgdiplus!
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BitmapData {
		internal int width;
		internal int height;
		internal int stride;
		internal PixelFormat pixel_format; // int
		internal IntPtr address;
		internal int reserved;
		
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
