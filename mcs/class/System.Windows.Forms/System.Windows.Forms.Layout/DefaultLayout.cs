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
using System.Collections;

namespace System.Windows.Forms.Layout
{
	class DefaultLayout : LayoutEngine
	{
		internal static readonly DefaultLayout Instance = new DefaultLayout();

		private DefaultLayout ()
		{
		}
	
		public override void InitLayout (object child, BoundsSpecified specified)
		{
			Control control = child as Control;
			Control parent = control.Parent;
			if (parent != null) {
				Rectangle bounds = control.Bounds;
				if ((specified & (BoundsSpecified.Width | BoundsSpecified.X)) != BoundsSpecified.None)
					control.dist_right = parent.DisplayRectangle.Right - bounds.X - bounds.Width;
				if ((specified & (BoundsSpecified.Height | BoundsSpecified.Y)) != BoundsSpecified.None)
					control.dist_bottom = parent.DisplayRectangle.Bottom - bounds.Y - bounds.Height;
			}		
		}

		static void LayoutDockedChildren (Control parent, Control[] controls)
		{
			Rectangle space = parent.DisplayRectangle;
			MdiClient mdi = null;
			
			// Deal with docking; go through in reverse, MS docs say that lowest Z-order is closest to edge
			for (int i = controls.Length - 1; i >= 0; i--) {
				Control child = controls[i];
				Size child_size = child.Size;

				if (!child.VisibleInternal || child.Dock == DockStyle.None)
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
					if (child.AutoSizeInternal)
						child_size = child.GetPreferredSize(new Size(0, space.Height));
					child.SetBoundsInternal (space.Left, space.Y, child_size.Width, space.Height, BoundsSpecified.None);
					space.X += child.Width;
					space.Width -= child.Width;
					break;

				case DockStyle.Top:
					if (child.AutoSizeInternal)
						child_size = child.GetPreferredSize(new Size(space.Width, 0));
					child.SetBoundsInternal (space.Left, space.Y, space.Width, child_size.Height, BoundsSpecified.None);
					space.Y += child.Height;
					space.Height -= child.Height;
					break;

				case DockStyle.Right:
					if (child.AutoSizeInternal)
						child_size = child.GetPreferredSize(new Size(0, space.Height));
					child.SetBoundsInternal (space.Right - child_size.Width, space.Y, child_size.Width, space.Height, BoundsSpecified.None);
					space.Width -= child.Width;
					break;

				case DockStyle.Bottom:
					if (child.AutoSizeInternal)
						child_size = child.GetPreferredSize(new Size(space.Width, 0));
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

		static void LayoutAnchoredChildren (Control parent, IList controls)
		{
			Rectangle space = parent.DisplayRectangle;

			foreach (Control child in controls) {
				if (!child.VisibleInternal || child.Dock != DockStyle.None)
					continue;

				AnchorStyles anchor = child.Anchor;

				int left = child.Left;
				int top = child.Top;
				int width = child.Width;
				int height = child.Height;

				if ((anchor & AnchorStyles.Right) != 0) {
					if ((anchor & AnchorStyles.Left) != 0)
						width = space.Right - child.dist_right - left;
					else
						left = space.Right - child.dist_right - width;
				}
				else if ((anchor & AnchorStyles.Left) == 0) {
					// left+=diff_width/2 will introduce rounding errors (diff_width removed from svn after r51780)
					// This calculates from scratch every time:
					left = left + (space.Width - (left + width + child.dist_right)) / 2;
					child.dist_right = space.Width - (left + width);
				}

				if ((anchor & AnchorStyles.Bottom) != 0) {
					if ((anchor & AnchorStyles.Top) != 0)
						height = space.Bottom - child.dist_bottom - top;
					else
						top = space.Bottom - child.dist_bottom - height;
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

				if (child.AutoSizeInternal) {
					Size proposedSize = Size.Empty;
					if ((anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right))
						proposedSize.Width = width;
					if ((anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom))
						proposedSize.Height = height;

					Size preferredsize = GetPreferredControlSize (child, proposedSize);

					if ((anchor & (AnchorStyles.Left | AnchorStyles.Right)) != AnchorStyles.Right)
						child.dist_right += width - preferredsize.Width;
					else
						left += width - preferredsize.Width;
					if ((anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) != AnchorStyles.Bottom)
						child.dist_bottom += height - preferredsize.Height;
					else
						top += height - preferredsize.Height;

					child.SetBoundsInternal (left, top, preferredsize.Width, preferredsize.Height, BoundsSpecified.None);
				} else {
					child.SetBoundsInternal (left, top, width, height, BoundsSpecified.None);
				}
			}
		}

		public override bool Layout (object container, LayoutEventArgs args)
		{
			Control parent = container as Control;
			Control[] controls = parent.Controls.GetAllControls ();

			LayoutDockedChildren (parent, controls);
			LayoutAnchoredChildren (parent, controls);

			return parent.AutoSize;
		}

		static private Size GetPreferredControlSize (Control child, Size proposed)
		{
			var preferredsize = child.GetPreferredSize (proposed);

			int width, height;
			if (child.GetAutoSizeMode () == AutoSizeMode.GrowAndShrink)
			{
				width = preferredsize.Width;
				height = preferredsize.Height;
			}
			else
			{
				width = child.ExplicitBounds.Width;
				height = child.ExplicitBounds.Height;
				if (preferredsize.Width > width)
					width = preferredsize.Width;
				if (preferredsize.Height > height)
					height = preferredsize.Height;
			}
			return new Size(width, height);
		}

		internal override Size GetPreferredSize (object container, Size proposedConstraints)
		{
			Control parent = container as Control;
			IList controls = parent.Controls;
			Size retsize = Size.Empty;

			// Add up the requested sizes for Docked controls
			for (int i = controls.Count - 1; i >= 0; i--) {
				Control child = (Control)controls[i];
				if (!child.VisibleInternal || child.Dock == DockStyle.None)
					continue;

				if (child.Dock == DockStyle.Left || child.Dock == DockStyle.Right) {
					Size sz = child.AutoSizeInternal ? child.GetPreferredSize (new Size(0, proposedConstraints.Height)) : child.Size;
					retsize.Width += sz.Width;
				} else if (child.Dock == DockStyle.Top || child.Dock == DockStyle.Bottom) {
					Size sz = child.AutoSizeInternal ? child.GetPreferredSize (new Size(proposedConstraints.Width, 0)) : child.Size;
					retsize.Height += sz.Height;
				} else if (child.Dock == DockStyle.Fill && child.AutoSizeInternal) {
					Size sz = child.GetPreferredSize (proposedConstraints);
					retsize += sz;
				}
			}

			// See if any non-Docked control is positioned lower or more right than our size
			foreach (Control child in parent.Controls) {
				if (!child.VisibleInternal || child.Dock != DockStyle.None)
					continue;
					
				// If its anchored to the bottom or right, that doesn't really count
				if ((child.Anchor & (AnchorStyles.Bottom | AnchorStyles.Top)) == AnchorStyles.Bottom ||
				    (child.Anchor & (AnchorStyles.Right | AnchorStyles.Left)) == AnchorStyles.Right)
					continue;

				Rectangle childBounds = child.Bounds;
				if (child.AutoSizeInternal) {
					Size proposedChildSize = Size.Empty;
					if ((child.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right)) {
						proposedChildSize.Width = proposedConstraints.Width - child.dist_right - (child.Left - parent.DisplayRectangle.Left);
					}
					if ((child.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom)) {
						proposedChildSize.Height = proposedConstraints.Height - child.dist_bottom - (child.Top - parent.DisplayRectangle.Top);
					}
					Size preferredsize = GetPreferredControlSize (child, proposedChildSize);
					childBounds = new Rectangle(child.Location, preferredsize);
				}

				// This is the non-sense Microsoft uses (Padding vs DisplayRectangle)
				retsize.Width = Math.Max (retsize.Width, childBounds.Right - parent.Padding.Left + child.Margin.Right);
				retsize.Height = Math.Max (retsize.Height, childBounds.Bottom - parent.Padding.Top + child.Margin.Bottom);
				//retsize.Width = Math.Max (retsize.Width, childBounds.Right - parent.DisplayRectangle.Left + child.Margin.Right);
				//retsize.Height = Math.Max (retsize.Height, childBounds.Bottom - parent.DisplayRectangle.Top + child.Margin.Bottom);
			}

			return retsize;
		}		
	}
}
