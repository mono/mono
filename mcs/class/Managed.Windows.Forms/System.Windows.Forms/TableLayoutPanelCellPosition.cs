//
// TableLayoutPanelCellPosition.cs
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
//	Miguel de Icaza (miguel@novell.com)
//


using System.ComponentModel;

namespace System.Windows.Forms
{
	[TypeConverter (typeof (TableLayoutPanelCellPositionTypeConverter))]
	public struct TableLayoutPanelCellPosition {
		int column, row;

		public TableLayoutPanelCellPosition (int column, int row)
		{
			this.column = column;
			this.row = row;
		}
		
		public int Column {
			get {
				return column;
			}

			set {
				column = value;
			}
		}

		public int Row {
			get {
				return row;
			}

			set {
				row = value;
			}
		}

		public override string ToString ()
		{
			return String.Concat (column.ToString (), ",", row.ToString ());
		}

		public override int GetHashCode ()
		{
			return column.GetHashCode () ^ row.GetHashCode ();
		}

		public static bool operator == (TableLayoutPanelCellPosition p1, TableLayoutPanelCellPosition p2)
		{
			return p1.column == p2.column && p1.row == p2.row;
		}

		public static bool operator != (TableLayoutPanelCellPosition p1, TableLayoutPanelCellPosition p2)
		{
			return !(p1.column == p2.column && p1.row == p2.row);
		}

		public override bool Equals (object other)
		{
			if (other == null)
				return false;
			if (!(other is TableLayoutPanelCellPosition))
				return false;
			TableLayoutPanelCellPosition o = (TableLayoutPanelCellPosition) other;
			return o.column == column && o.row == row;
		}
	}
	
	internal class TableLayoutPanelCellPositionTypeConverter : TypeConverter
	{
	}
}
