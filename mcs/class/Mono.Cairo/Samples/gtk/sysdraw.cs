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
	class Graphics
	{		
		//Use [DllImport("libgdk-win32-2.0-0.dll")] for  Win32 
		[DllImport("libgdk-x11-2.0.so")]
		internal static extern IntPtr gdk_x11_drawable_get_xdisplay (IntPtr raw);
	
		[DllImport("libgdk-x11-2.0.so")]
		internal static extern IntPtr gdk_x11_drawable_get_xid (IntPtr raw);
	
		public static void CreateDrawable (Gdk.Drawable drawable, Cairo.Graphics g)
		{
			IntPtr x_drawable = IntPtr.Zero;
			int x_off = 0, y_off = 0;			
			
			if (drawable is Gdk.Window)
				((Gdk.Window) drawable).GetInternalPaintInfo(out drawable, out x_off, out y_off);

			x_drawable = drawable.Handle;
			
			IntPtr display = gdk_x11_drawable_get_xdisplay (x_drawable);
			IntPtr Xdrawable = gdk_x11_drawable_get_xid (x_drawable);
   
    			g.SetTargetDrawable (display, Xdrawable);			
    
			if (drawable is Gdk.Window)
				g.Translate (-(double)x_off,-(double)y_off);
			
		}
	}
}
