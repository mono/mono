//
// System.Windows.Forms.DataGrid
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//		Modified for System.Window.Forms/Gtk by :
//			Joel Basson
// (C) Ximian, Inc., 2002/3
//
using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Runtime.InteropServices;
using GLib;
using Gtk;
using GtkSharp;

namespace System.Windows.Forms {

	/// <summary>
	/// Displays ADO.NET data in a scrollable grid.
	///
	/// </summary>

	[MonoTODO]
	public class DataGrid : Control, ISupportInitialize {
	
		TreeView treeView = null;
		DataGridColumn[] gridColumns = null;
		ListStore store = null;
		
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
		
		internal class DataGridColumn 
			{
			internal string columnName = "";
			internal TreeViewColumn treeViewColumn = null;

			internal string ColumnName {
				get {
					return columnName;
				}
				set {
					columnName = value;
				}
			}

			internal TreeViewColumn TreeViewColumn {
				get {
					return treeViewColumn;
				}
				set {
					treeViewColumn = value;
				}
			}
		}
		
		internal override Gtk.Widget CreateWidget () {
		
			//VBox vb = new VBox(false, 4);
		
			ScrolledWindow sw = new ScrolledWindow ();
			//vb.PackStart (sw, true, true, 0);

			treeView = new TreeView (store);
			treeView.HeadersVisible = CaptionVisible;
			treeView.Show();
			sw.Add (treeView);
			sw.Show();
			//vb.Show();
			return sw;
		}
		#endregion
		
