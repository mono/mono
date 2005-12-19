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

namespace System.Windows.Forms {

	public class DataGridViewTextBoxColumn : DataGridViewColumn {

		private int maxInputLength;
		private DataGridViewColumnSortMode sortMode;

		public DataGridViewTextBoxColumn () {
			base.CellTemplate = new DataGridViewTextBoxCell();
			maxInputLength = 32767;
			sortMode = DataGridViewColumnSortMode.Automatic;
		}

		public override DataGridViewCell CellTemplate {
			get { return base.CellTemplate; }
			set { base.CellTemplate = value as DataGridViewTextBoxCell; }
		}

		public int MaxInputLength {
			get { return maxInputLength; }
			set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException("Value is less than 0.");
				}
				maxInputLength = value;
			}
		}

		public new DataGridViewColumnSortMode SortMode {
			get { return sortMode; }
			set {
				if (DataGridView != null && DataGridView.SelectionMode == DataGridViewSelectionMode.FullRowSelect && value != DataGridViewColumnSortMode.NotSortable) {
					throw new InvalidOperationException("Value conflicts with DataGridView.SelectionMode.");
				}
				sortMode = value;
			}
		}

		public override string ToString () {
			return GetType().Name;
		}



	}

}

#endif
