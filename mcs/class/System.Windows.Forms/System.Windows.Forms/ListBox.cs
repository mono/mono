//
// System.Windows.Forms.ListBox.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//   Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) 2002/3 Ximian, Inc
//
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class ListBox : ListControl {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public ListBox() {
			SubClassWndProc_ = true;
			BorderStyle_ = BorderStyle.Fixed3D;
			BackColor = SystemColors.Window;
			controlStyles_ |= ControlStyles.AllPaintingInWmPaint; 
		}

		//
		//	 --- Protected Fields
		//
		protected int ColumnWidth_ = 0; // The columns will have default width
		protected bool IntegralHeight_ = true;
		protected ListBox.ObjectCollection	Items_ = null;
		protected ListBox.SelectedIndexCollection SelectedIndices_ = null;
		protected ListBox.SelectedObjectCollection SelectedObjects_ = null;
		protected DrawMode DrawMode_ = DrawMode.Normal;
		protected bool UseTabStops_ = false;
		protected bool MultiColumn_ = false;
		int selectedIndex = -1;
		bool Sorted_ = false;
		internal int prevSelectedIndex = -1;
		BorderStyle	BorderStyle_;
		
		//
		//	 --- Public Fields
		//
		public const int DefaultItemHeight = 500;//just guessing FIXME // = ??;
		public const int NoMatches = 0 ;//just guessing FIXME // = ??;
		

		//
		//  --- Public Properties
		//

		[MonoTODO]
		public override Color BackColor {
			get {
				//FIXME:
				return base.BackColor;
			}
			set {
				//FIXME:
				base.BackColor = value;
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
				return BorderStyle_;				
			}
			set {
				if( BorderStyle_ != value) {
					BorderStyle_ = value;
					if( IsHandleCreated) {
						
					}
				}
			}
		}
		public bool MultiColumn {
			get {
				return MultiColumn_;
			}
			set {
				if( MultiColumn_ != value) {
					MultiColumn_ = value;
					RecreateHandle();
				}
			}
		}

		[MonoTODO]
		public override RightToLeft RightToLeft {
			get {
				//FIXME:
				return base.RightToLeft;
			}
			set {
				//FIXME:
				base.RightToLeft = value;
			}
		}
		[MonoTODO]
		public bool ScrollAlwaysVisible {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public bool HorizontalScrollbar {
			get {
				throw new NotImplementedException ();
			}
			set {
				// FIXME
			}
		}
		
		[MonoTODO]
		public override int SelectedIndex {
			get {
				if( IsHandleCreated) {
					return Win32.SendMessage(Handle, (int)ListBoxMessages.LB_GETCURSEL, 0, 0);
				}
				else {
					return selectedIndex;
				}
			}
			set {
				prevSelectedIndex = selectedIndex;
				if( selectedIndex != value) {
					//FIXME: set exception parameters
					selectedIndex = value;
					if( IsHandleCreated) {
						Win32.SendMessage(Handle, (int)ListBoxMessages.LB_SETCURSEL, selectedIndex, 0);
					}
					OnSelectedIndexChanged(new EventArgs());
				}
			}
		}
		
		[MonoTODO]
		public ListBox.SelectedIndexCollection SelectedIndices {
			get {
				if( SelectedIndices_ == null) {
					SelectedIndices_ = new ListBox.SelectedIndexCollection(this);
				}
				return SelectedIndices_;
			}
		}
		[MonoTODO]
		public object SelectedItem {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public ListBox.SelectedObjectCollection SelectedItems {
			get {
				if( SelectedObjects_ == null) {
					SelectedObjects_ = new ListBox.SelectedObjectCollection(this);
				}
				return SelectedObjects_;
			}
		}
		[MonoTODO]
		public virtual SelectionMode SelectionMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool Sorted {
			get {
				return Sorted_;
			}
			set {
				if( Sorted_ != value){
					Sorted_ = value;
					if( Sorted_) {
						Items_.SwitchToSortedStyle();
/*						
						object[] items = new object[Items.Count];
						Items.CopyTo(items, 0);
						Items.Clear();
						Items.AddRange(items);
*/
					}
				}
			}
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
		public int TopIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool UseTabStops {
			get {
				return UseTabStops_;
			}
			set {
				UseTabStops_ = value;
			}
		}

		[MonoTODO]
		public virtual DrawMode DrawMode {
			get {
				return DrawMode_;
			}
			set {
				DrawMode_ = value;
				// FIXME: change styles of Windows control/ recreate control
			}
		}

		public int ColumnWidth {
			get {
				return ColumnWidth_;
			}
			set {
				ColumnWidth_ = value;
			}
		}
		
		[MonoTODO]
		public virtual int ItemHeight {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		public bool IntegralHeight {
			get {
				return IntegralHeight_;
			}
			set {
				IntegralHeight_ = value;
			}
		}
		
		public ListBox.ObjectCollection Items {
			get {
				if( Items_ == null) {
					Items_ = CreateItemCollection();
				}
				return Items_;
			}
		}
		
		internal virtual void OnObjectCollectionChanged() {
			SelectedIndices_ = null;
		}
		
		//
		//  --- Public Methods
		//
		[MonoTODO]
		public void BeginUpdate() {
			//FIXME:
		}
		[MonoTODO]
		public void ClearSelected() {
			//FIXME:
		}
		[MonoTODO]
		public void EndUpdate() {
			//FIXME:
		}
		[MonoTODO]
		public int FindString(string s) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int FindString(string s, int startIndex) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int FindStringExact(string s) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int FindStringExact(string s, int startIndex) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int GetItemHeight(int index) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Rectangle GetItemRectangle(int index) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public bool GetSelected(int index) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int IndexFromPoint(Point pt) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int IndexFromPoint(int val1, int val2) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SetSelected(int index, bool val) {
			//FIXME:
		}
		[MonoTODO]
		public override string ToString() {
			//FIXME:
			return base.ToString();
		}

		//
		//  --- Public Events
		//
		[MonoTODO]
		public event DrawItemEventHandler DrawItem;
		[MonoTODO]
		public event MeasureItemEventHandler MeasureItem;
		public event EventHandler SelectedIndexChanged;
		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
					CreateParams createParams = base.CreateParams;
					createParams.ClassName = "LISTBOX";
					createParams.ExStyle = (int)WindowExStyles.WS_EX_CLIENTEDGE;
					createParams.Style = (int) (
						WindowStyles.WS_CHILD | 
						WindowStyles.WS_VISIBLE |
						WindowStyles.WS_CLIPSIBLINGS);
					createParams.Style |= (int) (ListBoxStyles.LBS_NOTIFY | 
						ListBoxStyles.LBS_HASSTRINGS );
					if( !IntegralHeight_) {
						createParams.Style |= (int)ListBoxStyles.LBS_NOINTEGRALHEIGHT;
					}
					if( UseTabStops_ ) {
						createParams.Style |= (int)ListBoxStyles.LBS_USETABSTOPS;
					}
					switch( DrawMode_){
						case DrawMode.OwnerDrawFixed:
							createParams.Style |= (int)ListBoxStyles.LBS_OWNERDRAWFIXED;
							break;
						case DrawMode.OwnerDrawVariable:
							createParams.Style |= (int)ListBoxStyles.LBS_OWNERDRAWVARIABLE;
							break;
					}
					if( MultiColumn_) {
						createParams.Style |= (int)ListBoxStyles.LBS_MULTICOLUMN | (int)WindowStyles.WS_HSCROLL;
					}
					else {
						createParams.Style |= (int)WindowStyles.WS_VSCROLL;
					}
					return createParams;
			}		
		}

		protected override void OnCreateControl ()
		{
			base.OnCreateControl();
		}

		[MonoTODO]
		protected override Size DefaultSize {
			get {
				return new Size(120,95);
			}
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected virtual ObjectCollection CreateItemCollection() {
			return new ListBox.ObjectCollection( this);
		}

		[MonoTODO]
		protected override void OnChangeUICues(UICuesEventArgs e) {
			//FIXME:
			base.OnChangeUICues(e);
		}
		[MonoTODO]
		protected override void OnDataSourceChanged(EventArgs e) {
			//FIXME:
			base.OnDataSourceChanged(e);
		}
		[MonoTODO]
		protected override void OnDisplayMemberChanged(EventArgs e) {
			//FIXME:
			base.OnDisplayMemberChanged(e);
		}

		[MonoTODO]
		protected virtual void OnDrawItem(DrawItemEventArgs e) {
			if( DrawItem != null) {
				DrawItem(this, e);
			}
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
			if( Items_ != null) {
				Items_.PopulateControl();
			}
			if( ColumnWidth_ != 0 && MultiColumn_) {
				Win32.SendMessage( Handle, (int)ListBoxMessages.LB_SETCOLUMNWIDTH, ColumnWidth_, 0);
			}
			
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e) {
			//FIXME:
			base.OnHandleDestroyed(e);
		}

		[MonoTODO]
		protected virtual void OnMeasureItem(MeasureItemEventArgs e) {
			if( MeasureItem != null) {
				MeasureItem(this, e);
			}
		}

		[MonoTODO]
		protected override void OnParentChanged(EventArgs e) {
			//FIXME:
			base.OnParentChanged(e);
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e) {
			//FIXME:
			base.OnResize(e);
		}
		[MonoTODO]
		protected override void OnSelectedIndexChanged(EventArgs e) {
			//FIXME:
			base.OnSelectedIndexChanged(e);
			if( SelectedIndexChanged != null) {
				SelectedIndexChanged(this, e);
			}
		}

		[MonoTODO]
		protected override void OnSelectedValueChanged(EventArgs e) {
			//FIXME:
			base.OnSelectedValueChanged(e);
		}

		protected override void RefreshItem(int index) {
			//FIXME:
		}

		public override void Refresh() { // .NET V1.1 Beta
			base.Refresh();
		}
		
		[MonoTODO]
		protected override void SetBoundsCore( int x, int y,  int width, int height,  BoundsSpecified specified) {
			//FIXME:
			base.SetBoundsCore(x, y, width, height, specified);
		}
		[MonoTODO]
		protected void Sort() {
			//FIXME:
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) {
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
				case Msg.WM_COMMAND: 
					switch(m.HiWordWParam) {
						case (uint)ListBoxNotifications.LBN_SELCHANGE:
							SelectedIndex = Win32.SendMessage(Handle, (int)ListBoxMessages.LB_GETCURSEL, 0, 0);
							m.Result = IntPtr.Zero;
							CallControlWndProc(ref m);
							break;
						default:
							CallControlWndProc(ref m);
							break;
					}
					break;
				default:
					base.WndProc(ref m);
					break;
			}
		}

		//
		// <summary>
		// This is a subclass
		// </summary>

		public class SelectedObjectCollection :  IList, ICollection, IEnumerable {

			ArrayList   collection_;
			ListBox		owner_;
			//
			//  --- Constructor
			//
			[MonoTODO]
			public SelectedObjectCollection(ListBox owner) {
				owner_ = owner;
				collection_ = owner_.Items.CreateSelectedObjectsList();
			}

			//
			//  --- Public Properties
			//
			[MonoTODO]
			public int Count {
				get {
					return collection_.Count;
				}
			}
			[MonoTODO]
			public bool IsReadOnly {
				get {
					return true;
				}
			}
			[MonoTODO]
			public object this[int index] {
				get {
					return collection_[index];
				}
				set {
					//FIXME:
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public bool Contains(object selectedObject) {
				return collection_.Contains(selectedObject);;
			}
			[MonoTODO]
			public void CopyTo(Array dest, int index) {
				collection_.CopyTo(dest, index);
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
				return collection_.GetEnumerator();
			}
			[MonoTODO]
			public int IndexOf(object selectedObject) {
				return collection_.IndexOf(selectedObject);
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
#if A
			object IList.this[int index]{
				get{
					throw new NotImplementedException ();
				}
				set{
					throw new NotImplementedException ();
				}
			}
#endif
		
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
				throw new NotImplementedException ();
			}
			// End Of ICollection
		}//End of subclass

		// <summary>
		// </summary>

		public class ObjectCollection : IList, ICollection {

			internal class ListBoxItem {
				public object theData_ = null;
				public string dataRepresentation_ = String.Empty;
				// FIXME: change those fields to flags
				//public bool wasSorted_ = false;
				//public bool wasAddedToControl_ = false;
				public bool IsAddedToControl_ = false;
				public bool Selected_ = false;
				public bool Checked_ = false;
				
				public ListBoxItem( object data, string representation) {
					theData_ = data;
					dataRepresentation_ = representation;
				}
			}
			
			internal class ListItemRepresentationComparer : IComparer {
				public ListItemRepresentationComparer() {
				}
	
				public int Compare(object x, object y) {
					ListBoxItem left = x as ListBoxItem;
					ListBoxItem right = y as ListBoxItem;
					if( left == null || right == null) {
						throw new ArgumentException();
					}
					return left.dataRepresentation_.CompareTo(right.dataRepresentation_);
				}
			}
/*
			internal class ListItemDataComparer : IComparer {
				public ListItemDataComparer() {
				}
	
				public int Compare(object x, object y) {
					ListBoxItem left = x as ListBoxItem;
					ListBoxItem right = y as ListBoxItem;
					if( left == null || right == null) {
						throw new ArgumentException();
					}
					return left.theData_.CompareTo(right.dataRepresentation_);
				}
			}
*/			
			internal ArrayList CreateSelectedIndicesList() {
				ArrayList result = new ArrayList();
				int ordinalNumber = 0;
				foreach( ListBoxItem lbi in items_) {
					if( lbi.Selected_) {
						result.Add(ordinalNumber);	
					}
					++ordinalNumber;
				}
				return result;
			}
			
			internal ArrayList CreateSelectedObjectsList() {
				ArrayList result = new ArrayList();
				foreach( ListBoxItem lbi in items_) {
					if( lbi.Selected_) {
						result.Add(lbi.theData_);	
					}
				}
				return result;
			}
			
			internal ArrayList CreateCheckedIndexList() {
				ArrayList result = new ArrayList();
				int ordinalNumber = 0;
				foreach( ListBoxItem lbi in items_) {
					if( lbi.Checked_) {
						result.Add(ordinalNumber);	
					}
					++ordinalNumber;
				}
				return result;
			}
			
			internal ArrayList CreateCheckedItemList() {
				ArrayList result = new ArrayList();
				foreach( ListBoxItem lbi in items_) {
					if( lbi.Checked_) {
						result.Add(lbi.theData_);	
					}
				}
				return result;
			}
			
			internal ListBoxItem getItemAt(int index) {
				return (ListBoxItem)items_[index];
			}
			
			internal void SwitchToSortedStyle() {
				if( owner_.IsHandleCreated) {
					Win32.SendMessage(owner_.Handle, (int)ListBoxMessages.LB_RESETCONTENT, 0, 0);
				}
				items_.Sort(new ListItemRepresentationComparer());
				if( owner_.IsHandleCreated) {
					foreach( ListBoxItem lbi in items_) {
						Win32.SendMessage(owner_.Handle, (int)ListBoxMessages.LB_ADDSTRING, 0, lbi.dataRepresentation_);
					}
				}
				owner_.OnObjectCollectionChanged();
			}
			
			internal void PopulateControl() {
				foreach( ListBoxItem lbi in items_) {
					Win32.SendMessage(owner_.Handle, (int)ListBoxMessages.LB_ADDSTRING, 0, lbi.dataRepresentation_);
					lbi.IsAddedToControl_ = true;
				}
			}
			
			internal void DumpItems() {
				int ordinalNumber = 0;
				foreach( ListBoxItem lbi in items_) {
					Console.WriteLine("ListBoxItem {0} order {1} checked {2}", lbi.dataRepresentation_,
					                  ordinalNumber, lbi.Checked_);
					++ordinalNumber;					
				}
			}
			
			internal class ListBoxItemEnumerator : IEnumerator {
				private IEnumerator containerEnum_ = null;
				
				public ListBoxItemEnumerator( IEnumerator containerEnum) {
					containerEnum_ = containerEnum;
				}
				
				public object Current {
					get {
						return ((ListBoxItem)containerEnum_.Current).theData_;
					}
				}
				
				public bool MoveNext() {
					return containerEnum_.MoveNext();
				}
				
				public void Reset() {
					containerEnum_.Reset();
				}
			}
			
			
			protected ListBox owner_ = null;
			protected ArrayList items_ = new ArrayList(); // has ListBoxItem
			
			//
			//  --- Constructor
			//
			[MonoTODO]
			public ObjectCollection(ListBox box) {
				owner_ = box;
			}
			[MonoTODO]
			public ObjectCollection(ListBox box, object[] objs) {
				owner_ = box;
				AddRange(objs);
			}

			//
			//  --- Public Properties
			//
			[MonoTODO]
			public int Count {
				get {
					return items_.Count;
				}
			}
			[MonoTODO]
			public bool IsReadOnly {
				get {
					// FIXME: Is it always not ReadOnly
					return false;
				}
			}
			[MonoTODO]
			public virtual object this[int index] {
				get {
					return ((ListBoxItem)items_[index]).theData_;
				}
				set {
					((ListBoxItem)items_[index]).theData_ = value;
					// FIXME: assing representation and sort if needed
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public int Add(object item) {
				string representation = owner_.getDisplayMemberOfObj(item);
				ListBoxItem  newItem = new ListBoxItem(item, representation);
				int result = items_.Add(newItem);
				if( owner_.Sorted) {
					items_.Sort(new ListItemRepresentationComparer());
					result = items_.BinarySearch(newItem);
					if( owner_.IsHandleCreated) {
						Win32.SendMessage(owner_.Handle, (int)ListBoxMessages.LB_INSERTSTRING, result, representation);
					}
					owner_.OnObjectCollectionChanged();
				}					
				else {
					if( owner_.IsHandleCreated) {
						Win32.SendMessage(owner_.Handle, (int)ListBoxMessages.LB_ADDSTRING, 0, representation);
					}
				}
				newItem.IsAddedToControl_ = true;
				return result;
			}
			
			[MonoTODO]
			public void AddRange(object[] items) {
				if( items == null) throw new ArgumentException();
				ListBoxItem[] newItems = new ListBoxItem[items.Length];
				int idx = 0;
				foreach( object obj in items ) {
					newItems[idx] = new ListBoxItem(obj, owner_.getDisplayMemberOfObj(obj));
					++idx;
				}
				items_.AddRange(newItems);
				if( owner_.Sorted) {
					items_.Sort(new ListItemRepresentationComparer());
					if( owner_.IsHandleCreated) {
						int index = 0;
						foreach( ListBoxItem lbi in items_) {
							if(!lbi.IsAddedToControl_){
								Win32.SendMessage(owner_.Handle, (int)ListBoxMessages.LB_INSERTSTRING, index, lbi.dataRepresentation_);
							}
							++index;
						}
					}
					owner_.OnObjectCollectionChanged();
				}
				else {
					if( owner_.IsHandleCreated) {
						foreach(ListBoxItem lbi in newItems) {
							Win32.SendMessage(owner_.Handle, (int)ListBoxMessages.LB_ADDSTRING, 0, lbi.dataRepresentation_);
							lbi.IsAddedToControl_ = true;
						}
					}
				}
				// FIXME: Add items to control
			}
			[MonoTODO]
			public void AddRange(ListBox.ObjectCollection collection) {
				//FIXME:
			}
			[MonoTODO]
			public void Clear() {
				//FIXME:
				items_.Clear();
				if( owner_.IsHandleCreated) {
					Win32.SendMessage(owner_.Handle, (int)ListBoxMessages.LB_RESETCONTENT, 0, 0);
				}
				owner_.OnObjectCollectionChanged();
			}
			[MonoTODO]
			public bool Contains(object value) {
				// FIXME: determine whether some of ListBoxItem has value as theData_
				foreach( ListBoxItem lbi in items_) {
					if( lbi.theData_ == value) {
						return true;
					}
				}
				return false;
			}
			[MonoTODO]
			public void CopyTo(object[] dest, int arrayIndex) {
				// FIXME: copy theData_ from ListBoxItem, not the ListBoxItems
				items_.CopyTo(dest, arrayIndex);
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
				return new ListBoxItemEnumerator(items_.GetEnumerator());
			}
			[MonoTODO]
			public int IndexOf(object val) {
				// FIXME: find ListBoxItem object which has the val as theData_
				int result = -1;
				int index = 0;
				foreach( ListBoxItem lbi in items_) {
					if( lbi.theData_ == val) {
						result = index;
						break;
					}
					++index;
				}
				return result;
			}
			[MonoTODO]
			public void Insert(int index, object item) {
				//FIXME:
			}
			[MonoTODO]
			public void Remove(object val) {
				// FIXME: use some sort of Comparer ?
				int pos = 0;
				foreach(ListBoxItem lbi in items_) {
					if( lbi.theData_ == val) {
						RemoveAt(pos);
						break;
					}
					++pos;
				}
			}
			[MonoTODO]
			public void RemoveAt(int index) {
				if( index < 0 || index >= items_.Count) {
					//FIXME: set exception parameters
					throw new ArgumentOutOfRangeException();
				}
				items_.RemoveAt(index);
				if( owner_.IsHandleCreated) {
					Win32.SendMessage( owner_.Handle, (int)ListBoxMessages.LB_DELETESTRING, index, 0); 
				}
				owner_.OnObjectCollectionChanged();
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

#if A
			//[MonoTODO]
			object IList.this[int index]{
				get{
					throw new NotImplementedException ();
				}
				set{
					throw new NotImplementedException ();
				}
			}
#endif
		
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
				throw new NotImplementedException ();
			}
			// End Of ICollection
		}//end of SubClass

		// <summary>
		// </summary>

		public class SelectedIndexCollection :  IList, ICollection, IEnumerable {

			ArrayList   collection_;
			ListBox		owner_;
			
			//
			//  --- Constructor
			//
			[MonoTODO]
			public SelectedIndexCollection(ListBox owner) {
				owner_ = owner;
				collection_ = owner_.Items.CreateSelectedIndicesList();
			}

			//
			//  --- Public Properties
			//
			[MonoTODO]
			public int Count {
				get {
					return collection_.Count;
				}
			}
			[MonoTODO]
			public bool IsReadOnly {
				get {
					return true;
				}
			}
			[MonoTODO]
			public int this[int index] {
				get {
					return (int)collection_[index];
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public bool Contains(int selectedIndex) {
				return collection_.Contains(selectedIndex);
			}
			[MonoTODO]
			public void CopyTo(Array dest, int index) {
				collection_.CopyTo(dest, index);
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
				return collection_.GetEnumerator();
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
				throw new NotImplementedException ();
			}
			// End Of ICollection

		}//End of subclass

		

	}
}
