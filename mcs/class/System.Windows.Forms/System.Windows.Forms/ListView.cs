//
// System.Windows.Forms.ListView.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
// (C) 2002/3 Ximian, Inc
//
using System.Collections;
using System;
using System.Drawing;
namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>
	public class ListView : Control {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public ListView() {
			
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public ItemActivation Activation {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public ListViewAlignment Alignment {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool AllowColumnReorder {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool AutoArrange {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
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
		public BorderStyle BorderStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool CheckBoxes {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
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
		[MonoTODO]
		public ColumnHeaderCollection Columns {
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
		[MonoTODO]
		public bool FullRowSelect {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool GridLines {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public ColumnHeaderStyle HeaderStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool HideSelection {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool HoverSelection {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public ListView.ListViewItemCollection Items {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool LabelEdit {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool LabelWrap {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public ImageList LargeImageList {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool MultiSelect {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public bool Scrollable {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public ListView.SelectedIndexCollection SelectedIndices {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ListView.SelectedListViewItemCollection SelectedItems {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ImageList SmallImageList {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public SortOrder Sorting {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public ImageList StateImageList {
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
		public View View {
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
		public void ArrangeIcons() {
			//FIXME:
		}
		[MonoTODO]
		public void ArrangeIcons(ListViewAlignment align) {
			//FIXME:
		}

		[MonoTODO]
		public void BeginUpdate() {
			//FIXME:
		}
		[MonoTODO]
		public void Clear() {
			//FIXME:
		}
		[MonoTODO]
		public void EndUpdate() {
			//FIXME:
		}
		[MonoTODO]
		public void EnsureVisible(int index) {
			//FIXME:
		}
		[MonoTODO]
		public ListViewItem GetItemAt(int x, int y) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Rectangle RetItemRect(int val) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Rectangle RetItemRect(int val, ItemBoundsPortion portion) {
			throw new NotImplementedException ();
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
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "LISTVIEW";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				//			createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE);
				window.CreateHandle (createParams);
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

		[MonoTODO]
		protected virtual void  OnAfterLabelEdit(LabelEditEventArgs e) {
			//FIXME:
		}
		[MonoTODO]
		protected virtual void  OnBeforeLabelEdit(LabelEditEventArgs e) {
			//FIXME:
		}
		[MonoTODO]
		protected virtual void  OnColumnClick(ColumnClickEventArgs e) {
			//FIXME:
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
			//FIXME:
		}

		[MonoTODO]
		protected virtual void  OnItemActivate(EventArgs e) {
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
		[MonoTODO]
		protected virtual void  OnSelectedItemChanged(EventArgs e) {
			//FIXME:
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
			//FIXME:
		}
		[MonoTODO]
		protected override void WndProc(ref Message m) {
			//FIXME:
			base.WndProc(ref m);
		}
		//start subclasses
		//
		// System.Windows.Forms.ListView.SelectedListViewItemCollection.cs
		//
		// Author:
		//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
		//
		// (C) 2002 Ximian, Inc
		//
		// <summary>
		// </summary>

		public class SelectedListViewItemCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public SelectedListViewItemCollection(ListView owner) {
				
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
			public void Clear() {
				//FIXME:
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
			public CheckedListViewItemCollection(ListView owner) {
				
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
		//
		// (C) 2002 Ximian, Inc
		//
		// <summary>
		// </summary>

		public class ColumnHeaderCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public ColumnHeaderCollection(ListView owner) {
				
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
			public virtual ColumnHeader this[int index] {
				get {
					throw new NotImplementedException ();
				}
			}

			//
			//  --- Public Methods
			//
			[MonoTODO]
			public virtual int Add(ColumnHeader value) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public virtual ColumnHeader Add(string s, int b, HorizontalAlignment align) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public virtual void AddRange(ColumnHeader[] values) {
				//FIXME:
			}
			[MonoTODO]
			public void Clear() {
				//FIXME:
			}
			[MonoTODO]
			public bool Contains(ColumnHeader value) {
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
			public int IndexOf(ColumnHeader value) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Insert(int b, ColumnHeader value) {
				//FIXME:
			}
			[MonoTODO]
			public void Insert(int val1, string str, int val2, HorizontalAlignment align) {
				//FIXME:
			}
			[MonoTODO]
			public virtual void Remove(ColumnHeader value) {
				//FIXME:
			}
			[MonoTODO]
			public virtual void RemoveAt(int index) {
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
		//
		// (C) 2002 Ximian, Inc
		//
		// <summary>
		// </summary>

		public class ListViewItemCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public ListViewItemCollection (ListView owner) {
				
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
			public bool IsReadOnly  {
				get {
					throw new NotImplementedException ();
				}
			}
			[MonoTODO]
			public virtual ListViewItem this [int index] {
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
			public virtual ListViewItem Add (ListViewItem item) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public virtual ListViewItem Add (string str) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public virtual ListViewItem Add (string str, int val) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void AddRange(ListViewItem[] values) {
				//FIXME:
			}
			[MonoTODO]
			public void Clear() {
				//FIXME:
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
			[MonoTODO]
			public virtual void RemoveAt(int index) {
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
		//
		// (C) 2002 Ximian, Inc
		//

		// <summary>
		// </summary>

		public class SelectedIndexCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public SelectedIndexCollection(ListView owner) {
				
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
			public int this [int index] {
				get {
					throw new NotImplementedException ();
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
