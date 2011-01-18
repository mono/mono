//
// ToolStripArrowRenderEventArgs.cs
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
	public class ToolStripArrowRenderEventArgs : EventArgs
	{
		private Color arrow_color;
		private Rectangle arrow_rectangle;
		private ArrowDirection arrow_direction;
		private Graphics graphics;
		private ToolStripItem tool_strip_item;

		#region Public Constructors
		public ToolStripArrowRenderEventArgs (Graphics g, ToolStripItem toolStripItem, Rectangle arrowRectangle, Color arrowColor, ArrowDirection arrowDirection)
			: base ()
		{
			this.graphics = g;
			this.tool_strip_item = toolStripItem;
			this.arrow_rectangle = arrowRectangle;
			this.arrow_color = arrowColor;
			this.arrow_direction = arrowDirection;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public Color ArrowColor {
			get { return this.arrow_color; }
			set { this.arrow_color = value; }
		}

		public Rectangle ArrowRectangle {
			get { return this.arrow_rectangle; }
			set { this.arrow_rectangle = value; }
		}

		public ArrowDirection Direction {
			get { return this.arrow_direction; }
			set { this.arrow_direction = value; }
		}

		public Graphics Graphics {
			get { return this.graphics; }
		}

		public ToolStripItem Item {
			get { return this.tool_strip_item; }
		}
		#endregion	// Public Instance Properties
	}
}
