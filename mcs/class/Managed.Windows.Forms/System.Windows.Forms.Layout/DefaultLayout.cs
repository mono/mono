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
//

using System;
using System.Drawing;

namespace System.Windows.Forms.Layout
{
	class DefaultLayout : LayoutEngine
	{
		void LayoutDockedChildren (Control parent, Control[] controls)
		{
			Rectangle space = parent.ClientRectangle;
			MdiClient mdi = null;
			
			// Deal with docking; go through in reverse, MS docs say that lowest Z-order is closest to edge
			for (int i = controls.Length - 1; i >= 0; i--) {
				Control child = controls[i];

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
					child.SetBounds (space.Left, space.Y, child.Width, space.Height, BoundsSpecified.None);
					space.X += child.Width;
					space.Width -= child.Width;
					break;

				case DockStyle.Top:
					child.SetBounds (space.Left, space.Y, space.Width, child.Height, BoundsSpecified.None);
					space.Y += child.Height;
					space.Height -= child.Height;
					break;

				case DockStyle.Right:
					child.SetBounds (space.Right - child.Width, space.Y, child.Width, space.Height, BoundsSpecified.None);
					space.Width -= child.Width;
					break;

				case DockStyle.Bottom:
					child.SetBounds (space.Left, space.Bottom - child.Height, space.Width, child.Height, BoundsSpecified.None);
					space.Height -= child.Height;
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

		void LayoutAnchoredChildren (Control parent, Control[] controls)
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
				
#if NET_2_0
				Size preferredsize = child.PreferredSize;
				width = preferredsize.Width;
				height = preferredsize.Height;
#else
				width = child.Width;
				height = child.Height;
#endif

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
				}

				// Sanity
				if (width < 0)
					width = 0;

				if (height < 0)
					height = 0;

				child.SetBounds (left, top, width, height, BoundsSpecified.None);
			}
		}

		public override bool Layout (object container, LayoutEventArgs args)
		{
			Control parent = container as Control;

			Control[] controls = parent.Controls.GetAllControls ();

			LayoutDockedChildren (parent, controls);
			LayoutAnchoredChildren (parent, controls);

			return false;
		}
	}
}
