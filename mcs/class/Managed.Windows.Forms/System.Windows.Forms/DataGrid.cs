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
// Copyright (c) 2005,2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Jordi Mas i Hernandez	<jordi@ximian.com>
//	Chris Toshok		<toshok@ximian.com>
//
//

using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;
using System.Text;

namespace System.Windows.Forms
{
	internal class DataGridRelationshipRow {
		DataGrid owner;

		public DataGridRelationshipRow (DataGrid owner)
		{
			this.owner = owner;
			IsSelected = false;
			IsExpanded = false;
			height = 0;
			VerticalOffset = 0;
			RelationHeight = 0;
			relation_area = Rectangle.Empty;
		}

		public int height;

		/* this needs to be a property so that the Autosize
		 * example from the Windows.Forms FAQ will work */
		public int Height {
			get { return height; }
			set {
				if (height != value) {
					height = value;
					owner.UpdateRowsFrom (this);
				}
			}
		}

		public bool IsSelected;
		public bool IsExpanded;
		public int VerticalOffset;
		public int RelationHeight;
		public Rectangle relation_area; /* the Y coordinate of this rectangle is updated as needed */
	}

	internal class DataGridDataSource
	{
		public DataGrid owner;
		public CurrencyManager list_manager;
		public object view;
		public string data_member;
		public object data_source;
		public DataGridCell current;

		public DataGridDataSource (DataGrid owner, CurrencyManager list_manager, object data_source, string data_member, object view_data, DataGridCell current)
		{
			this.owner = owner;
			this.list_manager = list_manager;
			this.view = view_data;
			this.data_source = data_source;
			this.data_member = data_member;
			this.current = current;
		}

		DataGridRelationshipRow[] rows;
		public DataGridRelationshipRow[] Rows {
			get { return rows; }
			set { rows = value; }
		}

		Hashtable selected_rows;
		public Hashtable SelectedRows {
			get { return selected_rows; }
			set { selected_rows = value; }
		}

		int selection_start;
		public int SelectionStart {
			get { return selection_start; }
			set { selection_start = value; }
		}
	}

