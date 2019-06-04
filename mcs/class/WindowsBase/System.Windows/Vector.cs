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
//	Chris Toshok (toshok@novell.com)
//

using System.ComponentModel;
using System.Globalization;
using System.Windows.Converters;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows {

	[Serializable]
	[ValueSerializer (typeof (VectorValueSerializer))]
	[TypeConverter (typeof (VectorConverter))]
	public struct Vector : IFormattable
	{
		public Vector (double x, double y)
		{
			this._x = x;
			this._y = y;
		}

		public bool Equals (Vector value)
		{
			return _x == value.X && _y == value.Y;
		}

		public override bool Equals (object o)
		{
			if (!(o is Vector))
				return false;

			return Equals ((Vector)o);
		}

		public override int GetHashCode ()
		{
			unchecked
			{
				return (_x.GetHashCode () * 397) ^ _y.GetHashCode ();
			}
		}

		public static bool Equals (Vector vector1, Vector vector2)
		{
			return vector1.Equals (vector2);
		}

		public static Point Add (Vector vector, Point point)
		{
			return new Point (vector.X + point.X, vector.Y + point.Y);
		}

		public static Vector Add (Vector vector1, Vector vector2)
		{
			return new Vector (vector1.X + vector2.X,
					   vector1.Y + vector2.Y);
		}

		public static double AngleBetween (Vector vector1, Vector vector2)
		{
			double cos_theta = (vector1.X * vector2.X + vector1.Y * vector2.Y) / (vector1.Length * vector2.Length);

			return Math.Acos (cos_theta) / Math.PI * 180;
		}

		public static double CrossProduct (Vector vector1, Vector vector2)
		{
			// ... what operation is this exactly?
			return vector1.X * vector2.Y - vector1.Y * vector2.X;
		}

		public static double Determinant (Vector vector1, Vector vector2)
		{
			// same as CrossProduct, it appears.
			return vector1.X * vector2.Y - vector1.Y * vector2.X;
		}

		public static Vector Divide (Vector vector, double scalar)
		{
			return new Vector (vector.X / scalar, vector.Y / scalar);
		}

		public static double Multiply (Vector vector1, Vector vector2)
		{
			return vector1.X * vector2.X + vector1.Y * vector2.Y;
		}

		public static Vector Multiply (Vector vector, Matrix matrix)
		{
			return new Vector (vector.X * matrix.M11 + vector.Y * matrix.M21,
					   vector.X * matrix.M12 + vector.Y * matrix.M22);
		}

		public static Vector Multiply (double scalar, Vector vector)
		{
			return new Vector (scalar * vector.X, scalar * vector.Y);
		}

		public static Vector Multiply (Vector vector, double scalar)
		{
			return new Vector (scalar * vector.X, scalar * vector.Y);
		}

		public void Negate ()
		{
			_x = -_x;
			_y = -_y;
		}

		public void Normalize ()
		{
			double ls = LengthSquared;
			if (ls == 1)
				return;

			double l = Math.Sqrt (ls);
			_x /= l;
			_y /= l;
		}

		public static Vector Subtract (Vector vector1, Vector vector2)
		{
			return new Vector (vector1.X - vector2.X, vector1.Y - vector2.Y);
		}

		public static Vector Parse (string source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			var tokenizer = new NumericListTokenizer (source, CultureInfo.InvariantCulture);
			double x;
			double y;
			if (!double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out x) ||
			    !double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out y))
			{
				throw new FormatException (string.Format ("Invalid Vector format: {0}", source));
			}
			if (!tokenizer.HasNoMoreTokens ())
			{
				throw new InvalidOperationException("Invalid Vector format: " + source);
			}
			return new Vector(x, y);
		}

		public override string ToString ()
		{
			return ToString(null);
		}

		public string ToString (IFormatProvider provider)
		{
			return ToString (null, provider);
		}

		string IFormattable.ToString (string format, IFormatProvider provider)
		{
			return ToString (format, provider);
		}

		private string ToString(string format,IFormatProvider formatProvider)
		{
			if (formatProvider == null)
				formatProvider = CultureInfo.CurrentCulture;
			if (format == null)
				format = string.Empty;
			var separator = NumericListTokenizer.GetSeparator (formatProvider);
			var vectorFormat  = string.Format ("{{0:{0}}}{1}{{1:{0}}}", format, separator);
			return string.Format (formatProvider, vectorFormat, _x, _y);
		}

		public double Length {
			get { return Math.Sqrt (LengthSquared); }
		}

		public double LengthSquared {
			get { return _x * _x + _y * _y; }
		}

		public double X {
			get { return _x; }
			set { _x = value; }
		}

		public double Y {
			get { return _y; }
			set { _y = value; }
		}

		/* operators */
		public static explicit operator Point (Vector vector)
		{
			return new Point (vector.X, vector.Y);
		}

		public static explicit operator Size (Vector vector)
		{
			return new Size (vector.X, vector.Y);
		}

		public static Vector operator - (Vector vector1, Vector vector2)
		{
			return Subtract (vector1, vector2);
		}

		public static Vector operator - (Vector vector)
		{
			Vector result = vector;
			result.Negate ();
			return result;
		}

		public static bool operator != (Vector vector1, Vector vector2)
		{
			return !Equals (vector1, vector2);
		}

		public static bool operator == (Vector vector1, Vector vector2)
		{
			return Equals (vector1, vector2);
		}

		public static double operator * (Vector vector1, Vector vector2)
		{
			return Multiply (vector1, vector2);
		}

		public static Vector operator * (Vector vector, Matrix matrix)
		{
			return Multiply (vector, matrix);
		}

		public static Vector operator * (double scalar, Vector vector)
		{
			return Multiply (scalar, vector);
		}

		public static Vector operator * (Vector vector, double scalar)
		{
			return Multiply (vector, scalar);
		}

		public static Vector operator / (Vector vector, double scalar)
		{
			return Divide (vector, scalar);
		}

		public static Point operator + (Vector vector, Point point)
		{
			return Add (vector, point);
		}

		public static Vector operator + (Vector vector1, Vector vector2)
		{
			return Add (vector1, vector2);
		}

		double _x;
		double _y;
	}

}
