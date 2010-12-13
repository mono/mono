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

using System.ComponentModel;

namespace System.Windows.Forms {

	public class DataGridViewSortCompareEventArgs : HandledEventArgs {

		private DataGridViewColumn dataGridViewColumn;
		private object cellValue1;
		private object cellValue2;
		private int rowIndex1;
		private int rowIndex2;
		private int sortResult = 0;

		public DataGridViewSortCompareEventArgs (DataGridViewColumn dataGridViewColumn, object cellValue1, object cellValue2, int rowIndex1, int rowIndex2) {
			this.dataGridViewColumn = dataGridViewColumn;
			this.cellValue1 = cellValue1;
			this.cellValue2 = cellValue2;
			this.rowIndex1 = rowIndex1;
			this.rowIndex2 = rowIndex2;
		}

		public object CellValue1 {
			get { return cellValue1; }
		}

		public object CellValue2 {
			get { return cellValue2; }
		}

		public DataGridViewColumn Column {
			get { return dataGridViewColumn; }
		}

		public int RowIndex1 {
			get { return rowIndex1; }
		}

		public int RowIndex2 {
			get { return rowIndex2; }
		}

		public int SortResult {
			get { return sortResult; }
			set { sortResult = value; }
		}

	}

}

