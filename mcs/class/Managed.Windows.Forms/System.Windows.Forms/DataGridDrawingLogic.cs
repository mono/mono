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
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
// Datagrid drawing logic
//

// NOT COMPLETE

using System.Drawing;

namespace System.Windows.Forms
{
	internal class DataGridDrawing
	{
		#region	Local Variables

		private DataGrid grid;

		// Areas
		private Rectangle caption_area;
		private Rectangle parent_rows;
		private Rectangle columnshdrs_area;
		private Rectangle rowshdrs_area;
		private Rectangle cells_area;
		#endregion // Local Variables


		public DataGridDrawing (DataGrid datagrid)
		{
			 grid = datagrid;
		}

		#region Public Instance Methods

		// Calc the max with of all columns
		internal int CalcAllColumnsWidth ()
		{
			int width = 0;
			int cnt = grid.CurrentTableStyle.GridColumnStyles.Count;

			for (int i = 0; i < cnt; i++) {
				width += grid.CurrentTableStyle.GridColumnStyles[i].Width;
			}

			return width;
		}

		public void CalcGridAreas ()
		{
			if (grid.IsHandleCreated == false) // Delay calculations until the handle is created
				return;

			/* Order is important. E.g. row headers max. height depends on caption */
			CalcCaption ();
			CalcParentRows ();
			CalcRowsHeaders ();
			CalcColumnsHeader ();
			CalcCellsArea ();

			if (SetUpVerticalScrollBar ()) { // We need a Vertical ScrollBar
				if (grid.caption_visible == true) {
					caption_area.Width -= grid.vert_scrollbar.Width;
				}

				if (grid.parentrows_visible == true) {
					parent_rows.Width -= grid.vert_scrollbar.Width;
				}
				
				cells_area.Width -= grid.vert_scrollbar.Width;				
			}

			if (SetUpHorizontalScrollBar ()) { // We need a Horzintal ScrollBar
				cells_area.Height -= grid.horiz_scrollbar.Height;

				if (rowshdrs_area.X + rowshdrs_area.Height > grid.ClientRectangle.Height) {
					rowshdrs_area.Height -= grid.horiz_scrollbar.Width;
				}
			}

			// Reajust scrollbars to avoid overlapping
			if (grid.vert_scrollbar.Visible && grid.horiz_scrollbar.Visible) {
				grid.horiz_scrollbar.Width -= grid.vert_scrollbar.Width;
				grid.vert_scrollbar.Height -= grid.horiz_scrollbar.Height;
			}
			
			Console.WriteLine ("DataGridDrawing.CalcGridAreas cells:{0}", cells_area);
		}

		public void CalcCaption ()
		{
			if (grid.caption_visible == false) {
				caption_area = Rectangle.Empty;
				return;
			}

			caption_area.X = BorderStyleSize;
			caption_area.Y = BorderStyleSize;
			caption_area.Width = grid.ClientRectangle.Width - BorderStyleSize - BorderStyleSize;
			caption_area.Height = grid.CaptionFont.Height + 6;

			Console.WriteLine ("DataGridDrawing.CalcCaption {0}", caption_area);
		}

		public void CalcCellsArea ()
		{
			cells_area.X = BorderStyleSize + rowshdrs_area.Width;
			cells_area.Y = BorderStyleSize + caption_area.Height + parent_rows.Height + columnshdrs_area.Height;
			cells_area.Width = grid.ClientRectangle.Width - cells_area.X - BorderStyleSize;
			cells_area.Height = grid.ClientRectangle.Height - cells_area.Y - BorderStyleSize;
			
			Console.WriteLine ("DataGridDrawing.CalcCellsArea {0}", cells_area);
		}

