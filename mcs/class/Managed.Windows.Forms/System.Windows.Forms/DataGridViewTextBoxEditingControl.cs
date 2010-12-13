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
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	[ClassInterfaceAttribute(ClassInterfaceType.AutoDispatch)]
	[ComVisibleAttribute(true)]
	public class DataGridViewTextBoxEditingControl : TextBox, IDataGridViewEditingControl {

		private DataGridView editingControlDataGridView;
		private int rowIndex;
		private bool editingControlValueChanged;
		private bool repositionEditingControlOnValueChange;

		public DataGridViewTextBoxEditingControl ()
		{
			repositionEditingControlOnValueChange = false;
		}

		public virtual DataGridView EditingControlDataGridView {
			get { return editingControlDataGridView; }
			set { editingControlDataGridView = value; }
		}

		public virtual object EditingControlFormattedValue {
			get { return base.Text; }
			set { base.Text = (string) value; }
		}

		public virtual int EditingControlRowIndex {
			get { return rowIndex; }
			set { rowIndex = value; }
		}

		public virtual bool EditingControlValueChanged {
			get { return editingControlValueChanged; }
			set { editingControlValueChanged = value; }
		}

		public virtual Cursor EditingPanelCursor {
			get { return Cursors.Default; }
		}

		public virtual bool RepositionEditingControlOnValueChange {
			get { return repositionEditingControlOnValueChange; }
		}

		public virtual void ApplyCellStyleToEditingControl (DataGridViewCellStyle dataGridViewCellStyle)
		{
			Font = dataGridViewCellStyle.Font;
			BackColor = dataGridViewCellStyle.BackColor;
			ForeColor = dataGridViewCellStyle.ForeColor;
		}

		public virtual bool EditingControlWantsInputKey (Keys keyData, bool dataGridViewWantsInputKey)
		{
			switch (keyData) {
				case Keys.Left:
					return SelectionStart != 0;
				case Keys.Right:
					return SelectionStart != TextLength;
				case Keys.Down:
				case Keys.Up:
					return false;
			}
			
			return true;
		}

		public virtual object GetEditingControlFormattedValue (DataGridViewDataErrorContexts context)
		{
			return EditingControlFormattedValue;
		}

		public virtual void PrepareEditingControlForEdit (bool selectAll)
		{
			Focus();
			if (selectAll) {
				SelectAll();
			}
			editingControlValueChanged = false;
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel(e);
		}

		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged(e);
			editingControlValueChanged = true;
		}

		protected override bool ProcessKeyEventArgs (ref Message m)
		{
			return base.ProcessKeyEventArgs(ref m);
		}

	}

}

