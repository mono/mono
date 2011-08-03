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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)

using System;
using System.Drawing;

namespace System.Windows.Forms.Theming.Default
{
	internal class ToolStripPainter
	{
		public ToolStripPainter ()
		{
		}

		protected SystemResPool ResPool { get { return ThemeEngine.Current.ResPool; } }
		
		#region Painting
		public virtual void OnRenderButtonBackground (ToolStripItemRenderEventArgs e)
		{
			if (e.Item.Enabled == false)
				return;

			Rectangle paint_here = new Rectangle (0, 0, e.Item.Width, e.Item.Height);
			
			ToolStripButton tsb = e.Item as ToolStripButton;
			
			if (e.Item.Pressed || (tsb != null && tsb.Checked))
				ControlPaint.DrawBorder3D (e.Graphics, paint_here, Border3DStyle.SunkenOuter);
			else if (e.Item.Selected)
				ControlPaint.DrawBorder3D (e.Graphics, paint_here, Border3DStyle.RaisedInner);
			else if (e.Item.BackColor != Control.DefaultBackColor && e.Item.BackColor != Color.Empty)
				e.Graphics.FillRectangle (ResPool.GetSolidBrush (e.Item.BackColor), paint_here);
		}

		public virtual void OnRenderDropDownButtonBackground (ToolStripItemRenderEventArgs e)
		{
			if (e.Item.Enabled == false)
				return;

			Rectangle paint_here = new Rectangle (0, 0, e.Item.Width, e.Item.Height);

			if (e.Item.Pressed)
				ControlPaint.DrawBorder3D (e.Graphics, paint_here, Border3DStyle.SunkenOuter);
			else if (e.Item.Selected)
				ControlPaint.DrawBorder3D (e.Graphics, paint_here, Border3DStyle.RaisedInner);
			else if (e.Item.BackColor != Control.DefaultBackColor && e.Item.BackColor != Color.Empty)
				e.Graphics.FillRectangle (ResPool.GetSolidBrush (e.Item.BackColor), paint_here);
		}

		public virtual void OnRenderGrip (ToolStripGripRenderEventArgs e)
		{
			if (e.GripStyle == ToolStripGripStyle.Hidden)
				return;

			if (e.GripDisplayStyle == ToolStripGripDisplayStyle.Vertical) {
				e.Graphics.DrawLine (Pens.White, 0, 2, 1, 2);
				e.Graphics.DrawLine (Pens.White, 0, 2, 0, e.GripBounds.Height - 3);
				e.Graphics.DrawLine (SystemPens.ControlDark, 2, 2, 2, e.GripBounds.Height - 3);
				e.Graphics.DrawLine (SystemPens.ControlDark, 2, e.GripBounds.Height - 3, 0, e.GripBounds.Height - 3);
			}
			else {
				e.Graphics.DrawLine (Pens.White, 2, 0, e.GripBounds.Width - 3, 0);
				e.Graphics.DrawLine (Pens.White, 2, 0, 2, 1);
				e.Graphics.DrawLine (SystemPens.ControlDark, e.GripBounds.Width - 3, 0, e.GripBounds.Width - 3, 2);
				e.Graphics.DrawLine (SystemPens.ControlDark, 2, 2, e.GripBounds.Width - 3, 2);
			}
		}

		public virtual void OnRenderMenuItemBackground (ToolStripItemRenderEventArgs e)
		{
			ToolStripMenuItem tsmi = (ToolStripMenuItem)e.Item;
			Rectangle paint_here = new Rectangle (Point.Empty, tsmi.Size);
			
			if (tsmi.IsOnDropDown) {
				// Drop down menu item
				if (e.Item.Selected || e.Item.Pressed)
					e.Graphics.FillRectangle (SystemBrushes.Highlight, paint_here);
			} else {
				// Top level menu item
				if (e.Item.Pressed)
					ControlPaint.DrawBorder3D (e.Graphics, paint_here, Border3DStyle.SunkenOuter);
				else if (e.Item.Selected)
					ControlPaint.DrawBorder3D (e.Graphics, paint_here, Border3DStyle.RaisedInner);
				else if (e.Item.BackColor != Control.DefaultBackColor && e.Item.BackColor != Color.Empty)
					e.Graphics.FillRectangle (ResPool.GetSolidBrush (e.Item.BackColor), paint_here);
			}
		}