		public void CalcColumnsHeader ()
		{
			int width_all, max_width;

			if (grid.columnheaders_visible == false) {
				columnshdrs_area = Rectangle.Empty;
				return;
			}

			columnshdrs_area.Y = caption_area.Y + caption_area.Height + parent_rows.Height;
			columnshdrs_area.X = BorderStyleSize;

			columnshdrs_area.Height = ColumnsHeaderHeight;
			width_all = CalcAllColumnsWidth ();

			// TODO: take into account Scrollbars
			max_width = grid.ClientRectangle.Width - columnshdrs_area.X - BorderStyleSize;

			if (width_all > max_width) {
				columnshdrs_area.Width = max_width;
			} else {
				columnshdrs_area.Width = width_all;

				if (grid.rowheaders_visible) {
					if (columnshdrs_area.Width + grid.RowHeaderWidth <= max_width) {
						columnshdrs_area.Width += grid.RowHeaderWidth;
					}
				}

			}

			columnshdrs_area.Width = (max_width > width_all ) ? width_all : max_width;

			 {
				columnshdrs_area.Width += ColumnsHeaderHeight;
			}

			Console.WriteLine ("DataGridDrawing.CalcColumnsHeader {0}", columnshdrs_area);
		}

		public void CalcParentRows ()
		{
			if (grid.parentrows_visible == false) {
				parent_rows = Rectangle.Empty;
				return;
			}

			if (grid.caption_visible) {
				parent_rows.Y = caption_area.Y + caption_area.Height;

			} else {
				parent_rows.Y = BorderStyleSize;
			}

			parent_rows.X = BorderStyleSize;
			parent_rows.Width = grid.ClientRectangle.Width - BorderStyleSize - BorderStyleSize;
			parent_rows.Height = grid.CaptionFont.Height + 3;

			Console.WriteLine ("DataGridDrawing.CalcParentRows {0}", parent_rows);
		}

		public void CalcRowsHeaders ()
		{
			if (grid.rowheaders_visible == false) {
				rowshdrs_area = Rectangle.Empty;
				return;
			}

			UpdateVisibleRowCount ();

			rowshdrs_area.X = BorderStyleSize;
			rowshdrs_area.Y = BorderStyleSize + caption_area.Height + parent_rows.Height;
			rowshdrs_area.Width = grid.RowHeaderWidth;
			rowshdrs_area.Height = 1 + grid.visiblerow_count * grid.RowHeight;

			if (grid.columnheaders_visible) {
				rowshdrs_area.Y += ColumnsHeaderHeight;
			}

			Console.WriteLine ("DataGridDrawing.CalcRowsHeaders {0}", rowshdrs_area);
		}

		public void UpdateVisibleRowCount ()
		{
			int max_height = grid.ClientRectangle.Height - caption_area.Height -
				parent_rows.Height - columnshdrs_area.Height - BorderStyleSize;

			int rows_height = (grid.RowsCount - grid.first_visiblerow) * grid.RowHeight;

			if (rows_height > cells_area.Height) {
				grid.visiblerow_count = max_height / grid.RowHeight;
			} else {
				grid.visiblerow_count = grid.RowsCount;
			}
		}

		public void InvalidateCaption ()
		{
			if (caption_area.IsEmpty)
				return;

			grid.Invalidate (caption_area);
		}

		public void OnPaint (PaintEventArgs pe)
		{
			//pe.Graphics.Clear (grid.BackgroundColor);
			if (pe.ClipRectangle.IntersectsWith (caption_area)) {
				pe.Graphics.FillRectangle (new SolidBrush (grid.CaptionBackColor), caption_area);
			}

			if (pe.ClipRectangle.IntersectsWith (parent_rows)) {
				pe.Graphics.FillRectangle (new SolidBrush (grid.ParentRowsBackColor), parent_rows);
			}

			if (pe.ClipRectangle.IntersectsWith (columnshdrs_area)) {
				pe.Graphics.DrawRectangle (new Pen (Color.Green), columnshdrs_area);
			}

			pe.Graphics.DrawRectangle (new Pen (Color.Yellow), rowshdrs_area);
			pe.Graphics.DrawRectangle (new Pen (Color.Pink), cells_area);

			PaintRows (pe.Graphics, cells_area, pe.ClipRectangle);
		}

