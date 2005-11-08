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
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections;

namespace System.Windows.Forms {

	[ComVisibleAttribute(true)]
	[ClassInterfaceAttribute(ClassInterfaceType.AutoDispatch)]
	public class DataGridView : Control, ISupportInitialize {

		private DataGridViewAdvancedBorderStyle adjustedTopLeftHeaderBorderStyle;
		private DataGridViewAdvancedBorderStyle advancedCellBorderStyle;
		private DataGridViewAdvancedBorderStyle advancedColumnHeadersBorderStyle;
		private DataGridViewAdvancedBorderStyle advancedRowHeadersBorderStyle;
		private bool allowUserToAddRows;
		private bool allowUserToDeleteRows;
		private bool allowUserToOrderColumns;
		private bool allowUserToResizeColumns;
		private bool allowUserToResizeRows;
		private DataGridViewCellStyle alternatingRowsDefaultCellStyle;
		private bool autoGenerateColumns;
		private bool autoSize;
		private DataGridViewAutoSizeColumnsMode autoSizeColumnsMode;
		private DataGridViewAutoSizeRowsMode autoSizeRowsMode;
		private Color backColor;
		private Color backgroundColor;
		private Image backgroundImage;
		private ImageLayout backgroundImageLayout;
		private BorderStyle borderStyle;
		private DataGridViewCellBorderStyle cellBorderStyle;
		private DataGridViewClipboardCopyMode clipboardCopyMode;
		private DataGridViewHeaderBorderStyle columnHeadersBorderStyle;
		private DataGridViewCellStyle columnHeadersDefaultCellStyle;
		private int columnHeadersHeight;
		private DataGridViewColumnHeadersHeightSizeMode columnHeadersHeightSizeMode;
		private bool columnHeadersVisible;
		private DataGridViewColumnCollection columns;
		private DataGridViewCell currentCell;
		private Point currentCellAddress;
		private DataGridViewRow currentRow;
		private string dataMember;
		private object dataSource;
		private DataGridViewCellStyle defaultCellStyle;
		private Control editingControl;
		private DataGridViewEditMode editMode;
		private bool enableHeadersVisualStyles;
		private DataGridViewCell firstDisplayedCell;
		private int firstDisplayedScrollingColumnHiddenWidth;
		private int firstDisplayedScrollingColumnIndex;
		private int firstDisplayedScrollingRowIndex;
		private Font font = Control.DefaultFont;
		private Color foreColor = Control.DefaultForeColor;
		private Color gridColor = Color.FromKnownColor(KnownColor.ControlDarkDark);
		private int horizontalScrollingOffset;
		private bool isCurrentCellDirty;
		private bool isCurrentRowDirty;
		private bool multiSelect;
		private bool readOnly;
		private DataGridViewHeaderBorderStyle rowHeadersBorderStyle;
		private DataGridViewCellStyle rowHeadersDefaultCellStyle;
		private bool rowHeadersVisible;
		private int rowHeadersWidth;
		private DataGridViewRowHeadersWidthSizeMode rowHeadersWidthSizeMode;
		private DataGridViewRowCollection rows;
		private DataGridViewCellStyle rowsDefaultCellStyle;
		private DataGridViewRow rowTemplate;
		private ScrollBars scrollBars;
		private DataGridViewSelectionMode selectionMode;
		private bool showCellErrors;
		private bool showCellToolTips;
		private bool showEditingIcon;
		private bool showRowErrors;
		private DataGridViewColumn sortedColumn = null;
		private SortOrder sortOrder;
		private bool standardTab;
		private string text;
		private DataGridViewHeaderCell topLeftHeaderCell;
		private Cursor userSetCursor;
		private int verticalScrollingOffset;
		private bool virtualMode;
		private Size defaultSize;
		private HScrollBar horizontalScrollBar;
		private VScrollBar verticalScrollBar;

		internal int gridWidth;
		internal int gridHeight;

		public DataGridView () {
			adjustedTopLeftHeaderBorderStyle = new DataGridViewAdvancedBorderStyle();
			adjustedTopLeftHeaderBorderStyle.All = DataGridViewAdvancedCellBorderStyle.Single;
			advancedCellBorderStyle = new DataGridViewAdvancedBorderStyle();
			advancedCellBorderStyle.All = DataGridViewAdvancedCellBorderStyle.Single;
			advancedColumnHeadersBorderStyle = new DataGridViewAdvancedBorderStyle();
			advancedColumnHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.Single;
			advancedRowHeadersBorderStyle = new DataGridViewAdvancedBorderStyle();
			advancedRowHeadersBorderStyle.All = DataGridViewAdvancedCellBorderStyle.Single;
			alternatingRowsDefaultCellStyle = new DataGridViewCellStyle();
			allowUserToAddRows = true;
			allowUserToDeleteRows = true;
			allowUserToOrderColumns = false;
			allowUserToResizeColumns = true;
			allowUserToResizeRows = true;
			autoGenerateColumns = true;
			autoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
			autoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
			backColor = Control.DefaultBackColor;
			backgroundColor = SystemColors.AppWorkspace;
			borderStyle = BorderStyle.FixedSingle;
			cellBorderStyle = DataGridViewCellBorderStyle.None;
			clipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithAutoHeaderText;
			columnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
			columnHeadersDefaultCellStyle = new DataGridViewCellStyle();
			columnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
			columnHeadersDefaultCellStyle.ForeColor = SystemColors.WindowText;
			columnHeadersDefaultCellStyle.SelectionBackColor = SystemColors.Highlight;
			columnHeadersDefaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
			columnHeadersDefaultCellStyle.Font = this.Font;
			columnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
			columnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
			columnHeadersHeight = 23;
			columnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
			columnHeadersVisible = true;
			columns = CreateColumnsInstance();
			columns.CollectionChanged += OnColumnCollectionChanged;
			dataMember = String.Empty;
			defaultCellStyle = (DataGridViewCellStyle) columnHeadersDefaultCellStyle.Clone();
			editMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
			multiSelect = true;
			readOnly = false;
			rowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
			rowHeadersDefaultCellStyle = (DataGridViewCellStyle) defaultCellStyle.Clone();
			rowHeadersVisible = true;
			rowHeadersWidth = 43;
			rowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.EnableResizing;
			rows = CreateRowsInstance();
			rowsDefaultCellStyle = new DataGridViewCellStyle();
			selectionMode = DataGridViewSelectionMode.RowHeaderSelect;
			showCellErrors = true;
			showEditingIcon = true;
			userSetCursor = Cursor.Current;
			virtualMode = false;

			horizontalScrollBar = new HScrollBar();
			horizontalScrollBar.Dock = DockStyle.Bottom;
			horizontalScrollBar.Scroll += OnHScrollBarScroll;
			horizontalScrollBar.Visible = false;
			verticalScrollBar = new VScrollBar();
			verticalScrollBar.Dock = DockStyle.Right;
			verticalScrollBar.Scroll += OnVScrollBarScroll;
			verticalScrollBar.Visible = false;
		}

		public void BeginInit () {
		}

		public void EndInit () {
		}

		// Propiedades

		public virtual DataGridViewAdvancedBorderStyle AdjustedTopLeftHeaderBorderStyle {
			get { return adjustedTopLeftHeaderBorderStyle; }
		}

		public DataGridViewAdvancedBorderStyle AdvancedCellBorderStyle {
			get { return advancedCellBorderStyle; }
		}

		public DataGridViewAdvancedBorderStyle AdvancedColumnHeadersBorderStyle {
			get { return advancedColumnHeadersBorderStyle; }
		}

		public DataGridViewAdvancedBorderStyle AdvancedRowHeadersBorderStyle {
			get { return advancedRowHeadersBorderStyle; }
		}

		public bool AllowUserToAddRows {
			get { return allowUserToAddRows; }
			set {
				if (allowUserToAddRows != value) {
					allowUserToAddRows = value;
					OnAllowUserToAddRowsChanged(EventArgs.Empty);
				}
			}
		}

		public bool AllowUserToDeleteRows {
			get { return allowUserToDeleteRows; }
			set {
				if (allowUserToDeleteRows != value) {
					allowUserToDeleteRows = value;
					OnAllowUserToDeleteRowsChanged(EventArgs.Empty);
				}
			}
		}

		public bool AllowUserToOrderColumns {
			get { return allowUserToOrderColumns; }
			set {
				if (allowUserToOrderColumns != value) {
					allowUserToOrderColumns = value;
					OnAllowUserToOrderColumnsChanged(EventArgs.Empty);
				}
			}
		}

		public bool AllowUserToResizeColumns {
			get { return allowUserToResizeColumns; }
			set {
				if (allowUserToResizeColumns != value) {
					allowUserToResizeColumns = value;
					OnAllowUserToResizeColumnsChanged(EventArgs.Empty);
				}
			}
		}

		public bool AllowUserToResizeRows {
			get { return allowUserToResizeRows; }
			set {
				if (allowUserToResizeRows != value) {
					allowUserToResizeRows = value;
					OnAllowUserToResizeRowsChanged(EventArgs.Empty);
				}
			}
		}

		public DataGridViewCellStyle AlternatingRowsDefaultCellStyle {
			get { return alternatingRowsDefaultCellStyle; }
			set {
				if (alternatingRowsDefaultCellStyle != value) {
					alternatingRowsDefaultCellStyle = value;
					OnAlternatingRowsDefaultCellStyleChanged(EventArgs.Empty);
					Invalidate();
				}
			}
		}

		public bool AutoGenerateColumns {
			get { return autoGenerateColumns; }
			set {
				if (autoGenerateColumns != value) {
					autoGenerateColumns = value;
					OnAutoGenerateColumnsChanged(EventArgs.Empty);
				}
			}
		}

		//public override bool AutoSize {
		public bool AutoSize {
			get { return autoSize; }
			set {
				if (autoSize != value) {
					autoSize = value;
					//OnAutoSizeChanged(EventArgs.Empty);
				}
			}
		}

		public DataGridViewAutoSizeColumnsMode AutoSizeColumnsMode {
			get { return autoSizeColumnsMode; }
			set {
				if (!Enum.IsDefined(typeof(DataGridViewAutoSizeColumnsMode), value)) {
					throw new InvalidEnumArgumentException("Value is not valid DataGridViewAutoSizeColumnsMode.");
				}
				if (value == DataGridViewAutoSizeColumnsMode.ColumnHeader && columnHeadersVisible == false) {
					foreach (DataGridViewColumn col in columns) {
						if (col.AutoSizeMode == DataGridViewAutoSizeColumnMode.NotSet) {
								throw new InvalidOperationException("Cant set this property to ColumnHeader in this DataGridView.");
						}
					}
				}
				if (value == DataGridViewAutoSizeColumnsMode.Fill) {
					foreach (DataGridViewColumn col in columns) {
						if (col.AutoSizeMode == DataGridViewAutoSizeColumnMode.NotSet) {
							if (col.Frozen) {
								throw new InvalidOperationException("Cant set this property to Fill in this DataGridView.");
							}
						}
					}
				}
				autoSizeColumnsMode = value;
			}
		}

		public DataGridViewAutoSizeRowsMode AutoSizeRowsMode {
			get { return autoSizeRowsMode; }
			set {
				if (autoSizeRowsMode != value) {
					if (!Enum.IsDefined(typeof(DataGridViewAutoSizeRowsMode), value)) {
						throw new InvalidEnumArgumentException("Value is not valid DataGridViewRowsMode.");
					}
					if ((value == DataGridViewAutoSizeRowsMode.AllHeaders || value == DataGridViewAutoSizeRowsMode.DisplayedHeaders) && rowHeadersVisible == false) {
						throw new InvalidOperationException("Cant set this property to AllHeaders or DisplayedHeaders in this DataGridView.");
					}
					autoSizeRowsMode = value;
					OnAutoSizeRowsModeChanged(new DataGridViewAutoSizeModeEventArgs(false));
					////////////////////////////////////////////////////////////////
				}
			}
		}

