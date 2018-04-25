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

using System;
using System.ComponentModel;
using System.Windows.Converters;
using System.Windows.Markup;
using System.Windows.Media;
using System.Globalization;

namespace System.Windows {

	[Serializable]
	[TypeConverter (typeof (PointConverter))]
	[ValueSerializer (typeof (PointValueSerializer))]
	public struct Point : IFormattable
	{
		public Point (double x, double y)
		{
			this._x = x;
			this._y = y;
		}

		public double X {
			get { return _x; }
			set { _x = value; }
		}

		public double Y {
			get { return _y; }
			set { _y = value; }
		}

		public override bool Equals (object o)
		{
			if (!(o is Point))
				return false;
			return Equals ((Point)o);
		}

		public bool Equals (Point value)
		{
			return _x == value.X && _y == value.Y;
		}

		public override int GetHashCode ()
		{
		    return (_x.GetHashCode() ^ _y.GetHashCode());
		}


		public void Offset (double offsetX, double offsetY)
		{
			_x += offsetX;
			_y += offsetY;
		}

		public static Point Add (Point point, Vector vector)
		{
			return new Point (point.X + vector.X, point.Y + vector.Y);
		}

		public static bool Equals (Point point1, Point point2)
		{
			return point1.Equals (point2);
		}

		public static Point Multiply (Point point, Matrix matrix)
		{
			return new Point (point.X * matrix.M11 + point.Y * matrix.M21 + matrix.OffsetX,
					  point.X * matrix.M12 + point.Y * matrix.M22 + matrix.OffsetY);
		}

		public static Vector Subtract (Point point1, Point point2)
		{
			return new Vector (point1.X - point2.X, point1.Y - point2.Y);
		}

		public static Point Subtract (Point point, Vector vector)
		{
			return new Point (point.X - vector.X, point.Y - vector.Y);
		}

		/* operators */

		public static Vector operator -(Point point1, Point point2)
		{
			return Subtract (point1, point2);
		}

		public static Point operator -(Point point, Vector vector)
		{
			return Subtract (point, vector);
		}

		public static Point operator + (Point point, Vector vector)
		{
			return Add (point, vector);
		}

		public static Point operator * (Point point, Matrix matrix)
		{
			return Multiply (point, matrix);
		}

		public static bool operator != (Point point1, Point point2)
		{
			return !point1.Equals(point2);
		}

		public static bool operator == (Point point1, Point point2)
		{
			return point1.Equals(point2);
		}

		public static explicit operator Size (Point point)
		{
			return new Size (point.X, point.Y);
		}

		public static explicit operator Vector (Point point)
		{
			return new Vector (point.X, point.Y);
		}

		public static Point Parse (string source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			var tokenizer = new NumericListTokenizer (source, CultureInfo.InvariantCulture);
			double x;
			double y;
			if (!double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out x) ||
			    !double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out y))
			{
				throw new FormatException (string.Format ("Invalid Point format: {0}", source));
			}
			if (!tokenizer.HasNoMoreTokens ())
			{
				throw new InvalidOperationException ("Invalid Point format: " + source);
			}
			return new Point(x, y);
		}

		public override string ToString ()
		{
			return this.ToString(null, null);
		}

		public string ToString (IFormatProvider provider)
		{
			return this.ToString(null, provider);
		}

		private string ToString(string format,IFormatProvider formatProvider)
		{
			if (formatProvider == null)
				formatProvider = CultureInfo.CurrentCulture;
			if (format == null)
				format = string.Empty;
			var separator = NumericListTokenizer.GetSeparator (formatProvider);
			var pointFormat  = string.Format ("{{0:{0}}}{1}{{1:{0}}}", format, separator);
			return string.Format (formatProvider, pointFormat, _x, _y);
		}

		string IFormattable.ToString (string format, IFormatProvider formatProvider)
		{
			return this.ToString(format, formatProvider);
		}

		double _x;
		double _y;
	}
}