		protected override void Dispose(bool disposing){
		}


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
		public override System.Drawing.Image BackgroundImage {
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
		
		//[MonoTODO]
		//public override Cursor Cursor {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
				//FIXME:
		//	}
		//}
		
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
		public virtual bool BeginEdit(DataGridColumnStyle gridColumn,int rowNumber) 
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
		public virtual bool EndEdit(DataGridColumnStyle gridColumn,int rowNumber,bool shouldAbort) 
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
		protected override void OnKeyPress(KeyPressEventArgs kpe) 
		{
			//FIXME:
			base.OnKeyPress(kpe);
		}
		
		[MonoTODO]
		protected override void OnLayout(LayoutEventArgs levent) 
		{
			//FIXME:
			base.OnLayout(levent);
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
		protected override void OnPaint(PaintEventArgs pe) 
		{
			//FIXME:
			base.OnPaint(pe);
		}
		
		[MonoTODO]
		protected override void OnPaintBackground(PaintEventArgs ebe) 
		{
			//FIXME:
			base.OnPaintBackground(ebe);
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
		//protected override bool ProcessKeyPreview(ref Message m) 
		//{
			//FIXME:
			//return base.ProcessKeyPreview(ref m);
		//}
		
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
		
		// start of Gtk# stuff
		[MonoTODO]
		public void SetDataBinding(object mydataSource, string mydataMember) 
		{
			Clear();
			System.Object o = null;
			o = GetResolvedDataSource (mydataSource, mydataMember);
			IEnumerable ie = (IEnumerable) o;
			ITypedList tlist = (ITypedList) o;

			// FIXME: does not belong in this base method
			TreeIter iter = new TreeIter ();
									
			PropertyDescriptorCollection pdc = tlist.GetItemProperties (new PropertyDescriptor[0]);

			// FIXME: does not belong in this base method
			gridColumns = new DataGridColumn[pdc.Count];
			
			// FIXME: does not belong in base method
			// define the columns in the treeview store
			// based on the schema of the result
			uint[] theTypes = new uint[pdc.Count];
			
			for (int col = 0; col < pdc.Count; col++) {
				theTypes[col] = (int) TypeFundamentals.TypeString;
			}
			store.SetColumnTypes (theTypes);
			
			// FIXME: does not belong in this base method
			int colndx = -1;
			foreach (PropertyDescriptor pd in pdc) {
				colndx ++;
				gridColumns[colndx] = new DataGridColumn ();
				gridColumns[colndx].ColumnName = pd.Name;				
			}
			
			foreach (System.Object obj in ie) {
				ICustomTypeDescriptor custom = (ICustomTypeDescriptor) obj;
				PropertyDescriptorCollection properties;
				properties = custom.GetProperties ();
				
				iter = NewRow ();
				int cv = 0;
				foreach (PropertyDescriptor property in properties) {
					object oPropValue = property.GetValue (obj);
					string sPropValue = oPropValue.ToString ();
					
					// FIXME: does not belong in this base method
					SetColumnValue (iter, cv, sPropValue);

					cv++;
				}
			}

			// FIXME: does not belong in this base method
			treeView.Model = store;
			AutoCreateTreeViewColumns (treeView);
		}
		
		private IEnumerable GetResolvedDataSource(object source, string member) 
		{
			if (source != null && source is IListSource) {
				IListSource src = (IListSource) source;
				IList list = src.GetList ();
				if (!src.ContainsListCollection) {
					return list;
				}
				if (list != null && list is ITypedList) {

					ITypedList tlist = (ITypedList) list;
					PropertyDescriptorCollection pdc = tlist.GetItemProperties (new PropertyDescriptor[0]);
					if (pdc != null && pdc.Count > 0) {
						PropertyDescriptor pd = null;
						if (member != null && member.Length > 0) {
							pd = pdc.Find (member, true);
						} else {
							pd = pdc[0];
						}
						if (pd != null) {
							object rv = pd.GetValue (list[0]);
							if (rv != null && rv is IEnumerable) {
								return (IEnumerable)rv;
							}
						}
						throw new Exception ("ListSource_Missing_DataMember");
					}
					throw new Exception ("ListSource_Without_DataMembers");
				}
			}
			if (source is IEnumerable) {
				return (IEnumerable)source;
			}
			return null;
		}
		
		private TreeIter NewRow () 
		{ 
			TreeIter rowTreeIter = new TreeIter();
			store.Append (out rowTreeIter);
			return rowTreeIter;
		}
		
		private void AutoCreateTreeViewColumns (TreeView theTreeView) 
		{
			for(int col = 0; col < gridColumns.Length; col++) {
				// escape underscore _ because it is used
				// as the underline in menus and labels
				StringBuilder name = new StringBuilder ();
				foreach (char ch in gridColumns[col].ColumnName) {
					if (ch == '_')
						name.Append ("__");
					else
						name.Append (ch);
				}
				TreeViewColumn tvc;
				tvc = CreateColumn (theTreeView, col, 
						name.ToString ());
				theTreeView.AppendColumn (tvc);
			}
		}

		private void SetColumnValue (TreeIter iter, int column, string value) 
		{
			GLib.Value cell = new GLib.Value (value);
			store.SetValue (iter, column, cell);	
		}
		
		public TreeViewColumn CreateColumn (TreeView theTreeView, int col, 
						string columnName) 
		{
			TreeViewColumn NameCol = new TreeViewColumn ();		 
			CellRenderer NameRenderer = new CellRendererText ();
			
			NameCol.Title = columnName;
			NameCol.PackStart (NameRenderer, true);
			NameCol.AddAttribute (NameRenderer, "text", col);

			gridColumns[col].TreeViewColumn = NameCol;
			
			return NameCol;
		}
		
		internal void Clear () 
		{
			if (store != null) {
				store.Clear ();
				store = null;
				store = new ListStore ((int)TypeFundamentals.TypeString);
			}
			else
				store = new ListStore ((int)TypeFundamentals.TypeString);	

			if (gridColumns != null) {
				for (int c = 0; c < gridColumns.Length; c++) {
					if (gridColumns[c] != null) {
						if (gridColumns[c].TreeViewColumn != null) {
							treeView.RemoveColumn (gridColumns[c].TreeViewColumn);
							gridColumns[c].TreeViewColumn = null;
						}
						gridColumns[c] = null;
					}
				}
				gridColumns = null;
			}
		}
		// end of gtk# stuff
		
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