		public override Color BackColor {
			get { return backColor; }
			set {
				if (backColor != value) {
					backColor = value;
					OnBackColorChanged(EventArgs.Empty);
				}
			}
		}

		public Color BackgroundColor {
			get { return backgroundColor; }
			set {
				if (backgroundColor != value) {
					if (value == Color.Empty) {
						throw new ArgumentException("Cant set an Empty color.");
					}
					backgroundColor = value;
					OnBackgroundColorChanged(EventArgs.Empty);
				}
			}
		}

		public override Image BackgroundImage {
			get { return backgroundImage; }
			set {
				if (backgroundImage != value) {
					backgroundImage = value;
					OnBackgroundImageChanged(EventArgs.Empty);
				}
			}
		}

		//public override ImageLayout BackgroundImageLayout {
		public ImageLayout BackgroundImageLayout {
			get { return backgroundImageLayout; }
			set {
				if (backgroundImageLayout != value) {
					backgroundImageLayout = value;
					//OnBackgroundImageLayoutChanged(EventArg.Empty);
				}
			}
		}

		public BorderStyle BorderStyle {
			get { return borderStyle; }
			set {
				if (borderStyle != value) {
					if (!Enum.IsDefined(typeof(BorderStyle), value)) {
						throw new InvalidEnumArgumentException("Invalid border style.");
					}
					borderStyle = value;
					OnBorderStyleChanged(EventArgs.Empty);
				}
			}
		}

		public DataGridViewCellBorderStyle CellBorderStyle {
			get { return cellBorderStyle; }
			set {
				if (cellBorderStyle != value) {
					cellBorderStyle = value;
					OnCellBorderStyleChanged(EventArgs.Empty);
				}
			}
		}

		public DataGridViewClipboardCopyMode ClipboardCopyMode {
			get { return clipboardCopyMode; }
			set { clipboardCopyMode = value; }
		}

		public int ColumnCount {
			get { return columns.Count; }
			set {
				if (value < 0) {
					throw new ArgumentException("ColumnCount must be >= 0.");
				}
				if (dataSource != null) {
					throw new InvalidOperationException("Cant change column count if DataSource is set.");
				}
				if (value < columns.Count) {
					for (int i = value; i < columns.Count; i++) {
						columns.RemoveAt(i);
					}
				}
				else if (value > columns.Count) {
					for (int i = 0; i < value; i++) {
						DataGridViewColumn col = new DataGridViewColumn();
						columns.Add(col);
					}
				}
			}
		}

		public DataGridViewHeaderBorderStyle ColumnHeadersBorderStyle {
			get { return columnHeadersBorderStyle; }
			set {
				if (columnHeadersBorderStyle != value) {
					columnHeadersBorderStyle = value;
					OnColumnHeadersBorderStyleChanged(EventArgs.Empty);
				}
			}
		}

		public DataGridViewCellStyle ColumnHeadersDefaultCellStyle {
			get { return columnHeadersDefaultCellStyle; }
			set {
				if (columnHeadersDefaultCellStyle != value) {
					columnHeadersDefaultCellStyle = value;
					OnColumnHeadersDefaultCellStyleChanged(EventArgs.Empty);
				}
			}
		}

		public int ColumnHeadersHeight {
			get { return columnHeadersHeight; }
			set {
				if (columnHeadersHeight != value) {
					if (value < 4) {
						throw new ArgumentException("Column headers height cant be less than 4.");
					}
					columnHeadersHeight = value;
					OnColumnHeadersHeightChanged(EventArgs.Empty);
				}
			}
		}

		public DataGridViewColumnHeadersHeightSizeMode ColumnHeadersHeightSizeMode {
			get { return columnHeadersHeightSizeMode; }
			set {
				if (columnHeadersHeightSizeMode != value) {
					if (!Enum.IsDefined(typeof(DataGridViewColumnHeadersHeightSizeMode), value)) {
						throw new InvalidEnumArgumentException("Value is not a valid DataGridViewColumnHeadersHeightSizeMode.");
					}
					columnHeadersHeightSizeMode = value;
					OnColumnHeadersHeightSizeModeChanged(new DataGridViewAutoSizeModeEventArgs(false));
				}
			}
		}

		public bool ColumnHeadersVisible {
			get { return columnHeadersVisible; }
			set { columnHeadersVisible = value; }
		}

		public DataGridViewColumnCollection Columns {
			get { return columns; }
		}

		public DataGridViewCell CurrentCell {
			get { return currentCell; }
			set {
				/////////////////////////////////////////////////////
				/// *** InvalidOperationException ***
				/// Changes to the specified cell cannot be committed
				/// to the data cache, or the new cell is in a hidden
				/// row.
				/////////////////////////////////////////////////////
				if (value.DataGridView != this) {
					throw new ArgumentException("The cell is not in this DataGridView.");
				}
				currentCell = value;
			}
		}

		public Point CurrentCellAddress {
			get { return currentCellAddress; }
		}

		public DataGridViewRow CurrentRow {
			get { return currentRow; }
		}

		public string DataMember {
			get { return dataMember; }
			set {
				if (dataMember != value) {
					dataMember = value;
					OnDataMemberChanged(EventArgs.Empty);
				}
			}
		}

		public object DataSource {
			get { return dataSource; }
			set {
				if (dataSource != value) {
					/* The System.Windows.Forms.DataGridView class supports the standard Windows Forms data-binding model. This means the data source can be of any type that implements:
					 - the System.Collections.IList interface, including one-dimensional arrays.
					 - the System.ComponentModel.IListSource interface, such as the System.Data.DataTable and System.Data.DataSet classes.
					 - the System.ComponentModel.IBindingList interface, such as the System.ComponentModel.Collections.BindingList<> class.
					 - the System.ComponentModel.IBindingListView interface, such as the System.Windows.Forms.BindingSource class.
					*/
					if (!(value is IList) && !(value is IListSource) && !(value is IBindingList) && !(value is IBindingListView)) {
						throw new NotSupportedException("Type cant be binded.");
					}
					dataSource = value;
					OnDataSourceChanged(EventArgs.Empty);
					// DataBinding
					if (value is IList) {
						BindIList(value as IList);
					}
					else if (value is IListSource) {
						BindIListSource(value as IListSource);
					}
					else if (value is IBindingList) {
						BindIBindingList(value as IBindingList);
					}
					else if (value is IBindingListView) {
						BindIBindingListView(value as IBindingListView);
						//bool cosa = ((value as IBindingListView).SortDescriptions as IList).IsFixedSize;
					}
					OnDataBindingComplete(new DataGridViewBindingCompleteEventArgs(ListChangedType.Reset));
				}
			}
		}

		public DataGridViewCellStyle DefaultCellStyle {
			get { return defaultCellStyle; }
			set {
				if (defaultCellStyle != value) {
					defaultCellStyle = value;
					OnDefaultCellStyleChanged(EventArgs.Empty);
				}
			}
		}

		public override Rectangle DisplayRectangle {
			get { return base.DisplayRectangle; }
		}

		public Control EditingControl {
			get {
				if (currentCell == null || !currentCell.IsInEditMode) {
					return null;
				}
				return (Control) Activator.CreateInstance(currentCell.EditType);
			}
		}

		public Panel EditingPanel {
			get { throw new NotImplementedException(); }
		}

		public DataGridViewEditMode EditMode {
			get { return editMode; }
			set {
				if (editMode != value) {
					editMode = value;
					OnEditModeChanged(EventArgs.Empty);
				}
			}
		}

		public bool EnableHeadersVisualStyles {
			get { return enableHeadersVisualStyles; }
			set { enableHeadersVisualStyles = value; }
		}

		public DataGridViewCell FirstDisplayedCell {
			get { return firstDisplayedCell; }
			set {
				if (value.DataGridView != this) {
					throw new ArgumentException("The cell is not in this DataGridView.");
				}
				firstDisplayedCell = value;
			}
		}

		public int FirstDisplayedScrollingColumnHiddenWidth {
			get { return firstDisplayedScrollingColumnHiddenWidth; }
		}

		public int FirstDisplayedScrollingColumnIndex {
			get { return firstDisplayedScrollingColumnIndex; }
			set { firstDisplayedScrollingColumnIndex = value; }
		}

		public int FirstDisplayedScrollingRowIndex {
			get { return firstDisplayedScrollingRowIndex; }
			set { firstDisplayedScrollingRowIndex = value; }
		}

		public override Font Font {
			get { return font; }
			set {
				if (font != value) {
					font = value;
					OnFontChanged(EventArgs.Empty);
				}
			}
		}

		public override Color ForeColor {
			get { return foreColor; }
			set {
				if (foreColor != value) {
					foreColor = value;
					OnForeColorChanged(EventArgs.Empty);
				}
			}
		}

		public Color GridColor {
			get { return gridColor; }
			set {
				if (gridColor != value) {
					if (value == Color.Empty) {
						throw new ArgumentException("Cant set an Empty color.");
					}
					gridColor = value;
					OnGridColorChanged(EventArgs.Empty);
				}
			}
		}

		public int HorizontalScrollingOffset {
			get { return horizontalScrollingOffset; }
			set { horizontalScrollingOffset = value; }
		}

		public bool IsCurrentCellDirty {
			get { return isCurrentCellDirty; }
		}

		public bool IsCurrentCellInEditMode {
			get {
				if (currentCell == null) {
					return false;
				}
				return currentCell.IsInEditMode;
			}
		}

		public bool IsCurrentRowDirty {
			get {
				if (!virtualMode) {
					return IsCurrentCellDirty;
				}
				// Calcular
				throw new NotImplementedException();
			}
		}

		public DataGridViewCell this [int columnIndex, int rowIndex] {
			get { return rows[rowIndex].Cells[columnIndex]; }
			set { rows[rowIndex].Cells[columnIndex] = value; }
		}

		public DataGridViewCell this [string columnName, int rowIndex] {
			get {
				int columnIndex = -1;
				foreach (DataGridViewColumn col in columns) {
					if (col.Name == columnName) {
						columnIndex = col.Index;
						break;
					}
				}
				return this[columnIndex, rowIndex];
			}
			set {
				int columnIndex = -1;
				foreach (DataGridViewColumn col in columns) {
					if (col.Name == columnName) {
						columnIndex = col.Index;
						break;
					}
				}
				this[columnIndex, rowIndex] = value;
			}
		}

		public bool MultiSelect {
			get { return multiSelect; }
			set {
				if (multiSelect != value) {
					multiSelect = value;
					OnMultiSelectChanged(EventArgs.Empty);
				}
			}
		}

		public int NewRowIndex {
			get {
				if (!allowUserToAddRows) {
					return -1;
				}
				return rows.Count - 1;
			}
		}

		public new Padding Padding {
			get { return Padding.Empty; }
			set { }
		}

		public bool ReadOnly {
			get { return readOnly; }
			set {
				if (readOnly != value) {
					readOnly = value;
					OnReadOnlyChanged(EventArgs.Empty);
				}
			}
		}

		public int RowCount {
			get { return rows.Count; }
			set {
				if (value < 0) {
					throw new ArgumentException("RowCount must be >= 0.");
				}
				if (value < 1 && allowUserToAddRows) {
					throw new ArgumentException("RowCount must be >= 1 if AllowUserToAddRows is true.");
				}
				if (dataSource != null) {
					throw new InvalidOperationException("Cant change row count if DataSource is set.");
				}
				if (value < rows.Count) {
					for (int i = value; i < rows.Count; i++) {
						rows.RemoveAt(i);
					}
				}
				else if (value > rows.Count) {
					for (int i = 0; i < value; i++) {
						// DataGridViewRow row = new DataGridViewRow(); //(DataGridViewRow) rowTemplate.Clone();
						DataGridViewRow row = (DataGridViewRow) rowTemplate.Clone();
						rows.Add(row);
						foreach (DataGridViewColumn col in columns) {
							row.Cells.Add(col.CellTemplate.Clone() as DataGridViewCell);
						}
					}
				}
				if (ColumnCount == 0) {
					///////////////////////////////////////////////////////////////
					//columns.Add(new DataGridViewTextBoxColumn());
					throw new NotImplementedException();
				}
			}
		}

