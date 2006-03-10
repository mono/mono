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

		private MdiClient mdi_container;
		private Rectangle prev_virtual_position;

		private Rectangle prev_bounds;
		internal Rectangle IconicBounds;

		public MdiWindowManager (Form form, MdiClient mdi_container) : base (form)
		{
			this.mdi_container = mdi_container;
			form.GotFocus += new EventHandler (FormGotFocus);

			icon_menu = CreateIconMenu ();
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

		private MenuItem CreateIconMenu ()
		{
			MenuItem res = new MenuItem ();

			res.OwnerDraw = true;
			res.MeasureItem += new MeasureItemEventHandler (MeasureIconMenuItem);
			res.DrawItem += new DrawItemEventHandler (DrawIconMenuItem);

			MenuItem restore = new MenuItem ("Restore");;
			MenuItem move = new MenuItem ("Move");
			MenuItem size = new MenuItem ("Size");
			MenuItem minimize = new MenuItem ("Minimize");
			MenuItem maximize = new MenuItem ("Maximize");
			MenuItem close = new MenuItem ("Close");
			MenuItem next = new MenuItem ("Next");

			res.MenuItems.AddRange (new MenuItem [] { restore, move, size, minimize,
									maximize, close, next });
									
			return res;
		}

		private void DrawIconMenuItem (object sender, DrawItemEventArgs de)
		{
			de.Graphics.DrawIcon (form.Icon, new Rectangle (2, 2, de.Bounds.Height - 4, de.Bounds.Height - 4));
		}

		private void MeasureIconMenuItem (object sender, MeasureItemEventArgs me)
		{
			Form parent = (Form) mdi_container.Parent;
			int size = MaximizedMenu.MenuItems [0].MenuHeight;
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

		public override void SetWindowState (FormWindowState window_state)
		{
			switch (window_state) {
			case FormWindowState.Minimized:
				prev_bounds = form.Bounds;
				mdi_container.ArrangeIconicWindows ();
				break;
			case FormWindowState.Maximized:
				prev_bounds = form.Bounds;
				SizeMaximized ();
				break;
			case FormWindowState.Normal:
				form.Bounds = prev_bounds;
				break;
			}
		}

		internal void SizeMaximized ()
		{
			Rectangle pb = mdi_container.Bounds;
			form.Bounds = new Rectangle (pb.Left - BorderWidth,
					pb.Top - TitleBarHeight - BorderWidth,
					pb.Width + BorderWidth * 2,
					pb.Height + TitleBarHeight + BorderWidth * 2);
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

		protected override bool IsActive ()
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

