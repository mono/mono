//
// TestDataColumn.cs
//
// Tests for System.Data.DataColumn
//
// Authors:
//   Rodrigo Moya <rodrigo@ximian.com>
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;

namespace TestSystemData
{
	class TestDataColumn
	{
		static void Main (string[] args) {
			DataColumn colName = new DataColumn ("name");
			DataColumn colAge = new DataColumn ("age");

			// set properties
			colName.DataType = Type.GetType ("System.String");
			colName.RadOnly = true;
			colName.Caption = "Full name";

			colAge.DataType = Type.GetType ("System.Int");
			colAge.ReadOnly = false;
			colAge.Caption = "Age";

			// display properties
			_displayColumn (colAge);
			_displayColumn (colName);
		}

		static void _displayColumn (DataColumn col) {
			string msg = "Column name: " + col.ColumnName + "\n" +
				"Column Type: " + col.DataType + "\n" +
				"Read Only?: " + col.ReadOnly + "\n" +
				"Caption: " + col.Caption + "\n" +
				"Allow Nulls?: " + col.AllowDBNull;
			System.Console.WriteLine (msg);
		}
	}
}
