//
// FlowLayout.cs
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
using System.Collections.Generic;
using System.Drawing;

namespace System.Windows.Forms.Layout
{
	class FlowLayout : LayoutEngine
	{
		internal static readonly FlowLayout Instance = new FlowLayout();
		
		private static FlowLayoutSettings default_settings = new FlowLayoutSettings ();
		
		private FlowLayout ()
		{
		}

		public override void InitLayout (object child, BoundsSpecified specified)
		{
			base.InitLayout (child, specified);
		}

		private static FlowLayoutSettings GetLayoutSettings (IArrangedContainer container)
		{
			if (container is FlowLayoutPanel flp)
				return flp.LayoutSettings;
			if (container is ToolStrip ts)
				return (FlowLayoutSettings)ts.LayoutSettings;
			return default_settings;
		}

		public override bool Layout (object container, LayoutEventArgs args)
		{
			// TODO: Handle ToolStripPanel/ToolStripPanelRow
			if (container is ToolStripPanel)
				return false;
				
			IArrangedContainer parent = (IArrangedContainer)container;
			if (parent.Controls.Count == 0)
				return false;

			FlowLayoutSettings settings = GetLayoutSettings (parent);

			// Use DisplayRectangle so that parent.Padding is honored.
			Rectangle parentDisplayRectangle = parent.DisplayRectangle;
			Point currentLocation;

			// Set our starting point based on flow direction
			switch (settings.FlowDirection) {
				case FlowDirection.BottomUp:
					currentLocation = new Point (parentDisplayRectangle.Left, parentDisplayRectangle.Bottom);
					break;
				case FlowDirection.LeftToRight:
				case FlowDirection.TopDown:
				default:
					currentLocation = parentDisplayRectangle.Location;
					break;
				case FlowDirection.RightToLeft:
					currentLocation = new Point (parentDisplayRectangle.Right, parentDisplayRectangle.Top);
					break;
			}

			bool forceFlowBreak = false;

			List<IArrangedElement> rowControls = new List<IArrangedElement> ();

			foreach (IArrangedElement c in parent.Controls) {
				// Only apply layout to visible controls.
				if (!c.Visible) { continue; }

				// Resize any AutoSize controls to their preferred size
				if (c.AutoSize == true) {
					Size proposed_constraints = new Size(int.MaxValue, Math.Max(1, parentDisplayRectangle.Height - c.Margin.Vertical));
					if (currentLocation.X == parentDisplayRectangle.Left)
						proposed_constraints.Width = Math.Max(1, parentDisplayRectangle.Width - c.Margin.Horizontal);
					Size new_size = c.GetPreferredSize (proposed_constraints);
					c.SetBounds (c.Bounds.Left, c.Bounds.Top, new_size.Width, new_size.Height, BoundsSpecified.None);
				}

				switch (settings.FlowDirection) {
					case FlowDirection.BottomUp:
						// Decide if it's time to start a new column
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents)
							if ((currentLocation.Y) < (c.Bounds.Height + c.Margin.Top + c.Margin.Bottom) || forceFlowBreak) {

								currentLocation.X = FinishColumn (rowControls);
								currentLocation.Y = parentDisplayRectangle.Bottom;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the right margin and set the control to our point
						currentLocation.Offset (0, c.Margin.Bottom * -1);
						c.SetBounds (currentLocation.X + c.Margin.Left, currentLocation.Y - c.Bounds.Height, c.Bounds.Width, c.Bounds.Height, BoundsSpecified.None);

						// Update our location pointer
						currentLocation.Y -= (c.Bounds.Height + c.Margin.Top);
						break;
					case FlowDirection.LeftToRight:
					default:
						// Decide if it's time to start a new row
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents  && !(parent is ToolStripPanel))
							if ((parentDisplayRectangle.Width + parentDisplayRectangle.Left - currentLocation.X) < (c.Bounds.Width + c.Margin.Left + c.Margin.Right) || forceFlowBreak) {

								currentLocation.Y = FinishRow (rowControls);
								currentLocation.X = parentDisplayRectangle.Left;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the left margin and set the control to our point
						currentLocation.Offset (c.Margin.Left, 0);
						c.SetBounds (currentLocation.X, currentLocation.Y + c.Margin.Top, c.Bounds.Width, c.Bounds.Height, BoundsSpecified.None);

						// Update our location pointer
						currentLocation.X += c.Bounds.Width + c.Margin.Right;
						break;
					case FlowDirection.RightToLeft:
						// Decide if it's time to start a new row
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents)
							if ((currentLocation.X) < (c.Bounds.Width + c.Margin.Left + c.Margin.Right) || forceFlowBreak) {

								currentLocation.Y = FinishRow (rowControls);
								currentLocation.X = parentDisplayRectangle.Right;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the right margin and set the control to our point
						currentLocation.Offset (c.Margin.Right * -1, 0);
						c.SetBounds (currentLocation.X - c.Bounds.Width, currentLocation.Y + c.Margin.Top, c.Bounds.Width, c.Bounds.Height, BoundsSpecified.None);

						// Update our location pointer
						currentLocation.X -= (c.Bounds.Width + c.Margin.Left);
						break;
					case FlowDirection.TopDown:
						// Decide if it's time to start a new column
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents)
							if ((parentDisplayRectangle.Height + parentDisplayRectangle.Top - currentLocation.Y) < (c.Bounds.Height + c.Margin.Top + c.Margin.Bottom) || forceFlowBreak) {

								currentLocation.X = FinishColumn (rowControls);
								currentLocation.Y = parentDisplayRectangle.Top;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the top margin and set the control to our point
						currentLocation.Offset (0, c.Margin.Top);
						c.SetBounds (currentLocation.X + c.Margin.Left, currentLocation.Y, c.Bounds.Width, c.Bounds.Height, BoundsSpecified.None);

						// Update our location pointer
						currentLocation.Y += c.Bounds.Height + c.Margin.Bottom;
						break;
				}
				// Add it to our list of things to adjust the second dimension of
				rowControls.Add (c);

				// If user set a flowbreak on this control, it will be the last one in this row/column
				if (settings.GetFlowBreak (c))
					forceFlowBreak = true;
			}

			// Set the control heights/widths for the last row/column
			if (settings.FlowDirection == FlowDirection.LeftToRight || settings.FlowDirection == FlowDirection.RightToLeft)
				FinishRow (rowControls);
			else
				FinishColumn (rowControls);

			return parent.AutoSize;
		}

