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

#if NET_2_0
using System;
using System.Collections.Generic;
using System.Drawing;

namespace System.Windows.Forms.Layout
{
	class FlowLayout : LayoutEngine
	{
		private static FlowLayoutSettings default_settings = new FlowLayoutSettings ();
		
		public FlowLayout ()
		{
		}

		public override void InitLayout (object child, BoundsSpecified specified)
		{
			base.InitLayout (child, specified);
		}

		public override bool Layout (object container, LayoutEventArgs args)
		{
			if (container is ToolStripPanel)
				return false;
				
			if (container is ToolStrip)
				return LayoutToolStrip ((ToolStrip)container);
				
			Control parent = container as Control;
			
			FlowLayoutSettings settings;
			if (parent is FlowLayoutPanel)
				settings = (parent as FlowLayoutPanel).LayoutSettings;
			else
				settings = default_settings;

			// Nothing to layout, exit method
			if (parent.Controls.Count == 0) return false;

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

			List<Control> rowControls = new List<Control> ();

			foreach (Control c in parent.Controls) {
				// Only apply layout to visible controls.
				if (!c.Visible) { continue; }

				// Resize any AutoSize controls to their preferred size
				if (c.AutoSize == true) {
					Size new_size = c.GetPreferredSize (c.Size);
					c.SetBoundsInternal (c.Left, c.Top, new_size.Width, new_size.Height, BoundsSpecified.None);
				}

				switch (settings.FlowDirection) {
					case FlowDirection.BottomUp:
						// Decide if it's time to start a new column
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents)
							if ((currentLocation.Y) < (c.Height + c.Margin.Top + c.Margin.Bottom) || forceFlowBreak) {

								currentLocation.X = FinishColumn (rowControls);
								currentLocation.Y = parentDisplayRectangle.Bottom;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the right margin and set the control to our point
						currentLocation.Offset (0, c.Margin.Bottom * -1);
						c.SetBoundsInternal (currentLocation.X + c.Margin.Left, currentLocation.Y - c.Height, c.Width, c.Height, BoundsSpecified.None);

						// Update our location pointer
						currentLocation.Y -= (c.Height + c.Margin.Top);
						break;
					case FlowDirection.LeftToRight:
					default:
						// Decide if it's time to start a new row
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents  && !(parent is ToolStripPanel))
							if ((parentDisplayRectangle.Width + parentDisplayRectangle.Left - currentLocation.X) < (c.Width + c.Margin.Left + c.Margin.Right) || forceFlowBreak) {

								currentLocation.Y = FinishRow (rowControls);
								currentLocation.X = parentDisplayRectangle.Left;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the left margin and set the control to our point
						currentLocation.Offset (c.Margin.Left, 0);
						c.SetBoundsInternal (currentLocation.X, currentLocation.Y + c.Margin.Top, c.Width, c.Height, BoundsSpecified.None);

						// Update our location pointer
						currentLocation.X += c.Width + c.Margin.Right;
						break;
					case FlowDirection.RightToLeft:
						// Decide if it's time to start a new row
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents)
							if ((currentLocation.X) < (c.Width + c.Margin.Left + c.Margin.Right) || forceFlowBreak) {

								currentLocation.Y = FinishRow (rowControls);
								currentLocation.X = parentDisplayRectangle.Right;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the right margin and set the control to our point
						currentLocation.Offset (c.Margin.Right * -1, 0);
						c.SetBoundsInternal (currentLocation.X - c.Width, currentLocation.Y + c.Margin.Top, c.Width, c.Height, BoundsSpecified.None);

						// Update our location pointer
						currentLocation.X -= (c.Width + c.Margin.Left);
						break;
					case FlowDirection.TopDown:
						// Decide if it's time to start a new column
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents)
							if ((parentDisplayRectangle.Height + parentDisplayRectangle.Top - currentLocation.Y) < (c.Height + c.Margin.Top + c.Margin.Bottom) || forceFlowBreak) {

								currentLocation.X = FinishColumn (rowControls);
								currentLocation.Y = parentDisplayRectangle.Top;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the top margin and set the control to our point
						currentLocation.Offset (0, c.Margin.Top);
						c.SetBoundsInternal (currentLocation.X + c.Margin.Left, currentLocation.Y, c.Width, c.Height, BoundsSpecified.None);

						// Update our location pointer
						currentLocation.Y += c.Height + c.Margin.Bottom;
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

			return false;
		}

		// Calculate the heights of the controls, returns the y coordinate of the greatest height it uses
		private int FinishRow (List<Control> row)
		{
			// Nothing to do
			if (row.Count == 0) return 0;

			int rowTop = int.MaxValue;
			int rowBottom = 0;
			bool allDockFill = true;
			bool noAuto = true;

			// Special semantics if all controls are Dock.Fill/Anchor:Top,Bottom or AutoSize = true
			foreach (Control c in row) {
				if (c.Dock != DockStyle.Fill && !((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top && (c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
					allDockFill = false;
				if (c.AutoSize == true)
					noAuto = false;
			}

			// Find the tallest control with a concrete height
			foreach (Control c in row) {
				if (c.Bottom + c.Margin.Bottom > rowBottom && (c.Dock != DockStyle.Fill) && ((c.Anchor & AnchorStyles.Top) != AnchorStyles.Top || (c.Anchor & AnchorStyles.Bottom) != AnchorStyles.Bottom || c.AutoSize == true))
					rowBottom = c.Bottom + c.Margin.Bottom;
				if (c.Top - c.Margin.Top < rowTop)
					rowTop = c.Top - c.Margin.Top;
			}

			// Find the tallest control that is AutoSize = true
			if (rowBottom == 0)
				foreach (Control c in row)
					if (c.Bottom + c.Margin.Bottom > rowBottom && (c.Dock != DockStyle.Fill && c.AutoSize == true))
						rowBottom = c.Bottom + c.Margin.Bottom;

			// Find the tallest control that is Dock = Fill
			if (rowBottom == 0)
				foreach (Control c in row)
					if (c.Bottom + c.Margin.Bottom > rowBottom && (c.Dock == DockStyle.Fill))
						rowBottom = c.Bottom + c.Margin.Bottom;

			// Set the new heights for each control
			foreach (Control c in row)
				if (allDockFill && noAuto)
					c.SetBoundsInternal (c.Left, c.Top, c.Width, 0, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Fill || ((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top) && ((c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
					c.SetBoundsInternal (c.Left, c.Top, c.Width, rowBottom - c.Top - c.Margin.Bottom, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Bottom || ((c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
					c.SetBoundsInternal (c.Left, rowBottom - c.Margin.Bottom - c.Height, c.Width, c.Height, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Top || ((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top))
					continue;
				else
					c.SetBoundsInternal (c.Left, ((rowBottom - rowTop) / 2) - (c.Height / 2) + (int)Math.Floor (((c.Margin.Top - c.Margin.Bottom) / 2.0)) + rowTop, c.Width, c.Height, BoundsSpecified.None);

			// Return bottom y of this row used
			if (rowBottom == 0)
				return rowTop;

			return rowBottom;
		}

		// Calculate the widths of the controls, returns the x coordinate of the greatest width it uses
		private int FinishColumn (List<Control> col)
		{
			// Nothing to do
			if (col.Count == 0) return 0;

			int rowLeft = int.MaxValue;
			int rowRight = 0;
			bool allDockFill = true;
			bool noAuto = true;

			// Special semantics if all controls are Dock.Fill/Anchor:Left,Right or AutoSize = true
			foreach (Control c in col) {
				if (c.Dock != DockStyle.Fill && !((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left && (c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
					allDockFill = false;
				if (c.AutoSize == true)
					noAuto = false;
			}

			// Find the widest control with a concrete width
			foreach (Control c in col) {
				if (c.Right + c.Margin.Right > rowRight && (c.Dock != DockStyle.Fill) && ((c.Anchor & AnchorStyles.Left) != AnchorStyles.Left || (c.Anchor & AnchorStyles.Right) != AnchorStyles.Right || c.AutoSize == true))
					rowRight = c.Right + c.Margin.Right;
				if (c.Left - c.Margin.Left < rowLeft)
					rowLeft = c.Left - c.Margin.Left;
			}

			// Find the widest control that is AutoSize = true
			if (rowRight == 0)
				foreach (Control c in col)
					if (c.Right + c.Margin.Right > rowRight && (c.Dock != DockStyle.Fill && c.AutoSize == true))
						rowRight = c.Right + c.Margin.Right;

			// Find the widest control that is Dock = Fill
			if (rowRight == 0)
				foreach (Control c in col)
					if (c.Right + c.Margin.Right > rowRight && (c.Dock == DockStyle.Fill))
						rowRight = c.Right + c.Margin.Right;

			// Set the new widths for each control
			foreach (Control c in col)
				if (allDockFill && noAuto)
					c.SetBoundsInternal (c.Left, c.Top, 0, c.Height, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Fill || ((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left) && ((c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
					c.SetBoundsInternal (c.Left, c.Top, rowRight - c.Left - c.Margin.Right, c.Height, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Right || ((c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
					c.SetBoundsInternal (rowRight - c.Margin.Right - c.Width, c.Top, c.Width, c.Height, BoundsSpecified.None);
				else if (c.Dock == DockStyle.Left || ((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left))
					continue;
				else
					c.SetBoundsInternal (((rowRight - rowLeft) / 2) - (c.Width / 2) + (int)Math.Floor (((c.Margin.Left - c.Margin.Right) / 2.0)) + rowLeft, c.Top, c.Width, c.Height, BoundsSpecified.None);

			// Return rightmost x of this row used
			if (rowRight == 0)
				return rowLeft;

			return rowRight;
		}

		#region Layout for ToolStrip
		// ToolStrips use the same FlowLayout, but is made up of ToolStripItems which
		// are Components instead of Controls, so we have to duplicate this login for
		// ToolStripItems.
		private bool LayoutToolStrip (ToolStrip parent)
		{
			FlowLayoutSettings settings;
			settings = (FlowLayoutSettings)parent.LayoutSettings;

			// Nothing to layout, exit method
			if (parent.Items.Count == 0) return false;

			foreach (ToolStripItem tsi in parent.Items)
				tsi.SetPlacement (ToolStripItemPlacement.Main);
				
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

			List<ToolStripItem> rowControls = new List<ToolStripItem> ();

			foreach (ToolStripItem c in parent.Items) {
				// Only apply layout to visible controls.
				if (!c.Available) { continue; }

				// Resize any AutoSize controls to their preferred size
				if (c.AutoSize == true)
					c.SetBounds (new Rectangle (c.Location, c.GetPreferredSize (c.Size)));

				switch (settings.FlowDirection) {
					case FlowDirection.BottomUp:
						// Decide if it's time to start a new column
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents)
							if ((currentLocation.Y) < (c.Height + c.Margin.Top + c.Margin.Bottom) || forceFlowBreak) {

								currentLocation.X = FinishColumn (rowControls);
								currentLocation.Y = parentDisplayRectangle.Bottom;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the right margin and set the control to our point
						currentLocation.Offset (0, c.Margin.Bottom * -1);
						c.Location = new Point (currentLocation.X + c.Margin.Left, currentLocation.Y - c.Height);

						// Update our location pointer
						currentLocation.Y -= (c.Height + c.Margin.Top);
						break;
					case FlowDirection.LeftToRight:
					default:
						// Decide if it's time to start a new row
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents)
							if ((parentDisplayRectangle.Width - currentLocation.X) < (c.Width + c.Margin.Left + c.Margin.Right) || forceFlowBreak) {

								currentLocation.Y = FinishRow (rowControls);
								currentLocation.X = parentDisplayRectangle.Left;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the left margin and set the control to our point
						currentLocation.Offset (c.Margin.Left, 0);
						c.Location = new Point (currentLocation.X, currentLocation.Y + c.Margin.Top);

						// Update our location pointer
						currentLocation.X += c.Width + c.Margin.Right;
						break;
					case FlowDirection.RightToLeft:
						// Decide if it's time to start a new row
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents)
							if ((currentLocation.X) < (c.Width + c.Margin.Left + c.Margin.Right) || forceFlowBreak) {

								currentLocation.Y = FinishRow (rowControls);
								currentLocation.X = parentDisplayRectangle.Right;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the right margin and set the control to our point
						currentLocation.Offset (c.Margin.Right * -1, 0);
						c.Location = new Point (currentLocation.X - c.Width, currentLocation.Y + c.Margin.Top);

						// Update our location pointer
						currentLocation.X -= (c.Width + c.Margin.Left);
						break;
					case FlowDirection.TopDown:
						// Decide if it's time to start a new column
						// - Our settings must be WrapContents, and we ran out of room or the previous control's FlowBreak == true
						if (settings.WrapContents)
							if ((parentDisplayRectangle.Height - currentLocation.Y) < (c.Height + c.Margin.Top + c.Margin.Bottom) || forceFlowBreak) {

								currentLocation.X = FinishColumn (rowControls);
								currentLocation.Y = parentDisplayRectangle.Top;

								forceFlowBreak = false;
								rowControls.Clear ();
							}

						// Offset the top margin and set the control to our point
						currentLocation.Offset (0, c.Margin.Top);
						c.Location = new Point (currentLocation.X + c.Margin.Left, currentLocation.Y);

						// Update our location pointer
						currentLocation.Y += c.Height + c.Margin.Bottom;
						break;
				}
				// Add it to our list of things to adjust the second dimension of
				rowControls.Add (c);

				// If user set a flowbreak on this control, it will be the last one in this row/column
				if (settings.GetFlowBreak (c))
					forceFlowBreak = true;
			}

			int final_height = 0;
			
			// Set the control heights/widths for the last row/column
			if (settings.FlowDirection == FlowDirection.LeftToRight || settings.FlowDirection == FlowDirection.RightToLeft)
				final_height = FinishRow (rowControls);
			else
				FinishColumn (rowControls);

			if (final_height > 0)
				parent.SetBoundsInternal (parent.Left, parent.Top, parent.Width, final_height + parent.Padding.Bottom, BoundsSpecified.None);
				
			return false;

		}

		// Calculate the heights of the controls, returns the y coordinate of the greatest height it uses
		private int FinishRow (List<ToolStripItem> row)
		{
			// Nothing to do
			if (row.Count == 0) return 0;

			int rowTop = int.MaxValue;
			int rowBottom = 0;
			bool allDockFill = true;
			bool noAuto = true;

			// Special semantics if all controls are Dock.Fill/Anchor:Top,Bottom or AutoSize = true
			foreach (ToolStripItem c in row) {
				if (c.Dock != DockStyle.Fill && !((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top && (c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
					allDockFill = false;
				if (c.AutoSize == true)
					noAuto = false;
			}

			// Find the tallest control with a concrete height
			foreach (ToolStripItem c in row) {
				if (c.Bottom + c.Margin.Bottom > rowBottom && (c.Dock != DockStyle.Fill) && ((c.Anchor & AnchorStyles.Top) != AnchorStyles.Top || (c.Anchor & AnchorStyles.Bottom) != AnchorStyles.Bottom || c.AutoSize == true))
					rowBottom = c.Bottom + c.Margin.Bottom;
				if (c.Top - c.Margin.Top < rowTop)
					rowTop = c.Top - c.Margin.Top;
			}

			// Find the tallest control that is AutoSize = true
			if (rowBottom == 0)
				foreach (ToolStripItem c in row)
					if (c.Bottom + c.Margin.Bottom > rowBottom && (c.Dock != DockStyle.Fill && c.AutoSize == true))
						rowBottom = c.Bottom + c.Margin.Bottom;

			// Find the tallest control that is Dock = Fill
			if (rowBottom == 0)
				foreach (ToolStripItem c in row)
					if (c.Bottom + c.Margin.Bottom > rowBottom && (c.Dock == DockStyle.Fill))
						rowBottom = c.Bottom + c.Margin.Bottom;

			// Set the new heights for each control
			foreach (ToolStripItem c in row)
				if (allDockFill && noAuto)
					c.Height = 0;
				else if (c.Dock == DockStyle.Fill || ((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top) && ((c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
					c.Height = rowBottom - c.Top - c.Margin.Bottom;
				else if (c.Dock == DockStyle.Bottom || ((c.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom))
					c.Top = rowBottom - c.Margin.Bottom - c.Height;
				else if (c.Dock == DockStyle.Top || ((c.Anchor & AnchorStyles.Top) == AnchorStyles.Top))
					continue;
				else
					c.Top = ((rowBottom - rowTop) / 2) - (c.Height / 2) + (int)Math.Floor (((c.Margin.Top - c.Margin.Bottom) / 2.0)) + rowTop;

			// Return bottom y of this row used
			if (rowBottom == 0)
				return rowTop;

			return rowBottom;
		}

		// Calculate the widths of the controls, returns the x coordinate of the greatest width it uses
		private int FinishColumn (List<ToolStripItem> col)
		{
			// Nothing to do
			if (col.Count == 0) return 0;

			int rowLeft = int.MaxValue;
			int rowRight = 0;
			bool allDockFill = true;
			bool noAuto = true;

			// Special semantics if all controls are Dock.Fill/Anchor:Left,Right or AutoSize = true
			foreach (ToolStripItem c in col) {
				if (c.Dock != DockStyle.Fill && !((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left && (c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
					allDockFill = false;
				if (c.AutoSize == true)
					noAuto = false;
			}

			// Find the widest control with a concrete width
			foreach (ToolStripItem c in col) {
				if (c.Right + c.Margin.Right > rowRight && (c.Dock != DockStyle.Fill) && ((c.Anchor & AnchorStyles.Left) != AnchorStyles.Left || (c.Anchor & AnchorStyles.Right) != AnchorStyles.Right || c.AutoSize == true))
					rowRight = c.Right + c.Margin.Right;
				if (c.Left - c.Margin.Left < rowLeft)
					rowLeft = c.Left - c.Margin.Left;
			}

			// Find the widest control that is AutoSize = true
			if (rowRight == 0)
				foreach (ToolStripItem c in col)
					if (c.Right + c.Margin.Right > rowRight && (c.Dock != DockStyle.Fill && c.AutoSize == true))
						rowRight = c.Right + c.Margin.Right;

			// Find the widest control that is Dock = Fill
			if (rowRight == 0)
				foreach (ToolStripItem c in col)
					if (c.Right + c.Margin.Right > rowRight && (c.Dock == DockStyle.Fill))
						rowRight = c.Right + c.Margin.Right;

			// Set the new widths for each control
			foreach (ToolStripItem c in col)
				if (allDockFill && noAuto)
					c.Width = 0;
				else if (c.Dock == DockStyle.Fill || ((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left) && ((c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
					c.Width = rowRight - c.Left - c.Margin.Right;
				else if (c.Dock == DockStyle.Right || ((c.Anchor & AnchorStyles.Right) == AnchorStyles.Right))
					c.Left = rowRight - c.Margin.Right - c.Width;
				else if (c.Dock == DockStyle.Left || ((c.Anchor & AnchorStyles.Left) == AnchorStyles.Left))
					continue;
				else
					c.Left = ((rowRight - rowLeft) / 2) - (c.Width / 2) + (int)Math.Floor (((c.Margin.Left - c.Margin.Right) / 2.0)) + rowLeft;

			// Return rightmost x of this row used
			if (rowRight == 0)
				return rowLeft;

			return rowRight;
		}
		#endregion
	}
}
#endif