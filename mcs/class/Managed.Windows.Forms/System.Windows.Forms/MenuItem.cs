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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
//

// NOT COMPLETE

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Text;

namespace System.Windows.Forms
{
	[DefaultProperty("Text")]
	[DefaultEvent("Click")]
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	public class MenuItem : Menu
	{
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
		private Hashtable mdilist_items;
		private Hashtable mdilist_forms;
		private MdiClient mdicontainer;
		private bool is_window_menu_item;
		private bool defaut_item;
		private bool visible;
		private bool ownerdraw;
		private int menuid;
		private int mergeorder;
		private int xtab;
		private int menuheight;
		private bool menubar;
		private MenuMerge mergetype;
		// UIA Framework Note: Used to obtain item bounds
		internal Rectangle bounds;
		
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
			this.shortcut = shortcut;
			mergeorder = mergeOrder;
			mergetype = mergeType;

			Click += onClick;
			Popup += onPopup;
			Select += onSelect;
		}

		private void CommonConstructor (string text)
		{
			defaut_item = false;
			separator = false;
			break_ = false;
			bar_break = false;
			checked_ = false;
			radiocheck = false;
			enabled = true;
			showshortcut = true;
			visible = true;
			ownerdraw = false;
			menubar = false;
			menuheight = 0;
			xtab = 0;
			index = -1;
			mnemonic = '\0';
			menuid = -1;
			mergeorder = 0;
			mergetype = MenuMerge.Add;
			Text = text;	// Text can change separator status
		}

		#region Events
		static object ClickEvent = new object ();
		static object DrawItemEvent = new object ();
		static object MeasureItemEvent = new object ();
		static object PopupEvent = new object ();
		static object SelectEvent = new object ();

		public event EventHandler Click {
			add { Events.AddHandler (ClickEvent, value); }
			remove { Events.RemoveHandler (ClickEvent, value); }
		}

		public event DrawItemEventHandler DrawItem {
			add { Events.AddHandler (DrawItemEvent, value); }
			remove { Events.RemoveHandler (DrawItemEvent, value); }
		}

		public event MeasureItemEventHandler MeasureItem {
			add { Events.AddHandler (MeasureItemEvent, value); }
			remove { Events.RemoveHandler (MeasureItemEvent, value); }
		}

		public event EventHandler Popup {
			add { Events.AddHandler (PopupEvent, value); }
			remove { Events.RemoveHandler (PopupEvent, value); }
		}

		public event EventHandler Select {
			add { Events.AddHandler (SelectEvent, value); }
			remove { Events.RemoveHandler (SelectEvent, value); }
		}
		
		#region UIA Framework Events

		static object UIACheckedChangedEvent = new object ();

		internal event EventHandler UIACheckedChanged {
			add { Events.AddHandler (UIACheckedChangedEvent, value); }
			remove { Events.RemoveHandler (UIACheckedChangedEvent, value); }
		}

