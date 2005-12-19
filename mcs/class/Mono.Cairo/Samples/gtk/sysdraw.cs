//
//
//	GDK-X11 interface
//

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
using System.Reflection;
using System.Runtime.InteropServices;
using Gtk;
using Cairo;

namespace Gdk 
{		
        public class Context
	{		
		//Use [DllImport("libgdk-win32-2.0-0.dll")] for  Win32 
		[DllImport("libgdk-x11-2.0.so")]
		  internal static extern IntPtr gdk_x11_drawable_get_xdisplay (IntPtr raw);
		
		[DllImport("libgdk-x11-2.0.so")]
		  internal static extern IntPtr gdk_x11_drawable_get_xid (IntPtr raw);
		
		[DllImport("libgdk-x11-2.0.so")]
		  internal static extern IntPtr gdk_drawable_get_visual (IntPtr raw);
		
		[DllImport("libgdk-x11-2.0.so")]
		  internal static extern IntPtr gdk_x11_visual_get_xvisual (IntPtr raw);
		
		[DllImport("libgdk-x11-2.0.so")]
		  internal static extern IntPtr gdk_cairo_create (IntPtr raw);
		
	        public static Cairo.Context CreateDrawable (Gdk.Drawable drawable)
		{
			IntPtr x_drawable = IntPtr.Zero;
			int x_off = 0, y_off = 0;			
			
			int x, y, w, h, d;
			((Gdk.Window)drawable).GetGeometry(out x, out y, out w, out h, out d);

			bool is_gdk_window = drawable is Gdk.Window;
			if (is_gdk_window)
				((Gdk.Window) drawable).GetInternalPaintInfo(out drawable, 
									     out x_off, out y_off);
			
			x_drawable = drawable.Handle;			
			IntPtr visual = gdk_drawable_get_visual(x_drawable);
			
			IntPtr Xdisplay = gdk_x11_drawable_get_xdisplay(x_drawable);
			IntPtr Xvisual = gdk_x11_visual_get_xvisual(visual);
			IntPtr Xdrawable = gdk_x11_drawable_get_xid (x_drawable);
			
			Cairo.XlibSurface s = new Cairo.XlibSurface (Xdisplay,
								   Xdrawable,
								   Xvisual,
								   w, h);
			
			Cairo.Context g = new Cairo.Context (s);
			
			// this can be safely removed now, just keep it for a bit more
			//Cairo.Context g = new Cairo.Context (
			//                    gdk_cairo_create (x_drawable ));
			
			if (is_gdk_window)
				g.Translate (-(double)x_off,-(double)y_off);
			return g;
		}
	}
}
