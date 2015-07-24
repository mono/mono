//
// ToolStripGripRenderEventArgs.cs
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

using System.Drawing;

namespace System.Windows.Forms
{
	public class ToolStripGripRenderEventArgs : ToolStripRenderEventArgs
	{
		private Rectangle grip_bounds;
		private ToolStripGripDisplayStyle grip_display_style;
		private ToolStripGripStyle grip_style;
		
		public ToolStripGripRenderEventArgs (Graphics g, ToolStrip toolStrip) 
			: base (g, toolStrip)
		{
			this.grip_bounds = new Rectangle (2, 0, 3, 25);
			this.grip_display_style = ToolStripGripDisplayStyle.Vertical;
			this.grip_style = ToolStripGripStyle.Visible;
		}

		// There seems to be no public way to set these properties  :/
		internal ToolStripGripRenderEventArgs (Graphics g, ToolStrip toolStrip, Rectangle gripBounds, ToolStripGripDisplayStyle displayStyle, ToolStripGripStyle gripStyle)
			: base (g, toolStrip)
		{
			this.grip_bounds = gripBounds;
			this.grip_display_style = displayStyle;
			this.grip_style = gripStyle;
		}

		#region Public Properties
		public Rectangle GripBounds {
			get { return this.grip_bounds; }
		}

		public ToolStripGripDisplayStyle GripDisplayStyle {
			get { return this.grip_display_style; }
		}

		public ToolStripGripStyle GripStyle {
			get { return this.grip_style; }
		}
		#endregion
	}
}
