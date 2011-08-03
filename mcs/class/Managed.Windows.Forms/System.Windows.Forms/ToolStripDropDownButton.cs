//
// ToolStripDropDownButton.cs
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

using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
using System.ComponentModel;

namespace System.Windows.Forms
{
	[ToolStripItemDesignerAvailability (ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
	public class ToolStripDropDownButton : ToolStripDropDownItem
	{
		private bool show_drop_down_arrow = true;

		#region Public Constructors
		public ToolStripDropDownButton()
			: this (string.Empty, null, null, string.Empty)
		{
		}
		
		public ToolStripDropDownButton (Image image)
			: this (string.Empty, image, null, string.Empty)
		{
		}
		
		public ToolStripDropDownButton (string text)
			: this (text, null, null, string.Empty)
		{
		}
		
		public ToolStripDropDownButton (string text, Image image)
			: this (text, image, null, string.Empty)
		{
		}
		
		public ToolStripDropDownButton (string text, Image image, EventHandler onClick)
			: this (text, image, onClick, string.Empty)
		{
		}
		
		public ToolStripDropDownButton (string text, Image image, params ToolStripItem[] dropDownItems)
			: base (text, image, dropDownItems)
		{
		}
		
		public ToolStripDropDownButton (string text, Image image, EventHandler onClick, string name)
			: base (text, image, onClick, name)
		{
		}
		#endregion

		#region Public Properties
		[DefaultValue (true)]
		public new bool AutoToolTip {
			get { return base.AutoToolTip; }
			set { base.AutoToolTip = value; }
		}
		
		[DefaultValue (true)]
		public bool ShowDropDownArrow {
			get { return this.show_drop_down_arrow; }
			set { 
				if (this.show_drop_down_arrow != value) {
					this.show_drop_down_arrow = value;
					CalculateAutoSize ();
				}
			}
		}
		#endregion

		#region Protected Properties
		protected override bool DefaultAutoToolTip {
			get { return true; }
		}
		#endregion

		#region Protected Methods
		protected override ToolStripDropDown CreateDefaultDropDown ()
		{
			ToolStripDropDownMenu tsdd = new ToolStripDropDownMenu ();
			tsdd.OwnerItem = this;
			return tsdd;
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) {
				if (this.DropDown.Visible)
					this.HideDropDown (ToolStripDropDownCloseReason.ItemClicked);
				else
					this.ShowDropDown ();
			}
			
			base.OnMouseDown (e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseUp (e);
		}
		
		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);

			if (this.Owner != null) {
				Color font_color = this.Enabled ? this.ForeColor : SystemColors.GrayText;
				Image draw_image = this.Enabled ? this.Image : ToolStripRenderer.CreateDisabledImage (this.Image);

				this.Owner.Renderer.DrawDropDownButtonBackground (new System.Windows.Forms.ToolStripItemRenderEventArgs (e.Graphics, this));

				Rectangle text_layout_rect;
				Rectangle image_layout_rect;

				this.CalculateTextAndImageRectangles (out text_layout_rect, out image_layout_rect);

				if (text_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemText (new System.Windows.Forms.ToolStripItemTextRenderEventArgs (e.Graphics, this, this.Text, text_layout_rect, font_color, this.Font, this.TextAlign));
				if (image_layout_rect != Rectangle.Empty)
					this.Owner.Renderer.DrawItemImage (new System.Windows.Forms.ToolStripItemImageRenderEventArgs (e.Graphics, this, draw_image, image_layout_rect));
				if (this.ShowDropDownArrow)
					this.Owner.Renderer.DrawArrow (new ToolStripArrowRenderEventArgs (e.Graphics, this, new Rectangle (this.Width - 10, 0, 6, this.Height), Color.Black, ArrowDirection.Down));
				return;
			}
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
		internal override Size CalculatePreferredSize (Size constrainingSize)
		{
			Size preferred_size = base.CalculatePreferredSize (constrainingSize);

			if (this.ShowDropDownArrow)
				preferred_size.Width += 9;

			return preferred_size;
		}
		#endregion
	}
}
