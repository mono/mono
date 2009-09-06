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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//

using System;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Media.Converters;
using System.Windows.Threading;

namespace System.Windows.Media {

	[Serializable] 
	[TypeConverter (typeof(MatrixConverter))] 
	[ValueSerializer (typeof (MatrixValueSerializer))]
	public struct Matrix : IFormattable {

		double m11;
		double m12;
		double m21;
		double m22;
		double offsetX;
		double offsetY;

		public Matrix (double m11,
			       double m12,
			       double m21,
			       double m22,
			       double offsetX,
			       double offsetY)
		{
			this.m11 = m11;
			this.m12 = m12;
			this.m21 = m21;
			this.m22 = m22;
			this.offsetX = offsetX;
			this.offsetY = offsetY;
		}

		public void Append (Matrix matrix)
		{
			double _m11;
			double _m21;
			double _m12;
			double _m22;
			double _offsetX;
			double _offsetY;

			_m11 = m11 * matrix.M11 + m12 * matrix.M21;
			_m12 = m11 * matrix.M12 + m12 * matrix.M22;
			_m21 = m21 * matrix.M11 + m22 * matrix.M21;
			_m22 = m21 * matrix.M12 + m22 * matrix.M22;

			_offsetX = offsetX * matrix.M11 + offsetY * matrix.M21 + matrix.OffsetX;
			_offsetY = offsetX * matrix.M12 + offsetY * matrix.M22 + matrix.OffsetY;

			m11 = _m11;
			m12 = _m12;
			m21 = _m21;
			m22 = _m22;
			offsetX = _offsetX;
			offsetY = _offsetY;
		}

		public bool Equals (Matrix value)
		{
			return (m11 == value.M11 &&
				m12 == value.M12 &&
				m21 == value.M21 &&
				m22 == value.M22 &&
				offsetX == value.OffsetX &&
				offsetY == value.OffsetY);
		}

		public override bool Equals (object o)
		{
			if (!(o is Matrix))
				return false;

			return Equals ((Matrix)o);
		}

