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
			CalcColumnsHeader ();
			CalcRowsHeaders ();

			Region cells = new Region (grid.ClientRectangle);
			cells.Exclude (caption_area);
			cells.Exclude (parent_rows);
			cells.Exclude (columnshdrs_area);
			cells.Exclude (rowshdrs_area);

			if (grid.horiz_scrollbar.Visible) {
				Rectangle area = new Rectangle (grid.horiz_scrollbar.Location.X, grid.horiz_scrollbar.Location.Y,
					grid.horiz_scrollbar.Size.Width, grid.horiz_scrollbar.Size.Height);

				cells.Exclude (area);
			}

			if (grid.vert_scrollbar.Visible) {
				Rectangle area = new Rectangle (grid.vert_scrollbar.Location.X, grid.vert_scrollbar.Location.Y,
					grid.vert_scrollbar.Size.Width, grid.vert_scrollbar.Size.Height);

				cells.Exclude (area);
			}

			cells_area = Rectangle.Ceiling (cells.GetBounds (grid.CreateGraphics ())); 
			cells_area.Inflate (-1, -1);
			cells.Dispose ();			
			
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
			caption_area.Height = grid.CaptionFont.Height + 3;

			Console.WriteLine ("DataGridDrawing.CalcCaption {0}", caption_area);
		}

		public void CalcColumnsHeader ()
		{
			if (grid.columnheaders_visible == false) {
				columnshdrs_area = Rectangle.Empty;
				return;
			}
		}

		public void CalcParentRows ()
		{
			if (grid.parentrows_visible == false) {
				parent_rows = Rectangle.Empty;
				return;
			}

			if (caption_area.Height == 0) {
				parent_rows.Y = BorderStyleSize;
			} else {
				parent_rows.Y = caption_area.Y + caption_area.Height;
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

			int max_height = grid.ClientRectangle.Height - caption_area.Height - parent_rows.Height - BorderStyleSize;
			int rows_height = grid.RowsCount * grid.PreferredRowHeight;

			if (rows_height > max_height) {
				grid.visiblerow_count = max_height / grid.PreferredRowHeight;
			} else {
				grid.visiblerow_count = grid.RowsCount;
			}

			rowshdrs_area.X = BorderStyleSize;
			rowshdrs_area.Y = BorderStyleSize + caption_area.Height + parent_rows.Height;			
			rowshdrs_area.Width = grid.RowHeaderWidth;
			rowshdrs_area.Height = 1 + grid.visiblerow_count * grid.PreferredRowHeight;

			Console.WriteLine ("DataGridDrawing.CalcRowsHeaders {0}", rowshdrs_area);
		}
		
		public void InvalidateCaption ()
		{
			if (caption_area.IsEmpty)
				return;

			grid.Invalidate (caption_area);
		}
		
		public void OnPaint (PaintEventArgs pe)
		{
			pe.Graphics.Clear (grid.BackgroundColor);
			pe.Graphics.FillRectangle (new SolidBrush (grid.CaptionBackColor), caption_area);
			pe.Graphics.FillRectangle (new SolidBrush (grid.ParentRowsBackColor), parent_rows);
			
			//pe.Graphics.DrawRectangle (new Pen (Color.Red), caption_area);
			//pe.Graphics.DrawRectangle (new Pen (Color.Blue), parent_rows);
			pe.Graphics.DrawRectangle (new Pen (Color.Green), columnshdrs_area);
			pe.Graphics.DrawRectangle (new Pen (Color.Yellow), rowshdrs_area);
			pe.Graphics.DrawRectangle (new Pen (Color.Pink), cells_area);
			
			PaintRows (pe.Graphics, cells_area);
			
		}
		
		public void PaintRows (Graphics g, Rectangle cells)
		{			
			Rectangle rect = new Rectangle ();
			rect.X = cells.X;
			rect.Y = cells.Y;			
						
			for (int row = grid.first_visiblerow; row < grid.VisibleRowCount; row++) {
				// Get cell's width for column style
				rect.Width = grid.CurrentTableStyle.GridColumnStyles[row].Width;
				rect.Height = grid.RowHeight;
				PaintRow (g, row, rect);
			}			
		}
		
		public void PaintRow (Graphics g, int row, Rectangle row_rect)
		{
			Console.WriteLine ("PaintRow row: {0}, rect {1}", row, row_rect);
			g.DrawString ("prova", new Font ("Arial", 12), new SolidBrush (Color.Black), row_rect);
		}
		
		
		#endregion // Public Instance Methods

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

	}
}
