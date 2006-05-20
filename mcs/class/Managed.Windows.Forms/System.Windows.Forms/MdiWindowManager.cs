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

namespace System.Windows.Forms {

	internal class MdiWindowManager : InternalWindowManager {

		private static readonly int MdiBorderStyle = 0xFFFF;

		private MainMenu merged_menu;
		private MainMenu maximized_menu;
		private MenuItem icon_menu;
		private ContextMenu icon_popup_menu;
		private FormWindowState prev_window_state;

		private MdiClient mdi_container;
		private Rectangle prev_virtual_position;

		private Rectangle prev_bounds;

		internal Rectangle IconicBounds;
		internal int mdi_index;

		public MdiWindowManager (Form form, MdiClient mdi_container) : base (form)
		{
			this.mdi_container = mdi_container;
			prev_bounds = form.Bounds;
			prev_window_state = form.window_state;
			form.GotFocus += new EventHandler (FormGotFocus);

			CreateIconMenus ();
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
			MainMenu clone = (MainMenu) parent.Menu.CloneMenu ();
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
			MainMenu res = new MainMenu ();

			res.MenuItems.Add (icon_menu);

			if (parent.Menu != null) {
				MainMenu clone = (MainMenu) parent.Menu.CloneMenu ();
				res.MergeMenu (clone);
			}

			res.MenuItems.Add (new MenuItem ()); // Dummy item to get the menu height correct
			
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

			MenuItem restore = new MenuItem ("Restore", new EventHandler (RestoreItemHandler));
			MenuItem move = new MenuItem ("Move", new EventHandler (MoveItemHandler));
			MenuItem size = new MenuItem ("Size", new EventHandler (SizeItemHandler));
			MenuItem minimize = new MenuItem ("Minimize", new EventHandler (MinimizeItemHandler));
			MenuItem maximize = new MenuItem ("Maximize", new EventHandler (MaximizeItemHandler));
			MenuItem close = new MenuItem ("Close", new EventHandler (CloseItemHandler));
			MenuItem next = new MenuItem ("Next", new EventHandler (NextItemHandler));

			icon_menu.MenuItems.AddRange (new MenuItem [] { restore, move, size, minimize,
									maximize, close, next });
			icon_popup_menu.MenuItems.AddRange (new MenuItem [] { restore, move, size, minimize,
									maximize, close, next });
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

		public override void SetWindowState (FormWindowState old_state, FormWindowState window_state)
		{
			switch (window_state) {
			case FormWindowState.Minimized:
				maximize_button.Caption = CaptionButton.Maximize;
				minimize_button.Caption = CaptionButton.Restore;
				prev_window_state = old_state;
				prev_bounds = form.Bounds;
				mdi_container.ArrangeIconicWindows ();
				break;
			case FormWindowState.Maximized:
				maximize_button.Caption = CaptionButton.Restore;
				minimize_button.Caption = CaptionButton.Minimize;
				prev_window_state = old_state;
				prev_bounds = form.Bounds;
				SizeMaximized ();
				break;
			case FormWindowState.Normal:
				if (prev_window_state == FormWindowState.Maximized) {
					form.WindowState = FormWindowState.Maximized;
					break;
				} else if (prev_window_state == FormWindowState.Minimized) {
					form.WindowState = FormWindowState.Minimized;
					break;
				}
				maximize_button.Caption = CaptionButton.Maximize;
				minimize_button.Caption = CaptionButton.Minimize;
				prev_window_state = form.WindowState;
				form.Bounds = prev_bounds;
				break;
			}

			XplatUI.RequestNCRecalc (mdi_container.Parent.Handle);
		}

		internal void SizeMaximized ()
		{
			Rectangle pb = mdi_container.Bounds;
			int bw = ThemeEngine.Current.ManagedWindowBorderWidth (this);
			form.Bounds = new Rectangle (pb.Left - bw,
					pb.Top - TitleBarHeight - bw,
					pb.Width + bw * 2,
					pb.Height + TitleBarHeight + bw * 2);
		}


		protected override void CloseClicked (object sender, EventArgs e)
		{
			mdi_container.CloseChildForm (form);
		}

		/*
		public override void UpdateBorderStyle (FormBorderStyle border_style)
		{
			base.UpdateBorderStyle (border_style);

			Console.WriteLine ("MDI SETTING BORDER STYLE:   " + border_style);
			if (border_style != FormBorderStyle.None)
				XplatUI.SetBorderStyle (form.Handle, (FormBorderStyle) MdiBorderStyle);
			else
				XplatUI.SetBorderStyle (form.Handle, FormBorderStyle.None);
		}
		*/

		public override void DrawMaximizedButtons (PaintEventArgs pe, MainMenu menu)
		{
			Size bs = ButtonSize;
			Point pnt =  XplatUI.GetMenuOrigin (mdi_container.ParentForm.Handle);
			int bw = ThemeEngine.Current.ManagedWindowBorderWidth (this);
			
			close_button.Rectangle = new Rectangle (menu.Width - bw - bs.Width - 2,
					pnt.Y + 2, bs.Width, bs.Height);

			maximize_button.Rectangle = new Rectangle (close_button.Rectangle.Left - 2 - bs.Width,
					pnt.Y + 2, bs.Width, bs.Height);
				
			minimize_button.Rectangle = new Rectangle (maximize_button.Rectangle.Left - bs.Width,
					pnt.Y + 2, bs.Width, bs.Height);

			DrawTitleButton (pe.Graphics, minimize_button, pe.ClipRectangle);
			DrawTitleButton (pe.Graphics, maximize_button, pe.ClipRectangle);
			DrawTitleButton (pe.Graphics, close_button, pe.ClipRectangle);

			minimize_button.Rectangle.Y -= pnt.Y;
			maximize_button.Rectangle.Y -= pnt.Y;
			close_button.Rectangle.Y -= pnt.Y;
		}

		protected override void HandleTitleBarDown (int x, int y)
		{
			if (form.Icon != null) {
				int bw = ThemeEngine.Current.ManagedWindowBorderWidth (this);
				Rectangle icon = new Rectangle (bw + 3,
						bw + 2, IconWidth, IconWidth);
				if (icon.Contains (x, y)) {
					icon_popup_menu.Show (form, Point.Empty);
					return;
				}
			}

			base.HandleTitleBarDown (x, y);
		}

		protected override bool ShouldRemoveWindowManager (FormBorderStyle style)
		{
			return false;
		}

		protected override void HandleWindowMove (Message m)
		{
			Point move = MouseMove (m);

			int x = virtual_position.X + move.X;
			int y = virtual_position.Y + move.Y;

			UpdateVP (x, y, form.Width, form.Height);
			start = Cursor.Position;
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
			// 	mdi_container.EnsureScrollBars (form.Right, form.Bottom);

			form.Refresh ();
		}

		public override bool IsActive ()
		{
			return mdi_container.ActiveMdiChild == form;
		}

		protected override void Activate ()
		{
			mdi_container.ActivateChild (form);
			base.Activate ();
		}

		private void FormGotFocus (object sender, EventArgs e)
		{
			// Maybe we don't need to do this, maybe we do
			//	mdi_container.ActivateChild (form);
		}			
	}
}

