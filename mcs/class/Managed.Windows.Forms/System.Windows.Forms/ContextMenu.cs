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

using System;
using System.Drawing;

namespace System.Windows.Forms
{
	public class ContextMenu : Menu
	{
		private RightToLeft right_to_left;
		private Control	src_control;

		#region Events
		public event EventHandler Popup;
		#endregion Events

		public ContextMenu () : base (null)
		{
			right_to_left = RightToLeft.Inherit;
		}

		public ContextMenu (MenuItem[] items) : base (items)
		{
			right_to_left = RightToLeft.Inherit;
		}
		 
		#region Public Properties
		public virtual RightToLeft RightToLeft {
			get { return right_to_left; }
			set { right_to_left = value; }
		}

		public Control SourceControl {
			get { return src_control; }
		}

		#endregion Public Properties

		#region Public Methods
				
		protected internal virtual void OnPopup (EventArgs e)
		{
			if (Popup != null)
				Popup (this, e);
		}
		
		public void Show (Control control, Point pos)
		{
			if (control == null)
				throw new ArgumentException ();

			src_control = control;

			OnPopup (EventArgs.Empty);
			MenuAPI.TrackPopupMenu (Handle, Handle,	Control.MousePosition, false, control);
		}

		#endregion Public Methods


	}
}


