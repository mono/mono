//
// TableLayoutPanel.cs
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

using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms.Layout;
using System.ComponentModel.Design.Serialization;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ProvideProperty ("CellPosition", typeof (Control))]
	[ProvideProperty ("Column", typeof (Control))]
	[ProvideProperty ("ColumnSpan", typeof (Control))]
	[ProvideProperty ("Row", typeof (Control))]
	[ProvideProperty ("RowSpan", typeof (Control))]
	[DefaultProperty ("ColumnCount")]
	[Docking (DockingBehavior.Never)]
	[Designer ("System.Windows.Forms.Design.TableLayoutPanelDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DesignerSerializer ("System.Windows.Forms.Design.TableLayoutPanelCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
	public class TableLayoutPanel : Panel, IExtenderProvider
	{
		private TableLayoutSettings settings;
		private static TableLayout layout_engine = new TableLayout ();
		private TableLayoutPanelCellBorderStyle cell_border_style;

		// This is the row/column the Control actually got placed
		internal Control[,] actual_positions;
		
		// Widths and heights of each column/row
		internal int[] column_widths;
		internal int[] row_heights;

		#region Public Constructor
		public TableLayoutPanel ()
		{
			settings = new TableLayoutSettings(this);
			cell_border_style = TableLayoutPanelCellBorderStyle.None;
			column_widths = new int[0];
			row_heights = new int[0];
			CreateDockPadding ();
		}
		#endregion

		#region Public Properties
		[Localizable (true)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		new public BorderStyle BorderStyle {
			get { return base.BorderStyle; }
			set { base.BorderStyle = value; }
		}

		[Localizable (true)]
		[DefaultValue (TableLayoutPanelCellBorderStyle.None)]
		public TableLayoutPanelCellBorderStyle CellBorderStyle {
			get { return this.cell_border_style; }
			set { 
				if (this.cell_border_style != value) {
					this.cell_border_style = value;
					this.PerformLayout (this, "CellBorderStyle");
					this.Invalidate ();
				}
			}
		}

		[Localizable (true)]
		[DefaultValue (0)]
		public int ColumnCount {
			get { return settings.ColumnCount; }
			set { settings.ColumnCount = value; }
		}

		[Browsable (false)]
		[DisplayName ("Columns")]
		[MergableProperty (false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TableLayoutColumnStyleCollection ColumnStyles {
			get { return settings.ColumnStyles; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		new public TableLayoutControlCollection Controls {
			get { return (TableLayoutControlCollection) base.Controls; }
		}

		[DefaultValue (TableLayoutPanelGrowStyle.AddRows)]
		public TableLayoutPanelGrowStyle GrowStyle {
			get { return settings.GrowStyle; }
			set { settings.GrowStyle = value; }
		}

		public override System.Windows.Forms.Layout.LayoutEngine LayoutEngine {
			get { return TableLayoutPanel.layout_engine; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public TableLayoutSettings LayoutSettings {
			get { return this.settings; }
			set {
				if (value.isSerialized) {
					// Serialized version doesn't calculate these.
					value.ColumnCount = value.ColumnStyles.Count;
					value.RowCount = value.RowStyles.Count;
					value.panel = this;
					
					this.settings = value;
				} else
					throw new NotSupportedException ("LayoutSettings value cannot be set directly.");
			}
		}

		[Localizable (true)]
		[DefaultValue (0)]
		public int RowCount {
			get { return settings.RowCount; }
			set { settings.RowCount = value; }
		}

		[Browsable (false)]
		[DisplayName ("Rows")]
		[MergableProperty (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TableLayoutRowStyleCollection RowStyles {
			get { return settings.RowStyles; }
		}
		#endregion

		#region Public Methods
		[DefaultValue (-1)]
		[DisplayName ("Cell")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public TableLayoutPanelCellPosition GetCellPosition (Control control)
		{
			return settings.GetCellPosition (control);
		}

		[DisplayName ("Column")]
		[DefaultValue (-1)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int GetColumn (Control control)
		{
			return settings.GetColumn (control);
		}

		[DisplayName ("ColumnSpan")]
		[DefaultValue (1)]
		public int GetColumnSpan (Control control)
		{
			return settings.GetColumnSpan (control);
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int[] GetColumnWidths ()
		{
			return this.column_widths;
		}

		public Control GetControlFromPosition (int column, int row)
		{
			if (column < 0 || row < 0)
				throw new ArgumentException ();

			TableLayoutPanelCellPosition pos = new TableLayoutPanelCellPosition (column, row);

			foreach (Control c in this.Controls)
				if (settings.GetCellPosition (c) == pos)
					return c;

			return null;
		}

		public TableLayoutPanelCellPosition GetPositionFromControl (Control control)
		{
			for (int x = 0; x < this.actual_positions.GetLength (0); x++)
				for (int y = 0; y < this.actual_positions.GetLength (1); y++)
					if (this.actual_positions[x, y] == control)
						return new TableLayoutPanelCellPosition (x, y);

			return new TableLayoutPanelCellPosition (-1, -1);
		}

		[DisplayName ("Row")]
		[DefaultValue ("-1")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int GetRow (Control control)
		{
			return settings.GetRow (control);
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int[] GetRowHeights ()
		{
			return this.row_heights;
		}

		[DisplayName ("RowSpan")]
		[DefaultValue (1)]
		public int GetRowSpan (Control control)
		{
			return settings.GetRowSpan (control);
		}

		public void SetCellPosition (Control control, TableLayoutPanelCellPosition position)
		{
			settings.SetCellPosition (control, position);
		}

		public void SetColumn (Control control, int column)
		{
			settings.SetColumn (control, column);
		}

		public void SetColumnSpan (Control control, int value)
		{
			settings.SetColumnSpan (control, value);
		}

		public void SetRow (Control control, int row)
		{
			settings.SetRow (control, row);
		}

		public void SetRowSpan (Control control, int value)
		{
			settings.SetRowSpan (control, value);
		}
		#endregion

		#region Protected Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override ControlCollection CreateControlsInstance ()
		{
			return new TableLayoutControlCollection (this);
		}

		protected virtual void OnCellPaint (TableLayoutCellPaintEventArgs e)
		{
			TableLayoutCellPaintEventHandler eh = (TableLayoutCellPaintEventHandler)(Events [CellPaintEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnLayout (LayoutEventArgs levent)
		{
			base.OnLayout (levent);
			Invalidate ();
		}

		protected override void OnPaintBackground (PaintEventArgs e)
		{
			base.OnPaintBackground (e);

			DrawCellBorders (e);
			
			int border_width = GetCellBorderWidth (CellBorderStyle);

			int x = border_width;
			int y = border_width;
			
			for (int i = 0; i < column_widths.Length; i++) {
				for (int j = 0; j < row_heights.Length; j++) {
					this.OnCellPaint (new TableLayoutCellPaintEventArgs (e.Graphics, e.ClipRectangle, new Rectangle (x, y, column_widths[i] + border_width, row_heights[j] + border_width), i, j));
					y += row_heights[j] + border_width;
				}

				x += column_widths[i] + border_width;
				y = border_width;
			}
		}

		protected override void ScaleControl (SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl (factor, specified);
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void ScaleCore (float dx, float dy)
		{
			base.ScaleCore (dx, dy);
		}
		#endregion

		#region Internal Methods
		internal static int GetCellBorderWidth (TableLayoutPanelCellBorderStyle style)
		{
			switch (style) {
				case TableLayoutPanelCellBorderStyle.Single:
					return 1;
				case TableLayoutPanelCellBorderStyle.Inset:
				case TableLayoutPanelCellBorderStyle.Outset:
					return 2;
				case TableLayoutPanelCellBorderStyle.InsetDouble:
				case TableLayoutPanelCellBorderStyle.OutsetPartial:
				case TableLayoutPanelCellBorderStyle.OutsetDouble:
					return 3;
			}
			
			return 0;
		}
		
		private void DrawCellBorders (PaintEventArgs e)
		{
			Rectangle paint_here = new Rectangle (Point.Empty, this.Size);

			switch (CellBorderStyle) {
				case TableLayoutPanelCellBorderStyle.Single:
					DrawSingleBorder (e.Graphics, paint_here);
					break;
				case TableLayoutPanelCellBorderStyle.Inset:
					DrawInsetBorder (e.Graphics, paint_here);
					break;
				case TableLayoutPanelCellBorderStyle.InsetDouble:
					DrawInsetDoubleBorder (e.Graphics, paint_here);
					break;
				case TableLayoutPanelCellBorderStyle.Outset:
					DrawOutsetBorder (e.Graphics, paint_here);
					break;
				case TableLayoutPanelCellBorderStyle.OutsetDouble:
				case TableLayoutPanelCellBorderStyle.OutsetPartial:
					DrawOutsetDoubleBorder (e.Graphics, paint_here);
					break;
			}
		}

		private void DrawSingleBorder (Graphics g, Rectangle rect)
		{
			ControlPaint.DrawBorder (g, rect, SystemColors.ControlDark, ButtonBorderStyle.Solid);

			int x = DisplayRectangle.X;
			int y = DisplayRectangle.Y;

			for (int i = 0; i < column_widths.Length - 1; i++) {
				x += column_widths[i] + 1;

				g.DrawLine (SystemPens.ControlDark, new Point (x, 1), new Point (x, Bottom - 2));
			}

			for (int j = 0; j < row_heights.Length - 1; j++) {
				y += row_heights[j] + 1;

				g.DrawLine (SystemPens.ControlDark, new Point (1, y), new Point (Right - 2, y));
			}
		}

		private void DrawInsetBorder (Graphics g, Rectangle rect)
		{
			ControlPaint.DrawBorder3D (g, rect, Border3DStyle.Etched);
			
			int x = DisplayRectangle.X;
			int y = DisplayRectangle.Y;

			for (int i = 0; i < column_widths.Length - 1; i++) {
				x += column_widths[i] + 2;

				g.DrawLine (SystemPens.ControlDark, new Point (x, 1), new Point (x, Bottom - 3));
				g.DrawLine (Pens.White, new Point (x + 1, 1), new Point (x + 1, Bottom - 3));
			}

			for (int j = 0; j < row_heights.Length - 1; j++) {
				y += row_heights[j] + 2;

				g.DrawLine (SystemPens.ControlDark, new Point (1, y), new Point (Right - 3, y));
				g.DrawLine (Pens.White, new Point (1, y + 1), new Point (Right - 3, y + 1));
			}
		}

		private void DrawOutsetBorder (Graphics g, Rectangle rect)
		{
			g.DrawRectangle (SystemPens.ControlDark, new Rectangle (rect.Left + 1, rect.Top + 1, rect.Width - 2, rect.Height - 2));
			g.DrawRectangle (Pens.White, new Rectangle (rect.Left, rect.Top, rect.Width - 2, rect.Height - 2));

			int x = DisplayRectangle.X;
			int y = DisplayRectangle.Y;

			for (int i = 0; i < column_widths.Length - 1; i++) {
				x += column_widths[i] + 2;

				g.DrawLine (Pens.White, new Point (x, 1), new Point (x, Bottom - 3));
				g.DrawLine (SystemPens.ControlDark, new Point (x + 1, 1), new Point (x + 1, Bottom - 3));
			}

			for (int j = 0; j < row_heights.Length - 1; j++) {
				y += row_heights[j] + 2;

				g.DrawLine (Pens.White, new Point (1, y), new Point (Right - 3, y));
				g.DrawLine (SystemPens.ControlDark, new Point (1, y + 1), new Point (Right - 3, y + 1));
			}
		}

		private void DrawOutsetDoubleBorder (Graphics g, Rectangle rect)
		{
			rect.Width -= 1;
			rect.Height -= 1;
			
			g.DrawRectangle (SystemPens.ControlDark, new Rectangle (rect.Left + 2, rect.Top + 2, rect.Width - 2, rect.Height - 2));
			g.DrawRectangle (Pens.White, new Rectangle (rect.Left, rect.Top, rect.Width - 2, rect.Height - 2));

			int x = DisplayRectangle.X;
			int y = DisplayRectangle.Y;

			for (int i = 0; i < column_widths.Length - 1; i++) {
				x += column_widths[i] + 3;

				g.DrawLine (Pens.White, new Point (x, 3), new Point (x, Bottom - 5));
				g.DrawLine (SystemPens.ControlDark, new Point (x + 2, 3), new Point (x + 2, Bottom - 5));
			}

			for (int j = 0; j < row_heights.Length - 1; j++) {
				y += row_heights[j] + 3;

				g.DrawLine (Pens.White, new Point (3, y), new Point (Right - 4, y));
				g.DrawLine (SystemPens.ControlDark, new Point (3, y + 2), new Point (Right - 4, y + 2));
			}

			x = DisplayRectangle.X;
			y = DisplayRectangle.Y;

			for (int i = 0; i < column_widths.Length - 1; i++) {
				x += column_widths[i] + 3;

				g.DrawLine (ThemeEngine.Current.ResPool.GetPen (BackColor), new Point (x + 1, 3), new Point (x + 1, Bottom - 5));
			}

			for (int j = 0; j < row_heights.Length - 1; j++) {
				y += row_heights[j] + 3;

				g.DrawLine (ThemeEngine.Current.ResPool.GetPen (BackColor), new Point (3, y + 1), new Point (Right - 4, y + 1));
			}
		}
	
		private void DrawInsetDoubleBorder (Graphics g, Rectangle rect)
		{
			rect.Width -= 1;
			rect.Height -= 1;
			
			g.DrawRectangle (Pens.White, new Rectangle (rect.Left + 2, rect.Top + 2, rect.Width - 2, rect.Height - 2));
			g.DrawRectangle (SystemPens.ControlDark, new Rectangle (rect.Left, rect.Top, rect.Width - 2, rect.Height - 2));

			int x = DisplayRectangle.X;
			int y = DisplayRectangle.Y;

			for (int i = 0; i < column_widths.Length - 1; i++) {
				x += column_widths[i] + 3;

				g.DrawLine (SystemPens.ControlDark, new Point (x, 3), new Point (x, Bottom - 5));
				g.DrawLine (Pens.White, new Point (x + 2, 3), new Point (x + 2, Bottom - 5));
			}

			for (int j = 0; j < row_heights.Length - 1; j++) {
				y += row_heights[j] + 3;

				g.DrawLine (SystemPens.ControlDark, new Point (3, y), new Point (Right - 4, y));
				g.DrawLine (Pens.White, new Point (3, y + 2), new Point (Right - 4, y + 2));
			}

			x = DisplayRectangle.X;
			y = DisplayRectangle.Y;
			
			for (int i = 0; i < column_widths.Length - 1; i++) {
				x += column_widths[i] + 3;

				g.DrawLine (ThemeEngine.Current.ResPool.GetPen (BackColor), new Point (x + 1, 3), new Point (x + 1, Bottom - 5));
			}

			for (int j = 0; j < row_heights.Length - 1; j++) {
				y += row_heights[j] + 3;

				g.DrawLine (ThemeEngine.Current.ResPool.GetPen (BackColor), new Point (3, y + 1), new Point (Right - 4, y + 1));
			}
		}

		internal override Size GetPreferredSizeCore (Size proposedSize)
		{
			// If the tablelayoutowner is autosize, we have to make sure it is big enough
			// to hold every non-autosize control
			actual_positions = (LayoutEngine as TableLayout).CalculateControlPositions (this, Math.Max (ColumnCount, 1), Math.Max (RowCount, 1));
			
			// Use actual row/column counts, not user set ones
			int actual_cols = actual_positions.GetLength (0);
			int actual_rows = actual_positions.GetLength (1);
			
			// Find the largest column-span/row-span values.  A table entry that spans more than one
			// column (row) should not be treated as though it's width (height) all belongs to the
			// first column (row), but should be spread out across all the columns (rows) that are
			// spanned.  So we need to keep track of the widths (heights) of spans as well as
			// individual columns (rows).
			int max_colspan = 1, max_rowspan = 1;
			foreach (Control c in Controls)
			{
				max_colspan = Math.Max(max_colspan, GetColumnSpan(c));
				max_rowspan = Math.Max(max_rowspan, GetRowSpan(c));
			}

			// Figure out how wide the owner needs to be
			int[] column_widths = new int[actual_cols];
			// Keep track of widths for spans as well as columns. column_span_widths[i,j] stores
			// the maximum width for items column i than have a span of j+1 (ie, covers columns
			// i through i+j).
			int[,] column_span_widths = new int[actual_cols, max_colspan];
			int[] biggest = new int[max_colspan];
			float total_column_percentage = 0f;
			
			// Figure out how wide each column wants to be
			for (int i = 0; i < actual_cols; i++) {
				if (i < ColumnStyles.Count && ColumnStyles[i].SizeType == SizeType.Percent)
					total_column_percentage += ColumnStyles[i].Width;
				int absolute_width = -1;
				if (i < ColumnStyles.Count && ColumnStyles[i].SizeType == SizeType.Absolute)
					absolute_width = (int)ColumnStyles[i].Width;	// use the absolute width if it's absolute!

				for (int s = 0; s < max_colspan; ++s)
					biggest[s] = 0;

				for (int j = 0; j < actual_rows; j++) {
					Control c = actual_positions[i, j];

					if (c != null) {
						int colspan = GetColumnSpan (c);
						if (colspan == 0)
							continue;
						if (colspan == 1 && absolute_width > -1)
							biggest[0] = absolute_width;	// use the absolute width if the column has absolute width assigned!
						else if (!c.AutoSize)
							biggest[colspan-1] = Math.Max (biggest[colspan-1], c.ExplicitBounds.Width + c.Margin.Horizontal + Padding.Horizontal);
						else
							biggest[colspan-1] = Math.Max (biggest[colspan-1], c.PreferredSize.Width + c.Margin.Horizontal + Padding.Horizontal);
					}
					else if (absolute_width > -1) {
						biggest[0] = absolute_width;
					}
				}

				for (int s = 0; s < max_colspan; ++s)
					column_span_widths[i,s] = biggest[s];
			}

			for (int i = 0; i < actual_cols; ++i) {
				for (int s = 1; s < max_colspan; ++s) {
					if (column_span_widths[i,s] > 0)
						AdjustWidthsForSpans (column_span_widths, i, s);
				}
				column_widths[i] = column_span_widths[i,0];
			}

			// Because percentage based rows divy up the remaining space,
			// we have to make the owner big enough so that all the rows
			// get bigger, even if we only need one to be bigger.
			int non_percent_total_width = 0;
			int percent_total_width = 0;

			for (int i = 0; i < actual_cols; i++) {
				if (i < ColumnStyles.Count && ColumnStyles[i].SizeType == SizeType.Percent)
					percent_total_width = Math.Max (percent_total_width, (int)(column_widths[i] / ((ColumnStyles[i].Width) / total_column_percentage)));
				else
					non_percent_total_width += column_widths[i];
			}

			int border_width = GetCellBorderWidth (CellBorderStyle);
			int needed_width = non_percent_total_width + percent_total_width + (border_width * (actual_cols + 1));

			// Figure out how tall the owner needs to be
			int[] row_heights = new int[actual_rows];
			int[,] row_span_heights = new int[actual_rows, max_rowspan];
			biggest = new int[max_rowspan];
			float total_row_percentage = 0f;
		
			// Figure out how tall each row wants to be
			for (int j = 0; j < actual_rows; j++) {
				if (j < RowStyles.Count && RowStyles[j].SizeType == SizeType.Percent)
					total_row_percentage += RowStyles[j].Height;
				int absolute_height = -1;
				if (j < RowStyles.Count && RowStyles[j].SizeType == SizeType.Absolute)
					absolute_height = (int)RowStyles[j].Height;	// use the absolute height if it's absolute!
					
				for (int s = 0; s < max_rowspan; ++s)
					biggest[s] = 0;
				
				for (int i = 0; i < actual_cols; i++) {
					Control c = actual_positions[i, j];

					if (c != null) {
						int rowspan = GetRowSpan (c);
						if (rowspan == 0)
							continue;
						if (rowspan == 1 && absolute_height > -1)
							biggest[0] = absolute_height;    // use the absolute height if the row has absolute height assigned!
						else if (!c.AutoSize)
							biggest[rowspan-1] = Math.Max (biggest[rowspan-1], c.ExplicitBounds.Height + c.Margin.Vertical + Padding.Vertical);
						else
							biggest[rowspan-1] = Math.Max (biggest[rowspan-1], c.PreferredSize.Height + c.Margin.Vertical + Padding.Vertical);
					}
					else if (absolute_height > -1) {
						biggest[0] = absolute_height;
					}
				}

				for (int s = 0; s < max_rowspan; ++s)
					row_span_heights[j,s] = biggest[s];
			}

			for (int j = 0; j < actual_rows; ++j) {
				for (int s = 1; s < max_rowspan; ++s) {
					if (row_span_heights[j,s] > 0)
						AdjustHeightsForSpans (row_span_heights, j, s);
				}
				row_heights[j] = row_span_heights[j,0];
			}
			
			// Because percentage based rows divy up the remaining space,
			// we have to make the owner big enough so that all the rows
			// get bigger, even if we only need one to be bigger.
			int non_percent_total_height = 0;
			int percent_total_height = 0;

			for (int j = 0; j < actual_rows; j++) {
				if (j < RowStyles.Count && RowStyles[j].SizeType == SizeType.Percent)
					percent_total_height = Math.Max (percent_total_height, (int)(row_heights[j] / ((RowStyles[j].Height) / total_row_percentage)));
				else
					non_percent_total_height += row_heights[j];
			}

			int needed_height = non_percent_total_height + percent_total_height + (border_width * (actual_rows + 1));

			return new Size (needed_width, needed_height);
		}

		/// <summary>
		/// Adjust the widths of the columns underlying a span if necessary.
		/// </summary>
		private void AdjustWidthsForSpans (int[,] widths, int col, int span)
		{
			// Get the combined width of the columns underlying the span.
			int existing_width = 0;
			for (int i = col; i <= col+span; ++i)
				existing_width += widths[i,0];
			if (widths[col,span] > existing_width)
			{
				// We need to expand one or more of the underlying columns to fit the span,
				// preferably ones that are not Absolute style.
				int excess = widths[col,span] - existing_width;
				int remaining = excess;
				List<int> adjusting = new List<int>();
				List<float> adjusting_widths = new List<float>();
				for (int i = col; i <= col+span; ++i) {
					if (i < ColumnStyles.Count && ColumnStyles[i].SizeType != SizeType.Absolute) {
						adjusting.Add(i);
						adjusting_widths.Add((float)widths[i,0]);
					}
				}
				if (adjusting.Count == 0) {
					// if every column is Absolute, spread the gain across every column
					for (int i = col; i <= col+span; ++i) {
						adjusting.Add(i);
						adjusting_widths.Add((float)widths[i,0]);
					}
				}
				float original_total = 0f;
				foreach (var w in adjusting_widths)
					original_total += w;
				// Divide up the needed additional width proportionally.
				for (int i = 0; i < adjusting.Count; ++i) {
					var idx = adjusting[i];
					var percent = adjusting_widths[i] / original_total;
					var adjust = (int)(percent * excess);
					widths[idx,0] += adjust;
					remaining -= adjust;
				}
				// Any remaining fragment (1 or 2 pixels?) is divided evenly.
				while (remaining > 0) {
					for (int i = 0; i < adjusting.Count && remaining > 0; ++i) {
						++widths[adjusting[i],0];
						--remaining;
					}
				}
			}
		}

		/// <summary>
		/// Adjust the heights of the rows underlying a span if necessary.
		/// </summary>
		private void AdjustHeightsForSpans (int[,] heights, int row, int span)
		{
			// Get the combined height of the rows underlying the span.
			int existing_height = 0;
			for (int i = row; i <= row+span; ++i)
				existing_height += heights[i,0];
			if (heights[row,span] > existing_height)
			{
				// We need to expand one or more of the underlying rows to fit the span,
				// preferably ones that are not Absolute style.
				int excess = heights[row,span] - existing_height;
				int remaining = excess;
				List<int> adjusting = new List<int>();
				List<float> adjusting_heights = new List<float>();
				for (int i = row; i <= row+span; ++i) {
					if (i < RowStyles.Count && RowStyles[i].SizeType != SizeType.Absolute) {
						adjusting.Add(i);
						adjusting_heights.Add((float)heights[i,0]);
					}
				}
				if (adjusting.Count == 0) {
					// if every row is Absolute, spread the gain across every row
					for (int i = row; i <= row+span; ++i) {
						adjusting.Add(i);
						adjusting_heights.Add((float)heights[i,0]);
					}
				}
				float original_total = 0f;
				foreach (var w in adjusting_heights)
					original_total += w;
				// Divide up the needed additional height proportionally.
				for (int i = 0; i < adjusting.Count; ++i) {
					var idx = adjusting[i];
					var percent = adjusting_heights[i] / original_total;
					var adjust = (int)(percent * excess);
					heights[idx,0] += adjust;
					remaining -= adjust;
				}
				// Any remaining fragment (1 or 2 pixels?) is divided evenly.
				while (remaining > 0) {
					for (int i = 0; i < adjusting.Count && remaining > 0; ++i) {
						++heights[adjusting[i],0];
						--remaining;
					}
				}
			}
		}
		#endregion
		
		#region Public Events
		static object CellPaintEvent = new object ();

		public event TableLayoutCellPaintEventHandler CellPaint {
			add { Events.AddHandler (CellPaintEvent, value); }
			remove { Events.RemoveHandler (CellPaintEvent, value); }
		}
		#endregion
		
		#region IExtenderProvider
		bool IExtenderProvider.CanExtend (object obj)
		{
			if (obj is Control)
				if ((obj as Control).Parent == this)
					return true;

			return false;
		}
		#endregion
		
	}
}
