//
// Mono.Cairo.CairoObject.cs
//
// Author: Duncan Mak
//
// (C) Ximian Inc, 2003.
//
// This is an OO wrapper API for the Cairo API.
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Mono.Cairo;

namespace Mono.Cairo {

	public class CairoObject
        {
                IntPtr state;

                public CairoObject ()
                        : this (Create ())
                {
                }
                
                public CairoObject (IntPtr ptr)
                {
                        state = ptr;
                }
                
                public static IntPtr Create ()
                {
                        return Cairo.cairo_create ();
                }

                public IntPtr Destroy ()
                {
                        return Cairo.cairo_destroy (state);
                }
		
                public void Save ()
                {
                        Cairo.cairo_save (state);
                }

                public void Restore ()
                {
                        Cairo.cairo_restore (state);
                }
                
                public Cairo.Status Status {
                        get {
                                return Cairo.cairo_get_status (state);
                        }
                }

                public string StatusString {
                        get {
                                return Cairo.cairo_get_status_string (state);
                        }
                }

                public IntPtr Pointer {
                        get {
                                return state;
                        }
                }
                
                public Cairo.Operator Operator {
                        set {
                                Cairo.cairo_set_operator (state, value);
                        }

                        get {
                                return Cairo.cairo_get_operator (state);
                        }
                }
                
                public void SetRGBColor (double r, double g, double b)
                {
                        Cairo.cairo_set_rgb_color (state, r, g, b);
                }

                public double Tolerence {
                        set {
                                Cairo.cairo_set_tolerence (state, value);
                        }

                        get {
                                return Cairo.cairo_get_tolerence (state);
                        }
                }                                

                public double Alpha {
                        set {
                                Cairo.cairo_set_alpha (state, value);
                        }

                        get {
                                return Cairo.cairo_get_alpha (state);
                        }
                }
                
                public Cairo.FillRule FillRule {
                        set {
                                Cairo.cairo_set_fill_rule (state, value);
                        }

                        get {
                                return Cairo.cairo_get_fill_rule (state);
                        }
                }
                                        
                public double LineWidth {
                        set {
                                Cairo.cairo_set_line_width (state, value);
                        }

                        get {
                                return Cairo.cairo_get_line_width (state);
                        }
                }

                public Cairo.LineCap LineCap {
                        set {
                                Cairo.cairo_set_line_cap (state, value);
                        }

                        get {
                                return Cairo.cairo_get_line_cap (state);
                        }
                }

                public Cairo.LineJoin LineJoin {
                        set {
                                Cairo.cairo_set_line_join (state, value);
                        }

                        get {
                                return Cairo.cairo_get_line_join (state);
                        }
                }

                public void SetDash (double [] dashes, int ndash, double offset)
                {
                        Cairo.cairo_set_dash (state, dashes, ndash, offset);
                }

//              public CairoPatternObject Pattern {
//                       set {
//                              Cairo.cairo_set_pattern (state, value.Pointer);
//                      }
//              }

                public double MiterLimit {
                        set {
                                Cairo.cairo_set_miter_limit (state, value);
                        }

                        get {
                                return Cairo.cairo_get_miter_limit (state);
                        }
                }

                public void GetCurrentPoint (out double x, out double y)
                {
                        Cairo.cairo_get_current_point (state, out x, out y);
                }

                public Point CurrentPoint {
                        get {
                                double x, y;
                                Cairo.cairo_get_current_point (state, out x, out y);
                                return new Point ((int) x, (int) y);
                        }
                }

#region Path methods
                
                public void NewPath ()
                {
                        Cairo.cairo_new_path (state);
                }
		
                public void MoveTo (double x, double y)
                {
                        Cairo.cairo_move_to (state, x, y);
                }
		
                public void LineTo (double x, double y)
                {
                        Cairo.cairo_line_to (state, x, y);
                }

                public void CurveTo (double x1, double y1, double x2, double y2, double x3, double y3)
                {
                        Cairo.cairo_curve_to (state, x1, y1, x2, x2, x3, y3);
                }

                public void CurveTo (Point p1, Point p2, Point p3)
                {
                        Cairo.cairo_curve_to (state, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y);
                }

                public void RelMoveTo (double dx, double dy)
                {
                        Cairo.cairo_rel_move_to (state, dx, dy);
                }

                public void RelLineTo (double dx, double dy)
                {
                        Cairo.cairo_rel_line_to (state, dx, dy);
                }

                public void RelCurveTo (double dx1, double dy1, double dx2, double dy2, double dx3, double dy3)
                {
                        Cairo.cairo_rel_curve_to (state, dx1, dy1, dx2, dy2, dx3, dy3); 
                }

                public void Rectangle (double x, double y, double width, double height)
                {
                        Cairo.cairo_rectangle (state, x, y, width, height);
                }
		
                public void ClosePath ()
                {
                        Cairo.cairo_close_path (state);
                }
#endregion

#region Painting Methods

                public void Stroke ()
                {
                        Cairo.cairo_stroke (state);
                }

                public void Fill ()
                {
                        Cairo.cairo_fill (state);
                }

#endregion

                public void Clip ()
                {
                        Cairo.cairo_clip (state);
                }

#region Modified state

                public void SetTargetImage (
                        string data, Cairo.Format format, int width, int height, int stride)
                {
                        Cairo.cairo_set_target_image (state, data, format, width, height, stride);
                }

#endregion

                public void Rotate (double angle)
                {
                        Cairo.cairo_rotate (state, angle);
                }

                public void Scale (double sx, double sy)
                {
                        Cairo.cairo_scale (state, sx, sy);
                }

                public void Translate (double tx, double ty)
                {
                        Cairo.cairo_translate (state, tx, ty);
                }

                public void TransformPoint (ref double x, ref double y)
                {
                        Cairo.cairo_transform_point (state, ref x, ref y);
                }

                public void TransformDistance (ref double dx, ref double dy)
                {
                        Cairo.cairo_transform_distance (state, ref dx, ref dy);
                }

                public void InverseTransformPoint (ref double x, ref double y)
                {
                        Cairo.cairo_inverse_transform_point (state, ref x, ref y);
                }

                public void InverseTransformDistance (ref double dx, ref double dy)
                {
                        Cairo.cairo_inverse_transform_distance (state, ref dx, ref dy);
                }

                public void ConcatMatrix (CairoMatrixObject matrix)
                {
                        Cairo.cairo_concat_matrix (state, matrix.Pointer);
                }

                public CairoMatrixObject Matrix {
                        set {
                                Cairo.cairo_set_matrix (state, value.Pointer);
                        }
                }
        }
}
