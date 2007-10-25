using System;
using System.Runtime.InteropServices;
using Cairo;

namespace Gdk
{
	public class Context
	{
		private Context () {}

		// Note: we don't need or want a .dll.config since we p/invoke
		// the proper lib based on the os check below

		// win32 only imports
		[DllImport("libgdk-win32-2.0-0.dll")]
		internal static extern IntPtr gdk_win32_hdc_get(IntPtr drawable, IntPtr gc, int usage);
		
		[DllImport("libgdk-win32-2.0-0.dll")]
		internal static extern void gdk_win32_hdc_release(IntPtr drawable,IntPtr gc,int usage);

		// x11 only imports
		[DllImport("libgdk-x11-2.0.so.0")]
		internal static extern IntPtr gdk_x11_drawable_get_xdisplay (IntPtr handle);

		[DllImport("libgdk-x11-2.0.so.0")]
		internal static extern IntPtr gdk_drawable_get_visual (IntPtr handle);

		[DllImport("libgdk-x11-2.0.so.0")]
		internal static extern IntPtr gdk_x11_visual_get_xvisual (IntPtr handle);

		[DllImport("libgdk-x11-2.0.so.0")]
		internal static extern IntPtr gdk_x11_drawable_get_xid (IntPtr handle);
		
		public static Cairo.Context CreateDrawable (Gdk.Drawable drawable)
		{
			return CreateDrawable (drawable, true);
		}

		public static Cairo.Context CreateDrawable (Gdk.Drawable drawable, bool double_buffered)
		{
			int x, y, w, h, d;
			Cairo.Surface surface;
			bool needs_xlate;
			
			((Gdk.Window)drawable).GetGeometry(out x, out y, out w, out h, out d);
			
			PlatformID os = Environment.OSVersion.Platform;

			needs_xlate = drawable is Gdk.Window && double_buffered;
			
			if (needs_xlate)
				((Gdk.Window)drawable).GetInternalPaintInfo (out drawable, out x, out y);

			if (os == PlatformID.Win32Windows || os == PlatformID.Win32NT ||
			    os == PlatformID.Win32S || os == PlatformID.WinCE) {

				Gdk.GC gcc = new Gdk.GC (drawable);
				IntPtr windc = gdk_win32_hdc_get (drawable.Handle, gcc.Handle, 0);
				surface = new Win32Surface (windc);
				
				if (double_buffered)
					gdk_win32_hdc_release (drawable.Handle, gcc.Handle, 0);
			} else {
				IntPtr display = gdk_x11_drawable_get_xdisplay (drawable.Handle);
				IntPtr visual = gdk_drawable_get_visual (drawable.Handle);
				IntPtr xvisual = gdk_x11_visual_get_xvisual (visual);
				IntPtr xdrawable = gdk_x11_drawable_get_xid (drawable.Handle);
				surface = new XlibSurface (display, xdrawable, xvisual, w, h);
			}

			Cairo.Context ctx = new Cairo.Context (surface);

			if (needs_xlate)
				ctx.Translate (-(double) x, -(double) y);
			return ctx;
		}
	}
}