		public DataGridViewHeaderBorderStyle RowHeadersBorderStyle {
			get { return rowHeadersBorderStyle; }
			set {
				if (rowHeadersBorderStyle != value) {
					rowHeadersBorderStyle = value;
					OnRowHeadersBorderStyleChanged(EventArgs.Empty);
				}
			}
		}

		public DataGridViewCellStyle RowHeadersDefaultCellStyle {
			get { return rowHeadersDefaultCellStyle; }
			set {
				if (rowHeadersDefaultCellStyle != value) {
					rowHeadersDefaultCellStyle = value;
					OnRowHeadersDefaultCellStyleChanged(EventArgs.Empty);
				}
			}
		}

		public bool RowHeadersVisible {
			get { return rowHeadersVisible; }
			set { rowHeadersVisible = value; }
		}

		public int RowHeadersWidth {
			get { return rowHeadersWidth; }
			set {
				if (rowHeadersWidth != value) {
					if (value < 4) {
						throw new ArgumentException("RowHeadersWidth cant be less than 4.");
					}
					rowHeadersWidth = value;
					OnRowHeadersWidthChanged(EventArgs.Empty);
				}
			}
		}

		public DataGridViewRowHeadersWidthSizeMode RowHeadersWidthSizeMode {
			get { return rowHeadersWidthSizeMode; }
			set {
				if (rowHeadersWidthSizeMode != value) {
					if (!Enum.IsDefined(typeof(DataGridViewRowHeadersWidthSizeMode), value)) {
						throw new InvalidEnumArgumentException("Value is not valid DataGridViewRowHeadersWidthSizeMode.");
					}
					rowHeadersWidthSizeMode = value;
					OnRowHeadersWidthSizeModeChanged(new DataGridViewAutoSizeModeEventArgs(false));
				}
			}
		}

		public DataGridViewRowCollection Rows {
			get { return rows; }
		}

		public DataGridViewCellStyle RowsDefaultCellStyle {
			get { return rowsDefaultCellStyle; }
			set {
				if (rowsDefaultCellStyle != value) {
					rowsDefaultCellStyle = value;
					OnRowsDefaultCellStyleChanged(EventArgs.Empty);
				}
			}
		}

		public DataGridViewRow RowTemplate {
			get {
				if (rowTemplate == null) {
					return new DataGridViewRow();
				}
				return rowTemplate;
			}
			set {
				rowTemplate = value;
				rowTemplate.SetDataGridView(this);
			}
		}

		public ScrollBars ScrollBars {
			get { return scrollBars; }
			set {
				if (!Enum.IsDefined(typeof(ScrollBars), value)) {
					throw new InvalidEnumArgumentException("Invalid ScrollBars value.");
				}
				////////////////////////////////////////////////////////////
				/// *** InvalidOperationException ***
				/// The System.Windows.Forms.DataGridView is unable to
				/// scroll due to a cell change that cannot be committed
				/// or canceled.
				///////////////////////////////////////////////////////////
				scrollBars = value;
			}
		}

		public DataGridViewSelectedCellCollection SelectedCells {
			get {
				DataGridViewSelectedCellCollection selectedCells = new DataGridViewSelectedCellCollection();
				foreach (DataGridViewRow row in rows) {
					foreach (DataGridViewCell cell in row.Cells) {
						if (cell.Selected) {
							selectedCells.InternalAdd(cell);
						}
					}
				}
				return selectedCells;
			}
		}

		public DataGridViewSelectedColumnCollection SelectedColumns {
			get {
				DataGridViewSelectedColumnCollection selectedColumns = new DataGridViewSelectedColumnCollection();
				if (selectionMode == DataGridViewSelectionMode.FullColumnSelect || selectionMode == DataGridViewSelectionMode.ColumnHeaderSelect) {
					foreach (DataGridViewColumn col in columns) {
						if (col.Selected) {
							selectedColumns.InternalAdd(col);
						}
					}
				}
				return selectedColumns;
			}
		}

		public DataGridViewSelectedRowCollection SelectedRows {
			get {
				DataGridViewSelectedRowCollection selectedRows = new DataGridViewSelectedRowCollection();
				if (selectionMode == DataGridViewSelectionMode.FullColumnSelect || selectionMode == DataGridViewSelectionMode.RowHeaderSelect) {
					foreach (DataGridViewRow row in rows) {
						if (row.Selected) {
							selectedRows.InternalAdd(row);
						}
					}
				}
				return selectedRows;
			}
		}

		public DataGridViewSelectionMode SelectionMode {
			get { return selectionMode; }
			set {
				if (!Enum.IsDefined(typeof(DataGridViewSelectionMode), value)) {
					throw new InvalidEnumArgumentException("Value is not valid DataGridViewSelectionMode.");
				}
				selectionMode = value;
			}
		}

		public bool ShowCellErrors {
			get { return showCellErrors; }
			set { showCellErrors = value; }
		}

		public bool ShowCellToolTips {
			get { return showCellToolTips; }
			set { showCellToolTips = value; }
		}

		public bool ShowEditingIcon {
			get { return showEditingIcon; }
			set { showEditingIcon = value; }
		}

		public bool ShowRowErrors {
			get { return showRowErrors; }
			set { showRowErrors = value; }
		}

		public DataGridViewColumn SortedColumn {
			get { return sortedColumn; }
		}

		public SortOrder SortOrder {
			get { return sortOrder; }
		}

		public bool StandardTab {
			get { return standardTab; }
			set { standardTab = value; }
		}

		public override string Text {
			get { return text; }
			set { text = value; }
		}

		public DataGridViewHeaderCell TopLeftHeaderCell {
			get { return topLeftHeaderCell; }
			set { topLeftHeaderCell = value; }
		}

		public Cursor UserSetCursor {
			get { return userSetCursor; }
		}

		public int VerticalScrollingOffset {
			get { return verticalScrollingOffset; }
		}

		public bool VirtualMode {
			get { return virtualMode; }
			set { virtualMode = value; }
		}

		public event EventHandler AllowUserToAddRowsChanged;

		public event EventHandler AllowUserToDeleteRowsChanged;

		public event EventHandler AllowUserToOrderColumnsChanged;

		public event EventHandler AllowUserToResizeColumnsChanged;

		public event EventHandler AllowUserToResizeRowsChanged;

		public event EventHandler AlternatingRowsDefaultCellStyleChanged;

		public event EventHandler AutoGenerateColumnsChanged;

		public new event EventHandler AutoSizeChanged;

		public event DataGridViewAutoSizeColumnModeEventHandler AutoSizeColumnModeChanged;

		public event DataGridViewAutoSizeColumnsModeEventHandler AutoSizeColumnsModeChanged;

		public event DataGridViewAutoSizeModeEventHandler AutoSizeRowsModeChanged;

		public new event EventHandler BackColorChanged;

		public event EventHandler BackgroundColorChanged;

		public new event EventHandler BackgroundImageChanged;

		public new event EventHandler BackgroundImageLayoutChanged;

		public event EventHandler BorderStyleChanged;

		public event QuestionEventHandler CancelRowEdit;

		public event DataGridViewCellCancelEventHandler CellBeginEdit;

		public event EventHandler CellBorderStyleChanged;

		public event DataGridViewCellEventHandler CellClick;

		public event DataGridViewCellEventHandler CellContentClick;

		public event DataGridViewCellEventHandler CellContentDoubleClick;

		public event DataGridViewCellEventHandler CellContextMenuStripChanged;

		public event DataGridViewCellContextMenuStripNeededEventHandler CellContextMenuStripNeeded;

		public event DataGridViewCellEventHandler CellDoubleClick;

		public event DataGridViewCellEventHandler CellEndEdit;

		public event DataGridViewCellEventHandler CellEnter;

		public event DataGridViewCellEventHandler CellErrorTextChanged;

		public event DataGridViewCellErrorTextNeededEventHandler CellErrorTextNeeded;

		public event DataGridViewCellFormattingEventHandler CellFormatting;

		public event DataGridViewCellEventHandler CellLeave;

		public event DataGridViewCellMouseEventHandler CellMouseClick;

		public event DataGridViewCellMouseEventHandler CellMouseDoubleClick;

		public event DataGridViewCellMouseEventHandler CellMouseDown;

		public event DataGridViewCellEventHandler CellMouseEnter;

		public event DataGridViewCellEventHandler CellMouseLeave;

		public event DataGridViewCellMouseEventHandler CellMouseMove;

		public event DataGridViewCellMouseEventHandler CellMouseUp;

		public event DataGridViewCellPaintingEventHandler CellPainting;

		public event DataGridViewCellParsingEventHandler CellParsing;

		public event DataGridViewCellStateChangedEventHandler CellStateChanged;

		public event DataGridViewCellEventHandler CellStyleChanged;

		public event DataGridViewCellStyleContentChangedEventHandler CellStyleContentChanged;

		public event DataGridViewCellEventHandler CellToolTipTextChanged;

		public event DataGridViewCellToolTipTextNeededEventHandler CellToolTipTextNeeded;

		public event DataGridViewCellEventHandler CellValidated;

		public event DataGridViewCellValidatingEventHandler CellValidating;

		public event DataGridViewCellEventHandler CellValueChanged;

		public event DataGridViewCellValueEventHandler CellValueNeeded;

		public event DataGridViewCellValueEventHandler CellValuePushed;

		public event DataGridViewColumnEventHandler ColumnAdded;

		public event DataGridViewColumnEventHandler ColumnContextMenuStripChanged;

		public event DataGridViewColumnEventHandler ColumnDataPropertyNameChanged;

		public event DataGridViewColumnEventHandler ColumnDefaultCellStyleChanged;

		public event DataGridViewColumnEventHandler ColumnDisplayIndexChanged;

		public event DataGridViewColumnDividerDoubleClickEventHandler ColumnDividerDoubleClick;

		public event DataGridViewColumnEventHandler ColumnDividerWidthChanged;

		public event DataGridViewColumnEventHandler ColumnHeaderCellChanged;

		public event DataGridViewCellMouseEventHandler ColumnHeaderMouseClick;

		public event DataGridViewCellMouseEventHandler ColumnHeaderMouseDoubleClick;

		public event EventHandler ColumnHeadersBorderStyleChanged;

		public event EventHandler ColumnHeadersDefaultCellStyleChanged;

		public event EventHandler ColumnHeadersHeightChanged;

		public event DataGridViewAutoSizeModeEventHandler ColumnHeadersHeightSizeModeChanged;

		public event DataGridViewColumnEventHandler ColumnMinimumWidthChanged;

		public event DataGridViewColumnEventHandler ColumnNameChanged;

		public event DataGridViewColumnEventHandler ColumnRemoved;

		public event DataGridViewColumnEventHandler ColumnSortModeChanged;

		public event DataGridViewColumnStateChangedEventHandler ColumnStateChanged;

		public event DataGridViewColumnEventHandler ColumnToolTipTextChanged;

		public event DataGridViewColumnEventHandler ColumnWidthChanged;

		public event EventHandler CurrentCellChanged;

		public event EventHandler CurrentCellDirtyStateChanged;

		public event DataGridViewBindingCompleteEventHandler DataBindingComplete;

		public event DataGridViewDataErrorEventHandler DataError;

		public event EventHandler DataMemberChanged;

		public event EventHandler DataSourceChanged;

		public event EventHandler DefaultCellStyleChanged;

		public event DataGridViewRowEventHandler DefaultValuesNeeded;

		public event DataGridViewEditingControlShowingEventHandler EditingControlShowing;

		public event EventHandler EditModeChanged;

		public new event EventHandler FontChanged;

		public new event EventHandler ForeColorChanged;

