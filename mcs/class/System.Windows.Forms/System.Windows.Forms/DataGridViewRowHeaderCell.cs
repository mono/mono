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

using System.Drawing;

namespace System.Windows.Forms {

	public class DataGridViewRowHeaderCell : DataGridViewHeaderCell {

		private string headerText;
		
		public DataGridViewRowHeaderCell ()
		{
		}

		public override object Clone ()
		{
			return MemberwiseClone();
		}

		public override ContextMenuStrip GetInheritedContextMenuStrip (int rowIndex)
		{
			if (DataGridView == null)
				return null;

			if (rowIndex < 0 || rowIndex >= DataGridView.Rows.Count)
				throw new ArgumentOutOfRangeException ("rowIndex");

			if (ContextMenuStrip != null)
				return ContextMenuStrip;

			return DataGridView.ContextMenuStrip;
		}

		public override DataGridViewCellStyle GetInheritedStyle (DataGridViewCellStyle inheritedCellStyle, int rowIndex, bool includeColors)
		{
			DataGridViewCellStyle result = new DataGridViewCellStyle (DataGridView.DefaultCellStyle);

			result.ApplyStyle (DataGridView.RowHeadersDefaultCellStyle);
				
			if (HasStyle)
				result.ApplyStyle (Style);
				
			return result;
		}

		public override string ToString ()
		{
			return base.ToString();
		}

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new DataGridViewRowHeaderCellAccessibleObject(this);
		}

		protected override object GetClipboardContent (int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format)
		{
			string value;

			if (DataGridView == null)
				return null;

			if (rowIndex < 0 || rowIndex >= DataGridView.RowCount)
				throw new ArgumentOutOfRangeException ("rowIndex");
				
			value = GetValue (rowIndex) as string;

			string table_prefix = string.Empty, cell_prefix = string.Empty, row_prefix = string.Empty;
			string table_suffix = string.Empty, cell_suffix = string.Empty, row_suffix = string.Empty;

			if (format == DataFormats.UnicodeText || format == DataFormats.Text) {
				if (lastCell && !inLastRow)
					cell_suffix = Environment.NewLine;
				else if (!lastCell)
					cell_suffix = "\t";
			} else if (format == DataFormats.CommaSeparatedValue) {
				if (lastCell && !inLastRow)
					cell_suffix = Environment.NewLine;
				else if (!lastCell)
					cell_suffix = ",";
			} else if (format == DataFormats.Html) {
				if (inFirstRow) {
					table_prefix = "<TABLE>";
				}
				row_prefix = "<TR>";

				if (lastCell) {
					row_suffix = "</TR>";
					if (inLastRow) {
						table_suffix = "</TABLE>";
					}
				}

				cell_prefix = "<TD ALIGN=\"center\">";
				cell_suffix = "</TD>";

				if (value == null) {
					value = "&nbsp;";
				} else {
					value = "<B>" + value + "</B>";
				}
			} else {
				return value;
			}

			if (value == null)
				value = string.Empty;

			value = table_prefix + row_prefix + cell_prefix + value + cell_suffix + row_suffix + table_suffix;

			return value;
			
		}

		protected override Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			if (DataGridView == null)
				return Rectangle.Empty;

