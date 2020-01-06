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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Pedro Martínez Juliá <pedromj@gmail.com>
//


using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Forms
{
	[TypeConverter (typeof (DataGridViewRowConverter))]
	public class DataGridViewRow : DataGridViewBand
	{
		private AccessibleObject accessibilityObject;
		private DataGridViewCellCollection cells;
		private ContextMenuStrip contextMenuStrip;
		private int dividerHeight;
		private string errorText;
		private DataGridViewRowHeaderCell headerCell;
		private int height;
		private int minimumHeight;
		private int explicit_height;

		public DataGridViewRow ()
		{
			minimumHeight = 3;
			height = -1;
			explicit_height = -1;
			headerCell = new DataGridViewRowHeaderCell();
			headerCell.SetOwningRow (this);
			accessibilityObject = new AccessibleObject ();
			SetState (DataGridViewElementStates.Visible);
		}

		[Browsable (false)]
		public AccessibleObject AccessibilityObject {
			get { return accessibilityObject; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataGridViewCellCollection Cells {
			get {
				if (cells == null)
					cells = CreateCellsInstance ();
				return cells;
			}
		}

		[DefaultValue (null)]
		public override ContextMenuStrip ContextMenuStrip {
			get {
				if (IsShared)
					throw new InvalidOperationException ("Operation cannot be performed on a shared row.");
					
				return contextMenuStrip;
			}
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
			get {
				if (base.DataGridView != null && DataGridView.DataManager != null) {
					if (DataGridView.DataManager.Count > base.Index)
						return DataGridView.DataManager[base.Index];
				}
				return null;
			}
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
			get {
				if (IsShared)
					throw new InvalidOperationException ("Getting the Displayed property of a shared row is not a valid operation.");
					
				return base.Displayed;
			}
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
			get {
				if (IsShared)
					throw new InvalidOperationException ("Operation cannot be performed on a shared row.");
					
				return errorText == null ? string.Empty : errorText;
			}
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
			get {
				if (IsShared)
					throw new InvalidOperationException ("Getting the Frozen property of a shared row is not a valid operation.");
					
				return base.Frozen;
			}
			set { base.Frozen = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataGridViewRowHeaderCell HeaderCell {
			get { return headerCell; }
			set {
				if (headerCell != value) {
					headerCell = value;
					headerCell.SetOwningRow (this);
					
					if (DataGridView != null) {
						headerCell.SetDataGridView (DataGridView);
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
					if (Index >= 0 && InheritedStyle != null && InheritedStyle.Font != null) {
						return InheritedStyle.Font.Height + 9;
					}
					return System.Windows.Forms.Control.DefaultFont.Height + 9;
				}
				return height;
			}
			set {
				explicit_height = value;
				
				if (height != value) {
					if (value < minimumHeight) {
						height = minimumHeight;
					} else {
						height = value;
					}
					if (DataGridView != null) {
						DataGridView.Invalidate ();
						DataGridView.OnRowHeightChanged(new DataGridViewRowEventArgs(this));
					}
				}
			}
		}

		public override DataGridViewCellStyle InheritedStyle {
			get {
				if (Index == -1)
					throw new InvalidOperationException ("Getting the InheritedStyle property of a shared row is not a valid operation.");
					
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
				if (DataGridView != null && DataGridView.Rows[DataGridView.Rows.Count - 1] == this && DataGridView.NewRowIndex == Index) {
					return true;
				}
				return false;
			}
		}

		internal bool IsShared {
			get {
				return Index == -1 && DataGridView != null;
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
					if (height < value) {
						// don't let height get less than minimumHeight!
						Height = value;
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
			get {
				if (IsShared)
					throw new InvalidOperationException ("Getting the ReadOnly property of a shared row is not a valid operation.");
					
				if (DataGridView != null && DataGridView.ReadOnly)
					return true;
					
				return base.ReadOnly;
			}
			set { base.ReadOnly = value; }
		}

		[NotifyParentProperty (true)]
		public override DataGridViewTriState Resizable {
			get {
				if (IsShared)
					throw new InvalidOperationException ("Getting the Resizable property of a shared row is not a valid operation.");
					
				return base.Resizable;
			}
			set { base.Resizable = value; }
		}

		public override bool Selected {
			get {
				if (IsShared)
					throw new InvalidOperationException ("Getting the Selected property of a shared row is not a valid operation.");
					
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
			}
		}

		public override DataGridViewElementStates State {
			get {
				if (IsShared)
					throw new InvalidOperationException ("Getting the State property of a shared row is not a valid operation.");
					
				return base.State;
			}
		}

		[Browsable (false)]
		public override bool Visible {
			get {
				if (IsShared)
					throw new InvalidOperationException ("Getting the Visible property of a shared row is not a valid operation.");
					
				return base.Visible;
			}
			set {
				if (IsNewRow && value == false) {
					throw new InvalidOperationException("Cant make invisible a new row.");
				}
				if (!value && DataGridView != null && DataGridView.DataManager != null && 
				    DataGridView.DataManager.Position == Index)
					throw new InvalidOperationException("Row associated with the currency manager's position cannot be made invisible.");

				base.Visible = value;
				if (DataGridView != null)
					DataGridView.Invalidate ();
			}
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual DataGridViewAdvancedBorderStyle AdjustRowHeaderBorderStyle (DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStyleInput, DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedRow, bool isLastVisibleRow)
		{
			throw new NotImplementedException();
		}

		public override object Clone ()
		{
			DataGridViewRow row = (DataGridViewRow)MemberwiseClone ();

			row.DefaultCellStyle = (DataGridViewCellStyle)DefaultCellStyle.Clone ();
			row.HeaderCell = (DataGridViewRowHeaderCell)HeaderCell.Clone ();
			row.SetIndex (-1);
			
			row.cells = null;
			
			foreach (DataGridViewCell cell in Cells)
				row.Cells.Add (cell.Clone () as DataGridViewCell);

			row.SetDataGridView (null);

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
			Cells.Clear ();
			foreach (DataGridViewColumn column in dataGridView.Columns) {
				if (column.CellTemplate == null) {
					throw new InvalidOperationException("Cell template not set in column: " + column.Index.ToString() + ".");
				}
				Cells.Add((DataGridViewCell) column.CellTemplate.Clone());
			}
		}

		public void CreateCells (DataGridView dataGridView, params object[] values)
		{
			if (values == null) {
				throw new ArgumentNullException("values is null");
			}
			CreateCells(dataGridView);
			for (int i = 0; i < values.Length; i++) {
				Cells[i].Value = values[i];
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
			return string.Empty;
		}

		public virtual int GetPreferredHeight (int rowIndex, DataGridViewAutoSizeRowMode autoSizeRowMode, bool fixedWidth)
		{
			DataGridViewRow row;
			
			if (DataGridView != null)
				row = DataGridView.Rows.SharedRow (rowIndex);
			else
				row = this;

			int height = 0;

			if (autoSizeRowMode == DataGridViewAutoSizeRowMode.AllCells || autoSizeRowMode == DataGridViewAutoSizeRowMode.RowHeader)
				height = Math.Max (height, row.HeaderCell.PreferredSize.Height);

			if (autoSizeRowMode == DataGridViewAutoSizeRowMode.AllCells || autoSizeRowMode == DataGridViewAutoSizeRowMode.AllCellsExceptHeader)
				foreach (DataGridViewCell cell in row.Cells)
					height = Math.Max (height, cell.PreferredSize.Height);
			
			return height;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual DataGridViewElementStates GetState (int rowIndex)
		{
			DataGridViewElementStates state = DataGridViewElementStates.None;
			
			if (rowIndex == -1) {
				state |= DataGridViewElementStates.Displayed;
				
				if (DataGridView.ReadOnly)
					state |= DataGridViewElementStates.ReadOnly;
				if (DataGridView.AllowUserToResizeRows)
					state |= DataGridViewElementStates.Resizable;
				if (DataGridView.Visible)
					state |= DataGridViewElementStates.Visible;
			
				return state;
			}
			
			DataGridViewRow row = DataGridView.Rows[rowIndex];

			if (row.Displayed)
				state |= DataGridViewElementStates.Displayed;
			if (row.Frozen)
				state |= DataGridViewElementStates.Frozen;
			if (row.ReadOnly)
				state |= DataGridViewElementStates.ReadOnly;
			if (row.Resizable == DataGridViewTriState.True || (row.Resizable == DataGridViewTriState.NotSet && DataGridView.AllowUserToResizeRows))
				state |= DataGridViewElementStates.Resizable;
			if (row.Resizable == DataGridViewTriState.True)
				state |= DataGridViewElementStates.ResizableSet;
			if (row.Selected)
				state |= DataGridViewElementStates.Selected;
			if (row.Visible)
				state |= DataGridViewElementStates.Visible;
				
			return state;
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
				DataGridViewCell cell;
				if (Cells.Count > i) {
					cell = Cells [i];
				} else {
					cell = new DataGridViewTextBoxCell ();
					Cells.Add (cell);
				}
				cell.Value = values[i];
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
			return new DataGridViewCellCollection(this);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal virtual void DrawFocus (Graphics graphics, Rectangle clipBounds, Rectangle bounds, int rowIndex, DataGridViewElementStates rowState, DataGridViewCellStyle cellStyle, bool cellsPaintSelectionBackground)
		{
		}

		protected internal virtual void Paint (Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, bool isFirstDisplayedRow, bool isLastVisibleRow)
		{
			DataGridViewCellStyle style;
			
			if (Index == -1)
				style = DataGridView.RowsDefaultCellStyle;
			else
				style = InheritedStyle;
				
			DataGridViewRowPrePaintEventArgs pre = new DataGridViewRowPrePaintEventArgs (DataGridView, graphics, clipBounds, rowBounds, rowIndex, rowState, string.Empty, style, isFirstDisplayedRow, isLastVisibleRow);
			pre.PaintParts = DataGridViewPaintParts.All;

			DataGridView.OnRowPrePaint (pre);

			// The user has elected for us to not do anything
			if (pre.Handled)
				return;

			if (DataGridView.RowHeadersVisible)
				PaintHeader (graphics, pre.ClipBounds, rowBounds, rowIndex, rowState, isFirstDisplayedRow, isLastVisibleRow, pre.PaintParts);
			
			PaintCells (graphics, pre.ClipBounds, rowBounds, rowIndex, rowState, isFirstDisplayedRow, isLastVisibleRow, pre.PaintParts);

			DataGridViewRowPostPaintEventArgs post = new DataGridViewRowPostPaintEventArgs (DataGridView, graphics, pre.ClipBounds, rowBounds, rowIndex, rowState, pre.ErrorText, style, isFirstDisplayedRow, isLastVisibleRow);
			DataGridView.OnRowPostPaint (post);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal virtual void PaintCells (Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, bool isFirstDisplayedRow, bool isLastVisibleRow, DataGridViewPaintParts paintParts)
		{
			List<DataGridViewColumn> sortedColumns = DataGridView.Columns.ColumnDisplayIndexSortedArrayList;
			
			Rectangle bounds = rowBounds;
			
			// If row headers are visible, adjust our starting point
			if (DataGridView.RowHeadersVisible) {
				bounds.X += DataGridView.RowHeadersWidth;
				bounds.Width -= DataGridView.RowHeadersWidth;
			}

			bool singleVerticalBorderAdded = !DataGridView.RowHeadersVisible;
			bool singleHorizontalBorderAdded = !DataGridView.ColumnHeadersVisible;
			
			for (int i = DataGridView.first_col_index; i < sortedColumns.Count; i++) {
				DataGridViewColumn col = sortedColumns[i];
				
				if (!col.Visible)
					continue;
					
				if (!col.Displayed)
					break;
					
				bounds.Width = col.Width;
				DataGridViewCell cell = Cells[col.Index];
				
				if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
					graphics.FillRectangle (Brushes.White, bounds);
				
				DataGridViewCellStyle style;

				if (cell.RowIndex == -1)
					style = DefaultCellStyle;
				else
					style = cell.InheritedStyle;

				object value;
				DataGridViewElementStates cellState;
				
				if (cell.RowIndex == -1) {
					// TODO: Look up value if databound.
					value = null;
					cellState = cell.State;
				} else {
					value = cell.Value;
					cellState = cell.InheritedState;
				}

				DataGridViewAdvancedBorderStyle intermediateBorderStyle = (DataGridViewAdvancedBorderStyle)((ICloneable)DataGridView.AdvancedCellBorderStyle).Clone ();
				DataGridViewAdvancedBorderStyle borderStyle = cell.AdjustCellBorderStyle (DataGridView.AdvancedCellBorderStyle, intermediateBorderStyle, singleVerticalBorderAdded, singleHorizontalBorderAdded, cell.ColumnIndex == 0, cell.RowIndex == 0);
				DataGridView.OnCellFormattingInternal (new DataGridViewCellFormattingEventArgs (cell.ColumnIndex, cell.RowIndex, value, cell.FormattedValueType, style));


				cell.PaintWork (graphics, clipBounds, bounds, rowIndex, cellState, style, borderStyle, paintParts);
				bounds.X += bounds.Width;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected internal virtual void PaintHeader (Graphics graphics, Rectangle clipBounds, Rectangle rowBounds, int rowIndex, DataGridViewElementStates rowState, bool isFirstDisplayedRow, bool isLastVisibleRow, DataGridViewPaintParts paintParts)
		{
			rowBounds.Width = DataGridView.RowHeadersWidth;
			graphics.FillRectangle (Brushes.White, rowBounds);
	
			HeaderCell.PaintWork (graphics, clipBounds, rowBounds, rowIndex, rowState, HeaderCell.InheritedStyle, DataGridView.AdvancedRowHeadersBorderStyle, paintParts);
		}

		internal override void SetDataGridView (DataGridView dataGridView)
		{
			base.SetDataGridView(dataGridView);
			headerCell.SetDataGridView(dataGridView);
			foreach (DataGridViewCell cell in Cells)
				cell.SetDataGridView (dataGridView);
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
		
		// Set the row's height without overwriting the explicit_height, so we
		// can go back to the user's requested height when they turn off AutoSize
		internal void SetAutoSizeHeight (int height)
		{
			this.height = height;
			
			if (DataGridView != null) {
				DataGridView.Invalidate ();
				DataGridView.OnRowHeightChanged (new DataGridViewRowEventArgs (this));
			}
		}

		// If the user sets AutoSizeRowMode to None, reset every row to its explicit height
		internal void ResetToExplicitHeight ()
		{
			this.height = explicit_height;

			if (DataGridView != null)
				DataGridView.OnRowHeightChanged (new DataGridViewRowEventArgs (this));
		}
		
		[ComVisibleAttribute(true)]
		protected class DataGridViewRowAccessibleObject : AccessibleObject {

			private DataGridViewRow dataGridViewRow;

			public DataGridViewRowAccessibleObject ()
			{
			}

			public DataGridViewRowAccessibleObject (DataGridViewRow owner)
			{
				this.dataGridViewRow = owner;
			}

			public override Rectangle Bounds {
				get { throw new NotImplementedException(); }
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
	
	internal class DataGridViewRowConverter : TypeConverter
	{
	}
}

