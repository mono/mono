//
// System.Windows.Forms.ComboBox.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//	 Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows combo box control.
   	/// </summary>

	[MonoTODO]
	public class ComboBox : ListControl {

		// private fields
		DrawMode drawMode;
		ComboBoxStyle dropDownStyle;
		bool droppedDown;
		bool integralHeight;
		bool sorted;
		Image backgroundImage;
		ControlStyles controlStyles;
		string text;
		int selectedLength;
		string selectedText;
		int selectedIndex;
		object selectedItem;
		int selecedStart;
		private ComboBox.ObjectCollection Items_ = null;
		int itemHeight_;
		int maxDropDownItems;

		bool updateing; // true when begin update has been called. do not paint when true;
		// --- Constructor ---
		public ComboBox() : base() 
		{
			selectedLength = 0;
			selectedText = "";
			selectedIndex = -1;
			selectedItem = null;
			selecedStart = 0;
			updateing = false;
			//controlStyles = null;
			drawMode = DrawMode.Normal;
			dropDownStyle = ComboBoxStyle.DropDownList;
			droppedDown = false;
			integralHeight = true;
			sorted = false;
			backgroundImage = null;
			text = "";
			Items_ = new ComboBox.ObjectCollection(this);
			itemHeight_ = 13;
			maxDropDownItems = 8;
			SubClassWndProc_ = true;

			Size = DefaultSize;
		}
		
		// --- Properties ---
		[MonoTODO]
		public override Color BackColor {
			get { 
				return base.BackColor;
			}
			set { 
				if(BackColor.A != 255){
					if(
						(controlStyles & ControlStyles.SupportsTransparentBackColor) != 
						ControlStyles.SupportsTransparentBackColor 
						){
						throw new 
							ArgumentOutOfRangeException("BackColor", BackColor, "Transparant background color not allowed.");
					}
				}
				base.BackColor = value;
			}
		}
		
		public override Image BackgroundImage {
			get {
				return backgroundImage; 
			}
			set { 
				backgroundImage = value;
			}
		}
		
		internal int getDropDownHeight() {
			// FIXME: use PreferredHeight instead of DefaultSize.Height ?
			// FIXME: those calculations probably wrong
			return DefaultSize.Height + (maxDropDownItems + 1) * itemHeight_ - (itemHeight_ / 2);
		}

		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				if( Parent != null) {
					CreateParams createParams = new CreateParams ();
					if( window == null) {
						window = new ControlNativeWindow (this);
					}

					createParams.Caption = Text;
					createParams.ClassName = "ComboBox";
					createParams.X = Left;
					createParams.Y = Top;
					createParams.Width = Width;
					if( DropDownStyle == ComboBoxStyle.Simple) {
						createParams.Height = Height;
					}
					else {
						createParams.Height = getDropDownHeight();
					}
					createParams.ClassStyle = 0;
					createParams.ExStyle = (int)( WindowExStyles.WS_EX_RIGHTSCROLLBAR | WindowExStyles.WS_EX_NOPARENTNOTIFY);
					createParams.Param = 0;
					createParams.Parent = Parent.Handle;
					createParams.Style = (int) (
						(int)WindowStyles.WS_CHILD | 
						(int)WindowStyles.WS_VISIBLE |
						(int)WindowStyles.WS_VSCROLL |
						(int)WindowStyles.WS_TABSTOP |
						(int)ComboBoxStyles.CBS_HASSTRINGS );

					switch( DrawMode){
						case DrawMode.OwnerDrawFixed:
							createParams.Style |= (int)ComboBoxStyles.CBS_OWNERDRAWFIXED;
							break;
						case DrawMode.OwnerDrawVariable:
							createParams.Style |= (int)ComboBoxStyles.CBS_OWNERDRAWVARIABLE;
							break;
					}

					switch(DropDownStyle) {
						case ComboBoxStyle.Simple:
							createParams.Style |= (int)ComboBoxStyles.CBS_SIMPLE;
							break;
						case ComboBoxStyle.DropDown:
							createParams.Style |= (int)ComboBoxStyles.CBS_DROPDOWN;
							break;
						case ComboBoxStyle.DropDownList:
							createParams.Style |= (int)ComboBoxStyles.CBS_DROPDOWNLIST;
							break;
					}
					if( !integralHeight) {
						createParams.Style |= (int)ComboBoxStyles.CBS_NOINTEGRALHEIGHT;
					}
/*
 *	Keep Control unsorted, but sort data in Items (ArrayList)
					if( sorted) {
						createParams.Style |= (int)ComboBoxStyles.CBS_SORT;
					}
*/
					return createParams;
				}
				return null;
			}		
		}
		
		protected override Size DefaultSize {
			get {
				return new Size(121,21);//correct size
			}
		}
