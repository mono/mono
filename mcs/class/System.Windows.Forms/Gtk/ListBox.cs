//		
//			System.Windows.Forms.ListBox
//
//			Authors: 
//				Joel Basson		(jstrike@mweb.co.za)
//				Alberto Fernandez	(infjaf00@yahoo.es)
//
//

using System.ComponentModel;
using System;
using System.Collections;
using System.Drawing;
using Gtk;
using GtkSharp;
using GLib;





namespace System.Windows.Forms
{
	public class ListBox : ListControl
	{
		[MonoTODO]
		public ListBox ()
		{
		}
		public const int DefaultItemHeight = 13;
		public const int NoMatches = -1;

		[MonoTODO]
		public override Color BackColor {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}
		[MonoTODO]
		public override System.Drawing.Image BackgroundImage {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		// Don't use ?
		public BorderStyle BorderStyle {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		// Default is 0
		public int ColumnWidth {
			get{ throw new NotImplementedException (); }
			set { 
				if (value < 0)
					throw new ArgumentException ("ColumnWidth must be equal or greather than 0");
				if (value == 0){
					// Default value ??
				}
				else {
				}
			}				
		}
		[MonoTODO]
		protected override CreateParams CreateParams {
			get { throw new NotImplementedException (); }
		}
		protected override Size DefaultSize {
			get { return new Size (120,96); }
		}
		[MonoTODO]
		// Default = DrawMode.Normal
		public virtual DrawMode DrawMode {
			get { throw new NotImplementedException (); }
			set { 
				if (! Enum.IsDefined (typeof (DrawMode), value))
					throw new InvalidEnumArgumentException  ("");
				throw new NotImplementedException();
			}
		}
		[MonoTODO]
		public override Color ForeColor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		// Defaults to 0.
		public int HorizontalExtent {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public bool HorizontalScrollbar {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public bool IntegralHeight {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public virtual int ItemHeight {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public ListBox.ObjectCollection Items {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		// Default false.
		public bool MultiColumn {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public int PreferredHeight {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override RightToLeft RightToLeft {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public bool ScrollAlwaysVisible {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override int SelectedIndex {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public ListBox.SelectedIndexCollection SelectedIndices {
			get { throw new NotImplementedException (); }		
		}
		[MonoTODO]
		public object SelectedItem {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public ListBox.SelectedObjectCollection SelectedItems {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public virtual SelectionMode SelectionMode {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public bool Sorted {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override string Text {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public int TopIndex {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public bool UseTabStops {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }		
		}

		// Don't use
		[MonoTODO]
		protected virtual void AddItemsCore(object[] value)
		{
		}
		[MonoTODO]
		public void BeginUpdate()
		{
		}
		[MonoTODO]
		public void ClearSelected()
		{
		}
		[MonoTODO]
		protected virtual ObjectCollection CreateItemCollection()
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public void EndUpdate()
		{
		}
		[MonoTODO]
		public int FindString (string s)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public int FindString (string s, int startIndex)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public int FindStringExact (string s)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int FindStringExact (string s, int startIndex)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		public int GetItemHeight(int index)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Rectangle GetItemRectangle(int index)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public bool GetSelected(int index)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int IndexFromPoint(Point p)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int IndexFromPoint(int x, int y)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnChangeUICues(UICuesEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnDataSourceChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnDisplayMemberChanged(EventArgs e)
		{
		}
		[MonoTODO]
		protected virtual void OnDrawItem(DrawItemEventArgs e)
		{
		}
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e)
		{
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e)
		{
		}
		[MonoTODO]
		protected virtual void OnMeasureItem(MeasureItemEventArgs e)
		{
		}
		[MonoTODO]
		protected override void OnParentChanged(EventArgs e)
		{
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e)
		{
		}
		[MonoTODO]
		protected override void OnSelectedIndexChanged(EventArgs e)
		{
		}
		[MonoTODO]
		protected override void OnSelectedValueChanged(EventArgs e)
		{
		}
		[MonoTODO]
		public override void Refresh()
		{
		}
		[MonoTODO]
		protected override void RefreshItem(int index)
		{
		}
		[MonoTODO]
		protected override void SetBoundsCore(int x,int y, int width, int height, BoundsSpecified specified)
		{
		}
		[MonoTODO]
		protected override void SetItemCore(int index, object value)
		{
		}
		[MonoTODO]
		protected override void SetItemsCore(IList value)
		{
		}
		[MonoTODO]
		public void SetSelected(int index, bool value)
		{
		}
		[MonoTODO]
		protected virtual void Sort()
		{
		}
		[MonoTODO]
		public override string ToString()
		{
			return base.ToString();
		}
		[MonoTODO]
		protected virtual void WmReflectCommand(ref Message m)
		{
		}
		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
		}
		
		public new event EventHandler BackgroundImageChanged;
		public new event EventHandler Click;
		public event DrawItemEventHandler DrawItem;
		public event MeasureItemEventHandler MeasureItem;
		public new event PaintEventHandler Paint;
		public event EventHandler SelectedIndexChanged;
		public new event EventHandler TextChanged;
		
		
		


		[MonoTODO]
		public class ObjectCollection : IList, ICollection, IEnumerable 
		{
			[MonoTODO]
			public ObjectCollection(ListBox owner)
			{
			}
			[MonoTODO]
			public ObjectCollection(ListBox owner, object[] value)
			{
			}
			[MonoTODO]
			public ObjectCollection(ListBox owner, ObjectCollection value)
			{
			}
			[MonoTODO]
			public virtual int Count {
				get {throw new NotImplementedException (); }
			}
			[MonoTODO]
			public virtual bool IsReadOnly {
				get {throw new NotImplementedException (); }
			}
			[MonoTODO]
			public virtual object this[int index] {
				get {throw new NotImplementedException (); }
				set {throw new NotImplementedException (); }
			}
			// Net 1.1
			[MonoTODO]
			bool ICollection.IsSynchronized {
				get {throw new NotImplementedException ();}
			}
			// Net 1.1
			[MonoTODO]
			object ICollection.SyncRoot {
				get {throw new NotImplementedException (); }
			}
			// Net 1.1
			[MonoTODO]
			bool IList.IsFixedSize {
				get {throw new NotImplementedException (); }
			}

			[MonoTODO]
			public int Add(object item)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void AddRange(object[] items)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void AddRange(ListBox.ObjectCollection value)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual void Clear()
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual bool Contains(object value)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public void CopyTo(object[] dest, int arrayIndex)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual IEnumerator GetEnumerator()
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			void ICollection.CopyTo( Array dest,  int index)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			int IList.Add( object item)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual int IndexOf(object value)
			{
				throw new NotImplementedException ();
			}		
			
			[MonoTODO]
			public virtual void Insert( int index, object item)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual void Remove(object value)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual void RemoveAt(int index)
			{
				throw new NotImplementedException ();
			}

		}

		public class SelectedIndexCollection : IList, ICollection,  IEnumerable
		{
		
			public bool IsFixedSize {
				get { return false; }
			}
		
			public bool IsSynchronized {
				get { throw new NotImplementedException (); }
			}
			public object SyncRoot {
				get { throw new NotImplementedException (); }
			}
		
			[MonoTODO]
			public SelectedIndexCollection(ListBox owner)
			{
			}
			[MonoTODO]
			public virtual int Count {
				get { throw new NotImplementedException (); }
			}
			[MonoTODO]
			public virtual bool IsReadOnly {
				get { throw new NotImplementedException (); }
			}
			[MonoTODO]
			public object this[int index] {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public bool Contains(int selectedIndex)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual void CopyTo(Array dest, int index)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual IEnumerator GetEnumerator()
			{
				throw new NotImplementedException ();
			}
			
			// don't use
			[MonoTODO]
			int IList.Add(object value)
			{
				throw new NotImplementedException ();
			}
			
			// don't use
			[MonoTODO]
			void IList.Clear()
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			bool IList.Contains(object selectedIndex)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			int IList.IndexOf(object selectedIndex)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			void IList.Insert(int index, object value)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			void IList.Remove(object value)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			void IList.RemoveAt(int index)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public int IndexOf(int selectedIndex)
			{
				throw new NotImplementedException ();
			}			

		}
		
		
		
		public class SelectedObjectCollection : IList, ICollection, IEnumerable
		{
			public bool IsFixedSize {
				get { return false; }
			}
		
			public bool IsSynchronized {
				get { throw new NotImplementedException (); }
			}
			public object SyncRoot {
				get { throw new NotImplementedException (); }
			}
		
			[MonoTODO]
			public SelectedObjectCollection(ListBox owner)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual int Count {
				get { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public virtual bool IsReadOnly {
				get { throw new NotImplementedException (); }
			}
			[MonoTODO]
			public virtual object this[int index] {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}
			
			[MonoTODO]
			public virtual bool Contains(object selectedObject)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual void CopyTo(Array dest,  int index)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual IEnumerator GetEnumerator()
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			int IList.Add(object value)
			{
				throw new NotImplementedException ();
			}
			
			// don't use
			[MonoTODO]
			void IList.Clear()
			{
				throw new NotImplementedException ();
			}
			
			// don't use
			[MonoTODO]
			void IList.Insert(int index, object value)
			{
				throw new NotImplementedException ();
			}
			
			// don't use
			[MonoTODO]
			void IList.Remove(object value)
			{
				throw new NotImplementedException ();
			}
			
			// don't use
			[MonoTODO]
			void IList.RemoveAt(int index)
			{
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual int IndexOf(object selectedObject)
			{
				throw new NotImplementedException ();
			}

		}
	}
}




/*
old Joel Code.
namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows ListBox control.
	///
	/// </summary>

	public class ListBox: ListControl{
	
		ListStore store = null;
		TreeIter iter = new TreeIter ();
		public ItemCollection Items;
		//ListStore store = new ListStore ((int)TypeFundamentals.TypeString);
		
		public class ItemCollection {

			ListBox owner;
			TreeIter iter = new TreeIter ();
			 
			public ItemCollection (ListBox owner){

				this.owner = owner;
				owner.store = new ListStore (typeof (string));
				
			}
						
			public void Add(String items){
			
				Value value = new Value(items);
				iter = owner.store.Append ();
 				owner.store.SetValue (iter, 0, value);
				owner.UpdateStore();
			}
		}
		
		protected override void SetItemsCore(IList items){}
		public ListBox () : base (){
			this.Items = new ItemCollection(this);
		}
	
		internal override Gtk.Widget CreateWidget () {
		
			ListStore store = new ListStore (typeof (string));
			TreeView tv = new TreeView ();
			tv.HeadersVisible = true;
			tv.HeadersClickable = false;
			tv.EnableSearch = false;
			TreeViewColumn NameCol = new TreeViewColumn ();
			CellRenderer NameRenderer = new CellRendererText ();
			NameCol.Title = "Name";
			NameCol.PackStart (NameRenderer, true);
			NameCol.AddAttribute (NameRenderer, "text", 0);
			tv.AppendColumn (NameCol);
			tv.Model = store;
			return tv;
		}
		
		public void UpdateStore () {
			((Gtk.TreeView)Widget).Model = store;		
		}

		protected override void RefreshItem(int index) {
			//FIXME:
		}

		public override int SelectedIndex {
			get{
				throw new NotImplementedException ();
			}
			set{
				//FIXME:
			}
		}
	}
}
*/
