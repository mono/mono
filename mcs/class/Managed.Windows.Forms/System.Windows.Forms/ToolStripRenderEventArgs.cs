//
// ToolStripRenderEventArgs.cs
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
	public class ToolStripRenderEventArgs : EventArgs
	{
		private Rectangle affected_bounds;
		private Color back_color;
		private Rectangle connected_area;
		private Graphics graphics;
		private ToolStrip tool_strip;

		#region Public Constructors
		public ToolStripRenderEventArgs (Graphics g, ToolStrip toolStrip)
			: this (g, toolStrip, new Rectangle (0, 0, 100, 25), SystemColors.Control)
		{
		}
		
		public ToolStripRenderEventArgs (Graphics g, ToolStrip toolStrip, Rectangle affectedBounds, Color backColor)
		{
			this.graphics = g;
			this.tool_strip = toolStrip;
			this.affected_bounds = affectedBounds;
			this.back_color = backColor;
		}
		#endregion
		
		#region Public Properties
		public Rectangle AffectedBounds {
			get { return this.affected_bounds; }
		}

		public Color BackColor {
			get { return this.back_color; }
		}

		public Rectangle ConnectedArea {
			get { return this.connected_area; }
		}

		public Graphics Graphics {
			get { return this.graphics; }
		}

		public ToolStrip ToolStrip {
			get { return this.tool_strip; }
		}
		#endregion

		#region Internal Properties
		internal Rectangle InternalConnectedArea {
			set { this.connected_area = value; }
		}
		#endregion
	}
}
