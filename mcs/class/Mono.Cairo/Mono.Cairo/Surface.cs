//
// Mono.Cairo.CairoSurfaceObject.cs
//
// Author: Duncan Mak
//
// (C) Ximian Inc, 2003.
//
// This is an OO wrapper API for the Cairo API
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Cairo;

namespace Cairo {

	public class Surface
        {
                IntPtr surface;

                Surface (IntPtr ptr)
                {
                        surface = ptr;
                }

                public static Cairo.Surface CreateForImage (
                        string data, Cairo.Format format, int width, int height, int stride)
                {
                        IntPtr p = CairoAPI.cairo_surface_create_for_image (
                                data, format, width, height, stride);
                        
                        return new Cairo.Surface (p);
                }

                public static Cairo.Surface CreateSimilar (
                        Cairo.Surface surface, Cairo.Format format, int width, int height)
                {
                        IntPtr p = CairoAPI.cairo_surface_create_similar (
                                surface.Pointer, format, width, height);

                        return new Cairo.Surface (p);
                }

                public static Cairo.Surface CreateSimilarSolid (
                        Cairo.Surface surface, Cairo.Format format,
                        int width, int height, double red, double green, double blue, double alpha)
                {
                        IntPtr p = CairoAPI.cairo_surface_create_similar_solid (
                                surface.Pointer, format, width, height, red, green, blue, alpha);

                        return new Cairo.Surface (p);
                }

                public void Destroy ()
                {
                        CairoAPI.cairo_surface_destroy (surface);
                }

                public void PutImage (string data, int width, int height, int stride)
                {
                        CairoAPI.cairo_surface_put_image (surface, data, width, height, stride);
                }

                public IntPtr Pointer {
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
                                IntPtr p = IntPtr.Zero;
                                
                                CairoAPI.cairo_surface_get_matrix (surface, ref p);

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
