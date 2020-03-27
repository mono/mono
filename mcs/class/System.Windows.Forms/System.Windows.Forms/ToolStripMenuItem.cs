//
// ToolStripMenuItem.cs
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
using System.ComponentModel.Design.Serialization;

namespace System.Windows.Forms
{
	[ToolStripItemDesignerAvailability (ToolStripItemDesignerAvailability.MenuStrip | ToolStripItemDesignerAvailability.ContextMenuStrip)]
	[DesignerSerializer ("System.Windows.Forms.Design.ToolStripMenuItemCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
	public class ToolStripMenuItem : ToolStripDropDownItem
	{
		private CheckState checked_state;
		private bool check_on_click;
		private bool close_on_mouse_release;
		private string shortcut_display_string;
		private Keys shortcut_keys = Keys.None;
		private bool show_shortcut_keys = true;
		private Form mdi_client_form;

		#region Public Constructors
		public ToolStripMenuItem ()
			: this (null, null, null, string.Empty)
		{
		}

		public ToolStripMenuItem (Image image)
			: this (null, image, null, string.Empty)
		{
		}

		public ToolStripMenuItem (string text)
			: this (text, null, null, string.Empty)
		{
		}

		public ToolStripMenuItem (string text, Image image)
			: this (text, image, null, string.Empty)
		{
		}

		public ToolStripMenuItem (string text, Image image, EventHandler onClick)
			: this (text, image, onClick, string.Empty)
		{
		}

		public ToolStripMenuItem (string text, Image image, params ToolStripItem[] dropDownItems)
			: this (text, image, null, string.Empty)
		{
			if (dropDownItems != null)
				foreach (ToolStripItem tsi in dropDownItems)
					this.DropDownItems.Add (tsi);
		}

		public ToolStripMenuItem (string text, Image image, EventHandler onClick, Keys shortcutKeys)
			: this (text, image, onClick, string.Empty)
		{
		}

		public ToolStripMenuItem (string text, Image image, EventHandler onClick, string name)
			: base (text, image, onClick, name)
		{
			base.Overflow = ToolStripItemOverflow.Never;
		}
		#endregion

		#region Public Properties
		[Bindable (true)]
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.All)]
		public bool Checked {
			get {
				switch (this.checked_state) {
					case CheckState.Unchecked:
					default:
						return false;
					case CheckState.Checked:
					case CheckState.Indeterminate:
						return true;
				}
			}
			set {
				CheckState = value ? CheckState.Checked : CheckState.Unchecked;
			}
		}

		[DefaultValue (false)]
		public bool CheckOnClick {
			get { return this.check_on_click; }
			set {
				if (this.check_on_click != value) {
					this.check_on_click = value;
					OnUIACheckOnClickChangedEvent (EventArgs.Empty);
				}
			}
		}

		[Bindable (true)]
		[DefaultValue (CheckState.Unchecked)]
		[RefreshProperties (RefreshProperties.All)]
		public CheckState CheckState {
			get { return this.checked_state; }
			set
			{
				if (!Enum.IsDefined (typeof (CheckState), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for CheckState", value));

				if (value == checked_state)
					return;

				this.checked_state = value;
				this.Invalidate ();
				this.OnCheckedChanged (EventArgs.Empty);
				this.OnCheckStateChanged (EventArgs.Empty);
			}
		}

		public override bool Enabled {
			get { return base.Enabled; }
			set { base.Enabled = value; }
		}

		[Browsable (false)]
		public bool IsMdiWindowListEntry {
			get { return this.mdi_client_form != null; }
		}
		
		[DefaultValue (ToolStripItemOverflow.Never)]
		public new ToolStripItemOverflow Overflow {
			get { return base.Overflow; }
			set { base.Overflow = value; }
		}
		
		[Localizable (true)]
		[DefaultValue (true)]
		public bool ShowShortcutKeys {
			get { return this.show_shortcut_keys; }
			set { this.show_shortcut_keys = value; }
		}
		
		[Localizable (true)]
		[DefaultValue (null)]
		public string ShortcutKeyDisplayString {
			get { return this.shortcut_display_string; }
			set { this.shortcut_display_string = value; }
		}
		
		[Localizable (true)]
		[DefaultValue (Keys.None)]
		public Keys ShortcutKeys {
			get { return this.shortcut_keys; }
			set { 
				if (this.shortcut_keys != value) {
					this.shortcut_keys = value;
					
					if (this.Parent != null)
						ToolStripManager.AddToolStripMenuItem (this);
				}
			 }
		}
		#endregion

		#region Protected Properties
		protected internal override Padding DefaultMargin {
			get { return new Padding (0); }
		}

		protected override Padding DefaultPadding {
			get { return new Padding (4, 0, 4, 0); }
		}

		protected override Size DefaultSize {
			get { return new Size (32, 19); }
		}
		#endregion

		#region Protected Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new ToolStripMenuItemAccessibleObject ();
		}
		
		protected override ToolStripDropDown CreateDefaultDropDown ()
		{
			ToolStripDropDownMenu tsddm = new ToolStripDropDownMenu ();
			tsddm.OwnerItem = this;
			return tsddm;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		protected virtual void OnCheckedChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [CheckedChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCheckStateChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)Events [CheckStateChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected override void OnClick (EventArgs e)
		{
			if (!this.Enabled)
				return;
				
			if (this.HasDropDownItems) {
				base.OnClick (e);
				return;
			}
				
			if (this.OwnerItem is ToolStripDropDownItem)
				(this.OwnerItem as ToolStripDropDownItem).OnDropDownItemClicked (new ToolStripItemClickedEventArgs (this));

			if (this.IsOnDropDown) {
				ToolStrip ts = this.GetTopLevelToolStrip ();
				
				if (ts != null)
					ts.Dismiss (ToolStripDropDownCloseReason.ItemClicked);
			}

			if (this.IsMdiWindowListEntry) {
				this.mdi_client_form.MdiParent.MdiContainer.ActivateChild (this.mdi_client_form);
				return;
			}
			
			if (this.check_on_click)
				this.Checked = !this.Checked;

			base.OnClick (e);
			
			if (!this.IsOnDropDown && !this.HasDropDownItems) {
				ToolStrip ts = this.GetTopLevelToolStrip ();

				if (ts != null)
					ts.Dismiss (ToolStripDropDownCloseReason.ItemClicked);
			}
		}

		protected override void OnDropDownHide (EventArgs e)
		{
			base.OnDropDownHide (e);
		}

		protected override void OnDropDownShow (EventArgs e)
		{
			base.OnDropDownShow (e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (!this.IsOnDropDown && this.HasDropDownItems && this.DropDown.Visible)
				this.close_on_mouse_release = true;
				
			if (Enabled && !this.DropDown.Visible)
				this.ShowDropDown ();

			base.OnMouseDown (e);
		}

		protected override void OnMouseEnter (EventArgs e)
		{
			if (this.IsOnDropDown && this.HasDropDownItems && Enabled)
				this.ShowDropDown ();

			base.OnMouseEnter (e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			if (this.close_on_mouse_release) {
				this.Parent.Dismiss (ToolStripDropDownCloseReason.ItemClicked);
				this.Invalidate ();
				this.close_on_mouse_release = false;
			}
				
			if (!this.HasDropDownItems && Enabled)
				base.OnMouseUp (e);
		}

		protected override void OnOwnerChanged (EventArgs e)
		{
			base.OnOwnerChanged (e);
		}

		protected override void OnPaint (System.Windows.Forms.PaintEventArgs e)
		{
			base.OnPaint (e);

			// Can't render without an owner
			if (this.Owner == null)
				return;
				
			// If DropDown.ShowImageMargin is false, we don't display the image
			Image draw_image = this.UseImageMargin ? this.Image : null;
			
			// Disable this color detection until we do the color detection for ToolStrip *completely*
			// Color font_color = this.ForeColor == SystemColors.ControlText ? SystemColors.MenuText : this.ForeColor;
			Color font_color = ForeColor;
			
			if ((this.Selected || this.Pressed) && this.IsOnDropDown && font_color == SystemColors.MenuText)
				font_color = SystemColors.HighlightText;
			
			if (!this.Enabled && this.ForeColor == SystemColors.ControlText)
				font_color = SystemColors.GrayText;
			
			// Gray stuff out if we're disabled
			draw_image = this.Enabled ? draw_image : ToolStripRenderer.CreateDisabledImage (draw_image);
				
			// Draw our background
			this.Owner.Renderer.DrawMenuItemBackground (new ToolStripItemRenderEventArgs (e.Graphics, this));

			// Figure out where our text and image go
			Rectangle text_layout_rect;
			Rectangle image_layout_rect;

			this.CalculateTextAndImageRectangles (out text_layout_rect, out image_layout_rect);

			if (this.IsOnDropDown) {
				if (!this.UseImageMargin) {
					image_layout_rect = Rectangle.Empty;
					text_layout_rect = new Rectangle (8, text_layout_rect.Top, text_layout_rect.Width, text_layout_rect.Height);
				} else {
					text_layout_rect = new Rectangle (35, text_layout_rect.Top, text_layout_rect.Width, text_layout_rect.Height);
				
					if (image_layout_rect != Rectangle.Empty)
						image_layout_rect = new Rectangle (new Point (4, 3), base.GetImageSize ());
				}

				if (this.Checked && this.ShowMargin)
					this.Owner.Renderer.DrawItemCheck (new ToolStripItemImageRenderEventArgs (e.Graphics, this, new Rectangle (2, 1, 19, 19)));
			}
			if (text_layout_rect != Rectangle.Empty)
				this.Owner.Renderer.DrawItemText (new ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, font_color, this.Font, this.TextAlign));

			string key_string = GetShortcutDisplayString ();
			
			if (!string.IsNullOrEmpty (key_string) && !this.HasDropDownItems) {
				int offset = 15;
				Size key_string_size = TextRenderer.MeasureText (key_string, this.Font);
				Rectangle key_string_rect = new Rectangle (this.ContentRectangle.Right - key_string_size.Width - offset, text_layout_rect.Top, key_string_size.Width, text_layout_rect.Height);
				this.Owner.Renderer.DrawItemText (new ToolStripItemTextRenderEventArgs (e.Graphics, this, key_string, key_string_rect, font_color, this.Font, this.TextAlign));
			}
				
			if (image_layout_rect != Rectangle.Empty)
				this.Owner.Renderer.DrawItemImage (new ToolStripItemImageRenderEventArgs (e.Graphics, this, draw_image, image_layout_rect));

			if (this.IsOnDropDown && this.HasDropDownItems && this.Parent is ToolStripDropDownMenu)
				this.Owner.Renderer.DrawArrow (new ToolStripArrowRenderEventArgs (e.Graphics, this, new Rectangle (this.Bounds.Width - 17, 2, 10, 20), Color.Black, ArrowDirection.Right));
			
			return;
		}

		protected internal override bool ProcessCmdKey (ref Message m, Keys keyData)
		{
			Control source = Control.FromHandle (m.HWnd);
			Form f = source == null ? null : (Form)source.TopLevelControl;

			if (this.Enabled && keyData == this.shortcut_keys && GetTopLevelControl () == f) {
				this.FireEvent (EventArgs.Empty, ToolStripItemEventType.Click);
				return true;
			}
				
			return base.ProcessCmdKey (ref m, keyData);
		}

		Control GetTopLevelControl ()
		{
			ToolStripItem item = this;
			while (item.OwnerItem != null)
				item = item.OwnerItem;

			if (item.Owner == null)
				return null;

			if (item.Owner is ContextMenuStrip ownerContextMenuStrip) 
				return ownerContextMenuStrip.SourceControl?.TopLevelControl;

			// MainMenuStrip
			return item.Owner.TopLevelControl;
		}

		protected internal override bool ProcessMnemonic (char charCode)
		{
			if (!this.Selected)
				this.Parent.ChangeSelection (this);
				
			if (this.HasDropDownItems) {
				ToolStripManager.SetActiveToolStrip (this.Parent, true);
				this.ShowDropDown ();
				this.DropDown.SelectNextToolStripItem (null, true);
			} else
				this.PerformClick ();
			
			return true;
		}
		
		protected internal override void SetBounds (Rectangle rect)
		{
			base.SetBounds (rect);
		}
		#endregion

		#region Public Events
		static object CheckedChangedEvent = new object ();
		static object CheckStateChangedEvent = new object ();

		public event EventHandler CheckedChanged {
			add { Events.AddHandler (CheckedChangedEvent, value); }
			remove {Events.RemoveHandler (CheckedChangedEvent, value); }
		}

		public event EventHandler CheckStateChanged {
			add { Events.AddHandler (CheckStateChangedEvent, value); }
			remove {Events.RemoveHandler (CheckStateChangedEvent, value); }
		}
		#endregion

		#region UIA Framework Events
		static object UIACheckOnClickChangedEvent = new object ();
		
		internal event EventHandler UIACheckOnClickChanged {
			add { Events.AddHandler (UIACheckOnClickChangedEvent, value); }
			remove { Events.RemoveHandler (UIACheckOnClickChangedEvent, value); }
		}

		internal void OnUIACheckOnClickChangedEvent (EventArgs args)
		{
			EventHandler eh
				= (EventHandler) Events [UIACheckOnClickChangedEvent];
			if (eh != null)
				eh (this, args);
		}
		#endregion

		#region Internal Properties
		internal Form MdiClientForm {
			get { return this.mdi_client_form; }
			set { this.mdi_client_form = value; }
		}
		#endregion

		#region Internal Methods
		internal override Size CalculatePreferredSize (Size constrainingSize)
		{
			Size base_size = base.CalculatePreferredSize (constrainingSize);
			
			string key_string = GetShortcutDisplayString ();
			
			if (string.IsNullOrEmpty (key_string))
				return base_size;
			
			Size text_size = TextRenderer.MeasureText (key_string, this.Font);
			
			return new Size (base_size.Width + text_size.Width - 25, base_size.Height);
		}
		
		internal string GetShortcutDisplayString ()
		{
			if (this.show_shortcut_keys == false)
				return string.Empty;
			if (this.Parent == null || !(this.Parent is ToolStripDropDownMenu))
				return string.Empty;
				
			string key_string = string.Empty;

			if (!string.IsNullOrEmpty (this.shortcut_display_string))
				key_string = this.shortcut_display_string;
			else if (this.shortcut_keys != Keys.None) {
				KeysConverter kc = new KeysConverter ();
				key_string = kc.ConvertToString (this.shortcut_keys);
			}
			
			return key_string;
		}
		
		internal void HandleAutoExpansion ()
		{
			if (this.HasDropDownItems) {
				this.ShowDropDown ();
				this.DropDown.SelectNextToolStripItem (null, true);
			}
		}

		internal override void HandleClick (int mouse_clicks, EventArgs e)
		{
			this.OnClick (e);
			
			if (Parent != null)
				Parent.Invalidate ();
		}
		#endregion

		#region ToolStripMenuItemAccessibleObject
		private class ToolStripMenuItemAccessibleObject : AccessibleObject
		{
		}
		#endregion
	}
}