			Size s = new Size (11, 18);
			return new Rectangle (24, (OwningRow.Height - s.Height) / 2, s.Width, s.Height);
		}

		protected override Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			if (DataGridView == null || string.IsNullOrEmpty (DataGridView.GetRowInternal (rowIndex).ErrorText))
				return Rectangle.Empty;

			Size error_icon = new Size (12, 11);
			return new Rectangle (new Point (Size.Width - error_icon.Width - 5, (Size.Height - error_icon.Height) / 2), error_icon);
		}

		protected internal override string GetErrorText (int rowIndex)
		{
			if (DataGridView == null)
				return string.Empty;
				
			return DataGridView.GetRowInternal (rowIndex).ErrorText;
		}

		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
		{
			object o = FormattedValue;

			if (o != null) {
				Size s = DataGridViewCell.MeasureTextSize (graphics, o.ToString (), cellStyle.Font, TextFormatFlags.Default);
				s.Height = Math.Max (s.Height, 17);
				s.Width += 48;
				return s;
			} else
				return new Size (39, 17);
		}

		protected override object GetValue (int rowIndex)
		{
			if (headerText != null)
				return headerText;
				
			return null;
		}

		[MonoInternalNote ("Needs row header cell selected/edit pencil glyphs")]
		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			// Prepaint
			DataGridViewPaintParts pre = DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground;
			pre = pre & paintParts;

			base.Paint (graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, pre);

			// Paint content background
			if ((paintParts & DataGridViewPaintParts.ContentBackground) == DataGridViewPaintParts.ContentBackground) {
				Color color = Selected ? cellStyle.SelectionForeColor : cellStyle.ForeColor;
				Pen p = ThemeEngine.Current.ResPool.GetPen (color);
				int x = cellBounds.Left + 6;

				if (DataGridView.CurrentRow != null && DataGridView.CurrentRow.Index == rowIndex) {
					DrawRightArrowGlyph (graphics, p, x, cellBounds.Top + (cellBounds.Height / 2) - 4);
					x += 7;
				}

				if (DataGridView.Rows[rowIndex].IsNewRow)
					DrawNewRowGlyph (graphics, p, x, cellBounds.Top + (cellBounds.Height / 2) - 4);
			}

			// Paint content
			if ((paintParts & DataGridViewPaintParts.ContentForeground) == DataGridViewPaintParts.ContentForeground) {
				Color color = Selected ? cellStyle.SelectionForeColor : cellStyle.ForeColor;

				TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.TextBoxControl;

				Rectangle contentbounds = cellBounds;
				contentbounds.Height -= 2;
				contentbounds.Width -= 2;

				if (formattedValue != null)
					TextRenderer.DrawText (graphics, formattedValue.ToString (), cellStyle.Font, contentbounds, color, flags);
			}

			// Postpaint
			DataGridViewPaintParts post = DataGridViewPaintParts.Border | DataGridViewPaintParts.ErrorIcon;
			post = post & paintParts;

			base.Paint (graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, post);
		}

		protected override void PaintBorder (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle)
		{
			if (ThemeEngine.Current.DataGridViewRowHeaderCellDrawBorder (this, graphics, cellBounds))
				return;

			Pen p = GetBorderPen ();

			graphics.DrawLine (p, cellBounds.Left, cellBounds.Top, cellBounds.Left, cellBounds.Bottom - 1);
			graphics.DrawLine (p, cellBounds.Right - 1, cellBounds.Top, cellBounds.Right - 1, cellBounds.Bottom - 1);

			if (RowIndex == DataGridView.Rows.Count - 1 || RowIndex == -1)
				graphics.DrawLine (p, cellBounds.Left, cellBounds.Bottom - 1, cellBounds.Right - 1, cellBounds.Bottom - 1);
			else
				graphics.DrawLine (p, cellBounds.Left + 3, cellBounds.Bottom - 1, cellBounds.Right - 3, cellBounds.Bottom - 1);

			if (RowIndex == -1)
				graphics.DrawLine (p, cellBounds.Left, cellBounds.Top, cellBounds.Right - 1, cellBounds.Top);
		}
		
		internal override void PaintPartBackground (Graphics graphics, Rectangle cellBounds, DataGridViewCellStyle style)
		{
			if (ThemeEngine.Current.DataGridViewRowHeaderCellDrawBackground (this, graphics, cellBounds))
				return;
			base.PaintPartBackground (graphics, cellBounds, style);
		}

		internal override void PaintPartSelectionBackground (Graphics graphics, Rectangle cellBounds, DataGridViewElementStates cellState, DataGridViewCellStyle cellStyle)
		{
			if (ThemeEngine.Current.DataGridViewRowHeaderCellDrawSelectionBackground (this))
				return;
			base.PaintPartSelectionBackground (graphics, cellBounds, cellState, cellStyle);
		}

		private void DrawRightArrowGlyph (Graphics g, Pen p, int x, int y)
		{
			g.DrawLine (p, x, y, x, y + 8);
			g.DrawLine (p, x + 1, y + 1, x + 1, y + 7);
			g.DrawLine (p, x + 2, y + 2, x + 2, y + 6);
			g.DrawLine (p, x + 3, y + 3, x + 3, y + 5);
			g.DrawLine (p, x + 3, y + 4, x + 4, y + 4);
		}

		private void DrawNewRowGlyph (Graphics g, Pen p, int x, int y)
		{
			g.DrawLine (p, x, y + 4, x + 8, y + 4);
			g.DrawLine (p, x + 4, y, x + 4, y + 8);
			g.DrawLine (p, x + 1, y + 1, x + 7, y + 7);
			g.DrawLine (p, x + 7, y + 1, x + 1, y + 7);
		}

		internal override Rectangle InternalErrorIconsBounds {
			get { return GetErrorIconBounds (null, null, RowIndex); }
		}
		
		protected override bool SetValue (int rowIndex, object value)
		{
			headerText = (string) value;
			return true;
		}

		protected class DataGridViewRowHeaderCellAccessibleObject : DataGridViewCellAccessibleObject {

			public DataGridViewRowHeaderCellAccessibleObject (DataGridViewRowHeaderCell owner) : base(owner)
			{
			}

			public override Rectangle Bounds {
				get { return base.Bounds; }
			}

			public override string DefaultAction {
				get { return base.DefaultAction; }
			}

			public override string Name {
				get { return base.Name; }
			}

			public override AccessibleObject Parent {
				get { return base.Parent; }
			}

			public override AccessibleRole Role {
				get { return base.Role; }
			}

			public override AccessibleStates State {
				get { return base.State; }
			}
			
			public override string Value {
				get { return base.Value; }
			}

			public override void DoDefaultAction ()
			{
				base.DoDefaultAction();
			}

			public override AccessibleObject Navigate (AccessibleNavigation navigationDirection)
			{
				return base.Navigate(navigationDirection);
			}

			public override void Select (AccessibleSelection flags)
			{
				base.Select (flags);
			}
		}

	}

}
