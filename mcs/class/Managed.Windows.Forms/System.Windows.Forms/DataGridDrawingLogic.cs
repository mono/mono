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
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	internal class DataGridDrawing
	{
		#region	Local Variables

		private DataGrid grid;

		// Areas
		internal Rectangle caption_area;
		internal Rectangle parent_rows;
		internal Rectangle columnhdrs_area;	// Used columns header area
		internal int columnhdrs_maxwidth; 	// Total width (max width) for columns headrs
		internal Rectangle rowhdrs_area;	// Used Headers rows area
		internal int rowhdrs_maxheight; 	// Total height for rows (max height)
		internal Rectangle cells_area;
		internal Font font_newrow = new Font (FontFamily.GenericSansSerif, 16);
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

			for (int col = 0; col < cnt; col++) {
				width += grid.CurrentTableStyle.GridColumnStyles[col].Width;
			}

			return width;
		}

		// Gets a column from a pixel
		private int FromPixelToColumn (int pixel, out int column_x)
		{
			int width = 0;
			int cnt = grid.CurrentTableStyle.GridColumnStyles.Count;
			column_x = 0;

			if (cnt == 0)
				return 0;
				
			if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
				width += rowhdrs_area.X + rowhdrs_area.Width;
				column_x += rowhdrs_area.X + rowhdrs_area.Width;
			}

			for (int col = 0; col < cnt; col++) {
				width += grid.CurrentTableStyle.GridColumnStyles[col].Width;

				if (pixel < width)
					return col;

				column_x += grid.CurrentTableStyle.GridColumnStyles[col].Width;
			}

			return cnt - 1;
		}

		//
		public int GetColumnStartingPixel (int my_col)
		{
			int width = 0;
			int cnt = grid.CurrentTableStyle.GridColumnStyles.Count;

			for (int col = 0; col < cnt; col++) {

				if (my_col == col)
					return width;

				width += grid.CurrentTableStyle.GridColumnStyles[col].Width;
			}

			return 0;
		}
		
		// Which column has to be the first visible column to ensure a column visibility
		public int GetFirstColumnForColumnVisilibility (int current_first_visiblecolumn, int column)
		{
			int new_col = column;
			int width = 0;
			
			if (column > current_first_visiblecolumn) { // Going forward								
				for (new_col = column; new_col >= 0; new_col--){
					width += grid.CurrentTableStyle.GridColumnStyles[new_col].Width;
					
					if (width >= cells_area.Width)
						return new_col + 1;
						//return new_col < grid.CurrentTableStyle.GridColumnStyles.Count ? new_col + 1 : grid.CurrentTableStyle.GridColumnStyles.Count;
				}
				return 0;
			} else {				
				return  column;
			}			
		}

		bool in_calc_grid_areas;
		public void CalcGridAreas ()
		{
			if (grid.IsHandleCreated == false) // Delay calculations until the handle is created
				return;

			/* make sure we don't happen to end up in this method again */
			if (in_calc_grid_areas)
				return;

			in_calc_grid_areas = true;

			/* Order is important. E.g. row headers max. height depends on caption */
			grid.horz_pixeloffset = 0;			
			CalcCaption ();
			CalcParentRows ();
			UpdateVisibleRowCount ();
			CalcRowHeaders (grid.visiblerow_count);
			CalcColumnHeaders ();
			CalcCellsArea ();

			bool needHoriz = false;
			bool needVert = false;

			/* figure out which scrollbars we need, and what the visible areas are */
			int visible_cells_width = cells_area.Width;
			int visible_cells_height = cells_area.Height;
			int width_of_all_columns = CalcAllColumnsWidth ();
			int allrows = grid.RowsCount;
			if (grid.ShowEditRow && grid.RowsCount > 0)
				allrows++;

			/* use a loop to iteratively calculate whether
			 * we need horiz/vert scrollbars. */
			for (int i = 0; i < 3; i ++) {
				if (needVert)
					visible_cells_width = cells_area.Width - grid.vert_scrollbar.Width;
				if (needHoriz)
					visible_cells_height = cells_area.Height - grid.horiz_scrollbar.Height;

				UpdateVisibleRowCount ();

				needHoriz = (width_of_all_columns > visible_cells_width);
				needVert = (grid.visiblerow_count != allrows);
			}

			int horiz_scrollbar_width = grid.ClientRectangle.Width;
			int horiz_scrollbar_maximum = 0;
			int vert_scrollbar_height = 0;
			int vert_scrollbar_maximum = 0;

			if (needVert)
				SetUpVerticalScrollBar (out vert_scrollbar_height, out vert_scrollbar_maximum);

			if (needHoriz)
				SetUpHorizontalScrollBar (out horiz_scrollbar_maximum);

			cells_area.Width = visible_cells_width;
			cells_area.Height = visible_cells_height;

			if (needVert && needHoriz) {
				if (grid.ShowParentRowsVisible) {
					parent_rows.Width -= grid.vert_scrollbar.Width;
				}

				if (!ShowingColumnHeaders) {
					if (columnhdrs_area.X + columnhdrs_area.Width > grid.vert_scrollbar.Location.X) {
						columnhdrs_area.Width -= grid.vert_scrollbar.Width;
					}
				}

				horiz_scrollbar_width -= grid.vert_scrollbar.Width;
				vert_scrollbar_height -= grid.horiz_scrollbar.Height;
			}

			if (needVert) {
				if (rowhdrs_area.Y + rowhdrs_area.Height > grid.ClientRectangle.Y + grid.ClientRectangle.Height) {
					rowhdrs_area.Height -= grid.horiz_scrollbar.Height;
					rowhdrs_maxheight -= grid.horiz_scrollbar.Height;
				}

				grid.vert_scrollbar.Height = vert_scrollbar_height;
				grid.vert_scrollbar.Maximum = vert_scrollbar_maximum;
				grid.Controls.Add (grid.vert_scrollbar);
				grid.vert_scrollbar.Visible = true;
			}
			else {
				grid.Controls.Remove (grid.vert_scrollbar);
				grid.vert_scrollbar.Visible = false;
			}

			if (needHoriz) {
				grid.horiz_scrollbar.Width = horiz_scrollbar_width;
				grid.horiz_scrollbar.Maximum = horiz_scrollbar_maximum;
				grid.Controls.Add (grid.horiz_scrollbar);
				grid.horiz_scrollbar.Visible = true;
			}
			else {
				grid.Controls.Remove (grid.horiz_scrollbar);
				grid.horiz_scrollbar.Visible = false;
			}

			UpdateVisibleColumn ();
			UpdateVisibleRowCount ();

			//Console.WriteLine ("DataGridDrawing.CalcGridAreas caption_area:{0}", caption_area);
			//Console.WriteLine ("DataGridDrawing.CalcGridAreas parent_rows:{0}", parent_rows);
			//Console.WriteLine ("DataGridDrawing.CalcGridAreas rowhdrs_area:{0}", rowhdrs_area);
			//Console.WriteLine ("DataGridDrawing.CalcGridAreas columnhdrs_area:{0}", columnhdrs_area);
			//Console.WriteLine ("DataGridDrawing.CalcGridAreas cells:{0}", cells_area);

			in_calc_grid_areas = false;
		}

		private void CalcCaption ()
		{
			if (grid.caption_visible == false) {
				caption_area = Rectangle.Empty;
				return;
			}

			caption_area.X = grid.ClientRectangle.X;
			caption_area.Y = grid.ClientRectangle.Y;
			caption_area.Width = grid.ClientRectangle.Width;
			caption_area.Height = grid.CaptionFont.Height + 6;

			//Console.WriteLine ("DataGridDrawing.CalcCaption {0}", caption_area);
		}

		private void CalcCellsArea ()
		{
			if (grid.caption_visible) {
				cells_area.Y = caption_area.Y + caption_area.Height;
			} else {
				cells_area.Y = grid.ClientRectangle.Y;
			}

			if (grid.ShowParentRowsVisible) {
				cells_area.Y += parent_rows.Height;
			}

			if (ShowingColumnHeaders) {
				cells_area.Y += columnhdrs_area.Height;
			}

			cells_area.X = grid.ClientRectangle.X + rowhdrs_area.Width;
			cells_area.Width = grid.ClientRectangle.X + grid.ClientRectangle.Width - cells_area.X;
			cells_area.Height = grid.ClientRectangle.Y + grid.ClientRectangle.Height - cells_area.Y;

			//Console.WriteLine ("DataGridDrawing.CalcCellsArea {0}", cells_area);
		}

		private void CalcColumnHeaders ()
		{
			int width_all_cols, max_width_cols;
			
			if (!ShowingColumnHeaders) {
				columnhdrs_area = Rectangle.Empty;				
				return;
			}

			if (grid.caption_visible) {
				columnhdrs_area.Y = caption_area.Y + caption_area.Height;
			} else {
				columnhdrs_area.Y = grid.ClientRectangle.Y;
			}

			if (grid.ShowParentRowsVisible) {
				columnhdrs_area.Y += parent_rows.Height;
			}

			columnhdrs_area.X = grid.ClientRectangle.X;
			columnhdrs_area.Height = ColumnHeadersHeight;
			width_all_cols = CalcAllColumnsWidth ();

			// TODO: take into account Scrollbars
			columnhdrs_maxwidth = grid.ClientRectangle.X + grid.ClientRectangle.Width - columnhdrs_area.X;
			max_width_cols = columnhdrs_maxwidth;

			if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
				max_width_cols -= grid.RowHeaderWidth;
			}

			if (width_all_cols > max_width_cols) {
				columnhdrs_area.Width = columnhdrs_maxwidth;
			} else {
				columnhdrs_area.Width = width_all_cols;

				if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
					columnhdrs_area.Width += grid.RowHeaderWidth;
				}
			}

			//Console.WriteLine ("DataGridDrawing.CalcColumnHeaders {0}", columnhdrs_area);
		}

		private void CalcParentRows ()
		{
			if (grid.ShowParentRowsVisible == false) {
				parent_rows = Rectangle.Empty;
				return;
			}

			if (grid.caption_visible) {
				parent_rows.Y = caption_area.Y + caption_area.Height;

			} else {
				parent_rows.Y = grid.ClientRectangle.Y;
			}

			parent_rows.X = grid.ClientRectangle.X;
			parent_rows.Width = grid.ClientRectangle.Width;
			parent_rows.Height = grid.CaptionFont.Height + 3;

			//Console.WriteLine ("DataGridDrawing.CalcParentRows {0}", parent_rows);
		}

		private void CalcRowHeaders (int visiblerow_count)
		{
			if (grid.CurrentTableStyle.CurrentRowHeadersVisible == false) {
				rowhdrs_area = Rectangle.Empty;
				return;
			}

			if (grid.caption_visible) {
				rowhdrs_area.Y = caption_area.Y + caption_area.Height;
			} else {
				rowhdrs_area.Y = grid.ClientRectangle.Y;
			}

			if (grid.ShowParentRowsVisible) {
				rowhdrs_area.Y += parent_rows.Height;
			}

			if (ShowingColumnHeaders) { // first block is painted by ColumnHeader
				rowhdrs_area.Y += ColumnHeadersHeight;
			}

			rowhdrs_area.X = grid.ClientRectangle.X;
			rowhdrs_area.Width = grid.RowHeaderWidth;
			if (visiblerow_count == 0)
				rowhdrs_area.Height = 0;
			else
				rowhdrs_area.Height = (grid.rows[visiblerow_count + grid.FirstVisibleRow - 1].VerticalOffset - grid.rows[grid.FirstVisibleRow].VerticalOffset
						       + grid.rows[visiblerow_count + grid.FirstVisibleRow - 1].Height);
			rowhdrs_maxheight = grid.ClientRectangle.Height + grid.ClientRectangle.Y - rowhdrs_area.Y;

			//Console.WriteLine ("DataGridDrawing.CalcRowHeaders {0} {1}", rowhdrs_area,
			//	rowhdrs_maxheight);
		}

		private int GetVisibleRowCount (int visibleHeight)
		{
			//			Console.Write ("GetVisibleRowCount ({0}) - ", visibleHeight);
			int total_rows = grid.RowsCount;
			
			if (grid.ShowEditRow && grid.RowsCount > 0) {
				total_rows++;
			}

			int rows_height = 0;
			int r;
			for (r = grid.FirstVisibleRow; r < grid.RowsCount; r ++) {
				//				Console.Write ("{0},", grid.rows[r].Height);
				if (rows_height + grid.rows[r].Height >= visibleHeight)
					break;
				rows_height += grid.rows[r].Height;
			}

			/* add in the edit row if it'll fit */
			if (grid.ShowEditRow && grid.RowsCount > 0 && visibleHeight - rows_height > grid.RowHeight)
				r ++;

			if (r < grid.rows.Length - 1)
				r ++;
			//			Console.WriteLine (" rows_height = {0}, returning {1}", rows_height, r - grid.FirstVisibleRow);

			return r - grid.FirstVisibleRow;
		}

		public void UpdateVisibleColumn ()
		{
			if (grid.CurrentTableStyle.GridColumnStyles.Count == 0) {
				grid.visiblecolumn_count = 0;
				return;	
			}
			
			int col;
			int max_pixel = grid.horz_pixeloffset + cells_area.Width;
			int unused;

			grid.first_visiblecolumn = FromPixelToColumn (grid.horz_pixeloffset, out unused);

			col = FromPixelToColumn (max_pixel, out unused);
			
			grid.visiblecolumn_count = 1 + col - grid.first_visiblecolumn;
			
			if (grid.first_visiblecolumn + grid.visiblecolumn_count < grid.CurrentTableStyle.GridColumnStyles.Count) { 
				grid.visiblecolumn_count++; // Partially visible column
			}
		}

		public void UpdateVisibleRowCount ()
		{
			grid.visiblerow_count = GetVisibleRowCount (cells_area.Height);

			CalcRowHeaders (grid.visiblerow_count); // Height depends on num of visible rows

			// XXX
			grid.Invalidate ();
		}

		const int RESIZE_HANDLE_HORIZ_SIZE = 5;
		const int RESIZE_HANDLE_VERT_SIZE = 3;

		// From Point to Cell
		public DataGrid.HitTestInfo HitTest (int x, int y)
		{
			DataGrid.HitTestInfo hit = new DataGrid.HitTestInfo ();

			if (columnhdrs_area.Contains (x, y)) {
				int offset_x = x + grid.horz_pixeloffset;
				int column_x;
				int column_under_mouse = FromPixelToColumn (offset_x, out column_x);
				
				if ((column_x + grid.CurrentTableStyle.GridColumnStyles[column_under_mouse].Width - offset_x < RESIZE_HANDLE_HORIZ_SIZE)
				    && column_under_mouse < grid.CurrentTableStyle.GridColumnStyles.Count) {
					hit.type = DataGrid.HitTestType.ColumnResize;
					hit.column = column_under_mouse;
				}
				else {
					hit.type = DataGrid.HitTestType.ColumnHeader;
					hit.column = column_under_mouse;
				}
				return hit;
			}

			if (rowhdrs_area.Contains (x, y)) {
				int posy;
				int rcnt = grid.FirstVisibleRow + grid.VisibleRowCount;
				for (int r = grid.FirstVisibleRow; r < rcnt; r++) {
					posy = cells_area.Y + grid.rows[r].VerticalOffset - grid.rows[grid.FirstVisibleRow].VerticalOffset;
					if (y <= posy + grid.rows[r].Height) {
						if ((posy + grid.rows[r].Height) - y < RESIZE_HANDLE_VERT_SIZE) {
							hit.type = DataGrid.HitTestType.RowResize;
						}
						else {
							hit.type = DataGrid.HitTestType.RowHeader;
						}
						hit.row = r;
						break;
					}
				}
				return hit;
			}

			if (caption_area.Contains (x, y)) {
				hit.type = DataGrid.HitTestType.Caption;
				return hit;
			}

			if (parent_rows.Contains (x, y)) {
				hit.type = DataGrid.HitTestType.ParentRows;
				return hit;
			}

			int pos_y, pos_x, width;
			int rowcnt = grid.FirstVisibleRow + grid.VisibleRowCount;
			for (int row = grid.FirstVisibleRow; row < rowcnt; row++) {

				pos_y = cells_area.Y + grid.rows[row].VerticalOffset - grid.rows[grid.FirstVisibleRow].VerticalOffset;
				if (y <= pos_y + grid.rows[row].Height) {
					hit.row = row;
					hit.type = DataGrid.HitTestType.Cell;					
					int col_pixel;
					int column_cnt = grid.first_visiblecolumn + grid.visiblecolumn_count;
					for (int column = grid.first_visiblecolumn; column < column_cnt; column++) {

						col_pixel = GetColumnStartingPixel (column);
						pos_x = cells_area.X + col_pixel - grid.horz_pixeloffset;
						width = grid.CurrentTableStyle.GridColumnStyles[column].Width;

						if (x <= pos_x + width) { // Column found
							hit.column = column;
							break;
						}
					}

					break;
				}
			}

			return hit;
		}

		public Rectangle GetCellBounds (int row, int col)
		{
			Rectangle bounds = new Rectangle ();
			int col_pixel;

			bounds.Width = grid.CurrentTableStyle.GridColumnStyles[col].Width;
			bounds.Height = grid.rows[row].Height;
			bounds.Y = cells_area.Y + grid.rows[row].VerticalOffset - grid.rows[grid.FirstVisibleRow].VerticalOffset;
			col_pixel = GetColumnStartingPixel (col);
			bounds.X = cells_area.X + col_pixel - grid.horz_pixeloffset;
			return bounds;
		}

		public void InvalidateCaption ()
		{
			if (caption_area.IsEmpty)
				return;

			grid.Invalidate (caption_area);
		}

		public void InvalidateCells ()
		{
			if (cells_area.IsEmpty)
				return;

			grid.Invalidate (cells_area);
		}
		
		public void InvalidateRow (int row)
		{
			if (row < grid.FirstVisibleRow || row > grid.FirstVisibleRow + grid.VisibleRowCount) {
				return;
			}

			Rectangle rect_row = new Rectangle ();

			int row_width = CalcAllColumnsWidth ();
			if (row_width > cells_area.Width)
				row_width = cells_area.Width;
			rect_row.X = cells_area.X;
			rect_row.Width = row_width;
			rect_row.Height = grid.rows[row].Height;
			rect_row.Y = cells_area.Y + grid.rows[row].VerticalOffset - grid.rows[grid.FirstVisibleRow].VerticalOffset;
			grid.Invalidate (rect_row);
		}

		public void InvalidateRowHeader (int row)
		{
			Rectangle rect_rowhdr = new Rectangle ();
			rect_rowhdr.X = rowhdrs_area.X;
			rect_rowhdr.Width = rowhdrs_area.Width;
			rect_rowhdr.Height = grid.rows[row].Height;
			rect_rowhdr.Y = rowhdrs_area.Y + grid.rows[row].VerticalOffset - grid.rows[grid.FirstVisibleRow].VerticalOffset;
			grid.Invalidate (rect_rowhdr);
		}	

		public void InvalidateColumn (DataGridColumnStyle column)
		{
			Rectangle rect_col = new Rectangle ();
			int col_pixel;
			int col = -1;

			col = grid.CurrentTableStyle.GridColumnStyles.IndexOf (column);

			if (col == -1) {
				return;
			}

			rect_col.Width = column.Width;
			col_pixel = GetColumnStartingPixel (col);
			rect_col.X = cells_area.X + col_pixel - grid.horz_pixeloffset;
			rect_col.Y = cells_area.Y;
			rect_col.Height = cells_area.Height;
			grid.Invalidate (rect_col);
		}

		public void DrawResizeLineVert (int x)
		{
			XplatUI.DrawReversibleRectangle (grid.Handle,
							 new Rectangle (x, cells_area.Y, 1, cells_area.Height - 3),
							 2);
		}

		public void DrawResizeLineHoriz (int y)
		{
			XplatUI.DrawReversibleRectangle (grid.Handle,
							 new Rectangle (cells_area.X, y, cells_area.Width - 3, 1),
							 2);
		}

		void SetUpHorizontalScrollBar (out int maximum)
		{
			maximum = CalcAllColumnsWidth ();

			grid.horiz_scrollbar.Location = new Point (grid.ClientRectangle.X, grid.ClientRectangle.Y +
				grid.ClientRectangle.Height - grid.horiz_scrollbar.Height);

			grid.horiz_scrollbar.Size = new Size (grid.ClientRectangle.Width,
				grid.horiz_scrollbar.Height);

			grid.horiz_scrollbar.LargeChange = cells_area.Width;
		}


		void SetUpVerticalScrollBar (out int height, out int maximum)
		{
			int y;
			
			if (grid.caption_visible) {
				y = grid.ClientRectangle.Y + caption_area.Height;
				height = grid.ClientRectangle.Height - caption_area.Height;
			} else {
				y = grid.ClientRectangle.Y;
				height = grid.ClientRectangle.Height;
			}

			grid.vert_scrollbar.Location = new Point (grid.ClientRectangle.X +
				grid.ClientRectangle.Width - grid.vert_scrollbar.Width, y);

			grid.vert_scrollbar.Size = new Size (grid.vert_scrollbar.Width,
				height);

			maximum = grid.RowsCount;
			
			if (grid.ShowEditRow && grid.RowsCount > 0) {
				maximum++;	
			}
			
			grid.vert_scrollbar.LargeChange = VLargeChange;
		}

		#endregion // Public Instance Methods

		#region Instance Properties
		public Rectangle CellsArea {
			get {
				return cells_area;
			}
		}

		// Returns the ColumnHeaders area excluding the rectangle shared with RowHeaders
		public Rectangle ColumnHeadersArea {
			get {
				Rectangle columns_area = columnhdrs_area;

				if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
					columns_area.X += grid.RowHeaderWidth;
					columns_area.Width -= grid.RowHeaderWidth;
				}
				return columns_area;
			}
		}

		bool ShowingColumnHeaders {
			get { return grid.columnheaders_visible != false && grid.CurrentTableStyle.GridColumnStyles.Count > 0; }
		}

		int ColumnHeadersHeight {
			get {
				return grid.CurrentTableStyle.HeaderFont.Height + 6;
			}
		}

		public Rectangle RowHeadersArea {
			get {
				return rowhdrs_area;
			}
		}

		public int VLargeChange {
			get {
				return grid.VisibleRowCount;
			}
		}

		#endregion Instance Properties
	}
}
