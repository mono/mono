//
// System.Windows.Forms.ListBox.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Collections;
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

	public class ListBox : ListControl {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public ListBox() {
			
		}

		//
		//	 --- Protected Fields
		//
		protected int ColumnWidth_ = 0; // The columns will have default width
		protected bool IntegralHeight_ = true;
		protected ListBox.ObjectCollection	Items_ = null;
		protected DrawMode DrawMode_ = DrawMode.Normal;
		protected bool UseTabStops_ = false;
		
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
		public override int SelectedIndex {
			get {
				//FIXME:
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public ListBox.SelectedIndexCollection SelectedIndices {
			get {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
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
					Items_ = new ListBox.ObjectCollection( this);
				}
				return Items_;
			}
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
		public int FindString(string str) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int FindString(string str, int val) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int FindStringExact(string str) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int FindStringExact(string str, int val) {
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

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				// This is a child control, so it must have a parent for creation
				if( Parent != null) {
					CreateParams createParams = new CreateParams ();
					// CHECKME: here we must not overwrite window
					if( window == null) {
						window = new ControlNativeWindow (this);
					}

					createParams.Caption = Text;
					createParams.ClassName = "LISTBOX";
					createParams.X = Left;
					createParams.Y = Top;
					createParams.Width = Width;
					createParams.Height = Height;
					createParams.ClassStyle = 0;
					createParams.ExStyle = 0;
					createParams.Param = 0;
					createParams.Parent = Parent.Handle;
					createParams.Style = (int) (
						WindowStyles.WS_CHILD | 
						WindowStyles.WS_VISIBLE );
					createParams.Style |= (int) (LBS_.LBS_NOTIFY | 
						LBS_.LBS_HASSTRINGS );
					if( !IntegralHeight_) {
						createParams.Style |= (int)LBS_.LBS_NOINTEGRALHEIGHT;
					}
					if( UseTabStops_ ) {
						createParams.Style |= (int)LBS_.LBS_USETABSTOPS;
					}
					switch( DrawMode_){
						case DrawMode.OwnerDrawFixed:
							createParams.Style |= (int)LBS_.LBS_OWNERDRAWFIXED;
							break;
						case DrawMode.OwnerDrawVariable:
							createParams.Style |= (int)LBS_.LBS_OWNERDRAWVARIABLE;
							break;
					}
					// CHECKME : this call is commented because (IMHO) Control.CreateHandle supposed to do this
					// and this function is CreateParams, not CreateHandle
					// window.CreateHandle (createParams);
					return createParams;
				}
				return null;
			}		
		}

		internal void SafeAddItemToListControl( object item) {
			int res = Win32.SendMessage(Handle, (int)LB_.LB_ADDSTRING, 0, item.ToString());
		}
		
		internal void AddItemToListControl( object item) {
			if( IsHandleCreated) {
				SafeAddItemToListControl( item);	
			}
		}
		
		protected override void OnCreateControl ()
		{
			// Populate with Items
			if( IsHandleCreated) {
				foreach( object item in Items) {
					SafeAddItemToListControl(item);	
				}
				if( ColumnWidth_ != 0) {
					Win32.SendMessage( Handle, (int)LB_.LB_SETCOLUMNWIDTH, ColumnWidth_, 0);
				}
			}
		}

		protected override bool ReflectMessageHelper( ref Message m) {
			bool		result = false;
			switch (m.Msg) {
				case Msg.WM_MEASUREITEM: {
					MEASUREITEMSTRUCT mis = new MEASUREITEMSTRUCT();
					Win32.CopyMemory(ref mis, m.LParam, 24);
					MeasureItemEventArgs args = new MeasureItemEventArgs(CreateGraphics(),mis.itemID);
					args.ItemHeight = mis.itemHeight;
					args.ItemWidth = mis.itemWidth;
					OnMeasureItem( args);
					mis.itemHeight = args.ItemHeight;
					mis.itemWidth = args.ItemWidth;
					Win32.CopyMemory(m.LParam, ref mis, 24);
					result = true;
				}
				break;
				case Msg.WM_DRAWITEM: {
					DRAWITEMSTRUCT dis = new DRAWITEMSTRUCT();
					Win32.CopyMemory(ref dis, m.LParam, 48);
					Rectangle	rect = new Rectangle(dis.rcItem.left, dis.rcItem.top, dis.rcItem.right - dis.rcItem.left, dis.rcItem.bottom - dis.rcItem.top);
					DrawItemEventArgs args = new DrawItemEventArgs(Graphics.FromHdc(dis.hDC), Font,
						rect, dis.itemID, (DrawItemState)dis.itemState);
					OnDrawItem( args);
					Win32.CopyMemory(m.LParam, ref dis, 48);
					result = true;
				}
				break;
			}
			return result;
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
			throw new NotImplementedException ();
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
		protected override void WndProc(ref Message msg) {
			//FIXME:
			base.WndProc(ref msg);
		}

		//
		// <summary>
		// This is a subclass
		// </summary>

		public class SelectedObjectCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public SelectedObjectCollection(ListBox owner) {
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
			public object this[int index] {
				get {
					throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void CopyTo(Array dest, int index) {
				// FIXME:
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
			public int IndexOf(object selectedObject) {
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

			protected ListBox owner_ = null;
			protected ArrayList items_ = new ArrayList();
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
					return items_[index];
				}
				set {
					throw new NotImplementedException ();
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public int Add(object item) {
				int result = -1;
				if( item != null) {
					result = items_.Add(item);
				}
				return result;
			}
			[MonoTODO]
			public void AddRange(object[] items) {
				if( items != null) {
					items_.AddRange(items);
				}
			}
			[MonoTODO]
			public void AddRange(ListBox.ObjectCollection collection) {
				//FIXME:
			}
			[MonoTODO]
			public void Clear() {
				//FIXME:
			}
			[MonoTODO]
			public bool Contains(object value) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void CopyTo(object[] dest, int arrayIndex) {
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
				return items_.GetEnumerator();
			}
			[MonoTODO]
			public int IndexOf(object val) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Insert(int index, object item) {
				//FIXME:
			}
			[MonoTODO]
			public void Remove(object val) {
				//FIXME:
			}
			[MonoTODO]
			public void RemoveAt(int index) {
				//FIXME:
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

			//
			//  --- Constructor
			//
			[MonoTODO]
			public SelectedIndexCollection(ListBox owner) {
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
			public int this[int index] {
				get {
					throw new NotImplementedException ();
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public bool Contains(int selectedIndex) {
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
