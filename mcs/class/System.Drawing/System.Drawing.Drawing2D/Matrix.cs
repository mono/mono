//
// System.Drawing.Drawing2D.Matrix.cs
//
// Author:
//   Stefan Maierhofer <sm@cg.tuwien.ac.at>
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Drawing.Drawing2D
{
    public sealed class Matrix : MarshalByRefObject, IDisposable
    {
        // initialize to identity
        private float[] m = {1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f};
        
        // constructors
        public Matrix() { }
        
        /* TODO: depends on System.Drawing.Drawing2D.Rectangle
        public Matrix(Rectangle rect , Point[] plgpts)
        {
            // TODO
        }
        */
        
        /* TODO: depends on System.Drawing.Drawing2D.RectangleF
        public Matrix(RectangleF rect , PointF[] pa)
        {
            // TODO
        }
        */
        public Matrix(float m11, float m12, 
                      float m21, float m22, 
                      float dx, float dy)
        {
            m[0] = m11; m[1] = m12;
            m[2] = m21; m[3] = m22;
            m[4] = dx; m[5] = dy;
        }
        
        // properties
        public float[] Elements
        {
            get { return m; }
        }
        
        public bool IsIdentity
        {
            get 
            {
                if ( (m[0] == 1.0f) && (m[1] == 0.0f) &&
                     (m[2] == 0.0f) && (m[3] == 1.0f) &&
                     (m[4] == 0.0f) && (m[5] == 0.0f) )
                    return true;
                else 
                    return false;
            }
        }
        
        public bool IsInvertible
        {
            get 
            { 
                // matrix M is invertible if det(M) != 0
                float det = m[0] * m[3] - m[2] * m[1];
                if (det != 0.0f) return true;
                else return false;
            }
        }
        
        public float OffsetX
        {
            get { return m[4]; }
        }
        
        public float OffsetY
        {
            get { return m[5]; }
        }
        
        // methods
        public Matrix Clone()
        {
            return new Matrix(m[0], m[1], m[2], m[3], m[4], m[5]);
        }
        
        public void Dispose() { }
        
        public override bool Equals(object obj)
        {
            if (obj is Matrix)
            {
                float[] a = ((Matrix)obj).Elements;
                if ( m[0] == a[0] && m[1] == a[1] &&
                     m[2] == a[2] && m[3] == a[3] &&
                     m[4] == a[4] && m[5] == a[5] ) 
                    return true;
                else 
                    return false;
            }
            else
            {
                return false;
            }
        }
        
        ~Matrix() {}
        
        [StructLayout(LayoutKind.Explicit)]
        internal struct BitConverter 
        {
            [FieldOffset(0)] public float f;
            [FieldOffset(0)] public int i;
        }
        
        public override int GetHashCode()
        {
            BitConverter b;
            // compiler is not smart
            b.i = 0;
            int h = 0;
            for (int i = 0; i < 6; i++) 
            {
                b.f = m[i];
                h ^= b.i >> i;
            }
            return h;
        }
        
        public void Invert()
        {
            float det = m[0] * m[3] - m[2] * m[1];
            if (det != 0.0f)    // if invertible
            {
                float[] r = 
                {
                    m[3] / det, 
                    -m[1] / det,
                    -m[2] / det,
                     m[0] / det,
                    (-m[3] * m[4] + m[1] * m[5]) / det,
                    (m[2] * m[4] - m[0] * m[5]) / det
                };
                m = r;
            }
        }
        
        public void Multiply(Matrix matrix)
        {
            Multiply(matrix, MatrixOrder.Prepend);
        }
        
        public void Multiply(Matrix matrix, MatrixOrder order)
        {
            switch (order)
            {
                case MatrixOrder.Prepend:
                    // this = matrix * this
                    float[] p = matrix.Elements;
                    float[] r0 = 
                    {
                        p[0] * m[0] + p[1] * m[2],
                        p[0] * m[1] + p[1] * m[3],
                        p[2] * m[0] + p[3] * m[2],
                        p[2] * m[1] + p[3] * m[3],
                        p[4] * m[0] + p[5] * m[2] + m[4],
                        p[4] * m[1] + p[5] * m[3] + m[5]
                    };
                    m = r0;
                    break;
                case MatrixOrder.Append:
                    // this = this * matrix
                    float[] a = matrix.Elements;
                    float[] r1 = 
                    {
                        m[0] * a[0] + m[1] * a[2],
                        m[0] * a[1] + m[1] * a[3],
                        m[2] * a[0] + m[3] * a[2],
                        m[2] * a[1] + m[3] * a[3],
                        m[4] * a[0] + m[5] * a[2] + a[4],
                        m[4] * a[1] + m[5] * a[3] + a[5]
                    };
                    m = r1;
                    break;
            }
        }
        
        public void Reset()
        {
            m[0] = 1.0f; m[1] = 0.0f;
            m[2] = 0.0f; m[3] = 1.0f;
            m[4] = 0.0f; m[5] = 0.0f;
        }
        
        public void Rotate(float angle)
        {
            Rotate(angle, MatrixOrder.Prepend);
        }
        
        public void Rotate(float angle, MatrixOrder order)
        {
            angle *= (float)(Math.PI / 180.0);  // degrees to randians
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            switch (order)
            {
                case MatrixOrder.Prepend:
                    // this = rotation * this
                    float[] r0 = 
                    {
                         cos * m[0] + sin * m[2],
                         cos * m[1] + sin * m[3],
                        -sin * m[0] + cos * m[2],
                        -sin * m[1] + cos * m[3],
                        m[4],
                        m[5]
                    };
                    m = r0;
                    break;
                case MatrixOrder.Append:
                    // this = this * rotation
                    float[] r1 = 
                    {
                        m[0] * cos + m[1] * -sin,
                        m[0] * sin + m[1] *  cos,
                        m[2] * cos + m[3] * -sin,
                        m[2] * sin + m[3] *  cos,
                        m[4] * cos + m[5] * -sin,
                        m[4] * sin + m[5] *  cos
                    };
                    m = r1;
                    break;
            }
        }
        
        public void RotateAt(float angle, PointF point)
        {
            RotateAt(angle, point, MatrixOrder.Prepend);
        }
        
        public void RotateAt(float angle, PointF point, MatrixOrder order)
        {
            angle *= (float)(Math.PI / 180.0);  // degrees to randians
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            float e4 = -point.X * cos + point.Y * sin + point.X;
            float e5 = -point.X * sin - point.Y * cos + point.Y;
            switch (order)
            {
                case MatrixOrder.Prepend:
                    // this = rotation * this
                    float[] r0 = 
                    {
                        cos * m[0] + sin * m[2],
                        cos * m[1] + sin * m[3],
                        -sin * m[0] + cos * m[2],
                        -sin * m[1] + cos * m[3],
                        e4 * m[0] + e5 * m[2] + m[4],
                        e4 * m[1] + e5 * m[3] + m[5]
                    };
                    m = r0;
                    break;
                case MatrixOrder.Append:
                    // this = this * rotation
                    float[] r1 = 
                    {
                        m[0] * cos + m[1] * -sin,
                        m[0] * sin + m[1] * cos,
                        m[2] * cos + m[3] * -sin,
                        m[2] * sin + m[3] * cos,
                        m[4] * cos + m[5] * -sin + e4,
                        m[4] * sin + m[5] * cos + e5
                    };
                    m = r1;
                    break;
            }
        }
        
        public void Scale(float scaleX, float scaleY)
        {
            Scale(scaleX, scaleY, MatrixOrder.Prepend);
        }
        
        public void Scale(float scaleX, float scaleY, MatrixOrder order)
        {
            switch (order)
            {
                case MatrixOrder.Prepend:
                    // this = scale * this
                    m[0] *= scaleX; m[1] *= scaleX;
                    m[2] *= scaleY; m[3] *= scaleY;
                    break;
                case MatrixOrder.Append:
                    // this = this * scale
                    m[0] *= scaleX; m[1] *= scaleY;
                    m[2] *= scaleX; m[3] *= scaleY;
                    m[4] *= scaleX; m[5] *= scaleY;
                    break;
            }
        }
        
        public void Shear(float shearX, float shearY)
        {
            Shear(shearX, shearY, MatrixOrder.Prepend);
        }
        
        // LAMESPEC: quote from beta 2 sdk docs: "[To be supplied!]"
        //
        // assuming transformation matrix:
        //
        //      (1       shearY  0)
        //      (shearX  1       0)
        //      (0       0       1)
        //
        public void Shear(float shearX, float shearY, MatrixOrder order)
        {
            switch (order)
            {
                case MatrixOrder.Prepend:
                    // this = shear * this
                    float[] r0 = 
                    {
                        m[0] + shearY * m[2],
                        m[1] + shearY * m[3],
                        shearX * m[0] + m[2],
                        shearX * m[1] + m[3],
                        m[4],
                        m[5]
                    };
                    m = r0;
                    break;
                case MatrixOrder.Append:
                    // this = this * shear
                    float[] r1 = 
                    {
                        m[0] + m[1] * shearX,
                        m[0] * shearY + m[1],
                        m[2] + m[3] * shearX,
                        m[2] * shearY + m[3],
                        m[4] + m[5] * shearX ,
                        m[4] * shearY + m[5]
                    };
                    m = r1;
                    break;
            }
        }
        
        public void TransformPoints(Point[] pts)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                float x = (float)pts[i].X;
                float y = (float)pts[i].Y;
                pts[i].X = (int)(x * m[0] + y * m[2] + m[4]);
                pts[i].Y = (int)(x * m[1] + y * m[3] + m[5]);
            }
        }
        
        public void TransformPoints(PointF[] pts)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].X;
                float y = pts[i].Y;
                pts[i].X = x * m[0] + y * m[2] + m[4];
                pts[i].Y = x * m[1] + y * m[3] + m[5];
            }
        }
        
        public void TransformVectors(Point[] pts)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                float x = (float)pts[i].X;
                float y = (float)pts[i].Y;
                pts[i].X = (int)(x * m[0] + y * m[2]);
                pts[i].Y = (int)(x * m[1] + y * m[3]);
            }
        }
        
        public void TransformVectors(PointF[] pts)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].X;
                float y = pts[i].Y;
                pts[i].X = x * m[0] + y * m[2];
                pts[i].Y = x * m[1] + y * m[3];
            }
        }
        
        public void Translate(float offsetX, float offsetY)
        {
            Translate(offsetX, offsetY, MatrixOrder.Prepend);
        }
        
        public void Translate(float offsetX, float offsetY, MatrixOrder order)
        {
            switch (order)
            {
                case MatrixOrder.Prepend:
                    // this = translation * this
                    m[4] = offsetX * m[0] + offsetY * m[2] + m[4];
                    m[5] = offsetX * m[1] + offsetY * m[3] + m[5];
                    break;
                case MatrixOrder.Append:
                    // this = this * translation
                    m[4] += offsetX;
                    m[5] += offsetY;
                    break;
            }
        }
        
        // LAMESPEC: quote from beta 2 sdk docs: "[To be supplied!]"
//	[MonoTODO]    
	public void VectorTransformPoints(Point[] pts)
        {
            // TODO
        }
        
        // some simple test (TODO: remove)
        /*
        public static void Main()
        {
            PointF[] p = {new PointF(1.0f, 2.0f)};
            Console.WriteLine("(" + p[0].X + " " + p[0].Y + ")");
            Matrix m = new Matrix();
            
            m.Translate(1.0f, 1.0f); 
            m.Scale(2.0f, 2.0f); 
            m.Rotate(180.0f);
            
            m.TransformPoints(p);
            Console.WriteLine("(" + p[0].X + " " + p[0].Y + ")");
            m.Invert();
            m.TransformPoints(p);
            Console.WriteLine("(" + p[0].X + " " + p[0].Y + ")");
            
            Matrix a = new Matrix(1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f);
            Matrix b = new Matrix(2.0f, 0.0f, 0.0f, 2.0f, 0.0f, 0.0f);
            
            Console.WriteLine("h(a) = " + a.GetHashCode());
            Console.WriteLine("h(b) = " + b.GetHashCode());
        }
        */
        
    }
}
