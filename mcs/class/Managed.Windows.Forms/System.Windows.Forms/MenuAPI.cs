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
using System.Collections;

namespace System.Windows.Forms
{

	/*
		This class mimics the Win32 API Menu functionality
	*/
	internal class MenuAPI
	{
		static ArrayList menu_list = new ArrayList ();
		static Font MENU_FONT = new Font (FontFamily.GenericSansSerif, 8.25f);
		static int POPUP_ARROW_WITDH;
		static int POPUP_ARROW_HEIGHT;
		const int SEPARATOR_HEIGHT = 5;
		const int SM_CXBORDER = 1;
		const int SM_CYBORDER = 1;
		const int SM_CXMENUCHECK = 14;
    		const int SM_CYMENUCHECK = 14;

		public class MENU
		{
			public MF		Flags;       /* Menu flags (MF_POPUP, MF_SYSMENU) */
			public int		Width;        /* Width of the whole menu */
			public int		Height;       /* Height of the whole menu */
			public Control		Wnd;
			public ArrayList	items = new ArrayList (); /* Array of menu items */
			public int		FocusedItem;  /* Currently focused item */
		}

		public class MENUITEM
		{
			public	MenuItem 	item;
			public  Rectangle	rect = new Rectangle ();			
			public	int	fMask; 
			public	int	fType; 
			public	MF	fState; 
			public 	int    	wID; 
			public	IntPtr	hSubMenu; 
			//HBITMAP hbmpChecked; 
			//HBITMAP hbmpUnchecked; 
			//ULONG_PTR dwItemData; 
			//LPTSTR  dwTypeData; 
			//public int    cch;
			//HBITMAP hbmpItem;
		};

		static void DumpMenuItems (ArrayList list)
		{
			Console.WriteLine ("Menu items dump start--- ");

			for (int i = 0; i < list.Count; i++)
				Console.WriteLine ("idx:{0} {1} {2}", i, ((MENUITEM)list[i]).item, ((MENUITEM)list[i]).item.Separator);

			Console.WriteLine ("Menu items dump end --- ");
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
			MF_RIGHTJUSTIFY     = 0x4000
		}
				
