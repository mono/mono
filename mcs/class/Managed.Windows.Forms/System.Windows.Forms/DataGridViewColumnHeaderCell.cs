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


using System;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	public class DataGridViewColumnHeaderCell : DataGridViewHeaderCell {

		private SortOrder sortGlyphDirection = SortOrder.None;
		private object header_text;
		
		public DataGridViewColumnHeaderCell () {
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public SortOrder SortGlyphDirection {
			get { return sortGlyphDirection; }
			set { sortGlyphDirection = value; }
		}

		public override object Clone () {
			return MemberwiseClone();
		}

		public override ContextMenuStrip GetInheritedContextMenuStrip (int rowIndex) {
			if (rowIndex != -1) {
				throw new ArgumentOutOfRangeException("RowIndex is not -1");
			}
			if (base.ContextMenuStrip != null) {
				return base.ContextMenuStrip;
			}
			return base.GetInheritedContextMenuStrip(rowIndex); //////////////////////////////
		}

		public override DataGridViewCellStyle GetInheritedStyle (DataGridViewCellStyle inheritedCellStyle, int rowIndex, bool includeColors)
		{
			DataGridViewCellStyle result = new DataGridViewCellStyle (DataGridView.DefaultCellStyle);
	
			result.ApplyStyle (DataGridView.ColumnHeadersDefaultCellStyle);
	
			if (HasStyle)
				result.ApplyStyle (Style);

			return result;
		}

		public override string ToString () {
			return GetType().Name;
		}

		protected override AccessibleObject CreateAccessibilityInstance () {
			return new DataGridViewColumnHeaderCellAccessibleObject(this);
		}

		protected override object GetClipboardContent (int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format) {
			
			string value;
			
			if (rowIndex != -1)
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
				if (firstCell) {
					table_prefix = "<TABLE>";
					row_prefix = "<THEAD>";
				}

				cell_prefix = "<TH>";
				cell_suffix = "</TH>";

				if (lastCell) {
					row_suffix = "</THEAD>";
					if (inLastRow) {
						table_suffix = "</TABLE>";
					}
				}
				
				if (value == null) {
					value = "&nbsp;";
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

			object o = GetValue (-1);
			
			if (o == null || o.ToString () == string.Empty)
				return Rectangle.Empty;
				
			Size s = Size.Empty;

			if (o != null)
				s = DataGridViewCell.MeasureTextSize (graphics, o.ToString (), cellStyle.Font, TextFormatFlags.Default);

			return new Rectangle (3, (DataGridView.ColumnHeadersHeight - s.Height) / 2, s.Width, s.Height);
		}

		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
		{
			object o = header_text;

			if (o != null) {
				Size s = DataGridViewCell.MeasureTextSize (graphics, o.ToString (), cellStyle.Font, TextFormatFlags.Default);
				s.Height = Math.Max (s.Height, 18);
				s.Width += 25;
				return s;
			} else
				return new Size (19, 12);
		}

		protected override object GetValue (int rowIndex) {
			if (header_text != null)
				return header_text;

			if (OwningColumn != null && !OwningColumn.HeaderTextSet)
				return OwningColumn.Name;
			
			return null;
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
			// Prepaint
			DataGridViewPaintParts pre = DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground;
			pre = pre & paintParts;

			base.Paint (graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, pre);

			// Paint content
			if ((paintParts & DataGridViewPaintParts.ContentForeground) == DataGridViewPaintParts.ContentForeground) {
				Color color = Selected ? cellStyle.SelectionForeColor : cellStyle.ForeColor;

				TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.TextBoxControl;

				Rectangle contentbounds = cellBounds;
				contentbounds.Height -= 2;
				contentbounds.Width -= 2;

				if (formattedValue != null)
					TextRenderer.DrawText (graphics, formattedValue.ToString (), cellStyle.Font, contentbounds, color, flags);
					
				Point loc = new Point (cellBounds.Right - 14, cellBounds.Y + ((cellBounds.Height - 4) / 2));
				
				if (sortGlyphDirection == SortOrder.Ascending) {
					using (Pen p = new Pen (color)) {
						graphics.DrawLine (p, loc.X + 4, loc.Y + 1, loc.X + 4, loc.Y + 2);
						graphics.DrawLine (p, loc.X + 3, loc.Y + 2, loc.X + 5, loc.Y + 2);
						graphics.DrawLine (p, loc.X + 2, loc.Y + 3, loc.X + 6, loc.Y + 3);
						graphics.DrawLine (p, loc.X + 1, loc.Y + 4, loc.X + 7, loc.Y + 4);
						graphics.DrawLine (p, loc.X + 0, loc.Y + 5, loc.X + 8, loc.Y + 5);
					}
				} else if (sortGlyphDirection == SortOrder.Descending) {
					using (Pen p = new Pen (color)) {
						graphics.DrawLine (p, loc.X + 4, loc.Y + 5, loc.X + 4, loc.Y + 4);
						graphics.DrawLine (p, loc.X + 3, loc.Y + 4, loc.X + 5, loc.Y + 4);
						graphics.DrawLine (p, loc.X + 2, loc.Y + 3, loc.X + 6, loc.Y + 3);
						graphics.DrawLine (p, loc.X + 1, loc.Y + 2, loc.X + 7, loc.Y + 2);
						graphics.DrawLine (p, loc.X + 0, loc.Y + 1, loc.X + 8, loc.Y + 1);
					}
				}
			}

			// Postpaint
			DataGridViewPaintParts post = DataGridViewPaintParts.Border;
			
			if (this is DataGridViewTopLeftHeaderCell)
				post |= DataGridViewPaintParts.ErrorIcon;
				
			post = post & paintParts;

			base.Paint (graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, post);
		}

		protected override void PaintBorder (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle)
		{
			if (ThemeEngine.Current.DataGridViewColumnHeaderCellDrawBorder (this, graphics, cellBounds))
				return;

			Pen p = GetBorderPen ();

			if (ColumnIndex == -1) {
				graphics.DrawLine (p, cellBounds.Left, cellBounds.Top, cellBounds.Left, cellBounds.Bottom - 1);
				graphics.DrawLine (p, cellBounds.Right - 1, cellBounds.Top, cellBounds.Right - 1, cellBounds.Bottom - 1);

				graphics.DrawLine (p, cellBounds.Left, cellBounds.Bottom - 1, cellBounds.Right - 1, cellBounds.Bottom - 1);
				graphics.DrawLine (p, cellBounds.Left, cellBounds.Top, cellBounds.Right - 1, cellBounds.Top);				
			} else {
				graphics.DrawLine (p, cellBounds.Left, cellBounds.Bottom - 1, cellBounds.Right - 1, cellBounds.Bottom - 1);
				graphics.DrawLine (p, cellBounds.Left, cellBounds.Top, cellBounds.Right - 1, cellBounds.Top);

				if (ColumnIndex == DataGridView.Columns.Count - 1 || ColumnIndex == -1)
					graphics.DrawLine (p, cellBounds.Right - 1, cellBounds.Top, cellBounds.Right - 1, cellBounds.Bottom - 1);
				else
					graphics.DrawLine (p, cellBounds.Right - 1, cellBounds.Top + 3, cellBounds.Right - 1, cellBounds.Bottom - 3);
			}
		}
		
		internal override void PaintPartBackground (Graphics graphics, Rectangle cellBounds, DataGridViewCellStyle style)
		{
			if (ThemeEngine.Current.DataGridViewColumnHeaderCellDrawBackground (this, graphics, cellBounds))
				return;
			base.PaintPartBackground (graphics, cellBounds, style);
		}

		protected override bool SetValue (int rowIndex, object value) {
			header_text = value;
			return true;
		}

		protected class DataGridViewColumnHeaderCellAccessibleObject : DataGridViewCellAccessibleObject {

			public DataGridViewColumnHeaderCellAccessibleObject (DataGridViewColumnHeaderCell owner) : base (owner) {
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

			public override void DoDefaultAction () {
				base.DoDefaultAction();
			}

			public override AccessibleObject Navigate (AccessibleNavigation navigationDirection) {
				return base.Navigate(navigationDirection);
			}

			public override void Select (AccessibleSelection flags)
			{
				base.Select (flags);
			}
		}

	}

}