		public event EventHandler GridColorChanged;

		public event EventHandler MultiSelectChanged;

		public event DataGridViewRowEventHandler NewRowNeeded;

		public event EventHandler ReadOnlyChanged;

		public event DataGridViewRowEventHandler RowContextMenuStripChanged;

		public event DataGridViewRowContextMenuStripNeededEventHandler RowContextMenuStripNeeded;

		public event DataGridViewRowEventHandler RowDefaultCellStyleChanged;

		public event QuestionEventHandler RowDirtyStateNeeded;

		public event DataGridViewRowDividerDoubleClickEventHandler RowDividerDoubleClick;

		public event DataGridViewRowEventHandler RowDividerHeightChanged;

		public event DataGridViewCellEventHandler RowEnter;

		public event DataGridViewRowEventHandler RowErrorTextChanged;

		public event DataGridViewRowErrorTextNeededEventHandler RowErrorTextNeeded;

		public event DataGridViewRowEventHandler RowHeaderCellChanged;

		public event DataGridViewCellMouseEventHandler RowHeaderMouseClick;

		public event DataGridViewCellMouseEventHandler RowHeaderMouseDoubleClick;

		public event EventHandler RowHeadersBorderStyleChanged;

		public event EventHandler RowHeadersDefaultCellStyleChanged;

		public event EventHandler RowHeadersWidthChanged;

		public event DataGridViewAutoSizeModeEventHandler RowHeadersWidthSizeModeChanged;

		public event DataGridViewRowEventHandler RowHeightChanged;

		public event DataGridViewRowHeightInfoNeededEventHandler RowHeightInfoNeeded;

		public event DataGridViewRowHeightInfoPushedEventHandler RowHeightInfoPushed;

		public event DataGridViewCellEventHandler RowLeave;

		public event DataGridViewRowEventHandler RowMinimumHeightChanged;

		public event DataGridViewRowPostPaintEventHandler RowPostPaint;

		public event DataGridViewRowPrePaintEventHandler RowPrePaint;

		public event DataGridViewRowsAddedEventHandler RowsAdded;

		public event EventHandler RowsDefaultCellStyleChanged;

		public event DataGridViewRowsRemovedEventHandler RowsRemoved;

		public event DataGridViewRowStateChangedEventHandler RowStateChanged;

		public event DataGridViewRowEventHandler RowUnshared;

		public event DataGridViewCellEventHandler RowValidated;

		public event DataGridViewCellCancelEventHandler RowValidating;

		public event ScrollEventHandler Scroll;

		public event EventHandler SelectionChanged;

		public event DataGridViewSortCompareEventHandler SortCompare;

		public event EventHandler Sorted;

		public event DataGridViewRowEventHandler UserAddedRow;

		public event DataGridViewRowEventHandler UserDeletedRow;

		public event DataGridViewRowCancelEventHandler UserDeletingRow;

		public new event EventHandler StyleChanged;

		public new event EventHandler TextChanged;

		public virtual DataGridViewAdvancedBorderStyle AdjustColumnHeaderBorderStyle (DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStyleInput, DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder, bool isFirstDisplayedColumn, bool isLastVisibleColumn) {
			return (DataGridViewAdvancedBorderStyle) dataGridViewAdvancedBorderStyleInput.Clone();
		}

		public bool AreAllCellsSelected (bool includeInvisibleCells) {
			foreach (DataGridViewRow row in rows) {
				foreach (DataGridViewCell cell in row.Cells) {
					if (includeInvisibleCells == false && cell.Visible == false) {
						continue;
					}
					if (!cell.Selected) {
						return false;
					}
				}
			}
			return true;
		}

		public void AutoResizeColumn (int columnIndex) {
			throw new NotImplementedException();
		}

		public void AutoResizeColumn (int columnIndex, DataGridViewAutoSizeColumnMode autoSizeColumnMode) {
			throw new NotImplementedException();
		}

		public void AutoResizeColumnHeadersHeight () {
			throw new NotImplementedException();
		}

		public void AutoResizeColumnHeadersHeight (int columnIndex) {
			throw new NotImplementedException();
		}

		public void AutoResizeColumns () {
			throw new NotImplementedException();
		}

		public void AutoResizeColumns (DataGridViewAutoSizeColumnsMode autoSizeColumnsMode) {
			throw new NotImplementedException();
		}

		public void AutoResizeRow (int rowIndex) {
			throw new NotImplementedException();
		}

		public void AutoResizeRow (int rowIndex, DataGridViewAutoSizeRowMode autoSizeRowMode) {
			throw new NotImplementedException();
		}

		public void AutoResizeRowHeadersWidth (DataGridViewRowHeadersWidthSizeMode rowHeadersWidthSizeMode) {
			throw new NotImplementedException();
		}

		public void AutoResizeRowHeadersWidth (int rowIndex, DataGridViewRowHeadersWidthSizeMode rowHeadersWidthSizeMode) {
			throw new NotImplementedException();
		}

		public void AutoResizeRows () {
			throw new NotImplementedException();
		}

		public void AutoResizeRows (DataGridViewAutoSizeRowsMode autoSizeRowsMode) {
			if (!Enum.IsDefined(typeof(DataGridViewAutoSizeRowsMode), autoSizeRowsMode)) {
				throw new InvalidEnumArgumentException("Parameter AutoSizeRowsMode is not valid DataGridViewRowsMode.");
			}
			if ((autoSizeRowsMode == DataGridViewAutoSizeRowsMode.AllHeaders || autoSizeRowsMode == DataGridViewAutoSizeRowsMode.DisplayedHeaders) && rowHeadersVisible == false) {
				throw new InvalidOperationException("Parameter AutoSizeRowsMode cant be AllHeaders or DisplayedHeaders in this DataGridView.");
			}
			if (autoSizeRowsMode == DataGridViewAutoSizeRowsMode.None) {
				throw new ArgumentException("Parameter AutoSieRowsMode cant be None.");
			}
		}

		public virtual bool BeginEdit (bool selectAll) {
			throw new NotImplementedException();
		}

		public bool CancelEdit () {
			throw new NotImplementedException();
		}

		public void ClearSelection () {
			foreach (DataGridViewCell cell in SelectedCells) {
				cell.Selected = false;
			}
		}

		public bool CommitEdit (DataGridViewDataErrorContexts context) {
			throw new NotImplementedException();
		}

		public int DisplayedColumnCount (bool includePartialColumns) {
			/////////////////////// PartialColumns?
			int result = 0;
			foreach (DataGridViewColumn col in columns) {
				if (col.Visible) {
					result++;
				}
			}
			return result;
		}

		public int DisplayedRowCount (bool includePartialRow) {
			/////////////////////// PartialRows?
			int result = 0;
			foreach (DataGridViewRow row in rows) {
				if (row.Visible) {
					result++;
				}
			}
			return result;
		}

		public bool EndEdit () {
			throw new NotImplementedException();
		}

		public bool EndEdit (DataGridViewDataErrorContexts context) {
			throw new NotImplementedException();
		}

		public int GetCellCount (DataGridViewElementStates includeFilter) {
			int result = 0;
			foreach (DataGridViewRow row in rows) {
				foreach (DataGridViewCell cell in row.Cells) {
					if ((cell.State & includeFilter) != 0) {
						result++;
					}
				}
			}
			return result;
		}

		public Rectangle GetCellDisplayRectangle (int columnIndex, int rowIndex, bool cutOverflow) {
			if (columnIndex < 0 || columnIndex >= columns.Count) {
				throw new ArgumentOutOfRangeException("Column index is out of range.");
			}
			throw new NotImplementedException();
		}

		public virtual DataObject GetClipboardContent () {
			throw new NotImplementedException();
		}

		public Rectangle GetColumnDisplayRectangle (int columnIndex, bool cutOverflow) {
			throw new NotImplementedException();
		}

		public Rectangle GetRowDisplayRectangle (int rowIndex, bool cutOverflow) {
			throw new NotImplementedException();
		}

		public HitTestInfo HitTest (int x, int y) {
			///////////////////////////////////////////////////////
			x += horizontalScrollingOffset;
			y += verticalScrollingOffset;
			int rowIndex = -1;
			int totalHeight = (columnHeadersVisible)? 1 + columnHeadersHeight : 1;
			if (columnHeadersVisible && y <= totalHeight) {
				rowIndex = -1;
			}
			else {
				foreach (DataGridViewRow row in rows.RowIndexSortedArrayList) {
					totalHeight += row.Height;
					if (y <= totalHeight) {
						rowIndex = row.Index;
						break;
					}
					totalHeight++; // sumar el ancho de las lineas...
				}
			}
			int colIndex = -1;
			int totalWidth = (rowHeadersVisible)? 1 + rowHeadersWidth : 1;
			if (rowHeadersVisible && x <= totalWidth) {
				colIndex = -1;
			}
			else {
				foreach (DataGridViewColumn col in columns.ColumnDisplayIndexSortedArrayList) {
					totalWidth += col.Width;
					if (x <= totalWidth) {
						colIndex = col.Index;
						break;
					}
					totalWidth++;
				}
			}
			HitTestInfo result = new HitTestInfo(colIndex, x, rowIndex, y, (colIndex >= 0 && rowIndex >= 0)? DataGridViewHitTestType.Cell : DataGridViewHitTestType.None);
			return result;
		}

		public void InvalidateCell (DataGridViewCell dataGridViewCell) {
			if (dataGridViewCell == null) {
				throw new ArgumentNullException("Cell is null");
			}
			if (dataGridViewCell.DataGridView != this) {
				throw new ArgumentException("The specified cell does not belong to this DataGridView.");
			}
			throw new NotImplementedException();
		}

		public void InvalidateCell (int columnIndex, int rowIndex) {
			if (columnIndex < 0 || columnIndex >= columns.Count) {
				throw new ArgumentOutOfRangeException("Column index is out of range.");
			}
			if (rowIndex < 0 || rowIndex >= rows.Count) {
				throw new ArgumentOutOfRangeException("Row index is out of range.");
			}
			foreach (DataGridViewRow row in rows) {
				foreach (DataGridViewCell cell in row.Cells) {
					if (cell.RowIndex == rowIndex && cell.ColumnIndex == columnIndex) {
						InvalidateCell(cell); //// O al revés, que el otro llame a este !!!
						return;
					}
				}
			}
		}

		public void InvalidateColumn (int columnIndex) {
			if (columnIndex < 0 || columnIndex >= columns.Count) {
				throw new ArgumentOutOfRangeException("Column index is out of range.");
			}
			throw new NotImplementedException();
		}

		public void InvalidateRow (int rowIndex) {
			if (rowIndex < 0 || rowIndex >= rows.Count) {
				throw new ArgumentOutOfRangeException("Row index is out of range.");
			}
			throw new NotImplementedException();
		}

		public virtual void NotifyCurrentCellDirty (bool dirty) {
			throw new NotImplementedException();
		}

		public bool RefreshEdit () {
			throw new NotImplementedException();
		}

		public override void ResetText () {
			throw new NotImplementedException();
		}

		public void SelectAll () {
			switch (selectionMode) {
				case DataGridViewSelectionMode.FullRowSelect:
					foreach (DataGridViewRow row in rows) {
						(row as DataGridViewBand).Selected = true;
					}
					break;
				case DataGridViewSelectionMode.FullColumnSelect:
					foreach (DataGridViewColumn col in columns) {
						(col as DataGridViewBand).Selected = true;
					}
					break;
				default:
					foreach (DataGridViewRow row in rows) {
						foreach (DataGridViewCell cell in row.Cells) {
							cell.Selected = true;
						}
					}
					break;
			}
		}

		public virtual void Sort (IComparer comparer) {
			throw new NotImplementedException();
		}

		public virtual void Sort (DataGridViewColumn dataGridViewColumn, ListSortDirection direction) {
			throw new NotImplementedException();
		}

