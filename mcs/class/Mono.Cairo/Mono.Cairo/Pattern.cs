//                                                   
// Mono.Cairo.Pattern.cs
//
// Author: Jordi Mas (jordi@ximian.com)
//
// (C) Ximian Inc, 2004.
//
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

