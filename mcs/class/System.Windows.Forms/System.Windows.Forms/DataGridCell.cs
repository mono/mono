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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
//

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Windows.Forms
{
	public struct DataGridCell
	{
		#region	Local Variables
		private int row;
		private int column;
		#endregion	// Local Variables

		#region Constructors
		public DataGridCell (int r,  int c)
		{
			row = r;
			column = c;
		}
		#endregion

		#region Public Instance Properties
		public int ColumnNumber {
			get { return column; }
			set { column = value; }
		}

		public int RowNumber {
			get { return row; }
			set { row = value; }
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public override bool Equals (object o)
		{
			if (!(o is DataGridCell))
				return false;

			DataGridCell obj = (DataGridCell) o;
			return (obj.ColumnNumber == column && obj.RowNumber == row);

		}

		public override int GetHashCode ()
		{
			return row ^ column;
		}

		public override string ToString ()
		{
			return "DataGridCell {RowNumber = " + row +", ColumnNumber = " + column + "}";
		}

		#endregion	// Public Instance Methods

	}
}
