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

	public class DataGridViewRowPrePaintEventArgs : HandledEventArgs {

		private DataGridView dataGridView;
		private Graphics graphics;
		private Rectangle clipBounds;
		private Rectangle rowBounds;
		private int rowIndex;
		private DataGridViewElementStates rowState;
		private string errorText;
		private DataGridViewCellStyle inheritedRowStyle;
		private bool isFirstDisplayedRow;
		private bool isLastVisibleRow;
		private DataGridViewPaintParts paintParts;

		public DataGridViewRowPrePaintEventArgs (DataGridView dataGridView, Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, string errorText, DataGridViewCellStyle inheritedRowStyle, bool isFirstDisplayedRow, bool isLastVisibleRow) {
			this.dataGridView = dataGridView;
			this.graphics = graphics;
			this.clipBounds = clipBounds;
			this.rowBounds = rowBounds;
			this.rowIndex = rowIndex;
			this.rowState = rowState;
			this.errorText = errorText;
			this.inheritedRowStyle = inheritedRowStyle;
			this.isFirstDisplayedRow = isFirstDisplayedRow;
			this.isLastVisibleRow = isLastVisibleRow;
		}

		public Rectangle ClipBounds {
			get { return clipBounds; }
			set { clipBounds = value; }
		}

		public string ErrorText {
			get { return errorText; }
		}

		public Graphics Graphics {
			get { return graphics; }
		}

		public DataGridViewCellStyle InheritedRowStyle {
			get { return inheritedRowStyle; }
		}

		public bool IsFirstDisplayedRow {
			get { return isFirstDisplayedRow; }
		}

		public bool IsLastVisibleRow {
			get { return isLastVisibleRow; }
		}

		public DataGridViewPaintParts PaintParts {
			get { return paintParts; }
			set { paintParts = value; }
		}

		public Rectangle RowBounds {
			get { return rowBounds; }
		}

		public int RowIndex {
			get { return rowIndex; }
		}

		public DataGridViewElementStates State {
			get { return rowState; }
		}

		public void DrawFocus (Rectangle bounds, bool cellsPaintSelectionBackground) {
			if (rowIndex < 0 || rowIndex >= dataGridView.Rows.Count)
				throw new InvalidOperationException ("Invalid RowIndex.");

			DataGridViewRow row = dataGridView.GetRowInternal (rowIndex);

			row.PaintCells (graphics, clipBounds, bounds, rowIndex, rowState, isFirstDisplayedRow, isLastVisibleRow, DataGridViewPaintParts.Focus);
		}

		public void PaintCells (Rectangle clipBounds, DataGridViewPaintParts paintParts)
		{
			if (rowIndex < 0 || rowIndex >= dataGridView.Rows.Count)
				throw new InvalidOperationException ("Invalid RowIndex.");

			DataGridViewRow row = dataGridView.GetRowInternal (rowIndex);

			row.PaintCells (graphics, clipBounds, rowBounds, rowIndex, rowState, isFirstDisplayedRow, isLastVisibleRow, paintParts);
		}

		public void PaintCellsBackground (Rectangle clipBounds, bool cellsPaintSelectionBackground)
		{
			if (cellsPaintSelectionBackground)
				PaintCells (clipBounds, DataGridViewPaintParts.All);
			else
				PaintCells (clipBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.SelectionBackground);
		}

		public void PaintCellsContent (Rectangle clipBounds)
		{
			PaintCells (clipBounds, DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.ContentForeground);
		}

		public void PaintHeader (bool paintSelectionBackground)
		{
			if (paintSelectionBackground)
				PaintHeader (DataGridViewPaintParts.All);
			else
				PaintHeader (DataGridViewPaintParts.All & ~DataGridViewPaintParts.SelectionBackground);
		}

		public void PaintHeader (DataGridViewPaintParts paintParts)
		{
			if (rowIndex < 0 || rowIndex >= dataGridView.Rows.Count)
				throw new InvalidOperationException ("Invalid RowIndex.");

			DataGridViewRow row = dataGridView.GetRowInternal (rowIndex);
			
			row.PaintHeader (graphics, clipBounds, rowBounds, rowIndex, rowState, isFirstDisplayedRow, isLastVisibleRow, paintParts);
		}
	}
}
