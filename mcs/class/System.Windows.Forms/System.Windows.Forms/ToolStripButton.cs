//
// ToolStripButton.cs
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
	[ToolStripItemDesignerAvailability (ToolStripItemDesignerAvailability.ToolStrip)]
	public class ToolStripButton : ToolStripItem
	{
		private CheckState checked_state;
		private bool check_on_click;

		#region Public Constructors
		public ToolStripButton ()
			: this (null, null, null, String.Empty)
		{
		}

		public ToolStripButton (Image image)
			: this (null, image, null, String.Empty)
		{
		}

		public ToolStripButton (string text)
			: this (text, null, null, String.Empty)
		{
		}

		public ToolStripButton (string text, Image image)
			: this (text, image, null, String.Empty)
		{
		}

		public ToolStripButton (string text, Image image, EventHandler onClick)
			: this (text, image, onClick, String.Empty)
		{
		}

		public ToolStripButton (string text, Image image, EventHandler onClick, string name)
			: base (text, image, onClick, name)
		{
			this.checked_state = CheckState.Unchecked;
			this.ToolTipText = String.Empty;
		}
		#endregion

		#region Public Properties
		[DefaultValue (true)]
		public new bool AutoToolTip {
			get { return base.AutoToolTip; }
			set { base.AutoToolTip = value; }
		}

		public override bool CanSelect {
			get { return true; }
		}

		[DefaultValue (false)]
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
				if (this.checked_state != (value ? CheckState.Checked : CheckState.Unchecked)) {
					this.checked_state = value ? CheckState.Checked : CheckState.Unchecked;
					this.OnCheckedChanged (EventArgs.Empty);
					this.OnCheckStateChanged (EventArgs.Empty);
					this.Invalidate ();
				}
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

		[DefaultValue (CheckState.Unchecked)]
		public CheckState CheckState {
			get { return this.checked_state; }
			set {
				if (this.checked_state != value) {
					if (!Enum.IsDefined (typeof (CheckState), value))
						throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for CheckState", value));

					this.checked_state = value;
					this.OnCheckedChanged (EventArgs.Empty);
					this.OnCheckStateChanged (EventArgs.Empty);
					this.Invalidate ();
				}
			}
		}
		#endregion

		#region Protected Properties
		protected override bool DefaultAutoToolTip { get { return true; } }
		#endregion

		#region Public Methods
		public override Size GetPreferredSize (Size constrainingSize)
		{
			Size retval = base.GetPreferredSize (constrainingSize);
			
			if (retval.Width < 23)
				retval.Width = 23;
				
			return retval;
		}
		#endregion

		#region Protected Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			ToolStripItemAccessibleObject ao = new ToolStripItemAccessibleObject (this);

			ao.default_action = "Press";
			ao.role = AccessibleRole.PushButton;
			ao.state = AccessibleStates.Focusable;	

			return ao;
		}

		protected virtual void OnCheckedChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [CheckedChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnCheckStateChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [CheckStateChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnClick (EventArgs e)
		{
			if (this.check_on_click)
				this.Checked = !this.Checked;

			base.OnClick (e);

			ToolStrip ts = this.GetTopLevelToolStrip ();
			
			if (ts != null)
				ts.Dismiss (ToolStripDropDownCloseReason.ItemClicked);
		}

		protected override void OnPaint (System.Windows.Forms.PaintEventArgs e)
		{
			base.OnPaint (e);

			if (this.Owner != null) {
				Color font_color = this.Enabled ? this.ForeColor : SystemColors.GrayText;
				Image draw_image = this.Enabled ? this.Image : ToolStripRenderer.CreateDisabledImage (this.Image);

				this.Owner.Renderer.DrawButtonBackground (new System.Windows.Forms.ToolStripItemRenderEventArgs (e.Graphics, this));

				Rectangle text_layout_rect;
				Rectangle image_layout_rect;

				this.CalculateTextAndImageRectangles (out text_layout_rect, out image_layout_rect);

				if (text_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, font_color, this.Font, this.TextAlign));
				if (image_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemImage (new System.Windows.Forms.ToolStripItemImageRenderEventArgs (e.Graphics, this, draw_image, image_layout_rect));

				return;
			}
		}
		#endregion

		#region Public Events
		static object CheckedChangedEvent = new object ();
		static object CheckStateChangedEvent = new object ();

		public event EventHandler CheckedChanged {
			add { Events.AddHandler (CheckedChangedEvent, value); }
			remove { Events.RemoveHandler (CheckedChangedEvent, value); }
		}

		public event EventHandler CheckStateChanged {
			add { Events.AddHandler (CheckStateChangedEvent, value); }
			remove { Events.RemoveHandler (CheckStateChangedEvent, value); }
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
	}
}
