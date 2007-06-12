//
// TableLayout.cs
//
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
using System;
using System.Drawing;

namespace System.Windows.Forms.Layout
{
	internal class TableLayout : LayoutEngine
	{
		private static Control dummy_control = new Control ("Dummy");	// Used as a placeholder for row/col spans

		public TableLayout () : base ()
		{
		}
		
		public override void InitLayout (object child, BoundsSpecified specified)
		{
			base.InitLayout (child, specified);
		}

		// There are 3 steps to doing a table layout:
		// 1) Figure out which row/column each control goes into
		// 2) Figure out the sizes of each row/column
		// 3) Size and position each control
		public override bool Layout (object container, LayoutEventArgs args)
		{
			TableLayoutPanel panel = container as TableLayoutPanel;
			TableLayoutSettings settings = panel.LayoutSettings;
			
			// Nothing to layout, don't waste time
			if (panel.Controls.Count == 0) 
				return false;

			// STEP 1:
			// - Figure out which row/column each control goes into
			// - Store data in the TableLayoutPanel.actual_positions
			panel.actual_positions = CalculateControlPositions (panel, Math.Max (settings.ColumnCount, 1), Math.Max (settings.RowCount, 1));

			// STEP 2:
			// - Figure out the sizes of each row/column
			// - Store data in the TableLayoutPanel.widths/heights
			CalculateColumnRowSizes (panel, panel.actual_positions.GetLength (0), panel.actual_positions.GetLength (1));
			
			// STEP 3:
			// - Size and position each control
			LayoutControls(panel);

			return false;
		}

		private Control[,] CalculateControlPositions (TableLayoutPanel panel, int columns, int rows)
		{
			Control[,] grid = new Control[columns, rows];

			TableLayoutSettings settings = panel.LayoutSettings;

			// First place all controls that have an explicit col/row
			foreach (Control c in panel.Controls) {
				int col = settings.GetColumn (c);
				int row = settings.GetRow (c);

				if ((col >= 0 && col < columns) && (row >= 0 && row < rows)) {
					if (grid[col, row] == null) {
						int col_span = Math.Min (settings.GetColumnSpan (c), columns);
						int row_span = Math.Min (settings.GetRowSpan (c), rows);

						if (col + col_span > columns) {
							if (row + 1 < rows) {
								grid[col, row] = dummy_control;
								row++;
								col = 0;
							}
							else if (settings.GrowStyle == TableLayoutPanelGrowStyle.AddColumns)
								return CalculateControlPositions (panel, columns + 1, rows);
							else
								throw new ArgumentException ();
						}

						if (row + row_span > rows) {
							if (settings.GrowStyle == TableLayoutPanelGrowStyle.AddRows)
								return CalculateControlPositions (panel, columns, rows + 1);
							else
								throw new ArgumentException ();
						}

						grid[col, row] = c;

						for (int i = 1; i < col_span; i++)
							grid[col + i, row] = dummy_control;

						for (int i = 1; i < row_span; i++)
							grid[col, row + i] = dummy_control;
					}
				}
			}

			int x_pointer = 0;
			int y_pointer = 0;

			// Fill in gaps with controls that do not have an explicit col/row
			foreach (Control c in panel.Controls) {
				int col = settings.GetColumn (c);
				int row = settings.GetRow (c);

				if ((col >= 0 && col < columns) && (row >= 0 && row < rows))
					continue;

				for (int y = y_pointer; y < rows; y++) {
					y_pointer = y;
					x_pointer = 0;

					for (int x = x_pointer; x < columns; x++) {
						x_pointer = x;

						if (grid[x, y] == null) {
							int col_span = Math.Min (settings.GetColumnSpan (c), columns);
							int row_span = Math.Min (settings.GetRowSpan (c), rows);

							if (x + col_span > columns) {
								if (y + 1 < rows)
									break;
								else if (settings.GrowStyle == TableLayoutPanelGrowStyle.AddColumns)
									return CalculateControlPositions (panel, columns + 1, rows);
								else
									throw new ArgumentException ();
							}

							if (y + row_span > rows) {
								if (x + 1 < columns)
									break;
								else if (settings.GrowStyle == TableLayoutPanelGrowStyle.AddRows)
									return CalculateControlPositions (panel, columns, rows + 1);
								else
									throw new ArgumentException ();
							}

							grid[x, y] = c;

							for (int i = 1; i < col_span; i++)
								grid[x + i, y] = dummy_control;

							for (int i = 1; i < row_span; i++)
								grid[x, y + i] = dummy_control;

							// I know someone will kill me for using a goto, but 
							// sometimes they really are the easiest way...
							goto Found;
						}
					}
				}

				// We ran out of room in the grid, and have more controls, what is our GrowStyle?
				switch (settings.GrowStyle) {
					case TableLayoutPanelGrowStyle.AddColumns:
						return CalculateControlPositions (panel, columns + 1, rows);
					case TableLayoutPanelGrowStyle.AddRows:
					default:
						return CalculateControlPositions (panel, columns, rows + 1);
					case TableLayoutPanelGrowStyle.FixedSize:
						throw new ArgumentException ();
				}

			Found: ;
			}

			return grid;
		}

