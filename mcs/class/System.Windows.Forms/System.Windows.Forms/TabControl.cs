//
// System.Windows.Forms.TabControl
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
using System.Collections;
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

	public class TabControl : Control  {


		//
		//  --- Public Properties
		//
		[MonoTODO]
		public TabControl() {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public TabAlignment Alignment  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TabAppearance Appearance  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Color BackColor  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Image BackgroundImage  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Rectangle DisplayRectangle  {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TabDrawMode DrawMode  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Color ForeColor  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool HotTrack  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ImageList ImageList  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Size ItemSize  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool Multiline  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Point Padding  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int RowCount {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectedIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TabPage SelectedTab  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ShowToolTips  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TabSizeMode SizeMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int TabCount  {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public TabControl.TabPageCollection TabPages  {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override string Text  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		 
		// --- Public Methods
		
		[MonoTODO]
		public Rectangle GetTabRect(int index) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override string ToString() {
			throw new NotImplementedException ();
		}
		
		// --- Public Events
		
		[MonoTODO]
		public event DrawItemEventHandler DrawItem;
		
		// --- Protected Properties
		
		[MonoTODO]
		protected override CreateParams CreateParams  {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get {
				throw new NotImplementedException ();
			}
		}
		
		// --- Protected Methods
		
		[MonoTODO]
		protected override Control.ControlCollection CreateControlsInstance() {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void CreateHandle() {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override bool IsInputKey(Keys keyData) {
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
		protected override void OnKeyDown(KeyEventArgs ke) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnSelectedIndexChanged(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnStyleChanged(EventArgs e) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override bool ProcessKeyPreview(ref Message m) {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected void RemoveAll() {
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void WndProc(ref Message m) {
			throw new NotImplementedException ();
		}
		//FIXME DONT COMPILE
//		[MonoTODO]
//		public class ControlCollection {//: Control.ControlCollection {
//			//
//			// --- Public Methods
//			//
//			[MonoTODO]
//			public override void Add(Control value) {
//				throw new NotImplementedException ();
//			}
//			[MonoTODO]
//			public override void Remove(Control value) {
//				throw new NotImplementedException ();
//			}
//		}
		public class TabPageCollection : IList, ICollection, IEnumerable {
			//
			// --- Public Contructor
			//
			[MonoTODO]
			public TabPageCollection(TabControl owner) {
				throw new NotImplementedException ();
			}
			//
			// --- Public Properties
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
			public virtual TabPage this[int index] {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}
			
			//--- Public Methods
			
			[MonoTODO]
			public void Add(TabPage value) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void AddRange(TabPage[] pages) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public virtual void Clear() {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public bool Contains(TabPage page) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public IEnumerator GetEnumerator() {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public int IndexOf(TabPage page) {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			public void Remove(TabPage value) {
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
		}
	}
}

