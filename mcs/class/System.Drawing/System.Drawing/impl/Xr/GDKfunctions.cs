//
// System.Drawing.XrImpl.GDKfunctions.cs
//
// (C) 2002/3 Ximian, Inc.  http://www.ximian.com
// Author: Christian Meyer <Christian.Meyer@cs.tum.edu>
//			Jason Perkins <jason@379.com>
//			Dennis Hayes <dennish@raytek.com>
//			Alexandre Pigolkine <pigolkine@gmx.de>
//
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.XrImpl {
	internal class GDK {
	
			#region gtk
			/// <summary>
			// GTK/GDK interface
			/// </summary>
		
			const string GDKimp = "gdk-x11-2.0";
			const string GDKPixbuf = "gdk_pixbuf";
			const string GLIBimp = "gobject-2.0";
			//const string GDK = "libgdk-0.dll";
			//const string GLIB = "libgobject-2.0-0.dll";
			
			internal enum GdkColorspace : int
			{
				GDK_COLORSPACE_RGB = 0
			}
			
			
			static GDK() {		
				gdk_init(0, new IntPtr(0));
			}
		
			[DllImport(GDKimp)]
			internal static extern void gdk_init(int argc, IntPtr argv);
		
			[DllImport(GDKimp)]
			internal static extern IntPtr gdk_pixbuf_new(int colorspace, bool has_alpha, int bits_per_sample, int width, int height);

			[DllImport(GDKimp)]
			internal static extern IntPtr gdk_pixbuf_new_from_data (IntPtr data, GdkColorspace colorspace, bool has_alpha, int bits_per_sample, int width, int height, int rowstride, IntPtr destroy_fn, IntPtr destroy_fn_data);
			
			[DllImport(GDKimp)]
			internal static extern int gdk_pixbuf_get_width(IntPtr pixbuf);
		
			[DllImport(GDKimp)]
			internal static extern int gdk_pixbuf_get_height(IntPtr pixbuf);
		
			[DllImport(GDKimp)]
			internal static extern int gdk_pixbuf_get_rowstride(IntPtr pixbuf);
		
			[DllImport(GDKimp)]
			internal static extern IntPtr gdk_pixbuf_get_pixels(IntPtr pixbuf);

			[DllImport(GDKimp, EntryPoint="gdk_pixbuf_get_pixels")]
			internal static extern byte [] gdk_pixbuf_get_pixels_byte(IntPtr pixbuf);
			
			[DllImport(GDKimp)]
			internal static extern int gdk_pixbuf_get_has_alpha(IntPtr pixbuf);
		
			[DllImport(GDKimp)]
			internal static extern IntPtr gdk_pixbuf_loader_new();
		
			[DllImport(GDKimp)]
			internal static extern int gdk_pixbuf_loader_close(IntPtr loader, IntPtr error);
		
			[DllImport(GDKimp)]
			internal static extern int gdk_pixbuf_loader_write(IntPtr laoder, byte[] buffer, int size);
		
			[DllImport(GDKimp)]
			internal static extern IntPtr gdk_pixbuf_loader_get_pixbuf(IntPtr loader);
		
			[DllImport(GDKimp)]
			internal static extern void gdk_pixbuf_ref(IntPtr pixbuf);
		
			[DllImport(GDKimp)]
			internal static extern void gdk_pixbuf_unref(IntPtr pixbuf);
		
			[DllImport(GDKimp)]
			internal static extern void g_object_unref(IntPtr ptr);
			
			[DllImport(GDKPixbuf)]
			internal static extern void gdk_pixbuf_finalize(IntPtr pixbuf);

			#endregion
	}
}
