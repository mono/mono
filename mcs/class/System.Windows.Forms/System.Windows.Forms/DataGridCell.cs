//
// System.Windows.Forms.DataGridCell.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

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

using System;

namespace System.Windows.Forms {
	
	public struct DataGridCell { 

		private int rownumber;
		private int columnnumber;

		// -----------------------
		// Public Constructor
		// -----------------------

		/// <summary>
		/// 
		/// </summary>
		///
		/// <remarks>
		///
		/// </remarks>
		
		public DataGridCell (int r, int c)
		{
			rownumber = r;
			columnnumber = c;
		}

		// -----------------------
		// Public Shared Members
		// -----------------------

		/// <remarks>
		///	Compares two DataGridCell objects. The return value is
		///	based on the equivalence of the RowNumber and ColumnNumber properties of the two objects.
		/// </remarks>

		public static bool operator == (DataGridCell dgc_a, 
			DataGridCell dgc_b) {

			return ((dgc_a.rownumber == dgc_b.rownumber) &&
				(dgc_a.columnnumber == dgc_b.columnnumber));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two DataGridCell objects. The return value is
		///	based on the equivalence of the RowNumber and ColumnNumber properties of the two objects.
		/// </remarks>
		public static bool operator != (DataGridCell dgc_a, 
			DataGridCell dgc_b) {
			return ((dgc_a.rownumber != dgc_b.rownumber) ||
				(dgc_a.columnnumber != dgc_b.columnnumber));
		}
		
		// -----------------------
		// Public Instance Members
		// -----------------------


		public int RowNumber {
			get{
				return rownumber;
			}
			set{
				rownumber = value;
			}
		}

		public int ColumnNumber {
			get{
				return columnnumber;
			}
			set{
				columnnumber = value;
			}
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this DataGridCell and another object.
		/// </remarks>
		
		public override bool Equals (object o)
		{
			if (!(o is DataGridCell))
				return false;

			return (this == (DataGridCell) o);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode ()
		{
			return (int)( rownumber ^ columnnumber);
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the DataGridCell as a string.
		/// </remarks>
		
		public override string ToString ()
		{
			return String.Format ("[{0},{1}]", rownumber, columnnumber );
		}
	}
}
