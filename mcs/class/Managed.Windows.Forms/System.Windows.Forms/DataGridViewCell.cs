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

using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// XXX [TypeConverter (typeof (DataGridViewCellConverter))]
	public abstract class DataGridViewCell : DataGridViewElement, ICloneable, IDisposable
	{
		private DataGridView dataGridViewOwner;

		private AccessibleObject accessibilityObject;
		private int columnIndex;
		private Rectangle contentBounds;
		private ContextMenuStrip contextMenuStrip;
		private object defaultNewRowValue;
		private bool displayed;
		private object editedFormattedValue;
		private Rectangle errorIconBounds;
		private string errorText;
		private Type formattedValueType;
		private bool frozen;
		private DataGridViewElementStates inheritedState;
		private bool isInEditMode;
		private DataGridViewColumn owningColumn;
		private DataGridViewRow owningRow;
		private Size preferredSize;
		private bool readOnly;
		private bool resizable;
		private bool selected;
		private Size size;
		private DataGridViewCellStyle style;
		private object tag;
		private string toolTipText;
		private object valuex;
		private Type valueType;
		private bool visible;

		protected DataGridViewCell ()
		{
			columnIndex = -1;
		}

		~DataGridViewCell ()
		{
			Dispose(false);
		}

		[Browsable (false)]
		public AccessibleObject AccessibilityObject {
			get {
				if (accessibilityObject == null) {
					accessibilityObject = CreateAccessibilityInstance();
				}
				return accessibilityObject;
			}
		}

		public int ColumnIndex {
			get { 
				if (DataGridView == null)
					return -1;
				return columnIndex; 
			}
		}

		[Browsable (false)]
		public Rectangle ContentBounds {
			get { 
				return GetContentBounds (RowIndex);
			}
		}

		[DefaultValue (null)]
		public virtual ContextMenuStrip ContextMenuStrip {
			get { return contextMenuStrip; }
			set { contextMenuStrip = value; }
		}

		[Browsable (false)]
		public virtual object DefaultNewRowValue {
			get { return defaultNewRowValue; }
		}

		[Browsable (false)]
		public virtual bool Displayed {
			get { return displayed; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public object EditedFormattedValue {
			get { 
				return GetEditedFormattedValue (RowIndex, DataGridViewDataErrorContexts.Formatting);
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual Type EditType {
			get {
				return typeof (DataGridViewTextBoxEditingControl);
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public Rectangle ErrorIconBounds {
			get {
				DataGridViewCellStyle style = InheritedStyle;
				return errorIconBounds; 
			}
		}

		[Browsable (false)]
		public string ErrorText {
			get { 
				if (errorText == null)
					return string.Empty;
					
				return errorText; 
			}
			set {
				if (errorText != value) {
					errorText = value;
					OnErrorTextChanged(new DataGridViewCellEventArgs(ColumnIndex, RowIndex));
				}
			}
		}

		[Browsable (false)]
		public object FormattedValue {
			get {
				if (DataGridView == null)
					return null;
					
				DataGridViewCellStyle style = InheritedStyle;
				if (style.Format != String.Empty && FormattedValueType == typeof(string)) {
					return String.Format("{0:" + style.Format + "}", Value);
				}
				return Convert.ChangeType(Value, FormattedValueType, style.FormatProvider);
			}
		}

		[Browsable (false)]
		public virtual Type FormattedValueType {
			get { return formattedValueType; }
		}

		[Browsable (false)]
		public virtual bool Frozen {
			get { return frozen; }
		}

		[Browsable (false)]
		public bool HasStyle {
			get { return style != null; }
		}

		[Browsable (false)]
		public DataGridViewElementStates InheritedState {
			get { 
				return GetInheritedState (RowIndex);
			}
		}

		[Browsable (false)]
		public DataGridViewCellStyle InheritedStyle {
			get {
				return GetInheritedStyle (null, RowIndex, true);
			}
		}

		[Browsable (false)]
		public bool IsInEditMode {
			get {
				if (DataGridView == null)
					return false;
					
				if (RowIndex == -1)
					throw new InvalidOperationException ("Operation cannot be performed on a cell of a shared row.");
					
				return isInEditMode;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DataGridViewColumn OwningColumn {
			get { return owningColumn; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DataGridViewRow OwningRow {
			get { return owningRow; }
		}

		[Browsable (false)]
		public Size PreferredSize {
			get { 
				if (DataGridView == null)
					return Size.Empty;
					
				return GetPreferredSize (Hwnd.bmp_g, InheritedStyle, RowIndex, Size.Empty); 
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual bool ReadOnly {
			get { return readOnly; }
			set { readOnly = value; }
		}

		[Browsable (false)]
		public virtual bool Resizable {
			get {
				return resizable; 
			}
		}

		[Browsable (false)]
		public int RowIndex {
			get {
				if (owningRow == null) {
					return -1;
				}
				return owningRow.Index;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual bool Selected {
			get {
				if (selected)
					return true;
				
				if (DataGridView != null) {
					if (RowIndex >= 0 && DataGridView.Rows [RowIndex].Selected)
						return true;
						
					if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].Selected)
						return true;
				}
				
				return false;
			}
			set {
				if (value != ((State & DataGridViewElementStates.Selected) != 0)) {
					SetState(State ^ DataGridViewElementStates.Selected);
				}
				selected = value;
			}
		}

		[Browsable (false)]
		public Size Size {
			get {
				if (DataGridView == null)
					return Size.Empty;
					
				if (RowIndex == -1)
					throw new InvalidOperationException ("Getting the Size property of a cell in a shared row is not a valid operation.");
					
				return GetSize (RowIndex);
			}
		}

		[Browsable (true)]
		public DataGridViewCellStyle Style {
			get {
				if (style == null) {
					style = new DataGridViewCellStyle();
					style.StyleChanged += OnStyleChanged;
				}
				return style;
			}
			set { style = value; }
		}

		[Bindable (true, BindingDirection.OneWay)]
		[DefaultValue (null)]
		[Localizable (false)]
		[TypeConverter ("System.ComponentModel.StringConverter, " + Consts.AssemblySystem)]
		public object Tag {
			get { return tag; }
			set { tag = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string ToolTipText {
			get { return toolTipText == null ? string.Empty : toolTipText; }
			set { toolTipText = value; }
		}

		[Browsable (false)]
		public object Value {
			get {
				return GetValue (RowIndex);
			}
			set {
				SetValue (RowIndex, value);
			}
		}

		[Browsable (false)]
		public virtual Type ValueType {
			get { return valueType; }
			set { valueType = value; }
		}

		[Browsable (false)]
		public virtual bool Visible {
			get {
				// This is independent from State...
				DataGridViewColumn col = OwningColumn;
				DataGridViewRow row = OwningRow;
				
				bool rowVisible = true, colVisible = true;
				
				if (row == null && col == null)
					return false;
				
				if (row != null) {
					rowVisible = !row.IsShared && row.Visible;
				}
				
				if (col != null) {
					colVisible = col.Index >= 0 && col.Visible;
				}
				
				return rowVisible && colVisible;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual DataGridViewAdvancedBorderStyle AdjustCellBorderStyle (DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStyleInput,	DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow) {
			return dataGridViewAdvancedBorderStyleInput;
		}

		public virtual object Clone () {
			DataGridViewCell result = (DataGridViewCell) Activator.CreateInstance (GetType ());
			result.accessibilityObject = this.accessibilityObject;
			result.columnIndex = this.columnIndex;
			result.contentBounds = this.contentBounds;
			//result.contextMenuStrip = this.contextMenuStrip;
			result.defaultNewRowValue = this.defaultNewRowValue;
			result.displayed = this.displayed;
			result.editedFormattedValue = this.editedFormattedValue;
			result.errorIconBounds = this.errorIconBounds;
			result.errorText = this.errorText;
			result.formattedValueType = this.formattedValueType;
			result.frozen = this.frozen;
			result.inheritedState = this.inheritedState;
			result.isInEditMode = this.isInEditMode;
			result.owningColumn = this.owningColumn;
			result.owningRow = this.owningRow;
			result.preferredSize = this.preferredSize;
			result.readOnly = this.readOnly;
			result.resizable = this.resizable;
			result.selected = this.selected;
			result.size = this.size;
			result.style = this.style;
			result.tag = this.tag;
			result.toolTipText = this.toolTipText;
			result.valuex = this.valuex;
			result.valueType = this.valueType;
			result.visible = this.visible;
			return result;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual void DetachEditingControl () {
		}

		//public sealed void Dispose () {
		public void Dispose () {
		}

		public Rectangle GetContentBounds (int rowIndex) {
			if (DataGridView == null)
				return Rectangle.Empty;
			
			return GetContentBounds (Hwnd.bmp_g, InheritedStyle, rowIndex);
		}

		public object GetEditedFormattedValue (int rowIndex, DataGridViewDataErrorContexts context) {
			if (DataGridView == null)
				return null;
				
			DataGridViewCellStyle style = InheritedStyle;
			
			return editedFormattedValue;
		}

		public virtual ContextMenuStrip GetInheritedContextMenuStrip (int rowIndex)
		{
			throw new NotImplementedException();
		}

		public virtual DataGridViewElementStates GetInheritedState (int rowIndex) {
		
			if (DataGridView == null && rowIndex != -1)
				throw new ArgumentException ("msg?");
			
			if (DataGridView != null && (rowIndex < 0 || rowIndex >= DataGridView.Rows.Count))
				throw new ArgumentOutOfRangeException ("rowIndex", "Specified argument was out of the range of valid values.");
		
			DataGridViewElementStates result;
			
			result = DataGridViewElementStates.ResizableSet | State;
			
			DataGridViewColumn col = OwningColumn;
			DataGridViewRow row = OwningRow;

			if (DataGridView == null) {
				if (row != null) {
					if (row.Resizable == DataGridViewTriState.True)
						result |= DataGridViewElementStates.Resizable;
						
					if (row.Visible)
						result |= DataGridViewElementStates.Visible;
						
					if (row.ReadOnly)
						result |= DataGridViewElementStates.ReadOnly;
						
					if (row.Frozen)
						result |= DataGridViewElementStates.Frozen;
						
					if (row.Displayed)
						result |= DataGridViewElementStates.Displayed;
						
					/*if (row.Selected)
						result |= DataGridViewElementStates.Selected;*/
				}
				
				return result;
			}
			
			if (col.Resizable == DataGridViewTriState.True && row.Resizable == DataGridViewTriState.True)
				result |= DataGridViewElementStates.Resizable;
			
			if (col.Visible && row.Visible)
				result |= DataGridViewElementStates.Visible;

			if (col.ReadOnly && row.ReadOnly)
				result |= DataGridViewElementStates.ReadOnly;

			if (col.Frozen && row.Frozen)
				result |= DataGridViewElementStates.Frozen;

			if (col.Displayed && row.Displayed)
				result |= DataGridViewElementStates.Displayed;

			if (col.Selected || row.Selected)
				result |= DataGridViewElementStates.Selected;
			
			return result;
		}

		public virtual DataGridViewCellStyle GetInheritedStyle (DataGridViewCellStyle inheritedCellStyle, int rowIndex, bool includeColors) {
			/*
			 * System.InvalidOperationException :: The cell has no associated System.Windows.Forms.DataGridView, or the cell's System.Windows.Forms.DataGridViewCell.ColumnIndex is less than 0.
			 * System.ArgumentOutOfRangeException :: rowIndex is less than 0, or greater than or equal to the number of rows in the parent System.Windows.Forms.DataGridView.
			 * */
	
			if (DataGridView == null)
				throw new InvalidOperationException ("Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.");

			if (rowIndex < 0 || rowIndex >= DataGridView.Rows.Count)
				throw new ArgumentOutOfRangeException ("rowIndex", "Specified argument was out of the range of valid values.");

			DataGridViewCellStyle result = new DataGridViewCellStyle ();
			if (style != null && style.Alignment != DataGridViewContentAlignment.NotSet) {
				result.Alignment = style.Alignment;
			} else if (OwningRow != null && OwningRow.DefaultCellStyle.Alignment != DataGridViewContentAlignment.NotSet) {
				result.Alignment = OwningRow.DefaultCellStyle.Alignment;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && DataGridView.AlternatingRowsDefaultCellStyle.Alignment != DataGridViewContentAlignment.NotSet) {
					result.Alignment = DataGridView.AlternatingRowsDefaultCellStyle.Alignment;
				} else if (DataGridView.RowsDefaultCellStyle.Alignment != DataGridViewContentAlignment.NotSet) {
					result.Alignment = DataGridView.RowsDefaultCellStyle.Alignment;
				} else if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].DefaultCellStyle.Alignment != DataGridViewContentAlignment.NotSet) {
					result.Alignment = DataGridView.Columns [ColumnIndex].DefaultCellStyle.Alignment;
				} else {
					result.Alignment = DataGridView.DefaultCellStyle.Alignment;
				}
			}
			if (style != null && style.BackColor != Color.Empty) {
				result.BackColor = style.BackColor;
			} else if (OwningRow != null && OwningRow.DefaultCellStyle.BackColor != Color.Empty) {
				result.BackColor = OwningRow.DefaultCellStyle.BackColor;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && DataGridView.AlternatingRowsDefaultCellStyle.BackColor != Color.Empty) {
					result.BackColor = DataGridView.AlternatingRowsDefaultCellStyle.BackColor;
				} else if (DataGridView.RowsDefaultCellStyle.BackColor != Color.Empty) {
					result.BackColor = DataGridView.RowsDefaultCellStyle.BackColor;
				} else if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].DefaultCellStyle.BackColor != Color.Empty) {
					result.BackColor = DataGridView.Columns [ColumnIndex].DefaultCellStyle.BackColor;
				} else {
					result.BackColor = DataGridView.DefaultCellStyle.BackColor;
				}
			}
			if (style != null && style.Font != null) {
				result.Font = style.Font;
			} else if (OwningRow != null && OwningRow.DefaultCellStyle.Font != null) {
				result.Font = OwningRow.DefaultCellStyle.Font;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && DataGridView.AlternatingRowsDefaultCellStyle.Font != null) {
					result.Font = DataGridView.AlternatingRowsDefaultCellStyle.Font;
				} else if (DataGridView.RowsDefaultCellStyle.Font != null) {
					result.Font = DataGridView.RowsDefaultCellStyle.Font;
				} else if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].DefaultCellStyle.Font != null) {
					result.Font = DataGridView.Columns [ColumnIndex].DefaultCellStyle.Font;
				} else {
					result.Font = DataGridView.DefaultCellStyle.Font;
				}
			}
			if (style != null && style.ForeColor != Color.Empty) {
				result.ForeColor = style.ForeColor;
			} else if (OwningRow != null && OwningRow.DefaultCellStyle.ForeColor != Color.Empty) {
				result.ForeColor = OwningRow.DefaultCellStyle.ForeColor;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && DataGridView.AlternatingRowsDefaultCellStyle.ForeColor != Color.Empty) {
					result.ForeColor = DataGridView.AlternatingRowsDefaultCellStyle.ForeColor;
				} else if (DataGridView.RowsDefaultCellStyle.ForeColor != Color.Empty) {
					result.ForeColor = DataGridView.RowsDefaultCellStyle.ForeColor;
				} else if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].DefaultCellStyle.ForeColor != Color.Empty) {
					result.ForeColor = DataGridView.Columns [ColumnIndex].DefaultCellStyle.ForeColor;
				} else {
					result.ForeColor = DataGridView.DefaultCellStyle.ForeColor;
				}
			}
			if (style != null && style.Format != String.Empty) {
				result.Format = style.Format;
			} else if (OwningRow != null && OwningRow.DefaultCellStyle.Format != String.Empty) {
				result.Format = OwningRow.DefaultCellStyle.Format;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && DataGridView.AlternatingRowsDefaultCellStyle.Format != String.Empty) {
					result.Format = DataGridView.AlternatingRowsDefaultCellStyle.Format;
				} else if (DataGridView.RowsDefaultCellStyle.Format != String.Empty) {
					result.Format = DataGridView.RowsDefaultCellStyle.Format;
				} else if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].DefaultCellStyle.Format != String.Empty) {
					result.Format = DataGridView.Columns [ColumnIndex].DefaultCellStyle.Format;
				} else {
					result.Format = DataGridView.DefaultCellStyle.Format;
				}
			}
			if (style != null && style.FormatProvider != System.Globalization.CultureInfo.CurrentUICulture) {
				result.FormatProvider = style.FormatProvider;
			} else if (OwningRow != null && OwningRow.DefaultCellStyle.FormatProvider != System.Globalization.CultureInfo.CurrentUICulture) {
				result.FormatProvider = OwningRow.DefaultCellStyle.FormatProvider;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && DataGridView.AlternatingRowsDefaultCellStyle.FormatProvider != System.Globalization.CultureInfo.CurrentUICulture) {
					result.FormatProvider = DataGridView.AlternatingRowsDefaultCellStyle.FormatProvider;
				} else if (DataGridView.RowsDefaultCellStyle.FormatProvider != System.Globalization.CultureInfo.CurrentUICulture) {
					result.FormatProvider = DataGridView.RowsDefaultCellStyle.FormatProvider;
				} else if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].DefaultCellStyle.FormatProvider != System.Globalization.CultureInfo.CurrentUICulture) {
					result.FormatProvider = DataGridView.Columns [ColumnIndex].DefaultCellStyle.FormatProvider;
				} else {
					result.FormatProvider = DataGridView.DefaultCellStyle.FormatProvider;
				}
			}
			if (style != null && (string)style.NullValue != "(null)") {
				result.NullValue = style.NullValue;
			} else if (OwningRow != null && (string)OwningRow.DefaultCellStyle.NullValue != "(null)") {
				result.NullValue = OwningRow.DefaultCellStyle.NullValue;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && (string)DataGridView.AlternatingRowsDefaultCellStyle.NullValue != "(null)") {
					result.NullValue = DataGridView.AlternatingRowsDefaultCellStyle.NullValue;
				} else if ((string)DataGridView.RowsDefaultCellStyle.NullValue != "(null)") {
					result.NullValue = DataGridView.RowsDefaultCellStyle.NullValue;
				} else if (ColumnIndex >= 0 && (string)DataGridView.Columns [ColumnIndex].DefaultCellStyle.NullValue != "(null)") {
					result.NullValue = DataGridView.Columns [ColumnIndex].DefaultCellStyle.NullValue;
				} else {
					result.NullValue = DataGridView.DefaultCellStyle.NullValue;
				}
			}
			if (style != null && style.Padding != Padding.Empty) {
				result.Padding = style.Padding;
			} else if (OwningRow != null && OwningRow.DefaultCellStyle.Padding != Padding.Empty) {
				result.Padding = OwningRow.DefaultCellStyle.Padding;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && DataGridView.AlternatingRowsDefaultCellStyle.Padding != Padding.Empty) {
					result.Padding = DataGridView.AlternatingRowsDefaultCellStyle.Padding;
				} else if (DataGridView.RowsDefaultCellStyle.Padding != Padding.Empty) {
					result.Padding = DataGridView.RowsDefaultCellStyle.Padding;
				} else if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].DefaultCellStyle.Padding != Padding.Empty) {
					result.Padding = DataGridView.Columns [ColumnIndex].DefaultCellStyle.Padding;
				} else {
					result.Padding = DataGridView.DefaultCellStyle.Padding;
				}
			}
			if (style != null && style.SelectionBackColor != Color.Empty) {
				result.SelectionBackColor = style.SelectionBackColor;
			} else if (OwningRow != null && OwningRow.DefaultCellStyle.SelectionBackColor != Color.Empty) {
				result.SelectionBackColor = OwningRow.DefaultCellStyle.SelectionBackColor;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && DataGridView.AlternatingRowsDefaultCellStyle.SelectionBackColor != Color.Empty) {
					result.SelectionBackColor = DataGridView.AlternatingRowsDefaultCellStyle.SelectionBackColor;
				} else if (DataGridView.RowsDefaultCellStyle.SelectionBackColor != Color.Empty) {
					result.SelectionBackColor = DataGridView.RowsDefaultCellStyle.SelectionBackColor;
				} else if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].DefaultCellStyle.SelectionBackColor != Color.Empty) {
					result.SelectionBackColor = DataGridView.Columns [ColumnIndex].DefaultCellStyle.SelectionBackColor;
				} else {
					result.SelectionBackColor = DataGridView.DefaultCellStyle.SelectionBackColor;
				}
			}
			if (style != null && style.SelectionForeColor != Color.Empty) {
				result.SelectionForeColor = style.SelectionForeColor;
			} else if (OwningRow != null && OwningRow.DefaultCellStyle.SelectionForeColor != Color.Empty) {
				result.SelectionForeColor = OwningRow.DefaultCellStyle.SelectionForeColor;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && DataGridView.AlternatingRowsDefaultCellStyle.SelectionForeColor != Color.Empty) {
					result.SelectionForeColor = DataGridView.AlternatingRowsDefaultCellStyle.SelectionForeColor;
				} else if (DataGridView.RowsDefaultCellStyle.SelectionForeColor != Color.Empty) {
					result.SelectionForeColor = DataGridView.RowsDefaultCellStyle.SelectionForeColor;
				} else if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].DefaultCellStyle.SelectionForeColor != Color.Empty) {
					result.SelectionForeColor = DataGridView.Columns [ColumnIndex].DefaultCellStyle.SelectionForeColor;
				} else {
					result.SelectionForeColor = DataGridView.DefaultCellStyle.SelectionForeColor;
				}
			}
			if (style != null && style.Tag != null) {
				result.Tag = style.Tag;
			} else if (OwningRow != null && OwningRow.DefaultCellStyle.Tag != null) {
				result.Tag = OwningRow.DefaultCellStyle.Tag;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && DataGridView.AlternatingRowsDefaultCellStyle.Tag != null) {
					result.Tag = DataGridView.AlternatingRowsDefaultCellStyle.Tag;
				} else if (DataGridView.RowsDefaultCellStyle.Tag != null) {
					result.Tag = DataGridView.RowsDefaultCellStyle.Tag;
				} else if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].DefaultCellStyle.Tag != null) {
					result.Tag = DataGridView.Columns [ColumnIndex].DefaultCellStyle.Tag;
				} else {
					result.Tag = DataGridView.DefaultCellStyle.Tag;
				}
			}
			if (style != null && style.WrapMode != DataGridViewTriState.NotSet) {
				result.WrapMode = style.WrapMode;
			} else if (OwningRow != null && OwningRow.DefaultCellStyle.WrapMode != DataGridViewTriState.NotSet) {
				result.WrapMode = OwningRow.DefaultCellStyle.WrapMode;
			} else if (DataGridView != null) {
				if ((RowIndex % 2) == 1 && DataGridView.AlternatingRowsDefaultCellStyle.WrapMode != DataGridViewTriState.NotSet) {
					result.WrapMode = DataGridView.AlternatingRowsDefaultCellStyle.WrapMode;
				} else if (DataGridView.RowsDefaultCellStyle.WrapMode != DataGridViewTriState.NotSet) {
					result.WrapMode = DataGridView.RowsDefaultCellStyle.WrapMode;
				} else if (ColumnIndex >= 0 && DataGridView.Columns [ColumnIndex].DefaultCellStyle.WrapMode != DataGridViewTriState.NotSet) {
					result.WrapMode = DataGridView.Columns [ColumnIndex].DefaultCellStyle.WrapMode;
				} else {
					result.WrapMode = DataGridView.DefaultCellStyle.WrapMode;
				}
			}
			return result;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual void InitializeEditingControl (int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle) {
			if (DataGridView == null || DataGridView.EditingControl == null) {
				throw new InvalidOperationException("No editing control defined");
			}
		}

		public virtual bool KeyEntersEditMode (KeyEventArgs e) {
			throw new NotImplementedException();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static int MeasureTextHeight (Graphics graphics, string text, Font font, int maxWidth, TextFormatFlags flags) {
			if (graphics == null) {
				throw new ArgumentNullException("Graphics argument null");
			}
			if (font == null) {
				throw new ArgumentNullException("Font argument null");
			}
			if (maxWidth < 1) {
				throw new ArgumentOutOfRangeException("maxWidth is less than 1.");
			}
			// if (flags ---> InvalidEnumArgumentException	
			throw new NotImplementedException();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static int MeasureTextHeight (Graphics graphics, string text, Font font, int maxWidth, TextFormatFlags flags, out bool widthTruncated) {
			throw new NotImplementedException();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Size MeasureTextPreferredSize (Graphics graphics, string text, Font font, float maxRatio, TextFormatFlags flags) {
			if (graphics == null) {
				throw new ArgumentNullException("Graphics argument null");
			}
			if (font == null) {
				throw new ArgumentNullException("Font argument null");
			}
			if (maxRatio <= 0) {
				throw new ArgumentOutOfRangeException("maxRatio is less than or equals to 0.");
			}
			throw new NotImplementedException();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Size MeasureTextSize (Graphics graphics, string text, Font font, TextFormatFlags flags) {
			/////////////////////////// ¿flags?
			return graphics.MeasureString(text, font).ToSize();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static int MeasureTextWidth (Graphics graphics, string text, Font font, int maxHeight, TextFormatFlags flags) {
			if (graphics == null) {
				throw new ArgumentNullException("Graphics argument null");
			}
			if (font == null) {
				throw new ArgumentNullException("Font argument null");
			}
			if (maxHeight < 1) {
				throw new ArgumentOutOfRangeException("maxHeight is less than 1.");
			}
			throw new NotImplementedException();
		}

		public virtual object ParseFormattedValue (object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter) {
			if (cellStyle == null) {
				throw new ArgumentNullException("cellStyle is null.");
			}
			if (formattedValueType == null) {
				throw new FormatException("The System.Windows.Forms.DataGridViewCell.FormattedValueType property value is null.");
			}
			if (formattedValue == null) {
				throw new ArgumentException("formattedValue is null.");
			}
			if (formattedValue.GetType() != formattedValueType) {
			}
			return null;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual void PositionEditingControl (bool setLocation, bool setSize, Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow) {
			//throw new NotImplementedException();
			if (setLocation)
				DataGridView.EditingControl.Location = cellBounds.Location;
				
			if (setSize)
				DataGridView.EditingControl.Size = cellBounds.Size;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual Rectangle PositionEditingPanel (Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow) {
			throw new NotImplementedException();
		}

		public override string ToString () {
			return String.Format("{0} {RowIndex = {1}, ColumnIndex = {2}}", this.GetType().FullName, RowIndex, columnIndex);
		}

		protected virtual Rectangle BorderWidths (DataGridViewAdvancedBorderStyle advancedBorderStyle)
		{
			return new Rectangle (2, 2, 2, 2);
		}

		protected virtual bool ClickUnsharesRow (DataGridViewCellEventArgs e)
		{
			throw new NotImplementedException();
		}

		protected virtual bool ContentClickUnsharesRow (DataGridViewCellEventArgs e)
		{
			throw new NotImplementedException();
		}

		protected virtual bool ContentDoubleClickUnsharesRow (DataGridViewCellEventArgs e)
		{
			throw new NotImplementedException();
		}

		protected virtual AccessibleObject CreateAccessibilityInstance () {
			return new DataGridViewCellAccessibleObject(this);
		}

		protected virtual void Dispose (bool disposing) {
		}

		protected virtual bool DoubleClickUnsharesRow (DataGridViewCellEventArgs e) {
			throw new NotImplementedException();
		}

		protected virtual bool EnterUnsharesRow (int rowIndex, bool throughMouseClick) {
			throw new NotImplementedException();
		}

		protected virtual object GetClipboardContent (int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format) {
			throw new NotImplementedException();
		}

		protected virtual Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex) {
			return Rectangle.Empty;
		}

		protected virtual Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex) {
			throw new NotImplementedException();
		}

		protected internal virtual string GetErrorText (int rowIndex) {
			throw new NotImplementedException();
		}

		protected virtual object GetFormattedValue (object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
		{
			throw new NotImplementedException();
		}

		protected virtual Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize) {
			throw new NotImplementedException();
		}

		protected virtual Size GetSize (int rowIndex) {
			DataGridViewCellStyle style = InheritedStyle;
			throw new NotImplementedException();
		}

		protected virtual object GetValue (int rowIndex) {
			
			if (DataGridView != null && (RowIndex < 0 || RowIndex >= DataGridView.Rows.Count))
				throw new ArgumentOutOfRangeException ("rowIndex", "Specified argument was out of the range of valid values.");
				
			return valuex;
		}

		protected virtual bool KeyDownUnsharesRow (KeyEventArgs e, int rowIndex) {
			throw new NotImplementedException();
		}

		protected virtual bool KeyPressUnsharesRow (KeyPressEventArgs e, int rowIndex) {
			throw new NotImplementedException();
		}

		protected virtual bool KeyUpUnsharesRow (KeyEventArgs e, int rowIndex) {
			throw new NotImplementedException();
		}

		protected virtual bool LeaveUnsharesRow (int rowIndex, bool throughMouseClick) {
			throw new NotImplementedException();
		}

		protected virtual bool MouseClickUnsharesRow (DataGridViewCellMouseEventArgs e) {
			throw new NotImplementedException();
		}

		protected virtual bool MouseDoubleClickUnsharesRow (DataGridViewCellMouseEventArgs e) {
			throw new NotImplementedException();
		}

		protected virtual bool MouseDownUnsharesRow (DataGridViewCellMouseEventArgs e) {
			throw new NotImplementedException();
		}

		protected virtual bool MouseEnterUnsharesRow (int rowIndex) {
			throw new NotImplementedException();
		}

		protected virtual bool MouseLeaveUnsharesRow (int rowIndex) {
			throw new NotImplementedException();
		}

		protected virtual bool MouseMoveUnsharesRow (DataGridViewCellMouseEventArgs e) {
			throw new NotImplementedException();
		}

		protected virtual bool MouseUpUnsharesRow (DataGridViewCellMouseEventArgs e) {
			throw new NotImplementedException();
		}

		protected virtual void OnClick (DataGridViewCellEventArgs e) {
		}

		internal void OnClickInternal (DataGridViewCellEventArgs e) {
			OnClick (e);
		}

		protected virtual void OnContentClick (DataGridViewCellEventArgs e) {
		}
		
		internal void OnContentClickInternal (DataGridViewCellEventArgs e) {
			OnContentClick (e);
		}

		protected virtual void OnContentDoubleClick (DataGridViewCellEventArgs e) {
		}
		
		internal void OnContentDoubleClickInternal (DataGridViewCellEventArgs e) {
			OnContentDoubleClick (e);
		}

		protected override void OnDataGridViewChanged () {
		}
		
		internal void OnDataGridViewChangedInternal () {
			OnDataGridViewChanged ();
		}

		protected virtual void OnDoubleClick (DataGridViewCellEventArgs e) {
		}

		internal void OnDoubleClickInternal (DataGridViewCellEventArgs e) {
			OnDoubleClick (e);
		}
		
		protected virtual void OnEnter (int rowIndex, bool throughMouseClick) {
		}

		internal void OnEnterInternal (int rowIndex, bool throughMouseClick) {
			OnEnter (rowIndex, throughMouseClick);
		}

		protected virtual void OnKeyDown (KeyEventArgs e, int rowIndex) {
		}

		internal void OnKeyDownInternal (KeyEventArgs e, int rowIndex) {
			OnKeyDown (e, rowIndex);
		}

		protected virtual void OnKeyPress (KeyPressEventArgs e, int rowIndex) {
		}

		internal void OnKeyPressInternal (KeyPressEventArgs e, int rowIndex) {
			OnKeyPress (e, rowIndex);
		}

		protected virtual void OnKeyUp (KeyEventArgs e, int rowIndex) {
		}
		
		internal void OnKeyUpInternal (KeyEventArgs e, int rowIndex) {
			OnKeyUp (e, rowIndex);
		}

		protected virtual void OnLeave (int rowIndex, bool throughMouseClick) {
		}
		
		internal void OnLeaveInternal (int rowIndex, bool throughMouseClick) {
			OnLeave (rowIndex, throughMouseClick);
		}

		protected virtual void OnMouseClick (DataGridViewCellMouseEventArgs e) {
		}

		internal void OnMouseClickInternal (DataGridViewCellMouseEventArgs e) {
			OnMouseClick (e);
		}

		protected virtual void OnMouseDoubleClick (DataGridViewCellMouseEventArgs e) {
		}
		
		internal void OnMouseDoubleClickInternal (DataGridViewCellMouseEventArgs e) {
			OnMouseDoubleClick (e);
		}

		protected virtual void OnMouseDown (DataGridViewCellMouseEventArgs e) {
		}

		internal void OnMouseDownInternal (DataGridViewCellMouseEventArgs e) {
			OnMouseDown (e);
		}

		protected virtual void OnMouseEnter (int rowIndex) {
		}
		
		internal void OnMouseEnterInternal (int rowIndex) {
			OnMouseEnter (rowIndex) ;
		}

		protected virtual void OnMouseLeave (int rowIndex) {
		}

		internal void OnMouseLeaveInternal (int e) {
			OnMouseLeave (e);
		}
		
		protected virtual void OnMouseMove (DataGridViewCellMouseEventArgs e) {
		}
		
		internal void OnMouseMoveInternal (DataGridViewCellMouseEventArgs e) {
			OnMouseMove (e);
		}

		protected virtual void OnMouseUp (DataGridViewCellMouseEventArgs e) {
		}

		internal void OnMouseUpInternal (DataGridViewCellMouseEventArgs e) {
			OnMouseUp (e);
		}
		
		protected virtual void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
			throw new NotImplementedException();
		}

		protected virtual void PaintBorder (Graphics graphics, Rectangle clipBounds, Rectangle bounds, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle) {
			Pen pen = new Pen(DataGridView.GridColor);
			/*
			switch (advancedBorderStyle.All) {
				case DataGridViewAdvancedCellBorderStyle.None:
					break;
				case DataGridViewAdvancedCellBorderStyle.Single:
					graphics.DrawRectangle(pen, bounds);
					break;
				case DataGridViewAdvancedCellBorderStyle.Inset:
					bounds.X += 1;
					bounds.Y += 1;
					bounds.Width -= 2;
					bounds.Height -= 2;
					graphics.DrawRectangle(pen, bounds);
					break;
				case DataGridViewAdvancedCellBorderStyle.InsetDouble:
				case DataGridViewAdvancedCellBorderStyle.Outset:
				case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
				case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
					break;
				case DataGridViewAdvancedCellBorderStyle.NotSet:
				*/
					switch (advancedBorderStyle.Left) {
						case DataGridViewAdvancedCellBorderStyle.None:
							break;
						case DataGridViewAdvancedCellBorderStyle.Single:
							graphics.DrawLine(pen, bounds.X, bounds.Y, bounds.X, bounds.Y + bounds.Height - 1);
							break;
						case DataGridViewAdvancedCellBorderStyle.Inset:
							graphics.DrawLine(pen, bounds.X + 2, bounds.Y, bounds.X + 2, bounds.Y + bounds.Height - 1);
							break;
						case DataGridViewAdvancedCellBorderStyle.InsetDouble:
						case DataGridViewAdvancedCellBorderStyle.Outset:
						case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
							graphics.DrawLine(pen, bounds.X, bounds.Y, bounds.X, bounds.Y + bounds.Height - 1);
							graphics.DrawLine(pen, bounds.X + 2, bounds.Y, bounds.X + 2, bounds.Y + bounds.Height - 1);
							break;
						case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
							break;
					}
					switch (advancedBorderStyle.Right) {
						case DataGridViewAdvancedCellBorderStyle.None:
							break;
						case DataGridViewAdvancedCellBorderStyle.Single:
							graphics.DrawLine(pen, bounds.X + bounds.Width - 1, bounds.Y, bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);
							break;
						case DataGridViewAdvancedCellBorderStyle.Inset:
							graphics.DrawLine(pen, bounds.X + bounds.Width + 1, bounds.Y, bounds.X + bounds.Width - 3, bounds.Y + bounds.Height - 1);
							break;
						case DataGridViewAdvancedCellBorderStyle.InsetDouble:
						case DataGridViewAdvancedCellBorderStyle.Outset:
						case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
						case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
							break;
					}
					switch (advancedBorderStyle.Top) {
						case DataGridViewAdvancedCellBorderStyle.None:
							break;
						case DataGridViewAdvancedCellBorderStyle.Single:
							graphics.DrawLine(pen, bounds.X, bounds.Y, bounds.X + bounds.Width - 1, bounds.Y);
							break;
						case DataGridViewAdvancedCellBorderStyle.Inset:
							graphics.DrawLine(pen, bounds.X, bounds.Y + 2, bounds.X + bounds.Width - 1, bounds.Y + bounds.Height + 1);
							break;
						case DataGridViewAdvancedCellBorderStyle.InsetDouble:
						case DataGridViewAdvancedCellBorderStyle.Outset:
						case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
						case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
							break;
					}
					switch (advancedBorderStyle.Bottom) {
						case DataGridViewAdvancedCellBorderStyle.None:
							break;
						case DataGridViewAdvancedCellBorderStyle.Single:
							graphics.DrawLine(pen, bounds.X, bounds.Y + bounds.Height - 1, bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);
							break;
						case DataGridViewAdvancedCellBorderStyle.Inset:
						case DataGridViewAdvancedCellBorderStyle.InsetDouble:
						case DataGridViewAdvancedCellBorderStyle.Outset:
						case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
						case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
							break;
					}
			//		break;
			//}
		}

		protected virtual void PaintErrorIcon (Graphics graphics, Rectangle clipBounds, Rectangle cellValueBounds, string errorText) {
			throw new NotImplementedException();
		}

		protected virtual bool SetValue (int rowIndex, object value) {
			if (valuex != value) {
				valuex = value;
				RaiseCellValueChanged (new DataGridViewCellEventArgs (ColumnIndex, RowIndex));
				return true;
			}
			return false;
		}

		private void OnStyleChanged (object sender, EventArgs args) {
			if (DataGridView != null) {
				DataGridView.RaiseCellStyleChanged(new DataGridViewCellEventArgs(ColumnIndex, RowIndex));
			}
		}

		internal void InternalPaint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
			Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		}

		internal void SetOwningRow (DataGridViewRow row) {
			owningRow = row;
		}
		
		internal void SetOwningColumn (DataGridViewColumn col) {
			owningColumn = col;
		}

		internal void SetColumnIndex (int index) {
			columnIndex = index;
		}

		internal void SetContentBounds (Rectangle bounds) {
			contentBounds = bounds;
		}

		internal void SetIsInEditMode (bool isInEditMode) {
			this.isInEditMode = isInEditMode;
		}

		internal void SetSize (Size size) {
			this.size = size;
		}

		internal void OnErrorTextChanged (DataGridViewCellEventArgs args) {
			if (DataGridView != null) {
				DataGridView.OnCellErrorTextChanged(args);
			}
		}

		[ComVisibleAttribute(true)]
		protected class DataGridViewCellAccessibleObject : AccessibleObject {

			private DataGridViewCell dataGridViewCell;

			public DataGridViewCellAccessibleObject () {
			}

			public DataGridViewCellAccessibleObject (DataGridViewCell cell) {
				this.dataGridViewCell = cell;
			}

			public override Rectangle Bounds {
				get { throw new NotImplementedException(); }
			}

			public override string DefaultAction {
				get { return "Edit"; }
			}

			public override string Name {
				get { return dataGridViewCell.OwningColumn.HeaderText + ": " + dataGridViewCell.RowIndex.ToString(); }
			}

			public DataGridViewCell Owner {
				get { return dataGridViewCell; }
				set { dataGridViewCell = value; }
			}

			public override AccessibleObject Parent {
				get { return dataGridViewCell.OwningRow.AccessibilityObject; }
			}

			public override AccessibleRole Role {
				get { return AccessibleRole.Cell; }
			}

			public override AccessibleStates State {
				get {
					if (dataGridViewCell.Selected) {
						return AccessibleStates.Selected;
					}
					else {
						return AccessibleStates.Focused;
					}
				}
			}

			public override string Value {
				get {
					if (dataGridViewCell.FormattedValue == null) {
						return "(null)";
					}
					return dataGridViewCell.FormattedValue.ToString();
				}
			}

			public override void DoDefaultAction () {
				if (dataGridViewCell.DataGridView.EditMode != DataGridViewEditMode.EditProgrammatically) {
					if (dataGridViewCell.IsInEditMode) {
						// commit edit
					}
					else {
						// begin edit
					}
				}
			}

			public override AccessibleObject GetChild (int index) {
				throw new NotImplementedException();
			}

			public override int GetChildCount () {
				if (dataGridViewCell.IsInEditMode) {
					return 1;
				}
				return -1;
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
						dataGridViewCell.dataGridViewOwner.Focus();
						break;
					case AccessibleSelection.TakeSelection:
						//dataGridViewCell.Focus();
						break;
					case AccessibleSelection.AddSelection:
						dataGridViewCell.dataGridViewOwner.SelectedCells.InternalAdd(dataGridViewCell);
						break;
					case AccessibleSelection.RemoveSelection:
						dataGridViewCell.dataGridViewOwner.SelectedCells.InternalRemove(dataGridViewCell);
						break;
				}
			}

		}

	}

}

#endif
