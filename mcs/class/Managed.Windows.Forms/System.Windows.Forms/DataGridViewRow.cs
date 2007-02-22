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

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// XXX [TypeConverter (typeof (DataGridRowConverter))]
	public class DataGridViewRow : DataGridViewBand {

		private AccessibleObject accessibilityObject;
		private DataGridViewCellCollection cells;
		private ContextMenuStrip contextMenuStrip;
		private object dataBoundItem;
		private int dividerHeight;
		private string errorText;
		private DataGridViewRowHeaderCell headerCell;
		private int height;
		private int minimumHeight;

		public DataGridViewRow ()
		{
			cells = new DataGridViewCellCollection(this);
			minimumHeight = 3;
			height = -1;
			headerCell = new DataGridViewRowHeaderCell();
		}

		[Browsable (false)]
		public AccessibleObject AccessibilityObject {
			get { return accessibilityObject; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataGridViewCellCollection Cells {
			get { return cells; }
		}

		[DefaultValue (null)]
		public override ContextMenuStrip ContextMenuStrip {
			get { return contextMenuStrip; }
			set {
				if (contextMenuStrip != value) {
					contextMenuStrip = value;
					if (DataGridView != null) {
						DataGridView.OnRowContextMenuStripChanged(new DataGridViewRowEventArgs(this));
					}
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public object DataBoundItem {
			get { return dataBoundItem; }
		}

		[Browsable (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[NotifyParentProperty (true)]
		public override DataGridViewCellStyle DefaultCellStyle {
			get { return base.DefaultCellStyle; }
			set {
				if (DefaultCellStyle != value) {
					base.DefaultCellStyle = value;
					if (DataGridView != null) {
						DataGridView.OnRowDefaultCellStyleChanged(new DataGridViewRowEventArgs(this));
					}
				}
			}
		}

		[Browsable (false)]
		public override bool Displayed {
			get { return base.Displayed; }
		}

		[DefaultValue (0)]
		[NotifyParentProperty (true)]
		public int DividerHeight {
			get { return dividerHeight; }
			set { dividerHeight = value; }
		}

		[DefaultValue ("")]
		[NotifyParentProperty (true)]
		public string ErrorText {
			get { return errorText; }
			set {
				if (errorText != value) {
					errorText = value;
					if (DataGridView != null) {
						DataGridView.OnRowErrorTextChanged(new DataGridViewRowEventArgs(this));
					}
				}
			}
		}

		[Browsable (false)]
		public override bool Frozen {
			get { return base.Frozen; }
			set { base.Frozen = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataGridViewRowHeaderCell HeaderCell {
			get { return headerCell; }
			set {
				if (headerCell != value) {
					headerCell = value;
					if (DataGridView != null) {
						DataGridView.OnRowHeaderCellChanged(new DataGridViewRowEventArgs(this));
					}
				}
			}
		}

		[DefaultValue (22)]
		[NotifyParentProperty (true)]
		public int Height {
			get {
				if (height < 0) {
					if (DefaultCellStyle != null && DefaultCellStyle.Font != null) {
						return DefaultCellStyle.Font.Height + 9;
					}
					if (InheritedStyle != null && InheritedStyle.Font != null) {
						return InheritedStyle.Font.Height + 9;
					}
					return System.Windows.Forms.Control.DefaultFont.Height + 9;
				}
				return height;
			}
			set {
				if (height != value) {
					if (value < minimumHeight) {
						throw new ArgumentOutOfRangeException("Height can't be less than MinimumHeight.");
					}
					height = value;
					if (DataGridView != null) {
						DataGridView.OnRowHeightChanged(new DataGridViewRowEventArgs(this));
					}
				}
			}
		}

		public override DataGridViewCellStyle InheritedStyle {
			get {
				if (DataGridView == null) {
					return DefaultCellStyle;
				}
				else {
					if (DefaultCellStyle == null) {
						return DataGridView.DefaultCellStyle;
					}
					else {
						DataGridViewCellStyle style = (DataGridViewCellStyle) DefaultCellStyle.Clone();
						/////// Combination with dataGridView.DefaultCellStyle
						return style;
					}
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool IsNewRow {
			get {
				if (DataGridView != null && DataGridView.Rows[DataGridView.Rows.Count - 1] == this) {
					return true;
				}
				return false;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int MinimumHeight {
			get { return minimumHeight; }
			set {
				if (minimumHeight != value) {
					if (value < 2 || value > Int32.MaxValue) {
						throw new ArgumentOutOfRangeException("MinimumHeight should be between 2 and Int32.MaxValue.");
					}
					minimumHeight = value;
					if (DataGridView != null) {
						DataGridView.OnRowMinimumHeightChanged(new DataGridViewRowEventArgs(this));
					}
				}
			}
		}

		[Browsable (true)]
		[DefaultValue (false)]
		[NotifyParentProperty (true)]
		public override bool ReadOnly {
			get { return base.ReadOnly; }
			set { base.ReadOnly = value; }
		}

		[NotifyParentProperty (true)]
		public override DataGridViewTriState Resizable {
			get { return base.Resizable; }
			set { base.Resizable = value; }
		}

		public override bool Selected {
			get {
				if (Index == -1) {
					throw new InvalidOperationException("The row is a shared row.");
				}
				if (DataGridView == null) {
					throw new InvalidOperationException("The row has not been added to a DataGridView control.");
				}
				return base.Selected;
			}
			set {
				if (Index == -1) {
					throw new InvalidOperationException("The row is a shared row.");
				}
				if (DataGridView == null) {
					throw new InvalidOperationException("The row has not been added to a DataGridView control.");
				}
				base.Selected = value;
				foreach (DataGridViewCell cell in cells) {
					cell.Selected = value;
				}
			}
		}

		public override DataGridViewElementStates State {
			get { return base.State; }
		}

		[Browsable (false)]
		public override bool Visible {
			get { return base.Visible; }
			set {
				if (IsNewRow && value == false) {
					throw new InvalidOperationException("Cant make invisible a new row.");
				}
				base.Visible = value;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual DataGridViewAdvancedBorderStyle AdjustRowHeaderBorderStyle (DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStyleInput, DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedRow, bool isLastVisibleRow)
		{
			throw new NotImplementedException();
		}

		public override object Clone ()
		{
			DataGridViewRow row = (DataGridViewRow) MemberwiseClone();
			row.cells = new DataGridViewCellCollection(row);
			foreach (DataGridViewCell cell in cells) {
				row.cells.Add(cell.Clone() as DataGridViewCell);
			}
			return row;
		}

		public void CreateCells (DataGridView dataGridView)
		{
			if (dataGridView == null) {
				throw new ArgumentNullException("DataGridView is null.");
			}
			if (dataGridView.Rows.Contains(this)) {
				throw new InvalidOperationException("The row already exists in the DataGridView.");
			}
			DataGridViewCellCollection newCellCollection = new DataGridViewCellCollection(this);
			foreach (DataGridViewColumn column in dataGridView.Columns) {
				if (column.CellTemplate == null) {
					throw new InvalidOperationException("Cell template not set in column: " + column.Index.ToString() + ".");
				}
				newCellCollection.Add((DataGridViewCell) column.CellTemplate.Clone());
			}
			cells = newCellCollection;
		}

		public void CreateCells (DataGridView dataGridView, params object[] values)
		{
			if (values == null) {
				throw new ArgumentNullException("values is null");
			}
			CreateCells(dataGridView);
			for (int i = 0; i < values.Length; i++) {
				cells[i].Value = values[i];
			}
		}

		public ContextMenuStrip GetContextMenuStrip (int rowIndex)
		{
			if (rowIndex == -1) {
				throw new InvalidOperationException("rowIndex is -1");
			}
			if (rowIndex < 0 || rowIndex >= DataGridView.Rows.Count) {
				throw new ArgumentOutOfRangeException("rowIndex is out of range");
			}

			return null; // XXX
		}

		public string GetErrorText (int rowIndex)
		{
			return "";
		}

		public virtual int GetPreferredHeight (int rowIndex, DataGridViewAutoSizeRowMode autoSizeRowMode, bool fixedWidth)
		{
			throw new NotImplementedException();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual DataGridViewElementStates GetState (int rowIndex)
		{
			throw new NotImplementedException();
		}

		public bool SetValues (params object[] values)
		{
			if (values == null) {
				throw new ArgumentNullException("vues is null");
			}
			if (DataGridView != null && DataGridView.VirtualMode) {
				throw new InvalidOperationException("DataGridView is operating in virtual mode");
			}
			/////// COLUMNAS //////////
			for (int i = 0; i < values.Length; i++) {
				DataGridViewCell cell = new DataGridViewTextBoxCell();
				cell.Value = values[i];
				cells.Add(cell);
			}
			
			// XXX
			return true;
		}

		public override string ToString ()
		{
			return this.GetType().Name + ", Band Index: " + base.Index.ToString();
		}

		protected virtual AccessibleObject CreateAccessibilityInstance ()
		{
			return new DataGridViewRowAccessibleObject(this);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual DataGridViewCellCollection CreateCellsInstance ()
		{
			cells = new DataGridViewCellCollection(this);
			return cells;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal virtual void DrawFocus (Graphics graphics, Rectangle clipBounds, Rectangle bounds, int rowIndex, DataGridViewElementStates rowState, DataGridViewCellStyle cellStyle, bool cellsPaintSelectionBackground)
		{
		}

		protected internal virtual void Paint (Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, bool isFirstDisplayedRow, bool isLastVisibleRow)
		{
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal virtual void PaintCells (Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, bool isFirstDisplayedRow, bool isLastVisibleRow, DataGridViewPaintParts paintParts)
		{
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal virtual void PaintHeader (Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, bool isFirstDisplayedRow, bool isLastVisibleRow, DataGridViewPaintParts paintParts)
		{
		}

		internal override void SetDataGridView (DataGridView dataGridView)
		{
			base.SetDataGridView(dataGridView);
			headerCell.SetDataGridView(dataGridView);
		}

		internal override void SetState (DataGridViewElementStates state)
		{
			if (State != state) {
				base.SetState(state);
				if (DataGridView != null) {
					DataGridView.OnRowStateChanged(this.Index, new DataGridViewRowStateChangedEventArgs(this, state));
				}
			}
		}

		[ComVisibleAttribute(true)]
		protected class DataGridViewRowAccessibleObject : AccessibleObject {

			private DataGridViewRow dataGridViewRow;

			public DataGridViewRowAccessibleObject ()
			{
			}

			public DataGridViewRowAccessibleObject (DataGridViewRow row)
			{
				this.dataGridViewRow = row;
			}

			public override Rectangle Bounds {
				get { throw new NotImplementedException(); }
			}

			public override string DefaultAction {
				get { return "Edit"; }
			}

			public override string Name {
				get { return "Index: " + dataGridViewRow.Index.ToString(); }
			}

			public DataGridViewRow Owner {
				get { return dataGridViewRow; }
				set { dataGridViewRow = value; }
			}

			public override AccessibleObject Parent {
				get { return dataGridViewRow.AccessibilityObject; }
			}

			public override AccessibleRole Role {
				get { return AccessibleRole.Row; }
			}

			public override AccessibleStates State {
				get {
					if (dataGridViewRow.Selected) {
						return AccessibleStates.Selected;
					}
					else {
						return AccessibleStates.Focused;
					}
				}
			}

			public override string Value {
				get {
					if (dataGridViewRow.Cells.Count == 0) {
						return "(Create New)";
					}
					string result = "";
					foreach (DataGridViewCell cell in dataGridViewRow.Cells) {
						result += cell.AccessibilityObject.Value;
					}
					return result;
				}
			}

			public override AccessibleObject GetChild (int index) {
				throw new NotImplementedException();
			}

			public override int GetChildCount () {
				throw new NotImplementedException();
			}

			public override AccessibleObject GetFocused () {
				return null;
			}

			public override AccessibleObject GetSelected () {
				return null;
			}

			public override AccessibleObject Navigate (AccessibleNavigation navigationDirection) {
				switch (navigationDirection) {
					case AccessibleNavigation.Right:
						break;
					case AccessibleNavigation.Left:
						break;
					case AccessibleNavigation.Next:
						break;
					case AccessibleNavigation.Previous:
						break;
					case AccessibleNavigation.Up:
						break;
					case AccessibleNavigation.Down:
						break;
					default:
						return null;
				}
				return null;
			}

			public override void Select (AccessibleSelection flags) {
				switch (flags) {
					case AccessibleSelection.TakeFocus:
						dataGridViewRow.DataGridView.Focus();
						break;
					case AccessibleSelection.TakeSelection:
						//dataGridViewRow.Focus();
						break;
					case AccessibleSelection.AddSelection:
						dataGridViewRow.DataGridView.SelectedRows.InternalAdd(dataGridViewRow);
						break;
					case AccessibleSelection.RemoveSelection:
						dataGridViewRow.DataGridView.SelectedRows.InternalRemove(dataGridViewRow);
						break;
				}
			}

		}



	}

}

#endif
