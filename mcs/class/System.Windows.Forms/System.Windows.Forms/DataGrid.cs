//
// System.Windows.Forms.DataGrid
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002/3
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
				//FIXME:
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
				//FIXME:
				return base.BackgroundImage;
			}
			set {
				//FIXME:
				base.BackgroundImage = value;
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
			get {
				throw new NotImplementedException ();
			}
			set { 
				//FIXME:
			}
		}
		
		[MonoTODO]
		public int CurrentRowIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		public override Cursor Cursor {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		public string DataMember {
			get { return dataMember; }
			set { dataMember=value; }
		}
		
		[MonoTODO]
		public object DataSource {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		protected override Size DefaultSize {
			get {
				//FIXME: verify numbers
				return new Size(300,200);
			}
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
			get {
				//FIXME:
				return base.ForeColor;
			}
			set {
				//FIXME:
				base.ForeColor = value;
			}
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
				//FIXME:
				return base.Text; 
			}
			set {
				//FIXME:
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
			//FIXME:
		}
		
		[MonoTODO]
		public void Collapse(int row) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected internal virtual void ColumnStartedEditing(Control editingControl) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected internal virtual void ColumnStartedEditing(Rectangle bounds) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		 protected override AccessibleObject CreateAccessibilityInstance() 
		 {
			//FIXME:
			return base.CreateAccessibilityInstance();
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
		public void Expand(int row) 
		{
			throw new NotImplementedException ();
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
			//FIXME:
		}
		
		[MonoTODO]
		protected virtual void GridVScrolled(object sender,ScrollEventArgs se) 
		{
			//FIXME:
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
			//FIXME:
		}
		
		[MonoTODO]
		public void NavigateTo(int rowNumber,string relationName) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected virtual void OnAllowNavigationChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected void OnBackButtonClicked(object sender,EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e) 
		{
			//FIXME:
			base.OnBackColorChanged(e);
		}
		
		[MonoTODO]
		protected virtual void OnBackgroundColorChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void OnBindingContextChanged(EventArgs e) 
		{
			//FIXME:
			base.OnBindingContextChanged(e);
		}
		
		[MonoTODO]
		protected virtual void OnBorderStyleChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected virtual void OnCaptionVisibleChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected virtual void OnCurrentCellChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected virtual void OnDataSourceChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void OnEnter(EventArgs e) 
		{
			//FIXME:
			base.OnEnter(e);
		}
		
		[MonoTODO]
		protected virtual void OnFlatModeChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) 
		{
			//FIXME:
			base.OnFontChanged(e);
		}
		
		[MonoTODO]
		protected override void OnForeColorChanged(EventArgs e) 
		{
			//FIXME:
			base.OnForeColorChanged(e);
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			//FIXME:
			base.OnHandleCreated(e);
		}
		
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e) 
		{
			//FIXME:
			base.OnHandleDestroyed(e);
		}
		
		[MonoTODO]
		protected override void OnKeyDown(KeyEventArgs e) 
		{
			//FIXME:
			base.OnKeyDown(e);
		}
		
		[MonoTODO]
		protected override void OnKeyPress(KeyPressEventArgs e) 
		{
			//FIXME:
			base.OnKeyPress(e);
		}
		
		[MonoTODO]
		protected override void OnLayout(LayoutEventArgs e) 
		{
			//FIXME:
			base.OnLayout(e);
		}
		
		[MonoTODO]
		protected override void OnLeave(EventArgs e) 
		{
			//FIXME:
			base.OnLeave(e);
		}
		
		[MonoTODO]
		protected override void OnMouseDown(MouseEventArgs e) 
		{
			//FIXME:
			base.OnMouseDown(e);
		}
		
		[MonoTODO]
		protected override void OnMouseLeave(EventArgs e) 
		{
			//FIXME:
			base.OnMouseLeave(e);
		}
		
		[MonoTODO]
		protected override void OnMouseMove(MouseEventArgs e) 
		{
			//FIXME:
			base.OnMouseMove(e);
		}
		
		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs e) 
		{
			//FIXME:
			base.OnMouseUp(e);
		}
		
		[MonoTODO]
		protected override void OnMouseWheel(MouseEventArgs e) 
		{
			//FIXME:
			base.OnMouseWheel(e);
		}
		
		[MonoTODO]
		protected void OnNavigate(NavigateEventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void OnPaint(PaintEventArgs e) 
		{
			//FIXME:
			base.OnPaint(e);
		}
		
		[MonoTODO]
		protected override void OnPaintBackground(PaintEventArgs e) 
		{
			//FIXME:
			base.OnPaintBackground(e);
		}
		
		[MonoTODO]
		protected virtual void OnParentRowsLabelStyleChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected virtual void OnParentRowsVisibleChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected virtual void OnReadOnlyChanged(EventArgs e) 
		{
			//FIXME:
		}
		[MonoTODO]
		protected void OnRowHeaderClick(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected void OnScroll(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected void OnShowParentDetailsButtonClicked(object sender,EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override bool ProcessDialogKey(Keys keyData) 
		{
			//FIXME:
			return base.ProcessDialogKey(keyData);
		}
		
		[MonoTODO]
		protected bool ProcessGridKey(KeyEventArgs ke) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool ProcessKeyPreview(ref Message m) 
		{
			//FIXME:
			return base.ProcessKeyPreview(ref m);
		}
		
		[MonoTODO]
		protected bool ProcessTabKey(Keys keyData) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetAlternatingBackColor() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public override void ResetBackColor() 
		{
			//FIXME:
			base.ResetBackColor();
		}
		
		[MonoTODO]
		public override void ResetForeColor() 
		{
			//FIXME:
			base.ResetForeColor();
		}
		
		[MonoTODO]
		public void ResetGridLineColor() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public void ResetHeaderBackColor() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public void ResetHeaderFont() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public void ResetHeaderForeColor() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public void ResetLinkColor() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected void ResetSelection() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public void ResetSelectionBackColor() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public void ResetSelectionForeColor() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public void Select(int row) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public void SetDataBinding(object dataSource,string dataMember) 
		{
			//FIXME:
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
			//FIXME:
		}
		#endregion
		
		#region Events
		public event EventHandler AllowNavigationChanged;
		public event EventHandler BackButtonClick;
		public event EventHandler BackgroundColorChanged;
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
				get {
					throw new NotImplementedException ();
				}
			}
			
			[MonoTODO]
			public int Row {
				get {
					throw new NotImplementedException (); 
				}
			}
			
			[MonoTODO]
			public HitTestType Type {
				get {
					throw new NotImplementedException ();
				}
			}
			#endregion
		
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
}
