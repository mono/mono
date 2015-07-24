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
// Copyright (c) 2006 Jonathan Chambers
//
// Authors:
//	Jonathan Chambers (joncham@gmail.com)
//


using System.Drawing;

namespace System.Windows.Forms
{
	public class DrawTreeNodeEventArgs : EventArgs
	{
		private Rectangle bounds;
		private bool draw_default;
		private Graphics graphics;
		private TreeNode node;
		private TreeNodeStates state;

		#region Public Constructors
		public DrawTreeNodeEventArgs (Graphics graphics, TreeNode node,
			Rectangle bounds, TreeNodeStates state)
		{
			this.bounds = bounds;
			this.draw_default = false;
			this.graphics = graphics;
			this.node = node;
			this.state = state;
		}
		#endregion // Public Constructors

		#region Public Instance Properties
		public Rectangle Bounds
		{
			get { return bounds; }
		}

		public bool DrawDefault
		{
			get { return draw_default; }
			set { draw_default = value; }
		}

		public Graphics Graphics
		{
			get { return graphics; }
		}

		public TreeNode Node
		{
			get { return node; }
		}

		public TreeNodeStates State
		{
			get { return state; }
		}
		#endregion // Public Instance Properties
	}
}
