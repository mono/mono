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
		static private Font font_newrow = new Font (FontFamily.GenericSansSerif, 16);
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

		public void CalcClientArea ()
		{
			client_area = grid.ClientRectangle;
			client_area.X += BorderStyleSize;
			client_area.Y += BorderStyleSize;
			client_area.Width -= BorderStyleSize * 2;
			client_area.Height -= BorderStyleSize * 2;

			//Console.WriteLine ("CalcClientArea");

			//Console.WriteLine ("CalcClientArea ClientRectangle {0}, ClientArea {1}, BorderStyleSize {2}",
			//	 grid.ClientRectangle, client_area, BorderStyleSize);
		}

		public void CalcGridAreas ()
		{
			if (grid.IsHandleCreated == false) // Delay calculations until the handle is created
				return;

			/* Order is important. E.g. row headers max. height depends on caption */
			grid.horz_pixeloffset = 0;
			CalcClientArea ();
			CalcCaption ();
			CalcParentRows ();
			CalcRowsHeaders ();
			CalcColumnsHeader ();
			CalcCellsArea ();

			UpdateVisibleRowCount (); // need it to be able to calcultate the need of horz scrollbar
			if (SetUpVerticalScrollBar ()) { // We need a Vertical ScrollBar
				
				if (grid.parentrows_visible) {
					parent_rows.Width -= grid.vert_scrollbar.Width;
				}

				if (grid.columnheaders_visible) {
					if (columnshdrs_area.X + columnshdrs_area.Width > grid.vert_scrollbar.Location.X) {
						columnshdrs_area.Width -= grid.vert_scrollbar.Width;
					}
				}

				if (cells_area.X + cells_area.Width >= grid.vert_scrollbar.Location.X) {
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
			UpdateVisibleRowCount ();

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

			if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
				max_width_cols -= grid.RowHeaderWidth;
			}

			if (width_all_cols > max_width_cols) {
				columnshdrs_area.Width = columnshdrs_maxwidth;
			} else {
				columnshdrs_area.Width = width_all_cols;

				if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
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
			if (grid.CurrentTableStyle.CurrentRowHeadersVisible == false) {
				rowshdrs_area = Rectangle.Empty;
				return;
			}

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
			if (grid.CurrentTableStyle.GridColumnStyles.Count == 0) {
				grid.visiblecolumn_count = 0;
				return;	
			}
			
			int col;
			int max_pixel = grid.horz_pixeloffset + cells_area.Width;
			grid.first_visiblecolumn = FromPixelToColumn (grid.horz_pixeloffset);

			col = FromPixelToColumn (max_pixel);
			grid.visiblecolumn_count = (col - grid.first_visiblecolumn);
			
			//if (grid.first_visiblecolumn + grid.visiblecolumn_count + 1 < grid.CurrentTableStyle.GridColumnStyles.Count) { 
				grid.visiblecolumn_count++; // Partially visible column
			//}

			//Console.WriteLine ("UpdateVisibleColumn col: {0}, cnt {1}",
			//	grid.first_visiblecolumn, grid.visiblecolumn_count);
		}

		public void UpdateVisibleRowCount ()
		{
			int max_height = cells_area.Height;
			int total_rows = grid.RowsCount;
			
			if (grid.ShowEditRow) {
				total_rows++;
			}

			int rows_height = (total_rows - grid.first_visiblerow) * grid.RowHeight;
			int max_rows = max_height / grid.RowHeight;

			//Console.WriteLine ("UpdateVisibleRowCount {0} {1}/{2} (row h) {3}",
			//	max_rows, max_height, grid.RowHeight, cells_area.Height);

			if (max_rows > total_rows) {
				max_rows = total_rows;
			}

			if (rows_height > cells_area.Height) {
				grid.visiblerow_count = max_rows;
			} else {
				grid.visiblerow_count = total_rows;
			}			
			
			if (grid.visiblerow_count + grid.first_visiblerow > total_rows)
				grid.visiblerow_count = total_rows - grid.first_visiblerow;

			if (grid.visiblerow_count < max_rows) {
				grid.visiblerow_count = max_rows;
				grid.first_visiblerow = total_rows - max_rows;
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
				int posy;
				int rcnt = grid.FirstVisibleRow + grid.VisibleRowCount;
				for (int r = grid.FirstVisibleRow; r < rcnt; r++) {
					posy = cells_area.Y + ((r - grid.FirstVisibleRow) * grid.RowHeight);
					if (y <= posy + grid.RowHeight) { // Found row
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
				pos_y = cells_area.Y + ((row - grid.FirstVisibleRow) * grid.RowHeight);

				if (y <= pos_y + grid.RowHeight) { // Found row
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
			Console.WriteLine ("OnPaint {0}", pe.ClipRectangle);
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
			
			ThemeEngine.Current.CPDrawBorderStyle (pe.Graphics, grid.ClientRectangle, grid.border_style);
		}

		public void PaintCaption (Graphics g, Rectangle clip)
		{
			Rectangle modified_area = clip;
			modified_area.Intersect (caption_area);

			g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.CaptionBackColor),
				modified_area);

			g.DrawString (grid.CaptionText, grid.CaptionFont,
				ThemeEngine.Current.ResPool.GetSolidBrush (grid.CaptionForeColor),
				caption_area);		
		}

		public void PaintColumnsHdrs (Graphics g, Rectangle clip)
		{
			Rectangle columns_area = columnshdrs_area;

			if (grid.CurrentTableStyle.CurrentRowHeadersVisible) { // Paint corner shared between row and column header
				Rectangle rect_bloc = columnshdrs_area;
				rect_bloc.Width = grid.RowHeaderWidth;
				rect_bloc.Height = columnshdrs_area.Height;
				if (clip.IntersectsWith (rect_bloc)) {
					if (grid.visiblecolumn_count > 0) {
						g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.CurrentTableStyle.CurrentHeaderBackColor), rect_bloc);
					}else {
						g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.BackgroundColor), rect_bloc);
					}
				}

				columns_area.X += grid.RowHeaderWidth;
				columns_area.Width -= grid.RowHeaderWidth;
			}

			// Set unused area
			Rectangle columnshdrs_area_complete = columns_area;
			columnshdrs_area_complete.Width = columnshdrs_maxwidth;
			
			if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
				columnshdrs_area_complete.Width -= grid.RowHeaderWidth;
			}		

			// Set column painting
			Rectangle rect_columnhdr = new Rectangle ();
			int col_pixel;
			Region prev_clip = g.Clip, current_clip;
			rect_columnhdr.Y = columns_area.Y;
			rect_columnhdr.Height = columns_area.Height;

			Console.WriteLine ("PaintColumnsHdrs first {0} num {1}",
				grid.first_visiblecolumn, grid.visiblecolumn_count);
			
			int column_cnt = grid.first_visiblecolumn + grid.visiblecolumn_count;
			for (int column = grid.first_visiblecolumn; column < column_cnt; column++) {

				col_pixel = GetColumnStartingPixel (column);
				rect_columnhdr.X = columns_area.X + col_pixel - grid.horz_pixeloffset;
				rect_columnhdr.Width = grid.CurrentTableStyle.GridColumnStyles[column].Width;

				if (clip.IntersectsWith (rect_columnhdr) == false)
					continue;
									
				current_clip = new Region (columns_area);
				g.Clip = current_clip;

				grid.CurrentTableStyle.GridColumnStyles[column].PaintHeader (g, rect_columnhdr, column);

				g.Clip = prev_clip;
				current_clip.Dispose ();
			}

			
			// This fills with background colour the unused part in the row headers
			if (rect_columnhdr.X + rect_columnhdr.Height < client_area.X + client_area.Width) {
				
				Rectangle not_usedarea = columnshdrs_area_complete;				
				not_usedarea.X = rect_columnhdr.X + rect_columnhdr.Width;
				not_usedarea.Width = client_area.X + client_area.Width - rect_columnhdr.X - rect_columnhdr.Height;
			
				g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.BackgroundColor),
					not_usedarea);
			}
		}

		public void PaintRowsHeaders (Graphics g, Rectangle clip)
		{
			Rectangle rowshdrs_area_complete = rowshdrs_area;
			rowshdrs_area_complete.Height = rowshdrs_maxheight;
			Rectangle rect_row = new Rectangle ();
			rect_row.X = rowshdrs_area.X;			
			int last_y = 0;
			int rowcnt = grid.FirstVisibleRow + grid.VisibleRowCount;

			if (rowcnt < grid.RowsCount) { // Paint one row more for partial rows
				rowcnt++;
			}

			for (int row = grid.FirstVisibleRow; row < rowcnt; row++) {

				rect_row.Width = rowshdrs_area.Width;
				rect_row.Height = grid.RowHeight;
				rect_row.Y = rowshdrs_area.Y + ((row - grid.FirstVisibleRow) * grid.RowHeight);

				if (clip.IntersectsWith (rect_row)) {
					PaintRowHeader (g, rect_row, row);
					last_y = rect_row.Y;
				}
			}

			// This fills with background colour the unused part in the row headers
			if (last_y > 0 && rect_row.Y + rect_row.Height < cells_area.Y + cells_area.Height) {
				Rectangle not_usedarea = clip;
				not_usedarea.Intersect (rowshdrs_area_complete);
				
				not_usedarea.Y = rect_row.Y + rect_row.Height;
				not_usedarea.Height = rowshdrs_area_complete.Y + rowshdrs_area_complete.Height - rect_row.Height - rect_row.Y;
				g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.BackgroundColor),
					not_usedarea);
			}			
		}

		public void InvalidateRow (int row)
		{
			if (row < grid.FirstVisibleRow || row > grid.FirstVisibleRow + grid.VisibleRowCount) {
				return;
			}

			Rectangle rect_row = new Rectangle ();

			rect_row.X = cells_area.X;
			rect_row.Width = cells_area.Width;
			rect_row.Height = grid.RowHeight;
			rect_row.Y = cells_area.Y + ((row - grid.FirstVisibleRow) * grid.RowHeight);
			grid.Invalidate (rect_row);
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
			g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.CurrentTableStyle.CurrentHeaderBackColor),
				bounds);

			// Paint Borders
			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonHilight),
				bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y);

			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonHilight),
				bounds.X, bounds.Y + 1, bounds.X, bounds.Y + bounds.Height - 1);

			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow),
				bounds.X + bounds.Width - 1, bounds.Y + 1 , bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);

			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow),
				bounds.X, bounds.Y + bounds.Height -1, bounds.X + bounds.Width, bounds.Y  + bounds.Height -1);

			if (grid.ShowEditRow && row == grid.RowsCount  && !(row == grid.CurrentCell.RowNumber && grid.is_changing == true)) {
				
				g.DrawString ("*", font_newrow,	ThemeEngine.Current.ResPool.GetSolidBrush (Color.Black),
					bounds);
				
			} else {
				// Draw arrow
				if (row == grid.CurrentCell.RowNumber) {
	
					if (grid.is_changing == true) {
						g.DrawString ("...", grid.Font,
							ThemeEngine.Current.ResPool.GetSolidBrush (Color.Black),
							bounds);
	
					} else {
						int cx = 18;
						int cy = 18;
						Bitmap	bmp = new Bitmap (cx, cy);
						Graphics gr = Graphics.FromImage (bmp);
						Rectangle rect_arrow = new Rectangle (0, 0, cx, cy);
						ControlPaint.DrawMenuGlyph (gr, rect_arrow, MenuGlyph.Arrow);
						bmp.MakeTransparent ();
						g.DrawImage (bmp, bounds.X - 2, bounds.Y, cx, cy);
						gr.Dispose ();
						bmp.Dispose ();
					}
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
			Rectangle not_usedarea = new Rectangle ();
			rect_row.X = cells.X;

			int rowcnt = grid.FirstVisibleRow + grid.VisibleRowCount;
			
			if (grid.ShowEditRow) {
				rowcnt--;
			}			

			if (rowcnt < grid.RowsCount) { // Paint one row more for partial rows
				rowcnt++;
			}			
			
			rect_row.Height = grid.RowHeight;
			rect_row.Width = cells.Width;
			for (int row = grid.FirstVisibleRow; row < rowcnt; row++) {								
				rect_row.Y = cells.Y + ((row - grid.FirstVisibleRow) * grid.RowHeight);
				if (clip.IntersectsWith (rect_row)) {
					PaintRow (g, row, rect_row, false);					
				}
			}
			
			if (grid.ShowEditRow && grid.FirstVisibleRow + grid.VisibleRowCount == grid.RowsCount + 1) {
				rect_row.Y = cells.Y + ((rowcnt - grid.FirstVisibleRow) * grid.RowHeight);
				if (clip.IntersectsWith (rect_row)) {
					PaintRow (g, rowcnt, rect_row, true);					
				}
			}
			
			not_usedarea.Height = cells.Y + cells.Height - rect_row.Y - rect_row.Height;
			not_usedarea.Y = rect_row.Y + rect_row.Height;
			not_usedarea.Width = rect_row.Width = cells.Width;
			not_usedarea.X = cells.X;			

			g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.BackgroundColor),
				not_usedarea);			
		}
		
		public void PaintRow (Graphics g, int row, Rectangle row_rect, bool is_newrow)
		{
			//Console.WriteLine ("PaintRow row: {0}, rect {1}, is_newrow {2}", row, row_rect, is_newrow);

			Rectangle rect_cell = new Rectangle ();
			int col_pixel;
			Color backcolor, forecolor;
			Region prev_clip = g.Clip;
			Region current_clip;
			Rectangle not_usedarea = new Rectangle ();

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

				if (grid.IsSelected (row)) {
					backcolor =  grid.SelectionBackColor;
					forecolor =  grid.SelectionForeColor;
				} else {
					if (row % 2 == 0) {
						backcolor =  grid.BackColor;
					} else {
						backcolor =  grid.AlternatingBackColor;
					}
					
					forecolor =  grid.ForeColor;
				}			

				if (is_newrow) {
					grid.CurrentTableStyle.GridColumnStyles[column].PaintNewRow (g, rect_cell, 
						ThemeEngine.Current.ResPool.GetSolidBrush (backcolor),
						ThemeEngine.Current.ResPool.GetSolidBrush (forecolor));						
					
				} else {
					grid.CurrentTableStyle.GridColumnStyles[column].Paint (g, rect_cell, grid.ListManager, row,
						ThemeEngine.Current.ResPool.GetSolidBrush (backcolor),
						ThemeEngine.Current.ResPool.GetSolidBrush (forecolor),
						grid.RightToLeft == RightToLeft.Yes);
				}

				g.Clip = prev_clip;
				current_clip.Dispose ();
			}
			
			if (row_rect.X + row_rect.Width > rect_cell.X + rect_cell.Width) {

				not_usedarea.X = rect_cell.X + rect_cell.Width;
				not_usedarea.Width = row_rect.X + row_rect.Width - rect_cell.X - rect_cell.Width;
				not_usedarea.Y = row_rect.Y;
				not_usedarea.Height = row_rect.Height;
				g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (grid.BackgroundColor),
					not_usedarea);
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

			grid.horiz_scrollbar.Location = new Point (client_area.X, client_area.Y +
				client_area.Height - grid.horiz_scrollbar.Height);

			grid.horiz_scrollbar.Size = new Size (client_area.Width,
				grid.horiz_scrollbar.Height);

			grid.horiz_scrollbar.Maximum = width_all;// - cells_area.Width;
			grid.horiz_scrollbar.LargeChange = cells_area.Width;
			grid.Controls.Add (grid.horiz_scrollbar);
			grid.horiz_scrollbar.Visible = true;
			return true;
		}

		// Return true if the scrollbar is needed
		public bool SetUpVerticalScrollBar ()
		{
			int y, height;
			int allrows = grid.RowsCount;

			if (grid.ShowEditRow) {
				allrows++;
			}
			
			if (grid.visiblerow_count == allrows) {
				grid.vert_scrollbar.Visible = false;
				grid.Controls.Remove (grid.vert_scrollbar);
				return false;
			}
			
			if (grid.caption_visible) {
				y = client_area.Y + caption_area.Height;
				height = client_area.Height - caption_area.Height;
			} else {
				y = client_area.Y;
				height = client_area.Height;
			}

			grid.vert_scrollbar.Location = new Point (client_area.X +
				client_area.Width - grid.vert_scrollbar.Width, y);

			grid.vert_scrollbar.Size = new Size (grid.vert_scrollbar.Width,
				height);

			grid.vert_scrollbar.Maximum = grid.RowsCount;
			
			if (grid.ShowEditRow) {
				grid.vert_scrollbar.Maximum++;	
			}
			
			grid.vert_scrollbar.LargeChange = VLargeChange;

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

				if (grid.CurrentTableStyle.CurrentRowHeadersVisible) {
					columns_area.X += grid.RowHeaderWidth;
					columns_area.Width -= grid.RowHeaderWidth;
				}
				return columns_area;
			}
		}

		public int ColumnsHeaderHeight {
			get {
				return grid.CurrentTableStyle.HeaderFont.Height + 6;
			}
		}

		public Rectangle RowsHeadersArea {
			get {
				return rowshdrs_area;
			}
		}

		public int VLargeChange {
			get {
				return cells_area.Height / grid.RowHeight;
			}
		}

		#endregion Instance Properties
	}
}
