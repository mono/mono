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

using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {

	public class DataGridViewComboBoxCell : DataGridViewCell {

		private bool autoComplete;
		private object dataSource;
		private string displayMember;
		private DataGridViewComboBoxDisplayStyle displayStyle;
		private bool displayStyleForCurrentCellOnly;
		private int dropDownWidth;
		private FlatStyle flatStyle;
		private ObjectCollection items;
		private int maxDropDownItems;
		private bool sorted;
		private string valueMember;

		public DataGridViewComboBoxCell () : base() {
			autoComplete = true;
			dataSource = null;
			displayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
			displayStyleForCurrentCellOnly = false;
			dropDownWidth = 1;
			flatStyle = FlatStyle.Standard;
			items = new ObjectCollection(this);
			maxDropDownItems = 8;
			sorted = false;
		}

		public virtual bool AutoComplete {
			get { return autoComplete; }
			set { autoComplete = value; }
		}

		public virtual object DataSource {
			get { return dataSource; }
			set {
				if (value is IList || value is IListSource || value == null) {
					dataSource = value;
					return;
				}
				throw new Exception("Value is no IList, IListSource or null.");
			}
		}

		public virtual string DisplayMember {
			get { return displayMember; }
			set { displayMember = value; }
		}

		public DataGridViewComboBoxDisplayStyle DisplayStyle {
			get { return displayStyle; }
			set { displayStyle = value; }
		}

		public bool DisplayStyleForCurrentCellOnly {
			get { return displayStyleForCurrentCellOnly; }
			set { displayStyleForCurrentCellOnly = value; }
		}

		public virtual int DropDownWidth {
			get { return dropDownWidth; }
			set {
				if (value < 1) {
					throw new ArgumentOutOfRangeException("Value is less than 1.");
				}
				dropDownWidth = value;
			}
		}

		public override Type EditType {
			get { return typeof(DataGridViewComboBoxEditingControl); }
		}

		public FlatStyle FlatStyle {
			get { return flatStyle; }
			set {
				if (!Enum.IsDefined(typeof(FlatStyle), value)) {
					throw new InvalidEnumArgumentException("Value is not valid FlatStyle.");
				}
				flatStyle = value;
			}
		}

		public override Type FormattedValueType {
			get { return typeof(string); }
		}

		public virtual ObjectCollection Items {
			get { return items; }
		}

		public virtual int MaxDropDownItems {
			get { return maxDropDownItems; }
			set {
				if (value < 1 || value > 100) {
					throw new ArgumentOutOfRangeException("Value is less than 1 or greater than 100.");
				}
				maxDropDownItems = value;
			}
		}

		public virtual bool Sorted {
			get { return sorted; }
			set {
				/*
				if () {
					throw new ArgumentException("Cannot sort a cell attached to a data source.");
				}
				*/
				sorted = value;
			}
		}

		public virtual string ValueMember {
			get { return valueMember; }
			set { valueMember = value; }
		}

		public override Type ValueType {
			get { return typeof(string); }
		}

		public override object Clone () {
			DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell) base.Clone();
			cell.autoComplete = this.autoComplete;
			cell.dataSource = this.dataSource;
			cell.displayStyle = this.displayStyle;
			cell.displayStyleForCurrentCellOnly = this.displayStyleForCurrentCellOnly;
			cell.dropDownWidth = this.dropDownWidth;
			cell.flatStyle = this.flatStyle;
			cell.items.AddRange(this.items);
			cell.maxDropDownItems = this.maxDropDownItems;
			cell.sorted = this.sorted;
			return cell;
		}

		public override void DetachEditingControl () {
			throw new NotImplementedException();
		}

		public override void InitializeEditingControl (int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle) {
			throw new NotImplementedException();
		}

		public override bool KeyEntersEditMode (KeyEventArgs e) {
			throw new NotImplementedException();
		}

		public override object ParseFormattedValue (object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter) {
			throw new NotImplementedException();
		}

		public override string ToString () {
			throw new NotImplementedException();
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

		protected override void OnDataGridViewChanged () {
			throw new NotImplementedException();
		}

		protected override void OnEnter (int rowIndex, bool throughMouseClick) {
			throw new NotImplementedException();
		}

		protected override void OnLeave (int rowIndex, bool throughMouseClick) {
			throw new NotImplementedException();
		}

		protected override void OnMouseClick (DataGridViewCellMouseEventArgs e) {
			throw new NotImplementedException();
		}

		protected override void OnMouseEnter (int rowIndex) {
			throw new NotImplementedException();
		}

		protected override void OnMouseLeave (int rowIndex) {
			throw new NotImplementedException();
		}

		protected override void OnMouseMove (DataGridViewCellMouseEventArgs e) {
			throw new NotImplementedException();
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementeState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
			throw new NotImplementedException();
		}

		public class ObjectCollection : IList, ICollection, IEnumerable {

			private ArrayList list;

			private DataGridViewComboBoxCell owner;

			public ObjectCollection (DataGridViewComboBoxCell owner) {
				this.owner = owner;
				list = new ArrayList();
			}

			public virtual int Count {
				get { return list.Count; }
			}

			public virtual bool IsFixedSize {
				get { return list.IsFixedSize; }
			}

			public virtual bool IsReadOnly {
				get { return list.IsReadOnly; }
			}

			public virtual bool IsSynchronized {
				get { return list.IsSynchronized; }
			}

			public virtual object SyncRoot {
				get { return list.SyncRoot; }
			}

			public virtual object this [int index] {
				get { return list[index]; }
				set { list[index] = value; }
			}

			public int Add (object item) {
				return list.Add(item);
			}

			public void AddRange (ObjectCollection value) {
				list.AddRange(value.list);
			}

			public void AddRange (object[] items) {
				list.AddRange(items);
			}

			//public sealed void Clear () {
			public void Clear () {
				list.Clear();
			}

			//public sealed bool Contains (object value) {
			public bool Contains (object value) {
				return list.Contains(value);
			}

			public void CopyTo (Array destination, int arrayIndex) {
				list.CopyTo(destination, arrayIndex);
			}

			//public sealed IEnumerator GetEnumerator () {
			public IEnumerator GetEnumerator () {
				return list.GetEnumerator();
			}

			//public sealed int IndexOf (object value) {
			public int IndexOf (object value) {
				return list.IndexOf(value);
			}

			//public sealed void Insert (int index, object item) {
			public void Insert (int index, object item) {
				list.Insert(index, item);
			}

			//public sealed void Remove (object value) {
			public void Remove (object value) {
				list.Remove(value);
			}

			//public sealed void RemoveAt (int index) {
			public void RemoveAt (int index) {
				list.RemoveAt(index);
			}

		}

	}

}

#endif
