//
// Mono.Cairo.Matrix.cs
//
// Author: Duncan Mak
//         Hisham Mardam Bey (hisham.mardambey@gmail.com)
//         Idan Gazit (idan@fastmail.fm)
//
// (C) Ximian Inc, 2003 - 2005.
//
// This is an OO wrapper API for the Cairo API
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

	[StructLayout(LayoutKind.Sequential)]
	public class Matrix : ICloneable
	{
		public double Xx;
		public double Yx;
		public double Xy;
		public double Yy;
		public double X0;
		public double Y0;

		public Matrix (double xx, double yx, double xy, double yy,
				double x0, double y0)
		{
			this.Xx = xx; this.Yx = yx; this.Xy = xy;
			this.Yy = yy; this.X0 = x0; this.Y0 = y0;
		}

		public Matrix ()
		{
			this.InitIdentity ();
		}

		public bool IsIdentity ()
		{
			return (this == new Matrix ());
		}

		public void InitIdentity ()
		{
			// this.Init(1,0,0,1,0,0);
			NativeMethods.cairo_matrix_init_identity (this);
		}

		public void Init (double xx, double yx, double xy, double yy,
				  double x0, double y0)
		{
			this.Xx = xx; this.Yx = yx; this.Xy = xy;
			this.Yy = yy; this.X0 = x0; this.Y0 = y0;
		}

		public void InitTranslate (double tx, double ty)
		{
			//this.Init (1, 0, 0, 1, tx, ty);
			NativeMethods.cairo_matrix_init_translate (this, tx, ty);
		}

		public void Translate (double tx, double ty)
		{
			NativeMethods.cairo_matrix_translate (this, tx, ty);
		}

		public void InitScale (double sx, double sy)
		{
			//this.Init (sx, 0, 0, sy, 0, 0);
			NativeMethods.cairo_matrix_init_scale (this, sx, sy);
		}

		public void Scale (double sx, double sy)
		{
			NativeMethods.cairo_matrix_scale (this, sx, sy);
		}

		public void InitRotate (double radians)
		{
			/*
			double s, c;
			s = Math.Sin (radians);
			c = Math.Cos (radians);
			this.Init (c, s, -s, c, 0, 0);
			*/
			NativeMethods.cairo_matrix_init_rotate (this, radians);
		}

		public void Rotate (double radians)
		{
			NativeMethods.cairo_matrix_rotate (this, radians);
		}

		public Cairo.Status Invert ()
		{
			return NativeMethods.cairo_matrix_invert (this);
		}

		public void Multiply (Matrix b)
		{
			Matrix a = (Matrix) this.Clone ();
			NativeMethods.cairo_matrix_multiply (this, a, b);
		}

		public static Matrix Multiply (Matrix a, Matrix b) {
			Matrix result = new Matrix ();
			NativeMethods.cairo_matrix_multiply (result, a, b);
			return result;
		}


		public void TransformDistance (ref double dx, ref double dy)
		{
			NativeMethods.cairo_matrix_transform_distance (this, ref dx, ref dy);
		}

		public void TransformPoint (ref double x, ref double y)
		{
			NativeMethods.cairo_matrix_transform_point (this, ref x, ref y);
		}

		public override String ToString ()
		{
			String s = String.Format ("xx:{0:##0.0#} yx:{1:##0.0#} xy:{2:##0.0#} yy:{3:##0.0#} x0:{4:##0.0#} y0:{5:##0.0#}",
				this.Xx, this.Yx, this.Xy, this.Yy, this.X0, this.Y0);
			return s;
		}

		public static bool operator == (Matrix lhs, Matrix rhs)
		{
			return (lhs.Xx == rhs.Xx &&
				lhs.Xy == rhs.Xy &&
				lhs.Yx == rhs.Yx &&
				lhs.Yy == rhs.Yy &&
				lhs.X0 == rhs.X0 &&
				lhs.Y0 == rhs.Y0 );
		}

		public static bool operator != (Matrix lhs, Matrix rhs)
		{
			return !(lhs==rhs);
		}



		public override bool Equals(object o)
		{
			if (! (o is Matrix))
				return false;
			else
				return (this == (Matrix) o);
		}

		public override int GetHashCode()
		{
			return  (int)this.Xx ^ (int)this.Xx>>32 ^
				(int)this.Xy ^ (int)this.Xy>>32 ^
				(int)this.Yx ^ (int)this.Yx>>32 ^
				(int)this.Yy ^ (int)this.Yy>>32 ^
				(int)this.X0 ^ (int)this.X0>>32 ^
				(int)this.Y0 ^ (int)this.Y0>>32;
		}

		public object Clone()
		{
			return this.MemberwiseClone ();
		}

	}
}
