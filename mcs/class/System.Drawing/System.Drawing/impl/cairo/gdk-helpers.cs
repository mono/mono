using System;
using System.Runtime.InteropServices;

namespace Gdk {

        internal class Pixbuf
        {
                static Pixbuf ()
                {
                        Helpers.Init (0, new IntPtr (0));
                }
                
                const string libgdk_pixbuf = "libgdk_pixbuf-2.0-0.dll";

		[DllImport (libgdk_pixbuf, EntryPoint="gdk_pixbuf_new")]
		internal static extern IntPtr New (
                        int colorspace, bool has_alpha, int bits_per_sample,
                        int width, int height);

		[DllImport (libgdk_pixbuf, EntryPoint = "gdk_pixbuf_new_from_data")]
		internal static extern IntPtr NewFromData (
                        IntPtr data, Gdk.Colorspace colorspace, bool has_alpha, int bits_per_sample,
                        int width, int height, int rowstride,
                        IntPtr destroy_fn, IntPtr destroy_fn_data);

                [DllImport (libgdk_pixbuf, EntryPoint = "gdk_pixbuf_get_pixels")]
                internal static extern IntPtr GetPixels (IntPtr pixbuf);

                [DllImport (libgdk_pixbuf, EntryPoint = "gdk_pixbuf_get_rowstride")]
                internal static extern int GetRowstride (IntPtr pixbuf);
        }

        internal class Helpers
        {
                const string gdk = "libgdk-win32-2.0-0.dll";
                const string gobject = "libgobject-2.0-0.dll";

                [DllImport (gdk, EntryPoint = "gdk_init")]
                internal static extern void Init (int argc, IntPtr argv);

                [DllImport (gobject, EntryPoint = "g_object_unref")] 
                internal static extern void Unref (IntPtr obj);
        }

        internal enum Colorspace {
                Rgb = 0
        }
}
