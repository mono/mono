//
// System.Windows.Forms.Menu.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
//	Alexandre Pigolkine (pigolkine@gmx.de)
//
// (C) 2002 Ximian, Inc
//

using System;
using System.Reflection;
using System.Globalization;
//using System.Windows.Forms.AccessibleObject.IAccessible;
using System.Drawing;
using System.Collections;
using System.Runtime.Remoting;

namespace System.Windows.Forms  {


	/// <summary>
	/// </summary>
	using System.ComponentModel;
	public abstract class Menu : Component {

		//
		// -- Public Methods
		//

		[MonoTODO]
		public ContextMenu GetContextMenu() {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public MainMenu GetMainMenu() {
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual void MergeMenu(Menu menuSrc) {
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		public override string ToString() {
			//do our own ToString here.
			// this is overrridden
			return base.ToString();
		}

		//
		// -- Protected Methods
		//

		protected void CloneMenu(Menu menuSrc) {
			throw new NotImplementedException();
		}

		protected Menu( MenuItem[] items) {
			MenuItems.AddRange ( items);
		}

		//
		// -- Public Properties
		//

		protected internal bool menuStructureModified_ = true;
		
		internal void BuildMenuStructure ()
		{
			if( menuStructureModified_) {
				while( Win32.RemoveMenu( menuHandle_, 0, (uint)MF_.MF_BYPOSITION) != 0);
				int curIndex = 0;
				foreach(MenuItem mi in MenuItems) {
					System.Console.WriteLine("MenuItem {0} Parent {1}", mi.Text, mi.IsParent);
					if( mi.IsParent){
						Win32.AppendMenuA( menuHandle_, (int)MF_.MF_ENABLED | (int)MF_.MF_STRING | (int)MF_.MF_POPUP,
															mi.Handle, mi.Text);
						mi.Index = curIndex++;
					}
					else {
						Win32.AppendMenuA( menuHandle_, (int)MF_.MF_ENABLED | (int)MF_.MF_STRING,
								   (IntPtr) mi.GetID(), mi.Text);
						mi.Index = curIndex++;
					}
				}
				menuStructureModified_ = false;
			}
		}
		
        protected Menu parent_ = null;
        
		protected IntPtr menuHandle_ = IntPtr.Zero;
		internal void CreateMenuHandle()
		{
			if( menuHandle_ == IntPtr.Zero) {
				menuHandle_ = Win32.CreateMenu();
				//System.Console.WriteLine("Create menu {0}", menuHandle_);
				BuildMenuStructure();
				allMenus_[menuHandle_] = this;
			}
		}
		
		public IntPtr Handle {
			get {
				CreateMenuHandle();
				return menuHandle_;
			}
		}

		public virtual bool IsParent {

			get {
				return MenuItems.Count != 0;
			}
		}

		public MenuItem MdiListItem {

			get {
				throw new NotImplementedException();
			}
		}

		private Menu.MenuItemCollection  menuCollection_ = null;

		public Menu.MenuItemCollection MenuItems {
			get {
				if( menuCollection_ == null) {
					menuCollection_ = new Menu.MenuItemCollection( this);
				}
				return menuCollection_;
			}
		}


		// Library interface

		// Recursively searches for specified item in menu.
		// Goes immediately into child, when mets one.
		internal MenuItem GetMenuItemByID (uint id)
		{
			foreach( MenuItem mi in MenuItems) {
				if( mi.IsParent) {
					MenuItem submi = mi.GetMenuItemByID(id);
					if( submi != null) return submi;
				}
				else {
					if( mi.GetID() == id){
						return mi;
					}
				}
			}
			return null;
		}
		
		private static Hashtable allMenus_ = new Hashtable();
		
		internal static Menu GetMenuByHandle (IntPtr hMenu)
		{
			Menu result = null;
			try {
				result = allMenus_[hMenu] as Menu;
			}
			catch(ArgumentNullException) {
			}
			catch(NotSupportedException) {
			}
			return result;
		}
		
		internal void OnNewMenuItemAdd (MenuItem mi)
		{
			menuStructureModified_ = true;
			mi.SetParent( this);
		}
		
		internal void OnRemoveMenuItem (MenuItem mi)
		{
			if(menuHandle_ != IntPtr.Zero) {
				menuStructureModified_ = true;
			}
			mi.SetParent( null);
		}
		
		internal void OnLastSubItemRemoved ()
		{
			if( menuHandle_ != IntPtr.Zero) {
				//System.Console.WriteLine("Delete menu {0}", menuHandle_);
				Win32.DestroyMenu(menuHandle_);
				allMenus_.Remove(menuHandle_);
				menuHandle_ = IntPtr.Zero;
				
				if( parent_ != null) {
					parent_.menuStructureModified_ = true;
				}
			}
		}
		
		internal void OnWmInitMenu ()
		{
		}
		
		internal void OnWmInitMenuPopup ()
		{
			BuildMenuStructure();
		}

		//inherited
		//public virtual ISite Site {
		//	get {
		//		throw new NotImplementedException();
		//	}
		//	set {
		//		throw new NotImplementedException();
		//	}
		//}

		//
		// -- Protected Properties
		//

		//inherited
		//protected bool DesignMode {
		//	get {
		//		throw new NotImplementedException();
		//	}
		//}
		//protected EventHandlerList Events {
		//	get {
		//		throw new NotImplementedException();
		//	}
		//}


		//
		// System.Windows.Forms.Menu.MenuItemCollection.cs
		//
		// Author:
		//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
		//
		// (C) 2002 Ximian, Inc
		//
		/// <summary>
		/// ToDo note:
		///  - Nothing is implemented
		/// </summary>

		public class MenuItemCollection : IList, ICollection, IEnumerable {
			private ArrayList		items_ = new ArrayList();
			private Menu 			parentMenu_ = null;
			//
			// -- Constructor
			//

			public MenuItemCollection (Menu m) {
				parentMenu_ = m;
			}

			//
			// -- Public Methods
			//

			public virtual int Add (MenuItem mi) {
				int result = -1;
				if( mi != null && parentMenu_ != null){
					parentMenu_.OnNewMenuItemAdd(mi);
					items_.Add(mi);
					result = items_.Count;
					//System.Console.WriteLine("Adding menuItem {0}, parent {1}", mi.Text, mi.IsParent);
/*					
					if( mi.IsParent){
						Win32.AppendMenuA( parentMenu_.Handle, (int)MF_.MF_ENABLED | (int)MF_.MF_STRING | (int)MF_.MF_POPUP,
															mi.Handle, mi.Text);
					}
					else {
						Win32.AppendMenuA( parentMenu_.Handle, (int)MF_.MF_ENABLED | (int)MF_.MF_STRING,
								   (IntPtr) mi.GetID(), mi.Text);
					}
*/					
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
					parentMenu_.OnNewMenuItemAdd(mi);
					items_.Insert(i, mi);
					result = i;
/*					
					if( mi.IsParent){
						Win32.InsertMenuA( parentMenu_.Handle, (uint)i,
											(int)MF_.MF_ENABLED | (int)MF_.MF_STRING | (int)MF_.MF_POPUP | (int)MF_.MF_BYPOSITION,
											mi.Handle, mi.Text);
					}
					else {
						Win32.InsertMenuA( parentMenu_.Handle, (uint)i,
											(int)MF_.MF_ENABLED | (int)MF_.MF_STRING | (int)MF_.MF_BYPOSITION,
								   			(IntPtr) mi.GetID(), mi.Text);
					}
*/					
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

			public virtual void Clear() {
				if( parentMenu_ != null){
					foreach( MenuItem mi in items_) {
						parentMenu_.OnRemoveMenuItem( mi);
					}
				}
				items_.Clear();
				if( parentMenu_ != null){
					parentMenu_.OnLastSubItemRemoved();
				}				
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
					}
				}
			}

			public virtual void RemoveAt(int i) {
				Remove(items_[i] as MenuItem);
			}

			public override string ToString() {
				throw new NotImplementedException ();
			}

			//
			// -- Protected Methods
			//

			~MenuItemCollection() {
				Clear();
			}

			//inherited
			//protected object MemberwiseClone() {
			//	throw new NotImplementedException ();
			//}

			//
			// -- Public Properties
			//

			public int Count {

				get {
					return items_.Count;					
				}
			}

			//		public virtual MenuItem this(int i)
			//		{
			//			get
			//			{
			//				throw new NotImplementedException ();
			//			}
			//		}
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



