//                                                   
// Mono.Cairo.Pattern.cs
//
// Author: Jordi Mas (jordi@ximian.com)
//
// (C) Ximian Inc, 2004.
//
//

using System;
using System.Runtime.InteropServices;
using Cairo;

namespace Cairo {

        public class Pattern
        {
                IntPtr pattern = IntPtr.Zero;

                private Pattern ()
                {
                }

                public Pattern (Surface surface)
                {
                        pattern = CairoAPI.cairo_pattern_create_for_surface (surface.Pointer);
                }

                public Pattern (double x0, double y0, double x1, double y1)
                {
                        pattern = CairoAPI.cairo_pattern_create_linear (x0, y0, x1, y1);

                }

                public Pattern (double cx0, double cy0, double radius0,
		        	     double cx1, double cy1, double radius1)
                {
                        pattern = CairoAPI.cairo_pattern_create_radial (cx0, cy0, radius0,
                                cx1, cy1, radius1);
                }

                public void Reference ()
                {
                        CairoAPI.cairo_pattern_reference (pattern);
                }

                public void Destroy ()
                {
                        CairoAPI.cairo_pattern_destroy (pattern);
                }

                public Status AddColorStop (double offset, double red, double green,
                        double blue, double alpha)
                {
                        return CairoAPI.cairo_pattern_add_color_stop (pattern,
                                offset, red, green, blue, alpha);                
                }

                public Matrix Matrix {
                        set {
                                CairoAPI.cairo_pattern_set_matrix (pattern, value.Pointer);

                        }

                        get {
                                IntPtr matrix;

                                CairoAPI.cairo_pattern_get_matrix (pattern, out matrix);
                          return new Cairo.Matrix (matrix);
                        }
                }

                public Extend Extend {
                        set {
                                CairoAPI.cairo_pattern_set_extend (pattern, value);

                        }

                        get {
                                return CairoAPI.cairo_pattern_get_extend (pattern);                        
                        }
                }

                public Filter Filter {
                        set {
                                CairoAPI.cairo_pattern_set_filter (pattern, value);

                        }

                        get {
                                return CairoAPI.cairo_pattern_get_filter (pattern);
                        }
                }

                public IntPtr Pointer {
                        get { return pattern; }
                }

        }
}

