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
using System.Globalization;
using System.Windows.Markup;
using System.Windows.Media.Converters;
using System.Windows.Threading;

namespace System.Windows.Media {

	[Serializable] 
	[TypeConverter (typeof(MatrixConverter))] 
	[ValueSerializer (typeof (MatrixValueSerializer))]
	public struct Matrix : IFormattable {

		double _m11;
		double _m12;
		double _m21;
		double _m22;
		double _offsetX;
		double _offsetY;

		public Matrix (double m11,
			       double m12,
			       double m21,
			       double m22,
			       double offsetX,
			       double offsetY)
		{
			this._m11 = m11;
			this._m12 = m12;
			this._m21 = m21;
			this._m22 = m22;
			this._offsetX = offsetX;
			this._offsetY = offsetY;
		}

		public void Append (Matrix matrix)
		{
			double _m11;
			double _m21;
			double _m12;
			double _m22;
			double _offsetX;
			double _offsetY;

			_m11 = this._m11 * matrix.M11 + this._m12 * matrix.M21;
			_m12 = this._m11 * matrix.M12 + this._m12 * matrix.M22;
			_m21 = this._m21 * matrix.M11 + this._m22 * matrix.M21;
			_m22 = this._m21 * matrix.M12 + this._m22 * matrix.M22;

			_offsetX = this._offsetX * matrix.M11 + this._offsetY * matrix.M21 + matrix.OffsetX;
			_offsetY = this._offsetX * matrix.M12 + this._offsetY * matrix.M22 + matrix.OffsetY;

			this._m11 = _m11;
			this._m12 = _m12;
			this._m21 = _m21;
			this._m22 = _m22;
			this._offsetX = _offsetX;
			this._offsetY = _offsetY;
		}

		public bool Equals (Matrix value)
		{
			return (_m11 == value.M11 &&
				_m12 == value.M12 &&
				_m21 == value.M21 &&
				_m22 == value.M22 &&
				_offsetX == value.OffsetX &&
				_offsetY == value.OffsetY);
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
			unchecked
			{
				var hashCode = _m11.GetHashCode ();
				hashCode = (hashCode * 397) ^ _m12.GetHashCode ();
				hashCode = (hashCode * 397) ^ _m21.GetHashCode ();
				hashCode = (hashCode * 397) ^ _m22.GetHashCode ();
				hashCode = (hashCode * 397) ^ _offsetX.GetHashCode ();
				hashCode = (hashCode * 397) ^ _offsetY.GetHashCode ();
				return hashCode;
			}
		}

		public void Invert ()
		{
			if (!HasInverse)
				throw new InvalidOperationException ("Transform is not invertible.");

			double d = Determinant;

			/* 1/(ad-bc)[d -b; -c a] */

			double _m11 = this._m22;
			double _m12 = -this._m12;
			double _m21 = -this._m21;
			double _m22 = this._m11;

			double _offsetX = this._m21 * this._offsetY - this._m22 * this._offsetX;
			double _offsetY = this._m12 * this._offsetX - this._m11 * this._offsetY;

			this._m11 = _m11 / d;
			this._m12 = _m12 / d;
			this._m21 = _m21 / d;
			this._m22 = _m22 / d;
			this._offsetX = _offsetX / d;
			this._offsetY = _offsetY / d;
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
			if (source == null)
				throw new ArgumentNullException ("source");
			Matrix value;
			if (source.Trim () == "Identity")
			{
				value = Identity;
			}
			else
			{
				var tokenizer = new NumericListTokenizer (source, CultureInfo.InvariantCulture);
				double m11;
				double m12;
				double m21;
				double m22;
				double offsetX;
				double offsetY;
				if (double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out m11)
				    && double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out m12)
				    && double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out m21)
				    && double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out m22)
				    && double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out offsetX)
				    && double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out offsetY))
				{
					if (!tokenizer.HasNoMoreTokens ())
					{
						throw new InvalidOperationException ("Invalid Matrix format: " + source);
					}
					value = new Matrix (m11, m12, m21, m22, offsetX, offsetY);
				}
				else
				{
					throw new FormatException (string.Format ("Invalid Matrix format: {0}", source));
				}
			}
			return value;
		}

		public void Prepend (Matrix matrix)
		{
			double _m11;
			double _m21;
			double _m12;
			double _m22;
			double _offsetX;
			double _offsetY;

			_m11 = matrix.M11 * this._m11 + matrix.M12 * this._m21;
			_m12 = matrix.M11 * this._m12 + matrix.M12 * this._m22;
			_m21 = matrix.M21 * this._m11 + matrix.M22 * this._m21;
			_m22 = matrix.M21 * this._m12 + matrix.M22 * this._m22;

			_offsetX = matrix.OffsetX * this._m11 + matrix.OffsetY * this._m21 + this._offsetX;
			_offsetY = matrix.OffsetX * this._m12 + matrix.OffsetY * this._m22 + this._offsetY;

			this._m11 = _m11;
			this._m12 = _m12;
			this._m21 = _m21;
			this._m22 = _m22;
			this._offsetX = _offsetX;
			this._offsetY = _offsetY;
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
			_m11 = _m22 = 1.0;
			_m12 = _m21 = 0.0;
			_offsetX = _offsetY = 0.0;
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

		public override string ToString ()
		{
			return ToString (null);
		}

		public string ToString (IFormatProvider provider)
		{
			return ToString (null, provider);
		}

		string IFormattable.ToString (string format,
			IFormatProvider provider)
		{
			return ToString (provider);
		}

		private string ToString (string format, IFormatProvider provider)
		{
			if (IsIdentity)
				return "Identity";

			if (provider == null)
				provider = CultureInfo.CurrentCulture;

			if (format == null)
				format = string.Empty;

			var separator = NumericListTokenizer.GetSeparator (provider);

			var matrixFormat = string.Format (
				"{{0:{0}}}{1}{{1:{0}}}{1}{{2:{0}}}{1}{{3:{0}}}{1}{{4:{0}}}{1}{{5:{0}}}",
				format, separator);
			return string.Format (provider, matrixFormat,
				_m11, _m12, _m21, _m22, _offsetX, _offsetY);
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
			this._offsetX += offsetX;
			this._offsetY += offsetY;
		}

		public void TranslatePrepend (double offsetX,
					      double offsetY)
		{
			Matrix m = Matrix.Identity;
			m.Translate (offsetX, offsetY);
			Prepend (m);
		}

		public double Determinant {
			get { return _m11 * _m22 - _m12 * _m21; }
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
			get { return _m11; }
			set { _m11 = value; }
		}
		public double M12 { 
			get { return _m12; }
			set { _m12 = value; }
		}
		public double M21 { 
			get { return _m21; }
			set { _m21 = value; }
		}
		public double M22 { 
			get { return _m22; }
			set { _m22 = value; }
		}
		public double OffsetX { 
			get { return _offsetX; }
			set { _offsetX = value; }
		}
		public double OffsetY { 
			get { return _offsetY; }
			set { _offsetY = value; }
		}
	}

}
