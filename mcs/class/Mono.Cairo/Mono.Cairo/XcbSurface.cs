//
// Mono.Cairo.XcbSurface.cs
//
// Authors:
//    Alp Toker
//
// (C) Alp Toker, 2006.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Cairo {
	public class XcbSurface : Surface
	{
		internal XcbSurface (IntPtr handle, bool owns) : base (handle, owns)
		{
		}

		public XcbSurface (IntPtr connection, uint drawable, IntPtr visual, int width, int height)
			: base (NativeMethods.cairo_xcb_surface_create (connection, drawable, visual, width, height), true)
		{
		}

		public static XcbSurface FromBitmap (IntPtr connection, uint bitmap, IntPtr screen, int width, int height)
		{
			IntPtr ptr = NativeMethods.cairo_xcb_surface_create_for_bitmap (connection, bitmap, screen, width, height);
			return new XcbSurface (ptr, true);
		}

		public void SetSize (int width, int height)
		{
			NativeMethods.cairo_xcb_surface_set_size (Handle, width, height);
		}
	}
}
