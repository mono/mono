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

// NOT COMPLETE

using System.Drawing;
using System.Drawing.Text;
using System.Collections;

namespace System.Windows.Forms
{

	/*
		This class mimics the Win32 API Menu functionality

		When writing this code the Wine project was of great help to
		understand the logic behind some Win32 issues. Thanks to them. Jordi,
	*/
	internal class MenuAPI
	{
		static StringFormat string_format_text = new StringFormat ();
		static StringFormat string_format_shortcut = new StringFormat ();
		static StringFormat string_format_menubar_text = new StringFormat ();
		static ArrayList menu_list = new ArrayList ();
		static Font MENU_FONT = new Font (FontFamily.GenericSansSerif, 8.25f);
		static int POPUP_ARROW_WITDH;
		static int POPUP_ARROW_HEIGHT;
		const int SEPARATOR_HEIGHT = 5;
		const int SM_CXBORDER = 1;
		const int SM_CYBORDER = 1;
		const int SM_CXMENUCHECK = 14;		// Width of the menu check
    		const int SM_CYMENUCHECK = 14;		// Height of the menu check
		const int SM_CXARROWCHECK = 16;		// Width of the arrow
    		const int SM_CYARROWCHECK = 16;		// Height of the arrow
    		const int SM_CYMENU = 18;		// Minimum height of a menu
    		const int MENU_TAB_SPACE = 8;		// Pixels added to the width of an item because of a tab
    		const int MENU_BAR_ITEMS_SPACE = 8;	// Space between menu bar items

		public class MENU
		{
			public MF		Flags;		// Menu flags (MF_POPUP, MF_SYSMENU)
			public int		Width;		// Width of the whole menu
			public int		Height;		// Height of the whole menu
			public Control		Wnd;		// In a Popup menu is the PopupWindow and in a MenuBar the Form
			public ArrayList	items;		// Array of menu items
			public int		FocusedItem;	// Currently focused item
			public IntPtr		hParent;
			public TRACKER		tracker;
			public MENUITEM		SelectedItem;	// Currently focused item
			public bool 		bMenubar;

			public MENU ()
			{
				Wnd = null;
				hParent = IntPtr.Zero;
				items = new ArrayList ();
				Flags = MF.MF_INSERT;
				Width = Height = FocusedItem = 0;
				tracker = new TRACKER ();
				bMenubar = false;
			}

		}

		public class MENUITEM
		{
			public	MenuItem 	item;
			public  Rectangle	rect;
			public	MF		fState;
			public 	int    		wID;
			public	IntPtr		hSubMenu;
			public 	int		xTab;
			public  int 		pos;	/* Position in the menuitems array*/

			public MENUITEM ()
			{
				xTab = 0;
				wID = 0;
				pos = 0;
				rect = new Rectangle ();
			}

		};

		public class TRACKER
		{
		    	public 	IntPtr	hCurrentMenu;
		    	public	IntPtr	hTopMenu;

		    	public TRACKER ()
			{
				hCurrentMenu = hTopMenu = IntPtr.Zero;
			}
		};

		public enum MenuMouseEvent
		{
			Down,
			Move,
		}

		internal enum ItemNavigation
		{
			First,
			Last,
			Next,
			Previous,
		}

		internal enum MF
		{
			MF_INSERT           = 0x0,
			MF_APPEND           = 0x100,
			MF_DELETE           = 0x200,
			MF_REMOVE           = 0x1000,
			MF_BYCOMMAND        = 0,
			MF_BYPOSITION       = 0x400,
			MF_SEPARATOR        = 0x800,
			MF_ENABLED          = 0,
			MF_GRAYED           = 1,
			MF_DISABLED         = 2,
			MF_UNCHECKED        = 0,
			MF_CHECKED          = 8,
			MF_USECHECKBITMAPS  = 0x200,
			MF_STRING           = 0,
			MF_BITMAP           = 4,
			MF_OWNERDRAW        = 0x100,
			MF_POPUP            = 0x10,
			MF_MENUBARBREAK     = 0x20,
			MF_MENUBREAK        = 0x40,
			MF_UNHILITE         = 0,
			MF_HILITE           = 0x80,
			MF_DEFAULT          = 0x1000,
			MF_SYSMENU          = 0x2000,
			MF_HELP             = 0x4000,
			MF_RIGHTJUSTIFY     = 0x4000,
			MF_MENUBAR	    = 0x8000	// Internal
		}