		internal override Size GetPreferredSize (object container, Size proposedSize)
		{
			IArrangedContainer parent = (IArrangedContainer)container;
			FlowLayoutSettings settings = GetLayoutSettings (parent);
			int width = 0;
			int height = 0;
			bool horizontal = settings.FlowDirection == FlowDirection.LeftToRight || settings.FlowDirection == FlowDirection.RightToLeft;
			int size_in_flow_direction = 0;
			int size_in_other_direction = 0;
			int increase;
			Size margin = new Size (0, 0);
			bool force_flow_break = false;
			bool wrap = settings.WrapContents;

			foreach (IArrangedElement control in parent.Controls) {
				if (!control.Visible)
					continue;
				Size control_preferred_size;
				if (control.AutoSize) {
					Size proposed_constraints = new Size(int.MaxValue, Math.Max(1, proposedSize.Height - control.Margin.Vertical));
					if (size_in_flow_direction == 0)
						proposed_constraints.Width = Math.Max(1, proposedSize.Width - control.Margin.Horizontal);
					control_preferred_size = control.GetPreferredSize(proposed_constraints);
				} else {
					control_preferred_size = control.ExplicitBounds.Size;
					if ((control.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom))
						control_preferred_size.Height = control.MinimumSize.Height;
				}
				Padding control_margin = control.Margin;
				if (horizontal) {
					increase = control_preferred_size.Width + control_margin.Horizontal;
					if (wrap && ((proposedSize.Width > 0 && size_in_flow_direction != 0 && size_in_flow_direction + increase >= proposedSize.Width) || force_flow_break)) {
						width = Math.Max (width, size_in_flow_direction);
						size_in_flow_direction = 0;
						height += size_in_other_direction;
						size_in_other_direction = 0;
					}
					size_in_flow_direction += increase;
					size_in_other_direction = Math.Max (size_in_other_direction, control_preferred_size.Height + control_margin.Vertical);
				} else {
					increase = control_preferred_size.Height + control_margin.Vertical;
					if (wrap && ((proposedSize.Height > 0 && size_in_flow_direction != 0 && size_in_flow_direction + increase >= proposedSize.Height) || force_flow_break)) {
						height = Math.Max (height, size_in_flow_direction);
						size_in_flow_direction = 0;
						width += size_in_other_direction;
						size_in_other_direction = 0;
					}
					size_in_flow_direction += increase;
					size_in_other_direction = Math.Max (size_in_other_direction, control_preferred_size.Width + control_margin.Horizontal);
				}

				if (parent != null)
					force_flow_break = settings.GetFlowBreak(control);
			}

			if (horizontal) {
				width = Math.Max (width, size_in_flow_direction);
				height += size_in_other_direction;
			} else {
				height = Math.Max (height, size_in_flow_direction);
				width += size_in_other_direction;
			}

			return new Size (width, height);
		}
		