/*
		public new Size Size {
			//FIXME: should we return client size or someother size???
			get {
				return base.Size;
			}
			set {

				if( dropDownStyle == ComboBoxStyle.Simple) {
					Size sz = value;
					sz.Height += maxDropDownItems * itemHeight_;
					base.Size = sz;
				}
				else {
					base.Size = value;
				}
			}
		}
*/
		public DrawMode DrawMode {
			get {
				return drawMode;
			}
			set {
				if( drawMode != value) {
					drawMode = value;
					RecreateHandle();
				}
			}
		}

		public ComboBoxStyle DropDownStyle {
			get {
				return dropDownStyle;
			}
			set {
				if( dropDownStyle != value) {
					dropDownStyle = value;
					RecreateHandle();
					OnDropDownStyleChanged( new EventArgs());
				}
			}
		}
		
		[MonoTODO]
		public int DropDownWidth {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		public bool DroppedDown {
			get { 
				return droppedDown;
			}
			set {
				droppedDown = value; 
			}
		}
		
		[MonoTODO]
		public override bool Focused {
			get {
				throw new NotImplementedException ();
			}
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
		
		public bool IntegralHeight {
			get {
				return integralHeight;
			}
			set {
				if( integralHeight != value) {
					integralHeight = value;
					if( IsHandleCreated) {
						if( integralHeight) {
							Win32.UpdateWindowStyle(Handle, (int)ComboBoxStyles.CBS_NOINTEGRALHEIGHT, 0);
						}
						else {
							Win32.UpdateWindowStyle(Handle, 0, (int)ComboBoxStyles.CBS_NOINTEGRALHEIGHT);
						}
					}
				}
			}
		}
		
		[MonoTODO]
		public int ItemHeight {
			get {
				return itemHeight_;
			}
			set {
				itemHeight_ = value;
				if( IsHandleCreated) {
					if( DrawMode != DrawMode.OwnerDrawVariable) {
						Win32.SendMessage(Handle, (int)ComboBoxMessages.CB_SETITEMHEIGHT, 0, itemHeight_);
					}
					else {
						for( int i = 0; i < Items.Count; i++) {
							Win32.SendMessage(Handle, (int)ComboBoxMessages.CB_SETITEMHEIGHT, i, itemHeight_);
						}
					}
					Win32.SendMessage(Handle, (int)ComboBoxMessages.CB_SETITEMHEIGHT, -1, itemHeight_);
				}
			}
		}
		
		[MonoTODO]
		public ComboBox.ObjectCollection Items {
			get { 
				return Items_; 
			}
		}
		
		[MonoTODO]
		public int MaxDropDownItems {
			get {
				return maxDropDownItems;
			}
			set {
				if( maxDropDownItems != value) {
					maxDropDownItems = value;
					if( DropDownStyle != ComboBoxStyle.Simple) {
						Height = getDropDownHeight();
					}
				}
			}
		}
		
		[MonoTODO]
		public int MaxLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:		
			}
		}
		
		[MonoTODO]
		public int PreferredHeight {
			get {
				return 20; //FIXME: this is the default, good as any?
			}
		}
	
		[MonoTODO]
		public override int SelectedIndex {
			get {
				if( IsHandleCreated) {
					return Win32.SendMessage(Handle, (int)ComboBoxMessages.CB_GETCURSEL, 0, 0);
				}
				else {
					return selectedIndex;
				}
			}
			set {
				if( selectedIndex != value) {
					//FIXME: set exception parameters
					if( value >= Items_.Count) {
						throw new ArgumentOutOfRangeException();
					}
					selectedIndex = value;
					if( IsHandleCreated) {
						Win32.SendMessage(Handle, (int)ComboBoxMessages.CB_SETCURSEL, selectedIndex, 0);
					}
					OnSelectedIndexChanged(new EventArgs());
				}
			}
		}
		
		[MonoTODO]
		public object SelectedItem {
			get {
				if( IsHandleCreated) {
					return Items_[Win32.SendMessage(Handle, (int)ComboBoxMessages.CB_GETCURSEL, 0, 0)];
				}
				else {
					return Items_[selectedIndex];
				}
			}
			set { 
				//FIXME:
			}
		}
		
		[MonoTODO]
		public string SelectedText {
			get { 
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		public int SelectionLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		[MonoTODO]
		public int SelectionStart {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		public bool Sorted {
			get {
				return sorted;
			}
			set {
				if( sorted != value) {
					sorted = value;
					if( IsHandleCreated) {
						if( sorted) {
							object[] items = new object[Items.Count];
							Items.CopyTo(items, 0);
							Items.Clear();
							Items.AddRange(items);
						}
					}
					selectedIndex = -1;
				}
			}
		}
		
		[MonoTODO]
		public override string Text {
			get {
				return text;
			}
			set {
				text = value;
			}
		}
		
		/// --- Methods ---
		/// internal .NET framework supporting methods, not stubbed out:

		internal void populateControl( ICollection items) {
			if( IsHandleCreated && items != null) {
				foreach( object obj in items) {
					// CHECKME : shall we check for null here or in Add/Insert functions
					if( obj != null) {
						Win32.SendMessage(Handle, (int)ComboBoxMessages.CB_ADDSTRING, 0, getDisplayMemberOfObj(obj));
					}
				}
			}
		}

		protected override void OnCreateControl () {
		}

		[MonoTODO]
		protected override void OnSelectedValueChanged(EventArgs e){ // .NET V1.1 Beta
			//FIXME:
			base.OnSelectedValueChanged(e);
		}

		/// - protected override void SetItemCore(int index,object value);
		[MonoTODO]
		protected virtual void AddItemsCore(object[] value) {
			//FIXME:		
		}
		
		[MonoTODO]
		public void BeginUpdate() 
		{
			updateing = true;
		}
		
		[MonoTODO]
		public void EndUpdate() 
		{
			updateing = false;
		}
		
		[MonoTODO]
		public int FindString(string s) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int FindString(string s,int startIndex) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int FindStringExact(string s) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int FindStringExact(string s,int startIndex) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public int GetItemHeight(int index) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool IsInputKey(Keys keyData) 
		{
			//FIXME:
			return base.IsInputKey(keyData);
		}
		
		/// [methods for events]
		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e) 
		{
			//FIXME:
			base.OnBackColorChanged(e);
		}
		
		[MonoTODO]
		protected override void OnDataSourceChanged(EventArgs e) 
		{
			//FIXME:
			base.OnDataSourceChanged(e);
		}
		
		[MonoTODO]
		protected override void OnDisplayMemberChanged(EventArgs e) 
		{
			//FIXME:
			base.OnDisplayMemberChanged(e);
		}
		
		[MonoTODO]
		protected virtual void OnDrawItem(DrawItemEventArgs e) 
		{
			if( DrawItem != null) {
				DrawItem(this, e);
			}
		}
		
		[MonoTODO]
		protected virtual void OnDropDown(EventArgs e) 
		{
			//FIXME:		
		}
		
		[MonoTODO]
		protected virtual void OnDropDownStyleChanged(EventArgs e) 
		{
			if( DropDownStyleChanged != null) {
				DropDownStyleChanged(this, e);
			}
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
			populateControl(Items_);
			Win32.SendMessage(Handle, (int)ComboBoxMessages.CB_SETCURSEL, selectedIndex, 0);
		}
		
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e) 
		{
			//FIXME:
			base.OnHandleDestroyed(e);
		}
		
		[MonoTODO]
		protected override void OnKeyPress(KeyPressEventArgs e) 
		{
			//FIXME:
			base.OnKeyPress(e);
		}
		
		[MonoTODO]
		protected virtual void OnMeasureItem(MeasureItemEventArgs e) 
		{
			if(MeasureItem != null) {
				MeasureItem(this, e);
			}
		}
		
		[MonoTODO]
		protected override void OnParentBackColorChanged(EventArgs e) 
		{
			//FIXME:
			base.OnParentBackColorChanged(e);
		}
		
		[MonoTODO]
		protected override void OnResize(EventArgs e) 
		{
			//FIXME:
			base.OnResize(e);
		}
		
		[MonoTODO]
		protected override void OnSelectedIndexChanged(EventArgs e) 
		{
			if( SelectedIndexChanged != null) {
				SelectedIndexChanged( this, e);
			}
		}
		
		[MonoTODO]
		protected virtual void OnSelectionChangeCommitted(EventArgs e) 
		{
			//FIXME:		
		}
		/// end of [methods for events]
		
		
		[MonoTODO]
		protected override void RefreshItem(int index) 
		{
			//FIXME:
			base.Refresh();
		}
		
		[MonoTODO]
		public void Select(int start,int length) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public void SelectAll() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void SetBoundsCore(int x,int y,int width,int height,BoundsSpecified specified) 
		{
			//FIXME:
			// If DropDownStyle == ComboBoxStyle.Simple, the heigth is a real window height - no control over it
			// else, 
			//		if Handle created - specify complete height, ComboBox-control will adjust window rectangle
			//			else - set the height to Default.
			if(DropDownStyle != ComboBoxStyle.Simple) {
				if( IsHandleCreated) {
					height = getDropDownHeight();
				}
				else {
					height = DefaultSize.Height;
				}
			}
			base.SetBoundsCore(x,y,width,height,specified);
			// FIXME: this is needed, otherwise painting is not correct
			if( dropDownStyle == ComboBoxStyle.Simple ) {
				Win32.InvalidateRect(Handle, IntPtr.Zero, 0);
				Win32.UpdateWindow(Handle);
			}
		}
		
		// for IList interface
		// FIXME not sure how to handle this
		//[MonoTODO]
		//protected override void SetItemsCore(IList value) 
		//{
		//	throw new NotImplementedException ();
		//}
		
		[MonoTODO]
		public override string ToString() 
		{
			//FIXME:
			return base.ToString();
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			switch (m.Msg) {
				case Msg.WM_MEASUREITEM: {
					MEASUREITEMSTRUCT mis = new MEASUREITEMSTRUCT();
					mis = (MEASUREITEMSTRUCT)Marshal.PtrToStructure(m.LParam, mis.GetType());
					MeasureItemEventArgs args = new MeasureItemEventArgs(CreateGraphics(),mis.itemID);
					args.ItemHeight = mis.itemHeight;
					args.ItemWidth = mis.itemWidth;
					OnMeasureItem( args);
					mis.itemHeight = args.ItemHeight;
					mis.itemWidth = args.ItemWidth;
					Marshal.StructureToPtr(mis, m.LParam, false);
					m.Result = (IntPtr)1;
				}
					break;
				case Msg.WM_DRAWITEM: {
					DRAWITEMSTRUCT dis = new DRAWITEMSTRUCT();
					dis = (DRAWITEMSTRUCT)Marshal.PtrToStructure(m.LParam, dis.GetType());
					Rectangle	rect = new Rectangle(dis.rcItem.left, dis.rcItem.top, dis.rcItem.right - dis.rcItem.left, dis.rcItem.bottom - dis.rcItem.top);
					DrawItemEventArgs args = new DrawItemEventArgs(Graphics.FromHdc(dis.hDC), Font,
						rect, dis.itemID, (DrawItemState)dis.itemState);
					OnDrawItem( args);
					//Marshal.StructureToPtr(dis, m.LParam, false);
					m.Result = (IntPtr)1;
				}
					break;
/*
				case Msg.WM_COMPAREITEM: {
					int i = 10;
				}
					break;
*/					
				case Msg.WM_COMMAND: 
					if( m.LParam != Handle) {
						base.WndProc(ref m);
					}
					else {
						switch(m.HiWordWParam) {
							case (uint)ComboBoxNotification.CBN_SELCHANGE:
								//OnSelectedIndexChanged(new EventArgs());
								SelectedIndex = Win32.SendMessage(Handle, (int)ComboBoxMessages.CB_GETCURSEL, 0, 0);
								m.Result = IntPtr.Zero;
								//CallControlWndProc(ref m);
								break;
							default:
								Console.WriteLine("ComboBox enter default");
								//CallControlWndProc(ref m);
								m.Result = IntPtr.Zero;
								Console.WriteLine("ComboBox exit default");
								break;
						}
					}
					break;
				default:
					base.WndProc(ref m);
					break;
			}
		}
		
	
		/// --- Button events ---
		/// commented out, cause it only supports the .NET Framework infrastructure
		[MonoTODO]
		public event DrawItemEventHandler DrawItem;
		
		[MonoTODO]
		public event EventHandler DropDown;
		
		[MonoTODO]
		public event EventHandler DropDownStyleChanged;
		
		[MonoTODO]
		public event MeasureItemEventHandler MeasureItem;
		
		/* only supports .NET framework
			[MonoTODO]
			public new event PaintEventHandler Paint;
		*/
		
		[MonoTODO]
		public event EventHandler SelectedIndexChanged;
		
		[MonoTODO]
		public event EventHandler SelectionChangeCommitted;
		
		/// --- public class ComboBox.ChildAccessibleObject : AccessibleObject ---
		/// the class is not stubbed, cause it's only used for .NET framework
		
		
		/// sub-class: ComboBox.ObjectCollection
		/// <summary>
		/// Represents the collection of items in a ComboBox.
		/// </summary>
		[MonoTODO]
		public class ObjectCollection : IList, ICollection, IEnumerable {
			private ArrayList collection_ = new ArrayList ();
			private ComboBox owner_ = null;
			
			/// --- ObjectCollection.constructor ---
			[MonoTODO]
			public ObjectCollection (ComboBox owner) {
				owner_ = owner;
			}
			
			/// --- ObjectCollection Properties ---
			[MonoTODO]
			public int Count {
				get { 
					return collection_.Count;
				}
			}
			
			[MonoTODO]
			public bool IsReadOnly {
				get { 
					return collection_.IsReadOnly;
				}
			}

			object IList.this[int index] {
				get { return collection_[index]; }
				set { collection_[index] = value; }
			}
						
			[MonoTODO]
			public object this[int index] {
				get { return collection_[index]; }
				set { collection_[index] = value; }
			}

			/// --- ICollection properties ---

			bool IList.IsFixedSize {
				[MonoTODO] get { return collection_.IsFixedSize; }
			}


			object ICollection.SyncRoot {
				get { return collection_.SyncRoot; }
			}
	
			bool ICollection.IsSynchronized {

				[MonoTODO] get { return collection_.IsSynchronized; }
			}
			
			/// --- methods ---
			/// --- ObjectCollection Methods ---
			/// Note: IList methods are stubbed out, otherwise IList interface cannot be implemented
			[MonoTODO]
			public int Add(object item) {
				// FIXME: not optimal 
				int idx = collection_.Add(item);
				if( owner_.Sorted) {
					ListControl.ListControlComparer cic = new ListControl.ListControlComparer(owner_);
					collection_.Sort(cic);
					idx = collection_.BinarySearch(item,cic);
					if( owner_.IsHandleCreated) {
						Win32.SendMessage(owner_.Handle, (int)ComboBoxMessages.CB_INSERTSTRING, idx, owner_.getDisplayMemberOfObj(item));
					}
				}
				else {
					if( owner_.IsHandleCreated) {
						Win32.SendMessage(owner_.Handle, (int)ComboBoxMessages.CB_ADDSTRING, 0, owner_.getDisplayMemberOfObj(item));
					}
				}
				return idx;
			}
			
			[MonoTODO]
			public void AddRange(object[] items) 
			{
				// FIXME: not optimal 
				foreach(object item in items) {
					Add(item);
				}
//				owner_.populateControl(items);
//				collection_.AddRange(items);
			}
			
			[MonoTODO]
			public void Clear() 
			{
				collection_.Clear();
				Win32.SendMessage(owner_.Handle, (int)ComboBoxMessages.CB_RESETCONTENT, 0, 0);
			}
			
			[MonoTODO]
			public bool Contains(object value) 
			{
				return collection_.Contains(value);
			}
			
			[MonoTODO]
			public void CopyTo(object[] dest,int arrayIndex) 
			{
				collection_.CopyTo(dest, arrayIndex);
			}
			
			/// for ICollection:
			[MonoTODO]
			void ICollection.CopyTo(Array dest,int index) 
			{
				collection_.CopyTo(dest, index);
			}
			
			[MonoTODO]
			public IEnumerator GetEnumerator() 
			{
				return collection_.GetEnumerator();
			}
			
			[MonoTODO]
			public int IndexOf(object value) 
			{
				return collection_.IndexOf(value);
			}
			
			[MonoTODO]
			public void Insert(int index,object item) 
			{
				//FIXME:		
			}
			
			[MonoTODO]
			public void Remove(object value) 
			{
				//FIXME:		
			}
			
			[MonoTODO]
			public void RemoveAt(int index) 
			{
				//FIXME:		
			}
		}  // --- end of ComboBox.ObjectCollection ---
	}
}
