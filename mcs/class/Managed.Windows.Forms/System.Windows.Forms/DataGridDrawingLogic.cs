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
		private Rectangle client_area;		// ClientRectangle - BorderStyle decorations, effetive client area
		private Rectangle caption_area;
		private Rectangle parent_rows;
		private Rectangle columnshdrs_area;	// Used columns header area
		private int columnshdrs_maxwidth; 	// Total width (max width) for columns headrs
		private Rectangle rowshdrs_area;	// Used Headers rows area
		private int rowshdrs_maxheight; 	// Total height for rows (max height)
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

			for (int col = 0; col < cnt; col++) {
				width += grid.CurrentTableStyle.GridColumnStyles[col].Width;
			}

			return width;
		}

		// Gets a column from a pixel
		public int FromPixelToColumn (int pixel)
		{
			int width = 0;
			int cnt = grid.CurrentTableStyle.GridColumnStyles.Count;

			if (cnt == 0)
				return 0;

			for (int col = 0; col < cnt; col++) {
				width += grid.CurrentTableStyle.GridColumnStyles[col].Width;

				if (pixel < width)
					return col;
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

		public void CalcClientArea ()
		{
			client_area = grid.ClientRectangle;
			client_area.X += BorderStyleSize;
			client_area.Y += BorderStyleSize;
			client_area.Width -= BorderStyleSize * 2;
			client_area.Height -= BorderStyleSize * 2;

			//Console.WriteLine ("CalcClientArea ClientRectangle {0}, ClientArea {1}, BorderStyleSize {2}",
			//	 grid.ClientRectangle, client_area, BorderStyleSize);
		}

		public void CalcGridAreas ()
		{
			if (grid.IsHandleCreated == false) // Delay calculations until the handle is created
				return;

			/* Order is important. E.g. row headers max. height depends on caption */
			CalcClientArea ();
			CalcCaption ();
			CalcParentRows ();
			CalcRowsHeaders ();
			CalcColumnsHeader ();
			CalcCellsArea ();

			if (SetUpVerticalScrollBar ()) { // We need a Vertical ScrollBar
				if (grid.caption_visible) {
					caption_area.Width -= grid.vert_scrollbar.Width;
				}

				if (grid.parentrows_visible) {
					parent_rows.Width -= grid.vert_scrollbar.Width;
				}

				if (grid.columnheaders_visible) {
					if (columnshdrs_area.X + columnshdrs_area.Width > grid.vert_scrollbar.Location.X) {
						columnshdrs_area.Width -= grid.vert_scrollbar.Width;
					}
				}

				if (cells_area.X + cells_area.Width > grid.vert_scrollbar.Location.X) {
					cells_area.Width -= grid.vert_scrollbar.Width;
				}
			}

			if (SetUpHorizontalScrollBar ()) { // We need a Horizontal ScrollBar
				cells_area.Height -= grid.horiz_scrollbar.Height;

				if (rowshdrs_area.Y + rowshdrs_area.Height > client_area.Y + client_area.Height) {
					rowshdrs_area.Height -= grid.horiz_scrollbar.Width;
					rowshdrs_maxheight -= grid.horiz_scrollbar.Width;
				}
			}

			// Reajust scrollbars to avoid overlapping at the corners
			if (grid.vert_scrollbar.Visible && grid.horiz_scrollbar.Visible) {
				grid.horiz_scrollbar.Width -= grid.vert_scrollbar.Width;
				grid.vert_scrollbar.Height -= grid.horiz_scrollbar.Height;
			}

			UpdateVisibleColumn ();

			//Console.WriteLine ("DataGridDrawing.CalcGridAreas cells:{0}", cells_area);
		}

		public void CalcCaption ()
		{
			if (grid.caption_visible == false) {
				caption_area = Rectangle.Empty;
				return;
			}

			caption_area.X = client_area.X;
			caption_area.Y = client_area.Y;
			caption_area.Width = client_area.Width;
			caption_area.Height = grid.CaptionFont.Height + 6;

			//Console.WriteLine ("DataGridDrawing.CalcCaption {0}", caption_area);
		}

		public void CalcCellsArea ()
		{
			if (grid.caption_visible) {
				cells_area.Y = caption_area.Y + caption_area.Height;
			} else {
				cells_area.Y = client_area.Y;
			}

			if (grid.parentrows_visible) {
				cells_area.Y += parent_rows.Height;
			}

			if (grid.columnheaders_visible) {
				cells_area.Y += columnshdrs_area.Height;
			}

			cells_area.X = client_area.X + rowshdrs_area.Width;
			cells_area.Width = client_area.X + client_area.Width - cells_area.X;
			cells_area.Height = client_area.Y + client_area.Height - cells_area.Y;

			//Console.WriteLine ("DataGridDrawing.CalcCellsArea {0}", cells_area);
		}

		public void CalcColumnsHeader ()
		{
			int width_all_cols, max_width_cols;

			if (grid.columnheaders_visible == false) {
				columnshdrs_area = Rectangle.Empty;
				return;
			}

			if (grid.caption_visible) {
				columnshdrs_area.Y = caption_area.Y + caption_area.Height;
			} else {
				columnshdrs_area.Y = client_area.Y;
			}

			if (grid.parentrows_visible) {
				columnshdrs_area.Y += parent_rows.Height;
			}

			columnshdrs_area.X = client_area.X;
			columnshdrs_area.Height = ColumnsHeaderHeight;
			width_all_cols = CalcAllColumnsWidth ();

			// TODO: take into account Scrollbars
			columnshdrs_maxwidth = client_area.X + client_area.Width - columnshdrs_area.X;
			max_width_cols = columnshdrs_maxwidth;

			if (grid.rowheaders_visible) {
				max_width_cols -= grid.RowHeaderWidth;
			}

			if (width_all_cols > max_width_cols) {
				columnshdrs_area.Width = columnshdrs_maxwidth;
			} else {
				columnshdrs_area.Width = width_all_cols;

				if (grid.rowheaders_visible) {
					columnshdrs_area.Width += grid.RowHeaderWidth;
				}
			}

			//Console.WriteLine ("DataGridDrawing.CalcColumnsHeader {0}", columnshdrs_area);
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
				parent_rows.Y = client_area.Y;
			}

			parent_rows.X = client_area.X;
			parent_rows.Width = client_area.Width;
			parent_rows.Height = grid.CaptionFont.Height + 3;

			//Console.WriteLine ("DataGridDrawing.CalcParentRows {0}", parent_rows);
		}

		public void CalcRowsHeaders ()
		{
			if (grid.rowheaders_visible == false) {
				rowshdrs_area = Rectangle.Empty;
				return;
			}

			UpdateVisibleRowCount ();

			if (grid.caption_visible) {
				rowshdrs_area.Y = caption_area.Y + caption_area.Height;
			} else {
				rowshdrs_area.Y = client_area.Y;
			}

			if (grid.parentrows_visible) {
				rowshdrs_area.Y += parent_rows.Height;
			}

			if (grid.columnheaders_visible) { // first block is painted by ColumnHeader
				rowshdrs_area.Y += ColumnsHeaderHeight;
			}

			rowshdrs_area.X = client_area.X;
			rowshdrs_area.Width = grid.RowHeaderWidth;
			rowshdrs_area.Height = grid.visiblerow_count * grid.RowHeight;
			rowshdrs_maxheight = client_area.Height + client_area.Y - rowshdrs_area.Y;

			//Console.WriteLine ("DataGridDrawing.CalcRowsHeaders {0} {1}", rowshdrs_area,
			//	rowshdrs_maxheight);
		}

		public void UpdateVisibleColumn ()
		{
			int col;
			int max_pixel = grid.horz_pixeloffset + cells_area.Width;
			grid.first_visiblecolumn = FromPixelToColumn (grid.horz_pixeloffset);

			col = FromPixelToColumn (max_pixel);
			grid.visiblecolumn_count = (col - grid.first_visiblecolumn);

			if (grid.visiblecolumn_count > 0)
				grid.visiblecolumn_count++;

			//Console.WriteLine ("UpdateVisibleColumn col: {0}, cnt {1}",
			//	grid.first_visiblecolumn, grid.visiblecolumn_count);
		}

		public void UpdateVisibleRowCount ()
		{
			int max_height = client_area.Height - caption_area.Height -
				parent_rows.Height - columnshdrs_area.Height;

			int rows_height = (grid.RowsCount - grid.first_visiblerow) * grid.RowHeight;
			int max_rows = max_height / grid.RowHeight;
			
			if (max_rows > grid.RowsCount) {
				max_rows = grid.RowsCount;
			}

			if (rows_height > cells_area.Height) {
				grid.visiblerow_count = max_height / grid.RowHeight;
			} else {
				grid.visiblerow_count = grid.RowsCount;
			}

			if (grid.visiblerow_count + grid.first_visiblerow > grid.RowsCount)
				grid.visiblerow_count = grid.RowsCount - grid.first_visiblerow;
				
			if (grid.visiblerow_count < max_rows) {
				grid.visiblerow_count = max_rows;
				grid.first_visiblerow = grid.RowsCount - max_rows;
				grid.Invalidate ();
			}
		}

		// From Point to Cell
		public DataGrid.HitTestInfo HitTest (int x, int y)
		{
			DataGrid.HitTestInfo hit = new DataGrid.HitTestInfo ();

			// TODO: Add missing ColumnResize and RowResize checks
			if (columnshdrs_area.Contains (x, y)) {
				hit.type = DataGrid.HitTestType.ColumnHeader;
				return hit;
			}

			if (rowshdrs_area.Contains (x, y)) {
				hit.type = DataGrid.HitTestType.RowHeader;
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
				pos_y = cells_area.Y + ((row - grid.FirstVisibleRow) * grid.RowHeight);

				if (y <= pos_y + grid.RowHeight) { // Found row
					hit.row = row;
					hit.type = DataGrid.HitTestType.Cell;
					int cnt = grid.CurrentTableStyle.GridColumnStyles.Count;
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
			bounds.Height = grid.RowHeight;
			bounds.Y = cells_area.Y + ((row - grid.FirstVisibleRow) * grid.RowHeight);
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

		public void OnPaint (PaintEventArgs pe)
		{
			ThemeEngine.Current.CPDrawBorderStyle (pe.Graphics, grid.ClientRectangle, grid.border_style);

			if (pe.ClipRectangle.IntersectsWith (parent_rows)) {
				pe.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.ParentRowsBackColor), parent_rows);
			}

			PaintCaption (pe.Graphics, pe.ClipRectangle);
			PaintColumnsHdrs (pe.Graphics, pe.ClipRectangle);
			PaintRowsHeaders (pe.Graphics, pe.ClipRectangle);
			PaintRows (pe.Graphics, cells_area, pe.ClipRectangle);

			// Paint scrollBar corner
			if (grid.vert_scrollbar.Visible && grid.horiz_scrollbar.Visible) {

				Rectangle corner = new Rectangle (client_area.X + client_area.Width - grid.horiz_scrollbar.Height,
					 client_area.Y + client_area.Height - grid.horiz_scrollbar.Height,
					 grid.horiz_scrollbar.Height, grid.horiz_scrollbar.Height);

				if (pe.ClipRectangle.IntersectsWith (corner)) {
					pe.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.ParentRowsBackColor),
						corner);
				}
			}
		}

		public void PaintCaption (Graphics g, Rectangle clip)
		{
			Region modified_area =  new Region (clip);
			modified_area.Intersect (caption_area);

			g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.CaptionBackColor),
				Rectangle.Ceiling (modified_area.GetBounds (g)));

			g.DrawString (grid.CaptionText, grid.CaptionFont,
				ThemeEngine.Current.ResPool.GetSolidBrush (grid.CaptionForeColor),
				caption_area);

			modified_area.Dispose ();
		}

		public void PaintColumnsHdrs (Graphics g, Rectangle clip)
		{
			Rectangle columns_area = columnshdrs_area;

			if (grid.rowheaders_visible) { // Paint corner shared between row and column header
				Rectangle rect_bloc = columnshdrs_area;
				rect_bloc.Width = grid.RowHeaderWidth;
				rect_bloc.Height = columnshdrs_area.Height;
				if (clip.IntersectsWith (rect_bloc)) {
					g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.ParentRowsBackColor), rect_bloc);
				}

				columns_area.X += grid.RowHeaderWidth;
				columns_area.Width -= grid.RowHeaderWidth;
			}

			// Set unused area
			Rectangle columnshdrs_area_complete = columns_area;
			columnshdrs_area_complete.Width = columnshdrs_maxwidth;
			Region not_usedarea = new Region (clip);

			if (grid.rowheaders_visible) {
				columnshdrs_area_complete.Width -= grid.RowHeaderWidth;
			}

			not_usedarea.Intersect (columnshdrs_area_complete);

			// Set column painting
			Rectangle rect_columnhdr = new Rectangle ();
			int cnt = grid.CurrentTableStyle.GridColumnStyles.Count, col_pixel;
			Region prev_clip = g.Clip, current_clip;
			rect_columnhdr.Y = columns_area.Y;
			rect_columnhdr.Height = columns_area.Height;

			int column_cnt = grid.first_visiblecolumn + grid.visiblecolumn_count;
			for (int column = grid.first_visiblecolumn; column < column_cnt; column++) {

				col_pixel = GetColumnStartingPixel (column);
				rect_columnhdr.X = columns_area.X + col_pixel - grid.horz_pixeloffset;
				rect_columnhdr.Width = grid.CurrentTableStyle.GridColumnStyles[column].Width;

				if (clip.IntersectsWith (rect_columnhdr) == false)
					continue;

				not_usedarea.Exclude (rect_columnhdr);
				current_clip = new Region (columns_area);
				g.Clip = current_clip;

				grid.CurrentTableStyle.GridColumnStyles[column].PaintHeader (g, rect_columnhdr, column);

				g.Clip = prev_clip;
				current_clip.Dispose ();
			}

			// This fills with background colourt the unused part in the row headers
			g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.BackgroundColor),
				Rectangle.Ceiling (not_usedarea.GetBounds (g)));

			//Console.WriteLine ("PaintColumnsHdrs Clean {0}", Rectangle.Ceiling (not_usedarea.GetBounds (g)));

			not_usedarea.Dispose ();
		}

		public void PaintRowsHeaders (Graphics g, Rectangle clip)
		{
			Rectangle rowshdrs_area_complete = rowshdrs_area;
			rowshdrs_area_complete.Height = rowshdrs_maxheight;
			Rectangle rect_row = new Rectangle ();
			rect_row.X = rowshdrs_area.X;
			Region not_usedarea = new Region (clip);
			not_usedarea.Intersect (rowshdrs_area_complete);

			int rowcnt = grid.FirstVisibleRow + grid.VisibleRowCount;
			for (int row = grid.FirstVisibleRow; row < rowcnt; row++) {

				rect_row.Width = rowshdrs_area.Width;
				rect_row.Height = grid.RowHeight;
				rect_row.Y = rowshdrs_area.Y + ((row - grid.FirstVisibleRow) * grid.RowHeight);

				if (clip.IntersectsWith (rect_row)) {
					PaintRowHeader (g, rect_row, row);
					not_usedarea.Exclude (rect_row);
				}
			}

			// This fills with background colourt the unused part in the row headers
			g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.BackgroundColor),
				Rectangle.Ceiling (not_usedarea.GetBounds (g)));

			not_usedarea.Dispose ();
		}

		public void InvalidateRowHeader (int row)
		{
			Rectangle rect_rowhdr = new Rectangle ();
			rect_rowhdr.X = rowshdrs_area.X;
			rect_rowhdr.Width = rowshdrs_area.Width;
			rect_rowhdr.Height = grid.RowHeight;
			rect_rowhdr.Y = rowshdrs_area.Y + ((row - grid.FirstVisibleRow) * grid.RowHeight);
			grid.Invalidate (rect_rowhdr);
		}

		public void PaintRowHeader (Graphics g, Rectangle bounds, int row)
		{
			// Background
			g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.ParentRowsBackColor),
				bounds);

			// Paint Borders
			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonHilight),
				bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y);

			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonHilight),
				bounds.X, bounds.Y + 2, bounds.X, bounds.Y + bounds.Height - 2);

			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow),
				bounds.X + bounds.Width - 1, bounds.Y + 2 , bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 2);

			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow),
				bounds.X, bounds.Y + bounds.Height -1, bounds.X + bounds.Width, bounds.Y  + bounds.Height -1);

			// Draw arrow
			if (row == grid.CurrentCell.RowNumber) {

				if (grid.is_changing == true) {
					g.DrawString ("...", grid.Font,
						ThemeEngine.Current.ResPool.GetSolidBrush (Color.Black),
						bounds);

				} else {
					int cx = 16;
					int cy = 16;
					Bitmap	bmp = new Bitmap (cx, cy);
					Graphics gr = Graphics.FromImage (bmp);
					Rectangle rect_arrow = new Rectangle (0, 0, cx, cy);
					ControlPaint.DrawMenuGlyph (gr, rect_arrow, MenuGlyph.Arrow);
					bmp.MakeTransparent ();
					g.DrawImage (bmp, bounds.X + 1, bounds.Y + 1, cx, cy);
					gr.Dispose ();
					bmp.Dispose ();
				}
			}
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

		public void PaintRows (Graphics g, Rectangle cells, Rectangle clip)
		{
			Rectangle rect_row = new Rectangle ();
			rect_row.X = cells.X;
			Region not_usedarea = new Region (clip);
			not_usedarea.Intersect (cells_area);

			int rowcnt = grid.FirstVisibleRow + grid.VisibleRowCount;
			for (int row = grid.FirstVisibleRow; row < rowcnt; row++) {
				// Get cell's width for column style
				rect_row.Width = cells.Width;
				rect_row.Height = grid.RowHeight;
				rect_row.Y = cells.Y + ((row - grid.FirstVisibleRow) * grid.RowHeight);

				if (clip.IntersectsWith (rect_row)) {
					PaintRow (g, row, rect_row);
					not_usedarea.Exclude (rect_row);
				}
			}

			g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.BackgroundColor),
				Rectangle.Ceiling (not_usedarea.GetBounds (g)));

			not_usedarea.Dispose ();
		}

		public void PaintRow (Graphics g, int row, Rectangle row_rect)
		{
			//Console.WriteLine ("PaintRow row: {0}, rect {1}", row, row_rect);

			Rectangle rect_cell = new Rectangle ();
			int cnt = grid.CurrentTableStyle.GridColumnStyles.Count;
			int col_pixel;
			Region prev_clip = g.Clip;
			Region current_clip;
			Region not_usedarea = new Region (row_rect);

			rect_cell.Y = row_rect.Y;
			rect_cell.Height = row_rect.Height;

			// PaintCells at row, column
			int column_cnt = grid.first_visiblecolumn + grid.visiblecolumn_count;
			for (int column = grid.first_visiblecolumn; column < column_cnt; column++) {

				col_pixel = GetColumnStartingPixel (column);

				rect_cell.X = row_rect.X + col_pixel - grid.horz_pixeloffset;
				rect_cell.Width = grid.CurrentTableStyle.GridColumnStyles[column].Width;

				current_clip = new Region (row_rect);
				g.Clip = current_clip;

				g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.BackgroundColor), rect_cell);
				g.DrawRectangle (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow), rect_cell);

				not_usedarea.Exclude (rect_cell);
				grid.CurrentTableStyle.GridColumnStyles[column].Paint (g, rect_cell, grid.ListManager, row);

				g.Clip = prev_clip;
				current_clip.Dispose ();
			}

			g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.BackgroundColor),
				Rectangle.Ceiling (not_usedarea.GetBounds (g)));

			not_usedarea.Dispose ();

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

			grid.horiz_scrollbar.Location = new Point (client_area.X, client_area.Y +
				client_area.Height - grid.horiz_scrollbar.Height);

			grid.horiz_scrollbar.Size = new Size (client_area.Width,
				grid.horiz_scrollbar.Height);

			//grid.horiz_scrollbar.Maximum = width_all - cells_area.Width;
			//grid.horiz_scrollbar.LargeChange = cells_area.Width;
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

			grid.vert_scrollbar.Location = new Point (client_area.X +
				client_area.Width - grid.vert_scrollbar.Width, client_area.Y);

			grid.vert_scrollbar.Size = new Size (grid.vert_scrollbar.Width,
				client_area.Height);

			grid.vert_scrollbar.Maximum = grid.RowsCount;
			grid.vert_scrollbar.LargeChange = cells_area.Height / grid.RowHeight;

			grid.Controls.Add (grid.vert_scrollbar);
			grid.vert_scrollbar.Visible = true;
			return true;
		}

		#endregion // Public Instance Methods

		#region Instance Properties
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

		// Returns the ColumnsHeader area excluding the rectangle shared with RowsHeader
		public Rectangle ColumnsHeadersArea {
			get {
				Rectangle columns_area = columnshdrs_area;

				if (grid.rowheaders_visible) {
					columns_area.X += grid.RowHeaderWidth;
					columns_area.Width -= grid.RowHeaderWidth;
				}
				return columns_area;
			}
		}

		public int ColumnsHeaderHeight {
			get {
				return grid.Font.Height + 6;
			}
		}

		public Rectangle RowsHeadersArea {
			get {
				return rowshdrs_area;
			}
		}

		#endregion Instance Properties
	}
}
