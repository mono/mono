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

using System;
using System.ComponentModel;
using System.Windows.Converters;
using System.Windows.Markup;

namespace System.Windows {

	[Serializable]
	[TypeConverter (typeof(Int32RectConverter))]
	[ValueSerializer (typeof(Int32RectValueSerializer))]
	public struct Int32Rect : IFormattable
	{
		int x, y, width, height;

		public Int32Rect (int x, int y, int width, int height)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
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
			get { return height; }
			set { height = value; }
		}

		public bool IsEmpty {
			get { return width == 0 && height == 0; }
		}

		public int Width {
			get { return width; }
			set { width = value; }
		}

		public int X {
			get { return x; }
			set { x = value; }
		}

		public int Y {
			get { return y; }
			set { y = value; }
		}

		public bool Equals (Int32Rect value)
		{
			return (x == value.x &&
				y == value.y &&
				width == value.width &&
				height == value.height);
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
			throw new NotImplementedException ();
		}

		public static Int32Rect Parse (string source)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			if (IsEmpty)
				return "Empty";
			return String.Format ("{0},{1},{2},{3}", x, y, width, height);
		}

		public string ToString (IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}

		string IFormattable.ToString (string format, IFormatProvider provider)
		{
			throw new NotImplementedException ();
		}
	}
}

