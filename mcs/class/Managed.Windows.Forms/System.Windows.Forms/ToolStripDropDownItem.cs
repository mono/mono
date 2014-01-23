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

using System;
using System.Drawing;
using System.ComponentModel;
using System.Threading;

namespace System.Windows.Forms
{
	[DefaultProperty ("DropDownItems")]
	[Designer ("System.Windows.Forms.Design.ToolStripMenuItemDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class ToolStripDropDownItem : ToolStripItem
	{
		internal ToolStripDropDown drop_down;
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
		}
		#endregion

		#region Public Properties
		[TypeConverter (typeof (ReferenceConverter))]
		public ToolStripDropDown DropDown {
			get {
				if (this.drop_down == null) {
					this.drop_down = CreateDefaultDropDown ();
					this.drop_down.ItemAdded += new ToolStripItemEventHandler (DropDown_ItemAdded);
				}
			
				return this.drop_down;
			}
			set { 
				this.drop_down = value;
				this.drop_down.OwnerItem = this;
			}
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
			get { return this.DropDown.Items; }
		}

		[Browsable (false)]
		public virtual bool HasDropDownItems {
			get { return this.drop_down != null && this.DropDown.Items.Count != 0; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Pressed {
			get { return base.Pressed || (this.drop_down != null && this.DropDown.Visible); }
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
			if (this.drop_down == null || !this.DropDown.Visible)
				return;

			// OnDropDownHide is called before actually closing DropDown
			this.OnDropDownHide (EventArgs.Empty);
			this.DropDown.Close (ToolStripDropDownCloseReason.CloseCalled);
			this.is_pressed = false;
			this.Invalidate ();
		}

		public void ShowDropDown ()
		{
			// Don't go through this whole deal if
			// the DropDown is already visible
			if (this.DropDown.Visible)
				return;
				
			// Call this before the HasDropDownItems check to give
			// users a chance to handle it and add drop down items
			this.OnDropDownShow (EventArgs.Empty);
			
			if (!this.HasDropDownItems)
				return;
			
			this.Invalidate ();
			this.DropDown.Show (this.DropDownLocation);
		}
		#endregion

		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new ToolStripDropDownItemAccessibleObject (this);
		}
		
		protected virtual ToolStripDropDown CreateDefaultDropDown ()
		{
			ToolStripDropDown tsdd = new ToolStripDropDown ();
			tsdd.OwnerItem = this;
			return tsdd;
		}

		protected override void Dispose (bool disposing)
		{
			if (!IsDisposed) {
				if(disposing) {
					if (this.HasDropDownItems)
						foreach (ToolStripItem tsi in this.DropDownItems)
							if (tsi is ToolStripMenuItem)
								ToolStripManager.RemoveToolStripMenuItem ((ToolStripMenuItem)tsi);

					if (drop_down != null)
						ToolStripManager.RemoveToolStrip (drop_down);
				}
				base.Dispose (disposing);
			}
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
			ToolStripItemClickedEventHandler eh = (ToolStripItemClickedEventHandler)(Events [DropDownItemClickedEvent]);
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
			EventHandler eh = (EventHandler)(Events[DropDownOpeningEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);

			// don't use DropDown directly, since doing that
			// would created the DropDown control
			if (drop_down != null)
				drop_down.Font = Font;
		}

		protected override void OnRightToLeftChanged (EventArgs e)
		{
			base.OnRightToLeftChanged (e);
		}
		
		protected internal override bool ProcessCmdKey (ref Message m, Keys keyData)
		{
			if (this.HasDropDownItems)
				foreach (ToolStripItem tsi in this.DropDownItems)
					if (tsi.ProcessCmdKey (ref m, keyData) == true)
						return true;

			return base.ProcessCmdKey (ref m, keyData);
		}

		protected internal override bool ProcessDialogKey (Keys keyData)
		{
			if (!this.Selected || !this.HasDropDownItems)
				return base.ProcessDialogKey (keyData);
				
			if (!this.IsOnDropDown) {
				if (this.Parent.Orientation == Orientation.Horizontal) {
					if (keyData == Keys.Down || keyData == Keys.Enter) {
						if (this.Parent is MenuStrip)
							(this.Parent as MenuStrip).MenuDroppedDown = true;
						this.ShowDropDown ();
						this.DropDown.SelectNextToolStripItem (null, true);
						return true;
					}
				} else {
					if (keyData == Keys.Right || keyData == Keys.Enter) {
						if (this.Parent is MenuStrip)
							(this.Parent as MenuStrip).MenuDroppedDown = true;
						this.ShowDropDown ();
						this.DropDown.SelectNextToolStripItem (null, true);
						return true;
					}
				}
			} else {
				if (keyData == Keys.Right || keyData == Keys.Enter) {
					if (this.HasDropDownItems) {
						this.ShowDropDown ();
						this.DropDown.SelectNextToolStripItem (null, true);
						return true;
					}
				}
			}
			
			
			return base.ProcessDialogKey (keyData);
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
		internal override void Dismiss (ToolStripDropDownCloseReason reason)
		{
			if (this.HasDropDownItems && this.DropDown.Visible)
				this.DropDown.Dismiss (reason);
				
			base.Dismiss (reason);
		}

		internal override void HandleClick (int mouse_clicks, EventArgs e)
		{
			OnClick (e);
		}
		
		internal void HideDropDown (ToolStripDropDownCloseReason reason)
		{
			if (this.drop_down == null || !this.DropDown.Visible)
				return;

			// OnDropDownHide is called before actually closing DropDown
			this.OnDropDownHide (EventArgs.Empty);
			this.DropDown.Close (reason);
			this.is_pressed = false;
			this.Invalidate ();
		}
		
		private void DropDown_ItemAdded (object sender, ToolStripItemEventArgs e)
		{
			e.Item.owner_item = this;
		}
		#endregion
	}
}
