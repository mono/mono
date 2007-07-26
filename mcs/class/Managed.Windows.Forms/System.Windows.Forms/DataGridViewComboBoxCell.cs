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

		private DataGridViewComboBoxEditingControl editingControl;

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

		[DefaultValue (true)]
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

		[DefaultValue ("")]
		public virtual string DisplayMember {
			get { return displayMember; }
			set { displayMember = value; }
		}

		[DefaultValue (DataGridViewComboBoxDisplayStyle.DropDownButton)]
		public DataGridViewComboBoxDisplayStyle DisplayStyle {
			get { return displayStyle; }
			set { displayStyle = value; }
		}

		[DefaultValue (false)]
		public bool DisplayStyleForCurrentCellOnly {
			get { return displayStyleForCurrentCellOnly; }
			set { displayStyleForCurrentCellOnly = value; }
		}

		[DefaultValue (1)]
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

		[DefaultValue (FlatStyle.Standard)]
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

		[Browsable (false)]
		public virtual ObjectCollection Items {
			get { return items; }
		}

		[DefaultValue (8)]
		public virtual int MaxDropDownItems {
			get { return maxDropDownItems; }
			set {
				if (value < 1 || value > 100) {
					throw new ArgumentOutOfRangeException("Value is less than 1 or greater than 100.");
				}
				maxDropDownItems = value;
			}
		}

		[DefaultValue (false)]
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

		[DefaultValue ("")]
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
			this.DataGridView.EditingControlInternal = null;
		}

		public override void InitializeEditingControl (int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle) {
			base.InitializeEditingControl (rowIndex, initialFormattedValue, dataGridViewCellStyle);
			
			editingControl = DataGridView.EditingControl as DataGridViewComboBoxEditingControl;
			
			if (editingControl == null)
				return;
			
			// A simple way to check if the control has
			// been initialized already.
			if (editingControl.Items.Count > 0)
				return;
			
			editingControl.DropDownStyle = ComboBoxStyle.DropDownList;
			editingControl.Text = initialFormattedValue == null ? string.Empty : initialFormattedValue.ToString ();
			editingControl.SelectedIndexChanged += new EventHandler (editingControl_SelectedIndexChanged);
			editingControl.Items.Clear ();
			editingControl.Items.AddRange (this.Items);		
		}

		void editingControl_SelectedIndexChanged (object sender, EventArgs e)
		{
			Value = editingControl.SelectedItem;
		}

		public override bool KeyEntersEditMode (KeyEventArgs e) {
			throw new NotImplementedException();
		}

		public override object ParseFormattedValue (object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter) {
			throw new NotImplementedException();
		}

		public override string ToString () {
			return string.Format ("DataGridViewComboBoxCell {{ ColumnIndex={0}, RowIndex={1} }}", ColumnIndex, RowIndex);
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
			// Here we're supposed to do something with DataSource, etc, according to MSDN.
			base.OnDataGridViewChanged ();
		}

		protected override void OnEnter (int rowIndex, bool throughMouseClick) {
			base.OnEnter (rowIndex, throughMouseClick);
		}

		protected override void OnLeave (int rowIndex, bool throughMouseClick) {
			base.OnLeave (rowIndex, throughMouseClick);
		}

		protected override void OnMouseClick (DataGridViewCellMouseEventArgs e) {
			base.OnMouseClick (e);
		}

		protected override void OnMouseEnter (int rowIndex) {
			base.OnMouseEnter (rowIndex);
		}

		protected override void OnMouseLeave (int rowIndex) {
			base.OnMouseLeave (rowIndex);
		}

		protected override void OnMouseMove (DataGridViewCellMouseEventArgs e) {
			//Console.WriteLine ("MouseMove (Location: {0}", e.Location);
			base.OnMouseMove (e);
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, 
				int rowIndex, DataGridViewElementStates elementeState, object value, 
				object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, 
				DataGridViewPaintParts paintParts) {
			
			
			Rectangle button_area, text_area;
			text_area = cellBounds;
			button_area = CalculateButtonArea (cellBounds);
			
			graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (cellStyle.BackColor), cellBounds);
			ThemeEngine.Current.CPDrawComboButton (graphics, button_area, ButtonState.Normal);
			
			string text;
			if (formattedValue == null)
				text = string.Empty;
			else {
				text = formattedValue.ToString ();
			}
			
			graphics.DrawString (text, cellStyle.Font, ThemeEngine.Current.ResPool.GetSolidBrush (cellStyle.ForeColor), text_area, StringFormat.GenericTypographic);
		}

		private Rectangle CalculateButtonArea (Rectangle cellBounds)
		{
			Rectangle button_area, text_area;
			int border = ThemeEngine.Current.Border3DSize.Width;
			const int button_width = 16;

			text_area = cellBounds;

			button_area = cellBounds;
			button_area.X = text_area.Right - button_width - border;
			button_area.Y = text_area.Y + border;
			button_area.Width = button_width;
			button_area.Height = text_area.Height - 2 * border;
			
			return button_area;
		}

		[ListBindable (false)]
		public class ObjectCollection : IList, ICollection, IEnumerable {

			private ArrayList list;

			//private DataGridViewComboBoxCell owner;

			public ObjectCollection (DataGridViewComboBoxCell owner) {
				//this.owner = owner;
				list = new ArrayList();
			}

			public int Count {
				get { return list.Count; }
			}

			bool IList.IsFixedSize {
				get { return list.IsFixedSize; }
			}

			public bool IsReadOnly {
				get { return list.IsReadOnly; }
			}

			bool ICollection.IsSynchronized {
				get { return list.IsSynchronized; }
			}

			object ICollection.SyncRoot {
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

			public void AddRange (params object[] items) {
				list.AddRange(items);
			}

			public void Clear () {
				list.Clear();
			}

			public bool Contains (object value) {
				return list.Contains(value);
			}

			void ICollection.CopyTo (Array destination, int arrayIndex)
			{
				CopyTo ((object[])destination, arrayIndex);
			}

			public void CopyTo (object[] destination, int arrayIndex) {
				list.CopyTo(destination, arrayIndex);
			}

			public IEnumerator GetEnumerator () {
				return list.GetEnumerator();
			}

			public int IndexOf (object value) {
				return list.IndexOf(value);
			}

			public void Insert (int index, object item) {
				list.Insert(index, item);
			}

			public void Remove (object value) {
				list.Remove(value);
			}

			public void RemoveAt (int index) {
				list.RemoveAt(index);
			}


			int IList.Add (object value)
			{
				return Add (value);
			}

		}

	}

}

#endif
