//
// TableLayoutCellPaintEventArgs.cs
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

using System.Drawing;

namespace System.Windows.Forms
{
	public class TableLayoutCellPaintEventArgs : PaintEventArgs
	{
		private Rectangle cell_bounds;
		private int column;
		private int row;

		#region Public Constructors
		public TableLayoutCellPaintEventArgs (Graphics g, Rectangle clipRectangle,
			Rectangle cellBounds, int column, int row)
			: base (g, clipRectangle)
		{
			this.cell_bounds = cellBounds;
			this.column = column;
			this.row = row;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public Rectangle CellBounds {
			get { return this.cell_bounds; }
		}

		public int Column {
			get { return this.column; }
		}

		public int Row {
			get { return this.row; }
		}
		#endregion	// Public Instance Properties
	}
}
