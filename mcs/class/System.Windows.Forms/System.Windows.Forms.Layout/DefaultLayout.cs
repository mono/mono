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
			IArrangedElement control = (IArrangedElement)child;
			IArrangedElement parent = control.Parent;
			if (parent != null) {
				Rectangle bounds = control.Bounds;
				if ((specified & (BoundsSpecified.Width | BoundsSpecified.X)) != BoundsSpecified.None)
					control.DistanceRight = parent.DisplayRectangle.Right - bounds.X - bounds.Width;
				if ((specified & (BoundsSpecified.Height | BoundsSpecified.Y)) != BoundsSpecified.None)
					control.DistanceBottom = parent.DisplayRectangle.Bottom - bounds.Y - bounds.Height;
			}		
		}

		static void LayoutDockedChildren (IArrangedElement parent, IList controls)
		{
			Rectangle space = parent.DisplayRectangle;
			IArrangedElement mdi = null;
			
			// Deal with docking; go through in reverse, MS docs say that lowest Z-order is closest to edge
			for (int i = controls.Count - 1; i >= 0; i--) {
				IArrangedElement child = (IArrangedElement)controls[i];
				Size child_size = child.Bounds.Size;

				if (!child.Visible || child.Dock == DockStyle.None)
					continue;

				// MdiClient never fills the whole area like other controls, have to do it later
				if (child is MdiClient) {
					mdi = child;
					continue;
				}
				
				switch (child.Dock) {
				case DockStyle.None:
					// Do nothing
					break;

				case DockStyle.Left:
					if (child.AutoSize)
						child_size = child.GetPreferredSize(new Size(0, space.Height));
					child.SetBounds (space.Left, space.Y, child_size.Width, space.Height, BoundsSpecified.None);
					space.X += child_size.Width;
					space.Width -= child_size.Width;
					break;

				case DockStyle.Top:
					if (child.AutoSize)
						child_size = child.GetPreferredSize(new Size(space.Width, 0));
					child.SetBounds (space.Left, space.Y, space.Width, child_size.Height, BoundsSpecified.None);
					space.Y += child_size.Height;
					space.Height -= child_size.Height;
					break;

				case DockStyle.Right:
					if (child.AutoSize)
						child_size = child.GetPreferredSize(new Size(0, space.Height));
					child.SetBounds (space.Right - child_size.Width, space.Y, child_size.Width, space.Height, BoundsSpecified.None);
					space.Width -= child_size.Width;
					break;

				case DockStyle.Bottom:
					if (child.AutoSize)
						child_size = child.GetPreferredSize(new Size(space.Width, 0));
					child.SetBounds (space.Left, space.Bottom - child_size.Height, space.Width, child_size.Height, BoundsSpecified.None);
					space.Height -= child_size.Height;
					break;
					
				case DockStyle.Fill:
					child.SetBounds (space.Left, space.Top, space.Width, space.Height, BoundsSpecified.None);
					break;
				}
			}

			// MdiClient gets whatever space is left
			if (mdi != null)
				mdi.SetBounds (space.Left, space.Top, space.Width, space.Height, BoundsSpecified.None);
		}

		static void LayoutAnchoredChildren (IArrangedElement parent, IList controls)
		{
			Rectangle space = parent.DisplayRectangle;

			foreach (IArrangedElement child in controls) {
				if (!child.Visible || child.Dock != DockStyle.None)
					continue;

				AnchorStyles anchor = child.Anchor;
				Rectangle bounds = child.Bounds;
				int left = bounds.Left;
				int top = bounds.Top;
				int width = bounds.Width;
				int height = bounds.Height;

				if ((anchor & AnchorStyles.Right) != 0) {
					if ((anchor & AnchorStyles.Left) != 0)
						width = space.Right - child.DistanceRight - left;
					else
						left = space.Right - child.DistanceRight - width;
				}
				else if ((anchor & AnchorStyles.Left) == 0) {
					// left+=diff_width/2 will introduce rounding errors (diff_width removed from svn after r51780)
					// This calculates from scratch every time:
					left += (space.Width - (left + width + child.DistanceRight)) / 2;
					child.DistanceRight = space.Width - (left + width);
				}

				if ((anchor & AnchorStyles.Bottom) != 0) {
					if ((anchor & AnchorStyles.Top) != 0)
						height = space.Bottom - child.DistanceBottom - top;
					else
						top = space.Bottom - child.DistanceBottom - height;
				}
				else if ((anchor & AnchorStyles.Top) == 0) {
					// top += diff_height/2 will introduce rounding errors (diff_height removed from after r51780)
					// This calculates from scratch every time:
					top += (space.Height - (top + height + child.DistanceBottom)) / 2;
					child.DistanceBottom = space.Height - (top + height);
				}

				// Sanity
				if (width < 0)
					width = 0;
				if (height < 0)
					height = 0;

				if (child.AutoSize) {
					Size proposed_size = Size.Empty;
					if ((anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right))
						proposed_size.Width = width;
					if ((anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom))
						proposed_size.Height = height;

					Size preferred_size = GetPreferredControlSize (child, proposed_size);

					if ((anchor & (AnchorStyles.Left | AnchorStyles.Right)) != AnchorStyles.Right)
						child.DistanceRight += width - preferred_size.Width;
					else
						left += width - preferred_size.Width;
					if ((anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) != AnchorStyles.Bottom)
						child.DistanceBottom += height - preferred_size.Height;
					else
						top += height - preferred_size.Height;

					child.SetBounds (left, top, preferred_size.Width, preferred_size.Height, BoundsSpecified.None);
				} else {
					child.SetBounds (left, top, width, height, BoundsSpecified.None);
				}
			}
		}

		public override bool Layout (object container, LayoutEventArgs args)
		{
			IArrangedContainer parent = (IArrangedContainer)container;

			if (parent.Controls is Control.ControlCollection controlCollection) {
				LayoutDockedChildren (parent, controlCollection.GetAllControls());
				LayoutAnchoredChildren (parent, controlCollection.GetAllControls());
			} else {
				LayoutDockedChildren (parent, parent.Controls);
				LayoutAnchoredChildren (parent, parent.Controls);
			}

			return parent.AutoSize;
		}

		static private Size GetPreferredControlSize (IArrangedElement child, Size proposed)
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
			IArrangedContainer parent = (IArrangedContainer)container;
			IList controls = parent.Controls;
			Size retsize = Size.Empty;

			// Add up the requested sizes for Docked controls
			for (int i = controls.Count - 1; i >= 0; i--) {
				IArrangedElement child = (IArrangedElement)controls[i];
				if (!child.Visible || child.Dock == DockStyle.None)
					continue;

				if (child.Dock == DockStyle.Left || child.Dock == DockStyle.Right) {
					Size sz = child.AutoSize ? child.GetPreferredSize (new Size(0, proposedConstraints.Height)) : child.Bounds.Size;
					retsize.Width += sz.Width;
				} else if (child.Dock == DockStyle.Top || child.Dock == DockStyle.Bottom) {
					Size sz = child.AutoSize ? child.GetPreferredSize (new Size(proposedConstraints.Width, 0)) : child.Bounds.Size;
					retsize.Height += sz.Height;
				} else if (child.Dock == DockStyle.Fill && child.AutoSize) {
					Size sz = child.GetPreferredSize (proposedConstraints);
					retsize += sz;
				}
			}

			// See if any non-Docked control is positioned lower or more right than our size
			foreach (IArrangedElement child in parent.Controls) {
				if (!child.Visible || child.Dock != DockStyle.None)
					continue;
					
				// If its anchored to the bottom or right, that doesn't really count
				if ((child.Anchor & (AnchorStyles.Bottom | AnchorStyles.Top)) == AnchorStyles.Bottom ||
				    (child.Anchor & (AnchorStyles.Right | AnchorStyles.Left)) == AnchorStyles.Right)
					continue;

				Rectangle child_bounds = child.Bounds;
				if (child.AutoSize) {
					Size proposed_child_size = Size.Empty;
					if ((child.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right)) {
						proposed_child_size.Width = proposedConstraints.Width - child.DistanceRight - (child_bounds.Left - parent.DisplayRectangle.Left);
					}
					if ((child.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom)) {
						proposed_child_size.Height = proposedConstraints.Height - child.DistanceBottom - (child_bounds.Top - parent.DisplayRectangle.Top);
					}
					Size preferred_size = GetPreferredControlSize (child, proposed_child_size);
					child_bounds = new Rectangle (child_bounds.Location, preferred_size);
				}

				// This is the non-sense Microsoft uses (Padding vs DisplayRectangle)
				retsize.Width = Math.Max (retsize.Width, child_bounds.Right - parent.Padding.Left + child.Margin.Right);
				retsize.Height = Math.Max (retsize.Height, child_bounds.Bottom - parent.Padding.Top + child.Margin.Bottom);
				//retsize.Width = Math.Max (retsize.Width, child_bounds.Right - parent.DisplayRectangle.Left + child.Margin.Right);
				//retsize.Height = Math.Max (retsize.Height, child_bounds.Bottom - parent.DisplayRectangle.Top + child.Margin.Bottom);
			}

			return retsize;
		}		
	}
}
