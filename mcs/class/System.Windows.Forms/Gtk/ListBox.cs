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
		private ListBox.ObjectCollection items;
		private ListBox.SelectedIndexCollection selectedIndices;
		private ListBox.SelectedObjectCollection selectedItems;
		private bool sorted;
		
		
		public const int DefaultItemHeight = 13;
		public const int NoMatches = -1;
		
		[MonoTODO]
		public ListBox () : base ()
		{
			items = new ListBox.ObjectCollection (this);	
			selectedIndices = new ListBox.SelectedIndexCollection (this);
			selectedItems = new ListBox.SelectedObjectCollection (this);
		}
		

		[MonoTODO]
		public override Color BackColor {
			get { throw new NotImplementedException(); }
			set { }
		}
		[MonoTODO]
		public override System.Drawing.Image BackgroundImage {
			get { throw new NotImplementedException (); }
			set {  }
		}
		[MonoTODO]
		// Don't use ?
		public BorderStyle BorderStyle {
			get { throw new NotImplementedException (); }
			set {  }
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
			get { return DrawMode.Normal; }
			set { 
				if (! Enum.IsDefined (typeof (DrawMode), value))
					throw new InvalidEnumArgumentException  ("");
				throw new NotImplementedException();
			}
		}
		[MonoTODO]
		public override Color ForeColor {
			get { throw new NotImplementedException (); }
			set { }
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
			set {  }
		}
		[MonoTODO]
		public bool IntegralHeight {
			get { throw new NotImplementedException (); }
			set {  }
		}
		[MonoTODO]
		public virtual int ItemHeight {
			get { throw new NotImplementedException (); }
			set {  }
		}
		[MonoTODO]
		public ListBox.ObjectCollection Items {
			get { return items; }
		}
		[MonoTODO]
		// Default false.
		public bool MultiColumn {
			get { return false; }
			set { }
		}
		[MonoTODO]
		public int PreferredHeight {
			get { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override RightToLeft RightToLeft {
			get { throw new NotImplementedException (); }
			set {  }
		}
		[MonoTODO]
		public bool ScrollAlwaysVisible {
			get { throw new NotImplementedException (); }
			set {  }
		}
		[MonoTODO]
		public override int SelectedIndex {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public ListBox.SelectedIndexCollection SelectedIndices {
			get { return selectedIndices; }		
		}
		[MonoTODO]
		public object SelectedItem {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public ListBox.SelectedObjectCollection SelectedItems {
			get { return selectedItems; }
		}
		[MonoTODO]
		public virtual SelectionMode SelectionMode {
			get { throw new NotImplementedException (); }
			set { }
		}
		[MonoTODO]
		public bool Sorted {
			get { return sorted; }
			set { 
				if (value == true) 
					this.Sort();
				sorted = value;
			}
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
			ListStore store = new ListStore (typeof (string));
			foreach (Object o in Items){
				Value value = new Value (o);
				TreeIter iter = store.Append();
				store.SetValue (iter, 0, value);
			}
			(Widget as Gtk.TreeView).Model = store;
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
			private ListBox owner;
			private ArrayList list;
			
			public ObjectCollection(ListBox owner)
			{
				this.owner = owner;
				this.list = new ArrayList();
			}
			public ObjectCollection(ListBox owner, object[] value) : this(owner)
			{
				AddRange (value);
			}
			[MonoTODO]
			public ObjectCollection(ListBox owner, ObjectCollection value) : this (owner)
			{
				//TODO: Implement
			}
			[MonoTODO]
			public virtual int Count {
				get { return list.Count; }
			}
			[MonoTODO]
			public virtual bool IsReadOnly {
				get { return false; }
			}
			[MonoTODO]
			public virtual object this[int index] {
				get { return list[index]; }
				set { throw new NotImplementedException(); }
			}
			// Net 1.1
			bool ICollection.IsSynchronized {
				get { return list.IsSynchronized; }
			}
			// Net 1.1
			object ICollection.SyncRoot {
				get { return list.SyncRoot; }
			}
			// Net 1.1
			bool IList.IsFixedSize {
				get { return list.IsFixedSize; }
			}

			[MonoTODO]
			public int Add(object item)
			{
				// FIXME: Implement sorted insert, if ListBox.Sorted == true,
				int ret = list.Add (item);
				owner.Refresh();
				return ret;
			}
			
			/*public class ItemCollection {

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
		}*/
			
			[MonoTODO]
			public void AddRange(object[] items)
			{
				// FIXME: should stop Control refresh until added finished
				foreach (object o in items)
					this.Add (o);
			}
			
			[MonoTODO]
			public void AddRange(ListBox.ObjectCollection value)
			{
				list.AddRange (value);
			}
			
			[MonoTODO]
			public virtual void Clear()
			{
				list.Clear();
			}
			
			public virtual bool Contains(object value)
			{
				return list.Contains (value);
			}
			
			public void CopyTo(object[] dest, int arrayIndex)
			{
				list.CopyTo (dest, arrayIndex);
			}
			
			public virtual IEnumerator GetEnumerator()
			{
				return list.GetEnumerator();
			}
			
			void ICollection.CopyTo( Array dest,  int index)
			{
				list.CopyTo(dest, index);
			}
			
			[MonoTODO]
			int IList.Add( object item)
			{
				return this.Add (item);
			}
			
			public virtual int IndexOf(object value)
			{
				return list.IndexOf(value);
			}		
			
			[MonoTODO]
			public virtual void Insert( int index, object item)
			{
				list.Insert (index, item);
			}
			
			[MonoTODO]
			public virtual void Remove(object value)
			{
				list.Remove (value);
			}
			
			[MonoTODO]
			public virtual void RemoveAt(int index)
			{
				list.RemoveAt (index);
			}

		}

		public class SelectedIndexCollection : IList, ICollection,  IEnumerable
		{
		
			private ListBox owner;
			private ArrayList list;
			public bool IsFixedSize {
				get { return false; }
			}
		
			public bool IsSynchronized {
				get { return list.IsSynchronized; }
			}
			public object SyncRoot {
				get { return list.SyncRoot; }
			}
		
			public SelectedIndexCollection(ListBox owner)
			{
				this.owner = owner;
				list = new ArrayList ();
			}
			
			public virtual int Count {
				get { return list.Count; }
			}
			public virtual bool IsReadOnly {
				get { return false; }
			}
			[MonoTODO]
			public object this[int index] {
				get { return list[index]; }
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
				list.CopyTo(dest, index);
				//throw new NotImplementedException ();
			}
			
			[MonoTODO]
			public virtual IEnumerator GetEnumerator()
			{
				return list.GetEnumerator();
				//throw new NotImplementedException ();
			}
			
			// don't use
			[MonoTODO]
			int IList.Add(object value)
			{
				//list.Add (value);
				throw new NotImplementedException ();
			}
			
			// don't use
			[MonoTODO]
			void IList.Clear()
			{
				//throw new NotImplementedException ();
			}
			
			[MonoTODO]
			bool IList.Contains(object selectedIndex)
			{
				throw new NotImplementedException ();
			}
			
			int IList.IndexOf(object selectedIndex)
			{
				return list.IndexOf (selectedIndex);
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
		
		
			private ListBox owner;
			private ArrayList list;
			
			public bool IsFixedSize {
				get { return false; }
			}
		
			public bool IsSynchronized {
				get { return list.IsSynchronized; }
			}
			public object SyncRoot {
				get { return list.SyncRoot; }
			}
		
			public SelectedObjectCollection(ListBox owner)
			{
				this.owner = owner;
				list = new ArrayList();
			}
			
			public virtual int Count {
				get { return list.Count; }
			}
			
			public virtual bool IsReadOnly {
				get { return list.IsReadOnly; }
			}
			[MonoTODO]
			public virtual object this[int index] {
				get { return list[index]; }
				set { throw new NotImplementedException (); }
			}
			
			public virtual bool Contains(object selectedObject)
			{
				return list.Contains (selectedObject);
			}
			
			public virtual void CopyTo(Array dest,  int index)
			{
				list.CopyTo(dest, index);
			}
			
			public virtual IEnumerator GetEnumerator()
			{
				return list.GetEnumerator();
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
		
		internal override Gtk.Widget CreateWidget () {
						
			ListStore store = new ListStore (typeof (string));
			TreeView tv = new TreeView (store);
			tv.HeadersVisible = false;
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
	}	
}

		//ListStore store = null;
		//TreeIter iter = new TreeIter ();
		
		//public ItemCollection Items;
		//ListStore store = new ListStore ((int)TypeFundamentals.TypeString);
		
		/*public class ItemCollection {

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
		}*/
