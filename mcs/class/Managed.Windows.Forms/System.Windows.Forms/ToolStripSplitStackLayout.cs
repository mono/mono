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
					int x = ts.DisplayRectangle.Left;
					if (!(ts is MenuStrip))
						x += 2;
					int y = ts.DisplayRectangle.Top;

					foreach (ToolStripItem tsi in ts.Items) {
						if (!tsi.Visible)
							continue;
							
						Rectangle new_bounds = new Rectangle ();

						x += tsi.Margin.Left;

						new_bounds.Location = new Point (x, y + (tsi is ToolStripSeparator ? 0 : tsi.Margin.Top));
						new_bounds.Height = ts.DisplayRectangle.Height - tsi.Margin.Vertical;
						new_bounds.Width = tsi.GetPreferredSize (new Size (0, new_bounds.Height)).Width;

						tsi.SetBounds (new_bounds);

						x += new_bounds.Width + tsi.Margin.Right;
					}
				}
				else {
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
	}
}
#endif
