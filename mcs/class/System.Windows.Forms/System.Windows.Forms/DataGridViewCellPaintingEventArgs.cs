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
using System.Drawing;

namespace System.Windows.Forms {

	public class DataGridViewCellPaintingEventArgs : HandledEventArgs {

		private DataGridView dataGridView;
		private Graphics graphics;
		private Rectangle clipBounds; 
		private Rectangle cellBounds;
		private int rowIndex;
		private int columnIndex;
		private DataGridViewElementStates cellState;
		private object cellValue;
		private object formattedValue;
		private string errorText;
		private DataGridViewCellStyle cellStyle;
		private DataGridViewAdvancedBorderStyle advancedBorderStyle;
		private DataGridViewPaintParts paintParts;

		public DataGridViewCellPaintingEventArgs (DataGridView dataGridView, Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, int columnIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
			this.dataGridView = dataGridView;
			this.graphics = graphics;
			this.clipBounds = clipBounds;
			this.cellBounds = cellBounds;
			this.rowIndex = rowIndex;
			this.columnIndex = columnIndex;
			this.cellState = cellState;
			this.cellValue = value;
			this.formattedValue = formattedValue;
			this.errorText = errorText;
			this.cellStyle = cellStyle;
			this.advancedBorderStyle = advancedBorderStyle;
			this.paintParts = paintParts;
		}

		public DataGridViewAdvancedBorderStyle AdvancedBorderStyle {
			get { return advancedBorderStyle; }
		}

		public Rectangle CellBounds {
			get { return cellBounds; }
		}

		public DataGridViewCellStyle CellStyle {
			get { return cellStyle; }
		}

		public Rectangle ClipBounds {
			get { return clipBounds; }
		}

		public int ColumnIndex {
			get { return columnIndex; }
		}

		public string ErrorText {
			get { return errorText; }
		}

		public object FormattedValue {
			get { return formattedValue; }
		}

		public Graphics Graphics {
			get { return graphics; }
		}

		public DataGridViewPaintParts PaintParts {
			get { return paintParts; }
		}

		public int RowIndex {
			get { return rowIndex; }
		}

		public DataGridViewElementStates State {
			get { return cellState; }
		}

		public object Value {
			get { return cellValue; }
		}

		public void Paint (Rectangle clipBounds, DataGridViewPaintParts paintParts) {
			if (rowIndex < -1 || rowIndex >= dataGridView.Rows.Count)
				throw new InvalidOperationException("Invalid \"RowIndex.\"");
			if (columnIndex < -1 || columnIndex >= dataGridView.Columns.Count)
				throw new InvalidOperationException("Invalid \"ColumnIndex.\"");

			DataGridViewCell cell;

			if (rowIndex == -1 && columnIndex == -1)
				cell = dataGridView.TopLeftHeaderCell;
			else if (rowIndex == -1)
				cell = dataGridView.Columns[columnIndex].HeaderCell;
			else if (columnIndex == -1)
				cell = dataGridView.Rows[rowIndex].HeaderCell;
			else
				cell = dataGridView.Rows[rowIndex].Cells[columnIndex];

			cell.PaintInternal (graphics, clipBounds, cellBounds, rowIndex, cellState, Value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);				
		}

		public void PaintBackground (Rectangle clipBounds, bool cellsPaintSelectionBackground)
		{
			Paint (clipBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
		}
			
		[MonoInternalNote ("Needs row header cell edit pencil glyph")]
		public void PaintContent (Rectangle clipBounds)
		{
			Paint (clipBounds, DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.ContentForeground);
		}

	}

}
