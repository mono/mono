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
//	Ivan N. Zlatev <contact@i-nz.net>
//


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
		private DataGridViewComboBoxColumn owningColumnTemlate;

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
			owningColumnTemlate = null;
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
			get {
				if (DataGridView != null && DataGridView.BindingContext != null 
				    && DataSource != null && !String.IsNullOrEmpty (ValueMember)) {
					items.ClearInternal ();
					CurrencyManager dataManager = (CurrencyManager) DataGridView.BindingContext[DataSource];
					if (dataManager != null && dataManager.Count > 0) {
						foreach (object item in dataManager.List)
							items.AddInternal (item);
					}
				}

				return items;
			}
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

		// Valid only for template Cells and used as a bridge to push items
		internal DataGridViewComboBoxColumn OwningColumnTemplate {
			get { return owningColumnTemlate; }
			set { owningColumnTemlate = value; }
		}

		public override object Clone () {
			DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell) base.Clone();
			cell.autoComplete = this.autoComplete;
			cell.dataSource = this.dataSource;
			cell.displayStyle = this.displayStyle;
			cell.displayMember = this.displayMember;
			cell.valueMember = this.valueMember;
			cell.displayStyleForCurrentCellOnly = this.displayStyleForCurrentCellOnly;
			cell.dropDownWidth = this.dropDownWidth;
			cell.flatStyle = this.flatStyle;
			cell.items.AddRangeInternal(this.items);
			cell.maxDropDownItems = this.maxDropDownItems;
			cell.sorted = this.sorted;
			return cell;
		}

		public override void DetachEditingControl () {
			this.DataGridView.EditingControlInternal = null;
		}

		public override void InitializeEditingControl (int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle) {
			base.InitializeEditingControl (rowIndex, initialFormattedValue, dataGridViewCellStyle);
			
			ComboBox editingControl = DataGridView.EditingControl as ComboBox;
			
			editingControl.DropDownStyle = ComboBoxStyle.DropDownList;
			editingControl.Sorted = Sorted;
			editingControl.DataSource = null;
			editingControl.ValueMember = null;
			editingControl.DisplayMember = null;
			editingControl.Items.Clear();
			editingControl.SelectedIndex = -1;

			if (DataSource != null) {
				editingControl.DataSource = DataSource;
				editingControl.ValueMember = ValueMember;
				editingControl.DisplayMember = DisplayMember;
			} else {
				editingControl.Items.AddRange (this.Items);
				if (FormattedValue != null && editingControl.Items.IndexOf (FormattedValue) != -1)
					editingControl.SelectedItem = FormattedValue;
			}
		}

		internal void SyncItems ()
		{
			if (DataSource != null || OwningColumnTemplate == null)
				return;

			if (OwningColumnTemplate.DataGridView != null) {
				DataGridViewComboBoxEditingControl editor = OwningColumnTemplate.DataGridView.EditingControl
									    as DataGridViewComboBoxEditingControl;
				if (editor != null) {
					object selectedItem = editor.SelectedItem;
					editor.Items.Clear ();
					editor.Items.AddRange (items);
					if (editor.Items.IndexOf (selectedItem) != -1)
						editor.SelectedItem = selectedItem;
				}
			}

			// Push the new items to the column
			OwningColumnTemplate.SyncItems (Items);
		}

		public override bool KeyEntersEditMode (KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
				return true;
			if ((int)e.KeyCode >= 48 && (int)e.KeyCode <= 90)
				return true;
			if ((int)e.KeyCode >= 96 && (int)e.KeyCode <= 111)
				return true;
			if (e.KeyCode == Keys.BrowserSearch || e.KeyCode == Keys.SelectMedia)
				return true;
			if ((int)e.KeyCode >= 186 && (int)e.KeyCode <= 229)
				return true;
			if (e.KeyCode == Keys.Attn || e.KeyCode == Keys.Packet)
				return true;
			if ((int)e.KeyCode >= 248 && (int)e.KeyCode <= 254)
				return true;
			if (e.KeyCode == Keys.F4)
				return true;
			if ((e.Modifiers == Keys.Alt) && (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up))
				return true;

			return false;
		}

		public override object ParseFormattedValue (object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
		{
			return base.ParseFormattedValue (formattedValue, cellStyle, formattedValueTypeConverter, valueTypeConverter);
		}

		public override string ToString () {
			return string.Format ("DataGridViewComboBoxCell {{ ColumnIndex={0}, RowIndex={1} }}", ColumnIndex, RowIndex);
		}

		protected override Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			if (DataGridView == null)
				return Rectangle.Empty;

			object o = FormattedValue;
			Size s = Size.Empty;

			if (o != null)
				s = DataGridViewCell.MeasureTextSize (graphics, o.ToString (), cellStyle.Font, TextFormatFlags.Default);

			return new Rectangle (1, (OwningRow.Height - s.Height) / 2, s.Width - 3, s.Height);
		}

		protected override Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			if (DataGridView == null || string.IsNullOrEmpty (ErrorText))
				return Rectangle.Empty;

			Size error_icon = new Size (12, 11);
			return new Rectangle (new Point (Size.Width - error_icon.Width - 23, (Size.Height - error_icon.Height) / 2), error_icon);
		}

		protected override object GetFormattedValue (object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
		{
			return base.GetFormattedValue (value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
		}

		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
		{
			object o = FormattedValue;

			if (o != null) {
				Size s = DataGridViewCell.MeasureTextSize (graphics, o.ToString (), cellStyle.Font, TextFormatFlags.Default);
				s.Height = Math.Max (s.Height, 22);
				s.Width += 25;
				return s;
			} else
				return new Size (39, 22);
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

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			// The internal paint routines are overridden instead of
			// doing the custom paint logic here
			base.Paint (graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		}

		internal override void PaintPartContent (Graphics graphics, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, DataGridViewCellStyle cellStyle, object formattedValue)
		{
			Color color = Selected ? cellStyle.SelectionForeColor : cellStyle.ForeColor;
			TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.TextBoxControl;
	
			Rectangle text_area = ContentBounds;
			text_area.X += cellBounds.X;
			text_area.Y += cellBounds.Y;

			Rectangle button_area = CalculateButtonArea (cellBounds);

			// The background of the dropdown button should be gray, not
			// the background color of the cell.
			graphics.FillRectangle (SystemBrushes.Control, button_area);
			ThemeEngine.Current.CPDrawComboButton (graphics, button_area, ButtonState.Normal);

			if (formattedValue != null)
				TextRenderer.DrawText (graphics, formattedValue.ToString (), cellStyle.Font, text_area, color, flags);
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

		// IMPORTANT: Only call the internal methods from within DataGridViewComboBoxCell
		// for adding/removing/clearing because the other methods invoke an update of the 
		// column items collection and you might end up in an endless loop.
		//
		[ListBindable (false)]
		public class ObjectCollection : IList, ICollection, IEnumerable {

			private ArrayList list;
			private DataGridViewComboBoxCell owner;

			public ObjectCollection (DataGridViewComboBoxCell owner)
			{
				this.owner = owner;
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
				set {
					ThrowIfOwnerIsDataBound ();
					list[index] = value;
				}
			}

			public int Add (object item)
			{
				ThrowIfOwnerIsDataBound ();
				int index = AddInternal (item);
				SyncOwnerItems ();
				return index;
			}
			
			internal int AddInternal (object item)
			{
				return list.Add (item);
			}

			internal void AddRangeInternal (ICollection items)
			{
				list.AddRange (items);
			}

			public void AddRange (ObjectCollection value)
			{
				ThrowIfOwnerIsDataBound ();
				AddRangeInternal (value);
				SyncOwnerItems ();
			}

			private void SyncOwnerItems ()
			{
				ThrowIfOwnerIsDataBound ();
				if (owner != null)
					owner.SyncItems ();
			}

			public void ThrowIfOwnerIsDataBound ()
			{
				if (owner != null && owner.DataGridView != null && owner.DataSource != null)
					throw new ArgumentException ("Cannot modify collection if the cell is data bound.");
			}

			public void AddRange (params object[] items)
			{
				ThrowIfOwnerIsDataBound ();
				AddRangeInternal (items);
				SyncOwnerItems ();
			}

			public void Clear ()
			{
				ThrowIfOwnerIsDataBound ();
				ClearInternal ();
				SyncOwnerItems ();
			}

			internal void ClearInternal ()
			{
				list.Clear ();
			}

			public bool Contains (object value)
			{
				return list.Contains(value);
			}

			void ICollection.CopyTo (Array destination, int index)
			{
				CopyTo ((object[]) destination, index);
			}

			public void CopyTo (object[] destination, int arrayIndex)
			{
				list.CopyTo (destination, arrayIndex);
			}

			public IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator();
			}

			public int IndexOf (object value)
			{
				return list.IndexOf(value);
			}

			public void Insert (int index, object item)
			{
				ThrowIfOwnerIsDataBound ();
				InsertInternal (index, item);
				SyncOwnerItems ();
			}

			internal void InsertInternal (int index, object item)
			{
				list.Insert (index, item);
			}

			public void Remove (object value)
			{
				ThrowIfOwnerIsDataBound ();
				RemoveInternal (value);
				SyncOwnerItems ();
			}

			internal void RemoveInternal (object value)
			{
				list.Remove (value);
			}

			public void RemoveAt (int index)
			{
				ThrowIfOwnerIsDataBound ();
				RemoveAtInternal (index);
				SyncOwnerItems ();
			}

			internal void RemoveAtInternal (int index)
			{
				list.RemoveAt (index);
			}

			int IList.Add (object item)
			{
				return Add (item);
			}

		}

	}

}

