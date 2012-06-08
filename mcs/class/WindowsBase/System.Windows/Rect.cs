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
//	Chris Toshok <toshok@novell.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//

using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Converters;
using System.Windows.Markup;
using System.Windows.Media;

namespace System.Windows {

	[Serializable]
	[ValueSerializer (typeof (RectValueSerializer))]
	[TypeConverter (typeof (RectConverter))]
	public struct Rect : IFormattable
	{
		public Rect (Size size)
		{
			x = y = 0.0;
			width = size.Width;
			height = size.Height;
		}

		public Rect (Point point, Vector vector) : this (point, Point.Add (point, vector))
		{ }

		public Rect (Point point1, Point point2)
		{
			if (point1.X < point2.X) {
				x = point1.X;
				width = point2.X - point1.X;
			}
			else {
				x = point2.X;
				width = point1.X - point2.X;
			}

			if (point1.Y < point2.Y) {
				y = point1.Y;
				height = point2.Y - point1.Y;
			}
			else {
				y = point2.Y;
				height = point1.Y - point2.Y;
			}
		}

		public Rect (double x, double y, double width, double height)
		{
			if (width < 0 || height < 0)
				throw new ArgumentException ("width and height must be non-negative.");
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}

		public Rect (Point location, Size size)
		{
			x = location.X;
			y = location.Y;
			width = size.Width;
			height = size.Height;
		}

		public bool Equals (Rect value)
		{
			return (x == value.X &&
				y == value.Y &&
				width == value.Width &&
				height == value.Height);
		}

		public static bool operator != (Rect rect1, Rect rect2)
		{
			return !(rect1.Location == rect2.Location && rect1.Size == rect2.Size);
		}

		public static bool operator == (Rect rect1, Rect rect2)
		{
			return rect1.Location == rect2.Location && rect1.Size == rect2.Size;
		}

		public override bool Equals (object o)
		{
			if (!(o is Rect))
				return false;

			return Equals ((Rect)o);
		}

		public static bool Equals (Rect rect1, Rect rect2)
		{
			return rect1.Equals (rect2);
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		public bool Contains (Rect rect)
		{
			if (rect.Left < this.Left ||
			    rect.Right > this.Right)
				return false;

			if (rect.Top < this.Top ||
			    rect.Bottom > this.Bottom)
				return false;

			return true;
		}

		public bool Contains (double x, double y)
		{
			if (x < Left || x > Right)
				return false;
			if (y < Top || y > Bottom)
				return false;

			return true;
		}

		public bool Contains (Point point)
		{
			return Contains (point.X, point.Y);
		}

		public static Rect Inflate (Rect rect, double width, double height)
		{
			if (width < rect.Width * -2)
				return Rect.Empty;
			if (height < rect.Height * -2)
				return Rect.Empty;

			Rect result = rect;
			result.Inflate (width, height);
			return result;
		}

		public static Rect Inflate (Rect rect, Size size)
		{
			return Rect.Inflate (rect, size.Width, size.Height);
		}

		public void Inflate (double width, double height)
		{
			// XXX any error checking like in the static case?
			x -= width;
			y -= height;

			this.width += 2*width;
			this.height += 2*height;
		}

		public void Inflate (Size size)
		{
			Inflate (size.Width, size.Height);
		}

		public bool IntersectsWith(Rect rect)
		{
			return !((Left >= rect.Right) || (Right <= rect.Left) ||
			    (Top >= rect.Bottom) || (Bottom <= rect.Top));
		}

		public void Intersect(Rect rect)
		{
			double _x = Math.Max (x, rect.x);
			double _y = Math.Max (y, rect.y);
			double _width = Math.Min (Right, rect.Right) - _x;
			double _height = Math.Min (Bottom, rect.Bottom) - _y; 

			if (_width < 0 || _height < 0) {
				x = y = Double.PositiveInfinity;
				width = height = Double.NegativeInfinity;
			}
			else {
				x = _x;
				y = _y;
				width = _width;
				height = _height;
			}
		}

		public static Rect Intersect(Rect rect1, Rect rect2)
		{
			Rect result = rect1;
			result.Intersect (rect2);
			return result;
		}

		public void Offset(double offsetX, double offsetY)
		{
			x += offsetX;
			y += offsetY;
		}

		public static Rect Offset(Rect rect, double offsetX, double offsetY)
		{
			Rect result = rect;
			result.Offset (offsetX, offsetY);
			return result;
		}

		public void Offset (Vector offsetVector)
		{
			x += offsetVector.X;
			y += offsetVector.Y;
		}

		public static Rect Offset (Rect rect, Vector offsetVector)
		{
			Rect result = rect;
			result.Offset (offsetVector);
			return result;
		}

		public void Scale(double scaleX, double scaleY)
		{
			x *= scaleX;
			y *= scaleY;
			width *= scaleX;
			height *= scaleY;
		}

		public void Transform (Matrix matrix)
		{
			throw new NotImplementedException ();
		}

		public static Rect Transform (Rect rect, Matrix matrix)
		{
			Rect result = rect;
			result.Transform (matrix);
			return result;
		}

		public static Rect Union(Rect rect1, Rect rect2)
		{
			Rect result = rect1;
			result.Union (rect2);
			return result;
		}

		public static Rect Union(Rect rect, Point point)
		{
			Rect result = rect;
			result.Union (point);
			return result;
		}
		
		public void Union(Rect rect)
		{
			var left = Math.Min (Left, rect.Left);
			var top = Math.Min (Top, rect.Top);
			var right = Math.Max (Right, rect.Right);
			var bottom = Math.Max (Bottom, rect.Bottom);
			
			x = left;
			y = top;
			width = right - left;
			height = bottom - top;
		}

		public void Union(Point point)
		{
			Union (new Rect (point, point));
		}

		public static Rect Parse (string source)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			return ToString (null);
		}

