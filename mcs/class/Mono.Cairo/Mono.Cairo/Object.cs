//
// Mono.Cairo.Object.cs
//
// Author:
//   Duncan Mak (duncan@ximian.com)
//   Miguel de Icaza (miguel@novell.com)
//
// (C) Ximian Inc, 2003.
// (C) Novell Inc, 2003.
//
// This is an OO wrapper API for the Cairo API.
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Cairo;

namespace Cairo {

        public class Object : IDisposable 
        {
                IntPtr state;

                public Object ()
                {
			state = CairoAPI.cairo_create ();
                }

		private Object (IntPtr state)
		{
			this.state = state;
		}
		
		~Object ()
		{
			Console.WriteLine ("Cairo not thread safe, you might want to call IDisposable.Dispose on Cairo.Surface");

		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		
                protected virtual void Dispose (bool disposing)
                {
			if (!disposing){
				Console.WriteLine ("Cairo.Object: called from thread");
				return;
			}
			
			if (state == (IntPtr) 0)
				return;

                        CairoAPI.cairo_destroy (state);
			state = (IntPtr) 0;
                }

                public Cairo.Object Copy ()
                {
                        IntPtr dest;
                        CairoAPI.cairo_copy (out dest, state);
                        return new Cairo.Object (dest);
                }
                
                public void Save ()
                {
                        CairoAPI.cairo_save (state);
                }

                public void Restore ()
                {
                        CairoAPI.cairo_restore (state);
                }
                
                public Cairo.Status Status {
                        get {
                                return CairoAPI.cairo_status (state);
                        }
                }

                public string StatusString {
                        get {
                                return CairoAPI.cairo_status_string (state);
                        }
                }

                public IntPtr Handle {
                        get {
                                return state;
                        }
                }
                
                public Cairo.Operator Operator {
                        set {
                                CairoAPI.cairo_set_operator (state, value);
                        }

                        get {
                                return CairoAPI.cairo_current_operator (state);
                        }
                }
                
                public void SetRGBColor (double r, double g, double b)
                {
                        CairoAPI.cairo_set_rgb_color (state, r, g, b);
                }

                public double Tolerance {
                        set {
                                CairoAPI.cairo_set_tolerance (state, value);
                        }

                        get {
                                return CairoAPI.cairo_current_tolerance (state);
                        }
                }                                

                public double Alpha {
                        set {
                                CairoAPI.cairo_set_alpha (state, value);
                        }

                        get {
                                return CairoAPI.cairo_current_alpha (state);
                        }
                }
                
                public Cairo.FillRule FillRule {
                        set {
                                CairoAPI.cairo_set_fill_rule (state, value);
                        }

                        get {
                                return CairoAPI.cairo_current_fill_rule (state);
                        }
                }
                                        
                public double LineWidth {
                        set {
                                CairoAPI.cairo_set_line_width (state, value);
                        }

                        get {
                                return CairoAPI.cairo_current_line_width (state);
                        }
                }

                public Cairo.LineCap LineCap {
                        set {
                                CairoAPI.cairo_set_line_cap (state, value);
                        }

                        get {
                                return CairoAPI.cairo_current_line_cap (state);
                        }
                }

                public Cairo.LineJoin LineJoin {
                        set {
                                CairoAPI.cairo_set_line_join (state, value);
                        }

                        get {
                                return CairoAPI.cairo_current_line_join (state);
                        }
                }

                public void SetDash (double [] dashes, int ndash, double offset)
                {
                        CairoAPI.cairo_set_dash (state, dashes, ndash, offset);
                }

                public Cairo.Surface Pattern {
                        set {
                                CairoAPI.cairo_set_pattern (state, value.Handle);
                        }
                }

                public double MiterLimit {
                        set {
                                CairoAPI.cairo_set_miter_limit (state, value);
                        }

                        get {
                                return CairoAPI.cairo_current_miter_limit (state);
                        }
                }

                public void GetCurrentPoint (out double x, out double y)
                {
                        CairoAPI.cairo_current_point (state, out x, out y);
                }

                public Point CurrentPoint {
                        get {
                                double x, y;
                                CairoAPI.cairo_current_point (state, out x, out y);
                                return new Point ((int) x, (int) y);
                        }
                }

                public Cairo.Surface TargetSurface {
                        set {
                                CairoAPI.cairo_set_target_surface (state, value.Handle);
                        }

                        get {
                                return Cairo.Surface.LookupExternalSurface (
                                        CairoAPI.cairo_current_target_surface (state));
                        }
                }

#region Path methods
                
                public void NewPath ()
                {
                        CairoAPI.cairo_new_path (state);
                }
                
                public void MoveTo (double x, double y)
                {
                        CairoAPI.cairo_move_to (state, x, y);
                }
                
                public void LineTo (double x, double y)
                {
                        CairoAPI.cairo_line_to (state, x, y);
                }

                public void CurveTo (double x1, double y1, double x2, double y2, double x3, double y3)
                {
                        CairoAPI.cairo_curve_to (state, x1, y1, x2, x2, x3, y3);
                }

                public void CurveTo (Point p1, Point p2, Point p3)
                {
                        CairoAPI.cairo_curve_to (state, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
                }

                public void RelMoveTo (double dx, double dy)
                {
                        CairoAPI.cairo_rel_move_to (state, dx, dy);
                }

                public void RelLineTo (double dx, double dy)
                {
                        CairoAPI.cairo_rel_line_to (state, dx, dy);
                }

                public void RelCurveTo (double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
                {
                        CairoAPI.cairo_rel_curve_to (state, dx1, dy1, dx2, dy2, dx3, dy3); 
                }

                public void Arc (double xc, double yc, double radius, double angel1, double angel2)
                {
                        CairoAPI.cairo_arc (state, xc, yc, radius, angel1, angel2);
                }

                public void ArcNegative (double xc, double yc, double radius, double angel1, double angel2)
                {
                        CairoAPI.cairo_arc_negative (state, xc, yc, radius, angel1, angel2);
                }
                
                public void Rectangle (double x, double y, double width, double height)
                {
                        CairoAPI.cairo_rectangle (state, x, y, width, height);
                }
                
                public void ClosePath ()
                {
                        CairoAPI.cairo_close_path (state);
                }
#endregion

#region Painting Methods

                public void Stroke ()
                {
                        CairoAPI.cairo_stroke (state);
                }

                public void Fill ()
                {
                        CairoAPI.cairo_fill (state);
                }

#endregion

                public void Clip ()
                {
                        CairoAPI.cairo_clip (state);
                }

#region Modified state

                public void SetTargetImage (
                        string data, Cairo.Format format, int width, int height, int stride)
                {
                        CairoAPI.cairo_set_target_image (state, data, format, width, height, stride);
                }

		public void SetTargetDrawable (IntPtr dpy, IntPtr drawable)
		{
			CairoAPI.cairo_set_target_drawable (state, dpy, drawable);
		}

		public void SetPattern (Surface pattern)
		{
			CairoAPI.cairo_set_pattern (state, pattern.Handle);
		}
#endregion

                public void Rotate (double angle)
                {
                        CairoAPI.cairo_rotate (state, angle);
                }

                public void Scale (double sx, double sy)
                {
                        CairoAPI.cairo_scale (state, sx, sy);
                }

                public void Translate (double tx, double ty)
                {
                        CairoAPI.cairo_translate (state, tx, ty);
                }

                public void TransformPoint (ref double x, ref double y)
                {
                        CairoAPI.cairo_transform_point (state, ref x, ref y);
                }

                public void TransformDistance (ref double dx, ref double dy)
                {
                        CairoAPI.cairo_transform_distance (state, ref dx, ref dy);
                }

                public void InverseTransformPoint (ref double x, ref double y)
                {
                        CairoAPI.cairo_inverse_transform_point (state, ref x, ref y);
                }

                public void InverseTransformDistance (ref double dx, ref double dy)
                {
                        CairoAPI.cairo_inverse_transform_distance (state, ref dx, ref dy);
                }

                public void ConcatMatrix (Cairo.Matrix matrix)
                {
                        CairoAPI.cairo_concat_matrix (state, matrix.Pointer);
                }

                public Cairo.Matrix Matrix {
                        set {
                                CairoAPI.cairo_set_matrix (state, value.Pointer);
                        }

                        get {
                                IntPtr p;
                                CairoAPI.cairo_current_matrix (state, out p);
                                return new Cairo.Matrix (p);
                        }
                }
        }
}
