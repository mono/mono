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
	[StructLayout(LayoutKind.Sequential)]
	public sealed class BitmapData {
		internal int width, height, stride;
		internal PixelFormat pixel_format;		
		internal IntPtr address;
		internal int reserved;
		private	 bool bAllocated = false;
		internal bool own_scan0;		
		
		~BitmapData()
		{			
			if (bAllocated)
				Marshal.FreeHGlobal(address);						
		}
		
		internal bool Allocated {
			set {bAllocated = value;}
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
				
				if (bAllocated)
				{
					Marshal.FreeHGlobal(address);				
					bAllocated = false;
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
		
		internal unsafe void swap_red_blue_bytes () 
		{
			byte *start = (byte *) (void *) this.Scan0;
			int stride = this.Stride;
			for (int line = 0; line < this.Height; line++){
				// Exchange red <=> blue bytes
//				fixed (byte *pbuf = start) {
					byte* curByte = start;
					for (int i = 0; i < this.Width; i++) {
						byte t = *(curByte+2);
						*(curByte+2) = *curByte;
						*curByte = t;
						curByte += 3;
					}
//				}
				start += stride;
			}
		}
	}
}
