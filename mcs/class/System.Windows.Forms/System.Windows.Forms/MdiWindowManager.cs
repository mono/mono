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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
//


using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	internal class MdiWindowManager : InternalWindowManager {

		private MainMenu merged_menu;
		private MainMenu maximized_menu;
		private MenuItem icon_menu;
		private ContextMenu icon_popup_menu;
		internal bool was_minimized;
		
		private PaintEventHandler draw_maximized_buttons;
		internal EventHandler form_closed_handler;
		
		private MdiClient mdi_container;
		private Rectangle prev_virtual_position;

		private Point icon_clicked;
		private DateTime icon_clicked_time;
		private bool icon_dont_show_popup;

		private TitleButtons maximized_title_buttons;
		private bool is_visible_pending;
		private byte last_activation_event; // 0 = none, 1 = activated, 2 = deactivated.

		public void RaiseActivated ()
		{
			if (last_activation_event == 1)
				return;
			
			last_activation_event = 1;
			form.OnActivatedInternal ();
			form.SelectActiveControl ();
		}
		
		public void RaiseDeactivate ()
		{
			if (last_activation_event != 1)
				return;
			last_activation_event = 2;
			form.OnDeactivateInternal ();
		}

		public override int MenuHeight {
			get {
				// Mdi children don't get menus on the form, they're shown on the main form.
				return 0;
			}
		}

		internal bool IsVisiblePending {
			get {
				return is_visible_pending;
			}
			set {
				is_visible_pending = value;
			}
		}

		private TitleButtons MaximizedTitleButtons {
			get {
				if (maximized_title_buttons == null) {
					maximized_title_buttons = new TitleButtons (this.Form);
					maximized_title_buttons.CloseButton.Visible = true;
					maximized_title_buttons.RestoreButton.Visible = true;
					maximized_title_buttons.MinimizeButton.Visible = true;
				}
				return maximized_title_buttons;
			}
		}
		
		internal override Rectangle MaximizedBounds {
			get {
				Rectangle pb = mdi_container.ClientRectangle;
				int bw = ThemeEngine.Current.ManagedWindowBorderWidth (this);
				int tw = TitleBarHeight;

				Rectangle new_bounds = new Rectangle (pb.Left - bw,
						pb.Top - tw - bw,
						pb.Width + bw * 2,
						pb.Height + tw + bw * 2);
				return new_bounds;
			}
		}
		
		
		
		public MdiWindowManager (Form form, MdiClient mdi_container) : base (form)
		{
			this.mdi_container = mdi_container;
			if (form.WindowState == FormWindowState.Normal) {
				NormalBounds = form.Bounds;
			}
			form_closed_handler = new EventHandler (FormClosed);
			form.Closed += form_closed_handler;
			form.TextChanged += new EventHandler (FormTextChangedHandler);
			form.SizeChanged += new EventHandler (FormSizeChangedHandler);
			form.LocationChanged += new EventHandler (FormLocationChangedHandler);
			form.VisibleChanged += new EventHandler (FormVisibleChangedHandler);
			draw_maximized_buttons = new PaintEventHandler (DrawMaximizedButtons);
			CreateIconMenus ();
		}

		private void FormVisibleChangedHandler (object sender, EventArgs e)
		{
			if (mdi_container == null)
				return;
				
			if (form.Visible) {
				mdi_container.ActivateChild (form);
			} else if (mdi_container.Controls.Count > 1) {
				mdi_container.ActivateActiveMdiChild ();
			}
		}

		private void FormTextChangedHandler (object sender, EventArgs e)
		{
			mdi_container.SetParentText (false);

			if (form.MdiParent.MainMenuStrip != null)
				form.MdiParent.MainMenuStrip.RefreshMdiItems ();
		}

		private void FormLocationChangedHandler (object sender, EventArgs e)
		{
			if (form.window_state == FormWindowState.Minimized)
				IconicBounds = form.Bounds;
			form.MdiParent.MdiContainer.SizeScrollBars ();
		}

		private void FormSizeChangedHandler (object sender, EventArgs e)
		{
			if (form.window_state == FormWindowState.Maximized && form.Bounds != MaximizedBounds)
				form.Bounds = MaximizedBounds;
			
			form.MdiParent.MdiContainer.SizeScrollBars ();
		}
	
		public MainMenu MergedMenu {
			get {
				if (merged_menu == null)
					merged_menu = CreateMergedMenu ();
				return merged_menu;
			}
		}

		private MainMenu CreateMergedMenu ()
		{
			Form parent = (Form) mdi_container.Parent;
			MainMenu clone;
			if (parent.Menu != null)
				clone = (MainMenu) parent.Menu.CloneMenu ();
			else
				clone = new MainMenu ();
				
			if (form.WindowState == FormWindowState.Maximized) {
				
			}
			clone.MergeMenu (form.Menu);
			clone.MenuChanged += new EventHandler (MenuChangedHandler);
			clone.SetForm (parent);
			return clone;
		}

		public MainMenu MaximizedMenu {
			get {
				if (maximized_menu == null)
					maximized_menu = CreateMaximizedMenu ();
				return maximized_menu;
			}
		}

		private MainMenu CreateMaximizedMenu ()
		{
			Form parent = (Form) mdi_container.Parent;

			if (form.MainMenuStrip != null || parent.MainMenuStrip != null)
				return null;

			MainMenu res = new MainMenu ();

			if (parent.Menu != null) {
				MainMenu clone = (MainMenu) parent.Menu.CloneMenu ();
				res.MergeMenu (clone);
			}
			
			if (form.Menu != null) {
				MainMenu clone = (MainMenu) form.Menu.CloneMenu ();
				res.MergeMenu (clone);
			}
			
			if (res.MenuItems.Count == 0)
				res.MenuItems.Add (new MenuItem ()); // Dummy item to get the menu height correct
			
			res.MenuItems.Insert (0, icon_menu);
			
			res.SetForm (parent);
			return res;
		}

		private void CreateIconMenus ()
		{
			icon_menu = new MenuItem ();
			icon_popup_menu = new ContextMenu ();

			icon_menu.OwnerDraw = true;
			icon_menu.MeasureItem += new MeasureItemEventHandler (MeasureIconMenuItem);
			icon_menu.DrawItem += new DrawItemEventHandler (DrawIconMenuItem);
			icon_menu.Click += new EventHandler (ClickIconMenuItem);

			MenuItem restore = new MenuItem ("&Restore", new EventHandler (RestoreItemHandler));
			MenuItem move = new MenuItem ("&Move", new EventHandler (MoveItemHandler));
			MenuItem size = new MenuItem ("&Size", new EventHandler (SizeItemHandler));
			MenuItem minimize = new MenuItem ("Mi&nimize", new EventHandler (MinimizeItemHandler));
			MenuItem maximize = new MenuItem ("Ma&ximize", new EventHandler (MaximizeItemHandler));
			MenuItem close = new MenuItem ("&Close", new EventHandler (CloseItemHandler));
			MenuItem next = new MenuItem ("Nex&t", new EventHandler (NextItemHandler));

			icon_menu.MenuItems.AddRange (new MenuItem [] { restore, move, size, minimize,
									maximize, close, next });
			icon_popup_menu.MenuItems.AddRange (new MenuItem [] { restore, move, size, minimize,
									maximize, close, next });
		}

		private void ClickIconMenuItem(object sender, EventArgs e)
		{
			if ((DateTime.Now - icon_clicked_time).TotalMilliseconds <= SystemInformation.DoubleClickTime) {
				form.Close ();
				return;
			}
			icon_clicked_time = DateTime.Now;
			Point pnt = Point.Empty;
			pnt = form.MdiParent.PointToScreen (pnt);
			pnt = form.PointToClient (pnt);
			ShowPopup (pnt);
		}
		
		internal void ShowPopup (Point pnt)
		{
			// If we are using MainMenuStrip, display that menu instead
			if (form.WindowState == FormWindowState.Maximized && form.MdiParent.MainMenuStrip != null)
				if (form.MdiParent.MainMenuStrip.Items.Count > 0) {
					ToolStripItem tsi = form.MdiParent.MainMenuStrip.Items[0];
					
					if (tsi is MdiControlStrip.SystemMenuItem) {
						(tsi as MdiControlStrip.SystemMenuItem).ShowDropDown ();
						return;
					}
				}
				
			icon_popup_menu.MenuItems[0].Enabled = form.window_state != FormWindowState.Normal;    // restore
			icon_popup_menu.MenuItems[1].Enabled = form.window_state != FormWindowState.Maximized; // move
			icon_popup_menu.MenuItems[2].Enabled = form.window_state != FormWindowState.Maximized; // size
			icon_popup_menu.MenuItems[3].Enabled = form.window_state != FormWindowState.Minimized; // minimize
			icon_popup_menu.MenuItems[4].Enabled = form.window_state != FormWindowState.Maximized; // maximize
			icon_popup_menu.MenuItems[5].Enabled = true;  // close
			icon_popup_menu.MenuItems[6].Enabled = true;  // next
			
			icon_popup_menu.Show(form, pnt);
		}
		
		private void RestoreItemHandler (object sender, EventArgs e)
		{
			form.WindowState = FormWindowState.Normal;
		}

		private void MoveItemHandler (object sender, EventArgs e)
		{
			int x = 0;
			int y = 0;

			PointToScreen (ref x, ref y);
			Cursor.Position = new Point (x, y);
			form.Cursor = Cursors.Cross;
			state = State.Moving;
			form.Capture = true;
		}

		private void SizeItemHandler (object sender, EventArgs e)
		{
			int x = 0;
			int y = 0;

			PointToScreen (ref x, ref y);
			Cursor.Position = new Point (x, y);
			form.Cursor = Cursors.Cross;
			state = State.Sizing;
			form.Capture = true;
		}		

		private void MinimizeItemHandler (object sender, EventArgs e)
		{
			form.WindowState = FormWindowState.Minimized;
		}

		private void MaximizeItemHandler (object sender, EventArgs e)
		{
			if (form.WindowState != FormWindowState.Maximized)
				form.WindowState = FormWindowState.Maximized;
		}

		private void CloseItemHandler (object sender, EventArgs e)
		{
			form.Close ();
		}

		private void NextItemHandler (object sender, EventArgs e)
		{
			mdi_container.ActivateNextChild ();
		}

		private void DrawIconMenuItem (object sender, DrawItemEventArgs de)
		{
			de.Graphics.DrawIcon (form.Icon, new Rectangle (de.Bounds.X + 2, de.Bounds.Y + 2,
							      de.Bounds.Height - 4, de.Bounds.Height - 4));
		}

		private void MeasureIconMenuItem (object sender, MeasureItemEventArgs me)
		{
			int size = SystemInformation.MenuHeight;
			me.ItemHeight = size;
			me.ItemWidth = size + 2; // some padding
		}

		private void MenuChangedHandler (object sender, EventArgs e)
		{
			CreateMergedMenu ();
		}

		public override void PointToClient (ref int x, ref int y)
		{
			XplatUI.ScreenToClient (mdi_container.Handle, ref x, ref y);
		}

		public override void PointToScreen (ref int x, ref int y)
		{
			XplatUI.ClientToScreen (mdi_container.Handle, ref x, ref y);
		}

		public override void UpdateWindowDecorations (FormWindowState window_state)
		{			
			if (MaximizedMenu != null) {
				switch (window_state) {
				case FormWindowState.Minimized:
				case FormWindowState.Normal:
					MaximizedMenu.Paint -= draw_maximized_buttons;
					MaximizedTitleButtons.Visible = false;
					TitleButtons.Visible = true;
					break;
				case FormWindowState.Maximized:
					MaximizedMenu.Paint += draw_maximized_buttons;
					MaximizedTitleButtons.Visible = true;
					TitleButtons.Visible = false;
					break;
				}
			}
			
			base.UpdateWindowDecorations (window_state);
		}

		public override void SetWindowState (FormWindowState old_state, FormWindowState window_state)
		{
			mdi_container.SetWindowState (form, old_state, window_state, false);
		}

		private void FormClosed (object sender, EventArgs e)
		{
			mdi_container.ChildFormClosed (form);

			if (form.MdiParent.MainMenuStrip != null)
				form.MdiParent.MainMenuStrip.RefreshMdiItems ();

			mdi_container.RemoveControlMenuItems (this);
		}

		public override void DrawMaximizedButtons (object sender, PaintEventArgs pe)
		{
			Size bs = ThemeEngine.Current.ManagedWindowGetMenuButtonSize (this);
			Point pnt =  XplatUI.GetMenuOrigin (mdi_container.ParentForm.Handle);
			int bw = ThemeEngine.Current.ManagedWindowBorderWidth (this);
			TitleButtons buttons = MaximizedTitleButtons;
			
			buttons.Visible = true;
			TitleButtons.Visible = false;
			
			buttons.CloseButton.Rectangle = new Rectangle (mdi_container.ParentForm.Size.Width - 1 - bw - bs.Width - 2,
					pnt.Y + 2, bs.Width, bs.Height);

			buttons.RestoreButton.Rectangle = new Rectangle (buttons.CloseButton.Rectangle.Left - 2 - bs.Width,
					pnt.Y + 2, bs.Width, bs.Height);

			buttons.MinimizeButton.Rectangle = new Rectangle (buttons.RestoreButton.Rectangle.Left - bs.Width,
					pnt.Y + 2, bs.Width, bs.Height);

			DrawTitleButton (pe.Graphics, buttons.MinimizeButton, pe.ClipRectangle);
			DrawTitleButton (pe.Graphics, buttons.RestoreButton, pe.ClipRectangle);
			DrawTitleButton (pe.Graphics, buttons.CloseButton, pe.ClipRectangle);

			buttons.MinimizeButton.Rectangle.Y -= pnt.Y;
			buttons.RestoreButton.Rectangle.Y -= pnt.Y;
			buttons.CloseButton.Rectangle.Y -= pnt.Y;
		}
		
		public bool HandleMenuMouseDown (MainMenu menu, int x, int y)
		{
			Point pt = MenuTracker.ScreenToMenu (menu, new Point (x, y));

			HandleTitleBarDown (pt.X, pt.Y);
			return TitleButtons.AnyPushedTitleButtons;
		}

		public void HandleMenuMouseUp (MainMenu menu, int x, int y)
		{
			Point pt = MenuTracker.ScreenToMenu (menu, new Point (x, y));

			HandleTitleBarUp (pt.X, pt.Y);
		}

		public void HandleMenuMouseLeave (MainMenu menu, int x, int y)
		{
			Point pt = MenuTracker.ScreenToMenu (menu, new Point (x, y));
			HandleTitleBarLeave (pt.X, pt.Y);

		}

		public void HandleMenuMouseMove (MainMenu menu, int x, int y)
		{
			Point pt = MenuTracker.ScreenToMenu (menu, new Point (x, y));

			HandleTitleBarMouseMove (pt.X, pt.Y);

		}

		protected override void HandleTitleBarLeave (int x, int y)
		{
			base.HandleTitleBarLeave (x, y);

			if (maximized_title_buttons != null) {
				maximized_title_buttons.MouseLeave (x, y);
			}
			
			if (IsMaximized)
				XplatUI.InvalidateNC (form.MdiParent.Handle);
		}
		
		protected override void HandleTitleBarUp (int x, int y)
		{			
			if (IconRectangleContains (x, y)) {
				if (!icon_dont_show_popup) {
					if (IsMaximized)
						ClickIconMenuItem (null, null);
					else
						ShowPopup (Point.Empty);
				} else {
					icon_dont_show_popup = false;
				}
				return;
			}
			
			bool was_maximized = IsMaximized;
			base.HandleTitleBarUp (x, y);
			if (maximized_title_buttons != null && was_maximized) {
				maximized_title_buttons.MouseUp (x, y);
			}

			if (IsMaximized)
				XplatUI.InvalidateNC (mdi_container.Parent.Handle);
		}

		protected override void HandleTitleBarDoubleClick (int x, int y)
		{
			if (IconRectangleContains (x, y)) {
				form.Close ();
			} else if (form.MaximizeBox == true) {
				form.WindowState = FormWindowState.Maximized;
			}
			base.HandleTitleBarDoubleClick (x, y);
		}
		
		protected override void HandleTitleBarDown (int x, int y)
		{			
			if (IconRectangleContains (x, y)) {
				if ((DateTime.Now - icon_clicked_time).TotalMilliseconds <= SystemInformation.DoubleClickTime && icon_clicked.X == x && icon_clicked.Y == y) {
					form.Close ();
				} else {
					icon_clicked_time = DateTime.Now;
					icon_clicked.X = x;
					icon_clicked.Y = y;
				}
				
				return;
			}

			base.HandleTitleBarDown (x, y);

			if (maximized_title_buttons != null) {
				maximized_title_buttons.MouseDown (x, y);
			}
			
			if (IsMaximized) {
				XplatUI.InvalidateNC (mdi_container.Parent.Handle);
			}
		}

		protected override void HandleTitleBarMouseMove (int x, int y)
		{
			base.HandleTitleBarMouseMove (x, y);

			if (maximized_title_buttons != null && maximized_title_buttons.MouseMove (x, y))
				XplatUI.InvalidateNC (form.MdiParent.Handle);
		}

		protected override bool HandleLButtonDblClick (ref Message m)
		{

			int x = Control.LowOrder ((int)m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int)m.LParam.ToInt32 ());

			// Correct since we are in NC land.
			NCClientToNC (ref x, ref y);

			if (IconRectangleContains (x, y)) {
				icon_popup_menu.Wnd.Hide ();
				form.Close ();
				return true;
			}
			
			return base.HandleLButtonDblClick (ref m);
		}

		protected override bool HandleLButtonDown (ref Message m)
		{

			int x = Control.LowOrder ((int)m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int)m.LParam.ToInt32 ());

			// Correct y since we are in NC land.
			NCClientToNC(ref x, ref y);

			if (IconRectangleContains (x, y)){
				if ((DateTime.Now - icon_clicked_time).TotalMilliseconds <= SystemInformation.DoubleClickTime) {
					if (icon_popup_menu != null && icon_popup_menu.Wnd != null) {
						icon_popup_menu.Wnd.Hide ();
					}
					form.Close ();
					return true;
				} else if (form.Capture) {
					icon_dont_show_popup = true;
				}
			}
			return base.HandleLButtonDown (ref m);
		}

		protected override bool ShouldRemoveWindowManager (FormBorderStyle style)
		{
			return false;
		}

		protected override void HandleWindowMove (Message m)
		{
			Point pos = Cursor.Position;
			Point move = MouseMove (pos);
			
			if (move.X == 0 && move.Y == 0)
				return;
			
			int x = virtual_position.X + move.X;
			int y = virtual_position.Y + move.Y;
		
			Rectangle client = mdi_container.ClientRectangle;
			if (mdi_container.VerticalScrollbarVisible)
				client.Width -= SystemInformation.VerticalScrollBarWidth;
			if (mdi_container.HorizontalScrollbarVisible)
				client.Height -= SystemInformation.HorizontalScrollBarHeight;

			UpdateVP (x, y, form.Width, form.Height);

			start = pos;
		}

		protected override bool HandleNCMouseMove (ref Message m)
		{
			XplatUI.RequestAdditionalWM_NCMessages (form.Handle, true, true);
			return base.HandleNCMouseMove (ref m);
		}

		protected override void DrawVirtualPosition (Rectangle virtual_position)
		{
			ClearVirtualPosition ();

			if (form.Parent != null)
				XplatUI.DrawReversibleRectangle (form.Parent.Handle, virtual_position, 2);
			prev_virtual_position = virtual_position;
		}

		protected override void ClearVirtualPosition ()
		{
			if (prev_virtual_position != Rectangle.Empty && form.Parent != null)
				XplatUI.DrawReversibleRectangle (form.Parent.Handle,
						prev_virtual_position, 2);
			prev_virtual_position = Rectangle.Empty;
		}

		protected override void OnWindowFinishedMoving ()
		{
			form.Refresh ();
		}

		public override bool IsActive {
			get {
				if (mdi_container == null)
					return false;
				return mdi_container.ActiveMdiChild == form;
			}
		}

		protected override void Activate ()
		{
			if (mdi_container.ActiveMdiChild != form) {
				mdi_container.ActivateChild (form);
			}
			base.Activate ();
		}	
	}
}

