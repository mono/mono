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
	/// ToDo note:
	///  - Nothing is implemented
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
		protected override void Dispose(bool disposing) {
			throw new NotImplementedException ();
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
			if( items != null) {
				IsParent_ = true;
				MenuItems.AddRange ( items);
			}
		}

		//
		// -- Public Properties
		//


		private IntPtr menuHandle_ = IntPtr.Zero;
		public IntPtr Handle {
			get {
				if( menuHandle_ == IntPtr.Zero) {
					menuHandle_ = Win32.CreateMenu();
					//System.Console.WriteLine("Create menu {0}", menuHandle_);
				}
				return menuHandle_;
			}
		}

		protected bool IsParent_ = false;

		public virtual bool IsParent {

			get {
				return IsParent_;
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

		internal const uint INVALID_MENU_ID = 0xffffffff;
		// Variables are stored here to provide access for the base functions
		protected uint MenuID_ = INVALID_MENU_ID;

		// Provides unique id to all items in all menus, hopefully space is enougth.
		// Possible to use array to keep ids from deleted menu items
		// and reuse them.
		protected static uint MenuIDs_ = 1;

		// Library interface

		// Recursively searches for specified item in menu.
		// Goes immediately into child, when mets one.
		internal virtual MenuItem GetMenuItemByID( uint id)
		{
			foreach( MenuItem mi in MenuItems) {
				if( mi.IsParent_) {
					MenuItem submi = mi.GetMenuItemByID(id);
					if( submi != null) return submi;
				}
				else {
					if( mi.MenuID_ == id){
						return mi;
					}
				}
			}
			return null;
		}

		//
		// Btw, this function is funky, it is being used by routines that are supposed
		// to be passing an IntPtr to the AppendMenu function
		//
		internal virtual uint GetIDByMenuItem( MenuItem mi)
		{
			// FIXME: Pay attention, do not assign an id to a "stranger"
			// If reusing IDs, get one from array first
			if ( mi.MenuID_ == INVALID_MENU_ID) {
				mi.MenuID_ = MenuIDs_++;
			}
			return  mi.MenuID_;
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
			private Menu 				parentMenu_ = null;
			//
			// -- Constructor
			//

			public MenuItemCollection(Menu m) {
				parentMenu_ = m;
			}

			//
			// -- Public Methods
			//

			public virtual int Add(MenuItem mi) {
				int result = -1;
				// FIXME: MenuItem cannot be inserted to several containers. Check this here.
				if( mi != null){
					// FIXME: Set MenuItem's owner here.
					items_.Add(mi);
					result = items_.Count;
					//System.Console.WriteLine("Adding menuItem {0}, parent {1}", mi.Text, mi.IsParent);
					if( parentMenu_ != null) {
						if( mi.IsParent){
							Win32.AppendMenuA( parentMenu_.Handle, (int)MF_.MF_ENABLED | (int)MF_.MF_STRING | (int)MF_.MF_POPUP,
																mi.Handle, mi.Text);
						}
						else {
							Win32.AppendMenuA( parentMenu_.Handle, (int)MF_.MF_ENABLED | (int)MF_.MF_STRING,
									   (IntPtr) parentMenu_.GetIDByMenuItem(mi), mi.Text);
						}
					}
				}
				return result;
			}
			private MenuItem AddMenuItemCommon( MenuItem mi) {
				return ( -1 != Add (mi)) ? mi : null;
			}

			public virtual MenuItem Add ( string s) {
				return AddMenuItemCommon( new MenuItem (s));
			}

			public virtual int Add ( int i, MenuItem m) {
				throw new NotImplementedException ();
			}

			public virtual MenuItem Add (string s, EventHandler e) {
				return AddMenuItemCommon(new MenuItem ( s, e));
			}

			public virtual MenuItem Add (string s, MenuItem[] items) {
				return AddMenuItemCommon(new MenuItem ( s, items));
			}

			public virtual void AddRange(MenuItem[] items) {
				foreach( MenuItem mi in items) {
					Add(mi);
				}
			}

			public virtual void Clear() {
				throw new NotImplementedException ();
			}

			public bool Contains(MenuItem m) {
				throw new NotImplementedException ();
			}

			public void CopyTo(Array a, int i) {
				throw new NotImplementedException ();
			}

			public override bool Equals(object o) {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}

			public virtual void Remove(MenuItem m) {
				throw new NotImplementedException ();
			}

			public virtual void RemoveAt(int i) {
				throw new NotImplementedException ();
			}

			public override string ToString() {
				throw new NotImplementedException ();
			}

			//
			// -- Protected Methods
			//

			~MenuItemCollection() {
				throw new NotImplementedException ();
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
					throw new NotImplementedException ();
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



