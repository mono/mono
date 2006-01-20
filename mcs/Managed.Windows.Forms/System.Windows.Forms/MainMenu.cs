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

// COMPLETE

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
	[ToolboxItemFilter("System.Windows.Forms.MainMenu", ToolboxItemFilterType.Allow)]
	public class MainMenu : Menu
	{
		private RightToLeft right_to_left = RightToLeft.Inherit;
		private Form form = null;

    		public MainMenu () : base (null)
    		{
			
    		}

		public MainMenu (MenuItem[] items) : base (items)
		{
			
		}

		#region Public Properties
		[Localizable(true)]
		public virtual RightToLeft RightToLeft {
			get { return right_to_left;}
			set { right_to_left = value; }
		}

		#endregion Public Properties

		#region Public Methods
			
		public virtual MainMenu CloneMenu ()
		{
			MainMenu new_menu = new MainMenu ();
			new_menu.CloneMenu (this);
			return new_menu;
		}
		
		protected override void Dispose (bool disposing)
		{			
			base.Dispose (disposing);			
		}

		public Form GetForm ()
		{
			return form;
		}

		public override string ToString ()
		{
			return base.ToString () + ", GetForm: " + form;
		}

		#endregion Public Methods
		
		#region Private Methods

		internal void Draw () 		
		{
			Draw (Rect);
		}

		internal void Draw (Rectangle rect)
		{
			Graphics g;

			if (Wnd.window.Handle == IntPtr.Zero)
				return;

			X = rect.X;
			Y = rect.Y;
			Height = Rect.Height;

			g = XplatUI.GetMenuDC(Wnd.window.Handle, IntPtr.Zero);
			ThemeEngine.Current.DrawMenuBar (g, this, rect);
			XplatUI.ReleaseMenuDC(Wnd.window.Handle, g);
		}
		
		internal void SetForm (Form form)
		{
			this.form = form;
			Wnd = form;
			
			if (tracker == null)
				tracker = new MenuTracker (this); 
		}
		
		internal override void OnMenuChanged (EventArgs e)
		{
			base.OnMenuChanged (EventArgs.Empty);
			if (form == null)
				return;

			Height = 0;
			Draw ();
		}

		/* Mouse events from the form */
		internal void OnMouseDown (object window, MouseEventArgs args)
		{			
			tracker.OnClick (args);
		}
		
		internal void OnMouseMove (object window, MouseEventArgs e)
		{			
			MouseEventArgs args = new MouseEventArgs (e.Button, e.Clicks, Control.MousePosition.X, Control.MousePosition.Y, e.Delta);
			tracker.OnMotion (args);
		}
		
		#endregion Private Methods
	}
}


