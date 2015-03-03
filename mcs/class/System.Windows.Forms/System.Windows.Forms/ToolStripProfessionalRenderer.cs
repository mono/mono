//
// ToolStripProfessionalRenderer.cs
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace System.Windows.Forms
{
	public class ToolStripProfessionalRenderer : ToolStripRenderer
	{
		private ProfessionalColorTable color_table;
		private bool rounded_edges;

		#region Public Constructor
		public ToolStripProfessionalRenderer () : this (new ProfessionalColorTable ())
		{
		}
		
		public ToolStripProfessionalRenderer (ProfessionalColorTable professionalColorTable) : base ()
		{
			color_table = professionalColorTable;
			rounded_edges = true;
		}
		#endregion

		#region Public Properties
		public ProfessionalColorTable ColorTable {
			get { return this.color_table; }
		}

		public bool RoundedEdges {
			get { return this.rounded_edges; }
			set { this.rounded_edges = value; }
		}
		#endregion

		#region Protected Methods
		protected override void OnRenderArrow (ToolStripArrowRenderEventArgs e)
		{
			base.OnRenderArrow (e);
		}

		protected override void OnRenderButtonBackground (ToolStripItemRenderEventArgs e)
		{
			if (e.Item.Enabled == false)
				return;
				
			Rectangle paint_here = new Rectangle (0, 0, e.Item.Width, e.Item.Height);

			// Paint gradient background
			if (e.Item is ToolStripButton && (e.Item as ToolStripButton).Checked && !e.Item.Selected) {
				if (this.ColorTable.UseSystemColors)
					e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.ButtonCheckedHighlight), paint_here);
				else
					using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ButtonCheckedGradientBegin, this.ColorTable.ButtonCheckedGradientEnd, LinearGradientMode.Vertical))
					e.Graphics.FillRectangle (b, paint_here); }
			else
				if (e.Item is ToolStripDropDownItem && e.Item.Pressed) // || (e.Item as ToolStripMenuItem).DropDown.Visible == true))
					using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ToolStripGradientBegin, this.ColorTable.ToolStripGradientEnd, LinearGradientMode.Vertical))
						e.Graphics.FillRectangle (b, paint_here);
				else if (e.Item.Pressed || (e.Item is ToolStripButton && (e.Item as ToolStripButton).Checked))
					using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ButtonPressedGradientBegin, this.ColorTable.ButtonPressedGradientEnd, LinearGradientMode.Vertical))
						e.Graphics.FillRectangle (b, paint_here);
				else if (e.Item.Selected) // && !e.Item.Pressed)
					using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ButtonSelectedGradientBegin, this.ColorTable.ButtonSelectedGradientEnd, LinearGradientMode.Vertical))
						e.Graphics.FillRectangle (b, paint_here);
				else if (e.Item.BackColor != Control.DefaultBackColor && e.Item.BackColor != Color.Empty)
					using (Brush b = new SolidBrush (e.Item.BackColor))
						e.Graphics.FillRectangle (b, paint_here);

			paint_here.Width -= 1;
			paint_here.Height -= 1;

			// Paint border
			if (e.Item.Selected && !e.Item.Pressed)
				using (Pen p = new Pen (this.ColorTable.ButtonSelectedBorder))
					e.Graphics.DrawRectangle (p, paint_here);
			else if (e.Item.Pressed)
				using (Pen p = new Pen (this.ColorTable.ButtonPressedBorder))
					e.Graphics.DrawRectangle (p, paint_here);
			else if (e.Item is ToolStripButton && (e.Item as ToolStripButton).Checked)
				using (Pen p = new Pen (this.ColorTable.ButtonPressedBorder))
					e.Graphics.DrawRectangle (p, paint_here);

			base.OnRenderButtonBackground (e);
		}

		protected override void OnRenderDropDownButtonBackground (ToolStripItemRenderEventArgs e)
		{
			Rectangle paint_here = new Rectangle (0, 0, e.Item.Width, e.Item.Height);

			// Paint gradient background
			if (e.Item.Selected && !e.Item.Pressed)
				using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ButtonSelectedGradientBegin, this.ColorTable.ButtonSelectedGradientEnd, LinearGradientMode.Vertical))
					e.Graphics.FillRectangle (b, paint_here);
			else if (e.Item.Pressed)
				using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ImageMarginGradientMiddle, this.ColorTable.ImageMarginGradientEnd, LinearGradientMode.Vertical))
					e.Graphics.FillRectangle (b, paint_here);

			paint_here.Width -= 1;
			paint_here.Height -= 1;

			// Paint border
			if (e.Item.Selected && !e.Item.Pressed)
				using (Pen p = new Pen (this.ColorTable.ButtonSelectedBorder))
					e.Graphics.DrawRectangle (p, paint_here);
			else if (e.Item.Pressed)
				using (Pen p = new Pen (this.ColorTable.MenuBorder))
					e.Graphics.DrawRectangle (p, paint_here);

			base.OnRenderDropDownButtonBackground (e);
		}

		protected override void OnRenderGrip (ToolStripGripRenderEventArgs e)
		{
			if (e.GripStyle == ToolStripGripStyle.Hidden)
				return;

			if (e.GripDisplayStyle == ToolStripGripDisplayStyle.Vertical) {
				Rectangle r = new Rectangle (e.GripBounds.Left, e.GripBounds.Top + 5, 2, 2);

				for (int i = 0; i < e.GripBounds.Height - 12; i += 4) {
					e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.GripLight), r);
					r.Offset (0, 4);
			}

				Rectangle r2 = new Rectangle (e.GripBounds.Left - 1, e.GripBounds.Top + 4, 2, 2);

				for (int i = 0; i < e.GripBounds.Height - 12; i += 4) {
					e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.GripDark), r2);
					r2.Offset (0, 4);
				}
			}
			else {
				Rectangle r = new Rectangle (e.GripBounds.Left + 5, e.GripBounds.Top, 2, 2);

				for (int i = 0; i < e.GripBounds.Width - 11; i += 4) {
					e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.GripLight), r);
					r.Offset (4, 0);
				}

				Rectangle r2 = new Rectangle (e.GripBounds.Left + 4, e.GripBounds.Top - 1, 2, 2);

				for (int i = 0; i < e.GripBounds.Width - 11; i += 4) {
					e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.GripDark), r2);
					r2.Offset (4, 0);
				}
			}

			base.OnRenderGrip (e);
		}

		protected override void OnRenderImageMargin (ToolStripRenderEventArgs e)
		{
			if (!(e.ToolStrip is ToolStripOverflow)) {
				Rectangle side_gradient = new Rectangle (1, 2, 24, e.ToolStrip.Height - 3);
				using (LinearGradientBrush b = new LinearGradientBrush (side_gradient, this.ColorTable.ToolStripGradientBegin, this.ColorTable.ToolStripGradientEnd, LinearGradientMode.Horizontal))
					e.Graphics.FillRectangle (b, side_gradient);
			}

			base.OnRenderImageMargin (e);
		}

		protected override void OnRenderItemCheck (ToolStripItemImageRenderEventArgs e)
		{
			if (e.Item.Selected)
			{
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.CheckPressedBackground), e.ImageRectangle);
				e.Graphics.DrawRectangle (ThemeEngine.Current.ResPool.GetPen (this.ColorTable.ButtonPressedBorder), e.ImageRectangle);
			}
			else if (e.Item.Pressed)
			{
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.CheckSelectedBackground), e.ImageRectangle);
				e.Graphics.DrawRectangle (ThemeEngine.Current.ResPool.GetPen (this.ColorTable.ButtonSelectedBorder), e.ImageRectangle);
			}
			else
			{
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.CheckSelectedBackground), e.ImageRectangle);
				e.Graphics.DrawRectangle (ThemeEngine.Current.ResPool.GetPen (this.ColorTable.ButtonSelectedBorder), e.ImageRectangle);
			}
			if (e.Item.Image == null)
				ControlPaint.DrawMenuGlyph(e.Graphics, new Rectangle (6,5,7,6), MenuGlyph.Checkmark);
			
			base.OnRenderItemCheck (e);
		}
		
		protected override void OnRenderItemImage (ToolStripItemImageRenderEventArgs e)
		{
			base.OnRenderItemImage (e);
		}

		protected override void OnRenderItemText (ToolStripItemTextRenderEventArgs e)
		{
			base.OnRenderItemText (e);
		}

		protected override void OnRenderLabelBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderLabelBackground (e);
		}

		protected override void OnRenderMenuItemBackground (ToolStripItemRenderEventArgs e)
		{
			ToolStripMenuItem tsmi = (ToolStripMenuItem)e.Item;
			Rectangle paint_here;

			if (tsmi.IsOnDropDown) {
				paint_here = new Rectangle (1, 0, e.Item.Bounds.Width - 3, e.Item.Bounds.Height - 1);
				
				if (e.Item.Selected || e.Item.Pressed)
					if (e.Item.Enabled)
						e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.MenuItemSelectedGradientEnd), paint_here);

				if (tsmi.Selected || tsmi.Pressed)
					using (Pen p = new Pen (this.ColorTable.MenuItemBorder))
						e.Graphics.DrawRectangle (p, paint_here);
			}
			else {
				paint_here = new Rectangle (0, 0, e.Item.Width, e.Item.Height);

				if (e.Item.Pressed)
					using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ToolStripGradientBegin, this.ColorTable.ToolStripGradientEnd, LinearGradientMode.Vertical))
						e.Graphics.FillRectangle (b, paint_here);
				else if (e.Item.Selected)
					using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ButtonSelectedGradientBegin, this.ColorTable.ButtonSelectedGradientEnd, LinearGradientMode.Vertical))
						e.Graphics.FillRectangle (b, paint_here);
				else if (e.Item.BackColor != Control.DefaultBackColor && e.Item.BackColor != Color.Empty)
					using (Brush b = new SolidBrush (e.Item.BackColor))
						e.Graphics.FillRectangle (b, paint_here);

				paint_here.Width -= 1;
				paint_here.Height -= 1;

				if (tsmi.Selected || tsmi.Pressed) {
					if (tsmi.HasDropDownItems && tsmi.DropDown.Visible)
						using (Pen p = new Pen (this.ColorTable.MenuBorder))
							e.Graphics.DrawRectangle (p, paint_here);				
					else
						using (Pen p = new Pen (this.ColorTable.MenuItemBorder))
							e.Graphics.DrawRectangle (p, paint_here);
				}
			}
			
			base.OnRenderMenuItemBackground (e);
		}

		protected override void OnRenderOverflowButtonBackground (ToolStripItemRenderEventArgs e)
		{
			Rectangle paint_here;
			LinearGradientMode gradient_direction = e.ToolStrip.Orientation == Orientation.Vertical ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical;
			
			if (e.ToolStrip.Orientation == Orientation.Horizontal)
				paint_here = new Rectangle (e.Item.Width - 11, 0, 11, e.Item.Height - 1);
			else
				paint_here = new Rectangle (0, e.Item.Height - 11, e.Item.Width - 1, 11);
			
			// Paint gradient background
			if (e.Item.Selected && !e.Item.Pressed)
				using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ButtonSelectedGradientBegin, this.ColorTable.ButtonSelectedGradientEnd, gradient_direction))
					e.Graphics.FillRectangle (b, paint_here);
			else if (e.Item.Pressed)
				using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ButtonPressedGradientBegin, this.ColorTable.ButtonPressedGradientEnd, gradient_direction))
					e.Graphics.FillRectangle (b, paint_here);
			else
				using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.OverflowButtonGradientBegin, this.ColorTable.OverflowButtonGradientEnd, gradient_direction))
					e.Graphics.FillRectangle (b, paint_here);

			// Paint the arrow
			PaintOverflowArrow (e, paint_here);

			base.OnRenderOverflowButtonBackground (e);
		}

		protected override void OnRenderSeparator (ToolStripSeparatorRenderEventArgs e)
		{
			if (e.Vertical) {
				Rectangle r = new Rectangle (4, 6, 1, e.Item.Height - 10);
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.SeparatorLight), r);

				Rectangle r2 = new Rectangle (3, 5, 1, e.Item.Height - 10);
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.SeparatorDark), r2);
			}
			else {
				if (!e.Item.IsOnDropDown) {
					Rectangle r = new Rectangle (6, 4, e.Item.Width - 10, 1);
					e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.SeparatorLight), r);
				}

				Rectangle r3;
				if (e.Item.IsOnDropDown) {
					if (e.Item.UseImageMargin)
						r3 = new Rectangle (35, 3, e.Item.Width - 36, 1);
					else
						r3 = new Rectangle (7, 3, e.Item.Width - 7, 1);
				} else
					r3 = new Rectangle (5, 3, e.Item.Width - 10, 1);
					
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (this.ColorTable.SeparatorDark), r3);
			}

			base.OnRenderSeparator (e);
		}

		protected override void OnRenderSplitButtonBackground (ToolStripItemRenderEventArgs e)
		{
			ToolStripSplitButton tssb = (ToolStripSplitButton)e.Item;
			Rectangle paint_here = new Rectangle (0, 0, tssb.Width, tssb.Height);
			Rectangle button_part = new Rectangle (0, 0, tssb.ButtonBounds.Width, tssb.ButtonBounds.Height);

			// Paint gradient background
			if (tssb.ButtonSelected && !tssb.DropDownButtonPressed)
				using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ButtonSelectedGradientBegin, this.ColorTable.ButtonSelectedGradientEnd, LinearGradientMode.Vertical))
					e.Graphics.FillRectangle (b, paint_here);
			if (tssb.ButtonPressed)
				using (Brush b = new LinearGradientBrush (button_part, this.ColorTable.ButtonPressedGradientBegin, this.ColorTable.ButtonPressedGradientEnd, LinearGradientMode.Vertical))
					e.Graphics.FillRectangle (b, button_part);

			paint_here.Width -= 1;
			paint_here.Height -= 1;

			// Paint border
			if (e.Item.Selected && !tssb.DropDownButtonPressed)
				using (Pen p = new Pen (this.ColorTable.ButtonSelectedBorder))
				{
					e.Graphics.DrawRectangle (p, paint_here);
					e.Graphics.DrawLine (p, button_part.Right, 0, button_part.Right, button_part.Height);
				}
			else if (e.Item.Pressed)
				using (Pen p = new Pen (this.ColorTable.MenuBorder))
					e.Graphics.DrawRectangle (p, paint_here);

			base.OnRenderSplitButtonBackground (e);
		}

		protected override void OnRenderToolStripBackground (ToolStripRenderEventArgs e)
		{
			// Don't clear and fill the background if we already painted an image there
			if (e.ToolStrip.BackgroundImage != null) {
				if (e.ToolStrip is StatusStrip)
					e.Graphics.DrawLine (Pens.White, e.AffectedBounds.Left, e.AffectedBounds.Top, e.AffectedBounds.Right, e.AffectedBounds.Top);
			
				return;
			}
				
			if (e.ToolStrip is ToolStripDropDown) {
				e.Graphics.Clear (this.ColorTable.ToolStripDropDownBackground);
				return;
			}
			
			if (e.ToolStrip is MenuStrip || e.ToolStrip is StatusStrip) {
				using (LinearGradientBrush b = new LinearGradientBrush (e.AffectedBounds, this.ColorTable.MenuStripGradientBegin, this.ColorTable.MenuStripGradientEnd, e.ToolStrip.Orientation == Orientation.Horizontal ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical))
					e.Graphics.FillRectangle (b, e.AffectedBounds);
			}
			else
				using (LinearGradientBrush b = new LinearGradientBrush (e.AffectedBounds, this.ColorTable.ToolStripGradientBegin, this.ColorTable.ToolStripGradientEnd, e.ToolStrip.Orientation == Orientation.Vertical ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical))
					e.Graphics.FillRectangle (b, e.AffectedBounds);
					
			if (e.ToolStrip is StatusStrip)
				e.Graphics.DrawLine (Pens.White, e.AffectedBounds.Left, e.AffectedBounds.Top, e.AffectedBounds.Right, e.AffectedBounds.Top);

			base.OnRenderToolStripBackground (e);
		}

		protected override void OnRenderToolStripBorder (ToolStripRenderEventArgs e)
		{
			if (e.ToolStrip is ToolStripDropDown) {
				if (e.ToolStrip is ToolStripOverflow)
					e.Graphics.DrawLines (ThemeEngine.Current.ResPool.GetPen (this.ColorTable.MenuBorder), new Point[] { e.AffectedBounds.Location, new Point (e.AffectedBounds.Left, e.AffectedBounds.Bottom - 1), new Point (e.AffectedBounds.Right - 1, e.AffectedBounds.Bottom - 1), new Point (e.AffectedBounds.Right - 1, e.AffectedBounds.Top), new Point (e.AffectedBounds.Left, e.AffectedBounds.Top) });
				else
					e.Graphics.DrawLines (ThemeEngine.Current.ResPool.GetPen (this.ColorTable.MenuBorder), new Point[] { new Point (e.AffectedBounds.Left + e.ConnectedArea.Left, e.AffectedBounds.Top), e.AffectedBounds.Location, new Point (e.AffectedBounds.Left, e.AffectedBounds.Bottom - 1), new Point (e.AffectedBounds.Right - 1, e.AffectedBounds.Bottom - 1), new Point (e.AffectedBounds.Right - 1, e.AffectedBounds.Top), new Point (e.AffectedBounds.Left + e.ConnectedArea.Right, e.AffectedBounds.Top) });
				return;
			}

			if (e.ToolStrip is MenuStrip || e.ToolStrip is StatusStrip)
				return;
				
			using (Pen p = new Pen (this.ColorTable.ToolStripBorder)) {
				if (this.RoundedEdges == true) {
					e.Graphics.DrawLine (p, new Point (2, e.ToolStrip.Height - 1), new Point (e.ToolStrip.Width - 3, e.ToolStrip.Height - 1));
					e.Graphics.DrawLine (p, new Point (e.ToolStrip.Width - 2, e.ToolStrip.Height - 2), new Point (e.ToolStrip.Width - 1, e.ToolStrip.Height - 2));
					e.Graphics.DrawLine (p, new Point (e.ToolStrip.Width - 1, 2), new Point (e.ToolStrip.Width - 1, e.ToolStrip.Height - 3));
				}
				else
					e.Graphics.DrawLine (p, new Point (e.ToolStrip.Left, e.ToolStrip.Bottom - 1), new Point (e.ToolStrip.Width, e.ToolStrip.Bottom - 1));
			}

			base.OnRenderToolStripBorder (e);
		}
		
		protected override void OnRenderToolStripContentPanelBackground (ToolStripContentPanelRenderEventArgs e)
		{
			base.OnRenderToolStripContentPanelBackground (e);
		}
		
		protected override void OnRenderToolStripPanelBackground (ToolStripPanelRenderEventArgs e)
		{
			using (LinearGradientBrush b = new LinearGradientBrush (e.ToolStripPanel.Bounds, this.ColorTable.MenuStripGradientBegin, this.ColorTable.MenuStripGradientEnd, e.ToolStripPanel.Orientation == Orientation.Horizontal ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical))
				e.Graphics.FillRectangle (b, e.ToolStripPanel.Bounds);

			base.OnRenderToolStripPanelBackground (e);
		}

		protected override void OnRenderToolStripStatusLabelBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderToolStripStatusLabelBackground (e);
		}
		#endregion

		#region Private Methods
		private static void PaintOverflowArrow (ToolStripItemRenderEventArgs e, Rectangle paint_here)
		{
			if (e.ToolStrip.Orientation == Orientation.Horizontal) {
				// Paint down arrow
				Point arrow_loc = new Point (paint_here.X + 2, paint_here.Bottom - 9);

				e.Graphics.DrawLine (Pens.White, arrow_loc.X + 1, arrow_loc.Y + 1, arrow_loc.X + 5, arrow_loc.Y + 1);
				e.Graphics.DrawLine (Pens.Black, arrow_loc.X, arrow_loc.Y, arrow_loc.X + 4, arrow_loc.Y);

				e.Graphics.DrawLine (Pens.White, arrow_loc.X + 3, arrow_loc.Y + 4, arrow_loc.X + 5, arrow_loc.Y + 4);
				e.Graphics.DrawLine (Pens.White, arrow_loc.X + 3, arrow_loc.Y + 5, arrow_loc.X + 4, arrow_loc.Y + 5);
				e.Graphics.DrawLine (Pens.White, arrow_loc.X + 3, arrow_loc.Y + 4, arrow_loc.X + 3, arrow_loc.Y + 6);

				e.Graphics.DrawLine (Pens.Black, arrow_loc.X, arrow_loc.Y + 3, arrow_loc.X + 4, arrow_loc.Y + 3);
				e.Graphics.DrawLine (Pens.Black, arrow_loc.X + 1, arrow_loc.Y + 4, arrow_loc.X + 3, arrow_loc.Y + 4);
				e.Graphics.DrawLine (Pens.Black, arrow_loc.X + 2, arrow_loc.Y + 4, arrow_loc.X + 2, arrow_loc.Y + 5);
			} else {
				Point arrow_loc = new Point (paint_here.Right - 9, paint_here.Y + 2);

				e.Graphics.DrawLine (Pens.White, arrow_loc.X + 1, arrow_loc.Y + 1, arrow_loc.X + 1, arrow_loc.Y + 5);
				e.Graphics.DrawLine (Pens.Black, arrow_loc.X, arrow_loc.Y, arrow_loc.X, arrow_loc.Y + 4);

				e.Graphics.DrawLine (Pens.White, arrow_loc.X + 4, arrow_loc.Y + 3, arrow_loc.X + 4, arrow_loc.Y + 5);
				e.Graphics.DrawLine (Pens.White, arrow_loc.X + 5, arrow_loc.Y + 3, arrow_loc.X + 5, arrow_loc.Y + 4);
				e.Graphics.DrawLine (Pens.White, arrow_loc.X + 4, arrow_loc.Y + 3, arrow_loc.X + 6, arrow_loc.Y + 3);

				e.Graphics.DrawLine (Pens.Black, arrow_loc.X + 3, arrow_loc.Y, arrow_loc.X + 3, arrow_loc.Y + 4);
				e.Graphics.DrawLine (Pens.Black, arrow_loc.X + 4, arrow_loc.Y + 1, arrow_loc.X + 4, arrow_loc.Y + 3);
				e.Graphics.DrawLine (Pens.Black, arrow_loc.X + 4, arrow_loc.Y + 2, arrow_loc.X + 5, arrow_loc.Y + 2);
			}
		}
		#endregion
	}
}
