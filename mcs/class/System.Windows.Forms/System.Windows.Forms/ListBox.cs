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
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
		[MonoTODO]
		public override RightToLeft RightToLeft {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ScrollAlwaysVisible {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override int SelectedIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Sorted {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool UseTabStops {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual DrawMode DrawMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual int ItemHeight {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		//
		//  --- Public Methods
		//
		[MonoTODO]
		public void BeginUpdate() {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void ClearSelected() {
			throw new NotImplementedException ();
		}
		//inherited
		//public override void Dispose() {
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public void EndUpdate() {
			throw new NotImplementedException ();
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
		//inherited
		//public void Invalidate() {
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(bool val) {
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Rectangle rect) {
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Region reg) {
		//	throw new NotImplementedException ();
		//}
		//public void Invalidate(Region reg, bool val) {
		//	throw new NotImplementedException ();
		//}
		//public object Invoke(Delegate del) {
		//	throw new NotImplementedException ();
		//}
		//public object Invoke(Delegate del, object[] objs) {
		//	throw new NotImplementedException ();
		//}
		//public void PerformLayout() {
		//	throw new NotImplementedException ();
		//}
		//public void PerformLayout(Control ctl, string str ) {
		//	throw new NotImplementedException ();
		//}
		//public void ResumeLayout() {
		//	throw new NotImplementedException ();
		//}
		//public void ResumeLayout(bool val) {
		//	throw new NotImplementedException ();
		//}
		//public void Scale(float val) {
		//	throw new NotImplementedException ();
		//}
		//public void Scale(float val1, float val2) {
		//	throw new NotImplementedException ();
		//}
		//public override void Select(bool val1, bool val2) {
		//	throw new NotImplementedException ();
		//}
		//public override void SetBounds(int val1, int val2, int val3, int val4) {
		//	throw new NotImplementedException ();
		//}
		//public override void SetBounds(int val1, int val2, int val3, int val4, BoundsSpecified bounds) {
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		public void SetSelected(int index, bool val) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString() {
			throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected virtual ObjectCollection CreateItemCollection() {
			throw new NotImplementedException ();
		}

		//inherited
		//protected override void Dispose(bool val1) {
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected override void OnChangeUICues(UICuesEventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnDataSourceChanged(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnDisplayMemberChanged(EventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDrawItem(DrawItemEventArgs e) {
			throw new NotImplementedException ();
		}


		[MonoTODO]
		protected override void OnFontChanged(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnMeasureItem(MeasureItemEventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnParentChanged(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnSelectedIndexChanged(EventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnSelectedValueChanged(EventArgs e) {
			throw new NotImplementedException ();
		}

		protected override void RefreshItem(int index) {
			throw new NotImplementedException ();
		}

		public override void Refresh() { // .NET V1.1 Beta
			base.Refresh();
		}
		
		//Inherited
		//protected ContentAlignment RtlTranslateAlignment(ContentAlignment align) {
		//	throw new NotImplementedException ();
		//}
		//protected HorizontalAlignment RtlTranslateAlignment(HorizontalAlignment align) {
		//	throw new NotImplementedException ();
		//}
		//protected LeftRightAlignment RtlTranslateAlignment(LeftRightAlignment align) {
		//	throw new NotImplementedException ();
		//}
		//protected virtual void Select(bool val1, bool val2)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected override void SetBoundsCore( int x, int y,  int width, int height,  BoundsSpecified specified) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected void Sort() {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void WndProc(ref Message mgs) {
			throw new NotImplementedException ();
		}

		//
		// <summary>
		//	This is only a template.  Nothing is implemented yet.
		// This is a subclass
		// </summary>

		public class SelectedObjectCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public SelectedObjectCollection(ListBox owner) {
				throw new NotImplementedException ();
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
					throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public override bool Equals(object o) {
				throw new NotImplementedException ();
			}
			//inherited
			//public static bool Equals(object o1, object o2) {
			//	throw new NotImplementedException ();
			//}
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			// End Of ICollection
		}//End of subclass

		// <summary>
		//	This is only a template.  Nothing is implemented yet.
		//
		// </summary>

		public class ObjectCollection : IList, ICollection {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public ObjectCollection(ListBox box) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public ObjectCollection(ListBox box, object[] objs) {
				throw new NotImplementedException ();
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
					throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void AddRange(ListBox.ObjectCollection collection) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Clear() {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public bool Contains(object value) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void CopyTo(object[] dest, int arrayIndex) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public override bool Equals(object o) {
				throw new NotImplementedException ();
			}
			//inherited
			//[Serializable]
			//[ClassInterface(ClassInterfaceType.AutoDual)]
			//public static bool Equals(object objA, object objB) {
			//	throw new NotImplementedException ();
			//}
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
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Remove(object val) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void RemoveAt(int index) {
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			// End Of ICollection
		}//end of SubClass

		// <summary>
		//	This is only a template.  Nothing is implemented yet.
		//
		// </summary>

		public class SelectedIndexCollection :  IList, ICollection, IEnumerable {

			//
			//  --- Constructor
			//
			[MonoTODO]
			public SelectedIndexCollection(ListBox owner) {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public override bool Equals(object o) {
				throw new NotImplementedException ();
			}
			//inherited
			//public static bool Equals(object objA, object objB) {
			//	throw new NotImplementedException ();
			//}
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
					throw new NotImplementedException ();
				}
			}
		
			[MonoTODO]
			void IList.Clear(){
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
			// End Of ICollection

		}//End of subclass

		

	}
}
