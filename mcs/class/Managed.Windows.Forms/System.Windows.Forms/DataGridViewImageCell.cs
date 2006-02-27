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

	public class DataGridViewImageCell : DataGridViewCell {

		private object defaultNewRowValue;
		private string description;
		private DataGridViewImageCellLayout imageLayout;
		private bool valueIsIcon;

		public DataGridViewImageCell (bool valueIsIcon) {
			this.valueIsIcon = valueIsIcon;
			this.imageLayout = DataGridViewImageCellLayout.NotSet;
		}

		public DataGridViewImageCell () : this(false) {
		}

		public override object DefaultNewRowValue {
			get { return defaultNewRowValue; }
		}

		public string Description {
			get { return description; }
			set { description = value; }
		}

		public override Type EditType {
			get { return null; }
		}

		public override Type FormattedValueType {
			get { return (valueIsIcon)? typeof(Icon) : typeof(Image); }
		}

		public DataGridViewImageCellLayout ImageLayout {
			get { return imageLayout; }
			set {
				if (!Enum.IsDefined(typeof(DataGridViewImageCellLayout), value)) {
					throw new InvalidEnumArgumentException("Value is invalid image cell layout.");
				}
				imageLayout = value;
			}
		}

		public bool ValueIsIcon {
			get { return valueIsIcon; }
			set { valueIsIcon = value; }
		}

		public override Type ValueType {
			get {
				if (base.ValueType != null) {
					return base.ValueType;
				}
				if (OwningColumn != null) {
					return OwningColumn.ValueType;
				}
				if (valueIsIcon) {
					return typeof(Icon);
				}
				else {
					return typeof(Image);
				}
			}
			set { base.ValueType = value; }
		}

		public override object Clone () {
			DataGridViewImageCell cell = (DataGridViewImageCell) base.Clone();
			cell.defaultNewRowValue = this.defaultNewRowValue;
			cell.description = this.description;
			cell.valueIsIcon = this.valueIsIcon;
			return cell;
		}

		public override string ToString () {
			return GetType().Name;
		}

		protected override AccessibleObject CreateAccessibilityInstance () {
			return new DataGridViewImageCellAccessibleObject(this);
		}

		protected override Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex) {
			throw new NotImplementedException();
		}

		protected override Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex) {
			throw new NotImplementedException();
		}

		protected override object GetFormattedValue (object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context) {
			throw new NotImplementedException();
		}

		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize) {
			throw new NotImplementedException();
		}

		protected override object GetValue (int rowIndex) {
			throw new NotImplementedException();
		}

		protected override void OnMouseEnter (int rowIndex) {
			throw new NotImplementedException();
		}

		protected override void OnMouseLeave (int rowIndex) {
			throw new NotImplementedException();
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementeState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
			throw new NotImplementedException();
		}

		protected class DataGridViewImageCellAccessibleObject : DataGridViewCellAccessibleObject {

			public DataGridViewImageCellAccessibleObject (DataGridViewCell owner) : base(owner) {
			}

			public override string DefaultAction {
				get { return ""; }
			}

			public override string Description {
				get { return (Owner as DataGridViewImageCell).Description; }
			}

			public override void DoDefaultAction () {
				// The DataGridViewImageCell has no default action.
			}

			public override int GetChildCount () {
				return -1;
			}

		}

	}

}

#endif
