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
		private int menuid;
		private int mergeorder;
		private MenuMerge mergetype;

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

		public MenuItem (string text, EventHandler onClick) : base (null)
		{
			CommonConstructor (text);
			shortcut = Shortcut.None;
			Click += onClick;
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
			mergeorder = mergeOrder;
			mergetype = mergeType;

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
			menuid = -1;
			mergeorder = 0;
			mergetype = MenuMerge.Add;
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
			get { return IsPopup; }
		}

		public bool MdiList {
			get { return mdilist; }
			set { mdilist = value; }
		}

		protected int MenuID {
			get { return menuid; }
		}

		public int MergeOrder {
			get { return mergeorder; }
			set { mergeorder = value; }
		}

		public MenuMerge MergeType {
			get { return mergetype;	}
			set {
				if (!Enum.IsDefined (typeof (MenuMerge), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for MenuMerge", value));

				mergetype = value;
			}
		}

		public char Mnemonic {
			get { return mnemonic; }
		}

		public bool OwnerDraw {
			get { return ownerdraw; }
			set { ownerdraw = value; }
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
			MenuItem item = new MenuItem ();
			item.CloneMenu (item);
			return item;
		}

		protected void CloneMenu (MenuItem menuitem)
		{
			base.CloneMenu (menuitem); // Copy subitems

			// Properties
			BarBreak = menuitem.BarBreak;
			Break = menuitem.Break;
			Checked = menuitem.Checked;
			DefaultItem = menuitem.DefaultItem;
			Enabled = menuitem.Enabled;			
			MergeOrder = menuitem.MergeOrder;
			MergeType = menuitem.MergeType;
			OwnerDraw = menuitem.OwnerDraw;
			//Parent = menuitem.Parent;
			RadioCheck = menuitem.RadioCheck;
			Shortcut = menuitem.Shortcut;
			ShowShortcut = menuitem.ShowShortcut;
			Text = menuitem.Text;
			Visible = menuitem.Visible;

			// Events
			Click = menuitem.Click;
			DrawItem = menuitem.DrawItem;
			MeasureItem = menuitem.MeasureItem;
			Popup = menuitem.Popup;
			Select = menuitem.Select;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);			
		}

		public virtual void MergeMenu ()
		{
			base.MergeMenu (this);
		}

		public void MergeMenu (MenuItem menuitem)
		{
			base.MergeMenu (menuitem);
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
			OnPopup (e);
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

			menuid = index = MenuAPI.InsertMenuItem (Parent.Handle, -1, true, this, ref hSubMenu);
			IsDirty = false;

			if (IsPopup) {
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
		private string GetShortCutTextAlt () { return "Alt"; }
		private string GetShortCutTextShift () { return "Shift"; }		

		internal string GetShortCutText ()
		{
			/* Ctrl+A - Ctrl+Z */
			if (Shortcut >= Shortcut.CtrlA && Shortcut <= Shortcut.CtrlZ)
				return GetShortCutTextCtrl () + "+" + (char)((int) 'A' + (int)(Shortcut - Shortcut.CtrlA));

			/* Alt+0 - Alt+9 */
			if (Shortcut >= Shortcut.Alt0 && Shortcut <= Shortcut.Alt9)
				return GetShortCutTextAlt () + "+" + (char)((int) '0' + (int)(Shortcut - Shortcut.Alt0));

			/* Alt+F1 - Alt+F2 */
			if (Shortcut >= Shortcut.AltF1 && Shortcut <= Shortcut.AltF9)
				return GetShortCutTextAlt () + "+F" + (char)((int) '1' + (int)(Shortcut - Shortcut.AltF1));

			/* Ctrl+0 - Ctrl+9 */
			if (Shortcut >= Shortcut.Ctrl0 && Shortcut <= Shortcut.Ctrl9)
				return GetShortCutTextCtrl () + "+" + (char)((int) '0' + (int)(Shortcut - Shortcut.Ctrl0));
							
			/* Ctrl+F0 - Ctrl+F9 */
			if (Shortcut >= Shortcut.CtrlF1 && Shortcut <= Shortcut.CtrlF9)
				return GetShortCutTextCtrl () + "+F" + (char)((int) '1' + (int)(Shortcut - Shortcut.CtrlF1));
				
			/* Ctrl+Shift+0 - Ctrl+Shift+9 */
			if (Shortcut >= Shortcut.CtrlShift0 && Shortcut <= Shortcut.CtrlShift9)
				return GetShortCutTextCtrl () + "+" + GetShortCutTextShift () + "+" + (char)((int) '0' + (int)(Shortcut - Shortcut.CtrlShift0));
				
			/* Ctrl+Shift+A - Ctrl+Shift+Z */
			if (Shortcut >= Shortcut.CtrlShiftA && Shortcut <= Shortcut.CtrlShiftZ)
				return GetShortCutTextCtrl () + "+" + GetShortCutTextShift () + "+" + (char)((int) 'A' + (int)(Shortcut - Shortcut.CtrlShiftA));

			/* Ctrl+Shift+F1 - Ctrl+Shift+F9 */
			if (Shortcut >= Shortcut.CtrlShiftF1 && Shortcut <= Shortcut.CtrlShiftF9)
				return GetShortCutTextCtrl () + "+" + GetShortCutTextShift () + "+F" + (char)((int) '1' + (int)(Shortcut - Shortcut.CtrlShiftF1));
				
			/* F1 - F9 */
			if (Shortcut >= Shortcut.F1 && Shortcut <= Shortcut.F9)
				return "F" + (char)((int) '1' + (int)(Shortcut - Shortcut.F1));
				
			/* Shift+F1 - Shift+F9 */
			if (Shortcut >= Shortcut.ShiftF1 && Shortcut <= Shortcut.ShiftF9)
				return GetShortCutTextShift () + "+F" + (char)((int) '1' + (int)(Shortcut - Shortcut.ShiftF1));
			
			/* Special cases */
			switch (Shortcut) {
				case Shortcut.AltBksp:
					return "AltBksp";
				case Shortcut.AltF10:
					return GetShortCutTextAlt () + "+F10";
				case Shortcut.AltF11:
					return GetShortCutTextAlt () + "+F11";
				case Shortcut.AltF12:
					return GetShortCutTextAlt () + "+F12";
				case Shortcut.CtrlDel:		
					return GetShortCutTextCtrl () + "+Del";
				case Shortcut.CtrlF10:
					return GetShortCutTextCtrl () + "+F10";
				case Shortcut.CtrlF11:
					return GetShortCutTextCtrl () + "+F11";
				case Shortcut.CtrlF12:
					return GetShortCutTextCtrl () + "+F12";
				case Shortcut.CtrlIns:
					return GetShortCutTextCtrl () + "+Ins";
				case Shortcut.CtrlShiftF10:
					return GetShortCutTextCtrl () + "+" + GetShortCutTextShift () + "+F10";
				case Shortcut.CtrlShiftF11:
					return GetShortCutTextCtrl () + "+" + GetShortCutTextShift () + "+F11";
				case Shortcut.CtrlShiftF12:
					return GetShortCutTextCtrl () + "+" + GetShortCutTextShift () + "+F12";
				case Shortcut.Del:
					return "Del";
				case Shortcut.F10:
					return "F10";	
				case Shortcut.F11:
					return "F11";	
				case Shortcut.F12:
					return "F12";	
				case Shortcut.Ins:
					return "Ins";	
				case Shortcut.None:
					return "None";	
				case Shortcut.ShiftDel:
					return GetShortCutTextShift () + "+Del";
				case Shortcut.ShiftF10:
					return GetShortCutTextShift () + "+F10";
				case Shortcut.ShiftF11:
					return GetShortCutTextShift () + "+F11";
				case Shortcut.ShiftF12:
					return GetShortCutTextShift () + "+F12";				
				case Shortcut.ShiftIns:
					return GetShortCutTextShift () + "+Ins";
				default:
					break;
				}
				
			return "";
		}

		#endregion Private Methods

	}
}


