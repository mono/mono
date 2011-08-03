//
// ToolStripSystemRenderer.cs
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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms.Theming;

namespace System.Windows.Forms
{
	public class ToolStripSystemRenderer : ToolStripRenderer
	{
		#region Public Constructor
		public ToolStripSystemRenderer ()
		{
		}
		#endregion

		#region Protected Methods
		protected override void OnRenderButtonBackground (ToolStripItemRenderEventArgs e)
		{
			ThemeElements.CurrentTheme.ToolStripPainter.OnRenderButtonBackground (e);

			base.OnRenderButtonBackground (e);
		}

		protected override void OnRenderDropDownButtonBackground (ToolStripItemRenderEventArgs e)
		{
			ThemeElements.CurrentTheme.ToolStripPainter.OnRenderDropDownButtonBackground (e);

			base.OnRenderDropDownButtonBackground (e);
		}

		protected override void OnRenderGrip (ToolStripGripRenderEventArgs e)
		{
			ThemeElements.CurrentTheme.ToolStripPainter.OnRenderGrip (e);

			base.OnRenderGrip (e);
		}

		protected override void OnRenderImageMargin (ToolStripRenderEventArgs e)
		{
			base.OnRenderImageMargin (e);
		}

		protected override void OnRenderItemBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderItemBackground (e);
		}

		protected override void OnRenderLabelBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderLabelBackground (e);
		}

		protected override void OnRenderMenuItemBackground (ToolStripItemRenderEventArgs e)
		{
			ThemeElements.CurrentTheme.ToolStripPainter.OnRenderMenuItemBackground (e);
			
			base.OnRenderMenuItemBackground (e);
		}

		protected override void OnRenderOverflowButtonBackground (ToolStripItemRenderEventArgs e)
		{
			ThemeElements.CurrentTheme.ToolStripPainter.OnRenderOverflowButtonBackground (e);

			base.OnRenderOverflowButtonBackground (e);
		}

		protected override void OnRenderSeparator (ToolStripSeparatorRenderEventArgs e)
		{
			ThemeElements.CurrentTheme.ToolStripPainter.OnRenderSeparator (e);

			base.OnRenderSeparator (e);
		}

		protected override void OnRenderSplitButtonBackground (ToolStripItemRenderEventArgs e)
		{
			ThemeElements.CurrentTheme.ToolStripPainter.OnRenderSplitButtonBackground (e);

			base.OnRenderSplitButtonBackground (e);
		}

		protected override void OnRenderToolStripBackground (ToolStripRenderEventArgs e)
		{
			ThemeElements.CurrentTheme.ToolStripPainter.OnRenderToolStripBackground (e);

			base.OnRenderToolStripBackground (e);
		}

		protected override void OnRenderToolStripBorder (ToolStripRenderEventArgs e)
		{
			ThemeElements.CurrentTheme.ToolStripPainter.OnRenderToolStripBorder (e);

			base.OnRenderToolStripBorder (e);
		}

		protected override void OnRenderToolStripStatusLabelBackground (ToolStripItemRenderEventArgs e)
		{
			base.OnRenderToolStripStatusLabelBackground (e);
		}
		#endregion
	}
}
