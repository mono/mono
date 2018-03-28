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

using System.ComponentModel;
using System.Globalization;
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
			_x = _y = 0.0;
			_width = size.Width;
			_height = size.Height;
		}

		public Rect (Point point, Vector vector) : this (point, Point.Add (point, vector))
		{ }

		public Rect (Point point1, Point point2)
		{
			if (point1.X < point2.X) {
				_x = point1.X;
				_width = point2.X - point1.X;
			}
			else {
				_x = point2.X;
				_width = point1.X - point2.X;
			}

			if (point1.Y < point2.Y) {
				_y = point1.Y;
				_height = point2.Y - point1.Y;
			}
			else {
				_y = point2.Y;
				_height = point1.Y - point2.Y;
			}
		}

		public Rect (double x, double y, double width, double height)
		{
			if (width < 0 || height < 0)
				throw new ArgumentException ("width and height must be non-negative.");
			this._x = x;
			this._y = y;
			this._width = width;
			this._height = height;
		}

		public Rect (Point location, Size size)
		{
			_x = location.X;
			_y = location.Y;
			_width = size.Width;
			_height = size.Height;
		}

		public bool Equals (Rect value)
		{
			return (_x == value.X &&
				_y == value.Y &&
				_width == value.Width &&
				_height == value.Height);
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
			unchecked
			{
				var hashCode = _x.GetHashCode ();
				hashCode = (hashCode * 397) ^ _y.GetHashCode ();
				hashCode = (hashCode * 397) ^ _width.GetHashCode ();
				hashCode = (hashCode * 397) ^ _height.GetHashCode ();
				return hashCode;
			}
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
			_x -= width;
			_y -= height;

			this._width += 2*width;
			this._height += 2*height;
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
			double _x = Math.Max (this._x, rect._x);
			double _y = Math.Max (this._y, rect._y);
			double _width = Math.Min (Right, rect.Right) - _x;
			double _height = Math.Min (Bottom, rect.Bottom) - _y; 

			if (_width < 0 || _height < 0) {
				this._x = this._y = Double.PositiveInfinity;
				this._width = this._height = Double.NegativeInfinity;
			}
			else {
				this._x = _x;
				this._y = _y;
				this._width = _width;
				this._height = _height;
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
			_x += offsetX;
			_y += offsetY;
		}

		public static Rect Offset(Rect rect, double offsetX, double offsetY)
		{
			Rect result = rect;
			result.Offset (offsetX, offsetY);
			return result;
		}

		public void Offset (Vector offsetVector)
		{
			_x += offsetVector.X;
			_y += offsetVector.Y;
		}

		public static Rect Offset (Rect rect, Vector offsetVector)
		{
			Rect result = rect;
			result.Offset (offsetVector);
			return result;
		}

		public void Scale(double scaleX, double scaleY)
		{
			_x *= scaleX;
			_y *= scaleY;
			_width *= scaleX;
			_height *= scaleY;
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
			
			_x = left;
			_y = top;
			_width = right - left;
			_height = bottom - top;
		}

		public void Union(Point point)
		{
			Union (new Rect (point, point));
		}

		public static Rect Parse (string source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			Rect value;
			if (source.Trim () == "Empty")
			{
				value = Empty;
			}
			else
			{
				var tokenizer = new NumericListTokenizer (source, CultureInfo.InvariantCulture);
				double x;
				double y;
				double width;
				double height;
				if (double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out x)
					&& double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out y)
					&& double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out width)
					&& double.TryParse (tokenizer.GetNextToken (), NumberStyles.Float, CultureInfo.InvariantCulture, out height))
				{
					if (!tokenizer.HasNoMoreTokens ())
					{
						throw new InvalidOperationException ("Invalid Rect format: " + source);
					}
					value = new Rect (x, y, width, height);
				}
				else
				{
					throw new FormatException (string.Format ("Invalid Rect format: {0}", source));
				}
			}
			return value;
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

			var separator = NumericListTokenizer.GetSeparator (provider);

			var rectFormat = string.Format (
				"{{0:{0}}}{1}{{1:{0}}}{1}{{2:{0}}}{1}{{3:{0}}}",
				format, separator);
			return string.Format (provider, rectFormat,
				_x, _y, _width, _height);
		}

		public static Rect Empty { 
			get {
				Rect r = new Rect ();
				r._x = r._y = Double.PositiveInfinity;
				r._width = r._height = Double.NegativeInfinity;
				return r;
			} 
		}
		
		public bool IsEmpty { 
			get {
				return (_x == Double.PositiveInfinity &&
					_y == Double.PositiveInfinity &&
					_width == Double.NegativeInfinity &&
					_height == Double.NegativeInfinity);
			}
		}
		
		public Point Location { 
			get {
				return new Point (_x, _y);
			}
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				_x = value.X;
				_y = value.Y;
			}
		}
		
		public Size Size { 
			get { 
				if (IsEmpty)
					return Size.Empty; 
				return new Size (_width, _height);
			}
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				_width = value.Width;
				_height = value.Height;
			}
		}

		public double X {
			get { return _x; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				_x = value;
			}
		}

		public double Y {
			get { return _y; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				_y = value;
			}
		}

		public double Width {
			get { return _width; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				if (value < 0)
					throw new ArgumentException ("width must be non-negative.");

				_width = value;
			}
		}

		public double Height {
			get { return _height; }
			set {
				if (IsEmpty)
					throw new InvalidOperationException ("Cannot modify this property on the Empty Rect.");

				if (value < 0)
					throw new ArgumentException ("height must be non-negative.");

				_height = value;
			}
		}

		public double Left { 
			get { return _x; }
		}

		public double Top { 
			get { return _y; }
		}
		
		public double Right { 
			get { return _x + _width; }
		}
		
		public double Bottom { 
			get { return _y + _height; }
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
		
		double _x;
		double _y;
		double _width;
		double _height;
	}
}
