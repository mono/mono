//
// Mono.Cairo.Matrix.cs
//
// Author: Duncan Mak
//
// (C) Ximian Inc, 2003.
//
// This is an OO wrapper API for the Cairo API
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Cairo;

namespace Cairo {

        public class Matrix
        {
                IntPtr matrix;

                public Matrix ()
                        : this (Create ())
                {                        
                }

                internal Matrix (IntPtr ptr)
                {
                        matrix = ptr;
                }

                public static IntPtr Create ()
                {
                        return CairoAPI.cairo_matrix_create ();
                }

                public void Destroy ()
                {
                        CairoAPI.cairo_matrix_destroy (matrix);
                }

                public Cairo.Status Copy (out Cairo.Matrix other)
                {
                        IntPtr p = IntPtr.Zero;
                        
                        Cairo.Status status = CairoAPI.cairo_matrix_copy (matrix, out p);

                        other = new Cairo.Matrix (p);

                        return status;
                }

                public IntPtr Pointer {
                        get { return matrix; }
                }

                public Cairo.Status SetIdentity ()
                {
                        return CairoAPI.cairo_matrix_set_identity (matrix);
                }

                public Cairo.Status SetAffine (
                        double a, double b, double c, double d, double tx, double ty)
                {
                        return CairoAPI.cairo_matrix_set_affine (
                                matrix, a, b, c, d, tx, ty);
                }
                
                public Cairo.Status GetAffine (
                        out double a, out double b, out double c, out double d, out double tx, out double ty)
                {
                        return CairoAPI.cairo_matrix_get_affine (
                                matrix, out a, out b, out c, out d, out tx, out ty);
                }

                public Cairo.Status Scale (double sx, double sy)
                {
                        return CairoAPI.cairo_matrix_scale (matrix, sx, sy);
                }

                public Cairo.Status Rotate (double radians)
                {
                        return CairoAPI.cairo_matrix_rotate (matrix, radians);
                }

                public Cairo.Status Invert ()
                {
                        return CairoAPI.cairo_matrix_invert (matrix);
                }

                public static Cairo.Status Multiply (
                        out Cairo.Matrix result, Cairo.Matrix a, Cairo.Matrix b)
                {
                        IntPtr p = IntPtr.Zero;
                        
                        Cairo.Status status = CairoAPI.cairo_matrix_multiply (
                                out p, a.Pointer, b.Pointer);

                        result = new Cairo.Matrix (p);

                        return status;
                }

                public Cairo.Status TransformDistance (ref double dx, ref double dy)
                {
                        return CairoAPI.cairo_matrix_transform_distance (
                                matrix, ref dx, ref dy);
                }

                public Cairo.Status TransformPoint (ref double x, ref double y)
                {
                        return CairoAPI.cairo_matrix_transform_distance (
                                matrix, ref x, ref y);
                }
        }
}
