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
using Mono.Cairo;

namespace Mono.Cairo {

	public class CairoSurfaceObject
        {
                IntPtr surface;

                CairoSurfaceObject (IntPtr ptr)
                {
                        surface = ptr;
                }

                public static CairoSurfaceObject CreateFromImage (
                        string data, Cairo.Format format, int width, int height, int stride);
                {
                        IntPtr p = Cairo.cairo_surface_create_from_image (
                                surface, data, format, width, height, stride);
                        
                        return new CairoSurfaceObject (p);
                }

                public static CairoMatrixObject CreateSimilar (Cairo.Format format, int width, int height);
                {
                        IntPtr p = Cairo.cairo_surface_create_similar (
                                surface, format, width, height);

                        return new CairoSurfaceObject (p);
                }

                public static CairoSurfaceObject CreateSimilarSolid (
                        Cairo.Format format,
                        int width, int height, double red, double green, double blue, double alpha)
                {
                        IntPtr p = Cairo.cairo_surface_create_similiar_solid (
                                surface, format, width, height, red, green, blue, alpha);

                        return new CairoSurfaceObject (p);
                }

                public void Destroy ()
                {
                        Cairo.cairo_surface_destroy (surface);
                }

                public void PutImage (string data, int width, int height, int stride)
                {
                        Cairo.cairo_surface_put_image (surface, data, width, height, stride);
                }

                public IntPtr Pointer {
                        get { return surface; }
                }

                public int Repeat {
                        set {
                                Cairo.cairo_surface_set_repeat (surface, value);
                        }
                }

                public CairoMatrixObject Matrix {
                        set {
                                Cairo.cairo_surface_set_matrix (surface, value.Pointer);
                        }

                        get {
                                IntPtr p;
                                
                                Cairo.cairo_surface_get_matrix (surface, p);

                                return new CairoMatrixObject (p);
                        }
                }

                public Cairo.Filter Filter {
                        set {
                                Cairo.cairo_surface_set_filter (surface, value);
                        }
                }
        }
}
