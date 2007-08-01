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

		public override DataGridViewCellStyle GetInheritedStyle (DataGridViewCellStyle inheritedCellStyle, int rowIndex, bool includeColors) {
			
			DataGridViewCellStyle result;
			
			if (HasStyle) {
				result = Style;
			} else {
				result = DataGridView.ColumnHeadersDefaultCellStyle;
			}
			
			return result;			
		}

		public override string ToString () {
			return GetType().Name;
		}

		protected override AccessibleObject CreateAccessibilityInstance () {
			return new DataGridViewColumnHeaderCellAccessibleObject(this);
		}

		protected override object GetClipboardContent (int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format) {
			throw new NotImplementedException();
			//////////////////////////////////////////
		}

		protected override Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex) {
			return new Rectangle();
		}

		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize) {
			throw new NotImplementedException();
		}

		protected override object GetValue (int rowIndex) {
			if (header_text != null)
				return header_text;
				
			if (OwningColumn != null)
				return OwningColumn.Name;
			
			return null;
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
			graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (cellStyle.BackColor), cellBounds);
			
			StringFormat format = cellStyle.SetAlignment ((StringFormat) StringFormat.GenericTypographic.Clone ());
			Rectangle text_rect = cellBounds;
			Rectangle borders = BorderWidths (advancedBorderStyle);
			text_rect.X += borders.X;
			text_rect.Y += borders.Y;
			text_rect.Height -= (borders.Y + borders.Height);
			text_rect.Width -= (borders.X + borders.Width);
			graphics.DrawString ((string) formattedValue, cellStyle.Font, ThemeEngine.Current.ResPool.GetSolidBrush (cellStyle.ForeColor), text_rect, format);
			PaintBorder(graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
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

			public override string Value {
				get { return base.Value; }
			}

			public override void DoDefaultAction () {
				base.DoDefaultAction();
			}

			public override AccessibleObject Navigate (AccessibleNavigation navigationDirection) {
				return base.Navigate(navigationDirection);
			}

		}

	}

}

#endif
