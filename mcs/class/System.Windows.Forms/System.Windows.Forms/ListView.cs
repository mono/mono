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
		private bool bInicialised = false;
		private View viewMode = View.LargeIcon;
		private bool bAllowColumnReorder = false;
		private	SortOrder sortOrder = SortOrder.None;
		private bool bLabelEdit = false;
		private bool bFullRowSelect = false;
		private bool bGridLines = false;
		private bool bAutoArrange = true;
		private bool bLabelWrap = true;
		private bool bMultiSelect = true;
		private	Color backColor = SystemColors.Window;
		private	Color foreColor = SystemColors.WindowText;
		private SelectedListViewItemCollection selItemsCol = null;
		private	SelectedIndexCollection	selItemIndexs = null;
		private	ItemActivation	activation = ItemActivation.Standard; // todo: check default
				
		
		//
		//  --- Constructor
		//		
		public ListView()  : base()		{				
			
			itemsCollection = new ListViewItemCollection(this);			     
			columCol = new ColumnHeaderCollection(this);
			selItemsCol = new SelectedListViewItemCollection(this);
			selItemIndexs = new SelectedIndexCollection(this);
			
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
		[MonoTODO]
		public BorderStyle BorderStyle {
			get {
				//throw new NotImplementedException ();
				return 0;
			}
			set {
				//throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool CheckBoxes {
			get {
				throw new NotImplementedException ();
			}
			set {
				//throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public CheckedIndexCollection CheckedIndices {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public  CheckedListViewItemCollection CheckedItems {
			get {
				throw new NotImplementedException ();
			}
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
		[MonoTODO]
		public ColumnHeaderStyle HeaderStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				//throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool HideSelection {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool HoverSelection {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
		
		[MonoTODO]
		public bool Scrollable {
			get {				
				throw new NotImplementedException ();
			}
			set {
				//throw new NotImplementedException ();
			}
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
		public void ArrangeIcons(ListViewAlignment align) {
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
			
			if (!bInicialised) return;
			
			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_DELETEALLITEMS, 0,0);
						
			// Delete all columns
			int nRslt = 1;
			for (int n=0;  nRslt!=0; n++)
				nRslt = Win32.SendMessage(Handle, (int)ListViewMessages.LVM_DELETECOLUMN, 0,0);
			
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
		public Rectangle RetItemRect(int index) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Rectangle RetItemRect(int index, ItemBoundsPortion portion) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString() {
			
			return "List View object";			
		}

		//
		//  --- Public Events
		//
		[MonoTODO]
		public event LabelEditEventHandler AfterLabelEdit;
		public event LabelEditEventHandler BeforeLabelEdit;
		public event ColumnClickEventHandler ColumnClick;
		public event EventHandler ItemActivate;
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
		protected override bool IsInputKey(Keys keyData) {
			//FIXME:
			return base.IsInputKey(keyData);
		}

		
		protected virtual void  OnAfterLabelEdit(LabelEditEventArgs e) {
			
		}
		
		protected virtual void  OnBeforeLabelEdit(LabelEditEventArgs e) {
		
		}
		
		
		protected virtual void  OnColumnClick(ColumnClickEventArgs e) {
		
		}

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
			throw new NotImplementedException ();
		}

		
		protected virtual void  OnItemActivate(EventArgs ice) 	{
			
		}
		[MonoTODO]
		protected virtual void  OnItemCheck(ItemCheckEventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void  OnItemDrag(ItemDragEventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void  OnSelectedItemChanged(EventArgs e) {
			throw new NotImplementedException ();
		}

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
			throw new NotImplementedException ();
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
			
			Console.WriteLine("Sort value " + nRslt); 
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
			if (!bInicialised) return;
			
			LVCOLUMN lvc = new LVCOLUMN();					
			
			Console.WriteLine("Insert columns " + column.Text + " pos: " + column.Index+ " serial: "+  column.Serial);    						
					
			lvc.mask = (int)( ListViewColumnFlags.LVCF_FMT | ListViewColumnFlags.LVCF_WIDTH | ListViewColumnFlags.LVCF_TEXT | ListViewColumnFlags.LVCF_SUBITEM);
			lvc.iSubItem = column.Serial;
			lvc.pszText = column.Text;
			lvc.cx = column.Width;
			lvc.fmt = TextAlign(column.TextAlign);			
		
			IntPtr lvcBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(lvc));
  			Marshal.StructureToPtr(lvc, lvcBuffer, false);			
  			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_INSERTCOLUMNA, lvc.iSubItem, lvcBuffer);
  			Marshal.FreeHGlobal(lvcBuffer);
  			
		}	
		
		// Inserts an item in the control
		internal void InsertItemInCtrl(ListViewItem listViewItem)
		{			
			if (!bInicialised) return;
			
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
			if (!bInicialised) return;						
			Console.WriteLine("Deleting " + iIndex); 			
  			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_DELETEITEM, iIndex, 0); 						  			
		}	
		
		
		// Sets a subitem
		internal void SetItemInCtrl(ListViewItem.ListViewSubItem listViewSubItem, int nPos)
		{			
			if (!bInicialised) return;
			
			LVITEM item = new LVITEM();							
			
			item.pszText = listViewSubItem.Text;
			item.iItem = listViewSubItem.ListViewItem.Index;
			item.iSubItem = nPos;
			item.lParam = 0;
			item.mask = ListViewItemFlags.LVIF_TEXT;
			
			IntPtr liBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(item));
  			Marshal.StructureToPtr(item, liBuffer, false);			
  			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETITEMA, 0, liBuffer);
  			Marshal.FreeHGlobal(liBuffer);						
		}
		
		// Remove a column from the control
		internal void RemoveColumnInCtrl(int nIndex){	
			
			if (!bInicialised) return;
					
			Console.WriteLine("Delete column " + nIndex);    											
  			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_DELETECOLUMN, nIndex, 0);  			
		}			
		
		// Sets item activation
		
		
		internal void ItemActivationCtrl(){	
			
			ListViewExtendedFlags	flags = ItemActivationMsg(Activation);	// Thowns an exception
			
			if (!bInicialised) return;
					
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
			
			if (!bInicialised) return;
					
			if (bStatus)			
  				Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETEXTENDEDLISTVIEWSTYLE, (int)ExStyle, (int)ExStyle);  			
  			else
  				Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETEXTENDEDLISTVIEWSTYLE, (int)ExStyle, 0);  			
		}					
		
		
		// Sets Background color in control
		internal void SetBkColorCtrl(){	
			
			if (!bInicialised) return;					
			Win32.SendMessage(Handle, (int)ListViewMessages.LVM_SETBKCOLOR, 0, 	(int) (backColor.R | backColor.G<<8 | backColor.B <<16));  			
		}					
		
		
		//						
		protected override void WndProc(ref Message m) 	{			
						
			if (!bInicialised) {
				
				bInicialised=true;							
				
				if (bAllowColumnReorder) ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_HEADERDRAGDROP, bAllowColumnReorder);
				if (bFullRowSelect)	ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_FULLROWSELECT, bFullRowSelect);
				if (bGridLines)	ExtendedStyleCtrl(ListViewExtendedFlags.LVS_EX_GRIDLINES, bGridLines);
				
				SetBkColorCtrl();				
				ItemActivationCtrl();
								
				for (int i=0; i<Columns.Count; i++)	// Insert columns
					InsertColumnInCtrl(Columns[i]);							
					
				for (int i=0; i<Items.Count; i++){	// Insert items		
						
					InsertItemInCtrl(Items[i]);							 						   								
					
					for (int s=0; s<Items[i].SubItems.Count; s++)	// Insert subitems		
						SetItemInCtrl(Items[i].SubItems[s], s+1);
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
						
						EventArgs ice = new EventArgs();
					
						OnItemActivate(ice);
							
						break;
					}
					
					case (int)ListViewNotifyMsg.LVN_ITEMCHANGED:					
					{				
						
						NMLISTVIEW NmLstView = (NMLISTVIEW)Marshal.PtrToStructure (m.LParam,	typeof (NMLISTVIEW));								
						
						Console.WriteLine("ListViewMessages.LVN_ITEMCHANGED item:" + NmLstView.iItem + " sub: "+ NmLstView.iSubItem + "att:" +NmLstView.uChanged);    											
						
						//	TODO: An alternative implementation: use GetNexItem with the selected item flag
						//  Currently does not work with Ctrl selection						
						if ((NmLstView.uChanged & (uint)ListViewItemFlags.LVIF_STATE)==(uint)ListViewItemFlags.LVIF_STATE)
						{
							if ((NmLstView.uNewState & (uint)ListViewItemState.LVIS_SELECTED) == (uint)ListViewItemState.LVIS_SELECTED){
								Console.WriteLine("Selected! " +  NmLstView.iItem);
								selItemsCol.Add(NmLstView.iItem);								
							}
								
								
							if (((NmLstView.uNewState & (uint)ListViewItemState.LVIS_SELECTED)==0)
							&&((NmLstView.uOldState &  (uint)ListViewItemState.LVIS_SELECTED) !=(uint)ListViewItemState.LVIS_SELECTED)){
								Console.WriteLine("DeSelected! " +  NmLstView.iItem );
								selItemsCol.Remove(NmLstView.iItem);
							}
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
								
				get {return  container.Items[(int)collection[index]];}
				set { collection[index] = value.Index;}				
			}
			
			//
			//  --- Private Methods for the implementation
			//
			public void Add (int nIndex) {
				collection.Add(nIndex);
			}			
			
			public void Remove (int nIndex) {
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
			[MonoTODO]
			public IEnumerator GetEnumerator() {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public int IndexOf(ListViewItem item) {
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

			[MonoTODO]
			int IList.IndexOf( object value){
				throw new NotImplementedException ();
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
		// System.Windows.Forms.ListView.CheckedListViewItemCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//
		// (C) 2002 Ximian, Inc
		//
		// <summary>
		// </summary>

		public class CheckedListViewItemCollection : IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public CheckedListViewItemCollection(ListView owner) 
			{
				
			}

			//
			//  --- Public Properties
			//
			[MonoTODO]
			public int Count {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public bool IsReadOnly {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public ListViewItem this[int index] {
				get {
					throw new NotImplementedException ();
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public bool Contains(ListViewItem item) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public object CopyTo(Array dest, int index) {
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
			[MonoTODO]
			public int IndexOf(ListViewItem item) {
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

			[MonoTODO]
			int IList.IndexOf( object value){
				throw new NotImplementedException ();
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
			private	int	nUniqueSerial = 5000; // TODO: Change to 0

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
				get { return (ColumnHeader) collection[index];}
				set { collection[index] = value;}				
			}

			//
			//  --- Public Methods
			//
			
			public virtual int Add(ColumnHeader column) {
				
				if (column.Width==-1 ||column.Width==-2) column.Width=100; // TODO: Fix				
				column.Serial = nUniqueSerial;
				collection.Add(column);				
				return Count-1;
			}
			
			public virtual ColumnHeader Add(string s, int witdh, HorizontalAlignment align) {				
				// TODO: Witdh is Set to -1 to autosize the column header to the size of the largest subitem text 
				//in the column or -2 to autosize the column header to the size of the text of the column header.				
				//if (witdh==-1 ||witdh==-2) throw new NotImplementedException();		
				if (witdh==-1 ||witdh==-2) witdh=100; // TODO: Fix
								
				Console.WriteLine("ColumnHeader.Add " +  s);															
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
				
				return column;				
			}
			
			public virtual void AddRange(ColumnHeader[] values) {
				for (int i=0; i<values.Length; i++)
					collection.Add(values[i]);	
			}
			
			public void Clear() {
				collection.Clear();
			}
			
			[MonoTODO]
			public bool Contains(ColumnHeader value) 
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public override bool Equals(object obj) 
			{
				//FIXME:
				return base.Equals(obj);
			}
			[MonoTODO]
			public override int GetHashCode() 
			{
				//FIXME add our proprities
				return base.GetHashCode();
			}
			[MonoTODO]
			public IEnumerator GetEnumerator() 
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public int IndexOf(ColumnHeader value) 
			{
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Insert(int witdh, ColumnHeader value) {
				//FIXME:
			}
			[MonoTODO]
			public void Insert(int val1, string str, int val2, HorizontalAlignment align) 
			{
				//FIXME:
			}
			
			public virtual void Remove(ColumnHeader value) 
			{
				
			}
			
			public ColumnHeader FromSerial(int nSerial)
			{
				for (int i=0; i < collection.Count; i++)
				{
					ColumnHeader col = (ColumnHeader)collection[i];					
					if (col.Serial==nSerial) return col;					
				}
				
				return null;
			}
			
			public virtual void RemoveAt(int index) 
			{					
				if (index>=Count) 				
					throw new ArgumentOutOfRangeException("Invalid value for array index");											
				
				container.RemoveColumnInCtrl(index); 
				collection.Remove(collection[index]);				
				
				// The indexes have to be re-calculated
				for (int i=0; i < collection.Count; i++)
				{
					ColumnHeader col = (ColumnHeader)collection[i];
					col.CtrlIndex=i;				
				}
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

			[MonoTODO]
			int IList.IndexOf( object value){
				throw new NotImplementedException ();
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
				get { return (ListViewItem) collection[index];}
				set { collection[index] = value;}		
			}

			//
			//  --- Public Methods
			//			
			public virtual ListViewItem Add (ListViewItem item) {
				
				Console.WriteLine("ListViewItem.Add " +  item.Text + " idx: " + item.Index);											
				
				item.CtrlIndex = Count;				
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
					Add(values[i]);	
				
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
			[MonoTODO]
			public IEnumerator GetEnumerator() {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public int IndexOf(ListViewItem item) {
				throw new NotImplementedException ();
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

			[MonoTODO]
			int IList.IndexOf( object value){
				throw new NotImplementedException ();
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
				get {return container.SelectedItems[index].Index;}
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
			[MonoTODO]
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

			[MonoTODO]
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
		//
		// (C) 2002 Ximian, Inc
		//

		// <summary>
		// </summary>

		public class CheckedIndexCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public CheckedIndexCollection(ListView owner) {
				
			}

			//
			//  --- Public Properties
			//
			[MonoTODO]
			public int Count {
				get {
					throw new NotImplementedException ();
				}
				set {
					//FIXME:
				}
			}
			[MonoTODO]
			public bool IsReadOnly {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public int this[int index] {
				get {
					throw new NotImplementedException ();
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
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
			[MonoTODO]
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

			[MonoTODO]
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
