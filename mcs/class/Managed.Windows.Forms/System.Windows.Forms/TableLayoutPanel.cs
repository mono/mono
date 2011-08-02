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
					value.isSerialized = false;
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
			
			// Figure out how wide the owner needs to be
			int[] column_widths = new int[actual_cols];
			float total_column_percentage = 0f;
			
			// Figure out how tall each column wants to be
			for (int i = 0; i < actual_cols; i++) {
				if (i < ColumnStyles.Count && ColumnStyles[i].SizeType == SizeType.Percent)
					total_column_percentage += ColumnStyles[i].Width;
					
				int biggest = 0;

				for (int j = 0; j < actual_rows; j++) {
					Control c = actual_positions[i, j];

					if (c != null) {
						if (!c.AutoSize)
							biggest = Math.Max (biggest, c.ExplicitBounds.Width + c.Margin.Horizontal + Padding.Horizontal);
						else
							biggest = Math.Max (biggest, c.PreferredSize.Width + c.Margin.Horizontal + Padding.Horizontal);
					}
				}

				column_widths[i] = biggest;
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


			// Figure out how tall the owner needs to be
			int[] row_heights = new int[actual_rows];
			float total_row_percentage = 0f;
		
			// Figure out how tall each row wants to be
			for (int j = 0; j < actual_rows; j++) {
				if (j < RowStyles.Count && RowStyles[j].SizeType == SizeType.Percent)
					total_row_percentage += RowStyles[j].Height;
					
				int biggest = 0;
				
				for (int i = 0; i < actual_cols; i++) {
					Control c = actual_positions[i, j];

					if (c != null) {
						if (!c.AutoSize)
							biggest = Math.Max (biggest, c.ExplicitBounds.Height + c.Margin.Vertical + Padding.Vertical);
						else
							biggest = Math.Max (biggest, c.PreferredSize.Height + c.Margin.Vertical + Padding.Vertical);
					}
				}

				row_heights[j] = biggest;
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

			int border_width = GetCellBorderWidth (CellBorderStyle);
			return new Size (non_percent_total_width + percent_total_width + (border_width * (actual_cols + 1)), non_percent_total_height + percent_total_height + (border_width * (actual_rows + 1)));
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
