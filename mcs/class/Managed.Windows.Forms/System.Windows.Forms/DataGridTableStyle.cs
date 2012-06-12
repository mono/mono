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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//
//	Peter Bartok <pbartok@novell.com>
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
//
// NOT COMPLETE
//

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Data;
using System.Xml;
using System.Runtime.Serialization;

namespace System.Windows.Forms
{
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	public class DataGridTableStyle : Component, IDataGridEditingService
	{
		readonly
		public static DataGridTableStyle DefaultTableStyle = new DataGridTableStyle (true);

		#region	Local Variables
		private static readonly Color		def_alternating_backcolor = ThemeEngine.Current.DataGridAlternatingBackColor;
		private static readonly Color		def_backcolor = ThemeEngine.Current.DataGridBackColor;
		private static readonly Color		def_forecolor = SystemColors.WindowText;
		private static readonly Color		def_gridline_color = ThemeEngine.Current.DataGridGridLineColor;
		private static readonly Color		def_header_backcolor = ThemeEngine.Current.DataGridHeaderBackColor;
		private static readonly Font		def_header_font = ThemeEngine.Current.DefaultFont;
		private static readonly Color		def_header_forecolor = ThemeEngine.Current.DataGridHeaderForeColor;
		private static readonly Color		def_link_color = ThemeEngine.Current.DataGridLinkColor;
		private static readonly Color		def_link_hovercolor = ThemeEngine.Current.DataGridLinkHoverColor;
		private static readonly Color		def_selection_backcolor = ThemeEngine.Current.DataGridSelectionBackColor;
		private static readonly Color		def_selection_forecolor = ThemeEngine.Current.DataGridSelectionForeColor;
		private static readonly int		def_preferredrow_height = ThemeEngine.Current.DefaultFont.Height + 3;

		private bool				allow_sorting;
		private DataGrid			datagrid;
		private Color				header_forecolor;
		private string				mapping_name;
		private Color 				alternating_backcolor;
		private bool				columnheaders_visible;
		private GridColumnStylesCollection	column_styles;
		private Color 				gridline_color;
		private DataGridLineStyle 		gridline_style;
		private Color 				header_backcolor;
		private Font 				header_font;
		private Color 				link_color;
		private Color 				link_hovercolor;
		private int 				preferredcolumn_width;
		private int 				preferredrow_height;
		private bool 				_readonly;
		private bool 				rowheaders_visible;
		private Color 				selection_backcolor;
		private Color 				selection_forecolor;
		private int 				rowheaders_width;
		private Color				backcolor;
		private Color				forecolor;
		private bool				is_default;
		internal ArrayList                      table_relations;
		CurrencyManager				manager;
		#endregion	// Local Variables

		#region Constructors
		public DataGridTableStyle ()
			: this (false)
		{
		}

		public DataGridTableStyle (bool isDefaultTableStyle)
		{
			is_default = isDefaultTableStyle;
			allow_sorting = true;
			datagrid = null;
			header_forecolor = def_header_forecolor;
			mapping_name = string.Empty;
			table_relations = new ArrayList ();
			column_styles = new GridColumnStylesCollection (this);

			alternating_backcolor = def_alternating_backcolor;
			columnheaders_visible = true;
			gridline_color = def_gridline_color;
			gridline_style = DataGridLineStyle.Solid;
			header_backcolor = def_header_backcolor;
			header_font = null;
			link_color = def_link_color;
			link_hovercolor = def_link_hovercolor;
			preferredcolumn_width = ThemeEngine.Current.DataGridPreferredColumnWidth;
			preferredrow_height = ThemeEngine.Current.DefaultFont.Height + 3;
			_readonly = false;
			rowheaders_visible = true;
			selection_backcolor = def_selection_backcolor;
			selection_forecolor = def_selection_forecolor;
			rowheaders_width = 35;
			backcolor = def_backcolor;
			forecolor = def_forecolor;
		}

		public DataGridTableStyle (CurrencyManager listManager)
			: this (false)
		{
			manager = listManager;
		}
		#endregion

		#region Public Instance Properties
		[DefaultValue(true)]
		public bool AllowSorting {
			get { return allow_sorting; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (allow_sorting != value) {
					allow_sorting = value;
					OnAllowSortingChanged (EventArgs.Empty);
				}
			}
		}

		public Color AlternatingBackColor {
			get { return alternating_backcolor; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (alternating_backcolor != value) {
					alternating_backcolor = value;
					OnAlternatingBackColorChanged (EventArgs.Empty);
				}
			}
		}

		public Color BackColor {
			get { return backcolor; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (backcolor != value) {
					backcolor = value;
					// XXX This should be OnBackColorChanged, MS made a c&p error, I think
					OnForeColorChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		public bool ColumnHeadersVisible {
			get {
				return columnheaders_visible;
			}

			set {
				if (columnheaders_visible != value) {
					columnheaders_visible = value;
					OnColumnHeadersVisibleChanged (EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public virtual DataGrid DataGrid {
			get { return datagrid; }
			set {
				if (datagrid != value) {
					datagrid = value;

					/* now set the value on all our column styles */
					for (int i = 0; i < column_styles.Count; i ++) {
						column_styles[i].SetDataGridInternal (datagrid);
					}
				}
			}
		}

		public Color ForeColor {
			get { return forecolor; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (forecolor != value) {
					forecolor = value;
					// XXX This should be OnForeColorChanged, MS made a c&p error, I think
					OnBackColorChanged (EventArgs.Empty);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Localizable(true)]
		public virtual GridColumnStylesCollection GridColumnStyles {
			get { return column_styles; }
		}

		public Color GridLineColor {
			get { return gridline_color; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (gridline_color != value) {
					gridline_color = value;
					OnGridLineColorChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(DataGridLineStyle.Solid)]
		public DataGridLineStyle GridLineStyle {
			get { return gridline_style; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (gridline_style != value) {
					gridline_style = value;
					OnGridLineStyleChanged (EventArgs.Empty);
				}
			}
		}

		public Color HeaderBackColor {
			get { return header_backcolor; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (value == Color.Empty) {
					throw new ArgumentNullException ("Color.Empty value is invalid.");
				}

				if (header_backcolor != value) {
					header_backcolor = value;
					OnHeaderBackColorChanged (EventArgs.Empty);
				}
			}
		}

		[AmbientValue(null)]
		[Localizable(true)]
		public Font HeaderFont {
			get {
				if (header_font != null)
					return header_font;

				if (DataGrid != null)
					return DataGrid.Font;

				return def_header_font;
			}
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (header_font != value) {
					header_font = value;
					OnHeaderFontChanged (EventArgs.Empty);
				}
			}
		}

		public Color HeaderForeColor {
			get { return header_forecolor; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (header_forecolor != value) {
					header_forecolor = value;
					OnHeaderForeColorChanged (EventArgs.Empty);
				}
			}
		}

		public Color LinkColor {
			get { return link_color; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (link_color != value) {
					link_color = value;
					OnLinkColorChanged (EventArgs.Empty);
				}
			}
		}


		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public Color LinkHoverColor {
			get { return link_hovercolor; }
			set {
				if (link_hovercolor != value) {
					link_hovercolor = value;
					// XXX MS doesn't emit this event (even though they should...)
					// OnLinkHoverColorChanged (EventArgs.Empty);
				}
			}
		}

		[Editor("System.Windows.Forms.Design.DataGridTableStyleMappingNameEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[DefaultValue ("")]
		public string MappingName {
			get { return mapping_name; }
			set {
				if (value == null)
					value = "";

				if (mapping_name != value) {
					mapping_name = value;
					OnMappingNameChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(75)]
		[TypeConverter(typeof(DataGridPreferredColumnWidthTypeConverter))]
		[Localizable(true)]
		public int PreferredColumnWidth {
			get { return preferredcolumn_width; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (value < 0) {
					throw new ArgumentException ("PreferredColumnWidth is less than 0");
				}

				if (preferredcolumn_width != value) {
					preferredcolumn_width = value;
					OnPreferredColumnWidthChanged (EventArgs.Empty);
				}
			}
		}

		[Localizable(true)]
		public int PreferredRowHeight {
			get { return preferredrow_height; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (preferredrow_height != value) {
					preferredrow_height = value;
					OnPreferredRowHeightChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(false)]
		public virtual bool ReadOnly {
			get { return _readonly; }
			set {
				if (_readonly != value) {
					_readonly = value;
					OnReadOnlyChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		public bool RowHeadersVisible {
			get { return rowheaders_visible; }
			set {
				if (rowheaders_visible != value) {
					rowheaders_visible = value;
					OnRowHeadersVisibleChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(35)]
		[Localizable(true)]
		public int RowHeaderWidth {
			get { return rowheaders_width; }
			set {
				if (rowheaders_width != value) {
					rowheaders_width = value;
					OnRowHeaderWidthChanged (EventArgs.Empty);
				}
			}
		}

		public Color SelectionBackColor {
			get { return selection_backcolor; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (selection_backcolor != value) {
					selection_backcolor = value;
					OnSelectionBackColorChanged (EventArgs.Empty);
				}
			}
		}

		[Description("The foreground color for the current data grid row")]
		public Color SelectionForeColor  {
			get { return selection_forecolor; }
			set {
				if (is_default)
					throw new ArgumentException ("Cannot change the value of this property on the default DataGridTableStyle.");

				if (selection_forecolor != value) {
					selection_forecolor = value;
					OnSelectionForeColorChanged (EventArgs.Empty);
				}
			}
		}

		#endregion	// Public Instance Properties

		#region Private Instance Properties
		internal DataGridLineStyle CurrentGridLineStyle {
			get {
				if (is_default && datagrid != null) {
					return datagrid.GridLineStyle;
				}

				return gridline_style;
			}
		}

		internal Color CurrentGridLineColor {
			get {
				if (is_default && datagrid != null) {
					return datagrid.GridLineColor;
				}

				return gridline_color;
			}
		}

		internal Color CurrentHeaderBackColor {
			get {
				if (is_default && datagrid != null) {
					return datagrid.HeaderBackColor;
				}

				return header_backcolor;
			}
		}

		internal Color CurrentHeaderForeColor {
			get {
				if (is_default && datagrid != null) {
					return datagrid.HeaderForeColor;
				}

				return header_forecolor;
			}
		}

		internal int CurrentPreferredColumnWidth {
			get {
				if (is_default && datagrid != null) {
					return datagrid.PreferredColumnWidth;
				}

				return preferredcolumn_width;
			}
		}

		internal int CurrentPreferredRowHeight {
			get {
				if (is_default && datagrid != null) {
					return datagrid.PreferredRowHeight;
				}

				return preferredrow_height;
			}
		}
		
		internal bool CurrentRowHeadersVisible {
			get {
				if (is_default && datagrid != null) {
					return datagrid.RowHeadersVisible;
				}

				return rowheaders_visible;
			}
		}

		internal bool HasRelations {
			get { return table_relations.Count > 0; }
		}

		internal string[] Relations {
			get {
				string[] rel = new string[table_relations.Count];
				table_relations.CopyTo (rel, 0);
				return rel;
			}
		}

		#endregion Private Instance Properties

		#region Public Instance Methods

		[MonoTODO ("Not implemented, will throw NotImplementedException")]
		public bool BeginEdit (DataGridColumnStyle gridColumn, int rowNumber)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual DataGridColumnStyle CreateGridColumn (PropertyDescriptor prop)
		{
			return CreateGridColumn (prop,  false);
		}

		protected internal virtual DataGridColumnStyle CreateGridColumn (PropertyDescriptor prop,  bool isDefault)
		{
			if (prop.PropertyType == typeof (bool))
				return new DataGridBoolColumn (prop, isDefault);
			else {
				// At least to special cases with formats
				if (prop.PropertyType.Equals (typeof (DateTime))) {
					return new DataGridTextBoxColumn (prop, "d", isDefault);
				}

				if (prop.PropertyType.Equals (typeof (Int32)) ||
					prop.PropertyType.Equals (typeof (Int16))) {
					return new DataGridTextBoxColumn (prop, "G", isDefault);
				}

				return new DataGridTextBoxColumn (prop, isDefault);
			}
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		[MonoTODO ("Not implemented, will throw NotImplementedException")]
		public bool EndEdit (DataGridColumnStyle gridColumn, int rowNumber, bool shouldAbort)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnAllowSortingChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AllowSortingChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAlternatingBackColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [AlternatingBackColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnBackColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [BackColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnColumnHeadersVisibleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ColumnHeadersVisibleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnForeColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ForeColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnGridLineColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [GridLineColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnGridLineStyleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [GridLineStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnHeaderBackColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [HeaderBackColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnHeaderFontChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [HeaderFontChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnHeaderForeColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [HeaderForeColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnLinkColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [LinkColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnLinkHoverColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [LinkHoverColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnMappingNameChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [MappingNameChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnPreferredColumnWidthChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [PreferredColumnWidthChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnPreferredRowHeightChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [PreferredRowHeightChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnReadOnlyChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ReadOnlyChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnRowHeadersVisibleChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [RowHeadersVisibleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnRowHeaderWidthChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [RowHeaderWidthChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnSelectionBackColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [SelectionBackColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnSelectionForeColorChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [SelectionForeColorChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		public void ResetAlternatingBackColor ()
		{
			AlternatingBackColor = def_alternating_backcolor;
		}

		public void ResetBackColor ()
		{
			BackColor = def_backcolor;
		}

		public void ResetForeColor ()
		{
			ForeColor = def_forecolor;
		}

		public void ResetGridLineColor ()
		{
			GridLineColor = def_gridline_color;
		}

		public void ResetHeaderBackColor ()
		{
			HeaderBackColor = def_header_backcolor;
		}

		public void ResetHeaderFont ()
		{
			HeaderFont = def_header_font;
		}

		public void ResetHeaderForeColor ()
		{
			HeaderForeColor = def_header_forecolor;
		}

		public void ResetLinkColor ()
		{
			LinkColor = def_link_color;
		}

		public void ResetLinkHoverColor ()
		{
			LinkHoverColor = def_link_hovercolor;
		}

		public void ResetSelectionBackColor ()
		{
			SelectionBackColor = def_selection_backcolor;
		}

		public void ResetSelectionForeColor ()
		{
			SelectionForeColor = def_selection_forecolor;
		}

		protected virtual bool ShouldSerializeAlternatingBackColor ()
		{
			return (alternating_backcolor != def_alternating_backcolor);
		}

		protected bool ShouldSerializeBackColor ()
		{
			return (backcolor != def_backcolor);
		}

		protected bool ShouldSerializeForeColor ()
		{
			return (forecolor != def_forecolor);
		}

		protected virtual bool ShouldSerializeGridLineColor ()
		{
			return (gridline_color != def_gridline_color);
		}

		protected virtual bool ShouldSerializeHeaderBackColor ()
		{
			return (header_backcolor != def_header_backcolor);
		}

		protected virtual bool ShouldSerializeHeaderForeColor ()
		{
			return (header_forecolor != def_header_forecolor);
		}

		protected virtual bool ShouldSerializeLinkColor ()
		{
			return (link_color != def_link_color);
		}

		protected virtual bool ShouldSerializeLinkHoverColor ()
		{
			return (link_hovercolor != def_link_hovercolor);
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
		#endregion	// Protected Instance Methods

		#region Private Instance Properties
		// Create column styles for this TableStyle
		internal void CreateColumnsForTable (bool onlyBind)
		{
			CurrencyManager	mgr = manager;
			DataGridColumnStyle st;

			if (mgr == null) {
				mgr = datagrid.ListManager;

				if (mgr == null)
					return;
			}

			for (int i = 0; i < column_styles.Count; i ++)
				column_styles[i].bound = false;

			table_relations.Clear ();
			PropertyDescriptorCollection propcol = mgr.GetItemProperties ();

			for (int i = 0; i < propcol.Count; i++)
			{
				// The column style is already provided by the user
				st = column_styles[propcol[i].Name];
				if (st != null) {
					if (st.Width == -1)
						st.Width = CurrentPreferredColumnWidth;

					st.PropertyDescriptor = propcol[i];
					st.bound = true;
					continue;
				}

				if (onlyBind == true)
					continue;

				if (typeof (IBindingList).IsAssignableFrom (propcol[i].PropertyType)) {
					table_relations.Add (propcol[i].Name);
				} else {
					if (propcol[i].IsBrowsable) {
						st = CreateGridColumn (propcol[i],  true);
						st.bound = true;
						st.grid = datagrid;
						st.MappingName = propcol[i].Name;
						st.HeaderText = propcol[i].Name;
						st.Width = CurrentPreferredColumnWidth;
						column_styles.Add (st);
					}
				}
			}

		}

		#endregion Private Instance Properties

		#region Events
		static object AllowSortingChangedEvent = new object ();
		static object AlternatingBackColorChangedEvent = new object ();
		static object BackColorChangedEvent = new object ();
		static object ColumnHeadersVisibleChangedEvent = new object ();
		static object ForeColorChangedEvent = new object ();
		static object GridLineColorChangedEvent = new object ();
		static object GridLineStyleChangedEvent = new object ();
		static object HeaderBackColorChangedEvent = new object ();
		static object HeaderFontChangedEvent = new object ();
		static object HeaderForeColorChangedEvent = new object ();
		static object LinkColorChangedEvent = new object ();
		static object LinkHoverColorChangedEvent = new object ();
		static object MappingNameChangedEvent = new object ();
		static object PreferredColumnWidthChangedEvent = new object ();
		static object PreferredRowHeightChangedEvent = new object ();
		static object ReadOnlyChangedEvent = new object ();
		static object RowHeadersVisibleChangedEvent = new object ();
		static object RowHeaderWidthChangedEvent = new object ();
		static object SelectionBackColorChangedEvent = new object ();
		static object SelectionForeColorChangedEvent = new object ();

		public event EventHandler AllowSortingChanged {
			add { Events.AddHandler (AllowSortingChangedEvent, value); }
			remove { Events.RemoveHandler (AllowSortingChangedEvent, value); }
		}

		public event EventHandler AlternatingBackColorChanged {
			add { Events.AddHandler (AlternatingBackColorChangedEvent, value); }
			remove { Events.RemoveHandler (AlternatingBackColorChangedEvent, value); }
		}

		public event EventHandler BackColorChanged {
			add { Events.AddHandler (BackColorChangedEvent, value); }
			remove { Events.RemoveHandler (BackColorChangedEvent, value); }
		}

		public event EventHandler ColumnHeadersVisibleChanged {
			add { Events.AddHandler (ColumnHeadersVisibleChangedEvent, value); }
			remove { Events.RemoveHandler (ColumnHeadersVisibleChangedEvent, value); }
		}

		public event EventHandler ForeColorChanged {
			add { Events.AddHandler (ForeColorChangedEvent, value); }
			remove { Events.RemoveHandler (ForeColorChangedEvent, value); }
		}

		public event EventHandler GridLineColorChanged {
			add { Events.AddHandler (GridLineColorChangedEvent, value); }
			remove { Events.RemoveHandler (GridLineColorChangedEvent, value); }
		}

		public event EventHandler GridLineStyleChanged {
			add { Events.AddHandler (GridLineStyleChangedEvent, value); }
			remove { Events.RemoveHandler (GridLineStyleChangedEvent, value); }
		}

		public event EventHandler HeaderBackColorChanged {
			add { Events.AddHandler (HeaderBackColorChangedEvent, value); }
			remove { Events.RemoveHandler (HeaderBackColorChangedEvent, value); }
		}

		public event EventHandler HeaderFontChanged {
			add { Events.AddHandler (HeaderFontChangedEvent, value); }
			remove { Events.RemoveHandler (HeaderFontChangedEvent, value); }
		}

		public event EventHandler HeaderForeColorChanged {
			add { Events.AddHandler (HeaderForeColorChangedEvent, value); }
			remove { Events.RemoveHandler (HeaderForeColorChangedEvent, value); }
		}

		public event EventHandler LinkColorChanged {
			add { Events.AddHandler (LinkColorChangedEvent, value); }
			remove { Events.RemoveHandler (LinkColorChangedEvent, value); }
		}

		public event EventHandler LinkHoverColorChanged {
			add { Events.AddHandler (LinkHoverColorChangedEvent, value); }
			remove { Events.RemoveHandler (LinkHoverColorChangedEvent, value); }
		}

		public event EventHandler MappingNameChanged {
			add { Events.AddHandler (MappingNameChangedEvent, value); }
			remove { Events.RemoveHandler (MappingNameChangedEvent, value); }
		}

		public event EventHandler PreferredColumnWidthChanged {
			add { Events.AddHandler (PreferredColumnWidthChangedEvent, value); }
			remove { Events.RemoveHandler (PreferredColumnWidthChangedEvent, value); }
		}

		public event EventHandler PreferredRowHeightChanged {
			add { Events.AddHandler (PreferredRowHeightChangedEvent, value); }
			remove { Events.RemoveHandler (PreferredRowHeightChangedEvent, value); }
		}

		public event EventHandler ReadOnlyChanged {
			add { Events.AddHandler (ReadOnlyChangedEvent, value); }
			remove { Events.RemoveHandler (ReadOnlyChangedEvent, value); }
		}

		public event EventHandler RowHeadersVisibleChanged {
			add { Events.AddHandler (RowHeadersVisibleChangedEvent, value); }
			remove { Events.RemoveHandler (RowHeadersVisibleChangedEvent, value); }
		}

		public event EventHandler RowHeaderWidthChanged {
			add { Events.AddHandler (RowHeaderWidthChangedEvent, value); }
			remove { Events.RemoveHandler (RowHeaderWidthChangedEvent, value); }
		}

		public event EventHandler SelectionBackColorChanged {
			add { Events.AddHandler (SelectionBackColorChangedEvent, value); }
			remove { Events.RemoveHandler (SelectionBackColorChangedEvent, value); }
		}

		public event EventHandler SelectionForeColorChanged {
			add { Events.AddHandler (SelectionForeColorChangedEvent, value); }
			remove { Events.RemoveHandler (SelectionForeColorChangedEvent, value); }
		}
		#endregion	// Events
	}
}
