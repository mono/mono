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
using System.IO;

namespace System.Drawing.Imaging
{
	// MUST BE KEPT IN SYNC WITH gdip.h in libgdiplus!
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BitmapData {
		internal int width, height, stride;
		internal PixelFormat pixel_format;		
		internal IntPtr address;
		internal int reserved;
		internal bool own_scan0;
		
		~BitmapData()
		{
			if (address != IntPtr.Zero && own_scan0) {
				GDIPlus.GdipFree (address);
				address = IntPtr.Zero;
			}
		}
		
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
				if (address == value)
					return;

				// FIXME -- do we really want to prevent the
				// user from shooting themselves in the foot,
				// if they want to?
				if (address != IntPtr.Zero && own_scan0) {
					GDIPlus.GdipFree (address);
					address = IntPtr.Zero;
				}

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
