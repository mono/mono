//
// Mono.Cairo.XlibSurface.cs
//
// Authors:
//    Duncan Mak
//    Miguel de Icaza.
//
// (C) Ximian Inc, 2003.
// (C) Novell, Inc. 2003.
//
// This is an OO wrapper API for the Cairo API
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

	public class XlibSurface : Surface
	{
		public XlibSurface (IntPtr display, IntPtr drawable, IntPtr visual, int width, int height)
			: base (NativeMethods.cairo_xlib_surface_create (display, drawable, visual, width, height), true)
		{
		}

		public XlibSurface (IntPtr ptr, bool own) : base (ptr, own)
		{
		}

		public static XlibSurface FromBitmap (IntPtr display, IntPtr bitmap, IntPtr screen, int width, int height)
		{
			IntPtr ptr = NativeMethods.cairo_xlib_surface_create_for_bitmap (display, bitmap, screen, width, height);
			return new XlibSurface(ptr, true);
		}

		public void SetDrawable (IntPtr drawable, int width, int height)
		{
			NativeMethods.cairo_xlib_surface_set_drawable (Handle, drawable, width, height);
		}

		public void SetSize (int width, int height)
		{
			NativeMethods.cairo_xlib_surface_set_size (Handle, width, height);
		}

		public int Depth {
			get { return NativeMethods.cairo_xlib_surface_get_depth (Handle); }
		}
		
		public IntPtr Display {
			get { return NativeMethods.cairo_xlib_surface_get_display (Handle); }
		}

		public IntPtr Drawable {
			get { return NativeMethods.cairo_xlib_surface_get_drawable (Handle); }
		}

		public int Height {
			get { return NativeMethods.cairo_xlib_surface_get_height (Handle); }
		}

		public IntPtr Screen {
			get { return NativeMethods.cairo_xlib_surface_get_screen (Handle); }
		}

		public IntPtr Visual {
			get { return NativeMethods.cairo_xlib_surface_get_visual (Handle); }
		}

		public int Width {
			get { return NativeMethods.cairo_xlib_surface_get_width (Handle); }
		}

	}
}
