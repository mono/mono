// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
//

// NOT COMPLETE

using System.ComponentModel;
using System.Collections;

namespace System.Windows.Forms
{
	public class MenuItem : Menu
	{
		internal Menu parent_menu = null;
		internal bool separator;
		internal bool break_;
		internal bool bar_break;
		private Shortcut shortcut;
		private	string text;
		private bool checked_;
		private bool radiocheck;
		private bool enabled;
		private char mnemonic;
		private bool showshortcut;
		private int index;
		private bool mdilist;
		private bool defaut_item;
		private bool visible;
		private bool ownerdraw;

		public MenuItem (): base (null)
		{
			CommonConstructor (string.Empty);
			shortcut = Shortcut.None;
		}

		public MenuItem (string text) : base (null)
		{
			CommonConstructor (text);
			shortcut = Shortcut.None;
		}

		public MenuItem (string text, EventHandler e) : base (null)
		{
			CommonConstructor (text);
			shortcut = Shortcut.None;
		}

		public MenuItem (string text, MenuItem[] items) : base (items)
		{
			CommonConstructor (text);
			shortcut = Shortcut.None;
		}

		public MenuItem (string text, EventHandler onClick, Shortcut shortcut) : base (null)
		{
			CommonConstructor (text);
			Click += onClick;
			this.shortcut = shortcut;
		}

		public MenuItem (MenuMerge mergeType, int mergeOrder, Shortcut shortcut, string text,
			EventHandler onClick, EventHandler onPopup,  EventHandler onSelect,  MenuItem[] items)
			: base (items)
		{
			CommonConstructor (text);
			shortcut = shortcut;
			Click += onClick;
			Popup += onPopup;
			Select += onSelect;
		}

		private void CommonConstructor (string text)
		{
			separator = false;
			break_ = false;
			bar_break = false;
			checked_ = false;
			radiocheck = false;
			enabled = true;
			showshortcut = true;
			visible = true;
			ownerdraw = false;
			index = -1;
			mnemonic = '\0';

			Text = text;	// Text can change separator status
		}

		#region Events		
		public event EventHandler Click;		
		public event DrawItemEventHandler DrawItem;			
		public event MeasureItemEventHandler MeasureItem;
		public event EventHandler Popup;		
		public event EventHandler Select;
		#endregion // Events

		#region Public Properties

		public bool BarBreak {
			get { return break_; }
			set { break_ = value; }
		}
		
		public bool Break {
			get { return bar_break; }
			set { bar_break = value; } 
		}
		
		public bool Checked {
			get { return checked_; }
			set { checked_ = value; } 
		}		
		
		public bool DefaultItem {
			get { return defaut_item; }
			set { defaut_item = value; } 
		}
		
		public bool Enabled {
			get { return enabled; }
			set { enabled = value; } 
		}		
		
		public int Index {
			get { return index; }
			set { index = value; } 
		}
		
		public override bool IsParent {
			get {
				return IsPopup;
			}		
		}

