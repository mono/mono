//
// System.Drawing.Drawing2D.Matrix.cs
//
// Author:
//   Stefan Maierhofer <sm@cg.tuwien.ac.at>
//   Dennis Hayes (dennish@Raytek.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D
{
        public  sealed class Matrix : MarshalByRefObject, IDisposable
        {
                internal IntPtr nativeMatrix;
                
                // constructors
                Matrix (IntPtr ptr)
                {
                        nativeMatrix = ptr;
                }
                
                public Matrix ()
                {
                        Status s = GDIPlus.GdipCreateMatrix (out nativeMatrix);
                }
        
                public Matrix (Rectangle rect , Point[] plgpts)
                {
                        GpRect rectangle = new GpRect (rect);

                        GDIPlus.GdipCreateMatrix3I (rectangle, plgpts, out nativeMatrix);
                }
        
                public Matrix (RectangleF rect , PointF[] pa)
                {
                        GpRectF rectangle = new GpRectF (rect);

                        GDIPlus.GdipCreateMatrix3 (rectangle, pa, out nativeMatrix);
                }

                public Matrix (float m11, float m12, float m21, float m22, float dx, float dy)
                {
                        GDIPlus.GdipCreateMatrix2 (m11, m12, m21, m22, dx, dy, out nativeMatrix);
                }
        
                // properties
                public float[] Elements {
                        get {
                                IntPtr tmp = Marshal.AllocHGlobal (8 * 6);

                                Status s = GDIPlus.GdipGetMatrixElements (nativeMatrix, tmp);

                                float [] retval = new float [6];

                                Marshal.Copy (tmp, retval, 0, 6);

                                Marshal.FreeHGlobal (tmp);
                                return retval;
                        }
                }
        
                public bool IsIdentity {
                        get {
                                bool retval;
                                GDIPlus.GdipIsMatrixIdentity (nativeMatrix, out retval);

                                return retval;
                        }
                }
        
                public bool IsInvertible {
                        get {
                                bool retval;
                                GDIPlus.GdipIsMatrixInvertible (nativeMatrix, out retval);

                                return retval;
                        }
                }
        
                public float OffsetX {
                        get {
                                return this.Elements [4];
                        }
                }
        
                public float OffsetY {
                        get {
                                return this.Elements [5];
                        }
                }

                public Matrix Clone()
                {
                        IntPtr retval;
                        Status s = GDIPlus.GdipCloneMatrix (nativeMatrix, out retval);
                        return new Matrix (retval);
                }
                
        
                public void Dispose ()
                {
                        GDIPlus.GdipDeleteMatrix (nativeMatrix); 
                }                       
        
                public override bool Equals (object obj)
                {
                        Matrix m = obj as Matrix;

                        if (m != null) {
                                bool retval;
                                GDIPlus.GdipIsMatrixEqual (nativeMatrix, m.nativeMatrix, out retval);

                                return retval;

                        } else
                                return false;
                }
        
                ~Matrix()
                {
                        Dispose ();
                }
                
                public override int GetHashCode ()
                {
                        return base.GetHashCode ();
                }
        
                public void Invert ()
                {
                        GDIPlus.GdipInvertMatrix (nativeMatrix);
                }
        
                public void Multiply (Matrix matrix)
                {
                        Multiply (matrix, MatrixOrder.Prepend);
                }
        
                public void Multiply (Matrix matrix, MatrixOrder order)
                {
                        GDIPlus.GdipMultiplyMatrix (nativeMatrix, matrix.nativeMatrix, order);
                }
        
                public void Reset()
                {
                        GDIPlus.GdipSetMatrixElements (nativeMatrix, 1, 0, 0, 1, 0, 0);
                }

                public override string ToString ()
                {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder ();
                        sb.Append ("(");
                        sb.Append (Elements [0] + " ");
                        sb.Append (Elements [1] + " ");
                        sb.Append (Elements [2] + " ");
                        sb.Append (Elements [3] + " ");
                        sb.Append (Elements [4] + " ");
                        sb.Append (Elements [5] + ")");                        
                        return sb.ToString ();
                }
        
                public void Rotate (float angle)
                {
                        Rotate (angle, MatrixOrder.Prepend);
                }
        
                public void Rotate (float angle, MatrixOrder order)
                {
                        GDIPlus.GdipRotateMatrix (nativeMatrix, angle, order);
                }
        
                public void RotateAt (float angle, PointF point)
                {
                        RotateAt (angle, point, MatrixOrder.Prepend);
                }
        
                public void RotateAt (float angle, PointF point, MatrixOrder order)
                {
                        angle *= (float) (Math.PI / 180.0);  // degrees to radians
                        float cos = (float) Math.Cos (angle);
                        float sin = (float) Math.Sin (angle);
                        float e4 = -point.X * cos + point.Y * sin + point.X;
                        float e5 = -point.X * sin - point.Y * cos + point.Y;
                        float[] m = this.Elements;

                        if (order == MatrixOrder.Prepend)
                                GDIPlus.GdipSetMatrixElements (nativeMatrix,
                                                cos * m[0] + sin * m[2],
                                                cos * m[1] + sin * m[3],
                                                -sin * m[0] + cos * m[2],
                                                -sin * m[1] + cos * m[3],
                                                e4 * m[0] + e5 * m[2] + m[4],
                                                e4 * m[1] + e5 * m[3] + m[5]);
                        else
                                GDIPlus.GdipSetMatrixElements (nativeMatrix,
                                                m[0] * cos + m[1] * -sin,
                                                m[0] * sin + m[1] * cos,
                                                m[2] * cos + m[3] * -sin,
                                                m[2] * sin + m[3] * cos,
                                                m[4] * cos + m[5] * -sin + e4,
                                                m[4] * sin + m[5] * cos + e5);
                }
        
                public void Scale (float scaleX, float scaleY)
                {
                        Scale (scaleX, scaleY, MatrixOrder.Prepend);
                }
        
                public void Scale (float scaleX, float scaleY, MatrixOrder order)
                {
                        GDIPlus.GdipScaleMatrix (nativeMatrix, scaleX, scaleY, order);
                }
        
                public void Shear (float shearX, float shearY)
                {
                        Shear (shearX, shearY, MatrixOrder.Prepend);
                }
        
                public void Shear (float shearX, float shearY, MatrixOrder order)
                {
                        GDIPlus.GdipShearMatrix (nativeMatrix, shearX, shearY, order);
                }
        
                public void TransformPoints (Point[] pts)
                {
                        GDIPlus.GdipTransformMatrixPointsI (nativeMatrix, pts, pts.Length);
                }
        
                public void TransformPoints (PointF[] pts)
                {
                        GDIPlus.GdipTransformMatrixPoints (nativeMatrix, pts, pts.Length);
                }
        
                public void TransformVectors (Point[] pts)
                {
                        GDIPlus.GdipVectorTransformMatrixPointsI (nativeMatrix, pts, pts.Length);
                }
        
                public void TransformVectors (PointF[] pts)
                {
                        GDIPlus.GdipVectorTransformMatrixPoints (nativeMatrix, pts, pts.Length);                        
                }
        
                public void Translate (float offsetX, float offsetY)
                {
                        Translate (offsetX, offsetY, MatrixOrder.Prepend);
                }
        
                public void Translate (float offsetX, float offsetY, MatrixOrder order)
                {
                        GDIPlus.GdipTranslateMatrix (nativeMatrix, offsetX, offsetY, order);
                }
        
                public void VectorTransformPoints (Point[] pts)
                {
                        TransformVectors (pts);
                }
        }
}
