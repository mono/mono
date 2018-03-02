//
// DefaultLayout.cs
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
//	Stefan Noack (noackstefan@googlemail.com)
//

using System;
using System.Drawing;

namespace System.Windows.Forms.Layout
{
	class DefaultLayout : LayoutEngine
	{
		internal static readonly DefaultLayout Instance = new DefaultLayout();

		private DefaultLayout ()
		{
		}
		
		static void LayoutDockedChildren (Control parent, Control[] controls)
		{
			Rectangle space = parent.DisplayRectangle;
			MdiClient mdi = null;
			
			// Deal with docking; go through in reverse, MS docs say that lowest Z-order is closest to edge
			for (int i = controls.Length - 1; i >= 0; i--) {
				Control child = controls[i];
				Size child_size = child.Size;

				if (child.AutoSizeInternal)
					child_size = GetPreferredControlSize (child);

				if (!child.VisibleInternal
				    || child.ControlLayoutType == Control.LayoutType.Anchor)
					continue;

				// MdiClient never fills the whole area like other controls, have to do it later
				if (child is MdiClient) {
					mdi = (MdiClient)child;
					continue;
				}
				
				switch (child.Dock) {
				case DockStyle.None:
					// Do nothing
					break;

				case DockStyle.Left:
					child.SetBoundsInternal (space.Left, space.Y, child_size.Width, space.Height, BoundsSpecified.None);
					space.X += child.Width;
					space.Width -= child.Width;
					break;

				case DockStyle.Top:
					child.SetBoundsInternal (space.Left, space.Y, space.Width, child_size.Height, BoundsSpecified.None);
					space.Y += child.Height;
					space.Height -= child.Height;
					break;

				case DockStyle.Right:
					child.SetBoundsInternal (space.Right - child_size.Width, space.Y, child_size.Width, space.Height, BoundsSpecified.None);
					space.Width -= child.Width;
					break;

				case DockStyle.Bottom:
					child.SetBoundsInternal (space.Left, space.Bottom - child_size.Height, space.Width, child_size.Height, BoundsSpecified.None);
					space.Height -= child.Height;
					break;
					
				case DockStyle.Fill:
					child.SetBoundsInternal (space.Left, space.Top, space.Width, space.Height, BoundsSpecified.None);
					break;
				}
			}

			// MdiClient gets whatever space is left
			if (mdi != null)
				mdi.SetBoundsInternal (space.Left, space.Top, space.Width, space.Height, BoundsSpecified.None);
		}

		static void LayoutAnchoredChildren (Control parent, Control[] controls)
		{
			Rectangle space = parent.ClientRectangle;

			for (int i = 0; i < controls.Length; i++) {
				int left;
				int top;
				int width;
				int height;

				Control child = controls[i];

				if (!child.VisibleInternal
				    || child.ControlLayoutType == Control.LayoutType.Dock)
					continue;

				AnchorStyles anchor = child.Anchor;

				left = child.Left;
				top = child.Top;
				
				width = child.Width;
				height = child.Height;

				if ((anchor & AnchorStyles.Right) != 0) {
					if ((anchor & AnchorStyles.Left) != 0)
						width = space.Width - child.dist_right - left;
					else
						left = space.Width - child.dist_right - width;
				}
				else if ((anchor & AnchorStyles.Left) == 0) {
					// left+=diff_width/2 will introduce rounding errors (diff_width removed from svn after r51780)
					// This calculates from scratch every time:
					left = left + (space.Width - (left + width + child.dist_right)) / 2;
					child.dist_right = space.Width - (left + width);
				}

				if ((anchor & AnchorStyles.Bottom) != 0) {
					if ((anchor & AnchorStyles.Top) != 0)
						height = space.Height - child.dist_bottom - top;
					else
						top = space.Height - child.dist_bottom - height;
				}
				else if ((anchor & AnchorStyles.Top) == 0) {
					// top += diff_height/2 will introduce rounding errors (diff_height removed from after r51780)
					// This calculates from scratch every time:
					top = top + (space.Height - (top + height + child.dist_bottom)) / 2;
					child.dist_bottom = space.Height - (top + height);
				}

				// Sanity
				if (width < 0)
					width = 0;

				if (height < 0)
					height = 0;

				child.SetBoundsInternal (left, top, width, height, BoundsSpecified.None);
			}
		}
		
		static void LayoutAutoSizedChildren (Control parent, Control[] controls)
		{
			for (int i = 0; i < controls.Length; i++) {
				int left;
				int top;

				Control child = controls[i];
				if (!child.VisibleInternal
				    || child.ControlLayoutType == Control.LayoutType.Dock
				    || !child.AutoSizeInternal)
					continue;

				AnchorStyles anchor = child.Anchor;
				left = child.Left;
				top = child.Top;
				
				Size preferredsize = GetPreferredControlSize (child);

				if (((anchor & AnchorStyles.Left) != 0) || ((anchor & AnchorStyles.Right) == 0))
					child.dist_right += child.Width - preferredsize.Width;
				if (((anchor & AnchorStyles.Top) != 0) || ((anchor & AnchorStyles.Bottom) == 0))
					child.dist_bottom += child.Height - preferredsize.Height;

				child.SetBoundsInternal (left, top, preferredsize.Width, preferredsize.Height, BoundsSpecified.None);
			}
		}

