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
//	Ivan N. Zlatev  <contact@i-nz.net>
//


#if NET_2_0

using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	[TypeConverter (typeof (DataGridViewCellConverter))]
	public abstract class DataGridViewCell : DataGridViewElement, ICloneable, IDisposable
	{
		private DataGridView dataGridViewOwner;

		private AccessibleObject accessibilityObject;
		private int columnIndex;
		private ContextMenuStrip contextMenuStrip;
		private bool displayed;
		private string errorText;
		private bool isInEditMode;
		private DataGridViewRow owningRow;
		private DataGridViewTriState readOnly;
		private bool selected;
		private DataGridViewCellStyle style;
		private object tag;
		private string toolTipText;
		private object valuex;
		private Type valueType;

		protected DataGridViewCell ()
		{
			columnIndex = -1;
			dataGridViewOwner = null;
			errorText = string.Empty;
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
			get { return null; }
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
				if (this is DataGridViewTopLeftHeaderCell)
					return GetErrorIconBounds (null, null, RowIndex);

				if (DataGridView == null || columnIndex < 0)
					throw new InvalidOperationException ();
				if (RowIndex < 0 || RowIndex >= DataGridView.Rows.Count)
					throw new ArgumentOutOfRangeException ("rowIndex", "Specified argument was out of the range of valid values.");

				return GetErrorIconBounds (null, null, RowIndex);
			}
		}

		[Browsable (false)]
		public string ErrorText {
			get { 
				if (this is DataGridViewTopLeftHeaderCell)
					return GetErrorText (-1);
					
				if (OwningRow == null)
					return string.Empty;
					
				return GetErrorText (OwningRow.Index); 
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
				return GetFormattedValue (Value, RowIndex, ref style, null, null, DataGridViewDataErrorContexts.Formatting);
			}
		}

		[Browsable (false)]
		public virtual Type FormattedValueType {
			get { return null; }
		}

		[Browsable (false)]
		public virtual bool Frozen {
			get {
				if (DataGridView == null)
					return false;
				
				if (RowIndex >= 0)
					return OwningRow.Frozen && OwningColumn.Frozen;
					
				return false;
			}
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
			get {
				if (DataGridView == null || columnIndex < 0 || columnIndex >= DataGridView.Columns.Count)
					return null;
					
				return DataGridView.Columns[columnIndex];
			}
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
					return new Size (-1, -1);
					
				return GetPreferredSize (Hwnd.GraphicsContext, InheritedStyle, RowIndex, Size.Empty); 
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual bool ReadOnly {
			get {
				if (DataGridView != null && DataGridView.ReadOnly)
					return true;
				
				if (readOnly != DataGridViewTriState.NotSet)
					return readOnly == DataGridViewTriState.True;
					
				if (OwningRow != null && !OwningRow.IsShared && OwningRow.ReadOnly)
					return true;
				
				if (OwningColumn != null && OwningColumn.ReadOnly)
					return true;
					
				return false;
			}
			set {
				readOnly = value ? DataGridViewTriState.True : DataGridViewTriState.False;
				if (value) {
					SetState (DataGridViewElementStates.ReadOnly | State);
				} else {
					SetState (~DataGridViewElementStates.ReadOnly & State);
				}
			}
		}

		[Browsable (false)]
		public virtual bool Resizable {
			get {
				if (DataGridView == null)
					return false;

				// Shared cells aren't resizable
				if (RowIndex == -1 || columnIndex == -1)
					return false;
					
				return OwningRow.Resizable == DataGridViewTriState.True || OwningColumn.Resizable == DataGridViewTriState.True; 
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
					if (RowIndex >= 0 && RowIndex < DataGridView.Rows.Count && DataGridView.Rows [RowIndex].Selected)
						return true;
						
					if (ColumnIndex >= 0 && ColumnIndex < DataGridView.Columns.Count && DataGridView.Columns [ColumnIndex].Selected)
						return true;
				}
				
				return false;
			}
			set {
				bool changed = selected != value;
				selected = value;

				if (value != ((State & DataGridViewElementStates.Selected) != 0))
					SetState(State ^ DataGridViewElementStates.Selected);
				
				// If our row is selected, unselect it and select
				// the first cell in it that isn't us
				if (!selected && OwningRow != null && OwningRow.Selected) {
					OwningRow.Selected = false;
					
					if (columnIndex != 0 && OwningRow.Cells.Count > 0)
						OwningRow.Cells[0].Selected = true;
					else if (OwningRow.Cells.Count > 1)
						OwningRow.Cells[1].Selected = true;
				}

				if (changed && DataGridView != null && DataGridView.IsHandleCreated)
					DataGridView.Invalidate ();
			}
		}

		[Browsable (false)]
		public Size Size {
			get {
				if (DataGridView == null)
					return new Size (-1, -1);
					
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
			get { 
				if (valueType == null) {
					if (DataProperty != null)
						valueType = DataProperty.PropertyType;
					else if (OwningColumn != null)
						valueType = OwningColumn.ValueType;
				}

				return valueType;
			}
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

		internal override void SetState (DataGridViewElementStates state) {
			base.SetState (state);
			if (DataGridView != null)
				DataGridView.OnCellStateChangedInternal (new DataGridViewCellStateChangedEventArgs (this, state));
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual DataGridViewAdvancedBorderStyle AdjustCellBorderStyle (DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStyleInput,	DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow) {
			return dataGridViewAdvancedBorderStyleInput;
		}

		public virtual object Clone ()
		{
			DataGridViewCell result = (DataGridViewCell) Activator.CreateInstance (GetType ());
			result.accessibilityObject = this.accessibilityObject;
			result.columnIndex = this.columnIndex;
			result.displayed = this.displayed;
			result.errorText = this.errorText;
			result.isInEditMode = this.isInEditMode;
			result.owningRow = this.owningRow;
			result.readOnly = this.readOnly;
			result.selected = this.selected;
			result.style = this.style;
			result.tag = this.tag;
			result.toolTipText = this.toolTipText;
			result.valuex = this.valuex;
			result.valueType = this.valueType;
			return result;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual void DetachEditingControl ()
		{
		}

		public void Dispose ()
		{
		}

		public Rectangle GetContentBounds (int rowIndex)
		{
			if (DataGridView == null)
				return Rectangle.Empty;
			
			return GetContentBounds (Hwnd.GraphicsContext, InheritedStyle, rowIndex);
		}

		public object GetEditedFormattedValue (int rowIndex, DataGridViewDataErrorContexts context)
		{
			if (DataGridView == null)
				return null;
			
			if (rowIndex < 0 || rowIndex >= DataGridView.RowCount)
				throw new ArgumentOutOfRangeException ("rowIndex", "Specified argument was out of the range of valid values.");
			
			// If we are in edit mode, this returns the value of the editing control
			// If we aren't in edit mode, return the cell's value
			// Basically, return what the user is currently seeing
			if (IsInEditMode) {
				if (DataGridView.EditingControl != null)
					return (DataGridView.EditingControl as IDataGridViewEditingControl).GetEditingControlFormattedValue (context);
				else
					return (this as IDataGridViewEditingCell).GetEditingCellFormattedValue (context);
			}
				
			DataGridViewCellStyle style = InheritedStyle;
			
			return GetFormattedValue (GetValue (rowIndex), rowIndex, ref style, null, null, context);
		}

		public virtual ContextMenuStrip GetInheritedContextMenuStrip (int rowIndex)
		{
			if (DataGridView == null)
				return null;
				
			if (rowIndex < 0 || rowIndex >= DataGridView.Rows.Count)
				throw new ArgumentOutOfRangeException ("rowIndex");
			if (columnIndex < 0)
				throw new InvalidOperationException ("cannot perform this on a column header cell");
				
			if (contextMenuStrip != null)
				return contextMenuStrip;
			if (OwningRow.ContextMenuStrip != null)
				return OwningRow.ContextMenuStrip;
			if (OwningColumn.ContextMenuStrip != null)
				return OwningColumn.ContextMenuStrip;
				
			return DataGridView.ContextMenuStrip;
		}

		public virtual DataGridViewElementStates GetInheritedState (int rowIndex)
		{
		
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
						
					if (row.Selected)
						result |= DataGridViewElementStates.Selected;
				}
				
				return result;
			}
			
			if (col != null) {
				if (col.Resizable == DataGridViewTriState.True && row.Resizable == DataGridViewTriState.True)
					result |= DataGridViewElementStates.Resizable;
				
				if (col.Visible && row.Visible)
					result |= DataGridViewElementStates.Visible;

				if (col.ReadOnly || row.ReadOnly)
					result |= DataGridViewElementStates.ReadOnly;

				if (col.Frozen || row.Frozen)
					result |= DataGridViewElementStates.Frozen;

				if (col.Displayed && row.Displayed)
					result |= DataGridViewElementStates.Displayed;

				if (col.Selected || row.Selected)
					result |= DataGridViewElementStates.Selected;
			}
			
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
				throw new ArgumentOutOfRangeException ("rowIndex");

			// Start with DataGridView.DefaultCellStyle
			DataGridViewCellStyle result = new DataGridViewCellStyle (DataGridView.DefaultCellStyle);

			// If we have a column, add OwningColumn.DefaultCellStyle
			if (OwningColumn != null)
				result.ApplyStyle (OwningColumn.DefaultCellStyle);

			// Add DataGridView.RowsDefaultCellStyle
			result.ApplyStyle (DataGridView.RowsDefaultCellStyle);

			// If we are an odd row, add DataGridView.AlternatingRowsDefaultCellStyle
			if (rowIndex % 2 == 1)
				result.ApplyStyle (DataGridView.AlternatingRowsDefaultCellStyle);

			// Add Row.DefaultCellStyle
			result.ApplyStyle (DataGridView.Rows.SharedRow (rowIndex).DefaultCellStyle);
			
			// Add cell's style
			if (HasStyle)
				result.ApplyStyle (Style);
			
			return result;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual void InitializeEditingControl (int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle) {
			if (DataGridView == null || DataGridView.EditingControl == null) {
				throw new InvalidOperationException("No editing control defined");
			}
		}

		public virtual bool KeyEntersEditMode (KeyEventArgs e)
		{
			return false;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static int MeasureTextHeight (Graphics graphics, string text, Font font, int maxWidth, TextFormatFlags flags)
		{
			if (graphics == null)
				throw new ArgumentNullException ("Graphics argument null");
			if (font == null)
				throw new ArgumentNullException ("Font argument null");
			if (maxWidth < 1)
				throw new ArgumentOutOfRangeException ("maxWidth is less than 1.");

			return TextRenderer.MeasureText (graphics, text, font, new Size (maxWidth, 0), flags).Height;
		}

		[MonoTODO ("does not use widthTruncated parameter")]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static int MeasureTextHeight (Graphics graphics, string text, Font font, int maxWidth, TextFormatFlags flags, out bool widthTruncated)
		{
			widthTruncated = false;
			return TextRenderer.MeasureText (graphics, text, font, new Size (maxWidth, 0), flags).Height;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Size MeasureTextPreferredSize (Graphics graphics, string text, Font font, float maxRatio, TextFormatFlags flags)
		{
			if (graphics == null)
				throw new ArgumentNullException ("Graphics argument null");
			if (font == null)
				throw new ArgumentNullException ("Font argument null");
			if (maxRatio <= 0)
				throw new ArgumentOutOfRangeException ("maxRatio is less than or equals to 0.");

			// I couldn't find a case where maxRatio
			// affected anything on MS
			return MeasureTextSize (graphics, text, font, flags);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static Size MeasureTextSize (Graphics graphics, string text, Font font, TextFormatFlags flags)
		{
			return TextRenderer.MeasureText (graphics, text, font, Size.Empty, flags);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static int MeasureTextWidth (Graphics graphics, string text, Font font, int maxHeight, TextFormatFlags flags)
		{
			if (graphics == null)
				throw new ArgumentNullException ("Graphics argument null");
			if (font == null)
				throw new ArgumentNullException ("Font argument null");
			if (maxHeight < 1)
				throw new ArgumentOutOfRangeException ("maxHeight is less than 1.");

			return TextRenderer.MeasureText (graphics, text, font, new Size (0, maxHeight), flags).Width;
		}

		public virtual object ParseFormattedValue (object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
		{
			if (cellStyle == null)
				throw new ArgumentNullException ("cellStyle is null.");
			if (FormattedValueType == null)
				throw new FormatException ("The System.Windows.Forms.DataGridViewCell.FormattedValueType property value is null.");
			if (formattedValue == null)
				throw new ArgumentException ("formattedValue is null.");
			if (ValueType == null)
				throw new FormatException ("valuetype is null");
			if (!FormattedValueType.IsAssignableFrom (formattedValue.GetType ()))
				throw new ArgumentException ("formattedValue is not of formattedValueType.");
			
			if (formattedValueTypeConverter == null)
				formattedValueTypeConverter = FormattedValueTypeConverter;
			if (valueTypeConverter == null)
				valueTypeConverter = ValueTypeConverter;

			if (valueTypeConverter != null && valueTypeConverter.CanConvertFrom (FormattedValueType))
				return valueTypeConverter.ConvertFrom (formattedValue);
			if (formattedValueTypeConverter != null && formattedValueTypeConverter.CanConvertTo (ValueType))
				return formattedValueTypeConverter.ConvertTo (formattedValue, ValueType);
			return Convert.ChangeType (formattedValue, ValueType);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual void PositionEditingControl (bool setLocation, bool setSize, Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
		{
			if (DataGridView.EditingControl != null) {
				if (setLocation && setSize)
					DataGridView.EditingControl.Bounds = cellBounds;
				else if (setLocation)
					DataGridView.EditingControl.Location = cellBounds.Location;	
				else if (setSize)
					DataGridView.EditingControl.Size = cellBounds.Size;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual Rectangle PositionEditingPanel (Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow) {
			throw new NotImplementedException();
		}

		public override string ToString () {
			return String.Format("{0} {{ ColumnIndex={1}, RowIndex={2} }}", this.GetType().Name, ColumnIndex, RowIndex);
		}

		protected virtual Rectangle BorderWidths (DataGridViewAdvancedBorderStyle advancedBorderStyle)
		{
			Rectangle r = Rectangle.Empty;

			r.X = BorderToWidth (advancedBorderStyle.Left);
			r.Y = BorderToWidth (advancedBorderStyle.Top);
			r.Width = BorderToWidth (advancedBorderStyle.Right);
			r.Height = BorderToWidth (advancedBorderStyle.Bottom);
			
			if (OwningColumn != null)
				r.Width += OwningColumn.DividerWidth;
			if (OwningRow != null)
				r.Height += OwningRow.DividerHeight;
				
			return r;
		}

		private int BorderToWidth (DataGridViewAdvancedCellBorderStyle style)
		{
			switch (style) {
				case DataGridViewAdvancedCellBorderStyle.None:
					return 0;
				case DataGridViewAdvancedCellBorderStyle.NotSet:
				case DataGridViewAdvancedCellBorderStyle.Single:
				case DataGridViewAdvancedCellBorderStyle.Inset:
				case DataGridViewAdvancedCellBorderStyle.Outset:
				case DataGridViewAdvancedCellBorderStyle.OutsetPartial:
				default:
					return 1;
				case DataGridViewAdvancedCellBorderStyle.InsetDouble:
				case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
					return 2;
			}
		}
		
		protected virtual bool ClickUnsharesRow (DataGridViewCellEventArgs e)
		{
			return false;
		}

		protected virtual bool ContentClickUnsharesRow (DataGridViewCellEventArgs e)
		{
			return false;
		}

		protected virtual bool ContentDoubleClickUnsharesRow (DataGridViewCellEventArgs e)
		{
			return false;
		}

		protected virtual AccessibleObject CreateAccessibilityInstance () {
			return new DataGridViewCellAccessibleObject(this);
		}

		protected virtual void Dispose (bool disposing) {
		}

		protected virtual bool DoubleClickUnsharesRow (DataGridViewCellEventArgs e)
		{
			return false;
		}

		protected virtual bool EnterUnsharesRow (int rowIndex, bool throughMouseClick)
		{
			return false;
		}

		protected virtual object GetClipboardContent (int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format) {
			if (DataGridView == null)
				return null;
				
			if (rowIndex < 0 || rowIndex >= DataGridView.RowCount)
				throw new ArgumentOutOfRangeException ("rowIndex", "Specified argument was out of the range of valid values.");
			
			string value = null;
			
			if (Selected) {
//				DataGridViewCellStyle style = GetInheritedStyle (null, rowIndex, false);
				value = GetEditedFormattedValue (rowIndex, DataGridViewDataErrorContexts.ClipboardContent | DataGridViewDataErrorContexts.Formatting) as string;
			}

			if (value == null)
				value = string.Empty;
				
			string table_prefix = string.Empty, cell_prefix = string.Empty, row_prefix = string.Empty;
			string table_suffix = string.Empty, cell_suffix = string.Empty, row_suffix = string.Empty;
			
			if (format == DataFormats.UnicodeText || format == DataFormats.Text) {
				if (lastCell && !inLastRow)
					cell_suffix = Environment.NewLine;
				else if (!lastCell)
					cell_suffix = "\t";
			} else if (format == DataFormats.CommaSeparatedValue) {
				if (lastCell && !inLastRow)
					cell_suffix = Environment.NewLine;
				else if (!lastCell)
					cell_suffix = ",";
			} else if (format == DataFormats.Html) {
				if (inFirstRow && firstCell)
					table_prefix = "<TABLE>";
				if (inLastRow && lastCell)
					table_suffix = "</TABLE>";
				if (firstCell)
					row_prefix = "<TR>";
				if (lastCell)
					row_suffix = "</TR>";
				cell_prefix = "<TD>";
				cell_suffix = "</TD>";
				
				if (!Selected) {
					value = "&nbsp;";
				}
			} else {
				return value;
			}
						
			value = table_prefix + row_prefix + cell_prefix + value + cell_suffix + row_suffix + table_suffix;
			
			return value;
		}
		
		internal object GetClipboardContentInternal (int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format) {
			return GetClipboardContent (rowIndex, firstCell, lastCell, inFirstRow, inLastRow, format);
		}

		protected virtual Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex) {
			return Rectangle.Empty;
		}

		protected virtual Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			return Rectangle.Empty;
		}

		protected internal virtual string GetErrorText (int rowIndex)
		{
			return errorText;
		}

		protected virtual object GetFormattedValue (object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
		{
			if (DataGridView == null)
				return null;
				
			if (rowIndex < 0 || rowIndex >= DataGridView.RowCount)
				throw new ArgumentOutOfRangeException ("rowIndex");

			// Give the user a chance to custom format
			if (!(this is DataGridViewRowHeaderCell)) {
				DataGridViewCellFormattingEventArgs e = new DataGridViewCellFormattingEventArgs (ColumnIndex, rowIndex, value, FormattedValueType, cellStyle);
				DataGridView.OnCellFormattingInternal (e);
			
				if (e.FormattingApplied)
					return e.Value;
			
				cellStyle = e.CellStyle;
				value = e.Value;
			}
			
			if (value == null || (cellStyle != null && value == cellStyle.DataSourceNullValue)) {
				if (FormattedValueType == typeof (string))
					return String.Empty;
			}

			if (FormattedValueType == typeof(string) && value is IFormattable && !String.IsNullOrEmpty (cellStyle.Format))
				return ((IFormattable) value).ToString (cellStyle.Format, cellStyle.FormatProvider);
			if (value != null && FormattedValueType.IsAssignableFrom (value.GetType()))
				return value;

			if (formattedValueTypeConverter == null)
				formattedValueTypeConverter = FormattedValueTypeConverter;
			if (valueTypeConverter == null)
				valueTypeConverter = ValueTypeConverter;

			if (valueTypeConverter != null && valueTypeConverter.CanConvertTo (FormattedValueType))
				return valueTypeConverter.ConvertTo (value, FormattedValueType);
			if (formattedValueTypeConverter != null && formattedValueTypeConverter.CanConvertFrom (ValueType))
				return formattedValueTypeConverter.ConvertFrom (value);

			return Convert.ChangeType (value, FormattedValueType);
		}

		protected virtual Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
		{
			return new Size (-1, -1);
		}

		protected virtual Size GetSize (int rowIndex)
		{
			if (RowIndex == -1)
				throw new InvalidOperationException ("Getting the Size property of a cell in a shared row is not a valid operation.");

			return new Size (OwningColumn.Width, OwningRow.Height);
		}

		protected virtual object GetValue (int rowIndex) {
			if (DataGridView != null && (RowIndex < 0 || RowIndex >= DataGridView.Rows.Count))
				throw new ArgumentOutOfRangeException ("rowIndex", "Specified argument was out of the range of valid values.");
		
			if (OwningRow != null && DataGridView != null && OwningRow.Index == DataGridView.NewRowIndex)
				return DefaultNewRowValue;

			if (DataProperty != null && OwningRow.DataBoundItem != null)
				return DataProperty.GetValue (OwningRow.DataBoundItem);
			
			if (valuex != null)
				return valuex;

			DataGridViewCellValueEventArgs dgvcvea = new DataGridViewCellValueEventArgs (columnIndex, rowIndex);
			if (DataGridView != null)
				DataGridView.OnCellValueNeeded (dgvcvea);
			return dgvcvea.Value;
		}
		
		private PropertyDescriptor DataProperty {
			get {
				if (OwningColumn != null && OwningColumn.DataColumnIndex != -1 && 
				    DataGridView != null && DataGridView.DataManager != null)
					return DataGridView.DataManager.GetItemProperties()[OwningColumn.DataColumnIndex];
				return null;
			}
		}

		private TypeConverter FormattedValueTypeConverter {
			get {
				if (FormattedValueType != null)
					return TypeDescriptor.GetConverter (FormattedValueType);
				return null;
			}
		}

		private TypeConverter ValueTypeConverter {
			get {
				if (DataProperty != null && DataProperty.Converter != null)
					return DataProperty.Converter;
				if (Value != null)
					return TypeDescriptor.GetConverter (Value);
				if (ValueType != null)
					return TypeDescriptor.GetConverter (ValueType);
				return null;
			}
		}

		protected virtual bool KeyDownUnsharesRow (KeyEventArgs e, int rowIndex)
		{
			return false;
		}

		protected virtual bool KeyPressUnsharesRow (KeyPressEventArgs e, int rowIndex)
		{
			return false;
		}

		protected virtual bool KeyUpUnsharesRow (KeyEventArgs e, int rowIndex)
		{
			return false;
		}

		protected virtual bool LeaveUnsharesRow (int rowIndex, bool throughMouseClick)
		{
			return false;
		}

		protected virtual bool MouseClickUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			return false;
		}

		protected virtual bool MouseDoubleClickUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			return false;
		}

		protected virtual bool MouseDownUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			return false;
		}

		protected virtual bool MouseEnterUnsharesRow (int rowIndex)
		{
			return false;
		}

		protected virtual bool MouseLeaveUnsharesRow (int rowIndex)
		{
			return false;
		}

		protected virtual bool MouseMoveUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			return false;
		}

		protected virtual bool MouseUpUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			return false;
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

		internal void PaintInternal (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			Paint (graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		}
			
		protected virtual void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			if ((paintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
				PaintPartBackground (graphics, cellBounds, cellStyle);
			if ((paintParts & DataGridViewPaintParts.SelectionBackground) == DataGridViewPaintParts.SelectionBackground)
				PaintPartSelectionBackground (graphics, cellBounds, cellState, cellStyle);
			if ((paintParts & DataGridViewPaintParts.ContentForeground) == DataGridViewPaintParts.ContentForeground)
				PaintPartContent (graphics, cellBounds, rowIndex, cellState, cellStyle, formattedValue);
			if ((paintParts & DataGridViewPaintParts.Border) == DataGridViewPaintParts.Border)
				PaintBorder (graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
			if ((paintParts & DataGridViewPaintParts.Focus) == DataGridViewPaintParts.Focus)
				PaintPartFocus (graphics, cellBounds);
			if ((paintParts & DataGridViewPaintParts.ErrorIcon) == DataGridViewPaintParts.ErrorIcon)
				if (!string.IsNullOrEmpty (ErrorText))
					PaintErrorIcon (graphics, clipBounds, cellBounds, ErrorText);
		}

		protected virtual void PaintBorder (Graphics graphics, Rectangle clipBounds, Rectangle bounds, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle)
		{
			Pen pen = new Pen (DataGridView.GridColor);

			// Paint the left border, if any
			switch (advancedBorderStyle.Left) {
				case DataGridViewAdvancedCellBorderStyle.Single:
					if (DataGridView.CellBorderStyle != DataGridViewCellBorderStyle.Single)
						graphics.DrawLine (pen, bounds.X, bounds.Y, bounds.X, bounds.Y + bounds.Height - 1);
					break;
				case DataGridViewAdvancedCellBorderStyle.Outset:
				case DataGridViewAdvancedCellBorderStyle.Inset:
					graphics.DrawLine(pen, bounds.X, bounds.Y, bounds.X, bounds.Y + bounds.Height - 1);
					break;
				case DataGridViewAdvancedCellBorderStyle.InsetDouble:
				case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
					graphics.DrawLine(pen, bounds.X, bounds.Y, bounds.X, bounds.Y + bounds.Height - 1);
					graphics.DrawLine(pen, bounds.X + 2, bounds.Y, bounds.X + 2, bounds.Y + bounds.Height - 1);
					break;
			}
			
			// Paint the right border, if any
			switch (advancedBorderStyle.Right) {
				case DataGridViewAdvancedCellBorderStyle.Single:
					graphics.DrawLine(pen, bounds.X + bounds.Width - 1, bounds.Y, bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);
					break;
				case DataGridViewAdvancedCellBorderStyle.Outset:
				case DataGridViewAdvancedCellBorderStyle.Inset:
				case DataGridViewAdvancedCellBorderStyle.InsetDouble:
				case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
					graphics.DrawLine(pen, bounds.X + bounds.Width, bounds.Y, bounds.X + bounds.Width, bounds.Y + bounds.Height - 1);
					break;
			}
			
			// Paint the top border, if any
			switch (advancedBorderStyle.Top) {
				case DataGridViewAdvancedCellBorderStyle.Single:
					if (DataGridView.CellBorderStyle != DataGridViewCellBorderStyle.Single)
						graphics.DrawLine(pen, bounds.X, bounds.Y, bounds.X + bounds.Width - 1, bounds.Y);
					break;
				case DataGridViewAdvancedCellBorderStyle.Outset:
				case DataGridViewAdvancedCellBorderStyle.Inset:
				case DataGridViewAdvancedCellBorderStyle.InsetDouble:
				case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
					graphics.DrawLine(pen, bounds.X, bounds.Y, bounds.X + bounds.Width - 1, bounds.Y);
					break;
			}
			
			// Paint the bottom border, if any
			switch (advancedBorderStyle.Bottom) {
				case DataGridViewAdvancedCellBorderStyle.Outset:
				case DataGridViewAdvancedCellBorderStyle.Inset:
				case DataGridViewAdvancedCellBorderStyle.Single:
				case DataGridViewAdvancedCellBorderStyle.InsetDouble:
				case DataGridViewAdvancedCellBorderStyle.OutsetDouble:
					graphics.DrawLine(pen, bounds.X, bounds.Y + bounds.Height - 1, bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);
					break;
			}
		}

		protected virtual void PaintErrorIcon (Graphics graphics, Rectangle clipBounds, Rectangle cellValueBounds, string errorText)
		{
			Rectangle error_bounds = GetErrorIconBounds (graphics, null, RowIndex);
			
			if (error_bounds.IsEmpty)
				return;

			Point loc = error_bounds.Location;
			loc.X += cellValueBounds.Left;
			loc.Y += cellValueBounds.Top;

			graphics.FillRectangle (Brushes.Red, new Rectangle (loc.X + 1, loc.Y + 2, 10, 7));
			graphics.FillRectangle (Brushes.Red, new Rectangle (loc.X + 2, loc.Y + 1, 8, 9));
			graphics.FillRectangle (Brushes.Red, new Rectangle (loc.X + 4, loc.Y, 4, 11));
			graphics.FillRectangle (Brushes.Red, new Rectangle (loc.X, loc.Y + 4, 12, 3));

			graphics.FillRectangle (Brushes.White, new Rectangle (loc.X + 5, loc.Y + 2, 2, 4));
			graphics.FillRectangle (Brushes.White, new Rectangle (loc.X + 5, loc.Y + 7, 2, 2));
		}

		internal virtual void PaintPartBackground (Graphics graphics, Rectangle cellBounds, DataGridViewCellStyle style)
		{
			Color color = style.BackColor;
			graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (color), cellBounds);
		}

		internal Pen GetBorderPen ()
		{
			return ThemeEngine.Current.ResPool.GetPen (DataGridView.GridColor);
		}

		internal virtual void PaintPartContent (Graphics graphics, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, DataGridViewCellStyle cellStyle, object formattedValue)
		{
			if (IsInEditMode)
				return;
				
			Color color = Selected ? cellStyle.SelectionForeColor : cellStyle.ForeColor;

			TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.TextBoxControl;
			flags |= AlignmentToFlags (style.Alignment);
			
			cellBounds.Height -= 2;
			cellBounds.Width -= 2;

			if (formattedValue != null)
				TextRenderer.DrawText (graphics, formattedValue.ToString (), cellStyle.Font, cellBounds, color, flags);
		}
		
		private void PaintPartFocus (Graphics graphics, Rectangle cellBounds)
		{
			cellBounds.Width--;
			cellBounds.Height--;
			
			if (DataGridView.ShowFocusCues && DataGridView.CurrentCell == this && DataGridView.Focused)
				ControlPaint.DrawFocusRectangle (graphics, cellBounds);
		}

		internal virtual void PaintPartSelectionBackground (Graphics graphics, Rectangle cellBounds, DataGridViewElementStates cellState, DataGridViewCellStyle cellStyle)
		{
			if ((cellState & DataGridViewElementStates.Selected) != DataGridViewElementStates.Selected)
				return;

			if (RowIndex >= 0 && IsInEditMode && EditType != null)
				return;
				
			Color color = cellStyle.SelectionBackColor;
			graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (color), cellBounds);
		}

		internal void PaintWork (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			object value;
			object formattedvalue;
			
			if (RowIndex == -1 && !(this is DataGridViewColumnHeaderCell)) {
				value = null;
				formattedvalue = null;
			} else if (RowIndex == -1) {
				value = Value;
				formattedvalue = Value;
			} else {
				value = Value;
				formattedvalue = GetFormattedValue (Value, rowIndex, ref cellStyle, null, null, DataGridViewDataErrorContexts.Formatting);
			}

			DataGridViewCellPaintingEventArgs pea = new DataGridViewCellPaintingEventArgs (DataGridView, graphics, clipBounds, cellBounds, rowIndex, columnIndex, cellState, value, formattedvalue, ErrorText, cellStyle, advancedBorderStyle, paintParts);
			DataGridView.OnCellPaintingInternal (pea);
			
			if (pea.Handled)
				return;
				
			pea.Paint (pea.ClipBounds, pea.PaintParts);
		}
		
		protected virtual bool SetValue (int rowIndex, object value)
		{
			object oldValue = this.Value;

			if (DataProperty != null && !DataProperty.IsReadOnly)
				DataProperty.SetValue (OwningRow.DataBoundItem, value);
			else
				valuex = value;

			if (!Object.ReferenceEquals (oldValue, value) || !Object.Equals (oldValue, value)) {
				RaiseCellValueChanged (new DataGridViewCellEventArgs (ColumnIndex, RowIndex));
				
				// Set this dirty flag back to false
				if (this is IDataGridViewEditingCell)
					(this as IDataGridViewEditingCell).EditingCellValueChanged = false;
				
				if (DataGridView != null)
					DataGridView.InvalidateCell (this);
					
				return true;
			}
			return false;
		}

		private void OnStyleChanged (object sender, EventArgs args) {
			if (DataGridView != null) {
				DataGridView.RaiseCellStyleChanged(new DataGridViewCellEventArgs(ColumnIndex, RowIndex));
			}
		}

		internal virtual Rectangle InternalErrorIconsBounds {
			get { return GetErrorIconBounds (null, null, -1); }
		}

		internal void InternalPaint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts) {
			Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		}

		internal void SetOwningRow (DataGridViewRow row) {
			owningRow = row;
		}
		
		internal void SetOwningColumn (DataGridViewColumn col) {
			columnIndex = col.Index;
		}

		internal void SetColumnIndex (int index) {
			columnIndex = index;
		}

		internal void SetIsInEditMode (bool isInEditMode) {
			this.isInEditMode = isInEditMode;
		}

		internal void OnErrorTextChanged (DataGridViewCellEventArgs args) {
			if (DataGridView != null) {
				DataGridView.OnCellErrorTextChanged(args);
			}
		}

		internal TextFormatFlags AlignmentToFlags (DataGridViewContentAlignment align)
		{
			TextFormatFlags flags = TextFormatFlags.Default;

			switch (align) {
				case DataGridViewContentAlignment.BottomCenter:
					flags |= TextFormatFlags.Bottom;
					flags |= TextFormatFlags.HorizontalCenter;
					break;
				case DataGridViewContentAlignment.BottomLeft:
					flags |= TextFormatFlags.Bottom;
					break;
				case DataGridViewContentAlignment.BottomRight:
					flags |= TextFormatFlags.Bottom;
					flags |= TextFormatFlags.Right;
					break;
				case DataGridViewContentAlignment.MiddleCenter:
					flags |= TextFormatFlags.VerticalCenter;
					flags |= TextFormatFlags.HorizontalCenter;
					break;
				case DataGridViewContentAlignment.MiddleLeft:
					flags |= TextFormatFlags.VerticalCenter;
					break;
				case DataGridViewContentAlignment.MiddleRight:
					flags |= TextFormatFlags.VerticalCenter;
					flags |= TextFormatFlags.Right;
					break;
				case DataGridViewContentAlignment.TopLeft:
					flags |= TextFormatFlags.Top;
					break;
				case DataGridViewContentAlignment.TopCenter:
					flags |= TextFormatFlags.HorizontalCenter;
					flags |= TextFormatFlags.Top;
					break;
				case DataGridViewContentAlignment.TopRight:
					flags |= TextFormatFlags.Right;
					flags |= TextFormatFlags.Top;
					break;
			}

			return flags;
		}

		internal Rectangle AlignInRectangle (Rectangle outer, Size inner, DataGridViewContentAlignment align)
		{
			int x = 0;
			int y = 0;

			if (align == DataGridViewContentAlignment.BottomLeft || align == DataGridViewContentAlignment.MiddleLeft || align == DataGridViewContentAlignment.TopLeft)
				x = outer.X;
			else if (align == DataGridViewContentAlignment.BottomCenter || align == DataGridViewContentAlignment.MiddleCenter || align == DataGridViewContentAlignment.TopCenter)
				x = Math.Max (outer.X + ((outer.Width - inner.Width) / 2), outer.Left);
			else if (align == DataGridViewContentAlignment.BottomRight || align == DataGridViewContentAlignment.MiddleRight || align == DataGridViewContentAlignment.TopRight)
				x = Math.Max (outer.Right - inner.Width, outer.X);
			if (align == DataGridViewContentAlignment.TopCenter || align == DataGridViewContentAlignment.TopLeft || align == DataGridViewContentAlignment.TopRight)
				y = outer.Y;
			else if (align == DataGridViewContentAlignment.MiddleCenter || align == DataGridViewContentAlignment.MiddleLeft || align == DataGridViewContentAlignment.MiddleRight)
				y = Math.Max (outer.Y + (outer.Height - inner.Height) / 2, outer.Y);
			else if (align == DataGridViewContentAlignment.BottomCenter || align == DataGridViewContentAlignment.BottomRight || align == DataGridViewContentAlignment.BottomLeft)
				y = Math.Max (outer.Bottom - inner.Height, outer.Y);

			return new Rectangle (x, y, Math.Min (inner.Width, outer.Width), Math.Min (inner.Height, outer.Height));
		}

		[ComVisibleAttribute(true)]
		protected class DataGridViewCellAccessibleObject : AccessibleObject {

			private DataGridViewCell dataGridViewCell;

			public DataGridViewCellAccessibleObject () {
			}

			public DataGridViewCellAccessibleObject (DataGridViewCell owner) {
				this.dataGridViewCell = owner;
			}

			public override Rectangle Bounds {
				get { throw new NotImplementedException(); }
			}

			public override string DefaultAction {
				get { return "Edit"; }
			}

			public override string Help {
				get { return base.Help; }
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
				set {
					if (owner == null)
						throw new InvalidOperationException ("owner is null");
						
					throw new NotImplementedException ();
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

	internal class DataGridViewCellConverter : TypeConverter
	{
	}

}

#endif
