//
// System.Windows.Forms.Menu
// 

using System;
using System.ComponentModel;
using System.Collections;

using Gtk;

namespace System.Windows.Forms{

	public abstract class Menu : Component{
	
		private  MenuItemCollection _items;
		
		static Menu (){
			Gtk.Application.Init();
		}
		protected Menu (System.Windows.Forms.MenuItem[] items) {
			_items = new MenuItemCollection(this);
			if (items != null){
				foreach (System.Windows.Forms.MenuItem m in items)
					MenuItems.Add (m);
			}
		} 
		
		// ?
		// Public properties not to be used
		// Don't use
		//public const int FindHandle=0; 
		// Don't use. 
		//public const int FindShortcut=0;
		
		
		[MonoTODO]
		public virtual IntPtr Handle {
			get {return IntPtr.Zero;}
		}
		public virtual bool IsParent{
			get{ return (MenuItems.Count != 0); }
		}
		[MonoTODO]
		public virtual System.Windows.Forms.MenuItem MdiListItem {
			get{ return null; }
		}
		public virtual System.Windows.Forms.Menu.MenuItemCollection MenuItems { 
			get { return _items; }
		}
		
		[MonoTODO]
		protected virtual void CloneMenu (System.Windows.Forms.Menu menuSrc){
		}
		
		[MonoTODO]
		protected override void Dispose (bool disposing){
			base.Dispose (disposing);
		}
		
		// Don't use
		[MonoTODO]
		public virtual System.Windows.Forms.MenuItem FindMenuItem (int type, IntPtr value){
			return null;
		}
		// Don't use.
		[MonoTODO]
		protected virtual int FindMergePosition (int mergeOrder){
			throw new NotImplementedException();
		}

		public virtual ContextMenu GetContextMenu (){
				System.Windows.Forms.Menu menu = this;
				while((menu != null) && !(menu is ContextMenu)){
					if(menu is System.Windows.Forms.MenuItem){
						menu = ((System.Windows.Forms.MenuItem)menu).Parent;
					}
					else{
						return null;
					}
				}
				return (ContextMenu)menu;
		}
		public virtual MainMenu GetMainMenu(){
				Menu menu = this;
				while((menu != null) && !(menu is MainMenu)){
					if(menu is System.Windows.Forms.MenuItem){
						menu = ((System.Windows.Forms.MenuItem)menu).Parent;
					}
					else{
						return null;
					}
				}
				return (MainMenu)menu;
		}
		
		[MonoTODO]
		public virtual void MergeMenu (Menu menuSrc){
			if (menuSrc == this){
				throw new Exception ();
			}
			
			
		}
		// Don't use
		[MonoTODO]		
		protected internal virtual bool ProcessCmdKey (ref Message msg, Keys keyData){
			throw new NotImplementedException();
		}
		[MonoTODO]
		public override String ToString (){
			return base.ToString() + ", Item.Count=" + _items.Count;
		}
		
		
		
		internal virtual void OnNewMenuItemAdd (MenuItem item){
		}
		internal virtual void OnNewMenuItemAdd (int index, MenuItem item){
		}
		internal virtual void OnRemoveMenuItem (MenuItem item){
		}
		internal virtual void OnLastSubItemRemoved (){
		}
		
		
		internal Gtk.Widget widget;
		internal virtual Gtk.Widget Widget{
			get{
				if (widget == null){
					widget = CreateWidget();
				}
				return widget;
			}
		}
		internal abstract Gtk.Widget CreateWidget();
	
		
		
		public class MenuItemCollection : IList, ICollection, IEnumerable {
			private ArrayList		items_ = new ArrayList();
			private Menu 			parentMenu_ = null;
			//
			// -- Constructor
			//

			public MenuItemCollection (Menu m) {
				parentMenu_ = m;
			}
			
			/*[MonoTODO]
			internal void MoveItemToIndex( int index, MenuItem mi) {
				if( index >= items_.Count){
					// FIXME: Set exception parameters
					throw new ArgumentException();
				}
				else if( items_.Count != 1){
					items_.Remove (mi);
					items_.Insert (index, mi);
					mi.Index = index;
				}
			}*/

			//
			// -- Public Methods
			//

			public virtual int Add (MenuItem mi) {
				int result = -1;
				if( mi != null && parentMenu_ != null){
					parentMenu_.OnNewMenuItemAdd(mi);
					items_.Add(mi);
					result = items_.Count - 1;
					mi.Index = result;
					if (mi.Parent != null){
						mi.Parent.MenuItems.Remove (mi);
					}
					mi.SetParent (parentMenu_);
				}
				return result;
			}
			
			private MenuItem AddMenuItemCommon (MenuItem mi) {
				return ( -1 != Add (mi)) ? mi : null;
			}

			public virtual MenuItem Add ( string s) {
				return AddMenuItemCommon( new MenuItem (s));
			}

