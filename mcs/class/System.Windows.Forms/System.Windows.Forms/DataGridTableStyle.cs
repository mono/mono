//
// System.Windows.Forms.DataGridTableStyle
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;
using System.Drawing;
using System.Collections;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents the table drawn by the System.Windows.Forms.DataGrid control at run time.
	/// </summary>
	
	[MonoTODO]
	public class DataGridTableStyle : Component {

		#region Fields
		// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code:
		// public static DataGridTableStyle DefaultTableStyle;
		bool allowSorting;
		Color alternatingBackColor;
		Color backColor;
		bool columnHeadersVisible;
		DataGrid dataGrid;
		Color foreColor;
		Color gridLineColor;
		DataGridLineStyle gridLineStyle;
		Color headerBackColor;
		Font headerFont;
		Color headerForeColor;
		Color linkColor;
		string mappingName;
		int preferredColumnWidth;
		int preferredRowHeight;
		bool readOnly;
		bool rowHeadersVisible;
		int rowHeaderWidth;
		Color selectionBackColor;
		Color selectionForeColor;
		#endregion
		
		#region Constructors
		[MonoTODO]
		public DataGridTableStyle() 
		{
			allowSorting=true;
			alternatingBackColor=SystemColors.Window;
			gridLineStyle=DataGridLineStyle.Solid;
			rowHeadersVisible=true;
			throw new NotImplementedException ();
		}

		// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code:
		// public DataGridTableStyle(bool isDefaultTableStyle);
		
		[MonoTODO]
		public DataGridTableStyle(CurrencyManager listManager) : this() 
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region Properties
		public bool AllowSorting {
			get { return allowSorting; }
			set { allowSorting=value; }
		}
		
		public Color AlternatingBackColor {
			get { return alternatingBackColor; }
			set { alternatingBackColor=value; }
		}
		
		public Color BackColor {
			get { return backColor; }
			set { backColor=value; }
		}
		
		public bool ColumnHeadersVisible {
			get { return columnHeadersVisible; }
			set { columnHeadersVisible=value; }
		}
		
		public virtual DataGrid DataGrid {
			get { return dataGrid; }
			set { dataGrid=value; }
		}
		
		public Color ForeColor {
			get { return foreColor; }
			set { foreColor=value; }
		}
		
		[MonoTODO]
		public virtual GridColumnStylesCollection GridColumnStyles {
			get { throw new NotImplementedException (); }
		}
		
		public Color GridLineColor {
			get { return gridLineColor; }
			set { gridLineColor=value; }
		}
		
		public DataGridLineStyle GridLineStyle {
			get { return gridLineStyle; }
			set { gridLineStyle=value; }
		}
		
		public Color HeaderBackColor {
			get { return headerBackColor; }
			set { headerBackColor=value; }
		}
		
		public Font HeaderFont {
			get { return headerFont; }
			set { headerFont=value; }
		}
		
		public Color HeaderForeColor {
			get { return headerForeColor; }
			set { headerForeColor=value; }
		}
		
		public Color LinkColor {
			get { return linkColor; }
			set { linkColor=value; }
		}
		
		// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		// public Color LinkHoverColor {get; set;}
		
		public string MappingName {
			get { return mappingName; }
			set { mappingName=value; }
		}
		
		public int PreferredColumnWidth {
			get { return preferredColumnWidth; }
			set { preferredColumnWidth=value; }
		}
		
		public int PreferredRowHeight {
			get { return preferredRowHeight; }
			set { preferredRowHeight=value; }
		}
		
		public virtual bool ReadOnly {
			get { return allowSorting; }
			set { allowSorting=value; }
		}
		
		public bool RowHeadersVisible {
			get { return rowHeadersVisible; }
			set { rowHeadersVisible=value; }
		}
		
		[MonoTODO]
		public int RowHeaderWidth {
			get { return rowHeaderWidth; }
			set { rowHeaderWidth=value; }
		}
		
		public Color SelectionBackColor {
			get { return selectionBackColor; }
			set { selectionBackColor=value; }
		}
		
		public Color SelectionForeColor {
			get { return selectionForeColor; }
			set { selectionForeColor=value; }
		}
		#endregion
		
		#region Methods
		[MonoTODO]
		public bool BeginEdit(DataGridColumnStyle gridColumn,int rowNumber) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop,bool isDefault) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void Dispose(bool disposing) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool EndEdit(DataGridColumnStyle gridColumn,int rowNumber,bool shouldAbort) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnAllowSortingChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnAlternatingBackColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnBackColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnColumnHeadersVisibleChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnForeColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnGridLineColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnGridLineStyleChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnHeaderBackColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnHeaderFontChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnHeaderForeColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnLinkColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnLinkHoverColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnMappingNameChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnPreferredColumnWidthChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnPreferredRowHeightChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnReadOnlyChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnRowHeadersVisibleChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnRowHeaderWidthChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnSelectionBackColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnSelectionForeColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetAlternatingBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetForeColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetGridLineColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetHeaderBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetHeaderFont() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetHeaderForeColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetLinkColor() 
		{
			throw new NotImplementedException ();
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		// public void ResetLinkHoverColor
		
		[MonoTODO]
		public void ResetSelectionBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetSelectionForeColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeAlternatingBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected bool ShouldSerializeBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected bool ShouldSerializeForeColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeGridLineColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeHeaderBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeHeaderForeColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeLinkColor() 
		{
			throw new NotImplementedException ();
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// protected virtual bool ShouldSerializeLinkHoverColor();
		
		[MonoTODO]
		protected bool ShouldSerializePreferredRowHeight() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected bool ShouldSerializeSelectionBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeSelectionForeColor() 
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region Events
		public event EventHandler AllowSortingChanged ;
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
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public event EventHandler LinkHoverColorChanged;
		
		public event EventHandler MappingNameChanged;
		public event EventHandler PreferredColumnWidthChanged;
		public event EventHandler PreferredRowHeightChanged;
		public event EventHandler ReadOnlyChanged;
		public event EventHandler RowHeadersVisibleChanged;
		public event EventHandler RowHeaderWidthChanged;
		public event EventHandler SelectionBackColorChanged;
		public event EventHandler SelectionForeColorChanged;
		#endregion
	}
}
