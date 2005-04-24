//
// Mono.Cairo.Cairo.cs
//
// Author: Duncan Mak (duncan@ximian.com)
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
using System.Drawing;
using System.Runtime.InteropServices;

namespace Cairo {

	public class CairoAPI
        {
                internal const string CairoImp = "cairo";

                //
                // Manipulating state objects
                //
		[DllImport (CairoImp)]
		public static extern IntPtr cairo_create ();

		[DllImport (CairoImp)]
		public static extern void cairo_reference (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern void cairo_destroy (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern void cairo_save (IntPtr cr);                

		[DllImport (CairoImp)]
		public static extern void cairo_restore (IntPtr cr);

                [DllImport (CairoImp)]
		public static extern void cairo_copy (out IntPtr dest, IntPtr src);

                //
                // Modify state
                //
                [DllImport (CairoImp)]
                public static extern void cairo_set_target_surface (IntPtr cr, IntPtr surface);
                                
                [DllImport (CairoImp)]
                public static extern void cairo_set_target_image (
                        IntPtr cr, string data, Cairo.Format format, int width, int height, int stride);

                [DllImport (CairoImp)]
                public static extern void cairo_set_target_drawable (
								     IntPtr ct, IntPtr display, IntPtr drawable);

		[DllImport (CairoImp)]
		public static extern void cairo_set_operator (IntPtr cr, Cairo.Operator op);

       		[DllImport (CairoImp)]
		public static extern void cairo_set_rgb_color (IntPtr cr, double red, double green, double blue);

		[DllImport (CairoImp)]
		public static extern void cairo_set_alpha (IntPtr cr, double alpha);

                [DllImport (CairoImp)]
                public static extern void cairo_set_pattern (IntPtr cr, IntPtr pattern);

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
                public static extern void cairo_concat_matrix (IntPtr cr, IntPtr matrix);                
                
                [DllImport (CairoImp)]
                public static extern void cairo_set_matrix (IntPtr cr, IntPtr matrix);
                
                [DllImport (CairoImp)]
                public static extern void cairo_default_matrix (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_identity_matrix (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_transform_point (IntPtr cr, ref double x, ref double y);

                [DllImport (CairoImp)]
                public static extern void cairo_transform_distance (IntPtr cr, ref double dx, ref double dy);

                [DllImport (CairoImp)]
                public static extern void cairo_inverse_transform_point (IntPtr cr, ref double x, ref double y);

                [DllImport (CairoImp)]
                public static extern void cairo_inverse_transform_distance (IntPtr cr, ref double dx, ref double dy);

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
                public static extern void cairo_fill (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_clip (IntPtr cr);

                //
                // Font / Text
                //
                [DllImport (CairoImp)]
                public static extern void cairo_select_font (IntPtr cr, string key, FontSlant slant, FontWeight weight);

                [DllImport (CairoImp)]
                public static extern void cairo_scale_font (IntPtr cr, double scale);

                [DllImport (CairoImp)]
                public static extern void cairo_transform_font (IntPtr cr, IntPtr matrix);

                [DllImport (CairoImp)]
                public static extern void cairo_show_text (IntPtr cr, string utf8);

                [DllImport (CairoImp)]
                public static extern void cairo_font_set_transform (IntPtr font, IntPtr matrix);

                [DllImport (CairoImp)]
                public static extern void cairo_font_current_transform (IntPtr font, IntPtr matrix);

                [DllImport (CairoImp)]
                public static extern void cairo_font_reference (IntPtr font);

                [DllImport (CairoImp)]
                public static extern void cairo_font_destroy (IntPtr font);

                [DllImport (CairoImp)]
                public static extern void cairo_current_font_extents (IntPtr source, ref Extents extents);

                [DllImport (CairoImp)]
                public static extern void cairo_show_glyphs (IntPtr ct, IntPtr glyphs, int num_glyphs);

                [DllImport (CairoImp)]
                public static extern void cairo_text_path  (IntPtr ct, string utf8);

                [DllImport (CairoImp)]
                public static extern void cairo_glyph_path (IntPtr ct, IntPtr glyphs, int num_glyphs);


                // Cairo's font manipulation platform-specific Unix Fontconfig/Freetype interface

                [DllImport (CairoImp)]
                public static extern IntPtr cairo_ft_font_create (IntPtr ft_library, IntPtr ft_pattern);

        
                //
                // Image
                //                
                [DllImport (CairoImp)]
                public static extern void cairo_show_surface (IntPtr cr, IntPtr surface, int width, int height);



                //
                // query
                //                                
                [DllImport (CairoImp)]
		public static extern Cairo.Operator cairo_current_operator (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_current_rgb_color (
                        IntPtr cr, out double red, out double green, out double blue);

                [DllImport (CairoImp)]
                public static extern double cairo_current_alpha (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern bool cairo_in_stroke (IntPtr cr, double x, double y);

		[DllImport (CairoImp)]
		public static extern bool cairo_in_fill (IntPtr cr, double x, double y);

		[DllImport (CairoImp)]
		public static extern double cairo_current_tolerance (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern void cairo_current_point (
                        IntPtr cr, out double x, out double y);

		[DllImport (CairoImp)]
		public static extern Cairo.FillRule cairo_current_fill_rule (IntPtr cr);

                [DllImport (CairoImp)]
		public static extern double cairo_current_line_width (IntPtr cr);

                [DllImport (CairoImp)]
		public static extern LineCap cairo_current_line_cap (IntPtr cr);

       		[DllImport (CairoImp)]
		public static extern LineJoin cairo_current_line_join (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern double cairo_current_miter_limit (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_current_matrix (IntPtr cr, IntPtr matrix);

                [DllImport (CairoImp)]
                public static extern IntPtr cairo_current_target_surface (IntPtr cr);

                //
                // Error status queries
                //
                [DllImport (CairoImp)]
                public static extern Cairo.Status cairo_status (IntPtr cr);

		[DllImport (CairoImp, EntryPoint="cairo_status_string")]
		static extern IntPtr _cairo_status_string (IntPtr cr);

		public static string cairo_status_string (IntPtr cr)
		{
			return Marshal.PtrToStringAnsi (_cairo_status_string (cr));
		}
		
                //
                // Surface Manipulation
                //
                
                [DllImport (CairoImp)]                
                public static extern IntPtr cairo_surface_create_for_image (
                        string data, Cairo.Format format, int width, int height, int stride);

                [DllImport (CairoImp)]
                public static extern IntPtr cairo_image_surface_create (Cairo.Format format, int width,
                        int height);


                [DllImport (CairoImp)]
                public static extern IntPtr cairo_surface_create_similar (
                        IntPtr surface, Cairo.Format format, int width, int height);

                [DllImport (CairoImp)]                
                public static extern IntPtr cairo_surface_create_similar_solid (
                        IntPtr surface, Cairo.Format format,
                        int width, int height, double red, double green, double blue, double alpha);

                [DllImport (CairoImp)]
                public static extern void cairo_surface_reference (IntPtr surface);

                [DllImport (CairoImp)]                
                public static extern void cairo_surface_destroy (IntPtr surface);

                [DllImport (CairoImp)]                
                public static extern Cairo.Status cairo_surface_set_repeat (
                        IntPtr surface, int repeat);

                [DllImport (CairoImp)]                
                public static extern Cairo.Status cairo_surface_set_matrix (
                        IntPtr surface, IntPtr matrix);

                [DllImport (CairoImp)]                
                public static extern Cairo.Status cairo_surface_get_matrix (
                        IntPtr surface, out IntPtr matrix);

                [DllImport (CairoImp)]                
                public static extern Cairo.Status cairo_surface_set_filter (
                        IntPtr surface, Cairo.Filter filter);

                //
                // Matrix
                //

                [DllImport (CairoImp)]                
                public static extern IntPtr cairo_matrix_create ();

                [DllImport (CairoImp)]                
                public static extern void cairo_matrix_destroy (IntPtr matrix);

                [DllImport (CairoImp)]                                
                public static extern Cairo.Status cairo_matrix_copy (
                        IntPtr matrix, out IntPtr other);

                [DllImport (CairoImp)]                                
                public static extern Cairo.Status cairo_matrix_set_identity (IntPtr matrix);

                [DllImport (CairoImp)]                                
                public static extern Cairo.Status cairo_matrix_set_affine (
                        IntPtr matrix,
                        double a, double b, double c, double d, double tx, double ty);

                [DllImport (CairoImp)]                                
                public static extern Cairo.Status cairo_matrix_get_affine (
                        IntPtr matrix,
                        out double a, out double b, out double c, 
                        out double d, out double tx, out double ty);

                [DllImport (CairoImp)]                                
                public static extern Cairo.Status cairo_matrix_translate (
                        IntPtr matrix, double tx, double ty);

                [DllImport (CairoImp)]                                
                public static extern Cairo.Status cairo_matrix_scale (
                        IntPtr matrix, double sx, double sy);

                [DllImport (CairoImp)]                                
                public static extern Cairo.Status cairo_matrix_rotate (
                        IntPtr matrix, double radians);

                [DllImport (CairoImp)]                                
                public static extern Cairo.Status cairo_matrix_invert (IntPtr matrix);

                [DllImport (CairoImp)]                                
                public static extern Cairo.Status cairo_matrix_multiply (
                        out IntPtr result, IntPtr a, IntPtr b);

                [DllImport (CairoImp)]                                
                public static extern Cairo.Status cairo_matrix_transform_distance (
                        IntPtr matrix, ref double dx, ref double dy);

                [DllImport (CairoImp)]                                
                public static extern Cairo.Status cairo_matrix_transform_point (
                        IntPtr matrix, ref double x, ref double y);

                [DllImport (CairoImp)]
                public static extern void cairo_set_font (IntPtr ct, IntPtr font);

                [DllImport (CairoImp)]
                public static extern IntPtr cairo_current_font (IntPtr ct);

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
                public static extern void cairo_pattern_reference (IntPtr pattern);

                [DllImport (CairoImp)]
                public static extern void cairo_pattern_destroy (IntPtr pattern);

                [DllImport (CairoImp)]
                public static extern Status cairo_pattern_add_color_stop (IntPtr pattern,
		        double offset, double red, double green, double blue, double alpha);

                [DllImport (CairoImp)]
                public static extern Status cairo_pattern_set_matrix (IntPtr pattern, IntPtr matrix);

                [DllImport (CairoImp)]
                public static extern Status cairo_pattern_get_matrix (IntPtr pattern, out IntPtr matrix);

                [DllImport (CairoImp)]
                public static extern Status cairo_pattern_set_extend (IntPtr pattern, Extend extend);

                [DllImport (CairoImp)]
                public static extern Extend cairo_pattern_get_extend (IntPtr pattern);

                [DllImport (CairoImp)]
                public static extern Status cairo_pattern_set_filter (IntPtr pattern, Filter filter);

                [DllImport (CairoImp)]
                public static extern Filter cairo_pattern_get_filter (IntPtr pattern);

        }

        //
        // Freetype interface need it by cairo_ft interface calls
        //
        public class FreeType
        {
                internal const string FreeTypeImp = "freetype";

                [DllImport (FreeTypeImp)]
                public static extern int FT_Init_FreeType (out IntPtr library);

                [DllImport (FreeTypeImp)]
                public static extern int FT_Set_Char_Size (IntPtr face, long width, long height, uint horz_res, uint vert_res);
        }

        //
        // Fontconfig interface need it by cairo_ft interface calls
        //
        public class FontConfig
        {
                internal const string FontConfigImp = "fontconfig";

                public const string FC_FAMILY = "family";
                public const string FC_STYLE = "style";
                public const string FC_SLANT = "slant";
                public const string FC_WEIGHT = "weight";

                [DllImport (FontConfigImp)]
                public static extern bool FcPatternAddString (IntPtr pattern, string obj, string value);

                [DllImport (FontConfigImp)]
                public static extern bool FcPatternAddInteger (IntPtr pattern, string obj, int value);

                [DllImport (FontConfigImp)]
                public static extern IntPtr FcPatternCreate ();

                [DllImport (FontConfigImp)]
                public static extern bool FcPatternDestroy (IntPtr pattern); 
                
        }


        //
        // Enumerations
        //
        public enum Format {
                ARGB32 = 0,
                RGB24 = 1,
                A8 = 2,
                A1 = 4
        }

        public enum Operator {
                Clear = 0,
                Src = 1,
                Dst = 2,
                Over = 3,
                OverReverse = 4,
                In = 5,
                InReverse = 6,
                Out = 7,
                OutReverse = 8,
                Atop = 9,
                AtopReverse = 10,
                Xor = 11,
                Add = 12,
                Saturate = 13,
                
                DisjointClear = 16,
                DisjointSrc = 17,
                DisjointDst = 18,
                DisjointOver = 19,
                DisjointOverReverse = 20,
                DisjointIn = 21,
                DisjointInReverse = 22,
                DisjointOut = 23,
                DisjointOutReverse = 24,
                DisjointAtop = 25,
                DisjointAtopReverse = 26,
                DisjointXor = 27,

                ConjointClear = 32,
                ConjointSrc = 33,
                ConjointDst = 34,
                ConjointOver = 35,
                ConjointOverReverse = 36,
                ConjointIn = 37,
                ConjointInReverse = 38,
                ConjointOut = 39,
                ConjointOutReverse = 40,
                ConjointAtop = 41,
                ConjointAtopReverse = 42,
                ConjointXor = 43
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
                InvalidMatrix
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
                Repetat,
                Reflect,
        }

       
        [StructLayout(LayoutKind.Sequential)]
        public struct Extents
        {
                public  double x_bearing;
                public  double y_bearing;
                public  double width;
                public  double height;
                public  double x_advance;
                public  double y_advance;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Glyph
        {
                public  long index;
                public  double x;
                public  double y;
        }


}
