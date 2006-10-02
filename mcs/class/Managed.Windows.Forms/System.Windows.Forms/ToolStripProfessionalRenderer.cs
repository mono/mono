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
#if NET_2_0

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	class ToolStripProfessionalRenderer : ToolStripRenderer
	{
		private ProfessionalColorTable color_table;
		private bool rounded_edges;

		#region Public Constructor
		public ToolStripProfessionalRenderer () : base ()
		{
			color_table = new ProfessionalColorTable ();
			rounded_edges = true;
		}
		#endregion

		#region Public Properties
		public ProfessionalColorTable ColorTable {
			get { return this.color_table; }
			set { this.color_table = value; }
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
			
			using (Pen p = new Pen (e.ArrowColor)) {
				int x = e.ArrowRectangle.Left + (e.ArrowRectangle.Width / 2) - 3;
				int y = e.ArrowRectangle.Top + (e.ArrowRectangle.Height / 2) - 2;

				e.Graphics.DrawLine (p, x + 1, y, x + 5, y);
				e.Graphics.DrawLine (p, x + 2, y + 1, x + 4, y + 1);
				e.Graphics.DrawLine (p, x + 3, y + 1, x + 3, y + 2);
			}
		}

		protected override void OnRenderButtonBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderButtonBackground (e);

			if (e.Item.Enabled == false)
				return;
				
			Rectangle paint_here = new Rectangle (0, 0, e.Item.Width, e.Item.Height);

			// Paint gradient background
			if (e.Item is ToolStripButton && (e.Item as ToolStripButton).Checked && !e.Item.Selected)
				using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ButtonCheckedGradientBegin, this.ColorTable.ButtonCheckedGradientEnd, LinearGradientMode.Vertical))
					e.Graphics.FillRectangle (b, paint_here);
			else 
				if (e.Item.Pressed || (e.Item is ToolStripButton && (e.Item as ToolStripButton).Checked))
					using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ButtonPressedGradientBegin, this.ColorTable.ButtonPressedGradientEnd, LinearGradientMode.Vertical))
						e.Graphics.FillRectangle (b, paint_here);
				else if (e.Item.Selected && !e.Item.Pressed)
					using (Brush b = new LinearGradientBrush (paint_here, this.ColorTable.ButtonSelectedGradientBegin, this.ColorTable.ButtonSelectedGradientEnd, LinearGradientMode.Vertical))
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
		}

		protected override void OnRenderDropDownButtonBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderDropDownButtonBackground (e);

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
		}

		protected override void OnRenderGrip (ToolStripGripRenderEventArgs e)
		{
			base.OnRenderGrip (e);

			if (e.GripStyle == ToolStripGripStyle.Hidden)
				return;

			if (e.GripDisplayStyle == ToolStripGripDisplayStyle.Vertical) {
				using (Brush light_brush = new SolidBrush (this.ColorTable.GripLight)) {
					Rectangle r = new Rectangle (e.GripBounds.Left, e.GripBounds.Top + 5, 2, 2);

					for (int i = 0; i < e.GripBounds.Height - 11; i += 4) {
						e.Graphics.FillRectangle (light_brush, r);
						r.Offset (0, 4);
					}
				}

				using (Brush dark_brush = new SolidBrush (this.ColorTable.GripDark)) {
					Rectangle r = new Rectangle (e.GripBounds.Left - 1, e.GripBounds.Top + 4, 2, 2);

					for (int i = 0; i < e.GripBounds.Height - 11; i += 4) {
						e.Graphics.FillRectangle (dark_brush, r);
						r.Offset (0, 4);
					}
				}
			}
			else {
				using (Brush light_brush = new SolidBrush (this.ColorTable.GripLight)) {
					Rectangle r = new Rectangle (e.GripBounds.Left + 5, e.GripBounds.Top, 2, 2);

					for (int i = 0; i < e.GripBounds.Width - 11; i += 4) {
						e.Graphics.FillRectangle (light_brush, r);
						r.Offset (4, 0);
					}
				}

				using (Brush dark_brush = new SolidBrush (this.ColorTable.GripDark)) {
					Rectangle r = new Rectangle (e.GripBounds.Left + 4, e.GripBounds.Top - 1, 2, 2);

					for (int i = 0; i < e.GripBounds.Width - 11; i += 4) {
						e.Graphics.FillRectangle (dark_brush, r);
						r.Offset (4, 0);
					}
				}
			}
		}

		protected override void OnRenderImageMargin (ToolStripRenderEventArgs e)
		{
			base.OnRenderImageMargin (e);
		}

		protected override void OnRenderItemCheck (ToolStripItemImageRenderEventArgs e)
		{
			base.OnRenderItemCheck (e);
		}
		
		protected override void OnRenderItemImage (ToolStripItemImageRenderEventArgs e)
		{
			base.OnRenderItemImage (e);

			e.Graphics.DrawImage (e.Image, e.ImageRectangle);
		}

		protected override void OnRenderItemText (ToolStripItemTextRenderEventArgs e)
		{
			base.OnRenderItemText (e);

			TextRenderer.DrawText (e.Graphics, e.Text, e.TextFont, e.TextRectangle, e.TextColor, e.TextFormat);
		}

		protected override void OnRenderLabelBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderLabelBackground (e);
		}

		protected override void OnRenderMenuItemBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderMenuItemBackground (e);
		}

		protected override void OnRenderOverflowButtonBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderOverflowButtonBackground (e);
		}

		protected override void OnRenderSeparator (ToolStripSeparatorRenderEventArgs e)
		{
			base.OnRenderSeparator (e);

			if (e.Vertical) {
				using (Brush light_brush = new SolidBrush (this.ColorTable.SeparatorLight)) {
					Rectangle r = new Rectangle (4, 6, 1, e.Item.Height - 10);
					e.Graphics.FillRectangle (light_brush, r);
				}

				using (Brush dark_brush = new SolidBrush (this.ColorTable.SeparatorDark)) {
					Rectangle r = new Rectangle (3, 5, 1, e.Item.Height - 10);
					e.Graphics.FillRectangle (dark_brush, r);
				}
			}
			else {
				using (Brush light_brush = new SolidBrush (this.ColorTable.SeparatorLight)) {
					Rectangle r = new Rectangle (6, 4, e.Item.Width - 10, 1);
					e.Graphics.FillRectangle (light_brush, r);
				}

				using (Brush dark_brush = new SolidBrush (this.ColorTable.SeparatorDark)) {
					Rectangle r = new Rectangle (5, 3, e.Item.Width - 10, 1);
					e.Graphics.FillRectangle (dark_brush, r);
				}
			}
		}

		protected override void OnRenderSplitButtonBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderSplitButtonBackground (e);
		}

		protected override void OnRenderToolStripBackground (ToolStripRenderEventArgs e)
		{
			base.OnRenderToolStripBackground (e);

			//if (e.ToolStrip is ToolStripDropDown) {
			//        e.Graphics.Clear (this.ColorTable.ToolStripDropDownBackground);
			//        return;
			//}
			
			using (LinearGradientBrush b = new LinearGradientBrush (e.AffectedBounds, this.ColorTable.ToolStripGradientBegin, this.ColorTable.ToolStripGradientEnd, e.ToolStrip.Orientation == Orientation.Vertical ? LinearGradientMode.Horizontal : LinearGradientMode.Vertical))
				e.Graphics.FillRectangle (b, e.AffectedBounds);
		}

		protected override void OnRenderToolStripBorder (ToolStripRenderEventArgs e)
		{
			base.OnRenderToolStripBorder (e);

			//if (e.ToolStrip is ToolStripDropDown) {
			//        e.Graphics.DrawLines (new Pen (this.ColorTable.ToolStripBorder), new Point[] { e.AffectedBounds.Location, new Point (e.AffectedBounds.Left, e.AffectedBounds.Bottom - 1), new Point (e.AffectedBounds.Right - 1, e.AffectedBounds.Bottom - 1), new Point (e.AffectedBounds.Right - 1, e.AffectedBounds.Top), new Point (e.AffectedBounds.Left + e.ConnectedArea.Right, e.AffectedBounds.Top) });
			//        return;
			//}

			using (Pen p = new Pen (this.ColorTable.ToolStripBorder)) {
				if (this.RoundedEdges == true) {
					e.Graphics.DrawLine (p, new Point (e.ToolStrip.Left + 2, e.ToolStrip.Bottom - 1), new Point (e.ToolStrip.Right - 3, e.ToolStrip.Bottom - 1));
					e.Graphics.DrawLine (p, new Point (e.ToolStrip.Right - 2, e.ToolStrip.Bottom - 2), new Point (e.ToolStrip.Right - 1, e.ToolStrip.Bottom - 2));
					e.Graphics.DrawLine (p, new Point (e.ToolStrip.Right - 1, e.ToolStrip.Top + 2), new Point (e.ToolStrip.Right - 1, e.ToolStrip.Bottom - 3));
				}
				else
					e.Graphics.DrawLine (p, new Point (e.ToolStrip.Left, e.ToolStrip.Bottom - 1), new Point (e.ToolStrip.Width, e.ToolStrip.Bottom - 1));
			}
		}
		
		protected override void OnRenderToolStripContentPanelBackground (ToolStripContentPanelRenderEventArgs e)
		{
			base.OnRenderToolStripContentPanelBackground (e);
		}
		
		protected override void OnRenderToolStripPanelBackground (ToolStripPanelRenderEventArgs e)
		{
			base.OnRenderToolStripPanelBackground (e);
		}

		protected override void OnRenderToolStripStatusLabelBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderToolStripStatusLabelBackground (e);
		}
		#endregion
	}
}
#endif