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

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Reflection;
using System.Collections;
using System.Text;
using System.Collections.Generic;

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
		private Point anchor_cell;
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
		//private DataGridViewRow currentRow;
		private string dataMember;
		private object dataSource;
		private DataGridViewCellStyle defaultCellStyle;
		//private Control editingControl;
		private DataGridViewEditMode editMode;
		private bool enableHeadersVisualStyles = true;
		private DataGridViewCell firstDisplayedCell;
		private int firstDisplayedScrollingColumnHiddenWidth;
		private int firstDisplayedScrollingColumnIndex;
		private int firstDisplayedScrollingRowIndex;
		private Color gridColor = Color.FromKnownColor(KnownColor.ControlDark);
		private int horizontalScrollingOffset;
		private DataGridViewCell hover_cell = null;
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
		private bool is_autogenerating_columns = false;
		private bool is_binding = false;
		private bool new_row_editing = false;
		
		// These are used to implement selection behaviour with SHIFT pressed.
		private int selected_row = -1;
		private int selected_column = -1;
		
		// Stuff for error Tooltips
		private Timer tooltip_timer;
		private ToolTip tooltip_window;
		private DataGridViewCell tooltip_currently_showing;

		private DataGridViewSelectedRowCollection selected_rows;
		private DataGridViewSelectedColumnCollection selected_columns;
		private DataGridViewRow editing_row;
		
		DataGridViewHeaderCell pressed_header_cell;
		DataGridViewHeaderCell entered_header_cell;

		// For column/row resizing via mouse
		private bool column_resize_active = false;
		private bool row_resize_active = false;
		private int resize_band = -1;
		private int resize_band_start = 0;
		private int resize_band_delta = 0;
		
		public DataGridView ()
		{
			SetStyle (ControlStyles.Opaque, true);
			//SetStyle (ControlStyles.UserMouse, true);
			SetStyle (ControlStyles.OptimizedDoubleBuffer, true);
			
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
			cellBorderStyle = DataGridViewCellBorderStyle.Single;
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
			currentCellAddress = new Point (-1, -1);
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
			firstDisplayedScrollingColumnHiddenWidth = 0;
			isCurrentCellDirty = false;
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
			scrollBars = ScrollBars.Both;
			userSetCursor = Cursor.Current;
			virtualMode = false;

			horizontalScrollBar = new HScrollBar();
			horizontalScrollBar.Scroll += OnHScrollBarScroll;
			horizontalScrollBar.Visible = false;
			
			verticalScrollBar = new VScrollBar();
			verticalScrollBar.Scroll += OnVScrollBarScroll;
			verticalScrollBar.Visible = false;
			
			Controls.AddRange (new Control[] {horizontalScrollBar, verticalScrollBar});
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
			get { 
				if (allowUserToAddRows && DataManager != null)
					return DataManager.AllowNew;
				return allowUserToAddRows;
			}
			set {
				if (allowUserToAddRows != value) {
					allowUserToAddRows = value;
					if (!value) {
						if (new_row_editing)
							CancelEdit ();
						RemoveEditingRow ();
					} else {
						PrepareEditingRow (false, false);
					}
					OnAllowUserToAddRowsChanged(EventArgs.Empty);
					Invalidate ();
				}
			}
		}

		[DefaultValue (true)]
		public bool AllowUserToDeleteRows {
			get {
				if (allowUserToDeleteRows && DataManager != null)
					return DataManager.AllowRemove;
				return allowUserToDeleteRows;
			}
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
				AutoResizeColumns (value);
				Invalidate ();
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
					
					if (value == DataGridViewAutoSizeRowsMode.None)
						foreach (DataGridViewRow row in Rows)
							row.ResetToExplicitHeight ();
					else
						AutoResizeRows (value);
						
					OnAutoSizeRowsModeChanged(new DataGridViewAutoSizeModeEventArgs(false));
					Invalidate ();
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

		internal int BorderWidth {
			get {
				switch (BorderStyle) {
				case BorderStyle.Fixed3D:
					return 2;
				case BorderStyle.FixedSingle:
					return 1;
				case BorderStyle.None:
				default:
					return 0;
				}
			}
		}

		[Browsable (true)]
		[DefaultValue (DataGridViewCellBorderStyle.Single)]
		public DataGridViewCellBorderStyle CellBorderStyle {
			get { return cellBorderStyle; }
			set {
				if (cellBorderStyle != value) {
					if (value == DataGridViewCellBorderStyle.Custom)
						throw new ArgumentException ("CellBorderStyle cannot be set to Custom.");

					cellBorderStyle = value;

					DataGridViewAdvancedBorderStyle border = new DataGridViewAdvancedBorderStyle ();

					switch (cellBorderStyle) {
						case DataGridViewCellBorderStyle.Single:
							border.All = DataGridViewAdvancedCellBorderStyle.Single;
							break;
						case DataGridViewCellBorderStyle.Raised:
						case DataGridViewCellBorderStyle.RaisedVertical:
							border.Bottom = DataGridViewAdvancedCellBorderStyle.None;
							border.Top = DataGridViewAdvancedCellBorderStyle.None;
							border.Left = DataGridViewAdvancedCellBorderStyle.Outset;
							border.Right = DataGridViewAdvancedCellBorderStyle.Outset;
							break;
						case DataGridViewCellBorderStyle.Sunken:
							border.All = DataGridViewAdvancedCellBorderStyle.Inset;
							break;
						case DataGridViewCellBorderStyle.None:
							border.All = DataGridViewAdvancedCellBorderStyle.None;
							break;
						case DataGridViewCellBorderStyle.SingleVertical:
							border.Bottom = DataGridViewAdvancedCellBorderStyle.None;
							border.Top = DataGridViewAdvancedCellBorderStyle.None;
							border.Left = DataGridViewAdvancedCellBorderStyle.None;
							border.Right = DataGridViewAdvancedCellBorderStyle.Single;
							break;
						case DataGridViewCellBorderStyle.SunkenVertical:
							border.Bottom = DataGridViewAdvancedCellBorderStyle.None;
							border.Top = DataGridViewAdvancedCellBorderStyle.None;
							border.Left = DataGridViewAdvancedCellBorderStyle.Inset;
							border.Right = DataGridViewAdvancedCellBorderStyle.Inset;
							break;
						case DataGridViewCellBorderStyle.SingleHorizontal:
						case DataGridViewCellBorderStyle.SunkenHorizontal:
							border.Bottom = DataGridViewAdvancedCellBorderStyle.Inset;
							border.Top = DataGridViewAdvancedCellBorderStyle.Inset;
							border.Left = DataGridViewAdvancedCellBorderStyle.None;
							border.Right = DataGridViewAdvancedCellBorderStyle.None;
							break;
						case DataGridViewCellBorderStyle.RaisedHorizontal:
							border.Bottom = DataGridViewAdvancedCellBorderStyle.Outset;
							border.Top = DataGridViewAdvancedCellBorderStyle.Outset;
							border.Left = DataGridViewAdvancedCellBorderStyle.None;
							border.Right = DataGridViewAdvancedCellBorderStyle.None;
							break;
					}
					
					advancedCellBorderStyle = border;
					
					OnCellBorderStyleChanged (EventArgs.Empty);
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
					for (int i = columns.Count -1; i >= value; i--) {
						columns.RemoveAt(i);
					}
				}
				else if (value > columns.Count) {
					for (int i = columns.Count; i < value; i++) {
						DataGridViewTextBoxColumn col = new DataGridViewTextBoxColumn ();
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
					
					if (columnHeadersVisible)
						Invalidate ();
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
			set {
				if (columnHeadersVisible != value) {
					columnHeadersVisible = value;
					Invalidate ();
				}
			}
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
				if (value == null)
					MoveCurrentCell (-1, -1, true, false, false, true);
				else if (value.DataGridView != this)
					throw new ArgumentException("The cell is not in this DataGridView.");
				else
					MoveCurrentCell (value.OwningColumn.Index, value.OwningRow.Index, true, false, false, true);
			}
		}

		[Browsable (false)]
		public Point CurrentCellAddress {
			get { return currentCellAddress; }
		}

		[Browsable (false)]
		public DataGridViewRow CurrentRow {
			get { 
				if (currentCell != null)
					return currentCell.OwningRow;
				return null;
			}
		}

		[DefaultValue ("")]
		[Editor ("System.Windows.Forms.Design.DataMemberListEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string DataMember {
			get { return dataMember; }
			set {
				if (dataMember != value) {
					dataMember = value;
					if (BindingContext != null)
						ReBind ();
					OnDataMemberChanged(EventArgs.Empty);
				}
			}
		}

		[RefreshProperties (RefreshProperties.Repaint)]
		[DefaultValue (null)]
		[AttributeProvider (typeof (IListSource))]
		public object DataSource {
			get { return dataSource; }
			set {
				/* The System.Windows.Forms.DataGridView class supports the standard Windows Forms data-binding model. This means the data source can be of any type that implements:
				 - the System.Collections.IList interface, including one-dimensional arrays.
				 - the System.ComponentModel.IListSource interface, such as the System.Data.DataTable and System.Data.DataSet classes.
				 - the System.ComponentModel.IBindingList interface, such as the System.ComponentModel.Collections.BindingList<> class.
				 - the System.ComponentModel.IBindingListView interface, such as the System.Windows.Forms.BindingSource class.
				*/
				if (!(value == null || value is IList || value is IListSource || value is IBindingList || value is IBindingListView))
					throw new NotSupportedException ("Type cannot be bound.");
					
				if (value != DataSource) {
					if (IsHandleCreated && value != null && BindingContext != null && BindingContext[value] != null)
						DataMember = String.Empty;
					ClearBinding ();
	
	
					// Do not set dataSource prior to the BindingContext check because there is some lazy initialization 
					// code which might result in double call to ReBind here and in OnBindingContextChanged
					if (BindingContext != null) {
						dataSource = value;
						ReBind ();
					} else {
						dataSource = value;
					}

					OnDataSourceChanged (EventArgs.Empty);
				}
			}
		}

		internal CurrencyManager DataManager {
			get {
				if (DataSource != null && BindingContext != null) {
					string dataMember = DataMember;
					if (dataMember == null)
						dataMember = String.Empty;
					return (CurrencyManager) this.BindingContext[DataSource, dataMember];
				}
				return null;
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

		internal DataGridViewHeaderCell EnteredHeaderCell {
			get { return entered_header_cell; }
			set {
				if (entered_header_cell == value)
					return;
				if (ThemeEngine.Current.DataGridViewHeaderCellHasHotStyle (this)) {
					Region area_to_invalidate = new Region ();
					area_to_invalidate.MakeEmpty ();
					if (entered_header_cell != null)
						area_to_invalidate.Union (GetHeaderCellBounds (entered_header_cell));
					entered_header_cell = value;
					if (entered_header_cell != null)
						area_to_invalidate.Union (GetHeaderCellBounds (entered_header_cell));
					Invalidate (area_to_invalidate);
					area_to_invalidate.Dispose ();
				} else
					entered_header_cell = value;
			}
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
				if (!virtualMode)
					return IsCurrentCellDirty;

				QuestionEventArgs args = new QuestionEventArgs ();
				OnRowDirtyStateNeeded (args);
				return args.Response;
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
				if (!AllowUserToAddRows || ColumnCount == 0) {
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

		internal DataGridViewHeaderCell PressedHeaderCell {
			get { return pressed_header_cell; }
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
				if (value < 1 && AllowUserToAddRows) {
					throw new ArgumentException("RowCount must be >= 1 if AllowUserToAddRows is true.");
				}
				if (dataSource != null) {
					throw new InvalidOperationException("Cant change row count if DataSource is set.");
				}

				if (value < rows.Count) {
					int removeRangeEndIndex = rows.Count - 1;
					if (AllowUserToAddRows)
						removeRangeEndIndex--; // do not remove editing row

					int removeRangeStartIndex = value - 1;
					if (AllowUserToAddRows)
						removeRangeStartIndex--; // remove an extra row before/instead of the editing row

					for (int i = removeRangeEndIndex; i > removeRangeStartIndex; i--)
						rows.RemoveAt(i);
				} else if (value > rows.Count) {
					// If we need to add rows and don't have any columns,
					// we create one column
					if (ColumnCount == 0) {
						System.Diagnostics.Debug.Assert (rows.Count == 0);
						ColumnCount = 1; // this creates the edit row
						if (VirtualMode) {
							// update edit row height
							UpdateRowHeightInfo (0, false);
						}
					}

					List<DataGridViewRow> newRows = new List<DataGridViewRow> (value - rows.Count);
					for (int i = rows.Count; i < value; i++)
						newRows.Add ((DataGridViewRow) RowTemplateFull);
					rows.AddRange (newRows.ToArray());
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
			set {
				if (rowHeadersVisible != value) {
					rowHeadersVisible = value;
					Invalidate ();
				}
			}
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
					
					if (rowHeadersVisible)
						Invalidate ();
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

		// RowTemplate is just the row, it does not contain Cells
		[Browsable (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataGridViewRow RowTemplate {
			get {
				if (rowTemplate == null)
					rowTemplate = new DataGridViewRow ();

				return rowTemplate;
			}
			set {
				rowTemplate = value;
			}
		}

		// Take the RowTemplate, clone it, and add Cells
		// Note this is not stored, so you don't need to Clone it
		internal DataGridViewRow RowTemplateFull {
			get {
				DataGridViewRow row = (DataGridViewRow) RowTemplate.Clone ();
				
				for (int i = row.Cells.Count; i < Columns.Count; i++) {
					DataGridViewCell template = columns [i].CellTemplate;
					
					if (template == null)
						throw new InvalidOperationException ("At least one of the DataGridView control's columns has no cell template.");
					
					row.Cells.Add ((DataGridViewCell) template.Clone ());
				}
				
				return row;
			}
		}

		internal override bool ScaleChildrenInternal {
			get { return false; }
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
				PerformLayout ();
				Invalidate ();
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
				DataGridViewSelectedRowCollection result = new DataGridViewSelectedRowCollection (this);
				
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
				if (!Enum.IsDefined (typeof(DataGridViewSelectionMode), value))
					throw new InvalidEnumArgumentException ("Value is not valid DataGridViewSelectionMode.");

				if (value == DataGridViewSelectionMode.ColumnHeaderSelect || value == DataGridViewSelectionMode.FullColumnSelect)
					foreach (DataGridViewColumn col in Columns)
						if (col.SortMode == DataGridViewColumnSortMode.Automatic)
							throw new InvalidOperationException (string.Format ("Cannot set SelectionMode to {0} because there are Automatic sort columns.", value));
				
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
			get {
				if (topLeftHeaderCell == null) {
					topLeftHeaderCell = new DataGridViewTopLeftHeaderCell ();
					topLeftHeaderCell.SetDataGridView (this);
				}
				return topLeftHeaderCell;
			}
			set {
				if (topLeftHeaderCell == value)
					return;

				if (topLeftHeaderCell != null)
					topLeftHeaderCell.SetDataGridView (null);

				topLeftHeaderCell = value;

				if (topLeftHeaderCell != null)
					topLeftHeaderCell.SetDataGridView (this);
			}
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

		[MonoTODO ("VirtualMode is not supported.")]
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
					editingControl.Dispose();
				}
				
				
				if (value != null) {
					value.Visible = false;
					Controls.Add (value);
				}

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

		public void AutoResizeColumn (int columnIndex, DataGridViewAutoSizeColumnMode autoSizeColumnMode)
		{
			AutoResizeColumnInternal (columnIndex, autoSizeColumnMode);
		}

		public void AutoResizeColumnHeadersHeight ()
		{
			int new_height = 0;
			
			foreach (DataGridViewColumn col in Columns)
				new_height = Math.Max (new_height, col.HeaderCell.PreferredSize.Height);
			
			if (ColumnHeadersHeight != new_height)
				ColumnHeadersHeight = new_height;
		}

		[MonoTODO ("columnIndex parameter is not used")]
		public void AutoResizeColumnHeadersHeight (int columnIndex)
		{
			AutoResizeColumnHeadersHeight ();
		}

		public void AutoResizeColumns () {
			AutoResizeColumns (DataGridViewAutoSizeColumnsMode.AllCells);
		}

		public void AutoResizeColumns (DataGridViewAutoSizeColumnsMode autoSizeColumnsMode) {
			AutoResizeColumns (autoSizeColumnsMode, true);
		}

		public void AutoResizeRow (int rowIndex)
		{
			AutoResizeRow (rowIndex, DataGridViewAutoSizeRowMode.AllCells, true);
		}

		public void AutoResizeRow (int rowIndex, DataGridViewAutoSizeRowMode autoSizeRowMode)
		{
			AutoResizeRow (rowIndex, autoSizeRowMode, true);
		}

		public void AutoResizeRowHeadersWidth (DataGridViewRowHeadersWidthSizeMode rowHeadersWidthSizeMode)
		{
			if (rowHeadersWidthSizeMode == DataGridViewRowHeadersWidthSizeMode.AutoSizeToFirstHeader) {
				RowHeadersWidth = GetRowInternal (0).HeaderCell.PreferredSize.Width;
				return;
			}
			
			int new_width = 0;
			
			if (rowHeadersWidthSizeMode == DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders) {
				bool anyRowsDisplayed = false;
				foreach(DataGridViewRow row in Rows)
					if(row.Displayed) {
						anyRowsDisplayed = true;
						new_width = Math.Max (new_width, row.HeaderCell.PreferredSize.Width);
					}
	
			        // if there are no rows which are displayed, we still have to set new_width
				// to a value >= 4 as RowHeadersWidth will throw an exception otherwise	
				if(!anyRowsDisplayed) {
					foreach (DataGridViewRow row in Rows)
							new_width = Math.Max (new_width, row.HeaderCell.PreferredSize.Width);
			        }		
				
				if (RowHeadersWidth != new_width)
					RowHeadersWidth = new_width;
					
				return;
			}

			if (rowHeadersWidthSizeMode == DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders) {
				foreach (DataGridViewRow row in Rows)
					new_width = Math.Max (new_width, row.HeaderCell.PreferredSize.Width);

				if (RowHeadersWidth != new_width)
					RowHeadersWidth = new_width;

				return;
			}
		}

		[MonoTODO ("Does not use rowIndex parameter.")]
		public void AutoResizeRowHeadersWidth (int rowIndex, DataGridViewRowHeadersWidthSizeMode rowHeadersWidthSizeMode)
		{
			AutoResizeRowHeadersWidth (rowHeadersWidthSizeMode);
		}

		public void AutoResizeRows ()
		{
			AutoResizeRows (0, Rows.Count, DataGridViewAutoSizeRowMode.AllCells, false);
		}

		public void AutoResizeRows (DataGridViewAutoSizeRowsMode autoSizeRowsMode)
		{
			if (!Enum.IsDefined(typeof(DataGridViewAutoSizeRowsMode), autoSizeRowsMode))
				throw new InvalidEnumArgumentException ("Parameter autoSizeRowsMode is not a valid DataGridViewRowsMode.");
			if ((autoSizeRowsMode == DataGridViewAutoSizeRowsMode.AllHeaders || autoSizeRowsMode == DataGridViewAutoSizeRowsMode.DisplayedHeaders) && rowHeadersVisible == false)
				throw new InvalidOperationException ("Parameter autoSizeRowsMode cannot be AllHeaders or DisplayedHeaders in this DataGridView.");
			if (autoSizeRowsMode == DataGridViewAutoSizeRowsMode.None)
				throw new ArgumentException ("Parameter autoSizeRowsMode cannot be None.");
			
			AutoResizeRows (autoSizeRowsMode, false);
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
			
			if (editType == null && !(cell is IDataGridViewEditingCell))
				return false;
				
			// Give user a chance to cancel the edit
			DataGridViewCellCancelEventArgs e = new DataGridViewCellCancelEventArgs (cell.ColumnIndex, cell.RowIndex);
			OnCellBeginEdit (e);

			if (e.Cancel)
				return false;

			cell.SetIsInEditMode (true);
			
			// The cell has an editing control we need to setup
			if (editType != null) {
				Control ctrl = EditingControlInternal;
				
				// Check if we can reuse the one we already have
				bool isCorrectType = ctrl != null && ctrl.GetType () == editType;
				
				if (!isCorrectType)
					ctrl = null;
				
				// We couldn't use the existing one, create a new one
				if (ctrl == null) {
					ctrl = (Control) Activator.CreateInstance (editType);
					EditingControlInternal = ctrl;
				}
				
				// Call some functions that allows the editing control to get setup
				DataGridViewCellStyle style = cell.RowIndex == -1 ? DefaultCellStyle : cell.InheritedStyle;
				cell.InitializeEditingControl (cell.RowIndex, cell.FormattedValue, style);
				OnEditingControlShowing (new DataGridViewEditingControlShowingEventArgs (EditingControlInternal, style));
				cell.PositionEditingControl (true, true, this.GetCellDisplayRectangle (cell.ColumnIndex, cell.RowIndex, false), bounds, style, false, false, (columns [cell.ColumnIndex].DisplayIndex == 0), (cell.RowIndex == 0));

				// Show the editing control
				if (EditingControlInternal != null)
					EditingControlInternal.Visible = true;

				IDataGridViewEditingControl dgvEditingControl = (IDataGridViewEditingControl) EditingControlInternal;
				if (dgvEditingControl != null) {
					dgvEditingControl.EditingControlDataGridView = this;
					dgvEditingControl.EditingControlRowIndex = currentCell.OwningRow.Index;
					dgvEditingControl.ApplyCellStyleToEditingControl (style);
					dgvEditingControl.PrepareEditingControlForEdit (selectAll);
					dgvEditingControl.EditingControlFormattedValue = currentCell.EditedFormattedValue;
				}
				return true;
			}

			// If we are here, it means we have a cell that does not have an editing control
			// and simply implements IDataGridViewEditingCell itself.
			(cell as IDataGridViewEditingCell).PrepareEditingCellForEdit (selectAll);

			return true;
		}

		public bool CancelEdit ()
		{
			if (currentCell != null) {
				if (currentCell.IsInEditMode) {
					currentCell.SetIsInEditMode (false);
					currentCell.DetachEditingControl ();
				}

				if (currentCell.RowIndex == NewRowIndex) {
					if (DataManager != null)
						DataManager.CancelCurrentEdit ();

					new_row_editing = false;
					PrepareEditingRow (false, false);
					MoveCurrentCell (currentCell.ColumnIndex, NewRowIndex, true, false, false, true);
					OnUserDeletedRow (new DataGridViewRowEventArgs (EditingRow));
				}
			}

			return true;
		}

		public void ClearSelection ()
		{
			foreach (DataGridViewColumn col in SelectedColumns)
				col.Selected = false;
			foreach (DataGridViewRow row in SelectedRows)
				row.Selected = false;
			foreach (DataGridViewCell cell in SelectedCells)
				cell.Selected = false;
		}

		public bool CommitEdit (DataGridViewDataErrorContexts context)
		{
			if (currentCell == null)
				return true;

			try {
				// convert
				object newValue = currentCell.ParseFormattedValue (currentCell.EditedFormattedValue, 
										   currentCell.InheritedStyle, null, null);

				DataGridViewCellValidatingEventArgs validateArgs = new DataGridViewCellValidatingEventArgs (currentCell.ColumnIndex, 
															    currentCell.RowIndex, 
															    newValue);
				// validate
				OnCellValidating (validateArgs);
				if (validateArgs.Cancel)
					return false;
				OnCellValidated (new DataGridViewCellEventArgs (currentCell.ColumnIndex, currentCell.RowIndex));

				// commit
				currentCell.Value = newValue;

			} catch (Exception e) {
				DataGridViewDataErrorEventArgs args = new DataGridViewDataErrorEventArgs (e, currentCell.ColumnIndex, currentCell.RowIndex, 
													  DataGridViewDataErrorContexts.Commit);
				OnDataError (false, args);
				if (args.ThrowException)
					throw e;
				return false;
			}
			return true;
		}

		public int DisplayedColumnCount (bool includePartialColumns)
		{
			int result = 0;
			int columnLeft = 0;

			if (RowHeadersVisible)
				columnLeft += RowHeadersWidth;

			Size visibleClientArea = ClientSize;
			if (verticalScrollBar.Visible)
				visibleClientArea.Width -= verticalScrollBar.Width;
			if (horizontalScrollBar.Visible)
				visibleClientArea.Height -= horizontalScrollBar.Height;

			for (int index = first_col_index; index < Columns.Count; index++) {
				DataGridViewColumn column = Columns[ColumnDisplayIndexToIndex (index)];
				if (columnLeft + column.Width <= visibleClientArea.Width) {
					result++;
					columnLeft += column.Width;
				} else {
					if (includePartialColumns)
						result++;
					break;
				}
			}
					
			return result;
		}

		public int DisplayedRowCount (bool includePartialRow)
		{
			int result = 0;
			int rowTop = 0;

			if (ColumnHeadersVisible)
				rowTop += ColumnHeadersHeight;

			Size visibleClientArea = ClientSize;
			if (verticalScrollBar.Visible)
				visibleClientArea.Width -= verticalScrollBar.Width;
			if (horizontalScrollBar.Visible)
				visibleClientArea.Height -= horizontalScrollBar.Height;

			for (int index = first_row_index; index < Rows.Count; index++) {
				DataGridViewRow row = GetRowInternal (index);
				if (rowTop + row.Height <= visibleClientArea.Height) {
					result++;
					rowTop += row.Height;
				} else {
					if (includePartialRow)
						result++;
					break;
				}
			}
					
			return result;
		}

		public bool EndEdit ()
		{
			return EndEdit (DataGridViewDataErrorContexts.Commit);
		}

		[MonoTODO ("Does not use context parameter")]
		public bool EndEdit (DataGridViewDataErrorContexts context)
		{
			if (currentCell == null || !currentCell.IsInEditMode)
				return true;

			if (!CommitEdit (context)) {
				if (DataManager != null)
					DataManager.EndCurrentEdit ();
				if (EditingControl != null)
					EditingControl.Focus ();
				return false;
			}

			currentCell.SetIsInEditMode (false);
			currentCell.DetachEditingControl ();
			OnCellEndEdit (new DataGridViewCellEventArgs (currentCell.ColumnIndex, currentCell.RowIndex));
			if (context != DataGridViewDataErrorContexts.LeaveControl)
				Focus ();
			if (currentCell.RowIndex == NewRowIndex) {
				new_row_editing = false;
				editing_row = null; // editing row becomes a real row
				PrepareEditingRow (true, false); // add a new editing row
				MoveCurrentCell (currentCell.ColumnIndex, NewRowIndex, true, false, false, true);
			}
			return true;
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
			// Allow the column and row headers (index == -1).
			if (columnIndex < -1 || columnIndex >= columns.Count) {
				throw new ArgumentOutOfRangeException ("Column index is out of range.");
			}
			if (rowIndex < -1 || (rowIndex > 0 && rowIndex >= rows.Count)) {
				throw new ArgumentOutOfRangeException ("Row index is out of range.");
			}
			
			int x = 0, y = 0, w = 0, h = 0;
			
			x = BorderWidth;
			y = BorderWidth;
			
			if (ColumnHeadersVisible)
				y += ColumnHeadersHeight;
			
			if (RowHeadersVisible)
				x += RowHeadersWidth;

			// Handle the top left cell when both column and row headers are showing.
			if (columnIndex == -1 && rowIndex == -1)
				return new Rectangle (BorderWidth, BorderWidth, RowHeadersWidth, ColumnHeadersHeight);

			if (columnIndex >= 0)
			{
				List<DataGridViewColumn> cols = columns.ColumnDisplayIndexSortedArrayList;
	
				for (int i = first_col_index; i < cols.Count; i++) {
					if (!cols[i].Visible)
						continue;
						
					if (cols[i].Index == columnIndex) {
						w = cols[i].Width;
						break;
					}
	
					x += cols[i].Width;
				}
			}
			
			// Handle a cell in the header row.
			if (rowIndex == -1)
				return new Rectangle (x, BorderWidth, w, ColumnHeadersHeight);

			for (int i = first_row_index; i < Rows.Count; i++) {
				if (!rows[i].Visible)
					continue;
					
				if (rows[i].Index == rowIndex) {
					h = rows [i].Height;
					break;
				}
				
				y += rows [i].Height;
			}
			
			// Handle a cell in the header column.
			if (columnIndex == -1)
				return new Rectangle (BorderWidth, y, RowHeadersWidth, h);

			return new Rectangle (x, y, w, h);
		}

		public virtual DataObject GetClipboardContent () {
			
			if (clipboardCopyMode == DataGridViewClipboardCopyMode.Disable)
				throw new InvalidOperationException ("Generating Clipboard content is not supported when the ClipboardCopyMode property is Disable.");
			
			int start_row = int.MaxValue, end_row = int.MinValue;
			int start_col = int.MaxValue, end_col = int.MinValue;
			
			bool include_row_headers = false;
			bool include_col_headers = false;
			bool only_included_headers = false;
			bool headers_includable = false;
			
			switch (ClipboardCopyMode) {
			case DataGridViewClipboardCopyMode.EnableWithoutHeaderText:
				break;
			case DataGridViewClipboardCopyMode.EnableWithAutoHeaderText:
				// Headers are included if not selection mode is CellSelect, and any header is selected.
				headers_includable = selectionMode != DataGridViewSelectionMode.CellSelect;
				break;
			case DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText:
				include_col_headers = include_row_headers = true;
				break;
			}
			
			BitArray included_rows = new BitArray (RowCount);
			BitArray included_cols = new BitArray (ColumnCount);
			
			// If there are any selected columns,
			// include the column headers (if headers are to be shown).
			if (headers_includable && !include_col_headers) {
				for (int c = 0; c < ColumnCount; c++) {
					if (Columns [c].Selected) {
						include_col_headers = true;
						break;
					}
				}
			}
			
			// Find the smallest rectangle that encompasses all selected cells.
			for (int r = 0; r < RowCount; r++) {
				DataGridViewRow row = Rows [r];

				if (headers_includable && !include_row_headers && row.Selected) {
					include_row_headers = true;
				}
				
				for (int c = 0; c < ColumnCount; c++) {
					DataGridViewCell cell = row.Cells [c];
					
					if (cell == null || !cell.Selected)
						continue;
					
					included_cols [c] = true;
					included_rows [r] = true;
					
					start_row = Math.Min (start_row, r);
					start_col = Math.Min (start_col, c);
					end_row = Math.Max (end_row, r);
					end_col = Math.Max (end_col, c);
				}
			}
			
			// Mark rows/columns in between selected cells as included if the selection mode isn't FullHeaderSelect.
			switch (selectionMode){
			case DataGridViewSelectionMode.CellSelect:
			case DataGridViewSelectionMode.ColumnHeaderSelect:
			case DataGridViewSelectionMode.RowHeaderSelect:
				if (selectionMode != DataGridViewSelectionMode.ColumnHeaderSelect) {
					for (int r = start_row; r <= end_row; r++) {
						included_rows.Set (r, true);
					}
				} else if (start_row <= end_row) {
					included_rows.SetAll (true);
				}
				if (selectionMode != DataGridViewSelectionMode.RowHeaderSelect) {
					for (int c = start_col; c <= end_col; c++) {
						included_cols.Set (c, true);
					}
				}
				break;
			case DataGridViewSelectionMode.FullColumnSelect:
			case DataGridViewSelectionMode.FullRowSelect:
				only_included_headers = true;
				break;
			}
			
			if (start_row > end_row)
				return null;
				
			if (start_col > end_col)
				return null;
			
			DataObject result = new DataObject ();
			
			StringBuilder text_builder = new StringBuilder ();
			StringBuilder utext_builder = new StringBuilder ();
			StringBuilder html_builder = new StringBuilder ();
			StringBuilder csv_builder = new StringBuilder ();
			
			// Loop through all rows and columns to create the content.
			// -1 is the header row/column.
			int first_row = start_row;
			int first_col = start_col;
			if (include_col_headers) {
				first_row = -1;
			}
			for (int r = first_row; r <= end_row; r++) {
				DataGridViewRow row = null;
				
				if (r >= 0) {
					if (!included_rows [r])
						continue;
						
					row = Rows [r];
				}

				if (include_row_headers) {
					first_col = -1;
				}
				
				for (int c = first_col; c <= end_col; c++) {
					DataGridViewCell cell = null;

					if (c >= 0 && only_included_headers && !included_cols [c])
						continue;
				
					if (row == null) {
						if (c == -1) {
							cell = TopLeftHeaderCell;
						} else {
							cell = Columns [c].HeaderCell;
						}
					} else {
						if (c == -1) {
							cell = row.HeaderCell;
						} else {
							cell = row.Cells [c];
						}
					}
				
					string text, utext, html, csv;
					bool is_first_cell = (c == first_col);
					bool is_last_cell = (c == end_col);
					bool is_first_row = (r == first_row);
					bool is_last_row = (r == end_row);
					
					if (cell == null) {
						text = string.Empty;
						utext = string.Empty;
						html = string.Empty;
						csv = string.Empty;
					} else {
						text = cell.GetClipboardContentInternal (r, is_first_cell, is_last_cell, is_first_row, is_last_row, DataFormats.Text) as string;
						utext = cell.GetClipboardContentInternal (r, is_first_cell, is_last_cell, is_first_row, is_last_row, DataFormats.UnicodeText) as string;
						html = cell.GetClipboardContentInternal (r, is_first_cell, is_last_cell, is_first_row, is_last_row, DataFormats.Html) as string;
						csv = cell.GetClipboardContentInternal (r, is_first_cell, is_last_cell, is_first_row, is_last_row, DataFormats.CommaSeparatedValue) as string;
					}
					
					text_builder.Append (text);
					utext_builder.Append (utext);
					html_builder.Append (html);
					csv_builder.Append (csv);
					
					if (c == -1) { // If we just did the row header, jump to the first column.
						c = start_col - 1;
					}
				}

				if (r == -1) {// If we just did the column header, jump to the first row.
					r = start_row - 1;
				}
			}

			// 
			// Html content always get the \r\n newline
			// It's valid html anyway, and it eases testing quite a bit
			// (since otherwise we'd have to change the start indices
			// in the added prologue/epilogue text)
			// 
			int fragment_end = 135 + html_builder.Length;
			int html_end = fragment_end + 36;
			string html_start =
			"Version:1.0{0}" +
			"StartHTML:00000097{0}" +
			"EndHTML:{1:00000000}{0}" +
			"StartFragment:00000133{0}" +
			"EndFragment:{2:00000000}{0}" +
			"<HTML>{0}" +
			"<BODY>{0}" +
			"<!--StartFragment-->";
			
			html_start = string.Format (html_start, "\r\n", html_end, fragment_end);
			html_builder.Insert (0, html_start);
			html_builder.AppendFormat ("{0}<!--EndFragment-->{0}</BODY>{0}</HTML>", "\r\n");
			
			result.SetData (DataFormats.CommaSeparatedValue, false, csv_builder.ToString ());
			result.SetData (DataFormats.Html, false, html_builder.ToString ());
			result.SetData (DataFormats.UnicodeText, false, utext_builder.ToString ());
			result.SetData (DataFormats.Text, false, text_builder.ToString ());
			
			return result;
		}

		[MonoTODO ("Does not use cutOverflow parameter")]
		public Rectangle GetColumnDisplayRectangle (int columnIndex, bool cutOverflow)
		{
			if (columnIndex < 0 || columnIndex > Columns.Count - 1)
				throw new ArgumentOutOfRangeException ("columnIndex");
				
			int x = 0;
			int w = 0;

			x = BorderWidth;

			if (RowHeadersVisible)
				x += RowHeadersWidth;

			List<DataGridViewColumn> cols = columns.ColumnDisplayIndexSortedArrayList;

			for (int i = first_col_index; i < cols.Count; i++) {
				if (!cols[i].Visible)
					continue;
					
				if (cols[i].Index == columnIndex) {
					w = cols[i].Width;
					break;
				}

				x += cols[i].Width;
			}

			return new Rectangle (x, 0, w, Height);
		}

		[MonoTODO ("Does not use cutOverflow parameter")]
		public Rectangle GetRowDisplayRectangle (int rowIndex, bool cutOverflow)
		{
			if (rowIndex < 0 || rowIndex > Rows.Count - 1)
				throw new ArgumentOutOfRangeException ("rowIndex");

			int y = 0;
			int h = 0;

			y = BorderWidth;

			if (ColumnHeadersVisible)
				y += ColumnHeadersHeight;


			for (int i = first_row_index; i < Rows.Count; i++) {
				if (!rows[i].Visible)
					continue;
					
				if (rows[i].Index == rowIndex) {
					h = rows [i].Height;
					break;
				}
				
				y += rows [i].Height;
			}

			return new Rectangle (0, y, Width, h);
		}

		public HitTestInfo HitTest (int x, int y) {
			///////////////////////////////////////////////////////
			//Console.WriteLine ("HitTest ({0}, {1})", x, y);
			bool isInColHeader = columnHeadersVisible && y >= 0 && y <= ColumnHeadersHeight;
			bool isInRowHeader = rowHeadersVisible && x >= 0 && x <= RowHeadersWidth;
			
			// TopLeftHeader
			if (isInColHeader && isInRowHeader)
				return new HitTestInfo (-1, x, -1, y, DataGridViewHitTestType.TopLeftHeader);
			
			// HorizontalScrollBar
			if (horizontalScrollBar.Visible && horizontalScrollBar.Bounds.Contains (x, y))
				return new HitTestInfo (-1, x, -1, y, DataGridViewHitTestType.HorizontalScrollBar);

			// VerticalScrollBar
			if (verticalScrollBar.Visible && verticalScrollBar.Bounds.Contains (x, y))
				return new HitTestInfo (-1, x, -1, y, DataGridViewHitTestType.VerticalScrollBar);
			
			// The little box in the bottom right if both scrollbars are shown is None
			if (verticalScrollBar.Visible && horizontalScrollBar.Visible)
				if (new Rectangle (verticalScrollBar.Left, horizontalScrollBar.Top, verticalScrollBar.Width, horizontalScrollBar.Height).Contains (x, y))
					return new HitTestInfo (-1, x, -1, y, DataGridViewHitTestType.None);
			
			int rowindex = -1;
			int colindex = -1;
			
			int top = columnHeadersVisible ? columnHeadersHeight : 0;
			
			for (int i = first_row_index; i < Rows.Count; i++) {
				DataGridViewRow row = Rows[i];
				if (!row.Visible)
					continue;
				
				if (y > top && y <= (top + row.Height)) {
					rowindex = i;
					break;
				}
				
				top += row.Height;
			}
			
			int left = rowHeadersVisible ? RowHeadersWidth : 0;

			List<DataGridViewColumn> cols = columns.ColumnDisplayIndexSortedArrayList;
			
			for (int i = first_col_index; i < cols.Count; i++) {
				if (!cols[i].Visible)
					continue;
					
				if (x > left && x <= (left + cols[i].Width)) {
					colindex = cols[i].Index;
					break;
				}

				left += cols[i].Width;
			}

			if (colindex >= 0 && rowindex >= 0)
				return new HitTestInfo (colindex, x, rowindex, y, DataGridViewHitTestType.Cell);
			
			if (isInColHeader && colindex > -1)
				return new HitTestInfo (colindex, x, rowindex, y, DataGridViewHitTestType.ColumnHeader);
			
			if (isInRowHeader && rowindex > -1)
				return new HitTestInfo (colindex, x, rowindex, y, DataGridViewHitTestType.RowHeader);
				
			return new HitTestInfo (-1, x, -1, y, DataGridViewHitTestType.None);
		}

		public void InvalidateCell (DataGridViewCell dataGridViewCell)
		{
			if (dataGridViewCell == null)
				throw new ArgumentNullException ("Cell is null");

			if (dataGridViewCell.DataGridView != this)
				throw new ArgumentException ("The specified cell does not belong to this DataGridView.");

			InvalidateCell (dataGridViewCell.ColumnIndex, dataGridViewCell.RowIndex);
		}

		public void InvalidateCell (int columnIndex, int rowIndex)
		{
			// Allow the header column (columnIndex == -1).
			if (columnIndex < -1 || columnIndex >= columns.Count)
				throw new ArgumentOutOfRangeException ("Column index is out of range.");

			// Allow the header row (rowIndex == -1).
			if (rowIndex < -1 || rowIndex >= rows.Count)
				throw new ArgumentOutOfRangeException ("Row index is out of range.");

			if (!is_binding)
				Invalidate (GetCellDisplayRectangle (columnIndex, rowIndex, true));
		}

		public void InvalidateColumn (int columnIndex)
		{
			if (columnIndex < 0 || columnIndex >= columns.Count)
				throw new ArgumentOutOfRangeException ("Column index is out of range.");

			if (!is_binding)
				Invalidate (GetColumnDisplayRectangle (columnIndex, true));
		}

		public void InvalidateRow (int rowIndex)
		{
			if (rowIndex < 0 || rowIndex >= rows.Count)
				throw new ArgumentOutOfRangeException ("Row index is out of range.");

			if (!is_binding)
				Invalidate (GetRowDisplayRectangle (rowIndex, true));
		}

		public virtual void NotifyCurrentCellDirty (bool dirty) {
			if (currentCell != null)
				InvalidateCell (currentCell);
		}

		public bool RefreshEdit ()
		{
			if (IsCurrentCellInEditMode) {
				currentCell.InitializeEditingControl (currentCell.RowIndex, currentCell.FormattedValue, currentCell.InheritedStyle);
				return true;
			}
			
			return false;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void ResetText ()
		{
			Text = string.Empty;
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
			
			Invalidate ();
		}

		public virtual void Sort (IComparer comparer)
		{
			if (comparer == null)
				throw new ArgumentNullException ("comparer");
			if (VirtualMode || DataSource != null)
				throw new InvalidOperationException ();

			if (SortedColumn != null)
				SortedColumn.HeaderCell.SortGlyphDirection = SortOrder.None;

			EndEdit ();

			Rows.Sort (comparer);
			
			sortedColumn = null;
			sortOrder = SortOrder.None;
			
			currentCell = null;
			
			Invalidate ();
			
			OnSorted (EventArgs.Empty);
		}

		public virtual void Sort (DataGridViewColumn dataGridViewColumn, ListSortDirection direction)
		{
			if (dataGridViewColumn == null)
				throw new ArgumentNullException ("dataGridViewColumn");
			if (dataGridViewColumn.DataGridView != this)
				throw new ArgumentException ("dataGridViewColumn");

			if (!EndEdit ())
				return;

			if (SortedColumn != null)
				SortedColumn.HeaderCell.SortGlyphDirection = SortOrder.None;

			sortedColumn = dataGridViewColumn;
			sortOrder = direction == ListSortDirection.Ascending ? SortOrder.Ascending : SortOrder.Descending;
			
			if (Rows.Count == 0)
				return;

			if (dataGridViewColumn.IsDataBound) {
				IBindingList bindingList = DataManager.List as IBindingList;
				if (bindingList != null && bindingList.SupportsSorting) {
					bindingList.ApplySort (DataManager.GetItemProperties()[dataGridViewColumn.DataPropertyName], direction);
					dataGridViewColumn.HeaderCell.SortGlyphDirection = sortOrder;
				}
			} else {
				// Figure out if this is a numeric sort or text sort
				bool is_numeric = true;
				double n;
				
				foreach (DataGridViewRow row in Rows) {
					object val = row.Cells[dataGridViewColumn.Index].Value;
					
					if (val != null && !double.TryParse (val.ToString (), out n)) {
						is_numeric = false;
						break;
					}
				}
				
				ColumnSorter sorter = new ColumnSorter (dataGridViewColumn, direction, is_numeric);
				Rows.Sort (sorter);
				dataGridViewColumn.HeaderCell.SortGlyphDirection = sortOrder;
			}

			Invalidate ();
			OnSorted (EventArgs.Empty);
		}
		
		public void UpdateCellErrorText (int columnIndex, int rowIndex)
		{
			if (columnIndex < 0 || columnIndex > Columns.Count - 1)
				throw new ArgumentOutOfRangeException ("columnIndex");
			if (rowIndex < 0 || rowIndex > Rows.Count - 1)
				throw new ArgumentOutOfRangeException ("rowIndex");

			InvalidateCell (columnIndex, rowIndex);
		}

		public void UpdateCellValue (int columnIndex, int rowIndex)
		{
			if (columnIndex < 0 || columnIndex > Columns.Count - 1)
				throw new ArgumentOutOfRangeException ("columnIndex");
			if (rowIndex < 0 || rowIndex > Rows.Count - 1)
				throw new ArgumentOutOfRangeException ("rowIndex");
			
			InvalidateCell (columnIndex, rowIndex);
		}

		public void UpdateRowErrorText (int rowIndex)
		{
			if (rowIndex < 0 || rowIndex > Rows.Count - 1)
				throw new ArgumentOutOfRangeException ("rowIndex");

			InvalidateRow (rowIndex);
		}

		public void UpdateRowErrorText (int rowIndexStart, int rowIndexEnd)
		{
			if (rowIndexStart < 0 || rowIndexStart > Rows.Count - 1)
				throw new ArgumentOutOfRangeException ("rowIndexStart");
			if (rowIndexEnd < 0 || rowIndexEnd > Rows.Count - 1)
				throw new ArgumentOutOfRangeException ("rowIndexEnd");
			if (rowIndexEnd < rowIndexStart)
				throw new ArgumentOutOfRangeException ("rowIndexEnd", "rowIndexEnd must be greater than rowIndexStart");
				
			for (int i = rowIndexStart; i <= rowIndexEnd; i++)
				InvalidateRow (i);
		}

		public void UpdateRowHeightInfo (int rowIndex, bool updateToEnd)
		{
			if (rowIndex < 0 && updateToEnd)
				throw new ArgumentOutOfRangeException ("rowIndex");
			if (rowIndex < -1 && !updateToEnd)
				throw new ArgumentOutOfRangeException ("rowIndex");
			if (rowIndex >= Rows.Count)
				throw new ArgumentOutOfRangeException ("rowIndex");
			
			if (!VirtualMode && DataManager == null)
				return;

			if (rowIndex == -1) {
				updateToEnd = true;
				rowIndex = 0;
			}

			if (updateToEnd) {
				for (int i = rowIndex; i < Rows.Count; i++) {
					DataGridViewRow row = Rows[i];
					if (!row.Visible)
						continue;

					DataGridViewRowHeightInfoNeededEventArgs rowInfo = 
						new DataGridViewRowHeightInfoNeededEventArgs (row.Index, row.Height, row.MinimumHeight);
					OnRowHeightInfoNeeded (rowInfo);

					if (row.Height != rowInfo.Height || row.MinimumHeight != rowInfo.MinimumHeight) {
						row.Height = rowInfo.Height;
						row.MinimumHeight = rowInfo.MinimumHeight;
						OnRowHeightInfoPushed (new DataGridViewRowHeightInfoPushedEventArgs (row.Index, rowInfo.Height, 
														     rowInfo.MinimumHeight));
					}
				}
			} else {
				DataGridViewRow row = Rows[rowIndex];
				DataGridViewRowHeightInfoNeededEventArgs rowInfo = 
					new DataGridViewRowHeightInfoNeededEventArgs (row.Index, row.Height, row.MinimumHeight);
				OnRowHeightInfoNeeded (rowInfo);

				if (row.Height != rowInfo.Height || row.MinimumHeight != rowInfo.MinimumHeight) {
					row.Height = rowInfo.Height;
					row.MinimumHeight = rowInfo.MinimumHeight;
					OnRowHeightInfoPushed (new DataGridViewRowHeightInfoPushedEventArgs (row.Index, rowInfo.Height, 
													     rowInfo.MinimumHeight));
				}
			}
		}

		protected override bool CanEnableIme {
			get {
				if (CurrentCell != null && CurrentCell.EditType != null)
					return true;
				
				return false;
			}
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

		[MonoTODO ("Does not use fixedHeight parameter")]
		protected void AutoResizeColumn (int columnIndex, DataGridViewAutoSizeColumnMode autoSizeColumnMode, bool fixedHeight)
		{
			AutoResizeColumn (columnIndex, autoSizeColumnMode);
		}

		[MonoTODO ("Does not use fixedRowHeadersWidth or fixedColumnsWidth parameters")]
		protected void AutoResizeColumnHeadersHeight (bool fixedRowHeadersWidth, bool fixedColumnsWidth)
		{
			AutoResizeColumnHeadersHeight ();
		}

		[MonoTODO ("Does not use columnIndex or fixedRowHeadersWidth or fixedColumnsWidth parameters")]
		protected void AutoResizeColumnHeadersHeight (int columnIndex, bool fixedRowHeadersWidth, bool fixedColumnWidth)
		{
			AutoResizeColumnHeadersHeight (columnIndex);
		}

		protected void AutoResizeColumns (DataGridViewAutoSizeColumnsMode autoSizeColumnsMode, bool fixedHeight) {
			for (int i = 0; i < Columns.Count; i++) {
				AutoResizeColumn (i, (DataGridViewAutoSizeColumnMode) autoSizeColumnsMode, fixedHeight);
			}
		}

		[MonoTODO ("Does not use fixedWidth parameter")]
		protected void AutoResizeRow (int rowIndex, DataGridViewAutoSizeRowMode autoSizeRowMode, bool fixedWidth)
		{
			if (autoSizeRowMode == DataGridViewAutoSizeRowMode.RowHeader && !rowHeadersVisible)
				throw new InvalidOperationException ("row headers are not visible");
			if (rowIndex < 0 || rowIndex > Rows.Count - 1)
				throw new ArgumentOutOfRangeException ("rowIndex");

			DataGridViewRow row = GetRowInternal (rowIndex);

			int new_height = row.GetPreferredHeight (rowIndex, autoSizeRowMode, true);

			if (row.Height != new_height)
				row.SetAutoSizeHeight (new_height);
		}

		[MonoTODO ("Does not use fixedColumnHeadersHeight or fixedRowsHeight parameter")]
		protected void AutoResizeRowHeadersWidth (DataGridViewRowHeadersWidthSizeMode rowHeadersWidthSizeMode, bool fixedColumnHeadersHeight, bool fixedRowsHeight)
		{
			AutoResizeRowHeadersWidth (rowHeadersWidthSizeMode);
		}

		[MonoTODO ("Does not use rowIndex or fixedColumnHeadersHeight or fixedRowsHeight parameter")]
		protected void AutoResizeRowHeadersWidth (int rowIndex, DataGridViewRowHeadersWidthSizeMode rowHeadersWidthSizeMode, bool fixedColumnHeadersHeight, bool fixedRowHeight)
		{
			AutoResizeRowHeadersWidth (rowHeadersWidthSizeMode);
		}

		[MonoTODO ("Does not use fixedWidth parameter")]
		protected void AutoResizeRows (DataGridViewAutoSizeRowsMode autoSizeRowsMode, bool fixedWidth)
		{
			if (autoSizeRowsMode == DataGridViewAutoSizeRowsMode.None)
				return;
				
			bool displayed_only = false;
			DataGridViewAutoSizeRowMode mode = DataGridViewAutoSizeRowMode.AllCells;
			
			switch (autoSizeRowsMode) {
				case DataGridViewAutoSizeRowsMode.AllHeaders:
					mode = DataGridViewAutoSizeRowMode.RowHeader;
					break;
				case DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders:
					mode = DataGridViewAutoSizeRowMode.AllCellsExceptHeader;
					break;
				case DataGridViewAutoSizeRowsMode.AllCells:
					mode = DataGridViewAutoSizeRowMode.AllCells;
					break;
				case DataGridViewAutoSizeRowsMode.DisplayedHeaders:
					mode = DataGridViewAutoSizeRowMode.RowHeader;
					displayed_only = true;
					break;
				case DataGridViewAutoSizeRowsMode.DisplayedCellsExceptHeaders:
					mode = DataGridViewAutoSizeRowMode.AllCellsExceptHeader;
					displayed_only = true;
					break;
				case DataGridViewAutoSizeRowsMode.DisplayedCells:
					mode = DataGridViewAutoSizeRowMode.AllCells;
					displayed_only = true;
					break;
			}
			
			foreach (DataGridViewRow row in Rows) {
				if (!row.Visible)
					continue;
				if (!displayed_only || row.Displayed) {
					int new_height = row.GetPreferredHeight (row.Index, mode, fixedWidth);

					if (row.Height != new_height)
						row.SetAutoSizeHeight (new_height);
				}
			}
		}

		[MonoTODO ("Does not use fixedMode parameter")]
		protected void AutoResizeRows (int rowIndexStart, int rowsCount, DataGridViewAutoSizeRowMode autoSizeRowMode, bool fixedWidth)
		{
			for (int i = rowIndexStart; i < rowIndexStart + rowsCount; i++)
				AutoResizeRow (i, autoSizeRowMode, fixedWidth);
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
			if (disposing) {
				ClearSelection();
				foreach (DataGridViewColumn column in Columns)
					column.Dispose();
				Columns.Clear();
				foreach (DataGridViewRow row in Rows)
					row.Dispose();
				Rows.Clear();
			}
			editingControl = null;

			base.Dispose(disposing);
		}

		protected override AccessibleObject GetAccessibilityObjectById (int objectId)
		{
			throw new NotImplementedException();
		}

		protected override bool IsInputChar (char charCode)
		{
			return true;
		}

		protected override bool IsInputKey (Keys keyData)
		{
			// Don't look at the modifiers
			keyData = keyData & ~Keys.Modifiers;
			
			switch (keyData) {
				case Keys.Return:
				case Keys.PageUp:
				case Keys.Next:
				case Keys.End:
				case Keys.Home:
				case Keys.Left:
				case Keys.Up:
				case Keys.Right:
				case Keys.Down:
				case Keys.Delete:
				case Keys.D0:
				case Keys.NumPad0:
				case Keys.F2:
					return true;
			}

			return false;
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

		protected internal virtual void OnAutoSizeColumnModeChanged (DataGridViewAutoSizeColumnModeEventArgs e)
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
			ReBind();
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
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);
			cell.OnEnterInternal (e.RowIndex, true);

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
			DataGridViewCell cell = GetCellInternal (e.ColumnIndex, e.RowIndex);
			cell.OnLeaveInternal (e.RowIndex, true);

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

		internal void OnCellStateChangedInternal (DataGridViewCellStateChangedEventArgs e) {
			this.OnCellStateChanged (e);
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

		protected internal virtual void OnCellValueNeeded (DataGridViewCellValueEventArgs e)
		{
			DataGridViewCellValueEventHandler eh = (DataGridViewCellValueEventHandler)(Events [CellValueNeededEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnCellValuePushed (DataGridViewCellValueEventArgs e)
		{
			DataGridViewCellValueEventHandler eh = (DataGridViewCellValueEventHandler)(Events [CellValuePushedEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal void OnColumnAddedInternal (DataGridViewColumnEventArgs e)
		{
			e.Column.DataColumnIndex = FindDataColumnIndex (e.Column);

			if (e.Column.CellTemplate != null) {
				// The first column has just been added programatically instead of 
				// autogenerated so we need to create the rows for the first time.
				//
				if (!is_autogenerating_columns && columns.Count == 1)
					ReBind ();

				// Add/Remove/Update the cells in the existing rows
				foreach (DataGridViewRow row in Rows) {
					if (row.Index == NewRowIndex)
						continue;

					DataGridViewCell newCell = (DataGridViewCell)e.Column.CellTemplate.Clone ();
					if (row.Cells.Count == columns.Count) {
						DataGridViewCell oldCell = row.Cells[e.Column.Index];
						if (currentCell != null && oldCell == currentCell)
							currentCell = newCell;
						// copy the value only if the cell is not-databound
						if (!e.Column.IsDataBound && oldCell.OwningRow.DataBoundItem == null)
							newCell.Value = oldCell.Value;
						row.Cells.Replace (e.Column.Index, newCell);
					} else if (e.Column.Index >= row.Cells.Count)
						row.Cells.Add (newCell);
					else
						row.Cells.Insert (e.Column.Index, newCell);
				}
			}
			
			AutoResizeColumnsInternal ();
			OnColumnAdded (e);
			PrepareEditingRow (false, true);
		}

		private int FindDataColumnIndex (DataGridViewColumn column)
		{
			if (column != null && DataManager != null) {
				PropertyDescriptorCollection properties = DataManager.GetItemProperties();
				for (int i = 0; i < properties.Count; i++) {
					if (String.Compare (column.DataPropertyName, properties[i].Name, true) == 0)
						return i;
				}
			}

			return -1;
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
			DataGridViewColumn col = Columns[e.ColumnIndex];
			
			if (col.SortMode == DataGridViewColumnSortMode.Automatic) {
				ListSortDirection new_order;
				
				// Always use ascending unless we are clicking on a
				// column that is already sorted ascending.
				if (SortedColumn != col || sortOrder != SortOrder.Ascending)
					new_order = ListSortDirection.Ascending;
				else
					new_order = ListSortDirection.Descending;

				Sort (col, new_order);
			}
			
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

		internal void OnColumnPreRemovedInternal (DataGridViewColumnEventArgs e)
		{
			// The removed column should be removed from the selection too.
			if (selected_columns != null)
				SetSelectedColumnCore (e.Column.Index, false);

			if (Columns.Count - 1 == 0) {
				MoveCurrentCell (-1, -1, true, false, false, true);
				rows.ClearInternal ();
			} else if (currentCell != null && CurrentCell.ColumnIndex == e.Column.Index) {
				int nextColumnIndex = e.Column.Index;
				if (nextColumnIndex >= Columns.Count - 1)
					nextColumnIndex = Columns.Count - 1 - 1;
				MoveCurrentCell (nextColumnIndex, currentCell.RowIndex, true, false, false, true);
				if (hover_cell != null && hover_cell.ColumnIndex >= e.Column.Index)
					hover_cell = null;
			}
		}

		private void OnColumnPostRemovedInternal (DataGridViewColumnEventArgs e)
		{
			if (e.Column.CellTemplate != null) {
				int index = e.Column.Index;

				foreach (DataGridViewRow row in Rows)
					row.Cells.RemoveAt (index);
			}

			AutoResizeColumnsInternal ();
			PrepareEditingRow (false, true);

			OnColumnRemoved (e);
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

		protected virtual void OnDataError (bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e)
		{
			DataGridViewDataErrorEventHandler eh = (DataGridViewDataErrorEventHandler)(Events [DataErrorEvent]);
			
			if (eh != null)
				eh (this, e);
			else if (displayErrorDialogIfNoHandler)
				MessageBox.Show (e.ToString ());
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

			Point mouseLocation = this.PointToClient (Control.MousePosition);
			HitTestInfo hitInfo = HitTest (mouseLocation.X, mouseLocation.Y);
			if (hitInfo.Type == DataGridViewHitTestType.Cell)
				OnCellDoubleClick (new DataGridViewCellEventArgs (hitInfo.ColumnIndex, hitInfo.RowIndex));
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

			// To add focus rectangle if needed
			if (currentCell != null && ShowFocusCues)
				InvalidateCell (currentCell);
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
			ReBind ();
			
			if (DataManager == null && CurrentCell == null && Rows.Count > 0 && Columns.Count > 0)
				MoveCurrentCell (ColumnDisplayIndexToIndex (0), 0, true, false, false, false);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnKeyDown (KeyEventArgs e)
		{
			base.OnKeyDown (e);

			e.Handled = ProcessDataGridViewKey (e);
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
			if (horizontalScrollBar.Visible && verticalScrollBar.Visible) {
				horizontalScrollBar.Bounds = new Rectangle (BorderWidth, Height - BorderWidth - horizontalScrollBar.Height, Width - (2 * BorderWidth) - verticalScrollBar.Width, horizontalScrollBar.Height);
				verticalScrollBar.Bounds = new Rectangle (Width - BorderWidth - verticalScrollBar.Width, BorderWidth, verticalScrollBar.Width, Height - (2 * BorderWidth) - horizontalScrollBar.Height);
			} else if (horizontalScrollBar.Visible)
				horizontalScrollBar.Bounds = new Rectangle (BorderWidth, Height - BorderWidth - horizontalScrollBar.Height, Width - (2 * BorderWidth), horizontalScrollBar.Height);
			else if (verticalScrollBar.Visible)
				verticalScrollBar.Bounds = new Rectangle (Width - BorderWidth - verticalScrollBar.Width, BorderWidth, verticalScrollBar.Width,  Height - (2 * BorderWidth));

			AutoResizeColumnsInternal ();
			Invalidate ();
		}

		protected override void OnLeave (EventArgs e)
		{
			EndEdit (DataGridViewDataErrorContexts.LeaveControl);
			base.OnLeave(e);
		}

		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus (e);

			// To remove focus rectangle if needed
			if (currentCell != null && ShowFocusCues)
				InvalidateCell (currentCell);
		}

		protected override void OnMouseClick (MouseEventArgs e)
		{
			base.OnMouseClick(e);
			
			if (column_resize_active || row_resize_active)
				return;
				
			//Console.WriteLine("Mouse: Clicks: {0}; Delta: {1}; X: {2}; Y: {3};", e.Clicks, e.Delta, e.X, e.Y);
			HitTestInfo hit = HitTest (e.X, e.Y);

			switch (hit.Type) {
				case DataGridViewHitTestType.Cell:
					Rectangle display = GetCellDisplayRectangle (hit.ColumnIndex, hit.RowIndex, false);
					Point cellpoint = new Point (e.X - display.X, e.Y - display.Y);

					OnCellMouseClick (new DataGridViewCellMouseEventArgs (hit.ColumnIndex, hit.RowIndex, cellpoint.X, cellpoint.Y, e));
					
					DataGridViewCell cell = GetCellInternal (hit.ColumnIndex, hit.RowIndex);
					
					if (cell.GetContentBounds (hit.RowIndex).Contains (cellpoint)) {
						DataGridViewCellEventArgs dgvcea = new DataGridViewCellEventArgs (hit.ColumnIndex, hit.RowIndex);
						OnCellContentClick (dgvcea);
					}
						
					break;
				case DataGridViewHitTestType.ColumnHeader:
					Rectangle display2 = GetCellDisplayRectangle (hit.ColumnIndex, hit.RowIndex, false);
					Point cellpoint2 = new Point (e.X - display2.X, e.Y - display2.Y);
					
					OnColumnHeaderMouseClick (new DataGridViewCellMouseEventArgs (hit.ColumnIndex, hit.RowIndex, cellpoint2.X, cellpoint2.Y, e));
					break;
			}
		}

		protected override void OnMouseDoubleClick (MouseEventArgs e)
		{
			base.OnMouseDoubleClick(e);

			HitTestInfo hitInfo = HitTest (e.X, e.Y);
			if (hitInfo.Type == DataGridViewHitTestType.Cell)
				OnCellMouseDoubleClick (new DataGridViewCellMouseEventArgs (hitInfo.ColumnIndex, hitInfo.RowIndex,
											    hitInfo.ColumnX, hitInfo.RowY, e));
		}

		private void DoSelectionOnMouseDown (HitTestInfo hitTest)
		{
			Keys modifiers = Control.ModifierKeys;
			bool isControl = (modifiers & Keys.Control) != 0;
			bool isShift = (modifiers & Keys.Shift) != 0;
			//bool isRowHeader = hitTest.Type == DataGridViewHitTestType.RowHeader;
			//bool isColHeader = hitTest.Type == DataGridViewHitTestType.ColumnHeader;
			DataGridViewSelectionMode mode;
			
			switch (hitTest.Type) {
			case DataGridViewHitTestType.Cell:
				mode = selectionMode;
				break;
			case DataGridViewHitTestType.ColumnHeader:
				mode = selectionMode == DataGridViewSelectionMode.ColumnHeaderSelect ? DataGridViewSelectionMode.FullColumnSelect : selectionMode;
				
				if (mode != DataGridViewSelectionMode.FullColumnSelect)
					return;
				break;
			case DataGridViewHitTestType.RowHeader:
				mode = selectionMode == DataGridViewSelectionMode.RowHeaderSelect ?  DataGridViewSelectionMode.FullRowSelect : selectionMode;

				if (mode != DataGridViewSelectionMode.FullRowSelect)
					return;
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
			
			if (!EndEdit ())
				return;

			HitTestInfo hitTest = HitTest(e.X, e.Y);
			
			DataGridViewCell cell = null;
			DataGridViewRow row = null;
			Rectangle cellBounds;

			if ((hitTest.Type == DataGridViewHitTestType.ColumnHeader ||
			     (hitTest.Type == DataGridViewHitTestType.Cell && !ColumnHeadersVisible)) 
			    && MouseOverColumnResize (hitTest.ColumnIndex, e.X)) {
				if (e.Clicks == 2) {
					AutoResizeColumn (hitTest.ColumnIndex);
					return;
				}
				
				resize_band = hitTest.ColumnIndex;
				column_resize_active = true;
				resize_band_start = e.X;
				resize_band_delta = 0;
				DrawVerticalResizeLine (resize_band_start);
				return;
			}

			if ((hitTest.Type == DataGridViewHitTestType.RowHeader ||
			     (hitTest.Type == DataGridViewHitTestType.Cell && !RowHeadersVisible))
			    && MouseOverRowResize (hitTest.RowIndex, e.Y)) {
				if (e.Clicks == 2) {
					AutoResizeRow (hitTest.RowIndex);
					return;
				}

				resize_band = hitTest.RowIndex;
				row_resize_active = true;
				resize_band_start = e.Y;
				resize_band_delta = 0;
				DrawHorizontalResizeLine (resize_band_start);
				return;
			}

			if (hitTest.Type == DataGridViewHitTestType.Cell) {
				row = rows [hitTest.RowIndex];
				cell = row.Cells [hitTest.ColumnIndex];
				SetCurrentCellAddressCore (cell.ColumnIndex, cell.RowIndex, false, true, true);
				cellBounds = GetCellDisplayRectangle (hitTest.ColumnIndex, hitTest.RowIndex, false);
				OnCellMouseDown (new DataGridViewCellMouseEventArgs (hitTest.ColumnIndex, hitTest.RowIndex, e.X - cellBounds.X, e.Y - cellBounds.Y, e));
				OnCellClick (new DataGridViewCellEventArgs (hitTest.ColumnIndex, hitTest.RowIndex));
			}
			
			DoSelectionOnMouseDown (hitTest);
			
			if (hitTest.Type != DataGridViewHitTestType.Cell) {
				if (hitTest.Type == DataGridViewHitTestType.ColumnHeader)
					pressed_header_cell = columns [hitTest.ColumnIndex].HeaderCell;
				else if (hitTest.Type == DataGridViewHitTestType.RowHeader)
					pressed_header_cell = rows [hitTest.RowIndex].HeaderCell;
				Invalidate ();
				return;
			}
			
			Invalidate();
			return;
		}

		private void UpdateBindingPosition (int position)
		{
			if (DataManager != null)
				DataManager.Position = position;
		}

		protected override void OnMouseEnter (EventArgs e)
		{
			base.OnMouseEnter(e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
			
			if (hover_cell != null) {
				OnCellMouseLeave (new DataGridViewCellEventArgs (hover_cell.ColumnIndex, hover_cell.RowIndex));
				hover_cell = null;
			}
			
			EnteredHeaderCell = null;
		}
		
		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);

			if (column_resize_active) {
				// Erase the old line
				DrawVerticalResizeLine (resize_band_start + resize_band_delta);

				resize_band_delta = e.X - resize_band_start;

				// Draw the new line
				DrawVerticalResizeLine (resize_band_start + resize_band_delta);
				return;
			}

			if (row_resize_active) {
				// Erase the old line
				DrawHorizontalResizeLine (resize_band_start + resize_band_delta);

				resize_band_delta = e.Y - resize_band_start;

				// Draw the new line
				DrawHorizontalResizeLine (resize_band_start + resize_band_delta);
				return;
			}

			Cursor new_cursor = Cursors.Default;
			HitTestInfo hit = this.HitTest (e.X, e.Y);
			
			if (hit.Type == DataGridViewHitTestType.ColumnHeader || 
			    (!ColumnHeadersVisible && hit.Type == DataGridViewHitTestType.Cell && MouseOverColumnResize (hit.ColumnIndex, e.X))) {
				EnteredHeaderCell = Columns [hit.ColumnIndex].HeaderCell;
				if (MouseOverColumnResize (hit.ColumnIndex, e.X))
					new_cursor = Cursors.VSplit;
			} else if (!RowHeadersVisible && hit.Type == DataGridViewHitTestType.Cell && MouseOverRowResize (hit.RowIndex, e.Y)) {
				EnteredHeaderCell = Rows[hit.RowIndex].HeaderCell;
				new_cursor = Cursors.HSplit;
			} else if (hit.Type == DataGridViewHitTestType.Cell) {
				EnteredHeaderCell = null;

				DataGridViewCell new_cell = GetCellInternal (hit.ColumnIndex, hit.RowIndex);
				
				// Check if we have moved into an error icon area
				Rectangle icon = new_cell.ErrorIconBounds;

				if (!icon.IsEmpty) {
					Point loc = GetCellDisplayRectangle (hit.ColumnIndex, hit.RowIndex, false).Location;
					
					icon.X += loc.X;
					icon.Y += loc.Y;
					
					if (icon.Contains (e.X, e.Y)) {
						if (tooltip_currently_showing != new_cell)
							MouseEnteredErrorIcon (new_cell);
					} else
						MouseLeftErrorIcon (new_cell);
				}
				
				Cursor = new_cursor;

				// We have never been in a cell before
				if (hover_cell == null) {
					hover_cell = new_cell;
					OnCellMouseEnter (new DataGridViewCellEventArgs (hit.ColumnIndex, hit.RowIndex));
					
					Rectangle display = GetCellDisplayRectangle (hit.ColumnIndex, hit.RowIndex, false);
					OnCellMouseMove (new DataGridViewCellMouseEventArgs (hit.ColumnIndex, hit.RowIndex, e.X - display.X, e.Y - display.Y, e));
					
					return;
				}
			
				// Were we already in this cell?
				if (hover_cell.RowIndex == hit.RowIndex && hover_cell.ColumnIndex == hit.ColumnIndex) {
					Rectangle display = GetCellDisplayRectangle (hit.ColumnIndex, hit.RowIndex, false);
					OnCellMouseMove (new DataGridViewCellMouseEventArgs (hit.ColumnIndex, hit.RowIndex, e.X - display.X, e.Y - display.Y, e));
				
					return;
				}
			
				// We are changing cells
				OnCellMouseLeave (new DataGridViewCellEventArgs (hover_cell.ColumnIndex, hover_cell.RowIndex));

				hover_cell = new_cell;
				
				OnCellMouseEnter (new DataGridViewCellEventArgs (hit.ColumnIndex, hit.RowIndex));

				Rectangle display2 = GetCellDisplayRectangle (hit.ColumnIndex, hit.RowIndex, false);
				OnCellMouseMove (new DataGridViewCellMouseEventArgs (hit.ColumnIndex, hit.RowIndex, e.X - display2.X, e.Y - display2.Y, e));

				return;
			} else if (hit.Type == DataGridViewHitTestType.RowHeader) {
				DataGridViewRowHeaderCell new_cell = Rows[hit.RowIndex].HeaderCell;

				EnteredHeaderCell = new_cell;

				if (MouseOverRowResize (hit.RowIndex, e.Y))
					new_cursor = Cursors.HSplit;

				// Check if we have moved into an error icon area
				Rectangle icon = new_cell.InternalErrorIconsBounds;

				if (!icon.IsEmpty) {
					Point loc = GetCellDisplayRectangle (0, hit.RowIndex, false).Location;

					icon.X += BorderWidth;
					icon.Y += loc.Y;

					if (icon.Contains (e.X, e.Y)) {
						if (tooltip_currently_showing != new_cell)
							MouseEnteredErrorIcon (new_cell);
					} else
						MouseLeftErrorIcon (new_cell);
				}
			} else if (hit.Type == DataGridViewHitTestType.TopLeftHeader) {
				EnteredHeaderCell = null;

				DataGridViewTopLeftHeaderCell new_cell = (DataGridViewTopLeftHeaderCell)TopLeftHeaderCell;

				// Check if we have moved into an error icon area
				Rectangle icon = new_cell.InternalErrorIconsBounds;

				if (!icon.IsEmpty) {
					Point loc = Point.Empty;

					icon.X += BorderWidth;
					icon.Y += loc.Y;

					if (icon.Contains (e.X, e.Y)) {
						if (tooltip_currently_showing != new_cell)
							MouseEnteredErrorIcon (new_cell);
					} else
						MouseLeftErrorIcon (new_cell);
				}
			
			} else {
				EnteredHeaderCell = null;

				// We have left the cell area
				if (hover_cell != null) {
					OnCellMouseLeave (new DataGridViewCellEventArgs (hover_cell.ColumnIndex, hover_cell.RowIndex));
					hover_cell = null;
				}
			}
			
			Cursor = new_cursor;
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (column_resize_active) {
				column_resize_active = false;
				
				if (resize_band_delta + Columns[resize_band].Width < 0)
					resize_band_delta = -Columns[resize_band].Width;

				Columns[resize_band].Width = Math.Max (resize_band_delta + Columns[resize_band].Width, Columns[resize_band].MinimumWidth);
				Invalidate ();
				return;
			}

			if (row_resize_active) {
				row_resize_active = false;

				if (resize_band_delta + Rows[resize_band].Height < 0)
					resize_band_delta = -Rows[resize_band].Height;

				Rows[resize_band].Height = Math.Max (resize_band_delta + Rows[resize_band].Height, Rows[resize_band].MinimumHeight);
				Invalidate ();
				return;
			}
		
			HitTestInfo hit = this.HitTest (e.X, e.Y);

			if (hit.Type == DataGridViewHitTestType.Cell) {
				Rectangle display = GetCellDisplayRectangle (hit.ColumnIndex, hit.RowIndex, false);
				OnCellMouseUp (new DataGridViewCellMouseEventArgs (hit.ColumnIndex, hit.RowIndex, e.X - display.X, e.Y - display.Y, e));
			}

			if (pressed_header_cell != null) {
				DataGridViewHeaderCell cell = pressed_header_cell;
				pressed_header_cell = null;
				if (ThemeEngine.Current.DataGridViewHeaderCellHasPressedStyle (this))
					Invalidate (GetHeaderCellBounds (cell));
			}
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			int delta = SystemInformation.MouseWheelScrollLines * verticalScrollBar.SmallChange;
			if (e.Delta < 0)
				verticalScrollBar.SafeValueSet (verticalScrollBar.Value + delta);
			else
				verticalScrollBar.SafeValueSet (verticalScrollBar.Value - delta);

			OnVScrollBarScroll (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, verticalScrollBar.Value));
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

		int first_row_index = 0;
		internal int first_col_index = 0;

		protected override void OnPaint (PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			Rectangle bounds = ClientRectangle;
			
			// Paint the background
			PaintBackground (g, e.ClipRectangle, bounds);

			List<DataGridViewColumn> sortedColumns = columns.ColumnDisplayIndexSortedArrayList;
			
			// Take borders into account
			bounds.Inflate (-BorderWidth, -BorderWidth);
			
			// Paint the top left cell
			if (rowHeadersVisible && columnHeadersVisible && ColumnCount > 0) {
				Rectangle topleftbounds = new Rectangle (bounds.X, bounds.Y, rowHeadersWidth, columnHeadersHeight);
				
				TopLeftHeaderCell.PaintWork (g, e.ClipRectangle, topleftbounds, -1, TopLeftHeaderCell.State, ColumnHeadersDefaultCellStyle, AdvancedColumnHeadersBorderStyle, DataGridViewPaintParts.All);
			}
			
			// Paint the column headers
			if (columnHeadersVisible) {
				Rectangle headerBounds = bounds;
				headerBounds.Height = columnHeadersHeight;
				
				if (rowHeadersVisible)
					headerBounds.X += rowHeadersWidth;
				
				for (int index = first_col_index; index < sortedColumns.Count; index++) {
					DataGridViewColumn col = sortedColumns[index];
					
					if (!col.Visible)
						continue;
					
					headerBounds.Width = col.Width;
					DataGridViewCell cell = col.HeaderCell;

					DataGridViewAdvancedBorderStyle intermediateBorderStyle = (DataGridViewAdvancedBorderStyle)((ICloneable)this.AdvancedColumnHeadersBorderStyle).Clone ();
					DataGridViewAdvancedBorderStyle borderStyle = AdjustColumnHeaderBorderStyle (this.AdvancedColumnHeadersBorderStyle, intermediateBorderStyle, cell.ColumnIndex == 0, cell.ColumnIndex == columns.Count - 1);

					cell.PaintWork (g, e.ClipRectangle, headerBounds, -1, cell.State, cell.InheritedStyle, borderStyle, DataGridViewPaintParts.All);
					
					headerBounds.X += col.Width;
				}

				bounds.Y += columnHeadersHeight;
			}
			
			// Reset not displayed columns to !Displayed
			for (int i = 0; i < first_col_index; i++)
				Columns[i].DisplayedInternal = false;
			
			int gridWidth = rowHeadersVisible ? rowHeadersWidth : 0;

			// Set Displayed columns
			for (int i = first_col_index; i < Columns.Count; i++) {
				DataGridViewColumn col = Columns.ColumnDisplayIndexSortedArrayList[i];

				if (!col.Visible)
					continue;
			
				col.DisplayedInternal = true;
				gridWidth += col.Width;
				if (gridWidth >= Width)
					break;
			}
			
			// Reset all not displayed rows to !Displayed
			for (int i = 0; i < first_row_index; i++)
				GetRowInternal (i).DisplayedInternal = false;
			
			// Draw rows
			for (int index = first_row_index; index < Rows.Count; index++) {
				DataGridViewRow row = Rows[index];
				if (!row.Visible)
					continue;
				GetRowInternal (index).DisplayedInternal = true;
	
				bounds.Height = row.Height;
				bool is_first = row.Index == 0;
				bool is_last = row.Index == rows.Count - 1;

				row.Paint (g, e.ClipRectangle, bounds, row.Index, row.GetState (row.Index), is_first, is_last);

				bounds.Y += bounds.Height;
				bounds.X = BorderWidth;
				
				if (bounds.Y >= ClientSize.Height - (horizontalScrollBar.Visible ? horizontalScrollBar.Height : 0))
					break;
			}

			RefreshScrollBars ();
			
			// Paint the bottom right square if both scrollbars are displayed
			if (horizontalScrollBar.Visible && verticalScrollBar.Visible)
				g.FillRectangle (SystemBrushes.Control, new Rectangle (horizontalScrollBar.Right, verticalScrollBar.Bottom, verticalScrollBar.Width, horizontalScrollBar.Height));

			// Paint the border
			bounds = ClientRectangle;
			
			switch (BorderStyle) {
				case BorderStyle.FixedSingle:
					g.DrawRectangle (Pens.Black, new Rectangle (bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1));
					break;
				case BorderStyle.Fixed3D:
					ControlPaint.DrawBorder3D (g, bounds, Border3DStyle.Sunken);
					break;
			}

			// Call the base impl at the end.
			base.OnPaint(e);
		}

		private void RefreshScrollBars ()
		{
			int gridWidth = 0;
			int gridHeight = 0;

			foreach (DataGridViewColumn col in columns.ColumnDisplayIndexSortedArrayList)
				if (col.Visible)
					gridWidth += col.Width;
			
			foreach (DataGridViewRow row in Rows)
				if (row.Visible)
					gridHeight += row.Height;

			if (rowHeadersVisible)
				gridWidth += rowHeadersWidth;
			if (columnHeadersVisible)
				gridHeight += columnHeadersHeight;

			bool horizontalVisible = false;
			bool verticalVisible = false;
			
			if (AutoSize) {
				if (gridWidth > Size.Width || gridHeight > Size.Height)
					Size = new Size(gridWidth, gridHeight);
			}
			else {
				if (gridWidth > Size.Width)
					horizontalVisible = true;
				if (gridHeight > Size.Height)
					verticalVisible = true;

				if (horizontalScrollBar.Visible && (gridHeight + horizontalScrollBar.Height) > Size.Height)
					verticalVisible = true;
				if (verticalScrollBar.Visible && (gridWidth + verticalScrollBar.Width) > Size.Width) 
					horizontalVisible = true;

				if (horizontalVisible) {
					horizontalScrollBar.Minimum = 0;
					horizontalScrollBar.Maximum = gridWidth;
					horizontalScrollBar.SmallChange = Columns[first_col_index].Width;
					int largeChange = ClientSize.Width - rowHeadersWidth - horizontalScrollBar.Height;
					if (largeChange <= 0)
						largeChange = ClientSize.Width;
					horizontalScrollBar.LargeChange = largeChange;
				}

				if (verticalVisible) {
					verticalScrollBar.Minimum = 0;
					verticalScrollBar.Maximum = gridHeight;
					int first_row_height = Rows.Count > 0 ? Rows[Math.Min (Rows.Count - 1, first_row_index)].Height : 0;
					verticalScrollBar.SmallChange = first_row_height + 1;
					int largeChange = ClientSize.Height - columnHeadersHeight - verticalScrollBar.Width;
					if (largeChange <= 0)
						largeChange = ClientSize.Height;
					verticalScrollBar.LargeChange = largeChange;
				}

				// Force the visibility of the scrollbars *after* computing the scrolling values,
				// as we need them *always* for navigation purposes.
				if (scrollBars != ScrollBars.Vertical && scrollBars != ScrollBars.Both)
					verticalVisible = false;
				if (scrollBars != ScrollBars.Horizontal && scrollBars != ScrollBars.Both)
					horizontalVisible = false;

				// MSNET compatibility here
				if (RowCount <= 1)
					verticalVisible = false;


			}

			horizontalScrollBar.Visible = horizontalVisible;
			verticalScrollBar.Visible = verticalVisible;
		}

		protected virtual void OnReadOnlyChanged (EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ReadOnlyChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnResize (EventArgs e) {
			base.OnResize(e);
			AutoResizeColumnsInternal ();
			
			OnVScrollBarScroll (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, verticalScrollBar.Value));
			OnHScrollBarScroll (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, horizontalScrollBar.Value));
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
			UpdateRowHeightInfo (e.Row.Index, false);
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
			if (hover_cell != null && hover_cell.RowIndex >= e.RowIndex)
				hover_cell = null;

			// Select the first row if we are not databound. 
			// If we are databound selection is managed by the data manager.
			if (IsHandleCreated && DataManager == null && CurrentCell == null && Rows.Count > 0 && Columns.Count > 0)
				MoveCurrentCell (ColumnDisplayIndexToIndex (0), 0, true, false, false, true);

			AutoResizeColumnsInternal ();
			if (VirtualMode) {
				for (int i = 0; i < e.RowCount; i++)
					UpdateRowHeightInfo (e.RowIndex + i, false);
			}

			Invalidate ();
			OnRowsAdded (e);
		}

		protected virtual void OnRowsAdded (DataGridViewRowsAddedEventArgs e)
		{
			DataGridViewRowsAddedEventHandler eh = (DataGridViewRowsAddedEventHandler)(Events [RowsAddedEvent]);
			if (eh != null) eh (this, e);
		}

		protected virtual void OnRowsDefaultCellStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [RowsDefaultCellStyleChangedEvent]);
			if (eh != null) eh (this, e);
		}

		internal void OnRowsPreRemovedInternal (DataGridViewRowsRemovedEventArgs e)
		{
			// All removed rows should be removed from the selection too.
			if (selected_rows != null)
			{
				int lastRowIndex = e.RowIndex + e.RowCount;
				for (int rowIndex = e.RowIndex; rowIndex < lastRowIndex; ++rowIndex)
					SetSelectedRowCore (rowIndex, false);
			}

			if (Rows.Count - e.RowCount <= 0) {
				MoveCurrentCell (-1, -1, true, false, false, true);
				hover_cell = null;
			} else if (Columns.Count == 0) {
				MoveCurrentCell (-1, -1, true, false, false, true);
				hover_cell = null;
			} else if (currentCell != null && currentCell.RowIndex == e.RowIndex) {
				int nextRowIndex = e.RowIndex;
				if (nextRowIndex >= Rows.Count - e.RowCount)
					nextRowIndex = Rows.Count - 1 - e.RowCount;
				MoveCurrentCell (currentCell != null ? currentCell.ColumnIndex : 0, nextRowIndex, 
						 true, false, false, true);
				if (hover_cell != null && hover_cell.RowIndex >= e.RowIndex)
					hover_cell = null;
			}
		}

		internal void OnRowsPostRemovedInternal (DataGridViewRowsRemovedEventArgs e)
		{
			Invalidate ();
			OnRowsRemoved (e);
		}

		protected virtual void OnRowsRemoved (DataGridViewRowsRemovedEventArgs e)
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
			PrepareEditingRow (false, false);

			new_row_editing = true;
			if (DataManager != null) {
				// Switch the current editing row with a real one
				if (editing_row != null) {
					Rows.RemoveInternal (editing_row);
					editing_row = null;
				}
				DataManager.AddNew (); // will raise OnListPositionChanged
			}

			e = new DataGridViewRowEventArgs (Rows[NewRowIndex]);
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
			if (!MultiSelect)
				return false;
				
			if ((keyData & Keys.Control) == Keys.Control) {
				SelectAll ();
				return true;
			}
			
			return false;
		}

		protected virtual bool ProcessDataGridViewKey (KeyEventArgs e)
		{
			switch (e.KeyData & ~Keys.Modifiers) {
				case Keys.A:
					return ProcessAKey (e.KeyData);
				case Keys.Delete:
					return ProcessDeleteKey (e.KeyData);
				case Keys.Down:
					return ProcessDownKey (e.KeyData);
				case Keys.Escape:
					return ProcessEscapeKey (e.KeyData);
				case Keys.End:
					return ProcessEndKey (e.KeyData);
				case Keys.Enter:
					return ProcessEnterKey (e.KeyData);
				case Keys.F2:
					return ProcessF2Key (e.KeyData);
				case Keys.Home:
					return ProcessHomeKey (e.KeyData);
				case Keys.Left:
					return ProcessLeftKey (e.KeyData);
				case Keys.Next:
					return ProcessNextKey (e.KeyData);
				case Keys.Prior:
					return ProcessPriorKey (e.KeyData);
				case Keys.Right:
					return ProcessRightKey (e.KeyData);
				case Keys.Space:
					return ProcessSpaceKey (e.KeyData);
				case Keys.Tab:
					return ProcessTabKey (e.KeyData);
				case Keys.Up:
					return ProcessUpKey (e.KeyData);
				case Keys.D0:
				case Keys.NumPad0:
					return ProcessZeroKey (e.KeyData);
			}
			
			return false;
		}

		protected bool ProcessDeleteKey (Keys keyData)
		{
			if (!AllowUserToDeleteRows || SelectedRows.Count == 0)
				return false;

			for (int i = SelectedRows.Count - 1; i >= 0; i--) {
				DataGridViewRow row = SelectedRows[i];

				if (row.IsNewRow)
					continue;

				if (hover_cell != null && hover_cell.OwningRow == row)
					hover_cell = null;
					
				if (DataManager != null)
					DataManager.RemoveAt (row.Index);
				else
					Rows.RemoveAt (row.Index);
			}

			return true;
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			switch (keyData) {
				case Keys.Tab:
				case Keys.Shift | Keys.Tab:
					if (standardTab)
						return base.ProcessDialogKey (keyData & ~Keys.Control);
						
					if (ProcessDataGridViewKey (new KeyEventArgs (keyData)))
						return true;
						
					break;
				case Keys.Control | Keys.Tab:
				case Keys.Control | Keys.Shift | Keys.Tab:
					if (!standardTab)
						return base.ProcessDialogKey (keyData & ~Keys.Control);

					if (ProcessDataGridViewKey (new KeyEventArgs (keyData)))
						return true;
						
					break;
				case Keys.Enter:
				case Keys.Escape:
					if (ProcessDataGridViewKey (new KeyEventArgs (keyData)))
						return true;
						
					break;
			}
			
			return base.ProcessDialogKey(keyData);
		}

		protected bool ProcessDownKey (Keys keyData)
		{
			int current_row = CurrentCellAddress.Y;
			
			if (current_row < Rows.Count - 1) {
				// Move to the last cell in the column
				if ((keyData & Keys.Control) == Keys.Control)
					MoveCurrentCell (CurrentCellAddress.X, Rows.Count - 1, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);
				// Move one cell down
				else
					MoveCurrentCell (CurrentCellAddress.X, current_row + 1, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);
				
				return true;
			}
			
			return false;
		}

		protected bool ProcessEndKey (Keys keyData)
		{
			int disp_index = ColumnIndexToDisplayIndex (currentCellAddress.X);

			// Move to the last cell in the control
			if ((keyData & Keys.Control) == Keys.Control) {
				MoveCurrentCell (ColumnDisplayIndexToIndex (Columns.Count - 1), Rows.Count - 1, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);
				return true;
			}
			
			// Move to the last cell in the row
			if (disp_index < Columns.Count - 1) {
				MoveCurrentCell (ColumnDisplayIndexToIndex (Columns.Count - 1), currentCellAddress.Y, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);
				return true;
			}

			return false;
		}

		protected bool ProcessEnterKey (Keys keyData)
		{
			if (ProcessDownKey (keyData))
				return true;
			
			// ProcessDown may fail if we are on the last row,
			// but Enter should still EndEdit if this is the last row
			EndEdit ();
			return true;
		}

		protected bool ProcessEscapeKey (Keys keyData)
		{
			if (!IsCurrentCellInEditMode)
				return false;

			CancelEdit ();
			return true;
		}

		protected bool ProcessF2Key (Keys keyData)
		{
			if (editMode == DataGridViewEditMode.EditOnF2 || editMode == DataGridViewEditMode.EditOnKeystrokeOrF2) {
				BeginEdit (true);
				return true;
			}
			
			return false;
		}

		protected bool ProcessHomeKey (Keys keyData)
		{
			int disp_index = ColumnIndexToDisplayIndex (currentCellAddress.X);

			// Move to the first cell in the control
			if ((keyData & Keys.Control) == Keys.Control) {
				MoveCurrentCell (ColumnDisplayIndexToIndex (0), 0, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);
				return true;
			}
			
			// Move to the first cell in the row
			if (disp_index > 0) {
				MoveCurrentCell (ColumnDisplayIndexToIndex (0), currentCellAddress.Y, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);
				return true;
			}

			return false;
		}

		[MonoInternalNote ("What does insert do?")]
		protected bool ProcessInsertKey (Keys keyData)
		{
			return false;
		}

		protected override bool ProcessKeyEventArgs (ref Message m)
		{
			DataGridViewCell cell = CurrentCell;
			
			if (cell != null) {
				if (cell.KeyEntersEditMode (new KeyEventArgs ((Keys)m.WParam.ToInt32 ())))
					BeginEdit (true);
				if (EditingControl != null && ((Msg)m.Msg == Msg.WM_KEYDOWN || (Msg)m.Msg == Msg.WM_CHAR))
					XplatUI.SendMessage (EditingControl.Handle, (Msg)m.Msg, m.WParam, m.LParam);
			}

			return base.ProcessKeyEventArgs (ref m);
		}

		protected override bool ProcessKeyPreview (ref Message m)
		{
			if ((Msg)m.Msg == Msg.WM_KEYDOWN && (IsCurrentCellInEditMode || m.HWnd == horizontalScrollBar.Handle || m.HWnd == verticalScrollBar.Handle)) {
				KeyEventArgs e = new KeyEventArgs ((Keys)m.WParam.ToInt32 ());
			
				IDataGridViewEditingControl ctrl = (IDataGridViewEditingControl)EditingControlInternal;
				
				if (ctrl != null)
					if (ctrl.EditingControlWantsInputKey (e.KeyData, false))
						return false;

				switch (e.KeyData) {
					case Keys.Escape:
					case Keys.Down:
					case Keys.Up:
					case Keys.Left:
					case Keys.Right:
					case Keys.Tab:
					case Keys.Prior:
					case Keys.Next:
						return ProcessDataGridViewKey (e);
				}
			}
			
			return base.ProcessKeyPreview (ref m);
		}

		protected bool ProcessLeftKey (Keys keyData)
		{
			int disp_index = ColumnIndexToDisplayIndex (currentCellAddress.X);

			if (disp_index > 0) {
				// Move to the first cell in the row
				if ((keyData & Keys.Control) == Keys.Control)
					MoveCurrentCell (ColumnDisplayIndexToIndex (0), currentCellAddress.Y, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);
				// Move one cell to the left
				else
					MoveCurrentCell (ColumnDisplayIndexToIndex (disp_index - 1), currentCellAddress.Y, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);

				return true;
			}

			return false;
		}

		// Page Down
		protected bool ProcessNextKey (Keys keyData)
		{
			int current_row = CurrentCellAddress.Y;

			if (current_row < Rows.Count - 1) {
				// Move one "page" of cells down
				int new_row = Math.Min (Rows.Count - 1, current_row + DisplayedRowCount (false));

				MoveCurrentCell (CurrentCellAddress.X, new_row, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);

				return true;
			}

			return false;
		}

		// Page Up
		protected bool ProcessPriorKey (Keys keyData)
		{
			int current_row = CurrentCellAddress.Y;

			if (current_row > 0) {
				// Move one "page" of cells up
				int new_row = Math.Max (0, current_row - DisplayedRowCount (false));

				MoveCurrentCell (CurrentCellAddress.X, new_row, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);

				return true;
			}

			return false;
		}

		protected bool ProcessRightKey (Keys keyData)
		{
			int disp_index = ColumnIndexToDisplayIndex (currentCellAddress.X);

			if (disp_index < Columns.Count - 1) {
				// Move to the last cell in the row
				if ((keyData & Keys.Control) == Keys.Control)
					MoveCurrentCell (ColumnDisplayIndexToIndex (Columns.Count - 1), currentCellAddress.Y, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);
				// Move one cell to the right
				else
					MoveCurrentCell (ColumnDisplayIndexToIndex (disp_index + 1), currentCellAddress.Y, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);
				
				return true;
			}

			return false;
		}

		protected bool ProcessSpaceKey (Keys keyData)
		{
			if ((keyData & Keys.Shift) == Keys.Shift) {
				if (selectionMode == DataGridViewSelectionMode.RowHeaderSelect) {
					SetSelectedRowCore (CurrentCellAddress.Y, true);
					InvalidateRow (CurrentCellAddress.Y);
					return true;
				}
				if (selectionMode == DataGridViewSelectionMode.ColumnHeaderSelect) {
					SetSelectedColumnCore (CurrentCellAddress.X, true);
					InvalidateColumn (CurrentCellAddress.X);
					return true;
				}
			}
			
			if (CurrentCell is DataGridViewButtonCell || CurrentCell is DataGridViewLinkCell || CurrentCell is DataGridViewCheckBoxCell) {
				DataGridViewCellEventArgs e = new DataGridViewCellEventArgs (CurrentCell.ColumnIndex, CurrentCell.RowIndex);
				
				OnCellClick (e);
				OnCellContentClick (e);
				
				if (CurrentCell is DataGridViewButtonCell)
					(CurrentCell as DataGridViewButtonCell).OnClickInternal (e);
				if (CurrentCell is DataGridViewCheckBoxCell)
					(CurrentCell as DataGridViewCheckBoxCell).OnClickInternal (e);
					
				return true;
			}
			
			return false;
		}

		protected bool ProcessTabKey (Keys keyData)
		{
			Form f = FindForm ();
			
			if (f != null)
				f.ActivateFocusCues ();
			
			int disp_index = ColumnIndexToDisplayIndex (currentCellAddress.X);

			// Tab goes forward
			// Shift-tab goes backwards
			if ((keyData & Keys.Shift) == Keys.Shift) {
				if (disp_index > 0) {
					// Move one cell to the left
					MoveCurrentCell (ColumnDisplayIndexToIndex (disp_index - 1), currentCellAddress.Y, true, (keyData & Keys.Control) == Keys.Control, false, true);
					return true;
				} else if (currentCellAddress.Y > 0) {
					// Move to the last cell in the previous row
					MoveCurrentCell (ColumnDisplayIndexToIndex (Columns.Count - 1), currentCellAddress.Y - 1, true, false, false, true);
					return true;
				}
			
			} else {
				if (disp_index < Columns.Count - 1) {
					// Move one cell to the right
					MoveCurrentCell (ColumnDisplayIndexToIndex (disp_index + 1), currentCellAddress.Y, true, (keyData & Keys.Control) == Keys.Control, false, true);

					return true;
				} else if (currentCellAddress.Y < Rows.Count - 1) {
					// Move to the first cell in the next row
					MoveCurrentCell (ColumnDisplayIndexToIndex (0), currentCellAddress.Y + 1, true, false, false, true);
					return true;
				}

			
			}
			
			return false;
		}

		protected bool ProcessUpKey (Keys keyData)
		{
			int current_row = CurrentCellAddress.Y;

			if (current_row > 0) {
				// Move to the first cell in the column
				if ((keyData & Keys.Control) == Keys.Control)
					MoveCurrentCell (CurrentCellAddress.X, 0, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);
				// Move one cell up
				else
					MoveCurrentCell (CurrentCellAddress.X, current_row - 1, true, (keyData & Keys.Control) == Keys.Control, (keyData & Keys.Shift) == Keys.Shift, true);

				return true;
			}

			return false;
		}

		protected bool ProcessZeroKey (Keys keyData)
		{
			if ((keyData & Keys.Control) == Keys.Control && CurrentCell.EditType != null) {
				CurrentCell.Value = DBNull.Value;
				InvalidateCell (CurrentCell);
				return true;
			}
			
			return false;
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified) {
			base.SetBoundsCore(x, y, width, height, specified);
		}

		[MonoTODO ("Does not use validateCurrentCell")]
		protected virtual bool SetCurrentCellAddressCore (int columnIndex, int rowIndex, bool setAnchorCellAddress, bool validateCurrentCell, bool throughMouseClick)
		{
			if ((columnIndex < 0 || columnIndex > Columns.Count - 1) && rowIndex != -1)
				throw new ArgumentOutOfRangeException ("columnIndex");
			if ((rowIndex < 0 || rowIndex > Rows.Count - 1) && columnIndex != -1)
				throw new ArgumentOutOfRangeException ("rowIndex");
			
			DataGridViewCell cell;
			
			if (columnIndex == -1 && rowIndex == -1)
				cell = null;
			else
				cell = Rows.SharedRow (rowIndex).Cells[columnIndex];
			
			if (cell != null && !cell.Visible)
				throw new InvalidOperationException ("cell is not visible");
				
			// Always update the current cell address property
			// If the row has moved it would be out of date.
			if (currentCell != null) {
				if (setAnchorCellAddress) {
					anchor_cell.X = currentCell.ColumnIndex;
					anchor_cell.Y = currentCell.RowIndex;
				}
				currentCellAddress.X = currentCell.ColumnIndex;
				currentCellAddress.Y = currentCell.RowIndex;
			}

			if (cell != currentCell) {
				if (currentCell != null) {
					if (currentCell.IsInEditMode) {
						if (!EndEdit ())
							return false;
						else if (currentCell.RowIndex == NewRowIndex && new_row_editing)
							CancelEdit ();
					} else {
						// CancelEdit will replace the uncommited real editing row with a place holder row
						if (new_row_editing && currentCell.RowIndex == NewRowIndex)
							CancelEdit ();
					}
					OnCellLeave (new DataGridViewCellEventArgs(currentCell.ColumnIndex, currentCell.RowIndex));
					OnRowLeave (new DataGridViewCellEventArgs (currentCell.ColumnIndex, currentCell.RowIndex));
				}

				currentCell = cell;
				if (setAnchorCellAddress)
					anchor_cell = new Point (columnIndex, rowIndex);
				currentCellAddress = new Point (columnIndex, rowIndex);

				if (cell != null) {
					UpdateBindingPosition (cell.RowIndex);
					OnRowEnter (new DataGridViewCellEventArgs (cell.ColumnIndex, cell.RowIndex));
					OnCellEnter (new DataGridViewCellEventArgs(cell.ColumnIndex, cell.RowIndex));
				}
				OnCurrentCellChanged (EventArgs.Empty);

				if (cell != null) {
					// If the user begins an edit in the NewRow, add a new real row
					if (AllowUserToAddRows && cell.RowIndex == NewRowIndex && !is_binding && !new_row_editing) {
						// OnUserAddedRow will add a real row and reset the current cell
						OnUserAddedRow (new DataGridViewRowEventArgs (Rows[NewRowIndex]));
					} else {
						if (editMode == DataGridViewEditMode.EditOnEnter)
							BeginEdit (true);
					}
				}
			} else {
				if (cell != null && throughMouseClick)
					BeginEdit (true);
			}

			return true;
		}

		protected virtual void SetSelectedCellCore (int columnIndex, int rowIndex, bool selected) {
			rows [rowIndex].Cells [columnIndex].Selected = selected;
			
			OnSelectionChanged (EventArgs.Empty);
		}

		internal void SetSelectedColumnCoreInternal (int columnIndex, bool selected) {
			SetSelectedColumnCore (columnIndex, selected);
		}	
		
		protected virtual void SetSelectedColumnCore (int columnIndex, bool selected) {
			if (selectionMode != DataGridViewSelectionMode.ColumnHeaderSelect && selectionMode != DataGridViewSelectionMode.FullColumnSelect)
				return; 
			
			DataGridViewColumn col = columns [columnIndex];
			
			col.SelectedInternal = selected;
			
			if (selected_columns == null)
				selected_columns = new DataGridViewSelectedColumnCollection ();

			bool selectionChanged = false;
			if (!selected && selected_columns.Contains (col)) {
				selected_columns.InternalRemove (col);
				selectionChanged = true;
			} else if (selected && !selected_columns.Contains (col)) {
				selected_columns.InternalAdd (col);
				selectionChanged = true;
			}

			if (selectionChanged)
				OnSelectionChanged (EventArgs.Empty);

			Invalidate();
		}

		internal void SetSelectedRowCoreInternal (int rowIndex, bool selected) {
			if (rowIndex >= 0 && rowIndex < Rows.Count)
				SetSelectedRowCore (rowIndex, selected);
		}	

		protected virtual void SetSelectedRowCore (int rowIndex, bool selected) {
			DataGridViewRow row = rows [rowIndex];
			
			row.SelectedInternal = selected;
			
			if (selected_rows == null)
				selected_rows = new DataGridViewSelectedRowCollection (this);

			bool selectionChanged = false;
			if (!selected && selected_rows.Contains (row)) {
				selected_rows.InternalRemove (row);
				selectionChanged = true;
			} else if (selected && !selected_rows.Contains (row)) {
				selected_rows.InternalAdd (row);
				selectionChanged = true;
			}

			if (selectionChanged)
				OnSelectionChanged (EventArgs.Empty);

			Invalidate();
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}

		internal void InternalOnCellClick (DataGridViewCellEventArgs e)
		{
			OnCellClick (e);
		}

		internal void InternalOnCellContentClick (DataGridViewCellEventArgs e)
		{
			OnCellContentClick (e);
		}

		internal void InternalOnCellContentDoubleClick (DataGridViewCellEventArgs e)
		{
			OnCellContentDoubleClick (e);
		}

		internal void InternalOnCellValueChanged (DataGridViewCellEventArgs e)
		{
			OnCellValueChanged (e);
		}

		internal void InternalOnDataError (DataGridViewDataErrorEventArgs e)
		{
			/////////////// false? ////////////
			OnDataError (false, e);
		}

		internal void InternalOnMouseWheel (MouseEventArgs e)
		{
			OnMouseWheel (e);
		}

		internal void OnHScrollBarScroll (object sender, ScrollEventArgs e)
		{
			int lastRightVisibleColumntIndex = Columns.Count - DisplayedColumnCount (false);
			horizontalScrollingOffset = e.NewValue;
			int left = 0;

			for (int index = 0; index < Columns.Count; index++) {
				DataGridViewColumn col = Columns[index];

				if (col.Index >= lastRightVisibleColumntIndex) {
					first_col_index = lastRightVisibleColumntIndex;
					Invalidate ();
					OnScroll (e);
				} else if (e.NewValue < left + col.Width) {
					if (first_col_index != index) {
						first_col_index = index;
						Invalidate ();
						OnScroll (e);
					}

					return;
				}

				left += col.Width;
			}
		}

		internal void OnVScrollBarScroll (object sender, ScrollEventArgs e)
		{
			verticalScrollingOffset = e.NewValue;
			if (Rows.Count == 0)
				return;

			int top = 0;
			int lastTopVisibleRowIndex = Rows.Count - DisplayedRowCount (false);

			for (int index = 0; index < Rows.Count; index++) {
				DataGridViewRow row = Rows[index];
				if (!row.Visible)
					continue;

				if (row.Index >= lastTopVisibleRowIndex) {
					first_row_index = lastTopVisibleRowIndex;
					Invalidate ();
					OnScroll (e);
				} else if (e.NewValue < top + row.Height) {
					if (first_row_index != index) {
						first_row_index = index;
						Invalidate ();
						OnScroll (e);
					}
					
					return;
				}
				
				top += row.Height;
			}
			
			first_row_index = lastTopVisibleRowIndex;
			Invalidate ();
			OnScroll (e);
		}

		internal void RaiseCellStyleChanged (DataGridViewCellEventArgs e) {
			OnCellStyleChanged(e);
		}

		internal void OnColumnCollectionChanged (object sender, CollectionChangeEventArgs e) {
			switch (e.Action) {
				case CollectionChangeAction.Add:
					OnColumnAddedInternal(new DataGridViewColumnEventArgs(e.Element as DataGridViewColumn));
					break;
				case CollectionChangeAction.Remove:
					OnColumnPostRemovedInternal(new DataGridViewColumnEventArgs(e.Element as DataGridViewColumn));
					break;
				case CollectionChangeAction.Refresh:
					hover_cell = null;
					MoveCurrentCell (-1, -1, true, false, false, true);
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
			int spaceLeft = ClientSize.Width - (verticalScrollBar.VisibleInternal ? verticalScrollBar.Width : 0);

			if (RowHeadersVisible) {
				spaceLeft -= RowHeadersWidth;
			}
			spaceLeft -= BorderWidth * 2;
			
			int [] fixed_widths = new int [Columns.Count];
			int [] new_widths = new int [Columns.Count];
			bool fixed_any = false;
			
			for (int i = 0; i < Columns.Count; i++) {
				DataGridViewColumn col = Columns [i];

				if (!col.Visible)
					continue;

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
				
					if (!col.Visible)
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

				if (!Columns[i].Visible)
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
				size  = Math.Max (CalculateColumnCellWidth (columnIndex, col.InheritedAutoSizeMode), 
						  col.HeaderCell.ContentBounds.Width);
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
			int last_row = Rows.Count;
			int result = 0;

			if (mode == DataGridViewAutoSizeColumnMode.DisplayedCells || 
			    mode == DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader) {
				first_row = first_row_index;
				last_row = DisplayedRowCount (true);;
			}
			
			for (int i = first_row; i < last_row; i++) {
				if (!Rows[i].Visible)
					continue;
				
				int cell_width = Rows[i].Cells[index].PreferredSize.Width;
				result = Math.Max (result, cell_width);
			}
			
			return result;
		}

		Rectangle GetHeaderCellBounds (DataGridViewHeaderCell cell)
		{
			Rectangle bounds = new Rectangle (ClientRectangle.Location, cell.Size);
			if (cell is DataGridViewColumnHeaderCell) {
				if (RowHeadersVisible)
					bounds.X += RowHeadersWidth;
				List<DataGridViewColumn> sortedColumns = columns.ColumnDisplayIndexSortedArrayList;
				for (int index = first_col_index; index < sortedColumns.Count; index++) {
					DataGridViewColumn column = sortedColumns [index];
					if (column.Index == cell.ColumnIndex)
						break;
					bounds.X += column.Width;
				}
			} else {
				if (ColumnHeadersVisible)
					bounds.Y += ColumnHeadersHeight;
				for (int index = first_row_index; index < Rows.Count; index++) {
					DataGridViewRow row = GetRowInternal (index);
					if (row.HeaderCell == cell)
						break;
					bounds.Y += row.Height;
				}
			}
			return bounds;
		}

		private void PrepareEditingRow (bool cell_changed, bool column_changed)
		{
			if (new_row_editing)
				return;

			bool show = false;
			
			show = ColumnCount > 0 && AllowUserToAddRows;

			if (!show) {
				RemoveEditingRow ();
			} else if (show) {
				if (editing_row != null && (cell_changed || column_changed)) {
					// The row changed, it's no longer an editing row.
					//    or
					// The number of columns has changed, we need a new editing row.
					RemoveEditingRow ();
				}
				if (editing_row == null) {
					editing_row = RowTemplateFull;
					Rows.AddInternal (editing_row, false);
				}
			}
		}
		
		internal void RemoveEditingRow ()
		{
			if (editing_row != null) {
				if (Rows.Contains (editing_row))
				    Rows.RemoveInternal (editing_row);
				editing_row = null;
			}
		}

		internal DataGridViewRow EditingRow {
			get { return editing_row; }
		}

		private void AddBoundRow (object element)
		{
			// Don't add rows if there are no columns
			if (ColumnCount == 0)
				return;
				
			DataGridViewRow row = (DataGridViewRow)RowTemplateFull;
			rows.AddInternal (row, false);
		}
		
		private bool IsColumnAlreadyBound (string name)
		{
			foreach (DataGridViewColumn col in Columns)
				if (String.Compare (col.DataPropertyName, name, true) == 0)
					return true;

			return false;
		}

		private DataGridViewColumn CreateColumnByType (Type type)
		{
			if (type == typeof (bool))
				return new DataGridViewCheckBoxColumn ();
			else if (typeof(Bitmap).IsAssignableFrom (type))
				return new DataGridViewImageColumn ();
				
			return new DataGridViewTextBoxColumn ();
		}
		
		private void ClearBinding ()
		{
			if (IsCurrentCellInEditMode && !EndEdit ())
				CancelEdit ();
			MoveCurrentCell (-1, -1, false, false, false, true);

			if (DataManager != null) {
				DataManager.ListChanged -= OnListChanged;
				DataManager.PositionChanged -= OnListPositionChanged;
				columns.ClearAutoGeneratedColumns ();
				rows.Clear ();
				RemoveEditingRow ();
			}
		}

		private void ResetRows ()
		{
			rows.Clear ();
			RemoveEditingRow ();
			if (DataManager != null) {
				foreach (object element in DataManager.List)
					AddBoundRow (element);
			}
			PrepareEditingRow (false, true);
			OnListPositionChanged (this, EventArgs.Empty);
		}
		
		private void DoBinding ()
		{
			/* The System.Windows.Forms.DataGridView class supports the standard Windows Forms data-binding model. This means the data source can be of any type that implements:
			 - the System.Collections.IList interface, including one-dimensional arrays.
			 - the System.ComponentModel.IListSource interface, such as the System.Data.DataTable and System.Data.DataSet classes.
			 - the System.ComponentModel.IBindingList interface, such as the System.ComponentModel.Collections.BindingList<> class.
			 - the System.ComponentModel.IBindingListView interface, such as the System.Windows.Forms.BindingSource class.
			*/
			
			if (dataSource != null && DataManager != null) {
				if (autoGenerateColumns) {
					is_autogenerating_columns = true;

					foreach (PropertyDescriptor property in DataManager.GetItemProperties()) {
						// This keeps out things like arrays
						if ((typeof(ICollection).IsAssignableFrom (property.PropertyType)))
							continue;
						if (!property.IsBrowsable)
							continue;

						if (IsColumnAlreadyBound (property.Name))
							continue;

						DataGridViewColumn col = CreateColumnByType (property.PropertyType);
						col.Name = property.DisplayName;
						col.DataPropertyName = property.Name;
						col.ReadOnly = !DataManager.AllowEdit || property.IsReadOnly;
						col.ValueType = property.PropertyType;
						col.AutoGenerated = true;
						columns.Add (col);
					}

					is_autogenerating_columns = false;
				}

				// DataBind both autogenerated and not columns if there is a matching property
				foreach (DataGridViewColumn column in columns)
					column.DataColumnIndex = FindDataColumnIndex (column);

				foreach (object element in DataManager.List)
					AddBoundRow (element);

				DataManager.ListChanged += OnListChanged;
				DataManager.PositionChanged += OnListPositionChanged;
				OnDataBindingComplete (new DataGridViewBindingCompleteEventArgs (ListChangedType.Reset));
				OnListPositionChanged (this, EventArgs.Empty);
			} else {
				if (Rows.Count > 0 && Columns.Count > 0)
					MoveCurrentCell (0, 0, true, false, false, false);
			}

			PrepareEditingRow (false, true);
		}
		
		private void MoveCurrentCell (int x, int y, bool select, bool isControl, bool isShift, bool scroll)
		{
			if (x == -1 || y == -1)
				x = y = -1;
			else {
				if (x < 0 || x > Columns.Count - 1)
					throw new ArgumentOutOfRangeException ("x");
				if (y < 0 || y > Rows.Count - 1)
					throw new ArgumentOutOfRangeException ("y");

				if (!Rows[y].Visible) {
					for (int i = y; i < Rows.Count; i++) {
						if (Rows[i].Visible) {
							y = i;
							break;
						}
					}
				}

				if (!Columns[x].Visible) {
					for (int i = x; i < Columns.Count; i++) {
						if (Columns[i].Visible) {
							x = i;
							break;
						}
					}
				}

				// in case either no visible columns or rows
				if (!Rows[y].Visible || !Columns[x].Visible)
					x = y = -1;
			}

			if (!SetCurrentCellAddressCore (x, y, true, false, false)) {
				ClearSelection ();
				return;
			}
			if (x == -1 && y == -1) {
				ClearSelection ();
				return;
			}

			bool full_row_selected = Rows.SharedRow(CurrentCellAddress.Y).Selected;
			bool full_col_selected = Columns[CurrentCellAddress.X].Selected;
			
			// Move Selection
			DataGridViewSelectionMode mode = selectionMode;
			
			// If we are row header select and we clicked a row header, use full row
			if (mode == DataGridViewSelectionMode.RowHeaderSelect && (x == -1 || (full_row_selected && CurrentCellAddress.X == x)))
				mode = DataGridViewSelectionMode.FullRowSelect;
			else if (mode == DataGridViewSelectionMode.RowHeaderSelect)
				mode = DataGridViewSelectionMode.CellSelect;
				
			// If we are col header select and we clicked a col header, use full col
			if (mode == DataGridViewSelectionMode.ColumnHeaderSelect && (y == -1 || (full_col_selected && CurrentCellAddress.Y == y)))
				mode = DataGridViewSelectionMode.FullColumnSelect;
			else if (mode == DataGridViewSelectionMode.ColumnHeaderSelect)
				mode = DataGridViewSelectionMode.CellSelect;
			
			// If the current cell isn't visible, scroll to it
			if (scroll) {
				int disp_x = ColumnIndexToDisplayIndex (x);
				bool scrollbarsRefreshed = false;
				int displayedColumnsCount = DisplayedColumnCount (false);
				int delta_x = 0;

				// The trick here is that in order to avoid unnecessary calculations each time a row/column 
				// is added/removed we recalculate the whole grid size just before the scroll to selection.
				if (disp_x < first_col_index) {
					RefreshScrollBars ();
					scrollbarsRefreshed = true;

					if (disp_x == 0)
						delta_x = horizontalScrollBar.Value;
					else {
						// in case the column got removed
						if (first_col_index >= ColumnCount)
							first_col_index = ColumnCount - 1;
						for (int i = disp_x; i < first_col_index; i++)
							delta_x += Columns[ColumnDisplayIndexToIndex (i)].Width;
					}
				
					horizontalScrollBar.SafeValueSet (horizontalScrollBar.Value - delta_x);
					OnHScrollBarScroll (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, horizontalScrollBar.Value));
				} else if (disp_x > first_col_index + displayedColumnsCount - 1 && disp_x != 0) {
					RefreshScrollBars ();
					scrollbarsRefreshed = true;
					
					if (disp_x == Columns.Count - 1)
						delta_x = horizontalScrollBar.Maximum - horizontalScrollBar.Value;
					else
						for (int i = first_col_index + displayedColumnsCount - 1; i < disp_x && i != -1; i++)
							delta_x += Columns[ColumnDisplayIndexToIndex (i)].Width;

					horizontalScrollBar.SafeValueSet (horizontalScrollBar.Value + delta_x);
					OnHScrollBarScroll (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, horizontalScrollBar.Value));
				}

				int disp_y = y;
				int displayedRowsCount = DisplayedRowCount (false);
				int delta_y = 0;

				if (disp_y < first_row_index) {
					if (!scrollbarsRefreshed)
						RefreshScrollBars ();

					if (disp_y == 0)
						delta_y = verticalScrollBar.Value;
					else {
						// in case the row got removed
						if (first_row_index >= RowCount)
							first_row_index = RowCount - 1;
						for (int i = disp_y; i < first_row_index; i++)
							delta_y += GetRowInternal (i).Height;
					}

					verticalScrollBar.SafeValueSet (verticalScrollBar.Value - delta_y);
					OnVScrollBarScroll (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, verticalScrollBar.Value));
				} else if (disp_y > first_row_index + displayedRowsCount - 1 && disp_y != 0) {
					if (!scrollbarsRefreshed)
						RefreshScrollBars ();

					if (disp_y == Rows.Count - 1)
						delta_y = verticalScrollBar.Maximum - verticalScrollBar.Value;
					else
						for (int i = first_row_index + displayedRowsCount - 1; i < disp_y; i++)
							delta_y += GetRowInternal (i).Height;

					verticalScrollBar.SafeValueSet (verticalScrollBar.Value + delta_y);
					OnVScrollBarScroll (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, verticalScrollBar.Value));				
				}
			}
			
			if (!select)
				return;
			
			// Clear old selection unless multi-selecting
			if (!isShift)
				ClearSelection ();
			
			switch (mode) {
				case DataGridViewSelectionMode.CellSelect:
					SetSelectedCellCore (x, y, true);
					break;
				case DataGridViewSelectionMode.FullRowSelect:
					SetSelectedRowCore (y, true);
					break;
				case DataGridViewSelectionMode.FullColumnSelect:
					SetSelectedColumnCore (x, true);
					break;
			}
			
			Invalidate ();
		}
		
		private int ColumnIndexToDisplayIndex (int index)
		{
			if (index == -1)
				return index;
			return Columns[index].DisplayIndex;
		}
		
		private int ColumnDisplayIndexToIndex (int index)
		{
			return Columns.ColumnDisplayIndexSortedArrayList[index].Index;
		}
		
		private void OnListChanged (object sender, ListChangedEventArgs args)
		{
			switch (args.ListChangedType) {
				case ListChangedType.ItemAdded:
					AddBoundRow (DataManager[args.NewIndex]);
					break;
				case ListChangedType.ItemDeleted:
					Rows.RemoveAtInternal (args.NewIndex);
					break;
				case ListChangedType.ItemChanged:
					break;
				default:
					ResetRows ();
					break;
			}
			
			Invalidate ();
		}
		
		private void OnListPositionChanged (object sender, EventArgs args)
		{
			if (Rows.Count > 0 && Columns.Count > 0 && DataManager.Position != -1)
				MoveCurrentCell (currentCell != null ? currentCell.ColumnIndex : 0, DataManager.Position, 
						 true, false, false, true);
			else
				MoveCurrentCell (-1, -1, true, false, false, true);
		}

		private void ReBind ()
		{
			if (!is_binding) {
				SuspendLayout ();

				is_binding = true;
				ClearBinding ();
				DoBinding ();
				is_binding = false;

				ResumeLayout (true);
				Invalidate ();
			}
		}
		
		private bool MouseOverColumnResize (int col, int mousex)
		{
			if (!allowUserToResizeColumns)
				return false;
				
			Rectangle col_bounds = GetCellDisplayRectangle (col, 0, false);

			if (mousex >= col_bounds.Right - 4 && mousex <= col_bounds.Right)
				return true;

			return false;
		}

		private bool MouseOverRowResize (int row, int mousey)
		{
			if (!allowUserToResizeRows)
				return false;

			Rectangle row_bounds = GetCellDisplayRectangle (0, row, false);

			if (mousey >= row_bounds.Bottom - 4 && mousey <= row_bounds.Bottom)
				return true;

			return false;
		}

		private void DrawVerticalResizeLine (int x)
		{
			Rectangle splitter = new Rectangle (x, Bounds.Y + 3 + (ColumnHeadersVisible ? ColumnHeadersHeight : 0), 1, Bounds.Height - 3 - (ColumnHeadersVisible ? ColumnHeadersHeight : 0));
			XplatUI.DrawReversibleRectangle (Handle, splitter, 2);
		}

		private void DrawHorizontalResizeLine (int y)
		{
			Rectangle splitter = new Rectangle (Bounds.X + 3 + (RowHeadersVisible ? RowHeadersWidth : 0), y, Bounds.Width - 3 + (RowHeadersVisible ? RowHeadersWidth : 0), 1);
			XplatUI.DrawReversibleRectangle (Handle, splitter, 2);
		}

		#region Stuff for ToolTips
		private void MouseEnteredErrorIcon (DataGridViewCell item)
		{
			tooltip_currently_showing = item;
			ToolTipTimer.Start ();
		}

		private void MouseLeftErrorIcon (DataGridViewCell item)
		{
			ToolTipTimer.Stop ();
			ToolTipWindow.Hide (this);
			tooltip_currently_showing = null;
		}

		private Timer ToolTipTimer {
			get {
				if (tooltip_timer == null) {
					tooltip_timer = new Timer ();
					tooltip_timer.Enabled = false;
					tooltip_timer.Interval = 500;
					tooltip_timer.Tick += new EventHandler (ToolTipTimer_Tick);
				}

				return tooltip_timer;
			}
		}

		private ToolTip ToolTipWindow {
			get {
				if (tooltip_window == null)
					tooltip_window = new ToolTip ();

				return tooltip_window;
			}
		}

		private void ToolTipTimer_Tick (object o, EventArgs args)
		{
			string tooltip = tooltip_currently_showing.ErrorText;

			if (!string.IsNullOrEmpty (tooltip))
				ToolTipWindow.Present (this, tooltip);

			ToolTipTimer.Stop ();
		}
		#endregion

		private class ColumnSorter : IComparer
		{
			int column;
			int direction = 1;
			bool numeric_sort;
			
			public ColumnSorter (DataGridViewColumn column, ListSortDirection direction, bool numeric)
			{
				this.column = column.Index;
				this.numeric_sort = numeric;
				
				if (direction == ListSortDirection.Descending)
					this.direction = -1;
			}

			#region IComparer Members
			public int Compare (object x, object y)
			{
				DataGridViewRow row1 = (DataGridViewRow)x;
				DataGridViewRow row2 = (DataGridViewRow)y;

				if (row1.Cells[column].ValueType == typeof (DateTime) && row2.Cells[column].ValueType == typeof (DateTime))
					return DateTime.Compare ((DateTime)row1.Cells[column].Value, (DateTime)row2.Cells[column].Value) * direction;

				object val1 = row1.Cells[column].FormattedValue;
				object val2 = row2.Cells[column].FormattedValue;
				object val1NullValue = row1.Cells[column].InheritedStyle.NullValue;
				object val2NullValue = row2.Cells[column].InheritedStyle.NullValue;

				if (val1 == val1NullValue && val2 == val2NullValue)
					return 0;
				if (val1 == val1NullValue)
					return direction;
				if (val2 == val2NullValue)
					return -1 * direction;

				if (numeric_sort)
					return (int)(double.Parse (val1.ToString ()) - double.Parse (val2.ToString ())) * direction;
				else
					return string.Compare (val1.ToString (), val2.ToString ()) * direction;
			}
			#endregion
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
				return string.Format ("Type:{0}, Column:{1}, Row:{2}", type, columnIndex, rowIndex);
			}

		}

		[ComVisible (false)]
		public class DataGridViewControlCollection : Control.ControlCollection
		{
			private DataGridView owner;
			
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

			public override string Name {
				get { return base.Name; }
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
		
		[ComVisible (true)]
		protected class DataGridViewTopRowAccessibleObject : AccessibleObject
		{
			#region Constructors
			public DataGridViewTopRowAccessibleObject ()
			{
			}
			
			public DataGridViewTopRowAccessibleObject (DataGridView owner)
			{
				this.owner = owner;
			}
			#endregion
			
			#region Public Methods
			public override AccessibleObject GetChild (int index)
			{
				return base.GetChild (index);
			}

			public override int GetChildCount ()
			{
				return base.GetChildCount ();
			}

			public override AccessibleObject Navigate (AccessibleNavigation navigationDirection)
			{
				return base.Navigate (navigationDirection);
			}
			#endregion
			
			#region Public Properties
			public override Rectangle Bounds {
				get { return base.Bounds; }
			}
			
			public override string Name {
				get { return base.Name; }
			}

			public DataGridView Owner {
				get { return (DataGridView)owner; }
				set { 
					if (owner != null)
						throw new InvalidOperationException ("owner has already been set");
				
					owner = value;
				}
			}
			
			public override AccessibleObject Parent {
				get { return base.Parent; }
			}

			public override AccessibleRole Role {
				get { return base.Role; }
			}

			public override string Value {
				get { return base.Value; }
			}
			#endregion
		}
	}
}
