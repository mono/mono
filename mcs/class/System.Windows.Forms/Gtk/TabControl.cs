//
//		System.Windows.Forms.TabControl
//
//		Author:
//			Alberto Fernandez		(infjaf00@yahoo.es)
//


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms{

	public class TabControl : Control {
		[MonoTODO]
		public TabControl(){
		}

		//InvalidEnumArgumentException
		// Pred = top
		[MonoTODO]
		public TabAlignment Alignment {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public TabAppearance Appearance {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public override Color BackColor {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public override Image BackgroundImage {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		protected override CreateParams CreateParams {
			get { throw new NotImplementedException(); }
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get { return new Size (200,100); }
		}
		[MonoTODO]
		public override Rectangle DisplayRectangle {
			get { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public TabDrawMode DrawMode {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public override Color ForeColor {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public bool HotTrack {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public ImageList ImageList {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public Size ItemSize {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public bool Multiline {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public Point Padding {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }		
		}
		[MonoTODO]
		public int RowCount {
			get { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public int SelectedIndex {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }		
		}
		[MonoTODO]
		public TabPage SelectedTab {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public bool ShowToolTips {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }		
		}
		[MonoTODO]
		public TabSizeMode SizeMode {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }		
		}
		[MonoTODO]
		public int TabCount {
			get { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public TabControl.TabPageCollection TabPages {
			get { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public override string Text {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		protected override Control.ControlCollection CreateControlsInstance(){
			return new TabControl.ControlCollection(this);
		}
		[MonoTODO]
		protected override void CreateHandle(){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void Dispose(bool disposing){
			throw new NotImplementedException();
		}

		// No usar
		public Control GetControl(int index){
			throw new NotImplementedException();
		}
		protected virtual object[] GetItems(){
			throw new NotImplementedException();
		}
		protected virtual object[] GetItems(Type baseType){
			throw new NotImplementedException();
		}

		[MonoTODO]
		public Rectangle GetTabRect(int index){
			throw new NotImplementedException();
		}

		// No usar
		protected string GetToolTipText(object item){
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override bool IsInputKey(Keys keyData){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected virtual void OnDrawItem(DrawItemEventArgs e){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void OnKeyDown(KeyEventArgs ke){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected virtual void OnSelectedIndexChanged(EventArgs e){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void OnStyleChanged(EventArgs e){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override bool ProcessKeyPreview(ref Message m){
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected void RemoveAll(){
			throw new NotImplementedException();
		}
		
		public override string ToString(){
			return "System.Windows.Forms.TabContro, TabPages.Count: " +
				this.TabPages.Count;
			//System.Windows.Forms.TabControl, TabPages.Count: 0
		}

		// No usar
		protected void UpdateTabSelection( bool uiselected){
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void WndProc(ref Message m){
			throw new NotImplementedException();
		}


		public new event EventHandler BackColorChanged;
		public new event EventHandler BackgroundImageChanged;
		public event DrawItemEventHandler DrawItem;
		public new event EventHandler ForeColorChanged;
		public new event PaintEventHandler Paint;
		public event EventHandler SelectedIndexChanged;
		public new event EventHandler TextChanged;


		public new class ControlCollection : Control.ControlCollection {

			public ControlCollection ( TabControl owner ): base( owner ){ }

			public override void Add( Control c ) {
				//if ( !( c is TabPage ) ) {
				//	throw new ArgumentException();
				//}
				//base.Add(c);
				//if ( owner.IsHandleCreated )
				//	((TabControl) owner).addPage ( c, Count - 1);
			}

			public override void Clear () {
				//base.Clear ( );
				//if ( owner.IsHandleCreated )
				//	((TabControl) owner).removeAllTabs ( );
			}

			public override void Remove ( Control value ) {
				//int index = IndexOf ( value );
				//base.Remove ( value );
				//if ( index != -1 && owner.IsHandleCreated )
				//	((TabControl) owner).removeTab ( value, index );
			}
		}
				
		
		public class TabPageCollection : IList, ICollection, IEnumerable {
			TabControl owner;
			Control.ControlCollection collection;
			
			public TabPageCollection( TabControl owner ) {
				this.owner = owner;
				collection = owner.Controls;
			}

			public int Count {
				get { return collection.Count; }
			}

			public bool IsReadOnly {
				get {	return collection.IsReadOnly; }
			}

			[MonoTODO]
			public virtual TabPage this[int index] {
				get {	return collection[ index ] as TabPage; }
				set {	
					//( (IList)collection )[ index ] = value;
					//owner.update ( );
				}
			}
			
			public void Add(TabPage value) {
				collection.Add ( value );
			}

			public void AddRange( TabPage[] pages ) {
				collection.AddRange ( pages );
			}

			public virtual void Clear() {
				collection.Clear ( );
			}

			public bool Contains( TabPage page ) {
				return collection.Contains ( page );
			}

			public IEnumerator GetEnumerator() {
				return collection.GetEnumerator ( );
			}

			public int IndexOf( TabPage page ) {
				return collection.IndexOf ( page );
			}

			public void Remove( TabPage value ) {
				collection.Remove ( value );
			}

			public void RemoveAt(int index) {
				collection.RemoveAt ( index );
			}

			/// <summary>
			/// IList Interface implmentation.
			/// </summary>
			bool IList.IsReadOnly{
				get{	return this.IsReadOnly; }
			}

			bool IList.IsFixedSize{
				get{	return (( IList )collection).IsFixedSize; }
			}

			object IList.this[int index]{
				get{	return collection [ index ]; }
				set{
					//if ( ! (value is TabPage) )
					//	throw new ArgumentException ( );
					//this[ index ] = (TabPage) value;
					//owner.update ( );
				}
			}
		
			void IList.Clear(){
				this.Clear ( );
			}
		
			[MonoTODO]
			int IList.Add( object value ) {
				TabPage page = value as TabPage;
				if ( page == null )
					throw new ArgumentException ( );
				this.Add ( page );
				return this.IndexOf ( page );
			}

			[MonoTODO]
			bool IList.Contains( object value ){
				return this.Contains ( value as TabPage );
			}

			[MonoTODO]
			int IList.IndexOf( object value ){
				return this.IndexOf ( value as TabPage );
			}

			[MonoTODO]
			void IList.Insert(int index, object value){
				//if ( ! (value is TabPage) )
				//	throw new ArgumentException ( );

				//(( IList )collection).Insert ( index, value );
				//owner.update ( );
			}

			void IList.Remove( object value ){
				this.Remove ( value as TabPage );
			}

			void IList.RemoveAt( int index){
				this.RemoveAt ( index );
			}
			// End of IList interface

			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count{
				get{ return this.Count;	}
			}

			bool ICollection.IsSynchronized{
				get{ return ( (ICollection) collection).IsSynchronized;	}
			}

			object ICollection.SyncRoot{
				get{ return ( (ICollection) collection).SyncRoot; }
			}

			void ICollection.CopyTo(Array array, int index){
				( (ICollection) collection ).CopyTo ( array, index );
			}
			// End Of ICollection
		}
		
		/*
		[MonoTODO]
		public class TabControl.ControlCollection :  Control.ControlCollection {
			public TabControl.ControlCollection(TabControl owner);

			public override void Add(Control value);
			public override void Remove(Control value);

		}*/
		/*
		[MonoTODO]
		public class TabControl.TabPageCollection : IList, ICollection,  IEnumerable{

			public TabControl.TabPageCollection(TabControl owner);
			public virtual int Count {get;}
			public virtual bool IsReadOnly {get;}
			public virtual TabPage this[int index] {get; set;}
			int ICollection.Count {get;}
			bool ICollection.IsSynchronized {get;}
			object ICollection.SyncRoot {get;}
			bool IList.IsFixedSize {get;}
			bool IList.IsReadOnly {get;}
			object IList.this[ int index] {get; set;}

			public void Add( TabPage value);
			public void AddRange( TabPage[] pages);
			public virtual void Clear();
			public bool Contains( TabPage page);
			public virtual IEnumerator GetEnumerator();
			void ICollection.CopyTo(Array dest, int index);
			int IList.Add( object value);
			bool IList.Contains(object page);
			int IList.IndexOf( object page);
			void IList.Insert(int index, object value);
			void IList.Remove( object value);
			public int IndexOf( TabPage page);
			public void Remove( TabPage value);
			public virtual void RemoveAt(int index);


		}*/

	}

}
