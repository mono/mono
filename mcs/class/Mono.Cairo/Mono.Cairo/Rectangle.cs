//
// Mono.Cairo.Rectangle.cs
//
// Author:
//   John Luke (john.luke@gmail.com)
//
// (C) John Luke 2005.
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

namespace Cairo
{
	public struct Rectangle
	{
		double x;
		double y;
		double width;
		double height;
		
		public Rectangle (double x, double y, double width, double height)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}
		
		public Rectangle (Point point, double width, double height)
		{
			x = point.X;
			y = point.Y;
			this.width = width;
			this.height = height;
		}
		
		public double X {
			get { return x; }
		}
		
		public double Y {
			get { return y; }
		}
		
		public double Width {
			get { return width; }
		}
		
		public double Height {
			get { return height; }
		}
		
		public override bool Equals (object obj)
		{
			if (obj is Rectangle)
				return this == (Rectangle)obj;
			return false;
		}
		
		public override int GetHashCode ()
		{
			return (int) (x + y + width + height);
		}

		public override string ToString ()
		{
			return String.Format ("x:{0} y:{1} w:{2} h:{3}", x, y, width, height);
		}
		
		public static bool operator == (Rectangle rectangle, Rectangle other)
		{
			return rectangle.X == other.X && rectangle.Y == other.Y && rectangle.Width == other.Width && rectangle.Height == other.Height;
		}
		
		public static bool operator != (Rectangle rectangle, Rectangle other)
		{
			return !(rectangle == other);
		}
	}
}