		private void CalculateColumnRowSizes (TableLayoutPanel panel, int columns, int rows)
		{
			TableLayoutSettings settings = panel.LayoutSettings;

			panel.column_widths = new int[panel.actual_positions.GetLength (0)];
			panel.row_heights = new int[panel.actual_positions.GetLength (1)];

			Rectangle parentDisplayRectangle = panel.DisplayRectangle;

			TableLayoutColumnStyleCollection col_styles = new TableLayoutColumnStyleCollection (panel);
			
			foreach (ColumnStyle cs in settings.ColumnStyles)
				col_styles.Add( new ColumnStyle(cs.SizeType, cs.Width));

			TableLayoutRowStyleCollection row_styles = new TableLayoutRowStyleCollection (panel);

			foreach (RowStyle rs in settings.RowStyles)
				row_styles.Add (new RowStyle (rs.SizeType, rs.Height));
		
			// If we have more columns than columnstyles, temporarily add enough columnstyles
			if (columns > col_styles.Count)
			{
				for (int i = col_styles.Count; i < columns; i++)
					col_styles.Add(new ColumnStyle());			
			}

			// Same for rows..
			if (rows > row_styles.Count) 
			{
				for (int i = row_styles.Count; i < rows; i++)
					row_styles.Add (new RowStyle ());
			}

			while (row_styles.Count > rows)
				row_styles.RemoveAt (row_styles.Count - 1);
			while (col_styles.Count > columns)
				col_styles.RemoveAt (col_styles.Count - 1);
				
			// Figure up all the column widths
			int total_width = parentDisplayRectangle.Width;
			int index = 0;

			// First assign all the Absolute sized columns..
			foreach (ColumnStyle cs in col_styles) {
				if (cs.SizeType == SizeType.Absolute) {
					panel.column_widths[index] = (int)cs.Width;
					total_width -= (int)cs.Width;
				}

				index++;
			}

			index = 0;

			// Next, assign all the AutoSize columns..
			foreach (ColumnStyle cs in col_styles)
			{
				if (cs.SizeType == SizeType.AutoSize)
				{
					int max_width = 0; 
					
					// Find the widest control in the column
					for (int i = 0; i < rows; i ++)
					{
						Control c = panel.actual_positions[index, i];
						
						if (c != null  && c != dummy_control)
						{
							if (c.Dock == DockStyle.Fill || ((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left && (c.Anchor & AnchorStyles.Right) == AnchorStyles.Right)) {
								if (c.ExplicitBounds.Width + c.Margin.Left + c.Margin.Right > max_width)
									max_width = c.ExplicitBounds.Width + c.Margin.Left + c.Margin.Right;
									
								continue;
							}
							else if (settings.GetColumnSpan (c) > 1)
								continue;
							
							if (c.Width + c.Margin.Left + c.Margin.Right > max_width)
								max_width = c.Width + c.Margin.Left + c.Margin.Right;						
						}
					}

					panel.column_widths[index] = max_width;
					total_width -= max_width;				
				}
				
				index++;
			}
			
			index = 0;
			float total_percent = 0;
			
			// Finally, assign the remaining space to Percent columns..
			if (total_width > 0)
			{
				int percent_width = total_width; 
				
				// Find the total percent (not always 100%)
				foreach (ColumnStyle cs in col_styles) 
				{
					if (cs.SizeType == SizeType.Percent)
						total_percent += cs.Width;
				}

				// Divy up the space..
				foreach (ColumnStyle cs in col_styles) 
				{
					if (cs.SizeType == SizeType.Percent) 
					{
						panel.column_widths[index] = (int)((cs.Width / total_percent) * percent_width);
						total_width -= panel.column_widths[index];
					}

					index++;
				}
			}

			if (total_width > 0)
				panel.column_widths[col_styles.Count - 1] += total_width;

			// Figure up all the row heights
			int total_height = parentDisplayRectangle.Height;
			index = 0;

			// First assign all the Absolute sized rows..
			foreach (RowStyle rs in row_styles) {
				if (rs.SizeType == SizeType.Absolute) {
					panel.row_heights[index] = (int)rs.Height;
					total_height -= (int)rs.Height;
				}

				index++;
			}

			index = 0;

			// Next, assign all the AutoSize rows..
			foreach (RowStyle rs in row_styles) {
				if (rs.SizeType == SizeType.AutoSize) {
					int max_height = 0;

					// Find the tallest control in the row
					for (int i = 0; i < columns; i++) {
						Control c = panel.actual_positions[i, index];

						if (c != null && c != dummy_control) {
							if (c.Dock == DockStyle.Fill || ((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top && (c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)) {
								if (c.ExplicitBounds.Height + c.Margin.Top + c.Margin.Bottom > max_height)
									max_height = c.ExplicitBounds.Height + c.Margin.Top + c.Margin.Bottom;

								continue;
							}
							else if (settings.GetRowSpan (c) > 1)
								continue;

							if (c.Height + c.Margin.Top + c.Margin.Bottom > max_height)
								max_height = c.Height + c.Margin.Top + c.Margin.Bottom;
						}
					}

					panel.row_heights[index] = max_height;
					total_height -= max_height;
				}

				index++;
			}

			index = 0;
			total_percent = 0;

			// Finally, assign the remaining space to Percent columns..
			if (total_height > 0) {
				int percent_height = total_height;
				
				// Find the total percent (not always 100%)
				foreach (RowStyle rs in row_styles) {
					if (rs.SizeType == SizeType.Percent)
						total_percent += rs.Height;
				}

				// Divy up the space..
				foreach (RowStyle rs in row_styles) {
					if (rs.SizeType == SizeType.Percent) {
						panel.row_heights[index] = (int)((rs.Height / total_percent) * percent_height);
						total_height -= panel.row_heights[index];
					}

					index++;
				}
			}

			if (total_height > 0)
				panel.row_heights[row_styles.Count - 1] += total_height;
		}
		
		private void LayoutControls (TableLayoutPanel panel)
		{
			TableLayoutSettings settings = panel.LayoutSettings;

			int columns = panel.actual_positions.GetLength(0);
			int rows = panel.actual_positions.GetLength(1);
			
			Point current_pos = new Point(0,0);
			
			for (int y = 0; y < rows; y++)
			{
				for (int x = 0; x < columns; x ++)
				{
					Control c = panel.actual_positions[x,y];
					
					if(c != null && c != dummy_control)
					
					{		
						int new_x = 0;
						int new_y = 0;
						int new_width = 0;
						int new_height = 0;
						
						// Figure out the width of the control
						int column_width = panel.column_widths[x];
						
						for (int i = 1; i < settings.GetColumnSpan(c); i++)
							column_width += panel.column_widths[x + i];

						if (c.Dock == DockStyle.Fill || c.Dock == DockStyle.Top || c.Dock == DockStyle.Bottom || ((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left && (c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
							new_width = column_width - c.Margin.Left - c.Margin.Right;
						else
							new_width = Math.Min (c.Width, column_width - c.Margin.Left - c.Margin.Right);
							
						// Figure out the height of the control
						int column_height = panel.row_heights[y];

						for (int i = 1; i < settings.GetRowSpan (c); i++)
							column_height += panel.row_heights[y + i];

						if (c.Dock == DockStyle.Fill || c.Dock == DockStyle.Left || c.Dock == DockStyle.Right || ((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top && (c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
							new_height = column_height - c.Margin.Top - c.Margin.Bottom;
						else
							new_height = Math.Min (c.Height, column_height - c.Margin.Top - c.Margin.Bottom);

						// Figure out the left location of the control
						if (c.Dock == DockStyle.Left || c.Dock == DockStyle.Fill || (c.Anchor & AnchorStyles.Left) == AnchorStyles.Left)
							new_x = current_pos.X + c.Margin.Left;
						else if (c.Dock == DockStyle.Right || (c.Anchor & AnchorStyles.Right) == AnchorStyles.Right)
							new_x = (current_pos.X + column_width) - new_width - c.Margin.Right;
						else	// (center control)
							new_x = ((current_pos.X + column_width - c.Margin.Left - c.Margin.Right) / 2) + c.Margin.Left - (new_width / 2);

						// Figure out the top location of the control
						if (c.Dock == DockStyle.Top || c.Dock == DockStyle.Fill || (c.Anchor & AnchorStyles.Top) == AnchorStyles.Top)
							new_y = current_pos.Y + c.Margin.Top;
						else if (c.Dock == DockStyle.Bottom || (c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
							new_y = (current_pos.Y + column_height) - new_height - c.Margin.Bottom;
						else	// (center control)
							new_y = ((current_pos.Y + column_height - c.Margin.Top - c.Margin.Bottom) / 2) + c.Margin.Top - (new_height / 2);

						c.SetBounds (new_x, new_y, new_width, new_height, BoundsSpecified.None);
					}

					current_pos.Offset (panel.column_widths[x], 0);
				}

				current_pos.Offset (-1 * current_pos.X, panel.row_heights[y]);
			}
		}
	}
}
#endif
