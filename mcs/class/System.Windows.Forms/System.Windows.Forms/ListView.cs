//
// System.Windows.Forms.ListView.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu), Dennis Hayes (dennish@raytek.com)
//	 Implemented by Jordi Mas i Hernàndez (jmas@softcatala.org)
// (C) 2002/3 Ximian, Inc
//

/*
	TODO
	 
	- Multiple insertions of the same items should thown an exception
	- Images
	- Drag and drop
	- Font
*/

using System.Collections;
using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;


namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>	
	public class ListView : Control 	{		
		
		private ListViewItemCollection	itemsCollection = null;			     
		private ColumnHeaderCollection 	columCol = null;						 
		private bool bInitialised = false;					 
		private View viewMode = View.LargeIcon;
		private bool bAllowColumnReorder = false;
		private	SortOrder sortOrder = SortOrder.None;
		private bool bLabelEdit = false;
		private bool bFullRowSelect = false;
		private bool bGridLines = false;
		private bool bAutoArrange = true;
		private bool bLabelWrap = true;
		private bool bMultiSelect = true;
		private	bool bCheckBoxes = false;
		private	Color backColor = SystemColors.Window;
		private	Color foreColor = SystemColors.WindowText;
		private SelectedListViewItemCollection selItemsCol = null;
		private	SelectedIndexCollection	selItemIndexs = null;
		private CheckedListViewItemCollection chkItemCol = null;
		private CheckedIndexCollection chkIndexCol = null;
		private	ItemActivation	activation = ItemActivation.Standard; 
		private ColumnHeaderStyle headerStyle = ColumnHeaderStyle.Clickable;
		private	BorderStyle borderStyle = BorderStyle.Fixed3D;
		bool bScrollable = true;
		bool bHideSelection = true;
		bool bHoverSelection = false;
				
		
		//
		//  --- Constructor
		//		
		public ListView()  : base()		{				
			
			itemsCollection = new ListViewItemCollection(this);			     
			columCol = new ColumnHeaderCollection(this);
			selItemsCol = new SelectedListViewItemCollection(this);
			selItemIndexs = new SelectedIndexCollection(this);
			chkItemCol = new CheckedListViewItemCollection(this);
			chkIndexCol =  new CheckedIndexCollection(this);
			
			INITCOMMONCONTROLSEX	initEx = new INITCOMMONCONTROLSEX();
			initEx.dwICC = CommonControlInitFlags.ICC_LISTVIEW_CLASSES;
			Win32.InitCommonControlsEx(initEx);
			
			SubClassWndProc_ = true;
		}

		//
		//  --- Public Properties
		//
		
		public ItemActivation Activation {				
			get {return activation; }
			set {				
				activation = value;
				ItemActivationCtrl();
				}			
			
		}
		[MonoTODO]
		public ListViewAlignment Alignment {
			get {
				return 0;
				//throw new NotImplementedException ();
			}
			set {
				//throw new NotImplementedException ();
			}
		}
		
		public bool AllowColumnReorder {			
						
			get { return bAllowColumnReorder;  }
			set { 					
				ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_HEADERDRAGDROP, value);
				bAllowColumnReorder = value; 
			}			
		}
		
		
		public bool AutoArrange {
			
			get {return bAutoArrange; }
			set {bAutoArrange = value;}			
		}
		
		public override Color BackColor {
			
			get {return backColor; }
			set {
				
				backColor = value;
				SetBkColorCtrl();
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
			get { return borderStyle;  }
			set { borderStyle = value; }			
		}
		
		public bool CheckBoxes {
			
			get { return bCheckBoxes;  }
			set { 					
				ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_CHECKBOXES, value);
				bCheckBoxes = value; 
			}			
		}
		
		public CheckedIndexCollection CheckedIndices {
			get {return chkIndexCol;}
		}
		
		public  CheckedListViewItemCollection CheckedItems {
			get {return chkItemCol;}
		}
		
		public ColumnHeaderCollection Columns {
			get {return columCol;}
		}

		[MonoTODO]
		protected override void Dispose(bool disposing){
			base.Dispose(disposing);
		}

		
		public override Color ForeColor {
			get {return foreColor; }
			set {foreColor = value;}			
		}
		
		public bool FullRowSelect {
			
			get { return bFullRowSelect;}
				set { 					
					ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_FULLROWSELECT, value);
					bFullRowSelect = value; 
				}
		}
		
		public bool GridLines {
			
			get { return bGridLines;}
			set { 					
				ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_GRIDLINES, value);
				bGridLines = value; 
			}
		}
		
		public ColumnHeaderStyle HeaderStyle {
			get {return headerStyle; }
			set {headerStyle = value;}			
		}
		
		public bool HideSelection {
			get {return bHideSelection;}			
			set {bHideSelection=value;}
		}

		public bool HoverSelection {
			get {return bHideSelection;}			
			set {
				ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_TRACKSELECT, value);
				bHideSelection=value;
			}
		}
		
		public ListView.ListViewItemCollection Items {
			get {return itemsCollection;}
		}
		
		
		public bool LabelEdit {
			get {return bLabelEdit;}			
			set {bLabelEdit=value;}
		}
		
		public bool LabelWrap {
			get {return bLabelWrap;}			
			set {bLabelWrap=value;}
		}
		
		[MonoTODO]
		public ImageList LargeImageList {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public bool MultiSelect {
			
			get {return bMultiSelect;}			
			set {bMultiSelect=value;}
		}
		
		
		public bool Scrollable {
			get {return bScrollable;}			
			set {bScrollable=value;}
		}
		
		public ListView.SelectedIndexCollection SelectedIndices {
			get {return selItemIndexs;}
			
		}
		
		public ListView.SelectedListViewItemCollection SelectedItems {
			get {return selItemsCol;}
		}
		[MonoTODO]
		public ImageList SmallImageList {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public SortOrder Sorting {
			
			get {return sortOrder;}
			set {					
					SortMsg(value);// Throw an exception if an invalid enum value is given				
					sortOrder = value;
				}
		}
				
		
		[MonoTODO]
		public ImageList StateImageList {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}
		
		public View View 	{
			get {return viewMode;}
			
			set 
			{				
				ViewMode(value);	// Throw an exception if an invalid enum value is given				
				viewMode = value;
			}		
		}

		//
		//  --- Public Methods
		//
		[MonoTODO]
		public void ArrangeIcons() {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void ArrangeIcons(ListViewAlignment value) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void BeginUpdate() {
			throw new NotImplementedException ();
		}
		
		public void Clear() {
			
												
			selItemsCol.Clear();			
			itemsCollection.Clear();			     
			columCol.Clear();						 			
			
			if (!bInitialised) return;
			
			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_DELETEALLITEMS, 0,0);			
			
			
		}
		[MonoTODO]
		public void EndUpdate() {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void EnsureVisible(int index) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public ListViewItem GetItemAt(int x, int y) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Rectangle GetItemRect(int index) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Rectangle GetItemRect(int index, ItemBoundsPortion portion) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString() {
			
			return "List View object";			
		}

		//
		//  --- Public Events
		//		
		public event LabelEditEventHandler AfterLabelEdit;
		public event LabelEditEventHandler BeforeLabelEdit;
		public event ColumnClickEventHandler ColumnClick;
		public event EventHandler ItemActivate;
		
		[MonoTODO]
		public event ItemCheckEventHandler ItemCheck;
		public event ItemDragEventHandler ItemDrag;
		public event EventHandler SelectedIndexChanged;

		//
		//  --- Protected Properties
		//		
		protected override CreateParams CreateParams {
			get {			
											
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = "SysListView32";
				createParams.Style = ((int)WindowStyles.WS_CHILD |(int)WindowStyles.WS_VISIBLE|
					ViewMode(viewMode)| SortMsg(sortOrder));
					
				if (LabelEdit)	 createParams.Style |= (int) ListViewFlags.LVS_EDITLABELS;
				if (AutoArrange) createParams.Style |= (int) ListViewFlags.LVS_AUTOARRANGE;
				if (!bLabelWrap)  createParams.Style |= (int) ListViewFlags.LVS_NOLABELWRAP;
				if (!bMultiSelect) createParams.Style |= (int) ListViewFlags.LVS_SINGLESEL;
				if (!bScrollable)  createParams.Style |= (int) ListViewFlags.LVS_NOSCROLL;
				if (!bHideSelection) createParams.Style |= (int) ListViewFlags.LVS_SHOWSELALWAYS;
								
				switch (headerStyle)
				{
					case ColumnHeaderStyle.Clickable:	// Default						
						break;
					case ColumnHeaderStyle.Nonclickable:
						createParams.Style |= (int) ListViewFlags.LVS_NOSORTHEADER;
						break;
					case ColumnHeaderStyle.None:
						createParams.Style |= (int) ListViewFlags.LVS_NOCOLUMNHEADER;
						break;
					default:	
						break;				
				}
				
				switch (borderStyle)
				{
					case BorderStyle.Fixed3D:	
						createParams.ExStyle |= (int) WindowExStyles.WS_EX_CLIENTEDGE;
						break;
					case BorderStyle.FixedSingle:
						createParams.Style |= (int) WindowStyles.WS_BORDER;
						break;
					case BorderStyle.None:
						//createParams.ExStyle |= (int) ListViewFlags.LVS_NOCOLUMNHEADER;
						break;
					default:	
						break;				
				}
									
				return createParams;
			}		
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get {
				return new Size(121, 97); //Correct Size.
			}
		}

		//
		//  --- Protected Methods
		//
		
		[MonoTODO]
		protected override void CreateHandle() {
			base.CreateHandle();
		}

		[MonoTODO]
		protected override bool IsInputKey(Keys keyData) {
			//FIXME:
			return base.IsInputKey(keyData);
		}

		
		// Implemented
		protected virtual void  OnAfterLabelEdit(LabelEditEventArgs e) {}		
		protected virtual void  OnBeforeLabelEdit(LabelEditEventArgs e) {}		
		protected virtual void  OnColumnClick(ColumnClickEventArgs e) {}
		protected virtual void  OnItemActivate(EventArgs e) 	{}

		[MonoTODO]
		protected override void OnEnabledChanged(EventArgs e) {
			//FIXME:
			base.OnEnabledChanged(e);
		}

		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) {
			//FIXME:
			base.OnFontChanged(e);
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) {
			//FIXME:
			base.OnHandleCreated(e);
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e) {
			//FIXME:
		}

		
		[MonoTODO]
		protected virtual void  OnItemCheck(ItemCheckEventArgs e) {
			//FIXME:
		}
		[MonoTODO]
		protected virtual void  OnItemDrag(ItemDragEventArgs e) {
			//FIXME:
		}
//		[MonoTODO]
//		protected virtual void  OnSelectedItemChanged(EventArgs e) {
//			//FIXME:
//		}

		[MonoTODO]
		protected override void OnSystemColorsChanged(EventArgs e) {
			//FIXME:
			base.OnSystemColorsChanged(e);
		}
		[MonoTODO]
		protected override void Select(bool directed, bool forward) {
			//FIXME:
			base.Select(directed, forward);
		}
		[MonoTODO]
		protected void UpdateExtendedStyles() {
			//FIXME:
		}
		
		
		//
		//  --- Private Methods
		//		
		private int TextAlign(HorizontalAlignment textAlign)
		{			
			int style = 0;
			switch (textAlign) 
			{
				case HorizontalAlignment.Left:
					style = (int) ListViewColumnFlags.LVCFMT_LEFT;
					break;
				case HorizontalAlignment.Center:
					style = (int) ListViewColumnFlags.LVCFMT_CENTER;
					break;
				case HorizontalAlignment.Right:
					style = (int) ListViewColumnFlags.LVCFMT_RIGHT;
					break;				
					
				default:
					throw new  InvalidEnumArgumentException();	// TODO: Is this ok?
			}	
			
			return style;
		}
		
		// Converts from SortOrder enumerator to a LVS_ type
		private int SortMsg(SortOrder sortOrder) {
			
			int nRslt;
			
			switch (sortOrder) 
			{
				case SortOrder.Ascending:
					nRslt = (int) ListViewFlags.LVS_SORTASCENDING;
					break;
				case SortOrder.Descending:
					nRslt = (int) ListViewFlags.LVS_SORTDESCENDING;
					break;
				case SortOrder.None:
					nRslt = 0;
					break;				
					
				default:
					throw new  InvalidEnumArgumentException();	// TODO: Is this ok?
			}	
			
			//Console.WriteLine("Sort value " + nRslt); 
			return nRslt;
		}
		
		// Convers from SortOrder enumerator to a LVS_ type
		private ListViewExtendedFlags ItemActivationMsg(ItemActivation itemActivation) {
			
			ListViewExtendedFlags nRslt;
			
			switch (itemActivation) 
			{
				case ItemActivation.OneClick:
					nRslt = ListViewExtendedFlags.LVS_EX_ONECLICKACTIVATE;
					break;
				case ItemActivation.TwoClick:
					nRslt = ListViewExtendedFlags.LVS_EX_TWOCLICKACTIVATE;
					break;
				case ItemActivation.Standard:
					nRslt = 0;
					break;				
					
				default:
					throw new  InvalidEnumArgumentException();	
			}				
			
			return nRslt;
		}
		
		//
		//	Converts from enum View to Win32 LCV_ flag
		//
		private int ViewMode(View view)
		{							
			int viewCtrl = 0;
			switch (view) 
			{
				case View.Details:
					viewCtrl = (int) ListViewFlags.LVS_REPORT;
					break;
				case View.LargeIcon:
					viewCtrl = (int) ListViewFlags.LVS_ICON;
					break;
				case View.List:
					viewCtrl = (int) ListViewFlags.LVS_LIST;
					break;				
				case View.SmallIcon:
					viewCtrl = (int) ListViewFlags.LVS_SMALLICON;
					break;	
					
				default:
					throw new  InvalidEnumArgumentException();								
				
			}				
			return viewCtrl;
		}
		
		// Inserts a column in the control
		internal void InsertColumnInCtrl(ColumnHeader  column)
		{	
			if (!bInitialised) return;	
			
			LVCOLUMN lvc = new LVCOLUMN();				
			bool bAutoSizing = false;	
			int iCol;
			
			Console.WriteLine("Insert columns " + column.Text + " pos: " + column.Index+ " serial: "+  column.Serial+ " autosizing " + bAutoSizing);    						
					
			lvc.mask = (int)( ListViewColumnFlags.LVCF_FMT | ListViewColumnFlags.LVCF_WIDTH | ListViewColumnFlags.LVCF_TEXT | ListViewColumnFlags.LVCF_SUBITEM);
			lvc.iSubItem = column.Serial;
			lvc.pszText = column.Text;
			lvc.cx = column.Width;
			lvc.fmt = TextAlign(column.TextAlign);									
			
			IntPtr lvcBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(lvc));
  			Marshal.StructureToPtr(lvc, lvcBuffer, false);			
  			iCol = (int) Win32.SendMessage(Handle, (int)ListViewMessages.LVM_INSERTCOLUMNA, column.Index, lvcBuffer);
  			Marshal.FreeHGlobal(lvcBuffer);			
  			
		}	
		// Inserts an item in the control
		internal void InsertItemInCtrl(ListViewItem listViewItem)
		{			
			if (!bInitialised) return;
			
			LVITEM item = new LVITEM();							
			
			item.pszText = listViewItem.Text;
			item.iItem = listViewItem.Index;
			item.iSubItem = 0;
			item.lParam = 0;
			item.mask = ListViewItemFlags.LVIF_TEXT;
			
			IntPtr liBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(item));
  			Marshal.StructureToPtr(item, liBuffer, false);			
  			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_INSERTITEMA, listViewItem.Index, liBuffer);
  			Marshal.FreeHGlobal(liBuffer);						
  			
  			Console.WriteLine("Inserting " + listViewItem.Index + "-" + listViewItem.Text); 
		}
		
		// Removes an item in the control
		internal void RemoveItemInCtrl(int iIndex)
		{			
			if (!bInitialised) return;						
			Console.WriteLine("Deleting " + iIndex); 			
  			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_DELETEITEM, iIndex, 0); 						  			
		}	
		
		
		// Get item bound
		internal Rectangle GetItemBoundInCtrl(int iIndex)
		{			
			if (!bInitialised) return new Rectangle();						
			
			IntPtr lpBuffer = IntPtr.Zero;
			
			try {									
					RECT rect = new RECT();										
					rect.left = (int) SubItemPortion.LVIR_BOUNDS;
					lpBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(rect));					
					Marshal.StructureToPtr(rect, lpBuffer, false);													
					Win32.SendMessage (Handle, (int) ListViewMessages.LVM_GETITEMRECT , iIndex, lpBuffer);
					
					rect = (RECT) Marshal.PtrToStructure (lpBuffer, typeof (RECT));					
					return new Rectangle (rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top );
				}
				finally {
					Marshal.FreeHGlobal (lpBuffer);					
				}
		}	
		
		// Forces label edit
		internal void LabelEditInCtrl(int iIndex)
		{			
			if (!bInitialised) return;
			
			Win32.SetFocus(Handle);	// Need focus first
			Win32.SendMessage (Handle, (int) ListViewMessages.LVM_EDITLABEL, iIndex, 0);
		}
		
		
		// Sets a subitem
		internal void SetItemInCtrl(ListViewItem.ListViewSubItem listViewSubItem, int nPos)
		{			
			if (!bInitialised) return;			
			
			LVITEM item = new LVITEM();							
			
			item.pszText = listViewSubItem.Text;
			item.iItem = listViewSubItem.ListViewItem.Index;
			item.iSubItem = nPos;
			item.lParam = 0;
			item.mask = ListViewItemFlags.LVIF_TEXT;
			
			Console.WriteLine("SetItemInCtrl " + nPos); 			
			
			IntPtr liBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(item));
  			Marshal.StructureToPtr(item, liBuffer, false);			
  			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETITEMA, 0, liBuffer);
  			Marshal.FreeHGlobal(liBuffer);						
  			
  			
		}
		
		// Remove a column from the control
		internal int DeleteColumnInCtrl(int nIndex){	
			
			if (!bInitialised) return 0;
					
			Console.WriteLine("Delete column " + nIndex);    											
  			return Win32.SendMessage(Handle, (int)ListViewMessages.LVM_DELETECOLUMN, nIndex, 0);  			
		}				
		
		// Get the check status of an item						
		internal bool GetCheckStateInCtrl(int nIndex){					
			
			if (!bInitialised) return false;					

  			if ((Win32.SendMessage(Handle, (int)ListViewMessages.LVM_GETITEMSTATE, nIndex, 
  				(int)ListViewItemState.LVIS_STATEIMAGEMASK) & (int)ListViewItemState.LVIS_CHECKED)==(int)ListViewItemState.LVIS_CHECKED) 
  				return true;
  			else
  				return false;
		}					  
  		
		
		// Sets item activation	
		internal void ItemActivationCtrl(){	
			
			ListViewExtendedFlags	flags = ItemActivationMsg(Activation);	// Thowns an exception
			
			if (!bInitialised) return;
					
			if (flags==0)	// Standard mode
			{	
				Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETEXTENDEDLISTVIEWSTYLE, (int)ListViewExtendedFlags.LVS_EX_ONECLICKACTIVATE, 0);  					
				Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETEXTENDEDLISTVIEWSTYLE, (int)ListViewExtendedFlags.LVS_EX_TWOCLICKACTIVATE, 0);  			  				
  			}
  			else
  				Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETEXTENDEDLISTVIEWSTYLE, (int)flags, (int)flags);  			
		}					
		
		// Sets or remove and extended style from the control
		internal void ExtendedStyleCtrl(ListViewExtendedFlags ExStyle, bool bStatus){	
			
			if (!bInitialised) return;
					
			if (bStatus)			
  				Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETEXTENDEDLISTVIEWSTYLE, (int)ExStyle, (int)ExStyle);  			
  			else
  				Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETEXTENDEDLISTVIEWSTYLE, (int)ExStyle, 0);  			
		}					
		
		
		// Sets Background color in control
		internal void SetBkColorCtrl(){	
			
			if (!bInitialised) return;					
			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETBKCOLOR, 0, 	(int) (backColor.R | backColor.G<<8 | backColor.B <<16));  			
		}				
		
		// Sets Background color in an item
		internal void SetItemBkColorCtrl(Color col){	
			
			if (!bInitialised) return;					
			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETTEXTBKCOLOR, 0, 	(int) (col.R | col.G<<8 | col.B <<16));  			
		}				
		
		internal void insertItemsInCtrl()	{
			
			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_DELETEALLITEMS, 0,0);						
			Win32.SendMessage(Handle, (int)Msg.WM_SETREDRAW, 0,0);			
			for (int i=0; i<Items.Count; i++){	// Insert items		
					
				InsertItemInCtrl(Items[i]);							 						   																	
				
				for (int s=0; s<Items[i].SubItems.Count; s++)	// Insert subitems		
					SetItemInCtrl(Items[i].SubItems[s], s+1);
				
			}						
			Win32.SendMessage(Handle, (int)Msg.WM_SETREDRAW, 1,0);			
		}
		
		//						
		protected override void WndProc(ref Message m) 	{			
						
			if (!bInitialised) {
				
				bInitialised=true;							
				
				if (bAllowColumnReorder) ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_HEADERDRAGDROP, bAllowColumnReorder);
				if (bFullRowSelect)	ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_FULLROWSELECT, bFullRowSelect);
				if (bGridLines)	ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_GRIDLINES, bGridLines);
				if (bCheckBoxes) ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_CHECKBOXES, bCheckBoxes);
				if (bHoverSelection) ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_TRACKSELECT, bHoverSelection);							
				
				SetBkColorCtrl();				
				ItemActivationCtrl();
								
				for (int i=0; i<Columns.Count; i++)	// Insert columns
					InsertColumnInCtrl(Columns[i]);							
					
				insertItemsInCtrl();					
				
				// We need to setup the column autoresizing flags after all the columns and items
				// are inserted
				for (int i=0; i<Columns.Count; i++)	{							
					
					if (Columns[i].Width!=-1 && Columns[i].Width!=-2) continue;																
									
					Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETCOLUMNWIDTH, Columns[i].Index, 	
						Columns[i].Width);	
				}
			}
			
			
    		if (m.Msg==Msg.WM_NOTIFY)
			{				
				NMHDR nmhdr = (NMHDR)Marshal.PtrToStructure (m.LParam,	typeof (NMHDR));
			
				m.Result = IntPtr.Zero;				
				
				//Console.WriteLine("Notify->" + m);    											
				
				switch (nmhdr.code ) {
					case (int)ListViewNotifyMsg.LVN_COLUMNCLICK:					
					{																	
						NMLISTVIEW NmLstView = (NMLISTVIEW)Marshal.PtrToStructure (m.LParam,	typeof (NMLISTVIEW));						
						
						// Get get the index of the visual order, we have to we get the column unique ID to get its object						
						LVCOLUMN lvc = new LVCOLUMN();					
						lvc.mask = (int) ListViewColumnFlags.LVCF_SUBITEM;
						
						IntPtr lvcBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(lvc));
						Marshal.StructureToPtr(lvc, lvcBuffer, false);			
						Win32.SendMessage(Handle, (int)ListViewMessages.LVM_GETCOLUMNA, NmLstView.iSubItem, lvcBuffer);																
						lvc = (LVCOLUMN) Marshal.PtrToStructure(lvcBuffer,  typeof(LVCOLUMN));			   								
						
						ColumnHeader col = Columns.FromSerial(lvc.iSubItem);																							
						ColumnClickEventArgs eventArg = new ColumnClickEventArgs(col.Index);						 
						OnColumnClick(eventArg);
												
						Marshal.FreeHGlobal(lvcBuffer);			  									
						break;
					}
					
					case (int)ListViewNotifyMsg.LVN_BEGINLABELEDITA:					
					{
						Console.WriteLine("ListViewMessages.LVN_BEGINLABELEDITA");    											
						
						LVDISPINFO lvDispInfo = (LVDISPINFO)Marshal.PtrToStructure (m.LParam,	typeof (LVDISPINFO));						 												
						LabelEditEventArgs editEvent = new LabelEditEventArgs(lvDispInfo.item.iItem);						
						editEvent.CancelEdit = false;						
						
						OnBeforeLabelEdit(editEvent);
						
						if (editEvent.CancelEdit)
							m.Result = (System.IntPtr)1;
						else 
							m.Result = (System.IntPtr)0;									
						
						return;							
							
					}			
					
					case (int)ListViewNotifyMsg.LVN_ENDLABELEDITA:					
					{
						Console.WriteLine("ListViewMessages.LVN_ENDLABELEDITA");    											
						
						LVDISPINFO lvDispInfo = (LVDISPINFO)Marshal.PtrToStructure (m.LParam,	typeof (LVDISPINFO));						 												
						LabelEditEventArgs editEvent = new LabelEditEventArgs(lvDispInfo.item.iItem, lvDispInfo.item.pszText);						
						editEvent.CancelEdit = false;						
						
						OnAfterLabelEdit(editEvent);						
						
						if (editEvent.CancelEdit)
							m.Result = (System.IntPtr)0;
						else 
							m.Result = (System.IntPtr)1;									
							
						return;							
					}
					
					case (int)ListViewNotifyMsg.LVN_ITEMACTIVATE:
					{												
						//NMITEMACTIVATE is used instead of NMLISTVIEW in IE >= 0x400									  
						NMITEMACTIVATE NmItemAct = (NMITEMACTIVATE)Marshal.PtrToStructure (m.LParam,	typeof (NMITEMACTIVATE));								
						
						Console.WriteLine("ListViewMessages.LVN_ITEMACTIVATE " + NmItemAct.iItem + "sub: " + NmItemAct.iSubItem);    						
																	
						OnItemActivate(new EventArgs());
							
						break;
					}

					case (int)ListViewNotifyMsg.LVN_ITEMCHANGED:					
					{			
						
						NMLISTVIEW NmLstView = (NMLISTVIEW)Marshal.PtrToStructure (m.LParam,	typeof (NMLISTVIEW));								
						
						Console.WriteLine("ListViewMessages.LVN_ITEMCHANGED item:" + NmLstView.iItem + " sub: "+ NmLstView.iSubItem + "att:" +NmLstView.uChanged);    											
						
						// NOTE: Using the LVIS_SELECTED status does not work well when you use the control
						// to select diferent items.

						// Selected						
						selItemsCol.Clear();						
						for (int i=0; i < selItemIndexs.Count; i++)						
							Items[selItemIndexs[i]].Selected=false;				
						
						int nItem = Win32.SendMessage(Handle, (int)ListViewMessages.LVM_GETNEXTITEM, -1, (int) ListViewNotifyItem.LVNI_SELECTED);
												
						while (nItem!=-1)
						{
							selItemsCol.Add(nItem);								
							Items[nItem].Selected=true;				
							nItem = Win32.SendMessage(Handle, (int)ListViewMessages.LVM_GETNEXTITEM, nItem, (int) ListViewNotifyItem.LVNI_SELECTED);
						}
						
						// Check checked items
						int nItems = Win32.SendMessage(Handle, (int)ListViewMessages.LVM_GETITEMCOUNT, 0, 0);  					
					
						chkItemCol.Clear();
						for (int i=0; i < chkItemCol.Count; i++)						
							Items[i].Checked = false;				
						
						for (int i=0; i<nItems; i++){
							if (GetCheckStateInCtrl(i))	{
								chkItemCol.Add(i);
								Items[i].Checked = true;												   											
							}						
						}
						
						break;
					}							 

					// Note: Under WinXP we get HDN_ITEMCHANGEDW
					// seems that we change this using LVM_SETUNICODEFORMAT										
					case (int)HeaderCtrlNOtify.HDN_ITEMCHANGEDA:
					case (int)HeaderCtrlNOtify.HDN_ITEMCHANGEDW:
					{							
						
						NMHEADER NmHeader = (NMHEADER)Marshal.PtrToStructure (m.LParam,	typeof (NMHEADER));																																		
						HDITEM HDItem = (HDITEM)Marshal.PtrToStructure(NmHeader.pitem,	typeof (HDITEM));						
						
						if (((uint)HDItem.mask & (uint)HeaderItemFlags.HDI_HEIGHT)==(uint)HeaderItemFlags.HDI_HEIGHT)
						{
							Console.WriteLine("HDN_ITEMCHANGED item:" + NmHeader.iItem + " width: "  +HDItem.cxy); 							
							Columns[NmHeader.iItem].Width = HDItem.cxy;
						}
												   																										
						break;	
					}
						
					
					// Used to paint item colours and font
					case (int)NotificationMessages.NM_CUSTOMDRAW:
					{						
						
						NMLVCUSTOMDRAW LVNmCustom = (NMLVCUSTOMDRAW)Marshal.PtrToStructure (m.LParam,	typeof (NMLVCUSTOMDRAW));																																		
					
						switch(LVNmCustom.nmcd.dwDrawStage)    {							
						
						
						case (int)CustomDrawDrawStateFlags.CDDS_PREPAINT:														
							m.Result = (IntPtr)CustomDrawReturnFlags.CDRF_NOTIFYITEMDRAW;
							return;
					
						case (int)CustomDrawDrawStateFlags.CDDS_ITEMPREPAINT:						    						    	
						
							if (Items[(int)LVNmCustom.nmcd.dwItemSpec].UseItemStyleForSubItems)	{						
							  
								LVNmCustom.clrTextBk = (uint) Win32.RGB (Items[(int)LVNmCustom.nmcd.dwItemSpec].BackColor);						
								LVNmCustom.clrText = (uint) Win32.RGB (Items[(int)LVNmCustom.nmcd.dwItemSpec].ForeColor);						
								Marshal.StructureToPtr(LVNmCustom, m.LParam, false);								        				        
								m.Result =(IntPtr)CustomDrawReturnFlags.CDRF_NEWFONT;		    								
							}
							else
								m.Result =(IntPtr)CustomDrawReturnFlags.CDRF_NOTIFYSUBITEMDRAW;																		
							
							return;
								
						  case (int)(CustomDrawDrawStateFlags.CDDS_SUBITEM  | CustomDrawDrawStateFlags.CDDS_ITEMPREPAINT):						  						  
						  
						  	ListViewItem item = Items[(int)LVNmCustom.nmcd.dwItemSpec];						    
						  	
						    
						    if (LVNmCustom.iSubItem==0)    {						    	
						    	LVNmCustom.clrTextBk = (uint) Win32.RGB (Items[(int)LVNmCustom.nmcd.dwItemSpec].BackColor);						
						    	LVNmCustom.clrText = (uint) Win32.RGB (Items[(int)LVNmCustom.nmcd.dwItemSpec].ForeColor);
								Marshal.StructureToPtr(LVNmCustom, m.LParam, false);
						  		m.Result =(IntPtr)CustomDrawReturnFlags.CDRF_NEWFONT;									  									
							}
							else{						    																														
								
								ListViewItem.ListViewSubItem subItem;
								int nIdx = (int) LVNmCustom.iSubItem-1;																
								
								// We get an event by column even if the item is not inseted
								if (nIdx<item.SubItems.Count){ 
									subItem = item.SubItems[nIdx];																																																							
						    		LVNmCustom.clrTextBk = (uint) Win32.RGB (subItem.BackColor);												    							  								    	
						    		LVNmCustom.clrText = (uint) Win32.RGB (subItem.ForeColor);												    							  								    						    							    	
						    		Marshal.StructureToPtr(LVNmCustom, m.LParam, false);
						  			m.Result =(IntPtr)CustomDrawReturnFlags.CDRF_NEWFONT;									  									
						    	}								
						  	}													  	
						  	
						  	return;				
						  	
							
						default: 							
							break;
					    }						    
						
						break;
					}	
											
					default:
						break;
					}    		
			}
    			
   			CallControlWndProc (ref m);		
			
		}
		//start subclasses
		//
		// System.Windows.Forms.ListView.SelectedListViewItemCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//	 implemented by Jordi Mas i Hernàndez <jmas@softcatala.org>
		//
		//	It has been implemented really as an array of indexes of ListViewItemCollection
		//	to avoid information and item duplication in memory	/ Jordi
		//
		// (C) 2002-3 Ximian, Inc
		//
		// <summary>
		// </summary>

		public class SelectedListViewItemCollection :  IList, ICollection, IEnumerable {

			private ListView container = null;
			private ArrayList collection = new ArrayList();	

			//
			//  --- Constructor
			//			
			public SelectedListViewItemCollection(ListView owner) {				
				container = owner;
			}
			
			//
			//  --- Public Properties
			//			
			public int Count {
				get { return collection.Count; }
			}			
			public bool IsReadOnly {
				get { return collection.IsReadOnly; }
			}
						
			public ListViewItem this[int index] {				
								
				get {
					if (index<0 || index>=Count) throw  new  ArgumentOutOfRangeException();					
					return  container.Items[(int)collection[index]];
				}
				set { 	
					if (index<0 || index>=Count) throw  new  ArgumentOutOfRangeException();					
					collection[index] = value.Index;}				
				}
			
			//
			//  --- Internal Methods for the implementation
			//
			internal void Add (int nIndex) {
				collection.Add(nIndex);
			}			
			
			internal void Remove (int nIndex) {
				collection.Remove(nIndex);
			}			

			//
			//  --- Public Methods
			//
			
			public void Clear() {
				collection.Clear();
			}
			
			public bool Contains(ListViewItem item) {
				return collection.Contains(item);
			}
			
			public void CopyTo(Array dest, int index) {
				collection.CopyTo(dest, index);
			}
			
			public override bool Equals(object obj) {				
				return collection.Equals(obj);
			}

			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
			}
			
			public IEnumerator GetEnumerator() {
				return collection.GetEnumerator();
			}
			
			public int IndexOf(ListViewItem item) {
				return collection.IndexOf(item);
			}
			/// <summary>
			/// IList Interface implmentation.
			/// </summary>
			bool IList.IsReadOnly{
				get{return collection.IsReadOnly;}
			}
			bool IList.IsFixedSize{
				get{return collection.IsFixedSize;}
			}

			
			object IList.this[int index]{
				get { return collection[index]; }
				set { collection[index] = value; }
			}		

			void IList.Clear(){
				collection.Clear();
			}
		
			
			int IList.Add( object value){
				return collection.Add(value);
			}

			
			bool IList.Contains( object value){
				return collection.Contains(value);
			}

			
			int IList.IndexOf( object value){
				return collection.IndexOf(value);
			}

			
			void IList.Insert(int index, object value){
				collection.Insert(index, value);
			}

			
			void IList.Remove( object value){
				collection.Remove(value);
			}

			
			void IList.RemoveAt( int index){
				collection.RemoveAt(index);
			}
			// End of IList interface
			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count{
				get{
					throw new NotImplementedException ();
				}
			}
			bool ICollection.IsSynchronized{
				get{
					throw new NotImplementedException ();
				}
			}
			object ICollection.SyncRoot{
				get{
					throw new NotImplementedException ();
				}
			}
			void ICollection.CopyTo(Array array, int index){
				//FIXME:
			}
			// End Of ICollection
		}
		//
		// System.Windows.Forms.ListView.CheckedListViewItemCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//	 implemented by Jordi Mas i Hernàndez <jmas@softcatala.org>
		//		
		//	It has been implemented really as an array of indexes of ListViewItemCollection
		//	to avoid information and item duplication in memory	/ Jordi		
		// (C) 2002-3 Ximian, Inc
		//
		// <summary>
		// </summary>

		public class CheckedListViewItemCollection : IList, ICollection, IEnumerable { 
			
			private ListView container = null;
			private ArrayList collection = new ArrayList();	

			//
			//  --- Constructor
			//			
			public CheckedListViewItemCollection(ListView owner) 	{				
				container = owner;				
			}

			//
			//  --- Public Properties
			//			
			public int Count {
				get { return collection.Count; } 
			}
			
			public bool IsReadOnly {
				get {return true;}	
			} 			
			
			public virtual ListViewItem this[int index] {				
				get {
					if (index<0 || index>=Count) throw  new  ArgumentOutOfRangeException();					
					return  container.Items[(int)collection[index]];
				}
				set { 
					if (index<0 || index>=Count) throw  new  ArgumentOutOfRangeException();					
					collection[index] = value.Index;
				}	
			}
			
			//
			//  --- Internal Methods for the implementation
			//
			internal void Clear() {
				 collection.Clear();  
			}
			
			internal void Add (int nIndex) {
				collection.Add(nIndex);
			}				


			//
			//  --- Public Methods
			//			
			public bool Contains(ListViewItem item) {
				return collection.Contains(item);
			}
			
			[MonoTODO]
			public object CopyTo(Array dest, int index) {
				throw new NotImplementedException ();
			}
			
			public override bool Equals(object obj) {
				return collection.Equals(obj);
			}
			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
			}
			
			public IEnumerator GetEnumerator() {
				return collection.GetEnumerator();
			}
			
			public int IndexOf(ListViewItem item) {
				return collection.IndexOf(item);
			}
			/// <summary>
			/// IList Interface implementation.
			/// </summary>
			bool IList.IsReadOnly{
				get{return true;}
			}
			bool IList.IsFixedSize{
				get{return true;}
			}

			//[MonoTODO]
			object IList.this[int index]{
				get{
					throw new NotImplementedException ();
				}
				set{
					//FIXME:
				}
			}
		
			[MonoTODO]
			void IList.Clear(){
				//FIXME:
			}
		
			[MonoTODO]
			int IList.Add( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			bool IList.Contains( object value){
				throw new NotImplementedException ();
			}

			
			int IList.IndexOf( object value){
				return collection.IndexOf(value);
			}

			[MonoTODO]
			void IList.Insert(int index, object value){
				//FIXME:
			}

			[MonoTODO]
			void IList.Remove( object value){
				//FIXME:
			}

			[MonoTODO]
			void IList.RemoveAt( int index){
				//FIXME:
			}
			// End of IList interface
			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count{
				get{
					throw new NotImplementedException ();
				}
			}
			bool ICollection.IsSynchronized{
				get{
					throw new NotImplementedException ();
				}
			}
			object ICollection.SyncRoot{
				get{
					throw new NotImplementedException ();
				}
			}
			void ICollection.CopyTo(Array array, int index){
				//FIXME:
			}
			// End Of ICollection
		}
		//
		// System.Windows.Forms.ListView.ColumnHeaderCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//	 Implemented by Jordi Mas i Hernàndez (jmas@softcatala.org)
		//
		// (C) 2002-3 Ximian, Inc
		//
		// <summary>
		// </summary>
		public class ColumnHeaderCollection :  IList, ICollection, IEnumerable 
		{						
			private ListView container = null;
			private ArrayList collection = new ArrayList();	
			private	int	nUniqueSerial = 5000;

			//
			//  --- Constructor
			//			
			public ColumnHeaderCollection(ListView owner) {
				container = owner;
			}			
			
			//
			//  --- Public Properties
			//			
			public int Count{
				get { return collection.Count; }
			}
			
			public bool IsReadOnly {
				get { return collection.IsReadOnly; }				
			}
			
			public virtual ColumnHeader this[int index] {
				get { 
					if (index<0 || index>=Count) throw  new  ArgumentOutOfRangeException();					
					return (ColumnHeader) collection[index];
				}
				set { 
					if (index<0 || index>=Count) throw  new  ArgumentOutOfRangeException();					
					collection[index] = value;
				}				
			}
						
			//
			//  --- Private methods used by the implementation
			//			
			public ColumnHeader FromSerial(int nSerial)
			{
				for (int i=0; i < collection.Count; i++){
					ColumnHeader col = (ColumnHeader)collection[i];					
					if (col.Serial==nSerial) return col;					
				}
				
				return null;
			}
			
			// The indexes have to be re-calculated
			public void ReIndexCollection() {
				for (int i=0; i < collection.Count; i++)
				{
					ColumnHeader col = (ColumnHeader)collection[i];
					col.CtrlIndex=i;				
				}
				container.insertItemsInCtrl(); 
			}

			//
			//  --- Public Methods
			//
			
			public virtual int Add(ColumnHeader column) {								
				
				column.Serial = nUniqueSerial;
				column.CtrlIndex = Count;
				collection.Add(column);				
				nUniqueSerial++;
				
				container.InsertColumnInCtrl(column);
				return Count-1;				
			}
			
			public virtual ColumnHeader Add(string s, int witdh, HorizontalAlignment align) {				
				
				ColumnHeader column = new ColumnHeader();		
				
				/* The zero-based index of the column header within the ListView.ColumnHeaderCollection of the ListView control it is contained in.*/
				column.CtrlIndex = Count;
				column.Text = s;
				column.TextAlign = align;
				column.Width = witdh;						
				column.Container = container;
				column.Serial = nUniqueSerial;			
				collection.Add(column);								
				nUniqueSerial++;				
				container.InsertColumnInCtrl(column);				
				
				return column;				
			}
			
			public virtual void AddRange(ColumnHeader[] values) {
				for (int i=0; i<values.Length; i++)	{
					Add(values[i]);						
				}
			}
			
			public void Clear() {
				
				// Delete all columns
				int nRslt = 1;
				for (int n=0;  nRslt!=0; n++)
					nRslt = container.DeleteColumnInCtrl(0);
				
				collection.Clear();
			}			
			
			public bool Contains(ColumnHeader value) {
				return 	collection.Contains(value);			
			}
			
			public override bool Equals(object obj) {
				
				if(obj!= null && obj is ColumnHeaderCollection)	{
					ColumnHeaderCollection that = (ColumnHeaderCollection)obj;
					return (this.collection == that.collection);
				}
				
				return false;
			}
			
			[MonoTODO]
			public override int GetHashCode() 
			{
				//FIXME add our proprities
				return base.GetHashCode();
			}
			
			public IEnumerator GetEnumerator(){
				return collection.GetEnumerator();
			}
			
			public int IndexOf(ColumnHeader value) {
				return collection.IndexOf(value);
			}
			
			public void Insert(int index, ColumnHeader column) { 
								
				column.Serial = nUniqueSerial;								
				column.CtrlIndex = index;
				collection.Insert(index, column);				
				nUniqueSerial++;
				container.InsertColumnInCtrl(column);
				ReIndexCollection();
			}
			
			public void Insert(int index, string str, int witdh, HorizontalAlignment align) {
				
				ColumnHeader column = new ColumnHeader();									

				column.Text = str;
				column.TextAlign = align;
				column.Width = witdh;						
				column.Container = container;
				Insert(index, column);								
				
			}
			
			public virtual void Remove(ColumnHeader value) 	{
				RemoveAt(value.Index);
			}
					
			
			public virtual void RemoveAt(int index) 
			{					
				if (index>=Count) 				
					throw new ArgumentOutOfRangeException("Invalid value for array index");											
				
				container.DeleteColumnInCtrl(index); 
				collection.Remove(collection[index]);				
				
				ReIndexCollection();
			}
			
			/// <summary>
			/// IList Interface implmentation.
			/// </summary>
			bool IList.IsReadOnly{
				get{
					// We allow addition, removeal, and editing of items after creation of the list.
					return false;
				}
			}
			bool IList.IsFixedSize{
				get{
					// We allow addition and removeal of items after creation of the list.
					return false;
				}
			}

			object IList.this[int index]{
				[MonoTODO] get { throw new NotImplementedException (); }
				[MonoTODO] set { throw new NotImplementedException (); }
			}
		
			[MonoTODO]
			void IList.Clear(){
				//FIXME:
			}
		
			[MonoTODO]
			int IList.Add( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			bool IList.Contains( object value){
				throw new NotImplementedException ();
			}

			
			int IList.IndexOf( object value){
				return collection.IndexOf(value);
			}

			[MonoTODO]
			void IList.Insert(int index, object value){
				//FIXME:
			}

			[MonoTODO]
			void IList.Remove( object value){
				//FIXME:
			}

			[MonoTODO]
			void IList.RemoveAt( int index){
				//FIXME:
			}
			// End of IList interface
			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count{
				get{
					throw new NotImplementedException ();
				}
			}
			bool ICollection.IsSynchronized{
				get{
					throw new NotImplementedException ();
				}
			}
			object ICollection.SyncRoot{
				get{
					throw new NotImplementedException ();
				}
			}
			void ICollection.CopyTo(Array array, int index){
				//FIXME:
			}
			// End Of ICollection
			
			
		}
		//
		// System.Windows.Forms.ListView.ListViewItemCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//	 Implemented by Jordi i Hernàndez (jmas@softcatala.org)
		//
		// (C) 2002/3 Ximian, Inc
		//
		// <summary>
		// </summary>

		public class ListViewItemCollection :  IList, ICollection, IEnumerable 
		{
			private ListView container = null;
			private ArrayList collection = new ArrayList();	

			//
			//  --- Constructor
			//
			public ListViewItemCollection (ListView owner) 	{
				container = owner;
			}

			//
			//  --- Public Properties
			//			
			public int Count {
				get { return collection.Count; }
			}
			
			public bool IsReadOnly  {
				get { return collection.IsReadOnly; }
			}
			
			public virtual ListViewItem this [int index] {
				get { 
					
					if (index<0 || index>=Count) throw  new  ArgumentOutOfRangeException();					
					return (ListViewItem) collection[index];
				}
				set { 
					if (index<0 || index>=Count) throw new  ArgumentOutOfRangeException();					
					collection[index] = value;
				}		
			}

			//
			//  --- Public Methods
			//			
			public virtual ListViewItem Add (ListViewItem item) {
				
				Console.WriteLine("ListViewItem.Add " +  item.Text + " idx: " + item.Index);											
				
				item.CtrlIndex = Count;				
				item.Container = container;
				container.InsertItemInCtrl(item);		
				int nIdx = collection.Add(item);						
				return (ListViewItem)collection[nIdx];
				
			}
			[MonoTODO]
			public virtual ListViewItem Add (string str) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public virtual ListViewItem Add (string str, int val) {
				throw new NotImplementedException ();
			}
			
			public void AddRange(ListViewItem[] values) {
				
				for (int i=0; i<values.Length; i++)
				{
					container.InsertItemInCtrl(values[i]);
					Add(values[i]);	
				}
				
			}
			
			public void Clear() {
				collection.Clear();	
			}
			[MonoTODO]
			public bool Contains(ListViewItem item) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void CopyTo(Array dest, int index) {
				//FIXME:
			}
			[MonoTODO]
			public override bool Equals(object obj) {
				//FIXME:
				return base.Equals(obj);
			}

			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
			}
			
			public IEnumerator GetEnumerator() {
				return collection.GetEnumerator();
			}
			
			public int IndexOf(ListViewItem item) {
				return collection.IndexOf(item);
			}
			[MonoTODO]
			public virtual void Remove(ListViewItem item) {
				//FIXME:
			}
			
			public virtual void RemoveAt(int index) {
					
				if (index>=Count) 				
					throw new ArgumentOutOfRangeException("Invalid value for array index");											
				
				container.RemoveItemInCtrl(index); 
				collection.Remove(collection[index]);				
				
				// The indexes have to be re-calculated
				for (int i=0; i < collection.Count; i++)
				{
					ListViewItem item = (ListViewItem)collection[i];
					item.CtrlIndex=i;				
				}
				
				// Todo: Invalidate selection since indexes have changed
				
			}
			/// <summary>
			/// IList Interface implmentation.
			/// </summary>
			bool IList.IsReadOnly{
				get{
					// We allow addition, removeal, and editing of items after creation of the list.
					return false;
				}
			}
			bool IList.IsFixedSize{
				get{
					// We allow addition and removeal of items after creation of the list.
					return false;
				}
			}

			//[MonoTODO]
			object IList.this[int index]{
				get{
					throw new NotImplementedException ();
				}
				set{
					//FIXME:
				}
			}
		
			[MonoTODO]
			void IList.Clear(){
				//FIXME:
			}
		
			[MonoTODO]
			int IList.Add( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			bool IList.Contains( object value){
				throw new NotImplementedException ();
			}

			
			int IList.IndexOf( object value){
				return collection.IndexOf(value);
			}

			[MonoTODO]
			void IList.Insert(int index, object value){
				//FIXME:
			}

			[MonoTODO]
			void IList.Remove( object value){
				//FIXME:
			}

			[MonoTODO]
			void IList.RemoveAt( int index){
				//FIXME:
			}
			// End of IList interface
			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count{
				get{
					throw new NotImplementedException ();
				}
			}
			bool ICollection.IsSynchronized{
				get{
					throw new NotImplementedException ();
				}
			}
			object ICollection.SyncRoot{
				get{
					throw new NotImplementedException ();
				}
			}
			void ICollection.CopyTo(Array array, int index){
				//FIXME:
			}
			// End Of ICollection
		}
		//
		// System.Windows.Forms.ListView.SelectedIndexCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//	 Implemented by Jordi Mas i Hernàndez (jmas@softcatala.org)
		//
		//	 Implemented as a wrapper to SelectedListViewItemCollection
		//
		// (C) 2002-3 Ximian, Inc
		//

		// <summary>
		// </summary>

		public class SelectedIndexCollection :  IList, ICollection, IEnumerable {

			private ListView container = null;

			//
			//  --- Constructor
			//			
			public SelectedIndexCollection(ListView owner) {				
				container = owner;
			}

			//
			//  --- Public Properties
			//			
			public int Count {
				get { return container.SelectedItems.Count; }
			}
			
			public bool IsReadOnly {
				get { return container.SelectedItems.IsReadOnly; }
			}
			
			
			public int this [int index] {
				get {
					if (index<0 || index>=Count) throw  new  ArgumentOutOfRangeException();					
					return container.SelectedItems[index].Index;
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public bool Contains(ListView item) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void CopyTo(Array dest, int index) {
				//FIXME:
			}
			[MonoTODO]
			public override bool Equals(object obj) {
				//FIXME:
				return base.Equals(obj);
			}
			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
			}
			[MonoTODO]
			public IEnumerator GetEnumerator() {
				throw new NotImplementedException ();
			}
			
			public int IndexOf(int index) {
				throw new NotImplementedException ();
			}
			/// <summary>
			/// IList Interface implmentation.
			/// </summary>
			bool IList.IsReadOnly{
				get{
					// We allow addition, removeal, and editing of items after creation of the list.
					return false;
				}
			}
			bool IList.IsFixedSize{
				get{
					// We allow addition and removeal of items after creation of the list.
					return false;
				}
			}

			object IList.this[int index]{
				[MonoTODO] get { throw new NotImplementedException (); }
				[MonoTODO] set { throw new NotImplementedException (); }
			}
		
			[MonoTODO]
			void IList.Clear(){
				//FIXME:
			}
		
			[MonoTODO]
			int IList.Add( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			bool IList.Contains( object value){
				throw new NotImplementedException ();
			}

			
			int IList.IndexOf( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IList.Insert(int index, object value){
				//FIXME:
			}

			[MonoTODO]
			void IList.Remove( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IList.RemoveAt( int index){
				//FIXME:
			}
			// End of IList interface
			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count{
				get{
					throw new NotImplementedException ();
				}
			}
			bool ICollection.IsSynchronized{
				get{
					throw new NotImplementedException ();
				}
			}
			object ICollection.SyncRoot{
				get{
					throw new NotImplementedException ();
				}
			}
			void ICollection.CopyTo(Array array, int index){
				throw new NotImplementedException ();
			}
			// End Of ICollection
		}
		//
		// System.Windows.Forms.ListView.CheckedIndexCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//	 Implemented by Jordi Mas i Hernàndez (jmas@softcatala.org)
		//
		// (C) 2002-3 Ximian, Inc
		//
		// <summary>
		// </summary>
		public class CheckedIndexCollection :  IList, ICollection, IEnumerable { 
			
			private ListView container = null;

			//
			//  --- Constructor
			//			
			public CheckedIndexCollection(ListView owner) {
				container = owner;
			}

			//
			//  --- Public Properties
			//			
			public int Count {
				get{return container.CheckedItems.Count;}
			}
			
			public bool IsReadOnly {
				get {return container.CheckedItems.IsReadOnly;}
			}			
			
			public int this[int index] {
				get {
					if (index<0 || index>=Count) throw  new  ArgumentOutOfRangeException();					
					return container.CheckedItems[index].Index;
				}
			}

			//
			//  --- Public Methods
			//			
			public bool Contains(int checkedIndex) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public override bool Equals(object obj) {
				//FIXME:
				return base.Equals(obj);
			}
			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
			}
			[MonoTODO]
			public IEnumerator GetEnumerator() {
				throw new NotImplementedException ();
			}
			
			public int IndexOf(int checkedIndex) {
				throw new NotImplementedException ();
			}
			/// <summary>
			/// IList Interface implmentation.
			/// </summary>
			bool IList.IsReadOnly{
				get{
					// We allow addition, removeal, and editing of items after creation of the list.
					return false;
				}
			}
			bool IList.IsFixedSize{
				get{
					// We allow addition and removeal of items after creation of the list.
					return false;
				}
			}

			//[MonoTODO]
			object IList.this[int index]{
				get{
					throw new NotImplementedException ();
				}
				set{
					//FIXME:
				}
			}
		
			[MonoTODO]
			void IList.Clear(){
				//FIXME:
			}
		
			[MonoTODO]
			int IList.Add( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			bool IList.Contains( object value){
				throw new NotImplementedException ();
			}

			
			int IList.IndexOf( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IList.Insert(int index, object value){
				//FIXME:
			}

			[MonoTODO]
			void IList.Remove( object value){
				throw new NotImplementedException ();
			}

			[MonoTODO]
			void IList.RemoveAt( int index){
				throw new NotImplementedException ();
			}
			// End of IList interface
			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count{
				get{
					throw new NotImplementedException ();
				}
			}
			bool ICollection.IsSynchronized{
				get{
					throw new NotImplementedException ();
				}
			}
			object ICollection.SyncRoot{
				get{
					throw new NotImplementedException ();
				}
			}
			void ICollection.CopyTo(Array array, int index){
				//FIXME:
			}
			// End Of ICollection
		}
		//***********************************
		// Sub Class
		//***********************************
//		[MonoTODO]
//			// FIXME this sub class has many members that have not been stubbed out.
//			public class CheckedListViewItemCollection  {
//			CheckedListViewItemCollection(ListView owner){
//				throw new NotImplementedException ();
//			}
//		}
//		[MonoTODO]
//			// FIXME this sub class has many members that have not been stubbed out.
//			public class ColumnHeaderCollection  {
//			ColumnHeaderCollection(ListView owner){
//				throw new NotImplementedException ();
//			}
//
//		}
	}	
}