		public string ToString (IFormatProvider provider)
		{
			return ToString (null, provider);
		}

		string IFormattable.ToString (string format, IFormatProvider provider)
		{
			return ToString (format, provider);
		}

		private string ToString (string format, IFormatProvider provider)
		{
			if (IsEmpty)
				return "Empty";

			if (provider == null)
				provider = CultureInfo.CurrentCulture;

			if (format == null)
				format = string.Empty;

			string separator = ",";
			NumberFormatInfo numberFormat =
				provider.GetFormat (typeof (NumberFormatInfo)) as NumberFormatInfo;
			if (numberFormat != null &&
			    numberFormat.NumberDecimalSeparator == separator)
				separator = ";";

			string rectFormat = String.Format (
				"{{0:{0}}}{1}{{1:{0}}}{1}{{2:{0}}}{1}{{3:{0}}}",
				format, separator);
			return String.Format (provider, rectFormat,
				x, y, width, height);
		}

		public static Rect Empty { 
			get {
				Rect r = new Rect ();
				r.x = r.y = Double.PositiveInfinity;
				r.width = r.height = Double.NegativeInfinity;
				return r;
			} 
		}
		
		public bool IsEmpty { 
			get {
				return (x == Double.PositiveInfinity &&
					y == Double.PositiveInfinity &&
					width == Double.NegativeInfinity &&
					height == Double.NegativeInfinity);
			}
		}
		
		public Point Location { 
			get {
				return new Point (x, y);
			}
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				x = value.X;
				y = value.Y;
			}
		}
		
		public Size Size { 
			get { 
				if (IsEmpty)
					return Size.Empty; 
				return new Size (width, height);
			}
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				width = value.Width;
				height = value.Height;
			}
		}

		public double X {
			get { return x; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				x = value;
			}
		}

		public double Y {
			get { return y; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				y = value;
			}
		}

		public double Width {
			get { return width; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				if (value < 0)
					throw new ArgumentException ("width must be non-negative.");

				width = value;
			}
		}

		public double Height {
			get { return height; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				if (value < 0)
					throw new ArgumentException ("height must be non-negative.");

				height = value;
			}
		}

		public double Left { 
			get { return x; }
		}

		public double Top { 
			get { return y; }
		}
		
		public double Right { 
			get { return x + width; }
		}
		
		public double Bottom { 
			get { return y + height; }
		}
		
		public Point TopLeft { 
			get { return new Point (Left, Top); }
		}
		
		public Point TopRight { 
			get { return new Point (Right, Top); }
		}
		
		public Point BottomLeft { 
			get { return new Point (Left, Bottom); }
		}

		public Point BottomRight { 
			get { return new Point (Right, Bottom); }
		}
		
		double x;
		double y;
		double width;
		double height;
	}
}
