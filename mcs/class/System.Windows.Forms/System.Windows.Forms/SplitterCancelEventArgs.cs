//
// SplitterCancelEventArgs.cs
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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System.ComponentModel;

namespace System.Windows.Forms
{
	public class SplitterCancelEventArgs : CancelEventArgs
	{
		private int mouse_cursor_x;
		private int mouse_cursor_y;
		private int split_x;
		private int split_y;

		#region Public Constructors
		public SplitterCancelEventArgs (int mouseCursorX, int mouseCursorY, int splitX, int splitY) : base()
		{
			this.mouse_cursor_x = mouseCursorX;
			this.mouse_cursor_y = mouseCursorY;
			this.split_x = splitX;
			this.split_y = splitY;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public int MouseCursorX {
			get { return this.mouse_cursor_x; }
		}

		public int MouseCursorY {
			get { return this.mouse_cursor_y; }
		}

		public int SplitX {
			get { return this.split_x; }
			set { this.split_x = value; }
		}

		public int SplitY {
			get { return this.split_y; }
			set { this.split_y = value; }
		}
		#endregion	// Public Instance Properties
	}
}
