//
// System.Windows.Forms.Menu.MenuItem.cs
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
using System.Runtime.Remoting;
using System.ComponentModel;
namespace System.Windows.Forms {

	/// <summary>
	/// ToDo note:
	///  - Nothing is implemented
	/// </summary>

	public class MenuItem : Menu {
		//
		// - Constructor
		//
		public MenuItem() : base(null) {
		}

		public MenuItem(string s) : base(null) {
			throw new NotImplementedException ();
		}

		public MenuItem(string s, EventHandler e) : base(null) {
			throw new NotImplementedException ();
		}

		public MenuItem(string s, MenuItem[] items) : base(null) {
			throw new NotImplementedException ();
		}

		public MenuItem(string s, EventHandler e, Shortcut sc) : base(null) {
			throw new NotImplementedException ();
		}

		public MenuItem(MenuMerge mm, int i, Shortcut sc, string s, EventHandler e, EventHandler e1, EventHandler e2, MenuItem[] items)  : base(null){
			throw new NotImplementedException ();
		}

		//
		// -- Public Methods
		//

		public virtual MenuItem CloneMenu() {
			throw new NotImplementedException ();
		}

		public override ObjRef CreateObjRef(Type t) {
			throw new NotImplementedException ();
		}
        //inherited
		//public void Dispose()
		//{
		//        throw new NotImplementedException ();
		//}
		//
		//public override void Dispose(bool disposing)
		//{
		//        throw new NotImplementedException ();
		//}
		//public override bool Equals(object o, object o1) {
		//	throw new NotImplementedException ();
		//}
                
		public override bool Equals(object o) {
			throw new NotImplementedException ();
		}
                
		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}
		//inherited	
		//public ContextMenu GetContextMenu() {
		//	throw new NotImplementedException ();
		//}
                
		//public object GetLifetimeService() {
		//	throw new NotImplementedException ();
		//}
        //        
		//public MainMenu GetMainMenu() {
		//	throw new NotImplementedException ();
		//}
                
		//public Type GetType() {
		//	throw new NotImplementedException ();
		//}
                
		//public virtual object InitializeLifetimeService() {
		//	throw new NotImplementedException ();
		//}
                
		public virtual MenuItem MergeMenu() {
			throw new NotImplementedException ();
		}
                
		public void MergeMenu(MenuItem m) {
			throw new NotImplementedException ();
		}
                
		//inherited
		//public virtual void MergeMenu(Menu m) {
		//	throw new NotImplementedException ();
		//}
                
		public void PerformClick() {
			throw new NotImplementedException ();
		}
                
		public virtual void PerformSelect() {
			throw new NotImplementedException ();
		}
                
		public override string ToString() {
			throw new NotImplementedException ();
		}
                
		//
		// -- Protected Methods
		//
                
		protected void CloneMenu(MenuItem m) {
			throw new NotImplementedException ();
		}
                
		//inherited
		//protected void CloneMenu(Menu m) {
		//	throw new NotImplementedException ();
		//}
                
		//protected override void Dispose(bool disposing)
		//{
		//        throw new NotImplementedException ();
		//}
                
		~MenuItem() {
			throw new NotImplementedException ();
		}
                
		//protected virtual object GetService(Type t) {
		//	throw new NotImplementedException ();
		//}
                
		//protected object MemberwiseClone() {
		//	throw new NotImplementedException ();
		//}
                
		protected virtual void OnClick(EventArgs e) {
			throw new NotImplementedException ();
		}
                
		protected virtual void OnDrawItem(DrawItemEventArgs e) {
			throw new NotImplementedException ();
		}
                
		protected virtual void OnMeasureItem(MeasureItemEventArgs e) {
			throw new NotImplementedException ();
		}
                
		protected virtual void OnPopUp(EventArgs e) {
			throw new NotImplementedException ();
		}
                
		protected virtual void OnSelect(EventArgs e) {
			throw new NotImplementedException ();
		}
                
		//
		// -- Public Properties
		//
                
		public bool BarBreak {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		public bool Break {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		public bool Checked {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		//public IContainer Container {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
                
		public bool DefaultItem {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		public bool Enabled {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		//public IntPtr Handle {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
                
		public int Index {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		public override bool IsParent {

			get {
				throw new NotImplementedException ();
			}
		}
                
		public bool MdiList {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		//inherited
		//public MenuItem MdiListItem {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}        
		//public Menu.MenuItemCollection MenuItems {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
                
		public int MergeOrder {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		public MenuMerge MergeType {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		public char Mnemonic {

			get {
				throw new NotImplementedException ();
			}
		}
                
		public bool OwnerDraw {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		public Menu Parent {

			get {
				throw new NotImplementedException ();
			}
		}
                
		public bool RadioCheck {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		public Shortcut Shortcut {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		//inherited
		//public virtual ISite Site {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//	set {
		//		throw new NotImplementedException ();
		//	}
		//}
                
		private string text_ = String.Empty;

		public string Text {
			get {
				return text_;
			}
			set {
				text_ = value;
			}
		}
                
		public bool Visible {

			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
                
		//
		// -- Protected Properties
		//
                
		//inherited
		//protected bool DesignMode {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}      
		//protected EventHandlerList Events {
		//	get {
		//		throw new NotImplementedException ();
		//	}
		//}
                
		protected int MenuID {

			get {
				throw new NotImplementedException ();
			}
		}
                
		//
		// -- Public Events
		//
                
		public event EventHandler Click;
		//inherited
		//public event EventHandler Disposed;
		public event DrawItemEventHandler DrawItem;
		public event MeasureItemEventHandler MeasureItem;
		public event EventHandler PopUp;
		public event EventHandler Select;
	}
}
