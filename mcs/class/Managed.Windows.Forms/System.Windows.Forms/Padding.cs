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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Pedro Martínez Juliá <pedromj@gmail.com>
//


#if NET_2_0

using System.Drawing;

namespace System.Windows.Forms {

	[SerializableAttribute()]
	public struct Padding {

		private int bottom;
		private int left;
		private int right;
		private int top;
		private bool initialized;

		public Padding (int all) {
			Bottom = Left = Right = Top = all;
		}

		public Padding (int left, int top, int right, int bottom) {
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public static readonly Padding Empty = new Padding();

		public int All {
			get {
				if (initialized && left == top && left == right && left == bottom) {
					return left;
				}
				return -1;
			}
			set { Left = Top = Right = Bottom = value; }
		}

		public int Bottom {
			get { return bottom; }
			set { bottom = value; initialized = true; }
		}

		public int Horizontal {
			get { return left + right; }
		}

		public int Left {
			get { return left; }
			set { left = value; initialized = true; }
		}

		public int Right {
			get { return right; }
			set { right = value; initialized = true; }
		}

		public Size Size {
			get { return new Size(left + right, top + bottom); }
		}

		public int Top {
			get { return top; }
			set { top = value; initialized = true; }
		}

		public int Vertical {
			get { return top + bottom; }
		}

		public static Padding Add (Padding p1, Padding p2) {
			return op_Addition(p1, p2);
		}

		public override bool Equals (object other) {
			if (other is Padding) {
				Padding other_aux = (Padding) other;
				return this.left == other_aux.left &&
					this.top == other_aux.top &&
					this.right == other_aux.right &&
					this.bottom == other_aux.bottom;
			}
			return false;
		}

		public override int GetHashCode () {
			////////////////////////////// COMPROBAR EN Windows /////////////////////////
			return top ^ bottom ^ left ^ right;
		}

		public static Padding op_Addition (Padding p1, Padding p2) {
			return new Padding(p1.Left + p2.Left, p1.Top + p2.Top, p1.Right + p2.Right, p1.Bottom + p2.Bottom);
		}

		public static bool op_Equality (Padding p1, Padding p2) {
			return p1.Equals(p2);
		}

		public static bool op_Inequality (Padding p1, Padding p2) {
			return !(p1.Equals(p2));
		}

		public static Padding op_Subtraction (Padding p1, Padding p2) {
			return new Padding(p1.Left - p2.Left, p1.Top - p2.Top, p1.Right - p2.Right, p1.Bottom - p2.Bottom);
		}

		public static Padding Subtract (Padding p1, Padding p2) {
			return op_Subtraction(p1, p2);
		}

		public override string ToString () {
			return "{Left=" + Left + ",Top="+ Top + ",Right=" + Right + ",Bottom=" + Bottom + "}"; 
		}
	}

}

#endif
