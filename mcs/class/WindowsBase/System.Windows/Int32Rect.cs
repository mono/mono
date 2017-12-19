//
// DependencyPropertyChangedEventArgs.cs
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
// (C) 2007 Novell, Inc.
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

using System.ComponentModel;
using System.Globalization;
using System.Windows.Converters;
using System.Windows.Markup;

namespace System.Windows {

	[Serializable]
	[TypeConverter (typeof(Int32RectConverter))]
	[ValueSerializer (typeof(Int32RectValueSerializer))]
	public struct Int32Rect : IFormattable
	{
		int _x, _y, _width, _height;

		public Int32Rect (int x, int y, int width, int height)
		{
			this._x = x;
			this._y = y;
			this._width = width;
			this._height = height;
		}

		public static bool operator != (Int32Rect int32Rect1, Int32Rect int32Rect2)
		{
			return !int32Rect1.Equals(int32Rect2);
		}

		public static bool operator == (Int32Rect int32Rect1, Int32Rect int32Rect2)
		{
			return int32Rect1.Equals(int32Rect2);
		}

		public static Int32Rect Empty {
			get { return new Int32Rect (0, 0, 0, 0); }
		}

		public int Height {
			get { return _height; }
			set { _height = value; }
		}

		public bool IsEmpty {
			get { return _width == 0 && _height == 0; }
		}

		public int Width {
			get { return _width; }
			set { _width = value; }
		}

		public int X {
			get { return _x; }
			set { _x = value; }
		}

		public int Y {
			get { return _y; }
			set { _y = value; }
		}

		public bool Equals (Int32Rect value)
		{
			return (_x == value._x &&
				_y == value._y &&
				_width == value._width &&
				_height == value._height);
		}

		public override bool Equals (object o)
		{
			if (!(o is Int32Rect))
				return false;

			return Equals ((Int32Rect)o);
		}

		public static bool Equals (Int32Rect int32Rect1, Int32Rect int32Rect2)
		{
			return int32Rect1.Equals (int32Rect2);
		}

		public override int GetHashCode ()
		{
			unchecked
			{
				var hashCode = _x;
				hashCode = (hashCode * 397) ^ _y;
				hashCode = (hashCode * 397) ^ _width;
				hashCode = (hashCode * 397) ^ _height;
				return hashCode;
			}
		}

		public static Int32Rect Parse (string source)
		{
			if (source == null)
				throw new ArgumentNullException ("source");
			Int32Rect value;
			if (source.Trim () == "Empty")
			{
				value = Empty;
			}
			else
			{
				var tokenizer = new NumericListTokenizer (source, CultureInfo.InvariantCulture);
				int x;
				int y;
				int width;
				int height;
				if (int.TryParse (tokenizer.GetNextToken (), NumberStyles.Integer, CultureInfo.InvariantCulture, out x)
				    && int.TryParse (tokenizer.GetNextToken (), NumberStyles.Integer, CultureInfo.InvariantCulture, out y)
				    && int.TryParse (tokenizer.GetNextToken (), NumberStyles.Integer, CultureInfo.InvariantCulture, out width)
				    && int.TryParse (tokenizer.GetNextToken (), NumberStyles.Integer, CultureInfo.InvariantCulture, out height))
				{
					if (!tokenizer.HasNoMoreTokens ())
					{
						throw new InvalidOperationException ("Invalid Int32Rect format: " + source);
					}
					value = new Int32Rect (x, y, width, height);
				}
				else
				{
					throw new FormatException (string.Format ("Invalid Int32Rect format: {0}", source));
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
			return ToString (provider);
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
	}
}