		internal void OnUIACheckedChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIACheckedChangedEvent];
			if (eh != null)
				eh (this, e);
		}
		
		static object UIARadioCheckChangedEvent = new object ();

		internal event EventHandler UIARadioCheckChanged {
			add { Events.AddHandler (UIARadioCheckChangedEvent, value); }
			remove { Events.RemoveHandler (UIARadioCheckChangedEvent, value); }
		}

		internal void OnUIARadioCheckChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIARadioCheckChangedEvent];
			if (eh != null)
				eh (this, e);
		}
		
		static object UIAEnabledChangedEvent = new object ();

		internal event EventHandler UIAEnabledChanged {
			add { Events.AddHandler (UIAEnabledChangedEvent, value); }
			remove { Events.RemoveHandler (UIAEnabledChangedEvent, value); }
		}

		internal void OnUIAEnabledChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIAEnabledChangedEvent];
			if (eh != null)
				eh (this, e);
		}
		
		static object UIATextChangedEvent = new object ();

		internal event EventHandler UIATextChanged {
			add { Events.AddHandler (UIATextChangedEvent, value); }
			remove { Events.RemoveHandler (UIATextChangedEvent, value); }
		}

		internal void OnUIATextChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIATextChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		#endregion
		#endregion // Events

		#region Public Properties

		[Browsable(false)]
		[DefaultValue(false)]
		public bool BarBreak {
			get { return break_; }
			set { break_ = value; }
		}

		[Browsable(false)]
		[DefaultValue(false)]
		public bool Break {
			get { return bar_break; }
			set { bar_break = value; }
		}

		[DefaultValue(false)]
		public bool Checked {
			get { return checked_; }
			set {
				if (checked_ == value)
					return;
				
				checked_ = value;

				// UIA Framework Event: Checked Changed
				OnUIACheckedChanged (EventArgs.Empty);
			}
		}

		[DefaultValue(false)]
		public bool DefaultItem {
			get { return defaut_item; }
			set { defaut_item = value; }
		}

		[DefaultValue(true)]
		[Localizable(true)]
		public bool Enabled {
			get { return enabled; }
			set {
				if (enabled == value)
					return;
					
				enabled = value;

				// UIA Framework Event: Enabled Changed
				OnUIAEnabledChanged (EventArgs.Empty);

				Invalidate ();
			}
		}

		[Browsable(false)]
		public int Index {
			get { return index; }
			set { 
				if (Parent != null && Parent.MenuItems != null && (value < 0 || value >= Parent.MenuItems.Count))
					throw new ArgumentException ("'" + value + "' is not a valid value for 'value'");
				index = value; 
			}
		}

		[Browsable(false)]
		public override bool IsParent {
			get { return IsPopup; }
		}

		[DefaultValue(false)]
		public bool MdiList {
			get { return mdilist; }
			set {
				if (mdilist == value)
					return;
				mdilist = value;

				if (mdilist || mdilist_items == null)
					return;

				foreach (MenuItem item in mdilist_items.Keys)
					MenuItems.Remove (item);
				mdilist_items.Clear ();
				mdilist_items = null;
			}
		}

		protected int MenuID {
			get { return menuid; }
		}

		[DefaultValue(0)]
		public int MergeOrder {
			get { return mergeorder; }
			set { mergeorder = value; }
		}

		[DefaultValue(MenuMerge.Add)]
		public MenuMerge MergeType {
			get { return mergetype;	}
			set {
				if (!Enum.IsDefined (typeof (MenuMerge), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for MenuMerge", value));

				mergetype = value;
			}
		}

		[Browsable(false)]
		public char Mnemonic {
			get { return mnemonic; }
		}

		[DefaultValue(false)]
		public bool OwnerDraw {
			get { return ownerdraw; }
			set { ownerdraw = value; }
		}

		[Browsable(false)]
		public Menu Parent {
			get { return parent_menu;}
		}

		[DefaultValue(false)]
		public bool RadioCheck {
			get { return radiocheck; }
			set {
				if (radiocheck == value)
					return;
				
				radiocheck = value;

				// UIA Framework Event: Checked Changed
				OnUIARadioCheckChanged (EventArgs.Empty);
			}
		}

		[DefaultValue(Shortcut.None)]
		[Localizable(true)]
		public Shortcut Shortcut {
			get { return shortcut;}
			set {
				if (!Enum.IsDefined (typeof (Shortcut), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for Shortcut", value));

				shortcut = value;
				UpdateMenuItem ();
			}
		}

		[DefaultValue(true)]
		[Localizable(true)]
		public bool ShowShortcut {
			get { return showshortcut;}
			set { showshortcut = value; }
		}

		[Localizable(true)]
		public string Text {
			get { return text; }
			set {
				text = value;

				if (text == "-")
					separator = true;
				else
					separator = false;

				// UIA Framework Event: Text Changed
				OnUIATextChanged (EventArgs.Empty);

				ProcessMnemonic ();
				Invalidate ();
			}
		}

		[DefaultValue(true)]
		[Localizable(true)]
		public bool Visible {
			get { return visible;}
			set { 
				if (value == visible)
					return;

				visible = value;

				if (menu_items != null) {
					foreach (MenuItem mi in menu_items)
						mi.Visible = value;
				}

				if (parent_menu != null)
					parent_menu.OnMenuChanged (EventArgs.Empty);
			}
		}

		#endregion Public Properties

		#region Private Properties

		internal new int Height {
			get { return bounds.Height; }
			set { bounds.Height = value; }
		}

		internal bool IsPopup {
			get {
				if (menu_items.Count > 0)
					return true;
				else
					return false;
			}
		}
		
		internal bool MeasureEventDefined {
			get { 
				if (ownerdraw == true && Events [MeasureItemEvent] != null) {
					return true;
				} else {
					return false;
				}
			}
		}
		
		internal bool MenuBar {
			get { return menubar; }
			set { menubar = value; }
		}
		
		internal int MenuHeight {
			get { return menuheight; }
			set { menuheight = value; }
		}	

		bool selected;
		internal bool Selected {
			get { return selected; }
			set { selected = value; }
		}

		internal bool Separator {
			get { return separator; }
			set { separator = value; }
		}
		
		internal DrawItemState Status {
			get {
				DrawItemState status = DrawItemState.None;
				MenuTracker tracker = Parent.Tracker;
				if (Selected)
					status |= (tracker.active || tracker.Navigating ? DrawItemState.Selected : DrawItemState.HotLight);
				if (!Enabled)
					status |= DrawItemState.Grayed | DrawItemState.Disabled;
				if (Checked)
					status |= DrawItemState.Checked;
				if (!tracker.Navigating)
					status |= DrawItemState.NoAccelerator;
				return status;
			}
		}
		
		internal bool VisibleItems {
			get { 
				if (menu_items != null) {
					foreach (MenuItem mi in menu_items)
						if (mi.Visible)
							return true;
				}
				return false;
			}
		}

		internal new int Width {
			get { return bounds.Width; }
			set { bounds.Width = value; }
		}

		internal new int X {
			get { return bounds.X; }
			set { bounds.X = value; }
		}

		internal int XTab {
			get { return xtab; }
			set { xtab = value; }
		}

		internal new int Y {
			get { return bounds.Y; }
			set { bounds.Y = value; }
		}

		#endregion Private Properties

		#region Public Methods

		public virtual MenuItem CloneMenu ()
		{
			MenuItem item = new MenuItem ();
			item.CloneMenu (this);
			return item;
		}

		protected void CloneMenu (MenuItem itemSrc)
		{
			base.CloneMenu (itemSrc); // Copy subitems

			// Window list
			MdiList = itemSrc.MdiList;
			is_window_menu_item = itemSrc.is_window_menu_item;
			// Remove items corresponding to window menu items, and add new items
			// (Otherwise window menu items would show up twice, since the PopulateWindowMenu doesn't
			// now them)
			bool populated = false;
			for (int i = MenuItems.Count - 1; i >= 0; i--) {
				if (MenuItems [i].is_window_menu_item) {
					MenuItems.RemoveAt (i);
					populated = true;
				}
			}
			if (populated)
				PopulateWindowMenu ();

			// Properties
			BarBreak = itemSrc.BarBreak;
			Break = itemSrc.Break;
			Checked = itemSrc.Checked;
			DefaultItem = itemSrc.DefaultItem;
			Enabled = itemSrc.Enabled;			
			MergeOrder = itemSrc.MergeOrder;
			MergeType = itemSrc.MergeType;
			OwnerDraw = itemSrc.OwnerDraw;
			//Parent = menuitem.Parent;
			RadioCheck = itemSrc.RadioCheck;
			Shortcut = itemSrc.Shortcut;
			ShowShortcut = itemSrc.ShowShortcut;
			Text = itemSrc.Text;
			Visible = itemSrc.Visible;
			Name = itemSrc.Name;
			Tag = itemSrc.Tag;

			// Events
			Events[ClickEvent] = itemSrc.Events[ClickEvent];
			Events[DrawItemEvent] = itemSrc.Events[DrawItemEvent];
			Events[MeasureItemEvent] = itemSrc.Events[MeasureItemEvent];
			Events[PopupEvent] = itemSrc.Events[PopupEvent];
			Events[SelectEvent] = itemSrc.Events[SelectEvent];
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && parent_menu != null)
				parent_menu.MenuItems.Remove (this);
				
			base.Dispose (disposing);			
		}

		// This really clones the item
		public virtual MenuItem MergeMenu ()
		{
			MenuItem item = new MenuItem ();
			item.CloneMenu (this);
			return item;
		}

		public void MergeMenu (MenuItem itemSrc)
		{
			base.MergeMenu (itemSrc);
		}

		protected virtual void OnClick (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDrawItem (DrawItemEventArgs e)
		{
			DrawItemEventHandler eh = (DrawItemEventHandler)(Events [DrawItemEvent]);
			if (eh != null)
				eh (this, e);
		}


		protected virtual void OnInitMenuPopup (EventArgs e)
		{
			OnPopup (e);
		}

		protected virtual void OnMeasureItem (MeasureItemEventArgs e)
		{
			if (!OwnerDraw)
				return;

			MeasureItemEventHandler eh = (MeasureItemEventHandler)(Events [MeasureItemEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnPopup (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [PopupEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnSelect (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [SelectEvent]);
			if (eh != null)
				eh (this, e);
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
			return base.ToString () + ", Items.Count: " + MenuItems.Count + ", Text: " + text;
		}

		#endregion Public Methods

		#region Private Methods

		internal virtual void Invalidate ()
		{
			if ((Parent == null) || !(Parent is MainMenu) || (Parent.Wnd == null))
				return;
				
			Form form = Parent.Wnd.FindForm ();
			if ((form == null) || (!form.IsHandleCreated))
				return;
			
			XplatUI.RequestNCRecalc (form.Handle);
		}

		internal void PerformPopup ()
		{
			OnPopup (EventArgs.Empty);
		}

		internal void PerformDrawItem (DrawItemEventArgs e)
		{
			PopulateWindowMenu ();
			if (OwnerDraw)
				OnDrawItem (e);
			else
				ThemeEngine.Current.DrawMenuItem (this, e);
		}
		
		private void PopulateWindowMenu ()
		{
			if (mdilist) {
				if (mdilist_items == null) {
					mdilist_items = new Hashtable ();
					mdilist_forms = new Hashtable ();
				}
				
				do {
					MainMenu main = GetMainMenu ();
					if (main == null || main.GetForm () == null)
						break;

					Form form = main.GetForm ();
					mdicontainer = form.MdiContainer;
					if (mdicontainer == null)
						break;

					
					// Remove closed forms
					MenuItem[] items = new MenuItem[mdilist_items.Count];
					mdilist_items.Keys.CopyTo (items, 0);
					foreach (MenuItem item in items) {
						Form mdichild = (Form) mdilist_items [item];
						if (!mdicontainer.mdi_child_list.Contains(mdichild)) {
							mdilist_items.Remove (item);
							mdilist_forms.Remove (mdichild);
							MenuItems.Remove (item);
						}
					}
					
					// Add new forms and update state for existing forms.
					for (int i = 0; i < mdicontainer.mdi_child_list.Count; i++) {
						Form mdichild = (Form)mdicontainer.mdi_child_list[i];
						MenuItem item;
						if (mdilist_forms.Contains (mdichild)) {
							item = (MenuItem) mdilist_forms [mdichild];
						} else {
							item = new MenuItem ();
							item.is_window_menu_item = true;
							item.Click += new EventHandler (MdiWindowClickHandler);
							mdilist_items [item] = mdichild;
							mdilist_forms [mdichild] = item;
							MenuItems.AddNoEvents (item);
						}
						item.Visible = mdichild.Visible;
						item.Text = "&" + (i + 1).ToString () + " " + mdichild.Text;
						item.Checked = form.ActiveMdiChild == mdichild;
					}
				} while (false);
			} else {
				// Remove all forms
				if (mdilist_items != null) {
					foreach (MenuItem item in mdilist_items.Values) {
						MenuItems.Remove (item);
					}
					
					mdilist_forms.Clear ();
					mdilist_items.Clear ();
				}
			}
		}
		
		internal void PerformMeasureItem (MeasureItemEventArgs e)
		{
			OnMeasureItem (e);
		}

		private void ProcessMnemonic ()
		{
			if (text == null || text.Length < 2) {
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

		private void MdiWindowClickHandler (object sender, EventArgs e)
		{
			Form mdichild = (Form) mdilist_items [sender];

			// people could add weird items to the Window menu
			// so we can't assume its just us
			if (mdichild == null)
				return;

			mdicontainer.ActivateChild (mdichild);
		}

		private void UpdateMenuItem ()
		{
			if ((parent_menu == null) || (parent_menu.Tracker == null))
				return;

			parent_menu.Tracker.RemoveShortcuts (this);
			parent_menu.Tracker.AddShortcuts (this);
		}

		#endregion Private Methods

	}
}


