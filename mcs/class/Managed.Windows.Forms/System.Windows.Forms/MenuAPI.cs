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
    		const int MENU_BAR_ITEMS_SPACE = 12;	// Space between menu bar items

		public class MENU
		{
			public MF		Flags;		// Menu flags (MF_POPUP, MF_SYSMENU)
			public int		Width;		// Width of the whole menu
			public int		Height;		// Height of the whole menu
			public Control		Wnd;		// In a Popup menu is the PopupWindow and in a MenuBar the Form
			public ArrayList	items;		// Array of menu items
			public int		FocusedItem;	// Currently focused item
			public IntPtr		hParent;

			public MENUITEM		SelectedItem;	// Currently focused item

			public MENU ()
			{
				Wnd = null;
				hParent = IntPtr.Zero;
				items = new ArrayList ();
				Flags = MF.MF_INSERT;
				Width = Height = FocusedItem = 0;
			}
		}

		public class MENUITEM
		{
			public	MenuItem 	item;
			public  Rectangle	rect;
			public	int		fMask; 
			public	int		fType; 
			public	MF		fState; 
			public 	int    		wID; 
			public	IntPtr		hSubMenu;
			public 	int		xTab;

			public MENUITEM ()
			{
				xTab = 0;
				fMask = 0;
				wID = 0;
				rect = new Rectangle ();
			}
		};

		public class TRACKER
		{
		    	public 	IntPtr	hCurrentMenu;
		    	public	IntPtr	hTopMenu;
		    	public 	bool menubar;

		    	public TRACKER ()
			{
				hCurrentMenu = hTopMenu = IntPtr.Zero;
				menubar = false;
			}

		};

		static void DumpMenuItems (ArrayList list)
		{
			Console.WriteLine ("Menu items dump start--- ");

			for (int i = 0; i < list.Count; i++)
				Console.WriteLine ("idx:{0} {1} {2}", i, ((MENUITEM)list[i]).item, ((MENUITEM)list[i]).item.Separator);

			Console.WriteLine ("Menu items dump end --- ");
		}

		public enum MenuMouseEvent
		{
			Down,
			Move,
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
			Console.WriteLine ("MenuAPI::MenuAPI");
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
			//Console.WriteLine ("StoreMenuID:" + id + 1);
			return (IntPtr)(id + 1);
		}

		static public MENU GetMenuFromID (IntPtr ptr)
		{
			int id = (int)ptr;

			id = id - 1;
			return (MENU) menu_list[id];
		}

		static public IntPtr CreateMenu ()
		{
			MENU menu = new MENU ();
			return StoreMenuID (menu);
		}

		static public IntPtr CreatePopupMenu ()
		{
			Console.WriteLine ("MenuAPI.CreatePopupMenu");

			MENU popMenu = new MENU ();
			popMenu.Flags |= MF.MF_POPUP;
			return StoreMenuID (popMenu);
		}

		static public int InsertMenuItem (IntPtr hMenu, int uItem, bool fByPosition, MenuItem item,
			ref IntPtr hSubMenu)
		{
			int id;

			if (fByPosition == false)
				throw new NotImplementedException ();

			// Insert the item

			MENU menu = GetMenuFromID (hMenu);
			if ((uint)uItem > menu.items.Count)
				uItem =  menu.items.Count;

			MENUITEM menu_item = new MENUITEM ();
			menu_item.item = item;

			if (item.IsPopup) {
				menu_item.hSubMenu = CreatePopupMenu ();
			}
			else
				menu_item.hSubMenu = IntPtr.Zero;

			//menu_item.Flags |= MF.MF_POPUP;

			hSubMenu = menu_item.hSubMenu;

			id = menu.items.Count;
			menu.items.Insert (uItem, menu_item);

			//Console.WriteLine ("InsertMenuItem {0} {1} {2}" + menu.items.Count,
			//);
			return id;
		}

		// X and Y are screen coordinates
		static public bool TrackPopupMenu (IntPtr hTopMenu, IntPtr hMenu, Point pnt,  bool menubar, Control Wnd)
		{
			Console.WriteLine ("TrackPopupMenu start");
			MENU menu = GetMenuFromID (hMenu);
			TRACKER tracker = new TRACKER ();
			PopUpWindow popup = new PopUpWindow (hMenu, tracker);
			menu.Wnd = popup;
			tracker.hCurrentMenu = hMenu;
			tracker.hTopMenu = hTopMenu;
			tracker.menubar = menubar;

			//Console.WriteLine ("TrackPopupMenu:Setting current to {0}", menu.hCurrent);
			//Console.WriteLine ("menubar: path2 {0}" + pnt);

			popup.Location =  popup.PointToClient (pnt);
			popup.ShowWindow ();
			//MenuAPI.DumpMenuItems (menu.items);

			MSG msg = new MSG();

			while (XplatUI.GetMessage(ref msg, IntPtr.Zero, 0, 0))  {
				XplatUI.TranslateMessage(ref msg);
				XplatUI.DispatchMessage(ref msg);
			}

			//popup.DestroyHandle ();
			Console.WriteLine ("TrackPopupMenu end");
			menu.Wnd = null;
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
				//item.rect.Width += MENU_BAR_ITEMS_SPACE;
				x += item.rect.Width;
			}

			if (item.rect.Height < SM_CYMENU - 1)
				item.rect.Height = SM_CYMENU - 1;

			//Console.WriteLine ("CalcItemSize " + item.rect);
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

			//Console.WriteLine ("CalcPopupMenuSize {0} {1}", menu.Width, menu.Height);
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

			//Console.WriteLine ("!{0}, {1}, {2}", item.item.Text, item.rect, rect_text);

			if ((item.fState & MF.MF_HILITE) == MF.MF_HILITE) {
				dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(ThemeEngine.Current.ColorHilight), item.rect);				
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
				dc.DrawImage (bmp, area.X, item.rect.Y + (item.rect.Height /2)); //aki

				gr.Dispose ();
				bmp.Dispose ();
			}

		}

		static public void DrawPopupMenu (Graphics dc, IntPtr hMenu, Rectangle rect)
		{
			MENU menu = GetMenuFromID (hMenu);

			dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
				(ThemeEngine.Current.ColorMenu), rect);

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

			for (int i = 0; i < menu.items.Count; i++) {
				DrawMenuItem (dc, (MENUITEM) menu.items[i], menu.Height, false);
			}
		}

		// Updats the menu rect
		static public void MenuBarCalcSize (Graphics dc, IntPtr hMenu, Rectangle rect)
		{
			int x = 3;
			int i, y;

			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;

			x = rect.X;
			y = rect.Height + 1;
			menu.Width = 0;

			i = 0;
			while (i < menu.items.Count) {

				item = (MENUITEM) menu.items[i];
				CalcItemSize (dc, item, y, x, true);
				i = i + 1;
				x += item.rect.Width;
				item.fState |= MF.MF_MENUBAR;

				if (item.rect.Height > menu.Height)
					menu.Height = item.rect.Height;

				//Console.WriteLine ("MenuBarCalcSize {0} {1}", item.item.Text, item.rect.X);
			}

			menu.Width = x;

			//Console.WriteLine ("CalcPopupMenuSize {0} {1}", menu.Width, menu.Height);
		}

		// Draws a menu bar in a Window
		static public void DrawMenuBar (Graphics dc, IntPtr hMenu, Rectangle rect)
		{
			MENU menu = GetMenuFromID (hMenu);
			Rectangle rect_menu = new Rectangle ();

			if (menu.Height == 0)
				MenuBarCalcSize (dc, hMenu, rect_menu);

			rect.Height = menu.Height;
			rect.Width = menu.Width;

			for (int i = 0; i < menu.items.Count; i++) {
				DrawMenuItem (dc, (MENUITEM) menu.items[i], menu.Height, true);
			}
		}

		/*
			Menu handeling API
		*/
		static public MENUITEM FindItemByCoords (IntPtr hMenu, Point pt, ref int pos)
		{
			MENU menu = GetMenuFromID (hMenu);

			for (int i = 0; i < menu.items.Count; i++) {
				MENUITEM item = (MENUITEM) menu.items[i];
				if (item.rect.Contains (pt)) {
					//Console.WriteLine ("FindItemByCoords: " + item.item.Text);
					pos = i;
					return item;
				}
			}

			//Console.WriteLine ("FindItemByCoords none ");
			pos = -1;
			return null;
		}

		static public void SelectItem (TRACKER tracker, IntPtr hMenu, MENUITEM item, int pos, bool execute)
		{
			MENU menu = GetMenuFromID (hMenu);

			//Console.WriteLine ("Current: {0} select {1}", menu_parent.hCurrent, hMenu);
			MENUITEM highlight_item = null;

			/* Already selected */
			for (int i = 0; i < menu.items.Count; i++) {
				MENUITEM it = (MENUITEM) menu.items[i];

				if ((it.fState & MF.MF_HILITE) == MF.MF_HILITE) {
					if (item.rect == it.rect)
						return;

					highlight_item = item;
				}
			}

			//Console.WriteLine ("SelectItem:Current is {0} {1}", tracker.hCurrentMenu, hMenu);

			if (tracker.hCurrentMenu != hMenu) {
				Console.WriteLine ("Changing current menu!");
				HideSubPopups (hMenu);
				tracker.hCurrentMenu = hMenu;
			}

			/* Unselect previous item*/
			for (int i = 0; i < menu.items.Count; i++) {
				MENUITEM it = (MENUITEM) menu.items[i];

				if ((it.fState & MF.MF_HILITE) == MF.MF_HILITE) {
					it.fState = item.fState & ~MF.MF_HILITE;
					menu.items[i] = it;
				}
			}

			item.fState |= MF.MF_HILITE;
			menu.items[pos] = item;
			//Console.WriteLine ("SelectItem {0} {1} {2} {3}", item.item.Text, item.fState,
			//	((MENUITEM)(menu.items[pos])).fState, pos);

			if (execute)
				ExecFocusedItem (tracker, hMenu, item);
		}

		/*
			Used when the user executes the action of an item (press enter, shortcut)
			or a sub-popup menu has to be shown
		*/
		static public void ExecFocusedItem (TRACKER tracker, IntPtr hMenu, MENUITEM item)
		{
			if (item.item.IsPopup) {
				ShowSubPopup (tracker, hMenu, item.hSubMenu, item);
			}
			else {
				// Execute function
			}
		}

		static public void ShowSubPopup (TRACKER tracker, IntPtr hParent, IntPtr hMenu, MENUITEM item)
		{
			MENU menu = GetMenuFromID (hMenu);

			if (menu.Wnd != null) /* Already showing */
				return;

			if (item.item.Enabled == false)
				return;

			MENU menu_parent = GetMenuFromID (hParent);
			PopUpWindow popup = new PopUpWindow (hMenu, tracker);
			((PopUpWindow)menu_parent.Wnd).LostFocus ();
			menu.Wnd = popup;
			tracker.hCurrentMenu = hMenu;

			Console.WriteLine ("ShowSubPopup:Setting current to {0}", tracker.hCurrentMenu);

			Point pnt = new Point ();
			pnt.X = item.rect.X + item.rect.Width;
			pnt.Y = item.rect.Y + 1;
			Console.WriteLine ("ShowSubPopup prev:" + pnt);
			pnt = menu_parent.Wnd.PointToScreen (pnt);
			popup.Location = pnt;
			popup.ShowWindow ();
			popup.Refresh ();

			Console.WriteLine ("ShowSubPopup location:" + popup.Location);
		}

		/* Hides all the submenus open in a menu */
		static public void HideSubPopups (IntPtr hMenu)
		{
			Console.WriteLine ("HideSubPopups: " + hMenu);

			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;

			for (int i = 0; i < menu.items.Count; i++) {
				item = (MENUITEM) menu.items[i];
				if (item.item.IsPopup) {

					MENU sub_menu = GetMenuFromID (item.hSubMenu);

					if (sub_menu.Wnd != null) {
						Console.WriteLine ("Hiding!");
						HideSubPopups (item.hSubMenu);
						((PopUpWindow)sub_menu.Wnd).Destroy ();
						sub_menu.Wnd = null;
					}
				}
			}
		}

		static public void DestroyMenu (IntPtr hMenu)
		{
			MENU menu = GetMenuFromID (hMenu);
			MENUITEM item;

			for (int i = 0; i < menu.items.Count; i++) {
				item = (MENUITEM) menu.items[i];
				//Console.WriteLine ("Destroy item: "+ item.item.Text + " pop:" + item.item.IsPopup);
				if (item.item.IsPopup) {
					MENU sub_menu = GetMenuFromID (item.hSubMenu);
					if (sub_menu != null && sub_menu.Wnd != null) {
						// TODO: Remove from list
						HideSubPopups (item.hSubMenu);
						DestroyMenu (item.hSubMenu);
					}
				}
			}

			Console.WriteLine ("Menu.Wnd1: " + menu);
			Console.WriteLine ("Menu.Wnd2: " + menu.Wnd);
			// TODO: Remove from list

			// Do not destroy the window of a Menubar
			if (menu.Wnd != null && ((menu.Flags & MF.MF_POPUP) ==  MF.MF_POPUP)) {
				((PopUpWindow)menu.Wnd).Destroy ();
				menu.Wnd = null;
			}
		}

		static public void SetMenuBarWindow (IntPtr hMenu, Control wnd)
		{
			MENU menu = GetMenuFromID (hMenu);
			menu.Wnd = wnd;
		}

		static TRACKER tracker = new TRACKER ();
		//static Control mywnd  = null;

		// Function that process all menubar mouse events
		static public void TrackBarMouseEvent (IntPtr hMenu, Control wnd, MouseEventArgs e, MenuMouseEvent eventype)
		{
			MENU menu = GetMenuFromID (hMenu);
			int pos = 0;
			Point pnt;

			//if (mywnd == null)
			//	mywnd = wnd;

			switch (eventype) {
				case MenuMouseEvent.Down: {

					MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu,
						new Point (e.X, e.Y), ref pos);

					//Console.WriteLine ("menubar: {0} {1}",item.rect.X,
					//	item.rect.Y + item.rect.Height + 1);

					if (item != null) {

						MenuAPI.SelectItem (tracker, hMenu, item, pos, false);
						tracker.hCurrentMenu = hMenu;

						pnt = new Point (item.rect.X, item.rect.Y + item.rect.Height);
						pnt = wnd.PointToScreen (pnt);

						menu.SelectedItem = item;
						wnd.Refresh ();
						MenuAPI.TrackPopupMenu (hMenu, item.hSubMenu, pnt, true, null);
					}
					break;
				}

				case MenuMouseEvent.Move: { //aki

					if (tracker.hCurrentMenu != IntPtr.Zero) {

						Point p;
						p = new Point (e.X, e.Y);
						p = wnd.PointToScreen (p);
						p = menu.Wnd.PointToClient (p);

						MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, p, ref pos);

						if (item != null && menu.SelectedItem != item) {
							//Console.WriteLine ("Changing from MenuMouseEvent.Move {0} {1} {2} {3}",
							//	hMenu, item.item.Text, tracker.hCurrentMenu, hMenu);

							menu.SelectedItem = item;

							pnt = new Point (item.rect.X, item.rect.Y + item.rect.Height);
							pnt = menu.Wnd.PointToScreen (pnt);
							MenuAPI.SelectItem (tracker, hMenu, item, pos, false);

							MenuAPI.DestroyMenu (tracker.hCurrentMenu);
							tracker.hCurrentMenu = hMenu;

							menu.Wnd.Refresh ();
							MenuAPI.TrackPopupMenu (hMenu, item.hSubMenu, pnt, true, null);
						}
					}
					break;
				}

				default:
					break;
			}
		}
	}

	/*

		class PopUpWindow

	*/
	internal class PopUpWindow : Control
	{
		private IntPtr hMenu;
		private MenuAPI.TRACKER tracker;

		public PopUpWindow (IntPtr hMenu, MenuAPI.TRACKER tracker): base ()
		{
			this.hMenu = hMenu;
			this.tracker = tracker;
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
				cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_VISIBLE));
				cp.ExStyle |= (int)WindowStyles.WS_EX_TOOLWINDOW;
				return cp;
			}
		}

		public void ShowWindow ()
		{
			Capture = true;
			Show ();
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
			Console.WriteLine ("OnResize {0} {1} ", Width, Height);
		}

		private void OnPaintPUW (Object o, PaintEventArgs pevent)
		{
			//Console.WriteLine ("OnPaintPUW");

			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			Draw ();
			pevent.Graphics.DrawImage (ImageBuffer, 0, 0);
		}

		private void OnMouseDownPUW (object sender, MouseEventArgs e)
    		{
    			Console.WriteLine ("OnMouseDownPUW");
    			/* Click outside the client area*/
    			if (ClientRectangle.Contains (e.X, e.Y) == false) {
    				Console.WriteLine ("Hide");
    				Capture = false;
    				Hide ();
    			}
		}

		private void OnMouseUpPUW (object sender, MouseEventArgs e)
    		{
    			Console.WriteLine ("OnMouseUpPUW");
    			/* Click outside the client area*/
    			int pos = 0;
			MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, new Point (e.X, e.Y), ref pos);

			if (item != null && item.item.Enabled) {
				item.item.PerformClick ();
				MenuAPI.DestroyMenu (tracker.hTopMenu);

				Capture = false;
				Refresh ();
			}

		}

		private void OnMouseMovePUW (object sender, MouseEventArgs e)
		{
			//Console.WriteLine ("OnMouseMovePUW");
			int pos = 0;
			MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, new Point (e.X, e.Y), ref pos);

			if (item != null) {

				MenuAPI.MENU menu = MenuAPI.GetMenuFromID (hMenu);
				MenuAPI.SelectItem (tracker, hMenu, item, pos, true);
				Refresh ();
			} else {

				if (tracker.menubar) {
					//Console.WriteLine ("MenuBar tracker move " + e.Y);
					//MenuAPI.TrackBarMouseEvent (tracker.hTopMenu,
					//	this, e, MenuAPI.MenuMouseEvent.Move);

					Point pnt = PointToClient (MousePosition);

					MenuAPI.TrackBarMouseEvent (tracker.hTopMenu,
						this, new MouseEventArgs(e.Button, e.Clicks, pnt.X, pnt.Y, e.Delta),
						MenuAPI.MenuMouseEvent.Move); //aku
				}
			}
		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();

			MenuAPI.MENU menu = MenuAPI.GetMenuFromID (hMenu);
			MenuAPI.CalcPopupMenuSize (DeviceContext, hMenu);

			Width = menu.Width;
			Height = menu.Height;
			Console.WriteLine ("CreateHandle {0} {1}", Width, Height);
		}


		private void Draw ()
		{
			MenuAPI.DrawPopupMenu  (DeviceContext, hMenu, ClientRectangle);
		}
	}

}


