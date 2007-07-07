//                                                   
// Mono.Cairo.Pattern.cs
//
// Author: Jordi Mas (jordi@ximian.com)
//         Hisham Mardam Bey (hisham.mardambey@gmail.com)
// (C) Ximian Inc, 2004.
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

namespace Cairo {
   
        public class Pattern
        {
                protected IntPtr pattern = IntPtr.Zero;
		
                protected Pattern ()
                {
                }

		internal Pattern (IntPtr ptr)
		{			
			pattern = ptr;
		}		
		
                [Obsolete ("Use the SurfacePattern constructor")]
                public Pattern (Surface surface)
                {
                        pattern = CairoAPI.cairo_pattern_create_for_surface (surface.Handle);
                }
		
                protected void Reference ()
                {
                        CairoAPI.cairo_pattern_reference (pattern);
                }

                public void Destroy ()
                {
                        CairoAPI.cairo_pattern_destroy (pattern);
                }
		
		public Status Status
		{
			get { return CairoAPI.cairo_pattern_status (pattern); }
		}
		
                public Matrix Matrix {
                        set { 
				CairoAPI.cairo_pattern_set_matrix (pattern, value);
			}

                        get {
				Matrix m = new Matrix ();
				CairoAPI.cairo_pattern_get_matrix (pattern, m);
				return m;
                        }
                }

                public IntPtr Pointer {
                        get { return pattern; }
                }		

		public PatternType PatternType {
			get { return CairoAPI.cairo_pattern_get_type (pattern); }
		}
        }
}

