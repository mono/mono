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
using System.Text;

namespace System.Windows.Forms {

	/// <summary>
	/// ToDo note:
	/// </summary>

	public class MenuItem : Menu {
		//
		// - Constructor
		//
		public MenuItem() : base(null) {
		}

		public MenuItem(string s) : this(){
			text_ = s;
		}

		public MenuItem(string s, EventHandler e) : this() {
			text_ = s;
			Click += e;
		}

		public MenuItem(string s, MenuItem[] items) : base(items) {
			text_ = s;
		}

		public MenuItem(string s, EventHandler e, Shortcut sc) : this() {
			throw new NotImplementedException ();
		}

		public MenuItem(MenuMerge mm, int i, Shortcut sc, string s, EventHandler e, EventHandler e1, EventHandler e2, MenuItem[] items)  : base(items){
			throw new NotImplementedException ();
		}

		//
		// -- Public Methods
		//

		public virtual MenuItem CloneMenu() {
			MenuItem result = new MenuItem();
			result.CloneMenu(this);
			return result;
		}

		public override ObjRef CreateObjRef(Type t) {
			throw new NotImplementedException ();
		}

		public override bool Equals(object o) {
			return base.Equals(o);
		}

		[MonoTODO]
		public override int GetHashCode() {
			//FIXME add our proprities
			return base.GetHashCode();
		}

		public virtual MenuItem MergeMenu() {
			throw new NotImplementedException ();
		}

		public void MergeMenu(MenuItem m) {
			throw new NotImplementedException ();
		}

		public void PerformClick() {
			OnClick( new EventArgs());
		}

		public virtual void PerformSelect() {
			throw new NotImplementedException ();
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.Append(base.ToString());
			sb.AppendFormat(", Items.Count: {0}, Text: {1}", MenuItems.Count, Text);	
			return sb.ToString();
		}

		//
		// -- Protected Methods
		//

		protected void CloneMenu(MenuItem m) {
			Text = m.Text;
			Click += m.Click;
			if( m.MenuItems.Count != 0){
				MenuItem[] all_items = new MenuItem[m.MenuItems.Count];
				m.MenuItems.CopyTo(all_items, 0);
				MenuItems.AddRange(all_items);
			}
		}

		~MenuItem() {
			throw new NotImplementedException ();
		}

		protected virtual void OnClick(EventArgs e) {
			if( Click != null){
				Click(this,e);
			}
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
/*
		// completely inherit from base class
		public new IntPtr Handle {
			get {
				throw new NotImplementedException ();
			}
		}
*/
		protected int index_ = -1;
		public int Index {

			get {
				return index_;
			}
			set {
				index_ = value;
			}
		}

		public override bool IsParent {

			get {
				return base.IsParent;
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

        internal void SetParent( Menu parent) {
        	// FIXME: set exception parameters
        	if( parent != null && parent_ != null)
        		throw new System.Exception();
        	parent_ = parent;
        }
        
		public Menu Parent {

			get {
				return parent_;
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

		internal const int INVALID_MENU_ID = -1; //0xffffffff;
		// Variables are stored here to provide access for the base functions
		protected int MenuID_ = INVALID_MENU_ID;

		// Provides unique id to all items in all menus, hopefully space is enougth.
		// Possible to use array to keep ids from deleted menu items
		// and reuse them.
		protected static int MenuIDs_ = 1;

		protected int GetNewMenuID()
		{
			return MenuIDs_++;
		}

		protected int MenuID {

			get {
				if( MenuID_ == INVALID_MENU_ID) {
					MenuID_ = GetNewMenuID();
				}
				return (int)MenuID_;
			}
		}

		//
		// Btw, this function is funky, it is being used by routines that are supposed
		// to be passing an IntPtr to the AppendMenu function
		//
		internal int GetID()
		{
			return MenuID;
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