		static MenuAPI ()
		{
			string_format_text.LineAlignment = StringAlignment.Center;
			string_format_text.Alignment = StringAlignment.Near;
			string_format_text.HotkeyPrefix = HotkeyPrefix.Show;

			string_format_shortcut.LineAlignment = StringAlignment.Center;
			string_format_shortcut.Alignment = StringAlignment.Far;

			string_format_menubar_text.LineAlignment = StringAlignment.Center;
			string_format_menubar_text.Alignment = StringAlignment.Center;
			string_format_menubar_text.HotkeyPrefix = HotkeyPrefix.Show;
		}

		static public IntPtr StoreMenuID (MENU menu)
		{
			int id = menu_list.Add (menu);
			return (IntPtr)(id + 1);
		}

		static public MENU GetMenuFromID (IntPtr ptr)
		{
			int id = (int)ptr;
			id = id - 1;

			if (menu_list[id] == null) 	// It has been delete it
				return null;

			return (MENU) menu_list[id];
		}

		static public IntPtr CreateMenu ()
		{
			MENU menu = new MENU ();
			return StoreMenuID (menu);
		}

		static public IntPtr CreatePopupMenu ()
		{
			MENU popMenu = new MENU ();
			popMenu.Flags |= MF.MF_POPUP;
			return StoreMenuID (popMenu);
		}

		static public int InsertMenuItem (IntPtr hMenu, int uItem, bool fByPosition, MenuItem item, ref IntPtr hSubMenu)
		{
			int id;

			if (fByPosition == false)
				throw new NotImplementedException ();

			MENU menu = GetMenuFromID (hMenu);
			if ((uint)uItem > menu.items.Count)
				uItem =  menu.items.Count;

			MENUITEM menu_item = new MENUITEM ();
			menu_item.item = item;

			if (item.IsPopup) {
				menu_item.hSubMenu = CreatePopupMenu ();
				MENU submenu = GetMenuFromID (menu_item.hSubMenu);
				submenu.hParent = hMenu;
			}
			else
				menu_item.hSubMenu = IntPtr.Zero;

			hSubMenu = menu_item.hSubMenu;
			id = menu.items.Count;
			menu_item.pos = menu.items.Count;
			menu.items.Insert (uItem, menu_item);

			return id;
		}

		// The Point object contains screen coordinates
		static public bool TrackPopupMenu (IntPtr hTopMenu, IntPtr hMenu, Point pnt, bool bMenubar, Control Wnd)
		{

			MENU menu = GetMenuFromID (hMenu);
			menu.Wnd = new PopUpWindow (hMenu);
			menu.tracker.hCurrentMenu = hMenu;
			menu.tracker.hTopMenu = hTopMenu;

			MENUITEM select_item = GetNextItem (hMenu, ItemNavigation.First);

			if (select_item != null)
				MenuAPI.SelectItem (hMenu, select_item, false);

			menu.Wnd.Location =  menu.Wnd.PointToClient (pnt);
			((PopUpWindow)menu.Wnd).ShowWindow ();

			Application.Run ();

			if (menu.Wnd == null) {
				menu.Wnd.Dispose ();
				menu.Wnd = null;
			}

			return true;
		}

		/*
			Menu drawing API
		*/

