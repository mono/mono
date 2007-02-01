//
// ToolStripDropDownItem.cs
//
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//
#if NET_2_0

using System;
using System.Drawing;
using System.ComponentModel;
using System.Threading;

namespace System.Windows.Forms
{
	[DefaultProperty ("Click")]
	public abstract class ToolStripDropDownItem : ToolStripItem
	{
		private ToolStripDropDown drop_down;
		private ToolStripDropDownDirection drop_down_direction;

		#region Protected Constructors
		protected ToolStripDropDownItem () : this (string.Empty, null, null, string.Empty)
		{
		}

		protected ToolStripDropDownItem (string text, Image image, EventHandler onClick)
			: this (text, image, onClick, string.Empty)
		{
		}

		protected ToolStripDropDownItem (string text, Image image, params ToolStripItem[] dropDownItems)
			: this (text, image, null, string.Empty)
		{
		}

		protected ToolStripDropDownItem (string text, Image image, EventHandler onClick, string name)
			: base (text, image, onClick, name)
		{
			this.drop_down = CreateDefaultDropDown ();
			this.drop_down.ItemAdded += new ToolStripItemEventHandler (DropDown_ItemAdded);
		}
		#endregion

		#region Public Properties
		public ToolStripDropDown DropDown {
			get { return this.drop_down; }
			set { this.drop_down = value; }
		}

		[Browsable (false)]
		public ToolStripDropDownDirection DropDownDirection {
			get { return this.drop_down_direction; }
			set {
				if (!Enum.IsDefined (typeof (ToolStripDropDownDirection), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripDropDownDirection", value));

				this.drop_down_direction = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public ToolStripItemCollection DropDownItems {
			get { return this.drop_down.Items; }
		}

		[Browsable (false)]
		public virtual bool HasDropDownItems {
			get { return this.drop_down.Items.Count != 0; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Pressed {
			get { return base.Pressed || this.DropDown.Visible; }
		}
		#endregion

		#region Protected Properties
		protected internal virtual Point DropDownLocation {
			get {
				Point p;

				if (this.IsOnDropDown) {
					p = Parent.PointToScreen (new Point (this.Bounds.Left, this.Bounds.Top - 1));
					p.X += this.Bounds.Width;
					p.Y += this.Bounds.Left;
					return p;
				}
				else
					p = new Point (this.Bounds.Left, this.Bounds.Bottom - 1);

				return Parent.PointToScreen (p);
			}
		}
		#endregion

		#region Public Methods
		public void HideDropDown ()
		{
			if (!this.DropDown.Visible)
				return;

			this.DropDown.Close (ToolStripDropDownCloseReason.CloseCalled);
			this.is_pressed = false;
			this.Invalidate ();
			this.OnDropDownHide (EventArgs.Empty);
			this.OnDropDownClosed (EventArgs.Empty);
		}

		public void ShowDropDown ()
		{
			if (!this.HasDropDownItems)
				return;
				
			this.DropDown.OwnerItem = this;
			
			this.OnDropDownOpening (this, EventArgs.Empty);
			this.DropDown.Show (this.DropDownLocation);
			this.OnDropDownShow (EventArgs.Empty);
		}
		#endregion

		#region Protected Methods
		protected virtual ToolStripDropDown CreateDefaultDropDown ()
		{
			return new ToolStripDropDown ();
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		protected override void OnBoundsChanged ()
		{
			base.OnBoundsChanged ();
		}

		protected internal virtual void OnDropDownClosed (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DropDownClosedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDropDownHide (EventArgs e)
		{
		}

		protected internal virtual void OnDropDownItemClicked (ToolStripItemClickedEventArgs e)
		{
			ToolStripItemClickedEventHandler eh = (ToolStripItemClickedEventHandler)(Events [DropDownClosedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected internal virtual void OnDropDownOpened (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DropDownOpenedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDropDownShow (EventArgs e)
		{
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}
		#endregion

		#region Public Events
		static object DropDownClosedEvent = new object ();
		static object DropDownItemClickedEvent = new object ();
		static object DropDownOpenedEvent = new object ();
		static object DropDownOpeningEvent = new object ();

		public event EventHandler DropDownClosed {
			add { Events.AddHandler (DropDownClosedEvent, value); }
			remove { Events.RemoveHandler (DropDownClosedEvent, value); }
		}

		public event ToolStripItemClickedEventHandler DropDownItemClicked {
			add { Events.AddHandler (DropDownItemClickedEvent, value); }
			remove { Events.RemoveHandler (DropDownItemClickedEvent, value); }
		}

		public event EventHandler DropDownOpened {
			add { Events.AddHandler (DropDownOpenedEvent, value); }
			remove { Events.RemoveHandler (DropDownOpenedEvent, value); }
		}

		public event EventHandler DropDownOpening {
			add { Events.AddHandler (DropDownOpeningEvent, value); }
			remove { Events.RemoveHandler (DropDownOpeningEvent, value); }
		}
		#endregion

		#region Internal Methods
		internal void HideDropDown (ToolStripDropDownCloseReason reason)
		{
			if (!this.DropDown.Visible)
				return;

			this.DropDown.Close (reason);
			this.is_pressed = false;
			this.Invalidate ();
			this.OnDropDownHide (EventArgs.Empty);
			this.OnDropDownClosed (EventArgs.Empty);
		}
		
		private void DropDown_ItemAdded (object sender, ToolStripItemEventArgs e)
		{
			e.Item.owner_item = this;
		}
		
		private void OnDropDownOpening (object sender, EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[DropDownOpeningEvent]);
			if (eh != null)
				eh (this, e);
		}
		#endregion
	}
}
#endif
