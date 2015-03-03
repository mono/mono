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

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {
	public class DataGridViewHeaderCell : DataGridViewCell {

		private ButtonState buttonState;

		public DataGridViewHeaderCell ()
		{
			buttonState = ButtonState.Normal;
		}

		[Browsable (false)]
		public override bool Displayed {
			get { return base.Displayed; }
		}

		public override Type FormattedValueType {
			get { return typeof(string); } //base.FormattedValueType; }
		}

		[Browsable (false)]
		public override bool Frozen {
			get { return base.Frozen; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool ReadOnly {
			get { return base.ReadOnly; }
			set { base.ReadOnly = value; }
		}

		[Browsable (false)]
		public override bool Resizable {
			get { return base.Resizable; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Selected {
			get { return base.Selected; }
			set { base.Selected = value; }
		}

		public override Type ValueType {
			get { return base.ValueType; }
			set { base.ValueType = value; }
		}

		[Browsable (false)]
		public override bool Visible {
			get { return base.Visible; }
		}

		public override object Clone ()
		{
			DataGridViewHeaderCell result = new DataGridViewHeaderCell();
			return result;
		}

		protected override void Dispose (bool disposing)
		{
		}

		public override ContextMenuStrip GetInheritedContextMenuStrip (int rowIndex)
		{
			if (DataGridView == null)
				return null;

			if (ContextMenuStrip != null)
				return ContextMenuStrip;
			if (DataGridView.ContextMenuStrip != null)
				return DataGridView.ContextMenuStrip;

			return null;
		}

		public override DataGridViewElementStates GetInheritedState (int rowIndex)
		{
			DataGridViewElementStates result;

			result = DataGridViewElementStates.ResizableSet | State;

			return result;
		}

		public override string ToString ()
		{
			return string.Format ("DataGridViewHeaderCell {{ ColumnIndex={0}, RowIndex={1} }}", ColumnIndex, RowIndex);
		}

		protected override Size GetSize (int rowIndex)
		{
			if (DataGridView == null && rowIndex != -1)
				throw new ArgumentOutOfRangeException ("rowIndex");
			if (OwningColumn != null && rowIndex != -1)
				throw new ArgumentOutOfRangeException ("rowIndex");
			if (OwningRow != null && (rowIndex < 0 || rowIndex >= DataGridView.Rows.Count))
				throw new ArgumentOutOfRangeException ("rowIndex");
			if (OwningColumn == null && OwningRow == null && rowIndex != -1)
				throw new ArgumentOutOfRangeException ("rowIndex");
			if (OwningRow != null && OwningRow.Index != rowIndex)
				throw new ArgumentException ("rowIndex");
				
			if (DataGridView == null)
				return new Size (-1, -1);

			if (this is DataGridViewTopLeftHeaderCell)
				return new Size (DataGridView.RowHeadersWidth, DataGridView.ColumnHeadersHeight);
			if (this is DataGridViewColumnHeaderCell)
				return new Size (100, DataGridView.ColumnHeadersHeight);
			if (this is DataGridViewRowHeaderCell)
				return new Size (DataGridView.RowHeadersWidth, 22);
			
			return Size.Empty;
		}

		protected override object GetValue (int rowIndex)
		{
			return base.GetValue (rowIndex);
		}

		protected override bool MouseDownUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			if (DataGridView == null)
				return false;
				
			if (e.Button == MouseButtons.Left && Application.RenderWithVisualStyles && DataGridView.EnableHeadersVisualStyles)
				return true;
				
			return false;
		}

		protected override bool MouseEnterUnsharesRow (int rowIndex)
		{
			if (DataGridView == null)
				return false;

			if (Application.RenderWithVisualStyles && DataGridView.EnableHeadersVisualStyles)
				return true;

			return false;
		}

		protected override bool MouseLeaveUnsharesRow (int rowIndex)
		{
			if (DataGridView == null)
				return false;

			if (ButtonState != ButtonState.Normal && Application.RenderWithVisualStyles && DataGridView.EnableHeadersVisualStyles)
				return true;

			return false;
		}

		protected override bool MouseUpUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			if (DataGridView == null)
				return false;

			if (e.Button == MouseButtons.Left && Application.RenderWithVisualStyles && DataGridView.EnableHeadersVisualStyles)
				return true;

			return false;
		}

		protected override void OnMouseDown (DataGridViewCellMouseEventArgs e)
		{
			base.OnMouseDown (e);
		}

		protected override void OnMouseEnter (int rowIndex)
		{
			base.OnMouseEnter (rowIndex);
		}

		protected override void OnMouseLeave (int rowIndex)
		{
			base.OnMouseLeave (rowIndex);
		}

		protected override void OnMouseUp (DataGridViewCellMouseEventArgs e)
		{
			base.OnMouseUp (e);
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			base.Paint (graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		}
		
		protected ButtonState ButtonState {
			get { return buttonState; }
		}

	}

}