		public void UpdateCellErrorText (int columnIndex, int rowIndex) {
			throw new NotImplementedException();
		}

		public void UpdateRowErrorText (int rowIndex) {
			throw new NotImplementedException();
		}

		public void UpdateRowErrorText (int rowIndexStart, int rowIndexEnd) {
			throw new NotImplementedException();
		}

		public void UpdateRowHeightInfo (int rowIndex, bool updateToEnd) {
			throw new NotImplementedException();
		}

		protected override Size DefaultSize {
			get { return defaultSize; }
		}

		protected ScrollBar HorizontalScrollBar {
			get { return horizontalScrollBar; }
		}

		protected ScrollBar VerticalScrollBar {
			get { return verticalScrollBar; }
		}

		protected void AutoResizeColumn (int columnIndex, DataGridViewAutoSizeColumnMode autoSizeColumnMode, bool fixedHeight) {
			throw new NotImplementedException();
		}

		protected void AutoResizeColumnHeadersHeight (bool fixedRowHeadersWidth, bool fixedColumnsWidth) {
			throw new NotImplementedException();
		}

		protected void AutoResizeColumnHeadersHeight (int columnIndex, bool fixedRowHeadersWidth, bool fixedColumnWidth) {
			throw new NotImplementedException();
		}

		protected void AutoResizeColumns (DataGridViewAutoSizeColumnsMode autoSizeColumnsMode, bool fixedHeight) {
			throw new NotImplementedException();
		}

		protected void AutoResizeRow (int rowIndex, DataGridViewAutoSizeRowMode autoSizeRowMode, bool fixedWidth) {
			throw new NotImplementedException();
		}

		protected void AutoResizeRowHeadersWidth (DataGridViewRowHeadersWidthSizeMode rowHeadersWidthSizeMode, bool fixedColumnHeadersHeight, bool fixedRowsHeight) {
			throw new NotImplementedException();
		}

		protected void AutoResizeRowHeadersWidth (int rowIndex, DataGridViewRowHeadersWidthSizeMode rowHeadersWidthSizeMode, bool fixedColumnHeadersHeight, bool fixedRowHeight) {
			throw new NotImplementedException();
		}

		protected void AutoResizeRows (DataGridViewAutoSizeRowsMode autoSizeRowsMode, bool fixedWidth) {
			throw new NotImplementedException();
		}

		protected void AutoResizeRows (int rowIndexStart, int rowsCount, DataGridViewAutoSizeRowMode autoSizeRowMode, bool fixedWidth) {
			throw new NotImplementedException();
		}

		protected void ClearSelection (int columnIndexException, int rowIndexException, bool selectExceptionElement) {
			if (columnIndexException >= columns.Count) {
				throw new ArgumentOutOfRangeException("ColumnIndexException is greater than the highest column index.");
			}
			if (selectionMode == DataGridViewSelectionMode.FullRowSelect) {
				if (columnIndexException < -1) {
					throw new ArgumentOutOfRangeException("ColumnIndexException is less than -1.");
				}
			}
			else {
				if (columnIndexException < 0) {
					throw new ArgumentOutOfRangeException("ColumnIndexException is less than 0.");
				}
			}
			if (rowIndexException >= rows.Count) {
				throw new ArgumentOutOfRangeException("RowIndexException is greater than the highest row index.");
			}
			if (selectionMode == DataGridViewSelectionMode.FullColumnSelect) {
				if (rowIndexException < -1) {
					throw new ArgumentOutOfRangeException("RowIndexException is less than -1.");
				}
			}
			else {
				if (rowIndexException < 0) {
					throw new ArgumentOutOfRangeException("RowIndexException is less than 0.");
				}
			}
			switch (selectionMode) {
				case DataGridViewSelectionMode.FullRowSelect:
					foreach (DataGridViewRow row in rows) {
						if (selectExceptionElement && row.Index == rowIndexException) {
							continue;
						}
						row.Selected = false;
					}
					break;
				case DataGridViewSelectionMode.FullColumnSelect:
					foreach (DataGridViewColumn col in columns) {
						if (selectExceptionElement && col.Index == columnIndexException) {
							continue;
						}
						col.Selected = false;
					}
					break;
				default:
					foreach (DataGridViewCell cell in SelectedCells) {
						if (selectExceptionElement && cell.RowIndex == rowIndexException && cell.ColumnIndex == columnIndexException) {
							continue;
						}
						cell.Selected = false;
					}
					break;
			}
		}

		protected override AccessibleObject CreateAccessibilityInstance () {
			return new DataGridViewAccessibleObject(this);
		}

		protected virtual DataGridViewColumnCollection CreateColumnsInstance () {
			return new DataGridViewColumnCollection(this);
		}

		protected override Control.ControlCollection CreateControlsInstance () {
			return base.CreateControlsInstance(); //new Control.ControlCollection(this);
		}

		protected virtual DataGridViewRowCollection CreateRowsInstance () {
			return new DataGridViewRowCollection(this);
		}

		protected override void Dispose (bool disposing) {
		}

		//protected override AccessibleObject GetAccessibilityObjectById (int objectId) {
		protected AccessibleObject GetAccessibilityObjectById (int objectId) {
			throw new NotImplementedException();
		}

		protected override bool IsInputChar (char charCode) {
			return base.IsInputChar(charCode);
			//throw new NotImplementedException();
		}

		protected override bool IsInputKey (Keys keyData) {
			return base.IsInputKey(keyData);
			//throw new NotImplementedException();
		}

		protected virtual void OnAllowUserToAddRowsChanged (EventArgs e) {
			if (AllowUserToAddRowsChanged != null) {
				AllowUserToAddRowsChanged(this, e);
			}
		}

		protected virtual void OnAllowUserToDeleteRowsChanged (EventArgs e) {
			if (AllowUserToDeleteRowsChanged != null) {
				AllowUserToDeleteRowsChanged(this, e);
			}
		}

		protected virtual void OnAllowUserToOrderColumnsChanged (EventArgs e) {
			if (AllowUserToOrderColumnsChanged != null) {
				AllowUserToOrderColumnsChanged(this, e);
			}
		}

		protected virtual void OnAllowUserToResizeColumnsChanged (EventArgs e) {
			if (AllowUserToResizeColumnsChanged != null) {
				AllowUserToResizeColumnsChanged(this, e);
			}
		}

		protected virtual void OnAllowUserToResizeRowsChanged (EventArgs e) {
			if (AllowUserToResizeRowsChanged != null) {
				AllowUserToResizeRowsChanged(this, e);
			}
		}

		protected virtual void OnAlternatingRowsDefaultCellStyleChanged (EventArgs e) {
			if (AlternatingRowsDefaultCellStyleChanged != null) {
				AlternatingRowsDefaultCellStyleChanged(this, e);
			}
		}

		protected virtual void OnAutoGenerateColumnsChanged (EventArgs e) {
			if (AutoGenerateColumnsChanged != null) {
				AutoGenerateColumnsChanged(this, e);
			}
		}

		protected virtual void OnAutoSizeColumnModeChanged (DataGridViewAutoSizeColumnModeEventArgs e) {
			if (AutoSizeColumnModeChanged != null) {
				AutoSizeColumnModeChanged(this, e);
			}
		}

		protected virtual void OnAutoSizeColumnsModeChanged (DataGridViewAutoSizeColumnsModeEventArgs e) {
			if (AutoSizeColumnsModeChanged != null) {
				AutoSizeColumnsModeChanged(this, e);
			}
		}

		protected virtual void OnAutoSizeRowsModeChanged (DataGridViewAutoSizeModeEventArgs e) {
			if (AutoSizeRowsModeChanged != null) {
				AutoSizeRowsModeChanged(this, e);
			}
		}

		protected virtual void OnBackgroundColorChanged (EventArgs e) {
			if (BackgroundColorChanged != null) {
				BackgroundColorChanged(this, e);
			}
		}

		protected override void OnBindingContextChanged (EventArgs e) {
			base.OnBindingContextChanged(e);
		}

		protected virtual void OnBorderStyleChanged (EventArgs e) {
			if (BorderStyleChanged != null) {
				BorderStyleChanged(this, e);
			}
		}

		protected virtual void OnCancelRowEdit (QuestionEventArgs e) {
			if (CancelRowEdit != null) {
				CancelRowEdit(this, e);
			}
		}

		protected virtual void OnCellBeginEdit (DataGridViewCellCancelEventArgs e) {
			if (CellBeginEdit != null) {
				CellBeginEdit(this, e);
			}
		}

		protected virtual void OnCellBorderStyleChanged (EventArgs e) {
			if (CellBorderStyleChanged != null) {
				CellBorderStyleChanged(this, e);
			}
		}

		protected virtual void OnCellClick (DataGridViewCellEventArgs e) {
			if (CellClick != null) {
				CellClick(this, e);
			}
		}

		protected virtual void OnCellContentClick (DataGridViewCellEventArgs e) {
			if (CellContentClick != null) {
				CellContentClick(this, e);
			}
		}

		protected virtual void OnCellContentDoubleClick (DataGridViewCellEventArgs e) {
			if (CellContentDoubleClick != null) {
				CellContentDoubleClick(this, e);
			}
		}

		protected virtual void OnCellContextMenuStripChanged (DataGridViewCellEventArgs e) {
			if (CellContextMenuStripChanged != null) {
				CellContextMenuStripChanged(this, e);
			}
		}

		protected virtual void OnCellContextMenuStripNeeded (DataGridViewCellContextMenuStripNeededEventArgs e) {
			if (CellContextMenuStripNeeded != null) {
				CellContextMenuStripNeeded(this, e);
			}
		}

		protected virtual void OnCellDoubleClick (DataGridViewCellEventArgs e) {
			if (CellDoubleClick != null) {
				CellDoubleClick(this, e);
			}
		}

		protected virtual void OnCellEndEdit (DataGridViewCellEventArgs e) {
			if (CellEndEdit != null) {
				CellEndEdit(this, e);
			}
		}

