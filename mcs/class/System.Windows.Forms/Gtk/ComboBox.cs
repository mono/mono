//		
//			System.Windows.Forms.ComboBox
//
//			Author: 
//					Joel Basson		 	(jstrike@mweb.co.za)
//					Alberto Fernandez	(infjaf00@yahoo.es)
//
//

using System.Collections;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows ComboBox control.
	///
	/// </summary>

	public class ComboBox: ListControl{		
		
		public ObjectCollection items;
 		GLib.List list = new GLib.List (IntPtr.Zero, typeof (string));
		System.Collections.ArrayList alist = new System.Collections.ArrayList();
		
		private bool sorted = false;
		private int maxDropDownItems = 8;
		private ComboBoxStyle dropDownStyle = ComboBoxStyle.DropDown;
		private bool updating = false;
		private bool disableChanging = false;
		
		public ComboBox () : base (){
		}
		
		protected override Size DefaultSize {
			get {return new Size (121, 20); }
		}
		[MonoTODO]
		public DrawMode DrawMode {
			get{return DrawMode.Normal;}
			set{
				if (!Enum.IsDefined (typeof (DrawMode), value))
					throw new InvalidEnumArgumentException ("DrawMode");
			}
		}	
		[MonoTODO]
		public ComboBoxStyle DropDownStyle {
			get{return dropDownStyle;}
			set{
				if (!Enum.IsDefined (typeof(ComboBoxStyle), value))
					throw new InvalidEnumArgumentException ("DropDownStyle");
					
				if (dropDownStyle == value)
					return;
				
				switch (value){
					case ComboBoxStyle.Simple:
					case ComboBoxStyle.DropDown:
						(Widget as Gtk.Combo).Entry.Editable = true;
						break;
					case ComboBoxStyle.DropDownList:
						(Widget as Gtk.Combo).Entry.Editable = false;
						break;
				}
				dropDownStyle = value;
				OnDropDownStyleChanged (EventArgs.Empty);
			}		
		}
		[MonoTODO]
		public int DropDownWidth {
			get{ throw new NotImplementedException(); }
			set{
				if (value < 1)
					throw new ArgumentException();
				throw new NotImplementedException();}
		}
		
		[MonoTODO]
		public bool DroppedDown {
			get{ return false; }
			set{ return; }
		}
		[MonoTODO]
		public bool IntegralHeight {
			get{throw new NotImplementedException();}
			set{throw new NotImplementedException();}
		}
		[MonoTODO]
		public int ItemHeight {
			get{throw new NotImplementedException();}
			set{throw new NotImplementedException();}
		}
		public ComboBox.ObjectCollection Items{
			get{
				if (items == null)
					items = new ObjectCollection(this);
				return items;
			}
		}
		[MonoTODO]
		public int MaxDropDownItems {
			get{ return MaxDropDownItems; }
			set{
				if ((value < 1) || (value > 100))
					throw new ArgumentException();
				maxDropDownItems = value;				
			}
		}
		[MonoTODO]
		public int MaxLength {
			get{ return (Widget as Gtk.Combo).Entry.MaxLength; }
			set{
				if (value < 0)
					value = 0;
				(Widget as Gtk.Combo).Entry.MaxLength = value;
			}
		}
		[MonoTODO]
		public int PreferredHeight {
			get{throw new NotImplementedException();}
		}
		[MonoTODO]
		public override int SelectedIndex{
			get{ return Items.IndexOf(((Gtk.Combo)Widget).Entry.Text);}
			set{
				if ((value < -2) || (value > Items.Count))
					throw new ArgumentException ("SelectedIndex");
				
				if (SelectedIndex == value)
					return;
					
				disableChanging = true;
				((Gtk.Combo)Widget).Entry.Text = (string)Items [value];
				OnSelectedIndexChanged(EventArgs.Empty);
				disableChanging = false;
			}
		}
		[MonoTODO]
		public Object SelectedItem{
			get{ return Items[this.SelectedIndex];}
			set{ this.SelectedIndex = Items.IndexOf (value);}
		}
		[MonoTODO]
		public string SelectedText {
			get{
				if (this.DropDownStyle == ComboBoxStyle.DropDownList)
					return String.Empty;
				else{
					int start, end;				
					if ( (Widget as Gtk.Combo).Entry.GetSelectionBounds(out start, out end))
						return (Widget as Gtk.Combo).Entry.GetChars (start, end);
					else
						return String.Empty;
				}				
			}
			set{(Widget as Gtk.Combo).Entry.InsertText (value);}
		}
		[MonoTODO]
		public int SelectionLength {
			get{ return this.SelectedText.Length;	}
			set{ 
				if (value < 0)
					throw new ArgumentException ("SelectionLength");
				if ((SelectionStart + value) > Text.Length){
					value = Text.Length - SelectionStart;
				}
				(Widget as Gtk.Combo).Entry.SelectRegion (
					SelectionStart, SelectionStart + value);					
			}
		}
		[MonoTODO]
		public int SelectionStart {
			get{
				int start, end;
				
				if ( (Widget as Gtk.Combo).Entry.GetSelectionBounds(out start, out end))
					return start;
				else
					return (Widget as Gtk.Combo).Entry.Position;
			
			}
			set{
				if (value < 0)
					throw new ArgumentException ("SelectionStart");
				if (value > this.Text.Length)
					value = this.Text.Length;
				
				int length = SelectionLength;
				
				if ((value + length) > Text.Length)
					length = Text.Length - value;				
				(Widget as Gtk.Combo).Entry.SelectRegion (value, length);
				
				
			}		
		}
		
		[MonoTODO]
		public bool Sorted {
			get{return sorted;}
			set{
				if (! sorted && value )
					Items.Sort();				
				sorted = value;
			}
		}
		[MonoTODO]
		public override string Text{
			get{ return (Widget as Gtk.Combo).Entry.Text; }
			set{
				disableChanging = true;
				if ((value == null) || (value == String.Empty))
					this.SelectedIndex = -1;
				else
					(Widget as Gtk.Combo).Entry.Text = value;
				disableChanging = false;
			}
		}
		
		[MonoTODO]
		protected virtual void AddItemsCore( object[] value){
		}
		[MonoTODO]
		public void BeginUpdate () {
			updating = true;
		}
		[MonoTODO]
		protected override void Dispose( bool disposing){
			base.Dispose(disposing);
		}
		[MonoTODO]
		public void EndUpdate () {
			updating = false;
			Update();
		}
		[MonoTODO]
		public int FindString (string value){			
			return alist.IndexOf(value);
		}
		[MonoTODO]
		public int FindString(string s, int startIndex){
			throw new NotImplementedException();
		}
		[MonoTODO]
		public int FindStringExact( string s){
			throw new NotImplementedException();
		}
		[MonoTODO]
		public int FindStringExact( string s, int startIndex){
			throw new NotImplementedException();
		}
		[MonoTODO]
		public int GetItemHeight(int index){
			throw new NotImplementedException();
		}
		
		protected virtual void OnDrawItem(DrawItemEventArgs e){
			if (DrawItem != null)
				DrawItem (this,e);
		}
		protected virtual void OnDropDown( EventArgs e){
			if (DropDown != null)
				DropDown (this, e);
		}
		protected virtual void OnDropDownStyleChanged( EventArgs e){
			if (DropDownStyleChanged != null)
				DropDownStyleChanged (this, e);
		}
		
		protected virtual void OnMeasureItem(MeasureItemEventArgs e){
			if (MeasureItem != null)
				MeasureItem (this, e);
		}
		
		protected override void OnSelectedIndexChanged(EventArgs e){
			base.OnSelectedIndexChanged(e);
			if (SelectedIndexChanged != null)
				SelectedIndexChanged (this, e);
		}
		
		// Don't use
		[MonoTODO]
		protected virtual void OnSelectedItemChanged(EventArgs e){
			
		}
		protected override void OnSelectedValueChanged (EventArgs e){
			base.OnSelectedValueChanged (e);
		}
		protected virtual void OnSelectionChangeCommitted( EventArgs e){
			if (SelectionChangeCommitted != null)
				SelectionChangeCommitted (this, e);
		}
		[MonoTODO]
		protected override void RefreshItem(int index){
		}
		[MonoTODO]
		public void Select(int start, int length){
		}
		[MonoTODO]
		public void SelectAll(){
		}
		[MonoTODO]
		protected override void SetItemCore( int index,  object value){
		}
		[MonoTODO]
		protected override void SetItemsCore( IList value){
		}
		public override string ToString(){
			return "System.Windows.Forms.ComboBox, Items.Count: " + Items.Count;
		}
		
		// ?
		[MonoTODO]
		public override void Update () {
			disableChanging = true;		
			String tmp = (Widget as Gtk.Combo).Entry.Text;
			String[] strings = new String [this.Items.Count];
			int i=0;
			foreach (String s in Items){
				strings[i++] = s;
			}
			if (strings.Length > 0)
				(Widget as Gtk.Combo).SetPopdownStrings(strings);
			else
				(Widget as Gtk.Combo).SetPopdownStrings("");
			
			(Widget as Gtk.Combo).Entry.Text = tmp;
			disableChanging = false;
		}

		public event DrawItemEventHandler DrawItem;
		public event EventHandler DropDown;
		public event EventHandler DropDownStyleChanged;
		public event MeasureItemEventHandler MeasureItem;
		public event EventHandler SelectedIndexChanged;
		public event EventHandler SelectionChangeCommitted;	
		
		
		
		
		internal override Gtk.Widget CreateWidget () {
			Gtk.Combo com1 = new Gtk.Combo();
			com1.SetPopdownStrings("");
			com1.DisableActivate();
			com1.Entry.Changed += new EventHandler (OnEntryChanged);
			return com1;
		}
		internal void OnEntryChanged (object o, EventArgs args){
			if (disableChanging)
				return;
			if (this.SelectedIndex != -1){
				OnSelectionChangeCommitted (EventArgs.Empty);
				OnSelectedIndexChanged (EventArgs.Empty);
			}
		}		
		
		/// sub-class: ComboBox.ObjectCollection
		/// <summary>
		/// Represents the collection of items in a ComboBox.
		/// </summary>
		[MonoTODO]
		public class ObjectCollection : IList, ICollection, IEnumerable {
			private ArrayList collection_ = new ArrayList ();
			private ComboBox owner_ = null;
			
			/// --- ObjectCollection.constructor ---
			
			public ObjectCollection (ComboBox owner) {
				owner_ = owner;
			}
			
			/// --- ObjectCollection Properties ---
			
			public int Count {
				get { return collection_.Count; }
			}			
			public bool IsReadOnly {
				get { return collection_.IsReadOnly; }
			}
			[MonoTODO]
			object IList.this[int index] {
				get { return collection_[index]; }
				set { 
					collection_[index] = value; 
					if (!owner_.updating)
						owner_.Update();
				}
			}						
			[MonoTODO]
			public object this[int index] {
				get { return collection_[index]; }
				set { 
					collection_[index] = value; 
					if (!owner_.updating)
						owner_.Update();
				}
			}

			/// --- ICollection properties ---
			bool IList.IsFixedSize {
				get { return collection_.IsFixedSize; }
			}
			object ICollection.SyncRoot {
				get { return collection_.SyncRoot; }
			}
			bool ICollection.IsSynchronized {
				get { return collection_.IsSynchronized; }
			}
			
			/// --- methods ---
			/// --- ObjectCollection Methods ---
			/// Note: IList methods are stubbed out, otherwise IList interface cannot be implemented
			[MonoTODO]
			public int Add(object item) {
				int idx=0;
				if ( owner_.Sorted){
					idx = collection_.BinarySearch(item, ComboBoxComparer.Instance);
					if (idx < 0)
						idx = ~idx;
					collection_.Insert (idx, item);
				}
				else {
					idx = collection_.Add (item);				
				}
				if (!owner_.updating)
						owner_.Update();			
				return idx;
			}
			
			[MonoTODO]
			public void AddRange(object[] items){
				owner_.BeginUpdate();
				foreach(object item in items) {
					Add(item);
				}
				owner_.EndUpdate();
			}
			[MonoTODO]
			public void Clear(){
				collection_.Clear();
				owner_.Update();
			}
			public bool Contains(object value){
				return collection_.Contains(value);
			}
			public void CopyTo(object[] dest,int arrayIndex){
				collection_.CopyTo(dest, arrayIndex);
			}
			
			/// for ICollection:
			void ICollection.CopyTo(Array dest,int index){
				collection_.CopyTo(dest, index);
			}
			public IEnumerator GetEnumerator(){
				return collection_.GetEnumerator();
			}
			public int IndexOf(object value){
				return collection_.IndexOf(value);
			}			
			[MonoTODO]
			public void Insert(int index,object item){
				collection_.Insert (index, item);
				if (! owner_.updating)
					owner_.Update();	
			}
			public void Remove(object value){
				collection_.Remove(value);
				if (! owner_.updating)
					owner_.Update();
			}
			
			[MonoTODO]
			public void RemoveAt(int index){
				collection_.RemoveAt (index);
				if (!owner_.updating)
					owner_.Update();
			}
			
			internal void Sort(){
				collection_.Sort(ComboBoxComparer.Instance);
				owner_.Update();
			}
		}  // --- end of ComboBox.ObjectCollection ---
		
		internal class ComboBoxComparer : System.Collections.IComparer {
			private static ComboBoxComparer instance = null;
			private ComboBoxComparer(){
			}
			public static ComboBoxComparer Instance{
				get{
					if (instance == null)
						instance = new ComboBoxComparer();
					return instance;
				}
			}
			
			public int Compare (object x, object y){
				string s1 = (x as string).ToUpper();
				string s2 = (y as string).ToUpper();
				
				return Comparer.Default.Compare (s1, s2);
			}
		}
	}
}
