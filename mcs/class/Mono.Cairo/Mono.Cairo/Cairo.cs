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

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Cairo {

	public class Cairo
        {
                const string CairoImp = "cairo";
                //
                // Manipulating state objects
                //
		[DllImport (CairoImp)]
		public static extern IntPtr cairo_create ();

		[DllImport (CairoImp)]
		public static extern IntPtr cairo_destroy (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern void cairo_save (IntPtr cr);                

		[DllImport (CairoImp)]
		public static extern void cairo_restore (IntPtr cr);

                //
                // Modify state
                //
                [DllImport (CairoImp)]
                public static extern void cairo_set_target_surface (IntPtr cr, IntPtr surface);
                                
                [DllImport (CairoImp)]
                public static extern void cairo_set_target_image (
                        IntPtr cr, string data, Cairo.Format format, int width, int height, int stride);

		[DllImport (CairoImp)]
		public static extern void cairo_set_operator (IntPtr cr, Cairo.Operator op);

       		[DllImport (CairoImp)]
		public static extern void cairo_set_rgb_color (IntPtr cr, double red, double green, double blue);

		[DllImport (CairoImp)]
		public static extern void cairo_set_alpha (IntPtr cr, double alpha);

                [DllImport (CairoImp)]
                public static extern void cairo_set_pattern (IntPtr cr, IntPtr pattern);

		[DllImport (CairoImp)]
		public static extern void cairo_set_tolerence (IntPtr cr, double tolerance);

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
                public static extern void cairo_select_font (IntPtr cr, string key);

                [DllImport (CairoImp)]
                public static extern void cairo_scale_font (IntPtr cr, double scale);

                [DllImport (CairoImp)]
                public static extern void cairo_transform_font (
                        IntPtr cr, double a, double b, double c, double d);

                [DllImport (CairoImp)]
                public static extern void cairo_text_extents (
                        IntPtr cr, string utf8,
                        ref double x, ref double y,
                        ref double width, ref double height,
                        ref double dx, ref double dy);

                [DllImport (CairoImp)]
                public static extern void cairo_show_text (IntPtr cr, string utf8);

                //
                // Image
                //                
                [DllImport (CairoImp)]
                public static extern void cairo_show_surface (IntPtr cr, IntPtr surface, int width, int height);

                //
                // query
                //                                
                [DllImport (CairoImp)]
		public static extern Cairo.Operator cairo_get_operator (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern void cairo_get_rgb_color (
                        IntPtr cr, out double red, out double green, out double blue);

                [DllImport (CairoImp)]
                public static extern double cairo_get_alpha (IntPtr cr);

		[DllImport (CairoImp)]
		public static extern double cairo_get_tolerence (IntPtr cr);

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
                public static extern void cairo_get_matrix (
                        IntPtr cr,
                        out double a, out double b,
                        out double c, out double d,
                        out double tx, out double ty);

                public static extern void cairo_get_target_surface (IntPtr cr);

                //
                // Error status queries
                //
                [DllImport (CairoImp)]
                public static extern Cairo.Status cairo_get_status (IntPtr cr);

                [DllImport (CairoImp)]
                public static extern string cairo_get_status_string (IntPtr cr);

                //
                // Surface Manipulation
                //
                
                //
                // This is commented out because we don't have access
                // to the X11 Drawable and Visual types.
                //                
//              [DllImport (CairoImp)]
//              public static extern IntPtr cairo_surface_create_for_drawable (
//                      IntPtr display, Drawable drawable, IntPtr visual,
//                      Cairo.Format format, Colormap colormap);

                [DllImport (CairoImp)]                
                public static extern IntPtr cairo_surface_create_for_image (
                        string data, Cairo.Format format, int width, int height, int stride);

                [DllImport (CairoImp)]
                public static extern IntPtr cairo_surface_create_similar (
                        IntPtr surface, Cairo.Format format, int width, int height);

                [DllImport (CairoImp)]                
                public static extern IntPtr cairo_surface_create_similar_solid (
                        IntPtr surface, Cairo.Format format,
                        int width, int height, double red, double green, double blue, double alpha);

                [DllImport (CairoImp)]                
                public static extern void cairo_surface_destroy (IntPtr surface);

                [DllImport (CairoImp)]                
                public static extern Cairo.Status cairo_surface_put_image (
                        IntPtr surface, string data, int width, int height, int stride);

                [DllImport (CairoImp)]                
                public static extern Cairo.Status cairo_surface_set_repeat (
                        IntPtr surface, int repeat);

                [DllImport (CairoImp)]                
                public static extern Cairo.Status cairo_surface_set_matrix (
                        IntPtr surface, IntPtr matrix);

                [DllImport (CairoImp)]                
                public static extern Cairo.Status cairo_surface_get_matrix (
                        IntPtr surface, ref IntPtr matrix);

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
                        out double a, out double b, out double c, out double d, out double tx, out double ty);

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
                Bilinear
        }
}
