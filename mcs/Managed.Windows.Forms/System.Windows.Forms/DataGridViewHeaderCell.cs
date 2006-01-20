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

namespace System.Windows.Forms {
	public class DataGridViewHeaderCell : DataGridViewCell {

		private ButtonState buttonState;

		public DataGridViewHeaderCell () {
		}

		public override bool Displayed {
			get { return base.Displayed; }
		}

		public override Type FormattedValueType {
			get { return typeof(string); } //base.FormattedValueType; }
		}

		public override bool Frozen {
			get { return base.Frozen; }
		}

		public override bool ReadOnly {
			get { return base.ReadOnly; }
			set { base.ReadOnly = value; }
		}

		public override bool Resizable {
			get { return base.Resizable; }
		}

		public override bool Selected {
			get { return base.Selected; }
			set { base.Selected = value; }
		}

		public override Type ValueType {
			get { return base.ValueType; }
			set { base.ValueType = value; }
		}

		public override bool Visible {
			get { return base.Visible; }
		}

		public override object Clone () {
			DataGridViewHeaderCell result = new DataGridViewHeaderCell();
			return result;
		}

		/*
		public override ContextMenuStrip GetInheritedContextMenuStrip (int rowIndex) {
		}
		*/

		public override DataGridViewElementStates GetInheritedState (int rowIndex) {
			throw new NotImplementedException();
		}

		public override string ToString () {
			return "";
		}

		protected ButtonState ButtonState {
			get { return buttonState; }
		}

	}

}

#endif
