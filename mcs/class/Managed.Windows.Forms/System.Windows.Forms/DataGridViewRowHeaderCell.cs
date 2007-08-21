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


#if NET_2_0

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
			throw new NotImplementedException();
		}

		public override DataGridViewCellStyle GetInheritedStyle (DataGridViewCellStyle inheritedCellStyle, int rowIndex, bool includeColors)
		{
			DataGridViewCellStyle result;

			if (HasStyle) {
				result = Style;
			} else {
				result = DataGridView.RowHeadersDefaultCellStyle;
			}

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
			throw new NotImplementedException();
		}

		protected override Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			throw new NotImplementedException();
		}

		protected internal override string GetErrorText (int rowIndex)
		{
			throw new NotImplementedException();
		}

		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
		{
			throw new NotImplementedException();
		}

		protected override object GetValue (int rowIndex)
		{
			if (headerText != null)
				return headerText;
				
			return null;
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			graphics.FillRectangle(new SolidBrush(cellStyle.BackColor), cellBounds);
			if ((cellState & DataGridViewElementStates.Selected) != 0) {
				graphics.DrawString("*", cellStyle.Font, new SolidBrush(cellStyle.ForeColor), cellBounds, StringFormat.GenericDefault);
			}
			PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
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

		}

	}

}

#endif
