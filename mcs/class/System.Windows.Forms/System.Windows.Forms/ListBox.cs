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
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public virtual DrawMode DrawMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
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
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "LISTBOX";
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
			//FIXME:
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
			//FIXME:
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

			//
			//  --- Constructor
			//
			[MonoTODO]
			public ObjectCollection(ListBox box) {
				//FIXME:
			}
			[MonoTODO]
			public ObjectCollection(ListBox box, object[] objs) {
				//FIXME:
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
			public virtual object this[int index] {
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
			public int Add(object item) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void AddRange(object[] items) {
				//FIXME:
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
				throw new NotImplementedException ();
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