		static MenuAPI ()
		{
			//static POPUP_ARROW_WITDH;
			//static POPUP_ARROW_HEIGHT;
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

		static public bool InsertMenuItem (IntPtr hMenu, int uItem, bool fByPosition, MenuItem item)
		{
			if (fByPosition == false)
				throw new NotImplementedException ();

			// Insert the item

			MENU menu = GetMenuFromID (hMenu);
			if ((uint)uItem > menu.items.Count)
				uItem =  menu.items.Count;

			MENUITEM menu_item = new MENUITEM ();
			menu_item.item = item;
			menu.items.Insert (uItem, menu_item);
			//Console.WriteLine ("InsertMenuItem {0} {1} {2}" + menu.items.Count,
			//);
			return true;
		}

		static public bool TrackPopupMenu (IntPtr hMenu, int uFlags, int x,  int y,  int nReserved, Control hWnd)
		{
			Console.WriteLine ("TrackPopupMenu start");
			MENU menu = GetMenuFromID (hMenu);
			PopUpWindow popup = new PopUpWindow (hMenu);
			menu.Wnd = popup;

			Point pnt;
			pnt = popup.PointToClient (Control.MousePosition);
			popup.Location = pnt;
			popup.ShowWindow ();
			MenuAPI.DumpMenuItems (menu.items);

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

		static public void CalcMenuItemSize (Graphics dc, MENUITEM item, int y)
		{

			item.rect.Y = y;

			if (item.item.Separator == true) {
				item.rect.Height = SEPARATOR_HEIGHT / 2;
				item.rect.Width = -1;
				return;
			}

			SizeF size;
			size =  dc.MeasureString (item.item.Text, MENU_FONT);
			item.rect.Width = (int) size.Width + 4;
			item.rect.Height = (int) size.Height;

			item.rect.Width += SM_CXMENUCHECK * 2;

			//if (item.item.IsPopup)
	    			//item.rect.Width += arrow_bitmap_width;

			Console.WriteLine ("CalcMenuItemSize " + item.rect);
		}


		static public void CalcPopupMenuSize (Graphics dc, IntPtr hMenu)
		{
			int x = 3;
			int start = 0;
			int i, n, y, max;

			MENU menu = GetMenuFromID (hMenu);

			while (start < menu.items.Count) {

				y = 2;
				max = 0;
				for (i = start; i < menu.items.Count; i++) {
					MENUITEM item = (MENUITEM) menu.items[i];

					if ((i != start) && (item.item.Break || item.item.BarBreak))
						break;

					CalcMenuItemSize (dc, item, y);
					y += item.rect.Height;
					item.rect.X = x;

					if (item.rect.Width > max)
						max = item.rect.Width;
				}

				// Reemplace the -1 by the menu width (separators)
				for (n = start; n < i; n++, start++) {
					MENUITEM item = (MENUITEM) menu.items[n];
					item.rect.Width = max; //-4
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

			Console.WriteLine ("CalcPopupMenuSize {0} {1}", menu.Width, menu.Height);
		}

		static public void DrawMenuItem (Graphics dc, MENUITEM item, int menu_height)
		{
			Console.WriteLine ("Draw item {0} {1}", item.item.Text,
				item.item.Separator);

			if (item.item.Separator == true) {
				// TODO: ControlPaint.DrawBorder3D (dc, item.rect,
				//	Border3DStyle.Etched, BF_TOP);

				dc.FillRectangle (new SolidBrush (Color.Black),
					item.rect.X, item.rect.Y, item.rect.Width, item.rect.Height);

				return;
			}

			StringFormat string_format = new StringFormat ();
			string_format.LineAlignment = StringAlignment.Center;
			string_format.Alignment = StringAlignment.Near;
			Rectangle rect_text = item.rect;
			rect_text.X += SM_CXMENUCHECK;

			if (item.item.BarBreak) { /* Draw vertical break bar*/

				Rectangle rect = item.rect;
				rect.Y++;
	        		rect.Width = 3;
	        		rect.Height = menu_height - 6;

				//TODO: ControlPaint.DrawBorder3D (dc, item.rect,
				//	Border3DStyle.Etched, BF_TOP);

				dc.FillRectangle (new SolidBrush (Color.Black),
					rect);
			}

			if ((item.fState & MF.MF_HILITE) == MF.MF_HILITE) {

				dc.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(ThemeEngine.Current.ColorHilight), item.rect);

				dc.DrawString (item.item.Text, MENU_FONT, ThemeEngine.Current.ResPool.GetSolidBrush
					(ThemeEngine.Current.ColorHilightText), rect_text, string_format);
			}
			else
				dc.DrawString (item.item.Text, MENU_FONT, new SolidBrush (Color.Black),
						rect_text, string_format);
		}

		static public void DrawPopupMenu (Graphics dc, IntPtr hMenu)
		{
			MENU menu = GetMenuFromID (hMenu);

			for (int i = 0; i < menu.items.Count; i++) {
				DrawMenuItem (dc, (MENUITEM) menu.items[i], menu.Height);
			}
		}

		static public MENUITEM FindItemByCoords (IntPtr hMenu, Point pt, ref int pos)
		{
			MENU menu = GetMenuFromID (hMenu);

			for (int i = 0; i < menu.items.Count; i++) {
				MENUITEM item = (MENUITEM) menu.items[i];
				if (item.rect.Contains (pt)) {
					Console.WriteLine ("FindItemByCoords: " + item.item.Text);
					pos = i;
					return item;
				}
			}

			Console.WriteLine ("FindItemByCoords none ");
			pos = -1;
			return null;
		}

		static public void SelectItem (IntPtr hMenu, MENUITEM item, int pos)
		{
			MENU menu = GetMenuFromID (hMenu);

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
			Console.WriteLine ("SelectItem {0} {1} {2} {3}", item.item.Text, item.fState,
				((MENUITEM)(menu.items[pos])).fState, pos);

		}
	}

	internal class PopUpWindow : Control
	{
		private IntPtr hMenu;

		public PopUpWindow (IntPtr hMenu): base ()
		{
			this.hMenu = hMenu;
			MouseDown += new MouseEventHandler (OnMouseDownPUW);
			MouseMove += new MouseEventHandler (OnMouseMovePUW);
			Paint += new PaintEventHandler (OnPaintPUW);
			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
		}

		protected override CreateParams CreateParams
		{
			get {
				CreateParams cp = base.CreateParams;									
				cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_VISIBLE));
				return cp;
			}
		}

		public void ShowWindow ()
		{
			Capture = true;
			Show ();
		}

		protected override void OnResize(EventArgs e)
		{
			Console.WriteLine ("OnResize {0} {1} ", Width, Height);
			CreateBuffers (Width, Height);
		}

		private void OnPaintPUW (Object o, PaintEventArgs pevent)
		{
			Console.WriteLine ("OnPaintPUW");

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

		private void OnMouseMovePUW (object sender, MouseEventArgs e)
		{
			Console.WriteLine ("OnMouseMovePUW");
			int pos = 0;
			MenuAPI.MENUITEM item = MenuAPI.FindItemByCoords (hMenu, new Point (e.X, e.Y), ref pos);

			if (item != null) {
				MenuAPI.SelectItem (hMenu, item, pos);
				Refresh ();
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
			Rectangle rect = ClientRectangle;
			rect.Width = rect.Width - 1;
			rect.Height = rect.Height - 1;

			DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
				(ThemeEngine.Current.ColorMenu), ClientRectangle);

			DeviceContext.DrawRectangle (ThemeEngine.Current.ResPool.GetPen
				(ThemeEngine.Current.ColorHilightText), rect);

			MenuAPI.DrawPopupMenu  (DeviceContext, hMenu);
		}
	}


}


