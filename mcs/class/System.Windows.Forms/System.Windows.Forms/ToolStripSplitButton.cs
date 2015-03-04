//
// ToolStripSplitButton.cs
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
using System.Windows.Forms.Design;

namespace System.Windows.Forms
{
	[DefaultEvent ("ButtonClick")]
	[ToolStripItemDesignerAvailability (ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
	public class ToolStripSplitButton : ToolStripDropDownItem
	{
		private bool button_pressed;
		private ToolStripItem default_item;
		private bool drop_down_button_selected;
		private int drop_down_button_width;
		
		#region Public Constructors
		public ToolStripSplitButton()
			: this (string.Empty, null, null, string.Empty)
		{
		}
		
		public ToolStripSplitButton (Image image)
			: this (string.Empty, image, null, string.Empty)
		{
		}
		
		public ToolStripSplitButton (string text)
			: this (text, null, null, string.Empty)
		{
		}
		
		public ToolStripSplitButton (string text, Image image)
			: this (text, image, null, string.Empty)
		{
		}
		
		public ToolStripSplitButton (string text, Image image, EventHandler onClick)
			: this (text, image, onClick, string.Empty)
		{
		}
		
		public ToolStripSplitButton (string text, Image image, params ToolStripItem[] dropDownItems)
			: base (text, image, dropDownItems)
		{
			this.ResetDropDownButtonWidth ();
		}

		public ToolStripSplitButton (string text, Image image, EventHandler onClick, string name)
			: base (text, image, onClick, name)
		{
			this.ResetDropDownButtonWidth ();
		}
		#endregion

		#region Public Properties
		[DefaultValue (true)]
		public new bool AutoToolTip {
			get { return base.AutoToolTip; }
			set { base.AutoToolTip = value; }
		}

		[Browsable (false)]
		public Rectangle ButtonBounds {
			get { return new Rectangle (Bounds.Left, Bounds.Top, this.Bounds.Width - this.drop_down_button_width - 1, this.Height); }
		}

		[Browsable (false)]
		public bool ButtonPressed {
			get { return this.button_pressed; }
		}

		[Browsable (false)]
		public bool ButtonSelected {
			get { return base.Selected; }
		}

		[Browsable (false)]
		[DefaultValue (null)]
		public ToolStripItem DefaultItem {
			get { return this.default_item; }
			set {
				if (this.default_item != value) {
					this.default_item = value;
					this.OnDefaultItemChanged (EventArgs.Empty);
				}
			}
		}
		
		[Browsable (false)]
		public Rectangle DropDownButtonBounds {
			get { return new Rectangle (this.Bounds.Right - this.drop_down_button_width, 0, this.drop_down_button_width, this.Bounds.Height); }
		}

		[Browsable (false)]
		public bool DropDownButtonPressed {
			get { return this.drop_down_button_selected || (this.HasDropDownItems && this.DropDown.Visible); }
		}

		[Browsable (false)]
		public bool DropDownButtonSelected {
			get { return base.Selected; }
		}
		
		public int DropDownButtonWidth {
			get { return this.drop_down_button_width; }
			set { 
				if (value < 0)
					throw new ArgumentOutOfRangeException ();
				if (this.drop_down_button_width != value) {
					this.drop_down_button_width = value;
					CalculateAutoSize ();
				}
			}
		}

		[Browsable (false)]
		public Rectangle SplitterBounds {
			get { return new Rectangle (this.Bounds.Width - this.drop_down_button_width - 1, 0, 1, this.Height); }
		}
		#endregion

		#region Protected Properties
		protected override bool DefaultAutoToolTip {
			get { return true; }
		}

		protected internal override bool DismissWhenClicked {
			get { return true; }
		}
		#endregion

		#region Public Methods
		public override Size GetPreferredSize (Size constrainingSize)
		{
			// base should calculate the button part for us, add the splitter
			// and drop down arrow part to that
			Size s = base.GetPreferredSize (constrainingSize);

			if (s.Width < 23)
				s.Width = 23;

			// If we are a fixed size, we can't add more in for the drop down
			// button, but we can for autosize
			if (AutoSize)
				s.Width += (this.drop_down_button_width - 2);
			
			return s;
		}
		
		public virtual void OnButtonDoubleClick (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ButtonDoubleClickEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		public void PerformButtonClick ()
		{
			if (this.Enabled)
				this.OnButtonClick (EventArgs.Empty);
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual void ResetDropDownButtonWidth ()
		{
			this.DropDownButtonWidth = 11;
		}
		#endregion

		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new ToolStripSplitButtonAccessibleObject (this);
		}
		
		protected override ToolStripDropDown CreateDefaultDropDown ()
		{
			ToolStripDropDownMenu tsddm = new ToolStripDropDownMenu ();
			tsddm.OwnerItem = this;
			return tsddm;
		}
		
		protected virtual void OnButtonClick (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [ButtonClickEvent];
			if (eh != null)
				eh (this, e);
		}
		
		protected virtual void OnDefaultItemChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [DefaultItemChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (this.ButtonBounds.Contains (e.Location))
			{
				this.button_pressed = true;
				this.Invalidate ();
				base.OnMouseDown (e);
			}
			else if (this.DropDownButtonBounds.Contains (e.Location))
			{
				if (this.DropDown.Visible)
					this.HideDropDown (ToolStripDropDownCloseReason.ItemClicked);
				else
					this.ShowDropDown ();
			
				this.Invalidate ();
				base.OnMouseDown (e);
			}
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			this.drop_down_button_selected = false;
			this.button_pressed = false;
			
			this.Invalidate ();
			
			base.OnMouseLeave (e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			this.button_pressed = false;
			this.Invalidate ();
			
			base.OnMouseUp (e);
		}
		
		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);

			if (this.Owner != null) {
				Color font_color = this.Enabled ? this.ForeColor : SystemColors.GrayText;
				Image draw_image = this.Enabled ? this.Image : ToolStripRenderer.CreateDisabledImage (this.Image);

				this.Owner.Renderer.DrawSplitButton (new System.Windows.Forms.ToolStripItemRenderEventArgs (e.Graphics, this));

				Rectangle text_layout_rect;
				Rectangle image_layout_rect;

				Rectangle r = this.ContentRectangle;
				r.Width -= (this.drop_down_button_width + 1);
				
				this.CalculateTextAndImageRectangles (r, out text_layout_rect, out image_layout_rect);

				if (text_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, font_color, this.Font, this.TextAlign));
				if (image_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemImage (new System.Windows.Forms.ToolStripItemImageRenderEventArgs (e.Graphics, this, draw_image, image_layout_rect));

				this.Owner.Renderer.DrawArrow (new ToolStripArrowRenderEventArgs (e.Graphics, this, new Rectangle (this.Width - 9, 1, 6, this.Height), Color.Black, ArrowDirection.Down));
				
				return;
			}
		}

		protected override void OnRightToLeftChanged (EventArgs e)
		{
			base.OnRightToLeftChanged (e);
		}
		
		protected internal override bool ProcessDialogKey (Keys keyData)
		{
			if (this.Selected && keyData == Keys.Enter && this.DefaultItem != null) {
				this.DefaultItem.FireEvent (EventArgs.Empty, ToolStripItemEventType.Click);
				return true;
			}

			return base.ProcessDialogKey (keyData);
		}

		protected internal override bool ProcessMnemonic (char charCode)
		{
			if (!this.Selected)
				this.Parent.ChangeSelection (this);

			if (this.HasDropDownItems)
				this.ShowDropDown ();
			else
				this.PerformClick ();

			return true;
		}
		#endregion

		#region Internal Methods
		internal override void HandleClick (int mouse_clicks, EventArgs e)
		{
			base.HandleClick (mouse_clicks, e);

			MouseEventArgs mea = e as MouseEventArgs;
			
			if (mea != null)
				if (ButtonBounds.Contains (mea.Location))
					OnButtonClick (EventArgs.Empty);
		}
		#endregion
		
		#region Public Events
		static object ButtonClickEvent = new object ();
		static object ButtonDoubleClickEvent = new object ();
		static object DefaultItemChangedEvent = new object ();

		public event EventHandler ButtonClick {
			add { Events.AddHandler (ButtonClickEvent, value); }
			remove {Events.RemoveHandler (ButtonClickEvent, value); }
		}
		public event EventHandler ButtonDoubleClick {
			add { Events.AddHandler (ButtonDoubleClickEvent, value); }
			remove {Events.RemoveHandler (ButtonDoubleClickEvent, value); }
		}
		public event EventHandler DefaultItemChanged {
			add { Events.AddHandler (DefaultItemChangedEvent, value); }
			remove {Events.RemoveHandler (DefaultItemChangedEvent, value); }
		}
		#endregion

		#region ToolStripSplitButtonAccessibleObject Class
		public class ToolStripSplitButtonAccessibleObject : ToolStripItemAccessibleObject
		{
			#region Public Constructor
			public ToolStripSplitButtonAccessibleObject (ToolStripSplitButton item) : base (item)
			{
			}
			#endregion

			#region Public Method
			public override void DoDefaultAction ()
			{
				(owner_item as ToolStripSplitButton).PerformButtonClick ();
			}
			#endregion
		}
		#endregion
	}
}
