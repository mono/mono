//
// DataRowComparer_1.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc. http://www.novell.com
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
using System.Collections.Generic;

namespace System.Data
{
	public sealed class DataRowComparer<TRow> : IEqualityComparer<TRow> where TRow : DataRow
	{
		static readonly DataRowComparer<TRow> default_instance = new DataRowComparer<TRow> ();

		public static DataRowComparer<TRow> Default {
			get { return default_instance; }
		}

		private DataRowComparer ()
		{
		}

		// LAMESPEC: neither of the parameters throws ArgumentNullException if it's null
		public bool Equals (TRow leftRow, TRow rightRow)
		{
			if (object.ReferenceEquals (leftRow, rightRow))
				return true;

			if (leftRow == null || rightRow == null)
				return false;

			int columnCount = leftRow.Table.Columns.Count;
			if (columnCount != rightRow.Table.Columns.Count)
				return false;

			for (int i = 0; i < columnCount; i++)
				if (!ColumnsEqual (leftRow [i], rightRow [i]))
					return false;
			return true;
		}

		bool ColumnsEqual (object leftCol, object rightCol)
		{
			if (object.ReferenceEquals (leftCol, rightCol))
				return true;

			if (leftCol == null || rightCol == null)
				return false;

			var vt = leftCol as System.ValueType;
			if (vt != null && vt.Equals (rightCol))
				return true;

			return leftCol.Equals (rightCol);
		}
		
		public int GetHashCode (TRow row)
		{
			if (row == null)
				throw new ArgumentNullException ("row");

			DataTable table = row.Table;
			if (table == null)
				throw new ArgumentException ("The source DataRow objects does not belong to a DataTable.");

			DataColumnCollection columns = table.Columns;
			int columnCount = columns.Count;
			if (columnCount == 0)
				return 0;

			int ret = 0;
			object o;
			for (int i = 0; i < columnCount; i++) {
				o = row [i];
				if (o == null)
					continue;

				ret ^= o.GetHashCode ();
			}
				
			return ret;
		}
	}
}