		public virtual void OnRenderOverflowButtonBackground (ToolStripItemRenderEventArgs e)
		{
			Rectangle paint_here = new Rectangle (Point.Empty, e.Item.Size);

			if (e.Item.Pressed)
				ControlPaint.DrawBorder3D (e.Graphics, paint_here, Border3DStyle.SunkenOuter);
			else if (e.Item.Selected)
				ControlPaint.DrawBorder3D (e.Graphics, paint_here, Border3DStyle.RaisedInner);
			else if (e.Item.BackColor != Control.DefaultBackColor && e.Item.BackColor != Color.Empty)
				e.Graphics.FillRectangle (ResPool.GetSolidBrush (e.Item.BackColor), paint_here);

			// Paint the arrow
			ToolStripRenderer.DrawDownArrow (e.Graphics, SystemPens.ControlText, e.Item.Width / 2 - 3, e.Item.Height / 2 - 1);
		}

		public virtual void OnRenderSeparator (ToolStripSeparatorRenderEventArgs e)
		{
			if (e.Vertical) {
				e.Graphics.DrawLine (Pens.White, 4, 3, 4, e.Item.Height - 1);
				e.Graphics.DrawLine (SystemPens.ControlDark, 3, 3, 3, e.Item.Height - 1);
			} else {
				if (!e.Item.IsOnDropDown) {
					e.Graphics.DrawLine (Pens.White, 2, 4, e.Item.Right - 1, 4);
					e.Graphics.DrawLine (SystemPens.ControlDark, 2, 3, e.Item.Right - 1, 3);
				} else {
					e.Graphics.DrawLine (Pens.White, 3, 4, e.Item.Right - 4, 4);
					e.Graphics.DrawLine (SystemPens.ControlDark, 3, 3, e.Item.Right - 4, 3);
				}
			}
		}
		
		public virtual void OnRenderSplitButtonBackground (ToolStripItemRenderEventArgs e)
		{
			ToolStripSplitButton tssb = (ToolStripSplitButton)e.Item;

			Rectangle button_part = new Rectangle (Point.Empty, tssb.ButtonBounds.Size);
			Point drop_start = new Point (tssb.Width - tssb.DropDownButtonBounds.Width, 0);
			Rectangle drop_part = new Rectangle (drop_start, tssb.DropDownButtonBounds.Size);

			// Regular button part
			if (tssb.ButtonPressed)
				ControlPaint.DrawBorder3D (e.Graphics, button_part, Border3DStyle.SunkenOuter);
			else if (tssb.ButtonSelected)
				ControlPaint.DrawBorder3D (e.Graphics, button_part, Border3DStyle.RaisedInner);
			else if (e.Item.BackColor != Control.DefaultBackColor && e.Item.BackColor != Color.Empty)
				e.Graphics.FillRectangle (ResPool.GetSolidBrush (e.Item.BackColor), button_part);

			// Drop down button part
			if (tssb.DropDownButtonPressed || tssb.ButtonPressed)
				ControlPaint.DrawBorder3D (e.Graphics, drop_part, Border3DStyle.SunkenOuter);
			else if (tssb.DropDownButtonSelected || tssb.ButtonSelected)
				ControlPaint.DrawBorder3D (e.Graphics, drop_part, Border3DStyle.RaisedInner);
			else if (e.Item.BackColor != Control.DefaultBackColor && e.Item.BackColor != Color.Empty)
				e.Graphics.FillRectangle (ResPool.GetSolidBrush (e.Item.BackColor), drop_part);
		}

		public virtual void OnRenderToolStripBackground (ToolStripRenderEventArgs e)
		{
			if (e.ToolStrip.BackgroundImage == null)
				e.Graphics.Clear (e.BackColor);

			if (e.ToolStrip is StatusStrip)
				e.Graphics.DrawLine (Pens.White, e.AffectedBounds.Left, e.AffectedBounds.Top, e.AffectedBounds.Right, e.AffectedBounds.Top);
		}

		public virtual void OnRenderToolStripBorder (ToolStripRenderEventArgs e)
		{
			if (e.ToolStrip is StatusStrip)
				return;
				
			if (e.ToolStrip is ToolStripDropDown)
				ControlPaint.DrawBorder3D (e.Graphics, e.AffectedBounds, Border3DStyle.Raised);
			else {
				e.Graphics.DrawLine (SystemPens.ControlDark, new Point (e.ToolStrip.Left, e.ToolStrip.Height - 2), new Point (e.ToolStrip.Right, e.ToolStrip.Height - 2));
				e.Graphics.DrawLine (Pens.White, new Point (e.ToolStrip.Left, e.ToolStrip.Height - 1), new Point (e.ToolStrip.Right, e.ToolStrip.Height - 1));
			}
		}
		#endregion
	}
}