		static public void CalcItemSize (Graphics dc, MENUITEM item, int y, int x, bool menuBar)
		{
			item.rect.Y = y;
			item.rect.X = x;

			if (item.item.Visible == false)
				return;

			if (item.item.Separator == true) {
				item.rect.Height = SEPARATOR_HEIGHT / 2;
				item.rect.Width = -1;
				return;
			}

			SizeF size;
			size =  dc.MeasureString (item.item.Text, MENU_FONT);
			item.rect.Width = (int) size.Width;
			item.rect.Height = (int) size.Height;

			if (!menuBar) {

				if (item.item.Shortcut != Shortcut.None && item.item.ShowShortcut) {
					item.xTab = SM_CXMENUCHECK + MENU_TAB_SPACE + (int) size.Width;
					size =  dc.MeasureString (" " + item.item.GetShortCutText (), MENU_FONT);
					item.rect.Width += MENU_TAB_SPACE + (int) size.Width;
				}

				item.rect.Width += 4 + (SM_CXMENUCHECK * 2);
			}
			else {
				item.rect.Width += MENU_BAR_ITEMS_SPACE;
				x += item.rect.Width;
			}

			if (item.rect.Height < SM_CYMENU - 1)
				item.rect.Height = SM_CYMENU - 1;
		}

		static public void CalcPopupMenuSize (Graphics dc, IntPtr hMenu)
		{
			int x = 3;
			int start = 0;
			int i, n, y, max;

			MENU menu = GetMenuFromID (hMenu);
			menu.Height = 0;

			while (start < menu.items.Count) {
				y = 2;
				max = 0;
				for (i = start; i < menu.items.Count; i++) {
					MENUITEM item = (MENUITEM) menu.items[i];

					if ((i != start) && (item.item.Break || item.item.BarBreak))
						break;

					CalcItemSize (dc, item, y, x, false);
					y += item.rect.Height;

					if (item.rect.Width > max)
						max = item.rect.Width;
				}

				// Reemplace the -1 by the menu width (separators)
				for (n = start; n < i; n++, start++) {
					MENUITEM item = (MENUITEM) menu.items[n];
					item.rect.Width = max;
				}

				if (y > menu.Height)
					menu.Height = y;

				x+= max;
			}

			menu.Width = x;

			//space for border
			menu.Width += 2;
			menu.Height += 2;

			menu.Width += SM_CXBORDER;
    			menu.Height += SM_CYBORDER;
		}

		static public void DrawMenuItem (Graphics dc, MENUITEM item, int menu_height, bool menuBar)
		{
			StringFormat string_format;

			if (item.item.Visible == false)
				return;

			if (menuBar)
				string_format = string_format_menubar_text;
			else
				string_format = string_format_text;

			if (item.item.Separator == true) {

				dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow),
					item.rect.X, item.rect.Y, item.rect.X + item.rect.Width, item.rect.Y);

				dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonHilight),
					item.rect.X, item.rect.Y + 1, item.rect.X + item.rect.Width, item.rect.Y + 1);

				return;
			}

			Rectangle rect_text = item.rect;

			if (!menuBar)
				rect_text.X += SM_CXMENUCHECK;

			if (item.item.BarBreak) { /* Draw vertical break bar*/

				Rectangle rect = item.rect;
				rect.Y++;
	        		rect.Width = 3;
	        		rect.Height = menu_height - 6;

				dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow),
					rect.X, rect.Y , rect.X, rect.Y + rect.Height);

				dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonHilight),
					rect.X + 1, rect.Y , rect.X +1, rect.Y + rect.Height);

			}

			if ((item.fState & MF.MF_HILITE) == MF.MF_HILITE) {
				Rectangle rect = item.rect;
				rect.X++;
				rect.Width -=2;

				dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(ThemeEngine.Current.ColorHilight), rect);
			}

			if (item.item.Enabled) {

				Color color_text;

				if ((item.fState & MF.MF_HILITE) == MF.MF_HILITE)
					color_text = ThemeEngine.Current.ColorHilightText;
				else
					color_text = ThemeEngine.Current.ColorMenuText;


				dc.DrawString (item.item.Text, MENU_FONT,
					ThemeEngine.Current.ResPool.GetSolidBrush (color_text),
					rect_text, string_format);

				if (!menuBar && item.item.Shortcut != Shortcut.None && item.item.ShowShortcut) {

					string str = item.item.GetShortCutText ();
					Rectangle rect = rect_text;
					rect.X = item.xTab;
					rect.Width -= item.xTab;

					dc.DrawString (str, MENU_FONT, ThemeEngine.Current.ResPool.GetSolidBrush (color_text),
						rect, string_format_shortcut);

				}
			}
			else {
				ControlPaint.DrawStringDisabled (dc,
					item.item.Text, MENU_FONT, Color.Black, rect_text,
					string_format);
			}

			/* Draw arrow */
			if (menuBar == false && item.item.IsPopup) {

				Bitmap	bmp = new Bitmap (SM_CXARROWCHECK, SM_CYARROWCHECK);
				Graphics gr = Graphics.FromImage (bmp);
				Rectangle rect_arrow = new Rectangle (0, 0, SM_CXARROWCHECK, SM_CYARROWCHECK);
				ControlPaint.DrawMenuGlyph (gr, rect_arrow, MenuGlyph.Arrow);
				bmp.MakeTransparent ();
				dc.DrawImage (bmp, item.rect.X + item.rect.Width - SM_CXARROWCHECK,
					item.rect.Y + ((item.rect.Height - SM_CYARROWCHECK) /2));

				gr.Dispose ();
				bmp.Dispose ();
			}

			/* Draw checked or radio */
			if (menuBar == false && item.item.Checked) {

				Rectangle area = item.rect;
				Bitmap	bmp = new Bitmap (SM_CXMENUCHECK, SM_CYMENUCHECK);
				Graphics gr = Graphics.FromImage (bmp);
				Rectangle rect_arrow = new Rectangle (0, 0, SM_CXMENUCHECK, SM_CYMENUCHECK);

				if (item.item.RadioCheck)
					ControlPaint.DrawMenuGlyph (gr, rect_arrow, MenuGlyph.Bullet);
				else
					ControlPaint.DrawMenuGlyph (gr, rect_arrow, MenuGlyph.Checkmark);

				bmp.MakeTransparent ();
				dc.DrawImage (bmp, area.X, item.rect.Y + ((item.rect.Height - SM_CYMENUCHECK) / 2));

				gr.Dispose ();
				bmp.Dispose ();
			}

		}

		static public void DrawPopupMenu (Graphics dc, IntPtr hMenu, Rectangle cliparea, Rectangle rect)
		{
			MENU menu = GetMenuFromID (hMenu);

			dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
				(ThemeEngine.Current.ColorMenu), cliparea);

			/* Draw menu borders */
			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorHilightText),
				rect.X, rect.Y, rect.X + rect.Width, rect.Y);

			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorHilightText),
				rect.X, rect.Y, rect.X, rect.Y + rect.Height);

			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow),
				rect.X + rect.Width - 1 , rect.Y , rect.X + rect.Width - 1, rect.Y + rect.Height);

			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonDkShadow),
				rect.X + rect.Width, rect.Y , rect.X + rect.Width, rect.Y + rect.Height);

			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow),
				rect.X , rect.Y + rect.Height - 1 , rect.X + rect.Width - 1, rect.Y + rect.Height -1);

			dc.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonDkShadow),
				rect.X , rect.Y + rect.Height, rect.X + rect.Width - 1, rect.Y + rect.Height);

			for (int i = 0; i < menu.items.Count; i++)
				if (cliparea.IntersectsWith (((MENUITEM) menu.items[i]).rect)) {
					DrawMenuItem (dc, (MENUITEM) menu.items[i], menu.Height, menu.bMenubar);

			}
		}

		// Updates the menu rect and returns the height
		static public int MenuBarCalcSize (Graphics dc, IntPtr hMenu, int width)
		{
			int x = 0;
			int i = 0;
			int y = 0;
			MENU menu = GetMenuFromID (hMenu);
			menu.Height = 0;
			MENUITEM item;

			while (i < menu.items.Count) {

				item = (MENUITEM) menu.items[i];
				CalcItemSize (dc, item, y, x, true);
				i = i + 1;

				if (x + item.rect.Width > width) {
					item.rect.X = 0;
					y += item.rect.Height;
					item.rect.Y = y;
					x = 0;
				}

				x += item.rect.Width;
				item.fState |= MF.MF_MENUBAR;

				if (y + item.rect.Height > menu.Height)
					menu.Height = item.rect.Height + y;
			}

			menu.Width = width;
			return menu.Height;
		}

		// Draws a menu bar in a Window
		static public void DrawMenuBar (Graphics dc, IntPtr hMenu, Rectangle rect)
		{
			MENU menu = GetMenuFromID (hMenu);
			Rectangle rect_menu = new Rectangle ();

			if (menu.Height == 0)
				MenuBarCalcSize (dc, hMenu, rect_menu.Width);

			rect.Height = menu.Height;
			rect.Width = menu.Width;

			for (int i = 0; i < menu.items.Count; i++)
				DrawMenuItem (dc, (MENUITEM) menu.items[i], menu.Height, true);
		}

		/*
			Menu handeling API
		*/

		static public MENUITEM FindItemByCoords (IntPtr hMenu, Point pt)
		{
			MENU menu = GetMenuFromID (hMenu);

			for (int i = 0; i < menu.items.Count; i++) {
				MENUITEM item = (MENUITEM) menu.items[i];
				if (item.rect.Contains (pt)) {
					return item;
				}
			}

			return null;
		}

		// Select the item and unselect the previous selecte item
		static public void SelectItem (IntPtr hMenu, MENUITEM item, bool execute)
		{
			MENU menu = GetMenuFromID (hMenu);
			MENUITEM previous_selitem = null;

			/* Already selected */
			for (int i = 0; i < menu.items.Count; i++) {
				MENUITEM it = (MENUITEM) menu.items[i];

				if ((it.fState & MF.MF_HILITE) == MF.MF_HILITE) {
					if (item.rect == it.rect)
						return;

					/* Unselect previous item*/
					previous_selitem = it;
					it.fState = it.fState & ~MF.MF_HILITE;
					menu.Wnd.Invalidate (previous_selitem.rect);
					break;
				}
			}

			// If the previous item had subitems, hide them
			if (previous_selitem != null && previous_selitem.item.IsPopup)
				HideSubPopups (hMenu);

			if (menu.tracker.hCurrentMenu != hMenu)
				menu.tracker.hCurrentMenu = hMenu;

			menu.SelectedItem = item;
			item.fState |= MF.MF_HILITE;
			menu.Wnd.Invalidate (item.rect);

			item.item.PerformSelect ();

			if (execute)
				ExecFocusedItem (hMenu, item);
		}


		//	Used when the user executes the action of an item (press enter, shortcut)
		//	or a sub-popup menu has to be shown
		static public void ExecFocusedItem (IntPtr hMenu, MENUITEM item)
		{
			if (item.item.IsPopup) {
				ShowSubPopup (hMenu, item.hSubMenu, item);
			}
			else {
				// Execute function
			}
		}

		// Create a popup window and show it or only show it if it is already created
		static public void ShowSubPopup (IntPtr hParent, IntPtr hMenu, MENUITEM item)
		{
			MENU menu = GetMenuFromID (hMenu);

			if (item.item.Enabled == false)
				return;

			MENU menu_parent = GetMenuFromID (hParent);
			((PopUpWindow)menu_parent.Wnd).LostFocus ();
			menu.tracker.hCurrentMenu = hMenu;

			if (menu.Wnd == null) {
				Point pnt = new Point ();

				menu.Wnd = new PopUpWindow (hMenu);
				pnt.X = item.rect.X + item.rect.Width;
				pnt.Y = item.rect.Y + 1;
				pnt = menu_parent.Wnd.PointToScreen (pnt);
				menu.Wnd.Location = pnt;
			}

			MENUITEM select_item = GetNextItem (hMenu, ItemNavigation.First);

			if (select_item != null)
				MenuAPI.SelectItem (hMenu, select_item, false);

			((PopUpWindow)menu.Wnd).ShowWindow ();
			menu.Wnd.Refresh ();

		}

		/* Hides all the submenus open in a menu */
		static public void HideSubPopups (IntPtr hMenu)
		{
			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;

			for (int i = 0; i < menu.items.Count; i++) {
				item = (MENUITEM) menu.items[i];
				if (!item.item.IsPopup)
					continue;

				MENU sub_menu = GetMenuFromID (item.hSubMenu);

				if (sub_menu.Wnd != null) {
					HideSubPopups (item.hSubMenu);
					((PopUpWindow)sub_menu.Wnd).Hide ();
				}
			}
		}

		static public void DestroyMenu (IntPtr hMenu)
		{
			if (hMenu == IntPtr.Zero)
				return;

			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;

			for (int i = 0; i < menu.items.Count; i++) {
				item = (MENUITEM) menu.items[i];
				if (item.item.IsPopup) {
					MENU sub_menu = GetMenuFromID (item.hSubMenu);
					if (sub_menu != null && sub_menu.Wnd != null) {
						HideSubPopups (item.hSubMenu);
						DestroyMenu (item.hSubMenu);
					}
				}
			}

			// Do not destroy the window of a Menubar
			if (menu.Wnd != null && menu.bMenubar == false) {
				((PopUpWindow)menu.Wnd).Dispose ();
				menu.Wnd = null;

				/* Unreference from the array list */
				menu_list[((int)hMenu)-1] = null;
			}
		}

		static public void SetMenuBarWindow (IntPtr hMenu, Control wnd)
		{
			MENU menu = GetMenuFromID (hMenu);
			menu.Wnd = wnd;
			menu.bMenubar = true;
		}

		static private void MenuBarMove (IntPtr hMenu, MENUITEM item)
		{
			MENU menu = GetMenuFromID (hMenu);
			Point pnt = new Point (item.rect.X, item.rect.Y + item.rect.Height);
			pnt = menu.Wnd.PointToScreen (pnt);

			MenuAPI.SelectItem (hMenu, item, false);
			HideSubPopups (menu.tracker.hCurrentMenu);
			menu.tracker.hCurrentMenu = hMenu;
			MenuAPI.TrackPopupMenu (hMenu, item.hSubMenu, pnt, false, null);
		}

		// Function that process all menubar mouse events
		static public void TrackBarMouseEvent (IntPtr hMenu, Control wnd, MouseEventArgs e, MenuMouseEvent eventype)
		{
			MENU menu = GetMenuFromID (hMenu);

			switch (eventype) {
				case MenuMouseEvent.Down: {

					MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, new Point (e.X, e.Y));

					if (item != null && menu.SelectedItem != item)
						MenuBarMove (hMenu, item);

					break;
				}

				case MenuMouseEvent.Move: { /* Coordinates in screen position*/

					if (menu.tracker.hCurrentMenu != IntPtr.Zero) {

						Point pnt = new Point (e.X, e.Y);
						pnt = menu.Wnd.PointToClient (pnt);

						MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, pnt);

						if (item != null && menu.SelectedItem != item)
							MenuBarMove (hMenu, item);
					}
					break;
				}

				default:
					break;
			}
		}

		static public MENUITEM FindItemByKey (IntPtr hMenu, IntPtr key)
		{
			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;

			char key_char = (char ) (key.ToInt32() & 0xff);
			key_char = Char.ToUpper (key_char);

			for (int i = 0; i < menu.items.Count; i++) {
				item = (MENUITEM) menu.items[i];

				if (item.item.Mnemonic == '\0')
					continue;

				if (item.item.Mnemonic == key_char)
					return item;
			}

			return null;
		}

		// Get the next or previous selectable item on a menu
		static public MENUITEM GetNextItem (IntPtr hMenu, ItemNavigation navigation)
		{
			MENU menu = GetMenuFromID (hMenu);
			int pos = 0;
			bool selectable_items = false;
			MENUITEM item;

			// Check if there is at least a selectable item
			for (int i = 0; i < menu.items.Count; i++) {
				item = (MENUITEM)menu.items[i];
				if (item.item.Separator == false && item.item.Visible == true) {
					selectable_items = true;
					break;
				}
			}

			if (selectable_items == false)
				return null;

			switch (navigation) {
			case ItemNavigation.First: {
				pos = 0;

				/* Next item that is not separator and it is visible*/
				for (; pos < menu.items.Count; pos++) {
					item = (MENUITEM)menu.items[pos];
					if (item.item.Separator == false && item.item.Visible == true)
						break;
				}

				if (pos >= menu.items.Count) { /* Jump at the start of the menu */
					pos = 0;
					/* Next item that is not separator and it is visible*/
					for (; pos < menu.items.Count; pos++) {
						item = (MENUITEM)menu.items[pos];
						if (item.item.Separator == false && item.item.Visible == true)
							break;
					}
				}

				break;
			}

			case ItemNavigation.Last: { // Not used
				break;
			}

			case ItemNavigation.Next: {

				if (menu.SelectedItem != null)
					pos = menu.SelectedItem.pos;

				/* Next item that is not separator and it is visible*/
				for (pos++; pos < menu.items.Count; pos++) {
					item = (MENUITEM)menu.items[pos];
					if (item.item.Separator == false && item.item.Visible == true)
						break;
				}

				if (pos >= menu.items.Count) { /* Jump at the start of the menu */
					pos = 0;
					/* Next item that is not separator and it is visible*/
					for (; pos < menu.items.Count; pos++) {
						item = (MENUITEM)menu.items[pos];
						if (item.item.Separator == false && item.item.Visible == true)
							break;
					}
				}
				break;
			}

			case ItemNavigation.Previous: {

				if (menu.SelectedItem != null)
					pos = menu.SelectedItem.pos;

				/* Previous item that is not separator and it is visible*/
				for (pos--; pos >= 0; pos--) {
					item = (MENUITEM)menu.items[pos];
					if (item.item.Separator == false && item.item.Visible == true)
						break;
				}

				if (pos < 0 ) { /* Jump at the end of the menu*/
					pos = menu.items.Count - 1;
					/* Previous item that is not separator and it is visible*/
					for (; pos >= 0; pos--) {
						item = (MENUITEM)menu.items[pos];
						if (item.item.Separator == false && item.item.Visible == true)
							break;
					}
				}

				break;
			}

			default:
				break;
			}

			return (MENUITEM)menu.items[pos];
		}

		static public bool ProcessKeys (IntPtr hMenu, ref Message msg, Keys keyData)
		{
			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;

			switch (keyData) {
				case Keys.Up: {
					item = GetNextItem (hMenu, ItemNavigation.Previous);
					if (item != null)
						MenuAPI.SelectItem (hMenu, item, false);

					break;
				}

				case Keys.Down: {
					item = GetNextItem (hMenu, ItemNavigation.Next);

					if (item != null)
						MenuAPI.SelectItem (hMenu, item, false);
					break;
				}

				/* Menubar selects and opens next. Popups next or open*/
				case Keys.Right: {

					// Try to Expand popup first
					if (menu.SelectedItem.item.IsPopup) {
						ShowSubPopup (hMenu, menu.SelectedItem.hSubMenu, menu.SelectedItem);
					} else {

						MENU parent = null;
						if (menu.hParent != IntPtr.Zero)
							parent = GetMenuFromID (menu.hParent);

						if (parent != null && parent.bMenubar == true) {
							MENUITEM select_item = GetNextItem (menu.hParent, ItemNavigation.Next);
							MenuBarMove (menu.hParent, select_item);
						}
					}

					break;
				}

				case Keys.Left: {

					// Try to Collapse popup first
					if (menu.SelectedItem.item.IsPopup) {

					} else {

						MENU parent = null;
						if (menu.hParent != IntPtr.Zero)
							parent = GetMenuFromID (menu.hParent);

						if (parent != null && parent.bMenubar == true) {
							MENUITEM select_item = GetNextItem (menu.hParent, ItemNavigation.Previous);
							MenuBarMove (menu.hParent, select_item);
						}
					}

					break;
				}

				default:
					break;
			}

			/* Try if it is a menu hot key */
			item = MenuAPI.FindItemByKey (hMenu, msg.WParam);


			if (item != null) {
				MenuAPI.SelectItem (hMenu, item, false);
				return true;
			}

			return false;
		}
	}

	/*

		class PopUpWindow

	*/
	internal class PopUpWindow : Control
	{
		private IntPtr hMenu;

		public PopUpWindow (IntPtr hMenu): base ()
		{
			this.hMenu = hMenu;
			MouseDown += new MouseEventHandler (OnMouseDownPUW);
			MouseMove += new MouseEventHandler (OnMouseMovePUW);
			MouseUp += new MouseEventHandler (OnMouseUpPUW);
			Paint += new PaintEventHandler (OnPaintPUW);
			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
		}

		protected override CreateParams CreateParams
		{
			get {
				CreateParams cp = base.CreateParams;
				cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_VISIBLE | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN));
				cp.ExStyle |= (int)WindowStyles.WS_EX_TOOLWINDOW;
				return cp;
			}
		}

		public void ShowWindow ()
		{
			Show ();
			Refresh ();
		}

		public void Destroy ()
		{
			Capture = false;
			DestroyHandle ();
		}

		public void LostFocus ()
		{
			Capture = false;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize (e);
		}

		private void OnPaintPUW (Object o, PaintEventArgs pevent)
		{
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			Draw (pevent.ClipRectangle);
			pevent.Graphics.DrawImage (ImageBuffer, pevent.ClipRectangle, pevent.ClipRectangle, GraphicsUnit.Pixel);
		}

		private void OnMouseDownPUW (object sender, MouseEventArgs e)
    		{
    			/* Click outside the client area*/
    			if (ClientRectangle.Contains (e.X, e.Y) == false) {
    				Capture = false;
    				Hide ();
    			}
		}

		private void OnMouseUpPUW (object sender, MouseEventArgs e)
    		{
    			/* Click outside the client area*/
			MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, new Point (e.X, e.Y));
			MenuAPI.MENU menu = MenuAPI.GetMenuFromID (hMenu);

			if (item != null && item.item.Enabled) {
				item.item.PerformClick ();
				MenuAPI.DestroyMenu (menu.tracker.hTopMenu);

				Capture = false;
				Refresh ();
			}

		}

		private void OnMouseMovePUW (object sender, MouseEventArgs e)
		{
			MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, new Point (e.X, e.Y));
			MenuAPI.MENU menu = MenuAPI.GetMenuFromID (hMenu);

			if (item != null) {
				MenuAPI.SelectItem (hMenu, item, true);
			} else {

				MenuAPI.MENU menu_parent = null;

				if (menu.tracker.hTopMenu != IntPtr.Zero)
					menu_parent = MenuAPI.GetMenuFromID (menu.tracker.hTopMenu);

				if (menu_parent!=null && menu_parent.bMenubar) {

					MenuAPI.TrackBarMouseEvent (menu.tracker.hTopMenu,
						this, new MouseEventArgs(e.Button, e.Clicks, MousePosition.X, MousePosition.Y, e.Delta),
						MenuAPI.MenuMouseEvent.Move);
				}
			}
		}

		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
		{
			return MenuAPI.ProcessKeys (hMenu, ref msg, keyData);
		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();

			MenuAPI.MENU menu = MenuAPI.GetMenuFromID (hMenu);
			MenuAPI.CalcPopupMenuSize (DeviceContext, hMenu);

			Width = menu.Width;
			Height = menu.Height;
		}


		private void Draw (Rectangle clip)
		{
			MenuAPI.DrawPopupMenu  (DeviceContext, hMenu, clip, ClientRectangle);
		}
	}

}