		public void PaintRows (Graphics g, Rectangle cells, Rectangle clip)
		{
			Rectangle rect_row = new Rectangle ();
			rect_row.X = cells.X;

			for (int row = 0; row < grid.VisibleRowCount; row++) {
				// Get cell's width for column style
				rect_row.Width = cells.Width;
				rect_row.Height = grid.RowHeight;
				rect_row.Y = cells.Y + ((row + grid.first_visiblerow) * grid.RowHeight);

				if (clip.IntersectsWith (rect_row)) {
					PaintRow (g, row + grid.first_visiblerow, rect_row);
				}
			}
		}

		public void PaintRow (Graphics g, int row, Rectangle row_rect)
		{
			Console.WriteLine ("PaintRow row: {0}, rect {1}", row, row_rect);
			//g.DrawString ("prova", new Font ("Arial", 12), new SolidBrush (Color.Black), row_rect);

			Rectangle rect_cell = new Rectangle ();
			int cnt = grid.CurrentTableStyle.GridColumnStyles.Count;
			int width = 0;
			rect_cell.Y = row_rect.Y;
			rect_cell.Height = row_rect.Height;

			// PaintCells at row, column
			for (int column = 0; column < cnt; column++) {
				rect_cell.X = row_rect.X + width;
				width += grid.CurrentTableStyle.GridColumnStyles[column].Width;
				rect_cell.Width = grid.CurrentTableStyle.GridColumnStyles[column].Width;
				g.FillRectangle (new SolidBrush (grid.BackgroundColor), rect_cell);
				g.DrawRectangle (new Pen (Color.Black), rect_cell);
				// Ask DataGridColumnStyle class to paint this specific cell
				grid.CurrentTableStyle.GridColumnStyles[column].Paint (g, rect_cell, grid.ListManager, row);
			}
		}

		// Return true if the scrollbar is needed
		public bool SetUpHorizontalScrollBar ()
		{
			int width_all = CalcAllColumnsWidth ();

			if (width_all <= cells_area.Width) {
				grid.horiz_scrollbar.Visible = false;
				grid.Controls.Remove (grid.horiz_scrollbar);
				return false;
			}

			grid.horiz_scrollbar.Location = new Point (grid.ClientRectangle.X, grid.ClientRectangle.Y +
				grid.ClientRectangle.Height - grid.horiz_scrollbar.Height);

			grid.horiz_scrollbar.Size = new Size (grid.ClientRectangle.Width,
				grid.horiz_scrollbar.Height);

			grid.Controls.Add (grid.horiz_scrollbar);
			grid.horiz_scrollbar.Visible = true;
			return true;
		}

		// Return true if the scrollbar is needed
		public bool SetUpVerticalScrollBar ()
		{
			if (grid.visiblerow_count == grid.RowsCount) {
				grid.vert_scrollbar.Visible = false;
				grid.Controls.Remove (grid.vert_scrollbar);
				return false;
			}

			grid.vert_scrollbar.Location = new Point (grid.ClientRectangle.X +
				grid.ClientRectangle.Width - grid.vert_scrollbar.Width, grid.ClientRectangle.Y);

			grid.vert_scrollbar.Size = new Size (grid.vert_scrollbar.Width,
				grid.ClientRectangle.Height);

			grid.vert_scrollbar.Maximum = grid.RowsCount;

			grid.Controls.Add (grid.vert_scrollbar);
			grid.vert_scrollbar.Visible = true;
			return true;
		}

		#endregion // Public Instance Methods

		#region Instance Properties
		// Temp
		internal int BorderStyleSize {
			get {

				switch (grid.border_style) {
					case BorderStyle.Fixed3D:
						return 2;
					case BorderStyle.FixedSingle:
						return 1;
					case BorderStyle.None:
					default:
						break;
					}

				return 0;
			}
		}

		public Rectangle CellsArea {
			get {
				return cells_area;
			}
		}
		
		public int ColumnsHeaderHeight {
			get {
				return grid.Font.Height + 6;
			}
		}

		#endregion Instance Properties
	}
}