		protected virtual void OnCellEnter (DataGridViewCellEventArgs e) {
			if (CellEnter != null) {
				CellEnter(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnCellErrorTextChanged (DataGridViewCellEventArgs e) {
			if (CellErrorTextChanged != null) {
				CellErrorTextChanged(this, e);
			}
		}

		protected virtual void OnCellErrorTextNeeded (DataGridViewCellErrorTextNeededEventArgs e) {
			if (CellErrorTextNeeded != null) {
				CellErrorTextNeeded(this, e);
			}
		}

		protected virtual void OnCellFormatting (DataGridViewCellFormattingEventArgs e) {
			if (CellFormatting != null) {
				CellFormatting(this, e);
			}
		}

		protected virtual void OnCellLeave (DataGridViewCellEventArgs e) {
			if (CellLeave != null) {
				CellLeave(this, e);
			}
		}

		protected virtual void OnCellMouseClick (DataGridViewCellMouseEventArgs e) {
			if (CellMouseClick != null) {
				CellMouseClick(this, e);
			}
		}

		protected virtual void OnCellMouseDoubleClick (DataGridViewCellMouseEventArgs e) {
			if (CellMouseDoubleClick != null) {
				CellMouseDoubleClick(this, e);
			}
		}

		protected virtual void OnCellMouseDown (DataGridViewCellMouseEventArgs e) {
			if (CellMouseDown != null) {
				CellMouseDown(this, e);
			}
		}

		protected virtual void OnCellMouseEnter (DataGridViewCellEventArgs e) {
			if (CellMouseEnter != null) {
				CellMouseEnter(this, e);
			}
		}

		protected virtual void OnCellMouseLeave (DataGridViewCellEventArgs e) {
			if (CellMouseLeave != null) {
				CellMouseLeave(this, e);
			}
		}

		protected virtual void OnCellMouseMove (DataGridViewCellMouseEventArgs e) {
			if (CellMouseMove != null) {
				CellMouseMove(this, e);
			}
		}

		protected virtual void OnCellMouseUp (DataGridViewCellMouseEventArgs e) {
			if (CellMouseUp != null) {
				CellMouseUp(this, e);
			}
		}

		protected virtual void OnCellPainting (DataGridViewCellPaintingEventArgs e) {
			if (CellPainting != null) {
				CellPainting(this, e);
			}
		}

		protected internal virtual void OnCellParsing (DataGridViewCellParsingEventArgs e) {
			if (CellParsing != null) {
				CellParsing(this, e);
			}
		}

		protected virtual void OnCellStateChanged (DataGridViewCellStateChangedEventArgs e) {
			if (CellStateChanged != null) {
				CellStateChanged(this, e);
			}
		}

		protected virtual void OnCellStyleChanged (DataGridViewCellEventArgs e) {
			if (CellStyleChanged != null) {
				CellStyleChanged(this, e);
			}
		}

		protected virtual void OnCellStyleContentChanged (DataGridViewCellStyleContentChangedEventArgs e) {
			if (CellStyleContentChanged != null) {
				CellStyleContentChanged(this, e);
			}
		}

		protected virtual void OnCellToolTipTextChanged (DataGridViewCellEventArgs e) {
			if (CellToolTipTextChanged != null) {
				CellToolTipTextChanged(this, e);
			}
		}

		protected virtual void OnCellToolTipTextNeeded (DataGridViewCellToolTipTextNeededEventArgs e) {
			if (CellToolTipTextNeeded != null) {
				CellToolTipTextNeeded(this, e);
			}
		}

		protected virtual void OnCellValidated (DataGridViewCellEventArgs e) {
			if (CellValidated != null) {
				CellValidated(this, e);
			}
		}

		protected virtual void OnCellValidating (DataGridViewCellValidatingEventArgs e) {
			if (CellValidating != null) {
				CellValidating(this, e);
			}
		}

		protected virtual void OnCellValueChanged (DataGridViewCellEventArgs e) {
			if (CellValueChanged != null) {
				CellValueChanged(this, e);
			}
		}

		protected virtual void OnCellValueNeeded (DataGridViewCellValueEventArgs e) {
			if (CellValueNeeded != null) {
				CellValueNeeded(this, e);
			}
		}

		protected virtual void OnCellValuePushed (DataGridViewCellValueEventArgs e) {
			if (CellValuePushed != null) {
				CellValuePushed(this, e);
			}
		}

		protected virtual void OnColumnAdded (DataGridViewColumnEventArgs e) {
			if (ColumnAdded != null) {
				ColumnAdded(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnContextMenuStripChanged (DataGridViewColumnEventArgs e) {
			if (ColumnContextMenuStripChanged != null) {
				ColumnContextMenuStripChanged(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnDataPropertyNameChanged (DataGridViewColumnEventArgs e) {
			if (ColumnDataPropertyNameChanged != null) {
				ColumnDataPropertyNameChanged(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnDefaultCellStyleChanged (DataGridViewColumnEventArgs e) {
			if (ColumnDefaultCellStyleChanged != null) {
				ColumnDefaultCellStyleChanged(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnDisplayIndexChanged (DataGridViewColumnEventArgs e) {
			if (ColumnDisplayIndexChanged != null) {
				ColumnDisplayIndexChanged(this, e);
			}
		}

		protected virtual void OnColumnDividerDoubleClick (DataGridViewColumnDividerDoubleClickEventArgs e) {
			if (ColumnDividerDoubleClick != null) {
				ColumnDividerDoubleClick(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnDividerWidthChanged (DataGridViewColumnEventArgs e) {
			if (ColumnDividerWidthChanged != null) {
				ColumnDividerWidthChanged(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnHeaderCellChanged (DataGridViewColumnEventArgs e) {
			if (ColumnHeaderCellChanged != null) {
				ColumnHeaderCellChanged(this, e);
			}
		}

		protected virtual void OnColumnHeaderMouseClick (DataGridViewCellMouseEventArgs e) {
			if (ColumnHeaderMouseClick != null) {
				ColumnHeaderMouseClick(this, e);
			}
		}

		protected virtual void OnColumnHeaderMouseDoubleClick (DataGridViewCellMouseEventArgs e) {
			if (ColumnHeaderMouseDoubleClick != null) {
				ColumnHeaderMouseDoubleClick(this, e);
			}
		}

		protected virtual void OnColumnHeadersBorderStyleChanged (EventArgs e) {
			if (ColumnHeadersBorderStyleChanged != null) {
				ColumnHeadersBorderStyleChanged(this, e);
			}
		}

		protected virtual void OnColumnHeadersDefaultCellStyleChanged (EventArgs e) {
			if (ColumnHeadersDefaultCellStyleChanged != null) {
				ColumnHeadersDefaultCellStyleChanged(this, e);
			}
		}

		protected virtual void OnColumnHeadersHeightChanged (EventArgs e) {
			if (ColumnHeadersHeightChanged != null) {
				ColumnHeadersHeightChanged(this, e);
			}
		}

		protected virtual void OnColumnHeadersHeightSizeModeChanged (DataGridViewAutoSizeModeEventArgs e) {
			if (ColumnHeadersHeightSizeModeChanged != null) {
				ColumnHeadersHeightSizeModeChanged(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnMinimumWidthChanged (DataGridViewColumnEventArgs e) {
			if (ColumnMinimumWidthChanged != null) {
				ColumnMinimumWidthChanged(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnNameChanged (DataGridViewColumnEventArgs e) {
			if (ColumnNameChanged != null) {
				ColumnNameChanged(this, e);
			}
		}

		protected virtual void OnColumnRemoved (DataGridViewColumnEventArgs e) {
			if (ColumnRemoved != null) {
				ColumnRemoved(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnSortModeChanged (DataGridViewColumnEventArgs e) {
			if (ColumnSortModeChanged != null) {
				ColumnSortModeChanged(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnStateChanged (DataGridViewColumnStateChangedEventArgs e) {
			if (ColumnStateChanged != null) {
				ColumnStateChanged(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnToolTipTextChanged (DataGridViewColumnEventArgs e) {
			if (ColumnToolTipTextChanged != null) {
				ColumnToolTipTextChanged(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnColumnWidthChanged (DataGridViewColumnEventArgs e) {
			if (ColumnWidthChanged != null) {
				ColumnWidthChanged(this, e);
			}
		}

		protected virtual void OnCurrentCellChanged (EventArgs e) {
			if (CurrentCellChanged != null) {
				CurrentCellChanged(this, e);
			}
		}

		protected virtual void OnCurrentCellDirtyStateChanged (EventArgs e) {
			if (CurrentCellDirtyStateChanged != null) {
				CurrentCellDirtyStateChanged(this, e);
			}
		}

		protected virtual void OnDataBindingComplete (DataGridViewBindingCompleteEventArgs e) {
			if (DataBindingComplete != null) {
				DataBindingComplete(this, e);
			}
		}

		protected virtual void OnDataError (bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e) {
			if (DataError != null) {
				DataError(this, e);
			}
			else {
				if (displayErrorDialogIfNoHandler) {
					/////////////////////////////////// ERROR DIALOG //////////////////////////////////7
				}
			}
		}
		protected virtual void OnDataMemberChanged (EventArgs e) {
			if (DataMemberChanged != null) {
				DataMemberChanged(this, e);
			}
		}

		protected virtual void OnDataSourceChanged (EventArgs e) {
			if (DataSourceChanged != null) {
				DataSourceChanged(this, e);
			}
		}

		protected virtual void OnDefaultCellStyleChanged (EventArgs e) {
			if (DefaultCellStyleChanged != null) {
				DefaultCellStyleChanged(this, e);
			}
		}

		protected virtual void OnDefaultValuesNeeded (DataGridViewRowEventArgs e) {
			if (DefaultValuesNeeded != null) {
				DefaultValuesNeeded(this, e);
			}
		}

		protected override void OnDoubleClick (EventArgs e) {
			base.OnDoubleClick(e);
		}

		protected virtual void OnEditingControlShowing (DataGridViewEditingControlShowingEventArgs e) {
			if (EditingControlShowing != null) {
				EditingControlShowing(this, e);
			}
		}

		protected virtual void OnEditModeChanged (EventArgs e) {
			if (EditModeChanged != null) {
				EditModeChanged(this, e);
			}
		}

		protected override void OnEnabledChanged (EventArgs e) {
			base.OnEnabledChanged(e);
		}

		protected override void OnEnter (EventArgs e ) {
			base.OnEnter(e);
		}

		protected override void OnFontChanged (EventArgs e) {
			base.OnFontChanged(e);
			if (FontChanged != null) {
				FontChanged(this, e);
			}
		}

		protected override void OnForeColorChanged (EventArgs e) {
			base.OnForeColorChanged(e);
			if (ForeColorChanged != null) {
				ForeColorChanged(this, e);
			}
		}

		protected virtual void OnGridColorChanged (EventArgs e) {
			if (GridColorChanged != null) {
				GridColorChanged(this, e);
			}
		}

		protected override void OnHandleCreated (EventArgs e) {
			base.OnHandleCreated(e);
		}

		protected override void OnKeyDown (KeyEventArgs e) {
			base.OnKeyDown(e);
		}

		protected override void OnKeyPress (KeyPressEventArgs e) {
			base.OnKeyPress(e);
		}

		protected override void OnKeyUp (KeyEventArgs e) {
			base.OnKeyUp(e);
		}

		protected override void OnLayout (LayoutEventArgs e) {
			base.OnLayout(e);
		}

		protected override void OnLeave (EventArgs e) {
			base.OnLeave(e);
		}

		//protected override void OnMouseClick (MouseEventArgs e) {
		protected void OnMouseClick (MouseEventArgs e) {
			//base.OnMouseClick(e);
			//Console.WriteLine("Mouse: Clicks: {0}; Delta: {1}; X: {2}; Y: {3};", e.Clicks, e.Delta, e.X, e.Y);
		}

		//protected override void OnMouseDoubleClick (MouseEventArgs e) {
		protected void OnMouseDoubleClick (MouseEventArgs e) {
			//base.OnMouseDoubleClick(e);
		}

		protected override void OnMouseDown (MouseEventArgs e) {
			base.OnMouseDown(e);
			//Console.WriteLine("Mouse: Clicks: {0}; Delta: {1}; X: {2}; Y: {3};", e.Clicks, e.Delta, e.X, e.Y);
			HitTestInfo hitTest = HitTest(e.X, e.Y);
			//Console.WriteLine("HitTest: Column: {0}; Row: {1};", hitTest.ColumnIndex, hitTest.RowIndex);
			if (hitTest.RowIndex < 0 || hitTest.ColumnIndex < 0) {
				return;
			}
			OnCellClick(new DataGridViewCellEventArgs(hitTest.ColumnIndex, hitTest.RowIndex));
			DataGridViewRow row = rows[hitTest.RowIndex];
			DataGridViewCell cell = row.Cells[hitTest.ColumnIndex];
			ClearSelection(0, 0, false);
			switch (selectionMode) {
				case DataGridViewSelectionMode.FullRowSelect:
					row.Selected = true;
					break;
				case DataGridViewSelectionMode.FullColumnSelect:
					//////////////////
					break;
				default:
					cell.Selected = true;
					break;
			}
			if (cell == currentCell) {
				currentCell.SetIsInEditMode(true);
				OnCellBeginEdit(new DataGridViewCellCancelEventArgs(currentCell.ColumnIndex, currentCell.RowIndex));
				Invalidate();
				return;
			}
			if (currentCell != null) {
				if (currentCell.IsInEditMode) {
					currentCell.SetIsInEditMode(false);
					currentCell.DetachEditingControl();
					OnCellEndEdit(new DataGridViewCellEventArgs(currentCell.ColumnIndex, currentCell.RowIndex));
				}
				OnCellLeave(new DataGridViewCellEventArgs(currentCell.ColumnIndex, currentCell.RowIndex));
			}
			currentCell = cell;
			OnCurrentCellChanged(EventArgs.Empty);
			OnCellEnter(new DataGridViewCellEventArgs(cell.ColumnIndex, cell.RowIndex));
			if (editMode == DataGridViewEditMode.EditOnEnter) {
				currentCell.SetIsInEditMode(true);
				OnCellBeginEdit(new DataGridViewCellCancelEventArgs(currentCell.ColumnIndex, currentCell.RowIndex));
			}
			Invalidate();
			return;
		}

		protected override void OnMouseEnter (EventArgs e) {
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave (EventArgs e) {
			base.OnMouseLeave(e);
		}

		protected override void OnMouseMove (MouseEventArgs e) {
			base.OnMouseMove(e);
		}

		protected override void OnMouseUp (MouseEventArgs e) {
			base.OnMouseUp(e);
		}

		protected override void OnMouseWheel (MouseEventArgs e) {
			base.OnMouseWheel(e);
		}

		protected virtual void OnMultiSelectChanged (EventArgs e) {
			if (MultiSelectChanged != null) {
				MultiSelectChanged(this, e);
			}
		}

		protected virtual void OnNewRowNeeded (DataGridViewRowEventArgs e) {
			if (NewRowNeeded != null) {
				NewRowNeeded(this, e);
			}
		}

		protected override void OnPaint (PaintEventArgs e) {
			base.OnPaint(e);
			Console.WriteLine(e.ClipRectangle);
			Rectangle bounds = ClientRectangle; //e.ClipRectangle;
			e.Graphics.FillRectangle(new SolidBrush(backgroundColor), bounds);
			Pen pen = new Pen(gridColor);
			pen.Width = 1;
			int i = 0;
			ArrayList sortedColumns = columns.ColumnDisplayIndexSortedArrayList;
			bounds.Y = -verticalScrollingOffset;
			bounds.X = -horizontalScrollingOffset;
			gridWidth = 0;
			foreach (DataGridViewColumn col in sortedColumns) {
				gridWidth += col.Width;
			}
			if (columnHeadersVisible) {
				Rectangle headerBounds = bounds;
				if (rowHeadersVisible) {
					headerBounds.X += rowHeadersWidth;
				}
				headerBounds.Height = columnHeadersHeight;
				int j = 0;
				foreach (DataGridViewColumn col in sortedColumns) {
					headerBounds.Width = col.Width;
					DataGridViewCell cell = col.HeaderCell;
					DataGridViewCellStyle style = columnHeadersDefaultCellStyle;
					DataGridViewAdvancedBorderStyle intermediateBorderStyle = (DataGridViewAdvancedBorderStyle) this.AdvancedColumnHeadersBorderStyle.Clone();;
					DataGridViewAdvancedBorderStyle borderStyle = AdjustColumnHeaderBorderStyle(this.AdvancedColumnHeadersBorderStyle, intermediateBorderStyle, j == 0, j == columns.Count - 1);
					cell.InternalPaint(e.Graphics, e.ClipRectangle, headerBounds, cell.RowIndex, cell.State, cell.Value, cell.FormattedValue, cell.ErrorText, style, borderStyle, DataGridViewPaintParts.All);
					headerBounds.X += col.Width;
					j++;
				}
				bounds.Y += columnHeadersHeight;
			}
			gridHeight = 0;
			foreach (DataGridViewRow row in rows) {
				gridHeight += row.Height;
				if (rowHeadersVisible) {
					Rectangle rowHeaderBounds = bounds;
					rowHeaderBounds.Height = row.Height;
					rowHeaderBounds.Width = rowHeadersWidth;
					DataGridViewCell cell = row.HeaderCell;
					DataGridViewCellStyle style = rowHeadersDefaultCellStyle;
					DataGridViewAdvancedBorderStyle intermediateBorderStyle = (DataGridViewAdvancedBorderStyle) this.AdvancedRowHeadersBorderStyle.Clone();;
					DataGridViewAdvancedBorderStyle borderStyle = cell.AdjustCellBorderStyle(this.AdvancedRowHeadersBorderStyle, intermediateBorderStyle, true, true, false, cell.RowIndex == 0);
					cell.InternalPaint(e.Graphics, e.ClipRectangle, rowHeaderBounds, cell.RowIndex, cell.State, cell.Value, cell.FormattedValue, cell.ErrorText, style, borderStyle, DataGridViewPaintParts.All);
					//e.Graphics.FillRectangle(new SolidBrush(rowHeadersDefaultCellStyle.BackColor), rowHeadersBounds);
					bounds.X += rowHeadersWidth;
				}
				bounds.Height = row.Height;
				for (int j = 0; j < sortedColumns.Count; j++) {
					DataGridViewColumn col = (DataGridViewColumn) sortedColumns[j];
					foreach (DataGridViewCell cell in row.Cells) {
						if (cell.ColumnIndex == col.Index) {
							bounds.Width = col.Width;
							cell.SetSize(new Size(bounds.Width, bounds.Height));
							DataGridViewCellStyle style = cell.InheritedStyle;
							if (cell == currentCell && cell.IsInEditMode) {
								cell.InitializeEditingControl(cell.RowIndex, cell.FormattedValue, style);
								cell.PositionEditingControl(true, true, bounds, e.ClipRectangle, style, false, false, (columns[currentCell.ColumnIndex].DisplayIndex == 0), (currentCell.RowIndex == 0));
							}
							else {
								DataGridViewAdvancedBorderStyle intermediateBorderStyle = (DataGridViewAdvancedBorderStyle) this.AdvancedCellBorderStyle.Clone();;
								DataGridViewAdvancedBorderStyle borderStyle = cell.AdjustCellBorderStyle(this.AdvancedCellBorderStyle, intermediateBorderStyle, true, true, j == 0, cell.RowIndex == 0);
								OnCellFormatting(new DataGridViewCellFormattingEventArgs(cell.ColumnIndex, cell.RowIndex, cell.Value, cell.FormattedValueType, style));
								DataGridViewCellPaintingEventArgs args = new DataGridViewCellPaintingEventArgs (this, e.Graphics, e.ClipRectangle, bounds, cell.RowIndex, cell.ColumnIndex, cell.State, cell.Value, cell.FormattedValue, cell.ErrorText, style, borderStyle, DataGridViewPaintParts.All);
								OnCellPainting(args);
								if (!args.Handled) {
									cell.InternalPaint(e.Graphics, e.ClipRectangle, bounds, cell.RowIndex, cell.State, cell.Value, cell.FormattedValue, cell.ErrorText, style, borderStyle, DataGridViewPaintParts.All);
								}
							}
							bounds.X += bounds.Width;
						}
					}
				}
				bounds.Y += bounds.Height;
				bounds.X = -horizontalScrollingOffset;
				i++;
			}
			if (rowHeadersVisible) {
				gridWidth += rowHeadersWidth;
			}
			if (columnHeadersVisible) {
				gridHeight += columnHeadersHeight;
			}
			horizontalScrollBar.Visible = false;
			verticalScrollBar.Visible = false;
			if (AutoSize) {
				if (gridWidth > Size.Width || gridHeight > Size.Height) {
					Size = new Size(gridWidth, gridHeight);
				}
			}
			else {
				if (gridWidth > Size.Width) {
					horizontalScrollBar.Visible = true;
				}
				if (gridHeight > Size.Height) {
					verticalScrollBar.Visible = true;
				}
				if (horizontalScrollBar.Visible && (gridHeight + horizontalScrollBar.Height) > Size.Height) {
					verticalScrollBar.Visible = true;
				}
				if (verticalScrollBar.Visible && (gridWidth + horizontalScrollBar.Width) > Size.Width) {
					horizontalScrollBar.Visible = true;
				}
				if (horizontalScrollBar.Visible) {
					horizontalScrollBar.Minimum = 0;
					if (verticalScrollBar.Visible) {
						horizontalScrollBar.Maximum = gridWidth - ClientRectangle.Width + verticalScrollBar.Width;
					}
					else {
						horizontalScrollBar.Maximum = gridWidth - ClientRectangle.Width;
					}
					horizontalScrollBar.LargeChange = horizontalScrollBar.Maximum / 10;
					horizontalScrollBar.SmallChange = horizontalScrollBar.Maximum / 20;
				}
				if (verticalScrollBar.Visible) {
					verticalScrollBar.Minimum = 0;
					if (horizontalScrollBar.Visible) {
						verticalScrollBar.Maximum = gridHeight - ClientRectangle.Height + horizontalScrollBar.Height;
					}
					else {
						verticalScrollBar.Maximum = gridHeight - ClientRectangle.Height;
					}
					verticalScrollBar.LargeChange = verticalScrollBar.Maximum / 10;
					verticalScrollBar.SmallChange = verticalScrollBar.Maximum / 20;
				}
			}
			if (horizontalScrollBar.Visible && !this.Controls.Contains(horizontalScrollBar)) {
				this.Controls.Add(horizontalScrollBar);
			}
			else if (!horizontalScrollBar.Visible && this.Controls.Contains(horizontalScrollBar)) {
				this.Controls.Remove(horizontalScrollBar);
			}
			if (verticalScrollBar.Visible && !this.Controls.Contains(verticalScrollBar)) {
				this.Controls.Add(verticalScrollBar);
			}
			else if (!verticalScrollBar.Visible && this.Controls.Contains(verticalScrollBar)) {
				this.Controls.Remove(verticalScrollBar);
			}
		}

		protected virtual void OnReadOnlyChanged (EventArgs e) {
			if (ReadOnlyChanged != null) {
				ReadOnlyChanged(this, e);
			}
		}

		protected override void OnResize (EventArgs e) {
			base.OnResize(e);
			horizontalScrollingOffset = ((gridWidth - Size.Width) > 0)? (gridWidth - Size.Width) : 0;
			verticalScrollingOffset = ((gridHeight - Size.Height) > 0)? (gridHeight - Size.Height) : 0;

		}

		protected override void OnRightToLeftChanged (EventArgs e) {
			base.OnRightToLeftChanged(e);
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnRowContextMenuStripChanged (DataGridViewRowEventArgs e) {
			if (RowContextMenuStripChanged != null) {
				RowContextMenuStripChanged(this, e);
			}
		}

		protected virtual void OnRowContextMenuStripNeeded (DataGridViewRowContextMenuStripNeededEventArgs e) {
			if (RowContextMenuStripNeeded != null) {
				RowContextMenuStripNeeded(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnRowDefaultCellStyleChanged (DataGridViewRowEventArgs e) {
			if (RowDefaultCellStyleChanged != null) {
				RowDefaultCellStyleChanged(this, e);
			}
		}

		protected virtual void OnRowDirtyStateNeeded (QuestionEventArgs e) {
			if (RowDirtyStateNeeded != null) {
				RowDirtyStateNeeded(this, e);
			}
		}

		protected virtual void OnRowDividerDoubleClick (DataGridViewRowDividerDoubleClickEventArgs e) {
			if (RowDividerDoubleClick != null) {
				RowDividerDoubleClick(this, e);
			}
		}

		protected virtual void OnRowDividerHeightChanged (DataGridViewRowEventArgs e) {
			if (RowDividerHeightChanged != null) {
				RowDividerHeightChanged(this, e);
			}
		}

		protected virtual void OnRowEnter (DataGridViewCellEventArgs e) {
			if (RowEnter != null) {
				RowEnter(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnRowErrorTextChanged (DataGridViewRowEventArgs e) {
			if (RowErrorTextChanged != null) {
				RowErrorTextChanged(this, e);
			}
		}

		protected virtual void OnRowErrorTextNeeded (DataGridViewRowErrorTextNeededEventArgs e) {
			if (RowErrorTextNeeded != null) {
				RowErrorTextNeeded(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnRowHeaderCellChanged (DataGridViewRowEventArgs e) {
			if (RowHeaderCellChanged != null) {
				RowHeaderCellChanged(this, e);
			}
		}

		protected virtual void OnRowHeaderMouseClick (DataGridViewCellMouseEventArgs e) {
			if (RowHeaderMouseClick != null) {
				RowHeaderMouseClick(this, e);
			}
		}

		protected virtual void OnRowHeaderMouseDoubleClick (DataGridViewCellMouseEventArgs e) {
			if (RowHeaderMouseDoubleClick != null) {
				RowHeaderMouseDoubleClick(this, e);
			}
		}

		protected virtual void OnRowHeadersBorderStyleChanged (EventArgs e) {
			if (RowHeadersBorderStyleChanged != null) {
				RowHeadersBorderStyleChanged(this, e);
			}
		}

		protected virtual void OnRowHeadersDefaultCellStyleChanged (EventArgs e) {
			if (RowHeadersDefaultCellStyleChanged != null) {
				RowHeadersDefaultCellStyleChanged(this, e);
			}
		}

		protected virtual void OnRowHeadersWidthChanged (EventArgs e) {
			if (RowHeadersWidthChanged != null) {
				RowHeadersWidthChanged(this, e);
			}
		}

		protected virtual void OnRowHeadersWidthSizeModeChanged (DataGridViewAutoSizeModeEventArgs e) {
			if (RowHeadersWidthSizeModeChanged != null) {
				RowHeadersWidthSizeModeChanged(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnRowHeightChanged (DataGridViewRowEventArgs e) {
			if (RowHeightChanged != null) {
				RowHeightChanged(this, e);
			}
		}

		protected virtual void OnRowHeightInfoNeeded (DataGridViewRowHeightInfoNeededEventArgs e) {
			if (RowHeightInfoNeeded != null) {
				RowHeightInfoNeeded(this, e);
			}
		}

		protected virtual void OnRowHeightInfoPushed (DataGridViewRowHeightInfoPushedEventArgs e) {
			if (RowHeightInfoPushed != null) {
				RowHeightInfoPushed(this, e);
			}
		}

		protected virtual void OnRowLeave (DataGridViewCellEventArgs e) {
			if (RowLeave != null) {
				RowLeave(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnRowMinimumHeightChanged (DataGridViewRowEventArgs e) {
			if (RowMinimumHeightChanged != null) {
				RowMinimumHeightChanged(this, e);
			}
		}

		protected internal virtual void OnRowPostPaint (DataGridViewRowPostPaintEventArgs e) {
			if (RowPostPaint != null) {
				RowPostPaint(this, e);
			}
		}

		protected internal virtual void OnRowPrePaint (DataGridViewRowPrePaintEventArgs e) {
			if (RowPrePaint != null) {
				RowPrePaint(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnRowsAdded (DataGridViewRowsAddedEventArgs e) {
			if (RowsAdded != null) {
				RowsAdded(this, e);
			}
		}

		protected virtual void OnRowsDefaultCellStyleChanged (EventArgs e) {
			if (RowsDefaultCellStyleChanged != null) {
				RowsDefaultCellStyleChanged(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnRowsRemoved (DataGridViewRowsRemovedEventArgs e) {
			if (RowsRemoved != null) {
				RowsRemoved(this, e);
			}
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnRowStateChanged (int rowIndex, DataGridViewRowStateChangedEventArgs e) {
			if (RowStateChanged != null) {
				RowStateChanged(this, e);
			}
		}

		protected virtual void OnRowUnshared (DataGridViewRowEventArgs e) {
			if (RowUnshared != null) {
				RowUnshared(this, e);
			}
		}

		protected virtual void OnRowValidated (DataGridViewCellEventArgs e) {
			if (RowValidated != null) {
				RowValidated(this, e);
			}
		}

		protected virtual void OnRowValidating (DataGridViewCellCancelEventArgs e) {
			if (RowValidating != null) {
				RowValidating(this, e);
			}
		}

		protected virtual void OnScroll (ScrollEventArgs e) {
			if (Scroll != null) {
				Scroll(this, e);
			}
		}

		protected virtual void OnSelectionChanged (EventArgs e) {
			if (SelectionChanged != null) {
				SelectionChanged(this, e);
			}
		}

		protected virtual void OnSortCompare (DataGridViewSortCompareEventArgs e) {
			if (SortCompare != null) {
				SortCompare(this, e);
			}
		}

		protected virtual void OnSorted (EventArgs e) {
			if (Sorted != null) {
				Sorted(this, e);
			}
		}

		protected virtual void OnUserAddedRow (DataGridViewRowEventArgs e) {
			if (UserAddedRow != null) {
				UserAddedRow(this, e);
			}
		}

		protected virtual void OnUserDeletedRow (DataGridViewRowEventArgs e) {
			if (UserDeletedRow != null) {
				UserDeletedRow(this, e);
			}
		}

		protected virtual void OnUserDeletingRow (DataGridViewRowCancelEventArgs e) {
			if (UserDeletingRow != null) {
				UserDeletingRow(this, e);
			}
		}

		protected override void OnValidating (CancelEventArgs e) {
			base.OnValidating(e);
		}

		protected override void OnVisibleChanged (EventArgs e) {
			base.OnVisibleChanged(e);
		}

		protected virtual void PaintBackground (Graphics graphics, Rectangle clipBounds, Rectangle gridBounds) {
		}

		protected bool ProcessAKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessDataGridViewKey (KeyEventArgs e) {
			throw new NotImplementedException();
		}

		protected bool ProcessDeleteKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected override bool ProcessDialogKey (Keys keyData) {
			return base.ProcessDialogKey(keyData);
			//throw new NotImplementedException();
		}

		protected bool ProcessDownKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessEndKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessEnterKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessEscapeKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessF2Key (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessHomeKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessInsertKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected override bool ProcessKeyEventArgs (ref Message m) {
			return base.ProcessKeyEventArgs(ref m);
			//throw new NotImplementedException();
		}

		protected override bool ProcessKeyPreview (ref Message m) {
			return base.ProcessKeyPreview(ref m);
			//throw new NotImplementedException();
		}

		protected bool ProcessLeftKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessNextKey (Keys keyData) {
			// PAGE DOWN
			throw new NotImplementedException();
		}

		protected bool ProcessPriorKey (Keys keyData) {
			// PAGE UP
			throw new NotImplementedException();
		}

		protected bool ProcessRightKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessSpaceKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessTabKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessUpKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected bool ProcessZeroKey (Keys keyData) {
			throw new NotImplementedException();
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified) {
			base.SetBoundsCore(x, y, width, height, specified);
		}

		protected virtual bool SetCurrentCellAddressCore (int columnIndex, int rowIndex, bool setAnchorCellAddress, bool validateCurrentCell, bool throughMouseClick) {
			throw new NotImplementedException();
		}

		protected virtual void SetSelectedCellCore (int columnIndex, int rowIndex, bool selected) {
			throw new NotImplementedException();
		}

		protected virtual void SetSelectedColumnCore (int columnIndex, bool selected) {
			throw new NotImplementedException();
		}

		protected virtual void SetSelectedRowCore (int rowIndex, bool selected) {
			throw new NotImplementedException();
		}

		protected override void WndProc (ref Message m) {
			base.WndProc(ref m);
		}

		internal void InternalOnCellClick (DataGridViewCellEventArgs e) {
			OnCellClick(e);
		}

		internal void InternalOnCellContentClick (DataGridViewCellEventArgs e) {
			OnCellContentClick(e);
		}

		internal void InternalOnCellContentDoubleClick (DataGridViewCellEventArgs e) {
			OnCellContentDoubleClick(e);
		}

		internal void InternalOnCellValueChanged (DataGridViewCellEventArgs e) {
			OnCellValueChanged(e);
		}

		internal void InternalOnDataError (DataGridViewDataErrorEventArgs e) {
			/////////////// false? ////////////
			OnDataError(false, e);
		}

		internal void InternalOnMouseWheel (MouseEventArgs e) {
			OnMouseWheel(e);
		}

		internal void OnHScrollBarScroll (object sender, ScrollEventArgs e) {
			horizontalScrollingOffset = e.NewValue;
			Invalidate();
			OnScroll(e);
		}

		internal void OnVScrollBarScroll (object sender, ScrollEventArgs e) {
			verticalScrollingOffset = e.NewValue;
			Invalidate();
			OnScroll(e);
		}

		internal void RaiseCellStyleChanged (DataGridViewCellEventArgs e) {
			OnCellStyleChanged(e);
		}

		internal void OnColumnCollectionChanged (object sender, CollectionChangeEventArgs e) {
			switch (e.Action) {
				case CollectionChangeAction.Add:
					OnColumnAdded(new DataGridViewColumnEventArgs(e.Element as DataGridViewColumn));
					break;
				case CollectionChangeAction.Remove:
					OnColumnRemoved(new DataGridViewColumnEventArgs(e.Element as DataGridViewColumn));
					break;
				case CollectionChangeAction.Refresh:
					break;
			}
		}

		private void BindIList (IList list) {
			if (list.Count > 0) {
				DataGridViewCell template = new DataGridViewTextBoxCell();
				foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(list[0])) {
					DataGridViewColumn col = new DataGridViewColumn(template);
					col.Name = property.DisplayName;
					columns.Add(col);
				}
			}
			foreach (object element in list) {
				DataGridViewRow row = new DataGridViewRow();
				rows.InternalAdd(row);
				PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(element);
				foreach (PropertyDescriptor property in properties) {
					DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
					cell.Value = property.GetValue(element);
					cell.ValueType = property.PropertyType;
					row.Cells.Add(cell);
				}
			}
		}

		private void BindIListSource (IListSource list) {
			BindIList(list.GetList());
		}

		private void BindIBindingList (IBindingList list) {
			BindIList(list);
		}

		private void BindIBindingListView (IBindingListView list) {
			BindIList(list);
		}

		public sealed class HitTestInfo {

			public static readonly HitTestInfo Nowhere = new HitTestInfo(-1, -1, -1, -1, DataGridViewHitTestType.None);

			private int columnIndex;
			private int columnX;
			private int rowIndex;
			private int rowY;
			private DataGridViewHitTestType type;

			internal HitTestInfo (int columnIndex, int columnX, int rowIndex, int rowY, DataGridViewHitTestType type) {
				this.columnIndex = columnIndex;
				this.columnX = columnX;
				this.rowIndex = rowIndex;
				this.rowY = rowY;
				this.type = type;
			}

			public int ColumnIndex {
				get { return columnIndex; }
			}

			public int ColumnX {
				get { return columnX; }
			}

			public int RowIndex {
				get { return rowIndex; }
			}

			public int RowY {
				get { return rowY; }
			}

			public DataGridViewHitTestType Type {
				get { return type; }
			}

			public override bool Equals (object value) {
				if (value is HitTestInfo) {
					HitTestInfo aux = (HitTestInfo) value;
					if (aux.columnIndex == columnIndex && aux.columnX == columnX && aux.rowIndex == rowIndex && aux.rowY == rowY && aux.type == type) {
						return true;
					}
				}
				return false;
			}

			public override int GetHashCode () {
				return base.GetHashCode();
			}

			public override string ToString () {
				return GetType().Name;
			}

		}

		[ComVisibleAttribute(true)]
		protected class DataGridViewAccessibleObject : ControlAccessibleObject {

			public DataGridViewAccessibleObject (DataGridView owner) : base (owner){
			}

			public override AccessibleRole Role {
				get { return base.Role; }
			}

			public override AccessibleObject GetChild (int index) {
				return base.GetChild(index);
			}

			public override int GetChildCount () {
				return base.GetChildCount();
			}

			public override AccessibleObject GetFocused () {
				return base.GetFocused();
			}

			public override AccessibleObject GetSelected () {
				return base.GetSelected();
			}

			public override AccessibleObject HitTest (int x, int y) {
				return base.HitTest(x, y);
			}

			public override AccessibleObject Navigate( AccessibleNavigation navigationDirection) {
				return base.Navigate(navigationDirection);
			}

		}

	}

}

#endif
