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
using System.Reflection;
using System.Collections;
using System.Data;

namespace System.Windows.Forms {

	[ComVisibleAttribute(true)]
	[ClassInterfaceAttribute(ClassInterfaceType.AutoDispatch)]
	[Designer("System.Windows.Forms.Design.DataGridViewDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[Editor("System.Windows.Forms.Design.DataGridViewComponentEditor, " + Consts.AssemblySystem_Design, typeof (System.ComponentModel.ComponentEditor))]
	[ComplexBindingProperties ("DataSource", "DataMember")]
	[DefaultEvent ("CellContentClick")]
	[Docking (DockingBehavior.Ask)]
	public class DataGridView : Control, ISupportInitialize, IBindableComponent, IDropTarget
	{

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
		//private Control editingControl;
		private DataGridViewEditMode editMode;
		private bool enableHeadersVisualStyles;
		private DataGridViewCell firstDisplayedCell;
		private int firstDisplayedScrollingColumnHiddenWidth;
		private int firstDisplayedScrollingColumnIndex;
		private int firstDisplayedScrollingRowIndex;
		private Color gridColor = Color.FromKnownColor(KnownColor.ControlDark);
		private int horizontalScrollingOffset;
		private bool isCurrentCellDirty;
		//private bool isCurrentRowDirty;
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
		private DataGridViewHeaderCell topLeftHeaderCell;
		private Cursor userSetCursor;
		private int verticalScrollingOffset;
		private bool virtualMode;
		private HScrollBar horizontalScrollBar;
		private VScrollBar verticalScrollBar;
		private Control editingControl;

		// These are used to implement selection behaviour with SHIFT pressed.
		private int selected_row = -1;
		private int selected_column = -1;
		
		private DataGridViewSelectedRowCollection selected_rows;
		private DataGridViewSelectedColumnCollection selected_columns;
		
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
			defaultCellStyle = new DataGridViewCellStyle();
			defaultCellStyle.BackColor = SystemColors.Window;
			defaultCellStyle.ForeColor = SystemColors.ControlText;
			defaultCellStyle.SelectionBackColor = SystemColors.Highlight;
			defaultCellStyle.SelectionForeColor = SystemColors.HighlightText;
			defaultCellStyle.Font = this.Font;
			defaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
			defaultCellStyle.WrapMode = DataGridViewTriState.False;
			editMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
			multiSelect = true;
			readOnly = false;
			rowHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
			rowHeadersDefaultCellStyle = (DataGridViewCellStyle) columnHeadersDefaultCellStyle.Clone ();
			rowHeadersVisible = true;
			rowHeadersWidth = 41;
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
			Controls.Add (horizontalScrollBar);
			verticalScrollBar = new VScrollBar();
			verticalScrollBar.Dock = DockStyle.Right;
			verticalScrollBar.Scroll += OnVScrollBarScroll;
			verticalScrollBar.Visible = false;
			Controls.Add (verticalScrollBar);
		}

		void ISupportInitialize.BeginInit ()
		{
		}

		void ISupportInitialize.EndInit ()
		{
		}

		// Propiedades

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual DataGridViewAdvancedBorderStyle AdjustedTopLeftHeaderBorderStyle {
			get { return adjustedTopLeftHeaderBorderStyle; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DataGridViewAdvancedBorderStyle AdvancedCellBorderStyle {
			get { return advancedCellBorderStyle; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DataGridViewAdvancedBorderStyle AdvancedColumnHeadersBorderStyle {
			get { return advancedColumnHeadersBorderStyle; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public DataGridViewAdvancedBorderStyle AdvancedRowHeadersBorderStyle {
			get { return advancedRowHeadersBorderStyle; }
		}

		[DefaultValue (true)]
		public bool AllowUserToAddRows {
			get { return allowUserToAddRows; }
			set {
				if (allowUserToAddRows != value) {
					allowUserToAddRows = value;
					OnAllowUserToAddRowsChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue (true)]
		public bool AllowUserToDeleteRows {
			get { return allowUserToDeleteRows; }
			set {
				if (allowUserToDeleteRows != value) {
					allowUserToDeleteRows = value;
					OnAllowUserToDeleteRowsChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue (false)]
		public bool AllowUserToOrderColumns {
			get { return allowUserToOrderColumns; }
			set {
				if (allowUserToOrderColumns != value) {
					allowUserToOrderColumns = value;
					OnAllowUserToOrderColumnsChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue (true)]
		public bool AllowUserToResizeColumns {
			get { return allowUserToResizeColumns; }
			set {
				if (allowUserToResizeColumns != value) {
					allowUserToResizeColumns = value;
					OnAllowUserToResizeColumnsChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue (true)]
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

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DefaultValue (true)]
		public bool AutoGenerateColumns {
			get { return autoGenerateColumns; }
			set {
				if (autoGenerateColumns != value) {
					autoGenerateColumns = value;
					OnAutoGenerateColumnsChanged(EventArgs.Empty);
				}
			}
		}

		public override bool AutoSize {
			get { return autoSize; }
			set {
				if (autoSize != value) {
					autoSize = value;
					//OnAutoSizeChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue (DataGridViewAutoSizeColumnsMode.None)]
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

		[DefaultValue (DataGridViewAutoSizeRowsMode.None)]
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

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return backgroundImage; }
			set {
				if (backgroundImage != value) {
					backgroundImage = value;
					OnBackgroundImageChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		[DefaultValue (BorderStyle.FixedSingle)]
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

		internal Size BorderWidth {
			get {
				switch (BorderStyle) {
				case BorderStyle.Fixed3D:
					return ThemeEngine.Current.Border3DSize;
				case BorderStyle.FixedSingle:
					return ThemeEngine.Current.BorderSize;
				case BorderStyle.None:
					return Size.Empty;
				default:
					return Size.Empty;
				}
			}
		}

		[Browsable (true)]
		[DefaultValue (DataGridViewCellBorderStyle.Single)]
		public DataGridViewCellBorderStyle CellBorderStyle {
			get { return cellBorderStyle; }
			set {
				if (cellBorderStyle != value) {
					cellBorderStyle = value;
					OnCellBorderStyleChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable (true)]
		[DefaultValue (DataGridViewClipboardCopyMode.EnableWithAutoHeaderText)]
		public DataGridViewClipboardCopyMode ClipboardCopyMode {
			get { return clipboardCopyMode; }
			set { clipboardCopyMode = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[DefaultValue (0)]
		public int ColumnCount {
			get { return columns.Count; }
			set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException("ColumnCount", 
						"ColumnCount must be >= 0.");
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

		[Browsable (true)]
		[DefaultValue (DataGridViewHeaderBorderStyle.Raised)]
		public DataGridViewHeaderBorderStyle ColumnHeadersBorderStyle {
			get { return columnHeadersBorderStyle; }
			set {
				if (columnHeadersBorderStyle != value) {
					columnHeadersBorderStyle = value;
					OnColumnHeadersBorderStyleChanged(EventArgs.Empty);
				}
			}
		}

		[AmbientValue (null)]
		public DataGridViewCellStyle ColumnHeadersDefaultCellStyle {
			get { return columnHeadersDefaultCellStyle; }
			set {
				if (columnHeadersDefaultCellStyle != value) {
					columnHeadersDefaultCellStyle = value;
					OnColumnHeadersDefaultCellStyleChanged(EventArgs.Empty);
				}
			}
		}

		[Localizable (true)]
		public int ColumnHeadersHeight {
			get { return columnHeadersHeight; }
			set {
				if (columnHeadersHeight != value) {
					if (value < 4) {
						throw new ArgumentOutOfRangeException("ColumnHeadersHeight", 
							"Column headers height cant be less than 4.");
					}
					if (value > 32768 ) {
						throw new ArgumentOutOfRangeException("ColumnHeadersHeight", 
							"Column headers height cannot be more than 32768.");
					}
					columnHeadersHeight = value;
					OnColumnHeadersHeightChanged(EventArgs.Empty);
				}
			}
		}

		[RefreshProperties (RefreshProperties.All)]
		[DefaultValue (DataGridViewColumnHeadersHeightSizeMode.EnableResizing)]
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

		[DefaultValue (true)]
		public bool ColumnHeadersVisible {
			get { return columnHeadersVisible; }
			set { columnHeadersVisible = value; }
		}

		[MergableProperty (false)]
		[Editor ("System.Windows.Forms.Design.DataGridViewColumnCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataGridViewColumnCollection Columns {
			get { return columns; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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

		[Browsable (false)]
		public Point CurrentCellAddress {
			get { return currentCellAddress; }
		}

		[Browsable (false)]
		public DataGridViewRow CurrentRow {
			get { return currentRow; }
		}

		[DefaultValue ("")]
		[Editor ("System.Windows.Forms.Design.DataMemberListEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string DataMember {
			get { return dataMember; }
			set {
				if (dataMember != value) {
					dataMember = value;
					OnDataMemberChanged(EventArgs.Empty);
				}
			}
		}

		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (null)]
		[AttributeProvider (typeof (IListSource))]
		// XXX AttributeProviderAtribute
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
					if (!(value == null || value is IList || value is IListSource || value is IBindingList || value is IBindingListView)) {
						throw new NotSupportedException("Type cant be binded.");
					}
					if (dataSource != null) {
						columns.Clear();
						rows.Clear();
						if (dataSource is DataView) {
							(dataSource as DataView).ListChanged -= OnListChanged;
						}
						if (dataSource is DataTable) {
							((dataSource as IListSource).GetList() as DataView).ListChanged -= OnListChanged;
						}
					}
					dataSource = value;
					OnDataSourceChanged(EventArgs.Empty);
					if (dataSource != null) {
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
					Invalidate();
				}
			}
		}

		[AmbientValue (null)]
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

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public Control EditingControl {
			get {
				return editingControl;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public Panel EditingPanel {
			get { throw new NotImplementedException(); }
		}

		[DefaultValue (DataGridViewEditMode.EditOnKeystrokeOrF2)]
		public DataGridViewEditMode EditMode {
			get { return editMode; }
			set {
				if (editMode != value) {
					editMode = value;
					OnEditModeChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue (true)]
		public bool EnableHeadersVisualStyles {
			get { return enableHeadersVisualStyles; }
			set { enableHeadersVisualStyles = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataGridViewCell FirstDisplayedCell {
			get { return firstDisplayedCell; }
			set {
				if (value.DataGridView != this) {
					throw new ArgumentException("The cell is not in this DataGridView.");
				}
				firstDisplayedCell = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public int FirstDisplayedScrollingColumnHiddenWidth {
			get { return firstDisplayedScrollingColumnHiddenWidth; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int FirstDisplayedScrollingColumnIndex {
			get { return firstDisplayedScrollingColumnIndex; }
			set { firstDisplayedScrollingColumnIndex = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int FirstDisplayedScrollingRowIndex {
			get { return firstDisplayedScrollingRowIndex; }
			set { firstDisplayedScrollingRowIndex = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public override Font Font {
			get { return base.Font; }
			set { base.Font = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
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

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int HorizontalScrollingOffset {
			get { return horizontalScrollingOffset; }
			set { horizontalScrollingOffset = value; }
		}

		[Browsable (false)]
		public bool IsCurrentCellDirty {
			get { return isCurrentCellDirty; }
		}

		[Browsable (false)]
		public bool IsCurrentCellInEditMode {
			get {
				if (currentCell == null) {
					return false;
				}
				return currentCell.IsInEditMode;
			}
		}

		[Browsable (false)]
		public bool IsCurrentRowDirty {
			get {
				if (!virtualMode) {
					return IsCurrentCellDirty;
				}
				// Calcular
				throw new NotImplementedException();
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataGridViewCell this [int columnIndex, int rowIndex] {
			get { return rows[rowIndex].Cells[columnIndex]; }
			set { rows[rowIndex].Cells[columnIndex] = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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

		[DefaultValue (true)]
		public bool MultiSelect {
			get { return multiSelect; }
			set {
				if (multiSelect != value) {
					multiSelect = value;
					OnMultiSelectChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int NewRowIndex {
			get {
				if (!allowUserToAddRows) {
					return -1;
				}
				return rows.Count - 1;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Padding Padding {
			get { return Padding.Empty; }
			set { }
		}

		[Browsable (true)]
		[DefaultValue (false)]
		public bool ReadOnly {
			get { return readOnly; }
			set {
				if (readOnly != value) {
					readOnly = value;
					OnReadOnlyChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[DefaultValue (0)]
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

		[Browsable (true)]
		[DefaultValue (DataGridViewHeaderBorderStyle.Raised)]
		public DataGridViewHeaderBorderStyle RowHeadersBorderStyle {
			get { return rowHeadersBorderStyle; }
			set {
				if (rowHeadersBorderStyle != value) {
					rowHeadersBorderStyle = value;
					OnRowHeadersBorderStyleChanged(EventArgs.Empty);
				}
			}
		}

		[AmbientValue (null)]
		public DataGridViewCellStyle RowHeadersDefaultCellStyle {
			get { return rowHeadersDefaultCellStyle; }
			set {
				if (rowHeadersDefaultCellStyle != value) {
					rowHeadersDefaultCellStyle = value;
					OnRowHeadersDefaultCellStyleChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue (true)]
		public bool RowHeadersVisible {
			get { return rowHeadersVisible; }
			set { rowHeadersVisible = value; }
		}

		[Localizable (true)]
		public int RowHeadersWidth {
			get { return rowHeadersWidth; }
			set {
				if (rowHeadersWidth != value) {
					if (value < 4) {
						throw new ArgumentOutOfRangeException("RowHeadersWidth", 
							"Row headers width cant be less than 4.");
					}
					if (value > 32768 ) {
						throw new ArgumentOutOfRangeException("RowHeadersWidth", 
							"Row headers width cannot be more than 32768.");
					}
					rowHeadersWidth = value;
					OnRowHeadersWidthChanged(EventArgs.Empty);
				}
			}
		}

		[RefreshProperties (RefreshProperties.All)]
		[DefaultValue (DataGridViewRowHeadersWidthSizeMode.EnableResizing)]
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

		[Browsable (false)]
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

		[Browsable (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
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

		internal DataGridViewRow RowTemplateFull {
			get {
				DataGridViewRow row = (DataGridViewRow) RowTemplate.Clone ();
				
				for (int i = row.Cells.Count; i < Columns.Count; i++) {
					row.Cells.Add ((DataGridViewCell) columns [i].CellTemplate.Clone ());
				}
				
				return row;
			}
		}

		[DefaultValue (ScrollBars.Both)]
		[Localizable (true)]
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

		[Browsable (false)]
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

		[Browsable (false)]
		public DataGridViewSelectedColumnCollection SelectedColumns {
			get
			{
				DataGridViewSelectedColumnCollection result = new DataGridViewSelectedColumnCollection ();

				if (selectionMode != DataGridViewSelectionMode.FullColumnSelect && selectionMode != DataGridViewSelectionMode.ColumnHeaderSelect)
					return result;

				result.InternalAddRange (selected_columns);

				return result;
			}
		}

		[Browsable (false)]
		public DataGridViewSelectedRowCollection SelectedRows {
			get {
				DataGridViewSelectedRowCollection result = new DataGridViewSelectedRowCollection ();
				
				if (selectionMode != DataGridViewSelectionMode.FullRowSelect && selectionMode != DataGridViewSelectionMode.RowHeaderSelect)
					return result;
				
				result.InternalAddRange (selected_rows);

				return result;
			}
		}

		[Browsable (true)]
		[DefaultValue (DataGridViewSelectionMode.RowHeaderSelect)]
		public DataGridViewSelectionMode SelectionMode {
			get { return selectionMode; }
			set {
				if (!Enum.IsDefined(typeof(DataGridViewSelectionMode), value)) {
					throw new InvalidEnumArgumentException("Value is not valid DataGridViewSelectionMode.");
				}
				selectionMode = value;
			}
		}

		[DefaultValue (true)]
		public bool ShowCellErrors {
			get { return showCellErrors; }
			set { showCellErrors = value; }
		}

		[DefaultValue (true)]
		public bool ShowCellToolTips {
			get { return showCellToolTips; }
			set { showCellToolTips = value; }
		}

		[DefaultValue (true)]
		public bool ShowEditingIcon {
			get { return showEditingIcon; }
			set { showEditingIcon = value; }
		}

		[DefaultValue (true)]
		public bool ShowRowErrors {
			get { return showRowErrors; }
			set { showRowErrors = value; }
		}

		[Browsable (false)]
		public DataGridViewColumn SortedColumn {
			get { return sortedColumn; }
		}

		[Browsable (false)]
		public SortOrder SortOrder {
			get { return sortOrder; }
		}

		[DefaultValue (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool StandardTab {
			get { return standardTab; }
			set { standardTab = value; }
		}

		[Bindable (false)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataGridViewHeaderCell TopLeftHeaderCell {
			get { return topLeftHeaderCell; }
			set { topLeftHeaderCell = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public Cursor UserSetCursor {
			get { return userSetCursor; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int VerticalScrollingOffset {
			get { return verticalScrollingOffset; }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DefaultValue (false)]
		public bool VirtualMode {
			get { return virtualMode; }
			set { virtualMode = value; }
		}

		internal Control EditingControlInternal {
			get { 
				return editingControl; 
			}
			set {
				if (value == editingControl)
					return;

				if (editingControl != null) {
					// Can't use Controls.Remove (editingControls), because that method
					// is overriden to not remove the editing control.
					DataGridView.DataGridViewControlCollection ctrls = Controls as DataGridView.DataGridViewControlCollection;
					if (ctrls != null) {
						ctrls.RemoveInternal (editingControl);
					} else {
						Controls.Remove (editingControl);
					}
				}
				
				if (value != null)
					Controls.Add (value);

				editingControl = value;
			}
		}

		static object AllowUserToAddRowsChangedEvent = new object ();
		static object AllowUserToDeleteRowsChangedEvent = new object ();
		static object AllowUserToOrderColumnsChangedEvent = new object ();
		static object AllowUserToResizeColumnsChangedEvent = new object ();
		static object AllowUserToResizeRowsChangedEvent = new object ();
		static object AlternatingRowsDefaultCellStyleChangedEvent = new object ();
		static object AutoGenerateColumnsChangedEvent = new object ();
		static object AutoSizeColumnModeChangedEvent = new object ();
		static object AutoSizeColumnsModeChangedEvent = new object ();
		static object AutoSizeRowsModeChangedEvent = new object ();
		static object BackgroundColorChangedEvent = new object ();
		static object BorderStyleChangedEvent = new object ();
		static object CancelRowEditEvent = new object ();
		static object CellBeginEditEvent = new object ();
		static object CellBorderStyleChangedEvent = new object ();
		static object CellClickEvent = new object ();
		static object CellContentClickEvent = new object ();
		static object CellContentDoubleClickEvent = new object ();
		static object CellContextMenuStripChangedEvent = new object ();
		static object CellContextMenuStripNeededEvent = new object ();
		static object CellDoubleClickEvent = new object ();
		static object CellEndEditEvent = new object ();
		static object CellEnterEvent = new object ();
		static object CellErrorTextChangedEvent = new object ();
		static object CellErrorTextNeededEvent = new object ();
		static object CellFormattingEvent = new object ();
		static object CellLeaveEvent = new object ();
		static object CellMouseClickEvent = new object ();
		static object CellMouseDoubleClickEvent = new object ();
		static object CellMouseDownEvent = new object ();
		static object CellMouseEnterEvent = new object ();
		static object CellMouseLeaveEvent = new object ();
		static object CellMouseMoveEvent = new object ();
		static object CellMouseUpEvent = new object ();
		static object CellPaintingEvent = new object ();
		static object CellParsingEvent = new object ();
		static object CellStateChangedEvent = new object ();
		static object CellStyleChangedEvent = new object ();
		static object CellStyleContentChangedEvent = new object ();
		static object CellToolTipTextChangedEvent = new object ();
		static object CellToolTipTextNeededEvent = new object ();
		static object CellValidatedEvent = new object ();
		static object CellValidatingEvent = new object ();
		static object CellValueChangedEvent = new object ();
		static object CellValueNeededEvent = new object ();
		static object CellValuePushedEvent = new object ();
		static object ColumnAddedEvent = new object ();
		static object ColumnContextMenuStripChangedEvent = new object ();
		static object ColumnDataPropertyNameChangedEvent = new object ();
		static object ColumnDefaultCellStyleChangedEvent = new object ();
		static object ColumnDisplayIndexChangedEvent = new object ();
		static object ColumnDividerDoubleClickEvent = new object ();
		static object ColumnDividerWidthChangedEvent = new object ();
		static object ColumnHeaderCellChangedEvent = new object ();
		static object ColumnHeaderMouseClickEvent = new object ();
		static object ColumnHeaderMouseDoubleClickEvent = new object ();
		static object ColumnHeadersBorderStyleChangedEvent = new object ();
		static object ColumnHeadersDefaultCellStyleChangedEvent = new object ();
		static object ColumnHeadersHeightChangedEvent = new object ();
		static object ColumnHeadersHeightSizeModeChangedEvent = new object ();
		static object ColumnMinimumWidthChangedEvent = new object ();
		static object ColumnNameChangedEvent = new object ();
		static object ColumnRemovedEvent = new object ();
		static object ColumnSortModeChangedEvent = new object ();
		static object ColumnStateChangedEvent = new object ();
		static object ColumnToolTipTextChangedEvent = new object ();
		static object ColumnWidthChangedEvent = new object ();
		static object CurrentCellChangedEvent = new object ();
		static object CurrentCellDirtyStateChangedEvent = new object ();
		static object DataBindingCompleteEvent = new object ();
		static object DataErrorEvent = new object ();
		static object DataMemberChangedEvent = new object ();
		static object DataSourceChangedEvent = new object ();
		static object DefaultCellStyleChangedEvent = new object ();
		static object DefaultValuesNeededEvent = new object ();
		static object EditingControlShowingEvent = new object ();
		static object EditModeChangedEvent = new object ();
		static object GridColorChangedEvent = new object ();
		static object MultiSelectChangedEvent = new object ();
		static object NewRowNeededEvent = new object ();
		static object ReadOnlyChangedEvent = new object ();
		static object RowContextMenuStripChangedEvent = new object ();
		static object RowContextMenuStripNeededEvent = new object ();
		static object RowDefaultCellStyleChangedEvent = new object ();
		static object RowDirtyStateNeededEvent = new object ();
		static object RowDividerDoubleClickEvent = new object ();
		static object RowDividerHeightChangedEvent = new object ();
		static object RowEnterEvent = new object ();
		static object RowErrorTextChangedEvent = new object ();
		static object RowErrorTextNeededEvent = new object ();
		static object RowHeaderCellChangedEvent = new object ();
		static object RowHeaderMouseClickEvent = new object ();
		static object RowHeaderMouseDoubleClickEvent = new object ();
		static object RowHeadersBorderStyleChangedEvent = new object ();
		static object RowHeadersDefaultCellStyleChangedEvent = new object ();
		static object RowHeadersWidthChangedEvent = new object ();
		static object RowHeadersWidthSizeModeChangedEvent = new object ();
		static object RowHeightChangedEvent = new object ();
		static object RowHeightInfoNeededEvent = new object ();
		static object RowHeightInfoPushedEvent = new object ();
		static object RowLeaveEvent = new object ();
		static object RowMinimumHeightChangedEvent = new object ();
		static object RowPostPaintEvent = new object ();
		static object RowPrePaintEvent = new object ();
		static object RowsAddedEvent = new object ();
		static object RowsDefaultCellStyleChangedEvent = new object ();
		static object RowsRemovedEvent = new object ();
		static object RowStateChangedEvent = new object ();
		static object RowUnsharedEvent = new object ();
		static object RowValidatedEvent = new object ();
		static object RowValidatingEvent = new object ();
		static object ScrollEvent = new object ();
		static object SelectionChangedEvent = new object ();
		static object SortCompareEvent = new object ();
		static object SortedEvent = new object ();
		static object UserAddedRowEvent = new object ();
		static object UserDeletedRowEvent = new object ();
		static object UserDeletingRowEvent = new object ();


		//

		public event EventHandler AllowUserToAddRowsChanged {
			add { Events.AddHandler (AllowUserToAddRowsChangedEvent, value); }
			remove { Events.RemoveHandler (AllowUserToAddRowsChangedEvent, value); }
		}

		public event EventHandler AllowUserToDeleteRowsChanged {
			add { Events.AddHandler (AllowUserToDeleteRowsChangedEvent, value); }
			remove { Events.RemoveHandler (AllowUserToDeleteRowsChangedEvent, value); }
		}

		public event EventHandler AllowUserToOrderColumnsChanged {
			add { Events.AddHandler (AllowUserToOrderColumnsChangedEvent, value); }
			remove { Events.RemoveHandler (AllowUserToOrderColumnsChangedEvent, value); }
		}

		public event EventHandler AllowUserToResizeColumnsChanged {
			add { Events.AddHandler (AllowUserToResizeColumnsChangedEvent, value); }
			remove { Events.RemoveHandler (AllowUserToResizeColumnsChangedEvent, value); }
		}

		public event EventHandler AllowUserToResizeRowsChanged {
			add { Events.AddHandler (AllowUserToResizeRowsChangedEvent, value); }
			remove { Events.RemoveHandler (AllowUserToResizeRowsChangedEvent, value); }
		}

		public event EventHandler AlternatingRowsDefaultCellStyleChanged {
			add { Events.AddHandler (AlternatingRowsDefaultCellStyleChangedEvent, value); }
			remove { Events.RemoveHandler (AlternatingRowsDefaultCellStyleChangedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event EventHandler AutoGenerateColumnsChanged {
			add { Events.AddHandler (AutoGenerateColumnsChangedEvent, value); }
			remove { Events.RemoveHandler (AutoGenerateColumnsChangedEvent, value); }
		}

		public event DataGridViewAutoSizeColumnModeEventHandler AutoSizeColumnModeChanged {
			add { Events.AddHandler (AutoSizeColumnModeChangedEvent, value); }
			remove { Events.RemoveHandler (AutoSizeColumnModeChangedEvent, value); }
		}

		public event DataGridViewAutoSizeColumnsModeEventHandler AutoSizeColumnsModeChanged {
			add { Events.AddHandler (AutoSizeColumnsModeChangedEvent, value); }
			remove { Events.RemoveHandler (AutoSizeColumnsModeChangedEvent, value); }
		}

		public event DataGridViewAutoSizeModeEventHandler AutoSizeRowsModeChanged {
			add { Events.AddHandler (AutoSizeRowsModeChangedEvent, value); }
			remove { Events.RemoveHandler (AutoSizeRowsModeChangedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add { base.BackColorChanged += value; }
			remove { base.BackColorChanged -= value; }
		}

		public event EventHandler BackgroundColorChanged {
			add { Events.AddHandler (BackgroundColorChangedEvent, value); }
			remove { Events.RemoveHandler (BackgroundColorChangedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged  {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		public event EventHandler BorderStyleChanged {
			add { Events.AddHandler (BorderStyleChangedEvent, value); }
			remove { Events.RemoveHandler (BorderStyleChangedEvent, value); }
		}

		public event QuestionEventHandler CancelRowEdit {
			add { Events.AddHandler (CancelRowEditEvent, value); }
			remove { Events.RemoveHandler (CancelRowEditEvent, value); }
		}

		public event DataGridViewCellCancelEventHandler CellBeginEdit {
			add { Events.AddHandler (CellBeginEditEvent, value); }
			remove { Events.RemoveHandler (CellBeginEditEvent, value); }
		}

		public event EventHandler CellBorderStyleChanged {
			add { Events.AddHandler (CellBorderStyleChangedEvent, value); }
			remove { Events.RemoveHandler (CellBorderStyleChangedEvent, value); }
		}

		public event DataGridViewCellEventHandler CellClick {
			add { Events.AddHandler (CellClickEvent, value); }
			remove { Events.RemoveHandler (CellClickEvent, value); }
		}

		public event DataGridViewCellEventHandler CellContentClick {
			add { Events.AddHandler (CellContentClickEvent, value); }
			remove { Events.RemoveHandler (CellContentClickEvent, value); }
		}

		public event DataGridViewCellEventHandler CellContentDoubleClick {
			add { Events.AddHandler (CellContentDoubleClickEvent, value); }
			remove { Events.RemoveHandler (CellContentDoubleClickEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewCellEventHandler CellContextMenuStripChanged {
			add { Events.AddHandler (CellContextMenuStripChangedEvent, value); }
			remove { Events.RemoveHandler (CellContextMenuStripChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewCellContextMenuStripNeededEventHandler CellContextMenuStripNeeded {
			add { Events.AddHandler (CellContextMenuStripNeededEvent, value); }
			remove { Events.RemoveHandler (CellContextMenuStripNeededEvent, value); }
		}

		public event DataGridViewCellEventHandler CellDoubleClick {
			add { Events.AddHandler (CellDoubleClickEvent, value); }
			remove { Events.RemoveHandler (CellDoubleClickEvent, value); }
		}

		public event DataGridViewCellEventHandler CellEndEdit {
			add { Events.AddHandler (CellEndEditEvent, value); }
			remove { Events.RemoveHandler (CellEndEditEvent, value); }
		}

		public event DataGridViewCellEventHandler CellEnter {
			add { Events.AddHandler (CellEnterEvent, value); }
			remove { Events.RemoveHandler (CellEnterEvent, value); }
		}

		public event DataGridViewCellEventHandler CellErrorTextChanged {
			add { Events.AddHandler (CellErrorTextChangedEvent, value); }
			remove { Events.RemoveHandler (CellErrorTextChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewCellErrorTextNeededEventHandler CellErrorTextNeeded {
			add { Events.AddHandler (CellErrorTextNeededEvent, value); }
			remove { Events.RemoveHandler (CellErrorTextNeededEvent, value); }
		}

		public event DataGridViewCellFormattingEventHandler CellFormatting {
			add { Events.AddHandler (CellFormattingEvent, value); }
			remove { Events.RemoveHandler (CellFormattingEvent, value); }
		}

		public event DataGridViewCellEventHandler CellLeave {
			add { Events.AddHandler (CellLeaveEvent, value); }
			remove { Events.RemoveHandler (CellLeaveEvent, value); }
		}

		public event DataGridViewCellMouseEventHandler CellMouseClick {
			add { Events.AddHandler (CellMouseClickEvent, value); }
			remove { Events.RemoveHandler (CellMouseClickEvent, value); }
		}

		public event DataGridViewCellMouseEventHandler CellMouseDoubleClick {
			add { Events.AddHandler (CellMouseDoubleClickEvent, value); }
			remove { Events.RemoveHandler (CellMouseDoubleClickEvent, value); }
		}

		public event DataGridViewCellMouseEventHandler CellMouseDown {
			add { Events.AddHandler (CellMouseDownEvent, value); }
			remove { Events.RemoveHandler (CellMouseDownEvent, value); }
		}

		public event DataGridViewCellEventHandler CellMouseEnter {
			add { Events.AddHandler (CellMouseEnterEvent, value); }
			remove { Events.RemoveHandler (CellMouseEnterEvent, value); }
		}

		public event DataGridViewCellEventHandler CellMouseLeave {
			add { Events.AddHandler (CellMouseLeaveEvent, value); }
			remove { Events.RemoveHandler (CellMouseLeaveEvent, value); }
		}

		public event DataGridViewCellMouseEventHandler CellMouseMove {
			add { Events.AddHandler (CellMouseMoveEvent, value); }
			remove { Events.RemoveHandler (CellMouseMoveEvent, value); }
		}

		public event DataGridViewCellMouseEventHandler CellMouseUp {
			add { Events.AddHandler (CellMouseUpEvent, value); }
			remove { Events.RemoveHandler (CellMouseUpEvent, value); }
		}

		public event DataGridViewCellPaintingEventHandler CellPainting {
			add { Events.AddHandler (CellPaintingEvent, value); }
			remove { Events.RemoveHandler (CellPaintingEvent, value); }
		}

		public event DataGridViewCellParsingEventHandler CellParsing {
			add { Events.AddHandler (CellParsingEvent, value); }
			remove { Events.RemoveHandler (CellParsingEvent, value); }
		}

		public event DataGridViewCellStateChangedEventHandler CellStateChanged {
			add { Events.AddHandler (CellStateChangedEvent, value); }
			remove { Events.RemoveHandler (CellStateChangedEvent, value); }
		}

		public event DataGridViewCellEventHandler CellStyleChanged {
			add { Events.AddHandler (CellStyleChangedEvent, value); }
			remove { Events.RemoveHandler (CellStyleChangedEvent, value); }
		}

		public event DataGridViewCellStyleContentChangedEventHandler CellStyleContentChanged {
			add { Events.AddHandler (CellStyleContentChangedEvent, value); }
			remove { Events.RemoveHandler (CellStyleContentChangedEvent, value); }
		}

		public event DataGridViewCellEventHandler CellToolTipTextChanged {
			add { Events.AddHandler (CellToolTipTextChangedEvent, value); }
			remove { Events.RemoveHandler (CellToolTipTextChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewCellToolTipTextNeededEventHandler CellToolTipTextNeeded {
			add { Events.AddHandler (CellToolTipTextNeededEvent, value); }
			remove { Events.RemoveHandler (CellToolTipTextNeededEvent, value); }
		}

		public event DataGridViewCellEventHandler CellValidated {
			add { Events.AddHandler (CellValidatedEvent, value); }
			remove { Events.RemoveHandler (CellValidatedEvent, value); }
		}

		public event DataGridViewCellValidatingEventHandler CellValidating {
			add { Events.AddHandler (CellValidatingEvent, value); }
			remove { Events.RemoveHandler (CellValidatingEvent, value); }
		}

		public event DataGridViewCellEventHandler CellValueChanged {
			add { Events.AddHandler (CellValueChangedEvent, value); }
			remove { Events.RemoveHandler (CellValueChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewCellValueEventHandler CellValueNeeded {
			add { Events.AddHandler (CellValueNeededEvent, value); }
			remove { Events.RemoveHandler (CellValueNeededEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewCellValueEventHandler CellValuePushed {
			add { Events.AddHandler (CellValuePushedEvent, value); }
			remove { Events.RemoveHandler (CellValuePushedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnAdded {
			add { Events.AddHandler (ColumnAddedEvent, value); }
			remove { Events.RemoveHandler (ColumnAddedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnContextMenuStripChanged {
			add { Events.AddHandler (ColumnContextMenuStripChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnContextMenuStripChangedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnDataPropertyNameChanged {
			add { Events.AddHandler (ColumnDataPropertyNameChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnDataPropertyNameChangedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnDefaultCellStyleChanged {
			add { Events.AddHandler (ColumnDefaultCellStyleChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnDefaultCellStyleChangedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnDisplayIndexChanged {
			add { Events.AddHandler (ColumnDisplayIndexChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnDisplayIndexChangedEvent, value); }
		}

		public event DataGridViewColumnDividerDoubleClickEventHandler ColumnDividerDoubleClick {
			add { Events.AddHandler (ColumnDividerDoubleClickEvent, value); }
			remove { Events.RemoveHandler (ColumnDividerDoubleClickEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnDividerWidthChanged {
			add { Events.AddHandler (ColumnDividerWidthChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnDividerWidthChangedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnHeaderCellChanged {
			add { Events.AddHandler (ColumnHeaderCellChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnHeaderCellChangedEvent, value); }
		}

		public event DataGridViewCellMouseEventHandler ColumnHeaderMouseClick {
			add { Events.AddHandler (ColumnHeaderMouseClickEvent, value); }
			remove { Events.RemoveHandler (ColumnHeaderMouseClickEvent, value); }
		}

		public event DataGridViewCellMouseEventHandler ColumnHeaderMouseDoubleClick {
			add { Events.AddHandler (ColumnHeaderMouseDoubleClickEvent, value); }
			remove { Events.RemoveHandler (ColumnHeaderMouseDoubleClickEvent, value); }
		}

		public event EventHandler ColumnHeadersBorderStyleChanged {
			add { Events.AddHandler (ColumnHeadersBorderStyleChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnHeadersBorderStyleChangedEvent, value); }
		}

		public event EventHandler ColumnHeadersDefaultCellStyleChanged {
			add { Events.AddHandler (ColumnHeadersDefaultCellStyleChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnHeadersDefaultCellStyleChangedEvent, value); }
		}

		public event EventHandler ColumnHeadersHeightChanged {
			add { Events.AddHandler (ColumnHeadersHeightChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnHeadersHeightChangedEvent, value); }
		}

		public event DataGridViewAutoSizeModeEventHandler ColumnHeadersHeightSizeModeChanged {
			add { Events.AddHandler (ColumnHeadersHeightSizeModeChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnHeadersHeightSizeModeChangedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnMinimumWidthChanged {
			add { Events.AddHandler (ColumnMinimumWidthChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnMinimumWidthChangedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnNameChanged {
			add { Events.AddHandler (ColumnNameChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnNameChangedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnRemoved {
			add { Events.AddHandler (ColumnRemovedEvent, value); }
			remove { Events.RemoveHandler (ColumnRemovedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnSortModeChanged {
			add { Events.AddHandler (ColumnSortModeChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnSortModeChangedEvent, value); }
		}

		public event DataGridViewColumnStateChangedEventHandler ColumnStateChanged {
			add { Events.AddHandler (ColumnStateChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnStateChangedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnToolTipTextChanged {
			add { Events.AddHandler (ColumnToolTipTextChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnToolTipTextChangedEvent, value); }
		}

		public event DataGridViewColumnEventHandler ColumnWidthChanged {
			add { Events.AddHandler (ColumnWidthChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnWidthChangedEvent, value); }
		}

		public event EventHandler CurrentCellChanged {
			add { Events.AddHandler (CurrentCellChangedEvent, value); }
			remove { Events.RemoveHandler (CurrentCellChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event EventHandler CurrentCellDirtyStateChanged {
			add { Events.AddHandler (CurrentCellDirtyStateChangedEvent, value); }
			remove { Events.RemoveHandler (CurrentCellDirtyStateChangedEvent, value); }
		}

		public event DataGridViewBindingCompleteEventHandler DataBindingComplete {
			add { Events.AddHandler (DataBindingCompleteEvent, value); }
			remove { Events.RemoveHandler (DataBindingCompleteEvent, value); }
		}

		public event DataGridViewDataErrorEventHandler DataError {
			add { Events.AddHandler (DataErrorEvent, value); }
			remove { Events.RemoveHandler (DataErrorEvent, value); }
		}

		public event EventHandler DataMemberChanged {
			add { Events.AddHandler (DataMemberChangedEvent, value); }
			remove { Events.RemoveHandler (DataMemberChangedEvent, value); }
		}

		public event EventHandler DataSourceChanged {
			add { Events.AddHandler (DataSourceChangedEvent, value); }
			remove { Events.RemoveHandler (DataSourceChangedEvent, value); }
		}

		public event EventHandler DefaultCellStyleChanged {
			add { Events.AddHandler (DefaultCellStyleChangedEvent, value); }
			remove { Events.RemoveHandler (DefaultCellStyleChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewRowEventHandler DefaultValuesNeeded {
			add { Events.AddHandler (DefaultValuesNeededEvent, value); }
			remove { Events.RemoveHandler (DefaultValuesNeededEvent, value); }
		}

		public event DataGridViewEditingControlShowingEventHandler EditingControlShowing {
			add { Events.AddHandler (EditingControlShowingEvent, value); }
			remove { Events.RemoveHandler (EditingControlShowingEvent, value); }
		}

		public event EventHandler EditModeChanged {
			add { Events.AddHandler (EditModeChangedEvent, value); }
			remove { Events.RemoveHandler (EditModeChangedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public new event EventHandler FontChanged {
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler PaddingChanged {
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}

		public event EventHandler GridColorChanged {
			add { Events.AddHandler (GridColorChangedEvent, value); }
			remove { Events.RemoveHandler (GridColorChangedEvent, value); }
		}

		public event EventHandler MultiSelectChanged {
			add { Events.AddHandler (MultiSelectChangedEvent, value); }
			remove { Events.RemoveHandler (MultiSelectChangedEvent, value); }
		}

		public event DataGridViewRowEventHandler NewRowNeeded {
			add { Events.AddHandler (NewRowNeededEvent, value); }
			remove { Events.RemoveHandler (NewRowNeededEvent, value); }
		}

		public event EventHandler ReadOnlyChanged {
			add { Events.AddHandler (ReadOnlyChangedEvent, value); }
			remove { Events.RemoveHandler (ReadOnlyChangedEvent, value); }
		}

		public event DataGridViewRowEventHandler RowContextMenuStripChanged {
			add { Events.AddHandler (RowContextMenuStripChangedEvent, value); }
			remove { Events.RemoveHandler (RowContextMenuStripChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewRowContextMenuStripNeededEventHandler RowContextMenuStripNeeded {
			add { Events.AddHandler (RowContextMenuStripNeededEvent, value); }
			remove { Events.RemoveHandler (RowContextMenuStripNeededEvent, value); }
		}

		public event DataGridViewRowEventHandler RowDefaultCellStyleChanged {
			add { Events.AddHandler (RowDefaultCellStyleChangedEvent, value); }
			remove { Events.RemoveHandler (RowDefaultCellStyleChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event QuestionEventHandler RowDirtyStateNeeded {
			add { Events.AddHandler (RowDirtyStateNeededEvent, value); }
			remove { Events.RemoveHandler (RowDirtyStateNeededEvent, value); }
		}

		public event DataGridViewRowDividerDoubleClickEventHandler RowDividerDoubleClick {
			add { Events.AddHandler (RowDividerDoubleClickEvent, value); }
			remove { Events.RemoveHandler (RowDividerDoubleClickEvent, value); }
		}

		public event DataGridViewRowEventHandler RowDividerHeightChanged {
			add { Events.AddHandler (RowDividerHeightChangedEvent, value); }
			remove { Events.RemoveHandler (RowDividerHeightChangedEvent, value); }
		}

		public event DataGridViewCellEventHandler RowEnter {
			add { Events.AddHandler (RowEnterEvent, value); }
			remove { Events.RemoveHandler (RowEnterEvent, value); }
		}

		public event DataGridViewRowEventHandler RowErrorTextChanged {
			add { Events.AddHandler (RowErrorTextChangedEvent, value); }
			remove { Events.RemoveHandler (RowErrorTextChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewRowErrorTextNeededEventHandler RowErrorTextNeeded {
			add { Events.AddHandler (RowErrorTextNeededEvent, value); }
			remove { Events.RemoveHandler (RowErrorTextNeededEvent, value); }
		}

		public event DataGridViewRowEventHandler RowHeaderCellChanged {
			add { Events.AddHandler (RowHeaderCellChangedEvent, value); }
			remove { Events.RemoveHandler (RowHeaderCellChangedEvent, value); }
		}

		public event DataGridViewCellMouseEventHandler RowHeaderMouseClick {
			add { Events.AddHandler (RowHeaderMouseClickEvent, value); }
			remove { Events.RemoveHandler (RowHeaderMouseClickEvent, value); }
		}

		public event DataGridViewCellMouseEventHandler RowHeaderMouseDoubleClick {
			add { Events.AddHandler (RowHeaderMouseDoubleClickEvent, value); }
			remove { Events.RemoveHandler (RowHeaderMouseDoubleClickEvent, value); }
		}

		public event EventHandler RowHeadersBorderStyleChanged {
			add { Events.AddHandler (RowHeadersBorderStyleChangedEvent, value); }
			remove { Events.RemoveHandler (RowHeadersBorderStyleChangedEvent, value); }
		}

		public event EventHandler RowHeadersDefaultCellStyleChanged {
			add { Events.AddHandler (RowHeadersDefaultCellStyleChangedEvent, value); }
			remove { Events.RemoveHandler (RowHeadersDefaultCellStyleChangedEvent, value); }
		}

		public event EventHandler RowHeadersWidthChanged {
			add { Events.AddHandler (RowHeadersWidthChangedEvent, value); }
			remove { Events.RemoveHandler (RowHeadersWidthChangedEvent, value); }
		}

		public event DataGridViewAutoSizeModeEventHandler RowHeadersWidthSizeModeChanged {
			add { Events.AddHandler (RowHeadersWidthSizeModeChangedEvent, value); }
			remove { Events.RemoveHandler (RowHeadersWidthSizeModeChangedEvent, value); }
		}

		public event DataGridViewRowEventHandler RowHeightChanged {
			add { Events.AddHandler (RowHeightChangedEvent, value); }
			remove { Events.RemoveHandler (RowHeightChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewRowHeightInfoNeededEventHandler RowHeightInfoNeeded {
			add { Events.AddHandler (RowHeightInfoNeededEvent, value); }
			remove { Events.RemoveHandler (RowHeightInfoNeededEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewRowHeightInfoPushedEventHandler RowHeightInfoPushed {
			add { Events.AddHandler (RowHeightInfoPushedEvent, value); }
			remove { Events.RemoveHandler (RowHeightInfoPushedEvent, value); }
		}

		public event DataGridViewCellEventHandler RowLeave {
			add { Events.AddHandler (RowLeaveEvent, value); }
			remove { Events.RemoveHandler (RowLeaveEvent, value); }
		}

		public event DataGridViewRowEventHandler RowMinimumHeightChanged {
			add { Events.AddHandler (RowMinimumHeightChangedEvent, value); }
			remove { Events.RemoveHandler (RowMinimumHeightChangedEvent, value); }
		}

		public event DataGridViewRowPostPaintEventHandler RowPostPaint {
			add { Events.AddHandler (RowPostPaintEvent, value); }
			remove { Events.RemoveHandler (RowPostPaintEvent, value); }
		}

		public event DataGridViewRowPrePaintEventHandler RowPrePaint {
			add { Events.AddHandler (RowPrePaintEvent, value); }
			remove { Events.RemoveHandler (RowPrePaintEvent, value); }
		}

		public event DataGridViewRowsAddedEventHandler RowsAdded {
			add { Events.AddHandler (RowsAddedEvent, value); }
			remove { Events.RemoveHandler (RowsAddedEvent, value); }
		}

		public event EventHandler RowsDefaultCellStyleChanged {
			add { Events.AddHandler (RowsDefaultCellStyleChangedEvent, value); }
			remove { Events.RemoveHandler (RowsDefaultCellStyleChangedEvent, value); }
		}

		public event DataGridViewRowsRemovedEventHandler RowsRemoved {
			add { Events.AddHandler (RowsRemovedEvent, value); }
			remove { Events.RemoveHandler (RowsRemovedEvent, value); }
		}

		public event DataGridViewRowStateChangedEventHandler RowStateChanged {
			add { Events.AddHandler (RowStateChangedEvent, value); }
			remove { Events.RemoveHandler (RowStateChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewRowEventHandler RowUnshared {
			add { Events.AddHandler (RowUnsharedEvent, value); }
			remove { Events.RemoveHandler (RowUnsharedEvent, value); }
		}

		public event DataGridViewCellEventHandler RowValidated {
			add { Events.AddHandler (RowValidatedEvent, value); }
			remove { Events.RemoveHandler (RowValidatedEvent, value); }
		}

		public event DataGridViewCellCancelEventHandler RowValidating {
			add { Events.AddHandler (RowValidatingEvent, value); }
			remove { Events.RemoveHandler (RowValidatingEvent, value); }
		}

		public event ScrollEventHandler Scroll {
			add { Events.AddHandler (ScrollEvent, value); }
			remove { Events.RemoveHandler (ScrollEvent, value); }
		}

		public event EventHandler SelectionChanged {
			add { Events.AddHandler (SelectionChangedEvent, value); }
			remove { Events.RemoveHandler (SelectionChangedEvent, value); }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event DataGridViewSortCompareEventHandler SortCompare {
			add { Events.AddHandler (SortCompareEvent, value); }
			remove { Events.RemoveHandler (SortCompareEvent, value); }
		}

		public event EventHandler Sorted {
			add { Events.AddHandler (SortedEvent, value); }
			remove { Events.RemoveHandler (SortedEvent, value); }
		}

		public event DataGridViewRowEventHandler UserAddedRow {
			add { Events.AddHandler (UserAddedRowEvent, value); }
			remove { Events.RemoveHandler (UserAddedRowEvent, value); }
		}

		public event DataGridViewRowEventHandler UserDeletedRow {
			add { Events.AddHandler (UserDeletedRowEvent, value); }
			remove { Events.RemoveHandler (UserDeletedRowEvent, value); }
		}

		public event DataGridViewRowCancelEventHandler UserDeletingRow {
			add { Events.AddHandler (UserDeletingRowEvent, value); }
			remove { Events.RemoveHandler (UserDeletingRowEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler StyleChanged {
			add { base.StyleChanged += value; }
			remove { base.StyleChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public virtual DataGridViewAdvancedBorderStyle AdjustColumnHeaderBorderStyle (DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStyleInput, DataGridViewAdvancedBorderStyle dataGridViewAdvancedBorderStylePlaceholder, bool isFirstDisplayedColumn, bool isLastVisibleColumn) {
			return (DataGridViewAdvancedBorderStyle) ((ICloneable)dataGridViewAdvancedBorderStyleInput).Clone();
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
			AutoResizeColumn (columnIndex, DataGridViewAutoSizeColumnMode.AllCells);
		}

		public void AutoResizeColumn (int columnIndex, DataGridViewAutoSizeColumnMode autoSizeColumnMode) {
			AutoResizeColumn (columnIndex, autoSizeColumnMode, true);
		}

		public void AutoResizeColumnHeadersHeight () {
			throw new NotImplementedException();
		}

		public void AutoResizeColumnHeadersHeight (int columnIndex) {
			throw new NotImplementedException();
		}

		public void AutoResizeColumns () {
			AutoResizeColumns (DataGridViewAutoSizeColumnsMode.AllCells);
		}

		public void AutoResizeColumns (DataGridViewAutoSizeColumnsMode autoSizeColumnsMode) {
			AutoResizeColumns (autoSizeColumnsMode, true);
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
			if (currentCell == null || currentCell.IsInEditMode)
				return false;
			
			if (currentCell.RowIndex >= 0) {
				if ((currentCell.InheritedState & DataGridViewElementStates.ReadOnly) == DataGridViewElementStates.ReadOnly) {
					return false;
				}
			}
			
			DataGridViewCell cell = currentCell;
			Type editType = cell.EditType;
			if (editType == null)
				return false;

			cell.SetIsInEditMode (true);
			Control ctrl = EditingControlInternal;
			bool isCorrectType = ctrl != null && ctrl.GetType () == editType;
			if (ctrl != null && !isCorrectType) {
				ctrl = null;
			}
			if (ctrl == null) {
				ctrl = (Control) Activator.CreateInstance (editType);
				EditingControlInternal = ctrl;
			}

			IDataGridViewEditingControl edControl = ctrl as IDataGridViewEditingControl;
			DataGridViewCellStyle style = cell.RowIndex == -1 ? DefaultCellStyle : cell.InheritedStyle;
			cell.InitializeEditingControl (cell.RowIndex, cell.FormattedValue, style);
			cell.PositionEditingControl (true, true, this.GetCellDisplayRectangle (cell.ColumnIndex, cell.RowIndex, false), bounds, style, false, false, (columns [cell.ColumnIndex].DisplayIndex == 0), (cell.RowIndex == 0));
			if (edControl != null)
				edControl.PrepareEditingControlForEdit (selectAll);
			ctrl.Visible = true;

			OnCellBeginEdit (new DataGridViewCellCancelEventArgs (cell.ColumnIndex, cell.RowIndex));
			return true;
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
			if (currentCell != null && currentCell.IsInEditMode) {
				currentCell.SetIsInEditMode (false);
				currentCell.DetachEditingControl ();
				OnCellEndEdit (new DataGridViewCellEventArgs (currentCell.ColumnIndex, currentCell.RowIndex));
			}
			return true;
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

		internal DataGridViewRow GetRowInternal (int rowIndex)
		{
			return Rows.SharedRow (rowIndex);
		}

		internal DataGridViewCell GetCellInternal (int colIndex, int rowIndex)
		{
			return GetRowInternal (rowIndex).Cells.GetCellInternal (colIndex);
		}

		public Rectangle GetCellDisplayRectangle (int columnIndex, int rowIndex, bool cutOverflow) {
			if (columnIndex < 0 || columnIndex >= columns.Count) {
				throw new ArgumentOutOfRangeException("Column index is out of range.");
			}
			
			int x = 0, y = 0, w = 0, h = 0;
			
			if (ColumnHeadersVisible)
				y = ColumnHeadersHeight;
			
			if (RowHeadersVisible)
				x = RowHeadersWidth;
				
			for (int i = 0; i < Columns.Count; i++) {
				if (i == columnIndex) {
					w = columns [i].Width;
					break;
				}
					
				x += columns [i].Width;
			}
			
			for (int i = 0; i < Rows.Count; i++) {
				if (i == rowIndex) {
					h = rows [i].Height;
					break;
				}
				
				y += rows [i].Height;
			}
			
			return new Rectangle (x, y, w, h);
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
			//Console.WriteLine ("HitTest ({0}, {1})", x, y);
			bool isInColHeader = columnHeadersVisible && y >= 0 && y <= ColumnHeadersHeight;
			bool isInRowHeader = rowHeadersVisible && x >= 0 && x <= RowHeadersWidth;
			
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
						if (rowIndex == -1) {
							rowIndex = rows.SharedRowIndexOf (row);
						}
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
			HitTestInfo result;
			
			if (colIndex >= 0 && rowIndex >= 0) {
				result = new HitTestInfo (colIndex, x, rowIndex, y, DataGridViewHitTestType.Cell);
			} else if (isInColHeader && isInRowHeader) {
				result = new HitTestInfo (colIndex, x, rowIndex, y, DataGridViewHitTestType.TopLeftHeader);
			} else if (isInColHeader) {
				result = new HitTestInfo (colIndex, x, rowIndex, y, DataGridViewHitTestType.ColumnHeader);
			} else if (isInRowHeader) {
				result = new HitTestInfo (colIndex, x, rowIndex, y, DataGridViewHitTestType.RowHeader);
			} else {
				result = new HitTestInfo (colIndex, x, rowIndex, y, DataGridViewHitTestType.None);
			}

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

		[EditorBrowsable (EditorBrowsableState.Never)]
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

		public void UpdateCellErrorText (int columnIndex, int rowIndex)
		{
			throw new NotImplementedException();
		}

		public void UpdateCellValue (int columnIndex, int rowIndex)
		{
			throw new NotImplementedException();
		}

		public void UpdateRowErrorText (int rowIndex)
		{
			throw new NotImplementedException();
		}

		public void UpdateRowErrorText (int rowIndexStart, int rowIndexEnd) {
			throw new NotImplementedException();
		}

		public void UpdateRowHeightInfo (int rowIndex, bool updateToEnd) {
			throw new NotImplementedException();
		}

		protected override Size DefaultSize {
			get { return new Size (240, 150); }
		}

		protected ScrollBar HorizontalScrollBar {
			get { return horizontalScrollBar; }
		}

		protected ScrollBar VerticalScrollBar {
			get { return verticalScrollBar; }
		}

		protected virtual void AccessibilityNotifyCurrentCellChanged (Point cellAddress)
		{
			throw new NotImplementedException ();
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
			for (int i = 0; i < Columns.Count; i++) {
				AutoResizeColumn (i, (DataGridViewAutoSizeColumnMode) autoSizeColumnsMode, fixedHeight);
			}
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
						SetSelectedRowCore (row.Index, false);
					}
					break;
				case DataGridViewSelectionMode.FullColumnSelect:
					foreach (DataGridViewColumn col in columns) {
						if (selectExceptionElement && col.Index == columnIndexException) {
							continue;
						}
						SetSelectedColumnCore (col.Index, false);
					}
					break;
				default:
					foreach (DataGridViewCell cell in SelectedCells) {
						if (selectExceptionElement && cell.RowIndex == rowIndexException && cell.ColumnIndex == columnIndexException) {
							continue;
						}
						SetSelectedCellCore (cell.ColumnIndex, cell.RowIndex, false);
					}
					break;
			}
		}

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new DataGridViewAccessibleObject(this);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual DataGridViewColumnCollection CreateColumnsInstance ()
		{
			return new DataGridViewColumnCollection(this);
		}

		protected override Control.ControlCollection CreateControlsInstance ()
		{
			return new DataGridViewControlCollection (this);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual DataGridViewRowCollection CreateRowsInstance ()
		{
			return new DataGridViewRowCollection(this);
		}

		protected override void Dispose (bool disposing) {
		}

		protected override AccessibleObject GetAccessibilityObjectById (int objectId)
		{
			throw new NotImplementedException();
		}

		protected override bool IsInputChar (char charCode)
		{
			return base.IsInputChar(charCode);
		}

		protected override bool IsInputKey (Keys keyData)
		{
			return base.IsInputKey(keyData);
		}

		protected virtual void OnAllowUserToAddRowsChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AllowUserToAddRowsChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAllowUserToDeleteRowsChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AllowUserToDeleteRowsChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAllowUserToOrderColumnsChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AllowUserToOrderColumnsChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAllowUserToResizeColumnsChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AllowUserToResizeColumnsChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAllowUserToResizeRowsChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AllowUserToResizeRowsChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAlternatingRowsDefaultCellStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AlternatingRowsDefaultCellStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAutoGenerateColumnsChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AutoGenerateColumnsChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAutoSizeColumnModeChanged (DataGridViewAutoSizeColumnModeEventArgs e)
		{
			DataGridViewAutoSizeColumnModeEventHandler eh = (DataGridViewAutoSizeColumnModeEventHandler)(Events [AutoSizeColumnModeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAutoSizeColumnsModeChanged (DataGridViewAutoSizeColumnsModeEventArgs e)
		{
			DataGridViewAutoSizeColumnsModeEventHandler eh = (DataGridViewAutoSizeColumnsModeEventHandler)(Events [AutoSizeColumnsModeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAutoSizeRowsModeChanged (DataGridViewAutoSizeModeEventArgs e)
		{
			DataGridViewAutoSizeModeEventHandler eh = (DataGridViewAutoSizeModeEventHandler)(Events [AutoSizeRowsModeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnBackgroundColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [BackgroundColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnBindingContextChanged (EventArgs e)
		{
			base.OnBindingContextChanged(e);
		}

		protected virtual void OnBorderStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [BorderStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCancelRowEdit (QuestionEventArgs e) {
			QuestionEventHandler eh = (QuestionEventHandler)(Events [CancelRowEditEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellBeginEdit (DataGridViewCellCancelEventArgs e)
		{
			DataGridViewCellCancelEventHandler eh = (DataGridViewCellCancelEventHandler)(Events [CellBeginEditEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellBorderStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [CellBorderStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellClick (DataGridViewCellEventArgs e)
		{
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);

			cell.OnClickInternal (e);
	
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellContentClick (DataGridViewCellEventArgs e)
		{
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);

			cell.OnContentClickInternal (e);
			
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellContentClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellContentDoubleClick (DataGridViewCellEventArgs e)
		{
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);

			cell.OnContentDoubleClickInternal (e);
			
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellContentDoubleClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellContextMenuStripChanged (DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellContextMenuStripChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellContextMenuStripNeeded (DataGridViewCellContextMenuStripNeededEventArgs e)
		{
			DataGridViewCellContextMenuStripNeededEventHandler eh = (DataGridViewCellContextMenuStripNeededEventHandler)(Events [CellContextMenuStripNeededEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellDoubleClick (DataGridViewCellEventArgs e)
		{
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);

			cell.OnDoubleClickInternal (e);
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellDoubleClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellEndEdit (DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellEndEditEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellEnter (DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellEnterEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnCellErrorTextChanged (DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellErrorTextChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellErrorTextNeeded (DataGridViewCellErrorTextNeededEventArgs e)
		{
			DataGridViewCellErrorTextNeededEventHandler eh = (DataGridViewCellErrorTextNeededEventHandler)(Events [CellErrorTextNeededEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal void OnCellFormattingInternal (DataGridViewCellFormattingEventArgs e)
		{
			OnCellFormatting (e);
		}

		protected virtual void OnCellFormatting (DataGridViewCellFormattingEventArgs e)
		{
			DataGridViewCellFormattingEventHandler eh = (DataGridViewCellFormattingEventHandler)(Events [CellFormattingEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellLeave (DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellLeaveEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellMouseClick (DataGridViewCellMouseEventArgs e)
		{
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);

			cell.OnMouseClickInternal (e);
			
			DataGridViewCellMouseEventHandler eh = (DataGridViewCellMouseEventHandler)(Events [CellMouseClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellMouseDoubleClick (DataGridViewCellMouseEventArgs e)
		{
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);

			cell.OnMouseDoubleClickInternal (e);
			
			DataGridViewCellMouseEventHandler eh = (DataGridViewCellMouseEventHandler)(Events [CellMouseDoubleClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellMouseDown (DataGridViewCellMouseEventArgs e)
		{

			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);

			cell.OnMouseDownInternal (e);
			
			DataGridViewCellMouseEventHandler eh = (DataGridViewCellMouseEventHandler)(Events [CellMouseDownEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellMouseEnter (DataGridViewCellEventArgs e)
		{
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);

			cell.OnMouseEnterInternal (e.RowIndex);
			
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellMouseEnterEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellMouseLeave (DataGridViewCellEventArgs e)
		{
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);

			cell.OnMouseLeaveInternal (e.RowIndex);
			
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellMouseLeaveEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellMouseMove (DataGridViewCellMouseEventArgs e)
		{
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);
			
			cell.OnMouseMoveInternal (e);
		
			DataGridViewCellMouseEventHandler eh = (DataGridViewCellMouseEventHandler)(Events [CellMouseMoveEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellMouseUp (DataGridViewCellMouseEventArgs e)
		{
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);

			cell.OnMouseUpInternal (e);
			
			DataGridViewCellMouseEventHandler eh = (DataGridViewCellMouseEventHandler)(Events [CellMouseUpEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal void OnCellPaintingInternal (DataGridViewCellPaintingEventArgs e)
		{
			OnCellPainting (e);
		}

		protected virtual void OnCellPainting (DataGridViewCellPaintingEventArgs e)
		{
			DataGridViewCellPaintingEventHandler eh = (DataGridViewCellPaintingEventHandler)(Events [CellPaintingEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnCellParsing (DataGridViewCellParsingEventArgs e)
		{
			DataGridViewCellParsingEventHandler eh = (DataGridViewCellParsingEventHandler)(Events [CellParsingEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellStateChanged (DataGridViewCellStateChangedEventArgs e)
		{
			DataGridViewCellStateChangedEventHandler eh = (DataGridViewCellStateChangedEventHandler)(Events [CellStateChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellStyleChanged (DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellStyleContentChanged (DataGridViewCellStyleContentChangedEventArgs e) {
			DataGridViewCellStyleContentChangedEventHandler eh = (DataGridViewCellStyleContentChangedEventHandler)(Events [CellStyleContentChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellToolTipTextChanged (DataGridViewCellEventArgs e) {
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellToolTipTextChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellToolTipTextNeeded (DataGridViewCellToolTipTextNeededEventArgs e)
		{
			DataGridViewCellToolTipTextNeededEventHandler eh = (DataGridViewCellToolTipTextNeededEventHandler)(Events [CellToolTipTextNeededEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellValidated (DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellValidatedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellValidating (DataGridViewCellValidatingEventArgs e)
		{
			DataGridViewCellValidatingEventHandler eh = (DataGridViewCellValidatingEventHandler)(Events [CellValidatingEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellValueChanged (DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [CellValueChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellValueNeeded (DataGridViewCellValueEventArgs e)
		{
			DataGridViewCellValueEventHandler eh = (DataGridViewCellValueEventHandler)(Events [CellValueNeededEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCellValuePushed (DataGridViewCellValueEventArgs e)
		{
			DataGridViewCellValueEventHandler eh = (DataGridViewCellValueEventHandler)(Events [CellValuePushedEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal void OnColumnAddedInternal (DataGridViewColumnEventArgs e)
		{
			AutoResizeColumnsInternal ();
			OnColumnAdded (e);
		}

		protected virtual void OnColumnAdded (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnAddedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnContextMenuStripChanged (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnContextMenuStripChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnDataPropertyNameChanged (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnDataPropertyNameChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnDefaultCellStyleChanged (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnDefaultCellStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnDisplayIndexChanged (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnDisplayIndexChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnColumnDividerDoubleClick (DataGridViewColumnDividerDoubleClickEventArgs e)
		{
			DataGridViewColumnDividerDoubleClickEventHandler eh = (DataGridViewColumnDividerDoubleClickEventHandler)(Events [ColumnDividerDoubleClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnDividerWidthChanged (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnDividerWidthChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnHeaderCellChanged (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnHeaderCellChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnColumnHeaderMouseClick (DataGridViewCellMouseEventArgs e)
		{
			DataGridViewCellMouseEventHandler eh = (DataGridViewCellMouseEventHandler)(Events [ColumnHeaderMouseClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnColumnHeaderMouseDoubleClick (DataGridViewCellMouseEventArgs e)
		{
			DataGridViewCellMouseEventHandler eh = (DataGridViewCellMouseEventHandler)(Events [ColumnHeaderMouseDoubleClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnColumnHeadersBorderStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ColumnHeadersBorderStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnColumnHeadersDefaultCellStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ColumnHeadersDefaultCellStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnColumnHeadersHeightChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ColumnHeadersHeightChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnColumnHeadersHeightSizeModeChanged (DataGridViewAutoSizeModeEventArgs e)
		{
			DataGridViewAutoSizeModeEventHandler eh = (DataGridViewAutoSizeModeEventHandler)(Events [ColumnHeadersHeightSizeModeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnMinimumWidthChanged (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnMinimumWidthChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnNameChanged (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnNameChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnColumnRemoved (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnRemovedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnSortModeChanged (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnSortModeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnStateChanged (DataGridViewColumnStateChangedEventArgs e)
		{
			DataGridViewColumnStateChangedEventHandler eh = (DataGridViewColumnStateChangedEventHandler)(Events [ColumnStateChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnToolTipTextChanged (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnToolTipTextChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnColumnWidthChanged (DataGridViewColumnEventArgs e)
		{
			DataGridViewColumnEventHandler eh = (DataGridViewColumnEventHandler)(Events [ColumnWidthChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCurrentCellChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [CurrentCellChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCurrentCellDirtyStateChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [CurrentCellDirtyStateChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnCursorChanged (EventArgs e)
		{
			base.OnCursorChanged (e);
		}

		protected virtual void OnDataBindingComplete (DataGridViewBindingCompleteEventArgs e)
		{
			DataGridViewBindingCompleteEventHandler eh = (DataGridViewBindingCompleteEventHandler)(Events [DataBindingCompleteEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDataError (bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e) {
			DataGridViewDataErrorEventHandler eh = (DataGridViewDataErrorEventHandler)(Events [DataErrorEvent]);
			if (eh != null) {
				eh (this, e);
			}
			else {
				if (displayErrorDialogIfNoHandler) {
					/////////////////////////////////// ERROR DIALOG //////////////////////////////////7
				}
			}
		}
		protected virtual void OnDataMemberChanged (EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DataMemberChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDataSourceChanged (EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DataSourceChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDefaultCellStyleChanged (EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DefaultCellStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDefaultValuesNeeded (DataGridViewRowEventArgs e) {
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [DefaultValuesNeededEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnDoubleClick (EventArgs e) {
			base.OnDoubleClick(e);
		}

		protected virtual void OnEditingControlShowing (DataGridViewEditingControlShowingEventArgs e) {
			DataGridViewEditingControlShowingEventHandler eh = (DataGridViewEditingControlShowingEventHandler)(Events [EditingControlShowingEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnEditModeChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [EditModeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged(e);
		}

		protected override void OnEnter (EventArgs e )
		{
			base.OnEnter(e);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus (e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged(e);
		}

		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged(e);
		}

		protected virtual void OnGridColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [GridColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated(e);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnKeyDown (KeyEventArgs e)
		{
			base.OnKeyDown(e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			base.OnKeyPress(e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnKeyUp (KeyEventArgs e)
		{
			base.OnKeyUp(e);
		}

		protected override void OnLayout (LayoutEventArgs e)
		{
			base.OnLayout(e);
		}

		protected override void OnLeave (EventArgs e)
		{
			base.OnLeave(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus (e);
		}

		protected override void OnMouseClick (MouseEventArgs e)
		{
			base.OnMouseClick(e);
			//Console.WriteLine("Mouse: Clicks: {0}; Delta: {1}; X: {2}; Y: {3};", e.Clicks, e.Delta, e.X, e.Y);
			HitTestInfo hit = HitTest (e.X, e.Y);

			switch (hit.Type) 
			{
			case DataGridViewHitTestType.Cell:
				Rectangle display = GetCellDisplayRectangle (hit.ColumnIndex, hit.RowIndex, false);
				OnCellMouseClick (new DataGridViewCellMouseEventArgs (hit.ColumnIndex, hit.RowIndex, e.X - display.X, e.Y - display.Y, e));
				break;
			
			}
		}

		protected override void OnMouseDoubleClick (MouseEventArgs e)
		{
			base.OnMouseDoubleClick(e);
		}

		private void DoSelectionOnMouseDown (HitTestInfo hitTest)
		{
			Keys modifiers = Control.ModifierKeys;
			bool isControl = (modifiers & Keys.Control) != 0;
			bool isShift = (modifiers & Keys.Shift) != 0;
			bool isRowHeader = hitTest.Type == DataGridViewHitTestType.RowHeader;
			bool isColHeader = hitTest.Type == DataGridViewHitTestType.ColumnHeader;
			DataGridViewSelectionMode mode;
			
			switch (hitTest.Type) {
			case DataGridViewHitTestType.Cell:
				mode = selectionMode;
				break;
			case DataGridViewHitTestType.ColumnHeader:
				mode = selectionMode == DataGridViewSelectionMode.ColumnHeaderSelect ? DataGridViewSelectionMode.FullColumnSelect : selectionMode;
				break;
			case DataGridViewHitTestType.RowHeader:
				mode = selectionMode == DataGridViewSelectionMode.RowHeaderSelect ?  DataGridViewSelectionMode.FullRowSelect : selectionMode;
				break; // Handled below
			default:
				return;
			}
			
			if (!isControl) {
				// If SHIFT is pressed:
				//	Select all from selected_row/column/cell to current row/column/cell, unselect everything else
				// otherwise:
				//	Unselect all rows/columns/cells, select the clicked one
				int min_row, max_row;
				int min_col, max_col;
				if (!isShift) {
					selected_row = hitTest.RowIndex;
					selected_column = hitTest.ColumnIndex;
				} 
				if (!isShift) {
					if (selected_row != -1)
						selected_row = hitTest.RowIndex;
					if (selected_column != -1)
						selected_column = hitTest.ColumnIndex;
				}
				if (selected_row >= hitTest.RowIndex) {
					min_row = hitTest.RowIndex;
					max_row = isShift ? selected_row : min_row;
				} else {
					max_row = hitTest.RowIndex;
					min_row = isShift ? selected_row : max_row;
				}
				if (selected_column >= hitTest.ColumnIndex) {
					min_col = hitTest.ColumnIndex;
					max_col = isShift ? selected_column : min_col;
				} else {
					max_col = hitTest.ColumnIndex;
					min_col = isShift ? selected_column : max_col;
				}

				switch (mode) {
				case DataGridViewSelectionMode.FullRowSelect:
					for (int i = 0; i < RowCount; i++) {
						bool select = i >= min_row && i <= max_row;
						if (!select) {
							for (int c = 0; c < ColumnCount; c++) {
								if (Rows [i].Cells [c].Selected) {
									SetSelectedCellCore (c, i, false);
								}
							}
						}
						if (select != Rows [i].Selected) {
							SetSelectedRowCore (i, select);
						}
					}
					break;
				case DataGridViewSelectionMode.FullColumnSelect:
					for (int i = 0; i < ColumnCount; i++) {
						bool select = i >= min_col && i <= max_col;
						if (!select) {
							for (int r = 0; r < RowCount; r++) {
								if (Rows [r].Cells [i].Selected) {
									SetSelectedCellCore (i, r, false);
								}
							}
						}
						if (select != Columns [i].Selected) {
							SetSelectedColumnCore (i, select);
						}
					}
					break;
				case DataGridViewSelectionMode.ColumnHeaderSelect:
				case DataGridViewSelectionMode.RowHeaderSelect:
					//break;
				case DataGridViewSelectionMode.CellSelect:
					if (!isShift) {
						for (int c = 0; c < ColumnCount; c++) {
							if (columns [c].Selected)
								SetSelectedColumnCore (c, false);
						}
						
						for (int r = 0; r < RowCount; r++) {
							if (rows [r].Selected)
								SetSelectedRowCore (r, false);
						}
					}
					for (int r = 0; r < RowCount; r++) {
						for (int c = 0; c < ColumnCount; c++) {
							bool select = (r >= min_row && r <= max_row) && (c >= min_col && c <= max_col);
							if (select != Rows [r].Cells [c].Selected)
								SetSelectedCellCore (c, r, select);
						}
					}
					break;
				}
				
			} else if (isControl) {
				// Switch the selected state of the row.
				switch (mode) {
				case DataGridViewSelectionMode.FullRowSelect:
					SetSelectedRowCore (hitTest.RowIndex, !rows [hitTest.RowIndex].Selected);
					break;
				case DataGridViewSelectionMode.FullColumnSelect:
					SetSelectedColumnCore (hitTest.ColumnIndex, !columns [hitTest.ColumnIndex].Selected);
					break;
				case DataGridViewSelectionMode.ColumnHeaderSelect:
				case DataGridViewSelectionMode.RowHeaderSelect:
					//break;
				case DataGridViewSelectionMode.CellSelect:
					if (hitTest.ColumnIndex >= 0 && hitTest.RowIndex >= 0) {
						SetSelectedCellCore (hitTest.ColumnIndex, hitTest.RowIndex, !Rows [hitTest.RowIndex].Cells [hitTest.ColumnIndex].Selected);
					}
					break;
				}
			}
			
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown(e);
			
			HitTestInfo hitTest = HitTest(e.X, e.Y);
			
			DataGridViewCell cell = null;
			DataGridViewRow row = null;
			Rectangle cellBounds;

			if (hitTest.Type == DataGridViewHitTestType.Cell) {
				cellBounds = GetCellDisplayRectangle (hitTest.ColumnIndex, hitTest.RowIndex, false);
				OnCellMouseDown (new DataGridViewCellMouseEventArgs (hitTest.ColumnIndex, hitTest.RowIndex, e.X - cellBounds.X, e.Y - cellBounds.Y, e));
				OnCellClick (new DataGridViewCellEventArgs (hitTest.ColumnIndex, hitTest.RowIndex));
				row = rows [hitTest.RowIndex];
				cell = row.Cells [hitTest.ColumnIndex];
			}
			
			DoSelectionOnMouseDown (hitTest);
			
			if (hitTest.Type != DataGridViewHitTestType.Cell) {
				Invalidate ();
				return;
			}
			
			if (cell == currentCell) {
				BeginEdit (true);
			} else if (currentCell != null) {
				EndEdit ();
				OnCellLeave(new DataGridViewCellEventArgs(currentCell.ColumnIndex, currentCell.RowIndex));
			}
			currentCell = cell;
			OnCurrentCellChanged(EventArgs.Empty);
			OnCellEnter(new DataGridViewCellEventArgs(cell.ColumnIndex, cell.RowIndex));
			if (editMode == DataGridViewEditMode.EditOnEnter) {
				BeginEdit (true);
			}
			Invalidate();
			return;
		}

		protected override void OnMouseEnter (EventArgs e)
		{
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave(e);
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove(e);
			HitTestInfo hit = this.HitTest (e.X, e.Y);
			
			switch (hit.Type)
			{
			case DataGridViewHitTestType.Cell:
				Rectangle display = GetCellDisplayRectangle (hit.ColumnIndex, hit.RowIndex, false);
				OnCellMouseMove (new DataGridViewCellMouseEventArgs (hit.ColumnIndex, hit.RowIndex, e.X - display.X, e.Y - display.Y, e));
				break;
			case DataGridViewHitTestType.ColumnHeader:
			case DataGridViewHitTestType.RowHeader:
			case DataGridViewHitTestType.TopLeftHeader:
			
			case DataGridViewHitTestType.HorizontalScrollBar:
			case DataGridViewHitTestType.VerticalScrollBar:
			
			
			case DataGridViewHitTestType.None:
				break;
			}
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseUp(e);
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel(e);
		}

		protected virtual void OnMultiSelectChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [MultiSelectChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnNewRowNeeded (DataGridViewRowEventArgs e) {
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [NewRowNeededEvent]);
			if (eh != null) eh (this, e);
		}

		protected override void OnPaint (PaintEventArgs e) {
			base.OnPaint(e);
			//Console.WriteLine("DataGridView.OnPaint-ClipRectangle: {0};", e.ClipRectangle);
			Rectangle bounds = ClientRectangle; //e.ClipRectangle;
			PaintBackground (e.Graphics, e.ClipRectangle, bounds);
			
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
					DataGridViewAdvancedBorderStyle intermediateBorderStyle = (DataGridViewAdvancedBorderStyle) ((ICloneable)this.AdvancedColumnHeadersBorderStyle).Clone();
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
					DataGridViewAdvancedBorderStyle intermediateBorderStyle = (DataGridViewAdvancedBorderStyle) ((ICloneable)this.AdvancedRowHeadersBorderStyle).Clone();
					DataGridViewAdvancedBorderStyle borderStyle = cell.AdjustCellBorderStyle(this.AdvancedRowHeadersBorderStyle, intermediateBorderStyle, true, true, false, cell.RowIndex == 0);
					cell.InternalPaint(e.Graphics, e.ClipRectangle, rowHeaderBounds, cell.RowIndex, cell.State, cell.Value, cell.FormattedValue, cell.ErrorText, style, borderStyle, DataGridViewPaintParts.All);
					//e.Graphics.FillRectangle(new SolidBrush(rowHeadersDefaultCellStyle.BackColor), rowHeadersBounds);
					bounds.X += rowHeadersWidth;
				}
				bounds.Height = row.Height;
				row.Paint (e.Graphics, e.ClipRectangle, bounds, row.Index, row.State, row.Index == 0, row.Index == rows.Count - 1);
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
				if (verticalScrollBar.Visible && (gridWidth + verticalScrollBar.Width) > Size.Width) {
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
		}

		protected virtual void OnReadOnlyChanged (EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ReadOnlyChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnResize (EventArgs e) {
			base.OnResize(e);
			horizontalScrollingOffset = ((gridWidth - Size.Width) > 0)? (gridWidth - Size.Width) : 0;
			verticalScrollingOffset = ((gridHeight - Size.Height) > 0)? (gridHeight - Size.Height) : 0;
			AutoResizeColumnsInternal ();
		}

		protected override void OnRightToLeftChanged (EventArgs e) {
			base.OnRightToLeftChanged(e);
		}

		// In MSDN2 documentation there's no internal here
		protected internal virtual void OnRowContextMenuStripChanged (DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [RowContextMenuStripChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowContextMenuStripNeeded (DataGridViewRowContextMenuStripNeededEventArgs e)
		{
			DataGridViewRowContextMenuStripNeededEventHandler eh = (DataGridViewRowContextMenuStripNeededEventHandler)(Events [RowContextMenuStripNeededEvent]);
			if (eh != null) eh (this, e);
		}

		protected internal virtual void OnRowDefaultCellStyleChanged (DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [RowDefaultCellStyleChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowDirtyStateNeeded (QuestionEventArgs e)
		{
			QuestionEventHandler eh = (QuestionEventHandler)(Events [RowDirtyStateNeededEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowDividerDoubleClick (DataGridViewRowDividerDoubleClickEventArgs e)
		{
			DataGridViewRowDividerDoubleClickEventHandler eh = (DataGridViewRowDividerDoubleClickEventHandler)(Events [RowDividerDoubleClickEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowDividerHeightChanged (DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [RowDividerHeightChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowEnter (DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [RowEnterEvent]);
			if (eh != null) eh (this, e);
		}

		protected internal virtual void OnRowErrorTextChanged (DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [RowErrorTextChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowErrorTextNeeded (DataGridViewRowErrorTextNeededEventArgs e)
		{
			DataGridViewRowErrorTextNeededEventHandler eh = (DataGridViewRowErrorTextNeededEventHandler)(Events [RowErrorTextNeededEvent]);
			if (eh != null) eh (this, e);
		}

		protected internal virtual void OnRowHeaderCellChanged (DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [RowHeaderCellChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowHeaderMouseClick (DataGridViewCellMouseEventArgs e)
		{
			DataGridViewCellMouseEventHandler eh = (DataGridViewCellMouseEventHandler)(Events [RowHeaderMouseClickEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowHeaderMouseDoubleClick (DataGridViewCellMouseEventArgs e)
		{
			DataGridViewCellMouseEventHandler eh = (DataGridViewCellMouseEventHandler)(Events [RowHeaderMouseDoubleClickEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowHeadersBorderStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [RowHeadersBorderStyleChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowHeadersDefaultCellStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [RowHeadersDefaultCellStyleChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowHeadersWidthChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [RowHeadersWidthChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowHeadersWidthSizeModeChanged (DataGridViewAutoSizeModeEventArgs e)
		{
			DataGridViewAutoSizeModeEventHandler eh = (DataGridViewAutoSizeModeEventHandler)(Events [RowHeadersWidthSizeModeChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected internal virtual void OnRowHeightChanged (DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [RowHeightChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowHeightInfoNeeded (DataGridViewRowHeightInfoNeededEventArgs e)
		{
			DataGridViewRowHeightInfoNeededEventHandler eh = (DataGridViewRowHeightInfoNeededEventHandler)(Events [RowHeightInfoNeededEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowHeightInfoPushed (DataGridViewRowHeightInfoPushedEventArgs e)
		{
			DataGridViewRowHeightInfoPushedEventHandler eh = (DataGridViewRowHeightInfoPushedEventHandler)(Events [RowHeightInfoPushedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowLeave (DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [RowLeaveEvent]);
			if (eh != null) eh (this, e);
		}

		protected internal virtual void OnRowMinimumHeightChanged (DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [RowMinimumHeightChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected internal virtual void OnRowPostPaint (DataGridViewRowPostPaintEventArgs e)
		{
			DataGridViewRowPostPaintEventHandler eh = (DataGridViewRowPostPaintEventHandler)(Events [RowPostPaintEvent]);
			if (eh != null) eh (this, e);
		}

		protected internal virtual void OnRowPrePaint (DataGridViewRowPrePaintEventArgs e)
		{
			DataGridViewRowPrePaintEventHandler eh = (DataGridViewRowPrePaintEventHandler)(Events [RowPrePaintEvent]);
			if (eh != null) eh (this, e);
		}

		internal void OnRowsAddedInternal (DataGridViewRowsAddedEventArgs e)
		{
			Invalidate ();
			OnRowsAdded (e);
		}

		protected internal virtual void OnRowsAdded (DataGridViewRowsAddedEventArgs e)
		{
			DataGridViewRowsAddedEventHandler eh = (DataGridViewRowsAddedEventHandler)(Events [RowsAddedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowsDefaultCellStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [RowsDefaultCellStyleChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected internal virtual void OnRowsRemoved (DataGridViewRowsRemovedEventArgs e)
		{
			DataGridViewRowsRemovedEventHandler eh = (DataGridViewRowsRemovedEventHandler)(Events [RowsRemovedEvent]);
			if (eh != null) eh (this, e);
		}

		protected internal virtual void OnRowStateChanged (int rowIndex, DataGridViewRowStateChangedEventArgs e)
		{
			DataGridViewRowStateChangedEventHandler eh = (DataGridViewRowStateChangedEventHandler)(Events [RowStateChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowUnshared (DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [RowUnsharedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowValidated (DataGridViewCellEventArgs e)
		{
			DataGridViewCellEventHandler eh = (DataGridViewCellEventHandler)(Events [RowValidatedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowValidating (DataGridViewCellCancelEventArgs e)
		{
			DataGridViewCellCancelEventHandler eh = (DataGridViewCellCancelEventHandler)(Events [RowValidatingEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnScroll (ScrollEventArgs e)
		{
			ScrollEventHandler eh = (ScrollEventHandler)(Events [ScrollEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnSelectionChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [SelectionChangedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnSortCompare (DataGridViewSortCompareEventArgs e) {
			DataGridViewSortCompareEventHandler eh = (DataGridViewSortCompareEventHandler)(Events [SortCompareEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnSorted (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [SortedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnUserAddedRow (DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [UserAddedRowEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnUserDeletedRow (DataGridViewRowEventArgs e)
		{
			DataGridViewRowEventHandler eh = (DataGridViewRowEventHandler)(Events [UserDeletedRowEvent]);
			if (eh != null) eh (this, e);

		}

		protected virtual void OnUserDeletingRow (DataGridViewRowCancelEventArgs e)
		{
			DataGridViewRowCancelEventHandler eh = (DataGridViewRowCancelEventHandler)(Events [UserDeletingRowEvent]);
			if (eh != null) eh (this, e);
		}

		protected override void OnValidating (CancelEventArgs e)
		{
			base.OnValidating(e);
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged(e);
		}

		protected virtual void PaintBackground (Graphics graphics, Rectangle clipBounds, Rectangle gridBounds)
		{
			graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (backgroundColor), gridBounds);
		}

		protected bool ProcessAKey (Keys keyData)
		{
			throw new NotImplementedException();
		}

		protected virtual bool ProcessDataGridViewKey (KeyEventArgs e)
		{
			throw new NotImplementedException();
		}

		protected bool ProcessDeleteKey (Keys keyData)
		{
			throw new NotImplementedException();
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			return base.ProcessDialogKey(keyData);
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
			rows [rowIndex].Cells [columnIndex].Selected = selected;
		}

		internal void SetSelectedColumnCoreInternal (int columnIndex, bool selected) {
			SetSelectedColumnCore (columnIndex, selected);
		}	
		
		protected virtual void SetSelectedColumnCore (int columnIndex, bool selected) {
			DataGridViewColumn col = columns [columnIndex];
			
			col.SelectedInternal = selected;
			
			if (selected_columns == null)
				selected_columns = new DataGridViewSelectedColumnCollection ();
			
			if (!selected && selected_columns.Contains (col)) {
				selected_columns.InternalRemove (col);
			} else if (selected && !selected_columns.Contains (col)) {
				selected_columns.InternalAdd (col);
			}
		}

		internal void SetSelectedRowCoreInternal (int rowIndex, bool selected) {
			SetSelectedRowCore (rowIndex, selected);
		}	

		protected virtual void SetSelectedRowCore (int rowIndex, bool selected) {
			DataGridViewRow row = rows [rowIndex];
			
			row.SelectedInternal = selected;
			
			if (selected_rows == null)
				selected_rows = new DataGridViewSelectedRowCollection ();
				
			if (!selected && selected_rows.Contains (row)) {
				selected_rows.InternalRemove (row);
			} else if (selected && !selected_rows.Contains (row)) {
				selected_rows.InternalAdd (row);
			}
		}

		protected override void WndProc (ref Message m) {
			base.WndProc(ref m);
		}

		void IDropTarget.OnDragDrop (DragEventArgs e)
		{
			throw new NotImplementedException ();
		}

		void IDropTarget.OnDragEnter (DragEventArgs e)
		{
			throw new NotImplementedException ();
		}

		void IDropTarget.OnDragLeave (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		void IDropTarget.OnDragOver (DragEventArgs e)
		{
			throw new NotImplementedException ();
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

		// Resizes all columns according to their AutoResizeMode property.
		// First all the columns that aren't Filled are resized, then we resize all the Filled columns.
		internal void AutoResizeColumnsInternal ()
		{
			for (int i = 0; i < Columns.Count; i++)
				AutoResizeColumnInternal (i, Columns [i].InheritedAutoSizeMode);
			
			AutoFillColumnsInternal ();
		}

		internal void AutoFillColumnsInternal ()
		{
			float totalFillWeight = 0;
			int FillCount = 0; // The number of columns that has AutoSizeMode.Fill set
			int spaceLeft = ClientSize.Width;

			if (RowHeadersVisible) {
				spaceLeft -= RowHeadersWidth;
			}
			spaceLeft -= BorderWidth.Width * 2;
			
			int [] fixed_widths = new int [Columns.Count];
			int [] new_widths = new int [Columns.Count];
			bool fixed_any = false;
			
			for (int i = 0; i < Columns.Count; i++) {
				DataGridViewColumn col = Columns [i];

				switch (col.InheritedAutoSizeMode) {
				case DataGridViewAutoSizeColumnMode.Fill:
					FillCount++;
					totalFillWeight += col.FillWeight;
					break;
				case DataGridViewAutoSizeColumnMode.AllCellsExceptHeader:
				case DataGridViewAutoSizeColumnMode.AllCells:
				case DataGridViewAutoSizeColumnMode.DisplayedCells:
				case DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader:
				case DataGridViewAutoSizeColumnMode.None:
				case DataGridViewAutoSizeColumnMode.NotSet:
					spaceLeft -= Columns [i].Width;
					break;
				}
			}

			spaceLeft = Math.Max (0, spaceLeft);
			
			do {
				fixed_any = false;
				for (int i = 0; i < columns.Count; i++) {
					DataGridViewColumn col = Columns [i];
					int width;
					
					if (col.InheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.Fill)
						continue;
						
					if (fixed_widths [i] != 0)
						continue;
					
					width = (totalFillWeight == 0) ? 0 : (int) Math.Round (spaceLeft * (col.FillWeight / totalFillWeight), 0);
					
					if (width < 0)
						width = 0;
					
					if (width < col.MinimumWidth) {
						width = col.MinimumWidth;
						fixed_widths [i] = width;
						fixed_any = true;
						spaceLeft -= width;
						totalFillWeight -= col.FillWeight;
					}

					new_widths [i] = width;
				}
			} while (fixed_any);

			for (int i = 0; i < columns.Count; i++) {
				if (Columns [i].InheritedAutoSizeMode != DataGridViewAutoSizeColumnMode.Fill)
					continue;
					
				Columns [i].Width = new_widths [i];
			}
		}

		internal void AutoResizeColumnInternal (int columnIndex, DataGridViewAutoSizeColumnMode mode)
		{
			// http://msdn2.microsoft.com/en-us/library/ms171605.aspx
			int size = 0;

			DataGridViewColumn col = Columns [columnIndex];
			
			switch (mode) {
			case DataGridViewAutoSizeColumnMode.Fill:
				return;
			case DataGridViewAutoSizeColumnMode.AllCellsExceptHeader:
			case DataGridViewAutoSizeColumnMode.AllCells:
			case DataGridViewAutoSizeColumnMode.DisplayedCells:
			case DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader:
				size  = CalculateColumnCellWidth (columnIndex, col.InheritedAutoSizeMode);
				break;
			case DataGridViewAutoSizeColumnMode.ColumnHeader:
				size = col.HeaderCell.ContentBounds.Width;
				break;
			default:
				size = col.Width;
				break;
			}

			if (size < 0)
				size = 0;
			if (size < col.MinimumWidth)
				size = col.MinimumWidth;

			col.Width = size;
		}
		
		internal int CalculateColumnCellWidth (int index, DataGridViewAutoSizeColumnMode mode)
		{
			int first_row = 0;
			int result = 0;
			bool only_visible = false;
			
			if (mode == DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader || 
				mode == DataGridViewAutoSizeColumnMode.AllCellsExceptHeader)
				first_row++;
			
			only_visible = (mode == DataGridViewAutoSizeColumnMode.DisplayedCells || mode == DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader);
			
			for (int i = first_row; i < Rows.Count; i++) {
				if (only_visible) {
					Rectangle row_rect = this.GetRowDisplayRectangle (i, false);
					if (!ClientRectangle.IntersectsWith (row_rect))
						continue;
				}
				
				Rectangle cell_rect = GetCellDisplayRectangle (index, i, false);
				
				result = Math.Max (result, cell_rect.Width);
			}
			
			return result;
		}

		private void BindIList (IList list) {
			if (list is DataView) {
				DataView dataView = (DataView) list;
				DataTable table = dataView.Table;
				DataGridViewCell template = new DataGridViewTextBoxCell();
				foreach (DataColumn dataColumn in table.Columns) {
					DataGridViewColumn col = new DataGridViewColumn(template);
					col.Name = dataColumn.ColumnName;
					col.ValueType = dataColumn.DataType;
					columns.Add(col);
				}
				dataView.ListChanged += OnListChanged;
			}
			else if (list.Count > 0) {
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

		private void OnListChanged (object sender, ListChangedEventArgs args) {
			if (args.OldIndex >= 0) {
			}
			if (args.NewIndex >= 0) {
				object element = (sender as DataView)[args.NewIndex];
				DataGridViewRow row = new DataGridViewRow();
				rows.InternalAdd(row);
				PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(element);
				foreach (PropertyDescriptor property in properties) {
					DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
					cell.Value = property.GetValue(element);
					cell.ValueType = property.PropertyType;
					row.Cells.Add(cell);
				}
				Invalidate();
			}
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

		public class DataGridViewControlCollection : Control.ControlCollection
		{
			private new DataGridView owner;
			
			public DataGridViewControlCollection (DataGridView owner) : base (owner)
			{
				this.owner = owner;
			}
			
			public override void Clear ()
			{
				// 
				// This is severely buggy, just as MS' implementation is.
				//
				for (int i = 0; i < Count; i++) {
					Remove (this [i]);
				}
			}
			
			public void CopyTo (Control [] array, int index)
			{
				base.CopyTo (array, index);
			}
			
			public void Insert (int index, Control value)
			{
				throw new NotSupportedException ();
			}

			public override void Remove (Control value)
			{
				if (value == owner.horizontalScrollBar)
					return;
				
				if (value == owner.verticalScrollBar)
					return;
					
				if (value == owner.editingControl)
					return;
			
				base.Remove (value);
			}
			
			internal void RemoveInternal (Control value)
			{
				base.Remove (value);
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