	[DefaultEvent("Navigate")]
	[DefaultProperty("DataSource")]
	[Designer("System.Windows.Forms.Design.DataGridDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ComplexBindingProperties ("DataSource", "DataMember")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class DataGrid : Control, ISupportInitialize, IDataGridEditingService
	{
		[Flags]
		public enum HitTestType
		{
			None		= 0,
			Cell		= 1,
			ColumnHeader	= 2,
			RowHeader	= 4,
			ColumnResize	= 8,
			RowResize	= 16,
			Caption		= 32,
			ParentRows	= 64
		}

		public sealed class HitTestInfo
		{
			public static readonly HitTestInfo Nowhere = null;

			int row;
			int column;
			DataGrid.HitTestType type;

			#region Private Constructors
			internal HitTestInfo () : this (-1, -1, HitTestType.None)
			{
			}

			internal HitTestInfo (int row, int column, DataGrid.HitTestType type)
			{
				this.row = row;
				this.column = column;
				this.type = type;
			}
			#endregion


			#region Public Instance Properties
			public int Column {
				get { return column; }
			}

			public int Row {
				get { return row; }
			}
			public DataGrid.HitTestType Type {
				get { return type; }
			}
			#endregion //Public Instance Properties

			public override bool Equals (object value)
			{
				if (!(value is HitTestInfo))
					return false;

				HitTestInfo obj = (HitTestInfo) value;
				return (obj.Column == column && obj.Row == row && obj.Type ==type);
			}

			public override int GetHashCode ()
			{
				return row ^ column;
			}

			public override string ToString ()
			{
				return "{ " + type + "," + row + "," + column + "}";
			}
		}

		#region	Local Variables
		/* cached theme defaults */
		static readonly Color	def_background_color = ThemeEngine.Current.DataGridBackgroundColor;
		static readonly Color	def_caption_backcolor = ThemeEngine.Current.DataGridCaptionBackColor;
		static readonly Color	def_caption_forecolor = ThemeEngine.Current.DataGridCaptionForeColor;
		static readonly Color	def_parent_rows_backcolor = ThemeEngine.Current.DataGridParentRowsBackColor;
		static readonly Color	def_parent_rows_forecolor = ThemeEngine.Current.DataGridParentRowsForeColor;

		/* colors */
		// XXX this needs addressing. Control.background_color should not be internal.
		new Color background_color;
		Color caption_backcolor;
		Color caption_forecolor;
		Color parent_rows_backcolor;
		Color parent_rows_forecolor;

		/* flags to determine which areas of the datagrid are shown */
		bool caption_visible;
		bool parent_rows_visible;

		GridTableStylesCollection styles_collection;
		DataGridParentRowsLabelStyle parent_rows_label_style;
		DataGridTableStyle default_style;
		DataGridTableStyle grid_style;
		DataGridTableStyle current_style;

		/* selection */
		DataGridCell current_cell;
		Hashtable selected_rows;
		int selection_start; // used for range selection

		/* layout/rendering */
		bool allow_navigation;
		int first_visible_row;
		int first_visible_column;
		int visible_row_count;
		int visible_column_count;
		Font caption_font;
		string caption_text;
		bool flatmode;
		HScrollBar horiz_scrollbar;
		VScrollBar vert_scrollbar;
		int horiz_pixeloffset;

		internal Bitmap back_button_image;
		internal Rectangle back_button_rect;
		internal bool back_button_mouseover;
		internal bool back_button_active;
		internal Bitmap parent_rows_button_image;
		internal Rectangle parent_rows_button_rect;
		internal bool parent_rows_button_mouseover;
		internal bool parent_rows_button_active;

		/* databinding */
		object datasource;
		string datamember;
		CurrencyManager list_manager;
		bool refetch_list_manager = true;
		bool _readonly;
		DataGridRelationshipRow[] rows;

		/* column resize fields */
		bool column_resize_active;
		int resize_column_x;
		int resize_column_width_delta;
		int resize_column;
		
		/* row resize fields */
		bool row_resize_active;
		int resize_row_y;
		int resize_row_height_delta;
		int resize_row;

		/* used to make sure we don't endlessly recurse calling set_CurrentCell and OnListManagerPositionChanged */
		bool from_positionchanged_handler;

		/* editing state */
		bool cursor_in_add_row;
		bool add_row_changed;
		internal bool is_editing;		// Current cell is edit mode
		bool is_changing;
		bool commit_row_changes = true;		// Whether to commit current edit or cancel it
		bool adding_new_row;			// Used to temporary ignore the new row added by CurrencyManager.AddNew in CurrentCell

		internal Stack data_source_stack;

		#endregion // Local Variables

		#region Public Constructors
		public DataGrid ()
		{
			allow_navigation = true;
			background_color = def_background_color;
			border_style = BorderStyle.Fixed3D;
			caption_backcolor = def_caption_backcolor;
			caption_forecolor = def_caption_forecolor;
			caption_text = string.Empty;
			caption_visible = true;
			datamember = string.Empty;
			parent_rows_backcolor = def_parent_rows_backcolor;
			parent_rows_forecolor = def_parent_rows_forecolor;
			parent_rows_visible = true;
			current_cell = new DataGridCell ();
			parent_rows_label_style = DataGridParentRowsLabelStyle.Both;
			selected_rows = new Hashtable ();
			selection_start = -1;
			rows = new DataGridRelationshipRow [0];

			default_style = new DataGridTableStyle (true);
			grid_style = new DataGridTableStyle ();

			styles_collection = new GridTableStylesCollection (this);
			styles_collection.CollectionChanged += new CollectionChangeEventHandler (OnTableStylesCollectionChanged);

			CurrentTableStyle = grid_style;

			horiz_scrollbar = new ImplicitHScrollBar ();
			horiz_scrollbar.Scroll += new ScrollEventHandler (GridHScrolled);
			vert_scrollbar = new ImplicitVScrollBar ();
			vert_scrollbar.Scroll += new ScrollEventHandler (GridVScrolled);

			SetStyle (ControlStyles.UserMouse, true);

			data_source_stack = new Stack ();

			back_button_image = ResourceImageLoader.Get ("go-previous.png");
			back_button_image.MakeTransparent (Color.Transparent);
			parent_rows_button_image = ResourceImageLoader.Get ("go-top.png");
			parent_rows_button_image.MakeTransparent (Color.Transparent);
		}

		#endregion	// Public Constructor

		#region Public Instance Properties

		[DefaultValue(true)]
		public bool AllowNavigation {
			get { return allow_navigation; }
			set {
				if (allow_navigation != value) {
					allow_navigation = value;
					OnAllowNavigationChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		public bool AllowSorting {
			get { return grid_style.AllowSorting; }
			set { grid_style.AllowSorting = value; }
		}

		public Color AlternatingBackColor {
			get { return grid_style.AlternatingBackColor; }
			set { grid_style.AlternatingBackColor = value; }
		}

		public override Color BackColor {
			get { return grid_style.BackColor; }
			set { grid_style.BackColor = value; }
		}

		public Color BackgroundColor {
			get { return background_color; }
			set {
				 if (background_color != value) {
					background_color = value;
					OnBackgroundColorChanged (EventArgs.Empty);
					Invalidate ();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set {
				if (base.BackgroundImage == value)
					return;

				base.BackgroundImage = value;
				Invalidate ();
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		[DispId(-504)]
		[DefaultValue(BorderStyle.Fixed3D)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { 
				InternalBorderStyle = value; 
				CalcAreasAndInvalidate ();
				OnBorderStyleChanged (EventArgs.Empty);
			}
		}

		public Color CaptionBackColor {
			get { return caption_backcolor; }
			set {
				if (caption_backcolor != value) {
					caption_backcolor = value;
					InvalidateCaption ();
				}
			}
		}

		[Localizable(true)]
		[AmbientValue(null)]
		public Font CaptionFont {
			get {
				if (caption_font == null)
					return new Font (Font, FontStyle.Bold);

				return caption_font;
			}
			set {
				if (caption_font != null && caption_font.Equals (value))
					return;

				caption_font = value;
				CalcAreasAndInvalidate ();
			}
		}

		public Color CaptionForeColor {
			get { return caption_forecolor; }
			set {
				if (caption_forecolor != value) {
					caption_forecolor = value;
					InvalidateCaption ();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue("")]
		public string CaptionText {
			get { return caption_text; }
			set {
				if (caption_text != value) {
					caption_text = value;
					InvalidateCaption ();
				}
			}
		}

		[DefaultValue(true)]
		public bool CaptionVisible {
			get { return caption_visible; }
			set {
				if (caption_visible != value) {
					EndEdit ();
					caption_visible = value;
					CalcAreasAndInvalidate ();
					OnCaptionVisibleChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		public bool ColumnHeadersVisible {
			get { return grid_style.ColumnHeadersVisible; }
			set { 
				if (grid_style.ColumnHeadersVisible != value) {
					grid_style.ColumnHeadersVisible = value; 

					// UIA Framework: To keep track of header
					OnUIAColumnHeadersVisibleChanged ();
				}
			}
		}

		bool setting_current_cell;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DataGridCell CurrentCell {
			get { return current_cell; }
			set {
				if (setting_current_cell)
					return;
				setting_current_cell = true;

				if (!IsHandleCreated) {
					setting_current_cell = false;
					throw new Exception ("CurrentCell cannot be set at this time.");
				}

				/* Even if we are on the same cell, we could need to actually start edition */
				if (current_cell.Equals (value) && is_editing) {
					setting_current_cell = false;
					return;
				}

				/* make sure the new cell fits in the correct bounds for [row,column] */
				if (ReadOnly && value.RowNumber > RowsCount - 1)
					value.RowNumber = RowsCount - 1;
				else if (value.RowNumber > RowsCount)
					value.RowNumber = RowsCount;
				if (value.ColumnNumber >= CurrentTableStyle.GridColumnStyles.Count)
					value.ColumnNumber = CurrentTableStyle.GridColumnStyles.Count == 0 ? 0 : CurrentTableStyle.GridColumnStyles.Count - 1;


				/* now make sure we don't go negative */
				if (value.RowNumber < 0) value.RowNumber = 0;
				if (value.ColumnNumber < 0) value.ColumnNumber = 0;

				bool was_changing = is_changing;

				add_row_changed = add_row_changed || was_changing;

				EndEdit ();
				if (value.RowNumber != current_cell.RowNumber) {
					if (!from_positionchanged_handler) {
						try {
							if (commit_row_changes)
								ListManager.EndCurrentEdit ();
							else
								ListManager.CancelCurrentEdit ();
						}
						catch (Exception e) {
							DialogResult r = MessageBox.Show (String.Format ("{0} Do you wish to correct the value?", e.Message),
											  "Error when committing the row to the original data source",
											  MessageBoxButtons.YesNo);
							if (r == DialogResult.Yes) {
								InvalidateRowHeader (value.RowNumber);
								InvalidateRowHeader (current_cell.RowNumber);
								setting_current_cell = false;
								Edit ();
								return;
							}
							else
								ListManager.CancelCurrentEdit ();
						}
					}

					if (value.RowNumber == RowsCount && !ListManager.AllowNew)
						value.RowNumber --;
				}

				int old_row = current_cell.RowNumber;

				current_cell = value;

				EnsureCellVisibility (value);

				// by default, edition in existing rows is commited, and for new ones is discarded, unless
				// we receive actual input data from the user
				if (CurrentRow == RowsCount && ListManager.AllowNew) {
					commit_row_changes = false;
					cursor_in_add_row = true;
					add_row_changed = false;

					adding_new_row = true;
					AddNewRow ();
					adding_new_row = false;
				}
				else {
					cursor_in_add_row = false;
					commit_row_changes = true;
				}

				InvalidateRowHeader (old_row);
				InvalidateRowHeader (current_cell.RowNumber);

				list_manager.Position = current_cell.RowNumber;

				OnCurrentCellChanged (EventArgs.Empty);

				if (!from_positionchanged_handler)
					Edit ();

				setting_current_cell = false;
			}
		}

		internal void EditRowChanged (DataGridColumnStyle column_style)
		{
			if (cursor_in_add_row) {
				if (!commit_row_changes) { // first change in add row, time to show another row in the ui
					commit_row_changes = true;
					RecreateDataGridRows (true);
				}
			}
		}

		int CurrentRow {
			get { return current_cell.RowNumber; }
			set { CurrentCell = new DataGridCell (value, current_cell.ColumnNumber); }
		}

		int CurrentColumn {
			get { return current_cell.ColumnNumber; }
			set { CurrentCell = new DataGridCell (current_cell.RowNumber, value); }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int CurrentRowIndex {
			get {
				if (ListManager == null)
					return -1;
				
				return CurrentRow;
			}
			set { CurrentRow = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Cursor Cursor {
			get { return base.Cursor; }
			set { base.Cursor = value; }
		}

		[DefaultValue(null)]
		[Editor ("System.Windows.Forms.Design.DataMemberListEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string DataMember {
			get { return datamember; }
			set {
				if (BindingContext != null) {
					SetDataSource (datasource, value);
				}
				else {
					if (list_manager != null)
						list_manager = null;
					datamember = value;
					refetch_list_manager = true;
				}
			}
		}

		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[AttributeProvider (typeof (IListSource))]
		public object DataSource {
			get { return datasource; }
			set {
				if (BindingContext != null) {
					SetDataSource (value, ListManager == null ? datamember : string.Empty);
				}
				else {
					datasource = value;
					if (list_manager != null)
						datamember = string.Empty;

					if (list_manager != null)
						list_manager = null;
					refetch_list_manager = true;
				}
			}
		}

		protected override Size DefaultSize {
			get { return new Size (130, 80); }
		}

		[Browsable(false)]
		public int FirstVisibleColumn {
			get { return first_visible_column; }
		}

		[DefaultValue(false)]
		public bool FlatMode {
			get { return flatmode; }
			set {
				if (flatmode != value) {
					flatmode = value;
					OnFlatModeChanged (EventArgs.Empty);
					Refresh ();
				}
			}
		}

		public override Color ForeColor {
			get { return grid_style.ForeColor; }
			set { grid_style.ForeColor = value; }
		}

		public Color GridLineColor {
			get { return grid_style.GridLineColor; }
			set {
				if (value == Color.Empty)
					throw new ArgumentException ("Color.Empty value is invalid.");

				grid_style.GridLineColor = value;
			}
		}

		[DefaultValue(DataGridLineStyle.Solid)]
		public DataGridLineStyle GridLineStyle {
			get { return grid_style.GridLineStyle; }
			set { grid_style.GridLineStyle = value; }
		}

		public Color HeaderBackColor {
			get { return grid_style.HeaderBackColor; }
			set {
				if (value == Color.Empty)
					throw new ArgumentException ("Color.Empty value is invalid.");

				grid_style.HeaderBackColor = value;
			}
		}

		public Font HeaderFont {
			get { return grid_style.HeaderFont; }
			set { grid_style.HeaderFont = value; }
		}

		public Color HeaderForeColor {
			get { return grid_style.HeaderForeColor; }
			set { grid_style.HeaderForeColor = value; }
		}

		protected ScrollBar HorizScrollBar {
			get { return horiz_scrollbar; }
		}
		internal ScrollBar HScrollBar {
			get { return horiz_scrollbar; }
		}

		internal int HorizPixelOffset {
			get { return horiz_pixeloffset; }
		}

		internal bool IsChanging {
			get { return is_changing; }
		}

		public object this [DataGridCell cell] {
			get { return this [cell.RowNumber, cell.ColumnNumber]; }
			set { this [cell.RowNumber, cell.ColumnNumber] = value; }
		}

		public object this [int rowIndex, int columnIndex] {
			get { return CurrentTableStyle.GridColumnStyles[columnIndex].GetColumnValueAtRow (ListManager,
													  rowIndex); }
			set { 
				CurrentTableStyle.GridColumnStyles[columnIndex].SetColumnValueAtRow (ListManager,
												     rowIndex, value); 

				// UIA Framework: Raising changes in datasource.
				OnUIAGridCellChanged (new CollectionChangeEventArgs (CollectionChangeAction.Refresh,
				                                                     new DataGridCell (rowIndex,
 				                                                                       columnIndex)));
			}
		}

		public Color LinkColor {
			get { return grid_style.LinkColor; }
			set { grid_style.LinkColor = value; }
		}

		internal Font LinkFont {
			get { return new Font (Font, FontStyle.Underline); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Color LinkHoverColor {
			get { return grid_style.LinkHoverColor; }
			set { grid_style.LinkHoverColor = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected internal CurrencyManager ListManager {
			get {
				if (list_manager == null && refetch_list_manager) {
					SetDataSource (datasource, datamember);
					refetch_list_manager = false;
				}

				return list_manager;
			}
			set { throw new NotSupportedException ("Operation is not supported."); }
		}

		public Color ParentRowsBackColor {
			get { return parent_rows_backcolor; }
			set {
				if (parent_rows_backcolor != value) {
					parent_rows_backcolor = value;
					if (parent_rows_visible) {
						Refresh ();
					}
				}
			}
		}

		public Color ParentRowsForeColor {
			get { return parent_rows_forecolor; }
			set {
				if (parent_rows_forecolor != value) {
					parent_rows_forecolor = value;
					if (parent_rows_visible) {
						Refresh ();
					}
				}
			}
		}

		[DefaultValue(DataGridParentRowsLabelStyle.Both)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DataGridParentRowsLabelStyle ParentRowsLabelStyle {
			get { return parent_rows_label_style; }
			set {
				if (parent_rows_label_style != value) {
					parent_rows_label_style = value;
					if (parent_rows_visible) {
						Refresh ();
					}

					OnParentRowsLabelStyleChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		public bool ParentRowsVisible {
			get { return parent_rows_visible; }
			set {
				if (parent_rows_visible != value) {
					parent_rows_visible = value;
					CalcAreasAndInvalidate ();
					OnParentRowsVisibleChanged (EventArgs.Empty);
				}
			}
		}

		// Settting this property seems to have no effect.
		[DefaultValue(75)]
		[TypeConverter(typeof(DataGridPreferredColumnWidthTypeConverter))]
		public int PreferredColumnWidth {
			get { return grid_style.PreferredColumnWidth; }
			set { grid_style.PreferredColumnWidth = value; }
		}

		public int PreferredRowHeight {
			get { return grid_style.PreferredRowHeight; }
			set { grid_style.PreferredRowHeight = value; }
		}

		[DefaultValue(false)]
		public bool ReadOnly {
			get { return _readonly; }
			set {
				if (_readonly != value) {
					_readonly = value;
					OnReadOnlyChanged (EventArgs.Empty);
					CalcAreasAndInvalidate ();
				}
			}
		}

		[DefaultValue(true)]
		public bool RowHeadersVisible {
			get { return grid_style.RowHeadersVisible; }
			set { grid_style.RowHeadersVisible = value; }
		}

		[DefaultValue(35)]
		public int RowHeaderWidth {
			get { return grid_style.RowHeaderWidth; }
			set { grid_style.RowHeaderWidth = value; }
		}

		internal DataGridRelationshipRow[] DataGridRows {
			get { return rows; }
		}


		public Color SelectionBackColor {
			get { return grid_style.SelectionBackColor; }
			set { grid_style.SelectionBackColor = value; }
		}

		public Color SelectionForeColor {
			get { return grid_style.SelectionForeColor; }
			set { grid_style.SelectionForeColor = value; }
		}

		public override ISite Site {
			get { return base.Site; }
			set { base.Site = value; }
		}

		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridTableStylesCollection TableStyles {
			get { return styles_collection; }
		}

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected ScrollBar VertScrollBar {
			get { return vert_scrollbar; }
		}
		internal ScrollBar VScrollBar {
			get { return vert_scrollbar; }
		}

		[Browsable(false)]
		public int VisibleColumnCount {
			get { return visible_column_count; }
		}

		[Browsable(false)]
		public int VisibleRowCount {
			get { return visible_row_count; }
		}

		#endregion	// Public Instance Properties

		#region Private Instance Properties
		internal DataGridTableStyle CurrentTableStyle {
			get { return current_style; }
			set {
				if (current_style != value) {
					if (current_style != null)
						DisconnectTableStyleEvents ();

					current_style = value;

					if (current_style != null) {
						current_style.DataGrid = this;
						ConnectTableStyleEvents ();
					}
					CalcAreasAndInvalidate ();
				}
			}
		}

		internal int FirstVisibleRow {
			get { return first_visible_row; }
		}

		// As opposed to VisibleRowCount, this value is the maximum
		// *possible* number of visible rows given our area.
		internal int MaxVisibleRowCount {
			get {
				return cells_area.Height / RowHeight;
			}
		}
		
		internal int RowsCount {
			get { return ListManager != null ? ListManager.Count : 0; }
		}

		internal int RowHeight {
			get {
				if (CurrentTableStyle.CurrentPreferredRowHeight > Font.Height + 3 + 1 /* line */)
					return CurrentTableStyle.CurrentPreferredRowHeight;
				else
					return Font.Height + 3 + 1 /* line */;
			}
		}
		
		internal override bool ScaleChildrenInternal {
			get { return false; }
		}

		internal bool ShowEditRow {
			get {
				if (ListManager != null && !ListManager.AllowNew)
					return false;

				return !_readonly;
			}
		}
		
		internal bool ShowParentRows {
			get { return ParentRowsVisible && data_source_stack.Count > 0; }
		}
		
		#endregion Private Instance Properties

		#region Public Instance Methods

		void AbortEditing ()
		{
			if (is_changing) {
				CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].Abort (current_cell.RowNumber);
				is_changing = false;
				InvalidateRowHeader (current_cell.RowNumber);
			}
		}

		public bool BeginEdit (DataGridColumnStyle gridColumn, int rowNumber)
		{
			if (is_changing)
				return false;

			int column = CurrentTableStyle.GridColumnStyles.IndexOf (gridColumn);
			if (column < 0)
				return false;

			CurrentCell = new DataGridCell (rowNumber, column);

			/* force editing of CurrentCell if we aren't already editing */
			Edit ();

			return true;
		}

		public void BeginInit ()
		{
		}

		protected virtual void CancelEditing ()
		{
			if (CurrentTableStyle.GridColumnStyles.Count == 0)
				return;

			CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].ConcedeFocus ();

			if (is_changing) {
				if (current_cell.ColumnNumber < CurrentTableStyle.GridColumnStyles.Count)
					CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].Abort (current_cell.RowNumber);
				InvalidateRowHeader (current_cell.RowNumber);
			}

			if (cursor_in_add_row && !is_changing) {
				ListManager.CancelCurrentEdit ();
			}

			is_changing = false;
			is_editing = false;
		}

		public void Collapse (int row)
		{
			if (!rows[row].IsExpanded)
				return;

			SuspendLayout ();
			rows[row].IsExpanded = false;
			for (int i = 1; i < rows.Length - row; i ++)
				rows[row + i].VerticalOffset -= rows[row].RelationHeight;

			rows[row].height -= rows[row].RelationHeight;
			rows[row].RelationHeight = 0;
			ResumeLayout (false);

			/* XX need to redraw from @row down */
			CalcAreasAndInvalidate ();
		}

		protected internal virtual void ColumnStartedEditing (Control editingControl)
		{
			ColumnStartedEditing (editingControl.Bounds);
		}

		protected internal virtual void ColumnStartedEditing (Rectangle bounds)
		{
			bool need_invalidate = is_changing == false;
			// XXX calculate the row header to invalidate
			// instead of using CurrentRow
			is_changing = true;

			if (cursor_in_add_row && need_invalidate)
				RecreateDataGridRows (true);

			if (need_invalidate)
				InvalidateRowHeader (CurrentRow);
		}

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return base.CreateAccessibilityInstance ();
		}

		protected virtual DataGridColumnStyle CreateGridColumn (PropertyDescriptor prop)
		{
			return CreateGridColumn (prop, false);
		}

		[MonoTODO ("Not implemented, will throw NotImplementedException")]
		protected virtual DataGridColumnStyle CreateGridColumn (PropertyDescriptor prop, bool isDefault)
		{
			throw new NotImplementedException();
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		public bool EndEdit (DataGridColumnStyle gridColumn, int rowNumber, bool shouldAbort)
		{
			if (shouldAbort || (_readonly || gridColumn.TableStyleReadOnly || gridColumn.ReadOnly))
				gridColumn.Abort (rowNumber);
			else {
				gridColumn.Commit (ListManager, rowNumber);
				gridColumn.ConcedeFocus ();
			}

			if (is_editing || is_changing) {
				is_editing = false;
				is_changing = false;
				InvalidateRowHeader (rowNumber);
			}
			return true;
		}

		public void EndInit ()
		{
			if (grid_style != null)
				grid_style.DataGrid = this;
		}

		public void Expand (int row)
		{
			if (rows[row].IsExpanded)
				return;

			rows[row].IsExpanded = true;

			int i;

			string[] relations = CurrentTableStyle.Relations;
			StringBuilder relation_builder = new StringBuilder ("");

			for (i = 0; i < relations.Length; i ++) {
				if (i > 0)
					relation_builder.Append ("\n");

				relation_builder.Append (relations[i]);
			}
			string relation_text = relation_builder.ToString ();

			SizeF measured_area = TextRenderer.MeasureString (relation_text, LinkFont);

			rows[row].relation_area = new Rectangle (cells_area.X + 1,
								 0, /* updated as needed at the usage sites for relation_area */
								 (int)measured_area.Width + 4,
								 Font.Height * relations.Length);

			for (i = 1; i < rows.Length - row; i ++)
				rows[row + i].VerticalOffset += rows[row].relation_area.Height;
			rows[row].height += rows[row].relation_area.Height;
			rows[row].RelationHeight = rows[row].relation_area.Height;

			/* XX need to redraw from @row down */
			CalcAreasAndInvalidate ();
		}

		public Rectangle GetCellBounds (DataGridCell dgc)
		{
			return GetCellBounds (dgc.RowNumber, dgc.ColumnNumber);
		}

		public Rectangle GetCellBounds (int row, int col)
		{
			Rectangle bounds = new Rectangle ();
			int col_pixel;

			bounds.Width = CurrentTableStyle.GridColumnStyles[col].Width;
			bounds.Height = rows[row].Height - rows[row].RelationHeight;
			bounds.Y = cells_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
			col_pixel = GetColumnStartingPixel (col);
			bounds.X = cells_area.X + col_pixel - horiz_pixeloffset;
			return bounds;
		}

		public Rectangle GetCurrentCellBounds ()
		{
			return GetCellBounds (current_cell.RowNumber, current_cell.ColumnNumber);
		}

		protected virtual string GetOutputTextDelimiter ()
		{
			return string.Empty;
		}

		protected virtual void GridHScrolled (object sender, ScrollEventArgs se)
		{
			if (se.NewValue == horiz_pixeloffset ||
			    se.Type == ScrollEventType.EndScroll) {
				return;
			}

			ScrollToColumnInPixels (se.NewValue);
		}

		protected virtual void GridVScrolled (object sender, ScrollEventArgs se)
		{
			int old_first_visible_row = first_visible_row;
			first_visible_row = se.NewValue;

			if (first_visible_row == old_first_visible_row)
				return;

			UpdateVisibleRowCount ();

			if (first_visible_row == old_first_visible_row)
				return;
			
			ScrollToRow (old_first_visible_row, first_visible_row);
		}

		public HitTestInfo HitTest (Point position)
		{
			return HitTest (position.X, position.Y);
		}

		const int RESIZE_HANDLE_HORIZ_SIZE = 5;
		const int RESIZE_HANDLE_VERT_SIZE = 3;

		// From Point to Cell
		public HitTestInfo HitTest (int x, int y)
		{
			if (column_headers_area.Contains (x, y)) {
				int offset_x = x + horiz_pixeloffset;
				int column_x;
				int column_under_mouse = FromPixelToColumn (offset_x, out column_x);

				if (column_under_mouse == -1)
					return new HitTestInfo (-1, -1, HitTestType.None);

				if ((column_x + CurrentTableStyle.GridColumnStyles[column_under_mouse].Width - offset_x < RESIZE_HANDLE_HORIZ_SIZE)
				    && column_under_mouse < CurrentTableStyle.GridColumnStyles.Count) {

					return new HitTestInfo (-1, column_under_mouse, HitTestType.ColumnResize);
				}
				else {
					return new HitTestInfo (-1, column_under_mouse, HitTestType.ColumnHeader);
				}
			}

			if (row_headers_area.Contains (x, y)) {
				int posy;
				int rcnt = FirstVisibleRow + VisibleRowCount;
				for (int r = FirstVisibleRow; r < rcnt; r++) {
					posy = cells_area.Y + rows[r].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
					if (y <= posy + rows[r].Height) {
						if ((posy + rows[r].Height) - y < RESIZE_HANDLE_VERT_SIZE) {
							return new HitTestInfo (r, -1, HitTestType.RowResize);
						}
						else {
							return new HitTestInfo (r, -1, HitTestType.RowHeader);
						}
					}
				}
			}

			if (caption_area.Contains (x, y)) {
				return new HitTestInfo (-1, -1, HitTestType.Caption);
			}

			if (parent_rows.Contains (x, y)) {
				return new HitTestInfo (-1, -1, HitTestType.ParentRows);
			}

			int pos_y, pos_x, width;
			int rowcnt = FirstVisibleRow + VisibleRowCount;
			for (int row = FirstVisibleRow; row < rowcnt; row++) {

				pos_y = cells_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
				if (y <= pos_y + rows[row].Height) {
					int col_pixel;
					int column_cnt = first_visible_column + visible_column_count;
					if (column_cnt > 0) {
						for (int column = first_visible_column; column < column_cnt; column++) {
							if (CurrentTableStyle.GridColumnStyles[column].bound == false)
								continue;
							col_pixel = GetColumnStartingPixel (column);
							pos_x = cells_area.X + col_pixel - horiz_pixeloffset;
							width = CurrentTableStyle.GridColumnStyles[column].Width;

							if (x <= pos_x + width) { // Column found
								return new HitTestInfo (row, column, HitTestType.Cell);
							}
						}
					}
					else if (CurrentTableStyle.HasRelations) {
						/* XXX this needs checking against MS somehow... */
						if (x < rows[row].relation_area.X + rows[row].relation_area.Width)
							return new HitTestInfo (row, 0/*XXX?*/, HitTestType.Cell);
					}

					break;
				}
			}

			return new HitTestInfo ();
		}

		public bool IsExpanded (int rowNumber)
		{
			return (rows[rowNumber].IsExpanded);
		}

		public bool IsSelected (int row)
		{
			return rows[row].IsSelected;
		}

		public void NavigateBack ()
		{
			if (data_source_stack.Count == 0)
				return;

			DataGridDataSource source = (DataGridDataSource)data_source_stack.Pop ();
			list_manager = source.list_manager;
			rows = source.Rows;
			selected_rows = source.SelectedRows;
			selection_start = source.SelectionStart;
			SetDataSource (source.data_source, source.data_member);

			CurrentCell = source.current;
		}

		public void NavigateTo (int rowNumber, string relationName)
		{
			if (allow_navigation == false)
				return;

			DataGridDataSource previous_source = new DataGridDataSource (this, list_manager, datasource, datamember, list_manager.Current, CurrentCell);
			previous_source.Rows = rows;
			previous_source.SelectedRows = selected_rows;
			previous_source.SelectionStart = selection_start;

			data_source_stack.Push (previous_source);

			rows = null;
			selected_rows = new Hashtable ();
			selection_start = -1;

			DataMember = String.Format ("{0}.{1}", DataMember, relationName);
			OnDataSourceChanged (EventArgs.Empty);
		}

		protected virtual void OnAllowNavigationChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AllowNavigationChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected void OnBackButtonClicked (object sender, EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [BackButtonClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected virtual void OnBackgroundColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [BackgroundColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnBindingContextChanged (EventArgs e)
		{
			base.OnBindingContextChanged (e);

			SetDataSource (datasource, datamember);
		}

		protected virtual void OnBorderStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [BorderStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCaptionVisibleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [CaptionVisibleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCurrentCellChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [CurrentCellChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDataSourceChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DataSourceChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnEnter (EventArgs e)
		{
			base.OnEnter (e);
			Edit ();
		}

		protected virtual void OnFlatModeChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [FlatModeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			CalcGridAreas ();
			base.OnFontChanged (e);
		}

		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
			SetDataSource (datasource, datamember);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		// It seems we have repeated code with ProcessKeyPreview, specifically
		// the call to ProcessGridKey. In practice it seems this event is *never* fired
		// since the key events are handled by the current column's textbox. 
		// We are keeping commented anyway, in case we need to actually call it.
		protected override void OnKeyDown (KeyEventArgs ke)
		{
			base.OnKeyDown (ke);
			
			/*if (ProcessGridKey (ke) == true)
				ke.Handled = true;

			// TODO: we probably don't need this check,
			// since current_cell wouldn't have been set
			// to something invalid
			if (CurrentTableStyle.GridColumnStyles.Count > 0) {
				CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].OnKeyDown
					(ke, current_cell.RowNumber, current_cell.ColumnNumber);
			}*/
		}

		protected override void OnKeyPress (KeyPressEventArgs kpe)
		{
			base.OnKeyPress (kpe);
		}

		protected override void OnLayout (LayoutEventArgs levent)
		{
			base.OnLayout (levent);
			CalcAreasAndInvalidate ();
		}

		protected override void OnLeave (EventArgs e)
		{
			base.OnLeave (e);

			EndEdit ();
			if (commit_row_changes)
				ListManager.EndCurrentEdit ();
			else
				ListManager.CancelCurrentEdit ();
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown (e);

			bool ctrl_pressed = ((Control.ModifierKeys & Keys.Control) != 0);
			bool shift_pressed = ((Control.ModifierKeys & Keys.Shift) != 0);

			HitTestInfo testinfo;
			testinfo = HitTest (e.X, e.Y);

			switch (testinfo.Type) {
			case HitTestType.Cell:
				if (testinfo.Row < 0 || testinfo.Column < 0)
					break;

				if (rows[testinfo.Row].IsExpanded) {
					Rectangle relation_area = rows[testinfo.Row].relation_area;
					relation_area.Y = rows[testinfo.Row].VerticalOffset + cells_area.Y + rows[testinfo.Row].Height - rows[testinfo.Row].RelationHeight;
					if (relation_area.Contains (e.X, e.Y)) {
						/* the click happened in the relation area, navigate to the new table */
						int relative = e.Y - relation_area.Y;
						NavigateTo (testinfo.Row, CurrentTableStyle.Relations[relative / LinkFont.Height]);
						return;
					}
				}

				DataGridCell new_cell = new DataGridCell (testinfo.Row, testinfo.Column);

				if ((new_cell.Equals (current_cell) == false) || (!is_editing)) {
					ResetSelection ();
					CurrentCell = new_cell;
					Edit ();
				} else {
					CurrentTableStyle.GridColumnStyles[testinfo.Column].OnMouseDown (e, testinfo.Row, testinfo.Column);
				}

				break;

			case HitTestType.RowHeader:
				bool expansion_click = false;
				if (CurrentTableStyle.HasRelations) {
					if (e.X > row_headers_area.X + row_headers_area.Width / 2) {
						/* it's in the +/- space */
						if (IsExpanded (testinfo.Row))
							Collapse (testinfo.Row);
						else
							Expand (testinfo.Row);

						expansion_click = true;
					}
				}

				CancelEditing ();
				CurrentRow = testinfo.Row;

				if (!ctrl_pressed && !shift_pressed && !expansion_click) {
					ResetSelection (); // Invalidates selected rows
				}

				if ((shift_pressed || expansion_click) && selection_start != -1) {
					ShiftSelection (testinfo.Row);
				} else { // ctrl_pressed or single item
					selection_start = testinfo.Row;
					Select (testinfo.Row);
				}

				OnRowHeaderClick (EventArgs.Empty);

				break;

			case HitTestType.ColumnHeader:
				if (CurrentTableStyle.GridColumnStyles.Count == 0)
					break;

				if (AllowSorting == false)
					break;

				if (ListManager.List is IBindingList == false)
					break;

				// Don't do any sort if we are empty, as .net does
				if (ListManager.Count == 0)
					return;
			
				ListSortDirection direction = ListSortDirection.Ascending;
				PropertyDescriptor prop = CurrentTableStyle.GridColumnStyles[testinfo.Column].PropertyDescriptor;
				IBindingList list = (IBindingList) ListManager.List;

				if (list.SortProperty != null) {
					CurrentTableStyle.GridColumnStyles[list.SortProperty].ArrowDrawingMode 
						= DataGridColumnStyle.ArrowDrawing.No;
				}

				if (prop == list.SortProperty && list.SortDirection == ListSortDirection.Ascending) {
					direction = ListSortDirection.Descending;
				}
				
				CurrentTableStyle.GridColumnStyles[testinfo.Column].ArrowDrawingMode =
					direction == ListSortDirection.Ascending ? 
					DataGridColumnStyle.ArrowDrawing.Ascending : DataGridColumnStyle.ArrowDrawing.Descending;
				
				list.ApplySort (prop, direction);
				Refresh ();
				if (this.is_editing)
					//CurrentTableStyle.GridColumnStyles[CurrentColumn].UpdateUI ();
					this.InvalidateColumn (CurrentTableStyle.GridColumnStyles[CurrentColumn]);

				break;

			case HitTestType.ColumnResize:
				if (e.Clicks == 2) {
					EndEdit ();
					ColumnResize (testinfo.Column);
				} else {
					resize_column = testinfo.Column;
					column_resize_active = true;
					resize_column_x = e.X;
					resize_column_width_delta = 0;
					EndEdit ();
					DrawResizeLineVert (resize_column_x);
				}
				break;

			case HitTestType.RowResize:
				if (e.Clicks == 2) {
					EndEdit ();
					RowResize (testinfo.Row);
				} else {
					resize_row = testinfo.Row;
					row_resize_active = true;
					resize_row_y = e.Y;
					resize_row_height_delta = 0;
					EndEdit ();
					DrawResizeLineHoriz (resize_row_y);
				}
				break;

			case HitTestType.Caption:
				if (back_button_rect.Contains (e.X, e.Y)) {
					back_button_active = true;
					Invalidate (back_button_rect);
				}
				if (parent_rows_button_rect.Contains (e.X, e.Y)) {
					parent_rows_button_active = true;
					Invalidate (parent_rows_button_rect);
				}
				break;

			default:
				break;
			}
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);

			if (column_resize_active) {
				/* erase the old line */
				DrawResizeLineVert (resize_column_x + resize_column_width_delta);

				resize_column_width_delta = e.X - resize_column_x;

				/* draw the new line */
				DrawResizeLineVert (resize_column_x + resize_column_width_delta);
				return;
			}
			else if (row_resize_active) {
				/* erase the old line */
				DrawResizeLineHoriz (resize_row_y + resize_row_height_delta);

				resize_row_height_delta = e.Y - resize_row_y;

				/* draw the new line */
				DrawResizeLineHoriz (resize_row_y + resize_row_height_delta);
				return;
			}
			else {
				/* determine the cursor to use */
				HitTestInfo testinfo;
				testinfo = HitTest (e.X, e.Y);

				switch (testinfo.Type) {
				case HitTestType.ColumnResize:
					Cursor = Cursors.VSplit;
					break;
				case HitTestType.RowResize:
					Cursor = Cursors.HSplit;
					break;
				case HitTestType.Caption:
					Cursor = Cursors.Default;
					if (back_button_rect.Contains (e.X, e.Y)) {
						if (!back_button_mouseover)
							Invalidate (back_button_rect);
						back_button_mouseover = true;
					} else if (back_button_mouseover) {
						Invalidate (back_button_rect);
						back_button_mouseover = false;
					}

					if (parent_rows_button_rect.Contains (e.X, e.Y)) {
						if (parent_rows_button_mouseover)
							Invalidate (parent_rows_button_rect);
						parent_rows_button_mouseover = true;
					} else if (parent_rows_button_mouseover) {
						Invalidate (parent_rows_button_rect);
						parent_rows_button_mouseover = false;
					}
					break;
				case HitTestType.Cell:
					if (rows[testinfo.Row].IsExpanded) {
						Rectangle relation_area = rows[testinfo.Row].relation_area;
						relation_area.Y = rows[testinfo.Row].VerticalOffset + cells_area.Y + rows[testinfo.Row].Height - rows[testinfo.Row].RelationHeight;
						if (relation_area.Contains (e.X, e.Y)) {
							Cursor = Cursors.Hand;
							break;
						}
					}

					Cursor = Cursors.Default;
					break;
				case HitTestType.RowHeader:
					if (e.Button == MouseButtons.Left)
						ShiftSelection (testinfo.Row);

					Cursor = Cursors.Default;
					break;
				default:
					Cursor = Cursors.Default;
					break;
				}
			}
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseUp (e);

			if (column_resize_active) {
				column_resize_active = false;
				if (resize_column_width_delta + CurrentTableStyle.GridColumnStyles[resize_column].Width < 0)
					resize_column_width_delta = -CurrentTableStyle.GridColumnStyles[resize_column].Width;
				CurrentTableStyle.GridColumnStyles[resize_column].Width += resize_column_width_delta;
				width_of_all_columns += resize_column_width_delta;
				Edit ();
				Invalidate ();
			} else if (row_resize_active) {
				row_resize_active = false;

				if (resize_row_height_delta + rows[resize_row].Height < 0)
					resize_row_height_delta = -rows[resize_row].Height;

				rows[resize_row].height = rows[resize_row].Height + resize_row_height_delta;
				for (int i = resize_row + 1; i < rows.Length; i ++)
					rows[i].VerticalOffset += resize_row_height_delta;

				Edit ();
				CalcAreasAndInvalidate ();
			} else if (back_button_active) {
				if (back_button_rect.Contains (e.X, e.Y)) {
					Invalidate (back_button_rect);
					NavigateBack ();
					OnBackButtonClicked (this, EventArgs.Empty);
				}
				back_button_active = false;
			} else if (parent_rows_button_active) {
				if (parent_rows_button_rect.Contains (e.X, e.Y)) {
					Invalidate (parent_rows_button_rect);
					ParentRowsVisible = !ParentRowsVisible;
					OnShowParentDetailsButtonClicked (this, EventArgs.Empty);
				}
				parent_rows_button_active = false;
			}
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel (e);

			bool ctrl_pressed = ((Control.ModifierKeys & Keys.Control) != 0);
			int pixels;

			if (ctrl_pressed) { // scroll horizontally
				if (!horiz_scrollbar.Visible)
					return;

				if (e.Delta > 0) {
					/* left */
					pixels = Math.Max (horiz_scrollbar.Minimum,
							   horiz_scrollbar.Value - horiz_scrollbar.LargeChange);
				} else {
					/* right */
					pixels = Math.Min (horiz_scrollbar.Maximum - horiz_scrollbar.LargeChange + 1,
							   horiz_scrollbar.Value + horiz_scrollbar.LargeChange);
				}

				GridHScrolled (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, pixels));
				horiz_scrollbar.Value = pixels;
			} else {
				if (!vert_scrollbar.Visible)
					return;

				if (e.Delta > 0) {
					/* up */
					pixels = Math.Max (vert_scrollbar.Minimum,
							   vert_scrollbar.Value - vert_scrollbar.LargeChange);
				} else {
					/* down */
					pixels = Math.Min (vert_scrollbar.Maximum - vert_scrollbar.LargeChange + 1,
							   vert_scrollbar.Value + vert_scrollbar.LargeChange);
				}

				GridVScrolled (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, pixels));
				vert_scrollbar.Value = pixels;
			}
		}

		protected void OnNavigate (NavigateEventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [NavigateEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnPaint (PaintEventArgs pe)
		{
			ThemeEngine.Current.DataGridPaint (pe, this);
		}

		protected override void OnPaintBackground (PaintEventArgs ebe)
		{
		}

		protected virtual void OnParentRowsLabelStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ParentRowsLabelStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnParentRowsVisibleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ParentRowsVisibleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnReadOnlyChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ReadOnlyChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}

		protected void OnRowHeaderClick (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [RowHeaderClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected void OnScroll (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ScrollEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected void OnShowParentDetailsButtonClicked (object sender, EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ShowParentDetailsButtonClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			return ProcessGridKey (new KeyEventArgs (keyData));
		}

		void UpdateSelectionAfterCursorMove (bool extend_selection)
		{
			if (extend_selection) {
				CancelEditing ();
				ShiftSelection (CurrentRow);
			} else {
				ResetSelection ();
				selection_start = CurrentRow;
			}
		}

		protected bool ProcessGridKey (KeyEventArgs ke)
		{
			bool ctrl_pressed = ((ke.Modifiers & Keys.Control) != 0);
			//bool alt_pressed = ((ke.Modifiers & Keys.Alt) != 0);
			bool shift_pressed = ((ke.Modifiers & Keys.Shift) != 0);

			switch (ke.KeyCode) {
			case Keys.Escape:
				if (is_changing)
					AbortEditing ();
				else {
					CancelEditing ();

					if (cursor_in_add_row && CurrentRow > 0)
						CurrentRow--;
				}

				Edit ();
				return true;
				
			case Keys.D0:
				if (ctrl_pressed) {
					if (is_editing)
						CurrentTableStyle.GridColumnStyles[CurrentColumn].EnterNullValue ();
					return true;
				}
				return false;

			case Keys.Enter:
				if (is_changing)
					CurrentRow ++;
				return true;

			case Keys.Tab:
				if (shift_pressed) {
					if (CurrentColumn > 0)
						CurrentColumn --;
					else if ((CurrentRow > 0) && (CurrentColumn == 0))
						CurrentCell = new DataGridCell (CurrentRow - 1, CurrentTableStyle.GridColumnStyles.Count - 1);
				} else {
					if (CurrentColumn < CurrentTableStyle.GridColumnStyles.Count - 1)
						CurrentColumn ++;
					else if ((CurrentRow <= RowsCount) && (CurrentColumn == CurrentTableStyle.GridColumnStyles.Count - 1))
						CurrentCell = new DataGridCell (CurrentRow + 1, 0);
				}

				UpdateSelectionAfterCursorMove (false);

				return true;

			case Keys.Right:
				if (ctrl_pressed) {
					CurrentColumn = CurrentTableStyle.GridColumnStyles.Count - 1;
				} else {
					if (CurrentColumn < CurrentTableStyle.GridColumnStyles.Count - 1) {
						CurrentColumn ++;
					} else if (CurrentRow < RowsCount - 1
						   || (CurrentRow == RowsCount - 1
						       && !cursor_in_add_row)) {
						CurrentCell = new DataGridCell (CurrentRow + 1, 0);
					}
				}

				UpdateSelectionAfterCursorMove (false);

				return true;

			case Keys.Left:
				if (ctrl_pressed) {
					CurrentColumn = 0;
				} else {
					if (current_cell.ColumnNumber > 0)
						CurrentColumn --;
					else if (CurrentRow > 0)
						CurrentCell = new DataGridCell (CurrentRow - 1, CurrentTableStyle.GridColumnStyles.Count - 1);
				}

				UpdateSelectionAfterCursorMove (false);

				return true;

			case Keys.Up:
				if (ctrl_pressed)
					CurrentRow = 0;
				else if (CurrentRow > 0)
					CurrentRow --;

				UpdateSelectionAfterCursorMove (shift_pressed);

				return true;

			case Keys.Down:
				if (ctrl_pressed)
					CurrentRow = RowsCount - 1;
				else if (CurrentRow < RowsCount - 1)
					CurrentRow ++;
				else if (CurrentRow == RowsCount - 1 && cursor_in_add_row && (add_row_changed || is_changing))
					CurrentRow ++;
				else if (CurrentRow == RowsCount - 1 && !cursor_in_add_row && !shift_pressed)
					CurrentRow ++;

				UpdateSelectionAfterCursorMove (shift_pressed);

				return true;

			case Keys.PageUp:
				if (CurrentRow > VLargeChange)
					CurrentRow -= VLargeChange;
				else
					CurrentRow = 0;

				UpdateSelectionAfterCursorMove (shift_pressed);

				return true;

			case Keys.PageDown:
				if (CurrentRow < RowsCount - VLargeChange)
					CurrentRow += VLargeChange;
				else
					CurrentRow = RowsCount - 1;

				UpdateSelectionAfterCursorMove (shift_pressed);

				return true;

			case Keys.Home:
				if (ctrl_pressed)
					CurrentCell = new DataGridCell (0, 0);
				else
					CurrentColumn = 0;

				UpdateSelectionAfterCursorMove (ctrl_pressed && shift_pressed);

				return true;

			case Keys.End:
				if (ctrl_pressed)
					CurrentCell = new DataGridCell (RowsCount - 1, CurrentTableStyle.GridColumnStyles.Count - 1);
				else
					CurrentColumn = CurrentTableStyle.GridColumnStyles.Count - 1;

				UpdateSelectionAfterCursorMove (ctrl_pressed && shift_pressed);

				return true;

			case Keys.Delete:
				if (is_editing)
					return false;
				else if (selected_rows.Keys.Count > 0) {
					// the removal of the items in the source will cause to
					// reset the selection, so we need a copy of it.
					int [] rows = new int [selected_rows.Keys.Count];
					selected_rows.Keys.CopyTo (rows, 0);

					// reverse order to keep index sanity
					int edit_row_index = ShowEditRow ? RowsCount : -1; // new cell is +1
					for (int i = rows.Length - 1; i >= 0; i--)
						if (rows [i] != edit_row_index)
							ListManager.RemoveAt (rows [i]);

					CalcAreasAndInvalidate ();
				}

				return true;
			}

			return false; // message not processed
		}

		protected override bool ProcessKeyPreview (ref Message m)
		{
			if ((Msg) m.Msg == Msg.WM_KEYDOWN) {
				Keys key = (Keys) m.WParam.ToInt32 ();
				KeyEventArgs ke = new KeyEventArgs (key);
				if (ProcessGridKey (ke))
					return true;

				// if we receive a key event, make sure that input is actually
				// taken into account.
				if (!is_editing) {
					Edit ();
					InvalidateRow (current_cell.RowNumber);
					return true;
				}
			}

			return base.ProcessKeyPreview (ref m);
		}
		
		protected bool ProcessTabKey (Keys keyData)
		{
			return false;
		}

		public void ResetAlternatingBackColor ()
		{
			grid_style.AlternatingBackColor = default_style.AlternatingBackColor;
		}

		public override void ResetBackColor ()
		{
			grid_style.BackColor = default_style.BackColor;
		}

		public override void ResetForeColor ()
		{
			grid_style.ForeColor = default_style.ForeColor;
		}

		public void ResetGridLineColor ()
		{
			grid_style.GridLineColor = default_style.GridLineColor;
		}

		public void ResetHeaderBackColor ()
		{
			grid_style.HeaderBackColor = default_style.HeaderBackColor;
		}

		public void ResetHeaderFont ()
		{
			grid_style.HeaderFont = null;
		}

		public void ResetHeaderForeColor ()
		{
			grid_style.HeaderForeColor = default_style.HeaderForeColor;
		}

		public void ResetLinkColor ()
		{
			grid_style.LinkColor = default_style.LinkColor;
		}

		public void ResetLinkHoverColor ()
		{
			grid_style.LinkHoverColor = default_style.LinkHoverColor;
		}

		protected void ResetSelection ()
		{
			InvalidateSelection ();
			selected_rows.Clear ();
			selection_start = -1;
		}

		void InvalidateSelection ()
		{
			foreach (int row in selected_rows.Keys) {
				rows[row].IsSelected = false;
				InvalidateRow (row);
			}
		}

		public void ResetSelectionBackColor ()
		{
			grid_style.SelectionBackColor = default_style.SelectionBackColor;
		}

		public void ResetSelectionForeColor ()
		{
			grid_style.SelectionForeColor = default_style.SelectionForeColor;
		}

		public void Select (int row)
		{
			EndEdit();

			if (selected_rows.Count == 0)
				selection_start = row;

			// UIA Framework: To raise event only when selecting
			bool wasSelected = rows [row].IsSelected;

			selected_rows[row] = true;
			rows[row].IsSelected = true;

			InvalidateRow (row);

			// UIA Framework:
			if (!wasSelected)
				OnUIASelectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Add, row));

		}

		public void SetDataBinding (object dataSource, string dataMember)
		{
			SetDataSource (dataSource, dataMember);
		}

		protected virtual bool ShouldSerializeAlternatingBackColor ()
		{
			return (grid_style.AlternatingBackColor != default_style.AlternatingBackColor);
		}

		protected virtual bool ShouldSerializeBackgroundColor ()
		{
			return (background_color != def_background_color);
		}

		protected virtual bool ShouldSerializeCaptionBackColor ()
		{
			return (caption_backcolor != def_caption_backcolor);
		}

		protected virtual bool ShouldSerializeCaptionForeColor ()
		{
			return caption_forecolor != def_caption_forecolor;
		}

		protected virtual bool ShouldSerializeGridLineColor ()
		{
			return grid_style.GridLineColor != default_style.GridLineColor;
		}

		protected virtual bool ShouldSerializeHeaderBackColor ()
		{
			return grid_style.HeaderBackColor != default_style.HeaderBackColor;
		}

		protected bool ShouldSerializeHeaderFont ()
		{
			return grid_style.HeaderFont != default_style.HeaderFont;
		}

		protected virtual bool ShouldSerializeHeaderForeColor ()
		{
			return grid_style.HeaderForeColor != default_style.HeaderForeColor;
		}

		protected virtual bool ShouldSerializeLinkHoverColor ()
		{
			return grid_style.LinkHoverColor != grid_style.LinkHoverColor;
		}

		protected virtual bool ShouldSerializeParentRowsBackColor ()
		{
			return parent_rows_backcolor != def_parent_rows_backcolor;
		}

		protected virtual bool ShouldSerializeParentRowsForeColor ()
		{
			return parent_rows_backcolor != def_parent_rows_backcolor;
		}

		protected bool ShouldSerializePreferredRowHeight ()
		{
			return grid_style.PreferredRowHeight != default_style.PreferredRowHeight;
		}

		protected bool ShouldSerializeSelectionBackColor ()
		{
			return grid_style.SelectionBackColor != default_style.SelectionBackColor;
		}

		protected virtual bool ShouldSerializeSelectionForeColor ()
		{
			return grid_style.SelectionForeColor != default_style.SelectionForeColor;
		}

		public void SubObjectsSiteChange (bool site)
		{
		}

		public void UnSelect (int row)
		{
			// UIA Framework: To raise event only when unselecting 
			bool wasSelected = rows  [row].IsSelected;

			rows[row].IsSelected = false;
			selected_rows.Remove (row);
			InvalidateRow (row);

			// UIA Framework: Raises selection event
			if (!wasSelected)
				OnUIASelectionChangedEvent (new CollectionChangeEventArgs (CollectionChangeAction.Remove, row));
		}
		#endregion	// Public Instance Methods

		#region Private Instance Methods

		internal void CalcAreasAndInvalidate ()
		{
			CalcGridAreas ();
			Invalidate ();
		}
		
		private void ConnectListManagerEvents ()
		{
			list_manager.MetaDataChanged += new EventHandler (OnListManagerMetaDataChanged);
			list_manager.PositionChanged += new EventHandler (OnListManagerPositionChanged);
			list_manager.ItemChanged += new ItemChangedEventHandler (OnListManagerItemChanged);
		}
		
		private void DisconnectListManagerEvents ()
		{
			list_manager.MetaDataChanged -= new EventHandler (OnListManagerMetaDataChanged);
			list_manager.PositionChanged -= new EventHandler (OnListManagerPositionChanged);
			list_manager.ItemChanged -= new ItemChangedEventHandler (OnListManagerItemChanged);
		}

		void DisconnectTableStyleEvents ()
		{
			current_style.AllowSortingChanged -= new EventHandler (TableStyleChanged);
			current_style.AlternatingBackColorChanged -= new EventHandler (TableStyleChanged);
			current_style.BackColorChanged -= new EventHandler (TableStyleChanged);
			current_style.ColumnHeadersVisibleChanged -= new EventHandler (TableStyleChanged);
			current_style.ForeColorChanged -= new EventHandler (TableStyleChanged);
			current_style.GridLineColorChanged -= new EventHandler (TableStyleChanged);
			current_style.GridLineStyleChanged -= new EventHandler (TableStyleChanged);
			current_style.HeaderBackColorChanged -= new EventHandler (TableStyleChanged);
			current_style.HeaderFontChanged -= new EventHandler (TableStyleChanged);
			current_style.HeaderForeColorChanged -= new EventHandler (TableStyleChanged);
			current_style.LinkColorChanged -= new EventHandler (TableStyleChanged);
			current_style.LinkHoverColorChanged -= new EventHandler (TableStyleChanged);
			current_style.MappingNameChanged -= new EventHandler (TableStyleChanged);
			current_style.PreferredColumnWidthChanged -= new EventHandler (TableStyleChanged);
			current_style.PreferredRowHeightChanged -= new EventHandler (TableStyleChanged);
			current_style.ReadOnlyChanged -= new EventHandler (TableStyleChanged);
			current_style.RowHeadersVisibleChanged -= new EventHandler (TableStyleChanged);
			current_style.RowHeaderWidthChanged -= new EventHandler (TableStyleChanged);
			current_style.SelectionBackColorChanged -= new EventHandler (TableStyleChanged);
			current_style.SelectionForeColorChanged -= new EventHandler (TableStyleChanged);
		}

		void ConnectTableStyleEvents ()
		{
			current_style.AllowSortingChanged += new EventHandler (TableStyleChanged);
			current_style.AlternatingBackColorChanged += new EventHandler (TableStyleChanged);
			current_style.BackColorChanged += new EventHandler (TableStyleChanged);
			current_style.ColumnHeadersVisibleChanged += new EventHandler (TableStyleChanged);
			current_style.ForeColorChanged += new EventHandler (TableStyleChanged);
			current_style.GridLineColorChanged += new EventHandler (TableStyleChanged);
			current_style.GridLineStyleChanged += new EventHandler (TableStyleChanged);
			current_style.HeaderBackColorChanged += new EventHandler (TableStyleChanged);
			current_style.HeaderFontChanged += new EventHandler (TableStyleChanged);
			current_style.HeaderForeColorChanged += new EventHandler (TableStyleChanged);
			current_style.LinkColorChanged += new EventHandler (TableStyleChanged);
			current_style.LinkHoverColorChanged += new EventHandler (TableStyleChanged);
			current_style.MappingNameChanged += new EventHandler (TableStyleChanged);
			current_style.PreferredColumnWidthChanged += new EventHandler (TableStyleChanged);
			current_style.PreferredRowHeightChanged += new EventHandler (TableStyleChanged);
			current_style.ReadOnlyChanged += new EventHandler (TableStyleChanged);
			current_style.RowHeadersVisibleChanged += new EventHandler (TableStyleChanged);
			current_style.RowHeaderWidthChanged += new EventHandler (TableStyleChanged);
			current_style.SelectionBackColorChanged += new EventHandler (TableStyleChanged);
			current_style.SelectionForeColorChanged += new EventHandler (TableStyleChanged);
		}

		void TableStyleChanged (object sender, EventArgs args)
		{
			EndEdit ();
			CalcAreasAndInvalidate ();
		}


		private void EnsureCellVisibility (DataGridCell cell)
		{
			if (cell.ColumnNumber <= first_visible_column ||
				cell.ColumnNumber + 1 >= first_visible_column + visible_column_count) {

				first_visible_column = GetFirstColumnForColumnVisibility (first_visible_column, cell.ColumnNumber);
				int pixel = GetColumnStartingPixel (first_visible_column);
				ScrollToColumnInPixels (pixel);
				horiz_scrollbar.Value = pixel;
				Update();
			}

			if (cell.RowNumber < first_visible_row ||
			    cell.RowNumber + 1 >= first_visible_row + visible_row_count) {

				if (cell.RowNumber + 1 >= first_visible_row + visible_row_count) {
					int old_first_visible_row = first_visible_row;
					first_visible_row = 1 + cell.RowNumber - visible_row_count;
					UpdateVisibleRowCount ();
					ScrollToRow (old_first_visible_row, first_visible_row);
				} else {
					int old_first_visible_row = first_visible_row;
					first_visible_row = cell.RowNumber;
					UpdateVisibleRowCount ();
					ScrollToRow (old_first_visible_row, first_visible_row);
				}

				vert_scrollbar.Value = first_visible_row;
			}
		}

		private void SetDataSource (object source, string member)
		{
			SetDataSource (source, member, true);
		}

		bool in_setdatasource;
		private void SetDataSource (object source, string member, bool recreate_rows)
		{
			CurrencyManager old_lm = list_manager;

			/* we need this bool flag to work around a
			 * problem with OnBindingContextChanged.  once
			 * that stuff works properly, remove this
			 * hack */
			if (in_setdatasource)
				return;
			in_setdatasource = true;

#if false
			if (datasource == source && member == datamember)
				return;
#endif

			if (source != null && source as IListSource != null && source as IList != null)
				throw new Exception ("Wrong complex data binding source");

			datasource = source;
			datamember = member;

			if (is_editing)
				CancelEditing ();

			current_cell = new DataGridCell ();

			if (list_manager != null)
				DisconnectListManagerEvents ();

			list_manager = null;

			/* create the new list manager */
			if (BindingContext != null && datasource != null)
				list_manager = (CurrencyManager) BindingContext [datasource, datamember];

			if (list_manager != null)
				ConnectListManagerEvents ();

			if (old_lm != list_manager) {
				BindColumns ();

				/* reset first_visible_row to 0 here before
				 * doing anything that'll requires us to
				 * figure out if we need a scrollbar. */
				vert_scrollbar.Value = 0;
				horiz_scrollbar.Value = 0;
				first_visible_row = 0;

				if (recreate_rows)
					RecreateDataGridRows (false);
			}

			CalcAreasAndInvalidate ();

			in_setdatasource = false;

			OnDataSourceChanged (EventArgs.Empty);
		}

		void RecreateDataGridRows (bool recalc)
		{
			DataGridRelationshipRow[] new_rows = new DataGridRelationshipRow[RowsCount + (ShowEditRow ? 1 : 0)];
			int start_index = 0;
			if (rows != null) {
				start_index = rows.Length;
				Array.Copy (rows, 0, new_rows, 0, rows.Length < new_rows.Length ? rows.Length : new_rows.Length);
			}

			for (int i = start_index; i < new_rows.Length; i ++) {
				new_rows[i] = new DataGridRelationshipRow (this);
				new_rows[i].height = RowHeight;
				if (i > 0)
					new_rows[i].VerticalOffset = new_rows[i-1].VerticalOffset + new_rows[i-1].Height;
			}

			// UIA Framework event: Updates collection list depending on binding
			CollectionChangeAction action = CollectionChangeAction.Refresh;
			if (rows != null) {
				if (new_rows.Length - rows.Length > 0)
					action = CollectionChangeAction.Add;
				else
					action = CollectionChangeAction.Remove;
			}
			rows = new_rows;

			if (recalc)
				CalcAreasAndInvalidate ();
			// UIA Framework event: Row added/removed 
			OnUIACollectionChangedEvent (new CollectionChangeEventArgs (action, -1));
		}

		internal void UpdateRowsFrom (DataGridRelationshipRow row)
		{
			int start_index = Array.IndexOf (rows, row);
			if (start_index == -1)
				return;

			for (int i = start_index + 1; i < rows.Length; i ++)
				rows[i].VerticalOffset = rows[i-1].VerticalOffset + rows[i-1].Height;

			CalcAreasAndInvalidate ();
		}

		void BindColumns ()
		{
			if (list_manager != null) {
				string list_name = list_manager.GetListName (null);
				if (TableStyles[list_name] == null) {
					// no style exists by the supplied name
					current_style.GridColumnStyles.Clear ();
					current_style.CreateColumnsForTable (false);
				} else if (CurrentTableStyle == grid_style ||
					 CurrentTableStyle.MappingName != list_name) {
					// If the style has been defined by the user, use it
					// Also, if the user provided style is empty,
					// force a bind for it
					CurrentTableStyle = styles_collection[list_name];
					current_style.CreateColumnsForTable (current_style.GridColumnStyles.Count > 0);
				} else {
					current_style.CreateColumnsForTable (true);
				}
			} else
				current_style.CreateColumnsForTable (false);
		}

		private void OnListManagerMetaDataChanged (object sender, EventArgs e)
		{
			BindColumns ();
			CalcAreasAndInvalidate ();
		}

		private void OnListManagerPositionChanged (object sender, EventArgs e)
		{
			// Set the field directly, as we are empty now and using CurrentRow
			// directly would add a new row in this case.
			if (list_manager.Count == 0) {
				current_cell = new DataGridCell (0, 0);
				return;
			}

			from_positionchanged_handler = true;
			CurrentRow = list_manager.Position;
			from_positionchanged_handler = false;
		}

		private void OnListManagerItemChanged (object sender, ItemChangedEventArgs e)
		{
			// if it was us who created the new row in CurrentCell, ignore it and don't recreate the rows yet.
			if (adding_new_row)
				return;

			if (e.Index == -1) {
				ResetSelection ();
				if (rows == null || RowsCount != rows.Length - (ShowEditRow ? 1 : 0))
					RecreateDataGridRows (true);
			} else {
				InvalidateRow (e.Index);
			}
		}

		private void OnTableStylesCollectionChanged (object sender, CollectionChangeEventArgs e)
		{
			if (ListManager == null)
				return;
			
			string list_name = ListManager.GetListName (null);
			switch (e.Action) {
			case CollectionChangeAction.Add:
				if (e.Element != null && String.Compare (list_name, ((DataGridTableStyle)e.Element).MappingName, true) == 0) {
					CurrentTableStyle = (DataGridTableStyle)e.Element;
					// force to auto detect columns in case the new style is completely empty
					((DataGridTableStyle) e.Element).CreateColumnsForTable (CurrentTableStyle.GridColumnStyles.Count > 0);
				}
				break;
			case CollectionChangeAction.Remove:
				if (e.Element != null && String.Compare (list_name, ((DataGridTableStyle)e.Element).MappingName, true) == 0) {
					CurrentTableStyle = default_style;
					current_style.GridColumnStyles.Clear ();
					current_style.CreateColumnsForTable (false);
				}
				break;
			case CollectionChangeAction.Refresh:
				if (CurrentTableStyle == default_style
					|| String.Compare (list_name, CurrentTableStyle.MappingName, true) != 0) {
					DataGridTableStyle style = styles_collection [list_name];
					if (style != null) {
						CurrentTableStyle = style;
						current_style.CreateColumnsForTable (false);
					} else {
						CurrentTableStyle = default_style;
						current_style.GridColumnStyles.Clear ();
						current_style.CreateColumnsForTable (false);
					}
				}
				break;
			}
			CalcAreasAndInvalidate ();
		}

		private void AddNewRow ()
		{
			ListManager.EndCurrentEdit ();
			ListManager.AddNew ();
		}

		private void Edit ()
		{
			if (CurrentTableStyle.GridColumnStyles.Count == 0)
				return;

			if (!CurrentTableStyle.GridColumnStyles[CurrentColumn].bound)
				return;

			// if we don't have any rows nor the "new" cell, there's nothing to do
			if (ListManager != null && (ListManager.Count == 0 && !ListManager.AllowNew))
				return;

			is_editing = true;
			is_changing = false;

			CurrentTableStyle.GridColumnStyles[CurrentColumn].Edit (ListManager,
				CurrentRow, GetCellBounds (CurrentRow, CurrentColumn),
				_readonly, null, true);
		}

		private void EndEdit ()
		{
			if (CurrentTableStyle.GridColumnStyles.Count == 0)
				return;

			if (!CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].bound)
				return;

			EndEdit (CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber],
				current_cell.RowNumber, false);
		}

		private void ShiftSelection (int index)
		{
			// we have to save off selection_start
			// because ResetSelection clobbers it
			int saved_selection_start = selection_start;
			int start, end;

			ResetSelection ();
			selection_start = saved_selection_start;

			if (index >= selection_start) {
				start = selection_start;
				end = index;
			} else {
				start = index;
				end = selection_start;
			}

			if (start == -1) start = 0;

			for (int idx = start; idx <= end; idx ++)
				Select (idx);
		}

		private void ScrollToColumnInPixels (int pixel)
		{
			int pixels;

			if (pixel > horiz_pixeloffset) // ScrollRight
				pixels = -1 * (pixel - horiz_pixeloffset);
			else
				pixels = horiz_pixeloffset - pixel;

			Rectangle area = cells_area;

			if (ColumnHeadersVisible) {
				area.Y -= ColumnHeadersArea.Height;
				area.Height += ColumnHeadersArea.Height;
			}
				
			horiz_pixeloffset = pixel;
			UpdateVisibleColumn ();

			EndEdit ();

			XplatUI.ScrollWindow (Handle, area, pixels, 0, false);

			int pixel_offset = GetColumnStartingPixel (CurrentColumn);
			int next_pixel_offset = pixel_offset + CurrentTableStyle.GridColumnStyles[CurrentColumn].Width;

			if (pixel_offset >= horiz_pixeloffset
			    && next_pixel_offset < horiz_pixeloffset + cells_area.Width)
				Edit ();
		}

		private void ScrollToRow (int old_row, int new_row)
		{
			int pixels = 0;
			int i;

			if (new_row > old_row) { // Scrolldown
				for (i = old_row; i < new_row; i ++)
					pixels -= rows[i].Height;
			} else {
				for (i = new_row; i < old_row; i ++)
					pixels += rows[i].Height;
			}

			if (pixels == 0)
				return;

			Rectangle rows_area = cells_area; // Cells area - partial rows space

			if (RowHeadersVisible) {
				rows_area.X -= RowHeaderWidth;
				rows_area.Width += RowHeaderWidth;
			}

			/* scroll the window */
			XplatUI.ScrollWindow (Handle, rows_area, 0, pixels, false);

			/* if the row is still */
			if (CurrentRow >= first_visible_row && CurrentRow < first_visible_row + visible_row_count)
				Edit ();
		}


		private void ColumnResize (int column) 
		{
			CurrencyManager source = this.ListManager;
			DataGridColumnStyle style = CurrentTableStyle.GridColumnStyles[column];
			string headerText = style.HeaderText;
			using (Graphics g = base.CreateGraphics ()) {
				int rows = source.Count;
				int width = (int)g.MeasureString (headerText, CurrentTableStyle.HeaderFont).Width + 4;

				for (int i = 0; i < rows; i++) {
					int rowColWidth = (int)style.GetPreferredSize (g, style.GetColumnValueAtRow (source, i)).Width;
					if (rowColWidth > width)
						width = rowColWidth;
				}
				if (style.Width != width)
					style.Width = width;
			}
		}

		private void RowResize (int row)
		{
			CurrencyManager source = this.ListManager;
			using (Graphics g = base.CreateGraphics ()) {
				GridColumnStylesCollection columns = CurrentTableStyle.GridColumnStyles;
				int colCount = columns.Count;
				//int rowCount = source.Count;
				int height = 0;
				for (int i = 0; i < colCount; i++) {
					object val = columns[i].GetColumnValueAtRow (source, row);
					height = Math.Max (columns[i].GetPreferredHeight (g, val), height);
				}
				if (this.DataGridRows[row].Height != height)
					this.DataGridRows[row].Height = height;
			}
		}
		#endregion Private Instance Methods

		#region Events
		static object AllowNavigationChangedEvent = new object ();
		static object BackButtonClickEvent = new object ();
		static object BackgroundColorChangedEvent = new object ();
		static object BorderStyleChangedEvent = new object ();
		static object CaptionVisibleChangedEvent = new object ();
		static object CurrentCellChangedEvent = new object ();
		static object DataSourceChangedEvent = new object ();
		static object FlatModeChangedEvent = new object ();
		static object NavigateEvent = new object ();
		static object ParentRowsLabelStyleChangedEvent = new object ();
		static object ParentRowsVisibleChangedEvent = new object ();
		static object ReadOnlyChangedEvent = new object ();
		static object RowHeaderClickEvent = new object ();
		static object ScrollEvent = new object ();
		static object ShowParentDetailsButtonClickEvent = new object ();

		public event EventHandler AllowNavigationChanged {
			add { Events.AddHandler (AllowNavigationChangedEvent, value); }
			remove { Events.RemoveHandler (AllowNavigationChangedEvent, value); }
		}

		public event EventHandler BackButtonClick {
			add { Events.AddHandler (BackButtonClickEvent, value); }
			remove { Events.RemoveHandler (BackButtonClickEvent, value); }
		}

		public event EventHandler BackgroundColorChanged {
			add { Events.AddHandler (BackgroundColorChangedEvent, value); }
			remove { Events.RemoveHandler (BackgroundColorChangedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler CursorChanged {
			add { base.CursorChanged += value; }
			remove { base.CursorChanged -= value; }
		}

		public event EventHandler BorderStyleChanged {
			add { Events.AddHandler (BorderStyleChangedEvent, value); }
			remove { Events.RemoveHandler (BorderStyleChangedEvent, value); }
		}

		public event EventHandler CaptionVisibleChanged {
			add { Events.AddHandler (CaptionVisibleChangedEvent, value); }
			remove { Events.RemoveHandler (CaptionVisibleChangedEvent, value); }
		}

		public event EventHandler CurrentCellChanged {
			add { Events.AddHandler (CurrentCellChangedEvent, value); }
			remove { Events.RemoveHandler (CurrentCellChangedEvent, value); }
		}

		public event EventHandler DataSourceChanged {
			add { Events.AddHandler (DataSourceChangedEvent, value); }
			remove { Events.RemoveHandler (DataSourceChangedEvent, value); }
		}

		public event EventHandler FlatModeChanged {
			add { Events.AddHandler (FlatModeChangedEvent, value); }
			remove { Events.RemoveHandler (FlatModeChangedEvent, value); }
		}

		public event NavigateEventHandler Navigate {
			add { Events.AddHandler (NavigateEvent, value); }
			remove { Events.RemoveHandler (NavigateEvent, value); }
		}

		public event EventHandler ParentRowsLabelStyleChanged {
			add { Events.AddHandler (ParentRowsLabelStyleChangedEvent, value); }
			remove { Events.RemoveHandler (ParentRowsLabelStyleChangedEvent, value); }
		}

		public event EventHandler ParentRowsVisibleChanged {
			add { Events.AddHandler (ParentRowsVisibleChangedEvent, value); }
			remove { Events.RemoveHandler (ParentRowsVisibleChangedEvent, value); }
		}

		public event EventHandler ReadOnlyChanged {
			add { Events.AddHandler (ReadOnlyChangedEvent, value); }
			remove { Events.RemoveHandler (ReadOnlyChangedEvent, value); }
		}

		protected event EventHandler RowHeaderClick {
			add { Events.AddHandler (RowHeaderClickEvent, value); }
			remove { Events.RemoveHandler (RowHeaderClickEvent, value); }
		}

		public event EventHandler Scroll {
			add { Events.AddHandler (ScrollEvent, value); }
			remove { Events.RemoveHandler (ScrollEvent, value); }
		}

		public event EventHandler ShowParentDetailsButtonClick {
			add { Events.AddHandler (ShowParentDetailsButtonClickEvent, value); }
			remove { Events.RemoveHandler (ShowParentDetailsButtonClickEvent, value); }
		}
		#endregion	// Events

		#region Code originally in DataGridDrawingLogic.cs

		#region	Local Variables

		// Areas
		Rectangle parent_rows;
		int width_of_all_columns;

		internal Rectangle caption_area;
		internal Rectangle column_headers_area;	// Used columns header area
		internal int column_headers_max_width; 	// Total width (max width) for columns headrs
		internal Rectangle row_headers_area;	// Used Headers rows area
		internal Rectangle cells_area;
		#endregion // Local Variables

		#region Public Instance Methods

		// Calc the max with of all columns
		private int CalcAllColumnsWidth ()
		{
			int width = 0;
			int cnt = CurrentTableStyle.GridColumnStyles.Count;

			for (int col = 0; col < cnt; col++) {
				if (CurrentTableStyle.GridColumnStyles[col].bound == false) {
					continue;
				}
				width += CurrentTableStyle.GridColumnStyles[col].Width;
			}
			return width;
		}

		// Gets a column from a pixel
		private int FromPixelToColumn (int pixel, out int column_x)
		{
			int width = 0;
			int cnt = CurrentTableStyle.GridColumnStyles.Count;
			column_x = 0;

			if (cnt == 0)
				return -1;
				
			if (CurrentTableStyle.CurrentRowHeadersVisible) {
				width += row_headers_area.X + row_headers_area.Width;
				column_x += row_headers_area.X + row_headers_area.Width;
				if (pixel < width)
					return -1;
			}

			for (int col = 0; col < cnt; col++) {
				if (CurrentTableStyle.GridColumnStyles[col].bound == false)
					continue;

				width += CurrentTableStyle.GridColumnStyles[col].Width;

				if (pixel < width)
					return col;

				column_x += CurrentTableStyle.GridColumnStyles[col].Width;
			}

			return cnt - 1;
		}

		internal int GetColumnStartingPixel (int my_col)
		{
			int width = 0;
			int cnt = CurrentTableStyle.GridColumnStyles.Count;

			for (int col = 0; col < cnt; col++) {
				if (CurrentTableStyle.GridColumnStyles[col].bound == false) {
					continue;
				}

				if (my_col == col)
					return width;

				width += CurrentTableStyle.GridColumnStyles[col].Width;
			}

			return 0;
		}
		
		// Which column has to be the first visible column to ensure a column visibility
		int GetFirstColumnForColumnVisibility (int current_first_visible_column, int column)
		{
			int new_col = column;
			int width = 0;
			
			if (column > current_first_visible_column) { // Going forward
				for (new_col = column; new_col >= 0; new_col--) {
					if (!CurrentTableStyle.GridColumnStyles[new_col].bound)
						continue;
					width += CurrentTableStyle.GridColumnStyles[new_col].Width;
					
					if (width >= cells_area.Width)
						return new_col + 1;
						//return new_col < CurrentTableStyle.GridColumnStyles.Count ? new_col + 1 : CurrentTableStyle.GridColumnStyles.Count;
				}
				return 0;
			} else {
				return  column;
			}
		}

		bool in_calc_grid_areas;
		void CalcGridAreas ()
		{
			if (!IsHandleCreated) // Delay calculations until the handle is created
				return;

			/* make sure we don't happen to end up in this method again */
			if (in_calc_grid_areas)
				return;

			in_calc_grid_areas = true;

			/* Order is important. E.g. row headers max. height depends on caption */
			horiz_pixeloffset = 0;
			CalcCaption ();
			CalcParentRows ();
			CalcParentButtons ();
			UpdateVisibleRowCount ();
			CalcRowHeaders ();
			width_of_all_columns = CalcAllColumnsWidth ();
			CalcColumnHeaders ();
			CalcCellsArea ();

			bool needHoriz = false;
			bool needVert = false;

			/* figure out which scrollbars we need, and what the visible areas are */
			int visible_cells_width = cells_area.Width;
			int visible_cells_height = cells_area.Height;
			int allrows = RowsCount;

			if (ShowEditRow && RowsCount > 0)
				allrows++;

			/* use a loop to iteratively calculate whether
			 * we need horiz/vert scrollbars. */
			for (int i = 0; i < 3; i++) {
				if (needVert)
					visible_cells_width = cells_area.Width - vert_scrollbar.Width;
				if (needHoriz)
					visible_cells_height = cells_area.Height - horiz_scrollbar.Height;

				UpdateVisibleRowCount ();

				needHoriz = (width_of_all_columns > visible_cells_width);
				needVert = (allrows > MaxVisibleRowCount);
			}

			int horiz_scrollbar_width = ClientRectangle.Width;
			int horiz_scrollbar_maximum = 0;
			int vert_scrollbar_height = 0;
			int vert_scrollbar_maximum = 0;

			if (needVert)
				SetUpVerticalScrollBar (out vert_scrollbar_height, out vert_scrollbar_maximum);

			if (needHoriz)
				SetUpHorizontalScrollBar (out horiz_scrollbar_maximum);

			cells_area.Width = visible_cells_width;
			cells_area.Height = visible_cells_height;

			if (needVert && needHoriz) {
				if (ShowParentRows)
					parent_rows.Width -= vert_scrollbar.Width;

				if (!ColumnHeadersVisible) {
					if (column_headers_area.X + column_headers_area.Width > vert_scrollbar.Location.X) {
						column_headers_area.Width -= vert_scrollbar.Width;
					}
				}

				horiz_scrollbar_width -= vert_scrollbar.Width;
				vert_scrollbar_height -= horiz_scrollbar.Height;
			}

			if (needVert) {
				if (row_headers_area.Y + row_headers_area.Height > ClientRectangle.Y + ClientRectangle.Height) {
					row_headers_area.Height -= horiz_scrollbar.Height;
				}

				vert_scrollbar.Size = new Size (vert_scrollbar.Width,
								vert_scrollbar_height);

				vert_scrollbar.Maximum = vert_scrollbar_maximum;
				Controls.Add (vert_scrollbar);
				vert_scrollbar.Visible = true;
			} else {
				Controls.Remove (vert_scrollbar);
				vert_scrollbar.Visible = false;
			}

			if (needHoriz) {
				horiz_scrollbar.Size = new Size (horiz_scrollbar_width,
					horiz_scrollbar.Height);

				horiz_scrollbar.Maximum = horiz_scrollbar_maximum;
				Controls.Add (horiz_scrollbar);
				horiz_scrollbar.Visible = true;
			} else {
				Controls.Remove (horiz_scrollbar);
				horiz_scrollbar.Visible = false;
			}

			UpdateVisibleColumn ();
			UpdateVisibleRowCount ();

			in_calc_grid_areas = false;
		}

		void CalcCaption ()
		{
			caption_area.X = ClientRectangle.X;
			caption_area.Y = ClientRectangle.Y;
			caption_area.Width = ClientRectangle.Width;
			if (caption_visible) {
				caption_area.Height = CaptionFont.Height;
				if (caption_area.Height < back_button_image.Height)
					caption_area.Height = back_button_image.Height;
				caption_area.Height += 2;
			} else
				caption_area.Height = 0;
		}

		void CalcCellsArea ()
		{
			cells_area.X = ClientRectangle.X + row_headers_area.Width;
			cells_area.Y = column_headers_area.Y + column_headers_area.Height;
			cells_area.Width = ClientRectangle.X + ClientRectangle.Width - cells_area.X;
			if (cells_area.Width < 0)
				cells_area.Width = 0;
			cells_area.Height = ClientRectangle.Y + ClientRectangle.Height - cells_area.Y;
			if (cells_area.Height < 0)
				cells_area.Height = 0;
		}

		void CalcColumnHeaders ()
		{
			int max_width_cols;

			column_headers_area.X = ClientRectangle.X;
			column_headers_area.Y = parent_rows.Y + parent_rows.Height;

			// TODO: take into account Scrollbars
			column_headers_max_width = ClientRectangle.X + ClientRectangle.Width - column_headers_area.X;
			max_width_cols = column_headers_max_width;

			if (CurrentTableStyle.CurrentRowHeadersVisible)
				max_width_cols -= RowHeaderWidth;

			if (width_of_all_columns > max_width_cols) {
				column_headers_area.Width = column_headers_max_width;
			} else {
				column_headers_area.Width = width_of_all_columns;

				if (CurrentTableStyle.CurrentRowHeadersVisible)
					column_headers_area.Width += RowHeaderWidth;
			}

			if (ColumnHeadersVisible)
				column_headers_area.Height = CurrentTableStyle.HeaderFont.Height + 6;
			else
				column_headers_area.Height = 0;
		}

		void CalcParentRows ()
		{
			parent_rows.X = ClientRectangle.X;
			parent_rows.Y = caption_area.Y + caption_area.Height;
			parent_rows.Width = ClientRectangle.Width;
			if (ShowParentRows)
				parent_rows.Height = (CaptionFont.Height + 3) * data_source_stack.Count;
			else
				parent_rows.Height = 0;
		}

		void CalcParentButtons ()
		{
			if (data_source_stack.Count > 0 && CaptionVisible) {
				back_button_rect = new Rectangle (ClientRectangle.X + ClientRectangle.Width - 2 * (caption_area.Height - 2) - 8,
								  caption_area.Height / 2 - back_button_image.Height / 2,
								  back_button_image.Width, back_button_image.Height);
				parent_rows_button_rect = new Rectangle (ClientRectangle.X + ClientRectangle.Width - (caption_area.Height - 2) - 4,
									 caption_area.Height / 2 - parent_rows_button_image.Height / 2,
									 parent_rows_button_image.Width, parent_rows_button_image.Height);
			} else {
				back_button_rect = parent_rows_button_rect = Rectangle.Empty;
			}
		}

		void CalcRowHeaders ()
		{
			row_headers_area.X = ClientRectangle.X;
			row_headers_area.Y = column_headers_area.Y + column_headers_area.Height;
			row_headers_area.Height = ClientRectangle.Height + ClientRectangle.Y - row_headers_area.Y;

			if (CurrentTableStyle.CurrentRowHeadersVisible)
				row_headers_area.Width = RowHeaderWidth;
			else
				row_headers_area.Width = 0;
		}

		int GetVisibleRowCount (int visibleHeight)
		{
			int rows_height = 0;
			int r;
			for (r = FirstVisibleRow; r < rows.Length; r ++) {
				if (rows_height + rows[r].Height >= visibleHeight)
					break;
				rows_height += rows[r].Height;
			}

			if (r <= rows.Length - 1)
				r ++;

			return r - FirstVisibleRow;
		}

		void UpdateVisibleColumn ()
		{
			visible_column_count = 0;
			
			if (CurrentTableStyle.GridColumnStyles.Count == 0)
				return;

			int min_pixel;
			int max_pixel;
			int max_col;
			int unused;
			
			min_pixel = horiz_pixeloffset;
			if (CurrentTableStyle.CurrentRowHeadersVisible)
				min_pixel += row_headers_area.X + row_headers_area.Width;
			max_pixel = min_pixel + cells_area.Width;

			first_visible_column = FromPixelToColumn (min_pixel, out unused);
			max_col = FromPixelToColumn (max_pixel, out unused);

			for (int i = first_visible_column; i <= max_col; i ++) {
				if (CurrentTableStyle.GridColumnStyles[i].bound)
					visible_column_count++;
			}

			if (first_visible_column + visible_column_count < CurrentTableStyle.GridColumnStyles.Count) { 
				visible_column_count++; // Partially visible column
			}
		}

		void UpdateVisibleRowCount ()
		{
			visible_row_count = GetVisibleRowCount (cells_area.Height);

			CalcRowHeaders (); // Height depends on num of visible rows
		}

		void InvalidateCaption ()
		{
			if (caption_area.IsEmpty)
				return;

			Invalidate (caption_area);
		}

		void InvalidateRow (int row)
		{
			if (row < FirstVisibleRow || row > FirstVisibleRow + VisibleRowCount)
				return;

			Rectangle rect_row = new Rectangle ();

			rect_row.X = cells_area.X;
			rect_row.Width = width_of_all_columns;
			if (rect_row.Width > cells_area.Width)
				rect_row.Width = cells_area.Width;
			rect_row.Height = rows[row].Height;
			rect_row.Y = cells_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
			Invalidate (rect_row);
		}

		void InvalidateRowHeader (int row)
		{
			Rectangle rect_rowhdr = new Rectangle ();
			rect_rowhdr.X = row_headers_area.X;
			rect_rowhdr.Width = row_headers_area.Width;
			rect_rowhdr.Height = rows[row].Height;
			rect_rowhdr.Y = row_headers_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
			Invalidate (rect_rowhdr);
		}

		internal void InvalidateColumn (DataGridColumnStyle column)
		{
			Rectangle rect_col = new Rectangle ();
			int col_pixel;
			int col = -1;

			col = CurrentTableStyle.GridColumnStyles.IndexOf (column);

			if (col == -1)
				return;

			rect_col.Width = column.Width;
			col_pixel = GetColumnStartingPixel (col);
			rect_col.X = cells_area.X + col_pixel - horiz_pixeloffset;
			rect_col.Y = cells_area.Y;
			rect_col.Height = cells_area.Height;
			Invalidate (rect_col);
		}

		void DrawResizeLineVert (int x)
		{
			XplatUI.DrawReversibleRectangle (Handle,
							 new Rectangle (x, cells_area.Y, 1, cells_area.Height - 3),
							 2);
		}

		void DrawResizeLineHoriz (int y)
		{
			XplatUI.DrawReversibleRectangle (Handle,
							 new Rectangle (cells_area.X, y, cells_area.Width - 3, 1),
							 2);
		}

		void SetUpHorizontalScrollBar (out int maximum)
		{
			maximum = width_of_all_columns;

			horiz_scrollbar.Location = new Point (ClientRectangle.X, ClientRectangle.Y +
				ClientRectangle.Height - horiz_scrollbar.Height);

			horiz_scrollbar.LargeChange = cells_area.Width;
		}

		void SetUpVerticalScrollBar (out int height, out int maximum)
		{
			int y;
			
			y = ClientRectangle.Y + parent_rows.Y + parent_rows.Height;
			height = ClientRectangle.Height - parent_rows.Y - parent_rows.Height;

			vert_scrollbar.Location = new Point (ClientRectangle.X +
							     ClientRectangle.Width - vert_scrollbar.Width, y);

			maximum = RowsCount;
			
			if (ShowEditRow && RowsCount > 0) {
				maximum++;
			}
			
			vert_scrollbar.LargeChange = VLargeChange;
		}

		#endregion // Public Instance Methods

		#region Instance Properties
		// Returns the ColumnHeaders area excluding the rectangle shared with RowHeaders
		internal Rectangle ColumnHeadersArea {
			get {
				Rectangle columns_area = column_headers_area;

				if (CurrentTableStyle.CurrentRowHeadersVisible) {
					columns_area.X += RowHeaderWidth;
					columns_area.Width -= RowHeaderWidth;
				}
				return columns_area;
			}
		}

		internal Rectangle RowHeadersArea {
			get { return row_headers_area; }
		}

		internal Rectangle ParentRowsArea {
			get { return parent_rows; }
		}

		int VLargeChange {
			get { 
				return MaxVisibleRowCount;
			}
		}

		#endregion Instance Properties

		#endregion // Code originally in DataGridDrawingLogic.cs

		#region UIA Framework: Methods, Properties and Events
		
		static object UIACollectionChangedEvent = new object ();
		static object UIASelectionChangedEvent = new object ();
		static object UIAColumnHeadersVisibleChangedEvent = new object ();
		static object UIAGridCellChangedEvent = new object ();

		internal ScrollBar UIAHScrollBar {
			get { return horiz_scrollbar; }
		}

		internal ScrollBar UIAVScrollBar {
			get { return vert_scrollbar; }
		}

		internal DataGridTableStyle UIACurrentTableStyle {
			get { return current_style; }
		}

		internal int UIASelectedRows {
			get { return selected_rows.Count; }
		}

		internal Rectangle UIAColumnHeadersArea {
			get { return ColumnHeadersArea; }
		}

		internal Rectangle UIACaptionArea {
			get { return caption_area; }
		}

		internal Rectangle UIACellsArea {
			get { return cells_area; }
		}

		internal int UIARowHeight {
			get { return RowHeight; }
		}

		internal event CollectionChangeEventHandler UIACollectionChanged {
			add { Events.AddHandler (UIACollectionChangedEvent, value); }
			remove { Events.RemoveHandler (UIACollectionChangedEvent, value); }
		}

		internal event CollectionChangeEventHandler UIASelectionChanged {
			add { Events.AddHandler (UIASelectionChangedEvent, value); }
			remove { Events.RemoveHandler (UIASelectionChangedEvent, value); }
		}

		internal event EventHandler UIAColumnHeadersVisibleChanged {
			add { Events.AddHandler (UIAColumnHeadersVisibleChangedEvent, value); }
			remove { Events.RemoveHandler (UIAColumnHeadersVisibleChangedEvent, value); }
		}

		internal event CollectionChangeEventHandler UIAGridCellChanged {
			add { Events.AddHandler (UIAGridCellChangedEvent, value); }
			remove { Events.RemoveHandler (UIAGridCellChangedEvent, value); }
		}

		internal void OnUIACollectionChangedEvent (CollectionChangeEventArgs args)
		{
			CollectionChangeEventHandler eh
				= (CollectionChangeEventHandler) Events [UIACollectionChangedEvent];
			if (eh != null)
				eh (this, args);
		}

		internal void OnUIASelectionChangedEvent (CollectionChangeEventArgs args)
		{
			CollectionChangeEventHandler eh
				= (CollectionChangeEventHandler) Events [UIASelectionChangedEvent];
			if (eh != null)
				eh (this, args);
		}

		internal void OnUIAColumnHeadersVisibleChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIAColumnHeadersVisibleChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		internal void OnUIAGridCellChanged (CollectionChangeEventArgs args)
		{
			CollectionChangeEventHandler eh
				= (CollectionChangeEventHandler) Events [UIAGridCellChangedEvent];
			if (eh != null)
				eh (this, args);
		}

		#endregion // UIA Framework: Methods, Properties and Events

	}
}
