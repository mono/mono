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
			this.x = x;
			this.y = y;
		}

		public double X {
			get { return x; }
			set { x = value; }
		}

		public double Y {
			get { return y; }
			set { y = value; }
		}

		public override bool Equals (object o)
		{
			if (!(o is Point))
				return false;
			return Equals ((Point)o);
		}

		public bool Equals (Point value)
		{
			return x == value.X && y == value.Y;
		}

		public override int GetHashCode ()
		{
		    return (x.GetHashCode() ^ y.GetHashCode());
		}


		public void Offset (double offsetX, double offsetY)
		{
			x += offsetX;
			y += offsetY;
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
			string[] points = source.Split(',');

			if (points.Length<2)
				throw new InvalidOperationException ("source does not contain two numbers");
			if (points.Length > 2)
				throw new InvalidOperationException ("source contains too many delimiters");

			CultureInfo ci = CultureInfo.InvariantCulture;
			return new Point (Convert.ToDouble(points[0],ci), Convert.ToDouble(points[1],ci));	
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
			CultureInfo ci = (CultureInfo)formatProvider;

			if (ci == null)
				ci = CultureInfo.CurrentCulture;
			string seperator = ci.NumberFormat.NumberDecimalSeparator;
			if (seperator.Equals(","))
				seperator = ";";
			else
				seperator = ",";
			object[] ob = { this.x, seperator, this.y };

			return string.Format(formatProvider, "{0:" + format + "}{1}{2:" + format + "}", ob);
		}

		string IFormattable.ToString (string format, IFormatProvider formatProvider)
		{
			return this.ToString(format, formatProvider);
		}

		double x;
		double y;
	}
}
