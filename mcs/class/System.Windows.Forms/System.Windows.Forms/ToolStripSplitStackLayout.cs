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
					
				Rectangle ts_rect = ts.DisplayRectangle;

				// Mono hasn't yet implemented ToolStripLayoutStyle.Table, so it comes here
				// for layout.  The default (minimal) Table layout is 1 column, any number of rows,
				// which translates effectively to Vertical orientation.
				if (ts.Orientation == Orientation.Horizontal && ts.LayoutStyle != ToolStripLayoutStyle.Table)
					LayoutHorizontalToolStrip (ts, ts_rect);
				else
					LayoutVerticalToolStrip (ts, ts_rect);
					
				return ts.AutoSize;
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

				return ts.AutoSize;
			}
		}
		
		private void LayoutHorizontalToolStrip (ToolStrip ts, Rectangle bounds)
		{
			//if (!ts.Visible) return;
			
			ToolStripItemOverflow[] overflow = new ToolStripItemOverflow[ts.Items.Count];
			ToolStripItemPlacement[] placement = new ToolStripItemPlacement[ts.Items.Count];
			Size proposedSize = new Size (0, bounds.Height);
			int[] widths = new int[ts.Items.Count];
			int total_width = 0;
			int toolstrip_width = bounds.Width;
			int i = 0;
			bool can_overflow = ts.CanOverflow & !(ts is MenuStrip) & !(ts is StatusStrip);
			bool need_overflow = false;
			
			foreach (ToolStripItem tsi in ts.Items) {
				overflow[i] = tsi.Overflow;
				placement[i] = tsi.Overflow == ToolStripItemOverflow.Always ? ToolStripItemPlacement.Overflow : ToolStripItemPlacement.Main;
				widths[i] = tsi.GetPreferredSize (proposedSize).Width + tsi.Margin.Horizontal;
				if (!tsi.Available)
					placement[i] = ToolStripItemPlacement.None;
				total_width += placement[i] == ToolStripItemPlacement.Main ? widths[i] : 0;
				
				if (placement[i] == ToolStripItemPlacement.Overflow)
					need_overflow = true;
				i++;
			}

			// This is needed for a button set to Overflow = Always
			if (need_overflow) {
				ts.OverflowButton.Visible = true;
				ts.OverflowButton.SetBounds (new Rectangle (ts.Width - 16, 0, 16, ts.Height));
				toolstrip_width -= ts.OverflowButton.Width;
			} else
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

				// There's nothing left to remove, break or we will loop forever	
				if (!removed_one)
					break;
			}

			i = 0;
			Point start_layout_pointer = new Point (ts.DisplayRectangle.Left, ts.DisplayRectangle.Top);
			Point end_layout_pointer = new Point (ts.DisplayRectangle.Right, ts.DisplayRectangle.Top);
			int button_height = ts.DisplayRectangle.Height;
			
			// Now we should know where everything goes, so lay everything out
			foreach (ToolStripItem tsi in ts.Items) {
				tsi.SetPlacement (placement[i]);
				
				if (placement[i] == ToolStripItemPlacement.Main) {
					if (tsi.Alignment == ToolStripItemAlignment.Left) {
						tsi.SetBounds (new Rectangle (start_layout_pointer.X + tsi.Margin.Left, start_layout_pointer.Y + tsi.Margin.Top, widths[i] - tsi.Margin.Horizontal, button_height - tsi.Margin.Vertical));
						start_layout_pointer.X += widths[i];
					} else {
						tsi.SetBounds (new Rectangle (end_layout_pointer.X - tsi.Margin.Right - tsi.Width, end_layout_pointer.Y + tsi.Margin.Top, widths[i] - tsi.Margin.Horizontal, button_height - tsi.Margin.Vertical));
						end_layout_pointer.X -= widths[i];
					}			
				}
			
				i++;
			}			
		}

		private void LayoutVerticalToolStrip (ToolStrip ts, Rectangle bounds)
		{
			if (!ts.Visible) return;

			ToolStripItemOverflow[] overflow = new ToolStripItemOverflow[ts.Items.Count];
			ToolStripItemPlacement[] placement = new ToolStripItemPlacement[ts.Items.Count];
			Size proposedSize = new Size (bounds.Width, 0);
			int[] heights = new int[ts.Items.Count];
			int[] widths = new int[ts.Items.Count];	// needed if ts.LayoutStyle == ToolStripLayoutStyle.Table
			int total_height = 0;
			int toolstrip_height = bounds.Height;
			int i = 0;
			bool can_overflow = ts.CanOverflow & !(ts is MenuStrip) & !(ts is StatusStrip);

			foreach (ToolStripItem tsi in ts.Items) {
				overflow[i] = tsi.Overflow;
				placement[i] = tsi.Overflow == ToolStripItemOverflow.Always ? ToolStripItemPlacement.Overflow : ToolStripItemPlacement.Main;
				var size = tsi.GetPreferredSize (proposedSize);
				heights[i] = size.Height + tsi.Margin.Vertical;
				widths[i] = size.Width + tsi.Margin.Horizontal;
				if (!tsi.Available)
					placement[i] = ToolStripItemPlacement.None;
				total_height += placement[i] == ToolStripItemPlacement.Main ? heights[i] : 0;
				i++;
			}

			ts.OverflowButton.Visible = false;

			while (total_height > toolstrip_height) {
				// If we can overflow, get our overflow button setup, and subtract it's width
				// from our available width				
				if (can_overflow && !ts.OverflowButton.Visible) {
					ts.OverflowButton.Visible = true;
					ts.OverflowButton.SetBounds (new Rectangle (0, ts.Height - 16,  ts.Width, 16));
					toolstrip_height -= ts.OverflowButton.Height;
				}

				bool removed_one = false;

				// Start at the right, removing Overflow.AsNeeded first
				for (int j = heights.Length - 1; j >= 0; j--)
					if (overflow[j] == ToolStripItemOverflow.AsNeeded && placement[j] == ToolStripItemPlacement.Main) {
						placement[j] = ToolStripItemPlacement.Overflow;
						total_height -= heights[j];
						removed_one = true;
						break;
					}

				// If we didn't remove any AsNeeded ones, we have to start removing Never ones
				// These are not put on the Overflow, they are simply not shown
				if (!removed_one)
					for (int j = heights.Length - 1; j >= 0; j--)
						if (overflow[j] == ToolStripItemOverflow.Never && placement[j] == ToolStripItemPlacement.Main) {
							placement[j] = ToolStripItemPlacement.None;
							total_height -= heights[j];
							removed_one = true;
							break;
						}
					
				// There's nothing left to remove, break or we will loop forever	
				if (!removed_one)
					break;
			}

			i = 0;
			Point start_layout_pointer = new Point (ts.DisplayRectangle.Left, ts.DisplayRectangle.Top);
			Point end_layout_pointer = new Point (ts.DisplayRectangle.Left, ts.DisplayRectangle.Bottom);
			int button_width = ts.DisplayRectangle.Width;

			// Now we should know where everything goes, so lay everything out
			foreach (ToolStripItem tsi in ts.Items) {
				tsi.SetPlacement (placement[i]);
				// Table layout is defined to lay out items flush left.
				if (ts.LayoutStyle == ToolStripLayoutStyle.Table)
					button_width = widths[i];

				if (placement[i] == ToolStripItemPlacement.Main) {
					if (tsi.Alignment == ToolStripItemAlignment.Left) {
						tsi.SetBounds (new Rectangle (start_layout_pointer.X + tsi.Margin.Left, start_layout_pointer.Y + tsi.Margin.Top, button_width - tsi.Margin.Horizontal, heights[i] - tsi.Margin.Vertical));
						start_layout_pointer.Y += heights[i];
					} else {
						tsi.SetBounds (new Rectangle (end_layout_pointer.X + tsi.Margin.Left, end_layout_pointer.Y - tsi.Margin.Bottom - tsi.Height, button_width - tsi.Margin.Horizontal, heights[i] - tsi.Margin.Vertical));
						start_layout_pointer.Y += heights[i];
					}
				}

				i++;
			}
		}

		internal override Size GetPreferredSize(object container, Size proposedSize) {
			ToolStrip ts = (ToolStrip) container;
			Size new_size = Size.Empty;

			if (ts.Orientation == Orientation.Vertical) {
				foreach (ToolStripItem tsi in ts.Items)
					if (tsi.Available) {
						Size tsi_preferred = tsi.GetPreferredSize (Size.Empty);
						new_size.Height += tsi_preferred.Height + tsi.Margin.Top + tsi.Margin.Bottom;
						new_size.Width = Math.Max (new_size.Width, tsi_preferred.Width + tsi.Margin.Horizontal);
					}

				new_size.Height += (ts.GripRectangle.Height + ts.GripMargin.Vertical + 4);

				if (new_size.Width == 0)
					new_size.Width = ts.ExplicitBounds.Width;

				return new_size;
			} else {
				foreach (ToolStripItem tsi in ts.Items)
					if (tsi.Available) {
						Size tsi_preferred = tsi.GetPreferredSize (Size.Empty);
						new_size.Width += tsi_preferred.Width + tsi.Margin.Left + tsi.Margin.Right;
						new_size.Height = Math.Min (new_size.Height, tsi_preferred.Height + tsi.Margin.Vertical);
					}

				new_size.Width += (ts.GripRectangle.Width + ts.GripMargin.Horizontal + 4);

				if (new_size.Height == 0)
					new_size.Height = ts.ExplicitBounds.Height;

				if (ts is StatusStrip)
					new_size.Height = Math.Max (new_size.Height, 22);

				return new_size;
			}
		}
	}
}
