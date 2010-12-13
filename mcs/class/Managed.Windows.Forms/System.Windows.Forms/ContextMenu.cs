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

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;

namespace System.Windows.Forms
{
	[DefaultEvent("Popup")]
	public class ContextMenu : Menu
	{
		private RightToLeft right_to_left;
		private Control	src_control;

		#region Events
		static object CollapseEvent = new object ();
		static object PopupEvent = new object ();

		public event EventHandler Collapse {
			add { Events.AddHandler (CollapseEvent, value); }
			remove { Events.RemoveHandler (CollapseEvent, value); }
		}

		public event EventHandler Popup {
			add { Events.AddHandler (PopupEvent, value); }
			remove { Events.RemoveHandler (PopupEvent, value); }
		}
		
		#endregion Events

		public ContextMenu () : base (null)
		{
			tracker = new MenuTracker (this);
			right_to_left = RightToLeft.Inherit;
		}

		public ContextMenu (MenuItem [] menuItems) : base (menuItems)
		{
			tracker = new MenuTracker (this);
			right_to_left = RightToLeft.Inherit;
		}
		
		#region Public Properties
		
		[Localizable(true)]
		[DefaultValue (RightToLeft.No)]
		public virtual RightToLeft RightToLeft {
			get { return right_to_left; }
			set { right_to_left = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Control SourceControl {
			get { return src_control; }
		}

		#endregion Public Properties

		#region Public Methods

		protected internal virtual bool ProcessCmdKey (ref Message msg, Keys keyData, Control control)
		{
			src_control = control;
			return ProcessCmdKey (ref msg, keyData);
		}

		protected internal virtual void OnCollapse (EventArgs e)
		{
			EventHandler eh = (EventHandler) (Events [CollapseEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnPopup (EventArgs e)
		{
			EventHandler eh = (EventHandler) (Events [PopupEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		public void Show (Control control, Point pos)
		{
			if (control == null)
				throw new ArgumentException ();

			src_control = control;

			OnPopup (EventArgs.Empty);
			pos = control.PointToScreen (pos);
			MenuTracker.TrackPopupMenu (this, pos);
			OnCollapse (EventArgs.Empty);
		}

		public void Show (Control control, Point pos, LeftRightAlignment alignment)
		{
			Point point;
			
			if (alignment == LeftRightAlignment.Left)
				point = new Point ((pos.X - control.Width), pos.Y);
			else
				point = pos;

			Show (control, point);
		}
		#endregion Public Methods
		internal void Hide ()
		{
			tracker.Deactivate ();
		}
	}
}
