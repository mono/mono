//
// ToolStripSplitStackLayout.cs
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
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
	class ToolStripSplitStackLayout : LayoutEngine
	{
		public override bool Layout (object container, LayoutEventArgs args)
		{
			if (container is ToolStrip)
			{
				ToolStrip ts = (ToolStrip)container;

				if (ts.Items == null)
					return false;
					
				if (ts.Orientation == Orientation.Horizontal) {
					Rectangle ts_rect = ts.DisplayRectangle;
					
					LayoutHorizontalToolStrip (ts, ts_rect);
					return false;
				} else {
					int x = ts.DisplayRectangle.Left;
					int y = ts.DisplayRectangle.Top + 2;

					foreach (ToolStripItem tsi in ts.Items) {
						if (!tsi.Visible)
							continue;
						
						Rectangle new_bounds = new Rectangle ();

						y += tsi.Margin.Top;

						new_bounds.Location = new Point (x + (tsi is ToolStripSeparator ? 0 : tsi.Margin.Left), y);
						new_bounds.Width = ts.DisplayRectangle.Width - tsi.Margin.Horizontal - 3;
						new_bounds.Height = tsi.GetPreferredSize (new Size (new_bounds.Width, 0)).Height;

						tsi.SetBounds (new_bounds);

						y += new_bounds.Height + tsi.Margin.Bottom;
					}
				}
			} else	{
				ToolStripContentPanel ts = (ToolStripContentPanel)container;
				int x = ts.DisplayRectangle.Left;
				int y = ts.DisplayRectangle.Top;

				foreach (ToolStrip tsi in ts.Controls) {
					Rectangle new_bounds = new Rectangle ();

					x += tsi.Margin.Left;

					new_bounds.Location = new Point (x, y + tsi.Margin.Top);
					new_bounds.Height = ts.DisplayRectangle.Height - tsi.Margin.Vertical;
					new_bounds.Width = tsi.GetPreferredSize (new Size (0, new_bounds.Height)).Width;

					tsi.Width = new_bounds.Width + 12;

					x += new_bounds.Width + tsi.Margin.Right;
				}
			}

			return false;
		}
		
		private void LayoutHorizontalToolStrip (ToolStrip ts, Rectangle bounds)
		{
			if (!ts.Visible) return;
			
			ToolStripItemOverflow[] overflow = new ToolStripItemOverflow[ts.Items.Count];
			ToolStripItemPlacement[] placement = new ToolStripItemPlacement[ts.Items.Count];
			Size proposedSize = new Size (0, bounds.Height);
			int[] widths = new int[ts.Items.Count];
			int total_width = 0;
			int toolstrip_width = bounds.Width;
			int i = 0;
			bool can_overflow = ts.CanOverflow & !(ts is MenuStrip) & !(ts is StatusStrip);
			
			foreach (ToolStripItem tsi in ts.Items) {
				overflow[i] = tsi.Overflow;
				placement[i] = tsi.Overflow == ToolStripItemOverflow.Always ? ToolStripItemPlacement.Overflow : ToolStripItemPlacement.Main;
				widths[i] = tsi.GetPreferredSize (proposedSize).Width + tsi.Margin.Horizontal;
				total_width += placement[i] == ToolStripItemPlacement.Main ? widths[i] : 0;
				i++;
			}
			
			ts.OverflowButton.Visible = false;
			
			while (total_width > toolstrip_width) {
				// If we can overflow, get our overflow button setup, and subtract it's width
				// from our available width				
				if (can_overflow && !ts.OverflowButton.Visible) {
					ts.OverflowButton.Visible = true;
					ts.OverflowButton.SetBounds (new Rectangle (ts.Width - 16, 0, 16, ts.Height));
					toolstrip_width -= ts.OverflowButton.Width;
				}					
				
				bool removed_one = false;
				
				// Start at the right, removing Overflow.AsNeeded first
				for (int j = widths.Length - 1; j >= 0; j--)
					if (overflow[j] == ToolStripItemOverflow.AsNeeded && placement[j] == ToolStripItemPlacement.Main) {
						placement[j] = ToolStripItemPlacement.Overflow;
						total_width -= widths[j];
						removed_one = true;
						break;
					}
			
				// If we didn't remove any AsNeeded ones, we have to start removing Never ones
				// These are not put on the Overflow, they are simply not shown
				if (!removed_one)
					for (int j = widths.Length - 1; j >= 0; j--)
						if (overflow[j] == ToolStripItemOverflow.Never && placement[j] == ToolStripItemPlacement.Main) {
							placement[j] = ToolStripItemPlacement.None;
							total_width -= widths[j];
							removed_one = true;
							break;
						}
			}
			
			i = 0;
			Point layout_pointer = new Point (ts.DisplayRectangle.Left, ts.DisplayRectangle.Top);
			int button_height = ts.DisplayRectangle.Height;
			
			// Now we should know where everything goes, so lay everything out
			foreach (ToolStripItem tsi in ts.Items) {
				tsi.SetPlacement (placement[i]);
				
				if (placement[i] == ToolStripItemPlacement.Main) {
					tsi.SetBounds (new Rectangle (layout_pointer.X + tsi.Margin.Left, layout_pointer.Y + tsi.Margin.Top, widths[i] - tsi.Margin.Horizontal, button_height - tsi.Margin.Vertical));
					layout_pointer.X += widths[i];				
				}
			
				i++;
			}			
		}
	}
}
#endif
