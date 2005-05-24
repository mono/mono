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
		public static DataGridTableStyle DefaultTableStyle = new DataGridTableStyle (true);
		
		#region	Local Variables
		private static readonly Color		def_alternating_backcolor = SystemColors.Window;
		private static readonly Color		def_backcolor = SystemColors.Window;
		private static readonly Color		def_forecolor = SystemColors.WindowText;
		private static readonly Color		def_gridline_color = SystemColors.Control;
		private static readonly Color		def_header_backcolor = SystemColors.Control;
		private static readonly Font		def_header_font = null;
		private static readonly Color		def_header_forecolor = SystemColors.ControlText;
		private static readonly Color		def_link_color = SystemColors.HotTrack;
		private static readonly Color		def_link_hovercolor = SystemColors.HotTrack;
		private static readonly Color		def_selection_backcolor = SystemColors.ActiveCaption;
		private static readonly Color		def_selection_forecolor = SystemColors.ActiveCaptionText;

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
		CurrencyManager				manager;
		#endregion	// Local Variables

		#region Constructors
		public DataGridTableStyle ()
		{
			CommonConstructor ();
			is_default = false;
		}

		public DataGridTableStyle (bool isDefaultTableStyle)
		{
			CommonConstructor ();
			is_default = isDefaultTableStyle;
		}

		// TODO: What to do with the CurrencyManager
		public DataGridTableStyle (CurrencyManager listManager)
		{
			CommonConstructor ();
			is_default = false;
			manager = listManager;
		}

		private void CommonConstructor ()
		{
			allow_sorting = true;
			datagrid = null;
			header_forecolor = def_header_forecolor;
			mapping_name = string.Empty;
			column_styles = new GridColumnStylesCollection (this);

			alternating_backcolor = def_alternating_backcolor;
			columnheaders_visible = true;
			gridline_color = def_gridline_color;
			gridline_style = DataGridLineStyle.Solid;
			header_backcolor = def_header_backcolor;
			header_font = def_header_font;
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
		#endregion

		#region Public Instance Properties
		[DefaultValue(true)]
		public bool AllowSorting {
			get {
				return allow_sorting;
			}

			set {
				if (allow_sorting != value) {
					allow_sorting = value;
					OnAllowSortingChanged (EventArgs.Empty);
				}
			}
		}

		public Color AlternatingBackColor {
			get {
				return alternating_backcolor;
			}

			set {
				if (alternating_backcolor != value) {
					alternating_backcolor = value;
					OnAlternatingBackColorChanged (EventArgs.Empty);
				}
			}
		}

		public Color BackColor {
			get {
				return backcolor;
			}

			set {
				if (backcolor != value) {
					backcolor = value;
					OnBackColorChanged (EventArgs.Empty);
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
			get {
				return datagrid;
			}

			set {
				if (datagrid != value) {
					datagrid = value;
				}
			}
		}

		public Color ForeColor {
			get {
				return forecolor;
			}

			set {
				if (forecolor != value) {
					forecolor = value;
					OnForeColorChanged (EventArgs.Empty);
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Localizable(true)]
		public virtual GridColumnStylesCollection GridColumnStyles {
			get { return column_styles; }
		}

		public Color GridLineColor {
			get {
				return gridline_color;
			}

			set {
				if (gridline_color != value) {
					gridline_color = value;
					OnGridLineColorChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(DataGridLineStyle.Solid)]
		public DataGridLineStyle GridLineStyle {
			get {
				return gridline_style;
			}

			set {
				if (gridline_style != value) {
					gridline_style = value;
					OnGridLineStyleChanged (EventArgs.Empty);
				}
			}
		}

		public Color HeaderBackColor {
			get {
				return header_backcolor;
			}

			set {
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

				if (datagrid != null)
					return datagrid.Font;

				return ThemeEngine.Current.DefaultFont;
			}

			set {
				if (header_font != value) {
					header_font = value;
					OnHeaderFontChanged (EventArgs.Empty);
				}
			}
		}

		public Color HeaderForeColor {
			get {
				return header_forecolor;
			}

			set {

				if (header_forecolor != value) {
					header_forecolor = value;
					OnHeaderForeColorChanged (EventArgs.Empty);
				}
			}
		}

		public Color LinkColor {
			get {
				return link_color;
			}

			set {
				if (link_color != value) {
					link_color = value;
					OnLinkColorChanged (EventArgs.Empty);
				}
			}
		}

		[ComVisible(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public Color LinkHoverColor {
			get {
				return link_hovercolor;
			}

			set {
				if (link_hovercolor != value) {
					link_hovercolor = value;
					OnLinkHoverColorChanged (EventArgs.Empty);
				}
			}
		}

		[Editor("System.Windows.Forms.Design.DataGridTableStyleMappingNameEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		public string MappingName {
			get {
				return mapping_name;
			}

			set {
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
			get {
				return preferredcolumn_width;
			}

			set {
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
			get {
				return preferredrow_height;
			}

			set {
				if (preferredrow_height != value) {
					preferredrow_height = value;
					OnPreferredRowHeightChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(false)]
		public bool ReadOnly {
			get {
				return _readonly;
			}

			set {
				if (_readonly != value) {
					_readonly = value;
					OnReadOnlyChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		public bool RowHeadersVisible {
			get {
				return rowheaders_visible;
			}

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
			get {
				return rowheaders_width;
			}

			set {
				if (rowheaders_width != value) {
					rowheaders_width = value;
					OnRowHeaderWidthChanged (EventArgs.Empty);
				}
			}
		}

		public Color SelectionBackColor {
			get {
				return selection_backcolor;
			}

			set {
				if (selection_backcolor != value) {
					selection_backcolor = value;
					OnSelectionBackColorChanged (EventArgs.Empty);
				}
			}
		}

		[Description("The foreground color for the current data grid row")]
		public Color SelectionForeColor  {
			get {
				return selection_forecolor;
			}

			set {
				if (selection_forecolor != value) {
					selection_forecolor = value;
					OnSelectionForeColorChanged (EventArgs.Empty);
				}
			}
		}

		#endregion	// Public Instance Properties

		#region Public Instance Methods

		[MonoTODO]
		public virtual bool BeginEdit (DataGridColumnStyle gridColumn,  int rowNumber)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual DataGridColumnStyle CreateGridColumn (PropertyDescriptor prop)
		{	
			return CreateGridColumn (prop,  false);
		}
		
		protected internal virtual DataGridColumnStyle CreateGridColumn (PropertyDescriptor prop,  bool isDefault)
		{			
			if (DataGridBoolColumn.CanRenderType (prop.PropertyType)) {
				return new DataGridBoolColumn (prop, isDefault);
			}
			
			if (DataGridTextBoxColumn.CanRenderType (prop.PropertyType)) {
				return new DataGridTextBoxColumn (prop, isDefault);
			}
			
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		[MonoTODO]
		public virtual bool EndEdit ( DataGridColumnStyle gridColumn,  int rowNumber,  bool shouldAbort)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnAllowSortingChanged (EventArgs e)
		{
			if (AllowSortingChanged != null) {
				AllowSortingChanged (this, e);
			}
		}

		protected virtual void OnAlternatingBackColorChanged (EventArgs e)
		{
			if (AlternatingBackColorChanged != null) {
				AlternatingBackColorChanged (this, e);
			}
		}

		protected virtual void OnBackColorChanged (EventArgs e)
		{
			if (BackColorChanged != null) {
				BackColorChanged (this, e);
			}
		}

		protected virtual void OnColumnHeadersVisibleChanged (EventArgs e)
		{
			if (ColumnHeadersVisibleChanged != null) {
				ColumnHeadersVisibleChanged (this, e);
			}
		}

		protected virtual void OnForeColorChanged (EventArgs e)
		{
			if (ForeColorChanged != null) {
				ForeColorChanged (this, e);
			}
		}

		protected virtual void OnGridLineColorChanged (EventArgs e)
		{
			if (GridLineColorChanged != null) {
				GridLineColorChanged (this, e);
			}
		}

		protected virtual void OnGridLineStyleChanged (EventArgs e)
		{
			if (GridLineStyleChanged != null) {
				GridLineStyleChanged (this, e);
			}
		}

		protected virtual void OnHeaderBackColorChanged (EventArgs e)
		{
			if (HeaderBackColorChanged != null) {
				HeaderBackColorChanged (this, e);
			}
		}

		protected virtual void OnHeaderFontChanged (EventArgs e)
		{
			if (HeaderFontChanged != null) {
				HeaderFontChanged (this, e);
			}
		}

		protected virtual void OnHeaderForeColorChanged (EventArgs e)
		{
			if (HeaderForeColorChanged != null) {
				HeaderForeColorChanged (this, e);
			}
		}

		protected virtual void OnLinkColorChanged (EventArgs e)
		{
			if (LinkColorChanged != null) {
				LinkColorChanged (this, e);
			}
		}

		protected virtual void OnLinkHoverColorChanged (EventArgs e)
		{
			if (LinkHoverColorChanged != null) {
				LinkHoverColorChanged (this, e);
			}
		}

		protected virtual void OnMappingNameChanged (EventArgs e)
		{
			if (MappingNameChanged != null) {
				MappingNameChanged(this, e);
			}
		}

		protected virtual void OnPreferredColumnWidthChanged (EventArgs e)
		{
			if (PreferredColumnWidthChanged != null) {
				PreferredColumnWidthChanged (this, e);
			}
		}

		protected virtual void OnPreferredRowHeightChanged (EventArgs e)
		{
			if (PreferredRowHeightChanged != null) {
				PreferredRowHeightChanged (this, e);
			}
		}

		protected virtual void OnReadOnlyChanged (EventArgs e)
		{
			if (ReadOnlyChanged != null) {
				ReadOnlyChanged (this, e);
			}
		}

		protected virtual void OnRowHeadersVisibleChanged (EventArgs e)
		{
			if (RowHeadersVisibleChanged != null) {
				RowHeadersVisibleChanged (this, e);
			}
		}

		protected virtual void OnRowHeaderWidthChanged (EventArgs e)
		{
			if (RowHeaderWidthChanged != null) {
				RowHeaderWidthChanged (this, e);
			}
		}

		protected virtual void OnSelectionBackColorChanged (EventArgs e)
		{
			if (SelectionBackColorChanged != null) {
				SelectionBackColorChanged (this, e);
			}
		}

		protected virtual void OnSelectionForeColorChanged (EventArgs e)
		{
			if (SelectionForeColorChanged != null) {
				SelectionForeColorChanged (this, e);
			}
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
			return (preferredrow_height != ThemeEngine.Current.DefaultFont.Height + 3);
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
		internal void CreateColumnsForTable () 
		{
			CurrencyManager	mgr = null;			
			
			/*if (manager != null) {
				mgr = manager;
			} else {
				if (datagrid != null) {
					mgr = datagrid.ListManager;
				}
			}*/

			mgr = datagrid.ListManager;
			
			if (mgr == null) {
				return;
			}
			
			column_styles.Clear ();
			PropertyDescriptorCollection propcol = mgr.GetItemProperties ();			
			
			for (int i = 0; i < propcol.Count; i++)
			{				
				// TODO: What to do with relations?
				if (propcol[i].ComponentType.ToString () == "System.Data.DataTablePropertyDescriptor") {					
					Console.WriteLine ("CreateColumnsForTable::System.Data.DataTablePropertyDescriptor");
					
				} else {
					DataGridColumnStyle st = CreateGridColumn (propcol[i],  true);
					st.TableStyle = this;
					st.MappingName = propcol[i].Name;
					st.Width = PreferredColumnWidth;					
					column_styles.Add (st);					
				}				
			}
			
		}
		
		#endregion Private Instance Properties

		#region Events
		public event EventHandler AllowSortingChanged;
		public event EventHandler AlternatingBackColorChanged;
		public event EventHandler BackColorChanged;
		public event EventHandler ColumnHeadersVisibleChanged;
		public event EventHandler ForeColorChanged;
		public event EventHandler GridLineColorChanged;
		public event EventHandler GridLineStyleChanged;
		public event EventHandler HeaderBackColorChanged;
		public event EventHandler HeaderFontChanged;
		public event EventHandler HeaderForeColorChanged;
		public event EventHandler LinkColorChanged;
		public event EventHandler LinkHoverColorChanged;
		public event EventHandler MappingNameChanged;
		public event EventHandler PreferredColumnWidthChanged;
		public event EventHandler PreferredRowHeightChanged;
		public event EventHandler ReadOnlyChanged;
		public event EventHandler RowHeadersVisibleChanged;
		public event EventHandler RowHeaderWidthChanged;
		public event EventHandler SelectionBackColorChanged;
		public event EventHandler SelectionForeColorChanged;
		#endregion	// Events
	}
}

