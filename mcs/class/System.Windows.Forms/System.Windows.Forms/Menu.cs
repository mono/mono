//
// System.Windows.Forms.Menu.cs
//
// Author:
//   stubbed out by Paul Osman (paul.osman@sympatico.ca)
//	Dennis Hayes (dennish@raytek.com)
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
		//inherited
		//public virtual ObjRef CreateObjRef(Type t) {
		//	throw new NotImplementedException();
		//}
		//
		//public void Dispose() {
		//	throw new NotImplementedException();
		//}
		//
		//protected virtual void Dispose(bool disposing) {
		//	throw new NotImplementedException();
		//}

		public override bool Equals(object o) {
			throw new NotImplementedException();
		}

		//public static bool Equals(object o1, object o2) {
		//	throw new NotImplementedException();
		//}
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}

		public ContextMenu GetContextMenu() {
			throw new NotImplementedException();
		}

		public MainMenu GetMainMenu() {
			throw new NotImplementedException();
		}

		//inherited
		//public object GetLifetimeService() {
		//	throw new NotImplementedException();
		//}
		//public Type GetType() {
		//	throw new NotImplementedException();
		//}
		//public virtual object InitializeLifetimeService() {
		//	throw new NotImplementedException();
		//}

		public virtual void MergeMenu(Menu menuSrc) {
			throw new NotImplementedException();
		}

		public override string ToString() {
			throw new NotImplementedException();
		}

		//
		// -- Protected Methods
		//

		protected void CloneMenu(Menu menuSrc) {
			throw new NotImplementedException();
		}
		
		private IntPtr menuHandle_ = IntPtr.Zero;
		private Menu.MenuItemCollection  menuCollection_ = null;
		protected Menu( MenuItem[] items) {
			menuHandle_ = Win32.CreateMenu();
			menuCollection_ = new Menu.MenuItemCollection( this);
		}

		~Menu() {
			Win32.DestroyMenu( menuHandle_);
			//throw new NotImplementedException();
		}

		//protected virtual object GetService(Type service) {
		//	throw new NotImplementedException();
		//}
		//
		//protected object MemberwiseClone() {
		//	throw new NotImplementedException();
		//}

		//
		// -- Public Events
		//

		//inherited
		//public event EventHandler Disposed;

		//
		// -- Public Properties
		//

		//inherited
		//public IContainer Container {
		//	get {
		//		throw new NotImplementedException();
		//	}
		//}

		public IntPtr Handle {

			get {
				return menuHandle_;
			}
		}

		public virtual bool IsParent {

			get {
				throw new NotImplementedException();
			}
		}

		public MenuItem MdiListItem {

			get {
				throw new NotImplementedException();
			}
		}

		public Menu.MenuItemCollection MenuItems {

			get {
				return menuCollection_;
			}
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

			public virtual int Add(MenuItem m) {
				int result = -1;
				// FIXME: MenuItem cannot be inserted to several containers. Check this here.
				if( m != null){
					// FIXME: Set MenuItem's owner here.
					items_.Add(m);
					result = items_.Count;
					if( parentMenu_ != null) {
						Win32.AppendMenuA( parentMenu_.Handle, Win32.MF_ENABLED | Win32.MF_STRING,
															(uint)result, m.Text);
					}
				}
				return result;
			}

			public virtual MenuItem Add(string s) {
				MenuItem result = new MenuItem();
				result.Text = s;
				if( -1 == Add(result)){
					result = null;
				}
				return result;
			}

			public virtual int Add(int i, MenuItem m) {
				throw new NotImplementedException ();
			}

			public virtual MenuItem Add(string s, EventHandler e) {
				throw new NotImplementedException ();
			}

			public virtual MenuItem Add(string s, MenuItem[] items) {
				throw new NotImplementedException ();
			}

			public virtual void AddRange(MenuItem[] items) {
				throw new NotImplementedException ();
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
		
			//inherited
			//public static bool Equals(object o1, object o2) {
			//	throw new NotImplementedException ();
			//}
		
			[MonoTODO]
			public override int GetHashCode() {
				//FIXME add our proprities
				return base.GetHashCode();
			}

			public IEnumerator GetEnumerator() {
				throw new NotImplementedException ();
			}

			//public override Type GetType() {
			//	throw new NotImplementedException ();
			//}

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
			

