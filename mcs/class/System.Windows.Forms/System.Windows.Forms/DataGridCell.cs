//
// System.Windows.Forms.DataGridCell.cs
//
// Author:
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
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

		public int ColumeNumber {
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
		
		public override bool Equals (object obj)
		{
			if (!(obj is DataGridCell))
				return false;

			return (this == (DataGridCell) obj);
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