		public static bool Equals (Matrix matrix1,
					   Matrix matrix2)
		{
			return matrix1.Equals (matrix2);
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		public void Invert ()
		{
			if (!HasInverse)
				throw new InvalidOperationException ("Transform is not invertible.");

			double d = Determinant;

			/* 1/(ad-bc)[d -b; -c a] */

			double _m11 = m22;
			double _m12 = -m12;
			double _m21 = -m21;
			double _m22 = m11;

			double _offsetX = m21 * offsetY - m22 * offsetX;
			double _offsetY = m12 * offsetX - m11 * offsetY;

			m11 = _m11 / d;
			m12 = _m12 / d;
			m21 = _m21 / d;
			m22 = _m22 / d;
			offsetX = _offsetX / d;
			offsetY = _offsetY / d;
		}

		public static Matrix Multiply (Matrix trans1,
					       Matrix trans2)
		{
			Matrix m = trans1;
			m.Append (trans2);
			return m;
		}

		public static bool operator == (Matrix matrix1,
						Matrix matrix2)
		{
			return matrix1.Equals (matrix2);
		}

		public static bool operator != (Matrix matrix1,
						Matrix matrix2)
		{
			return !matrix1.Equals (matrix2);
		}

		public static Matrix operator * (Matrix trans1,
						 Matrix trans2)
		{
			Matrix result = trans1;
			result.Append (trans2);
			return result;
		}

		public static Matrix Parse (string source)
		{
			throw new NotImplementedException ();
		}

		public void Prepend (Matrix matrix)
		{
			double _m11;
			double _m21;
			double _m12;
			double _m22;
			double _offsetX;
			double _offsetY;

			_m11 = matrix.M11 * m11 + matrix.M12 * m21;
			_m12 = matrix.M11 * m12 + matrix.M12 * m22;
			_m21 = matrix.M21 * m11 + matrix.M22 * m21;
			_m22 = matrix.M21 * m12 + matrix.M22 * m22;

			_offsetX = matrix.OffsetX * m11 + matrix.OffsetY * m21 + offsetX;
			_offsetY = matrix.OffsetX * m12 + matrix.OffsetY * m22 + offsetY;

			m11 = _m11;
			m12 = _m12;
			m21 = _m21;
			m22 = _m22;
			offsetX = _offsetX;
			offsetY = _offsetY;
		}

		public void Rotate (double angle)
		{
			// R_theta==[costheta -sintheta; sintheta costheta],	
 			double theta = angle * Math.PI / 180;

			Matrix r_theta = new Matrix (Math.Cos (theta), Math.Sin(theta),
						     -Math.Sin (theta), Math.Cos(theta),
						     0, 0);

			Append (r_theta);
		}

		public void RotateAt (double angle,
				      double centerX,
				      double centerY)
		{
			Translate (-centerX, -centerY);
			Rotate (angle);
			Translate (centerX, centerY);
		}

		public void RotateAtPrepend (double angle,
					     double centerX,
					     double centerY)
		{
			Matrix m = Matrix.Identity;
			m.RotateAt (angle, centerX, centerY);
			Prepend (m);
		}

		public void RotatePrepend (double angle)
		{
			Matrix m = Matrix.Identity;
			m.Rotate (angle);
			Prepend (m);
		}

		public void Scale (double scaleX,
				   double scaleY)
		{
			Matrix scale = new Matrix (scaleX, 0,
						   0, scaleY,
						   0, 0);

			Append (scale);
		}

		public void ScaleAt (double scaleX,
				     double scaleY,
				     double centerX,
				     double centerY)
		{
			Translate (-centerX, -centerY);
			Scale (scaleX, scaleY);
			Translate (centerX, centerY);
		}

		public void ScaleAtPrepend (double scaleX,
					    double scaleY,
					    double centerX,
					    double centerY)
		{
			Matrix m = Matrix.Identity;
			m.ScaleAt (scaleX, scaleY, centerX, centerY);
			Prepend (m);
		}

		public void ScalePrepend (double scaleX,
					  double scaleY)
		{
			Matrix m = Matrix.Identity;
			m.Scale (scaleX, scaleY);
			Prepend (m);
		}

		public void SetIdentity ()
		{
			m11 = m22 = 1.0;
			m12 = m21 = 0.0;
			offsetX = offsetY = 0.0;
		}

		public void Skew (double skewX,
				  double skewY)
		{
			Matrix skew_m = new Matrix (1, Math.Tan (skewY * Math.PI / 180),
						    Math.Tan (skewX * Math.PI / 180), 1,
						    0, 0);
			Append (skew_m);
		}

		public void SkewPrepend (double skewX,
					 double skewY)
		{
			Matrix m = Matrix.Identity;
			m.Skew (skewX, skewY);
			Prepend (m);
		}

		string IFormattable.ToString (string format,
					      IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			if (IsIdentity)
				return "Identity";
			else
				return string.Format ("{0},{1},{2},{3},{4},{5}",
						      m11, m12, m21, m22, offsetX, offsetY);
		}

		public string ToString (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		public Point Transform (Point point)
		{
			return Point.Multiply (point, this);
		}

		public void Transform (Point[] points)
		{
			for (int i = 0; i < points.Length; i ++)
				points[i] = Transform (points[i]);
		}

		public Vector Transform (Vector vector)
		{
			return Vector.Multiply (vector, this);
		}

		public void Transform (Vector[] vectors)
		{
			for (int i = 0; i < vectors.Length; i ++)
				vectors[i] = Transform (vectors[i]);
		}

		public void Translate (double offsetX,
				       double offsetY)
		{
			this.offsetX += offsetX;
			this.offsetY += offsetY;
		}

		public void TranslatePrepend (double offsetX,
					      double offsetY)
		{
			Matrix m = Matrix.Identity;
			m.Translate (offsetX, offsetY);
			Prepend (m);
		}

		public double Determinant {
			get { return m11 * m22 - m12 * m21; }
		}

		public bool HasInverse {
			get { return Determinant != 0; }
		}

		public static Matrix Identity {
			get { return new Matrix (1.0, 0.0, 0.0, 1.0, 0.0, 0.0); }
		}

		public bool IsIdentity {
			get { return Equals (Matrix.Identity); }
		}

		public double M11 { 
			get { return m11; }
			set { m11 = value; }
		}
		public double M12 { 
			get { return m12; }
			set { m12 = value; }
		}
		public double M21 { 
			get { return m21; }
			set { m21 = value; }
		}
		public double M22 { 
			get { return m22; }
			set { m22 = value; }
		}
		public double OffsetX { 
			get { return offsetX; }
			set { offsetX = value; }
		}
		public double OffsetY { 
			get { return offsetY; }
			set { offsetY = value; }
		}
	}

}
