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

namespace System.Windows.Forms
{
	public class MainMenu : Menu
	{
		private RightToLeft right_to_left;
		private Form form;

    		public MainMenu () : base (null)
    		{
			form = null;
    		}

		public MainMenu (MenuItem[] items) : base (items)
		{
			form = null;
		}

		#region Public Properties
		
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
		
		protected override IntPtr CreateMenuHandle ()
		{				
			return MenuAPI.CreateMenu ();						
		}

		protected void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		public Form GetForm ()
		{
			return form;
		}

		public override string ToString ()
		{
			return base.ToString ();
		}

		#endregion Public Methods
		
		#region Private Methods
		
		internal void SetForm (Form form)
		{
			this.form = form;
		}
		
		/* Mouse events from the form */
		internal void OnMouseDown (Form window, MouseEventArgs e)
		{			
			MenuAPI.TrackBarMouseEvent (Handle, window, e, MenuAPI.MenuMouseEvent.Down);
		}
		
		internal void OnMouseMove (Form window, MouseEventArgs e)
		{			
			MouseEventArgs ev = new MouseEventArgs (e.Button, e.Clicks, Control.MousePosition.X, Control.MousePosition.Y, e.Delta);
			MenuAPI.TrackBarMouseEvent (Handle, window, ev, MenuAPI.MenuMouseEvent.Move);
		}
		
		#endregion Private Methods
	}
}