		// Calculate the heights of the controls, returns the y coordinate of the greatest height it uses
		private int FinishRow (List<IArrangedElement> row)
		{
			// Nothing to do
			if (row.Count == 0) return 0;

			int rowTop = int.MaxValue;
			int rowBottom = 0;
			bool allDockFill = true;
			bool noAuto = true;

			// Special semantics if all controls are Dock.Fill/Anchor:Top,Bottom or AutoSize = true
			foreach (IArrangedElement c in row) {
				if (c.Dock != DockStyle.Fill && !((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top && (c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
					allDockFill = false;
				if (c.AutoSize == true)
					noAuto = false;
			}

			// Find the tallest control with a concrete height
			foreach (IArrangedElement c in row) {
				if (c.Dock != DockStyle.Fill && ((c.Anchor & AnchorStyles.Top) != AnchorStyles.Top || (c.Anchor & AnchorStyles.Bottom) != AnchorStyles.Bottom || c.AutoSize == true))
					rowBottom = Math.Max (rowBottom, c.Bounds.Bottom + c.Margin.Bottom);
				rowTop = Math.Min (rowTop, c.Bounds.Top - c.Margin.Top);
			}

			// Find the tallest control that is AutoSize = true
			if (rowBottom == 0)
				foreach (IArrangedElement c in row)
					if (c.Dock != DockStyle.Fill && c.AutoSize == true)
						rowBottom = Math.Max (rowBottom, c.Bounds.Bottom + c.Margin.Bottom);

			// Find the tallest control that is Dock = Fill
			if (rowBottom == 0)
				foreach (IArrangedElement c in row)
					if (c.Dock == DockStyle.Fill)
						rowBottom = Math.Max (rowBottom, c.Bounds.Bottom + c.Margin.Bottom);

			// Set the new heights for each control
			foreach (IArrangedElement c in row)
				if (allDockFill && noAuto)
					c.SetBounds (c.Bounds.Left, c.Bounds.Top, c.Bounds.Width, 0, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Fill || ((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top) && ((c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
					c.SetBounds (c.Bounds.Left, c.Bounds.Top, c.Bounds.Width, rowBottom - c.Bounds.Top - c.Margin.Bottom, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Bottom || ((c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
					c.SetBounds (c.Bounds.Left, rowBottom - c.Margin.Bottom - c.Bounds.Height, c.Bounds.Width, c.Bounds.Height, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Top || ((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top))
					continue;
				else
					c.SetBounds (c.Bounds.Left, ((rowBottom - rowTop) / 2) - (c.Bounds.Height / 2) + (int)Math.Floor (((c.Margin.Top - c.Margin.Bottom) / 2.0)) + rowTop, c.Bounds.Width, c.Bounds.Height, BoundsSpecified.None);

			// Return bottom y of this row used
			if (rowBottom == 0)
				return rowTop;

			return rowBottom;
		}

		// Calculate the widths of the controls, returns the x coordinate of the greatest width it uses
		private int FinishColumn (List<IArrangedElement> col)
		{
			// Nothing to do
			if (col.Count == 0) return 0;

			int rowLeft = int.MaxValue;
			int rowRight = 0;
			bool allDockFill = true;
			bool noAuto = true;

			// Special semantics if all controls are Dock.Fill/Anchor:Left,Right or AutoSize = true
			foreach (IArrangedElement c in col) {
				if (c.Dock != DockStyle.Fill && !((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left && (c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
					allDockFill = false;
				if (c.AutoSize == true)
					noAuto = false;
			}

			// Find the widest control with a concrete width
			foreach (IArrangedElement c in col) {
				if (c.Dock != DockStyle.Fill && ((c.Anchor & AnchorStyles.Left) != AnchorStyles.Left || (c.Anchor & AnchorStyles.Right) != AnchorStyles.Right || c.AutoSize == true))
					rowRight = Math.Max (rowRight, c.Bounds.Right + c.Margin.Right);
				rowLeft = Math.Min (rowLeft, c.Bounds.Left - c.Margin.Left);
			}

			// Find the widest control that is AutoSize = true
			if (rowRight == 0)
				foreach (IArrangedElement c in col)
					if (c.Dock != DockStyle.Fill && c.AutoSize == true)
						rowRight = Math.Max (rowRight, c.Bounds.Right + c.Margin.Right);

			// Find the widest control that is Dock = Fill
			if (rowRight == 0)
				foreach (IArrangedElement c in col)
					if (c.Dock == DockStyle.Fill)
						rowRight = Math.Max (rowRight, c.Bounds.Right + c.Margin.Right);

			// Set the new widths for each control
			foreach (IArrangedElement c in col)
				if (allDockFill && noAuto)
					c.SetBounds (c.Bounds.Left, c.Bounds.Top, 0, c.Bounds.Height, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Fill || ((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left) && ((c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
					c.SetBounds (c.Bounds.Left, c.Bounds.Top, rowRight - c.Bounds.Left - c.Margin.Right, c.Bounds.Height, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Right || ((c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
					c.SetBounds (rowRight - c.Margin.Right - c.Bounds.Width, c.Bounds.Top, c.Bounds.Width, c.Bounds.Height, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Left || ((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left))
					continue;
				else
					c.SetBounds (((rowRight - rowLeft) / 2) - (c.Bounds.Width / 2) + (int)Math.Floor (((c.Margin.Left - c.Margin.Right) / 2.0)) + rowLeft, c.Bounds.Top, c.Bounds.Width, c.Bounds.Height, BoundsSpecified.None);

			// Return rightmost x of this row used
			if (rowRight == 0)
				return rowLeft;

			return rowRight;
		}
	}
}
