//                                                   
// Mono.Cairo.Font.cs
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
using System.Drawing;
using System.Runtime.InteropServices;
using Cairo;

namespace Cairo {

        /* Font object encapsulates all the functionality related to font handeling */
        public class Font
        {
                internal IntPtr font = IntPtr.Zero;

                internal IntPtr _create (string family, FontSlant fcslant, FontWeight fcweight)
                {
	                IntPtr font = IntPtr.Zero;
	                IntPtr pattern = IntPtr.Zero;
	                IntPtr library = IntPtr.Zero;
	                int error = 0;

	                pattern = FontConfig.FcPatternCreate ();                               
	                if (pattern == IntPtr.Zero)
		                return font;

	                FontConfig.FcPatternAddString (pattern, FontConfig.FC_FAMILY, family);
	                FontConfig.FcPatternAddInteger (pattern, FontConfig.FC_SLANT, (int) fcslant);
	                FontConfig.FcPatternAddInteger (pattern, FontConfig.FC_WEIGHT, (int) fcweight);

	                error = FreeType.FT_Init_FreeType (out library);                            
                	if (error != 0) {
                                FontConfig.FcPatternDestroy (pattern);
                		return font;
                	}

                	font = CairoAPI.cairo_ft_font_create (library, pattern);
                	if (font == IntPtr.Zero)
		                return font;

                        /*
	                ft_font = (cairo_ft_font_t *) font;

	                ft_font->owns_ft_library = 1;

                	FT_Set_Char_Size (ft_font->face,
		        	  DOUBLE_TO_26_6 (1.0),
			          DOUBLE_TO_26_6 (1.0),
			          0, 0);*/

                        FontConfig.FcPatternDestroy (pattern);
	                return font;
                }


                private Font ()
                {
                                     
                }

                
                public Font (string family, FontSlant fcslant, FontWeight fcweight)
                {
                        font =  _create (family, fcslant, fcweight);
                }
                

                internal Font (IntPtr native)
                {
                        font = native;
                }

               
                public void Destroy ()
                {
                        CairoAPI.cairo_font_destroy (font);
                }

                public void Reference ()
                {
                        CairoAPI.cairo_font_reference (font);

                }

                public Cairo.Matrix Transform {
                        set {
                                CairoAPI.cairo_font_set_transform (font, value.Pointer);
                        }

                        get {
                                IntPtr matrix;

                                CairoAPI.cairo_surface_get_matrix (font, out matrix);
                                return new Cairo.Matrix (matrix);
                        }
                }

                public IntPtr Pointer {
                        get { return font; }
                }


                              
        }
}
