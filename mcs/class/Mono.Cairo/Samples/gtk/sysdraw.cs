//
//
//	GDK-X11 interface
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
