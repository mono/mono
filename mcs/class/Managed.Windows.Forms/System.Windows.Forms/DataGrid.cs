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
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
//

// NOT COMPLETE


using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;

namespace System.Windows.Forms
{
	internal struct DataGridRow {
		public bool IsSelected;
		public bool IsExpanded;
		public int VerticalOffset;
		public int Height;
	}

	[DefaultEvent("Navigate")]
	[DefaultProperty("DataSource")]
	[Designer("System.Windows.Forms.Design.DataGridDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class DataGrid : Control, ISupportInitialize, IDataGridEditingService
	{
		[Flags]
		[Serializable]
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

			#region	Local Variables
			internal int column;
			internal int row;
			internal HitTestType type;
			#endregion // Local Variables

			#region Private Constructors
			internal HitTestInfo ()
			{
				column = -1;
				row = -1;
				type =  HitTestType.None;
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

			public override bool Equals (object o)
			{
				if (!(o is HitTestInfo))
					return false;

				HitTestInfo obj = (HitTestInfo) o;
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
		private static readonly Color	def_alternating_backcolor = ThemeEngine.Current.DataGridAlternatingBackColor;
		private static readonly Color	def_background_color = ThemeEngine.Current.DataGridBackgroundColor;
		private static readonly Color	def_caption_backcolor = ThemeEngine.Current.DataGridCaptionBackColor;
		private static readonly Color	def_caption_forecolor = ThemeEngine.Current.DataGridCaptionForeColor;
		private static readonly Color	def_gridline_color = ThemeEngine.Current.DataGridGridLineColor;
		private static readonly Color	def_header_backcolor = ThemeEngine.Current.DataGridHeaderBackColor;
		private static readonly Font	def_header_font = ThemeEngine.Current.DefaultFont;
		private static readonly Color	def_header_forecolor = ThemeEngine.Current.DataGridHeaderForeColor;
		private static readonly Color	def_link_hovercolor = ThemeEngine.Current.DataGridLinkHoverColor;
		private static readonly Color	def_parentrowsback_color = ThemeEngine.Current.DataGridParentRowsBackColor;
		private static readonly Color	def_parentrowsfore_color = ThemeEngine.Current.DataGridParentRowsForeColor;
		private static readonly Color	def_selection_backcolor = ThemeEngine.Current.DataGridSelectionBackColor;
		private static readonly Color	def_selection_forecolor = ThemeEngine.Current.DataGridSelectionForeColor;
		private static readonly Color	def_link_color = ThemeEngine.Current.DataGridLinkColor;
		internal readonly int def_preferredrow_height;

		internal bool allow_navigation;
		private bool allow_sorting;
		private Color alternating_backcolor;
		private Color backColor;
		private Color background_color;
		private Color caption_backcolor;
		private Font caption_font;
		private Color caption_forecolor;
		private string caption_text;
		internal bool caption_visible;
		internal bool columnheaders_visible;
		private object datasource;
		private string datamember;
		private int firstvisible_column;
		private bool flatmode;
		private Color gridline_color;
		private DataGridLineStyle gridline_style;
		private Color header_backcolor;
		private Color header_forecolor;
		private Font header_font;
		private Color link_color;
		private Color link_hovercolor;
		private Color parentrowsback_color;
		private Color parentrowsfore_color;
		private bool parentrows_visible;
		private int preferredcolumn_width;
		private int preferredrow_height;
		private bool _readonly;
		internal bool rowheaders_visible;
		private Color selection_backcolor;
		private Color selection_forecolor;
		private int rowheaders_width;
		internal int visiblecolumn_count;
		internal int visiblerow_count;
		internal int first_visiblecolumn;
		private GridTableStylesCollection styles_collection;
		private DataGridParentRowsLabelStyle parentrowslabel_style;
		internal DataGridCell current_cell;
		private DataGridTableStyle default_style;
		private DataGridTableStyle current_style;
		internal HScrollBar horiz_scrollbar;
		internal VScrollBar vert_scrollbar;
		private int first_visiblerow;
		internal int horz_pixeloffset;
		bool is_adding;			// Indicates when we are adding a row
		bool is_editing;		// Current cell is edit mode
		internal bool is_changing;	// Indicates if current cell is been changed (in edit mode)
		private Hashtable selected_rows;
		private Hashtable expanded_rows;
		private int selection_start; // used for range selection
		private bool begininit;
		private CurrencyManager list_manager;
		private bool accept_listmgrevents;
		internal DataGridRow[] rows;

		bool column_resize_active;
		int resize_column_x;
		int resize_column_width_delta;
		int resize_column;
		
		bool row_resize_active;
		int resize_row_y;
		int resize_row_height_delta;
		int resize_row;

		bool from_positionchanged_handler;

		Stack memberHistory;

		#endregion // Local Variables

		#region Public Constructors
		public DataGrid ()
		{
			allow_navigation = true;
			allow_sorting = true;
			begininit = false;
			backColor = ThemeEngine.Current.DataGridBackColor;
			alternating_backcolor = def_alternating_backcolor;
			background_color = def_background_color;
			border_style = BorderStyle.Fixed3D;
			caption_backcolor = def_caption_backcolor;
			caption_font = null;
			caption_forecolor = def_caption_forecolor;
			caption_text = string.Empty;
			caption_visible = true;
			columnheaders_visible = true;
			datasource = null;
			datamember = string.Empty;
			firstvisible_column = 0;
			flatmode = false;
			gridline_color = def_gridline_color;
			gridline_style = DataGridLineStyle.Solid;
			header_backcolor = def_header_backcolor;
			header_forecolor = def_header_forecolor;
			header_font = def_header_font;
			link_color = def_link_color;
			link_hovercolor = def_link_hovercolor;
			parentrowsback_color = def_parentrowsback_color;
			parentrowsfore_color = def_parentrowsfore_color;
			parentrows_visible = true;
			preferredcolumn_width = ThemeEngine.Current.DataGridPreferredColumnWidth;
			_readonly = false;
			rowheaders_visible = true;
			selection_backcolor = def_selection_backcolor;
			selection_forecolor = def_selection_forecolor;
			rowheaders_width = 35;
			visiblecolumn_count = 0;
			visiblerow_count = 0;
			current_cell = new DataGridCell ();
			first_visiblerow = 0;
			first_visiblecolumn = 0;
			horz_pixeloffset = 0;
			is_editing = false;
			is_changing = false;
			is_adding = false;
			parentrowslabel_style = DataGridParentRowsLabelStyle.Both;
			selected_rows = new Hashtable ();
			selection_start = -1;
			expanded_rows = new Hashtable ();
			preferredrow_height = def_preferredrow_height = FontHeight + 3;
			list_manager = null;
			rows = new DataGridRow[0];
			accept_listmgrevents = true;

			default_style = new DataGridTableStyle (true);
			styles_collection = new GridTableStylesCollection (this);
			styles_collection.CollectionChanged += new CollectionChangeEventHandler (OnTableStylesCollectionChanged);

			CurrentTableStyle = default_style;

			horiz_scrollbar = new ImplicitHScrollBar ();
			horiz_scrollbar.Scroll += new ScrollEventHandler (GridHScrolled);
			vert_scrollbar = new ImplicitVScrollBar ();
			vert_scrollbar.Scroll += new ScrollEventHandler (GridVScrolled);

			SetStyle (ControlStyles.UserMouse, true);

			memberHistory = new Stack ();
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
			get { return allow_sorting; }
			set {
				if (allow_sorting != value) {
					allow_sorting = value;
				}
			}
		}

		public Color AlternatingBackColor  {
			get { return alternating_backcolor; }
			set {
				if (alternating_backcolor != value) {
					alternating_backcolor = value;
					InvalidateCells ();
				}
			}
		}

		public override Color BackColor {
			get { return backColor; }
			set {
				if (backColor != value) {
					backColor = value;
					InvalidateCells ();
				}
			}
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
				base.BackgroundImage = value;
			}
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
				if (caption_font == null) {
					return Font;
				}

				return caption_font;
			}
			set {
				if (caption_font != null && caption_font.Equals (value)) {
					return;
				}

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
					caption_visible = value;
					CalcAreasAndInvalidate ();
					OnCaptionVisibleChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		public bool ColumnHeadersVisible {
			get { return columnheaders_visible; }
			set {
				if (columnheaders_visible != value) {
					columnheaders_visible = value;
					CalcAreasAndInvalidate ();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DataGridCell CurrentCell {
			get { return current_cell; }
			set {
				int old_row = current_cell.RowNumber;

				if (current_cell.Equals (value))
					return;

				bool was_editing = is_editing;
				bool need_add = value.RowNumber >= RowsCount;

				if (need_add)
					value.RowNumber = RowsCount;


				accept_listmgrevents = false;

#if false
				if (was_editing) {
					EndEdit (CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber],
						 current_cell.RowNumber,
						 is_adding && !is_changing);

					if (value.RowNumber != old_row) {
						ListManager.EndCurrentEdit ();
					}
				}
#else
				if (value.RowNumber != old_row && is_adding && !is_changing) {
					is_adding = false;
					ListManager.CancelCurrentEdit ();
					UpdateVisibleRowCount ();
				}
#endif

				if (was_editing)
					CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].ConcedeFocus ();

				//Console.WriteLine ("set_CurrentCell, {0}x{1}, RowsCount = {2}, from {3}", value.RowNumber, value.ColumnNumber, RowsCount, Environment.StackTrace);
				if (need_add) {
					//Console.WriteLine ("+ calling AddNew");
					ListManager.AddNew ();
					is_adding = true;
				}

				if (value.ColumnNumber >= CurrentTableStyle.GridColumnStyles.Count) {
					value.ColumnNumber = CurrentTableStyle.GridColumnStyles.Count == 0 ? 0: CurrentTableStyle.GridColumnStyles.Count - 1;
				}
					
				EnsureCellVisibility (value);
				current_cell = value;					
			
				if (current_cell.RowNumber != old_row) {
					InvalidateRowHeader (old_row);
				}

				if (!from_positionchanged_handler && !is_adding)
					list_manager.Position = current_cell.RowNumber;

				if (current_cell.RowNumber != old_row) {
					InvalidateCurrentRowHeader ();
				}
				accept_listmgrevents = true;
				OnCurrentCellChanged (EventArgs.Empty);

				if (was_editing)
					EditCurrentCell ();
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
				if (ListManager == null) {
					return -1;
				}
				
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
				if (SetDataMember (value)) {					
					SetDataSource (datasource);
					SetNewDataSource ();
				}
			}
		}

		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, " + Consts.AssemblySystem_Design)]
		public object DataSource {
			get { return datasource; }
			set {
				SetDataMember ("");
				SetDataSource (value);
				SetNewDataSource ();					
			}
		}

		protected override Size DefaultSize {
			get { return new Size (130, 80); }
		}

		[Browsable(false)]
		public int FirstVisibleColumn {
			get { return firstvisible_column; }
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
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}

		public Color GridLineColor {
			get { return gridline_color; }
			set {
				if (value == Color.Empty) {
					throw new ArgumentException ("Color.Empty value is invalid.");
				}

				if (gridline_color != value) {
					gridline_color = value;
					Refresh ();
				}
			}
		}

		[DefaultValue(DataGridLineStyle.Solid)]
		public DataGridLineStyle GridLineStyle {
			get { return gridline_style; }
			set {
				if (gridline_style != value) {
					gridline_style = value;
					Refresh ();
				}
			}
		}

		public Color HeaderBackColor {
			get { return header_backcolor; }
			set {
				if (value == Color.Empty) {
					throw new ArgumentException ("Color.Empty value is invalid.");
				}

				if (header_backcolor != value) {
					header_backcolor = value;
					Refresh ();
				}
			}
		}

		public Font HeaderFont {
			get { return header_font; }
			set {
				if (header_font != null && !header_font.Equals (value)) {
					header_font = value;
					CalcAreasAndInvalidate ();
				}
			}
		}

		public Color HeaderForeColor {
			get { return header_forecolor; }
			set {
				if (header_forecolor != value) {
					header_forecolor = value;
					Refresh ();
				}
			}
		}

		protected ScrollBar HorizScrollBar {
			get { return horiz_scrollbar; }
		}

		public object this [DataGridCell cell] {
			get { return this [cell.RowNumber, cell.ColumnNumber]; }
			set { this [cell.RowNumber, cell.ColumnNumber] = value; }
		}

		public object this [int rowIndex, int columnIndex] {
			get { return CurrentTableStyle.GridColumnStyles[columnIndex].GetColumnValueAtRow (ListManager,
													  rowIndex); }
			set { CurrentTableStyle.GridColumnStyles[columnIndex].SetColumnValueAtRow (ListManager,
												   rowIndex, value); }
		}

		public Color LinkColor {
			get { return link_color; }
			set {
				if (link_color != value) {
					link_color = value;
					Refresh ();
				}
			}
		}

		[ComVisible(false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Color LinkHoverColor {
			get { return link_hovercolor; }
			set {
				if (link_hovercolor != value) {
					link_hovercolor = value;
					Refresh ();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected internal CurrencyManager ListManager {
			get {
				if (BindingContext == null || DataSource  == null) {
					return null;
				}

				if (list_manager != null) {
					return list_manager;
				}

				list_manager = (CurrencyManager) BindingContext [datasource, DataMember];

				if (list_manager != null)
					ConnectListManagerEvents ();

				rows = new DataGridRow [list_manager.Count + 1];
				for (int i = 0; i < rows.Length; i ++) {
					rows[i].Height = RowHeight;
					if (i > 0)
						rows[i].VerticalOffset = rows[i-1].VerticalOffset + rows[i-1].Height;
				}
				return list_manager;
			}
			set { throw new NotSupportedException ("Operation is not supported."); }
		}

		public Color ParentRowsBackColor {
			get { return parentrowsback_color; }
			set {
				if (parentrowsback_color != value) {
					parentrowsback_color = value;
					if (parentrows_visible) {
						Refresh ();
					}
				}
			}
		}

		public Color ParentRowsForeColor {
			get { return parentrowsfore_color; }
			set {
				if (parentrowsfore_color != value) {
					parentrowsfore_color = value;
					if (parentrows_visible) {
						Refresh ();
					}
				}
			}
		}

		[DefaultValue(DataGridParentRowsLabelStyle.Both)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DataGridParentRowsLabelStyle ParentRowsLabelStyle {
			get { return parentrowslabel_style; }
			set {
				if (parentrowslabel_style != value) {
					parentrowslabel_style = value;
					if (parentrows_visible) {
						Refresh ();
					}

					OnParentRowsLabelStyleChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		public bool ParentRowsVisible {
			get { return parentrows_visible; }
			set {
				if (parentrows_visible != value) {
					parentrows_visible = value;
					CalcAreasAndInvalidate ();
					OnParentRowsVisibleChanged (EventArgs.Empty);
				}
			}
		}

		// Settting this property seems to have no effect.
		[DefaultValue(75)]
		[TypeConverter(typeof(DataGridPreferredColumnWidthTypeConverter))]
		public int PreferredColumnWidth {
			get { return preferredcolumn_width; }
			set {
				if (value < 0) {
					throw new ArgumentException ("PreferredColumnWidth is less than 0");
				}

				if (preferredcolumn_width != value) {
					preferredcolumn_width = value;
					Refresh ();
				}
			}
		}

		public int PreferredRowHeight {
			get { return preferredrow_height; }
			set {
				if (preferredrow_height != value) {
					preferredrow_height = value;
					CalcAreasAndInvalidate ();
				}
			}
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
			get { return rowheaders_visible; }
			set {
				if (rowheaders_visible != value) {
					rowheaders_visible = value;
					CalcAreasAndInvalidate ();
				}
			}
		}

		[DefaultValue(35)]
		public int RowHeaderWidth {
			get { return rowheaders_width; }
			set {
				if (rowheaders_width != value) {
					rowheaders_width = value;
					CalcAreasAndInvalidate ();
				}
			}
		}

		public Color SelectionBackColor {
			get { return selection_backcolor; }
			set {
				if (selection_backcolor != value) {
					selection_backcolor = value;
					InvalidateSelection ();
				}
			}
		}

		public Color SelectionForeColor  {
			get { return selection_forecolor; }
			set {
				if (selection_forecolor != value) {
					selection_forecolor = value;
					InvalidateSelection ();
				}
			}
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

		[Browsable(false)]
		public int VisibleColumnCount {
			get { return visiblecolumn_count; }
		}

		// Calculated at DataGridDrawing.CalcRowHeaders
		[Browsable(false)]
		public int VisibleRowCount {
			get { return visiblerow_count; }
		}

		#endregion	// Public Instance Properties

		#region Private Instance Properties
		internal DataGridTableStyle CurrentTableStyle {
			get { return current_style; }
			set {
				current_style = value;
				current_style.DataGrid = this;
				CalcAreasAndInvalidate ();
			}
		}

		internal int FirstVisibleRow {
			get { return first_visiblerow; }
			set { first_visiblerow = value;}
		}
		
		internal int RowsCount {
			get {				
				if (ListManager != null) {
					return ListManager.Count;					
				}

				return 0;
			}
		}

		internal int RowHeight {
			get {
				if (CurrentTableStyle.CurrentPreferredRowHeight > Font.Height + 3 + 1 /* line */) {
					return CurrentTableStyle.CurrentPreferredRowHeight;

				} else {
					return Font.Height + 3 + 1 /* line */;
				}
			}
		}
		
		internal bool ShowEditRow {
			get {
				if (ListManager != null && ListManager.CanAddRows == false) {
					return false;
				}
								
				return _readonly == false;
			}
		}
		
		// It should only be shown if there are relations that
		// we do not support right now
		internal bool ShowParentRowsVisible {
			//See parentrows_visible;
			get { return false; }
		}
		
		#endregion Private Instance Properties

		#region Public Instance Methods

		[MonoTODO]
		public bool BeginEdit (DataGridColumnStyle gridColumn, int rowNumber)
		{
			if (is_changing)
				return false;

			int column = CurrentTableStyle.GridColumnStyles.IndexOf (gridColumn);
			if (column < 0)
				return false;

			CurrentCell = new DataGridCell (rowNumber, column);

			/* force editing of CurrentCell if we aren't already editing */
			EditCurrentCell ();

			return true;
		}

		public void BeginInit ()
		{
			begininit = true;
		}

		protected virtual void CancelEditing ()
		{
			if (!is_editing)
				return;

			CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].ConcedeFocus ();

			if (is_changing) {
				if (current_cell.ColumnNumber < CurrentTableStyle.GridColumnStyles.Count)
					CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].Abort (current_cell.RowNumber);
				is_changing = false;
				InvalidateRowHeader (current_cell.RowNumber);
			}

			if (is_adding) {
				ListManager.CancelCurrentEdit ();
				is_adding = false;
			}

			is_editing = false;

			//Invalidate ();
		}

		[MonoTODO]
		public void Collapse (int row)
		{
			if (!expanded_rows.ContainsKey (row))
				return;

			expanded_rows.Remove (row);
			/* XX need to redraw from @row down */
			CalcAreasAndInvalidate ();			
		}

		protected internal virtual void ColumnStartedEditing (Control editingControl)
		{
			bool need_invalidate = is_changing == false;
			// XXX calculate the row header to invalidate
			// (using the editingControl's position?)
			// instead of using InvalidateCurrentRowHeader
			is_changing = true;
			if (need_invalidate)
				InvalidateCurrentRowHeader ();
		}

		protected internal virtual void ColumnStartedEditing (Rectangle bounds)
		{
			bool need_invalidate = is_changing == false;
			// XXX calculate the row header to invalidate
			// instead of using InvalidateCurrentRowHeader
			is_changing = true;
			if (need_invalidate)
				InvalidateCurrentRowHeader ();
		}

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return base.CreateAccessibilityInstance ();
		}

		protected virtual DataGridColumnStyle CreateGridColumn (PropertyDescriptor prop)
		{
			return CreateGridColumn (prop, false);
		}

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
			if (!is_editing && !is_changing)
				return true;

			if (is_adding) {
				if (shouldAbort)
					ListManager.CancelCurrentEdit ();
				else
					CalcAreasAndInvalidate ();
				is_adding = false;
			}

			if (shouldAbort || gridColumn.ParentReadOnly)
				gridColumn.Abort (rowNumber);
			else
				gridColumn.Commit (ListManager, rowNumber);

			is_editing = false;
			is_changing = false;
			InvalidateCurrentRowHeader ();
			return true;
		}

		public void EndInit ()
		{
			begininit = false;
		}

		public void Expand (int row)
		{
			if (expanded_rows.ContainsKey (row))
				return;

			expanded_rows[row] = true;
			/* XX need to redraw from @row down */
			CalcAreasAndInvalidate ();			
		}

		public Rectangle GetCellBounds (DataGridCell cell)
		{
			return GetCellBounds (cell.RowNumber, cell.ColumnNumber);
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
			if (se.NewValue == horz_pixeloffset ||
			    se.Type == ScrollEventType.EndScroll) {
				return;
			}

			ScrollToColumnInPixels (se.NewValue);
		}

		protected virtual void GridVScrolled (object sender, ScrollEventArgs se)
		{
			int old_first_visiblerow = first_visiblerow;
			first_visiblerow = se.NewValue;

			if (first_visiblerow == old_first_visiblerow)
				return;

			UpdateVisibleRowCount ();

			if (first_visiblerow == old_first_visiblerow)
				return;
			
			ScrollToRow (old_first_visiblerow, first_visiblerow);
		}

		public HitTestInfo HitTest (Point position)
		{
			return HitTest (position.X, position.Y);
		}

		public bool IsExpanded (int rowNumber)
		{
			return expanded_rows[rowNumber] != null && (bool)expanded_rows[rowNumber] == true;
		}

		public bool IsSelected (int row)
		{
			return selected_rows[row] != null;
		}

		[MonoTODO]
		public void NavigateBack ()
		{
			if (memberHistory.Count == 0)
				return;

			DataMember = (string)memberHistory.Pop ();
		}

		[MonoTODO]
		public void NavigateTo (int rowNumber, string relationName)
		{
			if (allow_navigation == false)
				return;
			
			memberHistory.Push (DataMember);

			DataMember = String.Format ("{0}.{1}", DataMember, relationName);
		}

		protected virtual void OnAllowNavigationChanged (EventArgs e)
		{
			if (AllowNavigationChanged != null) {
				AllowNavigationChanged (this, e);
			}
		}

		protected void OnBackButtonClicked (object sender,  EventArgs e)
		{
			if (BackButtonClick != null) {
				BackButtonClick (sender, e);
			}
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected virtual void OnBackgroundColorChanged (EventArgs e)
		{
			if (BackgroundColorChanged != null) {
				BackgroundColorChanged (this, e);
			}
		}

		protected override void OnBindingContextChanged (EventArgs e)
		{
			base.OnBindingContextChanged (e);

			current_style.CreateColumnsForTable (false);
			CalcAreasAndInvalidate ();
		}

		protected virtual void OnBorderStyleChanged (EventArgs e)
		{
			if (BorderStyleChanged != null) {
				BorderStyleChanged (this, e);
			}
		}

		protected virtual void OnCaptionVisibleChanged (EventArgs e)
		{
			if (CaptionVisibleChanged != null) {
				CaptionVisibleChanged (this, e);
			}
		}

		protected virtual void OnCurrentCellChanged (EventArgs e)
		{
			if (CurrentCellChanged != null) {
				CurrentCellChanged (this, e);
			}
		}

		protected virtual void OnDataSourceChanged (EventArgs e)
		{
			if (DataSourceChanged != null) {
				DataSourceChanged (this, e);
			}
		}

		protected override void OnEnter (EventArgs e)
		{
			base.OnEnter (e);
		}

		protected virtual void OnFlatModeChanged (EventArgs e)
		{
			if (FlatModeChanged != null) {
				FlatModeChanged (this, e);
			}
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
			CalcGridAreas ();
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnKeyDown (KeyEventArgs ke)
		{
			base.OnKeyDown (ke);
			
			if (ProcessGridKey (ke) == true) {
				ke.Handled = true;
			}

			/* TODO: we probably don't need this check,
			 * since current_cell wouldn't have been set
			 * to something invalid */
			if (CurrentTableStyle.GridColumnStyles.Count > 0) {
				CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].OnKeyDown
					(ke, current_cell.RowNumber, current_cell.ColumnNumber);
			}
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

#if false
			/* we get an OnLeave call when the
			 * DataGridTextBox control is focused, so we
			 * need to ignore that.  If we get an OnLeave
			 * call when a child control is not receiving
			 * focus, we need to cancel the current
			 * edit. */
			if (is_adding) {
				ListManager.CancelCurrentEdit ();
				is_adding = false;
			}
#endif
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown (e);

			bool ctrl_pressed = ((Control.ModifierKeys & Keys.Control) != 0);
			bool shift_pressed = ((Control.ModifierKeys & Keys.Shift) != 0);

			HitTestInfo testinfo;
			testinfo = HitTest (e.X, e.Y);

			switch (testinfo.type) {
			case HitTestType.Cell:
			{
				if (testinfo.Row < 0 || testinfo.Column < 0)
					break;
					
				DataGridCell new_cell = new DataGridCell (testinfo.Row, testinfo.Column);

				if ((new_cell.Equals (current_cell) == false) || (!is_editing)) {
					CurrentCell = new_cell;
					EditCurrentCell ();
				} else {
					CurrentTableStyle.GridColumnStyles[testinfo.Column].OnMouseDown (e, testinfo.Row, testinfo.Column);
				}

				break;
			}
			case HitTestType.RowHeader:
			{
				bool expansion_click = false;
				if (CurrentTableStyle.HasRelations) {
					if (e.X > rowhdrs_area.X + rowhdrs_area.Width / 2) {
						/* it's in the +/- space */
						if (IsExpanded (testinfo.Row))
							Collapse (testinfo.Row);
						else
							Expand (testinfo.Row);

						expansion_click = true;
					}
				}

				if (!ctrl_pressed &&
				    !shift_pressed &&
				    !expansion_click) {
					ResetSelection (); // Invalidates selected rows
				}

				if ((shift_pressed ||
				     expansion_click)
				    && selection_start != -1) {
					ShiftSelection (testinfo.Row);
				} else { // ctrl_pressed or single item
					selection_start = testinfo.Row;
					Select (testinfo.Row);
				}

				CancelEditing ();
				CurrentRow = testinfo.Row;
				OnRowHeaderClick (EventArgs.Empty);

				break;
			}

			case HitTestType.ColumnHeader:
			{
				if (CurrentTableStyle.GridColumnStyles.Count == 0)
					break;

				if (allow_sorting == false)
					break;

				if (ListManager.List is IBindingList == false)
					break;
			
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
				break;
			}

			case HitTestType.ColumnResize:
			{
				resize_column = testinfo.Column;
				column_resize_active = true;
				resize_column_x = e.X;
				resize_column_width_delta = 0;
				DrawResizeLineVert (resize_column_x);
				break;
			}

			case HitTestType.RowResize:
			{
				resize_row = testinfo.Row;
				row_resize_active = true;
				resize_row_y = e.Y;
				resize_row_height_delta = 0;
				DrawResizeLineHoriz (resize_row_y);
				break;
			}

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
				HitTestInfo testinfo;
				testinfo = HitTest (e.X, e.Y);

				switch (testinfo.type) {
				case HitTestType.ColumnResize:
					Cursor = Cursors.VSplit;
					break;
				case HitTestType.RowResize:
					Cursor = Cursors.HSplit;
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
				int new_width = CurrentTableStyle.GridColumnStyles[resize_column].Width + resize_column_width_delta;
				if (new_width < 0)
					new_width = 0;
				CurrentTableStyle.GridColumnStyles[resize_column].Width = new_width;
				Invalidate ();
			}
			else if (row_resize_active) {
				row_resize_active = false;

				if (resize_row_height_delta + rows[resize_row].Height < 0)
					resize_row_height_delta = -rows[resize_row].Height;

				rows[resize_row].Height = rows[resize_row].Height + resize_row_height_delta;
				for (int i = resize_row + 1; i < rows.Length; i ++)
					rows[i].VerticalOffset += resize_row_height_delta;

				CalcAreasAndInvalidate ();
			}
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel (e);

			bool ctrl_pressed = ((Control.ModifierKeys & Keys.Control) != 0);
			int pixels;

			if (ctrl_pressed) { // scroll horizontally
				if (e.Delta > 0) {
					/* left */
					pixels = Math.Max (horiz_scrollbar.Minimum,
							   horiz_scrollbar.Value - horiz_scrollbar.LargeChange);
				}
				else {
					/* right */
					pixels = Math.Min (horiz_scrollbar.Maximum - horiz_scrollbar.LargeChange + 1,
							   horiz_scrollbar.Value + horiz_scrollbar.LargeChange);
				}

				GridHScrolled (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, pixels));
				horiz_scrollbar.Value = pixels;
			} else {
				if (e.Delta > 0) {
					/* up */
					pixels = Math.Max (vert_scrollbar.Minimum,
							   vert_scrollbar.Value - vert_scrollbar.LargeChange);
				}
				else {
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
			if (Navigate != null) {
				Navigate (this, e);
			}
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
			if (ParentRowsLabelStyleChanged != null) {
				ParentRowsLabelStyleChanged (this, e);
			}
		}

		protected virtual void OnParentRowsVisibleChanged (EventArgs e)
		{
			if (ParentRowsVisibleChanged != null) {
				ParentRowsVisibleChanged (this, e);
			}
		}

		protected virtual void OnReadOnlyChanged (EventArgs e)
		{
			if (ReadOnlyChanged != null) {
				ReadOnlyChanged (this, e);
			}
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}

		protected void OnRowHeaderClick (EventArgs e)
		{
			if (RowHeaderClick != null) {
				RowHeaderClick (this, e);
			}
		}

		protected void OnScroll (EventArgs e)
		{
			if (Scroll != null) {
				Scroll (this, e);
			}
		}

		protected void OnShowParentDetailsButtonClicked (object sender, EventArgs e)
		{
			if (ShowParentDetailsButtonClick != null) {
				ShowParentDetailsButtonClick (sender, e);
			}
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
			}
			else {
				ResetSelection ();
				if (!is_editing)
					EditCurrentCell ();
			}
		}

		protected bool ProcessGridKey (KeyEventArgs ke)
		{
			if (RowsCount == 0) {
				return false;
			}

			bool ctrl_pressed = ((ke.Modifiers & Keys.Control) != 0);
			bool alt_pressed = ((ke.Modifiers & Keys.Alt) != 0);
			bool shift_pressed = ((ke.Modifiers & Keys.Shift) != 0);

			switch (ke.KeyCode) {
			case Keys.Escape:
				CancelEditing ();
				return true;
				
			case Keys.D0:
				if (alt_pressed) {
					if (is_editing)
						CurrentTableStyle.GridColumnStyles[CurrentColumn].EnterNullValue ();
				}
				return true;

			case Keys.Enter:
				CurrentRow ++;
				if (!is_editing)
					EditCurrentCell ();
				return true;

			case Keys.Tab:
				if (CurrentColumn < CurrentTableStyle.GridColumnStyles.Count - 1)
					CurrentColumn ++;
				else if ((CurrentRow <= RowsCount - 1) && (CurrentColumn == CurrentTableStyle.GridColumnStyles.Count - 1))
					CurrentCell = new DataGridCell (CurrentRow + 1, 0);

				UpdateSelectionAfterCursorMove (false);

				return true;

			case Keys.Right:
				if (ctrl_pressed) {
					CurrentColumn = CurrentTableStyle.GridColumnStyles.Count - 1;
				}
				else {
					if (CurrentColumn < CurrentTableStyle.GridColumnStyles.Count - 1) {
						CurrentColumn ++;
					} else if (CurrentRow < RowsCount - 1
						   || (CurrentRow == RowsCount - 1
						       && !is_adding)) {
						CurrentCell = new DataGridCell (CurrentRow + 1, 0);
					}
				}

				UpdateSelectionAfterCursorMove (false);

				return true;

			case Keys.Left:
				if (ctrl_pressed) {
					CurrentColumn = 0;
				}
				else {
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
				else if (CurrentRow == RowsCount - 1 && !is_adding && !shift_pressed)
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
				foreach (int row in selected_rows.Keys) {
					ListManager.RemoveAt (row);						
				}
				selected_rows.Clear ();
				CalcAreasAndInvalidate ();

				return true;
			}

			return false; // message not processed
		}

		protected override bool ProcessKeyPreview (ref Message m)
		{
			if ((Msg)m.Msg == Msg.WM_KEYDOWN) {
				Keys key = (Keys) m.WParam.ToInt32 ();
				KeyEventArgs ke = new KeyEventArgs (key);
				if (ProcessGridKey (ke) == true) {
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
			alternating_backcolor = def_alternating_backcolor;
		}

		public override void ResetBackColor ()
		{
			backColor = ThemeEngine.Current.DataGridBackColor;
		}

		public override void ResetForeColor ()
		{
			base.ResetForeColor ();
		}

		public void ResetGridLineColor ()
		{
			gridline_color = def_gridline_color;
		}

		public void ResetHeaderBackColor ()
		{
			header_backcolor = def_header_backcolor;
		}

		public void ResetHeaderFont ()
		{
			header_font = def_header_font;
		}

		public void ResetHeaderForeColor ()
		{
			header_forecolor = def_header_forecolor;
		}

		public void ResetLinkColor ()
		{
			link_color = def_link_color;
		}

		public void ResetLinkHoverColor ()
		{
			link_hovercolor = def_link_hovercolor;
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
				InvalidateRow (row);
				InvalidateRowHeader (row);
			}
		}

		public void ResetSelectionBackColor ()
		{
			selection_backcolor = def_selection_backcolor;
		}

		public void ResetSelectionForeColor ()
		{
			selection_forecolor = def_selection_forecolor;
		}

		public void Select (int row)
		{
			if (selected_rows.Count == 0)
				selection_start = row;

			if (selected_rows[row] == null) {
				selected_rows.Add (row, true);
			} else {
				selected_rows[row] = true;
			}

			InvalidateRow (row);
		}

		public void SetDataBinding (object dataSource, string dataMember)
		{
			this.datamember = string.Empty;
			SetDataSource (dataSource);
			SetDataMember (dataMember);		
			SetNewDataSource ();
		}

		protected virtual bool ShouldSerializeAlternatingBackColor ()
		{
			return (alternating_backcolor != def_alternating_backcolor);
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
			return (caption_forecolor != def_caption_forecolor);
		}

		protected virtual bool ShouldSerializeGridLineColor ()
		{
			return (gridline_color != def_gridline_color);
		}

		protected virtual bool ShouldSerializeHeaderBackColor ()
		{
			return (header_backcolor != def_header_backcolor);
		}

		protected bool ShouldSerializeHeaderFont ()
		{
			return (header_font != def_header_font);
		}

		protected virtual bool ShouldSerializeHeaderForeColor ()
		{
			return (header_forecolor != def_header_forecolor);
		}

		protected virtual bool ShouldSerializeLinkHoverColor ()
		{
			return (link_hovercolor != def_link_hovercolor);
		}

		protected virtual bool ShouldSerializeParentRowsBackColor ()
		{
			return (parentrowsback_color != def_parentrowsback_color);
		}

		protected virtual bool ShouldSerializeParentRowsForeColor ()
		{
			return (parentrowsback_color != def_parentrowsback_color);
		}

		protected bool ShouldSerializePreferredRowHeight ()
		{
			return (preferredrow_height != def_preferredrow_height);
		}

		protected bool ShouldSerializeSelectionBackColor ()
		{
			return (selection_backcolor != def_selection_backcolor);
		}

		protected virtual bool ShouldSerializeSelectionForeColor ()
		{
			return (selection_forecolor != def_selection_forecolor);
		}

		public void SubObjectsSiteChange (bool site)
		{
		}

		public void UnSelect (int row)
		{
			selected_rows.Remove (row);
			InvalidateRow (row);
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
			list_manager.PositionChanged += new EventHandler (OnListManagerPositionChanged);
			list_manager.ItemChanged += new ItemChangedEventHandler (OnListManagerItemChanged);
		}
		
		private void DisconnectListManagerEvents ()
		{
			list_manager.PositionChanged -= new EventHandler (OnListManagerPositionChanged);
			list_manager.ItemChanged -= new ItemChangedEventHandler (OnListManagerItemChanged);
		}

		private void EnsureCellVisibility (DataGridCell cell)
		{
			if (cell.ColumnNumber <= first_visiblecolumn ||
				cell.ColumnNumber + 1 >= first_visiblecolumn + visiblecolumn_count) {			

				first_visiblecolumn = GetFirstColumnForColumnVisilibility (first_visiblecolumn, cell.ColumnNumber);
                                int pixel = GetColumnStartingPixel (first_visiblecolumn);
				ScrollToColumnInPixels (pixel);
				horiz_scrollbar.Value = pixel;
				Update();
			}

			if (cell.RowNumber < first_visiblerow ||
			    cell.RowNumber + 1 >= first_visiblerow + visiblerow_count) {

                                if (cell.RowNumber + 1 >= first_visiblerow + visiblerow_count) {
					int old_first_visiblerow = first_visiblerow;
					first_visiblerow = 1 + cell.RowNumber - visiblerow_count;
					UpdateVisibleRowCount ();
					ScrollToRow (old_first_visiblerow, first_visiblerow);
				}else {
					int old_first_visiblerow = first_visiblerow;
					first_visiblerow = cell.RowNumber;
					UpdateVisibleRowCount ();
					ScrollToRow (old_first_visiblerow, first_visiblerow);
				}

				vert_scrollbar.Value = first_visiblerow;
			}
		}
		
		private void InvalidateCurrentRowHeader ()
		{
			InvalidateRowHeader (CurrentRow);
		}
		
		private bool SetDataMember (string member)
		{
			if (member == datamember) {
				return false;
			}

			datamember = member;

			if (list_manager != null) {
				DisconnectListManagerEvents ();
				list_manager = null;
			}

			return true;
		}

		private void SetDataSource (object source)
		{			
			if (source != null && source as IListSource != null && source as IList != null) {
				throw new Exception ("Wrong complex data binding source");
			}

			if (is_editing)
				CancelEditing ();

			current_cell = new DataGridCell ();
			datasource = source;
			if (list_manager != null) {
				DisconnectListManagerEvents ();
				list_manager = null;
			}

			OnDataSourceChanged (EventArgs.Empty);
		}

		private void SetNewDataSource ()
		{
			if (ListManager != null) {
				string list_name = ListManager.GetListName (null);
				if (TableStyles[list_name] == null) {
					current_style.GridColumnStyles.Clear ();			
					current_style.CreateColumnsForTable (false);
				}
				else if (CurrentTableStyle.MappingName != list_name) {
					// If the style has been defined by the user, use it
					CurrentTableStyle = styles_collection[list_name];
					current_style.CreateColumnsForTable (true);
				}
				else
					current_style.CreateColumnsForTable (false);
			}
			else
				current_style.CreateColumnsForTable (false);
			
			CalcAreasAndInvalidate ();			
		}

		private void OnListManagerPositionChanged (object sender, EventArgs e)
		{
			if (accept_listmgrevents == false)
				return;

			from_positionchanged_handler = true;
			CurrentRow = list_manager.Position;
			from_positionchanged_handler = false;
		}

		private void OnListManagerItemChanged (object sender, ItemChangedEventArgs e)
		{
			if (accept_listmgrevents == false)
				return;

			if (e.Index == -1)
				CalcAreasAndInvalidate ();
			else
				InvalidateRow (e.Index);
		}

		private void OnTableStylesCollectionChanged (object sender, CollectionChangeEventArgs e)
		{
			if (ListManager == null)
				return;
			
			string list_name = ListManager.GetListName (null);
			switch (e.Action){
				case CollectionChangeAction.Add: {
					if (e.Element != null && String.Compare (list_name, ((DataGridTableStyle)e.Element).MappingName, true) == 0) {
						CurrentTableStyle = (DataGridTableStyle)e.Element;
						((DataGridTableStyle) e.Element).CreateColumnsForTable (false);
					}
					break;
				}

				case CollectionChangeAction.Remove: {
					if (e.Element != null && String.Compare (list_name, ((DataGridTableStyle)e.Element).MappingName, true) == 0) {
						CurrentTableStyle = default_style;						
						current_style.GridColumnStyles.Clear ();
						current_style.CreateColumnsForTable (false);
					}
					break;
				}	

				
				case CollectionChangeAction.Refresh: {
					if (e.Element != null && String.Compare (list_name, ((DataGridTableStyle)e.Element).MappingName, true) == 0) {
						CurrentTableStyle = (DataGridTableStyle)e.Element;
						((DataGridTableStyle) e.Element).CreateColumnsForTable (false);
					} else {
						CurrentTableStyle = default_style;
						current_style.GridColumnStyles.Clear ();
						current_style.CreateColumnsForTable (false);

					}
					break;

				}
			}						
			CalcAreasAndInvalidate ();
		}

		private void EditCurrentCell ()
		{
			is_editing = true;

			CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].Edit (ListManager,
				current_cell.RowNumber, GetCellBounds (current_cell.RowNumber, current_cell.ColumnNumber),
				_readonly, string.Empty, true);
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
			}
			else {
				start = index;
				end = selection_start;
			}

			for (int idx = start; idx <= end; idx ++) {
				Select (idx);
			}
		}

		private void ScrollToColumnInPixels (int pixel)
		{
			int pixels;

			if (pixel > horz_pixeloffset) { // ScrollRight
				pixels = -1 * (pixel - horz_pixeloffset);
			}
			else {
				pixels = horz_pixeloffset - pixel;
			}

			Rectangle area = CellsArea;
				
			horz_pixeloffset = pixel;
			UpdateVisibleColumn ();

			if (columnheaders_visible == true) {
				area.Y -= ColumnHeadersArea.Height;
				area.Height += ColumnHeadersArea.Height;
			}

			XplatUI.ScrollWindow (Handle, area, pixels, 0, false);
		}

		private void ScrollToRow (int old_row, int new_row)
		{
			int pixels = 0;
			int i;

			if (new_row > old_row) { // Scrolldown
				for (i = old_row; i < new_row; i ++)
					pixels -= rows[i].Height;
			}
			else {
				for (i = new_row; i < old_row; i ++)
					pixels += rows[i].Height;
			}

			Rectangle rows_area = CellsArea; // Cells area - partial rows space
			if (rowheaders_visible) {
				rows_area.X -= RowHeaderWidth;
				rows_area.Width += RowHeaderWidth;
			}

			rows_area.Height = CellsArea.Height - CellsArea.Height % RowHeight;

			XplatUI.ScrollWindow (Handle, rows_area, 0, pixels, false);
		}

		#endregion Private Instance Methods


		#region Events
		public event EventHandler AllowNavigationChanged;
		public event EventHandler BackButtonClick;
		public event EventHandler BackgroundColorChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
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

		public event EventHandler BorderStyleChanged;
		public event EventHandler CaptionVisibleChanged;
		public event EventHandler CurrentCellChanged;
		public event EventHandler DataSourceChanged;
		public event EventHandler FlatModeChanged;
		public event NavigateEventHandler Navigate;
		public event EventHandler ParentRowsLabelStyleChanged;
		public event EventHandler ParentRowsVisibleChanged;
		public event EventHandler ReadOnlyChanged;
		protected event EventHandler RowHeaderClick;
		public event EventHandler Scroll;
		public event EventHandler ShowParentDetailsButtonClick;

		#endregion	// Events




		#region Code originally in DataGridDrawingLogic.cs

		#region	Local Variables

		// Areas
		internal Rectangle caption_area;
		internal Rectangle parent_rows;
		internal Rectangle columnhdrs_area;	// Used columns header area
		internal int columnhdrs_maxwidth; 	// Total width (max width) for columns headrs
		internal Rectangle rowhdrs_area;	// Used Headers rows area
		internal int rowhdrs_maxheight; 	// Total height for rows (max height)
		internal Rectangle cells_area;
		internal Font font_newrow = new Font (FontFamily.GenericSansSerif, 16);
		#endregion // Local Variables


		#region Public Instance Methods

		// Calc the max with of all columns
		internal int CalcAllColumnsWidth ()
		{
			int width = 0;
			int cnt = CurrentTableStyle.GridColumnStyles.Count;

			for (int col = 0; col < cnt; col++) {
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
				return 0;
				
			if (CurrentTableStyle.CurrentRowHeadersVisible) {
				width += rowhdrs_area.X + rowhdrs_area.Width;
				column_x += rowhdrs_area.X + rowhdrs_area.Width;
			}

			for (int col = 0; col < cnt; col++) {
				width += CurrentTableStyle.GridColumnStyles[col].Width;

				if (pixel < width)
					return col;

				column_x += CurrentTableStyle.GridColumnStyles[col].Width;
			}

			return cnt - 1;
		}

		//
		internal int GetColumnStartingPixel (int my_col)
		{
			int width = 0;
			int cnt = CurrentTableStyle.GridColumnStyles.Count;

			for (int col = 0; col < cnt; col++) {

				if (my_col == col)
					return width;

				width += CurrentTableStyle.GridColumnStyles[col].Width;
			}

			return 0;
		}
		
		// Which column has to be the first visible column to ensure a column visibility
		internal int GetFirstColumnForColumnVisilibility (int current_first_visiblecolumn, int column)
		{
			int new_col = column;
			int width = 0;
			
			if (column > current_first_visiblecolumn) { // Going forward								
				for (new_col = column; new_col >= 0; new_col--){
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
		internal void CalcGridAreas ()
		{
			if (IsHandleCreated == false) // Delay calculations until the handle is created
				return;

			/* make sure we don't happen to end up in this method again */
			if (in_calc_grid_areas)
				return;

			in_calc_grid_areas = true;

			/* Order is important. E.g. row headers max. height depends on caption */
			horz_pixeloffset = 0;			
			CalcCaption ();
			CalcParentRows ();
			UpdateVisibleRowCount ();
			CalcRowHeaders (visiblerow_count);
			CalcColumnHeaders ();
			CalcCellsArea ();

			bool needHoriz = false;
			bool needVert = false;

			/* figure out which scrollbars we need, and what the visible areas are */
			int visible_cells_width = cells_area.Width;
			int visible_cells_height = cells_area.Height;
			int width_of_all_columns = CalcAllColumnsWidth ();
			int allrows = RowsCount;
			if (ShowEditRow && RowsCount > 0)
				allrows++;

			/* use a loop to iteratively calculate whether
			 * we need horiz/vert scrollbars. */
			for (int i = 0; i < 3; i ++) {
				if (needVert)
					visible_cells_width = cells_area.Width - vert_scrollbar.Width;
				if (needHoriz)
					visible_cells_height = cells_area.Height - horiz_scrollbar.Height;

				UpdateVisibleRowCount ();

				needHoriz = (width_of_all_columns > visible_cells_width);
				needVert = (visiblerow_count != allrows);
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
				if (ShowParentRowsVisible) {
					parent_rows.Width -= vert_scrollbar.Width;
				}

				if (!ShowingColumnHeaders) {
					if (columnhdrs_area.X + columnhdrs_area.Width > vert_scrollbar.Location.X) {
						columnhdrs_area.Width -= vert_scrollbar.Width;
					}
				}

				horiz_scrollbar_width -= vert_scrollbar.Width;
				vert_scrollbar_height -= horiz_scrollbar.Height;
			}

			if (needVert) {
				if (rowhdrs_area.Y + rowhdrs_area.Height > ClientRectangle.Y + ClientRectangle.Height) {
					rowhdrs_area.Height -= horiz_scrollbar.Height;
					rowhdrs_maxheight -= horiz_scrollbar.Height;
				}

				vert_scrollbar.Height = vert_scrollbar_height;
				vert_scrollbar.Maximum = vert_scrollbar_maximum;
				Controls.Add (vert_scrollbar);
				vert_scrollbar.Visible = true;
			}
			else {
				Controls.Remove (vert_scrollbar);
				vert_scrollbar.Visible = false;
			}

			if (needHoriz) {
				horiz_scrollbar.Width = horiz_scrollbar_width;
				horiz_scrollbar.Maximum = horiz_scrollbar_maximum;
				Controls.Add (horiz_scrollbar);
				horiz_scrollbar.Visible = true;
			}
			else {
				Controls.Remove (horiz_scrollbar);
				horiz_scrollbar.Visible = false;
			}

			UpdateVisibleColumn ();
			UpdateVisibleRowCount ();

			//Console.WriteLine ("DataGridDrawing.CalcGridAreas caption_area:{0}", caption_area);
			//Console.WriteLine ("DataGridDrawing.CalcGridAreas parent_rows:{0}", parent_rows);
			//Console.WriteLine ("DataGridDrawing.CalcGridAreas rowhdrs_area:{0}", rowhdrs_area);
			//Console.WriteLine ("DataGridDrawing.CalcGridAreas columnhdrs_area:{0}", columnhdrs_area);
			//Console.WriteLine ("DataGridDrawing.CalcGridAreas cells:{0}", cells_area);

			in_calc_grid_areas = false;
		}

		private void CalcCaption ()
		{
			if (caption_visible == false) {
				caption_area = Rectangle.Empty;
				return;
			}

			caption_area.X = ClientRectangle.X;
			caption_area.Y = ClientRectangle.Y;
			caption_area.Width = ClientRectangle.Width;
			caption_area.Height = CaptionFont.Height + 6;

			//Console.WriteLine ("DataGridDrawing.CalcCaption {0}", caption_area);
		}

		private void CalcCellsArea ()
		{
			if (caption_visible) {
				cells_area.Y = caption_area.Y + caption_area.Height;
			} else {
				cells_area.Y = ClientRectangle.Y;
			}

			if (ShowParentRowsVisible) {
				cells_area.Y += parent_rows.Height;
			}

			if (ShowingColumnHeaders) {
				cells_area.Y += columnhdrs_area.Height;
			}

			cells_area.X = ClientRectangle.X + rowhdrs_area.Width;
			cells_area.Width = ClientRectangle.X + ClientRectangle.Width - cells_area.X;
			cells_area.Height = ClientRectangle.Y + ClientRectangle.Height - cells_area.Y;

			//Console.WriteLine ("DataGridDrawing.CalcCellsArea {0}", cells_area);
		}

		private void CalcColumnHeaders ()
		{
			int width_all_cols, max_width_cols;
			
			if (!ShowingColumnHeaders) {
				columnhdrs_area = Rectangle.Empty;				
				return;
			}

			if (caption_visible) {
				columnhdrs_area.Y = caption_area.Y + caption_area.Height;
			} else {
				columnhdrs_area.Y = ClientRectangle.Y;
			}

			if (ShowParentRowsVisible) {
				columnhdrs_area.Y += parent_rows.Height;
			}

			columnhdrs_area.X = ClientRectangle.X;
			columnhdrs_area.Height = ColumnHeadersHeight;
			width_all_cols = CalcAllColumnsWidth ();

			// TODO: take into account Scrollbars
			columnhdrs_maxwidth = ClientRectangle.X + ClientRectangle.Width - columnhdrs_area.X;
			max_width_cols = columnhdrs_maxwidth;

			if (CurrentTableStyle.CurrentRowHeadersVisible) {
				max_width_cols -= RowHeaderWidth;
			}

			if (width_all_cols > max_width_cols) {
				columnhdrs_area.Width = columnhdrs_maxwidth;
			} else {
				columnhdrs_area.Width = width_all_cols;

				if (CurrentTableStyle.CurrentRowHeadersVisible) {
					columnhdrs_area.Width += RowHeaderWidth;
				}
			}

			//Console.WriteLine ("DataGridDrawing.CalcColumnHeaders {0}", columnhdrs_area);
		}

		private void CalcParentRows ()
		{
			if (ShowParentRowsVisible == false) {
				parent_rows = Rectangle.Empty;
				return;
			}

			if (caption_visible) {
				parent_rows.Y = caption_area.Y + caption_area.Height;

			} else {
				parent_rows.Y = ClientRectangle.Y;
			}

			parent_rows.X = ClientRectangle.X;
			parent_rows.Width = ClientRectangle.Width;
			parent_rows.Height = CaptionFont.Height + 3;

			//Console.WriteLine ("DataGridDrawing.CalcParentRows {0}", parent_rows);
		}

		private void CalcRowHeaders (int visiblerow_count)
		{
			if (CurrentTableStyle.CurrentRowHeadersVisible == false) {
				rowhdrs_area = Rectangle.Empty;
				return;
			}

			if (caption_visible) {
				rowhdrs_area.Y = caption_area.Y + caption_area.Height;
			} else {
				rowhdrs_area.Y = ClientRectangle.Y;
			}

			if (ShowParentRowsVisible) {
				rowhdrs_area.Y += parent_rows.Height;
			}

			if (ShowingColumnHeaders) { // first block is painted by ColumnHeader
				rowhdrs_area.Y += ColumnHeadersHeight;
			}

			rowhdrs_area.X = ClientRectangle.X;
			rowhdrs_area.Width = RowHeaderWidth;
			if (visiblerow_count == 0)
				rowhdrs_area.Height = 0;
			else
				rowhdrs_area.Height = (rows[visiblerow_count + FirstVisibleRow - 1].VerticalOffset - rows[FirstVisibleRow].VerticalOffset
						       + rows[visiblerow_count + FirstVisibleRow - 1].Height);
			rowhdrs_maxheight = ClientRectangle.Height + ClientRectangle.Y - rowhdrs_area.Y;

			//Console.WriteLine ("DataGridDrawing.CalcRowHeaders {0} {1}", rowhdrs_area,
			//	rowhdrs_maxheight);
		}

		private int GetVisibleRowCount (int visibleHeight)
		{
			//			Console.Write ("GetVisibleRowCount ({0}) - ", visibleHeight);
			int total_rows = RowsCount;
			
			if (ShowEditRow && RowsCount > 0) {
				total_rows++;
			}

			int rows_height = 0;
			int r;
			for (r = FirstVisibleRow; r < RowsCount; r ++) {
				//				Console.Write ("{0},", rows[r].Height);
				if (rows_height + rows[r].Height >= visibleHeight)
					break;
				rows_height += rows[r].Height;
			}

			/* add in the edit row if it'll fit */
			if (ShowEditRow && RowsCount > 0 && visibleHeight - rows_height > RowHeight)
				r ++;

			if (r < rows.Length - 1)
				r ++;
			//			Console.WriteLine (" rows_height = {0}, returning {1}", rows_height, r - FirstVisibleRow);

			return r - FirstVisibleRow;
		}

		internal void UpdateVisibleColumn ()
		{
			if (CurrentTableStyle.GridColumnStyles.Count == 0) {
				visiblecolumn_count = 0;
				return;	
			}
			
			int col;
			int max_pixel = horz_pixeloffset + cells_area.Width;
			int unused;

			first_visiblecolumn = FromPixelToColumn (horz_pixeloffset, out unused);

			col = FromPixelToColumn (max_pixel, out unused);
			
			visiblecolumn_count = 1 + col - first_visiblecolumn;
			
			if (first_visiblecolumn + visiblecolumn_count < CurrentTableStyle.GridColumnStyles.Count) { 
				visiblecolumn_count++; // Partially visible column
			}
		}

		internal void UpdateVisibleRowCount ()
		{
			visiblerow_count = GetVisibleRowCount (cells_area.Height);

			CalcRowHeaders (visiblerow_count); // Height depends on num of visible rows

			// XXX
			Invalidate ();
		}

		const int RESIZE_HANDLE_HORIZ_SIZE = 5;
		const int RESIZE_HANDLE_VERT_SIZE = 3;

		// From Point to Cell
		internal HitTestInfo HitTest (int x, int y)
		{
			HitTestInfo hit = new HitTestInfo ();

			if (columnhdrs_area.Contains (x, y)) {
				int offset_x = x + horz_pixeloffset;
				int column_x;
				int column_under_mouse = FromPixelToColumn (offset_x, out column_x);
				
				if ((column_x + CurrentTableStyle.GridColumnStyles[column_under_mouse].Width - offset_x < RESIZE_HANDLE_HORIZ_SIZE)
				    && column_under_mouse < CurrentTableStyle.GridColumnStyles.Count) {
					hit.type = HitTestType.ColumnResize;
					hit.column = column_under_mouse;
				}
				else {
					hit.type = HitTestType.ColumnHeader;
					hit.column = column_under_mouse;
				}
				return hit;
			}

			if (rowhdrs_area.Contains (x, y)) {
				int posy;
				int rcnt = FirstVisibleRow + VisibleRowCount;
				for (int r = FirstVisibleRow; r < rcnt; r++) {
					posy = cells_area.Y + rows[r].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
					if (y <= posy + rows[r].Height) {
						if ((posy + rows[r].Height) - y < RESIZE_HANDLE_VERT_SIZE) {
							hit.type = HitTestType.RowResize;
						}
						else {
							hit.type = HitTestType.RowHeader;
						}
						hit.row = r;
						break;
					}
				}
				return hit;
			}

			if (caption_area.Contains (x, y)) {
				hit.type = HitTestType.Caption;
				return hit;
			}

			if (parent_rows.Contains (x, y)) {
				hit.type = HitTestType.ParentRows;
				return hit;
			}

			int pos_y, pos_x, width;
			int rowcnt = FirstVisibleRow + VisibleRowCount;
			for (int row = FirstVisibleRow; row < rowcnt; row++) {

				pos_y = cells_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
				if (y <= pos_y + rows[row].Height) {
					hit.row = row;
					hit.type = HitTestType.Cell;					
					int col_pixel;
					int column_cnt = first_visiblecolumn + visiblecolumn_count;
					for (int column = first_visiblecolumn; column < column_cnt; column++) {

						col_pixel = GetColumnStartingPixel (column);
						pos_x = cells_area.X + col_pixel - horz_pixeloffset;
						width = CurrentTableStyle.GridColumnStyles[column].Width;

						if (x <= pos_x + width) { // Column found
							hit.column = column;
							break;
						}
					}

					break;
				}
			}

			return hit;
		}

		internal Rectangle GetCellBounds (int row, int col)
		{
			Rectangle bounds = new Rectangle ();
			int col_pixel;

			bounds.Width = CurrentTableStyle.GridColumnStyles[col].Width;
			bounds.Height = rows[row].Height;
			bounds.Y = cells_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
			col_pixel = GetColumnStartingPixel (col);
			bounds.X = cells_area.X + col_pixel - horz_pixeloffset;
			return bounds;
		}

		internal void InvalidateCaption ()
		{
			if (caption_area.IsEmpty)
				return;

			Invalidate (caption_area);
		}

		internal void InvalidateCells ()
		{
			if (cells_area.IsEmpty)
				return;

			Invalidate (cells_area);
		}
		
		internal void InvalidateRow (int row)
		{
			if (row < FirstVisibleRow || row > FirstVisibleRow + VisibleRowCount) {
				return;
			}

			Rectangle rect_row = new Rectangle ();

			int row_width = CalcAllColumnsWidth ();
			if (row_width > cells_area.Width)
				row_width = cells_area.Width;
			rect_row.X = cells_area.X;
			rect_row.Width = row_width;
			rect_row.Height = rows[row].Height;
			rect_row.Y = cells_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
			Invalidate (rect_row);
		}

		internal void InvalidateRowHeader (int row)
		{
			Rectangle rect_rowhdr = new Rectangle ();
			rect_rowhdr.X = rowhdrs_area.X;
			rect_rowhdr.Width = rowhdrs_area.Width;
			rect_rowhdr.Height = rows[row].Height;
			rect_rowhdr.Y = rowhdrs_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
			Invalidate (rect_rowhdr);
		}	

		internal void InvalidateColumn (DataGridColumnStyle column)
		{
			Rectangle rect_col = new Rectangle ();
			int col_pixel;
			int col = -1;

			col = CurrentTableStyle.GridColumnStyles.IndexOf (column);

			if (col == -1) {
				return;
			}

			rect_col.Width = column.Width;
			col_pixel = GetColumnStartingPixel (col);
			rect_col.X = cells_area.X + col_pixel - horz_pixeloffset;
			rect_col.Y = cells_area.Y;
			rect_col.Height = cells_area.Height;
			Invalidate (rect_col);
		}

		internal void DrawResizeLineVert (int x)
		{
			XplatUI.DrawReversibleRectangle (Handle,
							 new Rectangle (x, cells_area.Y, 1, cells_area.Height - 3),
							 2);
		}

		internal void DrawResizeLineHoriz (int y)
		{
			XplatUI.DrawReversibleRectangle (Handle,
							 new Rectangle (cells_area.X, y, cells_area.Width - 3, 1),
							 2);
		}

		void SetUpHorizontalScrollBar (out int maximum)
		{
			maximum = CalcAllColumnsWidth ();

			horiz_scrollbar.Location = new Point (ClientRectangle.X, ClientRectangle.Y +
				ClientRectangle.Height - horiz_scrollbar.Height);

			horiz_scrollbar.Size = new Size (ClientRectangle.Width,
				horiz_scrollbar.Height);

			horiz_scrollbar.LargeChange = cells_area.Width;
		}


		void SetUpVerticalScrollBar (out int height, out int maximum)
		{
			int y;
			
			if (caption_visible) {
				y = ClientRectangle.Y + caption_area.Height;
				height = ClientRectangle.Height - caption_area.Height;
			} else {
				y = ClientRectangle.Y;
				height = ClientRectangle.Height;
			}

			vert_scrollbar.Location = new Point (ClientRectangle.X +
				ClientRectangle.Width - vert_scrollbar.Width, y);

			vert_scrollbar.Size = new Size (vert_scrollbar.Width,
				height);

			maximum = RowsCount;
			
			if (ShowEditRow && RowsCount > 0) {
				maximum++;	
			}
			
			vert_scrollbar.LargeChange = VLargeChange;
		}

		#endregion // Public Instance Methods

		#region Instance Properties
		internal Rectangle CellsArea {
			get {
				return cells_area;
			}
		}

		// Returns the ColumnHeaders area excluding the rectangle shared with RowHeaders
		internal Rectangle ColumnHeadersArea {
			get {
				Rectangle columns_area = columnhdrs_area;

				if (CurrentTableStyle.CurrentRowHeadersVisible) {
					columns_area.X += RowHeaderWidth;
					columns_area.Width -= RowHeaderWidth;
				}
				return columns_area;
			}
		}

		bool ShowingColumnHeaders {
			get { return columnheaders_visible != false && CurrentTableStyle.GridColumnStyles.Count > 0; }
		}

		int ColumnHeadersHeight {
			get {
				return CurrentTableStyle.HeaderFont.Height + 6;
			}
		}

		internal Rectangle RowHeadersArea {
			get {
				return rowhdrs_area;
			}
		}

		internal int VLargeChange {
			get {
				return VisibleRowCount;
			}
		}

		#endregion Instance Properties

		#endregion // Code originally in DataGridDrawingLogic.cs
	}
}
