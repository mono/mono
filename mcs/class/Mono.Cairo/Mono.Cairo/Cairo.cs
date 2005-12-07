//
// Mono.Cairo.Cairo.cs
//
// Authors: Duncan Mak (duncan@ximian.com)
//          Hisham Mardam Bey (hisham.mardambey@gmail.com)
//
// (C) Ximian, Inc. 2003
//
// This is a simplistic binding of the Cairo API to C#. All functions
// in cairo.h are transcribed into their C# equivelants and all
// enumerations are also listed here.
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

namespace Cairo {

	internal class CairoAPI
        {
                internal const string CairoImp = "libcairo-2.dll";
		
                //
                // Manipulating state objects
                //
		[DllImport (CairoImp)]
		public static extern IntPtr cairo_create (IntPtr target);

		[DllImport (CairoImp)]
		public static extern void cairo_reference (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern void cairo_destroy (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern void cairo_save (IntPtr cr);                

		[DllImport (CairoImp)]
		public static extern void cairo_restore (IntPtr cr);

                //
                // Modify state
                //
                [DllImport (CairoImp)]
                public static extern IntPtr cairo_image_surface_create_for_data (
                        IntPtr data, Cairo.Format format, int width, int height, int stride);

		[DllImport (CairoImp)]
		public static extern void cairo_set_operator (IntPtr cr, Cairo.Operator op);

       		[DllImport (CairoImp)]
		public static extern void cairo_set_source_rgba (IntPtr cr, double red, double green, double blue, double alpha);
		
       		[DllImport (CairoImp)]
		public static extern void cairo_set_source_rgb (IntPtr cr, double red, double green, double blue);		
				
                [DllImport (CairoImp)]
                public static extern void cairo_set_source (IntPtr cr, IntPtr pattern);
		
                [DllImport (CairoImp)]
                public static extern IntPtr cairo_get_source (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern void cairo_set_tolerance (IntPtr cr, double tolerance);

		[DllImport (CairoImp)]
		public static extern void cairo_set_fill_rule (IntPtr cr, Cairo.FillRule fill_rule);

		[DllImport (CairoImp)]
		public static extern void cairo_set_line_width (IntPtr cr, double width);

		[DllImport (CairoImp)]
		public static extern void cairo_set_line_cap (IntPtr cr, Cairo.LineCap line_cap);

		[DllImport (CairoImp)]
		public static extern void cairo_set_line_join (IntPtr cr, Cairo.LineJoin line_join);

       		[DllImport (CairoImp)]
		public static extern void cairo_set_dash (IntPtr cr, double [] dashes, int ndash, double offset);

		[DllImport (CairoImp)]
		public static extern void cairo_set_miter_limit (IntPtr cr, double limit);

                [DllImport (CairoImp)]
                public static extern void cairo_translate (IntPtr cr, double tx, double ty);

                [DllImport (CairoImp)]
                public static extern void cairo_scale (IntPtr cr, double sx, double sy);

                [DllImport (CairoImp)]                
                public static extern void cairo_rotate (IntPtr cr, double angle);
		
		[DllImport (CairoImp)]
		public static extern void cairo_transform (IntPtr cr, Matrix matrix);
                
                [DllImport (CairoImp)]
                public static extern void cairo_set_matrix (IntPtr cr, Matrix matrix);
                
                [DllImport (CairoImp)]
                public static extern void cairo_identity_matrix (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_user_to_device (IntPtr cr, ref double x, ref double y);

                [DllImport (CairoImp)]
                public static extern void cairo_user_to_device_distance (IntPtr cr, ref double dx, ref double dy);

                [DllImport (CairoImp)]
                public static extern void cairo_device_to_user (IntPtr cr, ref double x, ref double y);

                [DllImport (CairoImp)]
                public static extern void cairo_device_to_user_distance (IntPtr cr, ref double dx, ref double dy);
		
                //
                // Path creation
                //
		[DllImport (CairoImp)]
		public static extern void cairo_new_path (IntPtr cr);
		
		[DllImport (CairoImp)]
		public static extern void cairo_move_to (IntPtr cr, double x, double y);

		[DllImport (CairoImp)]
		public static extern void cairo_line_to (IntPtr cr, double x, double y);

		[DllImport (CairoImp)]
		public static extern void cairo_curve_to (
                        IntPtr cr, double x1, double y1, double x2, double y2, double x3, double y3);

                [DllImport (CairoImp)]
                public static extern void cairo_arc (
                        IntPtr cr, double xc, double yc, double radius, double angel1, double angel2);

                [DllImport (CairoImp)]
                public static extern void cairo_arc_negative (
                        IntPtr cr, double xc, double yc, double radius, double angel1, double angel2);
		
		[DllImport (CairoImp)]
		public static extern void cairo_rel_move_to (IntPtr cr, double dx, double dy);

		[DllImport (CairoImp)]
		public static extern void cairo_rel_line_to (IntPtr cr, double dx, double dy);

		[DllImport (CairoImp)]
		public static extern void cairo_rel_curve_to (
                        IntPtr cr, double dx1, double dy1, double dx2, double dy2, double dx3, double dy3);

		[DllImport (CairoImp)]
		public static extern void cairo_rectangle (IntPtr cr, double x, double y, double width, double height);

		[DllImport (CairoImp)]
		public static extern void cairo_close_path (IntPtr cr);

                //
                // Painting
                //
                [DllImport (CairoImp)]
                public static extern void cairo_stroke (IntPtr cr);
		
                [DllImport (CairoImp)]
                public static extern void cairo_stroke_preserve (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_stroke_extents (IntPtr cr, double x1, double y1, double x2, double y2);

                [DllImport (CairoImp)]
                public static extern void cairo_fill (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_fill_extents (IntPtr cr, double x1, double y1, double x2, double y2);

		[DllImport (CairoImp)]
                public static extern void cairo_fill_preserve (IntPtr cr);
		
		[DllImport (CairoImp)]
                public static extern void cairo_copy_page (IntPtr cr);
		
		[DllImport (CairoImp)]
                public static extern void cairo_show_page (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_clip (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_clip_preserve (IntPtr cr);
		
                [DllImport (CairoImp)]
                public static extern void cairo_reset_clip (IntPtr cr);

                //
                // Font / Text
                //
                [DllImport (CairoImp)]
                public static extern void cairo_select_font_face (IntPtr cr, 
							      string family, 
							      FontSlant slant, 
							      FontWeight weight);

		[DllImport (CairoImp)]
                public static extern void cairo_set_font_size (IntPtr cr,
							     double size);
		
		[DllImport (CairoImp)]
                public static extern IntPtr cairo_get_font_face (IntPtr cr);

		[DllImport (CairoImp)]
                public static extern void cairo_set_font_face (IntPtr cr, IntPtr fontFace);

		[DllImport (CairoImp)]
                public static extern Matrix cairo_get_font_matrix (IntPtr cr);

		[DllImport (CairoImp)]
                public static extern void cairo_set_font_matrix (IntPtr cr, Matrix matrix);
		
                [DllImport (CairoImp)]
                public static extern void cairo_show_text (IntPtr cr, string utf8);

                [DllImport (CairoImp)]
                public static extern void cairo_font_extents (IntPtr source, ref FontExtents extents);

                [DllImport (CairoImp)]
                public static extern void cairo_show_glyphs (IntPtr ct, IntPtr glyphs, int num_glyphs);

                [DllImport (CairoImp)]
                public static extern void cairo_text_path  (IntPtr ct, string utf8);

		[DllImport (CairoImp)]
                public static extern void cairo_text_extents  (IntPtr cr, string utf8, ref TextExtents extents);

				// FontOptions
				[DllImport (CairoImp)]
				internal static extern IntPtr cairo_font_options_create ();

				[DllImport (CairoImp)]
				internal static extern IntPtr cairo_font_options_copy (IntPtr handle);

				[DllImport (CairoImp)]
				internal static extern void cairo_font_options_destroy (IntPtr handle);

				[DllImport (CairoImp)]
				internal static extern bool cairo_font_options_equal (IntPtr h1, IntPtr h2);

				[DllImport (CairoImp)]
				internal static extern long cairo_font_options_hash (IntPtr handle);

				[DllImport (CairoImp)]
				internal static extern void cairo_font_options_merge (IntPtr handle, IntPtr other);

				[DllImport (CairoImp)]
				internal static extern Antialias cairo_font_options_get_antialias (IntPtr handle);

				[DllImport (CairoImp)]
				internal static extern void cairo_font_options_set_antialias (IntPtr handle, Antialias aa);

				[DllImport (CairoImp)]
				internal static extern HintMetrics cairo_font_options_get_hint_metrics (IntPtr handle);

				[DllImport (CairoImp)]
				internal static extern void cairo_font_options_set_hint_metrics (IntPtr handle, HintMetrics metrics);

				[DllImport (CairoImp)]
				internal static extern HintStyle cairo_font_options_get_hint_style (IntPtr handle);

				[DllImport (CairoImp)]
				internal static extern void cairo_font_options_set_hint_style (IntPtr handle, HintStyle style);

				[DllImport (CairoImp)]
				internal static extern SubpixelOrder cairo_font_options_get_subpixel_order (IntPtr handle);

				[DllImport (CairoImp)]
				internal static extern void cairo_font_options_set_subpixel_order (IntPtr handle, SubpixelOrder order);

				[DllImport (CairoImp)]
				internal static extern Status cairo_font_options_status (IntPtr handle);

				[DllImport (CairoImp)]
				internal static extern void cairo_get_font_options (IntPtr cr, IntPtr options);

				[DllImport (CairoImp)]
				internal static extern void cairo_set_font_options (IntPtr cr, IntPtr options);

                [DllImport (CairoImp)]
                public static extern void cairo_glyph_path (IntPtr ct, IntPtr glyphs, int num_glyphs);
        
                //
                // Image
                //
		[DllImport (CairoImp)]
                public static extern void cairo_set_source_surface (IntPtr cr, IntPtr surface, int width, int height);
		
                [DllImport (CairoImp)]
                internal static extern void cairo_mask (IntPtr cr, IntPtr pattern);
                
				[DllImport (CairoImp)]
                internal static extern void cairo_mask_surface (IntPtr cr, IntPtr surface, double x, double y);
				
                [DllImport (CairoImp)]
                public static extern void cairo_paint (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_paint_with_alpha (IntPtr cr, double alpha);
		
		[DllImport (CairoImp)]
                public static extern IntPtr cairo_image_surface_create_from_png  (string filename);
		
		[DllImport (CairoImp)]
                public static extern int cairo_image_surface_get_width  (IntPtr surface);
		
		[DllImport (CairoImp)]
                public static extern int cairo_image_surface_get_height (IntPtr surface);
		
                //
                // query
                //
		[DllImport (CairoImp)]
		public static extern bool cairo_in_stroke (IntPtr cr, double x, double y);

		[DllImport (CairoImp)]
		public static extern bool cairo_in_fill (IntPtr cr, double x, double y);

		[DllImport (CairoImp)]
		public static extern Cairo.Operator cairo_get_operator (IntPtr cr);
		
		[DllImport (CairoImp)]
		public static extern double cairo_get_tolerance (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern void cairo_get_current_point (
                        IntPtr cr, out double x, out double y);

		[DllImport (CairoImp)]
		public static extern Cairo.FillRule cairo_get_fill_rule (IntPtr cr);

                [DllImport (CairoImp)]
		public static extern double cairo_get_line_width (IntPtr cr);

                [DllImport (CairoImp)]
		public static extern LineCap cairo_get_line_cap (IntPtr cr);

       		[DllImport (CairoImp)]
		public static extern LineJoin cairo_get_line_join (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern double cairo_get_miter_limit (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_get_matrix (IntPtr cr, Matrix matrix);

                [DllImport (CairoImp)]
                public static extern IntPtr cairo_get_target (IntPtr cr);
		
                //
                // Error status queries
                //
                [DllImport (CairoImp)]
                public static extern Cairo.Status cairo_status (IntPtr cr);

                //
                // Surface Manipulation
                //
                
		[DllImport (CairoImp)]
                public static extern IntPtr cairo_xlib_surface_create (IntPtr dpi,
			IntPtr win, IntPtr visual, int w, int h);

		[DllImport (CairoImp)]
                public static extern void cairo_xlib_surface_set_drawable (IntPtr surface, IntPtr drawable, int width, int height);
		
		[DllImport (CairoImp)]
                public static extern void cairo_xlib_surface_set_size (IntPtr surface, int width, int height);
		
		[DllImport (CairoImp)]                
                public static extern Cairo.Status cairo_surface_finish (IntPtr surface);
		
		[DllImport (CairoImp)]                
                internal static extern Cairo.Status cairo_surface_status (IntPtr surface);
		
		[DllImport (CairoImp)]                
                public static extern void cairo_surface_set_device_offset (IntPtr surface,
								       double x, double y);

                [DllImport (CairoImp)]                
                public static extern IntPtr cairo_image_surface_create_for_data (
                        string data, Cairo.Format format, int width, int height, int stride);

                [DllImport (CairoImp)]
                public static extern IntPtr cairo_image_surface_create (Cairo.Format format, int width,
                        int height);


                [DllImport (CairoImp)]
                public static extern IntPtr cairo_surface_create_similar (
                        IntPtr surface, Cairo.Content content, int width, int height);

                [DllImport (CairoImp)]
                public static extern void cairo_surface_reference (IntPtr surface);

                [DllImport (CairoImp)]                
                public static extern void cairo_surface_destroy (IntPtr surface);

                [DllImport (CairoImp)]                
                public static extern void cairo_surface_write_to_png (IntPtr surface, string filename);

                [DllImport (CairoImp)]                
                public static extern IntPtr cairo_pdf_surface_create (string filename, double width, double height);

                [DllImport (CairoImp)]                
                public static extern void cairo_pdf_surface_set_dpi (IntPtr surface, double x_dpi, double y_dpi);

                [DllImport (CairoImp)]                
                public static extern IntPtr cairo_ps_surface_create (string filename, double width, double height);

                [DllImport (CairoImp)]                
                public static extern void cairo_ps_surface_set_dpi (IntPtr surface, double x_dpi, double y_dpi);
				
                [DllImport (CairoImp)]                
                public static extern IntPtr cairo_win32_surface_create (IntPtr hdc);

                //
                // Matrix
                //
		
		[DllImport (CairoImp)]                
		public static extern void cairo_matrix_init_translate (Matrix matrix, double tx, double ty);
		
		[DllImport (CairoImp)]                
		public static extern void cairo_matrix_translate (Matrix matrix, double tx, double ty);
		
		[DllImport (CairoImp)]
		public static extern void cairo_matrix_init_identity (Matrix matrix);

		[DllImport (CairoImp)]                
		public static extern void cairo_matrix_init_scale (Matrix matrix, double sx, double sy);
		
		[DllImport (CairoImp)]                
		public static extern void cairo_matrix_scale (Matrix matrix, double sx, double sy);

		[DllImport (CairoImp)]
		public static extern void cairo_matrix_init_rotate (Matrix matrix, double radians);		
		
		[DllImport (CairoImp)]                                
		public static extern void cairo_matrix_rotate (Matrix matrix, double radians);

		[DllImport (CairoImp)]                                
		public static extern Cairo.Status cairo_matrix_invert (Matrix matrix);

		[DllImport (CairoImp)]                                
		public static extern void cairo_matrix_multiply (Matrix result, Matrix a, Matrix b);

		[DllImport (CairoImp)]                                
		public static extern void cairo_matrix_transform_distance (Matrix matrix, ref double dx, ref double dy);

		[DllImport (CairoImp)]                                
		public static extern void cairo_matrix_transform_point (Matrix matrix, ref double x, ref double y);

                //
                // Pattern functions
                //
                [DllImport (CairoImp)]
                public static extern IntPtr cairo_pattern_create_for_surface (IntPtr surface);

                [DllImport (CairoImp)]
                public static extern IntPtr cairo_pattern_create_linear (double x0, double y0,
		        double x1, double y1);

                [DllImport (CairoImp)]
                public static extern IntPtr cairo_pattern_create_radial (double cx0, double cy0,
                        double radius0, double cx1, double cy1, double radius1);
		
		[DllImport (CairoImp)]
                public static extern IntPtr cairo_pattern_create_rgb (double r, 
								  double g,
								  double b);
		
		[DllImport (CairoImp)]
                public static extern IntPtr cairo_pattern_create_rgba (double r, 
								  double g,
								  double b,
								  double a);		

                [DllImport (CairoImp)]
                public static extern void cairo_pattern_reference (IntPtr pattern);

                [DllImport (CairoImp)]
                public static extern void cairo_pattern_destroy (IntPtr pattern);

                [DllImport (CairoImp)]
                public static extern Status cairo_pattern_add_color_stop_rgba (IntPtr pattern,
		        double offset, double red, double green, double blue, double alpha);
		
                [DllImport (CairoImp)]
                public static extern Status cairo_pattern_add_color_stop_rgb (IntPtr pattern,
		        double offset, double red, double green, double blue);

                [DllImport (CairoImp)]
                public static extern Status cairo_pattern_set_matrix (IntPtr pattern, Matrix matrix);

                [DllImport (CairoImp)]
                public static extern Status cairo_pattern_get_matrix (IntPtr pattern, Matrix matrix);

                [DllImport (CairoImp)]
                public static extern Status cairo_pattern_set_extend (IntPtr pattern, Extend extend);

                [DllImport (CairoImp)]
                public static extern Extend cairo_pattern_get_extend (IntPtr pattern);

                [DllImport (CairoImp)]
                public static extern Status cairo_pattern_set_filter (IntPtr pattern, Filter filter);

                [DllImport (CairoImp)]
                public static extern Filter cairo_pattern_get_filter (IntPtr pattern);
		
		[DllImport (CairoImp)]
                public static extern Status cairo_pattern_status (IntPtr pattern);

				[DllImport (CairoImp)]
				public static extern void cairo_set_antialias (IntPtr cr, Antialias antialias);

				[DllImport (CairoImp)]
				public static extern Antialias cairo_get_antialias (IntPtr cr);

        }

        //
        // Enumerations
        //
		
		public enum Antialias {
				Default,
				None,
				Gray,
				Subpixel,
		}

		public enum Content {
			Color = 0x1000,
			Alpha= 0x2000,
			ColorAlpha  = 0x3000,
		}
		
        public enum Format {
                ARGB32,
                RGB24,
                A8,
                A1,
        }

        public enum Operator {
		Clear,

		Source,
		Over,
		In,
		Out,
		Atop,

		Dest,
		DestOver,
		DestIn,
		DestOut,
		DestAtop,

		Xor,
		Add,
		Saturate,
        }

        public enum FillRule {
                Winding,
                EvenOdd
        }

        public enum LineCap {
                Butt, Round, Square
        }

        public enum LineJoin {
                Miter, Round, Bevel
        }

        public enum Status {
                Success = 0,
                NoMemory,
                InvalidRestore,
                InvalidPopGroup,
                NoCurrentPoint,
                InvalidMatrix,
				InvalidStatus,
				NullPointer,
				InvalidString,
				InvalidPathData,
				ReadError,
				WriteError,
				SurfaceFinished,
				SurfaceTypeMismatch,
				PatternTypeMismatch,
				InvalidContent,
				InvalidFormat,
				InvalidVisual,
				FileNotFound,
				InvalidDash
        }

        public enum Filter {
                Fast,
                Good,
                Best,
                Nearest,
                Bilinear,
                Gaussian,
        }

        public enum FontSlant {
                Normal  = 0,
                Italic  = 1,
                Oblique = 2
        }

        public enum FontWeight {
                Normal  = 0,
                Bold    = 1,
        }

        public enum Extend {
                None,
                Repeat,
                Reflect,
        }

		public enum HintMetrics {
			Default,
			Off,
			On,
		}

		public enum HintStyle {
			Default,
			None,
			Slight,
			Medium,
			Full,
		}

		public enum SubpixelOrder {
			Default,
			Rgb,
			Bgr,
			Vrgb,
			Vbgr,
		}

        [StructLayout(LayoutKind.Sequential)]
        public struct FontExtents
        {
                public  double Ascent;
                public  double Descent;
                public  double Height;
                public  double MaxXAdvance;
                public  double MaxYAdvance;
        }        
   
   
        [StructLayout(LayoutKind.Sequential)]
        public struct TextExtents
        {
                public  double XBearing;
                public  double YBearing;
                public  double Width;
                public  double Height;
                public  double XAdvance;
                public  double YAdvance;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Glyph
        {
                public  long Index;
                public  double X;
                public  double Y;
        }
}