		static void LayoutAutoSizeContainer (Control container)
		{
			int left;
			int top;
			int width;
			int height;

			if (!container.VisibleInternal || container.ControlLayoutType == Control.LayoutType.Dock || !container.AutoSizeInternal)
				return;

			left = container.Left;
			top = container.Top;

			Size preferredsize = container.PreferredSize;

			if (container.GetAutoSizeMode () == AutoSizeMode.GrowAndShrink) {
				width = preferredsize.Width;
				height = preferredsize.Height;
			} else {
				width = container.ExplicitBounds.Width;
				height = container.ExplicitBounds.Height;
				if (preferredsize.Width > width)
					width = preferredsize.Width;
				if (preferredsize.Height > height)
					height = preferredsize.Height;
			}

			// Sanity
			if (width < container.MinimumSize.Width)
				width = container.MinimumSize.Width;

			if (height < container.MinimumSize.Height)
				height = container.MinimumSize.Height;

			if (container.MaximumSize.Width != 0 && width > container.MaximumSize.Width)
				width = container.MaximumSize.Width;

			if (container.MaximumSize.Height != 0 && height > container.MaximumSize.Height)
				height = container.MaximumSize.Height;

			container.SetBoundsInternal (left, top, width, height, BoundsSpecified.None);
		}

		public override bool Layout (object container, LayoutEventArgs args)
		{
			Control parent = container as Control;

			Control[] controls = parent.Controls.GetAllControls ();

			LayoutDockedChildren (parent, controls);
			LayoutAnchoredChildren (parent, controls);
			LayoutAutoSizedChildren (parent, controls);
			if (parent is Form) LayoutAutoSizeContainer (parent);

			return false;
		}

		static private Size GetPreferredControlSize (Control child)
		{
			int width;
			int height;
			Size preferredsize = child.PreferredSize;

			if (child.GetAutoSizeMode () == AutoSizeMode.GrowAndShrink || (child.Dock != DockStyle.None && !(child is Button) && !(child is FlowLayoutPanel))) {
				width = preferredsize.Width;
				height = preferredsize.Height;
			} else {
				width = child.ExplicitBounds.Width;
				height = child.ExplicitBounds.Height;
				if (preferredsize.Width > width)
					width = preferredsize.Width;
				if (preferredsize.Height > height)
					height = preferredsize.Height;
			}
			if (child.AutoSizeInternal && child is FlowLayoutPanel && child.Dock != DockStyle.None) {
				switch (child.Dock) {
				case DockStyle.Left:
				case DockStyle.Right:
					if (preferredsize.Width < child.ExplicitBounds.Width && preferredsize.Height < child.Parent.PaddingClientRectangle.Height)
						width = preferredsize.Width;
					break;
				case DockStyle.Top:
				case DockStyle.Bottom:
					if (preferredsize.Height < child.ExplicitBounds.Height && preferredsize.Width < child.Parent.PaddingClientRectangle.Width)
						height = preferredsize.Height;
					break;
				}
			}
			// Sanity
			if (width < child.MinimumSize.Width)
				width = child.MinimumSize.Width;

			if (height < child.MinimumSize.Height)
				height = child.MinimumSize.Height;

			if (child.MaximumSize.Width != 0 && width > child.MaximumSize.Width)
				width = child.MaximumSize.Width;

			if (child.MaximumSize.Height != 0 && height > child.MaximumSize.Height)
				height = child.MaximumSize.Height;
				
			return new Size (width, height);
		}

		internal override Size GetPreferredSize (object container, Size proposedConstraints)
		{
			Control parent = container as Control;
			Size retsize = Size.Empty;

			// Add up the requested sizes for Docked controls
			foreach (Control child in parent.Controls) {
				if (!child.VisibleInternal)
					continue;
					
				var sz = child.GetPreferredSize (new Size(0, 0));
				//Console.WriteLine(" type=" + child.GetType().Name + ", text=" + (child.Text ?? "") + ", w=" + sz.Width + ", h=" + sz.Height + ", dock=" + child.Dock);

				if (child.Dock == DockStyle.Left || child.Dock == DockStyle.Right)
				{
					retsize.Width += sz.Width + child.Margin.Horizontal;
					retsize.Height = Math.Max(retsize.Height, sz.Height + child.Margin.Vertical);
				}
				else if (child.Dock == DockStyle.Top || child.Dock == DockStyle.Bottom)
				{
					retsize.Height += sz.Height + child.Margin.Vertical;
					retsize.Width = Math.Max(retsize.Width, sz.Width + child.Margin.Horizontal);
				}
				else if (child.Dock == DockStyle.None)
				{
				}
				else if (child.Dock == DockStyle.Fill)
				{
					// Strange, but it works
					retsize.Width = Math.Max(retsize.Width, sz.Width + child.Margin.Horizontal);
					retsize.Height = Math.Max(retsize.Height, sz.Height + child.Margin.Vertical);
				}
			}
			
			// See if any non-Docked control is positioned lower or more right than our size
			foreach (Control child in parent.Controls) {
				if (!child.VisibleInternal)
					continue;

				if (child.Dock != DockStyle.None)
					continue;
					
				// If its anchored to the bottom or right, that doesn't really count
				if ((child.Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom || (child.Anchor & AnchorStyles.Right) == AnchorStyles.Right)
					continue;
					
				retsize.Width = Math.Max (retsize.Width, child.Bounds.Right + child.Margin.Right);
				retsize.Height = Math.Max (retsize.Height, child.Bounds.Bottom + child.Margin.Bottom);
			}

			return retsize;
		}		
	}
}
