//
// System.Windows.Forms.DataGrid
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Displays ADO.NET data in a scrollable grid.
	///
	/// </summary>

	[MonoTODO]
	public class DataGrid : Control, ISupportInitialize {

		#region Fields
		bool allowNavigation;
		bool allowSorting;
		Color alternatingBackColor;
		Color backgroundColor;
		BorderStyle borderStyle;
		Color captionBackColor;
		Font captionFont;
		Color captionForeColor;
		string captionText;
		bool captionVisible;
		bool columnHeadersVisible;
		string dataMember;
		bool flatMode;
		Color gridLineColor;
		DataGridLineStyle gridLineStyle;
		Color headerBackColor;
		Color headerForeColor;
		Color linkColor;
		Color linkHoverColor;
		Color parentRowsBackColor;
		Color parentRowsForeColor;
		DataGridParentRowsLabelStyle parentRowsLabelStyle;
		bool parentRowsVisible;
		bool readOnly;
		int rowHeaderWidth;
		Color selectionBackColor;
		Color selectionForeColor;
		#endregion
		
		#region Constructors
		[MonoTODO]
		public DataGrid() 
		{
			// setting default values:
			allowNavigation=true;
			borderStyle = BorderStyle.FixedSingle;
			captionBackColor = SystemColors.ActiveCaption;
			captionForeColor = SystemColors.ActiveCaptionText;
			captionText = "";
			captionVisible = true;
			columnHeadersVisible = true;
			dataMember = "";
			flatMode = true;
			gridLineColor = SystemColors.Control;
			gridLineStyle = DataGridLineStyle.Solid;
			headerBackColor = SystemColors.Control;
			headerForeColor = SystemColors.ControlText;
			linkColor = SystemColors.HotTrack;
			linkHoverColor = SystemColors.HotTrack;
			parentRowsBackColor = SystemColors.Control;
			parentRowsForeColor = SystemColors.WindowText;
			parentRowsLabelStyle = DataGridParentRowsLabelStyle.Both;
			parentRowsVisible = true;
			readOnly = false;
			rowHeaderWidth = 50;
			selectionBackColor = SystemColors.ActiveCaption;
			selectionForeColor = SystemColors.ActiveCaptionText;
		}
		#endregion
		
		#region Properties
		public bool AllowNavigation {
			get {
				return allowNavigation;
			}
			set {
				allowNavigation=value;
			}
		}
		
		public bool AllowSorting {
			get { 
				return allowSorting; 
			}
			set { 
				allowSorting=value; 
			}
		}
		
		public Color AlternatingBackColor {
			get { 
				return alternatingBackColor;
			}
			set {
				alternatingBackColor=value;
			}
		}
		
		[MonoTODO]
		public override Color BackColor {
			get {
				throw new NotImplementedException ();
			}
			set { 
				throw new NotImplementedException (); 
			}
		}
		
		public Color BackgroundColor {
			get { 
				return backgroundColor;
			}
			set {
				backgroundColor=value;
			}
		}
		
		[MonoTODO]
		public override Image BackgroundImage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public BorderStyle BorderStyle {
			get {
				return borderStyle;
			}
			set {
				borderStyle=value;
			}
		}
		
		public Color CaptionBackColor {
			get {
				return captionBackColor; 
			}
			set { 
				captionBackColor=value;
			}
		}
		
		public Font CaptionFont {
			get {
				return captionFont;
			}
			set {
				captionFont=value;
			}
		}
		
		public Color CaptionForeColor {
			get {
				return captionForeColor;
			}
			set {
				captionForeColor=value;
			}
		}
		
		public string CaptionText {
			get {
				return captionText; 
			}
			set {
				captionText=value; 
			}
		}
		
		public bool CaptionVisible {
			get { 
				return captionVisible; 
			}
			set { 
				captionVisible=value;
			}
		}
		
		public bool ColumnHeadersVisible {
			get {
				return columnHeadersVisible;
			}
			set {
				columnHeadersVisible=value;
			}
		}
		
		[MonoTODO]
		public DataGridCell CurrentCell {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public int CurrentRowIndex {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override Cursor Cursor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public string DataMember {
			get { return dataMember; }
			set { dataMember=value; }
		}
		
		[MonoTODO]
		public object DataSource {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		protected override Size DefaultSize {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public int FirstVisibleColumn {
			get { throw new NotImplementedException (); }
		}
		
		public bool FlatMode {
			get { return flatMode; }
			set { flatMode=value; }
		}
		
		[MonoTODO]
		public override Color ForeColor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
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
			set {
				if (value==Color.Empty) throw new ArgumentNullException();
				headerBackColor=value;
			}
		}
		
		public Color HeaderForeColor {
			get { return headerForeColor; }
			set { headerForeColor=value; }
		}
		
		[MonoTODO]
		protected ScrollBar HorizScrollBar {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public object this[DataGridCell cell] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public object this[int rowIndex,int columnIndex] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public Color LinkColor {
			get { return linkColor; }
			set { linkColor=value; }
		}
		
		public Color LinkHoverColor {
			get { return linkHoverColor; }
			set { linkHoverColor=value; }
		}
		
		[MonoTODO]
		protected internal CurrencyManager ListManager {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public Color ParentRowsBackColor {
			get { return parentRowsBackColor; }
			set { parentRowsBackColor=value; }
		}
		
		public Color ParentRowsForeColor {
			get { return parentRowsForeColor; }
			set { parentRowsForeColor=value; }
		}
		
		public DataGridParentRowsLabelStyle ParentRowsLabelStyle {
			get { return parentRowsLabelStyle; }
			set { parentRowsLabelStyle=value; }
		}
		
		public bool ParentRowsVisible {
			get { return parentRowsVisible; }
			set { parentRowsVisible=value; }
		}
		
		[MonoTODO]
		public int PreferredColumnWidth {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public int PreferredRowHeight {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public bool ReadOnly {
			get { return readOnly; }
			set { readOnly=value; }
		}
		
		[MonoTODO]
		public bool RowHeadersVisible {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
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
		
		[MonoTODO]
		public override ISite Site {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public GridTableStylesCollection TableStyles {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override string Text {
			get { 
				return base.Text; 
			}
			set {
				base.Text = value; 
			}
		}
		
		[MonoTODO]
		protected ScrollBar VertScrollBar {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public int VisibleColumnCount {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public int VisibleRowCount {
			get { throw new NotImplementedException (); }
		}
		#endregion
		
		#region Methods
		/* Following members support the .NET Framework infrastructure and are not intended to be used directly from your code.
		 * Methods not stubbed out:
		   - protected virtual string GetOutputTextDelimiter()
			 - public void ResetLinkHoverColor()
			 - public void SubObjectsSiteChange(bool site)
		 */
		[MonoTODO]
		public bool BeginEdit(DataGridColumnStyle gridColumn,int rowNumber) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void BeginInit() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected virtual void CancelEditing() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void Collapse(int row) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual void ColumnStartedEditing(Control editingControl) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual void ColumnStartedEditing(Rectangle bounds) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		 protected override AccessibleObject CreateAccessibilityInstance() 
		 {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual DataGridColumnStyle CreateGridColumn(PropertyDescriptor prop,bool isDefault) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool EndEdit(DataGridColumnStyle gridColumn,int rowNumber,bool shouldAbort) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void EndInit() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public Rectangle GetCellBounds(DataGridCell dgc) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Rectangle GetCellBounds(int row,int col) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public Rectangle GetCurrentCellBounds() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void GridHScrolled(object sender,ScrollEventArgs se) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void GridVScrolled(object sender,ScrollEventArgs se) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public HitTestInfo HitTest(Point position) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public HitTestInfo HitTest(int x,int y) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool IsExpanded(int rowNumber) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool IsSelected(int row) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void NavigateBack() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void NavigateTo(int rowNumber,string relationName) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnAllowNavigationChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void OnBackButtonClicked(object sender,EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnBackgroundColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnBindingContextChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnBorderStyleChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnCaptionVisibleChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnCurrentCellChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnDataSourceChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnEnter(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnFlatModeChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnForeColorChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnKeyDown(KeyEventArgs ke) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnKeyPress(KeyPressEventArgs kpe) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnLayout(LayoutEventArgs levent) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnLeave(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnMouseDown(MouseEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnMouseLeave(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnMouseMove(MouseEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnMouseWheel(MouseEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void OnNavigate(NavigateEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnPaint(PaintEventArgs pe) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnPaintBackground(PaintEventArgs ebe) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParentRowsLabelStyleChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnParentRowsVisibleChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void OnReadOnlyChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected void OnRowHeaderClick(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void OnScroll(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void OnShowParentDetailsButtonClicked(object sender,EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool ProcessDialogKey(Keys keyData) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected bool ProcessGridKey(KeyEventArgs ke) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool ProcessKeyPreview(ref Message m) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected bool ProcessTabKey(Keys keyData) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetAlternatingBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void ResetBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override void ResetForeColor() 
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
		
		[MonoTODO]
		protected void ResetSelection() 
		{
			throw new NotImplementedException ();
		}
		
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
		public void Select(int row) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetDataBinding(object dataSource,string dataMember) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeAlternatingBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeBackgroundColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeCaptionBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeCaptionForeColor() 
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
		protected bool ShouldSerializeHeaderFont() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeHeaderForeColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeLinkHoverColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeParentRowsBackColor() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual bool ShouldSerializeParentRowsForeColor() 
		{
			throw new NotImplementedException ();
		}
		
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
		
		[MonoTODO]
		public void UnSelect(int row) 
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region Events
		[MonoTODO]
		public event EventHandler AllowNavigationChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler BackButtonClick {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler BackgroundColorChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler BorderStyleChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler CaptionVisibleChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler CurrentCellChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler DataSourceChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler FlatModeChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event NavigateEventHandler Navigate {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler ParentRowsLabelStyleChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler ParentRowsVisibleChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler ReadOnlyChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		protected event EventHandler RowHeaderClick {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler Scroll {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler ShowParentDetailsButtonClick {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		#endregion
		
		/// sub-class: DataGrid.HitTestInfo
		/// <summary>
		/// Contains information about a part of the System.Windows.Forms.DataGrid at a specified coordinate. This class cannot be inherited.
		/// </summary>
		[MonoTODO]
		public sealed class HitTestInfo {
			#region DataGrid.HitTestInfo: Fields
			[MonoTODO]
			public static readonly DataGrid.HitTestInfo Nowhere;
			#endregion
			
			#region DataGrid.HitTestInfo: Properties
			[MonoTODO]
			public int Column {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public int Row {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public HitTestType Type {
				get { throw new NotImplementedException (); }
			}
			#endregion
			
			#region DataGrid.HitTestInfo: Methods
			[MonoTODO]
			public override bool Equals(object value) 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public override int GetHashCode() 
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public override string ToString() 
			{
				throw new NotImplementedException ();
			}
			#endregion
		}
		
		/// sub-enumeration: DataGrid.HitTestType
		/// <summary>
		/// Specifies the part of the System.Windows.Forms.DataGrid control the user has clicked
		/// </summary>
		[Flags]
		[Serializable]
		public enum HitTestType {
			Caption = 32,
			Cell = 1,
			ColumnHeader = 2,
			ColumnResize = 8,
			None = 0,
			ParentRows = 64,
			RowHeader = 4,
			RowResize = 16
		}
	}
}