		public bool MdiList {
			get { return mdilist; }
			set { mdilist = value; } 
		}
		
		
		protected int MenuID {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public int MergeOrder{
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public MenuMerge MergeType {
			get {
				throw new NotImplementedException ();
			}
			set{
				throw new NotImplementedException ();
			}
		}
		
		public char Mnemonic {
			get { return mnemonic; }			
		}
		
		public bool OwnerDraw {
			get { return ownerdraw; }			
			set{				
				throw new NotImplementedException ();
			}
		}
		
		public Menu Parent {
			get { return parent_menu;}
		}
		
		public bool RadioCheck {
			get { return radiocheck; }
			set { radiocheck = value; } 
		}
		
		public Shortcut Shortcut {
			get { return shortcut;}
			set {
				if (!Enum.IsDefined (typeof (Shortcut), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for Shortcut", value));

				shortcut = value;
			}
		}
		
		public bool ShowShortcut {
			get { return showshortcut;}
			set { showshortcut = value; }
		}		
		
		public string Text {
			get { return text; }
			set {
				text = value;

				if (text == "-")
					separator = true;
				else
					separator = false;

				ProcessMnemonic ();	

			}
		}
		
		public bool Visible {
			get { return visible;}
			set { visible = value; }
		}

		#endregion Public Properties

		#region Private Properties

		internal bool IsPopup {
			get {
				if (menu_items.Count > 0)
					return true;
				else
					return false;
			}			
		}

		internal bool Separator {
			get { return separator; }
			set { separator = value; }
		}

		#endregion Private Properties

		#region Public Methods

		public virtual MenuItem CloneMenu ()
		{
			throw new NotImplementedException ();
		}
		
		protected void CloneMenu (MenuItem menuitem)
		{
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void MergeMenu ()
		{
			throw new NotImplementedException ();
		}		
		
		public void MergeMenu (MenuItem menuitem)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnClick (EventArgs e)
		{
			if (Click != null)
				Click (this, e);
		}

		protected virtual void OnDrawItem (DrawItemEventArgs e)
		{
			if (DrawItem != null)
				DrawItem (this, e);
		}

		
		protected virtual void OnInitMenuPopup (EventArgs e)
		{

		}

		protected virtual void OnMeasureItem (MeasureItemEventArgs e)
		{
			if (MeasureItem != null)
				MeasureItem (this, e);			
		}

		protected virtual void OnPopup (EventArgs e)
		{
			if (Popup != null)
				Popup (this, e);
		}

		protected virtual void OnSelect (EventArgs e)
		{
			if (Select != null)
				Select (this, e);
		}
		
		public void PerformClick ()
		{
			OnClick (EventArgs.Empty);
		}

		public virtual void PerformSelect ()
		{
			OnSelect (EventArgs.Empty);
		}
		
		public override string ToString ()
		{
			return "item:" + text;
		}

		#endregion Public Methods

		#region Private Methods

		internal void Create ()
		{
			IntPtr hSubMenu = IntPtr.Zero;			

			//Console.WriteLine ("MenuItem.Created:" + Text + " parent:" + Parent.menu_handle/* + " " +
			//	Environment.StackTrace*/);
			index = MenuAPI.InsertMenuItem (Parent.menu_handle, -1, true, this, ref hSubMenu);

			if (IsPopup) {
				//Console.WriteLine ("MenuItem.Create Popup:" + hSubMenu);
				menu_handle = hSubMenu;
				CreateItems ();
			}
		}

		private void ProcessMnemonic ()
		{
			if (text.Length < 2) {
				mnemonic = '\0';
				return;
			}

			bool bPrevAmp = false;
			for (int i = 0; i < text.Length -1 ; i++) {
				if (text[i] == '&') {
					if (bPrevAmp == false &&  (text[i+1] != '&')) {
						mnemonic = Char.ToUpper (text[i+1]);
						return;
					}

					bPrevAmp = true;
				}
				else
					bPrevAmp = false;
			}

			mnemonic = '\0';
		}
		
		private string GetShortCutTextCtrl () { return "Ctrl"; }

		internal string GetShortCutText ()
		{
			//TODO: Complete the table
			switch (Shortcut)
			{
				case Shortcut.Ctrl0:
					return GetShortCutTextCtrl () + "+0";
				case Shortcut.Ctrl1:	
					return GetShortCutTextCtrl () + "+1";
				case Shortcut.Ctrl2:	
					return GetShortCutTextCtrl () + "+2";
				case Shortcut.Ctrl3:	
					return GetShortCutTextCtrl () + "+3";
				case Shortcut.Ctrl4:	
					return GetShortCutTextCtrl () + "+4";
				case Shortcut.Ctrl5:	
					return GetShortCutTextCtrl () + "+5";
				case Shortcut.Ctrl6:	
					return GetShortCutTextCtrl () + "+6";
				case Shortcut.Ctrl7:	
					return GetShortCutTextCtrl () + "+7";
				case Shortcut.Ctrl8:	
					return GetShortCutTextCtrl () + "+8";
				case Shortcut.Ctrl9:	
					return GetShortCutTextCtrl () + "+9";
				case Shortcut.CtrlA:	
					return GetShortCutTextCtrl () + "+A";
				case Shortcut.CtrlB:	
					return GetShortCutTextCtrl () + "+B";
				case Shortcut.CtrlC:	
					return GetShortCutTextCtrl () + "+C";
				case Shortcut.CtrlD:	
					return GetShortCutTextCtrl () + "+D";
				case Shortcut.CtrlDel:	
					return GetShortCutTextCtrl () + "+Del";
				case Shortcut.CtrlE:	
					return GetShortCutTextCtrl () + "+E";
				case Shortcut.CtrlF:	
					return GetShortCutTextCtrl () + "+F";
				case Shortcut.CtrlF1:	
					return GetShortCutTextCtrl () + "+F1";
				case Shortcut.CtrlF10:	
					return GetShortCutTextCtrl () + "+F10";
				case Shortcut.CtrlF11:	
					return GetShortCutTextCtrl () + "+F11";
				case Shortcut.CtrlF12:	
					return GetShortCutTextCtrl () + "+F12";
				case Shortcut.CtrlF2:	
					return GetShortCutTextCtrl () + "+F2";
				case Shortcut.CtrlF3:	
					return GetShortCutTextCtrl () + "+F3";
				case Shortcut.CtrlF4:	
					return GetShortCutTextCtrl () + "+F4";
				case Shortcut.CtrlF5:	
					return GetShortCutTextCtrl () + "+F5";
				case Shortcut.CtrlF6:	
					return GetShortCutTextCtrl () + "+F6";
				case Shortcut.CtrlF7:	
					return GetShortCutTextCtrl () + "+F7";
				case Shortcut.CtrlF8:	
					return GetShortCutTextCtrl () + "+F8";
				case Shortcut.CtrlF9:	
					return GetShortCutTextCtrl () + "+F9";
				case Shortcut.CtrlG:	
					return GetShortCutTextCtrl () + "+G";
				case Shortcut.CtrlH:	
					return GetShortCutTextCtrl () + "+H";
				case Shortcut.CtrlI:	
					return GetShortCutTextCtrl () + "+I";
				case Shortcut.CtrlIns:	
					return GetShortCutTextCtrl () + "+Ins";
				case Shortcut.CtrlJ:	
					return GetShortCutTextCtrl () + "+J";
				case Shortcut.CtrlK:	
					return GetShortCutTextCtrl () + "+K";
				case Shortcut.CtrlL:	
					return GetShortCutTextCtrl () + "+L";
				case Shortcut.CtrlM:	
					return GetShortCutTextCtrl () + "+M";
				case Shortcut.CtrlN:	
					return GetShortCutTextCtrl () + "+N";
				case Shortcut.CtrlO:	
					return GetShortCutTextCtrl () + "+O";
				case Shortcut.CtrlP:	
					return GetShortCutTextCtrl () + "+P";
				case Shortcut.CtrlQ:	
					return GetShortCutTextCtrl () + "+Q";
				case Shortcut.CtrlR:	
					return GetShortCutTextCtrl () + "+R";
				case Shortcut.CtrlS:
					return GetShortCutTextCtrl () + "+S";									
				case Shortcut.CtrlT:
					return GetShortCutTextCtrl () + "+T";
				case Shortcut.CtrlU:
					return GetShortCutTextCtrl () + "+U";
				case Shortcut.CtrlV:
					return GetShortCutTextCtrl () + "+V";				
				case Shortcut.CtrlW:
					return GetShortCutTextCtrl () + "+W";
				case Shortcut.CtrlX:
					return GetShortCutTextCtrl () + "+X";				
				case Shortcut.CtrlY:
					return GetShortCutTextCtrl () + "+Y";				
				case Shortcut.CtrlZ:
					return GetShortCutTextCtrl () + "+Z";				
				default:
					return "";
			}
		}

		#endregion Private Methods

	}
}


