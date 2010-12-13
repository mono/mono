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


using System;
using System.ComponentModel;

namespace System.Windows.Forms {

	public sealed class DataGridViewAdvancedBorderStyle : ICloneable {

		private DataGridViewAdvancedCellBorderStyle bottom;
		private DataGridViewAdvancedCellBorderStyle left;
		private DataGridViewAdvancedCellBorderStyle right;
		private DataGridViewAdvancedCellBorderStyle top;

		public DataGridViewAdvancedBorderStyle ()
		{
			All = DataGridViewAdvancedCellBorderStyle.None;
		}

		public DataGridViewAdvancedCellBorderStyle All {
			get {
				if (bottom == left && left == right && right == top) {
					return bottom;
				}
				return DataGridViewAdvancedCellBorderStyle.NotSet;
			}
			set {
				if (!Enum.IsDefined(typeof(DataGridViewAdvancedCellBorderStyle), value)) {
					throw new InvalidEnumArgumentException("Value is not valid DataGridViewAdvancedCellBorderStyle.");
				}
				bottom = left = right = top = value;
			}
		}

		public DataGridViewAdvancedCellBorderStyle Bottom {
			get { return bottom; }
			set {
				if (!Enum.IsDefined(typeof(DataGridViewAdvancedCellBorderStyle), value)) {
					throw new InvalidEnumArgumentException("Value is not valid DataGridViewAdvancedCellBorderStyle.");
				}
				if (value == DataGridViewAdvancedCellBorderStyle.NotSet) {
					throw new ArgumentException("Invlid Bottom value.");
				}
				bottom = value;
			}
		}

		public DataGridViewAdvancedCellBorderStyle Left {
			get { return left; }
			set {
				if (!Enum.IsDefined(typeof(DataGridViewAdvancedCellBorderStyle), value)) {
					throw new InvalidEnumArgumentException("Value is not valid DataGridViewAdvancedCellBorderStyle.");
				}
				if (value == DataGridViewAdvancedCellBorderStyle.NotSet) {
					throw new ArgumentException("Invlid Left value.");
				}
				/*
				if (Control.RightToLeft == true && (value == DataGridViewAdvancedCellBorderStyle.InsetDouble || value == DataGridViewAdvancedCellBorderStyle.OutsetDouble)) {
					throw new ArgumentException("Invlid Left value.");
				}
				*/
				left = value;
			}
		}

		public DataGridViewAdvancedCellBorderStyle Right {
			get { return right; }
			set {
				if (!Enum.IsDefined(typeof(DataGridViewAdvancedCellBorderStyle), value)) {
					throw new InvalidEnumArgumentException("Value is not valid DataGridViewAdvancedCellBorderStyle.");
				}
				if (value == DataGridViewAdvancedCellBorderStyle.NotSet) {
					throw new ArgumentException("Invlid Right value.");
				}
				/*
				if (Control.RightToLeft == false && (value == DataGridViewAdvancedCellBorderStyle.InsetDouble || value == DataGridViewAdvancedCellBorderStyle.OutsetDouble)) {
					throw new ArgumentException("Invlid Right value.");
				}
				*/
				right = value;
			}
		}

		public DataGridViewAdvancedCellBorderStyle Top {
			get { return top; }
			set {
				if (!Enum.IsDefined(typeof(DataGridViewAdvancedCellBorderStyle), value)) {
					throw new InvalidEnumArgumentException("Value is not valid DataGridViewAdvancedCellBorderStyle.");
				}
				if (value == DataGridViewAdvancedCellBorderStyle.NotSet) {
					throw new ArgumentException("Invlid Top value.");
				}
				top = value;
			}
		}

		public override bool Equals (object other)
		{
			if (other is DataGridViewAdvancedBorderStyle) {
				DataGridViewAdvancedBorderStyle otherAux = (DataGridViewAdvancedBorderStyle) other;
				return bottom == otherAux.bottom &&
					left == otherAux.left &&
					right == otherAux.right &&
					top == otherAux.top;
			}
			return false;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode();
		}

		public override string ToString ()
		{
			return String.Format("DataGridViewAdvancedBorderStyle { All={0}, Left={1}, Right={2}, Top={3}, Bottom={4} }", All, Left, Right, Top, Bottom);
		}

		object ICloneable.Clone ()
		{
			DataGridViewAdvancedBorderStyle result = new DataGridViewAdvancedBorderStyle();
			result.bottom = this.bottom;
			result.left = this.left;
			result.right = this.right;
			result.top = this.top;
			return result;
		}

	}

}
