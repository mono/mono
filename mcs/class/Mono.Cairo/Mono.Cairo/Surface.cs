//
// Mono.Cairo.CairoSurfaceObject.cs
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

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;
using Cairo;

namespace Cairo {

	public class Surface : IDisposable 
        {
		static Hashtable surfaces = new Hashtable ();
                IntPtr surface;

                private Surface (IntPtr ptr, bool owns)
                {
                        surface = ptr;
			lock (typeof (Surface)){
				surfaces [ptr] = this;
			}
			if (!owns)
				CairoAPI.cairo_surface_reference (ptr);
                }

		static internal Surface LookupExternalSurface (IntPtr p)
		{
			lock (typeof (Surface)){
				object o = surfaces [p];
				if (o == null){
					return new Surface (p, false);
				}
				return (Surface) o;
			}
		}
		
                public static Cairo.Surface CreateForImage (
                        string data, Cairo.Format format, int width, int height, int stride)
                {
                        IntPtr p = CairoAPI.cairo_surface_create_for_image (
                                data, format, width, height, stride);
                        
                        return new Cairo.Surface (p, true);
                }

                public static Cairo.Surface CreateSimilar (
                        Cairo.Surface surface, Cairo.Format format, int width, int height)
                {
                        IntPtr p = CairoAPI.cairo_surface_create_similar (
                                surface.Handle, format, width, height);

                        return new Cairo.Surface (p, true);
                }

                public static Cairo.Surface CreateSimilarSolid (
                        Cairo.Surface surface, Cairo.Format format,
                        int width, int height, double red, double green, double blue, double alpha)
                {
                        IntPtr p = CairoAPI.cairo_surface_create_similar_solid (
                                surface.Handle, format, width, height, red, green, blue, alpha);

                        return new Cairo.Surface (p, true);
                }

		~Surface ()
		{
			Dispose (false);
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (surface == (IntPtr) 0)
				return;
			lock (typeof (Surface)){
				surfaces.Remove (surface);
			}
			CairoAPI.cairo_surface_destroy (surface);
			surface = (IntPtr) 0;
		}
		
                public IntPtr Handle {
                        get { return surface; }
                }

                public int Repeat {
                        set {
                                CairoAPI.cairo_surface_set_repeat (surface, value);
                        }
                }

                public Cairo.Matrix Matrix {
                        set {
                                CairoAPI.cairo_surface_set_matrix (surface, value.Pointer);
                        }

                        get {
                                IntPtr p;
                                
                                CairoAPI.cairo_surface_get_matrix (surface, out p);

                                return new Cairo.Matrix (p);
                        }
                }

                public Cairo.Filter Filter {
                        set {
                                CairoAPI.cairo_surface_set_filter (surface, value);
                        }
                }
        }
}
