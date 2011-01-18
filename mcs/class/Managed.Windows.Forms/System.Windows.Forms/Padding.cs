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
// Copyright (c) 2005,2006 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Pedro Martínez Juliá <pedromj@gmail.com>
//	Daniel Nauck    (dna(at)mono-project(dot)de)
//

using System;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {

	[Serializable]
	[TypeConverter(typeof(PaddingConverter))]
	public struct Padding {

		//NOTE: "_var" field name is required by serialization.
		private int _bottom;
		private int _left;
		private int _right;
		private int _top;
		private bool _all;

		public Padding (int all) {
			_left = all;
			_right = all;
			_top = all;
			_bottom = all;
			_all = true;
		}

		public Padding (int left, int top, int right, int bottom) {
			_left = left;
			_right = right;
			_top = top;
			_bottom = bottom;
			_all = (_left == _top) && (_left == _right) && (_left == _bottom);
		}

		public static readonly Padding Empty = new Padding(0);

		[RefreshProperties(RefreshProperties.All)]
		public int All {
			get { 
				if(!_all)
					return -1;
				else
					return _top; 
			}
			set { 
				_all = true;
				_left = _top = _right = _bottom = value;
			}
		}

		[RefreshProperties(RefreshProperties.All)]
		public int Bottom {
			get { return _bottom; }
			set { 
				_bottom = value;
				_all = false;
			}
		}

		[Browsable(false)]
		public int Horizontal {
			get { return _left + _right; }
		}

		[RefreshProperties(RefreshProperties.All)]
		public int Left {
			get { return _left; }
			set { 
				_left = value;
				_all = false;
			}
		}

		[RefreshProperties(RefreshProperties.All)]
		public int Right {
			get { return _right; }
			set { 
				_right = value;
				_all = false;
			}
		}

		[Browsable(false)]
		public Size Size {
			get { return new Size(Horizontal, Vertical); }
		}

		[RefreshProperties(RefreshProperties.All)]
		public int Top {
			get { return _top; }
			set { 
				_top = value;
				_all = false;
			}
		}

		[Browsable(false)]
		public int Vertical {
			get { return _top + _bottom; }
		}

		public static Padding Add (Padding p1, Padding p2) {
			return p1 + p2;
		}

		public override bool Equals (object other) {
			if (other is Padding) {
				Padding other_aux = (Padding) other;
				return _left == other_aux.Left &&
					_top == other_aux.Top &&
					_right == other_aux.Right &&
					_bottom == other_aux.Bottom;
			}
			return false;
		}

		public override int GetHashCode () {
			return _top ^ _bottom ^ _left ^ _right;
		}

		public static Padding operator+ (Padding p1, Padding p2) {
			return new Padding(p1.Left + p2.Left, p1.Top + p2.Top, p1.Right + p2.Right, p1.Bottom + p2.Bottom);
		}

		public static bool operator== (Padding p1, Padding p2) {
			return p1.Equals(p2);
		}

		public static bool operator!= (Padding p1, Padding p2) {
			return !(p1.Equals(p2));
		}

		public static Padding operator- (Padding p1, Padding p2) {
			return new Padding(p1.Left - p2.Left, p1.Top - p2.Top, p1.Right - p2.Right, p1.Bottom - p2.Bottom);
		}

		public static Padding Subtract (Padding p1, Padding p2) {
			return p1 - p2;
		}

		public override string ToString () {
			return "{Left=" + Left + ",Top="+ Top + ",Right=" + Right + ",Bottom=" + Bottom + "}"; 
		}
	}
}