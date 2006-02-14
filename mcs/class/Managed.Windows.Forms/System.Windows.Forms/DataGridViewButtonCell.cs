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
using System.ComponentModel;

namespace System.Windows.Forms {

	public class DataGridViewButtonCell : DataGridViewCell {

		private FlatStyle flatStyle;
		private bool useColumnTextForButtonValue;

		public DataGridViewButtonCell () {
			useColumnTextForButtonValue = false;
		}

		public override Type EditType {
			get { return null; }
		}

		public FlatStyle FlatStyle {
			get { return flatStyle; }
			set {
				if (!Enum.IsDefined(typeof(FlatStyle), value)) {
					throw new InvalidEnumArgumentException("Value is not valid FlatStyle.");
				}
				if (value == FlatStyle.Popup) {
					throw new Exception("FlatStyle cannot be set to Popup in this control.");
				}
			}
		}

		public override Type FormattedValueType {
			get { return base.FormattedValueType; }
		}

		public bool UseColumnTextForButtonValue {
			get { return useColumnTextForButtonValue; }
			set { useColumnTextForButtonValue = value; }
		}

		public override Type ValueType {
			get { return base.ValueType; }
		}

		public override object Clone () {
			DataGridViewButtonCell result = (DataGridViewButtonCell) base.Clone();
			result.flatStyle = this.flatStyle;
			result.useColumnTextForButtonValue = this.useColumnTextForButtonValue;
			return result;
		}

		public override string ToString () {
			return GetType().Name + ": RowIndex: " + RowIndex.ToString() + "; ColumnIndex: " + ColumnIndex.ToString() + ";";
		}

		protected override AccessibleObject CreateAccessibilityInstance () {
			return new DataGridViewButtonCellAccessibleObject(this);
		}

		protected override Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex) {
			throw new NotImplementedException();
		}

		protected override Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex) {
			throw new NotImplementedException();
		}

		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize) {
			throw new NotImplementedException();
		}

		protected override object GetValue (int rowIndex) {
			throw new NotImplementedException();
		}

		protected override bool KeyDownUnsharesRow (KeyEventArgs e, int rowIndex) {
			// true if the user pressed the SPACE key without modifier keys; otherwise, false
			throw new NotImplementedException();
		}

		protected override bool KeyUpUnsharesRow (KeyEventArgs e, int rowIndex) {
			// true if the user released the SPACE key; otherwise false
			throw new NotImplementedException();

		}

		protected override bool MouseDownUnsharesRow (DataGridViewCellMouseEventArgs e) {
			return (e.Button == MouseButtons.Left);
		}

		protected override bool MouseEnterUnsharesRow (int rowIndex) {
			// true if the cell was the last cell receiving a mouse click; otherwise, false.
			throw new NotImplementedException();
		}

		protected override bool MouseLeaveUnsharesRow (int rowIndex) {
			// true if the button displayed by the cell is in the pressed state; otherwise, false.
			throw new NotImplementedException();
		}

		protected override bool MouseUpUnsharesRow (DataGridViewCellMouseEventArgs e) {
			// true if the mouse up was caused by the release of the left mouse button; otherwise false.
			throw new NotImplementedException();
		}

		protected override void OnKeyDown (KeyEventArgs e, int rowIndex) {
			// when activated by the SPACE key, this method updates the cell's user interface
			throw new NotImplementedException();
		}

		protected override void OnKeyUp (KeyEventArgs e, int rowIndex) {
			// when activated by the SPACE key, this method updates the cell's user interface
			throw new NotImplementedException();
		}

		protected override void OnLeave (int rowIndex, bool throughMouseClick) {
			throw new NotImplementedException();
		}

		protected override void OnMouseDown (DataGridViewCellMouseEventArgs e) {
			// if activated by depresing the left mouse button, this method updates the cell's user interface
			throw new NotImplementedException();
		}

		protected override void OnMouseEnter (int rowIndex) {
			throw new NotImplementedException();
		}

		protected override void OnMouseLeave (int rowIndex) {
			// if the cell's button is not in its normal state, this method causes the cell's user interface to be updated.
			throw new NotImplementedException();
		}

		protected override void OnMouseUp (DataGridViewCellMouseEventArgs e) {
			// if activated by the left mouse button, this method updates the cell's user interface
			throw new NotImplementedException();
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
			throw new NotImplementedException();
		}

		protected class DataGridViewButtonCellAccessibleObject : DataGridViewCellAccessibleObject {

			public DataGridViewButtonCellAccessibleObject (DataGridViewCell owner) : base(owner) {
			}

			public override string DefaultAction {
				get {
					if (Owner.ReadOnly) {
						return "Press";
					}
					else {
						return "";
					}
				}
			}

			public override void DoDefaultAction () {
				// causes the button in the ButtonCell to be clicked
			}

			public override int GetChildCount () {
				return -1;
			}

		}

	}

}

#endif
