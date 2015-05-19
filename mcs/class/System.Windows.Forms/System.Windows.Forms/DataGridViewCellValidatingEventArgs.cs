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

	public class DataGridViewCellValidatingEventArgs : CancelEventArgs {

		private int columnIndex;
		private object formattedValue;
		private int rowIndex;

		internal DataGridViewCellValidatingEventArgs (int columnIndex, int rowIndex, object formattedValue)
		{
			this.columnIndex = columnIndex;
			this.rowIndex = rowIndex;
			this.formattedValue = formattedValue;
		}

		public int ColumnIndex {
			get { return columnIndex; }
		}

		public object FormattedValue {
			get { return formattedValue; }
		}

		public int RowIndex {
			get { return rowIndex; }
		}

	}

}
