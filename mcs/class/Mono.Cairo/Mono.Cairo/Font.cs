//                                                   
// Mono.Cairo.Font.cs
//
// Author: Jordi Mas (jordi@ximian.com)
//
// (C) Ximian Inc, 2004.
//
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
                        IntPtr ft_font = IntPtr.Zero;
	                IntPtr font = IntPtr.Zero;
	                IntPtr pattern = IntPtr.Zero;
	                IntPtr library = IntPtr.Zero;
	                int error = 0;

	                pattern = FontConfig.FcPatternCreate ();                               
	                if (pattern == IntPtr.Zero)
		                return ft_font;

	                FontConfig.FcPatternAddString (pattern, FontConfig.FC_FAMILY, family);
	                FontConfig.FcPatternAddInteger (pattern, FontConfig.FC_SLANT, (int) fcslant);
	                FontConfig.FcPatternAddInteger (pattern, FontConfig.FC_WEIGHT, (int) fcweight);

	                error = FreeType.FT_Init_FreeType (out library);                            
                	if (error != 0) {
                                FontConfig.FcPatternDestroy (pattern);
                		return ft_font;
                	}

                	font = CairoAPI.cairo_ft_font_create (library, pattern);
                	if (font == IntPtr.Zero)
		                return ft_font;

                        /*
	                ft_font = (cairo_ft_font_t *) font;

	                ft_font->owns_ft_library = 1;

                	FT_Set_Char_Size (ft_font->face,
		        	  DOUBLE_TO_26_6 (1.0),
			          DOUBLE_TO_26_6 (1.0),
			          0, 0);*/

                        FontConfig.FcPatternDestroy (pattern);
	                return ft_font;
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