			public virtual int Add ( int i, MenuItem mi) {
				if( i > items_.Count){
					// FIXME: Set exception details
					throw new System.ArgumentException();
				}
				int result = -1;
				if( mi != null && parentMenu_ != null){
					parentMenu_.OnNewMenuItemAdd(i, mi);
					items_.Insert(i, mi);
					result = i;
					mi.Index=result;
					if (mi.Parent != null){
						mi.Parent.MenuItems.Remove (mi);
					}
					mi.SetParent (parentMenu_);
				}
				return result;
			}

			public virtual MenuItem Add (string s, EventHandler e) {
				return AddMenuItemCommon(new MenuItem ( s, e));
			}

			public virtual MenuItem Add (string s, MenuItem[] items) {
				return AddMenuItemCommon(new MenuItem ( s, items));
			}

			public virtual void AddRange(MenuItem[] items) {
				if( items != null) {
					foreach( MenuItem mi in items) {
						Add(mi);
					}
				}
			}

			private void DoClear() {
				if( parentMenu_ != null) {
					foreach( MenuItem mi in items_) {
						parentMenu_.OnRemoveMenuItem( mi);
					}
				}
				items_.Clear();
				if( parentMenu_ != null) {
					parentMenu_.OnLastSubItemRemoved();
				}				
			}
			
			public virtual void Clear() {
				DoClear();
			}

			public bool Contains(MenuItem m) {
				return items_.Contains(m);
			}

			public void CopyTo(Array a, int i) {
				int targetIdx = i;
				foreach( MenuItem mi in items_) {
					MenuItem newMi = mi.CloneMenu();
					a.SetValue(newMi,targetIdx++);
				}
			}

			public override bool Equals(object o) {
				return base.Equals(o);
			}

			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
			}

			public IEnumerator GetEnumerator() {
				return items_.GetEnumerator();
			}

			public int IndexOf(MenuItem m) {
				return items_.IndexOf(m);
			}

			public virtual void Remove(MenuItem m) {
				if( m != null && parentMenu_ != null){
					if( Contains(m)){
						parentMenu_.OnRemoveMenuItem(m);
						items_.Remove(m);
						if( items_.Count == 0){
							parentMenu_.OnLastSubItemRemoved();
						}	
						m.SetParent(null);						
					}
				}
			}

			public virtual void RemoveAt(int i) {
				Remove(items_[i] as MenuItem);
			}

			public override string ToString() {
				return base.ToString();
				//throw new NotImplementedException ();
			}

			//
			// -- Protected Methods
			//

			~MenuItemCollection() {
				Clear();
			}


			//
			// -- Public Properties
			//

			public int Count {
				get {
					return items_.Count;					
				}
			}

			/// <summary>
			/// IList Interface implmentation.
			/// </summary>
			bool IList.IsReadOnly {
				get {
					// We allow addition, removeal, and editing of 
					// items after creation of the list.
					return false;
				}
			}

			bool IList.IsFixedSize {
				get {
					// We allow addition and removeal of 
					// items after creation of the list.
					return false;
				}
			}

			public MenuItem this[int index] {
				get {
					return items_[index] as MenuItem;
				}
			}

			//[MonoTODO]
			object IList.this[int index] {
				get {
					return items_[index];
				}
				set {
					// FIXME: Set exception members
					throw new System.NotSupportedException();
				}
			}
		
			[MonoTODO]
			void IList.Clear() {
				DoClear();
			}

			private MenuItem Object2MenuItem( object value) {
				MenuItem result = value as MenuItem;
				if( result == null) {
					// FIXME: Set exception parameters
					throw new System.ArgumentException();
				}
				return result;
			}

			[MonoTODO]
			int IList.Add( object value) {
				return Add( Object2MenuItem(value));
			}

			[MonoTODO]
			bool IList.Contains( object value) {
				return Contains(Object2MenuItem(value));
			}

			[MonoTODO]
			int IList.IndexOf( object value) {
				return IndexOf(Object2MenuItem(value));
			}

			[MonoTODO]
			void IList.Insert(int index, object value) {
				Add( index, Object2MenuItem(value));
			}

			[MonoTODO]
			void IList.Remove( object value) {
				Remove( Object2MenuItem(value));
			}

			[MonoTODO]
			void IList.RemoveAt( int index){
				RemoveAt(index);
			}
			// End of IList interface

			/// <summary>
			/// ICollection Interface implmentation.
			/// </summary>
			int ICollection.Count {
				get {
					return Count;
				}
			}
			bool ICollection.IsSynchronized {
				get {
					throw new NotImplementedException ();
				}
			}
			object ICollection.SyncRoot {
				get {
					throw new NotImplementedException ();
				}
			}
			void ICollection.CopyTo(Array array, int index){
				CopyTo(array, index);
			}
			// End Of ICollection
		}
	}
}
