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
using System.ComponentModel;

namespace System.Windows.Forms {

	public class DataGridViewElement {

		private DataGridView dataGridView;
		private DataGridViewElementStates state;

		public DataGridViewElement () {
			dataGridView = null;
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataGridView DataGridView {
			get { return dataGridView; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual DataGridViewElementStates State {
			get { return state; }
		}

		protected virtual void OnDataGridViewChanged ()
		{
		}

		protected void RaiseCellClick (DataGridViewCellEventArgs e) {
			if (dataGridView != null) {
				dataGridView.InternalOnCellClick(e);
			}
		}

		protected void RaiseCellContentClick (DataGridViewCellEventArgs e) {
			if (dataGridView != null) {
				dataGridView.InternalOnCellContentClick(e);
			}
		}

		protected void RaiseCellContentDoubleClick (DataGridViewCellEventArgs e) {
			if (dataGridView != null) {
				dataGridView.InternalOnCellContentDoubleClick(e);
			}
		}

		protected void RaiseCellValueChanged (DataGridViewCellEventArgs e) {
			if (dataGridView != null) {
				dataGridView.InternalOnCellValueChanged(e);
			}
		}

		protected void RaiseDataError (DataGridViewDataErrorEventArgs e) {
			if (dataGridView != null) {
				dataGridView.InternalOnDataError(e);
			}
		}

		protected void RaiseMouseWheel (MouseEventArgs e) {
			if (dataGridView != null) {
				dataGridView.InternalOnMouseWheel(e);
			}
		}

		internal virtual void SetDataGridView (DataGridView dataGridView) {
			if (dataGridView != this.DataGridView) {
				this.dataGridView = dataGridView;
				OnDataGridViewChanged();
			}
		}

		internal virtual void SetState (DataGridViewElementStates state) {
			this.state = state;
		}

	}

}

